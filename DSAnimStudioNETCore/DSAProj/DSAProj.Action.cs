using DSAnimStudio.TaeEditor;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsAssetPipeline.Animation.TAE;

namespace DSAnimStudio
{
    public partial class DSAProj
    {
        public class Action
        {
            public readonly string GUID = Guid.NewGuid().ToString();
            
            public float EditorHighlightDelayTimer = -1;
            public float EditorHighlightTimer = -1;

            public bool NeedsTextRefresh = false;
            public bool NeedsTextRefresh_LoadParams = false;

            public EditorInfo Info = new EditorInfo();
            public int Type;
            public bool Mute;
            public bool Solo;

            private float _startTime;
            private float _endTime;
            private int _trackIndex;

            public float StartTime
            {
                get => _startTime;
                set
                {
                    if (value != _startTime)
                    {
                        _startTime = value;
                        NeedsTextRefresh = true;
                    }
                }
            }
            public float EndTime
            {
                get => _endTime;
                set
                {
                    if (value != _endTime)
                    {
                        _endTime = value;
                        NeedsTextRefresh = true;
                    }
                }
            }
            public int TrackIndex
            {
                get => _trackIndex;
                set
                {
                    if (value != _trackIndex)
                    {
                        _trackIndex = value;
                        NeedsTextRefresh = true;
                    }
                }
            }

            public int Unk04;
            public byte[] ParameterBytes;
            public int OriginalParamBytesLength = -1;


            public bool Overlaps(Action other, bool perfectMatch)
            {
                if (TrackIndex != other.TrackIndex)
                    return false;

                int thisStartFrame = TaeExtensionMethods.GetTimeInIntegerFrames(StartTime) ?? (int)(StartTime / 0.001);
                int thisEndFrame = TaeExtensionMethods.GetTimeInIntegerFrames(EndTime) ?? (int)(EndTime / 0.001);
                int otherStartFrame = TaeExtensionMethods.GetTimeInIntegerFrames(other.StartTime) ?? (int)(other.StartTime / 0.001);
                int otherEndFrame = TaeExtensionMethods.GetTimeInIntegerFrames(other.EndTime) ?? (int)(other.EndTime / 0.001);

                if (perfectMatch)
                {
                    return thisStartFrame == otherStartFrame && thisEndFrame == otherEndFrame;
                }
                else
                {
                    var thisRectMeme = new Rectangle(thisStartFrame, 0, thisEndFrame - thisStartFrame, 16);
                    var otherRectMeme = new Rectangle(otherStartFrame, 0, otherEndFrame - otherStartFrame, 16);
                    return (thisRectMeme.Intersects(otherRectMeme));
                }
            }

            public long GetTrackSortRefID()
            {
                if (Type == 16)
                    return -1;
                if (Type == 0 || Type == 300)
                {
                    int chrActionFlag = Convert.ToInt32(this.ReadInternalSimField("JumpTableID"));
                    return chrActionFlag;
                }
                return Type * 0x1_00000000;
            }

            public int GetStateInfoID()
            {
                if (this.HasInternalSimField("StateInfo"))
                {
                    return Convert.ToInt32(this.ReadInternalSimField("StateInfo"));
                }
                
                return -1;
            }

            public Action GetClone()
            {
                var clone = new Action();
                clone.Type = Type;
                clone.StartTime = StartTime;
                clone.EndTime = EndTime;
                clone.TrackIndex = TrackIndex;
                clone.Unk04 = Unk04;
                clone.Template = Template;
                if (Template != null)
                    NewSaveParamsToBytes();
                clone.ParameterBytes = ParameterBytes.ToArray();
                clone.OriginalParamBytesLength = OriginalParamBytesLength;
                clone.Info = Info.GetClone();
                return clone;
            }

            public void Deserialize(BinaryReaderEx br, DSAProj proj)
            {
                if (proj.FILE_VERSION >= Versions.v20_00_00)
                {
                    Info = new EditorInfo();
                    Info.Deserialize(br, proj);
                }

                Type = br.ReadInt32();
                if (proj.FILE_VERSION >= Versions.v5)
                {
                    Mute = br.ReadBoolean();
                    Solo = br.ReadBoolean();
                }
                else
                {
                    Mute = false;
                    Solo = false;
                }
                StartTime = br.ReadSingle();
                EndTime = br.ReadSingle();
                TrackIndex = br.ReadInt32();
                Unk04 = br.ReadInt32();
                int paramByteCount = br.ReadInt32();
                ParameterBytes = br.ReadBytes(paramByteCount);
                OriginalParamBytesLength = ParameterBytes.Length;
                
                
                if (proj.FILE_VERSION < Versions.v20_00_00)
                {
                    Info = new EditorInfo();
                    var editorText = br.ReadNullPrefixUTF16();
                    Color? customColor = null;
                    if (proj.FILE_VERSION >= Versions.v11)
                        customColor = br.ReadNullPrefixColor();
                    Info.CustomColor = customColor;
                    Info.Description = editorText;
                }
            }

