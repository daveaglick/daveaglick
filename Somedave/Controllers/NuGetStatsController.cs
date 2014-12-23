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
                    Heading1 = "Package",
                    Heading2 = "Downloads",
                    IsPackage = true,
                    Entries = c => 
                        c.Packages
                        .GroupBy(x => x.Id)
                        .Select(x => new
                        {
                            Name = x.Key,
                            Value = x.Sum(y => y.VersionDownloadCount)
                        })
                        .OrderByDescending(x => x.Value)
                        .Take(10)
                        .Retry()
                        .ToList()
                        .Select(x => new Leaderboard.Entry
                        {
                            Name = x.Name,
                            Value = x.Value.ToString()
                        })
                }
            },
            {
                "most-downloaded-authors", 
                new Leaderboard.Meta
                {
                    Title = "Most Downloaded Authors", 
                    Description = "This list contains the authors with the most total downloads across all versions.",
                    Heading1 = "Author",
                    Heading2 = "Downloads",
                    IsPackage = false,
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
                        .Take(10)
                        .Retry()
                        .ToList()
                        .Select(x => new Leaderboard.Entry
                        {
                            Name = x.Name,
                            Value = x.Value.ToString()
                        })
                }
            },
            {
                "most-dependencies", 
                new Leaderboard.Meta
                {
                    Title = "Most Dependencies", 
                    Description = "This list contains the packages with the most direct dependencies on them irrespective of versions.",
                    Heading1 = "Package",
                    Heading2 = "Dependencies",
                    IsPackage = true,
                    Entries = c => 
                        c.Dependencies
                        .GroupBy(x => x.DependencyId)
                        .Select(x => new
                        {
                            Name = x.Key,
                            Value = x.Select(y => y.Id).Distinct().Count()
                        })
                        .OrderByDescending(x => x.Value)
                        .Take(10)
                        .Retry()
                        .ToList()
                        .Select(x => new Leaderboard.Entry
                        {
                            Name = x.Name,
                            Value = x.Value.ToString()
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

        [GET("dependencies/{package}")]
        public virtual ActionResult Dependencies(string package)
        {
            Dependencies dependencies = new Dependencies {Package = package};
            using (NuGetStatsDataContext context = new NuGetStatsDataContext())
            {
                SetContextState(context);
                dependencies.Dependency = 
                    context.Dependencies
                    .Where(x => x.Id == package)
                    .Select(x => x.DependencyId)
                    .Distinct()
                    .ToList();
                dependencies.Dependent = 
                    context.Dependencies
                    .Where(x => x.DependencyId == package)
                    .Select(x => x.Id)
                    .Distinct()
                    .ToList();
            }
            return View(dependencies);
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