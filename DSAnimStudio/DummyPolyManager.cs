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
    public static class DummyPolyManager
    {
        public static bool UseDummyPolyAnimation = true;

        // TODO: MAKE THIS ACTUALLY LIKE A LIVE TOGGLE SDFSDJHSKGJH
        public static bool IsViewingLeftHandHit = false;

        public static DbgPrimDummyPolyCluster StationaryDummyPolys;

        public static DbgPrimDummyPolyCluster[] AnimatedDummyPolyClusters;
        public static Dictionary<int, List<DbgPrimDummyPolyCluster>> ClusterWhichDmyPresidesIn
            = new Dictionary<int, List<DbgPrimDummyPolyCluster>>();

        public class HitboxPrimInfo
        {
            public List<IDbgPrim> Primitives = new List<IDbgPrim>();
            public ParamData.AtkParam AtkParam;
            public bool IsLeftHandAtk;
        }

        public static Dictionary<TaeEditor.TaeEditAnimEventBox, HitboxPrimInfo> HitboxPrimitiveInfos 
            = new Dictionary<TaeEditor.TaeEditAnimEventBox, HitboxPrimInfo>();

        public static void ClearAllHitboxPrimitives()
        {
            foreach (var hitboxKvp in HitboxPrimitiveInfos)
            {
                foreach (var hitbox in hitboxKvp.Value.Primitives)
                    DBG.RemovePrimitive(hitbox);

                hitboxKvp.Value.Primitives.Clear();
            }

            HitboxPrimitiveInfos.Clear();
        }

        public static Vector3 GetDummyPolyAbsolutePosition(int dmy)
        {
            if (dmy == -1)
            {
                return Vector3.Transform(new Vector3(0, GFX.World.ModelHeight_ForOrbitCam / 2, 0), TaeInterop.CurrentRootMotionMatrix);
            }

            if (ClusterWhichDmyPresidesIn.ContainsKey(dmy))
                return ClusterWhichDmyPresidesIn[dmy][0].GetDummyPosition(dmy, isAbsolute: true);
            else if (StationaryDummyPolys != null && StationaryDummyPolys.DummyPolyID.Contains(dmy))
                return StationaryDummyPolys.GetDummyPosition(dmy, isAbsolute: true);
            else
                return Vector3.Zero;
        }

        public static Matrix GetDummyPolyAbsoluteMatrix(int dmy)
        {
            if (dmy == -1)
            {
                return Matrix.CreateTranslation(0, GFX.World.ModelHeight_ForOrbitCam / 2, 0) * TaeInterop.CurrentRootMotionMatrix;
            }

            if (ClusterWhichDmyPresidesIn.ContainsKey(dmy))
                return ClusterWhichDmyPresidesIn[dmy][0].GetDummyMatrix(dmy, isAbsolute: true);
            else if (StationaryDummyPolys != null && StationaryDummyPolys.DummyPolyID.Contains(dmy))
                return StationaryDummyPolys.GetDummyMatrix(dmy, isAbsolute: true);
            else
                return Matrix.Identity;
        }

        private static void CreateHitboxPrimitive(TaeEditor.TaeEditAnimEventBox evBox, ParamData.AtkParam.Hit hit, Color c, string primText, bool showText)
        {
            if (!HitboxPrimitiveInfos.ContainsKey(evBox))
                HitboxPrimitiveInfos.Add(evBox, new HitboxPrimInfo());

            if (TaeInterop.IsPlayerLoaded)
            {
                hit.ShiftDmyPolyIDIntoPlayerWpnDmyPolyID(HitboxPrimitiveInfos[evBox].IsLeftHandAtk || IsViewingLeftHandHit);
            }

            //if (hit.DmyPoly1 == -1 && hit.DmyPoly2 == -1)
            //    return;

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
                DBG.AddPrimitive(capsule);
                HitboxPrimitiveInfos[evBox].Primitives.Add(capsule);
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
                DBG.AddPrimitive(sphere);
                HitboxPrimitiveInfos[evBox].Primitives.Add(sphere);
            }
        }

        public static void UpdateAllHitboxPrimitives()
        {
            foreach (var evBox in HitboxPrimitiveInfos.Keys)
            {
                UpdateHitboxPrimitive(evBox);
            }
        }

        private static void UpdateHitboxPrimitive(TaeEditor.TaeEditAnimEventBox evBox)
        {
            if (!HitboxPrimitiveInfos.ContainsKey(evBox))
                return;

            var attack = HitboxPrimitiveInfos[evBox].AtkParam;

            for (int i = 0; i < attack.Hits.Length; i++)
            {
                if (i < HitboxPrimitiveInfos[evBox].Primitives.Count)
                {
                    if (TaeInterop.IsPlayerLoaded)
                        attack.Hits[i].ShiftDmyPolyIDIntoPlayerWpnDmyPolyID(HitboxPrimitiveInfos[evBox].IsLeftHandAtk);

                    if (HitboxPrimitiveInfos[evBox].Primitives[i] is DbgPrimWireCapsule capsulePrim)
                    {
                        var a = GetDummyPolyAbsolutePosition(attack.Hits[i].DmyPoly1);
                        var b = GetDummyPolyAbsolutePosition(attack.Hits[i].DmyPoly2);
                        capsulePrim.UpdateCapsuleEndPoints(a, b, attack.Hits[i].Radius);
                    }
                    else if (HitboxPrimitiveInfos[evBox].Primitives[i] is DbgPrimWireSphere spherePrim)
                    {
                        var dmyPos = GetDummyPolyAbsolutePosition(attack.Hits[i].DmyPoly1);
                        var dmyMatrix = GetDummyPolyAbsoluteMatrix(attack.Hits[i].DmyPoly1);
                        var dmyRot = Quaternion.CreateFromRotationMatrix(dmyMatrix);
                        dmyRot.Normalize();
                        spherePrim.Transform = new Transform(//Matrix.CreateRotationX(MathHelper.PiOver2) *
                            Matrix.CreateScale(attack.Hits[i].Radius) 
                            * Matrix.CreateFromQuaternion(dmyRot) 
                            * Matrix.CreateTranslation(dmyPos));
                    }
                }

                    
            }
        }

        public static void RefreshHitboxPrimitives()
        {
            ClearAllHitboxPrimitives();
            BuildAllHitboxPrimitives();
        }

        public static void BuildAllHitboxPrimitives()
        {
            foreach (var evBox in Main.TAE_EDITOR.Graph.EventBoxes)
            {
                (long ID, ParamData.AtkParam Param) atkParam = (0, null);

                bool leftHand = IsViewingLeftHandHit;

                if (evBox.MyEvent.TypeName == "InvokeAttackBehavior" || evBox.MyEvent.TypeName == "InvokeThrowDamageBehavior")
                {
                    if (!TaeInterop.IsPlayerLoaded)
                        atkParam = ParamManager.GetNpcBasicAtkParam((int)evBox.MyEvent.Parameters["BehaviorSubID"]);
                    else
                        atkParam = ParamManager.GetPlayerBasicAtkParam((int)evBox.MyEvent.Parameters["BehaviorSubID"], isLeftHand: IsViewingLeftHandHit);
                }
                else if (evBox.MyEvent.TypeName == "InvokeCommonBehavior")
                {
                    atkParam = ParamManager.GetPlayerCommonAttack((int)evBox.MyEvent.Parameters["BehaviorParamID"]);
                }
                else if (evBox.MyEvent.TypeName == "InvokePCBehavior")
                {
                    if (TaeInterop.IsPlayerLoaded)
                    {
                        int condition = (int)evBox.MyEvent.Parameters["Condition"];
                        if (condition == 4)
                        {
                            atkParam = ParamManager.GetPlayerCommonAttack((int)evBox.MyEvent.Parameters["BehaviorSubID"]);
                        }
                        else if (condition == 8)
                        {
                            atkParam = ParamManager.GetPlayerBasicAtkParam((int)evBox.MyEvent.Parameters["BehaviorSubID"], isLeftHand: IsViewingLeftHandHit);
                        }
                        else if (condition == 2 || condition == 8)
                        {
                            atkParam = ParamManager.GetPlayerBasicAtkParam((int)evBox.MyEvent.Parameters["BehaviorSubID"], isLeftHand: true);
                            leftHand = true;
                        }
                        else
                        {
                            Console.WriteLine($"Unknown InvokePCBehavior condition: {condition}");
                        }
                    }
                }


                if (atkParam.Param == null)
                    continue;

                HitboxPrimitiveInfos.Add(evBox, new HitboxPrimInfo() { AtkParam = atkParam.Param, IsLeftHandAtk = leftHand });

                for (int i = 0; i < atkParam.Param.Hits.Length; i++)
                {
                    CreateHitboxPrimitive(evBox, atkParam.Param.Hits[i],
                        atkParam.Param.GetCapsuleColor(atkParam.Param.Hits[i]), $"ATK {atkParam.ID}{(atkParam.Param.ThrowTypeID > 0 ? $"\nTHROW {atkParam.Param.ThrowTypeID}" : "")}",
                        showText: i == 0);
                }
            }
        }

        public static void LoadDummiesFromFLVER(FLVER2 flver)
        {
            DBG.ClearPrimitives(DbgPrimCategory.DummyPoly);

            AnimatedDummyPolyClusters = new DbgPrimDummyPolyCluster[TaeInterop.FlverBoneCount];
            ClusterWhichDmyPresidesIn = new Dictionary<int, List<DbgPrimDummyPolyCluster>>();

            var dummiesByID = new Dictionary<int, List<FLVER2.Dummy>>();

            GFX.ModelDrawer.DummySphereInfo.Clear();

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

                if (!GFX.ModelDrawer.DummySphereInfo.ContainsKey(dmy.ReferenceID))
                {
                    GFX.ModelDrawer.DummySphereInfo.Add(dmy.ReferenceID, new ModelDrawer.HitSphereInfo());
                }
            }

            List<FLVER2.Dummy> stationaryDmy = new List<FLVER2.Dummy>();

            foreach (var kvp in dummiesByID)
            {
                if (kvp.Key >= 0)
                {
                    var dmyPrim = new DbgPrimDummyPolyCluster(0.5f, kvp.Value, flver.Bones);
                    foreach (var dmy in kvp.Value)
                    {
                        if (!ClusterWhichDmyPresidesIn.ContainsKey(dmy.ReferenceID))
                        {
                            ClusterWhichDmyPresidesIn.Add(dmy.ReferenceID, new List<DbgPrimDummyPolyCluster>());
                        }

                        if (!ClusterWhichDmyPresidesIn[dmy.ReferenceID].Contains(dmyPrim))
                            ClusterWhichDmyPresidesIn[dmy.ReferenceID].Add(dmyPrim);

                    }
                    DBG.AddPrimitive(dmyPrim);
                    AnimatedDummyPolyClusters[kvp.Key] = dmyPrim;
                }
            }

            if (dummiesByID.ContainsKey(-1))
            {
                StationaryDummyPolys = new DbgPrimDummyPolyCluster(0.5f, dummiesByID[-1], flver.Bones);
                DBG.AddPrimitive(StationaryDummyPolys);
            }
            else
            {
                if (StationaryDummyPolys != null)
                    DBG.RemovePrimitive(StationaryDummyPolys);
                StationaryDummyPolys = null;
            }



            Console.WriteLine("TEST");
        }

    }
}
