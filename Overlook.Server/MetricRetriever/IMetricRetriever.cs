using System;
using System.Collections.Generic;
using Overlook.Common.Data;

namespace Overlook.Server.MetricRetriever
{
    public interface IMetricRetriever
    {
        IEnumerable<KeyValuePair<Metric, decimal>> GetCurrentMetricValues();
    }
}
