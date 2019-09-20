using System;

namespace DSAnimStudio
{

    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        public static string[] ARGS;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)

        {
            ARGS = args;
            //ARGS = new string[] { @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\chr\c4100_bak-chrbnd\chr\c4100\c4100.flver" };
            using (var game = new Main())
            {
                game.Run(Microsoft.Xna.Framework.GameRunBehavior.Synchronous);
            }
        }
    }

}
