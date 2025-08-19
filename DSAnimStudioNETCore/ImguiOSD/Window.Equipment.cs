using DSAnimStudio.TaeEditor;
using HKX2;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Equipment : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            public override string NewImguiWindowTitle => "Equipment";
            
            protected override void Init()
            {
                
            }
            

            public static bool RequestIndexRefresh = true;

            private float RequestVerticalScroll = -1;

            public int EquipStyle_Index = -1;
            private string[] EquipStyle_Names;
            private NewChrAsm.WeaponStyleType[] EquipStyle_Values;


            public int PartSuffixes_Index = 0;
            private string[] PartSuffixes_Names;
            private ParamData.PartSuffixType[] PartSuffixes_Values;

            private NewChrAsm asm => DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.MainModel?.ChrAsm;
            private AC6NpcPartsEquipper AC6NpcParts => DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.MainModel?.AC6NpcParts;

            private string[] FaceEquip_Names;
            private string[] FacegenEquip_Names;
            private string[] HairEquip_Names;
            private string[] DebugEquip_Names;

            private int[] FaceEquip_FakeIDs;
            private int[] FacegenEquip_FakeIDs;
            private int[] HairEquip_FakeIDs;
            private int[] DebugEquip_FakeIDs;

            private List<NewEquipSlot_Armor.DirectEquipInfo> FaceEquip_Values = new List<NewEquipSlot_Armor.DirectEquipInfo>();
            private List<NewEquipSlot_Armor.DirectEquipInfo> FacegenEquip_Values = new List<NewEquipSlot_Armor.DirectEquipInfo>();
            private List<NewEquipSlot_Armor.DirectEquipInfo> HairEquip_Values = new List<NewEquipSlot_Armor.DirectEquipInfo>();
            private List<NewEquipSlot_Armor.DirectEquipInfo> DebugEquip_Values = new List<NewEquipSlot_Armor.DirectEquipInfo>();

            private List<ParamData.AC6NpcEquipPartsParam> AC6NpcEquipParts_Values =
                new List<ParamData.AC6NpcEquipPartsParam>();

            private int[] AC6NpcEquipParts_IDs;
            private string[] AC6NpcEquipParts_Names;

            private List<NewEquipSlot_Armor.DirectEquipInfo> SearchDirectEquipInfos(NewEquipSlot_Armor.DirectEquipPartPrefix prefix, bool gameUsesMaleForBoth)
            {
                var meme = new Dictionary<int, (bool A, bool M, bool F)>();

                var prefixLower = prefix.ToString().ToLower();

                var fileNames_a = zzz_DocumentManager.CurrentDocument.GameData.SearchFiles("/parts/", @$"\/parts\/{prefixLower}_a_\d\d\d\d\.partsbnd\.dcx");
                var fileNames_m = zzz_DocumentManager.CurrentDocument.GameData.SearchFiles("/parts/", @$"\/parts\/{prefixLower}_m_\d\d\d\d\.partsbnd\.dcx");
                var fileNames_f = zzz_DocumentManager.CurrentDocument.GameData.SearchFiles("/parts/", @$"\/parts\/{prefixLower}_f_\d\d\d\d\.partsbnd\.dcx");

                for (int i = 0; i < fileNames_a.Count; i++)
                {
                    if (int.TryParse(fileNames_a[i].Substring(12, 4), out int parsedInt))
                    {
                        if (!meme.ContainsKey(parsedInt))
                            meme[parsedInt] = (A: false, M: false, F: false);

                        var tuple = meme[parsedInt];
                        tuple.A = true;
                        meme[parsedInt] = tuple;
                    }
                }

                for (int i = 0; i < fileNames_m.Count; i++)
                {
                    if (int.TryParse(fileNames_m[i].Substring(12, 4), out int parsedInt))
                    {
                        if (!meme.ContainsKey(parsedInt))
                            meme[parsedInt] = (A: false, M: false, F: false);

                        var tuple = meme[parsedInt];
                        tuple.M = true;
                        meme[parsedInt] = tuple;
                    }
                }

                for (int i = 0; i < fileNames_f.Count; i++)
                {
                    if (int.TryParse(fileNames_f[i].Substring(12, 4), out int parsedInt))
                    {
                        if (!meme.ContainsKey(parsedInt))
                            meme[parsedInt] = (A: false, M: false, F: false);

                        var tuple = meme[parsedInt];
                        tuple.F = true;
                        meme[parsedInt] = tuple;
                    }
                }

                var result = new List<NewEquipSlot_Armor.DirectEquipInfo>();
                foreach (var kvp in meme)
                {
                    var equip = new NewEquipSlot_Armor.DirectEquipInfo();
                    equip.PartPrefix = prefix;
                    equip.ModelID = kvp.Key;
                    if (kvp.Value.A)
                        equip.Gender = NewEquipSlot_Armor.DirectEquipGender.UnisexUseA;
                    else if (kvp.Value.M && !kvp.Value.F)
                        equip.Gender = gameUsesMaleForBoth ? NewEquipSlot_Armor.DirectEquipGender.UnisexUseMForBoth : NewEquipSlot_Armor.DirectEquipGender.MaleOnlyUseM;
                    else if (!kvp.Value.M && kvp.Value.F)
                        equip.Gender = NewEquipSlot_Armor.DirectEquipGender.FemaleOnlyUseF;
                    else if (kvp.Value.M && kvp.Value.F)
                        equip.Gender = NewEquipSlot_Armor.DirectEquipGender.BothGendersUseMF;
                    result.Add(equip);
                }

                

                return result;
            }

            private string GetStringForEquipInfo(NewEquipSlot_Armor.DirectEquipInfo equipInfo)
            {
                var shortPartType = equipInfo.PartPrefix.ToString();
                if (equipInfo.Gender == NewEquipSlot_Armor.DirectEquipGender.UnisexUseA)
                    return $"{shortPartType}_A_{equipInfo.ModelID:D4}";
                else if (equipInfo.Gender is NewEquipSlot_Armor.DirectEquipGender.UnisexUseMForBoth or NewEquipSlot_Armor.DirectEquipGender.MaleOnlyUseM)
                    return $"{shortPartType}_M_{equipInfo.ModelID:D4}";
                else if (equipInfo.Gender is NewEquipSlot_Armor.DirectEquipGender.FemaleOnlyUseF)
                    return $"{shortPartType}_F_{equipInfo.ModelID:D4}";
                else if (equipInfo.Gender is NewEquipSlot_Armor.DirectEquipGender.BothGendersUseMF)
                    return $"{shortPartType}_F_{equipInfo.ModelID:D4} | {shortPartType}_M_{equipInfo.ModelID:D4}";

                return $"Gender: {equipInfo.Gender}, Model ID: {equipInfo.ModelID}";
            }

            private void SearchDirectEquips()
            {
                FaceEquip_Values = SearchDirectEquipInfos(NewEquipSlot_Armor.DirectEquipPartPrefix.FC, zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesMaleForAllProtectorParts);
                HairEquip_Values = SearchDirectEquipInfos(NewEquipSlot_Armor.DirectEquipPartPrefix.HR, zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesMaleForAllProtectorParts);
                FacegenEquip_Values = SearchDirectEquipInfos(NewEquipSlot_Armor.DirectEquipPartPrefix.FG, zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesMaleForAllProtectorParts);
                DebugEquip_Values = new();

                if (Main.IsDebugBuild)
                {
                    var prefixes = (NewEquipSlot_Armor.DirectEquipPartPrefix[])System.Enum.GetValues(typeof(NewEquipSlot_Armor.DirectEquipPartPrefix));
                    foreach (var p in prefixes)
                    {
                        if (p != NewEquipSlot_Armor.DirectEquipPartPrefix.None)
                        {
                            var partsForThisPrefix = SearchDirectEquipInfos(p, zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesMaleForAllProtectorParts);
                            DebugEquip_Values.AddRange(partsForThisPrefix);
                        }
                    }
                    var debugEquip_Names = DebugEquip_Values.Select(e => GetStringForEquipInfo(e)).ToList();
                    DebugEquip_Values.Insert(0, new NewEquipSlot_Armor.DirectEquipInfo());
                    debugEquip_Names.Insert(0, "None");
                    DebugEquip_Names = debugEquip_Names.ToArray();
                    DebugEquip_FakeIDs = DebugEquip_Values.Select((v, i) => i).ToArray();
                }
                else
                {
                    DebugEquip_Values = new();
                    DebugEquip_Names = new string[0];
                    DebugEquip_FakeIDs = new int[0];
                }

                var faceEquip_Names = FaceEquip_Values.Select(e => GetStringForEquipInfo(e)).ToList();
                var hairEquip_Names = HairEquip_Values.Select(e => GetStringForEquipInfo(e)).ToList();
                var facegenEquip_Names = FacegenEquip_Values.Select(e => GetStringForEquipInfo(e)).ToList();


                // Insert the None values/names
                FaceEquip_Values.Insert(0, new NewEquipSlot_Armor.DirectEquipInfo());
                HairEquip_Values.Insert(0, new NewEquipSlot_Armor.DirectEquipInfo());
                FacegenEquip_Values.Insert(0, new NewEquipSlot_Armor.DirectEquipInfo());

                faceEquip_Names.Insert(0, "None");
                hairEquip_Names.Insert(0, "None");
                facegenEquip_Names.Insert(0, "None");

                FaceEquip_Names = faceEquip_Names.ToArray();
                HairEquip_Names = hairEquip_Names.ToArray();
                FacegenEquip_Names = facegenEquip_Names.ToArray();

                FaceEquip_FakeIDs = FaceEquip_Values.Select((v, i) => i).ToArray();
                HairEquip_FakeIDs = HairEquip_Values.Select((v, i) => i).ToArray();
                FacegenEquip_FakeIDs = FacegenEquip_Values.Select((v, i) => i).ToArray();
            }

            

            protected override void PreUpdate()
            {
                if (MainProj == null)
                    IsOpen = false;
            }

            class EquipSearchState
            {
                public SoulsAssetPipeline.SoulsGames GameItsFor = SoulsAssetPipeline.SoulsGames.None;

                private string prevSearchTerm = "";
                public string SearchTerm = "";

                public int ScrollToSelectedLockRemainingFramesLmao = 3;

                public EquipSearchState(string[] sourceNames, int[] sourceIDs)
                {
                    GameItsFor = zzz_DocumentManager.CurrentDocument.GameRoot.GameType;

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
            protected override void BuildContents(ref bool anyFieldFocusedRef)
            {
                if (MainDoc == null || MainDoc.GameRoot.GameType is SoulsGames.None)
                    return;

                var fmgMan = MainDoc.FmgManager;

                bool oldIncludeAllIDs = fmgMan.IncludeAllIDs;
                ImGui.Checkbox("Show All Equipment Entries", ref fmgMan.IncludeAllIDs);
                if (fmgMan.IncludeAllIDs != oldIncludeAllIDs)
                {
                    fmgMan.LoadAllFMG(true);
                    RequestIndexRefresh = true;
                }

                var entityTypeCheck = MainTaeScreen?.Graph?.ViewportInteractor?.EntityType;
                var validEntity = entityTypeCheck == TaeViewportInteractor.TaeEntityType.PC ||
                                  (entityTypeCheck == TaeViewportInteractor.TaeEntityType.NPC &&
                                   (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.AC6));
                if (!validEntity)
                    return;
                
                if (IsFirstFrameOpen)
                    RequestIndexRefresh = true;

                bool disabledThisFrame = zzz_DocumentManager.CurrentDocument.LoadingTaskMan.AnyInteractionBlockingTasks();
                
                if (disabledThisFrame)
                    ImGui.BeginDisabled();

                float verticalScroll = ImGui.GetScrollY();
                
                
                bool anyFieldFocused = false;
                
                
                // if (asm == null)
                // {
                //     ImGui.Text("No player currently loaded.");
                //     equipSearchStates.Clear();
                //     return;
                // }
                
                if (RequestIndexRefresh || IsFirstFrameOpen)
                {
                    if (asm == null && zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.AC6)
                    {
                        equipSearchStates.Clear();
                        
                        AC6NpcEquipParts_Values = zzz_DocumentManager.CurrentDocument.ParamManager.AC6NpcEquipPartsParam.Values.OrderBy(x => x.ID).ToList();
                        AC6NpcEquipParts_IDs = AC6NpcEquipParts_Values.Select(x => x.ID).ToArray();
                        AC6NpcEquipParts_Names = AC6NpcEquipParts_Values.Select(x => $"{x.ID} {(x.Name ?? "")}").ToArray();
                    }
                    else
                    {

                        equipSearchStates.Clear();

                        if (EquipStyle_Values == null || EquipStyle_Names == null)
                        {
                            EquipStyle_Values = new NewChrAsm.WeaponStyleType[]
                            {
                                NewChrAsm.WeaponStyleType.None,
                                NewChrAsm.WeaponStyleType.OneHand,
                                NewChrAsm.WeaponStyleType.LeftBoth,
                                NewChrAsm.WeaponStyleType.RightBoth,
                                NewChrAsm.WeaponStyleType.OneHandTransformedL,
                                NewChrAsm.WeaponStyleType.OneHandTransformedR,
                            };

                            EquipStyle_Names = new string[]
                            {
                                "0: None",
                                "1: One-Handed",
                                "2: Left Weapon Two-Handed",
                                "3: Right Weapon Two-Handed",
                                "4: One-Handed (Left Weapon Transformed)",
                                "5: One-Handed (Right Weapon Transformed)",
                            };
                        }

                        if (PartSuffixes_Values == null || PartSuffixes_Names == null)
                        {
                            PartSuffixes_Values = new ParamData.PartSuffixType[]
                            {
                                ParamData.PartSuffixType.None,
                                ParamData.PartSuffixType.L,
                                ParamData.PartSuffixType.U,
                                ParamData.PartSuffixType.M,
                            };

                            PartSuffixes_Names = new string[]
                            {
                                "None",
                                "_L: Low-quality version",
                                "_U: Ultra-quality version (AC6 only)",
                                "_M: Hollowed skin version (DS1 only)",
                            };
                        }

                        EquipStyle_Index = EquipStyle_Values.ToList().IndexOf(asm.StartWeaponStyle);

                        PartSuffixes_Index = PartSuffixes_Values.ToList().IndexOf(asm.CurrentPartSuffixType);

                        SearchDirectEquips();
                    }
                }

                RequestIndexRefresh = false;

                bool isChanged = false;
                bool isChangedGenderOrSuffix = false;
                bool isForceUseNoCache = false;

                int EquipListIndexSelect(string disp, int currentSelectedID, string[] sourceNames, int[] sourceIDs, ref bool anyfieldfocused)
                {
                    //ImGui.PushID($"EditPlayerEquip_Dropdown_{disp}");
                    if (sourceNames == null || sourceIDs == null)
                        return currentSelectedID;
                    
                    
                    
                    if (!equipSearchStates.ContainsKey(disp) || equipSearchStates[disp].GameItsFor != zzz_DocumentManager.CurrentDocument.GameRoot.GameType)
                        equipSearchStates[disp] = new EquipSearchState(sourceNames, sourceIDs);

                    var state = equipSearchStates[disp];
                    
                    
                    int idx = state.SearchResultsIDs.ToList().IndexOf(currentSelectedID);

                    ImGui.SetNextItemOpen(state.IsDropdownOpen);
                    var tree = ImGui.TreeNode($"{disp}: {(idx >= 0 && idx < state.SearchResultsIDs.Length ? state.SearchResultsNames[idx] : "None")}##EditPlayerEquip_Dropdown_{disp}_Tree");
                    if (tree)
                    {
                        state.IsDropdownOpen = true;
                        ImGui.InputText($"##EditPlayerEquip_Dropdown_{disp}_SearchField", ref state.SearchTerm, 256);
                        anyfieldfocused |= ImGui.IsItemActive();
                        equipSearchStates[disp].CheckSearchTermUpdated(currentSelectedID);
                        idx = state.SearchResultsIDs.ToList().IndexOf(currentSelectedID);
                        int oldIdx = idx;
                        //ImGui.ListBox($"##EditPlayerEquip_Dropdown_{disp}_Dropdown", ref idx, state.SearchResultsNames, state.SearchResultsNames.Length);
                        ImGui.PushItemWidth(OSD.DefaultItemWidth * 3);
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
                        ImGui.PopItemWidth();

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

                NewEquipSlot_Armor.DirectEquipInfo DirectEquipListSelect(string slotName, NewEquipSlot_Armor.DirectEquipInfo equipInfo,
                List<NewEquipSlot_Armor.DirectEquipInfo> values, int[] fakeIDs, string[] names)
                {
                    int index = values.IndexOf(equipInfo);
                    int startIndex = index;
                    // Fake IDs are just the value list index in int form, so [0, 1, 2, 3, 4, 5, ...] etc
                    index = EquipListIndexSelect(slotName, index, names, fakeIDs, ref anyFieldFocused);
                    if (startIndex != index)
                    {
                        if (index >= 0 && index < values.Count)
                            equipInfo = values[index];
                        else
                            equipInfo = values[0];
                    }
                    return equipInfo;
                }

                if (asm == null && zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.AC6 && AC6NpcParts != null)
                {
                    ImGui.Checkbox(
                        "Use Parts File Caching\n(Disable if you want to see parts file changes reflected in editor)",
                        ref AC6NpcParts.EnablePartsFileCaching);

                    int oldEquip = AC6NpcParts.EquipID;
                    AC6NpcParts.EquipID = EquipListIndexSelect("NPC Parts", AC6NpcParts.EquipID,
                        AC6NpcEquipParts_Names, AC6NpcEquipParts_IDs, ref anyFieldFocused);
                    
                    if (oldEquip != AC6NpcParts.EquipID)
                        AC6NpcParts.UpdateModels();
                }
                else if (asm != null)
                {


                    //ImGui.Text("Debug - Reinit ModelSkeletonRemapper");

                    //if (Tools.SimpleClickButton("HD"))
                    //    asm.CreateSkelRemappersForArmorModel(NewChrAsm.EquipSlot.Head);
                    //ImGui.SameLine();
                    //if (Tools.SimpleClickButton("BD"))
                    //    asm.CreateSkelRemappersForArmorModel(NewChrAsm.EquipSlot.Body);
                    //ImGui.SameLine();
                    //if (Tools.SimpleClickButton("AM"))
                    //    asm.CreateSkelRemappersForArmorModel(NewChrAsm.EquipSlot.Arms);
                    //ImGui.SameLine();
                    //if (Tools.SimpleClickButton("LG"))
                    //    asm.CreateSkelRemappersForArmorModel(NewChrAsm.EquipSlot.Legs);
                    //ImGui.SameLine();
                    //if (Tools.SimpleClickButton("FC"))
                    //    asm.CreateSkelRemappersForArmorModel(NewChrAsm.EquipSlot.Face);
                    //ImGui.SameLine();
                    //if (Tools.SimpleClickButton("FG"))
                    //    asm.CreateSkelRemappersForArmorModel(NewChrAsm.EquipSlot.Facegen);
                    //ImGui.SameLine();
                    //if (Tools.SimpleClickButton("HR"))
                    //    asm.CreateSkelRemappersForArmorModel(NewChrAsm.EquipSlot.Hair);


                    ImGui.Checkbox(
                        "Use Parts File Caching\n(Disable if you want to see parts file changes reflected in editor)",
                        ref asm.EnablePartsFileCaching);

                    if (Tools.SimpleClickButton(
                            "Force Reload All Parts\n(Ignores cache option, always reloading the files. Can be slow.)"))
                    {
                        isChanged = true;
                        isChangedGenderOrSuffix = true;
                        isForceUseNoCache = true;
                    }

                    ImGui.Separator();


                    if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                    {
                        ImGui.Checkbox("Weapon Bay - Left Weapon Swapped", ref asm.AC6LeftWeaponBaySwapped);
                        ImGui.Checkbox("Weapon Bay - Right Weapon Swapped", ref asm.AC6RightWeaponBaySwapped);
                    }
                    else
                    {

                        Tools.FancyComboBox("Weapon Style", ref EquipStyle_Index, EquipStyle_Names);

                        NewChrAsm.WeaponStyleType oldStartWeaponStyle = asm.StartWeaponStyle;

                        asm.StartWeaponStyle = (EquipStyle_Index >= 0 && EquipStyle_Index < EquipStyle_Values.Length)
                            ? EquipStyle_Values[EquipStyle_Index]
                            : NewChrAsm.WeaponStyleType.None;

                        if (asm.StartWeaponStyle != oldStartWeaponStyle)
                        {
                            asm.WeaponStyle = asm.StartWeaponStyle;
                            asm.UpdateMasks();
                            asm.Update(0, forceSyncUpdate: true);
                        }


                    }

                    ImGui.Separator();

                    if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is not SoulsAssetPipeline.SoulsGames.AC6)
                    {

                        ImGui.Separator();

                        bool isFemale = asm.IsFemale;
                        ImGui.Checkbox("Is Female", ref isFemale);
                        if (isFemale != asm.IsFemale)
                        {
                            asm.IsFemale = isFemale;
                            isChangedGenderOrSuffix = true;
                        }

                        ImGui.Separator();
                    }



                    int oldPartsSuffixIndex = PartSuffixes_Index;
                    Tools.FancyComboBox("Parts File Suffix", ref PartSuffixes_Index, PartSuffixes_Names);

                    if (PartSuffixes_Index < 0 || PartSuffixes_Index > PartSuffixes_Values.Length)
                        PartSuffixes_Index = 0;

                    if (oldPartsSuffixIndex != PartSuffixes_Index)
                    {
                        asm.CurrentPartSuffixType = PartSuffixes_Values[PartSuffixes_Index];
                        isChangedGenderOrSuffix = true;
                    }

                    ImGui.Separator();

                    if (Tools.SimpleClickButton("Debug: Regen Skeleton Remappers And Gluers"))
                    {
                        asm.RegenSkelRemappersAndGluers();
                    }

                    //ImGui.Separator();

                    asm.ForAllArmorSlots(slot =>
                    {
                        if (slot.EquipSlotType == NewChrAsm.EquipSlotTypes.Head)
                        {
                            slot.EquipID = EquipListIndexSelect(slot.SlotDisplayName, slot.EquipID, fmgMan.EquipmentNamesHD,
                                fmgMan.EquipmentIDsHD, ref anyFieldFocused);
                        }
                        else if (slot.EquipSlotType == NewChrAsm.EquipSlotTypes.Body)
                        {
                            slot.EquipID = EquipListIndexSelect(slot.SlotDisplayName, slot.EquipID, fmgMan.EquipmentNamesBD,
                                fmgMan.EquipmentIDsBD, ref anyFieldFocused);
                        }
                        else if (slot.EquipSlotType == NewChrAsm.EquipSlotTypes.Arms)
                        {
                            slot.EquipID = EquipListIndexSelect(slot.SlotDisplayName, slot.EquipID, fmgMan.EquipmentNamesAM,
                                fmgMan.EquipmentIDsAM, ref anyFieldFocused);
                        }
                        else if (slot.EquipSlotType == NewChrAsm.EquipSlotTypes.Legs)
                        {
                            slot.EquipID = EquipListIndexSelect(slot.SlotDisplayName, slot.EquipID, fmgMan.EquipmentNamesLG,
                                fmgMan.EquipmentIDsLG, ref anyFieldFocused);
                        }
                        else if (slot.EquipSlotType == NewChrAsm.EquipSlotTypes.Face)
                        {
                            slot.DirectEquip = DirectEquipListSelect(slot.SlotDisplayName, slot.DirectEquip, FaceEquip_Values,
                                FaceEquip_FakeIDs, FaceEquip_Names);
                        }
                        else if (slot.EquipSlotType == NewChrAsm.EquipSlotTypes.Hair)
                        {
                            slot.DirectEquip = DirectEquipListSelect(slot.SlotDisplayName, slot.DirectEquip, HairEquip_Values,
                                HairEquip_FakeIDs, HairEquip_Names);
                        }
                        else if (!zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesOldShittyFacegen &&
                                 slot.EquipSlotType >= NewChrAsm.EquipSlotTypes.Facegen1 &&
                                 slot.EquipSlotType <= NewChrAsm.EquipSlotTypes.FacegenMax)
                        {
                            slot.DirectEquip = DirectEquipListSelect(slot.SlotDisplayName, slot.DirectEquip,
                                FacegenEquip_Values, FacegenEquip_FakeIDs, FacegenEquip_Names);
                        }

                        else if (Main.IsDebugBuild && slot.EquipSlotType >= NewChrAsm.EquipSlotTypes.Debug1)
                        {
                            slot.DirectEquip = DirectEquipListSelect(slot.SlotDisplayName, slot.DirectEquip,
                                DebugEquip_Values, DebugEquip_FakeIDs, DebugEquip_Names);
                        }
                    });

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

                    foreach (var slot in asm.WeaponSlots)
                    {
                        if (slot.EquipSlotType == NewChrAsm.EquipSlotTypes.LeftWeapon)
                            slot.EquipID = EquipListIndexSelect(slot.SlotDisplayName, slot.EquipID,
                                fmgMan.EquipmentNamesWP_L, fmgMan.EquipmentIDsWP_L, ref anyFieldFocused);

                        else if (slot.EquipSlotType == NewChrAsm.EquipSlotTypes.RightWeapon)
                            slot.EquipID = EquipListIndexSelect(slot.SlotDisplayName, slot.EquipID,
                                fmgMan.EquipmentNamesWP_R, fmgMan.EquipmentIDsWP_R, ref anyFieldFocused);

                        else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 &&
                                 slot.EquipSlotType == NewChrAsm.EquipSlotTypes.AC6BackLeftWeapon)
                            slot.EquipID = EquipListIndexSelect(slot.SlotDisplayName, slot.EquipID,
                                fmgMan.EquipmentNamesWP_BL, fmgMan.EquipmentIDsWP_BL, ref anyFieldFocused);

                        if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 &&
                            slot.EquipSlotType == NewChrAsm.EquipSlotTypes.AC6BackRightWeapon)
                            slot.EquipID = EquipListIndexSelect(slot.SlotDisplayName, slot.EquipID,
                                fmgMan.EquipmentNamesWP_BR, fmgMan.EquipmentIDsWP_BR, ref anyFieldFocused);




                        else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 &&
                                 slot.EquipSlotType == NewChrAsm.EquipSlotTypes.AC6BackLeftWeaponRail)
                            slot.EquipID = EquipListIndexSelect(slot.SlotDisplayName, slot.EquipID,
                                fmgMan.EquipmentNamesWP_BLWR, fmgMan.EquipmentIDsWP_BLWR, ref anyFieldFocused);

                        if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 &&
                            slot.EquipSlotType == NewChrAsm.EquipSlotTypes.AC6BackRightWeaponRail)
                            slot.EquipID = EquipListIndexSelect(slot.SlotDisplayName, slot.EquipID,
                                fmgMan.EquipmentNamesWP_BRWR, fmgMan.EquipmentIDsWP_BRWR, ref anyFieldFocused);
                        
                        else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.SDT &&
                                 slot.EquipSlotType == NewChrAsm.EquipSlotTypes.SekiroMortalBlade)
                        {
                            bool equipped = slot.EquipID == 9700000;
                            bool prevEquipped = equipped;
                            ImGui.Checkbox("Mortal Blade Equipped", ref equipped);
                            if (equipped != prevEquipped)
                            {
                                slot.EquipID = equipped ? 9700000 : -1;
                                isChanged = true;
                            }
                        }
                        
                        else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.SDT &&
                                 slot.EquipSlotType == NewChrAsm.EquipSlotTypes.SekiroGrapplingHook)
                        {
                            bool equipped = slot.EquipID == 9500000;
                            bool prevEquipped = equipped;
                            ImGui.Checkbox("Grappling Hook Equipped", ref equipped);
                            if (equipped != prevEquipped)
                            {
                                slot.EquipID = equipped ? 9500000 : -1;
                                isChanged = true;
                            }
                        }


                        var modelDebugLineupPre = slot.DebugWeaponModelPositions;
                        ImGui.Checkbox($"{slot.SlotDisplayName} Model Debug Lineup",
                            ref slot.DebugWeaponModelPositions);
                        if (modelDebugLineupPre != slot.DebugWeaponModelPositions)
                        {
                            asm.Update(0, forceSyncUpdate: true);
                        }

                        ImGui.Separator();
                    }


                    if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is not SoulsAssetPipeline.SoulsGames.AC6)
                    {

                        //var currentFaces = asm.PossibleFaceModels;
                        //var currentFacegens = asm.PossibleFacegenModels;
                        //var currentHairs = asm.PossibleHairModels;
                        //FaceNames = currentFaces.ToArray();
                        //FacegenNames = currentFacegens.ToArray();
                        //HairNames = currentHairs.ToArray();
                        ////FaceIndex = currentFaces.IndexOf(asm.FaceMesh?.Name);
                        ////FacegenIndex = currentFacegens.IndexOf(asm.FacegenMesh?.Name);
                        //var faceIndex = asm.FaceIndex;
                        //var facegenIndex = asm.FacegenIndex;
                        //var hairIndex = asm.HairIndex;

                        //var oldFaceIndex = faceIndex;
                        //if (FaceNames.Length > 0)
                        //{
                        //    if (faceIndex < 0)
                        //        faceIndex = asm.GetDefaultFaceIndexForCurrentGame(asm.IsFemale);
                        //    ImGui.ListBox($"Naked Body Model##EditPlayerEquip_FaceModel", ref faceIndex, FaceNames, FaceNames.Length);
                        //    if (oldFaceIndex != faceIndex)
                        //    {
                        //        asm.FaceIndex = faceIndex;
                        //    }

                        //}

                        //var oldFacegenIndex = facegenIndex;
                        //if (FacegenNames.Length > 0)
                        //{
                        //    if (facegenIndex < 0)
                        //        facegenIndex = 0;
                        //    ImGui.ListBox($"Face Model##EditPlayerEquip_FacegenModel", ref facegenIndex, FacegenNames, FacegenNames.Length);
                        //    if (oldFacegenIndex != facegenIndex)
                        //    {
                        //        asm.FacegenIndex = facegenIndex;
                        //    }

                        //}

                        //var oldHairIndex = hairIndex;
                        //if (HairNames.Length > 0)
                        //{
                        //    if (hairIndex < 0)
                        //        hairIndex = asm.GetDefaultHairIndexForCurrentGame(asm.IsFemale);
                        //    ImGui.ListBox($"Hair Model##EditPlayerEquip_HairModel", ref hairIndex, HairNames, HairNames.Length);
                        //    if (oldHairIndex != hairIndex)
                        //    {
                        //        asm.HairIndex = hairIndex;
                        //    }

                        //}
                    }

                    if (isChanged || isChangedGenderOrSuffix)
                    {
                        float verticalScrollCapture = verticalScroll;
                        asm.UpdateModels(isAsync: true,
                            onCompleteAction: () =>
                            {
                                FlverMaterialDefInfo.FlushBinderCache();
                                //RequestVerticalScroll = verticalScrollCapture;
                            },
                            forceReloadUnchanged: isChangedGenderOrSuffix,
                            disableCache: !asm.EnablePartsFileCaching || isForceUseNoCache);
                        
                    }

                    ImGui.Separator();




                    if (ImGui.TreeNode("[WEAPON SLOT DEBUG]"))
                    {
                        asm.ForAllWeaponSlots(slot =>
                        {
                            if (ImGui.TreeNode($"{slot.SlotDisplayName}"))
                            {
                                ImGui.Text($"{nameof(slot.Absorp)}.{nameof(slot.Absorp.Model0DummyPoly)} = {slot.Absorp.Model0DummyPoly}");
                                ImGui.Text($"{nameof(slot.Absorp)}.{nameof(slot.Absorp.Model1DummyPoly)} = {slot.Absorp.Model1DummyPoly}");
                                ImGui.Text($"{nameof(slot.Absorp)}.{nameof(slot.Absorp.Model2DummyPoly)} = {slot.Absorp.Model2DummyPoly}");
                                ImGui.Text($"{nameof(slot.Absorp)}.{nameof(slot.Absorp.Model3DummyPoly)} = {slot.Absorp.Model3DummyPoly}");
                                ImGui.Text($"{nameof(slot.Absorp)}.{nameof(slot.Absorp.Model0DispType)} = {slot.Absorp.Model0DispType}");
                                ImGui.Text($"{nameof(slot.Absorp)}.{nameof(slot.Absorp.Model1DispType)} = {slot.Absorp.Model1DispType}");
                                ImGui.Text($"{nameof(slot.Absorp)}.{nameof(slot.Absorp.Model2DispType)} = {slot.Absorp.Model2DispType}");
                                ImGui.Text($"{nameof(slot.Absorp)}.{nameof(slot.Absorp.Model3DispType)} = {slot.Absorp.Model3DispType}");
                                
                                ImGui.Text($"{nameof(slot.FinalAbsorpModelDmyPoly0)} = {slot.FinalAbsorpModelDmyPoly0}");
                                ImGui.Text($"{nameof(slot.FinalAbsorpModelDmyPoly1)} = {slot.FinalAbsorpModelDmyPoly1}");
                                ImGui.Text($"{nameof(slot.FinalAbsorpModelDmyPoly2)} = {slot.FinalAbsorpModelDmyPoly2}");
                                ImGui.Text($"{nameof(slot.FinalAbsorpModelDmyPoly3)} = {slot.FinalAbsorpModelDmyPoly3}");
                                
                                ImGui.Text($"{nameof(slot.DmySource0)} = {slot.DmySource0}");
                                ImGui.Text($"{nameof(slot.DmySource1)} = {slot.DmySource1}");
                                ImGui.Text($"{nameof(slot.DmySource2)} = {slot.DmySource2}");
                                ImGui.Text($"{nameof(slot.DmySource3)} = {slot.DmySource3}");
                                
                                ImGui.Text($"{nameof(slot.Model0MemeAbsorp)} = {slot.Model0MemeAbsorp}");
                                ImGui.Text($"{nameof(slot.Model1MemeAbsorp)} = {slot.Model1MemeAbsorp}");
                                ImGui.Text($"{nameof(slot.Model2MemeAbsorp)} = {slot.Model2MemeAbsorp}");
                                ImGui.Text($"{nameof(slot.Model3MemeAbsorp)} = {slot.Model3MemeAbsorp}");
                                
                                ImGui.Text($"{nameof(slot.SekiroProstheticOverrideTaeActive)} = {slot.SekiroProstheticOverrideTaeActive}");
                                ImGui.Text($"{nameof(slot.SekiroProstheticOverrideDmy0)} = {slot.SekiroProstheticOverrideDmy0}");
                                ImGui.Text($"{nameof(slot.SekiroProstheticOverrideDmy1)} = {slot.SekiroProstheticOverrideDmy1}");
                                ImGui.Text($"{nameof(slot.SekiroProstheticOverrideDmy2)} = {slot.SekiroProstheticOverrideDmy2}");
                                ImGui.Text($"{nameof(slot.SekiroProstheticOverrideDmy3)} = {slot.SekiroProstheticOverrideDmy3}");


                                if (slot.EquipParam != null)
                                {
                                    if (ImGui.TreeNode("EquipParam"))
                                    {
                                        ImGui.Text($"{nameof(slot.EquipParam.BehaviorVariationID)} = {slot.EquipParam.BehaviorVariationID}");
                                        ImGui.Text($"{nameof(slot.EquipParam.EquipModelID)} = {slot.EquipParam.EquipModelID}");
                                        ImGui.Text($"{nameof(slot.EquipParam.WepMotionCategory)} = {slot.EquipParam.WepMotionCategory}");
                                        ImGui.Text($"{nameof(slot.EquipParam.SpAtkCategory)} = {slot.EquipParam.SpAtkCategory}");
                                        ImGui.Text($"{nameof(slot.EquipParam.WepAbsorpPosID)} = {slot.EquipParam.WepAbsorpPosID}");
                                        
                                        ImGui.Text($"{nameof(slot.EquipParam.IsLeftHandEquippable)} = {slot.EquipParam.IsLeftHandEquippable}");
                                        ImGui.Text($"{nameof(slot.EquipParam.IsRightHandEquippable)} = {slot.EquipParam.IsRightHandEquippable}");
                                        ImGui.Text($"{nameof(slot.EquipParam.IsBothHandEquippable)} = {slot.EquipParam.IsBothHandEquippable}");
                                        ImGui.Text($"{nameof(slot.EquipParam.AC6IsBackLeftSlotEquippable)} = {slot.EquipParam.AC6IsBackLeftSlotEquippable}");
                                        ImGui.Text($"{nameof(slot.EquipParam.AC6IsBackRightSlotEquippable)} = {slot.EquipParam.AC6IsBackRightSlotEquippable}");
                                        
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model0_RH_Sheath)} = {slot.EquipParam.BB_DummyPoly_Model0_RH_Sheath}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model0_RH_FormA)} = {slot.EquipParam.BB_DummyPoly_Model0_RH_FormA}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model0_RH_FormB)} = {slot.EquipParam.BB_DummyPoly_Model0_RH_FormB}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model0_LH_Sheath)} = {slot.EquipParam.BB_DummyPoly_Model0_LH_Sheath}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model0_LH_FormA)} = {slot.EquipParam.BB_DummyPoly_Model0_LH_FormA}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model0_LH_FormB)} = {slot.EquipParam.BB_DummyPoly_Model0_LH_FormB}");
                                        
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model1_RH_Sheath)} = {slot.EquipParam.BB_DummyPoly_Model1_RH_Sheath}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model1_RH_FormA)} = {slot.EquipParam.BB_DummyPoly_Model1_RH_FormA}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model1_RH_FormB)} = {slot.EquipParam.BB_DummyPoly_Model1_RH_FormB}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model1_LH_Sheath)} = {slot.EquipParam.BB_DummyPoly_Model1_LH_Sheath}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model1_LH_FormA)} = {slot.EquipParam.BB_DummyPoly_Model1_LH_FormA}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model1_LH_FormB)} = {slot.EquipParam.BB_DummyPoly_Model1_LH_FormB}");
                                        
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model2_RH_Sheath)} = {slot.EquipParam.BB_DummyPoly_Model2_RH_Sheath}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model2_RH_FormA)} = {slot.EquipParam.BB_DummyPoly_Model2_RH_FormA}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model2_RH_FormB)} = {slot.EquipParam.BB_DummyPoly_Model2_RH_FormB}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model2_LH_Sheath)} = {slot.EquipParam.BB_DummyPoly_Model2_LH_Sheath}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model2_LH_FormA)} = {slot.EquipParam.BB_DummyPoly_Model2_LH_FormA}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model2_LH_FormB)} = {slot.EquipParam.BB_DummyPoly_Model2_LH_FormB}");
                                        
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model3_RH_Sheath)} = {slot.EquipParam.BB_DummyPoly_Model3_RH_Sheath}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model3_RH_FormA)} = {slot.EquipParam.BB_DummyPoly_Model3_RH_FormA}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model3_RH_FormB)} = {slot.EquipParam.BB_DummyPoly_Model3_RH_FormB}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model3_LH_Sheath)} = {slot.EquipParam.BB_DummyPoly_Model3_LH_Sheath}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model3_LH_FormA)} = {slot.EquipParam.BB_DummyPoly_Model3_LH_FormA}");
                                        ImGui.Text($"{nameof(slot.EquipParam.BB_DummyPoly_Model3_LH_FormB)} = {slot.EquipParam.BB_DummyPoly_Model3_LH_FormB}");
                                        
                                        
                                        
                                        ImGui.TreePop();
                                    }
                                }


                                ImGui.TreePop();
                            }
                        });
                        ImGui.TreePop();
                    }
                    
                    
                    
                    
                    

                    //bool applyEnabled = !LoadingTaskMan.AnyTasksRunning();

                    //if (!applyEnabled)
                    //    Tools.PushGrayedOut();

                    //ImGui.Button("Apply Equipment to Character");

                    //if (ImGui.IsItemClicked() && applyEnabled)
                    //    asm.UpdateModels(isAsync: true, onCompleteAction: null, updateFaceAndBody: false);

                    //if (!applyEnabled)
                    //    Tools.PopGrayedOut();

                    //ImGui.Separator();


                    //ImGui.Separator();

                    var chrCustomize = asm.ChrCustomize.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    foreach (var key in chrCustomize.Keys)
                    {
                        if (key == FlverMaterial.ChrCustomizeTypes.None)
                            continue;
                        var color = chrCustomize[key].ToCS();
                        ImGui.ColorEdit4($"{key}###ChrAsm_ChrCustomize_{key}", ref color, ImGuiColorEditFlags.HDR);
                        anyFieldFocused |= ImGui.IsItemActive();
                        chrCustomize[key] = color.ToXna();
                    }

                    asm.ChrCustomize = chrCustomize;
                }

                if (RequestVerticalScroll >= 0)
                {
                    ImGui.SetScrollY(RequestVerticalScroll);
                    RequestVerticalScroll = -1;
                }
                
                if (disabledThisFrame)
                    ImGui.EndDisabled();
                
                anyFieldFocusedRef |= anyFieldFocused;
            }
        }
    }
}
