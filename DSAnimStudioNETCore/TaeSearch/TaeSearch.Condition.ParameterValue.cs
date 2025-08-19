using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DSAnimStudio.ImguiOSD;
using ImGuiNET;
using SoulsAssetPipeline.Animation;

namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public abstract partial class Condition
        {
            public class ParameterValue : Condition
            {
                public TAE.Template.ParamTypes ParameterType { get; protected set; }

                public override ConditionType Type => ConditionType.ParameterValue;
                // Not the CategoryAnimationTrackActionParameter depth because it checks at the Action level, reading its entire Parameter collection.
                public override ConditionDepthType Depth => ConditionDepthType.CategoryAnimationTrackAction;
                public override bool IsStringOperator => false;

                private int ActionType = -1;
                private List<int> ActionTypes_Values;
                private string[] ActionTypes_Names;
                
                private int ParamIndex = -1;
                private ParameterRef? ParamRef;

                private TAE.Template.ActionTemplate ActionTemplate;
                private TAE.Template.ParameterTemplate ParamTemplate;
                
                private string[] ParameterNames;
                private List<ParameterRef> ParameterRefs;
                private List<TAE.Template.ParamTypes> ParameterTypes;

                public override object GetDefaultOperand()
                {
                    switch (ParameterType)
                    {
                        case TAE.Template.ParamTypes.b: return false;
                        case TAE.Template.ParamTypes.f32: return (float)0;
                        case TAE.Template.ParamTypes.f32grad: return System.Numerics.Vector2.Zero;
                        case TAE.Template.ParamTypes.s8: return (sbyte)0;
                        case TAE.Template.ParamTypes.u8: return (byte)0;
                        case TAE.Template.ParamTypes.x8: return (byte)0;
                        case TAE.Template.ParamTypes.s16: return (short)0;
                        case TAE.Template.ParamTypes.u16: return (ushort)0;
                        case TAE.Template.ParamTypes.x16: return (ushort)0;
                        case TAE.Template.ParamTypes.s32: return (int)0;
                        case TAE.Template.ParamTypes.u32: return (uint)0;
                        case TAE.Template.ParamTypes.x32: return (uint)0;
                        case TAE.Template.ParamTypes.s64: return (long)0;
                        case TAE.Template.ParamTypes.u64: return (ulong)0;
                        case TAE.Template.ParamTypes.x64: return (ulong)0;
                        default: throw new NotImplementedException();
                    }
                    
                    
                }

                public override List<OperatorType> AvailableOperators
                {
                    get
                    {
                        var paramType = ParameterType;
                        if (paramType is TAE.Template.ParamTypes.b)
                        {
                            return new()
                            {
                                OperatorType.BoolIsTrue,
                                OperatorType.BoolIsFalse,
                            };
                        }
                        else
                        {
                            return new()
                            {
                                OperatorType.Equals,
                                OperatorType.NotEquals,
                                OperatorType.GreaterThan,
                                OperatorType.LessThan,
                                OperatorType.GreaterThanOrEquals,
                                OperatorType.LessThanOrEquals
                            };
                        }
                    }
                }

                public override bool Deprecated_IsUseableOnParameter(TAE.Template.ParamTypes paramType)
                {
                    return paramType is TAE.Template.ParamTypes.b;
                }

                protected override void InnerShowImGuiControlForTypeCfg(DSAProj proj, ref bool anyFieldFocused)
                {
                     if (ActionTypes_Values == null || ActionTypes_Names == null)
                    {
                        ActionTypes_Values = new List<int>();
                        var actionTypeNames = new List<string>();
                        foreach (var kvp in proj.Template)
                        {
                            ActionTypes_Values.Add((int)kvp.Key);
                            actionTypeNames.Add($"{kvp.Key}: {kvp.Value.Name}");
                        }

                        ActionTypes_Names = actionTypeNames.ToArray();
                    }

                    int actionTypeIndex = (ActionType >= 0) ? ActionTypes_Values.IndexOf(ActionType) : -1;
                    int prevActionTypeIndex = actionTypeIndex;
                    ImGui.Combo("##Action", ref actionTypeIndex, ActionTypes_Names, ActionTypes_Names.Length);
                    
                    if (ImGui.IsItemActive())
                        anyFieldFocused = true;
                    
                    if (actionTypeIndex != prevActionTypeIndex || ParameterNames == null || ParameterRefs == null || ParameterTypes == null)
                    {
                        Operand = null;
                        
                        if (actionTypeIndex >= 0 && actionTypeIndex < ActionTypes_Values.Count)
                        {
                            
                            
                            
                            ActionType = ActionTypes_Values[actionTypeIndex];
                            
                            ActionTemplate = proj.Template[ActionType];
                            
                            var parameterNames = new List<string>();
                            ParameterRefs = new List<ParameterRef>();
                            ParameterTypes = new List<TAE.Template.ParamTypes>();
                            var actTemplate = proj.Template[ActionType];

                            foreach (var p in actTemplate)
                            {
                                if (p.ParamType == TAE.Template.ParamTypes.f32grad)
                                {
                                    parameterNames.Add(p.Name + ".Start");
                                    ParameterRefs.Add(new ParameterRef() { ParameterName = p.Name, Subtype = ParameterRef.ParameterSubtype.f32gradStart });
                                    parameterNames.Add(p.Name + ".End");
                                    ParameterRefs.Add(new ParameterRef() { ParameterName = p.Name, Subtype = ParameterRef.ParameterSubtype.f32gradEnd });
                                    
                                    //Pepega
                                    ParameterTypes.Add(p.ParamType);
                                    ParameterTypes.Add(p.ParamType);
                                }
                                else
                                {
                                    parameterNames.Add(p.Name);
                                    ParameterRefs.Add(new ParameterRef() { ParameterName = p.Name });
                                    ParameterTypes.Add(p.ParamType);
                                }
                                
                                
                            }

                            ParameterNames = parameterNames.ToArray();
                        }
                        else
                        {
                            ActionTemplate = null;
                            ActionType = -1;
                            ParameterNames = null;
                            ParameterRefs = null;
                            ParameterTypes = null;
                        }
                        ParamRef = null;
                        ParamIndex = -1;
                        ParamTemplate = null;
                        
                    }

                    ImGui.TableNextColumn();
                    if (ActionType >= 0 && ParameterNames != null && ParameterRefs != null && ParameterTypes != null)
                    {
                        var paramIndex = ParamRef.HasValue ? ParameterRefs.IndexOf(ParamRef.Value) : -1;
                        int prevParamIndex = paramIndex;
                        
                        ImGui.PushItemWidth(0);
                        ImGui.Combo("##Param", ref paramIndex, ParameterNames, ParameterNames.Length);
                        ImGui.PopItemWidth();
                        if (ImGui.IsItemActive())
                            anyFieldFocused = true;

                        if (paramIndex != prevParamIndex)
                        {
                            Operand = null;
                            if (paramIndex >= 0 && paramIndex < ParameterNames.Length)
                            {
                                ParamRef = ParameterRefs[paramIndex];
                                ParamIndex = proj.Template[ActionType].IndexOf(proj.Template[ActionType]
                                    .First(x => x.Name == ParamRef.Value.ParameterName));
                                ParameterType = ParameterTypes[paramIndex];
                                ParamTemplate = ActionTemplate[ParamIndex];
                            }
                            else
                            {
                                ParamRef = null;
                                ParamIndex = -1;
                                ParamTemplate = null;
                            }
                            
                            
                        }
                    }
                    else
                    {
                        ParamRef = null;
                    }
                    
                    ImGui.TableNextColumn();
                }

                protected override void InnerShowImGuiControlForOperand(DSAProj proj, ref bool anyFieldFocused)
                {
                    int ClampInt(int v, int min, int max)
                    {
                        v = Math.Max(v, min);
                        v = Math.Min(v, max);
                        return v;
                    }
                    
                    if (ParamTemplate == null)
                        return;
                    
                    var p = ParamTemplate;
                    
                    bool isEnum = p.EnumEntries != null && p.EnumEntries.Count > 0;

                    bool intSigned = p.ParamType == TAE.Template.ParamTypes.s8 || p.ParamType == TAE.Template.ParamTypes.s16 || p.ParamType == TAE.Template.ParamTypes.s32;
                    bool intUnsigned = p.ParamType == TAE.Template.ParamTypes.u8 || p.ParamType == TAE.Template.ParamTypes.u16 || p.ParamType == TAE.Template.ParamTypes.u32;
                    bool intHex = p.ParamType == TAE.Template.ParamTypes.x8 || p.ParamType == TAE.Template.ParamTypes.x16 || p.ParamType == TAE.Template.ParamTypes.x32;

                    if (intSigned || intUnsigned || intHex)
                    {
                        int currentVal = Convert.ToInt32(Operand);
                        int prevVal = currentVal;

                        if (isEnum)
                        {
                            //p.EnsureEnumEntry(currentVal);

                            string[] items = p.EnumEntries.Select(e => e.Key).ToArray();
                            string[] dispItems = new string[items.Length];

                            

                            int currentItemIndex = -1;
                            for (int i = 0; i < items.Length; i++)
                            {
                                if (Convert.ToInt32(p.EnumEntries[i].Value) == currentVal)
                                    currentItemIndex = i;

                                if (ActionType == 0 && items[i].Contains("|"))
                                {
                                    var allArgsSplit = items[i].Split('|').Select(x => x.Trim()).ToList();
                                    dispItems[i] = allArgsSplit[0];
                                    for (int j = 1; j < allArgsSplit.Count; j++)
                                    {
                                        var argSplit = allArgsSplit[j].Split(':').Select(x => x.Trim()).ToList();
                                        var argParameter = argSplit[0];
                                        var argName = argSplit[1];
                                    }
                                }
                                else
                                {
                                    dispItems[i] = items[i];
                                }
                            }
                            
                            
                            //ImGui.PushItemWidth(24);
                            int prevItemIndex = currentItemIndex;
                            ImGui.Combo($"###Operand_Enum", ref currentItemIndex, dispItems, items.Length);
                            //ImGui.PopItemWidth();
                            if (currentItemIndex != prevItemIndex && currentItemIndex >= 0 && currentItemIndex < items.Length)
                                currentVal = Convert.ToInt32(p.EnumEntries[currentItemIndex].Value);
                            
                            ImGui.SameLine();
                        }
                        
                        
                        if (isEnum)
                            ImGui.PushItemWidth(100);
                        
                        
                        ImGui.InputInt($"###Operand_IntVal", ref currentVal, 1, 5, intHex ? (ImGuiInputTextFlags.CharsHexadecimal |
                            ImGuiInputTextFlags.CharsUppercase) : ImGuiInputTextFlags.None);
                        anyFieldFocused |= ImGui.IsItemActive();
                        
                        if (isEnum)
                            ImGui.PopItemWidth();

                        if (currentVal != prevVal)
                        {
                            if (p.ParamType is TAE.Template.ParamTypes.u8 or TAE.Template.ParamTypes.x8)
                                Operand = (byte)ClampInt(currentVal, byte.MinValue, byte.MaxValue);
                            
                            else if (p.ParamType is TAE.Template.ParamTypes.s8)
                                Operand = (sbyte)ClampInt(currentVal, sbyte.MinValue, sbyte.MaxValue);
                            
                            else if (p.ParamType is TAE.Template.ParamTypes.s16)
                                Operand = (short)ClampInt(currentVal, short.MinValue, short.MaxValue);
                            
                            else if (p.ParamType is TAE.Template.ParamTypes.u16 or TAE.Template.ParamTypes.x16)
                                Operand = (ushort)ClampInt(currentVal, ushort.MinValue, ushort.MaxValue);
                            
                            else if (p.ParamType is TAE.Template.ParamTypes.s32)
                                Operand = (int)ClampInt(currentVal, int.MinValue, int.MaxValue);
                            
                            else if (p.ParamType is TAE.Template.ParamTypes.u32 or TAE.Template.ParamTypes.x32)
                                Operand = (uint)ClampInt(currentVal, (int)uint.MinValue, int.MaxValue);
                            
                            else if (p.ParamType is TAE.Template.ParamTypes.s64)
                                Operand = (long)currentVal;
                            
                            else if (p.ParamType is TAE.Template.ParamTypes.u64 or TAE.Template.ParamTypes.x64)
                                Operand = (ulong)currentVal;
                        }
                    }
                    else if (p.ParamType is TAE.Template.ParamTypes.f32 or TAE.Template.ParamTypes.f32grad)
                    {
                        float currentVal = Convert.ToSingle(Operand);
                        float prevVal = currentVal;
                        ImGui.InputFloat($"###Operand_Float", ref currentVal);
                        anyFieldFocused |= ImGui.IsItemActive();
                        if (currentVal != prevVal)
                            Operand = currentVal;
                    }
                    else if (p.ParamType is TAE.Template.ParamTypes.b)
                    {
                        bool currentVal = Convert.ToBoolean(Operand);
                        bool prevVal = currentVal;
                        ImGui.Checkbox($"###Operand_Bool", ref currentVal);
                        if (currentVal != prevVal)
                            Operand = currentVal;
                    }
                    
                    
                }

                public override bool Evaluate(DSAProj proj, Check check)
                {
                    if (check.Act.Type != ActionType)
                        return false;

                    if (ActionType < 0 || ParamRef == null || ParamTemplate == null)
                        return false;
                    
                    var paramTemplate = check.Proj.Template[ActionType]
                        .FirstOrDefault(x => x.Name == ParamRef.Value.ParameterName);
                    
                    var paramType = ParameterType;
                    if (paramType is TAE.Template.ParamTypes.b)
                    {
                        var a = Convert.ToBoolean(check.Act.Parameters[ParamIndex]);
                        return OperatorEvaluator.EvaluateBool(a, Operator);
                    }
                    else if (paramType is TAE.Template.ParamTypes.f32)
                    {
                        var a = Convert.ToSingle(check.Act.Parameters[ParamIndex]);
                        var b = Convert.ToSingle(Operand);
                        return OperatorEvaluator.EvaluateFloat(a, Operator, b);
                    }
                    else if (paramType is TAE.Template.ParamTypes.f32grad)
                    {
                        float a = 0;
                        if (ParamRef.Value.Subtype == ParameterRef.ParameterSubtype.f32gradStart)
                            a = ((System.Numerics.Vector2)(check.Act.Parameters[ParamIndex])).X;
                        else if (ParamRef.Value.Subtype == ParameterRef.ParameterSubtype.f32gradEnd)
                            a = ((System.Numerics.Vector2)(check.Act.Parameters[ParamIndex])).Y;
                        else
                            throw new InvalidOperationException();

                        var b = Convert.ToSingle(Operand);
                        return OperatorEvaluator.EvaluateFloat(a, Operator, b);
                    }
                    else if (paramType 
                             is TAE.Template.ParamTypes.s8 or TAE.Template.ParamTypes.u8 or TAE.Template.ParamTypes.x8 
                             or TAE.Template.ParamTypes.s16 or TAE.Template.ParamTypes.u16 or TAE.Template.ParamTypes.x16 
                             or TAE.Template.ParamTypes.s32 or TAE.Template.ParamTypes.u32 or TAE.Template.ParamTypes.x32 
                             or TAE.Template.ParamTypes.s64)
                    {
                        var a = Convert.ToInt64(check.Act.Parameters[ParamIndex]);
                        var b = Convert.ToInt64(Operand);
                        return OperatorEvaluator.EvaluateInteger(a, Operator, b);
                    }
                    else if (paramType 
                             is TAE.Template.ParamTypes.u64 or TAE.Template.ParamTypes.x64)
                    {
                        var a = Convert.ToUInt64(check.Act.Parameters[ParamIndex]);
                        var b = Convert.ToUInt64(Operand);
                        return OperatorEvaluator.EvaluateIntegerU64(a, Operator, b);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    
                }
            }
        }
    }
}