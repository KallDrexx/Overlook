using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nancy;
using Nancy.Testing;
using Newtonsoft.Json;
using System.Linq;
using Overlook.Common.Data;
using Overlook.Server.Storage;
using Overlook.Server.Web;

namespace Overlook.Tests.Web
{
    [TestClass]
    public class MetricsModuleTests
    {
        private Mock<IStorageEngine> _storageEngine;
        private Browser _browser;

        [TestInitialize]
        public void Init()
        {
            _storageEngine = new Mock<IStorageEngine>();

             var bootstrapper = new OverlookBootStrapper(_storageEngine.Object);
             _browser = new Browser(bootstrapper);
        }

        [TestMethod]
        public void List_Returns_Correct_Metrics()
        {
            var metric1 = new Metric("device1", "category1", "name1", "suffix");
            var metric2 = new Metric("device2", "category2", "name2", "suffix");
            _storageEngine.Setup(x => x.GetKnownMetrics()).Returns(new[] { metric1, metric2 });

            var result = _browser.Get("/metrics/list", with => with.HttpRequest());
            var metrics = JsonConvert.DeserializeObject<Metric[]>(result.Body.AsString());

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Incorrect status code returned");
            _storageEngine.Verify(x => x.GetKnownMetrics(), Times.Once());
            Assert.IsNotNull(metrics, "Metrics array was null");
            Assert.AreEqual(2, metrics.Length, "Metrics array length was incorrect");
            Assert.IsTrue(metrics.Contains(metric1), "First metric is not in the returned array");
            Assert.IsTrue(metrics.Contains(metric2), "Second metric is not in the returned array");
        }
    }
}
