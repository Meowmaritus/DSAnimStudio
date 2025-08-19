using ImGuiNET;
using Microsoft.Xna.Framework;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public static class Tools
    {
        private static string CurrentColorEditorOpen = "";

        private static ColorConfig DefaultColorConfig = new ColorConfig();

        
        // public static float FloatSlider(string name, float currentValue, float min, float max, string format = "%f", float? clampMin = null, float? clampMax = null)
        // {
        //     float v = currentValue;
        //     ImGui.SliderFloat(name, ref v, min, max, format, ImGuiSliderFlags.None);
        //     if (clampMin.HasValue && v < clampMin.Value)
        //         v = clampMin.Value;
        //     if (clampMax.HasValue && v > clampMax.Value)
        //         v = clampMax.Value;
        //     return v;
        // }
        
        
        public static int ColorButtonWidth => (int)Math.Round(300 * Main.DPI * OSD.RenderScale * OSD.WidthScale);
        public static int ColorButtonHeight => (int)Math.Round(26 * Main.DPI * OSD.RenderScale);
        private static Dictionary<string, Action> DefaultColorValueActions = new Dictionary<string, Action>();

        public static bool SimpleClickButton(string name)
        {
            ImGui.Button(name);
            return ImGui.IsItemClicked();
        }

        public static void CustomColorPicker(string text, string imguiID, ref Color? c, Color defaultColor)
        {
            bool hasValue = c.HasValue;
            ImGui.Checkbox($"##{imguiID}__Checkbox", ref hasValue);
            ImGui.SameLine();
            if (hasValue && !c.HasValue)
                c = defaultColor;
            else if (!hasValue && c.HasValue)
                c = null;

            if (c.HasValue)
            {
                var colVec4 = c.Value.ToNVector4();
                ImGui.ColorEdit4($"{text}##{imguiID}__ColorPicker", ref colVec4, ImGuiColorEditFlags.Uint8 | ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.PickerHueBar);
                c = new Color(colVec4);
            }
            else
            {
                ImGui.BeginDisabled();
                var colVec4 = System.Numerics.Vector4.Zero;
                ImGui.ColorEdit4($"{text}##{imguiID}__ColorPicker", ref colVec4, ImGuiColorEditFlags.Uint8 | ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.PickerHueBar);
                ImGui.EndDisabled();
            }


        }

        public static void HandleColor(ref bool anyFieldFocused, string name, Func<ColorConfig, Color> getColor, Action<ColorConfig, Color> setColor)
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
                anyFieldFocused |= ImGui.IsItemActive();
                OSD.TooltipManager_ColorPicker.DoTooltip($"Color: {name}", "Allows you to adjust the color in detail." +
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


        private class EnumPickerCache
        {
            public List<object> Values = new List<object>();
            public string[] Names;
        }
        private static object _lock_enumPickerCacheDict = new object();
        private static Dictionary<string, EnumPickerCache> enumPickerCacheDict = new Dictionary<string, EnumPickerCache>();

        public static void EnumPicker<T>(string format, ref T value, Func<T, string> getNameFunc = null, bool disableCache = false, List<T> validEntries = null)
            where T : Enum
        {
            EnumPickerCache cache = null;
            lock (_lock_enumPickerCacheDict)
            {
                string enumName = typeof(T).Name;
                if (!disableCache && enumPickerCacheDict.ContainsKey($"{enumName}|{format}"))
                {
                    cache = enumPickerCacheDict[$"{enumName}|{format}"];
                }
                else
                {
                    cache = new EnumPickerCache();
                    var enumValues = (T[])Enum.GetValues(typeof(T));
                    foreach (var v in enumValues)
                    {
                        if (validEntries != null && !validEntries.Contains(v))
                            continue;
                        cache.Values.Add(v);
                    }

                    if (getNameFunc != null)
                        cache.Names = cache.Values.Select(x => getNameFunc((T)x)).ToArray();
                    else
                        cache.Names = cache.Values.Select(x => x.ToString()).ToArray();
                    if (!disableCache)
                        enumPickerCacheDict[$"{enumName}|{format}"] = cache;
                }

                int selectedTaeFormatIndex = cache.Values.IndexOf(value);
                ImGui.Combo(format, ref selectedTaeFormatIndex, cache.Names, cache.Names.Length);
                if (selectedTaeFormatIndex >= 0 && selectedTaeFormatIndex < cache.Values.Count)
                    value = (T)cache.Values[selectedTaeFormatIndex];
            }
        }
        
        public static void EnumPicker(string format, ref object value, Type enumType, Func<object, string> getNameFunc = null, bool disableCache = false, List<object> validEntries = null)
        {
            EnumPickerCache cache = null;
            lock (_lock_enumPickerCacheDict)
            {
                string enumName = enumType.Name;
                if (!disableCache && enumPickerCacheDict.ContainsKey(enumName))
                {
                    cache = enumPickerCacheDict[enumName];
                }
                else
                {
                    cache = new EnumPickerCache();
                    var enumValues = Enum.GetValues(enumType);
                    var enumNames = new List<string>();
                    foreach (var v in enumValues)
                    {
                        if (validEntries != null && !validEntries.Contains(v))
                            continue;
                        cache.Values.Add(v);
                        enumNames.Add(getNameFunc != null ? getNameFunc(v) : v.ToString());
                    }

                    cache.Names = enumNames.ToArray();
                    if (!disableCache)
                        enumPickerCacheDict[enumName] = cache;
                }

                int selectedTaeFormatIndex = cache.Values.IndexOf(value);
                ImGui.Combo(format, ref selectedTaeFormatIndex, cache.Names, cache.Names.Length);
                if (selectedTaeFormatIndex >= 0 && selectedTaeFormatIndex < cache.Values.Count)
                    value = cache.Values[selectedTaeFormatIndex];
            }
        }

        public static void InputTextNullable(string label, ref string input, uint maxLength, string nullToken = "%null%",
            ImGuiInputTextFlags flags = ImGuiInputTextFlags.None, 
            ImGuiInputTextCallback callback = null, IntPtr user_data = new IntPtr())
        {
            string curVal = input ?? nullToken;
            ImGui.InputText(label, ref curVal, maxLength, flags, callback, user_data);
            if (curVal == nullToken)
                input = null;
            else
                input = curVal;
        }

        public static void GhettoInputByte(string label, ref byte v, byte step = 1, byte step_fast = 100, 
            ImGuiInputTextFlags flags = ImGuiInputTextFlags.None, byte minVal = byte.MinValue, byte maxVal = byte.MaxValue)
        {
            int curVal = v;
            ImGui.InputInt(label, ref curVal, step, step_fast, flags);
            byte newV = (byte)curVal;
            if (newV < minVal)
                newV = minVal;
            else if (newV > maxVal)
                newV = maxVal;
            v = newV;
        }
        
        public static void GhettoInputSbyte(string label, ref sbyte v, byte step = 1, byte step_fast = 100, 
            ImGuiInputTextFlags flags = ImGuiInputTextFlags.None, sbyte minVal = sbyte.MinValue, sbyte maxVal = sbyte.MaxValue)
        {
            int curVal = v;
            ImGui.InputInt(label, ref curVal, step, step_fast, flags);
            sbyte newV = (sbyte)curVal;
            if (newV < minVal)
                newV = minVal;
            else if (newV > maxVal)
                newV = maxVal;
            v = newV;
        }
        
        public static void GhettoInputShort(string label, ref short v, byte step = 1, byte step_fast = 100, 
            ImGuiInputTextFlags flags = ImGuiInputTextFlags.None, short minVal = short.MinValue, short maxVal = short.MaxValue)
        {
            int curVal = v;
            ImGui.InputInt(label, ref curVal, step, step_fast, flags);
            short newV = (short)curVal;
            if (newV < minVal)
                newV = minVal;
            else if (newV > maxVal)
                newV = maxVal;
            v = newV;
        }
        
        

        public static void GhettoInputLong(string label, ref long v, int step = 1, int step_fast = 100, 
            ImGuiInputTextFlags flags = ImGuiInputTextFlags.None, long minVal = long.MinValue, long maxVal = long.MaxValue)
        {
            int curVal = (int)v;
            ImGui.InputInt(label, ref curVal, step, step_fast, flags);
            
            long newV = curVal;
            if (newV < minVal)
                newV = minVal;
            else if (newV > maxVal)
                newV = maxVal;
            v = newV;
        }

        // Would be very hard to implement lol
        //public static bool AskYesNo(string question)
        //{

        //}
    }
}
