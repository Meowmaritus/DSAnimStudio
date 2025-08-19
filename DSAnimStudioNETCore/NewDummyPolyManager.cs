using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;
using System.Windows.Forms;

namespace DSAnimStudio
{
    public class NewDummyPolyManager
    {
        public readonly string GUID = Guid.NewGuid().ToString();
        public Model MODEL;

        public int OverrideUnmappedDummyPolyRefBoneID = -1;
        public bool OverrideDummyPolyFollowFlag_EnableOverride = false;
        public bool OverrideDummyPolyFollowFlag_Value = false;

        public enum AttackDirType
        {
            Any = 0,
            Forward = 1,
            Backward = 2,
            Left = 3,
            Right = 4,
        }

        public static float AttackDirTypeToAngle(AttackDirType dirType)
        {
            switch (dirType)
            {
                case AttackDirType.Forward: return 0;
                case AttackDirType.Backward: return MathHelper.Pi;
                case AttackDirType.Left: return -MathHelper.PiOver2;
                case AttackDirType.Right: return MathHelper.PiOver2;
                default: return 0;
            }
        }

        public enum HitboxSpawnTypes
        {
            None = -1,
            AttackBehavior = 0,
            CommonBehavior = 5,
            ThrowAttackBehavior = 304,
            PCBehavior = 307,
        }
        public class HitboxSlot
        {
            public int SlotID;
            public Dictionary<ParamData.AtkParam.Hit, List<IDbgPrim>> HitPrims = new Dictionary<ParamData.AtkParam.Hit, List<IDbgPrim>>();
            public float ElapsedTime;
            public ParamData.AtkParam Attack;

            public HitboxSpawnTypes HitboxSpawnType;
            //public List<IDbgPrim> HitPrims = new List<IDbgPrim>();
        }

        public struct DummyPolyShowAttackInfo
        {
            public bool IsNpcAtk;
            public int AtkParamID;
            public string AttackName;
            public Color Hit0Color;
            public HitboxSpawnTypes SpawnType;

            public DummyPolyShowAttackInfo(bool isNpcAtk, int atkParamID, string atkName, Color hit0Color, HitboxSpawnTypes spawnType)
            {
                IsNpcAtk = isNpcAtk;
                AtkParamID = atkParamID;
                AttackName = atkName;
                Hit0Color = hit0Color;
                SpawnType = spawnType;
            }
        }


        
        private object _lock_everything_monkaS = new object();

        //private Queue<ParamData.AtkParam.Hit> VisibleHitsToHideForHideAll = new Queue<ParamData.AtkParam.Hit>();

        private List<NewDummyPolyInfo> DummyPoly = new List<NewDummyPolyInfo>();

        private Dictionary<int, bool> _dummyPolyVisibleByRefID = new Dictionary<int, bool>();

        private Dictionary<int, List<NewDummyPolyInfo>> _dummyPolyByRefID = new Dictionary<int, List<NewDummyPolyInfo>>();
        private Dictionary<int, List<NewDummyPolyInfo>> _dummyPolyByBoneID = new Dictionary<int, List<NewDummyPolyInfo>>();
        private Dictionary<int, List<NewDummyPolyInfo>> _dummyPolyByBoneID_Ref = new Dictionary<int, List<NewDummyPolyInfo>>();

        private Queue<NewDummyPolyInfo> CurrentSFXSpawnQueueForClearAll = new Queue<NewDummyPolyInfo>();
        private Queue<NewDummyPolyInfo> CurrentBulletSpawnQueueForClearAll = new Queue<NewDummyPolyInfo>();
        private Queue<NewDummyPolyInfo> CurrentMiscSpawnQueueForClearAll = new Queue<NewDummyPolyInfo>();
        private Queue<NewDummyPolyInfo> CurrentShowAttackQueueForClearAll = new Queue<NewDummyPolyInfo>();

        //public ParamData.AtkParam.DummyPolySource defaultDummyPolySource = ParamData.AtkParam.DummyPolySource.Body;

        public void SpawnSFXOnDummyPoly(int sfxID, int dummyPolyID)
        {
            if (!NewCheckDummyPolyExists(dummyPolyID))
                return;

            foreach (var dmy in NewGetDummyPolyByRefID(dummyPolyID))
            {
                dmy.SFXSpawnIDs.Add(sfxID);

                if (!CurrentSFXSpawnQueueForClearAll.Contains(dmy))
                    CurrentSFXSpawnQueueForClearAll.Enqueue(dmy);
            }
        }

        public void SpawnBulletOnDummyPoly(int bulletID, int dummyPolyID)
        {
            if (!NewCheckDummyPolyExists(dummyPolyID))
                return;

            var dmyList = NewGetDummyPolyByRefID(dummyPolyID);

            foreach (var dmy in dmyList)
            {
                dmy.BulletSpawnIDs.Add(bulletID);

                if (!CurrentBulletSpawnQueueForClearAll.Contains(dmy))
                    CurrentBulletSpawnQueueForClearAll.Enqueue(dmy);
            }
        }

        public void SpawnMiscOnDummyPoly(string misc, int dummyPolyID)
        {
            if (!NewCheckDummyPolyExists(dummyPolyID))
                return;

            var dmyList = NewGetDummyPolyByRefID(dummyPolyID);

            for (int i = 0; i < dmyList.Count; i++)
            {
                if (dummyPolyID == 200 && i > 0)
                    misc = "";

                dmyList[i].MiscSpawnTexts.Add(misc);

                if (!CurrentMiscSpawnQueueForClearAll.Contains(dmyList[i]))
                    CurrentMiscSpawnQueueForClearAll.Enqueue(dmyList[i]);

            }
        }

       

        public void ShowAttackOnDummyPoly(bool npcAtk, ParamData.AtkParam atk, HitboxSpawnTypes spawnType, int dummyPolyID)
        {
            if (!NewCheckDummyPolyExists(dummyPolyID))
                return;

            var dmyList = NewGetDummyPolyByRefID(dummyPolyID);

            foreach (var dmy in dmyList)
            {
                var newAtkInfo =
                    new DummyPolyShowAttackInfo(npcAtk, atk.ID, atk.Name, atk.Hits[0].GetColor(spawnType), spawnType);
                if (!dmy.ShowAttackInfos.Contains(newAtkInfo))
                    dmy.ShowAttackInfos.Add(newAtkInfo);

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
                next.ShowAttackInfos.Clear();
            }
        }

