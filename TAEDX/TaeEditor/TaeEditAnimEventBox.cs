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
        private int _row;
        public int Row
        {
            get => _row;
            set
            {
                if (value != _row)
                {
                    int oldRow = _row;
                    _row = value;
                    RaiseRowChanged(oldRow);
                }
            }
        }

        public float Left => MyEvent.StartTime * OwnerPane.SecondsPixelSize;
        public float Right => MyEvent.EndTime * OwnerPane.SecondsPixelSize;
        public float Top => (Row * OwnerPane.RowHeight) + 1 + OwnerPane.TimeLineHeight;
        public float Bottom => Top + OwnerPane.RowHeight;
        public float Width => (MyEvent.EndTime - MyEvent.StartTime) * OwnerPane.SecondsPixelSize;
        public float Height => OwnerPane.RowHeight - 2;

        public float LeftFr => MyEvent.StartTimeFr * OwnerPane.SecondsPixelSize;
        public float RightFr => MyEvent.EndTimeFr * OwnerPane.SecondsPixelSize;
        public float WidthFr => (MyEvent.EndTimeFr - MyEvent.StartTimeFr) * OwnerPane.SecondsPixelSize;
        public float HeightFr => OwnerPane.RowHeight - 2;

        public string EventText { get; private set; }
        public string EventTextTall { get; private set; }

        public Color BGColor = Color.LightSteelBlue;

        public void DragWholeBoxToVirtualUnitX(float x)
        {
            float eventLength = MyEvent.EndTimeFr - MyEvent.StartTimeFr;
            MyEvent.StartTime = x / OwnerPane.SecondsPixelSize;
            MyEvent.EndTime = MyEvent.StartTimeFr + eventLength;
        }

        public void DragLeftSideOfBoxToVirtualUnitX(float x)
        {
            MyEvent.StartTime = x / OwnerPane.SecondsPixelSize;
        }

        public void DragRightSideOfBoxToVirtualUnitX(float x)
        {
            MyEvent.EndTime = x / OwnerPane.SecondsPixelSize;
        }

        //public void DragLeftSide(float dragAmount)
        //{
        //    float dragAmountInSeconds = dragAmount / OwnerPane.SecondsPixelSize;
        //    MyEvent.StartTime += dragAmountInSeconds;
        //    if (MyEvent.StartTime < 0)
        //        MyEvent.StartTime = 0;
        //}

        //public void DragRightSide(float dragAmount)
        //{
        //    float dragAmountInSeconds = dragAmount / OwnerPane.SecondsPixelSize;
        //    MyEvent.EndTime += dragAmountInSeconds;
        //}

        //public void DragMiddle(float dragAmount)
        //{
        //    float dragAmountInSeconds = dragAmount / OwnerPane.SecondsPixelSize;
        //    MyEvent.StartTime += dragAmountInSeconds;
        //    MyEvent.EndTime += dragAmountInSeconds;
        //    float amountToShiftRight = (0 - MyEvent.StartTime);
        //    if (amountToShiftRight > 0)
        //    {
        //        MyEvent.StartTime += amountToShiftRight;
        //        MyEvent.EndTime += amountToShiftRight;
        //    }
        //}

        public TaeEditAnimEventBox(TaeEditAnimEventGraph owner, TimeActEventBase myEvent)
        {
            OwnerPane = owner;
            MyEvent = myEvent;
            UpdateEventText();
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
