using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSAnimStudio.ImguiOSD;
using DSAnimStudio.TaeEditor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsFormats;

namespace DSAnimStudio
{
    public partial class DSAProj
    {
        public class EditorInfo
        {
            public string GUID = Guid.NewGuid().ToString();

            public string DisplayName;
            public string Description;
            public Color? CustomColor = null;
            public List<Tag> TagInstances = new List<Tag>();

            private TagDrawConfig DrawCfg = new TagDrawConfig();

            public EditorInfo()
            {
                
            }

            private static void DrawErrorIcon(Vector2 pos, float itemHeight, Color color)
            {
                var r = itemHeight / 2;
                r = MathF.Round(r);
                pos += new Vector2(0, 0);

                //pos = pos.RoundInt();

                r += 0;

                //var erOutlineColor = Color.Black;

                //var erFillColor = Main.PulseLerpColor(new Color(0x90, 0, 0, 0xFF), new Color(0xD0, 0, 0, 0xFF), 2);
                //var erOuterOutlineColor = Main.PulseLerpColor(new Color(200, 200, 200, 255), Color.White, 2);
                //var erCrossColor = erOuterOutlineColor;

                //ImGuiDebugDrawer.DrawCircle(pos + new Vector2(0, 0), r, erFillColor, thickness: 0, dpiScale: false);

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

                ImGuiDebugDrawer.DrawLine(a, b, color, thickness: 1.5f, dpiScale: false);
                ImGuiDebugDrawer.DrawLine(c, d, color, thickness: 1.5f, dpiScale: false);

                //ImGuiDebugDrawer.DrawCircle(pos, r, erOutlineColor, thickness: 3, dpiScale: false);
                //ImGuiDebugDrawer.DrawCircle(pos, r + 2, erOuterOutlineColor, thickness: 1, dpiScale: false);
            }

            public class TagDrawConfig
            {
                public Color OutlineColor = Color.Black;
                public Color TextColor = Color.White;

                public float BoxOutlineThickness = 1;
                public float ButtonOutlineThickness = 1;

                public float BoxRoundness = 8;
                public float ButtonRoundness = 8;

                public Vector2 XGlyphOffset = new Vector2(2, 2);
                public Vector2 XGlyphSize = new Vector2(8, 8);
                public float XGlyphThickness = 1;
                public Color XGlyphColor = Color.White;

                public float VerticalBoxSize = 14;
                public float PaddingTop = 2;
                public float PaddingLeft = 2;
                public float PaddingBetweenTextAndButton = 2;
                public float PaddingRight = 2;
                public Vector2 ButtonSize = new Vector2(10, 10);
                public float FontSize = 10;
                public float TextOffsetY = 0;
            }

            public Vector2 NewTagCalcOffsetAfterDraw(string text, TagDrawConfig cfg, Vector2 anchorPos, bool isRightAnchor, bool includeXButton)
            {
                Vector2 newAnchorPos = anchorPos;

                float fontSizeRatio = cfg.FontSize / Main.ImGuiFontPixelSize;
                Vector2 actualTextSize = ImGui.CalcTextSize(text) * fontSizeRatio;

                float totalBoxWidth = actualTextSize.X + cfg.PaddingLeft + cfg.PaddingRight;
                if (includeXButton)
                    totalBoxWidth += cfg.PaddingBetweenTextAndButton + cfg.ButtonSize.X;

                //Vector2 boxTopLeft = anchorPos;

                if (isRightAnchor)
                {
                    newAnchorPos = new Vector2(anchorPos.X - totalBoxWidth, anchorPos.Y);
                    //boxTopLeft = newAnchorPos;
                }
                else
                {
                    newAnchorPos = new Vector2(anchorPos.X + totalBoxWidth, anchorPos.Y);
                }

                return newAnchorPos;
            }

