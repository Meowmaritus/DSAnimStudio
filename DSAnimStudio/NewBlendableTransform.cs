using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public struct NewBlendableTransform
    {
        public Vector3 Translation;
        public Vector3 Scale;
        public Quaternion Rotation;

        public static NewBlendableTransform Identity => new NewBlendableTransform()
        {
            Translation = Vector3.Zero,
            Rotation = Quaternion.Identity,
            Scale = Vector3.One,
        };

        public static NewBlendableTransform Lerp(float lerp, NewBlendableTransform a, NewBlendableTransform b)
        {
            float posX = MathHelper.Lerp(a.Translation.X, b.Translation.X, lerp);
            float posY = MathHelper.Lerp(a.Translation.Y, b.Translation.Y, lerp);
            float posZ = MathHelper.Lerp(a.Translation.Z, b.Translation.Z, lerp);

            float scaleX = MathHelper.Lerp(a.Scale.X, b.Scale.X, lerp);
            float scaleY = MathHelper.Lerp(a.Scale.Y, b.Scale.Y, lerp);
            float scaleZ = MathHelper.Lerp(a.Scale.Z, b.Scale.Z, lerp);

            float rotationX = MathHelper.Lerp(a.Rotation.X, b.Rotation.X, lerp);
            float rotationY = MathHelper.Lerp(a.Rotation.Y, b.Rotation.Y, lerp);
            float rotationZ = MathHelper.Lerp(a.Rotation.Z, b.Rotation.Z, lerp);
            float rotationW = MathHelper.Lerp(a.Rotation.W, b.Rotation.W, lerp);

            return new NewBlendableTransform()
            {
                Translation = new Vector3(posX, posY, posZ),
                Scale = new Vector3(scaleX, scaleY, scaleZ),
                Rotation = new Quaternion(rotationX, rotationY, rotationZ, rotationW),
            };
        }

        public Matrix GetMatrixScale()
        {
            return Matrix.CreateScale(Scale);
        }

        public Matrix GetMatrix()
        {
            return 
                //Matrix.CreateScale(Scale) *
                Matrix.CreateFromQuaternion(Quaternion.Normalize(Rotation)) *
                //Matrix.CreateFromQuaternion(Rotation) *
                Matrix.CreateTranslation(Translation);
        }
    }
}
