using Microsoft.Xna.Framework;
using SFAnimExtensions.Havok;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DSAnimStudio
{
    public class HavokRecorder
    {
        public class HavokFrame
        {
            public Vector4 RootMotion = Vector4.Zero;
            public List<NewBlendableTransform> BoneTransforms = new List<NewBlendableTransform>();
        }

        public List<string> TransformTrackNames = new List<string>();

        public List<HavokFrame> Frames = new List<HavokFrame>();

        public float Duration => (float)(Frames.Count * DeltaTime);

        public double FrameRate = 60;
        public double DeltaTime => 1.0 / FrameRate;

        public HavokRecorder(List<NewAnimSkeleton.HkxBoneInfo> boneTransforms)
        {
            foreach (var transform in boneTransforms)
            {
                TransformTrackNames.Add(transform.Name);
            }
        }

        public void ClearRecording()
        {
            Frames.Clear();
        }

        public void AddFrame(Vector4 rootMotionDelta, List<NewAnimSkeleton.HkxBoneInfo> boneTransforms)
        {
            var frame = new HavokFrame();
            frame.RootMotion = (Frames.Count > 0 ? Frames[Frames.Count - 1].RootMotion : Vector4.Zero) + rootMotionDelta;
            foreach (var transform in boneTransforms)
            {
                frame.BoneTransforms.Add(transform.CurrentHavokTransform);
            }
            Frames.Add(frame);
        }

        public void FinalizeRecording()
        {
            var endFrame = new HavokFrame();
            foreach (var t in Frames[0].BoneTransforms)
            {
                endFrame.BoneTransforms.Add(t);
            }
            endFrame.RootMotion = Frames[Frames.Count - 1].RootMotion + (Frames[Frames.Count - 1].RootMotion - Frames[Frames.Count - 2].RootMotion);
            Frames.Add(endFrame);

            var rootMotionStart = Frames[0].RootMotion;

            for (int i = 0; i < Frames.Count; i++)
            {
                Frames[i].RootMotion -= rootMotionStart;
                var xyz = Vector3.Transform(Frames[i].RootMotion.XYZ(), Matrix.CreateRotationY(-rootMotionStart.W));
                Frames[i].RootMotion.X = xyz.X;
                Frames[i].RootMotion.Y = xyz.Y;
                Frames[i].RootMotion.Z = xyz.Z;
                Frames[i].RootMotion.W = Frames[i].RootMotion.W;
            }

            if (File.Exists("RECORD_TEST.xml"))
                File.Delete("RECORD_TEST.xml");

            using (var testStream = File.OpenWrite("RECORD_TEST.xml"))
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

            ClearRecording();
        }
    }
}
