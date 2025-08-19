using System.Linq;
using ImGuiNET;

namespace DSAnimStudio.ImguiOSD
{
    public static class ImGuiWidgets
    {

        public static byte[] NewParameterAob(string name, string imguiTag, byte[] input)
        {
            var aob = input.ToArray(); //Deref
            var windowWidth = ImGui.GetWindowSize().X;
            for (int b = 0; b < aob.Length; b++)
            {
                if (ImGui.GetCursorPosX() < (windowWidth - 48))
                    ImGui.SameLine();
                ImGui.PushItemWidth(24);
                int curBVal = aob[b];
                int prevBVal = curBVal;
                ImGui.InputInt($"{name}##{imguiTag}[{b}]", ref curBVal, 0, 0, ImGuiInputTextFlags.CharsHexadecimal |
                    ImGuiInputTextFlags.CharsUppercase);
                if (curBVal < 0)
                    curBVal = 0;
                if (curBVal > 255)
                    curBVal = 255;
                if (prevBVal != curBVal)
                {
                    aob[b] = (byte)curBVal;
                }

                ImGui.PopItemWidth();
            }

            return aob;
        }
        
    }
}