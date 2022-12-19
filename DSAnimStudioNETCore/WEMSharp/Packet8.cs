using System;
using System.IO;

namespace WEMSharp
{
    internal struct Packet8
    {
        private uint _offset;
        private uint _size;
        private uint _absoluteGranule;

        internal Packet8(Stream stream, uint offset)
        {
            this._offset = offset;
            this._size = 0xFFFF;
            this._absoluteGranule = 0;

            stream.Seek(this._offset, SeekOrigin.Begin);

            byte[] sizeBuffer = new byte[4];
            stream.Read(sizeBuffer, 0, 4);
            this._size = BitConverter.ToUInt32(sizeBuffer, 0);

            byte[] granuleBuffer = new byte[4];
            stream.Read(granuleBuffer, 0, 4);
            this._absoluteGranule = BitConverter.ToUInt32(granuleBuffer, 0);
        }

        internal uint GetHeaderSize()
        {
            return 8;
        }

        internal uint GetOffset()
        {
            return GetHeaderSize() + this._offset;
        }

        internal uint GetSize()
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
