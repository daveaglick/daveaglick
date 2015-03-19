using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting.Web.Mvc;
using DaveAGlick.Models.Home;
using RazorDatabase;

namespace DaveAGlick.Controllers
{
    public partial class HomeController : Controller
    {
        [GET("")]
        public virtual ActionResult Index()
        {
            return View(new Index()
                {
                    Posts = RazorDb.Get<BlogPost>()
                        .Where(x => x.IsPublished())
                        .OrderByDescending(x => x.Published)
                        .Take(3),
                    Tags = RazorDb.Get<BlogPost>()
                        .Where(x => x.Tags != null)
                        .SelectMany(x => x.Tags)
                        .Distinct()
                        .Select(x => new KeyValuePair<string, int>(x, RazorDb.Get<BlogPost>()
                            .Where(y => y.Tags != null)
                            .Count(y => y.Tags.Contains(x))))
                        .OrderByDescending(x => x.Value)
                        .Take(10)
                }
            );
        }

        [GET("about")]
        public virtual ActionResult About()
        {

            return View();
        }

        [GET("likes")]
        public virtual ActionResult Likes()
        {
            return View();
        }
    }
}
