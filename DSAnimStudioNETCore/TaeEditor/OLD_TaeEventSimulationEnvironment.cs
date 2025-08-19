//using DSAnimStudio.TaeEditor;
//using Microsoft.Xna.Framework;
//using SoulsAssetPipeline.Animation;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DSAnimStudio.TaeEditor
//{
//    public class OLD_TaeEventSimulationEnvironment
//    {
//        public static void GlobalPlaybackInstanceStop()
//        {
//            SoundManager.StopAllSounds();
//            RumbleCamManager.ClearActive();
//        }

//        public Model MODEL;
//        public NewGraph Graph;

//        private float AnimBlendLength = 0;

//        public bool SimEnabled_BasicBlending = true;
//        public bool SimEnabled_BasicBlending_ComboViewer = true;
//        public bool SimEnabled_Hitboxes = true;

//        public void NewSimulationInit(List<TAE.Event> events)
//        {
//            AnimBlendLength = 0;

//            foreach (var ev in events)
//            {
//                if (ev.Type == 16)
//                    AnimBlendLength = ev.EndTime - ev.StartTime;
//            }


//        }

//        public void NewSimulationScrub(float time, List<TAE.Event> events)
//        {
//            bool isInComboViewMode = (Graph?.ViewportInteractor?.CurrentComboIndex ?? -1) >= 0;

//            if (SimEnabled_BasicBlending && !isInComboViewMode)
//            {
//                float blendS = 1;
//                if (time < AnimBlendLength)
//                    blendS = time / AnimBlendLength;
//                NewDoEvent_BasicBlend(blendS);
//            }
//            else if (SimEnabled_BasicBlending_ComboViewer && isInComboViewMode)
//            {
//                float blendS = 1;
//                if (time < AnimBlendLength)
//                    blendS = time / AnimBlendLength;
//                DoEvent_BasicBlend_ComboViewer(blendS);
//            }
            
//            foreach (var ev in events)
//            {
//                if (ev.Type == 1 && SimEnabled_Hitboxes)
//                {

//                }
//            }
//        }


//        private void NewDoEvent_BasicBlend(float blendS)
//        {
//            void doAnimContainer(NewAnimationContainer animContainer)
//            {
//                animContainer.ForceDisableAnimLayerSystem = false;

//                lock (animContainer._lock_AnimationLayers)
//                {

//                    var layersCopy_Base = animContainer.AnimationLayers.Where(la => !la.IsUpperBody).ToList();
//                    var layersCopy_UpperBody = animContainer.AnimationLayers.Where(la => la.IsUpperBody).ToList();

//                    float referenceWeightTotal_Base = 0;
//                    for (int i = 0; i < layersCopy_Base.Count - 1; i++)
//                    {
//                        referenceWeightTotal_Base += layersCopy_Base[i].ReferenceWeight;
//                    }

//                    float referenceWeightTotal_UpperBody = 0;
//                    for (int i = 0; i < layersCopy_UpperBody.Count - 1; i++)
//                    {
//                        referenceWeightTotal_UpperBody += layersCopy_UpperBody[i].ReferenceWeight;
//                    }

//                    float blendToActiveAnimRatio = blendS;

//                    if (animContainer.AnimationLayers.Count > 1)
//                    {
//                        //var blend = evBoxes.FirstOrDefault(b => b.Type == 16);
//                        //if (blend != null)
//                        //{
//                        //    blendToActiveAnimRatio = MathHelper.Clamp((((float)Graph.PlaybackCursor.GUICurrentTime - blend.StartTime) / (blend.EndTime - blend.StartTime)), 0, 1);
//                        //}
//                        //else
//                        //{
//                        //    blendToActiveAnimRatio = 1;
//                        //    animContainer.RemoveCurrentTransitions(true, true);
//                        //}

//                        if (Graph.ViewportInteractor.CurrentComboBlendFramesAreSine_Base)
//                        {
//                            blendToActiveAnimRatio = 1 - (float)(Math.Cos(blendToActiveAnimRatio * Math.PI) / 2.0 + 0.5);
//                        }

//                        float inactiveReferenceWeightTotal = 1 - blendToActiveAnimRatio;

//                        for (int i = 0; i < layersCopy_Base.Count; i++)
//                        {
//                            if (i == layersCopy_Base.Count - 1)
//                            {
//                                layersCopy_Base[i].Weight = blendToActiveAnimRatio;
//                            }
//                            else
//                            {
//                                float inactiveAnimRefWeightRatio = referenceWeightTotal_Base != 0 ? (layersCopy_Base[i].ReferenceWeight / referenceWeightTotal_Base) : 0;
//                                layersCopy_Base[i].Weight = inactiveAnimRefWeightRatio * inactiveReferenceWeightTotal;
//                            }
//                        }

//                        for (int i = 0; i < layersCopy_UpperBody.Count; i++)
//                        {
//                            if (i == layersCopy_UpperBody.Count - 1)
//                            {
//                                layersCopy_UpperBody[i].Weight = blendToActiveAnimRatio;
//                            }
//                            else
//                            {
//                                float inactiveAnimRefWeightRatio = referenceWeightTotal_UpperBody != 0 ? (layersCopy_UpperBody[i].ReferenceWeight / referenceWeightTotal_UpperBody) : 0;
//                                layersCopy_UpperBody[i].Weight = inactiveAnimRefWeightRatio * inactiveReferenceWeightTotal;
//                            }
//                        }
//                    }
//                    else if (animContainer.AnimationLayers.Count == 1)
//                    {
//                        animContainer.AnimationLayers[0].Weight = 1;
//                        animContainer.AnimationLayers[0].ReferenceWeight = 1;
//                    }


//                }

//                animContainer.RemoveAnimsWithDeadReferenceWeights();
//            }

//            doAnimContainer(Graph.ViewportInteractor.CurrentModel.AnimContainer);
//            Graph.ViewportInteractor.CurrentModel.ChrAsm?.ForeachWeaponModel(wpnMdl => doAnimContainer(wpnMdl?.AnimContainer));
//        }

//        private void DoEvent_BasicBlend_ComboViewer(float blendS)
//        {
//            void doAnimContainer(NewAnimationContainer animContainer)
//            {
//                animContainer.ForceDisableAnimLayerSystem = false;

//                lock (animContainer._lock_AnimationLayers)
//                {

//                    var layersCopy_Base = animContainer.AnimationLayers.Where(la => !la.IsUpperBody).ToList();
//                    var layersCopy_UpperBody = animContainer.AnimationLayers.Where(la => la.IsUpperBody).ToList();

//                    float referenceWeightTotal_Base = 0;
//                    for (int i = 0; i < layersCopy_Base.Count - 1; i++)
//                    {
//                        referenceWeightTotal_Base += layersCopy_Base[i].ReferenceWeight;
//                    }

//                    float referenceWeightTotal_UpperBody = 0;
//                    for (int i = 0; i < layersCopy_UpperBody.Count - 1; i++)
//                    {
//                        referenceWeightTotal_UpperBody += layersCopy_UpperBody[i].ReferenceWeight;
//                    }

//                    float blendToActiveAnimRatio_Base = 0;
//                    float blendToActiveAnimRatio_UpperBody = 0;

//                    if (layersCopy_Base.Count > 1)
//                    {
//                        if (Graph.ViewportInteractor.CurrentComboIndex >= 0 && Graph.ViewportInteractor.CurrentComboBlendFramesOverride_Base >= 0 && Graph.ViewportInteractor.CurrentComboBlendFramesOverrideStartPoint_Base < 0)
//                        {
//                            Graph.ViewportInteractor.CurrentComboBlendFramesOverrideStartPoint_Base = Graph.PlaybackCursor.GUICurrentTime;
//                        }

//                        float comboUserBlendOverride_Base = Graph.ViewportInteractor.GetComboBlendOverrideIfApplicable_Base();

//                        if (comboUserBlendOverride_Base < 0)
//                        {
//                            //var blend = evBoxes.FirstOrDefault(b => b.Type == 16);
//                            //if (blend != null)
//                            //{
//                            //    blendToActiveAnimRatio_Base = MathHelper.Clamp((((float)Graph.PlaybackCursor.GUICurrentTime - blend.StartTime) / (blend.EndTime - blend.StartTime)), 0, 1);
//                            //}
//                            //else
//                            //{
//                            //    blendToActiveAnimRatio_Base = 1;
//                            //    animContainer.RemoveCurrentTransitions(removeBase: true, removeUpper: false);
//                            //}
//                            blendToActiveAnimRatio_Base = blendS;
//                            if (blendToActiveAnimRatio_Base == 0)
//                                animContainer.RemoveCurrentTransitions(removeBase: true, removeUpper: false);
//                        }
//                        else
//                        {
//                            blendToActiveAnimRatio_Base = comboUserBlendOverride_Base;
//                        }

//                        if (Graph.ViewportInteractor.CurrentComboBlendFramesAreSine_Base)
//                        {
//                            blendToActiveAnimRatio_Base = 1 - (float)(Math.Cos(blendToActiveAnimRatio_Base * Math.PI) / 2.0 + 0.5);
//                        }

//                        float inactiveReferenceWeightTotal_Base = 1 - blendToActiveAnimRatio_Base;



//                        for (int i = 0; i < layersCopy_Base.Count; i++)
//                        {
//                            if (i == layersCopy_Base.Count - 1)
//                            {
//                                layersCopy_Base[i].Weight = blendToActiveAnimRatio_Base;
//                            }
//                            else
//                            {
//                                float inactiveAnimRefWeightRatio = referenceWeightTotal_Base != 0 ? (layersCopy_Base[i].ReferenceWeight / referenceWeightTotal_Base) : 0;
//                                layersCopy_Base[i].Weight = inactiveAnimRefWeightRatio * inactiveReferenceWeightTotal_Base;
//                            }
//                        }
//                    }
//                    else if (layersCopy_Base.Count == 1)
//                    {
//                        if (layersCopy_Base[0].Weight != 1)
//                            layersCopy_Base[0].Weight = 1;
//                        if (layersCopy_Base[0].ReferenceWeight != 1)
//                            layersCopy_Base[0].ReferenceWeight = 1;
//                    }

//                    if (layersCopy_UpperBody.Count > 1)
//                    {
//                        if (Graph.ViewportInteractor.CurrentComboIndex >= 0 && Graph.ViewportInteractor.CurrentComboBlendFramesOverride_UpperBody >= 0 && Graph.ViewportInteractor.CurrentComboBlendFramesOverrideStartPoint_UpperBody < 0)
//                        {
//                            Graph.ViewportInteractor.CurrentComboBlendFramesOverrideStartPoint_UpperBody = Graph.PlaybackCursor.GUICurrentTime;
//                        }

//                        float comboUserBlendOverride_UpperBody = Graph.ViewportInteractor.GetComboBlendOverrideIfApplicable_UpperBody();

