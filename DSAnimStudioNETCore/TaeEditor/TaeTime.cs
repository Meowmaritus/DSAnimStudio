using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public static class TaeTime
    {
        public const double TAE_FRAME_30 = 1.0 / 30.0;
        public const double TAE_FRAME_60 = 1.0 / 60.0;

        public static double RoundTimeToFrame(double time, double frameDuration)
        {
            return Math.Round(time / frameDuration) * frameDuration;
        }

        public static double RoundTimeToFrameF(float time, float frameDuration)
        {
            return (float)RoundTimeToFrame(time, frameDuration);
        }

        public static double RoundTimeToCurrentSnapInterval(double time)
        {
            if (Main.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS30)
                return RoundTimeToFrame(time, TAE_FRAME_30);
            else if (Main.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS60)
                return RoundTimeToFrame(time, TAE_FRAME_60);
            else
                return time;
        }

        public static float RoundTimeToCurrentSnapIntervalF(float time)
        {
            return (float)RoundTimeToCurrentSnapInterval(time);
        }

        public static double RoundPixelsToCurrentSnapInterval(double pixels, double oneSecondPixelScale)
        {
            if (Main.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS30)
                return RoundTimeToFrame(pixels, oneSecondPixelScale / 30.0);
            else if (Main.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS60)
                return RoundTimeToFrame(pixels, oneSecondPixelScale / 60.0);
            else
                return pixels;
        }

        public static double RoundPixelsToCurrentSnapIntervalF(float pixels, float oneSecondPixelScale)
        {
            return (float)RoundPixelsToCurrentSnapInterval(pixels, oneSecondPixelScale);
        }

        public static int RoundPixelsToCurrentSnapIntervalI(float pixels, float oneSecondPixelScale)
        {
            return (int)Math.Round(RoundPixelsToCurrentSnapInterval(pixels, oneSecondPixelScale));
        }
    }
}
