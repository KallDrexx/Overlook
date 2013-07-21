using System;

namespace Overlook.Common.Data.Metrics
{
    public interface IMetric
    {
        Guid MetricTypeId { get; }
        string Name { get; }
        string RawValue { get; set; }
    }
}
