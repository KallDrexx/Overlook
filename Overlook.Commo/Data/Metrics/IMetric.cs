using System;

namespace Overlook.Common.Data.Metrics
{
    public interface IMetric
    {
        Guid MetricTypeId { get; }
        string Name { get; }
        decimal RawValue { get; set; }
    }
}
