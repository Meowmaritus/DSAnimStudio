using System;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace DarkSoulsScripting.Injection.Structures
{
    public class SafeRemoteThreadHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public readonly SafeRemoteHandle Func;
        public SafeRemoteThreadHandle(SafeRemoteHandle func) : base(true)
        {
            this.Func = func;
            Hook.DARKSOULS.OnDetach += DARKSOULS_OnDetach;
            Alloc();
        }
        private void Alloc()
        {
            IntPtr dsHandle = Hook.DARKSOULS.GetHandle();
            uint funcHandle = (uint)Func.GetHandle();
            if (funcHandle < Hook.DARKSOULS.SafeBaseMemoryOffset)
            {
                return;
            }

            SetHandle((IntPtr)Kernel.CreateRemoteThread(dsHandle, 0, 0, funcHandle, 0, 0, 0));
        }
        public IntPtr GetHandle()
        {
            if (IsClosed || IsInvalid || handle.ToInt32() < Hook.DARKSOULS.SafeBaseMemoryOffset)
            {
                Alloc();
            }
            return handle;
        }
        private void DARKSOULS_OnDetach()
        {
            SetHandleAsInvalid();
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            Hook.DARKSOULS.OnDetach -= DARKSOULS_OnDetach;

            if (handle.ToInt32() < Hook.DARKSOULS.SafeBaseMemoryOffset)
            {
                return false;
            }

            return Kernel.CloseHandle(handle);
        }

    }
}
