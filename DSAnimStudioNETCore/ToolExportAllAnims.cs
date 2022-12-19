using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DSAnimStudio
{
    public class ToolExportAllAnims
    {
        public enum ExportAnimsFileType
        {
            Havok2010_2_XML,
            Havok2010_2_Packfile_x32,
            //Havok2014_1_Packfile_x64,
            Havok2016_1_Tagfile_x64,
        }


        HavokSplineFixer splineFixer = null;
        public void InitForAnimContainer(NewAnimationContainer animContainer)
        {
            if (splineFixer == null)
                splineFixer = new HavokSplineFixer(animContainer.Skeleton);

            
        }

        private byte[] writeHkxSkeletonTo2010Xml(NewAnimSkeleton_HKX skeleton)
        {
            using (var testStream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(testStream, new XmlWriterSettings()
                {
                    Indent = true,
                    Encoding = Encoding.ASCII,
                }))
                {
                    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

                    writer.WriteStartDocument();
                    {
                        writer.WriteStartElement("hkpackfile");
                        {
                            writer.WriteAttributeString("classversion", "8");
                            writer.WriteAttributeString("contentsversion", "hk_2010.2.0-r1");
                            writer.WriteAttributeString("toplevelobject", "#0038");

                            writer.WriteStartElement("hksection");
                            {
                                writer.WriteAttributeString("name", "__data__");

                                writer.WriteStartElement("hkobject");
                                {
                                    writer.WriteAttributeString("name", "#0039");
                                    writer.WriteAttributeString("class", "hkaSkeleton");
                                    writer.WriteAttributeString("signature", "0x366e8220");

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "name");
                                        writer.WriteString("Master");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "parentIndices");
                                        var parentIndices = skeleton.GetParentIndices();
                                        writer.WriteAttributeString("numelements", $"{parentIndices.Count}");
                                        writer.WriteString(string.Join(" ", parentIndices));
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "bones");
                                        writer.WriteAttributeString("numelements", $"{skeleton.HkxSkeleton.Count}");
                                        foreach (var b in skeleton.HkxSkeleton)
                                        {
                                            writer.WriteStartElement("hkobject");
                                            {
                                                writer.WriteStartElement("hkparam");
                                                {
                                                    writer.WriteAttributeString("name", "name");
                                                    writer.WriteString(b.Name);
                                                }
                                                writer.WriteEndElement();

                                                writer.WriteStartElement("hkparam");
                                                {
                                                    writer.WriteAttributeString("name", "lockTranslation");
                                                    writer.WriteString("false");
                                                }
                                                writer.WriteEndElement();
                                            }
                                            writer.WriteEndElement();
                                        }
                                    }
                                    writer.WriteEndElement();


                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "referencePose");
                                        writer.WriteAttributeString("numelements", $"{skeleton.HkxSkeleton.Count}");
                                        var sb = new StringBuilder();
                                        foreach (var b in skeleton.HkxSkeleton)
                                        {
                                            sb.Append("\n\t\t\t\t");
                                            var tr = b.RelativeReferenceTransform;
                                            sb.Append($"({tr.Translation.X:0.000000} " +
                                                $"{tr.Translation.Y:0.000000} " +
                                                $"{tr.Translation.Z:0.000000})");
                                            sb.Append($"({tr.Rotation.X:0.000000} " +
                                                $"{tr.Rotation.Y:0.000000} " +
                                                $"{tr.Rotation.Z:0.000000} " +
                                                $"{tr.Rotation.W:0.000000})");
                                            sb.Append($"({tr.Scale.X:0.000000} " +
                                                $"{tr.Scale.Y:0.000000} " +
                                                $"{tr.Scale.Z:0.000000})");
                                        }
                                        sb.Append("\n\t\t\t");
                                        writer.WriteString(sb.ToString());
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "referenceFloats");
                                        writer.WriteAttributeString("numelements", "0");
                                        writer.WriteString("");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "floatSlots");
                                        writer.WriteAttributeString("numelements", "0");
                                        writer.WriteString("");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "localFrames");
                                        writer.WriteAttributeString("numelements", "0");
                                        writer.WriteString("");
                                    }
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();

                                writer.WriteStartElement("hkobject");
                                {
                                    writer.WriteAttributeString("name", "#0040");
                                    writer.WriteAttributeString("class", "hkaAnimationContainer");
                                    writer.WriteAttributeString("signature", "0x8dc20333");

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "skeletons");
                                        writer.WriteAttributeString("numelements", "1");
                                        writer.WriteString("#0039");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "animations");
                                        writer.WriteAttributeString("numelements", "0");
                                        writer.WriteString("");
                                    }
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("hkparam");
                                    {
                                        writer.WriteAttributeString("name", "bindings");
                                        writer.WriteAttributeString("numelements", "0");
                                        writer.WriteString("");
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
                                    writer.WriteAttributeString("name", "#0038");
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
                                                writer.WriteString("#0040");
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

                return testStream.ToArray();
            }


        }

        public byte[] ConvertHavok2010XmlToPackfile(byte[] xmlBytes)
        {
            var newGuid = Guid.NewGuid().ToString();

            var applicationDir = Main.Directory;

            string uncompressed = $@"{applicationDir}\SapResources\CompressAnim\{newGuid}.uncompressed.xml";
            string compressed = $@"{applicationDir}\SapResources\CompressAnim\{newGuid}.compressed.hkx";

            File.WriteAllBytes(uncompressed, xmlBytes);

            var rotationQuantizationType = SoulsAssetPipeline.Animation.SplineCompressedAnimation.RotationQuantizationType.THREECOMP40;
            float rotationTolerance = 0.001f;

            string args = $"\"{Path.GetFileName(uncompressed)}\" \"{Path.GetFileName(compressed)}\" {((int)rotationQuantizationType)} {rotationTolerance}";

            var xx = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo($@"{applicationDir}\SapResources\CompressAnim\CompressAnim.exe")
            {
                Arguments = args,
                CreateNoWindow = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                WorkingDirectory = $@"{applicationDir}\SapResources\CompressAnim",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });
            xx.WaitForExit();

            string standardOutput = xx.StandardOutput.ReadToEnd();
            string standardError = xx.StandardError.ReadToEnd();

            byte[] compressedHkx = File.ReadAllBytes(compressed);

            if (File.Exists(uncompressed))
                File.Delete(uncompressed);

            if (File.Exists(compressed))
                File.Delete(compressed);

            return compressedHkx;
        }

        public byte[] ExportHKX(NewAnimationContainer.AnimHkxInfo hkxInfo, HKX skeletonHKX, ExportAnimsFileType fileType, out bool userRequestCancel, string hkxNameForErrorMsg)
        {
            var hkx = NewAnimationContainer.GetHkxStructOfAnim(hkxInfo.HkxBytes, hkxInfo.CompendiumBytes);
            var result = ExportHKX(hkx, skeletonHKX, fileType, out bool reqCancel, hkxNameForErrorMsg);
            userRequestCancel = reqCancel;
            return result;
        }

        public byte[] ExportHKX(HKX hkx, HKX skeletonHKX, ExportAnimsFileType fileType, out bool userRequestCancel, string hkxNameForErrorMsg)
        {
            bool requestCancel = false;
            byte[] result = null;
            try
            {
                if (fileType == ExportAnimsFileType.Havok2010_2_Packfile_x32)
                {
                    var xmlBytes = HavokXmlWriter.WriteHavokPackfileToXML(hkx, skeletonHKX);
                    result = ConvertHavok2010XmlToPackfile(xmlBytes);
                }
                else if (fileType == ExportAnimsFileType.Havok2010_2_XML)
                {
                    result = HavokXmlWriter.WriteHavokPackfileToXML(hkx, skeletonHKX);
                }
                //else if (fileType == ExportAnimsFileType.Havok2014_1_Packfile_x64)
                //{
                //    //var xmlBytes = writeHkxSkeletonTo2010Xml(skeleton);
                //    //result = ConvertHavok2010XmlToPackfile(xmlBytes);
                //    skeleton.Variation = HKX.HKXVariation.HKXDS3;
                //    result = skeleton.Write();
                //}
                else if (fileType == ExportAnimsFileType.Havok2016_1_Tagfile_x64)
                {
                    var xmlBytes = HavokXmlWriter.WriteHavokPackfileToXML(hkx, skeletonHKX);
                    var hkx2010Bytes = ConvertHavok2010XmlToPackfile(xmlBytes);
                    result = HavokDowngrade.UpgradeHkx2010to2015(hkx2010Bytes);
                }
            }
            catch (Exception ex)
            {
                var dlgRes = System.Windows.Forms.MessageBox.Show($"Failed to export HKX file '{hkxNameForErrorMsg}'.\nWould you like to continue anyways?\n\n\nError shown below:\n\n{ex}",
                    "Continue With Errors?", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
                requestCancel = (dlgRes == System.Windows.Forms.DialogResult.No);
            }
            userRequestCancel = requestCancel;
            return result;
        }

        //public byte[] ExportSkeleton(HKX skeleton, ExportAnimsFileType fileType, out bool userRequestCancel)
        //{
        //    bool requestCancel = false;
        //    byte[] result = null;
        //    try
        //    {
        //        if (fileType == ExportAnimsFileType.Havok2010_2_Packfile_x32)
        //        {
        //            var xmlBytes = HavokXmlWriter.WriteHavokPackfileToXML(skeleton);
        //            result = ConvertHavok2010XmlToPackfile(xmlBytes);
        //        }
        //        else if (fileType == ExportAnimsFileType.Havok2010_2_XML)
        //        {
        //            result = HavokXmlWriter.WriteHavokPackfileToXML(skeleton);
        //        }
        //        //else if (fileType == ExportAnimsFileType.Havok2014_1_Packfile_x64)
        //        //{
        //        //    //var xmlBytes = writeHkxSkeletonTo2010Xml(skeleton);
        //        //    //result = ConvertHavok2010XmlToPackfile(xmlBytes);
        //        //    skeleton.Variation = HKX.HKXVariation.HKXDS3;
        //        //    result = skeleton.Write();
        //        //}
        //        else if (fileType == ExportAnimsFileType.Havok2016_1_Tagfile_x64)
        //        {
        //            var xmlBytes = HavokXmlWriter.WriteHavokPackfileToXML(skeleton);
        //            var hkx2010Bytes = ConvertHavok2010XmlToPackfile(xmlBytes);
        //            result = HavokDowngrade.UpgradeHkx2010to2015(hkx2010Bytes);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var dlgRes = System.Windows.Forms.MessageBox.Show($"Failed to export skeleton.\nWould you like to continue anyways?\n\n\nError shown below:\n\n{ex}", 
        //            "Continue With Errors?", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
        //        requestCancel = (dlgRes == System.Windows.Forms.DialogResult.No);
        //    }
        //    userRequestCancel = requestCancel;
        //    return result;
        //}



        //public byte[] ExportAnim(NewAnimationContainer.AnimHkxInfo animHkxInfo, ExportAnimsFileType fileType, out bool userRequestCancel, string fileNameForError)
        //{
        //    bool requestCancel = false;
        //    byte[] result = null;
        //    try
        //    {
        //        var hkx = NewAnimationContainer.GetHkxStructOfAnim(animHkxInfo.AnimBytes, animHkxInfo.CompendiumBytes);
                

        //        if (fileType == ExportAnimsFileType.Havok2010_2_XML)
        //        {
        //            var xml = HavokXmlWriter.WriteHavokPackfileToXML(hkx);
        //            result = xml;
        //        }
        //        else if (fileType == ExportAnimsFileType.Havok2010_2_Packfile_x32)
        //        {
        //            if (GameRoot.GameTypeIsHavokTagfile)
        //            {
        //                var xml = HavokXmlWriter.WriteHavokPackfileToXML(hkx);
        //                result = HavokXmlWriter.ConvertHavokXmlToPackfile(xml);
        //            }
        //            else
        //            {
        //                //TODO: Test
        //                hkx.Variation = HKX.HKXVariation.HKXDS1;
        //                result = hkx.Write();
        //            }
        //        }
        //        else if (fileType == ExportAnimsFileType.Havok2016_1_Tagfile_x64)
        //        {
        //            byte[] hkx2010Bytes = null;
        //            if (GameRoot.GameTypeIsHavokTagfile)
        //            {
        //                var xml = HavokXmlWriter.WriteHavokPackfileToXML(hkx);
        //                hkx2010Bytes = HavokXmlWriter.ConvertHavokXmlToPackfile(xml);
        //            }
        //            else
        //            {
        //                //TODO: Test
        //                hkx.Variation = HKX.HKXVariation.HKXDS1;
        //                hkx2010Bytes = hkx.Write();
        //            }
        //            result = HavokDowngrade.UpgradeHkx2010to2015(hkx2010Bytes);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        var dlgRes = System.Windows.Forms.MessageBox.Show($"Failed to export animation '{fileNameForError}'.\nWould you like to continue anyways?" +
        //            $"\n\n\nError shown below:\n\n{ex}", "Continue With Errors?", 
        //            System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
        //        requestCancel = (dlgRes == System.Windows.Forms.DialogResult.No);
        //    }
        //    userRequestCancel = requestCancel;
        //    return result;
        //}
    
    
    }
}
