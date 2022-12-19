using System;
using System.Windows.Forms;

namespace DSAnimStudio
{

    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        public static string[] ARGS;
        public static Main MainInstance;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {


            SoulsFormats.DCX.LoadOodleAction = () =>
            {
                MessageBox.Show("To load Sekiro or Elden Ring files, you need to give DS Anim Studio access to the 'oo2core_6_win64.dll' file bundled next to the EXE of either game. Click OK to browse to this file now.");

                var browseDlg = new OpenFileDialog()
                {
                    FileName = "",
                    CheckFileExists = false,
                    CheckPathExists = true,
                    Title = "Select Oodle DLL",
                    Filter = "DLLs (*.dll)|*.dll"
                };
                browseDlg.FileName = "oo2core_6_win64.dll";
                if (browseDlg.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.Copy(browseDlg.FileName, DSAnimStudio.Main.Directory + "\\oo2core_6_win64.dll", true);
                }
            };

            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            

            ARGS = args;
            //ARGS = new string[] { @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\chr\c4100_bak-chrbnd\chr\c4100\c4100.flver" };

#if !DEBUG
            try
            {
#endif
                MainInstance = new Main();

#if !DEBUG
        }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error occurred before DS Anim Studio had a chance to initialize (please report):\n\n{ex.ToString()}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif

            try
            {
                MainInstance.Run(Microsoft.Xna.Framework.GameRunBehavior.Synchronous);
            }
            finally
            {
                LiveRefresh.Memory.CloseHandle();
                MainInstance?.Dispose();
            }
        }
    }

}
