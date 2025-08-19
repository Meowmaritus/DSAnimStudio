using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSAnimStudio.NewDummyPolyManager;
using SoulsFormats;

namespace DSAnimStudio
{
    public class NewDummyPolyInfo
    {
        public readonly string GUID = Guid.NewGuid().ToString();
        public FLVER.Dummy dummy;

        public bool DummyFollowFlag => dummy.Flag1;

        public int Draw_CalculatedOrder = -1;

        //public bool Draw_IsDraw = true;
        //public Matrix Draw_CalculatedMatrix = Matrix.Identity;
        //public bool Draw_IsForce = false;
        //public bool Draw_IsForceBigger = false;
        //public float Draw_Opacity = 1;

        public int ReferenceID = -1;
        public Matrix ReferenceMatrix = Matrix.Identity;

        public int FollowBoneIndex => dummy.AttachBoneIndex;

        public Matrix AttachMatrix = Matrix.Identity;
        public Matrix CurrentMatrix => ReferenceMatrix * AttachMatrix;

        public List<DummyPolyShowAttackInfo> ShowAttackInfos = new();

        public DbgPrimWireArrow ArrowPrimitive = null;

        public List<int> SFXSpawnIDs = new List<int>();
        public List<int> BulletSpawnIDs = new List<int>();
        public List<string> MiscSpawnTexts = new List<string>();

        public StatusPrinter SpawnPrinter = new StatusPrinter(shadowThickness: 2, enableShadowCardinals: true);

        public bool DisableTextDraw = false;

        public bool DebugSelected = false;

        //public void NullRefBoneMatrix()
        //{
        //    ReferenceMatrix = Matrix.CreateWorld(
        //        Vector3.Zero,
        //        Vector3.Normalize(new Vector3(dummy.Forward.X, dummy.Forward.Y, dummy.Forward.Z)),
        //        dummy.UseUpwardVector ? Vector3.Normalize(new Vector3(dummy.Upward.X, dummy.Upward.Y, dummy.Upward.Z)) : Vector3.Up)
        //        * Matrix.CreateTranslation(new Vector3(dummy.Position.X, dummy.Position.Y, dummy.Position.Z));
        //}

        //public void UpdateRefBoneMatrix(Matrix refBoneMatrix)
        //{
        //    ReferenceMatrix = Matrix.CreateWorld(
        //        Vector3.Zero,
        //        Vector3.Normalize(new Vector3(dummy.Forward.X, dummy.Forward.Y, dummy.Forward.Z)),
        //        dummy.UseUpwardVector ? Vector3.Normalize(new Vector3(dummy.Upward.X, dummy.Upward.Y, dummy.Upward.Z)) : Vector3.Up)
        //        * Matrix.CreateTranslation(new Vector3(dummy.Position.X, dummy.Position.Y, dummy.Position.Z))
        //        * refBoneMatrix;


        //}

        public void NewUpdateRefBoneMatrix(NewAnimSkeleton_FLVER flverSkeleton, bool? forceFollowFlag, int unmappedBoneID)
        {
            if (flverSkeleton != null)
            {
                int boneIndex = dummy.ParentBoneIndex;

                if (boneIndex == -1)
                {
                    boneIndex = unmappedBoneID;
                }

                ReferenceMatrix = Matrix.CreateWorld(
                    Vector3.Zero,
                    Vector3.Normalize(new Vector3(dummy.Forward.X, dummy.Forward.Y, dummy.Forward.Z)),
                    dummy.UseUpwardVector ? Vector3.Normalize(new Vector3(dummy.Upward.X, dummy.Upward.Y, dummy.Upward.Z)) : Vector3.Up)
                    * Matrix.CreateTranslation(new Vector3(dummy.Position.X, dummy.Position.Y, dummy.Position.Z))
                    * ((boneIndex >= 0 && (forceFollowFlag ?? dummy.Flag1)) ? flverSkeleton.Bones[boneIndex].FKMatrix : Matrix.Identity);
                //dummy.Flag1 = DummyFollowFlag, whether it follows, afaik
            }
        }

