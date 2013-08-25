using System;
using System.Configuration;

namespace Overlook.Server
{
    public static class ApplicationSettings
    {
        public static string DatabaseName { get { return GetSetting<string>("DatabaseFileName"); } }
        public static int SecondsBetweenSnapshots { get { return GetSetting<int>("SecondsBetweenSnapshots"); } }
        public static int WebInterfacePort { get { return GetSetting<int>("WebInterfacePort"); } }
        public static int SecondsBetweenSnapshotFlushes { get { return GetSetting<int>("SecondsBetweenSnapshotFlushes"); } }

        private static T GetSetting<T>(string settingName)
        {
            var value = ConfigurationManager.AppSettings[settingName];
            if (value == null)
                return default(T);

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
