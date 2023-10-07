using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.LiveRefresh
{
    public class RequestFileReload
    {
        public static bool CanReloadEntity(string entityName)
        {
            if (entityName == null)
                return false;
            entityName = entityName.ToLower();
            // Character reload
            if (entityName.StartsWith("c"))
            {
                switch (GameRoot.GameType)
                {
                    case SoulsAssetPipeline.SoulsGames.DS1R:
                    case SoulsAssetPipeline.SoulsGames.DS3:
                    case SoulsAssetPipeline.SoulsGames.ER:
                    case SoulsAssetPipeline.SoulsGames.SDT:
                        return true;
                }
            }
            // Object reload
            else if (entityName.StartsWith("o"))
            {
                switch (GameRoot.GameType)
                {
                    case SoulsAssetPipeline.SoulsGames.DS3: 
                        return true;
                }
            }

            return false;
        }

        public static bool RequestReloadParts()
        {
            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
            {
                try
                {
                    
                    Memory.AttachProc("DarkSoulsIII");
                    var partsReloadPtr = IntPtr.Add(Memory.BaseAddress, 0x4768E78);
                    partsReloadPtr = new IntPtr(Memory.ReadInt64(partsReloadPtr));

                    Memory.WriteFloat(partsReloadPtr + 0x3048, (float)10);
                    Memory.WriteBoolean(partsReloadPtr + 0x3044, true);

                    return true;
                }
                finally
                {
                    //Memory.CloseHandle();
                }
            }

            return false;
        }

        public enum ReloadType
        {
            Parts,
            Chr,
            Object,
        }
        public static bool RequestReload(ReloadType type, string name)
        {
            if (type == ReloadType.Chr)
            {
                var result = RequestReloadChr(name);
                //GC.Collect();
                return result;
            }
            else if (type == ReloadType.Object)
            {
                var result = RequestReloadObj(name);
                //GC.Collect();
                return result;
            }
            else if (type == ReloadType.Parts)
            {
                var result = RequestReloadParts();
                //GC.Collect();
                return result;
            }

            return false;
        }

        private static void ShowInjectionFailed()
        {
            NotificationManager.PushNotification("Process injection failed. Make sure the game is running and DS Anim Studio has permission " +
                "to control processes (running as administrator will force this to be true)."
                + (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER ? "\n\nFor Elden Ring, make sure EasyAntiCheat is not enabled as it prevents all process memory writing." : ""), 
                showDuration: 10, color: NotificationManager.ColorWarning);
        }

        private static bool RequestReloadChr(string chrName)
        {
            byte[] chrNameBytes = Encoding.Unicode.GetBytes(chrName);

            if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.SDT)
            {
                try
                {
                    Memory.AttachProc("sekiro");

                    if (Memory.ProcessHandle != IntPtr.Zero)
                    {

                        Memory.WriteBoolean(Memory.BaseAddress + 0x3D7A34F, true);

                        var buffer = new byte[] {
                            0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rcx,0000000000000000 (read value at 143D7A1E0 and put it here)
                            0x48, 0xBA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rdx,0000000000000000 (address of chr name string)
                            0x48, 0x83, 0xEC, 0x28, // sub rsp,28 
                            0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08, 0x60, 0xAC, 0xA4, 0x40, 0x01, 0x00, 0x00, 0x00, //call 140A4AC60
                            0x48, 0x83, 0xC4, 0x28, // add rsp,28
                            0xC3, // ret
                        };

                        var ptrThingVal = Memory.ReadInt64((IntPtr)0x143D7A1E0);
                        var ptrThingVal_AsBytes = BitConverter.GetBytes((long)ptrThingVal);
                        Array.Copy(ptrThingVal_AsBytes, 0, buffer, 0x2, ptrThingVal_AsBytes.Length);

                        Memory.ExecuteBufferFunction(buffer, chrNameBytes, argLocationInAsmArray: 0xC);

                        return true;
                    }
                    else
                    {
                        ShowInjectionFailed();
                    }


                }
                catch
                {
                    ShowInjectionFailed();
                }
                finally
                {
                    //Memory.CloseHandle();
                }
            }

            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
            {
                try
                {
                    Memory.AttachProc("DarkSoulsIII");

                    if (Memory.ProcessHandle != IntPtr.Zero)
                    {

                        Memory.WriteBoolean(Memory.BaseAddress + 0x4768F7F, true);

                        var buffer = new byte[]
                        {
                            0x48, 0xBA, 0, 0, 0, 0, 0, 0, 0, 0, //mov rdx,Alloc
                            0x48, 0xA1, 0x78, 0x8E, 0x76, 0x44, 0x01, 0x00, 0x00, 0x00, //mov rax,[144768E78]
                            0x48, 0x8B, 0xC8, //mov rcx,rax
                            0x49, 0xBE, 0x10, 0x1E, 0x8D, 0x40, 0x01, 0x00, 0x00, 0x00, //mov r14,00000001408D1E10
                            0x48, 0x83, 0xEC, 0x28, //sub rsp,28
                            0x41, 0xFF, 0xD6, //call r14
                            0x48, 0x83, 0xC4, 0x28, //add rsp,28
                            0xC3 //ret
                        };

                        Memory.ExecuteBufferFunction(buffer, chrNameBytes);

                        return true;
                    }
                    else
                    {
                        ShowInjectionFailed();
                    }

                    
                }
                catch
                {
                    ShowInjectionFailed();
                }
                finally
                {
                    //Memory.CloseHandle();
                }
            }
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER)
            {
                try
                {
                    Memory.AttachProc("eldenring");
                    if (Memory.ProcessHandle == IntPtr.Zero)
                        Memory.AttachProc("start_protected_game");

                    

                    

                    if (Memory.ProcessHandle != IntPtr.Zero)
                    {
                        var fileInfo = Memory.AttachedProcess.MainModule.FileVersionInfo;
                        int gameVersion = fileInfo.FileMajorPart * 1_00_00_00
                            + fileInfo.FileMinorPart * 1_00_00
                            + fileInfo.FileBuildPart * 1_00
                            + fileInfo.FilePrivatePart;

                        var chrReload = Kernel32.VirtualAllocEx(Memory.ProcessHandle, IntPtr.Zero, 256, 0x1000 | 0x2000, 0x40);
                        var chrReload_DataSetup = Kernel32.VirtualAllocEx(Memory.ProcessHandle, IntPtr.Zero, 256, 0x1000 | 0x2000, 0x40);

                        if (chrReload != IntPtr.Zero && chrReload_DataSetup != IntPtr.Zero)
                        {
                            try
                            {
                                Memory.UpdateEldenRingAobs();

                                var dataPointer = Memory.ReadInt64((IntPtr)Memory.ReadInt64(Memory.EldenRing_WorldChrManPtr + (Memory.GetIngameReloadIniOptionIntHex("EldenRing_WorldChrManStructOffset") ?? 0x185C0)) + 0x0);

                                Memory.WriteInt64(chrReload_DataSetup + 0x8, dataPointer); // Pointer to data
                                Memory.WriteInt64(chrReload_DataSetup + 0x58, (chrReload_DataSetup + 0x100).ToInt64()); // Pointer to string
                                Memory.WriteInt8(chrReload_DataSetup + 0x70, 0x1F); // String length
                                Memory.WriteBytes(chrReload_DataSetup + 0x100, chrNameBytes);

                                // Crash fix offset, last updated for 1.05
                                var writeBytes = Memory.GetIngameReloadIniOptionByteArrayHex("EldenRing_CrashPatchOffset_WriteBytes") ?? new byte[] { 0x48, 0x31, 0xD2 };
                                Memory.WriteBytes(Memory.EldenRing_CrashFixPtr, writeBytes);

                                //OLD VERSION
                                //var buffer = new byte[]
                                //{
                                //    0x48, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // mov rbx,0000000000000000 (ChrReload_DataSetup)
                                //    0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // mov rcx,0000000000000000 (WorldChrMan)
                                //    0x48, 0x8B, 0x91, 0xC0, 0x85, 0x01, 0x00,                       // mov rdx,[rcx+000185C0]
                                //    0x48, 0x89, 0x1A,                                               // mov [rdx],rbx
                                //    0x48, 0x89, 0x13,                                               // mov [rbx],rdx
                                //    0x48, 0x8B, 0x91, 0xC0, 0x85, 0x01, 0x00,                       // mov rdx,[rcx+000185C0]
                                //    0x48, 0x89, 0x5A, 0x08,                                         // mov [rdx+08],rbx
                                //    0x48, 0x89, 0x53, 0x08,                                         // mov [rbx+08],rdx
                                //    0xC7, 0x81, 0xC8, 0x85, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00,     // mov [rcx+000185C8],00000001 { 1 }
                                //    0xC7, 0x81, 0xD0, 0x85, 0x01, 0x00, 0x00, 0x00, 0x20, 0x41,     // mov [rcx+000185D0],41200000 { 10.00 }
                                //    0xC3,                                                           // ret 
                                //};

                                byte[] buffer = null;

                                if (gameVersion >= 01_07_00_00)
                                {
                                    buffer = new byte[]
                                    {
                                        0x48, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // mov rbx,0000000000000000 (ChrReload_DataSetup)
                                        0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // mov rcx,0000000000000000 (WorldChrMan)
                                        0x48, 0x8B, 0x91, 0x60, 0xE6, 0x01, 0x00,                       // mov rdx,[rcx+0001E660]
                                        0x48, 0x89, 0x1A,                                               // mov [rdx],rbx
                                        0x48, 0x89, 0x13,                                               // mov [rbx],rdx
                                        0x48, 0x8B, 0x91, 0x60, 0xE6, 0x01, 0x00,                       // mov rdx,[rcx+0001E660]
                                        0x48, 0x89, 0x5A, 0x08,                                         // mov [rdx+08],rbx
                                        0x48, 0x89, 0x53, 0x08,                                         // mov [rbx+08],rdx
                                        0xC7, 0x81, 0x68, 0xE6, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00,     // mov [rcx+0001E668],00000001 { 1 }
                                        0xC7, 0x81, 0x70, 0xE6, 0x01, 0x00, 0x00, 0x00, 0x20, 0x41,     // mov [rcx+0001E670],41200000 { 10.00 }
                                        0xC3,                                                           // ret 
                                    };
                                }
                                else
                                {
                                    buffer = new byte[]
                                    {
                                        0x48, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // mov rbx,0000000000000000 (ChrReload_DataSetup)
                                        0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // mov rcx,0000000000000000 (WorldChrMan)
                                        0x48, 0x8B, 0x91, 0xC0, 0x85, 0x01, 0x00,                       // mov rdx,[rcx+000185C0]
                                        0x48, 0x89, 0x1A,                                               // mov [rdx],rbx
                                        0x48, 0x89, 0x13,                                               // mov [rbx],rdx
                                        0x48, 0x8B, 0x91, 0xC0, 0x85, 0x01, 0x00,                       // mov rdx,[rcx+000185C0]
                                        0x48, 0x89, 0x5A, 0x08,                                         // mov [rdx+08],rbx
                                        0x48, 0x89, 0x53, 0x08,                                         // mov [rbx+08],rdx
                                        0xC7, 0x81, 0xC8, 0x85, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00,     // mov [rcx+000185C8],00000001 { 1 }
                                        0xC7, 0x81, 0xD0, 0x85, 0x01, 0x00, 0x00, 0x00, 0x20, 0x41,     // mov [rcx+000185D0],41200000 { 10.00 }
                                        0xC3,                                                           // ret 
                                    };
                                }

                                Array.Copy(BitConverter.GetBytes(chrReload_DataSetup.ToInt64()), 0, buffer, 0x2, 0x8);
                                Array.Copy(BitConverter.GetBytes(Memory.EldenRing_WorldChrManPtr.ToInt64()), 0, buffer, 0xC, 0x8);


                                Memory.WriteBytes(chrReload, buffer);

                                var threadHandle = Kernel32.CreateRemoteThread(Memory.ProcessHandle, IntPtr.Zero, 0, chrReload, IntPtr.Zero, 0, out var threadId);
                                if (threadHandle != IntPtr.Zero)
                                {
                                    Kernel32.WaitForSingleObject(threadHandle, 30000);
                                }

                                return true;
                            }
                            finally
                            {
                                Kernel32.VirtualFreeEx(Memory.ProcessHandle, chrReload, 256, 2);
                                Kernel32.VirtualFreeEx(Memory.ProcessHandle, chrReload_DataSetup, 256, 2);
                            }
                        }
                    }
                    else
                    {
                        ShowInjectionFailed();
                    }
                }
                catch (Exception e)
                {
                    ShowInjectionFailed();
                }
                finally
                {
                    //Memory.CloseHandle();
                }
            }
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                try
                {

                    Memory.AttachProc("DarkSoulsRemastered");
                    if (Memory.ProcessHandle != IntPtr.Zero)
                    {
                        Memory.WriteBoolean((IntPtr)0x141D151DB, true);

                        var buffer = new byte[]
                        {
                        0x48, 0xBA, 0, 0, 0, 0, 0, 0, 0, 0, //mov rdx,Alloc
                        0x48, 0xA1, 0xB0, 0x51, 0xD1, 0x41, 0x01, 0x00, 0x00, 0x00,  //mov rax,[141D151B0]
                        0x48, 0x8B, 0xC8, //mov rcx,rax
                        0x49, 0xBE, 0xA0, 0x12, 0x37, 0x40, 0x01, 0x00, 0x00, 0x00, //mov r14,00000001403712A0
                        0x48, 0x83, 0xEC, 0x28, //sub rsp,28
                        0x41, 0xFF, 0xD6, //call r14
                        0x48, 0x83, 0xC4, 0x28,  //add rsp,28
                        0xC3 //Ret
                        };

                        Memory.ExecuteBufferFunction(buffer, chrNameBytes);

                        return true;
                    }
                    else
                    {
                        ShowInjectionFailed();
                    }
                }
                catch
                {
                    ShowInjectionFailed();
                }
                finally
                {
                    //Memory.CloseHandle();
                }
            }

            return false;
        }

        private static bool RequestReloadObj(string objName)
        {
            byte[] objNameBytes = Encoding.Unicode.GetBytes(objName);

            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
            {
                try
                {
                    Memory.AttachProc("DarkSoulsIII");

                    if (Memory.ProcessHandle != IntPtr.Zero)
                    {
                        var buffer = new byte[]
                        {
                            0x48, 0xBA, 0, 0, 0, 0, 0, 0, 0, 0, //mov rdx,Alloc
                            0x48, 0xA1, 0xC8, 0x51, 0x74, 0x44, 0x01, 0x00, 0x00, 0x00, //mov rax,[1447451C8]
                            0x48, 0x8B, 0xC8, //mov rcx,rax
                            0x49, 0xBE, 0x10, 0x1E, 0x8D, 0x40, 0x01, 0x00, 0x00, 0x00, //mov r14,000000014067FFF0
                            0x48, 0x83, 0xEC, 0x28, //sub rsp,28
                            0x41, 0xFF, 0xD6, //call r14
                            0x48, 0x83, 0xC4, 0x28, //add rsp,28
                            0xC3 //ret
                        };

                        Memory.ExecuteBufferFunction(buffer, objNameBytes);

                        return true;
                    }
                    else
                    {
                        ShowInjectionFailed();
                    }

                }
                finally
                {
                    //Memory.CloseHandle();
                }

                
            }
            return false;
            
        }
    }
}
