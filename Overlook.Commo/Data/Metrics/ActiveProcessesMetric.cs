using System;

namespace Overlook.Common.Data.Metrics
{
    public class ActiveProcessesMetric : IMetric
    {
        public Guid MetricTypeId { get { return new Guid("02C1E856-719A-400D-82AA-F5092E442F58"); } }
        public string Name { get { return "Active Processes"; } }
        public string RawValue { get; set; }
    }
}
