using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Linq;
using System.Text;

namespace DSAnimStudio.TaeEditor
{
    public class TaeEditAnimEventBox : ITaeClonable
    {
        public object ToClone()
        {
            var clone = new TaeEditAnimEventBox(OwnerPane, MyEvent, AnimMyEventIsFor);
            clone.CurrentGroupRegion = CurrentGroupRegion;
            clone.EventText = EventText;
            clone.PrevFrameEnteredState_ForSoundEffectPlayback = PrevFrameEnteredState_ForSoundEffectPlayback;
            clone._row = _row;
            clone.VisualRow = VisualRow;
            return clone;
        }

        public object FromClone(object cloneObj)
        {
            var clone = (TaeEditAnimEventBox)cloneObj;
            CurrentGroupRegion = clone.CurrentGroupRegion;
            EventText = clone.EventText;
            PrevFrameEnteredState_ForSoundEffectPlayback = clone.PrevFrameEnteredState_ForSoundEffectPlayback;
            _row = clone._row;
            VisualRow = clone.VisualRow;

            RowChanged = null;
            RowChanged += OwnerPane.Box_RowChanged;

            return this;
        }

        [Newtonsoft.Json.JsonIgnore]
        public readonly TaeEditAnimEventGraph OwnerPane;
        public TAE.Event MyEvent;
        public TAE.Animation AnimMyEventIsFor;

        public bool IsStateInfoEnabled = true;

        public TaeEventGroupRegion CurrentGroupRegion = null;

        public bool IsActive => OwnerPane != null && OwnerPane.AnimRef == AnimMyEventIsFor;


        public event EventHandler<int> RowChanged;
        private void RaiseRowChanged(int oldRow)
        {
            RowChanged?.Invoke(this, oldRow);
        }


        public const double TAE_FRAME_30 = 1.0 / 30.0;
        public const double TAE_FRAME_60 = 1.0 / 60.0;

        public static double FRAME
        {
            get
            {
                if (Main.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS30)
                    return TAE_FRAME_30;
                else if (Main.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS60)
                    return TAE_FRAME_60;
                else
                    return 0.01f;
            }
        }

        public float Left => MyEvent.StartTime * OwnerPane.SecondsPixelSize;
        public float Right => MyEvent.EndTime * OwnerPane.SecondsPixelSize;
        public float Top => (VisualRow * OwnerPane.RowHeight) + 2 + TaeEditAnimEventGraph.TimeLineHeight;
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
                if (value < 0)
                {
                    throw new InvalidOperationException($"Event box rowvalue invalid ({value}).");
                    
                }

