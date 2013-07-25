using System;
using System.Collections.Generic;

namespace Overlook.Common.Query
{
    public class QueryResult
    {
        public DateTime SnapshotDate { get; set; }
        public Dictionary<Guid, decimal> MetricValues { get; set; } 
    }
}
