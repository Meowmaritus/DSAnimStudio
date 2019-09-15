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

        public Color? OverrideColor { get; set; } = null;

        public DbgPrimCategory Category { get; set; } = DbgPrimCategory.Other;

        private List<DbgLabel> DbgLabels = new List<DbgLabel>();

        public object ExtraData { get; set; } = null;

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

        public void Draw(GameTime gameTime, IDbgPrim parentPrim)
        {
            PreDraw(gameTime);

            Vector3 oldDiffuseColor = Vector3.One;

            if (Shader == GFX.DbgPrimSolidShader || Shader == GFX.DbgPrimWireShader)
            {
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

                oldDiffuseColor = Shader.Effect.Parameters["DiffuseColor"].GetValueVector3();
                if (OverrideColor.HasValue)
                {
                    var overrideColor = new Vector3(OverrideColor.Value.R / 255f, OverrideColor.Value.G / 255f, OverrideColor.Value.B / 255f);

                    if (Shader == GFX.DbgPrimWireShader)
                    {
                        overrideColor *= 5;
                    }

                    Shader.Effect.Parameters["DiffuseColor"].SetValue(overrideColor);
                }
            }

            if (!EnableDraw)
                return;

            foreach (var c in Children)
                c.Draw(gameTime, this);

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
                        if (Shader == GFX.DbgPrimSolidShader || Shader == GFX.DbgPrimWireShader)
                            GFX.World.ApplyViewToShader(Shader, Transform.WorldMatrix * (parentPrim?.Transform.WorldMatrix ?? Matrix.Identity));
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
                        GFX.World.ApplyViewToShader(Shader, Transform.WorldMatrix * (parentPrim?.Transform.WorldMatrix ?? Matrix.Identity));
                    pass.Apply();
                    DrawPrimitive();
                }
            }

            if ((Shader == GFX.DbgPrimSolidShader || Shader == GFX.DbgPrimWireShader) && OverrideColor.HasValue)
            {
                Shader.Effect.Parameters["DiffuseColor"].SetValue(oldDiffuseColor);
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

        public void LabelDraw_Billboard()
        {
            if (!(EnableDbgLabelDraw && DBG.CategoryEnableDbgLabelDraw[Category] && (EnableDraw && DBG.CategoryEnableDraw[Category])))
                return;

            if (DbgLabels.Count > 0)
            {
                foreach (var label in DbgLabels)
                {
                    DBG.Draw3DBillboard(label.Text, Matrix.CreateTranslation(label.Position) * Transform.WorldMatrix, label.Color);
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
