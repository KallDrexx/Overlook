﻿using System;
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

        public SqliteStorageEngine(string databaseName, bool deleteIfDataExists = false)
        {
            var connectionString = string.Format("Data Source={0};Version=3", databaseName);
            _db = new SQLiteConnection(connectionString);
            _db.Open();

            DatabaseSchemaBuilder.InitializeSchema(_db);

            if (deleteIfDataExists)
                DatabaseQueries.DeleteAllData(_db);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void StoreSnapshot(Snapshot snapshot)
        {
            DatabaseQueries.AddSnapshot(_db, snapshot);
        }

        public IEnumerable<QueriedMetricResult> ExecuteQuery(Query query)
        {
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
            lock (_sqliteEngineLock)
            {
                return DatabaseQueries.GetKnownMetrics(_db);
            }
        }

        public int GetSnapshotCount()
        {
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
    }
}
