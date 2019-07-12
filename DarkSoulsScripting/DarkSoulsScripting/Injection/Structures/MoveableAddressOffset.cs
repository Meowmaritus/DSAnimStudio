using System;
namespace DarkSoulsScripting.Injection.Structures
{

    internal class MoveableAddressOffset
    {

        private uint offset = 0;
        private DSAsmCaller FuncCallerInstance { get; }

        public uint Location
        {
            get { return (uint)FuncCallerInstance.CodeHandle.GetHandle() + offset; }
            set { offset = (value - (uint)FuncCallerInstance.CodeHandle.GetHandle()); }
        }

        public MoveableAddressOffset(DSAsmCaller _funcCaller, IntPtr loc)
        {
            FuncCallerInstance = _funcCaller;
            Location = (uint)loc;
        }
    }

}
