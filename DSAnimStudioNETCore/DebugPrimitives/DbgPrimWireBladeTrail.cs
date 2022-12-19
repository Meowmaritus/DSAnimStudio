using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireBladeTrail : DbgPrimWire
    {
        public DbgPrimWireBladeTrail(string name, Color c, NewDummyPolyManager.DummyPolyBladePos blade, List<NewDummyPolyManager.DummyPolyBladePos> bladeTrail)
        {
            Name = name;
            NameColor = c;
            for (int i = 0; i < bladeTrail.Count; i++)
            {
                NewDummyPolyManager.DummyPolyBladePos curr = bladeTrail[i];
                NewDummyPolyManager.DummyPolyBladePos next = (i == bladeTrail.Count - 1) ? blade : bladeTrail[i + 1];

                AddLine(curr.Start, curr.End, c * curr.Opacity);
                AddLine(next.Start, next.End, c * next.Opacity);

                AddLine(curr.Start, next.Start, c * curr.Opacity, c * next.Opacity);
                AddLine(curr.End, next.End, c * curr.Opacity, c * next.Opacity);
            }

            this.FinalizeBuffers(true);
        }
    }
}
