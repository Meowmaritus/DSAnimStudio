using Microsoft.Xna.Framework;
using Newtonsoft.Json;
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
            = new Transform(Vector3.Zero, Quaternion.Identity, Vector3.One);

        public Transform(Vector3 pos, Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
            Scale = Vector3.One;
            OverrideMatrixWorld = Matrix.Identity;
        }

        public Transform(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            Position = pos;
            Rotation = rot;
            Scale = scale;
            OverrideMatrixWorld = Matrix.Identity;
        }

        public Transform(Matrix overrideMatrixWorld)
        {
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
            OverrideMatrixWorld = overrideMatrixWorld;
        }

        public Transform(float x, float y, float z, float rx, float ry, float rz, float rw)
            : this(new Vector3(x, y, z), new Quaternion(rx, ry, rz, rw))
        {

        }

        public Transform(float x, float y, float z, float rx, float ry, float rz, float rw, float sx, float sy, float sz)
            : this(new Vector3(x, y, z), new Quaternion(rx, ry, rz, rw), new Vector3(sx, sy, sz))
        {

        }

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        [JsonIgnore]
        public Matrix ScaleMatrix => Matrix.CreateScale(Scale);

        [JsonIgnore]
        public Matrix TranslationMatrix => Matrix.CreateTranslation(Position.X, Position.Y, Position.Z);
        [JsonIgnore]
        public Matrix RotationMatrix => Matrix.CreateFromQuaternion(Rotation);

        [JsonIgnore]
        public Matrix WorldMatrix => OverrideMatrixWorld != Matrix.Identity ? 
            OverrideMatrixWorld : ScaleMatrix * RotationMatrix * TranslationMatrix;

        public Matrix OverrideMatrixWorld;

        public static implicit operator Transform(Vector3 v)
        {
            return new Transform(v, Quaternion.Identity);
        }
    }
}
