using System;
using System.Linq;
using Lucene.Net.Store;
using NUnit.Framework;
using Overlook.Common.Data;
using Overlook.Common.Data.Metrics;
using Overlook.Server.Storage;

namespace Overlook.Tests.Storage
{
    [TestFixture]       
    public class LuceneStorageEngineTests
    {
        private IStorageEngine _storageEngine;

        [SetUp]
        public void Setup()
        {
            var directory = new RAMDirectory();
            _storageEngine = new LuceneStorageEngine(directory);
        }

        [Test]
        public void Can_Save_Snapshots_And_Get_By_Date_Range()
        {
            var snapshot1 = new Snapshot
            {
                Date = DateTime.Now,
                Metrics = new[]
                {
                    new TestMetric {RawValue = 123.4m},
                    new TestMetric {RawValue = 55m}
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
            Assert.IsTrue(snapshot1.Metrics.Any(x => x.RawValue == resultMetric1.RawValue), "Original snapshot did not contain the a metric with the same raw value");
            Assert.IsTrue(snapshot1.Metrics.Any(x => x.RawValue == resultMetric2.RawValue), "Original snapshot did not contain the a metric with the same raw value");
        }
    }
}
