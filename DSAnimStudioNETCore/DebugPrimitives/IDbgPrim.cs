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
        ParamData.AtkParam.DummyPolySource DmyPolySource { get; set; }
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
        List<IDbgPrim> UnparentedChildren { get; set; }

        void Draw(IDbgPrim parent, Matrix world);
        void LabelDraw(Matrix world);

        void LabelDraw_Billboard(Matrix world);

    }
}
