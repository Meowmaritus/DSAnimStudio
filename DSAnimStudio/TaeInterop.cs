//#define DISABLE_HKX_EXCEPTION_CATCH
//#define DISABLE_SPLINE_CACHE
using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSAnimStudio.DebugPrimitives;

namespace DSAnimStudio
{
    public static class TaeInterop
    {
        /// <summary>
        /// After 3D model is drawn.
        /// </summary>
        public static void TaeViewportDrawPost(GameTime gameTime)
        {
            if (HaventLoadedAnythingYet)
                return;

            var printer = new StatusPrinter(Vector2.One * 4);
            if (IncompatibleHavokVersion)
            {
                printer.AppendLine($"[UNSUPPORTED HAVOK VERSION]");
                printer.AppendLine($"Animation File: {CurrentAnimationName ?? "None"}");
            }
            else
            {
                if (HkxSkeleton != null)
                {
                    printer.AppendLine($"HKX Bone Count: {HkxSkeleton.Bones.Capacity} (Max Supported By Viewer: {GFXShaders.FlverShader.MAX_ALL_BONE_ARRAYS})", (HkxSkeleton.Bones.Capacity > GFXShaders.FlverShader.MAX_ALL_BONE_ARRAYS) ? Color.Red : Color.Yellow);
                }

                printer.AppendLine($"Animation File: {CurrentAnimationName ?? "None"}");
                if (CurrentAnimationHKX == null)
                {
                    printer.AppendLine($"Could not find valid HKX for this animation.", Color.Red);
                }
                else
                {
                    printer.AppendLine($"Anim BlendHint: {CurrentAnimBlendHint}", CurrentAnimBlendHint == HKX.AnimationBlendHint.NORMAL ? Color.Yellow : Color.Lime);
                    int fps = (int)Math.Round(1 / CurrentAnimationFrameDuration);
                    printer.AppendLine($"Anim Frame Rate: {fps} FPS", fps == 30 ? Color.Yellow : Color.Cyan);
                }
            }

            if (HkxAnimException != null)
            {
                var errTxt = $"HKX failed to load:\n\n{HkxAnimException}";
                printer.AppendLine(errTxt, Color.Red);
            }

            foreach (var fail in TexturePool.Failures)
            {
                printer.AppendLine($"Texture '{fail.Key.Texture.Name}' failed to load.", Color.Red);
            }

            //if (Main.TAE_EDITOR.SelectedTaeAnim != null)
            //{
            //    foreach (var eg in Main.TAE_EDITOR.SelectedTaeAnim.EventGroups)
            //    {
            //        printer.AppendLine($"Group [ID: {eg.EventType}] [Indices Count: {eg.Indices.Count}]", Color.Aqua);
            //    }
            //}

            printer.Draw();
        }

        public static string PlayerPartsModelHD = "HD_M_1900";
        public static string PlayerPartsModelBD = "BD_M_1900";
        public static string PlayerPartsModelAM = "AM_M_1900";
        public static string PlayerPartsModelLG = "LG_M_1900";

        private static int InterleavedCalculationState = 0;
        public static int InterleavedCalculationDivisor = 1;
        private static int InterleavedCalculationUpdatesPerCycle => (HkxBoneMatrices.Count / InterleavedCalculationDivisor);

        private static int InterleavedIndexRangeEndOnPreviousFrame = -1;

        private static int NumQueuedInterleavedScrubUpdateFrames = 0;

        private static (int Start, int End) IncrementAndGetNextInterleavedCalculationRange()
        {
            int startIndex = InterleavedCalculationState * InterleavedCalculationUpdatesPerCycle;

            InterleavedCalculationState++;

            if (InterleavedCalculationState >= InterleavedCalculationDivisor)
                InterleavedCalculationState = 0;

            return (startIndex, startIndex + Math.Min(HkxBoneMatrices.Count - 1 - startIndex, InterleavedCalculationUpdatesPerCycle));
        }

        private static float LastHkxFrameCalculated = -1;

        public static Vector4 CurrentRootMotionDisplacement = Vector4.Zero;
        public static Matrix CurrentRootMotionMatrix => Matrix.CreateRotationY(CurrentRootMotionDisplacement.W)
                    * Matrix.CreateTranslation(CurrentRootMotionDisplacement.XYZ());

        public static bool HaventLoadedAnythingYet = true;

        public static bool IncompatibleHavokVersion = false;
        public static HKX.HKXVariation CurrentHkxVariation = HKX.HKXVariation.HKXDS1;

        public static bool CameraFollowsRootMotion = true;

        /// <summary>
        /// The current ANIBND path, if one is loaded.
        /// </summary>
        public static string AnibndPath => Main.TAE_EDITOR.FileContainerName;

        public static FLVER2 CurrentModel;

        public static Exception HkxAnimException = null;

        public static bool IsSnapTo30FPS = false;

        public static bool ShowSFXSpawnWithCyanMarkers = true;

        public static bool PlaySoundEffectOnSoundEvents = false;
        public static bool PlaySoundEffectOnHighlightedEvents = false;
        public static bool PlaySoundEffectOnHighlightedEvents_Loop = true;

        /// <summary>
        /// The current event graph's playback cursor.
        /// </summary>
        public static TaeEditor.TaePlaybackCursor PlaybackCursor
            => Main.TAE_EDITOR?.PlaybackCursor;

        public static byte[] CurrentSkeletonHKXBytes = null;

        /// <summary>
        /// Currently-selected animation's HKX bytes.
        /// </summary>
        public static byte[] CurrentAnimationHKXBytes = null;

        public static bool EnableRootMotion = true;

        public static HKX CurrentSkeletonHKX = null;
        public static HKX CurrentAnimationHKX = null;
        public static List<Havok.SplineCompressedAnimation.TransformTrack[]> CurrentAnimationTracks = null;
        public static short[] TransformTrackToBoneIndices = null;
        public static int CurrentAnimationFrameCount = 0;
        public static float CurrentAnimationFrameDuration = 0.033333f;
        public static List<Dictionary<int, Havok.SplineCompressedAnimation.TransformTrack>> BoneToTransformTrackMap;
        public static float CurrentAnimBlockDuration = 8.5f;
        public static int CurrentAnimFramesPerBlock = 256;
        public static SoulsFormats.HKX.AnimationType CurrentAnimType = HKX.AnimationType.HK_UNKNOWN_ANIMATION;
        public static SoulsFormats.HKX.AnimationBlendHint CurrentAnimBlendHint = HKX.AnimationBlendHint.NORMAL;

        public static int CurrentBlock => PlaybackCursor != null ? (int)((PlaybackCursor.GUICurrentFrame % CurrentAnimationFrameCount) / CurrentAnimFramesPerBlock) : 0;

        public static HKX.HKASkeleton HkxSkeleton;
        public static List<DbgPrimSolidBone> HkxBonePrimitives;
        public static List<Matrix> HkxBoneMatrices;
        public static List<Matrix> HkxBoneMatrices_Reference;
        public static List<Matrix> HkxBoneParentMatrices_Reference;
        public static List<Matrix> HkxBoneParentMatrices;

        //the matrix required to go from the FLVER version of this bone to the HKX version of this bone
        public static List<Matrix> HkxBoneSkinToFlverMatrices;
        //public static List<Vector3> HkxBonePositions;
        public static List<Vector3> HkxBoneScales;
        public static List<Vector4> RootMotionFrames;
        public static float RootMotionDuration;

        public static Matrix[] ShaderMatrix0 = new Matrix[GFXShaders.FlverShader.NUM_BONES];
        public static Matrix[] ShaderMatrix1 = new Matrix[GFXShaders.FlverShader.NUM_BONES];
        public static Matrix[] ShaderMatrix2 = new Matrix[GFXShaders.FlverShader.NUM_BONES];

