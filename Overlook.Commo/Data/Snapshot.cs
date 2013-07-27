using System;
using System.Collections.Generic;
using Overlook.Common.Data.Metrics;

namespace Overlook.Common.Data
{
    public class Snapshot
    {
        public DateTime Date { get; set; }
        public IMetric[] Metrics { get; set; }
    }
}
