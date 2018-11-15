using System;

namespace TAEDX
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new TAEDX())
            {
                if (args.Length > 0)
                    game.AutoLoadAnibnd = args[0];
                game.Run();
            }
        }
    }
#endif
}
