using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.Threading.Tasks;
using NVorbis;

namespace DSAnimStudio.DynamicSongSys
{
    // https://www.reddit.com/r/monogame/comments/9unc7d/music_with_partial_loop/
    public class DynamicSong
    {
        //public string Name;

        private float duration;
        private float bytesOverMilliseconds;

        private byte[] byteArray;
        //private int count;


        public bool LoopEnabled = false;
        public Int64 LoopStart = 0;
        public Int64 LoopLength = 0;
        public Int64 LoopEnd = 0;
        public long SampleCount;

        int chunkId;
        int fileSize;
        int riffType;
        int fmtId;
        int fmtSize;
        int fmtCode;

        int channels;
        int sampleRate;

        int fmtAvgBps;
        int fmtBlockAlign;
        int bitDepth;

        int fmtExtraSize;

        int dataID;
        int dataSize;

        const int bufferDuration = 100;

        // Private

        public DynamicSong(byte[] bytes, long sampleCount)
        {
            SampleCount = sampleCount;
            ReadOgg(bytes);
            //Name = path.Split("/").Last();
            //Name = Name.Split(".")[0];
        }

        public DynamicSongInstance CreateInstance()
        {
            DynamicSoundEffectInstance dynamicSound = new DynamicSoundEffectInstance(sampleRate, (AudioChannels)channels);

            

            var count = AlignTo8Bytes(dynamicSound.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(bufferDuration)) + 4);
            //loopLengthBytes = AlignTo8Bytes(dynamicSound.GetSampleSizeInBytes(TimeSpan.FromSeconds((double)LoopLength / sampleRate)));
            var loopStartBytes = (int)(LoopStart * 2);//dynamicSound.GetSampleSizeInBytes(TimeSpan.FromSeconds((double)LoopEnd / sampleRate)); // doesn't need alignment
            var loopEndBytes = (int)(LoopEnd * 2);//dynamicSound.GetSampleSizeInBytes(TimeSpan.FromSeconds((double)LoopEnd / sampleRate)); // doesn't need alignment

            return new DynamicSongInstance(dynamicSound, byteArray, count, loopStartBytes, loopEndBytes, bytesOverMilliseconds, LoopEnabled);
        }

        private static int AlignTo8Bytes(int unalignedBytes)
        {
            int result = unalignedBytes + 4;
            result -= (result % 8);
            return result;
        }

