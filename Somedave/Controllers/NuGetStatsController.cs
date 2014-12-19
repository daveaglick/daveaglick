using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting;
using AttributeRouting.Web.Mvc;
using Somedave.Models.NuGetStats;

namespace Somedave.Controllers
{
    [RoutePrefix("nuget-stats")]
    public partial class NuGetStatsController : Controller
    {
        [GET("")]
        public virtual ActionResult Index()
        {
            return View();
        }

        [GET("leaderboards")]
        public virtual ActionResult Leaderboards()
        {
            using (NuGetStatsDataContext context = new NuGetStatsDataContext())
            {
                return View(new Leaderboards
                {
                    MostDownloadedPackages = context.Packages
                        .GroupBy(x => x.Id)
                        .Select(x => new
                        {
                            Name = x.Key, 
                            Value = x.Sum(y => y.VersionDownloadCount)
                        })
                        .OrderByDescending(x => x.Value)
                        .Take(10)
                        .ToList()
                        .Select(x => new Leaderboards.Entry
                        {
                            Name = x.Name,
                            Value = x.Value.ToString()
                        }),
                    MostDownloadedAuthors = context.Authors
                        .GroupBy(x => x.Name)
                        .Select(x => new
                        {
                            Name = x.Key, 
                            Value = x.Sum(y => context.Packages
                                .Where(z => z.Id == y.Id && z.Version == y.Version)
                                .Select(z => z.VersionDownloadCount)
                                .FirstOrDefault())
                        })
                        .OrderByDescending(x => x.Value)
                        .Take(10)
                        .ToList()
                        .Select(x => new Leaderboards.Entry
                        {
                            Name = x.Name,
                            Value = x.Value.ToString()
                        }),
                    MostDependencies = context.Dependencies
                        .GroupBy(x => x.DependencyId)
                        .Select(x => new
                        {
                            Name = x.Key,
                            Value = x.Select(y => y.Id).Distinct().Count()
                        })
                        .OrderByDescending(x => x.Value)
                        .Take(10)
                        .ToList()
                        .Select(x => new Leaderboards.Entry
                        {
                            Name = x.Name,
                            Value = x.Value.ToString()
                        })
                });
            }
        }

        [GET("status")]
        public virtual ActionResult Status()
        {
            using (NuGetStatsDataContext context = new NuGetStatsDataContext())
            {
                return View(new Status
                {
                    Histories = context.Histories.OrderByDescending(x => x.StartTime).Take(10).ToList().OrderBy(x => x.StartTime)
                });
            }
        }
    }
}