//                        if (comboUserBlendOverride_UpperBody < 0)
//                        {
//                            //var blend = evBoxes.FirstOrDefault(b => b.Type == 16);
//                            //if (blend != null)
//                            //{
//                            //    blendToActiveAnimRatio_UpperBody = MathHelper.Clamp((((float)Graph.PlaybackCursor.GUICurrentTime - blend.StartTime) / (blend.EndTime - blend.StartTime)), 0, 1);
//                            //}
//                            //else
//                            //{
//                            //    blendToActiveAnimRatio_UpperBody = 1;
//                            //    animContainer.RemoveCurrentTransitions(removeBase: false, removeUpper: true);
//                            //}

//                            blendToActiveAnimRatio_UpperBody = blendS;
//                            if (blendToActiveAnimRatio_UpperBody == 0)
//                                animContainer.RemoveCurrentTransitions(removeBase: false, removeUpper: true);
//                        }
//                        else
//                        {
//                            blendToActiveAnimRatio_UpperBody = comboUserBlendOverride_UpperBody;
//                        }


//                        if (Graph.ViewportInteractor.CurrentComboBlendFramesAreSine_UpperBody)
//                        {
//                            blendToActiveAnimRatio_UpperBody = 1 - (float)(Math.Cos(blendToActiveAnimRatio_UpperBody * Math.PI) / 2.0 + 0.5);
//                        }

//                        float inactiveReferenceWeightTotal_UpperBody = 1 - blendToActiveAnimRatio_UpperBody;

//                        for (int i = 0; i < layersCopy_UpperBody.Count; i++)
//                        {
//                            if (i == layersCopy_UpperBody.Count - 1)
//                            {
//                                layersCopy_UpperBody[i].Weight = blendToActiveAnimRatio_UpperBody;
//                            }
//                            else
//                            {
//                                float inactiveAnimRefWeightRatio = referenceWeightTotal_UpperBody != 0 ? (layersCopy_UpperBody[i].ReferenceWeight / referenceWeightTotal_UpperBody) : 0;
//                                layersCopy_UpperBody[i].Weight = inactiveAnimRefWeightRatio * inactiveReferenceWeightTotal_UpperBody;
//                            }
//                        }
//                    }
//                    else if (layersCopy_UpperBody.Count == 1)
//                    {
//                        if (layersCopy_UpperBody[0].Weight != 1)
//                            layersCopy_UpperBody[0].Weight = 1;
//                        if (layersCopy_UpperBody[0].ReferenceWeight != 1)
//                            layersCopy_UpperBody[0].ReferenceWeight = 1;
//                    }

//                }

//                animContainer.RemoveAnimsWithDeadReferenceWeights();
//            }

//            doAnimContainer(Graph.ViewportInteractor.CurrentModel.AnimContainer);
//            Graph.ViewportInteractor.CurrentModel.ChrAsm?.ForeachWeaponModel(wpnMdl => doAnimContainer(wpnMdl?.AnimContainer));
//        }

//        public static float TaeRootMotionScaleXZ = 1;

//        private ParamData.AtkParam.DummyPolySource HitViewDummyPolySource => 
//            MODEL.IS_PLAYER ? (OverrideHitViewDummyPolySource ?? Graph?.MainScreen.Config.HitViewDummyPolySource ?? ParamData.AtkParam.DummyPolySource.Body) 
//            : ParamData.AtkParam.DummyPolySource.Body;

//        public ParamData.AtkParam.DummyPolySource? OverrideHitViewDummyPolySource = null;

//#if NIGHTFALL
//        public int NF_CurrentDummyPolySoundSlot = 10000;
//        public void NF_IncrementCurrentDummyPolySoundSlot()
//        {
//            NF_CurrentDummyPolySoundSlot++;
//            if (NF_CurrentDummyPolySoundSlot >= 30000)
//                NF_CurrentDummyPolySoundSlot = 10000;
//        }
//#endif


//        public void DoHardReset()
//        {
//            NightfallTaeAddAnimCurWeightAtEventStart = -1;
            
//        }

        

//        private Dictionary<int, NewHavokAnimation.AnimOverlayRequest> CurOverlayAnims = new Dictionary<int, NewHavokAnimation.AnimOverlayRequest>();

//        private void SetOverlayAnim(int animID, NewHavokAnimation.AnimOverlayRequest req)
//        {
//            if (animID < 0)
//                return;

//            if (req.Weight > 0)
//            {
//                CurOverlayAnims[animID] = req;
//            }
//            else
//            {
//                if (CurOverlayAnims.ContainsKey(animID))
//                    CurOverlayAnims.Remove(animID);
//            }
//        }

//        private float NightfallTaeAddAnimCurWeightAtEventStart = 0;

//        public static float ProcessNightfallCurve(float input, byte curveType)
//        {
//            switch (curveType)
//            {
//                case 0:
//                    return input;
//                case 1:
//                    return 1 - (float)(Math.Cos(input * Math.PI) / 2.0 + 0.5);
//                case 2:
//                    return (float)Math.Sin(input * Math.PI * 0.5);
//                default:
//                    throw new NotImplementedException($"NIGHTFALL CURVE TYPE {curveType} NOT IMPLEMENTED");
//            }
//        }

//        public OLD_TaeEventSimulationEnvironment(NewGraph graph, Model mdl)
//        {
//            MODEL = mdl;
//            Graph = graph;

//            InitAllEntries();
//        }

//        public void ClearBoxStuff()
//        {
//            lock (_lock_bladeFFX)
//            {
//                SpawnFfxBladeEventMap.Clear();
//            }
//        }

//        private Dictionary<TAE.Event, NewDummyPolyManager.DummyPolyBladeSFX> SpawnFfxBladeEventMap = 
//            new Dictionary<TAE.Event, NewDummyPolyManager.DummyPolyBladeSFX>();

//        private object _lock_bladeFFX = new object();

//        private static ParamData.AtkParam.DummyPolySource GetDmySrcFromCStyleType(short cStyleType)
//        {
//            switch (cStyleType)
//            {
//                case 0: return ParamData.AtkParam.DummyPolySource.Body;
//                case 1: return ParamData.AtkParam.DummyPolySource.LeftWeapon0;
//                case 2: return ParamData.AtkParam.DummyPolySource.RightWeapon0;
//                default:
//                    throw new NotImplementedException($"Unknown SpawnFFX_Blade CStyleType Value: {cStyleType}");
//            }
//        }



//        public bool IsAnimBlendingEnabled = true;

//        private void ActivateBladeFfx(TAE.Event evBox)
//        {
//            if (evBox.Template == null || !evBox.IsStateInfoEnabled)
//                return;

//            lock (_lock_bladeFFX)
//            {
//                if (!SpawnFfxBladeEventMap.ContainsKey(evBox))
//                {
//                    var ffxid = Convert.ToInt32(evBox.Parameters["FFXID"]);
//                    var styleType = Convert.ToInt16(evBox.Parameters["CStyleType"]);
//                    var dmy1 = Convert.ToInt16(evBox.Parameters["DummyPolyBladeBaseID"]);
//                    var dmy2 = Convert.ToInt16(evBox.Parameters["DummyPolyBladeTipID"]);

//                    var newBlade = new NewDummyPolyManager.DummyPolyBladeSFX(ffxid, dmy1, dmy2, 
//                        GetDmySrcFromCStyleType(styleType), () => evBox.PlaybackHighlight);

//                    newBlade.UpdateLive(MODEL.DummyPolyMan);
//                    newBlade.UpdateLowHz(MODEL.DummyPolyMan);

//                    SpawnFfxBladeEventMap.Add(evBox, newBlade);
//                }
//            }
//        }

//        public void UpdateAllBladeSFXsLowHz()
//        {
//            lock (_lock_bladeFFX)
//            {
//                foreach (var kvp in SpawnFfxBladeEventMap)
//                {
//                    kvp.Value.UpdateLowHz(MODEL.DummyPolyMan);
//                }
//            }
//        }

//        public void UpdateAllBladeSFXsLive()
//        {
//            lock (_lock_bladeFFX)
//            {
//                var deadBlades = new List<TAE.Event>();
//                foreach (var kvp in SpawnFfxBladeEventMap)
//                {
//                    kvp.Value.UpdateLive(MODEL.DummyPolyMan);

//                    if (!kvp.Value.IsAlive)
//                        deadBlades.Add(kvp.Key);
//                }

//                foreach (var k in deadBlades)
//                {
//                    SpawnFfxBladeEventMap.Remove(k);
//                }
//            }
//        }

//        public void DrawAllBladeSFXs(Matrix m)
//        {
//            lock (_lock_bladeFFX)
//            {
//                foreach (var kvp in SpawnFfxBladeEventMap)
//                {
//                    kvp.Value.DoDebugDraw(m);
//                }
//            }
//        }

//        private Dictionary<string, EventSimEntry> entries;
//        public IReadOnlyDictionary<string, EventSimEntry> Entries => entries;

//        public List<string> SimulatedActiveSpEffects = new List<string>();

//        private ParamData.BehaviorParam GetBehaviorParamFromEvBox(TAE.Event evBox, ParamData.AtkParam.DummyPolySource dummyPolySource)
//        {
//            if (evBox.TypeName == "InvokeAttackBehavior" || 
//                evBox.TypeName == "InvokeThrowDamageBehavior" ||
//                evBox.TypeName == "InvokeBulletBehavior" ||
//                evBox.TypeName == "InvokeCommonBehavior" ||
//                evBox.TypeName == "InvokePCBehavior" ||
//                evBox.TypeName == "InvokeGunBehavior")
//            {
//                int behaviorSubID = (int)evBox.Parameters["BehaviorJudgeID"];
//                if (MODEL.IS_PLAYER)
//                {
//                    int behBase = 10_0000_000;
//                    int behVariationMult = 1_000;

//                    if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER && behaviorSubID >= 3000 && behaviorSubID < 4000)
//                    {
//                        behBase = 30_0000_000;
//                        behaviorSubID %= 1000;
//                    }

//                    var id = -1;
//                    if (evBox.TypeName == "InvokeCommonBehavior")
//                        id = behaviorSubID;
//                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon0 && MODEL.ChrAsm?.RightWeapon != null)
//                        id = behBase + (MODEL.ChrAsm.RightWeapon.BehaviorVariationID * behVariationMult) + behaviorSubID;
//                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon0 && MODEL.ChrAsm?.LeftWeapon != null)
//                        id = behBase + (MODEL.ChrAsm.LeftWeapon.BehaviorVariationID * behVariationMult) + behaviorSubID;
//                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.Body)
//                        id = behaviorSubID;

//                    if (ParamManager.BehaviorParam_PC.ContainsKey(id))
//                        return ParamManager.BehaviorParam_PC[id];

