using SoulsFormats;
using System.Collections.Generic;
using System.Linq;

namespace TAEDX.TaeEditor
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

        public bool IsDcx { get; private set; } = false;

        public static readonly string DefaultSaveFilter = ".(ANI)BND(.DCX)|*.*BND*|" +
                ".TAE(.DCX)|*.TAE*|" +
                "All Files|*.*";

        public string GetResaveFilter()
        {
            return "*.*|*.*";
        }

        //public Dictionary<int, TAE> StandardTAE { get; private set; } = null;
        //public Dictionary<int, TAE> PlayerTAE { get; private set; } = null;

        public IEnumerable<TAE> AllTAE => taeInBND.Values;

        public IReadOnlyDictionary<string, byte[]> AllHKXDict => hkxInBND;

        public IReadOnlyDictionary<string, TAE> AllTAEDict => taeInBND;

        public void LoadFromPath(string file)
        {
            ReloadType = TaeFileContainerReloadType.None;

            containerBND3 = null;
            containerBND4 = null;

            taeInBND.Clear();
            hkxInBND.Clear();

            if (BND3.Is(file))
            {
                ContainerType = TaeFileContainerType.BND3;
                containerBND3 = BND3.Read(file);
                foreach (var f in containerBND3.Files)
                {
                    if (TAE.Is(f.Bytes))
                    {
                        taeInBND.Add(f.Name, TAE.Read(f.Bytes));
                    }
                    else if (f.Name.ToUpper().EndsWith(".HKX"))
                    {
                        hkxInBND.Add(f.Name, f.Bytes);
                    }
                }
            }
            else if (BND4.Is(file))
            {
                ContainerType = TaeFileContainerType.BND4;
                containerBND4 = BND4.Read(file);
                foreach (var f in containerBND4.Files)
                {
                    if (TAE.Is(f.Bytes))
                    {
                        taeInBND.Add(f.Name, TAE.Read(f.Bytes));
                    }
                    else if (f.Name.ToUpper().EndsWith(".HKX"))
                    {
                        hkxInBND.Add(f.Name, f.Bytes);
                    }
                }
            }
            else if (TAE.Is(file))
            {
                ContainerType = TaeFileContainerType.TAE;
                taeInBND.Add(file, TAE.Read(file));
            }

            filePath = file;

            //SFTODO
            ReloadType = TaeFileContainerReloadType.None;
        }

        public void SaveToPath(string file)
        {
            file = file.ToUpper();
            IsDcx = false;
            if (file.EndsWith(".DCX"))
            {
                IsDcx = true;
            }

            if (ContainerType == TaeFileContainerType.BND3)
            {
                foreach (var f in containerBND3.Files)
                {
                    if (taeInBND.ContainsKey(f.Name))
                        f.Bytes = taeInBND[f.Name].Write();
                }

                containerBND3.Write(file);
            }
            else if (ContainerType == TaeFileContainerType.BND4)
            {
                foreach (var f in containerBND4.Files)
                {
                    if (taeInBND.ContainsKey(f.Name))
                        f.Bytes = taeInBND[f.Name].Write();
                }

                containerBND4.Write(file);
            }
            else if (ContainerType == TaeFileContainerType.TAE)
            {
                var tae = taeInBND[filePath];
                tae.Write(file);

                taeInBND.Clear();
                taeInBND.Add(file, taeInBND[filePath]);
            }
        }
    }
}
