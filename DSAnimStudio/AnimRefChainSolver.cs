using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class AnimRefChainSolver
    {
        private List<(TAE tae, TAE.Animation anim)> RefChain = new List<(TAE tae, TAE.Animation anim)>();
        private IReadOnlyDictionary<string, TAE> TaeDict = null;
        private IReadOnlyDictionary<string, byte[]> HKXDict = null;
        private HKX.HKXVariation Game = HKX.HKXVariation.HKXDS1;

        public AnimRefChainSolver(HKX.HKXVariation game, IReadOnlyDictionary<string, TAE> taeDict, IReadOnlyDictionary<string, byte[]> hkxDict)
        {
            Game = game;
            TaeDict = taeDict;
            HKXDict = hkxDict;
        }

        private (long Upper, long Lower) GetSplitAnimID(long id)
        {
            return ((Game == HKX.HKXVariation.HKXBloodBorne || Game == HKX.HKXVariation.HKXDS3) ? (id / 1000000) : (id / 10000), 
                (Game == HKX.HKXVariation.HKXBloodBorne || Game == HKX.HKXVariation.HKXDS3) ? (id % 1000000) : (id % 10000));
        }

        private bool DoesAnimExist(int compositeID)
        {
            var name = HKXNameFromCompositeID(compositeID);
            foreach (var kvp in HKXDict)
            {
                if (kvp.Key.ToUpper().EndsWith(name.ToUpper()))
                    return true;
            }
            return false;
        }

        public List<string> GetRefChainStrings()
        {
            var result = new List<string>();
            foreach (var rc in RefChain)
            {
                if (TaeDict.Count > 1)
                {
                    if (Game == HKX.HKXVariation.HKXBloodBorne || Game == HKX.HKXVariation.HKXDS3)
                    {
                        result.Add($"a{GetTAEID(rc.tae):D3}_{rc.anim.ID:D6}");
                    }
                    else
                    {
                        result.Add($"a{GetTAEID(rc.tae):D2}_{rc.anim.ID:D4}");
                    }
                }
                else
                {
                    var split = GetSplitAnimID(rc.anim.ID);

                    if (Game == HKX.HKXVariation.HKXBloodBorne || Game == HKX.HKXVariation.HKXDS3)
                    {
                        result.Add($"a{split.Upper:D3}_{split.Lower:D6}");
                    }
                    else
                    {
                        result.Add($"a{split.Upper:D2}_{split.Lower:D4}");
                    }
                }
                
            }
            return result;
        }

        private TAE GetTAE(long id)
        {
            foreach (var kvp in TaeDict)
            {
                if (kvp.Key.ToUpper().EndsWith($"{id:D2}.TAE"))
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        private TAE.Animation GetAnimInTAE(TAE tae, long id)
        {
            foreach (var a in tae.Animations)
            {
                if (a.ID == id)
                    return a;
            }
            return null;
        }

        private (long UpperID, TAE.Animation Anim) GetTAEAnim((long Upper, long Lower) id)
        {
            if (TaeDict.Count > 1)
            {
                var tae = GetTAE(id.Upper);
                if (tae != null)
                {
                    var anim = GetAnimInTAE(tae, id.Lower);
                    if (anim != null)
                        return (id.Upper, anim);
                }
            }
            else
            {
                var tae = TaeDict.First().Value;
                var anim = GetAnimInTAE(tae, GetCompositeAnimID(id));
                if (anim != null)
                    return (id.Upper, anim);
            }
            return (0, null);
        }

        private long GetCompositeAnimID((long Upper, long Lower) id)
        {
            if (Game == HKX.HKXVariation.HKXDS3 || Game == HKX.HKXVariation.HKXBloodBorne)
            {
                return (id.Upper * 1_000000) + (id.Lower % 1_000000);
            }
            else
            {
                return (id.Upper * 1_0000) + (id.Lower % 1_0000);
            }
        }

        private (long UpperID, TAE.Animation Anim) CheckForRef1(long animUpperID, TAE.Animation anim)
        {
            var compositeAnimID = GetCompositeAnimID((animUpperID, anim.ID));

            if (anim.Unknown1 == compositeAnimID || anim.Unknown1 <= 256)
                return (0, null);

            var refID = GetSplitAnimID(anim.Unknown1);
            return GetTAEAnim(refID);
        }

        private (long UpperID, TAE.Animation Anim)  CheckForRef2(long animUpperID, TAE.Animation anim)
        {
            var compositeAnimID = GetCompositeAnimID((animUpperID, anim.ID));

            if (anim.Unknown2 == compositeAnimID || anim.Unknown2 <= 256)
                return (0, null);

            var refID = GetSplitAnimID(anim.Unknown2);
            return GetTAEAnim(refID);
        }

        private long GetTAEID(TAE tae)
        {
            foreach (var kvp in TaeDict)
            {
                if (kvp.Value == tae)
                {
                    return long.Parse(Utils.GetFileNameWithoutAnyExtensions(Utils.GetFileNameWithoutDirectoryOrExtension(kvp.Key)).Substring(1));
                }
            }
            return -1;
        }

        private long GetReferencedAnimCompositeID(TAE tae, TAE.Animation anim)
        {
            RefChain.Add((tae, anim));

            if (anim.Unknown2 > 256 && DoesAnimExist(anim.Unknown2))
            {
                return anim.Unknown2;
            }

            if (anim.AnimFileReference)
            {
                if (TaeDict.Count > 1)
                {
                    var animRef1 = CheckForRef1(GetTAEID(tae), anim);
                    if (animRef1.Anim != null)
                    {
                        return GetReferencedAnimCompositeID(GetTAE(animRef1.UpperID), animRef1.Anim);
                    }
                }
                else
                {
                    var animRef1 = CheckForRef1(GetSplitAnimID(anim.ID).Upper, anim);
                    if (animRef1.Anim != null)
                    {
                        return GetReferencedAnimCompositeID(tae, animRef1.Anim);
                    }
                }
            }

            if (TaeDict.Count > 1)
            {
                var taeid = GetTAEID(tae);
                return GetCompositeAnimID((taeid, anim.ID));
            }
            else
            {
                return anim.ID;
            }
        }

        private string HKXNameFromCompositeID(long compositeID)
        {
            var splitID = GetSplitAnimID(compositeID);

            if (Game == HKX.HKXVariation.HKXBloodBorne || Game == HKX.HKXVariation.HKXDS3)
            {
                return $"a{splitID.Upper:D3}_{splitID.Lower:D6}.hkx";
            }
            else
            {
                return $"a{splitID.Upper:D2}_{splitID.Lower:D4}.hkx";
            }
        }

        public string GetHKXName(TAE tae, TAE.Animation anim)
        {
            var id = GetReferencedAnimCompositeID(tae, anim);
            return HKXNameFromCompositeID(id);
        }
    }
}
