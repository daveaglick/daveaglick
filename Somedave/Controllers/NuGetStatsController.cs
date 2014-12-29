using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting;
using AttributeRouting.Web.Mvc;
using LinqToSqlRetry;
using Somedave.Models.NuGetStats;
using Index = Somedave.Models.Home.Index;

namespace Somedave.Controllers
{
    [RoutePrefix("nuget-stats")]
    public partial class NuGetStatsController : Controller
    {
        [GET("")]
        public virtual ActionResult Index()
        {
            return View(new Models.NuGetStats.Index
            {
                Leaderboards = Leaderboards
            });
        }

        [GET("status")]
        public virtual ActionResult Status()
        {
            using (NuGetStatsDataContext context = new NuGetStatsDataContext())
            {
                return View(new Status
                {
                    Histories = context.Histories.OrderByDescending(x => x.StartTime).Take(10).Retry().ToList()
                });
            }
        }

        [ChildActionOnly]
        public virtual ActionResult Footer()
        {
            Footer footer = new Footer();
            using (NuGetStatsDataContext context = new NuGetStatsDataContext())
            {
                SetContextState(context);
                History history = context.Histories.OrderByDescending(x => x.LastUpdated).Retry().FirstOrDefault();
                footer.LastUpdated = history == null ? DateTime.MinValue : history.LastUpdated;
                footer.Packages = context.Packages.Retry().Count();
            }
            return View(footer);
        }

        public static readonly Dictionary<string, Leaderboard.Meta> Leaderboards = new Dictionary<string, Leaderboard.Meta>()
        {
            { 
                "most-downloaded-packages", 
                new Leaderboard.Meta
                {
                    Title = "Most Downloaded Packages", 
                    Description = "This list contains the packages with the most total downloads across all versions.",
                    NameHeading = "Package",
                    NameLink = (x, u) => u.Action(MVC.NuGetStats.Package(x)),
                    ValueHeading = "Downloads",
                    Entries = c => 
                        c.Packages
                        .GroupBy(x => x.Id)
                        .Select(x => new
                        {
                            Name = x.Key,
                            Value = x.Sum(y => y.VersionDownloadCount)
                        })
                        .OrderByDescending(x => x.Value)
                        .Take(20)
                        .Retry()
                        .ToList()
                        .Select(x => new Leaderboard.Entry
                        {
                            Name = x.Name,
                            Value = x.Value.ToString("N0")
                        })
                }
            },
            {
                "most-downloaded-authors", 
                new Leaderboard.Meta
                {
                    Title = "Most Downloaded Authors", 
                    Description = "This list contains the authors with the most total downloads across all versions.",
                    NameHeading = "Author",
                    ValueHeading = "Downloads",
                    Entries = c => 
                        c.Authors
                        .GroupBy(x => x.Name)
                        .Select(x => new
                        {
                            Name = x.Key,
                            Value = x.Sum(y => c.Packages
                                .Where(z => z.Id == y.Id && z.Version == y.Version)
                                .Select(z => z.VersionDownloadCount)
                                .FirstOrDefault())
                        })
                        .OrderByDescending(x => x.Value)
                        .Take(20)
                        .Retry()
                        .ToList()
                        .Select(x => new Leaderboard.Entry
                        {
                            Name = x.Name,
                            Value = x.Value.ToString("N0")
                        })
                }
            },
            {
                "most-dependencies", 
                new Leaderboard.Meta
                {
                    Title = "Most Dependencies", 
                    Description = "This list contains the packages with the most direct dependencies on them irrespective of versions.",
                    NameHeading = "Package",
                    NameLink = (x, u) => u.Action(MVC.NuGetStats.Package(x)),
                    ValueHeading = "Dependencies",
                    Entries = c => 
                        c.Dependencies
                        .GroupBy(x => x.DependencyId)
                        .Select(x => new
                        {
                            Name = x.Key,
                            Value = x.Select(y => y.Id).Distinct().Count()
                        })
                        .OrderByDescending(x => x.Value)
                        .Take(20)
                        .Retry()
                        .ToList()
                        .Select(x => new Leaderboard.Entry
                        {
                            Name = x.Name,
                            Value = x.Value.ToString("N0")
                        })
                }
            }
        };