        public static int FlverBoneCount;

        // ! BLESSED METHOD !
        private static void CopyHavokMatrixToOtherMatrices(int havokMatrixIndex)
        {
            if (!HkxBoneToFlverBoneMap.ContainsKey(havokMatrixIndex))
                return;

            var flverBoneIndex = HkxBoneToFlverBoneMap[havokMatrixIndex];
            var matrixBank = flverBoneIndex / GFXShaders.FlverShader.NUM_BONES;
            var relativeMatrixIndex = flverBoneIndex % GFXShaders.FlverShader.NUM_BONES;

            var finalMatrix = 
                Matrix.Invert(FlverBoneTPoseMatrices[flverBoneIndex])
                * HkxBoneParentMatrices[havokMatrixIndex]
                * CurrentRootMotionMatrix;

            if (matrixBank == 0)
                ShaderMatrix0[relativeMatrixIndex] = finalMatrix;
            else if (matrixBank == 1)
                ShaderMatrix1[relativeMatrixIndex] = finalMatrix;
            else if (matrixBank == 2)
                ShaderMatrix2[relativeMatrixIndex] = finalMatrix;

            AnimatedDummyPolyClusters[flverBoneIndex]?.UpdateWithBoneMatrix(finalMatrix);

            FlverBonePrims[flverBoneIndex].Transform = new Transform(finalMatrix);
        }

        private static void RevertToTPose()
        {
            for (int i = 0; i < FlverBoneTPoseMatrices.Count; i++)
            {
                var matrixBank = i / GFXShaders.FlverShader.NUM_BONES;
                var relativeMatrixIndex = i % GFXShaders.FlverShader.NUM_BONES;

                if (matrixBank == 0)
                    ShaderMatrix0[relativeMatrixIndex] = Matrix.Identity;
                else if (matrixBank == 1)
                    ShaderMatrix1[relativeMatrixIndex] = Matrix.Identity;
                else if (matrixBank == 2)
                    ShaderMatrix2[relativeMatrixIndex] = Matrix.Identity;

                AnimatedDummyPolyClusters[i]?.UpdateWithBoneMatrix(Matrix.Identity);

                FlverBonePrims[i].Transform = new Transform(Matrix.Identity);
            }
        }

        public static List<Matrix> FlverBoneTPoseMatrices;
        public static List<IDbgPrim> FlverBonePrims;


        //public static Matrix[] FlverAnimMatrices;

        public static bool UseDummyPolyAnimation = true;

        public static DbgPrimDummyPolyCluster[] AnimatedDummyPolyClusters;

        public static Dictionary<int, int> FlverBoneToHkxBoneMap;
        public static Dictionary<int, int> HkxBoneToFlverBoneMap;

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

            DBG.PrimitiveNametagSize = 0.25f;
            
            DBG.SimpleTextLabelSize = false;

            TaeSoundManager.LoadSoundsFromDir("Content\\SE\\CHR");
        }

        /// <summary>
        /// Called one time when the playback cursor first hits
        /// an event's start.
        /// </summary>
        public static void PlaybackHitEventStart(TaeEditor.TaeEditAnimEventBox evBox)
        {
            // epic
            if (PlaySoundEffectOnSoundEvents && evBox.MyEvent.TypeName.ToUpper().Contains("SOUND"))
            {
                //DBG.SE["selected_event_hit.wav"].Play();
                //System.Media.SystemSounds.Beep.Play();
                TaeSoundManager.SoundType soundType = (TaeSoundManager.SoundType)((int)evBox.MyEvent.Parameters["SoundType"]);
                int soundID = (int)evBox.MyEvent.Parameters["SoundID"];
                if (!TaeSoundManager.Play(soundType, soundID, 0.35f))
                {
                    DBG.SE["sound_event_hit.wav"].Play();
                }
            }
            else if (PlaySoundEffectOnHighlightedEvents && (Main.TAE_EDITOR.SelectedEventBox == evBox || Main.TAE_EDITOR.MultiSelectedEventBoxes.Contains(evBox)))
            {
                DBG.SE["sound_event_hit.wav"].Play();
            }
        }

        /// <summary>
        /// Called every frame during playback while the playback
        /// cursor is within the timeframe of an event.
        /// </summary>
        public static void PlaybackDuringEventSpan(TaeEditor.TaeEditAnimEventBox evBox)
        {
            if (ShowSFXSpawnWithCyanMarkers && evBox.MyEvent.Template != null)
            {
                //foreach (var key in evBox.MyEvent.Parameters.Template.Keys)
                //{
                //    if (key.StartsWith("DummyPolyID"))
                //    {
                //        var dummyPolyID = Convert.ToInt32(evBox.MyEvent.Parameters[key]);
                //        foreach (var dmy in AnimatedDummies.Values)
                //        {
                //            if (dmy.DummyPoly.ReferenceID == dummyPolyID)
                //            {
                //                dmy.HelperSize = 2;
                //            }
                //        }
                //    }
                //}

                //throw new NotImplementedException();
            }

            if (PlaySoundEffectOnHighlightedEvents_Loop && (Main.TAE_EDITOR.SelectedEventBox == evBox || Main.TAE_EDITOR.MultiSelectedEventBoxes.Contains(evBox)))
            {
                DBG.BeepVolume = 1.0f;
            }
        }

        public static void OnAnimFrameChange(bool isScrubbing)
        {
            if (IncompatibleHavokVersion)
            {
                CurrentSkeletonHKX = null;
                CurrentSkeletonHKXBytes = null;
                CurrentAnimationHKX = null;
                CurrentAnimationHKXBytes = null;
                return;
            }

            if (InterleavedCalculationDivisor > 1 && isScrubbing)
                NumQueuedInterleavedScrubUpdateFrames = InterleavedCalculationDivisor;

            if (CurrentAnimationHKX != null)
            {
                if (!isScrubbing || (InterleavedCalculationDivisor == 1) || NumQueuedInterleavedScrubUpdateFrames > 0)
                {
                    CalculateAnimation((float)PlaybackCursor.GUICurrentTime, (float)PlaybackCursor.GUICurrentFrame);

                    if (NumQueuedInterleavedScrubUpdateFrames > 0)
                        NumQueuedInterleavedScrubUpdateFrames--;
                }

                
                //UpdateFlverMatrices();

                //foreach (var mdl in GFX.ModelDrawer.Models)
                //{
                //    mdl.ShittyTransform.Position = CurrentRootMotionDisplacement.XYZ();
                //    mdl.ShittyTransform.EulerRotation.Y = CurrentRootMotionDisplacement.W;
                //}

                //if (UseDummyPolyAnimation && DBG.CategoryEnableDraw[DbgPrimCategory.DummyPoly])
                //    UpdateDummies();
            }
                
            

            
        }


