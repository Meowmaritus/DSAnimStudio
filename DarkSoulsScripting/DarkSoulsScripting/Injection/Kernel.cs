using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace DarkSoulsScripting.Injection
{
    static internal class Kernel
    {
        internal const int PROCESS_VM_READ = 0x10;
        internal const int TH32CS_SNAPPROCESS = 0x2;
        internal const int MEM_COMMIT = 4096;
        internal const int PAGE_READWRITE = 4;
        internal const int PAGE_EXECUTE_READWRITE = 0x40;
        internal const int PROCESS_CREATE_THREAD = (0x2);
        internal const int PROCESS_VM_OPERATION = (0x8);
        internal const int PROCESS_VM_WRITE = (0x20);
        internal const int PROCESS_ALL_ACCESS = 0x1f0fff;
        internal const int MEM_RELEASE = 0x8000;
        internal const int CREATE_SUSPENDED = 0x4;

        internal static Dictionary<long, bool> SafeMemoryIndex = new Dictionary<long, bool>();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        internal static bool CheckAddress(long addr, uint length, string action)
        {
            if (SafeMemoryIndex.ContainsKey(addr) && SafeMemoryIndex[addr])
            {
                return true;
            }
            else
            {
                if (addr > 0x7FFFFFFF || addr < Hook.DARKSOULS.SafeBaseMemoryOffset)
                {
                    Console.WriteLine($"Tried to {action} from invalid memory address 0x{addr:X} (minimum safe base offset address is hardcoded to 0x{Hook.DARKSOULS.SafeBaseMemoryOffset:X8}).");
                    return false;
                }
                else
                {
                    var query = VirtualQueryEx(Hook.DARKSOULS.GetHandle(), (IntPtr)addr, out var info, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
                    if (query == 0)
                    {
                        var lastError = new Win32Exception();
                        Console.Error.WriteLine($"WARNING: VirtualQueryEx at 0x{((uint)addr):X8} failed --> {lastError.Message}");
                        return false;
                    }
                    else
                    {
                        if (info.Protect != PAGE_EXECUTE_READWRITE)
                        {
                            if (VirtualProtectEx(Hook.DARKSOULS.GetHandle(), (IntPtr)addr, (UIntPtr)length, PAGE_EXECUTE_READWRITE, out var garbage))
                            {
                                SafeMemoryIndex.Add(addr, true);
                                return true;
                            }
                            else
                            {
                                var lastError = new Win32Exception();
                                Console.Error.WriteLine($"WARNING: VirtualProtectEx at 0x{((uint)addr):X8} failed --> {lastError.Message}");
                                return false;
                            }
                        }
                        else
                        {
                            SafeMemoryIndex.Add(addr, true);
                            return true;
                        }
                    }
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        static internal extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        static internal extern uint CreateRemoteThread(IntPtr hProcess, uint lpThreadAttributes, int dwStackSize, uint lpStartAddress, uint lpParameter, int dwCreationFlags, uint lpThreadId);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        static internal extern uint OpenProcess(int dwDesiredAcess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, uint lpBaseAddress, byte[] lpBuffer, int iSize, uint lpNumberOfBytesRead);

        static internal bool ReadProcessMemory_SAFE(IntPtr hProcess, uint lpBaseAddress, byte[] lpBuffer, int iSize, uint lpNumberOfBytesRead)
        {
            if (!CheckAddress(lpBaseAddress, (uint)iSize, "read"))
            {
                Array.Clear(lpBuffer, 0, iSize);
                return false;
            }

            return ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, iSize, lpNumberOfBytesRead);
        }

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, uint lpBaseAddress, byte[] lpBuffer, int iSize, int lpNumberOfBytesWritten);

        static internal bool WriteProcessMemory_SAFE(IntPtr hProcess, uint lpBaseAddress, byte[] lpBuffer, int iSize, int lpNumberOfBytesWritten)
        {
            if (!CheckAddress(lpBaseAddress, (uint)iSize, "write"))
            {
                return false;
            }

            return WriteProcessMemory(hProcess, lpBaseAddress, lpBuffer, iSize, lpNumberOfBytesWritten);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        static internal extern uint VirtualAllocEx(IntPtr hProcess, uint lpAddress, int dwSize, int flAllocationType, int flProtect);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        static internal extern bool VirtualFreeEx(IntPtr hProcess, uint lpAddress, int dwSize, int dwFreeType);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        static internal extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,
            UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        static internal extern int WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        static internal extern bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, uint lpBaseAddress, uint lpBuffer, int iSize, int lpNumberOfBytesWritten);

        static internal bool WriteProcessMemory_SAFE(IntPtr hProcess, uint lpBaseAddress, uint lpBuffer, int iSize, int lpNumberOfBytesWritten)
        {
            if (!CheckAddress(lpBaseAddress, (uint)iSize, "write"))
            {
                return false;
            }

            return WriteProcessMemory(hProcess, lpBaseAddress, lpBuffer, iSize, lpNumberOfBytesWritten);
        }

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static internal extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static internal extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static internal extern int ResumeThread(IntPtr hThread);

        internal static void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }

        internal static void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }
    }
}
