using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeComboEntry
    {
        public TaeComboMenu.TaeComboAnimType ComboType;
        public long AnimID;
        public int Event0CancelType;
        public float StartFrame = -1;
        public float EndFrame = -1;

        public TaeComboEntry(TaeComboMenu.TaeComboAnimType comboType, long animID, int event0CancelType, float startFrame, float endFrame)
        {
            ComboType = comboType;
            AnimID = animID;
            Event0CancelType = event0CancelType;
            StartFrame = startFrame;
            EndFrame = endFrame;
        }



        public override string ToString()
        {
            if (StartFrame >= 0 && EndFrame >= 0)
            {
                return $"{ComboType} {AnimID} [From frame {StartFrame} to {EndFrame}]";
            }
            else if (StartFrame < 0 && EndFrame >= 0)
            {
                return $"{ComboType} {AnimID} [Until frame {EndFrame}]";
            }
            else if (StartFrame >= 0 && EndFrame < 0)
            {
                return $"{ComboType} {AnimID} [From frame {StartFrame}]";
            }
            else
            {
                return $"{ComboType} {AnimID}";
            }
        }
    }
}
