using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using SoulsAssetPipeline.Animation;

namespace DSAnimStudio.Havok
{
    public class HKAnimRetarget
    {
       

        public class HKBoneTrackThing
        {
            public List<NewBlendableTransform> Frames = new List<NewBlendableTransform>();
            public List<NewBlendableTransform> RelativeFrames = new List<NewBlendableTransform>();

            public NewBlendableTransform ReferenceTransform = NewBlendableTransform.Identity;

            //public HKBoneTrackThing(List<NewBlendableTransform> frames)
            //{
            //    Frames = frames;
            //    CalculateRelativeFrames();
            //}

            public void UnfuckFirstRelFrame(NewBlendableTransform targetTPose, NewBlendableTransform importTPose, float posScale, string boneName)
            {
                //RelativeFrames[0] = new NewBlendableTransform()
                //{
                //    Translation = RelativeFrames[0].Translation,// + (targetTPose.Translation - importTPose.Translation),
                //    Scale = RelativeFrames[0].Scale,// * (targetTPose.Scale / importTPose.Scale),
                //    Rotation = RelativeFrames[0].Rotation * (importTPose.Rotation / targetTPose.Rotation),
                //};
            }


            public void Unfuck(NewBlendableTransform targetTPose, NewBlendableTransform importTPose, float posScale, string boneName)
            {
                var newFrames = new List<NewBlendableTransform>();
                for (int i = 0; i < Frames.Count; i++)
                {
                    var newPos = (Frames[i].Translation);

                    if (newPos.LengthSquared() > 0)
                    {
                        newPos = Vector3.Normalize(newPos.ToXna()).ToCS() * targetTPose.Translation.Length();
                    }

                    if (boneName == "MASTER")
                    {
                        //throw new Exception("breh");
                    }


                    newFrames.Add(new NewBlendableTransform()
                    {
                        Translation = newPos,
                        Scale = (Frames[i].Scale * (importTPose.Scale / targetTPose.Scale)),
                        Rotation = (Frames[i].Rotation.ToXna() * (importTPose.Rotation.ToXna() * Quaternion.Inverse(targetTPose.Rotation.ToXna()))).ToCS(),
                    });
                }
                Frames = newFrames;
            }

            public void ReplaceWithNewRelativeFrames(List<NewBlendableTransform> newRelativeFrames)
            {
                var oldRelFrame1 = RelativeFrames[0];
                RelativeFrames = newRelativeFrames;
                RelativeFrames[0] = oldRelFrame1;
                Frames = null;
            }

            public void CalculateAbsoluteFrames(float motionScale, float rotScale)
            {
                Frames = new List<NewBlendableTransform>();
                for (int i = 0; i < RelativeFrames.Count; i++)
                {
                    if (i == 0)
                    {
                        Frames.Add(new NewBlendableTransform()
                        {
                            Translation = (ReferenceTransform.Translation + (RelativeFrames[i].Translation)),
                            Scale = (ReferenceTransform.Scale * RelativeFrames[i].Scale),
                            Rotation = (ReferenceTransform.Rotation * RelativeFrames[i].Rotation),
                        });
                    }
                    else
                    {
                        Frames.Add(new NewBlendableTransform()
                        {
                            Translation = (Frames[i - 1].Translation + (RelativeFrames[i].Translation * motionScale)),
                            Scale = (Frames[i - 1].Scale * RelativeFrames[i].Scale),
                            Rotation = Frames[i - 1].Rotation * RelativeFrames[i].Rotation,//Quaternion.Slerp(Quaternion.CreateFromYawPitchRoll(0, 0, 0), RelativeFrames[i].Rotation, rotScale),
                        });
                    }
                    
                   
                }
            }

            public void CalculateRelativeFrames()
            {
                RelativeFrames.Clear();
                for (int i = 0; i < Frames.Count; i++)
                {
                    if (i == 0)
                    {
                        RelativeFrames.Add(new NewBlendableTransform()
                        {
                            Translation = (Frames[i].Translation - ReferenceTransform.Translation),
                            Scale = (Frames[i].Scale / ReferenceTransform.Scale),
                            Rotation = (Frames[i].Rotation / ReferenceTransform.Rotation),
                        });
                    }
                    else
                    {
                        RelativeFrames.Add(new NewBlendableTransform()
                        {
                            Translation = (Frames[i].Translation - Frames[i - 1].Translation),
                            Scale = (Frames[i].Scale / Frames[i - 1].Scale),
                            Rotation = (Frames[i].Rotation / Frames[i - 1].Rotation),
                        });
                    }

                    

                    

                }
            }
        }

        public class HKBoneTrackThing_Mirror
        {
            public List<NewBlendableTransform> Frames = new List<NewBlendableTransform>();
            public List<Matrix> Frames_World = new List<Matrix>();

            public string MirrorTarget;

            public bool HasBeenSwappedWithMirrorTarget = false;

            public string Parent = null;

            public List<string> Children = new List<string>();
        }

        public class HavokBoneMeme
        {
            public NewBlendableTransform ReferenceTransform;
            public string Parent = null;
            public List<string> Children = new List<string>();
        }

