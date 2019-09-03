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
        static Dictionary<TAE.Animation, bool> isModified = new Dictionary<TAE.Animation, bool>();

        public static void ClearMemes()
        {
            isModified.Clear();
        }


        public static bool GetIsModified(this TAE.Animation ev)
        {
            if (!isModified.ContainsKey(ev))
                isModified.Add(ev, false);

            return isModified[ev];
        }

        public static void SetIsModified(this TAE.Animation ev, bool v)
        {
            if (!isModified.ContainsKey(ev))
                isModified.Add(ev, false);

            //if (v)
            //{
            //    Console.WriteLine("REEE");
            //}

            isModified[ev] = v;
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
