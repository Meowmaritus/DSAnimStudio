using Assimp;
using DSAnimStudio.TaeEditor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using NMatrix = System.Numerics.Matrix4x4;
using NVector2 = System.Numerics.Vector2;
using NVector3 = System.Numerics.Vector3;
using NVector4 = System.Numerics.Vector4;
using NQuaternion = System.Numerics.Quaternion;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Animations : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.NoSave;

            public bool IsInEditMode = true;

            public override string NewImguiWindowTitle => "Animations";

            public zzz_DocumentIns Document;

            public Animations(zzz_DocumentIns doc)
            {
                Document = doc;
            }



            public class StructForStuffStoredWhenChangingDocs
            {
                public float ScrollY;
            }

            private bool requestLoadStuffStoredWhenChangingDocs = false;

            public void SetRequestLoadStuffStoredWhenChangingDocs()
            {
                lock (_lock_WindowAnimations)
                {
                    requestLoadStuffStoredWhenChangingDocs = true;
                }
            }

            private StructForStuffStoredWhenChangingDocs StuffStoredWhenChangingDocs = new StructForStuffStoredWhenChangingDocs();

            private void StuffStoredWhenChangingDocs_Save_CallFromImguiContext()
            {
                StuffStoredWhenChangingDocs.ScrollY = ImGui.GetScrollY();
            }

            private void StuffStoredWhenChangingDocs_Load_CallFromImguiContext()
            {
                ImGui.SetScrollY(StuffStoredWhenChangingDocs.ScrollY);
            }


            protected override void Init()
            {
                //Title = "Animation List";
                // Flags = ImGuiWindowFlags.NoCollapse |
                //         ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar |
                //         ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoResize;
            }

            protected override void PreUpdate()
            {
                IsOpen = true;
                // Flags = ImGuiWindowFlags.NoCollapse |
                //         ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar |
                //         ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoResize;
                //ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
                //ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0, 0, 0, 0));
                Flags = ImGuiWindowFlags.NoMove;
            }

            protected override void PostUpdate()
            {
                //ImGui.PopStyleColor();
                //ImGui.PopStyleVar();
            }

            public struct TaeBlockCacheEntry
            {
                public bool IsValid;
                public float BlockHeight;
                public bool Debug_WasRenderedLastFrame;
            }

            

            private Dictionary<DSAProj.AnimCategory, TaeBlockCacheEntry> TaeBlockCaches = new Dictionary<DSAProj.AnimCategory, TaeBlockCacheEntry>();
            public void ClearTaeBlockCache()
            {
                lock (_lock_WindowAnimations)
                {
                    TaeBlockCaches.Clear();
                    CurrentTreeNodeSize = -1;
                    CurrentTreeNodeSize_FontScaleItsFor = -1;
                }
            }

            private object _lock_WindowAnimations = new object();

            public void InvalidateTaeBlockCache(DSAProj.AnimCategory tae)
            {
                lock (_lock_WindowAnimations)
                {
                    if (!TaeBlockCaches.ContainsKey(tae))
                        TaeBlockCaches.Add(tae, new TaeBlockCacheEntry());

                    var cache = TaeBlockCaches[tae];
                    cache.IsValid = false;
                    cache.BlockHeight = 0;
                    TaeBlockCaches[tae] = cache;
                }
            }

            private DSAProj.AnimCategory scrollToAnimNextFrame_TAE = null;
            private DSAProj.Animation scrollToAnimNextFrame = null;
            private bool scrollToAnimNextFrame_IsCenter = false;
            public void ScrollToAnimRef(DSAProj.AnimCategory tae, DSAProj.Animation anim, bool center_Deprecated)
            {
                lock (_lock_WindowAnimations)
                {
                    scrollToAnimNextFrame_TAE = tae;
                    scrollToAnimNextFrame = anim;
                    scrollToAnimNextFrame_IsCenter = center_Deprecated;
                }
            }


            private float CurrentTreeNodeSize = -1;
            private float CurrentTreeNodeSize_FontScaleItsFor = -1;
            
            

            //private bool collapseAllNextFrame = false;
            //private bool expandAllNextFrame = false;
            public void CollapseAll()
            {
                lock (_lock_WindowAnimations)
                {
                    var proj = MainTaeScreen?.FileContainer?.Proj;
                    if (proj != null)
                    {
                        proj.SafeAccessAnimCategoriesList(categories =>
                        {
                            foreach (var category in categories)
                            {
                                category.IsTreeNodeOpen = false;
                            }
                        });
                       
                    }
                }
            }
            public void ExpandAll()
            {
                lock (_lock_WindowAnimations)
                {
                    var proj = MainTaeScreen?.FileContainer?.Proj;
                    if (proj != null)
                    {
                        proj.SafeAccessAnimCategoriesList(categories =>
                        {
                            foreach (var category in categories)
                            {
                                category.IsTreeNodeOpen = true;
                            }
                        });
                        
                    }
                }
            }

            float debug_LastScrollCenterItem = -1;

            private float autoScrollDestination = 0;
            private float autoScrollTimerMax = 0.25f;
            private float autoScrollTimer = -1;
            private float targetScrollBufferDist => 200f * Main.DPI;
            private float scrollBufferDist = 0;
            private float cullBufferDist = 64f;

            private void DoAutoScroll(float posY, float scrollDelta)
            {
                lock (_lock_WindowAnimations)
                {
                    //test
                    //ImGui.SetScrollY(ImGui.GetScrollY() + scrollDelta);
                    
                    float speedupRatio = MathHelper.Clamp((MathF.Abs(scrollDelta) - (scrollBufferDist / 2)) / scrollBufferDist, 0, 1);
                    
                    
                    autoScrollTimer = autoScrollTimerMax * (speedupRatio);
                    autoScrollDestination = posY;
                    if (autoScrollDestination < 0)
                        autoScrollDestination = 0;
                }
            }

            private static void DrawErrorIcon(Vector2 pos, float itemHeight)
            {
                var r = itemHeight / 2;
                r = MathF.Round(r);
                pos += new Vector2(0, 0);

                //pos = pos.RoundInt();

                r += 0;

                var erOutlineColor = Color.Black;
                
                var erFillColor = Main.PulseLerpColor(new Color(0x90, 0, 0, 0xFF), new Color(0xD0, 0, 0, 0xFF), 2);
                var erOuterOutlineColor = Main.PulseLerpColor(new Color(200, 200, 200, 255), Color.White, 2);
                var erCrossColor = erOuterOutlineColor;

                ImGuiDebugDrawer.DrawCircle(pos + new Vector2(0, 0), r, erFillColor, thickness: 0, dpiScale: false);

                const float oneOverSqrt2 = 0.70710678118654752440f;

                float crossRadius = r - 1;

                Vector2 a = pos + (new Vector2(-1, -1) * oneOverSqrt2 * crossRadius);
                Vector2 b = pos + (new Vector2(1, 1) * oneOverSqrt2 * crossRadius);
                Vector2 c = pos + (new Vector2(1, -1) * oneOverSqrt2 * crossRadius);
                Vector2 d = pos + (new Vector2(-1, 1) * oneOverSqrt2 * crossRadius);

                a = (a + new Vector2(-0.5f, -0.5f));
                b = (b + new Vector2(-0.5f, -0.5f));
                c = (c + new Vector2(-0.5f, -0.5f));
                d = (d + new Vector2(-0.5f, -0.5f));

                ImGuiDebugDrawer.DrawLine(a, b, erCrossColor, thickness: 1.5f, dpiScale: false);
                ImGuiDebugDrawer.DrawLine(c, d, erCrossColor, thickness: 1.5f, dpiScale: false);

                ImGuiDebugDrawer.DrawCircle(pos, r, erOutlineColor, thickness: 3, dpiScale: false);
                ImGuiDebugDrawer.DrawCircle(pos, r + 2, erOuterOutlineColor, thickness: 1, dpiScale: false);
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                if (MainDoc != Document)
                    return;

                lock (_lock_WindowAnimations)
                {
                    try
                    {
                        if (requestLoadStuffStoredWhenChangingDocs)
                        {
                            StuffStoredWhenChangingDocs_Load_CallFromImguiContext();
                            requestLoadStuffStoredWhenChangingDocs = false;
                        }
                        // if (MainTaeScreen.ImGuiNewAnimListSize.X == 0 || MainTaeScreen.ImGuiNewAnimListSize.Y == 0)
                        //     return;

                        Action endOfDrawAction = () => { };

                        float entireWindowScale = 1f; // 0.75f;

                        ImGui.SetWindowFontScale(entireWindowScale);

                        // ImGui.SetWindowPos((MainTaeScreen.ImGuiNewAnimListPos + new System.Numerics.Vector2(0, 0)) * Main.DPIVectorN);
                        // ImGui.SetWindowSize((MainTaeScreen.ImGuiNewAnimListSize - new System.Numerics.Vector2(0, 0)) * Main.DPIVectorN);

                        var drawList = ImGui.GetWindowDrawList();

                        //if (MainTaeScreen.SelectedTae != null)
                        //    MainTaeScreen.SelectedTae.IsTreeNodeOpen = true;

                        if (scrollToAnimNextFrame_TAE != null && scrollToAnimNextFrame != null)
                        {
                            scrollToAnimNextFrame_TAE.IsTreeNodeOpen = true;
                        }

                        var isDuplicateTaeSectionEnabled =
                            true; //MainTaeScreen.Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.PC;


                        DSAProj.AnimCategory jumptoTae = null;
                        DSAProj.Animation jumpToAnimation = null;
                        bool cancelJumpThisFrameDueToExtraButtonBeingClicked = false;
                        TaeEditorScreen.InsertAnimType insertNewAnimationAtIndex_IndexType =
                            TaeEditorScreen.InsertAnimType.None;
                        DSAProj.Animation insertNewAnimationAtIndex_FromAnim = null;
                        DSAProj.AnimCategory insertNewAnimationAtIndex_FromTae = null;



                        //ImGui.GetCursorPos();

                        var scrollX = ImGui.GetScrollX();
                        var scrollY = ImGui.GetScrollY();
                        var scroll = new NVector2(scrollX, scrollY);
                        var windowSize = ImGui.GetWindowSize();
                        var windowPos = ImGui.GetWindowPos();
                        float topOfScreen = scrollY;
                        float bottomOfScreen = scrollY + windowSize.Y;

                        var mouseCursorPos = ImGui.GetMousePos();
                        var mousePosLocal = mouseCursorPos + scroll - windowPos;

                        float scrollbarWidth = 21 * Main.DPI;

                        float currentFontSize = ImGui.GetFontSize();

                        scrollBufferDist = targetScrollBufferDist;
                        if (windowSize.Y <= (targetScrollBufferDist * 2f))
                        {
                            scrollBufferDist = windowSize.Y / 2;
                        }

                        float topOfScreen_CullBuffer = topOfScreen - cullBufferDist;
                        float bottomOfScreen_CullBuffer = bottomOfScreen + cullBufferDist;

                        float centerOfScreen = topOfScreen + (windowSize.Y / 2f);
                        float topOfScreenWithBuffer = topOfScreen + scrollBufferDist;
                        float bottomOfScreenWithBuffer = bottomOfScreen - scrollBufferDist;


                        float treeNodeHeight = (6 + (17 * Main.DPI)) * entireWindowScale;

                        if (CurrentTreeNodeSize > 0 && CurrentTreeNodeSize_FontScaleItsFor > 0 &&
                            Math.Abs(currentFontSize - CurrentTreeNodeSize_FontScaleItsFor) < 0.001f)
                        {
                            treeNodeHeight = CurrentTreeNodeSize;
                        }
                        else
                        {
                            CurrentTreeNodeSize = -1;
                            CurrentTreeNodeSize_FontScaleItsFor = -1;
                        }


                        void RenderTaeBlock(DSAProj proj, DSAProj.AnimCategory category, bool disableTaeCulling,
                            bool disableAnimCulling)
                        {
                            // Culling is possible if tree node is closed (since it's always the same size)
                            // or if culling is enabled and a valid cache exists
                            bool blockHasValidCache =
                                (TaeBlockCaches.ContainsKey(category) && TaeBlockCaches[category].IsValid);
                            bool taeCullingPossible =
                                !category.IsTreeNodeOpen || (!disableTaeCulling && blockHasValidCache);

                            var treeCursorPos = ImGui.GetCursorPos();

                            var approximateBottomOfTreeAndChildren = treeCursorPos.Y + treeNodeHeight;

                            if (category.IsTreeNodeOpen && blockHasValidCache)
                            {
                                approximateBottomOfTreeAndChildren =
                                    treeCursorPos.Y + TaeBlockCaches[category].BlockHeight;
                            }


                            bool isBlockInView = (treeCursorPos.Y <= topOfScreen_CullBuffer &&
                                                  approximateBottomOfTreeAndChildren > topOfScreen_CullBuffer)
                                                 || (treeCursorPos.Y <= bottomOfScreen_CullBuffer &&
                                                     approximateBottomOfTreeAndChildren > bottomOfScreen_CullBuffer)
                                                 || (treeCursorPos.Y >= topOfScreen_CullBuffer &&
                                                     approximateBottomOfTreeAndChildren <= bottomOfScreen_CullBuffer);

                            if (scrollToAnimNextFrame_TAE == category)
                                isBlockInView = true;

                            if (disableTaeCulling)
                                isBlockInView = true;

                            // If TAE block is offscreen and culling is possible, cull the block.
                            if (!isBlockInView && taeCullingPossible)
                            {
                                ImGui.SetCursorPos(new NVector2(treeCursorPos.X, approximateBottomOfTreeAndChildren));
                                return;
                            }

                            var topCursorPos_TAE = ImGui.GetCursorPosY();

                            ImGui.PushStyleColor(ImGuiCol.Header,
                                Main.Config.Colors.GuiColorAnimListAnimSectionHeaderRectFill.ToNVector4());
                            ImGui.PushStyleColor(ImGuiCol.Text,
                                Main.Config.Colors.GuiColorAnimListTextAnimSectionName.ToNVector4());
                            ImGui.PushStyleColor(ImGuiCol.Border,
                                Main.Config.Colors.GuiColorAnimListAnimSectionHeaderRectOutline.ToNVector4());
                            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);

                            var categoryColor = category.Info.CustomColor;

                            ImGui.SetNextItemOpen(category.IsTreeNodeOpen);

                            if (categoryColor.HasValue)
                            {
                                ImGui.PushStyleColor(ImGuiCol.Header, categoryColor.Value.PackedValue);
                                ImGui.PushStyleColor(ImGuiCol.TabHovered, categoryColor.Value.PackedValue);
                                ImGui.PushStyleColor(ImGuiCol.Tab, categoryColor.Value.PackedValue);
                                ImGui.PushStyleColor(ImGuiCol.TabUnfocused, categoryColor.Value.PackedValue);
                                ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, categoryColor.Value.PackedValue);
                            }

                            //Color defaultAnimationColor = adsfasdfasdf

                            var tree = ImGui.TreeNodeEx(
                                $"[{category.Info.DisplayName}]###NewTaeList_TaeTreeNode_{category.CategoryID}_IDStr",
                                ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.OpenOnArrow |
                                ImGuiTreeNodeFlags.Framed, $"###NewTaeList_TaeTreeNode_{category.CategoryID}_DispText");

                            if (categoryColor.HasValue)
                            {
                                ImGui.PopStyleColor(5);
                            }

                            var treeOpenToggled = ImGui.IsItemToggledOpen();



                            if (isBlockInView)
                            {
                                var bottomCursorPos_TAE = ImGui.GetCursorPosY();

                                var mouseHoveringOverAnim_TAE = (mouseCursorPos.Y >= windowPos.Y &&
                                                                 mouseCursorPos.Y <= windowPos.Y + windowSize.Y
                                                                 && mouseCursorPos.X >= windowPos.X &&
                                                                 mouseCursorPos.X <= windowPos.X + windowSize.X -
                                                                 scrollbarWidth) &&
                                                                (mousePosLocal.Y >= topCursorPos_TAE &&
                                                                 mousePosLocal.Y < bottomCursorPos_TAE);






                                var textPos = ImGui.GetWindowPos() + treeCursorPos +
                                              new System.Numerics.Vector2(24 * Main.DPI, -2.5f * entireWindowScale) -
                                              new NVector2(0, ImGui.GetScrollY());
                                textPos = new NVector2(MathF.Round(textPos.X), MathF.Round(textPos.Y));

                                
                                //if (categoryColor.HasValue)
                                //{
                                //    ImGuiDebugDrawer.DrawRect(textPos + new NVector2(0 * Main.DPI, 2 + (2 * Main.DPI)),
                                //        new NVector2(windowSize.X - scrollbarWidth - (0 * Main.DPI) - textPos.X, 17 * Main.DPI),
                                //        categoryColor.Value, dpiScale: false);
                                //}

                                

                                if (MainTaeScreen.HighlightOpacityDictCopy.ContainsKey(category))
                                {
                                    float highlightOpacity = MainTaeScreen.HighlightOpacityDictCopy[category];
                                    ImGuiDebugDrawer.DrawRect(textPos + new NVector2(0 * Main.DPI, 2 + (2 * Main.DPI)),
                                        new NVector2(windowSize.X - scrollbarWidth - (0 * Main.DPI) - textPos.X, 17 * Main.DPI),
                                        Color.Yellow.MultiplyAlpha(highlightOpacity * 0.75f), dpiScale: false);
                                }

                                var finalTaeNameString = $"[{category.CategoryID}] {category.Info.DisplayName}";

                                if (category.SAFE_GetIsModified())
                                {
                                    finalTaeNameString = $"[{category.CategoryID}]* {category.Info.DisplayName}";
                                }

                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI, textPos + new NVector2(-1, -1),
                                    Main.Config.Colors.GuiColorAnimListTextShadow.PackedValue, finalTaeNameString);

                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI, textPos + new NVector2(-1, 1),
                                    Main.Config.Colors.GuiColorAnimListTextShadow.PackedValue, finalTaeNameString);

                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI, textPos + new NVector2(1, -1),
                                    Main.Config.Colors.GuiColorAnimListTextShadow.PackedValue, finalTaeNameString);

                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI, textPos + new NVector2(1, 1),
                                    Main.Config.Colors.GuiColorAnimListTextShadow.PackedValue, finalTaeNameString);

                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI, textPos,
                                    Main.Config.Colors.GuiColorAnimListTextAnimSectionName.PackedValue,
                                    finalTaeNameString);

                                if (!MenuBar.IsAnyMenuOpen && !DialogManager.AnyDialogsShowing &&
                                    category.ErrorContainer != null && category.ErrorContainer.AnyErrors())
                                {
                                    category.ErrorContainer.UpdateAndBuildImGui(mouseHoveringOverAnim_TAE,
                                        MainTaeScreen.Input);
                                    //ImGuiDebugDrawer.DrawRect(textPos + new NVector2(-8, 0), new NVector2(windowSize.X - scrollbarWidth - 50, 22), new Color(0.75f, 0, 0f, 1), dpiScale: false);


                                    var erPos = textPos + new NVector2(0 * Main.DPI, 2 + (2 * Main.DPI));
                                    var erSize = new NVector2(windowSize.X - scrollbarWidth - (-2 * Main.DPI) - textPos.X, 17 * Main.DPI);
                                    //ImGuiDebugDrawer.DrawRect(erPos,
                                    //    erSize,
                                    //    new Color(1, 0, 0f, 0.65f), dpiScale: false);

                                    var newErPos = erPos + erSize + new Vector2(-erSize.Y / 2, -erSize.Y / 2);
                                    DrawErrorIcon(newErPos, erSize.Y);


                                }


                            }




                            ImGui.PopStyleVar();
                            ImGui.PopStyleColor();
                            ImGui.PopStyleColor();
                            ImGui.PopStyleColor();
                            if (tree)
                            {
                                var cache = TaeBlockCaches[category];
                                cache.Debug_WasRenderedLastFrame = true;
                                TaeBlockCaches[category] = cache;

                                ImGui.SetCursorPosX(8);

                                //another test
                                bool anyTagFieldFocused = false;
                                category.Info.ShowImGui(ref anyTagFieldFocused, proj,
                                    GetRect(true, true).DpiScaled(), 
                                    GetRect(false, false).DpiScaled(), isModified =>
                                {
                                    if (isModified)
                                        proj.SAFE_MarkAllModified();
                                }, showTagsOnly: true, out _);

                                ImGui.SetCursorPosX(8);
                                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 12);
                                //ImGui.SameLine();
                                //test
                                if (Tools.SimpleClickButton("Properties..."))
                                {
                                    MainTaeScreen.ShowDialogEditAnimCategoryProperties(category);
                                }

                                if (isDuplicateTaeSectionEnabled)
                                {
                                    ImGui.SameLine();
                                    if (Tools.SimpleClickButton("Duplicate..."))
                                    {
                                        MainTaeScreen.ShowDialogDuplicateToNewTaeSection(proj, category);
                                    }
                                }





                                float startOfThisCategorysAnims = ImGui.GetCursorPosY();

                                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 8);

                                int animIndex = 0;
                                var animsList = category.SAFE_GetAnimations();
                                foreach (var anim in animsList)
                                {
                                    if (Main.Debug.EnableAnimListDebug)
                                    {
                                        float lineX = windowPos.X;
                                        float lineY = (startOfThisCategorysAnims)
                                            + (treeNodeHeight * animIndex) - ImGui.GetScrollY() + (windowPos.Y);

                                        lineY -= (2 * Main.DPI);

                                        ImGuiDebugDrawer.DrawLine(new Vector2(lineX, lineY),
                                            new Vector2(lineX + windowSize.X / 2, lineY),
                                            Color.Turquoise, dpiScale: false);


                                    }


                                    var animNameCursorPos = ImGui.GetCursorPos();
                                    float approximateCenterOfItem = animNameCursorPos.Y + (8 * entireWindowScale);
                                    bool isAnimInView = (approximateCenterOfItem >= topOfScreen_CullBuffer &&
                                                         approximateCenterOfItem <= bottomOfScreen_CullBuffer);

                                    if (scrollToAnimNextFrame == anim)
                                        isAnimInView = true;

                                    if (disableAnimCulling)
                                        isAnimInView = true;

                                    if (!isAnimInView)
                                    {
                                        ImGui.SetCursorPos(new NVector2(treeCursorPos.X,
                                            animNameCursorPos.Y + treeNodeHeight));
                                        continue;
                                    }

                                    bool selected = Document.EditorScreen.SelectedAnim == anim;
                                    bool newSelected_Left = false;
                                    bool newSelected_Right = selected;




                                    var animFileName = anim.GetHkxID(proj);
                                    string prefix =
                                        ((animFileName != null &&
                                          (MainTaeScreen?.Graph?.ViewportInteractor?.IsAnimLoaded(animFileName) ??
                                           false))
                                            ? "■ "
                                            : "□ ");

                                    //ImGui.LabelText($"[{animIDStr}]###NewTaeList_Tae{tae.TaeBindIndex}_Anim[{animIndex}]", anim.AnimFileName);

                                    //MainTaeScreen?.Graph?.ViewportInteractor?.CurrentModel?.AnimContainer?.getwe

                                    var textColor = selected
                                        ? Main.Config.Colors.GuiColorAnimListTextAnimNameSelected
                                        : Main.Config.Colors.GuiColorAnimListTextAnimName;

                                    var textShadowColor = Main.Config.Colors.GuiColorAnimListTextShadow;

                                    var animBackgroundNormal = Main.Config.Colors.MainColorBackground;
                                    var animBackgroundHighlighted = Main.Config.Colors.GuiColorAnimListHighlightRectFill;

                                    if (categoryColor.HasValue)
                                    {
                                        animBackgroundNormal = categoryColor.Value.MultBrightness(0.5f);
                                        animBackgroundHighlighted = categoryColor.Value.MultBrightness(1f);
                                    }

                                    if (anim.Info.CustomColor.HasValue)
                                    {
                                        animBackgroundNormal = anim.Info.CustomColor.Value.MultBrightness(0.5f);
                                        animBackgroundHighlighted = anim.Info.CustomColor.Value.MultBrightness(1f);
                                    }

                                    ImGui.PushStyleColor(ImGuiCol.Text, textColor.ToNVector4());
                                    ImGui.PushStyleColor(ImGuiCol.HeaderActive,
                                        animBackgroundHighlighted.ToNVector4());
                                    ImGui.PushStyleColor(ImGuiCol.Header,
                                        animBackgroundNormal.ToNVector4());
                                    {


                                        var animNameTextPos = ImGui.GetWindowPos() + animNameCursorPos +
                                                              new System.Numerics.Vector2(-18, -2 * entireWindowScale) -
                                                              new NVector2(0, ImGui.GetScrollY());
                                        animNameTextPos = new NVector2(MathF.Round(animNameTextPos.X),
                                            MathF.Round(animNameTextPos.Y));


                                        if (selected)
                                        {
                                            ImGui.PushStyleColor(ImGuiCol.Border,
                                                Main.Config.Colors.GuiColorAnimListHighlightRectOutline.ToNVector4());
                                            ImGui.PushStyleColor(ImGuiCol.Header,
                                                animBackgroundHighlighted.ToNVector4());
                                            ImGui.PushStyleColor(ImGuiCol.HeaderActive,
                                                animBackgroundHighlighted.ToNVector4());
                                            ImGui.PushStyleColor(ImGuiCol.HeaderHovered,
                                                animBackgroundHighlighted.ToNVector4());
                                            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
                                        }
                                        else
                                        {
                                            ImGui.PushStyleColor(ImGuiCol.Header, animBackgroundNormal.ToNVector4());
                                            ImGui.PushStyleColor(ImGuiCol.HeaderActive, animBackgroundNormal.ToNVector4());
                                            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, animBackgroundNormal.ToNVector4());

                                        }
                                        //ImGui.Selectable($"###NewTaeList_Tae{tae.TaeBindIndex}_Anim[{animIndex}]", ref newSelected_Left);
                                        var topCursorPos = ImGui.GetCursorPosY() + 2; //animNameCursorPos.Y;
                                        //ImGui.MenuItem($"###NewTaeList_Tae{tae.TaeBindIndex}_Anim[{animIndex}]", "shortcut", ref newSelected_Left);
                                        ImGui.TreeNodeEx($"###NewTaeList_Tae{category.CategoryID}_Anim[{animIndex}]",
                                            ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen |
                                            ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding |
                                            ImGuiTreeNodeFlags.Framed, $" ");

                                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 4);

                                        if (ImGui.IsItemClicked())
                                            newSelected_Left = true;

                                        if (selected)
                                        {
                                            ImGui.PopStyleVar();
                                            ImGui.PopStyleColor();
                                            ImGui.PopStyleColor();
                                            ImGui.PopStyleColor();
                                            ImGui.PopStyleColor();
                                        }
                                        else
                                        {

                                            ImGui.PopStyleColor();
                                            ImGui.PopStyleColor();
                                            ImGui.PopStyleColor();
                                        }

                                        if (isAnimInView)
                                        {
                                            string finalAnimIDStr = $"{prefix}[{anim.SplitID.GetFormattedIDString(proj)}]";

                                            //if (anim.Info.DisplayName != null)
                                            //{
                                            //    finalAnimIDStr = $"{prefix}[{anim.NewID} {anim.Info.DisplayName}]";
                                            //}

                                            
                                            var bottomCursorPos = ImGui.GetCursorPosY() + 2;

                                            topCursorPos -= 2.5f;
                                            bottomCursorPos -= 2.5f;
                                            if (anim.SAFE_GetIsModified())
                                                finalAnimIDStr += "*";
                                            var mouseHoveringOverAnim = (mouseCursorPos.Y >= windowPos.Y &&
                                                                         mouseCursorPos.Y <= windowPos.Y + windowSize.Y
                                                                         && mouseCursorPos.X >= windowPos.X &&
                                                                         mouseCursorPos.X <=
                                                                         windowPos.X + windowSize.X - scrollbarWidth) &&
                                                                        (mousePosLocal.Y >= topCursorPos &&
                                                                         mousePosLocal.Y < bottomCursorPos);


                                            var animColor = anim.Info.CustomColor;
                                            if (animColor.HasValue)
                                            {
                                                ImGuiDebugDrawer.DrawRect(
                                                    animNameTextPos + new NVector2(18 * Main.DPI, 1 + (2 * Main.DPI)),
                                                    new NVector2(windowSize.X - scrollbarWidth - (18 * Main.DPI) - animNameTextPos.X, 17 * Main.DPI),
                                                    animColor.Value, dpiScale: false);
                                            }

                                            var animFileDispNameColor = Main.Config.Colors.GuiColorAnimListTextAnimFileName;
                                            var animFileDispNameShadowColor = Main.Config.Colors.GuiColorAnimListTextShadow;

                                            var origAnimNameTextPos = animNameTextPos;


                                            if (MainTaeScreen.HighlightOpacityDictCopy.ContainsKey(anim))
                                            {
                                                float highlightOpacity = MainTaeScreen.HighlightOpacityDictCopy[anim];
                                                ImGuiDebugDrawer.DrawRect(
                                                    animNameTextPos + new NVector2(18 * Main.DPI, 1 + (2 * Main.DPI)),
                                                    new NVector2(windowSize.X - scrollbarWidth - (18 * Main.DPI) - animNameTextPos.X, 17 * Main.DPI),
                                                    Color.Yellow.MultiplyAlpha(highlightOpacity * 0.75f),
                                                    dpiScale: false);
                                            }

                                            if (anim.IS_DUMMY_ANIM)
                                            {
                                                finalAnimIDStr = "  <Empty Category>";
                                                //textColor = animFileDispNameColor;
                                                //textShadowColor = animFileDispNameShadowColor;
                                            }

                                            if (selected)
                                            {
                                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                    animNameTextPos + new NVector2(-1, -1),
                                                    textShadowColor.PackedValue,
                                                    finalAnimIDStr);

                                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                    animNameTextPos + new NVector2(-1, 1),
                                                    textShadowColor.PackedValue,
                                                    finalAnimIDStr);

                                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                    animNameTextPos + new NVector2(1, -1),
                                                    textShadowColor.PackedValue,
                                                    finalAnimIDStr);

                                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                    animNameTextPos + new NVector2(1, 1),
                                                    textShadowColor.PackedValue,
                                                    finalAnimIDStr);
                                            }
                                            else
                                            {
                                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                    animNameTextPos + new NVector2(-1, -1),
                                                    textShadowColor.PackedValue,
                                                    finalAnimIDStr);

                                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                    animNameTextPos + new NVector2(-1, 1),
                                                    textShadowColor.PackedValue,
                                                    finalAnimIDStr);

                                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                    animNameTextPos + new NVector2(1, -1),
                                                    textShadowColor.PackedValue,
                                                    finalAnimIDStr);

                                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                    animNameTextPos + new NVector2(1, 1),
                                                    textShadowColor.PackedValue,
                                                    finalAnimIDStr);
                                            }

                                            drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI, animNameTextPos,
                                                textColor.PackedValue, finalAnimIDStr);

                                            animNameTextPos.X +=
                                                ((Document.GameRoot.GameTypeHasLongAnimIDs ? 128 : 108) * Main.DPI);

                                            var animFileDispNameThing = anim.Info.DisplayName;
                                            string animFileDispNameThing2 = anim.SAFE_GetHeaderClone().AnimFileName;

                                            //if (anim.Info.DisplayName != null && mouseHoveringOverAnim && anim.Header.AnimFileName != null)
                                            //{
                                            //    animFileDispNameThing = $"{anim.Info.DisplayName} <{anim.Header.AnimFileName}>";
                                            //}

                                            

                                            

                                            if (!string.IsNullOrWhiteSpace(animFileDispNameThing))
                                            {

                                                if (selected)
                                                {
                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(-1, -1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing);

                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(-1, 1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing);

                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(1, -1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing);

                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(1, 1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing);
                                                }
                                                else
                                                {
                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(-1, -1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing);

                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(-1, 1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing);

                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(1, -1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing);

                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(1, 1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing);

                                                }

                                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI, animNameTextPos,
                                                animFileDispNameColor.PackedValue,
                                                animFileDispNameThing);

                                                var animFileDispNameSize = ImGui.CalcTextSize(animFileDispNameThing) * Main.DPI;
                                                animNameTextPos.X += animFileDispNameSize.X + 12;
                                            }


                                            if (!anim.IS_DUMMY_ANIM && !string.IsNullOrWhiteSpace(animFileDispNameThing2) && ((Main.Config.AnimListShowHkxNames || mouseHoveringOverAnim) 
                                                || string.IsNullOrWhiteSpace(animFileDispNameThing)))
                                            {
                                                if (!string.IsNullOrWhiteSpace(animFileDispNameThing))
                                                    animFileDispNameThing2 = $"<{animFileDispNameThing2}>";

                                                animFileDispNameColor = animFileDispNameColor.MultBrightness(0.75f);
                                                animFileDispNameShadowColor = animFileDispNameShadowColor.MultBrightness(0.75f);

                                                if (selected)
                                                {
                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(-1, -1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing2);

                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(-1, 1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing2);

                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(1, -1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing2);

                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(1, 1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing2);
                                                }
                                                else
                                                {
                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(-1, -1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing2);

                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(-1, 1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing2);

                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(1, -1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing2);

                                                    drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                                        animNameTextPos + new NVector2(1, 1),
                                                        animFileDispNameShadowColor.PackedValue,
                                                        animFileDispNameThing2);

                                                }

                                                drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI, animNameTextPos,
                                                animFileDispNameColor.PackedValue,
                                                animFileDispNameThing2);

                                                //var animFileDispNameSize = ImGui.CalcTextSize(animFileDispNameThing2) * Main.DPI;
                                                //animNameTextPos.X += animFileDispNameSize.X;
                                            }



                                            if (!anim.IS_DUMMY_ANIM && anim.ErrorContainer != null && anim.ErrorContainer.AnyErrors())
                                            {
                                                if (!MenuBar.IsAnyMenuOpen && !DialogManager.AnyDialogsShowing)
                                                    anim.ErrorContainer.UpdateAndBuildImGui(mouseHoveringOverAnim,
                                                        MainTaeScreen.Input);
                                                // animNameTextPos = ImGui.GetWindowPos() + animNameCursorPos + new System.Numerics.Vector2(-20, 2 * entireWindowScale) - new NVector2(0, ImGui.GetScrollY());
                                                //animNameTextPos = new NVector2(MathF.Round(animNameTextPos.X), MathF.Round(animNameTextPos.Y));

                                                Vector2 erPos = origAnimNameTextPos + new NVector2(18 * Main.DPI, 1 + (2 * Main.DPI));
                                                Vector2 erSize = new NVector2(windowSize.X - scrollbarWidth - (16 * Main.DPI) - origAnimNameTextPos.X, 17 * Main.DPI);

                                                //ImGuiDebugDrawer.DrawRect(
                                                //    erPos,
                                                //    erSize,
                                                //    new Color(1, 0, 0f, 0.65f), dpiScale: false);

                                                var newErPos = erPos + erSize + new Vector2(-erSize.Y / 2, -erSize.Y / 2);
                                                DrawErrorIcon(newErPos, erSize.Y);
                                            }




                                            var actualItemHeight = (bottomCursorPos - topCursorPos);

                                            var actualItemCenterY = topCursorPos + (actualItemHeight / 2);
                                            var hitboxAboveStart = actualItemCenterY - actualItemHeight;
                                            var hitboxAboveEnd = actualItemCenterY;
                                            var hitboxBelowStart = actualItemCenterY;
                                            var hitboxBelowEnd = actualItemCenterY + actualItemHeight;





                                            //mouseCursorPos /= Main.DPIVector.ToNumerics();



                                            if (!MenuBar.IsAnyMenuOpen && !DialogManager.AnyDialogsShowing &&
                                                mouseCursorPos.Y >= windowPos.Y &&
                                                mouseCursorPos.Y <= windowPos.Y + windowSize.Y
                                                && mouseCursorPos.X >= windowPos.X && mouseCursorPos.X <=
                                                windowPos.X + windowSize.X - scrollbarWidth)
                                            {




                                                // var mouseHoveringOverAnim = mousePosLocal.Y >= topCursorPos &&
                                                //                             mousePosLocal.Y < bottomCursorPos;
                                                //
                                                // if (anim.ErrorContainer != null && anim.ErrorContainer.AnyErrors())
                                                // {
                                                //     anim.ErrorContainer.UpdateAndBuildImGui(mouseHoveringOverAnim, MainTaeScreen.Input);
                                                // }
                                                //


                                                Vector2 buttonAboveSize = new Vector2(10, 10);
                                                var buttonAbovePos = new Vector2(
                                                    windowSize.X - scrollbarWidth - buttonAboveSize.X,
                                                    topCursorPos - buttonAboveSize.Y / 2);

                                                var buttonBelowSize = new Vector2(10, 10);
                                                var buttonBelowPos = new Vector2(
                                                    windowSize.X - scrollbarWidth - buttonBelowSize.X,
                                                    bottomCursorPos - buttonBelowSize.Y / 2);

                                                // buttonAboveSize /= Main.DPIVector;
                                                // buttonAbovePos /= Main.DPIVector;
                                                // buttonBelowPos /= Main.DPIVector;
                                                // buttonBelowSize /= Main.DPIVector;

                                                bool drawButtonAbove = //animIndex == 0 &&
                                                                       mousePosLocal.Y >= buttonAbovePos.Y &&
                                                                       mousePosLocal.Y < (buttonAbovePos.Y +
                                                                           buttonAboveSize.Y);
                                                bool drawButtonBelow = //animIndex < (category.SAFE_GetAnimCount() - 1) &&
                                                                       mousePosLocal.Y >= buttonBelowPos.Y &&
                                                                       mousePosLocal.Y < (buttonBelowPos.Y +
                                                                           buttonAboveSize.Y);

                                                bool acceptInputForButtonAbove = drawButtonAbove && animIndex == 0;
                                                bool acceptInputForButtonBelow = drawButtonBelow;

                                                //test
                                                //drawButtonAbove = true;

                                                Color imguiTextColor = Color.White;
                                                //unsafe
                                                //{
                                                //    var txtCol = ImGui.GetStyleColorVec4(ImGuiCol.Text);
                                                //    var txtColVal = *txtCol;
                                                //    imguiTextColor = new Color(txtColVal.X, txtColVal.Y, txtColVal.Z, txtColVal.W);
                                                //}

                                                if (autoScrollTimer >= 0)
                                                {
                                                    drawButtonAbove = false;
                                                    drawButtonBelow = false;
                                                }

                                                if (drawButtonAbove)
                                                {


                                                    var lineA = new Vector2(4,
                                                        buttonAbovePos.Y + buttonAboveSize.Y / 2);
                                                    var lineB = new Vector2(
                                                        windowSize.X - scrollbarWidth - buttonAboveSize.X - 4,
                                                        buttonAbovePos.Y + (buttonAboveSize.Y / 2));
                                                    ImGuiDebugDrawer.DrawLine(
                                                        (lineA - scroll + windowPos) / Main.DPIVector,
                                                        (lineB - scroll + windowPos) / Main.DPIVector, Color.Yellow, thickness: 3);

                                                    ImGui.PushStyleColor(ImGuiCol.Button,
                                                        new NVector4(0.4f, 0.4f, 0, 1));
                                                    ImGui.PushStyleColor(ImGuiCol.ButtonActive,
                                                        new NVector4(0.5f, 0.5f, 0, 1));
                                                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered,
                                                        new NVector4(0.6f, 0.6f, 0, 1));

                                                    bool buttonClicked =
                                                            (ImGuiDebugDrawer.FakeButton(
                                                                (buttonAbovePos - scroll + windowPos) / Main.DPIVector,
                                                                (buttonAboveSize) / Main.DPIVector, "", 0,
                                                                out bool isHovering));

                                                    if (acceptInputForButtonAbove)
                                                    {
                                                        
                                                        if (isHovering)
                                                            cancelJumpThisFrameDueToExtraButtonBeingClicked = true;
                                                        if (buttonClicked)
                                                        {
                                                            insertNewAnimationAtIndex_IndexType =
                                                                TaeEditorScreen.InsertAnimType.Before;
                                                            insertNewAnimationAtIndex_FromTae = category;
                                                            insertNewAnimationAtIndex_FromAnim = anim;
                                                        }
                                                    }

                                                    ImGui.PopStyleColor(3);

                                                    ImGuiDebugDrawer.DrawRect(
                                                        (buttonAbovePos - scroll + windowPos) / Main.DPIVector,
                                                        (buttonAboveSize) / Main.DPIVector, Color.Yellow,
                                                        thickness: 1);

                                                    




                                                    Vector2 plusHorizontalLineSize =
                                                        new Vector2(buttonAboveSize.Y * 0.666f, 2);
                                                    Vector2 plusVerticalLineSize =
                                                        new Vector2(2, buttonAboveSize.Y * 0.666f);
                                                    Vector2 plusHorizontalLinePos =
                                                        new Vector2(
                                                            buttonAbovePos.X + (buttonAboveSize.X / 2) -
                                                            (plusHorizontalLineSize.X / 2),
                                                            buttonAbovePos.Y + (buttonAboveSize.Y / 2) -
                                                            (plusHorizontalLineSize.Y / 2));
                                                    Vector2 plusVerticalLinePos =
                                                        new Vector2(
                                                            buttonAbovePos.X + (buttonAboveSize.X / 2) -
                                                            (plusVerticalLineSize.X / 2),
                                                            buttonAbovePos.Y + (buttonAboveSize.Y / 2) -
                                                            (plusVerticalLineSize.Y / 2));
                                                    ImGuiDebugDrawer.DrawRect(
                                                        (plusHorizontalLinePos - scroll + windowPos) / Main.DPIVector,
                                                        (plusHorizontalLineSize) / Main.DPIVector, imguiTextColor,
                                                        thickness: 0);
                                                    ImGuiDebugDrawer.DrawRect(
                                                        (plusVerticalLinePos - scroll + windowPos) / Main.DPIVector,
                                                        (plusVerticalLineSize) / Main.DPIVector, imguiTextColor,
                                                        thickness: 0);
                                                }

                                                if (drawButtonBelow)
                                                {

                                                    var lineA = new Vector2(4,
                                                        buttonBelowPos.Y + buttonBelowSize.Y / 2);
                                                    var lineB = new Vector2(
                                                        windowSize.X - scrollbarWidth - buttonBelowSize.X - 4,
                                                        buttonBelowPos.Y + buttonBelowSize.Y / 2);
                                                    ImGuiDebugDrawer.DrawLine(
                                                        (lineA - scroll + windowPos) / Main.DPIVector,
                                                        (lineB - scroll + windowPos) / Main.DPIVector, Color.Yellow, thickness: 3);

                                                    ImGui.PushStyleColor(ImGuiCol.Button,
                                                        new NVector4(0.4f, 0.4f, 0, 1));
                                                    ImGui.PushStyleColor(ImGuiCol.ButtonActive,
                                                        new NVector4(0.5f, 0.5f, 0, 1));
                                                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered,
                                                        new NVector4(0.6f, 0.6f, 0, 1));

                                                    bool buttonClicked =
                                                            (ImGuiDebugDrawer.FakeButton(
                                                                (buttonBelowPos - scroll + windowPos) / Main.DPIVector,
                                                                (buttonBelowSize) / Main.DPIVector, "", 0,
                                                                out bool isHovering));

                                                    if (acceptInputForButtonBelow)
                                                    {
                                                        
                                                        if (isHovering)
                                                            cancelJumpThisFrameDueToExtraButtonBeingClicked = true;
                                                        if (buttonClicked)
                                                        {
                                                            insertNewAnimationAtIndex_IndexType =
                                                                TaeEditorScreen.InsertAnimType.After;
                                                            insertNewAnimationAtIndex_FromTae = category;
                                                            insertNewAnimationAtIndex_FromAnim = anim;
                                                        }
                                                    }

                                                    ImGui.PopStyleColor(3);

                                                    ImGuiDebugDrawer.DrawRect(
                                                        (buttonBelowPos - scroll + windowPos) / Main.DPIVector,
                                                        (buttonBelowSize) / Main.DPIVector, Color.Yellow,
                                                        thickness: 1);

                                                    var plusHorizontalLineSize =
                                                        new Vector2(buttonBelowSize.Y * 0.666f, 2);
                                                    var plusVerticalLineSize =
                                                        new Vector2(2, buttonBelowSize.Y * 0.666f);
                                                    var plusHorizontalLinePos =
                                                        new Vector2(
                                                            buttonBelowPos.X + (buttonBelowSize.X / 2) -
                                                            (plusHorizontalLineSize.X / 2),
                                                            buttonBelowPos.Y + (buttonBelowSize.Y / 2) -
                                                            (plusHorizontalLineSize.Y / 2));
                                                    var plusVerticalLinePos =
                                                        new Vector2(
                                                            buttonBelowPos.X + (buttonBelowSize.X / 2) -
                                                            (plusVerticalLineSize.X / 2),
                                                            buttonBelowPos.Y + (buttonBelowSize.Y / 2) -
                                                            (plusVerticalLineSize.Y / 2));
                                                    ImGuiDebugDrawer.DrawRect(
                                                        (plusHorizontalLinePos - scroll + windowPos) / Main.DPIVector,
                                                        (plusHorizontalLineSize) / Main.DPIVector, imguiTextColor,
                                                        thickness: 0);
                                                    ImGuiDebugDrawer.DrawRect(
                                                        (plusVerticalLinePos - scroll + windowPos) / Main.DPIVector,
                                                        (plusVerticalLineSize) / Main.DPIVector, imguiTextColor,
                                                        thickness: 0);
                                                }




                                            }


                                            // Mouse within window.
                                            //if (mouseCursorPos.Y >= windowPos.Y && mouseCursorPos.Y <= windowPos.Y + windowSize.Y
                                            //    && mouseCursorPos.X >= windowPos.X && mouseCursorPos.X <= windowPos.X + windowSize.X)
                                            //{
                                            //    var mousePosLocal = mouseCursorPos + scroll - windowPos;

                                            //    if (EnableAnimListDebug)
                                            //    {
                                            //        ImGuiDebugDrawer.DrawRect(mousePosLocal - scroll + windowPos, new Vector2(32, 32), Color.Yellow, filled: false);
                                            //    }



                                            //    if (mousePosLocal.Y >= topCursorPos && mousePosLocal.Y < bottomCursorPos)
                                            //    {

                                            //    }


                                            //}


                                        }




                                        if (scrollToAnimNextFrame == anim)
                                        {
                                            debug_LastScrollCenterItem = approximateCenterOfItem;

                                            if (scrollToAnimNextFrame_IsCenter)
                                            {
                                                if (approximateCenterOfItem < centerOfScreen)
                                                {
                                                    float scrollDelta = (approximateCenterOfItem - centerOfScreen);
                                                    DoAutoScroll(scrollY + scrollDelta, scrollDelta);
                                                }

                                                if (approximateCenterOfItem > centerOfScreen)
                                                {
                                                    float scrollDelta = (approximateCenterOfItem - centerOfScreen);
                                                    DoAutoScroll(scrollY + scrollDelta, scrollDelta);
                                                }
                                            }
                                            else
                                            {
                                                if (approximateCenterOfItem < topOfScreenWithBuffer)
                                                {
                                                    float scrollDelta =
                                                        (approximateCenterOfItem - topOfScreenWithBuffer);
                                                    DoAutoScroll(scrollY + scrollDelta, scrollDelta);
                                                }

                                                if (approximateCenterOfItem > bottomOfScreenWithBuffer)
                                                {
                                                    float scrollDelta = (approximateCenterOfItem -
                                                                         bottomOfScreenWithBuffer);
                                                    DoAutoScroll(scrollY + scrollDelta, scrollDelta);
                                                }
                                            }






                                            //ImGui.SetScrollHereY();
                                            //scrollWindowToHere = animNameCursorPos.Y;
                                            scrollToAnimNextFrame = null;
                                            scrollToAnimNextFrame_TAE = null;
                                        }
                                    }





                                    ImGui.PopStyleColor();
                                    ImGui.PopStyleColor();
                                    ImGui.PopStyleColor();

                                    //ImGui.SameLine();

                                    //ImGui.PushStyleColor(ImGuiCol.HeaderActive, Main.Config.Colors.GuiColorAnimListHighlightRectFill.ToNVector4());
                                    //ImGui.PushStyleColor(ImGuiCol.Header, Main.Config.Colors.GuiColorAnimListHighlightRectFill.ToNVector4());
                                    //ImGui.PushStyleColor(ImGuiCol.Text, Main.Colors.GuiColorAnimListTextAnimDevName.ToNVector4());
                                    //{
                                    //    ImGui.Selectable($"{anim.AnimFileName}###NewTaeList_Tae{tae.TaeBindIndex}_Anim[{animIndex}]_FileName", ref newSelected_Right);
                                    //}
                                    //ImGui.PopStyleColor();
                                    //ImGui.PopStyleColor();
                                    //ImGui.PopStyleColor();

                                    if ((newSelected_Left) || (newSelected_Right != selected))
                                    {
                                        jumptoTae = category;
                                        jumpToAnimation = anim;
                                    }




                                    

                                    animIndex++;
                                }

                                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 8);

                                ImGui.TreePop();
                            }


                            if (treeOpenToggled)
                                category.IsTreeNodeOpen = !category.IsTreeNodeOpen;
                        }






                        var fileContainer = Document.EditorScreen.FileContainer;
                        if (fileContainer != null && fileContainer.Proj != null)
                        {


                            var taes = fileContainer.AllTAE.ToList();
                            foreach (var tae in taes)
                            {
                                if (!TaeBlockCaches.ContainsKey(tae))
                                    TaeBlockCaches.Add(tae, new TaeBlockCacheEntry());

                                var cache = TaeBlockCaches[tae];
                                cache.Debug_WasRenderedLastFrame = false;
                                TaeBlockCaches[tae] = cache;

                                // If cache is invalid and tree node is expanded, then do the cache calculation
                                if ((!TaeBlockCaches[tae].IsValid && tae.IsTreeNodeOpen) ||
                                    Main.Debug.DisableAnimListCulling)
                                {
                                    float startCursorY = ImGui.GetCursorPosY();
                                    RenderTaeBlock(fileContainer.Proj, tae, disableTaeCulling: true,
                                        disableAnimCulling: true);
                                    float endCursorY = ImGui.GetCursorPosY();

                                    var fontSize = ImGui.GetFontSize();

                                    if (CurrentTreeNodeSize < 0 || CurrentTreeNodeSize_FontScaleItsFor < 0 ||
                                        Math.Abs(fontSize - CurrentTreeNodeSize_FontScaleItsFor) > 0.001f)
                                    {
                                        float startCursorY_Font = ImGui.GetCursorPosY();
                                        ImGui.TreeNodeEx($"###NewTaeList_CalibrationNode",
                                            ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen |
                                            ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding |
                                            ImGuiTreeNodeFlags.Framed, $" ");
                                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 4);
                                        float endCursorY_Font = ImGui.GetCursorPosY();
                                        CurrentTreeNodeSize = endCursorY_Font - startCursorY_Font;
                                        CurrentTreeNodeSize_FontScaleItsFor = fontSize;
                                        TaeBlockCaches.Clear();
                                    }
                                    else
                                    {
                                        cache = TaeBlockCaches[tae];
                                        cache.BlockHeight = endCursorY - startCursorY;
                                        cache.IsValid = true;
                                        TaeBlockCaches[tae] = cache;
                                    }




                                }
                                // Otherwise, normal render, which will read the cache if applicable
                                else
                                {
                                    RenderTaeBlock(fileContainer.Proj, tae, disableTaeCulling: false,
                                        disableAnimCulling: false);
                                }



                            }

                            // Dummy so it can scroll down even when stuff near the end of the list is culled.
                            ImGui.Text(" ");

                            if (jumptoTae != null && jumpToAnimation != null &&
                                !cancelJumpThisFrameDueToExtraButtonBeingClicked)
                            {
                                MainTaeScreen.SelectNewAnimRef(jumptoTae, jumpToAnimation, isPushCurrentToHistBackwardStack: true);
                            }

                            if (insertNewAnimationAtIndex_IndexType != TaeEditorScreen.InsertAnimType.None &&
                                insertNewAnimationAtIndex_FromTae != null && insertNewAnimationAtIndex_FromAnim != null)
                            {
                                MainTaeScreen.InsertNewAnimAtIndex(insertNewAnimationAtIndex_FromTae,
                                    insertNewAnimationAtIndex_FromAnim, insertNewAnimationAtIndex_IndexType);
                            }


                            endOfDrawAction?.Invoke();


                            //ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 60);
                        }

                        if (Main.Debug.EnableAnimListDebug)
                        {
                            drawList.AddLine(
                                ImGui.GetWindowPos() + new NVector2(0, topOfScreenWithBuffer) -
                                new NVector2(0, ImGui.GetScrollY()),
                                ImGui.GetWindowPos() + new NVector2(1000, topOfScreenWithBuffer) -
                                new NVector2(0, ImGui.GetScrollY()), 0xFFFF0000);

                            drawList.AddLine(
                                ImGui.GetWindowPos() + new NVector2(0, bottomOfScreenWithBuffer) -
                                new NVector2(0, ImGui.GetScrollY()),
                                ImGui.GetWindowPos() + new NVector2(1000, bottomOfScreenWithBuffer) -
                                new NVector2(0, ImGui.GetScrollY()), 0xFF00FFFF);

                            drawList.AddLine(
                                ImGui.GetWindowPos() + new NVector2(0, debug_LastScrollCenterItem) -
                                new NVector2(0, ImGui.GetScrollY()),
                                ImGui.GetWindowPos() + new NVector2(1000, debug_LastScrollCenterItem) -
                                new NVector2(0, ImGui.GetScrollY()), 0xFFFFFF00);

                            var sb = new StringBuilder();
                            sb.AppendLine("TAE blocks rendered last frame:");
                            var taeBlocksRenderedThisFrame =
                                TaeBlockCaches.Where(kvp => kvp.Value.Debug_WasRenderedLastFrame).ToList();
                            foreach (var blockKvp in taeBlocksRenderedThisFrame)
                            {
                                sb.AppendLine($"    {blockKvp.Key.CategoryID}");
                            }

                            var statusText = sb.ToString();
                            drawList.AddText(Main.ImGuiFontPointer, Main.ImGuiFontPixelSize * Main.DPI,
                                ImGui.GetWindowPos() + new NVector2(50, 50),
                                Color.Fuchsia.PackedValue, statusText);
                        }

                       


                        if (autoScrollDestination >= 0)
                        {
                            if (autoScrollTimer >= 0)
                            {
                                autoScrollTimer += Main.DELTA_UPDATE;

                                float scrollRatio = MathHelper.Clamp(autoScrollTimer / autoScrollTimerMax, 0, 1);
                                ImGui.SetScrollY(scrollY + ((autoScrollDestination - scrollY) * scrollRatio));

                                if (autoScrollTimer >= autoScrollTimerMax + 0.1f)
                                {
                                    autoScrollDestination = -1;
                                    autoScrollTimer = -1;
                                }
                            }
                        }
                        else
                        {
                            autoScrollTimer = -1;
                        }
                    }
                    catch (Exception handled_ex) when (Main.EnableErrorHandler.AnimListBuild)
                    {
                        Main.HandleError(nameof(Main.EnableErrorHandler.AnimListBuild), handled_ex);
                    }
                
                    finally
                    {
                        StuffStoredWhenChangingDocs_Save_CallFromImguiContext();
                    }
                }
                

            }





        }
    }
}
