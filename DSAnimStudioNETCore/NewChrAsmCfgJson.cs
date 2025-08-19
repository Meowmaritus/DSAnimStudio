using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewChrAsmCfgJson
    {
        public SoulsAssetPipeline.SoulsGames GameType;
        public bool IsFemale;
        public ParamData.PartSuffixType CurrentPartSuffixType = ParamData.PartSuffixType.None;
        public bool EnablePartsFileCaching = true;
        public Dictionary<NewChrAsm.EquipSlotTypes, int> EquipIDs = new Dictionary<NewChrAsm.EquipSlotTypes, int>();
        public Dictionary<NewChrAsm.EquipSlotTypes, NewEquipSlot_Armor.DirectEquipInfo> DirectEquipInfos = new Dictionary<NewChrAsm.EquipSlotTypes, NewEquipSlot_Armor.DirectEquipInfo>();
        public NewChrAsm.WeaponStyleType WeaponStyle = NewChrAsm.WeaponStyleType.OneHand;


        private Dictionary<FlverMaterial.ChrCustomizeTypes, Vector4> chrCustomize = new Dictionary<FlverMaterial.ChrCustomizeTypes, Vector4>();
        private object _lock = new object();
        public Dictionary<FlverMaterial.ChrCustomizeTypes, Vector4> ChrCustomize
        {
            get
            {
                Dictionary<FlverMaterial.ChrCustomizeTypes, Vector4> result = null;
                lock (_lock)
                {
                    result = chrCustomize.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return result;
            }
            set
            {
                lock (_lock)
                {
                    chrCustomize = value.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
        }

        public void WriteToChrAsm(NewChrAsm chrAsm)
        {
            chrAsm.IsFemale = IsFemale;
            chrAsm.CurrentPartSuffixType = CurrentPartSuffixType;
            chrAsm.EnablePartsFileCaching = EnablePartsFileCaching;

            chrAsm.ForAllWeaponSlots(slot =>
            {
                if (EquipIDs.ContainsKey(slot.EquipSlotType))
                    slot.EquipID = EquipIDs[slot.EquipSlotType];
            });

            chrAsm.ForAllArmorSlots(slot =>
            {
                if (slot.UsesEquipParam && EquipIDs.ContainsKey(slot.EquipSlotType))
                    slot.EquipID = EquipIDs[slot.EquipSlotType];
                if (!slot.UsesEquipParam && DirectEquipInfos.ContainsKey(slot.EquipSlotType))
                    slot.DirectEquip = DirectEquipInfos[slot.EquipSlotType];
            });





            chrAsm.StartWeaponStyle = WeaponStyle;
            chrAsm.WeaponStyle = WeaponStyle;
            //chrAsm.UpdateModels();
            var chrCustomizeDict = ChrCustomize.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var chrCustomizeValues = Enum.GetValues<FlverMaterial.ChrCustomizeTypes>();
            foreach (var e in chrCustomizeValues)
            {
                if (!chrCustomizeDict.ContainsKey(e))
                    chrCustomizeDict.Add(e, FlverMaterial.GetDefaultChrCustomizeColor(e));
            }

            chrAsm.ChrCustomize = chrCustomizeDict;

            chrAsm.Update(0, forceSyncUpdate: true);
        }

        public void CopyFromChrAsm(NewChrAsm chrAsm)
        {
            GameType = zzz_DocumentManager.CurrentDocument.GameRoot.GameType;
            IsFemale = chrAsm.IsFemale;
            CurrentPartSuffixType = chrAsm.CurrentPartSuffixType;
            EnablePartsFileCaching = chrAsm.EnablePartsFileCaching;

            EquipIDs.Clear();
            DirectEquipInfos.Clear();

            chrAsm.ForAllWeaponSlots(slot =>
            {
                EquipIDs[slot.EquipSlotType] = slot.EquipID;
            });

            chrAsm.ForAllArmorSlots(slot =>
            {
                if (slot.UsesEquipParam)
                    EquipIDs[slot.EquipSlotType] = slot.EquipID;
                else
                    DirectEquipInfos[slot.EquipSlotType] = slot.DirectEquip;
            });

          

            WeaponStyle = chrAsm.StartWeaponStyle;
            ChrCustomize = chrAsm.ChrCustomize.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
