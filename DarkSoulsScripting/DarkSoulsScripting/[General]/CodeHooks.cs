using DarkSoulsScripting.CodeHookTypes;
using Managed.X86;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsScripting
{
    public static class CodeHooks
    {
        public static Reg32Recorder TargetedChrPtrRecorder { get; private set; } = null;

        internal static void InitAll()
        {
            TargetedChrPtrRecorder = new Reg32Recorder(X86Register32.EAX, (IntPtr)0xFAE784, 8, true, new byte[] { 0x8B, 0x16, 0x8B, 0x82, 0xA0, 0x00, 0x00, 0x00 });
        }

        internal static void CleanupAll()
        {
            TargetedChrPtrRecorder?.Dispose();
        }
    }
}
