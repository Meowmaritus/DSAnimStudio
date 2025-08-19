using DSAnimStudio.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using static DSAnimStudio.TaeEditor.OLD_TaeEditAnimEventGraph;
using System.Diagnostics;
using SharpDX.DirectWrite;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using DSAnimStudio.ImguiOSD;
using SoulsAssetPipeline;
using static SoulsAssetPipeline.Audio.Wwise.WwiseEnums;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using System.Reflection.Metadata;
using static DSAnimStudio.ImguiOSD.Dialog;
using System.Runtime;
//using static DSAnimStudio.ImguiOSD.Window;

namespace DSAnimStudio.TaeEditor
{
    public class TaeEditorScreen : IDisposable
    {
        private bool _disposed = false;
        public void Dispose()
        {
            if (!_disposed)
            {
                Graph?.Dispose();
                _disposed = true;
            }
        }

        public static int GhettoInputCooldown = 0;

        public zzz_DocumentIns ParentDocument;

        //private static System.Diagnostics.Stopwatch debugTimerInputChatter = null;

        private object _lock_highlights = new object();
        private Dictionary<object, float> highlightTimers = new();
        private Dictionary<object, float> highlightOpacity = new();

        public Dictionary<object, float> HighlightOpacityDictCopy = new();

        private float highlightFadeIn = 0.15f;
        private float highlightSustain = 0.8f;
        private float highlightFadeOut = 0.4f;

        public void UpdateAllHighlightTimers(float deltaTime)
        {
            lock (_lock_highlights)
            {
                var keysCopy = highlightTimers.Keys.ToList();
                HighlightOpacityDictCopy.Clear();
                foreach (var key in keysCopy)
                {
                    highlightTimers[key] += deltaTime;
                    var h = highlightTimers[key];
                    if (h < highlightFadeIn)
                    {
                        highlightOpacity[key] = Utils.MapRange(h, 0, highlightFadeIn, 0, 1);
                        HighlightOpacityDictCopy.Add(key,  highlightOpacity[key]);
                    }
                    else if (h < (highlightFadeIn + highlightSustain))
                    {
                        highlightOpacity[key] = 1;
                        HighlightOpacityDictCopy.Add(key,  highlightOpacity[key]);
                    }
                    else if (h < (highlightFadeIn + highlightSustain + highlightFadeOut))
                    {
                        highlightOpacity[key] = Utils.MapRange(h, highlightFadeIn + highlightSustain, highlightFadeIn + highlightSustain + highlightFadeOut, 1, 0);
                        HighlightOpacityDictCopy.Add(key,  highlightOpacity[key]);
                    }
                    else
                    {
                        if (highlightTimers.ContainsKey(key))
                            highlightTimers.Remove(key);
                        if (highlightOpacity.ContainsKey(key))
                            highlightOpacity.Remove(key);
                    }
                }
            }
        }
        
        public void RequestHighlightAnimCategory(DSAProj.AnimCategory category)
        {
            lock (_lock_highlights)
            {
                highlightTimers[category] = 0;
                highlightOpacity[category] = 0;
            }
        }

        public void RequestHighlightAnimation(DSAProj.Animation anim)
        {
            lock (_lock_highlights)
            {
                highlightTimers[anim] = 0;
                highlightOpacity[anim] = 0;
            }
        }
        
        public void RequestHighlightAction(DSAProj.Action act)
        {
            lock (_lock_highlights)
            {
                highlightTimers[act] = 0;
                highlightOpacity[act] = 0;
            }
        }

        public void RequestHighlightTrack(DSAProj.ActionTrack track)
        {
            lock (_lock_highlights)
            {
                highlightTimers[track] = 0;
                highlightOpacity[track] = 0;
            }
        }
        
        
        
        private bool HardResetQueued = false;
        private bool HardResetQueued_StartPlaying = false;

        public class AnimViewHistoryEntry
        {
            public DSAProj.AnimCategory Tae;
            public DSAProj.Animation Anim;
            // Maybe something else :fatcat:
        }

        private Stack<AnimViewHistoryEntry> AnimViewBackwardStack = new Stack<AnimViewHistoryEntry>();
        private Stack<AnimViewHistoryEntry> AnimViewForwardStack = new Stack<AnimViewHistoryEntry>();

        public void ImguiDebugAddAnimViewBackwardStackItems()
        {
            foreach (var x in AnimViewBackwardStack)
            {
                ImGuiNET.ImGui.Text(x.Anim.SplitID.GetFormattedIDString(Proj));
            }
        }

        private AnimViewHistoryEntry GetAnimHistoryEntryFromCurrent()
        {
            return new AnimViewHistoryEntry()
            {
                Anim = SelectedAnim,
                Tae = SelectedAnimCategory,
            };
        }

        private bool AnimHistoryCanGoBack()
        {
            return AnimViewBackwardStack.Count > 0;
        }

        private bool AnimHistoryCanGoForward()
        {
            return AnimViewForwardStack.Count > 0;
        }

        private void AnimHistoryGoBack()
        {
            if (AnimViewBackwardStack.Count > 0)
            {
                AnimViewForwardStack.Push(GetAnimHistoryEntryFromCurrent());
                var next = AnimViewBackwardStack.Pop();
                SelectNewAnimRef(next.Tae, next.Anim, scrollOnCenter: true, isPushCurrentToHistBackwardStack: false);
            }
        }

        private void AnimHistoryGoForward()
        {
            if (AnimViewForwardStack.Count > 0)
            {
                AnimViewBackwardStack.Push(GetAnimHistoryEntryFromCurrent());
                var next = AnimViewForwardStack.Pop();
                SelectNewAnimRef(next.Tae, next.Anim, scrollOnCenter: true, isPushCurrentToHistBackwardStack: false);
            }
        }

        public string GetActionBoxText(DSAProj.Action act)
        {
            //throw new NotImplementedException();
            act.UpdateGraphDisplayText();
            return act.GraphDisplayText;
            //TODO: Read this shit from new project system.
        }


        public void FixActionSelectionAfterUndoRedo()
        {
            List<DSAProj.Action> invalidActions = new();
            Graph.AnimRef.SafeAccessActions(actions =>
            {
                invalidActions = NewSelectedActions.Where(x => !actions.Contains(x)).ToList();
            });
            foreach (var act in invalidActions)
                NewSelectedActions.Remove(act);
        }

        public void FixAnimSelectionAfterUndoRedo()
        {
            while (SelectedAnimCategory != null && !Proj.SAFE_CategoryExists(SelectedAnimCategory))
            {
                NextAnim(false, true);
            }

            while (SelectedAnim != null && Proj.SAFE_GetFirstAnimationFromFullID(SelectedAnim.SplitID) == null)
            {
                NextAnim(false, false);
            }
        }


        public bool RequestGoToEventSource = false;
        public bool REMO_HOTFIX_REQUEST_PLAY_RESUME_NEXT_FRAME = false;
        public bool REMO_HOTFIX_REQUEST_PLAY_RESUME_THIS_FRAME { get; private set; } = false;

        public bool REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME = false;

        public DSAProj.AnimCategory REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE = null;
        public DSAProj.Animation REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE_ANIM = null;
        public bool REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_PREV = false;
        public bool REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_SHIFT = false;
        public bool REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_CTRL = false;

        public bool REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME { get; private set; } = false;

        public const string BackupExtension = ".dsasbak";

        private ContentManager DebugReloadContentManager = null;

        public void Tools_ScanForUnusedAnimations()
        {
            List<SplitAnimID> usedAnims = new List<SplitAnimID>();
            List<SplitAnimID> unusedAnims = new List<SplitAnimID>();
            SelectedAnimCategory.SafeAccessAnimations(anims =>
            {
                foreach (var anim in anims)
                {
                    SplitAnimID hkx = anim.GetHkxID(Proj);
                    if (!usedAnims.Contains(hkx))
                        usedAnims.Add(hkx);
                }
            });
            
            foreach (var anim in Graph.ViewportInteractor.CurrentModel.AnimContainer.Animations.Keys)
            {
                if (!usedAnims.Contains(anim)
                && !unusedAnims.Contains(anim))
                    unusedAnims.Add(anim);
            }
            //var sb = new StringBuilder();
            foreach (var anim in unusedAnims)
            {
                //sb.AppendLine(anim);
                var newAnim = new DSAProj.Animation(Proj, SelectedAnimCategory, SplitAnimID.FromFullID(Proj, 9_000_000000 + anim.GetFullID(Proj)), new TAE.Animation.AnimFileHeader.Standard()
                {
                    ImportHKXSourceAnimID = (int)(anim.GetFullID(Proj)),
                    ImportsHKX = true,
                    AnimFileName = $"UNUSED:{anim}",
                });
                SelectedAnimCategory.SAFE_AddAnimation(newAnim);
            }
            RecreateAnimList();
        }

        // public void Tools_ExportCurrentTAE()
        // {
        //     Main.WinForm.Invoke(new Action(() =>
        //     {
        //         var browseDlg = new System.Windows.Forms.SaveFileDialog()
        //         {
        //             Filter = "TAE Files (*.tae)|*.tae",
        //             ValidateNames = true,
        //             CheckPathExists = true,
        //             //ShowReadOnly = true,
        //             Title = "Choose where to save loose TAE file.",
        //
        //         };
        //
        //         var decision = browseDlg.ShowDialog();
        //
        //         if (decision == System.Windows.Forms.DialogResult.OK)
        //         {
        //             try
        //             {
        //                 var binaryData = SelectedAnimCategory.ToBinary();
        //                 File.WriteAllBytes(browseDlg.FileName, binaryData);
        //                 System.Windows.Forms.MessageBox.Show("TAE saved successfully.", "Saved");
        //             }
        //             catch (Exception exc)
        //             {
        //                 System.Windows.Forms.MessageBox.Show($"Error saving TAE file:\n\n{exc}", "Failed to Save",
        //                     System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        //             }
        //         }
        //
        //     }));
        // }

        public void BringUpImporter_FLVER2()
        {
            if (ImporterWindow_FLVER2 == null || ImporterWindow_FLVER2.IsDisposed || !ImporterWindow_FLVER2.Visible)
            {
                ImporterWindow_FLVER2?.Dispose();
                ImporterWindow_FLVER2 = null;
            }
            ImporterWindow_FLVER2 = new SapImportFlver2Form();
            ImporterWindow_FLVER2.ImportConfig = Config.LastUsedImportConfig_FLVER2;
            ImporterWindow_FLVER2.MainScreen = this;
            ImporterWindow_FLVER2.Show();
            // The CenterParent stuff just doesn't work for some reason, heck it.

            Main.CenterForm(ImporterWindow_FLVER2);

            ImporterWindow_FLVER2.LoadValuesFromConfig();
        }

        public void BringUpImporter_FBXAnim()
        {
            if (ImporterWindow_FBXAnim == null || ImporterWindow_FBXAnim.IsDisposed || !ImporterWindow_FBXAnim.Visible)
            {
                ImporterWindow_FBXAnim?.Dispose();
                ImporterWindow_FBXAnim = null;
            }
            ImporterWindow_FBXAnim = new SapImportFbxAnimForm();
            ImporterWindow_FBXAnim.ImportConfig = Config.LastUsedImportConfig_AnimFBX;
            ImporterWindow_FBXAnim.MainScreen = this;
            ImporterWindow_FBXAnim.Show();
            // The CenterParent stuff just doesn't work for some reason, heck it.

            Main.CenterForm(ImporterWindow_FBXAnim);

            ImporterWindow_FBXAnim.LoadValuesFromConfig();
        }

        public void ShowExportAllAnimsMenu()
        {
            GameWindowAsForm.Invoke(new Action(() =>
            {
                if (!IsFileOpen)
                    return;

                if (ExportAllAnimsMenu == null || ExportAllAnimsMenu.IsDisposed)
                {
                    ExportAllAnimsMenu = new TaeExportAllAnimsForm();
                    ExportAllAnimsMenu.Owner = GameWindowAsForm;
                    ExportAllAnimsMenu.MainScreen = this;
                    ExportAllAnimsMenu.ShownInitValues();
                }

                ExportAllAnimsMenu.Show();
                Main.CenterForm(ExportAllAnimsMenu);
                ExportAllAnimsMenu.Activate();
            }));
        }

        public void ImmediateExportAllEventsToTextFile(string textFilePath)
        {
            var sb = new StringBuilder();

            FileContainer.Proj.SafeAccessAnimCategoriesList(categoriesList =>
            {
                foreach (var category in categoriesList)
                {
                    category.UnSafeAccessAnimations(animations =>
                    {
                        foreach (var anim in animations)
                        {
                            anim.UnSafeAccessHeader(header =>
                            {
                                string animName = anim.SplitID.ToString();
                                if (!string.IsNullOrWhiteSpace(header.AnimFileName))
                                    animName += " " + header.AnimFileName;
                                sb.AppendLine("--------------------------------------------------------------------------------");
                                sb.AppendLine(animName);
                                sb.AppendLine("--------------------------------------------------------------------------------");
                                if (header is TAE.Animation.AnimFileHeader.ImportOtherAnim asImportOtherAnim && asImportOtherAnim.ImportFromAnimID >= 0)
                                {
                                    sb.AppendLine($"  Imports all events from {SplitAnimID.FromFullID(Proj, asImportOtherAnim.ImportFromAnimID)}.");
                                }
                                else
                                {
                                    var actionList = anim.INNER_GetActions().OrderBy(ev => ev.Type);
                                    List<string> frameRangeTexts = new List<string>();
                                    List<string> eventInfoTexts = new List<string>();
                                    foreach (var ev in actionList)
                                    {
                                        int startFrame = (int)Math.Floor(ev.StartTime / Main.TAE_FRAME_30);
                                        int endFrame = (int)Math.Floor(ev.EndTime / Main.TAE_FRAME_30);
                                        string eventText = GetActionBoxText(ev);
                                        frameRangeTexts.Add($"  {startFrame}-{(ev.EndTime >= TAE.Action.EldenRingInfiniteLengthEventPlaceholder ? "M" : endFrame.ToString())}");
                                        eventInfoTexts.Add($"{eventText}");
                                    }
                                    if (frameRangeTexts.Count > 0)
                                    {
                                        int maxFrameRangeLength = frameRangeTexts.Max(x => x.Length);
                                        for (int i = 0; i < frameRangeTexts.Count; i++)
                                        {
                                            sb.AppendLine($"{(frameRangeTexts[i] + new string(' ', (maxFrameRangeLength) - frameRangeTexts[i].Length))} {eventInfoTexts[i]}");
                                        }
                                    }
                                    else
                                    {
                                        sb.AppendLine($"  Animation contains no events.");
                                    }
                                }
                            });


                            sb.AppendLine("");
                        }
                    });
                    
                }
            });

            

            File.WriteAllText(textFilePath, sb.ToString());
            zzz_NotificationManagerIns.PushNotification("Exported text file successfully.");
        }

        public void ImmediateExportAllAnimNamesToTextFile(string textFilePath)
        {
            var sb = new StringBuilder();

            FileContainer.Proj.SafeAccessAnimCategoriesList(categoriesList =>
            {
                foreach (var category in categoriesList)
                {
                    category.UnSafeAccessAnimations(anims =>
                    {
                        foreach (var anim in anims)
                        {
                            anim.UnSafeAccessHeader(header =>
                            {
                                string animIDString = anim.SplitID.ToString();
                                string animNameString = !string.IsNullOrWhiteSpace(header.AnimFileName) ? header.AnimFileName : "";
                                sb.AppendLine($"{animIDString}={animNameString}");
                            });
                            
                        }
                    });
                    
                    sb.AppendLine("");
                }
            });

            

            File.WriteAllText(textFilePath, sb.ToString());
            zzz_NotificationManagerIns.PushNotification("Exported text file successfully.");
        }

        public void ShowExportAllAnimNamesDialog()
        {
            var browseDlg = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = "Text Files (*.TXT)|*.TXT",
                ValidateNames = true,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            if (System.IO.File.Exists(NewFileContainerName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(NewFileContainerName);
                browseDlg.FileName = System.IO.Path.GetFileName(NewFileContainerName + ".AnimNames.txt");
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImmediateExportAllAnimNamesToTextFile(browseDlg.FileName);
            }
        }

        public void ImmediateImportAllAnimNamesFromTextFile(string textFilePath)
        {
            Dictionary<string, string> animNameDict = new Dictionary<string, string>();
            var txtFileLines = File.ReadAllLines(textFilePath);
            for (int i = 0; i < txtFileLines.Length; i++)
            {
                var t = txtFileLines[i].Trim();
                if (!string.IsNullOrWhiteSpace(t))
                {
                    var equalsIndex = t.IndexOf('=');
                    if (equalsIndex != -1)
                    {
                        var beforeEquals = t.Substring(0, equalsIndex).Trim().ToLower();
                        var afterEquals = equalsIndex < (t.Length - 1) ? t.Substring(equalsIndex + 1).Trim() : "";
                        if (!animNameDict.ContainsKey(beforeEquals))
                            animNameDict.Add(beforeEquals, afterEquals);
                        else
                            animNameDict[beforeEquals] = afterEquals;
                    }
                }
            }

            int numNamesEdited = 0;

            FileContainer.Proj.SafeAccessAnimCategoriesList(categoriesList =>
            {
                foreach (var category in categoriesList)
                {
                    category.UnSafeAccessAnimations(anims =>
                    {
                        foreach (var anim in anims)
                        {
                            string animIDString = anim.SplitID.ToString();
                            if (animNameDict.ContainsKey(animIDString))
                            {

                                anim.UnSafeAccessHeader(header =>
                                {
                                    var check = header.AnimFileName;
                                    if (string.IsNullOrWhiteSpace(check))
                                        check = "";
                                    if (check != animNameDict[animIDString])
                                    {
                                        header.AnimFileName = animNameDict[animIDString];
                                        anim.INNER_SetIsModified(true);
                                        category.INNER_SetIsModified(true);
                                        numNamesEdited++;
                                    }
                                });

                                
                            }
                        }
                    });
                    
                }
            });



            zzz_NotificationManagerIns.PushNotification($"Successfully imported animation names from text file." +
                $"\n{numNamesEdited} animation names were modified by the import process.");
        }

        public void ShowImportAllAnimNamesDialog()
        {
            var browseDlg = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "Text Files (*.TXT)|*.TXT",
                ValidateNames = true,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            string fileContainerName = FileContainer.Info.GetMainBinderName();

            if (System.IO.File.Exists(fileContainerName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(fileContainerName);
                browseDlg.FileName = System.IO.Path.GetFileName(fileContainerName + ".AnimNames.txt");
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImmediateImportAllAnimNamesFromTextFile(browseDlg.FileName);
            }
        }

        public void ShowExportAllActionsToTextFileDialog()
        {
            var browseDlg = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = "Text Files (*.TXT)|*.TXT",
                ValidateNames = true,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            if (System.IO.File.Exists(NewFileContainerName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(NewFileContainerName);
                browseDlg.FileName = System.IO.Path.GetFileName(NewFileContainerName + ".txt");
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImmediateExportAllEventsToTextFile(browseDlg.FileName);
            }
        }
        
        public TaeExportAllAnimsForm ExportAllAnimsMenu = null;

        public int AnimSwitchRenderCooldown = 0;
        public int AnimSwitchRenderCooldownMax = 2;
        public float AnimSwitchRenderCooldownFadeLength = 0.1f;
        public Color AnimSwitchRenderCooldownColor = Color.Black * 0.35f;

        private bool HasntSelectedAnAnimYetAfterBuildingAnimList = true;

        public enum DividerDragMode
        {
            None,
            LeftVertical,
            RightVertical,
            RightPaneHorizontal,
            BottomLeftCornerResize
        }

        public enum ScreenMouseHoverKind
        {
            None,
            AnimList,
            EventGraph,
            Inspector,
            ModelViewer,
            DividerBetweenCenterAndLeftPane,
            DividerBetweenCenterAndRightPane,
            DividerRightPaneHorizontal,
            BottomLeftCornerOfModelViewer,
            ShaderAdjuster
        }

        public DbgMenus.DbgMenuPadRepeater NextAnimRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadDown, 0.4f, 0.016666667f);
        public DbgMenus.DbgMenuPadRepeater PrevAnimRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadUp, 0.4f, 0.016666667f);

        public DbgMenus.DbgMenuPadRepeater AnimHistoryForwardRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadDown, 0.4f, 0.016666667f);
        public DbgMenus.DbgMenuPadRepeater AnimHistoryBackwardRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadUp, 0.4f, 0.016666667f);

