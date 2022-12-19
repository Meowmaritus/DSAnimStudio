using DSAnimStudio.ImguiOSD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class FlverShaderEnums
    {
        public static readonly ImGuiEnumListBoxPicker<NewDebugTypes> NewDebugTypes_Picker = new ImGuiEnumListBoxPicker<NewDebugTypes>(new Dictionary<NewDebugTypes, string>
            {
                { NewDebugTypes.None, "None" },
                { NewDebugTypes.Albedo, "Albedo" },
                { NewDebugTypes.Specular, "Specular" },
                { NewDebugTypes.Metalness, "Metalness" },
                { NewDebugTypes.Roughness, "Roughness" },
                { NewDebugTypes.Normals, "Normals" },
                { NewDebugTypes.ReflectanceMult, "Reflectance Mult" },
                { NewDebugTypes.BlendMask, "Blend Mask" },
                { NewDebugTypes.PBR_Direct, "PBR - Direct" },
                { NewDebugTypes.PBR_DirectDiffuse, "PBR - Direct Diffuse" },
                { NewDebugTypes.PBR_DirectSpecular, "PBR - Direct Specular" },
                { NewDebugTypes.PBR_Indirect, "PBR - Indirect" },
                { NewDebugTypes.PBR_IndirectDiffuse, "PBR - Indirect Diffuse" },
                { NewDebugTypes.PBR_IndirectSpecular, "PBR - Indirect Specular" },
                { NewDebugTypes.PBR_Diffuse, "PBR - All Diffuse" },
                { NewDebugTypes.PBR_Specular, "PBR - All Specular" },

            });

        public static readonly ImGuiEnumListBoxPicker<NewBlendOperations> NewBlendOperations_Picker = new ImGuiEnumListBoxPicker<NewBlendOperations>(new Dictionary<NewBlendOperations, string>
            {
                { NewBlendOperations.Always0, "Map A Only" },
                { NewBlendOperations.Always1, "Map B Only" },
                { NewBlendOperations.Lerp, "Linear Interpolation" },
                { NewBlendOperations.Multiply, "Multiplication" },
                { NewBlendOperations.Divide, "Division" },
                { NewBlendOperations.Add, "Addition" },
                { NewBlendOperations.Subtract, "Subtraction" },
                { NewBlendOperations.NormalMapBlend, "Normal Map Blend" },
            });

        public static readonly ImGuiEnumListBoxPicker<NewDebug_ShowTex_ChannelConfigs> NewDebug_ShowTex_ChannelConfigs_Picker = new ImGuiEnumListBoxPicker<NewDebug_ShowTex_ChannelConfigs>(new Dictionary<NewDebug_ShowTex_ChannelConfigs, string>
            {
                { NewDebug_ShowTex_ChannelConfigs.RGBA, "RGBA" },
                { NewDebug_ShowTex_ChannelConfigs.RGB, "RGB" },
                { NewDebug_ShowTex_ChannelConfigs.R, "R" },
                { NewDebug_ShowTex_ChannelConfigs.G, "G" },
                { NewDebug_ShowTex_ChannelConfigs.B, "B" },
                { NewDebug_ShowTex_ChannelConfigs.A, "A" },
            });
    }

    public enum NewDebug_ShowTex_ChannelConfigs : int
    {
        RGBA = 0,
        RGB = 1,
        R = 2,
        G = 3,
        B = 4,
        A = 5,
    }

    public enum NewDebugTypes : int
    {
        None = 0,

        Albedo = 100,
        Specular = 101,
        Metalness = 102,
        Roughness = 103,
        Normals = 104,
        ReflectanceMult = 105,
        BlendMask = 106,

        ShowTex = 200,

        PBR_Direct = 300,
        PBR_Indirect = 301,
        PBR_DirectDiffuse = 302,
        PBR_DirectSpecular = 303,
        PBR_IndirectDiffuse = 304,
        PBR_IndirectSpecular = 305,
        PBR_Diffuse = 306,
        PBR_Specular = 307,
    }

    public enum NewBlendOperations : int
    {
        Always0 = 0,
        Always1 = 1,
        Multiply = 2,
        Lerp = 3,
        Divide = 4,
        Add = 5,
        Subtract = 6,
        NormalMapBlend = 7,
    }

    public enum NewDebugTexTypes : int
    {
        DIFFUSE = 0,
        SPECULAR = 1,
        NORMAL = 2,
        EMISSIVE = 3,
        BLENDMASK = 4,
        SHININESS = 5,
    }

    public enum FlverShadingModes
    {
        HIGHLIGHT = -2,

        DEFAULT = -1,

        TEXDEBUG_DIFFUSEMAP = 0,
        TEXDEBUG_SPECULARMAP = 1,
        TEXDEBUG_NORMALMAP = 2,
        TEXDEBUG_EMISSIVEMAP = 3,
        TEXDEBUG_BLENDMASKMAP = 4,
        TEXDEBUG_SHININESSMAP = 5,
        TEXDEBUG_NORMALMAP_BLUE = 6,
        TEXDEBUG_UVCHECK_0 = 10,
        TEXDEBUG_UVCHECK_1 = 11,
        TEXDEBUG_UVCHECK_2 = 12,
        TEXDEBUG_UVCHECK_3 = 13,
        TEXDEBUG_UVCHECK_4 = 14,
        TEXDEBUG_UVCHECK_5 = 15,
        TEXDEBUG_UVCHECK_6 = 16,
        TEXDEBUG_UVCHECK_7 = 17,
        //TEXDEBUG_UVCHECK_1 = 8,
        //TEXDEBUG_UV1_DIFFUSE1 = 9,
        //TEXDEBUG_UV1_DIFFUSE2 = 10,
        //TEXDEBUG_UV2_DIFFUSE1 = 11,
        //TEXDEBUG_UV2_DIFFUSE2 = 12,

        MESHDEBUG_NORMALS = 100,
        MESHDEBUG_NORMALS_MESH_ONLY = 101,
        MESHDEBUG_VERTEX_COLOR_ALPHA = 102,
        MESHDEBUG_VERTEX_COLOR_RGB = 103,

        LEGACY = 200,
        PBR_GLOSS = 201,
        CLASSIC_DIFFUSE_PTDE = 203,
    }

    public enum PtdeMtdTypes : int
    {
        PTDE_MTD_TYPE_DEFAULT = 0,
        PTDE_MTD_TYPE_METAL = 1,
        PTDE_MTD_TYPE_WET = 2,
        PTDE_MTD_TYPE_DULL = 3,
    }
}
