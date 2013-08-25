using System;
using System.Data.SQLite;

namespace Overlook.Server.Storage.Sqlite
{
    internal static class DatabaseSchemaBuilder
    {
        public static void InitializeSchema(SQLiteConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            var commandsInOrder = new[]
            {
                CreateTablesCommand, 
                EnsureDateTimesStoredAsUnixTime
            };

            foreach (var commandText in commandsInOrder)
                using (var cmd = new SQLiteCommand(commandText, connection))
                    cmd.ExecuteNonQuery();
        }

        private const string CreateTablesCommand = 
            @"CREATE TABLE if not exists [MetricData]
                (
                    Id integer primary key,
                    Date integer,
                    MetricDevice text,
                    MetricCategory text,
                    MetricName text,
                    SuffixLabel text,
                    Value real
                );";

        /// <summary>
        /// Query to migrate date strings into unix timestamps at UTC.
        /// First versions stored dates as local time date stamps.  While the utc offset value
        /// won't be accurate if the original data spanned time zones or DST, since I'm the only
        /// one who has used this so far this should be fine, all new data should be utc epoch 
        /// timestamps
        /// </summary>
        private const string EnsureDateTimesStoredAsUnixTime =
            @"update MetricData 
                set Date = (strftime('%s', Date) + (strftime('%s', DateTime('now')) - strftime('%s', DateTime('now', 'localtime'))))
                where typeof(Date) <> 'integer';";
    }
}