        private void ReadOgg(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                using (VorbisReader vorbis = new VorbisReader(ms))
                {
                    channels = vorbis.Channels;
                    sampleRate = vorbis.SampleRate;
                    duration = (float)vorbis.TotalTime.TotalMilliseconds;

                    TimeSpan totalTime = vorbis.TotalTime;

                    float[] buffer = new float[channels * sampleRate / 5];

                    List<byte> byteList = new List<byte>();
                    int totalCount = 0;
                    while (totalCount < SampleCount)
                    {
                        int samplesToRead = (int)Math.Min(buffer.Length, SampleCount - totalCount);
                        if (samplesToRead <= 0)
                            break;
                        var samplesRead = vorbis.ReadSamples(buffer, 0, samplesToRead);
                        if (samplesRead <= 0)
                            break;
                        for (int i = 0; i < samplesRead; i++)
                        {
                            short temp = (short)(32767f * buffer[i]);
                            if (temp > 32767)
                            {
                                byteList.Add(0xFF);
                                byteList.Add(0x7F);
                            }
                            else if (temp < -32768)
                            {
                                byteList.Add(0x80);
                                byteList.Add(0x00);
                            }
                            byteList.Add((byte)temp);
                            byteList.Add((byte)(temp >> 8));
                        }
                        totalCount += samplesRead;
                    }

                    byteArray = byteList.ToArray();
                    bytesOverMilliseconds = byteArray.Length / duration;

                    //var test = vorbis.Tags.All;

                    //Int64.TryParse(
                    //    vorbis.Comments.FirstOrDefault(c => c.Contains("LOOPSTART"))?.Split("LOOPSTART=")[1],
                    //    out LoopStart
                    //);

                    //Int64.TryParse(
                    //    vorbis.Comments.FirstOrDefault(c => c.Contains("LOOPLENGTH"))?.Split("LOOPLENGTH=")[1],
                    //    out LoopLength
                    //);

                    //Int64.TryParse(
                    //    vorbis.Comments.FirstOrDefault(c => c.Contains("LOOPEND"))?.Split("LOOPEND=")[1],
                    //    out LoopEnd
                    //);

                    //if (LoopStart != 0)
                    //{
                    //    if (LoopEnd == 0)
                    //    {
                    //        LoopEnd = ((Int64)duration * (Int64)sampleRate) / 1000;
                    //    }

                    //    if (LoopLength == 0)
                    //    {
                    //        LoopLength = LoopEnd - LoopStart;
                    //    }
                    //}
                }
            }
        }

        private void ReadWav(string path, string absolutePath)
        {
            byte[] allBytes = File.ReadAllBytes(absolutePath);
            int byterate = BitConverter.ToInt32(new[] { allBytes[28], allBytes[29], allBytes[30], allBytes[31] }, 0);
            duration = (int)Math.Floor(((float)(allBytes.Length - 8) / (float)(byterate)) * 1000);

            Stream waveFileStream = TitleContainer.OpenStream(path);
            BinaryReader reader = new BinaryReader(waveFileStream);

            chunkId = reader.ReadInt32();
            fileSize = reader.ReadInt32();
            riffType = reader.ReadInt32();
            fmtId = reader.ReadInt32();
            fmtSize = reader.ReadInt32();
            fmtCode = reader.ReadInt16();

            channels = reader.ReadInt16();
            sampleRate = reader.ReadInt32();

            fmtAvgBps = reader.ReadInt32();
            fmtBlockAlign = reader.ReadInt16();
            bitDepth = reader.ReadInt16();

            if (fmtSize == 18)
            {
                // Read any extra values
                fmtExtraSize = reader.ReadInt16();
                reader.ReadBytes(fmtExtraSize);
            }

            dataID = reader.ReadInt32();
            dataSize = reader.ReadInt32();

            byteArray = reader.ReadBytes(dataSize);
            bytesOverMilliseconds = byteArray.Length / duration;

            // Load metainfo, or specifically, TXXX "LOOP_____" tags

            char[] sectionHeader = new char[4];
            int sectionSize;
            long sectionBasePosition;

            char[] localSectionHeader = new char[4];
            int localSectionSize;
            Int16 localFlags;

            bool isData;
            char inChar;
            string tagTitle;
            string tagData;

            while (waveFileStream.Position < waveFileStream.Length - 10) // -10s are to prevent overrunning the end of the file when a partial header or filler bytes are present
            {
                sectionHeader = reader.ReadChars(4);
                sectionSize = reader.ReadInt32();
                sectionBasePosition = waveFileStream.Position;

                if (new string(sectionHeader) != "id3 ")
                {
                    waveFileStream.Position += sectionSize;
                    continue;
                }

                waveFileStream.Position += 10; // skip the header

                while ((waveFileStream.Position < sectionBasePosition + sectionSize - 10) && (waveFileStream.Position < waveFileStream.Length))
                {
                    localSectionHeader = reader.ReadChars(4);
                    localSectionSize = 0;
                    // need to read this as big-endian
                    for (int i = 0; i < 4; i++)
                    {
                        localSectionSize = (localSectionSize << 8) + reader.ReadByte();
                    }
                    localFlags = reader.ReadInt16(); // probably also needs endian swap... if we were paying attention to it, which we don't need to

                    if (new String(localSectionHeader) != "TXXX")
                    {
                        waveFileStream.Position += localSectionSize;
                        continue;
                    }

                    isData = false;
                    tagTitle = "";
                    tagData = "";

                    reader.ReadByte(); // text encoding byte, we're gonna just ignore this

                    for (int i = 0; i < localSectionSize - 1; i++) // -1 due to aforementioned ignored byte
                    {
                        inChar = reader.ReadChar();
                        if (isData)
                        {
                            tagData += inChar;
                        }
                        else if (inChar == '\x00')
                        {
                            isData = true;
                        }
                        else
                        {
                            tagTitle += inChar;
                        }
                    }

                    // Process specific tag types we're looking for. If you want to use this for general tag-reading, you'll need to implement that yourself,
                    // keeping in mind this code has also filtered for TXXX records only.

                    switch (tagTitle)
                    {
                        case "LOOPSTART":
                            Int64.TryParse(tagData, out LoopStart);
                            break;
                        case "LOOPLENGTH":
                            Int64.TryParse(tagData, out LoopLength);
                            break;
                        case "LOOPEND":
                            Int64.TryParse(tagData, out LoopEnd);
                            break;
                    }
                }

                if (LoopEnd == 0)
                {
                    LoopEnd = ((Int64)duration * (Int64)sampleRate) / 1000;
                }

                if (LoopLength == 0)
                {
                    LoopLength = LoopEnd - LoopStart;
                }
            }
        }
    }
}