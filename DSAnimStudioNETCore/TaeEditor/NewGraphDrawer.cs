using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSAnimStudio.ImguiOSD;

namespace DSAnimStudio.TaeEditor
{
    public class NewGraphDrawer
    {
        public readonly NewGraph Graph;
        private NewGraphLayoutManager Layout => Graph.LayoutManager;

        public NewGraphDrawer(NewGraph graph)
        {
            Graph = graph;
        }

        public Color PlaybackCursorColor => Main.Colors.GuiColorActionGraphPlaybackCursor_V3;

        public float PlaybackCursorThickness = 2;
        public float MinPixelsBetweenFramesForHelperLines = 4;

        private Dictionary<int, float> GetSecondVerticalLineXPositions()
        {
            var result = new Dictionary<int, float>();
            float startTimeSeconds = Graph.ScrollViewer.RoundedScroll.X / Graph.LayoutManager.SecondsPixelSize;
            float endTimeSeconds = (Graph.ScrollViewer.RoundedScroll.X + Graph.ScrollViewer.Viewport.Width) / Graph.LayoutManager.SecondsPixelSize;

            for (int i = (int)startTimeSeconds; i <= (int)endTimeSeconds; i++)
            {
                result.Add(i, (i * Graph.LayoutManager.SecondsPixelSize));
            }

            return result;
        }
        
        private Dictionary<int, float> GetFrameVerticalLineXPositions(double? framesPixelSize)
        {
            var result = new Dictionary<int, float>();
           
            
            if (framesPixelSize.HasValue)
            {
                double startTimeFrames = Graph.ScrollViewer.RoundedScroll.X / framesPixelSize.Value;
                double endTimeFrames = (Graph.ScrollViewer.RoundedScroll.X + Graph.ScrollViewer.Viewport.Width) / framesPixelSize.Value;

                for (int i = (int)startTimeFrames; i <= (int)endTimeFrames; i++)
                {
                    result.Add(i, (float)(i * framesPixelSize.Value));
                }
            }
            
            

            return result;
        }

        private Dictionary<int, float> GetRowHorizontalLineYPositions()
        {
            var result = new Dictionary<int, float>();
            float startRow = Graph.ScrollViewer.RoundedScroll.Y / Graph.LayoutManager.RowHeight;
            float endRow = (Graph.ScrollViewer.RoundedScroll.Y + Graph.ScrollViewer.Viewport.Height) / Graph.LayoutManager.RowHeight;

            for (int i = (int)startRow; i <= (int)endRow; i++)
            {
                result.Add(i, ((i * Graph.LayoutManager.RowHeight)) + Graph.TimeLineHeight);
            }

            return result;
        }

        private void DrawTimeLine(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex,
            SpriteFont font, Dictionary<int, float> secondVerticalLineXPositions)
        {
            foreach (var kvp in secondVerticalLineXPositions)
            {
                //sb.DrawString(font, kvp.Key.ToString(), new Vector2((float)Math.Round(kvp.Value + 4 + 1), (float)Math.Round(ScrollViewer.Scroll.Y + 3 + 1)) + Main.GlobalTaeEditorFontOffset, Color.Black);
                //sb.DrawString(font, kvp.Key.ToString(), new Vector2((float)Math.Round(kvp.Value + 4), (float)Math.Round(ScrollViewer.Scroll.Y + 3)) + Main.GlobalTaeEditorFontOffset, Color.White);

                ImGuiDebugDrawer.DrawText(kvp.Key.ToString(), new Vector2((float)Math.Round(kvp.Value + 4 - Graph.ScrollViewer.RoundedScroll.X), 0), Color.White, null, 20);
            }
        }

        private void DrawFrameSnapLines(Texture2D boxTex, SpriteBatch sb, Color col)
        {
            double framePixelSize_TAE = 0;

            if (Graph.MainScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS60)
                framePixelSize_TAE = (Graph.LayoutManager.SecondsPixelSize / (1 / TaeExtensionMethods.TAE_FRAME_60));
            else if (Graph.MainScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS30)
                framePixelSize_TAE = (Graph.LayoutManager.SecondsPixelSize / (1 / TaeExtensionMethods.TAE_FRAME_30));

            bool zoomedEnoughForFrameLines_TAE = false;

            if (Graph.MainScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS60)
                zoomedEnoughForFrameLines_TAE = Graph.LayoutManager.SecondsPixelSize >= ((1 / TaeExtensionMethods.TAE_FRAME_60) * MinPixelsBetweenFramesForHelperLines);
            else if (Graph.MainScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS30)
                zoomedEnoughForFrameLines_TAE = Graph.LayoutManager.SecondsPixelSize >= ((1 / TaeExtensionMethods.TAE_FRAME_30) * MinPixelsBetweenFramesForHelperLines);


            if (zoomedEnoughForFrameLines_TAE)
            {
                int startFrame = (int)Math.Floor(Graph.ScrollViewer.RoundedScroll.X / framePixelSize_TAE);
                int endFrame = (int)Math.Ceiling((Graph.ScrollViewer.RoundedScroll.X + Graph.ScrollViewer.Viewport.Width) / framePixelSize_TAE);

                for (int i = startFrame; i <= endFrame; i++)
                {
                    int moduloFps = 30;

                    if (Graph.MainScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS60)
                        moduloFps = 60;

                    if (i % moduloFps == 0)
                        continue;

                    ImGuiDebugDrawer.DrawLine(new Vector2((float)(i * framePixelSize_TAE - Graph.ScrollViewer.RoundedScroll.X), 0),
                    new Vector2((float)(i * framePixelSize_TAE - Graph.ScrollViewer.RoundedScroll.X), GFX.Device.Viewport.Height / Main.DPI),
                    col);
                }
            }
        }


