using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Overlook.Common.Data;
using Overlook.Common.Data.Metrics;
using Overlook.Common.Query;
using Overlook.Server.Operations;
using Overlook.Server.Storage;

namespace Overlook.Tests.Storage
{
    public class StorageEngineBaseTests
    {
        protected StorageEngineBase _storageEngine;

        [Test]
        public void Store_Snapshot_Calls_Success_Delegate_On_Write_Operation()
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

            const int maxSecondsToWait = 15;
            bool? successRan = null;
            IOperation resultingOperation = null;
            OperationSuccessDelegate success = operation => { successRan = true; resultingOperation = operation; };
            OperationFailureDelegate failure = (operation, ex) => { successRan = false; resultingOperation = operation; };

            _storageEngine.StoreSnapshot(snapshot, success, failure);

            var startingTime = DateTime.Now;
            while ((DateTime.Now - startingTime).TotalSeconds < maxSecondsToWait)
            {
                if (successRan != null)
                    break;

                Thread.Sleep(500);
            }

            Assert.IsNotNull(successRan, "Callbacks were not called in the allotted time");
            Assert.IsTrue(successRan.Value, "Failure callback was called instead of success callback");
            Assert.IsNotNull(resultingOperation, "Supplied operation was null");
            Assert.IsInstanceOf<StoreSnapshotOperation>(resultingOperation, "Supplied operation was not the correct type");
            Assert.AreEqual(snapshot, ((StoreSnapshotOperation) resultingOperation).SnapshotToStore,
                            "Operation's snapshot was not correct");
        }

        [Test]
        public void Can_Retrieve_Saved_Snapshot()
        {
            const int maxSecondsToWait = 15;

            bool? writeResult = null;
            OperationSuccessDelegate writeSuccess = operation => writeResult = true;
            OperationFailureDelegate writeFailure = (operation, ex) => Assert.Fail("Write operation failed with exception: " + ex.GetType());

            bool? searchResult = null;
            IOperation queryOperation = null;
            OperationSuccessDelegate searchSuccess = operation => { searchResult = true; queryOperation = operation; };
            OperationFailureDelegate searchFailure = (operation, ex) => Assert.Fail("Search operation failed with exception: " + ex.GetType());

            var snapshot = new Snapshot
            {
                DateAndTime = DateTime.Now,
                Metrics = new[]
                {
                    new TestMetric {RawValue = "1"},
                    new TestMetric {RawValue = "2"}
                }
            };

            var query = new DateRangeQuery
            {
                StartDate = snapshot.DateAndTime.AddMinutes(-1),
                EndDate = snapshot.DateAndTime.AddMinutes(1)
            };

            // Run the test
            _storageEngine.StoreSnapshot(snapshot, writeSuccess,writeFailure);
            _storageEngine.RunQuery(query, searchSuccess, searchFailure);

            // Verify
            var startingTime = DateTime.Now;
            while ((DateTime.Now - startingTime).TotalSeconds < maxSecondsToWait)
            {
                if (searchResult != null)
                    break;

                Thread.Sleep(500);
            }

            Assert.IsNotNull(searchResult, "Callbacks were not called in the allotted time");
            Assert.IsTrue(searchResult.Value, "Failure callback was called instead of success callback");
            Assert.IsNotNull(queryOperation, "Supplied operation was null");
            Assert.IsInstanceOf<DateRangeQueryOperation>(queryOperation, "Query operation returned was of an incorrect type");
            Assert.IsInstanceOf<DateRangeQuery>(((IQueryOperation)queryOperation).RanQuery, "Ran query returned was not of the correct type");

            var ranQuery = (DateRangeQuery)((IQueryOperation)queryOperation).RanQuery;
            Assert.AreEqual(query.StartDate, ranQuery.StartDate, "Returned query's start date was incorrect");
            Assert.AreEqual(query.EndDate, ranQuery.EndDate, "Returned query's end date was incorrect");
            Assert.IsNotNull(ranQuery.Results, "Returned query's result array was null");
            Assert.AreEqual(1, ranQuery.Results.Length, "Returned query's result array had an incorrect number of elements");
            Assert.AreEqual(snapshot.DateAndTime, ranQuery.Results[0].SnapshotDate, "Returned query result had an incorrect snapshot date");
        }
    }
}
