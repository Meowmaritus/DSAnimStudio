using DSAnimStudio.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public enum DbgPrimCategory
    {
        AlwaysDraw,
        HkxBone,
        FlverBone,
        FlverBoneBoundingBox,
        DummyPoly,
        SoundEvent,
        WeaponDummyPoly,
        DummyPolyHelper,
        Skybox,
        DummyPolySpawnArrow,
        Other,
        RootMotionPath,
    }

    public class DbgLabel
    {
        public Matrix World = Matrix.Identity;
        public float Height = 1;
        public string Text = "?LabelText?";
        public Color Color;

        public DbgLabel(Matrix world, float height, string text, Color color)
        {
            World = world;
            Height = height;
            Text = text;
            Color = color;
        }
    }

    public abstract class DbgPrim<T> : IDbgPrim
        where T : Effect
    {
        public Transform Transform { get; set; } = Transform.Default;
        public string Name { get; set; }
        public Color NameColor { get; set; } = Color.Yellow;

        public Color? OverrideColor { get; set; } = null;

        public DbgPrimCategory Category { get; set; } = DbgPrimCategory.Other;

        private List<DbgLabel> DbgLabels = new List<DbgLabel>();

        public object ExtraData { get; set; } = null;

        public bool EnableDraw { get; set; } = true;
        public bool EnableDbgLabelDraw { get; set; } = true;
        public bool EnableNameDraw { get; set; } = true;

        public List<IDbgPrim> Children { get; set; } = new List<IDbgPrim>();
        public List<IDbgPrim> UnparentedChildren { get; set; } = new List<IDbgPrim>();

        protected int[] Indices = new int[0];
        protected VertexPositionColorNormal[] Vertices = new VertexPositionColorNormal[0];
        protected VertexBuffer VertBuffer;
        protected IndexBuffer IndexBuffer;
        protected bool NeedToRecreateVertBuffer = true;
        protected bool NeedToRecreateIndexBuffer = true;

        protected abstract PrimitiveType PrimType { get; }

        public bool BackfaceCulling = true;
        public bool Wireframe = false;
        public bool DisableLighting = false;

        public int VertexCount => Vertices.Length;
        public int IndexCount => Indices.Length;

        public ParamData.AtkParam.DummyPolySource DmyPolySource { get; set; } = ParamData.AtkParam.DummyPolySource.Body;

        protected void SetBuffers(VertexBuffer vertBuffer, IndexBuffer indexBuffer)
        {
            VertBuffer = vertBuffer;
            IndexBuffer = indexBuffer;
            NeedToRecreateIndexBuffer = false;
            NeedToRecreateVertBuffer = false;
        }

        protected void FinalizeBuffers(bool force = false)
        {
            if (force || NeedToRecreateVertBuffer)
            {
                VertBuffer?.Dispose();
                VertBuffer = null;
                if (Vertices.Length > 0)
                {
                    VertBuffer = new VertexBuffer(GFX.Device,
                    typeof(VertexPositionColorNormal), Vertices.Length, BufferUsage.WriteOnly);
                    VertBuffer.SetData(Vertices);
                }
                NeedToRecreateVertBuffer = false;
            }

            if (force || NeedToRecreateIndexBuffer)
            {
                IndexBuffer?.Dispose();
                IndexBuffer = null;
                if (Indices.Length > 0)
                {
                    IndexBuffer = new IndexBuffer(GFX.Device, IndexElementSize.ThirtyTwoBits, Indices.Length, BufferUsage.WriteOnly);
                    IndexBuffer.SetData(Indices);
                }
                NeedToRecreateIndexBuffer = false;
            }
        }

        protected void AddVertex(Vector3 pos, Color color, Vector3? normal = null)
        {
            Array.Resize(ref Vertices, Vertices.Length + 1);
            Vertices[Vertices.Length - 1].Position = pos;
            Vertices[Vertices.Length - 1].Color = color;
            Vertices[Vertices.Length - 1].Normal = normal ?? Vector3.Forward;

            NeedToRecreateVertBuffer = true;
        }

        protected void AddVertex(VertexPositionColorNormal vert)
        {
            Array.Resize(ref Vertices, Vertices.Length + 1);
            Vertices[Vertices.Length - 1] = vert;

            NeedToRecreateVertBuffer = true;
        }

        protected void AddIndex(int index)
        {
            Array.Resize(ref Indices, Indices.Length + 1);
            Indices[Indices.Length - 1] = index;
            NeedToRecreateIndexBuffer = true;
        }

        public void AddDbgLabel(Vector3 position, float height, string text, Color color)
        {
            DbgLabels.Add(new DbgLabel(Matrix.CreateTranslation(position), height, text, color));
        }

        public void AddDbgLabel(Matrix world, float height, string text, Color color)
        {
            DbgLabels.Add(new DbgLabel(world, height, text, color));
        }

        public abstract IGFXShader<T> Shader { get; }

        public abstract DbgPrim<T> Instantiate(string newName, Transform newLocation, Color? newNameColor = null);

        /// <summary>
        /// Set this to choose specific technique(s).
        /// Null to just use the current technique.
        /// </summary>
        //public virtual string[] ShaderTechniquesSelection => null;

        private void DrawPrimitive()
        {
            FinalizeBuffers();

            if (VertBuffer == null || IndexBuffer == null || VertBuffer.VertexCount == 0 || IndexBuffer.IndexCount == 0)
            {
                // This is some dummy parent thing with no geometry.
                if (Children.Count > 0 || UnparentedChildren.Count > 0)
                    return;

                //If it's NOT a parent thing, then it shouldn't have empty geometry.
                // Some mistake was made.
                throw new Exception("DbgPrim geometry is empty and it had no children. Something went wrong...");
            }

            GFX.Device.SetVertexBuffer(VertBuffer);
            GFX.Device.Indices = IndexBuffer;
            GFX.BackfaceCulling = BackfaceCulling;
            GFX.Wireframe = Wireframe;

            if (Shader is DbgPrimSolidShader solid)
                solid.Effect.LightingEnabled = !DisableLighting;

            GFX.Device.DrawIndexedPrimitives(PrimType, 0, 0, IndexBuffer.IndexCount);
        }

        protected virtual void PreDraw()
        {

        }

        public void Draw(IDbgPrim parentPrim, Matrix world)
        {
            PreDraw();

            // Always draw unparented children :fatcat:
            foreach (var c in UnparentedChildren)
                c.Draw(this, world);

            if (!EnableDraw)
                return;

            if (Category != DbgPrimCategory.AlwaysDraw && !DBG.GetCategoryEnableDraw(Category))
                return;

            if (Shader == GFX.DbgPrimSolidShader || Shader == GFX.DbgPrimWireShader)
            {
                if (OverrideColor.HasValue)
                {
                    var overrideColor = new Vector3(OverrideColor.Value.R / 255f, OverrideColor.Value.G / 255f, OverrideColor.Value.B / 255f);
                    if (Shader is DbgPrimSolidShader solid)
                    {
                        solid.VertexColorEnabled = false;
                        solid.DiffuseColor = overrideColor;
                        solid.Alpha = OverrideColor.Value.A / 255f;
                    }
                    else if (Shader is DbgPrimWireShader wire)
                    {
                        wire.VertexColorEnabled = false;
                        wire.DiffuseColor = overrideColor;
                        wire.Alpha = OverrideColor.Value.A / 255f;
                    }
                }
                else
                {
                    if (Shader is DbgPrimSolidShader solid)
                    {
                        solid.VertexColorEnabled = true;
                        solid.DiffuseColor = Vector3.One;
                        solid.Alpha = 1;
                    }
                    else if (Shader is DbgPrimWireShader wire)
                    {
                        wire.VertexColorEnabled = true;
                        wire.DiffuseColor = Vector3.One;
                        wire.Alpha = 1;
                    }
                }
            }

            var effect = Shader.Effect;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                if (Shader == GFX.DbgPrimSolidShader || Shader == GFX.DbgPrimWireShader)
                    GFX.CurrentWorldView.ApplyViewToShader(Shader, Transform.WorldMatrix * world);
                pass.Apply();
                DrawPrimitive();
            }

            if (Shader == GFX.DbgPrimSolidShader || Shader == GFX.DbgPrimWireShader)
            {
                if (Shader is DbgPrimSolidShader solid)
                {
                    solid.VertexColorEnabled = true;
                    solid.DiffuseColor = Vector3.One * 2;
                }
                else if (Shader is DbgPrimWireShader wire)
                {
                    wire.VertexColorEnabled = true;
                    wire.DiffuseColor = Vector3.One * 2;
                }
            }

            foreach (var c in Children)
                c.Draw(this, Transform.WorldMatrix * world);
        }

        public void LabelDraw(Matrix world)
        {
            // Always draw unparented children :fatcat:
            foreach (var c in UnparentedChildren)
                c.LabelDraw(world);

            if (!(EnableDbgLabelDraw && DBG.GetCategoryEnableDbgLabelDraw(Category)))
                return;

            if (DbgLabels.Count > 0)
            {
                foreach (var label in DbgLabels)
                {
                    DBG.DrawTextOn3DLocation_FixedPixelSize(label.World * Transform.WorldMatrix * world, Vector3.Zero,
                        label.Text, label.Color, label.Height * 1.5f, startAndEndSpriteBatchForMe: false);
                }
            }

            foreach (var c in Children)
                c.LabelDraw(Transform.WorldMatrix * world);
        }

        public void LabelDraw_Billboard(Matrix world)
        {
            // Always draw unparented children :fatcat:
            foreach (var c in UnparentedChildren)
                c.LabelDraw_Billboard(world);

            if (!(EnableDbgLabelDraw && DBG.GetCategoryEnableDbgLabelDraw(Category)))
                return;

            if (DbgLabels.Count > 0)
            {
                foreach (var label in DbgLabels.OrderByDescending(lbl => (GFX.CurrentWorldView.CameraLocationInWorld.Position - Vector3.Transform(Vector3.Zero, lbl.World)).LengthSquared()))
                {
                    DBG.Draw3DBillboard(label.Text, label.World * Transform.WorldMatrix * world, label.Color);
                }
            }

            foreach (var c in Children)
                c.LabelDraw_Billboard(Transform.WorldMatrix * world);
        }

        protected abstract void DisposeBuffers();

        public void Dispose()
        {
            DisposeBuffers();
        }
    }
}
