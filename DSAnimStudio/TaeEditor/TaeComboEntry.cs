using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeComboEntry
    {
        public string AnimID;
        public string Event0CancelType;

        public TaeComboEntry(string animID, string event0CancelType)
        {
            AnimID = animID;
            Event0CancelType = event0CancelType;
        }

        public override string ToString()
        {
            return $"{AnimID} [{Event0CancelType}]";
        }
    }
}
