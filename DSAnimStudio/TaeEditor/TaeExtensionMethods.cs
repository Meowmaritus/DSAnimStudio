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
            isModified_Anim.Clear();
            isModified_TAE.Clear();
        }


        public static bool GetIsModified(this TAE.Animation ev)
        {
            if (!isModified_Anim.ContainsKey(ev))
                isModified_Anim.Add(ev, false);

            return isModified_Anim[ev];
        }

        public static bool GetIsModified(this TAE tae)
        {
            if (!isModified_TAE.ContainsKey(tae))
                isModified_TAE.Add(tae, false);

            return isModified_TAE[tae];
        }

        public static void SetIsModified(this TAE.Animation ev, bool v)
        {
            if (!isModified_Anim.ContainsKey(ev))
                isModified_Anim.Add(ev, false);

            //if (v)
            //{
            //    Console.WriteLine("REEE");
            //}

            isModified_Anim[ev] = v;

            Main.TAE_EDITOR.UpdateIsModifiedStuff();
        }

        public static void SetIsModified(this TAE tae, bool v)
        {
            if (!isModified_TAE.ContainsKey(tae))
                isModified_TAE.Add(tae, false);

            //if (v)
            //{
            //    Console.WriteLine("REEE");
            //}

            isModified_TAE[tae] = v;

            Main.TAE_EDITOR.UpdateIsModifiedStuff();
        }



        const double FRAME = 0.0333333333333333;

        public static float RoundTimeToFrame(float time)
        {
            return (float)(Math.Round(time / FRAME) * FRAME);
        }

        public static float GetStartTimeFr(this TAE.Event ev)
        {
            return RoundTimeToFrame(ev.StartTime);
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
            return RoundTimeToFrame(ev.EndTime);
        }
    }
}
