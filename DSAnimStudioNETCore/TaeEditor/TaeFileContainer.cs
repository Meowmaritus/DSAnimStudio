using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using static DSAnimStudio.DSAProj;
using static DSAnimStudio.ManagerAction;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Text;
using TAE = SoulsAssetPipeline.Animation.TAE;

namespace DSAnimStudio.TaeEditor
{
    public class TaeFileContainer
    {
        public void ShowPopupForDSAProjRuntimeFlags(DSAProj.EditorFlags flags)
        {
            if (flags != EditorFlags.None)
            {
                var sb = new StringBuilder();
                sb.AppendLine("The file loaded has been marked as modified (with '*') for the following issues:");
                if ((flags & EditorFlags.NeedsResave_LegacySaveWithEventGroupsStripped) != 0)
                    sb.AppendLine($"    -It has animation entries which were mistakenly saved with event groups enabled in a game that doesn't support them.");
                if ((flags & EditorFlags.NeedsResave_DSAProjVersionOutdated) != 0)
                    sb.AppendLine($"    -The *{DSAProj.EXT} file was saved with an older version of DS Anim Studio.");
                if ((flags & EditorFlags.NeedsResave_NullAnimNameBug) != 0)
                    sb.AppendLine($"    -The *{DSAProj.EXT} file was saved with a bug that caused all anim file names " +
                                  $"to get wiped (anim file names have been loaded from the *.dsasbak file for any null file names).");
                sb.AppendLine("\nIt is reccomended that you resave the file now to apply the fixes for the above issues.");
                MessageBox.Show(sb.ToString(),
                                "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private readonly TaeEditorScreen Editor;

        //public enum TaeFileContainerType
        //{
        //    TAE,
        //    ANIBND,
        //    OBJBND,
        //    REMOBND
        //}

        public DSAProj Proj;
        
        

        public TaeFileContainer(TaeEditorScreen editor)
        {
            Editor = editor;
        }

        //private string filePath
        //{
        //    get => Editor.FileContainerName;
        //    set => Editor.FileContainerName = value;
        //}

        //public TaeFileContainerType ContainerType { get; private set; }

        private IBinder containerBinder;
        private IBinder containerBinder_2010;
        private bool containerBinder_2010_IsModified = false;

        //private IBinder containerOBJBND;

        //private string anibndPathInsideObjbnd = null;

        public TaeContainerInfo Info = null;

        public SoulsAssetPipeline.Animation.TAE.Template TaeTemplate = null;

        public List<DSAProj.AnimCategory> AllTAE => Proj?.SAFE_GetAllAnimCategories() ?? new List<DSAProj.AnimCategory>();
        private Dictionary<SplitAnimID, byte[]> hkxInBND = new Dictionary<SplitAnimID, byte[]>();
        private Dictionary<SplitAnimID, byte[]> sibcamInBND = new Dictionary<SplitAnimID, byte[]>();

        private void DeleteAnimHKX(SplitAnimID id)
        {
            //if (ContainerType != TaeFileContainerType.ANIBND)
            //    throw new NotImplementedException("Not supported for anything other than ANIBND right now.");

            throw new NotImplementedException();
        }

        public void AddNewHKX(SplitAnimID id, string name, byte[] data, out byte[] dataForAnimContainer)//, byte[] predeterminedDataHavok2010 = null)
        {
            throw new NotImplementedException();

            if (data == null)
            {
                dataForAnimContainer = null;
                return;
            }

            //if (ContainerType != TaeFileContainerType.ANIBND)
            //    throw new NotImplementedException("Not supported for anything other than ANIBND right now.");

            //dataForAnimContainer = data;

            //if (ContainerType == TaeFileContainerType.ANIBND)
            //{
            //    DeleteAnimHKX(name + ".hkx");

            //    hkxInBND.Add(name + ".hkx", data);
            //    BinderFile f = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", data);
            //    containerBinder.Files.Add(f);
            //    containerBinder.Files = containerBinder.Files.OrderBy(bf => bf.ID).ToList();
            //    IsModified = true;
            

            //    //if (GameDataManager.GameTypeIsHavokTagfile)
            //    //{
            //    //    if (HavokDowngrade.SimpleCheckIfHkxBytesAre2015(data))
            //    //    {
            //    //        BinderFile f = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", data);
            //    //        containerANIBND.Files.Add(f);
            //    //        containerANIBND.Files = containerANIBND.Files.OrderBy(bf => bf.ID).ToList();


            //    //        byte[] bytes2010 = predeterminedDataHavok2010 ?? HavokDowngrade.DowngradeSingleFileInANIBND(containerANIBND, f, isUpgrade: false);
            //    //        BinderFile f2010 = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", bytes2010);
            //    //        containerANIBND_2010.Files.Add(f2010);
            //    //        containerANIBND_2010.Files = containerANIBND_2010.Files.OrderBy(bf => bf.ID).ToList();
            //    //        containerANIBND_2010_IsModified = true;
            //    //        hkxInBND.Add(name + ".hkx", bytes2010);

            //    //        dataForAnimContainer = bytes2010;

            //    //        IsModified = true;
            //    //    }
            //    //    else
            //    //    {
            //    //        hkxInBND.Add(name + ".hkx", data);

            //    //        BinderFile f2010 = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", data);
            //    //        containerANIBND_2010.Files.Add(f2010);
            //    //        containerANIBND_2010.Files = containerANIBND_2010.Files.OrderBy(bf => bf.ID).ToList();
            //    //        containerANIBND_2010_IsModified = true;

            //    //        byte[] bytes2015 = HavokDowngrade.DowngradeSingleFileInANIBND(containerANIBND_2010, f2010, isUpgrade: true);

            //    //        BinderFile f = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", bytes2015);
            //    //        containerANIBND.Files.Add(f);
            //    //        containerANIBND.Files = containerANIBND.Files.OrderBy(bf => bf.ID).ToList();
            //    //        IsModified = true;
            //    //    }

            //    //}
            //    //else
            //    //{
            //    //    hkxInBND.Add(name + ".hkx", data);
            //    //    BinderFile f = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", data);
            //    //    containerANIBND.Files.Add(f);
            //    //    containerANIBND.Files = containerANIBND.Files.OrderBy(bf => bf.ID).ToList();
            //    //    IsModified = true;
            //    //}
            //}
            
        }

        public List<BinderFile> RelatedModelFiles = new List<BinderFile>();

        public bool IsModified => Proj?.SAFE_AnyModified() ?? false;

        public bool IsCurrentlyLoading { get; private set; } = false;

        public static readonly string DefaultSaveFilter =
            "Anim Container (*.ANIBND[.DCX]) |*.ANIBND*|" +
            //"Cutscene Container (*.REMOBND[.DCX]) |*.REMOBND*|" +
            "Object Container (*.OBJBND[.DCX]) |*.OBJBND*|" +
            "Parts Container (*.PARTSBND[.DCX]) |*.PARTSBND*|" +
            //"Loose TimeAct File (*.TAE) |*.TAE|" +
            "All Files|*.*";

        public static readonly string DefaultSaveFilter_Model =
            "Character Model Container (*.CHRBND[.DCX]) |*.CHRBND*|" +
            "Object Model Container (*.OBJBND[.DCX]) |*.OBJBND*|" +
            //"Loose FLVER Model File (*.FLVER) |*.FLVER|" +
            "All Files|*.*";

        public bool IsBloodborne => zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB;

        public string GetResaveFilter()
        {
            return DefaultSaveFilter;
        }

        //public Dictionary<int, TAE> StandardTAE { get; private set; } = null;
        //public Dictionary<int, TAE> PlayerTAE { get; private set; } = null;

        private object _lock_loading = new object();

        // public IReadOnlyDictionary<SplitAnimID, byte[]> AllHKXDict
        // {
        //     get
        //     {
        //         Dictionary<SplitAnimID, byte[]> result = null;
        //         lock (_lock_loading)
        //         {
        //             result = hkxInBND.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        //         }
        //         return result;
        //     }
        // }

        // public IReadOnlyDictionary<string, byte[]> AllSibcamDict
        // {
        //     get
        //     {
        //         Dictionary<string, byte[]> result = null;
        //         lock (_lock_loading)
        //         {
        //             result = sibcamInBND.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        //         }
        //         return result;
        //     }
        // }

        
        

        

        

        

        

        public static void CheckGameVersionForTaeInterop(string binderPath, IBinder binder)
        {
            if (binder != null)
            {
                var firstFile = binder.Files.FirstOrDefault();
                if (firstFile != null)
                {
                    CheckGameVersionForTaeInterop(binderPath, binder, firstFile.Name, false);
                }
            }
            //zzz_DocumentManager.CurrentDocument.GameRoot.InitializeFromBND(binderPath, binder);
        }

        public static void CheckGameVersionForTaeInterop(string binderPath, IBinder binder, string internalFilePath, bool isRemo)
        {
            var check = internalFilePath.ToUpper();
            string scratchFolder = zzz_DocumentManager.CurrentDocument.GameRoot.GetParentDirectory(binderPath);
            if (check.Contains("FRPG2"))
            {
                zzz_DocumentManager.CurrentDocument.GameRoot.Init(binderPath, SoulsAssetPipeline.SoulsGames.DS2SOTFS, scratchFolder);
            }
            else if (check.Contains(@"\FRPG\") && (check.Contains(@"INTERROOT_X64") || check.Contains(@"HKXX64") || (check.Contains(@"TAENEW\X64")) && isRemo))
            {
                zzz_DocumentManager.CurrentDocument.GameRoot.Init(binderPath, SoulsAssetPipeline.SoulsGames.DS1R, scratchFolder);
            }
            else if (check.Contains(@"\FRPG\") && (check.Contains(@"INTERROOT_WIN32") || check.Contains(@"HKXWIN32") || (check.Contains(@"TAENEW\WIN32")) && isRemo))
            {
                zzz_DocumentManager.CurrentDocument.GameRoot.Init(binderPath, SoulsAssetPipeline.SoulsGames.DS1, scratchFolder);
            }
            else if (check.Contains(@"\SPRJ\"))
            {
                zzz_DocumentManager.CurrentDocument.GameRoot.Init(binderPath, SoulsAssetPipeline.SoulsGames.BB, scratchFolder);
            }
            else if (check.Contains(@"\FDP\"))
            {
                zzz_DocumentManager.CurrentDocument.GameRoot.Init(binderPath, SoulsAssetPipeline.SoulsGames.DS3, scratchFolder);
            }
            else if (check.Contains(@"\DEMONSSOUL\"))
            {
                //scratchFolder = GameRoot.GetParentDirectory(scratchFolder);
                zzz_DocumentManager.CurrentDocument.GameRoot.Init(binderPath, SoulsAssetPipeline.SoulsGames.DES, scratchFolder);
            }
            else if (check.Contains(@"\NTC\"))
            {
                zzz_DocumentManager.CurrentDocument.GameRoot.Init(binderPath, SoulsAssetPipeline.SoulsGames.SDT, scratchFolder);
            }
            else if (check.Contains(@"\GR\"))
            {
                zzz_DocumentManager.CurrentDocument.GameRoot.Init(binderPath, SoulsAssetPipeline.SoulsGames.ER, scratchFolder);
            }
            else if (check.Contains(@"\CL\"))
            {
                zzz_DocumentManager.CurrentDocument.GameRoot.Init(binderPath, SoulsAssetPipeline.SoulsGames.ERNR, scratchFolder);
            }
            else if (check.Contains(@"\FNR\"))
            {
                zzz_DocumentManager.CurrentDocument.GameRoot.Init(binderPath, SoulsAssetPipeline.SoulsGames.AC6, scratchFolder);
            }
        }

        private static byte[] ConvertDeSAnimation(byte[] inputBytes)
        {
            string dir = $@"{Main.Directory}\Res\DesAnims";
            string bigEndianInput = $@"{dir}\Input.hkx";
            string littleEndianUncompressed2010Output = $@"{dir}\Inputhk2010.hkx";

            File.WriteAllBytes(bigEndianInput, inputBytes);

            var procStart = new ProcessStartInfo($@"{Main.Directory}\Res\DesAnims\despacito2.exe",
                $"-d \"{bigEndianInput}\"");
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

            byte[] result = File.ReadAllBytes(littleEndianUncompressed2010Output);

            return result;
        }

        public static IBinder GenerateDemonsSoulsConvertedAnibnd(string origFilePath)
        {
            IBinder containerANIBND_2010 = null;
            if (!System.IO.File.Exists(origFilePath + ".2010"))
            {
                zzz_DocumentManager.CurrentDocument.LoadingTaskMan.DoLoadingTaskSynchronous(null, "Converting Demon's Souls Animations", innerProgress =>
                {
                    File.Copy(origFilePath, origFilePath + ".2010", overwrite: true);

                    containerANIBND_2010 = BND3.Read(origFilePath + ".2010");

                    int i = 0;
                    foreach (var f in containerANIBND_2010.Files)
                    {
                        if (f.Name.ToLower().EndsWith(".hkx"))
                        {
                            f.Bytes = ConvertDeSAnimation(f.Bytes);
                        }

                        innerProgress.Report(1.0 * (++i) / containerANIBND_2010.Files.Count);
                    }

                    ((BND3)containerANIBND_2010).Write(origFilePath + ".2010");

                    innerProgress.Report(1.0);
                });
            }
            else
            {
                containerANIBND_2010 = BND3.Read(origFilePath + ".2010");
            }

            return containerANIBND_2010;
        }

        public static void ImportOodle(string interroot)
        {
            string oodleSource = Utils.Frankenpath(interroot, "oo2core_6_win64.dll");

            // modengine check
            if (!File.Exists(oodleSource))
            {
                oodleSource = Utils.Frankenpath(interroot, @"..\oo2core_6_win64.dll");
            }

            //if (!File.Exists(oodleSource))
            //{
            //    System.Windows.Forms.MessageBox.Show("Was unable to automatically find the " +
            //    "`oo2core_6_win64.dll` file in the Sekiro folder. Please copy that file to the " +
            //    "'lib' folder next to DS Anim Studio.exe in order to load Sekiro files.", "Unable to find compression DLL",
            //    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

            //    return false;
            //}

            string oodleTarget = Utils.Frankenpath(Main.Directory, "oo2core_6_win64.dll");

            if (System.IO.File.Exists(oodleSource) && !System.IO.File.Exists(oodleTarget))
            {
                System.IO.File.Copy(oodleSource, oodleTarget, true);

                zzz_NotificationManagerIns.PushNotification("Oodle compression library was automatically copied from game directory " +
                                    "to editor's '/lib' directory and Sekiro / Elden Ring files will load now.");
            }

        }

        public static bool AnibndContainsTae(byte[] anibndFile)
        {
            bool result = false;
            IBinder bnd = null;
            if (BND3.Is(anibndFile))
                bnd = BND3.Read(anibndFile);
            else if (BND4.Is(anibndFile))
                bnd = BND4.Read(anibndFile);
            if (bnd != null)
            {
                foreach (var f in bnd.Files)
                {
                    if (f.Name.ToLowerInvariant().EndsWith(".tae") && SoulsAssetPipeline.Animation.TAE.Is(f.Bytes))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public static bool AnibndContainsTae(string anibndFile)
        {
            bool result = false;
            IBinder bnd = null;
            if (BND3.Is(anibndFile))
                bnd = BND3.Read(anibndFile);
            else if (BND4.Is(anibndFile))
                bnd = BND4.Read(anibndFile);
            if (bnd != null)
            {
                foreach (var f in bnd.Files)
                {
                    if (f.Name.ToLowerInvariant().EndsWith(".tae") || SoulsAssetPipeline.Animation.TAE.Is(f.Bytes))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        // private void NewScanAssetsInBinder(IBinder binder, SoulsAssetPipeline.Animation.TAE.Template template)
        // {
        //     hkxInBND.Clear();
        //     sibcamInBND.Clear();
        //     if (binder != null)
        //     {
        //         // "data\Model\chr\blf\ver_0001.txt" which signals that its "ver_0001"
        //         bool ver_0001 = binder.Files.Any(f => f.ID == 9999999);
        //         int taeFileIDRangeMin = ver_0001 ? 5000000 : 3000000;
        //         int taeFileIDRangeMax = ver_0001 ? 5999999 : 3999999;
        //
        //         foreach (var f in binder.Files)
        //         {
        //             var nameCheck = f.Name.Trim().ToLowerInvariant();
        //             if (nameCheck.EndsWith(".hkx"))
        //             {
        //                 if (!hkxInBND.ContainsKey(nameCheck))
        //                     hkxInBND[f.Name] = f.Bytes;
        //
        //                 var matchingSibcam = binder.Files.FirstOrDefault(s => s.Name.ToLower().EndsWith("camera_win32.sibcam") && s.ID == f.ID - 1);
        //                 if (matchingSibcam != null)
        //                 {
        //                     if (!sibcamInBND.ContainsKey(f.Name))
        //                         sibcamInBND.Add(f.Name, matchingSibcam.Bytes);
        //                     else
        //                         sibcamInBND[f.Name] = matchingSibcam.Bytes;
        //                 }
        //             }
        //             // else if (f.ID >= taeFileIDRangeMin && f.ID <= taeFileIDRangeMax)
        //             // {
        //             //     Proj.RegisterTAE(f.Name, f, template);
        //             // }
        //         }
        //     }
        // }

        private bool CheckIfDemonsSouls2010AnibndIsValid(IBinder bnd)
        {
            foreach (var f in bnd.Files)
            {
                if (f.Bytes.Length >= 0x36)
                {
                    using (var memStream = new MemoryStream(f.Bytes))
                    {
                        var br = new BinaryReaderEx(false, memStream);
                        var test = br.GetASCII(0x28, 14);
                        if (test == "Havok-5.5.0-r1")
                            return false;
                    }
                }
            }
            return true;
        }


        public bool NewLoadFromContainerInfo(DSAProj.TaeContainerInfo info, out string errorMsg, TAE.Template template, zzz_DocumentIns parentDocument)
        {

            try
            {
                

                var fileInfoValid = info.CheckValidity(out string infoErrorMsg, out IBinder binder);

                

                if (fileInfoValid)
                {
                    containerBinder = binder;

                    string mainBinderPath = info.GetMainBinderName();

                    if (parentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DES)
                    {
                        string binder2010Path = info.GetMainBinderName() + ".2010";
                        if (File.Exists(binder2010Path))
                        {
                            containerBinder_2010 = Utils.ReadBinder(binder2010Path);
                            if (containerBinder_2010 == null || !CheckIfDemonsSouls2010AnibndIsValid(containerBinder_2010))
                            {
                                containerBinder_2010 = GenerateDemonsSoulsConvertedAnibnd(mainBinderPath);
                            }
                        }
                        else
                        {
                            containerBinder_2010 = GenerateDemonsSoulsConvertedAnibnd(mainBinderPath);
                        }
                        containerBinder_2010_IsModified = false;
                    }

                    CheckGameVersionForTaeInterop(mainBinderPath, containerBinder);

                    Editor.CheckAutoLoadXMLTemplate(mainBinderPath);
                    Editor.ApplyAlreadySetTAETemplate();
                    Info = info;

                    var dsaprojName = info.GetDSAProjFileName();

                    Proj = new DSAProj(parentDocument);
                    
                    if (File.Exists(dsaprojName))
                    {
                        Proj = new DSAProj(parentDocument);
                        //Proj.DeserializeFromFile(dsaprojName);

                        try
                        {
                            Proj.SAFE_DeserializeFromFile(dsaprojName, template);
                        }
                        catch (DSAProjTooNewException)
                        {
                            var choice = MessageBox.Show($"Associated *{DSAProj.EXT} file was saved with a newer version of DS Anim Studio. Cannot open.\n\n" +
                                "Resetting everything to default.\nWARNING: >>>If you save, it will overwrite the incompatible file.<<<",
                                $"Invalid Metadata (*{DSAProj.EXT}) File", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            Proj = DSAProj.CreateFromContainer(Info, template, parentDocument, checkValidity: false);
                        }
                        catch (Exception ex)
                        {
                            var choice = MessageBox.Show($"Associated *{DSAProj.EXT} file is invalid/corrupted. Cannot open.\n\n" +
                                "Resetting everything to default.\nWARNING: >>>If you save, it will overwrite the corrupted file.<<<" +
                                $"\n\nError shown below:\n{ex}",
                                $"Invalid Metadata (*{DSAProj.EXT}) File", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            Proj = DSAProj.CreateFromContainer(Info, template, parentDocument, checkValidity: false);
                        }

                        var runtimeFlags = Proj.GetAllTAEsRuntimeFlags();
                        if (runtimeFlags != EditorFlags.None)
                            ShowPopupForDSAProjRuntimeFlags(runtimeFlags);

                        
                    }
                    else
                    {
                        Proj = DSAProj.CreateFromContainer(Info, template, parentDocument, checkValidity: false);
                    }


                    //
                    //
                    // if (Proj.ContainerInfo is DSAProj.TaeContainerInfo.ContainerAnibnd asAnibnd)
                    // {
                    //     NewScanAssetsInBinder(containerBinder, template);
                    // }
                    // else if (Proj.ContainerInfo is DSAProj.TaeContainerInfo.ContainerAnibndInBinder asAnibndInBinder)
                    // {
                    //     foreach (var bf in containerBinder.Files)
                    //     {
                    //         if (bf.ID == asAnibndInBinder.BindID)
                    //         {
                    //             NewScanAssetsInBinder(Utils.ReadBinder(bf.Bytes), template);
                    //         }
                    //     }
                    // }




                    //if (File.Exists(Info.GetMainBinderName() + ".dsasbak"))
                    //{
                    //    var newlyGeneratedProj = DSAProj.CreateFromContainer(Info, template, parentDocument, checkValidity: false, readFromBackup: true);
                    //    Proj.ScanAnimNamesFromOtherProj(newlyGeneratedProj);
                    //}
                    
                    errorMsg = null;
                    return true;
                }
                else
                {
                    errorMsg = infoErrorMsg;
                    return false;
                }

            }
            catch (Exception ex) when (Main.EnableErrorHandler.TaeFileContainerLoad)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        private void NewSaveAssetsToBinder(IBinder binder, IProgress<double> prog, double progStart, double progWeight)
        {
            bool ver_0001 = binder.Files.Any(f => f.ID == 9999999);
            int taeFileIDRangeMin = ver_0001 ? 5000000 : 3000000;
            int taeFileIDRangeMax = ver_0001 ? 5999999 : 3999999;

            binder.Files.RemoveAll(f => f.ID >= taeFileIDRangeMin && f.ID <= taeFileIDRangeMax);

            var taeBinderFiles = Proj.WriteTaeFilesForAnibnd(prog, progStart, progWeight);
            binder.Files.AddRange(taeBinderFiles);


            binder.Files = binder.Files.OrderBy(x => x.ID).ToList();

            //Console.WriteLine("test");
        }

        public void NewSaveContainer(out bool savedDsaproj, out bool savedContainer, IProgress<double> prog, double progStart, double progWeight)
        {
            savedDsaproj = false;
            savedContainer = false;
            if (Info != null)
            {
                bool failed = false;
                Proj.ScanForErrors();
                if (Proj.ErrorContainer.AnyErrors())
                {
                    
                    try
                    {
                        Proj.SAFE_SerializeToFile(Proj.ContainerInfo.GetDSAProjFileName(), prog, progStart, progWeight);
                    }
                    catch (Exception ex)
                    {
                        ImguiOSD.DialogManager.DialogOK("Error", $"Failed to save project file.\n\n{ex}");

                        failed = true;
                    }

                    if (!failed)
                    {
                        Proj.SAFE_ClearAllModified();
                        Proj.SAFE_ClearRuntimeFlagsOnAll(DSAProj.EditorFlags.Combo_AllNeedsResaveFlags);

                        ImguiOSD.DialogManager.DialogOK("Error", $"Cannot output to game files due to errors (shown in the red 'ERRORS' window). " +
                                                                 $"\nThe *{DSAProj.EXT} file has been saved so your work is safe and will not be lost upon closing." +
                                                                 $"\nOnce you fix the errors, you may output to the game files by saving again.");
                        savedDsaproj = true;
                        savedContainer = false;
                    }
                    return;
                }

                savedContainer = false;
                if (Proj.ContainerInfo is DSAProj.TaeContainerInfo.ContainerAnibnd asAnibnd)
                {
                    NewSaveAssetsToBinder(containerBinder, prog, progStart, progWeight * 0.9);
                    savedContainer = true;
                }
                else if (Proj.ContainerInfo is DSAProj.TaeContainerInfo.ContainerAnibndInBinder asAnibndInBinder)
                {
                    foreach (var bf in containerBinder.Files)
                    {
                        if (bf.ID == asAnibndInBinder.BindID)
                        {
                            var binder = Utils.ReadBinder(bf.Bytes);
                            NewSaveAssetsToBinder(binder, prog, progStart, progWeight * 0.9);
                            bf.Bytes = Utils.WriteBinder(binder);
                        }
                    }
                    savedContainer = true;
                }


                
                try
                {
                    Proj.SAFE_SerializeToFile(Proj.ContainerInfo.GetDSAProjFileName(), prog, progStart + 0.9, progWeight * 0.1);
                }
                catch (Exception ex)
                {
                    ImguiOSD.DialogManager.DialogOK("Error", $"Failed to save project file.\n\n{ex}");

                    failed = true;
                }
                if (!failed)
                {
                    Proj.SAFE_ClearAllModified();
                    Proj.SAFE_ClearRuntimeFlagsOnAll(DSAProj.EditorFlags.Combo_AllNeedsResaveFlags);
                    savedDsaproj = true;
                }
                

                if (savedContainer)
                {
                    var containerBinderBytes = Utils.WriteBinder(containerBinder);
                    File.WriteAllBytes(Info.GetMainBinderName(), containerBinderBytes);
                }
            }
            else
            {
                savedContainer = false;
                savedDsaproj = false;
            }
        }
    }
}
