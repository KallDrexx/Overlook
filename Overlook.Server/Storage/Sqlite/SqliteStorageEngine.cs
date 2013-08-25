using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using Overlook.Common.Data;
using Overlook.Common.Queries;

namespace Overlook.Server.Storage.Sqlite
{
    public class SqliteStorageEngine : IStorageEngine
    {
        private readonly SQLiteConnection _db;
        private readonly object _sqliteEngineLock = new object();
        private readonly ConcurrentQueue<Snapshot> _writeQueue;
        private DateTime _lastSnapshotFlushTime;

        public SqliteStorageEngine(string databaseName, bool deleteIfDataExists = false)
        {
            _writeQueue = new ConcurrentQueue<Snapshot>();
            _lastSnapshotFlushTime = DateTime.MinValue;

            var connectionString = string.Format("Data Source={0};Version=3", databaseName);
            _db = new SQLiteConnection(connectionString);
            _db.Open();

            DatabaseSchemaBuilder.InitializeSchema(_db);

            if (deleteIfDataExists)
                DatabaseQueries.DeleteAllData(_db);
        }

        public void Dispose()
        {
            FlushUnsavedSnapshots();

            _db.Dispose();
        }

        public void StoreSnapshot(Snapshot snapshot)
        {
            _writeQueue.Enqueue(snapshot);

            // Check if it's time to flush snapshots
            if ((DateTime.Now - _lastSnapshotFlushTime).TotalSeconds >= ApplicationSettings.SecondsBetweenSnapshotFlushes)
                FlushUnsavedSnapshots();
        }

        public IEnumerable<QueriedMetricResult> ExecuteQuery(Query query)
        {
            FlushUnsavedSnapshots();

            if (query.Metrics == null)
                yield break;

            lock(_sqliteEngineLock)
            {
                foreach (var metric in query.Metrics)
                {
                    yield return
                        DatabaseQueries.GetMetricValuesBetweenDates(_db, metric, query.StartDate, query.EndDate,
                                                                    query.Resolution);
                }
            }
        }

        public IEnumerable<Metric> GetKnownMetrics()
        {
            FlushUnsavedSnapshots();

            lock (_sqliteEngineLock)
            {
                return DatabaseQueries.GetKnownMetrics(_db);
            }
        }

        public int GetSnapshotCount()
        {
            FlushUnsavedSnapshots();

            lock (_sqliteEngineLock)
            {
                return DatabaseQueries.GetSnapshotCounts(_db);
            }
        }

        public long GetStoredSize()
        {
            lock (_sqliteEngineLock)
            {
                var pageSize = DatabaseQueries.GetPageSize(_db);
                var pageCount = DatabaseQueries.GetPageCount(_db);

                return pageSize * pageCount;
            }
        }

        private void FlushUnsavedSnapshots()
        {
            lock (_sqliteEngineLock)
            {
                Snapshot snapshot;
                while (_writeQueue.TryDequeue(out snapshot))
                    DatabaseQueries.AddSnapshot(_db, snapshot);
            }

            _lastSnapshotFlushTime = DateTime.Now;
        }
    }
}