            public void NewTagDraw(string text, Color tagColor, TagDrawConfig cfg, Vector2 anchorPos, 
                bool isRightAnchor, bool includeXButton, out Vector2 newAnchorPos, 
                out bool wasXClicked, out bool wasTagClickedAnywhere, out bool mouseHeldDownOnTag,
                FancyInputHandler input, RectF clippingRect, Color tagColor_Hover)
            {
                newAnchorPos = anchorPos;
                wasXClicked = false;
                wasTagClickedAnywhere = false;
                mouseHeldDownOnTag = false;

                //if (input.LeftClickUp)
                //    ImGuiDebugDrawer.DrawRect(anchorPos, new Vector2(8, 8), Color.Fuchsia);

                var mousePos = input.MousePosition;
                bool acceptingInput = clippingRect.Contains(mousePos);

                float fontSizeRatio = cfg.FontSize / Main.ImGuiFontPixelSize;
                Vector2 actualTextSize = ImGui.CalcTextSize(text) * fontSizeRatio;

                float totalBoxWidth = actualTextSize.X + cfg.PaddingLeft + cfg.PaddingRight;
                if (includeXButton)
                    totalBoxWidth += cfg.PaddingBetweenTextAndButton + cfg.ButtonSize.X;

                Vector2 boxTopLeft = anchorPos;

                if (isRightAnchor)
                {
                    newAnchorPos = new Vector2(anchorPos.X - totalBoxWidth, anchorPos.Y);
                    boxTopLeft = newAnchorPos;
                }
                else
                {
                    newAnchorPos = new Vector2(anchorPos.X + totalBoxWidth, anchorPos.Y);
                }

                var actualBoxRect = new RectF(boxTopLeft, new Vector2(totalBoxWidth, cfg.VerticalBoxSize));

               

                bool wholeBoxHovered = acceptingInput && actualBoxRect.Contains(mousePos);

                if (wholeBoxHovered)
                {
                    if (input.LeftClickHeld)
                        mouseHeldDownOnTag = true;

                    if (input.LeftClickUp)
                        wasTagClickedAnywhere = true;

                    tagColor = tagColor_Hover;
                }

                // Box background
                ImGuiDebugDrawer.DrawRect(actualBoxRect, tagColor, cornerRounding: cfg.BoxRoundness, thickness: 0);

                // Text
                var textPos = new Vector2(boxTopLeft.X + cfg.PaddingLeft, boxTopLeft.Y + cfg.PaddingTop);
                textPos.Y += cfg.TextOffsetY;
                ImGuiDebugDrawer.DrawText(text, textPos.RoundInt(), cfg.TextColor, fontSize: cfg.FontSize, includeCardinalShadows: true);

                

                if (includeXButton)
                {
                    var buttonTopLeft = new Vector2(boxTopLeft.X + cfg.PaddingLeft + actualTextSize.X + cfg.PaddingBetweenTextAndButton,
                        boxTopLeft.Y + cfg.PaddingTop);

                    var buttonRect = new RectF(buttonTopLeft, cfg.ButtonSize);

                    Color buttonBackgroundColor = tagColor;

                    //TODO
                    bool buttonHovered = acceptingInput && buttonRect.Contains(mousePos);

                    if (buttonHovered)
                    {
                        if (input.LeftClickHeld)
                            mouseHeldDownOnTag = true;

                        if (input.LeftClickUp)
                            wasXClicked = true;
                    }

                    //test lol
                    //buttonHovered = true;

                    if (buttonHovered)
                        buttonBackgroundColor = tagColor.MultBrightness(0.6666f);

                    // Button background
                    ImGuiDebugDrawer.DrawRect(buttonTopLeft, cfg.ButtonSize, buttonBackgroundColor, cornerRounding: cfg.ButtonRoundness, thickness: 0);

                    // Button X Glyph
                    Vector2 xGlyphNW = buttonTopLeft + new Vector2(cfg.XGlyphOffset.X, cfg.XGlyphOffset.Y);
                    Vector2 xGlyphNE = buttonTopLeft + new Vector2(cfg.XGlyphOffset.X + cfg.XGlyphSize.X, cfg.XGlyphOffset.Y);
                    Vector2 xGlyphSW = buttonTopLeft + new Vector2(cfg.XGlyphOffset.X, cfg.XGlyphOffset.Y + cfg.XGlyphSize.Y);
                    Vector2 xGlyphSE = buttonTopLeft + new Vector2(cfg.XGlyphOffset.X + cfg.XGlyphSize.X, cfg.XGlyphOffset.Y + cfg.XGlyphSize.Y);

                    ImGuiDebugDrawer.DrawLine(xGlyphNW + new Vector2(-1, -1), xGlyphSE + new Vector2(1, 1), 
                        Color.Black, thickness: cfg.XGlyphThickness * 2);
                    ImGuiDebugDrawer.DrawLine(xGlyphSW + new Vector2(-1, 1), xGlyphNE + new Vector2(1, -1), 
                        Color.Black, thickness: cfg.XGlyphThickness * 2);

                    ImGuiDebugDrawer.DrawLine(xGlyphNW, xGlyphSE, cfg.XGlyphColor, thickness: cfg.XGlyphThickness);
                    ImGuiDebugDrawer.DrawLine(xGlyphSW, xGlyphNE, cfg.XGlyphColor, thickness: cfg.XGlyphThickness);

                    // Button outline
                    //ImGuiDebugDrawer.DrawRect(buttonTopLeft, cfg.ButtonSize, cfg.OutlineColor, cornerRounding: cfg.ButtonRoundness, thickness: cfg.ButtonOutlineThickness);
                }

                // Box outline
                ImGuiDebugDrawer.DrawRect(actualBoxRect, 
                    cfg.OutlineColor, cornerRounding: cfg.BoxRoundness, thickness: cfg.BoxOutlineThickness);






                //ImGuiDebugDrawer.DrawRect(actualBoxRect, Color.Fuchsia, thickness: 1);
                //ImGuiDebugDrawer.DrawRect(mousePos, new Vector2(4, 4), Color.Lime);
                //ImGuiDebugDrawer.DrawRect(clippingRect, Color.Yellow * 0.5f, thickness: 0);
            }

