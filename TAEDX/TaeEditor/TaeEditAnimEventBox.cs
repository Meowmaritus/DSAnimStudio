using MeowDSIO.DataTypes.TAE;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.TaeEditor
{
    public class TaeEditAnimEventBox
    {
        public readonly TaeEditAnimEventGraph OwnerPane;
        public TimeActEventBase MyEvent;

        public event EventHandler<int> RowChanged;
        private void RaiseRowChanged(int oldRow)
        {
            RowChanged?.Invoke(this, oldRow);
        }

        public float Left => MyEvent.StartTime * OwnerPane.SecondsPixelSize;
        public float Right => MyEvent.EndTime * OwnerPane.SecondsPixelSize;
        public float Top => (MyEvent.Row * OwnerPane.RowHeight) + 1 + OwnerPane.TimeLineHeight;
        public float Bottom => Top + OwnerPane.RowHeight;
        public float Width => (MyEvent.EndTime - MyEvent.StartTime) * OwnerPane.SecondsPixelSize;
        public float Height => OwnerPane.RowHeight - 2;

        public float LeftFr => MyEvent.StartTimeFr * OwnerPane.SecondsPixelSize;
        public float RightFr => MyEvent.EndTimeFr * OwnerPane.SecondsPixelSize;
        public float WidthFr => (MyEvent.EndTimeFr - MyEvent.StartTimeFr) * OwnerPane.SecondsPixelSize;
        public float HeightFr => OwnerPane.RowHeight - 2;

        public string EventText { get; private set; }
        public string EventTextTall { get; private set; }

        public Color ColorBG => OwnerPane.GetColorInfo(MyEvent.EventType)?.ColorA ?? Color.SkyBlue;
        public Color ColorOutline
        {
            get
            {
                return new Color(255 - ColorFG.R, 255 - ColorFG.G, 255 - ColorFG.B, 255);
            }
        }
        public Color ColorFG
        {
            get
            {
                var info = OwnerPane.GetColorInfo(MyEvent.EventType);
                if (info == null)
                    return Color.White;

                if (info.ColorAFlag == 5)
                    return Color.Black;
                else
                    return Color.White;
            }
        }

        public void DragWholeBoxToVirtualUnitX(float x)
        {
            x = MathHelper.Max(x, 0);
            float eventLength = MyEvent.EndTimeFr - MyEvent.StartTimeFr;
            MyEvent.StartTime = x / OwnerPane.SecondsPixelSize;
            MyEvent.EndTime = MyEvent.StartTimeFr + eventLength;
        }

        public void DragLeftSideOfBoxToVirtualUnitX(float x)
        {
            x = MathHelper.Max(x, 0);
            MyEvent.StartTime = x / OwnerPane.SecondsPixelSize;
        }

        public void DragRightSideOfBoxToVirtualUnitX(float x)
        {
            MyEvent.EndTime = (float)Math.Max(x / OwnerPane.SecondsPixelSize, MyEvent.StartTime + TimeActEventBase.FRAME);
        }

        public TaeEditAnimEventBox(TaeEditAnimEventGraph owner, TimeActEventBase myEvent)
        {
            //BGColor = TaeMiscUtils.GetRandomPastelColor();

            OwnerPane = owner;
            MyEvent = myEvent;
            UpdateEventText();
            myEvent.RowChanged += MyEvent_RowChanged;
        }

        public void ChangeEvent(TimeActEventBase newEvent)
        {
            MyEvent.RowChanged -= MyEvent_RowChanged;
            MyEvent = newEvent;
            MyEvent.RowChanged += MyEvent_RowChanged;
            UpdateEventText();
        }

        private void MyEvent_RowChanged(object sender, int e)
        {
            RaiseRowChanged(e);
        }

        public void UpdateEventText()
        {
            EventText = $"{MyEvent.EventType.ToString()}[{((int)MyEvent.EventType)}]({string.Join(", ", MyEvent.Parameters)})";
            EventTextTall = $"[{((int)MyEvent.EventType)}]\n" +
                $"{MyEvent.EventType.ToString()}\n\n" +
                $"{string.Join("\n", MyEvent.Parameters)}";
        }

        public void DeleteMe()
        {
            OwnerPane.DeleteEventBox(this);
        }
    }
}
