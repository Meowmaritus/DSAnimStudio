using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;

namespace TAEDX.TaeEditor
{
    public class TaeEditAnimList
    {
        public class TaeEditAnimInfo
        {
            public Func<string> GetName;
            public TAE.Animation Ref;
            public float VerticalOffset;
            public long TaePrefix;
            public long FullID => (TaePrefix * 10000) + Ref.ID;
        }

        public class TaeEditAnimListTaeSection
        {
            public string SectionName;
            public TAE Tae;
            public Dictionary<TAE.Animation, TaeEditAnimInfo> InfoMap = new Dictionary<TAE.Animation, TaeEditAnimInfo>();
            public float HeightOfAllAnims = 0;
            public bool Collapsed = false;
        }

        public Rectangle Rect;
        public readonly TaeEditorScreen MainScreen;

        public TaeScrollViewer ScrollViewer;

        private int AnimHeight = 24;

        public List<TaeEditAnimListTaeSection> AnimTaeSections = new List<TaeEditAnimListTaeSection>();

        int GroupBraceMarginLeft = 16;
        //int GroupBraceThickness = 2;

        public void ScrollToAnimRef(TAE.Animation anim, bool scrollOnCenter)
        {
            if (anim == null)
                return;
            float verticalOffset = 0;
            foreach (var section in AnimTaeSections)
            {
                verticalOffset += AnimHeight;
                if (section.InfoMap.ContainsKey(anim))
                {
                    // Un-collapse section where anim is.
                    if (section.Collapsed)
                        section.Collapsed = false;
                    verticalOffset += section.InfoMap[anim].VerticalOffset;
                    break;
                }
                else if (!section.Collapsed)
                {
                    verticalOffset += section.HeightOfAllAnims;
                }
            }

            if (scrollOnCenter)
            {
                ScrollViewer.Scroll.Y = (verticalOffset + AnimHeight / 2f) - (ScrollViewer.Viewport.Height / 2f);
            }
            else
            {
                if (ScrollViewer.Scroll.Y > (verticalOffset - AnimHeight))
                {
                    ScrollViewer.Scroll.Y = verticalOffset - AnimHeight;
                }
                else if (ScrollViewer.Scroll.Y + Rect.Height < verticalOffset + (AnimHeight * 2))
                {
                    ScrollViewer.Scroll.Y = verticalOffset + (AnimHeight * 2) - Rect.Height;
                }
            }

            
        }

