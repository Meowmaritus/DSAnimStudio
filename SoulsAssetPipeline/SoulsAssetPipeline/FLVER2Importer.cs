using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assimp;
using SoulsFormats;

namespace SoulsAssetPipeline
{
    public static class FLVER2Importer
    {
        public class ImportedFLVER2Model
        {
            public FLVER2 Flver;
            public Dictionary<string, byte[]> Textures = new Dictionary<string, byte[]>();

            //TODO: FLESH THIS OUT
        }

        public class FLVER2ImportSettings
        {
            public float SceneScale = 1.0f;
            public bool ConvertFromZUp = true;

            public int FlverVersion = 0x2000D;
        }

        public static ImportedFLVER2Model ImportFBX(string fbxPath, FLVER2ImportSettings settings)
        {
            using (var context = new AssimpContext())
            {
                var fbx = context.ImportFile(fbxPath, PostProcessSteps.CalculateTangentSpace);
                return ImportFromAssimpScene(fbx, settings);
            }
        }

        public static ImportedFLVER2Model ImportFromAssimpScene(Scene scene, FLVER2ImportSettings settings)
        {
            var result = new ImportedFLVER2Model();
            var flver = result.Flver = new FLVER2();

            //TODO: flver header version should be initialized BEFORE it attempts
            //      to read geometry as the version will determine things like
            //      whether to use relative or absolute bone indices etc.

            //Temporary
            flver.Header.Version = settings.FlverVersion;

            var flverSceneMatrix = System.Numerics.Matrix4x4.CreateScale(System.Numerics.Vector3.One * settings.SceneScale);

            

            if (settings.ConvertFromZUp)
            {
                flverSceneMatrix *= System.Numerics.Matrix4x4.CreateRotationZ((float)(Math.PI));
                flverSceneMatrix *= System.Numerics.Matrix4x4.CreateRotationX((float)(-Math.PI / 2.0));

            }

            flverSceneMatrix *= System.Numerics.Matrix4x4.CreateScale(1, 1, -1);


            var skeletonRootNode = AssimpUtilities.FindRootNode(scene, "root", out Matrix4x4 skeletonRootNodeMatrix);
            var metaskeleton = FLVERImportHelpers.GenerateFlverMetaskeletonFromRootNode(
                skeletonRootNode, skeletonRootNodeMatrix, flverSceneMatrix);

            flver.Bones = metaskeleton.Bones;
            flver.Dummies = metaskeleton.DummyPoly;

            foreach (var material in scene.Materials)
            {
                string[] materialNameSplit = material.Name.Split('|');
                //ErrorTODO: materialNameSplit should be 2 items long.
                var flverMaterial = new FLVER2.Material(materialNameSplit[0], materialNameSplit[1], 0);

                //TODO: Implement GX stuff somehow...? Gah

                //TODO: IMPLEMENT ACTUAL TEXTURE STUFF FROM XML DEFS
                flverMaterial.Textures.Add(new FLVER2.Texture(type: "g_Diffuse",
                    path: material.TextureDiffuse.FilePath,
                    scale: System.Numerics.Vector2.One,
                    0, false, 0, 0, 0));
                flverMaterial.Textures.Add(new FLVER2.Texture(type: "g_Specular",
                    path: material.TextureSpecular.FilePath,
                    scale: System.Numerics.Vector2.One,
                    0, false, 0, 0, 0));
                flverMaterial.Textures.Add(new FLVER2.Texture(type: "g_Bumpmap",
                    path: material.TextureNormal.FilePath,
                    scale: System.Numerics.Vector2.One,
                    0, false, 0, 0, 0));

                

                result.Textures.Add(
                    System.IO.Path.GetFileNameWithoutExtension(material.TextureDiffuse.FilePath),
                    scene.GetEmbeddedTexture(material.TextureDiffuse.FilePath).CompressedData);
                result.Textures.Add(
                    System.IO.Path.GetFileNameWithoutExtension(material.TextureSpecular.FilePath),
                    scene.GetEmbeddedTexture(material.TextureSpecular.FilePath).CompressedData);
                result.Textures.Add(
                    System.IO.Path.GetFileNameWithoutExtension(material.TextureNormal.FilePath),
                    scene.GetEmbeddedTexture(material.TextureNormal.FilePath).CompressedData);


                flver.Materials.Add(flverMaterial);
            }

            foreach (var mesh in scene.Meshes)
            {
                var flverMesh = new FLVER2.Mesh();

                //TODO: ACTUALLY READ FROM THINGS
                flverMesh.Dynamic = 1;

                int meshUVCount = 0;
                for (int i = 0; i < mesh.UVComponentCount.Length; i++)
                {
                    if (mesh.UVComponentCount[i] > 0)
                        meshUVCount++;
                }

                //TODO: Load buffer layout from FLVER2XmlVertLayoutManager
                var flverBufferLayout = new FLVER2.BufferLayout();

                //Temporary hack to make DSAnimStudio realize there are bone indices in the mesh
                flverBufferLayout.Add(new FLVER.LayoutMember(FLVER.LayoutType.Byte4A, FLVER.LayoutSemantic.BoneIndices, 0, 0));

                var flverFaceSet = new FLVER2.FaceSet();
                var flverVertBuffer = new FLVER2.VertexBuffer(layoutIndex: flver.Meshes.Count);

                flverMesh.MaterialIndex = mesh.MaterialIndex;

                flverMesh.Vertices = new List<FLVER.Vertex>(mesh.VertexCount);

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    var newVert = new FLVER.Vertex(uvCapacity: meshUVCount,
                        //TODO: Figure out what multiple tangents are used for etc and implement all
                        //      of that into the XML vert layout system stuff etc etc.
                        tangentCapacity: mesh.HasTangentBasis ? 1 : 0, 
                        colorCapacity: mesh.VertexColorChannelCount);

                    newVert.Position = System.Numerics.Vector3.Transform(mesh.Vertices[i].ToNumerics(), flverSceneMatrix);
                    newVert.Normal = System.Numerics.Vector3.TransformNormal(mesh.Normals[i].ToNumerics(), flverSceneMatrix);

                    if (mesh.HasTangentBasis)
                    {
                        //ErrorTODO: Throw error if mesh somehow has tangents but not normals.
                        var tan = mesh.Tangents[i];
                        var bitanXYZ = mesh.BiTangents[i];
                        //TODO: Check Bitangent W calculation
                        var bitanW = Vector3D.Dot(Vector3D.Cross(tan, mesh.Normals[i]), bitanXYZ) >= 0 ? 1 : -1;
                        var bitanXYZTransformed = System.Numerics.Vector3.TransformNormal(bitanXYZ.ToNumerics(), flverSceneMatrix);
                        newVert.Tangents.Add(new System.Numerics.Vector4(bitanXYZTransformed, bitanW));
                    }
                    
                    for (int j = 0; j < meshUVCount; j++)
                    {
                        var uv = mesh.TextureCoordinateChannels[j][i];
                        newVert.UVs.Add(new System.Numerics.Vector3(uv.X, 1 - uv.Y, uv.Z));
                    }

                    for (int j = 0; j < mesh.VertexColorChannelCount; j++)
                    {
                        newVert.Colors.Add(mesh.VertexColorChannels[j][i].ToFlverVertexColor());
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        newVert.BoneIndices[j] = -1;
                    }

                    flverMesh.Vertices.Add(newVert);
                }

                foreach (var bone in mesh.Bones)
                {
                    var boneIndex = flver.Bones.FindIndex(b => b.Name == bone.Name);

                    // Old versions used a list of relative bone indices.
                    if (flver.Header.Version <= 0x2000D)
                    {
                        if (!flverMesh.BoneIndices.Contains(boneIndex))
                            flverMesh.BoneIndices.Add(boneIndex);
                    }
                    
                    int GetNextAvailableBoneSlotOfVert(int vertIndex)
                    {
                        if (flverMesh.Vertices[vertIndex].BoneIndices[0] < 0)
                            return 0;
                        else if (flverMesh.Vertices[vertIndex].BoneIndices[1] < 0)
                            return 1;
                        else if (flverMesh.Vertices[vertIndex].BoneIndices[2] < 0)
                            return 2;
                        else if (flverMesh.Vertices[vertIndex].BoneIndices[3] < 0)
                            return 3;
                        else
                            return -1;
                    }

                    foreach (var weight in bone.VertexWeights)
                    {
                        int boneSlot = GetNextAvailableBoneSlotOfVert(weight.VertexID);
                        if (boneSlot >= 0)
                        {
                            flverMesh.Vertices[weight.VertexID].BoneIndices[boneSlot] =
                                flver.Header.Version <= 0x2000D ? flverMesh.BoneIndices.IndexOf(boneIndex) : boneIndex;
                            flverMesh.Vertices[weight.VertexID].BoneWeights[boneSlot] = weight.Weight;
                        }
                        else
                        {
                            Console.WriteLine("fatcat");
                        }
                    }
                }

                for (int i = 0; i < flverMesh.Vertices.Count; i++)
                {
                    float weightMult = 1 / (
                        flverMesh.Vertices[i].BoneWeights[0] + 
                        flverMesh.Vertices[i].BoneWeights[1] + 
                        flverMesh.Vertices[i].BoneWeights[2] + 
                        flverMesh.Vertices[i].BoneWeights[3]);

                    flverMesh.Vertices[i].BoneWeights[0] = flverMesh.Vertices[i].BoneWeights[0] * weightMult;
                    flverMesh.Vertices[i].BoneWeights[1] = flverMesh.Vertices[i].BoneWeights[1] * weightMult;
                    flverMesh.Vertices[i].BoneWeights[2] = flverMesh.Vertices[i].BoneWeights[2] * weightMult;
                    flverMesh.Vertices[i].BoneWeights[3] = flverMesh.Vertices[i].BoneWeights[3] * weightMult;
                }

                foreach (var face in mesh.Faces)
                {
                    //TODO: See if resets need to be added inbetween or anything.
                    flverFaceSet.Indices.AddRange(face.Indices);
                }


                flverMesh.FaceSets.Add(flverFaceSet);
                GenerateLodAndMotionBlurFacesets(flverMesh);

                flverMesh.VertexBuffers.Add(flverVertBuffer);
                flver.BufferLayouts.Add(flverBufferLayout);

                flver.Meshes.Add(flverMesh);
            }

