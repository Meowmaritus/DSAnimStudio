﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DSAnimStudio.LiveRefresh
{
    public class Memory
    {
        public enum Startbit : byte
        {
            Bit0 = 0,
            Bit1 = 1,
            Bit2 = 2,
            Bit3 = 3,
            Bit4 = 4,
            Bit5 = 5,
            Bit6 = 6,
            Bit7 = 7
        }

        //Memory Stuff
        public static bool Is64Bit => IntPtr.Size == 8;

        public static IntPtr ProcessHandle { get; set; }

        public static IntPtr BaseAddress { get; set; }
        //

        /// <summary>
        /// Checks if an address is valid.
        /// </summary>
        /// <param name="address">The address (the pointer points to).</param>
        /// <returns>True if (pointer points to a) valid address.</returns>


        public static System.Diagnostics.Process AttachedProcess = null;

        public static IntPtr ER_CrashFixPtr = IntPtr.Zero;
        public static IntPtr ER_WorldChrManPtr = IntPtr.Zero;

        public static IntPtr AC6_CrashFixPtr = IntPtr.Zero;
        public static IntPtr AC6_WorldChrManPtr = IntPtr.Zero;

        private static IntPtr ScanRelativeAob(AOBScanner aobScanner, string aob, int addrOffset, int endOfRelJumpInstr)
        {
            var aobLocation = aobScanner.Scan(AOBScanner.StringToAOB(aob));
            if (aobLocation == IntPtr.Zero)
                return IntPtr.Zero;
            uint relAddr = Kernel32.ReadUInt32(ProcessHandle, aobLocation + addrOffset);
            return (IntPtr)((ulong)(aobLocation + endOfRelJumpInstr) + relAddr);
        }

        private static Dictionary<string, string> _ini = new Dictionary<string, string>();
        private static object _iniLock = new object();

        public static void CheckIngameReloadINI()
        {
            lock (_iniLock)
            {
                var iniFilePath = Main.Directory + $"\\RES\\IngameReload_{zzz_DocumentManager.CurrentDocument.GameRoot.GameType}.ini";
                _ini = new Dictionary<string, string>();
                if (System.IO.File.Exists(iniFilePath))
                {
                    var iniText = System.IO.File.ReadAllLines(iniFilePath);
                    foreach (var line in iniText)
                    {
                        var l = line.Trim();
                        if (l.StartsWith("#"))
                            continue;
                        int equalsIndex = l.IndexOf("=");
                        if (equalsIndex < 0)
                            continue;
                        string beforeEquals = l.Substring(0, equalsIndex).Trim();
                        string afterEquals = l.Substring(equalsIndex + 1, l.Length - equalsIndex - 1).Trim();
                        _ini[beforeEquals] = afterEquals;
                    }
                }
            }
        }

        public static string GetIngameReloadIniOption(string optionName)
        {
            string result = null;
            lock (_iniLock)
            {
                if (_ini.ContainsKey(optionName))
                    result = _ini[optionName];
            }
            return result;
        }

        public static int? GetIngameReloadIniOptionInt(string optionName)
        {
            int? result = null;
            lock (_iniLock)
            {
                if (_ini.ContainsKey(optionName) && int.TryParse(_ini[optionName], out int asInt))
                    result = asInt;
            }
            return result;
        }

        public static int? GetIngameReloadIniOptionIntHex(string optionName)
        {
            int? result = null;
            lock (_iniLock)
            {
                if (_ini.ContainsKey(optionName) && int.TryParse(_ini[optionName], System.Globalization.NumberStyles.HexNumber, null, out int asInt))
                    result = asInt;
            }
            return result;
        }

        public static byte[] GetIngameReloadIniOptionByteArrayHex(string optionName)
        {
            byte[]? result = null;
            lock (_iniLock)
            {
                try
                {
                    if (_ini.ContainsKey(optionName))
                    {
                        result = _ini[optionName].Trim().Split(" ").Select(x => (byte)int.Parse(x, System.Globalization.NumberStyles.HexNumber)).ToArray();
                    }
                }
                catch
                {

                }
            }
            return result;
        }

        public static void UpdateAOBs_ER()
        {
            CheckIngameReloadINI();

            var aob = new AOBScanner(AttachedProcess);
            ER_WorldChrManPtr = Memory.ScanRelativeAob(aob, GetIngameReloadIniOption("ER_WorldChrManPtr_AOB") ?? "48 8B 05 ?? ?? ?? ?? 48 85 C0 74 0F 48 39 88 ?? ?? ?? ?? 75 06 89 B1 64 03 ?? ?? 0F 28 05 ?? ?? ?? ?? 4C 8D 45 E7",
                GetIngameReloadIniOptionInt("ER_WorldChrManPtr_JumpInstr_StartOffsetInAOB") ?? 3, GetIngameReloadIniOptionInt("ER_WorldChrManPtr_JumpInstr_EndOffsetInAOB") ?? 7);
            if (ER_WorldChrManPtr == IntPtr.Zero)
                zzz_NotificationManagerIns.PushNotification("Live reload WARNING - Could not find Elden Ring WorldChrMan AOB");
            ER_WorldChrManPtr = Kernel32.ReadIntPtr(ProcessHandle, ER_WorldChrManPtr, true);

            var crashPatchOffsetAob = AOBScanner.StringToAOB(GetIngameReloadIniOption("ER_CrashPatchOffset_AOB") ?? "80 65 ?? FD 48 C7 45 ?? 07 00 00 00 ?? 8D 45 48 4C 89 60 ?? 48 83 78 ?? 08 72 03 48 8B 00 66 44 89 20 49 8B 8F ?? ?? ?? ?? 48 8B 01 48 ?? ??");
            IntPtr crashPatchOffset = aob.Scan(crashPatchOffsetAob);
            if (crashPatchOffset == IntPtr.Zero)
                zzz_NotificationManagerIns.PushNotification("Live reload WARNING - Could not find Elden Ring crash patch AOB");
            crashPatchOffset = crashPatchOffset + crashPatchOffsetAob.Length - (GetIngameReloadIniOptionInt("ER_CrashPatchOffset_DistFromEndOfAOB") ?? 3);
            ER_CrashFixPtr = crashPatchOffset;
        }

        public static void UpdateAOBs_ERNR()
        {
            //ERNR TODO
            CheckIngameReloadINI();

            var aob = new AOBScanner(AttachedProcess);
            ER_WorldChrManPtr = Memory.ScanRelativeAob(aob, GetIngameReloadIniOption("ER_WorldChrManPtr_AOB") ?? "48 8B 05 ?? ?? ?? ?? 48 85 C0 74 0F 48 39 88 ?? ?? ?? ?? 75 06 89 B1 64 03 ?? ?? 0F 28 05 ?? ?? ?? ?? 4C 8D 45 E7",
                GetIngameReloadIniOptionInt("ER_WorldChrManPtr_JumpInstr_StartOffsetInAOB") ?? 3, GetIngameReloadIniOptionInt("ER_WorldChrManPtr_JumpInstr_EndOffsetInAOB") ?? 7);
            if (ER_WorldChrManPtr == IntPtr.Zero)
                zzz_NotificationManagerIns.PushNotification("Live reload WARNING - Could not find Elden Ring WorldChrMan AOB");
            ER_WorldChrManPtr = Kernel32.ReadIntPtr(ProcessHandle, ER_WorldChrManPtr, true);

            var crashPatchOffsetAob = AOBScanner.StringToAOB(GetIngameReloadIniOption("ER_CrashPatchOffset_AOB") ?? "80 65 ?? FD 48 C7 45 ?? 07 00 00 00 ?? 8D 45 48 4C 89 60 ?? 48 83 78 ?? 08 72 03 48 8B 00 66 44 89 20 49 8B 8F ?? ?? ?? ?? 48 8B 01 48 ?? ??");
            IntPtr crashPatchOffset = aob.Scan(crashPatchOffsetAob);
            if (crashPatchOffset == IntPtr.Zero)
                zzz_NotificationManagerIns.PushNotification("Live reload WARNING - Could not find Elden Ring crash patch AOB");
            crashPatchOffset = crashPatchOffset + crashPatchOffsetAob.Length - (GetIngameReloadIniOptionInt("ER_CrashPatchOffset_DistFromEndOfAOB") ?? 3);
            ER_CrashFixPtr = crashPatchOffset;
        }

        public static void UpdateAOBs_AC6()
        {
            CheckIngameReloadINI();

            var aob = new AOBScanner(AttachedProcess);

            var iniWorldChrManAob = GetIngameReloadIniOption("AC6_WorldChrManPtr_AOB");

            AC6_WorldChrManPtr = Memory.ScanRelativeAob(aob, iniWorldChrManAob ?? "48 8B 05 ?? ?? ?? ?? 48 85 C0 74 0F 48 39 88 ?? ?? ?? ?? 75 06 89 B1 64 03 ?? ?? 0F 28 05 ?? ?? ?? ?? 4C 8D 45 E7",
                GetIngameReloadIniOptionInt("AC6_WorldChrManPtr_JumpInstr_StartOffsetInAOB") ?? 3, GetIngameReloadIniOptionInt("AC6_WorldChrManPtr_JumpInstr_EndOffsetInAOB") ?? 7);
            if (AC6_WorldChrManPtr == IntPtr.Zero)
                zzz_NotificationManagerIns.PushNotification("Live reload WARNING - Could not find AC6 WorldChrMan AOB");
            AC6_WorldChrManPtr = Kernel32.ReadIntPtr(ProcessHandle, AC6_WorldChrManPtr, true);

            var iniCrashPtrAob = GetIngameReloadIniOption("AC6_CrashPatchOffset_AOB");

            var crashPatchOffsetAob = AOBScanner.StringToAOB(iniCrashPtrAob ?? "80 65 ?? FD 48 C7 45 ?? 07 00 00 00 ?? 8D 45 48 4C 89 60 ?? 48 83 78 ?? 08 72 03 48 8B 00 66 44 89 20 49 8B 8F ?? ?? ?? ?? 48 8B 01 48 ?? ??");
            IntPtr crashPatchOffset = aob.Scan(crashPatchOffsetAob);
            if (crashPatchOffset == IntPtr.Zero)
                zzz_NotificationManagerIns.PushNotification("Live reload WARNING - Could not find AC6 crash patch AOB");
            crashPatchOffset = crashPatchOffset + crashPatchOffsetAob.Length - (GetIngameReloadIniOptionInt("AC6_CrashPatchOffset_DistFromEndOfAOB") ?? 3);
            AC6_CrashFixPtr = crashPatchOffset;
        }

        public static void AttachProc(string procName)
        {
            if (AttachedProcess != null && AttachedProcess?.HasExited != true)
                return;

            if (!Kernel32.GetHandleInformation(ProcessHandle, out _))
            {
                CloseHandle(handleInvalid: true);
            }
            //CloseHandle();
            var processes = System.Diagnostics.Process.GetProcessesByName(procName);
            if (processes.Length > 0)
            {
                var Process = processes[0];
                BaseAddress = Process.MainModule.BaseAddress;
                try
                {
                    ProcessHandle = Kernel32.OpenProcess(0x2 | 0x8 | 0x10 | 0x20 | 0x400, false, Process.Id);
                    AttachedProcess = Process;
                    AttachedProcess.Exited += AttachedProcess_Exited;

                    if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER)
                    {
                        UpdateAOBs_ER();
                    }
                    else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ERNR)
                    {
                        UpdateAOBs_ERNR();
                    }
                    else
                    {
                        ER_CrashFixPtr = IntPtr.Zero;
                        ER_WorldChrManPtr = IntPtr.Zero;
                    }
                }
                catch
                {
                    CloseHandle();
                }
            }
            else
            {
                Console.WriteLine("Cant find process. Is it running?", "Process");
            }
        }

        private static void AttachedProcess_Exited(object sender, EventArgs e)
        {
            CloseHandle();
        }

        public static void CloseHandle(bool handleInvalid = false)
        {
            if (AttachedProcess != null)
            {
                AttachedProcess.Exited -= AttachedProcess_Exited;
                AttachedProcess.Dispose();
                AttachedProcess = null;
            }
            if (ProcessHandle != IntPtr.Zero)
            {
                if (!handleInvalid)
                {
                    try
                    {
                        Kernel32.CloseHandle(ProcessHandle);
                    }
                    catch
                    {

                    }
                }
                ProcessHandle = IntPtr.Zero;
            }
            ER_CrashFixPtr = IntPtr.Zero;
            ER_WorldChrManPtr = IntPtr.Zero;
        }

        // read address

        public static bool ReadBoolean(IntPtr address)
        {
            var readBuffer = new byte[sizeof(byte)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)1, UIntPtr.Zero);
            byte value = readBuffer[0];
            var boolRet = Convert.ToBoolean(value);
            return boolRet;
        }

        public static byte ReadInt8(IntPtr address)
        {
            var readBuffer = new byte[sizeof(byte)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)1, UIntPtr.Zero);
            var value = readBuffer[0];
            return value;
        }

        public static short ReadInt16(IntPtr address)
        {
            var readBuffer = new byte[sizeof(short)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)2, UIntPtr.Zero);
            var value = BitConverter.ToInt16(readBuffer, 0);
            return value;
        }

        public static int ReadInt32(IntPtr address)
        {
            var readBuffer = new byte[sizeof(int)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var value = BitConverter.ToInt32(readBuffer, 0);
            return value;
        }

        public static long ReadInt64(IntPtr address)
        {
            var readBuffer = new byte[sizeof(long)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var value = BitConverter.ToInt64(readBuffer, 0);
            return value;
        }

        public static float ReadFloat(IntPtr address)
        {
            var readBuffer = new byte[sizeof(float)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var value = BitConverter.ToSingle(readBuffer, 0);
            return value;
        }

        public static double ReadDouble(IntPtr address)
        {
            var readBuffer = new byte[sizeof(double)];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var value = BitConverter.ToDouble(readBuffer, 0);
            return value;
        }

        public static string ReadString(IntPtr address, int length, string encodingName)
        {
            var readBuffer = new byte[length];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var encodingType = System.Text.Encoding.GetEncoding(encodingName);
            string value = encodingType.GetString(readBuffer, 0, readBuffer.Length);

            return value;
        }

        public static string ReadUnicodeString(IntPtr address, int length)
        {
            var readBuffer = new byte[length];
            var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);

            for (int i = 0; i < readBuffer.Length; i++)
            {
                if (readBuffer[i] == 0 && readBuffer[i + 1] == 0)
                {
                    Array.Resize(ref readBuffer, i + 1);
                    break;
                }
                
            }

            var encodingType = System.Text.Encoding.GetEncoding("UNICODE");
            string value = encodingType.GetString(readBuffer, 0, readBuffer.Length);

            return value;
        }

        //write to address
        public static bool WriteFlags8(IntPtr address, bool value, Startbit startbit)
        {
            var WriteBit = Convert.ToByte(value) * Convert.ToByte(Math.Pow((double)2, (double)startbit));
            var WriteBit_ = (byte)WriteBit;


            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(WriteBit_), (UIntPtr)1, UIntPtr.Zero);
        }

        public static bool WriteBoolean(IntPtr address, bool value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)1, UIntPtr.Zero);
        }

        public static bool WriteInt8(IntPtr address, byte value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)1, UIntPtr.Zero);
        }

        public static bool WriteInt16(IntPtr address, short value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)2, UIntPtr.Zero);
        }

        public static bool WriteInt32(IntPtr address, int value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)4, UIntPtr.Zero);
        }

        public static bool WriteInt64(IntPtr address, long value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)8, UIntPtr.Zero);
        }

        public static bool WriteFloat(IntPtr address, float value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)4, UIntPtr.Zero);
        }

        public static bool WriteDouble(IntPtr address, double value)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)8, UIntPtr.Zero);
        }

        public static bool WriteBytes(IntPtr address, Byte[] val)
        {
            return Kernel32.WriteProcessMemory(ProcessHandle, address, val, new UIntPtr((uint)val.Length), UIntPtr.Zero);
        }

        public static bool WriteUnicodeString(IntPtr address, string String)
        {
            byte[] val = Encoding.Unicode.GetBytes(String);
            return Kernel32.WriteProcessMemory(ProcessHandle, address, val, new UIntPtr((uint)val.Length), UIntPtr.Zero);
        }

        public static bool WriteASCIIString(IntPtr address, string String)
        {
            byte[] val = Encoding.ASCII.GetBytes(String);
            return Kernel32.WriteProcessMemory(ProcessHandle, address, val, new UIntPtr((uint)val.Length), UIntPtr.Zero);
        }

        public static void ExecuteFunction(byte[] array)
        {
            var buffer = 0x100;

            var address = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            if (address != IntPtr.Zero)
            {
                if (WriteBytes(address, array))
                {
                    var threadHandle = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        Kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                Kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }

        public static void ExecuteBufferFunction(byte[] array, byte[] argument, int argLocationInAsmArray = 0x2)
        {
            var Size1 = 0x100;
            var Size2 = 0x100;

            var address = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, Size1, 0x1000 | 0x2000, 0x40);
            var bufferAddress = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, Size2, 0x1000 | 0x2000, 0x40);

            try
            {
                //var bytjmp = 0x2;
                var bytjmpAr = new byte[8];

                WriteBytes(bufferAddress, argument);

                bytjmpAr = BitConverter.GetBytes((long)bufferAddress);
                Array.Copy(bytjmpAr, 0, array, argLocationInAsmArray, bytjmpAr.Length);

                if (address != IntPtr.Zero && bufferAddress != IntPtr.Zero)
                {
                    if (WriteBytes(address, array))
                    {

                        var threadHandle = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                        if (threadHandle != IntPtr.Zero)
                        {
                            Kernel32.WaitForSingleObject(threadHandle, 30000);
                        }

                    }
                    Kernel32.VirtualFreeEx(ProcessHandle, address, Size1, 2);
                    Kernel32.VirtualFreeEx(ProcessHandle, bufferAddress, Size2, 2);
                }
            }
            finally
            {
                if (address != IntPtr.Zero)
                {
                    Kernel32.VirtualFreeEx(ProcessHandle, address, Size1, 2);
                }
                if (bufferAddress != IntPtr.Zero)
                {
                    Kernel32.VirtualFreeEx(ProcessHandle, bufferAddress, Size2, 2);
                }
            }

            
        }
    }
}
