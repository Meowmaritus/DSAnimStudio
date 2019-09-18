using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class DummyPolyManager
    {
        public readonly Model MODEL;

        public DummyPolyManager(Model mdl)
        {
            MODEL = mdl;
        }

        private object _lock_hitboxes = new object();

        public bool UseDummyPolyAnimation = true;

        public bool IsViewingLeftHandHit = false;

        public DbgPrimDummyPolyCluster StationaryDummyPolys;

        public Dictionary<int, DbgPrimDummyPolyCluster> AnimatedDummyPolyClusters = new Dictionary<int, DbgPrimDummyPolyCluster>();
        public Dictionary<int, List<DbgPrimDummyPolyCluster>> ClusterWhichDmyPresidesIn
            = new Dictionary<int, List<DbgPrimDummyPolyCluster>>();

        public class HitboxPrimInfo
        {
            public List<IDbgPrim> Primitives = new List<IDbgPrim>();
            public bool IsLeftHandAtk;

            public void Activate()
            {
                foreach (var prim in Primitives)
                    prim.EnableDraw = true;
            }

            public void Deactivate()
            {
                foreach (var prim in Primitives)
                    prim.EnableDraw = false;
            }
        }

        public Dictionary<ParamData.AtkParam, HitboxPrimInfo> HitboxPrimitiveInfos 
            = new Dictionary<ParamData.AtkParam, HitboxPrimInfo>();

        public void ClearAllHitboxPrimitives()
        {
            lock (_lock_hitboxes)
            {
                foreach (var hitboxKvp in HitboxPrimitiveInfos)
                {
                    foreach (var hitbox in hitboxKvp.Value.Primitives)
                        MODEL.DbgPrimDrawer.RemovePrimitive(hitbox);

                    hitboxKvp.Value.Primitives.Clear();
                }

                HitboxPrimitiveInfos.Clear();
            }
        }

        public Vector3? GetDummyPolyAbsolutePosition(int dmy, bool leftHandDefault)
        {
            //if (dmy == -1)
            //{
            //    return Vector3.Transform(new Vector3(0, GFX.World.ModelHeight_ForOrbitCam / 2, 0), TaeInterop.CurrentRootMotionMatrix);
            //}

            if ((dmy >= 100 && dmy <= 130 && !leftHandDefault) || (dmy >= 10000 && dmy <= 10999) || (dmy >= 12000 && dmy <= 12999))
            {
                if (MODEL.ChrAsm != null)
                {
                    if (MODEL.ChrAsm.RightWeaponModel != null)
                    {
                        var rightModelDmyPos = 
                            MODEL.ChrAsm.RightWeaponModel.DummyPolyMan.GetDummyPolyAbsolutePosition(dmy % 1000, leftHandDefault);
                        if (rightModelDmyPos != null)
                        {
                            return Vector3.Transform(rightModelDmyPos.Value,
                                MODEL.ChrAsm.RightWeaponModel.CurrentTransform.WorldMatrix
                                * Matrix.Invert(MODEL.CurrentRootMotionTransform.WorldMatrix));
                        }

                    }
                }

            }

            if ((dmy >= 100 && dmy <= 130) || (dmy >= 11000 && dmy <= 11999) || (dmy >= 13000 && dmy <= 13999))
            {
                if (MODEL.ChrAsm != null)
                {
                    if (MODEL.ChrAsm.LeftWeaponModel != null)
                    {
                        var leftModelDmyPos = 
                            MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan.GetDummyPolyAbsolutePosition(dmy % 1000, leftHandDefault);
                        if (leftModelDmyPos != null)
                        {
                            return Vector3.Transform(leftModelDmyPos.Value,
                                MODEL.ChrAsm.LeftWeaponModel.CurrentTransform.WorldMatrix
                                * Matrix.Invert(MODEL.CurrentRootMotionTransform.WorldMatrix));
                        }

                    }
                }
            }

            if (ClusterWhichDmyPresidesIn.ContainsKey(dmy))
                return ClusterWhichDmyPresidesIn[dmy][0].GetDummyPosition(dmy, isAbsolute: true);
            else if (StationaryDummyPolys != null && StationaryDummyPolys.DummyPolyID.Contains(dmy))
                return StationaryDummyPolys.GetDummyPosition(dmy, isAbsolute: true);
            else
            {
                if (MODEL.ChrAsm != null)
                {
                    if (!leftHandDefault && MODEL.ChrAsm.RightWeaponModel != null)
                    {
                        var rightModelDmyPos = 
                            MODEL.ChrAsm.RightWeaponModel.DummyPolyMan.GetDummyPolyAbsolutePosition(dmy, leftHandDefault);
                        if (rightModelDmyPos != null)
                        {
                            return Vector3.Transform(rightModelDmyPos.Value,
                                MODEL.ChrAsm.RightWeaponModel.CurrentTransform.WorldMatrix
                                * Matrix.Invert(MODEL.CurrentRootMotionTransform.WorldMatrix));
                        }

                    }

                    if (MODEL.ChrAsm.LeftWeaponModel != null)
                    {
                        var leftModelDmyPos = 
                            MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan.GetDummyPolyAbsolutePosition(dmy, leftHandDefault);
                        if (leftModelDmyPos != null)
                        {
                            return Vector3.Transform(leftModelDmyPos.Value,
                                MODEL.ChrAsm.LeftWeaponModel.CurrentTransform.WorldMatrix
                                * Matrix.Invert(MODEL.CurrentRootMotionTransform.WorldMatrix));
                        }

                    }
                }
            }

            return null;
        }

        public Matrix? GetDummyPolyAbsoluteMatrix(int dmy, bool leftHandDefault)
        {
            //if (dmy == -1)
            //{
            //    return Matrix.CreateTranslation(0, GFX.World.ModelHeight_ForOrbitCam / 2, 0) * TaeInterop.CurrentRootMotionMatrix;
            //}

            if ((dmy >= 100 && dmy <= 130 && !leftHandDefault) || (dmy >= 10000 && dmy <= 10999) || (dmy >= 12000 && dmy <= 12999))
            {
                if (MODEL.ChrAsm != null)
                {
                    if (MODEL.ChrAsm.RightWeaponModel != null)
                    {
                        var rightModelDmyMatrix = 
                            MODEL.ChrAsm.RightWeaponModel.DummyPolyMan.GetDummyPolyAbsoluteMatrix(dmy % 1000, leftHandDefault);

                        if (rightModelDmyMatrix != null)
                        {
                            return rightModelDmyMatrix.Value 
                                * MODEL.ChrAsm.RightWeaponModel.CurrentTransform.WorldMatrix
                                * Matrix.Invert(MODEL.CurrentRootMotionTransform.WorldMatrix);
                        }

                    }
                }

            }

            if ((dmy >= 100 && dmy <= 130) || (dmy >= 11000 && dmy <= 11999) || (dmy >= 13000 && dmy <= 13999))
            {
                if (MODEL.ChrAsm != null)
                {
                    if (MODEL.ChrAsm.LeftWeaponModel != null)
                    {
                        var leftModelDmyMatrix = 
                            MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan.GetDummyPolyAbsoluteMatrix(dmy % 1000, leftHandDefault);
                        if (leftModelDmyMatrix != null)
                        {
                            return leftModelDmyMatrix.Value
                                * MODEL.ChrAsm.LeftWeaponModel.CurrentTransform.WorldMatrix
                                * Matrix.Invert(MODEL.CurrentRootMotionTransform.WorldMatrix);
                        }

                    }
                }
            }

            if (ClusterWhichDmyPresidesIn.ContainsKey(dmy))
                return ClusterWhichDmyPresidesIn[dmy][0].GetDummyMatrix(dmy, isAbsolute: true);
            else if (StationaryDummyPolys != null && StationaryDummyPolys.DummyPolyID.Contains(dmy))
                return StationaryDummyPolys.GetDummyMatrix(dmy, isAbsolute: true);
            else
            {
                if (MODEL.ChrAsm != null)
                {
                    if (!leftHandDefault && MODEL.ChrAsm.RightWeaponModel != null)
                    {
                        var rightModelDmyMatrix = MODEL.ChrAsm.RightWeaponModel.DummyPolyMan.GetDummyPolyAbsoluteMatrix(dmy, leftHandDefault);
                        if (rightModelDmyMatrix != null)
                        {
                            return rightModelDmyMatrix.Value
                                * MODEL.ChrAsm.RightWeaponModel.CurrentTransform.WorldMatrix
                                * Matrix.Invert(MODEL.CurrentRootMotionTransform.WorldMatrix);
                        }

                    }

                    if (MODEL.ChrAsm.LeftWeaponModel != null)
                    {
                        var leftModelDmyMatrix = MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan.GetDummyPolyAbsoluteMatrix(dmy, leftHandDefault);
                        if (leftModelDmyMatrix != null)
                        {
                            return leftModelDmyMatrix.Value
                                * MODEL.ChrAsm.LeftWeaponModel.CurrentTransform.WorldMatrix
                                * Matrix.Invert(MODEL.CurrentRootMotionTransform.WorldMatrix);
                        }

                    }
                }

                return null;

            }
        }

        private void CreateHitboxPrimitive(ParamData.AtkParam atkParam, ParamData.AtkParam.Hit hit, Color c, string primText, bool showText)
        {
            lock (_lock_hitboxes)
            {
                if (!HitboxPrimitiveInfos.ContainsKey(atkParam))
                    HitboxPrimitiveInfos.Add(atkParam, new HitboxPrimInfo());

                //if (MODEL.IS_PLAYER)
                //{
                //    hit.ShiftDmyPolyIDIntoPlayerWpnDmyPolyID(HitboxPrimitiveInfos[atkParam].IsLeftHandAtk || IsViewingLeftHandHit);
                //}

                if (hit.DmyPoly1 == -1 && hit.DmyPoly2 == -1)
                    return;

                if (hit.IsCapsule)
                {
                    var capsule = new DbgPrimWireCapsule(12, c)
                    {
                        Category = DbgPrimCategory.DummyPolyHelper,
                        EnableDraw = false,
                        OverrideColor = c,
                        Name = showText ? primText : "",
                        EnableNameDraw = showText,
                        NameColor = c,
                    };
                    MODEL.DbgPrimDrawer.AddPrimitive(capsule);
                    HitboxPrimitiveInfos[atkParam].Primitives.Add(capsule);
                }
                else
                {
                    var sphere = new DbgPrimWireSphere(Transform.Default, 1, 12, 12, Color.White)
                    {
                        Category = DbgPrimCategory.DummyPolyHelper,
                        EnableDraw = false,
                        OverrideColor = c,
                        Name = showText ? primText : "",
                        EnableNameDraw = showText,
                        NameColor = c,
                    };
                    MODEL.DbgPrimDrawer.AddPrimitive(sphere);
                    HitboxPrimitiveInfos[atkParam].Primitives.Add(sphere);
                }
            }
        }

        public void UpdateAllHitboxPrimitives()
        {
            lock (_lock_hitboxes)
            {
                foreach (var atkParam in HitboxPrimitiveInfos.Keys)
                {
                    UpdateHitboxPrimitive(atkParam);
                }
            }
        }

        private void UpdateHitboxPrimitive(ParamData.AtkParam atkParam)
        {
            if (!HitboxPrimitiveInfos.ContainsKey(atkParam))
                return;

            for (int i = 0; i < atkParam.Hits.Length; i++)
            {
                if (i < HitboxPrimitiveInfos[atkParam].Primitives.Count)
                {
                    if (!HitboxPrimitiveInfos[atkParam].Primitives[i].EnableDraw)
                        continue;

                    //if (MODEL.IS_PLAYER)
                    //    atkParam.Hits[i].ShiftDmyPolyIDIntoPlayerWpnDmyPolyID(HitboxPrimitiveInfos[atkParam].IsLeftHandAtk);

                    if (HitboxPrimitiveInfos[atkParam].Primitives[i] is DbgPrimWireCapsule capsulePrim)
                    {
                        if (atkParam.Hits[i].DmyPoly1 == -1)
                            continue;

                        var a = GetDummyPolyAbsolutePosition(atkParam.Hits[i].DmyPoly1, 
                            HitboxPrimitiveInfos[atkParam].IsLeftHandAtk);
                        var b = GetDummyPolyAbsolutePosition(atkParam.Hits[i].DmyPoly2, 
                            HitboxPrimitiveInfos[atkParam].IsLeftHandAtk);

                        //if (a == null || b == null)
                        //{
                        //    a = MODEL.ParentModelForChrAsm?.DummyPolyMan?.GetDummyPolyAbsolutePosition(atkParam.Hits[i].DmyPoly1);
                        //    b = MODEL.ParentModelForChrAsm?.DummyPolyMan?.GetDummyPolyAbsolutePosition(atkParam.Hits[i].DmyPoly2);
                        //}

                        if (a != null && b != null)
                        {
                            capsulePrim.UpdateCapsuleEndPoints(a.Value, b.Value, atkParam.Hits[i].Radius);
                            
                        }
                        else
                        {
                            capsulePrim.UpdateCapsuleEndPoints(Vector3.Zero, Vector3.Zero, 0);
                        }
                    }
                    else if (HitboxPrimitiveInfos[atkParam].Primitives[i] is DbgPrimWireSphere spherePrim)
                    {
                        if (atkParam.Hits[i].DmyPoly1 == -1)
                            continue;

                        var dmyPos = GetDummyPolyAbsolutePosition(atkParam.Hits[i].DmyPoly1,
                            HitboxPrimitiveInfos[atkParam].IsLeftHandAtk);
                        var dmyMatrix = GetDummyPolyAbsoluteMatrix(atkParam.Hits[i].DmyPoly1,
                            HitboxPrimitiveInfos[atkParam].IsLeftHandAtk);

                        //if (dmyPos == null || dmyMatrix == null)
                        //{
                        //    dmyPos = MODEL.ParentModelForChrAsm?.DummyPolyMan?.GetDummyPolyAbsolutePosition(atkParam.Hits[i].DmyPoly1);
                        //    dmyMatrix = MODEL.ParentModelForChrAsm?.DummyPolyMan?.GetDummyPolyAbsoluteMatrix(atkParam.Hits[i].DmyPoly1);
                        //}

                        if (dmyPos != null && dmyMatrix != null)
                        {
                            var dmyRot = Quaternion.CreateFromRotationMatrix(dmyMatrix.Value);
                            dmyRot.Normalize();
                            spherePrim.Transform = new Transform(//Matrix.CreateRotationX(MathHelper.PiOver2) *
                                Matrix.CreateScale(atkParam.Hits[i].Radius)
                                * Matrix.CreateFromQuaternion(dmyRot)
                                * Matrix.CreateTranslation(dmyPos.Value));
                        }
                        else
                        {
                            spherePrim.Transform = new Transform(Vector3.Zero, Vector3.Zero, Vector3.Zero);
                        }

                        
                    }
                }

                    
            }
        }

        public void RecreateAllHitboxPrimitives(ParamData.EquipParamWeapon wpn, bool leftHand)
        {
            lock (_lock_hitboxes)
            {
                ClearAllHitboxPrimitives();
                BuildAllHitboxPrimitives(wpn, leftHand);
            }
        }

        public void RecreateAllHitboxPrimitives(ParamData.NpcParam npcParam)
        {
            lock (_lock_hitboxes)
            {
                ClearAllHitboxPrimitives();
                BuildAllHitboxPrimitives(npcParam);
            }
        }

        public void DeactivateAllHitboxes()
        {
            lock (_lock_hitboxes)
            {
                foreach (var kvp in HitboxPrimitiveInfos)
                {
                    kvp.Value.Deactivate();
                }
            }

            //if (MODEL.ChrAsm != null)
            //{
            //    if (MODEL.ChrAsm.RightWeaponModel != null)
            //        MODEL.ChrAsm.RightWeaponModel.DummyPolyMan.DeactivateAllHitboxes();

            //    if (MODEL.ChrAsm.LeftWeaponModel != null)
            //        MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan.DeactivateAllHitboxes();
            //}
        }

        public void ActivateAllHitboxes()
        {
            lock (_lock_hitboxes)
            {
                foreach (var kvp in HitboxPrimitiveInfos)
                {
                    kvp.Value.Activate();
                }
            }

            //if (MODEL.ChrAsm != null)
            //{
            //    if (MODEL.ChrAsm.RightWeaponModel != null)
            //        MODEL.ChrAsm.RightWeaponModel.DummyPolyMan.ActivateAllHitboxes();

            //    if (MODEL.ChrAsm.LeftWeaponModel != null)
            //        MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan.ActivateAllHitboxes();
            //}
        }

        public void ActivateHitbox(ParamData.AtkParam attack)
        {
            lock (_lock_hitboxes)
            {
                if (HitboxPrimitiveInfos.ContainsKey(attack))
                {
                    HitboxPrimitiveInfos[attack].Activate();
                }
            }

            //if (MODEL.ChrAsm != null)
            //{
            //    if (MODEL.ChrAsm.RightWeaponModel != null)
            //        MODEL.ChrAsm.RightWeaponModel.DummyPolyMan.ActivateHitbox(attack);

            //    if (MODEL.ChrAsm.LeftWeaponModel != null)
            //        MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan.ActivateHitbox(attack);
            //}
        }

        public void DeactivateHitbox(ParamData.AtkParam attack)
        {
            lock (_lock_hitboxes)
            {
                if (HitboxPrimitiveInfos.ContainsKey(attack))
                {
                    HitboxPrimitiveInfos[attack].Deactivate();
                }
            }

            //if (MODEL.ChrAsm != null)
            //{
            //    if (MODEL.ChrAsm.RightWeaponModel != null)
            //        MODEL.ChrAsm.RightWeaponModel.DummyPolyMan.DeactivateHitbox(attack);

            //    if (MODEL.ChrAsm.LeftWeaponModel != null)
            //        MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan.DeactivateHitbox(attack);
            //}
        }

        public void BuildAllHitboxPrimitives(ParamData.NpcParam npcParam)
        {
            int behaviorStart = 2_00000_000 + (npcParam.BehaviorVariationID * 1_000);
            var behaviors = ParamManager.BehaviorParam.Where(x => (x.Key >= behaviorStart) && (x.Key <= behaviorStart + 999));

            var atkParams = new List<ParamData.AtkParam>();
            foreach (var beh in behaviors)
            {
                if (beh.Value.RefType == 0 && ParamManager.AtkParam_Npc.ContainsKey(beh.Value.RefID))
                {
                    atkParams.Add(ParamManager.AtkParam_Npc[beh.Value.RefID]);
                }
            }

            BuildAllHitboxPrimitives(atkParams);
        }

        public void BuildAllHitboxPrimitives(List<ParamData.AtkParam> atkParams, bool isLeftHandAtk = false)
        {
            foreach (var atk in atkParams)
            {
                lock (_lock_hitboxes)
                {
                    HitboxPrimitiveInfos.Add(atk, new HitboxPrimInfo() { IsLeftHandAtk = isLeftHandAtk });
                }

                for (int i = 0; i < atk.Hits.Length; i++)
                {
                    lock (_lock_hitboxes)
                    {
                        CreateHitboxPrimitive(atk, atk.Hits[i],
                        atk.GetCapsuleColor(atk.Hits[i]), $"ATK {atk.ID}{(atk.ThrowTypeID > 0 ? $"\nTHROW {atk.ThrowTypeID}" : "")}",
                        showText: i == 0);
                    }
                }

            }
        }

        public void BuildAllCommonPCHitboxPrimitives()
        {
            var atkParams = new List<ParamData.AtkParam>();
            foreach (var behKvp in ParamManager.BehaviorParam_PC.Where(kvp => kvp.Key < 1000))
            {
                var beh = ParamManager.BehaviorParam_PC[behKvp.Key];
                if (beh.RefType == 0)
                {
                    // Check for non-existing attack reference memes.
                    if (ParamManager.AtkParam_Pc.ContainsKey(beh.RefID))
                    {
                        var atp = ParamManager.AtkParam_Pc[beh.RefID];
                        if (!atkParams.Contains(atp))
                            atkParams.Add(atp);
                    }
                }
            }

            BuildAllHitboxPrimitives(atkParams, false);
        }

        public void BuildAllHitboxPrimitives(ParamData.EquipParamWeapon wpn, bool leftHand)
        {
            bool isValidWeaponBehavior(long rowID)
            {
                return ((rowID >= 10_0000_000 + (wpn.BehaviorVariationID * 1_000)) &&
                    (rowID <= 10_0000_000 + (wpn.BehaviorVariationID * 1_000) + 999)) ||
                    ((rowID >= 10_0000_000 + ((wpn.WepMotionCategory * 100) * 1_000)) &&
                    (rowID <= 10_0000_000 + ((wpn.WepMotionCategory * 100) * 1_000) + 999));
            }

            var atkParams = new List<ParamData.AtkParam>();
            foreach (var behKvp in ParamManager.BehaviorParam_PC.Where(kvp => isValidWeaponBehavior(kvp.Key)))
            {
                var beh = ParamManager.BehaviorParam_PC[behKvp.Key];
                if (beh.RefType == 0)
                {
                    // Check for non-existing attack reference memes.
                    if (ParamManager.AtkParam_Pc.ContainsKey(beh.RefID))
                    {
                        var atp = ParamManager.AtkParam_Pc[beh.RefID];
                        if (!atkParams.Contains(atp))
                            atkParams.Add(atp);
                    }
                }
            }

            BuildAllHitboxPrimitives(atkParams, leftHand);
            
            
            
            

            //foreach (var evBox in Main.TAE_EDITOR.Graph.EventBoxes)
            //{
            //    (long ID, ParamData.AtkParam Param) atkParam = (0, null);

            //    bool leftHand = IsViewingLeftHandHit;

            //    if (evBox.MyEvent.TypeName == "InvokeAttackBehavior" || evBox.MyEvent.TypeName == "InvokeThrowDamageBehavior")
            //    {
            //        if (!MODEL.IS_PLAYER)
            //            atkParam = ParamManager.GetNpcBasicAtkParam((int)evBox.MyEvent.Parameters["BehaviorSubID"]);
            //        else
            //            atkParam = ParamManager.GetPlayerBasicAtkParam(IsViewingLeftHandHit ? MODEL.ChrAsm.LeftWeapon : MODEL.ChrAsm.RightWeapon, 
            //                (int)evBox.MyEvent.Parameters["BehaviorSubID"], isLeftHand: IsViewingLeftHandHit);
            //    }
            //    else if (evBox.MyEvent.TypeName == "InvokeCommonBehavior")
            //    {
            //        atkParam = ParamManager.GetPlayerCommonAttack((int)evBox.MyEvent.Parameters["BehaviorParamID"]);
            //    }
            //    else if (evBox.MyEvent.TypeName == "InvokePCBehavior")
            //    {
            //        if (MODEL.IS_PLAYER)
            //        {
            //            int condition = (int)evBox.MyEvent.Parameters["Condition"];
            //            if (condition == 4)
            //            {
            //                atkParam = ParamManager.GetPlayerCommonAttack((int)evBox.MyEvent.Parameters["BehaviorSubID"]);
            //            }
            //            else if (condition == 8)
            //            {
            //                atkParam = ParamManager.GetPlayerBasicAtkParam(
            //                    IsViewingLeftHandHit ? MODEL.ChrAsm.LeftWeapon : MODEL.ChrAsm.RightWeapon,
            //                    (int)evBox.MyEvent.Parameters["BehaviorSubID"], isLeftHand: IsViewingLeftHandHit);
            //            }
            //            else if (condition == 2 || condition == 8)
            //            {
            //                atkParam = ParamManager.GetPlayerBasicAtkParam(
            //                    IsViewingLeftHandHit ? MODEL.ChrAsm.LeftWeapon : MODEL.ChrAsm.RightWeapon, 
            //                    (int)evBox.MyEvent.Parameters["BehaviorSubID"], isLeftHand: true);
            //                leftHand = true;
            //            }
            //            else
            //            {
            //                Console.WriteLine($"Unknown InvokePCBehavior condition: {condition}");
            //            }
            //        }
            //    }


            //    if (atkParam.Param == null)
            //        continue;

            //    HitboxPrimitiveInfos.Add(atkParam.Param, new HitboxPrimInfo() { IsLeftHandAtk = leftHand });

            //    for (int i = 0; i < atkParam.Param.Hits.Length; i++)
            //    {
            //        CreateHitboxPrimitive(atkParam.Param, atkParam.Param.Hits[i],
            //            atkParam.Param.GetCapsuleColor(atkParam.Param.Hits[i]), $"ATK {atkParam.ID}{(atkParam.Param.ThrowTypeID > 0 ? $"\nTHROW {atkParam.Param.ThrowTypeID}" : "")}",
            //            showText: i == 0);
            //    }
            //}
        }

        public void LoadDummiesFromFLVER(FLVER2 flver, int baseDmyPolyID = 0)
        {
            MODEL.DbgPrimDrawer.ClearPrimitives(DbgPrimCategory.DummyPoly);

            AnimatedDummyPolyClusters = new Dictionary<int, DbgPrimDummyPolyCluster>();
            ClusterWhichDmyPresidesIn = new Dictionary<int, List<DbgPrimDummyPolyCluster>>();

            var dummiesByID = new Dictionary<int, List<FLVER2.Dummy>>();

            foreach (var dmy in flver.Dummies)
            {
                if (dummiesByID.ContainsKey(dmy.AttachBoneIndex))
                {
                    dummiesByID[dmy.AttachBoneIndex].Add(dmy);
                }
                else
                {
                    dummiesByID.Add(dmy.AttachBoneIndex, new List<FLVER2.Dummy> { dmy });
                }
            }

            List<FLVER2.Dummy> stationaryDmy = new List<FLVER2.Dummy>();

            foreach (var kvp in dummiesByID)
            {
                if (kvp.Key >= 0)
                {
                    var dmyPrim = new DbgPrimDummyPolyCluster(0.5f, kvp.Value, flver.Bones, baseDmyPolyID);
                    foreach (var dmy in kvp.Value)
                    {
                        if (!ClusterWhichDmyPresidesIn.ContainsKey(dmy.ReferenceID + baseDmyPolyID))
                        {
                            ClusterWhichDmyPresidesIn.Add(dmy.ReferenceID + baseDmyPolyID, new List<DbgPrimDummyPolyCluster>());
                        }

                        if (!ClusterWhichDmyPresidesIn[dmy.ReferenceID + baseDmyPolyID].Contains(dmyPrim))
                            ClusterWhichDmyPresidesIn[dmy.ReferenceID + baseDmyPolyID].Add(dmyPrim);

                    }
                    MODEL.DbgPrimDrawer.AddPrimitive(dmyPrim);
                    AnimatedDummyPolyClusters.Add(kvp.Key, dmyPrim);
                }
            }

            if (dummiesByID.ContainsKey(-1))
            {
                StationaryDummyPolys = new DbgPrimDummyPolyCluster(0.5f, dummiesByID[-1], flver.Bones, baseDmyPolyID);
                MODEL.DbgPrimDrawer.AddPrimitive(StationaryDummyPolys);
            }
            else
            {
                if (StationaryDummyPolys != null)
                    MODEL.DbgPrimDrawer.RemovePrimitive(StationaryDummyPolys);
                StationaryDummyPolys = null;
            }



            Console.WriteLine("TEST");
        }

    }
}
