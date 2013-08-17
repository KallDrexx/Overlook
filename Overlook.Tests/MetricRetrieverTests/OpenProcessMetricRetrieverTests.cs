using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Overlook.Server.MetricRetriever;

namespace Overlook.Tests.MetricRetrieverTests
{
    [TestClass]
    public class OpenProcessMetricRetrieverTests
    {
        [TestMethod]
        public void Can_Get_Running_Processes()
        {
            var retriever = new OpenProcessMetricRetriever();
            var metrics = retriever.GetCurrentMetricValues();
            
            // Since we can't gaurantee what processes will be shown, just 
            // make sure we at least metrics were returned
            Assert.IsNotNull(metrics, "Metric enumerable was null");
            Assert.IsTrue(metrics.Any(), "Metric enumerable was empty");
        }
    }
}
