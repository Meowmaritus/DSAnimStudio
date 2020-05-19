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

        //public const double FRAME = 0.0333333333333333;
        public const double FRAME = 0.01666666666666666;

        public float Left => MyEvent.StartTime * OwnerPane.SecondsPixelSize;
        public float Right => MyEvent.EndTime * OwnerPane.SecondsPixelSize;
        public float Top => (Row * OwnerPane.RowHeight) + 2 + TaeEditAnimEventGraph.TimeLineHeight;
        public float Bottom => Top + OwnerPane.RowHeight;
        public float Width => (MyEvent.EndTime - MyEvent.StartTime) * OwnerPane.SecondsPixelSize;
        public float Height => OwnerPane.RowHeight - 2;

        //public float LeftFr => MyEvent.GetStartTimeFr() * OwnerPane.SecondsPixelSize;
        //public float RightFr => MyEvent.GetEndTimeFr() * OwnerPane.SecondsPixelSize;
        //public float WidthFr => (MyEvent.GetEndTimeFr() - MyEvent.GetStartTimeFr()) * OwnerPane.SecondsPixelSize;
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

        public Rectangle GetTextRect(int outlineThickness, bool smallMode)
        {
            return smallMode ? new Rectangle((int)Math.Round(Left) + 5, (int)Math.Round(Top), (int)Math.Round(Width) - 10, (int)Math.Round(Height) - 1) : 
                new Rectangle((int)Math.Round(Left) + 2 + 2/*outlineThickness*/, 
                (int)Top + 1, (int)Width - (2/*outlineThickness*/ * 2), (int)Math.Round(HeightFr) - 2);
        }

        public TaeScrollingString EventText { get; private set; } = new TaeScrollingString();

        public Color ColorBG = new Color(80, 80, 80, 255);
        public Color ColorBGHighlighted = new Color((30.0f / 255.0f) * 0.75f, (144.0f / 255.0f) * 0.75f, 1 * 0.75f, 1);// Color.DodgerBlue;

        //public Color ColorBG_Selected = new Color(0, 196, 0, 255);// Color.White;// new Color(0, 127, 55, 255);
        //public Color ColorBGHighlighted_Selected = new Color(0, 198, 0, 255); //Color.White;// new Color(0, 198, 86, 255);

        private float selectionColorPulseRadians = 0;
        private float selectionColorPulseLerpRatio = 0;
        public float SelectionColorPulseRadiansPerSec = MathHelper.PiOver2;

        private Color colorBG_Selected_PulseStart = Color.Purple;
        private Color colorBG_Selected_PulseEnd = Color.Fuchsia;
        private Color colorBGHighlighted_Selected_PulseStart = Color.Purple;
        private Color colorBGHighlighted_Selected_PulseEnd = Color.Fuchsia;

        public Color ColorBG_Selected => Color.Lerp(colorBG_Selected_PulseStart, colorBG_Selected_PulseEnd, selectionColorPulseLerpRatio);
        public Color ColorBGHighlighted_Selected => Color.Lerp(colorBGHighlighted_Selected_PulseStart, colorBGHighlighted_Selected_PulseEnd, selectionColorPulseLerpRatio);

        public void UpdateSelectionColorPulse(float elapsedTime, bool isSelected)
        {
            if (isSelected)
            {
                selectionColorPulseRadians += (elapsedTime * SelectionColorPulseRadiansPerSec);

                // Loop circle
                selectionColorPulseRadians = selectionColorPulseRadians % MathHelper.Pi;

                selectionColorPulseLerpRatio = (float)((Math.Cos(selectionColorPulseRadians > MathHelper.PiOver2 ? (selectionColorPulseRadians + MathHelper.Pi) : selectionColorPulseRadians) / 2.0) + 0.5);
            }
            else
            {
                selectionColorPulseRadians = 0;
                selectionColorPulseLerpRatio = 0;
            }
        }

        public Color ColorOutline = Color.Black;
        public Color ColorFG => Color.White;

        //private bool CheckHighlight(double startOfHitWindow, double endOfHitWindow, TaeEditAnimEventBox mouseHoverBox)
        //{

        //    if (!OwnerPane.MainScreen.Config.SoloHighlightEventOnHover || 
        //        mouseHoverBox == null || OwnerPane.PlaybackCursor.IsPlaying || 
        //        OwnerPane.PlaybackCursor.Scrubbing)
        //    {
        //        return !(MyEvent.StartTime > endOfHitWindow || MyEvent.EndTime < startOfHitWindow);
        //    }
        //    else
        //    {
        //        return mouseHoverBox == this;
        //    }

        //}

        private bool CheckHighlight(double playbackCursorPos, TaeEditAnimEventBox mouseHoverBox)
        {
            playbackCursorPos = Math.Round(playbackCursorPos, 4);

            if (!OwnerPane.MainScreen.Config.SoloHighlightEventOnHover ||
                mouseHoverBox == null || OwnerPane.PlaybackCursor.IsPlaying ||
                OwnerPane.PlaybackCursor.Scrubbing)
            {
                return playbackCursorPos >= Math.Round(MyEvent.StartTime, 4) && playbackCursorPos < Math.Round(MyEvent.EndTime, 4);
            }
            else
            {
                return mouseHoverBox == this;
            }

        }

        //public bool PlaybackHighlight => OwnerPane?.PlaybackCursor == null ? false : CheckHighlight(
        //    OwnerPane.PlaybackCursor.HitWindowStart, OwnerPane.PlaybackCursor.HitWindowEnd, 
        //    OwnerPane.MainScreen.HoveringOverEventBox);

        //public bool PrevCyclePlaybackHighlight => !OwnerPane.PlaybackCursor.JustStartedPlaying &&
        //    CheckHighlight(OwnerPane.PlaybackCursor.OldHitWindowStart, OwnerPane.PlaybackCursor.OldHitWindowEnd,
        //        OwnerPane.MainScreen.PrevHoveringOverEventBox);

        public bool PlaybackHighlight => OwnerPane?.PlaybackCursor == null ? false : 
            CheckHighlight(OwnerPane.PlaybackCursor.CurrentTimeMod, OwnerPane.MainScreen.HoveringOverEventBox);

        public bool PrevCyclePlaybackHighlight => !OwnerPane.PlaybackCursor.JustStartedPlaying &&
            CheckHighlight(OwnerPane.PlaybackCursor.OldCurrentTimeMod, OwnerPane.MainScreen.PrevHoveringOverEventBox);

        public bool DragWholeBoxToVirtualUnitX(float x)
        {
            x = MathHelper.Max(x, 0);

            var originalStartFrame = MyEvent.GetStartTimeFr();
            var originalEndFrame = MyEvent.GetEndTimeFr();

            float eventLength = MyEvent.GetEndTimeFr() - MyEvent.GetStartTimeFr();
            MyEvent.StartTime = x / OwnerPane.SecondsPixelSize;
            MyEvent.EndTime = MyEvent.GetStartTimeFr() + eventLength;

            MyEvent.ApplyRounding();

            return (MyEvent.GetStartTimeFr() != originalStartFrame || 
                MyEvent.GetEndTimeFr() != originalEndFrame);
        }

        public bool DragLeftSideOfBoxToVirtualUnitX(float x)
        {
            var originalStartFrame = MyEvent.GetStartTimeFr();

            MyEvent.StartTime = (float)Math.Min(x / OwnerPane.SecondsPixelSize, MyEvent.EndTime - FRAME);
            MyEvent.ApplyRounding();

            return (MyEvent.GetStartTimeFr() != originalStartFrame);
        }

        public bool DragRightSideOfBoxToVirtualUnitX(float x)
        {
            var originalEndFrame = MyEvent.GetEndTimeFr();

            MyEvent.EndTime = (float)Math.Max(x / OwnerPane.SecondsPixelSize, MyEvent.StartTime + FRAME);

            MyEvent.ApplyRounding();

            return (MyEvent.GetEndTimeFr() != originalEndFrame);
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
                float startFrame = (float)(MyEvent.StartTime / OwnerPane.PlaybackCursor.CurrentSnapInterval);
                float endFrame = (float)(MyEvent.EndTime / OwnerPane.PlaybackCursor.CurrentSnapInterval);
                float frameCount = endFrame - startFrame;
                sb.AppendLine($"Start Frame: {startFrame:0.00}");
                sb.AppendLine($"  End Frame: {endFrame:0.00}");
                sb.AppendLine($"Frame Count: {frameCount:0.00}");

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

                        sb.Append(kvp.Value.ValueToString(MyEvent.Parameters[kvp.Key]));
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
