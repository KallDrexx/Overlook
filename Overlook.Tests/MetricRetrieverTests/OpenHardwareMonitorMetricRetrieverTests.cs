using NUnit.Framework;
using Overlook.Server.MetricRetriever;

namespace Overlook.Tests.MetricRetrieverTests
{
    [TestFixture]
    public class OpenHardwareMonitorMetricRetrieverTests
    {
        [Ignore("OpenHardwareMonitor crashes in Nunit due to Assembly.GetEntryAssembly() being null")]
        public void Can_Get_Metrics()
        {
            var retriever = new OpenHardwareMonitorMetricRetriever();
            var metricValues = retriever.GetCurrentMetricValues();

            // Since what gets returned is heavily based on what machine its run on
            // just make sure the results weren't empty
            Assert.IsNotNull(metricValues, "Returned metric values was null");
            Assert.IsNotEmpty(metricValues, "No metric values returned");
        }
    }
}
