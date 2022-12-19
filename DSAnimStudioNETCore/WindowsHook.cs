using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;

/* Author: Sekhat
 * 
 * License: Public Domain.
 * 
 * Usage:
 *
 * Inherit from this class, and override the WndProc function in your derived class, 
 * in which you handle your windows messages.
 * 
 * To start recieving the message, create an instance of your derived class, passing in the
 * window handle of the window you want to listen for messages for.
 * 
 * in XNA: this would be the Game.Window.Handle property
 * in Winforms Form.Handle property
 */

namespace WindowsHookExample
{
    public abstract class WindowsHook : IDisposable
    {
        IntPtr hHook;
        IntPtr hWnd;
        // Stored here to stop it from getting garbage collected
        Win32.WndProcDelegate wndProcDelegate;

        public WindowsHook(IntPtr hWnd)
        {
            this.hWnd = hWnd;

            wndProcDelegate = WndProcHook;

            CreateHook();
        }

        ~WindowsHook()
        {
            Dispose(false);
        }

        private void CreateHook()
        {

            uint threadId = Win32.GetWindowThreadProcessId(hWnd, IntPtr.Zero);

            hHook = Win32.SetWindowsHookEx(Win32.HookType.WH_CALLWNDPROC, wndProcDelegate, IntPtr.Zero, threadId);

        }

        private int WndProcHook(int nCode, IntPtr wParam, ref Win32.Message lParam)
        {
            if (nCode >= 0)
            {
                Win32.TranslateMessage(ref lParam); // You may want to remove this line, if you find your not quite getting the right messages through. This is here so that WM_CHAR is correctly called when a key is pressed.
                WndProc(ref lParam);
            }

            return Win32.CallNextHookEx(hHook, nCode, wParam, ref lParam);
        }

        protected abstract void WndProc(ref Win32.Message message);

        #region Interop Stuff
        // I say thankya to P/Invoke.net.
        // Contains all the Win32 functions I need to deal with
        protected static class Win32
        {
            public enum HookType : int
            {
                WH_JOURNALRECORD = 0,
                WH_JOURNALPLAYBACK = 1,
                WH_KEYBOARD = 2,
                WH_GETMESSAGE = 3,
                WH_CALLWNDPROC = 4,
                WH_CBT = 5,
                WH_SYSMSGFILTER = 6,
                WH_MOUSE = 7,
                WH_HARDWARE = 8,
                WH_DEBUG = 9,
                WH_SHELL = 10,
                WH_FOREGROUNDIDLE = 11,
                WH_CALLWNDPROCRET = 12,
                WH_KEYBOARD_LL = 13,
                WH_MOUSE_LL = 14
            }

            public struct Message
            {
                public IntPtr lparam;
                public IntPtr wparam;
                public uint msg;
                public IntPtr hWnd;
            }

            /// <summary>
            ///  Defines the windows proc delegate to pass into the windows hook
            /// </summary>                  
            public delegate int WndProcDelegate(int nCode, IntPtr wParam, ref Message m);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr SetWindowsHookEx(HookType hook, WndProcDelegate callback,
                IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref Message m);

            [DllImport("coredll.dll", SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string module);

            [DllImport("user32.dll", EntryPoint = "TranslateMessage")]
            public extern static bool TranslateMessage(ref Message m);

            [DllImport("user32.dll")]
            public extern static uint GetWindowThreadProcessId(IntPtr window, IntPtr module);
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed resources here
            }
            // Free unmanaged resources here
            if (hHook != IntPtr.Zero)
            {
                Win32.UnhookWindowsHookEx(hHook);
            }
        }

        #endregion
    }
}