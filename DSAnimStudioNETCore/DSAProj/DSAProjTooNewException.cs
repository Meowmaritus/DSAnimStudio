using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    internal class DSAProjTooNewException : Exception
    {
        public DSAProjTooNewException(DSAProj.Versions maxSupportedVersion, DSAProj.Versions fileVersion) 
            : base($"*{DSAProj.EXT} file format version is {fileVersion}, which is higher than what this version of DS Anim Studio supports ({maxSupportedVersion}).")
        { 

        }
    }
}
