using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeComboEntry
    {
        public TaeComboMenu.TaeComboAnimType ComboType;
        public int AnimID;
        public int Event0CancelType;

        public TaeComboEntry(TaeComboMenu.TaeComboAnimType comboType, int animID, int event0CancelType)
        {
            ComboType = comboType;
            AnimID = animID;
            Event0CancelType = event0CancelType;
        }

        public override string ToString()
        {
            return $"{ComboType} {AnimID} [Event0CancelType: {Event0CancelType}]";
        }
    }
}
