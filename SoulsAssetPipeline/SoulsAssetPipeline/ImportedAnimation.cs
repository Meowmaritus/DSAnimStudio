using Assimp;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SoulsAssetPipeline
{
    public class ImportedAnimation : HavokAnimationData
    {
        public ImportedAnimation()
            : base ("SAP Custom Animation", null, null, null)
        {

        }

        public class Frame
        {
            public System.Numerics.Vector4 RootMotion => 
                new System.Numerics.Vector4(
                    RootMotionTranslation.X, RootMotionTranslation.Y,
                    RootMotionTranslation.Z, RootMotionRotation);

            public System.Numerics.Vector3 RootMotionTranslation;
            public float RootMotionRotation;

            public List<NewBlendableTransform> BoneTransforms = new List<NewBlendableTransform>();
        }

        internal List<string> TransformTrackNames = new List<string>();

        internal List<Frame> Frames = new List<Frame>();

        //public double Duration;

        //public double Framerate = 60;
        //public double DeltaTime => 1.0 / Framerate;

        public void WriteToHavok2010InterleavedUncompressedXML(string fileName)
        {
            var endFrame = new Frame();
            foreach (var t in Frames[0].BoneTransforms)
            {
                endFrame.BoneTransforms.Add(t);
            }
            endFrame.RootMotionTranslation = Frames[Frames.Count - 1].RootMotionTranslation +
                (Frames[Frames.Count - 1].RootMotionTranslation - Frames[Frames.Count - 2].RootMotionTranslation);

            endFrame.RootMotionRotation = Frames[Frames.Count - 1].RootMotionRotation +
                (Frames[Frames.Count - 1].RootMotionRotation - Frames[Frames.Count - 2].RootMotionRotation);


            Frames.Add(endFrame);

            var rootMotionStart = Frames[0].RootMotion;

            for (int i = 0; i < Frames.Count; i++)
            {
                Frames[i].RootMotionTranslation.X -= rootMotionStart.X;
                Frames[i].RootMotionTranslation.Y -= rootMotionStart.Y;
                Frames[i].RootMotionTranslation.Z -= rootMotionStart.Z;
                Frames[i].RootMotionRotation -= rootMotionStart.W;
                var xyz = System.Numerics.Vector3.Transform(Frames[i].RootMotion.XYZ(), System.Numerics.Matrix4x4.CreateRotationY(-rootMotionStart.W));
                Frames[i].RootMotionTranslation.X = xyz.X;
                Frames[i].RootMotionTranslation.Y = xyz.Y;
                Frames[i].RootMotionTranslation.Z = xyz.Z;
                Frames[i].RootMotionRotation = Frames[i].RootMotion.W;
            }

            if (File.Exists(fileName))
                File.Delete(fileName);

            using (var testStream = File.OpenWrite(fileName))
            {
                using (var writer = XmlWriter.Create(testStream, new XmlWriterSettings()
                {
                    Indent = true,
                    Encoding = Encoding.ASCII,
                }))
                {
                    writer.WriteStartDocument();
                    {
                        writer.WriteStartElement("hkpackfile");
                        {
                            writer.WriteAttributeString("classversion", "8");
                            writer.WriteAttributeString("contentsversion", "hk_2010.2.0-r1");
                            writer.WriteAttributeString("toplevelobject", "#0040");

                            writer.WriteStartElement("hksection");
                            {
                                writer.WriteAttributeString("name", "__data__");

                                writer.WriteStartElement("hkobject");
                                {
                                    writer.WriteAttributeString("name", "#0043");
                                    writer.WriteAttributeString("class", "hkaDefaultAnimatedReferenceFrame");
                                    writer.WriteAttributeString("signature", "0x6d85e445");

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "up");
                                        writer.WriteString("(0.000000 1.000000 0.000000 0.000000)");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "forward");
                                        writer.WriteString("(0.000000 0.000000 1.000000 0.000000)");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "duration");
                                        writer.WriteString(Duration.ToString("0.000000"));
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "referenceFrameSamples");
                                        writer.WriteAttributeString("numelements", (Frames.Count - 1).ToString());
                                        var sb = new StringBuilder();
                                        for (int i = 0; i < Frames.Count - 1; i++)
                                        {
                                            sb.Append("\n\t\t\t\t");
                                            sb.Append($"({Frames[i].RootMotion.X:0.000000} {Frames[i].RootMotion.Y:0.000000} {Frames[i].RootMotion.Z:0.000000} {Frames[i].RootMotion.W:0.000000})");
                                        }
                                        sb.Append("\n\t\t\t");
                                        writer.WriteString(sb.ToString());
                                    }
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();


                                writer.WriteStartElement("hkobject");
                                {
                                    writer.WriteAttributeString("name", "#0042");
                                    writer.WriteAttributeString("class", "hkaInterleavedUncompressedAnimation");
                                    writer.WriteAttributeString("signature", "0x930af031");

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "type");
                                        writer.WriteString("HK_INTERLEAVED_ANIMATION");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "duration");
                                        writer.WriteString(Duration.ToString("0.000000"));
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "numberOfTransformTracks");
                                        writer.WriteString(TransformTrackNames.Count.ToString());
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "numberOfFloatTracks");
                                        writer.WriteString("0");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "extractedMotion");
                                        writer.WriteString("#0043");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "annotationTracks");
                                        writer.WriteAttributeString("numelements", TransformTrackNames.Count.ToString());
                                        foreach (var t in TransformTrackNames)
                                        {
                                            writer.WriteStartElement("hkobject");
                                            {
                                                writer.WriteStartElement("hkparam");
                                                {
                                                    writer.WriteAttributeString("name", "trackName");
                                                    writer.WriteString(t);
                                                }
                                                writer.WriteEndElement();

                                                writer.WriteStartElement("hkparam");
                                                {
                                                    writer.WriteAttributeString("name", "annotations");
                                                    writer.WriteAttributeString("numelements", "0");
                                                    writer.WriteString("");
                                                }
                                                writer.WriteEndElement();
                                            }
                                            writer.WriteEndElement();
                                        }
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "floats");
                                        writer.WriteAttributeString("numelements", "0");
                                        writer.WriteString("");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "transforms");

                                        var sb = new StringBuilder();

                                        int transformCount = 0;

                                        for (int f = 0; f < Frames.Count; f++)
                                        {
                                            for (int t = 0; t < TransformTrackNames.Count; t++)
                                            {
                                                sb.Append("\n\t\t\t\t");
                                                sb.Append($"({Frames[f].BoneTransforms[t].Translation.X:0.000000} " +
                                                    $"{Frames[f].BoneTransforms[t].Translation.Y:0.000000} " +
                                                    $"{Frames[f].BoneTransforms[t].Translation.Z:0.000000})");
                                                sb.Append($"({Frames[f].BoneTransforms[t].Rotation.X:0.000000} " +
                                                    $"{Frames[f].BoneTransforms[t].Rotation.Y:0.000000} " +
                                                    $"{Frames[f].BoneTransforms[t].Rotation.Z:0.000000} " +
                                                    $"{Frames[f].BoneTransforms[t].Rotation.W:0.000000})");
                                                sb.Append($"({Frames[f].BoneTransforms[t].Scale.X:0.000000} " +
                                                    $"{Frames[f].BoneTransforms[t].Scale.Y:0.000000} " +
                                                    $"{Frames[f].BoneTransforms[t].Scale.Z:0.000000})");
                                                transformCount++;
                                            }
                                        }

                                        sb.Append("\n\t\t\t");

                                        writer.WriteAttributeString("numelements", Frames.Count.ToString());

                                        writer.WriteString(sb.ToString());
                                    }
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();


                                writer.WriteStartElement("hkobject");
                                {
                                    writer.WriteAttributeString("name", "#0041");
                                    writer.WriteAttributeString("class", "hkaAnimationBinding");
                                    writer.WriteAttributeString("signature", "0x66eac971");

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "originalSkeletonName");
                                        writer.WriteString(TransformTrackNames[0]);
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "animation");
                                        writer.WriteString("#0042");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "transformTrackToBoneIndices");
                                        writer.WriteAttributeString("numelements", $"{TransformTrackNames.Count}");
                                        writer.WriteString(string.Join(" ", TransformTrackNames.Select((s, i) => i.ToString())));
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "floatTrackToFloatSlotIndices");
                                        writer.WriteAttributeString("numelements", "0");
                                        writer.WriteString("");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "blendHint");
                                        writer.WriteString("NORMAL");
                                    }
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();

                                writer.WriteStartElement("hkobject");
                                {
                                    writer.WriteAttributeString("name", "#0044");
                                    writer.WriteAttributeString("class", "hkaAnimationContainer");
                                    writer.WriteAttributeString("signature", "0x8dc20333");

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "skeletons");
                                        writer.WriteAttributeString("numelements", "0");
                                        writer.WriteString("");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "animations");
                                        writer.WriteAttributeString("numelements", "1");
                                        writer.WriteString("#0042");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "bindings");
                                        writer.WriteAttributeString("numelements", "1");
                                        writer.WriteString("#0041");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "attachments");
                                        writer.WriteAttributeString("numelements", "0");
                                        writer.WriteString("");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "skins");
                                        writer.WriteAttributeString("numelements", "0");
                                        writer.WriteString("");
                                    }
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();

                                writer.WriteStartElement("hkobject");
                                {
                                    writer.WriteAttributeString("name", "#0040");
                                    writer.WriteAttributeString("class", "hkRootLevelContainer");
                                    writer.WriteAttributeString("signature", "0x2772c11e");

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "namedVariants");
                                        writer.WriteAttributeString("numelements", "1");
                                        writer.WriteStartElement("hkobject");
                                        {

                                            writer.WriteStartElement("hkparam");
                                            {
                                                writer.WriteAttributeString("name", "name");
                                                writer.WriteString("Merged Animation Container");
                                            }
                                            writer.WriteEndElement();

                                            writer.WriteStartElement("hkparam");
                                            {
                                                writer.WriteAttributeString("name", "className");
                                                writer.WriteString("hkaAnimationContainer");
                                            }
                                            writer.WriteEndElement();

                                            writer.WriteStartElement("hkparam");
                                            {
                                                writer.WriteAttributeString("name", "variant");
                                                writer.WriteString("#0044");
                                            }
                                            writer.WriteEndElement();

                                        }
                                        writer.WriteEndElement();
                                    }
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndDocument();
                }
            }
        }

        public override NewBlendableTransform GetBlendableTransformOnFrame(int hkxBoneIndex, float frame)
        {
            var frameRatio = frame % 1;
            frame = frame % FrameCount;
            if (frameRatio != 0)
            {
                var blendFrom = Frames[(int)Math.Floor(frame)].BoneTransforms[hkxBoneIndex];
                var blendTo = Frames[(int)Math.Ceiling(frame)].BoneTransforms[hkxBoneIndex];
                return NewBlendableTransform.Lerp(blendFrom, blendTo, frameRatio);
            }
            else
            {
                return Frames[(int)frame].BoneTransforms[hkxBoneIndex];
            }
            
        }
    }
}
