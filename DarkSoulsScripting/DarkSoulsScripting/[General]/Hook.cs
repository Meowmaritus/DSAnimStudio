using System;
using System.Linq;
using DarkSoulsScripting.Injection.Structures;
using System.Threading;
using DarkSoulsScripting.Injection;
using System.Collections.Generic;
using Managed.X86;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace DarkSoulsScripting
{
    public static class Hook
    {
        public static InjectedPointersContainer InjectedPointers = null;

        public class InjectedPointersContainer
        {
            public SafeRemoteHandle ItemDropPtr = new SafeRemoteHandle(1024);
        }

        public static SafeDarkSoulsHandle DARKSOULS { get; private set; } = new SafeDarkSoulsHandle();

        private class HookState
        {
            public Thread Thread;
            public DSAsmCaller Asm;
            public byte[] Buffer;

            public long WBit_actualAddress = 0;
            public int WBit_mask = 0;

            public long RBit_actualAddress = 0;
            public int RBit_mask = 0;
        }

        public static void CallArrayOfBytes(byte[] asmBytes)
        {
            ASM.CallArrayOfBytes(asmBytes);
        }

        public static void CallCustomX86(Action<X86Writer> writeX86)
        {
            ASM.CallCustomX86(writeX86);
        }

        public static T Call<T>(FuncAddress address, params dynamic[] args)
        {
            return Call<T>(((int)address, (int)address + 0x1590), args);
        }

        public static T CallReg<T>(FuncAddress address, dynamic[] args,
            dynamic eax = null,
            dynamic ecx = null,
            dynamic edx = null,
            dynamic ebx = null,
            dynamic esp = null,
            dynamic esi = null,
            dynamic edi = null)
        {
            return CallReg<T>(((int)address, (int)address + 0x1590), args, eax, ecx, edx, ebx, esp, esi, edi);
        }

        public static T Call<T>(Memloc address, params dynamic[] args)
        {
            return ASM.CallIngameFunc<T>(address, args);
        }

        public static T CallReg<T>(Memloc address, dynamic[] args,
            dynamic eax = null,
            dynamic ecx = null,
            dynamic edx = null,
            dynamic ebx = null,
            dynamic esp = null,
            dynamic esi = null,
            dynamic edi = null)
        {
            return ASM.CallIngameFuncReg<T>(address, args, eax, ecx, edx, ebx, esp, esi, edi);
        }

        private static HookState GetCurrentState()
        {
            if (States.ContainsKey(Thread.CurrentThread.ManagedThreadId))
            {
                States.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var result);
                return result;
            }
            else
            {
                var newEntry = new HookState()
                {
                    Thread = Thread.CurrentThread,
                    Buffer = new byte[8],
                    Asm = new DSAsmCaller()
                };
                States.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, x => newEntry, (k, v) => v);
                return newEntry;
            }
        }

        internal static DSAsmCaller ASM => GetCurrentState().Asm;
        private static byte[] ByteBuffer => GetCurrentState().Buffer;

        private static ConcurrentDictionary<int, HookState> States = 
            new ConcurrentDictionary<int, HookState>();

        private static Thread BufferCollectThread;

        private static EventWaitHandle BufferCollectLoopStopTrigger = new EventWaitHandle(false, EventResetMode.ManualReset);
        private static EventWaitHandle BufferCollectLoopStopTriggerCallback = new EventWaitHandle(false, EventResetMode.ManualReset);

        private static void BufferCollectLoop()
        {
            States.AsParallel().ForAll(x =>
            {
                if (!(x.Value.Thread?.IsAlive ?? false))
                {
                    if (States.TryRemove(x.Key, out var deadState))
                    {
                        deadState.Asm?.Dispose();
                    }
                    else
                    {
                        Console.Error.WriteLine("WARNING: ConcurrentDictionary<int, HookState> Hook.States --> Call to Hook.States.TryRemove(...) failed.");
                    }
                }
            });

            if (BufferCollectLoopStopTrigger.WaitOne(1000))
            {
                BufferCollectLoopStopTriggerCallback.Set();
            }
        }

        internal static void Cleanup()
        {
            DARKSOULS.Close();
            if (BufferCollectThread?.IsAlive ?? false)
            {
                BufferCollectLoopStopTrigger.Set();
                BufferCollectLoopStopTriggerCallback.WaitOne();
                foreach (var kvp in States)
                {
                    kvp.Value.Asm.Dispose();
                }
            }
        }

        internal static bool Init()
        {
            BufferCollectLoopStopTrigger.Reset();
            BufferCollectLoopStopTriggerCallback.Reset();

            if (!(BufferCollectThread?.IsAlive ?? false))
            {
                BufferCollectThread = new Thread(new ThreadStart(BufferCollectLoop)) { IsBackground = true };
                BufferCollectThread.Start();
            }

            return true;
        }

        public static sbyte RInt8(Memloc addr)
        {
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, ByteBuffer, 1, 0);
            return (sbyte)ByteBuffer[0];
        }

        public static short RInt16(Memloc addr)
        {
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, ByteBuffer, 2, 0);
            return BitConverter.ToInt16(ByteBuffer, 0);
        }


        public static int RInt32(Memloc addr)
        {
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, ByteBuffer, 4, 0);
            return BitConverter.ToInt32(ByteBuffer, 0);
        }


        public static long RInt64(Memloc addr)
        {
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, ByteBuffer, 8, 0);
            return BitConverter.ToInt64(ByteBuffer, 0);
        }


        public static ushort RUInt16(Memloc addr)
        {
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, ByteBuffer, 2, 0);
            return BitConverter.ToUInt16(ByteBuffer, 0);
        }


        public static uint RUInt32(Memloc addr)
        {
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, ByteBuffer, 4, 0);
            return BitConverter.ToUInt32(ByteBuffer, 0);
        }


        public static ulong RUInt64(Memloc addr)
        {
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, ByteBuffer, 8, 0);
            return BitConverter.ToUInt64(ByteBuffer, 0);
        }


        public static float RFloat(Memloc addr)
        {
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, ByteBuffer, 4, 0);
            return BitConverter.ToSingle(ByteBuffer, 0);
        }


        public static double RDouble(Memloc addr)
        {
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, ByteBuffer, 8, 0);
            return BitConverter.ToDouble(ByteBuffer, 0);
        }

        public static IntPtr RIntPtr(Memloc addr)
        {
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, ByteBuffer, IntPtr.Size, 0);
            if (IntPtr.Size == 4)
            {
                return new IntPtr(BitConverter.ToUInt32(ByteBuffer, 0));
            }
            else
            {
                return new IntPtr(BitConverter.ToInt64(ByteBuffer, 0));
            }
        }

        public static byte[] RBytes(Memloc addr, int size)
        {
            byte[] _rtnBytes = new byte[size];
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, _rtnBytes, size, 0);
            return _rtnBytes;
        }


        public static byte RByte(Memloc addr)
        {
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, ByteBuffer, 1, 0);
            return ByteBuffer[0];
        }


        public static string RAsciiStr(Memloc addr, int maxLength)
        {
            System.Text.StringBuilder Str = new System.Text.StringBuilder(maxLength);
            int loc = 0;

            var nextChr = '?';


            if (maxLength != 0)
            {
                byte[] bytes = new byte[2];

                while ((maxLength < 0 || loc < maxLength))
                {
                    Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)(addr + loc), bytes, 1, 0);
                    nextChr = System.Text.Encoding.ASCII.GetChars(bytes)[0];

                    if (nextChr == (char)0)
                    {
                        break; // TODO: might not be correct. Was : Exit While
                    }
                    else
                    {
                        Str.Append(nextChr);
                    }

                    loc += 1;
                }

            }

            return Str.ToString();
        }


        public static string RUnicodeStr(Memloc addr, int maxLength)
        {
            System.Text.StringBuilder Str = new System.Text.StringBuilder(maxLength);
            int loc = 0;

            var nextChr = '?';


            if (maxLength != 0)
            {
                byte[] bytes = new byte[3];

                while ((maxLength < 0 || loc < maxLength))
                {
                    Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), ((uint)addr) + (uint)loc * 2, bytes, 2, 0);
                    nextChr = System.Text.Encoding.Unicode.GetChars(bytes)[0];

                    if (nextChr == (char)0)
                    {
                        break; // TODO: might not be correct. Was : Exit While
                    }
                    else
                    {
                        Str.Append(nextChr);
                    }

                    loc += 1;
                }

            }

            return Str.ToString();
        }


        public static bool RBool(Memloc addr)
        {
            Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, ByteBuffer, 1, 0);
            return (ByteBuffer[0] != 0);
        }


        public static void WBool(Memloc addr, bool val)
        {
            Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, BitConverter.GetBytes(val), 1, 0);
        }


        public static void WInt16(Memloc addr, Int16 val)
        {
            Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, BitConverter.GetBytes(val), 2, 0);
        }


        public static void WUInt16(Memloc addr, UInt16 val)
        {
            Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, BitConverter.GetBytes(val), 2, 0);
        }


        public static void WInt32(Memloc addr, int val)
        {
            Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, BitConverter.GetBytes(val), 4, 0);
        }


        public static void WUInt32(Memloc addr, uint val)
        {
            Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, BitConverter.GetBytes(val), 4, 0);
        }


        public static void WInt64(Memloc addr, Int64 val)
        {
            Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, BitConverter.GetBytes(val), 8, 0);
        }


        public static void WUInt64(Memloc addr, UInt64 val)
        {
            Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, BitConverter.GetBytes(val), 8, 0);
        }


        public static void WFloat(Memloc addr, float val)
        {
            Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, BitConverter.GetBytes(val), 4, 0);
        }


        public static void WBytes(Memloc addr, byte[] val)
        {
            Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, val, val.Length, 0);
        }


        public static void WByte(Memloc addr, byte val)
        {
            Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, new byte[] { val }, 1, 0);
        }


        public static void WAsciiStr(Memloc addr, string str)
        {
            Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, System.Text.Encoding.ASCII.GetBytes(str).Concat(new byte[] { 0 }).ToArray(), str.Length + 1, 0);
        }


        public static void WUnicodeStr(Memloc addr, string str)
        {
            Kernel.WriteProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)addr, System.Text.Encoding.Unicode.GetBytes(str).Concat(new byte[] {
                0,
                0
            }).ToArray(), str.Length * 2 + 2, 0);
        }

        public static void WBit(Memloc baseAddr, int bitOffset, bool val)
        {
            var state = GetCurrentState();
            state.WBit_actualAddress = (baseAddr + (bitOffset / 8));
            state.WBit_mask = 0b10000000 >> (bitOffset % 8);

            if (Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)(state.WBit_actualAddress), ByteBuffer, 1, 0))
            {
                byte b = RByte(state.WBit_actualAddress);
                // ((b & mask) == mask) is the boolean value of the flag
                if (((b & state.WBit_mask) == state.WBit_mask) != val)
                {
                    if (val)
                        WByte(state.WBit_actualAddress, (byte)(b | state.WBit_mask)); 
                    else
                        WByte(state.WBit_actualAddress, (byte)(b & (~state.WBit_mask)));
                }
            }
        }

        public static bool RBit(Memloc baseAddr, int bitOffset)
        {
            var state = GetCurrentState();
            state.RBit_actualAddress = (baseAddr + (bitOffset / 8));
            state.RBit_mask = 0b10000000 >> (bitOffset % 8);

            if (Kernel.ReadProcessMemory_SAFE(DARKSOULS.GetHandle(), (uint)(state.RBit_actualAddress), ByteBuffer, 1, 0))
            {
                byte b = RByte(state.RBit_actualAddress);
                return (b & state.RBit_mask) == state.RBit_mask;
            }
            else
            {
                return false;
            }
        }
    }
}
