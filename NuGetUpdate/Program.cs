using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using LinqToSqlRetry;
using NuGet;
using NuGetUpdate.NuGetFeed;

namespace NuGetUpdate
{
    class Program
    {
        private static readonly Uri NuGetFeedUri = new Uri("http://nuget.org/api/v2", UriKind.Absolute);
        private static readonly DateTime UnlistedPublishedDate = new DateTime(1900, 1, 1);
        private static readonly List<string> SkipAuthors = new List<string> { "Inc.", "Inc" }; 
        private const int BatchSize = 100;

        static void Main(string[] args)
        {
            V2FeedContext feed = new V2FeedContext(NuGetFeedUri);
            DateTime startTime = DateTime.UtcNow;
            DateTime lastUpdated = SqlDateTime.MinValue.Value;
            
            try
            {
                // Record start data
                using (NuGetStatsDataContext context = new NuGetStatsDataContext())
                {
                    // Get the last run time
                    History history = context.Histories.OrderByDescending(x => x.StartTime).Retry().FirstOrDefault();
                    if (history != null && history.EndTime == null)
                    {
                        Console.WriteLine("Already running or stopped prematurely, exiting.");
                        return;
                    }

                    // Get the last end time
                    history = context.Histories.OrderByDescending(x => x.LastUpdated).Retry().FirstOrDefault();
                    if (history != null)
                    {
                        lastUpdated = history.LastUpdated;
                    }
                    
                    // Record update start (with total count)
                    context.Histories.InsertOnSubmit(new History
                    {
                        StartTime = startTime, 
                        LastUpdated = lastUpdated, 
                        TotalCount = feed.Packages.Where(x => x.LastUpdated >= lastUpdated).Count(), 
                        ProcessedCount = 0
                    });
                    context.SubmitChangesRetry();
                }

                // Iterate the packages
                int count = 0;
                int processedCount = 0;
                do
                {
                    // Process
                    count = 0;
                    DateTime newLastUpdated = lastUpdated;
                    foreach (var package in feed.Packages
                        .Where(x => x.LastUpdated >= lastUpdated)
                        .OrderBy(x => x.LastUpdated)
                        .Skip(processedCount)
                        .Take(BatchSize)
                        .Select(x => new
                        {
                            x.Id,
                            x.Version,
                            x.Published,
                            x.VersionDownloadCount,
                            x.IsLatestVersion,
                            x.IsAbsoluteLatestVersion,
                            x.IsPrerelease,
                            x.Created,
                            x.LastUpdated,
                            x.Authors,
                            x.Dependencies,
                            x.Tags
                        }))
                    {
                        count++;
                        string id = Normalize(package.Id, 128);
                        string version = Normalize(package.Version, 50);

                        using (NuGetStatsDataContext context = new NuGetStatsDataContext())
                        {
                            // Check for existing
                            Package existing = context.Packages.Retry().FirstOrDefault(x => x.Id == id && x.Version == version);
                            if (existing != null)
                            {
                                // Is it unlisted?
                                if (package.Published == UnlistedPublishedDate)
                                {
                                    context.Packages.DeleteOnSubmit(existing);
                                }
                                else
                                {
                                    existing.VersionDownloadCount = package.VersionDownloadCount;
                                    existing.IsLatestVersion = package.IsLatestVersion;
                                    existing.IsAbsoluteLatestVersion = package.IsAbsoluteLatestVersion;
                                    existing.IsPrerelease = package.IsPrerelease;
                                }

                                // Delete details in both cases (use manual SQL commands to avoid having to get and then delete the records)
                                new LinearUpdateStatsRetry(context).Retry(() =>
                                {
                                    context.ExecuteCommand("DELETE FROM [Authors] WHERE [Id] = {0} AND [Version] = {1}", id, version);
                                    context.ExecuteCommand("DELETE FROM [Tags] WHERE [Id] = {0} AND [Version] = {1}", id, version);
                                    context.ExecuteCommand("DELETE FROM [Dependencies] WHERE [Id] = {0} AND [Version] = {1}", id, version);
                                });
                            }

                            // Don't add data for unlisted packages
                            if (package.Published != UnlistedPublishedDate)
                            {
                                // Add package
                                if (existing == null)
                                {
                                    context.Packages.InsertOnSubmit(new Package
                                    {
                                        Id = id,
                                        Version = version,
                                        Created = package.Created,
                                        VersionDownloadCount = package.VersionDownloadCount,
                                        IsLatestVersion = package.IsLatestVersion,
                                        IsAbsoluteLatestVersion = package.IsAbsoluteLatestVersion,
                                        IsPrerelease = package.IsPrerelease
                                    });
                                }

                                // Authors
                                if (!string.IsNullOrWhiteSpace(package.Authors))
                                {
                                    HashSet<string> authorHash = new HashSet<string>();
                                    foreach (string author in package.Authors
                                        .Split(new []{','}, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => x.Trim())
                                        .Where(x => x.IndexOf('�') == -1 && !SkipAuthors.Contains(x, StringComparer.OrdinalIgnoreCase))
                                        .Distinct(StringComparer.OrdinalIgnoreCase))
                                    {
                                        string normalizedAuthor = Normalize(author, 128);
                                        if (authorHash.Add(normalizedAuthor))
                                        {
                                            context.Authors.InsertOnSubmit(new Author
                                            {
                                                Id = id,
                                                Version = version,
                                                Name = normalizedAuthor
                                            });
                                        }
                                    }
                                }

                                // Tags       
                                if (!string.IsNullOrWhiteSpace(package.Tags))
                                {
                                    HashSet<string> tagHash = new HashSet<string>();
                                    foreach (string tag in package.Tags
                                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => x.Trim())
                                        .Where(x => x.IndexOf('�') == -1)
                                        .Distinct(StringComparer.OrdinalIgnoreCase))
                                    {
                                        string normalizedTag = Normalize(tag, 128);
                                        if (tagHash.Add(normalizedTag))
                                        {
                                            context.Tags.InsertOnSubmit(new Tag
                                            {
                                                Id = id,
                                                Version = version,
                                                Name = normalizedTag
                                            });
                                        }
                                    }
                                }

                                // Dependencies
                                if (!string.IsNullOrWhiteSpace(package.Dependencies))
                                {
                                    HashSet<string> dependencyHash = new HashSet<string>();
                                    List<PackageDependencySet> dependencySets = ParseDependencySet(package.Dependencies);
                                    if (dependencySets != null)
                                    {
                                        foreach (PackageDependency dependency in dependencySets
                                            .Where(x => x.Dependencies != null)
                                            .SelectMany(x => x.Dependencies)
                                            .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Id) && x.VersionSpec != null))
                                        {
                                            // Need to check for duplicates since the same dependency might be in more than one set
                                            string dependencyId = Normalize(dependency.Id, 128);
                                            string dependencyVersion = Normalize(dependency.VersionSpec.ToString(), 50);
                                            if (dependencyHash.Add(string.Format("{0}|{1}|{2}|{3}", id, version, dependencyId, dependencyVersion)))
                                            {
                                                context.Dependencies.InsertOnSubmit(new Dependency
                                                {
                                                    Id = id,
                                                    Version = version,
                                                    DependencyId = dependencyId,
                                                    DependencyVersion = dependencyVersion
                                                });
                                            }
                                        }
                                    }
                                }
                            }

                            context.SubmitChangesRetry();
                        }

                        newLastUpdated = package.LastUpdated;
                    }

