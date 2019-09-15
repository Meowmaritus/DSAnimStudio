//#define DISABLE_HKX_EXCEPTION_CATCH
//#define DISABLE_SPLINE_CACHE
//#define IGNORE_FLVER_REFERENCE_POSE
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
        public static void DEBUG_TEST_PlayerPartsLoad()
        {

        }

        public static void Init()
        {
            // This allows you to use the debug menu with the gamepad for testing.
            // Final release will have no gamepad support or menu.
            //DBG.EnableGamePadInput = true;
            //DBG.EnableMenu = true;

            GFX.ModelDrawer.ClearScene();
            DBG.ClearPrimitives(DebugPrimitives.DbgPrimCategory.HkxBone);
            DBG.ClearPrimitives(DebugPrimitives.DbgPrimCategory.DummyPoly);
            DBG.ClearPrimitives(DebugPrimitives.DbgPrimCategory.DummyPolyHelper);
            GFX.HideFLVERs = false;
            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.HkxBone] = false;
            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone] = false;
            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox] = false;
            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = false;
            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPolyHelper] = true;

            DBG.CategoryEnableDbgLabelDraw[DebugPrimitives.DbgPrimCategory.HkxBone] = false;
            DBG.CategoryEnableDbgLabelDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = false;
            DBG.CategoryEnableDbgLabelDraw[DebugPrimitives.DbgPrimCategory.DummyPolyHelper] = true;

            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.WeaponDummyPoly] = true;

            DBG.CategoryEnableNameDraw[DebugPrimitives.DbgPrimCategory.HkxBone] = false;

            GFX.SSAA = 2;
            //GFX.MSAA = 2;
            DbgPrimDummyPolyCluster.GlobalRenderSizeMult = 2;

        }

        public static bool IsLoadingAnimation = false;

        public static bool IsPlayerLoaded = false;

        public static void LoadContent()
        {
            
            TaeSoundManager.LoadSoundsFromDir($@"{Main.Directory}\Content\Sounds\Character");
        }

        /// <summary>
        /// After 3D model is drawn.
        /// </summary>
        public static void TaeViewportDrawPost(GameTime gameTime)
        {
            if (IsLoadingAnimation)
            {
                return;
            }

            var printer = new StatusPrinter(Vector2.One * 4);

            //printer.AppendLine(GFX.FlverOpacity.ToString());

            //printer.AppendLine($"Current Indirect Lighting Value: {GFX.IndirectLightMult}");
            //printer.AppendLine($"Mouse Click Start Type: {Main.TAE_EDITOR.WhereCurrentMouseClickStarted}");

            if (!HaventLoadedAnythingYet)
            {
                if (IncompatibleHavokVersion)
                {
                    printer.AppendLine($"[UNSUPPORTED HAVOK VERSION]", Color.Red);

                    if (CurrentAnimationHKXBytes == null)
                        printer.AppendLine($"Animation File: {CurrentAnimationName} (does not exist)", Color.Red);
                    else
                        printer.AppendLine($"Animation File: {CurrentAnimationName}");
                }
                else
                {
                    if (HkxSkeleton != null)
                    {
                        printer.AppendLine($"HKX Bone Count: {HkxSkeleton.Bones.Capacity} (Max Supported By Viewer: {GFXShaders.FlverShader.MAX_ALL_BONE_ARRAYS})", (HkxSkeleton.Bones.Capacity > GFXShaders.FlverShader.MAX_ALL_BONE_ARRAYS) ? Color.Red : Color.Cyan);
                    }

                    if (CurrentAnimationHKXBytes == null)
                        printer.AppendLine($"Animation File: {CurrentAnimationName} (does not exist)", Color.Red);
                    else
                        printer.AppendLine($"Animation File: {CurrentAnimationName}");

                    if (CurrentAnimationHKX != null)
                    {
                        printer.AppendLine($"Anim BlendHint: {CurrentAnimBlendHint}", CurrentAnimBlendHint == HKX.AnimationBlendHint.NORMAL ? Color.Cyan : Color.LightGreen);
                        int fps = (int)Math.Round(1 / CurrentAnimationFrameDuration);
                        printer.AppendLine($"Anim Frame Rate: {fps} FPS", fps == 30 ? Color.Cyan : Color.LightGreen);
                    }

                    //foreach (var ar in CurrentAnimReferenceChain)
                    //{
                    //    printer.AppendLine(ar);
                    //}
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

                if (ChrAsm.EquipRWeapon >= 0 && ParamManager.EquipParamWeapon.ContainsKey(ChrAsm.EquipRWeapon))
                {
                    if (ChrAsm.EquipRWeapon >= 0 && FmgManager.WeaponNames.ContainsKey((int)ChrAsm.EquipRWeapon))
                        printer.AppendLine($"WPN Name: {FmgManager.WeaponNames[(int)ChrAsm.EquipRWeapon]}");
                    printer.AppendLine($"WPN ID: {ChrAsm.EquipRWeapon}");
                    printer.AppendLine($"WPN Behavior Variation: {ParamManager.EquipParamWeapon[ChrAsm.EquipRWeapon].BehaviorVariationID}");
                    printer.AppendLine($"WPN Category: {ParamManager.EquipParamWeapon[ChrAsm.EquipRWeapon].WepMotionCategory}");
                    printer.AppendLine($"WPN Special ATK Category: {ParamManager.EquipParamWeapon[ChrAsm.EquipRWeapon].SpAtkCategory}");
                }
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

        public static bool Debug_LockToTPose = false;

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

        public static string InterrootPath = null;

        public static FLVER2 CurrentModel;

        public static Exception HkxAnimException = null;

        public static bool IsSnapTo30FPS = false;

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
        public static int CurrentAnimationFrameRateRounded => (int)Math.Round(1.0 / CurrentAnimationFrameDuration);
        public static List<Dictionary<int, Havok.SplineCompressedAnimation.TransformTrack>> BoneToTransformTrackMap;
        public static float CurrentAnimBlockDuration = 8.5f;
        public static int CurrentAnimFramesPerBlock = 256;
        public static SoulsFormats.HKX.AnimationType CurrentAnimType = HKX.AnimationType.HK_UNKNOWN_ANIMATION;
        public static SoulsFormats.HKX.AnimationBlendHint CurrentAnimBlendHint = HKX.AnimationBlendHint.NORMAL;

        public static int CurrentBlock => PlaybackCursor != null ?
            (int)((PlaybackCursor.GUICurrentFrame % CurrentAnimationFrameCount) / CurrentAnimFramesPerBlock) : 0;

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
        public static Matrix[] ShaderMatrix3 = new Matrix[GFXShaders.FlverShader.NUM_BONES];

        public static int FlverBoneCount;

        // ! BLESSED METHOD !
        private static void CopyHavokMatrixToOtherMatrices(int havokMatrixIndex)
        {
            if (IsLoadingAnimation)
            {
                return;
            }

            if (!HkxBoneToFlverBoneMap.ContainsKey(havokMatrixIndex))
                return;

            var flverBoneIndex = HkxBoneToFlverBoneMap[havokMatrixIndex];
            var matrixBank = flverBoneIndex / GFXShaders.FlverShader.NUM_BONES;
            var relativeMatrixIndex = flverBoneIndex % GFXShaders.FlverShader.NUM_BONES;
#if IGNORE_FLVER_REFERENCE_POSE
            var finalMatrix =
                Matrix.Invert(HkxBoneParentMatrices_Reference[havokMatrixIndex])
                * HkxBoneParentMatrices[havokMatrixIndex]
                * CurrentRootMotionMatrix;
#else
            var finalMatrix =
                (FlverBoneTPoseMatricesEnable[flverBoneIndex] ? Matrix.Invert(FlverBoneTPoseMatrices[flverBoneIndex]) : Matrix.Invert(HkxBoneParentMatrices_Reference[havokMatrixIndex]))
                * HkxBoneParentMatrices[havokMatrixIndex]
                * CurrentRootMotionMatrix;
#endif
            if (matrixBank == 0)
                ShaderMatrix0[relativeMatrixIndex] = finalMatrix;
            else if (matrixBank == 1)
                ShaderMatrix1[relativeMatrixIndex] = finalMatrix;
            else if (matrixBank == 2)
                ShaderMatrix2[relativeMatrixIndex] = finalMatrix;
            else if (matrixBank == 3)
                ShaderMatrix3[relativeMatrixIndex] = finalMatrix;

            DummyPolyManager.AnimatedDummyPolyClusters[flverBoneIndex]?.UpdateWithBoneMatrix(finalMatrix);
        }

        private static void RevertToTPose()
        {
            if (IsLoadingAnimation)
            {
                return;
            }

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
                else if (matrixBank == 3)
                    ShaderMatrix2[relativeMatrixIndex] = Matrix.Identity;

                DummyPolyManager.AnimatedDummyPolyClusters[i]?.UpdateWithBoneMatrix(Matrix.Identity);

                FlverBonePrims[i].Transform = new Transform(Matrix.Identity);
            }
        }

        public static List<Matrix> FlverBoneTPoseMatrices;
        public static List<bool> FlverBoneTPoseMatricesEnable;
        public static List<IDbgPrim> FlverBonePrims;


        //public static Matrix[] FlverAnimMatrices;

        

        public static Dictionary<int, int> FlverBoneToHkxBoneMap;
        public static Dictionary<int, int> HkxBoneToFlverBoneMap;

        /// <summary>
        /// Name of currently-selected animation.
        /// </summary>
        public static string CurrentAnimationName = null;

        public static List<string> CurrentAnimReferenceChain = new List<string>();

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
            set => PlaybackCursor.HkxAnimationLength = (value ?? 0) > 0 ? value : null;
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

        public static void OnAnimFrameChange(bool isScrubbing)
        {
            if (isScrubbing)
            {
                EventSim.OnSimulationScrub();
            }

            if (IsLoadingAnimation)
            {
                return;
            }

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
                    CalculateAnimation((float)PlaybackCursor.GUICurrentTime, (float)PlaybackCursor.GUICurrentFrame, Debug_LockToTPose);

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
            else
            {
                // If no valid anim, go to t-pose
                CalculateAnimation((float)PlaybackCursor.GUICurrentTime, (float)PlaybackCursor.GUICurrentFrame, lockToTpose: true);
            }
                
            

            
        }

        /// <summary>
        /// Runs once the TAE shit loads an ANIBND (doesn't run if a loose TAE is selected)
        /// Simply looks for shit named similarly to the ANIBND and loads those assets.
        /// </summary>
        public static void OnLoadANIBND(TaeEditor.TaeMenuBarBuilder menuBar, IProgress<double> progress)
        {
            Init();

            IsLoadingAnimation = true;
            try
            {
                LoadingTaskMan.DoLoadingTaskSynchronous("LoadModels", "Loading model(s)...", innerProgress =>
                {
                    for (int i = 0; i < GFXShaders.FlverShader.NUM_BONES; i++)
                    {
                        ShaderMatrix0[i] = Matrix.Identity;
                        ShaderMatrix1[i] = Matrix.Identity;
                        ShaderMatrix2[i] = Matrix.Identity;
                        ShaderMatrix3[i] = Matrix.Identity;
                    }

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

                    IsPlayerLoaded = chrNameBase.ToLower().EndsWith("c0000");

                    if (IsPlayerLoaded)
                    {
                        DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.HkxBone] = true;
                        GFX.World.ModelHeight_ForOrbitCam = 2;
                    }

                    var folder = new FileInfo(AnibndPath).DirectoryName;

                    var lastSlashInFolder = folder.LastIndexOf("\\");

                    InterrootPath = folder.Substring(0, lastSlashInFolder);

                    if (File.Exists($"{chrNameBase}.chrbnd.dcx"))
                    {
                        Load3DAsset($"{chrNameBase}.chrbnd.dcx", File.ReadAllBytes($"{chrNameBase}.chrbnd.dcx"), transform);
                    }
                    else if (File.Exists($"{chrNameBase}.chrbnd"))
                    {
                        Load3DAsset($"{chrNameBase}.chrbnd", File.ReadAllBytes($"{chrNameBase}.chrbnd"), transform);
                    }
                    else
                    {
                        if (File.Exists($"{chrNameBase}.flver.dcx"))
                        {
                            LoadFLVER(FLVER2.Read($"{chrNameBase}.flver.dcx"));
                        }
                        else if (File.Exists($"{chrNameBase}.flver"))
                        {
                            LoadFLVER(FLVER2.Read($"{chrNameBase}.flver"));
                        }

                        if (File.Exists($"{chrNameBase}.tpf.dcx"))
                        {
                            TexturePool.AddTpfFromPath($"{chrNameBase}.tpf.dcx");
                            GFX.ModelDrawer.RequestTextureLoad();
                        }
                        else if (File.Exists($"{chrNameBase}.tpf"))
                        {
                            TexturePool.AddTpfFromPath($"{chrNameBase}.tpf");
                            GFX.ModelDrawer.RequestTextureLoad();
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
                    innerProgress.Report(2.0 / 5.0);

                    string possibleSharedTexPack = chrNameBase.Substring(0, chrNameBase.Length - 1) + "9";

                    if (File.Exists($"{possibleSharedTexPack}.chrbnd.dcx"))
                    {
                        Load3DAsset($"{possibleSharedTexPack}.chrbnd.dcx", File.ReadAllBytes($"{possibleSharedTexPack}.chrbnd.dcx"), transform, dontLoadModels: true);
                    }
                    else if (File.Exists($"{possibleSharedTexPack}.chrbnd"))
                    {
                        Load3DAsset($"{possibleSharedTexPack}.chrbnd", File.ReadAllBytes($"{possibleSharedTexPack}.chrbnd"), transform, dontLoadModels: true);
                    }
                    innerProgress.Report(3.0 / 5.0);

                    if (File.Exists($"{possibleSharedTexPack}.texbnd.dcx"))
                    {
                        Load3DAsset($"{possibleSharedTexPack}.texbnd.dcx", File.ReadAllBytes($"{possibleSharedTexPack}.texbnd.dcx"), transform);
                    }
                    else if (File.Exists($"{possibleSharedTexPack}.texbnd"))
                    {
                        Load3DAsset($"{possibleSharedTexPack}.texbnd", File.ReadAllBytes($"{possibleSharedTexPack}.texbnd"), transform);
                    }
                    innerProgress.Report(4.0 / 5.0);

                    string extraBloodborneTextures = $"{chrNameBase}_2";

                    if (File.Exists($"{extraBloodborneTextures}.tpf.dcx"))
                    {
                        Load3DAsset($"{extraBloodborneTextures}.tpf.dcx", File.ReadAllBytes($"{extraBloodborneTextures}.tpf.dcx"), transform);
                    }
                    else if (File.Exists($"{extraBloodborneTextures}.tpf"))
                    {
                        Load3DAsset($"{extraBloodborneTextures}.tpf", File.ReadAllBytes($"{extraBloodborneTextures}.ypf"), transform);
                    }
                    innerProgress.Report(4.5 / 5.0);

                    if (Directory.Exists($"{chrNameBase}"))
                    {
                        TexturePool.AddTPFFolder($"{chrNameBase}");
                        GFX.ModelDrawer.RequestTextureLoad();
                    }

                    innerProgress.Report(1.0);

                });

                progress.Report(0.75);

                LoadingTaskMan.DoLoadingTaskSynchronous("LoadAnimations", "Loading animation(s) and GameParam...", innerProgress =>
                {
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

                    innerProgress.Report(0.25);


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

                    innerProgress.Report(0.5);

                    InitHavokBones();

                    innerProgress.Report(0.75);

                    var model = new Model(CurrentModel, useSecondUV: false);
                    var modelInstance = new ModelInstance("Character Model", model, Transform.Default, -1, -1, -1, -1);
                    GFX.ModelDrawer.AddModelInstance(model, "", Transform.Default);

                    if (!IsPlayerLoaded)
                        GFX.World.ModelHeight_ForOrbitCam = model.Bounds.Max.Y;

                    GFX.World.OrbitCamReset();

                    CreateMenuBarViewportSettings(menuBar);

                    //if (CurrentHkxVariation == HKX.HKXVariation.HKXDS1)
                    //{
                    //    if (GFX.FlverShaderWorkflowType != GFXShaders.FlverShader.FSWorkflowType.Ass)
                    //        GFX.FlverShaderWorkflowType = GFXShaders.FlverShader.FSWorkflowType.Ass;

                    //    Environment.DrawCubemap = false;
                    //}
                    //else
                    //{
                    //    if (GFX.FlverShaderWorkflowType == GFXShaders.FlverShader.FSWorkflowType.Ass)
                    //        GFX.FlverShaderWorkflowType = GFXShaders.FlverShader.FSWorkflowType.Gloss;

                    //    Environment.DrawCubemap = true;
                    //}

                    LoadParams();

                    if (!IsPlayerLoaded)
                    {
                        GFX.ModelDrawer.NPC_SelectedMaskPreset = ModelDrawer.NPC_DEFAULT_PRESET_NAME;

                        if (GFX.ModelDrawer.MaskPresets.Count > 1)
                        {
                            //foreach (var kvp in GFX.ModelDrawer.MaskPresets.Skip(1))
                            //{
                            //    if (!GFX.ModelDrawer.MaskPresetsInvisibility[kvp.Key])
                            //    {
                            //        GFX.ModelDrawer.SelectedMaskPreset = kvp.Key;
                            //        GFX.ModelDrawer.DefaultAllMaskValues();
                            //        break;
                            //    }
                            //}
                            GFX.ModelDrawer.NPC_SelectedMaskPreset = GFX.ModelDrawer.MaskPresets.Keys.ElementAt(1);
                        }
                    }

                    CreateMenuBarNPCOrPlayerSettings(menuBar);

                    DummyPolyManager.ClearAllHitboxPrimitives();

                    innerProgress.Report(1);
                });

                progress.Report(0.95);
            }
            finally
            {
                IsLoadingAnimation = false;
            }
        }

        public static void LoadParams()
        {
            var folder = new FileInfo(AnibndPath).DirectoryName;
            var lastSlashInFolder = folder.LastIndexOf("\\");
            if (ParamManager.LoadParamBND(CurrentHkxVariation, folder.Substring(0, lastSlashInFolder)))
            {
                if (!IsPlayerLoaded)
                {
                    GFX.ModelDrawer.NPC_ReadDrawMaskPresets(
                        ParamManager.GetParam(CurrentHkxVariation, "NpcParam"),
                        CurrentHkxVariation,
                        AnibndPath);
                }
            }

        }

        /// <summary>
        /// Called when user selects an animation in the lists and loads the event graph for it.
        /// </summary>
        public static void OnAnimationSelected(IReadOnlyDictionary<string, TAE> taeDict, TAE tae, TAE.Animation anim)
        {
            if (HaventLoadedAnythingYet)
                HaventLoadedAnythingYet = false;

            var refChainSolver = new AnimRefChainSolver(CurrentHkxVariation, taeDict, AllHkxFiles);
            CurrentAnimationName = refChainSolver.GetHKXName(tae, anim);
            CurrentAnimReferenceChain = refChainSolver.GetRefChainStrings();
            var forDebug = AllHkxFiles;
            CurrentAnimationHKXBytes = AllHkxFiles.FirstOrDefault(x => x.Key.ToUpper().Contains(CurrentAnimationName.ToUpper())).Value;

            EventSim.OnNewAnimSelected();

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

            // If STILL NULL just give up :MecHands:
            if (CurrentAnimationHKXBytes == null)
            {
                //CurrentAnimationName = null;
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

            OnAnimFrameChange(true);
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

        public static (Vector3 Scale, Quaternion Rotation, Vector3 Position) 
            GetTransformTrackValueForFrame(Havok.SplineCompressedAnimation.TransformTrack track, 
            float frame, HKX.Transform skeleTransform, bool additiveBlend)
        {
            Vector3 scale = Vector3.One;
            Quaternion rotation = Quaternion.Identity;
            Vector3 position = Vector3.Zero;

            var scaleX = track.SplineScale?.ChannelX == null
                ? (additiveBlend ? 1 : track.StaticScale.X) : track.SplineScale.GetValueX(frame);
            var scaleY = track.SplineScale?.ChannelY == null
                ? (additiveBlend ? 1 : track.StaticScale.Y) : track.SplineScale.GetValueY(frame);
            var scaleZ = track.SplineScale?.ChannelZ == null
                ? (additiveBlend ? 1 : track.StaticScale.Z) : track.SplineScale.GetValueZ(frame);

            if (!additiveBlend && (!track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineX) &&
                !track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticX)))
            {
                scaleX = skeleTransform.Scale.Vector.X;
            }

            if (!additiveBlend && (!track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineY) &&
                !track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticY)))
            {
                scaleY = skeleTransform.Scale.Vector.Y;
            }

            if (!additiveBlend && (!track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineZ) &&
                !track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticZ)))
            {
                scaleZ = skeleTransform.Scale.Vector.Z;
            }

            if (additiveBlend)
            {
                scaleX *= skeleTransform.Scale.Vector.X;
                scaleY *= skeleTransform.Scale.Vector.Y;
                scaleZ *= skeleTransform.Scale.Vector.Z;
            }

            if (track.HasSplineRotation)
            {
                rotation = track.SplineRotation.GetValue(frame);
            }
            else if (track.HasStaticRotation)
            {
                // We actually need static rotation or Gael hands become unbent among others
                rotation = track.StaticRotation;
            }
            else
            {
                rotation = additiveBlend ? Quaternion.Identity : new Quaternion(
                    skeleTransform.Rotation.Vector.X,
                    skeleTransform.Rotation.Vector.Y,
                    skeleTransform.Rotation.Vector.Z,
                    skeleTransform.Rotation.Vector.W);
            }

            if (additiveBlend)
            {
                rotation *= new Quaternion(
                    skeleTransform.Rotation.Vector.X,
                    skeleTransform.Rotation.Vector.Y,
                    skeleTransform.Rotation.Vector.Z,
                    skeleTransform.Rotation.Vector.W);
            }

            // We use skeleton transform here instead of static position, which fixes Midir, Ariandel, and other enemies
            // at the cost of Ringed Knight's weapons being misplaced from their hands. This way more shit works 
            // right but I wish I knew WHY and I wish ALL worked.
            //var posX = track.SplinePosition?.ChannelX == null
            //    ? (additiveBlend ? 0 : skeleTransform.Position.Vector.X) : track.SplinePosition.GetValueX(frame);
            //var posY = track.SplinePosition?.ChannelY == null
            //    ? (additiveBlend ? 0 : skeleTransform.Position.Vector.Y) : track.SplinePosition.GetValueY(frame);
            //var posZ = track.SplinePosition?.ChannelZ == null
            //    ? (additiveBlend ? 0 : skeleTransform.Position.Vector.Z) : track.SplinePosition.GetValueZ(frame);

            var posX = track.SplinePosition?.ChannelX == null
                ? (additiveBlend ? 0 : track.StaticPosition.X) : track.SplinePosition.GetValueX(frame);
            var posY = track.SplinePosition?.ChannelY == null
                ? (additiveBlend ? 0 : track.StaticPosition.Y) : track.SplinePosition.GetValueY(frame);
            var posZ = track.SplinePosition?.ChannelZ == null
                ? (additiveBlend ? 0 : track.StaticPosition.Z) : track.SplinePosition.GetValueZ(frame);


            //var posX = !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineX)
            //    ? (additiveBlend ? 0 : skeleTransform.Position.Vector.X + track.StaticPosition.X) : track.SplinePosition.GetValueX(frame);
            //var posY = !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineY)
            //    ? (additiveBlend ? 0 : skeleTransform.Position.Vector.Y + track.StaticPosition.Y) : track.SplinePosition.GetValueY(frame);
            //var posZ = !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineZ)
            //    ? (additiveBlend ? 0 : skeleTransform.Position.Vector.Z + track.StaticPosition.Z) : track.SplinePosition.GetValueZ(frame);


            //var nullPos = false;// (posX == 0 && posY == 0 && posZ == 0);

            if (!additiveBlend && (!track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineX) &&
                !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticX)))
            {
                posX = skeleTransform.Position.Vector.X;
            }

            if (!additiveBlend && (!track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineY) &&
                !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticY)))
            {
                posY = skeleTransform.Position.Vector.Y;
            }

            if (!additiveBlend && (!track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineZ) &&
                !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticZ)))
            {
                posZ = skeleTransform.Position.Vector.Z;
            }

            if (additiveBlend)
            {
                posX += skeleTransform.Position.Vector.X;
                posY += skeleTransform.Position.Vector.Y;
                posZ += skeleTransform.Position.Vector.Z;
            }

            return (new Vector3(scaleX, scaleY, scaleZ), rotation, new Vector3(posX, posY, posZ));
        }

        public static (Vector3 Scale, Quaternion Rotation, Vector3 Position) 
            GetAnimMotionForCurrentFrame(HKX.HKASkeleton s, int boneIndex, 
            HKX.Transform skeleTransform, bool additiveBlend)
        {
            Vector3 scale = Vector3.One;
            Quaternion rotation = Quaternion.Identity;
            Vector3 position = Vector3.Zero;

            float frame = (((float)PlaybackCursor.GUICurrentFrame % CurrentAnimationFrameCount + CurrentBlock)) % CurrentAnimFramesPerBlock;

            if (BoneToTransformTrackMap[CurrentBlock].ContainsKey(boneIndex))
            {
                var thisBlockTransformTrack = BoneToTransformTrackMap[CurrentBlock][boneIndex];

                var thisBlockMotion = GetTransformTrackValueForFrame(thisBlockTransformTrack, frame, skeleTransform, additiveBlend);

                // Blend between blocks
                if (frame >= CurrentAnimFramesPerBlock - 1 && CurrentBlock < CurrentAnimationTracks.Count - 1)
                {
                    var nextBlockTransformTrack = BoneToTransformTrackMap[CurrentBlock + 1][boneIndex];

                    var nextBlockMotion = GetTransformTrackValueForFrame(nextBlockTransformTrack, 1, skeleTransform, additiveBlend);

                    float frameLerp = (frame % 1);

                    scale.X = MathHelper.Lerp(thisBlockMotion.Scale.X, nextBlockMotion.Scale.X, frameLerp);
                    scale.Y = MathHelper.Lerp(thisBlockMotion.Scale.Y, nextBlockMotion.Scale.Y, frameLerp);
                    scale.Z = MathHelper.Lerp(thisBlockMotion.Scale.Z, nextBlockMotion.Scale.Z, frameLerp);

                    rotation.X = MathHelper.Lerp(thisBlockMotion.Rotation.X, nextBlockMotion.Rotation.X, frameLerp);
                    rotation.Y = MathHelper.Lerp(thisBlockMotion.Rotation.Y, nextBlockMotion.Rotation.Y, frameLerp);
                    rotation.Z = MathHelper.Lerp(thisBlockMotion.Rotation.Z, nextBlockMotion.Rotation.Z, frameLerp);
                    rotation.W = MathHelper.Lerp(thisBlockMotion.Rotation.W, nextBlockMotion.Rotation.W, frameLerp);

                    position.X = MathHelper.Lerp(thisBlockMotion.Position.X, nextBlockMotion.Position.X, frameLerp);
                    position.Y = MathHelper.Lerp(thisBlockMotion.Position.Y, nextBlockMotion.Position.Y, frameLerp);
                    position.Z = MathHelper.Lerp(thisBlockMotion.Position.Z, nextBlockMotion.Position.Z, frameLerp);
                }
                // Blend between loops
                else if (frame >= CurrentAnimationFrameCount - 1)
                {
                    var firstBlockTransformTrack = BoneToTransformTrackMap[0][boneIndex];

                    var nextBlockMotion = GetTransformTrackValueForFrame(firstBlockTransformTrack, 0, skeleTransform, additiveBlend);

                    float frameLerp = (frame % 1);

                    scale.X = MathHelper.Lerp(thisBlockMotion.Scale.X, nextBlockMotion.Scale.X, frameLerp);
                    scale.Y = MathHelper.Lerp(thisBlockMotion.Scale.Y, nextBlockMotion.Scale.Y, frameLerp);
                    scale.Z = MathHelper.Lerp(thisBlockMotion.Scale.Z, nextBlockMotion.Scale.Z, frameLerp);

                    rotation.X = MathHelper.Lerp(thisBlockMotion.Rotation.X, nextBlockMotion.Rotation.X, frameLerp);
                    rotation.Y = MathHelper.Lerp(thisBlockMotion.Rotation.Y, nextBlockMotion.Rotation.Y, frameLerp);
                    rotation.Z = MathHelper.Lerp(thisBlockMotion.Rotation.Z, nextBlockMotion.Rotation.Z, frameLerp);
                    rotation.W = MathHelper.Lerp(thisBlockMotion.Rotation.W, nextBlockMotion.Rotation.W, frameLerp);

                    position.X = MathHelper.Lerp(thisBlockMotion.Position.X, nextBlockMotion.Position.X, frameLerp);
                    position.Y = MathHelper.Lerp(thisBlockMotion.Position.Y, nextBlockMotion.Position.Y, frameLerp);
                    position.Z = MathHelper.Lerp(thisBlockMotion.Position.Z, nextBlockMotion.Position.Z, frameLerp);
                }
                else
                {
                    scale = thisBlockMotion.Scale;
                    rotation = thisBlockMotion.Rotation;
                    position = thisBlockMotion.Position;
                }
            }

            return (scale, rotation, position);
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

                    var trackCheck = !isJustSkeleton ? GetTransformTrackOfBone(s, parentBone) : null;

                    if (isJustSkeleton || trackCheck == null)
                    {
                        HKX.Transform t = skeleTransform;

                        thisBone.Item2 *= new Vector3(
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
                        var motion = GetAnimMotionForCurrentFrame(s, parentBone, skeleTransform, additiveBlend);

                        //result *= Matrix.CreateScale(scaleX, scaleY, scaleZ);
                        thisBone.Item2 *= motion.Scale;
                        thisBone.Item1 *= Matrix.CreateFromQuaternion(motion.Rotation);
                        thisBone.Item1 *= Matrix.CreateTranslation(motion.Position);
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
            //var HkxBonePositions = new List<Vector3>();
            HkxBoneScales = new List<Vector3>();
            HkxBonePrimitives = new List<DbgPrimSolidBone>();
            HkxBoneMatrices = new List<Matrix>();
            HkxBoneMatrices_Reference = new List<Matrix>();
            HkxBoneParentMatrices_Reference = new List<Matrix>();
            float frame = PlaybackCursor != null ? Math.Min((float)PlaybackCursor.GUICurrentFrame, CurrentAnimationFrameCount) : 0;

            var alreadyCalculatedBones = new Dictionary<int, (Matrix, Vector3)>();

            for (int i = 0; i < HkxSkeleton.Transforms.Size; i++)
            {
                var parentMatrix = GetBoneParentMatrixHavok(isJustSkeleton: true, HkxSkeleton, (short)i, (frame % CurrentAnimFramesPerBlock), alreadyCalculatedBones);
                HkxBoneParentMatrices.Add(parentMatrix.Item1);
                HkxBoneParentMatrices_Reference.Add(parentMatrix.Item1);
                //HkxBonePositions.Add(Vector3.Transform(Vector3.Zero, parentMatrix.Item1));
                HkxBoneScales.Add(parentMatrix.Item2);
            }
            //int boneIndex = 0;
            for (int i = 0; i < HkxSkeleton.Transforms.Size; i++)
            {
                var sktr = HkxSkeleton.Transforms[i];
                var boneLength = new Vector3(sktr.Position.Vector.X, sktr.Position.Vector.Y, sktr.Position.Vector.Z).Length();

                if (HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data >= 0)
                {
                    var realMatrix = HkxBoneParentMatrices[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data];//Matrix.CreateFromQuaternion(boneRot) * Matrix.CreateTranslation(HkxBonePositions[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data]);
                    var m = Matrix.CreateScale(HkxBoneScales[i]) * realMatrix;
                    HkxBoneMatrices.Add(m);
                    HkxBoneMatrices_Reference.Add(m);
                    //var boneLength = (HkxBonePositions[i/*boneIndex*/] - HkxBonePositions[HkxSkeleton.ParentIndices.GetArrayData().Elements[i].data]).Length();
                    
                    var newBonePrim = new DbgPrimSolidBone(isHkx: true, HkxSkeleton.Bones[i].Name.GetString(), new Transform(realMatrix), Quaternion.Identity, Math.Min(boneLength / 8, 0.25f), boneLength, Color.Yellow);
                    DBG.AddPrimitive(newBonePrim);
                    HkxBonePrimitives.Add(newBonePrim);

                }
                else
                {
                    HkxBoneMatrices.Add(Matrix.CreateScale(HkxBoneScales[i]) * HkxBoneParentMatrices[i]);
                    HkxBoneMatrices_Reference.Add(Matrix.CreateScale(HkxBoneScales[i]) * HkxBoneParentMatrices[i]);
                    var newBonePrim = new DbgPrimSolidBone(isHkx: true, HkxSkeleton.Bones[i].Name.GetString(), new Transform(HkxBoneParentMatrices[i]), Quaternion.Identity, Math.Min(boneLength / 8, 0.25f), boneLength, Color.Yellow);
                    DBG.AddPrimitive(newBonePrim);
                    HkxBonePrimitives.Add(newBonePrim);
                }
                //boneIndex++;

                CopyHavokMatrixToOtherMatrices(i);
            }
        }

        private static void CalculateAnimation(float totalTime, float frameNum, bool lockToTpose)
        {
            float frame = frameNum % CurrentAnimationFrameCount;

            if (frame != LastHkxFrameCalculated || lockToTpose)
            {
                if (RootMotionFrames != null && !lockToTpose && 
                    !(RootMotionFrames.Count == 0 || RootMotionDuration == 0 || !EnableRootMotion))
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

            if (DummyPolyManager.StationaryDummyPolys != null)
            {
                DummyPolyManager.StationaryDummyPolys.UpdateWithBoneMatrix(
                    Matrix.CreateRotationY(CurrentRootMotionDisplacement.W) 
                    * Matrix.CreateTranslation(
                        CurrentRootMotionDisplacement.X, 
                        CurrentRootMotionDisplacement.Y, 
                        CurrentRootMotionDisplacement.Z));
            }

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
                    parentMatrix = GetBoneParentMatrixHavok(isJustSkeleton: lockToTpose,
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

                            if (HkxBoneToFlverBoneMap.ContainsKey(i))
                            {
                                var flverBoneIndex = HkxBoneToFlverBoneMap[i];
                                FlverBonePrims[flverBoneIndex].Transform = new Transform(realMatrix * CurrentRootMotionMatrix);
                            }

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
                                * HkxBoneParentMatrices[i]
                                * CurrentRootMotionMatrix;

                            if (HkxBoneToFlverBoneMap.ContainsKey(i))
                            {
                                var flverBoneIndex = HkxBoneToFlverBoneMap[i];
                                FlverBonePrims[flverBoneIndex].Transform = new Transform(HkxBoneParentMatrices[i] * CurrentRootMotionMatrix);
                            }

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

            //foreach (var b in flver.Bones)
            //{
            //    if (Math.Abs(b.Scale.X - 1) > 0.01f || Math.Abs(b.Scale.Y - 1) > 0.01f || Math.Abs(b.Scale.Z - 1) > 0.01f)
            //    {
            //        Console.WriteLine("asdf");
            //    }
            //}

            //foreach (var m in flver.Meshes)
            //{
            //    foreach (var v in m.Vertices)
            //    {
            //        if (v.BoneWeights != null)
            //        {
            //            float weightAddition = v.BoneWeights[0] + v.BoneWeights[1] + v.BoneWeights[2] + v.BoneWeights[3];
            //            //Console.WriteLine(flver.Materials[m.MaterialIndex].Name + " " + weightAddition.ToString());

            //            for (int i = 0; i < 4; i++)
            //            {
            //                if (v.BoneWeights[i] > 0 && v.BoneIndices[0] == 0)
            //                    Console.WriteLine(flver.Materials[m.MaterialIndex].Name);
            //            }
            //        }
                    
            //    }
            //}

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

            DummyPolyManager.LoadDummiesFromFLVER(flver);

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
            FlverBoneTPoseMatricesEnable = new List<bool>();
            //List<Vector3> bonePos = new List<Vector3>();


            foreach (var b in flver.Bones)
            {
                var parentMatrix = GetBoneParentMatrix(b);

                FlverBoneTPoseMatrices.Add(parentMatrix);
                //FlverBoneTPoseMatricesEnable.Add(b.Unk3C == 0);
                FlverBoneTPoseMatricesEnable.Add(true);

                //bonePos.Add(Vector3.Transform(Vector3.Zero, parentMatrix));
            }
            int boneIndex = 0;
            foreach (var b in flver.Bones)
            {
                if (b.ParentIndex >= 0)
                {
                    var boneTransform = new Transform(FlverBoneTPoseMatrices[boneIndex]);
                    var boneLength = flver.Bones[boneIndex].Translation.Length();
                    var prim = new DbgPrimSolidBone(isHkx: false, getBoneSpacePrefix(b) + b.Name, boneTransform, Quaternion.Identity, boneLength / 8, boneLength, Color.Purple);

                    prim.Children.Add(new DbgPrimWireBox(new Transform(Matrix.Identity),
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

        public static void CreateMenuBarNPCOrPlayerSettings(TaeEditor.TaeMenuBarBuilder menu)
        {
            Main.TAE_EDITOR.GameWindowAsForm.Invoke(new Action(() =>
            {
                if (!IsPlayerLoaded)
                {
                    menu["NPC Settings"].Visible = true;
                    menu["NPC Settings"].Enabled = true;

                    menu["Player Settings"].Visible = false;
                    menu["Player Settings"].Enabled = false;

                    menu.ClearItem("NPC Settings");

                    var modelMaskPresetDict = new Dictionary<string, Action>();

                    foreach (var kvp in GFX.ModelDrawer.MaskPresets)
                    {
                        modelMaskPresetDict.Add(kvp.Key, () =>
                        {
                            GFX.ModelDrawer.NPC_SelectedMaskPreset = kvp.Key;
                            GFX.ModelDrawer.DefaultAllMaskValues();
                        });
                    }

                    menu.AddItem("NPC Settings", "Model Mask Preset", modelMaskPresetDict, () => GFX.ModelDrawer.NPC_SelectedMaskPreset);

                    menu["NPC Settings"].Font = new System.Drawing.Font(
                        menu["NPC Settings"].Font.Name,
                        menu["NPC Settings"].Font.Size,
                        System.Drawing.FontStyle.Bold);
                }
                else
                {
                    menu["NPC Settings"].Visible = false;
                    menu["NPC Settings"].Enabled = false;

                    menu["Player Settings"].Visible = true;
                    menu["Player Settings"].Enabled = true;

                    menu.ClearItem("Player Settings");

                    ChrAsm.LoadFace("FC_M_0000.partsbnd");

                    // TODO: TEST BLOODBORNE
                    if (CurrentHkxVariation == HKX.HKXVariation.HKXDS3 || CurrentHkxVariation == HKX.HKXVariation.HKXBloodBorne)
                    {
                        ChrAsm.LoadFaceGen("FG_A_0100.partsbnd");
                        //ChrAsm.LoadHair("HR_A_0002.partsbnd");
                    }
                    else
                    {
                        ChrAsm.LoadFaceGen("FG_A_0000.partsbnd");
                        //ChrAsm.LoadHair("HR_M_0001.partsbnd");

                        var fgbnd = BND3.Read($@"{InterrootPath}\facegen\FaceGen.fgbnd");
                        foreach (var f in fgbnd.Files)
                        {
                            if (TPF.Is(f.Bytes))
                            {
                                TexturePool.AddTpf(TPF.Read(f.Bytes));
                            }
                        }

                        GFX.ModelDrawer.RequestTextureLoad();
                    }
                    

                    FmgManager.LoadAllFMG();

                    Dictionary<string, Action> weaponChoicesR = new Dictionary<string, Action>();
                    Dictionary<string, Action> weaponChoicesL = new Dictionary<string, Action>();

                    weaponChoicesR.Add("None", () =>
                    {
                        ChrAsm.EquipRWeapon = -1;
                        ChrAsm.UpdateEquipment();
                    });

                    weaponChoicesL.Add("None", () =>
                    {
                        ChrAsm.EquipLWeapon = -1;
                        ChrAsm.UpdateEquipment();
                    });

                    foreach (var kvp in FmgManager.WeaponNames)
                    {
                        if (string.IsNullOrWhiteSpace(kvp.Value))
                            continue;

                        if (CurrentHkxVariation == HKX.HKXVariation.HKXDS1)
                        {
                            if ((kvp.Key % 1000) != 0)
                                continue;
                        }
                        else
                        {
                            if ((kvp.Key % 10000) != 0)
                                continue;
                        }



                        if (!weaponChoicesR.ContainsKey(kvp.Value))
                            weaponChoicesR.Add(kvp.Value, () =>
                            {
                                ChrAsm.EquipRWeapon = kvp.Key;
                                ChrAsm.UpdateEquipment();
                            });

                        if (!weaponChoicesL.ContainsKey(kvp.Value))
                            weaponChoicesL.Add(kvp.Value, () =>
                            {
                                ChrAsm.EquipLWeapon = kvp.Key;
                                ChrAsm.UpdateEquipment();
                            });
                    }

                    menu.AddItem("Player Settings", "R Weapon Select", weaponChoicesR, 
                        () => (ChrAsm.EquipRWeapon >= 0 && FmgManager.WeaponNames.ContainsKey((int)ChrAsm.EquipRWeapon)) 
                        ? FmgManager.WeaponNames[(int)ChrAsm.EquipRWeapon] : "None");

                    menu.AddItem("Player Settings", "L Weapon Select", weaponChoicesL, 
                        () => (ChrAsm.EquipLWeapon >= 0 && FmgManager.WeaponNames.ContainsKey((int)ChrAsm.EquipLWeapon)) 
                        ? FmgManager.WeaponNames[(int)ChrAsm.EquipLWeapon] : "None");

                    menu.AddItem("Player Settings", "Preview L Weapon Hitboxes", () => DummyPolyManager.IsViewingLeftHandHit, b => DummyPolyManager.IsViewingLeftHandHit = b);

                    menu.AddSeparator("Player Settings");

                    Dictionary<string, Action> hdChoices = new Dictionary<string, Action>();
                    Dictionary<string, Action> bdChoices = new Dictionary<string, Action>();
                    Dictionary<string, Action> amChoices = new Dictionary<string, Action>();
                    Dictionary<string, Action> lgChoices = new Dictionary<string, Action>();

                    hdChoices.Add("None", () =>
                    {
                        ChrAsm.EquipHead = -1;
                        ChrAsm.UpdateEquipment();
                    });

                    bdChoices.Add("None", () =>
                    {
                        ChrAsm.EquipBody = -1;
                        ChrAsm.UpdateEquipment();
                    });

                    amChoices.Add("None", () =>
                    {
                        ChrAsm.EquipArms = -1;
                        ChrAsm.UpdateEquipment();
                    });

                    lgChoices.Add("None", () =>
                    {
                        ChrAsm.EquipLegs = -1;
                        ChrAsm.UpdateEquipment();
                    });

                    foreach (var kvp in FmgManager.ProtectorNames_HD)
                    {
                        if (string.IsNullOrWhiteSpace(kvp.Value))
                            continue;

                        if (!hdChoices.ContainsKey(kvp.Value))
                            hdChoices.Add(kvp.Value, () =>
                            {
                                ChrAsm.EquipHead = kvp.Key;
                                ChrAsm.UpdateEquipment();
                            });
                    }

                    foreach (var kvp in FmgManager.ProtectorNames_BD)
                    {
                        if (string.IsNullOrWhiteSpace(kvp.Value))
                            continue;

                        if (!bdChoices.ContainsKey(kvp.Value))
                            bdChoices.Add(kvp.Value, () =>
                            {
                                ChrAsm.EquipBody = kvp.Key;
                                ChrAsm.UpdateEquipment();
                            });
                    }

                    foreach (var kvp in FmgManager.ProtectorNames_AM)
                    {
                        if (string.IsNullOrWhiteSpace(kvp.Value))
                            continue;

                        if (!amChoices.ContainsKey(kvp.Value))
                            amChoices.Add(kvp.Value, () =>
                            {
                                ChrAsm.EquipArms = kvp.Key;
                                ChrAsm.UpdateEquipment();
                            });
                    }

                    foreach (var kvp in FmgManager.ProtectorNames_LG)
                    {
                        if (string.IsNullOrWhiteSpace(kvp.Value))
                            continue;

                        if (!lgChoices.ContainsKey(kvp.Value))
                            lgChoices.Add(kvp.Value, () =>
                            {
                                ChrAsm.EquipLegs = kvp.Key;
                                ChrAsm.UpdateEquipment();
                            });
                    }

                    menu.AddItem("Player Settings", "Head Select", hdChoices,
                        () => (ChrAsm.EquipHead >= 0 && FmgManager.ProtectorNames_HD.ContainsKey((int)ChrAsm.EquipHead))
                        ? FmgManager.ProtectorNames_HD[(int)ChrAsm.EquipHead] : "None");

                    menu.AddItem("Player Settings", "Body Select", bdChoices,
                       () => (ChrAsm.EquipBody >= 0 && FmgManager.ProtectorNames_BD.ContainsKey((int)ChrAsm.EquipBody))
                       ? FmgManager.ProtectorNames_BD[(int)ChrAsm.EquipBody] : "None");

                    menu.AddItem("Player Settings", "Arms Select", amChoices,
                       () => (ChrAsm.EquipArms >= 0 && FmgManager.ProtectorNames_AM.ContainsKey((int)ChrAsm.EquipArms))
                       ? FmgManager.ProtectorNames_AM[(int)ChrAsm.EquipArms] : "None");

                    menu.AddItem("Player Settings", "Legs Select", lgChoices,
                       () => (ChrAsm.EquipLegs >= 0 && FmgManager.ProtectorNames_LG.ContainsKey((int)ChrAsm.EquipLegs))
                       ? FmgManager.ProtectorNames_LG[(int)ChrAsm.EquipLegs] : "None");

                    menu["Player Settings"].Font = new System.Drawing.Font(
                        menu["Player Settings"].Font.Name,
                        menu["Player Settings"].Font.Size,
                        System.Drawing.FontStyle.Bold);
                }

                
            }));
        }

        public static void CreateMenuBarViewportSettings(TaeEditor.TaeMenuBarBuilder menu)
        {
            Main.TAE_EDITOR.GameWindowAsForm.Invoke(new Action(() =>
            {
                menu.ClearItem("Scene");

                menu.AddItem("Scene", "Render Meshes", () => !GFX.HideFLVERs,
                    b => GFX.HideFLVERs = !b);

                menu.AddItem("Scene/Toggle Individual Meshes", "Show All", () =>
                {
                    foreach (var model in GFX.ModelDrawer.Models)
                    {
                        foreach (var sm in model.GetSubmeshes())
                        {
                            sm.IsVisible = true;
                        }
                    }
                });

                menu.AddItem("Scene/Toggle Individual Meshes", "Hide All", () =>
                {
                    foreach (var model in GFX.ModelDrawer.Models)
                    {
                        foreach (var sm in model.GetSubmeshes())
                        {
                            sm.IsVisible = false;
                        }
                    }
                });

                menu.AddSeparator("Scene/Toggle Individual Meshes");

                foreach (var model in GFX.ModelDrawer.Models)
                {
                    int i = 0;
                    foreach (var sm in model.GetSubmeshes())
                        menu.AddItem("Scene/Toggle Individual Meshes", $"{++i}: '{sm.MaterialName}'", () => sm.IsVisible, b => sm.IsVisible = b);
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

                menu.AddItem("Scene/Toggle By Model Mask", "Show All", () =>
                {
                    foreach (var kvp in modelMaskMap)
                    {
                        foreach (var sm in kvp.Value)
                        {
                            sm.IsVisible = true;
                        }
                    }
                });

                menu.AddItem("Scene/Toggle By Model Mask", "Hide All", () =>
                {
                    foreach (var kvp in modelMaskMap)
                    {
                        foreach (var sm in kvp.Value)
                        {
                            sm.IsVisible = false;
                        }
                    }
                });

                menu.AddSeparator("Scene/Toggle By Model Mask");

                foreach (var kvp in modelMaskMap.OrderBy(asdf => asdf.Key))
                {
                    if (kvp.Key >= 0)
                    {
                        menu.AddItem("Scene/Toggle By Model Mask",
                        $"Model Mask {kvp.Key}",
                        () => GFX.ModelDrawer.Mask[kvp.Key],
                        b => GFX.ModelDrawer.Mask[kvp.Key] = b);
                    }
                    
                }

                menu.AddItem("Scene", "Render HKX Skeleton (Yellow)", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.HkxBone],
                    b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.HkxBone] = b);

                menu.AddItem("Scene", "Render FLVER Skeleton (Purple)", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone],
                    b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone] = b);

                //menu.AddItem("Scene", "Render FLVER Skeleton Bounding Boxes (Orange)", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox],
                //    b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox] = b);

                menu.AddItem("Scene", "Render DummyPoly (Red/Green/Blue)", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
                    b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = b);

                //menu.AddItem("Scene", "[TEMP] Render DummyPoly Hit Spheres", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPolyHelper],
                //    b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPolyHelper] = b);

                //menu.AddItem("3D Preview/Items In Scene", "Render DummyPoly ID Tags", () => DBG.CategoryEnableDbgLabelDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
                //    b => DBG.CategoryEnableDbgLabelDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = b);

                //menu.AddItem("3D Preview/Items In Scene/Toggle DummyPoly By ID", $"Show All", () =>
                //{
                //    foreach (var prim in DBG.GetPrimitives().Where(p => p.Category == DebugPrimitives.DbgPrimCategory.DummyPoly))
                //    {
                //        if (prim is DbgPrimDummyPolyCluster cluster)
                //        {
                //            cluster.EnableDraw = true;
                //        }
                //    }
                //});

                //menu.AddItem("3D Preview/Items In Scene/Toggle DummyPoly By ID", $"Hide All", () =>
                //{
                //    foreach (var prim in DBG.GetPrimitives().Where(p => p.Category == DebugPrimitives.DbgPrimCategory.DummyPoly))
                //    {
                //        if (prim is DbgPrimDummyPolyCluster cluster)
                //        {
                //            cluster.EnableDraw = false;
                //        }
                //    }
                //});

                //menu.AddSeparator("3D Preview/Items In Scene/Toggle DummyPoly By ID");

                //foreach (var prim in DBG.GetPrimitives().Where(p => p.Category == DebugPrimitives.DbgPrimCategory.DummyPoly))
                //{
                //    if (prim is DbgPrimDummyPolyCluster cluster)
                //    {
                //        menu.AddItem("3D Preview/Items In Scene/Toggle DummyPoly By ID", $"{cluster.ID}", () => cluster.EnableDraw, b => cluster.EnableDraw = b);
                //    }
                //}
            }));
        }
    }
}