        public DbgMenus.DbgMenuPadRepeater NextAnimRepeaterButton_KeepSubID = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadDown, 0.4f, 0.016666667f);
        public DbgMenus.DbgMenuPadRepeater PrevAnimRepeaterButton_KeepSubID = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadUp, 0.4f, 0.016666667f);

        public DbgMenus.DbgMenuPadRepeater NextFrameRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadLeft, 0.3f, 0.016666667f);
        public DbgMenus.DbgMenuPadRepeater PrevFrameRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadRight, 0.3f, 0.016666667f);

        public static bool CurrentlyEditingSomethingInInspector;
        
        

        public SapImportFlver2Form ImporterWindow_FLVER2 = null;
        public SapImportFbxAnimForm ImporterWindow_FBXAnim = null;

        public TaePlaybackCursor PlaybackCursor => Graph?.PlaybackCursor;

        // public Rectangle ModelViewerBounds;
        // public Rectangle ModelViewerBounds_InputArea;

        private const int RECENT_FILES_MAX = 32;

        //TODO: CHECK THIS
        public static int TopMenuBarMargin => (int)Math.Ceiling(18 * Main.DPI);

        private int TopOfGraphAnimInfoMargin = 20;

        private int TransportHeight = 32;

        public TaeTransport Transport;

        public void GoToEventSource()
        {
            if (Graph.AnimRef?.IS_DUMMY_ANIM == true || Graph.GhostAnimRef?.IS_DUMMY_ANIM == true)
                return;

            var headerClone = Graph.AnimRef.SAFE_GetHeaderClone();
            if (headerClone is TAE.Animation.AnimFileHeader.ImportOtherAnim asImportOtherAnim)
            {

                var animRef = FileContainer.Proj.SAFE_GetFirstAnimationFromFullID(SplitAnimID.FromFullID(Proj, asImportOtherAnim.ImportFromAnimID));

                if (animRef == null || animRef.ParentCategory == null)
                {
                    DialogManager.DialogOK("Invalid Animation Reference", $"Animation ID referenced ({asImportOtherAnim.ImportFromAnimID}) does not exist.");
                    return;
                }
                SelectNewAnimRef(animRef.ParentCategory, animRef);
            }
        }

        //public bool CtrlHeld;
        //public bool ShiftHeld;
        //public bool AltHeld;

        private static object _lock_PauseUpdate = new object();
        private bool _PauseUpdate;
        private bool PauseUpdate
        {
            get
            {
                lock (_lock_PauseUpdate)
                    return _PauseUpdate;
            }
            set
            {
                lock (_lock_PauseUpdate)
                    _PauseUpdate = value;
            }
        }
        //private float _PauseUpdateTotalTime;
        //private float PauseUpdateTotalTime
        //{
        //    get
        //    {
        //        lock (_lock_PauseUpdate)
        //            return _PauseUpdateTotalTime;
        //    }
        //    set
        //    {
        //        lock (_lock_PauseUpdate)
        //            _PauseUpdateTotalTime = value;
        //    }
        //}

        //public Rectangle Rect;

        public Rectangle ModelViewerBounds => OSD.SpWindowViewport.GetRect().ToRectRounded();
        public Rectangle GraphRect => OSD.SpWindowGraph.GetRect().ToRectRounded();

        public Dictionary<DSAProj.Animation, TaeUndoMan> UndoManDictionary 
            = new Dictionary<DSAProj.Animation, TaeUndoMan>();

        private object _lock_UndoManDict = new object();
        
        public void DeleteCurrentAnimation()
        {
            if (false && SelectedAnimCategory.SAFE_GetAnimCount() <= 1)
            {
                DialogManager.DialogOK("Can't Delete Last Animation",
                    "Cannot delete the only animation remaining in the animation category.");
            }
            else
            {

                var category = SelectedAnimCategory;
                var anim = SelectedAnim;
                Proj.ParentDocument.EditorScreen.GlobalUndoMan.NewAction_AnimCategory(() =>
                {
                    int indexOfCurrentAnim = 0;
                    category.SafeAccessAnimations(anims =>
                    {
                        indexOfCurrentAnim = anims.IndexOf(anim);
                        category.INNER_RemoveAnimation(anim);
                    });
                    

                    RecreateAnimList();
                    UpdateSelectedTaeAnimInfoText();



                    category.SafeAccessAnimations(anims =>
                    {
                        if (indexOfCurrentAnim > anims.Count - 1)
                            indexOfCurrentAnim = anims.Count - 1;

                        if (indexOfCurrentAnim >= 0)
                            SelectNewAnimRef(category, anims[indexOfCurrentAnim]);
                        else
                            SelectNewAnimRef(category, anims[0]);
                    });


                }, actionDescription: $"Delete animation {anim.SplitID.GetFormattedIDString(Proj)}");



                SelectedAnimCategory.SAFE_SetIsModified(!IsReadOnlyFileMode);

                //DialogManager.DialogOK("Animation Deleted", "Animation has been deleted.");
            }
        }

        public TaeUndoMan GetUndoManForSpecificAnim(DSAProj.Animation anim)
        {
            TaeUndoMan result = null;
            lock (_lock_UndoManDict)
            {
                if (!UndoManDictionary.ContainsKey(anim))
                {
                    var newUndoMan = new TaeUndoMan(this, TaeUndoMan.UndoTypes.Anim);
                    UndoManDictionary.Add(anim, newUndoMan);
                }

                result = UndoManDictionary[anim];
            }

            return result;
        }

        public TaeUndoMan GlobalUndoMan;
        
        public TaeUndoMan CurrentAnimUndoMan
        {
            get
            {
                TaeUndoMan result = null;
                lock (_lock_UndoManDict)
                {
                    if (SelectedAnim != null)
                    {
                        if (!UndoManDictionary.ContainsKey(SelectedAnim))
                        {
                            var newUndoMan = new TaeUndoMan(this, TaeUndoMan.UndoTypes.Anim);
                            UndoManDictionary.Add(SelectedAnim, newUndoMan);
                        }

                        result = UndoManDictionary[SelectedAnim];
                    }
                }

                return result;
            }
        }

        //public bool IsModified
        //{
        //    get
        //    {
        //        try
        //        {
        //            return (SelectedAnimCategory?.Animations.Any(a => a.GetIsModified()) ?? false) ||
        //            (FileContainer?.AllTAE.Any(t => t.GetIsModified()) ?? false) || (FileContainer?.IsModified ?? false);
        //        }
        //        catch
        //        {
        //            return true;
        //        }
        //    }
        //}

        public bool IsModified => Proj?.SAFE_AnyModified() ?? false;

        private void PushNewRecentFile(DSAProj.TaeContainerInfo containerInfo)
        {
            lock (Main.Config._lock_ThreadSensitiveStuff)
            {
                var existingMatches = Config.RecentFilesList.Where(f => ((DSAProj.TaeContainerInfo)f).IsSameFileAs(containerInfo)).ToList();
                if (existingMatches.Count > 0)
                {
                    foreach (var match in existingMatches)
                        Config.RecentFilesList.Remove(match);
                }

                Config.RecentFilesList.Insert(0, containerInfo);
            }

            Main.SaveConfig();
        }


        private TaeButtonRepeater UndoButton = new TaeButtonRepeater(0.4f, 0.05f);
        private TaeButtonRepeater RedoButton = new TaeButtonRepeater(0.4f, 0.05f);

        
        public TaeFileContainer FileContainer;

        public string NewFileContainerName => FileContainer?.Info?.GetMainBinderName();
        public string NewFileContainerName_2010 => FileContainer?.Info?.GetMainBinderName() + ".2010";
        public string NewFileContainerName_Model
        {
            get
            {
                if (FileContainer != null && FileContainer.Info is DSAProj.TaeContainerInfo.ContainerAnibnd asAnibnd)
                {
                    return asAnibnd.ChrbndPath;
                }
                return null;
            }
        }

        public bool IsFileOpen => FileContainer != null;

        public DSAProj Proj => FileContainer?.Proj;
        
        public DSAProj.AnimCategory SelectedAnimCategory { get; private set; }
        
        public DSAProj.Animation SelectedAnim { get; private set; }

        public string SelectedTaeAnimInfoText;

        public bool TaeAnimInfoIsClone { get; private set; } = false;

        public readonly System.Windows.Forms.Form GameWindowAsForm;

        public DSAProj.Action QueuedChangeActionType = null;

        public DSAProj.Action InspectorAction => NewSelectedActions.Count == 1 ? NewSelectedActions[0] : null;

        public List<DSAProj.Action> NewSelectedActions = new();

        public void AddTagToSelectedActions(DSAProj proj, DSAProj.Tag tag)
        {
            Graph.MainScreen.CurrentAnimUndoMan.NewAction(() =>
            {
                var actions = NewSelectedActions.ToList();
                lock (proj._lock_Tags)
                {
                    foreach (var act in actions)
                    {
                        if (!act.Info.TagInstances.Contains(tag))
                            act.Info.TagInstances.Add(tag);
                    }

                    if (!proj.Tags.Contains(tag))
                        proj.Tags.Add(tag);
                }
            }, () => { }, "Add tag to selection action(s)");
            Graph.AnimRef.SAFE_SetIsModified(true);
        }

        public void ClearAllTagsFromSelectedActions(DSAProj proj)
        {
            Graph.MainScreen.CurrentAnimUndoMan.NewAction(() =>
            {
                var actions = NewSelectedActions.ToList();
                lock (proj._lock_Tags)
                {
                    foreach (var act in actions)
                    {
                        act.Info.TagInstances.Clear();
                    }
                }
            }, () => { }, "Clear tags from selected action(s)");
            Graph.AnimRef.SAFE_SetIsModified(true);
        }

        public void SetColorOfSelectedActions(bool isClearing)
        {
            Graph.MainScreen.CurrentAnimUndoMan.NewAction(() =>
            {
                var actions = NewSelectedActions.ToList();
                if (actions.Count < 1)
                    return;
                if (isClearing)
                {
                    foreach (var act in actions)
                        act.Info.CustomColor = null;
                }
                else
                {
                    DialogManager.ShowDialogChangeActionColors(actions);
                }
            }, () => { }, isClearing ? "Clear custom colors of selected action(s)" : "Set custom color of selected action(s)");
            Graph.AnimRef.SAFE_SetIsModified(true);
        }


        public void RefreshTemplateForAllActions(TAE.Template.ActionTemplate actTemplate, bool isSoftRefresh)
        {
            if (isSoftRefresh)
            {
                actTemplate.RefreshGUID();
                return;
            }

            foreach (var tae in FileContainer.AllTAE)
            {
                tae.SafeAccessAnimations(anims =>
                {
                    foreach (var anim in anims)
                    {
                        anim.UnSafeAccessActions(actions =>
                        {
                            foreach (var act in actions)
                            {
                                if (act.Template == actTemplate)
                                {
                                    act.NewLoadParamsFromBytes(lenientOnAssert: true);
                                    act.RequestUpdateText();
                                }
                                else if (act.Type == actTemplate.ID)
                                {
                                    act.Template = actTemplate;
                                    act.NewLoadParamsFromBytes(lenientOnAssert: true);
                                    act.RequestUpdateText();
                                }
                            }
                        });
                       
                    }
                });
               
            }
        }

        public void RefreshTextForAllActions()
        {
            foreach (var tae in FileContainer.AllTAE)
            {
                tae.SafeAccessAnimations(anims =>
                {
                    foreach (var anim in anims)
                    {
                        anim.UnSafeAccessActions(actions =>
                        {
                            foreach (var act in actions)
                            {
                                act.NewLoadParamsFromBytes(lenientOnAssert: true);
                                act.RequestUpdateText();
                            }
                        });

                    }
                });
               
            }
        }

        public DSAProj.Action MultiEditHoverAction = null;

        public DSAProj.Action NewHoverAction = null;
        public DSAProj.Action NewHoverAction_NeedsNoSelection = null;
        public DSAProj.Action NewHoverActionPrevFrame = null;

        public bool SingleEventBoxSelected => InspectorAction != null;
        
        public NewGraph Graph { get; private set; }
        public bool IsCurrentlyLoadingGraph { get; private set; } = false;

        private Color ColorInspectorBG = Color.DarkGray;
        public FancyInputHandler Input => Main.Input;

        public bool SuppressNextModelOverridePrompt = false;

        public bool IsReadOnlyFileMode = false;

        public TaeConfigFile Config => Main.Config;

        public bool? NewLoadFile_FromDocManager(DSAProj.TaeContainerInfo containerInfo)
        {
            
            IsCurrentlyLoadingGraph = true;
            ParentDocument.Scene.DisableModelDrawing();
            ParentDocument.Scene.DisableModelDrawing2();

            // Even if it fails to load, just always push it to the recent files list
            PushNewRecentFile(containerInfo);

            //string templateName = BrowseForXMLTemplate();

            //if (templateName == null)
            //{
            //    return false;
            //}

            //CurrentFileContainerInfo = containerInfo;
            var mainContainerName = containerInfo.GetMainBinderName();

            bool loadedSuccessfully = false;
            //var prevContainerInfo = CurrentFileContainerInfo.GetClone();

            if (System.IO.File.Exists(mainContainerName))
            {
                FileContainer = new TaeFileContainer(this);

                try
                {
                    string folder = new System.IO.FileInfo(mainContainerName).DirectoryName;

                    int lastSlashInFolder = folder.LastIndexOf("\\");

                    string interroot = folder.Substring(0, lastSlashInFolder);

                    string oodleSource = Utils.Frankenpath(interroot, "oo2core_6_win64.dll");

                    // modengine check
                    if (!File.Exists(oodleSource))
                    {
                        oodleSource = Utils.Frankenpath(interroot, @"..\oo2core_6_win64.dll");
                    }

                    //if (!File.Exists(oodleSource))
                    //{
                    //    System.Windows.Forms.MessageBox.Show("Was unable to automatically find the " +
                    //    "`oo2core_6_win64.dll` file in the Sekiro folder. Please copy that file to the " +
                    //    "'lib' folder next to DS Anim Studio.exe in order to load Sekiro files.", "Unable to find compression DLL",
                    //    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

                    //    return false;
                    //}

                    string oodleTarget = Utils.Frankenpath(Main.Directory, "oo2core_6_win64.dll");

                    if (System.IO.File.Exists(oodleSource) && !System.IO.File.Exists(oodleTarget))
                    {
                        System.IO.File.Copy(oodleSource, oodleTarget, true);

                        zzz_NotificationManagerIns.PushNotification("Oodle compression library was automatically copied from game directory " +
                                            "to editor's directory and Sekiro / Elden Ring files will load now.");
                    }
                    var mainBinderName = containerInfo.GetMainBinderName();
                    var mainBinder = Utils.ReadBinder(mainBinderName);
                    TaeFileContainer.CheckGameVersionForTaeInterop(mainBinderName, mainBinder);

                    if (Main.REQUEST_REINIT_EDITOR)
                        return false;

                    CheckAutoLoadXMLTemplate(containerInfo.GetMainBinderName());
                    var loadResult = FileContainer.NewLoadFromContainerInfo(containerInfo, out string loadErrMsg, FileContainer.TaeTemplate, ParentDocument);
                    if (loadResult)
                    {
                        loadedSuccessfully = true;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show($"Unable to load file. Error shown below.\n\n{loadErrMsg}", "Load Failed",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    }
                }
                catch (System.DllNotFoundException)
                {
                    //System.Windows.Forms.MessageBox.Show("Cannot open Sekiro files unless you " +
                    //    "copy the `oo2core_6_win64.dll` file from the Sekiro folder into the " +
                    //    "'lib' folder next to DS Anim Studio.exe.", "Additional DLL Required", 
                    //    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

                    System.Windows.Forms.MessageBox.Show("Was unable to automatically find the " +
                        "`oo2core_6_win64.dll` file in the Sekiro / Elden Ring game folder. Please copy that file to the " +
                        "program folder next to DS Anim Studio.exe in order to load Sekiro / Elden Ring files.", "Unable to find compression DLL",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);

                    Main.REQUEST_REINIT_EDITOR = true;

                    return false;
                }
                

                if (!FileContainer.AllTAE.Any())
                {
                    //if (TryAnimLoadFallback())
                    //{
                    //    return NewLoadFile();
                    //}
                    //else
                    //{
                    //    Main.REQUEST_REINIT_EDITOR = true;
                    //    return false;
                    //}

                    System.Windows.Forms.MessageBox.Show("No TAE files in selected binder. Cancelling load operation.", "Unsupported File",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);

                    Main.REQUEST_REINIT_EDITOR = true;

                    return false;
                }

                LoadTaeFileContainer(FileContainer);

                //if (templateName != null)
                //{
                //    LoadTAETemplate(templateName);
                //}

                IsCurrentlyLoadingGraph = false;

                OSD.SpWindowGraph.IsRequestFocus = true;
                
                
                return true;
            }
            else
            {
                return null;
            }
        }

        public void SaveCustomXMLTemplate()
        {
            var check = Utils.GetFileNameWithoutAnyExtensions(Utils.GetFileNameWithoutDirectoryOrExtension(NewFileContainerName)).ToLower();
            var objCheck = check.StartsWith("o") &&
               (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R);
            var remoCheck = check.StartsWith("scn") &&
                (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R);

            //var xmlPath = System.IO.Path.Combine(
            //    new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName,
            //    $@"Res\TAE.Template.{(FileContainer.IsBloodborne ? "BB" : SelectedTae.Format.ToString())}{(objCheck ? ".OBJ" : "")}.xml");

            string taeFormatStr = $"{ParentDocument.GameRoot.GameType}";

            if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                taeFormatStr = "DS1";

            var customXmlFolder = System.IO.Path.Combine(
                new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName, $@"CustomTemplates");

            if (!Directory.Exists(customXmlFolder))
                Directory.CreateDirectory(customXmlFolder);

            var customXmlPath = System.IO.Path.Combine(
                new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName,
                $@"CustomTemplates\TAE.CustomTemplate.{taeFormatStr}{(remoCheck ? ".REMO" : objCheck ? ".OBJ" : "")}.xml");

            SaveTAETemplate(customXmlPath);
        }

        public void CheckAutoLoadXMLTemplate(string fileContainerName)
        {
            if (ParentDocument.GameRoot.GameType == SoulsGames.None)
                return;
            
            var check = Utils.GetFileNameWithoutAnyExtensions(Utils.GetFileNameWithoutDirectoryOrExtension(fileContainerName)).ToLower();
            var objCheck = check.StartsWith("o") &&
                (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R);
            var remoCheck = check.StartsWith("scn") &&
                (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R);

            //var xmlPath = System.IO.Path.Combine(
            //    new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName,
            //    $@"Res\TAE.Template.{(FileContainer.IsBloodborne ? "BB" : SelectedTae.Format.ToString())}{(objCheck ? ".OBJ" : "")}.xml");

            string taeFormatStr = $"{ParentDocument.GameRoot.GameType}";

            if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                taeFormatStr = "DS1";

            var xmlPath = System.IO.Path.Combine(
                new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName,
                $@"Res\TAE.Template.{taeFormatStr}{(remoCheck ? ".REMO" : objCheck ? ".OBJ" : "")}.xml");

            var customXmlFolder = System.IO.Path.Combine(
                new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName, $@"CustomTemplates");

            if (!Directory.Exists(customXmlFolder))
                Directory.CreateDirectory(customXmlFolder);

            var customXmlPath = System.IO.Path.Combine(
                new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName,
                $@"CustomTemplates\TAE.CustomTemplate.{taeFormatStr}{(remoCheck ? ".REMO" : objCheck ? ".OBJ" : "")}.xml");


            if (System.IO.File.Exists(customXmlPath))
                SetTAETemplate(customXmlPath);
            else if (System.IO.File.Exists(xmlPath))
                SetTAETemplate(xmlPath);
        }

        public void SaveCurrentFile(Action afterSaveAction = null, string saveMessage = "Saving ANIBND...")
        {
            if (!IsFileOpen)
                return;

            CommitActiveGraphToTaeStruct();

            ParentDocument.GameData.SaveProjectJson();

            if (IsReadOnlyFileMode)
            {
                System.Windows.Forms.MessageBox.Show("Read-only mode is" +
                    " active so nothing was saved. To open a file in re-saveable mode," +
                    " make sure the Read-Only checkbox is unchecked in the open" +
                    " file dialog.", "Read-Only Mode Active",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Stop);
                return;
            }

            if (System.IO.File.Exists(NewFileContainerName) && 
                !System.IO.File.Exists(NewFileContainerName + BackupExtension))
            {
                System.IO.File.Copy(NewFileContainerName, NewFileContainerName + BackupExtension);
                zzz_NotificationManagerIns.PushNotification(
                    "A backup was not found and was created:\n" + NewFileContainerName + BackupExtension);
            }

            ParentDocument.LoadingTaskMan.DoLoadingTask("SaveFile", saveMessage, progress =>
            {
                //FileContainer.SaveToPath(FileContainerName, progress);
                FileContainer.NewSaveContainer(out bool savedDsaproj, out bool savedContainer, progress, 0, 1);

                if (savedContainer || savedDsaproj)
                {

                    var sb = new StringBuilder();

                    if (savedContainer)
                        sb.AppendLine($"Outputted to game file '{NewFileContainerName}'.");

                    if (savedDsaproj)
                    {
                        if (savedContainer)
                            sb.AppendLine();
                        sb.AppendLine($"Saved to project file '{FileContainer.Proj.ContainerInfo.GetDSAProjFileName()}'.");
                    }

                    zzz_NotificationManagerIns.PushNotification(sb.ToString());
                }

                
                if (Config.ExtensiveBackupsEnabled)
                {
                    var filesInDir = Directory.GetFiles(Path.GetDirectoryName(NewFileContainerName));
                    int maxBackupIndex = 0;
                    int maxBackupIndex_2010 = 0;
                    string baseBakPath = NewFileContainerName + ".dsasautobak";
                    string baseBakPath_2010 = NewFileContainerName_2010 + ".dsasautobak";
                    foreach (var f in filesInDir)
                    {
                        if (f.StartsWith(baseBakPath) && f.Length > baseBakPath.Length)
                        {
                            if (int.TryParse(f.Substring(baseBakPath.Length), out int backupIndexParsed))
                            {
                                if (backupIndexParsed > maxBackupIndex)
                                    maxBackupIndex = backupIndexParsed;
                            }
                        }
                        else if (f.StartsWith(baseBakPath_2010) && f.Length > baseBakPath_2010.Length)
                        {
                            if (int.TryParse(f.Substring(baseBakPath_2010.Length), out int backupIndexParsed_2010))
                            {
                                if (backupIndexParsed_2010 > maxBackupIndex_2010)
                                    maxBackupIndex_2010 = backupIndexParsed_2010;
                            }
                        }
                    }

                    var md5 = System.Security.Cryptography.MD5.Create();

                    if (!File.Exists($"{baseBakPath}{maxBackupIndex:D4}"))
                    {
                        string bakName = $"{baseBakPath}{(maxBackupIndex + 1):D4}";
                        File.Copy(NewFileContainerName, bakName);
                        zzz_NotificationManagerIns.PushNotification($"Saved file to '{bakName}'.");
                    }
                    else
                    {
                        var hash_bak = md5.ComputeHash(File.ReadAllBytes($"{baseBakPath}{maxBackupIndex:D4}"));
                        var hash = md5.ComputeHash(File.ReadAllBytes(NewFileContainerName));
                        string bakName = $"{baseBakPath}{(maxBackupIndex + 1):D4}";
                        if (!hash_bak.SequenceEqual(hash))
                        {
                            File.Copy(NewFileContainerName, bakName);
                            zzz_NotificationManagerIns.PushNotification($"Saved file to '{bakName}'.");
                        }
                    }

                    


                }

                if (Config.LiveRefreshOnSave)
                {
                    LiveRefresh();
                }

                progress.Report(1.0);

                afterSaveAction?.Invoke();
            }, flags: zzz_LoadingTaskManIns.TaskFlags.Critical);

            
        }

        private void LoadAnimIntoGraph(DSAProj.AnimCategory tae, DSAProj.Animation anim)
        {
            //if (!GraphLookup.ContainsKey(anim))
            //{
            //    var graph = new TaeEditAnimEventGraph(this, false, anim);
            //    GraphLookup.Add(anim, graph);

            //}

            //Graph = GraphLookup[anim];

            var selAnim = Proj.SAFE_SolveAnimRefChain(anim.SplitID);
            if (selAnim == anim)
                selAnim = null;

            if (Graph == null)
                Graph = new NewGraph(this, anim, selAnim);
            else
            {
                var graph = Graph;
                Graph = null;
                graph.ReadFromAnimRef(anim, selAnim);
                Graph = graph;
            }

            

        }

        private void LoadTaeFileContainer(TaeFileContainer fileContainer)
        {
            TaeExtensionMethods.ClearMemes();
            FileContainer = fileContainer;
            SelectedAnimCategory = FileContainer.AllTAE.First();
            GameWindowAsForm.Invoke(new Action(() =>
            {
                OSD.WindowComboViewer.IsOpen = false;
                ExportAllAnimsMenu?.Close();
                ExportAllAnimsMenu = null;
            }));
            if (SelectedAnimCategory.SAFE_GetAnimCount() == 0)
            {
                SelectedAnimCategory.SAFE_CreateDummyAnimation(Proj, 0);
            }
            SelectedAnim = SelectedAnimCategory.SAFE_GetAnimByIndex(0);

            
            Graph = null;
            LoadAnimIntoGraph(SelectedAnimCategory, SelectedAnim);
            if (Main.REQUEST_REINIT_EDITOR)
                return;

            FileContainer.Proj.SafeAccessAnimCategoriesList(categoriesList =>
            {
                foreach (var category in categoriesList)
                {
                    category.IsTreeNodeOpen = !Main.Config.AutoCollapseAllTaeSections;
                }
            });

            

            IsCurrentlyLoadingGraph = false;

            //if (FileContainer.ContainerType != TaeFileContainer.TaeFileContainerType.TAE)
            //{
            //    TaeInterop.OnLoadANIBND(MenuBar, progress);
            //}
            ApplyAlreadySetTAETemplate();
            SelectNewAnimRef(SelectedAnimCategory, SelectedAnimCategory.SAFE_GetAnimByIndex(0));
            
            RecreateAnimList();
        }

        public void RecreateAnimList()
        {


            //Vector2 oldScroll = AnimationListScreen.ScrollViewer.Scroll;
            //var sectionsCollapsed = AnimationListScreen
            //    .AnimTaeSections
            //    .ToDictionary(x => x.Key, x => x.Value.Collapsed);

            //AnimationListScreen = new TaeEditAnimList(this);


            //foreach (var section in AnimationListScreen.AnimTaeSections)
            //{
            //    if (sectionsCollapsed.ContainsKey(section.Key))
            //        section.Value.Collapsed = sectionsCollapsed[section.Key];
            //}

            //AnimationListScreen.ScrollViewer.Scroll = oldScroll;

            //FileContainer?.Proj?.ScanForErrors_Background();
            var proj = FileContainer?.Proj;
            if (proj != null)
            {
                proj.SAFE_ResortAnimCategoryIDs();
                proj.ScanForErrors_Background();
                proj.TimeSinceLastErrorCheck = 0;
            }


            HasntSelectedAnAnimYetAfterBuildingAnimList = true;

            ParentDocument.SpWindowAnimations.ClearTaeBlockCache();
        }

        public void ResortTracks_Anim()
        {
            CurrentAnimUndoMan.NewAction(doAction: () =>
            {
                SelectedAnim.SAFE_ResortTracks(DSAProj.Animation.TrackSortTypes.FirstActionType);
            }, undoAction: () =>
            {

            }, "Sort action tracks");
        }

        public void RegenTrackNames_Anim()
        {
            CurrentAnimUndoMan.NewAction(doAction: () =>
            {
                SelectedAnim.SAFE_GenerateTrackNames(Proj.Template, true);
            }, undoAction: () =>
            {

            }, "Regenerate action track names");
        }

        public void ResortTracks_Proj()
        {
            GlobalUndoMan.NewAction(doAction: () =>
            {
                ParentDocument.LoadingTaskMan.DoLoadingTask(null, null, prog =>
                {
                    foreach (var t in FileContainer.AllTAE)
                    {
                        t.SafeAccessAnimations(anims =>
                        {
                            foreach (var a in anims)
                            {
                                a.INNER_ResortTracks(DSAProj.Animation.TrackSortTypes.FirstActionType);
                            }
                        });

                    }
                });
               
            }, undoAction: () =>
            {

            }, "Sort action tracks");
        }

        public void RegenTrackNames_Proj()
        {
            GlobalUndoMan.NewAction(doAction: () =>
            {
                foreach (var t in FileContainer.AllTAE)
                {
                    t.SafeAccessAnimations(anims =>
                    {
                        foreach (var a in anims)
                        {
                            a.INNER_GenerateTrackNames(Proj.Template, true);
                        }
                    });
                    
                }
            }, undoAction: () =>
            {

            }, "Regenerate action track names");
        }

        private void ResortAndRegenRows_Inner_Deprecated(DSAProj.Animation anim)
        {
            //var actions = anim.SAFE_GetActions();
            //var sortDict = new Dictionary<string, List<DSAProj.Action>>();
            //foreach (var act in actions)
            //{
            //    var sortString = $"{act.Type} {string.Join(", ", act.ParameterBytes.Select(x => x.ToString("X2")))}";
            //    if (!sortDict.ContainsKey(sortString))
            //        sortDict[sortString] = new List<DSAProj.Action>();

            //    if (!sortDict[sortString].Contains(act))
            //        sortDict[sortString].Add(act);
            //}

            //var sortDictKeys = sortDict.Keys.ToList();
            //foreach (var k in sortDictKeys)
            //{
            //    var internalSimName = sortDict[k][0].GetInternalSimTypeName();
            //    var isFFXGroup = internalSimName.StartsWith("FFX_");
            //    var isSoundGroup = internalSimName.Contains("Play") && internalSimName.Contains("Sound");
            //    if (sortDict[k].Count == 1 && !(isFFXGroup || isSoundGroup))
            //    {
            //        sortDict.Remove(k);
            //    }
            //}
            
            //anim.ActionTracks.Clear();

            //    var remainingActions = actions.ToList();
            //    sortDictKeys = sortDict.Keys.OrderBy(x => x).ToList();
            //    foreach (var k in sortDictKeys)
            //    {
            //        foreach (var act in sortDict[k])
            //        {
            //            remainingActions.Remove(act);
            //            // Set index to the track we are about to add.
            //            act.TrackIndex = anim.ActionTracks.Count;
            //        }

            //        var trackName = sortDict[k][0].GraphDisplayText;
            //        var track = new DSAProj.ActionTrack()
            //        {
            //            Info = new DSAProj.EditorInfo(trackName),
            //            TrackData = new TAE.ActionTrack.ActionTrackDataStruct()
            //            {
            //                DataType = TAE.ActionTrack.ActionTrackDataType.TrackData0,
            //            },
            //            TrackType = sortDict[k][0].Type,
            //            NewActionDefaultType = sortDict[k][0].Type,
            //            NewActionDefaultParameters = sortDict[k][0].ParameterBytes,
            //            NewActionDefaultLength = sortDict[k][0].EndTime - sortDict[k][0].StartTime,
            //        };

            //        anim.ActionTracks.Add(track);
            //    }

            //    var actionsByType = new Dictionary<int, List<DSAProj.Action>>();
                

            //    foreach (var act in remainingActions)
            //    {
            //        var memeType = (act.Type * 100000);

            //        if (act.Type == 0 || act.Type == 300)
            //        {
            //            var jumpTableID = Convert.ToInt32(act.ReadInternalSimField("JumpTableID"));
            //            memeType = jumpTableID;
            //        }

            //        if (!actionsByType.ContainsKey(memeType))
            //            actionsByType[memeType] = new List<DSAProj.Action>();

            //        if (!actionsByType[memeType].Contains(act))
            //            actionsByType[memeType].Add(act);
            //    }

            //    var actionTypeKeys = actionsByType.Keys.OrderBy(x => x).ToList();

            //    foreach (var k in actionTypeKeys)
            //    {
            //        void doActionList(List<DSAProj.Action> actList)
            //        {
            //            foreach (var ev in actList)
            //            {
            //                // Set track index to the track we are about to add
            //                ev.TrackIndex = anim.ActionTracks.Count;
            //            }

            //            string trackName = (actList[0].TypeName ?? $"Type{(actList[0].Type)}");

            //            if (k < 100000)
            //            {
            //                int jumpTableID = k;
            //                var firstEvent = actList[0];
            //                bool foundEnumName = false;
            //                //0: JumpTableID
            //                foreach (var en in firstEvent.Template[0].EnumEntries)
            //                {
            //                    if (Convert.ToInt32(en.Value) == jumpTableID)
            //                    {
            //                        trackName = en.Key;
            //                        foundEnumName = true;
            //                        break;
            //                    }
            //                }

            //                if (!foundEnumName)
            //                {
            //                    trackName = $"ChrActionFlag {jumpTableID}";
            //                }
            //            }

            //            var track = new DSAProj.ActionTrack()
            //            {
            //                Info = new DSAProj.EditorInfo(trackName),
            //                TrackData = new TAE.ActionTrack.ActionTrackDataStruct()
            //                {
            //                    DataType = TAE.ActionTrack.ActionTrackDataType.TrackData0,
            //                },
            //                TrackType = actList[0].Type,
            //                NewActionDefaultType = actList[0].Type,
            //            };

            //            anim.ActionTracks.Add(track);
            //        }

            //        List<List<DSAProj.Action>> splitRows = new List<List<DSAProj.Action>>();
            //        List<DSAProj.Action> currentRow = new List<DSAProj.Action>();

            //        void commitCurrentRow()
            //        {
            //            if (currentRow.Count > 0)
            //            {
            //                splitRows.Add(currentRow.ToList());
            //                currentRow = new List<DSAProj.Action>();
            //            }
            //        }

            //        foreach (var act in actionsByType[k])
            //        {
            //            if (currentRow.Any(e => e.EndTime > act.StartTime || e.StartTime > act.StartTime))
            //            {
            //                commitCurrentRow();
            //            }
            //            currentRow.Add(act);
            //        }

            //        commitCurrentRow();

            //        foreach (var row in splitRows)
            //        {
            //            doActionList(row);
            //        }
            //    }

            //    anim.SetIsModified(true);
            
            
        }

        public void ShowDialogDuplicateCurrentAnimation()
        {
            if (FileContainer == null || SelectedAnimCategory == null)
                return;

            if (SelectedAnim?.IS_DUMMY_ANIM != false)
                return;

                var currentAnim = SelectedAnim;
            var currentAnimID = SelectedAnim.SplitID;

            DialogManager.AskForInputString("Duplicate To New Animation ID", "Enter the animation ID to duplicate the current animation to.\n" +
                "Accepts the full string with prefix or just the ID as a number.",
                ParentDocument.GameRoot.CurrentAnimIDFormatType.ToString(), result =>
                {
                    Main.WinForm.Invoke(new Action(() =>
                    {
                        bool parseSuccess =
                            SplitAnimID.TryParse(Proj, result, out SplitAnimID splitID, out string detailedError);

                        if (!parseSuccess)
                        {
                            DialogManager.DialogOK("Duplication Failed", detailedError);
                            return;
                        }

                        
                        // if (splitID == currentAnimID)
                        //{
                        //    DialogManager.DialogOK("Duplication Failed", $"Animation {splitID.GetFormattedIDString(Proj)} is the current animation.");
                        //    return;
                        //}

                        var existingAnim = Proj.SAFE_GetFirstAnimationFromFullID(splitID);
                        if (existingAnim == null)
                        {
                            var category = Proj.SAFE_RegistCategory(splitID.CategoryID);
                            var newAnimRef = new DSAProj.Animation(Proj, category);
                            ImmediateImportAnim(newAnimRef, currentAnimID);
                            newAnimRef.SplitID = splitID;
                            category.SAFE_AddAnimation(newAnimRef);
                            RecreateAnimList();
                            SelectNewAnimRef(category, newAnimRef);
                        }
                        else
                        {
                            DialogManager.AskYesNo("Anim ID Already Exists",
                                $"An animation already exists with ID {splitID.GetFormattedIDString(Proj)}. Duplicating to this ID will cause an ID conflict. Continue anyways?",
                                choice =>
                            {
                                if (choice)
                                {
                                    var newCategory = existingAnim.ParentCategory;

                                    var newAnimRef = new DSAProj.Animation(Proj, newCategory);
                                    ImmediateImportAnim(newAnimRef, currentAnimID);
                                    newAnimRef.SplitID = splitID;

                                    //newCategory.NEW_RemoveAnimation(existingAnim);
                                    newCategory.SAFE_AddAnimation(newAnimRef);
                                    newCategory.SAFE_ResortAnimIDs();

                                    //var existingIndex = SelectedAnimCategory.Animations.IndexOf(existingAnim);
                                    //if (existingIndex >= 0 && existingIndex < SelectedAnimCategory.Animations.Count)
                                    //{
                                    //    SelectedAnimCategory.Animations[existingIndex] = newAnimRef;
                                    //}
                                    //else
                                    //{
                                    //    SelectedAnimCategory.Animations.Add(newAnimRef);
                                    //    SelectedAnimCategory.Animations = SelectedAnimCategory.Animations
                                    //        .OrderBy(ani => (long)ani.NewID.GetFullID(Proj)).ToList();
                                    //}
                                    RecreateAnimList();
                                    SelectNewAnimRef(newCategory, newAnimRef);
                                }
                            });
                        }
                        
                    }));
                },checkError: input =>
                {
                    bool parseSuccess = SplitAnimID.TryParse(Proj, input, out SplitAnimID parsed, out string detailedError);

                    if (!parseSuccess)
                        return detailedError;

                    //if (parsed == currentAnimID)
                    //{
                    //    return $"Cannot duplicate to the same ID as the current animation, please enter any other ID.";
                    //}

                    return null;
                }, canBeCancelled: true);

            
        }

        public void SaveTAETemplate(string xmlFile)
        {
            

            try
            {
                var template = FileContainer.TaeTemplate;
                if (template != null)
                {
                    Dictionary<long, TAE.Template.ActionTemplate> orderedTemplate = new();
                    var keys = template.Keys.OrderBy(x => x).ToList();
                    foreach (var k in keys)
                    {
                        orderedTemplate[k] = template[k];
                    }
                    template.Clear();
                    foreach (var kvp in orderedTemplate)
                    {
                        template[kvp.Key] = kvp.Value;
                    }
                    template.WriteToXMLFile(xmlFile);
                }
            }
            catch (Exception ex) when (Main.EnableErrorHandler.ApplyTaeTemplate)
            {
                System.Windows.Forms.MessageBox.Show($"Failed to write TAE template:\n\n{ex}",
                    "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        public void SetTAETemplate(string xmlFile)
        {
            FileContainer.TaeTemplate = TAE.Template.ReadXMLFile(xmlFile, disableErrorHandling: !Main.EnableErrorHandler.LoadTaeTemplate);
        }

        public void ApplyAlreadySetTAETemplate()
        {
            FileContainer?.Proj?.SAFE_ApplyTemplate(FileContainer.TaeTemplate);
        }
     
        //public void CleanupForReinit()
        //{
        //    GameWindowAsForm.FormClosing -= GameWindowAsForm_FormClosing;
        //}

        public TaeEditorScreen(zzz_DocumentIns parentDocument, System.Windows.Forms.Form gameWindowAsForm)
        {
            ParentDocument = parentDocument;
            GlobalUndoMan = new TaeUndoMan(this, TaeUndoMan.UndoTypes.Proj);
            
            //Main.LoadConfig();

            GameWindowAsForm = gameWindowAsForm;

            GameWindowAsForm.MinimumSize = new System.Drawing.Size(1280  - 64, 720 - 64);

            Transport = new TaeTransport(this);
        }

        public void SetAllTAESectionsCollapsed(bool collapsed)
        {
            if (collapsed)
                ParentDocument.SpWindowAnimations.CollapseAll();
            else
                ParentDocument.SpWindowAnimations.ExpandAll();
        }

        public void LoadContent(ContentManager c)
        {
            Transport.LoadContent(c);
        }

        public void LiveRefresh()
        {
            

            var chrNameBase = Utils.GetFileNameWithoutAnyExtensions(Utils.GetFileNameWithoutDirectoryOrExtension(NewFileContainerName)).ToLower();

            if (DSAnimStudio.LiveRefresh.RequestFileReload.CanReloadEntity(chrNameBase))
            {
                if (chrNameBase.StartsWith("c"))
                {
                    if (DSAnimStudio.LiveRefresh.RequestFileReload.RequestReload(
                        DSAnimStudio.LiveRefresh.RequestFileReload.ReloadType.Chr, chrNameBase))
                        zzz_NotificationManagerIns.PushNotification($"Requested game to reload character '{chrNameBase}'.");

                }
                else if (chrNameBase.StartsWith("o"))
                {
                    if (DSAnimStudio.LiveRefresh.RequestFileReload.RequestReload(
                        DSAnimStudio.LiveRefresh.RequestFileReload.ReloadType.Object, chrNameBase))
                        zzz_NotificationManagerIns.PushNotification($"Requested game to reload object '{chrNameBase}'.");
                }
            }

                

            
            //if (FileContainer.ReloadType == TaeFileContainer.TaeFileContainerReloadType.CHR_PTDE || FileContainer.ReloadType == TaeFileContainer.TaeFileContainerReloadType.CHR_DS1R)
            //{
            //    var chrName = Utils.GetFileNameWithoutDirectoryOrExtension(FileContainerName);

            //    //In case of .anibnd.dcx
            //    chrName = Utils.GetFileNameWithoutDirectoryOrExtension(chrName);

            //    if (chrName.ToLower().StartsWith("c") && chrName.Length == 5)
            //    {
            //        if (FileContainer.ReloadType == TaeFileContainer.TaeFileContainerReloadType.CHR_PTDE)
            //        {
            //            TaeLiveRefresh.ForceReloadCHR_PTDE(chrName);
            //        }
            //        else
            //        {
            //            TaeLiveRefresh.ForceReloadCHR_DS1R(chrName);
            //        }
            //    }
            //}
        }

        //public void ShowDialogEditTaeHeader()
        //{

        //    PauseUpdate = true;
        //    var editForm = new TaeEditTaeHeaderForm(SelectedAnimCategory);
        //    editForm.Owner = GameWindowAsForm;
        //    editForm.ShowDialog();

        //    if (editForm.WereThingsChanged)
        //    {
        //        SelectedAnimCategory.SetIsModified(true);

        //        UpdateSelectedTaeAnimInfoText();
        //    }

        //    PauseUpdate = false;
        //}

        public void ShowDialogEditAnimCategoryProperties(DSAProj.AnimCategory category)
        {
            DialogManager.ShowTaeAnimCategoryPropertiesEditor(Proj, category);
        }

        //public bool DoesAnimIDExist(int id)
        //{
        //    foreach (var s in AnimationListScreen.AnimTaeSections.Values)
        //    {
        //        var matchedAnims = s.InfoMap.Where(x => x.Value.FullID == id);
        //        if (matchedAnims.Any())
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        public bool GotoAnimSectionID(int id, bool scrollOnCenter)
        {
            if (FileContainer.Proj.SAFE_CategoryExists(id))
            {
                var firstCategory = FileContainer.Proj.SAFE_GetFirstAnimCategoryFromCategoryID(id);



                var firstAnimInCategory = firstCategory.SAFE_GetFirstAnimInList();
                if (firstAnimInCategory != null)
                {
                    SelectNewAnimRef(firstCategory, firstAnimInCategory, scrollOnCenter, isPushCurrentToHistBackwardStack: true);
                    return true;
                }
            }

            return false;
        }


        public DSAProj.Animation CreateNewAnimWithFullID(SplitAnimID id, string animName, bool nukeExisting)
        {
            if (!Proj.SAFE_AnimExists(id) || nukeExisting)
            {
                var category = Proj.SAFE_RegistCategory(id.CategoryID);
                var anim = new DSAProj.Animation(Proj, category)
                {
                    SplitID = id,
                };

                anim.SAFE_SetHeader(new TAE.Animation.AnimFileHeader.Standard()
                {
                    AnimFileName = animName,
                });
                //var category = Proj.RegistCategory(id.CategoryID);
                //var existing = category.Animations.Where(a => a.SplitID == id);
                category.SAFE_AddAnimation(anim);
                category.SAFE_ResortAnimIDs();
                return anim;
            }

            return null;
        }

        public bool GotoAnimID(SplitAnimID id, bool scrollOnCenter, bool ignoreIfAlreadySelected, out DSAProj.Animation foundAnimRef, float startFrameOverride = -1, bool disableHkxSelect = false)
        {
            foundAnimRef = Proj.SAFE_GetFirstAnimationFromFullID(id);

            if (foundAnimRef != null)
            {
                if (!ignoreIfAlreadySelected || foundAnimRef != SelectedAnim)
                {
                    SelectNewAnimRef(foundAnimRef.ParentCategory, foundAnimRef, scrollOnCenter, doNotCommitToGraph: false, startFrameOverride, isPushCurrentToHistBackwardStack: true);
                    
                }
                return true;
            }



            foundAnimRef = null;
            return false;
        }


        public void ShowDialogEditCurrentAnimInfo()
        {
            if (SelectedAnim?.IS_DUMMY_ANIM == false)
                DialogManager.ShowTaeAnimPropertiesEditor(Proj, SelectedAnim);
        }
        
        // public void ShowDialogEditActionTrackProperties(int trackIndex, DSAProj.ActionTrack track)
        // {
        //     DialogManager.ShowTaeActionTrackPropertiesEditor(SelectedTaeAnim, trackIndex, track);
        // }

        public void ShowDialogEditRootTaeProperties()
        {
            DialogManager.ShowRootTaePropertiesEditor(Proj);
        }

       

        private void WinFormsMenuStrip_MenuDeactivate(object sender, EventArgs e)
        {
            //PauseUpdate = false;
        }

        private void WinFormsMenuStrip_MenuActivate(object sender, EventArgs e)
        {
            PauseUpdate = true;
            Input.CursorType = MouseCursorType.Arrow;
        }


        private string TryAnimLoadFallback(string fileContainerName)
        {
            string possibleFallbackPath = fileContainerName.ToLower().EndsWith(".dcx") ? (fileContainerName.Substring(0, fileContainerName.Length - "0.anibnd.dcx".Length) + "0.anibnd.dcx")
                        : (fileContainerName.Substring(0, fileContainerName.Length - "0.anibnd".Length) + "0.anibnd");

            var anibndDirectory = Path.GetDirectoryName(fileContainerName);
            var anibndShort = Utils.GetShortIngameFileName(fileContainerName);
            var anibndPathJustFile = Path.GetFileName(fileContainerName);
            var baseShort = Utils.GetShortIngameFileName(possibleFallbackPath);
            var basePathJustFile = Path.GetFileName(possibleFallbackPath);

            // TODO: Check EBLs for base anibnd and ask "would you like to unpack the found base ANIBND and open it for editing?"

            if (anibndShort.StartsWith("c0000_") && File.Exists($"{anibndDirectory}\\c0000.anibnd.dcx")
                && TaeFileContainer.AnibndContainsTae($"{anibndDirectory}\\c0000.anibnd.dcx"))
            {
                var askResult = System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                    $"have any TimeAct (.TAE) data inside of it, just animations.\n" +
                    $"It's a supplemental file which the base anibnd, 'c0000.anibnd.dcx', pulls animations from.\n" +
                    $"Would you like to load that base anibnd for editing instead?", "Load Base ANIBND?", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (askResult == System.Windows.Forms.DialogResult.Yes)
                {
                    fileContainerName = $"{anibndDirectory}\\c0000.anibnd.dcx";
                    return fileContainerName;
                }
                else if (askResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return null;
                }
            }
            else if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6 &&
                System.Text.RegularExpressions.Regex.IsMatch(anibndPathJustFile, @"c\d\d\d\d_div\d\d.anibnd.dcx$")
                && File.Exists($"{anibndDirectory}\\{anibndShort.Substring(0, 5)}.anibnd.dcx")
                && TaeFileContainer.AnibndContainsTae($"{anibndDirectory}\\{anibndShort.Substring(0, 5)}.anibnd.dcx"))
            {
                var askResult = System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                    $"have any TimeAct (.TAE) data inside of it, just animations.\n" +
                    $"It's a supplemental file which the base anibnd, '{anibndShort.Substring(0, 5)}.anibnd.dcx', pulls animations from.\n" +
                    $"Would you like to load that base anibnd for editing instead?", "Load Base ANIBND?", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (askResult == System.Windows.Forms.DialogResult.Yes)
                {
                    fileContainerName = $"{anibndDirectory}\\{anibndShort.Substring(0, 5)}.anibnd.dcx";
                    return fileContainerName;
                }
                else if (askResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return null;
                }
            }

            else if (File.Exists(possibleFallbackPath) && TaeFileContainer.AnibndContainsTae(possibleFallbackPath))
            {
                var askResult = System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                    $"have any TimeAct (.TAE) data inside of it. It appears to read from the base anibnd, '{basePathJustFile}', " +
                    $"which has TimeAct data. Would you like to load the base anibnd instead?", "Load Base ANIBND?", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (askResult == System.Windows.Forms.DialogResult.Yes)
                {
                    fileContainerName = possibleFallbackPath;
                    return fileContainerName;
                }
                else if (askResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return null;
                }
            }
            else
            {
                var gameDataAnibndPath = ParentDocument.GameData.FindFileInGameData(fileContainerName);
                string possibleGameDataFallbackPath = (gameDataAnibndPath.Substring(0, gameDataAnibndPath.Length - "0.anibnd.dcx".Length) + "0.anibnd.dcx");
                string possibleGameDataFallbackPathJustFile = Path.GetFileName(possibleGameDataFallbackPath);
                if (gameDataAnibndPath != null)
                {
                    if (anibndShort.StartsWith("c0000_") && ParentDocument.GameData.FileExists("chr/c0000.anibnd.dcx")
                        && TaeFileContainer.AnibndContainsTae(ParentDocument.GameData.ReadFile("chr/c0000.anibnd.dcx")))
                    {
                        var askResult = System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                        $"have any TimeAct (.TAE) data inside of it, just animations.\n" +
                        $"It's a supplemental file which the base anibnd, 'c0000.anibnd.dcx', pulls animations from.\n" +
                        $"Would you like to unpack that base anibnd into the project folder and open it for editing instead?", "Load Base ANIBND?", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                        if (askResult == System.Windows.Forms.DialogResult.Yes)
                        {
                            fileContainerName = $"{anibndDirectory}\\c0000.anibnd.dcx";
                            File.WriteAllBytes(fileContainerName, ParentDocument.GameData.ReadFile("chr/c0000.anibnd.dcx"));
                            return fileContainerName;
                        }
                        else if (askResult == System.Windows.Forms.DialogResult.Cancel)
                        {
                            return null;
                        }
                    }
                    else if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6 &&
                        System.Text.RegularExpressions.Regex.IsMatch(anibndPathJustFile, @"c\d\d\d\d_div\d\d.anibnd.dcx$")
                        && ParentDocument.GameData.FileExists($"chr/{anibndShort.Substring(0, 5)}.anibnd.dcx")
                        && TaeFileContainer.AnibndContainsTae(ParentDocument.GameData.ReadFile($"chr/{anibndShort.Substring(0, 5)}.anibnd.dcx")))
                    {
                        var askResult = System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                            $"have any TimeAct (.TAE) data inside of it, just animations.\n" +
                            $"It's a supplemental file which the base anibnd, '{anibndShort.Substring(0, 5)}.anibnd.dcx', pulls animations from.\n" +
                            $"Would you like to unpack that base anibnd into the project folder and open it for editing instead?", "Load Base ANIBND?", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                        if (askResult == System.Windows.Forms.DialogResult.Yes)
                        {
                            fileContainerName = $"{anibndDirectory}\\{anibndShort.Substring(0, 5)}.anibnd.dcx";
                            File.WriteAllBytes(fileContainerName, ParentDocument.GameData.ReadFile($"chr/{anibndShort.Substring(0, 5)}.anibnd.dcx"));
                            return fileContainerName;
                        }
                        else if (askResult == System.Windows.Forms.DialogResult.Cancel)
                        {
                            return null;
                        }
                    }
                    else if (ParentDocument.GameData.FileExists(possibleGameDataFallbackPath) && TaeFileContainer.AnibndContainsTae(ParentDocument.GameData.ReadFile(possibleGameDataFallbackPath)))
                    {
                        var askResult = System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                            $"have any TimeAct (.TAE) data inside of it. It appears to read from the base anibnd, '{possibleGameDataFallbackPathJustFile}', " +
                            $"which has TimeAct data. Would you like to unpack that base anibnd into the project folder and open it for editing instead?", "Load Base ANIBND?", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                        if (askResult == System.Windows.Forms.DialogResult.Yes)
                        {
                            fileContainerName = possibleFallbackPath;
                            File.WriteAllBytes(fileContainerName, ParentDocument.GameData.ReadFile(possibleGameDataFallbackPath));
                            return fileContainerName;
                        }
                        else if (askResult == System.Windows.Forms.DialogResult.Cancel)
                        {
                            return null;
                        }
                    }



                }

                System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                    $"have any TimeAct (.TAE) data inside of it. Unable to find any base ANIBND that the game reads TimeAct " +
                    $"data from in combination with this file. Aborting load operation.", "Invalid ANIBND", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return null;
            }

            return fileContainerName;
        }

        public static int ShowBinderFilePicker(string binderFilePath, Func<BinderFile, bool> predicate)
        {
            var binder = Utils.ReadBinder(binderFilePath);
            var matchedFiles = binder.Files.Where(x => predicate(x)).ToList();
            if (matchedFiles.Count == 0)
                return -1;
            var matchedFilesStrings = matchedFiles.Select(x => $"[{x.ID}] {(x.Name ?? "<No Name>")}").ToList();
            var picker = new TaeLoadFromArchivesFilePicker();
            picker.Text = "Select File In Binder";
            picker.InitEblFileList(matchedFilesStrings, null, null);
            picker.SelectedEblFileIndex = 0;
            var result = picker.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (picker.SelectedEblFileIndex < 0 || picker.SelectedEblFileIndex >= matchedFilesStrings.Count)
                    return -1;
                else
                    return matchedFiles[picker.SelectedEblFileIndex].ID;

            }

            return -1;
        }

        public void File_Open()
        {
            ParentDocument.SoundManager.StopAllSounds();
            ParentDocument.RumbleCamManager.ClearActive();
            //if (FileContainer != null && !IsReadOnlyFileMode && FileContainer.AllTAE.Any(x => x.Animations.Any(a => a.GetIsModified())))
            //{
            //    var yesNoCancel = System.Windows.Forms.MessageBox.Show(
            //        $"File \"{System.IO.Path.GetFileName(NewFileContainerName)}\" has " +
            //        $"unsaved changes. Would you like to save these changes before " +
            //        $"loading a new file?", "Save Unsaved Changes?",
            //        System.Windows.Forms.MessageBoxButtons.YesNoCancel,
            //        System.Windows.Forms.MessageBoxIcon.None);

            //    if (yesNoCancel == System.Windows.Forms.DialogResult.Yes)
            //    {
            //        SaveCurrentFile();
            //    }
            //    else if (yesNoCancel == System.Windows.Forms.DialogResult.Cancel)
            //    {
            //        return;
            //    }
            //    //If they chose no, continue as normal.
            //}

            var browseDlg = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = TaeFileContainer.DefaultSaveFilter,
                ValidateNames = true,
                CheckFileExists = true,
                CheckPathExists = true,
                //ShowReadOnly = true,
            };

            if (System.IO.File.Exists(NewFileContainerName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(NewFileContainerName);
                browseDlg.FileName = System.IO.Path.GetFileName(NewFileContainerName);
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                

                IsReadOnlyFileMode = browseDlg.ReadOnlyChecked;
                string fileContainerName = browseDlg.FileName;

                DSAProj.TaeContainerInfo container = null;

                var check = fileContainerName.ToLower();

                bool failedFileInBinderPicker = false;
                
                if (check.EndsWith(".anibnd.dcx") || check.EndsWith(".anibnd"))
                {
                    
                    var chrbndName = ParentDocument.GameData.ShowPickInsideBndPath("/chr/", @".*\/c\d\d\d\d.chrbnd.dcx$", $"/chr/{Utils.GetShortIngameFileName(check)}", 
                        $"Choose Character Model for '{Utils.GetShortIngameFileName(check)}.anibnd.dcx'", $"/chr/{Utils.GetShortIngameFileName(check)}.chrbnd.dcx");
                    container = new DSAProj.TaeContainerInfo.ContainerAnibnd(fileContainerName, chrbndName);
                    
                    bool containsNoTae = !TaeFileContainer.AnibndContainsTae(fileContainerName);
                
                    if (containsNoTae)
                    {
                        if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.None)
                        {
                            if (!ParentDocument.GameRoot.InitializeFromBND(fileContainerName))
                            {
                                Main.REQUEST_REINIT_EDITOR = true;
                                return;
                            }
                        }
                        var fallback = TryAnimLoadFallback(fileContainerName);
                        if (fallback == null)
                        {
                            Main.REQUEST_REINIT_EDITOR = true;
                            return;
                        }
                        else
                        {
                            fileContainerName = fallback;
                        }
                    }
                }
                else if (check.EndsWith(".partsbnd.dcx") || check.EndsWith(".partsbnd"))
                {
                    var bindFileID = ShowBinderFilePicker(fileContainerName, f => f.ID >= 400 && f.ID < 500 && TaeFileContainer.AnibndContainsTae(f.Bytes));
                    if (bindFileID >= 0)
                        container = new DSAProj.TaeContainerInfo.ContainerAnibndInBinder(fileContainerName, bindFileID);
                    else
                        failedFileInBinderPicker = true;
                }
                else if (check.EndsWith(".objbnd.dcx") || check.EndsWith(".objbnd"))
                {
                    var bindFileID = ShowBinderFilePicker(fileContainerName, f => f.ID >= 400 && f.ID < 500 && TaeFileContainer.AnibndContainsTae(f.Bytes));
                    if (bindFileID >= 0)
                        container = new DSAProj.TaeContainerInfo.ContainerAnibndInBinder(fileContainerName, bindFileID);
                    else
                        failedFileInBinderPicker = true;
                }

                //FileContainerName_Model = null;

                if (failedFileInBinderPicker)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Selected binder file had no ANIBND files containing TAE within it. " +
                        "Cancelling load operation.", "Invalid File",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Stop);
                    Main.REQUEST_REINIT_EDITOR = true;
                    return;
                }

                bool isCancel = false;

                zzz_DocumentManager.RequestFileOpenRecent = true;
                zzz_DocumentManager.RequestFileOpenRecent_SelectedFile = container;

                //if (!isCancel)
                //{
                //    ParentDocument.LoadingTaskMan.DoLoadingTask("File_Open", "Loading game assets...", progress =>
                //    {
                //        var loadFileResult = NewLoadFile(container);
                //        if (loadFileResult == false || !FileContainer.AllTAE.Any())
                //        {
                //            fileContainerName = "";
                //            System.Windows.Forms.MessageBox.Show(
                //                "Selected file had no TAE files within it. " +
                //                "Cancelling load operation.", "Invalid File",
                //                System.Windows.Forms.MessageBoxButtons.OK,
                //                System.Windows.Forms.MessageBoxIcon.Stop);
                //        }
                //        else if (loadFileResult == null)
                //        {
                //            fileContainerName = "";
                //            System.Windows.Forms.MessageBox.Show(
                //                "Selected file did not exist (how did you " +
                //                "get this message to appear, anyways?).", "File Does Not Exist",
                //                System.Windows.Forms.MessageBoxButtons.OK,
                //                System.Windows.Forms.MessageBoxIcon.Stop);
                //        }
                //    }, disableProgressBarByDefault: true);
                //}

               

               
            }
            else
            {
                // When you decide to cancel opening something
                ParentDocument.RequestClose_ForceDelete = true;
                Main.REQUEST_REINIT_EDITOR = true;
            }
        }

        private string BrowseForXMLTemplateLoop()
        {
            string selectedTemplate = null;
            do
            {
                var templateOption = System.Windows.Forms.MessageBox.Show("Select an XML template file to open.", "Open Template", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Information);
                if (templateOption == System.Windows.Forms.DialogResult.Cancel)
                {
                    return null;
                }
                else
                {
                    selectedTemplate = BrowseForXMLTemplate();
                }
            }
            while (selectedTemplate == null);

            return selectedTemplate;
        }

        private string BrowseForXMLTemplate()
        {
            var browseDlg = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Open TAE Template XML File",
                Filter = "TAE Templates (*.XML)|*.XML",
                ValidateNames = true,
                CheckFileExists = true,
                CheckPathExists = true,
            };

            browseDlg.InitialDirectory = System.IO.Path.Combine(new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName, "Res");

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsReadOnlyFileMode = browseDlg.ReadOnlyChecked;
                return browseDlg.FileName;
            }

            return null;
        }

        public void BrowseForMoreTextures()
        {
            List<string> texturesToLoad = new List<string>();
            lock (ParentDocument.Scene._lock_ModelLoad_Draw)
            {
                foreach (var m in ParentDocument.Scene.Models)
                {
                    texturesToLoad.AddRange(m.MainMesh.GetAllTexNamesToLoad());
                }
            }


            var browseDlg = new System.Windows.Forms.OpenFileDialog()
            {
                //Filter = "TPF[.DCX]/TFPBHD|*.TPF*",
                Multiselect = true,
                Title = "Load textures from CHRBND(s), TEXBND(s), TPF(s), and/or TPFBHD(s)..."
            };
            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var texFileNames = browseDlg.FileNames;
                List<string> bxfDupeCheck = new List<string>();
                ParentDocument.LoadingTaskMan.DoLoadingTask("BrowseForEntityTextures", "Scanning files for relevant textures...", progress =>
                {
                    double i = 0;
                    foreach (var tfn in texFileNames)
                    {
                        var shortName = Utils.GetShortIngameFileName(tfn);
                        if (!bxfDupeCheck.Contains(shortName))
                        {
                            if (TPF.Is(tfn))
                            {
                                ParentDocument.TexturePool.AddTpfFromPath(tfn);
                            }
                            else
                            {
                                ParentDocument.TexturePool.AddSpecificTexturesFromBinder(tfn, texturesToLoad);
                            }

                            bxfDupeCheck.Add(shortName);
                        }
                        progress.Report(++i / texFileNames.Length);
                    }
                    ParentDocument.Scene.RequestTextureLoad();
                    progress.Report(1);
                });


            }


        }


        //public void File_SaveAs()
        //{
        //    var browseDlg = new System.Windows.Forms.SaveFileDialog()
        //    {
        //        Filter = FileContainer?.GetResaveFilter()
        //                   ?? TaeFileContainer.DefaultSaveFilter,
        //        ValidateNames = true,
        //        CheckFileExists = false,
        //        CheckPathExists = true,
        //    };

        //    if (System.IO.File.Exists(NewFileContainerName))
        //    {
        //        browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(NewFileContainerName);
        //        browseDlg.FileName = System.IO.Path.GetFileName(NewFileContainerName);
        //    }

        //    if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        FileContainer.Info.Set
        //        FileContainerName = browseDlg.FileName;
        //        SaveCurrentFile();
        //    }
        //}

        public void ChangeTypeOfEvent(DSAProj.Action ev)
        {
            if (ev == null)
                return;

            var x = new TaeInspectorFormChangeEventType();
            Task.Run(() =>
            {
                PauseUpdate = true;

                var changeTypeDlg = new TaeInspectorFormChangeEventType();
                changeTypeDlg.ProjRef = Proj;
                changeTypeDlg.AnimCategoryRef = SelectedAnimCategory;
                changeTypeDlg.CurrentTemplate = ev.Template;
                changeTypeDlg.NewEventType = ev.Type;
                var dialogResult = System.Windows.Forms.DialogResult.Cancel;
                GameWindowAsForm.Invoke(new Action(() =>
                {
                    dialogResult = changeTypeDlg.ShowDialog(GameWindowAsForm);
                }));
                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    if (changeTypeDlg.NewEventType != ev.Type)
                    {
                        var ref_event = ev;

                        var ref_eventType = ev.Type;
                        var ref_eventTemplate = ev.Template;
                        if (ref_event.Parameters != null)
                            ref_event.NewSaveParamsToBytes();
                        var ref_eventParams = ev.ParameterBytes.ToArray();


                        CurrentAnimUndoMan.NewAction(
                            doAction: () =>
                            {
                                ref_event.LazySwitchEventTemplate(Proj.Template[changeTypeDlg.NewEventType]);


                                SelectedAnim.SAFE_SetIsModified(!IsReadOnlyFileMode);
                                SelectedAnimCategory.SAFE_SetIsModified(!IsReadOnlyFileMode);
                            },
                            undoAction: () =>
                            {
                                ref_event.Type = ref_eventType;
                                ref_event.Template = ref_eventTemplate;
                                ref_event.ParameterBytes = ref_eventParams.ToArray();
                                ref_event.RequestUpdateText();

                                SelectedAnim.SAFE_SetIsModified(!IsReadOnlyFileMode);
                                SelectedAnimCategory.SAFE_SetIsModified(!IsReadOnlyFileMode);
                            }, "Change type of event");
                    }
                }


                PauseUpdate = false;
            });

        }

        private (long Upper, long Lower) GetSplitAnimID(long id)
        {
            return ((ParentDocument.GameRoot.GameTypeHasLongAnimIDs) 
                ? (id / 1000000) : (id / 10000),
                (ParentDocument.GameRoot.GameTypeHasLongAnimIDs) 
                ? (id % 1000000) : (id % 10000));
        }

        private string HKXNameFromCompositeID(long compositeID)
        {
            if (compositeID < 0)
                return "<NONE>";

            var splitID = GetSplitAnimID(compositeID);

            if (ParentDocument.GameRoot.CurrentAnimIDFormatType == zzz_GameRootIns.AnimIDFormattingType.aXXX_YYYYYY)
            {
                return $"a{splitID.Upper:D3}_{splitID.Lower:D6}";
            }
            else if (ParentDocument.GameRoot.CurrentAnimIDFormatType == zzz_GameRootIns.AnimIDFormattingType.aXX_YY_ZZZZ)
            {
                string s = $"a{splitID.Upper:D3}_{splitID.Lower:D6}";
                return s.Insert(s.Length - 4, "_");
            }
            else
            {
                return $"a{splitID.Upper:D2}_{splitID.Lower:D4}";
            }
        }

        private string HKXSubIDDispNameFromInt(long subID)
        {
            if (FileContainer.AllTAE.Count() == 1)
            {
                return HKXNameFromCompositeID(subID);
            }
            else
            {
                if (ParentDocument.GameRoot.CurrentAnimIDFormatType == zzz_GameRootIns.AnimIDFormattingType.aXX_YY_ZZZZ)
                {
                    return $"aXXX_{subID:D6}";
                }
                else if (ParentDocument.GameRoot.CurrentAnimIDFormatType == zzz_GameRootIns.AnimIDFormattingType.aXX_YY_ZZZZ)
                {
                    string s = $"aXX_{subID:D6}";
                    return s.Insert(s.Length - 4, "_");
                }
                else
                {
                    return $"aXX_{subID:D4}";
                }
            }
            
        }

        public void UpdateSelectedTaeAnimInfoText()
        {
            var stringBuilder = new StringBuilder();

            if (SelectedAnim == null || SelectedAnim.IS_DUMMY_ANIM)
            {
                stringBuilder.Append("(No Animation Selected)");
                TaeAnimInfoIsClone = false;
            }
            else
            {
                stringBuilder.Append($"{SelectedAnim.SplitID.GetFormattedIDString(Proj)}");
                SelectedAnim.SafeAccessHeader(header =>
                {
                    if (header is TAE.Animation.AnimFileHeader.Standard asStandard)
                    {
                        stringBuilder.Append($" [Original Anim Entry]");

                        if (asStandard.ImportsHKX && asStandard.ImportHKXSourceAnimID >= 0)
                            stringBuilder.Append($", Override HKX: {HKXNameFromCompositeID(asStandard.ImportHKXSourceAnimID)}.hkx");

                        if (asStandard.AllowDelayLoad)
                            stringBuilder.Append(", DelayLoad Allowed");

                        if (asStandard.IsLoopByDefault)
                            stringBuilder.Append($", Loops By Default");

                        TaeAnimInfoIsClone = false;
                    }
                    else if (header is TAE.Animation.AnimFileHeader.ImportOtherAnim asImportOtherAnim)
                    {
                        stringBuilder.Append($" [CLONE OF {HKXNameFromCompositeID(asImportOtherAnim.ImportFromAnimID)}]");

                        TaeAnimInfoIsClone = true;
                    }
                });
                

                //SFTODO

                //if (SelectedTaeAnim.AnimFileReference)
                //{
                //    stringBuilder.Append($" [RefID: {SelectedTaeAnim.RefAnimID}]");
                //}
                //else
                //{
                //    stringBuilder.Append($" [\"{SelectedTaeAnim.Unknown1}\"]");
                //    stringBuilder.Append($" [\"{SelectedTaeAnim.Unknown2}\"]");

                //    //if (SelectedTaeAnim.IsLoopingObjAnim)
                //    //    stringBuilder.Append($" [ObjLoop]");

                //    //if (SelectedTaeAnim.UseHKXOnly)
                //    //    stringBuilder.Append($" [HKXOnly]");

                //    //if (SelectedTaeAnim.TAEDataOnly)
                //    //    stringBuilder.Append($" [TAEOnly]");

                //    //if (SelectedTaeAnim.OriginalAnimID >= 0)
                //    //    stringBuilder.Append($" [OrigID: {SelectedTaeAnim.OriginalAnimID}]");
                //}
            }

            SelectedTaeAnimInfoText = stringBuilder.ToString();
        }

        public void SelectAction(DSAProj.Action act)
        {
            NewSelectedActions.Clear();
            NewSelectedActions.Add(act);
            Graph.LayoutManager.ScrollToAction(act);
        }

        public void CommitActiveGraphToTaeStruct()
        {
            //Graph.EventBoxes = Graph.EventBoxes.OrderBy(evBox => evBox.MyEvent.StartTime + (evBox.Row * 1000)).ToList();

            //SelectedTaeAnim.Events = Graph.EventBoxes
            //    .Select(evBox => evBox.MyEvent)
            //    .ToList();

            //Graph?.GenerateFakeDS3EventGroups(threadLock: true);
            Graph?.WriteToAnimRef(SelectedAnim);
        }

        public enum InsertAnimType
        {
            None,
            Before,
            After,
        }
        public void InsertNewAnimAtIndex(DSAProj.AnimCategory fromTae, DSAProj.Animation fromAnim, InsertAnimType type)
        {
            //var currentAnim = SelectedTaeAnim;
            //var currentAnimID = SelectedTae.GetFullAnimationID(SelectedTaeAnim);

            

            var id = fromAnim.SplitID;
            var origID = id;
            var newSubID = id.SubID;

            if (type == InsertAnimType.Before)
            {
                newSubID--;
            }
            else if (type == InsertAnimType.After)
            {
                newSubID++;
            }

            if (newSubID < 0)
            {
                newSubID = 0;
            }

            int subIDMax = (ParentDocument.GameRoot.GameTypeHasLongAnimIDs ? 999999 : 9999);
            if (newSubID > subIDMax)
            {
                newSubID = subIDMax;
            }

            if (fromTae.SAFE_AnimExists_ByFullID(SplitAnimID.FromFullID(Proj,
                    (new SplitAnimID() { CategoryID = id.CategoryID, SubID = newSubID }).GetFullID(Proj))))
                newSubID = id.SubID;

            GlobalUndoMan.NewAction_AnimCategory(() =>
            {
                var newAnimRef = new DSAProj.Animation(Proj, fromTae);
                //newAnimRef.ParentAnimCategory = fromTae;
                newAnimRef.SAFE_SetHeader(new TAE.Animation.AnimFileHeader.Standard()
                {
                    AnimFileName = "New Anim Entry",
                    ImportsHKX = false,
                    ImportHKXSourceAnimID = -1,
                    IsNullHeader = false,
                });

                newAnimRef.SplitID = new SplitAnimID() { CategoryID = id.CategoryID, SubID = newSubID };

                int index = fromTae.SAFE_GetAnimIndexInList(fromAnim);
                if (type == InsertAnimType.After)
                    index++;

                //if (index < fromTae.Animations.Count)
                //    fromTae.Animations.Insert(index, newAnimRef);
                //else // at very end.
                //    fromTae.Animations.Add(newAnimRef);

                fromTae.SAFE_AddAnimation(newAnimRef, index);

                //ImmediateImportAnim(newAnimRef, currentAnimID);

                RecreateAnimList();
                SelectNewAnimRef(fromTae, newAnimRef);
            }, actionDescription: "Quick Add animation");

           
        }

        public void SelectNewAnimRef(DSAProj.AnimCategory category, DSAProj.Animation animRef, bool scrollOnCenter = false, 
            bool doNotCommitToGraph = false, float startFrameOverride = -1, bool isPushCurrentToHistBackwardStack = false,
            bool disableHkxSelect = false)
        {
            AnimSwitchRenderCooldown = 1;

            if (isPushCurrentToHistBackwardStack && animRef != null)
            {
                bool isDuplicate = false;
                AnimViewHistoryEntry mostRecentCheck = null;
                if (AnimViewBackwardStack.Count > 0)
                {
                    var peek = AnimViewBackwardStack.Peek();
                    if (peek.Anim == SelectedAnim)
                    {
                        isDuplicate = true;
                        isPushCurrentToHistBackwardStack = false;
                    }
                }
                if (!isDuplicate)
                {
                    AnimViewForwardStack.Clear();
                    AnimViewBackwardStack.Push(GetAnimHistoryEntryFromCurrent());
                }
            }

            if (!doNotCommitToGraph)
                CommitActiveGraphToTaeStruct();

            bool isBlend = (Graph != null && PlaybackCursor != null) && ((PlaybackCursor?.IsPlaying == true || Graph.ViewportInteractor.NewIsComboActive) && 
                Graph.ViewportInteractor.IsBlendingActive &&
                Graph.ViewportInteractor.EntityType != TaeViewportInteractor.TaeEntityType.REMO);

            if (Graph != null && Graph.ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
            {
                if (PlaybackCursor != null)
                    PlaybackCursor.CurrentTime = 0;
            }

            
            
            if (PlaybackCursor != null)
                PlaybackCursor.IsStepping = false;

            SelectedAnimCategory = category;

            var isNewAnimRef = SelectedAnim != animRef;
            
            

            var prevAnim = SelectedAnim;
            SelectedAnim = animRef;

            prevAnim?.SAFE_UnloadStringsToSaveMemory();

            UpdateSelectedTaeAnimInfoText();

            if (SelectedAnim != null)
            {
                if (isNewAnimRef)
                {
                    animRef?.SAFE_CheckActionInitialize(Proj.Template);
                    NewSelectedActions.Clear();
                }

                //bool wasFirstAnimSelected = false;

                //if (Graph == null)
                //{
                //    wasFirstAnimSelected = true;
                //    Graph = new TaeEditAnimEventGraph(this, false, SelectedTaeAnim);
                //}



                LoadAnimIntoGraph(SelectedAnimCategory, SelectedAnim);

                if (isNewAnimRef && Main.Config.ResetScrollWhenChangingAnimations)
                {
                    if (Graph != null)
                    {
                        Graph.ScrollViewer.Scroll = Vector2.Zero;
                    }
                }

                if (HasntSelectedAnAnimYetAfterBuildingAnimList)
                {
                    // Unused
                    HasntSelectedAnAnimYetAfterBuildingAnimList = false;
                }

                ParentDocument.SpWindowAnimations.ScrollToAnimRef(SelectedAnimCategory, SelectedAnim, scrollOnCenter);

                ParentDocument.LoadingTaskMan.DoLoadingTask("TaeEditorScreen_WaitForModelLoad", "Loading animation...", prog =>
                {
                    while (Graph?.ViewportInteractor?.IS_STILL_LOADING != false)
                    {
                        System.Threading.Thread.Sleep(20);
                        if (Main.REQUEST_REINIT_EDITOR)
                            break;
                    }

                    if (Main.REQUEST_REINIT_EDITOR)
                        return;

                    Graph.PlaybackCursor.ResetAll();

                    if (startFrameOverride >= 0)
                    {
                        float overrideStartTime = (float)(startFrameOverride * PlaybackCursor.CurrentSnapInterval);
                        var selAnim = SelectedAnim != null ? Proj?.SAFE_SolveAnimRefChain(SelectedAnim.SplitID) : null;
                        Graph.ViewportInteractor.OnNewAnimSelected(overrideStartTime, disableHkxSelect, selAnim ?? SelectedAnim);

                        PlaybackCursor.CurrentTime = PlaybackCursor.StartTime = overrideStartTime;
                        PlaybackCursor.IgnoreCurrentRelativeScrub();
                        //Graph.ViewportInteractor.OnScrubFrameChange(overrideStartTime, doNotScrubBackgroundLayers: true);
                        Graph.ViewportInteractor.NewScrub();
                    }
                    else
                    {
                        var selAnim = SelectedAnim != null ? Proj?.SAFE_SolveAnimRefChain(SelectedAnim.SplitID) : null;

                        Graph.ViewportInteractor.OnNewAnimSelected(0, disableHkxSelect, selAnim ?? SelectedAnim);
                        Graph.PlaybackCursor.RestartFromBeginning();
                        Graph.ViewportInteractor.NewScrub();

                        if (!isBlend || (PlaybackCursor.IsPlaying && PlaybackCursor.IsTempPausedUntilAnimChange))
                        {
                            if (Graph?.ViewportInteractor?.NewIsComboActive != true)
                                ActualHardReset();
                        }

                        PlaybackCursor.IsTempPausedUntilAnimChange = false;
                    }

                    Graph.PlaybackCursor.IsTempPausedUntilAnimChange = false;
                    SelectNewAnimRef(SelectedAnimCategory, SelectedAnim, scrollOnCenter, doNotCommitToGraph, startFrameOverride, isPushCurrentToHistBackwardStack: false, disableHkxSelect);
                    //Scene.UpdateAnimation();
                }, disableProgressBarByDefault: true);
            }
            else
            {
                NewSelectedActions.Clear();

                Graph = null;
            }
        }

        public void ShowDialogChangeAnimName(DSAProj proj, DSAProj.AnimCategory tae, DSAProj.Animation anim)
        {
            if (anim != null && !anim.IS_DUMMY_ANIM)
            {

                string animID = anim.SplitID.GetFormattedIDString(proj);
                DialogManager.AskForInputString("Set Animation Name", $"Set the display name of animation {animID}.", "", result =>
                {
                    if (string.IsNullOrWhiteSpace(result))
                        result = null;

                    if (anim.Info.DisplayName != result)
                    {
                        anim.Info.DisplayName = result;
                        anim.SAFE_SetIsModified(true);
                    }
                }, checkError: null, canBeCancelled: true, startingText: anim.Info.DisplayName ?? "");
            }
        }

        public void ShowDialogDuplicateToNewTaeSection(DSAProj proj, DSAProj.AnimCategory category)
        {
            if (proj == null || category == null)
                return;
            
            if (Graph.ViewportInteractor.EntityType != TaeViewportInteractor.TaeEntityType.PC)
            {
                // NPC
            }
            else
            {
                
            }
            
            if (proj != null && category != null)
            {
                DialogManager.ShowDialogAnimCategoryDuplicate(proj, category, result =>
                {
                    int fromCategoryID = category.CategoryID;
                    int toCategoryID = result.SelectedAnimCategoryID;

                    if (proj.SAFE_CategoryExists(toCategoryID) && !result.UserConfirmedTheyAreOkWithIDConflict)
                        return;
                    
                    
                    NextAnim(false, true);
                    
                    GlobalUndoMan.NewAction(() =>
                    {
                        var clonedCategory = category.SAFE_GetClone();
                        clonedCategory.CategoryID = result.SelectedAnimCategoryID;
                        if (!string.IsNullOrWhiteSpace(clonedCategory.Info.DisplayName))
                            clonedCategory.Info.DisplayName += " (copy)";

                        var anims = clonedCategory.SAFE_GetAnimations();



                        foreach (var anim in anims)
                        {
                            anim.SafeAccessHeader(header =>
                            {
                                if (header is SoulsAssetPipeline.Animation.TAE.Animation.AnimFileHeader.ImportOtherAnim asImportOtherAnim)
                                {
                                    if (asImportOtherAnim.ImportFromAnimID >= 0)
                                    {
                                        var impSection = asImportOtherAnim.ImportFromAnimID / ParentDocument.GameRoot.GameTypeUpperAnimIDModBy;
                                        if (impSection == category.CategoryID)
                                        {
                                            var upperID = clonedCategory.CategoryID * ParentDocument.GameRoot.GameTypeUpperAnimIDModBy;
                                            var lowerID = asImportOtherAnim.ImportFromAnimID % ParentDocument.GameRoot.GameTypeUpperAnimIDModBy;
                                            asImportOtherAnim.ImportFromAnimID = upperID + lowerID;
                                        }
                                    }
                                }
                                else if (header is TAE.Animation.AnimFileHeader.Standard asStandard)
                                {
                                    if (asStandard.ImportsHKX && asStandard.ImportHKXSourceAnimID >= 0)
                                    {

                                        // Reroute HKX references of original section to new section, if user has reference HKX of original turned off
                                        if (!result.ReferenceHkxOfOriginalCategory)
                                        {
                                            var impSection = asStandard.ImportHKXSourceAnimID / ParentDocument.GameRoot.GameTypeUpperAnimIDModBy;
                                            if (impSection == category.CategoryID)
                                            {
                                                var upperID = clonedCategory.CategoryID * ParentDocument.GameRoot.GameTypeUpperAnimIDModBy;
                                                var lowerID = asStandard.ImportHKXSourceAnimID % ParentDocument.GameRoot.GameTypeUpperAnimIDModBy;
                                                asStandard.ImportHKXSourceAnimID = upperID + lowerID;
                                            }
                                        }


                                    }
                                    else
                                    {

                                        // Turn regular HKX entries into references of original HKX
                                        if (result.ReferenceHkxOfOriginalCategory)
                                        {
                                            var upperID = (int)(category.CategoryID * ParentDocument.GameRoot.GameTypeUpperAnimIDModBy);
                                            var lowerID = anim.SplitID.SubID;// (int)(anim.NewID.GetFullID(Proj) % ParentDocument.GameRoot.GameTypeUpperAnimIDModBy);
                                            asStandard.ImportHKXSourceAnimID = upperID + lowerID;
                                            asStandard.ImportsHKX = true;
                                        }

                                    }

                                }
                            });

                            

                            anim.SplitID = new SplitAnimID() { CategoryID = clonedCategory.CategoryID, SubID = anim.SplitID.SubID };
                        }



                        //proj.AddOrOverwriteCategory(result.SelectedAnimCategoryID, clonedCategory, result.UserConfirmedTheyAreOkWithOverwriting);
                        proj.SAFE_AddAnimCategory(clonedCategory);
                        RecreateAnimList();

                        var firstAnim = clonedCategory.SAFE_GetFirstAnimInList();
                        if (firstAnim != null)
                        {
                            SelectNewAnimRef(clonedCategory, firstAnim, isPushCurrentToHistBackwardStack: true);
                        }
                    }, actionDescription: $"[Global] Cloned Anim Category {fromCategoryID} --> {toCategoryID}");
                    
                    
                });
            }
        }

        public void ShowDialogChangeAnimCategoryName(DSAProj.AnimCategory cat)
        {
            if (cat != null)
            {
                DialogManager.AskForInputString("Set Animation Category Name", 
                    $"Set the name of animation category {cat.CategoryID}. " +
                    $"Setting a name does not affect game data in any way, it is purely for your convenience.", 
                    "", result =>
                {
                    if (cat.Info.DisplayName != result)
                    {
                        cat.Info.DisplayName = result;
                        cat.SAFE_SetIsModified(true);
                        RecreateAnimList();
                    }
                }, checkError: null, canBeCancelled: true, startingText: cat.Info.DisplayName);
            }
        }

        public void NIGHTFALL_ToggleImport()
        {
            SelectedAnim.SafeAccessHeader(header =>
            {
                var s = header.AnimFileName;
                if (s.StartsWith("+"))
                {
                    while (s.StartsWith("+"))
                        s = s.TrimStart('+');
                }
                else
                {
                    s = "+" + s;
                }
                header.AnimFileName = s;
                SelectedAnim.INNER_SetIsModified(true);
            });
            
        }


        public void ShowDialogFind()
        {
            if (NewFileContainerName == null || SelectedAnimCategory == null)
                return;

            OSD.WindowFind.IsOpen = true;
            OSD.WindowFind.IsRequestFocus = true;
            //PauseUpdate = true;


            //var find = KeyboardInput.Show("Quick Find Event", "Finds the very first animation containing the event with the specified ID number or name (according to template).", "");
            //if (int.TryParse(find.Result, out int typeID))
            //{
            //    var gotoAnim = SelectedTae.Animations.Where(x => x.Events.Any(ev => (int)ev.Type == typeID));
            //    if (gotoAnim.Any())
            //        SelectNewAnimRef(SelectedTae, gotoAnim.First());
            //    else
            //        MessageBox.Show("None Found", "No events of that type found within the currently loaded files.", new[] { "OK" });
            //}
            //else 
            //{
            //    var gotoAnim = SelectedTae.Animations.Where(x => x.Events.Any(ev => ev.TypeName == find.Result));
            //    if (gotoAnim.Any())
            //        SelectNewAnimRef(SelectedTae, gotoAnim.First());
            //    else
            //        MessageBox.Show("None Found", "No events of that type found within the currently loaded files.", new[] { "OK" });
            //}



            //PauseUpdate = false;
        }

        public void ShowDialogGotoAnimSectionID()
        {
            if (FileContainer == null || SelectedAnimCategory == null)
                return;

            DialogManager.AskForInputString("Go To Anim Category ID", $"Enter the Anim Category ID (The X part of {ParentDocument.GameRoot.CurrentAnimIDFormatType})\n" +
                "to jump to the first animation in that category.",
                $"", result =>
                {
                    Main.WinForm.Invoke(new Action(() =>
                    {
                        if (int.TryParse(result.Replace("a", "").Replace("_", ""), out int id))
                        {
                            if (!GotoAnimSectionID(id, scrollOnCenter: true))
                            {
                                DialogManager.DialogOK("Goto Failed", $"Unable to find Anim Category {id}.");
                            }
                        }
                        else
                        {
                            DialogManager.DialogOK("Goto Failed", $"\"{result}\" is not a valid integer.");
                        }
                    }));
                    
                    
                }, checkError: input =>
                {
                    if (int.TryParse(input.Replace("a", "").Replace("_", ""), out int id))
                    {
                        if (!FileContainer.Proj.SAFE_CategoryExists(id))
                        {
                            return $"Animation category {id} does not exist.";
                        }
                    }
                    else
                    {
                        return $"\"{input}\" is not a valid integer.";
                    }

                    return null;
                }, canBeCancelled: true);
        }

        public void ShowDialogGotoAnimID()
        {
            if (FileContainer == null || SelectedAnimCategory == null)
                return;

            DialogManager.AskForInputString("Go To Animation ID", "Enter the animation ID to jump to.\n" +
                "Accepts the full string with prefix or just the ID as a number.",
                ParentDocument.GameRoot.CurrentAnimIDFormatType.ToString(), result =>
                {
                    Main.WinForm.Invoke(new Action(() =>
                    {
                        if (SplitAnimID.TryParse(Proj, result, out SplitAnimID id, out string detailedError))
                        {
                            if (!GotoAnimID(id, scrollOnCenter: true, ignoreIfAlreadySelected: false, out _))
                            {
                                zzz_NotificationManagerIns.PushNotification($"Go to animation failed: Unable to find animation with ID {id.GetFormattedIDString(Proj)}.");
                            }
                        }
                        else
                        {
                            zzz_NotificationManagerIns.PushNotification($"Go to animation failed: '{detailedError}'.");
                        }
                    }));
                }, checkError: input =>
                {
                    bool parseSuccess = SplitAnimID.TryParse(Proj, input, out SplitAnimID parsed, out string detailedError);
                    if (!parseSuccess)
                        return detailedError;

                    return null;
                }, canBeCancelled: true);
        }

        public bool ImmediateImportAnim(DSAProj.Animation importToAnim, SplitAnimID importFromID)
        {
            bool isInvalidHKX = false;
            var animRefToImportFrom = Proj.SAFE_GetFirstAnimationFromFullID(importFromID);

            void DoAnimRefThing(DSAProj.Animation anim, long animID)
            {
                anim.SafeAccessHeader(animHeader =>
                {
                    if (animHeader is TAE.Animation.AnimFileHeader.Standard asStandard)
                    {
                        var header = new TAE.Animation.AnimFileHeader.Standard();
                        header.ImportsHKX = true;

                        if (asStandard.ImportsHKX)
                        {
                            header.ImportHKXSourceAnimID = asStandard.ImportHKXSourceAnimID;
                            if (header.ImportHKXSourceAnimID == animID) // somehow lol
                            {
                                header.ImportHKXSourceAnimID = -1;
                                header.ImportsHKX = false;
                            }
                        }
                        else
                        {
                            header.ImportHKXSourceAnimID = (int)animID;
                        }

                        importToAnim.INNER_ClearActions();
                        importToAnim.INNER_ClearActionTracks();

                        anim.UnSafeAccessActions(actions =>
                        {
                            foreach (var act in actions)
                                importToAnim.INNER_AddAction(act.GetClone(readFromTemplate: true));
                        });

                        anim.UnSafeAccessActionTracks(actionTracks =>
                        {
                            foreach (var track in actionTracks)
                                importToAnim.INNER_AddActionTrack(track.GetClone());
                        });

                        //foreach (var ev in anim._actions)
                        //    importToAnim.Actions.Add(ev.GetClone(readFromTemplate: true));
                        //foreach (var evg in anim._actionTracks)
                        //    importToAnim.ActionTracks.Add(evg.GetClone());


                        // { origEventGroup, newEventGroup }
                        //var eventGroupMigrationMapping = new Dictionary<TAE.EventGroup, TAE.EventGroup>();

                        //foreach (var ev in anim.Events)
                        //    if (ev.Group != null && !eventGroupMigrationMapping.ContainsKey(ev.Group))
                        //        eventGroupMigrationMapping.Add(ev.Group, ev.Group.GetClone());

                        //foreach (var evg in anim.EventGroups)
                        //    if (!eventGroupMigrationMapping.ContainsKey(evg))
                        //        eventGroupMigrationMapping.Add(evg, evg.GetClone());

                        //foreach (var ev in anim.Events)
                        //{
                        //    var newEv = ev.GetClone();
                        //    newEv.Group = ev.Group;
                        //    if (ev.Group != null && eventGroupMigrationMapping.ContainsKey(ev.Group))
                        //    {
                        //        newEv.Group = eventGroupMigrationMapping[ev.Group];
                        //        if (!importToAnim.EventGroups.Contains(newEv.Group))
                        //            importToAnim.EventGroups.Add(newEv.Group);
                        //    }

                        //    importToAnim.Events.Add(newEv);
                        //}

                        importToAnim.INNER_SetHeader(header);
                        importToAnim.INNER_SetIsModified(true);

                    }
                    else if (animHeader is TAE.Animation.AnimFileHeader.ImportOtherAnim asImportOther)
                    {
                        if (asImportOther.ImportFromAnimID >= 0)
                        {
                            var referencedAnim = Proj.INNER_GetFirstAnimationFromFullID(SplitAnimID.FromFullID(Proj, asImportOther.ImportFromAnimID));

                            if (referencedAnim != null)
                            {
                                DoAnimRefThing(referencedAnim, asImportOther.ImportFromAnimID);
                            }
                            else
                            {

                                // BROKEN REFERENCE, GARBEGE

                                var header = new TAE.Animation.AnimFileHeader.Standard();
                                header.ImportsHKX = false;
                                header.ImportHKXSourceAnimID = -1;



                                importToAnim.INNER_ClearActionTracks();
                                importToAnim.INNER_ClearActionTracks();

                                anim.UnSafeAccessActions(actions =>
                                {
                                    foreach (var act in actions)
                                        importToAnim.INNER_AddAction(act.GetClone(readFromTemplate: true));
                                });

                                anim.UnSafeAccessActionTracks(actionTracks =>
                                {
                                    foreach (var track in actionTracks)
                                        importToAnim.INNER_AddActionTrack(track.GetClone());
                                });

                                //foreach (var ev in anim._actions)
                                //    importToAnim.Actions.Add(ev.GetClone(readFromTemplate: true));
                                //foreach (var evg in anim._actionTracks)
                                //    importToAnim.ActionTracks.Add(evg.GetClone());
                                importToAnim.INNER_SetHeader(header);
                                importToAnim.INNER_SetIsModified(true);

                                isInvalidHKX = true;
                            }
                        }
                    }
                });
                
            }

            DoAnimRefThing(animRefToImportFrom, importFromID.GetFullID(Proj));
            return !isInvalidHKX;
        }

        public void ShowDialogImportFromAnimID()
        {
            if (FileContainer == null || SelectedAnimCategory == null)
                return;

            if (SelectedAnim?.IS_DUMMY_ANIM != false)
                return;

            var currentAnimID = SelectedAnim.SplitID;
            
            DialogManager.AskForInputString("Import From Animation ID", "Enter the animation ID to import from. This will replace animation and all events with those of the specified animation.\n" +
                "Accepts the full string with prefix or just the ID as a number.",
                ParentDocument.GameRoot.CurrentAnimIDFormatType.ToString(), result =>
                {
                    Main.MainThreadLazyDispatch(new Action(() =>
                    {
                        if (SplitAnimID.TryParse(Proj, result, out SplitAnimID id, out string detailedError))
                        {
                            var animRefToImportFrom = Proj.SAFE_GetFirstAnimationFromFullID(id);

                            if (animRefToImportFrom == null)
                            {
                                DialogManager.DialogOK("Import Failed", $"Unable to find anim {id}.");
                            }
                            else if (id == currentAnimID)
                            {
                                DialogManager.DialogOK("Import Failed", $"Animation {id} is the current animation.");
                            }

                            
                            if (!ImmediateImportAnim(SelectedAnim, id))
                            {
                                DialogManager.DialogOK("Import Warning", $"Anim with ID {id} didn't resolve to a valid HKX so only event data was imported.");
                            }

                            SelectNewAnimRef(SelectedAnimCategory, SelectedAnim, doNotCommitToGraph: true, isPushCurrentToHistBackwardStack: true);
                            //HardReset();

                        }
                        else
                        {
                            DialogManager.DialogOK("Import Failed", $"\"{result}\" is not a valid animation ID.");
                        }
                    }));
                }, checkError: input =>
                {
                    bool parseSuccess = SplitAnimID.TryParse(Proj, input, out SplitAnimID parsed, out string detailedError);
                    if (!parseSuccess)
                        return detailedError;

                    var animRefToImportFrom = Proj.SAFE_GetFirstAnimationFromFullID(parsed);

                    if (animRefToImportFrom == null)
                    {
                        return $"Animation '{parsed}' does not exist.";
                    }
                    else if (parsed == currentAnimID)
                    {
                        return "Can not import from the same animation ID. Please choose another animation ID.";
                    }

                    return null;
                }, canBeCancelled: true);
        }

        public void NextAnim(bool shiftPressed, bool ctrlPressed)
        {
            try
            {
                if (SelectedAnimCategory != null)
                {
                    if (SelectedAnim != null)
                    {
                        var category = SelectedAnimCategory;
                        var anim = SelectedAnim;
                        void DoSmallStep()
                        {
                            anim = Proj.SAFE_GuiHelperSelectNextAnimation(anim);
                            if (anim == null)
                            {
                                DoBigStep();
                            }
                            else
                            {
                                //category = anim.ParentCategory;
                                category = Proj.SAFE_RegistCategory(anim.SplitID.CategoryID);
                            }
                            
                        }

                        void DoBigStep()
                        {
                            category = Proj.SAFE_GuiHelperSelectNextCategory(category);
                            anim = category.SAFE_GetFirstAnimInList();
                        }

                        void DoStep()
                        {
                            if (ctrlPressed)
                                DoBigStep();
                            else
                                DoSmallStep();
                        }

                        if (shiftPressed)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                DoStep();
                            }
                        }
                        else
                        {
                            DoStep();
                        }

                        try
                        {
                            SelectNewAnimRef(category, anim, scrollOnCenter: Input.ShiftHeld || Input.CtrlHeld, isPushCurrentToHistBackwardStack: true);
                        }
                        catch// (Exception innerEx)
                        {
                            //Console.WriteLine("FATCAT");
                        }
                        
                    }
                }
            }
            catch// (Exception ex)
            {
                Console.WriteLine("Fatcat");
            }
        }

        public void PrevAnim(bool shiftPressed, bool ctrlPressed)
        {
            try
            {
                if (SelectedAnimCategory != null)
                {
                    if (SelectedAnim != null)
                    {
                        var category = SelectedAnimCategory;
                        var anim = SelectedAnim;
                        void DoSmallStep()
                        {
                            anim = Proj.SAFE_GuiHelperSelectPrevAnimation(anim);
                            //category = anim.ParentCategory;
                            category = Proj.SAFE_RegistCategory(anim.SplitID.CategoryID);
                        }

                        void DoBigStep()
                        {
                            category = Proj.SAFE_GuiHelperSelectPrevCategory(category);
                            if (Main.Config.GoToFirstAnimInCategoryWhenChangingCategory)
                                anim = category.SAFE_GetFirstAnimInList(throwIfEmpty: true);
                            else
                                anim = category.SAFE_GetLastAnimInList(throwIfEmpty: true);
                        }

                        void DoStep()
                        {
                            if (ctrlPressed)
                                DoBigStep();
                            else
                                DoSmallStep();
                        }

                        if (shiftPressed)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                DoStep();
                            }
                        }
                        else
                        {
                            DoStep();
                        }

                        try
                        {
                            SelectNewAnimRef(category, anim, scrollOnCenter: Input.ShiftHeld || Input.CtrlHeld, isPushCurrentToHistBackwardStack: true);
                        }
                        catch// (Exception innerEx)
                        {
                            //Console.WriteLine("FATCAT");
                        }
                    }
                }
            }
            catch
            {

            }
        }

        public void TransportNextFrame()
        {
            if (PlaybackCursor.IsPlayingRemoFullPreview)
                return;

            PlaybackCursor.IsPlaying = false;
            PlaybackCursor.IsStepping = true;


            int nearestFrame = (int)Math.Round(PlaybackCursor.CurrentTime / PlaybackCursor.CurrentSnapInterval);
            double nearestFrameTime = nearestFrame * PlaybackCursor.CurrentSnapInterval;
            double deltaToSnapToNearestFrame = nearestFrameTime - PlaybackCursor.CurrentTime;
            PlaybackCursor.NewApplyRelativeScrub(deltaToSnapToNearestFrame);
            PlaybackCursor.NewApplyRelativeScrub(PlaybackCursor.CurrentSnapInterval);

            Graph.LayoutManager.ScrollToPlaybackCursor(-1, modTime: true, clampTime: true);
        }

        public void TransportPreviousFrame()
        {
            PlaybackCursor.IsPlaying = false;
            PlaybackCursor.IsStepping = true;
            int nearestFrame = (int)Math.Round(PlaybackCursor.CurrentTime / PlaybackCursor.CurrentSnapInterval);
            double nearestFrameTime = nearestFrame * PlaybackCursor.CurrentSnapInterval;
            double deltaToSnapToNearestFrame = nearestFrameTime - PlaybackCursor.CurrentTime;
            PlaybackCursor.NewApplyRelativeScrub(deltaToSnapToNearestFrame);
            PlaybackCursor.NewApplyRelativeScrub(-PlaybackCursor.CurrentSnapInterval);

            Graph.LayoutManager.ScrollToPlaybackCursor(-1, modTime: false, clampTime: true);
        }

        public void ReselectCurrentAnimation()
        {
            SelectNewAnimRef(SelectedAnimCategory, SelectedAnim);
        }



        private bool ActualHardReset()
        {
            if (Graph == null)
                return false;
            bool success = false;
            NewAnimationContainer.GLOBAL_SYNC_FORCE_REFRESH = true;
            try
            {
                SelectNewAnimRef(SelectedAnimCategory, SelectedAnim);
                ParentDocument.SoundManager.StopAllSounds();
                Graph.ViewportInteractor.CurrentModel?.AnimContainer?.ResetAll();
                Graph.ViewportInteractor.RootMotionSendHome();
                Graph.ViewportInteractor.ResetRootMotion();
                
                Graph.PlaybackCursor.RestartFromBeginning();
                Graph.PlaybackCursor.IsTempPausedUntilAnimChange = false;
                Graph.ViewportInteractor.CurrentModel?.AnimContainer?.ResetAll();
                Graph.ViewportInteractor.CurrentModel?.ChrAsm?.ForAllWeaponModels(wpnMdl =>
                    wpnMdl?.AnimContainer?.ResetAll());
                if (Graph.ViewportInteractor.CurrentModel?.AnimContainer != null)
                {
                    Graph.ViewportInteractor.CurrentModel?.AnimContainer.ResetRootMotion();
                    Graph.ViewportInteractor.HardResetRootMotionToStartHere();
                }
                Graph.ViewportInteractor.CurrentModel?.NewForceSyncUpdate();
                Graph.ViewportInteractor.CurrentModel?.ChrAsm?.ForAllWeaponModels(wpnMdl =>
                {
                    wpnMdl?.AnimContainer?.ResetAll();
                    wpnMdl?.NewForceSyncUpdate();
                });
                Graph.ViewportInteractor.CurrentModel?.AC6NpcParts?.AccessModelsOfAllParts((partIndex, part, model) =>
                {
                    model?.AnimContainer?.ResetAll();
                    model?.NewForceSyncUpdate();
                });


                ParentDocument.RumbleCamManager.ClearActive();
                GFX.CurrentWorldView.RootMotionFollow_Translation = Vector3.Zero;
                GFX.CurrentWorldView.RootMotionFollow_Rotation = 0;
                GFX.CurrentWorldView.Update(0);
                Graph?.ViewportInteractor?.ActionSim?.RequestAnimRestart();
                ParentDocument.SoundManager.SoftWipeAllSlots();
                lock (Graph._lock_ActionBoxManagement)
                {
                    Graph.AnimRef.SafeAccessActions(actions =>
                    {
                        foreach (var act in actions)
                        {
                            act.NewSimulationEnter = false;
                            act.NewSimulationExit = false;
                            act.NewSimulationActive = false;
                        }
                    });
                   
                }

                ParentDocument.SoundManager.Update(0, GFX.CurrentWorldView.CameraLocationInWorld.WorldMatrix, this);
                OSD.SpWindowGraph.IsRequestFocus = true;


                Graph.ViewportInteractor.RemoveTransition();
                Graph.PlaybackCursor.RestartFromBeginning();
                Graph.PlaybackCursor.IsTempPausedUntilAnimChange = false;
                Graph.ViewportInteractor.CurrentModel?.AnimContainer?.ResetAll();
                Graph.ViewportInteractor.CurrentModel?.ResetAttackDistMeasureAccumulation();

                Main.MainThreadLazyDispatch(() =>
                {
                    Graph.ViewportInteractor.NewScrub(forceRefreshTimeact: true);
                    Graph.ViewportInteractor.CurrentModel.NewForceSyncUpdate();
                });

                success = true;
            }
            catch (Exception handled_ex) when (Main.EnableErrorHandler.HardReset)
            {
                success = false;
                Main.HandleError(nameof(Main.EnableErrorHandler.HardReset), handled_ex);
            }
            finally
            {
                NewAnimationContainer.GLOBAL_SYNC_FORCE_REFRESH = false;
            }

            return success;
        }

        public void HardReset(bool startPlayback = false)
        {
            HardResetQueued = true;
            HardResetQueued_StartPlaying = startPlayback;
        }

        public void CopyCurrentAnimIDToClipboard(bool isUnformatted)
        {
            string text = isUnformatted ? ((long)SelectedAnim.SplitID.GetFullID(Proj)).ToString() : SelectedAnim.SplitID.GetFormattedIDString(Proj);
            System.Windows.Forms.Clipboard.SetText(text);
            zzz_NotificationManagerIns.PushNotification($"Copied '{text}' to clipboard.");
        }

        public void GotoAnimIDInClipboard()
        {
            var clip = new string(System.Windows.Forms.Clipboard.GetText().Where(c => (c >= '1' && c <= '9') || c == '0').ToArray());
            if (int.TryParse(clip, out int id))
            {
                if (GotoAnimID(SplitAnimID.FromFullID(Proj, id), true, false, out _))
                {
                    zzz_NotificationManagerIns.PushNotification($"Went to animation with ID in clipboard ({id}).");
                }
                else
                {
                    zzz_NotificationManagerIns.PushNotification($"Unable to find animation with ID in clipboard ({id}).");
                }
            }
            else
            {
                zzz_NotificationManagerIns.PushNotification("Text in clipboard was not a valid animation ID.");
            }
            
        }

        public void Update(float elapsedTime)
        {
            //Main.Input.Update()
            //Console.WriteLine($"Main.HasUncommittedWindowResize = {Main.HasUncommittedWindowResize}");
            if (Main.HasUncommittedWindowResize)
            {
                //MouseHoverKind = ScreenMouseHoverKind.None;
                //WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
                //WhereLastMouseClickStarted = ScreenMouseHoverKind.None;
                //CurrentDividerDragMode = DividerDragMode.None;
                return;
            }

            if (!ParentDocument.LoadingTaskMan.AnyInteractionBlockingTasks() && HardResetQueued)
            {
                if (ActualHardReset())
                {
                    if (HardResetQueued_StartPlaying)
                    {
                        PlaybackCursor.IsPlaying = true;
                    }
                    HardResetQueued_StartPlaying = false;
                    HardResetQueued = false;
                }
            }

            if (Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
            {
                if (Graph?.PlaybackCursor?.Scrubbing == true || Graph?.PlaybackCursor?.IsStepping == true)
                {
                    RemoManager.CancelFullPlayback();
                }

                if (REMO_HOTFIX_REQUEST_PLAY_RESUME_NEXT_FRAME && Graph != null && Graph.PlaybackCursor != null)
                {
                    HardReset();
                    REMO_HOTFIX_REQUEST_PLAY_RESUME_THIS_FRAME = true;
                    REMO_HOTFIX_REQUEST_PLAY_RESUME_NEXT_FRAME = false;
                }
                else if (REMO_HOTFIX_REQUEST_PLAY_RESUME_THIS_FRAME)
                {
                    HardReset();
                    REMO_HOTFIX_REQUEST_PLAY_RESUME_THIS_FRAME = false;
                }
            }

            if (QueuedChangeActionType != null)
            {
                ChangeTypeOfEvent(QueuedChangeActionType);
                QueuedChangeActionType = null;
            }

            if (ImporterWindow_FLVER2 == null || ImporterWindow_FLVER2.IsDisposed || !ImporterWindow_FLVER2.Visible)
            {
                ImporterWindow_FLVER2?.Dispose();
                ImporterWindow_FLVER2 = null;
            }
            ImporterWindow_FLVER2?.UpdateGuiLockout();

            //if (!Input.LeftClickHeld)
            //{
            //    Graph?.ReleaseCurrentDrag();
            //}

#if !DEBUG
            if (!Main.Active)
            {
                ParentDocument.SoundManager.StopAllSounds();
            }
#endif
            //TODO: CHECK THIS
            PauseUpdate = OSD.AnyFieldFocused;
            
            // Always update playback regardless of GUI memes.
            // Still only allow hitting spacebar to play/pause
            // if the window is in focus.
            // Also for Interactor
            if (Graph != null)
            {
                Graph.UpdatePlaybackCursor(allowPlayPauseInput: Main.Active);
                Graph.ViewportInteractor?.GeneralUpdate(elapsedTime);
            }

            

            //if (MenuBar.IsAnyMenuOpenChanged)
            //{
            //    ButtonEditCurrentAnimInfo.Visible = !MenuBar.IsAnyMenuOpen;
            //    ButtonEditCurrentTaeHeader.Visible = !MenuBar.IsAnyMenuOpen;
            //    ButtonGotoEventSource.Visible = !MenuBar.IsAnyMenuOpen;
            //    inspectorWinFormsControl.Visible = !MenuBar.IsAnyMenuOpen;
            //}

            if (OSD.AnyFieldFocused)
            {
                PauseUpdate = true;
            }

            if (!DialogManager.AnyDialogsShowing && !OSD.AnyFieldFocused)
            {
                Transport.PlaybackCursor = PlaybackCursor;
                Transport.Update(Main.DELTA_UPDATE);
            }

            if (PauseUpdate || DialogManager.AnyDialogsShowing)
            {
                
                return;
            }

            //if (!(OSD.Focused || DialogManager.AnyDialogsShowing))
            //    Transport.Update(Main.DELTA_UPDATE);

            

            bool isOtherPaneFocused = ModelViewerBounds.Contains((int)Input.LeftClickDownAnchor.X, (int)Input.LeftClickDownAnchor.Y);

            Input.CursorType = MouseCursorType.Arrow;

            if (Main.Active && !OSD.AnyFieldFocused)
            {
                if (GhettoInputCooldown > 0)
                {
                    GhettoInputCooldown--;
                    return;
                }
                if (Input.KeyDownNoMods(Keys.Insert) && Proj != null && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    ShowDialogDuplicateCurrentAnimation();
                }
                else if (Input.ShiftOnlyHeld && Input.KeyDown(Keys.Insert) && FileContainer?.Proj != null && Proj != null)
                {
                    ShowDialogDuplicateToNewTaeSection(FileContainer.Proj, SelectedAnimCategory);
                }

                if (Input.KeyDownNoMods(Keys.Escape) && Proj != null)
                {
                    ParentDocument.SoundManager.StopAllSounds();
                    ParentDocument.RumbleCamManager.ClearActive();
                }

                if (Input.KeyDownNoMods(Microsoft.Xna.Framework.Input.Keys.F1) && Proj != null
                    && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                    ChangeTypeOfEvent(InspectorAction);

                if (Input.KeyDownNoMods(Microsoft.Xna.Framework.Input.Keys.F2) && Proj != null
                     && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    ShowDialogChangeAnimName(Proj, SelectedAnimCategory, SelectedAnim);
                }
                else if (Input.KeyDown(Keys.F2) && Input.ShiftOnlyHeld && Proj != null)
                {
                    ShowDialogChangeAnimCategoryName(SelectedAnimCategory);
                }

                if (Input.KeyDownNoMods(Microsoft.Xna.Framework.Input.Keys.F3) && Proj != null 
                     && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    ShowDialogEditCurrentAnimInfo();
                }
                else if (Input.KeyDown(Keys.F3) && (Input.CtrlHeld && Input.ShiftHeld && !Input.AltHeld) && Proj != null)
                {
                    ShowDialogEditRootTaeProperties();
                }
                else if (Input.KeyDown(Keys.F3) && Input.ShiftOnlyHeld && Proj != null)
                {
                    ShowDialogEditAnimCategoryProperties(SelectedAnimCategory);
                }

                if ((Input.KeyDownNoMods(Microsoft.Xna.Framework.Input.Keys.F4) || RequestGoToEventSource) && Proj != null
                    && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    GoToEventSource();
                    RequestGoToEventSource = false;
                }

                if (Input.KeyDownNoMods(Microsoft.Xna.Framework.Input.Keys.F5) && Proj != null)
                    LiveRefresh();




                if (Input.KeyDownNoMods(Microsoft.Xna.Framework.Input.Keys.F6) && Proj != null)
                {
                    ResortTracks_Anim();
                }
                else if (Input.ShiftOnlyHeld && Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F6) && Proj != null)
                {
                    ResortTracks_Proj();
                }

                if (Input.KeyDownNoMods(Microsoft.Xna.Framework.Input.Keys.F7) && Proj != null)
                {
                    RegenTrackNames_Anim();
                }
                else if (Input.ShiftOnlyHeld && Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F7) && Proj != null)
                {
                    RegenTrackNames_Proj();
                }



                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.C) && Proj != null
                    && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    if (Input.ShiftOnlyHeld)
                    {
                        DialogManager.AskYesNo("Remove Custom Colors From Selected Actions", "Are you sure you want to remove the custom colors from all selected actions?", choice =>
                        {
                            if (choice)
                                Graph.MainScreen.SetColorOfSelectedActions(isClearing: true);
                        });
                        
                    }
                    else if (!Input.AnyModifiersHeld)
                    {
                        Graph.MainScreen.SetColorOfSelectedActions(isClearing: false);
                    }
                }

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.T) && Proj != null
                    && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    if (Input.ShiftOnlyHeld)
                    {
                        DialogManager.AskYesNo("Remove Tags From Selected Actions", "Are you sure you want to remove all tags from all selected actions?", choice =>
                        {
                            if (choice)
                                Graph.MainScreen.ClearAllTagsFromSelectedActions(Proj);
                        });

                    }
                    else if (!Input.AnyModifiersHeld)
                    {
                        DialogManager.ShowTagPickDialog("Add Tag To Selected Actions", Proj, tag =>
                        {
                            Graph.MainScreen.AddTagToSelectedActions(Proj, tag);
                        });
                    }
                }




                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F8))
                {
                    OSD.WindowComboViewer.IsOpen = !OSD.WindowComboViewer.IsOpen;
                }


                if (Main.IsNightfallBuild && Proj != null)
                {
                    // MEOW TESTING?
                    if (Input.KeyDownNoMods(Microsoft.Xna.Framework.Input.Keys.OemSemicolon))
                        NIGHTFALL_ToggleImport();
                }

              
                var zHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Z);
                var yHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Y);

                if (Input.AltOnlyHeld && Proj != null)
                {
                    if (Input.KeyDown(Keys.Left))
                        AnimHistoryGoBack();
                    else if (Input.KeyDown(Keys.Right))
                        AnimHistoryGoForward();
                }

                if (Input.CtrlOnlyHeld && !DialogManager.AnyDialogsShowing && Proj != null)
                {
                    if ((Input.KeyDown(Keys.OemPlus) || Input.KeyDown(Keys.Add)) && !isOtherPaneFocused)
                    {
                        Graph?.InputMan.ZoomInOneNotch(
                            (float)(
                            (Graph.PlaybackCursor.GUICurrentTime * Graph.LayoutManager.SecondsPixelSize)
                            - Graph.ScrollViewer.Scroll.X));
                    }
                    else if ((Input.KeyDown(Keys.OemMinus) || Input.KeyDown(Keys.Subtract)) && !isOtherPaneFocused)
                    {
                        Graph?.InputMan.ZoomOutOneNotch(
                            (float)(
                            (Graph.PlaybackCursor.GUICurrentTime * Graph.LayoutManager.SecondsPixelSize)
                            - Graph.ScrollViewer.Scroll.X));
                    }
                    else if ((Input.KeyDown(Keys.D0) || Input.KeyDown(Keys.NumPad0)) && !isOtherPaneFocused)
                    {
                        Graph?.InputMan.ResetZoom(0);
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.C) && !isOtherPaneFocused
                        && SelectedAnim?.IS_DUMMY_ANIM == false)
                    {
                        Graph?.InputMan.DoCopy();
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.X) && !isOtherPaneFocused
                        && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                    {
                        Graph?.InputMan.DoCut();
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.V) && !isOtherPaneFocused
                        && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                    {
                        //if (debugTimerInputChatter != null && debugTimerInputChatter.ElapsedMilliseconds < 100)
                        //{
                        //    Console.WriteLine("break");
                        //}

                        Graph?.InputMan.DoPaste(isAbsoluteLocation: false);

                        //debugTimerInputChatter = Stopwatch.StartNew();
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.A) && !isOtherPaneFocused
                        && SelectedAnim?.IS_DUMMY_ANIM == false)
                    {
                        Graph?.InputMan.DoSelectAll();
                    }
                    else if (Input.KeyDown(Keys.F))
                    {
                        ShowDialogFind();
                    }
                    else if (Input.KeyDown(Keys.G))
                    {
                        ShowDialogGotoAnimID();
                    }
                    else if (Input.KeyDown(Keys.H))
                    {
                        ShowDialogGotoAnimSectionID();
                    }
                    else if (Input.KeyDown(Keys.I))
                    {
                        ShowDialogImportFromAnimID();
                    }
                    else if (Input.KeyDown(Keys.J) && SelectedAnim?.IS_DUMMY_ANIM == false)
                    {
                        if (IsFileOpen)
                        {
                            CopyCurrentAnimIDToClipboard(false);
                        }
                    }
                    else if (Input.KeyDown(Keys.K))
                    {
                        if (IsFileOpen)
                        {
                            GotoAnimIDInClipboard();
                        }
                    }
                    else if (Input.KeyDown(Keys.S))
                    {
                        SaveCurrentFile();
                    }
                    else if (Graph != null && Input.KeyDown(Keys.R))
                    {
                        HardReset();
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.RightClickDown
                        && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                    {
                        Graph?.InputMan?.DoAddNewEventAtMouse();
                    }
                }

                if (Input.ShiftOnlyHeld && Input.KeyDown(Keys.Delete) && !DialogManager.AnyDialogsShowing)
                {
                    GhettoInputCooldown = 5;
                    DialogManager.AskYesNo("Permanently Delete Animation Entry?", $"Are you sure you want to delete the current animation?",
                        choice =>
                        {
                            if (choice)
                            {
                                //Main.WinForm.Invoke(() =>
                                //{
                                //    DeleteCurrentAnimation();
                                //});
                                GhettoInputCooldown = 5;
                                DeleteCurrentAnimation();
                            }

                        });
                    
                }

                if (Input.CtrlHeld && Input.ShiftHeld && !Input.AltHeld && Proj != null)
                {
                    if (Input.KeyDown(Keys.V) && !isOtherPaneFocused && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                    {
                        Graph?.InputMan?.DoPaste(isAbsoluteLocation: true);
                    }
                    //else if (Input.KeyDown(Keys.S))
                    //{
                    //    File_SaveAs();
                    //}
                    else if (Input.KeyDown(Keys.J) && SelectedAnim?.IS_DUMMY_ANIM == false)
                    {
                        if (IsFileOpen)
                        {
                            CopyCurrentAnimIDToClipboard(true);
                        }
                    }
                }

                if (!Input.CtrlHeld && Input.ShiftHeld && !Input.AltHeld && Proj != null)
                {
                    if (Input.KeyDown(Keys.D))
                    {
                        NewSelectedActions.Clear();
                    }
                }

                if (Input.KeyDownNoMods(Keys.Delete) && !isOtherPaneFocused && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    Graph?.InputMan?.DeleteSelectedActions();
                }
                
                if (Input.KeyDownNoMods(Keys.D1) && !isOtherPaneFocused && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    Graph?.InputMan?.ToggleMuteOnSelectedActions(uniform: false);
                }
                if (Input.ShiftOnlyHeld && Input.KeyDown(Keys.D1) && !isOtherPaneFocused && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    Graph?.InputMan?.ToggleMuteOnSelectedActions(uniform: true);
                }
                if (Input.KeyDownNoMods(Keys.D2) && !isOtherPaneFocused && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    Graph?.InputMan?.ToggleSoloOnSelectedActions(uniform: false);
                }
                if (Input.ShiftOnlyHeld && Input.KeyDown(Keys.D2) && !isOtherPaneFocused && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    Graph?.InputMan?.ToggleSoloOnSelectedActions(uniform: true);
                }
                if (Input.ShiftOnlyHeld && Input.KeyDown(Keys.D3) && !isOtherPaneFocused && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    Graph?.InputMan?.ClearAllSoloAndMute();
                }
                if (Input.KeyDownNoMods(Keys.D4) && !isOtherPaneFocused && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    Graph?.InputMan?.ToggleStateInfoOnSelectedActions(uniform: false);
                }
                if (Input.ShiftOnlyHeld && Input.KeyDown(Keys.D4) && !isOtherPaneFocused && !Graph.IsGhostGraph && SelectedAnim?.IS_DUMMY_ANIM == false)
                {
                    Graph?.InputMan?.ToggleStateInfoOnSelectedActions(uniform: true);
                }

                //if (Graph != null && Input.KeyDown(Keys.Home) && !Graph.PlaybackCursor.Scrubbing)
                //{
                //    if (CtrlHeld)
                //        Graph.PlaybackCursor.IsPlaying = false;

                //    Graph.PlaybackCursor.CurrentTime = ShiftHeld ? 0 : Graph.PlaybackCursor.StartTime;
                //    Graph.ViewportInteractor.ResetRootMotion(0);

                //    Graph.ScrollToPlaybackCursor(1);
                //}



                //if (Graph != null && Input.KeyDown(Keys.End) && !Graph.PlaybackCursor.Scrubbing)
                //{
                //    if (CtrlHeld)
                //        Graph.PlaybackCursor.IsPlaying = false;

                //    Graph.PlaybackCursor.CurrentTime = Graph.PlaybackCursor.MaxTime;
                //    Graph.ViewportInteractor.ResetRootMotion((float)Graph.PlaybackCursor.MaxFrame);

                //    Graph.ScrollToPlaybackCursor(1);
                //}

                NextAnimRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Down) && !Input.KeyHeld(Keys.Up));
                NextAnimRepeaterButton_KeepSubID.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.PageDown) && !Input.KeyHeld(Keys.PageUp));

                if (NextAnimRepeaterButton.State && Proj != null)
                {
                    if (Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
                    {
                        if (!REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME && Graph?.PlaybackCursor?.IsPlayingRemoFullPreview != true && !REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME)
                        {
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME = true;

                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_PREV = false;
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_SHIFT = Input.ShiftHeld;
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_CTRL = Input.CtrlHeld;

                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE = null;
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE_ANIM = null;
                        }
                    }
                    else
                    {
                        Graph?.ViewportInteractor?.CancelCombo();
                        RemoManager.CancelFullPlayback();
                        NextAnim(Input.ShiftHeld, Input.CtrlHeld);
                    }
                    
                }

                if (NextAnimRepeaterButton_KeepSubID.State && Proj != null)
                {
                    if (Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
                    {

                    }
                    else
                    {
                        var anim = SelectedAnim;
                        var category = SelectedAnimCategory;

                        do
                        {
                            category = Proj.SAFE_GuiHelperSelectNextCategory(category);
                        } while (!category.SAFE_GetAnimations().Any(a => a.SplitID.SubID == anim.SplitID.SubID));

                        var newAnim = category.SAFE_GetAnimations().First(a => a.SplitID.SubID == anim.SplitID.SubID);
                       
                        SelectNewAnimRef(category, newAnim, isPushCurrentToHistBackwardStack: true);
                    }
                }

                PrevAnimRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Up) && !Input.KeyHeld(Keys.Down));
                PrevAnimRepeaterButton_KeepSubID.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.PageUp) && !Input.KeyHeld(Keys.PageDown));

                if (PrevAnimRepeaterButton.State && Proj != null)
                {
                    if (Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
                    {
                        if (!REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME && Graph?.PlaybackCursor?.IsPlayingRemoFullPreview != true && !REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME)
                        {
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME = true;

                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_PREV = true;
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_SHIFT = Input.ShiftHeld;
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_CTRL = Input.CtrlHeld;

                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE = null;
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE_ANIM = null;
                            
                        }
                    }
                    else
                    {
                        Graph?.ViewportInteractor?.CancelCombo();
                        RemoManager.CancelFullPlayback();
                        PrevAnim(Input.ShiftHeld, Input.CtrlHeld);
                    }
                    
                }

                if (PrevAnimRepeaterButton_KeepSubID.State && Proj != null)
                {
                    if (Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
                    {

                    }
                    else
                    {
                        var anim = SelectedAnim;
                        var category = SelectedAnimCategory;

                        do
                        {
                            category = Proj.SAFE_GuiHelperSelectPrevCategory(category);
                        } while (!category.SAFE_GetAnimations().Any(a => a.SplitID.SubID == anim.SplitID.SubID));

                        var newAnim = category.SAFE_GetAnimations().First(a => a.SplitID.SubID == anim.SplitID.SubID);
                       
                        SelectNewAnimRef(category, newAnim, isPushCurrentToHistBackwardStack: true);
                    }
                }

                bool animHistoryGoBackwardHeld = Input.XButton1Held || (Input.AltOnlyHeld && Input.KeyHeld(Keys.Left));
                bool animHistoryGoForwardHeld = Input.XButton2Held || (Input.AltOnlyHeld && Input.KeyHeld(Keys.Right));

                AnimHistoryBackwardRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, animHistoryGoBackwardHeld && !animHistoryGoForwardHeld);
                AnimHistoryForwardRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, animHistoryGoForwardHeld && !animHistoryGoBackwardHeld);

                if (AnimHistoryBackwardRepeaterButton.State && Proj != null)
                {
                    AnimHistoryGoBack();
                }

                if (AnimHistoryForwardRepeaterButton.State && Proj != null)
                {
                    AnimHistoryGoForward();
                }


                if (REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME)
                {
                    if (REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE != null && REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE_ANIM != null)
                    {
                        SelectNewAnimRef(REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE, REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE_ANIM);
                    }
                    else
                    {
                        if (REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_PREV)
                            PrevAnim(REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_SHIFT, REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_CTRL);
                        else
                            NextAnim(REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_SHIFT, REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_CTRL);
                    }
                    //??????????????????????????????????????????????
                    // if (SelectedAnim.ID <= 9999)
                    //     Graph.PlaybackCursor.IsPlaying = true;
                }

                REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME = false;

                if (PlaybackCursor != null)
                {
                    NextFrameRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Right));

                    if (NextFrameRepeaterButton.State)
                    {
                        TransportNextFrame();
                    }

                    PrevFrameRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Left));

                    if (PrevFrameRepeaterButton.State)
                    {
                        TransportPreviousFrame();
                    }
                }

                if (Input.KeyDown(Keys.Space) && Input.CtrlHeld && !Input.AltHeld && Proj != null)
                {
                    if (SelectedAnimCategory != null)
                    {
                        if (SelectedAnim != null)
                        {
                            SelectNewAnimRef(SelectedAnimCategory, SelectedAnim);
                            if (Input.ShiftHeld)
                            {
                                Graph?.ViewportInteractor?.RemoveTransition();
                            }
                        }
                    }
                }

                if (Input.KeyDown(Keys.Back) && !isOtherPaneFocused)
                {
                    Graph?.ViewportInteractor?.RemoveTransition();
                }

                if (UndoButton.Update(Main.DELTA_UPDATE, (Input.CtrlHeld && !Input.AltHeld) && (zHeld && !yHeld)) && !isOtherPaneFocused && Proj != null)
                {
                    
                    if (Input.ShiftHeld)
                    {
                        if (GlobalUndoMan?.CanUndo == true)
                        {
                            DialogManager.AskYesNo("WARNING",
                                "All changes on all animations since the [Global] operation was " +
                                "\nperformed will be lost if you undo the global operation. " +
                                "\nAre you sure you wish to do this?",
                                choice =>
                                {
                                    if (choice)
                                    {
                                        GlobalUndoMan?.Undo();
                                    }
                                }, allowCancel: true, inputFlags: InputFlag.EscapeKeyToCancel | InputFlag.TitleBarXToCancel | InputFlag.EnterKeyToAccept);
                        }
                    }
                    else
                        CurrentAnimUndoMan?.Undo();
                }

                if (RedoButton.Update(Main.DELTA_UPDATE, (Input.CtrlHeld && !Input.AltHeld) && (!zHeld && yHeld)) && !isOtherPaneFocused && Proj != null)
                {
                    if (Input.ShiftHeld)
                    {
                        if (GlobalUndoMan?.CanRedo == true)
                        {
                            DialogManager.AskYesNo("WARNING",
                                "All changes on all animations since the [Global] operation was " +
                                "\nundone will be lost if you redo the global operation. " +
                                "\nAre you sure you wish to do this?",
                                choice =>
                                {
                                    if (choice)
                                    {
                                        GlobalUndoMan?.Redo();
                                    }
                                }, allowCancel: true, inputFlags: InputFlag.EscapeKeyToCancel | InputFlag.TitleBarXToCancel | InputFlag.EnterKeyToAccept);
                        }
                    }
                    else
                        CurrentAnimUndoMan?.Redo();
                }
            }

            if (Graph != null)
            {
                Graph?.InputMan?.UpdateMiddleClickPan();

                //if (!Graph.Rect.Contains(Input.MousePositionPoint))
                //{
                //    HoveringOverEventBox = null;
                //}
                if (Graph?.Rect.Contains(Input.MousePosition) ?? false)
                {
                    Graph?.Update(elapsedTime);
                }
                else
                {
                    Graph?.UpdateMouseOutsideRect(elapsedTime);
                    
                }
            }
            
        }
        
        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex,
            SpriteFont font, float elapsedSeconds, SpriteFont smallFont, Texture2D scrollbarArrowTex)
        {

            UpdateAllHighlightTimers(elapsedSeconds);

            if (true)//(AnimationListScreen != null)
            {
                var graphRect = Graph?.Rect ?? Rectangle.Empty;

                Rectangle curAnimInfoTextRect = new Rectangle(
                    0,
                    0,
                    graphRect.Width - 4,
                    TopOfGraphAnimInfoMargin + 2);


                graphRect.Y -= TopMenuBarMargin;
                graphRect.Height += TopMenuBarMargin;
                graphRect = graphRect.DpiScaled();

                
                //curAnimInfoTextRect.X -= graphRect.X;
                //curAnimInfoTextRect.Y -= graphRect.Y;

                

                var oldViewport = gd.Viewport;
                gd.Viewport = new Viewport(graphRect);
                {
                    sb.Begin();
                    try
                    {
                        if (TaeAnimInfoIsClone)
                        {
                            //ImGuiNET.ImGui.SetCursorPos(curAnimInfoTextRect.TopLeftCornerN());
                            //ImGuiNET.ImGui.Button("Go To Original (F4 Key)", new System.Numerics.Vector2(120, curAnimInfoTextRect.Height));
                            //if (ImGuiNET.ImGui.IsItemClicked())
                            //{
                            //    RequestGoToEventSource = true;
                            //}

                            //ImGuiDebugDrawer.DrawRect(new Rectangle(curAnimInfoTextRect.X, curAnimInfoTextRect.Y, 120, curAnimInfoTextRect.Height), )

                            // if (ImGuiDebugDrawer.FakeButton(curAnimInfoTextRect.TopLeftCorner() + new Vector2(2,3), 
                            //     new Vector2((int)(170), curAnimInfoTextRect.Height) - new Vector2(2,2), "Go To Original (F4 Key)", 0, out bool isHovering))
                            // {
                            //     RequestGoToEventSource = true;
                            // }
                            //
                            // curAnimInfoTextRect.X += (int)(170) + 8;
                            // curAnimInfoTextRect.Width -= (int)(170);
                            
                        }

                        // if (Config.EnableFancyScrollingStrings)
                        // {
                        //     SelectedTaeAnimInfoScrollingText.Draw(gd, sb, Matrix.Identity, curAnimInfoTextRect.DpiScaled(), 20f, elapsedSeconds, new Vector2(0, 4), restrictToParentViewport: false);
                        // }
                        // else
                        // {
                        //     var curAnimInfoTextPos = curAnimInfoTextRect.Location.ToVector2();
                        //
                        //     //sb.DrawString(font, SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + Vector2.One + Main.GlobalTaeEditorFontOffset, Color.Black);
                        //     //sb.DrawString(font, SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + (Vector2.One * 2) + Main.GlobalTaeEditorFontOffset, Color.Black);
                        //     //sb.DrawString(font, SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + Main.GlobalTaeEditorFontOffset, Color.White);
                        //     ImGuiDebugDrawer.DrawText(SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + new Vector2(0, 4), Color.White, Color.Black, 20);
                        // }

                        //sb.DrawString(font, SelectedTaeAnimInfoScrollingText, curAnimInfoTextPos + Vector2.One, Color.Black);
                        //sb.DrawString(font, SelectedTaeAnimInfoScrollingText, curAnimInfoTextPos + (Vector2.One * 2), Color.Black);
                        //sb.DrawString(font, SelectedTaeAnimInfoScrollingText, curAnimInfoTextPos, Color.White);
                    }
                    finally { sb.End(); }
                }
                gd.Viewport = oldViewport;

                



            }

            if (Graph != null)
            {
                try
                {
                    Graph.Draw(gd, sb, boxTex, font, elapsedSeconds, smallFont, scrollbarArrowTex);
                }
                catch
                {

                }

            }
            else
            {
                // Draws a very, very blank graph is none is loaded:

                //var graphRect = new Rectangle(
                //        (int)MiddleSectionStartX,
                //        Rect.Top + TopMenuBarMargin + TopOfGraphAnimInfoMargin,
                //        (int)MiddleSectionWidth,
                //        Rect.Height - TopMenuBarMargin - TopOfGraphAnimInfoMargin);

                //sb.Begin();
                //sb.Draw(texture: boxTex,
                //    position: new Vector2(graphRect.X, graphRect.Y),
                //    sourceRectangle: null,
                //    color: new Color(120, 120, 120, 255),
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2(graphRect.Width, graphRect.Height),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );

                //sb.Draw(texture: boxTex,
                //    position: new Vector2(graphRect.X, graphRect.Y),
                //    sourceRectangle: null,
                //    color: new Color(64, 64, 64, 255),
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2(graphRect.Width, TaeEditAnimEventGraph.TimeLineHeight),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );
                //sb.End();
            }

            //Transport.Draw(gd, sb, boxTex, smallFont);

            if (AnimSwitchRenderCooldown > 0 && !zzz_DocumentManager.AnyDocumentWithAnyInteractionBlockingLoadingTasks())
            {
                AnimSwitchRenderCooldown--;

                //float ratio = Math.Max(0, Math.Min(1, MathHelper.Lerp(0, 1, AnimSwitchRenderCooldown / AnimSwitchRenderCooldownFadeLength)));
                //sb.Begin();
                //sb.Draw(boxTex, graphRect, AnimSwitchRenderCooldownColor * ratio);
                //sb.End();
            }



            //editScreenGraphInspector.Draw(gd, sb, boxTex, font);

            //var oldViewport = gd.Viewport;
            //gd.Viewport = new Viewport(Rect.X, Rect.Y, Rect.Width, TopMargin);
            //{
            //    sb.Begin();

            //    sb.DrawString(font, $"{TaeFileName}", new Vector2(4, 4) + Vector2.One, Color.Black);
            //    sb.DrawString(font, $"{TaeFileName}", new Vector2(4, 4), Color.White);

            //    sb.End();
            //}
            //gd.Viewport = oldViewport;

            if (REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME)
            {
                float scale = 1;
                var textSize = (ImGuiNET.ImGui.CalcTextSize("LOADING CUTSCENE ENTITIES...").ToXna() * scale);
                var printer = new StatusPrinter(ModelViewerBounds.TopRightCorner() + new Vector2(-(textSize.X + 32), 8), Color.Lime, Color.Black);

                printer.AppendLine("LOADING CUTSCENE ENTITIES...");

                printer.BaseScale = scale;
                printer.Draw(out _);

                REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME = false;
                REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME = true;
            }

        }
    }
}
