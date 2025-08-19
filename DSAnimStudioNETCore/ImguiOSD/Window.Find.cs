using DSAnimStudio.TaeEditor;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Find : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            private TaeSearch search = new TaeSearch();
            private TaeSearch.ConditionProcessType processType = TaeSearch.ConditionProcessType.AllTrue;
            private bool forceReadAllParams = true;
            public override string NewImguiWindowTitle => "Find";

            private List<TaeSearch.Check> searchResults = new();

            private int searchResultPageCount = 0;
            private int searchResultPageIndex = 0;
            private int searchResultsPerPage = 1000;

            private int selectedResultIndex = -1;

            protected override void Init()
            {
                //Flags = ImGuiWindowFlags.NoDocking | ;
            }

            protected override void PreUpdate()
            {
                //IsOpen = true;
            }

            protected override void PostUpdate()
            {
                
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                if (MainProj == null)
                    return;
                //ImGui.SetWindowFocus();
                
                search.BuildImGui(MainTaeScreen.Proj, ref anyFieldFocused);

                bool anyConditions = search.Conditions.Count > 0;

                ImGui.Separator();

                ImGui.PushItemWidth(128 * Main.DPI);
                Tools.EnumPicker("Condition Check Type", ref processType, TaeSearch.ConditionProcessTypeToString);
                ImGui.PopItemWidth();
                
                ImGui.Checkbox("Force Refresh Params In All Actions", ref forceReadAllParams);

                ImGui.PushItemWidth(80);
                ImGui.InputInt("Search Results Per Page", ref searchResultsPerPage, 0, 0);
                ImGui.PopItemWidth();
                if (searchResultsPerPage < 10)
                    searchResultsPerPage = 10;
                if (searchResultsPerPage > 100000)
                    searchResultsPerPage = 100000;

                if (!anyConditions)
                    ImGui.BeginDisabled();

                bool searchButtonClicked = Tools.SimpleClickButton("SEARCH") && anyConditions;

                if (!anyConditions)
                    ImGui.EndDisabled();

                if (searchButtonClicked)
                {
                    searchResults = search.ProcessConditions(MainTaeScreen.Proj, processType, forceReadAllParams);
                    selectedResultIndex = -1;
                    searchResultPageIndex = 0;
                }

                ImGui.Separator();
                    
                searchResultPageCount = (searchResults.Count / searchResultsPerPage);
                    
                ImGui.Text("Page:");
                ImGui.SameLine();
                ImGui.PushItemWidth(120);
                float page = searchResultPageIndex + 1;
                ImGui.InputFloat("##PageFloat", ref page, 1.0f, 5.0f, $"%.0f / {(searchResultPageCount + 1)}");
                searchResultPageIndex = (int)Math.Round(page - 1);
                ImGui.PopItemWidth();
                    
                if (searchResultPageIndex < 0)
                    searchResultPageIndex = 0;
                else if (searchResultPageIndex > searchResultPageCount)
                    searchResultPageIndex = searchResultPageCount;

                int resultIndexMin = (searchResultPageIndex * searchResultsPerPage);
                int resultIndexMax =
                    Math.Min(((searchResultPageIndex + 1) * searchResultsPerPage) - 1, searchResults.Count - 1);

                
                ImGui.Text($"Results ({(resultIndexMin + 1)}-{(resultIndexMax + 1)} of {searchResults.Count}):");
                
                if (searchResults != null && searchResults.Count > 0)
                {
                    
                    
                    
                    
                    
                    

                    int columnCount = 0;
                    var absoluteDeepestCondition = TaeSearch.ConditionDepthType.None;

                    foreach (var c in searchResults)
                    {
                        if (c.DeepestCondition > absoluteDeepestCondition)
                            absoluteDeepestCondition = c.DeepestCondition;
                    }

                    if (absoluteDeepestCondition >= TaeSearch.ConditionDepthType.Category)
                        columnCount = 1;
                        
                    if (absoluteDeepestCondition >= TaeSearch.ConditionDepthType.CategoryAnimation)
                        columnCount = 2;
                        
                    if (absoluteDeepestCondition >= TaeSearch.ConditionDepthType.CategoryAnimationTrack)
                        columnCount = 3;
                        
                    if (absoluteDeepestCondition >= TaeSearch.ConditionDepthType.CategoryAnimationTrackAction)
                        columnCount = 4;

                    if (columnCount == 0)
                        return;
                    
                    //var cursorYAtListStart = ImGui.GetCursorPosY();
                    //ImGui.BeginChild("Window.Find.Child", new Vector2(LastSize.X - 16, LastSize.Y - cursorYAtListStart - 8), true, ImGuiWindowFlags.AlwaysVerticalScrollbar);
                    
                    if (ImGui.BeginTable($"##TaeSearchResultsTable4", columnCount,
                            ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable |
                            ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.ScrollY))
                    {
                        if (absoluteDeepestCondition >= TaeSearch.ConditionDepthType.Category)
                            ImGui.TableSetupColumn("Category", ImGuiTableColumnFlags.None, 3);
                        
                        if (absoluteDeepestCondition >= TaeSearch.ConditionDepthType.CategoryAnimation)
                            ImGui.TableSetupColumn("Animation", ImGuiTableColumnFlags.None, 5);
                        
                        if (absoluteDeepestCondition >= TaeSearch.ConditionDepthType.CategoryAnimationTrack)
                            ImGui.TableSetupColumn("Track", ImGuiTableColumnFlags.None, 3);
                        
                        if (absoluteDeepestCondition >= TaeSearch.ConditionDepthType.CategoryAnimationTrackAction)
                            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.None, 5);
                        
                        ImGui.TableSetupScrollFreeze(columnCount, 1);
    
                        ImGui.TableHeadersRow();
                    }
                    else
                    {
                        return;
                    }

                    int jumpToSearchResultIndex = -1;
                    
                    


                    
                    
                    for (int i = resultIndexMin; i <= resultIndexMax; i++)
                    {
                        if (i > 0)
                            ImGui.TableNextRow();

                        bool selected = selectedResultIndex == i;
                        
                        
                        if (searchResults[i].DeepestCondition >= TaeSearch.ConditionDepthType.Category)
                        {
                            ImGui.TableNextColumn();
                            ImGui.PushItemWidth(0);
                            if (ImGui.Selectable(
                                    $"{(searchResults[i].Category?.CategoryID.ToString() ?? "")} {(searchResults[i].Category?.Info?.DisplayName ?? "")}##SearchResult[{i}]",
                                    ref selected,
                                    ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick))
                            {
                                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                                    jumpToSearchResultIndex = i;
                            }
                            ImGui.PopItemWidth();
                            
                        }

                        //ImGui.TableNextColumn();
                        if (searchResults[i].DeepestCondition >= TaeSearch.ConditionDepthType.CategoryAnimation)
                        {
                            ImGui.TableNextColumn();
                            ImGui.PushItemWidth(0);

                            searchResults[i].Anim?.SafeAccessHeader(header =>
                            {
                                ImGui.Text(
                                $"{(searchResults[i].Anim?.SplitID.GetFormattedIDString(MainProj) ?? "")} {(header?.AnimFileName ?? "")}");
                            });

                            
                            ImGui.PopItemWidth();
                        }

                        //ImGui.TableNextColumn();
                        if (searchResults[i].DeepestCondition >= TaeSearch.ConditionDepthType.CategoryAnimationTrack)
                        {
                            ImGui.TableNextColumn();
                            ImGui.PushItemWidth(0);
                            ImGui.Text(
                                $"{(searchResults[i].Track?.TrackType.ToString() ?? "")} {(searchResults[i].Track?.Info?.DisplayName ?? "")}");
                            ImGui.PopItemWidth();
                        }
                        
                        //ImGui.TableNextColumn();
                        if (searchResults[i].DeepestCondition >= TaeSearch.ConditionDepthType.CategoryAnimationTrackAction)
                        {
                            ImGui.TableNextColumn();
                            ImGui.PushItemWidth(0);
                            ImGui.Text(searchResults[i].Act?.GraphDisplayText ?? "");
                            ImGui.PopItemWidth();
                        }

                        if (selected)
                            selectedResultIndex = i;
                    }

                    if (jumpToSearchResultIndex >= 0 && jumpToSearchResultIndex < searchResults.Count)
                    {
                        var res = searchResults[jumpToSearchResultIndex];
                        if (res.DeepestCondition == TaeSearch.ConditionDepthType.Category)
                        {
                            var firstAnim = res.Category.SAFE_GetFirstAnimInList();
                            if (firstAnim != null)
                            {
                                MainTaeScreen.RequestHighlightAnimCategory(res.Category);
                                MainTaeScreen.RequestHighlightAnimation(firstAnim);
                                MainTaeScreen.SelectNewAnimRef(res.Category, firstAnim);
                            }
                        }
                        else if (res.DeepestCondition == TaeSearch.ConditionDepthType.CategoryAnimation)
                        {
                            MainTaeScreen.RequestHighlightAnimCategory(res.Category);
                            MainTaeScreen.RequestHighlightAnimation(res.Anim);
                            MainTaeScreen.SelectNewAnimRef(res.Category, res.Anim);
                        }
                        else if (res.DeepestCondition == TaeSearch.ConditionDepthType.CategoryAnimationTrack)
                        {
                            MainTaeScreen.RequestHighlightAnimCategory(res.Category);
                            MainTaeScreen.RequestHighlightAnimation(res.Anim);
                            MainTaeScreen.RequestHighlightTrack(res.Track);
                            MainTaeScreen.SelectNewAnimRef(res.Category, res.Anim);
                            MainTaeScreen.NewSelectedActions = res.Track.GetActions(res.Anim);
                            MainTaeScreen.Graph.LayoutManager.ScrollToAction(MainTaeScreen.NewSelectedActions.FirstOrDefault());
                        }
                        else if (res.DeepestCondition == TaeSearch.ConditionDepthType.CategoryAnimationTrackAction)
                        {
                            MainTaeScreen.RequestHighlightAnimCategory(res.Category);
                            MainTaeScreen.RequestHighlightAnimation(res.Anim);
                            MainTaeScreen.RequestHighlightTrack(res.Track);
                            MainTaeScreen.RequestHighlightAction(res.Act);
                            MainTaeScreen.SelectNewAnimRef(res.Category, res.Anim);
                            //Highlight selected track here
                            //Highlight selected action here
                            MainTaeScreen.NewSelectedActions = new() { res.Act };
                            MainTaeScreen.Graph.LayoutManager.ScrollToAction(res.Act);
                        }
                    }
                    
                    
                    ImGui.EndTable();
                    //ImGui.EndChild();
                }
            }
        }
    }
}
