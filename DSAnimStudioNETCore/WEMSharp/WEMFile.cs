using System;
using System.IO;
using System.Linq;
using System.Text;

namespace WEMSharp
{
    public class WEMFile
    {
        public enum CodecTypes : ushort
        {
            PCM_Old = 0x0001,
            IMA = 0x0002,
            IMA_Old = 0x0069,
            XWMA_WMAv2 = 0x0161,
            XWMA_WMAPro = 0x0162,
            XMA2_XMA2Chunk = 0x0165,
            XMA2_FMTChunk = 0x0166,
            OPUSNX = 0x3039,
            OPUS = 0x3040,
            OPUSWW = 0x3041,
            PTADPCM = 0x8311,
            AAC = 0xAAC0,
            DSP = 0xFFF0,
            HEVAG = 0xFFFB,
            ATRAC9 = 0xFFFC,
            PCM = 0xFFFE,
            VORBIS = 0xFFFF,
        }

        private Stream _wemFileStream;

        public CodecTypes CodecType { get; private set; }

        private uint _fmtChunkOffset = 0xFFFFFFFF;
        private uint _fmtChunkSize = 0xFFFFFFFF;
        private uint _cueChunkOffset = 0xFFFFFFFF;
        private uint _cueChunkSize = 0xFFFFFFFF;
        private uint _listChunkOffset = 0xFFFFFFFF;
        private uint _listChunkSize = 0xFFFFFFFF;
        private uint _smplChunkOffset = 0xFFFFFFFF;
        private uint _smplChunkSize = 0xFFFFFFFF;
        private uint _vorbChunkOffset = 0xFFFFFFFF;
        private uint _vorbChunkSize = 0xFFFFFFFF;
        private uint _dataChunkOffset = 0xFFFFFFFF;
        private uint _dataChunkSize = 0xFFFFFFFF;

        /// <summary>
        /// Channel Count
        /// </summary>
        public ushort Channels { get; private set; }
        /// <summary>
        /// Sample Rate
        /// </summary>
        public uint SamplesPerSec { get; private set; }
        public uint AvgBytesPerSecond { get; private set; }
        public ushort BlockAlign { get; private set; }
        public ushort BitsPerSample { get; private set; }
        private ushort _fmtExtensionSize;

        public uint LoopCount => _loopCount;
        public uint LoopEnabled => _loopEnabled;
        public uint LoopStart => _loopStart;
        public uint LoopEnd => _loopEnd;

        public uint SampleCount => _sampleCount;

        private uint _cueCount;
        private uint _loopCount;
        private uint _loopEnabled;
        private uint _loopStart;
        
        private uint _loopEnd;
        private uint _sampleCount;
        private bool _noGranule;
        private bool _modPackets;
        private uint _setupPacketOffset;
        private uint _firstAudioPacketOffset;
        private bool _headerTriadPresent;
        private bool _oldPacketHeaders;
        private uint _uid;
        private byte _blocksize0Pow;
        private byte _blocksize1Pow;

