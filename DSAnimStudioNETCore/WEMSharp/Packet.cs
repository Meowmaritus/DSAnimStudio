using System;
using System.IO;

namespace WEMSharp
{
    internal struct Packet
    {
        private uint _offset;
        private ushort _size;
        private uint _absoluteGranule;
        private bool _noGranule;

        internal Packet(Stream stream, uint offset, bool noGranule = false)
        {
            this._offset = offset;
            this._size = 0xFFFF;
            this._absoluteGranule = 0;
            this._noGranule = noGranule;

            stream.Seek(this._offset, SeekOrigin.Begin);

            byte[] sizeBuffer = new byte[2];
            stream.Read(sizeBuffer, 0, 2);
            this._size = BitConverter.ToUInt16(sizeBuffer, 0);

            if (!this._noGranule)
            {
                byte[] granuleBuffer = new byte[4];
                stream.Read(granuleBuffer, 0, 4);
                this._absoluteGranule = BitConverter.ToUInt32(granuleBuffer, 0);
            }
        }

        internal uint GetHeaderSize()
        {
            return this._noGranule ? (uint)2 : 6;
        }

        internal uint GetOffset()
        {
            return GetHeaderSize() + this._offset;
        }

        internal ushort GetSize()
        {
            return this._size;
        }

        internal uint GetGranule()
        {
            return this._absoluteGranule;
        }

        internal uint NextOffset()
        {
            return this._offset + GetHeaderSize() + this._size;
        }
    }
}
