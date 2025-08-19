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
        float ElapsedTime { get; set; }

        ParamData.AtkParam.DummyPolySource DmyPolySource { get; set; }
        Transform Transform { get; set; }
        Color? OverrideColor { get; set; }

        object ExtraData { get; set; }

        bool EnableDraw { get; set; }

        List<IDbgPrim> Children { get; set; }
        List<IDbgPrim> UnparentedChildren { get; set; }

        void Draw(bool forceEnableDraw, IDbgPrim parent, Matrix world);

    }
}
