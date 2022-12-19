using System;
using System.IO;
using System.Linq;

namespace WEMSharp
{
    internal class CodebookLibrary
    {
        public uint CodebookCount { get; private set; }
        private byte[] _codebookData;
        private uint[] _codebookOffsets;

        internal CodebookLibrary() { }

        internal CodebookLibrary(string fileLocation)
        {
            using (BinaryReader br = new BinaryReader(File.OpenRead(fileLocation)))
            {
                br.BaseStream.Seek(br.BaseStream.Length - 4, SeekOrigin.Begin);

                uint offsetOffset = br.ReadUInt32();
                this.CodebookCount = (uint)(br.BaseStream.Length - offsetOffset) / 4;
                this._codebookOffsets = new uint[this.CodebookCount];

                br.BaseStream.Seek(0, SeekOrigin.Begin);

                this._codebookData = br.ReadBytes((int)offsetOffset);

                for (int i = 0; i < this.CodebookCount; i++)
                {
                    this._codebookOffsets[i] = br.ReadUInt32();
                }
            }
        }

        internal void Copy(BitStream bitStream, OggStream ogg)
        {
            //https://xiph.org/vorbis/doc/Vorbis_I_spec.pdf

            byte[] id = new byte[]
            {
                (byte)bitStream.Read(8),
                (byte)bitStream.Read(8),
                (byte)bitStream.Read(8)
            };

            ushort dimensions = (ushort)bitStream.Read(16);
            uint entries = bitStream.Read(24);

            if (id[0] != 0x42 || id[1] != 0x43 || id[2] != 0x56)
            {
                throw new Exception("Invalid Codebook Sync Pattern");
            }

            ogg.BitWrite(id[0]);
            ogg.BitWrite(id[1]);
            ogg.BitWrite(id[2]);

            ogg.BitWrite(dimensions);
            ogg.BitWrite(entries, 24);

            byte orderedFlag = (byte)bitStream.Read(1);
            ogg.WriteBit(orderedFlag);
            if (orderedFlag == 1)
            {
                byte initialLength = (byte)bitStream.Read(5);
                ogg.BitWrite(initialLength, 5);

                uint currentEntry = 0;
                while (currentEntry < entries)
                {
                    uint bitCount = ILog(entries - currentEntry);
                    uint number = bitStream.Read((int)bitCount);
                    ogg.BitWrite(number, (byte)bitCount);
                    currentEntry += number;
                }

                if (currentEntry > entries)
                {
                    throw new Exception("There was an error copying a codebook");
                }
            }
            else
            {
                byte sparseFlag = (byte)bitStream.Read(1);
                ogg.WriteBit(sparseFlag);

                for (int i = 0; i < entries; i++)
                {
                    bool presentBool = true;

                    if (sparseFlag == 1)
                    {
                        byte sparsePresenceFlag = (byte)bitStream.Read(1);
                        ogg.WriteBit(sparsePresenceFlag);

                        presentBool = sparsePresenceFlag == 1;
                    }
                    else
                    {
                        byte codewordLength = (byte)bitStream.Read(5);
                        ogg.BitWrite(codewordLength, 5);
                    }
                }
            }

            byte lookupType = (byte)bitStream.Read(4);
            ogg.BitWrite(lookupType, 4);
            if (lookupType == 1)
            {
                ogg.BitWrite(bitStream.Read(32));
                ogg.BitWrite(bitStream.Read(32));
                byte valueLength = (byte)bitStream.Read(4);
                ogg.BitWrite(valueLength, 4);
                ogg.WriteBit((byte)bitStream.Read(1));

                uint quantValues = QuantValues(entries, dimensions);
                for (int i = 0; i < quantValues; i++)
                {
                    uint value = bitStream.Read(valueLength + 1);
                    ogg.BitWrite(value, (byte)(valueLength + 1));
                }
            }
            else
            {
                throw new Exception("There was an error copying a codebook");
            }
        }

        internal void Rebuild(uint index, OggStream ogg)
        {
            byte[] codebook = GetCodebook(index);
            uint codebookSize = GetCodebookSize(index);

            if (codebookSize == 0xFFFFFFFF || codebook == null)
            {
                throw new Exception("Invalid Codebook Index");
            }

            Rebuild(new BitStream(new MemoryStream(codebook)), codebookSize, ogg);
        }

