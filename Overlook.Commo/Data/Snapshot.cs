using System;
using System.Collections.Generic;

namespace Overlook.Common.Data
{
    public class Snapshot
    {
        public DateTime Date { get; set; }
        public Metric[] Metrics { get; set; }
    }
}