        private static Dictionary<string, HavokBoneMeme> LoadBonesFromHKX2010(string hkxName)
        {
            var hkx = HKX.Read(hkxName, HKX.HKXVariation.HKXDS1, isDS1RAnimHotfix: false);

            var result = new Dictionary<string, HavokBoneMeme>();

            foreach (var obj in hkx.DataSection.Objects)
            {
                if (obj is HKX.HKASkeleton skelington)
                {
                    List<string> boneNames = new List<string>();

                    foreach (var bone in skelington.Bones.GetArrayData().Elements)
                    {
                        var n = bone.Name.GetString();
                        result.Add(n, new HavokBoneMeme());
                        boneNames.Add(n);
                    }

                    var skelingtonTransf = skelington.Transforms.GetArrayData().Elements;

                    for (int i = 0; i < skelingtonTransf.Count; i++)
                    {
                        result[boneNames[i]].ReferenceTransform.Translation = skelingtonTransf[i].Position.Vector.ToVector3();

                        result[boneNames[i]].ReferenceTransform.Scale = skelingtonTransf[i].Scale.Vector.ToVector3();

                        result[boneNames[i]].ReferenceTransform.Rotation = skelingtonTransf[i].Rotation.Vector.ToQuat();
                    }

                    var skelingtonParentIndices = skelington.ParentIndices.GetArrayData().Elements;

                    for (int i = 0; i < skelingtonParentIndices.Count; i++)
                    {
                        var parentIndex = skelingtonParentIndices[i].data;
                        result[boneNames[i]].Parent = parentIndex >= 0 ? boneNames[parentIndex] : null;
                    }

                    foreach (var kvp in result)
                    {
                        if (kvp.Value.Parent != null)
                        {
                            if (!result[kvp.Value.Parent].Children.Contains(kvp.Key))
                                result[kvp.Value.Parent].Children.Add(kvp.Key);
                        }
                    }
                }
            }

            return result;
        }

        //private static (float Duration, Dictionary<string, HKBoneTrackThing> Tracks) ReadJustBoneTracksFromSplineCompressedXML(string splineXmlName)
        //{
        //    XmlDocument doc = new XmlDocument();
        //    doc.Load(splineXmlName);
        //    XmlNode root = doc.DocumentElement;

        //    //root = root.SelectSingleNode("hkpackfile");

        //    if (root.Attributes["contentsversion"].Value != "hk_2010.2.0-r1")
        //        throw new Exception("bruh");

        //    foreach (XmlNode section in root.SelectNodes("hksection"))
        //    {
        //        if (section.Attributes["name"].Value == "__data__")
        //        {

        //            foreach (XmlNode hkObjectNode in section.SelectNodes("hkobject"))
        //            {

        //                if (hkObjectNode.Attributes["class"].Value == "hkaSplineCompressedAnimation")
        //                {
        //                    hkObjectNode.Attributes["class"].Value = "hkaInterleavedUncompressedAnimation";
        //                    hkObjectNode.Attributes["signature"].Value = "0x930af031";

        //                    Dictionary<string, XmlNode> hkParamNodeDict = new Dictionary<string, XmlNode>();
        //                    foreach (XmlNode hkParamNode in hkObjectNode.SelectNodes("hkparam"))
        //                    {
        //                        var paramName = hkParamNode.Attributes["name"].Value;
        //                        if (!hkParamNodeDict.ContainsKey(paramName))
        //                            hkParamNodeDict.Add(paramName, hkParamNode);
        //                    }

        //                    hkParamNodeDict["type"].InnerText = "HK_INTERLEAVED_ANIMATION";

        //                    var trackNames = new List<string>();
        //                    var node_annotationTracks = hkParamNodeDict["annotationTracks"];

        //                    foreach (XmlNode annotObj in node_annotationTracks.SelectNodes("hkobject"))
        //                    {
        //                        bool foundTrackName = false;
        //                        foreach (XmlNode annotParam in annotObj.SelectNodes("hkparam"))
        //                        {
        //                            if (annotParam.Attributes["name"].Value == "trackName")
        //                            {
        //                                foundTrackName = true;
        //                                trackNames.Add(annotParam.InnerText.Replace(" ", "_").ToUpper());
        //                            }
        //                        }
        //                        if (!foundTrackName)
        //                            throw new Exception("fatcat");
        //                    }

        //                    var node_numFrames = hkParamNodeDict["numFrames"];
        //                    var node_numBlocks = hkParamNodeDict["numBlocks"];
        //                    var node_maxFramesPerBlock = hkParamNodeDict["maxFramesPerBlock"];
        //                    var node_maskAndQuantizationSize = hkParamNodeDict["maskAndQuantizationSize"];
        //                    var node_blockDuration = hkParamNodeDict["blockDuration"];
        //                    var node_blockInverseDuration = hkParamNodeDict["blockInverseDuration"];
        //                    var node_frameDuration = hkParamNodeDict["frameDuration"];
        //                    var node_blockOffsets = hkParamNodeDict["blockOffsets"];
        //                    var node_floatBlockOffsets = hkParamNodeDict["floatBlockOffsets"];
        //                    var node_transformOffsets = hkParamNodeDict["transformOffsets"];
        //                    var node_floatOffsets = hkParamNodeDict["floatOffsets"];
        //                    var node_data = hkParamNodeDict["data"];
        //                    var node_endian = hkParamNodeDict["endian"];

        //                    int numTransformTracks = int.Parse(hkParamNodeDict["numberOfTransformTracks"].InnerText);
        //                    int numFrames = int.Parse(node_numFrames.InnerText);
        //                    int numBlocks = int.Parse(node_numBlocks.InnerText);
        //                    int numFramesPerBlock = int.Parse(node_maxFramesPerBlock.InnerText);

