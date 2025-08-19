using System;
using System.Collections.Generic;
using ImGuiNET;

namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public abstract partial class Condition
        {
            public class TrackID : Condition
            {
                public override ConditionType Type => ConditionType.TrackID;
                public override ConditionDepthType Depth => ConditionDepthType.CategoryAnimationTrack;
                public override bool IsStringOperator => false;

                public override object GetDefaultOperand() => (int)0;
                
                public override List<OperatorType> AvailableOperators => new()
                {
                    OperatorType.Equals,
                    OperatorType.NotEquals,
                    OperatorType.GreaterThan,
                    OperatorType.LessThan,
                    OperatorType.GreaterThanOrEquals,
                    OperatorType.LessThanOrEquals
                };
                
                protected override void InnerShowImGuiControlForOperand(DSAProj proj, ref bool anyFieldFocused)
                {
                    int actualOperand = Convert.ToInt32(Operand);
                    ImGui.InputInt("##Operand", ref actualOperand, 0, 0);
                    if (ImGui.IsItemActive())
                        anyFieldFocused = true;
                    Operand = actualOperand;
                }

                public override bool Evaluate(DSAProj proj, Check check)
                {
                    int actualOperand = Convert.ToInt32(Operand);
                    return OperatorEvaluator.EvaluateInteger(check.Track.TrackType, Operator, actualOperand);
                }
            }
        }
    }
}