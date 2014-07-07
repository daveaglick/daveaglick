using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Somedave
{
    public abstract class BlogPostViewPage<T> : SomedaveViewPage<T>
    {
        public string Title { get; set; }
        public string Lead { get; set; }
        public DateTime Published { get; set; }
        public DateTime Edited { get; set; }
        public string[] Tags { get; set; }
    }
}