using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAEDX.DebugPrimitives;

namespace TAEDX
{
    public static class TaeInterop
    {
        public static void CreateMenuBarViewportSettings(TaeEditor.TaeMenuBarBuilder menu)
        {
            menu.ClearItem("3D Preview");

            menu.AddItem("3D Preview", "Render Meshes", () => !GFX.HideFLVERs,
                b => GFX.HideFLVERs = !b);

            foreach (var model in GFX.ModelDrawer.Models)
            {
                int i = 0;
                foreach (var sm in model.GetSubmeshes())
                    menu.AddItem("3D Preview/Toggle Individual Meshes", $"{++i}: '{sm.MaterialName}'", () => sm.IsVisible, b => sm.IsVisible = b);
            }

            Dictionary<int, List<FlverSubmeshRenderer>> modelMaskMap = new Dictionary<int, List<FlverSubmeshRenderer>>();
            foreach (var model in GFX.ModelDrawer.Models)
            {
                foreach (var sm in model.GetSubmeshes())
                {
                    if (modelMaskMap.ContainsKey(sm.ModelMaskIndex))
                        modelMaskMap[sm.ModelMaskIndex].Add(sm);
                    else
                        modelMaskMap.Add(sm.ModelMaskIndex, new List<FlverSubmeshRenderer>() { sm });
                }
                
            }

            foreach (var kvp in modelMaskMap.OrderBy(asdf => asdf.Key))
            {
                menu.AddItem("3D Preview/Toggle By Model Mask", kvp.Key >= 0 ? $"Model Mask {kvp.Key}" : "Default", () => kvp.Value.All(sm => sm.IsVisible),
                    b =>
                    {
                        foreach (var sm in kvp.Value)
                        {
                            sm.IsVisible = b;
                        }
                    });
            }

            menu.AddItem("3D Preview", "Render Skeleton", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.Bone],
                b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.Bone] = b);

            menu.AddItem("3D Preview", "Render DummyPoly", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
                b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = b);

            menu.AddItem("3D Preview", "Render DummyPoly ID Tags", () => DBG.CategoryEnableDbgLabelDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
                b => DBG.CategoryEnableDbgLabelDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = b);

            Dictionary<string, List<DebugPrimitives.IDbgPrim>> dmyMap = new Dictionary<string, List<DebugPrimitives.IDbgPrim>>();
            foreach (var prim in DBG.GetPrimitives().Where(p => p.Category == DebugPrimitives.DbgPrimCategory.DummyPoly))
            {
                if (dmyMap.ContainsKey(prim.Name))
                    dmyMap[prim.Name].Add(prim);
                else
                    dmyMap.Add(prim.Name, new List<DebugPrimitives.IDbgPrim>() { prim });
            }

