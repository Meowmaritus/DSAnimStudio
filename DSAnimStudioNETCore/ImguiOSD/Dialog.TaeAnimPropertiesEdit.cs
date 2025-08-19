using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Dialog
    {
        public class TaeAnimPropertiesEdit : Dialog
        {
            string GetAnimIDString(long id)
            {
                if (id < 0)
                    return "";

                var animID_Lower = zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeHasLongAnimIDs
                ? (id % 1_000000) : (id % 1_0000);

                var animID_Upper = zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeHasLongAnimIDs
                    ? (id / 1_000000) : (id / 1_0000);

                string curIDText;
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeHasLongAnimIDs)
                {
                    bool ds2Meme = zzz_DocumentManager.CurrentDocument.GameRoot.CurrentAnimIDFormatType == zzz_GameRootIns.AnimIDFormattingType.aXX_YY_ZZZZ;
                    if (ds2Meme)
                    {
                        curIDText = $"a{(animID_Upper):D2}_{animID_Lower:D6}";
                        curIDText = curIDText.Insert(curIDText.Length - 4, "_");
                    }
                    else
                    {
                        curIDText = $"a{(animID_Upper):D3}_{animID_Lower:D6}";
                    }
                }
                else
                {
                    curIDText = $"a{(animID_Upper):D2}_{animID_Lower:D4}";
                }

                return curIDText;
            }

            public DSAProj Proj;

            public string TaeAnimID_String;
            public string TaeAnimID_Error;
            public SplitAnimID? TaeAnimID_Value;

            public Color? TaeAnimCustomColor;

            public string ImportFromAnimID_String;
            public string ImportFromAnimID_Error;
            public int? ImportFromAnimID_Value;

            public string OrigTaeAnimName;
            public string NewTaeAnimName;

            public DSAProj.EditorInfo OrigEditInfo;
            public DSAProj.EditorInfo NewEditInfo;

            public TAE.Animation.AnimFileHeader TaeAnimHeader;
            public bool WasAnimDeleted = false;

            public DSAProj.Animation OriginalAnimation;

            public bool HasAnyUnsavedChanges = false;



            public bool CheckForAnyUnsavedChanges()
            {
                var headerClone = OriginalAnimation.SAFE_GetHeaderClone();

                if (headerClone is TAE.Animation.AnimFileHeader.ImportOtherAnim origAsImportOtherAnim &&
                    TaeAnimHeader is TAE.Animation.AnimFileHeader.ImportOtherAnim newAsImportOtherAnim)
                {
                    if (origAsImportOtherAnim.ImportFromAnimID != newAsImportOtherAnim.ImportFromAnimID
                        || origAsImportOtherAnim.Unknown != newAsImportOtherAnim.Unknown)
                        return true;
                }
                else if (headerClone is TAE.Animation.AnimFileHeader.Standard origAsStandard &&
                    TaeAnimHeader is TAE.Animation.AnimFileHeader.Standard newAsStandard)
                {
                    if (origAsStandard.ImportsHKX != newAsStandard.ImportsHKX
                        || origAsStandard.IsLoopByDefault != newAsStandard.IsLoopByDefault
                        || origAsStandard.AllowDelayLoad != newAsStandard.AllowDelayLoad
                        || origAsStandard.ImportHKXSourceAnimID != newAsStandard.ImportHKXSourceAnimID)
                        return true;
                }
                else
                {
                    return true;
                }

                if (OriginalAnimation.SplitID != TaeAnimID_Value 
                    || !OrigEditInfo.SerializeToBytes(Proj).SequenceEqual(NewEditInfo.SerializeToBytes(Proj))
                    || NewTaeAnimName != OrigTaeAnimName)
                    return true;

                return false;
            }

            public TaeAnimPropertiesEdit(DSAProj proj, DSAProj.Animation anim)
                : base("Edit Animation Properties")
            {
                Proj = proj;
                AllowedResultTypes = ResultTypes.Accept | ResultTypes.Cancel;
                InputFlags = InputFlag.EscapeKeyToCancel | InputFlag.TitleBarXToCancel;
                CancelHandledByInheritor = true;
                AcceptHandledByInheritor = true;

                TaeAnimID_String = anim.SplitID.GetFormattedIDString(proj);
                TaeAnimID_Value = anim.SplitID;
                OriginalAnimation = anim;

                anim.SafeAccessHeader(header =>
                {
                    OrigTaeAnimName = header.AnimFileName;
                    NewTaeAnimName = OrigTaeAnimName;

                    var animEditInfoBytes = anim.Info.SerializeToBytes(proj);
                    OrigEditInfo = new DSAProj.EditorInfo();
                    OrigEditInfo.DeserializeFromBytes(animEditInfoBytes, proj.Template, proj);
                    NewEditInfo = new DSAProj.EditorInfo();
                    NewEditInfo.DeserializeFromBytes(animEditInfoBytes, proj.Template, proj);
                    if (header is TAE.Animation.AnimFileHeader.ImportOtherAnim asImportOtherAnim)
                    {
                        TaeAnimHeader = new TAE.Animation.AnimFileHeader.ImportOtherAnim()
                        {
                            ImportFromAnimID = asImportOtherAnim.ImportFromAnimID,
                            Unknown = asImportOtherAnim.Unknown,
                        };
                        ImportFromAnimID_String = GetAnimIDString(asImportOtherAnim.ImportFromAnimID);
                        ImportFromAnimID_Value = asImportOtherAnim.ImportFromAnimID;
                    }
                    else if (header is TAE.Animation.AnimFileHeader.Standard asStandard)
                    {
                        TaeAnimHeader = new TAE.Animation.AnimFileHeader.Standard()
                        {
                            AllowDelayLoad = asStandard.AllowDelayLoad,
                            ImportHKXSourceAnimID = asStandard.ImportHKXSourceAnimID,
                            ImportsHKX = asStandard.ImportsHKX,
                            IsLoopByDefault = asStandard.IsLoopByDefault,
                        };
                        ImportFromAnimID_String = GetAnimIDString(asStandard.ImportHKXSourceAnimID);
                        ImportFromAnimID_Value = asStandard.ImportHKXSourceAnimID;
                    }

                    //TaeAnimCustomColor = anim.Info.CustomColor;
                });

               
            }

            protected override void BuildInsideOfWindow()
            {
                if (TaeAnimID_Value != null)
                    TaeAnimID_Error = null;

                if (ImportFromAnimID_Value != null)
                    ImportFromAnimID_Error = null;

                bool isCurrentlyStandard = TaeAnimHeader.Type == TAE.Animation.AnimFileHeaderType.Standard;
                bool isCurrentlyImportOther = TaeAnimHeader.Type == TAE.Animation.AnimFileHeaderType.ImportOtherAnim;

                bool anyValueAdjusted = false;

                if (TaeAnimID_Value == null)
                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0, 0, 1));

                anyValueAdjusted |= ImGui.InputText("Animation ID", ref TaeAnimID_String, 256); 

                if (ImGui.IsItemHovered() && TaeAnimID_Error != null)
                    ImGui.SetTooltip(TaeAnimID_Error);
                if (TaeAnimID_Value == null)
                    ImGui.PopStyleColor();
                if (string.IsNullOrWhiteSpace(TaeAnimID_String))
                {
                    TaeAnimID_Value = null;
                    TaeAnimID_Error = "Animation entry ID must be specified.";
                }
                else
                {
                    
                    bool parseSuccess = SplitAnimID.TryParse(Proj.ParentDocument.GameRoot, TaeAnimID_String, out SplitAnimID animIdParsed,
                        out string detailedError);

                    if (parseSuccess)
                    {
                        TaeAnimID_Error = null;
                        TaeAnimID_Value = animIdParsed;
                    }
                    else
                    {
                        TaeAnimID_Error = detailedError;
                        TaeAnimID_Value = null;
                    }
                    
                 
                    
                }

                var newAnimName = NewTaeAnimName ?? "%null%";

                anyValueAdjusted |= ImGui.InputText("Anim File Name", ref newAnimName, 256);

                if (newAnimName == "%null%")
                    NewTaeAnimName = null;
                else
                    NewTaeAnimName = newAnimName;

                ImGui.Separator();

                
                bool editInfoAnyFieldFocused = false;
                NewEditInfo.ShowImGui(ref editInfoAnyFieldFocused, Proj,
                    GetRect(subtractScrollBar: true).DpiScaled(), 
                    GetRect(subtractScrollBar: false).DpiScaled(), modified =>
                {
                    
                }, showTagsOnly: false, out bool anyInfoUpdated);

                if (anyInfoUpdated)
                    anyValueAdjusted = true;

                //Tools.CustomColorPicker("Custom Display Color", $"{OriginalAnimation.GUID}__PropetiesEditor__CustomColor", ref TaeAnimCustomColor);

                ImGui.Separator();

                ImGui.Text("Animation Type:");

                ImGui.Indent();
                {
                    ImGui.MenuItem("Original Anim Entry", "", isCurrentlyStandard);
                    bool clickedStandard = ImGui.IsItemClicked();

                    ImGui.MenuItem("Clone Anim Entry", "", TaeAnimHeader.Type == TAE.Animation.AnimFileHeaderType.ImportOtherAnim);
                    bool clickedImportOther = ImGui.IsItemClicked();

                    if (clickedStandard && !isCurrentlyStandard)
                    {
                        DialogManager.AskYesNo("Change Animation Type", "Change animation type, losing the values you entered?",
                            choice =>
                            {
                                if (choice)
                                {
                                    TaeAnimHeader = new TAE.Animation.AnimFileHeader.Standard();
                                    HasAnyUnsavedChanges = true;
                                }
                            });
                    }
                    else if (clickedImportOther && !isCurrentlyImportOther)
                    {
                        DialogManager.AskYesNo("Change Animation Type", "Change animation type, losing the values you entered?",
                            choice =>
                            {
                                if (choice)
                                {
                                    TaeAnimHeader = new TAE.Animation.AnimFileHeader.ImportOtherAnim();
                                    HasAnyUnsavedChanges = true;
                                }
                            });
                    }
                }
                ImGui.Unindent();


                bool isNull = TaeAnimHeader.IsNullHeader;
                anyValueAdjusted |= ImGui.Checkbox("Is Null Header", ref isNull);
                if (isNull && !TaeAnimHeader.IsNullHeader)
                { 
                    DialogManager.AskYesNo("Wipe Header To Null", "Change animation header to null? Properties below the checkbox will be erased.",
                        choice =>
                        {
                            if (choice)
                            {
                                TaeAnimHeader.IsNullHeader = true;
                                HasAnyUnsavedChanges = true;
                            }
                        });
                }
                else if (!isNull && TaeAnimHeader.IsNullHeader)
                {
                    TaeAnimHeader.IsNullHeader = false;
                }

                if (!TaeAnimHeader.IsNullHeader)
                {


                    if (TaeAnimHeader is TAE.Animation.AnimFileHeader.ImportOtherAnim asImportOtherAnim)
                    {
                        ImGui.Text("Properties - Clone Anim Entry:");
                        ImGui.Indent();
                        {
                            //Tools.InputTextNullable("Animation Name", ref TaeAnimName, 256);
                            //ImGui.Separator();
                            if (ImportFromAnimID_Value == null)
                                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0, 0, 1));
                            anyValueAdjusted |= ImGui.InputText("Clone Anim Entry ID", ref ImportFromAnimID_String, 256);
                            if (ImGui.IsItemHovered() && ImportFromAnimID_Error != null)
                                ImGui.SetTooltip(ImportFromAnimID_Error);
                            if (ImportFromAnimID_Value == null)
                                ImGui.PopStyleColor();
                            if (string.IsNullOrWhiteSpace(ImportFromAnimID_String))
                            {
                                ImportFromAnimID_Value = asImportOtherAnim.ImportFromAnimID = -1;
                            }
                            else if (int.TryParse(ImportFromAnimID_String.Replace("a", "").Replace("A", "").Replace("_", ""), out int importFromIdParsed))
                            {
                                ImportFromAnimID_Value = asImportOtherAnim.ImportFromAnimID = importFromIdParsed;
                            }
                            else
                            {
                                ImportFromAnimID_Value = null;
                                if (zzz_DocumentManager.CurrentDocument.GameRoot.CurrentAnimIDFormatType == zzz_GameRootIns.AnimIDFormattingType.aXXX_YYYYYY)
                                    ImportFromAnimID_Error = "Invalid ID specified. Leave the box blank to specify no animation or enter an ID in either 'aXXX_YYYYYY' format or 'XXXYYYYYY' format.";
                                else if (zzz_DocumentManager.CurrentDocument.GameRoot.CurrentAnimIDFormatType == zzz_GameRootIns.AnimIDFormattingType.aXX_YY_ZZZZ)
                                    ImportFromAnimID_Error = "Invalid ID specified. Leave the box blank to specify no animation or enter an ID in either 'aXX_YY_ZZZZ' format or 'XXYYZZZZ' format.";
                                else if (zzz_DocumentManager.CurrentDocument.GameRoot.CurrentAnimIDFormatType == zzz_GameRootIns.AnimIDFormattingType.aXX_YYYY)
                                    ImportFromAnimID_Error = "Invalid ID specified. Leave the box blank to specify no animation or enter an ID in either 'aXX_YYYY' format or 'XXYYYY' format.";
                                else
                                    throw new NotImplementedException();
                            }

                            int prevUnknownVal = asImportOtherAnim.Unknown;
                            asImportOtherAnim.Unknown = MenuBar.IntItem("Unknown Value", asImportOtherAnim.Unknown);
                            if (prevUnknownVal != asImportOtherAnim.Unknown)
                                anyValueAdjusted = true;
                        }
                        ImGui.Unindent();
                    }
                    else if (TaeAnimHeader is TAE.Animation.AnimFileHeader.Standard asStandard)
                    {
                        ImGui.Text("Properties - Original Anim Entry:");
                        ImGui.Indent();
                        {
                            //Tools.InputTextNullable("Animation Name", ref TaeAnimName, 256);
                            //ImGui.Separator();
                            bool prevImportsHKX = asStandard.ImportsHKX;
                            asStandard.ImportsHKX = MenuBar.CheckboxBig("Override HKX", asStandard.ImportsHKX);
                            if (prevImportsHKX != asStandard.ImportsHKX)
                                anyValueAdjusted = true;

                            if (ImportFromAnimID_Value == null)
                                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0, 0, 1));

                            anyValueAdjusted |= ImGui.InputText("Override HKX ID", ref ImportFromAnimID_String, 256);

                            if (ImGui.IsItemHovered() && ImportFromAnimID_Error != null)
                                ImGui.SetTooltip(ImportFromAnimID_Error);
                            if (ImportFromAnimID_Value == null)
                                ImGui.PopStyleColor();
                            if (string.IsNullOrWhiteSpace(ImportFromAnimID_String))
                            {
                                ImportFromAnimID_Value = asStandard.ImportHKXSourceAnimID = -1;
                            }
                            else if (int.TryParse(ImportFromAnimID_String.Replace("a", "").Replace("A", "").Replace("_", ""), out int importFromIdParsed))
                            {
                                ImportFromAnimID_Value = asStandard.ImportHKXSourceAnimID = importFromIdParsed;
                            }
                            else
                            {
                                ImportFromAnimID_Value = null;
                                if (zzz_DocumentManager.CurrentDocument.GameRoot.CurrentAnimIDFormatType == zzz_GameRootIns.AnimIDFormattingType.aXXX_YYYYYY)
                                    ImportFromAnimID_Error = "Invalid ID specified. Leave the box blank to specify no animation or enter an ID in either 'aXXX_YYYYYY' format or 'XXXYYYYYY' format.";
                                else if (zzz_DocumentManager.CurrentDocument.GameRoot.CurrentAnimIDFormatType == zzz_GameRootIns.AnimIDFormattingType.aXX_YY_ZZZZ)
                                    ImportFromAnimID_Error = "Invalid ID specified. Leave the box blank to specify no animation or enter an ID in either 'aXX_YY_ZZZZ' format or 'XXYYZZZZ' format.";
                                else if (zzz_DocumentManager.CurrentDocument.GameRoot.CurrentAnimIDFormatType == zzz_GameRootIns.AnimIDFormattingType.aXX_YYYY)
                                    ImportFromAnimID_Error = "Invalid ID specified. Leave the box blank to specify no animation or enter an ID in either 'aXX_YYYY' format or 'XXYYYY' format.";
                                else
                                    throw new NotImplementedException();
                            }
                            bool prevAllowDelayLoad = asStandard.AllowDelayLoad;
                            bool prevIsLoopByDefault = asStandard.IsLoopByDefault;
                            asStandard.AllowDelayLoad = MenuBar.CheckboxBig("Allow loading from DelayLoad ANIBNDs", asStandard.AllowDelayLoad);
                            asStandard.IsLoopByDefault = MenuBar.CheckboxBig("Loop By Default", asStandard.IsLoopByDefault);
                            if (asStandard.AllowDelayLoad != prevAllowDelayLoad || asStandard.IsLoopByDefault != prevIsLoopByDefault)
                                anyValueAdjusted = true;
                        }
                        ImGui.Unindent();
                    }
                }

                ImGui.Separator();
                ImGui.Button("Delete This Animation...");
                if (ImGui.IsItemClicked())
                {
                    DialogManager.AskYesNo("Permanently Delete Animation Entry?", $"Are you sure you want to delete the current animation?",
                        choice =>
                        {
                            if (choice)
                            {
                                WasAnimDeleted = true;
                                ResultType = ResultTypes.Accept;
                                Dismiss();
                            }
                            
                        });
                    
                }
                ImGui.Separator();

                bool clickedCancel = Tools.SimpleClickButton("Cancel") || IsTitleBarXRequested;
                //bool pressedEscape = IsEscapeKeyRequested;

                if (anyValueAdjusted && !HasAnyUnsavedChanges)
                {
                    HasAnyUnsavedChanges = CheckForAnyUnsavedChanges();
                    //HasAnyUnsavedChanges = true;
                }

                if (clickedCancel || IsEscapeKeyRequested)
                {
                    //bool unsavedChanges = CheckForAnyUnsavedChanges();
                    bool forceClose = Main.Input.ShiftHeld;
                    if (!HasAnyUnsavedChanges || forceClose)
                    {
                        ResultType = ResultTypes.Cancel;
                        Dismiss();
                    }
                    else
                    {
                        DialogManager.AskYesNo("Unsaved Changes", "You have unsaved changes, would you like to discard them and close this dialog?", choice =>
                        {
                            if (choice == true)
                            {
                                ResultType = ResultTypes.Cancel;
                                Dismiss();
                            }
                        });
                    }
                }

                bool isInvalidState = (ImportFromAnimID_Value == null || TaeAnimID_Value == null);

                bool grayedOutAcceptButton = isInvalidState || !HasAnyUnsavedChanges;

                if (grayedOutAcceptButton)
                    Tools.PushGrayedOut();
                ImGui.Button("Save & Accept");
                if (grayedOutAcceptButton)
                {
                    Tools.PopGrayedOut();
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Cannot accept changes until the animation ID formatting errors (shown in red) are fixed.");
                    }
                }
                if (ImGui.IsItemClicked())
                {
                    if (!grayedOutAcceptButton)
                    {
                        ResultType = ResultTypes.Accept;
                        Dismiss();
                    }
                   
                }

                //throw new NotImplementedException();
            }
        }
    }
}