        public WEMFile(Stream stream, WEMForcePacketFormat forcePacketFormat)
        {
            this._wemFileStream = stream;

            using (BinaryReader br = new BinaryReader(this._wemFileStream, Encoding.UTF8, true))
            {
                string magic = Encoding.ASCII.GetString(br.ReadBytes(4));
                if (magic != "RIFF")
                {
                    throw new Exception("This is either not a WEM file or is of an unsupported type");
                }

                uint riffSize = br.ReadUInt32() + 8;

                string waveMagic = Encoding.ASCII.GetString(br.ReadBytes(4));
                if (waveMagic != "WAVE")
                {
                    throw new Exception("Missing WAVE magic");
                }

                uint chunkOffset = 12;
                while (chunkOffset < riffSize)
                {
                    br.BaseStream.Seek(chunkOffset, SeekOrigin.Begin);

                    string chunkName = Encoding.ASCII.GetString(br.ReadBytes(4));
                    uint chunkSize = br.ReadUInt32();

                    if (chunkName.Substring(0, 3) == "fmt")
                    {
                        this._fmtChunkOffset = chunkOffset + 8;
                        this._fmtChunkSize = chunkSize;
                    }
                    else if (chunkName.Substring(0, 3) == "cue")
                    {
                        this._cueChunkOffset = chunkOffset + 8;
                        this._cueChunkSize = chunkSize;
                    }
                    else if (chunkName == "LIST")
                    {
                        this._listChunkOffset = chunkOffset + 8;
                        this._listChunkSize = chunkSize;
                    }
                    else if (chunkName == "smpl")
                    {
                        this._smplChunkOffset = chunkOffset + 8;
                        this._smplChunkSize = chunkSize;
                    }
                    else if (chunkName == "vorb")
                    {
                        this._vorbChunkOffset = chunkOffset + 8;
                        this._vorbChunkSize = chunkSize;
                    }
                    else if (chunkName == "data")
                    {
                        this._dataChunkOffset = chunkOffset + 8;
                        this._dataChunkSize = chunkSize;
                    }

                    chunkOffset += chunkSize + 8;
                }

                if (chunkOffset > riffSize)
                {
                    throw new Exception("There was an error reading the file");
                }

                if (this._fmtChunkOffset == 0xFFFFFFFF)
                {
                    throw new Exception("WEM - No FMT chunk, file unreadable.");
                }

                if (this._dataChunkOffset == 0xFFFFFFFF)
                {
                    throw new Exception("WEM - No DATA chunk, file unreadable.");
                }

                br.BaseStream.Seek(this._fmtChunkOffset, SeekOrigin.Begin);

                CodecType = (CodecTypes)br.ReadUInt16();
                Channels = br.ReadUInt16();
                SamplesPerSec = br.ReadUInt32();
                AvgBytesPerSecond = br.ReadUInt32();
                BlockAlign = br.ReadUInt16();
                BitsPerSample = br.ReadUInt16();
                _fmtExtensionSize = br.ReadUInt16();

                
                

                switch (CodecType)
                {
                    case CodecTypes.VORBIS:
                        ParseCodec_Vorbis(br, forcePacketFormat);
                        break;
                    case CodecTypes.PCM:
                        ParseCodec_PCM(br);
                        break;
                    default:
                        throw new NotImplementedException($"Code to parse WEM codec type '{CodecType}' not implemented yet.");
                }


                














                //Read CUE Chunk
                if (this._cueChunkOffset != 0xFFFFFFFF)
                {
                    br.BaseStream.Seek(this._cueChunkOffset, SeekOrigin.Begin);

                    this._cueCount = br.ReadUInt32();
                }

                //Read SMPL Chunk
                if (this._smplChunkOffset != 0xFFFFFFFF)
                {
                    br.BaseStream.Seek(this._smplChunkOffset, SeekOrigin.Begin);

                    this._loopCount = br.ReadUInt32();
                    //if (this._loopCount != 1)
                    //{
                    //    throw new Exception("SMPL Chunk - Wrong Loop Count");
                    //}

                    br.BaseStream.Seek(this._smplChunkOffset + 0x1C, SeekOrigin.Begin);

                    this._loopEnabled = br.ReadUInt32();

                    br.BaseStream.Seek(this._smplChunkOffset + 0x2C, SeekOrigin.Begin);

                    this._loopStart = br.ReadUInt32();
                    this._loopEnd = br.ReadUInt32();
                }
                //else
                //{
                //    _loopCount = 0;
                //}

                if (this._loopCount != 0)
                {
                    if (this._loopEnd == 0)
                    {
                        this._loopEnd = this._sampleCount;
                    }
                    else
                    {
                        this._loopEnd++;
                    }

                    if (this._loopStart >= this._sampleCount)
                    {
                        throw new Exception($"WEM - SMPL Chunk - Loop starts at sample {_loopStart}, " +
                            $"which is at the end of the file (sample {(_sampleCount)}) or after it.");
                    }

                    if (this._loopEnd > this._sampleCount)
                    {
                        throw new Exception($"WEM - SMPL Chunk - Loop ends at sample {_loopEnd}, " +
                            $"which is after the end of the file (sample {(_sampleCount)}).");
                    }

                    if (this._loopStart > this._loopEnd)
                    {
                        throw new Exception($"WEM - SMPL Chunk - Loop starts at sample {_loopStart}, " +
                            $"which is after the loop end (sample {(_loopEnd)}).");
                    }
                }
            }
        }

        private void ParseCodec_PCM(BinaryReader br)
        {
            _sampleCount = (uint)((_dataChunkSize / Channels) / (BitsPerSample / 8));
        }