            public void DrawJustTheTags(DSAProj proj, Vector2 anchorPos, bool isRightAnchor, RectF clippingRect, float tagsDrawScale, bool includeXButtons, Action<bool> setIsModified)
            {
                DrawCfg.BoxOutlineThickness = 1 * tagsDrawScale;
                DrawCfg.ButtonOutlineThickness = 1 * tagsDrawScale;

                DrawCfg.BoxRoundness = 24 * tagsDrawScale;
                DrawCfg.ButtonRoundness = 24 * tagsDrawScale;

                DrawCfg.XGlyphOffset = new Vector2(3, 3) * tagsDrawScale;
                DrawCfg.XGlyphSize = new Vector2(7, 7) * tagsDrawScale;
                DrawCfg.XGlyphThickness = 1 * tagsDrawScale;

                DrawCfg.VerticalBoxSize = 18 * tagsDrawScale;
                DrawCfg.PaddingTop = 2 * tagsDrawScale;
                DrawCfg.PaddingLeft = 6 * tagsDrawScale;
                DrawCfg.PaddingBetweenTextAndButton = 1 * tagsDrawScale;
                DrawCfg.PaddingRight = 4 * tagsDrawScale;
                DrawCfg.ButtonSize = new Vector2(14, 14) * tagsDrawScale;
                DrawCfg.FontSize = 19f * tagsDrawScale;
                DrawCfg.TextOffsetY = -4 * tagsDrawScale;

                Vector2 curPos = anchorPos;
                int tagIndexToRemove = -1;
                lock (proj._lock_Tags)
                {
                    int tIndex = 0;

                    IEnumerable<DSAProj.Tag> tagListToIterate = TagInstances;

                    if (isRightAnchor)
                        tagListToIterate = tagListToIterate.Reverse();

                    foreach (var t in tagListToIterate)
                    {
                        var tagColor = t.CustomColor ?? new Color(150, 150, 150, 255);
                        NewTagDraw(t.Name, tagColor, DrawCfg,
                            curPos, isRightAnchor: isRightAnchor, includeXButton: includeXButtons,
                            out Vector2 newAnchorPos, out bool wasXClicked,
                            out bool wasTagClickedAnywhere, out bool mouseHeldDownOnTag,
                            Main.Input, clippingRect, tagColor);

                        if (includeXButtons && wasXClicked)
                        {
                            tagIndexToRemove = tIndex;
                        }

                        curPos = newAnchorPos;

                        curPos += new Vector2(isRightAnchor ? -1 : 1, 0);

                        tIndex++;
                    }

                    if (tagIndexToRemove >= 0)
                    {
                        TagInstances.Remove(TagInstances[tagIndexToRemove]);
                        setIsModified?.Invoke(true);
                    }
                }


            }

