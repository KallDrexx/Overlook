using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Testing;
using Overlook.Server.Storage;
using Overlook.Server.Web;

namespace Overlook.Tests.Web
{
    [TestClass]
    public class MetricsModuleTests
    {
        private Mock<IStorageEngine> _storageEngine;
        private INancyBootstrapper _bootstrapper;
        private Browser _browser;

        [TestInitialize]
        public void Init()
        {
            _storageEngine = new Mock<IStorageEngine>();
            _bootstrapper = new OverlookBootStrapper(_storageEngine.Object);
            _browser = new Browser(_bootstrapper);
        }

        [TestMethod]
        public void List_Returns_OK_Response()
        {
            var result = _browser.Get("/metrics/list", with => with.HttpRequest());
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Incorrect status code returned");
        }

        [TestMethod]
        public void List_Gets_Metrics_From_Storage_Engine()
        {
            _browser.Get("/metrics/list", with => with.HttpRequest());
            _storageEngine.Verify(x => x.GetKnownMetrics(), Times.Once());
        }
    }
}