        //                    byte[] animData = node_data.InnerText
        //                        .Replace("\n", " ")
        //                        .Replace("\t", "")
        //                        .Split(' ')
        //                        .Where(n => !string.IsNullOrWhiteSpace(n))
        //                        .Select(n => byte.Parse(n.Trim()))
        //                        .ToArray();

        //                    var uncompressedTransforms = SplineCompressedAnimation.ReadSplCmpAnimBytesAndSampleToUncomp
        //                        (animData, numTransformTracks, numBlocks, numFrames, numFramesPerBlock, trackNames);

        //                    Dictionary<string, HKBoneTrackThing> boneTrackMemes = new Dictionary<string, HKBoneTrackThing>();

        //                    foreach (var transf in uncompressedTransforms)
        //                    {
        //                        foreach (var kvp in transf)
        //                        {
        //                            if (!boneTrackMemes.ContainsKey(kvp.Key))
        //                                boneTrackMemes.Add(kvp.Key, new HKBoneTrackThing());

        //                            boneTrackMemes[kvp.Key].Frames.Add(kvp.Value);
        //                        }
        //                    }

        //                    foreach (var kvp in boneTrackMemes)
        //                    {
        //                        kvp.Value.CalculateRelativeFrames();
        //                    }

        //                    return (float.Parse(node_frameDuration.InnerText) * numFrames, boneTrackMemes);
        //                }

        //            }

        //        }
        //    }

        //    return (0, null);
        //}

        static string[] FirstLRBones = new string[]
        {
            "L_Po",
            "R_Po",
            "B_Wepon_Case0_R",
            "B_Wepon_Case0_L",
            "L_Clavicle",
            "R_Clavicle",
            "L_Thigh",
            "R_Thigh",
            "L_Wepon_Case",
            "R_Wepon_Case",
        };

        //public static void MirrorSplineCompressedXMLtoInterleavedUncompressedXML(string splineXmlName, string outputXmlname, string skeletonHkxName)
        //{
        //    XmlDocument doc = new XmlDocument();
        //    doc.Load(splineXmlName);
        //    XmlNode root = doc.DocumentElement;

        //    var skeleton = LoadBonesFromHKX2010(skeletonHkxName);

        //    //root = root.SelectSingleNode("hkpackfile");

        //    if (root.Attributes["contentsversion"].Value != "hk_2010.2.0-r1")
        //        throw new Exception("bruh");

        //    foreach (XmlNode section in root.SelectNodes("hksection"))
        //    {
        //        if (section.Attributes["name"].Value == "__data__")
        //        {

        //            foreach (XmlNode hkObjectNode in section.SelectNodes("hkobject"))
        //            {

        //                if (hkObjectNode.Attributes["class"].Value == "hkaSplineCompressedAnimation")
        //                {
        //                    hkObjectNode.Attributes["class"].Value = "hkaInterleavedUncompressedAnimation";
        //                    hkObjectNode.Attributes["signature"].Value = "0x930af031";

        //                    Dictionary<string, XmlNode> hkParamNodeDict = new Dictionary<string, XmlNode>();
        //                    foreach (XmlNode hkParamNode in hkObjectNode.SelectNodes("hkparam"))
        //                    {
        //                        var paramName = hkParamNode.Attributes["name"].Value;
        //                        if (!hkParamNodeDict.ContainsKey(paramName))
        //                            hkParamNodeDict.Add(paramName, hkParamNode);
        //                    }

        //                    hkParamNodeDict["type"].InnerText = "HK_INTERLEAVED_ANIMATION";

        //                    var trackNames = new List<string>();
        //                    var node_annotationTracks = hkParamNodeDict["annotationTracks"];

        //                    foreach (XmlNode annotObj in node_annotationTracks.SelectNodes("hkobject"))
        //                    {
        //                        bool foundTrackName = false;
        //                        foreach (XmlNode annotParam in annotObj.SelectNodes("hkparam"))
        //                        {
        //                            if (annotParam.Attributes["name"].Value == "trackName")
        //                            {
        //                                foundTrackName = true;
        //                                trackNames.Add(annotParam.InnerText);
        //                            }
        //                        }
        //                        if (!foundTrackName)
        //                            throw new Exception("fatcat");
        //                    }

        //                    var node_duration = hkParamNodeDict["duration"];

        //                    var node_numFrames = hkParamNodeDict["numFrames"];
        //                    var node_numBlocks = hkParamNodeDict["numBlocks"];
        //                    var node_maxFramesPerBlock = hkParamNodeDict["maxFramesPerBlock"];
        //                    var node_maskAndQuantizationSize = hkParamNodeDict["maskAndQuantizationSize"];
        //                    var node_blockDuration = hkParamNodeDict["blockDuration"];
        //                    var node_blockInverseDuration = hkParamNodeDict["blockInverseDuration"];
        //                    var node_frameDuration = hkParamNodeDict["frameDuration"];
        //                    var node_blockOffsets = hkParamNodeDict["blockOffsets"];
        //                    var node_floatBlockOffsets = hkParamNodeDict["floatBlockOffsets"];
        //                    var node_transformOffsets = hkParamNodeDict["transformOffsets"];
        //                    var node_floatOffsets = hkParamNodeDict["floatOffsets"];
        //                    var node_data = hkParamNodeDict["data"];
        //                    var node_endian = hkParamNodeDict["endian"];

