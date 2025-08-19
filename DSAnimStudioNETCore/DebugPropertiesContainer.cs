using DSAnimStudio.ImguiOSD;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace DSAnimStudio
{
    public abstract class DebugPropertiesContainer<TContainer>
        where TContainer : DebugPropertiesContainer<TContainer>
    {
        private object _lock = new object();

        private System.Reflection.FieldInfo[] _fields;

        public string XmlElementName => typeof(TContainer).Name;

        protected DebugPropertiesContainer()
        {
            RefreshReflection();
        }

        public void RefreshReflection()
        {
            lock (_lock)
            {
                var containerType = typeof(TContainer);
                _fields = containerType.GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray();
            }
        }

        public void SetAllToggles(bool val)
        {
            lock (_lock)
            {
                foreach (var f in _fields)
                {
                    if (f.FieldType == typeof(bool))
                        f.SetValue(this, val);
                }
            }
        }
        
        public void WriteXmlElement(XmlWriter writer)
        {
            lock (_lock)
            {
                writer.WriteStartElement(XmlElementName);
                {
                    lock (_lock)
                    {
                        foreach (var f in _fields)
                        {
                            writer.WriteStartElement("field");
                            writer.WriteAttributeString("name", f.Name);
                            var value = f.GetValue(this);
                            if (value is bool asBool)
                            {
                                writer.WriteAttributeString("value", asBool ? "true" : "false");
                            }
                            else if (value is Enum asEnum)
                            {
                                writer.WriteAttributeString("value", asEnum.ToString());
                            }
                            else
                            {
                                throw new NotImplementedException(
                                    $"DebugPropertiesContainer - Xml element write not implemented for Type '{f.FieldType}'.");
                            }

                            writer.WriteEndElement();
                        }
                    }
                }
                writer.WriteEndElement();
            }
        }

        public void ReadXmlElement(XmlNode xml)
        {
            lock (_lock)
            {
                XmlNode rootNode = xml.SelectSingleNode(XmlElementName);
                foreach (XmlNode fieldNode in rootNode.SelectNodes("field"))
                {
                    string fieldName = fieldNode.Attributes["name"].InnerText;
                    string fieldValue = fieldNode.Attributes["value"].InnerText;
                    var matchingField = _fields.FirstOrDefault(f => f.Name == fieldName);
                    if (matchingField != null)
                    {
                        object value = matchingField.GetValue(this);
                        bool valueIsInvalid = false;
                        if (value is bool asBool)
                        {
                            if (fieldValue == "true")
                                value = true;
                            else if (fieldValue == "false")
                                value = false;
                            else
                                valueIsInvalid = true;
                        }
                        else if (value is Enum asEnum)
                        {
                            value = Enum.Parse(matchingField.FieldType, fieldValue);
                        }
                        else
                        {
                            throw new NotImplementedException(
                                $"DebugPropertiesContainer - Xml element read not implemented for Type '{matchingField.FieldType}'.");
                        }

                        if (valueIsInvalid)
                        {
                            throw new Exception(
                                $"DebugPropertiesContainer - Invalid value string in xml for field '{fieldName}'.");
                        }
                        else
                        {
                            matchingField.SetValue(this, value);
                        }


                    }
                    else
                    {
                        throw new Exception($"DebugPropertiesContainer - Field '{fieldName}' does not exist");
                    }

                }
            }
        }

        private void DoImguiWidgetForField(object obj, FieldInfo f)
        {
            var curValObj = f.GetValue(obj);
            if (curValObj is bool asBool)
            {
                bool current = asBool;
                bool prev = current;
                ImGui.Checkbox(f.Name, ref current);
                if (prev != current)
                    f.SetValue(obj, current);
            }
            else if (curValObj is Enum asEnum)
            {
                object current = asEnum;
                Tools.EnumPicker(f.Name, ref current, f.FieldType);
                f.SetValue(obj, current);
            }
            else
            {
                throw new NotImplementedException($"DebugPropertiesContainer - ImGui debug widget not implemented for Type '{f.FieldType}'.");
            }
        }
        
        public void BuildImguiWidget(string treeNodeName)
        {
            ImGui.PushID($"DEBUG->DebugPropertiesContainer:{typeof(TContainer).Name}");
            try
            {
                ImGui.SetNextItemOpen(true, ImGuiCond.Once);
                if (ImGui.TreeNode(treeNodeName))
                {
                    if (Tools.SimpleClickButton("Enable All"))
                    {
                        SetAllToggles(true);
                    }
                    ImGui.SameLine();
                    if (Tools.SimpleClickButton("Disable All"))
                    {
                        SetAllToggles(false);
                    }
                    
                    
                    ImGui.Separator();

                    if (Tools.SimpleClickButton("Reinit Reflection"))
                    {
                        RefreshReflection();
                    }

                
                    ImGui.Separator();

                    lock (_lock)
                    {
                        if (_fields != null)
                        {
                            foreach (var f in _fields)
                            {
                                DoImguiWidgetForField(this, f);
                            
                            }
                        }
                    }
                    
                    ImGui.TreePop();
                }

                
               
            }
            finally
            {
                ImGui.PopID();
            }

        }
    }
}