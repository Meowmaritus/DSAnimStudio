using DSAnimStudio.TaeEditor;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Tooltip : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.NoSave;

            public override string NewImguiWindowTitle => "Tooltip";
            
            private string TooltipText = "";
            private System.Numerics.Vector2 TooltipTextScale;
            private float TooltipWrapX = -1;
            private System.Numerics.Vector2 CalcSize;
            public System.Numerics.Vector2 TooltipPos;

            public void SetTooltipText(string text, float wrapX = -1)
            {
                TooltipText = text;
                TooltipTextScale = wrapX > 0 ? ImGui.CalcTextSize(text, wrapX) : ImGui.CalcTextSize(text);
                TooltipWrapX = wrapX;
                var imguiStyle = ImGui.GetStyle();
                var padding = imguiStyle.WindowPadding;
                CalcSize = new Vector2(TooltipTextScale.X + (padding.X * 2) + 8, TooltipTextScale.Y + (padding.Y * 2) + 8);
            }
            
            protected override void Init()
            {
                
                Flags = ImGuiWindowFlags.NoCollapse |
                        ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar |
                        ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoFocusOnAppearing
                        | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoNavFocus;
            }

            protected override void PreUpdate()
            {
                
                Flags = ImGuiWindowFlags.NoCollapse |
                        ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar |
                        ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoFocusOnAppearing
                        | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoNavFocus;
                //ImGui.SetWindowSize(CalcSize);
                ImGui.PushStyleColor(ImGuiCol.WindowBg, 0xFF202020);
                if (IsOpen)
                {
                    var windowSize = Main.LastBounds.Size.ToVector2();
                    
                    var winTop = 0;
                    var winBottom = windowSize.Y;
                    var winLeft = 0;
                    var winRight = windowSize.X;
                    
                    float top = TooltipPos.Y;
                    float bottom = TooltipPos.Y + CalcSize.Y;
                    float left = TooltipPos.X; 
                    float right = TooltipPos.X + CalcSize.X;

                    if (top < winTop)
                    {
                        float shift = (winTop - top);
                        top += shift;
                        bottom += shift;
                    }
                    
                    if (bottom > winBottom)
                    {
                        float shift = (winBottom - bottom);
                        top += shift;
                        bottom += shift;
                    }

                    if (left < winLeft)
                    {
                        float shift = (winLeft - left);
                        left += shift;
                        right += shift;
                    }
                    
                    if (right > winRight)
                    {
                        float shift = (winRight - right);
                        left += shift;
                        right += shift;
                    }
                    
                    ImGui.SetNextWindowSize(new Vector2(right - left, bottom - top));
                    ImGui.SetNextWindowPos(new Vector2(left, top));
                }

                ClipDockTabArea = false;
            }

            protected override void PostUpdate()
            {
                ImGui.PopStyleColor();
            }

            protected override void PreBuild()
            {
                
                var x = ImGui.GetWindowViewport();
                x.Flags = ImGuiViewportFlags.TopMost;
                
                ImGui.SetWindowFocus();
                
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                float wrapX = TooltipWrapX;
                string text = TooltipText;
                if (wrapX > 0)
                {
                    ImGui.PushTextWrapPos(wrapX);
                    try
                    {
                        ImGui.TextWrapped($"{text}");
                    }
                    finally
                    {
                        ImGui.PopTextWrapPos();
                    }
                    
                    
                }
                else
                {
                    ImGui.Text($"{text}");
                }
            }
        }
    }
}
