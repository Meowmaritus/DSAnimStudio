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
using DSAnimStudio.GFXShaders;
using Newtonsoft.Json.Linq;
using SoulsAssetPipeline;
using System.Reflection.Metadata;
using System.Xml.Linq;
using System.Runtime;

namespace DSAnimStudio.TaeEditor
{
    public class TaeViewportInteractor
    {
        public readonly NewGraph Graph;

        public zzz_DocumentIns ParentDocument => Graph?.MainScreen?.ParentDocument;

        private object __lock__is_still_loading = new object();
        private bool __is_still_loading = true;
        public bool IS_STILL_LOADING
        {
            get
            {
                bool result = true;
                lock (__lock__is_still_loading)
                {
                    result = __is_still_loading;
                }
                return result;
            }
            private set
            {
                lock ( __lock__is_still_loading)
                {
                    __is_still_loading = value;
                }
            }
        }

        public enum TaeEntityType
        {
            NONE,
            PC,
            NPC,
            OBJ,
            PARTS,
            REMO
        }

        public float ModPlaybackSpeed_GrabityRate = 1.0f;
        public float ModPlaybackSpeed_Event603 = 1.0f;
        public float ModPlaybackSpeed_Event608 = 1.0f;
        public float ModPlaybackSpeed_NightfallEvent7032 = 1.0f;
        public float ModPlaybackSpeed_AC6Event9700 = 1.0f;

        //TODO, obviously
        public bool NewIsComboActive => Combo?.PlaybackState == NewCombo.PlaybackStates.Playing;
        public NewCombo Combo = null;

        public void CancelCombo()
        {
            //TODO
            Combo = null;
        }

        public void RequestCombo(List<NewCombo.Entry> entries)
        {
            Combo = new NewCombo();
            Combo.Entries = entries.ToList();
            Combo.Init(CurrentModel.AnimContainer, Graph.MainScreen);
            Combo.StartPlayback();
        }
        
        public bool IsBlendingActive => 
            (NewIsComboActive && Main.Config.SimEnabled_BasicBlending_ComboViewer) ||
            (!NewIsComboActive && Main.Config.SimEnabled_BasicBlending);

        private TaeEntityType _entityType = TaeEntityType.NONE;
        public TaeEntityType EntityType
        {
            get => _entityType;
            set
            {
                _entityType = value;
                OSD.WindowEntity.EntityType = value;
            }
        }

        

        public TaeActionSimulationEnvironment ActionSim { get; private set; }

        public static DebugPrimitives.DbgPrimWireArrow DbgPrim_RootMotionStartPoint = null;
        public static DebugPrimitives.DbgPrimWireArrow DbgPrim_RootMotionCurrentPoint = null;
        public static DebugPrimitives.DbgPrimWireBone DbgPrim_RootMotionPathLine = null;

        public Matrix DbgPrim_RootMotionStartPoint_Matrix = Matrix.Identity;
        public Matrix DbgPrim_RootMotionStartPoint_Matrix_PrevLoop = Matrix.CreateScale(0);

        public Queue<Transform> DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue = new Queue<Transform>();
        public Queue<Transform> DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_PrevLoop = new Queue<Transform>();
        private float DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_CurrentTime = 0;
        private float DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_TimeDeltaThreshold => 1f / Main.HelperDraw.RootMotionTrailUpdateRate;

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
                    DbgPrim_RootMotionStartPoint = new DebugPrimitives.DbgPrimWireArrow(new Transform(Matrix.CreateScale(0.25f)), Color.White)
                    {
                        OverrideColor = Main.Colors.ColorHelperRootMotionStartLocation,
                    };

                if (DbgPrim_RootMotionCurrentPoint == null)
                    DbgPrim_RootMotionCurrentPoint = new DebugPrimitives.DbgPrimWireArrow( new Transform(Matrix.CreateScale(0.15f)), Color.White)
                    {
                        OverrideColor = Main.Colors.ColorHelperRootMotionCurrentLocation,
                    };

