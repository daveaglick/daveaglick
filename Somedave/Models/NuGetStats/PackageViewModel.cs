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

        public class DependencyData
        {
            // Key = package, value = minimum depth
            public Dictionary<string, int> Dependents { get; set; }
            public Dictionary<string, int> Dependencies { get; set; }
            public double XCoord { get; set; }
            public double YCoord { get; set; }

            public DependencyData()
            {
                Dependents = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                Dependencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }

            public bool HasDependents
            {
                get { return Dependents.Count > 0; }
            }

            public bool HasDependencies
            {
                get { return Dependencies.Count > 0; }
            }
        }

        public string Id { get; set; }
        public IEnumerable<Version> Versions { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IDictionary<string, DependencyData> Dependencies { get; set; } 
    }
}