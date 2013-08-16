using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
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
            Application.Run(new SystemTray());
        }
    }
}
