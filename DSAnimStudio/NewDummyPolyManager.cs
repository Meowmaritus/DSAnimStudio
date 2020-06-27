using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewDummyPolyManager
    {
        public readonly Model MODEL;

        public class DummyPolyBladeKeyframeMod
        {
            // Lerp from this frame to current frame.
            public float DistanceLerp = 1;
            public float Constrict = 1;
            public float Opacity = 1;
            public DummyPolyBladeKeyframeMod(float distLerp, float constrict, float opacity)
            {
                DistanceLerp = distLerp;
                Constrict = constrict;
                Opacity = opacity;
            }
        }

        public struct DummyPolyBladePos
        {
            public Vector3 Start;
            public Vector3 End;
            public float Opacity;
            public Vector3 GetCenter() => (Start + End) / 2;
        }

        public class DummyPolyBladeSFX
        {
            public int FFXID;

            public int StartDummyPolyID;
            public int EndDummyPolyID;

            public float Opacity = 1;

            private Func<bool> CheckIfAliveFunc = null;

            public bool IsAlive { get; private set; }

            public DummyPolyBladeSFX(int ffxid, int startDummyPolyID, int endDummyPolyID,
                ParamData.AtkParam.DummyPolySource dmySource, Func<bool> checkIfAliveFunc)
            {
                FFXID = ffxid;
                StartDummyPolyID = startDummyPolyID;
                EndDummyPolyID = endDummyPolyID;
                DmyPolySource = dmySource;
                CheckIfAliveFunc = checkIfAliveFunc;
            }

            ParamData.AtkParam.DummyPolySource DmyPolySource = ParamData.AtkParam.DummyPolySource.Body;

            private static List<DummyPolyBladeKeyframeMod> KeyframeMods = new List<DummyPolyBladeKeyframeMod>
            {
                new DummyPolyBladeKeyframeMod(distLerp: 0.25f, constrict: 0.600f, opacity: 0.5f + (0.0f / 2.0f)),
                new DummyPolyBladeKeyframeMod(distLerp: 0.5f, constrict: 0.725f, opacity: 0.5f + (0.2f / 2.0f)),
                new DummyPolyBladeKeyframeMod(distLerp: 0.625f, constrict: 0.825f, opacity: 0.5f + (0.4f / 2.0f)),
                new DummyPolyBladeKeyframeMod(distLerp: 0.8f, constrict: 0.875f, opacity: 0.5f + (0.6f / 2.0f)),
                new DummyPolyBladeKeyframeMod(distLerp: 0.9f, constrict: 0.900f, opacity: 0.5f + (0.8f / 2.0f)),
            };

            public DummyPolyBladePos CurrentPos;
            public DummyPolyBladePos CurrentPosLive;
            //public Matrix MatrixToGetToLivePos = Matrix.Identity;

            public Queue<DummyPolyBladePos> Keyframes = new Queue<DummyPolyBladePos>();
            private void PushCurrentFrame()
            {
                if (Keyframes.Count >= KeyframeMods.Count)
                {
                    Keyframes.Dequeue();
                }

                Keyframes.Enqueue(CurrentPos);
            }

            public static (Vector3 Start, Vector3 End) Constrict(Vector3 start, Vector3 end, float ratio)
            {
                Vector3 center = (start + end) / 2;

                return (Vector3.Lerp(center, start, ratio), Vector3.Lerp(center, end, ratio));
            }

            public static (Vector3 Start, Vector3 End) UndoConstrict(Vector3 start, Vector3 end, float ratio)
            {
                Vector3 center = (start + end) / 2;

                return (XnaExtensions.GetLerpValARequiredForResult(center, start, 1 - ratio), 
                    XnaExtensions.GetLerpValARequiredForResult(center, end, 1 - ratio));
            }

            public List<DummyPolyBladePos> GetPosFrames(/*Matrix rotLowHzToLive, Vector3 posLowHzToLive, bool addTheShit*/)
            {
                var result = new List<DummyPolyBladePos>();
                var asList = Keyframes.ToList();

                for (int i = 0; i < asList.Count; i++)
                {
                    var mod = KeyframeMods[i];





                    //var constricted = Constrict(Vector3.Lerp(asList[i].Start, CurrentPos.Start, mod.DistanceLerp),
                    //    Vector3.Lerp(asList[i].End, CurrentPos.End, mod.DistanceLerp),
                    //    mod.Constrict);



                    //Vector3 finalStart = constricted.Start;
                    //Vector3 finalEnd = constricted.End;




                    var constricted = Constrict(asList[i].Start, asList[i].End, mod.Constrict);



                    Vector3 finalStart = Vector3.Lerp(constricted.Start, CurrentPos.Start, mod.DistanceLerp);
                    Vector3 finalEnd = Vector3.Lerp(constricted.End, CurrentPos.End, mod.DistanceLerp);
                    finalStart = Vector3.Lerp(finalStart, CurrentPosLive.Start, 0.5f);
                    finalEnd = Vector3.Lerp(finalEnd, CurrentPosLive.End, 0.5f);


                    //if (i == (asList.Count - 1))
                    //{
                    //    var dir = Vector3.Transform(constricted.Start - constricted.End, rotLowHzToLive);
                    //    finalEnd = constricted.End + posLowHzToLive;
                    //    finalStart = finalEnd + (dir);

                    //    //finalStart = Vector3.Lerp(finalStart, finalStart_Blend, (1.0f * i / asList.Count - 1));
                    //    //finalEnd = Vector3.Lerp(finalEnd, finalEnd_Blend, (1.0f * i / asList.Count - 1));


                    //    //var dir_Start = Vector3.Transform(constricted.Start - CurrentPos.Start, rotLowHzToLive);
                    //    //var dir_End = Vector3.Transform(constricted.End - CurrentPos.End, rotLowHzToLive);

                    //    //finalStart = CurrentPos.Start + dir_Start;// constricted.Start;
                    //    //finalEnd = CurrentPos.End + dir_End;// constricted.End;




                    //    //var dir_Start = Vector3.Transform(constricted.Start - CurrentPos.End, rotLowHzToLive);
                    //    //var dir_End = Vector3.Transform(constricted.End - CurrentPos.End, rotLowHzToLive);

                    //    //var finalStart = CurrentPos.End + dir_Start;// constricted.Start;
                    //    //var finalEnd = CurrentPos.End + dir_End;// constricted.End;
                    //}






                    //var finalStart = constricted.Start;
                    //var finalEnd = constricted.End;

                    result.Add(new DummyPolyBladePos()
                    {
                        Start = finalStart,
                        End = finalEnd,
                        Opacity = Opacity * KeyframeMods[i].Opacity,
                    });
                }
                return result;
            }

            public DummyPolyBladePos GetCurrentBladePos(NewDummyPolyManager manager)
            {
                var dmyPoly1 = StartDummyPolyID;
                var dmyPoly2 = EndDummyPolyID;

                DummyPolyBladePos GetPosFatcat(NewDummyPolyManager dpm, Matrix modelMatrix)
                {
                    var pos1 = Vector3.Transform(Vector3.Zero, dpm.DummyPolyByRefID[dmyPoly1][0].CurrentMatrix * modelMatrix);
                    var pos2 = Vector3.Transform(Vector3.Zero, dpm.DummyPolyByRefID[dmyPoly2][0].CurrentMatrix * modelMatrix);

                    // Get REAL pos1
                    var bladeLength = (pos1 - pos2).Length();
                    var pos2ExtendedOutward = Vector3.Transform(Vector3.Forward, dpm.DummyPolyByRefID[dmyPoly2][0].CurrentMatrix * modelMatrix);
                    var actualDirection = Vector3.Normalize(pos2ExtendedOutward - pos2);
                    pos1 = pos2 + (actualDirection * bladeLength);

                    return new DummyPolyBladePos()
                    {
                        Start = pos1,
                        End = pos2,
                        Opacity = Opacity,
                    };
                }

               

                if (DmyPolySource == ParamData.AtkParam.DummyPolySource.Body)
                {
                    return GetPosFatcat(manager, manager.MODEL.CurrentTransform.WorldMatrix);
                }
                else if (DmyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon &&
                    manager.MODEL.ChrAsm != null &&
                    manager.MODEL.ChrAsm.RightWeaponModel != null)
                {
                    return GetPosFatcat(manager.MODEL.ChrAsm.RightWeaponModel.DummyPolyMan, manager.MODEL.ChrAsm.RightWeaponModel.CurrentTransform.WorldMatrix);
                }
                else if (DmyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon &&
                    manager.MODEL.ChrAsm != null &&
                    manager.MODEL.ChrAsm.LeftWeaponModel != null)
                {
                    return GetPosFatcat(manager.MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan, manager.MODEL.ChrAsm.LeftWeaponModel.CurrentTransform.WorldMatrix);
                }
                else
                {
                    return new DummyPolyBladePos();
                }
            }

            public void UpdateLive(NewDummyPolyManager manager)
            {
                IsAlive = CheckIfAliveFunc?.Invoke() ?? true;

                var curBladePos = GetCurrentBladePos(manager);

                curBladePos.Opacity = Opacity;

                CurrentPosLive = curBladePos;
            }

            private void BakeKeyframesToLiveMod(List<DummyPolyBladePos> positions)
            {
                var posLowHzToLive = (CurrentPosLive.End - CurrentPos.End);
                var rotLowHzToLive = Matrix.CreateFromQuaternion(XnaExtensions.GetQuatOfBladePosDelta(CurrentPos, CurrentPosLive));

                var asList = Keyframes.ToList();

                for (int i = 0; i < asList.Count - 1; i++)
                {


                    DummyPolyBladePos bakedPos = new DummyPolyBladePos();

                    if (positions.Count < asList.Count)
                    {
                        bakedPos = positions[i];
                    }
                    else
                    {
                        if (i < positions.Count - 1)
                            bakedPos = positions[i + 1];
                        else
                            bakedPos = CurrentPos;
                    }

                    var mod = KeyframeMods[i + 1];
                    var finalEnd = bakedPos.End;
                    var finalStart = bakedPos.Start;


                    //var dir = Vector3.Transform(asList[i].Start - asList[i].End, rotLowHzToLive);
                    //var finalEnd = asList[i].End + posLowHzToLive;
                    //var finalStart = finalEnd + (dir);

                    //UN - constrict
                    (finalStart, finalEnd) = UndoConstrict(finalStart, finalEnd, mod.Constrict);

                    //UN - Lerp
                    finalStart = XnaExtensions.GetLerpValARequiredForResult(CurrentPos.Start, finalStart, mod.DistanceLerp);
                    finalEnd = XnaExtensions.GetLerpValARequiredForResult(CurrentPos.End, finalEnd, mod.DistanceLerp);

                    

           



                    asList[i] = new DummyPolyBladePos()
                    {
                        Start = finalStart,
                        End = finalEnd,
                        Opacity = Opacity * asList[i].Opacity,
                    };
                }
                Keyframes = new Queue<DummyPolyBladePos>(asList);
            }

            public void UpdateLowHz(NewDummyPolyManager manager)
            {
                var curBladePos = GetCurrentBladePos(manager);

                curBladePos.Opacity = Opacity;

                

                if (Keyframes.Count > 0)
                {
                    

                    //var posLowHzToLive = (CurrentPosLive.End - CurrentPos.End);
                    //var rotLowHzToLive = Matrix.CreateFromQuaternion(XnaExtensions.GetQuatOfBladePosDelta(CurrentPos, CurrentPosLive));
                    //var sampledFrames = GetPosFrames(rotLowHzToLive, posLowHzToLive, addTheShit: false);

                    

                    PushCurrentFrame();

                    //BakeKeyframesToLiveMod(sampledFrames);

                    CurrentPos = curBladePos;

                   

                    

                    

                    

                    
                }
                else
                {
                    CurrentPos = curBladePos;

                    PushCurrentFrame();
                }
            }

            DbgPrimWireCapsule debugDrawCapsule = new DbgPrimWireCapsule(Color.Yellow);
            public void DoDebugDraw(Matrix m)
            {
                //var posLowHzToLive = (CurrentPosLive.End - CurrentPos.End);
                //var rotLowHzToLive = Matrix.CreateFromQuaternion(XnaExtensions.GetQuatOfBladePosDelta(CurrentPos, CurrentPosLive));
                //var dir = Vector3.Transform(CurrentPos.Start - CurrentPos.End, rotLowHzToLive);
                //var finalEnd = CurrentPos.End + posLowHzToLive;
                //var finalStart = finalEnd + (dir);
                var frames = GetPosFrames(/*rotLowHzToLive, posLowHzToLive, addTheShit: true*/);

                //var adjustedCurrentPos = new DummyPolyBladePos()
                //{
                //    Start = finalStart,
                //    End = finalEnd,
                //    Opacity = CurrentPos.Opacity,
                //};

                var adjustedCurrentPos = CurrentPosLive;

                if (frames.Count > 0)
                {
                    using (var prim = new DbgPrimSolidBladeTrail($"Blade SFX {FFXID}", Color.White * 0.66666f, adjustedCurrentPos, frames))
                    {
                        prim.Category = DbgPrimCategory.AlwaysDraw;
                        prim.DisableLighting = true;
                        prim.Draw(null, Matrix.Identity);
                    }

                    debugDrawCapsule.UpdateCapsuleEndPoints(CurrentPosLive.Start, CurrentPosLive.End, 0.1f);
                    debugDrawCapsule.Draw(null, Matrix.Identity);
                }
               

                //debugDrawCapsule.OverrideColor = Color.Yellow * Opacity;
                //debugDrawCapsule.UpdateCapsuleEndPoints(CurrentPos.Start, CurrentPos.End, 0.1f);
                //debugDrawCapsule.Draw(null, m);
                
                //foreach (var f in frames)
                //{
                //    debugDrawCapsule.OverrideColor = Color.Yellow * f.Opacity;
                //    debugDrawCapsule.UpdateCapsuleEndPoints(f.Start, f.End, 0.1f);
                //    debugDrawCapsule.Draw(null, m);
                //}
            }
        }

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

            public DummyPolyInfo(FLVER.Dummy dmy, NewAnimSkeleton skeleton)
            {
                ReferenceID = dmy.ReferenceID;
                FollowBoneIndex = dmy.AttachBoneIndex;
                ReferenceMatrix = Matrix.CreateWorld(
                    Vector3.Zero,
                    Vector3.Normalize(new Vector3(dmy.Forward.X, dmy.Forward.Y, dmy.Forward.Z)),
                    dmy.UseUpwardVector ? Vector3.Normalize(new Vector3(dmy.Upward.X, dmy.Upward.Y, dmy.Upward.Z)) : Vector3.Up)
                    * Matrix.CreateTranslation(new Vector3(dmy.Position.X, dmy.Position.Y, dmy.Position.Z))
                    * (dmy.ParentBoneIndex >= 0 ? skeleton.FlverSkeleton[dmy.ParentBoneIndex].ReferenceMatrix : Matrix.Identity);
                CurrentMatrix = ReferenceMatrix;

                ArrowPrimitive = new DbgPrimWireArrow("DummyPoly Spawns", Transform.Default, Color.White)
                {
                    //Wireframe = true,
                    //BackfaceCulling = true,
                    //DisableLighting = true,
                    Category = DbgPrimCategory.DummyPolySpawnArrow,
                    OverrideColor = Color.Cyan,
                };

                SpawnPrinter.Font = DBG.DEBUG_FONT_SMALL;
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

        private Dictionary<DummyPolyBladeSFX, ParamData.AtkParam.DummyPolySource> BladeSfxHitFilters
            = new Dictionary<DummyPolyBladeSFX, ParamData.AtkParam.DummyPolySource>();

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
                            Quaternion q = Quaternion.CreateFromRotationMatrix(m);
                            Matrix rm = Matrix.CreateFromQuaternion(q);
                            Matrix tm = Matrix.CreateTranslation(Vector3.Transform(Vector3.Zero, m));
                            HitPrims[hit][i].Transform = new Transform(Matrix.CreateScale(hit.Radius) * rm * tm);
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

        public void AddDummyPoly(FLVER.Dummy dmy)
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
            //var newDmy = new FLVER2.Dummy();

            //newDmy.ReferenceID = 205;

            //newDmy.AttachBoneIndex = 19;
            //float newX = (-0.09398f / 2.25f) + 0.01f;
            //float newY = (((1.659459f) + (1.54340553f)) / 2) + 0.007f;
            //float newZ = (((-0.2146896f) + (-0.09559878f)) / 2) * 0.68f;

            //newDmy.Position = new System.Numerics.Vector3(newX, newY, newZ);

            //newDmy.Upward = new System.Numerics.Vector3(0, 1, 0);
            //newDmy.Forward = new System.Numerics.Vector3(0, 0, -1);

            //newDmy.UseUpwardVector = true;

            //newDmy.DummyBoneIndex = 59;

            //flver.Dummies.Add(newDmy);



            //newDmy = new FLVER2.Dummy();

            //newDmy.ReferenceID = 205;

            //newDmy.AttachBoneIndex = 19;
            //newX = -((-0.09398f / 2.25f) + 0.01f);
            //newY = (((1.659459f) + (1.54340553f)) / 2) + 0.007f;
            //newZ = (((-0.2146896f) + (-0.09559878f)) / 2) * 0.68f;

            //newDmy.Position = new System.Numerics.Vector3(newX, newY, newZ);

            //newDmy.Upward = new System.Numerics.Vector3(0, 1, 0);
            //newDmy.Forward = new System.Numerics.Vector3(0, 0, -1);

            //newDmy.UseUpwardVector = true;

            //newDmy.DummyBoneIndex = 59;

            //flver.Dummies.Add(newDmy);

            //flver.Write(@"C:\Users\Meowmaritus\GitHub\DSAnimStudio\DSAnimStudio\bin\x64\Debug\OUTPUTFLVER.flver");

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
