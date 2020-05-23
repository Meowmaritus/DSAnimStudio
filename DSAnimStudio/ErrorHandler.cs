using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class ErrorHandler
    {
        public static void Handle(Exception ex, string messageBeforeColon)
        {
            try
            {
                File.AppendAllText("DSAnimStudio_ErrorLog.txt",
                  $"\n\n[{DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss")}] " +
                  $"{messageBeforeColon}:\n{ex.ToString()}\n");

                System.Windows.Forms.MessageBox.Show(
                   $"[Logged to DSAnimStudio_ErrorLog.txt]\n" +
                   $"{messageBeforeColon}:\n{ex.ToString()}",
                   "Fatal Error Encountered");
            }
            catch (Exception ree)
            {
                System.Windows.Forms.MessageBox.Show(
                    "[FAILED TO LOG ERROR TO FILE. ERROR LOG OF TRYING TO LOG ERROR:]\n" + 
                    ree.ToString() + "\n\n" + $"[ERROR IT WAS TRYING TO LOG:]\n" +
                    $"{messageBeforeColon}:\n{ex.ToString()}",
                    "Catastrophic Failure Encountered");
            }
        }
    }
}
