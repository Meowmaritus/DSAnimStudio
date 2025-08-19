using Microsoft.Xna.Framework;
using Org.BouncyCastle.Asn1.X509;
using SoulsAssetPipeline.Animation;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class AC6NpcPartsEquipper
    {
        public Model MODEL;
        public readonly zzz_DocumentIns Document;
        public AC6NpcPartsEquipper(zzz_DocumentIns doc, int baseChrModelIdx, Model baseModel)
        {
            Document = doc;
            BaseChrModelIdx = baseChrModelIdx;
            MODEL = baseModel;
        }
        
        public void Dispose()
        {
            lock (_lock_Parts)
            {
                if (Parts != null)
                {
                    foreach (var part in Parts)
                    {
                        part?.Dispose();
                    }
                    Parts = null;
                }
            }
                
        }
        
        public bool EnablePartsFileCaching = true;

        private object _lock_Parts = new object();

        public bool DebugShowNonattachedParts = false;

        public int BaseChrModelIdx = 0;
        
        private Part[] Parts = new Part[32];

        public void AccessModelOfPart(int index, Action<Part, Model> doAction)
        {
            lock (_lock_Parts)
            {
                if (EquipPartsParam == null)
                    return;
                var part = Parts[index];
                if (part != null && part.Mdl != null)
                {
                    doAction(part, part.Mdl);
                }
            }
        }
        
        
        
        public NewDummyPolyManager GetDummyPolySpawnPlace(ParamData.AtkParam.DummyPolySource defaultDummySource, int dmy, NewDummyPolyManager bodyDmyForFallback)
        {
            NewDummyPolyManager wpnDmy = null;
            if (defaultDummySource is >= ParamData.AtkParam.DummyPolySource.AC6Parts0
                and <= ParamData.AtkParam.DummyPolySource.AC6Parts31)
            {
                int partIndex = defaultDummySource - ParamData.AtkParam.DummyPolySource.AC6Parts0;
                lock (_lock_Parts)
                {
                    wpnDmy = Parts[partIndex]?.Mdl?.DummyPolyMan;
                }
            }
            
            if (wpnDmy == null)
                wpnDmy = bodyDmyForFallback;

            if (!wpnDmy.DummyPolyByRefID.ContainsKey(dmy))
                return bodyDmyForFallback;
            else
                return wpnDmy;
        }

        public void Update(float timeDelta)
        {
            AccessModelsOfAllParts((partIndex, part, wpnMdl) =>
            {
                if (wpnMdl == null)
                    return;

                

                


                if (wpnMdl != null && wpnMdl.AnimContainer != null)
                {
                    if (!wpnMdl.ApplyBindPose)
                    {
                        wpnMdl.NewScrubSimTime(absolute: false, timeDelta, foreground: false, background: true, out _);

                        if (MODEL.AnimContainer.CurrentAnimDuration.HasValue && wpnMdl.AnimContainer.CurrentAnimDuration.HasValue)
                        {
                            float curModTime = MODEL.AnimContainer.CurrentAnimTime % MODEL.AnimContainer.CurrentAnimDuration.Value;
                            
                            // Limit time
                            if (curModTime > wpnMdl.AnimContainer.CurrentAnimDuration.Value)
                                curModTime = wpnMdl.AnimContainer.CurrentAnimDuration.Value;
                            
                            wpnMdl.NewScrubSimTime(absolute: true, curModTime, foreground: true, background: false, out _);
                        }
                        
                    }

                    //if (wpnMdl.TaeManager_ForParts != null)
                    //    wpnMdl.TaeManager_ForParts.UpdateTae();
                }

                //wpnMdl.DummyPolyMan?.UpdateAllHitPrims();

                //wpnMdl.NewUpdateByAnimTick();
            });
        }
        
        public void AccessModelsOfAllParts(Action<int, Part, Model> doAction)
        {
            lock (_lock_Parts)
            {
                if (EquipPartsParam == null)
                    return;
                for (int i = 0; i < Parts.Length; i++)
                {
                    if (Parts[i] != null && Parts[i].Mdl != null)
                    {
                        doAction(i, Parts[i], Parts[i].Mdl);
                    }
                }
            }
        }
        
        private ParamData.AC6NpcEquipPartsParam EquipPartsParam;

        public int EquipID;


        public ParamData.AtkParam.DummyPolySource SelectedDummyPolySource
        {
            get
            {
                ParamData.AtkParam.DummyPolySource result = ParamData.AtkParam.DummyPolySource.BaseModel;
                lock (_lock_ImguiDummyPolySourceFieldThing)
                {
                    result = (DummyPolySource_ImguiListIndex >= 0 && DummyPolySource_ImguiListIndex < DummyPolySource_ValueMap.Count)
                        ? DummyPolySource_ValueMap[DummyPolySource_ImguiListIndex] : ParamData.AtkParam.DummyPolySource.BaseModel;
                }
                return result;
            }
        }

        

        //private List<Part> DummyPolySource_PartMap = new List<Part>();
        private List<ParamData.AtkParam.DummyPolySource> DummyPolySource_ValueMap = new List<ParamData.AtkParam.DummyPolySource>();
        private int DummyPolySource_ImguiListIndex = -1;
        private string[] DummyPolySource_ImguiListStrings = new string[0];

        private object _lock_ImguiDummyPolySourceFieldThing = new object();

        public void DoImguiDummyPolySourceField(ref bool anyFieldFocused)
        {
            lock (_lock_ImguiDummyPolySourceFieldThing)
            {
                ImGuiNET.ImGui.Text("Behavior Source Model:");
                ImGuiNET.ImGui.PushItemWidth(400 * Main.DPI);
                ImGuiNET.ImGui.ListBox($"##{MODEL.GUID}_AC6NpcPartsEquipper_BehaviorSel", ref DummyPolySource_ImguiListIndex, 
                    DummyPolySource_ImguiListStrings, DummyPolySource_ImguiListStrings.Length);
                ImGuiNET.ImGui.PopItemWidth();
                anyFieldFocused |= ImGuiNET.ImGui.IsItemActive();
            }
        }

        private void InitImguiDummyPolySourceField()
        {
            var origSelectedValue = (DummyPolySource_ImguiListIndex >= 0 && DummyPolySource_ImguiListIndex < DummyPolySource_ValueMap.Count) 
                ? DummyPolySource_ValueMap[DummyPolySource_ImguiListIndex] : ParamData.AtkParam.DummyPolySource.BaseModel;

            lock (_lock_ImguiDummyPolySourceFieldThing)
            {
                DummyPolySource_ValueMap.Clear();
                var stringList = new List<string>();

                DummyPolySource_ValueMap.Add(ParamData.AtkParam.DummyPolySource.BaseModel); 
                stringList.Add($"Base Character Model [{MODEL.Name}]");

                lock (_lock_Parts)
                {

                    for (int i = 0; i < 32; i++)
                    {
                        if (Parts[i] != null && Parts[i].Mdl != null)
                        {
                            DummyPolySource_ValueMap.Add(ParamData.AtkParam.DummyPolySource.AC6Parts0 + i);
                            stringList.Add($"NPC Equip Part {i} [{Parts[i].Mdl.Name}]");
                        }
                    }

                }

                DummyPolySource_ImguiListStrings = stringList.ToArray();
                DummyPolySource_ImguiListIndex = DummyPolySource_ValueMap.IndexOf(origSelectedValue);
            }
        }

        public class Part
        {

            public Model Mdl = null;
            public ParamData.AC6NpcEquipPartsParam.PartInfo PartInfo;

            public ParamData.AC6AttachObj AttachObjEntry = null;

            public bool SuccessfullyAttached = false;

            public void Dispose()
            {
                Mdl?.Dispose();
                Mdl = null;
            }

            public void Draw(bool showIfNotAttached)
            {
                if (Mdl != null && (showIfNotAttached || SuccessfullyAttached))
                    Mdl.Draw(0, false, false, false, basePlayerModel: null);
            }

            public void UpdateAttach(Model target)
            {
                var attachSuccess = false;
                int defaultDummyPolyID = PartInfo.TargetMountDummyPoly;
                var defaultDummyPoly = target.DummyPolyMan.GetDummyMatricesByID(defaultDummyPolyID, ignoreAC6NpcParts: true);
                if (defaultDummyPoly.Count > 0)
                {
                    var defaultDestMatrix = defaultDummyPoly[0] * target.CurrentTransform.WorldMatrix;

                    if (Mdl != null)
                    {
                        //if (PartInfo.BreakDummyPolyID > 0)
                        //{
                        //    var dummyPolySrc = Mdl.DummyPolyMan.GetDummyMatricesByID(PartInfo.BreakDummyPolyID);
                        //    if (dummyPolySrc.Count > 0)
                        //        destMatrix = Matrix.Invert(dummyPolySrc[0]) * destMatrix;
                        //}

                        Mdl.CurrentTransform = Mdl.StartTransform = new Transform(defaultDestMatrix);
                        //attachSuccess = true;
                        if (AttachObjEntry != null)
                        {

                            for (int i = 0; i < 8; i++)
                            {
                                if (AttachObjEntry.AttachDmyID[i] == 0)
                                    continue;
                                if (string.IsNullOrWhiteSpace(AttachObjEntry.ObjName[i]))
                                    continue;

                                var destDummyPoly = target.DummyPolyMan.GetDummyMatricesByID(AttachObjEntry.AttachDmyID[i], ignoreAC6NpcParts: true);
                                if (destDummyPoly.Count > 0)
                                {

                                    var boneName = AttachObjEntry.ObjName[i];
                                    if (Mdl.AnimContainer.Skeleton.BoneIndices_ByName.ContainsKey(boneName))
                                    {
                                        var boneIndex = Mdl.AnimContainer.Skeleton.BoneIndices_ByName[boneName];
                                        if (boneIndex >= 0)
                                        {
                                            Mdl.AnimContainer.Skeleton.Bones[boneIndex].MasterCompleteOverrideFK =
                                                (destDummyPoly[0] * target.CurrentTransform.WorldMatrix) * Matrix.Invert(defaultDestMatrix);

                                            //Mdl.SkeletonFlver.Bones[boneIndex].MasterCompleteOverrideFK = null;

                                            attachSuccess = true;
                                        }
                                        else
                                        {
                                            Console.WriteLine("WHAT THE FUCK");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("WHAT THE FUCK");
                                    }


                                }




                            }

                        }
                        else
                        {
                            attachSuccess = true;
                        }

                        //Mdl.CurrentTransform = target.CurrentTransform;
                        
                    }

                    
                }

                if (attachSuccess && Mdl != null)
                {
                    Mdl.NewForceSyncUpdate();
                    //Mdl.AnimContainer.Update();
                    //if (Mdl.AnimContainer.CurrentAnimation == null)
                    //{
                    //    Mdl.AnimContainer.Skeleton.CalculateFKFromLocalTransforms();
                    //}
                    //else
                    //{
                    //    Mdl.AnimContainer.Update();
                    //}
                    //Mdl.AnimContainer.Skeleton.CalculateFKFromLocalTransforms();
                }

                SuccessfullyAttached = attachSuccess;
            }
        }

        public void UpdateModels()
        {
            int lastEquipID = -1;
            lock (_lock_Parts)
            {
                lastEquipID = EquipPartsParam?.ID ?? -1;
            }
            
            if (EquipID != lastEquipID)
            {
                if (EquipID >= 0 && zzz_DocumentManager.CurrentDocument.ParamManager.AC6NpcEquipPartsParam.ContainsKey(EquipID))
                    SwitchToPartListList(zzz_DocumentManager.CurrentDocument.ParamManager.AC6NpcEquipPartsParam[EquipID]);
                else
                    SwitchToPartListList(null);
            }
        }
        
        private void SwitchToPartListList(ParamData.AC6NpcEquipPartsParam partList)
        {
            zzz_DocumentManager.CurrentDocument.LoadingTaskMan.DoLoadingTask("AC6NpcPartsEquipper_SwitchToPartListList", "Updating AC6 NPC Parts...", progress =>
            {
                lock (_lock_Parts)
                {
                    EquipPartsParam = partList;
                }

                

                int progIndex = 0;

                for (int i = 0; i < 32; i++)
                {
                    AddPart(i, partList?.Parts[i] ?? new ParamData.AC6NpcEquipPartsParam.PartInfo());
                    
                    progress?.Report(1.0 * (i + 1) / 32.0);
                }
            });

                
        }
        
        public void SelectAnimation(SplitAnimID baseCharacterAnimTaeID)
        {
            AccessModelsOfAllParts((partIndex, part, model) =>
            {
                model?.TaeManager_ForParts?.SelectTaeAnimation(Document, baseCharacterAnimTaeID, forceNew: true);
            });
        }

        public void AddPart(int index, ParamData.AC6NpcEquipPartsParam.PartInfo partInfo)
        {

            
            var newPart = new Part();
            newPart.PartInfo = partInfo;

            if (partInfo.ModelID > 0)
            {
                try
                {
                    var partbnd = newPart.PartInfo.GetPartBnd(!EnablePartsFileCaching);
                    var partName = newPart.PartInfo.GetPartBndName();
                    if (partbnd != null)
                    {
                        var newlyLoadedModel = new Model(Document, null, partName, partbnd, modelIndex: BaseChrModelIdx, null,
                            ignoreStaticTransforms: true);
                        if (newlyLoadedModel.AnimContainer == null)
                        {
                            newlyLoadedModel?.Dispose();
                            newlyLoadedModel = null;
                        }
                        else
                        {
                            //newModelToLoad.IS_PLAYER_WEAPON = true;
                            //newModelToLoad.DummyPolyMan.GlobalDummyPolyIDOffset = 10000;
                            newlyLoadedModel.DummyPolyMan.GlobalDummyPolyIDPrefix = $"{partName}";
                        }

                        if (newlyLoadedModel != null && newlyLoadedModel.MainMesh != null)
                        {
                            newlyLoadedModel.MainMesh.Name = newlyLoadedModel.Name;
                            newlyLoadedModel.TaeManager_ForParts = NewChrAsmWpnTaeManager.LoadPartsbnd(Document, newlyLoadedModel, BaseChrModelIdx, partbnd);
                            newlyLoadedModel.ModelType = Model.ModelTypes.AC6NpcEquipModel;
                            //newlyLoadedModel.PARENT_PLAYER_MODEL
                        }

                        newPart.Mdl = newlyLoadedModel;

                    }
                }
                catch
                {
                    var previouslyLoadedModel = newPart.Mdl;
                    newPart.Mdl = null;
                    previouslyLoadedModel?.Dispose();
                    newPart = null;
                }

                
            }


            if (Document.ParamManager.AC6AttachObjParam_Npc.ContainsKey(newPart.PartInfo.AttachObjID))
            {
                newPart.AttachObjEntry = Document.ParamManager.AC6AttachObjParam_Npc[newPart.PartInfo.AttachObjID];
            }



            lock (_lock_Parts)
            {
                var oldPart = Parts[index];
                Parts[index] = (newPart);
                oldPart?.Dispose();
            }

            InitImguiDummyPolySourceField();
        }

        

        public void UpdateAttach(Model target)
        {
            lock (_lock_Parts)
            {
                var parts = Parts.ToList();
                foreach (var p in parts)
                {
                    p?.UpdateAttach(target);
                }
            }
        }

        public void Draw()
        {
            lock (_lock_Parts)
            {
                var parts = Parts.ToList();
                foreach (var p in parts)
                {
                    p?.Draw(DebugShowNonattachedParts);
                }
            }
        }

    }
}
