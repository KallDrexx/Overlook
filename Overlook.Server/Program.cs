using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Overlook.Server.MetricRetriever;

[assembly: InternalsVisibleTo("Overlook.Tests")]

namespace Overlook.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var retriever = new OpenHardwareMonitorMetricRetriever();
            var metrics = retriever.GetCurrentMetricValues().ToArray();
        }
    }
}
