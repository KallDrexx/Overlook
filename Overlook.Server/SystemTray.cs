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
using Overlook.Server.Ui;
using Overlook.Server.Web;

namespace Overlook.Server
{
    public class SystemTray : Form
    {
        private readonly NotifyIcon _trayIcon;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _processingTask;
        private readonly SystemTrayMenuManager _systemTrayMenuManager;
        private readonly int _webPortNumber;

        public SystemTray()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _trayIcon = new NotifyIcon
            {
                Text = "Overlook Server",
                Icon = new Icon(SystemIcons.Application, 40, 40),
                Visible = true
            };

            _webPortNumber = ApplicationSettings.WebInterfacePort;
            _systemTrayMenuManager = new SystemTrayMenuManager(_trayIcon, _webPortNumber);
            _systemTrayMenuManager.ExitRequested += OnExit;

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
            UpdateDisplays(storageEngine);

            // Start the webserver
            var bootstrapper = new OverlookBootStrapper(storageEngine);
            var uri = new Uri("http://localhost:" + _webPortNumber);
            var webServer = new NancyHost(uri, bootstrapper);
            webServer.Start();

            var retrievers = new IMetricRetriever[]
            {
                new OpenProcessMetricRetriever(),
                new OpenHardwareMonitorMetricRetriever()
            };

            var lastSnapshotTime = DateTime.MinValue;
            var secondsBetweenChecks = ApplicationSettings.SecondsBetweenSnapshots;
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if ((DateTime.Now - lastSnapshotTime).TotalSeconds > secondsBetweenChecks)
                {
                    var snapshot = new Snapshot
                    {
                        Date = DateTime.Now,
                        MetricValues = retrievers.SelectMany(x => x.GetCurrentMetricValues())
                                                 .ToArray()
                    };

                    storageEngine.StoreSnapshot(snapshot);
                    UpdateDisplays(storageEngine);

                    lastSnapshotTime = DateTime.Now;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            webServer.Stop();
        }

        private void UpdateDisplays(SqliteStorageEngine storageEngine)
        {
            var size = storageEngine.GetStoredSize();
            var snapshotCount = storageEngine.GetSnapshotCount();
            Invoke((Action) (() => _systemTrayMenuManager.UpdateStatus(ServerStatus.Running, size, snapshotCount)));
        }
    }
}
