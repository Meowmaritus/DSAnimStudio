using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public static class TaeExtensionMethods
    {
        static Dictionary<TAE.Animation, bool> isModified_Anim = new Dictionary<TAE.Animation, bool>();
        static Dictionary<TAE, bool> isModified_TAE = new Dictionary<TAE, bool>();

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

                return isModified_TAE[tae];
            }
           
        }

        public static void ApplyRounding(this TAE.Event ev)
        {
            if (Main.TAE_EDITOR.Config.EnableSnapTo30FPSIncrements)
            {
                ev.StartTime = ev.GetStartTimeFr();
                ev.EndTime = ev.GetEndTimeFr();
            }
        }

        public static void SetIsModified(this TAE.Animation ev, bool v, bool updateGui = true)
        {
            lock (isModified_Anim)
            {
                if (!isModified_Anim.ContainsKey(ev))
                    isModified_Anim.Add(ev, false);

                //if (v)
                //{
                //    Console.WriteLine("REEE");
                //}

                isModified_Anim[ev] = v;

                if (updateGui)
                    Main.TAE_EDITOR.UpdateIsModifiedStuff();
            }
                
        }

        public static void SetIsModified(this TAE tae, bool v, bool updateGui = true)
        {
            lock (isModified_TAE)
            {
                if (!isModified_TAE.ContainsKey(tae))
                    isModified_TAE.Add(tae, false);

                //if (v)
                //{
                //    Console.WriteLine("REEE");
                //}

                isModified_TAE[tae] = v;

                if (updateGui)
                    Main.TAE_EDITOR.UpdateIsModifiedStuff();
            }
                
        }



        const double FRAME = 0.0333333333333333;

        public static float RoundTimeToFrame(float time)
        {
            return (float)(Math.Round(time / FRAME) * FRAME);
        }

        public static float GetStartTimeFr(this TAE.Event ev)
        {
            return Main.TAE_EDITOR.Config.EnableSnapTo30FPSIncrements 
                ? RoundTimeToFrame(ev.StartTime) : ev.StartTime;
        }

        public static int GetStartFrame(this TAE.Event ev)
        {
            return (int)Math.Round(ev.StartTime / FRAME);
        }

        public static int GetEndFrame(this TAE.Event ev)
        {
            return (int)Math.Round(ev.EndTime / FRAME);
        }

        public static float GetEndTimeFr(this TAE.Event ev)
        {
            return Main.TAE_EDITOR.Config.EnableSnapTo30FPSIncrements
                ? RoundTimeToFrame(ev.EndTime) : ev.EndTime;
        }
    }
}
