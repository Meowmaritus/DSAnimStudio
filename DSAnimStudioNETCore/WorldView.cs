using DSAnimStudio.DebugPrimitives;
using DSAnimStudio.GFXShaders;
using DSAnimStudio.ImguiOSD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class WorldView
    {
        public bool ShowGrid = true;

        public bool LockAspectRatioDuringRemo = true;

        public string Name = null;
        public WorldView()
        {
            WindowsMouseHook.RawMouseMoved += HandleRawMouseMove;
        }
        public WorldView(string name)
        {
            Name = name;
            WindowsMouseHook.RawMouseMoved += HandleRawMouseMove;
        }
        //public WorldView()
        //{
        //    WindowsMouseHook.RawMouseMoved += HandleRawMouseMove;
        //}
        ~WorldView()
        {
            WindowsMouseHook.RawMouseMoved -= HandleRawMouseMove;
        }

        public Transform CameraLocationInWorld = Transform.Default;
        public Transform CameraLocationInWorld_CloserForSound = Transform.Default;

        public bool RootMotionFollow_EnableWrap = false;
        public Vector3 RootMotionFollow_Translation_WrappedIfApplicable => new Vector3(RootMotionFollow_EnableWrap ? (RootMotionFollow_Translation.X % 1) : RootMotionFollow_Translation.X,
            RootMotionFollow_Translation.Y,
            RootMotionFollow_EnableWrap ? (RootMotionFollow_Translation.Z % 1) : RootMotionFollow_Translation.Z);

        public Vector3 RootMotionOffsetFromWrappedCenter => RootMotionFollow_Translation_WrappedIfApplicable - RootMotionFollow_Translation;

        public Vector3 RootMotionFollow_Translation = Vector3.Zero;
        public float RootMotionFollow_Rotation = 0;

        public Matrix RootMotionFollow_Matrix => Matrix.CreateRotationY(RootMotionFollow_Rotation) * Matrix.CreateTranslation(RootMotionFollow_Translation);
        public Matrix RootMotionFollow_Matrix_WrappedIfApplicable => Matrix.CreateRotationY(RootMotionFollow_Rotation) * Matrix.CreateTranslation(RootMotionFollow_Translation_WrappedIfApplicable);

        public Vector3 CameraOrbitOrigin = new Vector3(0, 1, 0);
        public Vector3 CameraOrbitOriginOffset = new Vector3(0, 0, 0);
        public float CameraOrbigtDefaultHeightForRootMotion = 0;

        public Quaternion CameraLookDirection = Quaternion.Identity;
        public Vector3 OrbitCamEuler = Vector3.Zero;

        [JsonIgnore]
        public SoulsAssetPipeline.Animation.SIBCAM.SibcamPlayer.View? RemoCamView = null;
        [JsonIgnore]
        public SoulsAssetPipeline.Animation.SIBCAM.SibcamPlayer.View RumbleCam = SoulsAssetPipeline.Animation.SIBCAM.SibcamPlayer.View.Default;

        ///// <summary>
        ///// TODO: rumblecam simulation lol. Call <see cref="ClearCustomCameraView_Additive"/> first, 
        ///// then add any currently playing ones, taking into account the falloff distance thing for the weight pog
        ///// </summary>
        //public CameraViewModifier CUSTOM_VIEW_MULTIPLY = CameraViewModifier.Default;

        public float OrbitCamDistanceInput = 8;
        public float OrbitCamDistance => OrbitCamDistanceInput / (ProjectionVerticalFoV / 43);

        public float ProjectionVerticalFoV = 43;
        public bool ProjectionIsOrthographic => ProjectionVerticalFoV <= 2;
        public float ProjectionVerticalFovRatio => (ProjectionVerticalFoV / 43);
        public float ProjectionSkyboxVerticalFov = 70;
        public float ProjectionNearClipDist = 0.1f;
        public float ProjectionFarClipDist = 10000;
        public Matrix Matrix_Projection = Matrix.Identity;
        public Matrix Matrix_Projection_Skybox = Matrix.Identity;

        public Matrix Matrix_World = Matrix.Identity;
        public Matrix Matrix_View = Matrix.Identity;
        public Matrix Matrix_View_Skybox = Matrix.Identity;

        public bool DisableAllInput = false;

        public bool RequestRecenter = false;

        public bool FollowingLockon = true;
        public int FollowingLockonDummyPoly = -1;

        public float AngleSnap = MathHelper.PiOver4;
        public bool AngleSnapEnable = false;

        public bool PivotPrimIsEnabled = false;
        [JsonIgnore]
        public IDbgPrim PivotPrim = null;
        [JsonIgnore]
        public IDbgPrim PivotPrim_DrawOver = null;
        private float pivotPrimDrawoverPulseTimer = 0;
        private float pivotPrimDrawoverPulseTimerModulo = 1f;
        public float PivotPrimVisRatio { get; private set; } = 0;
        private float pivotPrimVisRatio_FadeOutDuration = 0.25f;
        private float pivotPrimVisRatio_FadeOutDelay = 1f;
        private float pivotPrimVisRatio_Timer = 0;
        private float pivotPrimVisRatio_TimerTarget = 0;
        private float pivotPrimVisRatio_TimerLerpS = 0.3f;
        private float pivotPrimVisRatio_MaxScale = 0.125f;
        public void MakePivotPrimVisible()
        {
            pivotPrimVisRatio_TimerTarget = -pivotPrimVisRatio_FadeOutDelay;
        }

        private void UpdatePivotPrim()
        {
            if (pivotPrimVisRatio_Timer < 0)
            {
                PivotPrimVisRatio = 1;
            }
            else
            {
                PivotPrimVisRatio = 1 - MathHelper.Clamp(pivotPrimVisRatio_Timer / pivotPrimVisRatio_FadeOutDuration, 0, 1);
            }

            if (PivotPrim == null)
            {
                PivotPrim = new DbgPrimWireBox(Transform.Default, -Vector3.One, Vector3.One, Color.Transparent);
            }

            if (PivotPrim_DrawOver == null)
            {
                PivotPrim_DrawOver = new DbgPrimWireBox(Transform.Default, -Vector3.One, Vector3.One, Color.Transparent);
            }

            float pulseRatio = (float)Math.Sin(MathHelper.Pi * (pivotPrimDrawoverPulseTimer / pivotPrimDrawoverPulseTimerModulo));

            PivotPrim.Transform = PivotPrim_DrawOver.Transform = new Transform(Matrix.CreateScale(
                pivotPrimVisRatio_MaxScale + (pivotPrimVisRatio_MaxScale * (1 + (pulseRatio * pulseRatio * 0.1f))) 
                * PivotPrimVisRatio * ProjectionVerticalFovRatio)
                // * Matrix.CreateFromQuaternion(CameraLookDirection)
                * Matrix.CreateRotationY(-RootMotionFollow_Rotation)
                * Matrix.CreateTranslation(CameraOrbitOrigin + CameraOrbitOriginOffset + Vector3.Transform(RootMotionFollow_Translation_WrappedIfApplicable, Matrix_World)));

            PivotPrim.OverrideColor = Color.Lerp(
                new Color(Main.Colors.ColorHelperCameraPivot.R, 
                Main.Colors.ColorHelperCameraPivot.G, 
                Main.Colors.ColorHelperCameraPivot.B, (byte)0), Main.Colors.ColorHelperCameraPivot, PivotPrimVisRatio);

            

            PivotPrim_DrawOver.OverrideColor = Color.Lerp(
                new Color(Main.Colors.ColorHelperCameraPivot.R,
                Main.Colors.ColorHelperCameraPivot.G,
                Main.Colors.ColorHelperCameraPivot.B, (byte)0),
                new Color(Main.Colors.ColorHelperCameraPivot.R,
                Main.Colors.ColorHelperCameraPivot.G,
                Main.Colors.ColorHelperCameraPivot.B, 
                (byte)(255f * 0.25f)), PivotPrimVisRatio);
        }


        enum ViewportDragType
        {
            None,
            Invalid,
            LeftClick,
            RightClick,
        }
        [JsonIgnore]
        ViewportDragType dragType = ViewportDragType.None;

        

        private void HandleRawMouseMove(int x, int y)
        {
            if (WorldViewManager.CurrentView != this)
                return;

            TooltipManager.CancelTooltip();

            if (!Main.Active)
            {
                Program.MainInstance.IsMouseVisible = true;
                return;
            }

            float baseMouseSpeed = ((float)System.Windows.Forms.SystemInformation.MouseSpeed / 20f) * OverallMouseSpeedMult;

            if (Main.Input.ShiftHeld)
                baseMouseSpeed *= 5;

            if (Main.Input.LeftClickHeld && dragType == ViewportDragType.LeftClick)
            {
                Program.MainInstance.IsMouseVisible = false;
                Mouse.SetPosition((int)Math.Round(Main.Input.LeftClickDownAnchor.X * Main.DPIX), 
                    (int)Math.Round(Main.Input.LeftClickDownAnchor.Y * Main.DPIY));

                bool isInvertedPitch = false;

                //float pitchCheck = OrbitCamEuler.X + MathHelper.PiOver2;

                //if (pitchCheck >= 0)
                //{
                //    isInvertedPitch = Math.Floor(pitchCheck / MathHelper.Pi) % 2 == 1;
                //}
                //else
                //{
                //    isInvertedPitch = Math.Floor(-pitchCheck / MathHelper.Pi) % 2 == 0;
                //}

                OrbitCamEuler += new Vector3(-y, isInvertedPitch ? x : -x, 0) * baseMouseSpeed * CameraTurnSpeedMouse * 0.01f;
            }
            else if (Main.Input.RightClickHeld && dragType == ViewportDragType.RightClick)
            {
                //FollowingLockon = false;

                Program.MainInstance.IsMouseVisible = false;
                Mouse.SetPosition((int)Math.Round(Main.Input.LeftClickDownAnchor.X * Main.DPIX),
                    (int)Math.Round(Main.Input.LeftClickDownAnchor.Y * Main.DPIY));

                if (Main.Input.CtrlHeld)
                {
                    CameraOrbitOriginOffset += Vector3.Transform(
                        new Vector3(-x, 0, -y),
                        CameraLocationInWorld.Rotation) * baseMouseSpeed
                        * CameraMoveSpeed * 0.0015f * OrbitCamDistance * ProjectionVerticalFovRatio;
                }
                else
                {
                    CameraOrbitOriginOffset += Vector3.Transform(
                        new Vector3(-x, y, 0),
                        CameraLocationInWorld.Rotation) * baseMouseSpeed
                        * CameraMoveSpeed * 0.0015f * OrbitCamDistance * ProjectionVerticalFovRatio;
                }
                
            }
            else
            {
                Program.MainInstance.IsMouseVisible = true;
            }
        }

        public void DrawMouseDragCursor()
        {
            if (dragType == ViewportDragType.LeftClick)
            {
                //GFX.SpriteBatchBegin(transformMatrix: Main.DPIMatrix);
                //Draw something at Main.Input.LeftClickDownAnchor
                //GFX.SpriteBatchEnd();
            }
            else if (dragType == ViewportDragType.RightClick)
            {
                //GFX.SpriteBatchBegin(transformMatrix: Main.DPIMatrix);
                //Draw something at Main.Input.RightClickDownAnchor
                //GFX.SpriteBatchEnd();
            }
        }

        public void Update(float deltaTime)
        {
            //OrbitCamEuler.Y += MathHelper.PiOver4 * Main.DELTA_UPDATE * 0.25f;
            UpdateDummyPolyFollowRefPoint(FollowingLockonDummyPoly);

            if (FollowingLockon)
            {
                NewRecenter(centerRotationToo: false);
                //MakePivotPrimVisible();
            }
            else
            {
                CameraOrbitOrigin = Vector3.Zero;
            }


            if (dragType == ViewportDragType.None)
            {
                Vector3 euler = OrbitCamEuler;

                if (AngleSnapEnable && AngleSnap > 0)
                {
                    euler.X = (float)(Math.Round(OrbitCamEuler.X / AngleSnap) * AngleSnap);
                    euler.Y = (float)(Math.Round(OrbitCamEuler.Y / AngleSnap) * AngleSnap);
                    euler.Z = (float)(Math.Round(OrbitCamEuler.Z / AngleSnap) * AngleSnap);
                }

                OrbitCamEuler = Vector3.Lerp(OrbitCamEuler, euler, 0.25f * (Main.DELTA_UPDATE / 0.0166667f));
            }

            if (RemoCamView == null)
            {
                CameraLookDirection = Quaternion.CreateFromYawPitchRoll(OrbitCamEuler.Y, OrbitCamEuler.X, OrbitCamEuler.Z);

                    Quaternion rot = CameraLookDirection;
                    rot = Quaternion.CreateFromYawPitchRoll(-RootMotionFollow_Rotation, 0, 0) * rot;

                    CameraLocationInWorld.Rotation = rot;

                CameraLocationInWorld_CloserForSound.Rotation = CameraLocationInWorld.Rotation;

                CameraLocationInWorld.Position = CameraOrbitOrigin + CameraOrbitOriginOffset + Vector3.Transform(RootMotionFollow_Translation_WrappedIfApplicable, Matrix_World) +
                (Vector3.Transform(Vector3.Backward * OrbitCamDistance, rot));

                CameraLocationInWorld_CloserForSound.Position = CameraOrbitOrigin + CameraOrbitOriginOffset + Vector3.Transform(RootMotionFollow_Translation, Matrix_World) +
                    (Vector3.Transform(Vector3.Backward * OrbitCamDistanceInput, rot));
            }
            else
            {
                CameraLocationInWorld.Position = RemoCamView.Value.MoveMatrix.Translation.ToXna();
                CameraLocationInWorld.Rotation = RemoCamView.Value.MoveMatrix.Rotation.ToXna();
                CameraLocationInWorld.Scale = RemoCamView.Value.MoveMatrix.Scale.ToXna();

                CameraLookDirection = RemoCamView.Value.MoveMatrix.Rotation.ToXna();

                CameraLocationInWorld_CloserForSound = CameraLocationInWorld;
            }

            

            Matrix_World = Matrix.CreateScale(1, 1, -1);

            var finalFov = ProjectionVerticalFoV
                    * (RemoCamView?.Fov ?? 1)
                    * RumbleCam.Fov;

            if (finalFov > 179)
                finalFov = 179;

            if (finalFov < 1)
                finalFov = 1;

            if (ProjectionIsOrthographic && RemoCamView == null)
            {
                Matrix_Projection = Matrix.CreateOrthographic(
                    (OrbitCamDistanceInput * GFX.LastViewport.Width / GFX.LastViewport.Height) * 0.75f,
                    (OrbitCamDistanceInput) * 0.75f,
                   ProjectionNearClipDist / ProjectionVerticalFovRatio, ProjectionFarClipDist);
            }
            else
            {
                Matrix_Projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(finalFov),
                   1.0f * GFX.LastViewport.Width / GFX.LastViewport.Height,
                   ProjectionNearClipDist / ProjectionVerticalFovRatio, ProjectionFarClipDist);
            }

            

            Matrix_Projection_Skybox = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(ProjectionSkyboxVerticalFov),
                   1.0f * GFX.LastViewport.Width / GFX.LastViewport.Height,
                   ProjectionNearClipDist, ProjectionFarClipDist);


            Matrix_View =
                Matrix.CreateTranslation(-CameraLocationInWorld.Position)
                * Matrix.CreateFromQuaternion(Quaternion.Inverse(CameraLocationInWorld.Rotation))
                * Matrix.Invert(RumbleCam.MoveMatrix.GetMatrix());



            Matrix_View_Skybox = Matrix.CreateFromQuaternion(Quaternion.Inverse(CameraLocationInWorld.Rotation));

            if (pivotPrimVisRatio_TimerTarget <= pivotPrimVisRatio_FadeOutDuration)
            {
                pivotPrimVisRatio_TimerTarget += deltaTime;
            }

            pivotPrimVisRatio_Timer = MathHelper.Lerp(pivotPrimVisRatio_Timer, 
                Math.Max(pivotPrimVisRatio_TimerTarget, 0), MathHelper.Clamp(pivotPrimVisRatio_TimerLerpS * (deltaTime / (1f / 60f)), 0, 1));

            pivotPrimDrawoverPulseTimer += deltaTime;
            pivotPrimDrawoverPulseTimer = pivotPrimDrawoverPulseTimer % pivotPrimDrawoverPulseTimerModulo;

            RumbleCamManager.UpdateAll(deltaTime, CameraLocationInWorld.Position);

            UpdatePivotPrim();
        }

        public void MiddleClickRecenter()
        {
            if (FollowingLockon)
            {
                CenterOnDummyPoly(Main.Config.CameraFollowDummyPolyID, -1);
                CameraOrbitOrigin = CameraOrbitOriginOffset;
                CameraOrbitOriginOffset = Vector3.Zero;
            }
            else
            {
                if (CameraOrbigtDefaultHeightForRootMotion == 0)
                {
                    CenterOnDummyPoly(240, 220);
                    CameraOrbigtDefaultHeightForRootMotion = CameraOrbitOrigin.Y + CameraOrbitOriginOffset.Y;
                }
                CameraOrbitOrigin = Vector3.Zero;
                CameraOrbitOriginOffset = new Vector3(0, CameraOrbigtDefaultHeightForRootMotion, 0);
                OrbitCamEuler = Vector3.Zero;
            }
        }

        public void DrawPrims()
        {
            if (PivotPrimIsEnabled)
                PivotPrim.Draw(null, Matrix_World);
        }

        public void DrawOverPrims()
        {
            if (PivotPrimIsEnabled)
                PivotPrim_DrawOver.Draw(null, Matrix_World);
        }

        public void UpdateInput()
        {
            if (RequestRecenter)
            {
                FollowingLockon = false;
                MiddleClickRecenter();
                RequestRecenter = false;
            }

            if (DisableAllInput || OSD.Focused || OSD.Hovered || Main.HasUncommittedWindowResize)
                return;



            if (dragType == ViewportDragType.None)
            {
                if (Main.Input.LeftClickDown)
                {
                    if (Main.TAE_EDITOR.ModelViewerBounds_InputArea.Contains(Main.Input.LeftClickDownAnchor))
                    {
                        dragType = ViewportDragType.LeftClick;
                    }
                    else
                    {
                        dragType = ViewportDragType.Invalid;
                    }
                }
                else if (Main.Input.RightClickDown)
                {
                    if (Main.TAE_EDITOR.ModelViewerBounds_InputArea.Contains(Main.Input.RightClickDownAnchor))
                    {
                        dragType = ViewportDragType.RightClick;
                    }
                    else
                    {
                        dragType = ViewportDragType.Invalid;
                    }
                }

                

                Program.MainInstance.IsMouseVisible = true;
            }
            else
            {
                if (dragType != ViewportDragType.Invalid)
                    MakePivotPrimVisible();

                if (dragType == ViewportDragType.LeftClick)
                {
                    if (Main.Input.LeftClickHeld)
                    {
                        Program.MainInstance.IsMouseVisible = false;
                    }
                    else
                    {
                        dragType = ViewportDragType.None;
                        Program.MainInstance.IsMouseVisible = true;
                    }
                }
                else if (dragType == ViewportDragType.RightClick)
                {
                    if (Main.Input.RightClickHeld)
                    {
                        Program.MainInstance.IsMouseVisible = false;
                    }
                    else
                    {
                        dragType = ViewportDragType.None;
                        Program.MainInstance.IsMouseVisible = true;
                    }
                }
                else if (dragType == ViewportDragType.Invalid)
                {
                    if (!Main.Input.LeftClickHeld && !Main.Input.RightClickHeld)
                    {
                        dragType = ViewportDragType.None;
                        Program.MainInstance.IsMouseVisible = true;
                    }
                }
            }


            bool isDragging = (dragType == ViewportDragType.LeftClick || dragType == ViewportDragType.RightClick);

            // Handle mouse wheel zoom.
            if ((Main.TAE_EDITOR.ModelViewerBounds_InputArea.Contains(Main.Input.LeftClickDownAnchor) &&
                Main.TAE_EDITOR.ModelViewerBounds_InputArea.Contains(Main.Input.MousePosition)) || isDragging)
            {
                if (Main.Input.MiddleClickHeld && !isDragging)
                {
                    //FollowingLockon = true;
                    MakePivotPrimVisible();

                    MiddleClickRecenter();

                    
                }
                else
                {
                    float zoomMult = (OrbitCamDistance / 50) * (OrbitCamDistance / 50);
                    zoomMult = (zoomMult * 0.75f) + 0.25f;
                    if (Main.Input.ShiftHeld)
                        zoomMult *= 5;
                    zoomMult = Math.Min(zoomMult, 50);
                    zoomMult = Math.Max(zoomMult, 0.25f);

                    float scrollDelta = Main.Input.ScrollDelta;

                    if (Main.Input.CtrlHeld /*&& !Main.Input.ShiftHeld*/ && !Main.Input.AltHeld)
                    {
                        if (Main.Input.KeyDown(Keys.OemPlus) || Main.Input.KeyDown(Keys.Add))
                        {
                            scrollDelta += 1;
                        }
                        else if (Main.Input.KeyDown(Keys.OemMinus) || Main.Input.KeyDown(Keys.Subtract))
                        {
                            scrollDelta -= 1;
                        }
                        else if (Main.Input.KeyDown(Keys.D0) || Main.Input.KeyDown(Keys.NumPad0))
                        {
                            OrbitCamDistanceInput = 5;
                            scrollDelta = 0;
                        }
                    }

                    OrbitCamDistanceInput -= scrollDelta * zoomMult * ProjectionVerticalFovRatio;

                    if (Main.Input.ScrollDelta != 0)
                        MakePivotPrimVisible();

                    if (OrbitCamDistanceInput < (0.05f / ProjectionVerticalFovRatio))
                    {
                        OrbitCamDistanceInput = (0.05f / ProjectionVerticalFovRatio);
                    }
                }
            }

        }

        public Vector3 GetCameraUp()
        {
            return Vector3.TransformNormal(Vector3.Up, Matrix.CreateFromQuaternion(CameraLocationInWorld.Rotation));
        }

        public Vector3 GetCameraForward()
        {
            return Vector3.TransformNormal(Vector3.Forward, Matrix.CreateFromQuaternion(CameraLocationInWorld.Rotation));
        }

        public void ApplyViewToShader_Skybox<T>(IGFXShader<T> shader)
           where T : Effect
        {
            Matrix m = Matrix.Identity;

            //if (TaeInterop.CameraFollowsRootMotion)
            //    m *= Matrix.CreateTranslation(-TaeInterop.CurrentRootMotionDisplacement.XYZ());

            shader.ApplyWorldView(m * Matrix_World, Matrix_View_Skybox, Matrix_Projection_Skybox);
        }

        public void ApplyViewToShader<T>(IGFXShader<T> shader, Matrix modelMatrix)
            where T : Effect
        {
            shader.ApplyWorldView(modelMatrix * Matrix_World, Matrix_View, Matrix_Projection);
        }

        public void ApplyViewToShader<T>(IGFXShader<T> shader, Transform modelTransform)
            where T : Effect
        {
            ApplyViewToShader(shader, modelTransform.WorldMatrix);
        }

        public void OnModelLoadedRecenter()
        {
            FollowingLockon = true;
            UpdateDummyPolyFollowRefPoint(220);
            CenterOnDummyPoly(240, 220);
            CameraOrbigtDefaultHeightForRootMotion = GFX.CurrentWorldView.CameraOrbitOrigin.Y + GFX.CurrentWorldView.CameraOrbitOriginOffset.Y;
            CameraOrbitOrigin = Vector3.Zero;
            CameraOrbitOriginOffset = new Vector3(0, CameraOrbigtDefaultHeightForRootMotion, 0);
        }

        [JsonIgnore]
        public Action NewDoRecenterAction = null;

        public void NewRecenter(bool centerRotationToo = true)
        {
            if (centerRotationToo)
                OrbitCamEuler = Vector3.Zero;
            CameraOrbitOrigin
                = OrbitCamCenter_DummyPolyFollowRefPoint_Init
                = Vector3.Transform(OrbitCamCenter_DummyPolyFollowRefPoint, Matrix.CreateRotationY(RootMotionFollow_Rotation) * Matrix_World);
            //CameraOrbitOriginOffset = Vector3.Zero;
            //NewDoRecenterAction?.Invoke();
        }

        public void CenterOnDummyPoly(int dummyPolyID, int altDummyPolyID)
        {
            var prevFollowLockon = FollowingLockon;
            FollowingLockon = true;
            if (!UpdateDummyPolyFollowRefPoint(dummyPolyID))
                UpdateDummyPolyFollowRefPoint(altDummyPolyID);
            NewRecenter(true);
            CameraOrbitOriginOffset = CameraOrbitOrigin;
            CameraOrbitOrigin = Vector3.Zero;
            FollowingLockon = prevFollowLockon;
        }
       
        public Vector3 OrbitCamCenter_DummyPolyFollowRefPoint = new Vector3(0, 0, 0);
        public Vector3 OrbitCamCenter_DummyPolyFollowRefPoint_Init = new Vector3(0, 0, 0);

        public bool UpdateDummyPolyFollowRefPoint(int dummyPolyID)
        {
            if (Scene.IsModelLoaded)
            {
                bool CheckCenterDummyPoly(int dmyID)
                {
                    if (Scene.MainModel == null)
                        return false;
                    if (Scene.MainModel.DummyPolyMan.DummyPolyByRefID.ContainsKey(dmyID))
                    {
                        var lockonPoint1 = Scene.MainModel.DummyPolyMan.GetDummyPosByID(dmyID, getAbsoluteWorldPos: false);
                        if (lockonPoint1.Count > 0)
                        {
                            OrbitCamCenter_DummyPolyFollowRefPoint = lockonPoint1[0];


                            //if (isFirstTime)
                            //{
                            //    OrbitCamCenter_DummyPolyFollowRefPoint_Init = lockonPoint1[0];
                            //}
                            //else
                            //{
                            //    //float screenPhysicalHeight = GetScreenPhysicalHeight();

                            //    //float verticalDistNeededToMove = 0;

                            //    float threshold = (NewOrbitCamDistance * 0.4f);

                            //    float verticalDistNeededToMove = lockonPoint1[0].Y - (OrbitCamCenter_DummyPolyFollowRefPoint.Y + (threshold / 2));
                                
                            //    if (verticalDistNeededToMove > 0)
                            //    {
                            //        float absDist = Math.Max(Math.Abs(verticalDistNeededToMove), 0);
                            //        verticalDistNeededToMove = absDist;
                            //    }
                            //    else if (verticalDistNeededToMove < 0)
                            //    {
                            //        float absDist = Math.Max(Math.Abs(verticalDistNeededToMove) - threshold, 0);
                            //        verticalDistNeededToMove = -absDist;
                            //    }

                            //    //float top = OrbitCamCenter_DummyPolyFollowRefPoint.Y + threshold;
                            //    //float bottom = OrbitCamCenter_DummyPolyFollowRefPoint.Y - threshold;

                            //    //if (lockonPoint1[0].Y > top)
                            //    //{
                            //    //    verticalDistNeededToMove = (lockonPoint1[0].Y - top) - ;
                            //    //}
                            //    //else if (lockonPoint1[0].Y < bottom)
                            //    //{
                            //    //    verticalDistNeededToMove = lockonPoint1[0].Y - bottom;
                            //    //}



                            //    //float moveUrgencyRatio = MathHelper.Clamp((Math.Abs(verticalDistNeededToMove / (screenPhysicalHeight / 2))), 0, 0.95f);
                            //    //moveUrgencyRatio = MathHelper.Clamp(Utils.MapRange(moveUrgencyRatio, DummyPolyFollowStartRatioFromScreenCenter, 0.6666f, 0, 0.9f), 0, 0.9f);
                            //    //float lerpedYCoord = MathHelper.Lerp(OrbitCamCenter_DummyPolyFollowRefPoint.Y, OrbitCamCenter_DummyPolyFollowRefPoint.Y + verticalDistNeededToMove, Main.DELTA_UPDATE * 10);

                            //    OrbitCamCenter_DummyPolyFollowRefPoint.Y = MathHelper.Lerp(OrbitCamCenter_DummyPolyFollowRefPoint.Y, OrbitCamCenter_DummyPolyFollowRefPoint.Y + verticalDistNeededToMove, Main.DELTA_UPDATE * 15);

                            //    //OrbitCamCenter_DummyPolyFollowRefPoint = lockonPoint1[0];
                            //}
                            return true;
                        }
                    }

                    return false;
                }

                return CheckCenterDummyPoly(dummyPolyID);


            }

            return false;
        }

        public float OverallMouseSpeedMult = 1.0f;
        public float CameraTurnSpeedMouse = 1;
        public float CameraMoveSpeed = 1;

        public Vector2 Project3DPosToScreen(Vector3 pos)
        {
            var screenPos = GFX.Device.Viewport.Project(pos, Matrix_Projection, Matrix_View, Matrix_World);
            return new Vector2(screenPos.X, screenPos.Y);
        }

        public Ray GetScreenRay(Vector2 screenPos)
        {
            var a = GFX.Device.Viewport.Unproject(
                new Vector3(screenPos, 0.1f),
                Matrix_Projection, Matrix_View, Matrix_World);

            var b = GFX.Device.Viewport.Unproject(
                new Vector3(screenPos, 0.2f),
                Matrix_Projection, Matrix_View, Matrix_World);

            return new Ray(a, Vector3.Normalize(b - a));
        }

        
    }
}