//                    if (dummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon0 && MODEL.ChrAsm?.RightWeapon != null)
//                        id = behBase + (MODEL.ChrAsm.RightWeapon.FallbackBehaviorVariationID * behVariationMult) + behaviorSubID;
//                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon0 && MODEL.ChrAsm?.LeftWeapon != null)
//                        id = behBase + (MODEL.ChrAsm.LeftWeapon.FallbackBehaviorVariationID * behVariationMult) + behaviorSubID;

//                    if (ParamManager.BehaviorParam_PC.ContainsKey(id))
//                        return ParamManager.BehaviorParam_PC[id];
//                }
//                else
//                {
//                    if (MODEL.NpcParam == null)
//                        return null;

//                    int id = -1;

//                    if (evBox.TypeName == "InvokeCommonBehavior")
//                    {
//                        id = behaviorSubID;
//                    }
//                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.Body)
//                    {
//                        id = 2_00000_000 + (MODEL.NpcParam.BehaviorVariationID * 1_000) + behaviorSubID;
//                    }

//                    if (ParamManager.BehaviorParam.ContainsKey(id))
//                        return ParamManager.BehaviorParam[id];
//                }
//            }

//            return null;
//        }

//        private ParamData.AtkParam GetAtkParamFromEventBox(TAE.Event evBox, ParamData.AtkParam.DummyPolySource dummyPolySource)
//        {
//            var behaviorParam = GetBehaviorParamFromEvBox(evBox, dummyPolySource);

//            if (behaviorParam == null)
//                return null;

//            if (behaviorParam.RefType == ParamData.BehaviorParam.RefTypes.Attack)
//            {
//                if (MODEL.IS_PLAYER)
//                {
//                    if (ParamManager.AtkParam_Pc.ContainsKey(behaviorParam.RefID))
//                        return ParamManager.AtkParam_Pc[behaviorParam.RefID];
//                }
//                else
//                {
//                    if (ParamManager.AtkParam_Npc.ContainsKey(behaviorParam.RefID))
//                        return ParamManager.AtkParam_Npc[behaviorParam.RefID];
//                }
//            }

//            return null;
//        }

//        private int GetBulletParamIDFromEvBox(TAE.Event evBox)
//        {
//            var behaviorParam = GetBehaviorParamFromEvBox(evBox, HitViewDummyPolySource);

//            if (behaviorParam == null)
//                return -1;

//            if (behaviorParam.RefType == ParamData.BehaviorParam.RefTypes.Bullet)
//            {
//                return behaviorParam.RefID;
//            }

//            return -1;
//        }

//        //private NewDummyPolyManager GetCurrentDummyPolyMan()
//        //{
//        //    if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.Body)
//        //    {
//        //        return MODEL.DummyPolyMan;
//        //    }
//        //    else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon &&
//        //        MODEL.ChrAsm?.RightWeaponModel != null)
//        //    {
//        //        return MODEL.ChrAsm.RightWeaponModel.DummyPolyMan;
//        //    }
//        //    else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon &&
//        //        MODEL.ChrAsm?.LeftWeaponModel != null)
//        //    {
//        //        return MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan;
//        //    }

//        //    return null;
//        //}

//        public Model GetModelOfBox(TAE.Event evBox)
//        {
//            var mdl = MODEL;

//            if (Graph.ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
//            {
//                mdl = RemoManager.LookupModelOfEventGroup(evBox.GetGroup(Graph.AnimRef)) ?? MODEL;
//            }

//            return mdl;
//        }

//        public void PlayRumbleCamOfBox(TAE.Event evBox)
//        {
//            var ev = evBox;
//            if (ev.Type == 144 || ev.Type == 145 && ev.TypeName != null)
//            {
//                var rumbleCamID = (short)ev.Parameters["RumbleCamID"];
//                Vector3? location = null;
//                float falloffStart = -1;
//                float falloffEnd = -1;
//                if (ev.Type == 144)
//                {
//                    var dummyPolyID = (ushort)ev.Parameters["DummyPolyID"];
//                    var locations = Graph.ViewportInteractor.CurrentModel.DummyPolyMan.GetDummyPosByID(dummyPolyID, getAbsoluteWorldPos: true);

//                    if (locations.Count > 0)
//                        location = locations[0];
//                    else
//                        location = Graph.ViewportInteractor.CurrentModel.CurrentTransformPosition;

//                    falloffStart = (float)ev.Parameters["FalloffStart"];
//                    falloffEnd = (float)ev.Parameters["FalloffEnd"];
//                }

//                //RumbleCamManager.ClearActive();
//                RumbleCamManager.AddRumbleCam(rumbleCamID, location, falloffStart, falloffEnd);
//            }
//        }

//        public string PeekSoundNameOfBox(TAE.Event evBox)
//        {
//            if (evBox.TypeName == null)
//                return null;

//            if (!(evBox.TypeName.Contains("Play") && evBox.TypeName.Contains("Sound")))
//                return null;

//            int soundType = Convert.ToInt32(evBox.Parameters["SoundType"]);
//            int soundID = Convert.ToInt32(evBox.Parameters["SoundID"]);

//            if (GameRoot.GameTypeUsesWwise && evBox.TypeName.StartsWith("Wwise"))
//            {
//                if (soundType == 8) // 8 = Floor Material Determined
//                {
//                    soundType = 1;
//                    soundID += (Wwise.GetDefensiveMaterialParamID() - 1);
//                }
//                else if (soundType == 9) // 9 = Armor Material Determined
//                {
//                    soundType = 1;
//                    soundID += ((int)FmodManager.ArmorMaterial);
//                }

//                var wwiseEventName = Wwise.GetSoundName(soundType, soundID);

//                return wwiseEventName;
//            }
//            else
//            {
//                return FmodManager.GetSoundName(soundType, soundID);
//            }
//        }

//        public TAE.Event MouseClickPreviewSlot_EvBox = null;
//        public int MouseClickPreviewSlot_SlotID = -1;
//        public string MouseClickPreviewSlot_EventName = null;
//        public int MouseClickPreviewSlot_DummyPolyID = -1;

//        public void DoMouseClickPreviewOfBoxSound(TAE.Event evBox)
//        {
//            if (MouseClickPreviewSlot_EvBox != null && MouseClickPreviewSlot_SlotID >= 0 && MouseClickPreviewSlot_EventName != null)
//            {
//                return;
//            }

//            if (evBox.Template.ContainsKey("SlotID"))
//            {
//                int slot = Convert.ToInt32(evBox.Parameters["SlotID"]);
//                int dummyPoly = -1;


//                MouseClickPreviewSlot_SlotID = slot;
//                MouseClickPreviewSlot_EvBox = evBox;
//                MouseClickPreviewSlot_EventName = PeekSoundNameOfBox(evBox);
//            }
            
//        }

//        public SoundPlayInfo GetSoundPlayInfoOfBox(TAE.Event evBox, bool isForOneShot)
//        {
//            if (evBox.TypeName == null)
//                return null;

//            if (!(evBox.TypeName.Contains("Play") && evBox.TypeName.Contains("Sound")))
//                return null;

//            //Graph?.ViewportInteractor?.LoadSoundsForCurrentModel(fresh: false);

//            int soundType = Convert.ToInt32(evBox.Parameters["SoundType"]);
//            int soundID = Convert.ToInt32(evBox.Parameters["SoundID"]);
//            float lifetime = -1;

//            bool updatingPosition = true;


//            if ((GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R) && isForOneShot)
//            {
//                updatingPosition = false;
//            }

//            int dummyPolyID = -1;

//            if (evBox.Template.ContainsKey("DummyPolyID"))
//            {
//                dummyPolyID = Convert.ToInt32(evBox.Parameters["DummyPolyID"]);

//#if NIGHTFALL
//                if (evBox.TypeName.StartsWith("NF_PlaySoundEx"))
//                {
//                    if (dummyPolyID <= 0)
//                        dummyPolyID = -1;
//                }
//#endif



//            }

//#if NIGHTFALL
//            if (evBox.TypeName.StartsWith("NF_PlayDummyPolyFollowSound"))
//            {
//                lifetime = Convert.ToSingle(evBox.Parameters["Lifetime"]);
//                updatingPosition = true;
//            }
//#endif

//            var soundPlaybackModel = MODEL;

//            if (Graph.ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
//            {
//                soundPlaybackModel = RemoManager.LookupModelOfEventGroup(evBox.GetGroup(Graph.AnimRef)) ?? MODEL;
//            }

//            if (Graph.ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
//            {
//                //getPosFunc = () => GFX.World.CameraLocationInWorld_CloserForSound.Position * new Vector3(1, 1, -1);

//                if (soundType == 9) // Armor material dependant
//                {
//                    soundID -= 56;
//                }
//            }




//            if (soundType == 8) // Floor Material Dependent
//            {
//                soundType = 1; // Change to character sound
//                if (GameRoot.GameTypeUsesWwise)
//                    soundID += (Wwise.GetDefensiveMaterialParamID() - 1);
//                else
//                    soundID += (FmodManager.FloorMaterial - 1);

//            }
//            else if (soundType == 9) // Armor Material Dependent
//            {
//                soundType = 1; // Change to character sound
//                if (GameRoot.GameTypeUsesWwise)
//                    soundID += Wwise.ArmorMaterial_Top;
//                else
//                    soundID += FmodManager.ArmorMaterial;
//            }

//            bool killSoundOnEventEnd = true;
//            if (GameRoot.GameTypeUsesWwise && evBox.Template.ContainsKey("KillSoundOnEventEnd"))
//            {
//                killSoundOnEventEnd = (bool)evBox.Parameters["KillSoundOnEventEnd"];
//            }

//            return new SoundPlayInfo()
//            {
//                DummyPolyID = dummyPolyID,
//                SoundType = soundType,
//                SoundID = soundID,
//                SourceModel = soundPlaybackModel,
//                UpdatingPosition = updatingPosition,
//                NightfallLifetime = lifetime,
//                KillSoundOnEventEnd = killSoundOnEventEnd,
//            };

            
//        }

//        public void PlayOneShotSoundOfBox(TAE.Event evBox, bool firstFrameEntered)
//        {
//            var soundInfo = GetSoundPlayInfoOfBox(evBox, isForOneShot: true);

//            if (soundInfo == null)
//                return;

//            int slot = -1;
//            if (evBox.Template.ContainsKey("SlotID"))
//            {
//                slot = Convert.ToInt32(evBox.Parameters["SlotID"]);
//            }

//            bool isAutoSlot = false;
//            if (evBox.Template.ContainsKey("KillSoundOnEventEnd"))
//            {
//                isAutoSlot = (bool)(evBox.Parameters["KillSoundOnEventEnd"]);
//            }

            

