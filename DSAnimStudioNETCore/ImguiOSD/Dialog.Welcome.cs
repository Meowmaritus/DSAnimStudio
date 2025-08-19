using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Dialog
    {
        public class Welcome : Dialog
        {

            
            public Welcome()
                : base("Welcome###NewWelcomeDialog")
            {
                CancelHandledByInheritor = true;
                AcceptHandledByInheritor = true;
                TitleBarXToCancel = false;
                EscapeKeyToCancel = false;
                AllowsResultCancel = false;

                AutoResize = false;
                NoMove = true;
                NoResize = true;

                
            }
            
            protected override void BuildInsideOfWindow()
            {
                var thisDialogSize = new Vector2(450, 280) * Main.DPIVector;
                var windowSize = Program.MainInstance.Window.ClientBounds.Size.ToVector2();

                ImGui.SetWindowPos(((windowSize / 2) - (thisDialogSize / 2)).ToCS());
                ImGui.SetWindowSize(thisDialogSize.ToCS());

                ImGui.TextWrapped(
                    "Welcome to DS Anim Studio v5.0.\n\n" +
                    "In this version the UI layout is a lot different.\n\n" +
                    "Try it out, but if you hate it, check out the very first\n" +
                    "option in the Toolbox panel (Window->Toolbox).\n\n" +
                    "Also, just so you know, the misc. panels such as \"Entity\",\n" +
                    "\"Scene\", etc. can be closed with the (X).");
                ImGui.Separator();
                bool iUnderstand = Main.Config.WelcomeMessageDisabled;
                ImGui.SetCursorPosX(240 * Main.DPI);
                ImGui.Checkbox("Don't show me this again.", ref iUnderstand);
                Main.Config.WelcomeMessageDisabled = iUnderstand;
                ImGui.Separator();
                if (Tools.SimpleClickButton("OK"))
                {
                    ResultType = ResultTypes.Accept;
                    Main.SaveConfig();
                    Dismiss();
                }
            }
        }
    }
}
