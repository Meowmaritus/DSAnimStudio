using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsAssetPipeline.Animation.TAE;
using DSAnimStudio.TaeEditor;

namespace DSAnimStudio
{
    public partial class DSAProj
    {
        public struct TaeProperties
        {
            public Binder.FileFlags BindFlags;
            public string BindDirectory;
            public DCX.Type BindDcxType;
            public int TaeRootBindID;
            public TAEFormat Format;
            public bool IsOldDemonsSoulsFormat_0x10000;
            public bool IsOldDemonsSoulsFormat_0x1000A;
            public long AnimCount2Value;
            public bool BigEndian;
            public byte Flags1;
            public byte Flags2;
            public byte Flags3;
            public byte Flags4;
            public byte Flags5;
            public byte Flags6;
            public byte Flags7;
            public byte Flags8;
            public string SkeletonName;
            public string SibName;
            public int ActionSetVersion_ForSingleTaeOutput;
            public bool SaveWithActionTracksStripped;
            public bool SaveEachCategoryToSeparateTae;
            public int DefaultTaeProjectID;
            public string DefaultTaeShortName;

            public static TaeProperties Deserialize(BinaryReaderEx br)
            {
                var t = new TaeProperties();
                t.BindFlags = (Binder.FileFlags)br.ReadByte();
                t.BindDirectory = br.ReadNullPrefixUTF16();
                t.BindDcxType = (DCX.Type)br.ReadInt32();
                t.TaeRootBindID = br.ReadInt32();
                t.Format = (TAEFormat)br.ReadInt32();
                t.IsOldDemonsSoulsFormat_0x10000 = br.ReadBoolean();
                t.IsOldDemonsSoulsFormat_0x1000A = br.ReadBoolean();
                t.AnimCount2Value = br.ReadInt64();
                t.BigEndian = br.ReadBoolean();
                t.Flags1 = br.ReadByte();
                t.Flags2 = br.ReadByte();
                t.Flags3 = br.ReadByte();
                t.Flags4 = br.ReadByte();
                t.Flags5 = br.ReadByte();
                t.Flags6 = br.ReadByte();
                t.Flags7 = br.ReadByte();
                t.Flags8 = br.ReadByte();
                t.SkeletonName = br.ReadNullPrefixUTF16();
                t.SibName = br.ReadNullPrefixUTF16();
                t.ActionSetVersion_ForSingleTaeOutput = br.ReadInt32();
                br.AssertInt32(0);
                t.SaveWithActionTracksStripped = br.ReadBoolean();
                t.SaveEachCategoryToSeparateTae = br.ReadBoolean();
                t.DefaultTaeProjectID = br.ReadInt32();
                t.DefaultTaeShortName = br.ReadNullPrefixUTF16();
                return t;
            }

            public void Serialize(BinaryWriterEx bw)
            {
                bw.WriteByte((byte)BindFlags);
                bw.WriteNullPrefixUTF16(BindDirectory);
                bw.WriteInt32((int)BindDcxType);
                bw.WriteInt32(TaeRootBindID);
                bw.WriteInt32((int)Format);
                bw.WriteBoolean(IsOldDemonsSoulsFormat_0x10000);
                bw.WriteBoolean(IsOldDemonsSoulsFormat_0x1000A);
                bw.WriteInt64(AnimCount2Value);
                bw.WriteBoolean(BigEndian);
                bw.WriteByte(Flags1);
                bw.WriteByte(Flags2);
                bw.WriteByte(Flags3);
                bw.WriteByte(Flags4);
                bw.WriteByte(Flags5);
                bw.WriteByte(Flags6);
                bw.WriteByte(Flags7);
                bw.WriteByte(Flags8);
                bw.WriteNullPrefixUTF16(SkeletonName);
                bw.WriteNullPrefixUTF16(SibName);
                bw.WriteInt32(ActionSetVersion_ForSingleTaeOutput);
                bw.WriteInt32(0);
                bw.WriteBoolean(SaveWithActionTracksStripped);
                bw.WriteBoolean(SaveEachCategoryToSeparateTae);
                bw.WriteInt32(DefaultTaeProjectID);
                bw.WriteNullPrefixUTF16(DefaultTaeShortName);
            }

            public bool Equals(TaeProperties other)
            {
                return BindFlags == other.BindFlags
                    && BindDirectory == other.BindDirectory
                    && BindDcxType == other.BindDcxType
                    && TaeRootBindID == other.TaeRootBindID
                    && Format == other.Format
                    && IsOldDemonsSoulsFormat_0x10000 == other.IsOldDemonsSoulsFormat_0x10000
                    && IsOldDemonsSoulsFormat_0x1000A == other.IsOldDemonsSoulsFormat_0x1000A
                    && AnimCount2Value == other.AnimCount2Value
                    && BigEndian == other.BigEndian
                    && Flags1 == other.Flags1
                    && Flags2 == other.Flags2
                    && Flags3 == other.Flags3
                    && Flags4 == other.Flags4
                    && Flags5 == other.Flags5
                    && Flags6 == other.Flags6
                    && Flags7 == other.Flags7
                    && Flags8 == other.Flags8
                    && SkeletonName == other.SkeletonName
                    && SibName == other.SibName
                    && ActionSetVersion_ForSingleTaeOutput == other.ActionSetVersion_ForSingleTaeOutput
                    && SaveWithActionTracksStripped == other.SaveWithActionTracksStripped
                    && SaveEachCategoryToSeparateTae == other.SaveEachCategoryToSeparateTae
                    && DefaultTaeProjectID == other.DefaultTaeProjectID
                    && DefaultTaeShortName == other.DefaultTaeShortName;
            }

            public static bool operator ==(TaeProperties a, TaeProperties b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(TaeProperties a, TaeProperties b)
            {
                return !a.Equals(b);
            }
        }
    }
}
