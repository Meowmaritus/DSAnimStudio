using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Windows.Forms;

namespace DSAnimStudio.TaeEditor
{
    public partial class TaeEditAnimPropertiesForm : Form
    {
        public bool IsAbsoluteAnimID;

        TAE.Animation AnimRef;

        readonly long originalID;

        readonly TAE.Animation.AnimMiniHeader originalMiniHeader;

        readonly string originalDisplayName;

        private bool ReadyToExit = false;

        public bool WereThingsChanged = false;

        public bool WasAnimIDChanged = false;

        public bool WasAnimDeleted = false;

        public TAE.Animation.MiniHeaderType CurrentMiniHeaderType
        {
            get
            {
                if (radioButtonMHStandard.Checked)
                    return TAE.Animation.MiniHeaderType.Standard;
                else if (radioButtonMHImportOtherAnimation.Checked)
                    return TAE.Animation.MiniHeaderType.ImportOtherAnim;
                else
                    throw new NotImplementedException("Mini header type not implemented");
            }
        }

        private (long Upper, long Lower) GetSplitAnimID(long id)
        {
            return ((GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB ||
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
                ? (id / 1000000) : (id / 10000),
                (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB ||
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
                ? (id % 1000000) : (id % 10000));
        }

        private string HKXNameFromCompositeID(long compositeID)
        {
            if (compositeID < 0)
                return "";

            var splitID = GetSplitAnimID(compositeID);

            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB ||
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
            {
                return $"a{splitID.Upper:D3}_{splitID.Lower:D6}";
            }
            else
            {
                return $"a{splitID.Upper:D2}_{splitID.Lower:D4}";
            }
        }

        private void MiniHeaderLoadValuesToGUI(TAE.Animation.AnimMiniHeader miniHeader)
        {
            if (miniHeader is TAE.Animation.AnimMiniHeader.Standard asStandard)
            {
                checkBoxMHStandardImportEvents.Checked = asStandard.AllowDelayLoad;
                checkBoxMHStandardImportHKX.Checked = asStandard.ImportsHKX;
                checkBoxMHStandardLoopByDefault.Checked = asStandard.IsLoopByDefault;
                textBoxMHStandardImportHKXFrom.Text = HKXNameFromCompositeID(asStandard.ImportHKXSourceAnimID);
            }
            else if (miniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOtherAnim)
            {
                textBoxMHDuplicateSourceAnimID.Text = HKXNameFromCompositeID(asImportOtherAnim.ImportFromAnimID);
                textBoxMHDuplicateUnk.Text = asImportOtherAnim.Unknown.ToString();
            }
            else
            {
                throw new NotImplementedException("MiniHeaderType not implemented.");
            }
        }

        private string CurFormattingString() => (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB ||
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3) ? "aXXX_YYYYY" : "aXX_YYYY";

        private TAE.Animation.AnimMiniHeader MiniHeaderSaveValuesFromGUI()
        {
            if (CurrentMiniHeaderType == TAE.Animation.MiniHeaderType.Standard)
            {
                var asStandard = new TAE.Animation.AnimMiniHeader.Standard();

                asStandard.AllowDelayLoad = checkBoxMHStandardImportEvents.Checked;
                asStandard.ImportsHKX = checkBoxMHStandardImportHKX.Checked;
                asStandard.IsLoopByDefault = checkBoxMHStandardLoopByDefault.Checked;

                if (string.IsNullOrWhiteSpace(textBoxMHStandardImportHKXFrom.Text))
                {
                    asStandard.ImportHKXSourceAnimID = -1;
                }
                else if (uint.TryParse(textBoxMHStandardImportHKXFrom.Text.Replace("_", "").Replace("a", ""), out uint importFromID))
                {
                    asStandard.ImportHKXSourceAnimID = (int)importFromID;
                }
                else
                {
                    MessageBox.Show("Invalid import from animation ID formatting. " +
                        $"Expected either a blank value for none or '{CurFormattingString()}' " +
                        "type formatting, where X and Y are integers.",
                    "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                return asStandard;
            }
            else if (CurrentMiniHeaderType == TAE.Animation.MiniHeaderType.ImportOtherAnim)
            {
                var asImportOtherAnim = new TAE.Animation.AnimMiniHeader.ImportOtherAnim();

                if (string.IsNullOrWhiteSpace(textBoxMHDuplicateSourceAnimID.Text))
                {
                    asImportOtherAnim.ImportFromAnimID = -1;
                }
                else if (uint.TryParse(textBoxMHDuplicateSourceAnimID.Text.Replace("_", "").Replace("a", ""), out uint importFromID))
                {
                    asImportOtherAnim.ImportFromAnimID = (int)importFromID;
                }
                else
                {
                    MessageBox.Show("Invalid import from animation ID formatting. " +
                        $"Expected either a blank value for none or '{CurFormattingString()}' " +
                        "type formatting, where X and Y are integers.",
                    "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                if (int.TryParse(textBoxMHDuplicateUnk.Text, out int unknownInt))
                {
                    asImportOtherAnim.Unknown = unknownInt;
                }
                else
                {
                    MessageBox.Show("Invalid Unknown value format. Expected an unsigned integer.",
                    "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                return asImportOtherAnim;
            }
            else
            {
                throw new NotImplementedException("MiniHeaderType not implemented.");
            }
        }

        private void MiniHeaderPrepareGUI(TAE.Animation.MiniHeaderType miniHeaderType)
        {
            if (miniHeaderType == TAE.Animation.MiniHeaderType.Standard)
            {
                //groupBoxMHStandard.Text = "Mini-Header Data: Standard";

                //checkBoxMHStandardImportEvents.Enabled = true;
                //checkBoxMHStandardImportEvents.Visible = true;

                //checkBoxMHStandardImportHKX.Enabled = true;
                //checkBoxMHStandardImportHKX.Visible = true;

                //checkBoxMHStandardLoopByDefault.Enabled = true;
                //checkBoxMHStandardLoopByDefault.Visible = true;

                groupBoxMHStandard.Enabled = true;
                groupBoxMHDuplicate.Enabled = false;

                textBoxMHDuplicateSourceAnimID.Text = "";
                textBoxMHDuplicateUnk.Text = "0";

                //radioButtonMHStandard.Checked = true;
                //radioButtonMHImportOtherAnimation.Checked = false;

                //textBoxMHStandardImportHKXFrom.Enabled = checkBoxMHStandardImportHKX.Checked;

                //CurrentMiniHeaderType = miniHeaderType;
                //checkBoxMHStandardImportHKX.Text = "Import the model animation data (.HKX) from this animation ID:";

            }
            else if (miniHeaderType == TAE.Animation.MiniHeaderType.ImportOtherAnim)
            {
                //groupBoxMHStandard.Text = "Mini-Header Data: Direct Reference";

                //checkBoxMHStandardImportEvents.Enabled = false;
                //checkBoxMHStandardImportEvents.Visible = false;

                //checkBoxMHStandardImportHKX.Enabled = false;
                //checkBoxMHStandardImportHKX.Visible = false;

                //checkBoxMHStandardLoopByDefault.Enabled = false;
                //checkBoxMHStandardLoopByDefault.Visible = false;

                groupBoxMHStandard.Enabled = false;
                groupBoxMHDuplicate.Enabled = true;



                checkBoxMHStandardImportEvents.Checked = false;
                checkBoxMHStandardImportHKX.Checked = false;
                checkBoxMHStandardLoopByDefault.Checked = false;
                textBoxMHStandardImportHKXFrom.Text = "";


                //radioButtonMHStandard.Checked = false;
                //radioButtonMHImportOtherAnimation.Checked = true;

                //CurrentMiniHeaderType = miniHeaderType;
            }
            else
            {
                throw new NotImplementedException("MiniHeaderType not implemented.");
            }
        }

        private string HKXSubIDDispNameFromInt_NoPrefix(long subID)
        {
            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB ||
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
            {
                return $"{subID:D6}";
            }
            else
            {
                return $"{subID:D4}";
            }
        }

        private void LoadToGUI()
        {
            
            textBoxDisplayName.Text = AnimRef.AnimFileName;

            if (IsAbsoluteAnimID)
            {
                labelAnimSubIDPrefix.Text = "Animation ID: ";
                textBoxAnimSubID.Text = HKXNameFromCompositeID(AnimRef.ID);
            }
            else
            {
                labelAnimSubIDPrefix.Text = GameRoot.GameTypeHasLongAnimIDs ? "Animation Sub-ID: aXXX_" : "Animation Sub-ID: aXX_";

                textBoxAnimSubID.Text = HKXSubIDDispNameFromInt_NoPrefix(AnimRef.ID);
            }

            

            //CurrentMiniHeaderType = AnimRef.MiniHeader.Type;

            radioButtonMHStandard.Checked = false;
            radioButtonMHImportOtherAnimation.Checked = false;

            if (AnimRef.MiniHeader.Type == TAE.Animation.MiniHeaderType.Standard)
            {
                radioButtonMHStandard.Checked = true;
            }
            else if (AnimRef.MiniHeader.Type == TAE.Animation.MiniHeaderType.ImportOtherAnim)
            {
                radioButtonMHImportOtherAnimation.Checked = true;
            }
            else
            {
                throw new NotImplementedException("Mini header type not implemented");
            }

            MiniHeaderLoadValuesToGUI(AnimRef.MiniHeader);
        }

        public bool WriteFromGUI()
        {
            var savedMiniHeader = MiniHeaderSaveValuesFromGUI();

            if (savedMiniHeader == null)
                return false;

            int savedSubID = 0;
            string savedDisplayName = textBoxDisplayName.Text;


            if (IsAbsoluteAnimID)
            {
                if (uint.TryParse(textBoxAnimSubID.Text.Replace("_", "").Replace("a", ""), out uint subID))
                {
                    savedSubID = (int)subID;
                }
                else
                {
                    MessageBox.Show("Invalid animation ID formatting. " +
                        $"Expected '{CurFormattingString()}' " +
                        "type formatting, where X and Y are integers.",
                    "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                if (uint.TryParse(textBoxAnimSubID.Text, out uint subID))
                {
                    savedSubID = (int)subID;
                }
                else
                {
                    MessageBox.Show("Invalid Sub-ID value format. Expected an unsigned integer.",
                        "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            

            AnimRef.ID = savedSubID;
            AnimRef.AnimFileName = savedDisplayName;
            AnimRef.MiniHeader = savedMiniHeader;

            return true;
        }

        public TaeEditAnimPropertiesForm(TAE.Animation animRef, bool isAbsoluteAnimID)
        {
            IsAbsoluteAnimID = isAbsoluteAnimID;
            AnimRef = animRef;
            originalID = animRef.ID;
            originalMiniHeader = animRef.MiniHeader.GetClone();
            originalDisplayName = animRef.AnimFileName;
            InitializeComponent();
        }

        private void TaeEditAnimPropertiesForm_Shown(object sender, EventArgs e)
        {
            //propertyGrid.SelectedObject = AnimRef;
            LoadToGUI();
        }

        private void buttonSaveChanges_Click(object sender, EventArgs e)
        {
            if (WriteFromGUI())
            {
                WasAnimIDChanged = (AnimRef.ID != originalID);

                ReadyToExit = true;
                WereThingsChanged = true;
                WasAnimDeleted = false;
                Close();
            }
            else
            {
                AnimRef.ID = originalID;
                AnimRef.MiniHeader = originalMiniHeader;
                AnimRef.AnimFileName = originalDisplayName;

                MessageBox.Show("Changes were not saved due to formatting errors.",
                    "Changes Not Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void buttonDiscardChanges_Click(object sender, EventArgs e)
        {
            AnimRef.ID = originalID;
            AnimRef.MiniHeader = originalMiniHeader;
            AnimRef.AnimFileName = originalDisplayName;

            ReadyToExit = true;
            WereThingsChanged = false;
            WasAnimDeleted = false;
            WasAnimIDChanged = false;
            Close();
        }

        private void TaeEditAnimPropertiesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ReadyToExit)
            {
                AnimRef.ID = originalID;
                AnimRef.MiniHeader = originalMiniHeader;
                AnimRef.AnimFileName = originalDisplayName;

                WereThingsChanged = false;
                WasAnimDeleted = false;
                WasAnimIDChanged = false;
            }
        }

        private void buttonDeleteAnim_Click(object sender, EventArgs e)
        {
            var yesNoDlgResult = MessageBox.Show(
                $"Are you sure you want to delete animation entry {originalID}?\nThis can NOT be undone!",
                "Permanently Delete Animation Entry?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (yesNoDlgResult == DialogResult.Yes)
            {
                WasAnimDeleted = true;
                ReadyToExit = true;
                Close();
            }
        }

        private void TaeEditAnimPropertiesForm_Load(object sender, EventArgs e)
        {
            //this.Scale(new System.Drawing.SizeF(Main.DPIX, Main.DPIY));
            //RescaleConstantsForDpi(96, 96 * 2);
        }

        private void radioButtonMHStandard_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonMHStandard.Checked)
            {
                radioButtonMHImportOtherAnimation.Checked = false;
                MiniHeaderPrepareGUI(TAE.Animation.MiniHeaderType.Standard);
            }
        }

        private void radioButtonMHImportOtherAnimation_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonMHImportOtherAnimation.Checked)
            {
                radioButtonMHStandard.Checked = false;
                MiniHeaderPrepareGUI(TAE.Animation.MiniHeaderType.ImportOtherAnim);
            }
        }

        private void checkBoxMHStandardImportHKX_CheckedChanged(object sender, EventArgs e)
        {
            textBoxMHStandardImportHKXFrom.Enabled = checkBoxMHStandardImportHKX.Checked;
        }
    }
}
