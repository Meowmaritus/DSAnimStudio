using DSAnimStudio.TaeEditor;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static SoulsAssetPipeline.Animation.TAE;

namespace DSAnimStudio
{
    public partial class DSAProj
    {
        public class BinderAttributes
        {
            public Binder.FileFlags Flags;
            public int ID;
            public string Name;
            public DCX.Type CompressionType;
            public BinderAttributes GetClone()
            {
                var clone = new BinderAttributes();
                clone.Flags = Flags;
                clone.ID = ID;
                clone.Name = Name;
                clone.CompressionType = CompressionType;
                return clone;
            }


            public static BinderAttributes Deserialize(BinaryReaderEx br)
            {
                var attr = new BinderAttributes();
                attr.Flags = (Binder.FileFlags)br.ReadByte();
                attr.ID = br.ReadInt32();
                attr.Name = br.ReadNullPrefixUTF16();
                attr.CompressionType = (DCX.Type)br.ReadInt32();
                return attr;
            }

            public void Serialize(BinaryWriterEx bw)
            {
                bw.WriteByte((byte)Flags);
                bw.WriteInt32(ID);
                bw.WriteNullPrefixUTF16(Name);
                bw.WriteInt32((int)CompressionType);
            }

            public static BinderAttributes FromBinderFile(BinderFile bf)
            {
                return new BinderAttributes()
                {
                    ID = bf.ID,
                    Name = bf.Name,
                    CompressionType = bf.CompressionType,
                    Flags = bf.Flags,
                };
            }

            public BinderFile ToBinderFile(byte[] data)
            {
                return new BinderFile()
                {
                    ID = ID,
                    Name = Name,
                    CompressionType = CompressionType,
                    Flags = Flags,
                    Bytes = data,
                };
            }
        }


        public class AnimCategory
        {
            public DSAProj ParentProj;
            public AnimCategory(DSAProj parentProj)
            {
                ParentProj = parentProj;
            }

            public void INNER_UnloadStringsToSaveMemory()
            {
                foreach (var a in Animations)
                    a.INNER_UnloadStringsToSaveMemory();
            }

            /// <summary>
            /// GUID used for ImGui tagging (to prevent control interference).
            /// </summary>
            [JsonIgnore]
            public readonly string GUID = Guid.NewGuid().ToString();

            public float EditorHighlightDelayTimer = -1;
            public float EditorHighlightTimer = -1;
            
            [JsonIgnore]
            public EditorFlags RuntimeFlags;
            [JsonIgnore]
            public ErrorContainerClass ErrorContainer = new ErrorContainerClass();

            public EditorInfo Info = new EditorInfo();

            public int CategoryID;

            public int ActionSetVersion_ForMultiTaeOutput = -1;

            //public void DirectSetCategoryID(int categoryID)
            //{
            //    _categoryID = categoryID;
            //}

            // public byte[] MD5Hash = null;
            // public BinderAttributes BinderInfo = new BinderAttributes();
            // public TaeProperties Properties;

            private void SAFE(System.Action act)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    act.Invoke();
                }
            }