        public NewDummyPolyInfo(FLVER.Dummy dmy, NewAnimSkeleton_FLVER skeleton)
        {
            dummy = dmy;
            ReferenceID = dmy.ReferenceID;
            ReferenceMatrix = Matrix.CreateWorld(
                Vector3.Zero,
                Vector3.Normalize(new Vector3(dmy.Forward.X, dmy.Forward.Y, dmy.Forward.Z)),
                dmy.UseUpwardVector ? Vector3.Normalize(new Vector3(dmy.Upward.X, dmy.Upward.Y, dmy.Upward.Z)) : Vector3.Up)
                * Matrix.CreateTranslation(new Vector3(dmy.Position.X, dmy.Position.Y, dmy.Position.Z))
                * (dmy.ParentBoneIndex >= 0 ? skeleton.Bones[dmy.ParentBoneIndex].ReferenceFKMatrix : Matrix.Identity);
            AttachMatrix = Matrix.Identity;

            ArrowPrimitive = new DbgPrimWireArrow(Transform.Default, Color.White)
            {
                //Wireframe = true,
                //BackfaceCulling = true,
                //DisableLighting = true,
                OverrideColor = Color.Cyan,
            };
        }

        //public static Color ColorSpawnSFX = Color.Cyan;
        //public static Color ColorSpawnBulletsMisc = Color.Yellow;
        //public static Color ColorSpawnSFXBulletsMisc = Color.Lime;

        private Color GetCurrentSpawnColor()
        {
            if (SFXSpawnIDs.Count > 0 && (BulletSpawnIDs.Count > 0 || MiscSpawnTexts.Count > 0))
                return Main.Colors.ColorHelperDummyPolySpawnSFXBulletsMisc;
            else if (SFXSpawnIDs.Count > 0 && (BulletSpawnIDs.Count == 0 && MiscSpawnTexts.Count == 0))
                return Main.Colors.ColorHelperDummyPolySpawnSFX;
            else if (SFXSpawnIDs.Count == 0 && (BulletSpawnIDs.Count > 0 || MiscSpawnTexts.Count > 0))
                return Main.Colors.ColorHelperDummyPolySpawnBulletsMisc;
            else
                return Main.Colors.ColorHelperDummyPolyTransforms;
        }

        public Matrix GetFinalMatrix(Matrix world, bool isForce, bool isForceBigger, float opacity)
        {
            bool generalDummyPolyTransformDraw = Main.HelperDraw.EnableDummyPolyTransforms || isForce || DebugSelected;

            bool hasSpawnStuff = !(SFXSpawnIDs.Count == 0 &&
                BulletSpawnIDs.Count == 0 && MiscSpawnTexts.Count == 0);

            Vector3 currentPos = Vector3.Transform(Vector3.Zero, CurrentMatrix);
            //Vector3 currentDir = Vector3.TransformNormal(Vector3.Forward, CurrentMatrix);
            Quaternion currentRot = Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(CurrentMatrix));

            Matrix unscaledCurrentMatrix = Matrix.CreateFromQuaternion(currentRot) * Matrix.CreateTranslation(currentPos);

            if (hasSpawnStuff && !isForce)
            {
                return DBG.NewTransformSizeMatrix_DummyPoly * Matrix.CreateScale(0.4f) * unscaledCurrentMatrix * world;
            }
            else if (DebugSelected)
            {
                return DBG.NewTransformSizeMatrix_DummyPoly * Matrix.CreateScale(isForceBigger ? 0.4f : 0.2f) * unscaledCurrentMatrix * world;
            }
            else if (generalDummyPolyTransformDraw)
            {
                return DBG.NewTransformSizeMatrix_DummyPoly * Matrix.CreateScale(isForceBigger ? 0.4f : 0.2f) * unscaledCurrentMatrix * world;
            }
            return CurrentMatrix * world;
        }

