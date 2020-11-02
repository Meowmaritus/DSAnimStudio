using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public static unsafe class ImGuiEx
    {
        internal static unsafe class Util
        {
            internal const int StackAllocationSizeLimit = 2048;

            public static string StringFromPtr(byte* ptr)
            {
                int characters = 0;
                while (ptr[characters] != 0)
                {
                    characters++;
                }

                return Encoding.UTF8.GetString(ptr, characters);
            }

            internal static bool AreStringsEqual(byte* a, int aLength, byte* b)
            {
                for (int i = 0; i < aLength; i++)
                {
                    if (a[i] != b[i]) { return false; }
                }

                if (b[aLength] != 0) { return false; }

                return true;
            }

            internal static byte* Allocate(int byteCount) => (byte*)Marshal.AllocHGlobal(byteCount);
            internal static void Free(byte* ptr) => Marshal.FreeHGlobal((IntPtr)ptr);
            internal static int GetUtf8(string s, byte* utf8Bytes, int utf8ByteCount)
            {
                fixed (char* utf16Ptr = s)
                {
                    return Encoding.UTF8.GetBytes(utf16Ptr, s.Length, utf8Bytes, utf8ByteCount);
                }
            }
        }

        public static bool BeginPopupModal(string name, ImGuiWindowFlags flags)
        {
            byte* native_name;
            int name_byteCount = 0;
            if (name != null)
            {
                name_byteCount = Encoding.UTF8.GetByteCount(name);
                if (name_byteCount > Util.StackAllocationSizeLimit)
                {
                    native_name = Util.Allocate(name_byteCount + 1);
                }
                else
                {
                    byte* native_name_stackBytes = stackalloc byte[name_byteCount + 1];
                    native_name = native_name_stackBytes;
                }
                int native_name_offset = Util.GetUtf8(name, native_name, name_byteCount);
                native_name[native_name_offset] = 0;
            }
            else { native_name = null; }
            byte* native_p_open = null;
            byte ret = ImGuiNative.igBeginPopupModal(native_name, native_p_open, flags);
            if (name_byteCount > Util.StackAllocationSizeLimit)
            {
                Util.Free(native_name);
            }
            return ret != 0;
        }
    }
}
