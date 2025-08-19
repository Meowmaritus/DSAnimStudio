using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public static partial class MenuBar
    {
        public static TaeEditor.TaeEditorScreen Tae => zzz_DocumentManager.CurrentDocument.EditorScreen;

        public static bool IsAnyMenuOpen = false;
        public static bool IsAnyMenuOpenChanged = false;
        private static bool prevIsAnyMenuOpen = false;

        public static void DoWindow(Window w, string shortcut = null)
        {
            w.IsOpen = Checkbox($"{w.NewImguiWindowTitle}###Checkbox_WindowIsOpen|{w.ImguiTag}", w.IsOpen, shortcut: shortcut);
        }
        
        public static void OpenSite(string url)
        {
            Main.MainThreadLazyDispatch(() =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            });
                
        }
        
        public static void BuildMenuBar(ref bool anyItemFocused)
        {
            bool anyItemFocusedInner = false;
            bool isAnyMenuExpanded = false;
            
            BuildMenuBar_000_File(ref anyItemFocusedInner, ref isAnyMenuExpanded);
            BuildMenuBar_010_Edit(ref anyItemFocusedInner, ref isAnyMenuExpanded);
            BuildMenuBar_020_Graph(ref anyItemFocusedInner, ref isAnyMenuExpanded);
            BuildMenuBar_030_Simulation(ref anyItemFocusedInner, ref isAnyMenuExpanded);
            BuildMenuBar_040_Animation(ref anyItemFocusedInner, ref isAnyMenuExpanded);
            BuildMenuBar_050_Tools(ref anyItemFocusedInner, ref isAnyMenuExpanded);
            BuildMenuBar_060_Window(ref anyItemFocusedInner, ref isAnyMenuExpanded);
            BuildMenuBar_070_Help(ref anyItemFocusedInner, ref isAnyMenuExpanded);
            BuildMenuBar_080_SupportMeowmaritus(ref anyItemFocusedInner, ref isAnyMenuExpanded);

            if (Main.IsDebugBuild)
            {
                ImGui.MenuItem("Debug ImGui Dock Edit", "", ref Main.Debug.EnableImguiDebugDockEdit);
            }
            

            if (anyItemFocusedInner)
                anyItemFocused = true;
            
            IsAnyMenuOpen = isAnyMenuExpanded;

            IsAnyMenuOpenChanged = IsAnyMenuOpen != prevIsAnyMenuOpen;

            prevIsAnyMenuOpen = IsAnyMenuOpen;

            
        }

        public static float FloatSlider(string name, float currentValue, float min, float max, string format = "%f", float? clampMin = null, float? clampMax = null)
        {
            float v = currentValue;
            ImGui.SliderFloat(name, ref v, min, max, format, ImGuiSliderFlags.None);
            if (clampMin.HasValue && v < clampMin.Value)
                v = clampMin.Value;
            if (clampMax.HasValue && v > clampMax.Value)
                v = clampMax.Value;
            return v;
        }

        public static T EnumSelectorItem<T>(string itemName, T currentValue, Dictionary<T, string> entries = null, bool enabled = true)
            where T : Enum
        {
            if (ImGui.BeginMenu(itemName, enabled))
            {
                foreach (var kvp in entries)
                {
                    ImGui.MenuItem(kvp.Value, null, currentValue.Equals(kvp.Key));
                    if (ImGui.IsItemClicked())
                    {
                        currentValue = kvp.Key;
                        break;
                    }
                }

                ImGui.EndMenu();
            }

            return currentValue;
        }

        public static bool ClickItem(string text, bool enabled = true, string shortcut = null,
            Color? textColor = null, Color? shortcutColor = null, bool selected = false, Action onHovered = null)
        {
            if (textColor != null)
                ImGui.PushStyleColor(ImGuiCol.Text, textColor.Value.ToNVector4());
            if (shortcutColor != null)
                ImGui.PushStyleColor(ImGuiCol.TextDisabled, shortcutColor.Value.ToNVector4());

            if (shortcut == null)
                ImGui.MenuItem(text, null, selected, enabled: enabled);
            else
                ImGui.MenuItem(text, shortcut, selected, enabled: enabled);

            if (ImGui.IsItemHovered())
            {
                onHovered?.Invoke();
            }

            //var textLineCount = text.Split('\n').Length;
            //if (textLineCount > 1)
            //{
            //    for (int i = 1; i < textLineCount; i++)
            //        ImGui.NewLine();
            //}

            if (textColor != null)
                ImGui.PopStyleColor();
            if (shortcutColor != null)
                ImGui.PopStyleColor();
            

            bool wasClicked = false;

            if (enabled)
                wasClicked = ImGui.IsItemClicked();

            return enabled && wasClicked;
        }

        public static int IntItem(string text, int currentValue)
        {
            int v = currentValue;
            ImGui.InputInt(text, ref v);
            return v;
        }


        public static bool CheckboxDual(string text, bool currentValue, bool enabled = true,
            string shortcut = null, Color? textColor = null, Color? shortcutColor = null)
        {
            


            bool v = currentValue;
            if (textColor != null)
                ImGui.PushStyleColor(ImGuiCol.Text, textColor.Value.ToNVector4());
            if (shortcutColor != null)
                ImGui.PushStyleColor(ImGuiCol.TextDisabled, shortcutColor.Value.ToNVector4());
            ImGui.Checkbox($"##Checkbox_{text}", ref v);
            ImGui.SameLine();

            ImGui.MenuItem(text, shortcut, ref v, enabled: enabled);
            if (textColor != null)
                ImGui.PopStyleColor();
            if (shortcutColor != null)
                ImGui.PopStyleColor();

            return v;
        }

        public static bool Checkbox(string text, bool currentValue, bool enabled = true,
            string shortcut = null, Color? textColor = null, Color? shortcutColor = null)
        {
            bool v = currentValue;
            if (textColor != null)
                ImGui.PushStyleColor(ImGuiCol.Text, textColor.Value.ToNVector4());
            if (shortcutColor != null)
                ImGui.PushStyleColor(ImGuiCol.TextDisabled, shortcutColor.Value.ToNVector4());
            ImGui.MenuItem(text, shortcut, ref v, enabled: enabled);
            if (textColor != null)
                ImGui.PopStyleColor();
            if (shortcutColor != null)
                ImGui.PopStyleColor();
            return v;
        }

        public static bool CheckboxBig(string text, bool currentValue)
        {
            bool v = currentValue;
            ImGui.Checkbox(text, ref v);
            return v;
        }
    }
}
