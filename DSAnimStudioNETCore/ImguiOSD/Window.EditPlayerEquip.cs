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
            public override string ImguiTag => $"{nameof(Window)}.{nameof(EditPlayerEquip)}";

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

            //private int FaceIndex = -1;
            private string[] FaceNames;
            //private int FacegenIndex = -1;
            private string[] FacegenNames;

            private string[] HairNames;

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

                public int ScrollToSelectedLockRemainingFramesLmao = 3;

                public EquipSearchState(string[] sourceNames, int[] sourceIDs)
                {
                    GameItsFor = GameRoot.GameType;

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

                    if (!equipSearchStates.ContainsKey(disp) || equipSearchStates[disp].GameItsFor != GameRoot.GameType)
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
                        //ImGui.ListBox($"##EditPlayerEquip_Dropdown_{disp}_Dropdown", ref idx, state.SearchResultsNames, state.SearchResultsNames.Length);

                        if (ImGui.BeginListBox($"##EditPlayerEquip_Dropdown_{disp}_Dropdown"))
                        {
                            for (int i = 0; i < state.SearchResultsNames.Length; i++)
                            {
                                bool selected = i == idx;
                                bool oldSelected = selected;
                                ImGui.Selectable(state.SearchResultsNames[i], ref selected);
                                if (selected)
                                {
                                    ImGui.SetItemDefaultFocus();

                                    if (equipSearchStates[disp].ScrollToSelectedLockRemainingFramesLmao > 0)
                                    {
                                        ImGui.SetScrollHereY();
                                        equipSearchStates[disp].ScrollToSelectedLockRemainingFramesLmao--;
                                    }

                                    if (selected != oldSelected)
                                    {
                                        idx = i;
                                        
                                    }
                                }
                            }
                            ImGui.EndListBox();
                        }

                        if (idx != oldIdx)
                        {
                            var newSelectedId = idx >= 0 && idx < state.SearchResultsIDs.Length ? state.SearchResultsIDs[idx] : -1;
                            if (newSelectedId != currentSelectedID)
                            {
                                currentSelectedID = newSelectedId;
                                isChanged = true;
                                equipSearchStates[disp].ScrollToSelectedLockRemainingFramesLmao = 3;
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

                //List<string> allEquipNames = new List<string>();

                //foreach (var wpn in FmgManager.WeaponNames)
                //{
                //    allEquipNames.Add($"WP_{ParamManager.EquipParamWeapon[wpn.Key].GetPartBndName().Substring(5)} - {wpn.Key} - {wpn.Value}");
                //}

                //foreach (var wpn in FmgManager.ProtectorNames_HD)
                //{
                //    allEquipNames.Add($"HD_{ParamManager.EquipParamProtector[wpn.Key].GetPartBndName(false).Substring(5)} - {wpn.Key} - {wpn.Value}");
                //}
                //foreach (var wpn in FmgManager.ProtectorNames_HD)
                //{
                //    allEquipNames.Add($"BD_{ParamManager.EquipParamProtector[wpn.Key].GetPartBndName(false).Substring(5)} - {wpn.Key} - {wpn.Value}");
                //}
                //foreach (var wpn in FmgManager.ProtectorNames_LG)
                //{
                //    allEquipNames.Add($"LG_{ParamManager.EquipParamProtector[wpn.Key].GetPartBndName(false).Substring(5)} - {wpn.Key} - {wpn.Value}");
                //}
                //foreach (var wpn in FmgManager.ProtectorNames_AM)
                //{
                //    allEquipNames.Add($"AM_{ParamManager.EquipParamProtector[wpn.Key].GetPartBndName(false).Substring(5)} - {wpn.Key} - {wpn.Value}");
                //}

                //allEquipNames = allEquipNames.OrderBy(xx => xx).ToList();
                //var sb = new StringBuilder();
                //foreach (var en in allEquipNames)
                //{
                //    sb.AppendLine(en);
                //}
                //System.IO.File.WriteAllText(@"E:\DarkSoulsModding\EldenRing\AllParts.txt", sb.ToString());
                ImGui.Separator();

                bool isFemale = asm.IsFemale;
                ImGui.Checkbox("Is Female", ref isFemale);
                if (isFemale != asm.IsFemale)
                {
                    asm.IsFemale = isFemale;
                    asm.UpdateModels(isAsync: true, onCompleteAction: () =>
                    {
                        FlverMaterialDefInfo.FlushBinderCache();
                    }, forceReloadArmor: true);
                }

                ImGui.Separator();

                asm.RightWeaponID = EquipListIndexSelect("Right Weapon", asm.RightWeaponID, FmgManager.EquipmentNamesWP, FmgManager.EquipmentIDsWP);
                asm.LeftWeaponID = EquipListIndexSelect("Left Weapon", asm.LeftWeaponID, FmgManager.EquipmentNamesWP, FmgManager.EquipmentIDsWP);


                var currentFaces = asm.PossibleFaceModels;
                var currentFacegens = asm.PossibleFacegenModels;
                var currentHairs = asm.PossibleHairModels;
                FaceNames = currentFaces.ToArray();
                FacegenNames = currentFacegens.ToArray();
                HairNames = currentHairs.ToArray();
                //FaceIndex = currentFaces.IndexOf(asm.FaceMesh?.Name);
                //FacegenIndex = currentFacegens.IndexOf(asm.FacegenMesh?.Name);
                var faceIndex = asm.FaceIndex;
                var facegenIndex = asm.FacegenIndex;
                var hairIndex = asm.HairIndex;

                var oldFaceIndex = faceIndex;
                if (FaceNames.Length > 0)
                {
                    if (faceIndex < 0)
                        faceIndex = asm.GetDefaultFaceIndexForCurrentGame(asm.IsFemale);
                    ImGui.ListBox($"Naked Body Model##EditPlayerEquip_FaceModel", ref faceIndex, FaceNames, FaceNames.Length);
                    if (oldFaceIndex != faceIndex)
                    {
                        asm.FaceIndex = faceIndex;
                    }
                    
                }

                var oldFacegenIndex = facegenIndex;
                if (FacegenNames.Length > 0)
                {
                    if (facegenIndex < 0)
                        facegenIndex = 0;
                    ImGui.ListBox($"Face Model##EditPlayerEquip_FacegenModel", ref facegenIndex, FacegenNames, FacegenNames.Length);
                    if (oldFacegenIndex != facegenIndex)
                    {
                        asm.FacegenIndex = facegenIndex;
                    }
                    
                }

                var oldHairIndex = hairIndex;
                if (HairNames.Length > 0)
                {
                    if (hairIndex < 0)
                        hairIndex = asm.GetDefaultHairIndexForCurrentGame(asm.IsFemale);
                    ImGui.ListBox($"Hair Model##EditPlayerEquip_HairModel", ref hairIndex, HairNames, HairNames.Length);
                    if (oldHairIndex != hairIndex)
                    {
                        asm.HairIndex = hairIndex;
                    }

                }

                if (isChanged)
                {
                    asm.UpdateModels(isAsync: true, onCompleteAction: () =>
                    {
                        FlverMaterialDefInfo.FlushBinderCache();
                    }, forceReloadArmor: true);
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

                ImGui.Separator();

                var chrCustomize = asm.ChrCustomize.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                foreach (var key in chrCustomize.Keys)
                {
                    if (key == FlverMaterial.ChrCustomizeTypes.None)
                        continue;
                    var color = chrCustomize[key].ToCS();
                    ImGui.ColorEdit4($"{key}###ChrAsm_ChrCustomize_{key}", ref color, ImGuiColorEditFlags.HDR);
                    chrCustomize[key] = color.ToXna();
                }
                asm.ChrCustomize = chrCustomize;
            }
        }
    }
}