        public TaeEditAnimList(TaeEditorScreen mainScreen)
        {
            MainScreen = mainScreen;

            //if (MainScreen.FileContainer.StandardTAE != null)
            //{
            //    foreach (var kvp in MainScreen.FileContainer.StandardTAE)
            //    {
            //        var taeSection = new TaeEditAnimListTaeSection();
            //        taeSection.Collapsed = MainScreen.Config.AutoCollapseAllTaeSections;
            //        taeSection.Tae = kvp.Value;
            //        taeSection.SectionName = System.IO.Path.GetFileName(kvp.Value.FilePath ?? kvp.Value.VirtualUri);
            //        foreach (var anim in kvp.Value.Animations)
            //        {
            //            var animID_Lower = anim.ID % 10000;
            //            var animID_Upper = anim.ID / 10000;
            //            var info = new TaeEditAnimInfo()
            //            {
            //                GetName = () => $"a{(kvp.Key + animID_Upper):D2}_{animID_Lower:D4}",
            //                Ref = anim,
            //                VerticalOffset = taeSection.HeightOfAllAnims,
            //                TaePrefix = kvp.Key,
            //            };

            //            taeSection.InfoMap.Add(anim, info);
            //            taeSection.HeightOfAllAnims += AnimHeight;
            //        }
            //        AnimTaeSections.Add(taeSection);
            //    }
            //}

            //if (MainScreen.FileContainer.PlayerTAE != null)
            //{
            //    foreach (var kvp in MainScreen.FileContainer.PlayerTAE)
            //    {
            //        var taeSection = new TaeEditAnimListTaeSection();
            //        taeSection.Collapsed = MainScreen.Config.AutoCollapseAllTaeSections;
            //        taeSection.Tae = kvp.Value;
            //        taeSection.SectionName = System.IO.Path.GetFileName(kvp.Value.FilePath ?? kvp.Value.VirtualUri);
            //        foreach (var anim in kvp.Value.Animations)
            //        {
            //            var animID_Lower = anim.ID % 10000;
            //            var animID_Upper = anim.ID / 10000;
            //            var info = new TaeEditAnimInfo()
            //            {
            //                GetName = () => $"a{(kvp.Key + animID_Upper):D2}_{animID_Lower:D4}",
            //                Ref = anim,
            //                VerticalOffset = taeSection.HeightOfAllAnims,
            //                TaePrefix = kvp.Key,
            //            };

            //            taeSection.InfoMap.Add(anim, info);
            //            taeSection.HeightOfAllAnims += AnimHeight;
            //        }
            //        AnimTaeSections.Add(taeSection);
            //    }
            //}

            foreach (var kvp in MainScreen.FileContainer.AllTAEDict)
            {
                var tae = kvp.Value;
                var taeSection = new TaeEditAnimListTaeSection();
                taeSection.Collapsed = MainScreen.Config.AutoCollapseAllTaeSections;
                taeSection.Tae = tae;
                taeSection.SectionName = System.IO.Path.GetFileName(kvp.Key);
                foreach (var anim in tae.Animations)
                {
                    var animID_Lower = MainScreen.FileContainer.ContainerType == TaeFileContainer.TaeFileContainerType.BND4 
                        ? (anim.ID % 1000000) : (anim.ID % 10000);

                    var animID_Upper = MainScreen.FileContainer.ContainerType == TaeFileContainer.TaeFileContainerType.BND4 
                        ? (anim.ID / 1000000) : (anim.ID / 10000);

                    var info = new TaeEditAnimInfo()
                    {
                        GetName = () => MainScreen.FileContainer.ContainerType == TaeFileContainer.TaeFileContainerType.BND4 
                          ? $"a{(animID_Upper):D3}_{animID_Lower:D6}" : $"a{(animID_Upper):D2}_{animID_Lower:D4}",
                        Ref = anim,
                        VerticalOffset = taeSection.HeightOfAllAnims,
                        TaePrefix = animID_Upper,
                    };

                    taeSection.InfoMap.Add(anim, info);
                    taeSection.HeightOfAllAnims += AnimHeight;
                }
                AnimTaeSections.Add(taeSection);
            }

            ScrollViewer = new TaeScrollViewer();
        }

        public void Update(float elapsedSeconds, bool allowMouseUpdate)
        {
            if (!allowMouseUpdate)
                return;

            ScrollViewer.UpdateInput(MainScreen.Input, elapsedSeconds, true);

            if (Rect.Contains(MainScreen.Input.MousePositionPoint))
            {
                MainScreen.Input.CursorType = MouseCursorType.Arrow;

                if (MainScreen.Input.LeftClickDown)
                {
                    var mouseCheckPoint = new Point(
                        (int)(MainScreen.Input.MousePosition.X - Rect.X + ScrollViewer.Scroll.X),
                        (int)(MainScreen.Input.MousePosition.Y - Rect.Y + ScrollViewer.Scroll.Y));

                    float offset = 0;
                    foreach (var taeSection in AnimTaeSections)
                    {
                        var thisGroupRect = new Rectangle(0, (int)offset, ScrollViewer.Viewport.Width, AnimHeight);

                        if (thisGroupRect.Contains(mouseCheckPoint))
                            taeSection.Collapsed = !taeSection.Collapsed;

                        offset += AnimHeight; //Section Header

                        if (taeSection.Collapsed)
                            continue;

                        float groupStartOffset = offset;
                        foreach (var anim in taeSection.InfoMap)
                        {
                            var thisAnimRect = new Rectangle(0, (int)(groupStartOffset + anim.Value.VerticalOffset),
                                ScrollViewer.Viewport.Width, AnimHeight);
                            if (thisAnimRect.Contains(mouseCheckPoint))
                                MainScreen.SelectNewAnimRef(taeSection.Tae, anim.Value.Ref);
                            offset += AnimHeight;
                        }
                    }
                }
            }

            if (MainScreen.Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Insert))
            {
                MainScreen.AddNewAnimation();
            }
        }

