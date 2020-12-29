using DSAnimStudio.TaeEditor;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class RemoEventSim
    {
        public static Color? CurrentFadeColor = null;

        public static void Clear()
        {
            CurrentFadeColor = null;
        }

        public static void Update(TaeEditAnimEventGraph graph, float time)
        {
            Clear();

            List<TaeEditAnimEventBox> eventsByTime = null;

            lock (graph._lock_EventBoxManagement)
            {
                eventsByTime = graph.EventBoxes
                    .Where(b => b.MyEvent.Template != null)
                    .OrderBy(b => b.MyEvent.StartTime)
                    .ToList();
            }

            foreach (var ev in eventsByTime)
            {
                // ScreenFadeInFromColor
                if (ev.MyEvent.Type == 0 && ev.MyEvent.StartTime <= time)
                {
                    float r = Convert.ToByte(ev.MyEvent.Parameters["Red"]) / 255f;
                    float g = Convert.ToByte(ev.MyEvent.Parameters["Green"]) / 255f;
                    float b = Convert.ToByte(ev.MyEvent.Parameters["Blue"]) / 255f;
                    float a = Utils.MapRange(MathHelper.Clamp(time, ev.MyEvent.StartTime, ev.MyEvent.EndTime), 
                        ev.MyEvent.StartTime, ev.MyEvent.EndTime, 1, 0);
                    a = a * a;
                    CurrentFadeColor = new Color(r, g, b, a);
                }
                // ScreenFadeOutToColor
                else if (ev.MyEvent.Type == 1 && ev.MyEvent.StartTime <= time)
                {
                    float r = Convert.ToByte(ev.MyEvent.Parameters["Red"]) / 255f;
                    float g = Convert.ToByte(ev.MyEvent.Parameters["Green"]) / 255f;
                    float b = Convert.ToByte(ev.MyEvent.Parameters["Blue"]) / 255f;
                    float a = Utils.MapRange(MathHelper.Clamp(time, ev.MyEvent.StartTime, ev.MyEvent.EndTime), 
                        ev.MyEvent.StartTime, ev.MyEvent.EndTime, 0, 1);
                    a = a * a;
                    CurrentFadeColor = new Color(r, g, b, a);
                }
            }
        }
    }
}
