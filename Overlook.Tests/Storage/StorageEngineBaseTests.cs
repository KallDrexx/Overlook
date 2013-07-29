using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Overlook.Common.Data;
using Overlook.Server.Storage;

namespace Overlook.Tests.Storage
{
    public class StorageEngineBaseTests
    {
        protected IStorageEngine _storageEngine;

        [Test]
        public void Can_Save_Snapshots_And_Get_By_Date_Range()
        {
            var snapshot1 = new Snapshot
            {
                Date = DateTime.Now,
                Metrics = new[]
                {
                    new Metric {Value = 123.4m},
                    new Metric {Value = 55m}
                }
            };

            _storageEngine.StoreSnapshot(snapshot1);

            var results = _storageEngine.GetSnapshots(snapshot1.Date.AddMinutes(-1),
                                                      snapshot1.Date.AddMinutes(1));

            Assert.IsNotNull(results, "Null results returned");
            Assert.AreEqual(1, results.Length, "Incorrect number of results returned");

            var result1 = results[0];
            Assert.AreEqual(snapshot1.Date, result1.Date, "Snapshot dates did not match");
            Assert.AreEqual(snapshot1.Metrics.Length, result1.Metrics.Length, "Snapshot had different number of metrics");

            var resultMetric1 = result1.Metrics[0];
            var resultMetric2 = result1.Metrics[1];
            Assert.IsTrue(snapshot1.Metrics.Any(x => x.Value == resultMetric1.Value), "Original snapshot did not contain the a metric with the same raw value");
            Assert.IsTrue(snapshot1.Metrics.Any(x => x.Value == resultMetric2.Value), "Original snapshot did not contain the a metric with the same raw value");
        }
    }
}
