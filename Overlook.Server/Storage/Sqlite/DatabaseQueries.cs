using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Overlook.Server.Extensions;
using Overlook.Common.Data;
using Overlook.Common.Queries;

namespace Overlook.Server.Storage.Sqlite
{
    internal static class DatabaseQueries
    {
        public static void DeleteAllData(SQLiteConnection connection)
        {
            const string deleteNonQuery = @"delete from MetricData;";

            if (connection == null)
                throw new ArgumentNullException("connection");

            using (var command = new SQLiteCommand(deleteNonQuery, connection))
                command.ExecuteNonQuery();
        }

        public static void AddSnapshot(SQLiteConnection connection, Snapshot snapshot)
        {
            const string insertNonQuery =
                @"insert into MetricData (Date, MetricDevice, MetricCategory, MetricName, SuffixLabel, Value)
                  Values (@date, @device, @category, @name, @suffix, @value);";

            if (connection == null)
                throw new ArgumentNullException("connection");

            if (snapshot == null)
                throw new ArgumentNullException("snapshot");

            if (snapshot.MetricValues != null)
            {
                foreach (var metricValuePair in snapshot.MetricValues)
                {
                    using (var command = new SQLiteCommand(insertNonQuery, connection))
                    {
                        command.Parameters.AddWithValue("@date", snapshot.Date.ToUtcUnixTimestamp());
                        command.Parameters.AddWithValue("@device", metricValuePair.Key.Device);
                        command.Parameters.AddWithValue("@category", metricValuePair.Key.Category);
                        command.Parameters.AddWithValue("@name", metricValuePair.Key.Name);
                        command.Parameters.AddWithValue("@suffix", metricValuePair.Key.SuffixLabel);
                        command.Parameters.AddWithValue("@value", metricValuePair.Value);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static QueriedMetricResult GetMetricValuesBetweenDates(SQLiteConnection connection, Metric metric,
                                                                      DateTime startDate, DateTime endDate,
                                                                      QueryResolution resolution)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            if (metric == null)
                throw new ArgumentNullException("metric");

            // Impossible to have values when the start date is after the end date, so don't even try
            if (endDate < startDate)
                return new QueriedMetricResult { Metric = metric, Values = new KeyValuePair<DateTime, decimal>[0] };

            var query = GenerateQueryAtResolution(resolution);
            var values = new List<KeyValuePair<DateTime, decimal>>();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@device", metric.Device);
                command.Parameters.AddWithValue("@category", metric.Category);
                command.Parameters.AddWithValue("@name", metric.Name);
                command.Parameters.AddWithValue("@start", startDate.ToUtcUnixTimestamp());
                command.Parameters.AddWithValue("@end", endDate.ToUtcUnixTimestamp());

                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                        ReadQueryResult(resolution, reader, values);
            }

            return new QueriedMetricResult
            {
                Metric = metric,
                Values = values.ToArray()
            };
        }

        public static IEnumerable<Metric> GetKnownMetrics(SQLiteConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            const string query =
                @"select distinct MetricDevice, MetricCategory, MetricName, SuffixLabel from MetricData
                    order by MetricDevice, MetricCategory, MetricName";

            using (var command = new SQLiteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var device = reader.GetString(0);
                        var category = reader.GetString(1);
                        var name = reader.GetString(2);
                        var suffix = reader.GetString(3);

                        yield return new Metric(device, category, name, suffix);
                    }
                }
            }
        }

        public static int GetSnapshotCounts(SQLiteConnection connection)
        {
            const string query = @"select count(*) from (select distinct Date from MetricData)";

            if (connection == null)
                throw new ArgumentNullException("connection");

            using (var command = new SQLiteCommand(query, connection))
                return Convert.ToInt32(command.ExecuteScalar());
        }

        public static long GetPageSize(SQLiteConnection connection)
        {
            const string query = @"pragma page_size;";

            if (connection == null)
                throw new ArgumentNullException("connection");

            using (var command = new SQLiteCommand(query, connection))
                return Convert.ToInt64(command.ExecuteScalar());
        }

