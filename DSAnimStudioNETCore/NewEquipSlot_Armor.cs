using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DSAnimStudio.NewChrAsm;
using static DSAnimStudio.ParamData;

namespace DSAnimStudio
{
    public class NewEquipSlot_Armor : NewEquipSlot
    {

        public NewEquipSlot_Armor(NewChrAsm asm, NewChrAsm.EquipSlotTypes equipSlot, string slotDisplayName, string slotDisplayNameShort, bool usesEquipParam)
            : base(asm, equipSlot, slotDisplayName, slotDisplayNameShort, usesEquipParam)
        {
            
        }


        public bool CheckIfModelLoaded()
        {
            bool result = false;
            lock (_lock_MODEL)
            {
                result = model != null;
            }
            return result;
        }

        private object _lock_MODEL = new object();
        private Model model = null;

        public override void AccessAllModels(Action<Model> doAction)
        {
            lock (_lock_MODEL)
            {
                if (model != null)
                    doAction(model);
            }
        }


        public void DrawAnimLayerDebug(ref Vector2 pos)
        {
            lock (_lock_MODEL)
            {
                model?.AnimContainer?.DrawDebug(ref pos);
            }
        }


        public ParamData.EquipParamProtector EquipParam 
            => (EquipID >= 0 && zzz_DocumentManager.CurrentDocument.ParamManager.EquipParamProtector.ContainsKey(EquipID))
            ? zzz_DocumentManager.CurrentDocument.ParamManager.EquipParamProtector[EquipID] : null;

        public enum DirectEquipGender
        {
            Invalid = 0,
            UnisexUseA = 1,
            MaleOnlyUseM = 2,
            FemaleOnlyUseF = 3,
            BothGendersUseMF = 4,
            UnisexUseMForBoth = 5,
        }

        public enum DirectEquipPartPrefix
        {
            None,
            HD,
            BD,
            AM,
            LG,
            FC,
            FG,
            HR,
            BS,
            CD,
        }

        public struct DirectEquipInfo
        {
            public DirectEquipPartPrefix PartPrefix = DirectEquipPartPrefix.None;
            public DirectEquipGender Gender = DirectEquipGender.Invalid;
            public int ModelID = -1;

            public DirectEquipInfo()
            {
            }

            public static bool operator ==(DirectEquipInfo a, DirectEquipInfo b)
            {
                return a.PartPrefix == b.PartPrefix && a.Gender == b.Gender && a.ModelID == b.ModelID;
            }
            public static bool operator !=(DirectEquipInfo a, DirectEquipInfo b)
            {
                return !(a.PartPrefix == b.PartPrefix && a.Gender == b.Gender && a.ModelID == b.ModelID);
            }

            public override bool Equals([NotNullWhen(true)] object obj)
            {
                if (obj is DirectEquipInfo asEquipInfo)
                    return this == asEquipInfo;

                return base.Equals(obj);
            }
        }

        public DirectEquipInfo lastDirectEquipLoaded;
        public DirectEquipInfo DirectEquip;

        private string DirectEquipGetPartFileNameStart()
        {
            switch (EquipSlotType)
            {
                case EquipSlotTypes.Head: return "HD";
                case EquipSlotTypes.Body: return "BD";
                case EquipSlotTypes.Arms: return "AM";
                case EquipSlotTypes.Legs: return "LG";
                case EquipSlotTypes.Face: return "FC";
                case EquipSlotTypes.Hair: return "HR";
                case EquipSlotTypes.Facegen1:
                case EquipSlotTypes.Facegen2:
                case EquipSlotTypes.Facegen3:
                case EquipSlotTypes.Facegen4:
                case EquipSlotTypes.Facegen5:
                case EquipSlotTypes.Facegen6:
                case EquipSlotTypes.Facegen7:
                case EquipSlotTypes.Facegen8:
                case EquipSlotTypes.Facegen9:
                case EquipSlotTypes.Facegen10:
                    return "FG";

            }
            return null;
        }

        public string GetPartsbndName(bool isFemale, PartSuffixType suffixType)
        {
            if (UsesEquipParam)
            {
                return EquipParam?.GetPartsbndName(isFemale, suffixType);
            }
            else
            {
                return DirectEquipGetPartsbndName(isFemale, suffixType);
            }
        }

        public IBinder GetPartsbnd(bool isFemale, PartSuffixType suffixType, bool disableCache)
        {
            if (EquipSlotType == EquipSlotTypes.OldShittyFacegen)
            {
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DES)
                    zzz_DocumentManager.CurrentDocument.TexturePool.AddTpfFromPath("/facegen/facegen.tpf");
                var fgbnd = zzz_DocumentManager.CurrentDocument.GameData.ReadBinder("/facegen/FaceGen.fgbnd", disableCache: disableCache);
                var tpfBytes = fgbnd.Files.FirstOrDefault(f => f.Name.ToLower().EndsWith(".tpf"))?.Bytes;
                if (tpfBytes != null)
                    zzz_DocumentManager.CurrentDocument.TexturePool.AddTpf(TPF.Read(tpfBytes));
                return fgbnd;
            }


