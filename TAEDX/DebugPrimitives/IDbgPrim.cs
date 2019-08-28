using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.DebugPrimitives
{
    public interface IDbgPrim : IDisposable
    {
        Transform Transform { get; set; }
        string Name { get; set; }
        Color NameColor { get; set; }

        DbgPrimCategory Category { get; set; }

        bool EnableDraw { get; set; }
        bool EnableDbgLabelDraw { get; set; }
        bool EnableNameDraw { get; set; }

        void Draw();
        void LabelDraw();


    }
}
