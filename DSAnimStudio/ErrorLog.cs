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

        public static void Log(string category, string message)
        {
            for (int i = 0; i < 10; i++)
            {
                bool succeeded = false;
                try
                {
                    File.AppendAllText("DSAnimStudio_Log.txt",
                      $"\n\n[{DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss")}][{category.ToUpper()}] " +
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

        public static void HandleException(Exception ex, string messageBeforeColon)
        {
            Log("ERROR", $"{messageBeforeColon}:\n{ex.ToString()}");

            System.Windows.Forms.MessageBox.Show(
               $"[Logged to DSAnimStudio_ErrorLog.txt]\n" +
               $"{messageBeforeColon}:\n{ex.ToString()}",
               "Fatal Error Encountered");
        }
    }
}
