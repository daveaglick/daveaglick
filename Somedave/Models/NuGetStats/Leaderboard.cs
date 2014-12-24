using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Somedave.Models.NuGetStats
{
    public class Leaderboard
    {
        public class Meta
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string NameHeading { get; set; }
            public Func<string, UrlHelper, string> NameLink { get; set; }
            public string ValueHeading { get; set; }
            public Func<NuGetStatsDataContext, IEnumerable<Entry>> Entries { get; set; } 
        }

        public class Entry
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public Meta Metadata { get; set; }
        public IEnumerable<Entry> Entries { get; set; } 
    }
}