using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DSAnimStudio.ImguiOSD;
using ImGuiNET;

namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public List<TaeSearch.Condition> Conditions = new List<Condition>();

        public void BuildImGui(DSAProj proj, ref bool anyFieldFocused)
        {
            
            if (proj == null)
                return;
            
            
            
            if (ImGui.BeginTable($"##TaeSearchTable7", 5,
                    ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable |
                    ImGuiTableFlags.NoSavedSettings))
            {
                ImGui.TableSetupColumn("Condition Type", ImGuiTableColumnFlags.None, 5);
                ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.None, 5);
                ImGui.TableSetupColumn("Param", ImGuiTableColumnFlags.None, 5);
                ImGui.TableSetupColumn("Operator", ImGuiTableColumnFlags.None, 1.25f);
                ImGui.TableSetupColumn("Operand", ImGuiTableColumnFlags.None, 5);
    
                ImGui.TableHeadersRow();
            }
            else
            {
                return;
            }
            
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, Vector2.Zero);
            ImGui.PushItemWidth(0);

            List<Condition> conditionsToDelete = new List<Condition>();
            
            //Conditions list
            for (int i = 0; i < Conditions.Count; i++)
            {
                ImGui.PushID($"Window.TaeSearch.Conditions[{i}]");
                try
                {
                    if (i > 0)
                        ImGui.TableNextRow();



                    // Column 1 - Condition Type
                    ImGui.TableNextColumn();

                    var clickedDeleteCondition = Tools.SimpleClickButton("X");

                    if (ImGui.IsItemActive())
                        anyFieldFocused = true;

                    if (clickedDeleteCondition)
                    {
                        conditionsToDelete.Add(Conditions[i]);
                    }

                    ImGui.SameLine();



                    var curConditionType = Conditions[i].Type;
                    var prevConditionType = curConditionType;
                    ImGui.PushItemWidth(0);
                    Tools.EnumPicker("##TaeSearch.ConditionTypeEnumPicker", ref curConditionType,
                        ConditionTypeToString);
                    ImGui.PopItemWidth();
                    if (ImGui.IsItemActive())
                        anyFieldFocused = true;
                    if (curConditionType != prevConditionType)
                        Conditions[i] = Condition.NewFromEnum(curConditionType);


                    // Column 2-4 - Action, Param, Operator
                    ImGui.TableNextColumn();

                    // Special type configuration for parameter value type lol
                    bool anyTypeCtrlFieldFocused = false;
                    Conditions[i].ShowImGuiControlForTypeCfg(proj, ref anyTypeCtrlFieldFocused);
                    if (anyTypeCtrlFieldFocused)
                        anyFieldFocused = true;


                    var curOperatorType = Conditions[i].Operator;
                    var prevOperatorType = curOperatorType;
                    ImGui.PushItemWidth(0);
                    Tools.EnumPicker("##TaeSearch.OperatorTypeEnumPicker", ref curOperatorType, OperatorTypeToString, disableCache: true, validEntries: Conditions[i].AvailableOperators);
                    ImGui.PopItemWidth();
                    if (ImGui.IsItemActive())
                        anyFieldFocused = true;
                    if (curOperatorType != prevOperatorType)
                        Conditions[i].Operator = curOperatorType;

                    // Column 5 = Operand
                    ImGui.TableNextColumn();
                    bool anyOperandFieldFocused = false;
                    Conditions[i].ShowImGuiControlForOperand(proj, ref anyOperandFieldFocused);
                    if (anyOperandFieldFocused)
                        anyFieldFocused = true;
                }
                finally
                {
                    ImGui.PopID();
                }
            }
            
            ImGui.PopItemWidth();
            ImGui.PopStyleVar();
            
            ImGui.EndTable();
            
            var clickedAddCondition = Tools.SimpleClickButton("Add Condition");
            
            if (ImGui.IsItemActive())
                anyFieldFocused = true;
            
            if (clickedAddCondition)
            {
                Conditions.Add(Condition.NewFromEnum(ConditionType.AnimationCategoryID));
            }
            
            foreach (var c in conditionsToDelete)
            {
                Conditions.Remove(c);
            }
            
            
            
            
        }
        
        public List<Check> ProcessConditions(DSAProj proj, ConditionProcessType processType, bool forceReadAllParameters)
        {
            var conditionsCategory = Conditions.Where(x => x.Depth == ConditionDepthType.Category).ToList();
            var conditionsCategoryAnimation = Conditions.Where(x => x.Depth == ConditionDepthType.CategoryAnimation).ToList();
            var conditionsCategoryAnimationTrack = Conditions.Where(x => x.Depth == ConditionDepthType.CategoryAnimationTrack).ToList();
            var conditionsCategoryAnimationTrackAction = Conditions.Where(x => x.Depth == ConditionDepthType.CategoryAnimationTrackAction).ToList();
            var conditionsCategoryAnimationTrackActionParameter = Conditions.Where(x => x.Depth == ConditionDepthType.CategoryAnimationTrackActionParameter).ToList();
            

            ConditionDepthType deepestCondition = ConditionDepthType.Category;

            if (conditionsCategoryAnimation.Count > 0)
                deepestCondition = ConditionDepthType.CategoryAnimation;
            if (conditionsCategoryAnimationTrack.Count > 0)
                deepestCondition = ConditionDepthType.CategoryAnimationTrack;
            if (conditionsCategoryAnimationTrackAction.Count > 0)
                deepestCondition = ConditionDepthType.CategoryAnimationTrackAction;
            if (conditionsCategoryAnimationTrackActionParameter.Count > 0)
                deepestCondition = ConditionDepthType.CategoryAnimationTrackActionParameter;
            

            List<Check> result = new();
            
            var check = new Check();
            check.DeepestCondition = deepestCondition;
            check.Proj = proj;
            proj.SafeAccessAnimCategoriesList(categories =>
            {
                foreach (var cat in categories)
                {
                    check.Category = cat;

                    if (processType == ConditionProcessType.AllTrue)
                    {
                        bool categoryFoundInSearch =
                            conditionsCategory.Count == 0 || conditionsCategory.All(c => c.Evaluate(proj, check));
                        if (categoryFoundInSearch)
                        {
                            if (deepestCondition == ConditionDepthType.Category)
                                result.Add(check.Clone());
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (processType == ConditionProcessType.AnyTrue)
                    {
                        if (deepestCondition == ConditionDepthType.Category)
                        {
                            if (Conditions.Any(x => x.Evaluate(proj, check)))
                                result.Add(check.Clone());
                            continue;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    cat.UnSafeAccessAnimations(animations =>
                    {
                        foreach (var anim in animations)
                        {
                            check.Anim = anim;

                            if (processType == ConditionProcessType.AllTrue)
                            {
                                bool animFoundInSearch = conditionsCategoryAnimation.Count == 0 ||
                                                         conditionsCategoryAnimation.All(c => c.Evaluate(proj, check));
                                if (animFoundInSearch)
                                {
                                    if (deepestCondition == ConditionDepthType.CategoryAnimation)
                                        result.Add(check.Clone());
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else if (processType == ConditionProcessType.AnyTrue)
                            {
                                if (deepestCondition == ConditionDepthType.CategoryAnimation)
                                {
                                    if (Conditions.Any(x => x.Evaluate(proj, check)))
                                        result.Add(check.Clone());
                                    continue;
                                }
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }

                            anim.UnSafeAccessActionTracks(tracks =>
                            {
                                foreach (var track in tracks)
                                {
                                    check.Track = track;

                                    if (processType == ConditionProcessType.AllTrue)
                                    {
                                        bool trackFoundInSearch = conditionsCategoryAnimationTrack.Count == 0 ||
                                                                  conditionsCategoryAnimationTrack.All(c => c.Evaluate(proj, check));
                                        if (trackFoundInSearch)
                                        {
                                            if (deepestCondition == ConditionDepthType.CategoryAnimationTrack)
                                                result.Add(check.Clone());
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    else if (processType == ConditionProcessType.AnyTrue)
                                    {
                                        if (deepestCondition == ConditionDepthType.CategoryAnimationTrack)
                                        {
                                            if (Conditions.Any(x => x.Evaluate(proj, check)))
                                                result.Add(check.Clone());
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        throw new NotImplementedException();
                                    }

                                    var actions = track.GetActions(anim);
                                    foreach (var act in actions)
                                    {
                                        if (!proj.Template.ContainsKey(act.Type))
                                            continue;
                                        if (forceReadAllParameters)
                                            act.NewLoadParamsFromBytes(true, proj.Template[act.Type]);
                                        check.Act = act;

                                        if (processType == ConditionProcessType.AllTrue)
                                        {
                                            bool actionFoundInSearch = conditionsCategoryAnimationTrackAction.Count == 0 ||
                                                                       conditionsCategoryAnimationTrackAction.All(c =>
                                                                           c.Evaluate(proj, check));
                                            if (actionFoundInSearch)
                                            {
                                                if (deepestCondition == ConditionDepthType.CategoryAnimationTrackAction)
                                                    result.Add(check.Clone());
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                        else if (processType == ConditionProcessType.AnyTrue)
                                        {
                                            if (deepestCondition == ConditionDepthType.CategoryAnimationTrackAction)
                                            {
                                                if (Conditions.Any(x => x.Evaluate(proj, check)))
                                                    result.Add(check.Clone());
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            throw new NotImplementedException();
                                        }

                                        if (act.Template != null)
                                        {
                                            for (int p = 0; p < act.Template.Count; p++)
                                            {
                                                check.ParameterName = act.Template[p].Name;

                                                if (processType == ConditionProcessType.AllTrue)
                                                {
                                                    bool parameterFoundInSearch =
                                                        conditionsCategoryAnimationTrackActionParameter.Count == 0 ||
                                                        conditionsCategoryAnimationTrackActionParameter.All(c => c.Evaluate(proj, check));
                                                    if (parameterFoundInSearch)
                                                    {
                                                        if (deepestCondition ==
                                                            ConditionDepthType.CategoryAnimationTrackActionParameter)
                                                            result.Add(check.Clone());
                                                    }
                                                    else
                                                    {
                                                        continue;
                                                    }
                                                }
                                                else if (processType == ConditionProcessType.AnyTrue)
                                                {
                                                    if (deepestCondition == ConditionDepthType.CategoryAnimationTrackActionParameter)
                                                    {
                                                        if (Conditions.Any(x => x.Evaluate(proj, check)))
                                                            result.Add(check.Clone());
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    throw new NotImplementedException();
                                                }
                                            }
                                        }
                                    }
                                }
                            });

                            
                        }
                    });

                    
                }
            });
            

            return result;
        }
    }
}