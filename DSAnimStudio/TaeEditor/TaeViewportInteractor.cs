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

        public bool IsBlendingActive => 
            (CurrentComboIndex >= 0 && EventSim.GetSimEnabled("EventSimBasicBlending_Combos")) ||
            (CurrentComboIndex < 0 && EventSim.GetSimEnabled("EventSimBasicBlending"));

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
                        CurrentModel.NpcParam.ApplyToNpcModel(CurrentModel);
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
                                CurrentModel.NpcParam.ApplyToNpcModel(CurrentModel);
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
                                    npc.ApplyToNpcModel(CurrentModel);
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

                //GFX.World.ModelCenter_ForOrbitCam = CurrentModel.MainMesh.Bounds.GetCenter();
                GFX.World.ModelHeight_ForOrbitCam = CurrentModel.MainMesh.Bounds.Max.Y - CurrentModel.MainMesh.Bounds.Min.Y;
                GFX.World.ModelDepth_ForOrbitCam = (CurrentModel.MainMesh.Bounds.Max.Z - CurrentModel.MainMesh.Bounds.Min.Z) * 1.5f;
                GFX.World.OrbitCamReset(isFirstTime: true);

                FmodManager.Purge();
                FmodManager.LoadMainFEVs();
                FmodManager.LoadInterrootFEV(CurrentModel.Name);
            }
            else if (shortFileName.StartsWith("o"))
            {
                SetEntityType(TaeEntityType.OBJ);

                GameDataManager.LoadObject(shortFileName);

                //GFX.World.ModelCenter_ForOrbitCam = CurrentModel.MainMesh.Bounds.GetCenter();
                GFX.World.ModelHeight_ForOrbitCam = CurrentModel.MainMesh.Bounds.Max.Y - CurrentModel.MainMesh.Bounds.Min.Y;
                GFX.World.ModelDepth_ForOrbitCam = (CurrentModel.MainMesh.Bounds.Max.Z - CurrentModel.MainMesh.Bounds.Min.Z) * 1.5f;
                GFX.World.OrbitCamReset(isFirstTime: true);

                FmodManager.Purge();
                FmodManager.LoadMainFEVs();

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

            CurrentModel.OnRootMotionWrap = (wrap) =>
            {
                EventSim?.RootMotionWrapForBlades(wrap);
            };

            Scene.EnableModelDrawing();
            if (!CurrentModel.IS_PLAYER)
                Scene.EnableModelDrawing2();
        }

        private void ChrAsm_EquipmentModelsUpdated(object sender, EventArgs e)
        {
            Graph.MainScreen.GameWindowAsForm.Invoke(new Action(() =>
            {
                void DoWPNAnims(string menuChoiceName, Model wpnMdl)
                {
                    Graph.MainScreen.MenuBar.CompletelyDestroyItem("Player Settings", menuChoiceName);

                    if (wpnMdl == null)
                        return;

                    var anims = wpnMdl.AnimContainer?.Animations;
                    if (anims != null && anims.Count > 0)
                    {
                        var choicesDict = new Dictionary<string, Action>();
                        foreach (var a in anims.Keys)
                        {
                            choicesDict.Add(a, () => wpnMdl.AnimContainer.CurrentAnimationName = a);
                        }

                        Graph.MainScreen.MenuBar.AddItem(
                               "Player Settings",
                               menuChoiceName,
                               choicesDict,
                               () => wpnMdl.AnimContainer.CurrentAnimationName);
                    }
                    else
                    {
                        Graph.MainScreen.MenuBar.AddItem($"Player Settings\\{menuChoiceName}", "Weapon model is not animated.");
                    }
                }

                DoWPNAnims("Right WPN Model 0 Anim", CurrentModel?.ChrAsm?.RightWeaponModel0);
                DoWPNAnims("Right WPN Model 1 Anim", CurrentModel?.ChrAsm?.RightWeaponModel1);
                DoWPNAnims("Right WPN Model 2 Anim", CurrentModel?.ChrAsm?.RightWeaponModel2);
                DoWPNAnims("Right WPN Model 3 Anim", CurrentModel?.ChrAsm?.RightWeaponModel3);

                DoWPNAnims("Left WPN Model 0 Anim", CurrentModel?.ChrAsm?.LeftWeaponModel0);
                DoWPNAnims("Left WPN Model 1 Anim", CurrentModel?.ChrAsm?.LeftWeaponModel1);
                DoWPNAnims("Left WPN Model 2 Anim", CurrentModel?.ChrAsm?.LeftWeaponModel2);
                DoWPNAnims("Left WPN Model 3 Anim", CurrentModel?.ChrAsm?.LeftWeaponModel3);

                Graph.MainScreen.SelectNewAnimRef(Graph.MainScreen.SelectedTae, Graph.MainScreen.SelectedTaeAnim);
            }));

            //V2.0: Scrub weapon anims to the current frame.

            CurrentModel.ChrAsm.RightWeaponModel0?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm.RightWeaponModel1?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm.RightWeaponModel2?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm.RightWeaponModel3?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm.LeftWeaponModel0?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm.LeftWeaponModel1?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm.LeftWeaponModel2?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm.LeftWeaponModel3?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);

            //V2.0: Update stuff probably
            CurrentModel.AfterAnimUpdate(0);
        }

        //private void EquipForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        public void CloseEquipForm()
        {
            Graph.MainScreen.GameWindowAsForm.Invoke(new Action(() =>
            {
                EquipForm?.Close();
                Graph.MainScreen.MenuBar["Player Settings\\Show Equip Change Menu"].Checked = false;
            }));
        }

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

            UpdateCombo();

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
            // Do this before setting current combo since inside here, it will check if there's a combo and not reset stuff if there's currently a combo happening.
            CurrentComboIndex = -1;
            EventSim.OnNewAnimSelected(Graph.EventBoxes);

            CurrentComboLoop = isLoop;
            CurrentCombo = entries;
            CurrentComboIndex = 0;
            if (CurrentModel.ChrAsm != null)
            {
                CurrentModel.ChrAsm.WeaponStyle = CurrentModel.ChrAsm.StartWeaponStyle;
            }

            StartCurrentComboEntry(true);
            RemoveTransition();
        }

        private void StartCurrentComboEntry(bool isFirstTime)
        {
            if (CurrentCombo[CurrentComboIndex].ComboType == TaeComboMenu.TaeComboAnimType.PlayerRH)
            {
                EventSim.OverrideHitViewDummyPolySource = ParamData.AtkParam.DummyPolySource.RightWeapon0;
            }
            else if (CurrentCombo[CurrentComboIndex].ComboType == TaeComboMenu.TaeComboAnimType.PlayerLH)
            {
                EventSim.OverrideHitViewDummyPolySource = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
            }
            else
            {
                EventSim.OverrideHitViewDummyPolySource = ParamData.AtkParam.DummyPolySource.Body;
            }

            if (CurrentCombo.Length > 1 || CurrentCombo[CurrentComboIndex].StartFrame < 0 || isFirstTime)
            {
                if (!Graph.MainScreen.GotoAnimID(CurrentCombo[CurrentComboIndex].AnimID, false))
                {
                    CurrentComboIndex = -1;
                    return;
                }
                else
                {
                    // lol idk why 
                    Graph.PlaybackCursor.RestartFromBeginning();

                    if (CurrentCombo[CurrentComboIndex].StartFrame > 0)
                    {
                        Graph.PlaybackCursor.GotoFrame(CurrentCombo[CurrentComboIndex].StartFrame);
                    }
                }
            }
            else
            {
                if (CurrentCombo[CurrentComboIndex].StartFrame >= 0)
                {
                    Graph.MainScreen.HardReset();
                    Graph.PlaybackCursor.GotoFrame(CurrentCombo[CurrentComboIndex].StartFrame);
                    Graph.PlaybackCursor.UpdateScrubbing();
                }
            }

            

            if (!Graph.PlaybackCursor.IsPlaying)
            {
                Graph.PlaybackCursor.Transport_PlayPause();
            }
        }

        public TaeComboEntry[] CurrentCombo = new TaeComboEntry[0];
        public int CurrentComboIndex = -1;
        public bool CurrentComboLoop = false;

        public void CancelCombo()
        {
            CurrentComboIndex = -1;
            EventSim.OverrideHitViewDummyPolySource = null;
        }

        private void GoToNextItemInCombo()
        {
            CurrentComboIndex++;
            if (CurrentComboIndex < CurrentCombo.Length)
            {
                StartCurrentComboEntry(false);
            }
            else
            {
                if (CurrentComboLoop)
                {
                    CurrentComboIndex = 0;
                    StartCurrentComboEntry(false);
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

                    if (Graph.PlaybackCursor.MaxTime > 0 && (Graph.PlaybackCursor.CurrentTime >= (Graph.PlaybackCursor.MaxTime - 0.00005f)) ||
                        (CurrentCombo[CurrentComboIndex].EndFrame >= 0 && Graph.PlaybackCursor.CurrentFrame >= CurrentCombo[CurrentComboIndex].EndFrame))
                    {
                        if (CurrentCombo[CurrentComboIndex].EndFrame >= 0)
                        {
                            Graph.PlaybackCursor.CurrentTime = (CurrentCombo[CurrentComboIndex].EndFrame * Graph.PlaybackCursor.CurrentSnapInterval);
                        }
                        else
                        {
                            Graph.PlaybackCursor.CurrentTime = Graph.PlaybackCursor.MaxTime - 0.00005f;
                        }
                        
                        //Graph.PlaybackCursor.UpdateScrubbing();

                        GoToNextItemInCombo();
                        return;
                    }
                    else if (CurrentComboIndex < CurrentCombo.Length - 1 || CurrentComboLoop)
                    {
                        foreach (var eventBox in Graph.EventBoxesToSimulate)
                        {
                            if (eventBox.PlaybackHighlight && eventBox.MyEvent.Type == 0 && eventBox.MyEvent.Template != null)
                            {
                                //var cancelTypeAsStr = eventBox.MyEvent.Template["JumpTableID"].ValueToString(eventBox.MyEvent.Parameters["JumpTableID"]);
                                //if (cancelTypeAsStr == CurrentCombo[CurrentComboIndex].Event0CancelType)
                                //{
                                //    GoToNextItemInCombo();
                                //    return;
                                //}
                                var cancelTypeAsInt = Convert.ToInt32(eventBox.MyEvent.Parameters["JumpTableID"]);
                                if (cancelTypeAsInt == CurrentCombo[CurrentComboIndex].Event0CancelType)
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

        //Model lastRightWeaponModelTAEWasReadFrom = null;
        //TAE lastRightWeaponTAE = null;

        //Model lastLeftWeaponModelTAEWasReadFrom = null;
        //TAE lastLeftWeaponTAE = null;



        private void CheckChrAsmWeapons()
        {
            //if (CurrentModel.ChrAsm != null)
            //{
            //    if (CurrentModel.ChrAsm.RightWeaponModel != null)
            //    {
            //        if (CurrentModel.ChrAsm.RightWeaponModel.AnimContainer.TimeActFiles.Count > 0)
            //        {
            //            if (CurrentModel.ChrAsm.RightWeaponModel != lastRightWeaponModelTAEWasReadFrom)
            //            {
            //                lastRightWeaponTAE = TAE.Read(CurrentModel.ChrAsm.RightWeaponModel.AnimContainer.TimeActFiles.First().Value);
            //                lastRightWeaponModelTAEWasReadFrom = CurrentModel.ChrAsm.RightWeaponModel;
            //            }
            //        }
            //        else
            //        {
            //            lastRightWeaponModelTAEWasReadFrom = null;
            //            lastRightWeaponTAE = null;
            //        }
            //    }

            //    if (CurrentModel.ChrAsm.LeftWeaponModel != null)
            //    {
            //        if (CurrentModel.ChrAsm.LeftWeaponModel.AnimContainer.TimeActFiles.Count > 0)
            //        {
            //            if (CurrentModel.ChrAsm.LeftWeaponModel != lastLeftWeaponModelTAEWasReadFrom)
            //            {
            //                lastLeftWeaponTAE = TAE.Read(CurrentModel.ChrAsm.LeftWeaponModel.AnimContainer.TimeActFiles.First().Value);
            //                lastLeftWeaponModelTAEWasReadFrom = CurrentModel.ChrAsm.LeftWeaponModel;
            //            }
            //        }
            //        else
            //        {
            //            lastLeftWeaponModelTAEWasReadFrom = null;
            //            lastLeftWeaponTAE = null;
            //        }
            //    }
                    

               
            //}
            //else
            //{
            //    lastRightWeaponModelTAEWasReadFrom = null;
            //    lastLeftWeaponModelTAEWasReadFrom = null;
            //    lastRightWeaponTAE = null;
            //    lastLeftWeaponTAE = null;
            //}
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

                CurrentModel.ChrAsm?.RightWeaponModel0?.ResetRootMotionTranslation();
                CurrentModel.ChrAsm?.RightWeaponModel1?.ResetRootMotionTranslation();
                CurrentModel.ChrAsm?.RightWeaponModel2?.ResetRootMotionTranslation();
                CurrentModel.ChrAsm?.RightWeaponModel3?.ResetRootMotionTranslation();

                CurrentModel.ChrAsm?.LeftWeaponModel0?.ResetRootMotionTranslation();
                CurrentModel.ChrAsm?.LeftWeaponModel1?.ResetRootMotionTranslation();
                CurrentModel.ChrAsm?.LeftWeaponModel2?.ResetRootMotionTranslation();
                CurrentModel.ChrAsm?.LeftWeaponModel3?.ResetRootMotionTranslation();
            }
        }

        public string GetFinalAnimFileName(TAE tae, TAE.Animation anim)
        {
            if (CurrentModel == null || CurrentModel?.AnimContainer == null || Graph == null || Graph?.MainScreen?.FileContainer?.AllTAEDict == null)
                return null;

            if (Graph.MainScreen.FileContainer.IsCurrentlyLoading)
                return null;

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

                //CurrentModel.AnimContainer.CurrentAnimation.RotMatrixAtStartOfAnim

                CurrentModel.AnimContainer.CurrentAnimationName = mainChrAnimName;

                lock (CurrentModel.AnimContainer._lock_AnimationLayers)
                {
                    if ((CurrentModel.AnimContainer.AnimationLayers.Count == 1 && CurrentModel.AnimContainer.AnimationLayers[0].IsAdditiveBlend) ||
                      (CurrentModel.AnimContainer.AnimationLayers.Count == 2 && (CurrentModel.AnimContainer.AnimationLayers[0].IsAdditiveBlend || CurrentModel.AnimContainer.AnimationLayers[1].IsAdditiveBlend)))
                    {
                        RemoveTransition();
                    }
                }

               

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

        private int lastFrameForTrails = -1;

        public void GeneralUpdate()
        {
            DBG.DbgPrimXRay = Graph.MainScreen.Config.DbgPrimXRay;

            
            if (CurrentModel != null)
            {
                GFX.World.ModelDirection_ForOrbitCam = Graph.MainScreen.Config.CameraFollowsRootMotionRotation ? CurrentModel.CurrentDirection : 0;

                GFX.World.WorldMatrixMOD = Matrix.Identity;

               

                if (Graph.MainScreen.Config.CameraFollowsRootMotion)
                {
                    //GFX.World.WorldMatrixMOD = //Matrix.CreateFromQuaternion(Quaternion.CreateFromRotationMatrix(CurrentModel.CurrentRootMotionTransform.WorldMatrix)) * 
                    //    Matrix.CreateTranslation(
                    //    -Vector3.Transform(Vector3.Zero,
                    //    CurrentModel.CurrentRootMotionTranslation));

                    GFX.World.WorldMatrixMOD *= //Matrix.CreateRotationY(CurrentModel.CurrentDirection) *
                        Matrix.CreateTranslation(
                        -Vector3.Transform(Vector3.Zero,
                        CurrentModel.CurrentRootMotionTranslation));
                }
                //else
                //{
                //    GFX.World.WorldMatrixMOD = Matrix.Identity;
                //}

                if (Graph.MainScreen.Config.CameraFollowsRootMotionRotation)
                {
                    GFX.World.WorldMatrixMOD *= Matrix.CreateRotationY(-CurrentModel.CurrentDirection);
                }

                if (Graph.MainScreen.Config.CameraPansVerticallyToFollowModel)
                {
                    GFX.World.UpdateDummyPolyFollowRefPoint(isFirstTime: false);
                }

                //if (Graph.MainScreen.Config.CameraFollowsRootMotionRotation)
                //{
                //    float turnAmount = CurrentModel.CurrentDirection - modelDirectionLastFrame;

                //    //GFX.World.CameraTransform.EulerRotationExtraY += turnAmount;

                //    GFX.World.ModelCenter_ForOrbitCam = Vector3.Transform(
                //        CurrentModel.MainMesh.Bounds.GetCenter(), CurrentModel.CurrentTransform.WorldMatrix);
                //    GFX.World.RotateFromRootMotion(-turnAmount);

                //    modelDirectionLastFrame = CurrentModel.CurrentDirection;
                //}
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

                int frame = (int)Math.Round(Graph.PlaybackCursor.CurrentTime / (1.0 / 60.0));

                if (Graph.PlaybackCursor.ContinuousTimeDelta != 0)
                {
                    //EventSim?.UpdateAllBladeSFXsLive();

                    if (frame != lastFrameForTrails)
                        EventSim?.UpdateAllBladeSFXsLowHz();


                    EventSim?.UpdateAllBladeSFXsLive();

                }

                lastFrameForTrails = frame;
            }
            else
            {
                GFX.World.WorldMatrixMOD = Matrix.Identity;
            }
            
        }

        public void DrawDebug()
        {
            if (CurrentModel != null)
            {
                EventSim?.DrawAllBladeSFXs(CurrentModel.CurrentTransform.WorldMatrix);
            }
        }

        public void DrawDebugOverlay()
        {
            

            var printer = new StatusPrinter(Vector2.One * 4, Main.Colors.GuiColorViewportStatus);

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
                    printer.AppendLine($"Warning: Model exceeds max bone count.", Main.Colors.GuiColorViewportStatusMaxBoneCountExceeded);
                }

                if (CurrentModel.AnimContainer.CurrentAnimation != null)
                {
                    printer.AppendLine($"Animation: {CurrentModel.AnimContainer.CurrentAnimation}");
                }
                else if (CurrentModel.AnimContainer.CurrentAnimationName != null)
                {
                    printer.AppendLine($"Animation: {(CurrentModel.AnimContainer.CurrentAnimationName)} (Invalid)", Main.Colors.GuiColorViewportStatusAnimDoesntExist);
                }

                if (CurrentComboIndex >= 0)
                {
                    printer.AppendLine($"Playing Combo ({(CurrentComboLoop ? "Looping" : "Once")}):", Main.Colors.GuiColorViewportStatusCombo);
                    for (int c = 0; c < CurrentCombo.Length; c++)
                    {
                        printer.AppendLine($"    {(CurrentComboIndex == c ? "■" : "□")} {CurrentCombo[c]}", Main.Colors.GuiColorViewportStatusCombo);
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

                void DoWpnAnim(string wpnKind, Model wpnMdl)
                {
                    if (wpnMdl?.AnimContainer?.CurrentAnimation != null)
                        printer.AppendLine($"{wpnKind} Animation: {(wpnMdl?.AnimContainer?.CurrentAnimation?.ToString() ?? "NONE")}");
                }

                if (CurrentModel?.ChrAsm?.RightWeapon != null)
                {
                    printer.AppendLine("[Right Weapon]");

                    var atk = CurrentModel.ChrAsm.RightWeapon.WepMotionCategory;
                    var spAtk = CurrentModel.ChrAsm.RightWeapon.SpAtkCategory;

                    printer.AppendLine($"    Part:            WP_A_{CurrentModel.ChrAsm.RightWeapon.EquipModelID:D4}");
                    printer.AppendLine($"    Moveset(s):      a{atk:D2}{(spAtk > 0 ? $", a{spAtk:D2}" : "")}");

                    DoWpnAnim("    MDL 0", CurrentModel?.ChrAsm?.RightWeaponModel0);
                    DoWpnAnim("    MDL 1", CurrentModel?.ChrAsm?.RightWeaponModel1);
                    DoWpnAnim("    MDL 2", CurrentModel?.ChrAsm?.RightWeaponModel2);
                    DoWpnAnim("    MDL 3", CurrentModel?.ChrAsm?.RightWeaponModel3);
                }

                

                if (CurrentModel?.ChrAsm?.LeftWeapon != null)
                {
                    printer.AppendLine("[Left Weapon]");

                    var atk = CurrentModel.ChrAsm.LeftWeapon.WepMotionCategory;
                    var spAtk = CurrentModel.ChrAsm.LeftWeapon.SpAtkCategory;

                    printer.AppendLine($"    Part:            WP_A_{CurrentModel.ChrAsm.LeftWeapon.EquipModelID:D4}");
                    printer.AppendLine($"    Moveset(s):      a{atk:D2}{(spAtk > 0 ? $", a{spAtk:D2}" : "")}");

                    DoWpnAnim("    MDL 0", CurrentModel?.ChrAsm?.LeftWeaponModel0);
                    DoWpnAnim("    MDL 1", CurrentModel?.ChrAsm?.LeftWeaponModel1);
                    DoWpnAnim("    MDL 2", CurrentModel?.ChrAsm?.LeftWeaponModel2);
                    DoWpnAnim("    MDL 3", CurrentModel?.ChrAsm?.LeftWeaponModel3);
                }

                

                printer.AppendLine();

                

                

                

                
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
