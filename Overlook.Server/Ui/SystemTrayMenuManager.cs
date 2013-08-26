using System;
using System.Windows.Forms;

namespace Overlook.Server.Ui
{
    public class SystemTrayMenuManager
    {
        private readonly ContextMenu _contextMenu;
        private readonly MenuItem _statusDisplayMenuItem;
        private readonly MenuItem _sizeDisplayMenuItem;
        private readonly MenuItem _exitMenuItem;

        public SystemTrayMenuManager(NotifyIcon systemTrayIcon, int webPort)
        {
            if (systemTrayIcon == null)
                throw new ArgumentNullException("systemTrayIcon");

            _sizeDisplayMenuItem = new MenuItem { Enabled = false};
            _statusDisplayMenuItem = new MenuItem {Enabled = false};
            _exitMenuItem = new MenuItem {Text = "Exit"};
            _exitMenuItem.Click += ProcessExitRequest;

            _contextMenu = new ContextMenu(new[]
            {
                _statusDisplayMenuItem,
                _sizeDisplayMenuItem,
                new MenuItem {Text = "Port: " + webPort, Enabled = false}, 
                new MenuItem("-"), 
                _exitMenuItem
            });

            systemTrayIcon.ContextMenu = _contextMenu;
        }

        public event EventHandler ExitRequested;

        public void UpdateStatus(ServerStatus status, long storageSize, long numSnapshots)
        {
            switch (status)
            {
                case ServerStatus.Running:
                    _statusDisplayMenuItem.Text = "Running...";
                    _exitMenuItem.Enabled = true;

                    var averagePerSnapshot = storageSize/numSnapshots;
                    var sizeDisplay = string.Format("Size: {0} (avg {1} per snapshot)",
                                                    GetFriendlySizeString(storageSize),
                                                    GetFriendlySizeString(averagePerSnapshot));

                    _sizeDisplayMenuItem.Text = sizeDisplay;
                    break;

                case ServerStatus.Exiting:
                    _statusDisplayMenuItem.Text = "Exitting...";
                    _exitMenuItem.Enabled = false;
                    break;

                default:
                    _statusDisplayMenuItem.Text = "Status Unknown";
                    _exitMenuItem.Enabled = true;
                    break;
            }
        }

        private void ProcessExitRequest(object sender, EventArgs e)
        {
            UpdateStatus(ServerStatus.Exiting, 0, 0);

            if (ExitRequested != null)
                ExitRequested(sender, e);
        }

        private static string GetFriendlySizeString(long size)
        {
            const int divisor = 1024;

            var displayedSize = Convert.ToDecimal(size);
            var labels = new[] { "B", "KB", "MB", "GB" };
            var labelIndex = 0;
            var currentLabel = labels[labelIndex];
            while (displayedSize > divisor)
            {
                displayedSize = displayedSize / divisor;
                labelIndex++;
                currentLabel = labels[labelIndex];
            }

            return string.Format("{0:0.00} {1}", displayedSize, currentLabel);
        }
    }
}