//#if NIGHTFALL
//            if (evBox.TypeName.StartsWith("NF_PlaySoundEx"))
//            {
//                if (soundInfo.DummyPolyID > 0)
//                    slot = Convert.ToInt32(evBox.Parameters["SlotID"]);
//            }
//            else if (evBox.TypeName.StartsWith("NF_PlayDummyPolyFollowSound"))
//            {
//                soundInfo.NightfallLifetime = Convert.ToSingle(evBox.Parameters["Lifetime"]);
//                //slot = NF_CurrentDummyPolySoundSlot;
//                slot = -1;
//                soundInfo.UpdatingPosition = true;
//            }
//#endif

//            if (slot >= 0 || isAutoSlot)
//                return;

//            SoundManager.PlayOneShotSound(soundInfo);

//        }

//        public void RootMotionWrapForBlades(Vector3 wrap)
//        {
//            foreach (var kvp in SpawnFfxBladeEventMap)
//            {
//                var blade = kvp.Value;
//                blade.CurrentPos.Start += wrap;
//                blade.CurrentPos.End += wrap;

//                var asList = blade.Keyframes.ToList();
//                for (int i = 0; i < blade.Keyframes.Count; i++)
//                {
//                    var b = asList[i];
//                    b.Start += wrap;
//                    b.End += wrap;
//                    asList[i] = b;
//                }

//                blade.Keyframes = new Queue<NewDummyPolyManager.DummyPolyBladePos>(asList);
//            }
//        }

//        static OLD_TaeEventSimulationEnvironment()
//        {
//            _allEntryDisplayNames = new Dictionary<string, string>();
//            _allEntryEnabledByDefault = new Dictionary<string, bool>();
//            var meme = new OLD_TaeEventSimulationEnvironment(null, null);
//            foreach (var e in meme.entries)
//            {
//                _allEntryDisplayNames.Add(e.Key, e.Value.MenuOptionName);
//                _allEntryEnabledByDefault.Add(e.Key, e.Value.IsEnabledByDefault);
//            }
//        }

//        private static volatile Dictionary<string, string> _allEntryDisplayNames;
//        private static volatile Dictionary<string, bool> _allEntryEnabledByDefault;
//        public static IReadOnlyDictionary<string, string> AllEntryDisplayNames => _allEntryDisplayNames;
//        public static IReadOnlyDictionary<string, bool> AllEntryEnabledByDefault => _allEntryEnabledByDefault;

//        public static bool GetEntryEnabledByDefault(string simKey)
//        {
//            if (_allEntryEnabledByDefault.ContainsKey(simKey))
//                return _allEntryEnabledByDefault[simKey];
//            return false;
//        }

//        private void InitAllEntries()
//        {
//            entries = new Dictionary<string, EventSimEntry>
//            {

//                {
//                    "EventSimSE",
//                    new EventSimEntry("Simulate Sound Effects", true)
//                    {
//                        AllowDuringRemo = true,
//                        SimulationFrameChangePerBoxAction = (entry, evBoxes, evBox, time) =>
//                        {
//                            if (!evBox.IsStateInfoEnabled)
//                                return;

//                            var thisBoxEntered = evBox.WasJustEnteredDuringPlayback;
//                            var initHit = thisBoxEntered && !evBox.PrevFrameEnteredState_ForSoundEffectPlayback;
//                            if (initHit)
//                            {
//                                PlayOneShotSoundOfBox(evBox, initHit);
//                            }
//                            evBox.PrevFrameEnteredState_ForSoundEffectPlayback = thisBoxEntered;

                            
//                        },
//                    }
//                },



//                {
//                    "EventSimRumbleCam",
//                    new EventSimEntry("Simulate Rumble Cam", true)
//                    {
//                        AllowDuringRemo = false,
//                        SimulationFrameChangePerBoxAction = (entry, evBoxes, evBox, time) =>
//                        {
//                            if (!evBox.IsStateInfoEnabled)
//                                return;

//                            var thisBoxEntered = evBox.WasJustEnteredDuringPlayback;
//                            if (!evBox.PrevFrameEnteredState_ForRumbleCamPlayback && thisBoxEntered)
//                            {
//                                PlayRumbleCamOfBox(evBox);
//                            }
//                            evBox.PrevFrameEnteredState_ForRumbleCamPlayback = thisBoxEntered;


//                        },
//                    }
//                },



//                {
//                    "EventSimWeaponStyle",
//                    new EventSimEntry("Simulate Weapon Style Changes", true, "SetWeaponStyle")
//                    {
//                        SimulationFrameChangeDisabledAction = (entry, evBoxes, time) =>
//                        {
//                            if (MODEL.ChrAsm == null)
//                                return;

//                            MODEL.ChrAsm.WeaponStyle = MODEL.ChrAsm.StartWeaponStyle;
//                        },
//                        SimulationFrameChangePerBoxAction = (entry, evBoxes, evBox, time) =>
//                        {
//                            if (MODEL.ChrAsm == null)
//                                return;

//                            if (evBox.Type != 32)
//                                return;

//                            // Not checking if the box is active because the effects
//                            // "accumulate" along the timeline.

//                            // Only simulate until the current time.
//                            if (evBox.StartTime > time)
//                                return;

//                            MODEL.ChrAsm.WeaponStyle = (NewChrAsm.WeaponStyleType)Convert.ToInt32(evBox.Parameters["WeaponStyle"]);
//                        },
//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            if (MODEL.ChrAsm == null)
//                                return;

//                            if ((Graph?.ViewportInteractor?.CurrentComboIndex ?? -1) < 0)
//                                MODEL.ChrAsm.WeaponStyle = MODEL.ChrAsm.StartWeaponStyle;
//                        },
//                    }
//                },
//                {
//                    "EventSimTracking",
//                    new EventSimEntry("Simulate Character Tracking (A/D Keys)", true)
//                    {
//                        SimulationFrameChangeDisabledAction = (entry, evBoxes, time) =>
//                        {
//                            MODEL.TrackingTestInput = 0;
//                            MODEL.CurrentTrackingSpeed = MODEL.BaseTrackingSpeed;
//                        },
//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {

//                            float activeTrackingSpeedThisFrame = MODEL.BaseTrackingSpeed;
//                            foreach (var evBox in evBoxes)
//                            {
//                                if (evBox.PlaybackHighlight && evBox.Template != null)
//                                {
//                                    if (evBox.Type == 0 && Convert.ToInt32(evBox.Parameters["JumpTableID"]) == 7)
//                                    {
//                                        // No matter what, if any disable rotation event is active the character can NOT rotate.
//                                        activeTrackingSpeedThisFrame = 0;
//                                        break;
//                                    }
//                                    else if (evBox.Type == 224)
//                                    {
//                                        activeTrackingSpeedThisFrame = Convert.ToSingle(evBox.Parameters["TurnSpeed"]);
//                                    }
//#if NIGHTFALL
//                                    else if (evBox.Type == 7021)
//                                    {
//                                        var turnSpeedBlend = ((System.Numerics.Vector2)evBox.Parameters["TurnSpeed"]);
//                                        var curveType = Convert.ToByte(evBox.Parameters["CurveType"]);
                                    

//                                        float fadeDuration = evBox.EndTime - evBox.StartTime;
//                                        if (fadeDuration > 0)
//                                        {
//                                            float timeSinceFadeStart = time - evBox.StartTime;
//                                            float lerpS = MathHelper.Clamp(timeSinceFadeStart / fadeDuration, 0, 1);

//                                            lerpS = ProcessNightfallCurve(lerpS, curveType);


//                                            activeTrackingSpeedThisFrame = MathHelper.Lerp(turnSpeedBlend.X, turnSpeedBlend.Y, lerpS);
//                                        }
                                        
//                                    }
//#endif
//                                }
//                            }

//                            if (float.IsNaN(activeTrackingSpeedThisFrame))
//                                Console.WriteLine("Breakpoint Hit");

//                            MODEL.CurrentTrackingSpeed = activeTrackingSpeedThisFrame;

//                            if (!Graph.MainScreen.Input.AnyModifiersHeld && !ImguiOSD.DialogManager.AnyDialogsShowing)
//                            {
//                                float trackDir = 0;
//                                if (Graph.MainScreen.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.D))
//                                    trackDir += 1;
//                                if (Graph.MainScreen.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.A))
//                                    trackDir -= 1;

//                                MODEL.TrackingTestInput = Model.GlobalTrackingInput + trackDir;
//                            }
//                        },
//                    }
//                },

//                {
//                    "EventSimAdditiveOverlays",
//                    new EventSimEntry("Simulate Additive Overlay Anim Playback Events", true)
//                    {
//                        SimulationFrameChangeDisabledAction = (entry, evBoxes, time) =>
//                        {
//                            CurOverlayAnims.Clear();
//                            MODEL.AnimContainer?.SetAdditiveLayers(CurOverlayAnims);
//                        },
//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            CurOverlayAnims.Clear();
//                        },
//                        SimulationStartAction = (entry, evBoxes) =>
//                        {
//                            NightfallTaeAddAnimCurWeightAtEventStart = -1;
//                        },
//                        SimulationFrameChangePerBoxAction = (entry, evBoxes, box, time) =>
//                        {
//                            if (Graph.ViewportInteractor.IsComboRecording)
//                                return;

//                            if (!box.PlaybackHighlight)
//                                return;

//                            var ev = box;
//                            if (ev.TypeName == null)
//                                return;

//                            if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS1 or SoulsAssetPipeline.SoulsGames.DS1R && ev.TypeName == "PlayAnimation")
//                            {
//                                if (MODEL?.AnimContainer?.AnyAdditiveLayers() == true)
//                                    return;

//                                int animID = Convert.ToInt32(box.Parameters["AnimID"]);

//                                if (animID < 0)
//                                    return;

//                                var req = new NewHavokAnimation.AnimOverlayRequest()
//                                {
//                                    Weight = 1,
//                                    LoopEnabled = false,
//                                    IsDS1PlayAtMaxWeightOneShotUntilEnd = true,
//                                };
//                                SetOverlayAnim(animID, req);
//                            }
//#if NIGHTFALL
//                            else if (ev.TypeName.Contains("NF_SetTaeExtraAnim"))
//                            {
//                                int animID = Convert.ToInt32(box.Parameters["AnimID"]);

//                                if (animID < 0)
//                                    return;

//                                if (box.WasJustEnteredDuringPlayback || NightfallTaeAddAnimCurWeightAtEventStart < 0)
//                                {
//                                    NightfallTaeAddAnimCurWeightAtEventStart = MODEL.AnimContainer.GetAdditiveOverlayWeight(animID);
//                                }

