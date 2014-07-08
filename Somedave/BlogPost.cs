using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RazorDatabase;

namespace Somedave
{
    public class BlogPost : ViewType<BlogPostViewPage<dynamic>>
    {
        public string Title { get; set; }
        public string Lead { get; set; }
        public DateTime Published { get; set; }
        public DateTime Edited { get; set; }
        public string[] Tags { get; set; }
    }
}