            return result;
        }

        private static void GenerateLodAndMotionBlurFacesets(FLVER2.Mesh mesh)
        {
            var newFacesetsToAdd = new List<SoulsFormats.FLVER2.FaceSet>();
            foreach (var faceset in mesh.FaceSets)
            {
                var lod1 = new SoulsFormats.FLVER2.FaceSet()
                {
                    CullBackfaces = faceset.CullBackfaces,
                    Flags = FLVER2.FaceSet.FSFlags.LodLevel1,
                    TriangleStrip = faceset.TriangleStrip,
                    Indices = faceset.Indices
                };

                var lod2 = new FLVER2.FaceSet()
                {
                    CullBackfaces = faceset.CullBackfaces,
                    Flags = FLVER2.FaceSet.FSFlags.LodLevel2,
                    TriangleStrip = faceset.TriangleStrip,
                    Indices = faceset.Indices
                };

                var mblur = new FLVER2.FaceSet()
                {
                    CullBackfaces = faceset.CullBackfaces,
                    Flags = FLVER2.FaceSet.FSFlags.MotionBlur,
                    TriangleStrip = faceset.TriangleStrip,
                    Indices = faceset.Indices
                };

                var mblurlod1 = new FLVER2.FaceSet()
                {
                    CullBackfaces = faceset.CullBackfaces,
                    Flags = FLVER2.FaceSet.FSFlags.LodLevel1 | FLVER2.FaceSet.FSFlags.MotionBlur,
                    TriangleStrip = faceset.TriangleStrip,
                    Indices = faceset.Indices
                };

                var mblurlod2 = new FLVER2.FaceSet()
                {
                    CullBackfaces = faceset.CullBackfaces,
                    Flags = FLVER2.FaceSet.FSFlags.LodLevel2 | FLVER2.FaceSet.FSFlags.MotionBlur,
                    TriangleStrip = faceset.TriangleStrip,
                    Indices = faceset.Indices
                };

                newFacesetsToAdd.Add(lod1);
                newFacesetsToAdd.Add(lod2);
                newFacesetsToAdd.Add(mblur);
                newFacesetsToAdd.Add(mblurlod1);
                newFacesetsToAdd.Add(mblurlod2);
            }

            foreach (var lod in newFacesetsToAdd)
            {
                mesh.FaceSets.Add(lod);
            }
        }
    }
}
