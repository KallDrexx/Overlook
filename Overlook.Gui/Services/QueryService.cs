using System.Collections.Generic;
using Newtonsoft.Json;
using Overlook.Common.Data;
using RestSharp;

namespace Overlook.Gui.Services
{
    public class QueryService
    {
        private const string ListServicePath = "/metrics/list";
        private const string QueryServicePath = "/metrics/query";

        public IEnumerable<Metric> GetAvailableMetrics(string webserviceUrl)
        {
            var client = new RestClient(webserviceUrl);
            var request = new RestRequest(ListServicePath, Method.GET);
            var response = client.Execute(request);
            if (response.ErrorException != null)
                throw response.ErrorException;

            // Use Json.Net explicitly instead of using the generic version of 
            // client.Execute<T>() because Metric does not have a parameterless
            // constructor, and thus can't be used.
            var metrics = JsonConvert.DeserializeObject<Metric[]>(response.Content);
            return metrics;
        }

        public QueriedMetricResult[] QueryForMetrics(string webserviceUrl, IEnumerable<Metric> metrics)
        {
            var client = new RestClient(webserviceUrl);
            var request = new RestRequest(QueryServicePath, Method.GET);

            foreach (var metric in metrics)
            {
                request.Parameters.Add(new Parameter
                {
                    Name = "metric",
                    Value = metric.ToParsableString(),
                    Type = ParameterType.GetOrPost
                });
            }

            var response = client.Execute(request);
            if (response.ErrorException != null)
                throw response.ErrorException;

            var result = JsonConvert.DeserializeObject<QueriedMetricResult[]>(response.Content);
            return result;
        }
    }
}
