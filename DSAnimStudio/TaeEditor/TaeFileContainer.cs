using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSAnimStudio.TaeEditor
{
    public class TaeFileContainer
    {
        public enum TaeFileContainerType
        {
            TAE,
            ANIBND,
            OBJBND
        }

        private string filePath;

        public TaeFileContainerType ContainerType { get; private set; }

        private IBinder containerANIBND;
        private IBinder containerANIBND_2010;
        private bool containerANIBND_2010_IsModified = false;

        private IBinder containerOBJBND;

        private string anibndPathInsideObjbnd = null;

        private Dictionary<string, TAE> taeInBND = new Dictionary<string, TAE>();
        private Dictionary<string, byte[]> hkxInBND = new Dictionary<string, byte[]>();

        private void DeleteAnimHKX(string name)
        {
            if (ContainerType != TaeFileContainerType.ANIBND)
                throw new NotImplementedException("Not supported for anything other than ANIBND right now.");

            BinderFile existingFile = containerANIBND.Files.FirstOrDefault(ff => ff.Name.Contains(name));
            BinderFile existingFile_2010 = containerANIBND_2010.Files.FirstOrDefault(ff => ff.Name.Contains(name));

            if (existingFile != null)
                containerANIBND.Files.Remove(existingFile);

            if (existingFile_2010 != null)
                containerANIBND_2010.Files.Remove(existingFile_2010);

            if (hkxInBND.ContainsKey(name))
                hkxInBND.Remove(name);
        }

        public void AddNewHKX(string name, byte[] data, out byte[] dataForAnimContainer)
        {
            if (ContainerType != TaeFileContainerType.ANIBND)
                throw new NotImplementedException("Not supported for anything other than ANIBND right now.");

            dataForAnimContainer = data;

            if (ContainerType == TaeFileContainerType.ANIBND)
            {
                DeleteAnimHKX(name + ".hkx");

                if (GameDataManager.GameTypeIsHavokTagfile)
                {
                    if (HavokDowngrade.SimpleCheckIfHkxBytesAre2015(data))
                    {
                        BinderFile f = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", data);
                        containerANIBND.Files.Add(f);
                        containerANIBND.Files = containerANIBND.Files.OrderBy(bf => bf.ID).ToList();
                        

                        byte[] bytes2010 = HavokDowngrade.DowngradeSingleFileInANIBND(containerANIBND, f, isUpgrade: false);
                        BinderFile f2010 = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", bytes2010);
                        containerANIBND_2010.Files.Add(f2010);
                        containerANIBND_2010.Files = containerANIBND_2010.Files.OrderBy(bf => bf.ID).ToList();
                        containerANIBND_2010_IsModified = true;
                        hkxInBND.Add(name + ".hkx", bytes2010);

                        dataForAnimContainer = bytes2010;

                        IsModified = true;
                    }
                    else
                    {
                        hkxInBND.Add(name + ".hkx", data);

                        BinderFile f2010 = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", data);
                        containerANIBND_2010.Files.Add(f2010);
                        containerANIBND_2010.Files = containerANIBND_2010.Files.OrderBy(bf => bf.ID).ToList();
                        containerANIBND_2010_IsModified = true;

                        byte[] bytes2015 = HavokDowngrade.DowngradeSingleFileInANIBND(containerANIBND_2010, f2010, isUpgrade: true);

                        BinderFile f = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", bytes2015);
                        containerANIBND.Files.Add(f);
                        containerANIBND.Files = containerANIBND.Files.OrderBy(bf => bf.ID).ToList();
                        IsModified = true;
                    }

                }
                else
                {
                    hkxInBND.Add(name + ".hkx", data);
                    BinderFile f = new BinderFile(Binder.FileFlags.Flag1, int.Parse(name.Replace("_", "").Replace("a", "")), name + ".hkx", data);
                    containerANIBND.Files.Add(f);
                    containerANIBND.Files = containerANIBND.Files.OrderBy(bf => bf.ID).ToList();
                    IsModified = true;
                }
            }
            
        }

        public List<BinderFile> RelatedModelFiles = new List<BinderFile>();

        public bool IsModified = false;

        public bool IsCurrentlyLoading { get; private set; } = false;

        public static readonly string DefaultSaveFilter = 
            "Anim Container (*.ANIBND[.DCX]) |*.ANIBND*|" +
            "Object Container (*.OBJBND[.DCX]) |*.OBJBND*|" +
            "Loose TimeAct File (*.TAE) |*.TAE|" +
            "All Files|*.*";

        public bool IsBloodborne => GameDataManager.GameType == GameDataManager.GameTypes.BB;

        public string GetResaveFilter()
        {
            return DefaultSaveFilter;
        }

        //public Dictionary<int, TAE> StandardTAE { get; private set; } = null;
        //public Dictionary<int, TAE> PlayerTAE { get; private set; } = null;

        public IEnumerable<TAE> AllTAE => taeInBND.Values;

        private object _lock_loading = new object();

        public IReadOnlyDictionary<string, byte[]> AllHKXDict
        {
            get
            {
                Dictionary<string, byte[]> result = null; 
                lock (_lock_loading)
                {
                    result = hkxInBND;
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
                    result = taeInBND;
                }
                return result;
            }
        }

        

        private (long Upper, long Lower) GetSplitAnimID(long id)
        {
            return ((GameDataManager.GameType == GameDataManager.GameTypes.BB || GameDataManager.GameType == GameDataManager.GameTypes.DS3) ? (id / 1000000) : (id / 10000),
                (GameDataManager.GameType == GameDataManager.GameTypes.BB || GameDataManager.GameType == GameDataManager.GameTypes.DS3) ? (id % 1000000) : (id % 10000));
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

        private TAE GetTAE(long id)
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

        private TAE.Animation GetAnimInTAE(TAE tae, long id)
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
            if (GameDataManager.GameType == GameDataManager.GameTypes.DS3 || GameDataManager.GameType == GameDataManager.GameTypes.BB)
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

        private void CheckGameVersionForTaeInterop(string internalFilePath)
        {
            var check = internalFilePath.ToUpper();
            string interroot = GameDataManager.GetInterrootFromFilePath(filePath);
            if (check.Contains("FRPG2"))
            {
                GameDataManager.Init(GameDataManager.GameTypes.DS2SOTFS, interroot);
            }
            else if (check.Contains(@"\FRPG\") && check.Contains(@"HKXX64"))
            {
                GameDataManager.Init(GameDataManager.GameTypes.DS1R, interroot);
            }
            else if (check.Contains(@"\FRPG\") && check.Contains(@"HKXWIN32"))
            {
                GameDataManager.Init(GameDataManager.GameTypes.DS1, interroot);
            }
            else if (check.Contains(@"\SPRJ\"))
            {
                GameDataManager.Init(GameDataManager.GameTypes.BB, interroot);
            }
            else if (check.Contains(@"\FDP\"))
            {
                GameDataManager.Init(GameDataManager.GameTypes.DS3, interroot);
            }
            else if (check.Contains(@"\DemonsSoul\"))
            {
                GameDataManager.Init(GameDataManager.GameTypes.DES, interroot);
            }
            else if (check.Contains(@"\NTC\"))
            {
                GameDataManager.Init(GameDataManager.GameTypes.SDT, interroot);
            }
        }

        public void LoadFromPath(string file)
        {
            IsCurrentlyLoading = true;

            filePath = file;

            containerANIBND = null;
            containerANIBND_2010 = null;
            containerANIBND_2010_IsModified = false;

            taeInBND.Clear();
            hkxInBND.Clear();

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
                CheckGameVersionForTaeInterop(file);

                ContainerType = TaeFileContainerType.TAE;
                var t = TAE.Read(file);
                taeInBND.Add(file, t);

                if (t.Format == TAE.TAEFormat.SOTFS)
                {
                    GameDataManager.Init(GameDataManager.GameTypes.DS2SOTFS,
                        GameDataManager.GetInterrootFromFilePath(filePath));
                }
                else
                {
                    throw new NotImplementedException("Non-DS2 loose .TAE files not supported yet.");
                }
            }

            //void DoBnd(IBinder bnd)
            //{

            //}

            if (ContainerType == TaeFileContainerType.ANIBND)
            {
                bool isSekiroMeme = false;

                if (System.IO.File.Exists(filePath + ".2010"))
                {
                    isSekiroMeme = true;

                    if (BND3.Is(file + ".2010"))
                    {
                        containerANIBND_2010 = BND3.Read(file + ".2010");
                    }
                    else if (BND4.Is(file + ".2010"))
                    {
                        containerANIBND_2010 = BND4.Read(file + ".2010");
                    }

                    if (containerANIBND_2010 != null)
                    {
                        foreach (var f in containerANIBND.Files)
                        {
                            if (f.Name.ToUpper().EndsWith(".HKX"))
                            {
                                if (!hkxInBND.ContainsKey(f.Name))
                                    hkxInBND.Add(f.Name, f.Bytes);
                                else
                                    hkxInBND[f.Name] = f.Bytes;
                            }
                        }
                    }
                }

                LoadingTaskMan.DoLoadingTaskSynchronous("TaeFileContainer_ANIBND", "Loading all TAE files in ANIBND...", innerProgress =>
                {
                    double i = 0;
                    foreach (var f in containerANIBND.Files)
                    {
                        innerProgress.Report(++i / containerANIBND.Files.Count);

                        CheckGameVersionForTaeInterop(f.Name);
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
                        }
                        else if (!isSekiroMeme & f.Name.ToUpper().EndsWith(".HKX"))
                        {
                            if (!hkxInBND.ContainsKey(f.Name))
                                hkxInBND.Add(f.Name, f.Bytes);
                            else
                                hkxInBND[f.Name] = f.Bytes;
                        }
                    }
                    innerProgress.Report(1);

                    if (ContainerType == TaeFileContainerType.OBJBND)
                    {
                        i = 0;
                        foreach (var f in containerANIBND.Files)
                        {
                            innerProgress.Report(++i / containerANIBND.Files.Count);

                            CheckGameVersionForTaeInterop(f.Name);
                            if (TAE.Is(f.Bytes))
                            {
                                if (!taeInBND.ContainsKey(f.Name))
                                    taeInBND.Add(f.Name, TAE.Read(f.Bytes));
                                else
                                    taeInBND[f.Name] = TAE.Read(f.Bytes);
                            }
                            else if (f.Name.ToUpper().EndsWith(".HKX"))
                            {
                                if (!hkxInBND.ContainsKey(f.Name))
                                    hkxInBND.Add(f.Name, f.Bytes);
                                else
                                    hkxInBND[f.Name] = f.Bytes;
                            }
                        }
                        innerProgress.Report(1);
                    }
                });
            }

            IsModified = false;
            IsCurrentlyLoading = false;
        }

        public void SaveToPath(string file, IProgress<double> progress)
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

                progress.Report(1.0);
            }

            IsModified = false;

            Main.TAE_EDITOR.UpdateIsModifiedStuff();
        }
    }
}
