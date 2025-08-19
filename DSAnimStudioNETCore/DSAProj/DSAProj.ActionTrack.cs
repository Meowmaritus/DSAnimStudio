using DSAnimStudio.TaeEditor;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsAssetPipeline.Animation.TAE.ActionTrack;

namespace DSAnimStudio
{
    public partial class DSAProj
    {

        public class ActionTrack
        {
            public readonly string GUID = Guid.NewGuid().ToString();
            
            public float EditorHighlightDelayTimer = -1;
            public float EditorHighlightTimer = -1;

            public EditorInfo Info = new EditorInfo();
            public int TrackType;
            //public int AutogenTrack_Type;
            //public int AutogenTrack_Subtype;

            public ActionTrackDataStruct TrackData;
            
            public int NewActionDefaultType = -1;
            public byte[] NewActionDefaultParameters = null;
            public float NewActionDefaultLength = -1;


            // Runtime stuff
            [JsonIgnore]
            public bool PlaybackHighlight = false;

            public long GetSortRefID(Animation anim, Animation.TrackSortTypes sortType)
            {
                var actions = GetActions(anim);
                if (actions.Count > 0)
                {
                    if (sortType == Animation.TrackSortTypes.FirstActionType)
                    {
                        return actions.OrderBy(x => x.StartTime).Select(x => x.GetTrackSortRefID()).First();
                    }
                    else if (sortType == Animation.TrackSortTypes.LowestActionType)
                    {
                        return actions.Select(x => x.GetTrackSortRefID()).OrderBy(x => x).First();
                    }
                }
                return -1;
            }

            public List<Action> GetActions(Animation anim)
            {
                int trackIndex = anim.SAFE_GetIndexOfActionTrack(this);
                if (trackIndex < 0)
                    return null;
                return anim.SAFE_GetActions().Where(a => a.TrackIndex == trackIndex).ToList();
            }

            public void Deserialize(BinaryReaderEx br, DSAProj proj)
            {
                if (proj.FILE_VERSION >= Versions.v20_00_00)
                {
                    Info = new EditorInfo();
                    Info.Deserialize(br, proj);
                }

                TrackData = new ActionTrackDataStruct();
                TrackType = br.ReadInt32();

                if (proj.FILE_VERSION < Versions.v20_00_00)
                {
                    int autogenTrackType = br.ReadInt32();
                }

                if (proj.FILE_VERSION < Versions.v20_00_00)
                {
                    Info = new EditorInfo();
                    Color? customColor = null;
                    if (proj.FILE_VERSION >= Versions.v11)
                        customColor = br.ReadNullPrefixColor();
                    var displayText = br.ReadNullPrefixUTF16();
                    Info.CustomColor = customColor;
                    Info.DisplayName = displayText;
                }

                
                TrackData.DataType = (ActionTrackDataType)br.ReadInt64();
                TrackData.CutsceneEntityType = (ActionTrackDataStruct.EntityTypes)br.ReadUInt16();
                TrackData.CutsceneEntityIDPart1 = br.ReadInt16();
                TrackData.CutsceneEntityIDPart2 = br.ReadInt16();
                TrackData.Area = br.ReadSByte();
                TrackData.Block = br.ReadSByte();

                NewActionDefaultType = br.ReadInt32();
                bool hasNewEventDefaultParameters = br.ReadBoolean();
                if (hasNewEventDefaultParameters)
                {
                    int byteCount = br.ReadInt32();
                    NewActionDefaultParameters = br.ReadBytes(byteCount);
                }
                NewActionDefaultLength = br.ReadSingle();
            }

            public void Serialize(BinaryWriterEx bw, DSAProj proj)
            {
                Info.Serialize(bw, proj);
                bw.WriteInt32(TrackType);
                bw.WriteInt64((long)TrackData.DataType);
                bw.WriteUInt16((ushort)TrackData.CutsceneEntityType);
                bw.WriteInt16(TrackData.CutsceneEntityIDPart1);
                bw.WriteInt16(TrackData.CutsceneEntityIDPart2);
                bw.WriteSByte(TrackData.Area);
                bw.WriteSByte(TrackData.Block);

                bw.WriteInt32(NewActionDefaultType);
                if (NewActionDefaultParameters == null)
                {
                    bw.WriteBoolean(false);
                }
                else
                {
                    bw.WriteBoolean(true);
                    bw.WriteInt32(NewActionDefaultParameters.Length);
                    bw.WriteBytes(NewActionDefaultParameters);
                }
                bw.WriteSingle(NewActionDefaultLength);
            }

            public ActionTrack GetClone()
            {
                return new ActionTrack()
                {
                    Info = Info.GetClone(),
                    TrackType = TrackType,
                    TrackData = TrackData,
                    NewActionDefaultType = NewActionDefaultType,
                    NewActionDefaultLength = NewActionDefaultLength,
                    NewActionDefaultParameters = NewActionDefaultParameters != null ? NewActionDefaultParameters.ToArray() : null,
                };
            }

            public static ActionTrack FromBinary(SoulsAssetPipeline.Animation.TAE.ActionTrack g)
            {
                var t = new ActionTrack();
                t.TrackType = g.TrackType;
                //t.AutogenTrack_Type = g.AutogenTrack_Type;
                //t.AutogenTrack_Subtype = g.AutogenTrack_Subtype;
                t.TrackData = g.TrackData;
                return t;
            }

            public SoulsAssetPipeline.Animation.TAE.ActionTrack ToBinary()
            {
                var t = new SoulsAssetPipeline.Animation.TAE.ActionTrack();
                t.TrackType = TrackType;

                //t.AutogenTrack_Type = AutogenTrack_Type;
                //t.AutogenTrack_Subtype = AutogenTrack_Subtype;
                t.TrackData = TrackData;
                return t;
            }
        }

    }
}
