using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Overlook.Common.Data;
using Overlook.Common.Data.Metrics;
using Overlook.Common.Search;
using Overlook.Server.Operations;
using Overlook.Server.Storage;

namespace Overlook.Tests.Storage
{
    [TestFixture(Ignore = true)]
    public class StorageEngineBaseTests
    {
        protected StorageEngineBase _storageEngine;

        [Test]
        public void Can_Save_Snapshots()
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
    }
}
