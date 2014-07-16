using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting.Web.Mvc;
using RazorDatabase;

namespace Somedave.Controllers
{
    public partial class BlogController : Controller
    {
        [GET("{viewName}")]
        public virtual ActionResult Post(string viewName)
        {
            // Make sure this post is actually published
            BlogPost post = RazorDb.Get<BlogPost>().FirstOrDefault(x => x.GetViewName() == viewName);
            if (post == null || !post.IsPublished())
            {
                return HttpNotFound();
            }
            return View("Posts/" + viewName);
        }

        [GET("archive")]
        public virtual ActionResult Archive()
        {
            return View();
        }
    }
}
