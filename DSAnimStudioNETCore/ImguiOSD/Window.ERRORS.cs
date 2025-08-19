using DSAnimStudio.TaeEditor;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class ERRORS : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            public override string NewImguiWindowTitle => "ERRORS";
            public DSAProj GetProj()
            {
                return MainTaeScreen?.FileContainer?.Proj;
            }
            protected override void Init()
            {
                
            }

            protected override void PreUpdate()
            {
                var proj = GetProj();
                if (proj != null && proj.ErrorContainer.AnyErrors())
                    IsOpen = true;

                CustomBorderColor_Focused = new Vector4(1, 0, 0, 1);
                CustomBorderColor_Unfocused = new Vector4(1, 0, 0, 1);
                CustomTextColor = null;
                
                ImGui.PushStyleColor(ImGuiCol.TitleBg, new Vector4(0.75f, 0, 0, 1f));
                ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.75f, 0, 0, 1f));
                ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, new Vector4(0.75f, 0, 0, 1f));
            }

            protected override void PostUpdate()
            {
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                var proj = GetProj();
                if (proj != null && proj.ErrorContainer.AnyErrors())
                {
                    proj.ErrorContainer.CreateJumpToErrorImguiControls(MainTaeScreen);
                }
            }
        }
    }
}
