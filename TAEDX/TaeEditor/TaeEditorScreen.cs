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

        private static object _lock_PauseUpdate = new object();
        private bool PauseUpdate;

        public Rectangle Rect;

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

        public string TaeFileName = "";

        public void LoadCurrentFile()
        {
            if (System.IO.File.Exists(TaeFileName))
            {
                var newTae = MeowDSIO.DataFile.LoadFromFile<TAE>(TaeFileName);
                LoadTAE(newTae);
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

            GameWindowAsForm.Controls.Add(inspectorWinFormsControl);

            var toolstripFile = new System.Windows.Forms.ToolStripMenuItem("File");

            var toolstripFile_Open = new System.Windows.Forms.ToolStripMenuItem("Open");
            toolstripFile_Open.Click += ToolstripFile_Open_Click;
            toolstripFile.DropDownItems.Add(toolstripFile_Open);

            toolstripFile.DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());

            var toolstripFile_Save = new System.Windows.Forms.ToolStripMenuItem("Save");
            toolstripFile_Save.Click += ToolstripFile_Save_Click;
            toolstripFile.DropDownItems.Add(toolstripFile_Save);

            var toolstripFile_SaveAs = new System.Windows.Forms.ToolStripMenuItem("Save As...");
            toolstripFile_SaveAs.Click += ToolstripFile_SaveAs_Click;
            toolstripFile.DropDownItems.Add(toolstripFile_SaveAs);

            var menuStrip = new System.Windows.Forms.MenuStrip();
            menuStrip.Items.Add(toolstripFile);

            GameWindowAsForm.Controls.Add(menuStrip);
        }

        private void ToolstripFile_Open_Click(object sender, EventArgs e)
        {
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
            lock (_lock_PauseUpdate)
            {
                PauseUpdate = true;
            }
            var changeTypeDlg = new TaeInspectorFormChangeEventType();
            if (changeTypeDlg.ShowDialog(GameWindowAsForm) == System.Windows.Forms.DialogResult.OK)
            {
                if (changeTypeDlg.NewEventType != SelectedEventBox.MyEvent.EventType)
                {
                    TaeAnim.Anim.EventList.Remove(SelectedEventBox.MyEvent);
                    int index = SelectedEventBox.MyEvent.Index;
                    SelectedEventBox.MyEvent = TimeActEventBase.GetNewEvent(changeTypeDlg.NewEventType, SelectedEventBox.MyEvent.StartTimeFr, SelectedEventBox.MyEvent.EndTimeFr);
                    TaeAnim.Anim.EventList.Insert(index, SelectedEventBox.MyEvent);

                    //Force Refresh lol
                    SelectedEventBox = SelectedEventBox;
                }
            }
            lock (_lock_PauseUpdate)
            {
                PauseUpdate = false;
            }
        }

        public void SelectNewAnimRef(AnimationRef animRef)
        {
            TaeAnim = animRef;
            SelectedEventBox = null;
            editScreenCurrentAnim.ChangeToNewAnimRef(TaeAnim);
        }

        public void Update(float elapsedSeconds)
        {
            lock (_lock_PauseUpdate)
            {
                if (PauseUpdate)
                    return;
            }

            Input.Update(Rect);

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

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font)
        {
            //throw new Exception("Make left/right edges of events line up to same vertical lines so the rounding doesnt make them 1 pixel off");
            //throw new Exception("Make dragging edges of scrollbar box do zoom");
            //throw new Exception("make ctrl+scroll zoom centered on mouse cursor pos");

            UpdateLayout();
            if (editScreenAnimList != null && editScreenCurrentAnim != null)
            {
                editScreenAnimList.Draw(gd, sb, boxTex, font);
                editScreenCurrentAnim.Draw(gd, sb, boxTex, font);
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
