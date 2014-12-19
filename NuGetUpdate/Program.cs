using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using NuGetUpdate.NuGetFeed;

namespace NuGetUpdate
{
    class Program
    {
        private static readonly Uri NuGetFeedUri = new Uri("http://nuget.org/api/v2", UriKind.Absolute);
        private static readonly DateTime UnlistedPublishedDate = new DateTime(1900, 1, 1);
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
                    // Get the last end time
                    History history = context.Histories.OrderByDescending(x => x.LastUpdated).FirstOrDefault();
                    if (history != null)
                    {
                        lastUpdated = history.LastUpdated;
                    }
                    
                    // Record update start (with total count)
                    context.Histories.InsertOnSubmit(new History
                    {
                        StartTime = startTime, 
                        LastUpdated = lastUpdated, 
                        TotalCount = feed.Packages.Where(x => x.LastUpdated >= lastUpdated && x.LastUpdated < startTime).Count(), 
                        ProcessedCount = 0
                    });
                    context.SubmitChanges();
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
                        .Where(x => x.LastUpdated >= lastUpdated && x.LastUpdated < startTime)
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
                            Package existing = context.Packages.FirstOrDefault(x => x.Id == id && x.Version == version);
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

                                // Delete details in either case (use manual SQL commands to avoid having to get and then delete the records)
                                context.ExecuteCommand("DELETE FROM dbo.Authors WHERE Id = {0} AND Version = {1}", id, version);
                                context.ExecuteCommand("DELETE FROM dbo.Tags WHERE Id = {0} AND Version = {1}", id, version);
                                context.ExecuteCommand("DELETE FROM dbo.Dependencies WHERE Id = {0} AND Version = {1}", id, version);
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
                                    foreach (string author in package.Authors
                                        .Split(new []{','}, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => x.Trim())
                                        .Where(x => x.IndexOf('�') == -1)
                                        .Distinct(StringComparer.OrdinalIgnoreCase))
                                    {
                                        context.Authors.InsertOnSubmit(new Author
                                        {
                                            Id = id,
                                            Version = version,
                                            Name = Normalize(author, 128)
                                        });
                                    }
                                }

                                // Tags                            
                                if (!string.IsNullOrWhiteSpace(package.Tags))
                                {
                                    foreach (string tag in package.Tags
                                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => x.Trim())
                                        .Where(x => x.IndexOf('�') == -1)
                                        .Distinct(StringComparer.OrdinalIgnoreCase))
                                    {
                                        context.Tags.InsertOnSubmit(new Tag
                                        {
                                            Id = id,
                                            Version = version,
                                            Name = Normalize(tag, 128)
                                        });
                                    }
                                }

                                // Dependencies
                                HashSet<string> dependencyHash = new HashSet<string>();
                                if (!string.IsNullOrWhiteSpace(package.Dependencies))
                                {
                                    foreach (KeyValuePair<string, string> dependency in package.Dependencies
                                        .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => x.Trim().Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries))
                                        .Where(x => x.Length == 2 && !string.IsNullOrWhiteSpace(x[0]) && !string.IsNullOrWhiteSpace(x[1]))
                                        .Select(x => new KeyValuePair<string, string>(x[0].Trim(), x[1].Trim())))
                                    {
                                        // Need to check for duplicates since the same dependency might be in more than one set
                                        string dependencyId = Normalize(dependency.Key, 128);
                                        string dependencyVersion = Normalize(dependency.Value, 50);
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

                            context.SubmitChanges();
                        }

                        newLastUpdated = package.LastUpdated;
                    }

                    // Cleanup
                    processedCount += count;
                    using (NuGetStatsDataContext context = new NuGetStatsDataContext())
                    {
                        History history = context.Histories.Single(x => x.StartTime == startTime);
                        history.ProcessedCount = processedCount;
                        history.LastUpdated = newLastUpdated;
                        context.SubmitChanges();
                    }
                    Console.WriteLine(processedCount + " " + newLastUpdated);

                } while (count == BatchSize);
            }
            catch (Exception ex)
            {
                // Log the exception
                using (NuGetStatsDataContext context = new NuGetStatsDataContext())
                {
                    History history = context.Histories.Single(x => x.StartTime == startTime);
                    history.Exception = ex.ToString();
                    history.EndTime = DateTime.UtcNow;
                    context.SubmitChanges();
                }
                Console.WriteLine("Exception:" + ex);
                throw;
            }

            // Done!
            using (NuGetStatsDataContext context = new NuGetStatsDataContext())
            {
                History history = context.Histories.Single(x => x.StartTime == startTime);
                history.EndTime = DateTime.UtcNow;
                context.SubmitChanges();
            }
            Console.WriteLine("Done!");
        }

        private static string Normalize(string value, int maxLength)
        {
	        if (string.IsNullOrEmpty(value)) return value;
	        value = value.Trim();
            return value.Length <= maxLength ? value : value.Substring(0, maxLength); 
        }
    }
}
