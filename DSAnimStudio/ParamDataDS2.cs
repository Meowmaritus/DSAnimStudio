using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class ParamDataDS2
    {

        public class ArmorParam : ParamData
        {
            public int ModelID;
            public override void Read(BinaryReaderEx br)
            {
                var start = br.Position;
                br.Position += 8;
                ModelID = br.ReadInt32();
            }
        }

        public class WeaponParam : ParamData
        {
            public int ModelID;
            public override void Read(BinaryReaderEx br)
            {
                var start = br.Position;
                br.Position += 4;
                ModelID = br.ReadInt32();
            }
        }




        private static List<bool> ReadBitmask(BinaryReaderEx br, int numBits)
        {
            List<bool> result = new List<bool>(numBits);
            var maskBytes = br.ReadBytes((int)Math.Ceiling(numBits / 8.0f));
            for (int i = 0; i < numBits; i++)
            {
                result.Add((maskBytes[i / 8] & (byte)(1 << (i % 8))) != 0);
            }
            return result;
        }
    }
}