            public void Serialize(BinaryWriterEx bw, DSAProj proj)
            {
                Info.Serialize(bw, proj);
                bw.WriteInt32(Type);
                bw.WriteBoolean(Mute);
                bw.WriteBoolean(Solo);
                bw.WriteSingle(StartTime);
                bw.WriteSingle(EndTime);
                bw.WriteInt32(TrackIndex);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(ParameterBytes.Length);
                bw.WriteBytes(ParameterBytes);
            }


            // Runtime memes
            [JsonIgnore]
            public bool NewSimulationEnter;
            [JsonIgnore]
            public bool NewSimulationExit;
            [JsonIgnore]
            public bool NewSimulationActive;
            //[JsonIgnore]
            //public bool PrevFrameEnteredState_ForRumbleCamPlayback;
            [JsonIgnore] 
            public bool IsActive => IsActive_BasedOnStateInfo && IsActive_BasedOnMuteSolo;
            [JsonIgnore]
            public bool IsActive_BasedOnStateInfo = true;
            [JsonIgnore]
            public bool IsActive_BasedOnMuteSolo = true;
            //[JsonIgnore]
            //public bool PlaybackHighlightMidst;
            //[JsonIgnore]
            //public bool PlaybackHighlight;
            //[JsonIgnore]
            //public bool PrevFrameEnteredState_ForSoundEffectPlayback;
            //[JsonIgnore]
            //public bool PrevCyclePlaybackHighlight;
            [JsonIgnore]
            public string DisplayStringForFmod = "";
            [JsonIgnore]
            public string GraphDisplayText = null;
            [JsonIgnore]
            public string TooltipDisplayText = null;

            public void INNER_UnloadStringsToSaveMemory()
            {
                TooltipDisplayText = null;
                GraphDisplayText = null;
                NeedsTextRefresh = true;
            }

            public static Action FromBinary(SoulsAssetPipeline.Animation.TAE.Action ev)
            {
                var e = new Action();

                e.Type = ev.Type;
                e.StartTime = ev.StartTime;
                e.EndTime = ev.EndTime;
                e.TrackIndex = ev.TrackIndex;
                e.Unk04  = ev.Unk04;
                e.ParameterBytes = ev.ParameterBytes.ToArray();
                e.OriginalParamBytesLength = e.ParameterBytes.Length;

                return e;
            }

            public SoulsAssetPipeline.Animation.TAE.Action ToBinary()
            {
                var ev = new SoulsAssetPipeline.Animation.TAE.Action(StartTime, EndTime, Type, Unk04, ParameterBytes.ToArray());
                ev.TrackIndex = TrackIndex;
                return ev;
            }















            /// <summary>
            /// A parameter in an event.
            /// </summary>
            public class ParameterContainer
            {
                private List<object> parameterValues;

                /// <summary>
                /// The template of the event for which these are the parameters.
                /// </summary>
                public Template.ActionTemplate Template { get; private set; }

                /// <summary>
                /// Returns all parameters.
                /// </summary>
                public IReadOnlyList<object> Values
                    => parameterValues;

                public List<string> GetValueStrings()
                {
                    var result = new List<string>();
                    for (int i = 0; i < Template.Count; i++)
                    {
                        result.Add(Template[i].ValueToString(parameterValues[i]));
                    }

                    return result;
                }
                
                public List<string> GetKeyValueStrings()
                {
                    var result = new List<string>();
                    for (int i = 0; i < Template.Count; i++)
                    {
                        result.Add(Template[i].Name + ": " + Template[i].ValueToString(parameterValues[i]));
                    }

                    return result;
                }

                /// <summary>
                /// Value of the specified parameter.
                /// </summary>
                public object this[int paramName]
                {
                    get => parameterValues[paramName];
                    set => parameterValues[paramName] = value;
                }

                ///// <summary>
                ///// Gets the value of a parameter.
                ///// </summary>
                //public object GetParamValue(string paramName)
                //{
                //    return this[paramName];
                //}

