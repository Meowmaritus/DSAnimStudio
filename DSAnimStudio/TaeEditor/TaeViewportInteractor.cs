using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        TaeEventSimulationEnvironment EventSim;

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
                        CurrentModel.DummyPolyMan.RecreateAllHitboxPrimitives(CurrentModel.NpcParam);
                    }
                }
            }
        }

        Model CurrentModel => Scene.Models.Count > 0 ? Scene.Models[0] : null;

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
            DBG.CategoryEnableDbgLabelDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = false;

            Graph = graph;
            Graph.PlaybackCursor.PlaybackStarted += PlaybackCursor_PlaybackStarted;
            Graph.PlaybackCursor.PlaybackFrameChange += PlaybackCursor_PlaybackFrameChange;
            Graph.PlaybackCursor.ScrubFrameChange += PlaybackCursor_ScrubFrameChange;
            Graph.PlaybackCursor.PlaybackEnded += PlaybackCursor_PlaybackEnded;
            Graph.PlaybackCursor.EventBoxEnter += PlaybackCursor_EventBoxEnter;
            Graph.PlaybackCursor.EventBoxMidst += PlaybackCursor_EventBoxMidst;
            Graph.PlaybackCursor.EventBoxExit += PlaybackCursor_EventBoxExit;
            Graph.PlaybackCursor.PlaybackLooped += PlaybackCursor_PlaybackLooped;

            NewAnimationContainer.AutoPlayAnimContainersUponLoading = false;

            Scene.ClearScene();
            TexturePool.Flush();

            var shortFileName = Utils.GetFileNameWithoutAnyExtensions(
                Utils.GetFileNameWithoutDirectoryOrExtension(Graph.MainScreen.FileContainerName)).ToLower();

            var fileName = Graph.MainScreen.FileContainerName.ToLower();

            if (shortFileName.StartsWith("c"))
            {
                GameDataManager.LoadCharacter(shortFileName);

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
                    if (PossibleNpcParams.Count > 0)
                        SelectedNpcParamIndex = 0;
                    else
                        SelectedNpcParamIndex = -1;

                    SetEntityType(TaeEntityType.NPC);

                    Graph.MainScreen.GameWindowAsForm.Invoke(new Action(() =>
                    {
                        EquipForm?.Dispose();
                        OnEquipFormClose();
                        EquipForm = null;

                        var menuItemLoadTextures = Graph.MainScreen.MenuBar["NPC Settings/Load Additional Texture File(s)..."];

                        Graph.MainScreen.MenuBar.ClearItem("NPC Settings");

                        Graph.MainScreen.MenuBar.AddItem("NPC Settings", menuItemLoadTextures);

                        Graph.MainScreen.MenuBar.AddSeparator("NPC Settings");

                        Graph.MainScreen.MenuBar.AddItem("NPC Settings/Draw Mask", "Show All", () =>
                        {
                            for (int i = 0; i < CurrentModel.DrawMask.Length; i++)
                            {
                                CurrentModel.DrawMask[i] = true;
                            }
                        });

                        var validNpcParams = ParamManager.FindNpcParams(CurrentModel.Name);

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

                        foreach (var npc in validNpcParams)
                        {
                            Graph.MainScreen.MenuBar.AddItem("NPC Settings/Draw Mask", 
                                $"{npc.GetDisplayName()} | {npc.GetMaskString(materialsPerMask, masksEnabledOnAllNpcParams)}", () =>
                            {
                                npc.ApplyMaskToModel(CurrentModel);
                            });
                        }

                        //foreach (var npc in validNpcParams)
                        //{
                        //    Graph.MainScreen.MenuBar.AddItem("Behavior Variation ID", $"Apply Behavior Variation ID " +
                        //        $"from NpcParam {npc.ID} {npc.Name}", () =>
                        //    {
                        //        npc.ApplyMaskToModel(CurrentModel);
                        //    });
                        //}
                    }));

                    //throw new NotImplementedException("Implement NPC param change you lazy fuck :tremblecat:");
                }

                GFX.World.ModelCenter_ForOrbitCam = CurrentModel.MainMesh.Bounds.GetCenter();
                GFX.World.ModelHeight_ForOrbitCam = CurrentModel.MainMesh.Bounds.Max.Y - CurrentModel.MainMesh.Bounds.Min.Y;
                GFX.World.OrbitCamReset();
            }
            else if (shortFileName.StartsWith("o"))
            {
                SetEntityType(TaeEntityType.OBJ);

                GameDataManager.LoadObject(shortFileName);

                GFX.World.ModelCenter_ForOrbitCam = CurrentModel.MainMesh.Bounds.GetCenter();
                GFX.World.ModelHeight_ForOrbitCam = CurrentModel.MainMesh.Bounds.Max.Y - CurrentModel.MainMesh.Bounds.Min.Y;
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
                var thingToKeep = Graph.MainScreen.MenuBar["Player Settings/Show Equip Change Menu"];
                Graph.MainScreen.MenuBar.ClearItem("Player Settings");
                Graph.MainScreen.MenuBar.AddItem("Player Settings", thingToKeep);
                Graph.MainScreen.MenuBar.AddSeparator("Player Settings");

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
                    Graph.MainScreen.MenuBar.AddItem("Player Settings/Select Right Weapon Anim", "Weapon is not animated.");
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
                    Graph.MainScreen.MenuBar.AddItem("Player Settings/Select Left Weapon Anim", "Weapon is not animated.");
                }
            }));
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

            if (Graph.MainScreen.MenuBar["Player Settings/Show Equip Change Menu"].Checked)
            {
                Graph.MainScreen.MenuBar["Player Settings/Show Equip Change Menu"].Checked = false;

                EquipForm.Close();
            }
            else
            {
                Graph.MainScreen.MenuBar["Player Settings/Show Equip Change Menu"].Checked = true;

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
                Graph.MainScreen.MenuBar["Player Settings/Show Equip Change Menu"].Checked = false;
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

            CurrentModel.AnimContainer.IsPlaying = false;
            CurrentModel.AnimContainer.ScrubCurrentAnimation((float)Graph.PlaybackCursor.GUICurrentTime);

            CheckSimEnvironment();
            EventSim.OnSimulationFrameChange(Graph.EventBoxes);
        }

        private void CheckSimEnvironment()
        {
            if (EventSim == null || EventSim.MODEL != CurrentModel)
            {
                EventSim = new TaeEventSimulationEnvironment(CurrentModel);
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
            EventSim.OnSimulationEnd(Graph.EventBoxes);
        }

        private void PlaybackCursor_ScrubFrameChange(object sender, EventArgs e)
        {
            Graph.PlaybackCursor.HkxAnimationLength = CurrentModel?.AnimContainer?.CurrentAnimDuration;
            CurrentModel.AnimContainer.IsPlaying = false;
            CurrentModel.AnimContainer.ScrubCurrentAnimation((float)Graph.PlaybackCursor.GUICurrentTime);
            CheckSimEnvironment();
            EventSim.OnSimulationFrameChange(Graph.EventBoxes);
        }

        private void PlaybackCursor_PlaybackStarted(object sender, EventArgs e)
        {
            CheckSimEnvironment();
            EventSim.OnSimulationStart(Graph.EventBoxes);
        }

        private void PlaybackCursor_PlaybackLooped(object sender, EventArgs e)
        {
            CheckSimEnvironment();
            EventSim.OnSimulationStart(Graph.EventBoxes);
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

        public void OnNewAnimSelected()
        {
            if (CurrentModel != null)
            {
                var mainChrSolver = new TaeAnimRefChainSolver(Graph.MainScreen.FileContainer.AllTAEDict, CurrentModel.AnimContainer.Animations);
                var mainChrAnimName = mainChrSolver.GetHKXName(Graph.MainScreen.SelectedTae, Graph.MainScreen.SelectedTaeAnim);

                CurrentModel.AnimContainer.CurrentAnimationName = mainChrAnimName;

                Graph.PlaybackCursor.HkxAnimationLength = CurrentModel.AnimContainer.CurrentAnimDuration;

                CheckChrAsmWeapons();

                //FindWeaponAnim(mainChrSolver, lastRightWeaponModelTAEWasReadFrom, lastRightWeaponTAE);
                //FindWeaponAnim(mainChrSolver, lastLeftWeaponModelTAEWasReadFrom, lastLeftWeaponTAE);

                CheckSimEnvironment();

                EventSim.OnNewAnimSelected(Graph.EventBoxes);
            }
            
        }

        public void GeneralUpdate()
        {
            if (Graph.MainScreen.Config.CameraFollowsRootMotion && CurrentModel != null)
            {
                GFX.World.WorldMatrixMOD = Matrix.CreateTranslation(
                    -Vector3.Transform(Vector3.Zero, 
                    CurrentModel.CurrentRootMotionTransform.WorldMatrix));
            }
        }

        public void DrawDebug()
        {
            var printer = new StatusPrinter(Vector2.Zero, Color.Yellow);

            printer.AppendLine($"Animation: {(CurrentModel?.AnimContainer?.CurrentAnimationName ?? "None")}");
            

            if (EntityType == TaeEntityType.PC)
            {
                printer.AppendLine($"R Weapon Animation: {(CurrentModel?.ChrAsm?.RightWeaponModel?.AnimContainer?.CurrentAnimationName ?? "NONE")}");
                printer.AppendLine($"L Weapon Animation: {(CurrentModel?.ChrAsm?.LeftWeaponModel?.AnimContainer?.CurrentAnimationName ?? "NONE")}");
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


            printer.Draw();
        }
    }
}
