using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewTaeContainer
    {
        public enum ContainerTypes
        {
            Player,
            Enemy,
        }
        public class TaeFileEntry
        {
            public enum BndFileTypes
            {
                None = 0,
                PlayerBaseAnibnd,
                EnemyAnibnd,
                ObjbndAnibnd,
                PartsbndAnibnd,
            }
            public BndFileTypes BndFileType;
            public string BndFile;
        }
        public ContainerTypes ContainerType;
        private List<TAE> Taes = new List<TAE>();
    }
}
