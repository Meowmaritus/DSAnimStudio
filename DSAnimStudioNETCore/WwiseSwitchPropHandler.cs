using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public abstract class WwiseSwitchPropHandler
    {
        private object _lock = new object();
        public readonly string SwitchPropKey;
        public readonly uint SwitchPropKeyHash;

        protected WwiseSwitchPropHandler(string switchPropKey)
        {
            SwitchPropKey = switchPropKey;
            SwitchPropKeyHash = zzz_SoundManagerIns.GetFnvHashOfString(switchPropKey);
        }

        
        protected abstract uint InnerGetSwitchPropValueHash(zzz_WwiseManagerInst ww);
        public uint GetSwitchPropValueHash(zzz_WwiseManagerInst ww)
        {
            uint result = 0;
            lock (_lock)
            {
                result = InnerGetSwitchPropValueHash(ww);
            }
            return result;
        }

        public bool BuildImguiIfApplicable(zzz_WwiseManagerInst ww, ref bool anyItemFocused)
        {
            var hasImgui = HasImguiInterface();
            if (hasImgui)
            {
                lock (_lock)
                {
                    if (ImGui.TreeNode($"{SwitchPropKey}###WwiseSwitchPropHandler_{SwitchPropKey}"))
                    {
                        bool innerItemFocused = false;
                        InnerBuildImguiIfApplicable(ww, ref innerItemFocused);
                        if (innerItemFocused)
                            anyItemFocused = true;
                        ImGui.TreePop();
                    }
                    
                }
                return true;
            }
            return false;
        }



        public abstract string Config_GetValue(zzz_WwiseManagerInst ww);
        public abstract void Config_SetValue(zzz_WwiseManagerInst ww, string value);
        public abstract bool Config_GetAutoEnabled();
        public abstract void Config_SetAutoEnabled(bool value);

        /// <summary>
        /// Returns true if ImGui interface was built, so OSD system can add separator etc.
        /// </summary>
        /// <returns></returns>
        protected abstract void InnerBuildImguiIfApplicable(zzz_WwiseManagerInst ww, ref bool anyItemFocused);

        protected abstract bool HasImguiInterface();

        //public class HardcodedMeme : WwiseSwitchPropHandler
        //{


        //    protected override string InnerGetSwitchPropValue(zzz_DocumentIns doc)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    protected override void InnerBuildImguiIfApplicable(zzz_DocumentIns doc, ref bool anyItemFocused)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    protected override bool HasImguiInterface()
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        public class ManualInput : WwiseSwitchPropHandler
        {
            public enum AutomationTypes
            {
                None = 0,
                AC6_Legs_ID = 1,
            }
            public AutomationTypes AutomationType;
            public bool AutomationEnabled = false;

            public string AutomationResultValue = "";
            public uint AutomationResultValueHash;

            public ManualInput(string switchPropKey, string initValue, AutomationTypes automationType = AutomationTypes.None, bool enableAutomationByDefault = true)
                : base(switchPropKey)
            {
                ManuallyPickedValue = initValue;
                AutomationType = automationType;
                AutomationEnabled = enableAutomationByDefault && automationType != AutomationTypes.None;
            }


            public string ManuallyPickedValue = "";

            private string GetAutomationValue(zzz_WwiseManagerInst ww)
            {
                switch (AutomationType)
                {
                    case AutomationTypes.AC6_Legs_ID:
                        string leg_id = "";

                        ww.ParentDocument.Scene.AccessMainModel(m =>
                        {
                            m.ChrAsm.AccessArmorSlot(NewChrAsm.EquipSlotTypes.Legs, lg =>
                            {
                                leg_id = $"lg{((ushort)lg.EquipParam.AC6LegTypeSEID).ToString("000000000")}";
                            });
                        });

                        return leg_id;

                    default:
                        return "";
                }
            }

            protected override uint InnerGetSwitchPropValueHash(zzz_WwiseManagerInst ww)
            {
                if (AutomationEnabled && AutomationType != AutomationTypes.None)
                {
                    string newVal = GetAutomationValue(ww);
                    if (newVal != AutomationResultValue)
                    {
                        AutomationResultValue = newVal;
                        AutomationResultValueHash = zzz_SoundManagerIns.GetFnvHashOfString(newVal);
                    }
                    return AutomationResultValueHash;
                }
                else
                {
                    return zzz_SoundManagerIns.GetFnvHashOfString(ManuallyPickedValue);
                }

                
            }

            protected override bool HasImguiInterface() => true;

            protected override void InnerBuildImguiIfApplicable(zzz_WwiseManagerInst ww, ref bool anyItemFocused)
            {
                if (AutomationType != AutomationTypes.None)
                {
                    ImGui.Checkbox($"Handle Automatically###WwiseSwitchPropHandler_{SwitchPropKey}_HandleAutomatically", ref AutomationEnabled);
                }

                if (AutomationEnabled && AutomationType != AutomationTypes.None)
                {
                    if (!string.IsNullOrEmpty(AutomationResultValue))
                        ImGui.Text($"Selected Value: {AutomationResultValue}");
                    else
                        ImGui.Text($"Selected Entry: <None>");
                }
                else
                {
                    ImGui.InputText($"###WwiseSwitchPropHandler_{SwitchPropKey}_TextField", ref ManuallyPickedValue, 256);
                    if (ImGui.IsItemFocused())
                        anyItemFocused = true;
                }

                
            }

            public override string Config_GetValue(zzz_WwiseManagerInst ww)
            {
                return ManuallyPickedValue;
            }

            public override void Config_SetValue(zzz_WwiseManagerInst ww, string value)
            {
                ManuallyPickedValue = value;
            }

            public override bool Config_GetAutoEnabled()
            {
                return AutomationEnabled;
            }

            public override void Config_SetAutoEnabled(bool value)
            {
                AutomationEnabled = value;
            }
        }

        public class ListSelector : WwiseSwitchPropHandler
        {
            private string[] ValueNames;
            private string[] Values;
            private uint[] ValueHashes;
            private int SelectedValueIndex;

            private string CustomValue = "";
            private uint CustomValueHash;

            

            public enum AutomationTypes
            {
                None = 0,
                AC6_Legs_Type = 1,
            }
            public AutomationTypes AutomationType;
            public bool AutomationEnabled = false;
            public string AutomationResultValue;
            public uint AutomationResultValueHash;

            public ListSelector(string switchPropKey, Dictionary<string, string> valueList, string initValue, AutomationTypes automationType = AutomationTypes.None, bool enableAutomationByDefault = true)
                : base(switchPropKey)
            {
                List<string> valueNames = new List<string>();
                List<string> values = new List<string>();
                List<uint> valueHashes = new List<uint>();
                foreach (var kvp in valueList)
                {
                    valueNames.Add(kvp.Key != null ? $"{kvp.Key} ['{kvp.Value}']" : kvp.Value);
                    values.Add(kvp.Value);
                    valueHashes.Add(zzz_SoundManagerIns.GetFnvHashOfString(kvp.Value));
                }

                valueNames.Add("<Custom>");
                values.Add(null);
                valueHashes.Add(0);

                ValueNames = valueNames.ToArray();
                Values = values.ToArray();
                ValueHashes = valueHashes.ToArray();



                SelectedValueIndex = valueNames.IndexOf(initValue);
                if (SelectedValueIndex < 0)
                {
                    CustomValue = initValue;
                    CustomValueHash = zzz_SoundManagerIns.GetFnvHashOfString(initValue);
                    SelectedValueIndex = valueNames.Count - 1;
                }
                AutomationType = automationType;
                AutomationEnabled = enableAutomationByDefault && automationType != AutomationTypes.None;
            }

            private string GetAutomationValue(zzz_WwiseManagerInst ww)
            {
                switch (AutomationType)
                {
                    case AutomationTypes.AC6_Legs_Type:
                        string leg_type = "";

                        ww.ParentDocument.Scene.AccessMainModel(m =>
                        {
                            m.ChrAsm.AccessArmorSlot(NewChrAsm.EquipSlotTypes.Legs, lg =>
                            {
                                switch (lg.EquipParam.AC6LegTypeModelID)
                                {
                                    case ParamData.EquipParamProtector.AC6LegTypeModelIDs.Bipedal:
                                        leg_type = "two_legs";
                                        break;
                                    case ParamData.EquipParamProtector.AC6LegTypeModelIDs.ReverseJoint:
                                        leg_type = "reverse_legs";
                                        break;
                                    case ParamData.EquipParamProtector.AC6LegTypeModelIDs.Tetrapod:
                                        leg_type = "four_legs";
                                        break;
                                    case ParamData.EquipParamProtector.AC6LegTypeModelIDs.Tank1:
                                    case ParamData.EquipParamProtector.AC6LegTypeModelIDs.Tank2:
                                    case ParamData.EquipParamProtector.AC6LegTypeModelIDs.Tank3:
                                        leg_type = "crawler_track";
                                        break;
                                }
                            });
                        });

                        return leg_type;

                    default:
                        return "";
                }
            }

            protected override uint InnerGetSwitchPropValueHash(zzz_WwiseManagerInst ww)
            {
                if (AutomationEnabled && AutomationType != AutomationTypes.None)
                {
                    string newVal = GetAutomationValue(ww);
                    if (newVal != AutomationResultValue)
                    {
                        AutomationResultValue = newVal;
                        AutomationResultValueHash = zzz_SoundManagerIns.GetFnvHashOfString(newVal);
                    }
                    return AutomationResultValueHash;
                }
                else
                {

                    if (SelectedValueIndex == Values.Length - 1)
                    {
                        return CustomValueHash;
                    }
                    else
                    {
                        if (SelectedValueIndex >= 0 && SelectedValueIndex < ValueHashes.Length)
                            return ValueHashes[SelectedValueIndex];
                        else
                            return 0;
                    }

                }

                
            }

            protected override void InnerBuildImguiIfApplicable(zzz_WwiseManagerInst ww, ref bool anyItemFocused)
            {
                if (AutomationType != AutomationTypes.None)
                {
                    ImGui.Checkbox($"Handle Automatically###WwiseSwitchPropHandler_{SwitchPropKey}_HandleAutomatically", ref AutomationEnabled);
                }

                if (AutomationEnabled && AutomationType != AutomationTypes.None)
                {
                    if (!string.IsNullOrEmpty(AutomationResultValue))
                        ImGui.Text($"Selected Value: {AutomationResultValue}");
                    else
                        ImGui.Text($"Selected Entry: <None>");
                }
                else
                {
                    ImGui.ListBox($"###WwiseSwitchPropHandler_{SwitchPropKey}_ListBox", ref SelectedValueIndex, ValueNames, ValueNames.Length);
                    if (ImGui.IsItemFocused())
                        anyItemFocused = true;

                    if (SelectedValueIndex == Values.Length - 1)
                    {
                        ImGui.Text("Custom Value:");
                        var prevCustomValue = CustomValue;
                        ImGui.InputText($"Custom Value###WwiseSwitchPropHandler_{SwitchPropKey}_CustomValueInput", ref CustomValue, 256);
                        if (ImGui.IsItemFocused())
                            anyItemFocused = true;
                        if (CustomValue != prevCustomValue)
                        {
                            if (string.IsNullOrEmpty(CustomValue))
                                CustomValueHash = 0;
                            else
                                CustomValueHash = zzz_SoundManagerIns.GetFnvHashOfString(CustomValue);
                        }
                    }
                }



                
            }

            protected override bool HasImguiInterface() => true;

            public override string Config_GetValue(zzz_WwiseManagerInst ww)
            {
                if (SelectedValueIndex == Values.Length - 1)
                    return CustomValue;
                else if (SelectedValueIndex >= 0 && SelectedValueIndex < Values.Length)
                    return Values[SelectedValueIndex];
                else
                    return "";
            }

            public override void Config_SetValue(zzz_WwiseManagerInst ww, string value)
            {
                var valueList = Values.ToList();
                if (valueList.Contains(value))
                {
                    SelectedValueIndex = valueList.IndexOf(value);
                    CustomValue = "";
                }
                else
                {
                    SelectedValueIndex = Values.Length - 1;
                    CustomValue = value;
                    CustomValueHash = zzz_SoundManagerIns.GetFnvHashOfString(value);
                }
            }

            public override bool Config_GetAutoEnabled()
            {
                return AutomationEnabled;
            }

            public override void Config_SetAutoEnabled(bool value)
            {
                AutomationEnabled = value;
            }
        }

        public class Param : WwiseSwitchPropHandler
        {
            public readonly string ParamName;
            bool hasCopiedFromParams = false;

            private string[] EntryNames;
            private uint[] EntryValueHashes;
            private string[] EntryValues;
            private int[] EntryParamRowIDs;

            private Dictionary<int, int> ParamRowIDToListIndexMap = new Dictionary<int, int>();

            private int SelectedEntryIndex = -1;
            private int DefaultParamRowID;



            private string CustomValue = "";
            private uint CustomValueHash;


            //private int GetEntryIndexForParamRowID(int rowID)
            //{
            //    if (ParamRowIDToListIndexMap.ContainsKey(rowID))
            //        return ParamRowIDToListIndexMap[rowID];

            //    return -1;
            //}

            public enum AutomationTypes
            {
                None = 0,
                PlayerEquipmentTops = 1,
                PlayerEquipmentBottoms = 2,
                DeffensiveMaterial = 3,
            }

            public AutomationTypes AutomationType = AutomationTypes.None;
            public bool AutomationEnabled = false;


            public Param(string switchParamName, string switchPropKey, int defaultParamRowID, 
                AutomationTypes automationType = AutomationTypes.None, bool enableAutomationByDefault = true)
                : base(switchPropKey)
            {
                ParamName = switchParamName;
                DefaultParamRowID = defaultParamRowID;
                AutomationType = automationType;
                AutomationEnabled = (automationType != AutomationTypes.None && enableAutomationByDefault);
            }

            private void CopyFromParamsIfHaventYet(zzz_WwiseManagerInst ww)
            {
                if (!hasCopiedFromParams)
                {
                    var param = ww.ParentDocument.ParamManager.GetParam(ParamName);

                    var entryNames = new List<string>();
                    var entryValues = new List<string>();
                    var entryValueHashes = new List<uint>();
                    var entryParamRowIDs = new List<int>();
                    ParamRowIDToListIndexMap.Clear();

                    for (int i = 0; i < param.Rows.Count; i++)
                    {
                        var row = param.Rows[i];

                        if (row.ID == DefaultParamRowID)
                        {
                            SelectedEntryIndex = i;
                        }

                        var x = new ParamData.WwiseValueToStrParam();
                        var br = param.GetRowReader(row);
                        x.Read(br);
                        entryValues.Add(x.WwiseString);
                        entryValueHashes.Add(zzz_SoundManagerIns.GetFnvHashOfString(x.WwiseString));
                        entryParamRowIDs.Add(row.ID);

                        // Only add if hasn't been added to simulate game's duplicate ID handling of 
                        // picking the first occurrence or something.
                        if (!ParamRowIDToListIndexMap.ContainsKey(row.ID))
                            ParamRowIDToListIndexMap[row.ID] = i;

                        if (row.Name != null)
                        {
                            entryNames.Add($"{row.Name} [{row.ID}]: '{x.WwiseString}'");
                        }
                        else
                        {
                            entryNames.Add($"[{row.ID}]: '{x.WwiseString}'");
                        }
                        
                    }

                    entryNames.Add("<Custom>");
                    entryValueHashes.Add(0);
                    entryValues.Add(null);

                    EntryNames = entryNames.ToArray();
                    EntryValueHashes = entryValueHashes.ToArray();
                    EntryValues = entryValues.ToArray();
                    EntryParamRowIDs = entryParamRowIDs.ToArray();

                    hasCopiedFromParams = true;
                }

                
            }

            private int GetAutomationParamRowID(zzz_WwiseManagerInst ww)
            {
                switch (AutomationType)
                {
                    case AutomationTypes.PlayerEquipmentTops:
                        return ww.ArmorMaterial_Top;
                    case AutomationTypes.PlayerEquipmentBottoms:
                        return ww.ArmorMaterial_Bottom;
                    case AutomationTypes.DeffensiveMaterial:
                        return ww.GetDefensiveMaterialParamID();
                    default:
                        return -1;
                }
            }

            protected override uint InnerGetSwitchPropValueHash(zzz_WwiseManagerInst ww)
            {
                CopyFromParamsIfHaventYet(ww);

                if (AutomationEnabled)
                {
                    var rowID = GetAutomationParamRowID(ww);
                    if (ParamRowIDToListIndexMap.ContainsKey(rowID))
                        SelectedEntryIndex = ParamRowIDToListIndexMap[rowID];
                    else
                        SelectedEntryIndex = -1;
                }

                if (SelectedEntryIndex == EntryValueHashes.Length - 1)
                    return CustomValueHash;
                else if (SelectedEntryIndex >= 0 && SelectedEntryIndex < EntryValueHashes.Length)
                    return EntryValueHashes[SelectedEntryIndex];
                else
                    return 0;
            }

            protected override bool HasImguiInterface() => true;

            protected override void InnerBuildImguiIfApplicable(zzz_WwiseManagerInst ww, ref bool anyItemFocused)
            {
                CopyFromParamsIfHaventYet(ww);

                if (AutomationType != AutomationTypes.None)
                {
                    ImGui.Checkbox($"Handle Automatically###WwiseSwitchPropHandler_{SwitchPropKey}_HandleAutomatically", ref AutomationEnabled);
                }

                if (AutomationEnabled && AutomationType != AutomationTypes.None)
                {
                    if (SelectedEntryIndex >= 0 && SelectedEntryIndex < EntryNames.Length)
                        ImGui.Text($"Selected Entry: {EntryNames[SelectedEntryIndex]}");
                    else
                        ImGui.Text($"Selected Entry: <None>");
                }
                else
                {
                    ImGui.ListBox($"###WwiseSwitchPropHandler_{SwitchPropKey}_ListBox", ref SelectedEntryIndex, EntryNames, EntryNames.Length);
                    if (ImGui.IsItemFocused())
                        anyItemFocused = true;

                    if (SelectedEntryIndex == EntryValues.Length - 1)
                    {
                        ImGui.Text("Custom Value:");
                        var prevCustomValue = CustomValue;
                        ImGui.InputText($"Custom Value###WwiseSwitchPropHandler_{SwitchPropKey}_CustomValueInput", ref CustomValue, 256);
                        if (ImGui.IsItemFocused())
                            anyItemFocused = true;
                        if (CustomValue != prevCustomValue)
                        {
                            if (string.IsNullOrEmpty(CustomValue))
                                CustomValueHash = 0;
                            else
                                CustomValueHash = zzz_SoundManagerIns.GetFnvHashOfString(CustomValue);
                        }
                    }

                }

                    
            }

            public override string Config_GetValue(zzz_WwiseManagerInst ww)
            {
                CopyFromParamsIfHaventYet(ww);

                if (AutomationEnabled)
                {
                    var rowID = GetAutomationParamRowID(ww);
                    if (ParamRowIDToListIndexMap.ContainsKey(rowID))
                        SelectedEntryIndex = ParamRowIDToListIndexMap[rowID];
                    else
                        SelectedEntryIndex = -1;
                }

                if (SelectedEntryIndex == EntryValues.Length - 1)
                    return CustomValue;
                else if (SelectedEntryIndex >= 0 && SelectedEntryIndex < EntryValues.Length)
                    return EntryValues[SelectedEntryIndex];
                else
                    return "";
            }

            public override void Config_SetValue(zzz_WwiseManagerInst ww, string value)
            {
                var valueList = EntryValues.ToList();
                SelectedEntryIndex = valueList.IndexOf(value);
                if (SelectedEntryIndex < 0)
                {
                    SelectedEntryIndex = EntryValues.Length - 1;
                    CustomValue = value;
                    CustomValueHash = zzz_SoundManagerIns.GetFnvHashOfString(value);
                }
                throw new NotImplementedException();
            }

            public override bool Config_GetAutoEnabled()
            {
                return AutomationEnabled;
            }

            public override void Config_SetAutoEnabled(bool value)
            {
                AutomationEnabled = value;
            }
        }
    }
}