                    // Cleanup
                    processedCount += count;
                    using (NuGetStatsDataContext context = new NuGetStatsDataContext())
                    {
                        History history = context.Histories.Retry().Single(x => x.StartTime == startTime);
                        history.ProcessedCount = processedCount;
                        history.LastUpdated = newLastUpdated;
                        context.SubmitChangesRetry();
                    }
                    Console.WriteLine(processedCount + " " + newLastUpdated);

                } while (count == BatchSize);
            }
            catch (Exception ex)
            {
                // Log the exception
                using (NuGetStatsDataContext context = new NuGetStatsDataContext())
                {
                    History history = context.Histories.Retry().Single(x => x.StartTime == startTime);
                    history.Exception = ex.ToString();
                    history.EndTime = DateTime.UtcNow;
                    context.SubmitChangesRetry();
                }
                Console.WriteLine("Exception:" + ex);
                throw;
            }

            // Done!
            using (NuGetStatsDataContext context = new NuGetStatsDataContext())
            {
                History history = context.Histories.Retry().Single(x => x.StartTime == startTime);
                history.EndTime = DateTime.UtcNow;
                context.SubmitChangesRetry();
            }
            Console.WriteLine("Done!");
        }

        private static string Normalize(string value, int maxLength)
        {
	        if (string.IsNullOrEmpty(value)) return value;
	        value = value.Trim();
            return value.Length <= maxLength ? value : value.Substring(0, maxLength); 
        }

        // From NuGet.Core src/Core/Packages/DataServicePackage.cs
        private static List<PackageDependencySet> ParseDependencySet(string value)
        {
            var dependencySets = new List<PackageDependencySet>();

            var dependencies = value.Split('|').Select(ParseDependency).ToList();

            // group the dependencies by target framework
            var groups = dependencies.GroupBy(d => d.Item3);

            dependencySets.AddRange(
                groups.Select(g => new PackageDependencySet(
                                           g.Key,   // target framework 
                                           g.Where(pair => !String.IsNullOrEmpty(pair.Item1))       // the Id is empty when a group is empty.
                                            .Select(pair => new PackageDependency(pair.Item1, pair.Item2)))));     // dependencies by that target framework
            return dependencySets;
        }

        private static Tuple<string, IVersionSpec, FrameworkName> ParseDependency(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            // IMPORTANT: Do not pass StringSplitOptions.RemoveEmptyEntries to this method, because it will break 
            // if the version spec is null, for in that case, the Dependencies string sent down is "<id>::<target framework>".
            // We do want to preserve the second empty element after the split.
            string[] tokens = value.Trim().Split(new[] { ':' });

            if (tokens.Length == 0)
            {
                return null;
            }

            // Trim the id
            string id = tokens[0].Trim();

            IVersionSpec versionSpec = null;
            if (tokens.Length > 1)
            {
                // Attempt to parse the version
                VersionUtility.TryParseVersionSpec(tokens[1], out versionSpec);
            }

            var targetFramework = (tokens.Length > 2 && !String.IsNullOrEmpty(tokens[2]))
                                    ? VersionUtility.ParseFrameworkName(tokens[2])
                                    : null;

            return Tuple.Create(id, versionSpec, targetFramework);
        }
    }
}