        public void DrawPrim(Matrix finalCalcMatrix, bool isForce, bool isForceBigger, float opacity)
        {
            bool generalDummyPolyTransformDraw = Main.HelperDraw.EnableDummyPolyTransforms || isForce || DebugSelected;

            bool hasSpawnStuff = !(SFXSpawnIDs.Count == 0 &&
                BulletSpawnIDs.Count == 0 && MiscSpawnTexts.Count == 0);

            if (!generalDummyPolyTransformDraw && !hasSpawnStuff && !DebugSelected)
                return;

            //Vector3 currentPos = Vector3.Transform(Vector3.Zero, CurrentMatrix);
            ////Vector3 currentDir = Vector3.TransformNormal(Vector3.Forward, CurrentMatrix);
            //Quaternion currentRot = Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(CurrentMatrix));
            //
            // Matrix unscaledCurrentMatrix = Matrix.CreateFromQuaternion(currentRot) * Matrix.CreateTranslation(currentPos);

            ArrowPrimitive.EnableExtraVisibility = DBG.BoostTransformVisibility_DummyPoly;
            ArrowPrimitive.ExtraVisibilityIgnoresAlpha = DBG.BoostTransformVisibility_DummyPoly_IgnoreAlpha;

            if (hasSpawnStuff && !isForce)
            {
                ArrowPrimitive.OverrideColor = GetCurrentSpawnColor() * opacity;

                ArrowPrimitive.Draw(isForce, null, finalCalcMatrix);
            }
            else if (DebugSelected)
            {
                ArrowPrimitive.OverrideColor = Main.Colors.ColorHelperDummyPolyDbgTexts * opacity;
                ArrowPrimitive.Draw(isForce, null, finalCalcMatrix);
            }
            else if (generalDummyPolyTransformDraw)
            {
                ArrowPrimitive.OverrideColor = Main.Colors.ColorHelperDummyPolyTexts * opacity;
                ArrowPrimitive.Draw(isForce, null, finalCalcMatrix);
            }

        }

        public void DrawPrimText(Matrix world, bool isForce, int globalIDOffset, float opacity, bool includeAttackInfos)
        {
            if (DisableTextDraw && !isForce)
                return;

            SpawnPrinter.Clear();

            SpawnPrinter.ShadowThickness = 2;
            SpawnPrinter.EnableShadowCardinals = true;

            bool hasSpawnStuff = !(SFXSpawnIDs.Count == 0 && BulletSpawnIDs.Count == 0 && MiscSpawnTexts.Count == 0);

            //string dmyIDTxt = (ReferenceID == 200) ? $"[{ReferenceID} (All Over Body)]" : $"[{ReferenceID}]";
            string dmyIDTxt = $"{(ReferenceID + globalIDOffset)}";

            if (hasSpawnStuff && !isForce)
                SpawnPrinter.AppendLine(dmyIDTxt, GetCurrentSpawnColor());
            else if (Main.HelperDraw.EnableDummyPolyIDs || isForce)
                SpawnPrinter.AppendLine(dmyIDTxt, Main.Colors.ColorHelperDummyPolyTransforms * opacity);


            Vector3 currentPos = Vector3.Transform(Vector3.Zero, CurrentMatrix * world);

            if (hasSpawnStuff && !isForce)
            {
                foreach (var sfx in SFXSpawnIDs)
                {
                    SpawnPrinter.AppendLine($"SFX {sfx}", Main.Colors.ColorHelperDummyPolySpawnSFX);
                }

                foreach (var bullet in BulletSpawnIDs)
                {
                    SpawnPrinter.AppendLine($"Bullet {bullet}", Main.Colors.ColorHelperDummyPolySpawnBulletsMisc);
                }

                foreach (var misc in MiscSpawnTexts)
                {
                    SpawnPrinter.AppendLine(misc, Main.Colors.ColorHelperDummyPolySpawnBulletsMisc);
                }
            }

            if (!isForce && includeAttackInfos)
            {
                foreach (var atkInfo in ShowAttackInfos)
                {
                    string atkName = atkInfo.AttackName;
                    if (!string.IsNullOrWhiteSpace(atkName) && atkName.Length > 64)
                    {
                        atkName = atkName.Substring(0, 64) + "...";
                    }
                    SpawnPrinter.AppendLine($"{(atkInfo.IsNpcAtk ? "NPC" : "PC")} ATK {atkInfo.AtkParamID}"
                                            + (!string.IsNullOrWhiteSpace(atkName) ? $" {atkName}" : ""),
                        atkInfo.Hit0Color);
                }
            }

            if (SpawnPrinter.LineCount > 0)
            {
                SpawnPrinter.Position3D = currentPos;
                SpawnPrinter.Draw(out _);
            }
        }
    }

}
