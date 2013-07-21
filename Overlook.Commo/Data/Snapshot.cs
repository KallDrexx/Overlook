using System;
using System.Collections.Generic;
using Overlook.Common.Data.Metrics;

namespace Overlook.Common.Data
{
    public class Snapshot
    {
        public DateTime DateAndTime { get; set; }
        public IEnumerable<IMetric> Metrics { get; set; }
    }
}
