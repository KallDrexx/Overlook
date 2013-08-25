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
using Overlook.Server.Web;

namespace Overlook.Server
{
    public class SystemTray : Form
    {
        private readonly NotifyIcon _trayIcon;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _processingTask;

        private readonly MenuItem _storageSizeMenuItem;

        public SystemTray()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var trayMenu = new ContextMenu();
            _storageSizeMenuItem = new MenuItem {Enabled = false};

            trayMenu.MenuItems.Add(new MenuItem
            {
                Text = "Running",
                Enabled = false
            });

            trayMenu.MenuItems.Add(_storageSizeMenuItem);
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

        private async void OnExit(object sender, EventArgs e)
        {
            const int maxSecondsToWaitForCancellation = 60;
            var cancellationTime = DateTime.Now;

            _cancellationTokenSource.Cancel();

            while (!_processingTask.IsCanceled && !_processingTask.IsCompleted && !_processingTask.IsFaulted)
            {
                if ((DateTime.Now - cancellationTime).TotalSeconds > maxSecondsToWaitForCancellation)
                    break;

                await Task.Delay(100);
            }

            _trayIcon.Dispose();
            Application.Exit();
        }

        private void ProcessMetricRequests()
        {
            var storageEngine = new SqliteStorageEngine(ApplicationSettings.DatabaseName);

            var size = storageEngine.GetStoredSize();
            Invoke((Action)(() => UpdateStorageSizeDisplay(size)));

            // Start the webserver
            var bootstrapper = new OverlookBootStrapper(storageEngine);
            var uri = new Uri("http://localhost:" + ApplicationSettings.WebInterfacePort);
            var webServer = new NancyHost(uri, bootstrapper);
            webServer.Start();

            var retrievers = new IMetricRetriever[]
            {
                new OpenProcessMetricRetriever(),
                new OpenHardwareMonitorMetricRetriever()
            };

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
                    
                    // Update displays
                    size = storageEngine.GetStoredSize();
                    Invoke((Action)(() => UpdateStorageSizeDisplay(size)));
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            webServer.Stop();
        }

        private void UpdateStorageSizeDisplay(long size)
        {
            const int divisor = 1024;

            var displayedSize = Convert.ToDecimal(size);
            var labels = new[] {"B", "KB", "MB", "GB"};
            var labelIndex = 0;
            var currentLabel = labels[labelIndex];
            while (displayedSize > divisor)
            {
                displayedSize = displayedSize/divisor;
                labelIndex++;
                currentLabel = labels[labelIndex];
            }

            var display = string.Format("Size: {0:0.00} {1}", displayedSize, currentLabel);
            _storageSizeMenuItem.Text = display;
        }
    }
}
