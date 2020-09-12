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

        public Vector3 CameraOrbitOrigin = Vector3.Zero;
        public Quaternion CameraLookDirection = Quaternion.Identity;
        public Vector3 OrbitCamEuler = Vector3.Zero;

        public float OrbitCamDistance = 2;

        public float ProjectionVerticalFoV = 43;
        public float ProjectionSkyboxVerticalFovMult = 1.5f;
        public float ProjectionNearClipDist = 0.1f;
        public float ProjectionFarClipDist = 10000;
        public Matrix Matrix_Projection = Matrix.Identity;
        public Matrix Matrix_Projection_Skybox = Matrix.Identity;

        public Matrix Matrix_World = Matrix.Identity;
        public Matrix Matrix_View = Matrix.Identity;
        public Matrix Matrix_View_Skybox = Matrix.Identity;

        public bool DisableAllInput = false;

        
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

            

            if (Main.Input.LeftClickHeld && dragType == ViewportDragType.LeftClick)
            {
                Program.MainInstance.IsMouseVisible = false;
                Mouse.SetPosition((int)Math.Round(Main.Input.LeftClickDownAnchor.X), (int)Math.Round(Main.Input.LeftClickDownAnchor.Y));
                OrbitCamEuler += new Vector3(-y, -x, 0) * baseMouseSpeed * CameraTurnSpeedMouse * 0.01f;
            }
            else if (Main.Input.RightClickHeld && dragType == ViewportDragType.RightClick)
            {
                Program.MainInstance.IsMouseVisible = false;
                Mouse.SetPosition((int)Math.Round(Main.Input.RightClickDownAnchor.X), (int)Math.Round(Main.Input.RightClickDownAnchor.Y));

                if (Main.Input.ShiftHeld)
                {
                    float zoomMult = (OrbitCamDistance / 50) * (OrbitCamDistance / 50);
                    zoomMult = Math.Min(zoomMult, 50);
                    zoomMult = Math.Max(zoomMult, 0.05f);
                    OrbitCamDistance -= x * zoomMult * baseMouseSpeed * 0.01f;

                    if (OrbitCamDistance < 0.05f)
                    {
                        float distToUnfuck = Math.Abs(OrbitCamDistance - 0.1f);
                        CameraOrbitOrigin += Vector3.Transform(
                        new Vector3(0, 0, -distToUnfuck),
                        CameraLocationInWorld.Rotation);
                        OrbitCamDistance = 0.05f;
                    }
                }
                else
                {
                    CameraOrbitOrigin += Vector3.Transform(
                        new Vector3(-x, y, 0),
                        CameraLocationInWorld.Rotation) * baseMouseSpeed
                        * CameraMoveSpeed * 0.0015f * OrbitCamDistance;
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

        public void Update()
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

            Matrix_Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(ProjectionVerticalFoV),
                   1.0f * GFX.LastViewport.Width / GFX.LastViewport.Height,
                   ProjectionNearClipDist, ProjectionFarClipDist);

            Matrix_Projection_Skybox = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(ProjectionVerticalFoV * ProjectionSkyboxVerticalFovMult),
                   1.0f * GFX.LastViewport.Width / GFX.LastViewport.Height,
                   ProjectionNearClipDist, ProjectionFarClipDist);

            Matrix_View = Matrix.CreateTranslation(-CameraLocationInWorld.Position)
                * Matrix.CreateFromQuaternion(Quaternion.Inverse(CameraLocationInWorld.Rotation));

            Matrix_View_Skybox = Matrix.CreateFromQuaternion(Quaternion.Inverse(CameraLocationInWorld.Rotation));

            
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
                    if (Main.TAE_EDITOR.ModelViewerBounds_InputArea.Contains(Main.Input.MousePositionPoint))
                    {
                        if (Main.Input.MiddleClickHeld)
                        {
                            NewRecenter();
                        }
                        else
                        {
                            float zoomMult = (OrbitCamDistance / 50) * (OrbitCamDistance / 50);
                            zoomMult = Math.Min(zoomMult, 50);
                            zoomMult = Math.Max(zoomMult, 0.25f);
                            OrbitCamDistance -= Main.Input.ScrollDelta * zoomMult;

                            if (OrbitCamDistance < 0.05f)
                            {
                                float distToUnfuck = Math.Abs(OrbitCamDistance - 0.1f);
                                CameraOrbitOrigin += Vector3.Transform(
                                new Vector3(0, 0, -distToUnfuck),
                                CameraLocationInWorld.Rotation);
                                OrbitCamDistance = 0.05f;
                            }
                        }
                    }
                }

                Program.MainInstance.IsMouseVisible = true;
            }
            else if (dragType == ViewportDragType.LeftClick)
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
                = OrbitCamCenter_DummyPolyFollowRefPoint;
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
