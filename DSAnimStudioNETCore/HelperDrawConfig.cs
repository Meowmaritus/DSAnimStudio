using System;
using DSAnimStudio.ImguiOSD;
using DSAnimStudio.TaeEditor;
using ImGuiNET;

namespace DSAnimStudio
{
    public struct HelperDrawConfig
    {
        public bool MASTER;

        public bool EnableFlverBoneTransforms;
        public bool EnableFlverBoneLines;
        public bool EnableFlverBoneNames;
        public bool EnableFlverBoneBoxes;
        public bool EnableHkxBoneTransforms;
        public bool EnableHkxBoneNames;
        public bool EnableBoneGlue;
        public bool EnableDummyPolyTransforms;

        public bool EnableDummyPolyIDs;
        public bool DummyPolyIDsAreGlobal;

        public bool EnableSoundInstanceTransforms;
        public bool EnableSoundInstanceNames;

        public bool EnableChrHitCapsule;
        public float ChrHitCapsuleOpacity;

        public bool EnableChrHitCapsule_StartOfAnim;
        public float ChrHitCapsuleOpacity_StartOfAnim;

        public bool EnableAttackDistanceLine;
        public float AttackDistanceLineOpacity;
        public float AttackDistanceLineOpacity_Secondary;

        public bool EnableAttackDistanceText;
        public float AttackDistanceTextOpacity;
        public float AttackDistanceTextOpacity_Secondary;

        public bool EnableRootMotionStartTransform;
        public bool EnableRootMotionTransform;

        public bool EnableRootMotionTrail;
        public float RootMotionTrailUpdateRate;
        public int RootMotionTrailSampleMax;
        public const int RootMotionTrailSampleMaxInfinityValue = 1000;

        public float AttackBehaviorHitboxOpacity;
        public float CommonBehaviorHitboxOpacity;
        public float ThrowAttackBehaviorHitboxOpacity;
        public float PCBehaviorHitboxOpacity;
        public float DummyPolyClusterOpacity;
        public float StationaryDummyPolyOpacity;

        public int DummyPolyClusterThreshold;

        public bool EnableAttackHitShapes;
        public bool EnableAttackNames;
        public bool EnableDummyPolyTexts;

        public bool BoostTransformVisibility_Bones;
        public bool BoostTransformVisibility_DummyPoly;
        public bool BoostTransformVisibility_Bones_IgnoreAlpha;
        public bool BoostTransformVisibility_DummyPoly_IgnoreAlpha;
        public float TransformScale;
        public float TransformScale_Bones;
        public float TransformScale_DummyPoly;
        public float ExtraTransformVisibilityThickness;



        public static readonly HelperDrawConfig Default = new()
        {
            MASTER = true,

            EnableRootMotionStartTransform = true,
            EnableRootMotionTransform = true,
            EnableRootMotionTrail = true,
            RootMotionTrailUpdateRate = 30,
            RootMotionTrailSampleMax = 200,

            AttackBehaviorHitboxOpacity = 1,
            CommonBehaviorHitboxOpacity = 0.5f,
            ThrowAttackBehaviorHitboxOpacity = 0.5f,
            PCBehaviorHitboxOpacity = 1,

            DummyPolyClusterOpacity = 0.25f,
            DummyPolyClusterThreshold = 5,
            StationaryDummyPolyOpacity = 0.25f,

            ChrHitCapsuleOpacity = 0.8f,
            ChrHitCapsuleOpacity_StartOfAnim = 0.4f,

            AttackDistanceLineOpacity = 1f,
            AttackDistanceTextOpacity = 1f,
            AttackDistanceLineOpacity_Secondary = 0.5f,
            AttackDistanceTextOpacity_Secondary = 0.5f,

            EnableAttackHitShapes = true,
            EnableAttackNames = true,

            EnableDummyPolyTexts = true,

            EnableXRayMode = true,

            BoostTransformVisibility_Bones = false,
            BoostTransformVisibility_DummyPoly = false,
            TransformScale = 1,
            TransformScale_Bones = 1,
            TransformScale_DummyPoly = 1,
            BoostTransformVisibility_Bones_IgnoreAlpha = false,
            BoostTransformVisibility_DummyPoly_IgnoreAlpha = false,

            ExtraTransformVisibilityThickness = 0.0801f, // the 0.0001 is added to the whole value as a sort of "epsilon" value to prevent z fighting 
        };
        
        public bool EnableXRayMode;


