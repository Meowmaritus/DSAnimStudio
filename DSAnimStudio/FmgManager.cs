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

        public static void LoadAllFMG()
        {
            FMG weaponNamesFmg = null;
            FMG protectorNamesFmg = null;

            if (TaeInterop.CurrentHkxVariation == SoulsFormats.HKX.HKXVariation.HKXDS1)
            {
                var msgbnd = BND3.Read($@"{TaeInterop.InterrootPath}\msg\ENGLISH\item.msgbnd");

                weaponNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("武器名.fmg")).Bytes);
                protectorNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("防具名.fmg")).Bytes);
            }
            else if (TaeInterop.CurrentHkxVariation == HKX.HKXVariation.HKXDS3)
            {
                var msgbnd = BND4.Read($@"{TaeInterop.InterrootPath}\msg\engus\item_dlc2.msgbnd.dcx");

                weaponNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("武器名.fmg")).Bytes);
                protectorNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("防具名.fmg")).Bytes);
            }
            else if (TaeInterop.CurrentHkxVariation == HKX.HKXVariation.HKXBloodBorne)
            {
                var msgbnd = BND4.Read($@"{TaeInterop.InterrootPath}\msg\engus\item.msgbnd.dcx");

                weaponNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("武器名.fmg")).Bytes);
                protectorNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("防具名.fmg")).Bytes);
            }


            if (weaponNamesFmg != null)
            {
                WeaponNames.Clear();
                foreach (var entry in weaponNamesFmg.Entries)
                {
                    if (ParamManager.EquipParamWeapon.ContainsKey(entry.ID))
                        WeaponNames.Add(entry.ID, entry.Text);
                }
            }

            if (protectorNamesFmg != null)
            {
                ProtectorNames_HD.Clear();
                ProtectorNames_BD.Clear();
                ProtectorNames_AM.Clear();
                ProtectorNames_LG.Clear();
                foreach (var entry in protectorNamesFmg.Entries)
                {
                    if (entry.ID < 1000000 && TaeInterop.CurrentHkxVariation == HKX.HKXVariation.HKXDS3)
                        continue;
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
            }
            

        }
    }
}
