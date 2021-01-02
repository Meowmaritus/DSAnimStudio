using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public static class Tools
    {
        private static string CurrentColorEditorOpen = "";

        private static ColorConfig DefaultColorConfig = new ColorConfig();

        
        public static int ColorButtonWidth => (int)Math.Round(300 * Main.DPIX * OSD.RenderScale * OSD.WidthScale);
        public static int ColorButtonHeight => (int)Math.Round(26 * Main.DPIY * OSD.RenderScale);
        private static Dictionary<string, Action> DefaultColorValueActions = new Dictionary<string, Action>();

        public static void HandleColor(string name, Func<ColorConfig, Color> getColor, Action<ColorConfig, Color> setColor)
        {
            if (OSD.IsInit)
            {
                if (!DefaultColorValueActions.ContainsKey(name))
                    DefaultColorValueActions.Add(name, () => setColor.Invoke(Main.Colors, getColor(DefaultColorConfig)));
            }

            var color = getColor.Invoke(Main.Colors);
            System.Numerics.Vector4 c = new System.Numerics.Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

            //ImGui.ColorEdit4(name, ref c);

            float colorLightness = (0.3086f * c.X + 0.6094f * c.Y + 0.0820f * c.Z) * c.W;

            if (colorLightness > 0.5f)
                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0, 0, 0, 1));
            else
                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 1, 1, 1));

            ImGui.PushStyleColor(ImGuiCol.Button, c);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, c * 1.25f);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, c * 0.75f);
            ImGui.Button(name, new System.Numerics.Vector2(ColorButtonWidth, ColorButtonHeight));
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                if (CurrentColorEditorOpen == name)
                    CurrentColorEditorOpen = "";
                else
                    CurrentColorEditorOpen = name;
            }
            else if (ImGui.IsItemClicked(ImGuiMouseButton.Middle))
            {
                color = getColor(DefaultColorConfig);
                c = new System.Numerics.Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
                setColor.Invoke(Main.Colors, color);
                //if (DefaultColorValueActions.ContainsKey(name))
                //    DefaultColorValueActions[name].Invoke();
            }


            //ImGui.ColorButton(name, c);

            if (CurrentColorEditorOpen == name)
            {
                ImGui.ColorPicker4(name, ref c);
                TooltipManager.DoTooltip($"Color: {name}", "Allows you to adjust the color in detail." +
                    "\n" +
                    "\nThe number boxes are as follows:" +
                    "\n    [R] [G] [B] [A] (Red/Green/Blue/Alpha)" +
                    "\n    [H] [S] [V] [A] (Hue/Saturation/Value/Alpha)" +
                    "\n    [ Hexadecimal ] (Hexadecimal representation of the color)");
                ImGui.Separator();
            }

            //if (ImGui.IsItemClicked())
            //{
            //    if (CurrentColorEditorOpen == name)
            //    {
            //        CurrentColorEditorOpen = "";
            //    }
            //    else
            //    {
            //        CurrentColorEditorOpen = name;
            //    }

            //}

            setColor(Main.Colors, new Color(c.X, c.Y, c.Z, c.W));
        }

        public static void PushGrayedOut(float valueMult = 0.5f)
        {
            var dv = new System.Numerics.Vector4(valueMult, valueMult, valueMult, 1);

            ImGui.PushStyleColor(ImGuiCol.Text,
                new System.Numerics.Vector4(255f / 255f, 255f / 255f, 255f / 255f, 1) * dv);
            ImGui.PushStyleColor(ImGuiCol.FrameBg,
                new System.Numerics.Vector4(40f / 255f, 57f / 255f, 83f / 255f, 1) * dv);
            ImGui.PushStyleColor(ImGuiCol.Button,
                new System.Numerics.Vector4(49f / 255f, 83f / 255f, 123f / 255f, 1) * dv);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive,
                new System.Numerics.Vector4(49f / 255f, 83f / 255f, 123f / 255f, 1) * dv);
                //new System.Numerics.Vector4(15f / 255f, 135f / 255f, 250f / 255f, 1) * dv);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered,
                new System.Numerics.Vector4(49f / 255f, 83f / 255f, 123f / 255f, 1) * dv);
                //new System.Numerics.Vector4(66f / 255f, 150f / 255f, 250f / 255f, 1) * dv);
        }

        public static void PopGrayedOut()
        {
            ImGui.PopStyleColor(5);
        }

        public static bool FancyComboBox(string disp, ref int curIndex, string[] items)
        {
            int idx = curIndex;
            bool res = ImGui.BeginCombo(disp, (idx >= 0 && idx < items.Length) ? items[idx] : "");
            if (res)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    bool selected = i == idx;
                    ImGui.Selectable(items[i], ref selected);
                    if (selected && ImGui.IsWindowAppearing())
                    {
                        ImGui.SetItemDefaultFocus();
                        ImGui.SetScrollHereY();
                    }

                    if (selected)
                        idx = i;
                }
                ImGui.EndCombo();
            }
            curIndex = idx;
            return res;
        }

        // Would be very hard to implement lol
        //public static bool AskYesNo(string question)
        //{

        //}
    }
}
