using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeAnimRefChainSolver
    {
        private List<(TAE tae, TAE.Animation anim)> RefChain = new List<(TAE tae, TAE.Animation anim)>();
        private IReadOnlyDictionary<string, TAE> TaeDict = null;
        private IReadOnlyDictionary<string, byte[]> HKXDict = null;
        private SoulsAssetPipeline.SoulsGames Game => GameDataManager.GameType;
        public bool StackLimitHit = false;

        public TaeAnimRefChainSolver(IReadOnlyDictionary<string, TAE> taeDict, IReadOnlyDictionary<string, byte[]> hkxDict)
        {
            TaeDict = taeDict;
            HKXDict = hkxDict;
        }

        bool IsGameLongAnimNames => (Game == SoulsAssetPipeline.SoulsGames.BB || 
            Game == SoulsAssetPipeline.SoulsGames.DS3 || 
            Game == SoulsAssetPipeline.SoulsGames.SDT);

        private (long Upper, long Lower) GetSplitAnimID(long id)
        {
            return (IsGameLongAnimNames ? (id / 1000000) : (id / 10000),
                IsGameLongAnimNames ? (id % 1000000) : (id % 10000));
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
                    if (IsGameLongAnimNames)
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

                    if (IsGameLongAnimNames)
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

        private (long UpperID, TAE.Animation Anim) GetTAEAnim(long compositeId)
        {
            var id = GetSplitAnimID(compositeId);

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
                if (TaeDict.Count > 0)
                {
                    var tae = TaeDict.FirstOrDefault().Value;
                    if (tae != null)
                    {
                        var anim = GetAnimInTAE(tae, GetCompositeAnimID(id));
                        if (anim != null)
                            return (id.Upper, anim);
                    }
                 
                }
                
            }
            return (0, null);
        }

        private long GetCompositeAnimID((long Upper, long Lower) id)
        {
            if (IsGameLongAnimNames)
            {
                return (id.Upper * 1_000000) + (id.Lower % 1_000000);
            }
            else
            {
                return (id.Upper * 1_0000) + (id.Lower % 1_0000);
            }
        }

        //private (long UpperID, TAE.Animation Anim) CheckForRef1(long animUpperID, TAE.Animation anim)
        //{
        //    var compositeAnimID = GetCompositeAnimID((animUpperID, anim.ID));

        //    if (anim.RefVerDirectReferenceID == compositeAnimID || anim.RefVerDirectReferenceID <= 256)
        //        return (0, null);

        //    var refID = GetSplitAnimID(anim.RefVerDirectReferenceID);
        //    return GetTAEAnim(refID);
        //}

        //private (long UpperID, TAE.Animation Anim)  CheckForRef2(long animUpperID, TAE.Animation anim)
        //{
        //    var compositeAnimID = GetCompositeAnimID((animUpperID, anim.ID));

        //    if (anim.SubID == compositeAnimID || anim.SubID <= 256)
        //        return (0, null);

        //    var refID = GetSplitAnimID(anim.SubID);
        //    return GetTAEAnim(refID);
        //}

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

        public long GetCompositeAnimIDOfAnimInTAE(TAE tae, TAE.Animation anim)
        {
            var upper = GetTAEID(tae);
            var lower = anim.ID;

            return GetCompositeAnimID((upper, lower));
        }

        private long GetReferencedAnimCompositeID(TAE tae, TAE.Animation anim, bool ignoreMultiTAE)
        {
            RefChain.Add((tae, anim));

            if (RefChain.Count >= 2 /*turns out it doesnt even chain and too lazy to remove this system lol*/)
            {
                StackLimitHit = true;
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

            //if (anim.RefVerDirectReferenceID == 256)
            //{
            //    if (DoesAnimExist(anim.SubID))
            //        return anim.SubID;
            //    else
            //    {
            //        if (TaeDict.Count > 1 && !ignoreMultiTAE)
            //        {
            //            var animRef2 = CheckForRef2(GetTAEID(tae), anim);
            //            if (animRef2.Anim != null)
            //                return GetReferencedAnimCompositeID(GetTAE(animRef2.UpperID), animRef2.Anim, ignoreMultiTAE);
            //        }
            //        else
            //        {
            //            var animRef2 = CheckForRef2(GetSplitAnimID(anim.ID).Upper, anim);
            //            if (animRef2.Anim != null)
            //                return GetReferencedAnimCompositeID(tae, animRef2.Anim, ignoreMultiTAE);
            //        }
            //    }
            //}

            if (anim.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOtherAnim)
            {
                var importAnim = GetTAEAnim(asImportOtherAnim.ImportFromAnimID);

                if (importAnim.Anim != null)
                {
                    return GetReferencedAnimCompositeID(GetTAE(importAnim.UpperID), importAnim.Anim, ignoreMultiTAE);
                }
            }
            else if (anim.MiniHeader is TAE.Animation.AnimMiniHeader.Standard asStandard)
            {
                if (asStandard.ImportsHKX)
                {
                    if (asStandard.ImportHKXSourceAnimID > 0)
                    {
                        var importAnim = GetTAEAnim(asStandard.ImportHKXSourceAnimID);

                        if (importAnim.Anim != null)
                        {
                            return GetReferencedAnimCompositeID(GetTAE(importAnim.UpperID), importAnim.Anim, ignoreMultiTAE);
                        }
                        else
                        {
                            return asStandard.ImportHKXSourceAnimID;
                        }
                    }
                    else
                    {
                        return asStandard.ImportHKXSourceAnimID;
                    }
                }
            }

            //if (TaeDict.Count > 1 && !ignoreMultiTAE)
            //{
            //    var animRef1 = CheckForRef1(GetTAEID(tae), anim);
            //    if (animRef1.Anim != null)
            //        return GetReferencedAnimCompositeID(GetTAE(animRef1.UpperID), animRef1.Anim, ignoreMultiTAE);
            //}
            //else
            //{
            //    var animRef1 = CheckForRef1(GetSplitAnimID(anim.ID).Upper, anim);
            //    if (animRef1.Anim != null)
            //        return GetReferencedAnimCompositeID(tae, animRef1.Anim, ignoreMultiTAE);
            //}

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

        public string HKXNameFromCompositeID(long compositeID)
        {
            var splitID = GetSplitAnimID(compositeID);

            if (IsGameLongAnimNames)
            {
                return $"a{splitID.Upper:D3}_{splitID.Lower:D6}.hkx";
            }
            else
            {
                return $"a{splitID.Upper:D2}_{splitID.Lower:D4}.hkx";
            }
        }

        public string GetHKXName(TAE tae, TAE.Animation anim, bool ignoreMultiTAE = false)
        {
            try
            {
                var id = GetReferencedAnimCompositeID(tae, anim, ignoreMultiTAE);
                return HKXNameFromCompositeID(id);
            }
            catch
            {
                return "?AnimName?";
            }
            
        }

        public string GetHKXNameIgnoreReferences(TAE tae, TAE.Animation anim)
        {
            return HKXNameFromCompositeID(GetCompositeAnimIDOfAnimInTAE(tae, anim));
        }
    }
}
