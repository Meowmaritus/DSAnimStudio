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

        public static DbgPrimDummyPolyCluster[] AnimatedDummyPolyClusters;
        public static Dictionary<int, List<DbgPrimDummyPolyCluster>> ClusterWhichDmyPresidesIn
            = new Dictionary<int, List<DbgPrimDummyPolyCluster>>();

        public static Dictionary<TaeEditor.TaeEditAnimEventBox, List<IDbgPrim>> HitboxPrimitives
            = new Dictionary<TaeEditor.TaeEditAnimEventBox, List<IDbgPrim>>();

        public static void ClearAllHitboxPrimitives()
        {
            foreach (var hitboxKvp in HitboxPrimitives)
            {
                foreach (var hitbox in hitboxKvp.Value)
                {
                    DBG.RemovePrimitive(hitbox);
                }
                hitboxKvp.Value.Clear();
            }
            HitboxPrimitives.Clear();
        }

        public static Vector3 GetDummyPolyAbsolutePosition(int dmy)
        {
            if (ClusterWhichDmyPresidesIn.ContainsKey(dmy))
            {
                return ClusterWhichDmyPresidesIn[dmy][0].GetDummyPosition(dmy, isAbsolute: true);
            }
            else
            {
                return Vector3.Zero;
            }
        }

        private static void CreateHitboxPrimitive(TaeEditor.TaeEditAnimEventBox evBox, ParamData.AtkParam.Hit hit)
        {
            if (!HitboxPrimitives.ContainsKey(evBox))
                HitboxPrimitives.Add(evBox, new List<IDbgPrim>());

            if (hit.IsCapsule)
            {
                var capsule = new DbgPrimWireCapsule(12, Color.Cyan)
                {
                    Category = DbgPrimCategory.DummyPolyHelper,
                    EnableDraw = false,
                };
                DBG.AddPrimitive(capsule);
                HitboxPrimitives[evBox].Add(capsule);
            }
            else
            {
                var sphere = new DbgPrimWireSphere(Transform.Default, 1, 12, 12, Color.Cyan)
                {
                    Category = DbgPrimCategory.DummyPolyHelper,
                    EnableDraw = false,
                    OverrideColor = Color.Cyan,
                };
                DBG.AddPrimitive(sphere);
                HitboxPrimitives[evBox].Add(sphere);
            }
        }

        public static void UpdateHitboxPrimitive(TaeEditor.TaeEditAnimEventBox evBox, ParamData.AtkParam attack)
        {
            if (!HitboxPrimitives.ContainsKey(evBox))
                return;

            for (int i = 0; i < attack.Hits.Length; i++)
            {
                var a = GetDummyPolyAbsolutePosition(attack.Hits[i].DmyPoly1 * (IsViewingLeftHandHit ? -1 : 1));

                if (attack.Hits[i].IsCapsule)
                {
                    var b = GetDummyPolyAbsolutePosition(attack.Hits[i].DmyPoly2 * (IsViewingLeftHandHit ? -1 : 1));

                    var capsulePrim = (DbgPrimWireCapsule)(HitboxPrimitives[evBox][i]);

                    capsulePrim.UpdateCapsuleEndPoints(a, b, attack.Hits[i].Radius);
                }
                else
                {
                    var spherePrim = (DbgPrimWireSphere)(HitboxPrimitives[evBox][i]);

                    spherePrim.Transform = new Transform(a, Vector3.Zero, Vector3.One * attack.Hits[i].Radius);
                }
            }
        }

        public static void BuildAllHitboxPrimitives()
        {
            foreach (var evBox in Main.TAE_EDITOR.Graph.EventBoxes)
            {
                if (evBox.MyEvent.TypeName == "InvokeAttackBehavior")
                {
                    if (GFX.ModelDrawer.CurrentNpcParamID >= 0)
                    {
                        var atkParam = ParamManager.GetNpcBasicAtkParam((int)evBox.MyEvent.Parameters["BehaviorSubID"]);

                        if (atkParam == null)
                            continue;

                        foreach (var hit in atkParam.Hits)
                        {
                            CreateHitboxPrimitive(evBox, hit);
                        }
                    }
                    else
                    {
                        var atkParam = ParamManager.GetPlayerBasicAtkParam((int)evBox.MyEvent.Parameters["BehaviorSubID"]);

                        if (atkParam == null)
                            continue;

                        foreach (var hit in atkParam.Hits)
                        {
                            CreateHitboxPrimitive(evBox, hit);
                        }
                    }
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
                if (dmy.AttachBoneIndex < 0)
                    continue;

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

            foreach (var kvp in dummiesByID)
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

    }
}
