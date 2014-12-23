using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToSqlRetry;

namespace NuGetUpdate
{
    public class LinearUpdateStatsRetry : LinearRetry
    {
        private readonly NuGetStatsDataContext _context;

        public LinearUpdateStatsRetry(NuGetStatsDataContext context)
        {
            _context = context;
        }

        public override TimeSpan? ShouldRetry(int retryCount, Exception exception)
        {
            if (retryCount == 0 && _context != null)
            {
                _context.ExecuteCommand("exec sp_updatestats");
            }
            return base.ShouldRetry(retryCount, exception);
        }
    }
}
