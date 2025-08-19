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
            public class Debug_ActionOverlap : Condition
            {
                public override ConditionType Type => ConditionType.Debug_ActionOverlap;
                public override ConditionDepthType Depth => ConditionDepthType.CategoryAnimationTrackAction;
                public override bool IsStringOperator => false;

                public override object GetDefaultOperand() => "";
                
                public override List<OperatorType> AvailableOperators => new()
                {
                    OperatorType.BoolIsTrue,
                    OperatorType.BoolIsFalse,
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
                    bool overlap = check.Anim.SAFE_GetActions().Any(x => x != check.Act && x.Overlaps(check.Act, perfectMatch: false));
                    if (Operator == OperatorType.BoolIsTrue)
                        return overlap == true;
                    else if (Operator == OperatorType.BoolIsFalse)
                        return overlap == false;

                    return false;
                }
            }
        }
    }
}