                ///// <summary>
                ///// Gets the value type of a parameter.
                ///// </summary>
                //public Template.ParamType GetParamValueType(string paramName)
                //{
                //    return Template[paramName].Type;
                //}

                ///// <summary>
                ///// Gets the whole template of a parameter.
                ///// </summary>
                //public Template.ParameterTemplate GetParamTemplate(string paramName)
                //{
                //    return Template[paramName];
                //}

                internal ParameterContainer(long animID, int eventIndex,
                    byte[] paramData, Template.ActionTemplate template, bool suppressAssert = false)
                {
                    parameterValues = new List<object>();
                    Template = template;
                    using (var memStream = new System.IO.MemoryStream(paramData))
                    {
                        var br = new BinaryReaderEx(template.BigEndian, memStream);
                        int i = 0;
                        foreach (var paramKvp in Template)
                        {
                            var p = paramKvp;
                            if (p.ValueToAssert != null)
                            {
                                if (!suppressAssert)
                                {
                                    try
                                    {
                                        p.AssertValue(br);
                                        parameterValues.Add(p.ValueToAssert);
                                    }
                                    catch (System.IO.InvalidDataException ex)
                                    {
                                        var txtField = p.Name != null ? $"'{p.Name}'" : $"{(i + 1)} of {Template.Count}";
                                        var txtActionType = Template.Name != null ? $"'{Template.Name}'" : Template.ID.ToString();

                                        throw new Exception($"Animation {animID}\nAction[{eventIndex}] (Type: {txtActionType})" +
                                                $"\n  -> Assert failed on field {txtField} (Type: {p.ParamType})", ex);
                                    }
                                }
                                else
                                {
                                    // lol I'm sorry TK
                                    parameterValues.Add(p.ReadValue(br));
                                }

                                //parameterValues.Add(0);
                            }
                            else
                            {

                                try
                                {
                                    parameterValues.Add(p.ReadValue(br));
                                }
                                catch (Exception ex)
                                {
                                    var txtField = p.Name != null ? $"'{p.Name}'" : $"{(i + 1)} of {Template.Count}";
                                    var txtActionType = Template.Name != null ? $"'{Template.Name}'" : Template.ID.ToString();

                                    throw new Exception($"Animation {animID}\nAction[{eventIndex}] (Type: {txtActionType})" +
                                            $"\n  -> Failed to read value of field {txtField} (Type: {p.ParamType})", ex);
                                }
                            }
                            i++;
                        }
                    }
                }

                internal ParameterContainer(byte[] paramData, Template.ActionTemplate template, bool suppressAssert = false)
                {
                    parameterValues = new List<object>();
                    Template = template;

                    var numBytes = template.GetAllParametersByteCount();
                    if (paramData.Length < numBytes)
                    {
                        Array.Resize(ref paramData, numBytes);
                    }

                    using (var memStream = new System.IO.MemoryStream(paramData))
                    {
                        var br = new BinaryReaderEx(template.BigEndian, memStream);
                        int i = 0;
                        foreach (var paramKvp in Template)
                        {
                            var p = paramKvp;
                            if (p.ValueToAssert != null)
                            {
                                if (!suppressAssert)
                                {
                                    try
                                    {
                                        p.AssertValue(br);
                                        parameterValues.Add(p.ValueToAssert);
                                    }
                                    catch (System.IO.InvalidDataException ex)
                                    {
                                        var txtField = p.Name != null ? $"'{p.Name}'" : $"{(i + 1)} of {Template.Count}";
                                        var txtActionType = Template.Name != null ? $"'{Template.Name}'" : Template.ID.ToString();

                                        throw new Exception($"Action Type: {txtActionType}" +
                                                $"\n  -> Assert failed on field {txtField}", ex);
                                    }
                                }
                                else
                                {
                                    parameterValues.Add(p.ReadValue(br));
                                }

                                
                            }
                            else
                            {
                                try
                                {
                                    parameterValues.Add(p.ReadValue(br));
                                }
                                catch (Exception ex)
                                {
                                    var txtField = p.Name != null ? $"'{p.Name}'" : $"{(i + 1)} of {Template.Count}";
                                    var txtActionType = Template.Name != null ? $"'{Template.Name}'" : Template.ID.ToString();

                                    throw new Exception($"Action Type: {txtActionType}" +
                                            $"\n  -> Failed to read value of field {txtField} (Type: {p.ParamType})", ex);
                                }
                            }
                            i++;
                        }
                    }
                }