//                                var weightGrad = (System.Numerics.Vector2)(box.Parameters["Weight"]);
//                                float weight = ev.ParameterLerp(time, weightGrad.X, weightGrad.Y);
//                                float reqLerpS = Convert.ToSingle(box.Parameters["LerpS"]);
//                                float evLerpS = ev.GetLerpS(time);
//                                var req = new NewHavokAnimation.AnimOverlayRequest()
//                                {
//                                    Weight = weight,
//                                    LoopEnabled = true,
//                                    IsDS1PlayAtMaxWeightOneShotUntilEnd = false,
//                                    NF_RequestedLerpS = reqLerpS,
//                                    NF_EvInputLerpS = evLerpS,
//                                    NF_WeightAtEvStart = NightfallTaeAddAnimCurWeightAtEventStart,
//                                };
//                                SetOverlayAnim(animID, req);
//                            }
//#endif
//                            else if (ev.TypeName.Contains("SetAdditiveAnim"))
//                            {
//                                int animType = Convert.ToInt32(box.Parameters["AnimType"]);
//                                float a = Convert.ToSingle(box.Parameters["WeightAtEventStart"]);
//                                float b = Convert.ToSingle(box.Parameters["WeightAtEventEnd"]);
//                                float weight = ev.ParameterLerp(time, a, b);

//                                int finalAnimID = animType;

//                                if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3 or SoulsAssetPipeline.SoulsGames.SDT or SoulsAssetPipeline.SoulsGames.ER)
//                                    finalAnimID += 40000;
//                                else if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.BB)
//                                    finalAnimID += 10;

//                                if (box.Template.ContainsKey("AnimType2"))
//                                {
//                                    finalAnimID += (10 * Convert.ToInt32(box.Parameters["AnimType2"]));

//                                }

//                                if (finalAnimID < 0)
//                                    return;

//                                var req = new NewHavokAnimation.AnimOverlayRequest()
//                                {
//                                    Weight = weight,
//                                    LoopEnabled = true,
//                                    IsDS1PlayAtMaxWeightOneShotUntilEnd = false,
//                                };

//                                SetOverlayAnim(finalAnimID, req);
//                            }

                            
//                        },
//                        SimulationFrameChangePostBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            MODEL.AnimContainer?.SetAdditiveLayers(CurOverlayAnims);
//                        },

//                    }
//                },

//                {
//                    "EventSimDS3PairedWeaponOverrideMemeEvents",
//                    new EventSimEntry("Simulate Weapon Model Location Override Events", true)
//                    {
//                        SimulationFrameChangeDisabledAction = (entry, evBoxes, time) =>
//                        {
//                            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
//                                MODEL.ChrAsm?.ClearDS3PairedWeaponMeme();
//                            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
//                                MODEL.ChrAsm?.ClearSekiroProstheticOverrideTae();
//                        },
//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            bool isAnyOverrideMemeHappeningThisFrame = false;

//                            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
//                                MODEL?.ChrAsm?.ClearSekiroProstheticOverrideTae();

//                            foreach (var evBox in evBoxes)
//                            {
//                                if (!evBox.IsStateInfoEnabled)
//                                    continue;

//                                if (evBox.Type == 712 && evBox.Template != null &&
//                                    GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 && MODEL.ChrAsm != null &&
//                                    evBox.PlaybackHighlight)
//                                {
//                                    if (MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.OneHand)
//                                    {
//                                        if (Convert.ToByte(evBox.Parameters["IsEnabledWhile1Handing"]) != 1)
//                                            continue;
//                                    }
//                                    else if (MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.TwoHandR)
//                                    {
//                                        if (Convert.ToByte(evBox.Parameters["IsEnabledWhile2HandingRightWeapon"]) != 1)
//                                            continue;
//                                    }
//                                    else if (MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.TwoHandL)
//                                    {
//                                        if (Convert.ToByte(evBox.Parameters["IsEnabledWhile2HandingLeftWeapon"]) != 1)
//                                            continue;
//                                    }

//                                    isAnyOverrideMemeHappeningThisFrame = true;

//                                    MODEL.ChrAsm.DS3PairedWeaponMemeR0_Flag = Convert.ToByte(evBox.Parameters["RHModel0AbsorpPosParamCondition"]);
//                                    MODEL.ChrAsm.DS3PairedWeaponMemeR1_Flag = Convert.ToByte(evBox.Parameters["RHModel1AbsorpPosParamCondition"]);
//                                    MODEL.ChrAsm.DS3PairedWeaponMemeR2_Flag = Convert.ToByte(evBox.Parameters["RHModel2AbsorpPosParamCondition"]);
//                                    MODEL.ChrAsm.DS3PairedWeaponMemeR3_Flag = Convert.ToByte(evBox.Parameters["RHModel3AbsorpPosParamCondition"]);
//                                    MODEL.ChrAsm.DS3PairedWeaponMemeL0_Flag = Convert.ToByte(evBox.Parameters["LHModel0AbsorpPosParamCondition"]);
//                                    MODEL.ChrAsm.DS3PairedWeaponMemeL1_Flag = Convert.ToByte(evBox.Parameters["LHModel1AbsorpPosParamCondition"]);
//                                    MODEL.ChrAsm.DS3PairedWeaponMemeL2_Flag = Convert.ToByte(evBox.Parameters["LHModel2AbsorpPosParamCondition"]);
//                                    MODEL.ChrAsm.DS3PairedWeaponMemeL3_Flag = Convert.ToByte(evBox.Parameters["LHModel3AbsorpPosParamCondition"]);

//                                    var changeTypeR0 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.Parameters["RHModel0ChangeType"]);
//                                    var changeTypeR1 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.Parameters["RHModel1ChangeType"]);
//                                    var changeTypeR2 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.Parameters["RHModel2ChangeType"]);
//                                    var changeTypeR3 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.Parameters["RHModel3ChangeType"]);
//                                    var changeTypeL0 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.Parameters["LHModel0ChangeType"]);
//                                    var changeTypeL1 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.Parameters["LHModel1ChangeType"]);
//                                    var changeTypeL2 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.Parameters["LHModel2ChangeType"]);
//                                    var changeTypeL3 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.Parameters["LHModel3ChangeType"]);


//                                    if (changeTypeR0 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
//                                        MODEL.ChrAsm.DS3PairedWeaponMemeR0 = changeTypeR0;

//                                    if (changeTypeR1 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
//                                        MODEL.ChrAsm.DS3PairedWeaponMemeR1 = changeTypeR1;

//                                    if (changeTypeR2 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
//                                        MODEL.ChrAsm.DS3PairedWeaponMemeR2 = changeTypeR2;

//                                    if (changeTypeR3 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
//                                        MODEL.ChrAsm.DS3PairedWeaponMemeR3 = changeTypeR3;

//                                    if (changeTypeL0 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
//                                        MODEL.ChrAsm.DS3PairedWeaponMemeL0 = changeTypeL0;

//                                    if (changeTypeL1 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
//                                        MODEL.ChrAsm.DS3PairedWeaponMemeL1 = changeTypeL1;

//                                    if (changeTypeL2 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
//                                        MODEL.ChrAsm.DS3PairedWeaponMemeL2 = changeTypeL2;

//                                    if (changeTypeL3 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
//                                        MODEL.ChrAsm.DS3PairedWeaponMemeL3 = changeTypeL3;
//                                }
//                                else if (evBox.Type == 715 && evBox.Template != null &&
//                                    GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT && MODEL.ChrAsm != null &&
//                                    evBox.PlaybackHighlight)
//                                {
//                                    isAnyOverrideMemeHappeningThisFrame = true;
//                                    var modelType = Convert.ToByte(evBox.Parameters["WeaponModelType"]);
//                                    var model0DummyPoly = Convert.ToInt32(evBox.Parameters["Model0DummyPolyID"]);
//                                    var model1DummyPoly = Convert.ToInt32(evBox.Parameters["Model1DummyPolyID"]);
//                                    var model2DummyPoly = Convert.ToInt32(evBox.Parameters["Model2DummyPolyID"]);
//                                    var model3DummyPoly = Convert.ToInt32(evBox.Parameters["Model3DummyPolyID"]);
//                                    MODEL.ChrAsm.RegistSekiroProstheticOverride((NewChrAsm.ProstheticOverrideModelType)modelType,
//                                        model0DummyPoly, model1DummyPoly, model2DummyPoly, model3DummyPoly);
//                                    MODEL.ChrAsm.SekiroProstheticOverrideTaeActive = true;
//                                }
//                            }

//                            if (!isAnyOverrideMemeHappeningThisFrame)
//                            {
//                                if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
//                                    MODEL.ChrAsm?.ClearDS3PairedWeaponMeme();
//                                else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
//                                    MODEL.ChrAsm?.ClearSekiroProstheticOverrideTae();
//                            }
//                        },
//                    }
//                },

//                {
//                    "EventSimAttackBehaviors",
//                    new EventSimEntry("Simulate Hitbox Events", isEnabledByDefault: true,
//                        "InvokeAttackBehavior", "InvokePCBehavior", "InvokeCommonBehavior", "InvokeThrowDamageBehavior")
//                    {
//                        NewAnimSelectedAction = (entry, evBoxes) =>
//                        {
//                            Scene.ForeachModel(mdl =>
//                            {
//                                mdl.DummyPolyMan.HideAllHitboxes();
//                                mdl.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.HideAllHitboxes());
//                            });

//                            //if (MODEL.ChrAsm?.RightWeaponModel != null)
//                            //    MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.HideAllHitboxes();

//                            //if (MODEL.ChrAsm?.LeftWeaponModel != null)
//                            //    MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.HideAllHitboxes();
//                        },
//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            Scene.ForeachModel(mdl =>
//                            {
//                                mdl.DummyPolyMan.HideAllHitboxes();
//                                mdl.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.HideAllHitboxes());
//                            });
//                            //if (MODEL.ChrAsm?.RightWeaponModel != null)
//                            //    MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.HideAllHitboxes();

//                            //if (MODEL.ChrAsm?.LeftWeaponModel != null)
//                            //    MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.HideAllHitboxes();
//                        },
//                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
//                        {
//                            if (!evBox.IsStateInfoEnabled)
//                                return;

//                            var mdl = GetModelOfBox(evBox);

//                            if (!evBox.PlaybackHighlight)
//                                return;

//                            var dmyPolySrcToUse = HitViewDummyPolySource;

                            

//                            //int dummyPolyOverride = -1;

//                            if (evBox.Template != null)
//                            {
//                                if (evBox.TypeName == "InvokePCBehavior")
//                                {
//                                    int pcBehaviorType = Convert.ToInt32(evBox.Parameters["PCBehaviorType"]);
//                                    if (pcBehaviorType == 1 || pcBehaviorType == 16)
//                                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.RightWeapon0;
//                                    else if (pcBehaviorType == 2 || pcBehaviorType == 128)
//                                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
//                                    else
//                                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.Body;
//                                }
//                                else if (evBox.TypeName == "InvokeAttackBehavior" && (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER || 
//                                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT))
//                                {
//                                    int sourceType = Convert.ToInt32(evBox.Parameters["Source"]);
//                                    if (sourceType == 1)
//                                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.RightWeapon0;
//                                    else if (sourceType == 2)
//                                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
//                                }
//                            }

