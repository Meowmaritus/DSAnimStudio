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
    public partial class TaeGameDirPicker : Form
    {
        public TaeGameDirPicker()
        {
            InitializeComponent();
        }

        public bool ClickedApply = false;

        private string _saveFile;
        private string SaveFile
        {
            get => _saveFile;
            set
            {
                _saveFile = value;
                textBoxProjectDirectory.Text = System.IO.Path.GetDirectoryName(value);
            }
        }
        //public TaeProjectJson SaveJsonData { get; set; } = new TaeProjectJson();
        //TODO: Check for threading issues on these? Might need to use the form to invoke.
        public string SelectedGameDir
        {
            get => textBoxGameDir.Text;
            set => textBoxGameDir.Text = value;
        }
        public string SelectedModengineDir
        {
            get => textBoxModDir.Text;
            set => textBoxModDir.Text = value;
        }
        public bool LoadLooseParams
        {
            get => checkBoxLoadLooseParams.Checked;
            set => checkBoxLoadLooseParams.Checked = value;
        }

        public bool LoadUnpackedGameFiles
        {
            get => checkBoxLoadUnpackedGameFiles.Checked;
            set => checkBoxLoadUnpackedGameFiles.Checked = value;
        }

        public bool InitAndCheckIfNeedsToBeShownLol()
        {
            bool shouldBeShown = true;
            SaveFile = GameRoot.ProjectPath;
            GameData.LoadProjectJson();

            GameData.ProjectJsonLockAct(proj =>
            {
                SelectedGameDir = proj.GameDirectory;
                SelectedModengineDir = proj.ModEngineDirectory;
                LoadLooseParams = proj.LoadLooseParams;
                LoadUnpackedGameFiles = proj.LoadUnpackedGameFiles;
            });
            

            return shouldBeShown;
        }

        private void TaeGameDirPicker_Load(object sender, EventArgs e)
        {
            
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            GameRoot.ProjectPath = SaveFile;
            GameData.ProjectJsonLockAct(proj =>
            {
                proj.GameDirectory = SelectedGameDir;
                proj.ModEngineDirectory = SelectedModengineDir;
                proj.LoadLooseParams = LoadLooseParams;
                proj.LoadUnpackedGameFiles = LoadUnpackedGameFiles;
                proj.Save(SaveFile);
            });
            GameData.SaveProjectJson();
            
            
            ClickedApply = true;
            Close();
        }

        private void buttonBrowseGameDir_Click(object sender, EventArgs e)
        {
            var dir = SelectedGameDir;
            var browseDlg = new SaveFileDialog()
            {
                FileName = "GO TO DIRECTORY AND CLICK SAVE",
                CheckFileExists = false,
                CheckPathExists = true,
                Title = "Select Game Directory",
            };
            if (!string.IsNullOrWhiteSpace(dir) && System.IO.Directory.Exists(dir))
            {
                browseDlg.InitialDirectory = dir;
            }
            if (browseDlg.ShowDialog() == DialogResult.OK)
            {
                SelectedGameDir = System.IO.Path.GetDirectoryName(browseDlg.FileName);
            }
        }

        private void buttonBrowseModDir_Click(object sender, EventArgs e)
        {
            var dir = SelectedModengineDir;
            var browseDlg = new SaveFileDialog()
            {
                FileName = "GO TO DIRECTORY AND CLICK SAVE",
                CheckFileExists = false,
                CheckPathExists = true,
                Title = "Select ModEngine Directory",
            };
            if (!string.IsNullOrWhiteSpace(dir) && System.IO.Directory.Exists(dir))
            {
                browseDlg.InitialDirectory = dir;
            }
            if (browseDlg.ShowDialog() == DialogResult.OK)
            {
                SelectedModengineDir = System.IO.Path.GetDirectoryName(browseDlg.FileName);
            }
        }

        private void buttonClearModDir_Click(object sender, EventArgs e)
        {
            SelectedModengineDir = "";
        }

        private void buttonHelpLoadLooseParams_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("If checked, DS Anim Studio will load parameters from loose GameParam file " +
                "rather than from encrypted regulation. Works exactly like the ModEngine setting \"loadLooseParams\".",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonHelpLoadUxmFiles_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("If checked, DS Anim Studio will load loose files unpacked with UXM instead of the ones in the data archives. " +
                "\nWorks exactly like the ModEngine setting \"loadUXMFiles\"." +
                "\nHas no effect on games that do not have archive files (DeS, DS1:PTDE, BB)",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void noPaddingButtonHelpModEngindDir_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("If a path is entered here, it will function exactly like the mod " +
                "override directory in ModEngine, making DS Anim Studio load files from there with first priority, " +
                "only falling back to loading from the regular game directory if the file isn't found in the " +
                "ModEngine directory.",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonABORT_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TaeGameDirPicker_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ClickedApply)
                System.Threading.Thread.CurrentThread.Interrupt();
        }
    }
}
