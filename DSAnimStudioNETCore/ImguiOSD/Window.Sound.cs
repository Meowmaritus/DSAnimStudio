using DSAnimStudio.TaeEditor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static DSAnimStudio.zzz_SoundManagerIns;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Sound : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            public override string NewImguiWindowTitle => "Sound";
            
            protected override void Init()
            {
                
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                ImGui.SliderFloat($"Volume", ref zzz_DocumentManager.CurrentDocument.SoundManager.AdjustSoundVolume, 0, 200, "%.2f%%");
                anyFieldFocused |= ImGui.IsItemActive();
                ImGui.Button("Reset to 100%");
                if (ImGui.IsItemClicked())
                    zzz_DocumentManager.CurrentDocument.SoundManager.AdjustSoundVolume = 100;
                

                var document = MainDoc;
                var soundManager = document?.SoundManager;

                if (soundManager == null)
                    return;

                ImGui.Separator();

                

                ImGui.Checkbox("SHOW DEBUG DIAGNOSTICS", ref soundManager.DebugShowDiagnostics);

                ImGui.Separator();

                var WW = soundManager.WwiseManager;
                var MO = soundManager.MagicOrchestraManager;

                if (Main.IsDebugBuild)
                {

                    if (soundManager.EngineType is EngineTypes.FMOD)
                    {

                    }
                    else if (soundManager.EngineType is EngineTypes.Wwise)
                    {
                        ImGui.Checkbox("Wwise - KEEP CONVERT TEMP FILES", ref zzz_WwiseManagerInst.DEBUG_KEEP_CONVERT_TEMP_FILES);
                    }
                    else if (soundManager.EngineType is EngineTypes.MagicOrchestra)
                    {
                        ImGui.Checkbox("MagicOrchestra - KEEP CONVERT TEMP FILES", ref zzz_MagicOrchestraManagerInst.DEBUG_KEEP_CONVERT_TEMP_FILES);
                    }

                    ImGui.Button("Clear /Temp/");
                    if (ImGui.IsItemClicked())
                    {
                        var dir = $@"{Main.Directory}\Temp";
                        if (Directory.Exists(dir))
                            Directory.Delete(dir, recursive: true);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                    }

                }

                ImGui.Separator();
                

                bool soundEnabled = !zzz_SoundManagerIns.SOUND_DISABLED;
                ImGui.Checkbox("Enable Audio System", ref soundEnabled);
                if (!soundEnabled != zzz_SoundManagerIns.SOUND_DISABLED)
                {
                    zzz_SoundManagerIns.SOUND_DISABLED = !soundEnabled;
                    if (soundEnabled)
                    {
                        //soundManager.SetEngineToCurrentGame(MainDoc.GameRoot.GameType);
                        MainDoc?.EditorScreen?.Graph?.ViewportInteractor?.LoadFmodSoundsForCurrentModel();
                    }
                    else
                    {

                        soundManager.PurgeLoadedAssets();
                    }
                }
                ImGui.Separator();

                ImGui.Button("Stop All Sounds");
                if (ImGui.IsItemClicked())
                    soundManager.StopAllSounds();

                ImGui.Button("Purge Sound Cache");
                if (ImGui.IsItemClicked())
                {
                    soundManager.PurgeLoadedAssets();
                    soundManager.SetEngineToCurrentGame(MainDoc.GameRoot.GameType, clearMasterBankList: false, clearLookupBankList: false);
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);
                }

                ImGui.Separator();

                if (ImGui.BeginTable("test_table", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.NoSavedSettings))
                {

                    ImGui.TableNextColumn();

                    lock (soundManager._lock_BankNameArray)
                    {
                        if (soundManager.BankNameArray != null)
                        {
                            ImGui.ListBox("All Banks", ref soundManager.BankNameArray_SelectedIndex,
                                soundManager.BankNameArray,
                                soundManager.BankNameArray.Length);

                            int selIndex = soundManager.BankNameArray_SelectedIndex;
                            if (selIndex < 0)
                                ImGuiDebugDrawer.PushDisabled();
                            if (Tools.SimpleClickButton("Add Selected"))
                            {
                                var selected = soundManager.BankNameArray[selIndex];

                                lock (soundManager._lock_LookupBanks)
                                {
                                    if (soundManager.LookupBanks == null)
                                        soundManager.LookupBanks = new string[] { selected };
                                    else if (!soundManager.LookupBanks.Contains(selected))
                                    {
                                        var banks = soundManager.LookupBanks.ToList();
                                        banks.Add(selected);
                                        soundManager.LookupBanks = banks.ToArray();
                                    }
                                }

                                soundManager.CopySoundBanksFromThisToProj();
                            }

                            if (selIndex < 0)
                                ImGuiDebugDrawer.PopDisabled();
                        }
                    }

                    ImGui.TableNextColumn();

                    lock (soundManager._lock_LookupBanks)
                    {
                        if (soundManager.LookupBanks != null)
                        {

                            ImGui.ListBox("Banks To Lookup Sounds From", ref soundManager.LookupBanks_SelectedIndex,
                                soundManager.LookupBanks,
                                soundManager.LookupBanks.Length);

                            int selIndex = soundManager.LookupBanks_SelectedIndex;
                            if (selIndex < 0)
                                ImGuiDebugDrawer.PushDisabled();

                            if (Tools.SimpleClickButton("Remove Selected"))
                            {
                                if (selIndex >= 0 && selIndex < soundManager.LookupBanks.Length)
                                {
                                    var selected = soundManager.LookupBanks[selIndex];

                                    if (soundManager.LookupBanks.Contains(selected))
                                    {
                                        var banks = soundManager.LookupBanks.ToList();
                                        banks.Remove(selected);
                                        soundManager.LookupBanks = banks.ToArray();
                                    }
                                }


                                soundManager.CopySoundBanksFromThisToProj();
                            }

                            if (selIndex < 0)
                                ImGuiDebugDrawer.PopDisabled();
                        }
                    }

                    ImGui.EndTable();
                }

                ImGui.Separator();

                if (soundManager.EngineType is EngineTypes.Wwise)
                {

                    Main.Config.Wwise_ShowMissingBankWarnings = MenuBar.CheckboxBig("Show Wwise Bank Not Found Warnings", Main.Config.Wwise_ShowMissingBankWarnings);
                    
                    
                    
                    
                    
                     
                    


                    ImGui.PushItemWidth(OSD.DefaultItemWidth * 3);
                    if (WW.DefensiveMaterialNames != null && WW.DefensiveMaterialNames.Length > 0)
                        ImGui.ListBox("Floor Material", ref WW.DefensiveMaterialIndex, WW.DefensiveMaterialNames, WW.DefensiveMaterialNames.Length);


                    //ImGui.ListBox("Armor Material (Top)", ref Wwise.ArmorMaterialIndexTop, Wwise.ArmorMaterialNames, Wwise.ArmorMaterialNames.Length);
                    //ImGui.ListBox("Armor Material (Bottom)", ref Wwise.ArmorMaterialIndexBottom, Wwise.ArmorMaterialNames, Wwise.ArmorMaterialNames.Length);

                    //ImGui.ListBox("Player Voice Type", ref Wwise.PlayerVoiceIndex, Wwise.PlayerVoiceTypes, Wwise.PlayerVoiceTypes.Length);

                    
                    foreach (var switchGroupHandler in WW.SwitchGroupHandlers)
                    {
                        switchGroupHandler.DoImGui();
                    }
                    ImGui.PopItemWidth();
                    
                    
                    
                    
                    
                    

                }
                else if (soundManager.EngineType is EngineTypes.FMOD)
                {
                    var fmod = document.Fmod;
                    //if (MenuBar.ClickItem("Reload All Sounds", fmod.IsInitialized))
                    //{
                    //    fmod.Purge();
                    //    fmod.LoadMainFEVs();
                    //    MainTaeScreen.Graph?.ViewportInteractor?.LoadFmodSoundsForCurrentModel();
                    //}

                    ImGui.Separator();

                    if (MenuBar.ClickItem("STOP ALL SOUNDS", fmod.IsInitialized, "Escape"))
                    {
                        soundManager.StopAllSounds();
                    }

                    ImGui.PushItemWidth(OSD.DefaultItemWidth * 2);
                    
                    //if (ImGui.BeginListBox("Load Additional Sounds"))
                    //{
                            
                    //    if (fmod.IsInitialized &&
                    //                        fmod.MediaPath != null)
                    //    {
                    //        //var fevFiles = zzz_DocumentManager.CurrentDocument.GameData.SearchFiles("/sound", @".*\.fev");
                    //        var fevFiles = fmod.ListOfAllFEVs;

                    //        for (int i = 0; i < fevFiles.Count; i++)
                    //        {
                                
                    //            var shortName = Path.GetFileNameWithoutExtension(fevFiles[i]);
                    //            bool thisSelected = fmod.LoadedFEVs.Contains(shortName);
                    //            //if (MenuBar.ClickItem(shortName, shortcut: thisSelected ? "(Loaded)" : null, selected: thisSelected))
                    //            //{
                    //            //    int underscoreIndex = shortName.IndexOf('_');
                    //            //    if (underscoreIndex >= 0)
                    //            //        shortName = shortName.Substring(Math.Min(underscoreIndex + 1, shortName.Length - 1));
                    //            //    fmod.LoadInterrootFEV(shortName);
                    //            //}

                    //            bool newSelected = MenuBar.CheckboxBig(shortName, thisSelected);
                    //            if (newSelected && !thisSelected)
                    //            {
                    //                int underscoreIndex = shortName.IndexOf('_');
                    //                if (underscoreIndex >= 0)
                    //                    shortName = shortName.Substring(Math.Min(underscoreIndex + 1, shortName.Length - 1));
                    //                fmod.LoadInterrootFEV(shortName);
                    //            }

                                

                    //        }
                    //    }

                    //    ImGui.EndListBox();
                    //}
                    

                    ImGui.Separator();

                    //FmodManager.ArmorMaterial = EnumSelectorItem("Player Armor Material",
                    //        FmodManager.ArmorMaterial, new Dictionary<FmodManager.ArmorMaterialType, string>
                    //    {
                    //            { FmodManager.ArmorMaterialType.Plates, "Platemail" },
                    //            { FmodManager.ArmorMaterialType.Chains, "Chainmail" },
                    //            { FmodManager.ArmorMaterialType.Cloth, "Cloth" },
                    //            { FmodManager.ArmorMaterialType.None, "Naked" },
                    //    });

                    if (ImGui.BeginListBox("Footstep Material"))
                    {
                        foreach (var mat in fmod.FloorMaterialNames)
                        {
                            if (MenuBar.CheckboxBig($"Material {mat.Key:D2}", mat.Key == fmod.FloorMaterial))
                            {
                                fmod.FloorMaterial = mat.Key;
                            }
                        }

                        ImGui.EndListBox();
                    }

                    ImGui.Text("\n\n\n\n");

                    ImGui.PopItemWidth();
                }
                else if (soundManager.EngineType is EngineTypes.MagicOrchestra)
                {


            



                    var fmod = document.Fmod;

                    if (ImGui.BeginListBox("Footstep Material"))
                    {
                        foreach (var mat in fmod.FloorMaterialNames)
                        {
                            if (MenuBar.CheckboxBig($"Material {mat.Key:D2}", mat.Key == fmod.FloorMaterial))
                            {
                                fmod.FloorMaterial = mat.Key;
                            }
                        }

                        ImGui.EndListBox();
                    }
                }
                
            }
        }
    }
}
