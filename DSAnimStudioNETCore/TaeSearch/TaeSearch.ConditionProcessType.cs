using System;

namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public enum ConditionProcessType
        {
            AllTrue,
            AnyTrue,
        }

        public static string ConditionProcessTypeToString(ConditionProcessType c)
        {
            switch (c)
            {
                case ConditionProcessType.AllTrue: return "All Are True";
                case ConditionProcessType.AnyTrue: return "Any Are True";
                default: throw new NotImplementedException();
            }
        }
    }
}