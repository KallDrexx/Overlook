using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32.TaskScheduler;

namespace Overlook.Server.Ui
{
    public static class ServerStartupManager
    {
        private const string TaskName = "Overlook Server";

        public static bool WillLaunchOnStartup()
        {
            var lastTaskName = ApplicationSettings.LaunchOnLoginTask;
            if (string.IsNullOrWhiteSpace(lastTaskName))
                return false;

            // Check if the task is still registered, if it isn't clear the task name
            // since it is no longer valid
            using (var taskService = new TaskService())
            {
                var task = taskService.RootFolder
                                      .Tasks
                                      .FirstOrDefault(x => x.Name == lastTaskName);

                if (task == null)
                {
                    ApplicationSettings.LaunchOnLoginTask = null;
                    return false;
                }

                return true;
            }
        }

        public static void DisableLaunchOnStartup()
        {
            using (var taskService = new TaskService())
            {
                var lastTaskName = ApplicationSettings.LaunchOnLoginTask;
                if (!string.IsNullOrWhiteSpace(lastTaskName))
                    taskService.RootFolder.DeleteTask(lastTaskName);

                ApplicationSettings.LaunchOnLoginTask = null;
            }
        }

        public static void EnableLaunchOnStartup()
        {
            using (var taskService = new TaskService())
            {
                var task = taskService.NewTask();
                task.RegistrationInfo.Description = "Starts Overlook Server On Login";
                task.Triggers.Add(new LogonTrigger());
                task.Actions.Add(new ExecAction(Application.ExecutablePath));
                task.Principal.RunLevel = TaskRunLevel.Highest;
                taskService.RootFolder.RegisterTaskDefinition(TaskName, task);

                ApplicationSettings.LaunchOnLoginTask = TaskName;
            }
        }
    }
}