        //private Dictionary<ParamData.AtkParam.Hit, List<IDbgPrim>> HitPrims
        //    = new Dictionary<ParamData.AtkParam.Hit, List<IDbgPrim>>();

        public List<HitboxSlot> HitboxSlots = new List<HitboxSlot>();

        private int lastDynamicAttackSlotID = 1000;
        private Dictionary<DSAProj.Action, int> eventBoxToAttackSlotMapping = new Dictionary<DSAProj.Action, int>();

        public int GetDynamicAttackSlotForEvent(DSAProj.Action ev)
        {
            if (eventBoxToAttackSlotMapping.ContainsKey(ev))
            {
                return eventBoxToAttackSlotMapping[ev];
            }
            else
            {
                int newSlot = lastDynamicAttackSlotID + 1;
                if (newSlot > 100000)
                    newSlot = 1000;
                lastDynamicAttackSlotID = newSlot;
                eventBoxToAttackSlotMapping.Add(ev, newSlot);
                return newSlot;
            }
        }

        //private Dictionary<ParamData.AtkParam.Hit, ParamData.AtkParam.DummyPolySource> HitFilters
        //    = new Dictionary<ParamData.AtkParam.Hit, ParamData.AtkParam.DummyPolySource>();

        //private Dictionary<DummyPolyBladeSFX, ParamData.AtkParam.DummyPolySource> BladeSfxHitFilters
        //    = new Dictionary<DummyPolyBladeSFX, ParamData.AtkParam.DummyPolySource>();

        private void UpdateHitPrim(HitboxSlot slot, ParamData.AtkParam.Hit hit)
        {
            lock (_lock_everything_monkaS)
            {
                //var hit = slot.Hit;

                var dmyA = hit.GetDmyPoly1Locations(MODEL, hit.DummyPolySourceSpawnedOn, MODEL.IS_PLAYER_WEAPON || MODEL.ChrAsm != null);
                var dmyB = hit.GetDmyPoly2Locations(MODEL, hit.DummyPolySourceSpawnedOn, MODEL.IS_PLAYER_WEAPON || MODEL.ChrAsm != null);

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 && hit.AC6_IsFan)
                {
                    if (dmyA.Count > slot.HitPrims[hit].Count)
                    {
                        if (slot.HitPrims.Count > 0)
                            AddNewHitPrim(slot.SlotID, slot.HitboxSpawnType, slot.Attack, hit, visible: false, dontUpdate: true, elapsedTime: slot.HitPrims[hit][0].ElapsedTime);
                        else
                            AddNewHitPrim(slot.SlotID, slot.HitboxSpawnType, slot.Attack, hit, visible: false, dontUpdate: true, elapsedTime: 0);
                    }


                    for (int i = 0; i < Math.Min(slot.HitPrims[hit].Count, dmyA.Count); i++)
                    {
                        Matrix m = dmyA[i];
                        Quaternion q = Quaternion.CreateFromRotationMatrix(m);
                        Matrix rm = Matrix.CreateFromQuaternion(Quaternion.Normalize(q));
                        Matrix tm = Matrix.CreateTranslation(Vector3.Transform(Vector3.Zero, m));
                        slot.HitPrims[hit][i].Transform = new Transform(rm * tm);
                        if (slot.HitPrims[hit][i] is DbgPrimWireFan asWireFan)
                        {
                            asWireFan.DmyPolySource = hit.DummyPolySourceSpawnedOn;
                        }
                    }
                }
                else if (hit.IsCapsule)
                {
                    if (hit.AC6_IsExtend && zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                    {
                        ((DbgPrimWireCapsule)slot.HitPrims[hit][0]).UpdateCapsuleEndPoints_AC6Extend(dmyA[0], hit, this,
                        hit.GetDmyPoly1SpawnPlace(MODEL, hit.DummyPolySourceSpawnedOn));
                    }
                    else
                    {
                        ((DbgPrimWireCapsule)slot.HitPrims[hit][0]).UpdateCapsuleEndPoints(Vector3.Transform(Vector3.Zero, dmyA[0]), Vector3.Transform(Vector3.Zero, dmyB[0]), hit, this,
                        hit.GetDmyPoly1SpawnPlace(MODEL, hit.DummyPolySourceSpawnedOn),
                        hit.GetDmyPoly2SpawnPlace(MODEL, hit.DummyPolySourceSpawnedOn),
                        hit.Radius);
                    }

                }
                else
                {
                    if (dmyA.Count > slot.HitPrims[hit].Count)
                    {
                        if (slot.HitPrims.Count > 0)
                            AddNewHitPrim(slot.SlotID, slot.HitboxSpawnType, slot.Attack, hit, visible: false, dontUpdate: true, elapsedTime: slot.HitPrims[hit][0].ElapsedTime);
                        else
                            AddNewHitPrim(slot.SlotID, slot.HitboxSpawnType, slot.Attack, hit, visible: false, dontUpdate: true, elapsedTime: 0);
                    }


                    for (int i = 0; i < Math.Min(slot.HitPrims[hit].Count, dmyA.Count); i++)
                    {
                        Matrix m = dmyA[i];
                        Quaternion q = Quaternion.CreateFromRotationMatrix(m);
                        Matrix rm = Matrix.CreateFromQuaternion(Quaternion.Normalize(q));
                        Matrix tm = Matrix.CreateTranslation(Vector3.Transform(Vector3.Zero, m));
                        slot.HitPrims[hit][i].Transform = new Transform(Matrix.CreateScale(hit.Radius) * rm * tm);
                        if (slot.HitPrims[hit][i] is DbgPrimWireSphere asWireSphere)
                        {
                            asWireSphere.DmyPolySource = hit.DummyPolySourceSpawnedOn;
                        }
                    }

                }

                for (int i = 0; i < slot.HitPrims[hit].Count; i++)
                {
                    slot.HitPrims[hit][i].OverrideColor = hit.GetColor(slot.HitboxSpawnType);
                }


            }
         
        }

        public bool GetFurthestHitboxDummyPolyLocation(bool onlyZX, bool isNearest, Matrix modelTransform, Matrix compareTransform, AttackDirType dirType, 
            out Vector3 result, out Vector3 resultLockedToDir, out float resultLockedToDir_RawDist, bool includeHitboxRadius)
        {
            modelTransform = modelTransform.ExtractTranslationAndHeading();
            compareTransform = compareTransform.ExtractTranslationAndHeading();

            result = Vector3.Zero;
            resultLockedToDir = Vector3.Zero;
            resultLockedToDir_RawDist = 0;

            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.AC6)
            {
                return false; //TODO
            }

