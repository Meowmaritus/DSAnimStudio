using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DSAnimStudio.TaeEditor
{
    public static class TaeExtensionMethods
    {
        static Dictionary<TAE.Animation, bool> isModified_Anim = new Dictionary<TAE.Animation, bool>();
        static Dictionary<TAE, bool> isModified_TAE = new Dictionary<TAE, bool>();

        public static Microsoft.Xna.Framework.Vector2 Round(this Microsoft.Xna.Framework.Vector2 v)
        {
            return new Microsoft.Xna.Framework.Vector2((float)Math.Round(v.X), (float)Math.Round(v.Y));
        }

        public static float GetLerpS(this TAE.Event ev, float time)
        {
            float lerpDuration = ev.EndTime - ev.StartTime;
            if (lerpDuration > 0)
            {
                float timeSinceFadeStart = time - ev.StartTime;
                return MathHelper.Clamp(timeSinceFadeStart / lerpDuration, 0, 1);
            }
            else
            {
                return time >= ev.StartTime ? 1 : 0;
            }
        }

        public static float ParameterLerp(this TAE.Event ev, float time, float a, float b)
        {
            return MathHelper.Lerp(a, b, ev.GetLerpS(time));
        }

        public static void ClearMemes()
        {
            lock (isModified_Anim)
                isModified_Anim.Clear();

            lock (isModified_TAE)
                isModified_TAE.Clear();
        }


        public static bool GetIsModified(this TAE.Animation ev)
        {
            lock (isModified_Anim)
            {
                if (!isModified_Anim.ContainsKey(ev))
                    isModified_Anim.Add(ev, false);

                return isModified_Anim[ev];
            }
         
        }

        public static bool GetIsModified(this TAE tae)
        {
            lock (isModified_TAE)
            {
                if (!isModified_TAE.ContainsKey(tae))
                    isModified_TAE.Add(tae, false);

                if (tae.HasErrorAndNeedsResave)
                    isModified_TAE[tae] = true;

                return isModified_TAE[tae];
            }
           
        }

        public static void ApplyRounding(this TAE.Event ev)
        {
            ev.StartTime = ev.GetStartTimeFr();
            ev.EndTime = ev.GetEndTimeFr();

            if (Main.TAE_EDITOR.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS30)
                ev.EndTime = (float)Math.Max(ev.EndTime, ev.StartTime + TAE_FRAME_30);
            else if (Main.TAE_EDITOR.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS60)
                ev.EndTime = (float)Math.Max(ev.EndTime, ev.StartTime + TAE_FRAME_60);
            else
                ev.EndTime = (float)Math.Max(ev.EndTime, ev.StartTime + 0.001f);
        }

        public static void SetIsModified(this TAE.Animation ev, bool v, bool updateGui = true)
        {
            lock (isModified_Anim)
            {
                if (!isModified_Anim.ContainsKey(ev))
                    isModified_Anim.Add(ev, false);

                isModified_Anim[ev] = v;
            }
                
        }

        public static void SetIsModified(this TAE tae, bool v, bool updateGui = true)
        {
            lock (isModified_TAE)
            {
                if (!isModified_TAE.ContainsKey(tae))
                    isModified_TAE.Add(tae, false);

                if (!v)
                    tae.HasErrorAndNeedsResave = false;

                isModified_TAE[tae] = v;
            }
                
        }



        public const double TAE_FRAME_30 = 1.0 / 30.0;
        public const double TAE_FRAME_60 = 1.0 / 60.0;

        public static float RoundTimeToCurrentSnapInterval(float time)
        {
            if (Main.TAE_EDITOR.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS30)
                return RoundTimeToFrame(time, TAE_FRAME_30);
            else if (Main.TAE_EDITOR.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS60)
                return RoundTimeToFrame(time, TAE_FRAME_60);
            else
                return time;
        }

        public static float RoundTo30FPS(float time)
        {
            return RoundTimeToFrame(time, TAE_FRAME_60);
        }

        public static float RoundTimeToFrame(float time, double frameDuration)
        {
            return (float)(Math.Round(time / frameDuration) * frameDuration);
        }

        public static float GetStartTimeFr(this TAE.Event ev)
        {
            return RoundTimeToCurrentSnapInterval(ev.StartTime);
        }

        //public static int GetStartFrame(this TAE.Event ev, double frameDuration)
        //{
        //    return (int)Math.Floor(ev.StartTime / frameDuration);
        //}

        //public static int GetEndFrame(this TAE.Event ev, double frameDuration)
        //{
        //    return (int)Math.Floor(ev.EndTime / frameDuration);
        //}

        //public static int GetStartTAEFrame(this TAE.Event ev)
        //{
        //    return (int)Math.Floor(ev.StartTime / TAE_FRAME);
        //}

        //public static int GetEndTAEFrame(this TAE.Event ev)
        //{
        //    return (int)Math.Floor(ev.EndTime / TAE_FRAME);
        //}

        public static float GetEndTimeFr(this TAE.Event ev)
        {
            return RoundTimeToCurrentSnapInterval(ev.EndTime);
        }
    }
}
