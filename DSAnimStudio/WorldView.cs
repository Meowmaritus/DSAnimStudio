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
        public Transform NewCameraTransform = Transform.Default;

        public Vector3 NewRootMotionFollow_Position = Vector3.Zero;
        public float NewRootMotionFollow_Rotation = 0;

        public Vector3 NewCameraOrbitOrigin = Vector3.Zero;
        public Quaternion NewCameraLookDirection = Quaternion.Identity;
        public Vector3 NewCameraEuler = Vector3.Zero;

        public float NewOrbitCamDistance = 2;

        public float FieldOfView = 43;
        public float NewNearClipDistance = 0.1f;
        public float NewFarClipDistance = 10000;
        public Matrix NewMatrix_Projection = Matrix.Identity;

        public Matrix NewMatrix_World = Matrix.Identity;
        public Matrix NewMatrix_View = Matrix.Identity;
        public Matrix NewMatrix_View_Skybox = Matrix.Identity;

        public bool DisableAllInput = false;

        public void NewUpdate()
        {
            UpdateDummyPolyFollowRefPoint(isFirstTime: false);

            NewCameraLookDirection = Quaternion.CreateFromYawPitchRoll(NewCameraEuler.Y, NewCameraEuler.X, NewCameraEuler.Z);

            //NewCameraOrbitOrigin = OrbitCamCenter_DummyPolyFollowRefPoint;

            Quaternion rot = NewCameraLookDirection;
            rot = Quaternion.CreateFromYawPitchRoll(-NewRootMotionFollow_Rotation, 0, 0) * rot;

            NewCameraTransform.Rotation = rot;
            NewCameraTransform.Position = NewCameraOrbitOrigin + Vector3.Transform(NewRootMotionFollow_Position, NewMatrix_World) + 
                (Vector3.Transform(Vector3.Backward * NewOrbitCamDistance, rot));

            NewMatrix_World = Matrix.CreateScale(1, 1, -1);

            NewMatrix_Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfView),
                   1.0f * GFX.LastViewport.Width / GFX.LastViewport.Height,
                   NewNearClipDistance, NewFarClipDistance);

            NewMatrix_View = Matrix.CreateTranslation(-NewCameraTransform.Position)
                * Matrix.CreateFromQuaternion(Quaternion.Inverse(NewCameraTransform.Rotation));

            NewMatrix_View_Skybox = Matrix.CreateFromQuaternion(Quaternion.Inverse(NewCameraTransform.Rotation));

            
        }

        public void NewUpdateInput(FancyInputHandler input)
        {
            if (DisableAllInput || OSD.Focused)
                return;

            if (input.LeftClickHeld && Main.TAE_EDITOR.ModelViewerBounds.Contains(input.LeftClickDownAnchor))
            {
                NewCameraEuler += new Vector3(-input.MousePositionDelta.Y, -input.MousePositionDelta.X, 0) * CameraTurnSpeedMouse * 0.01f;
            }
            else if (input.RightClickHeld && Main.TAE_EDITOR.ModelViewerBounds.Contains(input.RightClickDownAnchor))
            {
                NewCameraOrbitOrigin += Vector3.Transform(
                    new Vector3(-input.MousePositionDelta.X, input.MousePositionDelta.Y, 0),
                    NewCameraTransform.Rotation) 
                    * CameraMoveSpeed * 0.0015f * NewOrbitCamDistance;
            }
            else if (Main.TAE_EDITOR.ModelViewerBounds.Contains(input.MousePositionPoint))
            {
                if (input.MiddleClickDown)
                {
                    NewRecenter();
                }
                else
                {
                    NewOrbitCamDistance -= input.ScrollDelta;
                    if (NewOrbitCamDistance < 0.05f)
                    {
                        float distToUnfuck = Math.Abs(NewOrbitCamDistance - 0.1f);
                        NewCameraOrbitOrigin += Vector3.Transform(
                        new Vector3(0, 0, -distToUnfuck),
                        NewCameraTransform.Rotation);
                        NewOrbitCamDistance = 0.05f;
                    }
                }

                
            }
        }

        public Vector3 NewGetScreenSpaceUpVector()
        {
            return Vector3.TransformNormal(Vector3.Up, Matrix.CreateFromQuaternion(NewCameraTransform.Rotation));
        }

        public Vector3 NewGetScreenSpaceForwardVector()
        {
            return Vector3.TransformNormal(Vector3.Forward, Matrix.CreateFromQuaternion(NewCameraTransform.Rotation));
        }

        public void ApplyViewToShader_Skybox<T>(IGFXShader<T> shader)
           where T : Effect
        {
            Matrix m = Matrix.Identity;

            //if (TaeInterop.CameraFollowsRootMotion)
            //    m *= Matrix.CreateTranslation(-TaeInterop.CurrentRootMotionDisplacement.XYZ());

            shader.ApplyWorldView(m * NewMatrix_World, NewMatrix_View_Skybox, NewMatrix_Projection);
        }

        public void ApplyViewToShader<T>(IGFXShader<T> shader, Matrix modelMatrix)
            where T : Effect
        {
            shader.ApplyWorldView(modelMatrix * NewMatrix_World, NewMatrix_View, NewMatrix_Projection);
        }

        public void ApplyViewToShader<T>(IGFXShader<T> shader, Transform modelTransform)
            where T : Effect
        {
            ApplyViewToShader(shader, modelTransform.WorldMatrix);
        }



        public Action NewDoRecenterAction = null;

        public void NewRecenter()
        {
            NewCameraEuler = Vector3.Zero;
            NewCameraOrbitOrigin 
                = OrbitCamCenter_DummyPolyFollowRefPoint 
                = OrbitCamCenter_DummyPolyFollowRefPoint_Init;
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

        public float CameraTurnSpeedGamepad = 0.15f;
        public float CameraTurnSpeedMouse = 1;
        public float CameraMoveSpeed = 1;

        public Ray GetScreenRay(Vector2 screenPos)
        {
            var a = GFX.Device.Viewport.Unproject(
                new Vector3(screenPos, 0.1f),
                NewMatrix_Projection, NewMatrix_View, NewMatrix_World);

            var b = GFX.Device.Viewport.Unproject(
                new Vector3(screenPos, 0.2f),
                NewMatrix_Projection, NewMatrix_View, NewMatrix_World);

            return new Ray(a, Vector3.Normalize(b - a));
        }

        
    }
}
