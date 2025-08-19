using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;

namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public abstract partial class Condition
        {
            public class TagName : Condition
            {
                public override ConditionType Type => ConditionType.TagName;
                public override ConditionDepthType Depth => ConditionDepthType.CategoryAnimationTrackAction;
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
                    var tags = check.Act.Info.TagInstances.ToList();
                    foreach (var t in tags)
                    {
                        string a = t.Name;
                        if (a == null || Operand == null)
                            continue;
                        string b = Convert.ToString(Operand);

                        if (!check.StringIsCaseSensitive)
                        {
                            a = a.ToLowerInvariant();
                            b = b.ToLowerInvariant();
                        }

                        if (OperatorEvaluator.EvaluateString(a, Operator, b))
                            return true;
                    }

                    return false;
                }
            }
        }
    }
}