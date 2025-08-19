using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public partial class DSAProj
    {
        [Flags]
        public enum EditorFlags
        {
            None = 0,
            NeedsResave_LegacySaveWithEventGroupsStripped = 1 << 0,
            NeedsResave_DSAProjVersionOutdated = 1 << 1,
            NeedsResave_NullAnimNameBug = 1 << 2,

            Combo_AllNeedsResaveFlags = NeedsResave_DSAProjVersionOutdated | NeedsResave_LegacySaveWithEventGroupsStripped | NeedsResave_NullAnimNameBug,
        }

    }
}
