using System;
using Overlook.Common.Data;

namespace Overlook.Common.Queries
{
    public class Query
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Metric[] Metrics { get; set; }
        public QueryResolution Resolution { get; set; }
    }
}
