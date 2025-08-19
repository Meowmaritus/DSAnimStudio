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
    public abstract class NewAnimSkeleton
    {
        public readonly string GUID = Guid.NewGuid().ToString();
        
        public Transform CurrentTransform = Transform.Default;

        public string DebugName = "";
        
        public object _lock_DebugViewWeightOfBone = new object();
        public string[] DebugViewWeightOfBone_BoneNames = null;
        public int DebugViewWeightOfBone_ImguiIndex { get; protected set; } = -1;

        public float ImGuiDebugWeightListNameWidth = 150;
        public float ImGuiDebugWeightListBarWidth = 150;

        public float Debug_SetAllWeightsTo_Slider = 1;
        public float Debug_SetAllWeightsTo_Translation_Slider = 1;
        public float Debug_SetAllWeightsTo_Rotation_Slider = 1;
        public float Debug_SetAllWeightsTo_Scale_Slider = 1;
        public float Debug_SetDebugMults_Slider = 1;
        public float Debug_SetDebugMults_Translation_Slider = 1;
        public float Debug_SetDebugMults_Rotation_Slider = 1;
        public float Debug_SetDebugMults_Scale_Slider = 1;


        public void DebugViewWeightOfBone_GenerateBoneNames()
        {
            List<string> list = new List<string>();
            list.Add("<None>");
            foreach (var b in Bones)
            {
                list.Add($"[{b.Index}] {b.Name}");
            }

            DebugViewWeightOfBone_BoneNames = list.ToArray();
        }

        public bool EnableSkeletonMapping = true;
        
        public object _lock_OtherSkeletonMapping = new object();
        public NewAnimSkeleton OtherSkeletonThisIsMappedTo { get; private set; } = null;
        public List<int> BonesNotMappedToOtherSkeleton { get; private set; } = null;
        public void UnmapFromOtherSkeleton()
        {
            lock (_lock_OtherSkeletonMapping)
            {
                OtherSkeletonThisIsMappedTo = null;
                BonesNotMappedToOtherSkeleton = null;
            }
        }

        public NewBone GetParentOfBone(string boneName)
        {
            return GetParentOfBone(GetBoneIndexByName(boneName));
        }

        public NewBone GetParentOfBone(NewBone bone)
        {
            if (bone != null)
            {
                int boneIndex = Bones.IndexOf(bone);
                if (boneIndex >= 0)
                {
                    return GetParentOfBone(boneIndex);
                }
            }
            return null;
        }
        public NewBone GetParentOfBone(int boneIndex)
        {
            if (boneIndex >= 0 && boneIndex < Bones.Count)
            {
                var bone = Bones[boneIndex];
                if (bone.ParentIndex >= 0 && bone.ParentIndex < Bones.Count)
                {
                    return Bones[bone.ParentIndex];
                }
            }
            return null;
        }
        
        
        public void MapToOtherSkeleton(NewAnimSkeleton otherSkel)
        {
            //if (MODEL.Name.StartsWith("o"))
            //{
            //    Console.WriteLine("test");
            //}

            lock (_lock_OtherSkeletonMapping)
            {
                if (OtherSkeletonThisIsMappedTo != otherSkel)
                {
                    List<NewBone> nubBones = new();
                    BonesNotMappedToOtherSkeleton = new List<int>();
                    for (int i = 0; i < Bones.Count; i++)
                    {
                        var bone = Bones[i];
                        string hkxBoneName = bone.Name;
                        int otherBoneIndex = otherSkel.Bones.FindIndex(b => b.Name == hkxBoneName);
                        bone.MapToOtherBoneIndex = otherBoneIndex;
                        if (otherBoneIndex >= 0 && bone.IsNub)
                        {
                            nubBones.Add(bone);
                            bone.ApplyHkxBoneProperties(otherSkel.Bones[otherBoneIndex], otherSkel.Bones, Bones);
                        }

                        if (otherBoneIndex < 0)
                            BonesNotMappedToOtherSkeleton.Add(i);
                    }

                    TopLevelBoneIndices.Clear();
                    for (int i = 0; i < Bones.Count; i++)
                    {
                        var bone = Bones[i];

                        if (nubBones.Contains(bone))
                        {
                            bone.AdjustRelativeTransformsForNewHierarchy(Bones);
                        }

                        if (bone.ParentIndex < 0)
                            TopLevelBoneIndices.Add(i);
                    }
                }

                OtherSkeletonThisIsMappedTo = otherSkel;
            }
        }

        /// <summary>
        /// Needs use of <see cref="_lock_OtherSkeletonMapping"/>
        /// </summary>
        public void CopyMatricesDirectlyFromOtherSkeleton(bool writeToShaderMatrices)
        {
            if (OtherSkeletonThisIsMappedTo == null)
                return;
            
            
            for (int i = 0; i < Bones.Count; i++)
            {
                var thisBone = Bones[i];
                int hkxBoneIndex = Bones[i].MapToOtherBoneIndex;
                if (hkxBoneIndex >= 0 && hkxBoneIndex < OtherSkeletonThisIsMappedTo.Bones.Count)
                {
                    var otherBone = OtherSkeletonThisIsMappedTo.Bones[hkxBoneIndex];
                    thisBone.LocalTransform = otherBone.LocalTransform;
                    thisBone.FKMatrix = otherBone.FKMatrix;
                    if (writeToShaderMatrices && this is NewAnimSkeleton_FLVER asFlverSkel)
                        asFlverSkel.CopyBoneToShaderMatrices(i);
                }
            }
        }
        
        public void CalculateFKFromLocalTransforms(Func<int, Matrix, Matrix, NewBlendableTransform> getLocalTrack = null)
        {
            //OtherSkeletonThisIsMappedTo?.CalculateFKFromLocalTransforms();
            
            void WalkTree(int i, Matrix currentMatrix, Matrix scaleMatrix)
            {
                var parentTransformation = currentMatrix;
                var parentScaleMatrix = scaleMatrix;

                if (getLocalTrack  != null)
                {
                    Bones[i].LocalTransform = getLocalTrack(i, currentMatrix, scaleMatrix);
                    //if (Bones[i].SetWeight != 1)
                    //{
                    //    Bones[i].LocalTransform = NewBlendableTransform.Lerp(Bones[i].ReferenceLocalTransform, Bones[i].LocalTransform, Bones[i].SetWeight);
                    //}
                }

                var curTransform = Bones[i].LocalTransform;

                var weight_Translation = Bones[i].Weight * Bones[i].Weight_Translation;
                var weight_Rotation = Bones[i].Weight * Bones[i].Weight_Rotation;
                var weight_Scale = Bones[i].Weight * Bones[i].Weight_Scale;

                if (weight_Scale < 0.01f)
                    weight_Scale = 0.01f;

                var lerpedTransform = curTransform;
                if (weight_Translation != 1 || weight_Scale != 1 || weight_Rotation != 1)
                {
                    lerpedTransform = new NewBlendableTransform(
                        System.Numerics.Vector3.Lerp(Bones[i].ReferenceLocalTransform.Translation, curTransform.Translation, weight_Translation),
                        System.Numerics.Vector3.Lerp(Bones[i].ReferenceLocalTransform.Scale, curTransform.Scale, weight_Scale),
                        System.Numerics.Quaternion.Slerp(Bones[i].ReferenceLocalTransform.Rotation, curTransform.Rotation, weight_Rotation));
                }

                var dbgMult_Translation = Bones[i].DebugMult * Bones[i].DebugMultTranslation;
                var dbgMult_Rotation = Bones[i].DebugMult * Bones[i].DebugMultRotation;
                var dbgMult_Scale = Bones[i].DebugMult * Bones[i].DebugMultScale;

                if (Math.Abs(dbgMult_Scale) < 0.05f)
                    dbgMult_Scale = 0.05f;

                if (dbgMult_Translation != 1)
                {
                    lerpedTransform.Translation = System.Numerics.Vector3.Lerp(System.Numerics.Vector3.Zero, lerpedTransform.Translation, dbgMult_Translation);
                }

                if (dbgMult_Scale != 1)
                {
                    lerpedTransform.Scale = System.Numerics.Vector3.Lerp(System.Numerics.Vector3.Zero, lerpedTransform.Scale, dbgMult_Scale);
                }

                if (dbgMult_Rotation != 1)
                {
                    lerpedTransform.Rotation = System.Numerics.Quaternion.Slerp(System.Numerics.Quaternion.Identity, lerpedTransform.Rotation, dbgMult_Rotation);
                }

                //var lerpedTransform = (NewBlendableTransform.Lerp(Bones[i].ReferenceLocalTransform, curTransform, Bones[i].Weight));
                currentMatrix = lerpedTransform.GetMatrix().ToXna();

                scaleMatrix = lerpedTransform.GetMatrixScale().ToXna();

                //if (AnimationLayers[0].IsAdditiveBlend && (i >= 0 && i < MODEL.Skeleton.HkxSkeleton.Count))
                //    currentMatrix = MODEL.Skeleton.HkxSkeleton[i].RelativeReferenceMatrix * currentMatrix;

                currentMatrix *= parentTransformation;
                scaleMatrix *= parentScaleMatrix;

                if (Bones[i].MasterCompleteOverrideFK != null)
                {
                    currentMatrix = Bones[i].MasterCompleteOverrideFK.Value;
                    scaleMatrix = Matrix.Identity;
                }

                var finalMat = scaleMatrix * currentMatrix;

                
                
                int hkxBoneIndex = Bones[i].MapToOtherBoneIndex;
                
                if (EnableSkeletonMapping && OtherSkeletonThisIsMappedTo != null && hkxBoneIndex >= 0 && hkxBoneIndex < OtherSkeletonThisIsMappedTo.Bones.Count)
                {
                    var otherBone = OtherSkeletonThisIsMappedTo.Bones[hkxBoneIndex];
                    Bones[i].FKMatrix = otherBone.FKMatrix;



                    var weight_Translation_Mapper = Bones[i].Weight * Bones[i].Weight_Translation;
                    var weight_Rotation_Mapper = Bones[i].Weight * Bones[i].Weight_Rotation;
                    var weight_Scale_Mapper = Bones[i].Weight * Bones[i].Weight_Scale;

                    //if (weight_Scale_Mapper < 0.01f)
                    //    weight_Scale_Mapper = 0.01f;


                    if (weight_Translation_Mapper != 1 || weight_Scale_Mapper != 1 || weight_Rotation_Mapper != 1)
                    {
                        var lerpFromTransform_Mapper = (Bones[i].ReferenceLocalTransform.GetXnaMatrixFull() * (parentScaleMatrix * parentTransformation)).ToNewBlendableTransform();

                        var lerpedTransform_Mapper = Bones[i].FKMatrix.ToNewBlendableTransform();

                        lerpedTransform_Mapper = new NewBlendableTransform(
                            System.Numerics.Vector3.Lerp(lerpFromTransform_Mapper.Translation, lerpedTransform_Mapper.Translation, weight_Translation_Mapper),
                            System.Numerics.Vector3.Lerp(lerpFromTransform_Mapper.Scale, lerpedTransform_Mapper.Scale, weight_Scale_Mapper),
                            System.Numerics.Quaternion.Slerp(lerpFromTransform_Mapper.Rotation, lerpedTransform_Mapper.Rotation, weight_Rotation_Mapper));

                        Bones[i].FKMatrix = lerpedTransform_Mapper.GetXnaMatrixFull();
                    }

                   



                    //Debug sanity check
                    //Bones[i].FKMatrix *= Matrix.CreateTranslation(Main.RandFloat(), Main.RandFloat(), Main.RandFloat());


                    // Not tested:
                    scaleMatrix = Matrix.Identity;
                    currentMatrix = otherBone.FKMatrix;
                }
                else
                {
                    Bones[i].FKMatrix = finalMat;
                    
                    //Debug sanity check
                    //Bones[i].FKMatrix *= Matrix.CreateTranslation(Main.RandFloat(), Main.RandFloat(), Main.RandFloat());
                }
                
                
                
                OnBoneMatrixSet(i);

                foreach (var c in Bones[i].ChildIndices)
                    WalkTree(c, currentMatrix, scaleMatrix);
            }

            foreach (var root in TopLevelBoneIndices)
                WalkTree(root, Matrix.Identity, Matrix.Identity);
        }
        
        
        public void OnBoneMatrixSet(int index)
        {
            BoneMatrixSet?.Invoke(this, index);
        }

        public event EventHandler<int> BoneMatrixSet;

        public void OnBoneTransformSet(int index)
        {
            // debug shit
            //SetAllBoneOverrideFlags(false);

            //(this as INewAnimSkeletonHelper).ModifyBoneTransformFK(index, fk =>
            //{
            //    fk.Translation.X = 0;
            //    return fk;
            //}, setOverrideFlag: false);

            //if (Bones[index].Name == "Head")
            //{
            //    var hkSkel = Scene.MainModel?.ChrAsm?.FacegenModel?.HavokSkeleton;
            //    if (hkSkel == this)
            //    {
            //        var fk = (Scene.MainModel.HavokSkeleton as INewAnimSkeletonHelper).GetBoneTransformFK("Head");
                    
            //        if (hkSkel == this)
            //        {
            //            (hkSkel as INewAnimSkeletonHelper).SetBoneTransformFK(index, fk, true);
            //        }
            //    }
            //}

            //for (int i = 0; i < Bones.Count; i++)
            //{
            //    (this as INewAnimSkeletonHelper).ModifyBoneTransformFK(i, fk =>
            //    {
            //        fk.Translation *= 2;
            //        return fk;
            //    }, setOverrideFlag: false);
            //}

            //(this as INewAnimSkeletonHelper).ModifyBoneTransformFK("L_UpperArm", fk =>
            //{
            //    fk.Translation.X = 3;
            //    return fk;
            //}, setOverrideFlag: false);

            //(this as INewAnimSkeletonHelper).ModifyBoneTransformFK("R_UpperArm", fk =>
            //{
            //    fk.Translation.X = -3;
            //    return fk;
            //}, setOverrideFlag: false);


            BoneTransformSet?.Invoke(this, index);
        }

        public event EventHandler<int> BoneTransformSet;

        public void RevertToReferencePose()
        {
            CalculateFKFromLocalTransforms((idx, pMat, pSMat) => Bones[idx].ReferenceLocalTransform);
            if (this is NewAnimSkeleton_FLVER asFlverSkel)
            {
                for (int i = 0; i < Bones.Count; i++)
                {
                    asFlverSkel.CopyBoneToShaderMatrices(i);
                }
            }    
            
        }

        static NewAnimSkeleton()
        {
            DebugDrawTransformOfFlverBonePrim =
                new DbgPrimWireArrow(Transform.Default, Color.White);
            //DebugDrawTransformOfFlverBonePrim = new DbgPrimSolidArrow(Transform.Default, Color.White);
            DebugDrawTransformOfFlverBoneTextDrawer = new StatusPrinter(null, Color.White);
        }

        public List<NewBone> Bones = new List<NewBone>();
        public Dictionary<string, int> BoneIndices_ByName = new Dictionary<string, int>();

        // bone index input stuff

        public List<int> TopLevelBoneIndices = new List<int>();


        public int DebugDrawTransformOfFlverBoneIndex = -1;
        protected static DbgPrimWireArrow DebugDrawTransformOfFlverBonePrim;
        protected static StatusPrinter DebugDrawTransformOfFlverBoneTextDrawer;

        public List<int> UpperBodyIndices = new List<int>();

        public List<short> GetParentIndices() => Bones.Select(b => b.ParentIndex).ToList();

        public List<string> GetAncestorsBoneDescendsFrom(NewBone bone, params string[] ancestorFilter)
        {
            var result = new List<string>();
            var parent = bone.ParentIndex;
            while (parent >= 0)
            {
                var ancestorCheck = Bones[parent];
                if (ancestorFilter.Contains(ancestorCheck.Name) && !result.Contains(ancestorCheck.Name))
                    result.Add(ancestorCheck.Name);
                parent = ancestorCheck.ParentIndex;

                // If result is same length as filter, all ancestors were found
                if (result.Count == ancestorFilter.Length)
                    return result;
            }
            return result;
        }

        protected abstract bool GetGlobalEnableDrawTransforms();
        protected abstract bool GetGlobalEnableDrawLines();
        protected abstract bool GetGlobalEnableDrawBoxes();
        protected abstract bool GetGlobalEnableDrawText();
        protected abstract Color GetDrawColorBoneBoxes();
        protected abstract Color GetDrawColorBoneLines();
        protected abstract Color GetDrawColorBoneTransforms();
        protected abstract Color GetDrawColorBoneText();
        
        public bool ForceDrawBoneTransforms = false;
        public bool ForceDrawBoneLines = false;
        public bool ForceDrawBoneBoxes = false;
        public bool ForceDrawBoneText = false;



        public void DrawPrimitives(Model model)
        {
            bool globalEnableDrawTransforms = false; 
            bool globalEnableDrawLines = false;  
            bool globalEnableDrawBoxes = false;  
            bool globalEnableDrawText = false;
            
            if (GetGlobalEnableDrawTransforms() || ForceDrawBoneTransforms)
                globalEnableDrawTransforms = true;
            
            if (GetGlobalEnableDrawLines() || ForceDrawBoneLines)
                globalEnableDrawLines = true;
            
            if (GetGlobalEnableDrawBoxes() || ForceDrawBoneBoxes)
                globalEnableDrawBoxes = true;
            
            if (GetGlobalEnableDrawText() || ForceDrawBoneText)
                globalEnableDrawText = true;
            
            Color globalColorBoneBoxes = GetDrawColorBoneBoxes();
            Color globalColorBoneLines = GetDrawColorBoneLines();
            Color globalColorBoneTransforms = GetDrawColorBoneTransforms();
            Color globalColorBoneTransforms_Inactive = new Color(globalColorBoneTransforms.R,
                globalColorBoneTransforms.G, globalColorBoneTransforms.B, (byte)(255 / 2));
            Color globalColorBoneText = GetDrawColorBoneText();
            float globalTransformScale = 1.0f;
            //var camPosition = Vector3.Transform(Vector3.Zero, model.Document.WorldViewManager.CurrentView.CameraLocationInWorld.WorldMatrix);
            //var bonesInRenderOrder = Bones.OrderByDescending(bone => (Vector3.Transform(Vector3.Zero, bone.FKMatrix * model.CurrentTransform.WorldMatrix) - camPosition).LengthSquared()).ToList();

            foreach (var bone in Bones)
            {
                //if (Bones[i].IsNub)
                //    continue;
                bool enableDrawLines = bone.IsDebugDrawLines || globalEnableDrawLines;
                bool enableDrawTransforms = bone.IsDebugDrawTransforms || globalEnableDrawTransforms;
                bool enableDrawBoxes = bone.IsDebugDrawBoxes || globalEnableDrawBoxes;
                bool enableDrawText = bone.IsDebugDrawText || globalEnableDrawText;
                
                Color colorBoneBoxes = globalColorBoneBoxes;
                Color colorBoneLines = globalColorBoneLines;
                Color colorBoneTransforms = globalColorBoneTransforms;
                Color colorBoneTransforms_Inactive = globalColorBoneTransforms_Inactive;
                Color colorBoneText = globalColorBoneText;
                float transformScale = globalTransformScale;

                if (bone.IsBoneGluerDebugView)
                {
                    enableDrawTransforms = true;
                    colorBoneTransforms = Color.Red;//Main.Colors.ColorHelperBoneGlue;
                    colorBoneTransforms_Inactive = Color.Red;//Main.Colors.ColorHelperBoneGlue;
                    transformScale *= 2;
                }

                bone.DrawPrim(model.CurrentTransform.WorldMatrix, enableDrawLines, 
                    (enableDrawBoxes && DebugDrawTransformOfFlverBoneIndex < 0) || bone.Index == DebugDrawTransformOfFlverBoneIndex, colorBoneBoxes, colorBoneLines);

                if (bone.Index == DebugDrawTransformOfFlverBoneIndex || (bone.EnablePrimDraw && DebugDrawTransformOfFlverBoneIndex < 0))
                {
                    if (bone.Index == DebugDrawTransformOfFlverBoneIndex || enableDrawTransforms)
                    {
                        float scale = 0.1f * transformScale;
                        if (bone.Index == DebugDrawTransformOfFlverBoneIndex)
                            scale *= 2f;

                        DebugDrawTransformOfFlverBonePrim.EnableExtraVisibility = DBG.BoostTransformVisibility_Bones;
                        DebugDrawTransformOfFlverBonePrim.ExtraVisibilityIgnoresAlpha = DBG.BoostTransformVisibility_Bones_IgnoreAlpha;

                        DebugDrawTransformOfFlverBonePrim.Transform = new Transform(
                            DBG.NewTransformSizeMatrix_Bones *
                            Matrix.CreateScale(scale, scale, scale) *
                            Matrix.CreateRotationY(MathHelper.Pi) *
                            bone.FKMatrix);

                        if (DebugDrawTransformOfFlverBoneIndex == bone.Index)
                            DebugDrawTransformOfFlverBonePrim.OverrideColor = colorBoneTransforms;
                        else
                            DebugDrawTransformOfFlverBonePrim.OverrideColor = colorBoneTransforms_Inactive;

                        //DebugDrawTransformOfFlverBonePrim.DisableLighting = true;
                        //DebugDrawTransformOfFlverBonePrim.Wireframe = true;
                        DebugDrawTransformOfFlverBonePrim.Draw(true, null, model.CurrentTransform.WorldMatrix);
                    }



                    if (bone.Index == DebugDrawTransformOfFlverBoneIndex || enableDrawText)
                    {

                        DebugDrawTransformOfFlverBoneTextDrawer.Clear();

                        DebugDrawTransformOfFlverBoneTextDrawer.AppendLine(
                            bone.Name,
                            colorBoneText);

                        DebugDrawTransformOfFlverBoneTextDrawer.Position3D =
                            Vector3.Transform(Vector3.Zero,
                            bone.FKMatrix
                            * model.CurrentTransform.WorldMatrix);

                        //GFX.SpriteBatchBeginForText();
                        DebugDrawTransformOfFlverBoneTextDrawer.Draw(out _);
                        //GFX.SpriteBatchEnd();

                    }
                }
            }
        }
        
        protected void InitWalkTree(int i, int nestDeepness, Matrix currentFK, Matrix currentFKScale)
        {
            var transform = Bones[i].ReferenceLocalTransform;
            
            currentFK = transform.GetXnaMatrix() * currentFK;
            currentFKScale = transform.GetXnaMatrixScale() * currentFKScale;
            
            Bones[i].ReferenceFKMatrix = currentFKScale * currentFK;
            Bones[i].NestDeepness = nestDeepness;
            
            if (Bones[i].ParentIndex >= 0)
                    Bones[i].ParentReferenceFKMatrix = Bones[Bones[i].ParentIndex].ReferenceFKMatrix;
            
            foreach (var c in Bones[i].ChildIndices)
                InitWalkTree(c, nestDeepness + 1, currentFK, currentFKScale);
        }

        protected void InitBoneTree()
        {
            for (int i = 0; i < Bones.Count; i++)
            {
                //Bones[i].Index = i;

                if (Bones[i].ParentIndex < 0)
                    TopLevelBoneIndices.Add(i);

                Bones[i].Masks = NewBone.BoneMasks.None;


                {
                    var ancestors = GetAncestorsBoneDescendsFrom(Bones[i], "Upper_Root");

                    if (ancestors.Contains("Upper_Root"))
                    {
                        UpperBodyIndices.Add(i);
                        Bones[i].Masks |= NewBone.BoneMasks.UpperBody;
                    }
                }

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.SDT)
                {
                    var ancestors = GetAncestorsBoneDescendsFrom(Bones[i], "face_root");
                    if (ancestors.Contains("face_root"))
                        Bones[i].Masks |= NewBone.BoneMasks.SekiroFace;
                }

                if (Bones[i].ParentIndex >= 0)
                {
                    Bones[Bones[i].ParentIndex].ChildBones.Add(Bones[i]);
                    Bones[Bones[i].ParentIndex].ChildIndices.Add(i);
                }
            }
            
            foreach (var topBone in TopLevelBoneIndices)
                InitWalkTree(topBone, 0, Matrix.Identity, Matrix.Identity);
        }
        
        

        public int GetBoneIndexByName(string boneName)
        {
            if (boneName != null && BoneIndices_ByName.ContainsKey(boneName))
                return BoneIndices_ByName[boneName];
            else
                return -1;
        }

        public NewBone GetBoneByName(string boneName)
        {
            int boneIndex = GetBoneIndexByName(boneName);
            if (boneIndex >= 0 && boneIndex < Bones.Count)
                return Bones[boneIndex];
            return null;
        }

        private void RippleAdjustFKRecursive(int i, Matrix prevParentFK, Matrix newParentFK, bool copyToShaderMatrices)
        {
            var oldFK = Bones[i].FKMatrix;
            var relativeToParent = oldFK * Matrix.Invert(prevParentFK);
            Bones[i].FKMatrix = relativeToParent * newParentFK;
            if (copyToShaderMatrices && this is NewAnimSkeleton_FLVER asFlverSkeleton)
                asFlverSkeleton.CopyBoneToShaderMatrices(i);
            foreach (var ci in Bones[i].ChildIndices)
            {
                RippleAdjustFKRecursive(ci, oldFK, Bones[i].FKMatrix, copyToShaderMatrices);
            }
        }
        
        public void RippleAdjustFKOfBone(int i, Matrix newFK, bool copyToShaderMatrices)
        {
            var oldFK = Bones[i].FKMatrix;
            Bones[i].FKMatrix = newFK;
            if (copyToShaderMatrices && this is NewAnimSkeleton_FLVER asFlverSkeleton)
                asFlverSkeleton.CopyBoneToShaderMatrices(i);
            foreach (var ci in Bones[i].ChildIndices)
            {
                RippleAdjustFKRecursive(ci, oldFK, Bones[i].FKMatrix, copyToShaderMatrices);
            }
        }

        public void SimpleRippleAdjustFKRecursive(int i, Matrix shiftMatrix, bool copyToShaderMatrices)
        {
            Bones[i].FKMatrix *= shiftMatrix;
            if (copyToShaderMatrices && this is NewAnimSkeleton_FLVER asFlverSkeleton)
                asFlverSkeleton.CopyBoneToShaderMatrices(i);
            foreach (var ci in Bones[i].ChildIndices)
            {
                SimpleRippleAdjustFKRecursive(ci, shiftMatrix, copyToShaderMatrices);
            }
        }
        
    }
}
