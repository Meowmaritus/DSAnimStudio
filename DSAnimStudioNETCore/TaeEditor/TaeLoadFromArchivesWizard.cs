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
    public partial class TaeLoadFromArchivesWizard : Form
    {
        public TaeLoadFromArchivesWizard()
        {
            InitializeComponent();
        }

        public bool WizardCompleted = false;

        public class JsonSaveData
        {
            public string GameExePath { get; set; }
            public string AnibndPath { get; set; }
            public string ChrbndPath { get; set; }
            public string ProjectSaveFolder { get; set; }
            public bool LoadLooseGameParam { get; set; }
            public string ModEngineDirectory { get; set; }

            public JsonSaveData GetCopy()
            {
                return new JsonSaveData()
                {
                    GameExePath = GameExePath,
                    AnibndPath = AnibndPath,
                    ChrbndPath = ChrbndPath,
                    ProjectSaveFolder = ProjectSaveFolder,
                    LoadLooseGameParam = LoadLooseGameParam,
                    ModEngineDirectory = ModEngineDirectory,
                };
            }

            public void Save(string file)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
                var dir = System.IO.Path.GetDirectoryName(file);
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                System.IO.File.WriteAllText(file, json);
            }

            public static JsonSaveData Load(string file)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<JsonSaveData>(System.IO.File.ReadAllText(file));
            }
        }

        private int Step = 0;

        private void TaeLoadFromArchivesWizard_Load(object sender, EventArgs e)
        {
            WizardCompleted = false;

            labelLoadingText.Visible = false;
            progressBarLoading.Visible = false;

            Main.LoadConfig();
            var jsonSaveData = Main.Config.GetTaeLoadFromArchivesWizardJsonSaveData();
            if (jsonSaveData != null)
            {
                textBoxPathInterroot.Text = jsonSaveData.GameExePath;
                textBoxPathAnibnd.Text = jsonSaveData.AnibndPath;
                textBoxPathChrbnd.Text = jsonSaveData.ChrbndPath;
                textBoxPathSaveLoose.Text = jsonSaveData.ProjectSaveFolder;

                textBoxPathModEngineDir.Text = jsonSaveData.ModEngineDirectory;
                checkBoxLoadLooseParams.Checked = jsonSaveData.LoadLooseGameParam;

                UpdateSteps();
            }
        }

        private void buttonHelpGameEXE_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Browse for the game executable (e.g. DarkSoulsIII.exe). This tells DSAS what game it is and where the data is.",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonHelpAnibnd_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Select which animation file to edit from within the data archives. " +
                "See the Dark Souls Modding Wiki for lists of character IDs (http://soulsmodding.wikidot.com/reference:main)." +
                "The selected file will be unpacked into the project directory you select on step 4.",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonHelpChrbnd_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Select which model file to to use from within the data archives. " +
                "You only need to change this from the default if you're editing enemy variants which use multiple models for the same ANIBND file (used a lot in Elden Ring).",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void buttonHelpSaveLoose_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("The location where the loose project files will be saved. In it will be a configuration json, a couple of folders to help DSAS out with loading, and the file you selected in step 2.",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //private void ShowPathBrowse(string)

        private bool AllFilesExist(string interroot, params string[] files)
        {
            foreach (var f in files)
                if (!System.IO.File.Exists($@"{interroot}\{f}"))
                    return false;

            return true;
        }

        Task loadEblTask = null;

        private bool CheckCurrentInterroot()
        {
            var currentInterroot = GameRoot.InterrootPath;
            var currentGameType = GameRoot.GameType;
            string interroot = System.IO.Path.GetDirectoryName(textBoxPathInterroot.Text);
            GameRoot.InterrootPath = interroot;
            string projectLooseFilesDir = textBoxPathSaveLoose.Text.Trim().TrimEnd('\\').TrimEnd('/');
            if (System.IO.File.Exists($@"{interroot}\DARKSOULS.exe"))
            {
                if (!AllFilesExist(interroot, "dvdbnd1.bhd5", "dvdbnd1.bdt", "dvdbnd2.bhd5", "dvdbnd2.bdt", "dvdbnd3.bhd5", "dvdbnd3.bdt"))
                    return false;

                GameRoot.GameType = SoulsAssetPipeline.SoulsGames.DS1;
            }
            else if (System.IO.File.Exists($@"{interroot}\DarkSoulsIII.exe"))
            {
                if (!AllFilesExist(interroot, "Data1.bdt", "Data1.bhd", "Data2.bdt", "Data2.bhd", "Data3.bdt", "Data3.bhd", "Data4.bdt", "Data4.bhd", "Data5.bdt", "Data5.bhd"))
                    return false;

                GameRoot.GameType = SoulsAssetPipeline.SoulsGames.DS3;
            }
            else if (System.IO.File.Exists($@"{interroot}\sekiro.exe"))
            {
                if (!AllFilesExist(interroot, "Data1.bdt", "Data1.bhd", "Data2.bdt", "Data2.bhd", "Data3.bdt", "Data3.bhd", "Data4.bdt", "Data4.bhd", "Data5.bdt", "Data5.bhd"))
                    return false;

                GameRoot.GameType = SoulsAssetPipeline.SoulsGames.SDT;
            }
            else if (System.IO.File.Exists($@"{interroot}\EldenRing.exe"))
            {
                if (!AllFilesExist(interroot, "Data0.bdt", "Data0.bhd", "Data1.bdt", "Data1.bhd", "Data2.bdt", "Data2.bhd", "Data3.bdt", "Data3.bhd"))
                    return false;

                GameRoot.GameType = SoulsAssetPipeline.SoulsGames.ER;
            }
            else
            {
                return false;
            }

            if (GameRoot.GameType != currentGameType || GameRoot.InterrootPath != currentInterroot)
            {
                if (loadEblTask == null || loadEblTask.IsCompleted)
                {
                    loadEblTask = Task.Run(() =>
                    {
                        Invoke(new Action(() =>
                        {
                            Cursor = Cursors.WaitCursor;
                            labelLoadingText.Visible = true;
                            progressBarLoading.Visible = true;
                        }));
                        
                        GameData.InitEBLs();
                        GameData.SetCacheEnabled(false);
                        var chrEbl = GameData.PredictEblOfRootFolder("chr");
                        if (chrEbl != null)
                        {
                            GameData.ForceLoadEbl(chrEbl);
                        }

                        Invoke(new Action(() =>
                        {
                            Cursor = Cursors.Default;
                            labelLoadingText.Visible = false;
                            progressBarLoading.Visible = false;
                        }));
                    });
                    
                }
               
            }
            
            return true;
        }

        private void UpdateSteps()
        {
            if (loadEblTask != null && !loadEblTask.IsCompleted)
            {
                return;
            }

            int step = 0;
            if (CheckCurrentInterroot())
                step++;
            else
            {
                Invoke(new Action(() =>
                {
                    SetStepProgress(step);
                }));
                return;
            }

            void DoTheRestOfTheShit()
            {
                if (!string.IsNullOrWhiteSpace(textBoxPathAnibnd.Text) && GameData.FileExists(textBoxPathAnibnd.Text))
                {
                    step++;
                    if (string.IsNullOrWhiteSpace(textBoxPathChrbnd.Text))
                    {
                        Invoke(new Action(() =>
                        {
                            textBoxPathChrbnd.Text = textBoxPathAnibnd.Text.Substring(0, textBoxPathAnibnd.Text.Length - ".anibnd.dcx".Length) + ".chrbnd.dcx";
                        }));
                        step++;
                    }
                    else if (GameData.FileExists(textBoxPathChrbnd.Text))
                    {
                        step++;
                    }

                    bool validSaveLooseDir = false;
                    try
                    {
                        System.IO.Path.GetFullPath(textBoxPathSaveLoose.Text);
                        validSaveLooseDir = true;
                    }
                    catch
                    {

                    }

                    if (validSaveLooseDir && System.IO.Directory.Exists(textBoxPathSaveLoose.Text))
                    {
                        step++;
                    }

                    Invoke(new Action(() =>
                    {
                        SetStepProgress(step);
                    }));
                    return;
                }
                else
                {
                    Invoke(new Action(() =>
                    {
                        SetStepProgress(step);
                    }));
                    return;
                }
                if (!string.IsNullOrWhiteSpace(textBoxPathSaveLoose.Text))
                    step++;
                else
                {
                    Invoke(new Action(() =>
                    {
                        SetStepProgress(step);
                    }));
                    return;
                }

                Invoke(new Action(() =>
                {
                    SetStepProgress(step);
                }));
            }

            if (loadEblTask != null && !loadEblTask.IsCompleted)
            {
                loadEblTask.ContinueWith(task =>
                {
                    DoTheRestOfTheShit();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                DoTheRestOfTheShit();
            }

            

            
        }

        private void SetStepProgress(int step)
        {
            textBoxPathInterroot.Enabled = true;
            buttonPathInterroot.Enabled = true;

            textBoxPathAnibnd.ReadOnly = true;
            buttonPathAnibnd.Enabled = false;
            textBoxPathChrbnd.ReadOnly = true;
            buttonPathChrbnd.Enabled = false;
            textBoxPathSaveLoose.ReadOnly = true;
            buttonPathSaveLoose.Enabled = false;
            buttonCreateProject.Enabled = false;

            if (step >= 1)
            {
                textBoxPathAnibnd.ReadOnly = false;
                buttonPathAnibnd.Enabled = true;

                errorProvider1.SetError(textBoxPathInterroot, null);

            }

            if (step >= 2)
            {
                textBoxPathChrbnd.ReadOnly = false;
                buttonPathChrbnd.Enabled = true;

                
            }

            if (step >= 3)
            {
                textBoxPathSaveLoose.ReadOnly = false;
                buttonPathSaveLoose.Enabled = true;

                
            }

            if (step >= 4)
            {
                buttonCreateProject.Enabled = true;

            }

            Step = step;
        }

        private void buttonPathInterroot_Click(object sender, EventArgs e)
        {
            bool isPathValid = false;
            try
            {
                System.IO.Path.GetFullPath(textBoxPathInterroot.Text);
                isPathValid = true;
            }
            catch
            {

            }
            var dir = (isPathValid && !string.IsNullOrEmpty(textBoxPathInterroot.Text)) ? System.IO.Path.GetDirectoryName(textBoxPathInterroot.Text) : null;
            var browseDlg = new OpenFileDialog()
            {
                FileName = "",
                CheckFileExists = false,
                CheckPathExists = true,
                Title = "Select Game Executable",
                Filter = "Executables (*.exe)|*.EXE"
            };
            if (!string.IsNullOrWhiteSpace(dir) && System.IO.Directory.Exists(dir))
            {
                browseDlg.InitialDirectory = dir;
            }
            if (browseDlg.ShowDialog() == DialogResult.OK)
            {
                textBoxPathInterroot.Text = browseDlg.FileName;
                UpdateSteps();
            }
        }

        private void buttonPathAnibnd_Click(object sender, EventArgs e)
        {
            var anibnd = GameData.ShowPickInsideBndPath("/chr/", @".*\/c\d\d\d\d.anibnd.dcx$", textBoxPathAnibnd.Text, "Choose ANIBND", textBoxPathAnibnd.Text);
            if (anibnd != null)
            {
                textBoxPathAnibnd.Text = anibnd;
                UpdateSteps(); 
            }
        }

        private void buttonPathChrbnd_Click(object sender, EventArgs e)
        {
            var searchDefaultStart = Utils.GetFileNameWithoutAnyExtensions(textBoxPathAnibnd.Text);
            searchDefaultStart = searchDefaultStart.Substring(0, searchDefaultStart.Length - 1);
            var chrbnd = GameData.ShowPickInsideBndPath("/chr/", @".*\/c\d\d\d\d.chrbnd.dcx$", searchDefaultStart, "Choose CHRBND", textBoxPathAnibnd.Text.Replace(".anibnd.dcx", ".chrbnd.dcx"));
            if (chrbnd != null)
            {
                textBoxPathChrbnd.Text = chrbnd;
                UpdateSteps();
            }
        }

        private void buttonPathSaveLoose_Click(object sender, EventArgs e)
        {
            bool isPathValid = false;
            try
            {
                System.IO.Path.GetFullPath(textBoxPathSaveLoose.Text);
                isPathValid = true;
            }
            catch
            {

            }
            var dir = (isPathValid && !string.IsNullOrEmpty(textBoxPathSaveLoose.Text)) ? System.IO.Path.GetDirectoryName(textBoxPathSaveLoose.Text) : null;
            var browseDlg = new SaveFileDialog()
            {
                FileName = "GO TO DIRECTORY AND CLICK SAVE",
                CheckFileExists = false,
                CheckPathExists = true,
                Title = "Select Project Directory"
            };
            if (!string.IsNullOrWhiteSpace(dir) && System.IO.Directory.Exists(dir))
            {
                browseDlg.InitialDirectory = dir;
            }
            if (browseDlg.ShowDialog() == DialogResult.OK)
            {
                textBoxPathSaveLoose.Text = System.IO.Path.GetDirectoryName(browseDlg.FileName);
                UpdateSteps();
            }
        }

        private void buttonCreateProject_Click(object sender, EventArgs e)
        {
            SaveJsonCfg();
            UpdateSteps();
            if (Step >= 4)
            {
                bool isPathValid_Project = false;
                try
                {
                    System.IO.Path.GetFullPath(textBoxPathSaveLoose.Text);
                    isPathValid_Project = true;
                }
                catch
                {

                }

                bool isPathValid_Interroot = false;
                try
                {
                    System.IO.Path.GetFullPath(textBoxPathInterroot.Text);
                    isPathValid_Interroot = true;
                }
                catch
                {

                }

                if (isPathValid_Project)
                {
                    var projectDir = textBoxPathSaveLoose.Text;
                    var interrootDir = System.IO.Path.GetDirectoryName(textBoxPathInterroot.Text);
                    if (!System.IO.Directory.Exists(projectDir))
                        System.IO.Directory.CreateDirectory(projectDir);
                    var cacheDir = $@"{projectDir}\_DSAS_CACHE";
                    var tempDir = $@"{projectDir}\_DSAS_TEMP";
                    if (!System.IO.Directory.Exists(cacheDir))
                        System.IO.Directory.CreateDirectory(cacheDir);
                    GameData.SaveCurrentEblHeaderCaches(cacheDir);
                    GameRoot.InterrootPath = interrootDir;
                    GameRoot.InterrootModenginePath = textBoxPathModEngineDir.Text;
                    GameRoot.CachePath = cacheDir;
                    GameRoot.ScratchPath = tempDir;
                    GameRoot.ModengineLoadLooseParams = checkBoxLoadLooseParams.Checked;
                    GameData.CfgLoadUnpackedGameFiles = checkBoxCfgLoadUnpackedGameFiles.Checked;
                    var projectFile = $@"{projectDir}\_DSAS_PROJECT.json";
                    var saveJson = new TaeProjectJson()
                    {
                        GameDirectory = interrootDir,
                        ModEngineDirectory = textBoxPathModEngineDir.Text,
                        LoadLooseParams = checkBoxLoadLooseParams.Checked,
                        LoadUnpackedGameFiles = checkBoxCfgLoadUnpackedGameFiles.Checked,
                    };
                    saveJson.Save(projectFile);

                    var anibndFile = GameData.ReadFile(textBoxPathAnibnd.Text);
                    var anibndOutputPath = $@"{projectDir}\{Utils.GetShortIngameFileName(textBoxPathAnibnd.Text)}.anibnd.dcx";
                    System.IO.File.WriteAllBytes(anibndOutputPath, anibndFile);

                    Main.TAE_EDITOR.FileContainerName = anibndOutputPath;
                    Main.TAE_EDITOR.FileContainerName_Model = !string.IsNullOrWhiteSpace(textBoxPathChrbnd.Text) ? textBoxPathChrbnd.Text : textBoxPathAnibnd.Text;
                    // This is really pepega
                    Main.TAE_EDITOR.SuppressNextModelOverridePrompt = true;
                    GameRoot.SuppressNextInterrootBrowsePrompt = true;
                    Main.TAE_EDITOR.LoadCurrentFile();
                    WizardCompleted = true;
                    Close();
                }
            }
        }

        private void textBoxPathInterroot_Leave(object sender, EventArgs e)
        {
            UpdateSteps();
        }

        private void textBoxPathAnibnd_Leave(object sender, EventArgs e)
        {
            UpdateSteps();
        }

        private void textBoxPathChrbnd_Leave(object sender, EventArgs e)
        {
            UpdateSteps();
        }

        private void textBoxPathSaveLoose_Leave(object sender, EventArgs e)
        {
            UpdateSteps();
        }

        private void textBoxPathInterroot_Validating(object sender, CancelEventArgs e)
        {
            UpdateSteps();
            if (Step == 0)
            {
                //e.Cancel = true;
                errorProvider1.SetError(textBoxPathInterroot, "Invalid game installation.");
            }
            else
            {
                errorProvider1.SetError(textBoxPathInterroot, null);
            }
        }

        private void textBoxPathInterroot_Validated(object sender, EventArgs e)
        {

        }

        private void buttonBrowseModEngineDir_Click(object sender, EventArgs e)
        {
            bool isPathValid = false;
            try
            {
                System.IO.Path.GetFullPath(textBoxPathModEngineDir.Text);
                isPathValid = true;
            }
            catch
            {

            }
            var dir = (isPathValid && !string.IsNullOrEmpty(textBoxPathModEngineDir.Text)) ? System.IO.Path.GetDirectoryName(textBoxPathModEngineDir.Text) : null;
            var browseDlg = new SaveFileDialog()
            {
                FileName = "GO TO DIRECTORY AND CLICK SAVE",
                CheckFileExists = false,
                CheckPathExists = true,
                Title = "Select ModEngine Directory"
            };
            if (!string.IsNullOrWhiteSpace(dir) && System.IO.Directory.Exists(dir))
            {
                browseDlg.InitialDirectory = dir;
            }
            if (browseDlg.ShowDialog() == DialogResult.OK)
            {
                textBoxPathModEngineDir.Text = System.IO.Path.GetDirectoryName(browseDlg.FileName);
                UpdateSteps();
            }
        }

        private void buttonHelpCfgLoadLooseGameParam_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("If checked, DS Anim Studio will load parameters from loose GameParam file " +
                "rather than from encrypted regulation. Works exactly like the ModEngine setting \"loadLooseParams\".",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonHelpCfgModEngineDir_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("If a path is entered here, it will function exactly like the mod " +
                "override directory in ModEngine, making DS Anim Studio load files from there with first priority, " +
                "only falling back to loading from the regular game directory if the file isn't found in the " +
                "ModEngine directory.",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveJsonCfg()
        {
            Main.Config.TaeLoadFromArchivesWizardJsonSaveData = new JsonSaveData()
            {
                GameExePath = textBoxPathInterroot.Text,
                AnibndPath = textBoxPathAnibnd.Text,
                ChrbndPath = textBoxPathChrbnd.Text,
                ProjectSaveFolder = textBoxPathSaveLoose.Text,

                ModEngineDirectory = textBoxPathModEngineDir.Text,
                LoadLooseGameParam = checkBoxLoadLooseParams.Checked,
            };

            Main.SaveConfig();
        }

        private void TaeLoadFromArchivesWizard_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveJsonCfg();
        }

        private void buttonHelpCfgLoadUnpackedGameFiles_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("If checked, DS Anim Studio will load loose files unpacked with UXM instead of the ones in the data archives. " +
                "\nWorks exactly like the ModEngine setting \"loadUXMFiles\"." +
                "\nHas no effect on games that do not have archive files (DeS, DS1:PTDE, BB)",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonClearModEngineDir_Click(object sender, EventArgs e)
        {
            textBoxPathModEngineDir.Text = "";
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            SaveJsonCfg();
            WizardCompleted = false;
            Close();
        }
    }
}
