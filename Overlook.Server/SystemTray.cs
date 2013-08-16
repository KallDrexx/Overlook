using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nancy.Hosting.Self;
using Overlook.Common.Data;
using Overlook.Server.MetricRetriever;
using Overlook.Server.Storage.Sqlite;

namespace Overlook.Server
{

    public class SystemTray : Form
    {
        private readonly NotifyIcon _trayIcon;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _processingTask;

        public SystemTray()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add(new MenuItem
            {
                Text = "Running",
                Enabled = false
            });

            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit", OnExit);

            _trayIcon = new NotifyIcon
            {
                Text = "Overlook Server",
                Icon = new Icon(SystemIcons.Application, 40, 40),
                ContextMenu = trayMenu,
                Visible = true
            };

            _processingTask = new Task(ProcessMetricRequests);
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Don't show the main form
            ShowInTaskbar = false;

            _processingTask.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _trayIcon.Dispose();
            
            base.Dispose(disposing);
        }

        private void OnExit(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
            _processingTask.Wait();
            Application.Exit();
        }

        private void ProcessMetricRequests()
        {
            // Start the webserver
            var uri = new Uri("http://localhost:" + ApplicationSettings.WebInterfacePort);
            var webServer = new NancyHost(uri);
            webServer.Start();

            var retrievers = new IMetricRetriever[]
            {
                new OpenProcessMetricRetriever(),
                new OpenHardwareMonitorMetricRetriever()
            };

            var storageEngine = new SqliteStorageEngine(ApplicationSettings.DatabaseName);

            var lastCheck = DateTime.MinValue;
            var secondsBetweenChecks = ApplicationSettings.SecondsBetweenSnapshots;
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
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

            webServer.Stop();
        }
    }
}
