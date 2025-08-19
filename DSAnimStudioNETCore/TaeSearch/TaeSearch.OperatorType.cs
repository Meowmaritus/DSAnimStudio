using System;

namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public enum OperatorType
        {
            Equals,
            NotEquals,
            GreaterThan,
            GreaterThanOrEquals,
            LessThan,
            LessThanOrEquals,
            
            StringIs,
            StringIsNot,
            StringContains,
            StringMatchesRegex,
            
            BoolIsTrue,
            BoolIsFalse,
        }

        public static string OperatorTypeToString(OperatorType op)
        {
            switch (op)
            {
                case OperatorType.Equals: return "==";
                case OperatorType.NotEquals: return "!=";
                case OperatorType.GreaterThan: return ">";
                case OperatorType.GreaterThanOrEquals: return ">=";
                case OperatorType.LessThan: return "<";
                case OperatorType.LessThanOrEquals: return "<=";
                case OperatorType.StringIs: return "is";
                case OperatorType.StringIsNot: return "is not";
                case OperatorType.StringContains: return "contains";
                case OperatorType.StringMatchesRegex: return "matches RegEx";
                case OperatorType.BoolIsTrue: return "is True";
                case OperatorType.BoolIsFalse: return "is False";
                
                default: throw new NotImplementedException();
            }
        }
    }
}