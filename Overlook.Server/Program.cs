using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Overlook.Common.Data;
using Overlook.Server.MetricRetriever;
using Overlook.Server.Storage.Sqlite;

[assembly: InternalsVisibleTo("Overlook.Tests")]

namespace Overlook.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var retrievers = new IMetricRetriever[]
            {
                new OpenProcessMetricRetriever(),
                new OpenHardwareMonitorMetricRetriever()
            };

            var storageEngine = new SqliteStorageEngine(ApplicationSettings.DatabaseName);

            var lastCheck = DateTime.MinValue;
            var secondsBetweenChecks = ApplicationSettings.SecondsBetweenSnapshots;
            while (true)
            {
                if ((DateTime.Now - lastCheck).TotalSeconds > secondsBetweenChecks)
                {
                    var snapshot = new Snapshot
                    {
                        Date = DateTime.Now,
                        MetricValues = retrievers.SelectMany(x => x.GetCurrentMetricValues())
                                                 .ToArray()
                    };

                    storageEngine.StoreSnapshot(snapshot);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
