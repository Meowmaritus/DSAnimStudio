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

        public static bool HandleException(Exception ex, string messageBeforeColon)
        {
            Log("ERROR", $"{messageBeforeColon}:\n{ex.ToString()}");

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
            else
            {
                return true;
            }
        }
    }
}
