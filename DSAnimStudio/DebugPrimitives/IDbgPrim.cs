using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public interface IDbgPrim : IDisposable
    {
        Transform Transform { get; set; }
        string Name { get; set; }
        Color NameColor { get; set; }
        Color? OverrideColor { get; set; }

        object ExtraData { get; set; }

        DbgPrimCategory Category { get; set; }

        bool EnableDraw { get; set; }
        bool EnableDbgLabelDraw { get; set; }
        bool EnableNameDraw { get; set; }

        List<IDbgPrim> Children { get; set; }

        void Draw(GameTime gameTime, IDbgPrim parent, Matrix world);
        void LabelDraw();

        void LabelDraw_Billboard();

    }
}
