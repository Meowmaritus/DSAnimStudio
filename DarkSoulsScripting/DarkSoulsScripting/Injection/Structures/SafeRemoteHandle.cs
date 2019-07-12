using System;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace DarkSoulsScripting.Injection.Structures
{
    public class SafeRemoteHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public readonly int Size;
        public SafeRemoteHandle(int size) : base(true)
        {
            this.Size = size;
            Hook.DARKSOULS.OnDetach += DARKSOULS_OnDetach;
            Alloc();
        }
        private void Alloc()
        {
            IntPtr dsHandle = Hook.DARKSOULS.GetHandle();
            IntPtr h = (IntPtr)Kernel.VirtualAllocEx(dsHandle, 0, Size, Kernel.MEM_COMMIT, Kernel.PAGE_EXECUTE_READWRITE);
            SetHandle(h);
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

            IntPtr dsHandle = Hook.DARKSOULS.GetHandle();

            return Kernel.VirtualFreeEx(dsHandle, (uint)handle, 0, Kernel.MEM_RELEASE);
        }
        public void MemPatch(byte[] src, int? srcIndex = null, int? destOffset = null, int? numBytes = null)
        {
            IntPtr dsHandle = Hook.DARKSOULS.GetHandle();
            if (dsHandle.ToInt32() < Hook.DARKSOULS.SafeBaseMemoryOffset)
            {
                return;
            }

            if ((destOffset ?? 0 + numBytes ?? src.Length) > Size)
            {
                throw new Exception("Bytes will not fit in allocated space.");
            }
            byte[] buf = new byte[numBytes ?? src.Length];
            Array.Copy(src, srcIndex ?? 0, buf, 0, numBytes ?? src.Length);
            Kernel.WriteProcessMemory_SAFE(dsHandle, (uint)(handle + (destOffset ?? 0)), buf, numBytes ?? src.Length, 0);
        }

        public void MemPatch(SafeMarshalledHandle src, int? destOffset = null, int? numBytes = null)
        {
            if (handle.ToInt32() < Hook.DARKSOULS.SafeBaseMemoryOffset)
            {
                return;
            }

            if ((destOffset ?? 0 + numBytes ?? src.Size) > Size)
            {
                throw new Exception("Bytes will not fit in allocated space.");
            }
            byte[] buf = new byte[numBytes ?? src.Size];

            IntPtr dsHandle = Hook.DARKSOULS.GetHandle();

            Kernel.WriteProcessMemory_SAFE(dsHandle, (uint)(handle + (destOffset ?? 0)), (uint)src.GetHandle(), numBytes ?? src.Size, 0);
        }

        public byte[] GetFuncReturnValue()
        {
            byte[] result = new byte[DSAsmCaller.INT32_SIZE];

            if (handle.ToInt32() < Hook.DARKSOULS.SafeBaseMemoryOffset)
            {
                return result;
            }

            IntPtr dsHandle = Hook.DARKSOULS.GetHandle();
            if (!Kernel.ReadProcessMemory_SAFE(dsHandle, (uint)handle + DSAsmCaller.FUNC_RETURN_ADDR_OFFSET, result, DSAsmCaller.INT32_SIZE, 0))
            {
                //Throw New Exception("Kernel.ReadProcessMemory Fail for SafeRemoteHandle.GetFuncReturnValue()")
            }
            return result;
        }
    }
}