        //                    int numTransformTracks = int.Parse(hkParamNodeDict["numberOfTransformTracks"].InnerText);
        //                    int numFrames = int.Parse(node_numFrames.InnerText);
        //                    int numBlocks = int.Parse(node_numBlocks.InnerText);
        //                    int numFramesPerBlock = int.Parse(node_maxFramesPerBlock.InnerText);

        //                    byte[] animData = node_data.InnerText
        //                        .Replace("\n", " ")
        //                        .Replace("\t", "")
        //                        .Split(' ')
        //                        .Where(n => !string.IsNullOrWhiteSpace(n))
        //                        .Select(n => byte.Parse(n.Trim()))
        //                        .ToArray();

        //                    var uncompressedTransforms = SplineCompressedAnimation.ReadSplCmpAnimBytesAndSampleToUncomp
        //                        (animData, numTransformTracks, numBlocks, numFrames, numFramesPerBlock, trackNames);

        //                    Dictionary<string, HKBoneTrackThing_Mirror> boneTrackMemes = new Dictionary<string, HKBoneTrackThing_Mirror>();

        //                    foreach (var transf in uncompressedTransforms)
        //                    {
        //                        foreach (var kvp in transf)
        //                        {
        //                            if (!boneTrackMemes.ContainsKey(kvp.Key))
        //                                boneTrackMemes.Add(kvp.Key, new HKBoneTrackThing_Mirror());

        //                            boneTrackMemes[kvp.Key].Frames.Add(kvp.Value);
        //                        }
        //                    }

        //                    foreach (var kvp in boneTrackMemes)
        //                    {
        //                        if (kvp.Value.MirrorTarget == null)
        //                        {
        //                            string tryRL = kvp.Key.Replace("R", "L");
        //                            if (tryRL != kvp.Key && boneTrackMemes.ContainsKey(tryRL))
        //                            {
        //                                kvp.Value.MirrorTarget = tryRL;
        //                                boneTrackMemes[tryRL].MirrorTarget = kvp.Key;
        //                                continue;
        //                            }

        //                            string tryLR = kvp.Key.Replace("L", "R");
        //                            if (tryLR != kvp.Key && boneTrackMemes.ContainsKey(tryLR))
        //                            {
        //                                kvp.Value.MirrorTarget = tryLR;
        //                                boneTrackMemes[tryLR].MirrorTarget = kvp.Key;
        //                            }
        //                        }
                                
        //                    }

        //                    foreach (var kvp in boneTrackMemes)
        //                    {
        //                        kvp.Value.Parent = skeleton[kvp.Key].Parent;
        //                        kvp.Value.Children = skeleton[kvp.Key].Children;

        //                        if (kvp.Value.MirrorTarget != null && !kvp.Value.HasBeenSwappedWithMirrorTarget)
        //                        {
        //                            var a = new List<NewBlendableTransform>();
        //                            var b = new List<NewBlendableTransform>();

        //                            for (int i = 0; i < kvp.Value.Frames.Count; i++)
        //                            {
        //                                a.Add(kvp.Value.Frames[i]);
        //                                b.Add(boneTrackMemes[kvp.Value.MirrorTarget].Frames[i]);
        //                            }

        //                            //for (int i = 0; i < boneTrackMemes[kvp.Value.MirrorTarget].Frames.Count; i++)
        //                            //{

        //                            //}

        //                            for (int i = 0; i < kvp.Value.Frames.Count; i++)
        //                            {
        //                                //var x = kvp.Value.Frames[i];
        //                                //x.Translation = b[i].Translation;
        //                                kvp.Value.Frames[i] = b[i];

        //                                //var y = boneTrackMemes[kvp.Value.MirrorTarget].Frames[i];
        //                                //y.Translation = a[i].Translation;
        //                                boneTrackMemes[kvp.Value.MirrorTarget].Frames[i] = a[i];
        //                            }

        //                            //kvp.Value.Frames = b;
        //                            //boneTrackMemes[kvp.Value.MirrorTarget].Frames = a;

        //                            kvp.Value.HasBeenSwappedWithMirrorTarget = true;
        //                            boneTrackMemes[kvp.Value.MirrorTarget].HasBeenSwappedWithMirrorTarget = true;
        //                        }

        //                        //kvp.Value.Frames_World.Clear();
        //                        //foreach (var f in kvp.Value.Frames)
        //                        //{
        //                        //    kvp.Value.Frames_World.Add(Matrix.Identity);
        //                        //}
        //                    }

        //                    for (int i = 0; i < numFrames; i++)
        //                    {
        //                        var topLevelBones = boneTrackMemes.Where(b => b.Value.Parent == null);

        //                        void BoneToWorldSpace(HKBoneTrackThing_Mirror b, Matrix parentMatrix)
        //                        {
        //                            b.Frames[i] = b.Frames[i].Composed();
        //                            b.Frames_World[i] = parentMatrix * b.Frames[i].ComposedMatrix;

        //                            foreach (var c in b.Children)
        //                            {
        //                                BoneToWorldSpace(boneTrackMemes[c], b.Frames_World[i]);
        //                            }
        //                        }

