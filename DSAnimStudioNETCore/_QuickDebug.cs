using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using SoulsAssetPipeline.Animation.SIBCAM;
using SoulsAssetPipeline.FLVERImporting;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMatrix = System.Numerics.Matrix4x4;
using NVector2 = System.Numerics.Vector2;
using NVector3 = System.Numerics.Vector3;
using NVector4 = System.Numerics.Vector4;
using NQuaternion = System.Numerics.Quaternion;
using DSAnimStudio.TaeEditor;
using DSAnimStudio.ImguiOSD;
using NAudio.Wave;
using System.Windows.Forms;
using NAudio.Gui;
using FSBANKLOL;
using System.Runtime.InteropServices;
using static SoulsAssetPipeline.Audio.Wwise.WwiseBlock;

namespace DSAnimStudio
{
    public static class _QuickDebug
    {

        public static void BuildDebugMenu(TaeEditorScreen mainScreen)
        {
            var mainDoc = mainScreen.ParentDocument;
            var mainProj = mainScreen.Proj;

            if (DebugTestButton("Load Default ImGui INI - V5"))
            {
                OSD.RequestLoadDefaultLayout = OSD.DefaultLayoutTypes.V5;
            }

            if (DebugTestButton("Load Default ImGui INI - Legacy"))
            {
                OSD.RequestLoadDefaultLayout = OSD.DefaultLayoutTypes.Legacy;
            }

            if (DebugTestButton("Wwise error testing"))
            {
                try
                {
                    var bnkBytes = mainDoc.GameData.ReadWwiseSoundFile($"/enus/cs_main.bnk");
                    var bnk = SoulsAssetPipeline.Audio.Wwise.WwiseBNK.Read(bnkBytes);
                    var test = bnk.HIRC.LoadObjectDynamic(0xba919146);
                    Console.WriteLine("test");
                }
                catch
                {

                }
            }

            if (DebugTestButton("Clear animation cache"))
            {
                var animContainer = mainDoc?.EditorScreen?.Graph?.ViewportInteractor?.CurrentModel?.AnimContainer;
                if (animContainer != null)
                {
                    animContainer.ClearAnimationCache();
                }
            }

            if (DebugTestButton("SIBCAM2 Test"))
            {
                var sibcam2 = SIBCAM2.Read(@"C:\Users\Green\Downloads\rpcs3-v0.0.29-15434-744a1528_win64\dev_hdd0\game\NPUB30910\USRDIR\other\default-rumblebnd\cam_data\eventcam\camera_151_flame_naguri2.sibcam");

                Console.WriteLine("test");
            }


            if (DebugTestButton("Clear sibcam"))
            {
                mainDoc.RumbleCamManager.ClearAll();
            }

            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();

            if (DebugTestButton("FSBANK test"))
            {
                string outputFileName = @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600.fsb";

                string[] sounds = {
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008001b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008001c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008001d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008002.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008003.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008003b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008003c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008003d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008004.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008004b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008005.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008005b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008006.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008007.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008008.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008009.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008009b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008009c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008009d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008010.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008010b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008010c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008010d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008011.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008012.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008012b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008013.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008014.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001001.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001001b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001001c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001001d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001004.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001004b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001004c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001012.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001012b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001012c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001012d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001018.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001018b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001018c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001028.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001028b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001028c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001028d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001030.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001030b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001030c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001031.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001031b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260001031c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260002001.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260002001b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260002001c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260002001d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260003001.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260003004.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260003012.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260003018.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260003028.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260003030.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260003031.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260003100.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004000.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004100.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004200.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004500.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004500b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004500c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004500d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004500e.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004501.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004501b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004501c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004501d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004502.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004502b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004502c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260004502d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260005001.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260005001b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260005001c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260005001d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260005001e.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260005001f.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260005001g.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260005001h.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260005002.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260005002b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260005002c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260005002d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006000.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006000b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006000c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006500.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006500b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006500c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006500d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006501.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006501b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006501c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006502.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006502b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260006502c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260007000.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260007000b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260007000c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260007000d.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260007001.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260007002.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260007500.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260007500b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008000.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008000b.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008000c.wav",
                    @"C:\PS4\CUSA03173-app\dvdroot_ps4\sound_win\sprj_c2600\c260008001.wav"
                };

                var soundDatas = sounds.Select(x => File.ReadAllBytes(x)).ToArray();

                FSBANK.FSBank_Init(FSBVERSION.FSB5, INITFLAGS.INIT_NORMAL, numSimultaneousJobs: 8, IntPtr.Zero);

                Console.WriteLine("fatcat");

                var bw = new BinaryWriterEx(false);

                List<long> fixupLocations = new List<long>();

                for (int i = 0; i < sounds.Length; i++)
                {
                    fixupLocations.Add(bw.Position);
                    bw.ReserveInt64($"Sound[{i}].Name"); //fileNames
                    fixupLocations.Add(bw.Position); 
                    bw.ReserveInt64($"Sound[{i}].Data"); //fileData
                    bw.WriteInt32(soundDatas[i].Length); //fileDataLengths
                    bw.WriteInt32(soundDatas.Length); //numFiles
                    bw.WriteInt32((int)BUILDFLAGS.BUILD_DEFAULT); //overrideFlags
                    bw.WriteInt32(100); // overrideQuality
                    bw.WriteSingle(0); // desiredSampleRate
                    bw.WriteSingle(0); // percentOptimizedRate
                }

                for (int i = 0; i < sounds.Length; i++)
                {
                    bw.FillInt64($"Sound[{i}].Name", bw.Position);
                    bw.WriteASCII(sounds[i], terminate: true);
                }

                for (int i = 0; i < sounds.Length; i++)
                {
                    bw.Pad(0x10);
                    bw.FillInt64($"Sound[{i}].Data", bw.Position);
                    bw.WriteBytes(soundDatas[i]);
                }

                bw.Pad(0x10);
                var localOffsetOfOutputNameWithinDataBlock = bw.Position;
                bw.WriteASCII(outputFileName, terminate: true);

                var totalDataBlockLength = (int)bw.Position;

                IntPtr dataBlockPtr = Marshal.AllocHGlobal(totalDataBlockLength);

                try
                {
                    var br = new BinaryReaderEx(false, bw.Stream);
                    foreach (var pos in fixupLocations)
                    {
                        var value = br.GetInt64(pos);
                        value += dataBlockPtr.ToInt64();
                        bw.Position = pos;
                        bw.WriteInt64(value);
                    }
                    var finalDataBlockBytes = bw.FinishBytes();
                    Marshal.Copy(finalDataBlockBytes, 0, dataBlockPtr, finalDataBlockBytes.Length);

                    var result = FSBANK.FSBank_Build(dataBlockPtr, (uint)sounds.Length, FORMAT.FSBANK_FORMAT_VORBIS, BUILDFLAGS.BUILD_DEFAULT, quality: 100, encryptKey: IntPtr.Zero, (IntPtr)(localOffsetOfOutputNameWithinDataBlock + dataBlockPtr.ToInt64()));

                    Console.WriteLine("fatcat");
                }
                finally
                {
                    Marshal.FreeHGlobal(dataBlockPtr);
                }
            }


           
            //for (int i = 0; i < 30; i++)
            //{
            //    ImGui.Separator();
            //}
            if (DebugTestButton("Scan for unmapped action param bytes"))
            {
                const int maxExampleCount = 100;

                Dictionary<int, int> actionByteCounts = new Dictionary<int, int>();
                Dictionary<int, List<(SplitAnimID animID, DSAProj.Action action)>> sussyActionExamples = new();
                Dictionary<int, int> lowestActByteCounts = new();

                foreach (var act in mainProj.Template)
                {
                    actionByteCounts[(int)act.Key] = act.Value.GetAllParametersByteCount();
                    sussyActionExamples[(int)act.Key] = new();
                    lowestActByteCounts[(int)act.Key] = int.MaxValue;
                }

                mainProj.SafeAccessAnimCategoriesList(categories =>
                {
                    foreach (var cat in categories)
                    {
                        cat.UnSafeAccessAnimations(animations =>
                        {
                            foreach (var anim in animations)
                            {
                                anim.UnSafeAccessActions(actions =>
                                {
                                    foreach (var act in actions)
                                    {
                                        if (act.OriginalParamBytesLength < lowestActByteCounts[act.Type])
                                            lowestActByteCounts[act.Type] = act.OriginalParamBytesLength;

                                        var correspondingExampleList = sussyActionExamples[act.Type];
                                        if (correspondingExampleList.Count < maxExampleCount)
                                        {
                                            if (act.OriginalParamBytesLength > actionByteCounts[act.Type])
                                            {
                                                correspondingExampleList.Add((anim.SplitID, act));
                                            }
                                        }
                                    }
                                });
                            }
                        });
                    }
                });

                var actualSus = sussyActionExamples.Where(kvp => kvp.Value.Count > 0).ToList();

                var testOtherSus = actualSus.Select(x => (x.Key, x.Value.Count, x.Value.First().animID)).ToList();



                var extremelySus = actionByteCounts.Where(kvp => kvp.Value < lowestActByteCounts[kvp.Key]).Select(x => (x.Key, x.Value, lowestActByteCounts[x.Key])).ToList();

                var extremelySus_Tested = extremelySus.Where(x => x.Item3 < int.MaxValue).ToList();
                var extremelySus_Untested = extremelySus.Where(x => x.Item3 == int.MaxValue).ToList();

                Console.WriteLine("holy fuck");
            }


            if (DebugTestButton("Scan ac6 weapon meme"))
            {
                var sb = new StringBuilder();
                Dictionary<int, List<string>> memes = new Dictionary<int, List<string>>();
                var partsbnds = zzz_DocumentManager.CurrentDocument.GameData.GetFilesInDir("/parts", zzz_GameDataIns.SearchType.AllFiles);
                foreach (var file in partsbnds)
                {
                    if (file.EndsWith("_l.partsbnd.dcx"))
                        continue;
                    if (file.EndsWith("_u.partsbnd.dcx"))
                        continue;

                    var shortName = Utils.GetShortIngameFileName(file);

                    if (shortName.StartsWith("wp_e"))
                        continue;
                    if (!shortName.StartsWith("wp_"))
                        continue;

                   
                    if (int.TryParse(shortName.Substring(shortName.Length - 4, 4), out int parsedInt))
                    {
                        int partID = parsedInt;
                        if (!memes.ContainsKey(partID))
                            memes[partID] = new List<string>();

                        memes[partID].Add(file);

                        if (file.EndsWith(".tpf.dcx"))
                            continue;

                        if (BND4.IsRead(zzz_DocumentManager.CurrentDocument.GameData.ReadFile(file), out BND4 asBND4))
                        {
                            foreach (var bf in asBND4.Files)
                            {
                                memes[partID].Add($"  [{bf.ID}]{bf.Name}");
                                if (bf.ID >= 400 && bf.ID < 500)
                                {
                                    IBinder anibnd = null;
                                    if (BND3.IsRead(bf.Bytes, out BND3 anibndAsBND3))
                                        anibnd = anibndAsBND3;
                                    else if (BND4.IsRead(bf.Bytes, out BND4 anibndAsBND4))
                                        anibnd = anibndAsBND4;
                                    foreach (var af in anibnd.Files)
                                    {
                                        memes[partID].Add($"    [{af.ID}]{af.Name}");
                                        if (af.ID == 3000000 || af.Name.EndsWith(".tae"))
                                        {
                                            var tae = TAE.Read(af.Bytes);
                                            foreach (var a in tae.Animations)
                                            {
                                                if (a.Actions.Count > 0)
                                                    memes[partID].Add($"      Anim{a.ID} - has actions");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                    }
                }

                var memeKeys = memes.Keys.OrderBy(k => k).ToList();

                foreach (var key in memeKeys)
                {
                    foreach (var fuck in memes[key])
                    {
                        sb.AppendLine(fuck);
                    }
                    sb.AppendLine();
                    sb.AppendLine();
                }

                Clipboard.SetText( sb.ToString() );
                Console.WriteLine("bruh");
            }

            if (DebugTestButton("Scan ac6 partsbnd"))
            {
                

                List<int> binderIDs_insidePartsbnd = new List<int>();
                List<int> binderIDs_insideAnibnd = new List<int>();
                var sb = new StringBuilder();
                var partsbnds = zzz_DocumentManager.CurrentDocument.GameData.GetFilesInDir("/parts", zzz_GameDataIns.SearchType.AllFiles);
                foreach (var file in partsbnds)
                {
                    if (file.EndsWith(".partsbnd.dcx"))
                    {
                        sb.AppendLine(file);
                        var bnd = BND4.Read(zzz_DocumentManager.CurrentDocument.GameData.ReadFile(file));
                        foreach (var bf in bnd.Files)
                        {
                            if (!binderIDs_insidePartsbnd.Contains(bf.ID))
                                binderIDs_insidePartsbnd.Add(bf.ID);
                            sb.AppendLine($"  [{bf.ID}]{bf.Name}");
                            if (bf.ID >= 400 && bf.ID < 500)
                            {
                                IBinder anibnd = null;
                                if (BND3.IsRead(bf.Bytes, out BND3 anibndAsBND3))
                                    anibnd = anibndAsBND3;
                                else if (BND4.IsRead(bf.Bytes, out BND4 anibndAsBND4))
                                    anibnd = anibndAsBND4;
                                foreach (var af in anibnd.Files)
                                {
                                    if (!binderIDs_insideAnibnd.Contains(af.ID))
                                        binderIDs_insideAnibnd.Add(af.ID);
                                    sb.AppendLine($"    [{af.ID}]{af.Name}");
                                    if (af.ID == 3000000 || af.Name.EndsWith(".tae"))
                                    {
                                        var tae = TAE.Read(af.Bytes);
                                        foreach (var a in tae.Animations)
                                        {
                                            if (a.Actions.Count > 0)
                                                sb.AppendLine($"      Anim{a.ID} - has events");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                binderIDs_insidePartsbnd = binderIDs_insidePartsbnd.OrderBy(x => x).ToList();
                binderIDs_insideAnibnd = binderIDs_insideAnibnd.OrderBy(x => x).ToList();
                Clipboard.SetText( sb.ToString() );
                Console.WriteLine("bruh");
            }


            if (DebugTestButton("Test template custom write"))
            {
                zzz_DocumentManager.CurrentDocument.EditorScreen.SaveCustomXMLTemplate();
                Console.WriteLine("fatcat");
            }

                // Dark Souls: Nightfall tests removed for public version lel
                if (DebugTestButton("Copy animation to clipboard"))
            {
                var currentAnimBytes = zzz_DocumentManager.CurrentDocument.EditorScreen.Graph.AnimRef.SerializeToBytes(zzz_DocumentManager.CurrentDocument.EditorScreen.Proj);
                string animBytesStr = string.Join(" ", currentAnimBytes.Select(x => x.ToString("X2")));
                Clipboard.SetText(animBytesStr);
            }

            if (DebugTestButton("ds1 param scan"))
            {
                StringBuilder sb = new StringBuilder();

                var scanStr = "3290";
                var parambndName = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\param\GameParam\GameParam.parambnd";

                var bnd = BND3.Read(parambndName);
                foreach (var f in bnd.Files)
                {
                    var pName = Utils.GetShortIngameFileName(f.Name);
                    var p = PARAM.Read(f.Bytes);
                    foreach (var r in p.Rows)
                    {
                        if (r.Name != null && r.Name.Contains(scanStr))
                        {
                            sb.AppendLine($"{pName} {r.ID} {r.Name}");
                        }
                    }
                }

                Clipboard.SetText(sb.ToString());

                Console.WriteLine("fatcat");
            }
        }

        private enum DebugValType
        {
            Int,
            Float,
            Bool,
            String
        };

        private static Dictionary<string, int> debugValFields_Int = new Dictionary<string, int>();
        private static Dictionary<string, float> debugValFields_Float = new Dictionary<string, float>();
        private static Dictionary<string, bool> debugValFields_Bool = new Dictionary<string, bool>();
        private static Dictionary<string, string> debugValFields_String = new Dictionary<string, string>();

        private static int UpdateInt(string key, string dispName, int defaultValue = 0)
        {
            var dict = debugValFields_Int;

            if (!dict.ContainsKey(key))
                dict.Add(key, defaultValue);

            var curVal = dict[key];
            ImGui.InputInt($"{dispName}##{key}", ref curVal);
            dict[key] = curVal;

            return curVal;
        }

        private static float UpdateFloat(string key, string dispName, float defaultValue = 0)
        {
            var dict = debugValFields_Float;

            if (!dict.ContainsKey(key))
                dict.Add(key, defaultValue);

            var curVal = dict[key];
            ImGui.InputFloat($"{dispName}##{key}", ref curVal);
            dict[key] = curVal;

            return curVal;
        }

        private static bool UpdateBool(string key, string dispName, bool defaultValue = false)
        {
            var dict = debugValFields_Bool;

            if (!dict.ContainsKey(key))
                dict.Add(key, defaultValue);

            var curVal = dict[key];
            ImGui.Checkbox($"{dispName}##{key}", ref curVal);
            dict[key] = curVal;

            return curVal;
        }

        private static string UpdateString(string key, string dispName, string defaultValue = "", uint maxStrLength = 128)
        {
            var dict = debugValFields_String;

            if (!dict.ContainsKey(key))
                dict.Add(key, defaultValue);

            var curVal = dict[key];
            ImGui.InputText($"{dispName}##{key}", ref curVal, maxStrLength);
            dict[key] = curVal;

            return curVal;
        }

        private static int GetInt(string key)
        {
            var dict = debugValFields_Int;
            return dict.ContainsKey(key) ? dict[key] : 0;
        }

        private static float GetFloat(string key)
        {
            var dict = debugValFields_Float;
            return dict.ContainsKey(key) ? dict[key] : 0;
        }

        private static bool GetBool(string key)
        {
            var dict = debugValFields_Bool;
            return dict.ContainsKey(key) ? dict[key] : false;
        }

        private static string GetString(string key)
        {
            var dict = debugValFields_String;
            return dict.ContainsKey(key) ? dict[key] : "";
        }

        private static bool DebugTestButton(string name)
        {
            ImGui.Button("TEST: " + name);
            return ImGui.IsItemClicked();
        }
    }
}