        /// <summary>
        /// Runs once the TAE shit loads an ANIBND (doesn't run if a loose TAE is selected)
        /// Simply looks for shit named similarly to the ANIBND and loads those assets.
        /// </summary>
        public static void OnLoadANIBND(TaeEditor.TaeMenuBarBuilder menuBar)
        {
            GFX.ModelDrawer.ClearScene();
            DBG.ClearPrimitives(DebugPrimitives.DbgPrimCategory.HkxBone);
            DBG.ClearPrimitives(DebugPrimitives.DbgPrimCategory.DummyPoly);
            GFX.HideFLVERs = false;
            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.HkxBone] = false;
            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone] = false;
            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox] = false;
            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = true;
            DBG.CategoryEnableDbgLabelDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = true;

            if (HaventLoadedAnythingYet)
                HaventLoadedAnythingYet = false;

            if (IncompatibleHavokVersion)
            {
                CurrentSkeletonHKX = null;
                CurrentSkeletonHKXBytes = null;
                CurrentAnimationHKX = null;
                CurrentAnimationHKXBytes = null;
                TrueAnimLenghForPlaybackCursor = null;
                RevertToTPose();
                return;
            }

            var transform = new Transform(0, 0, 0, 0, 0, 0);

            var chrNameBase = Utils.GetFileNameWithoutAnyExtensions(AnibndPath);

            if (chrNameBase.EndsWith("c0000"))
            {
                var folder = new FileInfo(AnibndPath).DirectoryName;

                var lastSlashInFolder = folder.LastIndexOf("\\");

                var interrootFolder = folder.Substring(0, lastSlashInFolder);

                FLVER2 c0000 = null;

                string c0000Path = $"{chrNameBase}.chrbnd";

                if (File.Exists(c0000Path + ".dcx"))
                {
                    c0000Path = c0000Path + ".dcx";
                }

                if (BND4.Is(c0000Path))
                {
                    var bnd = BND4.Read(c0000Path);

                    foreach (var f in bnd.Files)
                    {
                        if (FLVER2.Is(f.Bytes))
                        {
                            c0000 = FLVER2.Read(f.Bytes);
                            break;
                        }
                    }
                }
                else if (BND3.Is(c0000Path))
                {
                    var bnd = BND3.Read(c0000Path);

                    foreach (var f in bnd.Files)
                    {
                        if (FLVER2.Is(f.Bytes))
                        {
                            c0000 = FLVER2.Read(f.Bytes);
                            break;
                        }
                    }
                }

                //load it

                string partA = $"{interrootFolder}\\parts\\{PlayerPartsModelHD}.partsbnd";
                string partB = $"{interrootFolder}\\parts\\{PlayerPartsModelBD}.partsbnd";
                string partC = $"{interrootFolder}\\parts\\{PlayerPartsModelAM}.partsbnd";
                string partD = $"{interrootFolder}\\parts\\{PlayerPartsModelLG}.partsbnd";

                if (File.Exists(partA + ".dcx"))
                    partA = partA + ".dcx";

                if (File.Exists(partB + ".dcx"))
                    partB = partB + ".dcx";

                if (File.Exists(partC + ".dcx"))
                    partC = partC + ".dcx";

                if (File.Exists(partD + ".dcx"))
                    partD = partD + ".dcx";

                var parts = LoadFourPartsFiles(c0000, partA, partB, partC, partD);

                LoadFLVER(parts.Item1);

                foreach (var tpf in parts.Item2)
                {
                    TexturePool.AddTpf(tpf);
                }

                GFX.ModelDrawer.RequestTextureLoad();
            }
            else
            {
                if (File.Exists($"{chrNameBase}.chrbnd.dcx"))
                {
                    Load3DAsset($"{chrNameBase}.chrbnd.dcx", File.ReadAllBytes($"{chrNameBase}.chrbnd.dcx"), transform);
                }
                else if (File.Exists($"{chrNameBase}.chrbnd"))
                {
                    Load3DAsset($"{chrNameBase}.chrbnd", File.ReadAllBytes($"{chrNameBase}.chrbnd"), transform);
                }
            }

            

            if (File.Exists($"{chrNameBase}.texbnd.dcx"))
            {
                Load3DAsset($"{chrNameBase}.texbnd.dcx", File.ReadAllBytes($"{chrNameBase}.texbnd.dcx"), transform);
            }
            else if (File.Exists($"{chrNameBase}.texbnd"))
            {
                Load3DAsset($"{chrNameBase}.texbnd", File.ReadAllBytes($"{chrNameBase}.texbnd"), transform);
            }

            string possibleSharedTexPack = chrNameBase.Substring(0, chrNameBase.Length - 1) + "9";

            if (File.Exists($"{possibleSharedTexPack}.chrbnd.dcx"))
            {
                Load3DAsset($"{possibleSharedTexPack}.chrbnd.dcx", File.ReadAllBytes($"{possibleSharedTexPack}.chrbnd.dcx"), transform, dontLoadModels: true);
            }
            else if (File.Exists($"{possibleSharedTexPack}.chrbnd"))
            {
                Load3DAsset($"{possibleSharedTexPack}.chrbnd", File.ReadAllBytes($"{possibleSharedTexPack}.chrbnd"), transform, dontLoadModels: true);
            }

            if (File.Exists($"{possibleSharedTexPack}.texbnd.dcx"))
            {
                Load3DAsset($"{possibleSharedTexPack}.texbnd.dcx", File.ReadAllBytes($"{possibleSharedTexPack}.texbnd.dcx"), transform);
            }
            else if (File.Exists($"{possibleSharedTexPack}.texbnd"))
            {
                Load3DAsset($"{possibleSharedTexPack}.texbnd", File.ReadAllBytes($"{possibleSharedTexPack}.texbnd"), transform);
            }

            string extraBloodborneTextures = $"{chrNameBase}_2";

            if (File.Exists($"{extraBloodborneTextures}.tpf.dcx"))
            {
                Load3DAsset($"{extraBloodborneTextures}.tpf.dcx", File.ReadAllBytes($"{extraBloodborneTextures}.tpf.dcx"), transform);
            }
            else if (File.Exists($"{extraBloodborneTextures}.tpf"))
            {
                Load3DAsset($"{extraBloodborneTextures}.tpf", File.ReadAllBytes($"{extraBloodborneTextures}.ypf"), transform);
            }

            if (Directory.Exists($"{chrNameBase}"))
            {
                TexturePool.AddTPFFolder($"{chrNameBase}");
                GFX.ModelDrawer.RequestTextureLoad();
            }

            // Attempt to load the skeleton hkx file first
            CurrentSkeletonHKXBytes = AllHkxFiles.FirstOrDefault(kvp => kvp.Key.ToUpper().Contains("SKELETON.HKX")).Value;
            CurrentSkeletonHKX = HKX.Read(CurrentSkeletonHKXBytes, CurrentHkxVariation);

            HkxSkeleton = null;
            foreach (var cl in CurrentSkeletonHKX.DataSection.Objects)
            {
                if (cl is HKX.HKASkeleton)
                {
                    HkxSkeleton = (HKX.HKASkeleton)cl;
                }
            }

            

            FlverBoneToHkxBoneMap = new Dictionary<int, int>();
            HkxBoneToFlverBoneMap = new Dictionary<int, int>();
            for (int i = 0; i < HkxSkeleton.Bones.Capacity; i++)
            {
                var hkxName = HkxSkeleton.Bones[i].ToString();
                var flverBone = CurrentModel.Bones.LastOrDefault(b => b.Name == hkxName);
                if (flverBone == null)
                {
                    Console.WriteLine($"FLVER did not have bone '{hkxName}' but HKX did;");
                }
                //else if (hkxName.EndsWith("Nub"))
                //{
                //    Console.WriteLine($"DEBUG: Ignoring nub '{hkxName}'...");
                //}
                else
                {
                    FlverBoneToHkxBoneMap.Add(CurrentModel.Bones.IndexOf(flverBone), i);
                    HkxBoneToFlverBoneMap.Add(i, CurrentModel.Bones.IndexOf(flverBone));
                }
            }

            InitHavokBones();

            var model = new Model(CurrentModel, new Dictionary<int, Matrix>());
            var modelInstance = new ModelInstance("Character Model", model, Transform.Default, -1, -1, -1, -1);
            GFX.ModelDrawer.AddModelInstance(model, "", Transform.Default);
            GFX.World.ModelHeight_ForOrbitCam = model.Bounds.Max.Y;
            GFX.World.OrbitCamReset();

            CreateMenuBarViewportSettings(menuBar);
        }

        /// <summary>
        /// Called when user selects an animation in the lists and loads the event graph for it.
        /// </summary>
        public static void OnAnimationSelected(TAE.Animation anim)
        {
            if (HaventLoadedAnythingYet)
                HaventLoadedAnythingYet = false;

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

            if (IncompatibleHavokVersion)
            {
                CurrentSkeletonHKX = null;
                CurrentSkeletonHKXBytes = null;
                CurrentAnimationHKX = null;
                CurrentAnimationHKXBytes = null;
                TrueAnimLenghForPlaybackCursor = null;
                RevertToTPose();
                return;
            }

            //Try to load the actual ID in the TAE Animation struct.
            TryToLoadAnimFile(anim.ID);

            //For some reference animations, we have to use the anim they are referencing
            if (CurrentAnimationHKXBytes == null && anim.Unknown1 > 0)
            {
                TryToLoadAnimFile(anim.Unknown1);
            }

            if (CurrentAnimationHKXBytes == null && anim.Unknown2 > 0)
            {
                TryToLoadAnimFile(anim.Unknown2);
            }

            // If STILL NULL just give up :MecHands:
            if (CurrentAnimationHKXBytes == null)
            {
                CurrentAnimationName = null;
                CurrentAnimationHKX = null;
                TrueAnimLenghForPlaybackCursor = null;
                return;
            }

            //TAE_TODO: Read HKX bytes here.

            //TESTING
            //var testtest = HKX.Read(File.ReadAllBytes(@"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\chr\c6200-anibnd-dcx\chr\c6200\hkx\a000_000020.hkx"), HKX.HKXVariation.HKXDS1);
#if !DISABLE_HKX_EXCEPTION_CATCH
            try
            {
#endif
                CurrentAnimationHKX = HKX.Read(CurrentAnimationHKXBytes, CurrentHkxVariation);

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
                CurrentAnimationFrameDuration = anime.FrameDuration;
                TrueAnimLenghForPlaybackCursor = anime.Duration;

                TransformTrackToBoneIndices = new short[(int)animBinding.TransformTrackToBoneIndices.Capacity];

                BoneToTransformTrackMap = new List<Dictionary<int, Havok.SplineCompressedAnimation.TransformTrack>>();
                

                CurrentAnimBlockDuration = anime.BlockDuration;
                CurrentAnimFramesPerBlock = anime.FramesPerBlock;
                CurrentAnimType = anime.AnimationType;
                CurrentAnimBlendHint = animBinding.BlendHint;

                for (int b = 0; b < anime.BlockCount; b++)
                {
                    BoneToTransformTrackMap.Add(new Dictionary<int, Havok.SplineCompressedAnimation.TransformTrack>());
                    for (int i = 0; i < TransformTrackToBoneIndices.Length; i++)
                    {
                        TransformTrackToBoneIndices[i] = animBinding.TransformTrackToBoneIndices[i].data;
                        if (TransformTrackToBoneIndices[i] >= 0)
                        {
                            BoneToTransformTrackMap[b].Add(TransformTrackToBoneIndices[i], CurrentAnimationTracks[b][i]);
                        }
                    }
                }

                RootMotionFrames = new List<Vector4>();
                RootMotionDuration = 0;
                if (animRefFrame != null)
                {
                    RootMotionDuration = animRefFrame.Duration;
                    for (int i = 0; i < animRefFrame.ReferenceFrameSamples.Capacity; i++)
                    {
                        var refVec4 = animRefFrame.ReferenceFrameSamples[i].Vector;
                        RootMotionFrames.Add(new Vector4(refVec4.X, refVec4.Y, refVec4.Z, refVec4.W));
                    }
                }

                HkxAnimException = null;
#if !DISABLE_HKX_EXCEPTION_CATCH
            }
            catch (Exception ex)
            {
                CurrentAnimationHKX = null;
                HkxAnimException = ex;
            }
#endif


        }

        //public static void UpdateFlverMatrices()
        //{
        //    for (int i = 0; i < FlverAnimMatrices.Length; i++)
        //    {
        //        if (FlverBoneToHkxBoneMap.ContainsKey(i))
        //        {
        //            int hkxBoneIndex = FlverBoneToHkxBoneMap[i];
        //            FlverAnimMatrices[i] =
        //                // Matrix.Invert(HkxBoneParentMatrices_Reference[hkxBoneIndex])
        //                Matrix.Invert(FlverBoneTPoseMatrices[i])
        //                * HkxBoneParentMatrices[hkxBoneIndex] 
        //                * CurrentRootMotionMatrix;



        //            FlverBonePrims[i].Transform = new Transform(FlverAnimMatrices[i]);
        //            foreach (var c in FlverBonePrims[i].Children)
        //            {
        //                c.Transform = HkxBonePrimitives[hkxBoneIndex].Transform;
        //            }
        //        }
        //        else
        //        {
        //            FlverAnimMatrices[i] = Matrix.Identity * CurrentRootMotionMatrix;

        //            FlverBonePrims[i].Transform = new Transform(FlverBoneTPoseMatrices[i] * CurrentRootMotionMatrix);
        //            foreach (var c in FlverBonePrims[i].Children)
        //            {
        //                c.Transform = new Transform(FlverBoneTPoseMatrices[i] * CurrentRootMotionMatrix);
        //            }
        //        }

                
        //    }
        //}

        //public static void UpdateDummies()
        //{
        //    foreach (var dmy in AnimatedDummies)
        //    {
        //        if (dmy.DummyPoly.AttachBoneIndex >= 0)
        //        {
        //            dmy.Transform = new Transform(dmy.DummyPolyMatrix
        //            * FlverAnimMatrices[dmy.DummyPoly.AttachBoneIndex]);
        //        }
        //        else
        //        {
        //            dmy.Transform = new Transform(dmy.DummyPolyMatrix);
        //        }
        //    }
        //}

        //public static Matrix[] GetFlverShaderBoneMatrix(int bank)
        //{
        //    var result = new Matrix[GFXShaders.FlverShader.NUM_BONES];
        //    //result[0] = Matrix.Identity;
        //    for (int i = 0; i < Math.Min((CurrentModel.Bones.Count - (bank * GFXShaders.FlverShader.NUM_BONES)), GFXShaders.FlverShader.NUM_BONES); i++)
        //    {
        //        result[i] = FlverAnimMatrices?[i + (bank * GFXShaders.FlverShader.NUM_BONES)] ?? Matrix.Identity;
        //    }
        //    return result;
        //}

        /// <summary>
        /// Before 3D model is drawn.
        /// </summary>
        public static void TaeViewportDrawPre(GameTime gameTime)
        {
            //if (CurrentSkeletonHKX != null && CurrentAnimationHKX != null)
            //    DrawHavokBones();
        }

        private static Havok.SplineCompressedAnimation.TransformTrack GetTransformTrackOfBone(HKX.HKASkeleton s, int boneIndex)
        {
            if (BoneToTransformTrackMap[CurrentBlock].ContainsKey(boneIndex))
                return BoneToTransformTrackMap[CurrentBlock][boneIndex];
            else
                return null;
        }

        private static (Matrix, Vector3) GetBoneParentMatrixHavok(bool isJustSkeleton, 
            HKX.HKASkeleton s, short b, float frame, 
            Dictionary<int, (Matrix, Vector3)> alreadyCalculatedBones, 
            bool additiveBlend = false)
        {
            short parentBone = b;
            var result = Matrix.Identity;
            Vector3 resultScale = Vector3.One;

            do
            {
                (Matrix, Vector3) thisBone = (Matrix.Identity, Vector3.One);

#if !DISABLE_SPLINE_CACHE
                if (alreadyCalculatedBones.ContainsKey(parentBone))
                {
                    thisBone.Item1 *= alreadyCalculatedBones[parentBone].Item1;
                    thisBone.Item2 *= alreadyCalculatedBones[parentBone].Item2;
                }
                else
                {
#endif
                    HKX.Transform skeleTransform = s.Transforms.GetArrayData().Elements[parentBone];

                    var track = !isJustSkeleton ? GetTransformTrackOfBone(s, parentBone) : null;

                    if (isJustSkeleton || track == null)
                    {
                        HKX.Transform t = skeleTransform;

                        thisBone.Item1 *= Matrix.CreateScale(
                            t.Scale.Vector.X, t.Scale.Vector.Y, t.Scale.Vector.Z);
                        thisBone.Item1 *= Matrix.CreateFromQuaternion(new Quaternion(
                            t.Rotation.Vector.X, 
                            t.Rotation.Vector.Y,
                            t.Rotation.Vector.Z,
                            t.Rotation.Vector.W));
                        thisBone.Item1 *= Matrix.CreateTranslation(
                            t.Position.Vector.X, 
                            t.Position.Vector.Y, 
                            t.Position.Vector.Z);

                        //if (parentBone < HkxBoneMatrices_Reference.Count)
                        //{
                        //    thisBone.Item1 *= HkxBoneMatrices_Reference[parentBone];
                        //}
                        //else
                        //{
                        //    HKX.Transform t = skeleTransform;

                        //    thisBone.Item1 *= Matrix.CreateScale(t.Scale.Vector.X, t.Scale.Vector.Y, t.Scale.Vector.Z);
                        //    thisBone.Item1 *= Matrix.CreateFromQuaternion(new Quaternion(t.Rotation.Vector.X, t.Rotation.Vector.Y, t.Rotation.Vector.Z, t.Rotation.Vector.W));
                        //    thisBone.Item1 *= Matrix.CreateTranslation(t.Position.Vector.X, t.Position.Vector.Y, t.Position.Vector.Z);
                        //}
                    }
                    else
                    {
                        var scaleX = track.SplineScale?.ChannelX == null 
                            ? track.StaticScale.X : track.SplineScale.GetValueX(frame);
                        var scaleY = track.SplineScale?.ChannelY == null 
                            ? track.StaticScale.Y : track.SplineScale.GetValueY(frame);
                        var scaleZ = track.SplineScale?.ChannelZ == null 
                            ? track.StaticScale.Z : track.SplineScale.GetValueZ(frame);

                        if (!track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineX) && 
                            !track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticX))
                        {
                            scaleX = skeleTransform.Scale.Vector.X;
                        }

                        if (!track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineY) && 
                            !track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticY))
                        {
                            scaleY = skeleTransform.Scale.Vector.Y;
                        }

                        if (!track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineZ) && 
                            !track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticZ))
                        {
                            scaleZ = skeleTransform.Scale.Vector.Z;
                        }

                        if (additiveBlend)
                        {
                            scaleX *= skeleTransform.Scale.Vector.X;
                            scaleY *= skeleTransform.Scale.Vector.Y;
                            scaleZ *= skeleTransform.Scale.Vector.Z;
                        }

                        //result *= Matrix.CreateScale(scaleX, scaleY, scaleZ);
                        thisBone.Item2 *= new Vector3(scaleX, scaleY, scaleZ);

                        Quaternion rotation = Quaternion.Identity;

                        if (track.HasSplineRotation)
                        {
                            rotation = track.SplineRotation.GetValue(frame);
                            //rotation = track.SplineRotation.Channel.Values[0];
                            //rotation = track.SplineRotation.GetValue(0);
                        }
                        else if (track.HasStaticRotation)
                        {
                            rotation = track.StaticRotation;
                            //result *= Matrix.CreateFromQuaternion(new Quaternion(skeleTransform.Rotation.Vector.X, skeleTransform.Rotation.Vector.Y, skeleTransform.Rotation.Vector.Z, skeleTransform.Rotation.Vector.W));
                        }
                        else
                        {
                            //result *= Matrix.CreateFromQuaternion(new Quaternion(skeleTransform.Rotation.Vector.X, skeleTransform.Rotation.Vector.Y, skeleTransform.Rotation.Vector.Z, skeleTransform.Rotation.Vector.W));
                        }

                        if (additiveBlend)
                        {
                            rotation *= new Quaternion(
                                skeleTransform.Rotation.Vector.X, 
                                skeleTransform.Rotation.Vector.Y, 
                                skeleTransform.Rotation.Vector.Z, 
                                skeleTransform.Rotation.Vector.W);
                        }

                        thisBone.Item1 *= Matrix.CreateFromQuaternion(rotation);

                        var posX = !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineX) 
                            ? (track.StaticPosition.X) : track.SplinePosition.GetValueX(frame);
                        var posY = !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineY) 
                            ? (track.StaticPosition.Y) : track.SplinePosition.GetValueY(frame);
                        var posZ = !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineZ) 
                            ? (track.StaticPosition.Z) : track.SplinePosition.GetValueZ(frame);

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

                        if (additiveBlend)
                        {
                            posX += skeleTransform.Position.Vector.X;
                            posY += skeleTransform.Position.Vector.Y;
                            posZ += skeleTransform.Position.Vector.Z;
                        }

                        thisBone.Item1 *= Matrix.CreateTranslation(posX, posY, posZ);
