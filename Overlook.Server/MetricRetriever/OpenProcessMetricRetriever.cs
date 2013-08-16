using System.Collections.Generic;
using System.Diagnostics;
using Overlook.Common.Data;

namespace Overlook.Server.MetricRetriever
{
    public class OpenProcessMetricRetriever : IMetricRetriever
    {
        public IEnumerable<KeyValuePair<Metric, decimal>> GetCurrentMetricValues()
        {
            const string device = "Processes";
            const string openCategory = "Open";

            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                var name = process.ProcessName;
                var metric = new Metric(device, openCategory, name, string.Empty);
                yield return new KeyValuePair<Metric, decimal>(metric, 1m);
            }
        }
    }
}
