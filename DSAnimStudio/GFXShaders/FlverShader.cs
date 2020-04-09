using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.GFXShaders
{
    public class FlverShader : Effect, IGFXShader<FlverShader>
    {
        public const int MaxBonePerMatrixArray = 255;

        public FlverShader Effect => this;

        public FlverShadingMode WorkflowType
        {
            get => (FlverShadingMode)Parameters[nameof(WorkflowType)].GetValueInt32();
            set => Parameters[nameof(WorkflowType)].SetValue((int)value);
        }

        public bool DisableAlpha
        {
            get => Parameters[nameof(DisableAlpha)].GetValueBoolean();
            set => Parameters[nameof(DisableAlpha)].SetValue(value);
        }

        public bool EnableBlendTextures
        {
            get => Parameters[nameof(EnableBlendTextures)].GetValueSingle() > 0;
            set => Parameters[nameof(EnableBlendTextures)].SetValue(value ? 1.0f : 0.0f);
        }

        public bool EnableBlendMaskMap
        {
            get => Parameters[nameof(EnableBlendMaskMap)].GetValueBoolean();
            set => Parameters[nameof(EnableBlendMaskMap)].SetValue(value);
        }

        public Vector3 LightDirection
        {
            get => Parameters[nameof(LightDirection)].GetValueVector3();
            set => Parameters[nameof(LightDirection)].SetValue(value);
        }

        public float AlphaTest
        {
            get => Parameters[nameof(AlphaTest)].GetValueSingle();
            set => Parameters[nameof(AlphaTest)].SetValue(value);
        }

        public Vector3 EyePosition
        {
            get => Parameters[nameof(EyePosition)].GetValueVector3();
            set => Parameters[nameof(EyePosition)].SetValue(value);
        }

        public float AmbientLightMult
        {
            get => Parameters[nameof(AmbientLightMult)].GetValueSingle();
            set => Parameters[nameof(AmbientLightMult)].SetValue(value);
        }

        public float DirectLightMult
        {
            get => Parameters[nameof(DirectLightMult)].GetValueSingle();
            set => Parameters[nameof(DirectLightMult)].SetValue(value);
        }

        public float IndirectLightMult
        {
            get => Parameters[nameof(IndirectLightMult)].GetValueSingle();
            set => Parameters[nameof(IndirectLightMult)].SetValue(value);
        }

        public float EmissiveMapMult
        {
            get => Parameters[nameof(EmissiveMapMult)].GetValueSingle();
            set => Parameters[nameof(EmissiveMapMult)].SetValue(value);
        }

        public float SceneBrightness
        {
            get => Parameters[nameof(SceneBrightness)].GetValueSingle();
            set => Parameters[nameof(SceneBrightness)].SetValue(value);
        }

        public float Opacity
        {
            get => Parameters[nameof(Opacity)].GetValueSingle();
            set => Parameters[nameof(Opacity)].SetValue(value);
        }

        public bool IsSkybox
        {
            get => Parameters[nameof(IsSkybox)].GetValueBoolean();
            set => Parameters[nameof(IsSkybox)].SetValue(value);
        }

        #region MATRICES
        public Matrix World
        {
            get => Parameters[nameof(World)].GetValueMatrix();
            set => Parameters[nameof(World)].SetValue(value);
        }

        public Matrix View
        {
            get => Parameters[nameof(View)].GetValueMatrix();
            set => Parameters[nameof(View)].SetValue(value);
        }

        //public Matrix ViewInverse
        //{
        //    get => Parameters[nameof(ViewInverse)].GetValueMatrix();
        //    set => Parameters[nameof(ViewInverse)].SetValue(value);
        //}

        public Matrix Projection
        {
            get => Parameters[nameof(Projection)].GetValueMatrix();
            set => Parameters[nameof(Projection)].SetValue(value);
        }
        #endregion


        #region LEGACY
        public float Legacy_NormalMapCustomZ
        {
            get => Parameters[nameof(Legacy_NormalMapCustomZ)].GetValueSingle();
            set => Parameters[nameof(Legacy_NormalMapCustomZ)].SetValue(value);
        }

        public float Legacy_DiffusePower
        {
            get => Parameters[nameof(Legacy_DiffusePower)].GetValueSingle();
            set => Parameters[nameof(Legacy_DiffusePower)].SetValue(value);
        }

        public Vector4 Legacy_SpecularColor
        {
            get => Parameters[nameof(Legacy_SpecularColor)].GetValueVector4();
            set => Parameters[nameof(Legacy_SpecularColor)].SetValue(value);
        }

        public float Legacy_SpecularPower
        {
            get => Parameters[nameof(Legacy_SpecularPower)].GetValueSingle();
            set => Parameters[nameof(Legacy_SpecularPower)].SetValue(value);
        }

        public Vector4 Legacy_AmbientColor
        {
            get => Parameters[nameof(Legacy_AmbientColor)].GetValueVector4();
            set => Parameters[nameof(Legacy_AmbientColor)].SetValue(value);
        }

        public float Legacy_AmbientIntensity
        {
            get => Parameters[nameof(Legacy_AmbientIntensity)].GetValueSingle();
            set => Parameters[nameof(Legacy_AmbientIntensity)].SetValue(value);
        }

        public Vector4 Legacy_DiffuseColor
        {
            get => Parameters[nameof(Legacy_DiffuseColor)].GetValueVector4();
            set => Parameters[nameof(Legacy_DiffuseColor)].SetValue(value);
        }

        public float Legacy_DiffuseIntensity
        {
            get => Parameters[nameof(Legacy_DiffuseIntensity)].GetValueSingle();
            set => Parameters[nameof(Legacy_DiffuseIntensity)].SetValue(value);
        }

        public float Legacy_SceneBrightness
        {
            get => Parameters[nameof(Legacy_SceneBrightness)].GetValueSingle();
            set => Parameters[nameof(Legacy_SceneBrightness)].SetValue(value);
        }

        #endregion

        #region TEXTURE MAPS
        public Texture2D ColorMap
        {
            get => Parameters[nameof(ColorMap)].GetValueTexture2D();
            set => Parameters[nameof(ColorMap)]?.SetValue(value);
        }

        public Texture2D NormalMap
        {
            get => Parameters[nameof(NormalMap)].GetValueTexture2D();
            set => Parameters[nameof(NormalMap)]?.SetValue(value);
        }

        public Texture2D SpecularMap
        {
            get => Parameters[nameof(SpecularMap)].GetValueTexture2D();
            set => Parameters[nameof(SpecularMap)]?.SetValue(value);
        }

        public Texture2D ColorMap2
        {
            get => Parameters[nameof(ColorMap2)].GetValueTexture2D();
            set => Parameters[nameof(ColorMap2)]?.SetValue(value);
        }

        public Texture2D NormalMap2
        {
            get => Parameters[nameof(NormalMap2)].GetValueTexture2D();
            set => Parameters[nameof(NormalMap2)]?.SetValue(value);
        }

        public Texture2D SpecularMap2
        {
            get => Parameters[nameof(SpecularMap2)].GetValueTexture2D();
            set => Parameters[nameof(SpecularMap2)]?.SetValue(value);
        }

        public Texture2D SpecularMapBB
        {
            get => Parameters[nameof(SpecularMapBB)].GetValueTexture2D();
            set => Parameters[nameof(SpecularMapBB)]?.SetValue(value);
        }

        public Texture2D EmissiveMap
        {
            get => Parameters[nameof(EmissiveMap)].GetValueTexture2D();
            set => Parameters[nameof(EmissiveMap)]?.SetValue(value);
        }

        public Texture2D BlendmaskMap
        {
            get => Parameters[nameof(BlendmaskMap)].GetValueTexture2D();
            set => Parameters[nameof(BlendmaskMap)]?.SetValue(value);
        }

        public TextureCube EnvironmentMap
        {
            get => Parameters[nameof(EnvironmentMap)].GetValueTextureCube();
            set => Parameters[nameof(EnvironmentMap)]?.SetValue(value);
        }


        public Vector2 ColorMapScale
        {
            get => Parameters[nameof(ColorMapScale)].GetValueVector2();
            set => Parameters[nameof(ColorMapScale)]?.SetValue(value);
        }

        public Vector2 NormalMapScale
        {
            get => Parameters[nameof(NormalMapScale)].GetValueVector2();
            set => Parameters[nameof(NormalMapScale)]?.SetValue(value);
        }

        public Vector2 SpecularMapScale
        {
            get => Parameters[nameof(SpecularMapScale)].GetValueVector2();
            set => Parameters[nameof(SpecularMapScale)]?.SetValue(value);
        }

        public Vector2 ColorMapScale2
        {
            get => Parameters[nameof(ColorMapScale2)].GetValueVector2();
            set => Parameters[nameof(ColorMapScale2)]?.SetValue(value);
        }

        public Vector2 NormalMapScale2
        {
            get => Parameters[nameof(NormalMapScale2)].GetValueVector2();
            set => Parameters[nameof(NormalMapScale2)]?.SetValue(value);
        }

        public Vector2 SpecularMapScale2
        {
            get => Parameters[nameof(SpecularMapScale2)].GetValueVector2();
            set => Parameters[nameof(SpecularMapScale2)]?.SetValue(value);
        }

        public Vector2 SpecularMapScaleBB
        {
            get => Parameters[nameof(SpecularMapScaleBB)].GetValueVector2();
            set => Parameters[nameof(SpecularMapScaleBB)]?.SetValue(value);
        }

        public Vector2 EmissiveMapScale
        {
            get => Parameters[nameof(EmissiveMapScale)].GetValueVector2();
            set => Parameters[nameof(EmissiveMapScale)]?.SetValue(value);
        }

        public Vector2 BlendmaskMapScale
        {
            get => Parameters[nameof(BlendmaskMapScale)].GetValueVector2();
            set => Parameters[nameof(BlendmaskMapScale)]?.SetValue(value);
        }

        #endregion

        #region SKINNING
        public Matrix[] Bones0
        {
            get => Parameters[nameof(Bones0)].GetValueMatrixArray(MaxBonePerMatrixArray);
            set => Parameters[nameof(Bones0)]?.SetValue(value);
        }

        public Matrix[] Bones1
        {
            get => Parameters[nameof(Bones1)].GetValueMatrixArray(MaxBonePerMatrixArray);
            set => Parameters[nameof(Bones1)]?.SetValue(value);
        }

        public Matrix[] Bones2
        {
            get => Parameters[nameof(Bones2)].GetValueMatrixArray(MaxBonePerMatrixArray);
            set => Parameters[nameof(Bones2)]?.SetValue(value);
        }

        public Matrix[] Bones3
        {
            get => Parameters[nameof(Bones3)].GetValueMatrixArray(MaxBonePerMatrixArray);
            set => Parameters[nameof(Bones3)]?.SetValue(value);
        }

        public Matrix[] Bones4
        {
            get => Parameters[nameof(Bones4)].GetValueMatrixArray(MaxBonePerMatrixArray);
            set => Parameters[nameof(Bones4)]?.SetValue(value);
        }

        public Matrix[] Bones5
        {
            get => Parameters[nameof(Bones5)].GetValueMatrixArray(MaxBonePerMatrixArray);
            set => Parameters[nameof(Bones5)]?.SetValue(value);
        }
        #endregion

        public FlverShader(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
        }

        public FlverShader(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
        }

        public FlverShader(Effect cloneSource) : base(cloneSource)
        {
        }

        public void ApplyWorldView(Matrix world, Matrix view, Matrix projection)
        {
            World = world;
            View = view;
            Projection = projection;
            //ViewInverse = Matrix.Invert(view);
        }
    }
}
