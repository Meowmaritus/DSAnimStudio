using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSAnimStudio.LiveRefresh;

namespace DSAnimStudio
{
    public static class EldenRingSoundInject
    {
        static IntPtr processHandle = IntPtr.Zero;

        static string BytesToHexStr(byte[] input)
        {
            return string.Join(" ", input.Select(i => i.ToString("X2")));
        }

        const int PlaySoundFunc = 0x5EC380;
        static IntPtr PanicCodeOffset = (IntPtr)0x1E18009;

        static readonly byte[] PanicCodeBytes = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };

        static object _lock_All = new object();

        static byte[] codeBytes = new byte[] {
                        0x48, 0xBA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rdx, <chrID_Addr_Hex>
                        0x41, 0xB9, 0x00, 0x00, 0x00, 0x00, // mov r9d,<soundID_Hex>
                        0x41, 0xB8, 0x00, 0x00, 0x00, 0x00, // mov r8d,<soundType_Hex>
                        0x48, 0x83, 0xEC, 0x28, // sub rsp, 0x28
                        0x49, 0xBE, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov r14, <func_Addr_Hex>
                        0x41, 0xFF, 0xD6, // call r14
                        0x48, 0x83, 0xC4, 0x28, // add rsp, 0x28
                        0xC3, // ret
                    };

        public static void PlaySound(int chrID, int soundType, int soundID)
        {
            return;

            lock (_lock_All)
            {
                IntPtr chrIDPtr = IntPtr.Zero;
                try
                {
                    if (Memory.ProcessHandle == IntPtr.Zero)
                        Memory.AttachProc("eldenring");

                    if (Memory.ProcessHandle == IntPtr.Zero)
                    {
                        Memory.AttachProc("start_protected_game");
                    }

                    if (Memory.ProcessHandle != IntPtr.Zero)
                    {
                        Memory.WriteBytes((IntPtr)(Memory.BaseAddress.ToInt64() + PanicCodeOffset.ToInt64()), PanicCodeBytes);

                        chrIDPtr = Kernel32.VirtualAllocEx(Memory.ProcessHandle, IntPtr.Zero, 4, 0x1000 | 0x2000, 0X40);
                        if (chrIDPtr != IntPtr.Zero)
                        {
                            if (Memory.WriteInt32(chrIDPtr, chrID))
                            {

                                Array.Copy(BitConverter.GetBytes(chrIDPtr.ToInt64()), 0, codeBytes, 0x02, 8);
                                Array.Copy(BitConverter.GetBytes(soundID), 0, codeBytes, 0x0C, 4);
                                Array.Copy(BitConverter.GetBytes(soundType), 0, codeBytes, 0x12, 4);
                                Array.Copy(BitConverter.GetBytes((Memory.BaseAddress + PlaySoundFunc).ToInt64()), 0, codeBytes, 0x1C, 8);

                                var buffer = 0x1000;

                                var address = Kernel32.VirtualAllocEx(Memory.ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);


                                if (address != IntPtr.Zero)
                                {
                                    try
                                    {
                                        if (Memory.WriteBytes(address, codeBytes))
                                        {
                                            var threadHandle = Kernel32.CreateRemoteThread(Memory.ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                                            if (threadHandle != IntPtr.Zero)
                                            {
                                                Kernel32.WaitForSingleObject(threadHandle, 30000);
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        Kernel32.VirtualFreeEx(Memory.ProcessHandle, address, buffer, 2);
                                    }

                                }



                            }
                        }
                    }
                }
                finally
                {
                    if (chrIDPtr != IntPtr.Zero)
                        Kernel32.VirtualFreeEx(Memory.ProcessHandle, chrIDPtr, 4, 2);

                    //Memory.CloseHandle();
                }
                

                
            }

        }
    }
}
