using DSAnimStudio.TaeEditor;
using SoulsAssetPipeline.Animation;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;

namespace DSAnimStudio
{
    public class NewChrAsmWpnTaeManager
    {
        //public event EventHandler<int> HkxSelected;
        //private void OnHkxSelected(int hkx)
        //{
        //    HkxSelected?.Invoke(this, hkx);
        //}

        private float prevFrameTime = 0;

        public readonly int ModelIndex = 0;
        public Model Mdl;
        public TaeActionSimulationEnvironment ActionSim;
        public NewChrAsmWpnTaeManager(int modelIndex)
        {
            ModelIndex = modelIndex;
        }

        //public NewGraph Graph;

        private IBinder Partsbnd;
        private BinderFile PartsbndBinderfileAnibnd;
        private IBinder Anibnd;
        private BinderFile AnibndBinderFileTae;

        public DSAProj.AnimCategory TaeCategory;
        public DSAProj.Animation Anim;
        public DSAProj FakeProj;
        public zzz_DocumentIns FakeDoc = new zzz_DocumentIns(null);

        public SplitAnimID GetDefaultAnimID(DSAProj proj)
        {
            
            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.DES)
            {
                return SplitAnimID.FromFullID(proj, 00_0000);
            }
            else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.DS1 or SoulsGames.DS1R)
            {
                return SplitAnimID.FromFullID(proj, 00_0000);
            }
            else
            {
                return SplitAnimID.FromFullID(proj, 999_000000);
            }
        }

        public DSAProj.Animation GetTaeAnimSolveRefs(zzz_DocumentIns doc, SplitAnimID taeAnimID)
        {
            if (TaeCategory == null)
            {
                if (FakeProj == null)
                {
                    FakeProj = new DSAProj(FakeDoc);
                    FakeDoc.FakeProj = FakeProj;
                }
                TaeCategory = new DSAProj.AnimCategory(FakeProj);
            }
            
            // Add dummy TAE entries for any HKXs that exist but aren't in the TAE, ghetto but should hopefully work xd
            var hkxs = Mdl.AnimContainer.GetAllAnimationIDs();
            var animationList = TaeCategory.INNER_GetAnimations();
            foreach (var h in hkxs)
            {
                if (!animationList.Any(a => a.SplitID == h))
                {
                    if (FakeProj == null)
                    {
                        FakeProj = new DSAProj(FakeDoc);
                        FakeDoc.FakeProj = FakeProj;
                    }
                    var newAnim = new DSAProj.Animation(FakeProj, TaeCategory, h,
                        new TAE.Animation.AnimFileHeader.Standard() { IsNullHeader = true });
                    TaeCategory.INNER_AddAnimation(newAnim);
                }
            }

            animationList = TaeCategory.INNER_GetAnimations();
            
            DSAProj.Animation selectAnimation(SplitAnimID id)
            {
                foreach (var anim in animationList)
                {
                    if (anim.SplitID == id)
                    {
                        return anim;
                    }
                }
                return null;
            }

            DSAProj.Animation selectReferencedAnimation(DSAProj.Animation anim)
            {
                var headerClone = anim.INNER_GetHeaderClone();
                if (headerClone is SoulsAssetPipeline.Animation.TAE.Animation.AnimFileHeader.ImportOtherAnim asImportOtherAnim)
                {
                    if (asImportOtherAnim.ImportFromAnimID >= 0)
                    {
                        var refAnim = selectAnimation(SplitAnimID.FromFullID(doc.GameRoot, asImportOtherAnim.ImportFromAnimID));
                        if (refAnim != null)
                            return selectReferencedAnimation(refAnim);
                    }
                }

                return anim;
            }

            DSAProj.Animation anim = null;
            //SplitAnimID hkxAnimID = taeAnimID;
            //string hkxName = null;
            
            void selectTaeAnimID(SplitAnimID _taeAnimID)
            {
                anim = selectAnimation(_taeAnimID);

                // Default to going directly to HKX and only use TAE if it exists
                //hkxAnimID = _taeAnimID;

                if (anim != null)
                {
                    anim = selectReferencedAnimation(anim);
                    //hkxAnimID = (int)anim.NewID;
                    //if (anim.Header is SoulsAssetPipeline.Animation.TAE.Animation.AnimFileHeader.Standard asStandard)
                    //{
                    //    //if (asStandard.ImportsHKX)
                    //    //    hkxAnimID = asStandard.ImportHKXSourceAnimID;
                    //}
                }
                //hkxName = (hkxAnimID >= 0 ? hkxAnimID.ToString() + ".hkx" : null);
            }
            
            if (TaeCategory != null)
                selectTaeAnimID(taeAnimID);

            if (anim == null)
                selectTaeAnimID(GetDefaultAnimID(doc.Proj));
            
            return anim;
        }


        public void SelectTaeAnimation(zzz_DocumentIns doc, SplitAnimID taeAnimID, bool forceNew)
        {
            Mdl.AnimContainer.EquipmentTaeManager = this;
            
            var anim = GetTaeAnimSolveRefs(doc, taeAnimID);


            if (anim == null)
            {
                Mdl.AnimContainer.ForcePlayAnim = false;
                        
                Mdl.AnimContainer.RequestAnim(NewAnimSlot.SlotTypes.Base, SplitAnimID.Invalid, forceNew, animWeight: 1, startTime: 0,
                    blendDuration: anim?.INNER_GetBlendDuration() ?? 0);
                            

                Mdl.AnimContainer.EnableLooping = false;
            }
            else
            {
                Mdl.AnimContainer.ForcePlayAnim = false;
                        
                Mdl.AnimContainer.RequestAnim(NewAnimSlot.SlotTypes.Base, taeAnimID, forceNew, animWeight: 1, startTime: 0,
                    blendDuration: anim?.INNER_GetBlendDuration() ?? 0);
                            

                Mdl.AnimContainer.EnableLooping = false;
            }
            
            
            
            
            //
            // if (hkxAnimID < 0 || !Mdl.AnimContainer.AnimExists(hkxAnimID))
            //     selectTaeAnimID(GameRoot.GameTypeHasLongAnimIDs ? 000_000000 : 00_0000);
            // if (hkxAnimID < 0 || !Mdl.AnimContainer.AnimExists(hkxAnimID))
            //     selectTaeAnimID(GameRoot.GameTypeHasLongAnimIDs ? 999_000000 : 99_0000);
            // if (hkxAnimID < 0 || !Mdl.AnimContainer.AnimExists(hkxAnimID))
            //     selectTaeAnimID(GameRoot.GameTypeHasLongAnimIDs ? 999_999999 : 99_9999);
            //
            // if (hkxAnimID < 0 || !Mdl.AnimContainer.AnimExists(hkxAnimID))
            //     hkxAnimID = (GameRoot.GameTypeHasLongAnimIDs ? 000_000000 : 00_0000);
            // if (hkxAnimID < 0 || !Mdl.AnimContainer.AnimExists(hkxAnimID))
            //     hkxAnimID = (GameRoot.GameTypeHasLongAnimIDs ? 999_000000 : 99_0000);
            // if (hkxAnimID < 0 || !Mdl.AnimContainer.AnimExists(hkxAnimID))
            //     hkxAnimID = (GameRoot.GameTypeHasLongAnimIDs ? 999_999999 : 99_9999);
            //
            //
            // //Mdl.AnimContainer.ClearAnimation();
            //
            // if (Mdl.AnimContainer.Skeleton?.OriginalHavokSkeleton == null)
            // {
            //     Mdl.SkeletonFlver?.RevertToReferencePose();
            // }
            // else
            // {
            //
            //     if (hkxAnimID >= 0 && Mdl.AnimContainer.AnimExists(hkxAnimID))
            //     {
            //
            //         if (Mdl != null && Mdl.AnimContainer != null)
            //         {
            //             Mdl.AnimContainer.ForcePlayAnim = false;
            //             
            //             Mdl.AnimContainer.RequestAnim(NewAnimSlot.SlotTypes.Base, taeAnimID, forceNew: true, animWeight: 1, startTime: 0,
            //                 blendDuration: anim?.GetBlendDuration() ?? 0);
            //                 
            //
            //             Mdl.AnimContainer.EnableLooping = false;
            //         }
            //     }
            //     else
            //     {
            //         Mdl.AnimContainer.ClearAnimation();
            //     }
            // }







            Anim = anim;

            Mdl.NewForceSyncUpdate();
        }

        // public void SavePartsbnd(string partsbndPath)
        // {
        //     AnibndBinderFileTae.Bytes = Tae.ToBinary();
        //
        //     if (Anibnd is BND3 anibndAsBND3)
        //         PartsbndBinderfileAnibnd.Bytes = anibndAsBND3.Write();
        //     else if (Anibnd is BND4 anibndAsBND4)
        //         PartsbndBinderfileAnibnd.Bytes = anibndAsBND4.Write();
        //
        //     if (Partsbnd is BND3 partsbndAsBND3)
        //         partsbndAsBND3.Write(partsbndPath);
        //     else if (Partsbnd is BND4 partsbndAsBND4)
        //         partsbndAsBND4.Write(partsbndPath);
        // }

        public void UpdateTae()
        {
            float time = Mdl.AnimContainer.CurrentAnimTime;
            float duration = Mdl.AnimContainer.CurrentAnimDuration ?? 0.001f;
            if (Anim != null)
                ActionSim.OnSimulationFrameChange(graphOptional: null, TaeCategory.INNER_FindFirstAnimByFullID, Anim.SplitID, false, ignoreBlendProcess: true);
            prevFrameTime = time;
        }
        
        public static NewChrAsmWpnTaeManager LoadPartsbnd(zzz_DocumentIns doc, Model mdl, int modelIndex, IBinder partsbnd)
        {
            var m = new NewChrAsmWpnTaeManager(modelIndex);
            m.Mdl = mdl;
            m.Partsbnd = partsbnd;
            m.ActionSim = new TaeActionSimulationEnvironment(doc, mdl);
            foreach (var f in m.Partsbnd.Files)
            {
                if ((modelIndex >= 0 && f.ID == 400 + m.ModelIndex) || (modelIndex < 0 && f.Name.Trim().ToLower().EndsWith(".anibnd")))
                {
                    m.PartsbndBinderfileAnibnd = f;
                    break;
                }
            }

            if (m.PartsbndBinderfileAnibnd != null)
            {
                if (BND3.IsRead(m.PartsbndBinderfileAnibnd.Bytes, out BND3 anibndAsBND3))
                    m.Anibnd = anibndAsBND3;
                else if (BND4.IsRead(m.PartsbndBinderfileAnibnd.Bytes, out BND4 anibndAsBND4))
                    m.Anibnd = anibndAsBND4;



                foreach (var f in m.Anibnd.Files)
                {
                    if (f.ID == 3000000)
                    {
                        m.AnibndBinderFileTae = f;
                        break;
                    }
                }



                if (m.AnibndBinderFileTae != null)
                {
                    if (m.FakeProj == null)
                    {
                        m.FakeProj = new DSAProj(m.FakeDoc);
                        m.FakeDoc.FakeProj = m.FakeProj;
                    }
                    m.TaeCategory = new DSAProj.AnimCategory(m.FakeProj);

                    var tae = SoulsAssetPipeline.Animation.TAE.Read(m.AnibndBinderFileTae.Bytes);
                    foreach (var anim in tae.Animations)
                    {
                        m.TaeCategory.INNER_AddAnimation(DSAProj.Animation.LegacyFromBinary(doc.Proj, anim, m.TaeCategory));
                    }
                    //if (m.Tae.Animations.Count > 0)
                    //{
                    //    m.Graph = new NewGraph(mainScreen, isGhostGraph: false, startingAnimRef: m.Tae.Animations[0], isAdditionalGraph: true);
                    //    m.Graph.EventSim = new TaeEventSimulationEnvironment(m.Graph, m.Mdl);
                    //    m.Graph.GraphTabName = Utils.GetShortIngameFileName(partsbndName);
                    //    if (m.ModelIndex > 0)
                    //    {
                    //        m.Graph.GraphTabName += $"_{m.ModelIndex}";
                    //    }
                    //    mainScreen.Graph.RegistChildGraph(m.Graph);
                    //}
                    //return m;
                }
                return m;
                
            }

            return null;

        }





    }
}
