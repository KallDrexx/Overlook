using System;
using System.Collections.Generic;
using Overlook.Common.Data;
using Overlook.Common.Queries;

namespace Overlook.Server.Storage
{
    public interface IStorageEngine : IDisposable
    {
        /// <summary>
        /// Stores a snapshot in the storage engine
        /// </summary>
        /// <param name="snapshot"></param>
        /// <exception cref="ArgumentNullException">Thrown when a null snapshot is provided</exception>
        void StoreSnapshot(Snapshot snapshot);

        IEnumerable<QueriedMetricResult> ExecuteQuery(Query query);
        IEnumerable<Metric> GetKnownMetrics();
        int GetSnapshotCount();
        long GetStoredSize();
    }
}
