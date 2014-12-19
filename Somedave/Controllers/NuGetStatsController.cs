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
            return View();
        }

        [GET("status")]
        public virtual ActionResult Status()
        {
            Status status;
            using (NuGetStatsDataContext context = new NuGetStatsDataContext())
            {
                status = new Status
                {
                    Histories = context.Histories.OrderByDescending(x => x.StartTime).Take(10).ToList().OrderBy(x => x.StartTime)
                };
            }
            return View(status);
        }
    }
}