//                            var atkParam = GetAtkParamFromEventBox(evBox, dmyPolySrcToUse);

//                            //MODEL.DummyPolyMan.defaultDummyPolySource = dmyPolySrcToUse;

//                            if (atkParam != null)
//                            {
//                                mdl.DummyPolyMan.SetAttackVisibility(atkParam, true, mdl.ChrAsm, -1 /*dummyPolyOverride*/, dmyPolySrcToUse);
//                            }
//                        },
//                    }
//                },

//                { 
//                    "EventSimSpEffects", 
//                    new EventSimEntry("Simulate SpEffects", true,
//                        "AddSpEffect",
//                        "AddSpEffect_Multiplayer",
//                        "AddSpEffect_Multiplayer_401",
//                        "AddSpEffect_DragonForm",
//                        "AddSpEffect_WeaponArts",
//                        "AddSpEffect_CultRitualCompletion")
//                    {

//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            SimulatedActiveSpEffects.Clear();

//                            Graph.PlaybackCursor.ModPlaybackSpeed_Event603 = 1;
//                        },
//                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
//                        {
//                            if (!evBox.PlaybackHighlightMidst)
//                                return;

//                            if (!evBox.IsStateInfoEnabled)
//                                return;

//                            if (evBox.Parameters.Template.ContainsKey("SpEffectID"))
//                            {
//                                int spEffectID = Convert.ToInt32(evBox.Parameters["SpEffectID"]);

//                                if (ParamManager.SpEffectParam.ContainsKey(spEffectID))
//                                {
//                                    var spEffect = ParamManager.SpEffectParam[spEffectID];

//                                    if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
//                                    {
//                                        if (spEffect.GrabityRate > 0)
//                                            Graph.PlaybackCursor.ModPlaybackSpeed_Event603 *= spEffect.GrabityRate;
//                                    }

//                                    SimulatedActiveSpEffects.Add($"{spEffect.GetDisplayName()}");
//                                }
//                                else
//                                {
//                                    SimulatedActiveSpEffects.Add($"[Doesn't Exist] {spEffectID}");
//                                }



//                            }

//                        },
//                    }
//                },

//                {
//                    "EventSimDS3DebugAnimSpeed",
//                    new EventSimEntry("Simulate DS3 DebugAnimSpeed", isEnabledByDefault: false, "DebugAnimSpeed")
//                    {
//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            Graph.PlaybackCursor.ModPlaybackSpeed_Event603 = 1;
//                        },
//                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
//                        {
//                            if (!evBox.IsStateInfoEnabled)
//                                return;

//                            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 && evBox.PlaybackHighlight)
//                            {
//                                var eventFrames = Convert.ToUInt32(evBox.Parameters["AnimSpeed"]);
//                                Graph.PlaybackCursor.ModPlaybackSpeed_Event603 = (((evBox.EndTime - evBox.StartTime) * 30.0f) / (float)eventFrames);
//                            }
//                        },
//                    }
//                },

//                {
//                    "EventSimERSpeedGradient",

//                    new EventSimEntry("Simulate ER AnimSpeedGradient", isEnabledByDefault: true, "AnimSpeedGradient"
//#if NIGHTFALL
//                        , "NF_SpeedGradient"
//#endif
//                    )
//                    {
//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            Graph.PlaybackCursor.ModPlaybackSpeed_Event608 = 1;
//                        },
//                        SimulationFrameChangeDisabledAction = (entry, evBoxes, time) =>
//                        {
//                            Graph.PlaybackCursor.ModPlaybackSpeed_Event608 = 1;
//                        },
//                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
//                        {
//                            if (!evBox.IsStateInfoEnabled)
//                                return;

//                            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER)
//                            {
//                                if (evBox.TypeName == "AnimSpeedGradient")
//                                {
//                                    float startSpeed = Convert.ToSingle(evBox.Parameters["SpeedAtStart"]);
//                                    float endSpeed = Convert.ToSingle(evBox.Parameters["SpeedAtEnd"]);

//                                    float fadeDuration = evBox.EndTime - evBox.StartTime;
//                                    if (fadeDuration > 0)
//                                    {
//                                        float timeSinceFadeStart = time - evBox.StartTime;
//                                        float lerpS = MathHelper.Clamp(timeSinceFadeStart / fadeDuration, 0, 1);

//                                        Graph.PlaybackCursor.ModPlaybackSpeed_Event608 = MathHelper.Lerp(startSpeed, endSpeed, lerpS);
//                                    }
//                                }
//                            }
//#if NIGHTFALL
//                            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
//                            {
//                                if (evBox.TypeName == "NF_SpeedGradient")
//                                {
//                                    var blend = ((System.Numerics.Vector2)evBox.Parameters["Speed"]);
//                                    var curveType = Convert.ToByte(evBox.Parameters["CurveType"]);

//                                    float fadeDuration = evBox.EndTime - evBox.StartTime;
//                                    if (fadeDuration > 0)
//                                    {
//                                        float timeSinceFadeStart = time - evBox.StartTime;
//                                        float lerpS = MathHelper.Clamp(timeSinceFadeStart / fadeDuration, 0, 1);

//                                        lerpS = ProcessNightfallCurve(lerpS, curveType);

//                                        Graph.PlaybackCursor.ModPlaybackSpeed_Event608 *= MathHelper.Lerp(blend.X, blend.Y, lerpS);
//                                    }
//                                }
//                            }
//#endif
//                        },
//                    }
//                },

//                {
//                    "EventSimBulletBehaviors",
//                    new EventSimEntry("Simulate Bullet Spawns", isEnabledByDefault: true,
//                        "InvokeBulletBehavior")
//                    {
//                        NewAnimSelectedAction = (entry, evBoxes) =>
//                        {
//                            Scene.ForeachModel(mdl =>
//                            {
//                                mdl.DummyPolyMan.ClearAllBulletSpawns();
//                                mdl.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.ClearAllBulletSpawns());
//                            });
//                        },
//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            Scene.ForeachModel(mdl =>
//                            {
//                                mdl.DummyPolyMan.ClearAllBulletSpawns();
//                                mdl.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.ClearAllBulletSpawns());
//                            });
//                        },
//                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
//                        {
//                            if (!evBox.PlaybackHighlight)
//                                return;

//                            if (!evBox.IsStateInfoEnabled)
//                                return;

//                            if (evBox.Parameters.Template.ContainsKey("DummyPolyID"))
//                            {
//                                var mdl = GetModelOfBox(evBox);
//                                var bulletParamID = GetBulletParamIDFromEvBox(evBox);
//                                int dummyPolyID = Convert.ToInt32(evBox.Parameters["DummyPolyID"]);

//                                if (bulletParamID >= 0)
//                                {
//                                    var asmSrc = mdl.ChrAsm?.GetDummyPolySpawnPlace(ParamData.AtkParam.DummyPolySource.Body, dummyPolyID, mdl.DummyPolyMan);
//                                    if (asmSrc == null || asmSrc == mdl.DummyPolyMan)
//                                    {
//                                        mdl.DummyPolyMan.SpawnBulletOnDummyPoly(bulletParamID, dummyPolyID);
//                                    }
//                                    else
//                                    {
//                                        asmSrc.SpawnBulletOnDummyPoly(bulletParamID, dummyPolyID % 1000);
//                                    }
//                                }

//                            }
//                        },
//                    }
//                },

//                {
//                    "EventSimSFXSpawns",
//                    new EventSimEntry("Simulate SFX Spawns", isEnabledByDefault: true,
//                        "N/A")
//                    {
//                        AllowDuringRemo = true,
//                        NewAnimSelectedAction = (entry, evBoxes) =>
//                        {
//                            Scene.ForeachModel(mdl =>
//                            {
//                                mdl.DummyPolyMan.ClearAllSFXSpawns();
//                                mdl.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.ClearAllSFXSpawns());
//                            });
//                        },
//                        SimulationFrameChangeDisabledAction = (entry, evBoxes, time) =>
//                        {
//                            Scene.ForeachModel(mdl =>
//                            {
//                                mdl.DummyPolyMan.ClearAllSFXSpawns();
//                                mdl.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.ClearAllSFXSpawns());
//                            });
//                        },
//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            Scene.ForeachModel(mdl =>
//                            {
//                                mdl.DummyPolyMan.ClearAllSFXSpawns();
//                                mdl.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.ClearAllSFXSpawns());
//                            });
//                        },
//                        SimulationFrameChangePerBoxAction = (entry, evBoxes, evBox, time) =>
//                        {
//                            if (!evBox.PlaybackHighlight)
//                                return;

//                            if (evBox.Template == null)
//                                return;

//                            if (!evBox.IsStateInfoEnabled)
//                                return;

//                            if (evBox.Parameters.Template.ContainsKey("FFXID") &&
//                                evBox.Parameters.Template.ContainsKey("DummyPolyID"))
//                            {
//                                // Using convert here since they might be various numeric
//                                // value types.
//                                int ffxid = Convert.ToInt32(evBox.Parameters["FFXID"]);
//                                int dummyPolyID = Convert.ToInt32(evBox.Parameters["DummyPolyID"]);


//                                var mdl = GetModelOfBox(evBox);

//                                if (evBox.Parameters.Template.ContainsKey("DummyPolySource"))
//                                {
//                                    ParamData.AtkParam.DummyPolySource dummyPolySrc = ParamData.AtkParam.DummyPolySource.Body;

//                                    int dplsVal = Convert.ToInt32(evBox.Parameters["DummyPolySource"]);
//                                    if (dplsVal == 0)
//                                    {
//                                        dummyPolySrc = ParamData.AtkParam.DummyPolySource.Body;
//                                    }
//                                    else if (dplsVal == 1)
//                                    {
//                                        if (mdl.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.OneHandTransformedL ||
//                                        mdl.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.TwoHandL)
//                                        {
//                                             dummyPolySrc = ParamData.AtkParam.DummyPolySource.LeftWeapon2;
//                                        }
//                                        else
//                                        {
//                                             dummyPolySrc = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
//                                        }

//                                    }
//                                    else if (dplsVal == 2)
//                                    {
//                                        if (mdl.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.OneHandTransformedR ||
//                                        mdl.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.TwoHandR)
//                                        {
//                                             dummyPolySrc = ParamData.AtkParam.DummyPolySource.RightWeapon2;
//                                        }
//                                        else
//                                        {
//                                             dummyPolySrc = ParamData.AtkParam.DummyPolySource.RightWeapon0;
//                                        }

//                                    }

//                                    var asmSrc = mdl.ChrAsm?.GetDummyManager(dummyPolySrc);