        //                        void BoneToBoneSpace(HKBoneTrackThing_Mirror b, Matrix parentMatrix, bool isTopLevel)
        //                        {
        //                            var thisBoneMatrix = b.Frames_World[i];

        //                            if (!isTopLevel)
        //                                b.Frames_World[i] = Matrix.Invert(parentMatrix) * b.Frames_World[i];

        //                            foreach (var c in b.Children)
        //                            {
        //                                BoneToBoneSpace(boneTrackMemes[c], thisBoneMatrix, false);
        //                            }
        //                        }

        //                        void UnfuckBone(HKBoneTrackThing_Mirror b, bool doChildren)
        //                        {
        //                            var fr = b.Frames[i];

                                    

        //                            //newRot.X *= -1;
        //                            //newRot.Y *= -1;
        //                            //newRot.Z *= -1;

        //                            foreach (var c in b.Children)
        //                            {
        //                                var chfr = boneTrackMemes[c].Frames[i];
        //                                chfr.Translation *= -1;
        //                                chfr.Rotation.X *= -1;
        //                                chfr.Rotation.Y *= -1;
        //                                chfr.Rotation.Z *= -1;
        //                                boneTrackMemes[c].Frames[i] = chfr;
        //                            }

        //                            fr.Rotation.X *= -1;
        //                            fr.Rotation.Y *= -1;
        //                            fr.Rotation.Z *= -1;

        //                            b.Frames[i] = fr;

        //                            if (doChildren)
        //                            {


        //                                foreach (var c in b.Children)
        //                                {
        //                                    UnfuckBone(boneTrackMemes[c], true);
        //                                }
        //                            }
        //                        }

        //                        foreach (var kvp in boneTrackMemes)
        //                        {
        //                            var fr = kvp.Value.Frames[i];
        //                            fr.Translation.X *= -1;
        //                            fr.Rotation.X *= -1;
        //                            fr.Rotation.W *= -1;
        //                            kvp.Value.Frames[i] = fr;
        //                        }

        //                        foreach (var kvp in topLevelBones)
        //                        {
        //                            UnfuckBone(kvp.Value, false);
        //                        }

        //                        //foreach (var kvp in topLevelBones)
        //                        //{
        //                        //    BoneToWorldSpace(kvp.Value, Matrix.Identity);
        //                        //}

        //                        // Bones are now in world space

        //                        //foreach (var kvp in boneTrackMemes)
        //                        //{
        //                        //    //kvp.Value.Frames_World[i] *= Matrix.CreateScale(-1, -1, -1);
        //                        //    var frameData = kvp.Value.Frames[i];
        //                        //    frameData.ComposedMatrix = kvp.Value.Frames_World[i];
        //                        //    frameData = frameData.Decomposed();
        //                        //    frameData.Translation.X *= -1;
        //                        //    frameData.Rotation.X *= -1;
        //                        //    frameData.Rotation.W *= -1;
        //                        //    frameData = frameData.Composed();
        //                        //    kvp.Value.Frames[i] = frameData;
        //                        //    kvp.Value.Frames_World[i] = kvp.Value.Frames[i].ComposedMatrix;
        //                        //}

        //                        // Bones are now about to go back to bone space

        //                        //foreach (var kvp in topLevelBones)
        //                        //{
        //                        //    BoneToBoneSpace(kvp.Value, Matrix.Identity, true);
        //                        //}

        //                        //foreach (var kvp in boneTrackMemes)
        //                        //{
        //                        //    //if (kvp.Value.Parent != null)
        //                        //    //{
        //                        //    //    kvp.Value.Frames_World[i] = Matrix.Invert(boneTrackMemes[kvp.Value.Parent].Frames_World[i]) * kvp.Value.Frames_World[i];
        //                        //    //}

        //                        //    var frameData = kvp.Value.Frames[i];

        //                        //    frameData.ComposedMatrix = kvp.Value.Frames_World[i];
        //                        //    frameData = frameData.Decomposed();

        //                        //    kvp.Value.Frames[i] = frameData;
        //                        //}
        //                    }

        //                    //foreach (var kvp in boneTrackMemes)
        //                    //{
        //                    //    for (int i = 0; i < kvp.Value.Frames.Count; i++)
        //                    //    {
        //                    //        //if (kvp.Key == "Master")
        //                    //        //{
        //                    //        //    continue;
        //                    //        //}

        //                    //        var frame = kvp.Value.Frames[i];



        //                    //        //frame.Translation.X *= -1;
        //                    //        //frame.Translation.Y *= -1;
        //                    //        //frame.Translation.Z *= -1;
        //                    //        //frame.Translation.Z *= -1;
        //                    //        //frame.Rotation.X *= -1;
        //                    //        //frame.Rotation.Y *= -1;
        //                    //        //frame.Rotation.Z *= -1;
        //                    //        //frame.Rotation.W *= -1;
        //                    //        //frame.Scale *= -1;

        //                    //        //frame.Rotation.X *= -1;
        //                    //        //frame.Rotation.Y *= -1;
        //                    //        //frame.Rotation.Z *= -1;
        //                    //        //frame.Rotation.W *= -1;

        //                    //        if (FirstLRBones.Contains(kvp.Key))
        //                    //        {
        //                    //            //frame.Translation.X *= -1;

        //                    //            //frame.Rotation.X *= -1;
        //                    //            ////frame.Rotation.Y *= -1;
        //                    //            ////frame.Rotation.Z *= -1;
        //                    //            //frame.Rotation.W *= -1;

