using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSAnimStudio.TaeEditor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsFormats;

namespace DSAnimStudio
{
    public partial class DSAProj
    {
        public class Tag
        {
            public string GUID = Guid.NewGuid().ToString();

            public string Name;
            public Color? CustomColor;

            public Tag()
            {

            }

            public Tag(string name, Color? customColor = null)
            {
                Name = name;
                CustomColor = customColor;
            }

            public void Deserialize(BinaryReaderEx br, DSAProj proj)
            {
                Name = br.ReadUTF16();
                CustomColor = br.ReadNullPrefixColor();
            }

            public void Serialize(BinaryWriterEx bw)
            {
                bw.WriteUTF16(Name, true);
                bw.WriteNullPrefixColor(CustomColor);
            }
        }
    }
}
