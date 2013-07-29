using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Overlook.Common.Data;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace Overlook.Server.Storage
{
    public class SqliteStorageEngine : IStorageEngine
    {
        private readonly IDbConnection _db;

        public SqliteStorageEngine(string databaseName, bool deleteIfDataExists = false)
        {
            // Setup the connection
            var factory = new OrmLiteConnectionFactory(databaseName, SqliteDialect.Provider);
            _db = factory.OpenDbConnection();

            if (deleteIfDataExists)
            {
                _db.DropAndCreateTable<DbSnapshot>();
                _db.DropAndCreateTable<DbSnapshotMetric>();
            }
            else
            {
                _db.CreateTableIfNotExists<DbSnapshot>();
                _db.CreateTableIfNotExists<DbSnapshotMetric>();
            }
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void StoreSnapshot(Snapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException("snapshot");

            var dbSnapshot = new DbSnapshot { Date = snapshot.Date };
            _db.Insert(dbSnapshot);

            foreach (var metric in snapshot.Metrics)
            {
                _db.Insert(new DbSnapshotMetric
                {
                    MetricName = metric.MetricName,
                    Device = metric.Device,
                    Value = metric.Value,
                    SnapshotId = (int)_db.GetLastInsertId(),
                    Label = metric.Label
                });
            }
        }

        public Snapshot[] GetSnapshots(DateTime startSearchDate, DateTime endSearchDate)
        {
            if (endSearchDate <= startSearchDate)
                return new Snapshot[0];

            var dbSnapshots = _db.Select<DbSnapshot>()
                                    .Where(x => x.Date >= startSearchDate)
                                    .Where(x => x.Date <= endSearchDate)
                                    .OrderBy(x => x.Date)
                                    .ToArray();

            // Retrieve all the metrics for these snapshots
            var snapshotIds = dbSnapshots.Select(x => x.Id)
                                         .Distinct()
                                         .ToArray();

            var dbMetrics = _db.Select<DbSnapshotMetric>()
                               .Where(x => Sql.In(x.SnapshotId, snapshotIds))
                               .ToArray();

            var results = new List<Snapshot>();
            foreach (var snapshot in dbSnapshots)
            {
                var resultSnapshot = new Snapshot
                {
                    Date = snapshot.Date,
                    Metrics = dbMetrics.Where(x => x.SnapshotId == snapshot.Id)
                                       .Select(x => new Metric
                                       {
                                           Device = x.Device,
                                           Label = x.Label,
                                           MetricName = x.MetricName,
                                           Value = x.Value
                                       })
                                       .ToArray()
                };

                results.Add(resultSnapshot);
            }

            return results.ToArray();
        }

        #region Entity Types

        private class DbSnapshot
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [Index]
            public DateTime Date { get; set; }
        }

        private class DbSnapshotMetric
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [Index]
            public int SnapshotId { get; set; }
            public string Device { get; set; }
            public string MetricName { get; set; }
            public decimal Value { get; set; }
            public string Label { get; set; }
        }

        #endregion
    }
}
