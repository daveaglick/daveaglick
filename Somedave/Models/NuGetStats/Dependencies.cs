using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Somedave.Models.NuGetStats
{
    public class Dependencies
    {
        public string Package { get; set; }
        public IEnumerable<string> Dependency { get; set; } 
        public IEnumerable<string> Dependent { get; set; } 
    }
}