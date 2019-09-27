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
            ARGS = args;
            //ARGS = new string[] { @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\chr\c4100_bak-chrbnd\chr\c4100\c4100.flver" };

            try
            {
                MainInstance = new Main();

                MainInstance.Run(Microsoft.Xna.Framework.GameRunBehavior.Synchronous);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error occurred before DS Anim Studio had a chance to initialize (please report):\n\n{ex.ToString()}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MainInstance?.Dispose();
            }
        }
    }

}
