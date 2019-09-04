using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimDummyPolyCluster : DbgPrimWire
    {
        public List<FLVER2.Dummy> DummyPoly;
        private float RenderSize = 1.0f;

        public int ID { get; private set; }

        //private float _helperSize;
        //public float HelperSize
        //{
        //    get => _helperSize;
        //    set
        //    {
        //        if (value > 0)
        //        {
        //            if (!Children.Contains(Helper))
        //                Children.Add(Helper);
        //        }
        //        else if (value <= 0)
        //        {
        //            if (Children.Contains(Helper))
        //                Children.Remove(Helper);
        //        }

        //        _helperSize = value;
        //    }
        //}

        Matrix GetDummyPolyMatrix(FLVER2.Dummy dummy, List<FLVER2.Bone> bones)
        {
            var dummyMatrix = GetBoneParentMatrix(bones[dummy.DummyBoneIndex], bones);

            return Matrix.CreateScale(RenderSize) *
            Matrix.CreateLookAt(
            Vector3.Zero,
            new Vector3(dummy.Forward.X, dummy.Forward.Y, dummy.Forward.Z),
            dummy.UseUpwardVector ? new Vector3(dummy.Upward.X, dummy.Upward.Y, dummy.Upward.Z) : Vector3.Up)
            * Matrix.CreateTranslation(new Vector3(dummy.Position.X, dummy.Position.Y, dummy.Position.Z))
            * dummyMatrix;
        }

        protected override void PreDraw(GameTime gameTime)
        {
            //if (HelperSize > 0.05f)
            //    HelperSize *= 0.25f;
            //else
            //    HelperSize = 0;

            ////Test
            ////HelperSize = 0.25f;

            //UpdateHelper();
        }

        private Matrix GetBoneParentMatrix(SoulsFormats.FLVER2.Bone b, List<FLVER2.Bone> bones)
        {
            SoulsFormats.FLVER2.Bone parentBone = b;

            var result = Matrix.Identity;

            do
            {
                result *= Matrix.CreateScale(parentBone.Scale.X, parentBone.Scale.Y, parentBone.Scale.Z);
                result *= Matrix.CreateRotationX(parentBone.Rotation.X);
                result *= Matrix.CreateRotationZ(parentBone.Rotation.Z);
                result *= Matrix.CreateRotationY(parentBone.Rotation.Y);
                result *= Matrix.CreateTranslation(parentBone.Translation.X, parentBone.Translation.Y, parentBone.Translation.Z);

                if (parentBone.ParentIndex >= 0)
                {
                    parentBone = bones[parentBone.ParentIndex];
                }
                else
                {
                    parentBone = null;
                }
            }
            while (parentBone != null);

            return result;
        }

        private void AddDummy(FLVER2.Dummy dummy, List<FLVER2.Bone> bones)
        {
            float forwardLength = dummy.Forward.Length();
            var m = GetDummyPolyMatrix(dummy, bones);
            AddLine(Vector3.Transform(Vector3.Zero, m), Vector3.Transform(Vector3.Up * forwardLength * 1.5f, m), Color.Lime);
            AddLine(Vector3.Transform(Vector3.Zero, m), Vector3.Transform(Vector3.Forward * forwardLength * 1.5f, m), Color.Blue);
            AddLine(Vector3.Transform(Vector3.Zero, m), Vector3.Transform(Vector3.Left * forwardLength * 1.5f, m), Color.Red);
            AddDbgLabel(Vector3.Transform(Vector3.Zero, m), 0.25f, dummy.ReferenceID.ToString(), new Color(dummy.Color.R, dummy.Color.G, dummy.Color.B, dummy.Color.A));
        }

        public DbgPrimDummyPolyCluster(float size, List<FLVER2.Dummy> dummies, List<FLVER2.Bone> bones)
        {
            RenderSize = size;
            //NameColor = new Color(dummy.Color.R, dummy.Color.G, dummy.Color.B, (byte)255);
            //Name = dummy.ReferenceID.ToString();
            DummyPoly = dummies;
            Category = DbgPrimCategory.DummyPoly;
            //Transform = new Transform(DummyPolyMatrix);

            foreach (var dmy in DummyPoly)
            {
                AddDummy(dmy, bones);
                ID = dmy.ReferenceID;
            }
                
        }

        public void UpdateWithBoneMatrix(Matrix boneMatrix)
        {
            Transform = new Transform(boneMatrix);
        }
    }
}
