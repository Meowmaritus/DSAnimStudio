using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;

namespace DSAnimStudio
{

    public class NewBone
    {
        public enum BoneMasks
        {
            None = 0,
            UpperBody = 1 << 0,
            SekiroFace = 1 << 1,
        }

        public BoneMasks Masks;

        public int MapToOtherBoneIndex = -1;

        public int NestDeepness = 0;

        public int Index = -1;
        //public string DisplayName => $"[{Index}] {Name}";

        public string Name;
        public short ParentIndex = -1;
        public Matrix ParentReferenceFKMatrix = Matrix.Identity;
        public Matrix ReferenceLocalMatrix = Matrix.Identity;
        public NewBlendableTransform ReferenceLocalTransform = NewBlendableTransform.Identity;
            
        public Matrix ReferenceFKMatrix = Matrix.Identity;

        public List<NewBone> ChildBones = new List<NewBone>();
        public List<int> ChildIndices = new List<int>();

        public NewBlendableTransform LocalTransform = NewBlendableTransform.Identity;

        public Matrix? MasterCompleteOverrideFK = null;

        private Matrix fkMatrix = Matrix.Identity;
        public Matrix FKMatrix
        {
            get => fkMatrix;
            set
            {
                if (Main.Debug.BreakOnBadBoneFKMatrixWrite && value.HasAnyNaN() || value.HasAnyInfinity())
                {
                    Console.Write("break");
                }
                fkMatrix = value;
            }
        }
            
        public float SetWeight = 1;
        public bool DisableWeight = false;
        public float Weight => !DisableWeight ? SetWeight : 0;



        public float SetWeight_Translation = 1;
        public bool DisableWeight_Translation = false;
        public float Weight_Translation => !DisableWeight_Translation ? SetWeight_Translation : 0;



        public float SetWeight_Rotation = 1;
        public bool DisableWeight_Rotation = false;
        public float Weight_Rotation => !DisableWeight_Rotation ? SetWeight_Rotation : 0;




        public float SetWeight_Scale = 1;
        public bool DisableWeight_Scale = false;
        public float Weight_Scale => !DisableWeight_Scale ? SetWeight_Scale : 0;






        public float DebugMult = 1;
        public float DebugMultTranslation = 1;
        public float DebugMultRotation = 1;
        public float DebugMultScale = 1;



        public bool HasBoneGluer = false;
        
        
        
        
        
        public bool IsBoneGluerDebugView = false;
        
        public bool IsDebugDrawTransforms;
        public bool IsDebugDrawLines;
        public bool IsDebugDrawBoxes;
        public bool IsDebugDrawText;
        
        
        // FLVER stuff
        public Matrix? NubReferenceFKMatrix = null;
        public bool IsNub = false;
        public bool StartedAsNub = false;
        public DbgPrimWireBox BoundingBoxPrim;
        static IDbgPrim GlobalBonePrim;
        public bool EnablePrimDraw = true;
        
        public float Debug_SkeletonMapperWeight = 1;
        
        
        public void ApplyHkxBoneProperties(NewBone copyFromBone, List<NewBone> copyFromBoneList, List<NewBone> thisBoneList)
        {
            Masks = copyFromBone.Masks;
            MapToOtherBoneIndex = copyFromBoneList.IndexOf(copyFromBone);
            
            var thisBoneIndex = thisBoneList.IndexOf(this);
            
            ReferenceFKMatrix = copyFromBone.ReferenceFKMatrix;
            IsNub = false;
            //ParentIndex = -1;
            
            // Remove from old parent if needed.
            if (ParentIndex >= 0)
            {
                var ogParent = thisBoneList[ParentIndex];
                if (ogParent.ChildIndices.Contains(thisBoneIndex))
                    ogParent.ChildIndices.Remove(thisBoneIndex);
                if (ogParent.ChildBones.Contains(this))
                    ogParent.ChildBones.Remove(this);
            }
            
            var otherParentIndex = copyFromBone.ParentIndex;
            if (otherParentIndex >= 0 && otherParentIndex < copyFromBoneList.Count)
            {
                var otherParent = copyFromBoneList[otherParentIndex];
                var matchingParentBone = thisBoneList.FirstOrDefault(b => b.Name == otherParent.Name);
                if (matchingParentBone != null)
                {
                    

                    ParentIndex = (short)otherParentIndex;
                    if (!matchingParentBone.ChildBones.Contains(this))
                        matchingParentBone.ChildBones.Add(this);

                    if (!matchingParentBone.ChildIndices.Contains(thisBoneIndex))
                        matchingParentBone.ChildIndices.Add(thisBoneIndex);
                }
                else
                {
                    
                }
            }
            
        }

