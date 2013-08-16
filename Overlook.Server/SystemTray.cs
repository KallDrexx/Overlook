using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Overlook.Common.Data;
using Overlook.Server.MetricRetriever;
using Overlook.Server.Storage.Sqlite;

namespace Overlook.Server
{

    public class SystemTray : Form
    {
        private readonly NotifyIcon _trayIcon;
        private readonly Task _processingTask;

        public SystemTray()
        {
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

            _processingTask = ProcessMetricRequests();
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
            Application.Exit();
        }

        private async Task ProcessMetricRequests()
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
                    await Task.Delay(100);
                }
            }
        }
    }
}
