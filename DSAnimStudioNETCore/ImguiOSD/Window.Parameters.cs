using Assimp;
using DSAnimStudio.TaeEditor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = System.Numerics.Vector2;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Parameters : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            public override string NewImguiWindowTitle => "Parameters";
            
            public bool IsInEditMode = false;

            public bool IsInTableMode = true;

            private object _lock_TemplateRefreshQueue = new object();
            public List<TAE.Template.ActionTemplate> TemplateRefreshesQueued = new();

            protected override void Init()
            {
                //Title = "Action Inspector";
                // Flags = ImGuiWindowFlags.NoCollapse |
                //         ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar |
                //         ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoResize;
            }

            string CurrentUnmappedEventHex = "";

            static string[] listBox_EntityTypeNames = new string[]
            {
                "0: Character (cXXXX_YYYY)",
                "1: Object (oXXXX_YYYY)",
                "2: Map Piece (mXXXX_YYYY)",
                "4: Dummy Node (dXXXX_YYYY)",
            };
            static List<SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataStruct.EntityTypes> listBox_EntityTypes = 
                new List<SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataStruct.EntityTypes>
            { 
                SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataStruct.EntityTypes.Character,
                SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataStruct.EntityTypes.Object,
                SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataStruct.EntityTypes.MapPiece,
                SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataStruct.EntityTypes.DummyNode
            };

            static string[] listBox_ActionTrackDataTypeNames = new string[]
            {
                "0: Standard",
                "16: Unknown",
                "128: Specific Scene Entity",
                "192: Unknown",
            };
            static List<SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataType> listBox_EventGroupDataTypes =
                new List<SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataType>
            {
                SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataType.TrackData0,
                SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataType.TrackData16,
                SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataType.ApplyToSpecificCutsceneEntity,
                SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataType.TrackData192,
            };

            

            private static string[] _paramTypeNames = null;
            private static List<TAE.Template.ParamTypes> _paramTypeValues = null;
            private TAE.Template.ParamTypes ShowParamTypeEditor(TAE.Template.ParamTypes inputParamType, int paramIndex)
            {
                if (_paramTypeNames == null || _paramTypeValues == null)
                {
                    _paramTypeValues = ((TAE.Template.ParamTypes[])Enum.GetValues(typeof(TAE.Template.ParamTypes))).ToList();
                    _paramTypeNames = _paramTypeValues.Select(x => x.ToString()).ToArray();
                }

                int curValType = _paramTypeValues.IndexOf(inputParamType);

                ImGui.PushItemWidth(60 * Main.DPI);
                if (ImGui.BeginCombo($"###ParamTypeEditor_{paramIndex}", _paramTypeNames[curValType]))
                {
                    for (int i = 0; i < _paramTypeNames.Length; i++)
                    {
                        bool selected = i == curValType;
                        bool prevSelected = selected;
                        ImGui.Selectable($"{_paramTypeNames[i]}###ParamTypeEditor_{paramIndex}_Type{i}", ref selected);
                        if (selected && !prevSelected)
                            curValType = i;
                    }
                    ImGui.EndCombo();
                }
                ImGui.PopItemWidth();

                //ImGui.Combo("Value Type", ref curValType, _paramTypeNames, _paramTypeNames.Length);
                if (curValType >= 0 && curValType < _paramTypeValues.Count)
                    return _paramTypeValues[curValType];
                else
                    return inputParamType;
            }

            protected override void PreUpdate()
            {
                IsOpen = true;
                //ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0, 0, 0, 0));
            }

            protected override void PostUpdate()
            {
                ImGui.PopStyleColor();
                //ImGui.PopStyleVar();
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                // ImGui.SetWindowPos((MainTaeScreen.ImGuiEventInspectorPos + new System.Numerics.Vector2(0, 12)) * Main.DPIVectorN);
                // ImGui.SetWindowSize((MainTaeScreen.ImGuiEventInspectorSize - new System.Numerics.Vector2(0, 12)) * Main.DPIVectorN);

                DSAProj.Action multiEditHoverAction = null;

                bool editMode = IsInEditMode;
                bool tableMode = !editMode && IsInTableMode;

                //int evIndex = -1;

                bool isReadOnlyGraph = MainTaeScreen?.Graph?.IsGhostGraph != false; // != false so if it's null it also acts read-only, for safety.

                if (isReadOnlyGraph)
                {
                    ImGui.BeginDisabled();
                }

                try
                {



                    for (int actIndex = 0; actIndex < MainTaeScreen.NewSelectedActions.Count; actIndex++)
                    {
                        bool breakAfterThisAction = false;

                        var act = MainTaeScreen.NewSelectedActions[actIndex];
                        var imguiCursorBeforeEvent = ImGui.GetCursorPos();

                        ImGui.PushItemWidth(256 * Main.DPI);




                        //var ev = Tae.InspectorEvent;

                        // Debug stuff

                        //ImGui.LabelText("Debug Stuff", "");
                        //int evGroupIndex = ev.GroupIndex;
                        //ImGui.InputInt("Event Row", ref evGroupIndex);
                        //ev.GroupIndex = evGroupIndex;

                        Flags |= ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.AlwaysHorizontalScrollbar;






                        //ImGui.Separator();

                        string actionTypeString = (act.TypeName ?? $"[Action Type {act.Type}]") + $"###{actIndex}";

                        //ImGui.Button("★");
                        //if (ImGui.IsItemHovered())
                        //{
                        //    multiEditHoverAction = act;
                        //}
                        //if (ImGui.IsItemClicked())
                        //{
                        //    MainTaeScreen.QueuedChangeActionType = act;
                        //}
                        //ImGui.SameLine();
                        //ImGui.Text(actionTypeString);
                        ImGui.SameLine();
                        ImGui.Checkbox($"###{actIndex}_TemplateEditModeToggle", ref editMode);
                        IsInEditMode = editMode;

                        ImGui.SameLine();

                        if (MenuBar.ClickItem(actionTypeString,
                            shortcut: "(Change)", shortcutColor: Color.Cyan, onHovered: () =>
                            {
                                multiEditHoverAction = act;
                            }))
                        {
                            MainTaeScreen.QueuedChangeActionType = act;
                        }

                        //ImGui.SameLine();




                        if (IsInEditMode)
                        {


                            if (act.Template != null)
                            {

                                ImGui.Separator();

                                string evTemplateName = act.Template.Name;
                                string prevEvTemplateName = evTemplateName;
                                ImGui.InputText($"Action {act.Type} Name###{actIndex}_TemplateEditModeToggle_Template{act.Type}_Name", ref evTemplateName, 256);
                                anyFieldFocused |= ImGui.IsItemActive();
                                if (evTemplateName != prevEvTemplateName)
                                {
                                    act.Template.Name = evTemplateName;
                                    act.Template.UpdateAllParamsUnkNames();
                                    act.NewLoadParamsFromBytes(lenientOnAssert: true);

                                    lock (_lock_TemplateRefreshQueue)
                                    {
                                        if (!TemplateRefreshesQueued.Contains((act.Template)))
                                            TemplateRefreshesQueued.Add(act.Template);
                                    }

                                    //break;
                                }

                                ImGui.Separator();
                                if (Tools.SimpleClickButton("Quick Save CustomTemplate XML"))
                                {
                                    MainTaeScreen.SaveCustomXMLTemplate();
                                }
                                ImGui.Separator();
                                if (Tools.SimpleClickButton("Immediately Force Refresh Template On All Actions (Slow)"))
                                {
                                    MainTaeScreen.RefreshTemplateForAllActions(act.Template, isSoftRefresh: false);
                                }
                            }
                        }




                        //int test = ev.Unk04;
                        //ImGui.InputInt("Unknown Field (Sekiro)", ref test);

                        //ImGui.Separator();

                        int ClampInt(int v, int min, int max)
                        {
                            v = Math.Max(v, min);
                            v = Math.Min(v, max);
                            return v;
                        }

                        if (act.TypeName != null)
                        {
                            CurrentUnmappedEventHex = "";

                            Dictionary<string, string> event0ParameterMap = null;
                            if (act.Type == 0)
                                event0ParameterMap = new Dictionary<string, string>();

                            string currentParameterGroup = null;

                            //int pIndex = -1;

                            bool didTemplateChange = false;


                            float tableWidth = ImGui.GetWindowSize().X;
                            float columnA = 32 * Main.DPI;
                            float columnB = (tableWidth - columnA) * 0.3333333f;
                            float columnC = (tableWidth - columnA) * 0.6666666f;

                            if (tableMode)
                            {


                                if (ImGui.BeginTable($"##Window.Parameters.Table|New5", 3,
                                        ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable |
                                        ImGuiTableFlags.NoSavedSettings))
                                {
                                    ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.None, columnA);
                                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.None, columnB);
                                    ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.None, columnC);

                                    ImGui.TableHeadersRow();
                                }
                                else
                                {
                                    tableMode = false;
                                }


                            }


                            if (act.Parameters.Template.Count == 0)
                            {
                                if (Tools.SimpleClickButton($"+###AddFieldHere_-1"))
                                {
                                    act.Template.Add(new TAE.Template.ParameterTemplate()
                                    {
                                        Name = "Unk00",
                                        NameIsUnk = true,
                                        ParamType = TAE.Template.ParamTypes.u8,
                                    });
                                    act.NewLoadParamsFromBytes(lenientOnAssert: true, act.Template);
                                    //MainTaeScreen.RefreshTemplateForAllEvents(act.Template);
                                    lock (_lock_TemplateRefreshQueue)
                                    {
                                        if (!TemplateRefreshesQueued.Contains((act.Template)))
                                            TemplateRefreshesQueued.Add(act.Template);
                                    }

                                    RequestMaintainScrollFrames += 2;
                                    breakAfterThisAction = true;
                                    break;
                                }
                            }

                            for (int pIndex = 0; pIndex < act.Parameters.Template.Count; pIndex++)
                            {
                                if (pIndex >= act.Parameters.Values.Count)
                                    continue;

                                var p = act.Parameters.Template[pIndex];


                                int templateImguiParamIndex = (actIndex * 1000) + pIndex;


                                string parameterName = tableMode ? p.Name : $"{p.ParamType} {p.Name}";





                                if (editMode)
                                {
                                    ImGui.Separator();

                                    int paramOffset = act.Template.GetParameterByteOffset(p);
                                    ImGui.Text($"[+0x{paramOffset:X2}]");

                                    ImGui.SameLine();

                                    if (Tools.SimpleClickButton($"X###RemoveFieldHere_{templateImguiParamIndex}"))
                                    {
                                        act.Template.Remove(p);
                                        act.NewLoadParamsFromBytes(lenientOnAssert: true, act.Template);
                                        //MainTaeScreen.RefreshTemplateForAllEvents(act.Template);
                                        lock (_lock_TemplateRefreshQueue)
                                        {
                                            if (!TemplateRefreshesQueued.Contains((act.Template)))
                                                TemplateRefreshesQueued.Add(act.Template);
                                        }

                                        RequestMaintainScrollFrames += 2;
                                        breakAfterThisAction = true;
                                        break;
                                    }
                                    ImGui.SameLine();

                                    if (Tools.SimpleClickButton($"+###AddFieldHere_{templateImguiParamIndex}"))
                                    {
                                        act.Template.Insert(act.Template.IndexOf(p), p.GetCopy());
                                        act.NewLoadParamsFromBytes(lenientOnAssert: true, act.Template);
                                        //MainTaeScreen.RefreshTemplateForAllEvents(act.Template);
                                        lock (_lock_TemplateRefreshQueue)
                                        {
                                            if (!TemplateRefreshesQueued.Contains((act.Template)))
                                                TemplateRefreshesQueued.Add(act.Template);
                                        }

                                        RequestMaintainScrollFrames += 2;
                                        breakAfterThisAction = true;
                                        break;
                                    }
                                    ImGui.SameLine();



                                    var prevParamType = p.ParamType;
                                    p.ParamType = ShowParamTypeEditor(p.ParamType, templateImguiParamIndex);
                                    ImGui.SameLine();
                                    bool isAssert = p.ValueToAssert != null;
                                    bool prevIsAssert = isAssert;
                                    ImGui.Checkbox($"###AssertFlag{templateImguiParamIndex}", ref isAssert);
                                    if (isAssert != prevIsAssert)
                                    {
                                        if (isAssert)
                                            p.ValueToAssert = act.Parameters[pIndex];
                                        else
                                            p.ValueToAssert = null;
                                        lock (_lock_TemplateRefreshQueue)
                                        {
                                            if (!TemplateRefreshesQueued.Contains((act.Template)))
                                                TemplateRefreshesQueued.Add(act.Template);
                                        }
                                    }
                                    ImGui.SameLine();
                                    string currentName = p.NameIsUnk ? "" : p.Name;
                                    string prevName = currentName;
                                    var prevNameIsUnk = p.NameIsUnk;
                                    ImGui.PushItemWidth(256 * Main.DPI);
                                    ImGui.InputText($"###NameInput{templateImguiParamIndex}", ref currentName, 256);
                                    ImGui.PopItemWidth();
                                    anyFieldFocused |= ImGui.IsItemActive();
                                    if (string.IsNullOrWhiteSpace(currentName))
                                    {
                                        p.NameIsUnk = true;
                                    }
                                    else
                                    {
                                        p.NameIsUnk = false;
                                        p.Name = currentName;
                                    }

                                    if (p.ParamType != prevParamType)
                                    {
                                        if (p.ValueToAssert != null)
                                        {
                                            using (var memStream = new MemoryStream(act.ParameterBytes))
                                            {
                                                var br = new BinaryReaderEx(MainTaeScreen.FileContainer.Proj.RootTaeProperties.BigEndian, memStream);
                                                br.Position = act.Template.GetParameterByteOffset(p);
                                                p.ValueToAssert = p.ReadValue(br);

                                            }
                                        }
                                        lock (_lock_TemplateRefreshQueue)
                                        {
                                            if (!TemplateRefreshesQueued.Contains((act.Template)))
                                                TemplateRefreshesQueued.Add(act.Template);
                                        }
                                    }

                                    ImGui.SameLine();
                                    parameterName = "";
                                    if (p.ParamType != prevParamType || prevName != currentName || prevNameIsUnk != p.NameIsUnk)
                                    {
                                        didTemplateChange = true;
                                        act.Template.UpdateAllParamsUnkNames();
                                        act.NewLoadParamsFromBytes(lenientOnAssert: true);
                                        //MainTaeScreen.RefreshTemplateForAllEvents(act.Template);

                                        lock (_lock_TemplateRefreshQueue)
                                        {
                                            if (!TemplateRefreshesQueued.Contains((act.Template)))
                                                TemplateRefreshesQueued.Add(act.Template);
                                        }

                                        // Force maintain vertical scroll for 2 frames.
                                        //RequestMaintainScrollFrames += 2;

                                        //break;
                                    }
                                }



                                if (currentParameterGroup != null && p.NameGroup != currentParameterGroup)
                                {
                                    ImGui.Unindent();
                                    currentParameterGroup = null;
                                }

                                if (currentParameterGroup == null && p.NameGroup != null)
                                {
                                    ImGui.Text(p.NameGroup);
                                    ImGui.Indent();
                                    currentParameterGroup = p.NameGroup;
                                }

                                bool isAssertValue = p.ValueToAssert != null;

                                if (isAssertValue)
                                {
                                    if (editMode)
                                    {
                                        ImGui.BeginDisabled();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }



                                bool isEnum = p.EnumEntries != null && p.EnumEntries.Count > 0;

                                bool intSigned = p.ParamType == TAE.Template.ParamTypes.s8 || p.ParamType == TAE.Template.ParamTypes.s16 || p.ParamType == TAE.Template.ParamTypes.s32;
                                bool intUnsigned = p.ParamType == TAE.Template.ParamTypes.u8 || p.ParamType == TAE.Template.ParamTypes.u16 || p.ParamType == TAE.Template.ParamTypes.u32;
                                bool intHex = p.ParamType == TAE.Template.ParamTypes.x8 || p.ParamType == TAE.Template.ParamTypes.x16 || p.ParamType == TAE.Template.ParamTypes.x32;

                                string dispName = parameterName;
                                bool grayedOut = false;
                                if (act.Type == 0 && pIndex > 0)
                                {
                                    if (event0ParameterMap.ContainsKey(p.Name))
                                    {
                                        dispName = $"{parameterName}: {event0ParameterMap[p.Name]}";
                                    }
                                    // else
                                    //     grayedOut = true;
                                }

                                if (grayedOut)
                                {
                                    if (editMode)
                                        ImGui.BeginDisabled();
                                    else
                                        continue;
                                }

                                if (tableMode)
                                {
                                    if (pIndex > 0)
                                        ImGui.TableNextRow();

                                    ImGui.TableNextColumn();
                                    ImGui.Text(p.ParamType.ToString());

                                    ImGui.TableNextColumn();
                                    ImGui.Text(dispName);

                                    ImGui.TableNextColumn();

                                    parameterName = "";
                                    dispName = "";
                                }

                                if (!tableMode)
                                    ImGui.PushItemWidth(128);

                                if (intSigned || intUnsigned || intHex)
                                {
                                    int currentVal = Convert.ToInt32(act.Parameters[pIndex]);

                                    int prevVal = currentVal;







                                    string inputIntName = dispName;

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

                                            if (act.Type == 0 && items[i].Contains("|"))
                                            {
                                                var allArgsSplit = items[i].Split('|').Select(x => x.Trim()).ToList();
                                                dispItems[i] = allArgsSplit[0];
                                                for (int j = 1; j < allArgsSplit.Count; j++)
                                                {
                                                    var argSplit = allArgsSplit[j].Split(':').Select(x => x.Trim()).ToList();
                                                    var argParameter = argSplit[0];
                                                    var argName = argSplit[1];
                                                    if (i == currentItemIndex && !event0ParameterMap.ContainsKey(argParameter))
                                                        event0ParameterMap.Add(argParameter, argName);
                                                }
                                            }
                                            else
                                            {
                                                dispItems[i] = items[i];
                                            }
                                        }


                                        //ImGui.PushItemWidth(24);
                                        int prevItemIndex = currentItemIndex;
                                        ImGui.Combo($"###{actIndex}|{pIndex}|Combo", ref currentItemIndex, dispItems, items.Length);
                                        //ImGui.PopItemWidth();
                                        if (currentItemIndex != prevItemIndex && currentItemIndex >= 0 && currentItemIndex < items.Length)
                                            currentVal = Convert.ToInt32(p.EnumEntries[currentItemIndex].Value);

                                        ImGui.SameLine();
                                    }


                                    if (isEnum)
                                    {
                                        // For enum entries we have the name on the combo box not the int input
                                        //inputIntName = "";

                                        ImGui.PushItemWidth(100);
                                    }

                                    ImGui.InputInt(inputIntName + $"###{actIndex}|{pIndex}", ref currentVal, 1, 5, intHex ? (ImGuiInputTextFlags.CharsHexadecimal |
                                    ImGuiInputTextFlags.CharsUppercase) : ImGuiInputTextFlags.None);
                                    anyFieldFocused |= ImGui.IsItemActive();

                                    if (isEnum)
                                        ImGui.PopItemWidth();

                                    //if (grayedOut)
                                    //    Tools.PopGrayedOut();














                                    if (currentVal < 0 && intUnsigned)
                                    {
                                        currentVal = 0;
                                    }



                                    if (currentVal != prevVal)
                                    {
                                        MainTaeScreen.CurrentAnimUndoMan.NewAction(doAction: () =>
                                        {
                                            if (p.ParamType is TAE.Template.ParamTypes.u8 or TAE.Template.ParamTypes.x8)
                                                act.Parameters[pIndex] = (byte)ClampInt(currentVal, byte.MinValue, byte.MaxValue);

                                            else if (p.ParamType is TAE.Template.ParamTypes.s8)
                                                act.Parameters[pIndex] = (sbyte)ClampInt(currentVal, sbyte.MinValue, sbyte.MaxValue);

                                            else if (p.ParamType is TAE.Template.ParamTypes.s16)
                                                act.Parameters[pIndex] = (short)ClampInt(currentVal, short.MinValue, short.MaxValue);

                                            else if (p.ParamType is TAE.Template.ParamTypes.u16 or TAE.Template.ParamTypes.x16)
                                                act.Parameters[pIndex] = (ushort)ClampInt(currentVal, ushort.MinValue, ushort.MaxValue);

                                            else if (p.ParamType is TAE.Template.ParamTypes.s32)
                                                act.Parameters[pIndex] = (int)ClampInt(currentVal, int.MinValue, int.MaxValue);

                                            else if (p.ParamType is TAE.Template.ParamTypes.u32 or TAE.Template.ParamTypes.x32)
                                                act.Parameters[pIndex] = (uint)ClampInt(currentVal, (int)uint.MinValue, int.MaxValue);

                                            else if (p.ParamType is TAE.Template.ParamTypes.s64)
                                                act.Parameters[pIndex] = (long)currentVal;

                                            else if (p.ParamType is TAE.Template.ParamTypes.u64 or TAE.Template.ParamTypes.x64)
                                                act.Parameters[pIndex] = (ulong)currentVal;

                                            act.NewSaveParamsToBytes();
                                            MainTaeScreen.SelectedAnim?.SAFE_SetIsModified(true);
                                        }, undoAction: () =>
                                        {
                                            if (p.ParamType is TAE.Template.ParamTypes.u8 or TAE.Template.ParamTypes.x8)
                                                act.Parameters[pIndex] = (byte)ClampInt(prevVal, byte.MinValue, byte.MaxValue);

                                            else if (p.ParamType is TAE.Template.ParamTypes.s8)
                                                act.Parameters[pIndex] = (sbyte)ClampInt(prevVal, sbyte.MinValue, sbyte.MaxValue);

                                            else if (p.ParamType is TAE.Template.ParamTypes.s16)
                                                act.Parameters[pIndex] = (short)ClampInt(prevVal, short.MinValue, short.MaxValue);

                                            else if (p.ParamType is TAE.Template.ParamTypes.u16 or TAE.Template.ParamTypes.x16)
                                                act.Parameters[pIndex] = (ushort)ClampInt(prevVal, ushort.MinValue, ushort.MaxValue);

                                            else if (p.ParamType is TAE.Template.ParamTypes.s32)
                                                act.Parameters[pIndex] = (int)ClampInt(prevVal, int.MinValue, int.MaxValue);

                                            else if (p.ParamType is TAE.Template.ParamTypes.u32 or TAE.Template.ParamTypes.x32)
                                                act.Parameters[pIndex] = (uint)ClampInt(prevVal, (int)uint.MinValue, int.MaxValue);

                                            else if (p.ParamType is TAE.Template.ParamTypes.s64)
                                                act.Parameters[pIndex] = (long)prevVal;

                                            else if (p.ParamType is TAE.Template.ParamTypes.u64 or TAE.Template.ParamTypes.x64)
                                                act.Parameters[pIndex] = (ulong)prevVal;


                                            act.NewSaveParamsToBytes();
                                            MainTaeScreen.SelectedAnim?.SAFE_SetIsModified(true);
                                        },
                                        $"Change value of '{p.Name}' from '{prevVal}' to '{currentVal}'");


                                    }
                                }
                                else if (p.ParamType == TAE.Template.ParamTypes.f32)
                                {
                                    float currentVal = Convert.ToSingle(act.Parameters[pIndex]);
                                    float prevVal = currentVal;

                                    if (grayedOut)
                                    {
                                        if (editMode)
                                            ImGuiDebugDrawer.PushDisabled(true);
                                        else
                                            continue;
                                    }

                                    ImGui.InputFloat(dispName + $"###{actIndex}|{pIndex}", ref currentVal);
                                    anyFieldFocused |= ImGui.IsItemActive();

                                    if (grayedOut)
                                    {
                                        if (editMode)
                                            ImGui.EndDisabled();
                                    }

                                    //if (grayedOut)
                                    //    Tools.PopGrayedOut();



                                    if (currentVal != prevVal)
                                    {
                                        int indexCopy = pIndex;
                                        MainTaeScreen.CurrentAnimUndoMan.NewAction(doAction: () =>
                                            {
                                                act.Parameters[indexCopy] = currentVal;
                                                act.NewSaveParamsToBytes();
                                                MainTaeScreen.SelectedAnim?.SAFE_SetIsModified(true);
                                            }, undoAction: () =>
                                            {
                                                act.Parameters[indexCopy] = prevVal;
                                                act.NewSaveParamsToBytes();
                                                MainTaeScreen.SelectedAnim?.SAFE_SetIsModified(true);
                                            },
                                            $"Change value of '{p.Name}' from '{prevVal}' to '{currentVal}'");
                                    }


                                }
                                else if (p.ParamType == TAE.Template.ParamTypes.f32grad)
                                {
                                    System.Numerics.Vector2 currentVal = (System.Numerics.Vector2)(act.Parameters[pIndex]);
                                    System.Numerics.Vector2 prevVal = currentVal;

                                    if (grayedOut)
                                    {
                                        if (editMode)
                                            ImGuiDebugDrawer.PushDisabled(true);
                                        else
                                            continue;
                                    }

                                    //ImGui.Text(dispName);
                                    //ImGui.SameLine();
                                    //ImGui.InputFloat("Start", ref current.X);
                                    //ImGui.SameLine();
                                    //ImGui.InputFloat("End", ref current.Y);

                                    ImGui.InputFloat2(dispName + "###" + p.Name + $"###{actIndex}|{pIndex}", ref currentVal);
                                    anyFieldFocused |= ImGui.IsItemActive();

                                    if (grayedOut)
                                    {
                                        if (editMode)
                                            ImGuiDebugDrawer.PopDisabled();
                                    }

                                    //if (grayedOut)
                                    //    Tools.PopGrayedOut();



                                    if (currentVal != prevVal)
                                    {
                                        MainTaeScreen.CurrentAnimUndoMan.NewAction(doAction: () =>
                                            {
                                                act.Parameters[pIndex] = currentVal;
                                                act.NewSaveParamsToBytes();
                                                MainTaeScreen.SelectedAnim?.SAFE_SetIsModified(true);
                                            }, undoAction: () =>
                                            {
                                                act.Parameters[pIndex] = prevVal;
                                                act.NewSaveParamsToBytes();
                                                MainTaeScreen.SelectedAnim?.SAFE_SetIsModified(true);
                                            },
                                            $"Change value of '{p.Name}' from '{prevVal}' to '{currentVal}'");
                                    }
                                }
                                else if (p.ParamType == TAE.Template.ParamTypes.b)
                                {
                                    float checkboxWidth = (19f * Main.DPI) + 8f;
                                    byte currentVal = Convert.ToByte(act.Parameters[pIndex]);
                                    byte prevVal = currentVal;
                                    if (currentVal is 0 or 1)
                                    {
                                        bool curValBool = currentVal == 1;
                                        bool prevValBool = curValBool;
                                        ImGui.Checkbox($"###{actIndex}|{pIndex}__Checkbox", ref curValBool);
                                        if (curValBool != prevValBool)
                                            currentVal = (byte)(curValBool ? 1 : 0);
                                        ImGui.SameLine();
                                    }
                                    else
                                    {
                                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + checkboxWidth);
                                    }

                                    
                                    int currentValInt = currentVal;
                                    int prevValInt = prevVal;
                                    ImGui.PushItemWidth(ImGui.CalcItemWidth() - checkboxWidth);
                                    ImGui.InputInt(parameterName + $"###{actIndex}|{pIndex}", ref currentValInt,
                                        1, 5, intHex
                                            ? (ImGuiInputTextFlags.CharsHexadecimal |
                                               ImGuiInputTextFlags.CharsUppercase)
                                            : ImGuiInputTextFlags.None);
                                    anyFieldFocused |= ImGui.IsItemActive();
                                    ImGui.PopItemWidth();
                                    if (currentValInt != prevValInt)
                                    {
                                        currentVal = (byte)ClampInt(currentValInt, byte.MinValue, byte.MaxValue);
                                    }

                                    

                                    if (editMode)
                                    {
                                        int byteOffset = act.Template.GetParameterByteOffset(p);
                                        if (byteOffset < act.ParameterBytes.Length)
                                        {
                                            byte byteVal = act.ParameterBytes[byteOffset];
                                            ImGui.SameLine();
                                            ImGui.Text($"(0x{byteVal:X2})");
                                        }

                                    }

                                    if (currentVal != prevVal)
                                    {
                                        MainTaeScreen.CurrentAnimUndoMan.NewAction(doAction: () =>
                                            {
                                                act.Parameters[pIndex] = currentVal;
                                                act.NewSaveParamsToBytes();
                                                MainTaeScreen.SelectedAnim?.SAFE_SetIsModified(true);
                                            }, undoAction: () =>
                                            {
                                                act.Parameters[pIndex] = prevVal;
                                                act.NewSaveParamsToBytes();
                                                MainTaeScreen.SelectedAnim?.SAFE_SetIsModified(true);
                                            },
                                            $"Change value of '{p.Name}' from '{prevVal}' to '{currentVal}'");
                                    }
                                }
                                else if (p.ParamType == TAE.Template.ParamTypes.aob)
                                {
                                    byte[] buf = ((byte[])(act.Parameters[pIndex])).ToArray();
                                    byte[] newBuf = ImGuiWidgets.NewParameterAob("", $"###{actIndex}|{pIndex}", buf);

                                    if (!buf.SequenceEqual(newBuf))
                                    {
                                        act.Parameters[pIndex] = newBuf;
                                        act.NewSaveParamsToBytes();
                                        MainTaeScreen.SelectedAnim?.SAFE_SetIsModified(true);

                                        MainTaeScreen.CurrentAnimUndoMan.NewAction(doAction: () =>
                                            {
                                                act.Parameters[pIndex] = newBuf.ToArray();
                                                act.NewSaveParamsToBytes();
                                                MainTaeScreen.SelectedAnim?.SAFE_SetIsModified(true);
                                            }, undoAction: () =>
                                            {
                                                act.Parameters[pIndex] = buf.ToArray();
                                                act.NewSaveParamsToBytes();
                                                MainTaeScreen.SelectedAnim?.SAFE_SetIsModified(true);
                                            },
                                            $"Change value of '{p.Name}'");
                                    }
                                }

                                if (!tableMode)
                                    ImGui.PopItemWidth();

                                if (grayedOut)
                                {
                                    if (editMode)
                                        ImGui.EndDisabled();
                                }


                                if (editMode)
                                {
                                    if (isAssertValue)
                                        ImGui.EndDisabled();
                                    ImGui.SameLine();
                                    if (ImGui.TreeNode($"Enum Values###TemplateEdit_Parameter{templateImguiParamIndex}_EnumValueTree"))
                                    {
                                        if (p.EnumEntries != null)
                                        {
                                            //int enumEntryIndex = 0;
                                            for (int enumEntryIndex = 0; enumEntryIndex < p.EnumEntries.Count; enumEntryIndex++)
                                            {
                                                var enumEntry = p.EnumEntries[enumEntryIndex];
                                                if (Tools.SimpleClickButton("X"))
                                                {
                                                    p.EnumEntries.Remove(enumEntry);
                                                    if (p.EnumEntries.Count == 0)
                                                        p.EnumEntries = null;
                                                    breakAfterThisAction = true;
                                                    RequestMaintainScrollFrames += 2;
                                                    break;
                                                }
                                                string curEnumEntryName = enumEntry.Key;
                                                var prevEnumEntryName = curEnumEntryName;
                                                ImGui.SameLine();
                                                ImGui.InputText($"###{actIndex}|{pIndex}_EnumEntry{enumEntryIndex}_Name", ref curEnumEntryName, 256);
                                                anyFieldFocused |= ImGui.IsItemActive();
                                                if (curEnumEntryName != prevEnumEntryName)
                                                {
                                                    //if (p.EnumEntries.ContainsKey(prevEnumEntryName))
                                                    //    p.EnumEntries.Remove(prevEnumEntryName);
                                                    //p.EnumEntries[curEnumEntryName] = enumEntry.Value;
                                                    enumEntry.Key = curEnumEntryName;
                                                    //break;
                                                }

                                                if (intSigned || intUnsigned || intHex)
                                                {
                                                    int curEnumVal = Convert.ToInt32(enumEntry.Value);
                                                    int prevEnumVal = curEnumVal;
                                                    ImGui.SameLine();
                                                    ImGui.InputInt($"###{actIndex}|{pIndex}_EnumEntry{enumEntryIndex}",
                                                        ref curEnumVal, 1, 5, intHex ? (ImGuiInputTextFlags.CharsHexadecimal |
                                                        ImGuiInputTextFlags.CharsUppercase) : ImGuiInputTextFlags.None);
                                                    anyFieldFocused |= ImGui.IsItemActive();
                                                    if (curEnumVal != prevEnumVal)
                                                    {
                                                        if (p.ParamType == TAE.Template.ParamTypes.u8)
                                                            enumEntry.Value = (byte)ClampInt(curEnumVal, byte.MinValue, byte.MaxValue);
                                                        else if (p.ParamType == TAE.Template.ParamTypes.s8)
                                                            enumEntry.Value = (sbyte)ClampInt(curEnumVal, sbyte.MinValue, sbyte.MaxValue);
                                                        else if (p.ParamType == TAE.Template.ParamTypes.s16)
                                                            enumEntry.Value = (short)ClampInt(curEnumVal, short.MinValue, short.MaxValue);
                                                        else if (p.ParamType == TAE.Template.ParamTypes.u16)
                                                            enumEntry.Value = (ushort)ClampInt(curEnumVal, ushort.MinValue, ushort.MaxValue);
                                                        else if (p.ParamType == TAE.Template.ParamTypes.s32)
                                                            enumEntry.Value = (int)ClampInt(curEnumVal, int.MinValue, int.MaxValue);
                                                        else if (p.ParamType == TAE.Template.ParamTypes.u32)
                                                            enumEntry.Value = (uint)ClampInt(curEnumVal, (int)uint.MinValue, int.MaxValue);
                                                        //break;
                                                    }
                                                }
                                                else if (p.ParamType == TAE.Template.ParamTypes.f32)
                                                {
                                                    float curEnumVal = Convert.ToSingle(enumEntry.Value);
                                                    float prevEnumVal = curEnumVal;
                                                    ImGui.SameLine();
                                                    ImGui.InputFloat($"###{actIndex}|{pIndex}_EnumEntry{enumEntryIndex}", ref curEnumVal);
                                                    anyFieldFocused |= ImGui.IsItemActive();
                                                    if (curEnumVal != prevEnumVal)
                                                    {
                                                        enumEntry.Value = curEnumVal;
                                                        //break;
                                                    }
                                                }
                                                else if (p.ParamType == TAE.Template.ParamTypes.f32grad)
                                                {
                                                    System.Numerics.Vector2 curEnumVal = (System.Numerics.Vector2)(act.Parameters[pIndex]);
                                                    System.Numerics.Vector2 prevEnumVal = curEnumVal;
                                                    ImGui.SameLine();
                                                    ImGui.InputFloat2($"###{actIndex}|{pIndex}_EnumEntry{enumEntryIndex}", ref curEnumVal);
                                                    anyFieldFocused |= ImGui.IsItemActive();
                                                    if (curEnumVal != prevEnumVal)
                                                    {
                                                        enumEntry.Value = curEnumVal;
                                                        //break;
                                                    }
                                                }
                                                else if (p.ParamType == TAE.Template.ParamTypes.b)
                                                {
                                                    byte curEnumVal = Convert.ToByte(act.Parameters[pIndex]);
                                                    byte prevEnumVal = curEnumVal;
                                                    ImGui.SameLine();

                                                    if (curEnumVal is 0 or 1)
                                                    {
                                                        bool curEnumValBool = curEnumVal == 1;
                                                        bool prevEnumValBool = curEnumValBool;
                                                        ImGui.Checkbox(
                                                            $"###{actIndex}|{pIndex}_EnumEntry{enumEntryIndex}__Checkbox",
                                                            ref curEnumValBool);
                                                        if (curEnumValBool != prevEnumValBool)
                                                            curEnumVal = (byte)(curEnumValBool ? 1 : 0);
                                                        
                                                        ImGui.SameLine();
                                                    }
                                                    else
                                                    {
                                                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (27 * Main.DPI));
                                                    }

                                                    int curEnumValInt = curEnumVal;
                                                    int prevEnumValInt = curEnumValInt;

                                                    
                                                    ImGui.InputInt( $"###{actIndex}|{pIndex}_EnumEntry{enumEntryIndex}", ref curEnumValInt, 
                                                        1, 5, intHex ? (ImGuiInputTextFlags.CharsHexadecimal |
                                                                        ImGuiInputTextFlags.CharsUppercase) : ImGuiInputTextFlags.None);

                                                    if (curEnumValInt != prevEnumValInt)
                                                    {
                                                        curEnumVal = (byte)ClampInt(curEnumValInt, byte.MinValue, byte.MaxValue);
                                                    }
                                                    
                                                    if (curEnumVal != prevEnumVal)
                                                    {
                                                        enumEntry.Value = curEnumVal;
                                                        //break;
                                                    }
                                                }
                                                else if (p.ParamType == TAE.Template.ParamTypes.aob)
                                                {
                                                    byte[] buf = (byte[])(act.Parameters[pIndex]);
                                                    byte[] newBuf = new byte[buf.Length];
                                                    string current = string.Join("", buf.Select(bb => bb.ToString("X2")));
                                                    ImGui.SameLine();
                                                    ImGui.InputText($"###{actIndex}|{pIndex}_EnumEntry{enumEntryIndex}", ref current, (uint)((p.AobLength * 2) - 0), ImGuiInputTextFlags.CharsHexadecimal |
                                                        ImGuiInputTextFlags.CharsUppercase);
                                                    anyFieldFocused |= ImGui.IsItemActive();
                                                    //current = current.Replace(" ", "");
                                                    if (current.Length < p.AobLength * 2)
                                                        current += new string('0', (p.AobLength * 2) - current.Length);
                                                    bool wasModified = false;
                                                    for (int i = 0; i < buf.Length; i++)
                                                    {
                                                        newBuf[i] = byte.Parse(current.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                                                        if (newBuf[i] != buf[i])
                                                            wasModified = true;
                                                    }
                                                    if (wasModified)
                                                    {
                                                        enumEntry.Value = newBuf;
                                                        //break;
                                                    }
                                                }
                                                enumEntryIndex++;
                                            }



                                        }

                                        if (Tools.SimpleClickButton("+"))
                                        {
                                            if (p.EnumEntries == null)
                                            {
                                                p.EnumEntries = new List<SoulsAssetPipeline.Animation.TAE.Template.ParameterTemplate.EnumEntry>();
                                            }
                                            p.EnumEntries.Add(new SoulsAssetPipeline.Animation.TAE.Template.ParameterTemplate.EnumEntry($"Entry{(p.EnumEntries.Count + 1)}", p.GetDefaultValue()));
                                        }

                                        ImGui.TreePop();
                                    }

                                }
                            }

                            if (tableMode)
                            {
                                ImGui.EndTable();
                                //ImGui.PopStyleVar();
                            }

                            if (breakAfterThisAction)
                                break;



                            if (editMode)
                            {



                                if (didTemplateChange)
                                {
                                    act.Template.UpdateAllParamsUnkNames();
                                    act.NewLoadParamsFromBytes(lenientOnAssert: true);
                                    MainTaeScreen.RefreshTemplateForAllActions(act.Template, isSoftRefresh: true);

                                    //break;
                                }

                                var templateByteCount = act.Template.GetAllParametersByteCount();
                                if (act.OriginalParamBytesLength > templateByteCount)
                                {
                                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0, 0, 1));
                                    ImGui.NewLine();

                                    int extraDataAtEndCount = (act.OriginalParamBytesLength - templateByteCount);
                                    var extraDataAtEnd = string.Join(" ", act.ParameterBytes
                                        .Skip(act.ParameterBytes.Length - extraDataAtEndCount)
                                        .Select(xx => xx.ToString("X2")));
                                    ImGui.TextWrapped($"WARNING: Data {extraDataAtEndCount} bytes longer than template.\n" +
                                        $"Remaining Unmapped Data:\n{extraDataAtEnd}");
                                    ImGui.PopStyleColor();
                                }
                                else if (act.OriginalParamBytesLength < templateByteCount)
                                {
                                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0, 0, 1));
                                    ImGui.NewLine();
                                    ImGui.TextWrapped($"WARNING: Template {(templateByteCount - act.OriginalParamBytesLength)} bytes longer than data.");
                                    ImGui.PopStyleColor();
                                }
                            }

                            if (currentParameterGroup != null)
                            {
                                ImGui.Unindent();
                            }
                        }
                        else
                        {
                            CurrentUnmappedEventHex = ""; // temp idk when i'm gonna bother

                            if (editMode)
                            {
                                if (Tools.SimpleClickButton($"Add Template for Event {act.Type}"))
                                {
                                    var newEvTemplate = new TAE.Template.ActionTemplate(act.Type, $"Type{act.Type}", MainTaeScreen.FileContainer.Proj.RootTaeProperties.BigEndian);
                                    newEvTemplate.Add(new TAE.Template.ParameterTemplate()
                                    {
                                        Name = "Unk00",
                                        NameIsUnk = true,
                                        ParamType = TAE.Template.ParamTypes.u8,
                                    });
                                    MainTaeScreen.FileContainer.Proj.Template.Add(act.Type, newEvTemplate);
                                    MainTaeScreen.RefreshTemplateForAllActions(newEvTemplate, isSoftRefresh: false);
                                    break;
                                }
                            }

                            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0, 0, 1));
                            ImGui.TextWrapped("Unmapped Event Data:\n" + string.Join(" ", act.ParameterBytes.Select(xx => xx.ToString("X2"))));
                            ImGui.PopStyleColor();



                            ImGui.Button("Copy to clipboard" + $"###{actIndex}");
                            if (ImGui.IsItemClicked())
                            {
                                System.Windows.Forms.Clipboard.SetText(string.Join(" ", act.ParameterBytes.Select(xx => xx.ToString("X2"))));
                            }
                        }

                        var actionTrack = act.GetTrack(MainTaeScreen.SelectedAnim);

                        if (actionTrack != null && MainTaeScreen?.Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)//Tae?.Graph?.IsSimpleRemoGroupMode == true)
                        {
                            ImGui.Separator();

                            ImGui.LabelText("", "CUTSCENE DATA");



                            int groupTypeIndex = listBox_EventGroupDataTypes.IndexOf(actionTrack.TrackData.DataType);
                            ImGui.ListBox("Cutscene Event Context", ref groupTypeIndex, listBox_ActionTrackDataTypeNames, listBox_ActionTrackDataTypeNames.Length);
                            actionTrack.TrackData.DataType = groupTypeIndex < 0 ?
                                SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataType.TrackData0
                                : listBox_EventGroupDataTypes[groupTypeIndex];

                            actionTrack.TrackType = (int)actionTrack.TrackData.DataType;

                            if (actionTrack.TrackData.DataType == SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataType.ApplyToSpecificCutsceneEntity)
                            {
                                ImGui.Separator();

                                ImGui.LabelText("", "CUTSCENE ENTITY SELECTION");

                                int entityTypeIndex = listBox_EntityTypes.IndexOf(actionTrack.TrackData.CutsceneEntityType);
                                ImGui.ListBox("Entity Type" + $"###{actIndex}", ref entityTypeIndex, listBox_EntityTypeNames, listBox_EntityTypeNames.Length);
                                actionTrack.TrackData.CutsceneEntityType = entityTypeIndex < 0 ?
                                    SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataStruct.EntityTypes.Character
                                    : listBox_EntityTypes[entityTypeIndex];

                                int x = actionTrack.TrackData.CutsceneEntityIDPart1;
                                ImGui.InputInt("Entity ID First Half (XXXX)" + $"###{actIndex}", ref x);
                                anyFieldFocused |= ImGui.IsItemActive();
                                x = Math.Min(Math.Max(x, 0), 9999);
                                actionTrack.TrackData.CutsceneEntityIDPart1 = (short)x;

                                x = actionTrack.TrackData.CutsceneEntityIDPart2;
                                ImGui.InputInt("Entity ID Second Half (YYYY)" + $"###{actIndex}", ref x);
                                anyFieldFocused |= ImGui.IsItemActive();
                                x = Math.Min(Math.Max(x, 0), 9999);
                                actionTrack.TrackData.CutsceneEntityIDPart2 = (short)x;

                                x = actionTrack.TrackData.Area;
                                ImGui.InputInt("Entity Area (-1: Default)" + $"###{actIndex}", ref x);
                                anyFieldFocused |= ImGui.IsItemActive();
                                x = Math.Min(Math.Max(x, -1), 99);
                                actionTrack.TrackData.Area = (sbyte)x;

                                x = actionTrack.TrackData.Block;
                                ImGui.InputInt("Entity Block (-1: Default)" + $"###{actIndex}", ref x);
                                anyFieldFocused |= ImGui.IsItemActive();
                                x = Math.Min(Math.Max(x, -1), 99);
                                actionTrack.TrackData.Block = (sbyte)x;
                            }


                        }

                        

                        



                        if (ImGui.TreeNode($"Action Info##{nameof(Window)}.{nameof(Window.Parameters)}_{nameof(DSAProj.EditorInfo)}_Tree"))
                        {
                            ImGui.Text($"Start Time: {act.StartTime}, End Time: {act.EndTime}");

                            

                            

                            ImGui.Separator();

                            bool actInfoAnyFieldFocused = false;
                            act.Info.ShowImGui(ref actInfoAnyFieldFocused, MainTaeScreen.Proj,
                                GetRect(subtractScrollBar: true).DpiScaled(),
                                GetRect(subtractScrollBar: false).DpiScaled(),
                                isModified =>
                                {
                                    if (isModified)
                                    {
                                        MainTaeScreen.SelectedAnim.SAFE_SetIsModified(true);
                                        MainTaeScreen.SelectedAnimCategory.SAFE_SetIsModified(true);
                                    }

                                }, showTagsOnly: false, out bool anyEditorInfoModified);

                            if (anyEditorInfoModified)
                            {
                                act.RequestUpdateText();
                            }

                            if (actInfoAnyFieldFocused)
                                anyFieldFocused = true;

                            ImGui.TreePop();
                        }

                        ImGui.Button("Delete Action...");
                        if (ImGui.IsItemClicked())
                        {
                            DialogManager.AskYesNo("Delete Action?", "Are you sure you want to delete this action?", choice =>
                            {
                                if (choice)
                                {
                                    MainTaeScreen.CurrentAnimUndoMan.NewAction(() =>
                                    {
                                        var anim = MainTaeScreen.SelectedAnim;
                                        if (anim != null)
                                        {
                                            anim.SafeAccessActions(actions =>
                                            {
                                                if (actions.Contains(act))
                                                    actions.Remove(act);
                                            });
                                         
                                            if (MainTaeScreen.NewSelectedActions.Contains(act))
                                                MainTaeScreen.NewSelectedActions.Remove(act);
                                            anim.SAFE_SetIsModified(true);
                                        }
                                    }, actionDescription: "Delete action");

                                    breakAfterThisAction = true;
                                }
                            });
                        }
                        ImGui.Separator();

                        ImGui.Text(" ");

                        

                        ImGui.PopItemWidth();

                        var imguiCursorAfterEvent = ImGui.GetCursorPos();
                        var imguiStyle = ImGui.GetStyle();
                        var imguiWinPadding = imguiStyle.WindowPadding;
                        var windowSize = ImGui.GetWindowSize();
                        var windowPos = ImGui.GetWindowPos();
                        var scroll = ImGui.GetScrollY();
                        var hoverRectTopLeft = new System.Numerics.Vector2(imguiCursorBeforeEvent.X, imguiCursorBeforeEvent.Y - scroll);
                        var hoverRectBottomRight = new System.Numerics.Vector2(windowSize.X - (imguiWinPadding.X / 2), imguiCursorAfterEvent.Y - scroll);
                        hoverRectTopLeft += windowPos;
                        hoverRectBottomRight += windowPos;
                        if (ImGui.IsMouseHoveringRect(hoverRectTopLeft, hoverRectBottomRight) && ImGui.IsMouseHoveringRect(windowPos, windowPos + windowSize))
                        {
                            multiEditHoverAction = act;
                            var oldViewport = GFX.Device.Viewport;
                            try
                            {

                                GFX.Device.Viewport = new Microsoft.Xna.Framework.Graphics.Viewport((int)Math.Round(windowPos.X), (int)Math.Round(windowPos.Y),
                                (int)Math.Round(windowSize.X), (int)Math.Round(windowSize.Y));

                                var finalRectPos = hoverRectTopLeft - new System.Numerics.Vector2(2, 2) - windowPos;
                                var finalRectSize = hoverRectBottomRight - hoverRectTopLeft +
                                                           new System.Numerics.Vector2(-19 * Main.DPI, 4);

                                finalRectPos /= Main.DPIVectorN;
                                finalRectSize /= Main.DPIVectorN;

                                ImGuiDebugDrawer.DrawRect(finalRectPos,
                                    finalRectSize, Color.Red, thickness: 1);
                            }
                            finally
                            {
                                GFX.Device.Viewport = oldViewport;
                            }

                        }

                       

                        if (breakAfterThisAction)
                            break;
                    }
                }
                catch (Exception ex) when (Main.EnableErrorHandler.ParameterWindow)
                {
                    Main.HandleError(nameof(Main.EnableErrorHandler.ParameterWindow), ex);
                }
                finally
                {
                    if (isReadOnlyGraph)
                        ImGui.EndDisabled();
                }

                

                MainTaeScreen.MultiEditHoverAction = multiEditHoverAction;
                if (!anyFieldFocused)
                {
                    lock (_lock_TemplateRefreshQueue)
                    {
                        foreach (var template in TemplateRefreshesQueued)
                        {
                            if (!isReadOnlyGraph)
                                MainTaeScreen.RefreshTemplateForAllActions(template, isSoftRefresh: true);
                        }

                        TemplateRefreshesQueued.Clear();
                    }
                }
            }
        }
    }
}
