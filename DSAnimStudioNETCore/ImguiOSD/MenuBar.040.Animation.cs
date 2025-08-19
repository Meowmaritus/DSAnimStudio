using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public static partial class MenuBar
    {
        public static void BuildMenuBar_040_Animation(ref bool anyItemFocused, ref bool isAnyMenuExpanded)
        {
            if (ImGui.BeginMenu("Animation"))
            {
                isAnyMenuExpanded = true;

                Tae.Config.LockFramerateToOriginalAnimFramerate = Checkbox(
                    "Lock Cursor to Frame Rate Defined in HKX", Tae.Config.LockFramerateToOriginalAnimFramerate,
                    shortcut: Tae.PlaybackCursor != null
                    ? $"({((int)Math.Round(Tae.PlaybackCursor.CurrentSnapFPS))} FPS)" : null);

                float curPlaybackSpeed = TaeEditor.TaePlaybackCursor.GlobalBasePlaybackSpeed;
                
                curPlaybackSpeed = FloatSlider("Playback Speed",
                    curPlaybackSpeed * 100f, 0, 100, "%.2f%%") / 100f;
                
                if (ImGui.IsItemActive())
                    anyItemFocused = true;

                if (curPlaybackSpeed > 999.99f)
                    curPlaybackSpeed = 999.99f;

                if (curPlaybackSpeed < 0)
                    curPlaybackSpeed = 0;

                TaeEditor.TaePlaybackCursor.GlobalBasePlaybackSpeed = curPlaybackSpeed;

                ImGui.Separator();

                Tae.Config.EnableAnimRootMotion = Checkbox(
                    "Enable Root Motion", Tae.Config.EnableAnimRootMotion);

                Tae.Config.RootMotionTranslationMultiplierXZ = FloatSlider("Root Motion Translation Mult XZ",
                    Tae.Config.RootMotionTranslationMultiplierXZ, 0, 20, "%.2f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                Tae.Config.RootMotionTranslationPowerXZ = FloatSlider("Root Motion Translation Power XZ",
                   Tae.Config.RootMotionTranslationPowerXZ, 0, 2, "%.2f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;

                Tae.Config.RootMotionTranslationMultiplierY = FloatSlider("Root Motion Translation Mult Y",
                    Tae.Config.RootMotionTranslationMultiplierY, 0, 1, "%.2f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                Tae.Config.RootMotionTranslationPowerY = FloatSlider("Root Motion Translation Power Y",
                    Tae.Config.RootMotionTranslationPowerY, 0, 1, "%.2f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;

                Tae.Config.RootMotionRotationMultiplier = FloatSlider("Root Motion Rotation Mult",
                    Tae.Config.RootMotionRotationMultiplier, 0, 1, "%.2f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                Tae.Config.RootMotionRotationPower = FloatSlider("Root Motion Rotation Power",
                    Tae.Config.RootMotionRotationPower, 0, 1, "%.2f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;

                ImGui.Separator();

                if (ClickItem($"Follow Type: Root Motion", shortcut: $"", shortcutColor: Color.Cyan, selected: Tae.Config.CameraFollowType == TaeEditor.TaeConfigFile.CameraFollowTypes.RootMotion))
                {
                    Tae.Config.CameraFollowType = TaeEditor.TaeConfigFile.CameraFollowTypes.RootMotion;
                }

                if (ClickItem($"Follow Type: Body DummyPoly", shortcut: $"", shortcutColor: Color.Cyan, selected: Tae.Config.CameraFollowType == TaeEditor.TaeConfigFile.CameraFollowTypes.BodyDummyPoly))
                {
                    Tae.Config.CameraFollowType = TaeEditor.TaeConfigFile.CameraFollowTypes.BodyDummyPoly;
                }



                Tae.Config.CameraFollowDummyPolyID = IntItem("DummyPoly Follow ID", Tae.Config.CameraFollowDummyPolyID);
                if (ImGui.IsItemActive())
                    anyItemFocused = true;

                ImGui.Separator();

                Tae.Config.CameraFollowsRootMotionZX = Checkbox(
                    "Camera Follow - Translation ZX", Tae.Config.CameraFollowsRootMotionZX, shortcut: "", shortcutColor: Color.White);

                //Tae.Config.CameraFollowsRootMotionZX_Interpolation = FloatSlider(
                //    "Camera Follow - Translation ZX - Interpolation", Tae.Config.CameraFollowsRootMotionZX_Interpolation, 
                //    0, 1, "%f", power: 2f);

                ImGui.Separator();

                Tae.Config.CameraFollowsRootMotionY = Checkbox(
                    "Camera Follow - Translation Y", Tae.Config.CameraFollowsRootMotionY, shortcut: "", shortcutColor: Color.White);

                //Tae.Config.CameraFollowsRootMotionY_Interpolation = FloatSlider(
                //    "Camera Follow - Translation Y - Interpolation", Tae.Config.CameraFollowsRootMotionY_Interpolation,
                //    0, 1, "%f", power: 2f);

                ImGui.Separator();

                Tae.Config.CameraFollowsRootMotionRotation = Checkbox(
                    "Camera Follow - Rotation", Tae.Config.CameraFollowsRootMotionRotation, shortcut: "", shortcutColor: Color.White);

                Tae.Config.CameraFollowsRootMotionRotation_Interpolation = FloatSlider(
                    "Camera Follow - Rotation - Interpolation", Tae.Config.CameraFollowsRootMotionRotation_Interpolation,
                    0, 1, "%f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;

                //ImGui.Separator();

                //Tae.Config.WrapRootMotion = Checkbox(
                //    "Prevent Root Motion From Reaching End Of Grid", Tae.Config.WrapRootMotion);

                ImGui.Separator();
                
                Tae.Config.EnableRumbleCamSmoothing = Checkbox(
                    "Smooth RumbleCam End Transitions", Tae.Config.EnableRumbleCamSmoothing);

                ImGui.Separator();

                Tae.Config.EnableBoneScale_NormalAnims = Checkbox(
                    "Enable Bone Scale Changes (Normal Animations)", Tae.Config.EnableBoneScale_NormalAnims);

                Tae.Config.EnableBoneScale_AdditiveAnims = Checkbox(
                    "Enable Bone Scale Changes (Additive Animations)", Tae.Config.EnableBoneScale_AdditiveAnims);

                Tae.Config.EnableBoneTranslation_NormalAnims = Checkbox(
                    "Enable Bone Translation Changes (Normal Animations)", Tae.Config.EnableBoneTranslation_NormalAnims);

                Tae.Config.EnableBoneTranslation_AdditiveAnims = Checkbox(
                    "Enable Bone Translation Changes (Additive Animations)", Tae.Config.EnableBoneTranslation_AdditiveAnims);

                ImGui.Separator();

                Tae.Config.ResetFloorOnAnimStart = Checkbox(
                    "Reset Floor On Animation Start", Tae.Config.ResetFloorOnAnimStart);

                Tae.Config.ResetHeadingOnAnimStart = Checkbox(
                    "Reset Heading On Animation Start", Tae.Config.ResetHeadingOnAnimStart);

                ImGui.EndMenu();
            }

        }
    }
}
