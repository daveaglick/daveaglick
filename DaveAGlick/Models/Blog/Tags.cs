using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaveAGlick.Models.Blog
{
    public class Tags
    {
        public string Tag { get; set; }
        public IEnumerable<BlogPost> Posts { get; set; }
        public IEnumerable<KeyValuePair<string, int>> AllTags { get; set; }
    }
}