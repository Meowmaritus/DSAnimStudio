using SoulsAssetPipeline.Animation;
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

        public float SetValueFloat;

        public int SetUpperBodyAnim_AnimID = -1;
        public float SetUpperBodyAnim_StartFrame = -1;


        public long AnimID;

        public SoulsAssetPipeline.Animation.TAE.Animation ResolvedAnimRef;
        public float ResolvedStartTime = -1;
        public float ResolvedEndTime = -1;

        public int Event0CancelType = -1;
        public float StartFrame = -1;
        public float EndFrame = -1;

        public Dictionary<TAE.Event, int> EventRowCache = new Dictionary<TAE.Event, int>();

        public TaeComboEntry(TaeComboMenu.TaeComboAnimType comboType, long animID, int event0CancelType, float startFrame, float endFrame)
        {
            ComboType = comboType;
            AnimID = animID;
            Event0CancelType = event0CancelType;
            StartFrame = startFrame;
            EndFrame = endFrame;
        }

        public TaeComboEntry(TaeComboMenu.TaeComboAnimType comboType, float setValueFloat)
        {
            ComboType = comboType;
            SetValueFloat = setValueFloat;
        }

        public TaeComboEntry(TaeComboMenu.TaeComboAnimType comboType, int upperBodyBlendAnimID, float upperBodyStartFrame)
        {
            ComboType = comboType;
            SetUpperBodyAnim_AnimID = upperBodyBlendAnimID;
            SetUpperBodyAnim_StartFrame = upperBodyStartFrame;
        }



        public override string ToString()
        {
            if (ComboType.ToString().ToLower().StartsWith("set"))
            {
                return $"{ComboType}({SetValueFloat})";
            }
            else if (StartFrame >= 0 && EndFrame >= 0)
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
