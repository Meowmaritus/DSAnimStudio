using ImGuiNET;
using Microsoft.Xna.Framework;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class ColorConfig
    {
        [Newtonsoft.Json.JsonIgnore]
        public ColorEditNode.Group RootColorEditNode = new ColorEditNode.Group();

        public ColorConfig()
        {
            InitColorEntries();
            ResetToDefaults();
        }

        private void ResetNodeToDefault(ColorEditNode node)
        {
            if (node is ColorEditNode.ColorEdit asColorEdit)
            {
                var defVal = asColorEdit.DefaultVal;
                asColorEdit.Field.SetValue(this, new Color(defVal.X, defVal.Y, defVal.Z, defVal.W));
            }
            else if (node is ColorEditNode.Group asGroup)
            {
                var children = asGroup.ChildNodes.OrderBy(x => x.Key).ToList();
                foreach (var n in children)
                    ResetNodeToDefault(n.Value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void ResetToDefaults()
        {
            ResetNodeToDefault(RootColorEditNode);
        }

        private ColorEditNode.Group EnsureGroup(string folderPath)
        {
            string[] folderPathParts = folderPath.Split("/");

            ColorEditNode.Group node = RootColorEditNode;
            foreach (var pathPart in folderPathParts)
            {
                node = node.EnsureChildGroupNode(pathPart);
            }

            return node;
        }

        private ColorEditNode.ColorEdit EnsureColorEdit(string fullPath)
        {
            string[] pathParts = fullPath.Split("/");

            ColorEditNode.Group node = RootColorEditNode;
            if (pathParts.Length > 1)
            {
                for (int i = 0; i < pathParts.Length - 1; i++)
                {
                    node = node.EnsureChildGroupNode(pathParts[i]);
                }
            }

            return node.EnsureChildColorEditNode(pathParts[pathParts.Length - 1]);
        }

        public abstract class ColorEditNode
        {
            public string GUID = Guid.NewGuid().ToString();

            public class ColorEdit : ColorEditNode
            {
                public System.Numerics.Vector4 DefaultVal;
                public System.Reflection.FieldInfo Field;
            }

            public class Group : ColorEditNode
            {
                public Dictionary<string, ColorEditNode> ChildNodes = new Dictionary<string, ColorEditNode>();

                public ColorEditNode.Group EnsureChildGroupNode(string nodeName)
                {
                    if (ChildNodes.ContainsKey(nodeName) && ChildNodes[nodeName] is not ColorEditNode.Group)
                        ChildNodes.Remove(nodeName);

                    if (!ChildNodes.ContainsKey(nodeName))
                        ChildNodes.Add(nodeName, new ColorEditNode.Group());

                    return ChildNodes[nodeName] as ColorEditNode.Group;
                }

                public ColorEditNode.ColorEdit EnsureChildColorEditNode(string nodeName)
                {
                    if (ChildNodes.ContainsKey(nodeName) && ChildNodes[nodeName] is not ColorEditNode.ColorEdit)
                        ChildNodes.Remove(nodeName);

                    if (!ChildNodes.ContainsKey(nodeName))
                        ChildNodes.Add(nodeName, new ColorEditNode.ColorEdit());

                    return ChildNodes[nodeName] as ColorEditNode.ColorEdit;
                }
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public object _lock_EditEntries = new object();
        public void InitColorEntries()
        {
            lock (_lock_EditEntries)
            {
                var fields = typeof(ColorConfig).GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray();
                foreach (var f in fields)
                {
                    var editAttribute = f.GetCustomAttribute<ColorEditCfgAttribute>();
                    if (editAttribute != null)
                    {
                        var entry = EnsureColorEdit(editAttribute.DispName);
                        entry.DefaultVal = editAttribute.DefaultValue;
                        entry.Field = f;
                    }
                }
            }
        }

        private void DoImguiOfNode(string nodeKey, ColorEditNode node)
        {
            ImGui.PushID(node.GUID);
            try
            {
                if (node is ColorEditNode.ColorEdit asColorEdit)
                {
                    System.Numerics.Vector4 curVal = ((Color)(asColorEdit.Field.GetValue(this))).ToNVector4();
                    var prevVal = curVal;
                    ImGui.ColorPicker4(nodeKey, ref curVal);
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Middle))
                    {
                        var defVal = asColorEdit.DefaultVal;
                        asColorEdit.Field.SetValue(this, new Color(defVal.X, defVal.Y, defVal.Z, defVal.W));
                    }
                    else if (curVal.X != prevVal.X || curVal.Y != prevVal.Y || curVal.Z != prevVal.Z || curVal.W != prevVal.W)
                    {
                        asColorEdit.Field.SetValue(this, new Color(curVal.X, curVal.Y, curVal.Z, curVal.W));
                    }
                }
                else if (node is ColorEditNode.Group asGroup)
                {
                    if (ImGui.TreeNode($"{nodeKey}##ColorEditNode_{asGroup.GUID}"))
                    {
                        try
                        {
                            var children = asGroup.ChildNodes.OrderBy(x => x.Key).ToList();
                            foreach (var n in children)
                                DoImguiOfNode(n.Key, n.Value);
                        }
                        finally
                        {
                            ImGui.TreePop();
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            finally
            {
                ImGui.PopID();
            }
        }

        public void DoImgui()
        {
            DoImguiOfNode("[Colors]", RootColorEditNode);
        }

        [ColorEditCfg("Viewport/Grid/Base", 32, 112, 39)]
        public Color ColorGrid1u;

        [ColorEditCfg("Viewport/Grid/10-Unit", 0, 255, 0)]
        public Color ColorGrid10u;

        [ColorEditCfg("Viewport/Grid/100-Unit", 255, 255, 0)]
        public Color ColorGrid100u;

        //[ColorEditCfg("Viewport/Grid/Origin", 255, 0, 0)]
        //public Color ColorGridOrigin;

        [ColorEditCfg("Helpers/Melee Hitboxes/Root", 26, 26, 230)]
        public Color ColorHelperHitboxRoot;

        [ColorEditCfg("Helpers/Melee Hitboxes/Middle", 230, 26, 26)]
        public Color ColorHelperHitboxMiddle;

        [ColorEditCfg("Helpers/Melee Hitboxes/Tip", 231, 186, 50)]
        public Color ColorHelperHitboxTip;

        [ColorEditCfg("Helpers/Melee Hitboxes/Unknown", 255, 0, 0)]
        public Color ColorHelperHitboxUnknown;

        [ColorEditCfg("Helpers/FLVER Skeleton Transforms", 0xFFFF00FF)]
        public Color ColorHelperFlverBoneTransforms;

        [ColorEditCfg("Helpers/FLVER Skeleton Lines", 0xFFFF00FF)]
        public Color ColorHelperFlverBoneLines;

        [ColorEditCfg("Helpers/FLVER Skeleton Texts", 0xFFFF00FF)]
        public Color ColorHelperFlverBoneTexts;

        [ColorEditCfg("Helpers/FLVER Skeleton Boxes", 0x00FF00FF)]
        public Color ColorHelperFlverBoneBoundingBox;

        [ColorEditCfg("Helpers/HKX Skeleton", 0xFF0000FF)]
        public Color ColorHelperHkxBone;

        [ColorEditCfg("Helpers/Bone Glue Entries", 255, 165, 0)]
        public Color ColorHelperBoneGlue;

        [ColorEditCfg("Helpers/DummyPoly Texts", 231, 60, 0)]
        public Color ColorHelperDummyPolyTexts;

        [ColorEditCfg("Helpers/DummyPoly Transforms", 231, 60, 0)]
        public Color ColorHelperDummyPolyTransforms;

        [ColorEditCfg("Helpers/DummyPoly Texts (Debug Highlight)", 0xFFFF00FF)]
        public Color ColorHelperDummyPolyDbgTexts;

        [ColorEditCfg("Helpers/DummyPoly Transforms (Debug Highlight)", 0xFFFF00FF)]
        public Color ColorHelperDummyPolyDbgTransforms;

        [ColorEditCfg("Helpers/Sound Event Transforms", 0xFF0000FF)]
        public Color ColorHelperSoundEventTransforms;

        [ColorEditCfg("Helpers/Sound Event Texts", 0xFF0000FF)]
        public Color ColorHelperSoundEventTexts;

        [ColorEditCfg("Helpers/DummyPoly SFX Spawns", 0x00FFFFFF)]
        public Color ColorHelperDummyPolySpawnSFX;

        [ColorEditCfg("Helpers/DummyPoly Bullet/Misc Spawns", 0xFFFF00FF)]
        public Color ColorHelperDummyPolySpawnBulletsMisc;

        [ColorEditCfg("Helpers/DummyPoly SFX+Bullet/Misc Spawns", 0x00FF00FF)]
        public Color ColorHelperDummyPolySpawnSFXBulletsMisc;

        [ColorEditCfg("Helpers/Camera Pivot Cube", 0x00FFFFFF)]
        public Color ColorHelperCameraPivot;

        [ColorEditCfg("Helpers/Character Hit Capsule", 152, 99, 45)]
        public Color ColorHelperChrHitCapsule;

        [ColorEditCfg("Helpers/Attack Distance Line", 255, 0, 0)]
        public Color ColorHelperAttackDistanceLine;

        [ColorEditCfg("Helpers/Attack Distance Text", 255, 0, 0)]
        public Color ColorHelperAttackDistanceText;

        [ColorEditCfg("Main/UI Background Color", 35, 35, 35, 255)]
        public Color MainColorBackground;

        [ColorEditCfg("Main/Viewport Background Color", 0.1f, 0.1f, 0.1f)]
        public Color MainColorViewportBackground;


        [ColorEditCfg("Viewport/Memory Usage/Good", 255, 255, 0)]
        public Color GuiColorMemoryUseTextGood;
        [ColorEditCfg("Viewport/Memory Usage/Okay", 255, 165, 0)]
        public Color GuiColorMemoryUseTextOkay;
        [ColorEditCfg("Viewport/Memory Usage/Bad", 255, 0, 0)]
        public Color GuiColorMemoryUseTextBad;

        [ColorEditCfg("Viewport/Status/Status Text", 255, 255, 0)]
        public Color GuiColorViewportStatus;
        [ColorEditCfg("Viewport/Status/Bone Count Exceeded Text", 255, 165, 0)]
        public Color GuiColorViewportStatusMaxBoneCountExceeded;
        [ColorEditCfg("Viewport/Status/Animation Doesn't Exist Text", 255, 0, 0)]
        public Color GuiColorViewportStatusAnimDoesntExist;
        [ColorEditCfg("Viewport/Status/Combo Viewer Status Text", 0, 255, 255)]
        public Color GuiColorViewportStatusCombo;



        [ColorEditCfg("Graph/Action Box/Default Fill", 30, 144, 255, 255)]
        public Color GuiColorActionBox_Normal_Fill;

        [ColorEditCfg("Graph/Action Box/Outline", 7, 36, 63, 255)]
        public Color GuiColorActionBox_Normal_Outline;

        

        [ColorEditCfg("Graph/Action Box/Text", 0xFFFFFFFF)]
        public Color GuiColorActionBox_Normal_Text;

        [ColorEditCfg("Graph/Action Box/Text Shadow", 0x000000FF)]
        public Color GuiColorActionBox_Normal_TextShadow;

        [ColorEditCfg("Graph/Action Box/Selected Pulse Start", 180, 180, 180, 255)]
        public Color GuiColorActionBox_Selected_Outline_PulseStart;

        [ColorEditCfg("Graph/Action Box/Selected Pulse End", 255, 255, 255, 255)]
        public Color GuiColorActionBox_Selected_Outline_PulseEnd;

        [ColorEditCfg("Graph/Action Box/Selection Dimming Overlay", 0x00000080)]
        public Color GuiColorActionBox_SelectionDimmingOverlay;

        [ColorEditCfg("Graph/Background", 120, 120, 120, 255)]
        public Color GuiColorActionGraphBackground;

        [ColorEditCfg("Graph/Ghost Overlay", 116, 116, 116, 120)]
        public Color GuiColorActionGraphGhostOverlay;

        [ColorEditCfg("Graph/Anim End Vertical Line", 0xFFFFFFFF)]
        public Color GuiColorActionGraphAnimEndVerticalLine;

        [ColorEditCfg("Graph/Anim End Darken Rect", 0x00000040)]
        public Color GuiColorActionGraphAnimEndDarkenRect;

        [ColorEditCfg("Graph/Row Horizontal Lines", 0x00000040)]
        public Color GuiColorActionGraphRowHorizontalLines;

        [ColorEditCfg("Graph/Timeline Fill", 75, 75, 75, 255)]
        public Color GuiColorActionGraphTimelineFill;

        [ColorEditCfg("Graph/Frame Vertical Lines (Timeline)", 0, 0, 0, 0.125f)]
        public Color GuiColorActionGraphTimelineFrameVerticalLines;

        [ColorEditCfg("Graph/Frame Number Text", 0xFFFFFFFF)]
        public Color GuiColorActionGraphTimelineFrameNumberText;

        [ColorEditCfg("Graph/Frame Vertical Lines", (211f / 255f), (211f / 255f), (211f / 255f), 0.5f)]
        public Color GuiColorActionGraphVerticalFrameLines;

        [ColorEditCfg("Graph/Seconds Vertical Lines", (211f / 255f), (211f / 255f), (211f / 255f), 0.75f)]
        public Color GuiColorActionGraphVerticalSecondLines;

        [ColorEditCfg("Graph/Selection Rectangle Fill", (32f / 255f), (144f / 255f), (255f / 255f), 0.5f)]
        public Color GuiColorActionGraphSelectionRectangleFill;

        [ColorEditCfg("Graph/Selection Rectangle Outline", 0xFFFFFFFF)]
        public Color GuiColorActionGraphSelectionRectangleOutline;

        [ColorEditCfg("Graph/Action Slice Tool", 0x00FFFFFF)]
        public Color GuiColorActionGraphSliceToolLine;

        [ColorEditCfg("Graph/Playback Current Time Vertical Line", 0xFF0000FF)]
        public Color GuiColorActionGraphPlaybackCursor_V3;

        [ColorEditCfg("Graph/Playback Start Time Vertical Line", 0x808080FF)]
        public Color GuiColorActionGraphPlaybackStartTime_V2;

        //[ColorEditCfg("Graph/Action Tooltip Fill", 64, 64, 64)]
        //public Color GuiColorActionGraphHoverInfoBoxFill;

        //[ColorEditCfg("Graph/Action Tooltip Text", 0xFFFFFFFF)]
        //public Color GuiColorActionGraphHoverInfoBoxText;

        //[ColorEditCfg("Graph/Action Tooltip Outline", 32, 32, 32)]
        //public Color GuiColorActionGraphHoverInfoBoxOutline;

        [ColorEditCfg("Graph/Scroll Bar/Background Fill", 0.25f, 0.25f, 0.25f)]
        public Color GuiColorActionGraphScrollbarBackground;

        [ColorEditCfg("Graph/Scroll Bar/Foreground Inactive Fill", 0.45f, 0.45f, 0.45f)]
        public Color GuiColorActionGraphScrollbarForegroundInactive;

        [ColorEditCfg("Graph/Scroll Bar/Foreground Active Fill", 0.55f, 0.55f, 0.55f)]
        public Color GuiColorActionGraphScrollbarForegroundActive;

        [ColorEditCfg("Graph/Scroll Bar/Arrow Button Inactive", 0.35f, 0.35f, 0.35f)]
        public Color GuiColorActionGraphScrollbarArrowButtonForegroundInactive;

        [ColorEditCfg("Graph/Scroll Bar/Arrow Button Active", 0.45f, 0.45f, 0.45f)]
        public Color GuiColorActionGraphScrollbarArrowButtonForegroundActive;

        //[ColorEditCfg("", 0x000000FF)]
        //public Color GuiColorAnimListCollapsePlusMinusForeground;

        //[ColorEditCfg("", 0xFFFFFFFF)]
        //public Color GuiColorAnimListCollapsePlusMinusBackground;

        [ColorEditCfg("Animation List/Anim Category Header Outline", 0xFFFFFFFF)]
        public Color GuiColorAnimListAnimSectionHeaderRectOutline;

        [ColorEditCfg("Animation List/Anim Category Header Fill", 0x808080FF)]
        public Color GuiColorAnimListAnimSectionHeaderRectFill;

        [ColorEditCfg("Animation List/Anim Category Header Text", 0xFFFFFFFF)]
        public Color GuiColorAnimListTextAnimSectionName;

        [ColorEditCfg("Animation List/Anim Name", 0xFFFFFFFF)]
        public Color GuiColorAnimListTextAnimName;

        //[ColorEditCfg("", 0x808080FF)]
        //public Color GuiColorAnimListTextAnimNameMinBlend;

        [ColorEditCfg("Animation List/Anim Name (Selected)", 0xFFFF00FF)]
        public Color GuiColorAnimListTextAnimNameSelected;

        [ColorEditCfg("Animation List/Anim File Name", 238, 232, 170)]
        public Color GuiColorAnimListTextAnimFileName;

        [ColorEditCfg("Animation List/Text Shadow", 0x000000FF)]
        public Color GuiColorAnimListTextShadow;

        [ColorEditCfg("Animation List/Highlight Rect Fill", 30, 144, 255)]
        public Color GuiColorAnimListHighlightRectFill;

        [ColorEditCfg("Animation List/Highlight Rect Outline", 200, 200, 200)]
        public Color GuiColorAnimListHighlightRectOutline;

        [ColorEditCfg("Helpers/Root Motion Start Location", 0xFF0000FF)]
        public Color ColorHelperRootMotionStartLocation;

        [ColorEditCfg("Helpers/Root Motion Trail", 0xFFFF00FF)]
        public Color ColorHelperRootMotionTrail;

        [ColorEditCfg("Helpers/Root Motion Current Location", 0x00FF00FF)]
        public Color ColorHelperRootMotionCurrentLocation;

        [ColorEditCfg("Helpers/Root Motion Start Location (Previous Loop)", 0xFF000080)]
        public Color ColorHelperRootMotionStartLocation_PrevLoop;

        [ColorEditCfg("Helpers/Root Motion Trail (Previous Loop)", 0xFFFF0080)]
        public Color ColorHelperRootMotionTrail_PrevLoop;

        //[ColorEditCfg("", 0x00FF0080)]
        //public Color ColorHelperRootMotionCurrentLocation_PrevLoop;

        [ColorEditCfg("Project/Default Tag Color", 0x7F7F7FFF)]
        public Color ColorProjectTagDefault;

        public void ReadColorsFromConfig()
        {
            
        }

        public void WriteColorsToConfig()
        {
            
        }
    }
}
