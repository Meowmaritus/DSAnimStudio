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
                    equipSearchStates.Clear();

                    EquipHD = asm.HeadID;
                    EquipBD = asm.BodyID;
                    EquipAM = asm.ArmsID;
                    EquipLG = asm.LegsID;

                    EquipWPR = asm.RightWeaponID;
                    EquipWPL = asm.LeftWeaponID;

                    if (EquipStylesList == null || EquipStylesNamesList == null)
                    {
                        EquipStylesList = new NewChrAsm.WeaponStyleType[]
                        {
                            NewChrAsm.WeaponStyleType.None,
                            NewChrAsm.WeaponStyleType.OneHand,
                            NewChrAsm.WeaponStyleType.TwoHandL,
                            NewChrAsm.WeaponStyleType.TwoHandR,
                            NewChrAsm.WeaponStyleType.OneHandTransformedL,
                            NewChrAsm.WeaponStyleType.OneHandTransformedR,
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

            class EquipSearchState
            {
                public SoulsAssetPipeline.SoulsGames GameItsFor = SoulsAssetPipeline.SoulsGames.None;

                private string prevSearchTerm = "";
                public string SearchTerm = "";

                public EquipSearchState(string[] sourceNames, int[] sourceIDs)
                {
                    GameItsFor = GameDataManager.GameType;

                    SearchSourceNames = sourceNames;
                    SearchSourceIDs = sourceIDs;

                    SearchResultsNames = SearchSourceNames.ToArray();
                    SearchResultsIDs = SearchSourceIDs.ToArray();
                }

                public void CheckSearchTermUpdated(int curSelectedID)
                {
                    if (SearchTerm != prevSearchTerm)
                    {
                        var s = SearchTerm.Trim().ToLower();
                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            
                            var resNames = new List<string>();
                            var resIDs = new List<int>();
                            for (int i = 0; i < SearchSourceNames.Length; i++)
                            {
                                if (SearchSourceNames[i].ToLower().Contains(s) || SearchSourceIDs[i] == curSelectedID)
                                {
                                    resNames.Add(SearchSourceNames[i]);
                                    resIDs.Add(SearchSourceIDs[i]);
                                }
                            }
                            SearchResultsNames = resNames.ToArray();
                            SearchResultsIDs = resIDs.ToArray();
                        }
                        else
                        {
                            SearchResultsNames = SearchSourceNames.ToArray();
                            SearchResultsIDs = SearchSourceIDs.ToArray();
                        }
                        
                    }

                    prevSearchTerm = SearchTerm;
                }

                public string[] SearchSourceNames;
                public int[] SearchSourceIDs;
                public string[] SearchResultsNames;
                public int[] SearchResultsIDs;

                public bool IsDropdownOpen = false;
            }

            Dictionary<string, EquipSearchState> equipSearchStates = new Dictionary<string, EquipSearchState>();
            protected override void BuildContents()
            {
                if (asm == null)
                {
                    ImGui.Text("No player currently loaded.");
                    equipSearchStates.Clear();
                    return;
                }

                bool isChanged = false;

                int EquipListIndexSelect(string disp, int currentSelectedID, string[] sourceNames, int[] sourceIDs)
                {
                    //ImGui.PushID($"EditPlayerEquip_Dropdown_{disp}");

                    if (!equipSearchStates.ContainsKey(disp) || equipSearchStates[disp].GameItsFor != GameDataManager.GameType)
                        equipSearchStates[disp] = new EquipSearchState(sourceNames, sourceIDs);

                    var state = equipSearchStates[disp];
                    
                    
                    int idx = state.SearchResultsIDs.ToList().IndexOf(currentSelectedID);
                    

                    ImGui.SetNextItemOpen(state.IsDropdownOpen);
                    var tree = ImGui.TreeNode($"{disp}: {(idx >= 0 && idx < state.SearchResultsIDs.Length ? state.SearchResultsNames[idx] : "None")}##EditPlayerEquip_Dropdown_{disp}_Tree");
                    if (tree)
                    {
                        state.IsDropdownOpen = true;
                        ImGui.InputText($"##EditPlayerEquip_Dropdown_{disp}_SearchField", ref state.SearchTerm, 256);
                        equipSearchStates[disp].CheckSearchTermUpdated(currentSelectedID);
                        idx = state.SearchResultsIDs.ToList().IndexOf(currentSelectedID);
                        int oldIdx = idx;
                        ImGui.ListBox($"##EditPlayerEquip_Dropdown_{disp}_Dropdown", ref idx, state.SearchResultsNames, state.SearchResultsNames.Length);
                        if (idx != oldIdx)
                        {
                            var newSelectedId = idx >= 0 && idx < state.SearchResultsIDs.Length ? state.SearchResultsIDs[idx] : -1;
                            if (newSelectedId != currentSelectedID)
                            {
                                currentSelectedID = newSelectedId;
                                isChanged = true;
                            }
                        }
                        ImGui.TreePop();
                    }
                    else
                    {
                        state.IsDropdownOpen = false;
                    }

                    

                    //ImGui.PopID();

                    

                    return currentSelectedID;
                }

                

                asm.HeadID = EquipListIndexSelect("Head", asm.HeadID, FmgManager.EquipmentNamesHD, FmgManager.EquipmentIDsHD);
                asm.BodyID = EquipListIndexSelect("Body", asm.BodyID, FmgManager.EquipmentNamesBD, FmgManager.EquipmentIDsBD);
                asm.ArmsID = EquipListIndexSelect("Arms", asm.ArmsID, FmgManager.EquipmentNamesAM, FmgManager.EquipmentIDsAM);
                asm.LegsID = EquipListIndexSelect("Legs", asm.LegsID, FmgManager.EquipmentNamesLG, FmgManager.EquipmentIDsLG);

                ImGui.Separator();

                bool isFemale = asm.IsFemale;
                ImGui.Checkbox("Is Female", ref isFemale);
                if (isFemale != asm.IsFemale)
                {
                    asm.IsFemale = isFemale;
                    asm.UpdateModels(isAsync: true, onCompleteAction: null, updateFaceAndBody: true, forceReloadArmor: true);
                }

                ImGui.Separator();

                asm.RightWeaponID = EquipListIndexSelect("Right Weapon", asm.RightWeaponID, FmgManager.EquipmentNamesWP, FmgManager.EquipmentIDsWP);
                asm.LeftWeaponID = EquipListIndexSelect("Left Weapon", asm.LeftWeaponID, FmgManager.EquipmentNamesWP, FmgManager.EquipmentIDsWP);

                if (isChanged)
                {
                    asm.UpdateModels(isAsync: true, onCompleteAction: null, updateFaceAndBody: true, forceReloadArmor: true);
                }

                ImGui.Separator();

                Tools.FancyComboBox("Weapon Style", ref EquipStyleIndex, EquipStylesNamesList);

                NewChrAsm.WeaponStyleType oldStartWeaponStyle = asm.StartWeaponStyle;

                asm.StartWeaponStyle = (EquipStyleIndex >= 0 && EquipStyleIndex < EquipStylesList.Length) ? 
                    EquipStylesList[EquipStyleIndex] : NewChrAsm.WeaponStyleType.None;

                if (asm.StartWeaponStyle != oldStartWeaponStyle)
                    asm.WeaponStyle = asm.StartWeaponStyle;

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