        public void UpdateMouseOutsideRect(float elapsedSeconds, bool allowMouseUpdate)
        {
            if (!allowMouseUpdate)
                return;

            ScrollViewer.UpdateInput(MainScreen.Input, elapsedSeconds, allowScrollWheel: false);

            if (MainScreen.Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Insert))
            {
                MainScreen.AddNewAnimation();
            }
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font)
        {
            float EntireListHeight = 0;
            foreach (var section in AnimTaeSections)
            {
                EntireListHeight += AnimHeight;
                if (!section.Collapsed)
                    EntireListHeight += section.HeightOfAllAnims;
            }

            EntireListHeight += AnimHeight;

            ScrollViewer.SetDisplayRect(Rect, new Point(Rect.Width, (int)EntireListHeight));

            ScrollViewer.Draw(gd, sb, boxTex, font);

            var oldViewport = gd.Viewport;
            gd.Viewport = new Viewport(ScrollViewer.Viewport);
            {
                sb.Begin(transformMatrix: ScrollViewer.GetScrollMatrix());

                //sb.Draw(texture: boxTex,
                //    position: Vector2.Zero,
                //    sourceRectangle: null,
                //    color: Color.DarkGray,
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2(Rect.Width, Rect.Height),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );

                float offset = 0;

                foreach (var taeSection in AnimTaeSections)
                {
                    var thisGroupRect = new Rectangle(1, (int)offset + 1, ScrollViewer.Viewport.Width - 2, AnimHeight - 2);
                    int border = MainScreen.Config.EnableColorBlindMode ? 2 : 1;
                    sb.Draw(boxTex, thisGroupRect, Color.White);
                    sb.Draw(boxTex, 
                        new Rectangle(
                            thisGroupRect.X + border, 
                            thisGroupRect.Y + border, 
                            thisGroupRect.Width - border * 2, 
                            thisGroupRect.Height - border * 2), 
                        MainScreen.Config.EnableColorBlindMode ? Color.Black : Color.Gray);
                    sb.DrawString(font, $"[{taeSection.SectionName}]", new Vector2(4 + AnimHeight, (int)(offset)) + Vector2.One, Color.Black);
                    sb.DrawString(font, $"[{taeSection.SectionName}]", new Vector2(4 + AnimHeight, (int)(offset)) + (Vector2.One * 2), Color.Black);
                    sb.DrawString(font, $"[{taeSection.SectionName}]", new Vector2(4 + AnimHeight, (int)(offset)), Color.White);


                    Rectangle sectionCollapseButton = new Rectangle(
                            (int)(AnimHeight / 4f),
                            (int)(offset + AnimHeight / 4f),
                            (int)(AnimHeight / 2f),
                            (int)(AnimHeight / 2f));

                    sb.Draw(texture: boxTex,
                            position: new Vector2(sectionCollapseButton.X, sectionCollapseButton.Y),
                            sourceRectangle: null,
                            color: Color.White,
                            rotation: 0,
                            origin: Vector2.Zero,
                            scale: new Vector2(sectionCollapseButton.Width, sectionCollapseButton.Height),
                            effects: SpriteEffects.None,
                            layerDepth: 0.01f
                            );

                    var collapseStr = taeSection.Collapsed ? "＋" : "－";

                    var collapseStrScale = font.MeasureString(collapseStr);

                    Vector2 collapseStrPoint =
                        new Vector2(AnimHeight / 2, offset + AnimHeight / 2 + 1)
                        - (collapseStrScale / 2);

                    collapseStrPoint = new Vector2((int)collapseStrPoint.X, (int)collapseStrPoint.Y);

                    sb.DrawString(font, collapseStr, collapseStrPoint, Color.Black);

                    offset += AnimHeight; //Section Header

                    if (taeSection.Collapsed)
                        continue;

                    float sectionStartOffset = offset;

                    foreach (var anim in taeSection.InfoMap)
                    {
                        string animNameStr = (anim.Value.Ref.GetIsModified() ? $"{anim.Value.GetName()}*" : anim.Value.GetName());

                        if (offset + AnimHeight < ScrollViewer.Scroll.Y || offset > ScrollViewer.Scroll.Y + Rect.Height)
                        {
                            offset += AnimHeight;
                            continue;
                        }

                        if (anim.Value.Ref == MainScreen.SelectedTaeAnim)
                        {
                            var thisAnimRect = new Rectangle(
                                GroupBraceMarginLeft, 
                                (int)(sectionStartOffset + anim.Value.VerticalOffset), 
                                ScrollViewer.Viewport.Width - (GroupBraceMarginLeft * 2), 
                                AnimHeight);

                            sb.Draw(boxTex, thisAnimRect, MainScreen.Config.EnableColorBlindMode ? Color.White : Color.Blue);

                            sb.Draw(boxTex,
                                new Rectangle(
                                    thisAnimRect.X + border,
                                    thisAnimRect.Y + border,
                                    thisAnimRect.Width - border * 2,
                                    thisAnimRect.Height - border * 2),
                                MainScreen.Config.EnableColorBlindMode ? Color.Black : Color.CornflowerBlue);

                            sb.DrawString(font, animNameStr, new Vector2(
                                    GroupBraceMarginLeft + 4,
                                    (int)(sectionStartOffset + anim.Value.VerticalOffset)) + Vector2.One, Color.Black);
                            sb.DrawString(font, animNameStr, new Vector2(
                                GroupBraceMarginLeft + 4,
                                (int)(sectionStartOffset + anim.Value.VerticalOffset)), Color.White);
                        }
                        else
                        {
                            sb.DrawString(font, animNameStr, new Vector2(
                                GroupBraceMarginLeft + 4,
                                (int)(sectionStartOffset + anim.Value.VerticalOffset)) + Vector2.One, Color.Black);
                            sb.DrawString(font, animNameStr, new Vector2(
                                GroupBraceMarginLeft + 4,
                                (int)(sectionStartOffset + anim.Value.VerticalOffset)), Color.White);
                        }

                        offset += AnimHeight;
                    }

                    //foreach (var group in taeSection.Tae.AnimationGroups)
                    //{
                    //    float startHeight = taeSection.InfoMap[group.FirstID].VerticalOffset + (AnimHeight / 2f) + sectionStartOffset;
                    //    if (startHeight > (ScrollViewer.Scroll.Y + Rect.Height))
                    //        continue;
                    //    float endHeight = taeSection.InfoMap[group.LastID].VerticalOffset + (AnimHeight / 2f) + sectionStartOffset;
                    //    if (endHeight < ScrollViewer.Scroll.Y)
                    //        continue;

                    //    float startX = GroupBraceMarginLeft / 2f;
                    //    float endX = GroupBraceMarginLeft;

                    //    if (startHeight == endHeight)
                    //    {
                    //        sb.Draw(texture: boxTex,
                    //        position: new Vector2(GroupBraceMarginLeft / 2f, startHeight)
                    //          + new Vector2(-GroupBraceThickness / 2f, -GroupBraceThickness / 2f),
                    //        sourceRectangle: null,
                    //        color: Color.White,
                    //        rotation: 0,
                    //        origin: Vector2.Zero,
                    //        scale: new Vector2((GroupBraceMarginLeft / 2f) + (GroupBraceThickness / 2f), GroupBraceThickness),
                    //        effects: SpriteEffects.None,
                    //        layerDepth: 0
                    //        );
                    //    }
                    //    else
                    //    {
                    //        sb.Draw(texture: boxTex,
                    //        position: new Vector2((GroupBraceMarginLeft / 2f), startHeight)
                    //          + new Vector2(-GroupBraceThickness / 2f, -GroupBraceThickness / 2f),
                    //        sourceRectangle: null,
                    //        color: Color.White,
                    //        rotation: 0,
                    //        origin: Vector2.Zero,
                    //        scale: new Vector2(GroupBraceThickness, endHeight - startHeight + GroupBraceThickness),
                    //        effects: SpriteEffects.None,
                    //        layerDepth: 0
                    //        );

                    //        sb.Draw(texture: boxTex,
                    //            position: new Vector2(GroupBraceMarginLeft / 2f, startHeight)
                    //              + new Vector2(-GroupBraceThickness / 2f, -GroupBraceThickness / 2f),
                    //            sourceRectangle: null,
                    //            color: Color.White,
                    //            rotation: 0,
                    //            origin: Vector2.Zero,
                    //            scale: new Vector2((GroupBraceMarginLeft / 2f) + (GroupBraceThickness / 2f), GroupBraceThickness),
                    //            effects: SpriteEffects.None,
                    //            layerDepth: 0
                    //            );

                    //        sb.Draw(texture: boxTex,
                    //            position: new Vector2(GroupBraceMarginLeft / 2f, endHeight)
                    //              + new Vector2(-GroupBraceThickness / 2f, GroupBraceThickness / 2f),
                    //            sourceRectangle: null,
                    //            color: Color.White,
                    //            rotation: 0,
                    //            origin: Vector2.Zero,
                    //            scale: new Vector2((GroupBraceMarginLeft / 2f) + (GroupBraceThickness / 2f), GroupBraceThickness),
                    //            effects: SpriteEffects.None,
                    //            layerDepth: 0
                    //            );
                    //    }


                    //}
                }
                

                sb.End();
            }
            gd.Viewport = oldViewport;
        }
    }
}
