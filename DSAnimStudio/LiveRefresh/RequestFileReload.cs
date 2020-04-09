using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.LiveRefresh
{
    public class RequestFileReload
    {
        internal static long GetReloadPtr()
        {
            if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
            {
                var GetReloadPtr_ = IntPtr.Add(Memory.BaseAddress, 0x4768E78);
                GetReloadPtr_ = new IntPtr(Memory.ReadInt64(GetReloadPtr_));
                return (long)GetReloadPtr_;
            }
            else
            {
                return 0;
            }

            
        }

        public static void RequestReloadParts()
        {
            var PartsPtr = (IntPtr)GetReloadPtr();

            if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
            {
                try
                {
                    Memory.AttachProc("DarkSoulsIII");

                    Memory.WriteFloat(PartsPtr + 0x3048, (float)10);
                    Memory.WriteBoolean(PartsPtr + 0x3044, true);
                }
                finally
                {
                    Memory.CloseHandle();
                }
            }

           
        }

        public enum ReloadType
        {
            Parts,
            Chr,
            Object,
        }
        public static void RequestReload(ReloadType type, string name)
        {
            if (type == ReloadType.Chr)
                RequestReloadChr(name);
            else if (type == ReloadType.Object)
                RequestReloadObj(name);
            else if (type == ReloadType.Parts)
                RequestReloadParts();
        }

        private static void RequestReloadChr(string chrName)
        {
            byte[] chrNameBytes = Encoding.Unicode.GetBytes(chrName);

            if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
            {
                try
                {
                    Memory.AttachProc("DarkSoulsIII");

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
                }
                finally
                {
                    Memory.CloseHandle();
                }
            }
            else if (GameDataManager.GameType == GameDataManager.GameTypes.DS1R)
            {
                try
                {

                    Memory.AttachProc("DarkSoulsRemastered");

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
                }
                finally
                {
                    Memory.CloseHandle();
                }
            }

            
        }

        private static void RequestReloadObj(string objName)
        {
            byte[] objNameBytes = Encoding.Unicode.GetBytes(objName);

            if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
            {
                try
                {
                    Memory.AttachProc("DarkSoulsIII");

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
                }
                finally
                {
                    Memory.CloseHandle();
                }

                
            }

            
        }
    }
}
