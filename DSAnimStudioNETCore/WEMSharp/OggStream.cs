using System;
using System.IO;
using System.Text;

namespace WEMSharp
{
    public class OggStream : BinaryWriter
    {
        private const uint HEADER_SIZE = 27;
        private const uint MAX_SEGMENTS = 255;
        private const uint SEGMENT_SIZE = 255;

        private byte _bitBuffer;
        private int _bitsStored;
        private uint _payloadBytes;
        private bool _first = true;
        private bool _continued;
        private byte[] _pageBuffer = new byte[HEADER_SIZE + MAX_SEGMENTS + SEGMENT_SIZE * MAX_SEGMENTS];
        private uint _granule;
        private uint _sequenceNumber;

        internal OggStream(Stream file) : base(file) { }
        ~OggStream()
        {
            FlushPage();
        }

        internal void WriteBit(byte bit)
        {
            if (bit == 1)
            {
                this._bitBuffer |= (byte)(1U << this._bitsStored);
            }

            this._bitsStored++;

            if (this._bitsStored == 8)
            {
                FlushBits();
            }
        }

        internal void FlushBits()
        {
            if (this._bitsStored != 0)
            {
                if (this._payloadBytes == SEGMENT_SIZE * MAX_SEGMENTS)
                {
                    throw new Exception("Ran out of space in an OGG packet");
                }

                this._pageBuffer[HEADER_SIZE + MAX_SEGMENTS + this._payloadBytes] = this._bitBuffer;
                this._payloadBytes++;

                this._bitBuffer = 0;
                this._bitsStored = 0;
            }
        }

        internal void FlushPage(bool nextContinued = false, bool last = false)
        {
            if (this._payloadBytes != MAX_SEGMENTS * SEGMENT_SIZE)
            {
                FlushBits();
            }

            if (this._payloadBytes != 0)
            {
                uint segments = (this._payloadBytes + SEGMENT_SIZE) / SEGMENT_SIZE;
                if (segments == MAX_SEGMENTS + 1)
                {
                    segments = MAX_SEGMENTS;
                }

                for (int i = 0; i < this._payloadBytes; i++)
                {
                    this._pageBuffer[HEADER_SIZE + segments + i] = this._pageBuffer[HEADER_SIZE + MAX_SEGMENTS + i];
                }

                this._pageBuffer[0] = (byte)'O';
                this._pageBuffer[1] = (byte)'g';
                this._pageBuffer[2] = (byte)'g';
                this._pageBuffer[3] = (byte)'S';
                this._pageBuffer[4] = 0;
                this._pageBuffer[5] = (byte)((this._continued ? 1 : 0) | (this._first ? 2 : 0) | (last ? 4 : 0));
                Buffer.BlockCopy(BitConverter.GetBytes(this._granule), 0, this._pageBuffer, 6, 4);

                if (this._granule == 0xFFFFFFFF)
                {
                    this._pageBuffer[10] = 0xFF;
                    this._pageBuffer[11] = 0xFF;
                    this._pageBuffer[12] = 0xFF;
                    this._pageBuffer[13] = 0xFF;
                }
                else
                {
                    this._pageBuffer[10] = 0;
                    this._pageBuffer[11] = 0;
                    this._pageBuffer[12] = 0;
                    this._pageBuffer[13] = 0;
                }

                this._pageBuffer[14] = 1;
                Buffer.BlockCopy(BitConverter.GetBytes(this._sequenceNumber), 0, this._pageBuffer, 18, 4);
                this._pageBuffer[22] = 0;
                this._pageBuffer[23] = 0;
                this._pageBuffer[24] = 0;
                this._pageBuffer[25] = 0;
                this._pageBuffer[26] = (byte)segments;

                for (uint i = 0, bytesLeft = this._payloadBytes; i < segments; i++)
                {
                    if (bytesLeft >= SEGMENT_SIZE)
                    {
                        bytesLeft -= SEGMENT_SIZE;
                        this._pageBuffer[27 + i] = (byte)SEGMENT_SIZE;
                    }
                    else
                    {
                        this._pageBuffer[27 + i] = (byte)bytesLeft;
                    }
                }

                Buffer.BlockCopy(BitConverter.GetBytes(CRC32.Compute(this._pageBuffer, HEADER_SIZE + segments + this._payloadBytes)), 0, this._pageBuffer, 22, 4);

                for (int i = 0; i < HEADER_SIZE + segments + this._payloadBytes; i++)
                {
                    Write(this._pageBuffer[i]);
                }

                this._sequenceNumber++;
                this._first = false;
                this._continued = nextContinued;
                this._payloadBytes = 0;
            }
        }

        internal void BitWrite(byte value, byte bitCount = 8)
        {
            BitWrite(value, (int)bitCount);
        }

        internal void BitWrite(ushort value, byte bitCount = 16)
        {
            BitWrite(value, (int)bitCount);
        }

        internal void BitWrite(uint value, byte bitCount = 32)
        {
            BitWrite(value, (int)bitCount);
        }

        private void BitWrite(uint value, int size)
        {
            for (int i = 0; i < size; i++)
            {
                WriteBit((value & (1 << i)) != 0 ? (byte)1 : (byte)0);
            }
        }

        internal void WriteVorbisHeader(byte type)
        {
            byte[] vorbisString = Encoding.UTF8.GetBytes("vorbis");

            BitWrite(type);
            for (int i = 0; i < vorbisString.Length; i++)
            {
                BitWrite(vorbisString[i]);
            }
        }

        internal void SetGranule(uint granule)
        {
            this._granule = granule;
        }
    }
}
