using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using NuGet;

namespace NuGetUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            using (NuGetDbDataContext context = new NuGetDbDataContext())
            {
                // Record update start
                History history = new History {StartTime = DateTime.UtcNow};
                context.Histories.InsertOnSubmit(history);
                context.SubmitChanges();

                try
                {
                    // Clear all tables
                    context.ExecuteCommand("TRUNCATE TABLE dbo.Dependencies");
                    context.ExecuteCommand("TRUNCATE TABLE dbo.Tags");
                    context.ExecuteCommand("TRUNCATE TABLE dbo.Authors");
                    context.ExecuteCommand("DELETE FROM dbo.Packages");  // Have to DELETE for Packages because of foreign key constraint
                    history.Messages = "0";
                    context.SubmitChanges();

                    // Initialize NuGet
                    IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
                    int count = 0;
                    List<IPackage> packages;
                    HashSet<string> packageHash = new HashSet<string>();
                    do
                    {
                        // Get the next set of packages
                        packages = repo.GetPackages().Skip(count).Take(10000).ToList();

                        // Process packages
                        foreach (IPackage package in packages)
                        {
                            // Check for duplicates
                            string id = Normalize(package.Id, 128);
                            string version = Normalize(package.Version.ToString(), 50);
                            if (!packageHash.Add(id + ":" + version))
                            {
                                continue;
                            }

                            // Package
                            context.Packages.InsertOnSubmit(new Package()
                            {
                                Id = id,
                                Version = version,
                                Description = package.Description,
                                DownloadCount = package.DownloadCount,
                                Listed = package.Listed
                            });

                            // Authors
                            if (package.Authors != null)
                            {
                                foreach (string author in package.Authors
                                    .Where(x => !string.IsNullOrWhiteSpace(x) && x.IndexOf('�') == -1))
                                {
                                    context.Authors.InsertOnSubmit(new Author()
                                    {
                                        Id = id,
                                        Version = version,
                                        Author1 = Normalize(author, 128)
                                    });
                                }
                            }

                            // Tags
                            if (!string.IsNullOrWhiteSpace(package.Tags))
                            {
                                foreach (string tag in package.Tags
                                    .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                                    .Where(x => x.IndexOf('�') == -1)
                                    .Distinct(StringComparer.OrdinalIgnoreCase))
                                {
                                    context.Tags.InsertOnSubmit(new Tag()
                                    {
                                        Id = id,
                                        Version = version,
                                        Tag1 = Normalize(tag, 128)
                                    });
                                }
                            }

                            // Dependencies
                            HashSet<string> dependencyHash = new HashSet<string>();
                            if (package.DependencySets != null)
                            {
                                foreach (PackageDependency dependency in package.DependencySets
                                    .Where(x => x.Dependencies != null)
                                    .SelectMany(x => x.Dependencies)
                                    .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Id) && x.VersionSpec != null))
                                {
                                    // Need to check for duplicates since the same dependency might be in more than one set
                                    string dependencyId = Normalize(dependency.Id, 128);
                                    string dependencyVersion = Normalize(dependency.VersionSpec.ToString(), 50);
                                    if (
                                        dependencyHash.Add(string.Format("{0}|{1}|{2}|{3}", id, version,
                                            dependencyId, dependencyVersion)))
                                    {
                                        context.Dependencies.InsertOnSubmit(new Dependency()
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

                        // Submit and cleanup
                        count += packages.Count;
                        history.Messages = packages.Count.ToString();
                        context.SubmitChanges();
                        Console.WriteLine(count);

                    } while (packages.Count == 10000);

                    history.Messages = string.Empty;
                    Console.WriteLine("Done");
                }
                catch (Exception ex)
                {
                    // Log exception
                    history.Messages = ex.ToString();
                }

                // Record end time
                history.EndTime = DateTime.UtcNow;
                context.SubmitChanges();
            }
        }

        private static string Normalize(string value, int maxLength)
        {
	        if (string.IsNullOrEmpty(value)) return value;
	        value = value.Trim();
            return value.Length <= maxLength ? value : value.Substring(0, maxLength); 
        }
    }
}
