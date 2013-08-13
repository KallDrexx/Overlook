using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Overlook.Common.Data;

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
                        command.Parameters.AddWithValue("@date", snapshot.Date);
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
                                                                      DateTime startDate, DateTime endDate)
        {
            const string query =
                @"select Date, Value from MetricData 
                    where MetricDevice = @device 
                    and MetricCategory = @category
                    and MetricName = @name
                    and Date between @start and @end";

            if (connection == null)
                throw new ArgumentNullException("connection");

            if (metric == null)
                throw new ArgumentNullException("metric");

            // Impossible to have values when the start date is after the end date, so don't even try
            if (endDate < startDate)
                return new QueriedMetricResult {Metric = metric, Values = new KeyValuePair<DateTime, decimal>[0]};

            var values = new List<KeyValuePair<DateTime, decimal>>();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@device", metric.Device);
                command.Parameters.AddWithValue("@category", metric.Category);
                command.Parameters.AddWithValue("@name", metric.Name);
                command.Parameters.AddWithValue("@start", startDate);
                command.Parameters.AddWithValue("@end", endDate);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var date = reader.GetDateTime(0);
                        var value = reader.GetDecimal(1);
                        values.Add(new KeyValuePair<DateTime, decimal>(date, value));
                    }
                }
            }

            return new QueriedMetricResult
            {
                Metric = metric,
                Values = values.ToArray()
            };
        }
    }
}
