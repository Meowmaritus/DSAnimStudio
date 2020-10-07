using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class SapImportConfigs
    {
        public class ImportConfigFlver2
        {
            //public enum EquipModelImportSlot
            //{
            //    Head,
            //    Body,
            //    Arms,
            //    Legs,
            //    RightWeapon0,
            //    RightWeapon1,
            //    RightWeapon2,
            //    RightWeapon3,
            //    LeftWeapon0,
            //    LeftWeapon1,
            //    LeftWeapon2,
            //    LeftWeapon3,
            //}

            public string AssetPath { get; set; }

            public float SceneScale { get; set; } = 1.0f;
            public bool ConvertFromZUp { get; set; } = true;

            public bool KeepOriginalDummyPoly { get; set; } = true;
        }
    }
}
