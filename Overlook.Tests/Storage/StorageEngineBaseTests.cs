using System;
using System.Linq;
using NUnit.Framework;
using Overlook.Common.Data;
using Overlook.Common.Data.Metrics;
using Overlook.Common.Search;
using Overlook.Server.Storage;

namespace Overlook.Tests.Storage
{
    [TestFixture(Ignore = true)]
    public class StorageEngineBaseTests
    {
        protected IStorageEngine _storageEngine;

        [Test]
        public void Can_Save_And_Search_Snapshots()
        {
            var snapshot = new Snapshot
            {
                DateAndTime = DateTime.Now,
                Metrics = new[]
                {
                    new TestMetric {RawValue = "1"},
                    new TestMetric {RawValue = "2"}
                }
            };

            _storageEngine.StoreSnapshot(snapshot);
            var results = _storageEngine.Search(new DateRangeSearch
            {
                StartDate = snapshot.DateAndTime.AddMinutes(-1),
                EndDate = snapshot.DateAndTime.AddMinutes(1)
            });

            Assert.IsNotNull(results, "Null results returned");
            Assert.AreEqual(1, results.Length, "Incorrect number of results returned");

            var returnedSnapshot = results[0];
            Assert.AreEqual(snapshot.DateAndTime,returnedSnapshot.DateAndTime, "Returned snapshot had an incorrect date and time");
            Assert.AreEqual(snapshot.Metrics.Count(), returnedSnapshot.Metrics.Count(), "Returned snapshot had an incorrect number of metrics");

            var expectedMetrics = snapshot.Metrics.ToArray();
            var returnedMetrics = returnedSnapshot.Metrics.ToArray();

            Assert.AreEqual(expectedMetrics[0].MetricTypeId, returnedMetrics[0].MetricTypeId, "First returned metric had an invalid id");
            Assert.AreEqual(expectedMetrics[0].RawValue, returnedMetrics[0].RawValue, "First returned metric had an invalid raw value");
            Assert.AreEqual(expectedMetrics[1].MetricTypeId, returnedMetrics[1].MetricTypeId, "Second returned metric had an invalid id");
            Assert.AreEqual(expectedMetrics[1].RawValue, returnedMetrics[1].RawValue, "Second returned metric had an invalid raw value");
        }

        [Test]
        public void Storage_Engine_Returns_Empty_Array_When_No_Results_Matched_Search()
        {
            var snapshot = new Snapshot
            {
                DateAndTime = DateTime.Now,
                Metrics = new[]
                {
                    new TestMetric {RawValue = "1"},
                    new TestMetric {RawValue = "2"}
                }
            };

            _storageEngine.StoreSnapshot(snapshot);
            var results = _storageEngine.Search(new DateRangeSearch
            {
                StartDate = snapshot.DateAndTime.AddMinutes(1),
                EndDate = snapshot.DateAndTime.AddMinutes(2)
            });

            Assert.IsNotNull(results, "Null results returned");
            Assert.IsEmpty(results, "Returned results array was not empty");
        }
    }
}
