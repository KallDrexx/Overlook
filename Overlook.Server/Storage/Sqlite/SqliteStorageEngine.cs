using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Overlook.Common.Data;
using Overlook.Common.Queries;

namespace Overlook.Server.Storage.Sqlite
{
    public class SqliteStorageEngine : IStorageEngine
    {
        private readonly SQLiteConnection _db;

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

            foreach (var metric in query.Metrics)
            {
                yield return
                    DatabaseQueries.GetMetricValuesBetweenDates(_db, metric, query.StartDate, query.EndDate,
                                                                query.Resolution);
            }
        }

        public IEnumerable<Metric> GetKnownMetrics()
        {
            return DatabaseQueries.GetKnownMetrics(_db);
        }
    }
}
