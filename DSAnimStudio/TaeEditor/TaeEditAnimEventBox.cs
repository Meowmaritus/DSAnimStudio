using Microsoft.Xna.Framework;
using SoulsFormats;
using SFAnimExtensions;
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
        public float Top => (Row * OwnerPane.RowHeight) + 2 + OwnerPane.TimeLineHeight;
        public float Bottom => Top + OwnerPane.RowHeight;
        public float Width => (MyEvent.EndTime - MyEvent.StartTime) * OwnerPane.SecondsPixelSize;
        public float Height => OwnerPane.RowHeight - 2;

        public float LeftFr => MyEvent.GetStartTimeFr() * OwnerPane.SecondsPixelSize;
        public float RightFr => MyEvent.GetEndTimeFr() * OwnerPane.SecondsPixelSize;
        public float WidthFr => (MyEvent.GetEndTimeFr() - MyEvent.GetStartTimeFr()) * OwnerPane.SecondsPixelSize;
        public float HeightFr => OwnerPane.RowHeight - 4;

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
        public Color ColorBGSelected = new Color((30.0f / 255.0f) * 1, (144.0f / 255.0f) * 0.75f, 1 * 0.75f, 1);// Color.DodgerBlue;
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

        public bool PlaybackHighlight => OwnerPane?.PlaybackCursor == null ? false : CheckHighlight(
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

        public string GetPopupTitle()
        {
            if (MyEvent.Template == null)
                return $"Event Type {MyEvent.Type} [Unmapped]";
            else
                return MyEvent.TypeName;
        }

        public string GetPopupText()
        {
            var sb = new StringBuilder();

            if (MyEvent.Template == null)
            {
                var paramBytes = MyEvent.GetParameterBytes(OwnerPane.MainScreen.SelectedTae.BigEndian/*lol*/);
                int bytesOnCurrentLine = 0;

                for (int i = 0; i < paramBytes.Length; i++)
                {
                    if (bytesOnCurrentLine >= 8)
                    {
                        sb.AppendLine();
                        bytesOnCurrentLine = 0;
                    }

                    sb.Append($"{paramBytes[i]:X2} ");
                    bytesOnCurrentLine++;
                }
            }
            else
            {
                int startFrame = MyEvent.GetStartFrame(OwnerPane.PlaybackCursor.CurrentSnapInterval);
                int endFrame = MyEvent.GetEndFrame(OwnerPane.PlaybackCursor.CurrentSnapInterval);
                int frameCount = endFrame - startFrame;
                sb.AppendLine($"Start Frame: {startFrame}");
                sb.AppendLine($"  End Frame: {endFrame}");
                sb.AppendLine($"Frame Count: {frameCount}");

                var paramKvps = MyEvent.Parameters.Template.Where(kvp => kvp.Value.ValueToAssert == null).ToList();
                
                if (paramKvps.Count > 0)
                    sb.AppendLine();

                int longestParamNameLength = 0;
                int longestParamValueLength = 0;
                foreach (var kvp in paramKvps)
                {
                    if (kvp.Key.Length > longestParamNameLength)
                        longestParamNameLength = kvp.Key.Length;

                    var valStr = MyEvent.Parameters.Template[kvp.Key].ValueToString(MyEvent.Parameters[kvp.Key]);
                    if (valStr.Length > longestParamValueLength)
                        longestParamValueLength = valStr.Length;
                }

                foreach (var kvp in paramKvps)
                {
                    var nameStr = string.Format($"{{0,-{(longestParamNameLength + 1)}}}", kvp.Key + ":");
                    var valStr = string.Format($"{{0, {longestParamValueLength}}}",
                        MyEvent.Parameters.Template[kvp.Key].ValueToString(MyEvent.Parameters[kvp.Key]));
                    sb.AppendLine($"[{kvp.Value.Type,3}] {nameStr}    {valStr}");
                }

                
            }

            return sb.ToString();
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
