using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public partial class DSAProj
    {

        public class ClipboardContents
        {
            public DSAProj.Versions FILE_VERSION = DSAProj.LATEST_FILE_VERSION;

            public List<Action> Actions = new List<Action>();

            public void Deserialize(BinaryReaderEx br, DSAProj proj)
            {
                br.AssertASCII("DSCB");
                FILE_VERSION = (DSAProj.Versions)br.ReadInt32();
                int actionCount = br.ReadInt32();
                Actions.Clear();
                for (int i = 0; i < actionCount; i++)
                {
                    var act = new Action();
                    act.Deserialize(br, proj);
                    Actions.Add(act);
                }
            }

            public void Serialize(BinaryWriterEx bw, DSAProj proj)
            {
                bw.WriteASCII("DSCB");
                bw.WriteInt32((int)LATEST_FILE_VERSION);
                bw.WriteInt32(Actions.Count);
                foreach (var act in Actions)
                {
                    act.Serialize(bw, proj);
                }
            }


            public string SerializeToBytesString(bool dcx, DSAProj proj)
            {
                var result = SerializeToBytes(dcx, proj);
                return string.Join(" ", result.Select(e => e.ToString("X2")));
            }

            public void DeserializeFromBytesString(string bytesString, bool dcx, DSAProj proj)
            {
                var bytes = bytesString.Split(" ").Select(e => byte.Parse(e, System.Globalization.NumberStyles.HexNumber)).ToArray();
                DeserializeFromBytes(bytes, dcx, proj);
            }

            public byte[] SerializeToBytes(bool dcx, DSAProj proj)
            {
                var bw = new BinaryWriterEx(false);
                Serialize(bw, proj);
                var result = bw.FinishBytes();
                if (dcx)
                    result = DCX.Compress(result, DCX.Type.DCX_DFLT_10000_24_9);
                return result;
            }

            public void DeserializeFromBytes(byte[] data, bool dcx, DSAProj proj)
            {
                if (dcx)
                    data = DCX.Decompress(data);
                var br = new BinaryReaderEx(false, data);
                Deserialize(br, proj);

                if (proj != null && proj.Template != null)
                {
                    foreach (var act in Actions)
                    {
                        if (proj.Template.ContainsKey(act.Type))
                        {
                            act.ApplyTemplate(proj.Template[act.Type]);
                        }
                    }
                }

            }

        }


    }
}