            /// <summary>
            /// Make sure AlwaysShowVerticalScrollBar flag is set on whatever window this is shown in :fatcat:
            /// </summary>
            public void ShowImGui(ref bool anyFieldFocused, DSAProj proj, RectF imguiWindowClipRect, 
                RectF imguiWindowActualRect, Action<bool> setIsModified, bool showTagsOnly, 
                out bool anyInfoUpdated, float tagsDrawScale = 1)
            {
                var imguiStyle = ImGui.GetStyle();
                var imguiButtonColor = imguiStyle.Colors[(int)ImGuiCol.Button];
                var imguiButtonColor_Hover = imguiStyle.Colors[(int)ImGuiCol.ButtonHovered];


                //test
                //tagsDrawScale = (19f / 13f);


                





                anyInfoUpdated = false;
                lock (proj._lock_Tags)
                {
                    string oldDisplayName = DisplayName;
                    string oldDescription = Description;
                    Color? oldCustomColor = CustomColor;

                    if (!showTagsOnly)
                    {
                        ImguiOSD.Tools.InputTextNullable("Custom Name", ref DisplayName, 1024, nullToken: "");
                        anyFieldFocused |= ImGui.IsItemActive();
                        ImguiOSD.Tools.InputTextNullable("Custom Description", ref Description, 1024, nullToken: "");
                        anyFieldFocused |= ImGui.IsItemActive();
                        ImguiOSD.Tools.CustomColorPicker("Custom Color", $"EditorInfoCustomColor_{GUID}", ref CustomColor, Main.Colors.ColorProjectTagDefault);
                        anyFieldFocused |= ImGui.IsItemActive();

                        if (DisplayName != oldDisplayName || oldDescription != Description || oldCustomColor != CustomColor)
                        {
                            setIsModified(true);
                            anyInfoUpdated = true;
                        }
                    }

                    var dpi = Main.DPI;

                    //ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (4 * dpi));
                    //ImGui.Text("Tags:");
                    //ImGui.SameLine();
                    //ImGui.SetCursorPosY(ImGui.GetCursorPosY() - (8 * dpi));
                    //if (ImGui.BeginTabBar("TEST_TAB_BAR"))
                    //{
                    //    ImGui.TabItemButton("Test 1");
                    //    ImGui.TabItemButton("Test 2");
                    //    ImGui.TabItemButton("Test 3");

                    //    ImGui.EndTabBar();
                    //}

                    //var imguiStyle = ImGui.GetStyle();

                    var imguiTextPadding = imguiStyle.CellPadding;



                    //TEST. DELETE THIS
                    //ImGuiDebugDrawer.DrawRect(imguiWindowActualRect, Color.Red, 0, 16);
                    //ImGuiDebugDrawer.DrawRect(imguiWindowClipRect, Color.Lime, 0, 4);
                    
                    //ImGuiDebugDrawer.DrawRect(
                    //    imguiWindowActualRect.Location + ImGui.GetCursorPos().ToXna(), 
                    //    new Vector2(8, 8), 
                    //    Color.Cyan, 0, 2);
                    ////ImGui.Button("Test button");
                    //TEST END

                    Vector2 windowPos = ImGui.GetWindowPos();
                    Vector2 windowSize = ImGui.GetWindowSize();
                    //var imguiWindowClipRect = new Rectangle((int)windowPos.X, (int)windowPos.Y, (int)windowSize.X, (int)windowSize.Y);
                    var imguiScroll = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());




                    //var nextTagStartCursorPos = ImGui.GetCursorPos() + new System.Numerics.Vector2(0, 0);

                    //var newLineStartCursorPosX = nextTagStartCursorPos.X;
                    var startCursorPos = ImGui.GetCursorPos();

                    var rightClickInputRect_StartY = ImGui.GetCursorPosY();


                    //DrawCfg.BoxOutlineThickness = 1 * tagsDrawScale;
                    //DrawCfg.ButtonOutlineThickness = 1 * tagsDrawScale;

                    //DrawCfg.BoxRoundness = 16 * tagsDrawScale;
                    //DrawCfg.ButtonRoundness = 0 * tagsDrawScale;

                    //DrawCfg.XGlyphOffset = new Vector2(1, 1) * tagsDrawScale;
                    //DrawCfg.XGlyphSize = new Vector2(5, 5) * tagsDrawScale;
                    //DrawCfg.XGlyphThickness = 1 * tagsDrawScale;

                    //DrawCfg.VerticalBoxSize = 12 * tagsDrawScale;
                    //DrawCfg.PaddingTop = 2 * tagsDrawScale;
                    //DrawCfg.PaddingLeft = 4 * tagsDrawScale;
                    //DrawCfg.PaddingBetweenTextAndButton = 2 * tagsDrawScale;
                    //DrawCfg.PaddingRight = 4 * tagsDrawScale;
                    //DrawCfg.ButtonSize = new Vector2(8, 8) * tagsDrawScale;
                    //DrawCfg.FontSize = 13f * tagsDrawScale;
                    //DrawCfg.TextOffsetY = -3 * tagsDrawScale;


                    DrawCfg.BoxOutlineThickness = 1 * tagsDrawScale;
                    DrawCfg.ButtonOutlineThickness = 1 * tagsDrawScale;

                    DrawCfg.BoxRoundness = 24 * tagsDrawScale;
                    DrawCfg.ButtonRoundness = 24 * tagsDrawScale;

                    DrawCfg.XGlyphOffset = new Vector2(3, 3) * tagsDrawScale;
                    DrawCfg.XGlyphSize = new Vector2(7, 7) * tagsDrawScale;
                    DrawCfg.XGlyphThickness = 1 * tagsDrawScale;

                    DrawCfg.VerticalBoxSize = 18 * tagsDrawScale;
                    DrawCfg.PaddingTop = 2 * tagsDrawScale;
                    DrawCfg.PaddingLeft = 6 * tagsDrawScale;
                    DrawCfg.PaddingBetweenTextAndButton = 1 * tagsDrawScale;
                    DrawCfg.PaddingRight = 4 * tagsDrawScale;
                    DrawCfg.ButtonSize = new Vector2(14, 14) * tagsDrawScale;
                    DrawCfg.FontSize = 19f * tagsDrawScale;
                    DrawCfg.TextOffsetY = -4 * tagsDrawScale;


                    float SpacingBetweenTagsX = 2 * tagsDrawScale;
                    float SpacingBetweenTagsY = 2 * tagsDrawScale;


                    
                    




                    int tagIndexToRemove = -1;
                    if (TagInstances.Count > 0)
                    {
                        for (int tIndex = 0; tIndex < TagInstances.Count; tIndex++)
                        {

                            var originalCursorPos = ImGui.GetCursorPos();

                            var t = TagInstances[tIndex];
                            //if (tIndex > 0)
                            //    ImGui.SameLine();

                            //ImGui.SetCursorPos(nextTagStartCursorPos);

                            

                            var newTagDraw_CurrentPos = ImGui.GetCursorPos().ToXna();
                            newTagDraw_CurrentPos += imguiWindowActualRect.Location;
                            newTagDraw_CurrentPos -= imguiScroll;




                            var predictAnchorAfterDraw = NewTagCalcOffsetAfterDraw(t.Name, DrawCfg, newTagDraw_CurrentPos,
                                isRightAnchor: false, includeXButton: true);

                            // This tag would cut off on right side of window, go to next line;
                            if (newTagDraw_CurrentPos.X >= imguiWindowClipRect.Right || predictAnchorAfterDraw.X >= imguiWindowClipRect.Right)
                            {
                                // Test dummy blank thing to make window scroll horizontally
                                var tempStoreImguiCursorPos_Inner = ImGui.GetCursorPos();
                                ImGui.SameLine();
                                ImGui.Text("    ");
                                ImGui.SetCursorPos(tempStoreImguiCursorPos_Inner);

                                //TODO
                                //Probably will need 'newLineStartCursorPosX' here.
                                //Make sure to move the Imgui cursor down as well.
                                var curCursorPos = ImGui.GetCursorPos();

                                curCursorPos.X = startCursorPos.X;
                                curCursorPos.Y += DrawCfg.VerticalBoxSize;
                                curCursorPos.Y += SpacingBetweenTagsY;

                                ImGui.SetCursorPos(curCursorPos);

                                newTagDraw_CurrentPos = curCursorPos.ToXna();
                                newTagDraw_CurrentPos += imguiWindowActualRect.Location;
                                newTagDraw_CurrentPos -= imguiScroll;
                            }
                            var tagColor = t.CustomColor ?? new Color(150, 150, 150, 255);
                            NewTagDraw(t.Name, tagColor, DrawCfg, 
                                newTagDraw_CurrentPos, isRightAnchor: false, includeXButton: true, 
                                out Vector2 newAnchorPos, out bool wasXClicked, 
                                out bool wasTagClickedAnywhere, out bool mouseHeldDownOnTag, 
                                Main.Input, imguiWindowClipRect, tagColor);

                            //anyFieldFocused |= mouseHeldDownOnTag;

                            newAnchorPos += imguiScroll;
                            newAnchorPos -= imguiWindowActualRect.Location;
                            ImGui.SetCursorPos(newAnchorPos.ToCS());

                            // Test dummy blank thing to make window scroll horizontally
                            var tempStoreImguiCursorPos = ImGui.GetCursorPos();
                            ImGui.SameLine();
                            ImGui.Text("    ");
                            ImGui.SetCursorPos(tempStoreImguiCursorPos);

                            if (wasXClicked)
                            {
                                tagIndexToRemove = tIndex;
                            }




                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + SpacingBetweenTagsX);

                            //nextTagStartCursorPos = new System.Numerics.Vector2(newAnchorPos.X + SpacingBetweenTagsX, newAnchorPos.Y);

                           
                        }

                        //ImGui.Text(" ");


                    }
                    //else
                    //{
                    //    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (12 + ((dpi - 1) * 2)));
                    //}


