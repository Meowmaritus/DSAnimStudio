using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public class ImGuiEnumListBoxPicker<T>
    {
        private Dictionary<T, string> itemDict;
        private List<T> itemValues;
        private string[] itemNames;

        public ImGuiEnumListBoxPicker(Dictionary<T, string> dict)
        {
            itemDict = dict;
            itemValues = dict.Keys.ToList();
            itemNames = dict.Values.ToArray();
        }

        public void ShowPickerCombo(string label, string imguiTag, ref T val, T unmappedVal, string descriptionForTooltip = null, int imguiItemWidthBase = 320)
        {
            ImGui.PushItemWidth(imguiItemWidthBase * Main.DPIX * OSD.RenderScale * OSD.WidthScale);
            ImGui.LabelText(" ", $"{(label ?? " ")}");
            ImGui.PopItemWidth();

            if (descriptionForTooltip != null)
                TooltipManager.DoTooltip(label ?? " ", descriptionForTooltip ?? " ");
            ImGui.PushItemWidth(imguiItemWidthBase * Main.DPIX * OSD.RenderScale * OSD.WidthScale);
            {
                int currentItemIndex = itemValues.IndexOf(val);
                //ImGui.ListBox($" ###{imguiTag}", ref currentItemIndex, itemNames, itemNames.Length);
                ImGui.Combo($" ###{imguiTag}", ref currentItemIndex, itemNames, itemNames.Length);

                val = currentItemIndex >= 0 ? itemValues[currentItemIndex] : unmappedVal;
            }
            ImGui.PopItemWidth();
        }

        public void ShowPicker(string label, string imguiTag, ref T val, T unmappedVal, string descriptionForTooltip = null, int imguiItemWidthBase = 320)
        {
            ImGui.PushItemWidth(imguiItemWidthBase * Main.DPIX * OSD.RenderScale * OSD.WidthScale);
            ImGui.LabelText(" ", $"{(label ?? " ")}###{imguiTag}");
            ImGui.PopItemWidth();

            if (descriptionForTooltip != null)
                TooltipManager.DoTooltip(label ?? " ", descriptionForTooltip ?? " ");
            ImGui.PushItemWidth(imguiItemWidthBase * Main.DPIX * OSD.RenderScale * OSD.WidthScale);
            {
                int currentItemIndex = itemValues.IndexOf(val);
                ImGui.ListBox($" ###{imguiTag}", ref currentItemIndex, itemNames, itemNames.Length);
                val = currentItemIndex >= 0 ? itemValues[currentItemIndex] : unmappedVal;
            }
            ImGui.PopItemWidth();
        }
    }
}