//                                    if (asmSrc != null)
//                                    {
//                                        asmSrc.SpawnSFXOnDummyPoly(ffxid, dummyPolyID);
//                                    }
//                                    else
//                                    {
//                                        mdl.DummyPolyMan.SpawnSFXOnDummyPoly(ffxid, dummyPolyID);
//                                    }
//                                }

//                                else
//                                {
//                                    var asmSrc = mdl.ChrAsm?.GetDummyPolySpawnPlace(ParamData.AtkParam.DummyPolySource.Body, dummyPolyID, mdl.DummyPolyMan);
//                                    if (asmSrc == null || asmSrc == mdl.DummyPolyMan)
//                                    {
//                                        mdl.DummyPolyMan.SpawnSFXOnDummyPoly(ffxid, dummyPolyID);
//                                    }
//                                    else
//                                    {
//                                        asmSrc.SpawnSFXOnDummyPoly(ffxid, dummyPolyID % 1000);
//                                    }
//                                }

                                
//                            }
//                        },
//                    }
//                },

//                {
//                    "EventSimOpacity",
//                    new EventSimEntry("Simulate Opacity Change Events", isEnabledByDefault: true,
//                        "SetOpacityKeyframe", "RemoSetOpacityKeyframe")
//                    {
//                        AllowDuringRemo = true,
//                        NewAnimSelectedAction = (entry, evBoxes) =>
//                        {
//                            Scene.ForeachModel(m =>
//                            {
//                                m.Opacity = 1;
//                            });
//                        },
//                        //SimulationStartAction = (entry, evBoxes) =>
//                        //{
//                        //    GFX.FlverOpacity = 1.0f;
//                        //},
//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            Scene.ForeachModel(m =>
//                            {
//                                m.Opacity = 1;
//                            });
//                        },
//                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
//                        {
//                            if (!evBox.PlaybackHighlight)
//                                return;

//                            if (!evBox.IsStateInfoEnabled)
//                                return;

//                            float fadeDuration = evBox.EndTime - evBox.StartTime;
//                            float timeSinceFadeStart = time - evBox.StartTime;

//                            float fadeStartOpacity = evBox.TypeName == "RemoSetOpacityKeyframe" ? 
//                            ((byte)evBox.Parameters["OpacityByteAtEventStart"] / 255f) : 
//                            (float)evBox.Parameters["OpacityAtEventStart"];

//                            float fadeEndOpacity = evBox.TypeName == "RemoSetOpacityKeyframe" ?
//                            ((byte)evBox.Parameters["OpacityByteAtEventEnd"] / 255f) :
//                            (float)evBox.Parameters["OpacityAtEventEnd"];

//                            var mdl = GetModelOfBox(evBox);

//                            if (mdl != null)
//                                mdl.Opacity = MathHelper.Lerp(fadeStartOpacity, fadeEndOpacity, timeSinceFadeStart / fadeDuration);
//                        },
//                        //DuringAction = (entry, evBox) =>
//                        //{
                            
//                        //},
//                        //SimulationEndAction = (entry, evBoxes) =>
//                        //{
//                        //    GFX.FlverOpacity = 1.0f;
//                        //},
                        
//                    }
//                },

//#if NIGHTFALL
//                {
//                    "EventSimNightfallEvents",
//                    new EventSimEntry("Simulate Nightfall Events", isEnabledByDefault: true)
//                    {
//                        AllowDuringRemo = false,
//                        NewAnimSelectedAction = (entry, evBoxes) =>
//                        {
//                            TaeRootMotionScaleXZ = 1;
//                        },
//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            TaeRootMotionScaleXZ = 1;
//                        },
//                        SimulationFrameChangePerBoxAction = (entry, evBoxes, evBox, time) =>
//                        {
//                            if (!evBox.PlaybackHighlight)
//                                return;

//                            if (!evBox.IsStateInfoEnabled)
//                                return;

//                            if (evBox.Type == 7027) //NF_ProperRootMotionMult
//                            {

//                                float fadeDuration = evBox.EndTime - evBox.StartTime;
//                                float timeSinceFadeStart = time - evBox.StartTime;

//                                var grad = (System.Numerics.Vector2)(evBox.Parameters["Mult"]);

//                                TaeRootMotionScaleXZ *= MathHelper.Lerp(grad.X, grad.Y, MathHelper.Clamp(timeSinceFadeStart / fadeDuration, 0, 1));
//                            }
//                        },
//                        //DuringAction = (entry, evBox) =>
//                        //{
                            
//                        //},
//                        //SimulationEndAction = (entry, evBoxes) =>
//                        //{
//                        //    GFX.FlverOpacity = 1.0f;
//                        //},
                        
//                    }
//                },
//#endif

//                {
//                    "EventSimDrawMasks",
//                    new EventSimEntry("Simulate Draw Mask Changes", isEnabledByDefault: true,
//                        "ShowModelMask", "HideModelMask", "ChangeChrDrawMask", "HideEquippedWeapon")
//                    {
//                        NewAnimSelectedAction = (entry, evBoxes) =>
//                        {
//                            //entry.Vars.ClearAllData();

//                            //MODEL.DefaultAllMaskValues();
//                        },
//                        SimulationStartAction = (entry, evBoxes) =>
//                        {
//                            //MODEL.DefaultAllMaskValues();
//                        },
//                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
//                        {
//                            MODEL.ResetDrawMaskToDefault();

//                            if (MODEL.ChrAsm != null)
//                            {
//                                MODEL.ChrAsm.SetRightWeaponVisible(true);
//                                MODEL.ChrAsm.SetLeftWeaponVisible(true);
//                            }
//                        },
//                        SimulationFrameChangePerBoxAction = (entry, evBoxes, evBox, time) =>
//                        {
//                            if (!evBox.IsStateInfoEnabled)
//                                return;

//                            // Not checking if the box is active because the effects
//                            // "accumulate" along the timeline.

//                            // Only simulate until the current time.
//                            if (evBox.StartTime > time)
//                                return;

//                            int maskLength = 32;

//                            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || 
//                                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
//                                maskLength = 8;

//                            if (evBox.Template == null)
//                                return;

//                            if (evBox.TypeName == "ChangeChrDrawMask")
//                            {
//                                for (int i = 0; i < maskLength; i++)
//                                {
//                                    var maskByte = (byte)evBox.Parameters[$"Mask{(i + 1)}"];
                                    
//                                    // Before you get out the torch, be aware that the game 
//                                    // uses some value other than 0 to SKIP
//                                    if (maskByte == 0)
//                                        MODEL.DrawMask[i] = false;
//                                    else if (maskByte == 1)
//                                        MODEL.DrawMask[i] = true;
//                                }
//                            }
//                            else if (MODEL.ChrAsm != null && evBox.TypeName == "HideEquippedWeapon")
//                            {
//                                if (evBox.PlaybackHighlight)
//                                {
//                                    if ((bool)evBox.Parameters["RightHand"])
//                                    {
//                                        MODEL.ChrAsm.SetRightWeaponVisible(false);
//                                    }

//                                    if ((bool)evBox.Parameters["LeftHand"])
//                                    {
//                                        MODEL.ChrAsm.SetLeftWeaponVisible(false);
//                                    }
//                                }

//                            }
//                            else if (evBox.PlaybackHighlight)
//                            {
//                                if (evBox.TypeName == "ShowModelMask")
//                                {
//                                    for (int i = 0; i < maskLength; i++)
//                                    {
//                                        if ((bool)evBox.Parameters[$"Mask{(i + 1)}"])
//                                                MODEL.DrawMask[i] = true;
//                                    }
//                                }
//                                else if (evBox.TypeName == "HideModelMask")
//                                {
//                                    for (int i = 0; i < maskLength; i++)
//                                    {
//                                        if ((bool)evBox.Parameters[$"Mask{(i + 1)}"])
//                                                MODEL.DrawMask[i] = false;
//                                    }
//                                }
//                            }
//                        },
//                        EnterAction = (entry, evBox) =>
//                        {
//                            //if (evBox.TypeName == "ShowModelMask" || evBox.TypeName == "HideModelMask")
//                            //{
//                            //    //if (entry.Vars["OriginalMask"] == null)
//                            //    //entry.Vars["OriginalMask"] = new EventSimEntryVariableContainer();

//                            //    //var maskVar = (EventSimEntryVariableContainer)(entry.Vars["OriginalMask"]);

//                            //    //var newMaskCopy = maskVar[evBox] != null ? ((bool[])maskVar[evBox]) : new bool[MODEL.DrawMask.Length];

//                            //    //Array.Copy(MODEL.DrawMask, newMaskCopy, MODEL.DrawMask.Length);
//                            //    //maskVar[evBox] = newMaskCopy;
//                            //}

//                            //for (int i = 0; i < 32; i++)
//                            //{
//                            //    if (evBox.Parameters.Template.ContainsKey($"Mask{(i + 1)}"))
//                            //    {
//                            //        if (evBox.TypeName == "ChangeChrDrawMask")
//                            //        {
//                            //            var maskByte = (byte)evBox.Parameters[$"Mask{(i + 1)}"];

//                            //            // Before you get out the torch, be aware that the game 
//                            //            // uses some value other than 0 to SKIP
//                            //            if (maskByte == 0)
//                            //                MODEL.DrawMask[i] = false;
//                            //            else if (maskByte == 1)
//                            //                MODEL.DrawMask[i] = true;
//                            //        }
//                            //    }
//                            //}
//                        },
//                        DuringAction = (entry, evBox) =>
//                        {
//                            //for (int i = 0; i < 32; i++)
//                            //{
//                            //    if (evBox.Parameters.Template.ContainsKey($"Mask{(i + 1)}"))
//                            //    {
//                            //        if (evBox.TypeName == "ShowModelMask")
//                            //        {
//                            //            if ((bool)evBox.Parameters[$"Mask{(i + 1)}"])
//                            //                MODEL.DrawMask[i] = true;
//                            //        }
//                            //        else if (evBox.TypeName == "HideModelMask")
//                            //        {
//                            //            if ((bool)evBox.Parameters[$"Mask{(i + 1)}"])
//                            //                MODEL.DrawMask[i] = false;
//                            //        }
//                            //    }
//                            //}
//                        },
//                        ExitAction = (entry, evBox) =>
//                        {
//                             //if (evBox.TypeName == "ShowModelMask" || evBox.TypeName == "HideModelMask")
//                             //{
//                             //    if (entry.Vars["OriginalMask"] != null)
//                             //    {
//                             //        var maskVar = (EventSimEntryVariableContainer)(entry.Vars["OriginalMask"]);
//                             //        if (maskVar != null)
//                             //        {
//                             //            MODEL.DrawMask = (bool[])(maskVar[evBox]);
//                             //        }

//                             //    }
//                             //}
//                        },
//                    }
//                },

