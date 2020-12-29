using DSAnimStudio.DbgMenus;
using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using DSAnimStudio.ImguiOSD;
using DSAnimStudio.DebugPrimitives;

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
            (CurrentComboIndex >= 0 && EventSim?.GetSimEnabled("EventSimBasicBlending_Combos") == true) ||
            (CurrentComboIndex < 0 && EventSim?.GetSimEnabled("EventSimBasicBlending") == true);

        public TaeEntityType EntityType { get; private set; } = TaeEntityType.NONE;

        public TaeEventSimulationEnvironment EventSim { get; private set; }

        public static DebugPrimitives.DbgPrimWireArrow DbgPrim_RootMotionStartPoint = null;
        public static DebugPrimitives.DbgPrimWireArrow DbgPrim_RootMotionCurrentPoint = null;
        public static DebugPrimitives.DbgPrimWireBone DbgPrim_RootMotionPathLine = null;

        public Matrix DbgPrim_RootMotionStartPoint_Matrix = Matrix.Identity;
        public Matrix DbgPrim_RootMotionStartPoint_Matrix_PrevLoop = Matrix.CreateScale(0);

        public Queue<Transform> DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue = new Queue<Transform>();
        public Queue<Transform> DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_PrevLoop = new Queue<Transform>();
        private float DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_CurrentTime = 0;
        private float DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_TimeDeltaThreshold => 1f / Main.Config.RootMotionPathUpdateRate;

        public void HardResetRootMotionToStartHere()
        {
            lock (_lock_UpdateAndDrawRootMotionPoints)
            {
                DbgPrim_RootMotionStartPoint_Matrix_PrevLoop = Matrix.CreateScale(0);
                DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_PrevLoop = new Queue<Transform>();
                DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue = new Queue<Transform>();
            }
            MarkRootMotionStart();
        }

        private void CreateRootMotionPoints()
        {
            lock (_lock_UpdateAndDrawRootMotionPoints)
            {
                if (DbgPrim_RootMotionStartPoint == null)
                    DbgPrim_RootMotionStartPoint = new DebugPrimitives.DbgPrimWireArrow("Root Motion Start Point", new Transform(Matrix.CreateScale(0.25f)), Color.White)
                    {
                        Category = DbgPrimCategory.AlwaysDraw,
                        OverrideColor = Main.Colors.ColorHelperRootMotionStartLocation,
                    };

                if (DbgPrim_RootMotionCurrentPoint == null)
                    DbgPrim_RootMotionCurrentPoint = new DebugPrimitives.DbgPrimWireArrow("Root Motion Current Point", new Transform(Matrix.CreateScale(0.15f)), Color.White)
                    {
                        Category = DbgPrimCategory.AlwaysDraw,
                        OverrideColor = Main.Colors.ColorHelperRootMotionCurrentLocation,
                    };

                if (DbgPrim_RootMotionPathLine == null)
                {
                    DbgPrim_RootMotionPathLine = new DbgPrimWireBone("Root Motion Trail Piece", new Transform(Matrix.Identity), Color.White)
                    {
                        Category = DbgPrimCategory.AlwaysDraw,
                        OverrideColor = Main.Colors.ColorHelperRootMotionTrail,
                    };
                }

            }
        }

        private object _lock_UpdateAndDrawRootMotionPoints = new object();

        public void UpdateAndDrawRootMotionPoints(NewBlendableTransform absoluteRootMotionLocation, bool doDraw, float deltaTime = 0)
        {
            CreateRootMotionPoints();

            if (!Main.Config.RootMotionPathEnabled)
            {
                DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.Clear();
                DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_PrevLoop.Clear();
            }

            lock (_lock_UpdateAndDrawRootMotionPoints)
            {
                DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_CurrentTime += Math.Abs(deltaTime);

                if (DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_CurrentTime >= DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_TimeDeltaThreshold)
                {
                    if (Main.Config.RootMotionPathEnabled)
                        DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.Enqueue(new Transform(Matrix.CreateScale(0.15f) * absoluteRootMotionLocation.GetMatrix().ToXna()));

                    while (DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.Count > Main.Config.RootMotionPathSampleMax && Main.Config.RootMotionPathSampleMax < TaeConfigFile.RootMotionPathSampleMaxInfinityValue)
                        DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.Dequeue();

                    DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_CurrentTime = 0;
                }

                var rootMotionDispLocation = absoluteRootMotionLocation;

                if (Graph.MainScreen.Config.WrapRootMotion)
                {
                    rootMotionDispLocation.Translation.X = rootMotionDispLocation.Translation.X % 1;
                    rootMotionDispLocation.Translation.Z = rootMotionDispLocation.Translation.Z % 1;
                }

                Vector3 globalTranslationOffset = (rootMotionDispLocation.Translation - absoluteRootMotionLocation.Translation).ToXna();

                if (doDraw && Main.Config.RootMotionPathEnabled)
                {
                    DbgPrim_RootMotionStartPoint.OverrideColor = Main.Colors.ColorHelperRootMotionStartLocation;
                    DbgPrim_RootMotionStartPoint.Transform = new Transform(DbgPrim_RootMotionStartPoint_Matrix);
                    DbgPrim_RootMotionStartPoint.Draw(null, Matrix.CreateTranslation(globalTranslationOffset));

                    DbgPrim_RootMotionStartPoint.OverrideColor = Main.Colors.ColorHelperRootMotionStartLocation_PrevLoop;
                    DbgPrim_RootMotionStartPoint.Transform = new Transform(DbgPrim_RootMotionStartPoint_Matrix_PrevLoop);
                    DbgPrim_RootMotionStartPoint.Draw(null, Matrix.CreateTranslation(globalTranslationOffset));
                }

                DbgPrim_RootMotionPathLine.OverrideColor = Main.Colors.ColorHelperRootMotionTrail;

                if (doDraw)
                {
                    var asList = DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.ToList();


                    for (int i = 0; i < asList.Count; i++)
                    {
                        if (doDraw)
                        {


                            if (i < asList.Count - 1)
                            {
                                var next = asList[i + 1];
                                var curPos = Vector3.Transform(Vector3.Zero, asList[i].WorldMatrix);
                                var nextPos = Vector3.Transform(Vector3.Zero, asList[i + 1].WorldMatrix);
                                float length = (nextPos - curPos).Length();
                                DbgPrim_RootMotionPathLine.Transform = new Transform(Matrix.CreateScale(length) *
                                    Matrix.CreateRotationY(MathHelper.PiOver2) *
                                    Matrix.CreateWorld(curPos, nextPos - curPos, Vector3.Up));
                                DbgPrim_RootMotionPathLine.Draw(null, Matrix.CreateTranslation(globalTranslationOffset));
                            }
                        }
                    }


                    if (DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_PrevLoop != null)
                    {
                        DbgPrim_RootMotionPathLine.OverrideColor = Main.Colors.ColorHelperRootMotionTrail_PrevLoop;

                        asList = DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_PrevLoop.ToList();

                        for (int i = 0; i < asList.Count; i++)
                        {
                            if (doDraw)
                            {


                                if (i < asList.Count - 1)
                                {
                                    var next = asList[i + 1];
                                    var curPos = Vector3.Transform(Vector3.Zero, asList[i].WorldMatrix);
                                    var nextPos = Vector3.Transform(Vector3.Zero, asList[i + 1].WorldMatrix);
                                    float length = (nextPos - curPos).Length();
                                    DbgPrim_RootMotionPathLine.Transform = new Transform(Matrix.CreateScale(length) *
                                        Matrix.CreateRotationY(MathHelper.PiOver2) *
                                        Matrix.CreateWorld(curPos, nextPos - curPos, Vector3.Up));
                                    DbgPrim_RootMotionPathLine.Draw(null, Matrix.CreateTranslation(globalTranslationOffset));
                                }
                            }
                        }
                    }
                }

                if (doDraw)
                {
                    DbgPrim_RootMotionCurrentPoint.OverrideColor = Main.Colors.ColorHelperRootMotionCurrentLocation;
                    DbgPrim_RootMotionCurrentPoint.Transform = new Transform(Matrix.CreateScale(0.25f) * absoluteRootMotionLocation.GetMatrix().ToXna());

                    if (Main.Config.RootMotionPathEnabled)
                        DbgPrim_RootMotionCurrentPoint.Draw(null, Matrix.CreateTranslation(globalTranslationOffset));
                }


                if (!doDraw)
                {

                    if (Graph.MainScreen.Config.CameraFollowsRootMotion)
                    {
                        GFX.World.RootMotionFollow_Translation = rootMotionDispLocation.Translation.ToXna();
                    }
                    else
                    {
                        if (GFX.World.RootMotionFollow_Translation.LengthSquared() != 0)
                        {
                            GFX.World.CameraOrbitOrigin += GFX.World.RootMotionFollow_Translation;
                        }

                        GFX.World.RootMotionFollow_Translation = Vector3.Zero;
                    }

                    if (Graph.MainScreen.Config.CameraFollowsRootMotionRotation)
                    {
                        GFX.World.RootMotionFollow_Rotation = rootMotionDispLocation.GetWrappedYawAngle();
                    }
                    else
                    {
                        if (GFX.World.RootMotionFollow_Rotation != 0)
                        {
                            GFX.World.CameraLookDirection *= Quaternion.CreateFromYawPitchRoll(GFX.World.RootMotionFollow_Rotation, 0, 0);
                        }

                        GFX.World.RootMotionFollow_Rotation = 0;
                    }

                    CurrentModel.StartTransform = CurrentModel.CurrentTransform = new Transform(rootMotionDispLocation.GetMatrix().ToXna());

                    //GFX.World.UpdateDummyPolyFollowRefPoint(isFirstTime: false);
                    GFX.World.Update(0);
                }
            }
        }

        public void MarkRootMotionStart()
        {
            CreateRootMotionPoints();

            var curTransform = Matrix.CreateScale(0.5f) * (CurrentModel?.AnimContainer?.RootMotionTransform ?? NewBlendableTransform.Identity).GetMatrix().ToXna();

            if (DbgPrim_RootMotionStartPoint_Matrix != curTransform)
            {
                // Force update last frame.
                UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false, DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_TimeDeltaThreshold);
            }

            lock (_lock_UpdateAndDrawRootMotionPoints)
            {
                DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_CurrentTime = 0;

                

                if (DbgPrim_RootMotionStartPoint_Matrix != curTransform)
                {
                    DbgPrim_RootMotionStartPoint_Matrix_PrevLoop = DbgPrim_RootMotionStartPoint_Matrix;
                    DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_PrevLoop = DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue;
                    DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue = new Queue<Transform>();
                    DbgPrim_RootMotionStartPoint_Matrix = curTransform;
                }

                
            }

            // Force update first frame.
            UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false, DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_TimeDeltaThreshold);
        }


        public static float StatusTextScale = 100.0f;

        public Model CurrentModel => Scene.MainModel;

        public void SetEntityType(TaeEntityType entityType)
        {
            EntityType = entityType;

            if (entityType != TaeEntityType.NPC && CurrentModel != null)
            {
                lock (CurrentModel._lock_NpcParams)
                {
                    CurrentModel?.PossibleNpcParams?.Clear();
                    CurrentModel.SelectedNpcParamIndex = -1;
                }
            }

            if (!(entityType == TaeEntityType.PC || entityType == TaeEntityType.REMO))
            {
                OSD.WindowEditPlayerEquip.IsOpen = false;
            }
          
        }

        

        public void InitializeCharacterModel(Model mdl, bool isRemo)
        {
            if (mdl.IS_PLAYER)
            {
                lock (mdl._lock_NpcParams)
                {
                    mdl.PossibleNpcParams.Clear();
                    mdl.SelectedNpcParamIndex = -1;
                }

                mdl.CreateChrAsm();

                mdl.ChrAsm.EquipmentModelsUpdated += ChrAsm_EquipmentModelsUpdated;

                if (!Graph.MainScreen.Config.ChrAsmConfigurations.ContainsKey(GameDataManager.GameType))
                {
                    Graph.MainScreen.Config.ChrAsmConfigurations.Add
                        (GameDataManager.GameType, new NewChrAsmCfgJson());
                }

                Graph.MainScreen.Config.ChrAsmConfigurations[GameDataManager.GameType]
                    .WriteToChrAsm(mdl.ChrAsm);

                mdl.ChrAsm.UpdateModels(isAsync: true);

                SetEntityType(isRemo ? TaeEntityType.REMO : TaeEntityType.PC);
            }
            else
            {
                mdl.RescanNpcParams();

                SetEntityType(isRemo ? TaeEntityType.REMO : TaeEntityType.NPC);

                lock (mdl._lock_NpcParams)
                {

                    mdl.NpcMaterialNamesPerMask = mdl.GetMaterialNamesPerMask();


                    mdl.NpcMasksEnabledOnAllNpcParams = mdl.NpcMaterialNamesPerMask.Select(kvp => kvp.Key).ToList();
                    foreach (var kvp in mdl.NpcMaterialNamesPerMask)
                    {
                        if (kvp.Key < 0)
                            continue;

                        foreach (var npcParam in mdl.PossibleNpcParams)
                        {
                            if (npcParam.DrawMask.Length <= kvp.Key || !npcParam.DrawMask[kvp.Key])
                            {
                                if (mdl.NpcMasksEnabledOnAllNpcParams.Contains(kvp.Key))
                                    mdl.NpcMasksEnabledOnAllNpcParams.Remove(kvp.Key);

                                break;
                            }
                        }
                    }

                }

                //foreach (var npc in validNpcParams)
                //{
                //    Graph.MainScreen.MenuBar.AddItem("Behavior Variation ID", $"Apply Behavior Variation ID " +
                //        $"from NpcParam {npc.ID} {npc.Name}", () =>
                //    {
                //        npc.ApplyMaskToModel(CurrentModel);
                //    });
                //}

            }
        }

        public TaeViewportInteractor(TaeEditAnimEventGraph graph)
        {
            
            RemoManager.NukeEntireRemoSystemAndGoBackToNormalDSAnimStudio();
            

            OSD.WindowEditPlayerEquip.IsOpen = false;

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

                InitializeCharacterModel(CurrentModel, isRemo: false);

                CurrentModel.AfterAnimUpdate(timeDelta: 0);

                GFX.World.NewDoRecenterAction = () =>
                {
                    GFX.World.CameraLookDirection = Quaternion.Identity;
                };
                GFX.World.NewRecenter();

                LoadSoundsForCurrentModel();
            }
            else if (shortFileName.StartsWith("o"))
            {
                SetEntityType(TaeEntityType.OBJ);

                GameDataManager.LoadObject(shortFileName);

                GFX.World.NewDoRecenterAction = () =>
                {
                    GFX.World.CameraLookDirection = Quaternion.Identity;
                };
                GFX.World.NewRecenter();

                LoadSoundsForCurrentModel();

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

                Scene.DisableModelDrawing();

                RemoManager.ViewportInteractor = this;

                RemoManager.DisposeAllModels();
                RemoManager.RemoName = Utils.GetShortIngameFileName(Graph.MainScreen.FileContainerName);



                FmodManager.Purge();
                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                {
                    FmodManager.LoadInterrootFEV("main");
                    var dlc = FmodManager.GetFevPathFromInterroot("main", isDs1Dlc: true);
                    if (System.IO.File.Exists(dlc))
                        FmodManager.LoadFEV(dlc);

                    FmodManager.LoadInterrootFEV("smain");
                    dlc = FmodManager.GetFevPathFromInterroot("smain", isDs1Dlc: true);
                    if (System.IO.File.Exists(dlc))
                        FmodManager.LoadFEV(dlc);

                    FmodManager.LoadInterrootFEV($"m{RemoManager.AreaInt:D2}");
                    dlc = FmodManager.GetFevPathFromInterroot($"m{RemoManager.AreaInt:D2}", isDs1Dlc: true);
                    if (System.IO.File.Exists(dlc))
                        FmodManager.LoadFEV(dlc);

                    FmodManager.LoadInterrootFEV($"sm{RemoManager.AreaInt:D2}");
                    dlc = FmodManager.GetFevPathFromInterroot($"sm{RemoManager.AreaInt:D2}", isDs1Dlc: true);
                    if (System.IO.File.Exists(dlc))
                        FmodManager.LoadFEV(dlc);

                    FmodManager.LoadInterrootFEV($"p{RemoManager.RemoName.Substring(3)}");
                    dlc = FmodManager.GetFevPathFromInterroot($"p{RemoManager.RemoName.Substring(3)}", isDs1Dlc: true);
                    if (System.IO.File.Exists(dlc))
                        FmodManager.LoadFEV(dlc);
                }
                
                RemoManager.LoadRemoDict(Graph.MainScreen.FileContainer);

                if (Scene.Models.Count == 0)
                    GameDataManager.LoadCharacter("c0000");

                lock (Scene._lock_ModelLoad_Draw)
                {
                    Scene.Models = Scene.Models.OrderBy(m => m.IS_PLAYER ? 0 : 1).ToList();
                }

                //throw new NotImplementedException("REMO NOT SUPPORTED YET");
            }

            

            InitializeForCurrentModel();

            Scene.EnableModelDrawing();
            if (Scene.IsEmpty)
                GameDataManager.LoadCharacter("c1000");

            if (!CurrentModel.IS_PLAYER)
                Scene.EnableModelDrawing2();
        }

        public void LoadSoundsForCurrentModel(bool fresh = true)
        {
            if (fresh)
                FmodManager.Purge();
            FmodManager.LoadMainFEVs();
            FmodManager.LoadInterrootFEV(CurrentModel.Name);
        }

        public void InitializeForCurrentModel()
        {
            if (CurrentModel != null)
            {
                if (CurrentModel?.AnimContainer.Skeleton != null)
                    CurrentComboRecorder = new HavokRecorder(CurrentModel.AnimContainer.Skeleton.HkxSkeleton);

                GFX.World.OrbitCamDistanceInput = (CurrentModel.Bounds.Max - CurrentModel.Bounds.Min).Length() * 2f;
                if (GFX.World.OrbitCamDistanceInput < 0.5f)
                    GFX.World.OrbitCamDistanceInput = 5;

                lock (CurrentModel._lock_NpcParams)
                {

                    CurrentModel.NpcMaterialNamesPerMask = CurrentModel.GetMaterialNamesPerMask();

                    CurrentModel.NpcMasksEnabledOnAllNpcParams = CurrentModel.NpcMaterialNamesPerMask.Select(kvp => kvp.Key).ToList();
                    foreach (var kvp in CurrentModel.NpcMaterialNamesPerMask)
                    {
                        if (kvp.Key < 0)
                            continue;

                        foreach (var npcParam in CurrentModel.PossibleNpcParams)
                        {
                            if (npcParam.DrawMask.Length <= kvp.Key || !npcParam.DrawMask[kvp.Key])
                            {
                                if (CurrentModel.NpcMasksEnabledOnAllNpcParams.Contains(kvp.Key))
                                    CurrentModel.NpcMasksEnabledOnAllNpcParams.Remove(kvp.Key);

                                break;
                            }
                        }
                    }
                }

                OSD.WindowEntitySettings.IsOpen = true;
            }
        }

        private void ChrAsm_EquipmentModelsUpdated(object sender, EventArgs e)
        {
            if (Graph == null || Graph.PlaybackCursor == null || Graph.MainScreen.PlaybackCursor == null)
                return;

            Graph.MainScreen.SelectNewAnimRef(Graph.MainScreen.SelectedTae, Graph.MainScreen.SelectedTaeAnim);

            //V2.0: Scrub weapon anims to the current frame.

            CurrentModel.ChrAsm?.RightWeaponModel0?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm?.RightWeaponModel1?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm?.RightWeaponModel2?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm?.RightWeaponModel3?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm?.LeftWeaponModel0?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm?.LeftWeaponModel1?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm?.LeftWeaponModel2?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);
            CurrentModel.ChrAsm?.LeftWeaponModel3?.AnimContainer.ScrubRelative(timeDelta: CurrentModel.AnimContainer.CurrentAnimTime);

            //V2.0: Update stuff probably
            CurrentModel.AfterAnimUpdate(0);
        }

        //private void EquipForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

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
            if (EntityType == TaeEntityType.REMO)
            {
                bool remoCutAdv = RemoManager.UpdateCutAdvance();
                if (remoCutAdv)
                    return;
            }

            //V2.0
            //CurrentModel.AnimContainer.IsPlaying = false;
            CurrentModel.AnimContainer.EnableLooping = Main.Config.LoopEnabled;
            CurrentModel.AnimContainer.ScrubRelative(timeDelta);

            

            

            if (UpdateCombo())
            {
                return;
            }

            //V2.0
            //CurrentModel.ChrAsm?.UpdateWeaponTransforms(timeDelta);

            CheckSimEnvironment();
            EventSim.OnSimulationFrameChange(Graph.EventBoxesToSimulate, (float)Graph.PlaybackCursor.CurrentTimeMod);

            if (EntityType == TaeEntityType.REMO)
            {
                RemoManager.UpdateRemoTime((float)Graph.PlaybackCursor.GUICurrentTimeMod);
            }

            GFX.World.Update(0);

            UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false, timeDelta);
            CurrentModel.AfterAnimUpdate(timeDelta);
        }

        private void CheckSimEnvironment()
        {
            if (EventSim == null || EventSim.MODEL != CurrentModel)
            {
                EventSim = new TaeEventSimulationEnvironment(Graph, CurrentModel);
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

            //TODO: Check if this is going to deadlock
          
                try
                {
                    //V2.0
                    //CurrentModel.AnimContainer.IsPlaying = false;
                    CurrentModel.AnimContainer.EnableLooping = Main.Config.LoopEnabled;
                    CurrentModel.AnimContainer.ScrubRelative(timeDelta);

                UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false, timeDelta);

                CurrentModel.AfterAnimUpdate(timeDelta);

                

                if (CurrentComboIndex >= 0 && IsComboRecording)
                    {
                        if (UpdateCombo())
                            return;
                    }

                    //TODO: Check if putting this inside the lock() fixes.
                    CheckSimEnvironment();
                    EventSim.OnSimulationFrameChange(Graph.EventBoxesToSimulate, (float)Graph.PlaybackCursor.GUICurrentTimeMod);
                }
                catch
                {

                }
            
            //V2.0
            //CurrentModel.ChrAsm?.UpdateWeaponTransforms(timeDelta);



            GFX.World.Update(0);

            if (EntityType == TaeEntityType.REMO)
            {
                RemoManager.UpdateRemoTime((float)Graph.PlaybackCursor.GUICurrentTimeMod);
            }
        }

        public void StartCombo(bool isLoop, bool isRecord, TaeComboEntry[] entries)
        {
            if (CurrentComboIndex >= 0)
            {
                // Just in case?
                CancelCombo();
            }    

            Main.Config.LoopEnabled_BeforeCombo = Main.Config.LoopEnabled;
            // Do this before setting current combo since inside here, it will check if there's a combo and not reset stuff if there's currently a combo happening.
            CurrentComboIndex = -1;
            lock (Graph._lock_EventBoxManagement)
            {
                EventSim.OnNewAnimSelected(Graph.EventBoxes);
            }

            if (isRecord)
                CurrentComboRecorder.ClearRecording();

            CurrentComboLoop = isLoop;
            IsComboRecording = false;
            CurrentCombo = entries;
            CurrentComboIndex = 0;
            if (CurrentModel.ChrAsm != null)
            {
                CurrentModel.ChrAsm.WeaponStyle = CurrentModel.ChrAsm.StartWeaponStyle;
            }

            StartCurrentComboEntry(true, isRecord);
            RemoveTransition();
        }

        private void StartCurrentComboEntry(bool isFirstTime, bool isRecord = false)
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

            if (isRecord)
            {
                if (Graph.PlaybackCursor.IsPlaying)
                {
                    Graph.PlaybackCursor.Transport_PlayPause(isUserInput: false);
                }

                //RecordCurrentComboFrame();

                IsComboRecording = true;
            }
            else
            {
                if (!Graph.PlaybackCursor.IsPlaying)
                {
                    Graph.PlaybackCursor.Transport_PlayPause(isUserInput: false);
                }
            }

            
        }

        public TaeComboEntry[] CurrentCombo = new TaeComboEntry[0];
        public int CurrentComboIndex = -1;
        public bool CurrentComboLoop = false;
        public bool IsComboRecording = false;
        private Vector4 CurrentComboRecordLastRootMotion = Vector4.Zero;
        private HavokRecorder CurrentComboRecorder;

        public void CancelCombo()
        {
            Main.Config.LoopEnabled = Main.Config.LoopEnabled_BeforeCombo;
            IsComboRecording = false;
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
                        Graph.PlaybackCursor.Transport_PlayPause(isUserInput: false);

                    if (IsComboRecording)
                        CurrentComboRecorder?.FinalizeRecording();

                    CurrentComboIndex = -1;
                }

                
            }
        }

        private void RecordCurrentComboFrame()
        {
            Vector3 curModelPosition = Vector3.Transform(Vector3.Zero, CurrentModel.CurrentTransform.WorldMatrix);

            throw new NotImplementedException();
            Vector4 curRootMotionLocation = new Vector4(curModelPosition, 0/*CurrentModel.AnimContainer.Skeleton.CurrentDirection*/);
            Vector4 rootMotionDelta = curRootMotionLocation - CurrentComboRecordLastRootMotion;
            CurrentComboRecorder.AddFrame(rootMotionDelta, CurrentModel.AnimContainer.Skeleton.HkxSkeleton);
            CurrentComboRecordLastRootMotion = curRootMotionLocation;
        }

        private bool UpdateCombo()
        {
            if ((!Graph.PlaybackCursor.IsPlaying || Graph.PlaybackCursor.Scrubbing) && !IsComboRecording)
                return false;


            if (CurrentComboIndex >= 0)
            {
                Main.Config.LoopEnabled = false;

                if (CurrentComboIndex < CurrentCombo.Length)
                {

                    if (Graph.PlaybackCursor.MaxTime > 0 && (Graph.PlaybackCursor.CurrentTime >= Graph.PlaybackCursor.MaxTime) ||
                        (CurrentCombo[CurrentComboIndex].EndFrame >= 0 && Graph.PlaybackCursor.CurrentFrame >= CurrentCombo[CurrentComboIndex].EndFrame))
                    {
                        if (CurrentCombo[CurrentComboIndex].EndFrame >= 0)
                        {
                            Graph.PlaybackCursor.CurrentTime = (CurrentCombo[CurrentComboIndex].EndFrame * Graph.PlaybackCursor.CurrentSnapInterval);
                        }
                        //else
                        //{
                        //    Graph.PlaybackCursor.CurrentTime = Graph.PlaybackCursor.MaxTime - 0.00005f;
                        //}
                        
                        //Graph.PlaybackCursor.UpdateScrubbing();

                        GoToNextItemInCombo();
                        return true;
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
                                    return true;
                                }
                            }
                        }

                    }




                }
                else
                {
                    CurrentComboIndex = -1;
                    IsComboRecording = false;
                    CurrentComboRecorder.FinalizeRecording();
                    return true;
                }


                
            }

            return false;
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
            CurrentModel.AnimContainer?.ResetRootMotion();
            MarkRootMotionStart();
        }

        public void RemoveTransition()
        {
            void DoAnimContainer(NewAnimationContainer animContainer)
            {
                if (animContainer == null)
                    return;
                if (animContainer != null && animContainer.AnimationLayers.Count > 1)
                {
                    lock (animContainer._lock_AnimationLayers)
                    {
                        var curAnim = animContainer.AnimationLayers[animContainer.AnimationLayers.Count - 1];
                        animContainer.AnimationLayers.Clear();
                        curAnim.Weight = 1;
                        curAnim.ReferenceWeight = 1;
                        animContainer.AnimationLayers.Add(curAnim);
                    }
                }
            }

            DoAnimContainer(CurrentModel?.AnimContainer);
            DoAnimContainer(CurrentModel?.ChrAsm?.RightWeaponModel0?.AnimContainer);
            DoAnimContainer(CurrentModel?.ChrAsm?.RightWeaponModel1?.AnimContainer);
            DoAnimContainer(CurrentModel?.ChrAsm?.RightWeaponModel2?.AnimContainer);
            DoAnimContainer(CurrentModel?.ChrAsm?.RightWeaponModel3?.AnimContainer);
            DoAnimContainer(CurrentModel?.ChrAsm?.LeftWeaponModel0?.AnimContainer);
            DoAnimContainer(CurrentModel?.ChrAsm?.LeftWeaponModel1?.AnimContainer);
            DoAnimContainer(CurrentModel?.ChrAsm?.LeftWeaponModel2?.AnimContainer);
            DoAnimContainer(CurrentModel?.ChrAsm?.LeftWeaponModel3?.AnimContainer);
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
                CurrentModel.AnimContainer.ResetRootMotion();
                //CurrentModel.AnimContainer.CurrentRootMotionDirection = 0;

                CurrentModel.ChrAsm?.RightWeaponModel0?.AnimContainer?.ResetRootMotion();
                CurrentModel.ChrAsm?.RightWeaponModel1?.AnimContainer?.ResetRootMotion();
                CurrentModel.ChrAsm?.RightWeaponModel2?.AnimContainer?.ResetRootMotion();
                CurrentModel.ChrAsm?.RightWeaponModel3?.AnimContainer?.ResetRootMotion();

                CurrentModel.ChrAsm?.LeftWeaponModel0?.AnimContainer?.ResetRootMotion();
                CurrentModel.ChrAsm?.LeftWeaponModel1?.AnimContainer?.ResetRootMotion();
                CurrentModel.ChrAsm?.LeftWeaponModel2?.AnimContainer?.ResetRootMotion();
                CurrentModel.ChrAsm?.LeftWeaponModel3?.AnimContainer?.ResetRootMotion();
            }
        }

        public string GetFinalAnimFileName(TAE tae, TAE.Animation anim)
        {
            if (EntityType == TaeEntityType.REMO)
            {
                return $"a{anim.ID:D4}.hkx";
            }

            if (CurrentModel == null || CurrentModel?.AnimContainer == null || Graph == null || Graph?.MainScreen?.FileContainer?.AllTAEDict == null)
                return null;

            if (Graph.MainScreen.FileContainer.IsCurrentlyLoading)
                return null;

            var mainChrSolver = new TaeAnimRefChainSolver(Graph.MainScreen.FileContainer.AllTAEDict, CurrentModel.AnimContainer.Animations);
            return mainChrSolver.GetHKXName(tae, anim);
        }

        public void OnNewAnimSelected()
        {
            if (EntityType == TaeEntityType.REMO)
            {
                RemoManager.LoadRemoCut($"a{ Graph.MainScreen.SelectedTaeAnim.ID:D4}.hkx");
                RemoManager.AnimContainer.ScrubRelative(0);
                GFX.World.Update(0);
            }
            else if (CurrentModel != null)
            {
                var mainChrSolver = new TaeAnimRefChainSolver(Graph.MainScreen.FileContainer.AllTAEDict, CurrentModel.AnimContainer.Animations);
                var mainChrAnimName = mainChrSolver.GetHKXName(Graph.MainScreen.SelectedTae, Graph.MainScreen.SelectedTaeAnim);

                //V2.0: See if this needs something similar in the new system.
                //CurrentModel.AnimContainer.StoreRootMotionRotation();

                //CurrentModel.AnimContainer.CurrentAnimation.RotMatrixAtStartOfAnim

                CurrentModel.AnimContainer.CurrentAnimationName = mainChrAnimName;

                CurrentModel.ChrAsm?.SelectWeaponAnimations(mainChrAnimName);

                prevRootMotionLoopCount = 0;
                MarkRootMotionStart();

                //lock (CurrentModel.AnimContainer._lock_AnimationLayers)
                //{
                //    if ((CurrentModel.AnimContainer.AnimationLayers.Count == 1 && CurrentModel.AnimContainer.AnimationLayers[0].IsAdditiveBlend) ||
                //      (CurrentModel.AnimContainer.AnimationLayers.Count == 2 && (CurrentModel.AnimContainer.AnimationLayers[0].IsAdditiveBlend || CurrentModel.AnimContainer.AnimationLayers[1].IsAdditiveBlend)))
                //    {
                //        RemoveTransition();
                //    }
                //}



                Graph.PlaybackCursor.ResetAll();

                Graph.PlaybackCursor.HkxAnimationLength = CurrentModel.AnimContainer.CurrentAnimDuration;
                Graph.PlaybackCursor.SnapInterval = CurrentModel?.AnimContainer?.CurrentAnimFrameDuration;
                Graph.PlaybackCursor.CurrentTime = 0;
                Graph.PlaybackCursor.StartTime = 0;
                

                CheckChrAsmWeapons();

                //FindWeaponAnim(mainChrSolver, lastRightWeaponModelTAEWasReadFrom, lastRightWeaponTAE);
                //FindWeaponAnim(mainChrSolver, lastLeftWeaponModelTAEWasReadFrom, lastLeftWeaponTAE);

                CheckSimEnvironment();

                lock (Graph._lock_EventBoxManagement)
                {
                    EventSim.OnNewAnimSelected(Graph.EventBoxes);
                }

                if (CurrentModel.AnimContainer.CurrentAnimation != null)
                {
                    //V2.0: Check
                    CurrentModel.AnimContainer.CurrentAnimation.Reset(CurrentModel.AnimContainer.RootMotionTransform.GetRootMotionVector4());

                    UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false);

                    OnScrubFrameChange(0);

                    CurrentModel.AnimContainer.ScrubRelative(0);

                    MarkRootMotionStart();

                    UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false);
                }
                else
                {
                    CurrentModel.SkeletonFlver.RevertToReferencePose();
                }
            }
            
        }

        public bool IsAnimLoaded(string name)
        {
            if (EntityType == TaeEntityType.REMO)
            {
                return RemoManager.RemoCutLoaded(name);
            }
            else
            {
                return CurrentModel?.AnimContainer?.IsAnimLoaded(name) ?? false;
            }
           
        }

        float modelDirectionLastFrame = 0;

        private int lastFrameForTrails = -1;

        public void GeneralUpdate_BeforePrimsDraw()
        {
            DBG.DbgPrimXRay = Graph.MainScreen.Config.DbgPrimXRay;

            if (CurrentModel != null)
            {
                int frame = (int)Math.Round(Graph.PlaybackCursor.CurrentTime / (1.0 / 60.0));

                if (Graph.PlaybackCursor.ContinuousTimeDelta != 0)
                {
                    if (frame != lastFrameForTrails)
                        EventSim?.UpdateAllBladeSFXsLowHz();


                    EventSim?.UpdateAllBladeSFXsLive();

                }





                lastFrameForTrails = frame;
            }
            else
            {
                GFX.World.RootMotionFollow_Translation = Vector3.Zero;
                GFX.World.RootMotionFollow_Rotation = 0;
            }
        }

        int prevRootMotionLoopCount = -1;

        public void GeneralUpdate(bool allowPlaybackManipulation = true)
        {
            

            
            if (CurrentModel != null)
            {
                // allowPlaybackManipulation==false prevents infinite recursion.
                if (allowPlaybackManipulation && IsComboRecording && CurrentComboIndex >= 0)
                {
                    Graph.PlaybackCursor.IsPlaying = false;
                    Graph.PlaybackCursor.Scrubbing = true;
                    Graph.PlaybackCursor.IsStepping = false;
                    Graph.PlaybackCursor.CurrentTime += CurrentComboRecorder.DeltaTime;
                    Graph.PlaybackCursor.UpdateScrubbing();
                    RecordCurrentComboFrame();
                }

                
            }

            if (CurrentModel.AnimContainer?.CurrentAnimation?.RootMotion != null)
            {

                int curRootMotionLoopCount = CurrentModel.AnimContainer.CurrentAnimation.RootMotion.CurrentLoopNum;
                if (curRootMotionLoopCount != prevRootMotionLoopCount)
                {
                    MarkRootMotionStart();
                }
                prevRootMotionLoopCount = curRootMotionLoopCount;

            }
            //else
            //{
            //    DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.Clear();
            //}

        }

        public void DrawDebug()
        {
            if (CurrentModel != null)
            {
                EventSim?.DrawAllBladeSFXs(CurrentModel.CurrentTransform.WorldMatrix);

                UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, true);
            }

            
        }

        public void DrawDebugOverlay()
        {
            

            var printer = new StatusPrinter(Vector2.One * 4, Main.Colors.GuiColorViewportStatus);

            if (Graph?.PlaybackCursor.IsPlayingRemoFullPreview == true)
            {
                printer.AppendLine("[CUTSCENE PLAY MODE]", Color.Cyan);
                printer.BaseScale = (StatusTextScale / 100) * 0.75f;
                printer.Draw();
                return;
            }

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
                if (CurrentModel.SkeletonFlver.BoneLimitExceeded)
                {
                    printer.AppendLine($"Warning: Model exceeds max bone count.", Main.Colors.GuiColorViewportStatusMaxBoneCountExceeded);
                }

                printer.AppendLine($"[ANIMATION LAYERS:]");
                lock (CurrentModel.AnimContainer._lock_AnimationLayers)
                {
                    foreach (var anim in CurrentModel.AnimContainer.AnimationLayers)
                        printer.AppendLine($"    [x{anim.Weight:0.00}] [x{anim.CurrentTime:00.000}/{anim.Duration:00.000}] {anim.Name}");
                }

                if (CurrentComboIndex >= 0)
                {
                    printer.AppendLine($"\nPlaying Combo ({(CurrentComboLoop ? "Looping" : "Once")}):", Main.Colors.GuiColorViewportStatusCombo);
                    for (int c = 0; c < CurrentCombo.Length; c++)
                    {
                        printer.AppendLine($"    {(CurrentComboIndex == c ? "■" : "□")} {CurrentCombo[c]}", Main.Colors.GuiColorViewportStatusCombo);
                    }
                }

                //if (CurrentModel.AnimContainer.AnimationLayers.Count > 0)
                //{
                //    //printer.AppendLine($"Active Anims:");

                //    //for (int i = 0; i < CurrentModel.AnimContainer.AnimationLayers.Count; i++)
                //    //{
                //    //    var layer = CurrentModel.AnimContainer.AnimationLayers[i];
                //    //    printer.AppendLine($"[{i:D2}] [{layer.Weight:0.000}x] {layer.Name} [{layer.CurrentTime:0.000}/{layer.Duration:0.000}]");
                //    //}
                //    var tr = GetCurrentTransition();
                //    if (tr != null)
                //    {
                //        printer.AppendLine($"<Showing Transition from {tr}>");
                //    }
                //}
            }

            if (EntityType == TaeEntityType.PC)
            {
                printer.AppendLine();

                void DoWpnAnim(string wpnKind, Model wpnMdl)
                {
                    printer.AppendLine(wpnKind);
                    if (wpnMdl?.AnimContainer?.CurrentAnimation != null)
                    {
                        lock (wpnMdl.AnimContainer._lock_AnimationLayers)
                        {
                            foreach (var anim in wpnMdl.AnimContainer.AnimationLayers)
                                printer.AppendLine($"            [x{anim.Weight:0.00}] [x{anim.CurrentTime:00.000}/{anim.Duration:00.000}] {anim.Name}");
                        }
                    }
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
                EventSim.GetSimEnabled("EventSimSpEffects") && 
                EventSim.SimulatedActiveSpEffects.Count > 0)
            {
                printer.AppendLine("[Active SpEffects:]");

                foreach (var spe in EventSim.SimulatedActiveSpEffects)
                {
                    printer.AppendLine("    " + spe);
                }
            }

            printer.BaseScale = StatusTextScale / 100;

            printer.Draw();
        }
    }
}
