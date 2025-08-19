using DSAnimStudio.TaeEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;
using SoulsAssetPipeline.Animation;
using static DSAnimStudio.ManagerAction;
using static SoulsAssetPipeline.Animation.TAE.Animation;
using static System.Resources.ResXFileRef;
using Microsoft.Xna.Framework;
using static DSAnimStudio.ImguiOSD.Window;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DSAnimStudio
{
    public partial class DSAProj
    {
        public class Animation
        {
            public DSAProj ParentProj;
            public AnimCategory ParentCategory;
            public Animation(DSAProj parentProj, AnimCategory parentCategory)
            {
                ParentProj = parentProj;
                ParentCategory = parentCategory;
            }

            public bool IS_DUMMY_ANIM = false;

            public void INNER_UnloadStringsToSaveMemory()
            {
                foreach (var a in Actions)
                    a.INNER_UnloadStringsToSaveMemory();
            }

            public void SAFE_UnloadStringsToSaveMemory()
                => SAFE(INNER_UnloadStringsToSaveMemory);



            //private object _lock = new();
            private void SAFE(System.Action act)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    act.Invoke();
                }
            }

            private T SAFE<T>(System.Func<T> func)
            {
                if (ParentProj != null)
                {
                    lock (ParentProj._lock_DSAProj)
                    {
                        return func.Invoke();
                    }
                }
                else
                {
                    return func.Invoke();
                }
            }

            public void SafeAccessActions(Action<List<Action>> doAction)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    doAction?.Invoke(Actions);
                }
            }
            public void UnSafeAccessActions(Action<List<Action>> doAction)
            {
                doAction?.Invoke(Actions);
            }




            public void SafeAccessActionTracks(Action<List<ActionTrack>> doAction)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    doAction?.Invoke(ActionTracks);
                }
            }
            public void UnSafeAccessActionTracks(Action<List<ActionTrack>> doAction)
            {
                doAction?.Invoke(ActionTracks);
            }


            public void SafeAccessHeader(Action<AnimFileHeader> doAction)
            {
                if (ParentProj != null)
                {
                    lock (ParentProj._lock_DSAProj)
                    {
                        doAction?.Invoke(_header);
                    }
                }
                else
                {
                    doAction?.Invoke(_header);
                }
            }
            public void UnSafeAccessHeader(Action<AnimFileHeader> doAction)
            {
                doAction?.Invoke(_header);
            }

            public TAE.Animation.AnimFileHeader INNER_GetHeaderClone()
            {
                TAE.Animation.AnimFileHeader headerClone = null;
                headerClone = _header.GetClone();
                return headerClone;
            }

            public TAE.Animation.AnimFileHeader SAFE_GetHeaderClone()
            {
                TAE.Animation.AnimFileHeader headerClone = null;
                SafeAccessHeader(header =>
                {
                    headerClone = header.GetClone();
                });
                return headerClone;
            }



            //public void SafeAccess(Action<Animation> doAction)
            //{
            //    lock (_lock)
            //    {
            //        doAction?.Invoke(this);
            //    }
            //}






            /// <summary>
            /// GUID used for ImGui tagging (to prevent control interference).
            /// </summary>
            public readonly string GUID = Guid.NewGuid().ToString();

            public EditorInfo Info = new EditorInfo();

            public float EditorHighlightDelayTimer = -1;
            public float EditorHighlightTimer = -1;
            
            public ErrorContainerClass ErrorContainer = new ErrorContainerClass();

            public SplitAnimID SplitID;

            public void INNER_SetHeader(AnimFileHeader newVal)
            {
                _header = newVal;
            }

            public void SAFE_SetHeader(AnimFileHeader newVal)
            {
                SAFE(() =>
                {
                    INNER_SetHeader(newVal);
                });
            }

            //public AnimFileHeader GetHeader() => SAFE(() => _header);

            private SoulsAssetPipeline.Animation.TAE.Animation.AnimFileHeader _header;

            public void INNER_SetActionTracks(List<ActionTrack> newVal)
            {
                ActionTracks = newVal;
            }

            public void SAFE_SetActionTracks(List<ActionTrack> newVal)
            {
                SAFE(() =>
                {
                    INNER_SetActionTracks(newVal);
                });
            }

            public void INNER_SetActions(List<Action> newVal)
            {
                Actions = newVal;
            }
            public void SAFE_SetActions(List<Action> newVal)
            {
                SAFE(() =>
                {
                    INNER_SetActions(newVal);
                });
            }



            public void INNER_AddAction(Action act)
            {
                if (!Actions.Contains(act))
                    Actions.Add(act);
            }
            public void SAFE_AddAction(Action act) => SAFE(() => INNER_AddAction(act));


            private void INNER_RemoveAction(Action act)
            {
                if (Actions.Contains(act))
                    Actions.Remove(act);
            }
            public void SAFE_RemoveAction(Action act) => SAFE(() => INNER_RemoveAction(act));

            private int INNER_GetActionCount() => Actions.Count;
            public int SAFE_GetActionCount() => SAFE(INNER_GetActionCount);

            private Action INNER_GetActionByIndex(int index) =>
                index >= 0 && index < Actions.Count ? Actions[index] : null;
            public Action SAFE_GetActionByIndex(int index) => SAFE(() => INNER_GetActionByIndex(index));

            public void INNER_ClearActions() => Actions.Clear();
            public void SAFE_ClearActions() => SAFE(INNER_ClearActions);




            public void INNER_AddActionTrack(ActionTrack track)
            {
                if (!ActionTracks.Contains(track))
                    ActionTracks.Add(track);
            }
            public void SAFE_AddActionTrack(ActionTrack track) => SAFE(() => INNER_AddActionTrack(track));

            private void INNER_RemoveActionTrack(ActionTrack track)
            {
                if (ActionTracks.Contains(track))
                    ActionTracks.Remove(track);
            }
            public void SAFE_RemoveActionTrack(ActionTrack track) => SAFE(() => INNER_RemoveActionTrack(track));

            private int INNER_GetActionTrackCount() => ActionTracks.Count;
            public int SAFE_GetActionTrackCount() => SAFE(INNER_GetActionTrackCount);

            private ActionTrack INNER_GetActionTrackByIndex(int index) =>
                index >= 0 && index < ActionTracks.Count ? ActionTracks[index] : null;
            public ActionTrack SAFE_GetActionTrackByIndex(int index) => SAFE(() => INNER_GetActionTrackByIndex(index));


            private int INNER_GetIndexOfActionTrack(ActionTrack track) => ActionTracks.IndexOf(track);
            public int SAFE_GetIndexOfActionTrack(ActionTrack track) => SAFE(() => INNER_GetIndexOfActionTrack(track));


            private List<ActionTrack> ActionTracks = new List<ActionTrack>();
            private List<Action> Actions = new List<Action>();

            public bool ActionsNeedInitialize = true;

            private void INNER_SetActionAtIndex(int index, Action act)
            {
                if (index >= 0 && index < Actions.Count)
                    Actions[index] = act;
            }
            public void SAFE_SetActionAtIndex(int index, Action act) => SAFE(() => INNER_SetActionAtIndex(index, act));

            private void INNER_SetActionTrackAtIndex(int index, ActionTrack track)
            {
                if (index >= 0 && index < ActionTracks.Count)
                    ActionTracks[index] = track;
            }
            public void SAFE_SetActionTrackAtIndex(int index, ActionTrack track) => SAFE(() => INNER_SetActionTrackAtIndex(index, track));


            public List<Action> INNER_GetActions() => Actions.ToList();
            public List<Action> SAFE_GetActions() => SAFE(INNER_GetActions);


            private List<ActionTrack> INNER_GetActionTracks() => ActionTracks.ToList();
            public List<ActionTrack> SAFE_GetActionTracks() => SAFE(INNER_GetActionTracks);

            public void INNER_ClearActionTracks() => ActionTracks.Clear();
            public void SAFE_ClearActionTracks() => SAFE(INNER_ClearActionTracks);




            public float INNER_GetBlendDuration()
            {
                //if (_header is AnimFileHeader.ImportOtherAnim asImportOther)
                //{
                //    if (asImportOther.ImportFromAnimID >= 0)
                //    {
                //        var other = ParentProj.INNER_GetFirstAnimationFromFullID(SplitAnimID.FromFullID(ParentProj, asImportOther.ImportFromAnimID));
                //        if (other != null)
                //            return other.INNER_GetBlendDuration();
                //    }
                //}

                // Performance meme
                if (Actions.Count > 0 && Actions[0].Type == 16)
                {
                    var blendAction = Actions[0];
                    if (blendAction == null)
                        return 0;

                    return blendAction.EndTime - blendAction.StartTime;
                }
                else
                {
                    var blendAction = Actions.FirstOrDefault(a => a.Type == 16);
                    if (blendAction == null)
                        return 0;

                    Actions.Remove(blendAction);
                    Actions.Insert(0, blendAction);

                    return blendAction.EndTime - blendAction.StartTime;
                }


            }
            public float SAFE_GetBlendDuration()
            {
                if (ParentProj != null)
                    return SAFE(INNER_GetBlendDuration);
                else
                    return INNER_GetBlendDuration();
            }

            public SplitAnimID INNER_GetHkxID(DSAProj proj)
            {
                if (_header is TAE.Animation.AnimFileHeader.ImportOtherAnim asImportOtherAnim)
                {
                    var referenced = proj.SAFE_SolveAnimRefChain(SplitID);

                    if (referenced == this)
                    {
                        return SplitID;
                    }

                    return referenced?.GetHkxID(proj) ?? SplitAnimID.Invalid;
                }
                else if (_header is TAE.Animation.AnimFileHeader.Standard asStandard)
                {
                    if (asStandard.ImportsHKX && asStandard.ImportHKXSourceAnimID >= 0)
                        return SplitAnimID.FromFullID(proj, asStandard.ImportHKXSourceAnimID);
                    else
                        return SplitID;
                }

                return SplitAnimID.Invalid;
            }
            public SplitAnimID GetHkxID(DSAProj proj) => SAFE(() => INNER_GetHkxID(proj));

            public Animation INNER_GetClone()
            {
                var clone = new Animation(ParentProj, ParentCategory);
                clone.SplitID = SplitID;
                clone.Info = Info.GetClone();
                clone.IS_DUMMY_ANIM = IS_DUMMY_ANIM;
                if (_header is AnimFileHeader.Standard asStandard)
                {
                    clone._header = new AnimFileHeader.Standard()
                    {
                        IsNullHeader = asStandard.IsNullHeader,
                        AllowDelayLoad = asStandard.AllowDelayLoad,
                        AnimFileName = asStandard.AnimFileName,
                        ImportHKXSourceAnimID = asStandard.ImportHKXSourceAnimID,
                        ImportsHKX = asStandard.ImportsHKX,
                        IsLoopByDefault = asStandard.IsLoopByDefault,
                        Type = asStandard.Type,
                    };
                }
                else if (_header is AnimFileHeader.ImportOtherAnim asImportOtherAnim)
                {
                    clone._header = new AnimFileHeader.ImportOtherAnim()
                    {
                        IsNullHeader = asImportOtherAnim.IsNullHeader,
                        AnimFileName = asImportOtherAnim.AnimFileName,
                        Type = asImportOtherAnim.Type,
                        ImportFromAnimID = asImportOtherAnim.ImportFromAnimID,
                        Unknown = asImportOtherAnim.Unknown,
                    };
                }
                foreach (var act in Actions)
                {
                    clone.Actions.Add(act.GetClone());
                }
                foreach (var track in ActionTracks)
                {
                    clone.ActionTracks.Add(track.GetClone());
                }
                return clone;
            }

            public Animation SAFE_GetClone() => SAFE(INNER_GetClone);

            public static Animation NewDummyAnim(DSAProj parentProj, AnimCategory parentCategory)
            {
                var anim = new Animation(parentProj, parentCategory);
                anim.SET_AS_DUMMY_ANIM(parentCategory.CategoryID);
                return anim;
            }

            public void SET_AS_DUMMY_ANIM(int categoryID)
            {
                IS_DUMMY_ANIM = true;
                SplitID = new SplitAnimID() { CategoryID = categoryID, SubID = -1 };
                Info = new EditorInfo();
                _header = new AnimFileHeader.Standard();
                _header.AnimFileName = "DummyAnim";
                ActionTracks = new List<ActionTrack>();
                Actions = new List<Action>();
            }

            public void INNER_Deserialize(BinaryReaderEx br, SoulsAssetPipeline.Animation.TAE.Template template, DSAProj proj, DSAProj.AnimCategory category)
            {
                IS_DUMMY_ANIM = false;

                if (proj.FILE_VERSION >= Versions.v21_01_00)
                {
                    IS_DUMMY_ANIM = br.ReadBoolean();
                }

                if (IS_DUMMY_ANIM)
                {
                    SET_AS_DUMMY_ANIM(category.CategoryID);
                    return;
                }

                if (proj.FILE_VERSION >= Versions.v20_00_00)
                {
                    Info = new EditorInfo();
                    Info.Deserialize(br, proj);
                }
                SplitID = SplitAnimID.FromFullID(proj, br.ReadInt64());
                if (proj.FILE_VERSION < Versions.v20_00_00)
                {
                    Info = new EditorInfo();
                    Color? customColor = null;
                    if (proj.FILE_VERSION >= Versions.v11)
                        customColor = br.ReadNullPrefixColor();
                    Info.CustomColor = customColor;
                }
                var animFileHeaderType = br.ReadEnum32<AnimFileHeaderType>();
                if (animFileHeaderType == AnimFileHeaderType.Standard)
                {
                    var m = new SoulsAssetPipeline.Animation.TAE.Animation.AnimFileHeader.Standard();
                    m.AnimFileName = br.ReadNullPrefixUTF16();
                    m.IsLoopByDefault = br.ReadBoolean();
                    m.ImportsHKX = br.ReadBoolean();
                    m.AllowDelayLoad = br.ReadBoolean();
                    m.ImportHKXSourceAnimID = br.ReadInt32();
                    _header = m;
                }
                else if (animFileHeaderType == AnimFileHeaderType.ImportOtherAnim)
                {
                    var m = new AnimFileHeader.ImportOtherAnim();
                    m.AnimFileName = br.ReadNullPrefixUTF16();
                    m.ImportFromAnimID = br.ReadInt32();
                    m.Unknown = br.ReadInt32();
                    _header = m;
                }

                int actionTrackCount = br.ReadInt32();
                ActionTracks.Clear();
                for (int i = 0; i < actionTrackCount; i++)
                {
                    var track = new ActionTrack();
                    track.Deserialize(br, proj);
                    ActionTracks.Add(track);
                }
                int actionCount = br.ReadInt32();
                Actions.Clear();
                for (int i = 0; i < actionCount; i++)
                {
                    var act = new Action();
                    act.Deserialize(br, proj);
                    Actions.Add(act);
                }

                ActionsNeedInitialize = true;

                

                
            }

            public void INNER_CheckActionInitialize(SoulsAssetPipeline.Animation.TAE.Template template)
            {
                if (ActionsNeedInitialize)
                {
                    if (template != null)
                    {
                        foreach (var act in Actions)
                        {
                            if (template.ContainsKey(act.Type))
                            {
                                act.ApplyTemplate(template[act.Type]);
                            }
                        }
                    }

                    INNER_GenerateTrackNames(template, overwriteExisting: true);

                    ActionsNeedInitialize = false;
                }
            }
            public void SAFE_CheckActionInitialize(SoulsAssetPipeline.Animation.TAE.Template template)
                => SAFE(() => INNER_CheckActionInitialize(template));

            public void SAFE_Deserialize(BinaryReaderEx br, SoulsAssetPipeline.Animation.TAE.Template template, DSAProj proj, DSAProj.AnimCategory category)
                => SAFE(() => INNER_Deserialize(br, template, proj, category));

            public void INNER_Serialize(BinaryWriterEx binaryWriter, DSAProj proj)
            {
                var bw = new BinaryWriterEx(false);
                {
                    bw.WriteBoolean(IS_DUMMY_ANIM);
                    if (!IS_DUMMY_ANIM)
                    {
                        Info.Serialize(bw, proj);
                        bw.WriteInt64(SplitID.GetFullID(proj));
                        bw.WriteInt32((int)_header.Type);
                        bw.WriteNullPrefixUTF16(_header.AnimFileName);
                        if (_header is AnimFileHeader.Standard asStandard)
                        {
                            bw.WriteBoolean(asStandard.IsLoopByDefault);
                            bw.WriteBoolean(asStandard.ImportsHKX);
                            bw.WriteBoolean(asStandard.AllowDelayLoad);
                            bw.WriteInt32(asStandard.ImportHKXSourceAnimID);
                        }
                        else if (_header is AnimFileHeader.ImportOtherAnim asImportOtherAnim)
                        {
                            bw.WriteInt32(asImportOtherAnim.ImportFromAnimID);
                            bw.WriteInt32(asImportOtherAnim.Unknown);
                        }
                        bw.WriteInt32(ActionTracks.Count);
                        foreach (var track in ActionTracks)
                            track.Serialize(bw, proj);
                        bw.WriteInt32(Actions.Count);
                        foreach (var act in Actions)
                            act.Serialize(bw, proj);
                    }
                }

                SerializedCache = bw.FinishBytes();
                binaryWriter.WriteBytes(SerializedCache);
            }

            public void SAFE_Serialize(BinaryWriterEx bw, DSAProj proj) => SAFE(() => INNER_Serialize(bw, proj));

            //public Animation()
            //{

            //}

            public Animation(DSAProj parentProj, AnimCategory parentAnimCategory, SplitAnimID id, SoulsAssetPipeline.Animation.TAE.Animation.AnimFileHeader animMiniHeader)
            {
                ParentProj = parentProj;
                ParentCategory = parentAnimCategory;
                SplitID = id;
                _header = animMiniHeader;
            }

            internal bool _isModified = false;
            //public bool IsModified
            //{
            //    get
            //    {
            //        return _isModified;
            //    }
            //    set
            //    {
            //        _isModified = value;
            //        if (ParentCategory != null)
            //        {
            //            ParentCategory.SAFE_RegistAnimModifiedFlag(this, value);


            //            lock (ParentProj._lock_DSAProj)
            //            {
            //                if (value)
            //                {
            //                    // If just marked as modified then this is not good data to save[?]
            //                    SerializedCache = null;
            //                }
            //                //else
            //                //{
            //                //    // If just marked as not modified then this is good data to save[?]
            //                //    SerializedCache = SerializeToBytes(ParentProj);
            //                //}
            //            }


            //        }
            //    }
            //}

            private byte[] SerializedCache = null;

            public byte[] INNER_GetSerializedCache()
            {
                byte[] result = null;
                if (SerializedCache != null)
                    result = SerializedCache.ToArray();
                return result;
            }
            public byte[] SAFE_GetSerializedCache() => SAFE(INNER_GetSerializedCache);

            public List<DSAProj.Action> SelectedActions = new();

            public EditorFlags RuntimeFlags;

            public void INNER_SetIsModified(bool value)
            {
                _isModified = value;
                if (ParentCategory != null)
                {
                    ParentCategory.INNER_RegistAnimModifiedFlag(this, value);


                    if (value)
                    {
                        // If just marked as modified then this is not good data to save[?]
                        SerializedCache = null;
                    }
                    //else
                    //{
                    //    // If just marked as not modified then this is good data to save[?]
                    //    SerializedCache = SerializeToBytes(ParentProj);
                    //}


                }
            }
            public void SAFE_SetIsModified(bool value)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_SetIsModified(value);
                }
            }
            public bool INNER_GetIsModified()
            {
                return _isModified;
            }
            public bool SAFE_GetIsModified()
            {
                bool val = true;
                lock (ParentProj._lock_DSAProj)
                {
                    val = _isModified;
                }
                return _isModified;
            }

            public void INNER_EnsureMinimumActionTrackCount(int count)
            {
                while (ActionTracks.Count < count)
                {
                    ActionTracks.Add(new ActionTrack()
                    {
                        Info = new EditorInfo(),
                        TrackData = new SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataStruct(),
                        TrackType = 0,

                    });
                }
            }
            public void SAFE_EnsureMinimumActionTrackCount(int count)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_EnsureMinimumActionTrackCount(count);
                }
            }
            
            public SoulsAssetPipeline.Animation.TAE.Animation LegacyToBinary(DSAProj proj)
            {
                var anim = new SoulsAssetPipeline.Animation.TAE.Animation(SplitID.GetFullID(proj), _header.GetClone());
                anim.ActionTracks = !proj.ParentDocument.GameRoot.GameTypeUsesLegacyEmptyEventGroups ? ActionTracks.Select(t => t.ToBinary()).ToList() : new List<SoulsAssetPipeline.Animation.TAE.ActionTrack>();

                anim.Actions = new();

                var anySolo = Actions.Any(act => act.Solo);
                foreach (var act in Actions)
                {
                    bool writeThisAction = true;
                    if (anySolo)
                        writeThisAction = act.Solo;
                    else
                        writeThisAction = !act.Mute;
                    
                    if (writeThisAction)
                        anim.Actions.Add(act.ToBinary());
                }
                
                return anim;
            }
            

            //public static Animation LegacyFromBinary(DSAProj proj, SoulsAssetPipeline.Animation.TAE.Animation a, AnimCategory parent, SoulsAssetPipeline.Animation.TAE.Template template)
            //{
            //    var x = LegacyFromBinary(proj, a);
            //    return x;
            //}

            public static Animation LegacyFromBinary(DSAProj proj, SoulsAssetPipeline.Animation.TAE.Animation a, AnimCategory parentCategory)
            {

                var entry = new Animation(proj, parentCategory);
                entry.SplitID = SplitAnimID.FromFullID(proj, a.ID);
                entry._header = a.Header.GetClone();
                entry.ActionTracks = a.ActionTracks.Select(g => ActionTrack.FromBinary(g)).ToList();
                entry.Actions = a.Actions.Select(e => Action.FromBinary(e)).ToList();
                
                return entry;
            }

            public enum TrackSortTypes
            {
                FirstActionType,
                LowestActionType,
                RowName,
            }

            public void INNER_ResortTracks(TrackSortTypes sortType)
            {
                var prevTracks = ActionTracks.ToList();
                List<DSAProj.ActionTrack> newTracks = null;
                if (sortType is TrackSortTypes.LowestActionType or TrackSortTypes.FirstActionType)
                {
                    newTracks = ActionTracks.OrderBy(x => x.GetSortRefID(this, sortType)).ToList();
                }
                else if (sortType is TrackSortTypes.RowName)
                {
                    newTracks = ActionTracks.OrderBy(x => x.Info.DisplayName).ToList();
                }
                else
                {
                    throw new NotImplementedException();
                }

                foreach (var act in Actions)
                {
                    // If track index was outside this range then resort should just leave it.
                    // Since no tracks are being added or removed, out of range indices don't need
                    // to be shifted or anything.
                    if (act.TrackIndex >= 0 && act.TrackIndex < prevTracks.Count)
                    {
                        var matchingTrack = prevTracks[act.TrackIndex];
                        act.TrackIndex = newTracks.IndexOf(matchingTrack);
                    }

                }
                ActionTracks = newTracks;
            }

            public void SAFE_ResortTracks(TrackSortTypes sortType) => SAFE(() => INNER_ResortTracks(sortType));

            public void INNER_GenerateTrackNames(SoulsAssetPipeline.Animation.TAE.Template template, bool overwriteExisting)
            {
                if (template == null)
                    return;

                for (int i = 0; i < ActionTracks.Count; i++)
                {
                    // Skip if value already exists, unless overwriteExisting is true.
                    if (!(overwriteExisting || string.IsNullOrWhiteSpace(ActionTracks[i].Info.DisplayName)))
                        continue;

                    var track = ActionTracks[i];

                    string rowName = $"Track {(i + 1)}";


                    var actionsInTrack = track.GetActions(this);
                    if (actionsInTrack.Count > 0)
                    {
                        var firstActionInTrack = actionsInTrack.First();

                        int actType = firstActionInTrack.Type;
                        int chrActionFlagType = 0;
                        if (firstActionInTrack.Type == 0 || firstActionInTrack.Type == 300)
                        {
                            chrActionFlagType = Convert.ToInt32(firstActionInTrack.ReadInternalSimField("JumpTableID"));
                        }

                        if (template != null)
                        {
                            if (actType == 0 || (actType == 300))
                            {
                                var actionFlagEnumEntries = template[0][0].EnumEntries;
                                bool foundFlagName = false;
                                foreach (var afEntry in actionFlagEnumEntries)
                                {
                                    if (chrActionFlagType == Convert.ToInt32(afEntry.Value))
                                    {
                                        rowName = $"Flag {afEntry.Key}";
                                        foundFlagName = true;
                                        break;
                                    }
                                }
                                if (!foundFlagName)
                                    rowName = $"Flag {chrActionFlagType}";
                            }
                            else if (template.ContainsKey(actType))
                            {
                                rowName = template[actType].Name;

                                if (Main.Debug.GenTrackNamesIncludeExtraInfo)
                                {
                                    rowName = $"{rowName}[{actType}]";
                                }
                            }
                        }
                    }



                    //rowName = null;//$"Row {(i + 1)} [" + rowName + "]";

                    if (Main.Debug.GenTrackNamesIncludeExtraInfo)
                    {
                        rowName = $"{rowName} [Track Type: {track.TrackType}]";
                    }

                    track.Info.DisplayName = rowName;
                }

            }

            public void SAFE_GenerateTrackNames(SoulsAssetPipeline.Animation.TAE.Template template, bool overwriteExisting)
                => SAFE(() => INNER_GenerateTrackNames(template, overwriteExisting));

            public byte[] SerializeToBytes(DSAProj proj)
            {
                var bw = new BinaryWriterEx(false);
                SAFE_Serialize(bw, proj);
                var result =  bw.FinishBytes();
                return result;
            }

            public void DeserializeFromBytes(byte[] data, SoulsAssetPipeline.Animation.TAE.Template template, DSAProj proj, DSAProj.AnimCategory category)
            {
                var br = new BinaryReaderEx(false, data);
                SAFE_Deserialize(br, template, proj, category);
                
            }

        }


    }
}