                    // Shift cursor to underneath the last row of tags.
                    //ImGui.SetCursorPos(new System.Numerics.Vector2(startCursorPos.X, 
                    //    ImGui.GetCursorPosY() + DrawCfg.VerticalBoxSize + SpacingBetweenTagsY));



                    

                    if (tagIndexToRemove >= 0)
                    {
                        TagInstances.Remove(TagInstances[tagIndexToRemove]);
                        setIsModified(true);
                    }



                    DrawCfg.BoxRoundness = 0 * tagsDrawScale;
                    DrawCfg.FontSize = 19f * tagsDrawScale;
                    DrawCfg.PaddingRight += 2 * tagsDrawScale;
                    //DrawCfg.TextOffsetY += -4 * tagsDrawScale;

                    var addTagButtonAnchorPos = ImGui.GetCursorPos().ToXna();
                    addTagButtonAnchorPos += imguiWindowActualRect.Location;
                    addTagButtonAnchorPos -= imguiScroll;

                    var addTagButtonColor = imguiButtonColor.PackToXnaColor();
                    var addTagButtonHoverColor = imguiButtonColor_Hover.PackToXnaColor();

                    var isPopupOpen = ImGui.IsPopupOpen($"EditorInfo_AvailableTagsList##{GUID}");

                    if (isPopupOpen)
                    {
                        addTagButtonHoverColor = addTagButtonColor;
                    }