        //                    //            //frame.Rotation = Quaternion.Identity;
        //                    //        }



        //                    //        //frame.Rotation = Quaternion.Inverse(frame.Rotation);


        //                    //        //frame.Rotation.X *= -1;
        //                    //        //frame.Rotation.Z *= -1;
        //                    //        //frame.Rotation.W *= -1;



        //                    //        //frame.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.Pi);

        //                    //        //frame.Rotation.X *= -1;
        //                    //        //frame.Rotation.Y *= -1;
        //                    //        //frame.Rotation.W *= -1;

        //                    //        //frame.Rotation = Quaternion.Negate(frame.Rotation);

        //                    //        if (!kvp.Value.HasBeenSwappedWithMirrorTarget)
        //                    //        {
        //                    //            //frame.Translation.X *= -1;

        //                    //            //frame.Rotation.X *= -1;
        //                    //            //frame.Rotation.Z *= -1;
        //                    //            //frame.Rotation.W *= -1;
        //                    //        }
        //                    //        else
        //                    //        {
        //                    //            //frame.Rotation = Quaternion.Inverse(frame.Rotation);
        //                    //        }

        //                    //        frame.Compose();
        //                    //        frame.Decompose();

        //                    //        //if (kvp.Value.HasBeenSwappedWithMirrorTarget)// && kvp.Key != "Master" && kvp.Key != "Upper_Root" && kvp.Key != "Lower_Root" && kvp.Key != "Pelvis" && kvp.Key != "Spine")
        //                    //        //{
        //                    //        //    //Matrix rotMatrix = Matrix.CreateFromQuaternion(frame.Rotation);
        //                    //        //    ////rotMatrix *= Matrix.CreateScale(-1, 1, 1);
        //                    //        //    //rotMatrix *= Matrix.CreateRotationZ(MathHelper.Pi);
        //                    //        //    //frame.Rotation = Quaternion.CreateFromRotationMatrix(rotMatrix);

        //                    //        //    frame.Rotation.X *= -1;
        //                    //        //    //frame.Rotation.Y *= -1;
        //                    //        //    //frame.Rotation.Z *= -1;
        //                    //        //    frame.Rotation.W *= -1;

        //                    //        //    frame.Translation.X *= -1;
        //                    //        //}
        //                    //        //else
        //                    //        //{
        //                    //        //    //frame.Translation.X *= -1;
        //                    //        //}



        //                    //        //if (kvp.Key == "Master")
        //                    //        //{
        //                    //        //    frame.Scale.X *= -1;
        //                    //        //    frame.Scale.Y *= -1;
        //                    //        //    frame.Scale.Z *= -1;
        //                    //        //}

        //                    //        //frame.Scale.X *= -1;
        //                    //        //frame.Scale.Y *= -1;
        //                    //        //frame.Scale.Z *= -1;



        //                    //        //frame.Compose();

        //                    //        //frame.ComposedMatrix *= Matrix.CreateScale(-1, 1, 1);

        //                    //        //frame.Decompose();

        //                    //        kvp.Value.Frames[i] = frame;
        //                    //    }
        //                    //}

        //                    //Change data to transforms
        //                    node_data.Attributes["name"].Value = "transforms";

        //                    var uncompStrBuild = new StringBuilder();

        //                    int totalTransformCount = 0;

        //                    for (int i = 0; i < numFrames; i++)
        //                    {
        //                        foreach (var kvp in boneTrackMemes)
        //                        {
        //                            //if (targetSkeleton.ContainsKey(kvp.Key) && importSkeleton.ContainsKey(kvp.Key))
        //                            //{
        //                            //    boneTrackMemes[kvp.Key].Unfuck(targetSkeleton[kvp.Key].ReferenceTransform, importSkeleton[kvp.Key].ReferenceTransform);
        //                            //}

        //                            uncompStrBuild.Append("\n        ");

        //                            var pos = kvp.Value.Frames[i].Translation;

        //                            uncompStrBuild.Append($"({pos.X:0.000000} {pos.Y:0.000000} {pos.Z:0.000000})");
        //                            //uncompStrBuild.Append($"(0 0 0)");
        //                            uncompStrBuild.Append($"({kvp.Value.Frames[i].Rotation.X:0.000000} {kvp.Value.Frames[i].Rotation.Y:0.000000} {kvp.Value.Frames[i].Rotation.Z:0.000000} {kvp.Value.Frames[i].Rotation.W:0.000000})");
        //                            uncompStrBuild.Append($"({kvp.Value.Frames[i].Scale.X:0.000000} {kvp.Value.Frames[i].Scale.Y:0.000000} {kvp.Value.Frames[i].Scale.Z:0.000000})");

        //                            totalTransformCount++;
        //                        }
        //                    }



        //                    uncompStrBuild.Append("\n      ");

        //                    node_data.InnerText = uncompStrBuild.ToString();
        //                    node_data.Attributes["numelements"].Value = totalTransformCount.ToString();

        //                    //Change floatOffsets to floats
        //                    node_floatOffsets.Attributes["name"].Value = "floats";
        //                    node_floatOffsets.Attributes["numelements"].Value = "0";
        //                    node_floatOffsets.InnerText = "";