            private T SAFE<T>(System.Func<T> func)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    return func.Invoke();
                }
            }






            public void SafeAccessAnimations(Action<List<Animation>> doAction)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    doAction?.Invoke(Animations);
                }
            }
            public void UnSafeAccessAnimations(Action<List<Animation>> doAction)
            {
                doAction?.Invoke(Animations);
            }







            public void SafeAccessModifiedAnimations(Action<List<Animation>> doAction)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    doAction?.Invoke(ModifiedAnimations);
                }
            }
            public void UnSafeAccessModifiedAnimations(Action<List<Animation>> doAction)
            {
                doAction?.Invoke(ModifiedAnimations);
            }






            public void SafeAccessErroredAnimations(Action<List<Animation>> doAction)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    doAction?.Invoke(ErroredAnimations);
                }
            }
            public void UnSafeAccessErroredAnimations(Action<List<Animation>> doAction)
            {
                doAction?.Invoke(ErroredAnimations);
            }


            //public void SafeAccess(Action<AnimCategory> doAction)
            //{
            //    lock (_lock)
            //    {
            //        doAction?.Invoke(this);
            //    }
            //}



            private int INNER_GetAnimIndexInList(Animation anim)
            {
                return Animations.IndexOf(anim);
            }

            public int SAFE_GetAnimIndexInList(Animation anim)
            {
                int result = -1;
                lock (ParentProj._lock_DSAProj)
                {
                    result = INNER_GetAnimIndexInList(anim);
                }
                return result;
            }




            private Animation INNER_GetAnimByIndex(int index)
            {
                if (index >= 0 && index < Animations.Count)
                    return Animations[index];
                else
                    return null;
            }

            public Animation SAFE_GetAnimByIndex(int index) => SAFE(() => INNER_GetAnimByIndex(index));






            private Animation INNER_GetFirstAnimInList(bool throwIfEmpty = false)
            {
                if (Animations.Count == 0 && throwIfEmpty)
                    throw new InvalidOperationException("Tried to get first animation in an empty category");
                return Animations.FirstOrDefault();
            }
            public Animation SAFE_GetFirstAnimInList(bool throwIfEmpty = false) => SAFE(() => INNER_GetFirstAnimInList(throwIfEmpty));




            private Animation INNER_GetLastAnimInList(bool throwIfEmpty = false)
            {
                if (Animations.Count == 0 && throwIfEmpty)
                    throw new InvalidOperationException("Tried to get last animation in an empty category");
                return Animations.LastOrDefault();
            }
            public Animation SAFE_GetLastAnimInList(bool throwIfEmpty = false) => SAFE(() => INNER_GetLastAnimInList(throwIfEmpty));



            public int INNER_GetAnimCount()
            {
                return Animations.Count;
            }

            public int SAFE_GetAnimCount()
            {
                int result = 0;
                lock (ParentProj._lock_DSAProj)
                {
                    result = INNER_GetAnimCount();
                }
                return result;
            }




            private void INNER_CreateDummyAnimation(DSAProj proj, int id)
            {
                var newAnim = new Animation(proj, this, SplitAnimID.FromFullID(proj, id), new SoulsAssetPipeline.Animation.TAE.Animation.AnimFileHeader.Standard()
                {
                    IsNullHeader = true,
                });
                INNER_AddAnimation(newAnim);
            }

            public void SAFE_CreateDummyAnimation(DSAProj proj, int id)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_CreateDummyAnimation(proj, id);
                }
            }



            
            private AnimCategory INNER_GetClone()
            {
                var clone = new AnimCategory(ParentProj);
                // clone.MD5Hash = null;
                // clone.BinderInfo = BinderInfo.GetClone();
                // clone.Properties = Properties;
                var anims = Animations.ToList();
                foreach (var anim in anims)
                {
                    var animClone = anim.INNER_GetClone();
                    animClone.ParentCategory = clone;
                    animClone.INNER_SetIsModified(true);
                    clone.INNER_AddAnimation(animClone);
                }

                clone.Info = Info.GetClone();

                INNER_SetIsModified(true);

                return clone;
            }

            public AnimCategory SAFE_GetClone()
            {
                AnimCategory result = null;
                lock (ParentProj._lock_DSAProj)
                {
                    result = INNER_GetClone();
                }
                return result;
            }




            // public int GetFullAnimationID(Animation anim)
            // {
            //     var animID_Lower = GameRoot.GameTypeHasLongAnimIDs
            //                             ? ((int)anim.ID % 1_000000) : ((int)anim.ID % 1_0000);
            //     var upperAnimIdFromEntryID = (int)anim.ID / GameRoot.GameTypeUpperAnimIDModBy;
            //     var upperAnimIDFromTaeID = Properties.TaeBindIndex;
            //     var animID_Upper = (Properties.TaeBindIndex == 0 ? upperAnimIdFromEntryID : upperAnimIDFromTaeID);
            //
            //     return (animID_Upper * GameRoot.GameTypeUpperAnimIDModBy) + animID_Lower;
            // }
            //
            // public string GetFullAnimationIDString(Animation anim)
            // {
            //     var animID_Lower = GameRoot.GameTypeHasLongAnimIDs
            //                             ? (anim.ID % 1_000000) : (anim.ID % 1_0000);
            //
            //     var upperAnimIdFromEntryID = anim.ID / GameRoot.GameTypeUpperAnimIDModBy;
            //     //var upperAnimIDFromTaeID = CategoryID;
            //     var animID_Upper = upperAnimIdFromEntryID;
            //     return (GameRoot.GameTypeHasLongAnimIDs ? $"a{(animID_Upper):D3}_{animID_Lower:D6}" : $"a{(animID_Upper):D2}_{animID_Lower:D4}");
            // }




            private void INNER_ClearAnimations()
            {
                Animations.Clear();
                NEW_AnimationLookupDict.Clear();
                AllAnimSubIDs.Clear();
            }

            public void SAFE_ClearAnimations()
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_ClearAnimations();
                }
            }




            private List<Animation> Animations = new List<Animation>();


            


            //private object _lock = new();


            private Dictionary<int, List<Animation>> NEW_AnimationLookupDict = new();
            private List<int> AllAnimSubIDs = new List<int>();




            

            private void INNER_RemoveAllAnimationsBySubID(int animSubID)
            {
                var anims = INNER_GetAnimationsBySubID(animSubID);
                foreach (var anim in anims)
                {
                    INNER_RemoveAnimation(anim);
                }
            }

            public void SAFE_RemoveAllAnimationsBySubID(int animSubID)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_RemoveAllAnimationsBySubID(animSubID);
                }
            }





            public List<Animation> INNER_GetAnimationsBySubID(int animSubID)
            {
                if (!NEW_AnimationLookupDict.ContainsKey(animSubID))
                    return new List<Animation>();
                else
                    return NEW_AnimationLookupDict[animSubID];
            }

            public List<Animation> SAFE_GetAnimationsBySubID(int animSubID)
            {
                List<Animation> result = new();
                lock (ParentProj._lock_DSAProj)
                {
                    result = INNER_GetAnimationsBySubID(animSubID);
                }
                return result;
            }




            public List<Animation> INNER_GetAnimations() => Animations.ToList();

            public List<Animation> SAFE_GetAnimations() => SAFE(INNER_GetAnimations);




            private void INNER_ResortAnimIDs()
            {
                AllAnimSubIDs = AllAnimSubIDs.OrderBy(x => x).ToList();
                Animations = Animations.OrderBy(x => x.SplitID.SubID).ToList();
            }

            public void SAFE_ResortAnimIDs()
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_ResortAnimIDs();
                }
            }




            public void INNER_AddAnimation(Animation anim, int index = -1, bool ignoreModified = false)
            {
                if (!NEW_AnimationLookupDict.ContainsKey(anim.SplitID.SubID))
                    NEW_AnimationLookupDict.Add(anim.SplitID.SubID, new List<Animation>());

                var list = NEW_AnimationLookupDict[anim.SplitID.SubID];
                if (!list.Contains(anim))
                    list.Add(anim);

                if (!Animations.Contains(anim))
                {
                    if (index >= 0)
                        Animations.Insert(index, anim);
                    else
                        Animations.Add(anim);
                }

                anim.ParentProj = ParentProj;
                anim.ParentCategory = this;

                if (!AllAnimSubIDs.Contains(anim.SplitID.SubID))
                {
                    AllAnimSubIDs.Add(anim.SplitID.SubID);
                    //AllAnimSubIDs = AllAnimSubIDs.OrderBy(x => x).ToList();
                }

                if (!anim.IS_DUMMY_ANIM)
                {
                    INNER_RemoveDummyAnimations();
                }

                if (!ignoreModified)
                    INNER_SetIsModified(true);
            }

            public void INNER_RemoveDummyAnimations()
            {
                var dummyAnims = Animations.Where(x => x.IS_DUMMY_ANIM).ToList();
                foreach (var d in dummyAnims)
                {
                    INNER_RemoveAnimation(d);
                }
            }

            public void SAFE_AddAnimation(Animation anim, int index = -1)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_AddAnimation(anim, index);
                }
            }




            public void INNER_RemoveAnimation(Animation anim)
            {
                if (NEW_AnimationLookupDict.ContainsKey(anim.SplitID.SubID))
                {
                    var list = NEW_AnimationLookupDict[anim.SplitID.SubID];
                    if (list.Contains(anim))
                        list.Remove(anim);

                    if (list.Count == 0)
                    {
                        NEW_AnimationLookupDict.Remove(anim.SplitID.SubID);
                        if (AllAnimSubIDs.Contains(anim.SplitID.SubID))
                            AllAnimSubIDs.Remove(anim.SplitID.SubID);
                    }
                }

                if (Animations.Contains(anim))
                    Animations.Remove(anim);

                if (Animations.Count == 0)
                {
                    INNER_AddAnimation(Animation.NewDummyAnim(ParentProj, this));
                }
            }

            public void SAFE_RemoveAnimation(Animation anim)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_RemoveAnimation(anim);
                }
            }









            //public void SortAnimations()
            //{
            //    Animations = Animations.OrderBy(x => x.NewID).ToList();
            //}



            public Animation INNER_FindFirstAnimByFullID(SplitAnimID id)
            {
                if (NEW_AnimationLookupDict.ContainsKey(id.SubID))
                {
                    var anims = NEW_AnimationLookupDict[id.SubID];
                    if (anims.Count > 0)
                        return anims.First();
                }
                return null;
            }

            public Animation SAFE_FindFirstAnimByFullID(SplitAnimID id) => SAFE(() => INNER_FindFirstAnimByFullID(id));



            private Animation INNER_FindFirstAnimBySubID(int subID)
            {
                if (NEW_AnimationLookupDict.ContainsKey(subID))
                {
                    var anims = NEW_AnimationLookupDict[subID];
                    if (anims.Count > 0)
                        return anims.First();
                }
                return null;
            }

            public Animation SAFE_FindFirstAnimBySubID(int subID) => SAFE(() => INNER_FindFirstAnimBySubID(subID));



            private bool INNER_AnimExists_ByFullID(SplitAnimID id)
            {
                if (NEW_AnimationLookupDict.ContainsKey(id.SubID))
                {
                    var anims = NEW_AnimationLookupDict[id.SubID];
                    if (anims.Count > 0)
                        return true;
                }
                return false;
            }

            public bool SAFE_AnimExists_ByFullID(SplitAnimID id) => SAFE(() => INNER_AnimExists_ByFullID(id));





            public bool INNER_AnimExists_BySubID(int animSubID)
            {
                if (NEW_AnimationLookupDict.ContainsKey(animSubID))
                {
                    var anims = NEW_AnimationLookupDict[animSubID];
                    if (anims.Count > 0)
                        return true;
                }
                return false;
            }

            public bool SAFE_AnimExists_BySubID(int animSubID) => SAFE(() => INNER_AnimExists_BySubID(animSubID));



            public void INNER_RegistAnimModifiedFlag(Animation anim, bool modified)
            {
                if (modified)
                {
                    if (!ModifiedAnimations.Contains(anim))
                        ModifiedAnimations.Add(anim);
                }
                else
                {
                    if (ModifiedAnimations.Contains(anim))
                        ModifiedAnimations.Remove(anim);
                }

                if (ModifiedAnimations.Count > 0)
                {
                    _isModified = true;
                    ParentProj.INNER_RegistAnimCategoryModifiedFlag(this, true);
                }
            }

            public void SAFE_RegistAnimModifiedFlag(Animation anim, bool modified)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_RegistAnimModifiedFlag(anim, modified);
                }
            }





            private byte[] SerializedCache = null;




            private bool INNER_HasSerializedCache()
            {
                return SerializedCache != null;
            }

            public bool SAFE_HasSerializedCache()
            {
                bool result = false;
                lock (ParentProj._lock_DSAProj)
                {
                    result = INNER_HasSerializedCache();
                }
                return result;
            }




            private void INNER_DeleteSerializedCache()
            {
                SerializedCache = null;
            }

            public void SAFE_DeleteSerializedCache()
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_DeleteSerializedCache();
                }
            }




            private void INNER_GenerateSerializedCache()
            {
                SerializedCache = INNER_SerializeToBytes(ParentProj);
            }

            public void SAFE_GenerateSerializedCache()
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_GenerateSerializedCache();
                }
            }




            internal byte[] INNER_GetSerializedCache()
            {
                return SerializedCache?.ToArray();
            }

            public byte[] SAFE_GetSerializedCache()
            {
                byte[] result = null;
                lock (ParentProj._lock_DSAProj)
                {
                    result = INNER_GetSerializedCache();
                }
                return result;
            }




            [JsonIgnore]
            internal bool _isModified = false;



            //[JsonIgnore]
            //public bool IsModified

            //{
            //    get => _isModified;
            //    set
            //    {
            //        // Going from true --> false, wipe the modified states of the animations
            //        if (_isModified && !value)
            //        {
            //            lock (ParentProj._lock_DSAProj)
            //            {
            //                foreach (var a in Animations)
            //                    a._isModified = value;
            //                ModifiedAnimations.Clear();
            //            }
            //        }
            //        _isModified = value;

            //        ParentProj?.SAFE_RegistAnimCategoryModifiedFlag(this, value);

            //        if (value)
            //        {
            //            SAFE_DeleteSerializedCache();
            //        }
            //        //else
            //        //{
            //        //    SAFE_GenerateSerializedCache();
            //        //}
            //    }
            //}





            private List<Animation> ModifiedAnimations = new List<Animation>();
            private List<Animation> ErroredAnimations = new List<Animation>();

            [JsonIgnore]
            public bool IsTreeNodeOpen = false;

            //public void SetIsModified(bool isModified) => IsModified = isModified;
            //public bool GetIsModified() => IsModified;


            public void INNER_SetIsModified(bool value)
            {
                // Going from true --> false, wipe the modified states of the animations
                if (_isModified && !value)
                {
                    lock (ParentProj._lock_DSAProj)
                    {
                        foreach (var a in Animations)
                            a._isModified = value;
                        ModifiedAnimations.Clear();
                    }
                }
                _isModified = value;

                //if (value)
                //{
                //    Console.WriteLine("test");
                //}

                ParentProj?.INNER_RegistAnimCategoryModifiedFlag(this, value);

                if (value)
                {
                    INNER_DeleteSerializedCache();
                }
                //else
                //{
                //    INNER_GenerateSerializedCache();
                //}
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
                return val;
            }
            


            private void INNER_Deserialize(BinaryReaderEx br, SoulsAssetPipeline.Animation.TAE.Template template, DSAProj proj)
            {
                if (proj.FILE_VERSION >= Versions.v20_00_00)
                {
                    Info = new EditorInfo();
                    Info.Deserialize(br, proj);
                }

                if (proj.FILE_VERSION < Versions.v10)
                    throw new InvalidOperationException(
                        $"Tried to load new AnimCategory struct on legacy DSAProj (FILE_VERSION < 10)");

                CategoryID = br.ReadInt32();

                if (proj.FILE_VERSION < Versions.v20_00_00)
                {
                    var categoryName = br.ReadNullPrefixUTF16();
                    Color? customColor = null;
                    if (proj.FILE_VERSION >= Versions.v11)
                        customColor = br.ReadNullPrefixColor();
                    Info = new EditorInfo();
                    Info.DisplayName = categoryName;
                    Info.CustomColor = customColor;
                }

                int animCount = br.ReadInt32();
                INNER_ClearAnimations();
                for (int i = 0; i < animCount; i++)
                {
                    var a = new Animation(proj, this);
                    a.INNER_Deserialize(br, template, proj, this);
                    a.ParentProj = proj;
                    a.ParentCategory = this;
                    INNER_AddAnimation(a, ignoreModified: true);
                }
            }

            public void SAFE_Deserialize(BinaryReaderEx br, SoulsAssetPipeline.Animation.TAE.Template template, DSAProj proj)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_Deserialize(br, template, proj);
                }
            }



            internal void INNER_Serialize(BinaryWriterEx binaryWriter, DSAProj proj)
            {
                var bw = new BinaryWriterEx(false);

                {
                    Info.Serialize(bw, proj);
                    bw.WriteInt32(CategoryID);
                    bw.WriteInt32(Animations.Count);
                    foreach (var a in Animations)
                    {
                        var cache = a.INNER_GetSerializedCache();

                        if (!a.INNER_GetIsModified() && cache != null)
                            bw.WriteBytes(cache);
                        else
                            a.INNER_Serialize(bw, proj);
                    }
                }

                SerializedCache = bw.FinishBytes();
                binaryWriter.WriteBytes(SerializedCache);
            }

            public void SAFE_Serialize(BinaryWriterEx bw, DSAProj proj)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_Serialize(bw, proj);
                }
            }





            private byte[] INNER_SerializeToBytes(DSAProj proj)
            {
                var bw = new BinaryWriterEx(false);
                INNER_Serialize(bw, proj);
                var result = bw.FinishBytes();
                return result;
            }

            public byte[] SAFE_SerializeToBytes(DSAProj proj)
            {
                byte[] result = null;
                lock (ParentProj._lock_DSAProj)
                {
                    result = INNER_SerializeToBytes(proj);
                }
                return result;
            }




            private void INNER_DeserializeFromBytes(byte[] data, SoulsAssetPipeline.Animation.TAE.Template template, DSAProj proj)
            {
                var br = new BinaryReaderEx(false, data);
                INNER_Deserialize(br, template, proj);
            }

            public void SAFE_DeserializeFromBytes(byte[] data, SoulsAssetPipeline.Animation.TAE.Template template, DSAProj proj)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_DeserializeFromBytes(data, template, proj);
                }

            }

            




            private void INNER_ClearRuntimeFlagsOnAll(EditorFlags flags)
            {
                foreach (var animation in Animations)
                    animation.RuntimeFlags &= ~flags;
                RuntimeFlags &= ~flags;
            }

            public void SAFE_ClearRuntimeFlagsOnAll(EditorFlags flags)
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_ClearRuntimeFlagsOnAll(flags);
                }
            }




            private void INNER_ClearAllModified()
            {
                List<Animation> modifiedAnimsClone = ModifiedAnimations.ToList();
                foreach (var animation in modifiedAnimsClone)
                    animation._isModified = false;
                _isModified = false;
                ModifiedAnimations.Clear();
            }
            public void SAFE_ClearAllModified()
            {
                lock (ParentProj._lock_DSAProj)
                {
                    INNER_ClearAllModified();
                }
            }



        }

    }
}
