using System;
using System.Text;

namespace DSAnimStudio.TaeEditor
{
    public class TaeLiveRefresh
    {
        public static void ForceReloadCHR_DS1R(string chrName)
        {
            //var info = new System.Diagnostics.ProcessStartInfo()
            //{
            //    FileName = $@"{Main.Directory}\Content\ModelReloaderDS1R\ModelReloaderDS1R.exe",
            //    Arguments = chrName,
            //    CreateNoWindow = true,
            //    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
            //};
            //System.Diagnostics.Process.Start(info);
        }

        public static void ForceReloadCHR_PTDE(string chrName)
        {
            //if (!DarkSoulsScripting.Hook.DARKSOULS.Attached)
            //{
            //    if (!DarkSoulsScripting.Hook.DARKSOULS.TryAttachToDarkSouls(out string errorMsg))
            //    {
            //        //System.Windows.Forms.MessageBox.Show($"Failed to hook to Dark Souls: PTDE\n\n{errorMsg}");
            //        return;
            //    }
            //}

            //DarkSoulsScripting.Hook.WByte(0x013784F3, 1);

            //var stringAlloc = new DarkSoulsScripting.Injection.Structures.SafeRemoteHandle(chrName.Length * 2);

            //DarkSoulsScripting.Hook.WBytes(stringAlloc.GetHandle(), Encoding.Unicode.GetBytes(chrName));

            //DarkSoulsScripting.Hook.CallCustomX86((asm) =>
            //{
            //    asm.Mov32(Managed.X86.X86Register32.ECX, stringAlloc.GetHandle().ToInt32());
            //    asm.RawAsmBytes(new byte[] { 0x8B, 0x35, 0x44, 0xD6, 0x37, 0x01 }); //mov esi,[0x0137D644]
            //    asm.Push32(Managed.X86.X86Register32.ECX);
            //    asm.Call(new IntPtr(0x00E3F440));
            //    asm.Retn();
            //});
        }
    }
}
