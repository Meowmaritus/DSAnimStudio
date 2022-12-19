using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DbgMenus
{
    public class DbgMenuItemGfxFlverShaderAdjust : DbgMenuItem
    {
        private void AddVector4(string name)
        {
            Vector4 GetVector4()
            {
                return GFX.FlverShader.Effect.Parameters[name].GetValueVector4();
            }

            void SetVector4(float x, float y, float z, float w)
            {
                GFX.FlverShader.Effect.Parameters[name].SetValue(new Vector4(x, y, z, w));
            }

            Items.Add(new DbgMenuItemNumber($"{name}.X", 0, 1, 0.01f,
                f =>
                {
                    var v = GetVector4();
                    SetVector4(f, v.Y, v.Z, v.W);
                },
                () => GetVector4().X));
            Items.Add(new DbgMenuItemNumber($"{name}.Y", 0, 1, 0.01f,
                f =>
                {
                    var v = GetVector4();
                    SetVector4(v.X, f, v.Z, v.W);
                },
                () => GetVector4().Y));
            Items.Add(new DbgMenuItemNumber($"{name}.Z", 0, 1, 0.01f,
                f =>
                {
                    var v = GetVector4();
                    SetVector4(v.X, v.Y, f, v.W);
                },
                () => GetVector4().Z));
            Items.Add(new DbgMenuItemNumber($"{name}.W", 0, 1, 0.01f,
                f =>
                {
                    var v = GetVector4();
                    SetVector4(v.X, v.Y, v.Z, f);
                },
                () => GetVector4().W));
        }

        private void AddFloat(string name)
        {
            Items.Add(new DbgMenuItemNumber(name, 0, 10, 0.01f,
                (f) => GFX.FlverShader.Effect.Parameters[name].SetValue(f), 
                () => GFX.FlverShader.Effect.Parameters[name].GetValueSingle()));
        }

        public DbgMenuItemGfxFlverShaderAdjust()
        {
            Text = "[GFX.FlverShader]";

            AddVector4("AmbientColor");
            AddFloat("AmbientIntensity");

            AddVector4("DiffuseColor");
            AddFloat("DiffuseIntensity");

            AddVector4("SpecularColor");
            AddFloat("SpecularPower");

            //AddVector4("LightColor");

            AddFloat("NormalMapCustomZ");
        }

        public override void UpdateUI()
        {
            foreach (var item in Items)
            {
                if (item is DbgMenuItemNumber num)
                    num.UpdateText();
            }
        }

        public override void OnRequestTextRefresh()
        {
            UpdateUI();
        }
    }
}
