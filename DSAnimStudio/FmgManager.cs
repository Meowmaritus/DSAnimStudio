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

        public static void LoadAllFMG()
        {
            if (TaeInterop.CurrentHkxVariation == SoulsFormats.HKX.HKXVariation.HKXDS1)
            {

                var msgbnd = BND3.Read($@"{TaeInterop.InterrootPath}\msg\ENGLISH\item.msgbnd");

                var weaponNamesFmg = FMG.Read(msgbnd.Files.Last(f => f.Name.EndsWith("武器名.fmg")).Bytes);

                WeaponNames.Clear();
                foreach (var entry in weaponNamesFmg.Entries)
                {
                    if (ParamManager.EquipParamWeapon.ContainsKey(entry.ID))
                        WeaponNames.Add(entry.ID, entry.Text);
                }
            }
            else
            {
                throw new NotImplementedException("Only DS1 FMG loading currently in here :fatcateh:");
            }

            


        }
    }
}
