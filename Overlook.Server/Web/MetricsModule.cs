using System;
using System.Linq;
using Nancy;
using Overlook.Common.Data;
using Overlook.Common.Queries;
using Overlook.Server.Storage;

namespace Overlook.Server.Web
{
    public class MetricsModule : NancyModule
    {
        private IStorageEngine _storageEngine;

        public MetricsModule(IStorageEngine storageEngine) : base("/metrics")
        {
            _storageEngine = storageEngine;

            Get["/list"] = parameters =>
            {
                var metrics = _storageEngine.GetKnownMetrics();
                return Response.AsJson(metrics.ToArray());
            };

            Get["/query"] = parameters =>
            {
                DateTime start, end;
                DateTime.TryParse(Request.Query.start, out start);
                if (!DateTime.TryParse(Request.Query.end, out end))
                    end = DateTime.MaxValue;

                string rawMetrics = Convert.ToString(Request.Query.metric.Value);
                var metrics = new Metric[0];
                if (rawMetrics != null)
                {
                    var splitRawMetrics = rawMetrics.Split(',');
                    metrics = splitRawMetrics.Select(Metric.Create).ToArray();
                }

                var query = new Query
                {
                    StartDate = start,
                    EndDate = end,
                    Metrics = metrics
                };

                var results = _storageEngine.ExecuteQuery(query);
                return Response.AsJson(results.ToArray());
            };
        }
    }
}
