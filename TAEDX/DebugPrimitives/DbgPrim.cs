using TAEDX.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.DebugPrimitives
{
    public enum DbgPrimCategory
    {
        Bone,
        DummyPoly,
        Other
    }

    public class DbgLabel
    {
        public Vector3 Position = Vector3.Zero;
        public float Height = 0.5f;
        public string Text = "?LabelText?";
        public Color Color;

        public DbgLabel(Vector3 position, float height, string text, Color color)
        {
            Position = position;
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

        public DbgPrimCategory Category { get; set; } = DbgPrimCategory.Other;

        private List<DbgLabel> DbgLabels = new List<DbgLabel>();

        public bool EnableDraw { get; set; } = true;
        public bool EnableDbgLabelDraw { get; set; } = true;
        public bool EnableNameDraw { get; set; } = true;

        public void AddDbgLabel(Vector3 position, float height, string text, Color color)
        {
            DbgLabels.Add(new DbgLabel(position, height, text, color));
        }

        public abstract IGFXShader<T> Shader { get; }

        public abstract DbgPrim<T> Instantiate(string newName, Transform newLocation, Color? newNameColor = null);

        /// <summary>
        /// Set this to choose specific technique(s).
        /// Null to just use the current technique.
        /// </summary>
        public virtual string[] ShaderTechniquesSelection => null;

        protected abstract void DrawPrimitive();

        public void Draw()
        {
            if (!(EnableDraw && DBG.CategoryEnableDraw[Category]))
                return;

            var techniques = ShaderTechniquesSelection;

            var effect = Shader.Effect;

            if (techniques != null)
            {
                foreach (var techniqueName in techniques)
                {
                    effect.CurrentTechnique = effect.Techniques[techniqueName];
                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        GFX.World.ApplyViewToShader(Shader, Transform);
                        pass.Apply();
                        DrawPrimitive();
                    }
                }
            }
            else
            {
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    GFX.World.ApplyViewToShader(Shader, Transform);
                    pass.Apply();
                    DrawPrimitive();
                }
            }
        }

        public void LabelDraw()
        {
            if (!(EnableDbgLabelDraw && DBG.CategoryEnableDbgLabelDraw[Category] && (EnableDraw && DBG.CategoryEnableDraw[Category])))
                return;

            if (DbgLabels.Count > 0)
            {
                foreach (var label in DbgLabels)
                {
                    DBG.DrawTextOn3DLocation(Vector3.Transform(label.Position, Transform.WorldMatrix), 
                        label.Text, label.Color, label.Height, startAndEndSpriteBatchForMe: false);
                }
            }
        }

        protected abstract void DisposeBuffers();

        public void Dispose()
        {
            DisposeBuffers();
        }
    }
}
