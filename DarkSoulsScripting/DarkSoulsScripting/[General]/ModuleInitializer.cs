// This file can be modified in any way, with two exceptions. 1) The name of
// this class must be "ModuleInitializer". 2) This class must have a public or
// internal parameterless "Run" method that returns void. In addition to these
// modifications, this file may also be moved to any location, as long as it
// remains a part of its current project.

using System;
using System.Threading;
using System.Windows;

namespace DarkSoulsScripting
{
    internal static class ModuleInitializer
    {
        public static SafetyFinalizerHandler Finalizer = null;

        private static Thread CleanExitThread;
        private static EventWaitHandle CleanExitTrigger = new EventWaitHandle(false, EventResetMode.ManualReset);

        internal static void Run()
        {
            /*
                The kernel32 functions seem to completely block the game's draw thread.
                We can reduce interruptions to the game's drawing by executing the kernel calls as fast as possible.
                Therefore, the below line actually (very noticably) boosts the performance of both DarkSoulsScripting AND Dark Souls!
            */
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            if (Hook.DARKSOULS.TryAttachToDarkSouls(out string error))
            {
                Hook.Init();

                Finalizer = new SafetyFinalizerHandler();

                CleanExitThread = new Thread(new ThreadStart(DoCleanExitWait)) { IsBackground = true };
                CleanExitThread.Start();

                CodeHooks.InitAll();
            }
            else
            {
                //if (MessageBox.Show(error + "\n\nWould you like to proceed anyways?", "Failed to attach to Dark Souls", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                //{
                //    if (System.Windows.Application.Current != null) //WPF
                //        System.Windows.Application.Current.Shutdown();
                //    else if (System.Windows.Forms.Application.MessageLoop) //WinForms
                //        System.Windows.Forms.Application.Exit();
                //    else //Console application
                //        Environment.Exit(1);
                //    Application.Current.Shutdown(1);
                //    return;
                //}
                Console.Error.WriteLine(">>>>>>>>>>>>>>>> ERROR: " + error);
                Hook.Cleanup();
            }
        }

        private static void CleanExit()
        {
            CodeHooks.CleanupAll();
            Hook.Cleanup();
        }

        private static void DoCleanExitWait()
        {
            bool doCleanExit = false;

            try
            {
                do
                {
                    doCleanExit = CleanExitTrigger.WaitOne(5000);
                } while (!(doCleanExit));

                GC.KeepAlive(Finalizer);
            }
            catch
            {

            }
            finally
            {
                CleanExit();
            }
        }

        public class SafetyFinalizerHandler
        {
            ~SafetyFinalizerHandler()
            {
                CleanExit();
            }
        }

    }
}