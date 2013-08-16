using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Overlook.Server.MetricRetriever;

namespace Overlook.Tests.MetricRetrieverTests
{
    [TestFixture]
    class OpenProcessMetricRetrieverTests
    {
        [Test]
        public void Can_Get_Running_Processes()
        {
            var retriever = new OpenProcessMetricRetriever();
            var metrics = retriever.GetCurrentMetricValues();
            
            // Since we can't gaurantee what processes will be shown, just 
            // make sure we at least metrics were returned
            Assert.IsNotNull(metrics, "Metric enumerable was null");
            Assert.IsNotEmpty(metrics, "Metric enumerable was empty");
        }
    }
}