        [GET("leaderboard/{leaderboard}")]
        public virtual ActionResult Leaderboard(string leaderboard)
        {
            Leaderboard.Meta meta;
            if (!Leaderboards.TryGetValue(leaderboard, out meta))
            {
                return HttpNotFound();
            }
            IEnumerable<Leaderboard.Entry> entries;
            using (NuGetStatsDataContext context = new NuGetStatsDataContext())
            {
                SetContextState(context);
                entries = meta.Entries(context);
            }
            return View(MVC.NuGetStats.Views.Leaderboard, new Leaderboard
            {
                Entries = entries,
                Metadata = meta
            });
        }

        [GET("package/{id}")]
        public virtual ActionResult Package(string id)
        {
            PackageViewModel model = new PackageViewModel { Id = id };
            using (NuGetStatsDataContext context = new NuGetStatsDataContext())
            {
                SetContextState(context);

                // Get versions
                model.Versions = context.Packages
                    .Where(x => x.Id == id)
                    .OrderByDescending(x => x.Created)
                    .Select(x => new PackageViewModel.Version
                    {
                        Name = x.Version,
                        DownloadCount = x.VersionDownloadCount,
                        Created = x.Created
                    })
                    .ToList();

                // Get authors
                model.Authors = context.Authors
                    .Where(x => x.Id == id)
                    .Select(x => x.Name)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                // Get tags
                model.Tags = context.Tags
                    .Where(x => x.Id == id)
                    .Select(x => x.Name)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                // Get all dependent packages
                Dictionary<string, Dictionary<string, int>> dependentDictionary = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase) { { id, new Dictionary<string, int>() } };
                List<string> queryList = new List<string> { id };
                int depth = 1;
                do
                {
                    // Server can only handle a maximum of 2000 parameters...
                    List<string> cloneList = queryList.ToArray().ToList();
                    queryList = new List<string>();
                    while (cloneList.Count > 0)
                    {
                        List<string> containsList = cloneList.Take(2000).ToList();
                        cloneList = cloneList.Skip(2000).ToList();
                        foreach (var result in context.Dependencies
                            .Where(x => containsList.Contains(x.DependencyId))
                            .Select(x => new { x.Id, x.DependencyId })
                            .Retry())
                        {
                            Dictionary<string, int> valueDictionary;
                            if (!dependentDictionary.TryGetValue(result.Id, out valueDictionary))
                            {
                                valueDictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                                dependentDictionary[result.Id] = valueDictionary;
                                queryList.Add(result.Id);
                            }
                            if (!valueDictionary.ContainsKey(result.DependencyId))
                            {
                                valueDictionary[result.DependencyId] = depth;
                            }
                        }
                    }
                    depth++;

                } while (queryList.Count > 0);
                dependentDictionary.Remove(id);
                model.Dependent = dependentDictionary;

                // Get all dependencies
                Dictionary<string, Dictionary<string, int>> dependencyDictionary = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase) { { id, new Dictionary<string, int>() } };
                queryList = new List<string> { id };
                depth = 1;
                do
                {
                    // Server can only handle a maximum of 2000 parameters...
                    List<string> cloneList = queryList.ToArray().ToList();
                    queryList = new List<string>();
                    while (cloneList.Count > 0)
                    {
                        List<string> containsList = cloneList.Take(2000).ToList();
                        cloneList = cloneList.Skip(2000).ToList();
                        foreach (var result in context.Dependencies
                            .Where(x => containsList.Contains(x.Id))
                            .Select(x => new {x.Id, x.DependencyId})
                            .Retry())
                        {
                            Dictionary<string, int> valueDictionary;
                            if (!dependencyDictionary.TryGetValue(result.DependencyId, out valueDictionary))
                            {
                                valueDictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                                dependencyDictionary[result.DependencyId] = valueDictionary;
                                queryList.Add(result.DependencyId);
                            }
                            if (!valueDictionary.ContainsKey(result.Id))
                            {
                                valueDictionary[result.Id] = depth;
                            }
                        }
                    }
                    depth++;
                } while (queryList.Count > 0);
                dependencyDictionary.Remove(id);
                model.Dependency = dependencyDictionary;
            }
            return View(model);
        }

        [POST("package")]
        public virtual ActionResult PackagePost(string id)
        {
            return RedirectToActionPermanent(MVC.NuGetStats.Package(id));
        }

        // This sets a new timeout and changes the transaction level
        // Use it any time the main tables are accessed
        // See http://omaralzabir.com/linq_to_sql_solve_transaction_deadlock_and_query_timeout_problem_using_uncommitted_reads/
        private void SetContextState(NuGetStatsDataContext context)
        {
            context.ExecuteCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");
        }
    }
}