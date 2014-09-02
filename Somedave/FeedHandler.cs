using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Syndication;
using System.Xml;
using RazorDatabase;
using System.Web.Mvc;

namespace Somedave
{
    // Copied mostly from MiniBlog - https://github.com/madskristensen/MiniBlog/blob/master/Website/app_code/handlers/FeedHandler.cs
    public class FeedHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            SyndicationFeed feed = new SyndicationFeed()
            {
                Title = new TextSyndicationContent("Somedave"),
                Description = new TextSyndicationContent("Latest blog posts by Dave Glick"),
                BaseUri = new Uri(context.Request.Url.Scheme + "://" + context.Request.Url.Authority),
                Items = GetItems(),
            };

            feed.Links.Add(new SyndicationLink(feed.BaseUri));

            using (XmlTextWriter writer = new XmlTextWriter(context.Response.Output))
            {
                SyndicationFeedFormatter formatter = GetFormatter(context, feed);
                formatter.WriteTo(writer);
            }

            context.Response.ContentType = "text/xml";
        }

        private IEnumerable<SyndicationItem> GetItems()
        {
            UrlHelper urlHelper = new UrlHelper(((MvcHandler)HttpContext.Current.Handler).RequestContext);
            foreach (BlogPost post in RazorDb.Get<BlogPost>().Where(x => x.IsPublished()).OrderByDescending(x => x.Published).Take(10))
            {
                string action = urlHelper.Action(post.GetAction());
                SyndicationItem item = new SyndicationItem(
                    post.Title + (string.IsNullOrWhiteSpace(post.Lead) ? string.Empty : " - " + post.Lead), 
                    post.Rendered, new Uri(action), action, post.Edited == default(DateTime) ? post.Published : post.Edited)
                {
                    PublishDate = post.Published
                };

                string excerpt = post.GetExcerpt();
                if (!string.IsNullOrWhiteSpace(excerpt))
                {
                    item.Summary = new TextSyndicationContent(excerpt);
                }

                item.Authors.Add(new SyndicationPerson("", "Dave Glick", ""));
                yield return item;
            }
        }

        private SyndicationFeedFormatter GetFormatter(HttpContext context, SyndicationFeed feed)
        {
            string path = context.Request.Path.Trim('/');
            int index = path.LastIndexOf('/');

            if (index > -1 && path.Substring(index + 1) == "atom")
            {
                context.Response.ContentType = "application/atom+xml";
                return new Atom10FeedFormatter(feed);
            }

            context.Response.ContentType = "application/rss+xml";
            return new Rss20FeedFormatter(feed);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}