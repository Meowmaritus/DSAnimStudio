using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DarkSoulsScripting.Injection
{
    internal class PSAPI
    {
        [DllImport("psapi.dll", SetLastError = true)]
        internal static extern bool EnumProcessModules(
            IntPtr hProcess,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)]
            [In()] [Out()] uint[] lphModule,
            uint cb,
            [MarshalAs(UnmanagedType.U4)] ref uint lpcbNeeded);

        [DllImport("psapi.dll")]
        internal static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, uint nSize);
    }
}
