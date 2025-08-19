using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Dialog
    {
        public class EditActionColors : Dialog
        {
            public bool IsCustomColorSet = true;
            public Color SelectedColor;

            public EditActionColors()
                : base("Set Custom Color Of Selected Action(s)")
            {
                //CancleHandledByInheritor = true;
                //AcceptHandledByInheritor = true;
                TitleBarXToCancel = true;
                EscapeKeyToCancel = true;
                AllowsResultCancel = true;
                EnterKeyToAccept = true;
                AllowsResultAccept = true;

                SelectedColor = Main.Colors.GuiColorActionBox_Normal_Fill;
            }

            protected override void BuildInsideOfWindow()
            {

                var colVec4 = SelectedColor.ToNVector4();
                if (ImGui.ColorPicker4($"Color##{GUID}__ColorPicker", ref colVec4, 
                    ImGuiColorEditFlags.Uint8 | ImGuiColorEditFlags.DisplayHex | 
                    ImGuiColorEditFlags.PickerHueBar | ImGuiColorEditFlags.DisplayRGB | ImGuiColorEditFlags.DisplayHSV))
                {
                    SelectedColor = new Color(colVec4);
                    IsCustomColorSet = true;
                }

                ImGui.Separator();
                ImGui.Button("Remove Custom Color");
                if (ImGui.IsItemClicked())
                {
                    IsCustomColorSet = false;
                    ResultType = ResultTypes.Accept;
                    Dismiss();
                }

                ImGui.Separator();

                ImGui.Button("Accept");
                if (ImGui.IsItemClicked())
                {
                    ResultType = ResultTypes.Accept;
                    Dismiss();
                }

                ImGui.Button("Cancel");
                if (ImGui.IsItemClicked())
                {
                    ResultType = ResultTypes.Cancel;
                    Dismiss();
                }


            }
        }
    }
}
