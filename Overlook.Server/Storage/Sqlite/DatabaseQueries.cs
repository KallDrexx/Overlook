using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
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
                                                                      DateTime startDate, DateTime endDate,
                                                                      QueryResolution resolution)
        {
            const string fromClause = @"from MetricData ";
            const string orderByClause = @"order by Date ";
            const string whereClause = @"where MetricDevice = @device 
                    and MetricCategory = @category
                    and MetricName = @name
                    and Date between @start and @end ";

            // Different select clauses for the different resolutions
            const string allSelectClause = @"select Date, Value ";
            const string minuteSelectClause =
                @"select strftime('%Y',Date) as 'year', strftime('%m', Date) as 'month', strftime('%d', Date) as 'day',
                            strftime('%H', Date) as 'hour', strftime('%M', Date) as 'minute', avg(Value) as 'Value' ";

            const string minuteGroupByClause =
                @"group by strftime('%Y-%m-%dT%H:%M:00.000', Date) ";

            if (connection == null)
                throw new ArgumentNullException("connection");

            if (metric == null)
                throw new ArgumentNullException("metric");

            // Impossible to have values when the start date is after the end date, so don't even try
            if (endDate < startDate)
                return new QueriedMetricResult {Metric = metric, Values = new KeyValuePair<DateTime, decimal>[0]};

            // Build the query
            var query = string.Empty;
            var groupBy = string.Empty;
            switch (resolution)
            {
                case QueryResolution.All:
                    query = allSelectClause;
                    break;

                case QueryResolution.Minute:
                    query = minuteSelectClause;
                    groupBy = minuteGroupByClause;
                    break;
            }

            query = string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}", Environment.NewLine, query, fromClause, whereClause,
                                  groupBy, orderByClause);

            var values = new List<KeyValuePair<DateTime, decimal>>();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@device", metric.Device);
                command.Parameters.AddWithValue("@category", metric.Category);
                command.Parameters.AddWithValue("@name", metric.Name);
                command.Parameters.AddWithValue("@start", startDate);
                command.Parameters.AddWithValue("@end", endDate);

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

        private static void ReadQueryResult(QueryResolution resolution, SQLiteDataReader reader, List<KeyValuePair<DateTime, decimal>> values)
        {
            switch (resolution)
            {
                case QueryResolution.All:
                    var date = reader.GetDateTime(0);
                    var value = reader.GetDecimal(1);
                    values.Add(new KeyValuePair<DateTime, decimal>(date, value));
                    break;

                case QueryResolution.Minute:
                    var columnNames = Enumerable.Range(0, reader.FieldCount)
                                                .Select(reader.GetName)
                                                .ToArray();

                    string rawYear;
                    string rawMonth = string.Empty;
                    string rawDay = string.Empty;
                    string rawHour = string.Empty;
                    string rawMinute = string.Empty;

                    if (!columnNames.Any(x => x.Equals("year", StringComparison.OrdinalIgnoreCase)))
                        throw new InvalidOperationException("No year column returned even though required");

                    rawYear = reader.GetString(reader.GetOrdinal("year"));

                    if (columnNames.Any(x => x.Equals("month", StringComparison.OrdinalIgnoreCase)))
                        rawMonth = reader.GetString(reader.GetOrdinal("month"));

                    if (columnNames.Any(x => x.Equals("day", StringComparison.OrdinalIgnoreCase)))
                        rawDay = reader.GetString(reader.GetOrdinal("day"));

                    if (columnNames.Any(x => x.Equals("hour", StringComparison.OrdinalIgnoreCase)))
                        rawHour = reader.GetString(reader.GetOrdinal("hour"));

                    if (columnNames.Any(x => x.Equals("minute", StringComparison.OrdinalIgnoreCase)))
                        rawMinute = reader.GetString(reader.GetOrdinal("minute"));

                    int year, month, day, hour, minute;
                    int.TryParse(rawYear, out year);
                    int.TryParse(rawMonth, out month);
                    int.TryParse(rawDay, out day);
                    int.TryParse(rawHour, out hour);
                    int.TryParse(rawMinute, out minute);

                    var resolutionDate = new DateTime(year, month, day, hour, minute, 0);
                    var resolutionValue = reader.GetDecimal(reader.GetOrdinal("Value"));
                    values.Add(new KeyValuePair<DateTime, decimal>(resolutionDate, resolutionValue));
                    break;
            }
        }
    }
}
