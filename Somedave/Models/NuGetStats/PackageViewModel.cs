using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Somedave.Models.NuGetStats
{
    public class PackageViewModel
    {
        public class Version
        {
            public string Name { get; set; }
            public int DownloadCount { get; set; }
            public DateTime Created { get; set; }
        }

        public string Id { get; set; }
        public IEnumerable<Version> Versions { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<KeyValuePair<string, int>> Dependent { get; set; } 
        public IEnumerable<KeyValuePair<string, int>> Dependency { get; set; } 
    }
}