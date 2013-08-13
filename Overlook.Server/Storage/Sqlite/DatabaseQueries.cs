using System;
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
    }
}
