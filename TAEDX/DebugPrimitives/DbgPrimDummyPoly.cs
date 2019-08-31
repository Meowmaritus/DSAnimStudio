using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.DebugPrimitives
{
    public class DbgPrimDummyPoly : DbgPrimWire
    {
        public FLVER2.Dummy DummyPoly;
        public float RenderSize = 1.0f;

        private float _helperSize;
        public float HelperSize
        {
            get => _helperSize;
            set
            {
                if (value > 0)
                {
                    if (!Children.Contains(Helper))
                        Children.Add(Helper);
                }
                else if (value <= 0)
                {
                    if (Children.Contains(Helper))
                        Children.Remove(Helper);
                }

                _helperSize = value;
            }
        }

        public DbgPrimWireSphere Helper;

        public void UpdateHelper()
        {
            Helper.Transform = new Transform(Matrix.CreateScale(HelperSize) * Transform.WorldMatrix);
        }

        public Matrix DummyPolyMatrix => 
            Matrix.CreateScale(RenderSize) *
            Matrix.CreateLookAt(
            Vector3.Zero,
            new Vector3(DummyPoly.Forward.X, DummyPoly.Forward.Y, DummyPoly.Forward.Z),
            DummyPoly.UseUpwardVector ? new Vector3(DummyPoly.Upward.X, DummyPoly.Upward.Y, DummyPoly.Upward.Z) : Vector3.Up)
            * Matrix.CreateTranslation(new Vector3(DummyPoly.Position.X, DummyPoly.Position.Y, DummyPoly.Position.Z));

        protected override void PreDraw(GameTime gameTime)
        {
            if (HelperSize > 0.05f)
                HelperSize *= 0.25f;
            else
                HelperSize = 0;

            //Test
            //HelperSize = 0.25f;

            UpdateHelper();
        }

        public DbgPrimDummyPoly(FLVER2.Dummy dummy, float size)
        {
            RenderSize = size;
            NameColor = new Color(dummy.Color.R, dummy.Color.G, dummy.Color.B, (byte)255);
            Name = dummy.ReferenceID.ToString();
            DummyPoly = dummy;
            Category = DbgPrimCategory.DummyPoly;
            Transform = new Transform(DummyPolyMatrix);

            Helper = new DbgPrimWireSphere(new Transform(Matrix.CreateScale(HelperSize) * DummyPolyMatrix), 1.0f, 2, 4, Color.Cyan);
            Helper.Category = DbgPrimCategory.DummyPolyHelper;

            float forwardLength = dummy.Forward.Length();
            AddLine(Vector3.Zero, Vector3.Up * forwardLength * 1.5f, Color.Lime);
            AddLine(Vector3.Zero, Vector3.Forward * forwardLength * 1.5f, Color.Blue);
            AddLine(Vector3.Zero, Vector3.Left * forwardLength * 1.5f, Color.Red);

            Children.Add(Helper);

        }
    }
}
