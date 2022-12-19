using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewAnimSkeleton_FLVER
    {
        public bool BoneLimitExceeded => FlverSkeleton.Count > MaxBoneCount;

        public const int MaxBoneCount = 
            // There is no point to writing this out like this
            // other than to remind me to update it if I add
            // another bone list
            GFXShaders.FlverShader.MaxBonePerMatrixArray/*0*/ +
            GFXShaders.FlverShader.MaxBonePerMatrixArray/*1*/ +
            GFXShaders.FlverShader.MaxBonePerMatrixArray/*2*/ +
            GFXShaders.FlverShader.MaxBonePerMatrixArray/*3*/ +
            GFXShaders.FlverShader.MaxBonePerMatrixArray/*4*/ +
            GFXShaders.FlverShader.MaxBonePerMatrixArray/*5*/;

        public Matrix[] ShaderMatrices0 = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices1 = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices2 = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices3 = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices4 = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices5 = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];

        public Matrix[] ShaderMatrices0_RefPose = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices1_RefPose = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices2_RefPose = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices3_RefPose = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices4_RefPose = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices5_RefPose = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];

        public static Matrix[] IDENTITY_MATRICES = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];

        static NewAnimSkeleton_FLVER()
        {
            for (int i = 0; i < GFXShaders.FlverShader.MaxBonePerMatrixArray; i++)
            {
                IDENTITY_MATRICES[i] = Matrix.Identity;
            }

            DebugDrawTransformOfFlverBonePrim =
            new DbgPrimWireArrow("", Transform.Default, Color.White)
            {
                Category = DbgPrimCategory.AlwaysDraw,
            };
            DebugDrawTransformOfFlverBoneTextDrawer = new StatusPrinter(null, Color.White);
        }

        public List<FlverBoneInfo> FlverSkeleton = new List<FlverBoneInfo>();

        public List<int> TopLevelFlverBoneIndices = new List<int>();

        public readonly Model MODEL;

        public bool EnableRefPoseMatrices = true;

        public static int DebugDrawTransformOfFlverBoneIndex = -1;
        private static DbgPrimWireArrow DebugDrawTransformOfFlverBonePrim;
        private static StatusPrinter DebugDrawTransformOfFlverBoneTextDrawer;

        public NewAnimSkeleton_HKX HavokSkeletonThisIsMappedTo { get; private set; } = null;

        public NewAnimSkeleton_FLVER(Model mdl, List<FLVER.Bone> flverBones)
        {
            MODEL = mdl;

            int[] childCounts = new int[flverBones.Count];

            FlverSkeleton.Clear();
            TopLevelFlverBoneIndices.Clear();

            for (int i = 0; i < flverBones.Count; i++)
            {
                if (flverBones[i].ParentIndex < 0)
                    TopLevelFlverBoneIndices.Add(i);
                var newBone = new FlverBoneInfo(flverBones[i], flverBones);
                if (flverBones[i].ParentIndex >= 0)
                    childCounts[flverBones[i].ParentIndex]++;
                FlverSkeleton.Add(newBone);
            }

            for (int i = 0; i < FlverSkeleton.Count; i++)
            {
                //FlverSkeleton[i].Length = Math.Max(0.1f, 
                //    (flverBones[i].BoundingBoxMax.Z - flverBones[i].BoundingBoxMin.Z) * 0.8f);

                //if (childCounts[i] == 1 && flverBones[i].ChildIndex >= 0)
                //{
                //    var parentChildDifference = Vector3.Transform(Vector3.Zero, 
                //        FlverSkeleton[flverBones[i].ChildIndex].ReferenceMatrix) -
                //        Vector3.Transform(Vector3.Zero, FlverSkeleton[i].ReferenceMatrix);

                //    var parentChildDirection = Vector3.Normalize(parentChildDifference);

                //    var parentDir = Vector3.TransformNormal(Vector3.Backward,
                //        Matrix.CreateRotationX(flverBones[i].Rotation.X) *
                //        Matrix.CreateRotationZ(flverBones[i].Rotation.Z) *
                //        Matrix.CreateRotationY(flverBones[i].Rotation.Y));

                //    var dot = Vector3.Dot(parentDir, parentChildDirection);

                //    FlverSkeleton[i].Length = parentChildDifference.Length() * (float)Math.Cos(dot);
                //}
                //else
                //{
                //     FlverSkeleton[i].Length = Math.Max(0.1f, 
                //    (flverBones[i].BoundingBoxMax.Z - flverBones[i].BoundingBoxMin.Z) * 0.8f);
                //}

                if (flverBones[i].ParentIndex >= 0 && flverBones[i].ParentIndex < flverBones.Count)
                {
                    FlverSkeleton[flverBones[i].ParentIndex].ChildBones.Add(FlverSkeleton[i]);
                }
            }

            for (int i = 0; i < GFXShaders.FlverShader.MaxBonePerMatrixArray; i++)
            {
                ShaderMatrices0[i] = Matrix.Identity;
                ShaderMatrices1[i] = Matrix.Identity;
                ShaderMatrices2[i] = Matrix.Identity;
                ShaderMatrices3[i] = Matrix.Identity;
                ShaderMatrices4[i] = Matrix.Identity;
                ShaderMatrices5[i] = Matrix.Identity;
                ShaderMatrices0_RefPose[i] = Matrix.Identity;
                ShaderMatrices1_RefPose[i] = Matrix.Identity;
                ShaderMatrices2_RefPose[i] = Matrix.Identity;
                ShaderMatrices3_RefPose[i] = Matrix.Identity;
                ShaderMatrices4_RefPose[i] = Matrix.Identity;
                ShaderMatrices5_RefPose[i] = Matrix.Identity;
            }
        }

        public void DrawPrimitives()
        {
            for (int i = 0; i < FlverSkeleton.Count; i++)
            {
                var prevBoneColor = DBG.COLOR_FLVER_BONE;

                if (DebugDrawTransformOfFlverBoneIndex >= 0)
                {
                    if (DebugDrawTransformOfFlverBoneIndex == i)
                        DBG.COLOR_FLVER_BONE = Main.Colors.ColorHelperFlverBoneBoundingBox * 0.75f;
                    else
                        DBG.COLOR_FLVER_BONE = Main.Colors.ColorHelperFlverBone * 0.5f;
                }

                FlverSkeleton[i].DrawPrim(MODEL.CurrentTransform.WorldMatrix);

                DBG.COLOR_FLVER_BONE = Main.Colors.ColorHelperFlverBone = prevBoneColor;

                if (i == DebugDrawTransformOfFlverBoneIndex || (FlverSkeleton[i].EnablePrimDraw && DebugDrawTransformOfFlverBoneIndex < 0))
                {
                    if (DBG.GetCategoryEnableDraw(DebugPrimitives.DbgPrimCategory.FlverBone))
                    {
                        DebugDrawTransformOfFlverBonePrim.Transform = new Transform(
                    Matrix.CreateRotationY(MathHelper.Pi) *
                    Matrix.CreateScale(0.1f, 0.1f, 0.1f) * FlverSkeleton[i].CurrentMatrix);

                        if (DebugDrawTransformOfFlverBoneIndex == i)
                            DebugDrawTransformOfFlverBonePrim.OverrideColor = Main.Colors.ColorHelperFlverBone;
                        else
                            DebugDrawTransformOfFlverBonePrim.OverrideColor = new Color(Main.Colors.ColorHelperFlverBone.R,
                                Main.Colors.ColorHelperFlverBone.G, Main.Colors.ColorHelperFlverBone.B, (byte)(255 / 4));
                        DebugDrawTransformOfFlverBonePrim.Draw(null, MODEL.CurrentTransform.WorldMatrix);
                    }

                    if (i == DebugDrawTransformOfFlverBoneIndex || DBG.GetCategoryEnableNameDraw(DebugPrimitives.DbgPrimCategory.FlverBone))
                    {

                        DebugDrawTransformOfFlverBoneTextDrawer.Clear();

                        DebugDrawTransformOfFlverBoneTextDrawer.AppendLine(
                            FlverSkeleton[i].Name,
                            Main.Colors.ColorHelperFlverBone);

                        DebugDrawTransformOfFlverBoneTextDrawer.Position3D =
                            Vector3.Transform(Vector3.Zero,
                            FlverSkeleton[i].CurrentMatrix
                            * MODEL.CurrentTransform.WorldMatrix);

                        GFX.SpriteBatchBeginForText();
                        DebugDrawTransformOfFlverBoneTextDrawer.Draw();
                        GFX.SpriteBatchEnd();

                    }
                }
            }
        }

        public Matrix this[int boneIndex]
        {
            get
            {
                int bank = boneIndex / GFXShaders.FlverShader.MaxBonePerMatrixArray;
                int bone = boneIndex % GFXShaders.FlverShader.MaxBonePerMatrixArray;

                if (bank == 0)
                    return ShaderMatrices0[bone];
                else if (bank == 1)
                    return ShaderMatrices1[bone];
                else if (bank == 2)
                    return ShaderMatrices2[bone];
                else if (bank == 3)
                    return ShaderMatrices3[bone];
                else if (bank == 4)
                    return ShaderMatrices4[bone];
                else if (bank == 5)
                    return ShaderMatrices5[bone];
                else
                    return Matrix.Identity;
            }
            set
            {

                //Matrix v = value;

                //if (EnableRefPoseMatrices)
                //{
                //    v = FlverSkeleton[boneIndex].ReferenceMatrix * v;
                //}

                FlverSkeleton[boneIndex].CurrentMatrix = FlverSkeleton[boneIndex].ReferenceMatrix * value;

                

                int bank = boneIndex / GFXShaders.FlverShader.MaxBonePerMatrixArray;
                int bone = boneIndex % GFXShaders.FlverShader.MaxBonePerMatrixArray;

                if (bank == 0)
                    ShaderMatrices0[bone] = value;
                else if (bank == 1)
                    ShaderMatrices1[bone] = value;
                else if (bank == 2)
                    ShaderMatrices2[bone] = value;
                else if (bank == 3)
                    ShaderMatrices3[bone] = value;
                else if (bank == 4)
                    ShaderMatrices4[bone] = value;
                else if (bank == 5)
                    ShaderMatrices5[bone] = value;

                if (EnableRefPoseMatrices)
                {
                    if (bank == 0)
                        ShaderMatrices0_RefPose[bone] = FlverSkeleton[boneIndex].ReferenceMatrix * value;
                    else if (bank == 1)
                        ShaderMatrices1_RefPose[bone] = FlverSkeleton[boneIndex].ReferenceMatrix * value;
                    else if (bank == 2)
                        ShaderMatrices2_RefPose[bone] = FlverSkeleton[boneIndex].ReferenceMatrix * value;
                    else if (bank == 3)
                        ShaderMatrices3_RefPose[bone] = FlverSkeleton[boneIndex].ReferenceMatrix * value;
                    else if (bank == 4)
                        ShaderMatrices4_RefPose[bone] = FlverSkeleton[boneIndex].ReferenceMatrix * value;
                    else if (bank == 5)
                        ShaderMatrices5_RefPose[bone] = FlverSkeleton[boneIndex].ReferenceMatrix * value;
                }

                MODEL.DummyPolyMan.UpdateFlverBone(boneIndex, value);
            }
        }

        public void RevertToReferencePose()
        {
            for (int i = 0; i < FlverSkeleton.Count; i++)
            {
                this[i] = Matrix.Identity;
            }
        }

        public void MapToSkeleton(NewAnimSkeleton_HKX skel, bool isRemo)
        {
            //if (MODEL.Name.StartsWith("o"))
            //{
            //    Console.WriteLine("test");
            //}

            for (int i = 0; i < FlverSkeleton.Count; i++)
            {
                var bone = FlverSkeleton[i];
                string hkxBoneName = isRemo ? ((bone.Name.ToUpper() == "MASTER" || i == 0) ? MODEL.Name : $"{MODEL.Name}_{bone.Name}") : bone.Name;
                int boneIndex = skel.HkxSkeleton.FindIndex(b => b.Name == hkxBoneName);
                bone.HkxBoneIndex = boneIndex;
            }
            HavokSkeletonThisIsMappedTo = skel;
        }

        public void CopyFromHavokSkeleton()
        {
            if (HavokSkeletonThisIsMappedTo == null)
                return;

            for (int i = 0; i < FlverSkeleton.Count; i++)
            {
                int hkxBoneIndex = FlverSkeleton[i].HkxBoneIndex;
                if (hkxBoneIndex >= 0 && hkxBoneIndex < HavokSkeletonThisIsMappedTo.HkxSkeleton.Count)
                {
                    this[i] = Matrix.Invert(FlverSkeleton[i].ReferenceMatrix) * HavokSkeletonThisIsMappedTo.HkxSkeleton[hkxBoneIndex].CurrentMatrix;
                }
                else
                {
                    this[i] = FlverSkeleton[i].ReferenceMatrix;
                }
            }
        }

        public class FlverBoneInfo
        {
            public string Name;
            public int ParentIndex;
            public Matrix ParentReferenceMatrix = Matrix.Identity;
            public Matrix ReferenceMatrix = Matrix.Identity;
            public int HkxBoneIndex = -1;
            public Matrix CurrentMatrix = Matrix.Identity;

            public Matrix? NubReferenceMatrix = null;
            //public Matrix? NubCurrentMatrix = null;

            public List<FlverBoneInfo> ChildBones = new List<FlverBoneInfo>();

            //public float Length = 1.0f;
            //public IDbgPrim BonePrim;
            public DbgPrimWireBox BoundingBoxPrim;

            static IDbgPrim GlobalBonePrim;

            //public StatusPrinter SpawnPrinter = new StatusPrinter(null);

            public bool IsNub = false;

            public bool EnablePrimDraw = true;

            public void ApplyHkxBoneProperties(NewAnimSkeleton_HKX.HkxBoneInfo copyFromHkx, List<NewAnimSkeleton_HKX.HkxBoneInfo> hkxBoneList, List<FlverBoneInfo> flverBoneList)
            {
                Name = copyFromHkx.Name;
                ParentReferenceMatrix = copyFromHkx.ReferenceMatrix * Matrix.Invert(copyFromHkx.RelativeReferenceMatrix);
                ReferenceMatrix = copyFromHkx.ReferenceMatrix;
                CurrentMatrix = ReferenceMatrix;
                HkxBoneIndex = hkxBoneList.IndexOf(copyFromHkx);

                IsNub = false;
                ChildBones.Clear();
                foreach (var ci in copyFromHkx.ChildIndices)
                {
                    if (ci >= 0 && ci < hkxBoneList.Count)
                    {
                        var matchingFlverChildBone = flverBoneList.FirstOrDefault(b => b.Name == hkxBoneList[ci].Name);
                        if (matchingFlverChildBone != null && !ChildBones.Contains(matchingFlverChildBone))
                        {
                            ChildBones.Add(matchingFlverChildBone);
                        }
                    }
                }
            }

            public FlverBoneInfo(NewAnimSkeleton_HKX.HkxBoneInfo copyFromHkx, List<NewAnimSkeleton_HKX.HkxBoneInfo> hkxBoneList, List<FlverBoneInfo> flverBoneList)
            {
                ApplyHkxBoneProperties(copyFromHkx, hkxBoneList, flverBoneList);

                BoundingBoxPrim = new DbgPrimWireBox(Transform.Default,
                        Vector3.One * -0.01f,
                        Vector3.One * 0.01f,
                        DBG.COLOR_FLVER_BONE_BBOX)
                {
                    Category = DbgPrimCategory.FlverBoneBoundingBox,
                };

                //SpawnPrinter.AppendLine(Name, DBG.COLOR_FLVER_BONE);
            }

            public FlverBoneInfo(FLVER.Bone bone, List<FLVER.Bone> boneList)
            {
                ParentIndex = bone.ParentIndex;

                if (GlobalBonePrim == null)
                {
                    GlobalBonePrim = new DbgPrimWireBone("(BONE)", new Transform(Matrix.Identity), DBG.COLOR_FLVER_BONE)
                    {
                        Category = DbgPrimCategory.FlverBone,
                    };
                }

                Matrix GetBoneMatrix(SoulsFormats.FLVER.Bone b, bool saveParentBone)
                {
                    SoulsFormats.FLVER.Bone parentBone = b;

                    var result = Matrix.Identity;

                    bool isTopBone = true;

                    do
                    {
                        result *= Matrix.CreateScale(parentBone.Scale.X, parentBone.Scale.Y, parentBone.Scale.Z);
                        result *= Matrix.CreateRotationX(parentBone.Rotation.X);
                        result *= Matrix.CreateRotationZ(parentBone.Rotation.Z);
                        result *= Matrix.CreateRotationY(parentBone.Rotation.Y);
                        result *= Matrix.CreateTranslation(parentBone.Translation.X, parentBone.Translation.Y, parentBone.Translation.Z);

                        if (parentBone.ParentIndex >= 0)
                            parentBone = boneList[parentBone.ParentIndex];
                        else
                            parentBone = null;

                        isTopBone = false;

                        if (saveParentBone && isTopBone)
                            ParentReferenceMatrix = GetBoneMatrix(parentBone, saveParentBone: false);
                    }
                    while (parentBone != null);

                    return result;
                }

                ReferenceMatrix = GetBoneMatrix(bone, saveParentBone: true);
                Name = bone.Name;

                //SpawnPrinter.AppendLine(Name, DBG.COLOR_FLVER_BONE);

                if (bone.Unk3C == 0)
                {
                    var nubBone = boneList.Where(bn => bn.Name == bone.Name + "Nub").FirstOrDefault();

                    if (nubBone != null)
                    {
                        var nubMat = Matrix.Identity;
                        nubMat *= Matrix.CreateScale(nubBone.Scale.X, nubBone.Scale.Y, nubBone.Scale.Z);
                        nubMat *= Matrix.CreateRotationX(nubBone.Rotation.X);
                        nubMat *= Matrix.CreateRotationZ(nubBone.Rotation.Z);
                        nubMat *= Matrix.CreateRotationY(nubBone.Rotation.Y);
                        nubMat *= Matrix.CreateTranslation(nubBone.Translation.X, nubBone.Translation.Y, nubBone.Translation.Z);

                        NubReferenceMatrix = nubMat;
                    }

                    
                }

                IsNub = bone.Unk3C != 0;

                BoundingBoxPrim = new DbgPrimWireBox(Transform.Default,
                        new Vector3(bone.BoundingBoxMin.X, bone.BoundingBoxMin.Y, bone.BoundingBoxMin.Z),
                        new Vector3(bone.BoundingBoxMax.X, bone.BoundingBoxMax.Y, bone.BoundingBoxMax.Z),
                        DBG.COLOR_FLVER_BONE_BBOX)
                {
                    Category = DbgPrimCategory.FlverBoneBoundingBox,
                };

            }

            public void DrawPrim(Matrix world)
            {
                if (BoundingBoxPrim != null)
                {
                    //BonePrim.Transform = new Transform(Matrix.CreateScale(Length) * CurrentMatrix);
                    //BonePrim.Draw(null, world);
                    BoundingBoxPrim.OverrideColor = DBG.COLOR_FLVER_BONE_BBOX;
                    BoundingBoxPrim.UpdateTransform(new Transform(CurrentMatrix));
                    BoundingBoxPrim.Draw(null, world);
                }

                Vector3 boneStart = Vector3.Transform(Vector3.Zero, CurrentMatrix);

                void DrawToEndPoint(Vector3 endPoint)
                {
                    var forward = -Vector3.Normalize(endPoint - boneStart);

                    Matrix hitboxMatrix = Matrix.CreateWorld(boneStart, forward, Vector3.Up);

                    if (forward.X == 0 && forward.Z == 0)
                    {
                        if (forward.Y >= 0)
                            hitboxMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(boneStart);
                        else
                            hitboxMatrix = Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(boneStart);
                    }

                    float boneLength = (endPoint - boneStart).Length();

                    GlobalBonePrim.Transform = new Transform(Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateScale(boneLength) * hitboxMatrix);
                    GlobalBonePrim.OverrideColor = DBG.COLOR_FLVER_BONE;
                    GlobalBonePrim.Draw(null, world);

                    //using (var tempBone = new DbgPrimWireBone(Name, new Transform(Matrix.CreateRotationY(-MathHelper.PiOver2) * hitboxMatrix), DBG.COLOR_FLVER_BONE, boneLength, boneLength * 0.2f))
                    //{
                    //    tempBone.Draw(null, world);
                    //}
                }

                if (DBG.GetCategoryEnableDraw(DbgPrimCategory.FlverBone))
                {
                    if (NubReferenceMatrix != null)
                    {
                        DrawToEndPoint(Vector3.Transform(Vector3.Zero, NubReferenceMatrix.Value * CurrentMatrix));
                    }

                    foreach (var cb in ChildBones)
                    {
                        DrawToEndPoint(Vector3.Transform(Vector3.Zero, cb.CurrentMatrix));
                    }
                }

                //if (DBG.CategoryEnableNameDraw[DbgPrimCategory.FlverBone])
                //{
                //    SpawnPrinter.Position3D = Vector3.Transform(Vector3.Zero, CurrentMatrix * world);
                //    SpawnPrinter.Draw();
                //}

               
            }
        }

        
    }
}
