using Assimp;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoulsAssetPipeline
{
    public static class FLVERImportHelpers
    {
        public enum SoulsGames
        {
            DS1,
            DS1R,
            DS2,
            DS2SOTFS,
            DS3,
            BB,
            SDT,
        }

        public enum TextureChannelSemantic
        {
            Diffuse,
            SpecularColor,
            Shininess,
            NormalMap,
            DetailNormalMap,
            Emissive,

            Blendmask,

            Diffuse2,
            SpecularColor2,
            Shininess2,
            NormalMap2,
            DetailNormalMap2,
            Emissive2,
        }

        public static FLVER.VertexColor ToFlverVertexColor(this Color4D c)
        {
            return new FLVER.VertexColor(c.A, c.R, c.G, c.B);
        }

        public static System.Numerics.Vector3 ToNumerics(this Vector3D v)
        {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }

        public static System.Numerics.Quaternion ToNumerics(this Quaternion q)
        {
            return new System.Numerics.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static System.Numerics.Vector3 GetFlverBoneEulerFromQuaternion(Quaternion q)
        {
            // Store the Euler angles in radians
            double yaw;
            double pitch;
            double roll;

            double sqw = q.W * q.W;
            double sqx = q.X * q.X;
            double sqy = q.Y * q.Y;
            double sqz = q.Z * q.Z;

            // If quaternion is normalised the unit is one, otherwise it is the correction factor
            double unit = sqx + sqy + sqz + sqw;
            double test = q.X * q.Y + q.Z * q.W;

            if (test > 0.4995 * unit) // 0.4999 OR 0.5 - EPSILON
            {
                // Singularity at north pole
                yaw = 2.0 * Math.Atan2(q.X, q.W);
                pitch = Math.PI * 0.5;
                roll = 0.0;
            }
            else if (test < -0.4995 * unit) // -0.4999 OR -0.5 + EPSILON
            {
                // Singularity at south pole
                yaw = -2.0 * Math.Atan2(q.X, q.W);
                pitch = -Math.PI * 0.5;
                roll = 0.0;
            }
            else
            {
                yaw = Math.Atan2(2.0 * q.Y * q.W - 2.0 * q.X * q.Z, sqx - sqy - sqz + sqw);
                pitch = Math.Asin(2.0 * test / unit);
                roll = Math.Atan2(2.0 * q.X * q.W - 2.0 * q.Y * q.Z, -sqx + sqy - sqz + sqw);
            }

            return new System.Numerics.Vector3((float)pitch, (float)yaw, (float)roll);
        }


        public struct FLVERBoneTransform
        {
            public System.Numerics.Vector3 Translation;
            public System.Numerics.Vector3 Rotation;
            public System.Numerics.Vector3 Scale;

            public static FLVERBoneTransform FromMatrix4x4(Matrix4x4 m, System.Numerics.Matrix4x4 flverSceneMatrix)
            {
                var result = new FLVERBoneTransform();
                m.Decompose(out Vector3D s, out Quaternion rq, out Vector3D t);
                result.Translation = System.Numerics.Vector3.Transform(t.ToNumerics(), flverSceneMatrix);
                result.Scale = s.ToNumerics();
                var transformedQuat = rq.ToNumerics() * System.Numerics.Quaternion.CreateFromRotationMatrix(flverSceneMatrix);
                result.Rotation = GetFlverBoneEulerFromQuaternion(
                    new Quaternion(transformedQuat.X, transformedQuat.Y, transformedQuat.Z, transformedQuat.W));
                return result;
            }
        }

        public class FLVERMetaskeleton
        {
            public List<FLVER.Bone> Bones = new List<FLVER.Bone>();
            public List<FLVER.Dummy> DummyPoly = new List<FLVER.Dummy>();
        }

        public static FLVERMetaskeleton GenerateFlverMetaskeletonFromRootNode(
            Node rootNode, Matrix4x4 rootNodeAbsoluteMatrix, System.Numerics.Matrix4x4 flverSceneMatrix)
        {
            var bonesAssimp = new List<Node>();
            var skel = new FLVERMetaskeleton();
            var dummyAttachBoneNames = new List<string>();

            // Returns index of bone in master bone list if boneNode is a bone.
            // Returns -1 if boneNode is a DummyPoly (denoted with a node name starting with "DUMMY_POLY").
            int AddBone(Node boneNode, Node parentBoneNode, Matrix4x4 parentAbsoluteMatrix)
            {
                short parentBoneIndex = (short)(bonesAssimp.IndexOf(parentBoneNode));

                var thisNodeAbsoluteMatrix = boneNode.Transform * parentAbsoluteMatrix;

                if (boneNode.Name.StartsWith("DUMMY_POLY"))
                {
                    thisNodeAbsoluteMatrix.Decompose(out Vector3D dummyScale, out Quaternion dummyQuat, out Vector3D dummyTranslation);
                    var dmy = new FLVER.Dummy();
                    dmy.ParentBoneIndex = parentBoneIndex;
                    dmy.Position = System.Numerics.Vector3.Transform(dummyTranslation.ToNumerics(), flverSceneMatrix);

                    // Format: "DUMMY_POLY|<RefID>|<AttachBoneName>"
                    // Example: "DUMMY_POLY|220|Spine1"
                    string[] dummyNameParts = boneNode.Name.Split('|');

                    //ErrorTODO: TryParse
                    dmy.ReferenceID = short.Parse(dummyNameParts[1].Trim());

                    if (dummyNameParts.Length == 3)
                        dummyAttachBoneNames.Add(dummyNameParts[2]);
                    else
                        dummyAttachBoneNames.Add(null);

                    //NOTE: Maybe this should be specifiable? I forget what the point of false is here.
                    dmy.UseUpwardVector = true;

                    var sceneRotation = (dummyQuat.ToNumerics() * System.Numerics.Quaternion.CreateFromRotationMatrix(flverSceneMatrix));

                    dmy.Upward = System.Numerics.Vector3.Transform(new System.Numerics.Vector3(0, 1, 0), sceneRotation);
                    //TODO: Check if forward vector3 should be 1 or -1;
                    dmy.Forward = System.Numerics.Vector3.Transform(new System.Numerics.Vector3(0, 0, 1), sceneRotation);

                    skel.DummyPoly.Add(dmy);

                    return -1;
                }
                else
                {
                    bonesAssimp.Add(boneNode);

                    int thisBoneIndex = bonesAssimp.Count - 1;

                    var flverBone = new FLVER.Bone();

                    if (parentBoneNode != null)
                        flverBone.ParentIndex = parentBoneIndex;

                    flverBone.Name = boneNode.Name;
                    flverBone.BoundingBoxMin = System.Numerics.Vector3.One * 0.05f;
                    flverBone.BoundingBoxMax = System.Numerics.Vector3.One * -0.05f;

                    var boneTrans = FLVERBoneTransform.FromMatrix4x4(boneNode.Transform
                        // If this is the topmost bone in heirarchy, apply transform of whole skeleton.
                        * (parentBoneNode == null ? rootNodeAbsoluteMatrix : Matrix4x4.Identity),
                        flverSceneMatrix);

                    flverBone.Translation = boneTrans.Translation;
                    flverBone.Rotation = boneTrans.Rotation;
                    flverBone.Scale = boneTrans.Scale;

                    skel.Bones.Add(flverBone);

                    List<int> childBoneIndices = new List<int>();

                    foreach (var c in boneNode.Children)
                    {
                        int cIndex = AddBone(c, boneNode, thisNodeAbsoluteMatrix);

                        //cIndex will be -1 if the child node was a DummyPoly instead of a bone.
                        if (cIndex >= 0)
                            childBoneIndices.Add(cIndex);
                    }

                    if (childBoneIndices.Count > 0)
                    {
                        flverBone.ChildIndex = (short)childBoneIndices[0];

                        for (int i = 0; i < childBoneIndices.Count; i++)
                        {
                            var thisChildBone = skel.Bones[childBoneIndices[i]];
                            if (i == 0)
                                thisChildBone.PreviousSiblingIndex = -1;
                            else
                                thisChildBone.PreviousSiblingIndex = (short)(childBoneIndices[i - 1]);

                            if (i == childBoneIndices.Count - 1)
                                thisChildBone.NextSiblingIndex = -1;
                            else
                                thisChildBone.NextSiblingIndex = (short)(childBoneIndices[i + 1]);
                        }
                    }

                    return thisBoneIndex;
                }

                
               
            }

            //if (rootNode.Children == null)
            //    throw new InvalidDataException("Assimp scene has no heirarchy.");

            AddBone(rootNode.Children[0], null, rootNodeAbsoluteMatrix);

            return skel;
        }

    }
}
