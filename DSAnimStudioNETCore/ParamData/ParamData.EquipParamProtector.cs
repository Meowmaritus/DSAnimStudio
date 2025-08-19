using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSAnimStudio.ParamData.AtkParam;
using static DSAnimStudio.ParamData.WepAbsorpPosParam;

namespace DSAnimStudio
{
    public abstract partial class ParamData
    {
        public class EquipParamProtector : ParamData
        {
            public short EquipModelID;
            public EquipModelGenders EquipModelGender;
            public bool HeadEquip;
            public bool BodyEquip;
            public bool ArmEquip;
            public bool LegEquip;
            public List<bool> InvisibleFlags = new List<bool>();

            public int DefMaterialType;

            public bool CanEquipOnGender(bool isFemale)
            {
                if (EquipModelGender == EquipModelGenders.UnisexUseA || EquipModelGender == EquipModelGenders.BothGendersUseMF || EquipModelGender == EquipModelGenders.UnisexUseMForBoth)
                    return true;
                else if (EquipModelGender == EquipModelGenders.MaleOnlyUseM)
                    return isFemale == false;
                else if (EquipModelGender == EquipModelGenders.FemaleOnlyUseF)
                    return isFemale == true;
                else
                    return false;
            }

            public bool[] ApplyInvisFlagsToMask(bool[] mask)
            {
                for (int i = 0; i < InvisibleFlags.Count; i++)
                {
                    if (i > mask.Length)
                        break;

                    if (InvisibleFlags[i])
                        mask[i] = false;
                }

                return mask;
            }

            private string GetPartFileNameStart()
            {
                if (HeadEquip)
                    return "HD";
                else if (BodyEquip)
                    return "BD";
                else if (ArmEquip)
                    return "AM";
                else if (LegEquip)
                    return "LG";
                else
                    return null;
            }

            

            public IBinder GetPartsbnd(bool isFemale, PartSuffixType suffixType, bool disableCache)
            {
                var partName = GetPartsbndName(isFemale, suffixType);
                if (partName != null)
                    return zzz_DocumentManager.CurrentDocument.GameData.ReadBinder($@"/parts/{partName}", disableCache: disableCache);
                return null;
            }

            private string GetPartsbndName_WithSuffix(bool isFemale, PartSuffixType suffixType, bool ignoreSuffix)
            {
                string start = GetPartFileNameStart();

                if (start == null)
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

                switch (EquipModelGender)
                {
                    case EquipModelGenders.UnisexUseA:

                        return $"{start}_A_{EquipModelID:D4}{_suffix}.partsbnd.dcx";
                    case EquipModelGenders.MaleOnlyUseM:
                    case EquipModelGenders.UnisexUseMForBoth:
                        return $"{start}_M_{EquipModelID:D4}{_suffix}.partsbnd.dcx";
                    case EquipModelGenders.FemaleOnlyUseF:
                        return $"{start}_F_{EquipModelID:D4}{_suffix}.partsbnd.dcx";
                    case EquipModelGenders.BothGendersUseMF:
                        return $"{start}_{(isFemale ? "F" : "M")}_{EquipModelID:D4}{_suffix}.partsbnd.dcx";
                }

                return null;
            }

            public string GetPartsbndName(bool isFemale, PartSuffixType suffixType)
            {
                var nameWithSuffix = GetPartsbndName_WithSuffix(isFemale, suffixType, ignoreSuffix: false);
                if (nameWithSuffix == null)
                    return null;
                if (zzz_DocumentManager.CurrentDocument.GameData.FileExists($"/parts/{nameWithSuffix}"))
                    return nameWithSuffix;
                else
                    return GetPartsbndName_WithSuffix(isFemale, suffixType, ignoreSuffix: true);
            }

            public override void Read(BinaryReaderEx br)
            {
                // Empty bytes added at start
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6)
                    br.Position += 4;

                long start = br.Position;

                br.Position += 0xA0;

                EquipModelID = br.ReadInt16();

                br.Position = start + 0xD1;

                EquipModelGender = (EquipModelGenders)br.ReadByte();

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                    EquipModelGender = EquipModelGenders.UnisexUseMForBoth;

                br.Position = start + 0xD8;

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB) //TODO_DES
                {
                    var firstBitmask = ReadBitmask(br, 6 + 48);
                    //IsDeposit = firstBitmask[0]
                    HeadEquip = firstBitmask[1];
                    BodyEquip = firstBitmask[2];
                    ArmEquip = firstBitmask[3];
                    LegEquip = firstBitmask[4];
                    //UseFaceScale = firstBitmask[5];
                    InvisibleFlags.Clear();
                    for (int i = 0; i <= 47; i++)
                    {
                        InvisibleFlags.Add(firstBitmask[i + 6]);
                    }

                    if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
                    {
                        br.Position = start + 0xFD;
                        var mask48to62 = ReadBitmask(br, 15);

                        InvisibleFlags.AddRange(mask48to62);
                    }
                }
                else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
                {
                    br.Position = start + 1;
                    EquipModelGender = (EquipModelGenders)br.ReadByte();
                    EquipModelID = br.ReadInt16();

                    br.Position = start + 0x09;
                    HeadEquip = br.ReadBoolean();
                    BodyEquip = br.ReadBoolean();
                    ArmEquip = br.ReadBoolean();
                    LegEquip = br.ReadBoolean();

                    br.Position = start + 0x50;
                    var firstBitmask = ReadBitmask(br, 32);
                    InvisibleFlags.Clear();
                    for (int i = 0; i < 32; i++)
                    {
                        InvisibleFlags.Add(firstBitmask[i]);
                    }
                }
                else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3
                    or SoulsAssetPipeline.SoulsGames.SDT
                    or SoulsAssetPipeline.SoulsGames.ER
                    or SoulsAssetPipeline.SoulsGames.ERNR
                    or SoulsAssetPipeline.SoulsGames.AC6)
                {
                    var firstBitmask = ReadBitmask(br, 5);
                    //IsDeposit = firstBitmask[0]
                    HeadEquip = firstBitmask[1];
                    BodyEquip = firstBitmask[2];
                    ArmEquip = firstBitmask[3];
                    LegEquip = firstBitmask[4];

                    br.Position = start + 0x12E;

                    for (int i = 0; i < 98; i++)
                    {
                        InvisibleFlags.Add(br.ReadByte() == 1);
                    }
                }

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.BB
                    or SoulsAssetPipeline.SoulsGames.DS1)
                {
                    br.Position = start + 0xD3;
                    DefMaterialType = br.ReadByte();
                }
                else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DES)
                {
                    br.Position = start + 0xD;
                    DefMaterialType = br.ReadByte();
                }
                else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3 
                    or SoulsAssetPipeline.SoulsGames.SDT
                    or SoulsAssetPipeline.SoulsGames.ER
                    or SoulsAssetPipeline.SoulsGames.ERNR
                    or SoulsAssetPipeline.SoulsGames.AC6)
                {
                    br.Position = start + 0x100;
                    DefMaterialType = br.ReadUInt16();
                }

            }
        }

    }
}