                if (DbgPrim_RootMotionPathLine == null)
                {
                    DbgPrim_RootMotionPathLine = new DbgPrimWireBone(new Transform(Matrix.Identity), Color.White)
                    {
                        OverrideColor = Main.Colors.ColorHelperRootMotionTrail,
                    };
                }

            }
        }

        private object _lock_UpdateAndDrawRootMotionPoints = new object();

        public void UpdateCameraForRootMotion(float deltaTime)
        {

        }

        public void RegisterWorldShift(Vector3 v)
        {
            if (v.LengthSquared() > 0)
            {
                lock (_lock_UpdateAndDrawRootMotionPoints)
                {
                    var shiftMatrix = Matrix.CreateTranslation(v);
                    var transforms = DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.ToList();
                    var transformsPrevLoop = DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_PrevLoop.ToList();
                    for (int i = 0; i < transforms.Count; i++)
                    {
                        transforms[i] = new Transform(transforms[i].WorldMatrix * shiftMatrix);
                    }
                    for (int i = 0; i < transformsPrevLoop.Count; i++)
                    {
                        transformsPrevLoop[i] = new Transform(transformsPrevLoop[i].WorldMatrix * shiftMatrix);
                    }
                    DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue = new Queue<Transform>(transforms);
                    DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_PrevLoop = new Queue<Transform>(transformsPrevLoop);
                    DbgPrim_RootMotionCurrentPoint.Transform.ApplyShift(shiftMatrix);
                    DbgPrim_RootMotionStartPoint.Transform.ApplyShift(shiftMatrix);
                    DbgPrim_RootMotionStartPoint_Matrix *= shiftMatrix;
                    DbgPrim_RootMotionStartPoint_Matrix_PrevLoop *= shiftMatrix;
                }
            }
        }

        public void UpdateAndDrawRootMotionPoints(NewBlendableTransform absoluteRootMotionLocation, bool doDraw, float animDeltaTime = 0, float programDeltaTime = 0)
        {
            CreateRootMotionPoints();

            if (!Main.HelperDraw.EnableRootMotionTrail)
            {
                DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.Clear();
                DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_PrevLoop.Clear();
            }

            GFX.CurrentWorldView.FollowingLockon = Main.Config.CameraFollowType == TaeConfigFile.CameraFollowTypes.BodyDummyPoly;
            GFX.CurrentWorldView.FollowingLockonDummyPoly = Main.Config.CameraFollowDummyPolyID;

            lock (_lock_UpdateAndDrawRootMotionPoints)
            {
                DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_CurrentTime += Math.Abs(animDeltaTime);

                if (DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_CurrentTime >= DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_TimeDeltaThreshold)
                {
                    if (Main.HelperDraw.EnableRootMotionTrail)
                        DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.Enqueue(new Transform(Matrix.CreateScale(0.15f) * absoluteRootMotionLocation.GetMatrix().ToXna()));

                    while (DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.Count > Main.HelperDraw.RootMotionTrailSampleMax && Main.HelperDraw.RootMotionTrailSampleMax < HelperDrawConfig.RootMotionTrailSampleMaxInfinityValue)
                        DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.Dequeue();

                    DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_CurrentTime = 0;
                }

                var rootMotionDispLocation = absoluteRootMotionLocation;
                Vector3 cameraTrans = GFX.CurrentWorldView.RootMotionFollow_Translation;

                

                float transZModify = 0;

                

                if (Graph.MainScreen.Config.CameraFollowsRootMotionZX && !(NewIsComboActive))
                {
                    GFX.CurrentWorldView.RootMotionFollow_EnableWrap = true;

                    //rootMotionDispLocation.Translation.X = rootMotionDispLocation.Translation.X % 1;

                    //rootMotionDispLocation.Translation.Z = rootMotionDispLocation.Translation.Z % 1;
                }
                else
                {
                    GFX.CurrentWorldView.RootMotionFollow_EnableWrap = false;
                }

                if (Graph.MainScreen.Config.CameraFollowsRootMotionY && !(NewIsComboActive))
                {
                    GFX.CurrentWorldView.RootMotionFollow_EnableWrap_Y = true;

                    //rootMotionDispLocation.Translation.X = rootMotionDispLocation.Translation.X % 1;

                    //rootMotionDispLocation.Translation.Z = rootMotionDispLocation.Translation.Z % 1;
                }
                else
                {
                    GFX.CurrentWorldView.RootMotionFollow_EnableWrap_Y = false;
                }


                //Vector3 globalTranslationOffset = (GFX.CurrentWorldView.RootMotionFollow_Translation_WrappedIfApplicable - GFX.CurrentWorldView.RootMotionFollow_Translation) * new Vector3(1, 0, 1);

                //if (Graph.MainScreen.Config.WrapRootMotion && !(CurrentComboIndex >= 0 && IsComboRecording))
                //{
                //    cameraTrans.X -= globalTranslationOffset.X;
                //    cameraTrans.Z -= globalTranslationOffset.Z;
                //    rootMotionDispLocation = absoluteRootMotionLocation;
                //}


                if (doDraw && Main.HelperDraw.EnableRootMotionStartTransform)
                {
                    DbgPrim_RootMotionStartPoint.OverrideColor = Main.Colors.ColorHelperRootMotionStartLocation;
                    DbgPrim_RootMotionStartPoint.Transform = new Transform(DBG.NewTransformSizeMatrix * DbgPrim_RootMotionStartPoint_Matrix);
                    DbgPrim_RootMotionStartPoint.Draw(true, null, Matrix.Identity);

                    DbgPrim_RootMotionStartPoint.OverrideColor = Main.Colors.ColorHelperRootMotionStartLocation_PrevLoop;
                    DbgPrim_RootMotionStartPoint.Transform = new Transform(DBG.NewTransformSizeMatrix * DbgPrim_RootMotionStartPoint_Matrix_PrevLoop);
                    DbgPrim_RootMotionStartPoint.Draw(true, null, Matrix.Identity);
                }

                

                DbgPrim_RootMotionPathLine.OverrideColor = Main.Colors.ColorHelperRootMotionTrail;

                if (doDraw && Main.HelperDraw.EnableRootMotionTrail)
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

                                var posDelta = nextPos - curPos;
                                var isCompletelyVertical = posDelta.X == 0 && posDelta.Z == 0;
                                var lineDirMatrix = isCompletelyVertical ? (Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(curPos)) : Matrix.CreateWorld(curPos, posDelta, Vector3.Up);
                                
                                
                                
                                DbgPrim_RootMotionPathLine.Transform = new Transform(Matrix.CreateScale(length) *
                                    Matrix.CreateRotationY(MathHelper.PiOver2) * lineDirMatrix);
                                DbgPrim_RootMotionPathLine.Draw(true, null, Matrix.Identity);
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
                                    
                                    var posDelta = nextPos - curPos;
                                    var isCompletelyVertical = posDelta.X == 0 && posDelta.Z == 0;
                                    var lineDirMatrix = isCompletelyVertical ? (Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(curPos)) : Matrix.CreateWorld(curPos, posDelta, Vector3.Up);

                                    
                                    DbgPrim_RootMotionPathLine.Transform = new Transform(Matrix.CreateScale(length) *
                                        Matrix.CreateRotationY(MathHelper.PiOver2) * lineDirMatrix);
                                    DbgPrim_RootMotionPathLine.Draw(true, null, Matrix.Identity);
                                }
                            }
                        }
                    }
                }

                if (doDraw)
                {
                    DbgPrim_RootMotionCurrentPoint.OverrideColor = Main.Colors.ColorHelperRootMotionCurrentLocation;
                    DbgPrim_RootMotionCurrentPoint.Transform = new Transform(DBG.NewTransformSizeMatrix * Matrix.CreateScale(0.25f) * absoluteRootMotionLocation.GetMatrix().ToXna());

                    if (Main.HelperDraw.EnableRootMotionTransform)
                        DbgPrim_RootMotionCurrentPoint.Draw(true, null, Matrix.Identity);
                }

                float lerpSMult = programDeltaTime / (1f / 60f);

                if (!doDraw)
                {
                    //var oldCamTrans = cameraTrans;

                    //if (Graph.MainScreen.Config.CameraFollowsRootMotionZX || Graph.MainScreen.Config.CameraFollowsRootMotionY)
                    //{
                        
                    //    Vector3 deltaToTarget = (rootMotionDispLocation.Translation - cameraTrans);
                    //    float distToTarget = deltaToTarget.Length();
                    //    //float test_CamSpeed = 12 * programDeltaTime; // 3 meters per second
                    //    float test_CamSpeed = distToTarget;// MathHelper.Lerp(3 * programDeltaTime, distToTarget, lerpSMult * (1 - Graph.MainScreen.Config.CameraFollowsRootMotionZX_Interpolation));
                    //    if (distToTarget > 0)
                    //    {
                    //        test_CamSpeed = Math.Min(test_CamSpeed, distToTarget);
                    //        cameraTrans += Vector3.Normalize(deltaToTarget) * test_CamSpeed;
                    //    }
                    //    //float lerpS = MathHelper.Clamp((1 - Graph.MainScreen.Config.CameraFollowsRootMotionZX_Interpolation) * lerpSMult, 0, 1);
                    //    //cameraTrans.X = MathHelper.LerpPrecise(cameraTrans.X, rootMotionDispLocation.Translation.X, MathHelper.Clamp((1 - Graph.MainScreen.Config.CameraFollowsRootMotionZX_Interpolation) * lerpSMult, 0, 1));
                    //    //cameraTrans.Z = MathHelper.LerpPrecise(cameraTrans.Z, rootMotionDispLocation.Translation.Z, MathHelper.Clamp((1 - Graph.MainScreen.Config.CameraFollowsRootMotionZX_Interpolation) * lerpSMult, 0, 1));

                    //    //cameraTrans.X = (float)(((1d - (double)lerpS) * (double)cameraTrans.X) + ((double)rootMotionDispLocation.Translation.X * (double)lerpS));
                    //    //cameraTrans.Z = (float)(((1d - (double)lerpS) * (double)cameraTrans.Z) + ((double)rootMotionDispLocation.Translation.Z * (double)lerpS));

                    //    //cameraTrans.X = rootMotionDispLocation.Translation.X;
                    //    //cameraTrans.Z = rootMotionDispLocation.Translation.Z;

                    //    if (!Graph.MainScreen.Config.CameraFollowsRootMotionZX)
                    //    {
                    //        cameraTrans.Z = oldCamTrans.Z;
                    //        cameraTrans.X = oldCamTrans.X;
                    //    }

                    //    if (!Graph.MainScreen.Config.CameraFollowsRootMotionY)
                    //    {
                    //        cameraTrans.Y = oldCamTrans.Y;
                    //    }
                    //}


                    //if (Graph.MainScreen.Config.CameraFollowsRootMotionRotation)
                    //{
                    //    GFX.CurrentWorldView.RootMotionFollow_Rotation = NewBlendableTransform.Lerp(NewBlendableTransform.FromRootMotionSample(new System.Numerics.Vector4(0, 0, 0, GFX.CurrentWorldView.RootMotionFollow_Rotation)), rootMotionDispLocation,
                    //        MathHelper.Clamp((1 - Graph.MainScreen.Config.CameraFollowsRootMotionRotation_Interpolation) * lerpSMult, 0, 1)).GetWrappedYawAngle();
                    //}
                    //else
                    //{
                    //    if (GFX.CurrentWorldView.RootMotionFollow_Rotation != 0)
                    //    {
                    //        GFX.CurrentWorldView.CameraLookDirection *= Quaternion.CreateFromYawPitchRoll(GFX.CurrentWorldView.RootMotionFollow_Rotation, 0, 0);
                    //    }

                    //    GFX.CurrentWorldView.RootMotionFollow_Rotation = 0;
                    //}

                    //if (Graph.MainScreen.Config.WrapRootMotion && !(CurrentComboIndex >= 0 && IsComboRecording))
                    //{
                    //    cameraTrans.X = cameraTrans.X % 1;
                    //    cameraTrans.Z = cameraTrans.Z % 1;
                    //}

                    //GFX.CurrentWorldView.RootMotionFollow_Translation = cameraTrans;

                    //if (Graph.MainScreen.Config.WrapRootMotion && !(CurrentComboIndex >= 0 && IsComboRecording))
                    //{
                    //    rootMotionDispLocation.Translation.X = rootMotionDispLocation.Translation.X % 1;
                    //    rootMotionDispLocation.Translation.Z = rootMotionDispLocation.Translation.Z % 1;
                    //}
                    //rootMotionDispLocation.Translation = absoluteRootMotionLocation.Translation;

                    //if (Graph.MainScreen.Config.WrapRootMotion && !(NewIsComboActive))
                    //{
                    //    //rootMotionDispLocation.Translation.X = rootMotionDispLocation.Translation.X % 1;
                    //    //rootMotionDispLocation.Translation.Z = rootMotionDispLocation.Translation.Z % 1;

                    //    //rootMotionDispLocation.Translation += ((GFX.CurrentWorldView.RootMotionFollow_Translation_WrappedIfApplicable - GFX.CurrentWorldView.RootMotionFollow_Translation) * new Vector3(1, 1, 1)).ToCS();
                    //    //rootMotionDispLocation.Translation.Y = absoluteRootMotionLocation.Translation.Y;
                    //}

                    //CurrentModel.StartTransform = CurrentModel.CurrentTransform = new Transform(CurrentModel.OriginOffsetMatrix * rootMotionDispLocation.GetMatrix().ToXna());

                    //GFX.World.UpdateDummyPolyFollowRefPoint(isFirstTime: false);
                    //GFX.CurrentWorldView.Update(0);
                }
            }
        }

        public void MarkRootMotionStart()
        {
            CreateRootMotionPoints();



            float floorDifference = 0;

            if (Main.Config.ResetFloorOnAnimStart)
            {
                lock (DBG._lock_NewGrid3D)
                {
                    if (DBG.NewGrid3D != null)
                    {
                        //var modelFollowPos = Vector3.Zero;

                        ParentDocument.Scene.AccessMainModel(model =>
                        {
                            //modelFollowPos = model.CurrentTransformPosition;
                            model.CurrentTransform = model.StartTransform = new Transform(model.CurrentTransform.WorldMatrix * Matrix.CreateTranslation(0, -model.CurrentTransformPosition.Y, 0));

                            model.AnimContainer.ResetRootMotionYOnly(out float yValue);
                            floorDifference = -yValue;

                            model.NewForceSyncUpdate();

                            ParentDocument.WorldViewManager.CurrentView.Update(0);
                        });

                        //DBG.NewGrid3D.WorldShiftOffset = new Vector3(0, modelFollowPos.Y, 0);
                  
                        DBG.NewGrid3D.ResetWorldShiftOffset();
                    }
                }
            }

            if (Main.Config.ResetHeadingOnAnimStart)
            {
                lock (DBG._lock_NewGrid3D)
                {
                    if (DBG.NewGrid3D != null)
                    {
                        //var modelFollowPos = Vector3.Zero;

                        ParentDocument.Scene.AccessMainModel(model =>
                        {
                            //modelFollowPos = model.CurrentTransformPosition;
                            var tr = model.CurrentTransform.WorldMatrix.ToNewBlendableTransform();
                            tr.Rotation = System.Numerics.Quaternion.Identity;
                            model.CurrentTransform = new Transform(tr.GetXnaMatrixFull());

                            model.AnimContainer.ResetRootMotionWOnly();
                        });

                        //DBG.NewGrid3D.WorldShiftOffset = new Vector3(0, modelFollowPos.Y, 0);


                    }
                }
            }

            if (floorDifference != 0)
            {
                var queuedTransforms = DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.ToList();
                DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.Clear();
                foreach (var tr in queuedTransforms)
                {
                    DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.Enqueue(
                        new Transform(tr.WorldMatrix * Matrix.CreateTranslation(0, floorDifference, 0)));
                }
                
                var queuedTransforms_PrevLoop = DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue.ToList();
                DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_PrevLoop.Clear();
                foreach (var tr in queuedTransforms_PrevLoop)
                {
                    DbgPrim_DbgPrim_RootMotionMidstPoint_TransformQueue_PrevLoop.Enqueue(
                        new Transform(tr.WorldMatrix * Matrix.CreateTranslation(0, floorDifference, 0)));
                }

                DbgPrim_RootMotionStartPoint_Matrix *= Matrix.CreateTranslation(0, floorDifference, 0);
                DbgPrim_RootMotionStartPoint_Matrix_PrevLoop *= Matrix.CreateTranslation(0, floorDifference, 0);
            }
            
            var curTransform = Matrix.CreateScale(0.5f) * (CurrentModel?.AnimContainer?.RootMotionTransform ?? NewBlendableTransform.Identity).GetMatrix().ToXna();
            
            if (DbgPrim_RootMotionStartPoint_Matrix != curTransform)
            {
                // Force update last frame.
                UpdateAndDrawRootMotionPoints(CurrentModel?.AnimContainer?.RootMotionTransform ?? NewBlendableTransform.Identity, false, 0);
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
                    //CurrentModel?.SetAnimStartTransform(curTransform);
                    //CurrentModel?.ResetAttackDistMeasureAccumulation();
                }

                CurrentModel?.SetAnimStartTransform(curTransform);
                CurrentModel?.ResetAttackDistMeasureAccumulation();
            }

            

            //if (Main.Config.ResetGridOriginAtAnimStart)
            //{

            //    Matrix worldShift = Matrix.Identity;

            //    lock (DBG._lock_NewGrid3D)
            //    {
            //        if (DBG.NewGrid3D != null)
            //        {
            //            DBG.NewGrid3D.OriginOffsetForWrap = Vector3.Zero;

            //            worldShift = Matrix.Invert(CurrentModel.CurrentTransform.WorldMatrix);

            //        }
            //    }

            //    GFX.CurrentWorldView.RegisterWorldShift(Vector3.Transform(Vector3.Zero, worldShift));

            //    var inverseModelRotationMatrix = Matrix.Invert(Matrix.CreateFromQuaternion(CurrentModel.CurrentTransform.WorldMatrix.ToNewBlendableTransform().Rotation));

            //    GFX.CurrentWorldView.RegisterWorldShift_Rotation(inverseModelRotationMatrix);

            //    //CurrentModel.CurrentTransform = CurrentModel.StartTransform = new Transform(Matrix.Identity);
            //}

            // Force update first frame.
            UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false, 0);
        }


        //public static float StatusTextScale = 100.0f;

        public Model CurrentModel => ParentDocument.Scene.MainModel;

        public void SetEntityType(TaeEntityType entityType)
        {
            if (ParentDocument.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR)
            {
                if (entityType == TaeEntityType.PC)
                    CurrentModel.EldenRingHandPoseAnimID = new SplitAnimID() { CategoryID = -1, SubID = 9 };
                else if (entityType == TaeEntityType.NPC)
                    CurrentModel.EldenRingHandPoseAnimID = new SplitAnimID() { CategoryID = -1, SubID = 43000 };
                else
                    CurrentModel.EldenRingHandPoseAnimID = SplitAnimID.Invalid;
            }
            else
            {
                CurrentModel.EldenRingHandPoseAnimID = SplitAnimID.Invalid;
            }

            CurrentModel.NewForceSyncUpdate();

            EntityType = entityType;

            if (entityType != TaeEntityType.NPC && CurrentModel != null)
            {
                lock (CurrentModel._lock_NpcParams)
                {
                    CurrentModel?.PossibleNpcParams?.Clear();
                    CurrentModel.SelectedNpcParamIndex = -1;
                }
            }

            if (entityType == TaeEntityType.PC)
            {
                OSD.WindowEquipment.IsOpen = true;
            }
            else if (entityType == TaeEntityType.NPC && ParentDocument.GameRoot.GameType is SoulsGames.AC6)
            {
                OSD.WindowEquipment.IsOpen = true;
            }
            
            // if (!(entityType == TaeEntityType.PC || entityType == TaeEntityType.REMO))
            // {
            //     OSD.WindowEquipment.IsOpen = false;
            // }
          
        }



        public void InitializeCharacterModel(Model mdl, bool isRemo, bool isActive = true)
        {
            lock (mdl)
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

                    if (!Graph.MainScreen.Config.ChrAsmConfigurations.ContainsKey(ParentDocument.GameRoot.GameType))
                    {
                        Graph.MainScreen.Config.ChrAsmConfigurations.Add
                            (ParentDocument.GameRoot.GameType, new NewChrAsmCfgJson());
                    }

                    Graph.MainScreen.Config.ChrAsmConfigurations[ParentDocument.GameRoot.GameType]
                        .WriteToChrAsm(mdl.ChrAsm);

                    mdl.ChrAsm.UpdateModels(isAsync: true, onCompleteAction: null, forceReloadUnchanged: false, disableCache: false);

                    Graph.MainScreen.HardReset();
                    NewScrub();

                    SetEntityType(isRemo ? TaeEntityType.REMO : TaeEntityType.PC);

                    if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                        GFX.CurrentWorldView.SetStartPositionForCharacterModel(height: 16, diameter: 10, posYOffset: -2.5f);
                    else
                        GFX.CurrentWorldView.SetStartPositionForCharacterModel(height: 2.25f, diameter: 1, posYOffset: -0.2f);

                    if (ParentDocument.GameRoot.GameType is SoulsGames.AC6)
                    {
                        mdl.ChrHitCapsuleHeight = 15f; //TODO: Get actual value
                        mdl.ChrHitCapsuleRadius = 04f; //TODO: Get actual value
                        mdl.ChrHitCapsuleYOffset = 0; //TODO: Get actual value
                    }
                    else
                    {
                        mdl.ChrHitCapsuleHeight = 1.5f;
                        mdl.ChrHitCapsuleRadius = 0.4f;
                        mdl.ChrHitCapsuleYOffset = 0;
                    }
                }
                else
                {
                    mdl.RescanNpcParams();

                    SetEntityType(isRemo ? TaeEntityType.REMO : TaeEntityType.NPC);

                    lock (mdl._lock_NpcParams)
                    {
                        if (mdl.NpcParam != null)
                        {
                            mdl.ChrHitCapsuleHeight = mdl.NpcParam.HitHeight;
                            mdl.ChrHitCapsuleRadius = mdl.NpcParam.HitRadius;
                            mdl.ChrHitCapsuleYOffset = mdl.NpcParam.HitYOffset;

                            GFX.CurrentWorldView.SetStartPositionForCharacterModel(mdl.NpcParam.HitHeight, mdl.NpcParam.HitRadius * 2, mdl.NpcParam.HitYOffset);
                        }
                        else
                        {
                            if (ParentDocument.GameRoot.GameType is SoulsGames.AC6)
                            {
                                mdl.ChrHitCapsuleHeight = 15f; //TODO: Get actual value
                                mdl.ChrHitCapsuleRadius = 04f; //TODO: Get actual value
                                mdl.ChrHitCapsuleYOffset = 0; //TODO: Get actual value
                            }
                            else
                            {
                                mdl.ChrHitCapsuleHeight = 1.5f;
                                mdl.ChrHitCapsuleRadius = 0.4f;
                                mdl.ChrHitCapsuleYOffset = 0;
                            }
                           
                        }

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


                

                //mdl.IsVisible = isActive;
            }
        }

        public TaeViewportInteractor(NewGraph graph)
        {
            
            RemoManager.NukeEntireRemoSystemAndGoBackToNormalDSAnimStudio();
            

            OSD.WindowEquipment.IsOpen = false;

            Graph = graph;
            //Graph.PlaybackCursor.PlaybackFrameChange += PlaybackCursor_PlaybackFrameChange;
            //Graph.PlaybackCursor.ScrubFrameChange += PlaybackCursor_ScrubFrameChange;

            //V2.0
            //NewAnimationContainer.AutoPlayAnimContainersUponLoading = false;

            ParentDocument.Scene.ClearScene();
            ParentDocument.TexturePool.Flush();

            var shortFileName = Utils.GetShortIngameFileName(Graph.MainScreen.NewFileContainerName).ToLower();

            var containerInfo = Graph.MainScreen.FileContainer.Info;

            var fileName = Graph.MainScreen.NewFileContainerName.ToLower();

            var modelFileName = Graph.MainScreen.NewFileContainerName_Model ?? Graph.MainScreen.NewFileContainerName;

            var shortFileName_Model = Utils.GetFileNameWithoutAnyExtensions(
                Utils.GetFileNameWithoutDirectoryOrExtension(modelFileName)).ToLower();

            var fileName_Model = modelFileName.ToLower();

            ParentDocument.LoadingTaskMan.DoLoadingTask("NewModelLoad", "Loading Model...", prog =>
            {
                IS_STILL_LOADING = true;
                try
                {
                    if (shortFileName.StartsWith("c"))
                    {
                        if (!Graph.MainScreen.SuppressNextModelOverridePrompt && ParentDocument.GameRoot.GameType is not SoulsAssetPipeline.SoulsGames.DES)
                        {
                            
                            var newChrbndName = ParentDocument.GameData.ShowPickInsideBndPath("/chr/", @".*\/c\d\d\d\d.chrbnd.dcx$", $@"/chr/{shortFileName.Substring(0, 4)}",
                                $"Choose Character Model for '{shortFileName}.anibnd.dcx'", $@"/chr/{shortFileName}.chrbnd.dcx");
                            if (newChrbndName != null)
                            {
                                modelFileName = fileName_Model = newChrbndName;
                                shortFileName_Model = Utils.GetFileNameWithoutAnyExtensions(
                                    Utils.GetFileNameWithoutDirectoryOrExtension(modelFileName)).ToLower();
                            }
                            else
                            {
                                System.Windows.Forms.MessageBox.Show("Unable to find any characters in game data. " +
                                    "Make sure your directories are setup properly on the 'Setup Project Directories' menu.", "Error", System.Windows.Forms.MessageBoxButtons.OK,
                                    System.Windows.Forms.MessageBoxIcon.Error);

                                Main.REQUEST_REINIT_EDITOR = true;
                                return;
                            }

                        }
                        else
                        {
                            Graph.MainScreen.SuppressNextModelOverridePrompt = false;
                        }


                        var loadedModels = ParentDocument.GameRoot.LoadCharacter(shortFileName.Substring(0, 5), shortFileName_Model.Substring(0, 5));

                        var models = ParentDocument.Scene.Models.ToList();
                        foreach (var m in models)
                        {
                            if (m != null)
                                InitializeCharacterModel(m, isRemo: false, isActive: ParentDocument.Scene.MainModel == m);
                        }

                        if (loadedModels.Length == 0 || !loadedModels.Any(x => x != null))
                        {
                            ParentDocument.Scene.SetMainModelAsDummy();
                            return;
                        }

                        CurrentModel?.NewForceSyncUpdate();

                        GFX.CurrentWorldView.NewDoRecenterAction = () =>
                        {
                            GFX.CurrentWorldView.CameraLookDirection = Quaternion.Identity;
                        };

                        LoadFmodSoundsForCurrentModel();




                        ParentDocument.GameRoot.LoadSystex();
                    }
                    else if (shortFileName.StartsWith("o"))
                    {
                        

                        //GameRoot.LoadObject(shortFileName);

                        //GFX.CurrentWorldView.NewDoRecenterAction = () =>
                        //{
                        //    GFX.CurrentWorldView.CameraLookDirection = Quaternion.Identity;
                        //};
                        //GFX.CurrentWorldView.NewRecenter();

                        //LoadSoundsForCurrentModel();

                        //throw new NotImplementedException("OBJECTS NOT SUPPORTED YET");

                        if (containerInfo is DSAProj.TaeContainerInfo.ContainerAnibndInBinder asAnibndInBinder)
                        {
                            var model = ParentDocument.GameRoot.NewLoadObj(shortFileName, asAnibndInBinder.BindID - 400);
                            //InitializeCharacterModel(model, isRemo: false, isActive: true);
                            ////var models = Scene.Models.ToList();
                            //foreach (var m in models)
                            //{
                            //    if (m != null)
                            //        InitializeCharacterModel(m, isRemo: false, isActive: Scene.MainModel == m);
                            //}

                            if (model == null)
                            {
                                ParentDocument.Scene.SetMainModelAsDummy();
                                return;
                            }

                            SetEntityType(TaeEntityType.OBJ);

                            CurrentModel.NewForceSyncUpdate();

                            GFX.CurrentWorldView.NewDoRecenterAction = () =>
                            {
                                GFX.CurrentWorldView.CameraLookDirection = Quaternion.Identity;
                            };

                            LoadFmodSoundsForCurrentModel();

                            ParentDocument.GameRoot.LoadSystex();
                        }


                    }
                    else if (fileName.EndsWith(".partsbnd") || fileName.EndsWith(".partsbnd.dcx"))
                    {
                        

                        //throw new NotImplementedException("PARTS NOT SUPPORTED YET");
                        if (containerInfo is DSAProj.TaeContainerInfo.ContainerAnibndInBinder asAnibndInBinder)
                        {
                            var model = ParentDocument.GameRoot.NewLoadParts(shortFileName, asAnibndInBinder.BindID - 400);
                            //InitializeCharacterModel(model, isRemo: false, isActive: true);
                            ////var models = Scene.Models.ToList();
                            //foreach (var m in models)
                            //{
                            //    if (m != null)
                            //        InitializeCharacterModel(m, isRemo: false, isActive: Scene.MainModel == m);
                            //}
                            if (model == null)
                            {
                                ParentDocument.Scene.SetMainModelAsDummy();
                                return;
                            }

                            SetEntityType(TaeEntityType.PARTS);

                            CurrentModel.NewForceSyncUpdate();

                            GFX.CurrentWorldView.NewDoRecenterAction = () =>
                            {
                                GFX.CurrentWorldView.CameraLookDirection = Quaternion.Identity;
                            };

                            LoadFmodSoundsForCurrentModel();

                            ParentDocument.GameRoot.LoadSystex();
                        }
                    }
                    else if (fileName.EndsWith(".remobnd") || fileName.EndsWith(".remobnd.dcx"))
                    {
                        SetEntityType(TaeEntityType.REMO);

                        ParentDocument.Scene.DisableModelDrawing();

                        RemoManager.ViewportInteractor = this;

                        RemoManager.DisposeAllModels();
                        RemoManager.RemoName = Utils.GetShortIngameFileName(Graph.MainScreen.NewFileContainerName);

                        var fmod = ParentDocument.Fmod;

                        fmod.Purge();
                        if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                        ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                        {
                            fmod.LoadInterrootFEV("main");
                            var dlc = fmod.GetFevPathFromInterroot("main", isDs1Dlc: true);
                            if (System.IO.File.Exists(dlc))
                                fmod.LoadFEV(dlc);

                            fmod.LoadInterrootFEV("smain");
                            dlc = fmod.GetFevPathFromInterroot("smain", isDs1Dlc: true);
                            if (System.IO.File.Exists(dlc))
                                fmod.LoadFEV(dlc);

                            fmod.LoadInterrootFEV($"m{RemoManager.AreaInt:D2}");
                            dlc = fmod.GetFevPathFromInterroot($"m{RemoManager.AreaInt:D2}", isDs1Dlc: true);
                            if (System.IO.File.Exists(dlc))
                                fmod.LoadFEV(dlc);

                            fmod.LoadInterrootFEV($"sm{RemoManager.AreaInt:D2}");
                            dlc = fmod.GetFevPathFromInterroot($"sm{RemoManager.AreaInt:D2}", isDs1Dlc: true);
                            if (System.IO.File.Exists(dlc))
                                fmod.LoadFEV(dlc);

                            fmod.LoadInterrootFEV($"p{RemoManager.RemoName.Substring(3)}");
                            dlc = fmod.GetFevPathFromInterroot($"p{RemoManager.RemoName.Substring(3)}", isDs1Dlc: true);
                            if (System.IO.File.Exists(dlc))
                                fmod.LoadFEV(dlc);
                        }

                        RemoManager.LoadRemoDict(Graph.MainScreen.FileContainer);

                        if (ParentDocument.Scene.Models.Count == 0)
                            ParentDocument.GameRoot.LoadCharacter("c0000", "c0000");

                        lock (ParentDocument.Scene._lock_ModelLoad_Draw)
                        {
                            ParentDocument.Scene.Models = ParentDocument.Scene.Models.OrderBy(m => m.IS_PLAYER ? 0 : 1).ToList();
                        }

                        ParentDocument.GameRoot.LoadSystex();

                        //throw new NotImplementedException("REMO NOT SUPPORTED YET");
                    }



                    InitializeForCurrentModel();

                    ParentDocument.Scene.EnableModelDrawing();
                    if (ParentDocument.Scene.IsEmpty)
                        ParentDocument.Scene.SetMainModelAsDummy();

                    if (!CurrentModel.IS_PLAYER)
                        ParentDocument.Scene.EnableModelDrawing2();

                    //if (ParentDocument.GameRoot.GameTypeUsesWwise)
                    //{
                    //    Wwise.InitLookupBanks();
                    //}
                    
                    Graph?.MainScreen?.SelectNewAnimRef(Graph.MainScreen.SelectedAnimCategory, Graph.MainScreen.SelectedAnim);
                }
                finally
                {
                    Main.MainThreadLazyDispatch(() =>
                    {
                        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                        //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: false, compacting: true);
                        //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: false);
                        GC.Collect();
                    }, waitForAllLoadingTasks: true);
                    IS_STILL_LOADING = false;
                }
            });
        }

        public void LoadFmodSoundsForCurrentModel(bool fresh = true)
        {
            var document = Graph.MainScreen.ParentDocument;

            document.SoundManager.PurgeLoadedAssets();
            document.SoundManager.SetEngineToCurrentGame(document.GameRoot.GameType);
            document.SoundManager.ClearBankLists(masterList: false, lookupList: true);
            document.SoundManager.CopySoundBanksFromProjToThis();

            //if (document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.FMOD)
            //{
            //    if (!document.Fmod.IsFevLoaded(CurrentModel?.Name) || !document.Fmod.AreMainFevsLoaded())
            //    {
            //        document.Fmod.LoadMainFEVs();
            //        document.Fmod.LoadInterrootFEV(CurrentModel?.Name);
            //    }
            //}
            //else if (document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.Wwise)
            //{
            //    var soundBankNames = document.SoundManager.GetAdditionalSoundBankNames();
            //    Wwise.PurgeLoadedAssets(document.SoundManager);
            //    Wwise.AddLookupBanks(soundBankNames);
            //}
            //else if (document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.MagicOrchestra)
            //{
            //    var soundBankNames = document.SoundManager.GetAdditionalSoundBankNames();
            //    MagicOrchestra.PurgeLoadedAssets(document.SoundManager);
            //    MagicOrchestra.AddLookupBanks(soundBankNames);
            //}

            ////if (ParentDocument.GameRoot.GameTypeUsesWwise)
            ////{
            ////    Wwise.InitLookupBanks();
            ////}
        }

        public void InitializeForCurrentModel()
        {
            if (CurrentModel != null)
            {
                // if (CurrentModel?.AnimContainer.Skeleton != null)
                //     CurrentComboRecorder = new HavokRecorder(CurrentModel.AnimContainer.Skeleton.Bones);

                //GFX.CurrentWorldView.OrbitCamDistanceInput = (CurrentModel.Bounds.Max - CurrentModel.Bounds.Min).Length() * 4f;
                //if (GFX.CurrentWorldView.OrbitCamDistanceInput < 0.5f)
                //    GFX.CurrentWorldView.OrbitCamDistanceInput = 5;

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

                OSD.WindowEntity.IsOpen = true;
            }

        }

        private void ChrAsm_EquipmentModelsUpdated(object sender, EventArgs e)
        {
            if (Graph == null || Graph.PlaybackCursor == null || Graph.MainScreen.PlaybackCursor == null)
                return;

            Graph.MainScreen.SelectNewAnimRef(Graph.MainScreen.SelectedAnimCategory, Graph.MainScreen.SelectedAnim);

            //V2.0: Scrub weapon anims to the current frame.

            if (CurrentModel.ChrAsm != null)
            {
                CurrentModel.ChrAsm.UpdateWeaponTransforms(0);
                CurrentModel.ChrAsm.UpdateEquipmentAnimation(0, forceSyncUpdate: true);
            }


            //V2.0: Update stuff probably
            CurrentModel.NewForceSyncUpdate();
        }

        //private void EquipForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        public void SaveChrAsm()
        {
            if (CurrentModel?.ChrAsm != null)
            {
                if (!Graph.MainScreen.Config.ChrAsmConfigurations.ContainsKey(ParentDocument.GameRoot.GameType))
                {
                    Graph.MainScreen.Config.ChrAsmConfigurations.Add
                        (ParentDocument.GameRoot.GameType, new NewChrAsmCfgJson());
                }

                Graph.MainScreen.Config.ChrAsmConfigurations[ParentDocument.GameRoot.GameType].CopyFromChrAsm(CurrentModel.ChrAsm);
            }
        }

        // private void PlaybackCursor_PlaybackFrameChange(object sender, EventArgs e)
        // {
        //     // if (Graph.PlaybackCursor.Scrubbing)
        //     //     return;
        //     
        //     Graph.PlaybackCursor.HkxAnimationLength = CurrentModel?.AnimContainer?.CurrentAnimDuration;
        //     Graph.PlaybackCursor.SnapInterval = CurrentModel?.AnimContainer?.CurrentAnimFrameDuration;
        //
        //
        //     var timeDelta = (float)(Graph.PlaybackCursor.GUICurrentTime - Graph.PlaybackCursor.OldGUICurrentTime);
        //     if (EntityType == TaeEntityType.REMO)
        //     {
        //         bool remoCutAdv = RemoManager.UpdateCutAdvance();
        //         if (remoCutAdv)
        //             return;
        //     }
        //
        //     //V2.0
        //     //CurrentModel.AnimContainer.IsPlaying = false;
        //     CurrentModel.AnimContainer.EnableLooping = Main.Config.LoopEnabled;
        //     CurrentModel.ScrubAnim(absolute: true, (float)(Graph.PlaybackCursor.GUICurrentTime), true, true);
        //     // Update root motion before world for camera follow root motion mode
        //     UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false, timeDelta);
        //     GFX.CurrentWorldView.Update(0);
        //     CurrentModel.AfterAnimUpdate(timeDelta);
        //
        //     //V2.0
        //     //CurrentModel.ChrAsm?.UpdateWeaponTransforms(timeDelta);
        //
        //     CheckSimEnvironment();
        //
        //     ActionSim.IsRemoModeEnabled = (EntityType == TaeEntityType.REMO);
        //     ActionSim.GraphForRemoSpecifically = Graph;
        //
        //
        //     try
        //     {
        //         var pb = Graph.PlaybackCursor;
        //         ActionSim.OnSimulationFrameChange(Graph, Graph.MainScreen.Proj.GetAnimation, Graph.MainScreen.SelectedAnim.NewID, pb.JustStartedPlaying && (pb.Scrubbing || pb.IsPlaying));
        //         //var childGraphs = Graph.GetChildGraphs();
        //         //foreach (var graph in childGraphs)
        //         //{
        //         //    graph.PlaybackCursor = Graph.PlaybackCursor;
        //         //    graph.EventSim?.OnSimulationFrameChange(graph.AnimRef.Events.OrderBy(x => x.StartTime).ToList(), (float)pb.CurrentTimeMod, (float)pb.OldCurrentTimeMod, pb.JustStartedPlaying && (pb.Scrubbing || pb.IsPlaying), pb.CurrentLoopCountDelta, (float)pb.MaxTime, timeDelta);
        //         //}
        //     }
        //     catch
        //     {
        //
        //     }
        //
        //     
        //     Graph.PlaybackCursor.ModPlaybackSpeed_GrabityRate = ActionSim.ModPlaybackSpeed_GrabityRate;
        //     Graph.PlaybackCursor.ModPlaybackSpeed_Event603 = ActionSim.ModPlaybackSpeed_Event603;
        //     Graph.PlaybackCursor.ModPlaybackSpeed_Event608 = ActionSim.ModPlaybackSpeed_Event608;
        //     Graph.PlaybackCursor.ModPlaybackSpeed_NightfallEvent7032 = ActionSim.ModPlaybackSpeed_NightfallEvent7032;
        //     Graph.PlaybackCursor.ModPlaybackSpeed_AC6Event9700 = ActionSim.ModPlaybackSpeed_AC6Event9700;
        //
        //
        //     if (EntityType == TaeEntityType.REMO)
        //     {
        //         RemoManager.UpdateRemoTime((float)Graph.PlaybackCursor.GUICurrentTimeMod);
        //     }
        //
        //     GFX.CurrentWorldView.Update(0);
        //
        //     
        // }

        private void CheckSimEnvironment()
        {
            if (ActionSim == null || ActionSim.MODEL != CurrentModel)
            {
                ActionSim = new TaeActionSimulationEnvironment(ParentDocument, CurrentModel);
                Graph.ActionSim = ActionSim;
            }
        }

        //public void NewScrubKeepRootMotion(bool absolute = false, float time = 0, bool foreground = true, bool background = true, bool forceRefreshTimeact = false)
        //{
        //    var prevRootMotion = CurrentModel.AnimContainer.RootMotionTransformVec;
        //    NewScrub(absolute, time, foreground, background, forceRefreshTimeact);
        //    CurrentModel.AnimContainer.AddRelativeRootMotionRotation
        //}


        public void NewScrub(bool absolute = false, float time = 0, bool foreground = true, 
            bool background = true, bool forceRefreshTimeact = false, bool ignoreRootMotion = false)
        {
            Graph.PlaybackCursor.HkxAnimationLength = CurrentModel?.AnimContainer?.CurrentAnimDuration;
            Graph.PlaybackCursor.SnapInterval = CurrentModel?.AnimContainer?.CurrentAnimFrameDuration;

            

            //var timeDelta = forceCustomTimeDelta ?? (float)(Graph.PlaybackCursor.GUICurrentTime - Graph.PlaybackCursor.OldGUICurrentTime);

            //TODO: Check if this is going to deadlock
          
            try
            {
                //V2.0
                //CurrentModel.AnimContainer.IsPlaying = false;
                //CurrentModel.AnimContainer.Proj = Graph.MainScreen.Proj;
                if (!NewIsComboActive)
                    CurrentModel.AnimContainer.EnableLooping = Main.Config.LoopEnabled;
                //CurrentModel.ScrubAnim(absolute, time, foreground, background, out float timeDelta);

                

                var pb = Graph.PlaybackCursor;

                //TODO: Check if putting this inside the lock() fixes.
                CheckSimEnvironment();

                if (!ParentDocument.LoadingTaskMan.AnyInteractionBlockingTasks() || forceRefreshTimeact)
                {
                    
                    // Uses "GUI" times in playback cursor specifically since it's for scrubbing, which might have snap to frames enabled.
                    ActionSim.OnSimulationFrameChange(Graph, Graph.MainScreen.Proj.SAFE_GetFirstAnimationFromFullID, Graph.MainScreen.SelectedAnim.SplitID, pb.JustStartedPlaying);
                    //var childGraphs = Graph.GetChildGraphs();
                    //foreach (var graph in childGraphs)
                    //{
                    //    graph.PlaybackCursor = Graph.PlaybackCursor;
                    //    graph.EventSim.OnSimulationFrameChange(graph.AnimRef.Events.OrderBy(x => x.StartTime).ToList(), (float)pb.GUICurrentTimeMod, (float)pb.OldGUICurrentTimeMod, pb.JustStartedPlaying && (pb.Scrubbing || pb.IsPlaying), pb.CurrentLoopCountDelta, (float)pb.MaxTime, timeDelta);
                    //}
                }

                CurrentModel.NewScrubSimTime(absolute, time, true, true, out float timeDelta, 
                    baseSlotOnly: absolute && !NewAnimationContainer.GLOBAL_SYNC_FORCE_REFRESH, 
                    forceSyncUpdate: NewAnimationContainer.GLOBAL_SYNC_FORCE_REFRESH, ignoreRootMotion);

                UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false, timeDelta);

                if (!ignoreRootMotion)
                {
                    

                    if (Graph.PlaybackCursor.CurrentLoopCountDelta != 0 && timeDelta != 0)
                        MarkRootMotionStart();
                }

                //CurrentModel?.NewUpdateByAnimTick(timeDelta, forceSyncUpdate: false);


               

                ActionSim.IsRemoModeEnabled = (EntityType == TaeEntityType.REMO);
                ActionSim.GraphForRemoSpecifically = Graph;

               

                Graph.PlaybackCursor.ModPlaybackSpeed_GrabityRate = ActionSim.ModPlaybackSpeed_GrabityRate;
                Graph.PlaybackCursor.ModPlaybackSpeed_Event603 = ActionSim.ModPlaybackSpeed_Event603;
                Graph.PlaybackCursor.ModPlaybackSpeed_Event608 = ActionSim.ModPlaybackSpeed_Event608;
                Graph.PlaybackCursor.ModPlaybackSpeed_NightfallEvent7032 = ActionSim.ModPlaybackSpeed_NightfallEvent7032;
                Graph.PlaybackCursor.ModPlaybackSpeed_AC6Event9700 = ActionSim.ModPlaybackSpeed_AC6Event9700;
            }
            catch(Exception ex) when (Main.EnableErrorHandler.ActionSimUpdate_Inner)
            {
                if (Main.IsDebugBuild)
                    Console.WriteLine("test");
            }
            
            //V2.0
            //CurrentModel.ChrAsm?.UpdateWeaponTransforms(timeDelta);



            GFX.CurrentWorldView.Update(0);

            if (EntityType == TaeEntityType.REMO)
            {
                RemoManager.UpdateRemoTime((float)Graph.PlaybackCursor.GUICurrentTimeMod);
            }
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
        //                weaponModel.AnimContainer?.ClearAnimation();
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
            CurrentModel?.AnimContainer?.ResetRootMotion();
            MarkRootMotionStart();
        }

        public void RemoveTransition()
        {
            CurrentModel.AnimContainer.NewSetBlendDurationOfSlot(NewAnimSlot.SlotTypes.Base, SplitAnimID.Invalid, -1, false);
            CurrentModel.AnimContainer.RemoveTransition();
        }
        

        public void RootMotionSendHome()
        {
            if (CurrentModel != null)
            {
                //CurrentModel.AnimContainer.ResetRootMotion();
                CurrentModel.AnimContainer.ResetRootMotion();
                //CurrentModel.AnimContainer.CurrentRootMotionDirection = 0;

                if (CurrentModel.ChrAsm != null)
                {
                    foreach (var slot in CurrentModel.ChrAsm.WeaponSlots)
                    {
                        slot.AccessAllModels(model =>
                        {
                            model.AnimContainer?.ResetRootMotion();
                        });
                    }
                }
            }
        }
        

        public void OnNewAnimSelected(float startTime, bool disableHkxSelect, DSAProj.Animation animRef)
        {
            if (EntityType == TaeEntityType.REMO)
            {
                RemoManager.LoadRemoCut($"a{((long)Graph.MainScreen.SelectedAnim.SplitID.SubID):D4}.hkx");
                //RemoManager.AnimContainer.ScrubRelative(0, ScrubTypes.All);
                GFX.CurrentWorldView.Update(0);
            }
            else if (CurrentModel != null)
            {
                //var mainChrAnimName = Graph.MainScreen.SelectedAnim.GetHkxID(Graph.MainScreen.Proj);

                //if (Main.Config.ResetFloorOnAnimStart)
                //{
                //    lock (DBG._lock_NewGrid3D)
                //    {
                //        if (DBG.NewGrid3D != null)
                //        {
                //            var modelFollowPos = Vector3.Zero;

                //            ParentDocument.Scene.AccessMainModel(model =>
                //            {
                //                modelFollowPos = model.CurrentTransformPosition;
                //            });

                //            DBG.NewGrid3D.WorldShiftOffset = new Vector3(0, -modelFollowPos.Y, 0);

                //        }
                //    }
                //}


                //V2.0: See if this needs something similar in the new system.
                //CurrentModel.AnimContainer.StoreRootMotionRotation();

                //CurrentModel.AnimContainer.CurrentAnimation.RotMatrixAtStartOfAnim



                if (!disableHkxSelect)
                {
                    NewAnimationContainer.GLOBAL_SYNC_FORCE_REFRESH = true;
                    try
                    {
                        CurrentModel.AnimContainer.RequestAnim(NewAnimSlot.SlotTypes.Base, Graph.MainScreen.SelectedAnim.SplitID, forceNew: true, animWeight: 1, startTime: 0, blendDuration: animRef?.SAFE_GetBlendDuration() ?? 0);
                        CurrentModel.NewScrubSimTime(absolute: false, time: 0, foreground: true, background: true, out _, forceSyncUpdate: true);
                    }
                    finally
                    {
                        NewAnimationContainer.GLOBAL_SYNC_FORCE_REFRESH = false;
                    }
                    
                    
                }
                
                

                //CurrentModel.ChrAsm?.SelectWeaponAnimations(mainChrAnimName)
                var asm = CurrentModel.ChrAsm;
                if (asm != null)
                {
                    asm.SelectPartsAnimations(Graph.MainScreen.SelectedAnim.SplitID);
                }

                var ac6Parts = CurrentModel.AC6NpcParts;
                if (ac6Parts != null)
                {
                    ac6Parts.SelectAnimation(Graph.MainScreen.SelectedAnim.SplitID);
                }


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
                Graph.PlaybackCursor.CurrentTime = startTime;
                Graph.PlaybackCursor.StartTime = startTime;
                Graph.PlaybackCursor.IgnoreCurrentRelativeScrub();



                CheckChrAsmWeapons();

                //FindWeaponAnim(mainChrSolver, lastRightWeaponModelTAEWasReadFrom, lastRightWeaponTAE);
                //FindWeaponAnim(mainChrSolver, lastLeftWeaponModelTAEWasReadFrom, lastLeftWeaponTAE);

                CheckSimEnvironment();

                lock (Graph._lock_ActionBoxManagement)
                {
                    ActionSim.OnNewAnimSelected(Graph.GetActionListCopy_UsesLock());
                    //Graph.PlaybackCursor.Update(Graph.EventBoxes, ignoreDeltaTime: true);
                    //EventSim.OnSimulationFrameChange(Graph.EventBoxes, 0);
                }


                //V2.0: Check
                //CurrentModel.AnimContainer.CurrentAnimation.Reset(CurrentModel.AnimContainer.RootMotionTransform.GetRootMotionVector4());

                //CurrentModel.AnimContainer.CurrentAnimation.SyncRootMotion(CurrentModel.AnimContainer.RootMotionTransform.GetRootMotionVector4());

                UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false);

                if (CurrentModel.AnimContainer.CurrentAnimation != null)
                {
                    NewScrub();
                }
                else
                {
                    CurrentModel.SkeletonFlver?.RevertToReferencePose();
                    CurrentModel.AnimContainer.CompletelyClearAllSlots();
                }

                CurrentModel.NewForceSyncUpdate();

                MarkRootMotionStart();

                //CurrentModel.NewUpdate(0);

                UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false);


                //if (CurrentModel.AnimContainer.CurrentAnimation != null)
                //{
                //    //V2.0: Check
                //    //CurrentModel.AnimContainer.CurrentAnimation.Reset(CurrentModel.AnimContainer.RootMotionTransform.GetRootMotionVector4());

                //    //CurrentModel.AnimContainer.CurrentAnimation.SyncRootMotion(CurrentModel.AnimContainer.RootMotionTransform.GetRootMotionVector4());

                //    UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false);

                //    if (CurrentModel.AnimContainer.CurrentAnimation != null)
                //        NewScrub();
                //    else
                //        CurrentModel.SkeletonFlver?.RevertToReferencePose();

                //    CurrentModel.NewForceSyncUpdate();

                //    MarkRootMotionStart();

                //    //CurrentModel.NewUpdate(0);

                //    UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false);
                //}
                //else
                //{
                //    CurrentModel.SkeletonFlver?.RevertToReferencePose();
                //}

                //if (asm != null)
                //{
                //    asm.Update(0);
                //}
            }

            NewScrub();
            ParentDocument.WorldViewManager.CurrentView.Update(0);
        }

        public bool IsAnimLoaded(SplitAnimID name)
        {
            return CurrentModel?.AnimContainer?.IsAnimLoaded(name) ?? false;
           
        }

        float modelDirectionLastFrame = 0;

        public void GeneralUpdate_BeforePrimsDraw()
        {
            if (CurrentModel != null)
            {
                //int frame = (int)Math.Round(Graph.PlaybackCursor.CurrentTime / (1.0 / 60.0));
            }
            else
            {
                GFX.CurrentWorldView.RootMotionFollow_Translation = Vector3.Zero;
                GFX.CurrentWorldView.RootMotionFollow_Rotation = 0;
            }
        }

        int prevRootMotionLoopCount = -1;

        public void GeneralUpdate(float timeDelta, bool allowPlaybackManipulation = true)
        {
            

            
            if (CurrentModel != null)
            {
                if (ActionSim.GhettoFix_DisableSimulationTemporarily > 0)
                {
                    ActionSim.GhettoFix_DisableSimulationTemporarily--;
                }

                // allowPlaybackManipulation==false prevents infinite recursion.
                // if (allowPlaybackManipulation && IsComboRecording && CurrentComboIndex >= 0)
                // {
                //     Graph.PlaybackCursor.IsPlaying = false;
                //     Graph.PlaybackCursor.Scrubbing = true;
                //     Graph.PlaybackCursor.IsStepping = false;
                //     Graph.PlaybackCursor.CurrentTime += CurrentComboRecorder.DeltaTime;
                //     Graph.PlaybackCursor.UpdateScrubbing();
                //     RecordCurrentComboFrame();
                // }
    
                CurrentModel?.NewUpdateByFixedProgramTick(timeDelta);
                //CurrentModel?.AnimContainer?.Scrub(false, 0, false, false, out _, false);
                UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, false, 0, timeDelta);
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
            if (Combo != null)
                Combo.PlaybackSpeed = Graph?.PlaybackCursor?.EffectivePlaybackSpeed ?? 1;
            Combo?.Update(timeDelta);
        }

        public void DrawDebug()
        {
            if (CurrentModel != null)
            {
                //EventSim?.DrawAllBladeSFXs(CurrentModel.CurrentTransform.WorldMatrix);

                UpdateAndDrawRootMotionPoints(CurrentModel.AnimContainer.RootMotionTransform, true);
            }

            
        }
        
        
        private StatusPrinter BuildStatus(TaeConfigFile.ViewportStatusTypes type)
        {
            

            var printer = new StatusPrinter(Vector2.One * 4, Main.Colors.GuiColorViewportStatus);

            if (IS_STILL_LOADING)
                return printer;

            if (CurrentModel == null)
                return printer;

            string getAnimSizeString(string prefix, long animSize)
            {
                if (animSize < 0)
                    return "";

                const double MEM_KB = 1024f;
                const double MEM_MB = 1024f * 1024f;
                //const double MEM_GB = 1024f * 1024f * 1024f;

                if (animSize < MEM_KB)
                    return $" [{prefix}{(1.0 * animSize):0} B]";
                else if (animSize < MEM_MB)
                    return $" [{prefix}{(1.0 * animSize / MEM_KB):0.000} KB]";
                else// if (MemoryUsage < MEM_GB)
                    return $" [{prefix}{(1.0 * animSize / MEM_MB):0.000} MB]";
                //else
                //    return $"{prefix}{(1.0 * MemoryUsage / MEM_GB):0.00} GB";
            }

            var animDebugReport = CurrentModel.AnimContainer.GetAllSlotsDebugReports();

            if (type is TaeConfigFile.ViewportStatusTypes.Condensed)
            {
                
                var firstAnim = CurrentModel.AnimContainer.CurrentAnimation;
                if (firstAnim != null)
                {
                    var debugReportEntries = animDebugReport[NewAnimSlot.SlotTypes.Base].AnimEntries;
                    if (debugReportEntries.Count > 0)
                    {
                        printer.AppendLine($"Anim: <{debugReportEntries[0].ID.GetFormattedIDString(ParentDocument.GameRoot)}>{debugReportEntries[0].Name}");
                    }
                }

                foreach (var kvp in animDebugReport)
                {
                    if (kvp.Key == NewAnimSlot.SlotTypes.Base)
                        continue;
                        
                    var debugReportForThisSlot =
                        kvp.Value.AnimEntries.FirstOrDefault();
                    if (debugReportForThisSlot != null)
                    {
                        if (debugReportForThisSlot.Weight > 0)
                            printer.AppendLine($"     +<{debugReportForThisSlot.ID.GetFormattedIDString(ParentDocument.GameRoot)}>{debugReportForThisSlot.Name}[{debugReportForThisSlot.Weight:0.00}]");
                    }
                }

                if (CurrentModel?.ChrAsm != null)
                {
                    foreach (var slot in CurrentModel.ChrAsm.WeaponSlots)
                    {
                        if (slot.EquipParam != null)
                        {
                            if (slot.EquipSlotType is NewChrAsm.EquipSlotTypes.SekiroGrapplingHook
                                or NewChrAsm.EquipSlotTypes.SekiroMortalBlade)
                                continue;
                            var atk = slot.EquipParam.WepMotionCategory;
                            var spAtk = slot.EquipParam.SpAtkCategory;
                            printer.AppendLine($"{slot.SlotDisplayNameShort}: {slot.EquipParam.GetPartBndName_Short(slot.EquipSlotType)}[a{atk:D2}{(spAtk > 0 ? $", a{spAtk:D2}" : "")}]");
                        }
                    }
                }

                return printer;
            }

            if (Graph?.PlaybackCursor.IsPlayingRemoFullPreview == true)
            {
                printer.AppendLine("[CUTSCENE PLAY MODE]", Color.Cyan);
                return printer;
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
                bool exceedsBoneCount = false;
                if (CurrentModel.USE_GLOBAL_BONE_MATRIX)
                {
                    if (CurrentModel.SkeletonFlver.Bones.Count > FlverShader.BoneMatrixSize)
                    {
                        printer.AppendLine($"Warning: Model '{CurrentModel.Name}' exceeds max per-submesh bone count of {FlverShader.BoneMatrixSize}.", Main.Colors.GuiColorViewportStatusMaxBoneCountExceeded);
                    }
                }
                else
                {
                    if (CurrentModel.ExceedsBoneCount)
                    {
                        printer.AppendLine($"Warning: Model '{CurrentModel.Name}' exceeds max per-FLVER bone count of {FlverShader.BoneMatrixSize}.", Main.Colors.GuiColorViewportStatusMaxBoneCountExceeded);
                    }
                }

                printer.AppendLine($"[ANIMATION SLOTS:]");
                foreach (var kvp in animDebugReport)
                {
                    // if (kvp.Key == NewAnimSlot.SlotTypes.Base)
                    //     continue;
                    //     
                    var dr =
                        kvp.Value.AnimEntries.FirstOrDefault();
                    if (dr != null)
                    {
                        printer.AppendLine($"  {kvp.Key}:");
                        printer.AppendLine($"    [x{dr.Weight:0.00}] [x{dr.Time:00.000}|{dr.PrevTime:00.000}/{dr.Duration:00.000}] [{dr.LoopCount}|{dr.PrevLoopCount} Loops] <{dr.ID.GetFormattedIDString(ParentDocument.GameRoot)}>{dr.Name}{getAnimSizeString("", dr.AnimFileSize)}");
                    }
                }
            }

            if (EntityType == TaeEntityType.PC)
            {
                printer.AppendLine();

                void DoWpnAnim(string wpnKind, Model wpnMdl)
                {
                    var wpnAnimDbgReport = wpnMdl.AnimContainer.GetAllSlotsDebugReports();
                    
                    printer.AppendLine(wpnKind);
                    if (wpnMdl?.AnimContainer?.CurrentAnimation != null)
                    {
                        
                        foreach (var kvp in wpnAnimDbgReport)
                        {
                            if (kvp.Key == NewAnimSlot.SlotTypes.Base)
                                continue;
                        
                            var dr =
                                kvp.Value.AnimEntries.FirstOrDefault();
                            if (dr != null)
                            {
                                printer.AppendLine($"      {kvp.Key}:");
                                printer.AppendLine($"        [x{dr.Weight:0.00}] [x{dr.Time:00.000}/{dr.Duration:00.000}] {dr.Name}{getAnimSizeString("", dr.AnimFileSize)}");
                            }
                        }
                    }
                }

                if (CurrentModel?.ChrAsm != null)
                {
                    foreach (var slot in CurrentModel.ChrAsm.WeaponSlots)
                    {
                        var equipParam = slot.EquipParam;
                        if (equipParam != null)
                        {
                            printer.AppendLine($"[{slot.SlotDisplayName}]");

                            var atk = equipParam.WepMotionCategory;
                            var spAtk = equipParam.SpAtkCategory;

                            printer.AppendLine(
                                $"    Part:            {equipParam.GetPartBndName_Short(slot.EquipSlotType)}");
                            printer.AppendLine($"    Moveset(s):      a{atk:D2}{(spAtk > 0 ? $", a{spAtk:D2}" : "")}");

                            slot.AccessModel(0, model => DoWpnAnim("    MDL 0", model));
                            slot.AccessModel(1, model => DoWpnAnim("    MDL 1", model));
                            slot.AccessModel(2, model => DoWpnAnim("    MDL 2", model));
                            slot.AccessModel(3, model => DoWpnAnim("    MDL 3", model));
                        }
                    }
                }



                printer.AppendLine();

            }

            if (ActionSim != null &&
                Main.Config.SimEnabled_SpEffects &&
                ActionSim.SimulatedActiveSpEffects.Count > 0)
            {
                printer.AppendLine("[Active SpEffects:]");

                foreach (var spe in ActionSim.SimulatedActiveSpEffects)
                {
                    printer.AppendLine("    " + spe);
                }
            }

            return printer;
        }

        public void DrawStatusInViewport(TaeConfigFile.ViewportStatusTypes type)
        {
            var scale = Main.Config.ViewportStatusTextSize;
            if (scale > 0)
            {
                var printer = BuildStatus(type);
                printer.BaseScale = (scale / 100) * 0.8f;

                if (Main.Debug.EnableImGuiFocusDebug)
                {
                    printer.AppendLine("--------------------");
                    printer.AppendLine("[IMGUI OSD FOCUS DEBUG]");
                    printer.AppendLine($"ActualFocusedWindow.NewImguiWindowTitle={(OSD.ActualFocusedWindow?.NewImguiWindowTitle ?? "<NULL>")}");
                    printer.AppendLine($"{nameof(OSD.AnyFieldFocused)}={OSD.AnyFieldFocused}");
                    printer.AppendLine($"{nameof(OSD.AuxFocus)}={OSD.AuxFocus}");
                    printer.AppendLine($"{nameof(OSD.Focused)}={OSD.Focused}");
                    printer.AppendLine($"{nameof(OSD.FocusedWindow)}.NewImguiWindowTitle={(OSD.FocusedWindow?.NewImguiWindowTitle ?? "<NULL>")}");
                    printer.AppendLine($"{nameof(OSD.Hovered)}={OSD.Hovered}");
                    printer.AppendLine("--------------------");
                }

                printer.Draw(out float finalHeight);

                Vector2 additionalMemeLocation = new Vector2(4, 4 + finalHeight);

                if (Main.Debug.EnableViewportAnimLayerDebug)
                {
                    
                
                    CurrentModel?.AnimContainer?.DrawDebug(ref additionalMemeLocation);
                    CurrentModel?.ChrAsm?.DrawAnimLayerDebug(ref additionalMemeLocation);


                }

                
            }
        }

        public void BuildImGuiStatus()
        {
            var printer = BuildStatus(TaeConfigFile.ViewportStatusTypes.Full);
            printer.DoAsImGuiLayout();
        }

    }
}
