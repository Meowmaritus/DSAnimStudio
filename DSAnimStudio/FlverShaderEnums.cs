using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
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
        TEXDEBUG_UVCHECK_0 = 7,
        TEXDEBUG_UVCHECK_1 = 8,
        //TEXDEBUG_UV1_DIFFUSE1 = 9,
        //TEXDEBUG_UV1_DIFFUSE2 = 10,
        //TEXDEBUG_UV2_DIFFUSE1 = 11,
        //TEXDEBUG_UV2_DIFFUSE2 = 12,

        MESHDEBUG_NORMALS = 100,
        MESHDEBUG_NORMALS_MESH_ONLY = 101,
        MESHDEBUG_VERTEX_COLOR_ALPHA = 102,
        MESHDEBUG_VERTEX_COLOR_RGB = 103,

        LEGACY = 200,
        PBR_GLOSS_DS3 = 201,
        PBR_GLOSS_BB = 202,
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
