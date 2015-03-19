using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaveAGlick.Models.Home
{
    public class Index
    {
        public IEnumerable<BlogPost> Posts { get; set; }
        public IEnumerable<KeyValuePair<string, int>> Tags { get; set; }
    }
}