        private void ParseCodec_Vorbis(BinaryReader br, WEMForcePacketFormat forcePacketFormat)
        {
            if (BlockAlign != 0)
            {
                throw new Exception("WEM - Vorbis codec - FMT Chunk - Wrong Block Align");
            }
            if (BitsPerSample != 0)
            {
                throw new Exception("WEM - Vorbis codec - FMT Chunk - Wrong Bits Per Sample");
            }
            if (_fmtExtensionSize != (_fmtChunkSize - 0x12))
            {
                throw new Exception($"WEM - Vorbis codec - FMT Chunk - " +
                    $"Extra FMT Chunk Size was 0x{_fmtExtensionSize:X2}, expected 0x{(_fmtChunkSize - 0x12):X2}");
            }


            if (_vorbChunkOffset == 0xFFFFFFFF)
            {
                if (_fmtChunkSize == 0x42)
                {
                    this._vorbChunkOffset = this._fmtChunkOffset + 0x18;
                }
                else
                {
                    throw new Exception($"WEM - Vorbis codec - Since a VORB chunk was not present, " +
                        $"the FMT chunk size was expected to be 0x42, but it was 0x{_fmtChunkSize:X2}");
                    //throw new Exception($"WEM - Vorbis codec - Unknown combination of " +
                    //    $"{nameof(_vorbChunkOffset)}==0x{_vorbChunkOffset:X8} and " +
                    //    $"{nameof(_fmtChunkSize)}==0x{_fmtChunkSize:X2} " +
                    //    $"--> Expected {nameof(_fmtChunkSize)}==0x42 if {nameof(_vorbChunkOffset)}==0xFFFFFFFF");
                }
            }
            else
            {
                if (!(this._fmtChunkSize == 0x28 || this._fmtChunkSize == 0x18 || this._fmtChunkSize == 0x12))
                {
                    throw new Exception($"WEM - Vorbis codec - Since a VORB chunk was present, " +
                        $"the FMT chunk size was expected to be 0x12, 0x18, or 0x28, but it was 0x{_fmtChunkSize:X2}");
                }
            }






            if (this._fmtChunkSize == 0x28)
            {
                byte[] unknownBufferCheck = new byte[] { 1, 0, 0, 0, 0, 0, 0x10, 0, 0x80, 0, 0, 0xAA, 0, 0x38, 0x9b, 0x71 };
                byte[] unknownBuffer;

                unknownBuffer = br.ReadBytes(16);

                if (unknownBuffer.SequenceEqual(unknownBufferCheck))
                {
                    throw new Exception("WEM - Vorbis codec - FMT Chunk - Wrong Unknown Buffer Signature");
                }
            }



            //Read VORB Chunk
            switch (this._vorbChunkSize)
            {
                case 0xFFFFFFFF:
                case 0x28:
                case 0x2A:
                case 0x2C:
                case 0x32:
                case 0x34:
                    br.BaseStream.Seek(this._vorbChunkOffset, SeekOrigin.Begin);
                    break;
                default:
                    throw new Exception($"WEM - Vorbis codec - VORB Chunk - VORB Chunk Size was 0x{_vorbChunkSize:X2}, " +
                        "expected 0xFFFFFFFF, 0x28, 0x2A, 0x2C, 0x32, or 0x34");
            }

            this._sampleCount = br.ReadUInt32();

            switch (this._vorbChunkSize)
            {
                case 0xFFFFFFFF:
                case 0x2A:
                    {
                        this._noGranule = true;

                        br.BaseStream.Seek(this._vorbChunkOffset + 4, SeekOrigin.Begin);

                        uint modSignal = br.ReadUInt32();

                        if (modSignal != 0x4A && modSignal != 0x4B && modSignal != 0x69 && modSignal != 0x70)
                        {
                            this._modPackets = true;
                        }

                        br.BaseStream.Seek(this._vorbChunkOffset + 0x10, SeekOrigin.Begin);

                        break;
                    }

                default:
                    br.BaseStream.Seek(this._vorbChunkOffset + 0x18, SeekOrigin.Begin);
                    break;
            }

            if (forcePacketFormat == WEMForcePacketFormat.ForceNoModPackets)
            {
                this._modPackets = false;
            }
            else if (forcePacketFormat == WEMForcePacketFormat.ForceModPackets)
            {
                this._modPackets = true;
            }

            this._setupPacketOffset = br.ReadUInt32();
            this._firstAudioPacketOffset = br.ReadUInt32();

            switch (this._vorbChunkSize)
            {
                case 0xFFFFFFFF:
                case 0x2A:
                    br.BaseStream.Seek(this._vorbChunkOffset + 0x24, SeekOrigin.Begin);
                    break;

                case 0x32:
                case 0x34:
                    br.BaseStream.Seek(this._vorbChunkOffset + 0x2C, SeekOrigin.Begin);
                    break;
            }

            switch (this._vorbChunkSize)
            {
                case 0x28:
                case 0x2C:
                    this._headerTriadPresent = true;
                    this._oldPacketHeaders = true;
                    break;

                case 0xFFFFFFFF:
                case 0x2A:
                case 0x32:
                case 0x34:
                    this._uid = br.ReadUInt32();
                    this._blocksize0Pow = br.ReadByte();
                    this._blocksize1Pow = br.ReadByte();
                    break;
            }

            

        }