            bool isFirst = true;

            Vector3 furthestPoint = new Vector3(0, 0, float.MinValue);
            Vector3 furthestPointLockedToDir = new Vector3(0, 0, float.MinValue);
            float furthestPointLockedToDir_RawDist = float.MinValue;

            if (dirType != AttackDirType.Any)
            {
                var originOffset = Vector3.Transform(Vector3.Forward * MODEL.ChrHitCapsuleRadius, Matrix.CreateRotationY(NewDummyPolyManager.AttackDirTypeToAngle(dirType)));
                compareTransform = Matrix.CreateTranslation(originOffset) * compareTransform;

                
            }

            Vector3 compareOrigin = Vector3.Transform(Vector3.Zero, compareTransform);

            if (isNearest)
            {
                furthestPoint = new Vector3(0, 0, float.MaxValue);
                furthestPointLockedToDir = new Vector3(0, 0, float.MaxValue);
                furthestPointLockedToDir_RawDist = float.MaxValue;
            }

            bool success = false;

            lock (_lock_everything_monkaS)
            {
                

                void ComparePoint(Vector3 testPoint, float hitRadius)
                {
                    testPoint = Vector3.Transform(testPoint, modelTransform);

                    if (includeHitboxRadius)
                    {
                        var awayFromOriginVector = (testPoint - compareOrigin);
                        if (awayFromOriginVector.LengthSquared() > 0)
                        {
                            var awayFromOriginDirection = Vector3.Normalize(awayFromOriginVector);

                            var forwardFromOrigin = Vector3.Transform(Vector3.Forward, compareTransform);
                            if (Vector3.Dot(forwardFromOrigin, awayFromOriginDirection) < 0)
                                awayFromOriginDirection *= -1;

                            if (isNearest)
                                testPoint -= awayFromOriginDirection * hitRadius;
                            else
                                testPoint += awayFromOriginDirection * hitRadius;
                        }
                        
                    }

                    if (isFirst)
                    {
                        //furthestPoint = testPoint;
                        isFirst = false;
                    }

                    success = true;
                    
                    if (dirType == AttackDirType.Any)
                    {
                        var a = testPoint - compareOrigin;
                        var b = Vector3.Transform(furthestPoint, modelTransform) - compareOrigin;
                        if (onlyZX)
                        {
                            a.Y = 0;
                            b.Y = 0;
                        }
                        bool compareResult = isNearest ? (a.LengthSquared() < b.LengthSquared()) : (a.LengthSquared() > b.LengthSquared());
                        if (compareResult)
                        {
                            furthestPoint = testPoint;
                            furthestPointLockedToDir = testPoint;
                        }
                    }
                    else
                    {
                        var angle = AttackDirTypeToAngle(dirType);
                        //var testPointLockedToDir = Vector3.Transform(testPoint - compareOrigin, Matrix.CreateRotationY(-angle));
                        var testPointLockedToDir = Vector3.Transform(Vector3.Transform(testPoint, Matrix.Invert(compareTransform)), Matrix.CreateRotationY(-angle));
                        testPointLockedToDir.X = 0;

                        float testPointLockedToDir_RawDist = -testPointLockedToDir.Z;

                        bool compareResult = isNearest ? (testPointLockedToDir_RawDist < furthestPointLockedToDir_RawDist) : (testPointLockedToDir_RawDist > furthestPointLockedToDir_RawDist);

                        testPointLockedToDir = Vector3.Transform(testPointLockedToDir, Matrix.CreateRotationY(angle));
                        testPointLockedToDir = Vector3.Transform(testPointLockedToDir, compareTransform);

                        //var a = testPointLockedToDir;
                        //var b = furthestPointLockedToDir - compareOrigin;
                        //if (onlyZX)
                        //{
                        //    a.Y = 0;
                        //    b.Y = 0;
                        //}
                        //bool compareResult = isNearest ? (a.LengthSquared() < b.LengthSquared()) : (a.LengthSquared() > b.LengthSquared());
                        if (compareResult)
                        {
                            furthestPoint = testPoint;
                            furthestPointLockedToDir = testPointLockedToDir;
                            furthestPointLockedToDir_RawDist = testPointLockedToDir_RawDist;
                        }
                    }

                   
                }

                foreach (var slot in HitboxSlots)
                {
                    var hits = slot.HitPrims.Keys.ToList();
                    foreach (var hit in hits)
                    {
                        var dmyA = hit.GetDmyPoly1Locations(MODEL, hit.DummyPolySourceSpawnedOn, MODEL.IS_PLAYER_WEAPON || MODEL.ChrAsm != null);
                        var dmyB = hit.GetDmyPoly2Locations(MODEL, hit.DummyPolySourceSpawnedOn, MODEL.IS_PLAYER_WEAPON || MODEL.ChrAsm != null);

                        if (hit.IsCapsule)
                        {
                            ComparePoint(Vector3.Transform(Vector3.Zero, dmyA[0]), hit.Radius);
                            ComparePoint(Vector3.Transform(Vector3.Zero, dmyB[0]), hit.Radius);
                        }
                        else
                        {
                            foreach (var d in dmyA)
                                ComparePoint(Vector3.Transform(Vector3.Zero, d), hit.Radius);
                        }
                    }
                }
            }

            result = furthestPoint;
            resultLockedToDir = furthestPointLockedToDir;
            resultLockedToDir_RawDist = furthestPointLockedToDir_RawDist;

            return success;
        }

        public void UpdateAllHitPrims()
        {
            lock (_lock_everything_monkaS)
            {
                if (DummyPolyByBoneID_AttachBone.ContainsKey(-1))
                {
                    foreach (var dmy in DummyPolyByBoneID_AttachBone[-1])
                    {
                        dmy.AttachMatrix = Matrix.Identity;
                    }
                }

                if (DummyPolyByBoneID_ReferenceBone.ContainsKey(-1))
                {
                    foreach (var dmy in DummyPolyByBoneID_ReferenceBone[-1])
                    {
                        //dmy.NullRefBoneMatrix();
                        OverrideDummyPolyFollowFlag_EnableOverride = true;
                        OverrideDummyPolyFollowFlag_Value = true;

                        dmy.NewUpdateRefBoneMatrix(MODEL.SkeletonFlver,
                            OverrideDummyPolyFollowFlag_EnableOverride ? OverrideDummyPolyFollowFlag_Value : null, 
                            OverrideUnmappedDummyPolyRefBoneID);
                    }
                }

                foreach (var slot in HitboxSlots)
                {
                    var hits = slot.HitPrims.Keys.ToList();
                    foreach (var hit in hits)
                        UpdateHitPrim(slot, hit);
                }
                    
                    
            }
        }

