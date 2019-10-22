using DSAnimStudio.GFXShaders;
using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using MeowDSIO.DataTypes.FLVER;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace DSAnimStudio
{
    public class FlverSubmeshRenderer : IDisposable
    {
        public BoundingBox Bounds;

        private struct FlverSubmeshRendererFaceSet
        {
            public int IndexCount;
            public IndexBuffer IndexBuffer;
            public bool BackfaceCulling;
            public bool IsTriangleStrip;
            public byte LOD;
            public bool IsMotionBlur;
        }

        List<FlverSubmeshRendererFaceSet> MeshFacesets = new List<FlverSubmeshRendererFaceSet>();

        private bool HasNoLODs = true;

        VertexBuffer VertBuffer;
        //VertexBufferBinding VertBufferBinding;

        public string TexNameDiffuse { get; private set; } = null;
        public string TexNameDiffuse2 { get; private set; } = null;
        public string TexNameSpecular { get; private set; } = null;
        public string TexNameSpecular2 { get; private set; } = null;
        public string TexNameNormal { get; private set; } = null;
        public string TexNameNormal2 { get; private set; } = null;
        public string TexNameEmissive { get; private set; } = null;
        public string TexNameShininess { get; private set; } = null;
        public string TexNameBlendmask { get; private set; } = null;
        public string TexNameDOL1 { get; private set; } = null;
        public string TexNameDOL2 { get; private set; } = null;

        public Texture2D TexDataDiffuse { get; private set; } = null;
        public Texture2D TexDataDiffuse2 { get; private set; } = null;
        public Texture2D TexDataSpecular { get; private set; } = null;
        public Texture2D TexDataSpecular2 { get; private set; } = null;
        public Texture2D TexDataNormal { get; private set; } = null;
        public Texture2D TexDataNormal2 { get; private set; } = null;
        public Texture2D TexDataEmissive { get; private set; } = null;
        public Texture2D TexDataShininess { get; private set; } = null;
        public Texture2D TexDataBlendmask { get; private set; } = null;
        public Texture2D TexDataDOL1 { get; private set; } = null;
        public Texture2D TexDataDOL2 { get; private set; } = null;

        public GFXDrawStep DrawStep { get; private set; }

        public int VertexCount { get; private set; }

        public readonly NewMesh Parent;

        public bool IsVisible { get; set; } = true;

        private string _fullMaterialName;
        public string FullMaterialName
        {
            get => _fullMaterialName;
            set
            {
                if (_fullMaterialName != value)
                {
                    _fullMaterialName = value;
                    var shit = GetModelMaskIndexAndPrettyNameStartForCurrentMaterialName();
                    ModelMaskIndex = shit.MaskIndex;
                    PrettyMaterialName = value.Substring(shit.SubstringStartIndex).Trim();
                }
            }
        }

        public string PrettyMaterialName { get; private set; }

        private (int MaskIndex, int SubstringStartIndex) GetModelMaskIndexAndPrettyNameStartForCurrentMaterialName()
        {
            if (string.IsNullOrEmpty(FullMaterialName))
                return (-1, 0);

            int firstHashtag = FullMaterialName.IndexOf("#");
            if (firstHashtag == -1)
                return (-1, 0);
            int secondHashtagSearchStart = firstHashtag + 1;
            int secondHashtag = FullMaterialName.Substring(secondHashtagSearchStart).IndexOf("#");
            if (secondHashtag == -1)
                return (-1, 0);
            else
                secondHashtag += secondHashtagSearchStart;

            string maskText = FullMaterialName.Substring(secondHashtagSearchStart, secondHashtag - secondHashtagSearchStart);

            if (int.TryParse(maskText, out int mask))
                return (mask, secondHashtag + 1);
            else
                return (-1, 0);
        }

        //public string DefaultBoneName { get; set; } = null;
        public int DefaultBoneIndex { get; set; } = -1;

        public FlverShadingMode ShadingMode { get; set; } = FlverShadingMode.PBR_GLOSS_DS3;

        public int ModelMaskIndex { get; private set; }

        static System.Numerics.Vector3 SkinVector3(System.Numerics.Vector3 vOof, Matrix[] bones, FLVER.VertexBoneWeights weights, bool isNormal = false)
        {
            var v = new Vector3(vOof.X, vOof.Y, vOof.Z);
            Vector3 a = isNormal ? Vector3.TransformNormal(v, bones[0]) * weights[0] : Vector3.Transform(v, bones[0]) * weights[0];
            Vector3 b = isNormal ? Vector3.TransformNormal(v, bones[1]) * weights[1] : Vector3.Transform(v, bones[1]) * weights[1];
            Vector3 c = isNormal ? Vector3.TransformNormal(v, bones[2]) * weights[2] : Vector3.Transform(v, bones[2]) * weights[2];
            Vector3 d = isNormal ? Vector3.TransformNormal(v, bones[3]) * weights[3] : Vector3.Transform(v, bones[3]) * weights[3];

            var r = (a + b + c + d) / (weights[0] + weights[1] + weights[2] + weights[3]);
            return new System.Numerics.Vector3(r.X, r.Y, r.Z);
        }

        static System.Numerics.Vector4 SkinVector4(System.Numerics.Vector4 vOof, Matrix[] bones, FLVER.VertexBoneWeights weights, bool isNormal = false)
        {
            var v = new Vector4(vOof.X, vOof.Y, vOof.Z, vOof.W);

            Vector3 a = isNormal ? Vector3.TransformNormal(v.XYZ(), bones[0]) * weights[0] : Vector3.Transform(v.XYZ(), bones[0]) * weights[0];
            Vector3 b = isNormal ? Vector3.TransformNormal(v.XYZ(), bones[1]) * weights[1] : Vector3.Transform(v.XYZ(), bones[1]) * weights[1];
            Vector3 c = isNormal ? Vector3.TransformNormal(v.XYZ(), bones[2]) * weights[2] : Vector3.Transform(v.XYZ(), bones[2]) * weights[2];
            Vector3 d = isNormal ? Vector3.TransformNormal(v.XYZ(), bones[3]) * weights[3] : Vector3.Transform(v.XYZ(), bones[3]) * weights[3];

            var r = (a + b + c + d) / (weights[0] + weights[1] + weights[2] + weights[3]);
            return new System.Numerics.Vector4(r.X, r.Y, r.Z, v.W);
        }

        static void ApplySkin(FLVER.Vertex vert, 
            List<Matrix> boneMatrices, List<int> meshBoneIndices, bool usesMeshBoneIndices)
        {
            int i1 = vert.BoneIndices[0];
            int i2 = vert.BoneIndices[1];
            int i3 = vert.BoneIndices[2];
            int i4 = vert.BoneIndices[3];

            if (usesMeshBoneIndices)
            {
                i1 = meshBoneIndices[i1];
                i2 = meshBoneIndices[i2];
                i3 = meshBoneIndices[i3];
                i4 = meshBoneIndices[i4];
            }

            vert.Position = SkinVector3(vert.Position, new Matrix[]
                {
                    i1 >= 0 ? boneMatrices[i1] : Matrix.Identity,
                    i2 >= 0 ? boneMatrices[i2] : Matrix.Identity,
                    i3 >= 0 ? boneMatrices[i3] : Matrix.Identity,
                    i4 >= 0 ? boneMatrices[i4] : Matrix.Identity,
                }, vert.BoneWeights);

            vert.Normal = SkinVector3(vert.Normal, new Matrix[]
                {
                    i1 >= 0 ? boneMatrices[i1] : Matrix.Identity,
                    i2 >= 0 ? boneMatrices[i2] : Matrix.Identity,
                    i3 >= 0 ? boneMatrices[i3] : Matrix.Identity,
                    i4 >= 0 ? boneMatrices[i4] : Matrix.Identity,
                }, vert.BoneWeights, isNormal: true);

            vert.Bitangent = SkinVector4(vert.Bitangent, new Matrix[]
                {
                    i1 >= 0 ? boneMatrices[i1] : Matrix.Identity,
                    i2 >= 0 ? boneMatrices[i2] : Matrix.Identity,
                    i3 >= 0 ? boneMatrices[i3] : Matrix.Identity,
                    i4 >= 0 ? boneMatrices[i4] : Matrix.Identity,
                }, vert.BoneWeights, isNormal: true);

            vert.Tangents[0] = SkinVector4(vert.Tangents[0], new Matrix[]
                {
                    i1 >= 0 ? boneMatrices[i1] : Matrix.Identity,
                    i2 >= 0 ? boneMatrices[i2] : Matrix.Identity,
                    i3 >= 0 ? boneMatrices[i3] : Matrix.Identity,
                    i4 >= 0 ? boneMatrices[i4] : Matrix.Identity,
                }, vert.BoneWeights, isNormal: true);
        }

        public FlverSubmeshRenderer(NewMesh parent, FLVER2 flvr, FLVER2.Mesh mesh, 
            bool useSecondUV, Dictionary<string, int> boneIndexRemap = null,
            bool ignoreStaticTransforms = false)
        {
            if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
            {
                ShadingMode = FlverShadingMode.PBR_GLOSS_DS3;
            }
            else if (GameDataManager.GameType == GameDataManager.GameTypes.BB)
            {
                ShadingMode = FlverShadingMode.PBR_GLOSS_BB;
            }
            else if (GameDataManager.GameType == GameDataManager.GameTypes.DS1)
            {
                ShadingMode = FlverShadingMode.CLASSIC_DIFFUSE_PTDE;
            }
            else if (GameDataManager.GameType == GameDataManager.GameTypes.SDT)
            {
                //TEMP
                ShadingMode = FlverShadingMode.PBR_GLOSS_DS3;
            }
            else
            {
                ShadingMode = FlverShadingMode.TEXDEBUG_DIFFUSEMAP;
            }

            Parent = parent;

            FullMaterialName = flvr.Materials[mesh.MaterialIndex].Name;

            DefaultBoneIndex = mesh.DefaultBoneIndex;

            var shortMaterialName = Utils.GetFileNameWithoutDirectoryOrExtension(flvr.Materials[mesh.MaterialIndex].MTD);
            if (shortMaterialName.EndsWith("_Alp") ||
                shortMaterialName.Contains("_Edge") ||
                shortMaterialName.Contains("_Decal") ||
                shortMaterialName.Contains("_Cloth") ||
                shortMaterialName.Contains("_al") ||
                shortMaterialName.Contains("BlendOpacity"))
            {
                DrawStep = GFXDrawStep.AlphaEdge;
            }
            else
            {
                DrawStep = GFXDrawStep.Opaque;
            }

            //bool hasLightmap = false;

            Dictionary<int, int> finalBoneRemapper = null;

            if (boneIndexRemap != null)
            {
                finalBoneRemapper = new Dictionary<int, int>();
                for (int i = 0; i < flvr.Bones.Count; i++)
                {
                    if (boneIndexRemap.ContainsKey(flvr.Bones[i].Name))
                    {
                        finalBoneRemapper.Add(i, boneIndexRemap[flvr.Bones[i].Name]);
                    }
                }
            }

            //// For DS1 FLVER, just mess with the mesh bones beforehand, way simpler.
            //if (finalBoneRemapper != null && flvr.Header.Version <= 0x2000D)
            //{
            //    foreach (var m in flvr.Meshes)
            //    {
            //        for (int i = 0; i < m.BoneIndices.Count; i++)
            //        {
            //            if (finalBoneRemapper.ContainsKey(m.BoneIndices[i]))
            //                m.BoneIndices[i] = finalBoneRemapper[m.BoneIndices[i]];
            //        }

            //        if (finalBoneRemapper.ContainsKey(m.DefaultBoneIndex))
            //            m.DefaultBoneIndex = finalBoneRemapper[m.DefaultBoneIndex];
            //    }
            //}

            //bool isDs3FacegenSkin = flvr.Materials[mesh.MaterialIndex].MTD.Contains(@"N:\FDP\data\Material\mtd\parts\Special\P[ChrCustom_Skin]_SSS");

            foreach (var matParam in flvr.Materials[mesh.MaterialIndex].Textures)
            {
                var paramNameCheck = matParam.Type.ToUpper();
                string shortTexPath = Utils.GetShortIngameFileName(matParam.Path);
                // DS3/BB
                if (paramNameCheck.Contains("DIFFUSE_2") || paramNameCheck.Contains("ALBEDO_2"))
                    TexNameDiffuse2 = shortTexPath;
                else if (paramNameCheck.Contains("DIFFUSE") || paramNameCheck.Contains("ALBEDO"))
                    TexNameDiffuse = shortTexPath;
                else if (paramNameCheck.Contains("SPECULAR_2") || paramNameCheck.Contains("REFLECTANCE_2"))
                    TexNameSpecular2 = shortTexPath;
                else if (paramNameCheck.Contains("SPECULAR") || paramNameCheck.Contains("REFLECTANCE"))
                    TexNameSpecular = shortTexPath;
                else if ((paramNameCheck.Contains("BUMPMAP_2") && !paramNameCheck.Contains("DETAILBUMP_2"))
                    || paramNameCheck.Contains("NORMALMAP_2"))
                    TexNameNormal2 = shortTexPath;
                else if ((paramNameCheck.Contains("BUMPMAP") && !paramNameCheck.Contains("DETAILBUMP"))
                    || paramNameCheck.Contains("NORMALMAP"))
                    TexNameNormal = shortTexPath;
                else if (paramNameCheck.Contains("EMISSIVE"))
                    TexNameEmissive = shortTexPath;
                else if (paramNameCheck.Contains("SHININESS"))
                    TexNameShininess = shortTexPath;
                else if (paramNameCheck.Contains("BLENDMASK"))
                    TexNameBlendmask = shortTexPath;
                else if (paramNameCheck == "G_DOLTEXTURE1")
                {
                    TexNameDOL1 = shortTexPath;
                    //hasLightmap = true;
                }
                else if (paramNameCheck == "G_DOLTEXTURE2")
                    TexNameDOL2 = shortTexPath;
                // DS1 params
                else if (paramNameCheck == "G_LIGHTMAP")
                {
                    TexNameDOL1 = shortTexPath;
                    //hasLightmap = true;
                }
                else
                {
                    Console.WriteLine($"\nUnrecognized Material Param:\n    [{matParam.Type}]\n    [{matParam.Path}]\n");
                }
                // Alternate material params that work as diffuse
            }

            // MTD lookup
            //MTD mtd = null; //InterrootLoader.GetMTD(flvr.Materials[mesh.MaterialIndex].MTD);

            //var debug_LowestBoneWeight = float.MaxValue;
            //var debug_HighestBoneWeight = float.MinValue;

            //var debug_sortedByZ = new List<FLVER.Vertex>();

            Matrix GetBoneMatrix(SoulsFormats.FLVER2.Bone b)
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
                        parentBone = flvr.Bones[parentBone.ParentIndex];
                    else
                        parentBone = null;
                }
                while (parentBone != null);

                return result;
            }

            var MeshVertices = new FlverShaderVertInput[mesh.Vertices.Count];
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {

                var vert = mesh.Vertices[i];

                var ORIG_BONE_WEIGHTS = vert.BoneWeights;
                var ORIG_BONE_INDICES = vert.BoneIndices;

                MeshVertices[i] = new FlverShaderVertInput();

                if (vert.BoneWeights[0] == 0 && vert.BoneWeights[1] == 0 && vert.BoneWeights[2] == 0 && vert.BoneWeights[3] == 0)
                {
                    vert.BoneWeights[0] = 1;
                }

                // Apply normal W channel bone index (for some weapons etc)
                if (!vert.UsesBoneIndices)
                {
                    int boneIndex = vert.NormalW;

                    //if (boneIndex == 0 && mesh.DefaultBoneIndex != 0)
                    //    boneIndex = mesh.DefaultBoneIndex;

                    vert.BoneIndices[0] = boneIndex;
                    //vert.BoneIndices[1] = 0;
                    //vert.BoneIndices[2] = 0;
                    //vert.BoneIndices[3] = 0;

                    vert.BoneWeights[0] = 1;
                    //vert.BoneWeights[1] = 0;
                    //vert.BoneWeights[2] = 0;
                    //vert.BoneWeights[3] = 0;
                }

                // Apply bind pose of bone to actual vert if !mesh.Dynamic
                if (!mesh.Dynamic)
                {
                    ApplySkin(vert, flvr.Bones.Select(b => GetBoneMatrix(b)).ToList(), mesh.BoneIndices, (flvr.Header.Version <= 0x2000D));

                    //if (!vert.UsesBoneIndices)
                    //{
                    //    vert.BoneIndices[0] = 0;
                    //    vert.BoneIndices[1] = 0;
                    //    vert.BoneIndices[2] = 0;
                    //    vert.BoneIndices[3] = 0;

                    //    vert.BoneWeights[0] = 0;
                    //    vert.BoneWeights[1] = 0;
                    //    vert.BoneWeights[2] = 0;
                    //    vert.BoneWeights[3] = 0;
                    //}

                }

                //vert.BoneWeights = ORIG_BONE_WEIGHTS;
                //vert.BoneIndices = ORIG_BONE_INDICES;

                //for (int j = 0; j < 4; j++)
                //{
                //    if (vert.BoneIndices[j] > 0 && vert.BoneWeights[j] == 0)
                //    {
                //        vert.BoneIndices[j] = mesh.DefaultBoneIndex;
                //        vert.BoneWeights[j] = 1;
                //    }
                //}

                MeshVertices[i].BoneWeights = new Vector4(vert.BoneWeights[0], vert.BoneWeights[1], vert.BoneWeights[2], vert.BoneWeights[3]);

                // Apply per-mesh bone indices for DS1 and older
                if (flvr.Header.Version <= 0x2000D)
                {
                    // Hotfix for my own bad models imported with DSFBX / FBX2FLVER lol im sorry i learned now that
                    // they don't use -1
                    if (vert.BoneIndices[0] < 0)
                        vert.BoneIndices[0] = 0;
                    if (vert.BoneIndices[1] < 0)
                        vert.BoneIndices[1] = 0;
                    if (vert.BoneIndices[2] < 0)
                        vert.BoneIndices[2] = 0;
                    if (vert.BoneIndices[3] < 0)
                        vert.BoneIndices[3] = 0;

                    if (vert.BoneIndices[0] >= mesh.BoneIndices.Count)
                        vert.BoneIndices[0] = 0;
                    if (vert.BoneIndices[1] >= mesh.BoneIndices.Count)
                        vert.BoneIndices[1] = 0;
                    if (vert.BoneIndices[2] >= mesh.BoneIndices.Count)
                        vert.BoneIndices[2] = 0;
                    if (vert.BoneIndices[3] >= mesh.BoneIndices.Count)
                        vert.BoneIndices[3] = 0;

                    vert.BoneIndices[0] = mesh.BoneIndices[vert.BoneIndices[0]];
                    vert.BoneIndices[1] = mesh.BoneIndices[vert.BoneIndices[1]];
                    vert.BoneIndices[2] = mesh.BoneIndices[vert.BoneIndices[2]];
                    vert.BoneIndices[3] = mesh.BoneIndices[vert.BoneIndices[3]];
                }

                if (finalBoneRemapper != null)
                {
                    if (finalBoneRemapper.ContainsKey(vert.BoneIndices[0]))
                        vert.BoneIndices[0] = finalBoneRemapper[vert.BoneIndices[0]];

                    if (finalBoneRemapper.ContainsKey(vert.BoneIndices[1]))
                        vert.BoneIndices[1] = finalBoneRemapper[vert.BoneIndices[1]];

                    if (finalBoneRemapper.ContainsKey(vert.BoneIndices[2]))
                        vert.BoneIndices[2] = finalBoneRemapper[vert.BoneIndices[2]];

                    if (finalBoneRemapper.ContainsKey(vert.BoneIndices[3]))
                        vert.BoneIndices[3] = finalBoneRemapper[vert.BoneIndices[3]];
                }

                MeshVertices[i].BoneIndices = new Vector4(
                    (int)(vert.BoneIndices[0] >= 0 ? vert.BoneIndices[0] % FlverShader.MaxBonePerMatrixArray : -1),
                    (int)(vert.BoneIndices[1] >= 0 ? vert.BoneIndices[1] % FlverShader.MaxBonePerMatrixArray : -1),
                    (int)(vert.BoneIndices[2] >= 0 ? vert.BoneIndices[2] % FlverShader.MaxBonePerMatrixArray : -1),
                    (int)(vert.BoneIndices[3] >= 0 ? vert.BoneIndices[3] % FlverShader.MaxBonePerMatrixArray : -1));

                MeshVertices[i].BoneIndicesBank = new Vector4(
                   (float)(vert.BoneIndices[0] >= 0 ? Math.Floor(1.0f * vert.BoneIndices[0] / FlverShader.MaxBonePerMatrixArray) : -1.0),
                   (float)(vert.BoneIndices[1] >= 0 ? Math.Floor(1.0f * vert.BoneIndices[1] / FlverShader.MaxBonePerMatrixArray) : -1.0),
                   (float)(vert.BoneIndices[2] >= 0 ? Math.Floor(1.0f * vert.BoneIndices[2] / FlverShader.MaxBonePerMatrixArray) : -1.0),
                   (float)(vert.BoneIndices[3] >= 0 ? Math.Floor(1.0f * vert.BoneIndices[3] / FlverShader.MaxBonePerMatrixArray) : -1.0));

                if (vert.BoneIndices[0] < 0)
                    MeshVertices[i].BoneWeights.X = 0;

                if (vert.BoneIndices[1] < 0)
                    MeshVertices[i].BoneWeights.Y = 0;

                if (vert.BoneIndices[2] < 0)
                    MeshVertices[i].BoneWeights.Z = 0;

                if (vert.BoneIndices[3] < 0)
                    MeshVertices[i].BoneWeights.W = 0;

                vert.BoneWeights = ORIG_BONE_WEIGHTS;
                vert.BoneIndices = ORIG_BONE_INDICES;

                MeshVertices[i].Position = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);

                MeshVertices[i].Normal = Vector3.Normalize(new Vector3(vert.Normal.X, vert.Normal.Y, vert.Normal.Z));

                if (vert.Colors.Count >= 1)
                    MeshVertices[i].Color = new Vector4(vert.Colors[0].R, vert.Colors[0].G, vert.Colors[0].B, vert.Colors[0].A);

                if (vert.Tangents.Count > 0)
                {
                    MeshVertices[i].Bitangent = new Vector4(vert.Tangents[0].X, vert.Tangents[0].Y, vert.Tangents[0].Z, vert.Tangents[0].W);
                    MeshVertices[i].Binormal = Vector3.Cross(Vector3.Normalize(MeshVertices[i].Normal), Vector3.Normalize(new Vector3(MeshVertices[i].Bitangent.X, MeshVertices[i].Bitangent.Y, MeshVertices[i].Bitangent.Z))) * vert.Tangents[0].W;
                }

                //if (vert.BoneWeights[0] < debug_LowestBoneWeight)
                //    debug_LowestBoneWeight = vert.BoneWeights[0];

                //if (vert.BoneWeights[1] < debug_LowestBoneWeight)
                //    debug_LowestBoneWeight = vert.BoneWeights[1];

                //if (vert.BoneWeights[2] < debug_LowestBoneWeight)
                //    debug_LowestBoneWeight = vert.BoneWeights[2];

                //if (vert.BoneWeights[3] < debug_LowestBoneWeight)
                //    debug_LowestBoneWeight = vert.BoneWeights[3];


                //if (vert.BoneWeights[0] > debug_HighestBoneWeight)
                //    debug_HighestBoneWeight = vert.BoneWeights[0];

                //if (vert.BoneWeights[1] > debug_HighestBoneWeight)
                //    debug_HighestBoneWeight = vert.BoneWeights[1];

                //if (vert.BoneWeights[2] > debug_HighestBoneWeight)
                //    debug_HighestBoneWeight = vert.BoneWeights[2];

                //if (vert.BoneWeights[3] > debug_HighestBoneWeight)
                //    debug_HighestBoneWeight = vert.BoneWeights[3];



                //TESTING
                //if (MeshVertices[i].BoneIndices.X != 42)
                //{
                //    MeshVertices[i].BoneWeights = new Vector4(MeshVertices[i].BoneWeights.X, 0, 0, 0);
                //    MeshVertices[i].BoneIndices = new Vector4(MeshVertices[i].BoneIndices.X, 0, 0, 0);
                //}

                //if (MeshVertices[i].BoneIndices.X == 42)
                //{
                //    Console.WriteLine("DEBUG");
                //}

                //if (MeshVertices[i].BoneIndices.X == -1 || MeshVertices[i].BoneIndices.X > (FlverShader.NUM_BONES - 1))
                //    MeshVertices[i].BoneIndices.X = 0;

                //if (MeshVertices[i].BoneIndices.Y == -1 || MeshVertices[i].BoneIndices.Y > (FlverShader.NUM_BONES - 1))
                //    MeshVertices[i].BoneIndices.Y = 0;

                //if (MeshVertices[i].BoneIndices.Z == -1 || MeshVertices[i].BoneIndices.Z > (FlverShader.NUM_BONES - 1))
                //    MeshVertices[i].BoneIndices.Z = 0;

                //if (MeshVertices[i].BoneIndices.W == -1 || MeshVertices[i].BoneIndices.W > (FlverShader.NUM_BONES - 1))
                //    MeshVertices[i].BoneIndices.W = 0;



                //if (MeshVertices[i].BoneWeights.X < 0)
                //    MeshVertices[i].BoneWeights.X = 1;

                //if (MeshVertices[i].BoneWeights.Y < 0)
                //    MeshVertices[i].BoneWeights.Y = 1;

                //if (MeshVertices[i].BoneWeights.Z < 0)
                //    MeshVertices[i].BoneWeights.Z = 1;

                //if (MeshVertices[i].BoneWeights.W < 0)
                //    MeshVertices[i].BoneWeights.W = 1;
                //MeshVertices[i].BoneWeights = new Vector4(1,0,0,0);

                //if (MeshVertices[i].BoneIndices.X < 1)
                //    MeshVertices[i].BoneWeights.X = 0;

                //if (MeshVertices[i].BoneIndices.Y < 1)
                //    MeshVertices[i].BoneWeights.Y = 0;

                //if (MeshVertices[i].BoneIndices.Z < 1)
                //    MeshVertices[i].BoneWeights.Z = 0;

                //if (MeshVertices[i].BoneIndices.W < 1)
                //    MeshVertices[i].BoneWeights.W = 0;

                //MeshVertices[i] = ApplySkin(MeshVertices[i],
                //    vert.BoneWeights[0], vert.BoneWeights[1], vert.BoneWeights[2], vert.BoneWeights[3],
                //    vert.BoneIndices[0], vert.BoneIndices[1], vert.BoneIndices[2], vert.BoneIndices[3],
                //    flverTposeToHkxTposeMatrices);

                if (vert.UVs.Count > 0)
                {
                    if (useSecondUV && vert.UVs.Count > 1)
                        MeshVertices[i].TextureCoordinate = new Vector2(vert.UVs[1].X, vert.UVs[1].Y);
                    else
                        MeshVertices[i].TextureCoordinate = new Vector2(vert.UVs[0].X, vert.UVs[0].Y);

                    if (vert.UVs.Count >= 2)
                    {
                        MeshVertices[i].TextureCoordinate2 = new Vector2(vert.UVs[1].X, vert.UVs[1].Y);
                    }

                    //if (vert.UVs.Count > 1 && hasLightmap)
                    //{
                    //    if (mtd == null)
                    //    {
                    //        // Really stupid heuristic to determine light map UVs without reading mtd files or something
                    //        if (vert.UVs.Count > 2 && flvr.Materials[mesh.MaterialIndex].Textures.Count > 11)
                    //        {
                    //            MeshVertices[i].TextureCoordinate2 = new Vector2(vert.UVs[2].X, vert.UVs[2].Y);
                    //        }
                    //        else
                    //        {
                    //            MeshVertices[i].TextureCoordinate2 = new Vector2(vert.UVs[1].X, vert.UVs[1].Y);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // Better heuristic with MTDs
                    //        int uvindex = mtd.Textures.Find(tex => tex.Type.ToUpper() == "G_LIGHTMAP" || tex.Type.ToUpper() == "G_DOLTEXTURE1").UVNumber;
                    //        int uvoffset = 1;
                    //        for (int j = 1; j < uvindex; j++)
                    //        {
                    //            if (!mtd.Textures.Any(t => (t.UVNumber == j)))
                    //            {
                    //                uvoffset++;
                    //            }
                    //        }
                    //        uvindex -= uvoffset;
                    //        if (vert.UVs.Count > uvindex)
                    //        {
                    //            MeshVertices[i].TextureCoordinate2 = new Vector2(vert.UVs[uvindex].X, vert.UVs[uvindex].Y);
                    //        }
                    //        else
                    //        {
                    //            MeshVertices[i].TextureCoordinate2 = new Vector2(vert.UVs[1].X, vert.UVs[1].Y);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    MeshVertices[i].TextureCoordinate2 = Vector2.Zero;
                    //}
                }
                else
                {
                    MeshVertices[i].TextureCoordinate = Vector2.Zero;
                    MeshVertices[i].TextureCoordinate2 = Vector2.Zero;
                }
            }

            //debug_sortedByZ = debug_sortedByZ.OrderBy(v => v.Position.Z).ToList();

            VertexCount = MeshVertices.Length;

            MeshFacesets = new List<FlverSubmeshRendererFaceSet>();

            foreach (var faceset in mesh.FaceSets)
            {
                if (faceset.Indices.Count == 0)
                    continue;

                //At this point they use 32-bit faceset vertex indices
                bool is32bit = flvr.Header.Version > 0x20005;

                var newFaceSet = new FlverSubmeshRendererFaceSet()
                {
                    BackfaceCulling = faceset.CullBackfaces,
                    IsTriangleStrip = faceset.TriangleStrip,
                    IndexBuffer = new IndexBuffer(
                                GFX.Device,
                                is32bit ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits,
                                faceset.Indices.Count,
                                BufferUsage.WriteOnly),
                    IndexCount = faceset.Indices.Count,
                };

                if ((faceset.Flags & FLVER2.FaceSet.FSFlags.LodLevel1) > 0)
                {
                    newFaceSet.LOD = 1;
                    HasNoLODs = false;
                    newFaceSet.IsMotionBlur = false;
                }
                else if ((faceset.Flags & FLVER2.FaceSet.FSFlags.LodLevel2) > 0)
                {
                    newFaceSet.LOD = 2;
                    HasNoLODs = false;
                    newFaceSet.IsMotionBlur = false;
                }

                if ((faceset.Flags & FLVER2.FaceSet.FSFlags.MotionBlur) > 0)
                {
                    newFaceSet.IsMotionBlur = true;
                }

                if (is32bit)
                {
                    newFaceSet.IndexBuffer.SetData(faceset.Indices.Select(x => (x == 0xFFFF && x > mesh.Vertices.Count) ? -1 : x).ToArray());
                }
                else
                {
                    newFaceSet.IndexBuffer.SetData(faceset.Indices.Select(x => (x == 0xFFFF && x > mesh.Vertices.Count) ? -1 : (ushort)x).ToArray());
                }

                MeshFacesets.Add(newFaceSet);
            }

            Bounds = BoundingBox.CreateFromPoints(MeshVertices.Select(x => x.Position));

            VertBuffer = new VertexBuffer(GFX.Device,
                typeof(FlverShaderVertInput), MeshVertices.Length, BufferUsage.WriteOnly);
            VertBuffer.SetData(MeshVertices);

            //VertBufferBinding = new VertexBufferBinding(VertBuffer, 0, 0);

            TryToLoadTextures();
        }

        public List<string> GetAllTexNamesToLoad()
        {
            List<string> result = new List<string>();
            if (TexDataDiffuse == null && TexNameDiffuse != null)
                result.Add(Utils.GetShortIngameFileName(TexNameDiffuse));

            if (TexDataSpecular == null && TexNameSpecular != null)
                result.Add(Utils.GetShortIngameFileName(TexNameSpecular));

            if (TexDataNormal == null && TexNameNormal != null)
                result.Add(Utils.GetShortIngameFileName(TexNameNormal));

            if (TexDataDiffuse2 == null && TexNameDiffuse2 != null)
                result.Add(Utils.GetShortIngameFileName(TexNameDiffuse2));

            if (TexDataSpecular2 == null && TexNameSpecular2 != null)
                result.Add(Utils.GetShortIngameFileName(TexNameSpecular2));

            if (TexDataNormal2 == null && TexNameNormal2 != null)
                result.Add(Utils.GetShortIngameFileName(TexNameNormal2));

            if (TexDataEmissive == null && TexNameEmissive != null)
                result.Add(Utils.GetShortIngameFileName(TexNameEmissive));

            if (TexDataShininess == null && TexNameShininess != null)
                result.Add(Utils.GetShortIngameFileName(TexNameShininess));

            if (TexDataBlendmask == null && TexNameBlendmask != null)
                result.Add(Utils.GetShortIngameFileName(TexNameBlendmask));

            if (TexDataDOL1 == null && TexNameDOL1 != null)
                result.Add(Utils.GetShortIngameFileName(TexNameDOL1));

            if (TexDataDOL2 == null && TexNameDOL2 != null)
                result.Add(Utils.GetShortIngameFileName(TexNameDOL2));

            return result;
        }

        public void TryToLoadTextures()
        {
            if (TexDataDiffuse == null && TexNameDiffuse != null)
                TexDataDiffuse = TexturePool.FetchTexture2D(TexNameDiffuse);

            if (TexDataSpecular == null && TexNameSpecular != null)
                TexDataSpecular = TexturePool.FetchTexture2D(TexNameSpecular);

            if (TexDataNormal == null && TexNameNormal != null)
                TexDataNormal = TexturePool.FetchTexture2D(TexNameNormal);

            if (TexDataDiffuse2 == null && TexNameDiffuse2 != null)
                TexDataDiffuse2 = TexturePool.FetchTexture2D(TexNameDiffuse2);

            if (TexDataSpecular2 == null && TexNameSpecular2 != null)
                TexDataSpecular2 = TexturePool.FetchTexture2D(TexNameSpecular2);

            if (TexDataNormal2 == null && TexNameNormal2 != null)
                TexDataNormal2 = TexturePool.FetchTexture2D(TexNameNormal2);

            if (TexDataEmissive == null && TexNameEmissive != null)
                TexDataEmissive = TexturePool.FetchTexture2D(TexNameEmissive);

            if (TexDataShininess == null && TexNameShininess != null)
                TexDataShininess = TexturePool.FetchTexture2D(TexNameShininess);

            if (TexDataBlendmask == null && TexNameBlendmask != null)
                TexDataBlendmask = TexturePool.FetchTexture2D(TexNameBlendmask);

            if (TexDataDOL1 == null && TexNameDOL1 != null)
            {
                TexDataDOL1 = TexturePool.FetchTexture2D(TexNameDOL1);
                if (TexDataDOL1 == null)
                {
                    Console.WriteLine("Failed to load lightmap: " + TexNameDOL1);
                }
            }

            if (TexDataDOL2 == null && TexNameDOL2 != null)
                TexDataDOL2 = TexturePool.FetchTexture2D(TexNameDOL2);
        }

        public void Draw<T>(int lod, bool motionBlur, IGFXShader<T> shader, bool[] mask, bool forceNoBackfaceCulling = false, bool isSkyboxLol = false)
            where T : Effect
        {
            if (!IsVisible)
                return;

            if (mask != null && ModelMaskIndex >= 0 && !mask[ModelMaskIndex])
                return;

            var oldWorldMatrix = ((FlverShader)shader).World;

            //if (DefaultBoneIndex >= FlverShader.NUM_BONES * 3)
            //{
            //    ((FlverShader)shader).World = oldWorldMatrix * TaeInterop.ShaderMatrix3[DefaultBoneIndex % FlverShader.NUM_BONES] * TaeInterop.ShaderMatrix3[DefaultBoneIndex % FlverShader.NUM_BONES] * TaeInterop.ShaderMatrix3[DefaultBoneIndex % FlverShader.NUM_BONES] * TaeInterop.ShaderMatrix3[DefaultBoneIndex % FlverShader.NUM_BONES];
            //}
            //else if (DefaultBoneIndex >= FlverShader.NUM_BONES * 2)
            //{
            //    ((FlverShader)shader).World = oldWorldMatrix * TaeInterop.ShaderMatrix2[DefaultBoneIndex % FlverShader.NUM_BONES];
            //}
            //else if (DefaultBoneIndex >= FlverShader.NUM_BONES * 1)
            //{
            //    ((FlverShader)shader).World = oldWorldMatrix * TaeInterop.ShaderMatrix1[DefaultBoneIndex % FlverShader.NUM_BONES];
            //}
            //else if (DefaultBoneIndex >= FlverShader.NUM_BONES * 0)
            //{
            //    ((FlverShader)shader).World = oldWorldMatrix * TaeInterop.ShaderMatrix0[DefaultBoneIndex % FlverShader.NUM_BONES];
            //}

            if (GFX.EnableTextures && shader == GFX.FlverShader)
            {
                if (TexDataDiffuse == null)
                    TryToLoadTextures();

                GFX.FlverShader.Effect.ColorMap = TexDataDiffuse ?? Main.DEFAULT_TEXTURE_DIFFUSE;
                GFX.FlverShader.Effect.SpecularMap = TexDataSpecular ?? Main.DEFAULT_TEXTURE_SPECULAR;
                GFX.FlverShader.Effect.NormalMap = TexDataNormal ?? Main.DEFAULT_TEXTURE_NORMAL;

                GFX.FlverShader.Effect.EnableBlendMaskMap = TexDataBlendmask != null
                    && TexNameBlendmask != "SYSTEX_DummyBurn_m"; // aa

                if (TexDataDiffuse2 == null)
                {
                    GFX.FlverShader.Effect.EnableBlendTextures = false;

                    GFX.FlverShader.Effect.BlendmaskMap = Main.DEFAULT_TEXTURE_EMISSIVE;
                    GFX.FlverShader.Effect.ColorMap2 = Main.DEFAULT_TEXTURE_DIFFUSE;
                    GFX.FlverShader.Effect.SpecularMap2 = Main.DEFAULT_TEXTURE_SPECULAR;
                    GFX.FlverShader.Effect.NormalMap2 = Main.DEFAULT_TEXTURE_NORMAL;
                }
                else
                {
                    GFX.FlverShader.Effect.EnableBlendTextures = !GFX.FlverDisableTextureBlending;

                    GFX.FlverShader.Effect.ColorMap2 = TexDataDiffuse2 ?? Main.DEFAULT_TEXTURE_DIFFUSE;
                    GFX.FlverShader.Effect.SpecularMap2 = TexDataSpecular2 ?? Main.DEFAULT_TEXTURE_SPECULAR;
                    GFX.FlverShader.Effect.NormalMap2 = TexDataNormal2 ?? Main.DEFAULT_TEXTURE_NORMAL;

                    GFX.FlverShader.Effect.BlendmaskMap = TexDataBlendmask ?? Main.DEFAULT_TEXTURE_EMISSIVE;
                }
                

                // Hotfix because loading the DS3 character embered effect with the default shader just makes
                // them have various fixed glowing orange spots
                if (TexNameEmissive != "SYSTEX_DummyBurn_em")
                    GFX.FlverShader.Effect.EmissiveMap = TexDataEmissive ?? Main.DEFAULT_TEXTURE_EMISSIVE;
                else
                    GFX.FlverShader.Effect.EmissiveMap = Main.DEFAULT_TEXTURE_EMISSIVE;

                GFX.FlverShader.Effect.SpecularMapBB = TexDataShininess ?? Main.DEFAULT_TEXTURE_EMISSIVE;
                //GFX.FlverShader.Effect.LightMap2 = TexDataDOL2 ?? Main.DEFAULT_TEXTURE_DIFFUSE;

                GFX.FlverShader.Effect.WorkflowType = GFX.ForcedFlverShadingMode ?? ShadingMode;
            }

            // TEMPORARY
            //GFX.FlverShader.Effect.UseSpecularMapBB = false;// TaeInterop.CurrentHkxVariation == HKX.HKXVariation.HKXBloodBorne || TexDataShininess != null;

            //GFX.FlverShader.Effect.UseSpecularMapBB = true;

            

            //if (GFX.EnableLightmapping /*&& !GFX.EnableLighting*/)
            //{
            //    GFX.FlverShader.Effect.LightMap1 = TexDataDOL1 ?? Main.DEFAULT_TEXTURE_DIFFUSE;
            //}

            //foreach (var technique in shader.Effect.Techniques)
            //{
            //    shader.Effect.CurrentTechnique = technique;

            

            foreach (EffectPass pass in shader.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GFX.Device.SetVertexBuffer(VertBuffer);

                foreach (var faceSet in MeshFacesets)
                {
                    if (faceSet.IndexCount == 0)
                        continue;

                    if (!HasNoLODs && (lod != -1 && faceSet.LOD != lod) || (faceSet.IsMotionBlur != motionBlur))
                        continue;

                    GFX.Device.Indices = faceSet.IndexBuffer;

                    GFX.BackfaceCulling = forceNoBackfaceCulling ? false : faceSet.BackfaceCulling;

                    GFX.Device.DrawIndexedPrimitives(faceSet.IsTriangleStrip ? PrimitiveType.TriangleStrip : PrimitiveType.TriangleList, 0, 0,
                        faceSet.IsTriangleStrip ? (faceSet.IndexCount - 2) : (faceSet.IndexCount / 3));

                }
            }

            ((FlverShader)shader).World = oldWorldMatrix;

            //}
        }

        public void Dispose()
        {
            for (int i = 0; i < MeshFacesets.Count; i++)
            {
                MeshFacesets[i].IndexBuffer.Dispose();
            }

            MeshFacesets = null;

            VertBuffer.Dispose();

            // Just leave the texture data as-is, since 
            // TexturePool handles memory cleanup
        }
    }
}
