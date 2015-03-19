using AttributeRouting.Web.Mvc;
using RazorDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace DaveAGlick.Controllers
{
    public partial class BackendController : Controller
    {
        // Copied mostly from MiniBlog - https://github.com/madskristensen/MiniBlog/blob/master/Website/app_code/handlers/FeedHandler.cs
        [GET("feed/{feedType?}")]
        public virtual ActionResult Feed(string feedType)
        {
            Uri baseUri = new Uri(HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority);
            SyndicationFeed feed = new SyndicationFeed()
            {
                Title = new TextSyndicationContent("Dave Glick"),
                Description = new TextSyndicationContent("Latest blog posts by Dave Glick"),
                BaseUri = baseUri,
                Items = GetItems(baseUri),
            };

            feed.Links.Add(new SyndicationLink(feed.BaseUri));

            using(StringWriter content = new StringWriter())
            {
                using (XmlTextWriter writer = new XmlTextWriter(content))
                {
                    string contentType;
                    SyndicationFeedFormatter formatter = GetFormatter(feedType, feed, out contentType);
                    formatter.WriteTo(writer);
                    return Content(content.ToString(), contentType);
                }
            }
        }

        private IEnumerable<SyndicationItem> GetItems(Uri baseUri)
        {
            UrlHelper urlHelper = new UrlHelper(Request.RequestContext);
            foreach (BlogPost post in RazorDb.Get<BlogPost>().Where(x => x.IsPublished()).OrderByDescending(x => x.Published).Take(10))
            {
                Uri uri = new Uri(baseUri, urlHelper.Action(post.GetAction()));
                SyndicationItem item = new SyndicationItem(
                    post.Title + (string.IsNullOrWhiteSpace(post.Lead) ? string.Empty : " - " + post.Lead),
                    post.RenderedContent, uri, uri.ToString(), post.Edited == default(DateTime) ? post.Published : post.Edited)
                {
                    PublishDate = post.Published
                };

                item.Authors.Add(new SyndicationPerson("", "Dave Glick", ""));
                yield return item;
            }
        }

        private SyndicationFeedFormatter GetFormatter(string feedType, SyndicationFeed feed, out string contentType)
        {
            if (feedType == "atom")
            {
                contentType = "application/atom+xml";
                return new Atom10FeedFormatter(feed);
            }

            contentType = "application/rss+xml";
            return new Rss20FeedFormatter(feed);
        }
    }
}
