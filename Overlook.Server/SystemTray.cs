using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using Nancy.Hosting.Self;
using Overlook.Common.Data;
using Overlook.Server.MetricRetriever;
using Overlook.Server.Storage;
using Overlook.Server.Storage.Sqlite;
using Overlook.Server.Ui;
using Overlook.Server.Web;
using Timer = System.Windows.Forms.Timer;

namespace Overlook.Server
{
    public class SystemTray : Form
    {
        private readonly NotifyIcon _trayIcon;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _processingTask;
        private readonly SystemTrayMenuManager _systemTrayMenuManager;
        private readonly int _webPortNumber;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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

            var errorTicker = new Timer { Interval = 1000 };
            errorTicker.Tick += ErrorTickerOnTick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _trayIcon.Dispose();
            
            base.Dispose(disposing);
        }

        private void ErrorTickerOnTick(object sender, EventArgs eventArgs)
        {
            if (_processingTask.Exception != null)
            {
                _logger.Fatal("{0} exception occurred in metric processing task", _processingTask.Exception.InnerException.GetType());

                var message = string.Format("An {0} exception occurred in the metric processing task",
                                            _processingTask.Exception.InnerException.GetType());
                MessageBox.Show(message, "Overlook Server Error Occurred");

                OnExit(null, null);
            }
        }

        private async void OnExit(object sender, EventArgs e)
        {
            const int maxSecondsToWaitForCancellation = 60;
            var cancellationTime = DateTime.Now;

            _logger.Debug("Cancellation requested");
            _cancellationTokenSource.Cancel();

            while (!_processingTask.IsCanceled && !_processingTask.IsCompleted && !_processingTask.IsFaulted)
            {
                if ((DateTime.Now - cancellationTime).TotalSeconds > maxSecondsToWaitForCancellation)
                    break;

                await Task.Delay(100);
            }

            _logger.Debug("Task cancelled or waiting period expired");
            _trayIcon.Dispose();
            Application.Exit();
        }

        private void ProcessMetricRequests()
        {
            _logger.Debug("Beginning to process metric requests");

            var storageEngine = new SqliteStorageEngine(ApplicationSettings.DatabaseName);

            // Start the webserver
            _logger.Debug("Beginning webserver on port {0}", _webPortNumber);
            var bootstrapper = new OverlookBootStrapper(storageEngine);
            var uri = new Uri("http://localhost:" + _webPortNumber);
            var webServer = new NancyHost(uri, bootstrapper);
            webServer.Start();
            _logger.Debug("Web server started");

            var retrievers = new IMetricRetriever[]
            {
                new OpenProcessMetricRetriever(),
                new OpenHardwareMonitorMetricRetriever()
            };

            UpdateDisplays(storageEngine);

            var lastSnapshotTime = DateTime.MinValue;
            var secondsBetweenChecks = ApplicationSettings.SecondsBetweenSnapshots;
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if ((DateTime.Now - lastSnapshotTime).TotalSeconds > secondsBetweenChecks)
                {
                    _logger.Debug("Generating snapshot");

                    var snapshot = new Snapshot
                    {
                        Date = DateTime.Now,
                        MetricValues = retrievers.SelectMany(x => x.GetCurrentMetricValues())
                                                 .ToArray()
                    };

                    _logger.Debug("Storing Snapshot");
                    storageEngine.StoreSnapshot(snapshot);
                    _logger.Debug("Snapshot stored");

                    UpdateDisplays(storageEngine);
                    lastSnapshotTime = DateTime.Now;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            _logger.Debug("Stopping webserver");
            webServer.Stop();
            _logger.Debug("Webserver stopped");
        }

        private void UpdateDisplays(IStorageEngine storageEngine)
        {
            _logger.Debug("Updating storage engine displays");

            var size = storageEngine.GetStoredSize();
            var snapshotCount = storageEngine.GetSnapshotCount();

            Invoke((Action) (() =>
            {
                _logger.Debug("Updating Status");
                _systemTrayMenuManager.UpdateStatus(ServerStatus.Running, size, snapshotCount);
                _logger.Debug("Status updated");
            }));

            _logger.Debug("Storage engine displays updated");
        }
    }
}