                if (value != _row)
                {
                    var oldRow = _row;
                    _row = value;
                    RaiseRowChanged(oldRow);
                }
            }
        }

        public int VisualRow = 0;

        public void SetRowSilently(int newRow)
        {
            _row = newRow;
        }

        public Rectangle GetTextRect(int outlineThickness, bool smallMode)
        {
            return smallMode ? new Rectangle((int)Math.Round(Left) + 5, (int)Math.Round(Top), (int)Math.Round(Width) - 10, (int)Math.Round(Height) - 1) : 
                new Rectangle((int)Math.Round(Left) + 2 + 2/*outlineThickness*/, 
                (int)Top + 1, (int)Width - (2/*outlineThickness*/ * 2), (int)Math.Round(HeightFr) - 2);
        }

        public TaeScrollingString EventText { get; private set; } = new TaeScrollingString();

        //public Color ColorBG = new Color(80, 80, 80, 255);
        //public Color ColorBGHighlighted = new Color((30.0f / 255.0f) * 0.75f, (144.0f / 255.0f) * 0.75f, 1 * 0.75f, 1);// Color.DodgerBlue;

        //public Color ColorBG_Selected = new Color(0, 196, 0, 255);// Color.White;// new Color(0, 127, 55, 255);
        //public Color ColorBGHighlighted_Selected = new Color(0, 198, 0, 255); //Color.White;// new Color(0, 198, 86, 255);

        //private float selectionColorPulseRadians = 0;
        //private float selectionColorPulseLerpRatio = 0;
        //public float SelectionColorPulseRadiansPerSec = MathHelper.PiOver2;

        //private Color colorBG_Selected_PulseStart = Color.Purple;
        //private Color colorBG_Selected_PulseEnd = Color.Fuchsia;
        //private Color colorBGHighlighted_Selected_PulseStart = Color.Purple;
        //private Color colorBGHighlighted_Selected_PulseEnd = Color.Fuchsia;

        //public Color ColorBG_Selected => Color.Lerp(colorBG_Selected_PulseStart, colorBG_Selected_PulseEnd, selectionColorPulseLerpRatio);
        //public Color ColorBGHighlighted_Selected => Color.Lerp(colorBGHighlighted_Selected_PulseStart, colorBGHighlighted_Selected_PulseEnd, selectionColorPulseLerpRatio);

        //public void UpdateSelectionColorPulse(float elapsedTime, bool isSelected)
        //{
        //    if (isSelected)
        //    {
        //        selectionColorPulseRadians += (elapsedTime * SelectionColorPulseRadiansPerSec);

        //        // Loop circle
        //        selectionColorPulseRadians = selectionColorPulseRadians % MathHelper.Pi;

        //        selectionColorPulseLerpRatio = (float)((Math.Cos(selectionColorPulseRadians > MathHelper.PiOver2 ? (selectionColorPulseRadians + MathHelper.Pi) : selectionColorPulseRadians) / 2.0) + 0.5);
        //    }
        //    else
        //    {
        //        selectionColorPulseRadians = 0;
        //        selectionColorPulseLerpRatio = 0;
        //    }
        //}

        //public Color ColorOutline = Color.Black;
        //public Color ColorFG => Color.White;

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
            if (!IsActive)
                return false;

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

        public bool PlaybackHighlight => IsActive && ((OwnerPane?.PlaybackCursor == null ? false :
            CheckHighlight(OwnerPane.PlaybackCursor.CurrentTimeMod, OwnerPane.MainScreen.HoveringOverEventBox)) || WasJustEnteredDuringPlayback);

        public bool PlaybackHighlightMidst => IsActive && ((OwnerPane?.PlaybackCursor == null ? false :
            CheckHighlight(OwnerPane.PlaybackCursor.CurrentTimeMod, OwnerPane.MainScreen.HoveringOverEventBox)));

        public bool PrevFrameEnteredState_ForSoundEffectPlayback = false;
        public bool PrevFrameEnteredState_ForRumbleCamPlayback = false;

        public bool WasJustEnteredDuringPlayback
        {
            get
            {
                if (OwnerPane == null || OwnerPane.PlaybackCursor == null || !IsActive)
                    return false;

                // Can't get .PlaybackHighlight because it gets .WasJustEnteredDuringPlayback; stack overflow
                var curHighlight = OwnerPane?.PlaybackCursor == null ? false :
                    CheckHighlight(OwnerPane.PlaybackCursor.CurrentTimeMod, OwnerPane.MainScreen.HoveringOverEventBox);
                var prevHighlight = !OwnerPane.PlaybackCursor.JustStartedPlaying &&
                    CheckHighlight(OwnerPane.PlaybackCursor.OldCurrentTimeMod, OwnerPane.MainScreen.PrevHoveringOverEventBox);

                if (OwnerPane.PlaybackCursor.OldCurrentTime == 0)
                {
                    // Prevent an occasional double register?
                    prevHighlight = true;
                }

                if (OwnerPane.PlaybackCursor.Scrubbing || OwnerPane.PlaybackCursor.IsPlaying)
                {
                    if (curHighlight && (!prevHighlight || OwnerPane.PlaybackCursor.JustStartedPlaying))
                        return true;

                    if (OwnerPane.PlaybackCursor.IsPlaying)
                    {
                        float timeDirection = (float)(OwnerPane.PlaybackCursor.BasePlaybackSpeed * OwnerPane.PlaybackCursor.ModPlaybackSpeed_Event603 * OwnerPane.PlaybackCursor.ModPlaybackSpeed_Event608);

                        if (timeDirection > 0)
                        {
                            if (OwnerPane.PlaybackCursor.CurrentTimeMod >= MyEvent.StartTime)
                            {
                                if (OwnerPane.PlaybackCursor.OldCurrentTimeModWrapped < MyEvent.StartTime || 
                                    OwnerPane.PlaybackCursor.JustLooped || OwnerPane.PlaybackCursor.JustStartedPlaying || 
                                    OwnerPane.PlaybackCursor.OldCurrentTime == 0)
                                {
                                    return true;
                                }
                            }
                        }
                        else if (timeDirection < 0)
                        {
                            if (OwnerPane.PlaybackCursor.CurrentTimeMod < MyEvent.EndTime)
                            {
                                if (OwnerPane.PlaybackCursor.OldCurrentTimeModWrapped >= MyEvent.EndTime ||
                                    OwnerPane.PlaybackCursor.JustLooped || OwnerPane.PlaybackCursor.JustStartedPlaying || 
                                    OwnerPane.PlaybackCursor.CurrentTime == OwnerPane.PlaybackCursor.MaxTime)
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {

                            Console.WriteLine(DateTime.Now.Millisecond +  " - Fatcat");
                        }

                        //if ((timeDirection > 0 && (OwnerPane.PlaybackCursor.OldCurrentTimeModWrapped < MyEvent.StartTime && OwnerPane.PlaybackCursor.CurrentTimeMod >= MyEvent.StartTime)) ||
                        //    (timeDirection < 0 && (OwnerPane.PlaybackCursor.OldCurrentTimeModWrapped >= MyEvent.EndTime && OwnerPane.PlaybackCursor.CurrentTimeMod < MyEvent.EndTime)))
                        //{
                        //    return true;
                        //}
                    }

                    
                }

                if (curHighlight && !OwnerPane.PlaybackCursor.OldIsPlaying && OwnerPane.PlaybackCursor.IsPlaying)
                {
                    return true;
                }

                return false;
            }
        }

        public bool PrevCyclePlaybackHighlight => IsActive && (!OwnerPane.PlaybackCursor.JustStartedPlaying &&
            CheckHighlight(OwnerPane.PlaybackCursor.OldCurrentTimeMod, OwnerPane.MainScreen.PrevHoveringOverEventBox));

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
            x = MathHelper.Max(x, 0);

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

        public TaeEditAnimEventBox(TaeEditAnimEventGraph owner, TAE.Event myEvent, TAE.Animation animEventIsFor)
        {
            //BGColor = TaeMiscUtils.GetRandomPastelColor();

            OwnerPane = owner;
            MyEvent = myEvent;
            AnimMyEventIsFor = animEventIsFor;

            UpdateEventText();
        }

        public void ChangeEvent(TAE.Event newEvent, TAE.Animation animNewEventIsFor)
        {
            newEvent.Group = MyEvent.Group;
            var oldBytes = MyEvent.GetParameterBytes(bigEndian: GameRoot.IsBigEndianGame);
            var newBytes = newEvent.GetParameterBytes(bigEndian: GameRoot.IsBigEndianGame);
            MyEvent = newEvent;
            Array.Resize(ref oldBytes, newBytes.Length);
            MyEvent.SetParameterBytes(bigEndian: GameRoot.IsBigEndianGame, oldBytes, lenientOnAssert: true);
            AnimMyEventIsFor = animNewEventIsFor;
            UpdateEventText();
        }

        private void MyEvent_RowChanged(object sender, int e)
        {
            RaiseRowChanged(e);
        }

        public string DispEventName => $"{(MyEvent.TypeName ?? "")}[{MyEvent.Type}]";

        public string GetPopupTitle()
        {
            return DispEventName;
        }

        public string GetPopupText()
        {
            var sb = new StringBuilder();

            if (MyEvent.Template == null)
            {
                var paramBytes = MyEvent.GetParameterBytes(GameRoot.IsBigEndianGame);
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

        public static string GetEventBoxText(TAE.Event ev)
        {
            var sb = new StringBuilder($"{(ev.TypeName ?? "") }[{ev.Type}](");
            bool first = true;
            if (ev.Parameters != null)
            {
                foreach (var kvp in ev.Parameters.Template)
                {
                    if (kvp.Value.ValueToAssert == null)
                    {
                        if (first)
                            first = false;
                        else
                            sb.Append(", ");

                        sb.Append(kvp.Value.ValueToString(ev.Parameters[kvp.Key]));
                    }
                }
                sb.Append(")");
            }
            else
            {
                sb.Append($"{(ev.TypeName ?? "") }[{ev.Type}]({string.Join(" ", ev.GetParameterBytes(GameRoot.IsBigEndianGame).Select(b => b.ToString("X2")))})");
            }
            
            return sb.ToString();
        }

        public void UpdateEventText()
        {
            if (MyEvent.Template != null)
            {
                var sb = new StringBuilder($"{(MyEvent.TypeName ?? "") }[{MyEvent.Type}](");
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
                EventText.SetText($"{(MyEvent.TypeName ?? "") }[{MyEvent.Type}]({string.Join(" ", MyEvent.GetParameterBytes(GameRoot.IsBigEndianGame).Select(b => b.ToString("X2")))})");
            }
        }

       

        //public void DeleteMe()
        //{
        //    OwnerPane.DeleteEventBox(this);
        //}
    }
}
