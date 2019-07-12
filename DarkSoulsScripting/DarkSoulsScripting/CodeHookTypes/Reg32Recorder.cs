using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Managed.X86;
using DarkSoulsScripting.Injection.Structures;
using static DarkSoulsScripting.Hook;
using System.IO;

namespace DarkSoulsScripting.CodeHookTypes
{
    public class Reg32Recorder : CodeHookBase
    {
        public X86Register32 Register { get; private set; }
        public int RecordedValue => RInt32(RecordedValueHandle.GetHandle().ToInt64());

        private SafeRemoteHandle RecordedValueHandle { get; set; }

        /*
         * 01710000 - 89 35 00107101 - mov [target_ptr],esi        | 6 Bytes
         * 0171000E - E9 79E789FF    - jmp DARKSOULS.exe+BAE78C    | 5 Bytes
         * 
         * Total custom code size **NOT INCLUDING ORIGINAL CODE**: 11 bytes
         * 
         */

        private const int CustomCodeSize = 11;

        public Reg32Recorder(X86Register32 register, IntPtr originalLocalCodeStart, int originalLocalCodeLength, bool originalCodeAtEnd = true, byte[] specificOriginalCode = null) 
            : base(originalLocalCodeStart, originalLocalCodeLength, CustomCodeSize + originalLocalCodeLength, originalCodeAtEnd, specificOriginalCode)
        {
            Register = register;
        }

        protected override void BuildCustomRemoteCode(X86Writer w, MemoryStream ms)
        {
            RecordedValueHandle?.Dispose();
            RecordedValueHandle = new SafeRemoteHandle(sizeof(int));

            //Move address in Register into the value of RecordedValueHandle
            //
            //  e.g. with RecordedValueHandle = 7777 and Register = X86Register32.EAX:
            //      mov [7777],eax
            w.Mov32(new X86Address(X86Register32.None, RecordedValueHandle.GetHandle().ToInt32()), Register);
        }

        public override void Dispose()
        {
            RecordedValueHandle?.Dispose();

            base.Dispose();
        }
    }
}
