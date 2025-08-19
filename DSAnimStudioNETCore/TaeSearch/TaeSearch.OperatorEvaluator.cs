using System;
using System.Text.RegularExpressions;

namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public static class OperatorEvaluator
        {
            public static bool EvaluateInteger(long a, OperatorType op, long b)
            {
                switch (op)
                {
                    case OperatorType.Equals: return a == b;
                    case OperatorType.NotEquals: return a != b;
                    case OperatorType.GreaterThan: return a > b;
                    case OperatorType.GreaterThanOrEquals: return a >= b;
                    case OperatorType.LessThan: return a < b;
                    case OperatorType.LessThanOrEquals: return a <= b;
                    
                    case OperatorType.StringIs:
                    case OperatorType.StringIsNot:
                    case OperatorType.StringContains:
                    case OperatorType.StringMatchesRegex:
                        throw new InvalidOperationException($"Cannot use string operations on integers.");
                    
                    case OperatorType.BoolIsTrue:
                    case OperatorType.BoolIsFalse:
                        throw new InvalidOperationException($"Cannot use boolean operations on integers.");
                }

                throw new NotImplementedException();
            }
            
            public static bool EvaluateIntegerU64(ulong a, OperatorType op, ulong b)
            {
                switch (op)
                {
                    case OperatorType.Equals: return a == b;
                    case OperatorType.NotEquals: return a != b;
                    case OperatorType.GreaterThan: return a > b;
                    case OperatorType.GreaterThanOrEquals: return a >= b;
                    case OperatorType.LessThan: return a < b;
                    case OperatorType.LessThanOrEquals: return a <= b;
                    
                    case OperatorType.StringIs:
                    case OperatorType.StringIsNot:
                    case OperatorType.StringContains:
                    case OperatorType.StringMatchesRegex:
                        throw new InvalidOperationException($"Cannot use string operations on integers.");
                    
                    case OperatorType.BoolIsTrue:
                    case OperatorType.BoolIsFalse:
                        throw new InvalidOperationException($"Cannot use boolean operations on integers.");
                }

                throw new NotImplementedException();
            }

            public static bool EvaluateBool(bool a, OperatorType op)
            {
                switch (op)
                {
                    case OperatorType.Equals:
                    case OperatorType.NotEquals:
                    case OperatorType.GreaterThan:
                    case OperatorType.GreaterThanOrEquals:
                    case OperatorType.LessThan:
                    case OperatorType.LessThanOrEquals:
                        throw new InvalidOperationException($"Cannot use numeric operations on booleans.");
                    case OperatorType.StringIs:
                    case OperatorType.StringIsNot:
                    case OperatorType.StringContains:
                    case OperatorType.StringMatchesRegex:
                        throw new InvalidOperationException($"Cannot use string operations on booleans.");
                    case OperatorType.BoolIsTrue: return a == true;
                    case OperatorType.BoolIsFalse: return a == false;
                }
                
                throw new NotImplementedException();
            }

            public static bool EvaluateFloat(double a, OperatorType op, double b)
            {
                switch (op)
                {
                    case OperatorType.Equals: return a == b;
                    case OperatorType.NotEquals: return a != b;
                    case OperatorType.GreaterThan: return a > b;
                    case OperatorType.GreaterThanOrEquals: return a >= b;
                    case OperatorType.LessThan: return a < b;
                    case OperatorType.LessThanOrEquals: return a <= b;
                    case OperatorType.StringIs:
                    case OperatorType.StringIsNot:
                    case OperatorType.StringContains:
                    case OperatorType.StringMatchesRegex:
                        throw new InvalidOperationException($"Cannot use string operations on floating-point numbers.");
                    case OperatorType.BoolIsTrue:
                    case OperatorType.BoolIsFalse:
                        throw new InvalidOperationException($"Cannot use boolean operations on floating-point numbers.");
                }

                throw new NotImplementedException();
            }

            public static bool EvaluateString(string a, OperatorType op, string b)
            {
                switch (op)
                {
                    case OperatorType.StringIs: return a.Trim() == b.Trim();
                    case OperatorType.StringIsNot: return a.Trim() != b.Trim();
                    case OperatorType.StringContains: return a.Trim().Contains(b.Trim());
                    case OperatorType.StringMatchesRegex: return Regex.IsMatch(a, b);
                    
                    case OperatorType.Equals:
                    case OperatorType.NotEquals:
                    case OperatorType.GreaterThan:
                    case OperatorType.GreaterThanOrEquals:
                    case OperatorType.LessThan:
                    case OperatorType.LessThanOrEquals:
                        throw new InvalidOperationException($"Cannot use numeric operation on strings.");
                    case OperatorType.BoolIsTrue:
                    case OperatorType.BoolIsFalse:
                        throw new InvalidOperationException($"Cannot use boolean operations on strings.");
                    
                    default: throw new NotImplementedException();
                }
            }
        }
    }
}