        public void ShowImGui(ref bool anyFieldFocused)
        {
            MASTER =
                MenuBar.CheckboxDual("[MASTER TOGGLE] ALL HELPERS",
                    MASTER,
                    enabled: true);

            EnableDummyPolyTexts =
                MenuBar.CheckboxDual("[MASTER TOGGLE] DummyPoly Text Displays",
                    EnableDummyPolyTexts,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperDummyPolyTexts);

            EnableFlverBoneLines =
                MenuBar.CheckboxDual("FLVER Bone Lines",
                    EnableFlverBoneLines,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperFlverBoneLines);
            
            EnableFlverBoneTransforms =
                MenuBar.CheckboxDual("FLVER Bone Transforms",
                    EnableFlverBoneTransforms,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperFlverBoneTransforms);

            EnableFlverBoneNames =
                MenuBar.CheckboxDual("FLVER Bone Names",
                    EnableFlverBoneNames,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperFlverBoneTexts);

            EnableFlverBoneBoxes =
                MenuBar.CheckboxDual("FLVER Bone Boxes",
                    EnableFlverBoneBoxes,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperFlverBoneBoundingBox);
            
            EnableHkxBoneTransforms =
                MenuBar.CheckboxDual("HKX Bone Transforms",
                    EnableHkxBoneTransforms,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperHkxBone);

            EnableHkxBoneNames =
                MenuBar.CheckboxDual("HKX Bone Names",
                    EnableHkxBoneNames,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperHkxBone);

            EnableBoneGlue =
                MenuBar.CheckboxDual("Bone Glue Joints",
                    EnableBoneGlue,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperBoneGlue);

            EnableDummyPolyTransforms =
                MenuBar.CheckboxDual("DummyPoly Transforms",
                    EnableDummyPolyTransforms,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperDummyPolyTransforms);

            EnableDummyPolyIDs =
                MenuBar.CheckboxDual("DummyPoly IDs",
                    EnableDummyPolyIDs,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperDummyPolyTexts);

            DummyPolyIDsAreGlobal = 
                MenuBar.CheckboxDual("Show c0000 Parts DummyPoly IDs With Global Offsets (1000+)", 
                    DummyPolyIDsAreGlobal);

            ImGui.Separator();

            BoostTransformVisibility_Bones =
                MenuBar.CheckboxDual("Boost Transform Visibility - Bones",
                    BoostTransformVisibility_Bones,
                    enabled: true);

            BoostTransformVisibility_Bones_IgnoreAlpha =
                MenuBar.CheckboxDual("Boost Transform Visibility - Bones - Force Opaque",
                    BoostTransformVisibility_Bones_IgnoreAlpha,
                    enabled: true);

            BoostTransformVisibility_DummyPoly =
                MenuBar.CheckboxDual("Boost Transform Visibility - DummyPoly",
                    BoostTransformVisibility_DummyPoly,
                    enabled: true);

            BoostTransformVisibility_DummyPoly_IgnoreAlpha =
                MenuBar.CheckboxDual("Boost Transform Visibility - DummyPoly - Force Opaque",
                    BoostTransformVisibility_DummyPoly_IgnoreAlpha,
                    enabled: true);

            float thickness = (ExtraTransformVisibilityThickness * 100) - 0.01f;

            ImGui.DragFloat("Boost Transform Visibility - Thickness", ref thickness, 0.1f, 0, 500);
            if (thickness != ExtraTransformVisibilityThickness)
            {
                ExtraTransformVisibilityThickness = ((thickness + 0.01f) / 100);
            }
            DoOpacitySlider("Transform Render Scale - Bones", ref TransformScale_Bones, 0, 400);
            DoOpacitySlider("Transform Render Scale - DummyPoly", ref TransformScale_DummyPoly, 0, 400);
            DoOpacitySlider("Transform Render Scale - Other", ref TransformScale, 0, 400);

            ImGui.Separator();

            EnableChrHitCapsule =
                MenuBar.CheckboxDual("Character Hit Capsule",
                    EnableChrHitCapsule,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperChrHitCapsule);

            DoOpacitySlider("Character Hit Capsule Opacity", ref ChrHitCapsuleOpacity);

            EnableChrHitCapsule_StartOfAnim =
                MenuBar.CheckboxDual("Character Hit Capsule (Start Of Anim)",
                    EnableChrHitCapsule_StartOfAnim,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperChrHitCapsule);

            DoOpacitySlider("Character Hit Capsule (Start Of Anim) Opacity", ref ChrHitCapsuleOpacity_StartOfAnim);

            ImGui.Separator();

            bool prevShowAttackHitShapes = EnableAttackHitShapes;
            bool prevShowAttackNames = EnableAttackNames;

            EnableAttackHitShapes =
                MenuBar.CheckboxDual("Attack Hit Shapes",
                    EnableAttackHitShapes,
                    enabled: true);

            EnableAttackNames =
                MenuBar.CheckboxDual("Attack Names",
                    EnableAttackNames,
                    enabled: true);

            if (!EnableAttackHitShapes && prevShowAttackHitShapes && !EnableAttackNames)
            {
                EnableAttackHitShapes = true;
            }

            if (!EnableAttackNames && prevShowAttackNames && !EnableAttackHitShapes)
            {
                EnableAttackNames = true;
            }

            ImGui.Separator();

            EnableAttackDistanceLine =
                MenuBar.CheckboxDual("Attack Distance Line",
                    EnableAttackDistanceLine,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperAttackDistanceLine);

            DoOpacitySlider("Attack Distance Line Opacity", ref AttackDistanceLineOpacity);
            DoOpacitySlider("Attack Distance Line Opacity (Secondary)", ref AttackDistanceLineOpacity_Secondary);

            EnableAttackDistanceText =
                MenuBar.CheckboxDual("Attack Distance Text",
                    EnableAttackDistanceText,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperAttackDistanceText);

            DoOpacitySlider("Attack Distance Text Opacity", ref AttackDistanceTextOpacity);
            DoOpacitySlider("Attack Distance Text Opacity (Secondary)", ref AttackDistanceTextOpacity_Secondary);



            ImGui.Separator();
            
            EnableSoundInstanceTransforms =
                MenuBar.CheckboxDual("Sound Instance Transforms",
                    EnableSoundInstanceTransforms,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperSoundEventTransforms);
            
            EnableSoundInstanceNames =
                MenuBar.CheckboxDual("Sound Instance Names",
                    EnableSoundInstanceNames,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperSoundEventTexts);
            
            ImGui.Separator();
            
            EnableRootMotionStartTransform =
                MenuBar.CheckboxDual("Root Motion Start Transform",
                    EnableRootMotionStartTransform,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperRootMotionStartLocation);
            
            EnableRootMotionTransform =
                MenuBar.CheckboxDual("Root Motion Transform",
                    EnableRootMotionTransform,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperRootMotionCurrentLocation);
            
            EnableRootMotionTrail =
                MenuBar.CheckboxDual("Root Motion Trail",
                    EnableRootMotionTrail,
                    enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperRootMotionTrail);

            float rootMotionUpdateRate = RootMotionTrailUpdateRate;
            ImGui.SliderFloat("Root Motion Path Update Rate", ref rootMotionUpdateRate, 1, 300, "%.3f Hz");
            anyFieldFocused |= ImGui.IsItemActive();
            RootMotionTrailUpdateRate = Math.Max(Math.Min(rootMotionUpdateRate, 300), 1);

            int rootMotionMaxSamples = RootMotionTrailSampleMax;
            ImGui.SliderInt("Root Motion Path Sample Max", ref rootMotionMaxSamples, 1, RootMotionTrailSampleMaxInfinityValue, rootMotionMaxSamples >= RootMotionTrailSampleMaxInfinityValue ? "%d (Unlimited)" : "Up to %d");
            anyFieldFocused |= ImGui.IsItemActive();
            RootMotionTrailSampleMax = Math.Max(Math.Min(rootMotionMaxSamples, RootMotionTrailSampleMaxInfinityValue), 1);

            ImGui.Separator();

            // RemoManager.EnableDummyPrims =
            //     MenuBar.CheckboxDual("Cutscene Dummies",
            //     RemoManager.EnableDummyPrims,
            //     enabled: true, shortcut: "(This Color)", shortcutColor: Microsoft.Xna.Framework.Color.Lime);

            DoOpacitySlider("Attack (Action 1) Hitbox Opacity", ref AttackBehaviorHitboxOpacity);
            DoOpacitySlider("CommonBehavior (Action 5) Hitbox Opacity", ref CommonBehaviorHitboxOpacity);
            DoOpacitySlider("ThrowAttack (Action 304) Hitbox Opacity", ref ThrowAttackBehaviorHitboxOpacity);
            DoOpacitySlider("PCBehavior (Action 307) Hitbox Opacity", ref PCBehaviorHitboxOpacity);
            ImGui.Separator();
            DoOpacitySlider("DummyPoly Cluster Opacity", ref DummyPolyClusterOpacity);
            ImGui.InputInt("DummyPoly Cluster Threshold", ref DummyPolyClusterThreshold);
            if (DummyPolyClusterThreshold < 2)
                DummyPolyClusterThreshold = 2;
            ImGui.Separator();
            DoOpacitySlider("Stationary DummyPoly Opacity", ref StationaryDummyPolyOpacity);
            
            ImGui.Separator();

            EnableXRayMode = MenuBar.CheckboxDual("Helper X-Ray Mode", EnableXRayMode);
        }

        private static void DoOpacitySlider(string name, ref float val, float min = 0, float max = 100)
        {
            float valPercent = val * 100f;
            ImGui.SliderFloat(name, ref valPercent, min, max, "%.0f %%");
            val = valPercent / 100f;
        }
        
        
    }
}