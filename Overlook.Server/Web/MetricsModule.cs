using System.Linq;
using Nancy;
using Overlook.Common.Data;
using Overlook.Server.Storage;

namespace Overlook.Server.Web
{
    public class MetricsModule : NancyModule
    {
        private IStorageEngine _storageEngine;

        public MetricsModule(IStorageEngine storageEngine) : base("/metrics")
        {
            _storageEngine = storageEngine;

            Get["/list"] = parameters => Response.AsJson(GetAvailableMetrics());
        }

        private Metric[] GetAvailableMetrics()
        {
            var metrics = _storageEngine.GetKnownMetrics();
            return metrics.ToArray();
        }
    }
}
