using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Somedave.Models.NuGetStats
{
    public class Leaderboard
    {
        public class Meta
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Heading1 { get; set; }
            public string Heading2 { get; set; }
            public bool IsPackage { get; set; }
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