        public byte[] GenerateOGG(string codebooksLocation, bool inlineCodebooks, bool fullSetup)
        {
            if (inlineCodebooks && !string.IsNullOrEmpty(codebooksLocation))
            {
                throw new ArgumentException("Only one of Inline Codebooks or Codebooks Location can be set");
            }
            else if (!inlineCodebooks && string.IsNullOrEmpty(codebooksLocation))
            {
                throw new ArgumentException("Either Inline Codebooks or Codebooks Location must be set");
            }
            byte[] result = null;
            using (var memStream = new MemoryStream())
            {
                using (var ogg = new OggStream(memStream))
                {
                    bool[] modeBlockFlag = null;
                    uint modeBits = 0;
                    bool previousBlockFlag = false;

                    if (this._headerTriadPresent)
                    {
                        GenerateOGGHeaderTriad(ogg);
                    }
                    else
                    {
                        GenerateOGGHeader(ogg, codebooksLocation, inlineCodebooks, fullSetup, ref modeBlockFlag, ref modeBits);
                    }

                    {
                        uint offset = this._dataChunkOffset + this._firstAudioPacketOffset;

                        while (offset < this._dataChunkOffset + this._dataChunkSize)
                        {
                            uint size;
                            uint granule;
                            uint packetHeaderSize;
                            uint packetPayloadOffset;
                            uint nextOffset;

                            if (this._oldPacketHeaders)
                            {
                                Packet8 audioPacket = new Packet8(this._wemFileStream, offset);
                                packetHeaderSize = audioPacket.GetHeaderSize();
                                size = audioPacket.GetSize();
                                packetPayloadOffset = audioPacket.GetOffset();
                                granule = audioPacket.GetGranule();
                                nextOffset = audioPacket.NextOffset();
                            }
                            else
                            {
                                Packet audioPacket = new Packet(this._wemFileStream, offset, this._noGranule);
                                packetHeaderSize = audioPacket.GetHeaderSize();
                                size = audioPacket.GetSize();
                                packetPayloadOffset = audioPacket.GetOffset();
                                granule = audioPacket.GetGranule();
                                nextOffset = audioPacket.NextOffset();
                            }

                            if (offset + packetHeaderSize > this._dataChunkOffset + this._dataChunkSize)
                            {
                                throw new Exception("There was an error generating a vorbis packet");
                            }

                            offset = packetPayloadOffset;

                            this._wemFileStream.Seek(offset, SeekOrigin.Begin);

                            if (granule == 0xFFFFFFFF)
                            {
                                ogg.SetGranule(1);
                            }
                            else
                            {
                                ogg.SetGranule(granule);
                            }

                            if (this._modPackets)
                            {
                                if (modeBlockFlag == null)
                                {
                                    throw new Exception("There was an error generating a vorbis packet");
                                }

                                byte packetType = 0;
                                ogg.WriteBit(packetType);

                                uint modeNumber = 0;
                                uint remainder = 0;

                                {
                                    BitStream bitStream = new BitStream(this._wemFileStream);

                                    modeNumber = bitStream.Read((int)modeBits);
                                    ogg.BitWrite(modeNumber, (byte)modeBits);

                                    remainder = bitStream.Read(8 - (int)modeBits);
                                }

                                if (modeBlockFlag[modeNumber])
                                {
                                    this._wemFileStream.Seek(nextOffset, SeekOrigin.Begin);

                                    bool nextBlockFlag = false;
                                    if (nextOffset + packetHeaderSize <= this._dataChunkOffset + this._dataChunkSize)
                                    {
                                        Packet audioPacket = new Packet(this._wemFileStream, nextOffset, this._noGranule);
                                        uint nextPacketSize = audioPacket.GetSize();

                                        if (nextPacketSize != 0xFFFFFFFF)
                                        {
                                            this._wemFileStream.Seek(audioPacket.GetOffset(), SeekOrigin.Begin);

                                            BitStream bitStream = new BitStream(this._wemFileStream);
                                            uint nextModeNumber = bitStream.Read((int)modeBits);

                                            nextBlockFlag = modeBlockFlag[nextModeNumber];
                                        }
                                    }

                                    byte previousWindowType = previousBlockFlag ? (byte)1 : (byte)0;
                                    ogg.WriteBit(previousWindowType);

                                    byte nextWindowType = previousBlockFlag ? (byte)1 : (byte)0;
                                    ogg.WriteBit(nextWindowType);

                                    this._wemFileStream.Seek(offset + 1, SeekOrigin.Begin);
                                }

                                previousBlockFlag = modeBlockFlag[modeNumber];
                                ogg.BitWrite(remainder, (byte)(8 - modeBits));
                            }
                            else
                            {
                                int b = this._wemFileStream.ReadByte();
                                if (b < 0)
                                {
                                    throw new Exception("There was an error generating a vorbis packet");
                                }

                                ogg.BitWrite((byte)b);
                            }

                            for (int i = 1; i < size; i++)
                            {
                                int b = this._wemFileStream.ReadByte();
                                if (b < 0)
                                {
                                    throw new Exception("There was an error generating a vorbis packet");
                                }

                                ogg.BitWrite((byte)b);
                            }

                            offset = nextOffset;
                            ogg.FlushPage(false, (offset == this._dataChunkOffset + this._dataChunkSize));
                        }

                        if (offset > this._dataChunkOffset + this._dataChunkSize)
                        {
                            throw new Exception("There was an error while creating the OGG file");
                        }
                    }

                    result = memStream.ToArray();
                }
            }

            return result;
        }

