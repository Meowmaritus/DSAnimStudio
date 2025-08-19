using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static SoulsAssetPipeline.Animation.HKX;

namespace DSAnimStudio
{
    public static class HavokXmlWriter
    {
        public static byte[] ConvertHavokXmlToPackfile(byte[] xml)
        {
            string dir = $@"{Main.Directory}\Res\DesAnims";
            string xmlInput = $@"{dir}\Input.xml";
            string hkx2010Output = $@"{dir}\Input_hk2010.hkx";

            File.WriteAllBytes(xmlInput, xml);

            var procStart = new ProcessStartInfo($@"{Main.Directory}\Res\DesAnims\hkxcmd.exe",
                $"Convert -i \"{xmlInput}\" -o \"{hkx2010Output}\" -v:WIN32");
            procStart.CreateNoWindow = true;
            procStart.WindowStyle = ProcessWindowStyle.Hidden;
            procStart.UseShellExecute = false;
            procStart.RedirectStandardOutput = true;
            procStart.RedirectStandardError = true;
            procStart.WorkingDirectory = dir;
            var proc = new Process();
            proc.StartInfo = procStart;
            proc.Start();
            proc.WaitForExit();

            byte[] result = File.ReadAllBytes(hkx2010Output);

            return result;
        }

        public static void WriteHkparamField(XmlWriter writer, string paramName, string paramValue)
        {
            writer.WriteStartElement("hkparam");
            {
                writer.WriteAttributeString("name", paramName);
                writer.WriteString(paramValue);
            }
            writer.WriteEndElement();
        }

        public static void WriteEmptyHkparamArrayToXml(XmlWriter writer, string paramName)
        {
            writer.WriteStartElement("hkparam");
            {
                writer.WriteAttributeString("name", paramName);
                writer.WriteAttributeString("numelements", "0");
                writer.WriteString("");
            }
            writer.WriteEndElement();
        }

        public static void WriteHkparamArrayToXml<T>(XmlWriter writer, string paramName, List<T> data, Func<T, string> selDataStr, int numPerLine = 16)
        {


            writer.WriteStartElement("hkparam");
            {
                writer.WriteAttributeString("name", paramName);
                writer.WriteAttributeString("numelements", data.Count.ToString());

                var sb = new StringBuilder();

                if (data.Count > 0)
                {

                    sb.Append($"\n{writer.Settings.IndentChars}{writer.Settings.IndentChars}{writer.Settings.IndentChars}{writer.Settings.IndentChars}");

                    int numBytesOnCurLine = 0;
                    int arrIndex = 0;
                    foreach (var b in data)
                    {
                        sb.Append(selDataStr(b));
                        numBytesOnCurLine++;

                        if (arrIndex < data.Count - 1)
                        {
                            if (numBytesOnCurLine >= numPerLine)
                            {
                                sb.Append($"\n{writer.Settings.IndentChars}{writer.Settings.IndentChars}{writer.Settings.IndentChars}{writer.Settings.IndentChars}");
                                numBytesOnCurLine = 0;
                            }
                            else
                            {
                                sb.Append(" ");
                            }
                        }

                        arrIndex++;
                    }

                    sb.Append($"\n{writer.Settings.IndentChars}{writer.Settings.IndentChars}{writer.Settings.IndentChars}");
                }

                writer.WriteString(sb.ToString());
            }
            writer.WriteEndElement();
        }

        public static void WriteHkparamArrayToXml<T>(XmlWriter writer, string paramName, HKX.HKArray<T> hkArray, Func<T, string> selDataStr, int numPerLine = 16)
            where T : IHKXSerializable, new()
        {


            writer.WriteStartElement("hkparam");
            {
                var arrayData = hkArray.GetArrayData();
                List<T> data = new List<T>();
                if (arrayData != null)
                {
                    data = hkArray.GetArrayData().Elements;
                }
                writer.WriteAttributeString("name", paramName);
                writer.WriteAttributeString("numelements", data.Count.ToString());

                var sb = new StringBuilder();
                if (data.Count > 0)
                {

                    sb.Append($"\n{writer.Settings.IndentChars}{writer.Settings.IndentChars}{writer.Settings.IndentChars}{writer.Settings.IndentChars}");

                    int numBytesOnCurLine = 0;
                    int arrIndex = 0;
                    foreach (var b in data)
                    {
                        sb.Append(selDataStr(b));
                        numBytesOnCurLine++;

                        if (arrIndex < data.Count - 1)
                        {
                            if (numBytesOnCurLine >= numPerLine)
                            {
                                sb.Append($"\n{writer.Settings.IndentChars}{writer.Settings.IndentChars}{writer.Settings.IndentChars}{writer.Settings.IndentChars}");
                                numBytesOnCurLine = 0;
                            }
                            else
                            {
                                sb.Append(" ");
                            }
                        }
                        arrIndex++;
                    }

                    sb.Append($"\n{writer.Settings.IndentChars}{writer.Settings.IndentChars}{writer.Settings.IndentChars}");
                }
                writer.WriteString(sb.ToString());
            }
            writer.WriteEndElement();
        }