                    NewTagDraw("...", addTagButtonColor, DrawCfg, addTagButtonAnchorPos,
                        isRightAnchor: false, includeXButton: false,
                        out Vector2 addTagButtonNewAnchorPos, out _, 
                        out bool addTagButtonClicked, out bool addTagButtonMouseHeld,
                        Main.Input, imguiWindowClipRect, addTagButtonHoverColor);

                    //anyFieldFocused |= (addTagButtonMouseHeld);

                    addTagButtonNewAnchorPos += imguiScroll;
                    addTagButtonNewAnchorPos -= imguiWindowActualRect.Location;
                    ImGui.SetCursorPos(addTagButtonNewAnchorPos.ToCS() + new System.Numerics.Vector2(0, 4));

                    //ImGui.SameLine();
                    //ImGui.Text("    ");


                    if (addTagButtonClicked && !isPopupOpen)
                    {
                        ImGui.OpenPopup($"EditorInfo_AvailableTagsList##{GUID}");
                    }






                    ImGui.Text(" ");


                    //ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4 * dpi);
                    //ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(2 * dpi, 0));
                    //{
                    //    ImGui.Button("...", new System.Numerics.Vector2(28 * dpi, 16 * dpi));
                    //    anyFieldFocused |= ImGui.IsItemActive();
                    //    if (ImGui.IsItemClicked())
                    //    {
                    //        ImGui.OpenPopup("EditorInfo_AvailableTagsList");
                    //    }
                    //}
                    //ImGui.PopStyleVar();
                    //ImGui.PopStyleVar();

                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (10 * dpi));