            foreach (var kvp in dmyMap.OrderBy(asdf => int.Parse(asdf.Key)))
            {
                menu.AddItem("3D Preview/Toggle DummyPoly By ID", $"{kvp.Key}", () => kvp.Value.Any(pr => pr.EnableDraw),
                    b =>
                    {
                        foreach (var pr in kvp.Value)
                        {
                            pr.EnableDraw = b;
                        }
                    });
            }
        }

        /// <summary>
        /// The current ANIBND path, if one is loaded.
        /// </summary>
        public static string AnibndPath => Main.TAE_EDITOR.FileContainerName;

        public static FLVER2 CurrentModel;

        /// <summary>
        /// The current event graph's playback cursor.
        /// </summary>
        public static TaeEditor.TaePlaybackCursor PlaybackCursor
            => Main.TAE_EDITOR.PlaybackCursor;

        public static byte[] CurrentSkeletonHKXBytes = null;

        /// <summary>
        /// Currently-selected animation's HKX bytes.
        /// </summary>
        public static byte[] CurrentAnimationHKXBytes = null;

        public static HKX CurrentSkeletonHKX = null;
        public static HKX CurrentAnimationHKX = null;
        public static Havok.SplineCompressedAnimation.TransformTrack[] CurrentAnimationTracks = null;
        public static short[] TransformTrackToBoneIndices = null;
        public static int CurrentAnimationFrameCount = 0;
        public static Dictionary<int, Havok.SplineCompressedAnimation.TransformTrack> BoneToTransformTrackMap;

        public static HKX.HKASkeleton HkxSkeleton;
        public static List<DbgPrimSolidBone> HkxBonePrimitives;
        public static List<Matrix> HkxBoneMatrices;
        public static List<Matrix> HkxBoneMatrices_Reference;
        public static List<Matrix> HkxBoneParentMatrices;
        public static List<Vector3> HkxBonePositions;
        public static List<Vector3> HkxBoneScales;
        public static List<Vector3> RootMotionFrames;
        public static float RootMotionDuration;

        public static Dictionary<int, int> FlverBoneToHkxBoneMap;

        /// <summary>
        /// Name of currently-selected animation.
        /// </summary>
        public static string CurrentAnimationName = null;

        /// <summary>
        /// Debug draw the havok skeleton instead of the flver skeleton
        /// </summary>
        public static bool DrawHavokSkeleton = true;

        /// <summary>
        /// Apply loaded animation to the model
        /// </summary>
        public static bool ApplyAnimation = true;

        /// <summary>
        /// The true HKX animation length from the file.
        /// Must be set otherwise the playback cursor will 
        /// just go until the end of the last event
        /// </summary>
        public static double? TrueAnimLenghForPlaybackCursor
        {
            get => PlaybackCursor.HkxAnimationLength;
            set => PlaybackCursor.HkxAnimationLength = value;
        }

        /// <summary>
        /// Dictionary of (BND file path, file bytes) for all HKX
        /// if an ANIBND is loaded.
        /// </summary>
        public static IReadOnlyDictionary<string, byte[]> AllHkxFiles =>
            Main.TAE_EDITOR.FileContainer.AllHKXDict;

        /// <summary>
        /// Rectangle of the model viewer relative to window top-left
        /// </summary>
        public static Rectangle ModelViewerWindowRect => Main.TAE_EDITOR.ModelViewerBounds;

        public static float ModelViewerAspectRatio =>
            1.0f * ModelViewerWindowRect.Width / ModelViewerWindowRect.Height;

        public static void Init()
        {
            // This allows you to use the debug menu with the gamepad for testing.
            // Final release will have no gamepad support or menu.
            //DBG.EnableGamePadInput = true;
            //DBG.EnableMenu = true;
        }

        /// <summary>
        /// Called one time when the playback cursor first hits
        /// an event's start.
        /// </summary>
        public static void PlaybackHitEventStart(TAE.Event ev)
        {
            // epic
            //if (ev.TypeName.ToUpper().Contains("SOUND"))
            //{
            //    System.Media.SystemSounds.Hand.Play();
            //}
        }

        /// <summary>
        /// Called every frame during playback while the playback
        /// cursor is within the timeframe of an event.
        /// </summary>
        public static void PlaybackDuringEventSpan(TAE.Event ev)
        {

        }

        public static void OnAnimFrameChange()
        {
            UpdateHavokBones((float)PlaybackCursor.GUICurrentTime, (float)PlaybackCursor.GUICurrentFrame);
            //foreach (var mdl in GFX.ModelDrawer.Models)
            //{
            //    mdl.ShittyTransform.Position = GetRootMotion((float)PlaybackCursor.CurrentTime);
            //}
        }

        /// <summary>
        /// Called when user selects an animation in the lists and loads the event graph for it.
        /// </summary>
        public static void OnAnimationSelected(TAE.Animation anim)
        {
            void TryToLoadAnimFile(long id)
            {
                var animID_Lower = Main.TAE_EDITOR.FileContainer.ContainerType == TaeEditor.TaeFileContainer.TaeFileContainerType.BND4
                        ? (id % 1000000) : (id % 10000);

                var animID_Upper = Main.TAE_EDITOR.FileContainer.ContainerType == TaeEditor.TaeFileContainer.TaeFileContainerType.BND4
                    ? (id / 1000000) : (id / 10000);

                string animFileName = Main.TAE_EDITOR.FileContainer.ContainerType == TaeEditor.TaeFileContainer.TaeFileContainerType.BND4
                      ? $"a{(animID_Upper):D3}_{animID_Lower:D6}" : $"a{(animID_Upper):D2}_{animID_Lower:D4}";


                CurrentAnimationName = animFileName + ".hkx";
                CurrentAnimationHKXBytes = AllHkxFiles.FirstOrDefault(x => x.Key.ToUpper().Contains(animFileName.ToUpper())).Value;
            }

            //Try to load the actual ID in the TAE Animation struct.
            TryToLoadAnimFile(anim.ID);

            //For some reference animations, we have to use the anim they are referencing
            if (CurrentAnimationHKXBytes == null)
            {
                TryToLoadAnimFile(anim.Unknown1);
            }

            if (CurrentAnimationHKXBytes == null)
            {
                TryToLoadAnimFile(anim.Unknown2);
            }

            //TAE_TODO: Read HKX bytes here.

            //TESTING
            //var testtest = HKX.Read(File.ReadAllBytes(@"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\chr\c6200-anibnd-dcx\chr\c6200\hkx\a000_000020.hkx"), HKX.HKXVariation.HKXDS1);

            CurrentAnimationHKX = HKX.Read(CurrentAnimationHKXBytes, HKX.HKXVariation.HKXDS3);

            // TEST
            HKX.HKASplineCompressedAnimation anime = null;
            HKX.HKAAnimationBinding animBinding = null;
            HKX.HKADefaultAnimatedReferenceFrame animRefFrame = null;
            foreach (var cl in CurrentAnimationHKX.DataSection.Objects)
            {
                if (cl is HKX.HKASplineCompressedAnimation asAnim)
                {
                    anime = asAnim;
                }
                else if (cl is HKX.HKAAnimationBinding asBinding)
                {
                    animBinding = asBinding;
                }
                else if (cl is HKX.HKADefaultAnimatedReferenceFrame asRefFrame)
                {
                    animRefFrame = asRefFrame;
                }
            }

            CurrentAnimationTracks = Havok.SplineCompressedAnimation.ReadSplineCompressedAnimByteBlock(false, anime.GetData(), anime.TransformTrackCount, anime.BlockCount);
            CurrentAnimationFrameCount = anime.FrameCount;
            TrueAnimLenghForPlaybackCursor = anime.Duration;

            TransformTrackToBoneIndices = new short[(int)animBinding.TransformTrackToBoneIndices.Capacity];

            BoneToTransformTrackMap = new Dictionary<int, Havok.SplineCompressedAnimation.TransformTrack>();

            for (int i = 0; i < TransformTrackToBoneIndices.Length; i++)
            {
                TransformTrackToBoneIndices[i] = animBinding.TransformTrackToBoneIndices[i].data;
                if (TransformTrackToBoneIndices[i] >= 0)
                {
                    BoneToTransformTrackMap.Add(TransformTrackToBoneIndices[i], CurrentAnimationTracks[i]);
                }
            }

            RootMotionFrames = new List<Vector3>();
            RootMotionDuration = 0;
            if (animRefFrame != null)
            {
                RootMotionDuration = animRefFrame.Duration;
                for (int i = 0; i < animRefFrame.ReferenceFrameSamples.Capacity; i++)
                {
                    var refVec4 = animRefFrame.ReferenceFrameSamples[i].Vector;
                    RootMotionFrames.Add(new Vector3(refVec4.X, refVec4.Y, refVec4.Z));
                }
            }

            HkxSkeleton = null;
            foreach (var cl in CurrentSkeletonHKX.DataSection.Objects)
            {
                if (cl is HKX.HKASkeleton)
                {
                    HkxSkeleton = (HKX.HKASkeleton)cl;
                }
            }

            FlverBoneToHkxBoneMap = new Dictionary<int, int>();
            for (int i = 0; i < HkxSkeleton.Bones.Capacity; i++)
            {
                var hkxName = HkxSkeleton.Bones[i].ToString();
                var flverBone = CurrentModel.Bones.First(b => b.Name == hkxName);
                FlverBoneToHkxBoneMap.Add(CurrentModel.Bones.IndexOf(flverBone), i);
            }

            InitHavokBones();
        }

        public static Matrix[] GetFlverShaderBoneMatrix()
        {
            var result = new Matrix[GFXShaders.FlverShader.NUM_BONES];
            result[0] = Matrix.Identity;
            for (int i = 0; i < Math.Min(CurrentModel.Bones.Count, GFXShaders.FlverShader.NUM_BONES) - 1; i++)
            {
                if (FlverBoneToHkxBoneMap.ContainsKey(i))
                {
                    var hkxBoneIndex = FlverBoneToHkxBoneMap[i];
                    result[i + 1] = Matrix.Invert(HkxBoneMatrices_Reference[hkxBoneIndex]) * HkxBoneMatrices[hkxBoneIndex];
                    //result[i] = Matrix.Identity;
                }
            }
            return result;
        }


        /// <summary>
        /// Runs once the TAE shit loads an ANIBND (doesn't run if a loose TAE is selected)
        /// Simply looks for shit named similarly to the ANIBND and loads those assets.
        /// </summary>
        public static void OnLoadANIBND()
        {
            var transform = new Transform(0, 0, 0, 0, 0, 0);
            // Attempt to load the skeleton hkx file first
            CurrentSkeletonHKXBytes = AllHkxFiles.FirstOrDefault(kvp => kvp.Key.ToUpper().Contains("SKELETON.HKX")).Value;
            CurrentSkeletonHKX = HKX.Read(CurrentSkeletonHKXBytes, HKX.HKXVariation.HKXDS3);

            var chrNameBase = Utils.GetFileNameWithoutAnyExtensions(AnibndPath);
            if (File.Exists($"{chrNameBase}.chrbnd.dcx"))
            {
                Load3DAsset($"{chrNameBase}.chrbnd.dcx", File.ReadAllBytes($"{chrNameBase}.chrbnd.dcx"), transform);
            }
            else if (File.Exists($"{chrNameBase}.chrbnd"))
            {
                Load3DAsset($"{chrNameBase}.chrbnd", File.ReadAllBytes($"{chrNameBase}.chrbnd"), transform);
            }

            if (File.Exists($"{chrNameBase}.texbnd.dcx"))
            {
                Load3DAsset($"{chrNameBase}.texbnd.dcx", File.ReadAllBytes($"{chrNameBase}.texbnd.dcx"), transform);
            }
            else if (File.Exists($"{chrNameBase}.texbnd"))
            {
                Load3DAsset($"{chrNameBase}.texbnd", File.ReadAllBytes($"{chrNameBase}.texbnd"), transform);
            }
        }

        /// <summary>
        /// Before 3D model is drawn.
        /// </summary>
        public static void TaeViewportDrawPre(GameTime gameTime)
        {
            //if (CurrentSkeletonHKX != null && CurrentAnimationHKX != null)
            //    DrawHavokBones();
        }

        /// <summary>
        /// After 3D model is drawn.
        /// </summary>
        public static void TaeViewportDrawPost(GameTime gameTime)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Previewing: {CurrentAnimationName ?? "None"}");
            if (CurrentAnimationHKXBytes != null)
                sb.AppendLine($"HKX Filesize: {(CurrentAnimationHKXBytes.Length / 1024.0f):0.00} KB");
            DBG.DrawOutlinedText(sb.ToString(), Vector2.One * 2, Color.Yellow, scale: 0.75f);
        }

        private static Havok.SplineCompressedAnimation.TransformTrack GetTransformTrackOfBone(HKX.HKASkeleton s, int boneIndex)
        {
            if (BoneToTransformTrackMap.ContainsKey(boneIndex))
                return BoneToTransformTrackMap[boneIndex];
            else
                return null;
        }

        private static Vector3 GetRootMotion(float time)
        {
            if (RootMotionFrames.Count == 0 || RootMotionDuration == 0)
                return Vector3.Zero;

            time %= RootMotionDuration;

            float sampleDuration = RootMotionDuration / RootMotionFrames.Count;
            float smoothSampleIndex = time / sampleDuration;
            float ratioBetweenSamples = smoothSampleIndex % 1;
            int sampleA = (int)Math.Floor(smoothSampleIndex);
            int sampleB = (int)Math.Ceiling(smoothSampleIndex);
            if (sampleB < RootMotionFrames.Count)
            {
                Vector3 sampleDif = RootMotionFrames[sampleB] - RootMotionFrames[sampleA];
                return RootMotionFrames[sampleA] + (sampleDif * ratioBetweenSamples);
            }
            else
            {
                return RootMotionFrames[sampleA];
            }
            
        }

        private static (Matrix, Vector3) GetBoneParentMatrixHavok(bool isJustSkeleton, HKX.HKASkeleton s, short b, float frame)
        {
            short parentBone = b;
            var result = Matrix.Identity;
            Vector3 resultScale = Vector3.One;

            do
            {
                HKX.Transform skeleTransform = s.Transforms.GetArrayData().Elements[parentBone];

                var track = GetTransformTrackOfBone(s, parentBone);

                if (isJustSkeleton || track == null)
                {
                    HKX.Transform t = skeleTransform;

                    result *= Matrix.CreateScale(t.Scale.Vector.X, t.Scale.Vector.Y, t.Scale.Vector.Z);
                    result *= Matrix.CreateFromQuaternion(new Quaternion(t.Rotation.Vector.X, t.Rotation.Vector.Y, t.Rotation.Vector.Z, t.Rotation.Vector.W));
                    result *= Matrix.CreateTranslation(t.Position.Vector.X, t.Position.Vector.Y, t.Position.Vector.Z);
                    
                }
                else
                {
                    var scaleX = track.SplineScale?.ChannelX == null ? track.StaticScale.X : track.SplineScale.GetValueX(frame);
                    var scaleY = track.SplineScale?.ChannelY == null ? track.StaticScale.Y : track.SplineScale.GetValueY(frame);
                    var scaleZ = track.SplineScale?.ChannelZ == null ? track.StaticScale.Z : track.SplineScale.GetValueZ(frame);

                    if (!track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineX) && !track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticX))
                    {
                        scaleX = skeleTransform.Scale.Vector.X;
                    }

                    if (!track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineY) && !track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticY))
                    {
                        scaleY = skeleTransform.Scale.Vector.Y;
                    }

                    if (!track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineZ) && !track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticZ))
                    {
                        scaleZ = skeleTransform.Scale.Vector.Z;
                    }

                    //result *= Matrix.CreateScale(scaleX, scaleY, scaleZ);
                    resultScale *= new Vector3(scaleX, scaleY, scaleZ);

                    if (track.HasSplineRotation)
                    {
                        //rotation = track.SplineRotation.Channel.Values[0];
                        result *= Matrix.CreateFromQuaternion(track.SplineRotation.GetValue(frame));
                        //rotation = track.SplineRotation.GetValue(0);
                    }
                    else if (track.HasStaticRotation)
                    {
                        //result *= Matrix.CreateFromQuaternion(new Quaternion(skeleTransform.Rotation.Vector.X, skeleTransform.Rotation.Vector.Y, skeleTransform.Rotation.Vector.Z, skeleTransform.Rotation.Vector.W));
                        result *= Matrix.CreateFromQuaternion(track.StaticRotation);
                    }
                    else
                    {
                        //result *= Matrix.CreateFromQuaternion(new Quaternion(skeleTransform.Rotation.Vector.X, skeleTransform.Rotation.Vector.Y, skeleTransform.Rotation.Vector.Z, skeleTransform.Rotation.Vector.W));
                    }



                    var posX = !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineX) ? (track.StaticPosition.X) : track.SplinePosition.GetValueX(frame);
                    var posY = !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineY) ? (track.StaticPosition.Y) : track.SplinePosition.GetValueY(frame);
                    var posZ = !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineZ) ? (track.StaticPosition.Z) : track.SplinePosition.GetValueZ(frame);

                    //if (!track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineX) && !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticX))
                    //{
                    //    posX = skeleTransform.Position.Vector.X;
                    //}

                    //if (!track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineY) && !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticY))
                    //{
                    //    posY = skeleTransform.Position.Vector.Y;
                    //}

                    //if (!track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineZ) && !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticZ))
                    //{
                    //    posZ = skeleTransform.Position.Vector.Z;
                    //}

                    result *= Matrix.CreateTranslation(posX, posY, posZ);



                    


                }

                if (s.ParentIndices.GetArrayData().Elements[parentBone].data >= 0)
                {
                    parentBone = s.ParentIndices.GetArrayData().Elements[parentBone].data;
                }
                else
                {
                    parentBone = -1;
                }
            }
            while (parentBone != -1);

            return (result, resultScale);
        }

        private static void InitHavokBones()
        {
            DBG.ClearPrimitives();
            HkxBoneParentMatrices = new List<Matrix>();
            HkxBonePositions = new List<Vector3>();
            HkxBoneScales = new List<Vector3>();
            HkxBonePrimitives = new List<DbgPrimSolidBone>();
            HkxBoneMatrices = new List<Matrix>();
            HkxBoneMatrices_Reference = new List<Matrix>();
            float frame = Math.Min((float)PlaybackCursor.GUICurrentFrame, CurrentAnimationFrameCount);

            for (int i = 0; i < HkxSkeleton.Transforms.Size; i++)
            {
                var parentMatrix = GetBoneParentMatrixHavok(isJustSkeleton: true, HkxSkeleton, (short)i, frame);
                HkxBoneParentMatrices.Add(parentMatrix.Item1);
                HkxBonePositions.Add(Vector3.Transform(Vector3.Zero, parentMatrix.Item1));
                HkxBoneScales.Add(parentMatrix.Item2);
            }
            //int boneIndex = 0;
            for (int i = 0; i < HkxSkeleton.Transforms.Size; i++)
            {
                if (HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data >= 0)
                {
                    if (HkxBoneParentMatrices[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data].Decompose(out Vector3 boneScale, out Quaternion boneRot, out Vector3 boneTranslation))
                    {
                        var realMatrix = HkxBoneParentMatrices[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data];//Matrix.CreateFromQuaternion(boneRot) * Matrix.CreateTranslation(HkxBonePositions[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data]);
                        var m = Matrix.CreateScale(HkxBoneScales[i]) * realMatrix;
                        HkxBoneMatrices.Add(m);
                        HkxBoneMatrices_Reference.Add(m);
                        var boneLength = (HkxBonePositions[i/*boneIndex*/] - HkxBonePositions[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data]).Length();
                        var newBonePrim = new DbgPrimSolidBone("", new Transform(realMatrix), Quaternion.Identity, Math.Min(boneLength / 8, 0.25f), boneLength, Color.Yellow);
                        DBG.AddPrimitive(newBonePrim);
                        HkxBonePrimitives.Add(newBonePrim);
                    }
                    else
                    {
                        throw new Exception("OOF");
                    }

                }
                else
                {
                    HkxBoneMatrices.Add(HkxBoneParentMatrices[i/*boneIndex*/]);
                    HkxBoneMatrices_Reference.Add(HkxBoneParentMatrices[i/*boneIndex*/]);
                    var newBonePrim = new DbgPrimSolidBone("", new Transform(HkxBoneParentMatrices[i/*boneIndex*/]), Quaternion.CreateFromYawPitchRoll(0, 0, 0), 0.15f, 0.3f, Color.Yellow);
                    DBG.AddPrimitive(newBonePrim);
                    HkxBonePrimitives.Add(newBonePrim);
                }
                //boneIndex++;
            }
        }

        private static void UpdateHavokBones(float time, float frameNum)
        {
            var rootMotion = GetRootMotion(time);
            float frame = frameNum % CurrentAnimationFrameCount;

            for (int i = 0; i < HkxSkeleton.Transforms.Size; i++)
            {
                var parentMatrix = GetBoneParentMatrixHavok(isJustSkeleton: false, HkxSkeleton, (short)i, frame);
                HkxBoneParentMatrices[i] = parentMatrix.Item1;
                HkxBonePositions[i] = Vector3.Transform(Vector3.Zero, parentMatrix.Item1);
                HkxBoneScales[i] = parentMatrix.Item2;
            }
            //int boneIndex = 0;
            for (int i = 0; i < HkxSkeleton.Transforms.Size - 1; i++)
            {
                if (i < HkxBonePrimitives.Count)
                {
                    if (HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data >= 0)
                    {
                        var realMatrix = HkxBoneParentMatrices[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data];// Matrix.CreateFromQuaternion(boneRot) * Matrix.CreateTranslation(HkxBonePositions[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data]);
                                                                                                                          //var realMatrix = HkxBoneParentMatrices[i];
                        HkxBoneMatrices[i] = Matrix.CreateScale(HkxBoneScales[i]) * realMatrix * Matrix.CreateTranslation(rootMotion);
                        //var boneLength = (HkxBonePositions[i/*boneIndex*/] - HkxBonePositions[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data]).Length();
                        //var newBonePrim = new DbgPrimSolidBone("", new Transform(realMatrix), Quaternion.Identity, Math.Min(boneLength / 8, 0.25f), boneLength, Color.Yellow);
                        //DBG.AddPrimitive(newBonePrim);
                        HkxBonePrimitives[i].Transform = new Transform(HkxBoneMatrices[i]);
                    }
                    else
                    {
                        HkxBoneMatrices[i] = Matrix.CreateScale(HkxBoneScales[i]) * HkxBoneParentMatrices[i/*boneIndex*/] * Matrix.CreateTranslation(rootMotion);
                        //var newBonePrim = new DbgPrimSolidBone("", new Transform(HkxBoneParentMatrices[i/*boneIndex*/]), Quaternion.Identity, 0.15f, 0.3f, Color.Yellow);
                        //DBG.AddPrimitive(newBonePrim);
                        //HkxBonePrimitives.Add(newBonePrim);
                        HkxBonePrimitives[i].Transform = new Transform(HkxBoneMatrices[i]);
                    }
                }

                
                //boneIndex++;
            }
        }

        private static void Load3DAsset(string assetUri, byte[] assetBytes, Transform transform)
        {
            var shortName = Path.GetFileNameWithoutExtension(assetUri);
            var upper = assetUri.ToUpper();
            if (upper.EndsWith(".BND") || upper.EndsWith(".TEXBND") || upper.EndsWith(".CHRBND") || upper.EndsWith(".OBJBND") || upper.EndsWith(".PARTSBND") ||
                upper.EndsWith(".BND.DCX") || upper.EndsWith(".TEXBND.DCX") || upper.EndsWith(".CHRBND.DCX") || upper.EndsWith(".OBJBND.DCX") || upper.EndsWith(".PARTSBND.DCX"))
            {
                if (SoulsFormats.BND3.Is(assetBytes))
                {
                    var bnd = SoulsFormats.BND3.Read(assetBytes);
                    foreach (var f in bnd.Files)
                    {
                        Load3DAsset(f.Name, f.Bytes, transform);
                    }
                }
                else if (SoulsFormats.BND4.Is(assetBytes))
                {
                    var bnd = SoulsFormats.BND4.Read(assetBytes);
                    foreach (var f in bnd.Files)
                    {
                        Load3DAsset(f.Name, f.Bytes, transform);
                    }
                }
            }
            else if (upper.EndsWith(".FLVER") || upper.EndsWith(".FLVER.DCX") || upper.EndsWith(".FLV") || upper.EndsWith(".FLV.DCX"))
            {
                if (SoulsFormats.FLVER0.Is(assetBytes))
                {
                    var flver = SoulsFormats.FLVER0.Read(assetBytes);
                    var model = new Model(flver);
                    var modelInstance = new ModelInstance(shortName, model, Transform.Default, -1, -1, -1, -1);
                    GFX.ModelDrawer.AddModelInstance(model, "", transform);
                    //throw new NotImplementedException();

                    Matrix GetBoneParentMatrix(SoulsFormats.FLVER0.Bone b)
                    {
                        SoulsFormats.FLVER0.Bone parentBone = b;

                        var result = Matrix.Identity;

                        do
                        {
                            result *= Matrix.CreateScale(parentBone.Scale.X, parentBone.Scale.Y, parentBone.Scale.Z);
                            result *= Matrix.CreateRotationX(parentBone.Rotation.X);
                            result *= Matrix.CreateRotationZ(parentBone.Rotation.Z);
                            result *= Matrix.CreateRotationY(parentBone.Rotation.Y);
                            result *= Matrix.CreateTranslation(parentBone.Translation.X, parentBone.Translation.Y, parentBone.Translation.Z);

                            if (parentBone.ParentIndex >= 0)
                            {
                                parentBone = flver.Bones[parentBone.ParentIndex];
                            }
                            else
                            {
                                parentBone = null;
                            }
                        }
                        while (parentBone != null);

                        return result;
                    }

                    foreach (var dmy in flver.Dummies)
                    {
                        DBG.AddPrimitive(new DbgPrimWireSphere(new Transform(dmy.Position.X, dmy.Position.Y, dmy.Position.Z, 0, 0, 0), 0.01f, 8, 8, Color.Cyan)
                        {
                            Name = $"{dmy.ReferenceID}",
                            Category = DbgPrimCategory.DummyPoly,
                        });

                    }

                    string getBoneSpacePrefix(SoulsFormats.FLVER0.Bone b)
                    {
                        SoulsFormats.FLVER0.Bone currentBone = b;
                        string prefix = "";
                        int parentIndex = b.ParentIndex;
                        while (parentIndex >= 0)
                        {
                            prefix += "  ";
                            currentBone = flver.Bones[parentIndex];
                            parentIndex = currentBone.ParentIndex;
                        }
                        return prefix;
                    }

                    List<Matrix> parentBoneMatrices = new List<Matrix>();
                    List<Vector3> bonePos = new List<Vector3>();

                    foreach (var b in flver.Bones)
                    {
                        var parentMatrix = GetBoneParentMatrix(b);

                        parentBoneMatrices.Add(parentMatrix);

                        bonePos.Add(Vector3.Transform(Vector3.Zero, parentMatrix));


                    }
                    int boneIndex = 0;
                    foreach (var b in flver.Bones)
                    {


                        if (b.ParentIndex >= 0)
                        {
                            if (parentBoneMatrices[b.ParentIndex].Decompose(out Vector3 boneScale, out Quaternion boneRot, out Vector3 boneTranslation))
                            {
                                var realMatrix = Matrix.CreateFromQuaternion(boneRot) * Matrix.CreateTranslation(bonePos[b.ParentIndex]);

                                if (realMatrix.Decompose(out Vector3 realBoneScale, out Quaternion realBoneRot, out Vector3 realBoneTranslation))
                                {
                                    var boneTransform = new Transform(realBoneTranslation, Vector3.Zero, realBoneScale);
                                    var boneLength = (bonePos[boneIndex] - bonePos[b.ParentIndex]).Length();
                                    DBG.AddPrimitive(new DbgPrimSolidBone(getBoneSpacePrefix(b) + b.Name, boneTransform, realBoneRot, boneLength / 8, boneLength, Color.Yellow));
                                }
                            }


                        }
                        else
                        {
                            if (parentBoneMatrices[boneIndex].Decompose(out Vector3 boneScale, out Quaternion boneRot, out Vector3 boneTranslation))
                            {
                                var boneTransform = new Transform(boneTranslation, Vector3.Zero, boneScale);
                                DBG.AddPrimitive(new DbgPrimWireBox(boneTransform, Vector3.One * 0.05f, Color.Yellow)
                                {
                                    Name = getBoneSpacePrefix(b) + b.Name,
                                    Category = DbgPrimCategory.Bone
                                });
                            }
                        }

                        boneIndex++;
                    }

                    GFX.World.ModelHeight_ForOrbitCam = model.Bounds.Max.Y;
                    GFX.World.OrbitCamReset();
                }
                else
                {
                    var flver = SoulsFormats.FLVER2.Read(assetBytes);
                    CurrentModel = flver;
                    var model = new Model(flver);
                    var modelInstance = new ModelInstance(shortName, model, Transform.Default, -1, -1, -1, -1);
                    GFX.ModelDrawer.AddModelInstance(model, "", transform);
                    //throw new NotImplementedException();

                    Matrix GetBoneParentMatrix(SoulsFormats.FLVER2.Bone b)
                    {
                        SoulsFormats.FLVER2.Bone parentBone = b;

                        var result = Matrix.Identity;

                        do
                        {
                            result *= Matrix.CreateScale(parentBone.Scale.X, parentBone.Scale.Y, parentBone.Scale.Z);
                            result *= Matrix.CreateRotationX(parentBone.Rotation.X);
                            result *= Matrix.CreateRotationZ(parentBone.Rotation.Z);
                            result *= Matrix.CreateRotationY(parentBone.Rotation.Y);
                            result *= Matrix.CreateTranslation(parentBone.Translation.X, parentBone.Translation.Y, parentBone.Translation.Z);

                            if (parentBone.ParentIndex >= 0)
                            {
                                parentBone = flver.Bones[parentBone.ParentIndex];
                            }
                            else
                            {
                                parentBone = null;
                            }
                        }
                        while (parentBone != null);

                        return result;
                    }

                    foreach (var dmy in flver.Dummies)
                    {
                        DBG.AddPrimitive(new DbgPrimWireSphere(new Transform(dmy.Position.X, dmy.Position.Y, dmy.Position.Z, 0, 0, 0), 0.01f, 8, 8, Color.Cyan)
                        {
                            Name = $"{dmy.ReferenceID}",
                            Category = DbgPrimCategory.DummyPoly,
                        });

                    }

                    string getBoneSpacePrefix(SoulsFormats.FLVER2.Bone b)
                    {
                        SoulsFormats.FLVER2.Bone currentBone = b;
                        string prefix = "";
                        int parentIndex = b.ParentIndex;
                        while (parentIndex >= 0)
                        {
                            prefix += "  ";
                            currentBone = flver.Bones[parentIndex];
                            parentIndex = currentBone.ParentIndex;
                        }
                        return prefix;
                    }

                    List<Matrix> parentBoneMatrices = new List<Matrix>();
                    List<Vector3> bonePos = new List<Vector3>();


                    foreach (var b in flver.Bones)
                    {
                        var parentMatrix = GetBoneParentMatrix(b);

                        parentBoneMatrices.Add(parentMatrix);

                        bonePos.Add(Vector3.Transform(Vector3.Zero, parentMatrix));


                    }
                    int boneIndex = 0;
                    foreach (var b in flver.Bones)
                    {


                        if (b.ParentIndex >= 0)
                        {
                            if (parentBoneMatrices[b.ParentIndex].Decompose(out Vector3 boneScale, out Quaternion boneRot, out Vector3 boneTranslation))
                            {
                                var realMatrix = Matrix.CreateFromQuaternion(boneRot) * Matrix.CreateTranslation(bonePos[b.ParentIndex]);

                                if (realMatrix.Decompose(out Vector3 realBoneScale, out Quaternion realBoneRot, out Vector3 realBoneTranslation))
                                {
                                    var boneTransform = new Transform(realBoneTranslation, Vector3.Zero, realBoneScale);
                                    var boneLength = (bonePos[boneIndex] - bonePos[b.ParentIndex]).Length();
                                    DBG.AddPrimitive(new DbgPrimSolidBone(getBoneSpacePrefix(b) + b.Name, boneTransform, realBoneRot, boneLength / 8, boneLength, Color.Yellow));
                                }
                            }


                        }
                        else
                        {
                            if (parentBoneMatrices[boneIndex].Decompose(out Vector3 boneScale, out Quaternion boneRot, out Vector3 boneTranslation))
                            {
                                var boneTransform = new Transform(boneTranslation, Vector3.Zero, boneScale);
                                DBG.AddPrimitive(new DbgPrimWireBox(boneTransform, Vector3.One * 0.05f, Color.Yellow)
                                {
                                    Name = getBoneSpacePrefix(b) + b.Name,
                                    Category = DbgPrimCategory.Bone
                                });
                            }
                        }

                        boneIndex++;
                    }
                    

                    GFX.World.ModelHeight_ForOrbitCam = model.Bounds.Max.Y;
                    GFX.World.OrbitCamReset();
                }
            }
            else if (upper.EndsWith(".TPF") || upper.EndsWith(".TPF.DCX"))
            {
                try
                {
                    TexturePool.AddTpf(SoulsFormats.TPF.Read(assetBytes));
                    GFX.ModelDrawer.RequestTextureLoad();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
