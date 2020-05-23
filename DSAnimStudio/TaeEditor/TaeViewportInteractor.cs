using DSAnimStudio.DbgMenus;
using Microsoft.Xna.Framework;
using SoulsFormats;
using SFAnimExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace DSAnimStudio.TaeEditor
{
    public class TaeViewportInteractor
    {
        public readonly TaeEditAnimEventGraph Graph;

        public enum TaeEntityType
        {
            NONE,
            PC,
            NPC,
            OBJ,
            PARTS,
            REMO
        }

        public TaeEntityType EntityType { get; private set; } = TaeEntityType.NONE;

        public NewChrAsmEquipForm EquipForm = null;

        public TaeEventSimulationEnvironment EventSim { get; private set; }

        public List<ParamData.NpcParam> PossibleNpcParams = new List<ParamData.NpcParam>();
        private int _selectedNpcParamIndex = -1;
        public int SelectedNpcParamIndex
        {
            get => _selectedNpcParamIndex;
            set
            {
                if (CurrentModel != null)
                {
                    CurrentModel.NpcParam = (value >= 0 && value < PossibleNpcParams.Count)
                        ? PossibleNpcParams[value] : null;
                    _selectedNpcParamIndex = value;
                    if (CurrentModel.NpcParam != null)
                    {
                        //CurrentModel.DummyPolyMan.RecreateAllHitboxPrimitives(CurrentModel.NpcParam);
                        CurrentModel.NpcParam.ApplyMaskToModel(CurrentModel);
                    }
                }
            }
        }

        public Model CurrentModel => Scene.Models.Count > 0 ? Scene.Models[0] : null;

        private void SetEntityType(TaeEntityType entityType)
        {
            EntityType = entityType;

            if (entityType != TaeEntityType.NPC)
            {
                PossibleNpcParams.Clear();
                SelectedNpcParamIndex = -1;
            }

            if (entityType != TaeEntityType.PC)
            {
                EquipForm?.Close();
                EquipForm?.Dispose();
                EquipForm = null;
            }

            Graph.MainScreen.GameWindowAsForm.Invoke(new Action(() =>
            {
                Graph.MainScreen.MenuBar["NPC Settings"].Enabled = entityType == TaeEntityType.NPC;
                Graph.MainScreen.MenuBar["NPC Settings"].Visible = entityType == TaeEntityType.NPC;

                Graph.MainScreen.MenuBar["Player Settings"].Enabled = entityType == TaeEntityType.PC;
                Graph.MainScreen.MenuBar["Player Settings"].Visible = entityType == TaeEntityType.PC;

                Graph.MainScreen.MenuBar["Object Settings"].Enabled = entityType == TaeEntityType.OBJ;
                Graph.MainScreen.MenuBar["Object Settings"].Visible = entityType == TaeEntityType.OBJ;

                Graph.MainScreen.MenuBar["Animated Equipment Settings"].Enabled = entityType == TaeEntityType.PARTS;
                Graph.MainScreen.MenuBar["Animated Equipment Settings"].Visible = entityType == TaeEntityType.PARTS;

                Graph.MainScreen.MenuBar["Cutscene Settings"].Enabled = entityType == TaeEntityType.REMO;
                Graph.MainScreen.MenuBar["Cutscene Settings"].Visible = entityType == TaeEntityType.REMO;
            }));
          
        }

        public TaeViewportInteractor(TaeEditAnimEventGraph graph)
        {
            Graph = graph;
            Graph.PlaybackCursor.PlaybackStarted += PlaybackCursor_PlaybackStarted;
            Graph.PlaybackCursor.PlaybackFrameChange += PlaybackCursor_PlaybackFrameChange;
            Graph.PlaybackCursor.ScrubFrameChange += PlaybackCursor_ScrubFrameChange;
            Graph.PlaybackCursor.PlaybackEnded += PlaybackCursor_PlaybackEnded;
            Graph.PlaybackCursor.EventBoxEnter += PlaybackCursor_EventBoxEnter;
            Graph.PlaybackCursor.EventBoxMidst += PlaybackCursor_EventBoxMidst;
            Graph.PlaybackCursor.EventBoxExit += PlaybackCursor_EventBoxExit;
            Graph.PlaybackCursor.PlaybackLooped += PlaybackCursor_PlaybackLooped;

            //V2.0
            //NewAnimationContainer.AutoPlayAnimContainersUponLoading = false;

            Scene.ClearScene();
            TexturePool.Flush();

            var shortFileName = Utils.GetFileNameWithoutAnyExtensions(
                Utils.GetFileNameWithoutDirectoryOrExtension(Graph.MainScreen.FileContainerName)).ToLower();

            var fileName = Graph.MainScreen.FileContainerName.ToLower();

            if (shortFileName.StartsWith("c"))
            {
                GameDataManager.LoadCharacter(shortFileName.Substring(0, 5));

                if (CurrentModel.IS_PLAYER)
                {
                    PossibleNpcParams.Clear();
                    SelectedNpcParamIndex = -1;

                    CurrentModel.CreateChrAsm();

                    CurrentModel.ChrAsm.EquipmentModelsUpdated += ChrAsm_EquipmentModelsUpdated;

                    if (!Graph.MainScreen.Config.ChrAsmConfigurations.ContainsKey(GameDataManager.GameType))
                    {
                        Graph.MainScreen.Config.ChrAsmConfigurations.Add
                            (GameDataManager.GameType, new NewChrAsmCfgJson());
                    }

                    Graph.MainScreen.Config.ChrAsmConfigurations[GameDataManager.GameType]
                        .WriteToChrAsm(CurrentModel.ChrAsm);

                    CurrentModel.ChrAsm.UpdateModels(isAsync: true);

                    SetEntityType(TaeEntityType.PC);

                    Graph.MainScreen.GameWindowAsForm.Invoke(new Action(() =>
                    {
                        EquipForm?.Dispose();
                        EquipForm = null;

                        EquipForm = new NewChrAsmEquipForm();
                        EquipForm.Owner = Graph.MainScreen.GameWindowAsForm;
                        EquipForm.Hidden += EquipForm_Hidden;
                        //EquipForm.FormClosing += EquipForm_FormClosing;
                    }));
                }
                else
                {
                    PossibleNpcParams = ParamManager.FindNpcParams(CurrentModel.Name);

                    

                    if (PossibleNpcParams.Count == 0)
                    {
                        var cname = CurrentModel.Name;
                        var cname0 = CurrentModel.Name.Substring(0, 4) + "0";
                        var dlgres = System.Windows.Forms.MessageBox.Show(
                            $"No NpcParams matched for {cname}.\nWould you like to try to use NpcParams matching {cname0}?",
                            "No NpcParams Matched", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
                        if (dlgres == System.Windows.Forms.DialogResult.Yes)
                        {
                            PossibleNpcParams = ParamManager.FindNpcParams(CurrentModel.Name, matchCXXX0: true);
                        }
                    }

                    if (PossibleNpcParams.Count > 0)
                        SelectedNpcParamIndex = 0;
                    else
                        SelectedNpcParamIndex = -1;

                    SetEntityType(TaeEntityType.NPC);

                    var validNpcParams = PossibleNpcParams;

                    var materialsPerMask = CurrentModel.GetMaterialNamesPerMask();

                    var masksEnabledOnAllNpcParams = materialsPerMask.Select(kvp => kvp.Key).ToList();
                    foreach (var kvp in materialsPerMask)
                    {
                        if (kvp.Key < 0)
                            continue;

                        foreach (var npcParam in validNpcParams)
                        {
                            if (npcParam.DrawMask.Length <= kvp.Key || !npcParam.DrawMask[kvp.Key])
                            {
                                if (masksEnabledOnAllNpcParams.Contains(kvp.Key))
                                    masksEnabledOnAllNpcParams.Remove(kvp.Key);

                                break;
                            }
                        }
                    }

                    var behaviorVariationChoicesDict = new Dictionary<string, Action>();

                    foreach (var npc in validNpcParams)
                    {
                        behaviorVariationChoicesDict.Add($"{npc.GetDisplayName()}|" +
                            npc.GetMaskString(materialsPerMask, masksEnabledOnAllNpcParams) +
                            $"\nBehaviorVariationID: {npc.BehaviorVariationID}",
                            () =>
                            {
                                CurrentModel.NpcParam = npc;
                                CurrentModel.NpcParam.ApplyMaskToModel(CurrentModel);
                            });
                    }

                    Graph.MainScreen.GameWindowAsForm.Invoke(new Action(() =>
                    {
                        EquipForm?.Dispose();
                        OnEquipFormClose();
                        EquipForm = null;

                        var menuItemLoadTextures = Graph.MainScreen.MenuBar["NPC Settings\\Load Additional Texture File(s)..."];

                        Graph.MainScreen.MenuBar.ClearItem("NPC Settings");

                        Graph.MainScreen.MenuBar.AddItem("NPC Settings", menuItemLoadTextures);

                        Graph.MainScreen.MenuBar.AddSeparator("NPC Settings");

                        
                        if (CurrentModel.NpcParam != null)
                        {
                            Graph.MainScreen.MenuBar.AddItem("NPC Settings", "NpcParam", behaviorVariationChoicesDict,
                            () => $"{CurrentModel.NpcParam.GetDisplayName()}|" +
                            CurrentModel.NpcParam.GetMaskString(materialsPerMask, masksEnabledOnAllNpcParams) +
                            $"\nBehaviorVariationID: {CurrentModel.NpcParam.BehaviorVariationID}");
                        }
                        else
                        {
                            Graph.MainScreen.MenuBar.AddItem("NPC Settings", "NpcParam (None Found)");
                        }

                        Graph.MainScreen.MenuBar.AddItem("NPC Settings\\Override Draw Mask", "Show All", () =>
                        {
                            for (int i = 0; i < CurrentModel.DrawMask.Length; i++)
                            {
                                CurrentModel.DrawMask[i] = true;
                            }
                        }, closeOnClick: false);

                    }));

                    foreach (var npc in validNpcParams)
                    {
                        Graph.MainScreen.GameWindowAsForm.Invoke(new Action(() =>
                        {
                            Graph.MainScreen.MenuBar.AddItem("NPC Settings\\Override Draw Mask",
                                $"{npc.GetDisplayName()}|{npc.GetMaskString(materialsPerMask, masksEnabledOnAllNpcParams)}", () =>
                                {
                                    npc.ApplyMaskToModel(CurrentModel);
                                }, closeOnClick: false);
                        }));
                    }

                    //foreach (var npc in validNpcParams)
                    //{
                    //    Graph.MainScreen.MenuBar.AddItem("Behavior Variation ID", $"Apply Behavior Variation ID " +
                    //        $"from NpcParam {npc.ID} {npc.Name}", () =>
                    //    {
                    //        npc.ApplyMaskToModel(CurrentModel);
                    //    });
                    //}
                    

                    //throw new NotImplementedException("Implement NPC param change you lazy fuck :tremblecat:");
                }

                CurrentModel.AfterAnimUpdate(timeDelta: 0);

                GFX.World.ModelCenter_ForOrbitCam = CurrentModel.MainMesh.Bounds.GetCenter();
                GFX.World.ModelHeight_ForOrbitCam = CurrentModel.MainMesh.Bounds.Max.Y - CurrentModel.MainMesh.Bounds.Min.Y;
                GFX.World.ModelDepth_ForOrbitCam = (CurrentModel.MainMesh.Bounds.Max.Z - CurrentModel.MainMesh.Bounds.Min.Z) * 1.5f;
                GFX.World.OrbitCamReset();

                FmodManager.Purge();
                FmodManager.LoadMainFEVs();
                FmodManager.LoadInterrootFEV(CurrentModel.Name);
            }
            else if (shortFileName.StartsWith("o"))
            {
                SetEntityType(TaeEntityType.OBJ);

                GameDataManager.LoadObject(shortFileName);

                GFX.World.ModelCenter_ForOrbitCam = CurrentModel.MainMesh.Bounds.GetCenter();
                GFX.World.ModelHeight_ForOrbitCam = CurrentModel.MainMesh.Bounds.Max.Y - CurrentModel.MainMesh.Bounds.Min.Y;
                GFX.World.ModelDepth_ForOrbitCam = (CurrentModel.MainMesh.Bounds.Max.Z - CurrentModel.MainMesh.Bounds.Min.Z) * 1.5f;
                GFX.World.OrbitCamReset();

                //throw new NotImplementedException("OBJECTS NOT SUPPORTED YET");
            }
            else if (fileName.EndsWith(".partsbnd") || fileName.EndsWith(".partsbnd.dcx"))
            {
                SetEntityType(TaeEntityType.PARTS);

                throw new NotImplementedException("PARTS NOT SUPPORTED YET");
            }
            else if (fileName.EndsWith(".remobnd") || fileName.EndsWith(".remobnd.dcx"))
            {
                SetEntityType(TaeEntityType.REMO);

                throw new NotImplementedException("REMO NOT SUPPORTED YET");
            }
        }

        private void ChrAsm_EquipmentModelsUpdated(object sender, EventArgs e)
        {
            Graph.MainScreen.GameWindowAsForm.Invoke(new Action(() =>
            {
                //var thingToKeep = Graph.MainScreen.MenuBar["Player Settings/Show Equip Change Menu"];
                Graph.MainScreen.MenuBar.ClearItem("Player Settings\\Select Right Weapon Anim");
                Graph.MainScreen.MenuBar.ClearItem("Player Settings\\Select Left Weapon Anim");
                //Graph.MainScreen.MenuBar.AddItem("Player Settings", thingToKeep);
                //Graph.MainScreen.MenuBar.AddSeparator("Player Settings");

                // Right weapon
                //Graph.MainScreen.MenuBar.ClearItem("Player Settings/Select Right Weapon Anim");
                var rightAnims = CurrentModel?.ChrAsm?.RightWeaponModel?.AnimContainer?.Animations;
                if (rightAnims != null && rightAnims.Count > 0)
                {
                    var choicesDict = new Dictionary<string, Action>();
                    foreach (var a in rightAnims.Keys)
                    {
                        choicesDict.Add(a, () => CurrentModel.ChrAsm.RightWeaponModel.AnimContainer.CurrentAnimationName = a);
                    }

                    Graph.MainScreen.MenuBar.AddItem(
                           "Player Settings",
                           "Select Right Weapon Anim",
                           choicesDict,
                           () => CurrentModel.ChrAsm.RightWeaponModel.AnimContainer.CurrentAnimationName);
                }
                else
                {
                    Graph.MainScreen.MenuBar.AddItem("Player Settings\\Select Right Weapon Anim", "Weapon is not animated.");
                }

                //Left Weapon
                //Graph.MainScreen.MenuBar.ClearItem("Player Settings/Select Left Weapon Anim");
                var leftAnims = CurrentModel?.ChrAsm?.LeftWeaponModel?.AnimContainer?.Animations;
                if (leftAnims != null && leftAnims.Count > 0)
                {
                    var choicesDict = new Dictionary<string, Action>();
                    foreach (var a in leftAnims.Keys)
                    {
                        choicesDict.Add(a, () => CurrentModel.ChrAsm.LeftWeaponModel.AnimContainer.CurrentAnimationName = a);
                    }

                    Graph.MainScreen.MenuBar.AddItem(
                           "Player Settings",
                           "Select Left Weapon Anim",
                           choicesDict,
                           () => CurrentModel.ChrAsm.LeftWeaponModel.AnimContainer.CurrentAnimationName);
                }
                else
                {
                    Graph.MainScreen.MenuBar.AddItem("Player Settings\\Select Left Weapon Anim", "Weapon is not animated.");
                }
            }));

            //V2.0: Scrub weapon anims to the current frame.

            if (CurrentModel.ChrAsm.RightWeaponModel != null)
            {
                CurrentModel.ChrAsm.RightWeaponModel.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            }

            if (CurrentModel.ChrAsm.LeftWeaponModel != null)
            {
                CurrentModel.ChrAsm.LeftWeaponModel.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            }

            //V2.0: Update stuff probably
            CurrentModel.AfterAnimUpdate(0);
        }

        //private void EquipForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        public void BringUpEquipForm()
        {
            if (EquipForm == null)
            {
                throw new Exception("ERROR: Equip menu failed to initialize (please report).");
            }

            if (Graph.MainScreen.MenuBar["Player Settings\\Show Equip Change Menu"].Checked)
            {
                Graph.MainScreen.MenuBar["Player Settings\\Show Equip Change Menu"].Checked = false;

                EquipForm.Close();
            }
            else
            {
                Graph.MainScreen.MenuBar["Player Settings\\Show Equip Change Menu"].Checked = true;

                if (EquipForm.ChrAsm != CurrentModel.ChrAsm)
                {
                    EquipForm.ChrAsm = CurrentModel.ChrAsm;
                    EquipForm.WriteChrAsmToGUI();
                }

                EquipForm.Show();
            }
        }

        private void OnEquipFormClose()
        {
            Graph.MainScreen.GameWindowAsForm.Invoke(new Action(() =>
            {
                Graph.MainScreen.MenuBar["Player Settings\\Show Equip Change Menu"].Checked = false;
            }));
        }

        private void EquipForm_Hidden(object sender, EventArgs e)
        {
            OnEquipFormClose();
        }

        public void SaveChrAsm()
        {
            if (CurrentModel.ChrAsm != null)
            {
                if (!Graph.MainScreen.Config.ChrAsmConfigurations.ContainsKey(GameDataManager.GameType))
                {
                    Graph.MainScreen.Config.ChrAsmConfigurations.Add
                        (GameDataManager.GameType, new NewChrAsmCfgJson());
                }

                Graph.MainScreen.Config.ChrAsmConfigurations[GameDataManager.GameType].CopyFromChrAsm(CurrentModel.ChrAsm);
            }
        }

        private void PlaybackCursor_PlaybackFrameChange(object sender, EventArgs e)
        {
            Graph.PlaybackCursor.HkxAnimationLength = CurrentModel?.AnimContainer?.CurrentAnimDuration;
            Graph.PlaybackCursor.SnapInterval = CurrentModel?.AnimContainer?.CurrentAnimFrameDuration;

            var timeDelta = (float)(Graph.PlaybackCursor.GUICurrentTime - Graph.PlaybackCursor.OldGUICurrentTime);

            //V2.0
            //CurrentModel.AnimContainer.IsPlaying = false;
            CurrentModel.AnimContainer.ScrubRelative(timeDelta);

            CurrentModel.AfterAnimUpdate(timeDelta);

            //V2.0
            //CurrentModel.ChrAsm?.UpdateWeaponTransforms(timeDelta);

            CheckSimEnvironment();
            EventSim.OnSimulationFrameChange(Graph.EventBoxesToSimulate, (float)Graph.PlaybackCursor.CurrentTimeMod);

            UpdateCombo();
        }

        private void CheckSimEnvironment()
        {
            if (EventSim == null || EventSim.MODEL != CurrentModel)
            {
                EventSim = new TaeEventSimulationEnvironment(Graph, CurrentModel);
                Graph.MainScreen.GameWindowAsForm.Invoke(new Action(() =>
                {
                    EventSim.BuildEventSimMenuBar(Graph.MainScreen.MenuBar);
                }));
            }
        }

        private void PlaybackCursor_EventBoxExit(object sender, TaeEditAnimEventBox e)
        {
            CheckSimEnvironment();
            EventSim.OnEventExit(e);
        }

        private void PlaybackCursor_EventBoxMidst(object sender, TaeEditAnimEventBox e)
        {
            CheckSimEnvironment();
            EventSim.OnEventMidFrame(e);
        }

        private void PlaybackCursor_EventBoxEnter(object sender, TaeEditAnimEventBox e)
        {
            CheckSimEnvironment();
            EventSim.OnEventEnter(e);
        }

        private void PlaybackCursor_PlaybackEnded(object sender, EventArgs e)
        {
            CheckSimEnvironment();
            EventSim.OnSimulationEnd(Graph.EventBoxesToSimulate);
        }

        public void OnScrubFrameChange(float? forceCustomTimeDelta = null)
        {
            Graph.PlaybackCursor.HkxAnimationLength = CurrentModel?.AnimContainer?.CurrentAnimDuration;
            Graph.PlaybackCursor.SnapInterval = CurrentModel?.AnimContainer?.CurrentAnimFrameDuration;

            var timeDelta = forceCustomTimeDelta ?? (float)(Graph.PlaybackCursor.GUICurrentTime - Graph.PlaybackCursor.OldGUICurrentTime);

            //V2.0
            //CurrentModel.AnimContainer.IsPlaying = false;
            CurrentModel.AnimContainer.ScrubRelative(timeDelta);

            CurrentModel.AfterAnimUpdate(timeDelta);

            //V2.0
            //CurrentModel.ChrAsm?.UpdateWeaponTransforms(timeDelta);

            CheckSimEnvironment();
            EventSim.OnSimulationFrameChange(Graph.EventBoxesToSimulate, (float)Graph.PlaybackCursor.GUICurrentTimeMod);

            
        }

        public void StartCombo(bool isLoop, TaeComboEntry[] entries)
        {
            CurrentComboLoop = isLoop;
            CurrentCombo = entries;
            CurrentComboIndex = 0;
            StartCurrentComboEntry();
            RemoveTransition();
        }

        private void StartCurrentComboEntry()
        {
            if (Graph.MainScreen.GotoAnimID(int.Parse(CurrentCombo[CurrentComboIndex].AnimID.Replace("_", "").Replace("a", "")), false))
            {
                if (!Graph.PlaybackCursor.IsPlaying)
                {
                    Graph.PlaybackCursor.Transport_PlayPause();
                }
            }
            else
            {
                CurrentComboIndex = -1;
            }
        }

        public TaeComboEntry[] CurrentCombo = new TaeComboEntry[0];
        public int CurrentComboIndex = -1;
        public bool CurrentComboLoop = false;

        private void GoToNextItemInCombo()
        {
            CurrentComboIndex++;
            if (CurrentComboIndex < CurrentCombo.Length)
            {
                StartCurrentComboEntry();
            }
            else
            {
                if (CurrentComboLoop)
                {
                    CurrentComboIndex = 0;
                    StartCurrentComboEntry();
                }
                else
                {
                    if (Graph.PlaybackCursor.IsPlaying)
                        Graph.PlaybackCursor.Transport_PlayPause();
                    CurrentComboIndex = -1;
                }

                
            }
        }

        private void UpdateCombo()
        {
            if (!Graph.PlaybackCursor.IsPlaying || Graph.PlaybackCursor.Scrubbing)
                return;

            if (CurrentComboIndex >= 0)
            {
                if (CurrentComboIndex < CurrentCombo.Length)
                {

                    if (Graph.PlaybackCursor.CurrentTime >= (Graph.PlaybackCursor.MaxTime - Graph.PlaybackCursor.CurrentSnapInterval))
                    {
                        GoToNextItemInCombo();
                        return;
                    }
                    else if (CurrentComboIndex < CurrentCombo.Length - 1 || CurrentComboLoop)
                    {
                        foreach (var eventBox in Graph.EventBoxesToSimulate)
                        {
                            if (eventBox.PlaybackHighlight && eventBox.MyEvent.Type == 0)
                            {
                                var cancelTypeAsStr = eventBox.MyEvent.Template["JumpTableID"].ValueToString(eventBox.MyEvent.Parameters["JumpTableID"]);
                                if (cancelTypeAsStr == CurrentCombo[CurrentComboIndex].Event0CancelType)
                                {
                                    GoToNextItemInCombo();
                                    return;
                                }
                            }
                        }

                    }




                }
                else
                {
                    CurrentComboIndex = -1;
                }


                
            }
        }

        private void PlaybackCursor_ScrubFrameChange(object sender, EventArgs e)
        {
            OnScrubFrameChange();
        }

        private void PlaybackCursor_PlaybackStarted(object sender, EventArgs e)
        {
            CheckSimEnvironment();
            EventSim.OnSimulationStart(Graph.EventBoxesToSimulate);
        }

        private void PlaybackCursor_PlaybackLooped(object sender, EventArgs e)
        {
            CheckSimEnvironment();
            EventSim.OnSimulationStart(Graph.EventBoxesToSimulate);
        }

        Model lastRightWeaponModelTAEWasReadFrom = null;
        TAE lastRightWeaponTAE = null;

        Model lastLeftWeaponModelTAEWasReadFrom = null;
        TAE lastLeftWeaponTAE = null;



        private void CheckChrAsmWeapons()
        {
            if (CurrentModel.ChrAsm != null)
            {
                if (CurrentModel.ChrAsm.RightWeaponModel != null)
                {
                    if (CurrentModel.ChrAsm.RightWeaponModel.AnimContainer.TimeActFiles.Count > 0)
                    {
                        if (CurrentModel.ChrAsm.RightWeaponModel != lastRightWeaponModelTAEWasReadFrom)
                        {
                            lastRightWeaponTAE = TAE.Read(CurrentModel.ChrAsm.RightWeaponModel.AnimContainer.TimeActFiles.First().Value);
                            lastRightWeaponModelTAEWasReadFrom = CurrentModel.ChrAsm.RightWeaponModel;
                        }
                    }
                    else
                    {
                        lastRightWeaponModelTAEWasReadFrom = null;
                        lastRightWeaponTAE = null;
                    }
                }

                if (CurrentModel.ChrAsm.LeftWeaponModel != null)
                {
                    if (CurrentModel.ChrAsm.LeftWeaponModel.AnimContainer.TimeActFiles.Count > 0)
                    {
                        if (CurrentModel.ChrAsm.LeftWeaponModel != lastLeftWeaponModelTAEWasReadFrom)
                        {
                            lastLeftWeaponTAE = TAE.Read(CurrentModel.ChrAsm.LeftWeaponModel.AnimContainer.TimeActFiles.First().Value);
                            lastLeftWeaponModelTAEWasReadFrom = CurrentModel.ChrAsm.LeftWeaponModel;
                        }
                    }
                    else
                    {
                        lastLeftWeaponModelTAEWasReadFrom = null;
                        lastLeftWeaponTAE = null;
                    }
                }
                    

               
            }
            else
            {
                lastRightWeaponModelTAEWasReadFrom = null;
                lastLeftWeaponModelTAEWasReadFrom = null;
                lastRightWeaponTAE = null;
                lastLeftWeaponTAE = null;
            }
        }

        //private void FindWeaponAnim(TaeAnimRefChainSolver solver, Model weaponModel, TAE weaponTae)
        //{
        //    if (weaponModel != null && weaponModel.AnimContainer.Animations.Count > 0)
        //    {
        //        // If weapon has TAE
        //        if (weaponTae != null)
        //        {
        //            var compositeAnimID = solver.GetCompositeAnimIDOfAnimInTAE(Graph.MainScreen.SelectedTae, Graph.MainScreen.SelectedTaeAnim);
        //            var matchingAnim = weaponTae.Animations.Where(a => a.ID == compositeAnimID).FirstOrDefault();
        //            if (matchingAnim != null)
        //            {


        //                int animID = (int)matchingAnim.ID;

        //                bool AnimExists(int id)
        //                {
        //                    return (weaponModel.AnimContainer.Animations.ContainsKey(solver.HKXNameFromCompositeID(id)));
        //                }

        //                if (matchingAnim.Unknown1 == 256)
        //                {
        //                    if (matchingAnim.Unknown2 > 0 && AnimExists(matchingAnim.Unknown2))
        //                    {
        //                        animID = matchingAnim.Unknown2;
        //                    }
        //                }
        //                else if (matchingAnim.Unknown1 > 256 && AnimExists(matchingAnim.Unknown1))
        //                {
        //                    animID = matchingAnim.Unknown1;
        //                }

        //                var weaponHkxName = solver.HKXNameFromCompositeID(animID);
        //                weaponModel.AnimContainer.CurrentAnimationName = weaponHkxName;
        //            }
        //            else
        //            {
        //                weaponModel.AnimContainer.CurrentAnimationName = null;
        //            }
        //        }
        //        else
        //        {
        //            // If weapon has no TAE, it's usually just the player's TAE anim entry ID as an anim name.
        //            var simpleAnimID = solver.GetHKXNameIgnoreReferences(Graph.MainScreen.SelectedTae, Graph.MainScreen.SelectedTaeAnim);
        //            weaponModel.AnimContainer.CurrentAnimationName = simpleAnimID;
        //        }
        //    }
        //}

        public void ResetRootMotion()
        {
            if (CurrentModel != null)
            {
                CurrentModel.AnimContainer.ResetRootMotion();
            }
        }

        public void RemoveTransition()
        {
            if (CurrentModel?.AnimContainer != null && CurrentModel.AnimContainer.AnimationLayers.Count == 2)
            {
                var curAnim = CurrentModel.AnimContainer.AnimationLayers[1];
                CurrentModel.AnimContainer.AnimationLayers.Clear();
                curAnim.Weight = 1;
                CurrentModel.AnimContainer.AnimationLayers.Add(curAnim);
            }
        }

        public float GetAnimWeight(string animName)
        {
            if ((CurrentModel?.AnimContainer?.AnimationLayers.Count ?? -1) < 2)
            {
                return -1;
            }
            var check = CurrentModel?.AnimContainer?.GetAnimLayerIndexByName(animName) ?? -1;
            if (check >= 0)
            {
                return CurrentModel.AnimContainer.AnimationLayers[check].Weight;
            }
            else return -1;
        }

        public string GetCurrentTransition()
        {
            if (CurrentModel?.AnimContainer != null && CurrentModel.AnimContainer.AnimationLayers.Count == 2)
            {
                return CurrentModel.AnimContainer.AnimationLayers[0].Name;
            }

            return null;
        }

        public void RootMotionSendHome()
        {
            if (CurrentModel != null)
            {
                //CurrentModel.AnimContainer.ResetRootMotion();
                CurrentModel.CurrentRootMotionTranslation = Matrix.Identity;
                //CurrentModel.AnimContainer.CurrentRootMotionDirection = 0;
            }
        }

        public string GetFinalAnimFileName(TAE tae, TAE.Animation anim)
        {
            var mainChrSolver = new TaeAnimRefChainSolver(Graph.MainScreen.FileContainer.AllTAEDict, CurrentModel.AnimContainer.Animations);
            return mainChrSolver.GetHKXName(tae, anim);
        }

        public void OnNewAnimSelected()
        {
            if (CurrentModel != null)
            {
                var mainChrSolver = new TaeAnimRefChainSolver(Graph.MainScreen.FileContainer.AllTAEDict, CurrentModel.AnimContainer.Animations);
                var mainChrAnimName = mainChrSolver.GetHKXName(Graph.MainScreen.SelectedTae, Graph.MainScreen.SelectedTaeAnim);

                //V2.0: See if this needs something similar in the new system.
                //CurrentModel.AnimContainer.StoreRootMotionRotation();

                CurrentModel.AnimContainer.CurrentAnimationName = mainChrAnimName;

                Graph.PlaybackCursor.ResetAll();

                Graph.PlaybackCursor.HkxAnimationLength = CurrentModel.AnimContainer.CurrentAnimDuration;
                Graph.PlaybackCursor.SnapInterval = CurrentModel?.AnimContainer?.CurrentAnimFrameDuration;
                Graph.PlaybackCursor.CurrentTime = 0;
                Graph.PlaybackCursor.StartTime = 0;
                

                CheckChrAsmWeapons();

                //FindWeaponAnim(mainChrSolver, lastRightWeaponModelTAEWasReadFrom, lastRightWeaponTAE);
                //FindWeaponAnim(mainChrSolver, lastLeftWeaponModelTAEWasReadFrom, lastLeftWeaponTAE);

                CheckSimEnvironment();

                EventSim.OnNewAnimSelected(Graph.EventBoxes);

                if (CurrentModel.AnimContainer.CurrentAnimation != null)
                {
                    //V2.0: Check
                    CurrentModel.AnimContainer.CurrentAnimation.Reset();
                    CurrentModel.AnimContainer.ScrubRelative(0);
                }
                else
                {
                    CurrentModel.Skeleton.RevertToReferencePose();
                }
            }
            
        }

        public bool IsAnimLoaded(string name)
        {
            return CurrentModel?.AnimContainer?.IsAnimLoaded(name) ?? false;
        }

        float modelDirectionLastFrame = 0;
        public void GeneralUpdate()
        {
            DBG.DbgPrimXRay = Graph.MainScreen.Config.DbgPrimXRay;

            
            if (CurrentModel != null)
            {
                GFX.World.ModelDirection_ForOrbitCam = CurrentModel.CurrentDirection;

                if (Graph.MainScreen.Config.CameraFollowsRootMotion)
                {
                    //GFX.World.WorldMatrixMOD = //Matrix.CreateFromQuaternion(Quaternion.CreateFromRotationMatrix(CurrentModel.CurrentRootMotionTransform.WorldMatrix)) * 
                    //    Matrix.CreateTranslation(
                    //    -Vector3.Transform(Vector3.Zero,
                    //    CurrentModel.CurrentRootMotionTranslation));

                    GFX.World.WorldMatrixMOD = //Matrix.CreateRotationY(CurrentModel.CurrentDirection) *
                        Matrix.CreateTranslation(
                        -Vector3.Transform(Vector3.Zero,
                        CurrentModel.CurrentRootMotionTranslation));
                }
                else
                {
                    GFX.World.WorldMatrixMOD = Matrix.Identity;
                }

                if (Graph.MainScreen.Config.CameraFollowsRootMotionRotation)
                {
                    float turnAmount = CurrentModel.CurrentDirection - modelDirectionLastFrame;

                    //GFX.World.CameraTransform.EulerRotationExtraY += turnAmount;

                    GFX.World.RotateFromRootMotion(-turnAmount);

                    modelDirectionLastFrame = CurrentModel.CurrentDirection;
                }
                //else
                //{
                //    if (GFX.World.CameraTransform.EulerRotationExtraY != 0)
                //    {
                //        GFX.World.RotateCameraOrbit(GFX.World.CameraTransform.EulerRotationExtraY, 0, 1);
                //        GFX.World.CameraTransform.EulerRotationExtraY = 0;
                //    }
                //}

                if (CurrentModel.AnimContainer != null)
                {
                    CurrentModel.AnimContainer.EnableRootMotion = Graph.MainScreen.Config.EnableAnimRootMotion;
                    CurrentModel.AnimContainer.EnableRootMotionWrap = Graph.MainScreen.Config.WrapRootMotion;
                    //V2.0
                    //if (CurrentModel.AnimContainer.CurrentAnimation != null && CurrentModel.AnimContainer.CurrentAnimation.data.RootMotion != null)
                    //    CurrentModel.AnimContainer.CurrentAnimation.RootMotion.Accumulate = Graph.MainScreen.Config.AccumulateRootMotion;
                }
            }
            else
            {
                GFX.World.WorldMatrixMOD = Matrix.Identity;
            }
            
        }

        public void DrawDebug()
        {
            var printer = new StatusPrinter(Vector2.One * 4, Color.Yellow);

            //if (FmodManager.LoadedFEVs.Count > 0)
            //{
            //    printer.AppendLine($"FEVs Loaded:", Color.Lime);
            //    foreach (var fevName in FmodManager.LoadedFEVs)
            //    {
            //        printer.AppendLine($"    {fevName}", Color.Lime);
            //    }
            //}

            //printer.AppendLine("[DbgMenuPadRepeater Debug Info]");
            //foreach (var repeater in DbgMenuPadRepeater.ALL_INSTANCES)
            //{
            //    printer.AppendLine(repeater.ToString());
            //}
            //printer.AppendLine(" ");

            if (CurrentModel != null && CurrentModel.AnimContainer != null)
            {
                if (CurrentModel.Skeleton.BoneLimitExceeded)
                {
                    printer.AppendLine($"Warning: Model exceeds max bone count.", Color.Orange);
                }

                if (CurrentModel.AnimContainer.CurrentAnimation != null)
                {
                    printer.AppendLine($"Animation: {CurrentModel.AnimContainer.CurrentAnimation}");
                }
                else if (CurrentModel.AnimContainer.CurrentAnimationName != null)
                {
                    printer.AppendLine($"Animation: {(CurrentModel.AnimContainer.CurrentAnimationName)} (Invalid)", Color.Red);
                }

                if (CurrentComboIndex >= 0)
                {
                    printer.AppendLine($"Playing Combo ({(CurrentComboLoop ? "Looping" : "Once")}):", Color.Cyan);
                    for (int c = 0; c < CurrentCombo.Length; c++)
                    {
                        printer.AppendLine($"    {(CurrentComboIndex == c ? "■" : "□")} {CurrentCombo[c]}", Color.Cyan);
                    }
                }

                if (CurrentModel.AnimContainer.AnimationLayers.Count > 0)
                {
                    //printer.AppendLine($"Active Anims:");

                    //for (int i = 0; i < CurrentModel.AnimContainer.AnimationLayers.Count; i++)
                    //{
                    //    var layer = CurrentModel.AnimContainer.AnimationLayers[i];
                    //    printer.AppendLine($"[{i:D2}] [{layer.Weight:0.000}x] {layer.Name} [{layer.CurrentTime:0.000}/{layer.Duration:0.000}]");
                    //}
                    var tr = GetCurrentTransition();
                    if (tr != null)
                    {
                        printer.AppendLine($"<Showing Transition from {tr}>");
                    }
                }
            }

            if (EntityType == TaeEntityType.PC)
            {
                printer.AppendLine();
                
                

                if (CurrentModel?.ChrAsm?.RightWeapon != null)
                {
                    printer.AppendLine($"R Weapon Animation: {(CurrentModel?.ChrAsm?.RightWeaponModel?.AnimContainer?.CurrentAnimation?.ToString() ?? "NONE")}");

                    var atk = CurrentModel.ChrAsm.RightWeapon.WepMotionCategory;
                    var spAtk = CurrentModel.ChrAsm.RightWeapon.SpAtkCategory;

                    printer.AppendLine($"R Weapon Moveset(s): a{atk:D2}{(spAtk > 0 ? $", a{spAtk:D2}" : "")}");
                }

                printer.AppendLine();
                

                if (CurrentModel?.ChrAsm?.LeftWeapon != null)
                {
                    printer.AppendLine($"L Weapon Animation: {(CurrentModel?.ChrAsm?.LeftWeaponModel?.AnimContainer?.CurrentAnimation?.ToString() ?? "NONE")}");

                    var atk = CurrentModel.ChrAsm.LeftWeapon.WepMotionCategory;
                    var spAtk = CurrentModel.ChrAsm.LeftWeapon.SpAtkCategory;

                    printer.AppendLine($"L Weapon Moveset(s): a{atk:D2}{(spAtk > 0 ? $", a{spAtk:D2}" : "")}");
                }
            }

            //printer.AppendLine($"RWPN ANIM LIST:");
            //var anims = CurrentModel?.ChrAsm?.RightWeaponModel?.AnimContainer?.Animations;
            //if (anims != null)
            //{
            //    foreach (var a in anims.Keys)
            //    {
            //        printer.AppendLine("  " + a);
            //    }
            //}
            //printer.AppendLine($"LWPN ANIM LIST:");
            //var animsL = CurrentModel?.ChrAsm?.LeftWeaponModel?.AnimContainer?.Animations;
            //if (animsL != null)
            //{
            //    foreach (var a in animsL.Keys)
            //    {
            //        printer.AppendLine("  " + a);
            //    }
            //}

        

            if (EventSim != null &&
                Graph.MainScreen.Config.EventSimulationsEnabled["EventSimSpEffects"] && 
                EventSim.SimulatedActiveSpEffects.Count > 0)
            {
                printer.AppendLine("[Active SpEffects:]");

                foreach (var spe in EventSim.SimulatedActiveSpEffects)
                {
                    printer.AppendLine("    " + spe);
                }
            }

            GFX.SpriteBatchBeginForText();
            printer.Draw();
            GFX.SpriteBatchEnd();
        }
    }
}
