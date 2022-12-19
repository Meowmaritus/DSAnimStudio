using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSAnimStudio.TaeEditor
{
    public class TaeEditAnimList
    {
        public const int BorderThickness = 1;

        public class TaeEditAnimInfo
        {
            public Func<string> GetName;
            public TAE.Animation Ref;
            public float VerticalOffset;
            public long TaePrefix;
            public bool IsLargeID;
            public bool IsRedErrorPrefix;
            public long FullID => (TaePrefix * (IsLargeID ? 1_000000 : 1_0000)) + Ref.ID;
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

        private int AnimSectionHeaderHeight = 25;
        private int AnimHeight = 20;

        public Dictionary<int, TaeEditAnimListTaeSection> AnimTaeSections = new Dictionary<int, TaeEditAnimListTaeSection>();

        int StartDrawSection = -1;
        int StartDrawAnim = -1;
        float StartDrawSectionOffset = -1;
        float StartDrawAnimOffsetInSection = -1;
        float StartDrawScroll = -1;

        float DevNameStartX = 108;

        public void ClearStartDrawSkip()
        {
            StartDrawSection = -1;
            StartDrawAnim = -1;
            StartDrawSectionOffset = -1;
            StartDrawAnimOffsetInSection = -1;
            StartDrawScroll = -1;
        }

        int GroupBraceMarginLeft = 0;
        //int GroupBraceThickness = 2;

        public void ScrollToAnimRef(TAE.Animation anim, bool scrollOnCenter)
        {
            if (anim == null)
                return;
            float verticalOffset = 0;
            float subOffset = 0;
            foreach (var section in AnimTaeSections.Values)
            {
                verticalOffset += AnimSectionHeaderHeight; //section header
                if (section.InfoMap.ContainsKey(anim))
                {
                    // Un-collapse section where anim is.
                    if (section.Collapsed)
                        section.Collapsed = false;
                    subOffset = section.InfoMap[anim].VerticalOffset;
                    break;
                }

                if (!section.Collapsed)
                {
                    verticalOffset += section.HeightOfAllAnims;
                }
            }

            verticalOffset += subOffset;

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

            bool isMultiSectionTae = MainScreen.FileContainer.AllTAEDict.Count > 1;

            foreach (var kvp in MainScreen.FileContainer.AllTAEDict)
            {
                var tae = kvp.Value;
                var taeSection = new TaeEditAnimListTaeSection();
                taeSection.Collapsed = MainScreen.Config.AutoCollapseAllTaeSections;
                taeSection.Tae = tae;
                taeSection.SectionName = System.IO.Path.GetFileName(kvp.Key);
                foreach (var anim in tae.Animations)
                {
                    var animID_Lower = isMultiSectionTae ? anim.ID : GameRoot.GameTypeHasLongAnimIDs
                        ? (anim.ID % 1_000000) : (anim.ID % 1_0000);

                    var animID_Upper = taeSection.SectionName.StartsWith("a") ? 
                        long.Parse(Utils.GetFileNameWithoutAnyExtensions(taeSection.SectionName).Substring(1)) 
                        : ((GameRoot.GameTypeHasLongAnimIDs
                            ? (anim.ID / 1_000000) : (anim.ID / 1_0000)));

                    var info = new TaeEditAnimInfo()
                    {
                        GetName = () =>
                        {
                            if (MainScreen?.Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
                            {
                                return anim.ID < 1_0000 ? $"cut{anim.ID:D4}" : $"[Entry {anim.ID}]";
                            }
                            else if (GameRoot.GameTypeHasLongAnimIDs)
                            {
                                bool ds2Meme = GameRoot.CurrentAnimIDFormatType == GameRoot.AnimIDFormattingType.aXX_YY_ZZZZ;
                                string res = ds2Meme ? $"a{(animID_Upper):D2}_{animID_Lower:D6}" : 
                                $"a{(animID_Upper):D3}_{animID_Lower:D6}";
                                if (ds2Meme)
                                {
                                    res = res.Insert(res.Length - 4, "_");
                                }
                                return res;
                            }
                            else
                            {
                                return $"a{(animID_Upper):D2}_{animID_Lower:D4}";
                            }
                          //  (GameDataManager.GameTypeHasLongAnimIDs
                          //? $"a{(animID_Upper):D3}_{animID_Lower:D6}" : $"a{(animID_Upper):D2}_{animID_Lower:D4}"),
                        },
                        Ref = anim,
                        VerticalOffset = taeSection.HeightOfAllAnims,
                        TaePrefix = MainScreen.FileContainer.AllTAEDict.Count == 1 ? 0 : animID_Upper,
                        IsLargeID = GameRoot.GameTypeHasLongAnimIDs,
                        IsRedErrorPrefix = isMultiSectionTae && ((anim.ID > (GameRoot.GameTypeHasLongAnimIDs ? 999999 : 9999) || anim.ID < 0))
                    };



                    taeSection.InfoMap.Add(anim, info);
                    taeSection.HeightOfAllAnims += AnimHeight;
                }
                int taeKey = taeSection.SectionName.StartsWith("a") ?
                        int.Parse(Utils.GetFileNameWithoutAnyExtensions(taeSection.SectionName).Substring(1)) : 0;
                AnimTaeSections.Add(taeKey, taeSection);
            }

            ScrollViewer = new TaeScrollViewer();

            UpdateScrollViewerRect();
        }

        public void Update(float elapsedSeconds, bool allowMouseUpdate)
        {
            if (!allowMouseUpdate)
                return;

            ScrollViewer.UpdateInput(MainScreen.Input, elapsedSeconds, true);

            if (Rect.Contains(MainScreen.Input.MousePositionPoint))
            {
               // MainScreen.Input.CursorType = MouseCursorType.Arrow;

                if (MainScreen.Input.LeftClickDown)
                {
                    var mouseCheckPoint = new Point(
                        (int)(MainScreen.Input.MousePosition.X - Rect.X + ScrollViewer.Scroll.X),
                        (int)(MainScreen.Input.MousePosition.Y - Rect.Y + ScrollViewer.Scroll.Y));

                    float offset = 0;
                    foreach (var taeSection in AnimTaeSections.Values)
                    {
                        var thisGroupRect = new Rectangle(0, (int)offset, ScrollViewer.Viewport.Width, AnimSectionHeaderHeight);

                        if (thisGroupRect.Contains(mouseCheckPoint))
                            taeSection.Collapsed = !taeSection.Collapsed;

                        offset += AnimSectionHeaderHeight; //Section Header

                        if (taeSection.Collapsed)
                            continue;

                        float groupStartOffset = offset;
                        foreach (var anim in taeSection.InfoMap)
                        {
                            var thisAnimRect = new Rectangle(0, (int)(groupStartOffset + anim.Value.VerticalOffset),
                                ScrollViewer.Viewport.Width, AnimHeight);
                            if (thisAnimRect.Contains(mouseCheckPoint))
                            {
                                if (MainScreen.Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
                                {
                                    MainScreen.REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME = true;
                                    MainScreen.REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE = taeSection.Tae;
                                    MainScreen.REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE_ANIM = anim.Value.Ref;
                                }
                                else
                                {
                                    MainScreen.Graph?.ViewportInteractor?.CancelCombo();
                                    RemoManager.CancelFullPlayback();
                                    MainScreen.SelectNewAnimRef(taeSection.Tae, anim.Value.Ref);
                                }
                            }
                            offset += AnimHeight;
                        }
                    }
                }
            }

            if (MainScreen.Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Insert))
            {
                MainScreen.DuplicateCurrentAnimation();
            }
        }

        public void UpdateMouseOutsideRect(float elapsedSeconds, bool allowMouseUpdate)
        {
            if (!allowMouseUpdate)
                return;

            ScrollViewer.UpdateInput(MainScreen.Input, elapsedSeconds, allowScrollWheel: false);

            if (MainScreen.Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Insert))
            {
                MainScreen.DuplicateCurrentAnimation();
            }
        }

        public void UpdateScrollViewerRect()
        {
            float EntireListHeight = 0;
            foreach (var section in AnimTaeSections.Values)
            {
                EntireListHeight += AnimSectionHeaderHeight;
                if (!section.Collapsed)
                    EntireListHeight += section.HeightOfAllAnims;
            }

            EntireListHeight += AnimHeight;

            ScrollViewer.SetDisplayRect(Rect, new Point(Rect.Width, (int)EntireListHeight));
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font, Texture2D scrollbarArrowTex)
        {
            if (MainScreen.Graph == null)
                return;

            UpdateScrollViewerRect();

            if (ScrollViewer.Scroll.Y != StartDrawScroll)
            {
                //UpdateScrollViewerRect();
                ClearStartDrawSkip();
            }
                

            ScrollViewer.Draw(gd, sb, boxTex, scrollbarArrowTex);

            var oldViewport = gd.Viewport;
            gd.Viewport = new Viewport(ScrollViewer.Viewport.DpiScaled());
            {
                sb.Begin(transformMatrix: ScrollViewer.GetScrollMatrix() * Main.DPIMatrix);
                try
                {
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

                    bool isUsingSavedStart = false;
                    bool hasFoundStart = false;
                    float offset = 0;

                    //if (StartDrawSectionOffset >= 0 && 
                    //    StartDrawSection >= 0 && StartDrawSection < AnimTaeSections.Count && 
                    //    StartDrawAnim >= 0 && StartDrawAnim < AnimTaeSections[StartDrawSection].InfoMap.Count &&
                    //    StartDrawScroll >= 0)
                    //{
                    //    // Commented out because it was buggy and I realized the performance wasn't 
                    //    // too bad without this skip drawing system in place
                    //    //isUsingSavedStart = true;
                    //    //offset = StartDrawSectionOffset;
                    //}
                    //else
                    //{
                    //    ClearStartDrawSkip();
                    //}

                    var taeSectionValueList = AnimTaeSections.Values.ToList();

                    for (int taeSectionIndex = (isUsingSavedStart ? StartDrawSection : 0); taeSectionIndex < AnimTaeSections.Count; taeSectionIndex++)
                    {
                        var sectionOffset = offset;
                        var taeSection = taeSectionValueList[taeSectionIndex];
                        var thisGroupRect = new Rectangle(0, (int)offset + 1, ScrollViewer.Viewport.Width, AnimSectionHeaderHeight - 2);
                        int border = BorderThickness;
                        sb.Draw(boxTex, thisGroupRect, Main.Colors.GuiColorAnimListAnimSectionHeaderRectOutline);
                        sb.Draw(boxTex,
                            new Rectangle(
                                thisGroupRect.X + border,
                                thisGroupRect.Y + border,
                                thisGroupRect.Width - border * 2,
                                thisGroupRect.Height - border * 2),
                            Main.Colors.GuiColorAnimListAnimSectionHeaderRectFill);


                        //sb.DrawString(font, $"[{taeSection.SectionName}]", new Vector2(22, (int)(offset) + (float)Math.Round((AnimSectionHeaderHeight / 2f) - (font.LineSpacing / 2f))) + Vector2.One
                        //     + Main.GlobalTaeEditorFontOffset, Main.Colors.GuiColorAnimListTextShadow);
                        //sb.DrawString(font, $"[{taeSection.SectionName}]", new Vector2(22, (int)(offset) + (float)Math.Round((AnimSectionHeaderHeight / 2f) - (font.LineSpacing / 2f))) + (Vector2.One * 2)
                        //     + Main.GlobalTaeEditorFontOffset, Main.Colors.GuiColorAnimListTextShadow);
                        //sb.DrawString(font, $"[{taeSection.SectionName}]", new Vector2(22, (int)(offset) + (float)Math.Round((AnimSectionHeaderHeight / 2f) - (font.LineSpacing / 2f)))
                        //     + Main.GlobalTaeEditorFontOffset, Main.Colors.GuiColorAnimListTextAnimSectionName);


                        ImGuiDebugDrawer.DrawText($"[{taeSection.SectionName}]", new Vector2(22, (int)(offset) + (float)Math.Round((AnimSectionHeaderHeight / 2f) -
                            (font.LineSpacing / 2f))) + new Vector2(0, -ScrollViewer.Scroll.Y), Main.Colors.GuiColorAnimListTextAnimSectionName, Main.Colors.GuiColorAnimListTextShadow, fontSize: 20);

                        Rectangle sectionCollapseButton = new Rectangle(
                                (int)(AnimSectionHeaderHeight / 4f),
                                (int)(offset + AnimSectionHeaderHeight / 4f),
                                (int)(AnimSectionHeaderHeight / 2f),
                                (int)(AnimSectionHeaderHeight / 2f));

                        sb.Draw(texture: boxTex,
                                position: new Vector2(sectionCollapseButton.X, sectionCollapseButton.Y),
                                sourceRectangle: null,
                                color: Main.Colors.GuiColorAnimListCollapsePlusMinusBackground,
                                rotation: 0,
                                origin: Vector2.Zero,
                                scale: new Vector2(sectionCollapseButton.Width, sectionCollapseButton.Height),
                                effects: SpriteEffects.None,
                                layerDepth: 0.01f
                                );

                        var collapseStr = taeSection.Collapsed ? "＋" : "－";

                        var collapseStrScale = font.MeasureString(collapseStr);

                        Vector2 collapseStrPoint =
                            new Vector2(AnimSectionHeaderHeight / 2, offset + AnimSectionHeaderHeight / 2 + 1)
                            - (collapseStrScale / 2);

                        collapseStrPoint = new Vector2((int)collapseStrPoint.X, (int)collapseStrPoint.Y);

                        sb.DrawString(font, collapseStr, collapseStrPoint + Main.GlobalTaeEditorFontOffset, Main.Colors.GuiColorAnimListCollapsePlusMinusForeground);

                        offset += AnimSectionHeaderHeight; //Section Header

                        if (taeSection.Collapsed)
                            continue;

                        float animsInSectionStartOffset = offset;

                        //if (isUsingSavedStart && taeSectionIndex == StartDrawSection)
                        //    offset += animsInSectionStartOffset;

                        for (int animIndex = 0; animIndex < taeSection.Tae.Animations.Count; animIndex++)
                        {
                            var anim = taeSection.InfoMap[taeSection.Tae.Animations[animIndex]];

                            if (offset + AnimHeight < ScrollViewer.Scroll.Y || offset > ScrollViewer.Scroll.Y + Rect.Height)
                            {
                                offset += AnimHeight;
                                if (offset > ScrollViewer.Scroll.Y + Rect.Height)
                                    break;
                                else
                                    continue;
                            }

                            if (!hasFoundStart)
                            {
                                StartDrawSection = taeSectionIndex;
                                StartDrawAnim = animIndex;
                                StartDrawSectionOffset = sectionOffset;
                                StartDrawAnimOffsetInSection = offset - animsInSectionStartOffset;
                                StartDrawScroll = ScrollViewer.Scroll.Y;
                                hasFoundStart = true;
                            }

                            string animIDText = "";

                            float animBlendWeight = 1;

                            if (MainScreen.IsCurrentlyLoadingGraph)
                            {
                                animIDText = "□ " + (anim.Ref.GetIsModified() ? $"{anim.GetName()}*" : anim.GetName());
                            }
                            else
                            {
                                var animFileName = MainScreen.Graph.ViewportInteractor.GetFinalAnimFileName(taeSection.Tae, anim.Ref);
                                animIDText = ((animFileName != null && MainScreen.Graph.ViewportInteractor.IsAnimLoaded(animFileName)) ? "■ " : "□ ") +
                                    (anim.Ref.GetIsModified() ? $"{anim.GetName()}*" : $"{anim.GetName()} ");
                                animBlendWeight = animFileName != null ? MainScreen.Graph.ViewportInteractor.GetAnimWeight(animFileName) : -1;
                            }

                            //var animIDTextSize = font.MeasureString(animIDText);
                            var animIDTextSize = ImGuiDebugDrawer.MeasureString(animIDText, 20);
                            if ((animIDTextSize.X + 16) > DevNameStartX)
                            {
                                DevNameStartX = animIDTextSize.X + 16;
                            }

                            string animNameText = (anim.Ref.AnimFileName ?? "<null>");

                            if (anim.Ref == MainScreen.SelectedTaeAnim)
                            {
                                var thisAnimRect = new Rectangle(
                                    GroupBraceMarginLeft / 2,
                                    (int)(animsInSectionStartOffset + anim.VerticalOffset) - 1,
                                    ScrollViewer.Viewport.Width - (GroupBraceMarginLeft),
                                    AnimHeight + 1);

                                sb.Draw(boxTex, thisAnimRect, Main.Colors.GuiColorAnimListHighlightRectOutline);

                                sb.Draw(boxTex,
                                    new Rectangle(
                                        thisAnimRect.X + border,
                                        thisAnimRect.Y + border,
                                        thisAnimRect.Width - border * 2,
                                        thisAnimRect.Height - border * 2),
                                    Main.Colors.GuiColorAnimListHighlightRectFill);
                            }

                            Color animNameColor = Color.White;

                            if (animBlendWeight >= 0)
                            {
                                animNameColor = Color.Lerp(Main.Colors.GuiColorAnimListTextAnimNameMinBlend, 
                                    Main.Colors.GuiColorAnimListTextAnimNameMaxBlend, MathHelper.Clamp(animBlendWeight, 0, 1));
                            }
                            else if (anim.Ref == MainScreen.SelectedTaeAnim)
                            {
                                animNameColor = Main.Colors.GuiColorAnimListTextAnimNameMaxBlend;
                            }

                            //sb.DrawString(font, animIDText, new Vector2(
                            //        GroupBraceMarginLeft + 4,
                            //        (int)(animsInSectionStartOffset + anim.VerticalOffset + (float)Math.Round((AnimHeight / 2f) - (font.LineSpacing / 2f)))) + (Vector2.One * 1.25f)
                            //         + Main.GlobalTaeEditorFontOffset, Main.Colors.GuiColorAnimListTextShadow);

                            ImGuiDebugDrawer.DrawText(animIDText, new Vector2(
                                    GroupBraceMarginLeft + 4,
                                    (int)(animsInSectionStartOffset + anim.VerticalOffset + (float)Math.Round((AnimHeight / 2f) - (font.LineSpacing / 2f)))) + (Vector2.One * 1.25f)
                                     + new Vector2(0, -ScrollViewer.Scroll.Y - 3), anim.IsRedErrorPrefix ? Color.Red : animNameColor, Main.Colors.GuiColorAnimListTextShadow, 20);

                            //sb.DrawString(font, animIDText, new Vector2(
                            //    GroupBraceMarginLeft + 4,
                            //    (int)(animsInSectionStartOffset + anim.VerticalOffset + (float)Math.Round((AnimHeight / 2f) - (font.LineSpacing / 2f))))
                            //     + Main.GlobalTaeEditorFontOffset, anim.IsRedErrorPrefix ? Color.Red : animNameColor);

                            //var animNameTextSize = font.MeasureString(animNameText);
                            var animNameTextSize = ImGuiDebugDrawer.MeasureString(animNameText, 20);

                            var devNamePos = new Vector2(
                                    DevNameStartX,
                                    (int)(animsInSectionStartOffset + anim.VerticalOffset + (float)Math.Round((AnimHeight / 2f) - (font.LineSpacing / 2f))));

                            //sb.DrawString(font, animNameText, devNamePos + (Vector2.One * 1.25f)
                            //         + Main.GlobalTaeEditorFontOffset, Main.Colors.GuiColorAnimListTextShadow);
                            //sb.DrawString(font, animNameText, devNamePos
                            //     + Main.GlobalTaeEditorFontOffset, Main.Colors.GuiColorAnimListTextAnimDevName);

                            ImGuiDebugDrawer.DrawText(animNameText, devNamePos + new Vector2(0, -ScrollViewer.Scroll.Y - 2), 
                                Main.Colors.GuiColorAnimListTextAnimDevName, Main.Colors.GuiColorAnimListTextShadow, 20);

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

                }
                catch
                {

                }
                finally { sb.End(); }
            }
            gd.Viewport = oldViewport;
        }
    }
}
