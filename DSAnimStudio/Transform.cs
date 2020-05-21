using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public struct Transform
    {
        public static readonly Transform Default
            = new Transform(Vector3.Zero, Vector3.Zero, Vector3.One);

        public Transform(Vector3 pos, Vector3 rot)
        {
            Position = pos;
            EulerRotation = rot;
            Scale = Vector3.One;
            OverrideMatrixWorld = Matrix.Identity;
            CameraViewMatrixModification = Matrix.Identity;
        }

        public Transform(Vector3 pos, Vector3 rot, Vector3 scale)
        {
            Position = pos;
            EulerRotation = rot;
            Scale = scale;
            OverrideMatrixWorld = Matrix.Identity;
            CameraViewMatrixModification = Matrix.Identity;
        }

        public Transform(Matrix overrideMatrixWorld)
        {
            Position = Vector3.Zero;
            EulerRotation = Vector3.Zero;
            Scale = Vector3.One;
            OverrideMatrixWorld = overrideMatrixWorld;
            CameraViewMatrixModification = Matrix.Identity;
        }

        public Transform(float x, float y, float z, float rx, float ry, float rz)
            : this(new Vector3(x, y, z), new Vector3(rx, ry, rz))
        {

        }

        public Transform(float x, float y, float z, float rx, float ry, float rz, float sx, float sy, float sz)
            : this(new Vector3(x, y, z), new Vector3(rx, ry, rz), new Vector3(sx, sy, sz))
        {

        }

        public Vector3 Position;
        public Vector3 EulerRotation;
        public Vector3 Scale;

        public Matrix ScaleMatrix => Matrix.CreateScale(Scale);

        public Matrix TranslationMatrix => Matrix.CreateTranslation(Position.X, Position.Y, Position.Z);
        public Matrix RotationMatrix => Matrix.CreateRotationY(EulerRotation.Y)
            * Matrix.CreateRotationZ(EulerRotation.Z)
            * Matrix.CreateRotationX(EulerRotation.X);

        public Matrix RotationMatrixNeg => Matrix.CreateRotationY(-EulerRotation.Y)
           * Matrix.CreateRotationZ(-EulerRotation.Z)
           * Matrix.CreateRotationX(-EulerRotation.X);

        public Matrix RotationMatrixXYZ => Matrix.CreateRotationX(EulerRotation.X)
            * Matrix.CreateRotationY(EulerRotation.Y)
            * Matrix.CreateRotationZ(EulerRotation.Z);

        
        public Matrix WorldMatrix => OverrideMatrixWorld != Matrix.Identity ? OverrideMatrixWorld : ScaleMatrix * RotationMatrix * TranslationMatrix;

        public Matrix CameraViewMatrixModification;

        public Matrix CameraViewMatrix => Matrix.CreateTranslation(-Position.X, -Position.Y, -Position.Z)
            * Matrix.CreateRotationY(EulerRotation.Y)
            * Matrix.CreateRotationZ(EulerRotation.Z)
            * Matrix.CreateRotationX(EulerRotation.X) * CameraViewMatrixModification;

        private static Random rand = new Random();
        public static Transform RandomUnit(bool randomRot = false)
        {
            float randFloat() => (float)((rand.NextDouble() * 2) - 1);
            return new Transform(randFloat(), randFloat(), randFloat(),
                randomRot ? (randFloat() * MathHelper.PiOver2) : 0,
                randomRot ? (randFloat() * MathHelper.PiOver2) : 0,
                randomRot ? (randFloat() * MathHelper.PiOver2) : 0);
        }

        public Matrix OverrideMatrixWorld;

        public override string ToString()
        {
            return $"Pos: {Position.ToString()} Rot (deg): {EulerRotation.Rad2Deg().ToString()}";
        }

        public static Transform operator *(Transform a, float b)
        {
            return new Transform(a.Position * b, a.EulerRotation);
        }

        public static Transform operator /(Transform a, float b)
        {
            return new Transform(a.Position / b, a.EulerRotation);
        }

        public static Transform operator +(Transform a, Vector3 b)
        {
            return new Transform(a.Position + b, a.EulerRotation);
        }

        public static Transform operator -(Transform a, Vector3 b)
        {
            return new Transform(a.Position - b, a.EulerRotation);
        }

        public static implicit operator Transform(Vector3 v)
        {
            return new Transform(v, Vector3.Zero);
        }
    }
}