        private void GenerateOGGHeaderTriad(OggStream ogg)
        {
            uint offset = this._dataChunkOffset + this._setupPacketOffset;

            //Information Packet
            {
                Packet8 informationPacket = new Packet8(this._wemFileStream, offset);
                uint informationPacketSize = informationPacket.GetSize();

                if (informationPacket.GetGranule() != 0)
                {
                    throw new Exception("There was an error while creating the header");
                }

                this._wemFileStream.Seek(informationPacket.GetOffset(), SeekOrigin.Begin);

                byte[] informationPacketType = new byte[1];
                this._wemFileStream.Read(informationPacketType, 0, 1);
                if (informationPacketType[0] != 1)
                {
                    throw new Exception("There was an error while creating the header");
                }
                ogg.BitWrite(informationPacketType[0]);

                byte[] b = new byte[1];
                for (int i = 0; i < informationPacketSize; i++)
                {
                    this._wemFileStream.Read(b, 0, 1);
                    ogg.BitWrite(b[0]);
                }

                ogg.FlushPage();
                offset = informationPacket.NextOffset();
            }

            //Comment Packet
            {
                Packet8 commentPacket = new Packet8(this._wemFileStream, offset);
                uint commentPacketSize = commentPacket.GetSize();

                if (commentPacket.GetGranule() != 0)
                {
                    throw new Exception("There was an error while creating the header");
                }

                this._wemFileStream.Seek(commentPacket.GetOffset(), SeekOrigin.Begin);

                byte[] commentPacketType = new byte[1];
                this._wemFileStream.Read(commentPacketType, 0, 1);
                if (commentPacketType[0] != 3)
                {
                    throw new Exception("There was an error while creating the header");
                }
                ogg.BitWrite(commentPacketType[0]);

                byte[] b = new byte[1];
                for (int i = 0; i < commentPacketSize; i++)
                {
                    this._wemFileStream.Read(b, 0, 1);
                    ogg.BitWrite(b[0]);
                }

                ogg.FlushPage();
                offset = commentPacket.NextOffset();
            }

            //Setup Packet
            {
                Packet8 setupPacket = new Packet8(this._wemFileStream, offset);
                this._wemFileStream.Seek(setupPacket.GetOffset(), SeekOrigin.Begin);

                if (setupPacket.GetGranule() != 0)
                {
                    throw new Exception("There was an error while creating the header");
                }

                BitStream bitStream = new BitStream(this._wemFileStream);
                byte setupPacketType = (byte)bitStream.Read(8);
                if (setupPacketType != 5)
                {
                    throw new Exception("There was an error while creating the header");
                }
                ogg.BitWrite(setupPacketType);

                for (int i = 0; i < 6; i++)
                {
                    ogg.BitWrite((byte)bitStream.Read(8));
                }

                byte codebookCount = (byte)bitStream.Read(8);
                ogg.BitWrite(codebookCount);
                codebookCount++;

                CodebookLibrary codebook = new CodebookLibrary();
                for (int i = 0; i < codebookCount; i++)
                {
                    codebook.Copy(bitStream, ogg);
                }

                while (bitStream.TotalBitsRead < setupPacket.GetSize() * 8)
                {
                    ogg.WriteBit((byte)bitStream.Read(1));
                }

                ogg.FlushPage();
                offset = setupPacket.NextOffset();
            }

            if (offset != this._dataChunkOffset + this._firstAudioPacketOffset)
            {
                throw new Exception("There was an error while creating the header");
            }
        }