        public void DrawDebug()
        {
            if (Main.Debug.EnableGraphDebug)
            {
                //Rectangle debug_view_rect = Graph.WholeScrollViewerRect;


                //debug_view_rect = Graph.ScrollViewerContentsRect;

                int shiftTextHeightEach = 0;
                int i = 0;
                
                ImGuiDebugDrawer.DrawRect(Graph.ScrollViewerContentsRect, Color.Red * 0.75f, 0, thickness: 0);
                if (Graph.ScrollViewerContentsRect.Contains(Main.Input.MousePositionPoint))
                    ImGuiDebugDrawer.DrawText(nameof(Graph.ScrollViewerContentsRect), Graph.ScrollViewerContentsRect.BottomLeftCorner() 
                        + new Vector2(0, shiftTextHeightEach * (i++)), Color.Red);
                
                ImGuiDebugDrawer.DrawRect(Graph.ActionTrackHeaderTextRect, Color.Yellow * 0.75f, 0, thickness: 0);
                if (Graph.ActionTrackHeaderTextRect.Contains(Main.Input.MousePositionPoint))
                    ImGuiDebugDrawer.DrawText(nameof(Graph.ActionTrackHeaderTextRect), Graph.ActionTrackHeaderTextRect.BottomLeftCorner() 
                        + new Vector2(0, shiftTextHeightEach * (i++)), Color.Yellow);
                
                ImGuiDebugDrawer.DrawRect(Graph.ActionTrackHeaderEditButtonRect, Color.Green * 0.75f, 0, thickness: 0);
                if (Graph.ActionTrackHeaderEditButtonRect.Contains(Main.Input.MousePositionPoint))
                    ImGuiDebugDrawer.DrawText(nameof(Graph.ActionTrackHeaderEditButtonRect), Graph.ActionTrackHeaderEditButtonRect.BottomLeftCorner() 
                        + new Vector2(0, shiftTextHeightEach * (i++)), Color.Green);
                
                //ImGuiDebugDrawer.DrawRect(Graph.Rect, Color.Yellow, 0, true);
                // if (Graph.ScrollViewerContentsRect.Contains(Main.Input.MousePositionPoint))
                //     ImGuiDebugDrawer.DrawRect(Graph.ScrollViewerContentsRect, Color.Red * 0.75f, 0, true);
                // if (Graph.ActionTrackHeaderTextRect.Contains(Main.Input.MousePositionPoint))
                //     ImGuiDebugDrawer.DrawRect(Graph.ActionTrackHeaderTextRect, Color.Yellow * 0.75f, 0, true);
                // if (Graph.ActionTrackHeaderEditButtonRect.Contains(Main.Input.MousePositionPoint))
                //     ImGuiDebugDrawer.DrawRect(Graph.ActionTrackHeaderEditButtonRect, Color.Green * 0.75f, 0, true);
                ImGuiDebugDrawer.DrawRect(Main.Input.MousePosition, new Vector2(32, 32), Color.Fuchsia, 0, thickness: 1);
            }
        }
        

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex,
            SpriteFont font, float elapsedSeconds, SpriteFont smallFont, Texture2D scrollbarArrowTex, DSAProj proj)
        {
            bool ghostGraph = Graph.IsGhostGraph;
            
            //Graph.ScrollViewer.SetDisplayRect(Graph.WholeScrollViewerRect, Layout.GetVirtualAreaSize());

            //Test
            //Graph.ScrollViewer.RoundingUnitX = Layout.FramesPixelSize;
            //Graph.ScrollViewer.RoundingUnitY = Layout.RowHeight;
            
            // Graph.ScrollViewer.RoundingUnitX = 1;
            // Graph.ScrollViewer.RoundingUnitY = 1;
            
            Graph.ScrollViewer.RoundingUnitX = 0;
            Graph.ScrollViewer.RoundingUnitY = 0;
            
            Action drawAfterAllActions = () => { };

            var scrollMatrix = Graph.ScrollViewer.GetScrollMatrix();

            var actionTracksCopy = Graph.GetActionTracksCopy_UsesLock(useGhostIfAvailable: true);
            // Track names on left, fixed pane thing.
            var scrolLMatrix_YOnly = Graph.ScrollViewer.GetScrollMatrixYOnly();

            var oldViewport = gd.Viewport;


            

            var viewportRect = new Rectangle(Graph.WholeScrollViewerRect.X, Graph.Rect.Y, (int)Graph.WholeScrollViewerRect.Width - NewGraph.SpacingMemeBottomRightOfGraph, Graph.Rect.Height - NewGraph.SpacingMemeBottomRightOfGraph);
            gd.Viewport = new Viewport(viewportRect.DpiScaled());
            {
                sb.Begin(transformMatrix: scrolLMatrix_YOnly * Main.DPIMatrix);
                try
                {
                    var timelinePositions_Seconds = GetSecondVerticalLineXPositions();
                    
                    foreach (var x in timelinePositions_Seconds)
                    {
                        ImGuiDebugDrawer.DrawLine(new Vector2(x.Value - Graph.ScrollViewer.RoundedScroll.X, 0), new Vector2(x.Value - Graph.ScrollViewer.RoundedScroll.X, gd.Viewport.Height / Main.DPI), Color.White);
                    }
                    
                    double framesPixelSize = Graph.LayoutManager.FramesPixelSize;
                    double currentFrameDuration = NewGraphLayoutManager.GetFrameDuration();

                    var timelinePositions_Frames = GetFrameVerticalLineXPositions(framesPixelSize);
                    foreach (var kvp in timelinePositions_Frames)
                    {
                        string numString = kvp.Key.ToString();
                        float numDigits = numString.Length;

                        float digitSize = Graph.LayoutManager.MinFramePixelSizeToSwitchToFrameText_PerDigit;
                        
                        
                        
                        if (numDigits < 1.25f)
                            numDigits = 1.25f;
                        
                        // Basically a really stupid way to say skip the frames that align with seconds so like 0, 30, 60, 90, etc.
                        float howCloseThisIsToASecond = (kvp.Value / Graph.LayoutManager.SecondsPixelSize) % 1;
                        if (howCloseThisIsToASecond < (currentFrameDuration / 2) || howCloseThisIsToASecond > (1 - (currentFrameDuration / 2)))
                            continue;
                        if (framesPixelSize >=
                            (Graph.LayoutManager.MinFramePixelSizeToSwitchToFrameText_PerDigit * numDigits))
                        {
                            ImGuiDebugDrawer.DrawText(numString, new Vector2((float)Math.Round(kvp.Value + 2 - Graph.ScrollViewer.RoundedScroll.X), 0), Color.White, null, 15);
                        }
                        else if (framesPixelSize >= Graph.LayoutManager.MinFramePixelSizeToSwitchToFrameText_PerDigit * 1.25f)
                        {
                            ImGuiDebugDrawer.DrawText("-", new Vector2((float)Math.Round(kvp.Value + 2 - Graph.ScrollViewer.RoundedScroll.X), 0), Color.White, null, 15);
                        }
                    }
                    
                    DrawTimeLine(gd, sb, boxTex, font, timelinePositions_Seconds);
                    
                    DrawFrameSnapLines(boxTex, sb, Color.White * 0.5f);


                }
                finally
                {
                    sb.End();
                }
            }
            gd.Viewport = oldViewport;

            gd.Viewport = new Viewport(Graph.ScrollViewer.Viewport.DpiScaled());
            {
                sb.Begin(transformMatrix: scrollMatrix * Main.DPIMatrix);
                try
                {
                    var dragActions = Graph.InputMan.GetCurrentDragActions();
                    
                    // Action tracks alternating colors
                    for (int i = 0; i < actionTracksCopy.Count; i++)
                    {
                        
                        if (actionTracksCopy[i].PlaybackHighlight)
                            ImGuiDebugDrawer.DrawRect(new Vector2(0, i * Layout.RowHeight) + new Vector2(0, -Graph.ScrollViewer.RoundedScroll.Y), new Vector2(gd.Viewport.Width / Main.DPI, Layout.RowHeight), Color.White * 0.5f);
                        
                        if (Graph.MainScreen.HighlightOpacityDictCopy.ContainsKey(actionTracksCopy[i]))
                        {
                            float highlightOpacity = Graph.MainScreen.HighlightOpacityDictCopy[actionTracksCopy[i]];
                            
                            ImGuiDebugDrawer.DrawRect(new Vector2(0, i * Layout.RowHeight) + new Vector2(0, -Graph.ScrollViewer.RoundedScroll.Y), new Vector2(gd.Viewport.Width / Main.DPI, Layout.RowHeight), Color.Yellow.MultiplyAlpha(highlightOpacity * 0.5f));
                        }
                        
                        if (i % 2 == 0)
                        {
                            ImGuiDebugDrawer.DrawRect(new Vector2(0, i * Layout.RowHeight) + new Vector2(0, -Graph.ScrollViewer.RoundedScroll.Y), new Vector2(gd.Viewport.Width / Main.DPI, Layout.RowHeight), Color.Black * 0.3f);

                        }
                    }


                    List<DSAProj.Action> actionsCopy = Graph.GetActionListCopy_UsesLock(useGhostIfAvailable: true);

                    //bool anyHoverEvent = false;

                    var scrollViewVisibleRect = Graph.ScrollViewer.RelativeViewportRounded;

                    

                    foreach (var act in actionsCopy)
                    {
                        int row = Layout.GetRowOfAction(act);

                        //TODO: Make proper color variables.
                        var color_fill = Main.Colors.GuiColorActionBox_Normal_Fill;
                        var color_line = Main.Colors.GuiColorActionBox_Normal_Outline;
                        var color_text = Main.Colors.GuiColorActionBox_Normal_Text;
                        var color_text_shadow = Main.Colors.GuiColorActionBox_Normal_TextShadow;
                        float line_thickness = 1;


                        if (act.Info.CustomColor.HasValue)
                        {
                            color_fill = act.Info.CustomColor.Value;
                        }

                        if (act.NewSimulationActive && act.IsActive)
                        {
                            //color_fill = new Color((30.0f / 255.0f), (144.0f / 255.0f), 1, 1);

                            

                        }
                        else
                        {
                            color_fill = new Color((color_fill.R / 255.0f) * 0.55f, (color_fill.G / 255.0f) * 0.55f, (color_fill.B / 255.0f) * 0.55f, 1);

                            
                        }

                        if (Graph.MainScreen.NewSelectedActions.Contains(act) || Graph.MainScreen.InspectorAction == act)
                        {
                            //color_fill = new Color((30.0f / 255.0f), (144.0f / 255.0f), 1, 1);
                            color_line = Main.PulseLerpColor(Main.Colors.GuiColorActionBox_Selected_Outline_PulseStart, 
                                Main.Colors.GuiColorActionBox_Selected_Outline_PulseEnd, 2);
                            //color_text = Color.Black;
                            //color_text_shadow = Color.White;

                            //if (act.NewSimulationActive && act.IsActive)
                            //    color_fill = new Color(1 * 0.75f, 1 * 0.75f, 0, 1);
                            //else
                            //    color_fill = new Color(1 * 0.350f, 1 * 0.35f, 0, 1);

                            line_thickness = 2;

                            var hsl = color_fill.GetHSL();
                            hsl.Y += 0.2f;
                            if (hsl.Y > 1)
                                hsl.Y = 1;
                            hsl.Z += 0.2f;
                            if (hsl.Z > 1)
                                hsl.Z = 1;


                            var newFillColor = Utils.HSLtoRGB(hsl.X, hsl.Y, hsl.Z, 1);
                            color_fill = Main.PulseLerpColor(color_fill, newFillColor, 2);
                        }


                        


                        float start = act.StartTime * Graph.LayoutManager.SecondsPixelSize;
                        float end = act.EndTime * Graph.LayoutManager.SecondsPixelSize;


                        var actRect = new Rectangle((int)start, (int)(row * Layout.RowHeight), (int)(end - start), (int)Layout.RowHeight);
                        if (!actRect.Intersects(scrollViewVisibleRect))
                            continue;

                        var actTopLeft = new Vector2(start, row * Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;
                        var actBottomLeft = new Vector2(start, (row * Layout.RowHeight) + Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;

                        var actBottomRight = new Vector2(end, (row * Layout.RowHeight) + Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;
                        var actTopRight = new Vector2(end, row * Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;

                       

                        if (Graph.MainScreen.MultiEditHoverAction == act)
                        {
                            //color_fill = new Color(1f, 0f, 0f, 1f);
                            color_line = Color.Red;


                            drawAfterAllActions += () =>
                            {
                                var copy_eventTopLeft = actTopLeft;
                                var copy_eventBottomLeft = actBottomLeft;
                                var copy_eventBottomRight = actBottomRight;
                                var copy_eventTopRight = actTopRight;

                                var new_eventTopLeft = copy_eventTopLeft + new Vector2(-4, -4);
                                var new_eventBottomLeft = actBottomLeft + new Vector2(-4, 4);
                                var new_eventBottomRight = actBottomRight + new Vector2(4, 4);
                                var new_eventTopRight = actTopRight + new Vector2(4, -4);

                                var oldViewport = gd.Viewport;
                                gd.Viewport = new Viewport(Graph.ScrollViewer.Viewport.DpiScaled());
                                ImGuiDebugDrawer.DrawRect(copy_eventTopLeft + new Vector2(-4, -4), new Vector2(actRect.Width + 8, 4), Color.Red);
                                ImGuiDebugDrawer.DrawRect(copy_eventTopLeft + new Vector2(-4, -4), new Vector2(4, actRect.Height + 8), Color.Red);
                                ImGuiDebugDrawer.DrawRect(copy_eventBottomLeft + new Vector2(-4, 0), new Vector2(actRect.Width + 8, 4), Color.Red);
                                ImGuiDebugDrawer.DrawRect(copy_eventTopRight + new Vector2(0, -4), new Vector2(4, actRect.Height + 8), Color.Red);
                                gd.Viewport = oldViewport;
                            };
                        }

                        // Adjust for row dividers getting in the way
                        actTopLeft.Y += 1;
                        actTopRight.Y += 1;
                        actBottomLeft.Y -= 1;
                        actBottomRight.Y -= 1;
                        actRect.Y += 1;
                        actRect.Height -= 2;


                        var diamondPointLeft = new Vector2(start - Layout.DiamondWidth, (row * Layout.RowHeight) + Layout.RowHeight / 2f) - Graph.ScrollViewer.RoundedScroll;
                        var diamondPointRight = new Vector2(end + Layout.DiamondWidth, (row * Layout.RowHeight) + Layout.RowHeight / 2f) - Graph.ScrollViewer.RoundedScroll;

                        if (Graph.InputMan.CurrentDragType is NewGraphInput.DragTypes.ResizeLeft or NewGraphInput.DragTypes.ResizeRight or NewGraphInput.DragTypes.Move
                            && dragActions.Contains(act))
                        {
                            color_fill = color_fill.MultBrightness(0.5f);
                            color_line = color_line.MultBrightness(0.5f);
                            color_text = color_text.MultBrightness(0.5f);
                        }
                        else if (!act.IsActive_BasedOnMuteSolo)
                        {
                            color_fill = color_fill.MultBrightness(0.6f);
                            color_line = color_line.MultBrightness(0.6f);
                            color_text = color_text.MultBrightness(0.6f);
                        }

                        if (Graph.MainScreen.HighlightOpacityDictCopy.ContainsKey(act))
                        {
                            float meme = 4;
                            float highlightOpacity = Graph.MainScreen.HighlightOpacityDictCopy[act];
                            if (highlightOpacity > 0)
                            {
                                drawAfterAllActions += () =>
                                {
                                    float meme = 4;
                                    Color c = Color.Yellow.MultiplyAlpha(highlightOpacity * 0.75f);
                                
                                    var copy_eventTopLeft = actTopLeft;
                                    var copy_eventBottomLeft = actBottomLeft;
                                    var copy_eventBottomRight = actBottomRight;
                                    var copy_eventTopRight = actTopRight;

                                    var new_eventTopLeft = copy_eventTopLeft + new Vector2(-meme, -meme);
                                    var new_eventBottomLeft = actBottomLeft + new Vector2(-meme, meme);
                                    var new_eventBottomRight = actBottomRight + new Vector2(meme, meme);
                                    var new_eventTopRight = actTopRight + new Vector2(meme, -meme);

                                    var oldViewport = gd.Viewport;
                                    gd.Viewport = new Viewport(Graph.ScrollViewer.Viewport.DpiScaled());
                                    ImGuiDebugDrawer.DrawRect(copy_eventTopLeft + new Vector2(-meme, -meme), new Vector2(actRect.Width + (meme * 2), meme), c, customClippingRect: Graph.Rect);
                                    
                                    ImGuiDebugDrawer.DrawRect(copy_eventBottomLeft + new Vector2(-meme, 0), new Vector2(actRect.Width + (meme * 2), meme), c, customClippingRect: Graph.Rect);
                                    
                                    
                                    ImGuiDebugDrawer.DrawRect(copy_eventTopLeft + new Vector2(-meme, 0), new Vector2(meme, actRect.Height), c, customClippingRect: Graph.Rect);
                                    ImGuiDebugDrawer.DrawRect(copy_eventTopRight + new Vector2(0, 0), new Vector2(meme, actRect.Height), c, customClippingRect: Graph.Rect);
                                    gd.Viewport = oldViewport;
                                };
                            }
                            
                            // var oldViewport2 = gd.Viewport;
                            // gd.Viewport = new Viewport(Graph.ScrollViewer.Viewport.DpiScaled());
                            
                            //ImGuiDebugDrawer.DrawRect(highlightPos, highlightSize, Color.Yellow.MultiplyAlpha(highlightOpacity * 0.5f));
                        }
                        
                        if (act.Solo && Graph.MainScreen.MultiEditHoverAction != act)
                        {
                            color_line = Color.Lime;


                            drawAfterAllActions += () =>
                            {
                                float meme = 1.5f;
                                Color c = Color.White;
                                
                                var copy_eventTopLeft = actTopLeft;
                                var copy_eventBottomLeft = actBottomLeft;
                                var copy_eventBottomRight = actBottomRight;
                                var copy_eventTopRight = actTopRight;

                                var new_eventTopLeft = copy_eventTopLeft + new Vector2(-meme, -meme);
                                var new_eventBottomLeft = actBottomLeft + new Vector2(-meme, meme);
                                var new_eventBottomRight = actBottomRight + new Vector2(meme, meme);
                                var new_eventTopRight = actTopRight + new Vector2(meme, -meme);

                                var oldViewport = gd.Viewport;
                                gd.Viewport = new Viewport(Graph.ScrollViewer.Viewport.DpiScaled());
                                ImGuiDebugDrawer.DrawRect(copy_eventTopLeft + new Vector2(-meme, -meme), new Vector2(actRect.Width + (meme * 2), meme), c);
                                ImGuiDebugDrawer.DrawRect(copy_eventTopLeft + new Vector2(-meme, -meme), new Vector2(meme, actRect.Height + (meme * 2)), c);
                                ImGuiDebugDrawer.DrawRect(copy_eventBottomLeft + new Vector2(-meme, 0), new Vector2(actRect.Width + (meme * 2), meme), c);
                                ImGuiDebugDrawer.DrawRect(copy_eventTopRight + new Vector2(0, -meme), new Vector2(meme, actRect.Height + (meme * 2)), c);
                                gd.Viewport = oldViewport;
                            };
                        }
                        

                        //ImGuiDebugDrawer.DrawTriangle(diamondPointLeft, eventTopLeft, eventBottomLeft, eventShapeFillColor);
                        //ImGuiDebugDrawer.DrawTriangle(diamondPointRight, eventBottomRight, eventTopRight, eventShapeFillColor);
                        ImGuiDebugDrawer.DrawRect(actTopLeft, actBottomRight - actTopLeft, color_fill);
                        //ImGuiDebugDrawer.DrawLine(diamondPointLeft, eventTopLeft, Color.Black);
                        //ImGuiDebugDrawer.DrawLine(eventTopLeft, eventTopRight, Color.Black);
                        //ImGuiDebugDrawer.DrawLine(eventTopRight, diamondPointRight, Color.Black);
                        //ImGuiDebugDrawer.DrawLine(diamondPointRight, eventBottomRight, Color.Black);
                        //ImGuiDebugDrawer.DrawLine(eventBottomRight, eventBottomLeft, Color.Black);
                        //ImGuiDebugDrawer.DrawLine(eventBottomLeft, diamondPointLeft, Color.Black);


                        //ImGuiDebugDrawer.DrawLine(actTopLeft, actTopRight, color_line);
                        //ImGuiDebugDrawer.DrawLine(actTopRight, actBottomRight, color_line);
                        //ImGuiDebugDrawer.DrawLine(actBottomLeft, actBottomRight, color_line);
                        //ImGuiDebugDrawer.DrawLine(actTopLeft, actBottomLeft, color_line);

                        ImGuiDebugDrawer.DrawRect(actTopLeft + new Vector2(line_thickness - 1, 0), 
                            (actBottomRight - actTopLeft) + new Vector2(2 - line_thickness, 1), 
                            color_line, thickness: line_thickness);


                        Vector2 mousePos = Graph.MainScreen.Input.MousePosition - gd.Viewport.Bounds.TopLeftCorner();

                        //TODO: Maybe move hover into input update
                        //if (mousePos.X >= eventTopLeft.X && mousePos.X <= eventTopRight.X && mousePos.Y >= eventTopLeft.Y && mousePos.Y <= eventBottomLeft.Y)
                        //{
                        //    Graph.MainScreen.NewHoverEvent = ev;
                        //    anyHoverEvent = true;
                        //}

                        //if (Graph.MainScreen.Input.ShiftHeld || Graph.MainScreen.NewHoverEvent == ev)
                        //{
                        //    if (ev.GraphDisplayText == null)
                        //        ev.UpdateGraphDisplayText();
                        //    var eventText = ev.GraphDisplayText ?? "";
                        //    if (!string.IsNullOrEmpty(eventText))
                        //    {
                        //        ImGuiDebugDrawer.DrawText(eventText, eventTopLeft + new Vector2(2, 1), Color.White, fontSize: 16);
                        //    }
                        //}


                        var eventRectOnscreenLeft = Math.Max(actRect.Left, scrollViewVisibleRect.Left);
                        var eventRectOnscreenRight = Math.Min(actRect.Right, scrollViewVisibleRect.Right);
                        var eventRectOnscreenTop = Math.Max(actRect.Top, scrollViewVisibleRect.Top);
                        var eventRectOnscreenBottom = Math.Min(actRect.Bottom, scrollViewVisibleRect.Bottom);

                        var eventOnscreenViewportRect = new Rectangle((int)(eventRectOnscreenLeft - Graph.ScrollViewer.RoundedScroll.X) + Graph.WholeScrollViewerRect.Left, 
                            (int)(eventRectOnscreenTop - Graph.ScrollViewer.RoundedScroll.Y + Graph.WholeScrollViewerRect.Top),
                            eventRectOnscreenRight - eventRectOnscreenLeft, eventRectOnscreenBottom - eventRectOnscreenTop);

                        var oldViewport2 = gd.Viewport;
                        gd.Viewport = new Viewport(eventOnscreenViewportRect.DpiScaled());
                        {
                            if (act.GraphDisplayText == null || act.NeedsTextRefresh || act.TooltipDisplayText == null)
                                act.UpdateGraphDisplayText();
                            var eventText = act.GraphDisplayText ?? "";
                            ImGuiDebugDrawer.DrawText(eventText, new Vector2(4 * Main.DPI, -1 + (0.4f * Main.DPI)).RoundInt(), color_text, color_text_shadow, fontSize: 15, includeCardinalShadows: true, shadowThickness: 1);
                        }
                        gd.Viewport = oldViewport2;
                        
                        if (!act.IsActive_BasedOnStateInfo)
                        {
                            ImGuiDebugDrawer.DrawLine(actTopLeft, actBottomRight, Color.Red);
                            ImGuiDebugDrawer.DrawLine(actBottomLeft, actTopRight, Color.Red);
                        }



                        lock (proj._lock_Tags)
                        {
                            if (act.Info.TagInstances.Count > 0)
                            {
                                act.Info.DrawJustTheTags(proj, actTopRight + new Vector2(0, 1.5f), isRightAnchor: true, actRect, tagsDrawScale: 0.69f, includeXButtons: false,
                                    setIsModified: isModified =>
                                    {
                                        if (isModified)
                                            Graph.AnimRef.SAFE_SetIsModified(true);
                                    });
                            }
                        }
                    }

                    
                    if (Graph.MainScreen.NewHoverAction_NeedsNoSelection != null && Main.Config.ShowActionTooltips 
                        && !MenuBar.IsAnyMenuOpen && !DialogManager.AnyDialogsShowing && (Graph.PlaybackCursor?.IsPlaying == false)
                        && !OSD.Hovered)
                    {
                        Graph.MainScreen.NewHoverAction_NeedsNoSelection?.CheckUpdateText();
                        OSD.TooltipManager_GraphActions.DoTooltipManual($"ActionHover", Graph.MainScreen.NewHoverAction_NeedsNoSelection.TooltipDisplayText, true);
                    }
                    // else
                    // {
                    //     OSD.TooltipManager_GraphActions.CancelTooltip();
                    // }
                    //

                    if ((Graph.PlaybackCursor.IsPlaying && Main.Config.AutoScrollDuringAnimPlayback) || (Main.Config.NewRelativeTimelineScrubEnabled && Main.Config.NewRelativeTimelineScrubScrollScreen && Graph.PlaybackCursor.Scrubbing))
                    {
                        Graph.LayoutManager.ScrollToPlaybackCursor(-1, modTime: true);
                    }

                    //if (!anyHoverEvent)
                    //    Graph.MainScreen.NewHoverEvent = null;

                    ImGuiDebugDrawer.DrawRect(new Vector2(0, Graph.AnimRef.SAFE_GetActionTrackCount() * Layout.RowHeight - Graph.ScrollViewer.RoundedScroll.Y),
                        Graph.LayoutManager.GetVirtualAreaSize().ToVector2(), Main.Colors.GuiColorActionGraphAnimEndDarkenRect);


                    if (Graph.IsGhostGraph)
                        ImGuiDebugDrawer.DrawRect(new Vector2(-4, -4), Graph.Rect.Size.ToVector2() + new Vector2(4, 4),
                        Main.Colors.GuiColorActionGraphGhostOverlay, 0, thickness: 0);

                    Graph.InputMan.DrawGizmos();
                }
                finally
                {
                    sb.End();
                }
            }






            var trackPaneRect = ImguiOSD.OSD.SpWindowGraph_Tracks.GetRect();
            float margin = Graph.ActionTrackNamePaneEffectiveWidth -
                           trackPaneRect.Width;
                    
            margin -= 5;



            
            viewportRect = new Rectangle(Graph.ActionTrackHeaderTextRect.X, Graph.ActionTrackHeaderTextRect.Y, 
                (int)Graph.ActionTrackHeaderTextRect.Width, Graph.ScrollViewer.Viewport.Height);

            viewportRect.Width -= (int)Math.Round(margin + Graph.SpacingBetweenEditButtonAndText);
            
            gd.Viewport = new Viewport(viewportRect.DpiScaled());
            {
                sb.Begin(transformMatrix: scrolLMatrix_YOnly * Main.DPIMatrix);
                try
                {
                    //ImGuiDebugDrawer.DrawText("TRACK NAMES\nTEST 1\nTEST 2", Vector2.Zero, Color.Lime, Color.Black);

                    // Event groups alternating colors
                    for (int i = 0; i < actionTracksCopy.Count; i++)
                    {
                        

                        if (actionTracksCopy[i].PlaybackHighlight)
                            ImGuiDebugDrawer.DrawRect(new Vector2(0, i * Layout.RowHeight) + new Vector2(0, -Graph.ScrollViewer.RoundedScroll.Y), new Vector2(gd.Viewport.Width / Main.DPI, Layout.RowHeight), Color.White * 0.5f);

                        if (Graph.MainScreen.HighlightOpacityDictCopy.ContainsKey(actionTracksCopy[i]))
                        {
                            float highlightOpacity = Graph.MainScreen.HighlightOpacityDictCopy[actionTracksCopy[i]];
                            ImGuiDebugDrawer.DrawRect(new Vector2(0, i * Layout.RowHeight) + new Vector2(0, -Graph.ScrollViewer.RoundedScroll.Y), new Vector2(gd.Viewport.Width / Main.DPI, Layout.RowHeight), Color.Yellow.MultiplyAlpha(highlightOpacity * 0.5f));
                        }
                        
                        if (i % 2 == 0)
                        {
                            ImGuiDebugDrawer.DrawRect(new Vector2(0, i * Layout.RowHeight) + new Vector2(0, -Graph.ScrollViewer.RoundedScroll.Y), new Vector2(gd.Viewport.Width / Main.DPI, Layout.RowHeight), Color.Black * 0.3f);

                        }
                    }

                    //
                    for (int i = 0; i < actionTracksCopy.Count; i++)
                    {
                        ImGuiDebugDrawer.DrawText(actionTracksCopy[i].Info.DisplayName ?? $"%null% (Row {i + 1})", new Vector2(2, (i * Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll.Y - 4), 
                            Color.White, Color.Black);


                    }

                    // Thicc black line on right
                    //ImGuiDebugDrawer.DrawRect(new Vector2(gd.Viewport.Width - 4, 0), new Vector2(4, gd.Viewport.Height), Color.White);

                    ImGuiDebugDrawer.DrawRect(new Vector2(0, Graph.AnimRef.SAFE_GetActionTrackCount() * Layout.RowHeight - Graph.ScrollViewer.RoundedScroll.Y),
                        Graph.LayoutManager.GetVirtualAreaSize().ToVector2(), Main.Colors.GuiColorActionGraphAnimEndDarkenRect);
                    
                    
                    
                    
                    ImGuiDebugDrawer.DrawRect(new Vector2(Graph.ActionTrackHeaderTextRect.Width - margin, 0),
                        new Vector2(margin, trackPaneRect.Height),
                        Color.Black * 0.5f, thickness: 0);
                }
                finally
                {
                    sb.End();
                }

                if (Graph.IsGhostGraph)
                    ImGuiDebugDrawer.DrawRect(new Vector2(-4, -4), Graph.Rect.Size.ToVector2() + new Vector2(4, 4),
                    Main.Colors.GuiColorActionGraphGhostOverlay, 0, thickness: 0);

            }
            
            
            viewportRect = new Rectangle(Graph.ActionTrackHeaderEditButtonRect.X, Graph.ActionTrackHeaderEditButtonRect.Y, 
                (int)Graph.ActionTrackHeaderEditButtonRect.Width, Graph.ScrollViewer.Viewport.Height);
            gd.Viewport = new Viewport(viewportRect.DpiScaled());
            {
                
                
                
                sb.Begin(transformMatrix: scrolLMatrix_YOnly * Main.DPIMatrix);
                try
                {
                    //ImGuiDebugDrawer.DrawText("TRACK NAMES\nTEST 1\nTEST 2", Vector2.Zero, Color.Lime, Color.Black);

                    // Event groups alternating colors
                    for (int i = 0; i < actionTracksCopy.Count; i++)
                    {
                        //test
                        ImGuiDebugDrawer.DrawRect(new Vector2(0, i * Layout.RowHeight) + new Vector2(0, -Graph.ScrollViewer.RoundedScroll.Y),
                            new Vector2(Graph.ActionTrackEditButtonWidth, Graph.ActionTrackEditButtonHeight),
                            Color.Yellow, thickness: 1);

                        var editButtonPos = new Vector2(0, i * Layout.RowHeight) +
                                            new Vector2(0, -Graph.ScrollViewer.RoundedScroll.Y);

                        var editButtonSize = new Vector2(Graph.ActionTrackEditButtonWidth,
                            Graph.ActionTrackEditButtonHeight);

                        // if (ghostGraph)
                        // {
                        //     editButtonSize = Vector2.Zero;
                        //     editButtonPos = Vector2.Zero;
                        // }

                        bool editButtonPressed = ImGuiDebugDrawer.FakeButton(editButtonPos, editButtonSize, "Edit", 0,
                            out bool isHovering, overrideFontSize: Graph.ActionTrackEditButtonFontSize,
                            absoluteClippingRect: Graph.ActionTrackHeaderEditButtonRect, disableInput: ghostGraph);

                        if (ghostGraph)
                        {
                            editButtonPressed = false;
                        }
                        
                        if (!ImguiOSD.MenuBar.IsAnyMenuOpen && !ImguiOSD.DialogManager.AnyDialogsShowing && editButtonPressed)
                        {
                            //Graph.MainScreen.ShowDialogEditActionTrackProperties(i, actionTracksCopy[i]);
                            var track = actionTracksCopy[i];
                            var trackIndex = i;
                            ImguiOSD.DialogManager.ShowTaeActionTrackPropertiesEditor(Graph.AnimRef, trackIndex, track);
                        }
                    }

                   
                }
                finally
                {
                    sb.End();
                }

                if (Graph.IsGhostGraph)
                    ImGuiDebugDrawer.DrawRect(new Vector2(-4, -4), Graph.Rect.Size.ToVector2() + new Vector2(4, 4),
                    Main.Colors.GuiColorActionGraphGhostOverlay, 0, thickness: 0);

            }
            
            
            viewportRect = new Rectangle(Graph.WholeScrollViewerRect.X, Graph.Rect.Y, (int)Graph.WholeScrollViewerRect.Width - NewGraph.SpacingMemeBottomRightOfGraph, Graph.Rect.Height - NewGraph.SpacingMemeBottomRightOfGraph);
            gd.Viewport = new Viewport(viewportRect.DpiScaled());
            {
                sb.Begin(transformMatrix: scrolLMatrix_YOnly * Main.DPIMatrix);
                try
                {
                    //Playback cursor playback start time
                    float playbackCursorStartX = (float)((Graph.PlaybackCursor.GUIStartTime * Layout.SecondsPixelSize) - Graph.ScrollViewer.RoundedScroll.X);
                    playbackCursorStartX = (float)Math.Round(playbackCursorStartX);
                    
                    // Playback cursor
                    float playbackCursorModX = (float)((Graph.PlaybackCursor.GUICurrentTimeMod * Layout.SecondsPixelSize) - Graph.ScrollViewer.RoundedScroll.X);
                    playbackCursorModX = (float)Math.Round(playbackCursorModX);
                    
                    // transparent non-wrapped playback cursor
                    float playbackCursorX = (float)((Graph.PlaybackCursor.GUICurrentTime * Layout.SecondsPixelSize) - Graph.ScrollViewer.RoundedScroll.X);
                    playbackCursorX = (float)Math.Round(playbackCursorX);
                    
                    ImGuiDebugDrawer.DrawLine(new Vector2(playbackCursorStartX, 0), new Vector2(playbackCursorStartX, Graph.Rect.Height), Main.Config.Colors.GuiColorActionGraphPlaybackStartTime_V2);
                    ImGuiDebugDrawer.DrawLine(new Vector2(playbackCursorModX, 0), new Vector2(playbackCursorModX, Graph.Rect.Height), Main.Config.Colors.GuiColorActionGraphPlaybackCursor_V3);

                    
                    if (playbackCursorX > playbackCursorModX)
                    {
                        ImGuiDebugDrawer.DrawLine(new Vector2(playbackCursorX, 0), new Vector2(playbackCursorX, Graph.Rect.Height), Main.Config.Colors.GuiColorActionGraphPlaybackCursor_V3 * 0.5f);
                    }

                    // darkening after end of anim
                    float darkenStartX = (float)((Graph.PlaybackCursor.MaxTime * Layout.SecondsPixelSize) -
                                                 Graph.ScrollViewer.RoundedScroll.X);
                    ImGuiDebugDrawer.DrawRect(new Vector2(Math.Max(darkenStartX, 0), 0),
                        new Vector2(viewportRect.Width + 1, viewportRect.Height + 1), Main.Config.Colors.GuiColorActionGraphAnimEndDarkenRect);




                }
                finally
                {
                    sb.End();
                }
            }
            gd.Viewport = oldViewport;




            // White line divider lol
            int spacingMeme = 0;

            viewportRect = new Rectangle(Graph.Rect.X - spacingMeme, Graph.Rect.Y - spacingMeme, (int)Graph.Rect.Width + (spacingMeme * 2), Graph.Rect.Height + (spacingMeme * 2));
            gd.Viewport = new Viewport(viewportRect.DpiScaled());
            {
                sb.Begin(transformMatrix: scrolLMatrix_YOnly * Main.DPIMatrix);
                try
                {
                   

                    // Horizontal white line on top of whole graph
                    ImGuiDebugDrawer.DrawRect(new Vector2(0, 0), new Vector2(viewportRect.Width, 1), Color.White);

                    // Horizontal white line on bottom of whole graph
                    ImGuiDebugDrawer.DrawRect(new Vector2(0, viewportRect.Height - 1), new Vector2(viewportRect.Width, 1), Color.White);

                    // Vertical white line on left of whole graph
                    ImGuiDebugDrawer.DrawRect(new Vector2(0, 0), new Vector2(1, viewportRect.Height), Color.White);

                    // Vertical white line on right of whole graph
                    ImGuiDebugDrawer.DrawRect(new Vector2(viewportRect.Width - 1, 0), new Vector2(1, viewportRect.Height), Color.White);

                    // Vertical white line splitting left/right panes
                    //ImGuiDebugDrawer.DrawRect(new Vector2(Graph.ActionTrackHeaderRect.Width - 1 + spacingMeme, 0), new Vector2(1, viewportRect.Height), Color.White);

                    // Horizontal white line on bottom of timeline
                    ImGuiDebugDrawer.DrawRect(new Vector2(0, Graph.TimeLineHeight - 2 + spacingMeme), new Vector2(viewportRect.Width, 1), Color.White);
                }
                finally
                {
                    sb.End();
                }
            }

            drawAfterAllActions?.Invoke();



            gd.Viewport = oldViewport;

            Graph.ScrollViewer.Draw(gd, sb, boxTex, scrollbarArrowTex);


            gd.Viewport = oldViewport;

        }
    }
}
