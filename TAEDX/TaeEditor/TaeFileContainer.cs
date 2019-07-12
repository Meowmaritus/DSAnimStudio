using MeowDSIO;
using MeowDSIO.DataFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.TaeEditor
{
    public class TaeFileContainer
    {
        public enum TaeFileContainerType
        {
            Tae,
            Anibnd,
            Remobnd,
            Objbnd,
        }

        public enum TaeFileContainerReloadType
        {
            None,
            CHR_PTDE,
            CHR_DS1R
        }

        public TaeFileContainerType ContainerType { get; private set; }
        private TAE dataTAE = null;
        private ANIBND dataANIBND = null;
        private EntityBND dataEntityBND = null;
        private REMOBND dataREMOBND = null;

        public bool IsModified = false;

        public TaeFileContainerReloadType ReloadType = TaeFileContainerReloadType.None;

        public bool IsDcx { get; private set; } = false;

        public static readonly string DefaultSaveFilter = "ANIBND|*.ANIBND|" +
                "OBJBND|*.OBJBND|" +
                "REMOBND|*.REMOBND|" +
                "TAE|*.TAE|" +
                "ANIBND DCX|*.ANIBND.DCX|" +
                "OBJBND DCX|*.OBJBND.DCX|" +
                "REMOBND DCX|*.REMOBND.DCX|" +
                "TAE DCX|*.TAE.DCX|" +
                "All Files|*.*";

        public string GetResaveFilter()
        {
            if (ContainerType == TaeFileContainerType.Anibnd)
                return !IsDcx ?
                    "ANIBND|*.ANIBND|ANIBND DCX|*.ANIBND.DCX" :
                    "ANIBND DCX|*.ANIBND.DCX|ANIBND|*.ANIBND";
            else if (ContainerType == TaeFileContainerType.Tae)
                return !IsDcx ?
                    "TAE|*.TAE|TAE DCX|*.TAE.DCX" :
                    "TAE DCX|*.TAE.DCX|TAE|*.TAE";
            else if (ContainerType == TaeFileContainerType.Remobnd)
                return !IsDcx ?
                    "REMOBND|*.REMOBND|REMOBND DCX|*.REMOBND.DCX" :
                    "REMOBND DCX|*.REMOBND.DCX|REMOBND|*.REMOBND";
            else if (ContainerType == TaeFileContainerType.Objbnd)
                return !IsDcx ?
                    "OBJBND|*.OBJBND|OBJBND DCX|*.OBJBND.DCX" :
                    "OBJBND DCX|*.OBJBND.DCX|OBJBND|*.OBJBND";
            else
                return null;
        }

        public Dictionary<int, TAE> StandardTAE { get; private set; } = null;
        public Dictionary<int, TAE> PlayerTAE { get; private set; } = null;

        public IEnumerable<TAE> AllTAE
        {
            get
            {
                if (ContainerType == TaeFileContainerType.Anibnd)
                    return dataANIBND.AllTAE;
                else if (ContainerType == TaeFileContainerType.Tae)
                    return new List<TAE> { dataTAE };
                else if (ContainerType == TaeFileContainerType.Remobnd)
                    return new List<TAE> { dataREMOBND.Tae };
                else if (ContainerType == TaeFileContainerType.Objbnd)
                    return dataEntityBND.GetAllTAE();
                else
                    return new List<TAE>();
            }
        }

        public void LoadFromPath(string file)
        {
            ReloadType = TaeFileContainerReloadType.None;

            StandardTAE = null;
            PlayerTAE = null;

            file = file.ToUpper();
            var fileNoDcx = file;
            IsDcx = false;
            if (file.EndsWith(".DCX"))
            {
                IsDcx = true;
                fileNoDcx = file.Substring(0, file.Length - 4);
            }

            if (fileNoDcx.EndsWith(".ANIBND"))
            {
                if (IsDcx)
                    dataANIBND = DataFile.LoadFromFile<ANIBND>(file, loadDcxVersion: true);
                else
                    dataANIBND = DataFile.LoadFromFile<ANIBND>(fileNoDcx);

                StandardTAE = dataANIBND.StandardTAE;
                PlayerTAE = dataANIBND.PlayerTAE;

                if (dataANIBND.IsRemaster)
                    ReloadType = TaeFileContainerReloadType.CHR_DS1R;
                else
                    ReloadType = TaeFileContainerReloadType.CHR_PTDE;

                ContainerType = TaeFileContainerType.Anibnd;
            }
            else if (fileNoDcx.EndsWith(".OBJBND"))
            {
                if (IsDcx)
                    dataEntityBND = DataFile.LoadFromFile<EntityBND>(file, loadDcxVersion: true);
                else
                    dataEntityBND = DataFile.LoadFromFile<EntityBND>(fileNoDcx);

                StandardTAE = new Dictionary<int, TAE>();
                PlayerTAE = new Dictionary<int, TAE>();

                foreach (var m in dataEntityBND.Models)
                {
                    if (m.AnimContainer != null)
                    {
                        foreach (var tae in m.AnimContainer.StandardTAE)
                        {
                            StandardTAE.Add(tae.Key, tae.Value);
                        }
                        foreach (var tae in m.AnimContainer.PlayerTAE)
                        {
                            PlayerTAE.Add(tae.Key, tae.Value);
                        }
                    }
                  
                }

                ContainerType = TaeFileContainerType.Objbnd;
            }
            else if (file.EndsWith(".REMOBND"))
            {
                if (IsDcx)
                    dataREMOBND = DataFile.LoadFromFile<REMOBND>(file, loadDcxVersion: true);
                else
                    dataREMOBND = DataFile.LoadFromFile<REMOBND>(fileNoDcx);
                ContainerType = TaeFileContainerType.Remobnd;
            }
            else if (fileNoDcx.EndsWith(".TAE"))
            {
                if (IsDcx)
                    dataTAE = DataFile.LoadFromFile<TAE>(file, loadDcxVersion: true);
                else
                    dataTAE = DataFile.LoadFromFile<TAE>(fileNoDcx);
                ContainerType = TaeFileContainerType.Tae;
            }
        }

        public void SaveToPath(string file)
        {
            file = file.ToUpper();
            IsDcx = false;
            if (file.EndsWith(".DCX"))
            {
                IsDcx = true;
            }

            if (ContainerType == TaeFileContainerType.Anibnd)
            {
                if (IsDcx)
                    DataFile.SaveToFile(dataANIBND, file);
                else
                    DataFile.SaveToFile(dataANIBND, file);
            }
            else if (ContainerType == TaeFileContainerType.Objbnd)
            {
                if (IsDcx)
                    DataFile.SaveToFile(dataEntityBND, file);
                else
                    DataFile.SaveToFile(dataEntityBND, file);
            }
            else if (ContainerType == TaeFileContainerType.Remobnd)
            {
                if (IsDcx)
                    DataFile.SaveToFile(dataREMOBND, file);
                else
                    DataFile.SaveToFile(dataREMOBND, file);
            }
            else if (ContainerType == TaeFileContainerType.Tae)
            {
                if (IsDcx)
                    DataFile.SaveToFile(dataTAE, file);
                else
                    DataFile.SaveToFile(dataTAE, file);
            }
        }
    }
}
