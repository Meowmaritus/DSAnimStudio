using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class ErrorLog
    {
        public static void LogError(string message)
        {
            Log("ERROR", message);
        }

        public static void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        private static object _lock_timeOfLastLog = new object();
        private static DateTime timeOfLastLog = DateTime.MinValue;

        public static void Log(string category, string message)
        {
            bool hasBeenAWhileSinceLastLog = false;
            var currentTime = DateTime.Now;
            lock (_lock_timeOfLastLog)
            {
                var elapsed = currentTime.Subtract(timeOfLastLog);
                hasBeenAWhileSinceLastLog = elapsed.TotalSeconds >= 5;

                timeOfLastLog = currentTime;
            }

            for (int i = 0; i < 10; i++)
            {
                bool succeeded = false;
                try
                {
                    File.AppendAllText("DSAnimStudio_Log.txt",
                      $"{(hasBeenAWhileSinceLastLog ? "\n\n\n\n\n" : "")}[{DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss")}][{category.ToUpper()}] " +
                      $"{message}\n");

                    succeeded = true;
                }
                catch (Exception ree)
                {

                }

                if (succeeded)
                    break;

                Thread.Sleep(100);
            }

        }

        private static object _lock_IgnoredErrors = new object();
        private static List<string> IgnoredErrors = new List<string>();

        public static bool HandleException(Exception ex, string messageBeforeColon, bool alwaysPushOnly = false, bool shortDescForNotif = true)
        {
            Log("ERROR", $"{messageBeforeColon}:\n{ex.ToString()}\n\n");

            var errorText = ex.ToString();
            bool isIgnored = false;
            lock (_lock_IgnoredErrors)
            {
                if (IgnoredErrors.Contains(errorText))
                    isIgnored = true;
            }


            if (isIgnored || alwaysPushOnly)
            {
                NotificationManager.PushNotification(shortDescForNotif ? messageBeforeColon + ". See details in DSAnimStudio_Log.txt" : $"{messageBeforeColon}:\n{ex.ToString()}", 0.5f, fadeDuration: 0.15f, color: NotificationManager.ColorError);
                return true;
            }
            else
            {

                var dlgBox = new ExceptionHandleForm();

                string file = Main.TAE_EDITOR.FileContainerName;
                string fileBackupPath = $"{file}.{(DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss").Replace(".", ""))}.errbak";
                dlgBox.InitializeForException(ex, messageBeforeColon, fileBackupPath);
                var result = dlgBox.ShowDialog();



                if (dlgBox.CreateBackup)
                {
                    if (file != null && File.Exists(file))
                    {
                        File.Copy(file, fileBackupPath, true);
                    }
                }

                if (result == System.Windows.Forms.DialogResult.Abort)
                {
                    return false;
                }
                else if (result == System.Windows.Forms.DialogResult.Ignore)
                {
                    if (!IgnoredErrors.Contains(errorText))
                        IgnoredErrors.Add(errorText);
                    return true;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
