using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using SoulsAssetPipeline.Animation;

namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public abstract partial class Condition
        {
            public abstract ConditionType Type { get; }
            public abstract ConditionDepthType Depth { get; }
            public abstract bool IsStringOperator { get; }

            public abstract object GetDefaultOperand();
            
            public virtual void InitImgui(DSAProj proj)
            {
                
            }
            
            public virtual bool Deprecated_IsUseableOnParameter(TAE.Template.ParamTypes paramType)
            {
                return true;
            }
            
            public abstract List<OperatorType> AvailableOperators { get; }

            public OperatorType Operator;
            public object Operand;

            protected abstract void InnerShowImGuiControlForOperand(DSAProj proj, ref bool anyFieldFocused);

            protected virtual void InnerShowImGuiControlForTypeCfg(DSAProj proj, ref bool anyFieldFocused)
            {
                // Default behavior: skip Action and Param columns
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
            }

            public void ShowImGuiControlForTypeCfg(DSAProj proj, ref bool anyFieldFocused)
            {
                bool innerAnyFieldFocused = false;
                ImGui.PushItemWidth(0);
                InnerShowImGuiControlForTypeCfg(proj, ref innerAnyFieldFocused);
                ImGui.PopItemWidth();
                if (innerAnyFieldFocused)
                    anyFieldFocused = true;
            }
            
            public void ShowImGuiControlForOperand(DSAProj proj, ref bool anyFieldFocused)
            {
                if (Operand == null)
                    Operand = GetDefaultOperand();
                bool innerAnyFieldFocused = false;
                ImGui.PushItemWidth(0);
                InnerShowImGuiControlForOperand(proj, ref innerAnyFieldFocused);
                ImGui.PopItemWidth();
                if (innerAnyFieldFocused)
                    anyFieldFocused = true;
            }

            public static Condition NewFromEnum(ConditionType type)
            {
                switch (type)
                {
                    case ConditionType.AnimationCategoryName: return new AnimationCategoryName();
                    case ConditionType.AnimationCategoryID: return new AnimationCategoryID();
                    case ConditionType.AnimationSubID: return new AnimationSubID();
                    case ConditionType.AnimationFullID: return new AnimationFullID();
                    case ConditionType.AnimationName: return new AnimationName();
                    case ConditionType.TrackID: return new TrackID();
                    case ConditionType.TrackName: return new TrackName();
                    case ConditionType.ActionID: return new ActionID();
                    case ConditionType.ActionName: return new ActionName();
                    case ConditionType.ParameterName: return new ParameterName();
                    case ConditionType.ParameterValue: return new ParameterValue();
                    case ConditionType.Debug_ActionOverlap: return new Debug_ActionOverlap();
                    case ConditionType.Debug_ActionOverlapFull: return new Debug_ActionOverlapFull();
                    default: throw new NotImplementedException();
                }
            }
            
            public abstract bool Evaluate(DSAProj proj, Check check);
        }
    }
}