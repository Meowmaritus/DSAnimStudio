using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSAnimStudio.ParamData.AtkParam;
using static DSAnimStudio.ParamData.WepAbsorpPosParam;

namespace DSAnimStudio
{

    public abstract partial class ParamData
    {
        public int ID;
        public string Name;

        

        public string GetDisplayName()
        {
            return $"{ID}{(!string.IsNullOrWhiteSpace(Name) ? $": {Name}" : "")}";
        }

        public abstract void Read(BinaryReaderEx br);

        public enum PartSuffixType
        {
            None = 0,
            M = 1,
            L = 2,
            U = 3,
        }

        public enum EquipModelGenders : byte
        {
            Invalid = 255,
            UnisexUseA = 0,
            MaleOnlyUseM = 1,
            FemaleOnlyUseF = 2,
            BothGendersUseMF = 3,
            UnisexUseMForBoth = 4,
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

        private static List<bool> GetBitmask(BinaryReaderEx br, long offset, int numBits)
        {
            List<bool> result = new List<bool>(numBits);
            var maskBytes = br.GetBytes(offset, (int)Math.Ceiling(numBits / 8.0f));
            for (int i = 0; i < numBits; i++)
            {
                result.Add((maskBytes[i / 8] & (byte)(1 << (i % 8))) != 0);
            }
            return result;
        }
    }
}
