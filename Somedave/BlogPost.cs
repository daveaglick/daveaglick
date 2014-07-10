using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RazorDatabase;
using System.Web.Mvc;

namespace Somedave
{
    public class BlogPost : ViewType<BlogPostViewPage<dynamic>>
    {
        public string Title { get; set; }
        public string Lead { get; set; }
        public DateTime Published { get; set; }
        public DateTime Edited { get; set; }
        public string[] Tags { get; set; }

        public ActionResult Action()
        {
            // Replace the underscores with hyphens since they got substituted in the type name
            return MVC.Blog.Post(ViewTypeName.Replace('_', '-'));
        }

        // Exclude the layout view
        protected override bool GetView(Type x)
        {
            return x.Name != "Layout";
        }
    }
}