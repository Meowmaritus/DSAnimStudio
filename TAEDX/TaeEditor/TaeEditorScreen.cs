using MeowDSIO.DataFiles;
using MeowDSIO.DataTypes.TAE;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.TaeEditor
{
    public class TaeEditorScreen
    {
        enum DividerDragMode
        {
            None,
            Left,
            Right,
        }

        enum ScreenMouseHoverKind
        {
            None,
            AnimList,
            EventGraph,
            Inspector
        }

        private int TopMargin = 32;

        const string HELP_TEXT = 
            "Left Click + Drag Middle of Event:\n" +
            "    Move whole event\n" +
            "Left Click + Drag Left/Right Side of Event:\n" +
            "    Move start/end of event\n" +
            "Left Click:\n" +
            "    Highlight event under mouse cursor\n" +
            "Right Click:\n" +
            "    Place copy of last highlighted event at mouse cursor\n" +
            "Delete Key:\n" +
            "    Delete highlighted event.\n\n\n" +
            "The pane on the right shows the parameters of the highlighted event." +
            "Click \"Change Type\" on the upper-right corner to change the event type of the highlighted event.";

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

        public Rectangle Rect;

        public Dictionary<AnimationRef, TaeUndoMan> UndoManDictionary 
            = new Dictionary<AnimationRef, TaeUndoMan>();

        public TaeUndoMan UndoMan
        {
            get
            {
                if (!UndoManDictionary.ContainsKey(TaeAnim))
                {
                    var newUndoMan = new TaeUndoMan();
                    newUndoMan.CanUndoMaybeChanged += UndoMan_CanUndoMaybeChanged;
                    newUndoMan.CanRedoMaybeChanged += UndoMan_CanRedoMaybeChanged;
                    UndoManDictionary.Add(TaeAnim, newUndoMan);
                }
                return UndoManDictionary[TaeAnim];
            }
        }

        private bool _IsModified = false;
        public bool IsModified
        {
            get => _IsModified;
            set
            {
                _IsModified = value;
                ToolStripFileSave.Enabled = value;
            }
        }

        private System.Windows.Forms.ToolStripMenuItem ToolStripFileSave;
        private System.Windows.Forms.ToolStripMenuItem ToolStripFileSaveAs;

        private void UndoMan_CanRedoMaybeChanged(object sender, EventArgs e)
        {
            ToolStripEditRedo.Enabled = UndoMan.CanRedo;
        }

        private void UndoMan_CanUndoMaybeChanged(object sender, EventArgs e)
        {
            ToolStripEditUndo.Enabled = UndoMan.CanUndo;
        }


        private TaeButtonRepeater UndoButton = new TaeButtonRepeater(0.5f, 0.15f);
        private TaeButtonRepeater RedoButton = new TaeButtonRepeater(0.5f, 0.15f);
        private System.Windows.Forms.ToolStripMenuItem ToolStripEditUndo;
        private System.Windows.Forms.ToolStripMenuItem ToolStripEditRedo;

        //private System.Windows.Forms.ToolStripMenuItem ToolStripAccessibilityDisableRainbow;
        private System.Windows.Forms.ToolStripMenuItem ToolStripAccessibilityColorBlindMode;

        private float LeftSectionWidth = 256;
        private float DividerLeftGrabStart => Rect.Left + LeftSectionWidth - (DividerHitboxPad / 2f);
        private float DividerLeftGrabEnd => Rect.Left + LeftSectionWidth + (DividerHitboxPad / 2f);

        private float RightSectionWidth = 320;
        private float DividerRightGrabStart => Rect.Right - RightSectionWidth - (DividerHitboxPad / 2f);
        private float DividerRightGrabEnd => Rect.Right - RightSectionWidth + (DividerHitboxPad / 2f);

        private float LeftSectionStartX => Rect.Left;
        private float MiddleSectionStartX => LeftSectionStartX + LeftSectionWidth + (DividerHitboxPad / 2f) - (DividerVisiblePad / 2f);
        private float RightSectionStartX => Rect.Right - RightSectionWidth - (DividerHitboxPad / 2f) - (DividerVisiblePad / 2f);

        private float MiddleSectionWidth => DividerRightGrabStart - DividerLeftGrabEnd - (DividerHitboxPad / 2f) + (DividerVisiblePad / 2f);

        private float DividerVisiblePad = 4;
        private float DividerHitboxPad = 8;

        private DividerDragMode CurrentDividerDragMode = DividerDragMode.None;

        private ScreenMouseHoverKind MouseHoverKind = ScreenMouseHoverKind.None;
        private ScreenMouseHoverKind oldMouseHoverKind = ScreenMouseHoverKind.None;

        public TAE Tae;

        public AnimationRef TaeAnim { get; private set; }

        public readonly System.Windows.Forms.Form GameWindowAsForm;

        private TaeEditAnimEventBox _selectedEventBox = null;
        public TaeEditAnimEventBox SelectedEventBox
        {
            get => _selectedEventBox;
            set
            {
                _selectedEventBox = value;
                inspectorWinFormsControl.labelEventType.Text = 
                    _selectedEventBox?.MyEvent.EventType.ToString() ?? "(None Selected)";
                if (_selectedEventBox == null)
                {
                    inspectorWinFormsControl.buttonChangeType.Enabled = false;
                }
                else
                {
                    inspectorWinFormsControl.buttonChangeType.Enabled = true;
                }
                inspectorWinFormsControl.propertyGrid.SelectedObject = _selectedEventBox?.MyEvent;
            }
        }

        private TaeEditAnimList editScreenAnimList;
        private TaeEditAnimEventGraph editScreenCurrentAnim;
        //private TaeEditAnimEventGraphInspector editScreenGraphInspector;

        private Color ColorInspectorBG = Color.DarkGray;
        private TaeInspectorWinFormsControl inspectorWinFormsControl;

        public TaeInputHandler Input;

        private System.Windows.Forms.MenuStrip WinFormsMenuStrip;

        public string TaeFileName = "";

        public TaeConfigFile Config = new TaeConfigFile();

        public void LoadConfig()
        {
            if (!System.IO.File.Exists("TAEDX_Config.json"))
            {
                Config = new TaeConfigFile();
                SaveConfig();
            }

            var jsonText = System.IO.File.ReadAllText("TAEDX_Config.json");

            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<TaeConfigFile>(jsonText);
        }

        public void SaveConfig()
        {
            var jsonText = Newtonsoft.Json.JsonConvert
                .SerializeObject(Config,
                Newtonsoft.Json.Formatting.Indented);

            System.IO.File.WriteAllText("TAEDX_Config.json", jsonText);
        }

        public void LoadCurrentFile()
        {
            if (System.IO.File.Exists(TaeFileName))
            {
                var newTae = MeowDSIO.DataFile.LoadFromFile<TAE>(TaeFileName);
                LoadTAE(newTae);
                ToolStripFileSaveAs.Enabled = true;
            }
        }

        public void SaveCurrentFile()
        {
            if (System.IO.File.Exists(TaeFileName) && 
                !System.IO.File.Exists(TaeFileName + ".taedxbak"))
            {
                System.IO.File.Copy(TaeFileName, TaeFileName + ".taedxbak");
                System.Windows.Forms.MessageBox.Show(
                    "A backup was not found and was created:\n" + TaeFileName + ".taedxbak",
                    "Backup Created", System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Information);
            }
            MeowDSIO.DataFile.SaveToFile(Tae, TaeFileName);
            foreach (var animRef in Tae.Animations)
            {
                animRef.IsModified = false;
            }
            IsModified = false;
        }

        private void LoadTAE(TAE tae)
        {
            Tae = tae;
            TaeAnim = Tae.Animations[0];
            editScreenAnimList = new TaeEditAnimList(this);
            editScreenCurrentAnim = new TaeEditAnimEventGraph(this);
        }

        public TaeEditorScreen(System.Windows.Forms.Form gameWindowAsForm)
        {
            LoadConfig();

            GameWindowAsForm = gameWindowAsForm;

            Input = new TaeInputHandler();

            //editScreenAnimList = new TaeEditAnimList(this);
            //editScreenCurrentAnim = new TaeEditAnimEventGraph(this);
            //editScreenGraphInspector = new TaeEditAnimEventGraphInspector(this);

            inspectorWinFormsControl = new TaeInspectorWinFormsControl();

            // This might change in the future if I actually add text description attributes to some things.
            inspectorWinFormsControl.propertyGrid.HelpVisible = false;

            inspectorWinFormsControl.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            inspectorWinFormsControl.propertyGrid.ToolbarVisible = false;

            //inspectorPropertyGrid.ViewBackColor = System.Drawing.Color.FromArgb(
            //    ColorInspectorBG.A, ColorInspectorBG.R, ColorInspectorBG.G, ColorInspectorBG.B);

            inspectorWinFormsControl.propertyGrid.LargeButtons = true;

            inspectorWinFormsControl.propertyGrid.CanShowVisualStyleGlyphs = false;

            inspectorWinFormsControl.buttonChangeType.Click += ButtonChangeType_Click;

            inspectorWinFormsControl.propertyGrid.PropertyValueChanged += PropertyGrid_PropertyValueChanged;

            GameWindowAsForm.Controls.Add(inspectorWinFormsControl);

            var toolstripFile = new System.Windows.Forms.ToolStripMenuItem("File");
            {
                var toolstripFile_Open = new System.Windows.Forms.ToolStripMenuItem("Open");
                toolstripFile_Open.Click += ToolstripFile_Open_Click;
                toolstripFile.DropDownItems.Add(toolstripFile_Open);

                toolstripFile.DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());

                ToolStripFileSave = new System.Windows.Forms.ToolStripMenuItem("Save");
                ToolStripFileSave.Enabled = false;
                ToolStripFileSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
                ToolStripFileSave.Click += ToolstripFile_Save_Click;
                toolstripFile.DropDownItems.Add(ToolStripFileSave);

                ToolStripFileSaveAs = new System.Windows.Forms.ToolStripMenuItem("Save As...");
                ToolStripFileSaveAs.Enabled = false;
                ToolStripFileSaveAs.Click += ToolstripFile_SaveAs_Click;
                toolstripFile.DropDownItems.Add(ToolStripFileSaveAs);
            }

            var toolstripEdit = new System.Windows.Forms.ToolStripMenuItem("Edit");
            {
                ToolStripEditUndo = new System.Windows.Forms.ToolStripMenuItem("Undo");
                ToolStripEditUndo.ShortcutKeyDisplayString = "Ctrl+Z";
                ToolStripEditUndo.Click += ToolStripEditUndo_Click;

                ToolStripEditRedo = new System.Windows.Forms.ToolStripMenuItem("Redo");
                ToolStripEditRedo.ShortcutKeyDisplayString = "Ctrl+Y";
                ToolStripEditRedo.Click += ToolStripEditRedo_Click;

                toolstripEdit.DropDownItems.Add(ToolStripEditUndo);
                toolstripEdit.DropDownItems.Add(ToolStripEditRedo);
            }

            var toolstripAccessibility = new System.Windows.Forms.ToolStripMenuItem("Accessibility");
            {
                //ToolStripAccessibilityDisableRainbow = new System.Windows.Forms.ToolStripMenuItem("Use brightness for selection instead of rainbow pulsing");
                //ToolStripAccessibilityDisableRainbow.CheckOnClick = true;
                //ToolStripAccessibilityDisableRainbow.CheckedChanged += ToolStripAccessibilityDisableRainbow_CheckedChanged;
                //ToolStripAccessibilityDisableRainbow.Checked = Config.DisableRainbow;
                //toolstripAccessibility.DropDownItems.Add(ToolStripAccessibilityDisableRainbow);

                ToolStripAccessibilityColorBlindMode = new System.Windows.Forms.ToolStripMenuItem("Color-Blind Mode");
                ToolStripAccessibilityColorBlindMode.CheckOnClick = true;
                ToolStripAccessibilityColorBlindMode.CheckedChanged += ToolStripAccessibilityColorBlindMode_CheckedChanged;
                ToolStripAccessibilityColorBlindMode.Checked = Config.EnableColorBlindMode;
                toolstripAccessibility.DropDownItems.Add(ToolStripAccessibilityColorBlindMode);
            }

            var toolstripHelp = new System.Windows.Forms.ToolStripMenuItem("Help");
            toolstripHelp.Click += ToolstripHelp_Click;

            WinFormsMenuStrip = new System.Windows.Forms.MenuStrip();
            WinFormsMenuStrip.Items.Add(toolstripFile);
            WinFormsMenuStrip.Items.Add(toolstripEdit);
            WinFormsMenuStrip.Items.Add(toolstripAccessibility);
            WinFormsMenuStrip.Items.Add(toolstripHelp);

            WinFormsMenuStrip.MenuActivate += WinFormsMenuStrip_MenuActivate;
            WinFormsMenuStrip.MenuDeactivate += WinFormsMenuStrip_MenuDeactivate;

            GameWindowAsForm.Controls.Add(WinFormsMenuStrip);
        }

        private void ToolStripAccessibilityColorBlindMode_CheckedChanged(object sender, EventArgs e)
        {
            Config.EnableColorBlindMode = ToolStripAccessibilityColorBlindMode.Checked;
            SaveConfig();
        }

        //private void ToolStripAccessibilityDisableRainbow_CheckedChanged(object sender, EventArgs e)
        //{
        //    Config.DisableRainbow = ToolStripAccessibilityDisableRainbow.Checked;
        //    SaveConfig();
        //}

        private void PropertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            var gridReference = (System.Windows.Forms.PropertyGrid)s;
            var boxReference = SelectedEventBox;
            var newValReference = e.ChangedItem.Value;
            var oldValReference = e.OldValue;

            UndoMan.NewAction(doAction: () =>
            {
                e.ChangedItem.PropertyDescriptor.SetValue(boxReference.MyEvent, newValReference);

                TaeAnim.IsModified = true;
                IsModified = true;

                gridReference.Refresh();
            },
            undoAction: () =>
            {
                e.ChangedItem.PropertyDescriptor.SetValue(boxReference.MyEvent, oldValReference);

                gridReference.Refresh();
            });
        }

        private void ToolStripEditRedo_Click(object sender, EventArgs e)
        {
            UndoMan.Redo();
        }

        private void ToolStripEditUndo_Click(object sender, EventArgs e)
        {
            UndoMan.Undo();
        }

        private void ToolstripHelp_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(HELP_TEXT, "TAE Editor Help",
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        private void WinFormsMenuStrip_MenuDeactivate(object sender, EventArgs e)
        {
            PauseUpdate = false;
        }

        private void WinFormsMenuStrip_MenuActivate(object sender, EventArgs e)
        {
            PauseUpdate = true;
        }

        private void ToolstripFile_Open_Click(object sender, EventArgs e)
        {
            if (Tae != null && Tae.Animations.Any(a => a.IsModified))
            {
                var yesNoCancel = System.Windows.Forms.MessageBox.Show(
                    $"File \"{System.IO.Path.GetFileName(TaeFileName)}\" has " +
                    $"unsaved changes. Would you like to save these changes before " +
                    $"loading a new file?", "Save Unsaved Changes?",
                    System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                    System.Windows.Forms.MessageBoxIcon.None);

                if (yesNoCancel == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveCurrentFile();
                }
                else if (yesNoCancel == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                //If they chose no, continue as normal.
            }

            var browseDlg = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "TAE Files (*.TAE)|*.TAE|All Files|*.*",
                ValidateNames = true,
                CheckFileExists = true,
                CheckPathExists = true,
            };

            if (System.IO.File.Exists(TaeFileName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(TaeFileName);
                browseDlg.FileName = System.IO.Path.GetFileName(TaeFileName);
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TaeFileName = browseDlg.FileName;
                LoadCurrentFile();
            }
        }


        private void ToolstripFile_Save_Click(object sender, EventArgs e)
        {
            SaveCurrentFile();
        }

        private void ToolstripFile_SaveAs_Click(object sender, EventArgs e)
        {
            var browseDlg = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = "TAE Files (*.TAE)|*.TAE|All Files|*.*",
                ValidateNames = true,
                CheckFileExists = true,
                CheckPathExists = true,
            };

            if (System.IO.File.Exists(TaeFileName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(TaeFileName);
                browseDlg.FileName = System.IO.Path.GetFileName(TaeFileName);
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TaeFileName = browseDlg.FileName;
                SaveCurrentFile();
            }
        }

        private void ButtonChangeType_Click(object sender, EventArgs e)
        {
            PauseUpdate = true;

            var changeTypeDlg = new TaeInspectorFormChangeEventType();
            if (changeTypeDlg.ShowDialog(GameWindowAsForm) == System.Windows.Forms.DialogResult.OK)
            {
                if (changeTypeDlg.NewEventType != SelectedEventBox.MyEvent.EventType)
                {
                    var referenceToEventBox = SelectedEventBox;
                    var referenceToPreviousEvent = referenceToEventBox.MyEvent;
                    int index = TaeAnim.Anim.EventList.IndexOf(referenceToEventBox.MyEvent);
                    int row = referenceToEventBox.MyEvent.Row;

                    UndoMan.NewAction(
                        doAction: () =>
                        {
                            TaeAnim.Anim.EventList.Remove(referenceToPreviousEvent);
                            referenceToEventBox.ChangeEvent(
                                TimeActEventBase.GetNewEvent(
                                    changeTypeDlg.NewEventType,
                                    referenceToPreviousEvent.StartTimeFr,
                                    referenceToPreviousEvent.EndTimeFr));

                            TaeAnim.Anim.EventList.Insert(index, referenceToEventBox.MyEvent);

                            SelectedEventBox = referenceToEventBox;

                            SelectedEventBox.MyEvent.Row = row;

                            editScreenCurrentAnim.RegisterEventBoxExistance(SelectedEventBox);

                            TaeAnim.IsModified = true;
                            IsModified = true;
                        },
                        undoAction: () =>
                        {
                            TaeAnim.Anim.EventList.RemoveAt(index);
                            referenceToEventBox.ChangeEvent(referenceToPreviousEvent);
                            TaeAnim.Anim.EventList.Insert(index, referenceToPreviousEvent);

                            SelectedEventBox = referenceToEventBox;

                            SelectedEventBox.MyEvent.Row = row;

                            editScreenCurrentAnim.RegisterEventBoxExistance(SelectedEventBox);
                        });
                }
            }

            PauseUpdate = false;
        }

        public void SelectNewAnimRef(AnimationRef animRef)
        {
            TaeAnim = animRef;
            ToolStripEditUndo.Enabled = UndoMan.CanUndo;
            ToolStripEditRedo.Enabled = UndoMan.CanRedo;
            SelectedEventBox = null;
            editScreenCurrentAnim.ChangeToNewAnimRef(TaeAnim);
        }

        public void Update(float elapsedSeconds)
        {
            if (PauseUpdate)
            {
                //PauseUpdateTotalTime += elapsedSeconds;
                return;
            }
            else
            {
                //PauseUpdateTotalTime = 0;
            }


            Input.Update(Rect);

            var ctrlHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.LeftControl) 
                || Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.RightControl);

            var zHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Z);
            var yHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Y);

            if (UndoButton.Update(elapsedSeconds, ctrlHeld && (zHeld && !yHeld)))
            {
                UndoMan.Undo();
            }

            if (RedoButton.Update(elapsedSeconds, ctrlHeld && (!zHeld && yHeld)))
            {
                UndoMan.Redo();
            }

            if (CurrentDividerDragMode == DividerDragMode.None)
            {
                if (Input.MousePosition.X >= DividerLeftGrabStart && Input.MousePosition.X <= DividerLeftGrabEnd)
                {
                    MouseHoverKind = ScreenMouseHoverKind.None;
                    Input.CursorType = MouseCursorType.DragX;
                    if (Input.LeftClickDown)
                    {
                        CurrentDividerDragMode = DividerDragMode.Left;
                    }
                }
                else if (Input.MousePosition.X >= DividerRightGrabStart && Input.MousePosition.X <= DividerRightGrabEnd)
                {
                    MouseHoverKind = ScreenMouseHoverKind.None;
                    Input.CursorType = MouseCursorType.DragX;
                    if (Input.LeftClickDown)
                    {
                        CurrentDividerDragMode = DividerDragMode.Right;
                    }
                }
            }
            else if (CurrentDividerDragMode == DividerDragMode.Left)
            {
                if (Input.LeftClickHeld)
                {
                    Input.CursorType = MouseCursorType.DragX;
                    LeftSectionWidth = MathHelper.Max(Input.MousePosition.X - (DividerHitboxPad / 2), 64);
                }
                else
                {
                    Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                }
            }
            else if (CurrentDividerDragMode == DividerDragMode.Right)
            {
                if (Input.LeftClickHeld)
                {
                    Input.CursorType = MouseCursorType.DragX;
                    RightSectionWidth = MathHelper.Max((Rect.Right - Input.MousePosition.X) + (DividerHitboxPad / 2), 64);
                }
                else
                {
                    Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                }
            }

            if (editScreenAnimList != null && editScreenCurrentAnim != null)
            {
                if (editScreenAnimList.Rect.Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.AnimList;
                else if (editScreenCurrentAnim.Rect.Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.EventGraph;
                else if (
                    new Rectangle(
                        inspectorWinFormsControl.Bounds.Left,
                        inspectorWinFormsControl.Bounds.Top,
                        inspectorWinFormsControl.Bounds.Width,
                        inspectorWinFormsControl.Bounds.Height
                        )
                        .Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.Inspector;
                else
                    MouseHoverKind = ScreenMouseHoverKind.None;

                if (MouseHoverKind == ScreenMouseHoverKind.AnimList)
                    editScreenAnimList.Update(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                else
                    editScreenAnimList.UpdateMouseOutsideRect(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);

                if (MouseHoverKind == ScreenMouseHoverKind.EventGraph)
                    editScreenCurrentAnim.Update(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                else
                    editScreenCurrentAnim.UpdateMouseOutsideRect(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);

            }
            else
            {
                if (new Rectangle(
                inspectorWinFormsControl.Bounds.Left,
                inspectorWinFormsControl.Bounds.Top,
                inspectorWinFormsControl.Bounds.Width,
                inspectorWinFormsControl.Bounds.Height)
                .Contains(Input.MousePositionPoint))
                {
                    MouseHoverKind = ScreenMouseHoverKind.Inspector;
                }
                else
                {
                    MouseHoverKind = ScreenMouseHoverKind.None;
                }

                Input.CursorType = MouseCursorType.StopUpdating;
            }

            


            if (MouseHoverKind != ScreenMouseHoverKind.None && oldMouseHoverKind == ScreenMouseHoverKind.None)
            {
                Input.CursorType = MouseCursorType.Arrow;
            }

            if (MouseHoverKind == ScreenMouseHoverKind.Inspector)
                Input.CursorType = MouseCursorType.StopUpdating;

            //if (editScreenGraphInspector.Rect.Contains(Input.MousePositionPoint))
            //    editScreenGraphInspector.Update(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
            //else
            //    editScreenGraphInspector.UpdateMouseOutsideRect(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);

            oldMouseHoverKind = MouseHoverKind;
        }

        private void UpdateLayout()
        {
            if (editScreenAnimList != null && editScreenCurrentAnim != null)
            {
                editScreenAnimList.Rect = new Rectangle((int)LeftSectionStartX, Rect.Top + TopMargin, (int)LeftSectionWidth, Rect.Height - TopMargin);
                editScreenCurrentAnim.Rect = new Rectangle((int)MiddleSectionStartX, Rect.Top + TopMargin,
                    (int)MiddleSectionWidth, Rect.Height - TopMargin);
            }
            //editScreenGraphInspector.Rect = new Rectangle(Rect.Width - LayoutInspectorWidth, 0, LayoutInspectorWidth, Rect.Height);
            inspectorWinFormsControl.Bounds = new System.Drawing.Rectangle((int)RightSectionStartX, Rect.Top + TopMargin, (int)RightSectionWidth, Rect.Height - TopMargin);
        }

        public void Draw(GameTime gt, GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font)
        {
            sb.Begin();
            sb.Draw(boxTex, Rect, Config.EnableColorBlindMode ? Color.Black : new Color(0.2f, 0.2f, 0.2f));
            sb.End();
            //throw new Exception("TaeUndoMan");

            //throw new Exception("Make left/right edges of events line up to same vertical lines so the rounding doesnt make them 1 pixel off");
            //throw new Exception("Make dragging edges of scrollbar box do zoom");
            //throw new Exception("make ctrl+scroll zoom centered on mouse cursor pos");

            UpdateLayout();
            if (editScreenAnimList != null && editScreenCurrentAnim != null)
            {
                editScreenAnimList.Draw(gd, sb, boxTex, font);
                editScreenCurrentAnim.Draw(gt, gd, sb, boxTex, font);
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
        }
    }
}
