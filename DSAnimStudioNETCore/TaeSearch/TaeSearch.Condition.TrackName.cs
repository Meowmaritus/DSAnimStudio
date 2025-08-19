using System;
using System.Collections.Generic;
using ImGuiNET;

namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public abstract partial class Condition
        {
            public class TrackName : Condition
            {
                public override ConditionType Type => ConditionType.TrackName;
                public override ConditionDepthType Depth => ConditionDepthType.Category;
                public override bool IsStringOperator => true;

                public override object GetDefaultOperand() => "";
                
                public override List<OperatorType> AvailableOperators => new()
                {
                    OperatorType.StringIs,
                    OperatorType.StringIsNot,
                    OperatorType.StringContains,
                    OperatorType.StringMatchesRegex,
                };
                
                protected override void InnerShowImGuiControlForOperand(DSAProj proj, ref bool anyFieldFocused)
                {
                    string actualOperand = Convert.ToString(Operand);
                    ImGui.InputText("##Operand", ref actualOperand, 1024);
                    if (ImGui.IsItemActive())
                        anyFieldFocused = true;
                    Operand = actualOperand;
                }

                public override bool Evaluate(DSAProj proj, Check check)
                {
                    string a = check.Track.Info.DisplayName;
                    if (a == null || Operand == null)
                        return false;
                    string b = Convert.ToString(Operand);

                    if (!check.StringIsCaseSensitive)
                    {
                        a = a.ToLowerInvariant();
                        b = b.ToLowerInvariant();
                    }
                    
                    return OperatorEvaluator.EvaluateString(a, Operator, b);
                }
            }
        }
    }
}