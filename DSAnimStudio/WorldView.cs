﻿using DSAnimStudio.DebugPrimitives;
using DSAnimStudio.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        public Transform CameraLocationInWorld = Transform.Default;

        public Vector3 RootMotionFollow_Translation = Vector3.Zero;
        public float RootMotionFollow_Rotation = 0;

        public Vector3 CameraOrbitOrigin = new Vector3(0, 1, 0);
        public Quaternion CameraLookDirection = Quaternion.Identity;
        public Vector3 OrbitCamEuler = Vector3.Zero;

        private float orbitCamDistanceInput = 5;
        public float OrbitCamDistance => orbitCamDistanceInput / (ProjectionVerticalFoV / 43);

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

        public IDbgPrim PivotPrim = null;
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
                * Matrix.CreateTranslation(CameraOrbitOrigin + Vector3.Transform(RootMotionFollow_Translation, Matrix_World)));

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
        ViewportDragType dragType = ViewportDragType.None;

        public WorldView()
        {
            WindowsMouseHook.RawMouseMoved += HandleRawMouseMove;
        }

        ~WorldView()
        {
            WindowsMouseHook.RawMouseMoved -= HandleRawMouseMove;
        }

        private void HandleRawMouseMove(int x, int y)
        {
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
                Mouse.SetPosition((int)Math.Round(Main.Input.LeftClickDownAnchor.X), (int)Math.Round(Main.Input.LeftClickDownAnchor.Y));

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
                Program.MainInstance.IsMouseVisible = false;
                Mouse.SetPosition((int)Math.Round(Main.Input.RightClickDownAnchor.X), (int)Math.Round(Main.Input.RightClickDownAnchor.Y));

                if (Main.Input.CtrlHeld)
                {
                    CameraOrbitOrigin += Vector3.Transform(
                        new Vector3(-x, 0, -y),
                        CameraLocationInWorld.Rotation) * baseMouseSpeed
                        * CameraMoveSpeed * 0.0015f * OrbitCamDistance * ProjectionVerticalFovRatio;
                }
                else
                {
                    CameraOrbitOrigin += Vector3.Transform(
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
            UpdateDummyPolyFollowRefPoint(isFirstTime: false);

            CameraLookDirection = Quaternion.CreateFromYawPitchRoll(OrbitCamEuler.Y, OrbitCamEuler.X, OrbitCamEuler.Z);

            //NewCameraOrbitOrigin = OrbitCamCenter_DummyPolyFollowRefPoint;

            Quaternion rot = CameraLookDirection;
            rot = Quaternion.CreateFromYawPitchRoll(-RootMotionFollow_Rotation, 0, 0) * rot;

            CameraLocationInWorld.Rotation = rot;
            CameraLocationInWorld.Position = CameraOrbitOrigin + Vector3.Transform(RootMotionFollow_Translation, Matrix_World) + 
                (Vector3.Transform(Vector3.Backward * OrbitCamDistance, rot));

            Matrix_World = Matrix.CreateScale(1, 1, -1);

            if (ProjectionIsOrthographic)
            {
                Matrix_Projection = Matrix.CreateOrthographic(
                    (orbitCamDistanceInput * GFX.LastViewport.Width / GFX.LastViewport.Height) * 0.75f,
                    (orbitCamDistanceInput) * 0.75f,
                   ProjectionNearClipDist / ProjectionVerticalFovRatio, ProjectionFarClipDist);
            }
            else
            {
                Matrix_Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(ProjectionVerticalFoV),
                   1.0f * GFX.LastViewport.Width / GFX.LastViewport.Height,
                   ProjectionNearClipDist / ProjectionVerticalFovRatio, ProjectionFarClipDist);
            }

            

            Matrix_Projection_Skybox = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(ProjectionSkyboxVerticalFov),
                   1.0f * GFX.LastViewport.Width / GFX.LastViewport.Height,
                   ProjectionNearClipDist, ProjectionFarClipDist);

            Matrix_View = Matrix.CreateTranslation(-CameraLocationInWorld.Position)
                * Matrix.CreateFromQuaternion(Quaternion.Inverse(CameraLocationInWorld.Rotation));

            Matrix_View_Skybox = Matrix.CreateFromQuaternion(Quaternion.Inverse(CameraLocationInWorld.Rotation));

            if (pivotPrimVisRatio_TimerTarget <= pivotPrimVisRatio_FadeOutDuration)
            {
                pivotPrimVisRatio_TimerTarget += deltaTime;
            }

            pivotPrimVisRatio_Timer = MathHelper.Lerp(pivotPrimVisRatio_Timer, 
                Math.Max(pivotPrimVisRatio_TimerTarget, 0), MathHelper.Clamp(pivotPrimVisRatio_TimerLerpS * (deltaTime / (1f / 60f)), 0, 1));

            pivotPrimDrawoverPulseTimer += deltaTime;
            pivotPrimDrawoverPulseTimer = pivotPrimDrawoverPulseTimer % pivotPrimDrawoverPulseTimerModulo;

            UpdatePivotPrim();
        }

        public void DrawPrims()
        {
            PivotPrim.Draw(null, Matrix_World);
        }

        public void DrawOverPrims()
        {
            PivotPrim_DrawOver.Draw(null, Matrix_World);
        }

        public void UpdateInput()
        {
            if (DisableAllInput || OSD.Focused)
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
                else
                {
                    

                        // Handle mouse wheel zoom.
                    if (Main.TAE_EDITOR.ModelViewerBounds_InputArea.Contains(Main.Input.LeftClickDownAnchor))
                    {
                        if (Main.Input.MiddleClickHeld)
                        {
                            MakePivotPrimVisible();
                            NewRecenter();
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
                                    orbitCamDistanceInput = 5;
                                    scrollDelta = 0;
                                }
                            }

                            orbitCamDistanceInput -= scrollDelta * zoomMult * ProjectionVerticalFovRatio;

                            if (Main.Input.ScrollDelta != 0)
                                MakePivotPrimVisible();

                            if (orbitCamDistanceInput < (0.05f / ProjectionVerticalFovRatio))
                            {
                                orbitCamDistanceInput = (0.05f / ProjectionVerticalFovRatio);
                            }
                        }
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



        public Action NewDoRecenterAction = null;

        public void NewRecenter()
        {
            OrbitCamEuler = Vector3.Zero;
            CameraOrbitOrigin  
                = OrbitCamCenter_DummyPolyFollowRefPoint_Init
                = new Vector3(0, OrbitCamCenter_DummyPolyFollowRefPoint.Y, 0);
            //NewDoRecenterAction?.Invoke();
        }

       
        public Vector3 OrbitCamCenter_DummyPolyFollowRefPoint = new Vector3(0, 0.5f, 0);
        public Vector3 OrbitCamCenter_DummyPolyFollowRefPoint_Init = new Vector3(0, 0.5f, 0);

        public void UpdateDummyPolyFollowRefPoint(bool isFirstTime)
        {
            if (Scene.Models.Count > 0)
            {
                bool CheckCenterDummyPoly(int dmyID)
                {
                    if (Scene.Models[0].DummyPolyMan.DummyPolyByRefID.ContainsKey(dmyID))
                    {
                        var lockonPoint1 = Scene.Models[0].DummyPolyMan.GetDummyPosByID(dmyID, 
                            Scene.Models[0].StartTransform.WorldMatrix * 
                            Matrix.Invert(Scene.Models[0].CurrentRootMotionRotation * 
                            Scene.Models[0].CurrentRootMotionTranslation));
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

                bool foundDmy = false;

                foundDmy = CheckCenterDummyPoly(240);

                if (!foundDmy)
                    foundDmy = CheckCenterDummyPoly(220);


            }
        }

        public float OverallMouseSpeedMult = 1.0f;
        public float CameraTurnSpeedMouse = 1;
        public float CameraMoveSpeed = 1;

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
