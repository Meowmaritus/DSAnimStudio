using ImGuiNET;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Dialog
    {
        public class RootTaePropertiesEdit : Dialog
        {
            public DSAProj Proj;
            public DSAProj.TaeProperties RootTaeProperties;
            public static bool ShowAdvancedProperties = false;

            public RootTaePropertiesEdit(DSAProj proj)
                : base("Edit Project Root TAE Properties")
            {
                Proj = proj;
                RootTaeProperties = proj.RootTaeProperties;
                CancelHandledByInheritor = true;
                AcceptHandledByInheritor = true;
                TitleBarXToCancel = true;
                EscapeKeyToCancel = true;
                AllowsResultCancel = true;
            }

            protected override void BuildInsideOfWindow()
            {
                Tools.EnumPicker("Bind Flags", ref RootTaeProperties.BindFlags);
                ImGui.InputText("Bind Directory", ref RootTaeProperties.BindDirectory, 1024);
                Tools.EnumPicker("Bind DCX Type", ref RootTaeProperties.BindDcxType);
                ImGui.InputInt("TAE Root Bind ID", ref RootTaeProperties.TaeRootBindID);
                Tools.EnumPicker("Format", ref RootTaeProperties.Format);
                ImGui.Checkbox("Is Old Demons Soul's Format 0x10000", ref RootTaeProperties.IsOldDemonsSoulsFormat_0x10000);
                ImGui.Checkbox("Is Old Demons Soul's Format 0x1000A", ref RootTaeProperties.IsOldDemonsSoulsFormat_0x1000A);
                Tools.GhettoInputLong("'AnimCount2' Field Value", ref RootTaeProperties.AnimCount2Value);
                ImGui.Checkbox("Is Big Endian", ref RootTaeProperties.BigEndian);
                Tools.GhettoInputByte("Flags 1/8", ref RootTaeProperties.Flags1);
                Tools.GhettoInputByte("Flags 2/8", ref RootTaeProperties.Flags2);
                Tools.GhettoInputByte("Flags 3/8", ref RootTaeProperties.Flags3);
                Tools.GhettoInputByte("Flags 4/8", ref RootTaeProperties.Flags4);
                Tools.GhettoInputByte("Flags 5/8", ref RootTaeProperties.Flags5);
                Tools.GhettoInputByte("Flags 6/8", ref RootTaeProperties.Flags6);
                Tools.GhettoInputByte("Flags 7/8", ref RootTaeProperties.Flags7);
                Tools.GhettoInputByte("Flags 8/8", ref RootTaeProperties.Flags8);
                Tools.InputTextNullable("Skeleton Name", ref RootTaeProperties.SkeletonName, 1024, "%null%");
                Tools.InputTextNullable("SIB Name", ref RootTaeProperties.SibName, 1024, "%null%");
                ImGui.Checkbox("Save With Action Track Info Stripped", ref RootTaeProperties.SaveWithActionTracksStripped);

                ImGui.Separator();

                if (RootTaeProperties.SaveEachCategoryToSeparateTae)
                    ImGui.Text("(Action Set Version is located in Anim Category Properties for c0000)");
                else
                    ImGui.InputInt("Action Set Version", ref RootTaeProperties.ActionSetVersion_ForSingleTaeOutput);


                ImGui.Separator();

                bool clickedCancel = Tools.SimpleClickButton("Cancel") || IsTitleBarXRequested;
                bool pressedEscape = IsEscapeKeyRequested;

                if (clickedCancel || pressedEscape)
                {
                    bool unsavedChanges = RootTaeProperties != Proj.RootTaeProperties;
                    bool forceClose = Main.Input.ShiftHeld;
                    if (!unsavedChanges || forceClose)
                    {
                        ResultType = ResultTypes.Cancel;
                        Dismiss();
                    }
                    else
                    {
                        DialogManager.AskYesNo("Unsaved Changes", "You have unsaved changes, would you like to discard them and close this dialog?", choice =>
                        {
                            if (choice == true)
                            {
                                ResultType = ResultTypes.Cancel;
                                Dismiss();
                            }
                        });
                    }
                }

                if (Tools.SimpleClickButton("Save & Accept"))
                {
                    ResultType = ResultTypes.Accept;
                    Dismiss();
                }

            }
        }
    }
}
