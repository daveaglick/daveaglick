using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting;
using AttributeRouting.Web.Mvc;
using GraphSharp.Algorithms.Layout.Simple.Hierarchical;
using LinqToSqlRetry;
using QuickGraph;
using Index = Somedave.Models.Home.Index;
using GraphSharp.Algorithms.Layout.Simple.FDP;

namespace Somedave.Controllers
{
    [AttributeRouting.RoutePrefix("nuget-stats")]
    public partial class NuGetStatsController : Controller
    {
        [GET("{a?}/{b?}/{c?}")]
        public virtual ActionResult Index()
        {
            return View();
        }
    }
}