        public Matrix GetPrimDrawMatrix(ParamData.AtkParam.DummyPolySource dmyPolySource)
        {
            if (dmyPolySource == ParamData.AtkParam.DummyPolySource.BaseModel)
                return MODEL.CurrentTransform.WorldMatrix;

            if (MODEL.ChrAsm?.WeaponSlots != null)
            {
                foreach (var slot in MODEL.ChrAsm?.WeaponSlots)
                {
                    if (slot.DmySource0 == dmyPolySource)
                    {
                        var transform = slot.GetModelTransform(0);
                        if (transform != null)
                            return transform.Value;
                    }

                    if (slot.DmySource1 == dmyPolySource)
                    {
                        var transform = slot.GetModelTransform(1);
                        if (transform != null)
                            return transform.Value;
                    }

                    if (slot.DmySource2 == dmyPolySource)
                    {
                        var transform = slot.GetModelTransform(2);
                        if (transform != null)
                            return transform.Value;
                    }

                    if (slot.DmySource3 == dmyPolySource)
                    {
                        var transform = slot.GetModelTransform(3);
                        if (transform != null)
                            return transform.Value;
                    }
                }
            }

            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 && dmyPolySource == ParamData.AtkParam.DummyPolySource.HeadPart)
            {
                Matrix result = MODEL.CurrentTransform.WorldMatrix;
                MODEL.ChrAsm?.AccessArmorSlot(NewChrAsm.EquipSlotTypes.Head, slot =>
                {
                    slot.AccessAllModels(model =>
                    {
                        result = model.CurrentTransform.WorldMatrix;
                    });
                });
                return result;
            }
            else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 && dmyPolySource == ParamData.AtkParam.DummyPolySource.BodyPart)
            {
                Matrix result = MODEL.CurrentTransform.WorldMatrix;
                MODEL.ChrAsm?.AccessArmorSlot(NewChrAsm.EquipSlotTypes.Body, slot =>
                {
                    slot.AccessAllModels(model =>
                    {
                        result = model.CurrentTransform.WorldMatrix;
                    });
                });
                return result;
            }
            else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 && dmyPolySource == ParamData.AtkParam.DummyPolySource.ArmsPart)
            {
                Matrix result = MODEL.CurrentTransform.WorldMatrix;
                MODEL.ChrAsm?.AccessArmorSlot(NewChrAsm.EquipSlotTypes.Arms, slot =>
                {
                    slot.AccessAllModels(model =>
                    {
                        result = model.CurrentTransform.WorldMatrix;
                    });
                });
                return result;
            }
            else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 && dmyPolySource == ParamData.AtkParam.DummyPolySource.LegsPart)
            {
                Matrix result = MODEL.CurrentTransform.WorldMatrix;
                MODEL.ChrAsm?.AccessArmorSlot(NewChrAsm.EquipSlotTypes.Legs, slot =>
                {
                    slot.AccessAllModels(model =>
                    {
                        result = model.CurrentTransform.WorldMatrix;
                    });
                });
                return result;
            }
            else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.AC6 &&
                     dmyPolySource is >= ParamData.AtkParam.DummyPolySource.AC6Parts0
                         and <= ParamData.AtkParam.DummyPolySource.AC6Parts31)
            {
                int partIndex = dmyPolySource - ParamData.AtkParam.DummyPolySource.AC6Parts0;
                Matrix result = MODEL.CurrentTransform.WorldMatrix;
                MODEL.AC6NpcParts?.AccessModelOfPart(partIndex, (part, model) =>
                {
                    result = model.CurrentTransform.WorldMatrix;
                });
                return result;
            }
            else
                throw new NotImplementedException();
        }

        public void DrawAllHitPrims(bool isForce)
        {
            if (!Main.HelperDraw.EnableAttackHitShapes)
                return;

            

            if (GlobalForceDummyPolyIDVisible < -1)
                return;

            lock (_lock_everything_monkaS)
            {
                if (GlobalForceDummyPolyIDVisible == -1)
                {
                    foreach (var slot in HitboxSlots)
                    {
                        foreach (var kvp in slot.HitPrims)
                        {
                            foreach (var prim in kvp.Value)
                                prim.Draw(true, null, GetPrimDrawMatrix(prim.DmyPolySource));
                            //foreach (var prim in kvp.Value)
                            //    prim.Draw(true, null, Matrix.Identity);
                        }
                    }
                }
                
                foreach (var dmy in DummyPoly)
                {
                    float opacity = 1;
                    if (_dummyPolyByRefID.TryGetValue(dmy.ReferenceID, out var value))
                    {
                        if (value.Count >= Main.HelperDraw.DummyPolyClusterThreshold)
                        {
                            opacity *= Main.HelperDraw.DummyPolyClusterOpacity;
                        }
                    }

                    if (dmy.FollowBoneIndex < 0)
                    {
                        opacity *= Main.HelperDraw.StationaryDummyPolyOpacity;
                    }

                    bool isVis = (GlobalForceDummyPolyIDVisible == -1 && DummyPolyVisibleByRefID.ContainsKey(dmy.ReferenceID)
                        && (DummyPolyVisibleByRefID[dmy.ReferenceID] ||
                        dmy.ShowAttackInfos.Count > 0 || dmy.BulletSpawnIDs.Count > 0 || dmy.MiscSpawnTexts.Count > 0 || dmy.SFXSpawnIDs.Count > 0));
                    bool isForceByHover = GlobalForceDummyPolyIDVisible == (GlobalDummyPolyIDOffset + dmy.ReferenceID);

                    if (isVis || isForceByHover || isForce)
                    {
                        var mat = dmy.GetFinalMatrix(GetPrimDrawMatrix(dmy.ArrowPrimitive.DmyPolySource), isForceByHover || isForce, isForceByHover, opacity);
                        dmy.DrawPrim(mat, isForceByHover || isForce, isForceByHover, opacity);
                        //if (MODEL.IS_CHRASM_CHILD_MODEL || MODEL.IS_PLAYER_WEAPON)
                        //{
                        //    MODEL.PARENT_PLAYER_MODEL.DummyPolyDrawList.Add(dmy);
                        //}
                        //else
                        //{
                        //    MODEL.DummyPolyDrawList.Add(dmy);
                        //}
                        
                    }
                    //else
                    //{
                    //    dmy.Draw_IsDraw = false;
                    //}
                }

                
            }


                MODEL.ForAllAC6NpcParts((partIndex, part, model) =>
            {
                model.DummyPolyMan.DrawAllHitPrims(isForce);
            });

        }

        public void DrawAllHitPrimTexts(bool isForce)
        {
            if (!Main.HelperDraw.EnableDummyPolyTexts)
                return;
            //if (!ShowAttackNames)
            //    return;

            if (GlobalForceDummyPolyIDVisible < -1)
                return;

            lock (_lock_everything_monkaS)
            {
                var dummyPolyDrawInOrder = DummyPoly;

                foreach (var dmy in dummyPolyDrawInOrder)
                {
                    float opacity = 1;
                    if (_dummyPolyByRefID.TryGetValue(dmy.ReferenceID, out var value))
                    {
                        if (value.Count >= Main.HelperDraw.DummyPolyClusterThreshold)
                        {
                            opacity *= Main.HelperDraw.DummyPolyClusterOpacity;
                        }
                    }

                    if (dmy.FollowBoneIndex < 0)
                    {
                        opacity *= Main.HelperDraw.StationaryDummyPolyOpacity;
                    }
                    
                    bool isVis = (GlobalForceDummyPolyIDVisible < 0 && DummyPolyVisibleByRefID.ContainsKey(dmy.ReferenceID) 
                        && (DummyPolyVisibleByRefID[dmy.ReferenceID] ||
                        dmy.ShowAttackInfos.Count > 0 || dmy.BulletSpawnIDs.Count > 0 || dmy.MiscSpawnTexts.Count > 0 || dmy.SFXSpawnIDs.Count > 0));
                    bool isForceByHover = GlobalForceDummyPolyIDVisible == (GlobalDummyPolyIDOffset + dmy.ReferenceID);
                    if (isVis || isForce || isForceByHover)
                        dmy.DrawPrimText(MODEL.CurrentTransform.WorldMatrix, isForce || isForceByHover,

                            Main.HelperDraw.DummyPolyIDsAreGlobal ? GlobalDummyPolyIDOffset : 0, opacity,
                            includeAttackInfos: Main.HelperDraw.EnableAttackNames);
                }
            }

            MODEL.ForAllAC6NpcParts((partIndex, part, model) =>
            {
                model.DummyPolyMan.DrawAllHitPrimTexts(isForce);
            });

        }

        private HitboxSlot FindOrCreateHitSlot(int slotID, ParamData.AtkParam createAtk, HitboxSpawnTypes spawnType)
        {
            foreach (var slot in HitboxSlots)
            {
                if (slot.SlotID == slotID)
                {
                    // slot.Attack = createAtk;
                    // slot.HitboxSpawnType = spawnType;
                    return slot;
                }
            }

            var newSlot = new HitboxSlot();
            newSlot.Attack = createAtk;
            newSlot.SlotID = slotID;
            newSlot.HitboxSpawnType = spawnType;
            HitboxSlots.Add(newSlot);
            return newSlot;
        }

        private HitboxSlot AddNewHitPrim(int slotID, HitboxSpawnTypes spawnType, ParamData.AtkParam attack, ParamData.AtkParam.Hit hit, bool visible, bool dontUpdate, float elapsedTime)
        {
            var slot = FindOrCreateHitSlot(slotID, attack, spawnType);
            if (slot.Attack != attack)
            {
                ClearHitboxSlot(slotID);
                slot = FindOrCreateHitSlot(slotID, attack, spawnType);
            }
            slot.Attack = attack;
            slot.ElapsedTime = elapsedTime;
            slot.HitboxSpawnType = spawnType;

            if (!slot.HitPrims.ContainsKey(hit))
            {
                slot.HitPrims.Add(hit, new List<IDbgPrim>());

                //slot.HitPrims[hit].Clear();
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 && hit.AC6_IsFan)
                {
                    //throw new NotImplementedException();
                    slot.HitPrims[hit].Add(new DbgPrimWireFan(Color.White, hit)
                    {
                        EnableDraw = visible,
                        OverrideColor = hit.GetColor(slot.HitboxSpawnType),
                        DmyPolySource = hit.DummyPolySourceSpawnedOn,
                        ElapsedTime = elapsedTime,
                    });

                }
                else if (hit.IsCapsule)
                {
                    slot.HitPrims[hit].Add(new DbgPrimWireCapsule(hit.GetColor(slot.HitboxSpawnType))
                    {
                        EnableDraw = visible,
                        //OverrideColor = hit.GetColor(),
                        DmyPolySource = hit.DummyPolySourceSpawnedOn,
                        ElapsedTime = elapsedTime,
                    });
                }
                else
                {
                    slot.HitPrims[hit].Add(new DbgPrimWireSphere(new Transform(Matrix.CreateScale(hit.Radius)), Color.White)
                    {
                        EnableDraw = visible,
                        OverrideColor = hit.GetColor(slot.HitboxSpawnType),
                        DmyPolySource = hit.DummyPolySourceSpawnedOn,
                        ElapsedTime = elapsedTime,
                    });
                    //if (DummyPolyByRefID.ContainsKey(hit.DmyPoly1))
                    //{
                    //    foreach (var dmy in DummyPolyByRefID[hit.DmyPoly1])
                    //    {
                    //        HitPrims[hit].Add(new DbgPrimWireSphere(new Transform(Matrix.CreateScale(hit.Radius) * 
                    //            dmy.ReferenceMatrix), Color.White) 
                    //        { 
                    //            EnableDraw = visible,
                    //            Category = DbgPrimCategory.DummyPolyHelper,
                    //            OverrideColor = hit.GetColor(),
                    //        });

                    //    }
                    //}
                }
            }

            lock (_lock_everything_monkaS)
            {
                //if (visible && !VisibleHitsToHideForHideAll.Contains(hit))
                //    VisibleHitsToHideForHideAll.Enqueue(hit);

                if (!dontUpdate)
                    UpdateHitPrim(slot, hit);
            }

            return slot;
        }

        public void SetHitVisibility(int slotID, HitboxSpawnTypes spawnType, ParamData.AtkParam attack, ParamData.AtkParam.Hit hit, bool visible, float elapsedTime)
        {
            lock (_lock_everything_monkaS)
            {
                //if (!HitboxSlots.ContainsKey(slotID))
                //{
                //    AddNewHitPrim(slotID, hit, visible, false, elapsedTime);
                //}
                var slot = AddNewHitPrim(slotID, spawnType, attack, hit, visible, false, elapsedTime);

                UpdateHitPrim(slot, hit);

                foreach (var kvp in slot.HitPrims)
                {
                    foreach (var prim in kvp.Value)
                    {
                        prim.EnableDraw = visible;
                        if (elapsedTime >= 0)
                            prim.ElapsedTime = elapsedTime;
                    }
                }
                //else
                //{
                //    AddNewHitPrim(hit, visible);
                //}

                //if (visible && !VisibleHitsToHideForHideAll.Contains(hit))
                //    VisibleHitsToHideForHideAll.Enqueue(hit);
            }
        }

        public void ClearAllHitboxes()
        {
            lock (_lock_everything_monkaS)
            {
                HitboxSlots.Clear();
                //VisibleHitsToHideForHideAll.Clear();
            }

        }



        //public void HideAllHitboxes()
        //{
        //    ClearAllShowAttacks();

        //    while (VisibleHitsToHideForHideAll.Count > 0)
        //    {
        //        var nextHitToHide = VisibleHitsToHideForHideAll.Dequeue();
        //        SetHitVisibility(nextHitToHide, false, -1);
        //    }
        //}

        public void ClearHitboxSlot(int slotID)
        {
            var inactive = HitboxSlots.Where(s =>s.SlotID == slotID).ToList();
            var inactiveDynamicMaps = eventBoxToAttackSlotMapping.Where(kvp => kvp.Value == slotID).Select(kvp => kvp.Key).ToList();
            foreach (var slot in inactive)
            {
                foreach (var kvp in slot.HitPrims)
                    foreach (var prim in kvp.Value)
                        prim?.Dispose();
                HitboxSlots.Remove(slot);
            }
            foreach (var key in inactiveDynamicMaps)
            {
                eventBoxToAttackSlotMapping.Remove(key);
            }
        }

        public void ClearAllHitboxesExceptActive(List<int> activeSlots)
        {
            var inactive = HitboxSlots.Where(s => !activeSlots.Contains(s.SlotID)).ToList();
            var inactiveDynamicMaps = eventBoxToAttackSlotMapping.Where(kvp => !activeSlots.Contains(kvp.Value)).Select(kvp => kvp.Key).ToList();
            foreach (var slot in inactive)
            {
                foreach (var kvp in slot.HitPrims)
                    foreach (var prim in kvp.Value)
                        prim?.Dispose();
                HitboxSlots.Remove(slot);
            }
            foreach (var key in inactiveDynamicMaps)
            {
                eventBoxToAttackSlotMapping.Remove(key);
            }
        }

        public void SetAttackVisibility(int slotID, HitboxSpawnTypes spawnType, bool isNpcAtk, ParamData.AtkParam atk, bool visible, int dummyPolyOverride, 
            ParamData.AtkParam.DummyPolySource defaultDummyPolySource, float elapsedTime)
        {
            //if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 && atk.AC6HitboxType == ParamData.AtkParam.AC6HitboxTypes.Fan)
            //{

            //}
            //else
            //{

            //}

            if (dummyPolyOverride >= 0)
            {
                var fakeHitRadius = atk.Hits[0].Radius;
                var fakeHitType = atk.Hits[0].HitType;
                var fakeHit = new ParamData.AtkParam.Hit()
                {
                    DmyPoly1 = (short)dummyPolyOverride,
                    DmyPoly2 = -1,
                    Radius = fakeHitRadius,
                    HitType = fakeHitType,
                };
                ShowAttackOnDummyPoly(isNpcAtk, atk, spawnType, dummyPolyOverride);
                SetHitVisibility(slotID, spawnType, atk, fakeHit, visible, elapsedTime);
            }
            else
            {
                bool isFirstValidDmyPoly = true;

                //var dmyPolySrcToUse_ForAC6EnemyAttack = defaultDummyPolySource;

                //for (int i = 0; i < atk.Hits.Length; i++)
                //{
                //    int dmyPoly1 = atk.Hits[i].DmyPoly1;

                //    if (MODEL.Document.GameRoot.GameType is SoulsGames.AC6 && !MODEL.IS_PLAYER && MODEL.AC6NpcParts != null)
                //    {
                //        for (int j = 0; j < 32; j++)
                //        {
                //            bool isBreak = false;
                //            MODEL.AC6NpcParts.AccessModelOfPart(j, (p, m) =>
                //            {
                //                if (m.DummyPolyMan?.DummyPolyByRefID?.ContainsKey(dmyPoly1) == true)
                //                {
                //                    dmyPolySrcToUse_ForAC6EnemyAttack = (ParamData.AtkParam.DummyPolySource)(ParamData.AtkParam.DummyPolySource.AC6Parts0 + j);
                //                    isBreak = true;
                //                }
                //            });

                //            if (isBreak)
                //                break;
                //        }
                //        //dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.BaseModel;
                //    }
                //}


                for (int i = 0; i < atk.Hits.Length; i++)
                {
                    int dmyPoly1 = atk.Hits[i].DmyPoly1;

                    if (dmyPoly1 <= 0)
                    {
                        continue;
                    }

                    var dmyPolySrcToUse = defaultDummyPolySource;

                    if (atk.HitSourceType == ParamData.AtkParam.HitSourceTypes.Body)
                    {
                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.BaseModel;

                        
                    }
                    else if (atk.HitSourceType >= ParamData.AtkParam.HitSourceTypes.Parts0 &&
                             atk.HitSourceType <= ParamData.AtkParam.HitSourceTypes.Parts31)
                    {
                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.AC6Parts0 +
                                          (atk.HitSourceType - ParamData.AtkParam.HitSourceTypes.Parts0);
                    }
                    //else if (MODEL.Document.GameRoot.GameType is SoulsGames.AC6 && !MODEL.IS_PLAYER && MODEL.AC6NpcParts != null)
                    //{
                    //    //for (int j = 0; j < 32; j++)
                    //    //{
                    //    //    bool isBreak = false;
                    //    //    MODEL.AC6NpcParts.AccessModelOfPart(j, (p, m) =>
                    //    //    {
                    //    //        if (m.DummyPolyMan?.DummyPolyByRefID?.ContainsKey(dmyPoly1) == true)
                    //    //        {
                    //    //            dmyPolySrcToUse = (ParamData.AtkParam.DummyPolySource)(ParamData.AtkParam.DummyPolySource.AC6Parts0 + j);
                    //    //            isBreak = true;
                    //    //        }
                    //    //    });

                    //    //    if (isBreak)
                    //    //        break;
                    //    //}
                    //    //dmyPolySrcToUse = dmyPolySrcToUse_ForAC6EnemyAttack;
                    //    dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.BaseModel;
                    //}



                    if (MODEL.ChrAsm != null)
                    {
                        var dummyPolySrcOfAtk = atk.Hits[i].GetDmyPoly1SpawnPlace(MODEL, dmyPolySrcToUse);
                        atk.Hits[i].DummyPolySourceSpawnedOn = MODEL.ChrAsm.GetDummySourceFromManager(dummyPolySrcOfAtk);
                        if (!dummyPolySrcOfAtk.NewCheckDummyPolyExists(dmyPoly1) && NewCheckDummyPolyExists(dmyPoly1))
                        {
                            atk.Hits[i].DmyPoly1FallbackToBody = true;
                            if (visible && isFirstValidDmyPoly && dmyPoly1 != -1)
                            {
                                ShowAttackOnDummyPoly(isNpcAtk, atk, spawnType, dmyPoly1 % 1000);
                                isFirstValidDmyPoly = false;
                            }
                        }
                        else
                        {
                            if (visible && isFirstValidDmyPoly && dmyPoly1 != -1)
                            {
                                dummyPolySrcOfAtk?.ShowAttackOnDummyPoly(isNpcAtk, atk, spawnType, dmyPoly1 % 1000);
                                isFirstValidDmyPoly = false;
                            }
                        }

                        if (!dummyPolySrcOfAtk.NewCheckDummyPolyExists(atk.Hits[i].DmyPoly2) && NewCheckDummyPolyExists(atk.Hits[i].DmyPoly2))
                        {
                            atk.Hits[i].DmyPoly2FallbackToBody = true;
                        }
                    }
                    else if (MODEL.AC6NpcParts != null && dmyPolySrcToUse is >= ParamData.AtkParam.DummyPolySource.AC6Parts0 and <= ParamData.AtkParam.DummyPolySource.AC6Parts31)
                    {
                        atk.Hits[i].DummyPolySourceSpawnedOn = dmyPolySrcToUse;
                        int partIndex = dmyPolySrcToUse - ParamData.AtkParam.DummyPolySource.AC6Parts0; 
                        MODEL.AC6NpcParts.AccessModelOfPart(partIndex, (part, model) =>
                        {
                            if (isFirstValidDmyPoly)
                            {
                                model.DummyPolyMan.ShowAttackOnDummyPoly(isNpcAtk, atk, spawnType, dmyPoly1);
                                isFirstValidDmyPoly = false;
                            }
                        });
                    }
                    else
                    {
                        if (isFirstValidDmyPoly)
                        {
                            ShowAttackOnDummyPoly(isNpcAtk, atk, spawnType, dmyPoly1);
                            isFirstValidDmyPoly = false;
                        }
                        
                        
                    }

                    var actualHit = atk.Hits[i];

                    //actualHit.DummyPolySourceSpawnedOn = dmyPolySrcToUse;

                    SetHitVisibility(slotID, spawnType, atk, actualHit, visible, elapsedTime);



                    isFirstValidDmyPoly = false;
                }
            }

            
        }


        // -1 = Show all normally
        // -2 or lower = show none
        public int GlobalForceDummyPolyIDVisible = -1;
        //public static bool ShowGlobalIDOffset = true;

        public int GlobalDummyPolyIDOffset = 0;

        public string GlobalDummyPolyIDPrefix = "Body - ";

        public void SetDummyPolyVisibility(int id, bool visible)
        {
            if (!_dummyPolyVisibleByRefID.ContainsKey(id))
                _dummyPolyVisibleByRefID.Add(id, visible);
            else
                _dummyPolyVisibleByRefID[id] = visible;
        }
        public IReadOnlyDictionary<int, bool> DummyPolyVisibleByRefID => _dummyPolyVisibleByRefID;
        public IReadOnlyDictionary<int, List<NewDummyPolyInfo>> DummyPolyByRefID => _dummyPolyByRefID;
        public IReadOnlyDictionary<int, List<NewDummyPolyInfo>> DummyPolyByBoneID_AttachBone => _dummyPolyByBoneID;
        public IReadOnlyDictionary<int, List<NewDummyPolyInfo>> DummyPolyByBoneID_ReferenceBone => _dummyPolyByBoneID_Ref;

        public List<NewDummyPolyInfo> NewGetDummyPolyByRefID(int id)
        {
            List<NewDummyPolyInfo> ac6PartsResult = null;
            if (MODEL.AC6NpcParts != null)
            {
                //for (int i = 31; i >= 0; i--)
                //{
                //    MODEL.AC6NpcParts.AccessModelOfPart(i, (p, m) =>
                //    {
                //        if (m.DummyPolyMan == null)
                //            return;

                //        bool dmyExistsInPart = m.DummyPolyMan.DummyPolyByRefID.ContainsKey(id);
                //        if (dmyExistsInPart)
                //        {
                //            ac6PartsResult = m.DummyPolyMan.DummyPolyByRefID[id];
                //        }
                //    });
                //    if (ac6PartsResult != null)
                //        break;
                //}
            }

            if (ac6PartsResult != null)
                return ac6PartsResult;

            if (DummyPolyByRefID.ContainsKey(id))
                return DummyPolyByRefID[id];
            else
                return null;
        }

        public bool NewCheckDummyPolyExists(int dummyPolyID)
        {
            bool baseExists = DummyPolyByRefID.ContainsKey(dummyPolyID);
            if (baseExists)
                return true;

            bool foundDmyExists = false;

            if (MODEL.AC6NpcParts != null)
            {
                //for (int i = 31; i >= 0; i--)
                //{
                //    MODEL.AC6NpcParts.AccessModelOfPart(i, (p, m) =>
                //    {
                //        bool dmyExistsInPart = m.DummyPolyMan?.NewCheckDummyPolyExists(dummyPolyID) ?? false;
                //        if (dmyExistsInPart)
                //        {
                //            foundDmyExists = true;
                //        }
                //    });
                //    if (foundDmyExists)
                //        break;
                //}
            }

            return foundDmyExists;
        }

        public List<Matrix> GetDummyMatricesByID(int id, bool getAbsoluteWorldPos = false, bool ignoreAC6NpcParts = false)
        {
            List<Matrix> ac6PartsResult = null;
            if (!ignoreAC6NpcParts && MODEL.AC6NpcParts != null)
            {
                //for (int i = 31; i >= 0; i--)
                //{
                //    MODEL.AC6NpcParts.AccessModelOfPart(i, (p, m) =>
                //    {
                //        if (m.DummyPolyMan == null)
                //            return;

                //        if (m.DummyPolyMan.NewCheckDummyPolyExists(id))
                //        {
                //            // Always get this as absolute world pos so we don't have to deal with pos relative to the individual parts model
                //            ac6PartsResult = m.DummyPolyMan.GetDummyMatricesByID(id, getAbsoluteWorldPos: true);

                //            if (ac6PartsResult != null)
                //            {
                //                // If absolute world pos is not requested, then make it relative to the character model after the fact
                //                if (!getAbsoluteWorldPos)
                //                {
                //                    for (int i = 0; i < ac6PartsResult.Count; i++)
                //                    {
                //                        ac6PartsResult[i] *= Matrix.Invert(MODEL.CurrentTransform.WorldMatrix);
                //                    }
                //                }
                //            }
                //        }
                //    });
                //    if (ac6PartsResult != null)
                //        break;
                //}
            }

            if (ac6PartsResult != null)
                return ac6PartsResult;

            var result = new List<Matrix>();

            var world = (getAbsoluteWorldPos ? MODEL.CurrentTransform.WorldMatrix : Matrix.Identity);

            if (!_dummyPolyByRefID.ContainsKey(id))
            {
                result.Add(world);
            }
            else
            {
                foreach (var d in DummyPolyByRefID[id])
                    result.Add(d.CurrentMatrix * world);
            }

            return result;
        }

        public List<Vector3> GetDummyPosByID(int id, bool getAbsoluteWorldPos, bool ignoreAC6NpcParts = false)
        {
            return GetDummyMatricesByID(id, getAbsoluteWorldPos, ignoreAC6NpcParts).Select(x => Vector3.Transform(Vector3.Zero, x)).ToList();
        }

        public void UpdateFlverBone(int index, Matrix relativeMatrix, Matrix fkMatrix)
        {
            lock (_lock_everything_monkaS)
            {
                if (MODEL.ApplyBindPose)
                    return;

                if (DummyPolyByBoneID_AttachBone.ContainsKey(index))
                {
                    foreach (var d in DummyPolyByBoneID_AttachBone[index])
                    {
                        if (d.DummyFollowFlag)
                            d.AttachMatrix = relativeMatrix;
                        else
                            d.AttachMatrix = Matrix.Identity;
                    }
                }

                if (DummyPolyByBoneID_ReferenceBone.ContainsKey(index))
                {
                    foreach (var d in DummyPolyByBoneID_ReferenceBone[index])
                    {
                        //if (d.DummyFollowFlag)
                        //    d.UpdateRefBoneMatrix(fkMatrix);
                        //else
                        //    d.NullRefBoneMatrix();

                        OverrideDummyPolyFollowFlag_EnableOverride = true;
                        OverrideDummyPolyFollowFlag_Value = true;

                        d.NewUpdateRefBoneMatrix(MODEL.SkeletonFlver,
                            OverrideDummyPolyFollowFlag_EnableOverride ? OverrideDummyPolyFollowFlag_Value : null, 
                            OverrideUnmappedDummyPolyRefBoneID);
                    }
                }


            }
        }

        public void AddDummyPoly(FLVER.Dummy dmy)
        {
            lock (_lock_everything_monkaS)
            {
                var di = new NewDummyPolyInfo(dmy, MODEL.SkeletonFlver);
                DummyPoly.Add(di);

                if (!_dummyPolyByRefID.ContainsKey(dmy.ReferenceID))
                    _dummyPolyByRefID.Add(dmy.ReferenceID, new List<NewDummyPolyInfo>());

                if (!_dummyPolyVisibleByRefID.ContainsKey(dmy.ReferenceID))
                    _dummyPolyVisibleByRefID.Add(dmy.ReferenceID, true);

                if (!_dummyPolyByRefID[dmy.ReferenceID].Contains(di))
                    _dummyPolyByRefID[dmy.ReferenceID].Add(di);

                if (!_dummyPolyByBoneID.ContainsKey(dmy.AttachBoneIndex))
                    _dummyPolyByBoneID.Add(dmy.AttachBoneIndex, new List<NewDummyPolyInfo>());

                if (!_dummyPolyByBoneID[dmy.AttachBoneIndex].Contains(di))
                    _dummyPolyByBoneID[dmy.AttachBoneIndex].Add(di);

                if (!_dummyPolyByBoneID_Ref.ContainsKey(dmy.ParentBoneIndex))
                    _dummyPolyByBoneID_Ref.Add(dmy.ParentBoneIndex, new List<NewDummyPolyInfo>());

                if (!_dummyPolyByBoneID_Ref[dmy.ParentBoneIndex].Contains(di))
                    _dummyPolyByBoneID_Ref[dmy.ParentBoneIndex].Add(di);
            }    
        }

        public void AddAllDummiesFromFlver(FLVER2 flver)
        {
            foreach (var d in flver.Dummies)
                AddDummyPoly(d);

            _dummyPolyByRefID = _dummyPolyByRefID.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public void AddAllDummiesFromFlver(FLVER0 flver)
        {
            foreach (var d in flver.Dummies)
                AddDummyPoly(d);

            _dummyPolyByRefID = _dummyPolyByRefID.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public NewDummyPolyManager(Model mdl)
        {
            MODEL = mdl;
        }
    }
}
