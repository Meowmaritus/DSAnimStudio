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
            BND3,
            BND4
        }

        public enum TaeFileContainerReloadType
        {
            None,
            CHR_PTDE,
            CHR_DS1R
        }

        private string filePath;

        public TaeFileContainerType ContainerType { get; private set; }

        private BND3 containerBND3;
        private BND4 containerBND4;

        private Dictionary<string, TAE> taeInBND = new Dictionary<string, TAE>();
        private Dictionary<string, byte[]> hkxInBND = new Dictionary<string, byte[]>();

        public List<BinderFile> RelatedModelFiles = new List<BinderFile>();

        public bool IsModified = false;

        public TaeFileContainerReloadType ReloadType = TaeFileContainerReloadType.None;

        public static readonly string DefaultSaveFilter = "Anim Container (*.ANIBND[.DCX]) |*.ANIBND*|" +
                "All Files|*.*";

        public bool IsBloodborne = false;

        public string GetResaveFilter()
        {
            return DefaultSaveFilter;
        }

        //public Dictionary<int, TAE> StandardTAE { get; private set; } = null;
        //public Dictionary<int, TAE> PlayerTAE { get; private set; } = null;

        public IEnumerable<TAE> AllTAE => taeInBND.Values;

        public IReadOnlyDictionary<string, byte[]> AllHKXDict => hkxInBND;

        public IReadOnlyDictionary<string, TAE> AllTAEDict => taeInBND;

        private void CheckGameVersionForTaeInterop(string filePath)
        {
            var check = filePath.ToUpper();
            if (check.Contains("FRPG2"))
            {
                TaeInterop.IncompatibleHavokVersion = true;
            }
            else if (check.Contains(@"\FRPG\") && check.Contains(@"HKXX64"))
            {
                TaeInterop.IncompatibleHavokVersion = true;
            }
            else if (check.Contains(@"\FRPG\") && check.Contains(@"HKXWIN32"))
            {
                TaeInterop.IncompatibleHavokVersion = false;
                TaeInterop.CurrentHkxVariation = HKX.HKXVariation.HKXDS1;
            }
            else if (check.Contains(@"\SPRJ\"))
            {
                TaeInterop.IncompatibleHavokVersion = false;
                TaeInterop.CurrentHkxVariation = HKX.HKXVariation.HKXBloodBorne;
                IsBloodborne = true;
            }
            else if (check.Contains(@"\FDP\"))
            {
                TaeInterop.IncompatibleHavokVersion = false;
                TaeInterop.CurrentHkxVariation = HKX.HKXVariation.HKXDS3;
            }
            else if (check.Contains(@"\DemonsSoul\"))
            {
                TaeInterop.IncompatibleHavokVersion = false;
                TaeInterop.CurrentHkxVariation = HKX.HKXVariation.HKSDeS;
            }
            else if (check.Contains(@"\NTC\"))
            {
                TaeInterop.IncompatibleHavokVersion = true;
                TaeInterop.CurrentHkxVariation = HKX.HKXVariation.HKSDeS;
            }
        }

        public void LoadFromPath(string file, IProgress<double> progress)
        {
            ReloadType = TaeFileContainerReloadType.None;

            containerBND3 = null;
            containerBND4 = null;

            taeInBND.Clear();
            hkxInBND.Clear();

            IsBloodborne = false;

            if (BND3.Is(file))
            {
                ContainerType = TaeFileContainerType.BND3;
                LoadingTaskMan.DoLoadingTaskSynchronous("c0000_ANIBND", "Loading all TAE files in ANIBND...", innerProgress =>
                {
                    containerBND3 = BND3.Read(file);
                    double i = 0;
                    foreach (var f in containerBND3.Files)
                    {
                        innerProgress.Report(++i / containerBND3.Files.Count);

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
                });
                
            }
            else if (BND4.Is(file))
            {
                ContainerType = TaeFileContainerType.BND4;
                LoadingTaskMan.DoLoadingTaskSynchronous("c0000_ANIBND", "Loading all TAE files in ANIBND...", innerProgress =>
                {
                    containerBND4 = BND4.Read(file);
                    double i = 0;
                    foreach (var f in containerBND4.Files)
                    {
                        innerProgress.Report(++i / containerBND4.Files.Count);

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
                });
                   
            }
            else if (TAE.Is(file))
            {
                CheckGameVersionForTaeInterop(file);

                ContainerType = TaeFileContainerType.TAE;
                taeInBND.Add(file, TAE.Read(file));
            }

            progress.Report(0.25);

            if (ContainerType != TaeFileContainerType.TAE)
            {
                var nameBase = Utils.GetFileNameWithoutAnyExtensions(file);
                var folder = new System.IO.FileInfo(file).DirectoryName;

                if (nameBase.EndsWith("c0000"))
                {
                    LoadingTaskMan.DoLoadingTaskSynchronous("c0000_ANIBND", "Loading additional player animations...", innerProgress =>
                    {
                        var anibndFiles = System.IO.Directory.GetFiles(folder, "c0000_*.anibnd*");
                        double i = 0;
                        foreach (var additionalAnibnd in anibndFiles)
                        {
                            innerProgress.Report(++i / anibndFiles.Length);
                            if (BND3.Is(additionalAnibnd))
                            {
                                var additionalContainerBND3 = BND3.Read(additionalAnibnd);
                                foreach (var f in additionalContainerBND3.Files)
                                {
                                    CheckGameVersionForTaeInterop(f.Name);
                                    if (f.Name.ToUpper().EndsWith(".HKX"))
                                    {
                                        if (!hkxInBND.ContainsKey(f.Name))
                                            hkxInBND.Add(f.Name, f.Bytes);
                                    }
                                }
                            }
                            else if (BND4.Is(additionalAnibnd))
                            {
                                var additionalContainerBND4 = BND4.Read(additionalAnibnd);
                                foreach (var f in additionalContainerBND4.Files)
                                {
                                    CheckGameVersionForTaeInterop(f.Name);

                                    if (f.Name.ToUpper().EndsWith(".HKX"))
                                    {
                                        if (!hkxInBND.ContainsKey(f.Name))
                                            hkxInBND.Add(f.Name, f.Bytes);
                                    }
                                }
                            }
                            
                        }
                        innerProgress.Report(1);
                    });
                }
            }

            progress.Report(0.5);

            filePath = file;

            //SFTODO
            ReloadType = TaeFileContainerReloadType.None;
        }

        public void SaveToPath(string file, IProgress<double> progress)
        {
            file = file.ToUpper();

            if (ContainerType == TaeFileContainerType.BND3)
            {
                double i = 0;
                foreach (var f in containerBND3.Files)
                {
                    progress.Report((++i / containerBND3.Files.Count) * 0.9);
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

                containerBND3.Write(file);

                progress.Report(1.0);
            }
            else if (ContainerType == TaeFileContainerType.BND4)
            {
                double i = 0;
                foreach (var f in containerBND4.Files)
                {
                    progress.Report((++i / containerBND4.Files.Count) * 0.9);
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

                containerBND4.Write(file);

                progress.Report(1.0);
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