        //                    hkObjectNode.RemoveChild(node_numFrames);
        //                    hkObjectNode.RemoveChild(node_numBlocks);
        //                    hkObjectNode.RemoveChild(node_maxFramesPerBlock);
        //                    hkObjectNode.RemoveChild(node_maskAndQuantizationSize);
        //                    hkObjectNode.RemoveChild(node_blockDuration);
        //                    hkObjectNode.RemoveChild(node_blockInverseDuration);
        //                    hkObjectNode.RemoveChild(node_frameDuration);
        //                    hkObjectNode.RemoveChild(node_blockOffsets);
        //                    hkObjectNode.RemoveChild(node_floatBlockOffsets);
        //                    hkObjectNode.RemoveChild(node_transformOffsets);
        //                    //hkObjectNode.RemoveChild(node_floatOffsets);
        //                    //hkObjectNode.RemoveChild(node_data);
        //                    hkObjectNode.RemoveChild(node_endian);
        //                }

        //            }

        //        }
        //    }



        //    doc.Save(outputXmlname);
        //}

        public static void ConvertSplineXMLtoUncompressedXML(string splineXmlName, string uncompressedXmlName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(splineXmlName);
            XmlNode root = doc.DocumentElement;

            //var targetSkeleton = LoadBonesFromHKX2010(targetSkeletonHkxName);
            //var importSkeleton = LoadBonesFromHKX2010(importSkeletonHkxName);

            //root = root.SelectSingleNode("hkpackfile");

            if (root.Attributes["contentsversion"].Value != "hk_2010.2.0-r1")
                throw new Exception("bruh");

            foreach (XmlNode section in root.SelectNodes("hksection"))
            {
                if (section.Attributes["name"].Value == "__data__")
                {

                    foreach (XmlNode hkObjectNode in section.SelectNodes("hkobject"))
                    {

                        if (hkObjectNode.Attributes["class"].Value == "hkaSplineCompressedAnimation")
                        {
                            hkObjectNode.Attributes["class"].Value = "hkaInterleavedUncompressedAnimation";
                            hkObjectNode.Attributes["signature"].Value = "0x930af031";

                            Dictionary<string, XmlNode> hkParamNodeDict = new Dictionary<string, XmlNode>();
                            foreach (XmlNode hkParamNode in hkObjectNode.SelectNodes("hkparam"))
                            {
                                var paramName = hkParamNode.Attributes["name"].Value;
                                if (!hkParamNodeDict.ContainsKey(paramName))
                                    hkParamNodeDict.Add(paramName, hkParamNode);
                            }

                            hkParamNodeDict["type"].InnerText = "HK_INTERLEAVED_ANIMATION";

                            var trackNames = new List<string>();
                            var node_annotationTracks = hkParamNodeDict["annotationTracks"];

                            foreach (XmlNode annotObj in node_annotationTracks.SelectNodes("hkobject"))
                            {
                                bool foundTrackName = false;
                                foreach (XmlNode annotParam in annotObj.SelectNodes("hkparam"))
                                {
                                    if (annotParam.Attributes["name"].Value == "trackName")
                                    {
                                        foundTrackName = true;
                                        trackNames.Add(annotParam.InnerText.Replace(" ", "_").ToUpper());
                                    }
                                }
                                if (!foundTrackName)
                                    throw new Exception("fatcat");
                            }

                            var node_duration = hkParamNodeDict["duration"];

                            var node_numFrames = hkParamNodeDict["numFrames"];
                            var node_numBlocks = hkParamNodeDict["numBlocks"];
                            var node_maxFramesPerBlock = hkParamNodeDict["maxFramesPerBlock"];
                            var node_maskAndQuantizationSize = hkParamNodeDict["maskAndQuantizationSize"];
                            var node_blockDuration = hkParamNodeDict["blockDuration"];
                            var node_blockInverseDuration = hkParamNodeDict["blockInverseDuration"];
                            var node_frameDuration = hkParamNodeDict["frameDuration"];
                            var node_blockOffsets = hkParamNodeDict["blockOffsets"];
                            var node_floatBlockOffsets = hkParamNodeDict["floatBlockOffsets"];
                            var node_transformOffsets = hkParamNodeDict["transformOffsets"];
                            var node_floatOffsets = hkParamNodeDict["floatOffsets"];
                            var node_data = hkParamNodeDict["data"];
                            var node_endian = hkParamNodeDict["endian"];

                            int numTransformTracks = int.Parse(hkParamNodeDict["numberOfTransformTracks"].InnerText);
                            int numFrames = int.Parse(node_numFrames.InnerText);
                            int numBlocks = int.Parse(node_numBlocks.InnerText);
                            int numFramesPerBlock = int.Parse(node_maxFramesPerBlock.InnerText);

                            byte[] animData = node_data.InnerText
                                .Replace("\n", " ")
                                .Replace("\t", "")
                                .Split(' ')
                                .Where(n => !string.IsNullOrWhiteSpace(n))
                                .Select(n => byte.Parse(n.Trim()))
                                .ToArray();

                            var uncompressedTransforms = SplineCompressedAnimation.ReadSplCmpAnimBytesAndSampleToUncomp
                                (animData, numTransformTracks, numBlocks, numFrames, numFramesPerBlock);


                            //Change data to transforms
                            node_data.Attributes["name"].Value = "transforms";

                            var uncompStrBuild = new StringBuilder();



                            //foreach (var transf in uncompressedTransforms)
                            //{
                            //    foreach (var kvp in transf)
                            //    {
                            //        uncompStrBuild.Append("\n        ");

                            //        var pos = kvp.Value.Translation;
                            //        uncompStrBuild.Append($"({pos.X:0.000000} {pos.Y:0.000000} {pos.Z:0.000000})");
                            //        //uncompStrBuild.Append($"(0 0 0)");
                            //        uncompStrBuild.Append($"({kvp.Value.Rotation.X:0.000000} {kvp.Value.Rotation.Y:0.000000} {kvp.Value.Rotation.Z:0.000000} {kvp.Value.Rotation.W:0.000000})");
                            //        uncompStrBuild.Append($"({kvp.Value.Scale.X:0.000000} {kvp.Value.Scale.Y:0.000000} {kvp.Value.Scale.Z:0.000000})");
                            //    }

                            //}

                            //foreach (var kvp in boneTrackMemes)
                            //{
                            //    if (targetSkeleton.ContainsKey(kvp.Key) && importSkeleton.ContainsKey(kvp.Key))
                            //    {
                            //        boneTrackMemes[kvp.Key].Unfuck(targetSkeleton[kvp.Key].ReferenceTransform, importSkeleton[kvp.Key].ReferenceTransform, 1f / 3, kvp.Key);
                            //    }
                            //}

                            //foreach (var kvp in boneTrackMemes)
                            //{
                            //    if (targetSkeleton.ContainsKey(kvp.Key) && importSkeleton.ContainsKey(kvp.Key))
                            //    {
                            //        boneTrackMemes[kvp.Key].Unfuck(targetSkeleton[kvp.Key].ReferenceTransform, importSkeleton[kvp.Key].ReferenceTransform, 1f / 3, kvp.Key);
                            //    }
                            //}

                            foreach (var t in uncompressedTransforms)
                            {
                                uncompStrBuild.Append("\n        ");

                                var pos = t.Translation;

                                uncompStrBuild.Append($"({pos.X:0.000000} {pos.Y:0.000000} {pos.Z:0.000000})");
                                //uncompStrBuild.Append($"(0 0 0)");
                                uncompStrBuild.Append($"({t.Rotation.X:0.000000} {t.Rotation.Y:0.000000} {t.Rotation.Z:0.000000} {t.Rotation.W:0.000000})");
                                uncompStrBuild.Append($"({t.Scale.X:0.000000} {t.Scale.Y:0.000000} {t.Scale.Z:0.000000})");

                            }

                            //int totalTransformCount = 0;

                            //for (int i = 0; i < importedFrameCount; i++)
                            //{
                            //    foreach (var kvp in boneTrackMemes)
                            //    {
                            //        //if (targetSkeleton.ContainsKey(kvp.Key) && importSkeleton.ContainsKey(kvp.Key))
                            //        //{
                            //        //    boneTrackMemes[kvp.Key].Unfuck(targetSkeleton[kvp.Key].ReferenceTransform, importSkeleton[kvp.Key].ReferenceTransform);
                            //        //}

                            //        uncompStrBuild.Append("\n        ");

                            //        var pos = kvp.Value.Frames[i].Translation;

                            //        uncompStrBuild.Append($"({pos.X:0.000000} {pos.Y:0.000000} {pos.Z:0.000000})");
                            //        //uncompStrBuild.Append($"(0 0 0)");
                            //        uncompStrBuild.Append($"({kvp.Value.Frames[i].Rotation.X:0.000000} {kvp.Value.Frames[i].Rotation.Y:0.000000} {kvp.Value.Frames[i].Rotation.Z:0.000000} {kvp.Value.Frames[i].Rotation.W:0.000000})");
                            //        uncompStrBuild.Append($"({kvp.Value.Frames[i].Scale.X:0.000000} {kvp.Value.Frames[i].Scale.Y:0.000000} {kvp.Value.Frames[i].Scale.Z:0.000000})");

                            //        totalTransformCount++;
                            //    }
                            //}

                           

                            uncompStrBuild.Append("\n      ");

                            node_data.InnerText = uncompStrBuild.ToString();
                            node_data.Attributes["numelements"].Value = uncompressedTransforms.Count.ToString();

                            //Change floatOffsets to floats
                            node_floatOffsets.Attributes["name"].Value = "floats";
                            node_floatOffsets.Attributes["numelements"].Value = "0";
                            node_floatOffsets.InnerText = "";

                            hkObjectNode.RemoveChild(node_numFrames);
                            hkObjectNode.RemoveChild(node_numBlocks);
                            hkObjectNode.RemoveChild(node_maxFramesPerBlock);
                            hkObjectNode.RemoveChild(node_maskAndQuantizationSize);
                            hkObjectNode.RemoveChild(node_blockDuration);
                            hkObjectNode.RemoveChild(node_blockInverseDuration);
                            hkObjectNode.RemoveChild(node_frameDuration);
                            hkObjectNode.RemoveChild(node_blockOffsets);
                            hkObjectNode.RemoveChild(node_floatBlockOffsets);
                            hkObjectNode.RemoveChild(node_transformOffsets);
                            //hkObjectNode.RemoveChild(node_floatOffsets);
                            //hkObjectNode.RemoveChild(node_data);
                            hkObjectNode.RemoveChild(node_endian);
                        }

                    }

                }
            }

            

            doc.Save(uncompressedXmlName);
        }



    }
}
