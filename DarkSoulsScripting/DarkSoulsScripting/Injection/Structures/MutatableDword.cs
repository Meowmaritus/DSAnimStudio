using System;
using System.Runtime.InteropServices;

namespace DarkSoulsScripting.Injection.Structures
{
    [StructLayout(LayoutKind.Explicit)]
    public struct MutatableDword
    {
        [FieldOffset(0)]
        public byte Byte1;

        [FieldOffset(1)]
        public byte Byte2;

        [FieldOffset(2)]
        public byte Byte3;

        [FieldOffset(3)]
        public byte Byte4;

        [FieldOffset(0)]
        public sbyte SByte1;

        [FieldOffset(1)]
        public sbyte SByte2;

        [FieldOffset(2)]
        public sbyte SByte3;

        [FieldOffset(3)]
        public sbyte SByte4;

        public byte[] ByteArray
        {
            get
            {
                return new byte[] { Byte1, Byte2, Byte3, Byte4 };
            }
        }

        public void SetBytes(byte[] newBytes)
        {
            if (newBytes.Length != 4)
                throw new ArgumentException("Byte array must be exactly 4 bytes long.", "newBytes");

            Byte1 = newBytes[0];
            Byte2 = newBytes[1];
            Byte3 = newBytes[2];
            Byte4 = newBytes[3];
        }

        [FieldOffset(0)]
        public int Int1;

        [FieldOffset(0)]
        public uint UInt1;

        [FieldOffset(0)]
        public float Float1;

        [FieldOffset(0)]
        public short Short1;

        [FieldOffset(2)]
        public short Short2;

        [FieldOffset(0)]
        public ushort UShort1;

        [FieldOffset(2)]
        public ushort UShort2;

        public bool Bool1
        {
            get { return (Byte1 != 0); }
        }

        public bool Bool2
        {
            get { return (Byte2 != 0); }
        }

        public bool Bool3
        {
            get { return (Byte3 != 0); }
        }

        public bool Bool4
        {
            get { return (Byte4 != 0); }
        }

        public static implicit operator MutatableDword(short a) => new MutatableDword() { Int1 = a };
        public static implicit operator MutatableDword(ushort a) => new MutatableDword() { UInt1 = a };
        public static implicit operator MutatableDword(byte a) => new MutatableDword() { UInt1 = a };
        public static implicit operator MutatableDword(sbyte a) => new MutatableDword() { Int1 = a };
        public static implicit operator MutatableDword(int a) => new MutatableDword() { Int1 = a };
        public static implicit operator MutatableDword(uint a) => new MutatableDword() { UInt1 = a };
        public static implicit operator MutatableDword(float a) => new MutatableDword() { Float1 = a };
        public static implicit operator MutatableDword(bool a) => new MutatableDword() { Int1 = a ? 1 : 0 };

    }

}