#if !DISABLE_SPLINE_CACHE
                }
                    alreadyCalculatedBones.Add(parentBone, thisBone);
#endif




                }

                result *= thisBone.Item1;
                resultScale *= thisBone.Item2;

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

        public static void InitHavokBones()
        {
            DBG.ClearPrimitives(DbgPrimCategory.HkxBone);
            HkxBoneParentMatrices = new List<Matrix>();
            var HkxBonePositions = new List<Vector3>();
            HkxBoneScales = new List<Vector3>();
            HkxBonePrimitives = new List<DbgPrimSolidBone>();
            HkxBoneMatrices = new List<Matrix>();
            HkxBoneMatrices_Reference = new List<Matrix>();
            HkxBoneParentMatrices_Reference = new List<Matrix>();
            float frame = PlaybackCursor != null ? Math.Min((float)PlaybackCursor.GUICurrentFrame, CurrentAnimationFrameCount) : 0;

            var alreadyCalculatedBones = new Dictionary<int, (Matrix, Vector3)>();

            for (int i = 0; i < HkxSkeleton.Transforms.Size; i++)
            {
                var parentMatrix = GetBoneParentMatrixHavok(isJustSkeleton: true, HkxSkeleton, (short)i, frame % CurrentAnimFramesPerBlock, alreadyCalculatedBones);
                HkxBoneParentMatrices.Add(parentMatrix.Item1);
                HkxBoneParentMatrices_Reference.Add(parentMatrix.Item1);
                HkxBonePositions.Add(Vector3.Transform(Vector3.Zero, parentMatrix.Item1));
                HkxBoneScales.Add(parentMatrix.Item2);
            }
            //int boneIndex = 0;
            for (int i = 0; i < HkxSkeleton.Transforms.Size; i++)
            {
                if (HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data >= 0)
                {
                    var realMatrix = HkxBoneParentMatrices[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data];//Matrix.CreateFromQuaternion(boneRot) * Matrix.CreateTranslation(HkxBonePositions[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data]);
                    var m = Matrix.CreateScale(HkxBoneScales[i]) * realMatrix;
                    HkxBoneMatrices.Add(m);
                    HkxBoneMatrices_Reference.Add(m);
                    var boneLength = (HkxBonePositions[i/*boneIndex*/] - HkxBonePositions[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data]).Length();
                    var newBonePrim = new DbgPrimSolidBone(isHkx: true, HkxSkeleton.Bones[i].Name.GetString(), new Transform(realMatrix), Quaternion.Identity, Math.Min(boneLength / 8, 0.25f), boneLength, Color.Yellow);
                    DBG.AddPrimitive(newBonePrim);
                    HkxBonePrimitives.Add(newBonePrim);

                }
                else
                {
                    HkxBoneMatrices.Add(HkxBoneParentMatrices[i/*boneIndex*/]);
                    HkxBoneMatrices_Reference.Add(HkxBoneParentMatrices[i/*boneIndex*/]);
                    var newBonePrim = new DbgPrimSolidBone(isHkx: true, HkxSkeleton.Bones[i].Name.GetString(), new Transform(HkxBoneParentMatrices[i/*boneIndex*/]), Quaternion.CreateFromYawPitchRoll(0, 0, 0), 0.15f, 0.3f, Color.Yellow);
                    DBG.AddPrimitive(newBonePrim);
                    HkxBonePrimitives.Add(newBonePrim);
                }
                //boneIndex++;

                CopyHavokMatrixToOtherMatrices(i);
            }
        }

        private static void CalculateAnimation(float totalTime, float frameNum)
        {
            float frame = frameNum % CurrentAnimationFrameCount;

            if (frame != LastHkxFrameCalculated)
            {
                if (!(RootMotionFrames.Count == 0 || RootMotionDuration == 0 || !EnableRootMotion))
                {
                    float rootMotionTime = totalTime % RootMotionDuration;
                    float sampleDuration = RootMotionDuration / RootMotionFrames.Count;
                    float smoothSampleIndex = rootMotionTime / sampleDuration;
                    float ratioBetweenSamples = smoothSampleIndex % 1;
                    int sampleA = (int)Math.Floor(smoothSampleIndex);
                    int sampleB = (int)Math.Ceiling(smoothSampleIndex);
                    if (sampleB < RootMotionFrames.Count)
                    {
                        Vector4 sampleDif = RootMotionFrames[sampleB] - RootMotionFrames[sampleA];
                        CurrentRootMotionDisplacement = RootMotionFrames[sampleA] + (sampleDif * ratioBetweenSamples);
                    }
                    else if (sampleA < RootMotionFrames.Count)
                    {
                        CurrentRootMotionDisplacement = RootMotionFrames[sampleA];
                    }
                    else
                    {
                        CurrentRootMotionDisplacement = RootMotionFrames[RootMotionFrames.Count - 1];
                    }
                }
                else
                {
                    CurrentRootMotionDisplacement = Vector4.Zero;
                }
            }

            var rootMotion = CurrentRootMotionDisplacement;

            (int Start, int End) thisFrameRange = (0, HkxBoneMatrices.Count - 1);

            //if (InterleavedCalculation)
            //{
            //    if (frame != LastHkxFrameCalculated && InterleavedCalculationState == -1)
            //    {
            //        InterleavedCalculationState = 0;
            //    }
            //}

            if (InterleavedCalculationDivisor > 1)
            {
                thisFrameRange = IncrementAndGetNextInterleavedCalculationRange();

                if (thisFrameRange.Start > InterleavedIndexRangeEndOnPreviousFrame)
                    thisFrameRange.Start = InterleavedIndexRangeEndOnPreviousFrame + 1;

                // Be sure to update that last bone or 2 after rounding :fatcat:
                if (InterleavedCalculationState == (InterleavedCalculationDivisor - 1))
                {
                    thisFrameRange.End = (HkxBoneMatrices.Count - 1);
                }
            }

            if ((InterleavedCalculationDivisor == 1) || (InterleavedCalculationState >= 0))
            {
                (Matrix, Vector3) parentMatrix = (Matrix.Identity, Vector3.One);

                var alreadyCalculatedBones = new Dictionary<int, (Matrix, Vector3)>();

                for (int i = thisFrameRange.Start; i <= thisFrameRange.End; i++)
                {
                    parentMatrix = GetBoneParentMatrixHavok(isJustSkeleton: false,
                        HkxSkeleton, (short)i, frame % CurrentAnimFramesPerBlock,
                        alreadyCalculatedBones,
                        additiveBlend: CurrentAnimBlendHint == HKX.AnimationBlendHint.ADDITIVE ||
                                CurrentAnimBlendHint == HKX.AnimationBlendHint.ADDITIVE_DEPRECATED);
                    HkxBoneParentMatrices[i] = parentMatrix.Item1;
                    //HkxBonePositions[i] = Vector3.Transform(Vector3.Zero, parentMatrix.Item1);
                    HkxBoneScales[i] = parentMatrix.Item2;

                    //if (CurrentAnimBlendHint == HKX.AnimationBlendHint.ADDITIVE ||
                    //            CurrentAnimBlendHint == HKX.AnimationBlendHint.ADDITIVE_DEPRECATED)
                    //{


                    //    //Matrix tposeMatrix = Matrix.Identity;
                    //    //if (TransformTrackToBoneIndices[i] >= 0)
                    //    //{
                    //    //    HKX.Transform t = HkxSkeleton.Transforms.GetArrayData().Elements[TransformTrackToBoneIndices[i]];

                    //    //    tposeMatrix *= Matrix.CreateScale(t.Scale.Vector.X, t.Scale.Vector.Y, t.Scale.Vector.Z);
                    //    //    tposeMatrix *= Matrix.CreateFromQuaternion(new Quaternion(t.Rotation.Vector.X, t.Rotation.Vector.Y, t.Rotation.Vector.Z, t.Rotation.Vector.W));
                    //    //    tposeMatrix *= Matrix.CreateTranslation(t.Position.Vector.X, t.Position.Vector.Y, t.Position.Vector.Z);

                    //    //}

                    //    HkxBoneParentMatrices[i] = HkxBoneParentMatrices_Reference[i] + HkxBoneParentMatrices[i];
                    //}

                    if (i < HkxBonePrimitives.Count)
                    {
                        var parentIndex = HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data;
                        if (parentIndex >= 0)
                        {
                            var realMatrix = HkxBoneParentMatrices[parentIndex];// Matrix.CreateFromQuaternion(boneRot) * Matrix.CreateTranslation(HkxBonePositions[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data]);
                                                                                                                              //var realMatrix = HkxBoneParentMatrices[i];
                            HkxBoneMatrices[i] = 
                                Matrix.CreateScale(HkxBoneScales[i])
                                * realMatrix
                                * CurrentRootMotionMatrix;

                            //if (CurrentAnimBlendHint == HKX.AnimationBlendHint.ADDITIVE ||
                            //    CurrentAnimBlendHint == HKX.AnimationBlendHint.ADDITIVE_DEPRECATED)
                            //{
                            //    Matrix tposeMatrix = Matrix.Identity;
                            //    if (TransformTrackToBoneIndices[parentIndex] >= 0)
                            //    {
                            //        HKX.Transform t = HkxSkeleton.Transforms.GetArrayData().Elements[TransformTrackToBoneIndices[parentIndex]];

                            //        tposeMatrix *= Matrix.CreateScale(t.Scale.Vector.X, t.Scale.Vector.Y, t.Scale.Vector.Z);
                            //        tposeMatrix *= Matrix.CreateFromQuaternion(new Quaternion(t.Rotation.Vector.X, t.Rotation.Vector.Y, t.Rotation.Vector.Z, t.Rotation.Vector.W));
                            //        tposeMatrix *= Matrix.CreateTranslation(t.Position.Vector.X, t.Position.Vector.Y, t.Position.Vector.Z);

                            //    }

                            //    HkxBoneMatrices[i] = tposeMatrix;// + HkxBoneMatrices[i];
                            //}

                            //var boneLength = (HkxBonePositions[i/*boneIndex*/] - HkxBonePositions[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data]).Length();
                            //var newBonePrim = new DbgPrimSolidBone("", new Transform(realMatrix), Quaternion.Identity, Math.Min(boneLength / 8, 0.25f), boneLength, Color.Yellow);
                            //DBG.AddPrimitive(newBonePrim);
                            HkxBonePrimitives[i].Transform = new Transform(HkxBoneMatrices[i]);
                        }
                        else
                        {
                            HkxBoneMatrices[i] =
                                Matrix.CreateScale(HkxBoneScales[i])
                                * HkxBoneParentMatrices[i/*boneIndex*/]
                                * CurrentRootMotionMatrix;

                            //if (CurrentAnimBlendHint == HKX.AnimationBlendHint.ADDITIVE ||
                            //    CurrentAnimBlendHint == HKX.AnimationBlendHint.ADDITIVE_DEPRECATED)
                            //{
                            //    Matrix tposeMatrix = Matrix.Identity;
                            //    if (TransformTrackToBoneIndices[i] >= 0)
                            //    {
                            //        HKX.Transform t = HkxSkeleton.Transforms.GetArrayData().Elements[TransformTrackToBoneIndices[i]];

                            //        tposeMatrix *= Matrix.CreateScale(t.Scale.Vector.X, t.Scale.Vector.Y, t.Scale.Vector.Z);
                            //        tposeMatrix *= Matrix.CreateFromQuaternion(new Quaternion(t.Rotation.Vector.X, t.Rotation.Vector.Y, t.Rotation.Vector.Z, t.Rotation.Vector.W));
                            //        tposeMatrix *= Matrix.CreateTranslation(t.Position.Vector.X, t.Position.Vector.Y, t.Position.Vector.Z);

                            //    }

                            //    HkxBoneMatrices[i] = tposeMatrix;// + HkxBoneMatrices[i];
                            //}

                            //var newBonePrim = new DbgPrimSolidBone("", new Transform(HkxBoneParentMatrices[i/*boneIndex*/]), Quaternion.Identity, 0.15f, 0.3f, Color.Yellow);
                            //DBG.AddPrimitive(newBonePrim);
                            //HkxBonePrimitives.Add(newBonePrim);
                            HkxBonePrimitives[i].Transform = new Transform(HkxBoneMatrices[i]);
                        }
                    }

                    

                    CopyHavokMatrixToOtherMatrices(i);
                    //boneIndex++;
                }
            }

            LastHkxFrameCalculated = frame;
            InterleavedIndexRangeEndOnPreviousFrame = thisFrameRange.End;
        }

        private static void Load3DAsset(string assetUri, byte[] assetBytes, Transform transform, bool dontLoadModels = false)
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
                        Load3DAsset(f.Name, f.Bytes, transform, dontLoadModels);
                    }
                }
                else if (SoulsFormats.BND4.Is(assetBytes))
                {
                    var bnd = SoulsFormats.BND4.Read(assetBytes);
                    foreach (var f in bnd.Files)
                    {
                        Load3DAsset(f.Name, f.Bytes, transform, dontLoadModels);
                    }
                }
            }
            else if (!dontLoadModels && (upper.EndsWith(".FLVER") || upper.EndsWith(".FLVER.DCX") || upper.EndsWith(".FLV") || upper.EndsWith(".FLV.DCX")))
            {
                DBG.ClearPrimitives(DbgPrimCategory.FlverBone);
                DBG.ClearPrimitives(DbgPrimCategory.DummyPoly);

                if (SoulsFormats.FLVER0.Is(assetBytes))
                {
                    var flver = SoulsFormats.FLVER0.Read(assetBytes);
                    LoadFLVER(flver);
                }
                else
                {
                    var flver = SoulsFormats.FLVER2.Read(assetBytes);
                    LoadFLVER(flver);
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

        private static void LoadFLVER(FLVER0 flver)
        {
            throw new NotImplementedException();
        }

        private static void LoadFLVER(FLVER2 flver)
        {
            CurrentModel = flver;

            FlverBoneCount = flver.Bones.Count;

            //throw new NotImplementedException();

            FlverBonePrims = new List<IDbgPrim>();

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

            AnimatedDummyPolyClusters = new DbgPrimDummyPolyCluster[FlverBoneCount];

            var dummiesByID = new Dictionary<int, List<FLVER2.Dummy>>();

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
            }

            foreach (var kvp in dummiesByID)
            {
                var dmyPrim = new DbgPrimDummyPolyCluster(0.5f, kvp.Value, flver.Bones);
                DBG.AddPrimitive(dmyPrim);
                AnimatedDummyPolyClusters[kvp.Key] = dmyPrim;
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

            FlverBoneTPoseMatrices = new List<Matrix>();
            List<Vector3> bonePos = new List<Vector3>();


            foreach (var b in flver.Bones)
            {
                var parentMatrix = GetBoneParentMatrix(b);

                FlverBoneTPoseMatrices.Add(parentMatrix);

                bonePos.Add(Vector3.Transform(Vector3.Zero, parentMatrix));
            }
            int boneIndex = 0;
            foreach (var b in flver.Bones)
            {
                if (b.ParentIndex >= 0)
                {
                    var boneTransform = new Transform(FlverBoneTPoseMatrices[boneIndex]);
                    var boneLength = (bonePos[boneIndex] - bonePos[b.ParentIndex]).Length();
                    var prim = new DbgPrimSolidBone(isHkx: false, getBoneSpacePrefix(b) + b.Name, boneTransform, Quaternion.Identity, Math.Min(boneLength / 4, 0.25f), boneLength, Color.Purple);

                    prim.Children.Add(new DbgPrimWireBox(new Transform(FlverBoneTPoseMatrices[boneIndex]),
                        new Vector3(b.BoundingBoxMin.X, b.BoundingBoxMin.Y, b.BoundingBoxMin.Z),
                        new Vector3(b.BoundingBoxMax.X, b.BoundingBoxMax.Y, b.BoundingBoxMax.Z),
                        Color.Orange)
                    {
                        Category = DbgPrimCategory.FlverBoneBoundingBox
                    });

                    DBG.AddPrimitive(prim);
                    FlverBonePrims.Add(prim);
                }
                else
                {
                    var boneTransform = new Transform(FlverBoneTPoseMatrices[boneIndex]);
                    var prim = new DbgPrimWireBox(boneTransform, Vector3.One * 0.05f, Color.Purple)
                    {
                        Name = getBoneSpacePrefix(b) + b.Name,
                        Category = DbgPrimCategory.FlverBone
                    };

                    prim.Children.Add(new DbgPrimWireBox(new Transform(FlverBoneTPoseMatrices[boneIndex]),
                            new Vector3(b.BoundingBoxMin.X, b.BoundingBoxMin.Y, b.BoundingBoxMin.Z),
                            new Vector3(b.BoundingBoxMax.X, b.BoundingBoxMax.Y, b.BoundingBoxMax.Z),
                            Color.Orange)
                    {
                        Category = DbgPrimCategory.FlverBoneBoundingBox
                    });

                    DBG.AddPrimitive(prim);
                    FlverBonePrims.Add(prim);
                }

                boneIndex++;
            }
        }

        public static void CreateMenuBarViewportSettings(TaeEditor.TaeMenuBarBuilder menu)
        {
            var vsync = menu["3D Preview/Vsync"];

            menu.ClearItem("3D Preview");

            menu.AddItem("3D Preview", vsync);

            menu.AddSeparator("3D Preview");

            menu.AddItem("3D Preview", "Render Meshes", () => !GFX.HideFLVERs,
                b => GFX.HideFLVERs = !b);

            menu.AddItem("3D Preview/Toggle Individual Meshes", "Show All", () =>
            {
                foreach (var model in GFX.ModelDrawer.Models)
                {
                    foreach (var sm in model.GetSubmeshes())
                    {
                        sm.IsVisible = true;
                    }
                }
            });

            menu.AddItem("3D Preview/Toggle Individual Meshes", "Hide All", () =>
            {
                foreach (var model in GFX.ModelDrawer.Models)
                {
                    foreach (var sm in model.GetSubmeshes())
                    {
                        sm.IsVisible = false;
                    }
                }
            });

            menu.AddSeparator("3D Preview/Toggle Individual Meshes");

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

            menu.AddItem("3D Preview/Toggle By Model Mask", "Show All", () =>
            {
                foreach (var kvp in modelMaskMap)
                {
                    foreach (var sm in kvp.Value)
                    {
                        sm.IsVisible = true;
                    }
                }
            });

            menu.AddItem("3D Preview/Toggle By Model Mask", "Hide All", () =>
            {
                foreach (var kvp in modelMaskMap)
                {
                    foreach (var sm in kvp.Value)
                    {
                        sm.IsVisible = false;
                    }
                }
            });

            menu.AddSeparator("3D Preview/Toggle By Model Mask");

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

            menu.AddItem("3D Preview", "Render HKX Skeleton (Yellow)", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.HkxBone],
                b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.HkxBone] = b);

            menu.AddItem("3D Preview", "Render FLVER Skeleton (Purple)", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone],
                b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone] = b);

            menu.AddItem("3D Preview", "Render FLVER Skeleton Bounding Boxes (Orange)", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox],
                b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox] = b);

            menu.AddItem("3D Preview", "Render DummyPoly (Red/Green/Blue)", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
                b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = b);

            //menu.AddItem("3D Preview", "Render DummyPoly ID Tags", () => DBG.CategoryEnableDbgLabelDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
            //    b => DBG.CategoryEnableDbgLabelDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = b);

            menu.AddItem("3D Preview/Toggle DummyPoly By ID", $"Show All", () =>
            {
                foreach (var prim in DBG.GetPrimitives().Where(p => p.Category == DebugPrimitives.DbgPrimCategory.DummyPoly))
                {
                    if (prim is DbgPrimDummyPolyCluster cluster)
                    {
                        cluster.EnableDraw = true;
                    }
                }
            });

            menu.AddItem("3D Preview/Toggle DummyPoly By ID", $"Hide All", () =>
            {
                foreach (var prim in DBG.GetPrimitives().Where(p => p.Category == DebugPrimitives.DbgPrimCategory.DummyPoly))
                {
                    if (prim is DbgPrimDummyPolyCluster cluster)
                    {
                        cluster.EnableDraw = false;
                    }
                }
            });

            menu.AddSeparator("3D Preview/Toggle DummyPoly By ID");

            foreach (var prim in DBG.GetPrimitives().Where(p => p.Category == DebugPrimitives.DbgPrimCategory.DummyPoly))
            {
                if (prim is DbgPrimDummyPolyCluster cluster)
                {
                    menu.AddItem("3D Preview/Toggle DummyPoly By ID", $"{cluster.ID}", () => cluster.EnableDraw, b => cluster.EnableDraw = b);
                }
            }
        }

        private static (FLVER2, List<TPF>) LoadFourPartsFiles(FLVER2 baseChr, string part1Name, string part2Name, string part3Name, string part4Name)
        {
            Dictionary<string, int> baseChrBoneMap = new Dictionary<string, int>();
            for (int i = 0; i < baseChr.Bones.Count; i++)
            {
                baseChrBoneMap.Add(baseChr.Bones[i].Name, i);
            }

            List<TPF> tpfsUsed = new List<TPF>();

            void DoPart(string p)
            {
                if (!File.Exists(p))
                {
                    Console.WriteLine($"Parts model '{p}' did not exist");
                    return;
                }

                FLVER2 partFlver = null;
                string debug_PartFlverName = null;

                if (BND4.Is(p))
                {
                    var bnd = BND4.Read(p);

                    foreach (var f in bnd.Files)
                    {
                        if (partFlver == null && FLVER2.Is(f.Bytes))
                        {
                            partFlver = FLVER2.Read(f.Bytes);
                            debug_PartFlverName = f.Name;
                        }
                        else if (TPF.Is(f.Bytes))
                        {
                            tpfsUsed.Add(TPF.Read(f.Bytes));
                        }
                    }
                }
                else if (BND3.Is(p))
                {
                    var bnd = BND3.Read(p);

                    foreach (var f in bnd.Files)
                    {
                        if (partFlver == null && FLVER2.Is(f.Bytes))
                        {
                            partFlver = FLVER2.Read(f.Bytes);
                            debug_PartFlverName = f.Name;
                        }
                        else if (TPF.Is(f.Bytes))
                        {
                            tpfsUsed.Add(TPF.Read(f.Bytes));
                        }
                    }
                }

                int RemapBone(int boneIndex)
                {
                    if (boneIndex == -1)
                        return -1;

                    if (baseChrBoneMap.ContainsKey(partFlver.Bones[boneIndex].Name))
                        return baseChrBoneMap[partFlver.Bones[boneIndex].Name];
                    else
                        return -1;
                }

                foreach (var mesh in partFlver.Meshes)
                {
                    // Check if DS1 memes
                    if (partFlver.Header.Version <= 0x2000D)
                    {
                        for (int i = 0; i < mesh.BoneIndices.Count; i++)
                        {
                            mesh.BoneIndices[i] = RemapBone(mesh.BoneIndices[i]);
                        }

                        mesh.DefaultBoneIndex = RemapBone(mesh.DefaultBoneIndex);
                    }
                    else
                    {
                        foreach (var vert in mesh.Vertices)
                        {
                            for (int i = 0; i < vert.BoneIndices.Length; i++)
                            {
                                vert.BoneIndices[i] = RemapBone(vert.BoneIndices[i]);
                            }
                        }
                    }

                    if (mesh.MaterialIndex >= partFlver.Materials.Count)
                    {
                        mesh.MaterialIndex = partFlver.Materials.Count - 1;
                    }

                    baseChr.Materials.Add(partFlver.Materials[mesh.MaterialIndex]);
                    

                    if (partFlver.Materials[mesh.MaterialIndex].GXIndex >= 0)
                    {
                        baseChr.GXLists.Add(partFlver.GXLists[partFlver.Materials[mesh.MaterialIndex].GXIndex]);
                        partFlver.Materials[mesh.MaterialIndex].GXIndex = baseChr.GXLists.Count - 1;
                    }

                    mesh.MaterialIndex = baseChr.Materials.Count - 1;

                    foreach (var vb in mesh.VertexBuffers)
                    {
                        baseChr.BufferLayouts.Add(partFlver.BufferLayouts[vb.LayoutIndex]);
                        vb.LayoutIndex = baseChr.BufferLayouts.Count - 1;
                    }

                    baseChr.Meshes.Add(mesh);
                }
            }

            DoPart(part1Name);
            DoPart(part2Name);
            DoPart(part3Name);
            DoPart(part4Name);

            return (baseChr, tpfsUsed);
        }
    }
}