                internal byte[] AsBytes(bool writeAssertValueInsteadOfCurrentValue)
                {
                    using (var memStream = new System.IO.MemoryStream())
                    {
                        var bw = new BinaryWriterEx(Template.BigEndian, memStream);
                        byte[] result = null;
                        try
                        {
                            int i = 0;
                            foreach (var paramKvp in Template)
                            {
                                var p = paramKvp;
                                if (p.ValueToAssert != null)
                                {
                                    if (writeAssertValueInsteadOfCurrentValue)
                                        p.WriteValue(bw, p.ValueToAssert);
                                    else
                                        p.WriteValue(bw, this[i]);
                                }
                                else
                                {
                                    p.WriteValue(bw, this[i]);
                                }

                                i++;
                            }
                            result = memStream.ToArray();
                        }
                        catch when (Main.EnableErrorHandler.ActionParametersWrite)
                        {
                            
                        }

                        return result;
                    }
                }
            }



            public void LazySwitchEventTemplate(SoulsAssetPipeline.Animation.TAE.Template.ActionTemplate evTemplate)
            {
                Type = evTemplate.ID;
                if (Parameters != null)
                    ParameterBytes = Parameters.AsBytes(writeAssertValueInsteadOfCurrentValue: false);
                Array.Resize(ref ParameterBytes, evTemplate.GetAllParametersByteCount());
                Template = evTemplate;
                Parameters = new ParameterContainer(ParameterBytes, evTemplate, suppressAssert: true);
                UpdateGraphDisplayText();
            }

            public BinaryReaderEx GetParamBinReader(bool bigEndian)
            {
                return new BinaryReaderEx(bigEndian, ParameterBytes);
            }

            public bool IsIdenticalTo(Action otherAction)
            {
                return Type == otherAction.Type && ParameterBytes.SequenceEqual(otherAction.ParameterBytes);
            }

            public Action GetClone(bool readFromTemplate)
            {
                NewSaveParamsToBytes();

                var ev = new Action();
                ev.StartTime = StartTime;
                ev.EndTime = EndTime;
                ev.Type = Type;
                ev.Unk04 = Unk04;
                ev.ParameterBytes = ParameterBytes.ToArray();
                ev.OriginalParamBytesLength = OriginalParamBytesLength;
                ev.TrackIndex = TrackIndex;
                ev.Solo = Solo;
                ev.Mute = Mute;

                ev.Template = Template;
                if (readFromTemplate && ev.Template != null)
                    ev.UpdateGraphDisplayText(loadParams: true);
                

                return ev;
            }

            /// <summary>
            /// Indexable parameter container of this event.
            /// Use .Parameters[name] for basic value get/set
            /// and use .GetValueType(name) to see how to convert
            /// it to/from System.Object.
            /// </summary>
            [JsonIgnore]
            public ParameterContainer Parameters { get; private set; }

            /// <summary>
            /// The GUID of the ActionTemplate last used on this action. Check for inequality to know that this action needs to be refreshed.
            /// </summary>
            public string LastTemplateGUID;

            /// <summary>
            /// The EventTemplate applied to this event, if any.
            /// </summary>
            [JsonIgnore] 
            public Template.ActionTemplate Template;

            /// <summary>
            /// Gets the name of this event's type if a template has been loaded.
            /// Otherwise returns null.
            /// </summary>
            [JsonIgnore]
            public string TypeName
            {
                get
                {
                    var template = Parameters?.Template;
                    if (template != null)
                    {
                        if (Main.Config.ShowActionIDs)
                            return $"{template.Name}[{template.ID}]";
                        else
                            return template.Name;
                    }
                    return null;
                }
            }

            /// <summary>
            /// Applies a template to allow editing of the parameters.
            /// </summary>
            internal void ApplyTemplate(AnimCategory containingCategory, Template template,
                long animID, int eventIndex, int eventType)
            {

                if (template.ContainsKey(eventType))
                {
                    Template = template[eventType];
                }
                if (Template != null)
                    NewLoadParamsFromBytes(lenientOnAssert: true);
            }

            internal void ChangeTemplateAfterLoading(AnimCategory containingCategory, Template template,
                long animID, int eventIndex, int eventType)
            {
                ApplyTemplate(containingCategory, template, animID, eventIndex, eventType);
            }

            /// <summary>
            /// Applies a template to allow editing of the parameters.
            /// </summary>
            public void ApplyTemplate(Template.ActionTemplate template)
            {
                if (template.ID != Type)
                {
                    throw new ArgumentException($"Template is for acction type {template.ID} but this action is type {Type}");
                }
                Template = template;
                if (Template != null)
                    NewLoadParamsFromBytes(lenientOnAssert: true);
            }