        private void GenerateOGGHeader(OggStream ogg, string codebooksLocation, bool inlineCodebooks, bool fullSetup, ref bool[] modeBlockFlag, ref uint modeBits)
        {
            ogg.WriteVorbisHeader(1);
            ogg.BitWrite((uint)0);
            ogg.BitWrite((byte)this.Channels);
            ogg.BitWrite(this.SamplesPerSec);
            ogg.BitWrite((uint)0);
            ogg.BitWrite(this.AvgBytesPerSecond * 8);
            ogg.BitWrite((uint)0);
            ogg.BitWrite(this._blocksize0Pow, 4);
            ogg.BitWrite(this._blocksize1Pow, 4);
            ogg.WriteBit(1);
            ogg.FlushPage();

            //Comment Packet
            {
                ogg.WriteVorbisHeader(3);

                byte[] vendor = Encoding.UTF8.GetBytes("Converted from Audiokinetic Wwise by WEMSharp");
                ogg.BitWrite((uint)vendor.Length);
                for (int i = 0; i < vendor.Length; i++)
                {
                    ogg.BitWrite(vendor[i]);
                }

                if (this._loopCount == 0)
                {
                    ogg.BitWrite((uint)0);
                }
                else
                {
                    ogg.BitWrite((uint)2);

                    byte[] loopStart = Encoding.UTF8.GetBytes("LoopStart=" + this._loopStart.ToString());
                    ogg.BitWrite((uint)loopStart.Length);
                    for (int i = 0; i < loopStart.Length; i++)
                    {
                        ogg.BitWrite(loopStart[i]);
                    }

                    byte[] loopEnd = Encoding.UTF8.GetBytes("LoopEnd=" + this._loopEnd.ToString());
                    ogg.BitWrite((uint)loopEnd.Length);
                    for (int i = 0; i < loopEnd.Length; i++)
                    {
                        ogg.BitWrite(loopEnd[i]);
                    }
                }

                ogg.WriteBit(1);
                ogg.FlushPage();
            }

            //Setup Packet
            {
                ogg.WriteVorbisHeader(5);

                Packet setupPacket = new Packet(this._wemFileStream, this._dataChunkOffset + this._setupPacketOffset, this._noGranule);

                this._wemFileStream.Seek(setupPacket.GetOffset(), SeekOrigin.Begin);

                if (setupPacket.GetGranule() != 0)
                {
                    throw new Exception("There was an error generating a vorbis packet");
                }

                BitStream bitStream = new BitStream(this._wemFileStream);

                uint codebookCount = bitStream.Read(8);
                ogg.BitWrite((byte)codebookCount);
                codebookCount++;

                if (inlineCodebooks)
                {
                    CodebookLibrary codebook = new CodebookLibrary();

                    for (int i = 0; i < codebookCount; i++)
                    {
                        if (fullSetup)
                        {
                            codebook.Copy(bitStream, ogg);
                        }
                        else
                        {
                            codebook.Rebuild(bitStream, 0, ogg);
                        }
                    }
                }
                else
                {
                    CodebookLibrary codebook = new CodebookLibrary(codebooksLocation);

                    for (int i = 0; i < codebookCount; i++)
                    {
                        ushort codebookID = (ushort)bitStream.Read(10);
                        codebook.Rebuild(codebookID, ogg);
                    }
                }

                byte timeCountLess1 = 0;
                ushort dummyTimeValue = 0;
                ogg.BitWrite(timeCountLess1, 6);
                ogg.BitWrite(dummyTimeValue);

                if (fullSetup)
                {
                    while (bitStream.TotalBitsRead < setupPacket.GetSize() * 8U)
                    {
                        ogg.WriteBit((byte)bitStream.Read(1));
                    }
                }
                else
                {
                    byte floorCount = (byte)bitStream.Read(6);
                    ogg.BitWrite(floorCount, 6);
                    floorCount++;

                    for (int i = 0; i < floorCount; i++)
                    {
                        ushort floorType = 1;
                        ogg.BitWrite(floorType);

                        byte floor1Partitions = (byte)bitStream.Read(5);
                        ogg.BitWrite(floor1Partitions, 5);

                        uint[] floor1PartitionClassList = new uint[floor1Partitions];
                        uint maximumClass = 0;
                        for (int j = 0; j < floor1Partitions; j++)
                        {
                            byte floor1PartitionClass = (byte)bitStream.Read(4);
                            ogg.BitWrite(floor1PartitionClass, 4);

                            floor1PartitionClassList[j] = floor1PartitionClass;

                            if (floor1PartitionClass > maximumClass)
                            {
                                maximumClass = floor1PartitionClass;
                            }
                        }

                        uint[] floor1ClassDimensionList = new uint[maximumClass + 1];
                        for (int j = 0; j <= maximumClass; j++)
                        {
                            byte classDimension = (byte)bitStream.Read(3);
                            ogg.BitWrite(classDimension, 3);

                            floor1ClassDimensionList[j] = classDimension + 1U;

                            byte classSubclasses = (byte)bitStream.Read(2);
                            ogg.BitWrite(classSubclasses, 2);

                            if (classSubclasses != 0)
                            {
                                byte masterbook = (byte)bitStream.Read(8);
                                ogg.BitWrite(masterbook);

                                if (maximumClass >= codebookCount)
                                {
                                    throw new Exception("There was an error generating a vorbis packet.");
                                }
                            }

                            for (int k = 0; k < (1 << classSubclasses); k++)
                            {
                                byte subclassBook = (byte)bitStream.Read(8);
                                ogg.BitWrite(subclassBook);

                                if ((subclassBook - 1) >= 0 && (subclassBook - 1) >= codebookCount)
                                {
                                    throw new Exception("There was an error generating a vorbis packet.");
                                }
                            }
                        }

                        byte floor1Multiplier = (byte)bitStream.Read(2);
                        ogg.BitWrite(floor1Multiplier, 2);

                        byte rangeBits = (byte)bitStream.Read(4);
                        ogg.BitWrite(rangeBits, 4);

                        for (int j = 0; j < floor1Partitions; j++)
                        {
                            uint currentClassNumber = floor1PartitionClassList[j];
                            for (int k = 0; k < floor1ClassDimensionList[currentClassNumber]; k++)
                            {
                                ogg.BitWrite(bitStream.Read(rangeBits), rangeBits);
                            }
                        }
                    }

                    byte residueCount = (byte)bitStream.Read(6);
                    ogg.BitWrite(residueCount, 6);
                    residueCount++;

                    for (int i = 0; i < residueCount; i++)
                    {
                        byte residueType = (byte)bitStream.Read(2);
                        ogg.BitWrite((ushort)residueType);

                        if (residueType > 2)
                        {
                            throw new Exception("There was an error generating a vorbis packet.");
                        }

                        uint residueBegin = bitStream.Read(24);
                        uint residueEnd = bitStream.Read(24);
                        uint residuePartitionSize = bitStream.Read(24);
                        byte residueClassifications = (byte)bitStream.Read(6);
                        byte residueClassbook = (byte)bitStream.Read(8);

                        ogg.BitWrite(residueBegin, 24);
                        ogg.BitWrite(residueEnd, 24);
                        ogg.BitWrite(residuePartitionSize, 24);
                        ogg.BitWrite(residueClassifications, 6);
                        ogg.BitWrite(residueClassbook, 8);

                        residueClassifications++;

                        if (residueClassbook >= codebookCount)
                        {
                            throw new Exception("There was an error generating a vorbis packet.");
                        }

                        uint[] residueCascade = new uint[residueClassifications];
                        for (int j = 0; j < residueClassifications; j++)
                        {
                            byte highBits = 0;
                            byte lowBits = (byte)bitStream.Read(3);

                            ogg.BitWrite(lowBits, 3);

                            byte bitFlag = (byte)bitStream.Read(1);
                            ogg.WriteBit(bitFlag);

                            if (bitFlag == 1)
                            {
                                highBits = (byte)bitStream.Read(5);
                                ogg.BitWrite(highBits, 5);
                            }

                            residueCascade[j] = highBits * 8U + lowBits;
                        }

                        for (int j = 0; j < residueClassifications; j++)
                        {
                            for (int k = 0; k < 8; k++)
                            {
                                if ((residueCascade[j] & (1 << k)) != 0)
                                {
                                    byte residueBook = (byte)bitStream.Read(8);
                                    ogg.BitWrite(residueBook);

                                    if (residueBook >= codebookCount)
                                    {
                                        throw new Exception("There was an error generating a vorbis packet.");
                                    }
                                }
                            }
                        }
                    }

                    byte mappingCount = (byte)bitStream.Read(6);
                    ogg.BitWrite(mappingCount, 6);
                    mappingCount++;

                    for (int i = 0; i < mappingCount; i++)
                    {
                        ushort mappingType = 0;
                        ogg.BitWrite(mappingType);

                        byte submapsFlag = (byte)bitStream.Read(1);
                        ogg.WriteBit(submapsFlag);

                        uint submaps = 1;
                        if (submapsFlag == 1)
                        {
                            byte submapsLess = (byte)bitStream.Read(4);
                            submaps = submapsLess + 1U;
                            ogg.BitWrite(submapsLess, 4);
                        }

                        byte squarePolarFlag = (byte)bitStream.Read(1);
                        ogg.WriteBit(squarePolarFlag);

                        if (squarePolarFlag == 1)
                        {
                            byte couplingSteps = (byte)bitStream.Read(8);
                            ogg.BitWrite(couplingSteps);
                            couplingSteps++;

                            for (int j = 0; j < couplingSteps; j++)
                            {
                                uint magnitude = bitStream.Read((int)ILog(this.Channels - 1U));
                                uint angle = bitStream.Read((int)ILog(this.Channels - 1U));

                                ogg.BitWrite(magnitude, (byte)ILog(this.Channels - 1U));
                                ogg.BitWrite(angle, (byte)ILog(this.Channels - 1U));

                                if (angle == magnitude || magnitude >= this.Channels || angle >= this.Channels)
                                {
                                    throw new Exception("There was an error generating a vorbis packet.");
                                }
                            }
                        }

                        byte mappingReserved = (byte)bitStream.Read(2);
                        ogg.BitWrite(mappingReserved, 2);

                        if (mappingReserved != 0)
                        {
                            throw new Exception("There was an error generating a vorbis packet.");
                        }

                        if (submaps > 1)
                        {
                            for (int j = 0; j < this.Channels; j++)
                            {
                                byte mappingMux = (byte)bitStream.Read(4);
                                ogg.BitWrite(mappingMux, 4);

                                if (mappingMux >= submaps)
                                {
                                    throw new Exception("There was an error generating a vorbis packet.");
                                }
                            }
                        }

                        for (int j = 0; j < submaps; j++)
                        {
                            byte timeConfig = (byte)bitStream.Read(8);
                            ogg.BitWrite(timeConfig);

                            byte floorNumber = (byte)bitStream.Read(8);
                            ogg.BitWrite(floorNumber);
                            if (floorNumber >= floorCount)
                            {
                                throw new Exception("There was an error generating a vorbis packet.");
                            }

                            byte residueNumber = (byte)bitStream.Read(8);
                            ogg.BitWrite(residueNumber);
                            if (residueNumber >= residueCount)
                            {
                                throw new Exception("There was an error generating a vorbis packet.");
                            }
                        }
                    }

                    byte modeCount = (byte)bitStream.Read(6);
                    ogg.BitWrite(modeCount, 6);
                    modeCount++;

                    modeBlockFlag = new bool[modeCount];
                    modeBits = ILog(modeCount - 1U);

                    for (int i = 0; i < modeCount; i++)
                    {
                        byte blockFlag = (byte)bitStream.Read(1);
                        ogg.WriteBit(blockFlag);

                        modeBlockFlag[i] = blockFlag == 1;

                        ushort windowType = 0;
                        ushort transformType = 0;
                        ogg.BitWrite(windowType);
                        ogg.BitWrite(transformType);

                        byte mapping = (byte)bitStream.Read(8);
                        ogg.BitWrite(mapping, 8);

                        if (mapping >= mappingCount)
                        {
                            throw new Exception("There was an error generating a vorbis packet.");
                        }
                    }

                    ogg.WriteBit(1);
                }

                ogg.FlushPage();

                if ((bitStream.TotalBitsRead + 7) / 8 != setupPacket.GetSize())
                {
                    throw new Exception("There was an error generating a vorbis packet.");
                }

                if (setupPacket.NextOffset() != this._dataChunkOffset + this._firstAudioPacketOffset)
                {
                    throw new Exception("There was an error generating a vorbis packet.");
                }
            }
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
    }

    /// <summary>
    /// Forcing of the OGG Packet Format
    /// </summary>
    public enum WEMForcePacketFormat
    {
        /// <summary>
        /// Uses the original Mod Packet Format from the WEM file
        /// </summary>
        NoForcePacketFormat,
        /// <summary>
        /// Forces to modify the original packets
        /// </summary>
        ForceModPackets,
        /// <summary>
        /// Forces to not modify the original packets
        /// </summary>
        ForceNoModPackets
    }
}