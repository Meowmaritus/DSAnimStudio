using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;
using System.Drawing.Printing;

namespace DSAnimStudio
{
    public class zzz_FmgManagerIns
    {
        public zzz_DocumentIns ParentDocument;

        public zzz_FmgManagerIns(zzz_DocumentIns parentDoc)
        {
            ParentDocument = parentDoc;
        }

        public bool IncludeAllIDs = false;

        public string[] EquipmentNamesHD;
        public string[] EquipmentNamesBD;
        public string[] EquipmentNamesAM;
        public string[] EquipmentNamesLG;
        public string[] EquipmentNamesWP_A;
        public string[] EquipmentNamesWP_L;
        public string[] EquipmentNamesWP_R;
        public string[] EquipmentNamesWP_BL;
        public string[] EquipmentNamesWP_BR;
        public string[] EquipmentNamesWP_BLWR;
        public string[] EquipmentNamesWP_BRWR;

        public int[] EquipmentIDsHD;
        public int[] EquipmentIDsBD;
        public int[] EquipmentIDsAM;
        public int[] EquipmentIDsLG;
        public int[] EquipmentIDsWP_A;
        public int[] EquipmentIDsWP_L;
        public int[] EquipmentIDsWP_R;
        public int[] EquipmentIDsWP_BL;
        public int[] EquipmentIDsWP_BR;
        public int[] EquipmentIDsWP_BLWR;
        public int[] EquipmentIDsWP_BRWR;

        public Dictionary<int, string> WeaponNamesMap_A = new Dictionary<int, string>();
        public Dictionary<int, string> WeaponNamesMap_L = new Dictionary<int, string>();
        public Dictionary<int, string> WeaponNamesMap_R = new Dictionary<int, string>();
        public Dictionary<int, string> WeaponNamesMap_BL = new Dictionary<int, string>();
        public Dictionary<int, string> WeaponNamesMap_BR = new Dictionary<int, string>();
        public Dictionary<int, string> WeaponNamesMap_BLWR = new Dictionary<int, string>();
        public Dictionary<int, string> WeaponNamesMap_BRWR = new Dictionary<int, string>();
        public Dictionary<int, string> ProtectorNamesMap_HD = new Dictionary<int, string>();
        public Dictionary<int, string> ProtectorNamesMap_BD = new Dictionary<int, string>();
        public Dictionary<int, string> ProtectorNamesMap_AM = new Dictionary<int, string>();
        public Dictionary<int, string> ProtectorNamesMap_LG = new Dictionary<int, string>();

        public void Dispose()
        {
            EquipmentNamesHD = null;
            EquipmentNamesBD = null;
            EquipmentNamesAM = null;
            EquipmentNamesLG = null;
            EquipmentNamesWP_A = null;
            EquipmentNamesWP_L = null;
            EquipmentNamesWP_R = null;
            EquipmentNamesWP_BL = null;
            EquipmentNamesWP_BR = null;
            EquipmentNamesWP_BLWR = null;
            EquipmentNamesWP_BRWR = null;

            EquipmentIDsHD = null;
            EquipmentIDsBD = null;
            EquipmentIDsAM = null;
            EquipmentIDsLG = null;
            EquipmentIDsWP_A = null;
            EquipmentIDsWP_L = null;
            EquipmentIDsWP_R = null;
            EquipmentIDsWP_BL = null;
            EquipmentIDsWP_BR = null;
            EquipmentIDsWP_BLWR = null;
            EquipmentIDsWP_BRWR = null;


            WeaponNamesMap_A?.Clear();
            WeaponNamesMap_L?.Clear();
            WeaponNamesMap_R?.Clear();
            WeaponNamesMap_BL?.Clear();
            WeaponNamesMap_BR?.Clear();
            WeaponNamesMap_BLWR?.Clear();
            WeaponNamesMap_BRWR?.Clear();
            ProtectorNamesMap_HD?.Clear();
            ProtectorNamesMap_BD?.Clear();
            ProtectorNamesMap_AM?.Clear();
            ProtectorNamesMap_LG?.Clear();

            WeaponNamesMap_A = null;
            WeaponNamesMap_L = null;
            WeaponNamesMap_R = null;
            WeaponNamesMap_BL = null;
            WeaponNamesMap_BR = null;
            WeaponNamesMap_BLWR = null;
            WeaponNamesMap_BRWR = null;
            ProtectorNamesMap_HD = null;
            ProtectorNamesMap_BD = null;
            ProtectorNamesMap_AM = null;
            ProtectorNamesMap_LG = null;
        }

