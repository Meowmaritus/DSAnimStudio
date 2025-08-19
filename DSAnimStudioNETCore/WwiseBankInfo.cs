using Microsoft.Xna.Framework;
using NAudio.Wave;
using SoulsAssetPipeline.Audio.Wwise;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;

namespace DSAnimStudio
{
    public class WwiseBankInfo
    {
        public uint BankNameHash;
        public string BankName;
        public WwiseBNK Bank;
        public bool IncludedInLookup;

    }
}