        public void AdjustRelativeTransformsForNewHierarchy(List<NewBone> thisBoneList)
        {
            ReferenceLocalMatrix = ReferenceFKMatrix;
            ParentReferenceFKMatrix = Matrix.Identity;
            if (ParentIndex >= 0)
            {
                var parent = thisBoneList[ParentIndex];
                ParentReferenceFKMatrix = parent.ReferenceFKMatrix;
                ReferenceLocalMatrix *= Matrix.Invert(ParentReferenceFKMatrix);
            }
            ReferenceLocalTransform = ReferenceLocalMatrix.ToNewBlendableTransform();
        }

        public NewBone()
        {
            
        }
        
        public NewBone(NewBone copyFromHkx, List<NewBone> otherBoneList, List<NewBone> boneList)
        {
            ApplyHkxBoneProperties(copyFromHkx, otherBoneList, boneList);

            BoundingBoxPrim = new DbgPrimWireBox(Transform.Default,
                Vector3.One * -0.01f,
                Vector3.One * 0.01f,
                Color.White);

            //SpawnPrinter.AppendLine(Name, DBG.COLOR_FLVER_BONE);
        }

        public NewBone(FLVER.Node bone, List<FLVER.Node> boneList)
        {
            ParentIndex = bone.ParentIndex;

            if (GlobalBonePrim == null)
            {
                GlobalBonePrim =
                    new DbgPrimWireBone(new Transform(Matrix.Identity), Color.White);
            }

            Matrix GetBoneMatrix(SoulsFormats.FLVER.Node b)
            {
                SoulsFormats.FLVER.Node parentBone = b;

                var result = Matrix.Identity;

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
                }
                while (parentBone != null);

                return result;
            }

            ReferenceFKMatrix = FKMatrix = GetBoneMatrix(bone);
            if (bone.ParentIndex >= 0 && bone.ParentIndex < boneList.Count)
            {
                var parentBone = boneList[bone.ParentIndex];
                ParentReferenceFKMatrix = GetBoneMatrix(parentBone);
            }

            ReferenceLocalMatrix = Matrix.CreateScale(bone.Scale.X, bone.Scale.Y, bone.Scale.Z)
                * Matrix.CreateRotationX(bone.Rotation.X)
                * Matrix.CreateRotationZ(bone.Rotation.Z)
                * Matrix.CreateRotationY(bone.Rotation.Y)
                * Matrix.CreateTranslation(bone.Translation.X, bone.Translation.Y, bone.Translation.Z);

            ReferenceLocalTransform = LocalTransform = ReferenceLocalMatrix.ToNewBlendableTransform();
            
            Name = bone.Name;

            //SpawnPrinter.AppendLine(Name, DBG.COLOR_FLVER_BONE);

            if ((bone.Flags & FLVER.Node.NodeFlags.DummyOwner) == 0)
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

                    NubReferenceFKMatrix = nubMat;
                }

                
            }

            //ac6 test fix?
            StartedAsNub = IsNub = ((bone.Flags & FLVER.Node.NodeFlags.DummyOwner) != 0);

            BoundingBoxPrim = new DbgPrimWireBox(Transform.Default,
                new Vector3(bone.BoundingBoxMin.X, bone.BoundingBoxMin.Y, bone.BoundingBoxMin.Z),
                new Vector3(bone.BoundingBoxMax.X, bone.BoundingBoxMax.Y, bone.BoundingBoxMax.Z),
                Color.White);

            }

            public void DrawPrim(Matrix world, bool enableDrawLines, bool enableDrawBox, Color bbColor, Color lineColor)
            {
                if (BoundingBoxPrim != null && (enableDrawBox))
                {
                    //BonePrim.Transform = new Transform(Matrix.CreateScale(Length) * CurrentMatrix);
                    //BonePrim.Draw(null, world);
                    BoundingBoxPrim.OverrideColor = bbColor;
                    BoundingBoxPrim.UpdateTransform(new Transform(FKMatrix));
                    BoundingBoxPrim.Draw(enableDrawBox, null, world);
                }

                Vector3 boneStart = Vector3.Transform(Vector3.Zero, FKMatrix);

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
                    GlobalBonePrim.OverrideColor = lineColor;
                    GlobalBonePrim.Draw(true, null, world);

                    //using (var tempBone = new DbgPrimWireBone(Name, new Transform(Matrix.CreateRotationY(-MathHelper.PiOver2) * hitboxMatrix), DBG.COLOR_FLVER_BONE, boneLength, boneLength * 0.2f))
                    //{
                    //    tempBone.Draw(null, world);
                    //}
                }

                if (enableDrawLines)
                {
                    if (NubReferenceFKMatrix != null)
                    {
                        DrawToEndPoint(Vector3.Transform(Vector3.Zero, NubReferenceFKMatrix.Value * LocalTransform.GetXnaMatrixFull()));
                    }

                    foreach (var cb in ChildBones)
                    {
                        DrawToEndPoint(Vector3.Transform(Vector3.Zero, cb.FKMatrix));
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