        private SoulsAssetPipeline.SoulsGames GameTypeCurrentFmgsAreLoadedFrom = SoulsAssetPipeline.SoulsGames.None;

        private bool WeaponIDValid(long weaponID)
        {
            if (IncludeAllIDs)
                return true;

            if (ParentDocument.GameRoot.GameType == SoulsGames.DES && ((int)weaponID % 100) != 0)
                return false;

            if (ParentDocument.GameRoot.GameType is SoulsGames.DS1 or SoulsGames.DS1R && ((int)weaponID % 1000) != 0)
                return false;

            if (ParentDocument.GameRoot.GameType == SoulsGames.BB && ((int)weaponID % 10000) != 0)
                return false;

            if (ParentDocument.GameRoot.GameType == SoulsGames.DS3 && ((int)weaponID % 10000) != 0)
                return false;

            if ((ParentDocument.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR)  && ((int)weaponID % 10000) != 0)
                return false;

            if (ParentDocument.GameRoot.GameType is SoulsGames.SDT && (weaponID < 5000 || weaponID >= 100000))
                return false;

            return true;
        }

        public void LoadAllFMG(bool forceReload)
        {
            if ((!forceReload && GameTypeCurrentFmgsAreLoadedFrom == ParentDocument.GameRoot.GameType) && Main.Config?.EnableFileCaching == true)
                return;

            List<FMG> weaponNameFMGs = new List<FMG>();
            List<FMG> armorNameFMGs = new List<FMG>();

            /*
                [ptde]
                weapon 1
                armor 2

                [ds1r]
                weapon 1, 30
                armor 2, 32

                [ds3]
                weapon 1
                armor 2
                weapon_dlc1 18
                armor_dlc1 19
                weapon_dlc2 33
                armor_dlc2 34

                [bb]
                weapon 1
                armor 2

                [sdt]
                weapon 1
                armor 2
            */

            List<string> msgbndsSearched_ForError = new List<string>();

            void TryToLoadFromMSGBND(string language, string msgbndName, int weaponNamesIdx, int armorNamesIdx)
            {

                var msgbndRelativePath = $@"/msg/{language}/{msgbndName}";
                //var fullMsgbndPath = msgbndRelativePath;// GameRoot.GetInterrootPath(msgbndRelativePath);
                IBinder msgbnd = null;
                msgbndsSearched_ForError.Add(msgbndRelativePath);



                if (ParentDocument.GameData.FileExists(msgbndRelativePath))
                {
                    var msgbndFile = ParentDocument.GameData.ReadFile(msgbndRelativePath);
                    if (BND3.Is(msgbndFile))
                        msgbnd = BND3.Read(msgbndFile);
                    else if (BND4.Is(msgbndFile))
                        msgbnd = BND4.Read(msgbndFile);

                    var weaponFile = msgbnd.Files.FirstOrDefault(x => x.ID == weaponNamesIdx);
                    var armorFile = msgbnd.Files.FirstOrDefault(x => x.ID == armorNamesIdx);

                    if (weaponFile != null)
                        weaponNameFMGs.Add(FMG.Read(weaponFile.Bytes));

                    if (armorFile != null)
                        armorNameFMGs.Add(FMG.Read(armorFile.Bytes));
                }

                if (msgbnd == null)
                {
                    ImguiOSD.DialogManager.DialogOK("Unable to find asset",
                        $"Unable to find text file '{msgbndRelativePath}'. Some player equipment may not show names.");

                    return;
                }
            }

            if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1)
            {
                TryToLoadFromMSGBND("ENGLISH", "item.msgbnd.dcx", 11, 12);
                TryToLoadFromMSGBND("ENGLISH", "menu.msgbnd.dcx", 115, 117); //Patch
            }
            else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
            {
                TryToLoadFromMSGBND("na_english", "item.msgbnd.dcx", 11, 12);
            }
            else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                TryToLoadFromMSGBND("ENGLISH", "item.msgbnd.dcx", 11, 12);
                TryToLoadFromMSGBND("ENGLISH", "item.msgbnd.dcx", 115, 117); //Patch
            }
            else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
            {
                // Old way of loading:
                //TryToLoadFromMSGBND("engus", "item_dlc1.msgbnd.dcx", 11, 12); //vanilla
                //TryToLoadFromMSGBND("engus", "item_dlc1.msgbnd.dcx", 211, 212); //DLC1

                //TryToLoadFromMSGBND("engus", "item_dlc2.msgbnd.dcx", 11, 12); //vanilla
                //TryToLoadFromMSGBND("engus", "item_dlc2.msgbnd.dcx", 211, 212); //DLC1
                //TryToLoadFromMSGBND("engus", "item_dlc2.msgbnd.dcx", 251, 252); //DLC2

                // New way of loading (based on Elden Ring):
                if (ParentDocument.GameData.FileExists($@"/msg/engus/item_dlc2.msgbnd.dcx"))
                {
                    // Reverse load order so that DLC stuff is overwritten by vanilla stuff
                    TryToLoadFromMSGBND("engus", "item_dlc2.msgbnd.dcx", 251, 252); // dlc2
                    TryToLoadFromMSGBND("engus", "item_dlc2.msgbnd.dcx", 211, 212); // dlc1
                    TryToLoadFromMSGBND("engus", "item_dlc2.msgbnd.dcx", 11, 12); // vanilla
                }
                else if (ParentDocument.GameData.FileExists($@"/msg/engus/item_dlc1.msgbnd.dcx"))
                {
                    // Reverse load order so that DLC stuff is overwritten by vanilla stuff
                    TryToLoadFromMSGBND("engus", "item_dlc1.msgbnd.dcx", 211, 212); // dlc1
                    TryToLoadFromMSGBND("engus", "item_dlc1.msgbnd.dcx", 11, 12); // vanilla
                }
                else
                {
                    TryToLoadFromMSGBND("engus", "item.msgbnd.dcx", 11, 12); // vanilla
                }

            }
            else if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER)
            {
                if (ParentDocument.GameData.FileExists($@"/msg/engus/item_dlc02.msgbnd.dcx"))
                {
                    // Reverse load order so that DLC stuff is overwritten by vanilla stuff
                    TryToLoadFromMSGBND("engus", "item_dlc02.msgbnd.dcx", 410, 413); // dlc02
                    TryToLoadFromMSGBND("engus", "item_dlc02.msgbnd.dcx", 310, 313); // dlc01
                    TryToLoadFromMSGBND("engus", "item_dlc02.msgbnd.dcx", 11, 12); // vanilla
                }
                else if (ParentDocument.GameData.FileExists($@"/msg/engus/item_dlc01.msgbnd.dcx"))
                {
                    // Reverse load order so that DLC stuff is overwritten by vanilla stuff
                    TryToLoadFromMSGBND("engus", "item_dlc01.msgbnd.dcx", 310, 313); // dlc01
                    TryToLoadFromMSGBND("engus", "item_dlc01.msgbnd.dcx", 11, 12); // vanilla
                }
                else
                {
                    TryToLoadFromMSGBND("engus", "item.msgbnd.dcx", 11, 12); // vanilla
                }
            }
            else if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ERNR)
            {
                //if (ParentDocument.GameData.FileExists($@"/msg/engus/item_dlc02.msgbnd.dcx"))
                //{
                //    // Reverse load order so that DLC stuff is overwritten by vanilla stuff
                //    TryToLoadFromMSGBND("engus", "item_dlc02.msgbnd.dcx", 410, 413); // dlc02
                //    TryToLoadFromMSGBND("engus", "item_dlc02.msgbnd.dcx", 310, 313); // dlc01
                //    TryToLoadFromMSGBND("engus", "item_dlc02.msgbnd.dcx", 11, 12); // vanilla
                //}
                //else if (ParentDocument.GameData.FileExists($@"/msg/engus/item_dlc01.msgbnd.dcx"))
                //{
                //    // Reverse load order so that DLC stuff is overwritten by vanilla stuff
                //    TryToLoadFromMSGBND("engus", "item_dlc01.msgbnd.dcx", 310, 313); // dlc01
                //    TryToLoadFromMSGBND("engus", "item_dlc01.msgbnd.dcx", 11, 12); // vanilla
                //}
                //else
                //{
                //    TryToLoadFromMSGBND("engus", "item.msgbnd.dcx", 11, 12); // vanilla
                //}
                TryToLoadFromMSGBND("engus", "item.msgbnd.dcx", 11, 12); // vanilla
            }
            else if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.SDT
                or SoulsAssetPipeline.SoulsGames.AC6)
            {
                TryToLoadFromMSGBND("engus", "item.msgbnd.dcx", 11, 12);
            }
            else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
            {
                TryToLoadFromMSGBND("engus", "item.msgbnd.dcx", 11, 12);
            }

            if (weaponNameFMGs.Count == 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Unable to find any weapon names in the game's text files, so functionality of player equipment editor may be limited. Files searched: ");
                foreach (var fn in msgbndsSearched_ForError)
                    sb.AppendLine($"    -{fn}");
                ImguiOSD.DialogManager.DialogOK("Text Not Found", sb.ToString());
            }

            if (armorNameFMGs.Count == 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Unable to find any armor names in the game's text files, so functionality of player equipment editor may be limited. Files searched: ");
                foreach (var fn in msgbndsSearched_ForError)
                    sb.AppendLine($"    -{fn}");
                ImguiOSD.DialogManager.DialogOK("Text Not Found", sb.ToString());
            }

            WeaponNamesMap_A.Clear();
            WeaponNamesMap_L.Clear();
            WeaponNamesMap_R.Clear();
            WeaponNamesMap_BL.Clear();
            WeaponNamesMap_BR.Clear();
            WeaponNamesMap_BLWR.Clear();
            WeaponNamesMap_BRWR.Clear();

            void DoWeaponEntry(FMG.Entry entry)
            {
                if (string.IsNullOrWhiteSpace(entry.Text) ||
                    !ParentDocument.ParamManager.EquipParamWeapon.ContainsKey(entry.ID))
                    return;

                var wpn = ParentDocument.ParamManager.EquipParamWeapon[entry.ID];



                // if (GameRoot.GameType == SoulsGames.DS3 && (entry.ID % 10000) != 0)
                //     return;
                // if ((entry.ID % 1000) != 0)
                //     return;

                if (!WeaponIDValid(entry.ID))
                    return;

                string val = entry.Text + $" <{entry.ID}>";
                WeaponNamesMap_A[entry.ID] = val;

                if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn299000 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[entry.ID] = val;
                    WeaponNamesMap_BLWR[entry.ID] = val;
                }
                else if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn299100 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[entry.ID] = val;
                    WeaponNamesMap_BLWR[entry.ID] = val;
                }
                else if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn299200 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[entry.ID] = val;
                    WeaponNamesMap_BLWR[entry.ID] = val;
                }
                else if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn299300 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[entry.ID] = val;
                    WeaponNamesMap_BLWR[entry.ID] = val;
                }
                else if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn299400 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[entry.ID] = val;
                    WeaponNamesMap_BLWR[entry.ID] = val;
                }
                else if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn299500 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[entry.ID] = val;
                    WeaponNamesMap_BLWR[entry.ID] = val;
                }
                else if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn3800500 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[entry.ID] = val;
                    WeaponNamesMap_BLWR[entry.ID] = val;
                }
                else
                {
                    if (wpn.IsLeftHandEquippable)
                        WeaponNamesMap_L[entry.ID] = val;
                    if (wpn.IsRightHandEquippable)
                        WeaponNamesMap_R[entry.ID] = val;
                    if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                    {
                        if (wpn.AC6IsBackLeftSlotEquippable)
                            WeaponNamesMap_BL[entry.ID] = val;
                        if (wpn.AC6IsBackRightSlotEquippable)
                            WeaponNamesMap_BR[entry.ID] = val;
                    }
                }



            }

            foreach (var wpnNameFmg in weaponNameFMGs)
                foreach (var entry in wpnNameFmg.Entries)
                    DoWeaponEntry(entry);

            ProtectorNamesMap_HD.Clear();
            ProtectorNamesMap_BD.Clear();
            ProtectorNamesMap_AM.Clear();
            ProtectorNamesMap_LG.Clear();

            void DoProtectorParamEntry(FMG.Entry entry)
            {
                if (string.IsNullOrWhiteSpace(entry.Text))
                    return;
                //if (entry.ID < 1000000 && GameDataManager.GameType == GameDataManager.GameTypes.DS3)
                //    return;
                if (ParentDocument.ParamManager.EquipParamProtector.ContainsKey(entry.ID))
                {
                    string val = entry.Text + $" <{entry.ID}>";
                    var protectorParam = ParentDocument.ParamManager.EquipParamProtector[entry.ID];
                    if (protectorParam.HeadEquip)
                    {
                        if (ProtectorNamesMap_HD.ContainsKey(entry.ID))
                            ProtectorNamesMap_HD[entry.ID] = val;
                        else
                            ProtectorNamesMap_HD.Add(entry.ID, val);
                    }
                    else if (protectorParam.BodyEquip)
                    {
                        if (ProtectorNamesMap_BD.ContainsKey(entry.ID))
                            ProtectorNamesMap_BD[entry.ID] = val;
                        else
                            ProtectorNamesMap_BD.Add(entry.ID, entry.Text + $" <{entry.ID}>");
                    }
                    else if (protectorParam.ArmEquip)
                    {
                        if (ProtectorNamesMap_AM.ContainsKey(entry.ID))
                            ProtectorNamesMap_AM[entry.ID] = val;
                        else
                            ProtectorNamesMap_AM.Add(entry.ID, entry.Text + $" <{entry.ID}>");
                    }
                    else if (protectorParam.LegEquip)
                    {
                        if (ProtectorNamesMap_LG.ContainsKey(entry.ID))
                            ProtectorNamesMap_LG[entry.ID] = val;
                        else
                            ProtectorNamesMap_LG.Add(entry.ID, entry.Text + $" <{entry.ID}>");
                    }
                }
            }

            foreach (var armorNameFmg in armorNameFMGs)
                foreach (var entry in armorNameFmg.Entries)
                    DoProtectorParamEntry(entry);

            //WeaponNames_A = WeaponNames_A.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            //WeaponNames_L = WeaponNames_L.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            //WeaponNames_R = WeaponNames_R.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            //WeaponNames_BL = WeaponNames_BL.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            //WeaponNames_BR = WeaponNames_BR.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ProtectorNamesMap_HD = ProtectorNamesMap_HD.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ProtectorNamesMap_BD = ProtectorNamesMap_BD.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ProtectorNamesMap_AM = ProtectorNamesMap_AM.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ProtectorNamesMap_LG = ProtectorNamesMap_LG.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var protector in ParentDocument.ParamManager.EquipParamProtector)
            {
                if (protector.Value.HeadEquip && !ProtectorNamesMap_HD.ContainsKey((int)protector.Key))
                {
                    ProtectorNamesMap_HD.Add((int)protector.Key, $"<{protector.Key}> {protector.Value.Name ?? ""}");
                }
                else if (protector.Value.BodyEquip && !ProtectorNamesMap_BD.ContainsKey((int)protector.Key))
                {
                    ProtectorNamesMap_BD.Add((int)protector.Key, $"<{protector.Key}> {protector.Value.Name ?? ""}");
                }
                else if (protector.Value.ArmEquip && !ProtectorNamesMap_AM.ContainsKey((int)protector.Key))
                {
                    ProtectorNamesMap_AM.Add((int)protector.Key, $"<{protector.Key}> {protector.Value.Name ?? ""}");
                }
                else if (protector.Value.LegEquip && !ProtectorNamesMap_LG.ContainsKey((int)protector.Key))
                {
                    ProtectorNamesMap_LG.Add((int)protector.Key, $"<{protector.Key}> {protector.Value.Name ?? ""}");
                }
            }

            foreach (var kvp in ParentDocument.ParamManager.EquipParamWeapon)
            {
                var wpn = kvp.Value;

                if (!WeaponIDValid(kvp.Key))
                {
                    continue;
                }


                if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn299000 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                    WeaponNamesMap_BLWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                }
                else if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn299100 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                    WeaponNamesMap_BLWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                }
                else if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn299200 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                    WeaponNamesMap_BLWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                }
                else if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn299300 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                    WeaponNamesMap_BLWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                }
                else if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn299400 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                    WeaponNamesMap_BLWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                }
                else if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn299500 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                    WeaponNamesMap_BLWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                }
                else if (wpn.ID == ParamData.EquipParamWeapon.WP_ID_AC6_UnkWpn3800500 && ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WeaponNamesMap_BRWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                    WeaponNamesMap_BLWR[(int)kvp.Key] = $"<{kvp.Key}> {kvp.Value.Name ?? ""}";
                }
                else
                {
                    if (!WeaponNamesMap_A.ContainsKey((int)kvp.Key))
                        WeaponNamesMap_A.Add((int)kvp.Key, $"<{kvp.Key}> {kvp.Value.Name ?? ""}");
                    if (kvp.Value.IsLeftHandEquippable && !WeaponNamesMap_L.ContainsKey((int)kvp.Key))
                        WeaponNamesMap_L.Add((int)kvp.Key, $"<{kvp.Key}> {kvp.Value.Name ?? ""}");
                    if (kvp.Value.IsRightHandEquippable && !WeaponNamesMap_R.ContainsKey((int)kvp.Key))
                        WeaponNamesMap_R.Add((int)kvp.Key, $"<{kvp.Key}> {kvp.Value.Name ?? ""}");
                    if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                    {
                        if (kvp.Value.AC6IsBackLeftSlotEquippable && !WeaponNamesMap_BL.ContainsKey((int)kvp.Key))
                            WeaponNamesMap_BL.Add((int)kvp.Key, $"<{kvp.Key}> {kvp.Value.Name ?? ""}");
                        if (kvp.Value.AC6IsBackRightSlotEquippable && !WeaponNamesMap_BR.ContainsKey((int)kvp.Key))
                            WeaponNamesMap_BR.Add((int)kvp.Key, $"<{kvp.Key}> {kvp.Value.Name ?? ""}");
                    }
                }


            }

            void EnsureNoneEntry(Dictionary<int, string> dict)
            {
                dict[-1] = "None";
            }

            EnsureNoneEntry(ProtectorNamesMap_HD);
            EnsureNoneEntry(ProtectorNamesMap_BD);
            EnsureNoneEntry(ProtectorNamesMap_AM);
            EnsureNoneEntry(ProtectorNamesMap_LG);
            EnsureNoneEntry(WeaponNamesMap_A);
            EnsureNoneEntry(WeaponNamesMap_L);
            EnsureNoneEntry(WeaponNamesMap_R);
            EnsureNoneEntry(WeaponNamesMap_BL);
            EnsureNoneEntry(WeaponNamesMap_BR);
            EnsureNoneEntry(WeaponNamesMap_BLWR);
            EnsureNoneEntry(WeaponNamesMap_BRWR);


            ProtectorNamesMap_HD = ProtectorNamesMap_HD.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ProtectorNamesMap_BD = ProtectorNamesMap_BD.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ProtectorNamesMap_AM = ProtectorNamesMap_AM.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ProtectorNamesMap_LG = ProtectorNamesMap_LG.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            WeaponNamesMap_A = WeaponNamesMap_A.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            WeaponNamesMap_L = WeaponNamesMap_L.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            WeaponNamesMap_R = WeaponNamesMap_R.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            WeaponNamesMap_BL = WeaponNamesMap_BL.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            WeaponNamesMap_BR = WeaponNamesMap_BR.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            WeaponNamesMap_BLWR = WeaponNamesMap_BLWR.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            WeaponNamesMap_BRWR = WeaponNamesMap_BRWR.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);





            EquipmentNamesHD = ProtectorNamesMap_HD.Select(kvp => kvp.Value).ToArray();
            EquipmentIDsHD = ProtectorNamesMap_HD.Select(kvp => kvp.Key).ToArray();

            EquipmentNamesBD = ProtectorNamesMap_BD.Select(kvp => kvp.Value).ToArray();
            EquipmentIDsBD = ProtectorNamesMap_BD.Select(kvp => kvp.Key).ToArray();

            EquipmentNamesAM = ProtectorNamesMap_AM.Select(kvp => kvp.Value).ToArray();
            EquipmentIDsAM = ProtectorNamesMap_AM.Select(kvp => kvp.Key).ToArray();

            EquipmentNamesLG = ProtectorNamesMap_LG.Select(kvp => kvp.Value).ToArray();
            EquipmentIDsLG = ProtectorNamesMap_LG.Select(kvp => kvp.Key).ToArray();

            EquipmentNamesWP_A = WeaponNamesMap_A.Select(kvp => kvp.Value).ToArray();
            EquipmentIDsWP_A = WeaponNamesMap_A.Select(kvp => kvp.Key).ToArray();

            EquipmentNamesWP_L = WeaponNamesMap_L.Select(kvp => kvp.Value).ToArray();
            EquipmentIDsWP_L = WeaponNamesMap_L.Select(kvp => kvp.Key).ToArray();

            EquipmentNamesWP_R = WeaponNamesMap_R.Select(kvp => kvp.Value).ToArray();
            EquipmentIDsWP_R = WeaponNamesMap_R.Select(kvp => kvp.Key).ToArray();

            EquipmentNamesWP_BL = WeaponNamesMap_BL.Select(kvp => kvp.Value).ToArray();
            EquipmentIDsWP_BL = WeaponNamesMap_BL.Select(kvp => kvp.Key).ToArray();

            EquipmentNamesWP_BR = WeaponNamesMap_BR.Select(kvp => kvp.Value).ToArray();
            EquipmentIDsWP_BR = WeaponNamesMap_BR.Select(kvp => kvp.Key).ToArray();

            EquipmentNamesWP_BLWR = WeaponNamesMap_BLWR.Select(kvp => kvp.Value).ToArray();
            EquipmentIDsWP_BLWR = WeaponNamesMap_BLWR.Select(kvp => kvp.Key).ToArray();

            EquipmentNamesWP_BRWR = WeaponNamesMap_BRWR.Select(kvp => kvp.Value).ToArray();
            EquipmentIDsWP_BRWR = WeaponNamesMap_BRWR.Select(kvp => kvp.Key).ToArray();

            GameTypeCurrentFmgsAreLoadedFrom = ParentDocument.GameRoot.GameType;
        }
    }
}
