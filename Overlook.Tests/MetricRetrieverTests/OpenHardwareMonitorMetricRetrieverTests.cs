using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Overlook.Server.MetricRetriever;

namespace Overlook.Tests.MetricRetrieverTests
{
    [TestClass]
    public class OpenHardwareMonitorMetricRetrieverTests
    {
        //[TestMethod] // Ignored test as OHM gets a null reference exception when unit testing
        public void Can_Get_Metrics()
        {
            var retriever = new OpenHardwareMonitorMetricRetriever();
            var metricValues = retriever.GetCurrentMetricValues();

            // Since what gets returned is heavily based on what machine its run on
            // just make sure the results weren't empty
            Assert.IsNotNull(metricValues, "Returned metric values was null");
            Assert.IsTrue(metricValues.Any(), "No metric values returned");
        }
    }
}
