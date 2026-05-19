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
        public class GenericText : Dialog
        {
            public string Text;
            public Vector2 TextDialogSize = new Vector2(450, 280);

            public GenericText(string text, float sizeX, float sizeY)
                : base("Welcome###NewWelcomeDialog")
            {
                Text = text;
                TextDialogSize = new Vector2(sizeX, sizeY);

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
                var thisDialogSize = TextDialogSize * Main.DPIVector;
                var windowSize = Program.MainInstance.Window.ClientBounds.Size.ToVector2();

                ImGui.SetWindowPos(((windowSize / 2) - (thisDialogSize / 2)).ToCS());
                ImGui.SetWindowSize(thisDialogSize.ToCS());

                ImGui.TextWrapped(Text);
                //ImGui.Separator();
                //bool iUnderstand = Main.Config.WelcomeMessageDisabled;
                //ImGui.SetCursorPosX(240 * Main.DPI);
                //ImGui.Checkbox("Don't show me this again.", ref iUnderstand);
                //Main.Config.WelcomeMessageDisabled = iUnderstand;
                ImGui.Separator();
                if (Tools.SimpleClickButton("OK"))
                {
                    ResultType = ResultTypes.Accept;
                    //Main.SaveConfig();
                    Dismiss();
                }
            }
        }
    }
}
