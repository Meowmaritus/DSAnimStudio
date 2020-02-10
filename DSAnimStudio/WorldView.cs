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
        public bool DisableAllInput = false;

        public Transform CameraTransform = Transform.Default;
        public Transform CameraOrigin = Transform.Default;
        public Transform CameraPositionDefault = Transform.Default;
        public float OrbitCamDistance = 2;
        public float ModelHeight_ForOrbitCam = 1;
        public float ModelDepth_ForOrbitCam = 1;
        public Vector3 ModelCenter_ForOrbitCam = Vector3.Zero;
        public Vector3 OrbitCamCenter = new Vector3(0, 0.5f, 0);
        public bool IsOrbitCam = true;

        public Matrix WorldMatrixMOD = Matrix.Identity;

        private float ViewportAspectRatio => 1.0f * GFX.LastViewport.Width / GFX.LastViewport.Height;

        public void OrbitCamReset()
        {
            var distDetermine = Math.Max(ModelHeight_ForOrbitCam, ModelDepth_ForOrbitCam);
            if (ViewportAspectRatio < 1)
                OrbitCamDistance = (float)Math.Sqrt((distDetermine * 2) / (ViewportAspectRatio * 0.66f));
            else
                OrbitCamDistance = (float)Math.Sqrt(distDetermine * 2);

            OrbitCamCenter = new Vector3(0, ModelCenter_ForOrbitCam.Y, 0);

            CameraTransform.EulerRotation = CameraDefaultRot;
        }


        public static int BGUpdateWaitMS = 16;
        private Thread BGUpdateThread = null;
        public WorldView()
        {
            //BGUpdateThread = new Thread(new ThreadStart(new Action(() =>
            //{
            //    var stopwatch = new System.Diagnostics.Stopwatch();
            //    float lastFrame = 0;
            //    while (true)
            //    {
            //        stopwatch.Restart();
            //        UpdateInput(Program.MainInstance, 0.0166667f);
            //        Thread.Sleep(BGUpdateWaitMS);
            //        stopwatch.Stop();
            //        lastFrame = (float)stopwatch.Elapsed.TotalSeconds;
            //    }
                
            //})));

            //BGUpdateThread.IsBackground = true;
            //BGUpdateThread.Start();
        }


        public float LightRotationH = 0;
        public float LightRotationV = 0;
        public Vector3 LightDirectionVector
        {
            get
            {
                var horiz = Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(LightRotationH));
                float y = (float)Math.Sin(LightRotationV);
                return new Vector3(horiz.X, y, horiz.Z);
            }
        }

        public Matrix MatrixWorld;
        public Matrix MatrixProjection;

        public float FieldOfView = 43;
        public float NearClipDistance = 0.1f;
        public float FarClipDistance = 10000;
        public float CameraTurnSpeedGamepad = 0.15f;
        public float CameraTurnSpeedMouse = 1;
        public float CameraMoveSpeed = 1;

        public static readonly Vector3 CameraDefaultPos = new Vector3(0, 0.25f, -5);
        public static readonly Vector3 CameraDefaultRot = new Vector3(0, 0, 0);

        public void ResetCameraLocation()
        {
            CameraTransform.Position = CameraDefaultPos;
            CameraTransform.EulerRotation = CameraDefaultRot;
        }

        public void LookAtTransform(Transform t)
        {
            var newLookDir = Vector3.Normalize(t.Position - (CameraTransform.Position));
            CameraTransform.EulerRotation.Y = (float)Math.Atan2(-newLookDir.X, newLookDir.Z);
            CameraTransform.EulerRotation.X = (float)Math.Asin(newLookDir.Y);
            CameraTransform.EulerRotation.Z = 0;
        }

        public void GoToTransformAndLookAtIt(Transform t, float distance)
        {
            var positionOffset = Vector3.Transform(Vector3.Forward, t.RotationMatrix) * distance;
            CameraTransform.Position = t.Position + positionOffset;
            LookAtTransform(t);
        }

        public float GetDistanceSquaredFromCamera(Transform t)
        {
            return (t.Position - GetCameraPhysicalLocation().Position).LengthSquared();
        }

        public byte GetLOD(Transform modelTransform)
        {
            if (GFX.LODMode >= 0)
                return (byte)GFX.LODMode;
            else
            {
                var distSquared = GetDistanceSquaredFromCamera(modelTransform);
                if (distSquared >= (GFX.LOD2Distance * GFX.LOD2Distance))
                    return (byte)2;
                else if (distSquared >= (GFX.LOD1Distance * GFX.LOD1Distance))
                    return (byte)1;
                else
                    return (byte)0;
            }
        }

        //public void ApplyViewToShader<T>(IGFXShader<T> shader)
        //    where T : Effect
        //{
        //    Matrix m = Matrix.Identity;

        //    if (TaeInterop.CameraFollowsRootMotion)
        //        m *= Matrix.CreateTranslation(-TaeInterop.CurrentRootMotionDisplacement.XYZ());

        //    shader.ApplyWorldView(m, CameraTransform.CameraViewMatrix * Matrix.Invert(MatrixWorld), MatrixProjection);
        //}

        public void ApplyViewToShader_Skybox<T>(IGFXShader<T> shader)
            where T : Effect
        {
            Matrix m = Matrix.Identity;

            //if (TaeInterop.CameraFollowsRootMotion)
            //    m *= Matrix.CreateTranslation(-TaeInterop.CurrentRootMotionDisplacement.XYZ());

            shader.ApplyWorldView(m * WorldMatrixMOD, CameraTransform.RotationMatrixNeg * Matrix.CreateScale(-1), MatrixProjection);
        }

        public void ApplyViewToShader<T>(IGFXShader<T> shader, Matrix modelMatrix)
            where T : Effect
        {
            shader.ApplyWorldView(modelMatrix * WorldMatrixMOD, CameraTransform.CameraViewMatrix * Matrix.Invert(MatrixWorld), MatrixProjection);
        }

        public void ApplyViewToShader<T>(IGFXShader<T> shader, Transform modelTransform)
            where T : Effect
        {
            ApplyViewToShader(shader, modelTransform.WorldMatrix);
        }

        public bool IsInFrustum(BoundingBox objBounds, Transform objTransform)
        {
            if (!GFX.EnableFrustumCulling)
                return true;
            return new BoundingFrustum(CameraTransform.CameraViewMatrix * MatrixProjection)
                .Intersects(new BoundingBox(
                    Vector3.Transform(objBounds.Min, objTransform.WorldMatrix),
                    Vector3.Transform(objBounds.Max, objTransform.WorldMatrix)
                    ));
        }

        public Vector3 ROUGH_GetPointOnFloor(Vector3 pos, Vector3 dir, float stepDist)
        {
            Vector3 result = pos;
            Vector3 nDir = Vector3.Normalize(dir);
            while (result.Y > 0)
            {
                if (result.Y >= 1)
                    result += nDir * 1;
                else
                    result += nDir * stepDist;
            }
            result.Y = 0;
            return result;
        }

        public Transform GetSpawnPointFromScreenPos(Vector2 screenPos, float distance, bool faceBackwards, bool lockPitch, bool alignToFloor)
        {
            var result = Transform.Default;
            var point1 = GFX.Device.Viewport.Unproject(
                new Vector3(screenPos, 0),
                MatrixProjection, CameraTransform.CameraViewMatrix, MatrixWorld);

            var point2 = GFX.Device.Viewport.Unproject(
                new Vector3(screenPos, 0.5f),
                MatrixProjection, CameraTransform.CameraViewMatrix, MatrixWorld);



            var directionVector = Vector3.Normalize(point2 - point1);

            //If align to floor is requested, the camera is looking downward, and the camera is above the floor
            if (alignToFloor && directionVector.Y < 0 && point1.Y > 0)
            {
                result.Position = ROUGH_GetPointOnFloor(point1, directionVector, 0.05f);
            }
            else
            {
                result.Position = point1 + (directionVector * distance);
            }

            if (faceBackwards)
                directionVector = -directionVector;

            result.EulerRotation.Y = (float)Math.Atan2(directionVector.X, directionVector.Z);
            result.EulerRotation.X = lockPitch ? 0 : (float)Math.Asin(directionVector.Y);
            result.EulerRotation.Z = 0;

            return result;
        }

        public Transform GetSpawnPointInFrontOfCamera(float distance, bool faceBackwards, bool lockPitch, bool alignToFloor)
        {
            return GetSpawnPointFromScreenPos(new Vector2(GFX.Device.Viewport.Width * 0.5f, GFX.Device.Viewport.Height * 0.5f),
                distance, faceBackwards, lockPitch, alignToFloor);
        }

        public Transform GetSpawnPointFromMouseCursor(float distance, bool faceBackwards, bool lockPitch, bool alignToFloor)
        {
            var mouse = Mouse.GetState();
            return GetSpawnPointFromScreenPos(mouse.Position.ToVector2() - GFX.Device.Viewport.Bounds.Location.ToVector2(),
                distance, faceBackwards, lockPitch, alignToFloor);
        }

        public Transform GetCameraPhysicalLocation()
        {
            var result = Transform.Default;
            var point1 = GFX.Device.Viewport.Unproject(
                new Vector3(GFX.Device.Viewport.Width * 0.5f,
                GFX.Device.Viewport.Height * 0.5f, 0),
                MatrixProjection, CameraTransform.CameraViewMatrix, MatrixWorld);

            var point2 = GFX.Device.Viewport.Unproject(
                new Vector3(GFX.Device.Viewport.Width * 0.5f,
                GFX.Device.Viewport.Height * 0.5f, 0.5f),
                MatrixProjection, CameraTransform.CameraViewMatrix, MatrixWorld);

            result.Position = point1;

            var directionVector = Vector3.Normalize(point2 - point1);
            result.EulerRotation.Y = (float)Math.Atan2(directionVector.X, directionVector.Z);
            result.EulerRotation.X = (float)Math.Asin(directionVector.Y);
            result.EulerRotation.Z = 0;

            return result;
        }

        public void SetCameraLocation(Vector3 pos, Vector3 rot)
        {
            CameraTransform.Position = pos;
            CameraTransform.EulerRotation = rot;
        }

        private float GetDistanceFromCam(Vector3 location)
        {
            return (location - ScreenPointToWorld(Vector2.One / 2)).Length();
        }

        public Vector3 ScreenPointToWorld(Vector2 screenPoint, float depth = 0)
        {
            return GFX.LastViewport.Unproject(
                new Vector3(GFX.LastViewport.Width * screenPoint.X, 
                GFX.LastViewport.Height * screenPoint.Y, depth), 
                MatrixProjection, CameraTransform.CameraViewMatrix, MatrixWorld);
        }

        public void UpdateMatrices(GraphicsDevice d)
        {
            MatrixWorld = Matrix.CreateRotationY(MathHelper.Pi)
                * Matrix.CreateTranslation(0, 0, 0)
                * Matrix.CreateScale(-1, 1, 1)
                // * Matrix.Invert(CameraOrigin.ViewMatrix)
                ;
            //if (Main.SceneRenderTarget != null)
            //{
            //    MatrixProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfView),
            //       (float)Main.SceneRenderTarget.Width / (float)Main.SceneRenderTarget.Height,
            //       NearClipDistance, FarClipDistance);
            //}

            MatrixProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfView),
                   ViewportAspectRatio,
                   NearClipDistance, FarClipDistance);

        }

        public void MoveCamera(float x, float y, float z, float speed)
        {
            CameraTransform.Position += Vector3.Transform(new Vector3(x, y, z),
                Matrix.CreateRotationX(-CameraTransform.EulerRotation.X)
                * Matrix.CreateRotationY(-CameraTransform.EulerRotation.Y)
                * Matrix.CreateRotationZ(-CameraTransform.EulerRotation.Z)
                ) * speed;
        }

        public void RotateCameraOrbit(float h, float v, float speed)
        {
            CameraTransform.EulerRotation.Y -= h * speed;
            CameraTransform.EulerRotation.X += v * speed;
            CameraTransform.EulerRotation.Z = 0;
        }

        

        public void MoveCamera_OrbitCenterPoint_MouseDelta(Vector2 curMouse, Vector2 oldMouse, float elapsed)
        {
            var curMouse3DX = GFX.LastViewport.Unproject(new Vector3(curMouse.X - GFX.LastViewport.X, GFX.LastViewport.Height / 2f, 0),
                GFX.World.MatrixProjection, GFX.World.CameraTransform.CameraViewMatrix * Matrix.Invert(GFX.World.MatrixWorld), Matrix.Identity);

            var curMouse3DY = GFX.LastViewport.Unproject(new Vector3(GFX.LastViewport.Width / 2f, curMouse.Y - GFX.LastViewport.Y, 0),
               GFX.World.MatrixProjection, GFX.World.CameraTransform.CameraViewMatrix * Matrix.Invert(GFX.World.MatrixWorld), Matrix.Identity);

            var oldMouse3DX = GFX.LastViewport.Unproject(new Vector3(oldMouse.X - GFX.LastViewport.X, GFX.LastViewport.Height / 2f, 0),
                GFX.World.MatrixProjection, GFX.World.CameraTransform.CameraViewMatrix * Matrix.Invert(GFX.World.MatrixWorld), Matrix.Identity);

            var oldMouse3DY = GFX.LastViewport.Unproject(new Vector3(GFX.LastViewport.Width / 2f, oldMouse.Y - GFX.LastViewport.Y, 0),
               GFX.World.MatrixProjection, GFX.World.CameraTransform.CameraViewMatrix * Matrix.Invert(GFX.World.MatrixWorld), Matrix.Identity);

            //int minDimension = Math.Min(GFX.Device.Viewport.Width, GFX.Device.Viewport.Height);

            //float hDist = (curMouse3DX - oldMouse3DX).Length() / GFX.Device.Viewport.Width * minDimension;
            //float vDist = (curMouse3DY - oldMouse3DY).Length() / GFX.Device.Viewport.Height * minDimension;

            float aspectRatio = 1.0f * GFX.LastViewport.Width / GFX.LastViewport.Height;

            float hDist = (curMouse3DX - oldMouse3DX).Length();
            float vDist = (curMouse3DY - oldMouse3DY).Length();

            if (aspectRatio < 1)
            {
                hDist /= aspectRatio;
            }

            bool isNegH = (curMouse.X - oldMouse.X) < 0;
            bool isNegV = (curMouse.Y - oldMouse.Y) < 0;

            MoveCamera_OrbitCenterPoint(-hDist * (isNegH ? -1 : 1), vDist * (isNegV ? -1 : 1), 0, 50 * 40 * elapsed * CameraMoveSpeed);
        }

        public void MoveCamera_OrbitCenterPoint(float x, float y, float z, float speed)
        {
            OrbitCamCenter += (Vector3.Transform(new Vector3(x, y, z),
                Matrix.CreateRotationX(-CameraTransform.EulerRotation.X)
                * Matrix.CreateRotationY(-CameraTransform.EulerRotation.Y)
                * Matrix.CreateRotationZ(-CameraTransform.EulerRotation.Z)
                ) * speed) * (OrbitCamDistance * OrbitCamDistance) * 0.5f;
        }

        public void PointCameraToLocation(Vector3 location)
        {
            var newLookDir = Vector3.Normalize(location - (CameraTransform.Position));
            CameraTransform.EulerRotation.Y = (float)Math.Atan2(newLookDir.X, newLookDir.Z);
            CameraTransform.EulerRotation.X = (float)Math.Asin(newLookDir.Y);
            CameraTransform.EulerRotation.Z = 0;
        }


        private Vector2 mousePos = Vector2.Zero;
        private Vector2 oldMouse = Vector2.Zero;
        private int oldWheel = 0;
        private bool currentMouseClickL = false;
        private bool currentMouseClickR = false;
        private bool currentMouseClickM = false;
        private bool currentMouseClickStartedInWindow = false;
        private bool oldMouseClickL = false;
        private bool oldMouseClickR = false;
        private bool oldMouseClickM = false;
        private MouseClickType currentClickType = MouseClickType.None;
        private MouseClickType oldClickType = MouseClickType.None;
        //軌道カムトグルキー押下
        bool oldOrbitCamToggleKeyPressed = false;
        //非常に悪いカメラピッチ制限    ファトキャット
        const float SHITTY_CAM_PITCH_LIMIT_FATCAT = 0.999f;
        //非常に悪いカメラピッチ制限リミッタ    ファトキャット
        const float SHITTY_CAM_PITCH_LIMIT_FATCAT_CLAMP = 0.999f;
        const float SHITTY_CAM_ZOOM_MIN_DIST = 0.2f;

        private bool oldResetKeyPressed = false;

        private float GetGamepadTriggerDeadzone(float t, float d)
        {
            if (t < d)
                return 0;
            else if (t >= 1)
                return 0;

            return (t - d) * (1.0f / (1.0f - d));
        }

        public enum MouseClickType
        {
            None,
            Left,
            Right,
            Middle,
            Extra1,
            Extra2,
        }

        public void UpdateInput(Main game, float elapsed)
        {
            if (game == null)
                return;
            //if (DisableAllInput)
            //{
            //    oldWheel = Mouse.GetState(game.Window).ScrollWheelValue;
                
            //    return;
            //}

            // Kinda lazy but I really don't wanna go replace instances of gameTime and shit
            //gameTime = new GameTime(Main.MeasuredTotalTime, Main.MeasuredElapsedTime);

            //if (GFX.TestLightSpin)
            //{
            //    LightRotation.Y += MathHelper.PiOver4 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //    LightRotation.X += MathHelper.PiOver4 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //}

            var gamepad = DBG.EnableGamePadInput ? GamePad.GetState(PlayerIndex.One) : DBG.DisabledGamePadState;

            MouseState mouse = DBG.EnableMouseInput ? Mouse.GetState(game.Window) : DBG.DisabledMouseState;

            //TODO: Figure out what I was thinking here??
            float clampedLerpF = 1;// MathHelper.Clamp(30 * elapsed, 0, 1);

            mousePos = new Vector2(MathHelper.Lerp(oldMouse.X, mouse.X, clampedLerpF),
                MathHelper.Lerp(oldMouse.Y, mouse.Y, clampedLerpF));

            

            KeyboardState keyboard = DBG.EnableKeyboardInput ? Keyboard.GetState() : DBG.DisabledKeyboardState;
            int currentWheel = mouse.ScrollWheelValue;

            if (DisableAllInput || OSD.Focused)
            {
                oldMouseClickL = false;
                oldMouseClickM = false;
                oldMouseClickR = false;

                if (mousePos.X != oldMouse.X || mousePos.Y != oldMouse.Y)
                    OSD.CancelTooltip();

                oldMouse = mousePos;
                oldClickType = MouseClickType.None;
                oldWheel = currentWheel;
                oldResetKeyPressed = false;
                oldOrbitCamToggleKeyPressed = false;
                return;
            }

            bool mouseInWindow = Main.Active && mousePos.X >= game.ClientBounds.Left && mousePos.X < game.ClientBounds.Right && mousePos.Y > game.ClientBounds.Top && mousePos.Y < game.ClientBounds.Bottom;

            currentClickType = MouseClickType.None;

            if (mouse.LeftButton == ButtonState.Pressed)
                currentClickType = MouseClickType.Left;
            else if (mouse.RightButton == ButtonState.Pressed)
                currentClickType = MouseClickType.Right;
            else if (mouse.MiddleButton == ButtonState.Pressed)
                currentClickType = MouseClickType.Middle;
            else if (mouse.XButton1 == ButtonState.Pressed)
                currentClickType = MouseClickType.Extra1;
            else if (mouse.XButton2 == ButtonState.Pressed)
                currentClickType = MouseClickType.Extra2;
            else
                currentClickType = MouseClickType.None;

            currentMouseClickL = currentClickType == MouseClickType.Left;
            currentMouseClickR = currentClickType == MouseClickType.Right;
            currentMouseClickM = currentClickType == MouseClickType.Middle;

            if (currentClickType != MouseClickType.None && oldClickType == MouseClickType.None)
                currentMouseClickStartedInWindow = mouseInWindow;

            if (currentClickType == MouseClickType.None)
            {
                // If nothing is pressed, just dont bother lerping
                mousePos = new Vector2(mouse.X, mouse.Y);
            }

            bool isSpeedupKeyPressed = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
            bool isSlowdownKeyPressed = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            bool isResetKeyPressed = keyboard.IsKeyDown(Keys.R);
            bool isMoveLightKeyPressed = keyboard.IsKeyDown(Keys.Space);
            bool isOrbitCamToggleKeyPressed = false;// keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F);
            bool isPointCamAtObjectKeyPressed = false;// keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.T);
            

            if (!currentMouseClickStartedInWindow)
            {
                oldMouse = mousePos;

                if (IsOrbitCam)
                {
                    //DEBUG("Dist:" + ORBIT_CAM_DISTANCE);
                    //DEBUG("AngX:" + CameraTransform.Rotation.X / MathHelper.Pi + " PI");
                    //DEBUG("AngY:" + CameraTransform.Rotation.Y / MathHelper.Pi + " PI");
                    //DEBUG("AngZ:" + CameraTransform.Rotation.Z / MathHelper.Pi + " PI");

                    CameraTransform.EulerRotation.X = MathHelper.Clamp(CameraTransform.EulerRotation.X, -MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT_CLAMP, MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT_CLAMP);

                    OrbitCamDistance = Math.Max(OrbitCamDistance, SHITTY_CAM_ZOOM_MIN_DIST);

                    var distanceVectorAfterMove = -Vector3.Transform(Vector3.Forward, CameraTransform.RotationMatrixXYZ * Matrix.CreateRotationY(MathHelper.Pi)) * new Vector3(-1, 1, 1);
                    CameraTransform.Position = (OrbitCamCenter + (distanceVectorAfterMove * (OrbitCamDistance * OrbitCamDistance)));

                    
                }
                else
                {
                    CameraTransform.EulerRotation.X = MathHelper.Clamp(CameraTransform.EulerRotation.X, -MathHelper.PiOver2, MathHelper.PiOver2);
                }

                //LightRotation.X = MathHelper.Clamp(LightRotation.X, -MathHelper.PiOver2, MathHelper.PiOver2);
                oldWheel = currentWheel;

                //prev_isToggleAllSubmeshKeyPressed = isToggleAllSubmeshKeyPressed;
                //prev_isToggleAllDummyKeyPressed = isToggleAllDummyKeyPressed;
                //prev_isToggleAllBonesKeyPressed = isToggleAllBonesKeyPressed;

                oldClickType = currentClickType;

                oldMouseClickL = currentMouseClickL;
                oldMouseClickR = currentMouseClickR;
                oldMouseClickM = currentMouseClickM;

                oldOrbitCamToggleKeyPressed = isOrbitCamToggleKeyPressed;

                return;
            }

            if (currentMouseClickM && !oldMouseClickM && IsOrbitCam)
            {
                OrbitCamReset();
            }

            if (currentClickType == MouseClickType.None && gamepad.IsConnected)
            {
                if (gamepad.IsButtonDown(Buttons.LeftShoulder))
                    isSlowdownKeyPressed = true;
                if (gamepad.IsButtonDown(Buttons.RightShoulder))
                    isSpeedupKeyPressed = true;
                if (gamepad.IsButtonDown(Buttons.RightStick))
                    isResetKeyPressed = true;
                if (gamepad.IsButtonDown(Buttons.LeftStick))
                    isMoveLightKeyPressed = true;
                //if (gamepad.IsButtonDown(Buttons.DPadDown))
                //    isOrbitCamToggleKeyPressed = true;
                //if (gamepad.IsButtonDown(Buttons.RightStick))
                //    isPointCamAtObjectKeyPressed = true;
            }

            

            if (isResetKeyPressed && !oldResetKeyPressed)
            {
                ResetCameraLocation();
            }

            oldResetKeyPressed = isResetKeyPressed;

            if (isOrbitCamToggleKeyPressed && !oldOrbitCamToggleKeyPressed)
            {
                if (!IsOrbitCam)
                {
                    CameraOrigin.Position.Y = CameraPositionDefault.Position.Y;
                    OrbitCamDistance = (CameraOrigin.Position - (CameraTransform.Position)).Length();
                }
                IsOrbitCam = !IsOrbitCam;
            }

            if (isPointCamAtObjectKeyPressed)
            {
                PointCameraToLocation(CameraPositionDefault.Position);
            }

            float moveMult = elapsed * CameraMoveSpeed;

            if (isSpeedupKeyPressed)
            {
                moveMult *= 10f;
            }

            if (isSlowdownKeyPressed)
            {
                moveMult /= 10f;
            }

            var cameraDist = CameraOrigin.Position - CameraTransform.Position;

            if (currentClickType == MouseClickType.None && gamepad.IsConnected)
            {
                var lt = GetGamepadTriggerDeadzone(gamepad.Triggers.Left, 0.1f);
                var rt = GetGamepadTriggerDeadzone(gamepad.Triggers.Right, 0.1f);


                if (IsOrbitCam && !isMoveLightKeyPressed)
                {
                    float camH = gamepad.ThumbSticks.Left.X * (float)1.5f * CameraTurnSpeedGamepad * elapsed;
                    float camV = gamepad.ThumbSticks.Left.Y * (float)1.5f * CameraTurnSpeedGamepad * elapsed;




                    //DEBUG($"{(CameraTransform.Rotation.X / MathHelper.PiOver2)}");
                    if (CameraTransform.EulerRotation.X >= MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT)
                    {
                        //DEBUG("UPPER CAM LIMIT");
                        camV = Math.Min(camV, 0);
                    }
                    if (CameraTransform.EulerRotation.X <= -MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT)
                    {
                        //DEBUG("LOWER CAM LIMIT");
                        camV = Math.Max(camV, 0);
                    }

                    RotateCameraOrbit(camH, camV, MathHelper.PiOver2);

                    var zoom = gamepad.Triggers.Right - gamepad.Triggers.Left;

                    if (Math.Abs(cameraDist.Length()) <= SHITTY_CAM_ZOOM_MIN_DIST)
                    {
                        zoom = Math.Min(zoom, 0);
                    }


                    OrbitCamDistance -= zoom * moveMult;




                    //PointCameraToModel();
                    MoveCamera_OrbitCenterPoint(gamepad.ThumbSticks.Right.X, gamepad.ThumbSticks.Right.Y, 0, moveMult);
                }
                else
                {
                    float camH = gamepad.ThumbSticks.Right.X * (float)1.5f * CameraTurnSpeedGamepad * elapsed;
                    float camV = gamepad.ThumbSticks.Right.Y * (float)1.5f * CameraTurnSpeedGamepad * elapsed;

                    if (isMoveLightKeyPressed)
                    {
                        LightRotationH += camH;
                        LightRotationV -= camV;
                    }
                    else
                    {
                        MoveCamera(gamepad.ThumbSticks.Left.X, gamepad.Triggers.Right - gamepad.Triggers.Left, gamepad.ThumbSticks.Left.Y, moveMult);



                        CameraTransform.EulerRotation.Y -= camH;
                        CameraTransform.EulerRotation.X += camV;
                    }
                }


            }




            if (IsOrbitCam)
            {
                if (currentMouseClickL)
                {
                    float x = 0;
                    float z = 0;
                    float y = 0;

                    if (keyboard.IsKeyDown(Keys.W) && Math.Abs(cameraDist.Length()) > 0.1f)
                        z += 1;
                    if (keyboard.IsKeyDown(Keys.S))
                        z -= 1;
                    if (keyboard.IsKeyDown(Keys.E))
                        y += 1;
                    if (keyboard.IsKeyDown(Keys.Q))
                        y -= 1;
                    if (keyboard.IsKeyDown(Keys.A))
                        x -= 1;
                    if (keyboard.IsKeyDown(Keys.D))
                        x += 1;


                    if (Math.Abs(cameraDist.Length()) <= SHITTY_CAM_ZOOM_MIN_DIST)
                    {
                        z = Math.Min(z, 0);
                    }

                    //OrbitCamDistance -= z * moveMult;

                    //MoveCamera_OrbitCenterPoint(x, y, 0, moveMult * 4);
                }
                else if (currentMouseClickR)
                {
                    MoveCamera_OrbitCenterPoint_MouseDelta(mousePos, oldMouse, elapsed);
                    //Vector2 mouseDelta = mousePos - oldMouse;
                    //MoveCamera_OrbitCenterPoint(-mouseDelta.X, mouseDelta.Y, 0, moveMult);
                }


                if (GFX.LastViewport.Bounds.Contains(mouse.Position))
                    OrbitCamDistance -= ((currentWheel - oldWheel) / 150f) * 0.25f;
                
            }
            else
            {
                float x = 0;
                float y = 0;
                float z = 0;

                if (keyboard.IsKeyDown(Keys.D))
                    x += 1;
                if (keyboard.IsKeyDown(Keys.A))
                    x -= 1;
                if (keyboard.IsKeyDown(Keys.E))
                    y += 1;
                if (keyboard.IsKeyDown(Keys.Q))
                    y -= 1;
                if (keyboard.IsKeyDown(Keys.W))
                    z += 1;
                if (keyboard.IsKeyDown(Keys.S))
                    z -= 1;

                MoveCamera(x, y, z, moveMult);
            }


            //if (isToggleAllSubmeshKeyPressed && !prev_isToggleAllSubmeshKeyPressed)
            //{
            //    game.ModelListWindow.TOGGLE_ALL_SUBMESH();
            //}

            //if (isToggleAllDummyKeyPressed && !prev_isToggleAllDummyKeyPressed)
            //{
            //    game.ModelListWindow.TOGGLE_ALL_DUMMY();
            //}

            //if (isToggleAllBonesKeyPressed && !prev_isToggleAllBonesKeyPressed)
            //{
            //    game.ModelListWindow.TOGGLE_ALL_BONES();
            //}

            if (currentMouseClickL)
            {
                if (!oldMouseClickL)
                {
                    //game.IsMouseVisible = false;
                    oldMouse = mousePos;
                    //Mouse.SetPosition(game.ClientBounds.X + game.ClientBounds.Width / 2, game.ClientBounds.Y + game.ClientBounds.Height / 2);
                    //mousePos = new Vector2(game.ClientBounds.X + game.ClientBounds.Width / 2, game.ClientBounds.Y + game.ClientBounds.Height / 2);
                    //oldMouseClick = true;
                    //return;
                }
                else
                {
                    //game.IsMouseVisible = false;
                    Vector2 mouseDelta = mousePos - oldMouse;

                    if (mouseDelta.LengthSquared() == 0)
                    {
                        // Prevents a meme
                        oldWheel = currentWheel;
                        return;
                    }

                    //Mouse.SetPosition(game.ClientBounds.X + game.ClientBounds.Width / 2, game.ClientBounds.Y + game.ClientBounds.Height / 2);

                    

                    float camH = mouseDelta.X * 1 * CameraTurnSpeedMouse * elapsed * 0.15f;
                    float camV = mouseDelta.Y * -1 * CameraTurnSpeedMouse * elapsed * 0.15f;

                    if (IsOrbitCam && !isMoveLightKeyPressed)
                    {
                        if (CameraTransform.EulerRotation.X >= MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT)
                        {
                            camV = Math.Min(camV, 0);
                        }
                        if (CameraTransform.EulerRotation.X <= -MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT)
                        {
                            camV = Math.Max(camV, 0);
                        }

                        RotateCameraOrbit(camH, camV, MathHelper.PiOver2);
                        //PointCameraToModel();
                    }
                    else if (isMoveLightKeyPressed)
                    {
                        LightRotationH += camH;
                        LightRotationV -= camV;
                    }
                    else
                    {
                        CameraTransform.EulerRotation.Y -= camH;
                        CameraTransform.EulerRotation.X += camV;
                    }
                }


                //CameraTransform.Rotation.Z -= (float)Math.Cos(MathHelper.PiOver2 - CameraTransform.Rotation.Y) * camV;

                //RotateCamera(mouseDelta.Y * -0.01f * (float)moveMult, 0, 0, moveMult);
                //RotateCamera(0, mouseDelta.X * 0.01f * (float)moveMult, 0, moveMult);
            }
            else
            {
                if (IsOrbitCam)
                {
                    RotateCameraOrbit(0, 0, MathHelper.PiOver2);
                }

                if (oldMouseClickL)
                {
                    //Mouse.SetPosition((int)oldMouse.X, (int)oldMouse.Y);
                }
                //game.IsMouseVisible = true;
            }


            if (IsOrbitCam)
            {
                //DEBUG("Dist:" + ORBIT_CAM_DISTANCE);
                //DEBUG("AngX:" + CameraTransform.Rotation.X / MathHelper.Pi + " PI");
                //DEBUG("AngY:" + CameraTransform.Rotation.Y / MathHelper.Pi + " PI");
                //DEBUG("AngZ:" + CameraTransform.Rotation.Z / MathHelper.Pi + " PI");

                CameraTransform.EulerRotation.X = MathHelper.Clamp(CameraTransform.EulerRotation.X, -MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT_CLAMP, MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT_CLAMP);

                OrbitCamDistance = Math.Max(OrbitCamDistance, SHITTY_CAM_ZOOM_MIN_DIST);

                var distanceVectorAfterMove = -Vector3.Transform(Vector3.Forward, CameraTransform.RotationMatrixXYZ * Matrix.CreateRotationY(MathHelper.Pi)) * new Vector3(-1, 1, 1);
                CameraTransform.Position = (OrbitCamCenter + (distanceVectorAfterMove * (OrbitCamDistance * OrbitCamDistance)));
            }
            else
            {
                CameraTransform.EulerRotation.X = MathHelper.Clamp(CameraTransform.EulerRotation.X, -MathHelper.PiOver2, MathHelper.PiOver2);
            }

            LightRotationV = MathHelper.Clamp(LightRotationV, -MathHelper.PiOver2, MathHelper.PiOver2);
            oldWheel = currentWheel;

            //prev_isToggleAllSubmeshKeyPressed = isToggleAllSubmeshKeyPressed;
            //prev_isToggleAllDummyKeyPressed = isToggleAllDummyKeyPressed;
            //prev_isToggleAllBonesKeyPressed = isToggleAllBonesKeyPressed;

            oldClickType = currentClickType;

            oldMouseClickL = currentMouseClickL;
            oldMouseClickR = currentMouseClickR;
            oldMouseClickM = currentMouseClickM;

            oldOrbitCamToggleKeyPressed = isOrbitCamToggleKeyPressed;

            oldMouse = mousePos;
        }
    }
}
