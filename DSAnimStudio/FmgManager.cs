using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class FmgManager
    {


        public static Dictionary<int, string> WeaponNames = new Dictionary<int, string>();
        public static Dictionary<int, string> ProtectorNames_HD = new Dictionary<int, string>();
        public static Dictionary<int, string> ProtectorNames_BD = new Dictionary<int, string>();
        public static Dictionary<int, string> ProtectorNames_AM = new Dictionary<int, string>();
        public static Dictionary<int, string> ProtectorNames_LG = new Dictionary<int, string>();

        private static GameDataManager.GameTypes GameTypeCurrentFmgsAreLoadedFrom = GameDataManager.GameTypes.None;

        public static void LoadAllFMG(bool forceReload)
        {
            if (!forceReload && GameTypeCurrentFmgsAreLoadedFrom == GameDataManager.GameType)
                return;

            FMG weaponNamesFmg = null;
            FMG protectorNamesFmg = null;

            FMG weaponNamesFmg_dlc1 = null;
            FMG protectorNamesFmg_dlc1 = null;

            FMG weaponNamesFmg_dlc2 = null;
            FMG protectorNamesFmg_dlc2 = null;

            if (GameDataManager.GameType == GameDataManager.GameTypes.DS1)
            {
                var msgbnd = BND3.Read($@"{GameDataManager.InterrootPath}\msg\ENGLISH\item.msgbnd");

                weaponNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("武器名.fmg")).Bytes);
                protectorNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("防具名.fmg")).Bytes);
            }
            else if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
            {
                var msgbnd = BND4.Read($@"{GameDataManager.InterrootPath}\msg\engus\item_dlc2.msgbnd.dcx");

                weaponNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("武器名.fmg")).Bytes);
                protectorNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("防具名.fmg")).Bytes);

                var weaponNamesFmg_dlc1_entry = msgbnd.Files.LastOrDefault(f => f.Name.EndsWith("武器名_dlc1.fmg"));
                if (weaponNamesFmg_dlc1_entry != null)
                    weaponNamesFmg_dlc1 = FMG.Read(weaponNamesFmg_dlc1_entry.Bytes);

                var weaponNamesFmg_dlc2_entry = msgbnd.Files.LastOrDefault(f => f.Name.EndsWith("武器名_dlc2.fmg"));
                if (weaponNamesFmg_dlc2_entry != null)
                    weaponNamesFmg_dlc2 = FMG.Read(weaponNamesFmg_dlc2_entry.Bytes);

                var protectorNamesFmg_dlc1_entry = msgbnd.Files.LastOrDefault(f => f.Name.EndsWith("防具名_dlc1.fmg"));
                if (protectorNamesFmg_dlc1_entry != null)
                    protectorNamesFmg_dlc1 = FMG.Read(protectorNamesFmg_dlc1_entry.Bytes);

                var protectorNamesFmg_dlc2_entry = msgbnd.Files.LastOrDefault(f => f.Name.EndsWith("防具名_dlc2.fmg"));
                if (protectorNamesFmg_dlc2_entry != null)
                    protectorNamesFmg_dlc2 = FMG.Read(protectorNamesFmg_dlc2_entry.Bytes);
            }
            else if (GameDataManager.GameType == GameDataManager.GameTypes.BB)
            {
                var msgbnd = BND4.Read($@"{GameDataManager.InterrootPath}\msg\engus\item.msgbnd.dcx");

                weaponNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("武器名.fmg")).Bytes);
                protectorNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("防具名.fmg")).Bytes);
            }

            WeaponNames.Clear();

            void DoWeaponEntry(FMG.Entry entry)
            {
                if (string.IsNullOrWhiteSpace(entry.Text) ||
                    !ParamManager.EquipParamWeapon.ContainsKey(entry.ID))
                    return;

                if (GameDataManager.GameType == GameDataManager.GameTypes.DS3 && (entry.ID % 10000) != 0)
                    return;
                else if ((entry.ID % 1000) != 0)
                    return;

                WeaponNames.Add(entry.ID, entry.Text);
            }

            if (weaponNamesFmg != null)
                foreach (var entry in weaponNamesFmg.Entries)
                    DoWeaponEntry(entry);

            if (weaponNamesFmg_dlc1 != null)
                foreach (var entry in weaponNamesFmg_dlc1.Entries)
                    DoWeaponEntry(entry);

            if (weaponNamesFmg_dlc2 != null)
                foreach (var entry in weaponNamesFmg_dlc2.Entries)
                    DoWeaponEntry(entry);

            ProtectorNames_HD.Clear();
            ProtectorNames_BD.Clear();
            ProtectorNames_AM.Clear();
            ProtectorNames_LG.Clear();

            void DoProtectorParamEntry(FMG.Entry entry)
            {
                if (string.IsNullOrWhiteSpace(entry.Text))
                    return;
                if (entry.ID < 1000000 && GameDataManager.GameType == GameDataManager.GameTypes.DS3)
                    return;
                if (ParamManager.EquipParamProtector.ContainsKey(entry.ID))
                {
                    var protectorParam = ParamManager.EquipParamProtector[entry.ID];
                    if (protectorParam.HeadEquip)
                        ProtectorNames_HD.Add(entry.ID, entry.Text);
                    else if (protectorParam.BodyEquip)
                        ProtectorNames_BD.Add(entry.ID, entry.Text);
                    else if (protectorParam.ArmEquip)
                        ProtectorNames_AM.Add(entry.ID, entry.Text);
                    else if (protectorParam.LegEquip)
                        ProtectorNames_LG.Add(entry.ID, entry.Text);
                }
            }

            if (protectorNamesFmg != null)
            {
                foreach (var entry in protectorNamesFmg.Entries)
                {
                    DoProtectorParamEntry(entry);
                }
            }

            if (protectorNamesFmg_dlc1 != null)
            {
                foreach (var entry in protectorNamesFmg_dlc1.Entries)
                {
                    DoProtectorParamEntry(entry);
                }
            }

            if (protectorNamesFmg_dlc2 != null)
            {
                foreach (var entry in protectorNamesFmg_dlc2.Entries)
                {
                    DoProtectorParamEntry(entry);
                }
            }

            WeaponNames = WeaponNames.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ProtectorNames_HD = ProtectorNames_HD.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ProtectorNames_BD = ProtectorNames_BD.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ProtectorNames_AM = ProtectorNames_AM.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ProtectorNames_LG = ProtectorNames_LG.OrderBy(x => x.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            GameTypeCurrentFmgsAreLoadedFrom = GameDataManager.GameType;
        }
    }
}
