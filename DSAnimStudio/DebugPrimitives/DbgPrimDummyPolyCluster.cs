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

        Matrix GetDummyPolyMatrix(FLVER2.Dummy dummy)
        {
            return Matrix.CreateScale(RenderSize) *
            Matrix.CreateLookAt(
            Vector3.Zero,
            new Vector3(dummy.Forward.X, dummy.Forward.Y, dummy.Forward.Z),
            dummy.UseUpwardVector ? new Vector3(dummy.Upward.X, dummy.Upward.Y, dummy.Upward.Z) : Vector3.Up)
            * Matrix.CreateTranslation(new Vector3(dummy.Position.X, dummy.Position.Y, dummy.Position.Z));
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

        private void AddDummy(FLVER2.Dummy dummy)
        {
            float forwardLength = dummy.Forward.Length();
            var m = GetDummyPolyMatrix(dummy);
            AddLine(Vector3.Transform(Vector3.Zero, m), Vector3.Transform(Vector3.Up * forwardLength * 1.5f, m), Color.Lime);
            AddLine(Vector3.Transform(Vector3.Zero, m), Vector3.Transform(Vector3.Forward * forwardLength * 1.5f, m), Color.Blue);
            AddLine(Vector3.Transform(Vector3.Zero, m), Vector3.Transform(Vector3.Left * forwardLength * 1.5f, m), Color.Red);
        }

        public DbgPrimDummyPolyCluster(float size, List<FLVER2.Dummy> dummies)
        {
            RenderSize = size;
            //NameColor = new Color(dummy.Color.R, dummy.Color.G, dummy.Color.B, (byte)255);
            //Name = dummy.ReferenceID.ToString();
            DummyPoly = dummies;
            Category = DbgPrimCategory.DummyPoly;
            //Transform = new Transform(DummyPolyMatrix);

            foreach (var dmy in DummyPoly)
            {
                AddDummy(dmy);
                ID = dmy.ReferenceID;
            }
                
        }

        public void UpdateWithBoneMatrix(Matrix boneMatrix)
        {
            Transform = new Transform(boneMatrix);
        }
    }
}
