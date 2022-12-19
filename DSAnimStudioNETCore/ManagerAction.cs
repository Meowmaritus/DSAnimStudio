using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public abstract class ManagerAction
    {
        public abstract string ActionText { get; }
        public override string ToString() => ActionText;
        public class Add : ManagerAction
        {
            public string Name;
            public override string ActionText => $"Add {Name}";
        }
        public class Remove : ManagerAction
        {
            public string Name;
            public override string ActionText => $"Remove {Name}";
        }
        public class Clone : ManagerAction
        {
            public string NameSource;
            public string NameDest;
            public override string ActionText => $"Clone {NameSource} --> {NameDest}";
        }
        public class Rename : ManagerAction
        {
            public string NameSource;
            public string NameDest;
            public override string ActionText => $"Rename {NameSource} --> {NameDest}";
        }
    }
}
