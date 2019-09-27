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
        HkxBone,
        FlverBone,
        FlverBoneBoundingBox,
        DummyPoly,
        WeaponDummyPoly,
        DummyPolyHelper,
        Skybox,
        Other,
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
        public virtual string[] ShaderTechniquesSelection => null;

        protected abstract void DrawPrimitive();

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

            if (!DBG.CategoryEnableDraw[Category])
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
                    }
                    else if (Shader is DbgPrimWireShader wire)
                    {
                        wire.VertexColorEnabled = false;
                        wire.DiffuseColor = overrideColor * 5;
                    }
                }
                else
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
            }


            var techniques = ShaderTechniquesSelection;

            var effect = Shader.Effect;

            if (techniques != null)
            {
                foreach (var techniqueName in techniques)
                {
                    effect.CurrentTechnique = effect.Techniques[techniqueName];
                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        if (Shader == GFX.DbgPrimSolidShader || Shader == GFX.DbgPrimWireShader)
                            GFX.World.ApplyViewToShader(Shader, Transform.WorldMatrix * world);
                        pass.Apply();
                        DrawPrimitive();
                    }
                }
            }
            else
            {
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    if (Shader == GFX.DbgPrimSolidShader || Shader == GFX.DbgPrimWireShader)
                        GFX.World.ApplyViewToShader(Shader, Transform.WorldMatrix * world);
                    pass.Apply();
                    DrawPrimitive();
                }
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

            if (!(EnableDbgLabelDraw && DBG.CategoryEnableDbgLabelDraw[Category]))
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

            if (!(EnableDbgLabelDraw && DBG.CategoryEnableDbgLabelDraw[Category]))
                return;

            if (DbgLabels.Count > 0)
            {
                foreach (var label in DbgLabels.OrderByDescending(lbl => (GFX.World.CameraTransform.Position - Vector3.Transform(Vector3.Zero, lbl.World)).LengthSquared()))
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
