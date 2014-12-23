using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Somedave.Models.NuGetStats
{
    public class Index
    {
        public IEnumerable<KeyValuePair<string, Leaderboard.Meta>> Leaderboards { get; set; } 
    }
}