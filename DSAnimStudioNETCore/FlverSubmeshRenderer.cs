﻿using DSAnimStudio.GFXShaders;
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
    public class FlverSubmeshRenderer : IDisposable, IHighlightableThing
    {
        
        
        public bool ExceedsBoneCount = false;
        
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

        public int DebugViewWeightOfBoneIndex = -1;

        public int VertexCount { get; private set; }

        public NewMesh Parent;

        public bool IsVisible { get; set; } = true;

        public bool UsesRefPose { get; set; } = false;

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
                    NewMaterial.ModelMaskIndex = shit.MaskIndex;
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
        
        public int[] BoneIndices = null;
        public Matrix[] BoneMatrices = null;
        public Matrix[] BoneMatrices_RefPose = null;


        public int NewMaterialIndex = 0;
        public FlverMaterial NewMaterial => (NewMaterialIndex >= 0 && NewMaterialIndex < Parent.Materials.Count) ? Parent.Materials[NewMaterialIndex] : null;


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

        public FlverSubmeshRenderer(int submeshIndex, Model containingModel, NewMesh parent, FLVER2 flvr, FLVER2.Mesh mesh, 
            bool useSecondUV, Dictionary<string, int> boneIndexRemap = null,
            bool ignoreStaticTransforms = false)
        {
            bool bufferUsesBoneIndices = false;
            //bool bufferUsesBoneWeights = false;

            UsesRefPose = mesh.Dynamic == 0;

            foreach (var buffer in mesh.VertexBuffers)
            {
                var layout = flvr.BufferLayouts[buffer.LayoutIndex];
                foreach (var thing in layout)
                {
                    if (thing.Semantic == FLVER.LayoutSemantic.BoneIndices)
                    {
                        bufferUsesBoneIndices = true;
                        break;
                    }
                    //else if (thing.Semantic == FLVER.LayoutSemantic.BoneWeights)
                    //{
                    //    bufferUsesBoneWeights = true;
                    //}
                }
            }

            if (Model.DEBUG_FORCE_NO_GLOBAL_BONE_MATRIX)
            {
                containingModel.USE_GLOBAL_BONE_MATRIX = false;
            }
            else
            {
                containingModel.USE_GLOBAL_BONE_MATRIX = flvr.Nodes.Count < FlverShader.BoneMatrixSize;
            }


            Parent = parent;

            NewMaterialIndex = mesh.MaterialIndex;

            FullMaterialName = flvr.Materials[mesh.MaterialIndex].Name;

            DefaultBoneIndex = mesh.NodeIndex;
            
            BoneIndices = mesh.BoneIndices.ToArray();

            if (!containingModel.USE_GLOBAL_BONE_MATRIX)
            {
                BoneMatrices = new Matrix[BoneIndices.Length];
                BoneMatrices_RefPose = new Matrix[BoneIndices.Length];
                for (int i = 0; i < BoneIndices.Length; i++)
                {
                    BoneMatrices[i] = Matrix.Identity;
                    BoneMatrices_RefPose[i] = Matrix.Identity;
                }

                containingModel.SkeletonFlver.RegistPerMeshBoneIndices(submeshIndex, BoneIndices);
            }
            

            if (!containingModel.USE_GLOBAL_BONE_MATRIX)
                ExceedsBoneCount = BoneIndices.Length > FlverShader.BoneMatrixSize;
            else
                ExceedsBoneCount = false;

            //bool hasLightmap = false;

            Dictionary<int, int> finalBoneRemapper = null;

            if (boneIndexRemap != null)
            {
                finalBoneRemapper = new Dictionary<int, int>();
                for (int i = 0; i < flvr.Nodes.Count; i++)
                {
                    if (boneIndexRemap.ContainsKey(flvr.Nodes[i].Name))
                    {
                        finalBoneRemapper.Add(i, boneIndexRemap[flvr.Nodes[i].Name]);
                    }
                    else if (boneIndexRemap.ContainsKey(flvr.Nodes[0].Name))
                    {
                        finalBoneRemapper.Add(i, boneIndexRemap[flvr.Nodes[0].Name]);
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

            

            


            // TEMP DEBUG TEST DELETE ME

            //if (flvr.Materials[mesh.MaterialIndex].MTD.ToUpper().Contains("AX"))
            //{
            //    Console.WriteLine("WTF");
            //}

            //////////////////////////
            



            // MTD lookup
            //MTD mtd = null; //InterrootLoader.GetMTD(flvr.Materials[mesh.MaterialIndex].MTD);

            //var debug_LowestBoneWeight = float.MaxValue;
            //var debug_HighestBoneWeight = float.MinValue;

            //var debug_sortedByZ = new List<FLVER.Vertex>();

            Matrix GetBoneMatrix(SoulsFormats.FLVER.Node b)
            {
                SoulsFormats.FLVER.Node parentBone = b;

                var result = Matrix.Identity;

                do
                {
                    result *= Matrix.CreateScale(parentBone.Scale.X, parentBone.Scale.Y, parentBone.Scale.Z);
                    result *= Matrix.CreateRotationX(parentBone.Rotation.X);
                    result *= Matrix.CreateRotationZ(parentBone.Rotation.Z);
                    result *= Matrix.CreateRotationY(parentBone.Rotation.Y);
                    result *= Matrix.CreateTranslation(parentBone.Translation.X, parentBone.Translation.Y, parentBone.Translation.Z);

                    if (parentBone.ParentIndex >= 0)
                        parentBone = flvr.Nodes[parentBone.ParentIndex];
                    else
                        parentBone = null;
                }
                while (parentBone != null);

                return result;
            }

            var MeshVertices = new FlverShaderVertInput[mesh.Vertices.Count];
            
            var usedBoneIndices = mesh.BoneIndices.ToList();
            
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {

                var vert = mesh.Vertices[i];

                if (vert.Colors.Count == 2 && zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS)
                {
                    vert.Colors[0] = new FLVER.VertexColor(vert.Colors[0].A, vert.Colors[1].R, vert.Colors[1].R, vert.Colors[1].R);
                }

                var ORIG_BONE_WEIGHTS = vert.BoneWeights;
                var ORIG_BONE_INDICES = vert.BoneIndices;

                MeshVertices[i] = new FlverShaderVertInput();

                if (vert.BoneWeights[0] == 0 && vert.BoneWeights[1] == 0 && vert.BoneWeights[2] == 0 && vert.BoneWeights[3] == 0)
                {
                    vert.BoneWeights[0] = 1;
                }

                // Apply normal W channel bone index (for some weapons etc)
                if (!bufferUsesBoneIndices)
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
                if (mesh.Dynamic == 0)
                {
                    //ApplySkin(vert, flvr.Bones.Select(b => GetBoneMatrix(b)).ToList(), mesh.BoneIndices, (flvr.Header.Version <= 0x2000D));

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

                bool fileHasPerMeshBoneIndices = flvr.Header.Version <= 0x2000D;
                if (fileHasPerMeshBoneIndices)
                {
                    for (int v = 0; v < 4; v++)
                    {
                        int perMeshIndex = vert.BoneIndices[v];
                        if (perMeshIndex < 0 || perMeshIndex >= mesh.BoneIndices.Count)
                        {
                            vert.BoneIndices[v] = 0;
                            vert.BoneWeights[v] = 0;
                            continue;
                        }
                        
                        int globalBoneIndex = mesh.BoneIndices[perMeshIndex];
                        if (globalBoneIndex < 0 || globalBoneIndex >= flvr.Nodes.Count)
                        {
                            vert.BoneIndices[v] = 0;
                            vert.BoneWeights[v] = 0;
                            continue;
                        }

                        vert.BoneIndices[v] = containingModel.USE_GLOBAL_BONE_MATRIX ? globalBoneIndex : perMeshIndex;
                    }
                }
                else
                {
                    for (int v = 0; v < 4; v++)
                    {
                        int globalBoneIndex = vert.BoneIndices[v];

                        if (globalBoneIndex < 0 || globalBoneIndex >= flvr.Nodes.Count)
                        {
                            globalBoneIndex = 0;
                            vert.BoneWeights[v] = 0;
                        }

                        if (containingModel.USE_GLOBAL_BONE_MATRIX)
                        {
                            vert.BoneIndices[v] = globalBoneIndex;
                        }
                        else
                        {
                            if (!usedBoneIndices.Contains(globalBoneIndex))
                                usedBoneIndices.Add(globalBoneIndex);
                            vert.BoneIndices[v] = usedBoneIndices.IndexOf(globalBoneIndex);
                        }
                        
                    }
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

                MeshVertices[i].BoneIndices = new Vector4(vert.BoneIndices[0], vert.BoneIndices[1], vert.BoneIndices[2], vert.BoneIndices[3]);
                
                

                //if (vert.BoneIndices[0] < 0)
                //    MeshVertices[i].BoneWeights.X = 0;

                //if (vert.BoneIndices[1] < 0)
                //    MeshVertices[i].BoneWeights.Y = 0;

                //if (vert.BoneIndices[2] < 0)
                //    MeshVertices[i].BoneWeights.Z = 0;

                //if (vert.BoneIndices[3] < 0)
                //    MeshVertices[i].BoneWeights.W = 0;

                vert.BoneWeights = ORIG_BONE_WEIGHTS;
                vert.BoneIndices = ORIG_BONE_INDICES;

                MeshVertices[i].Position = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);

                // DEBUG TEST
                //if (flvr.Header.Version > 0x2000D)
                //{
                //    MeshVertices[i].Position += new Vector3(0, 0.38f, 0);
                //}

                MeshVertices[i].Normal = Vector3.Normalize(new Vector3(vert.Normal.X, vert.Normal.Y, vert.Normal.Z));

                if (vert.Colors.Count >= 1)
                    MeshVertices[i].Color = new Vector4(vert.Colors[0].R, vert.Colors[0].G, vert.Colors[0].B, vert.Colors[0].A);

                MeshVertices[i].Bitangent = Vector4.Zero;

                if (vert.Tangents.Count > 1)
                {
                    MeshVertices[i].Bitangent = vert.Tangents[0].ToXna();
                    MeshVertices[i].Bitangent2 = vert.Tangents[1].ToXna();
                }
                else if (vert.Tangents.Count > 0)
                {
                    MeshVertices[i].Bitangent = vert.Tangents[0].ToXna();
                    MeshVertices[i].Bitangent2 = vert.Tangents[0].ToXna();
                }
                else
                {
                    Vector3 fakeNormal = new Vector3(1, 1, 0);
                    Vector3 fakeTangent;
                    Vector3 t1 = Vector3.Cross(fakeNormal, Vector3.Forward);
                    Vector3 t2 = Vector3.Cross(fakeNormal, Vector3.Up);
                    if (t1.LengthSquared() > t2.LengthSquared())
                    {
                        fakeTangent = t1;
                    }
                    else
                    {
                        fakeTangent = t2;
                    }

                    MeshVertices[i].Bitangent = new Vector4(fakeTangent.X, fakeTangent.Y, fakeTangent.Z, 1);
                    MeshVertices[i].Bitangent2 = new Vector4(fakeTangent.X, fakeTangent.Y, fakeTangent.Z, 1);
                }

                MeshVertices[i].Binormal = Vector3.Cross(Vector3.Normalize(MeshVertices[i].Normal), 
                    Vector3.Normalize(new Vector3(MeshVertices[i].Bitangent.X, MeshVertices[i].Bitangent.Y, 
                    MeshVertices[i].Bitangent.Z))) * MeshVertices[i].Bitangent.W;

                MeshVertices[i].Binormal2 = Vector3.Cross(Vector3.Normalize(MeshVertices[i].Normal),
                    Vector3.Normalize(new Vector3(MeshVertices[i].Bitangent2.X, MeshVertices[i].Bitangent2.Y,
                    MeshVertices[i].Bitangent2.Z))) * MeshVertices[i].Bitangent2.W;

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
                    for (int u = 0; u < vert.UVs.Count; u++)
                    {
                        if (u == 0)
                            MeshVertices[i].TextureCoordinate = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                        else if (u == 1)
                            MeshVertices[i].TextureCoordinate2 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                        else if (u == 2)
                            MeshVertices[i].TextureCoordinate3 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                        else if (u == 3)
                            MeshVertices[i].TextureCoordinate4 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                        else if (u == 4)
                            MeshVertices[i].TextureCoordinate5 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                        else if (u == 5)
                            MeshVertices[i].TextureCoordinate6 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                        else if (u == 6)
                            MeshVertices[i].TextureCoordinate7 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                        else if (u == 7)
                            MeshVertices[i].TextureCoordinate8 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                    }

                    //if (useSecondUV && vert.UVs.Count > 1)
                    //    MeshVertices[i].TextureCoordinate = new Vector2(vert.UVs[1].X, vert.UVs[1].Y);
                    //else
                    //    MeshVertices[i].TextureCoordinate = new Vector2(vert.UVs[0].X, vert.UVs[0].Y);

                    //if (vert.UVs.Count >= 2)
                    //{
                    //    MeshVertices[i].TextureCoordinate2 = new Vector2(vert.UVs[1].X, vert.UVs[1].Y);
                    //}
                    //else
                    //{
                    //    MeshVertices[i].TextureCoordinate2 = MeshVertices[i].TextureCoordinate;
                    //}

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
                //else
                //{
                //    MeshVertices[i].TextureCoordinate = Vector2.Zero;
                //    MeshVertices[i].TextureCoordinate2 = Vector2.Zero;
                //    MeshVertices[i].TextureCoordinate3 = Vector2.Zero;
                //    MeshVertices[i].TextureCoordinate4 = Vector2.Zero;
                //    MeshVertices[i].TextureCoordinate5 = Vector2.Zero;
                //    MeshVertices[i].TextureCoordinate6 = Vector2.Zero;
                //    MeshVertices[i].TextureCoordinate7 = Vector2.Zero;
                //    MeshVertices[i].TextureCoordinate8 = Vector2.Zero;
                //}
            }
            
            BoneIndices = usedBoneIndices.ToArray();

            if (!containingModel.USE_GLOBAL_BONE_MATRIX)
            {
                BoneMatrices = new Matrix[BoneIndices.Length];
                BoneMatrices_RefPose = new Matrix[BoneIndices.Length];
                for (int i = 0; i < BoneIndices.Length; i++)
                {
                    BoneMatrices[i] = Matrix.Identity;
                    BoneMatrices_RefPose[i] = Matrix.Identity;
                }

                containingModel.SkeletonFlver.RegistPerMeshBoneIndices(submeshIndex, BoneIndices);
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
        }

        #region FLVER0
        public FlverSubmeshRenderer(int submeshIndex, Model containingModel, NewMesh parent, FLVER0 flvr, FLVER0.Mesh mesh,
            bool useSecondUV, Dictionary<string, int> boneIndexRemap = null,
            bool ignoreStaticTransforms = false)
        {
            bool bufferUsesBoneIndices = false;
            //bool bufferUsesBoneWeights = false;

            UsesRefPose = mesh.Dynamic == 0;

            if (mesh.MaterialIndex >= 0 && mesh.LayoutIndex >= 0)
            {
                if (flvr.Materials[mesh.MaterialIndex].Layouts != null)
                {
                    var layout = flvr.Materials[mesh.MaterialIndex].Layouts[mesh.LayoutIndex];
                    foreach (var thing in layout)
                    {
                        if (thing.Semantic == FLVER.LayoutSemantic.BoneIndices)
                        {
                            bufferUsesBoneIndices = true;
                            break;
                        }
                    }
                }
            }

            if (Model.DEBUG_FORCE_NO_GLOBAL_BONE_MATRIX)
            {
                containingModel.USE_GLOBAL_BONE_MATRIX = false;
            }
            else
            {
                containingModel.USE_GLOBAL_BONE_MATRIX = flvr.Nodes.Count < FlverShader.BoneMatrixSize;
            }

            
            NewMaterialIndex = mesh.MaterialIndex;

            Parent = parent;

            FullMaterialName = flvr.Materials[mesh.MaterialIndex].Name;

            DefaultBoneIndex = mesh.NodeIndex;
            
            BoneIndices = mesh.BoneIndices.Select(i => (int)i).ToArray();

            if (!containingModel.USE_GLOBAL_BONE_MATRIX)
            {
                BoneMatrices = new Matrix[BoneIndices.Length];
                BoneMatrices_RefPose = new Matrix[BoneIndices.Length];
                for (int i = 0; i < BoneIndices.Length; i++)
                {
                    BoneMatrices[i] = Matrix.Identity;
                    BoneMatrices_RefPose[i] = Matrix.Identity;
                }

                containingModel.SkeletonFlver.RegistPerMeshBoneIndices(submeshIndex, BoneIndices);
            }

            Dictionary<int, int> finalBoneRemapper = null;

            if (boneIndexRemap != null)
            {
                finalBoneRemapper = new Dictionary<int, int>();
                for (int i = 0; i < flvr.Nodes.Count; i++)
                {
                    if (boneIndexRemap.ContainsKey(flvr.Nodes[i].Name))
                    {
                        finalBoneRemapper.Add(i, boneIndexRemap[flvr.Nodes[i].Name]);
                    }
                    else if (boneIndexRemap.ContainsKey(flvr.Nodes[0].Name))
                    {
                        finalBoneRemapper.Add(i, boneIndexRemap[flvr.Nodes[0].Name]);
                    }
                }
            }

            // MTD lookup
            //MTD mtd = null; //InterrootLoader.GetMTD(flvr.Materials[mesh.MaterialIndex].MTD);

            //var debug_LowestBoneWeight = float.MaxValue;
            //var debug_HighestBoneWeight = float.MinValue;

            //var debug_sortedByZ = new List<FLVER.Vertex>();

            Matrix GetBoneMatrix(SoulsFormats.FLVER.Node b)
            {
                SoulsFormats.FLVER.Node parentBone = b;

                var result = Matrix.Identity;

                do
                {
                    result *= Matrix.CreateScale(parentBone.Scale.X, parentBone.Scale.Y, parentBone.Scale.Z);
                    result *= Matrix.CreateRotationX(parentBone.Rotation.X);
                    result *= Matrix.CreateRotationZ(parentBone.Rotation.Z);
                    result *= Matrix.CreateRotationY(parentBone.Rotation.Y);
                    result *= Matrix.CreateTranslation(parentBone.Translation.X, parentBone.Translation.Y, parentBone.Translation.Z);

                    if (parentBone.ParentIndex >= 0)
                        parentBone = flvr.Nodes[parentBone.ParentIndex];
                    else
                        parentBone = null;
                }
                while (parentBone != null);

                return result;
            }
            if (mesh.Vertices != null)
            {
                var MeshVertices = new FlverShaderVertInput[mesh.Vertices.Count];
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {

                    var vert = mesh.Vertices[i];

                    if (vert.Colors.Count == 2 && zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS)
                    {
                        vert.Colors[0] = new FLVER.VertexColor(vert.Colors[0].A, vert.Colors[1].R, vert.Colors[1].R, vert.Colors[1].R);
                    }

                    var ORIG_BONE_WEIGHTS = vert.BoneWeights;
                    var ORIG_BONE_INDICES = vert.BoneIndices;

                    MeshVertices[i] = new FlverShaderVertInput();

                    if (vert.BoneWeights[0] == 0 && vert.BoneWeights[1] == 0 && vert.BoneWeights[2] == 0 && vert.BoneWeights[3] == 0)
                    {
                        vert.BoneWeights[0] = 1;
                    }

                    // Apply normal W channel bone index (for some weapons etc)
                    if (!bufferUsesBoneIndices)
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
                    if (mesh.Dynamic == 0)
                    {
                        //ApplySkin(vert, flvr.Bones.Select(b => GetBoneMatrix(b)).ToList(), mesh.BoneIndices, (flvr.Header.Version <= 0x2000D));

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

                    
                    
                    // File always has per-mesh bone indices in FLVER0
                    for (int v = 0; v < 4; v++)
                    {
                        int perMeshIndex = vert.BoneIndices[v];
                        if (perMeshIndex < 0 || perMeshIndex >= mesh.BoneIndices.Length)
                        {
                            vert.BoneIndices[v] = 0;
                            vert.BoneWeights[v] = 0;
                            continue;
                        }
                        
                        int globalBoneIndex = mesh.BoneIndices[perMeshIndex];
                        if (globalBoneIndex < 0 || globalBoneIndex >= flvr.Nodes.Count)
                        {
                            vert.BoneIndices[v] = 0;
                            vert.BoneWeights[v] = 0;
                            continue;
                        }

                        vert.BoneIndices[v] = containingModel.USE_GLOBAL_BONE_MATRIX ? globalBoneIndex : perMeshIndex;
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

                    MeshVertices[i].BoneIndices = new Vector4(vert.BoneIndices[0], vert.BoneIndices[1], vert.BoneIndices[2], vert.BoneIndices[3]);
                    
                    vert.BoneWeights = ORIG_BONE_WEIGHTS;
                    vert.BoneIndices = ORIG_BONE_INDICES;

                    MeshVertices[i].Position = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);

                    MeshVertices[i].Normal = Vector3.Normalize(new Vector3(vert.Normal.X, vert.Normal.Y, vert.Normal.Z));

                    if (vert.Colors.Count >= 1)
                        MeshVertices[i].Color = new Vector4(vert.Colors[0].R, vert.Colors[0].G, vert.Colors[0].B, vert.Colors[0].A);

                    MeshVertices[i].Bitangent = Vector4.Zero;

                    if (vert.Tangents.Count > 1)
                    {
                        MeshVertices[i].Bitangent = vert.Tangents[0].ToXna();
                        MeshVertices[i].Bitangent2 = vert.Tangents[1].ToXna();
                    }
                    else if (vert.Tangents.Count > 0)
                    {
                        MeshVertices[i].Bitangent = vert.Tangents[0].ToXna();
                        MeshVertices[i].Bitangent2 = vert.Tangents[0].ToXna();
                    }
                    else
                    {
                        Vector3 fakeNormal = new Vector3(1, 1, 0);
                        Vector3 fakeTangent;
                        Vector3 t1 = Vector3.Cross(fakeNormal, Vector3.Forward);
                        Vector3 t2 = Vector3.Cross(fakeNormal, Vector3.Up);
                        if (t1.LengthSquared() > t2.LengthSquared())
                        {
                            fakeTangent = t1;
                        }
                        else
                        {
                            fakeTangent = t2;
                        }

                        MeshVertices[i].Bitangent = new Vector4(fakeTangent.X, fakeTangent.Y, fakeTangent.Z, 1);
                        MeshVertices[i].Bitangent2 = new Vector4(fakeTangent.X, fakeTangent.Y, fakeTangent.Z, 1);
                    }

                    MeshVertices[i].Binormal = Vector3.Cross(Vector3.Normalize(MeshVertices[i].Normal),
                        Vector3.Normalize(new Vector3(MeshVertices[i].Bitangent.X, MeshVertices[i].Bitangent.Y,
                        MeshVertices[i].Bitangent.Z))) * MeshVertices[i].Bitangent.W;

                    MeshVertices[i].Binormal2 = Vector3.Cross(Vector3.Normalize(MeshVertices[i].Normal),
                        Vector3.Normalize(new Vector3(MeshVertices[i].Bitangent2.X, MeshVertices[i].Bitangent2.Y,
                        MeshVertices[i].Bitangent2.Z))) * MeshVertices[i].Bitangent2.W;

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
                        for (int u = 0; u < vert.UVs.Count; u++)
                        {
                            if (u == 0)
                                MeshVertices[i].TextureCoordinate = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                            else if (u == 1)
                                MeshVertices[i].TextureCoordinate2 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                            else if (u == 2)
                                MeshVertices[i].TextureCoordinate3 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                            else if (u == 3)
                                MeshVertices[i].TextureCoordinate4 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                            else if (u == 4)
                                MeshVertices[i].TextureCoordinate5 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                            else if (u == 5)
                                MeshVertices[i].TextureCoordinate6 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                            else if (u == 6)
                                MeshVertices[i].TextureCoordinate7 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
                            else if (u == 7)
                                MeshVertices[i].TextureCoordinate8 = new Vector2(vert.UVs[u].X, vert.UVs[u].Y);
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

                bool is32bit = flvr.Header.VertexIndexSize == 32;
                var newFaceSet = new FlverSubmeshRendererFaceSet()
                {
                    BackfaceCulling = false, //TODO: CHECK IF THIS IS AN UNK OR SOMETHING
                    IsTriangleStrip = true, //TODO: CHECK THIS
                    IndexBuffer = new IndexBuffer(
                                    GFX.Device,
                                    is32bit ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits,
                                    mesh.Indices.Count,
                                    BufferUsage.WriteOnly),
                    IndexCount = mesh.Indices.Count
                };

                if (is32bit)
                {
                    newFaceSet.IndexBuffer.SetData(mesh.Indices.Select(x => (x == 0xFFFF && x > mesh.Vertices.Count) ? -1 : x).ToArray());
                }
                else
                {
                    newFaceSet.IndexBuffer.SetData(mesh.Indices.Select(x => (x == 0xFFFF || x > mesh.Vertices.Count) ? (ushort)0xFFFF : (ushort)x).ToArray());
                }

                MeshFacesets.Add(newFaceSet);

                Bounds = BoundingBox.CreateFromPoints(MeshVertices.Select(x => x.Position));

                VertBuffer = new VertexBuffer(GFX.Device,
                    typeof(FlverShaderVertInput), MeshVertices.Length, BufferUsage.WriteOnly);
                VertBuffer.SetData(MeshVertices);
            }
            else
            {

                VertBuffer = null;
                MeshFacesets = new List<FlverSubmeshRendererFaceSet>();
            }
            //VertBufferBinding = new VertexBufferBinding(VertBuffer, 0, 0);

            NewMaterial.TryToLoadTextures();
        }
        #endregion

        public void Draw(int lod, bool motionBlur, bool[] mask, bool forceNoBackfaceCulling, 
            NewAnimSkeleton_FLVER skeleton, Action<Exception> onDrawFail, Model model, Model basePlayerModel)
        {
            if (VertBuffer == null)
                return;

            // TODO add material-specific memes here.
            bool enableWireframeOverlay = GFX.FlverWireframeOverlay_Enabled;
            bool wireframeOverlayObeysAlpha = GFX.FlverWireframeOverlay_ObeysTextureAlphas;
            Vector4 wireframeOverlayColor = GFX.FlverWireframeOverlay_Color;



            bool isForceDrawForDebugHover = (GFX.HighlightedThing == this || GFX.HighlightedThing == Parent || GFX.HighlightedThing == NewMaterial || GFX.HighlightedThing == model);

            if (!isForceDrawForDebugHover && (!IsVisible || !NewMaterial.IsVisible))
                return;

            if (!isForceDrawForDebugHover && (mask != null && NewMaterial.ModelMaskIndex >= 0 && !mask[NewMaterial.ModelMaskIndex]))
                return;

            //if (NewMaterial.DrawStep != GFX.CurrentStep)
            //    return;

            var flverShader = GFX.FlverShader.Effect;
            
            // if (model.Name.ToUpper().StartsWith("WP_"))
            //     Console.WriteLine("test");
            
            if (skeleton != null)
            {
                if (model.USE_GLOBAL_BONE_MATRIX)
                {
                    if (UsesRefPose)
                        flverShader.BonesNew = skeleton.ShaderMatricesNew_RefPose ?? new Matrix[0];
                    else
                        flverShader.BonesNew = skeleton.ShaderMatricesNew ?? new Matrix[0];
                }
                else
                {
                    if (UsesRefPose)
                        flverShader.BonesNew = BoneMatrices_RefPose ?? new Matrix[0];
                    else
                        flverShader.BonesNew = BoneMatrices ?? new Matrix[0];
                }
                
                
            }
            else
            {
                GFX.FlverShader.Effect.BonesNew = FlverShader.IdentityBoneMatrix;
            }

            flverShader.DebugViewWeightOfBone_Index = DebugViewWeightOfBoneIndex;

            

            var oldWorldMatrix = GFX.FlverShader.Effect.World;

            bool initMatAndCheckIfRender = false;
            try
            {
                initMatAndCheckIfRender = NewMaterial.ApplyToFlverShader(Parent, this, model, basePlayerModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to initialize Flver material.");
            }
            if ((!initMatAndCheckIfRender && !isForceDrawForDebugHover) && DebugViewWeightOfBoneIndex == -1)
                return;

            if (DebugViewWeightOfBoneIndex != -1)
            {
                flverShader.DebugViewWeightOfBone_EnableLighting = GFX.FlverShader_DebugViewWeightOfBone_EnableLighting;
                flverShader.DebugViewWeightOfBone_ClipUnweightedGeometry = GFX.FlverShader_DebugViewWeightOfBone_ClipUnweightedGeometry;
                flverShader.DebugViewWeightOfBone_LightingPower = GFX.FlverShader_DebugViewWeightOfBone_LightingPower;
                flverShader.DebugViewWeightOfBone_LightingMult = GFX.FlverShader_DebugViewWeightOfBone_LightingMult;
                flverShader.DebugViewWeightOfBone_LightingGain = GFX.FlverShader_DebugViewWeightOfBone_LightingGain;

                flverShader.DebugViewWeightOfBone_Lighting_AlbedoMult = GFX.FlverShader_DebugViewWeightOfBone_Lighting_AlbedoMult;
                flverShader.DebugViewWeightOfBone_Lighting_ReflectanceMult = GFX.FlverShader_DebugViewWeightOfBone_Lighting_ReflectanceMult;
                flverShader.DebugViewWeightOfBone_Lighting_Gloss = GFX.FlverShader_DebugViewWeightOfBone_Lighting_Gloss;

                flverShader.DebugViewWeightOfBone_BaseColor = GFX.FlverShader_DebugViewWeightOfBone_BaseColor;
                flverShader.DebugViewWeightOfBone_WeightColor = GFX.FlverShader_DebugViewWeightOfBone_WeightColor;
                flverShader.DebugViewWeightOfBone_WireframeWeightColor = GFX.FlverShader_DebugViewWeightOfBone_WireframeWeightColor;

                flverShader.DisableAlpha = !GFX.FlverShader_DebugViewWeightOfBone_EnableTextureAlphas;

                enableWireframeOverlay = GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_Enabled;
                wireframeOverlayObeysAlpha = GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_ObeysTextureAlphas;
                wireframeOverlayColor = GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_Color;
                flverShader.DebugViewWeightOfBone_WireframeWeightColor = GFX.FlverShader_DebugViewWeightOfBone_WireframeWeightColor;
            }

            try
            {
                GFX.Wireframe = false;
                flverShader.WireframeColorOverride_Enabled = false;
                if (MeshFacesets != null)
                {
                    
                    foreach (EffectPass pass in GFX.FlverShader.Effect.CurrentTechnique.Passes)
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

                            

                            try
                            {
                                GFX.Device.DrawIndexedPrimitives(faceSet.IsTriangleStrip ? PrimitiveType.TriangleStrip : PrimitiveType.TriangleList, 0, 0,
                                    faceSet.IsTriangleStrip ? (faceSet.IndexCount - 2) : (faceSet.IndexCount / 3));
                            }
                            catch (Exception ex)
                            {
                                onDrawFail?.Invoke(ex);
                            }
                        }
                    }
                }

                if (enableWireframeOverlay)
                {
                    flverShader.WireframeColorOverride_Enabled = true;
                    flverShader.WireframeColorOverride_Color = wireframeOverlayColor;
                    if (!wireframeOverlayObeysAlpha)
                        flverShader.DisableAlpha = true;

                    GFX.Wireframe = true;
                    if (MeshFacesets != null)
                    {
                        foreach (EffectPass pass in GFX.FlverShader.Effect.CurrentTechnique.Passes)
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



                                try
                                {
                                    GFX.Device.DrawIndexedPrimitives(faceSet.IsTriangleStrip ? PrimitiveType.TriangleStrip : PrimitiveType.TriangleList, 0, 0,
                                        faceSet.IsTriangleStrip ? (faceSet.IndexCount - 2) : (faceSet.IndexCount / 3));
                                }
                                catch (Exception ex)
                                {
                                    onDrawFail?.Invoke(ex);
                                }
                            }
                        }
                    }
                    
                }

            }
            finally
            {
                GFX.FlverShader.Effect.World = oldWorldMatrix;
            }



            

            //}
        }

        public void Dispose()
        {
            if (MeshFacesets != null)
            {
                for (int i = 0; i < MeshFacesets.Count; i++)
                {
                    MeshFacesets[i].IndexBuffer.Dispose();
                }
                MeshFacesets?.Clear();
                MeshFacesets = null;
            }

            
            VertBuffer?.Dispose();
            VertBuffer = null;

            this.BoneMatrices = null;
            this.BoneMatrices_RefPose = null;
            this.Parent = null;

            // Just leave the texture data as-is, since 
            // TexturePool handles memory cleanup
        }
    }
}
