using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RazorDatabase;

namespace Somedave
{
    public class BlogPost : ViewType<BlogPostViewPage<dynamic>>
    {
        public DateTime Published { get; set; }
    }
}