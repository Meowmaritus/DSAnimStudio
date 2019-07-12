using System;
using System.Windows.Forms;

namespace DarkSoulsScripting.Extra
{
    public class Dbg
    {
        public enum DbgPrintType
        {
            Normal,
            Info,
            Warn,
            Err,
            ClearAll
        }

        public static event OnPrintEventHandler OnPrint;
        public delegate void OnPrintEventHandler(System.DateTime time, DbgPrintType type, string text);

        public static void Print(string text)
        {
            OnPrint?.Invoke(DateTime.Now, DbgPrintType.Normal, text);
        }

        public static void PrintClearAll()
        {
            OnPrint?.Invoke(DateTime.Now, DbgPrintType.ClearAll, "");
        }


        public static void PrintInfo(string text)
        {
            OnPrint?.Invoke(DateTime.Now, DbgPrintType.Info, text);
        }

        public static void PrintWarn(string text)
        {
            OnPrint?.Invoke(DateTime.Now, DbgPrintType.Warn, text);
        }

        public static void PrintErr(string text)
        {
            OnPrint?.Invoke(DateTime.Now, DbgPrintType.Err, text);
        }

        private static object lock_Dump = new object();

        private static string debugDumpFileName = "Debug_Dump.txt";
        //Public Shared Sub PrintArray(arr As Object())
        //    Console.WriteLine(arr.ToString() & "{" & String.Join(", ", arr.Select(Function(x) x.ToString)) & "}")
        //End Sub

        //Public Shared Sub PrintArray(arr As Integer())
        //    Console.WriteLine(arr.ToString() & "{" & String.Join(", ", arr.Select(Function(x) x.ToString)) & "}")
        //End Sub

        public static void Dump(string text)
        {
            lock (lock_Dump)
            {
                var time = System.DateTime.Now;
                var fullTimeStr = time.ToLongTimeString();
                var ampm = fullTimeStr.Substring(fullTimeStr.Length - 3);
                fullTimeStr = fullTimeStr.Substring(0, fullTimeStr.Length - 3);
                var ms = time.Millisecond.ToString("000");

                text = "[" + fullTimeStr + "." + ms + ampm + "] " + text;
                if (!System.IO.File.Exists(debugDumpFileName))
                {
                    using (var newFile = System.IO.File.CreateText(debugDumpFileName))
                    {
                        newFile.WriteLine(text);
                    }
                }
                else
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(debugDumpFileName, true))
                    {
                        sw.WriteLine(text);
                    }
                }
            }
        }

        public static bool? PopupErr(string msg)
        {
            return Popup(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool? PopupErrQue(string msg)
        {
            return Popup(msg, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        }

        public static bool? PopupOk(string msg, string title = "(Untitled)")
        {
            return Popup(msg, title);
        }

        public static bool? Popup(string msg, string title = "(Untitled)", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            var result = MessageBox.Show(msg, title, buttons, icon);
            if (result == DialogResult.Yes || result == DialogResult.OK)
            {
                return true;
            }
            else if (result == DialogResult.No)
            {
                return false;
            }
            else
            {
                return null;
            }
        }

    }
}
