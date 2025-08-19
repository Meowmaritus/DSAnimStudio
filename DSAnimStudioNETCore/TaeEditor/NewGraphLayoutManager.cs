using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class NewGraphLayoutManager
    {
        public readonly NewGraph Graph;

        public NewGraphLayoutManager(NewGraph graph)
        {
            Graph = graph;
        }
        

        public const float SecondsPixelSizeDefault = 128 * 2;
        public float SecondsPixelSize = SecondsPixelSizeDefault;
        public float SecondsPixelSizeMin => 16;

        public float MinFramePixelSizeToSwitchToFrameText_PerDigit => 7;
        
        public static int GetTimeInIntegerFrames(float time)
        {
            if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS60)
                return (int)Math.Round(time / TaeExtensionMethods.TAE_FRAME_60);
            else
                return (int)Math.Round(time / TaeExtensionMethods.TAE_FRAME_30);
        }

        public static double GetFrameDuration()
        {
            if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS60)
                return TaeExtensionMethods.TAE_FRAME_60;
            else
                return TaeExtensionMethods.TAE_FRAME_30;
        }
        
        public double FramesPixelSize
        {
            get
            {
                if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS60)
                    return SecondsPixelSize * TaeExtensionMethods.TAE_FRAME_60;
                else
                    return SecondsPixelSize * TaeExtensionMethods.TAE_FRAME_30;

            }
        }
        public float RowHeight = 16f;
        public float DiamondWidth = 4f;

        //public float TimeLineHeight = 24f;


        public float AfterAutoScrollHorizontalMargin = 48;

        public Rectangle? ScrollToRect = null;
        public float ScrollToRect_Margin = 16;
        public float ScrollToRect_Speed => 5 * SecondsPixelSize;
        
        public void ScrollToAction(DSAProj.Action act)
        {
            if (act == null)
                return;
            
            int row = GetRowOfAction(act);
            
            float start = act.StartTime * Graph.LayoutManager.SecondsPixelSize;
            float end = act.EndTime * Graph.LayoutManager.SecondsPixelSize;


            var actRect = new Rectangle((int)start, (int)(row * RowHeight), (int)(end - start), (int)RowHeight);

            var maxWidth = (int)(Graph.ScrollViewer.RelativeViewport.Width - (ScrollToRect_Margin * 2));
            if (actRect.Width > maxWidth)
                actRect.Width = 1;


            ScrollToRect = actRect;


            //throw new NotImplementedException();
        }
        
        public void UpdateMouseOutsideRect(float elapsedTime)
        {
            Graph.ScrollViewer.SetDisplayRect(Graph.WholeScrollViewerRect, GetVirtualAreaSize());
            UpdateScrollToRect(elapsedTime);

            Graph.ScrollViewer.UpdateInput(Graph.MainScreen.Input, Main.DELTA_UPDATE, allowScrollWheel: false);
        }

        private void UpdateScrollToRect(float elapsedTime)
        {
            

            if (Graph.PlaybackCursor != null && Graph.PlaybackCursor.Scrubbing || Graph.PlaybackCursor.IsPlaying)
            {
                ScrollToRect = null;
            }

            if (ScrollToRect != null)
            {
                float velocity = ScrollToRect_Speed * elapsedTime;

                float screenLeft = Graph.ScrollViewer.Scroll.X;
                float screenTop = Graph.ScrollViewer.Scroll.Y;
                Vector2 screenSize = Graph.ScrollViewer.RelativeViewport.Size.ToVector2();
                float screenBottom = screenTop + screenSize.Y;
                float screenRight = screenLeft + screenSize.X;

                screenLeft += ScrollToRect_Margin;
                screenRight -= ScrollToRect_Margin;
                screenTop += ScrollToRect_Margin;
                screenBottom -= ScrollToRect_Margin;

                var dest = ScrollToRect.Value;

                bool stillScrolling = false;

                if (dest.Bottom > screenBottom)
                {
                    Graph.ScrollViewer.Scroll.Y += velocity;
                    stillScrolling = true;

                    if (Graph.ScrollViewer.Scroll.Y >= Graph.ScrollViewer.MaxScroll.Y)
                        stillScrolling = false;
                }
                else if (dest.Top < screenTop)
                {
                    Graph.ScrollViewer.Scroll.Y -= velocity;
                    stillScrolling = true;

                    if (Graph.ScrollViewer.Scroll.Y <= 0)
                        stillScrolling = false;
                }


                if (dest.Right > screenRight)
                {
                    Graph.ScrollViewer.Scroll.X += velocity;
                    stillScrolling = true;

                    if (Graph.ScrollViewer.Scroll.X >= Graph.ScrollViewer.MaxScroll.X)
                        stillScrolling = false;
                }
                else if (dest.Left < screenLeft)
                {
                    Graph.ScrollViewer.Scroll.X -= velocity;
                    stillScrolling = true;

                    if (Graph.ScrollViewer.Scroll.X <= 0)
                        stillScrolling = false;
                }

                if (!stillScrolling)
                {
                    ScrollToRect = null;
                }
            }
        }
        
        public void Update(float elapsedTime)
        {
            Graph.ScrollViewer.SetDisplayRect(Graph.WholeScrollViewerRect, GetVirtualAreaSize());
            UpdateScrollToRect(elapsedTime);
            
            if (Graph.MainScreen.Input.MiddleClickHeld && Graph.WholeScrollViewerRect.Contains(Graph.MainScreen.Input.MiddleClickDownAnchor))
            {
                //TODO: Disable any tooltips here
                //Middle click panning

                return;
            }

            Graph.ScrollViewer.UpdateInput(Graph.MainScreen.Input, Main.DELTA_UPDATE, allowScrollWheel: !Graph.MainScreen.Input.CtrlHeld);

            if (Graph.MainScreen.Input.CtrlHeld)
            {
                Graph.InputMan.ZoomDelta(Graph.MainScreen.Input.ScrollDelta, Graph.MainScreen.Input.MousePosition.X - Graph.WholeScrollViewerRect.X);
            }
        }

        
        public void ScrollTowardTime(float animTime, float deltaTime, 
            float distThreshRatio, float maxDistThreshRatio, float scrollSpeedMax, bool limitSpeedToMax,
            float speedRampPower, float wholeThingLerpS, bool clampTimeMin = true, bool clampTimeMax = true)
        {
            if (Graph.MainScreen.Input.MiddleClickHeld && Graph.WholeScrollViewerRect.Contains(Graph.MainScreen.Input.MiddleClickDownAnchor))
            {
                return;
            }

            const float margin = 200;

            // float time =
            //     (float)(modTime ? Graph.PlaybackCursor.GUICurrentTimeMod : Graph.PlaybackCursor.GUICurrentTime);

            if (clampTimeMin && animTime < 0)
                animTime = 0;
            
            if (clampTimeMax && animTime > Graph.PlaybackCursor.MaxTime)
                animTime = (float)Graph.PlaybackCursor.MaxTime;
            
            
            // var endOfAnimCheckX = SecondsPixelSize * (float)Graph.PlaybackCursor.MaxTime;
            var playbackCursorPixelCheckX = SecondsPixelSize * animTime;
            float scrollXTarget = (float)Math.Round(playbackCursorPixelCheckX - (Graph.ScrollViewer.Viewport.Width / 2));
            
            
            
            
            float centerOfScreenCheckX = (Graph.ScrollViewer.Scroll.X + (Graph.ScrollViewer.Viewport.Width / 2));
            float dist = Math.Abs(playbackCursorPixelCheckX - centerOfScreenCheckX);
            float maxDist = (Graph.ScrollViewer.Viewport.Width / 2) * maxDistThreshRatio;
            float actualDistThresh = (Graph.ScrollViewer.Viewport.Width / 2) * distThreshRatio;

            float scrollSpeed = 0;
            
            if (dist < actualDistThresh)
            {
                scrollSpeed = 0;
            }
            else
            {
                float s = (dist - actualDistThresh) / (maxDist - actualDistThresh);

                if (s < 0)
                    s = 0;
                
                if (limitSpeedToMax && s > 1)
                    s = 1;

                if (speedRampPower != 1)
                    s = MathF.Pow(s, speedRampPower);
                
                scrollSpeed = MathHelper.Lerp(0, scrollSpeedMax, s);
            }
            
            
            // float rightOfScreenCheckX = centerOfScreenCheckX;// (ScrollViewer.Scroll.X + (ScrollViewer.Viewport.Width) - margin);
            // float leftOfScreenCheckX = centerOfScreenCheckX;// (ScrollViewer.Scroll.X + margin);
            //
            // if (playbackCursorPixelCheckX > rightOfScreenCheckX)
            // {
            //     Graph.ScrollViewer.Scroll.X += (playbackCursorPixelCheckX - rightOfScreenCheckX) * lerpAmount;
            //
            //     var maxScrollX = (endOfAnimCheckX + AfterAutoScrollHorizontalMargin - Graph.ScrollViewer.Viewport.Width);
            //     if (Graph.ScrollViewer.Scroll.X > maxScrollX)
            //         Graph.ScrollViewer.Scroll.X = maxScrollX;
            //
            //     Graph.ScrollViewer.ClampScroll();
            // }
            // else if (playbackCursorPixelCheckX < leftOfScreenCheckX)
            // {
            //     //Graph.ScrollViewer.Scroll.X += (playbackCursorPixelCheckX - leftOfScreenCheckX) * lerpAmount;
            //     Graph.ScrollViewer.Scroll.X = playbackCursorPixelCheckX - (Graph.ScrollViewer.Viewport.Width / 2);
            //     Graph.ScrollViewer.ClampScroll();
            // }
            
            
            scrollSpeed *= deltaTime;
                
            float deltaToTarget = (scrollXTarget - Graph.ScrollViewer.Scroll.X);

            float finalScrollX = Graph.ScrollViewer.Scroll.X;
            
            if (Math.Abs(deltaToTarget) <= scrollSpeed)
            {
                finalScrollX = scrollXTarget;
            }
            else
            {
                if (deltaToTarget > 0)
                    finalScrollX += scrollSpeed;
                else if (deltaToTarget < 0)
                    finalScrollX -= scrollSpeed;
            }



            
            

            if (wholeThingLerpS >= 1)
                Graph.ScrollViewer.Scroll.X = finalScrollX;
            else
                Graph.ScrollViewer.Scroll.X =
                    MathHelper.Lerp(Graph.ScrollViewer.Scroll.X, finalScrollX, wholeThingLerpS);
                    
            Graph.ScrollViewer.ClampScroll();
        }

        
        
        public void ScrollToPlaybackCursor(float lerpS, bool modTime, bool clampTime = false)
        {
            ScrollToRect = null;
            if (Graph.MainScreen.Input.MiddleClickHeld && Graph.WholeScrollViewerRect.Contains(Graph.MainScreen.Input.MiddleClickDownAnchor))
            {
                return;
            }

            const float margin = 200;

            float time =
                (float)(modTime ? Graph.PlaybackCursor.GUICurrentTimeMod : Graph.PlaybackCursor.GUICurrentTime);

            if (clampTime && time > Graph.PlaybackCursor.MaxTime)
                time = (float)Graph.PlaybackCursor.MaxTime;
            
            var playbackCursorPixelCheckX = SecondsPixelSize * time;
            // var endOfAnimCheckX = SecondsPixelSize * (float)Graph.PlaybackCursor.MaxTime;
            // float centerOfScreenCheckX = (Graph.ScrollViewer.Scroll.X + (Graph.ScrollViewer.Viewport.Width / 2));
            // float rightOfScreenCheckX = centerOfScreenCheckX;// (ScrollViewer.Scroll.X + (ScrollViewer.Viewport.Width) - margin);
            // float leftOfScreenCheckX = centerOfScreenCheckX;// (ScrollViewer.Scroll.X + margin);
            //
            // if (playbackCursorPixelCheckX > rightOfScreenCheckX)
            // {
            //     Graph.ScrollViewer.Scroll.X += (playbackCursorPixelCheckX - rightOfScreenCheckX) * lerpAmount;
            //
            //     var maxScrollX = (endOfAnimCheckX + AfterAutoScrollHorizontalMargin - Graph.ScrollViewer.Viewport.Width);
            //     if (Graph.ScrollViewer.Scroll.X > maxScrollX)
            //         Graph.ScrollViewer.Scroll.X = maxScrollX;
            //
            //     Graph.ScrollViewer.ClampScroll();
            // }
            // else if (playbackCursorPixelCheckX < leftOfScreenCheckX)
            // {
            //     //Graph.ScrollViewer.Scroll.X += (playbackCursorPixelCheckX - leftOfScreenCheckX) * lerpAmount;
            //     Graph.ScrollViewer.Scroll.X = playbackCursorPixelCheckX - (Graph.ScrollViewer.Viewport.Width / 2);
            //     Graph.ScrollViewer.ClampScroll();
            // }
            
            if (lerpS > 0)
                Graph.ScrollViewer.Scroll.X = MathHelper.Lerp(Graph.ScrollViewer.Scroll.X, 
                    (float)Math.Round(playbackCursorPixelCheckX - (Graph.ScrollViewer.Viewport.Width / 2)), lerpS);
            else
                Graph.ScrollViewer.Scroll.X = (float)Math.Round(playbackCursorPixelCheckX - (Graph.ScrollViewer.Viewport.Width / 2));
            
            
            Graph.ScrollViewer.ClampScroll();
        }

        public int GetRowOfAction(DSAProj.Action ev)
        {
            //int row = Graph.AnimRef.EventGroups.IndexOf(ev.Group);

            //if (row < 0)
            //    row = Graph.AnimRef.EventGroups.Count;

            ////quick test
            ////if (row == -1)
            ////{
            ////    row = ev.Type;
            ////}

            //return row;
            return ev.FixAndGetRow(Graph.AnimRef);
        }

        public Point GetVirtualAreaSize()
        {
            //TODO: Calculate max size of boxes and shit.

            float endTime = 5;
            float maxRow = 1;
            
            List<DSAProj.Action> eventsCopy = Graph.GetActionListCopy_UsesLock(useGhostIfAvailable: true);
            if (eventsCopy.Count > 0)
            {
                endTime = eventsCopy.Max(ev => ev.EndTime);
                maxRow = eventsCopy.Max(ev => GetRowOfAction(ev));
            }

            float maxAnimTimeCheck = (float)(Graph.PlaybackCursor?.MaxTime ?? 0);

            if (maxAnimTimeCheck > endTime)
                endTime = maxAnimTimeCheck;
            
            return new Point((int)((endTime * SecondsPixelSize) + Graph.Rect.Width), (int)((maxRow * RowHeight) + Graph.Rect.Height));
        }
    }
}
