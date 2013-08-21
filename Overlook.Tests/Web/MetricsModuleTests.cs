using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nancy;
using Nancy.Testing;
using Newtonsoft.Json;
using System.Linq;
using Overlook.Common.Data;
using Overlook.Common.Queries;
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

        [TestMethod]
        public void Query_Builds_Correct_Query_From_Parameters()
        {
            var metric = new Metric("device", "category", "name", "suffix");
            var path = "/metrics/query";
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddMinutes(10);

            // Set dates to their parsed dates to make sure they are equal to how they will
            // be read by the query string
            startDate = DateTime.Parse(startDate.ToString());
            endDate = DateTime.Parse(endDate.ToString());

            var response = _browser.Get(path, with =>
            {
                with.HttpRequest();
                with.Query("start", startDate.ToString());
                with.Query("end", endDate.ToString());
                with.Query("metrics", metric.ToParsableString());
            });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Incorrect status code returned");
            _storageEngine.Verify(x => x.ExecuteQuery(It.Is<Query>(y => y.StartDate == startDate)), Times.Once());
            _storageEngine.Verify(x => x.ExecuteQuery(It.Is<Query>(y => y.EndDate == endDate)), Times.Once());
            _storageEngine.Verify(x => x.ExecuteQuery(It.Is<Query>(y => y.Metrics.Contains(metric))), Times.Once());
        }

        [TestMethod]
        public void Query_Returns_Results_From_Storage_Engine()
        {
            var metric1 = new Metric("device", "category", "name", "suffix");
            var value1 = 3m;
            var date1 = DateTime.Parse(DateTime.Now.ToString());
            var expectedResults = new QueriedMetricResult
            {
                Metric = metric1,
                Values = new[] {new KeyValuePair<DateTime, decimal>(date1, value1)}
            };

            _storageEngine.Setup(x => x.ExecuteQuery(It.IsAny<Query>()))
                          .Returns(new[] {expectedResults});

            var response = _browser.Get("/metrics/query", with => with.HttpRequest());
            var result = JsonConvert.DeserializeObject<QueriedMetricResult[]>(response.Body.AsString());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Incorrect status code returned");
            Assert.IsNotNull(result, "Returned result was not a valid QueriedMetricResult array");
            Assert.AreEqual(1, result.Length, "Returned array had an incorrect number of elements");
            Assert.AreEqual(metric1, result[0].Metric, "Returned array's metric was incorrect");
            Assert.AreEqual(1, result[0].Values.Length, "Value array had an incorrect number of elements");
            Assert.AreEqual(date1, result[0].Values[0].Key, "Value array's date was incorrect");
            Assert.AreEqual(value1, result[0].Values[0].Value, "Value array's value was incorrect");
        }

        [TestMethod]
        public void Query_Assumes_Max_DateTime_For_End_If_Not_Specified()
        {
            var path = "/metrics/query";

            var response = _browser.Get(path, with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Incorrect status code returned");
            _storageEngine.Verify(x => x.ExecuteQuery(It.Is<Query>(y => y.EndDate == DateTime.MaxValue)), Times.Once());
        }

        [TestMethod]
        public void Query_Assumes_Min_DateTime_For_Start_If_Not_Specified()
        {
            var path = "/metrics/query";

            var response = _browser.Get(path, with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Incorrect status code returned");
            _storageEngine.Verify(x => x.ExecuteQuery(It.Is<Query>(y => y.StartDate == DateTime.MinValue)), Times.Once());
        }
    }
}
