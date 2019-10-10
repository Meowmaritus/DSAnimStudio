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
    public class NewDummyPolyManager
    {
        public readonly Model MODEL;

        public class DummyPolyInfo
        {
            public int ReferenceID = -1;
            public Matrix ReferenceMatrix = Matrix.Identity;
            public int FollowBoneIndex = -1;
            public Matrix CurrentMatrix = Matrix.Identity;

            public ParamData.AtkParam ShowAttack = null;

            public IDbgPrim ArrowPrimitive = null;

            public List<int> SFXSpawnIDs = new List<int>();
            public List<int> BulletSpawnIDs = new List<int>();
            public List<string> MiscSpawnTexts = new List<string>();

            public StatusPrinter SpawnPrinter = new StatusPrinter(null);

            public bool DisableTextDraw = false;

            public DummyPolyInfo(FLVER2.Dummy dmy, NewAnimSkeleton skeleton)
            {
                ReferenceID = dmy.ReferenceID;
                FollowBoneIndex = dmy.AttachBoneIndex;
                ReferenceMatrix = Matrix.CreateWorld(
                    Vector3.Zero,
                    Vector3.Normalize(new Vector3(dmy.Forward.X, dmy.Forward.Y, dmy.Forward.Z)),
                    dmy.UseUpwardVector ? Vector3.Normalize(new Vector3(dmy.Upward.X, dmy.Upward.Y, dmy.Upward.Z)) : Vector3.Up)
                    * Matrix.CreateTranslation(new Vector3(dmy.Position.X, dmy.Position.Y, dmy.Position.Z))
                    * (dmy.DummyBoneIndex >= 0 ? skeleton.FlverSkeleton[dmy.DummyBoneIndex].ReferenceMatrix : Matrix.Identity);
                CurrentMatrix = ReferenceMatrix;

                ArrowPrimitive = new DbgPrimWireArrow("DummyPoly Spawns", Transform.Default, Color.White)
                {
                    //Wireframe = true,
                    //BackfaceCulling = true,
                    //DisableLighting = true,
                    Category = DbgPrimCategory.DummyPolySpawnArrow,
                    OverrideColor = Color.Cyan,
                };

                SpawnPrinter.Font = DBG.DEBUG_FONT_VERY_SMALL;
                SpawnPrinter.FullyOutlined = true;
            }

            private Color GetCurrentSpawnColor()
            {
                if (SFXSpawnIDs.Count > 0 && (BulletSpawnIDs.Count > 0 || MiscSpawnTexts.Count > 0))
                    return Color.Lime;
                else if (SFXSpawnIDs.Count > 0 && (BulletSpawnIDs.Count == 0 && MiscSpawnTexts.Count == 0))
                    return Color.Cyan;
                else if (SFXSpawnIDs.Count == 0 && (BulletSpawnIDs.Count > 0 || MiscSpawnTexts.Count > 0))
                    return Color.Yellow;
                else
                    return DBG.COLOR_DUMMY_POLY;
            }

            public void DrawPrim(Matrix world)
            {
                bool generalDummyPolyDraw = DBG.CategoryEnableDraw[DbgPrimCategory.DummyPoly];

                bool hasSpawnStuff = !(SFXSpawnIDs.Count == 0 &&
                    BulletSpawnIDs.Count == 0 && MiscSpawnTexts.Count == 0);

                if (!generalDummyPolyDraw && !hasSpawnStuff)
                    return;

                Vector3 currentPos = Vector3.Transform(Vector3.Zero, CurrentMatrix);
                //Vector3 currentDir = Vector3.TransformNormal(Vector3.Forward, CurrentMatrix);
                Quaternion currentRot = Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(CurrentMatrix));

                Matrix unscaledCurrentMatrix = Matrix.CreateFromQuaternion(currentRot) * Matrix.CreateTranslation(currentPos);

                if (hasSpawnStuff)
                {
                    ArrowPrimitive.OverrideColor = GetCurrentSpawnColor();

                    ArrowPrimitive.Draw(null, Matrix.CreateScale(0.4f) * unscaledCurrentMatrix * world);
                }

                if (generalDummyPolyDraw)
                {
                    ArrowPrimitive.OverrideColor = DBG.COLOR_DUMMY_POLY;
                    ArrowPrimitive.Draw(null, Matrix.CreateScale(0.2f) * unscaledCurrentMatrix * world);
                }
            }

            public void DrawPrimText(Matrix world)
            {
                if (DisableTextDraw)
                    return;

                SpawnPrinter.Clear();

                bool hasSpawnStuff = !(SFXSpawnIDs.Count == 0 && BulletSpawnIDs.Count == 0 && MiscSpawnTexts.Count == 0);

                string dmyIDTxt = (ReferenceID == 200) ? $"[{ReferenceID} (All Over Body)]" : $"[{ReferenceID}]";

                if (hasSpawnStuff)
                    SpawnPrinter.AppendLine(dmyIDTxt, GetCurrentSpawnColor() * 2);
                else if (DBG.CategoryEnableNameDraw[DbgPrimCategory.DummyPoly])
                    SpawnPrinter.AppendLine(dmyIDTxt, DBG.COLOR_DUMMY_POLY * 3);
                

                Vector3 currentPos = Vector3.Transform(Vector3.Zero, CurrentMatrix * world);

                if (hasSpawnStuff)
                {
                    foreach (var sfx in SFXSpawnIDs)
                    {
                        SpawnPrinter.AppendLine($"SFX {sfx}", Color.Cyan * 2);
                    }

                    foreach (var bullet in BulletSpawnIDs)
                    {
                        SpawnPrinter.AppendLine($"Bullet {bullet}", Color.Yellow * 2);
                    }

                    foreach (var misc in MiscSpawnTexts)
                    {
                        SpawnPrinter.AppendLine(misc, Color.Yellow * 2);
                    }
                }

                if (ShowAttack != null)
                {
                    string atkName = ShowAttack.Name;
                    if (!string.IsNullOrWhiteSpace(atkName) && atkName.Length > 32)
                    {
                        atkName = atkName.Substring(0, 32) + "...";
                    }
                    SpawnPrinter.AppendLine($"ATK {ShowAttack.ID}"
                        + (!string.IsNullOrWhiteSpace(atkName) ? $" {atkName}" : ""),
                        ShowAttack.Hits[0].GetColor());
                }

                if (SpawnPrinter.LineCount > 0)
                {
                    SpawnPrinter.Position3D = currentPos;
                    SpawnPrinter.Draw();
                }
            }
        }

        private object _lock_everything_monkaS = new object();

        private Queue<ParamData.AtkParam.Hit> VisibleHitsToHideForHideAll = new Queue<ParamData.AtkParam.Hit>();

        private List<DummyPolyInfo> DummyPoly = new List<DummyPolyInfo>();

        private Dictionary<int, List<DummyPolyInfo>> _dummyPolyByRefID = new Dictionary<int, List<DummyPolyInfo>>();
        private Dictionary<int, List<DummyPolyInfo>> _dummyPolyByBoneID = new Dictionary<int, List<DummyPolyInfo>>();

        private Queue<DummyPolyInfo> CurrentSFXSpawnQueueForClearAll = new Queue<DummyPolyInfo>();
        private Queue<DummyPolyInfo> CurrentBulletSpawnQueueForClearAll = new Queue<DummyPolyInfo>();
        private Queue<DummyPolyInfo> CurrentMiscSpawnQueueForClearAll = new Queue<DummyPolyInfo>();
        private Queue<DummyPolyInfo> CurrentShowAttackQueueForClearAll = new Queue<DummyPolyInfo>();

        public void SpawnSFXOnDummyPoly(int sfxID, int dummyPolyID)
        {
            if (!DummyPolyByRefID.ContainsKey(dummyPolyID))
                return;

            foreach (var dmy in DummyPolyByRefID[dummyPolyID])
            {
                dmy.SFXSpawnIDs.Add(sfxID);

                if (!CurrentSFXSpawnQueueForClearAll.Contains(dmy))
                    CurrentSFXSpawnQueueForClearAll.Enqueue(dmy);
            }
        }

        public void SpawnBulletOnDummyPoly(int bulletID, int dummyPolyID)
        {
            if (!DummyPolyByRefID.ContainsKey(dummyPolyID))
                return;

            foreach (var dmy in DummyPolyByRefID[dummyPolyID])
            {
                dmy.BulletSpawnIDs.Add(bulletID);

                if (!CurrentBulletSpawnQueueForClearAll.Contains(dmy))
                    CurrentBulletSpawnQueueForClearAll.Enqueue(dmy);
            }
        }

        public void SpawnMiscOnDummyPoly(string misc, int dummyPolyID)
        {
            if (!DummyPolyByRefID.ContainsKey(dummyPolyID))
                return;

            for (int i = 0; i < DummyPolyByRefID[dummyPolyID].Count; i++)
            {
                if (dummyPolyID == 200 && i > 0)
                    misc = "";

                DummyPolyByRefID[dummyPolyID][i].MiscSpawnTexts.Add(misc);

                if (!CurrentMiscSpawnQueueForClearAll.Contains(DummyPolyByRefID[dummyPolyID][i]))
                    CurrentMiscSpawnQueueForClearAll.Enqueue(DummyPolyByRefID[dummyPolyID][i]);

            }
        }

        public void ShowAttackOnDummyPoly(ParamData.AtkParam atk, int dummyPolyID)
        {
            if (!DummyPolyByRefID.ContainsKey(dummyPolyID))
                return;

            foreach (var dmy in DummyPolyByRefID[dummyPolyID])
            {
                dmy.ShowAttack = atk;

                if (!CurrentShowAttackQueueForClearAll.Contains(dmy))
                    CurrentShowAttackQueueForClearAll.Enqueue(dmy);
            }
        }

        public void ClearAllSFXSpawns()
        {
            while (CurrentSFXSpawnQueueForClearAll.Count > 0)
            {
                var next = CurrentSFXSpawnQueueForClearAll.Dequeue();
                next.SFXSpawnIDs.Clear();
            }
        }

        public void ClearAllBulletSpawns()
        {
            while (CurrentBulletSpawnQueueForClearAll.Count > 0)
            {
                var next = CurrentBulletSpawnQueueForClearAll.Dequeue();
                next.BulletSpawnIDs.Clear();
            }
        }

        public void ClearAllMiscSpawns()
        {
            while (CurrentMiscSpawnQueueForClearAll.Count > 0)
            {
                var next = CurrentMiscSpawnQueueForClearAll.Dequeue();
                next.MiscSpawnTexts.Clear();
            }
        }

        public void ClearAllShowAttacks()
        {
            while (CurrentShowAttackQueueForClearAll.Count > 0)
            {
                var next = CurrentShowAttackQueueForClearAll.Dequeue();
                next.ShowAttack = null;
            }
        }

        private Dictionary<ParamData.AtkParam.Hit, List<IDbgPrim>> HitPrims
            = new Dictionary<ParamData.AtkParam.Hit, List<IDbgPrim>>();

        private Dictionary<ParamData.AtkParam.Hit, ParamData.AtkParam.DummyPolySource> HitFilters
            = new Dictionary<ParamData.AtkParam.Hit, ParamData.AtkParam.DummyPolySource>();

        private void UpdateHitPrim(ParamData.AtkParam.Hit hit)
        {
            lock (_lock_everything_monkaS)
            {
                int dmyPoly1 = hit.DmyPoly1;
                int dmyPoly2 = hit.DmyPoly2;

                var filter = ParamData.AtkParam.DummyPolySource.Body;

                if (HitFilters.ContainsKey(hit))
                {
                    dmyPoly1 = hit.GetFilteredDmyPoly1(HitFilters[hit]);
                    dmyPoly2 = hit.GetFilteredDmyPoly2(HitFilters[hit]);
                    filter = HitFilters[hit];
                }

                if (hit.IsCapsule)
                {
                    if (DummyPolyByRefID.ContainsKey(dmyPoly1) && DummyPolyByRefID.ContainsKey(dmyPoly2))
                    {
                        Vector3 pointA = Vector3.Transform(Vector3.Zero, DummyPolyByRefID[dmyPoly1][0].CurrentMatrix);
                        Vector3 pointB = Vector3.Transform(Vector3.Zero, DummyPolyByRefID[dmyPoly2][0].CurrentMatrix);

                        ((DbgPrimWireCapsule)HitPrims[hit][0]).UpdateCapsuleEndPoints(pointA, pointB, hit.Radius);

                        
                    } 
                    else
                    {
                        ((DbgPrimWireCapsule)HitPrims[hit][0]).UpdateCapsuleEndPoints(Vector3.Zero, Vector3.Zero, hit.Radius);
                    }
                }
                else
                {
                    if (dmyPoly1 == -1 || hit.Radius == 0)
                    {
                        for (int i = 0; i < HitPrims[hit].Count; i++)
                        {
                            HitPrims[hit][i].Transform = new Transform(Matrix.CreateScale(hit.Radius));
                        }
                    }
                    else if (DummyPolyByRefID.ContainsKey(dmyPoly1))
                    {
                        if (DummyPolyByRefID[dmyPoly1].Count != HitPrims[hit].Count)
                            AddNewHitPrim(hit, filter, false, dontUpdate: true);

                        for (int i = 0; i < Math.Min(HitPrims[hit].Count, DummyPolyByRefID[dmyPoly1].Count); i++)
                        {
                            Matrix m = DummyPolyByRefID[dmyPoly1][i].CurrentMatrix;
                            HitPrims[hit][i].Transform = new Transform(Matrix.CreateScale(hit.Radius) * m);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < HitPrims[hit].Count; i++)
                        {
                            HitPrims[hit][i].Transform = new Transform(Matrix.CreateScale(hit.Radius));
                        }
                    }
                   
                }
            }
         
        }

        public void UpdateAllHitPrims()
        {
            lock (_lock_everything_monkaS)
            {
                if (DummyPolyByBoneID.ContainsKey(-1))
                {
                    foreach (var dmy in DummyPolyByBoneID[-1])
                    {
                        dmy.CurrentMatrix = dmy.ReferenceMatrix;// * MODEL.CurrentTransform.WorldMatrix;
                    }
                }

                foreach (var kvp in HitPrims)
                    UpdateHitPrim(kvp.Key);
            }
        }

        public void DrawAllHitPrims()
        {
            lock (_lock_everything_monkaS)
            {
                foreach (var kvp in HitPrims)
                {
                    foreach (var prim in kvp.Value)
                    {
                        prim.Draw(null, MODEL.CurrentTransform.WorldMatrix);
                    }
                }

                foreach (var dmy in DummyPoly)
                {
                    dmy.DrawPrim(MODEL.CurrentTransform.WorldMatrix);
                }
            }

        }

        public void DrawAllHitPrimTexts()
        {
            lock (_lock_everything_monkaS)
            {
                foreach (var dmy in DummyPoly)
                {
                    dmy.DrawPrimText(MODEL.CurrentTransform.WorldMatrix);
                }
            }

        }

        private void AddNewHitPrim(ParamData.AtkParam.Hit hit, ParamData.AtkParam.DummyPolySource dmyFilter, bool visible = false, bool dontUpdate = false)
        {
            if (!HitPrims.ContainsKey(hit))
                HitPrims.Add(hit, new List<IDbgPrim>());

            if (!HitFilters.ContainsKey(hit))
                HitFilters.Add(hit, dmyFilter);
            else
                HitFilters[hit] = dmyFilter;

            if (hit.IsCapsule)
            {
                HitPrims[hit].Add(new DbgPrimWireCapsule(hit.GetColor()) 
                { 
                    EnableDraw = visible,
                    Category = DbgPrimCategory.DummyPolyHelper,
                    //OverrideColor = hit.GetColor(),
                });
            }
            else
            {
                if (DummyPolyByRefID.ContainsKey(hit.DmyPoly1))
                {
                    foreach (var dmy in DummyPolyByRefID[hit.DmyPoly1])
                    {
                        HitPrims[hit].Add(new DbgPrimWireSphere(new Transform(Matrix.CreateScale(hit.Radius) * 
                            dmy.ReferenceMatrix), Color.White) 
                        { 
                            EnableDraw = visible,
                            Category = DbgPrimCategory.DummyPolyHelper,
                            OverrideColor = hit.GetColor(),
                        });

                    }
                }
            }

            lock (_lock_everything_monkaS)
            {
                if (visible && !VisibleHitsToHideForHideAll.Contains(hit))
                    VisibleHitsToHideForHideAll.Enqueue(hit);

                if (!dontUpdate)
                    UpdateHitPrim(hit);
            }
        }

        public void SetHitVisibility(ParamData.AtkParam.Hit hit, bool visible, ParamData.AtkParam.DummyPolySource dmyFilter)
        {
            lock (_lock_everything_monkaS)
            {
                if (HitPrims.ContainsKey(hit))
                {
                    foreach (var prim in HitPrims[hit])
                        prim.EnableDraw = visible;
                }
                else
                {
                    AddNewHitPrim(hit, dmyFilter, visible);
                }
                if (visible && !VisibleHitsToHideForHideAll.Contains(hit))
                    VisibleHitsToHideForHideAll.Enqueue(hit);
            }
        }

        public void ClearAllHitboxes()
        {
            lock (_lock_everything_monkaS)
            {
                HitPrims.Clear();
                HitFilters.Clear();
                VisibleHitsToHideForHideAll.Clear();
            }

        }

        public void HideAllHitboxes()
        {
            ClearAllShowAttacks();

            while (VisibleHitsToHideForHideAll.Count > 0)
            {
                var nextHitToHide = VisibleHitsToHideForHideAll.Dequeue();
                SetHitVisibility(nextHitToHide, false, ParamData.AtkParam.DummyPolySource.Body);
            }
        }

        public void SetAttackVisibility(ParamData.AtkParam atk, bool visible, ParamData.AtkParam.DummyPolySource dmyFilter)
        {
            bool isFirstValidDmyPoly = true;
            for (int i = 0; i < atk.Hits.Length; i++)
            {
                int dmyPoly1 = atk.Hits[i].GetFilteredDmyPoly1(dmyFilter);

                if (dmyPoly1 == -1)
                {
                    continue;
                }

                if (visible && isFirstValidDmyPoly && dmyPoly1 != -1 && DummyPolyByRefID.ContainsKey(dmyPoly1))
                {
                    ShowAttackOnDummyPoly(atk, dmyPoly1);
                    isFirstValidDmyPoly = false;
                }

                SetHitVisibility(atk.Hits[i], visible, dmyFilter);
            }
        }

        public IReadOnlyDictionary<int, List<DummyPolyInfo>> DummyPolyByRefID => _dummyPolyByRefID;
        public IReadOnlyDictionary<int, List<DummyPolyInfo>> DummyPolyByBoneID => _dummyPolyByBoneID;

        public void UpdateFlverBone(int index, Matrix relativeMatrix)
        {
            lock (_lock_everything_monkaS)
            {
                if (DummyPolyByBoneID.ContainsKey(index))
                {
                    foreach (var d in DummyPolyByBoneID[index])
                    {
                        d.CurrentMatrix = d.ReferenceMatrix * relativeMatrix;
                    }
                }
               
               
            }
        }

        public void AddDummyPoly(FLVER2.Dummy dmy)
        {
            lock (_lock_everything_monkaS)
            {
                var di = new DummyPolyInfo(dmy, MODEL.Skeleton);
                DummyPoly.Add(di);

                if (!_dummyPolyByRefID.ContainsKey(dmy.ReferenceID))
                    _dummyPolyByRefID.Add(dmy.ReferenceID, new List<DummyPolyInfo>());

                if (!_dummyPolyByRefID[dmy.ReferenceID].Contains(di))
                    _dummyPolyByRefID[dmy.ReferenceID].Add(di);

                if (!_dummyPolyByBoneID.ContainsKey(dmy.AttachBoneIndex))
                    _dummyPolyByBoneID.Add(dmy.AttachBoneIndex, new List<DummyPolyInfo>());

                if (!_dummyPolyByBoneID[dmy.AttachBoneIndex].Contains(di))
                    _dummyPolyByBoneID[dmy.AttachBoneIndex].Add(di);
            }    
        }

        public void AddAllDummiesFromFlver(FLVER2 flver)
        {
            foreach (var d in flver.Dummies)
                AddDummyPoly(d);

            if (DummyPolyByRefID.ContainsKey(200))
            {
                for (int i = 0; i < DummyPolyByRefID[200].Count; i++)
                {
                    DummyPolyByRefID[200][i].DisableTextDraw = i > 0;
                }
            }
        }

        public NewDummyPolyManager(Model mdl)
        {
            MODEL = mdl;
        }
    }
}
