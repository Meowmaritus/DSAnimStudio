using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DarkSoulsScripting.Injection.Structures
{
    public class SafeMarshalledHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public readonly object Obj;

        public readonly int Size;
        public SafeMarshalledHandle(object obj) : base(true)
        {
            Size = Marshal.SizeOf(obj);
            this.Obj = obj;
            Alloc();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }

        private void Alloc()
        {
            SetHandle(Marshal.AllocHGlobal(Size));
            Marshal.StructureToPtr(Obj, handle, true);
        }
        public IntPtr GetHandle()
        {
            if (IsClosed | IsInvalid)
            {
                Alloc();
            }
            return handle;
        }
    }
}