                    if (ImGui.BeginPopup($"EditorInfo_AvailableTagsList##{GUID}"))
                    {
                        anyFieldFocused = true;

                        ImGui.MenuItem($"Quickly Add New Tag...##EditorInfo_AvailableTagsList_AddNewTag");
                        anyFieldFocused |= ImGui.IsItemActive();
                        if (ImGui.IsItemClicked())
                        {
                            ImGui.CloseCurrentPopup();

                            ImguiOSD.DialogManager.AskForInputString("Add New Tag", "Enter name for new tag.", "", result =>
                            {
                                lock (proj._lock_Tags)
                                {
                                    result = result.Trim();

                                    var resultTag = proj.Tags.FirstOrDefault(t => t.Name == result);
                                    if (resultTag == null)
                                    {
                                        resultTag = new Tag(result);
                                        proj.Tags.Add(resultTag);
                                    }
                                    if (resultTag != null && !TagInstances.Contains(resultTag))
                                        TagInstances.Add(resultTag);
                                    setIsModified(true);
                                }
                            }, errCheck =>
                            {
                                string result = null;
                                lock (proj._lock_Tags)
                                {
                                    errCheck = errCheck.Trim();
                                    if (proj.Tags.Any(tag => tag.Name == errCheck))
                                        result = $"Tag named '{errCheck}' already exists.";
                                }
                                return result;
                            }, canBeCancelled: true);
                        }

                        ImGui.Separator();
                        ImGui.MenuItem("Open Tags section in Project panel##EditorInfo_AvailableTagsList_OpenTagsPanel");
                        anyFieldFocused |= ImGui.IsItemActive();
                        if (ImGui.IsItemClicked())
                        {
                            DSAnimStudio.ImguiOSD.OSD.WindowProject.IsOpen = true;
                            DSAnimStudio.ImguiOSD.OSD.WindowProject.RequestShowTagsSection = true;
                            DSAnimStudio.ImguiOSD.OSD.WindowProject.IsRequestFocus = true;
                            //ImguiOSD.OSD.RequestWindowBringToFront(DSAnimStudio.ImguiOSD.OSD.WindowProject);
                        }

                        ImGui.Separator();

                        for (int i = 0; i < proj.Tags.Count; i++)
                        {
                            var thisTag = proj.Tags[i];
                            bool isTagAdded = TagInstances.Contains(thisTag);
                            ImGui.MenuItem($"{thisTag.Name}##EditorInfo_AvailableTagsList_Tag{i}", null, isTagAdded);
                            anyFieldFocused |= ImGui.IsItemActive();
                            if (ImGui.IsItemClicked())
                            {
                                if (isTagAdded)
                                {
                                    if (TagInstances.Contains(thisTag))
                                        TagInstances.Remove(thisTag);
                                    setIsModified(true);
                                }
                                else
                                {
                                    if (!TagInstances.Contains(thisTag))
                                        TagInstances.Add(thisTag);
                                    setIsModified(true);
                                }

                                ImGui.CloseCurrentPopup();
                                break;

                            }
                        }

                        

                        ImGui.EndPopup();
                    }

                    var mousePos = Main.Input.MousePosition * dpi;

                    var rightClickInputRect_EndY = ImGui.GetCursorPosY();


                    var rightClickInputRect = new RectF(windowPos.X, rightClickInputRect_StartY + windowPos.Y - imguiScroll.Y,
                        windowSize.X, rightClickInputRect_EndY - rightClickInputRect_StartY);

