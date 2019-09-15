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

        public static Dictionary<TaeEditor.TaeEditAnimEventBox, List<IDbgPrim>> HitboxPrimitives
            = new Dictionary<TaeEditor.TaeEditAnimEventBox, List<IDbgPrim>>();

        public static Dictionary<TaeEditor.TaeEditAnimEventBox, ParamData.AtkParam> HitboxAtkParams
           = new Dictionary<TaeEditor.TaeEditAnimEventBox, ParamData.AtkParam>();

        public static void ClearAllHitboxPrimitives()
        {
            foreach (var hitboxKvp in HitboxPrimitives)
            {
                foreach (var hitbox in hitboxKvp.Value)
                    DBG.RemovePrimitive(hitbox);

                hitboxKvp.Value.Clear();
            }

            HitboxPrimitives.Clear();
            HitboxAtkParams.Clear();
        }

        public static Vector3 GetDummyPolyAbsolutePosition(int dmy)
        {
            if (ClusterWhichDmyPresidesIn.ContainsKey(dmy))
                return ClusterWhichDmyPresidesIn[dmy][0].GetDummyPosition(dmy, isAbsolute: true);
            else if (StationaryDummyPolys != null && StationaryDummyPolys.DummyPolyID.Contains(dmy))
                return StationaryDummyPolys.GetDummyPosition(dmy, isAbsolute: true);
            else
                return Vector3.Zero;
        }

        public static Matrix GetDummyPolyAbsoluteMatrix(int dmy)
        {
            if (ClusterWhichDmyPresidesIn.ContainsKey(dmy))
                return ClusterWhichDmyPresidesIn[dmy][0].GetDummyMatrix(dmy, isAbsolute: true);
            else if (StationaryDummyPolys != null && StationaryDummyPolys.DummyPolyID.Contains(dmy))
                return StationaryDummyPolys.GetDummyMatrix(dmy, isAbsolute: true);
            else
                return Matrix.Identity;
        }

        private static void CreateHitboxPrimitive(TaeEditor.TaeEditAnimEventBox evBox, ParamData.AtkParam.Hit hit, Color c, string primText, bool showText)
        {
            if (!HitboxPrimitives.ContainsKey(evBox))
                HitboxPrimitives.Add(evBox, new List<IDbgPrim>());

            if (TaeInterop.IsPlayerLoaded)
            {
                hit.ShiftDmyPolyIDIntoPlayerWpnDmyPolyID(IsViewingLeftHandHit);
            }

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
                DBG.AddPrimitive(capsule);
                HitboxPrimitives[evBox].Add(capsule);
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
                HitboxPrimitives[evBox].Add(sphere);
            }
        }

        public static void UpdateAllHitboxPrimitives()
        {
            foreach (var evBox in HitboxPrimitives.Keys)
            {
                UpdateHitboxPrimitive(evBox);
            }
        }

        private static void UpdateHitboxPrimitive(TaeEditor.TaeEditAnimEventBox evBox)
        {
            if (!HitboxPrimitives.ContainsKey(evBox))
                return;

            var attack = HitboxAtkParams[evBox];

            for (int i = 0; i < attack.Hits.Length; i++)
            {
                if (i < HitboxPrimitives[evBox].Count)
                {
                    if (TaeInterop.IsPlayerLoaded)
                        attack.Hits[i].ShiftDmyPolyIDIntoPlayerWpnDmyPolyID(IsViewingLeftHandHit);

                    if (attack.Hits[i].IsCapsule)
                    {
                        var a = GetDummyPolyAbsolutePosition(attack.Hits[i].DmyPoly1);
                        var b = GetDummyPolyAbsolutePosition(attack.Hits[i].DmyPoly2);
                        var capsulePrim = (DbgPrimWireCapsule)(HitboxPrimitives[evBox][i]);
                        capsulePrim.UpdateCapsuleEndPoints(a, b, attack.Hits[i].Radius);
                    }
                    else
                    {
                        var dmyPos = GetDummyPolyAbsolutePosition(attack.Hits[i].DmyPoly1);
                        var dmyMatrix = GetDummyPolyAbsoluteMatrix(attack.Hits[i].DmyPoly1);
                        var dmyRot = Quaternion.CreateFromRotationMatrix(dmyMatrix);
                        dmyRot.Normalize();
                        var spherePrim = (DbgPrimWireSphere)(HitboxPrimitives[evBox][i]);
                        spherePrim.Transform = new Transform(//Matrix.CreateRotationX(MathHelper.PiOver2) *
                            Matrix.CreateScale(attack.Hits[i].Radius) 
                            * Matrix.CreateFromQuaternion(dmyRot) 
                            * Matrix.CreateTranslation(dmyPos));
                    }
                }

                    
            }
        }

        public static void BuildAllHitboxPrimitives()
        {
            foreach (var evBox in Main.TAE_EDITOR.Graph.EventBoxes)
            {
                (long ID, ParamData.AtkParam Param) atkParam = (0, null);

                if (evBox.MyEvent.TypeName == "InvokeAttackBehavior")
                {
                    if (!TaeInterop.IsPlayerLoaded)
                        atkParam = ParamManager.GetNpcBasicAtkParam((int)evBox.MyEvent.Parameters["BehaviorSubID"]);
                    else
                        atkParam = ParamManager.GetPlayerBasicAtkParam((int)evBox.MyEvent.Parameters["BehaviorSubID"]);
                }
                else if (evBox.MyEvent.TypeName == "InvokePCBehavior" || evBox.MyEvent.TypeName == "InvokeCommonBehavior")
                {
                    atkParam = ParamManager.GetPlayerCommonAttack((int)evBox.MyEvent.Parameters["BehaviorParamID"]);
                }


                if (atkParam.Param == null)
                    continue;

                HitboxAtkParams.Add(evBox, atkParam.Param);

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
