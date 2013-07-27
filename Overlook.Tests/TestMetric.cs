using System;
using Overlook.Common.Data.Metrics;

namespace Overlook.Tests
{
    public class TestMetric : IMetric 
    {
        public Guid MetricTypeId { get { return new Guid("EF9D4C62-DB17-4A2C-8D7C-E9EA39FE8A9E"); } }
        public string Name { get { return "Test Metric"; } }
        public decimal RawValue { get; set; }
    }
}