        public static long GetPageCount(SQLiteConnection connection)
        {
            const string query = @"pragma page_count;";

            if (connection == null)
                throw new ArgumentNullException("connection");

            using (var command = new SQLiteCommand(query, connection))
                return Convert.ToInt64(command.ExecuteScalar());
        }

        private static string GenerateQueryAtResolution(QueryResolution resolution)
        {
            const string fromClause = @"from MetricData ";
            const string orderByClause = @"order by Date ";
            const string whereClause = @"where MetricDevice = @device 
                    and MetricCategory = @category
                    and MetricName = @name
                    and Date between @start and @end ";

            // Different select clauses for the different resolutions
            const string allSelectClause = @"select strftime('%Y-%m-%dT%H:%M:%S', datetime(Date, 'unixepoch', 'localtime')) as 'Date', Value ";
            const string resolutionSelectClause = @"select {0} as 'GroupedDate', avg(Value) as 'Value' ";
            const string resolutionGroupByClause = @"group by {0} ";

            var resolutionDateParts = new Dictionary<QueryResolution, string>
            {
                {QueryResolution.Minute, "strftime('%Y-%m-%dT%H:%M:00.000', datetime(Date, 'unixepoch', 'localtime'))"},
                {QueryResolution.Hour, "strftime('%Y-%m-%dT%H:00:00.000', datetime(Date, 'unixepoch', 'localtime'))"},
                {QueryResolution.Day, "strftime('%Y-%m-%dT00:00:00.000', datetime(Date, 'unixepoch', 'localtime'))"},
                {QueryResolution.Month,"strftime('%Y-%m-01T00:00:00.000', datetime(Date, 'unixepoch', 'localtime'))" },
                {QueryResolution.Year,"strftime('%Y-01-01T00:00:00.000', datetime(Date, 'unixepoch', 'localtime'))" },
                {QueryResolution.FifteenMinutes, "strftime('%Y-%m-%dT%H:', datetime(Date, 'unixepoch', 'localtime')) || ((strftime('%M', datetime(Date, 'unixepoch', 'localtime'))/15) * 15)"},
                {QueryResolution.TenMinutes, "strftime('%Y-%m-%dT%H:', datetime(Date, 'unixepoch', 'localtime')) || ((strftime('%M', datetime(Date, 'unixepoch', 'localtime'))/10) * 10)"},
                {QueryResolution.HalfHour, "strftime('%Y-%m-%dT%H:', datetime(Date, 'unixepoch', 'localtime')) || ((strftime('%M', datetime(Date, 'unixepoch', 'localtime'))/30) * 30)"},
            };

            // Build the query
            string query;
            string groupBy;
            switch (resolution)
            {
                case QueryResolution.All:
                    query = allSelectClause;
                    groupBy = string.Empty;
                    break;

                default:
                    var datePart = resolutionDateParts[resolution];
                    query = string.Format(resolutionSelectClause, datePart);
                    groupBy = string.Format(resolutionGroupByClause, datePart);
                    break;
            }

            return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}", 
                Environment.NewLine, query, fromClause, whereClause, groupBy, orderByClause);
        }

        private static void ReadQueryResult(QueryResolution resolution, SQLiteDataReader reader, List<KeyValuePair<DateTime, decimal>> values)
        {
            switch (resolution)
            {
                case QueryResolution.All:
                    var date = reader.GetDateTime(0);
                    var value = reader.GetDecimal(1);
                    values.Add(new KeyValuePair<DateTime, decimal>(date, value));
                    break;

                default:
                    var rawDate = Convert.ToString(reader["GroupedDate"]);
                    
                    // Partial time resolutions will cause the raw date to have a minute value
                    // of :0 instead of :00, which will cause parsing errors, so try and fix that
                    if (rawDate.EndsWith(":0"))
                        rawDate += "0";

                    var resolutionDate = DateTime.Parse(rawDate);
                    var resolutionValue = reader.GetDecimal(reader.GetOrdinal("Value"));
                    values.Add(new KeyValuePair<DateTime, decimal>(resolutionDate, resolutionValue));
                    break;
            }
        }
    }
}