            if (UsesEquipParam)
            {
                return EquipParam?.GetPartsbnd(isFemale, suffixType, disableCache);
            }
            else
            {
                var partName = DirectEquipGetPartsbndName(isFemale, suffixType);
                if (partName != null)
                    return zzz_DocumentManager.CurrentDocument.GameData.ReadBinder($@"/parts/{partName}", disableCache: disableCache);
            }
            return null;
        }

        public bool CanEquipOnGender(bool isFemale)
        {
            if (EquipSlotType == EquipSlotTypes.OldShittyFacegen)
                return true;

            if (UsesEquipParam)
            {
                return EquipParam?.CanEquipOnGender(isFemale) ?? false;
            }
            else
            {
                if (DirectEquip.Gender == DirectEquipGender.UnisexUseA || DirectEquip.Gender == DirectEquipGender.BothGendersUseMF || DirectEquip.Gender == DirectEquipGender.UnisexUseMForBoth)
                    return true;
                else if (DirectEquip.Gender == DirectEquipGender.MaleOnlyUseM)
                    return isFemale == false;
                else if (DirectEquip.Gender == DirectEquipGender.FemaleOnlyUseF)
                    return isFemale == true;
                else
                    return false;
            }
            
        }

        private string DirectEquipGetPartsbndName_WithSuffix(bool isFemale, PartSuffixType suffixType, bool ignoreSuffix)
        {
            string start = DirectEquipGetPartFileNameStart();

            if (DirectEquip.PartPrefix != DirectEquipPartPrefix.None)
                start = DirectEquip.PartPrefix.ToString();

            if (start == null)
                return null;

            if (DirectEquip.ModelID < 0 || DirectEquip.Gender == DirectEquipGender.Invalid)
                return null;

            string _suffix = "";
            if (suffixType == PartSuffixType.M && zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeSupportsPartsSuffixM)
                _suffix = "_M";
            else if (suffixType is PartSuffixType.L && zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeSupportsPartsSuffixL)
                _suffix = "_L";
            else if (suffixType is PartSuffixType.U && zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeSupportsPartsSuffixU)
                _suffix = "_U";

            // Suffix M cannot be ignored lol
            if (ignoreSuffix && suffixType != PartSuffixType.M)
                _suffix = "";

            switch (DirectEquip.Gender)
            {
                case DirectEquipGender.UnisexUseA:
                    return $"{start}_A_{DirectEquip.ModelID:D4}{_suffix}.partsbnd.dcx";
                case DirectEquipGender.MaleOnlyUseM:
                case DirectEquipGender.UnisexUseMForBoth:
                    return $"{start}_M_{DirectEquip.ModelID:D4}{_suffix}.partsbnd.dcx";
                case DirectEquipGender.FemaleOnlyUseF:
                    return $"{start}_F_{DirectEquip.ModelID:D4}{_suffix}.partsbnd.dcx";
                case DirectEquipGender.BothGendersUseMF:
                    return $"{start}_{(isFemale ? "F" : "M")}_{DirectEquip.ModelID:D4}{_suffix}.partsbnd.dcx";
            }

            return null;
        }

        private string DirectEquipGetPartsbndName(bool isFemale, PartSuffixType suffixType)
        {
            var nameWithSuffix = DirectEquipGetPartsbndName_WithSuffix(isFemale, suffixType, ignoreSuffix: false);
            if (nameWithSuffix == null)
                return null;

            if (zzz_DocumentManager.CurrentDocument.GameData.FileExists($"/parts/{nameWithSuffix}"))
                return nameWithSuffix;
            else
                return DirectEquipGetPartsbndName_WithSuffix(isFemale, suffixType, ignoreSuffix: true);
        }

        public class ModelManipStruct
        {
            public Model Model;
        }

        public void ManipModel(Action<ModelManipStruct> doManip)
        {
            lock (_lock_MODEL)
            {
                var manipStruct = new ModelManipStruct()
                {
                    Model = model,
                };
                doManip(manipStruct);
                model = manipStruct.Model;
            }
        }

        public override void TryToLoadTextures()
        {
            lock (_lock_MODEL)
            {
                model?.TryToLoadTextures();
            }
        }

        protected override void InnerDispose()
        {
            lock (_lock_MODEL)
            {
                model?.Dispose();
                model = null;
                
            }

            ASM = null;
        }
    }
}
