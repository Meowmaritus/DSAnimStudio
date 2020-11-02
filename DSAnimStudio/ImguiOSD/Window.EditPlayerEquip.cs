using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class EditPlayerEquip : Window
        {
            public override string Title => "Player Equipment Editor";

            public static bool RequestIndexRefresh = false;

            public int EquipHD = -1;
            public int EquipBD = -1;
            public int EquipAM = -1;
            public int EquipLG = -1;
            public int EquipWPL = -1;
            public int EquipWPR = -1;

            public int EquipStyleIndex = -1;
            private string[] EquipStylesNamesList;
            private NewChrAsm.WeaponStyleType[] EquipStylesList;

            private NewChrAsm asm => Scene.MainModel?.ChrAsm;

            protected override void PreUpdate()
            {
                if (RequestIndexRefresh || IsFirstFrameOpen)
                {
                    EquipHD = FmgManager.EquipmentIDsHD.ToList().IndexOf(asm.HeadID);
                    EquipBD = FmgManager.EquipmentIDsBD.ToList().IndexOf(asm.BodyID);
                    EquipAM = FmgManager.EquipmentIDsAM.ToList().IndexOf(asm.ArmsID);
                    EquipLG = FmgManager.EquipmentIDsLG.ToList().IndexOf(asm.LegsID);

                    EquipWPR = FmgManager.EquipmentIDsWP.ToList().IndexOf(asm.RightWeaponID);
                    EquipWPL = FmgManager.EquipmentIDsWP.ToList().IndexOf(asm.LeftWeaponID);

                    if (EquipStylesList == null || EquipStylesNamesList == null)
                    {
                        EquipStylesList = new NewChrAsm.WeaponStyleType[]
                        {
                            NewChrAsm.WeaponStyleType.None,
                            NewChrAsm.WeaponStyleType.OneHand,
                            NewChrAsm.WeaponStyleType.TwoHandL,
                            NewChrAsm.WeaponStyleType.TwoHandR,
                            NewChrAsm.WeaponStyleType.OneHandTransformedL,
                            NewChrAsm.WeaponStyleType.OneHandTransformedL,
                        };

                        EquipStylesNamesList = new string[]
                        {
                            "0: None",
                            "1: One-Handed",
                            "2: Left Weapon Two-Handed",
                            "3: Right Weapon Two-Handed",
                            "4: One-Handed (Left Weapon Transformed)",
                            "5: One-Handed (Right Weapon Transformed)",
                        };
                    }

                    EquipStyleIndex = EquipStylesList.ToList().IndexOf(asm.StartWeaponStyle);

                    
                }
            }

            protected override void BuildContents()
            {
                if (asm == null)
                {
                    ImGui.Text("No player currently loaded.");
                    return;
                }

                int EquipListIndexSelect(string disp, ref int curIndex, string[] names, int[] ids)
                {
                    int idx = curIndex;

                    Tools.FancyComboBox(disp, ref idx, names);

                    //ImGui.Combo(disp, ref idx, names, names.Length);
                    //ImGui.ListBox("##" + disp, ref idx, names, names.Length);
                    if (curIndex != idx)
                        asm.UpdateModels(isAsync: true, onCompleteAction: null, updateFaceAndBody: false);
                    curIndex = idx;
                    return (idx >= 0 && idx < ids.Length) ? ids[idx] : -1;
                }

                asm.HeadID = EquipListIndexSelect("Head", ref EquipHD, FmgManager.EquipmentNamesHD, FmgManager.EquipmentIDsHD);
                asm.BodyID = EquipListIndexSelect("Body", ref EquipBD, FmgManager.EquipmentNamesBD, FmgManager.EquipmentIDsBD);
                asm.ArmsID = EquipListIndexSelect("Arms", ref EquipAM, FmgManager.EquipmentNamesAM, FmgManager.EquipmentIDsAM);
                asm.LegsID = EquipListIndexSelect("Legs", ref EquipLG, FmgManager.EquipmentNamesLG, FmgManager.EquipmentIDsLG);

                ImGui.Separator();

                asm.RightWeaponID = EquipListIndexSelect("Right Weapon", ref EquipWPR, FmgManager.EquipmentNamesWP, FmgManager.EquipmentIDsWP);
                asm.LeftWeaponID = EquipListIndexSelect("Left Weapon", ref EquipWPL, FmgManager.EquipmentNamesWP, FmgManager.EquipmentIDsWP);

                ImGui.Separator();

                Tools.FancyComboBox("Weapon Style", ref EquipStyleIndex, EquipStylesNamesList);

                asm.StartWeaponStyle = (EquipStyleIndex >= 0 && EquipStyleIndex < EquipStylesList.Length) ? 
                    EquipStylesList[EquipStyleIndex] : NewChrAsm.WeaponStyleType.None;

                ImGui.Separator();

                //bool applyEnabled = !LoadingTaskMan.AnyTasksRunning();

                //if (!applyEnabled)
                //    Tools.PushGrayedOut();

                //ImGui.Button("Apply Equipment to Character");

                //if (ImGui.IsItemClicked() && applyEnabled)
                //    asm.UpdateModels(isAsync: true, onCompleteAction: null, updateFaceAndBody: false);

                //if (!applyEnabled)
                //    Tools.PopGrayedOut();

                //ImGui.Separator();

                ImGui.Checkbox("Right Weapon Model Debug Lineup", ref asm.DebugRightWeaponModelPositions);
                ImGui.Checkbox("Left Weapon Model Debug Lineup", ref asm.DebugLeftWeaponModelPositions);
            }
        }
    }
}