        internal void Rebuild(BitStream bitStream, uint codebookSize, OggStream ogg)
        {
            byte dimensions = (byte)bitStream.Read(4);
            ushort entries = (ushort)bitStream.Read(14);

            ogg.BitWrite(0x564342, 24);
            ogg.BitWrite((ushort)dimensions, 16);
            ogg.BitWrite((uint)entries, 24);

            byte orderedFlag = (byte)bitStream.Read(1);
            ogg.WriteBit(orderedFlag);
            if (orderedFlag == 1)
            {
                byte initialLength = (byte)bitStream.Read(5);
                ogg.BitWrite(initialLength, 5);

                uint currentEntry = 0;
                while (currentEntry < entries)
                {
                    uint bitCount = ILog(entries - currentEntry);
                    uint number = bitStream.Read((int)bitCount);
                    ogg.BitWrite(number, (byte)bitCount);
                    currentEntry += number;
                }

                if (currentEntry > entries)
                {
                    throw new Exception("There was an error rebuilding a codebook");
                }
            }
            else
            {
                byte codewordLengthLength = (byte)bitStream.Read(3);
                byte sparseFlag = (byte)bitStream.Read(1);
                ogg.WriteBit(sparseFlag);

                if (codewordLengthLength == 0 || codewordLengthLength > 5)
                {
                    throw new Exception("There was an error rebuilding a codebook");
                }

                for (int i = 0; i < entries; i++)
                {
                    bool presentBool = true;

                    if (sparseFlag == 1)
                    {
                        byte sparsePresenceFlag = (byte)bitStream.Read(1);
                        ogg.WriteBit(sparsePresenceFlag);

                        presentBool = sparsePresenceFlag == 1;
                    }

                    if (presentBool)
                    {
                        byte codewordLength = (byte)bitStream.Read(codewordLengthLength);
                        ogg.BitWrite(codewordLength, 5);
                    }
                }
            }

            byte lookupType = (byte)bitStream.Read(1);
            ogg.BitWrite(lookupType, 4);
            if (lookupType == 1)
            {
                ogg.BitWrite(bitStream.Read(32));
                ogg.BitWrite(bitStream.Read(32));
                byte valueLength = (byte)bitStream.Read(4);
                ogg.BitWrite(valueLength, 4);
                ogg.WriteBit((byte)bitStream.Read(1));

                uint quantValues = QuantValues(entries, dimensions);
                for (int i = 0; i < quantValues; i++)
                {
                    uint value = bitStream.Read(valueLength + 1);
                    ogg.BitWrite(value, (byte)(valueLength + 1));
                }
            }
            else if (lookupType != 0)
            {
                throw new Exception("There was an error copying a codebook");
            }

            if (codebookSize != 0 && bitStream.TotalBitsRead / 8 + 1 != codebookSize)
            {
                throw new Exception("There was an error rebuilding a codebook");
            }
        }

        internal byte[] GetCodebook(uint index)
        {
            if (this._codebookData == null || this._codebookOffsets == null)
            {
                throw new Exception("This Codebook Library is not initialized");
            }

            if (index >= this.CodebookCount - 1)
            {
                return null;
            }

            return this._codebookData.ToList().GetRange((int)this._codebookOffsets[index], (int)GetCodebookSize(index)).ToArray();
        }

        internal uint GetCodebookSize(uint index)
        {
            if (this._codebookData == null || this._codebookOffsets == null)
            {
                throw new Exception("This Codebook Library is not initialized");
            }

            if (index >= this.CodebookCount - 1)
            {
                return 0xFFFFFFFF;
            }

            return this._codebookOffsets[index + 1] - this._codebookOffsets[index];
        }

        //https://xiph.org/vorbis/doc/Vorbis_I_spec.pdf#subsubsection.9.2.1
        private uint ILog(uint value)
        {
            uint ret = 0;

            while (value != 0)
            {
                ret++;
                value >>= 1;
            }

            return ret;
        }

        private uint QuantValues(uint entries, uint dimensions)
        {
            uint bits = ILog(entries);
            uint values = entries >> (int)((bits - 1) * (dimensions - 1) / dimensions);

            while (true)
            {
                ulong acc = 1;
                ulong acc1 = 1;
                for (int i = 0; i < dimensions; i++)
                {
                    acc *= values;
                    acc1 *= values + 1;
                }
                if (acc <= entries && acc1 > entries)
                {
                    return values;
                }
                else
                {
                    if (acc > entries)
                    {
                        values--;
                    }
                    else
                    {
                        values++;
                    }
                }
            }
        }
    }
}