//                // DEFAULT EVENT TEMPLATE FOR PASTING
//                /*
//                {
//                    new[] { "EVENT_TYPE_NAME", },
//                    new EventSimEntry("MENU_BAR_NAME")
//                    {
//                        NewAnimSelectedAction = (entry) =>
//                        {

//                        },
//                        SimulationStartAction = (entry) =>
//                        {

//                        },
//                        SimulationScrubAction = (entry) =>
//                        {

//                        },
//                        EnterAction = (entry, evBox) =>
//                        {

//                        },
//                        DuringAction = (entry, evBox) =>
//                        {

//                        },
//                        ExitAction = (entry, evBox) =>
//                        {

//                        },
//                        SimulationEndAction = (entry) =>
//                        {

//                        },
//                    }
//                },
//                */

//            };
//        }

//        //public bool GetSimEnabled(string simName)
//        //{
//        //    if (simName == null)
//        //        return false;

//        //    bool result = false;

//        //    lock (Main.Config._lock_ThreadSensitiveStuff)
//        //    {
//        //        if (!zzz_DocumentManager.CurrentDocument.EditorScreen.Config.EventSimulationsEnabled.ContainsKey(simName))
//        //            zzz_DocumentManager.CurrentDocument.EditorScreen.Config.EventSimulationsEnabled.Add(simName, Entries[simName].IsEnabledByDefault);

//        //        result = zzz_DocumentManager.CurrentDocument.EditorScreen.Config.EventSimulationsEnabled[simName];
//        //    }

//        //    return result;
//        //}

//        //public void SetSimEnabled(string simName, bool enabled)
//        //{
//        //    lock (Main.Config._lock_ThreadSensitiveStuff)
//        //    {
//        //        if (!zzz_DocumentManager.CurrentDocument.EditorScreen.Config.EventSimulationsEnabled.ContainsKey(simName))
//        //            zzz_DocumentManager.CurrentDocument.EditorScreen.Config.EventSimulationsEnabled.Add(simName, enabled);
//        //        else
//        //            zzz_DocumentManager.CurrentDocument.EditorScreen.Config.EventSimulationsEnabled[simName] = enabled;
//        //    }
//        //}

//        public void OnNewAnimSelected(List<TAE.Event> evBoxes)
//        {


//            if (Graph == null)
//                return;

//            NewSimulationInit(evBoxes);

//            foreach (var kvp in entries)
//            {
//                if (!Main.Config.GetEventSimulationEnabled(kvp.Key))
//                    continue;

//                if (Graph.ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO
//                    && !kvp.Value.AllowDuringRemo)
//                    continue;

//                kvp.Value.NewAnimSelectedAction?.Invoke(kvp.Value, evBoxes);
//            }
//        }

//        private string GetSimEntryForEventBox(TAE.Event evBox)
//        {
//            return entries.FirstOrDefault(kvp => kvp.Value.EventTypes.Contains(evBox.TypeName)).Key;
//        }

//        public void OnSimulationStart(List<TAE.Event> evBoxes)
//        {
//            if (Graph == null)
//                return;

//            foreach (var kvp in entries)
//            {
//                if (!Main.Config.GetEventSimulationEnabled(kvp.Key))
//                    continue;

//                if (Graph.ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO
//                    && !kvp.Value.AllowDuringRemo)
//                    continue;

//                kvp.Value.SimulationStartAction?.Invoke(kvp.Value, evBoxes);
//            }
//        }

//        public void OnSimulationEnd(List<TAE.Event> evBoxes)
//        {
//            if (Graph == null)
//                return;

//            foreach (var kvp in entries)
//            {
//                if (!Main.Config.GetEventSimulationEnabled(kvp.Key))
//                    continue;

//                if (Graph.ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO
//                    && !kvp.Value.AllowDuringRemo)
//                    continue;

//                kvp.Value.SimulationEndAction?.Invoke(kvp.Value, evBoxes);
//            }
//        }

//        public void OnSimulationFrameChange(List<TAE.Event> evBoxes, float time)
//        {
//            if (Graph == null)
//                return;
//            var enabledEntries = entries.Keys.ToList();

//            NewSimulationScrub(time, evBoxes);

//            try
//            {
//                foreach (var kvp in entries)
//                {
//                    bool skipDuringRemo = (Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO
//                    && !kvp.Value.AllowDuringRemo);

//                    if (!skipDuringRemo && Main.Config.GetEventSimulationEnabled(kvp.Key))
//                    {
//                        kvp.Value.SimulationFrameChangePreBoxesAction?.Invoke(kvp.Value, evBoxes, time);
//                    }
//                    else
//                    {
//                        kvp.Value.SimulationFrameChangeDisabledAction?.Invoke(kvp.Value, evBoxes, time);
//                        enabledEntries.Remove(kvp.Key);
//                        continue;
//                    }
//                }
//            }
//            catch (Exception exc)
//            {
//                if (!ErrorLog.HandleException(exc, "Error occurred while simulating events", alwaysPushOnly: true, shortDescForNotif: true))
//                {
//                    Main.WinForm.Close();
//                }
//            }

//            try
//            {

//                var orderedBoxes = evBoxes.OrderBy(evb => evb.StartTime).ToList();

//                foreach (var evBox in orderedBoxes)
//                {
//                    foreach (var entryName in enabledEntries)
//                    {
//                        entries[entryName].SimulationFrameChangePerBoxAction?.Invoke(entries[entryName], evBoxes, evBox, time);

//                        if (evBox.PlaybackHighlight && entries[entryName].DoesEventMatch(evBox))
//                        {
//                            entries[entryName].SimulationFrameChangePerMatchingBoxAction?.Invoke(entries[entryName], evBoxes, evBox, time);
//                        }
//                    }
//                }
//            }
//            catch (Exception exc)
//            {
//                if (!ErrorLog.HandleException(exc, "Error occurred while simulating events", alwaysPushOnly: true, shortDescForNotif: true))
//                {
//                    Main.WinForm.Close();
//                }
//            }

//            try
//            {
//                foreach (var kvp in entries)
//                {
//                    bool skipDuringRemo = (Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO
//                    && !kvp.Value.AllowDuringRemo);

//                    if (!skipDuringRemo && Main.Config.GetEventSimulationEnabled(kvp.Key))
//                    {
//                        kvp.Value.SimulationFrameChangePostBoxesAction?.Invoke(kvp.Value, evBoxes, time);
//                    }
//                }
//            }
//            catch (Exception exc)
//            {
//                if (!ErrorLog.HandleException(exc, "Error occurred while simulating events", alwaysPushOnly: true, shortDescForNotif: true))
//                {
//                    Main.WinForm.Close();
//                }
//            }
//        }

//        public void OnEventEnter(TAE.Event evBox)
//        {
//            if (Graph == null)
//                return;

//            var matchingSim = GetSimEntryForEventBox(evBox);

//            if (!Main.Config.GetEventSimulationEnabled(matchingSim))
//                return;

//            if (Graph.ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO
//                    && !entries[matchingSim]?.AllowDuringRemo == true)
//                return;

//            if (entries[matchingSim] != null)
//                entries[matchingSim].EnterAction?.Invoke(entries[matchingSim], evBox);
//        }

//        public void OnEventMidFrame(TAE.Event evBox)
//        {
//            if (Graph == null)
//                return;

//            var matchingSim = GetSimEntryForEventBox(evBox);

//            if (!Main.Config.GetEventSimulationEnabled(matchingSim))
//                return;

//            if (Graph.ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO
//                    && !entries[matchingSim]?.AllowDuringRemo == true)
//                return;

//            if (entries[matchingSim] != null)
//                entries[matchingSim].DuringAction?.Invoke(entries[matchingSim], evBox);
//        }

//        public void OnEventExit(TAE.Event evBox)
//        {
//            var matchingSim = GetSimEntryForEventBox(evBox);

//            if (!Main.Config.GetEventSimulationEnabled(matchingSim))
//                return;

//            if (Graph.ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO
//                    && !entries[matchingSim]?.AllowDuringRemo == true)
//                return;

//            if (entries[matchingSim] != null)
//                entries[matchingSim].ExitAction?.Invoke(entries[matchingSim], evBox);

//        }

//        public class EventSimEntryVariableContainer
//        {
//            private Dictionary<object, object> Data = new Dictionary<object, object>();
//            public object this[object name]
//            {
//                get
//                {
//                    if (Data.ContainsKey(name))
//                        return Data[name];
//                    else
//                        return null;
//                }
//                set
//                {
//                    if (Data.ContainsKey(name))
//                        Data[name] = value;
//                    else
//                        Data.Add(name, value);
//                }
//            }

//            public void ClearAllData()
//            {
//                Data.Clear();
//            }
//        }

//        public class EventSimEntry
//        {
//            public bool AllowDuringRemo = false;

//            public List<string> EventTypes = new List<string>();

//            public Action<EventSimEntry, List<TAE.Event>> NewAnimSelectedAction;
//            public Action<EventSimEntry, List<TAE.Event>> SimulationStartAction;
//            public Action<EventSimEntry, List<TAE.Event>> SimulationEndAction;

//            public Action<EventSimEntry, List<TAE.Event>, float>
//                SimulationFrameChangePreBoxesAction;

//            public Action<EventSimEntry, List<TAE.Event>, float>
//                SimulationFrameChangePostBoxesAction;

//            public Action<EventSimEntry, List<TAE.Event>, float>
//                SimulationFrameChangeDisabledAction;

//            public Action<EventSimEntry, List<TAE.Event>, TAE.Event, float> 
//                SimulationFrameChangePerBoxAction;

//            public Action<EventSimEntry, List<TAE.Event>, TAE.Event, float> 
//                SimulationFrameChangePerMatchingBoxAction;

//            public Action<EventSimEntry, TAE.Event> EnterAction;
//            public Action<EventSimEntry, TAE.Event> DuringAction;
//            public Action<EventSimEntry, TAE.Event> ExitAction;

//            public EventSimEntryVariableContainer Vars = new EventSimEntryVariableContainer();

//            public readonly string MenuOptionName;
//            public readonly bool IsEnabledByDefault;

//            public bool DoesEventMatch(TAE.Event evBox)
//            {
//                if (evBox.TypeName == null)
//                    return false;

//                return EventTypes.Contains(evBox.TypeName);
//            }

//            public EventSimEntry(string menuOptionName, bool isEnabledByDefault)
//            {
//                MenuOptionName = menuOptionName;
//                IsEnabledByDefault = isEnabledByDefault;
//            }

//            public EventSimEntry(string menuOptionName, bool isEnabledByDefault, params string[] eventTypes)
//            {
//                MenuOptionName = menuOptionName;
//                IsEnabledByDefault = isEnabledByDefault;
//                EventTypes = eventTypes.ToList();
//            }
//        }
//    }
//}