        public static byte[] WriteHavokPackfileToXML(HKX hkx, HKX skeletonHKX)
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            HKASkeleton skel = null;
            foreach (var obj in hkx.DataSection.Objects)
            {
                if (obj is HKASkeleton asSkeleton)
                {
                    skel = asSkeleton;
                }
            }
            if (skel == null && skeletonHKX != null)
            {
                foreach (var obj in skeletonHKX.DataSection.Objects)
                {
                    if (obj is HKASkeleton asSkeleton)
                    {
                        skel = asSkeleton;
                    }
                }
            }

            if (skel == null)
            {
                throw new InvalidDataException("No Havok skeleton found.");
            }

            List<string> transformTrackNames = new List<string>();


            using (var testStream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(testStream, new XmlWriterSettings()
                {
                    Indent = true,
                    Encoding = Encoding.ASCII,
                    IndentChars = "\t",
                }))
                {
                    uint OBJ_INDEX = 0x40;

                    uint rootLevelContainerIndex = OBJ_INDEX++;

                    Dictionary<HKX.HKXObject, uint> objIDs = new Dictionary<HKX.HKXObject, uint>();
                    Dictionary<uint, HKX.HKXObject> objsByID = new Dictionary<uint, HKX.HKXObject>();

                    List<uint> obj_Animations = new List<uint>();
                    List<uint> obj_Bindings = new List<uint>();
                    List<uint> obj_Skeletons = new List<uint>();
                    uint obj_RootMotion = 0;

                    void WriteObject (HKX.HKXObject obj, uint objID)
                    {
                        writer.WriteStartElement("hkobject");
                        {
                            writer.WriteAttributeString("name", $"#{(objID):X4}");
                            

                            // Inner write.
                            if (obj is HKX.HKADefaultAnimatedReferenceFrame objHKADefaultAnimatedReferenceFrame)
                            {
                                writer.WriteAttributeString("class", "hkaDefaultAnimatedReferenceFrame");
                                writer.WriteAttributeString("signature", "0x6d85e445");

                                var up = objHKADefaultAnimatedReferenceFrame.Up.ToXna();
                                WriteHkparamField(writer, "up", $"({up.X:0.000000} {up.Y:0.000000} {up.Z:0.000000} {up.W:0.000000})");

                                var forward = objHKADefaultAnimatedReferenceFrame.Up.ToXna();
                                WriteHkparamField(writer, "forward", $"({forward.X:0.000000} {forward.Y:0.000000} {forward.Z:0.000000} {forward.W:0.000000})");

                                WriteHkparamField(writer, "duration", objHKADefaultAnimatedReferenceFrame.Duration.ToString("0.000000"));

                                WriteHkparamArrayToXml(writer, "referenceFrameSamples", objHKADefaultAnimatedReferenceFrame.ReferenceFrameSamples,
                                    f => $"({f.Vector.X:0.000000} {f.Vector.Y:0.000000} {f.Vector.Z:0.000000} {f.Vector.W:0.000000})", numPerLine: 1);
                            }
                            else if (obj is HKX.HKAAnimationBinding objHKAAnimationBinding)
                            {
                                writer.WriteAttributeString("class", "hkaAnimationBinding");
                                writer.WriteAttributeString("signature", "0x66eac971");

                                WriteHkparamField(writer, "originalSkeletonName", objHKAAnimationBinding.OriginalSkeletonName);
                                // Awful lazy hack
                                WriteHkparamField(writer, "animation", obj_Animations.Count > 0 ? $"#{(obj_Animations[0]):X4}" : "null");
                                WriteHkparamArrayToXml(writer, "transformTrackToBoneIndices", objHKAAnimationBinding.TransformTrackToBoneIndices, d => d.data.ToString());
                                WriteHkparamArrayToXml(writer, "floatTrackToFloatSlotIndices", objHKAAnimationBinding.FloatTrackToFloatSlotIndices, d => d.data.ToString());
                                // Lazy hack
                                WriteHkparamField(writer, "blendHint", (uint)objHKAAnimationBinding.BlendHint > 0 ? "ADDITIVE" : "NORMAL");
                            }
                            else if (obj is HKX.HKASkeleton objHKASkeleton)
                            {
                                writer.WriteAttributeString("class", "hkaSkeleton");
                                writer.WriteAttributeString("signature", "0x366e8220");
                                WriteHkparamField(writer, "name", objHKASkeleton.Name.GetString());
                                WriteHkparamArrayToXml(writer, "parentIndices", objHKASkeleton.ParentIndices, d => d.data.ToString());

                                writer.WriteStartElement("hkparam");
                                {
                                    var bones = objHKASkeleton.Bones.GetArrayData().Elements;
                                    writer.WriteAttributeString("name", "bones");
                                    writer.WriteAttributeString("numelements", bones.Count.ToString());

                                    foreach (var b in bones)
                                    {
                                        writer.WriteStartElement("hkobject");
                                        {
                                            WriteHkparamField(writer, "name", b.Name.GetString());
                                            WriteHkparamField(writer, "lockTranslation", b.LockTranslation > 0 ? "true" : "false");
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();

                                WriteHkparamArrayToXml(writer, "referencePose", objHKASkeleton.Transforms, t =>
                                    $"({t.Position.Vector.X:0.000000} {t.Position.Vector.Y:0.000000} {t.Position.Vector.Z:0.000000})" +
                                    $"({t.Rotation.Vector.X:0.000000} {t.Rotation.Vector.Y:0.000000} {t.Rotation.Vector.Z:0.000000} {t.Rotation.Vector.W:0.000000})" +
                                    $"({t.Scale.Vector.X:0.000000} {t.Scale.Vector.Y:0.000000} {t.Scale.Vector.Z:0.000000})", numPerLine: 1);

                                WriteHkparamArrayToXml(writer, "referenceFloats", objHKASkeleton.ReferenceFloats, d => d.data.ToString());
                                WriteEmptyHkparamArrayToXml(writer, "floatSlots");
                                WriteEmptyHkparamArrayToXml(writer, "localFrames");
                            }
                            else if (obj is HKX.HKAInterleavedUncompressedAnimation objHKAInterleavedUncompressedAnimation)
                            {
                                writer.WriteAttributeString("class", "hkaInterleavedUncompressedAnimation");
                                writer.WriteAttributeString("signature", "0x930af031");

                                WriteHkparamField(writer, "type", "HK_INTERLEAVED_ANIMATION");
                                WriteHkparamField(writer, "duration", objHKAInterleavedUncompressedAnimation.Duration.ToString("0.000000"));
                                WriteHkparamField(writer, "numberOfTransformTracks", objHKAInterleavedUncompressedAnimation.TransformTrackCount.ToString());
                                WriteHkparamField(writer, "numberOfFloatTracks", "0");
                                WriteHkparamField(writer, "extractedMotion", obj_RootMotion > 0 ? $"#{obj_RootMotion:X4}" : "null");
                                //WriteEmptyHkparamArrayToXml(writer, "annotationTracks");
                                

                                writer.WriteStartElement("hkparam");
                                {
                                    writer.WriteAttributeString("name", "annotationTracks");
                                    writer.WriteAttributeString("numelements", transformTrackNames.Count.ToString());

                                    foreach (var tr in transformTrackNames)
                                    {
                                        writer.WriteStartElement("hkobject");
                                        {
                                            writer.WriteStartElement("hkparam");
                                            {
                                                writer.WriteAttributeString("name", "trackName");
                                                writer.WriteString(tr);
                                            }
                                            writer.WriteEndElement();
                                            WriteEmptyHkparamArrayToXml(writer, "annotations");
                                        }
                                        writer.WriteEndElement();
                                        
                                    }
                                }
                                writer.WriteEndElement();
                                WriteEmptyHkparamArrayToXml(writer, "floats");

                                WriteHkparamArrayToXml(writer, "transforms", objHKAInterleavedUncompressedAnimation.Transforms, t =>
                                    $"({t.Position.Vector.X:0.000000} {t.Position.Vector.Y:0.000000} {t.Position.Vector.Z:0.000000})" +
                                    $"({t.Rotation.Vector.X:0.000000} {t.Rotation.Vector.Y:0.000000} {t.Rotation.Vector.Z:0.000000} {t.Rotation.Vector.W:0.000000})" +
                                    $"({t.Scale.Vector.X:0.000000} {t.Scale.Vector.Y:0.000000} {t.Scale.Vector.Z:0.000000})", numPerLine: 1);

                            }
                            else if (obj is HKX.HKASplineCompressedAnimation objHKASplineCompressedAnimation)
                            {
                                writer.WriteAttributeString("class", "hkaSplineCompressedAnimation");
                                writer.WriteAttributeString("signature", "0x792ee0bb");

                                WriteHkparamField(writer, "type", "HK_SPLINE_COMPRESSED_ANIMATION");
                                WriteHkparamField(writer, "duration", objHKASplineCompressedAnimation.Duration.ToString("0.000000"));
                                WriteHkparamField(writer, "numberOfTransformTracks", objHKASplineCompressedAnimation.TransformTrackCount.ToString());
                                WriteHkparamField(writer, "numberOfFloatTracks", "0");
                                WriteHkparamField(writer, "extractedMotion", obj_RootMotion > 0 ? $"#{obj_RootMotion:X4}" : "null");
                                //WriteEmptyHkparamArrayToXml(writer, "annotationTracks");
                                writer.WriteStartElement("hkparam");
                                {
                                    writer.WriteAttributeString("name", "annotationTracks");
                                    writer.WriteAttributeString("numelements", transformTrackNames.Count.ToString());

                                    foreach (var tr in transformTrackNames)
                                    {
                                        writer.WriteStartElement("hkobject");
                                        {
                                            writer.WriteStartElement("hkparam");
                                            {
                                                writer.WriteAttributeString("name", "trackName");
                                                writer.WriteString(tr);
                                            }
                                            writer.WriteEndElement();
                                            WriteEmptyHkparamArrayToXml(writer, "annotations");
                                        }
                                        writer.WriteEndElement();

                                    }
                                }
                                writer.WriteEndElement();
                                WriteHkparamField(writer, "numFrames", objHKASplineCompressedAnimation.FrameCount.ToString());
                                WriteHkparamField(writer, "numBlocks", objHKASplineCompressedAnimation.BlockCount.ToString());
                                WriteHkparamField(writer, "maxFramesPerBlock", objHKASplineCompressedAnimation.FramesPerBlock.ToString());
                                WriteHkparamField(writer, "maskAndQuantizationSize", objHKASplineCompressedAnimation.MaskAndQuantization.ToString());
                                WriteHkparamField(writer, "blockDuration", objHKASplineCompressedAnimation.BlockDuration.ToString("0.000000"));
                                WriteHkparamField(writer, "blockInverseDuration", objHKASplineCompressedAnimation.InverseBlockDuration.ToString("0.000000"));
                                WriteHkparamField(writer, "frameDuration", objHKASplineCompressedAnimation.FrameDuration.ToString("0.000000"));
                                WriteHkparamArrayToXml(writer, "blockOffsets", objHKASplineCompressedAnimation.BlockOffsets, d => d.data.ToString());
                                WriteHkparamArrayToXml(writer, "floatBlockOffsets", objHKASplineCompressedAnimation.FloatBlockOffsets, d => d.data.ToString());
                                WriteHkparamArrayToXml(writer, "transformOffsets", objHKASplineCompressedAnimation.TransformOffsets, d => d.data.ToString());
                                WriteHkparamArrayToXml(writer, "floatOffsets", objHKASplineCompressedAnimation.FloatOffsets, d => d.data.ToString());
                                WriteHkparamArrayToXml(writer, "data", objHKASplineCompressedAnimation.Data, d => d.data.ToString());
                                WriteHkparamField(writer, "endian", objHKASplineCompressedAnimation.Endian.ToString());


                            }

                            //writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteStartDocument();
                    {
                        writer.WriteStartElement("hkpackfile");
                        {
                            writer.WriteAttributeString("classversion", "8");
                            writer.WriteAttributeString("contentsversion", "hk_2010.2.0-r1");
                            writer.WriteAttributeString("toplevelobject", $"#{rootLevelContainerIndex:X4}");

                            writer.WriteStartElement("hksection");
                            {
                                writer.WriteAttributeString("name", "__data__");

                                

                                // Map out IDs first
                                foreach (var obj in hkx.DataSection.Objects)
                                {
                                    uint objID = OBJ_INDEX++;
                                    objIDs[obj] = objID;
                                    objsByID[objID] = obj;

                                    if (obj is HKX.HKAInterleavedUncompressedAnimation or HKX.HKASplineCompressedAnimation)
                                        obj_Animations.Add(objID);
                                    else if (obj is HKX.HKAAnimationBinding)
                                        obj_Bindings.Add(objID);
                                    else if (obj is HKX.HKASkeleton)
                                        obj_Skeletons.Add(objID);
                                    else if (obj is HKX.HKADefaultAnimatedReferenceFrame)
                                        obj_RootMotion = objID;
                                }

                                transformTrackNames.Clear();
                                if (obj_Bindings.Count > 0)
                                {
                                    var bones = skel.Bones.GetArrayData().Elements;
                                    var bind = objsByID[obj_Bindings.First()] as HKAAnimationBinding;
                                    if (bind != null)
                                    {
                                        var boneIndices = bind.TransformTrackToBoneIndices.GetArrayData().Elements;
                                        for (int i = 0; i < boneIndices.Count; i++)
                                        {
                                            var idx = boneIndices[i].data;
                                            if (idx >= 0 && idx < bones.Count)
                                            {
                                                var bone = bones[idx];
                                                var boneName = bone.Name.GetString();
                                                transformTrackNames.Add(boneName);
                                            }
                                        }
                                    }
                                }

                                foreach (var obj in hkx.DataSection.Objects)
                                {
                                    if (obj is HKX.HKAInterleavedUncompressedAnimation or HKX.HKASplineCompressedAnimation
                                        or HKX.HKAAnimationBinding or HKX.HKASkeleton or HKADefaultAnimatedReferenceFrame)
                                    {
                                        uint objID = objIDs[obj];
                                        WriteObject(obj, objID);
                                    }
                                    else
                                    {
                                        Console.WriteLine("breakpoint");
                                    }
                                    
                                }

                                uint animationContainerIndex = OBJ_INDEX++;

                                writer.WriteStartElement("hkobject");
                                {
                                    writer.WriteAttributeString("name", $"#{(animationContainerIndex):X4}");
                                    writer.WriteAttributeString("class", "hkaAnimationContainer");
                                    writer.WriteAttributeString("signature", "0x8dc20333");

                                    WriteHkparamArrayToXml(writer, "skeletons", obj_Skeletons, s => $"#{s:X4}");
                                    WriteHkparamArrayToXml(writer, "animations", obj_Animations, s => $"#{s:X4}");
                                    WriteHkparamArrayToXml(writer, "bindings", obj_Bindings, s => $"#{s:X4}");
                                    WriteEmptyHkparamArrayToXml(writer, "attachments");
                                    WriteEmptyHkparamArrayToXml(writer, "skins");
                                }
                                writer.WriteEndElement();

                                writer.WriteStartElement("hkobject");
                                {
                                    writer.WriteAttributeString("name", $"#{rootLevelContainerIndex:X4}");
                                    writer.WriteAttributeString("class", "hkRootLevelContainer");
                                    writer.WriteAttributeString("signature", "0x2772c11e");

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "namedVariants");
                                        writer.WriteAttributeString("numelements", "1");
                                        writer.WriteStartElement("hkobject");
                                        {
                                            WriteHkparamField(writer, "name", "Merged Animation Container");
                                            WriteHkparamField(writer, "className", "hkaAnimationContainer");
                                            WriteHkparamField(writer, "variant", $"#{animationContainerIndex:X4}");
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

                return testStream.ToArray();
            }
        }
    }
}
