using SoulsFormats;
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

        private IBinder containerOBJBND;

        private string anibndPathInsideObjbnd = null;

        private Dictionary<string, TAE> taeInBND = new Dictionary<string, TAE>();
        private Dictionary<string, byte[]> hkxInBND = new Dictionary<string, byte[]>();

        public List<BinderFile> RelatedModelFiles = new List<BinderFile>();

        public bool IsModified = false;

        public static readonly string DefaultSaveFilter = 
            "Anim Container (*.ANIBND[.DCX]) |*.ANIBND*|" +
            "Object Container (*.OBJBND[.DCX]) |*.OBJBND*|" +
            "All Files|*.*";

        public bool IsBloodborne => GameDataManager.GameType == GameDataManager.GameTypes.BB;

        public string GetResaveFilter()
        {
            return DefaultSaveFilter;
        }

        //public Dictionary<int, TAE> StandardTAE { get; private set; } = null;
        //public Dictionary<int, TAE> PlayerTAE { get; private set; } = null;

        public IEnumerable<TAE> AllTAE => taeInBND.Values;

        public IReadOnlyDictionary<string, byte[]> AllHKXDict => hkxInBND;

        public IReadOnlyDictionary<string, TAE> AllTAEDict => taeInBND;

        private string GetInterrootFromPath()
        {
            var folder = new System.IO.FileInfo(filePath).DirectoryName;

            var lastSlashInFolder = folder.LastIndexOf("\\");

            return folder.Substring(0, lastSlashInFolder);
        }

        private void CheckGameVersionForTaeInterop(string filePath)
        {
            var check = filePath.ToUpper();
            string interroot = GetInterrootFromPath();
            if (check.Contains("FRPG2"))
            {
                // SLHSDJFSHH
                //GameType = TaeGameType.DS2;
                //GameDataManager.GameType = GameDataManager.GameTypes.DS2;
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
            filePath = file;

            containerANIBND = null;

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
                taeInBND.Add(file, TAE.Read(file));
            }

            //void DoBnd(IBinder bnd)
            //{
               
            //}

            if (ContainerType == TaeFileContainerType.ANIBND)
            {
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
                            taeInBND.Add(f.Name, TAE.Read(f.Bytes));
                        }
                        else if (f.Name.ToUpper().EndsWith(".HKX"))
                        {
                            hkxInBND.Add(f.Name, f.Bytes);
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
                                taeInBND.Add(f.Name, TAE.Read(f.Bytes));
                            }
                            else if (f.Name.ToUpper().EndsWith(".HKX"))
                            {
                                hkxInBND.Add(f.Name, f.Bytes);
                            }
                        }
                        innerProgress.Report(1);
                    }
                });
            }
        }

        public void SaveToPath(string file, IProgress<double> progress)
        {
            file = file.ToUpper();

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

            Main.TAE_EDITOR.UpdateIsModifiedStuff();
        }
    }
}
