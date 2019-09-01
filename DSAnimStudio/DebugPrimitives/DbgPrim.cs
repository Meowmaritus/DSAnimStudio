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
        DummyPolyHelper,
        Other,
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

        private float _fadeOutTimer = -1;
        private float FadeOutTimerMax;
        public float FadeOutTimer
        {
            get => _fadeOutTimer;
            set
            {
                if (_fadeOutTimer == -1 && value >= 0)
                {
                    FadeOutTimerMax = value;
                }
                _fadeOutTimer = value;
            }
        }

        public List<IDbgPrim> Children { get; set; } = new List<IDbgPrim>();

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

        protected virtual void PreDraw(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {
            PreDraw(gameTime);

            if (FadeOutTimer > -1)
            {
                Shader.Effect.Parameters["DiffuseColor"].SetValue(Vector3.One * (FadeOutTimer / FadeOutTimerMax));

                if (FadeOutTimer > 0)
                {
                    FadeOutTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    DBG.MarkPrimitiveForDeletion(this);
                    return;
                }
            }
            else
            {
                Shader.Effect.Parameters["DiffuseColor"].SetValue(Vector3.One);
            }

            if (!EnableDraw)
                return;

            foreach (var c in Children)
                c.Draw(gameTime);

            if (!DBG.CategoryEnableDraw[Category])
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
