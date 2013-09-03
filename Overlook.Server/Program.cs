using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using NLog;
using Overlook.Common.Data;
using Overlook.Server.MetricRetriever;
using Overlook.Server.Storage.Sqlite;

[assembly: InternalsVisibleTo("Overlook.Tests")]
namespace Overlook.Server
{
    class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Application.ThreadException += ApplicationOnThreadException;

            Application.Run(new SystemTray());
        }

        private static void ApplicationOnThreadException(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
        {
            _logger.Fatal("Uncaught exception caught: {0}", threadExceptionEventArgs.Exception.GetType());
        }
    }
}
