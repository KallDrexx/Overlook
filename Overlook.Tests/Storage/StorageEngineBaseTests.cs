using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Overlook.Common.Data;
using Overlook.Common.Queries;
using Overlook.Server.Storage;

namespace Overlook.Tests.Storage
{
    public class StorageEngineBaseTests
    {
        protected IStorageEngine _storageEngine;

        [Test]
        public void Can_Save_Snapshots_And_Get_By_Date_Range()
        {
            const string device = "device";
            const string category = "category";
            const string name1 = "name1";
            const string suffix = "suffix";
            const decimal metric1Value = 123.4m;

            var metric1 = new Metric(device, category, name1, suffix);

            var snapshot1 = new Snapshot
            {
                Date = DateTime.Now,
                MetricValues = new KeyValuePair<Metric, decimal>[]
                {
                    new KeyValuePair<Metric, decimal>(metric1, metric1Value), 
                }
            };

            _storageEngine.StoreSnapshot(snapshot1);

            var query = new Query
            {
                StartDate = snapshot1.Date.AddMinutes(-1),
                EndDate = snapshot1.Date.AddMinutes(1),
                Metrics = new[] {metric1}
            };

            var results = _storageEngine.ExecuteQuery(query);

            Assert.IsNotNull(results, "Null results returned");

            var resultsArray = results.ToArray();
            Assert.AreEqual(1, resultsArray.Length, "Incorrect number of results were returned");
            Assert.AreEqual(metric1, resultsArray[0].Metric, "Returned metric was not correct");
            Assert.IsNotNull(resultsArray[0].Values, "Returned metric values array was null");
            Assert.AreEqual(1, resultsArray[0].Values.Length, "Incorrect number of metric values returned");
            Assert.AreEqual(metric1Value, resultsArray[0].Values[0], "Incorrect metric value");
        }
    }
}
