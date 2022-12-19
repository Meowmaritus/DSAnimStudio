using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio
{
    public partial class NewChrAsmEquipForm : Form
    {
        public NewChrAsm ChrAsm;

        public List<int> ParamID_HD = new List<int>();
        public List<int> ParamID_BD = new List<int>();
        public List<int> ParamID_AM = new List<int>();
        public List<int> ParamID_LG = new List<int>();
        public List<int> ParamID_WPR = new List<int>();
        public List<int> ParamID_WPL = new List<int>();

        public SoulsAssetPipeline.SoulsGames GameTypesParamsAndFmgsWereLoadedFrom = SoulsAssetPipeline.SoulsGames.None;

        public NewChrAsmEquipForm()
        {
            InitializeComponent();
            comboBoxWPRIndex.SelectedIndex = 1;
            ApplyCurrentParamsAndFmgs();

            menuStrip1.Renderer = new DarkToolStripRenderer();
        }

        public event EventHandler Hidden;

        protected void OnHidden()
        {
            Hidden?.Invoke(this, EventArgs.Empty);
        }

        private void NewChrAsmEquipForm_Load(object sender, EventArgs e)
        {
            
        }

        public void ApplyCurrentParamsAndFmgs()
        {
            void DoParamList(ComboBox cb, List<int> idList, Dictionary<int, string> paramNames)
            {
                cb.Items.Clear();
                idList.Clear();
                foreach (var kvp in paramNames)
                {
                    cb.Items.Add(kvp.Value);
                    idList.Add(kvp.Key);
                }
                if (cb.Items.Count > 0)
                    cb.SelectedIndex = 0;
            }

            DoParamList(comboBoxHD, ParamID_HD, FmgManager.ProtectorNames_HD);
            DoParamList(comboBoxBD, ParamID_BD, FmgManager.ProtectorNames_BD);
            DoParamList(comboBoxAM, ParamID_AM, FmgManager.ProtectorNames_AM);
            DoParamList(comboBoxLG, ParamID_LG, FmgManager.ProtectorNames_LG);
            DoParamList(comboBoxWPR, ParamID_WPR, FmgManager.WeaponNames);
            DoParamList(comboBoxWPL, ParamID_WPL, FmgManager.WeaponNames);

            GameTypesParamsAndFmgsWereLoadedFrom = GameRoot.GameType;

            //comboBoxHD.ed
        }

        public void WriteChrAsmToGUI()
        {
            comboBoxHD.SelectedIndex = ParamID_HD.IndexOf(ChrAsm.HeadID);
            comboBoxBD.SelectedIndex = ParamID_BD.IndexOf(ChrAsm.BodyID);
            comboBoxAM.SelectedIndex = ParamID_AM.IndexOf(ChrAsm.ArmsID);
            comboBoxLG.SelectedIndex = ParamID_LG.IndexOf(ChrAsm.LegsID);
            comboBoxWPR.SelectedIndex = ParamID_WPR.IndexOf(ChrAsm.RightWeaponID);
            comboBoxWPL.SelectedIndex = ParamID_WPL.IndexOf(ChrAsm.LeftWeaponID);
            comboBoxWPRIndex.SelectedIndex = (int)ChrAsm.StartWeaponStyle;
            //checkBoxRWeaponFlipBackwards.Checked = ChrAsm.RightWeaponFlipBackwards;
            //checkBoxRWeaponFlipSideways.Checked = ChrAsm.RightWeaponFlipSideways;
            //checkBoxLWeaponFlipBackwards.Checked = ChrAsm.LeftWeaponFlipBackwards;
            //checkBoxLWeaponFlipSideways.Checked = ChrAsm.LeftWeaponFlipSideways;

            checkBoxDbgRHMdlPos.Checked = ChrAsm.DebugRightWeaponModelPositions;
            checkBoxDbgLHMdlPos.Checked = ChrAsm.DebugLeftWeaponModelPositions;
        }

        public void WriteGUIToChrAsm()
        {
            ChrAsm.HeadID = comboBoxHD.SelectedIndex >= 0 ? ParamID_HD[comboBoxHD.SelectedIndex] : -1;
            ChrAsm.BodyID = comboBoxBD.SelectedIndex >= 0 ? ParamID_BD[comboBoxBD.SelectedIndex] : -1;
            ChrAsm.ArmsID = comboBoxAM.SelectedIndex >= 0 ? ParamID_AM[comboBoxAM.SelectedIndex] : -1;
            ChrAsm.LegsID = comboBoxLG.SelectedIndex >= 0 ? ParamID_LG[comboBoxLG.SelectedIndex] : -1;
            ChrAsm.RightWeaponID = comboBoxWPR.SelectedIndex >= 0 ? ParamID_WPR[comboBoxWPR.SelectedIndex] : -1;
            ChrAsm.LeftWeaponID = comboBoxWPL.SelectedIndex >= 0 ? ParamID_WPL[comboBoxWPL.SelectedIndex] : -1;
            ChrAsm.StartWeaponStyle = (NewChrAsm.WeaponStyleType)comboBoxWPRIndex.SelectedIndex;

            ChrAsm.DebugRightWeaponModelPositions = checkBoxDbgRHMdlPos.Checked;
            ChrAsm.DebugLeftWeaponModelPositions = checkBoxDbgLHMdlPos.Checked;
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                WriteGUIToChrAsm();
                var jsonThing = new NewChrAsmCfgJson();
                jsonThing.CopyFromChrAsm(ChrAsm);
                var jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(jsonThing);

                var dialog = new SaveFileDialog()
                {
                    Filter = "JSON Files (*.json)|*.json",
                    FileName = $"{GameRoot.GameType}_NewCharacter01.json",
                    Title = "Export c0000 equipment to JSON file...",
                    OverwritePrompt = true,
                };

                var result = dialog.ShowDialog(this);

                if (result == DialogResult.OK || result == DialogResult.Yes)
                {
                    System.IO.File.WriteAllText(dialog.FileName, jsonText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while exporting equipment (please report):\n\n\n{ex}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog()
                {
                    Filter = "JSON Files (*.json)|*.json",
                    Title = "Import c0000 equipment from JSON file...",
                };

                var result = dialog.ShowDialog(this);

                if (result == DialogResult.OK || result == DialogResult.Yes)
                {
                    var jsonText = System.IO.File.ReadAllText(dialog.FileName);

                    var jsonThing = Newtonsoft.Json.JsonConvert.DeserializeObject<NewChrAsmCfgJson>(jsonText);

                    if (!GameRoot.CheckGameTypeParamIDCompatibility(GameRoot.GameType, jsonThing.GameType))
                    {
                        MessageBox.Show($"Cannot import equipment. " +
                            $"Equipment IDs in file are for " +
                            $"{GameRoot.GameTypeNames[jsonThing.GameType]}, " +
                            $"but the GameParam and text currently loaded are those " +
                            $"of {GameRoot.GameTypeNames[GameRoot.GameType]}", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }

                    jsonThing.WriteToChrAsm(ChrAsm);

                    WriteChrAsmToGUI();

                    SetEverythingDisabled(true);
                    ChrAsm.UpdateModels(isAsync: true, onCompleteAction: () =>
                    {
                        Invoke(new Action(() =>
                        {
                            SetEverythingDisabled(false);
                        }));
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while importing equipment (please report):\n\n\n{ex}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckBoxRWeaponFlipBackwards_CheckedChanged(object sender, EventArgs e)
        {
            //ChrAsm.RightWeaponFlipBackwards = checkBoxRWeaponFlipBackwards.Checked;
            ChrAsm.MODEL.AfterAnimUpdate(0);
            Main.WinForm.Activate();
        }

        private void CheckBoxRWeaponFlipSideways_CheckedChanged(object sender, EventArgs e)
        {
            //ChrAsm.RightWeaponFlipSideways = checkBoxRWeaponFlipSideways.Checked;
            ChrAsm.MODEL.AfterAnimUpdate(0);
            Main.WinForm.Activate();
        }

        private void CheckBoxLWeaponFlipBackwards_CheckedChanged(object sender, EventArgs e)
        {
            //ChrAsm.LeftWeaponFlipBackwards = checkBoxLWeaponFlipBackwards.Checked;
            ChrAsm.MODEL.AfterAnimUpdate(0);
            Main.WinForm.Activate();
        }

        private void CheckBoxLWeaponFlipSideways_CheckedChanged(object sender, EventArgs e)
        {
            //ChrAsm.LeftWeaponFlipSideways = checkBoxLWeaponFlipSideways.Checked;
            ChrAsm.MODEL.AfterAnimUpdate(0);
            Main.WinForm.Activate();
        }

        private void SetEverythingDisabled(bool isDisabled)
        {
            comboBoxHD.Enabled = !isDisabled;
            comboBoxBD.Enabled = !isDisabled;
            comboBoxAM.Enabled = !isDisabled;
            comboBoxLG.Enabled = !isDisabled;

            comboBoxWPL.Enabled = !isDisabled;
            //comboBoxWPLIndex.Enabled = !isDisabled;
            checkBoxLWeaponFlipBackwards.Enabled = !isDisabled;
            checkBoxLWeaponFlipSideways.Enabled = !isDisabled;

            comboBoxWPR.Enabled = !isDisabled;
            comboBoxWPRIndex.Enabled = !isDisabled;
            checkBoxRWeaponFlipBackwards.Enabled = !isDisabled;
            checkBoxRWeaponFlipSideways.Enabled = !isDisabled;

            checkBoxDbgRHMdlPos.Enabled = !isDisabled;
            checkBoxDbgLHMdlPos.Enabled = !isDisabled;

            menuStrip1.Enabled = !isDisabled;
            buttonApplyChanges.Enabled = !isDisabled;
        }

        private void ButtonApplyChanges_Click(object sender, EventArgs e)
        {
            SetEverythingDisabled(true);

            Scene.DisableModelDrawing();
            Scene.DisableModelDrawing2();
            WriteGUIToChrAsm();
            ChrAsm.UpdateModels(isAsync: true, onCompleteAction: () =>
            {
                
                Thread.Sleep(100);

                Scene.EnableModelDrawing();
                Scene.EnableModelDrawing2();

                // I'm sorry
                Main.TAE_EDITOR.HardReset();
                Invoke(new Action(() =>
                {
                    SetEverythingDisabled(false);
                    Main.WinForm.Activate();
                }));
            });
        }

        private void NewChrAsmEquipForm_Shown(object sender, EventArgs e)
        {
            WriteChrAsmToGUI();

            //if (GameTypesParamsAndFmgsWereLoadedFrom != GameDataManager.GameType)
            //{
            //    ApplyCurrentParamsAndFmgs();
            //}
        }

        private void NewChrAsmEquipForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
            OnHidden();
        }

        static System.Drawing.Color DarkBackColor = Color.FromArgb(255, 64, 64, 64);
        static System.Drawing.Color DarkBackColor_Selected = System.Drawing.Color.FromArgb(255, 40, 40, 40);
        static System.Drawing.Color OutlineColor = System.Drawing.Color.FromArgb(255, 80, 80, 80);
        static System.Drawing.Color DarkForeColor = Color.White;

        static int BorderThickness = 1;

        static System.Drawing.Bitmap CustomCheck = null;

        class DarkToolStripRenderer : ToolStripSystemRenderer
        {
            protected override void InitializeItem(ToolStripItem item)
            {
                base.InitializeItem(item);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                using (var brush = new System.Drawing.SolidBrush(DarkBackColor))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, e.Item.Bounds.Width, e.Item.Bounds.Height);
                }

                using (var brush = new System.Drawing.SolidBrush(DarkBackColor_Selected))
                {
                    e.Graphics.FillRectangle(brush, 0, (e.Item.Bounds.Height / 2.0f) - 1, e.Item.Bounds.Width, 2);
                }
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Selected)
                {
                    using (var brush = new System.Drawing.SolidBrush(DarkBackColor_Selected))
                    {
                        e.Graphics.FillRectangle(brush, -2, -2, e.Item.Bounds.Width + 4, e.Item.Bounds.Height + 4);
                    }
                }
                else
                {
                    using (var brush = new System.Drawing.SolidBrush(DarkBackColor))
                    {
                        e.Graphics.FillRectangle(brush, -2, -2, e.Item.Bounds.Width + 4, e.Item.Bounds.Height + 4);
                    }
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                //// IM SO SORRY
                //if (e.Item.Text == "NPC Settings" || e.Item.Text == "Player Settings")
                //{
                //    e.Item.ForeColor = System.Drawing.Color.Cyan;
                //    base.OnRenderItemText(e);
                //    return;
                //}

                if (!e.Item.Enabled)
                {
                    //using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Gray))
                    //    e.Graphics.DrawString(e.Item.Text, e.Item.Font, brush, e.Item.Padding.Size.Width, e.Item.Padding.Top);

                    if (e.Item.Selected)
                    {
                        e.Item.ForeColor = System.Drawing.Color.Black;
                    }
                    else
                    {
                        e.Item.ForeColor = System.Drawing.Color.Gray;
                    }

                }
                else
                {
                    e.Item.ForeColor = DarkForeColor;

                    //using (var brush = new System.Drawing.SolidBrush(ForeColor))
                    //    e.Graphics.DrawString(e.Item.Text, e.Item.Font, brush, e.Item.Padding.Size.Width, e.Item.Padding.Top);
                }

                base.OnRenderItemText(e);
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                //base.OnRenderToolStripBackground(e);
                using (var brush = new System.Drawing.SolidBrush(DarkBackColor))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, e.ToolStrip.Bounds.Width, e.ToolStrip.Bounds.Height);
                }

                using (var brush = new System.Drawing.SolidBrush(OutlineColor))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, e.ToolStrip.Bounds.Width, BorderThickness);
                    e.Graphics.FillRectangle(brush, 0, 0, BorderThickness, e.ToolStrip.Bounds.Height);
                    e.Graphics.FillRectangle(brush, 0, e.ToolStrip.Bounds.Height - BorderThickness, e.ToolStrip.Bounds.Width, BorderThickness);
                    e.Graphics.FillRectangle(brush, e.ToolStrip.Bounds.Width - BorderThickness, 0, BorderThickness, e.ToolStrip.Bounds.Height);
                }
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                using (var brush = new System.Drawing.SolidBrush(OutlineColor))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, e.ToolStrip.Bounds.Width, BorderThickness);
                    e.Graphics.FillRectangle(brush, 0, 0, BorderThickness, e.ToolStrip.Bounds.Height);
                    e.Graphics.FillRectangle(brush, 0, e.ToolStrip.Bounds.Height - BorderThickness, e.ToolStrip.Bounds.Width, BorderThickness);
                    e.Graphics.FillRectangle(brush, e.ToolStrip.Bounds.Width - BorderThickness, 0, BorderThickness, e.ToolStrip.Bounds.Height);
                }
                //base.OnRenderToolStripBorder(e);
            }

            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
            {
                if (CustomCheck == null)
                {
                    CustomCheck = new System.Drawing.Bitmap(e.Image);
                    for (int y = 0; (y <= (CustomCheck.Height - 1)); y++)
                    {
                        for (int x = 0; (x <= (CustomCheck.Width - 1)); x++)
                        {
                            var c = CustomCheck.GetPixel(x, y);
                            CustomCheck.SetPixel(x, y, System.Drawing.Color.FromArgb(c.A, 255 - c.R, 255 - c.G, 255 - c.B));
                        }
                    }
                }

                e.Graphics.DrawImage(CustomCheck, e.ImageRectangle);
            }
        }

        private void checkBoxDbgRHMdlPos_CheckedChanged(object sender, EventArgs e)
        {
            ChrAsm.DebugRightWeaponModelPositions = checkBoxDbgRHMdlPos.Checked;
            ChrAsm.MODEL.AfterAnimUpdate(0);
            Main.WinForm.Activate();
        }

        private void checkBoxDbgLHMdlPos_CheckedChanged(object sender, EventArgs e)
        {
            ChrAsm.DebugLeftWeaponModelPositions = checkBoxDbgLHMdlPos.Checked;
            ChrAsm.MODEL.AfterAnimUpdate(0);
            Main.WinForm.Activate();
        }
    }
}
