using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting.Web.Mvc;
using RazorDatabase;
using Somedave.Models.Blog;

namespace Somedave.Controllers
{
    public partial class BlogController : Controller
    {
        [GET("posts/{viewName?}")]
        public virtual ActionResult Posts(string viewName)
        {
            if (viewName == null)
            {
                return View("Archive", 
                    RazorDb.Get<BlogPost>()
                        .Where(x => x.IsPublished())
                        .OrderByDescending(x => x.Published)
                        .GroupBy(x => x.Published == default(DateTime) ? Int32.MaxValue : x.Published.Year)
                        .OrderByDescending(x => x.Key));
            }

            // Uncomment the publish check below to return a 404 if the post isn't published yet
            BlogPost post = RazorDb.Get<BlogPost>().FirstOrDefault(x => x.GetViewName() == viewName);
            if (post == null /* || !post.IsPublished() */)
            {
                throw new HttpException(404, "Page Not Found");
            }
            return View("Posts/" + viewName, post);
        }

        [GET("tags/{tag?}")]
        public virtual ActionResult Tags(string tag)
        {
            IEnumerable<string> tags = RazorDb.Get<BlogPost>()
                .Where(x => x.Tags != null)
                .SelectMany(x => x.Tags)
                .Distinct();
            return View(new Tags()
                {
                    Tag = tags.FirstOrDefault(x => x.ToLowerInvariant().Replace(' ', '-') == tag),
                    Posts = tag == null ? null : RazorDb.Get<BlogPost>()
                        .Where(x => x.IsPublished() && x.Tags != null && x.Tags.Any(y => y.ToLowerInvariant().Replace(' ', '-') == tag))
                        .OrderByDescending(x => x.Published),
                    AllTags = tags
                        .Select(x => new KeyValuePair<string, int>(x, RazorDb.Get<BlogPost>()
                            .Where(y => y.Tags != null)
                            .Count(y => y.Tags.Contains(x))))
                        .OrderBy(x => x.Key)
                }
            );
        }
    }
}
