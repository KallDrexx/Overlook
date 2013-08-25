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
                CreateTablesCommand
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
    }
}
