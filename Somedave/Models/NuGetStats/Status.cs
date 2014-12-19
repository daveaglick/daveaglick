using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Somedave.Models.NuGetStats
{
    public class Status
    {
        public IEnumerable<History> Histories { get; set; }
    }
}