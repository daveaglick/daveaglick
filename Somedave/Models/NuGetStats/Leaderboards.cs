using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Somedave.Models.NuGetStats
{
    public class Leaderboards
    {
        public class Entry
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public IEnumerable<Entry> MostDownloadedPackages { get; set; } 
        public IEnumerable<Entry> MostDownloadedAuthors { get; set; }
        public IEnumerable<Entry> MostDependencies { get; set; } 
    }
}