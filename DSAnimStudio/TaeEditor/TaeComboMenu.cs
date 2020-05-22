using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio.TaeEditor
{
    public partial class TaeComboMenu : Form
    {
        public TaeEditorScreen MainScreen;

        private string[] autocompleteListAnimIDs = null;
        private string[] autocompleteListCancelTypes = null;

        public TaeComboMenu()
        {
            InitializeComponent();
        }

        private void buttonPlayCombo_Click(object sender, EventArgs e)
        {
            if (dataGridViewComboEntries.RowCount <= 1)
            {
                MessageBox.Show("Combo has nothing in it.",
                        "Empty Combo", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }

            TaeComboEntry[] combo = new TaeComboEntry[dataGridViewComboEntries.RowCount - 1];
            for (int r = 0; r < dataGridViewComboEntries.RowCount - 1; r++)
            {
                var animIDCellValue = dataGridViewComboEntries[0, r]?.Value;
                var event0CancelTypeCellValue = dataGridViewComboEntries[1, r]?.Value;

                if (animIDCellValue != null && event0CancelTypeCellValue != null)
                {
                    if (!int.TryParse(animIDCellValue.ToString().Replace("_", "").Replace("a", ""), out int animID))
                    {
                        MessageBox.Show($"\"{animIDCellValue.ToString()}\" is not a valid animation ID. Expected XXYYYY, aXX_YYYY, XXXYYYYYY, or aXXX_YYYYYY format.",
                            "Invalid Animation ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!MainScreen.DoesAnimIDExist(animID))
                    {
                        MessageBox.Show($"Animation {animIDCellValue.ToString()} does not exist.",
                            "Non-existant Animation ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!MainScreen.SelectedTae.BankTemplate[0]["JumpTableID"].EnumEntries.ContainsKey(event0CancelTypeCellValue.ToString()))
                    {
                        MessageBox.Show($"\"{event0CancelTypeCellValue.ToString()}\" is not a valid JumpTableID value. Go click a JumpTable event to see all of the possible values in the inspector.",
                          "Invalid JumpTableID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    combo[r] = new TaeComboEntry(animIDCellValue.ToString(), event0CancelTypeCellValue.ToString());
                }
                else
                {
                    MessageBox.Show("Invalid combo. Check that every row (other than the blank row at the end) has both a valid animation ID and a valid JumpTableID.", 
                        "Invalid Combo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            MainScreen.Graph.ViewportInteractor.StartCombo(checkBoxLoop.Checked, combo);
            MainScreen.GameWindowAsForm.Activate();
        }

        public void SetupTaeComboBoxes()
        {
            var jumpTableEnumValues = MainScreen?.SelectedTae?.BankTemplate?[0]?["JumpTableID"]?.EnumEntries;

            var cancelTypes = new List<string>();

            if (jumpTableEnumValues != null)
            {
                foreach (var kvp in jumpTableEnumValues)
                {
                    cancelTypes.Add(kvp.Key);
                }
            }

            autocompleteListCancelTypes = cancelTypes.ToArray();

            var anims = new List<string>();

            foreach (var taeSection in MainScreen.AnimationListScreen.AnimTaeSections)
            {
                foreach (var kvp in taeSection.InfoMap)
                {
                    anims.Add(kvp.Value.GetName());
                }
            }

            autocompleteListAnimIDs = anims.ToArray();
        }

        private void TaeComboMenu_Load(object sender, EventArgs e)
        {

        }

        //public void UpdateComboStatus()
        //{

        //}

        private void buttonCancelCombo_Click(object sender, EventArgs e)
        {
            if (MainScreen.Graph.ViewportInteractor.CurrentComboIndex >= 0)
            {
                MainScreen.Graph.ViewportInteractor.CurrentComboIndex = -1;

                MainScreen.GameWindowAsForm.Activate();
            }
        }

        private void buttonCopyToClipboard_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            for (int r = 0; r < dataGridViewComboEntries.RowCount - 1; r++)
            {
                var animIDCellValue = dataGridViewComboEntries[0, r]?.Value;
                var event0CancelTypeCellValue = dataGridViewComboEntries[1, r]?.Value;

                if (animIDCellValue != null)
                    sb.Append(animIDCellValue.ToString());
                else
                    sb.Append("None");

                sb.Append("|");

                if (event0CancelTypeCellValue != null)
                    sb.Append(event0CancelTypeCellValue.ToString());
                else
                    sb.Append("None");

                sb.AppendLine();
            }
            Clipboard.SetText(sb.ToString());
        }

        private static void ErrorInvalidClipboard()
        {
            MessageBox.Show("Invalid combo in clipboard contents.", "Invalid Combo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void buttonPasteFromClipboard_Click(object sender, EventArgs e)
        {
            var clipboardText = Clipboard.GetText();

            dataGridViewComboEntries.Rows.Clear();

            var allLines = clipboardText.Replace("\r", "").Split('\n');
            foreach (var line in allLines)
            {
                var lineTrimmed = line.Trim();

                if (string.IsNullOrWhiteSpace(lineTrimmed))
                    continue;

                var parts = lineTrimmed.Split('|').Select(x => x.Trim()).ToArray();
                if (parts.Length != 2)
                {
                    ErrorInvalidClipboard();
                    return;
                }
                
                try
                {
                    dataGridViewComboEntries.Rows.Add();
                    dataGridViewComboEntries.Rows[dataGridViewComboEntries.RowCount - 2].Cells[0].Value = parts[0];
                    dataGridViewComboEntries.Rows[dataGridViewComboEntries.RowCount - 2].Cells[1].Value = parts[1];
                }
                catch
                {
                    ErrorInvalidClipboard();
                    return;
                }

            }
        }

        private void dataGridViewComboEntries_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            int column = dataGridViewComboEntries.CurrentCell.ColumnIndex;
            if (e.Control is TextBox ctrlAsTextbox)
            {


                //ctrlAsTextbox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                //ctrlAsTextbox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                //AutoCompleteStringCollection DataCollection = new AutoCompleteStringCollection();

                //DataCollection.AddRange(column == 1 ? autocompleteListCancelTypes : autocompleteListAnimIDs);

                //ctrlAsTextbox.AutoCompleteCustomSource = DataCollection;
                AutoCompleteExt.EnableAutoSuggest(ctrlAsTextbox, column == 1 ? autocompleteListCancelTypes : autocompleteListAnimIDs);
            }

        }

        private void dataGridViewComboEntries_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var hitTestInfo = dataGridViewComboEntries.HitTest(e.X, e.Y);
                if (hitTestInfo.Type == DataGridViewHitTestType.Cell)
                    dataGridViewComboEntries.BeginEdit(true);
                else
                    dataGridViewComboEntries.EndEdit();
            }
        }

        private void dataGridViewComboEntries_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void TaeComboMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
