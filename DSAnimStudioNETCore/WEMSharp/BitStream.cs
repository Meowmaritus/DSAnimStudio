using System.IO;

namespace WEMSharp
{
    /// <summary>
    /// Wrapper around a Stream that supports reading bits
    /// </summary>
    internal class BitStream
    {
        private Stream _stream;
        private byte _bitBuffer;
        private int _bitsLeft;
        internal ulong TotalBitsRead { get; private set; }

        internal BitStream(Stream stream)
        {
            this._stream = stream;
        }

        /// <summary>
        /// Reads a bit from the Stream
        /// </summary>
        internal byte GetBit()
        {
            if (this._bitsLeft == 0)
            {
                this._bitBuffer = (byte)this._stream.ReadByte();
                this._bitsLeft = 8;
            }

            this.TotalBitsRead++;
            this._bitsLeft--;

            return (this._bitBuffer & (0x80 >> this._bitsLeft)) != 0 ? (byte)1 : (byte)0;
        }

        /// <summary>
        /// Read an arbitary amount of bits from the stream
        /// </summary>
        /// <param name="bitCount">Amount of bits to read</param>
        internal uint Read(int bitCount)
        {
            uint result = 0;

            for (int i = 0; i < bitCount; i++)
            {
                if (GetBit() == 1)
                {
                    result |= (1U << i);
                }
            }

            return result;
        }
    }
}
