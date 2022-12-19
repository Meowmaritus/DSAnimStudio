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

            if (entriesTaeComboAnimType == null)
            {
                entriesTaeComboAnimType = (TaeComboAnimType[])Enum.GetValues(typeof(TaeComboAnimType));
            }
        }

        public static List<int> ListOfKnownTransitionalEvent0s = new List<int>
        {
            4, 11, 16, 22, 23, 26, 30, 31, 32, 68, 78, 79, 86,
        };

        public enum TaeComboAnimType : int
        {
            NoCancel = -1,
            PlayerRH = 4,
            PlayerMove = 11,
            PlayerLH = 16,
            PlayerGuard = 22,
            EnemyComboAtk = 23,
            PlayerDodge = 26,
            PlayerEstus = 30,
            PlayerItem = 31,
            PlayerWeaponSwitch = 32,
            ThrowEscape = 68,
            EnemyMove = 78,
            EnemyDodge = 79,
            EnemyAtk = 86,

            SetRootMotionMultXZ = -2,
            SetRootMotionMultY = -3,
            SetPlaybackSpeed = -4,
            SetBlend = -5,
            SetUpperBodyBlend = -6,
            //SetHideWeapons = -7,
            SetBlendSine = -8,
            SetUpperBodyBlendSine = -9,

            SetAdditiveAnim = -100,
            SetUpperBodyAnim = -101,


        }

        TaeComboAnimType[] entriesTaeComboAnimType = null;
        Dictionary<string, TaeComboAnimType> lowercaseMapping = null;


        private void buttonPlayCombo_Click(object sender, EventArgs e)
        {
            var combo = new List<TaeComboEntry>();


            

            if (lowercaseMapping == null)
            {
                lowercaseMapping = new Dictionary<string, TaeComboAnimType>();
                foreach (var entry in entriesTaeComboAnimType)
                {
                    lowercaseMapping.Add(entry.ToString().ToLower(), entry);
                }
            }

            var validLineList = textBoxComboSeq.Lines.Select(x => x.Trim().ToLower()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            int firstEvent0CancelType = -1;

            int indexOfLastThingThatWasAnAnimation = -1;

            for (int i = 0; i < validLineList.Count; i++)
            {
                var split = validLineList[i]
                    .Split(' ')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x
                        .Replace("\n", "")
                        .Replace("\r", "")
                        .Trim())
                    .ToList();

                if (split.Count >= 2)
                {
                    var cancelTypeKey = split[0].ToLower();
                    if (lowercaseMapping.ContainsKey(cancelTypeKey))
                    {
                        var cancelBeforeNext = lowercaseMapping[cancelTypeKey];

                        if (cancelTypeKey == "setadditiveanim")
                        {
                            throw new NotImplementedException();

                            if (float.TryParse(split[1], out float setFloat))
                            {
                                combo.Add(new TaeComboEntry(cancelBeforeNext, setFloat));
                            }
                            else
                            {
                                MessageBox.Show($"\"{split[1]}\" is not a valid floating point value, which is what {cancelBeforeNext} expects.",
                                       "Invalid Float", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                        }
                        else if (cancelTypeKey == "setupperbodyanim")
                        {
                            if (int.TryParse(split[1], out int animID))
                            {
                                float startTime = -1;
                                if (split.Count >= 3)
                                {
                                    if (float.TryParse(split[2], out float upperBodyAnim_StartTime))
                                    {
                                        startTime = upperBodyAnim_StartTime;
                                    }
                                    else
                                    {
                                        MessageBox.Show($"\"{split[2]}\" is not a valid floating point value, which is what SetUpperBodyAnim's second arg expects.",
                                       "Invalid Float", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                                combo.Add(new TaeComboEntry(cancelBeforeNext, animID, startTime));
                            }
                            else
                            {
                                MessageBox.Show($"\"{split[1]}\" is not a valid integer value, which is what SetUpperBodyAnim's first arg expects.",
                                       "Invalid Int", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                        }
                        else if (cancelTypeKey.StartsWith("set"))
                        {
                            if (float.TryParse(split[1], out float setFloat))
                            {
                                combo.Add(new TaeComboEntry(cancelBeforeNext, setFloat));
                            }
                            else
                            {
                                MessageBox.Show($"\"{split[1]}\" is not a valid floating point value, which is what {cancelBeforeNext} expects.",
                                       "Invalid Float", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                        }
                        else
                        {
                            if (combo.Count > 0 && indexOfLastThingThatWasAnAnimation >= 0)
                                combo[indexOfLastThingThatWasAnAnimation].Event0CancelType = (int)cancelBeforeNext;
                            else
                                firstEvent0CancelType = (int)cancelBeforeNext;

                            indexOfLastThingThatWasAnAnimation = i;

                            float startFrame = -1;
                            float endFrame = -1;

                            if (!long.TryParse(split[1].Replace("_", "").Replace("a", ""), out long animID))
                            {
                                MessageBox.Show($"\"{split[1]}\" is not a valid animation ID. Expected XXYYYY, aXX_YYYY, XXXYYYYYY, or aXXX_YYYYYY format.",
                                    "Invalid Animation ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            if (split.Count >= 3)
                            {
                                if (!float.TryParse(split[2], out float enteredStartFrame))
                                {
                                    MessageBox.Show($"\"{split[2]}\" is not a valid starting frame number.",
                                        "Invalid Start Frame", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                                else
                                {
                                    startFrame = enteredStartFrame;

                                    if (split.Count >= 4)
                                    {
                                        if (!float.TryParse(split[3], out float enteredEndFrame))
                                        {
                                            MessageBox.Show($"\"{split[3]}\" is not a valid ending frame number.",
                                                "Invalid End Frame", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            return;
                                        }
                                        else
                                        {
                                            endFrame = enteredEndFrame;
                                        }
                                    }
                                }

                            }

                            combo.Add(new TaeComboEntry(cancelBeforeNext, animID, i == validLineList.Count - 1 ? firstEvent0CancelType : -1, startFrame, endFrame));
                        }

                        
                    }
                }

                
            }

            if (combo.Count > 1)
            {
                combo[combo.Count - 1].Event0CancelType = firstEvent0CancelType;
            }

            //if (dataGridViewComboEntries.RowCount <= 1)
            //{
            //    MessageBox.Show("Combo has nothing in it.",
            //            "Empty Combo", MessageBoxButtons.OK, MessageBoxIcon.None);
            //    return;
            //}

            //TaeComboEntry[] combo = new TaeComboEntry[dataGridViewComboEntries.RowCount - 1];
            //for (int r = 0; r < dataGridViewComboEntries.RowCount - 1; r++)
            //{
            //    var animIDCellValue = dataGridViewComboEntries[0, r]?.Value;
            //    var event0CancelTypeCellValue = dataGridViewComboEntries[1, r]?.Value;

            //    if (animIDCellValue != null && event0CancelTypeCellValue != null)
            //    {
            //        if (!int.TryParse(animIDCellValue.ToString().Replace("_", "").Replace("a", ""), out int animID))
            //        {
            //            MessageBox.Show($"\"{animIDCellValue.ToString()}\" is not a valid animation ID. Expected XXYYYY, aXX_YYYY, XXXYYYYYY, or aXXX_YYYYYY format.",
            //                "Invalid Animation ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //            return;
            //        }

            //        if (!MainScreen.DoesAnimIDExist(animID))
            //        {
            //            MessageBox.Show($"Animation {animIDCellValue.ToString()} does not exist.",
            //                "Non-existant Animation ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //            return;
            //        }

            //        if (!MainScreen.SelectedTae.BankTemplate[0]["JumpTableID"].EnumEntries.ContainsKey(event0CancelTypeCellValue.ToString()))
            //        {
            //            MessageBox.Show($"\"{event0CancelTypeCellValue.ToString()}\" is not a valid JumpTableID value. Go click a JumpTable event to see all of the possible values in the inspector.",
            //              "Invalid JumpTableID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //            return;
            //        }

            //        combo[r] = new TaeComboEntry(animIDCellValue.ToString(), event0CancelTypeCellValue.ToString());
            //    }
            //    else
            //    {
            //        MessageBox.Show("Invalid combo. Check that every row (other than the blank row at the end) has both a valid animation ID and a valid JumpTableID.", 
            //            "Invalid Combo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //}

            //Disable recording
#if !DEBUG
            checkBoxRecord.Checked = false;
            checkBoxRecord60FPS.Checked = false;
            checkBoxRecordHkxOnly.Checked = false;
#endif

            MainScreen.Graph.ViewportInteractor.StartCombo(checkBoxLoop.Checked, checkBoxRecord.Checked, combo.ToArray(), checkBoxRecordHkxOnly.Checked, checkBoxRecord60FPS.Checked);
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

            foreach (var taeSection in MainScreen.AnimationListScreen.AnimTaeSections.Values)
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
            //Disable recording
#if !DEBUG
            checkBoxRecord.Visible = false;
            checkBoxRecord60FPS.Visible = false;
            checkBoxRecordHkxOnly.Visible = false;
            checkBoxRecord.Enabled = false;
            checkBoxRecord60FPS.Enabled = false;
            checkBoxRecordHkxOnly.Enabled = false;
#endif
        }

        //public void UpdateComboStatus()
        //{

        //}

        private void buttonCancelCombo_Click(object sender, EventArgs e)
        {
            if (MainScreen.Graph.ViewportInteractor.CurrentComboIndex >= 0)
            {
                MainScreen.Graph.ViewportInteractor.CancelCombo();

                MainScreen.GameWindowAsForm.Activate();
            }
        }

        //private void buttonCopyToClipboard_Click(object sender, EventArgs e)
        //{
        //    var sb = new StringBuilder();
        //    for (int r = 0; r < dataGridViewComboEntries.RowCount - 1; r++)
        //    {
        //        var animIDCellValue = dataGridViewComboEntries[0, r]?.Value;
        //        var event0CancelTypeCellValue = dataGridViewComboEntries[1, r]?.Value;

        //        if (animIDCellValue != null)
        //            sb.Append(animIDCellValue.ToString());
        //        else
        //            sb.Append("None");

        //        sb.Append("|");

        //        if (event0CancelTypeCellValue != null)
        //            sb.Append(event0CancelTypeCellValue.ToString());
        //        else
        //            sb.Append("None");

        //        sb.AppendLine();
        //    }
        //    Clipboard.SetText(sb.ToString());
        //}

        //private static void ErrorInvalidClipboard()
        //{
        //    MessageBox.Show("Invalid combo in clipboard contents.", "Invalid Combo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //}

        //private void buttonPasteFromClipboard_Click(object sender, EventArgs e)
        //{
        //    var clipboardText = Clipboard.GetText();

        //    dataGridViewComboEntries.Rows.Clear();

        //    var allLines = clipboardText.Replace("\r", "").Split('\n');
        //    foreach (var line in allLines)
        //    {
        //        var lineTrimmed = line.Trim();

        //        if (string.IsNullOrWhiteSpace(lineTrimmed))
        //            continue;

        //        var parts = lineTrimmed.Split('|').Select(x => x.Trim()).ToArray();
        //        if (parts.Length != 2)
        //        {
        //            ErrorInvalidClipboard();
        //            return;
        //        }
                
        //        try
        //        {
        //            dataGridViewComboEntries.Rows.Add();
        //            dataGridViewComboEntries.Rows[dataGridViewComboEntries.RowCount - 2].Cells[0].Value = parts[0];
        //            dataGridViewComboEntries.Rows[dataGridViewComboEntries.RowCount - 2].Cells[1].Value = parts[1];
        //        }
        //        catch
        //        {
        //            ErrorInvalidClipboard();
        //            return;
        //        }

        //    }
        //}

        //private void dataGridViewComboEntries_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        //{
        //    int column = dataGridViewComboEntries.CurrentCell.ColumnIndex;
        //    if (e.Control is TextBox ctrlAsTextbox)
        //    {


        //        //ctrlAsTextbox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        //        //ctrlAsTextbox.AutoCompleteSource = AutoCompleteSource.CustomSource;
        //        //AutoCompleteStringCollection DataCollection = new AutoCompleteStringCollection();

        //        //DataCollection.AddRange(column == 1 ? autocompleteListCancelTypes : autocompleteListAnimIDs);

        //        //ctrlAsTextbox.AutoCompleteCustomSource = DataCollection;
        //        AutoCompleteExt.EnableAutoSuggest(ctrlAsTextbox, column == 1 ? autocompleteListCancelTypes : autocompleteListAnimIDs);
        //    }

        //}

        //private void dataGridViewComboEntries_MouseUp(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        var hitTestInfo = dataGridViewComboEntries.HitTest(e.X, e.Y);
        //        if (hitTestInfo.Type == DataGridViewHitTestType.Cell)
        //            dataGridViewComboEntries.BeginEdit(true);
        //        else
        //            dataGridViewComboEntries.EndEdit();
        //    }
        //}

        //private void dataGridViewComboEntries_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        //{
            
        //}

        private void TaeComboMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        //private void dataGridViewComboEntries_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
        //{
        //    //e.Row.Cells[1].Value = 
        //}
    }
}
