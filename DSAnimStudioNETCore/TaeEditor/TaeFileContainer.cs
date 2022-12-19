using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace DSAnimStudio.TaeEditor
{
    public class TaeFileContainer
    {
        private readonly TaeEditorScreen Editor;

        public enum TaeFileContainerType
        {
            TAE,
            ANIBND,
            OBJBND,
            REMOBND
        }

        public TaeFileContainer(TaeEditorScreen editor)
        {
            Editor = editor;
        }

        private string filePath
        {
            get => Editor.FileContainerName;
            set => Editor.FileContainerName = value;
        }

        public TaeFileContainerType ContainerType { get; private set; }

        private IBinder containerANIBND;
        private IBinder containerANIBND_2010;
        private bool containerANIBND_2010_IsModified = false;

        private IBinder containerOBJBND;

        private string anibndPathInsideObjbnd = null;

        private Dictionary<string, TAE> taeInBND = new Dictionary<string, TAE>();
        private Dictionary<string, byte[]> hkxInBND = new Dictionary<string, byte[]>();
        private Dictionary<string, byte[]> sibcamInBND = new Dictionary<string, byte[]>();
        private List<int> taeSectionsInBND = new List<int>();

        private void DeleteAnimHKX(string name)
        {
            if (ContainerType != TaeFileContainerType.ANIBND)
                throw new NotImplementedException("Not supported for anything other than ANIBND right now.");

            BinderFile existingFile = containerANIBND.Files.FirstOrDefault(ff => ff.Name.Contains(name));
            BinderFile existingFile_2010 = containerANIBND_2010?.Files.FirstOrDefault(ff => ff.Name.Contains(name));

            if (existingFile != null)
                containerANIBND.Files.Remove(existingFile);

            if (existingFile_2010 != null)
                containerANIBND_2010.Files.Remove(existingFile_2010);

            if (hkxInBND.ContainsKey(name))
                hkxInBND.Remove(name);
        }

        public void AddNewHKX(string name, byte[] data, out byte[] dataForAnimContainer)//, byte[] predeterminedDataHavok2010 = null)
        {
            if (data == null)
            {
                dataForAnimContainer = null;
                return;
            }

            if (ContainerType != TaeFileContainerType.ANIBND)
                throw new NotImplementedException("Not supported for anything other than ANIBND right now.");

            dataForAnimContainer = data;

            if (ContainerType == TaeFileContainerType.ANIBND)
            {
                DeleteAnimHKX(name + ".hkx");

                hkxInBND.Add(name + ".hkx", data);
                BinderFile f = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", data);
                containerANIBND.Files.Add(f);
                containerANIBND.Files = containerANIBND.Files.OrderBy(bf => bf.ID).ToList();
                IsModified = true;
            

                //if (GameDataManager.GameTypeIsHavokTagfile)
                //{
                //    if (HavokDowngrade.SimpleCheckIfHkxBytesAre2015(data))
                //    {
                //        BinderFile f = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", data);
                //        containerANIBND.Files.Add(f);
                //        containerANIBND.Files = containerANIBND.Files.OrderBy(bf => bf.ID).ToList();


                //        byte[] bytes2010 = predeterminedDataHavok2010 ?? HavokDowngrade.DowngradeSingleFileInANIBND(containerANIBND, f, isUpgrade: false);
                //        BinderFile f2010 = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", bytes2010);
                //        containerANIBND_2010.Files.Add(f2010);
                //        containerANIBND_2010.Files = containerANIBND_2010.Files.OrderBy(bf => bf.ID).ToList();
                //        containerANIBND_2010_IsModified = true;
                //        hkxInBND.Add(name + ".hkx", bytes2010);

                //        dataForAnimContainer = bytes2010;

                //        IsModified = true;
                //    }
                //    else
                //    {
                //        hkxInBND.Add(name + ".hkx", data);

                //        BinderFile f2010 = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", data);
                //        containerANIBND_2010.Files.Add(f2010);
                //        containerANIBND_2010.Files = containerANIBND_2010.Files.OrderBy(bf => bf.ID).ToList();
                //        containerANIBND_2010_IsModified = true;

                //        byte[] bytes2015 = HavokDowngrade.DowngradeSingleFileInANIBND(containerANIBND_2010, f2010, isUpgrade: true);

                //        BinderFile f = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", bytes2015);
                //        containerANIBND.Files.Add(f);
                //        containerANIBND.Files = containerANIBND.Files.OrderBy(bf => bf.ID).ToList();
                //        IsModified = true;
                //    }

                //}
                //else
                //{
                //    hkxInBND.Add(name + ".hkx", data);
                //    BinderFile f = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", data);
                //    containerANIBND.Files.Add(f);
                //    containerANIBND.Files = containerANIBND.Files.OrderBy(bf => bf.ID).ToList();
                //    IsModified = true;
                //}
            }
            
        }

        public List<BinderFile> RelatedModelFiles = new List<BinderFile>();

        public bool IsModified = false;

        public bool IsCurrentlyLoading { get; private set; } = false;

        public static readonly string DefaultSaveFilter =
            "Anim Container (*.ANIBND[.DCX]) |*.ANIBND*|" +
            "Cutscene Container (*.REMOBND[.DCX]) |*.REMOBND*|" +
            "Object Container (*.OBJBND[.DCX]) |*.OBJBND*|" +
            "Loose TimeAct File (*.TAE) |*.TAE|" +
            "All Files|*.*";

        public static readonly string DefaultSaveFilter_Model =
            "Character Model Container (*.CHRBND[.DCX]) |*.CHRBND*|" +
            "Object Model Container (*.OBJBND[.DCX]) |*.OBJBND*|" +
            //"Loose FLVER Model File (*.FLVER) |*.FLVER|" +
            "All Files|*.*";

        public bool IsBloodborne => GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB;

        public string GetResaveFilter()
        {
            return DefaultSaveFilter;
        }

        //public Dictionary<int, TAE> StandardTAE { get; private set; } = null;
        //public Dictionary<int, TAE> PlayerTAE { get; private set; } = null;

        public IReadOnlyList<TAE> AllTAE
        {
            get
            {
                IReadOnlyList<TAE> tae = null;
                lock (_lock_loading)
                {
                    tae = taeInBND.Values.ToList();
                }
                return tae;
            }
        }

        private object _lock_loading = new object();

        public IReadOnlyDictionary<string, byte[]> AllHKXDict
        {
            get
            {
                Dictionary<string, byte[]> result = null;
                lock (_lock_loading)
                {
                    result = hkxInBND.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return result;
            }
        }

        public IReadOnlyDictionary<string, byte[]> AllSibcamDict
        {
            get
            {
                Dictionary<string, byte[]> result = null;
                lock (_lock_loading)
                {
                    result = sibcamInBND.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return result;
            }
        }

        public IReadOnlyDictionary<string, TAE> AllTAEDict
        {
            get
            {
                Dictionary<string, TAE> result = null;
                lock (_lock_loading)
                {
                    result = taeInBND.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return result;
            }
        }

        public IReadOnlyList<int> AllTAESections
        {
            get
            {
                List<int> result = null;
                lock (_lock_loading)
                {
                    result = taeSectionsInBND.ToList();
                }
                return result;
            }
        }

        private void RegistTaeSections(string taeName, TAE tae)
        {
            taeName = Utils.GetShortIngameFileName(taeName);
            if (taeName.StartsWith("c"))
            {
                foreach (var anim in tae.Animations)
                {
                    int v = (int)(anim.ID / (GameRoot.GameTypeHasLongAnimIDs ? 1_000000 : 1_0000));
                    if (!taeSectionsInBND.Contains(v))
                        taeSectionsInBND.Add(v);
                }
            }
            else if (taeName.StartsWith("a"))
            {
                int v = int.Parse(taeName.Substring(1));
                if (!taeSectionsInBND.Contains(v))
                    taeSectionsInBND.Add(v);
            }
        }

        public (int Min, int Max) GetTaeSectionMinMax()
        {
            int min = -1;
            int max = -1;
            foreach (var s in taeSectionsInBND)
            {
                if (s < min)
                    min = s;
                if (s > max)
                    max = s;
            }
            return (min, max);
        }
        

        public (long Upper, long Lower) GetSplitAnimID(long id)
        {
            return ((GameRoot.GameTypeHasLongAnimIDs) ? (id / 1000000) : (id / 10000),
                (GameRoot.GameTypeHasLongAnimIDs) ? (id % 1000000) : (id % 10000));
        }

        private (long UpperID, TAE.Animation Anim) GetTAEAnim(long compositeId)
        {
            var id = GetSplitAnimID(compositeId);

            if (AllTAEDict.Count > 1)
            {
                var tae = GetTAE(id.Upper);
                if (tae != null)
                {
                    var anim = GetAnimInTAE(tae, id.Lower);
                    if (anim != null)
                        return (id.Upper, anim);
                }
            }
            else
            {
                var tae = AllTAEDict.First().Value;
                var anim = GetAnimInTAE(tae, GetCompositeAnimID(id));
                if (anim != null)
                    return (id.Upper, anim);
            }
            return (0, null);
        }

        public TAE GetTAE(long id)
        {
            if (AllTAEDict.Count == 1)
                return AllTAEDict.FirstOrDefault().Value;

            foreach (var kvp in AllTAEDict)
            {
                if (kvp.Key.ToUpper().EndsWith($"{id:D2}.TAE"))
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        public TAE.Animation GetAnimInTAE(TAE tae, long id)
        {
            foreach (var a in tae.Animations)
            {
                if (a.ID == id)
                    return a;
            }
            return null;
        }

        private long GetCompositeAnimID((long Upper, long Lower) id)
        {
            if (GameRoot.GameTypeHasLongAnimIDs)
            {
                return (id.Upper * 1_000000) + (id.Lower % 1_000000);
            }
            else
            {
                return (id.Upper * 1_0000) + (id.Lower % 1_0000);
            }
        }

        public TAE.Animation GetAnimRef(int compositeID)
        {
            var anim = GetTAEAnim(compositeID);
            return anim.Anim;
        }

        public (TAE, TAE.Animation) GetAnimRefFull(int compositeID)
        {
            var anim = GetTAEAnim(compositeID);
            var tae = GetTAE(anim.UpperID);
            return (tae, anim.Anim);
        }

        public static void CheckGameVersionForTaeInterop(string binderPath)
        {
            IBinder binder = null;
            if (BND3.Is(binderPath))
                binder = BND3.Read(binderPath);
            else if (BND4.Is(binderPath))
                binder = BND4.Read(binderPath);
            if (binder != null)
            {
                var firstFile = binder.Files.FirstOrDefault();
                if (firstFile != null)
                {
                    CheckGameVersionForTaeInterop(binderPath, firstFile.Name, false);
                }
            }
        }

        public static void CheckGameVersionForTaeInterop(string filePath, string internalFilePath, bool isRemo)
        {
            var check = internalFilePath.ToUpper();
            string scratchFolder = GameRoot.GetParentDirectory(filePath);
            if (check.Contains("FRPG2"))
            {
                GameRoot.Init(filePath, SoulsAssetPipeline.SoulsGames.DS2SOTFS, scratchFolder);
            }
            else if ((check.Contains(@"\FRPG\") && check.Contains(@"HKXX64")) || (check.Contains(@"TAENEW\X64") && isRemo))
            {
                GameRoot.Init(filePath, SoulsAssetPipeline.SoulsGames.DS1R, scratchFolder);
            }
            else if ((check.Contains(@"\FRPG\") && check.Contains(@"HKXWIN32")) || (check.Contains(@"TAENEW\WIN32") && isRemo))
            {
                GameRoot.Init(filePath, SoulsAssetPipeline.SoulsGames.DS1, scratchFolder);
            }
            else if (check.Contains(@"\SPRJ\"))
            {
                GameRoot.Init(filePath, SoulsAssetPipeline.SoulsGames.BB, scratchFolder);
            }
            else if (check.Contains(@"\FDP\"))
            {
                GameRoot.Init(filePath, SoulsAssetPipeline.SoulsGames.DS3, scratchFolder);
            }
            else if (check.Contains(@"\DEMONSSOUL\"))
            {
                //scratchFolder = GameRoot.GetParentDirectory(scratchFolder);
                GameRoot.Init(filePath, SoulsAssetPipeline.SoulsGames.DES, scratchFolder);
            }
            else if (check.Contains(@"\NTC\"))
            {
                GameRoot.Init(filePath, SoulsAssetPipeline.SoulsGames.SDT, scratchFolder);
            }
            else if (check.Contains(@"\GR\"))
            {
                GameRoot.Init(filePath, SoulsAssetPipeline.SoulsGames.ER, scratchFolder);
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
                LoadingTaskMan.DoLoadingTaskSynchronous(null, "Converting Demon's Souls Animations", innerProgress =>
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

                NotificationManager.PushNotification("Oodle compression library was automatically copied from game directory " +
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
                    if (f.Name.ToLowerInvariant().EndsWith(".tae") && TAE.Is(f.Bytes))
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
                    if (f.Name.ToLowerInvariant().EndsWith(".tae") && TAE.Is(f.Bytes))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public void LoadFromPath(string file, string possibleFallbackPath)
        {
            lock (Main.TAE_EDITOR)
            {
                GameRoot.ClearInterroot();

                IsCurrentlyLoading = true;

                filePath = file;

                string fileTypeCheck = Path.GetFileName(filePath).ToUpper();
                bool isRemo = fileTypeCheck.Contains(".REMOBND");

                containerANIBND = null;
                containerANIBND_2010 = null;
                containerANIBND_2010_IsModified = false;

                taeSectionsInBND.Clear();
                taeInBND.Clear();
                hkxInBND.Clear();
                sibcamInBND.Clear();

                if (BND3.Is(file))
                {
                    ContainerType = TaeFileContainerType.ANIBND;
                    containerANIBND = BND3.Read(file);
                }
                else if (BND4.Is(file))
                {
                    ContainerType = TaeFileContainerType.ANIBND;
                    containerANIBND = BND4.Read(file);
                }
                else if (TAE.Is(file))
                {
                    CheckGameVersionForTaeInterop(filePath, file, false);

                    ContainerType = TaeFileContainerType.TAE;
                    var t = TAE.Read(file);
                    taeInBND.Add(file, t);

                    if (t.Format == TAE.TAEFormat.SOTFS)
                    {
                        GameRoot.Init(filePath, SoulsAssetPipeline.SoulsGames.DS2SOTFS,
                            GameRoot.GetParentDirectory(filePath));
                    }
                    else
                    {
                        throw new NotImplementedException("Non-DS2 loose .TAE files not supported yet.");
                    }
                }

                if (containerANIBND != null && possibleFallbackPath != null && File.Exists(possibleFallbackPath) && !containerANIBND.Files.Any(f => f.Name.ToLower().EndsWith(".tae")))
                {

                    LoadFromPath(possibleFallbackPath, null);
                    return;
                }

                //void DoBnd(IBinder bnd)
                //{

                //}

                if (ContainerType == TaeFileContainerType.ANIBND)
                {
                    foreach (var f in containerANIBND.Files)
                    {
                        CheckGameVersionForTaeInterop(filePath, f.Name, isRemo);
                    }

                    if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES && !GameRoot.GameIsDemonsSoulsRemastered)
                    {
                        containerANIBND_2010 = GenerateDemonsSoulsConvertedAnibnd(filePath);
                    }

                    LoadingTaskMan.DoLoadingTaskSynchronous("TaeFileContainer_ANIBND", "Loading all TAE files in ANIBND...", innerProgress =>
                    {
                        double i = 0;
                        foreach (var f in containerANIBND.Files)
                        {
                            //DESR .hkt hotfix
                            if (f.Name.ToLowerInvariant().EndsWith(".hkt"))
                                f.Name = f.Name.Substring(0, f.Name.Length - 3) + "hkx";

                            innerProgress.Report(++i / containerANIBND.Files.Count);

                            CheckGameVersionForTaeInterop(filePath, f.Name, isRemo);
                            if (BND3.Is(f.Bytes))
                            {
                                ContainerType = TaeFileContainerType.OBJBND;
                                containerOBJBND = containerANIBND;
                                containerANIBND = BND3.Read(f.Bytes);
                                anibndPathInsideObjbnd = f.Name;
                                break;
                            }
                            else if (BND4.Is(f.Bytes))
                            {
                                ContainerType = TaeFileContainerType.OBJBND;
                                containerOBJBND = containerANIBND;
                                containerANIBND = BND4.Read(f.Bytes);
                                anibndPathInsideObjbnd = f.Name;
                                break;
                            }
                            else if (TAE.Is(f.Bytes))
                            {
                                if (!taeInBND.ContainsKey(f.Name))
                                    taeInBND.Add(f.Name, TAE.Read(f.Bytes));
                                else
                                    taeInBND[f.Name] = TAE.Read(f.Bytes);
                                RegistTaeSections(f.Name, taeInBND[f.Name]);
                            }
                            else if (f.Name.ToUpper().EndsWith(".HKX"))
                            {
                                if (!hkxInBND.ContainsKey(f.Name))
                                    hkxInBND.Add(f.Name, f.Bytes);
                                else
                                    hkxInBND[f.Name] = f.Bytes;

                                var matchingSibcam = containerANIBND.Files.FirstOrDefault(s => s.Name.ToLower().EndsWith("camera_win32.sibcam") && s.ID == f.ID - 1);
                                if (matchingSibcam != null)
                                {
                                    if (!sibcamInBND.ContainsKey(f.Name))
                                        sibcamInBND.Add(f.Name, matchingSibcam.Bytes);
                                    else
                                        sibcamInBND[f.Name] = matchingSibcam.Bytes;
                                }
                            }
                        }
                        innerProgress.Report(1);

                        if (ContainerType == TaeFileContainerType.OBJBND)
                        {
                            i = 0;
                            foreach (var f in containerANIBND.Files)
                            {
                                //DESR hotfix
                                if (f.Name.ToLowerInvariant().EndsWith(".hkt"))
                                    f.Name = f.Name.Substring(0, f.Name.Length - 3) + "hkx";

                                innerProgress.Report(++i / containerANIBND.Files.Count);

                                CheckGameVersionForTaeInterop(filePath, f.Name, isRemo);
                                if (TAE.Is(f.Bytes))
                                {
                                    if (!taeInBND.ContainsKey(f.Name))
                                        taeInBND.Add(f.Name, TAE.Read(f.Bytes));
                                    else
                                        taeInBND[f.Name] = TAE.Read(f.Bytes);
                                    RegistTaeSections(f.Name, taeInBND[f.Name]);
                                }
                                else if (f.Name.ToUpper().EndsWith(".HKX"))
                                {
                                    if (!hkxInBND.ContainsKey(f.Name))
                                        hkxInBND.Add(f.Name, f.Bytes);
                                    else
                                        hkxInBND[f.Name] = f.Bytes;

                                    var matchingSibcam = containerANIBND.Files.FirstOrDefault(s => s.Name.ToLower().EndsWith("camera_win32.sibcam") && s.ID == f.ID - 1);
                                    if (matchingSibcam != null)
                                    {
                                        if (!sibcamInBND.ContainsKey(f.Name))
                                            sibcamInBND.Add(f.Name, matchingSibcam.Bytes);
                                        else
                                            sibcamInBND[f.Name] = matchingSibcam.Bytes;
                                    }
                                }
                            }
                            innerProgress.Report(1);
                        }
                    });
                }

                IsModified = false;
                IsCurrentlyLoading = false;
            }
        }

        public void SaveToPath(string file, IProgress<double> progress)
        {
            lock (Main.TAE_EDITOR)
            {
                //What the hell was this for?
                //file = file.ToUpper();

                if (ContainerType == TaeFileContainerType.ANIBND)
                {
                    double i = 0;
                    foreach (var f in containerANIBND.Files)
                    {
                        progress.Report((++i / containerANIBND.Files.Count) * 0.9);
                        if (taeInBND.ContainsKey(f.Name))
                        {
                            bool needToSave = false;

                            foreach (var anim in taeInBND[f.Name].Animations)
                            {
                                if (anim.GetIsModified())
                                    needToSave = true;

                                // Regardless of whether we need to save this TAE, this anim should 
                                // be set to not modified :fatcat:
                                anim.SetIsModified(false, updateGui: false);
                            }

                            if (needToSave)
                            {
                                f.Bytes = taeInBND[f.Name].Write();
                                taeInBND[f.Name].SetIsModified(false, updateGui: false);
                            }
                        }
                    }

                    if (containerANIBND is BND3 asBND3)
                        asBND3.Write(file);
                    else if (containerANIBND is BND4 asBND4)
                        asBND4.Write(file);

                    if (containerANIBND_2010 != null && containerANIBND_2010_IsModified)
                    {
                        if (containerANIBND_2010 is BND3 asBND3_2010)
                            asBND3_2010.Write(file + ".2010");
                        else if (containerANIBND_2010 is BND4 asBND4_2010)
                            asBND4_2010.Write(file + ".2010");
                    }

                    progress.Report(1.0);
                }
                else if (ContainerType == TaeFileContainerType.OBJBND)
                {
                    double i = 0;
                    foreach (var f in containerANIBND.Files)
                    {
                        progress.Report((++i / containerANIBND.Files.Count) * 0.9);
                        if (taeInBND.ContainsKey(f.Name))
                        {
                            bool needToSave = false;

                            foreach (var anim in taeInBND[f.Name].Animations)
                            {
                                if (anim.GetIsModified())
                                    needToSave = true;

                                // Regardless of whether we need to save this TAE, this anim should 
                                // be set to not modified :fatcat:
                                anim.SetIsModified(false, updateGui: false);
                            }

                            if (needToSave)
                            {
                                f.Bytes = taeInBND[f.Name].Write();
                                taeInBND[f.Name].SetIsModified(false, updateGui: false);
                            }
                        }
                    }

                    var anibndInObjbnd = containerOBJBND.Files.FirstOrDefault(f => f.Name == anibndPathInsideObjbnd);
                    if (anibndInObjbnd == null)
                    {
                        throw new Exception("Error: Could not find ANIBND within the OBJBND (please report)");
                    }

                    if (containerANIBND is BND3 asBND3)
                        anibndInObjbnd.Bytes = asBND3.Write();
                    else if (containerANIBND is BND4 asBND4)
                        anibndInObjbnd.Bytes = asBND4.Write();

                    if (containerOBJBND is BND3 asOBJBND3)
                        asOBJBND3.Write(file);
                    else if (containerOBJBND is BND4 asOBJBND4)
                        asOBJBND4.Write(file);
                }
                else if (ContainerType == TaeFileContainerType.TAE)
                {
                    var tae = taeInBND[filePath];
                    tae.Write(file);

                    taeInBND.Clear();
                    taeInBND.Add(file, taeInBND[filePath]);

                    taeSectionsInBND.Clear();
                    RegistTaeSections(file, taeInBND[filePath]);

                    progress.Report(1.0);
                }

                IsModified = false;
            }
        }
    }
}