            /// <summary>
            /// Applies a template to this TAE for editing and also wipes all
            /// values and replaces them with default values.
            /// </summary>
            public void ApplyTemplateWithDefaultValues(Template.ActionTemplate template)
            {
                Type = template.ID;
                ParameterBytes = template.GetDefaultBytes();
                ApplyTemplate(template);
            }


            public void CheckUpdateText()
            {
                if (NeedsTextRefresh)
                {
                    UpdateGraphDisplayText(NeedsTextRefresh_LoadParams);
                    NeedsTextRefresh = false;
                    NeedsTextRefresh_LoadParams = false;
                }
            }

            public void RequestUpdateText(bool loadParams = true)
            {
                NeedsTextRefresh = true;
                NeedsTextRefresh_LoadParams = loadParams;
            }

            public void UpdateGraphDisplayText(bool loadParams = true)
            {
                
                string parameters = "";
                string tooltipParameters = "";
                
                if (Template != null)
                {
                    if (loadParams)
                        NewLoadParamsFromBytes(true);
                    parameters = string.Join(", ",  Parameters.GetValueStrings());
                    tooltipParameters = string.Join("\n  ",  Parameters.GetKeyValueStrings());
                }
                else
                {
                    parameters = string.Join(" ", ParameterBytes.Select(x => x.ToString("X2")));
                    tooltipParameters = string.Join(" ", ParameterBytes.Select(x => x.ToString("X2")));
                }
                string actionName = TypeName ?? $"Type{Type}";
                

                GraphDisplayText = $"{actionName}({parameters})";

                var sb = new StringBuilder();

                if (Info != null)
                {
                    bool anyInfo = false;
                    if (!string.IsNullOrWhiteSpace(Info.DisplayName))
                    {
                        anyInfo = true;
                        sb.AppendLine(Info.DisplayName);
                    }

                    if (!string.IsNullOrWhiteSpace(Info.Description))
                    {
                        anyInfo = true;
                        sb.AppendLine(Info.Description);
                    }

                    if (anyInfo)
                        sb.AppendLine();
                }

                sb.AppendLine(actionName);
                sb.AppendLine($"  {tooltipParameters}");

                sb.AppendLine();
                sb.AppendLine();
                
                int? startFrame = TaeExtensionMethods.GetTimeInIntegerFrames(StartTime);
                int? endFrame = TaeExtensionMethods.GetTimeInIntegerFrames(EndTime);
                
                sb.Append($"Start: {StartTime:0.000} sec");
                if (startFrame.HasValue)
                    sb.AppendLine($" (frame {startFrame.Value})");
                else
                    sb.AppendLine();
                
                sb.Append($"End: {EndTime:0.000} sec");
                
                if (endFrame.HasValue)
                    sb.AppendLine($" (frame {endFrame.Value})");
                else 
                    sb.AppendLine();

                sb.Append($"Duration: {(EndTime - StartTime):0.000} sec");
                
                if (startFrame.HasValue && endFrame.HasValue)
                {
                    int frameCount = endFrame.Value - startFrame.Value;
                    if (frameCount == 1)
                        sb.AppendLine($" (1 frame)");
                    else
                        sb.AppendLine($" ({frameCount} frames)");
                }
                else
                {
                    sb.AppendLine();
                }
                
                TooltipDisplayText = sb.ToString();
                NeedsTextRefresh = false;
                NeedsTextRefresh_LoadParams = false;
            }

            public void NewSaveParamsToBytes()
            {
                if (Parameters != null)
                    ParameterBytes = Parameters.AsBytes(writeAssertValueInsteadOfCurrentValue: false);
                //UpdateGraphDisplayText();
                NeedsTextRefresh = true;
            }
            public void NewLoadParamsFromBytes(bool lenientOnAssert = false, Template.ActionTemplate actTemplate = null)
            {
                Template = actTemplate ?? Template;
                Parameters = new ParameterContainer(ParameterBytes, actTemplate ?? Template, suppressAssert: lenientOnAssert || !Main.Debug.EnableTaeTemplateAssert);
                //UpdateGraphDisplayText(loadParams: false);
                NeedsTextRefresh = true;
                LastTemplateGUID = Template.GUID;
            }

            public void CheckRefreshGUID()
            {
                if (Template != null && LastTemplateGUID != Template?.GUID)
                {
                    NewLoadParamsFromBytes();
                }
            }

        }


    }
}
