using ImGuiNET;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsFormats;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Dialog
    {
        public class RootTaePropertiesEdit : Dialog
        {
            public DSAProj Proj;
            public DSAProj.TaeProperties RootTaeProperties;
            public static bool ShowAdvancedProperties = false;

            private string[] dcxTypeNames;
            private List<DCX.Type> dcxTypeValues;
            private int dcxTypeIndex = -1;
            
            private string[] oodleCompressionTypeNames;
            private List<Oodle.OodleLZ_Compressor> oodleCompressionTypeValues;

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

                dcxTypeValues = ((DCX.Type[])Enum.GetValues<DCX.Type>()).ToList();
                dcxTypeNames = dcxTypeValues.Select(x => x.ToString()).ToArray();

                oodleCompressionTypeValues = ((Oodle.OodleLZ_Compressor[])Enum.GetValues<Oodle.OodleLZ_Compressor>()).ToList();
                oodleCompressionTypeNames = oodleCompressionTypeValues.Select(x => x.ToString()).ToArray();
            }

            protected override void BuildInsideOfWindow()
            {
                Tools.EnumPicker("Bind Flags", ref RootTaeProperties.BindFlags);
                ImGui.InputText("Bind Directory", ref RootTaeProperties.BindDirectory, 1024);

                if (ImGui.TreeNode("Bind DCX CompressionInfo"))
                {

                    


                    

                    ImGui.Text($"DCX Type: {RootTaeProperties.BindDcxCompressionInfo.Type}");

                    switch (RootTaeProperties.BindDcxCompressionInfo)
                    {
                        case DCX.DcxDfltCompressionInfo dcxDflt:
                            int dcxDfltUnk04 = dcxDflt.Unk04;
                            int dcxDfltUnk10 = dcxDflt.Unk10;
                            int dcxDfltUnk14 = dcxDflt.Unk14;
                            byte dcxDfltUnk30 = dcxDflt.Unk30;
                            byte dcxDfltUnk38 = dcxDflt.Unk38;

                            ImGui.InputInt($"DCX_DFLT.{nameof(dcxDflt.Unk04)}", ref dcxDfltUnk04, 
                                1, 0x10, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.CharsUppercase);

                            ImGui.InputInt($"DCX_DFLT.{nameof(dcxDflt.Unk10)}", ref dcxDfltUnk10, 
                                1, 0x10, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.CharsUppercase);

                            ImGui.InputInt($"DCX_DFLT.{nameof(dcxDflt.Unk14)}", ref dcxDfltUnk14, 
                                1, 0x10, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.CharsUppercase);

                            Tools.GhettoInputByte($"DCX_DFLT.{nameof(dcxDflt.Unk30)}", ref dcxDfltUnk30, 
                                1, 0x10, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.CharsUppercase);

                            Tools.GhettoInputByte($"DCX_DFLT.{nameof(dcxDflt.Unk38)}", ref dcxDfltUnk38, 
                                1, 0x10, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.CharsUppercase);

                            RootTaeProperties.BindDcxCompressionInfo = new DCX.DcxDfltCompressionInfo(
                                dcxDfltUnk04, dcxDfltUnk10, dcxDfltUnk14, dcxDfltUnk30, dcxDfltUnk38);

                            break;
                        case DCX.DcxKrakCompressionInfo dcxKrak:
                            byte dcxKrakCompressionLevel = dcxKrak.CompressionLevel;
                            int selectedDcxKrakOodleCompressionTypeIndex = oodleCompressionTypeValues.IndexOf(dcxKrak.OodleCompressorType);

                            Tools.GhettoInputByte($"DCX_KRAK.{nameof(dcxKrak.CompressionLevel)}", 
                                ref dcxKrakCompressionLevel, 
                                flags: ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.CharsUppercase);

                            ImGui.ListBox($"DCX_KRAK.{nameof(dcxKrak.OodleCompressorType)}", 
                                ref selectedDcxKrakOodleCompressionTypeIndex, oodleCompressionTypeNames, 
                                oodleCompressionTypeNames.Length);
                            RootTaeProperties.BindDcxCompressionInfo = new DCX.DcxKrakCompressionInfo(
                                dcxKrakCompressionLevel, 
                                oodleCompressionTypeValues[selectedDcxKrakOodleCompressionTypeIndex]);
                            break;

                        case DCX.DcxZstdCompressionInfo dcxZstd:
                            byte dcxZstdCompressionLevel = dcxZstd.CompressionLevel;

                            Tools.GhettoInputByte($"DCX_ZSTD.{nameof(dcxZstd.CompressionLevel)}",
                                ref dcxZstdCompressionLevel, 
                                flags: ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.CharsUppercase);

                            RootTaeProperties.BindDcxCompressionInfo = new DCX.DcxZstdCompressionInfo(dcxZstdCompressionLevel);
                            break;
                        case DCX.NoCompressionInfo:
                        case DCX.UnkCompressionInfo:
                        case DCX.ZlibCompressionInfo:
                        case DCX.DcpDfltCompressionInfo:
                        case DCX.DcpEdgeCompressionInfo:
                        case DCX.DcxEdgeCompressionInfo:
                        default:
                            break;
                    }

                    if (ImGui.TreeNode("DCX Type Changer"))
                    {
                        int origDcxTypeIndex = dcxTypeValues.IndexOf(RootTaeProperties.BindDcxCompressionInfo.Type);

                        if (dcxTypeIndex < 0)
                            dcxTypeIndex = dcxTypeValues.IndexOf(RootTaeProperties.BindDcxCompressionInfo.Type);


                        ImGui.ListBox("DCX Type", ref dcxTypeIndex, dcxTypeNames, dcxTypeNames.Length);

                        bool differentTypeSelected = dcxTypeIndex != -1 && dcxTypeIndex != origDcxTypeIndex;


                        if (!differentTypeSelected)
                            ImGuiDebugDrawer.PushDisabled();

                        ImGui.Button("Change DCX Type");
                        if (ImGui.IsItemClicked() && differentTypeSelected)
                        {
                            RootTaeProperties.BindDcxCompressionInfo = DSAProj.GetNewDefaultDcxCompressionInfo(dcxTypeValues[dcxTypeIndex]);
                        }

                        if (!differentTypeSelected)
                            ImGuiDebugDrawer.PopDisabled();

                        ImGui.TreePop();
                    }

                    ImGui.TreePop();
                }







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
