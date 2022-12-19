using DSAnimStudio.TaeEditor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public static class OSD
    {
        public static TaeEditorScreen Tae => Main.TAE_EDITOR;

        public static bool RequestCollapse = true;
        public static bool RequestExpandAllTreeNodes = true;
        public static bool DummyPolyListOpen = false;
        public static bool RenderConfigOpen = false;
        public static bool Focused;
        public static bool Hovered;
        public static Vector2 QueuedWindowPosChange = Vector2.Zero;
        public static Vector2 QueuedWindowSizeChange = Vector2.Zero;
        public static float RenderScale = 1;
        public static float WidthScale = 1;
        public static float RenderScaleTarget = 100;
        public static float WidthScaleTarget = 100;
#if DEBUG
        //very FromSoft style
        public static bool EnableDebugMenu = true;
        public static bool EnableDebugMenuFull = true;
#else
        public static bool EnableDebugMenu = true;
        public static bool EnableDebugMenuFull = false;
#endif
        public static bool IsInit { get; private set; } = true;


        public static Window.Debug WindowDebug = new Window.Debug();
        public static Window.EditPlayerEquip WindowEditPlayerEquip = new Window.EditPlayerEquip();
        public static Window.Help WindowHelp = new Window.Help();
        public static Window.SceneManager WindowSceneManager = new Window.SceneManager();
        public static Window.Toolbox WindowToolbox = new Window.Toolbox();
        public static Window.WorkspaceConfig WindowEntitySettings = new Window.WorkspaceConfig();
        public static Window.WwiseManager WindowWwiseManager = new Window.WwiseManager();


        public static Window.EventInspector SpWindowEventInspector = new Window.EventInspector();

        public static void Build(float elapsedTime, float offsetX, float offsetY)
        {
            var io = ImGui.GetIO();
            io.FontGlobalScale = Main.DPIY;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 7.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(14.0f, 8.0f));
            
            DialogManager.UpdateDialogs();

            if (ImGui.BeginMainMenuBar())
            {
                MenuBar.BuildMenuBar();
                ImGui.EndMainMenuBar();
            }

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0.15f, 0.15f, 0.15f, Focused ? 1 : 0.65f));
            TooltipManager.PreUpdate(elapsedTime, offsetX, offsetY);

            WindowDebug.Update();
            WindowEditPlayerEquip.Update();
            WindowHelp.Update();
            WindowSceneManager.Update();
            WindowToolbox.Update();
            WindowEntitySettings.Update();
            WindowWwiseManager.Update();


            SpWindowEventInspector.Update();


            ImGui.PopStyleColor();
            ImGui.PopStyleVar(3);
            TooltipManager.PostUpdate();
            IsInit = false;

            RequestExpandAllTreeNodes = false;
            RequestCollapse = false;


            Focused = ImGui.IsAnyItemFocused() || ImGui.IsWindowFocused(ImGuiFocusedFlags.AnyWindow) || DialogManager.AnyDialogsShowing;
            Hovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) || ImGui.IsAnyItemHovered();
            //Focused = io.WantCaptureKeyboard;
            //Hovered = io.WantCaptureMouse;

            if (Focused || Hovered)
            {
                Main.Input.CursorType = MouseCursorType.Arrow;
            }
        }
    }
}