                    rightClickInputRect.Width -= Main.ImGuiScrollBarPixelSize * dpi;

                    //ImGuiDebugDrawer.DrawRect(rightClickInputRect.InverseDpiScaled(), Color.Fuchsia, 0, 2, new RectF(0, 0, Program.MainInstance.Window.ClientBounds.Width, Program.MainInstance.Window.ClientBounds.Height));

                    //ImGuiDebugDrawer.DrawRect(new RectF(mousePos, new Vector2(32,32)).InverseDpiScaled(), 
                    //    Color.Lime, 0, 2, new RectF(0, 0, Program.MainInstance.Window.ClientBounds.Width, 
                    //    Program.MainInstance.Window.ClientBounds.Height));

                    //ImGuiDebugDrawer.DrawRect(imguiWindowClipRect.InverseDpiScaled(), Color.Cyan, 0, 2, new RectF(0, 0, Program.MainInstance.Window.ClientBounds.Width, Program.MainInstance.Window.ClientBounds.Height));

                    if (Main.Input.RightClickUp && rightClickInputRect.Contains(mousePos) && imguiWindowClipRect.Contains(mousePos))
                    {
                        ImGui.OpenPopup($"EditorInfo_AvailableTagsList##{GUID}");
                    }



                    //TEST
                    //var testPos = ImGui.GetCursorPos().ToXna();
                    //testPos += imguiWindowActualRect.Location;
                    //testPos -= imguiScroll;
                    //ImGuiDebugDrawer.DrawText(Main.Input.GetDebugReport(true, true), testPos, Color.Yellow);
                }
            }

            public EditorInfo(string displayName, string desc = null, Color? customColor = null, List<Tag> tagInstances = null)
            {
                DisplayName = displayName;
                Description = desc;
                CustomColor = customColor;
                TagInstances = tagInstances?.ToList() ?? new();
            }

            public bool IsTheSameAs(EditorInfo other)
            {
                return DisplayName == other.DisplayName
                        && Description == other.Description
                        && CustomColor == other.CustomColor
                        && TagInstances.Count == other.TagInstances.Count
                        && (TagInstances.ToList().OrderBy(x => x.GUID).ToList()
                            .SequenceEqual(other.TagInstances.ToList().OrderBy(x => x.GUID).ToList()));
            }

            public EditorInfo GetClone()
            {
                var clone = new EditorInfo();
                clone.DisplayName = DisplayName;
                clone.Description = Description;
                clone.CustomColor = CustomColor;
                clone.TagInstances = TagInstances.ToList();
                return clone;
            }

            public void Deserialize(BinaryReaderEx br, DSAProj proj)
            {
                DisplayName = br.ReadNullPrefixUTF16();
                Description = br.ReadNullPrefixUTF16();
                CustomColor = br.ReadNullPrefixColor();
                TagInstances.Clear();
                int tagCount = br.ReadInt32();
                lock (proj._lock_Tags)
                {
                    for (int i = 0; i < tagCount; i++)
                    {
                        var tagIndex = br.ReadInt32();
                        if (tagIndex >= 0 && tagIndex < proj.Tags.Count)
                        {
                            TagInstances.Add(proj.Tags[tagIndex]);
                        }
                    }
                }
            }

            public void Serialize(BinaryWriterEx bw, DSAProj proj)
            {
                bw.WriteNullPrefixUTF16(DisplayName);
                bw.WriteNullPrefixUTF16(Description);
                bw.WriteNullPrefixColor(CustomColor);
                bw.WriteInt32(TagInstances.Count);
                lock (proj._lock_Tags)
                {
                    foreach (var t in TagInstances)
                    {
                        bw.WriteInt32(proj.Tags.IndexOf(t));
                    }
                }
            }

            public byte[] SerializeToBytes(DSAProj proj)
            {
                var bw = new BinaryWriterEx(false);
                Serialize(bw, proj);
                var result = bw.FinishBytes();
                return result;
            }

            public void DeserializeFromBytes(byte[] data, SoulsAssetPipeline.Animation.TAE.Template template, DSAProj proj)
            {
                var br = new BinaryReaderEx(false, data);
                Deserialize(br, proj);

            }
        }
    }
}
