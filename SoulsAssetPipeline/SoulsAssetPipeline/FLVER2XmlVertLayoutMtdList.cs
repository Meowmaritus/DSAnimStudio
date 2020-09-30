using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static SoulsFormats.FLVER2;

namespace SoulsAssetPipeline
{
    public class FLVER2XmlVertLayoutMtdList
    {
        public Dictionary<string, BufferLayout> BufferLayoutsPerMTD = new Dictionary<string, BufferLayout>();

        public FLVER2XmlVertLayoutMtdList(XmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
