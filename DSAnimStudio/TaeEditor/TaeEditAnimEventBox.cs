using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Linq;
using System.Text;

namespace DSAnimStudio.TaeEditor
{
    public class TaeEditAnimEventBox
    {
        [Newtonsoft.Json.JsonIgnore]
        public readonly TaeEditAnimEventGraph OwnerPane;
        public TAE.Event MyEvent;

        public event EventHandler<int> RowChanged;
        private void RaiseRowChanged(int oldRow)
        {
            RowChanged?.Invoke(this, oldRow);
        }

        public const double FRAME = 0.0333333333333333;

        public float Left => MyEvent.StartTime * OwnerPane.SecondsPixelSize;
        public float Right => MyEvent.EndTime * OwnerPane.SecondsPixelSize;
        public float Top => (Row * OwnerPane.RowHeight) + 1 + OwnerPane.TimeLineHeight;
        public float Bottom => Top + OwnerPane.RowHeight;
        public float Width => (MyEvent.EndTime - MyEvent.StartTime) * OwnerPane.SecondsPixelSize;
        public float Height => OwnerPane.RowHeight - 2;

        public float LeftFr => MyEvent.GetStartTimeFr() * OwnerPane.SecondsPixelSize;
        public float RightFr => MyEvent.GetEndTimeFr() * OwnerPane.SecondsPixelSize;
        public float WidthFr => (MyEvent.GetEndTimeFr() - MyEvent.GetStartTimeFr()) * OwnerPane.SecondsPixelSize;
        public float HeightFr => OwnerPane.RowHeight - 2;

        private int _row = -1;
        public int Row
        {
            get => _row;
            set
            {
                if (value != _row)
                {
                    var oldRow = _row;
                    _row = value;
                    RaiseRowChanged(oldRow);
                }
            }
        }

        public Rectangle GetTextRect(int outlineThickness)
        {
            return new Rectangle((int)LeftFr + 2 + 2/*outlineThickness*/, 
                (int)Top + 1, (int)WidthFr - 4 - (2/*outlineThickness*/ * 2), (int)HeightFr - 2);
        }

        public TaeScrollingString EventText { get; private set; } = new TaeScrollingString();

        public Color ColorBG = new Color(80, 80, 80, 255);
        public Color ColorBGSelected = Color.DodgerBlue;
        public Color ColorOutline = Color.Black;
        public Color ColorFG => Color.White;

        private bool CheckHighlight(double curFrame, TaeEditAnimEventBox hoverBox)
        {
            if (!OwnerPane.MainScreen.Config.SoloHighlightEventOnHover || 
                hoverBox == null || OwnerPane.PlaybackCursor.IsPlaying || 
                OwnerPane.PlaybackCursor.Scrubbing)
            {
                return curFrame >=
                MyEvent.GetStartFrame(OwnerPane.PlaybackCursor.CurrentSnapInterval) &&
                curFrame <
                MyEvent.GetEndFrame(OwnerPane.PlaybackCursor.CurrentSnapInterval);
            }
            else
            {
                return hoverBox == this;
            }
            
        }

        public bool PlaybackHighlight => CheckHighlight(
            OwnerPane.PlaybackCursor.GUICurrentFrameMod, 
            OwnerPane.MainScreen.HoveringOverEventBox);

        public bool PrevCyclePlaybackHighlight => !OwnerPane.PlaybackCursor.JustStartedPlaying &&
            CheckHighlight(OwnerPane.PlaybackCursor.OldGUICurrentFrameMod,
                OwnerPane.MainScreen.PrevHoveringOverEventBox);

        public bool DragWholeBoxToVirtualUnitX(float x)
        {
            x = MathHelper.Max(x, 0);

            int originalStartFrame = MyEvent.GetStartTAEFrame();
            int originalEndFrame = MyEvent.GetEndTAEFrame();

            float eventLength = MyEvent.GetEndTimeFr() - MyEvent.GetStartTimeFr();
            MyEvent.StartTime = x / OwnerPane.SecondsPixelSize;
            MyEvent.EndTime = MyEvent.GetStartTimeFr() + eventLength;

            MyEvent.ApplyRounding();

            return (MyEvent.GetStartTAEFrame() != originalStartFrame || 
                MyEvent.GetEndTAEFrame() != originalEndFrame);
        }

        public bool DragLeftSideOfBoxToVirtualUnitX(float x)
        {
            int originalStartFrame = MyEvent.GetStartTAEFrame();

            MyEvent.StartTime = (float)Math.Min(x / OwnerPane.SecondsPixelSize, MyEvent.EndTime - FRAME);
            MyEvent.ApplyRounding();

            return (MyEvent.GetStartTAEFrame() != originalStartFrame);
        }

        public bool DragRightSideOfBoxToVirtualUnitX(float x)
        {
            float originalEndFrame = MyEvent.GetEndTAEFrame();

            MyEvent.EndTime = (float)Math.Max(x / OwnerPane.SecondsPixelSize, MyEvent.StartTime + FRAME);

            MyEvent.ApplyRounding();

            return (MyEvent.GetEndTAEFrame() != originalEndFrame);
        }

        public TaeEditAnimEventBox(TaeEditAnimEventGraph owner, TAE.Event myEvent)
        {
            //BGColor = TaeMiscUtils.GetRandomPastelColor();

            OwnerPane = owner;
            MyEvent = myEvent;
            UpdateEventText();
        }

        public void ChangeEvent(TAE.Event newEvent)
        {
            MyEvent = newEvent;
            UpdateEventText();
        }

        private void MyEvent_RowChanged(object sender, int e)
        {
            RaiseRowChanged(e);
        }

        public string GetPopupText()
        {
            return "[TODO: TaeEditAnimEventBox.GetPopupText()]\n\n\n\ntest test";
        }

        public void UpdateEventText()
        {
            if (MyEvent.Template != null)
            {
                var sb = new StringBuilder($"{MyEvent.Template.Name}(");
                bool first = true;
                foreach (var kvp in MyEvent.Parameters.Template)
                {
                    if (kvp.Value.ValueToAssert == null)
                    {
                        if (first)
                            first = false;
                        else
                            sb.Append(", ");

                        sb.Append(MyEvent.Parameters[kvp.Key].ToString());
                    }
                }
                sb.Append(")");
                EventText.SetText(sb.ToString());
            }
            else
            {
                EventText.SetText($"[{MyEvent.Type}]({string.Join(" ", MyEvent.GetParameterBytes(false).Select(b => b.ToString("X2")))})");
            }
        }

        //public void DeleteMe()
        //{
        //    OwnerPane.DeleteEventBox(this);
        //}
    }
}
