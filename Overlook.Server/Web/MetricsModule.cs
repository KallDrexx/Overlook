using System;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
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
                DateTime.TryParse(Request.Query.end, out end);
                var metric = Metric.Create(Request.Query.metrics);
                var query = new Query
                {
                    StartDate = start,
                    EndDate = end,
                    Metrics = new Metric[] {metric}
                };

                var results = _storageEngine.ExecuteQuery(query);
                return Response.AsJson(results.ToArray());
            };
        }
    }
}
