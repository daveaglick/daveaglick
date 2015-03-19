using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RazorDatabase;
using System.Web.Mvc;
using HtmlAgilityPack;

namespace DaveAGlick
{
    public interface IBlogPost
    {
        string Title { get; set; }
        string Lead { get; set; }
        DateTime Published { get; set; }
        DateTime Edited { get; set; }
        string[] Tags { get; set; }
    }

    public class BlogPost : ViewType<BlogPostViewPage<dynamic>>, IBlogPost
    {
        public string Title { get; set; }
        public string Lead { get; set; }
        public DateTime Published { get; set; }
        public DateTime Edited { get; set; }
        public string[] Tags { get; set; }

        // Exclude the layout view
        protected override bool GetView(Type x)
        {
            return x.Name != "Layout";
        }

        public string GetViewName()
        {
            return ViewTypeName
                .Replace("_Views_Blog_Posts_", "")
                .Replace("_cshtml", "")
                .Replace('_', '-');
        }

        public ActionResult GetAction()
        {
            // Replace the underscores with hyphens since they got substituted in the type name
            return MVC.Blog.Posts(GetViewName());
        }

        // Check if this should be published
        public bool IsPublished()
        {
            return Published == default(DateTime) || Published < DateTime.Now;
        }

        // Gets the first paragraph
        public string GetExcerpt()
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(RenderedContent);
                HtmlNode p = doc.DocumentNode.SelectSingleNode("//p");
                if (p != null)
                {
                    return p.OuterHtml;
                }
            }
            catch(Exception)
            {     
            }
            return string.Empty;
        }
    }
}