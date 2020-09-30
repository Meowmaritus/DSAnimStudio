using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.FLVER2;
using static SoulsAssetPipeline.FLVERImportHelpers;
using System.Xml;

namespace SoulsAssetPipeline
{
    public class FLVER2XmlVertLayoutMtd
    {
        public BufferLayout Layout = new BufferLayout();
        public Dictionary<TextureChannelSemantic, string> TextureChannelNames = new Dictionary<TextureChannelSemantic, string>();

        public FLVER2XmlVertLayoutMtd(XmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
