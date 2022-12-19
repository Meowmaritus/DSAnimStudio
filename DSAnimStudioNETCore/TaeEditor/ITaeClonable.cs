using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public interface ITaeClonable
    {
        object ToClone();
        object FromClone(object cloneObj);
    }
}
