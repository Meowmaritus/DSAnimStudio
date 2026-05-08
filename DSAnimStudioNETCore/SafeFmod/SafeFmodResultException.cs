
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.SafeFmod
{
    public class SafeFmodResultException : Exception
    {
        public SafeFmodResultException(string message, FMOD.RESULT res)
            : base(GetMessageString(message, res))
        {
            
        }

        public static string GetMessageString(string message, FMOD.RESULT res)
        {
            return $"{message}\n\nFMOD result '{res}': '{FMOD.Error.String(res)}'";
        }
    }
}
