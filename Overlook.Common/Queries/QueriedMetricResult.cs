using System;
using System.Collections.Generic;

namespace Overlook.Common.Data
{
    public class QueriedMetricResult
    {
        public Metric Metric { get; set; }
        public KeyValuePair<DateTime, decimal>[] Values { get; set; }
    }
}
