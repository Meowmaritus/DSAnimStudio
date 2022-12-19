using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class SceneManager : Window
        {
            public override string Title => "Scene Explorer";
            public override string ImguiTag => $"{nameof(Window)}.{nameof(SceneManager)}";
            protected override void BuildContents()
            {
                GFX.HighlightedThing = null;

                GFX.NewDebug_ShowTex_Material = null;
                GFX.NewDebug_ShowTex_Sampler = null;
                GFX.NewDebug_ShowTex_SamplerConfig = null;
                int TREE_INDEX = 0;

                lock (Scene._lock_ModelLoad_Draw)
                {
                    int index_Mesh = 0;

                    void DoMesh(Model mdl, NewMesh mesh, string meshName)
                    {
                        if (mesh == null)
                            return;

                        void inner_Submeshes()
                        {
                            var nodeOpen = ImGui.TreeNode($"Meshes###SceneManager_Mesh{index_Mesh}");

                            //if (ImGui.IsItemHovered())
                            //{
                            //    GFX.HighlightedThing = mesh;
                            //}

                            if (nodeOpen)
                            {
                                foreach (var sm in mesh.Submeshes)
                                {
                                    bool isMeshGrayedOut = sm.NewMaterial.ModelMaskIndex >= 0 && sm.NewMaterial.ModelMaskIndex < mesh.DrawMask.Length && !mesh.DrawMask[sm.NewMaterial.ModelMaskIndex];
                                    ImGui.PushStyleColor(ImGuiCol.Text, !isMeshGrayedOut ? new System.Numerics.Vector4(1, 1, 1, 1)
                                        : new System.Numerics.Vector4(0.75f, 0.25f, 0.25f, 1));
                                    ImGui.PushStyleColor(ImGuiCol.TextDisabled, isMeshGrayedOut ? new System.Numerics.Vector4(1, 0, 0, 1)
                                        : new System.Numerics.Vector4(0, 1, 0, 1));
                                    sm.IsVisible = MenuBar.Checkbox(sm.FullMaterialName, sm.IsVisible, enabled: true,
                                        shortcut: (sm.NewMaterial.ModelMaskIndex >= 0 ? $"[Mask {(sm.NewMaterial.ModelMaskIndex)}]" : null));

                                    if (ImGui.IsItemHovered())
                                    {
                                        GFX.HighlightedThing = sm;
                                    }

                                    ImGui.PopStyleColor(2);
                                }

                                ImGui.TreePop();
                            }
                        }

                        void inner_Materials()
                        {
                            if (ImGui.TreeNode($"Materials"))
                            {
                                int index_Material = 0;
                                foreach (var m in mesh.Materials)
                                {
                                    int index_Sampler = 0;
                                    var nodeOpen = ImGui.TreeNode($"[{index_Material}] {(m.Name ?? "")} ---> {(m.ShaderConfigName ?? "Unk Shader")}###SceneManager_Mesh{index_Mesh}_Material{index_Material}");

                                    if (ImGui.IsItemHovered())
                                    {
                                        GFX.HighlightedThing = m;
                                    }

                                    if (nodeOpen)
                                    {
                                        ImGui.Button($"Save global shader config defaults for '{m.ShaderConfigName}'###Name###SceneManager_Mesh{index_Mesh}_Material{index_Material}_SaveGlobalShaderConfig");
                                        if (ImGui.IsItemClicked())
                                        {
                                            ImguiOSD.DialogManager.AskYesNo("Save Global Defaults?", "Save global defaults for this shader type? This will affect every material that uses this shader in any file you open.", choice =>
                                            {
                                                if (choice)
                                                {
                                                    FlverShaderConfig.SaveGlobalDefault(m.ShaderConfig, m.ShaderConfigName);
                                                    FlverShaderConfig.ClearCache();
                                                    mdl.TryLoadGlobalShaderConfigs();
                                                }
                                            }, allowCancel: true, enterKeyForYes: false);
                                        }


                                        ImGui.LabelText($"Name###SceneManager_Mesh{index_Mesh}_Material{index_Material}_Name", m.Name ?? "<NULL>");
                                        ImGui.LabelText($"MTD###SceneManager_Mesh{index_Mesh}_Material{index_Material}_MTD", m.MtdName ?? "<NULL>");

                                        if (ImGui.TreeNode($"MTD Parameters###SceneManager_Mesh{index_Mesh}_Material{index_Material}_MTDParameters"))
                                        {


                                            var p = m.MtdInfo.ShaderParameters;
                                            foreach (var k in m.MtdInfo.ShaderParameters.Keys)
                                            {
                                                bool isUsed = m.MtdInfo.IsParameterUsedByDSAS(k);
                                                var imguiKey = $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_MTDParameters_{k}";
                                                if (!isUsed)
                                                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1));

                                                if (p[k] is bool asBool)
                                                {
                                                    var v = asBool;
                                                    ImGui.Checkbox($"{k}###{imguiKey}", ref v);
                                                    p[k] = v;
                                                }
                                                else if (p[k] is int asInt)
                                                {
                                                    var v = asInt;
                                                    ImGui.InputInt($"{k}###{imguiKey}", ref v);
                                                    p[k] = v;
                                                }
                                                else if (p[k] is int[] asIntArr)
                                                {
                                                    for (int i = 0; i < asIntArr.Length; i++)
                                                    {
                                                        var v = asIntArr[i];
                                                        ImGui.InputInt($"{(i == 0 ? k : " ")}###{imguiKey}[{i}]", ref v);
                                                        asIntArr[i] = v;
                                                    }
                                                }
                                                else if (p[k] is float asFloat)
                                                {
                                                    var v = asFloat;
                                                    ImGui.InputFloat($"{k}###{imguiKey}", ref v);
                                                    p[k] = v;
                                                }
                                                else if (p[k] is float[] asFloatArr)
                                                {
                                                    if (asFloatArr.Length == 1)
                                                    {
                                                        var v = asFloatArr[0];
                                                        ImGui.InputFloat($"{k}###{imguiKey}", ref v);
                                                        asFloatArr[0] = v;
                                                    }
                                                    else if (asFloatArr.Length == 2)
                                                    {
                                                        var v = new System.Numerics.Vector2(asFloatArr[0], asFloatArr[1]);
                                                        ImGui.InputFloat2($"{k}###{imguiKey}", ref v);
                                                        asFloatArr[0] = v.X;
                                                        asFloatArr[1] = v.Y;
                                                    }
                                                    else if (asFloatArr.Length == 3)
                                                    {
                                                        var v = new System.Numerics.Vector3(asFloatArr[0], asFloatArr[1], asFloatArr[2]);
                                                        ImGui.InputFloat3($"{k}###{imguiKey}", ref v);
                                                        asFloatArr[0] = v.X;
                                                        asFloatArr[1] = v.Y;
                                                        asFloatArr[2] = v.Z;
                                                    }
                                                    else if (asFloatArr.Length == 4)
                                                    {
                                                        var v = new System.Numerics.Vector4(asFloatArr[0], asFloatArr[1], asFloatArr[2], asFloatArr[3]);
                                                        ImGui.InputFloat4($"{k}###{imguiKey}", ref v);
                                                        asFloatArr[0] = v.X;
                                                        asFloatArr[1] = v.Y;
                                                        asFloatArr[2] = v.Z;
                                                        asFloatArr[3] = v.W;
                                                    }
                                                    else if (asFloatArr.Length == 5)
                                                    {
                                                        for (int i = 0; i < asFloatArr.Length; i++)
                                                        {
                                                            var v = asFloatArr[i];
                                                            ImGui.InputFloat($"{(i == 0 ? k : " ")}###{imguiKey}[{i}]", ref v);
                                                            asFloatArr[i] = v;
                                                        }
                                                    }

                                                    
                                                }

                                                if (!isUsed)
                                                    ImGui.PopStyleColor();
                                            }

                                            ImGui.TreePop();
                                        }

                                        ImGui.LabelText($"Shader###SceneManager_Mesh{index_Mesh}_Material{index_Material}_Shader", m.ShaderConfigName ?? "<NULL>");

                                        ImGui.Separator();

                                        System.Numerics.Vector2 uvTile1 = m.UVTileMult1.ToNumerics();
                                        ImGui.InputFloat2($"UV Group 1 Tile Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_UVTileMult1", ref uvTile1);
                                        m.UVTileMult1 = uvTile1.ToXna();

                                        System.Numerics.Vector2 uvTile2 = m.UVTileMult2.ToNumerics();
                                        ImGui.InputFloat2($"UV Group 2 Tile Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_UVTileMult2", ref uvTile2);
                                        m.UVTileMult2 = uvTile2.ToXna();

                                        System.Numerics.Vector2 uvTile3 = m.UVTileMult3.ToNumerics();
                                        ImGui.InputFloat2($"UV Group 3 Tile Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_UVTileMult3", ref uvTile3);
                                        m.UVTileMult3 = uvTile3.ToXna();

                                        System.Numerics.Vector2 uvTile4 = m.UVTileMult4.ToNumerics();
                                        ImGui.InputFloat2($"UV Group 4 Tile Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_UVTileMult4", ref uvTile4);
                                        m.UVTileMult4 = uvTile4.ToXna();

                                        System.Numerics.Vector2 uvTile5 = m.UVTileMult5.ToNumerics();
                                        ImGui.InputFloat2($"UV Group 5 Tile Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_UVTileMult5", ref uvTile5);
                                        m.UVTileMult5 = uvTile5.ToXna();

                                        System.Numerics.Vector2 uvTile6 = m.UVTileMult6.ToNumerics();
                                        ImGui.InputFloat2($"UV Group 6 Tile Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_UVTileMult6", ref uvTile6);
                                        m.UVTileMult6 = uvTile6.ToXna();

                                        System.Numerics.Vector2 uvTile7 = m.UVTileMult7.ToNumerics();
                                        ImGui.InputFloat2($"UV Group 7 Tile Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_UVTileMult7", ref uvTile7);
                                        m.UVTileMult7 = uvTile7.ToXna();

                                        System.Numerics.Vector2 uvTile8 = m.UVTileMult8.ToNumerics();
                                        ImGui.InputFloat2($"UV Group 8 Tile Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_UVTileMult8", ref uvTile8);
                                        m.UVTileMult8 = uvTile8.ToXna();

                                        ImGui.Separator();



                                        if (ImGui.TreeNode("Blend Settings - Diffuse", $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_BlendSettings_Diffuse"))
                                        {
                                            FlverShaderEnums.NewBlendOperations_Picker.ShowPickerCombo("Blend Operation", $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendOperation", ref m.ShaderConfig.NewBlendOperation_Diffuse, NewBlendOperations.Multiply);
                                            ImGui.Checkbox($"Diffuse - Reverse Blend Dir###SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendReverseDir_Diffuse", ref m.ShaderConfig.NewBlendReverseDir_Diffuse);
                                            ImGui.Checkbox($"Diffuse - Invert Blend Mask Value###SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendInverseVal_Diffuse", ref m.ShaderConfig.NewBlendInverseVal_Diffuse);
                                            ImGui.TreePop();
                                        }

                                        ImGui.Separator();

                                        if (ImGui.TreeNode("Blend Settings - Specular", $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_BlendSettings_Specular"))
                                        {
                                            FlverShaderEnums.NewBlendOperations_Picker.ShowPickerCombo("Specular - Blend Operation", $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendOperation_Specular", ref m.ShaderConfig.NewBlendOperation_Specular, NewBlendOperations.Multiply);
                                            ImGui.Checkbox($"Specular - Reverse Blend Dir###SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendReverseDir_Specular", ref m.ShaderConfig.NewBlendReverseDir_Specular);
                                            ImGui.Checkbox($"Specular - Invert Blend Mask Value###SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendInverseVal_Specular", ref m.ShaderConfig.NewBlendInverseVal_Specular);
                                            ImGui.TreePop();
                                        }
                                        ImGui.Separator();

                                        if (ImGui.TreeNode("Blend Settings - Normal", $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_BlendSettings_Normal"))
                                        {
                                            FlverShaderEnums.NewBlendOperations_Picker.ShowPickerCombo("Normal - Blend Operation", $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendOperation_Normal", ref m.ShaderConfig.NewBlendOperation_Normal, NewBlendOperations.Multiply);
                                            ImGui.Checkbox($"Normal - Reverse Blend Dir###SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendReverseDir_Normal", ref m.ShaderConfig.NewBlendReverseDir_Normal);
                                            ImGui.Checkbox($"Normal - Invert Blend Mask Value###SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendInverseVal_Normal", ref m.ShaderConfig.NewBlendInverseVal_Normal);
                                            ImGui.TreePop();
                                        }
                                        ImGui.Separator();

                                        if (ImGui.TreeNode("Blend Settings - Shininess", $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_BlendSettings_Shininess"))
                                        {
                                            FlverShaderEnums.NewBlendOperations_Picker.ShowPickerCombo("Shininess - Blend Operation", $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendOperation_Shininess", ref m.ShaderConfig.NewBlendOperation_Shininess, NewBlendOperations.Multiply);
                                            ImGui.Checkbox($"Shininess - Reverse Blend Dir###SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendReverseDir_Shininess", ref m.ShaderConfig.NewBlendReverseDir_Shininess);
                                            ImGui.Checkbox($"Shininess - Invert Blend Mask Value###SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendInverseVal_Shininess", ref m.ShaderConfig.NewBlendInverseVal_Shininess);
                                            ImGui.TreePop();
                                        }
                                        ImGui.Separator();

                                        if (ImGui.TreeNode("Blend Settings - Emissive", $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_BlendSettings_Emissive"))
                                        {
                                            FlverShaderEnums.NewBlendOperations_Picker.ShowPickerCombo("Emissive - Blend Operation", $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendOperation_Emissive", ref m.ShaderConfig.NewBlendOperation_Emissive, NewBlendOperations.Multiply);
                                            ImGui.Checkbox($"Emissive - Reverse Blend Dir###SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendReverseDir_Emissive", ref m.ShaderConfig.NewBlendReverseDir_Emissive);
                                            ImGui.Checkbox($"Emissive - Invert Blend Mask Value###SceneManager_Mesh{index_Mesh}_Material{index_Material}_NewBlendInverseVal_Emissive", ref m.ShaderConfig.NewBlendInverseVal_Emissive);
                                            ImGui.TreePop();
                                        }




                                        ImGui.Separator();

                                        ImGui.Checkbox($"Visible###SceneManager_Mesh{index_Mesh}_Material{index_Material}_IsVisible", ref m.IsVisible);

                                        bool isSoloVisible = m.IsSoloVisible;
                                        ImGui.Checkbox($"Is The ONLY One Visible###SceneManager_Mesh{index_Mesh}_Material{index_Material}_IsSoloVisible", ref isSoloVisible);
                                        if (isSoloVisible != m.IsSoloVisible)
                                        {
                                            for (int i = 0; i < mesh.Materials.Count; i++)
                                            {
                                                if (i == index_Material)
                                                    continue;
                                                mesh.Materials[i].IsSoloVisible = false;
                                            }

                                            m.IsSoloVisible = isSoloVisible;
                                        }

                                        string[] ptdeMtdTypes = new string[]
                                        {
                                            "Default",
                                            "Metal",
                                            "Wet",
                                            "Dull",
                                        };
                                        int ptdeMtdTypeIndex = (int)m.PtdeMtdType;
                                        ImGui.Combo($"DS1 PTDE MTD Type###SceneManager_Mesh{index_Mesh}_Material{index_Material}_PtdeMtdType",
                                            ref ptdeMtdTypeIndex, ptdeMtdTypes,
                                            ptdeMtdTypes.Length);
                                        m.PtdeMtdType = (PtdeMtdTypes)ptdeMtdTypeIndex;

                                        ImGui.Checkbox($"IsDS2EmissiveFlow###SceneManager_Mesh{index_Mesh}_Material{index_Material}_IsDS2EmissiveFlow", ref m.ShaderConfig.IsDS2EmissiveFlow);
                                        ImGui.Checkbox($"IsDS3Veil###SceneManager_Mesh{index_Mesh}_Material{index_Material}_IsDS3Veil", ref m.ShaderConfig.IsDS3Veil);
                                        ImGui.Checkbox($"IsShaderDoubleFaceCloth###SceneManager_Mesh{index_Mesh}_Material{index_Material}_IsShaderDoubleFaceCloth", ref m.ShaderConfig.IsShaderDoubleFaceCloth);
                                        ImGui.Checkbox($"Invert Blend Mask###SceneManager_Mesh{index_Mesh}_Material{index_Material}_InvertBlendMaskMap", ref m.ShaderConfig.InvertBlendMaskMap);

                                        ImGui.Separator();
                                        ImGui.Checkbox($"Is Metallic###SceneManager_Mesh{index_Mesh}_Material{index_Material}_IsMetallic", ref m.ShaderConfig.IsMetallic);
                                        ImGui.Checkbox($"Is Undefined Metallic###SceneManager_Mesh{index_Mesh}_Material{index_Material}_IsUndefinedMetallic", ref m.IsUndefinedMetallic);
                                        ImGui.SliderFloat($"Undefined Metallic Value###SceneManager_Mesh{index_Mesh}_Material{index_Material}_UndefinedMetallicValue", ref m.ShaderConfig.UndefinedMetallicValue, 0, 1);
                                        var nonmetallicColor = m.ShaderConfig.NonMetallicSpecColor.ToNumerics();
                                        ImGui.ColorEdit3($"NonMetallicSpecColor###SceneManager_Mesh{index_Mesh}_Material{index_Material}_NonMetallicSpecColor", ref nonmetallicColor, ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.InputRGB);
                                        m.ShaderConfig.NonMetallicSpecColor = nonmetallicColor.ToXna();

                                        ImGui.Separator();
                                        ImGui.Checkbox($"Enable Blend Mask###SceneManager_Mesh{index_Mesh}_Material{index_Material}_EnableBlendMask", ref m.ShaderConfig.EnableBlendMask);
                                        ImGui.Checkbox($"Is Undefined Blend Mask###SceneManager_Mesh{index_Mesh}_Material{index_Material}_IsUndefinedBlendMask", ref m.IsUndefinedBlendMask);
                                        ImGui.SliderFloat($"Undefined Blend Mask Value###SceneManager_Mesh{index_Mesh}_Material{index_Material}_UndefinedBlendMaskValue", ref m.ShaderConfig.UndefinedBlendMaskValue, 0, 1);
                                        ImGui.Checkbox($"Get Blend Mask From Normal Map 1 Alpha###SceneManager_Mesh{index_Mesh}_Material{index_Material}_BlendMaskFromNormalMap1Alpha", ref m.ShaderConfig.BlendMaskFromNormalMap1Alpha);
                                        ImGui.Checkbox($"Get Blend Mask From Normal Map 1 Alpha - Is Inverted###SceneManager_Mesh{index_Mesh}_Material{index_Material}_BlendMaskFromNormalMap1Alpha_IsReverse", ref m.ShaderConfig.BlendMaskFromNormalMap1Alpha_IsReverse);
                                        ImGui.Checkbox($"Mult Blend Mask By Albedo Map 2 Alpha###SceneManager_Mesh{index_Mesh}_Material{index_Material}_BlendMaskMultByAlbedoMap2Alpha", ref m.ShaderConfig.BlendMaskMultByAlbedoMap2Alpha);
                                        ImGui.Checkbox($"Mult Blend Mask By Albedo Map 2 Alpha - Is Inverted###SceneManager_Mesh{index_Mesh}_Material{index_Material}_BlendMaskMultByAlbedoMap2Alpha_IsReverse", ref m.ShaderConfig.BlendMaskMultByAlbedoMap2Alpha_IsReverse);
                                        ImGui.Separator();

                                        ImGui.Checkbox($"Enable Texture Alphas###SceneManager_Mesh{index_Mesh}_Material{index_Material}_EnableAlphas", ref m.ShaderConfig.EnableAlphas);
                                        ImGui.Checkbox($"Use Fancy Texture Alphas###SceneManager_Mesh{index_Mesh}_Material{index_Material}_UseFancyAlphas", ref m.ShaderConfig.UseFancyAlphas);
                                        ImGui.SliderFloat($"Fancy Texture Alpha Cutoff###SceneManager_Mesh{index_Mesh}_Material{index_Material}_FancyAlphaCutoff", ref m.ShaderConfig.FancyAlphaCutoff, 0, 1, (m.ShaderConfig.FancyAlphaCutoff < 1) ? "%.2f" : "(Use Default)");
                                        //ImGui.Checkbox("Enable Texture Blending", ref GFX.FlverEnableTextureBlending);
                                        
                                        ImGui.SliderFloat($"Direct Light Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_DirectLightMult", ref m.ShaderConfig.DirectLightingMult, 0, 3);

                                        TooltipManager.DoTooltip("Direct Light Multiplier", "Multiplies the brightness of light reflected directly off" +
                                            "\nthe surface of the model.");


                                        ImGui.SliderFloat($"Indirect Light Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_IndirectLightMult", ref m.ShaderConfig.IndirectLightingMult, 0, 3);
                                        TooltipManager.DoTooltip("Indirect Light Multiplier", "Multiplies the brightness of environment map lighting reflected.");

                                        ImGui.SliderFloat("Direct Diffuse Mult", ref m.ShaderConfig.DirectDiffuseMult, 0, 3);
                                        ImGui.SliderFloat("Direct Specular Mult", ref m.ShaderConfig.DirectSpecularMult, 0, 3);
                                        ImGui.SliderFloat("Indirect Diffuse Mult", ref m.ShaderConfig.IndirectDiffuseMult, 0, 3);
                                        ImGui.SliderFloat("Indirect Specular Mult", ref m.ShaderConfig.IndirectSpecularMult, 0, 3);

                                        //ImGui.SliderFloat("Ambient Light Mult", ref Environment.AmbientLightMult, 0, 3);
                                        ImGui.SliderFloat($"Specular Power Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_SpecularPowerMult", ref m.ShaderConfig.SpecularPowerMult, 1, 8);
                                        TooltipManager.DoTooltip("Specular Power Multiplier", "Multiplies the specular power of the lighting. " +
                                            "\nHigher makes thing's very glossy. " +
                                            "\nMight make some Bloodborne kin of the cosmos look more accurate.");
                                        ImGui.SliderFloat($"LdotN Power Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_LdotNPowerMult", ref m.ShaderConfig.LdotNPowerMult, 1, 8);
                                        TooltipManager.DoTooltip("L dot N Vector Power Multiplier", "Advanced setting. If you know you know.");

                                        ImGui.SliderFloat($"Emissive Light Mult###SceneManager_Mesh{index_Mesh}_Material{index_Material}_EmissiveLightMult", ref m.ShaderConfig.EmissiveMult, 0, 3);


                                        TooltipManager.DoTooltip("Emissive Light Mult", "Multiplies the brightness of light emitted by the model's " +
                                            "\nemissive texture map, if applicable.");


                                        ImGui.Checkbox($"Get Emissive Color From Albedo###SceneManager_Mesh{index_Mesh}_Material{index_Material}_EmissiveColorFromAlbedo", ref m.ShaderConfig.EmissiveColorFromAlbedo);


                                        ImGui.Separator();

                                        ImGui.Checkbox($"Reflectance Mult In Normal Map Alpha###SceneManager_Mesh{index_Mesh}_Material{index_Material}_IsReflectMultInNormalAlpha", ref m.ShaderConfig.IsReflectMultInNormalAlpha);
                                        ImGui.Checkbox($"Swap X & Y Channels In Normal Map###SceneManager_Mesh{index_Mesh}_Material{index_Material}_SwapNormalXY", ref m.ShaderConfig.SwapNormalXY);

                                        ImGui.Separator();

                                        int curChrCustomizeType = m.ShaderConfig.chrCustomizeType_Values.IndexOf(m.ShaderConfig.ChrCustomizeType);
                                        ImGui.ListBox($"ChrCustomize Type###SceneManager_Mesh{index_Mesh}_Material{index_Material}_ChrCustomizeType", ref curChrCustomizeType, m.ShaderConfig.chrCustomizeType_Names, m.ShaderConfig.chrCustomizeType_Names.Length);
                                        if (curChrCustomizeType == -1)
                                            curChrCustomizeType = 0;
                                        m.ShaderConfig.ChrCustomizeType = m.ShaderConfig.chrCustomizeType_Values[curChrCustomizeType];

                                        ImGui.Checkbox($"Enable SSS###SceneManager_Mesh{index_Mesh}_Material{index_Material}_UseSSS", ref m.ShaderConfig.EnableSSS);
                                        var sssColor = m.ShaderConfig.SSSColor.ToCS();
                                        ImGui.DragFloat3($"SSS Color###SceneManager_Mesh{index_Mesh}_Material{index_Material}_SSSColor", ref sssColor);
                                        m.ShaderConfig.SSSColor = sssColor.ToXna();
                                        ImGui.DragFloat($"SSS Intensity###SceneManager_Mesh{index_Mesh}_Material{index_Material}_SSSIntensity", ref m.ShaderConfig.SSSIntensity);

                                        ImGui.Separator();

                                        FlverShaderEnums.NewDebugTypes_Picker.ShowPickerCombo($"Shader Debug Type", $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_ShaderDebugType", ref m.NewDebugType, NewDebugTypes.None);

                                        //int drawStepIndex = GFX.DRAW_STEP_LIST.ToList().IndexOf(m.DrawStep);
                                        //var drawStepNames = GFX.DRAW_STEP_LIST.Select(x => x.ToString()).ToArray();
                                        //ImGui.ListBox($"DRAW_STEP###SceneManager_Mesh{index_Mesh}_Material{index_Material}_DRAW_STEP",
                                        //    ref drawStepIndex, drawStepNames,
                                        //    GFX.DRAW_STEP_LIST.Length);
                                        //m.DrawStep = GFX.DRAW_STEP_LIST[drawStepIndex];

                                        //int shadingModeIndex = GFX.FlverShadingModeList_NoDefault.IndexOf(m.ShadingMode);
                                        //ImGui.ListBox($"Shading Mode###SceneManager_Mesh{index_Mesh}_Material{index_Material}_ShadingMode",
                                        //    ref shadingModeIndex, GFX.FlverShadingModeNamesList_NoDefault,
                                        //    GFX.FlverShadingModeNamesList_NoDefault.Length);
                                        //m.ShadingMode = GFX.FlverShadingModeList_NoDefault[shadingModeIndex];

                                        if (ImGui.TreeNode($"Samplers###SceneManager_Mesh{index_Mesh}_Material{index_Material}_Samplers"))
                                        {
                                            bool anySamplerDebugView = m.Samplers.Any(smp => smp.IsDebugView);

                                            foreach (var s in m.Samplers)
                                            {
                                                bool samplerNodeOpen = ImGui.TreeNode($"{s.Name} [{s.TexPath}]###SceneManager_Mesh{index_Mesh}_Material{index_Material}_Sampler{index_Sampler}");
                                                var matchingSamplerConfig = m.GetSamplerConfig(s.Name);
                                                if (ImGui.IsItemHovered() && !anySamplerDebugView)
                                                {
                                                    GFX.NewDebug_ShowTex_Material = m;
                                                    GFX.NewDebug_ShowTex_Sampler = s;
                                                    GFX.NewDebug_ShowTex_SamplerConfig = matchingSamplerConfig;
                                                }

                                                if (samplerNodeOpen)
                                                {
                                                    
                                                    if (matchingSamplerConfig != null)
                                                    {
                                                        int texTypeIndex = FlverMaterial.AllTextureTypes.IndexOf(matchingSamplerConfig.TexType);
                                                        ImGui.Combo($"Texture Type###SceneManager_Mesh{index_Mesh}_Material{index_Material}_Sampler{index_Sampler}_TextureType", ref texTypeIndex, FlverMaterial.AllTextureTypeNames, FlverMaterial.AllTextureTypeNames.Length);
                                                        if (texTypeIndex < 0)
                                                            texTypeIndex = 0;
                                                        if (texTypeIndex >= FlverMaterial.AllTextureTypes.Count)
                                                            texTypeIndex = FlverMaterial.AllTextureTypes.Count - 1;
                                                        matchingSamplerConfig.TexType = FlverMaterial.AllTextureTypes[texTypeIndex];

                                                        int uvIndexPlusOne = matchingSamplerConfig.UVIndex + 1;
                                                        ImGui.InputInt($"UV Map###SceneManager_Mesh{index_Mesh}_Material{index_Material}_Sampler{index_Sampler}_UVMapIndex", ref uvIndexPlusOne);
                                                        if (uvIndexPlusOne < 1)
                                                            uvIndexPlusOne = 1;
                                                        if (uvIndexPlusOne > 8)
                                                            uvIndexPlusOne = 8;
                                                        matchingSamplerConfig.UVIndex = uvIndexPlusOne - 1;


                                                        int uvGroupPlusOne = matchingSamplerConfig.UVGroup + 1;
                                                        ImGui.InputInt($"UV Group###SceneManager_Mesh{index_Mesh}_Material{index_Material}_Sampler{index_Sampler}_UVMapGroup", ref uvGroupPlusOne);
                                                        if (uvGroupPlusOne < 1)
                                                            uvGroupPlusOne = 1;
                                                        if (uvGroupPlusOne > 8)
                                                            uvGroupPlusOne = 8;
                                                        matchingSamplerConfig.UVGroup = uvGroupPlusOne - 1;


                                                        ImGui.LabelText($"Default Texture###SceneManager_Mesh{index_Mesh}_Material{index_Material}_Sampler{index_Sampler}_DefaultTexName", matchingSamplerConfig.DefaultTexPath ?? "<NULL>");
                                                    }
                                                    else
                                                    {
                                                        ImGui.LabelText(" ", "Error: SamplerConfig not found.");
                                                    }

                                                    
                                                    float uvSizeX = s.UVScale.X, uvSizeY = s.UVScale.Y;
                                                    ImGui.InputFloat($"UV Scale X###SceneManager_Mesh{index_Mesh}_Material{index_Material}_Sampler{index_Sampler}_UVScaleX", ref uvSizeX);
                                                    ImGui.InputFloat($"UV Scale Y###SceneManager_Mesh{index_Mesh}_Material{index_Material}_Sampler{index_Sampler}_UVScaleY", ref uvSizeY);
                                                    s.UVScale = new Microsoft.Xna.Framework.Vector2(uvSizeX, uvSizeY);

                                                    ImGui.Separator();

                                                    bool prevDebugView = s.IsDebugView;

                                                    ImGui.Checkbox($"Lock Debug Render On###SceneManager_Mesh{index_Mesh}_Material{index_Material}_Sampler{index_Sampler}_IsDebugView", ref s.IsDebugView);
                                                    FlverShaderEnums.NewDebug_ShowTex_ChannelConfigs_Picker.ShowPickerCombo("Debug Render Channels", 
                                                        $"SceneManager_Mesh{index_Mesh}_Material{index_Material}_Sampler{index_Sampler}_ChannelConfig_ForDebug", 
                                                        ref s.ChannelConfig_ForDebug, NewDebug_ShowTex_ChannelConfigs.RGBA);

                                                    if (s.IsDebugView)
                                                    {
                                                        GFX.NewDebug_ShowTex_Material = m;
                                                        GFX.NewDebug_ShowTex_Sampler = s;
                                                        GFX.NewDebug_ShowTex_SamplerConfig = matchingSamplerConfig;

                                                        // On initial enable, shutoff all others
                                                        if (!prevDebugView)
                                                        {
                                                            var memes = m.Samplers.Where(smp => smp != s).ToList();
                                                            foreach (var meme in memes)
                                                                meme.IsDebugView = false;
                                                        }
                                                    }

                                                    ImGui.TreePop();
                                                }
                                                else
                                                {
                                                    s.IsDebugView = false;
                                                }

                                                index_Sampler++;

                                            }

                                            ImGui.TreePop();
                                        }


                                        ImGui.Separator();
                                        ImGui.LabelText(" ", " ");
                                        ImGui.Separator();

                                        ImGui.TreePop();
                                    }

                                    

                                    index_Material++;
                                }

                                ImGui.TreePop();
                            }
                           
                        }

                        if (meshName != null)
                        {
                            var dispMeshName = mesh?.Name != null ? Utils.GetShortIngameFileName(mesh.Name) : "None";
                            if (dispMeshName.Contains("_"))
                                dispMeshName = dispMeshName.ToUpperInvariant();
                            if (dispMeshName.Contains("facegen"))
                                dispMeshName = dispMeshName.Replace("facegen", "FaceGen");
                            if (ImGui.TreeNode($"{meshName} <{dispMeshName}>"))
                            {
                                inner_Submeshes();
                                inner_Materials();
                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            inner_Submeshes();
                            inner_Materials();
                        }

                        index_Mesh++;
                    }

                    if (ImGui.TreeNode("Models"))
                    {
                        GFX.HideFLVERs = !MenuBar.Checkbox("Render Models", !GFX.HideFLVERs);

                        ImGui.Separator();

                        void DoModel(Model mdl)
                        {
                            if (ImGui.TreeNode(mdl.Name))
                            {
                                var maskDict = mdl.GetMaterialNamesPerMask();


                                foreach (var kvp in maskDict)
                                {
                                    if (kvp.Key >= 0)
                                        mdl.DefaultDrawMask[kvp.Key] = MenuBar.Checkbox($"Mask {kvp.Key}", mdl.DefaultDrawMask[kvp.Key]);
                                }

                                ImGui.Separator();

                                DoMesh(mdl, mdl.MainMesh, null);

                                if (mdl.ChrAsm != null)
                                {
                                    
                                    ImGui.Separator();
                                    DoMesh(mdl, mdl.ChrAsm.HeadMesh, "Head");
                                    DoMesh(mdl, mdl.ChrAsm.BodyMesh, "Body");
                                    DoMesh(mdl, mdl.ChrAsm.ArmsMesh, "Arms");
                                    DoMesh(mdl, mdl.ChrAsm.LegsMesh, "Legs");
                                    ImGui.Separator();
                                    DoMesh(mdl, mdl.ChrAsm.FaceMesh, "Naked Body");
                                    DoMesh(mdl, mdl.ChrAsm.FacegenMesh, "Face");
                                    DoMesh(mdl, mdl.ChrAsm.HairMesh, "Hair");

                                    if (mdl.ChrAsm.RightWeaponModel0?.MainMesh != null || 
                                        mdl.ChrAsm.RightWeaponModel1?.MainMesh != null || 
                                        mdl.ChrAsm.RightWeaponModel2?.MainMesh != null || 
                                        mdl.ChrAsm.RightWeaponModel3?.MainMesh != null)
                                    {
                                        ImGui.Separator();
                                        DoMesh(mdl, mdl.ChrAsm.RightWeaponModel0?.MainMesh, "Right Weapon[0]");
                                        DoMesh(mdl, mdl.ChrAsm.RightWeaponModel1?.MainMesh, "Right Weapon[1]");
                                        DoMesh(mdl, mdl.ChrAsm.RightWeaponModel2?.MainMesh, "Right Weapon[2]");
                                        DoMesh(mdl, mdl.ChrAsm.RightWeaponModel3?.MainMesh, "Right Weapon[3]");
                                    }

                                    if (mdl.ChrAsm.LeftWeaponModel0?.MainMesh != null ||
                                        mdl.ChrAsm.LeftWeaponModel1?.MainMesh != null ||
                                        mdl.ChrAsm.LeftWeaponModel2?.MainMesh != null ||
                                        mdl.ChrAsm.LeftWeaponModel3?.MainMesh != null)
                                    {
                                        ImGui.Separator();
                                        DoMesh(mdl, mdl.ChrAsm.LeftWeaponModel0?.MainMesh, "Left Weapon[0]");
                                        DoMesh(mdl, mdl.ChrAsm.LeftWeaponModel1?.MainMesh, "Left Weapon[1]");
                                        DoMesh(mdl, mdl.ChrAsm.LeftWeaponModel2?.MainMesh, "Left Weapon[2]");
                                        DoMesh(mdl, mdl.ChrAsm.LeftWeaponModel3?.MainMesh, "Left Weapon[3]");
                                    }


                                    
                                }

                                ImGui.TreePop();
                            }


                        }

                        foreach (var mdl in Scene.Models)
                        {


                            DoModel(mdl);
                            
                        }
                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("Helpers"))
                    {
                        lock (DBG._lock_DebugDrawEnablers)
                        {
                            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone] =
                                MenuBar.Checkbox("Bone Lines",
                                DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone],
                                enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperFlverBone);

                            DBG.CategoryEnableNameDraw[DebugPrimitives.DbgPrimCategory.FlverBone] =
                                MenuBar.Checkbox("Bone Names",
                                DBG.CategoryEnableNameDraw[DebugPrimitives.DbgPrimCategory.FlverBone],
                                enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperFlverBone);

                            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox] =
                                MenuBar.Checkbox("Bone Boxes",
                                DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox],
                                enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperFlverBoneBoundingBox);

                            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] =
                                MenuBar.Checkbox("DummyPoly",
                                DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
                                enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperDummyPoly);

                            DBG.CategoryEnableNameDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] =
                                MenuBar.Checkbox("DummyPoly IDs",
                                DBG.CategoryEnableNameDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
                                enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperDummyPoly);

                            NewDummyPolyManager.ShowGlobalIDOffset = MenuBar.Checkbox("Show c0000 Weapon Global \nDummyPoly ID Values (10000+)", NewDummyPolyManager.ShowGlobalIDOffset);
                            ImGui.NewLine();
                            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.SoundEvent] =
                                MenuBar.Checkbox("Sound Instances",
                                DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.SoundEvent],
                                enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperSoundEvent);

                            RemoManager.EnableDummyPrims =
                                MenuBar.Checkbox("Cutscene Dummies",
                                RemoManager.EnableDummyPrims,
                                enabled: true, shortcut: "(This Color)", shortcutColor: Microsoft.Xna.Framework.Color.Lime);

                        }

                        ImGui.Separator();

                        Tae.Config.DbgPrimXRay = MenuBar.Checkbox("Helper X-Ray Mode", Tae.Config.DbgPrimXRay);

                        ImGui.TreePop();
                    }
                }

                ImGui.Separator();

                WorldViewManager.DrawImguiPanel();
            }
        }
    }
}
