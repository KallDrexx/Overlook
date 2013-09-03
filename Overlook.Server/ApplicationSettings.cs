using System;
using System.Configuration;

namespace Overlook.Server
{
    public static class ApplicationSettings
    {
        public static string DatabaseName { get { return GetSetting<string>("DatabaseFileName"); } }
        public static int SecondsBetweenSnapshots { get { return GetSetting<int>("SecondsBetweenSnapshots"); } }
        public static int WebInterfacePort { get { return GetSetting<int>("WebInterfacePort"); } }

        public static string LaunchOnLoginTask
        {
            get { return GetSetting<string>("LaunchOnStartupTask"); }
            set { SetSetting("LaunchOnStartupTask", value); }
        }

        private static T GetSetting<T>(string settingName)
        {
            var value = ConfigurationManager.AppSettings[settingName];
            if (value == null)
                return default(T);

            return (T)Convert.ChangeType(value, typeof(T));
        }

        private static void SetSetting(string settingName, string settingValue)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (ConfigurationManager.AppSettings[settingName] == null)
            {
                config.AppSettings.Settings.Add(settingName, settingValue);
            }
            else
            {
                config.AppSettings.Settings[settingName].Value = settingValue;
            }

            config.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
