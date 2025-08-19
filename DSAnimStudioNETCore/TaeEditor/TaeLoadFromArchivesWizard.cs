using SoulsFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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

        private bool CANCELLED = false;
        private bool CANCELLED_SAFE_TO_CLOSE = false;


        public class JsonSaveData
        {
            public string GameExePath { get; set; }
            public string AnibndPath { get; set; }
            public string ChrbndPath { get; set; }
            public string ProjectSaveFolder { get; set; }
            public bool LoadLooseGameParam { get; set; }
            public bool LoadUnpackedFiles { get; set; }
            public bool DisableInterrootDCX { get; set; }
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
                    LoadUnpackedFiles = LoadUnpackedFiles,
                    DisableInterrootDCX = DisableInterrootDCX,
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

        private string[] FileTypeStrings_OBJ = new string[]
        {
            "Select a file type",
            "Character (/chr)",
            "Parts (/parts)",
            "Object (/obj)"
        };

        private string[] FileTypeStrings_AEG = new string[]
        {
            "Select a file type",
            "Character (/chr)",
            "Parts (/parts)",
            "Geometry (/asset)"
        };

        private int FileTypeIndex_NONE = 0;
        private int FileTypeIndex_Character = 1;
        private int FileTypeIndex_Parts = 2;
        private int FileTypeIndex_ObjAeg = 3;

        private string selectAnibndInBinder_LastPartsbndText = null;
        private IBinder selectAnibndInBinder_Binder = null;
        private List<string> selectAnibndInBinder_Names = new List<string>();
        private List<int> selectAnibndInBinder_BindIDs = new List<int>();
        private List<IBinder> selectAnibndInBinder_AnibndsInBinder = new List<IBinder>();

        private void NewSetInterroot(string interroot)
        {
            textBoxPathInterroot.Text = interroot;
            NewRefreshSteps();
        }

        private bool selectAnibndInBinder_IsEmpty = false;

        FileTypes FileType = FileTypes.None;

        //private int Step = 0;

        private void SetInterrootLockout(bool lockout)
        {
            Invoke(new Action(() =>
            {
                textBoxPathInterroot.Enabled = !lockout;
                buttonPathInterroot.Enabled = !lockout;
            }));
        }

        private void SetFileTypeStrings(bool aeg)
        {
            comboBoxFileType.Items.Clear();
            var arr = aeg ? FileTypeStrings_AEG : FileTypeStrings_OBJ;
            for (int i = 0; i < arr.Length; i++)
                comboBoxFileType.Items.Add(arr[i]);

            comboBoxFileType.SelectedIndex = FileTypeIndex_NONE;
        }

        private void TaeLoadFromArchivesWizard_Load(object sender, EventArgs e)
        {

            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Location = textBox_Chr_PathChrbnd.Location;
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Site = textBox_Chr_PathChrbnd.Site;
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Enabled = false;
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Visible = false;
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Text = "";
            WizardCompleted = false;

            labelLoadingText.Visible = false;
            progressBarLoading.Visible = false;

            InitControlCategories();
            SetFileTypeStrings(false);


            Main.LoadConfig();
            var jsonSaveData = Main.Config.GetTaeLoadFromArchivesWizardJsonSaveData();
            if (jsonSaveData != null)
            {
                textBoxPathInterroot.Text = jsonSaveData.GameExePath;
                textBoxPathMainBinder.Text = jsonSaveData.AnibndPath;
                textBox_Chr_PathChrbnd.Text = jsonSaveData.ChrbndPath;
                textBoxPathSaveLoose.Text = jsonSaveData.ProjectSaveFolder;

                textBoxPathModEngineDir.Text = jsonSaveData.ModEngineDirectory;
                checkBoxLoadLooseParams.Checked = jsonSaveData.LoadLooseGameParam;

                checkBoxCfgLoadUnpackedGameFiles.Checked = jsonSaveData.LoadUnpackedFiles;
                checkBoxDisableInterrootDCX.Checked = jsonSaveData.DisableInterrootDCX;
            }
            CurrentStep = Steps.Start;
            NewRefreshSteps();
        }

        private void buttonHelpGameEXE_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Browse for the game executable (e.g. DarkSoulsIII.exe). This tells DSAS what game it is and where the data is.",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonHelpSaveLoose_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("The location where the loose project files will be saved. In it will be a configuration json, a couple of folders to help DSAS out with loading, and the file you selected in step 3.",
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
            var currentInterroot = zzz_DocumentManager.CurrentDocument.GameRoot.InterrootPath;
            var currentGameType = zzz_DocumentManager.CurrentDocument.GameRoot.GameType;
            string interroot = System.IO.Path.GetDirectoryName(textBoxPathInterroot.Text);
            zzz_DocumentManager.CurrentDocument.GameRoot.InterrootPath = interroot;
            //zzz_DocumentManager.CurrentDocument.GameRoot.CachePath = interroot + "\\_DSAS_CACHE";
            string projectLooseFilesDir = textBoxPathSaveLoose.Text.Trim().TrimEnd('\\').TrimEnd('/');
            if (System.IO.File.Exists($@"{interroot}\DARKSOULS.exe"))
            {
                if (!AllFilesExist(interroot, "dvdbnd1.bhd5", "dvdbnd1.bdt", "dvdbnd2.bhd5", "dvdbnd2.bdt", "dvdbnd3.bhd5", "dvdbnd3.bdt"))
                    return false;

                zzz_DocumentManager.CurrentDocument.GameRoot.GameType = SoulsAssetPipeline.SoulsGames.DS1;
            }
            else if (System.IO.File.Exists($@"{interroot}\DarkSoulsIII.exe"))
            {
                if (!AllFilesExist(interroot, "Data1.bdt", "Data1.bhd", "Data2.bdt", "Data2.bhd", "Data3.bdt", "Data3.bhd", "Data4.bdt", "Data4.bhd", "Data5.bdt", "Data5.bhd"))
                    return false;

                zzz_DocumentManager.CurrentDocument.GameRoot.GameType = SoulsAssetPipeline.SoulsGames.DS3;
            }
            else if (System.IO.File.Exists($@"{interroot}\sekiro.exe"))
            {
                if (!AllFilesExist(interroot, "Data1.bdt", "Data1.bhd", "Data2.bdt", "Data2.bhd", "Data3.bdt", "Data3.bhd", "Data4.bdt", "Data4.bhd", "Data5.bdt", "Data5.bhd"))
                    return false;

                zzz_DocumentManager.CurrentDocument.GameRoot.GameType = SoulsAssetPipeline.SoulsGames.SDT;
            }
            else if (System.IO.File.Exists($@"{interroot}\EldenRing.exe"))
            {
                if (!AllFilesExist(interroot, "Data0.bdt", "Data0.bhd", "Data1.bdt", "Data1.bhd", "Data2.bdt", "Data2.bhd", "Data3.bdt", "Data3.bhd"))
                    return false;

                zzz_DocumentManager.CurrentDocument.GameRoot.GameType = SoulsAssetPipeline.SoulsGames.ER;
            }
            else if (System.IO.File.Exists($@"{interroot}\Nightreign.exe"))
            {
                if (!AllFilesExist(interroot, "Data0.bdt", "Data0.bhd", "Data1.bdt", "Data1.bhd", "Data2.bdt", "Data2.bhd", "Data3.bdt", "Data3.bhd"))
                    return false;

                zzz_DocumentManager.CurrentDocument.GameRoot.GameType = SoulsAssetPipeline.SoulsGames.ERNR;
            }
            else if (System.IO.File.Exists($@"{interroot}\armoredcore6.exe"))
            {
                if (!AllFilesExist(interroot, "Data0.bdt", "Data0.bhd", "Data1.bdt", "Data1.bhd", "Data2.bdt", "Data2.bhd", "Data3.bdt", "Data3.bhd"))
                    return false;

                zzz_DocumentManager.CurrentDocument.GameRoot.GameType = SoulsAssetPipeline.SoulsGames.AC6;
            }
            else
            {
                return false;
            }

            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType != currentGameType || zzz_DocumentManager.CurrentDocument.GameRoot.InterrootPath != currentInterroot)
            {
                SetFileTypeStrings(zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesAEG);

                if (loadEblTask == null || loadEblTask.IsCompleted)
                {
                    comboBoxFileType.Enabled = false;
                    loadEblTask = Task.Run(() =>
                    {
                        SetInterrootLockout(true);

                        Invoke(new Action(() =>
                        {
                            Cursor = Cursors.WaitCursor;
                            labelLoadingText.Visible = true;
                            progressBarLoading.Visible = true;
                        }));

                        if (CANCELLED)
                            return;

                        zzz_DocumentManager.CurrentDocument.GameData.InitEBLs();

                        if (CANCELLED)
                            return;

                        //GameData.SetCacheEnabled(false);
                        var eblName = zzz_DocumentManager.CurrentDocument.GameData.PredictEblOfRootFolder("chr");

                        if (CANCELLED)
                            return;

                        if (eblName != null)
                        {
                            zzz_DocumentManager.CurrentDocument.GameData.ForceLoadEbl(eblName);
                        }

                        if (CANCELLED)
                            return;

                        eblName = zzz_DocumentManager.CurrentDocument.GameData.PredictEblOfRootFolder("parts");

                        if (CANCELLED)
                            return;

                        if (eblName != null)
                        {
                            zzz_DocumentManager.CurrentDocument.GameData.ForceLoadEbl(eblName);
                        }

                        if (CANCELLED)
                            return;

                        eblName = zzz_DocumentManager.CurrentDocument.GameData.PredictEblOfRootFolder("obj");

                        if (CANCELLED)
                            return;

                        if (eblName != null)
                        {
                            zzz_DocumentManager.CurrentDocument.GameData.ForceLoadEbl(eblName);
                        }

                        if (CANCELLED)
                            return;

                        eblName = zzz_DocumentManager.CurrentDocument.GameData.PredictEblOfRootFolder("asset");

                        if (CANCELLED)
                            return;

                        if (eblName != null)
                        {
                            zzz_DocumentManager.CurrentDocument.GameData.ForceLoadEbl(eblName);
                        }

                        if (CANCELLED)
                            return;

                        Invoke(new Action(() =>
                        {
                            Cursor = Cursors.Default;
                            labelLoadingText.Visible = false;
                            progressBarLoading.Visible = false;
                        }));


                    });

                    loadEblTask.ContinueWith(t =>
                    {
                        NewRefreshSteps();

                        SetInterrootLockout(false);
                    });
                }

            }

            return true;
        }

        private void NewRefreshSteps()
        {
            Invoke(() =>
            {
                var step = Steps.Start;

                ControlCategories[ControlCategoryTypes.InterrootSelect].IsGrayedOut = false;
                ControlCategories[ControlCategoryTypes.FileTypeSelect].IsGrayedOut = true;
                ControlCategories[ControlCategoryTypes.MainBinderSelect].IsGrayedOut = true;


                ControlCategories[ControlCategoryTypes.SecondaryBinderLabelAndHelp].IsHidden = true;

                ControlCategories[ControlCategoryTypes.Chr_ChrbndSelect].IsGrayedOut = true;
                ControlCategories[ControlCategoryTypes.Chr_ChrbndSelect].IsHidden = true;

                ControlCategories[ControlCategoryTypes.PartsObjAeg_AnibndInBinderSelect].IsGrayedOut = true;
                ControlCategories[ControlCategoryTypes.PartsObjAeg_AnibndInBinderSelect].IsHidden = true;

                ControlCategories[ControlCategoryTypes.OutputDirSelect].IsGrayedOut = true;
                ControlCategories[ControlCategoryTypes.ModEngineDirSelect].IsGrayedOut = true;

                ControlCategories[ControlCategoryTypes.Chr_ChrbndSelect].IsHidden = true;

                ControlCategories[ControlCategoryTypes.CreateButton].IsGrayedOut = true;

                if (CheckCurrentInterroot())
                {
                    step = Steps.InterrootValidating;
                    ControlCategories[ControlCategoryTypes.InterrootSelect].IsGrayedOut = true;

                    if (loadEblTask == null || loadEblTask.IsCompleted)
                    {
                        ControlCategories[ControlCategoryTypes.InterrootSelect].IsGrayedOut = false;
                        step = Steps.InterrootSet;
                        ControlCategories[ControlCategoryTypes.FileTypeSelect].IsGrayedOut = false;

                        FileType = FileTypes.None;
                        if (comboBoxFileType.SelectedIndex == FileTypeIndex_Character)
                        {
                            FileType = FileTypes.Chr;
                        }
                        else if (comboBoxFileType.SelectedIndex == FileTypeIndex_Parts)
                        {
                            FileType = FileTypes.Parts;
                        }
                        else if (comboBoxFileType.SelectedIndex == FileTypeIndex_ObjAeg)
                        {
                            FileType = FileTypes.ObjAeg;
                        }

                        if (FileType != FileTypes.None)
                        {
                            step = Steps.FileTypeSet;
                            ControlCategories[ControlCategoryTypes.MainBinderSelect].IsGrayedOut = false;

                            // Show stuff for current FileType
                            if (FileType == FileTypes.Chr)
                            {
                                labelStep3A.Text = "3a. Choose ANIBND";
                                labelStep3B.Text = "3b. Choose CHRBND";
                            }
                            else if (FileType is FileTypes.Parts)
                            {
                                labelStep3A.Text = "3a. Choose PARTSBND";
                                labelStep3B.Text = "3b. Choose ANIBND Within";
                            }
                            else if (FileType is FileTypes.ObjAeg)
                            {
                                labelStep3A.Text = zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesAEG ? "3a. Choose GEOMBND" : "3a. Choose OBJBND";
                                labelStep3B.Text = "3b. Choose ANIBND Within";
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }




                            if (FileType == FileTypes.Chr)
                                ControlCategories[ControlCategoryTypes.Chr_ChrbndSelect].IsHidden = false;
                            else if (FileType is FileTypes.Parts or FileTypes.ObjAeg)
                                ControlCategories[ControlCategoryTypes.PartsObjAeg_AnibndInBinderSelect].IsHidden = false;
                            else
                                throw new NotImplementedException();

                            bool mainBinderValid = false;

                            if (FileType is FileTypes.Chr)
                            {
                                if (CheckIfBinderHasTaeAndSetError(textBoxPathMainBinder.Text, textBoxPathMainBinder))
                                {
                                    mainBinderValid = true;
                                }
                            }
                            else if (FileType is FileTypes.Parts or FileTypes.ObjAeg)
                            {
                                if (CheckIfBinderHasAnibndsAndSetError(textBoxPathMainBinder.Text, textBoxPathMainBinder))
                                {
                                    mainBinderValid = true;
                                }
                            }


                            if (mainBinderValid)
                            {
                                step = Steps.MainBinderSet;

                                ControlCategories[ControlCategoryTypes.SecondaryBinderLabelAndHelp].IsHidden = false;

                                if (FileType == FileTypes.Chr)
                                {
                                    ControlCategories[ControlCategoryTypes.Chr_ChrbndSelect].IsGrayedOut = false;
                                    if (zzz_DocumentManager.CurrentDocument.GameData.FileExists(textBox_Chr_PathChrbnd.Text))
                                    {
                                        step = Steps.SecondaryBinderSet;

                                    }
                                }
                                else if (FileType is FileTypes.Parts or FileTypes.ObjAeg)
                                {
                                    ControlCategories[ControlCategoryTypes.PartsObjAeg_AnibndInBinderSelect].IsGrayedOut = false;
                                    if (comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Items.Count > 0 && comboBox_PartsObjAeg_SelectAnibndInPartsbnd.SelectedIndex >= 0)
                                    {
                                        var binder = selectAnibndInBinder_AnibndsInBinder[comboBox_PartsObjAeg_SelectAnibndInPartsbnd.SelectedIndex];
                                        if (SimpleCheckIfBinderHasTae(binder))
                                        {
                                            step = Steps.SecondaryBinderSet;
                                        }
                                        else
                                        {
                                            errorProvider1.SetError(comboBox_PartsObjAeg_SelectAnibndInPartsbnd, "Selected ANIBND has no TAE inside.");
                                        }

                                    }
                                }
                                else
                                {
                                    throw new NotImplementedException();
                                }
                            }



                            // Step out here and check if step got deep enough to avoid nesting a bunch of duplicate ugly shit in the above 2 branching paths
                            if (step == Steps.SecondaryBinderSet)
                            {
                                ControlCategories[ControlCategoryTypes.OutputDirSelect].IsGrayedOut = false;
                                bool outputDirValid = false;
                                try
                                {
                                    outputDirValid = Directory.Exists(textBoxPathSaveLoose.Text);
                                }
                                catch
                                {

                                }

                                if (outputDirValid)
                                {
                                    step = Steps.ProjectSet;
                                    ControlCategories[ControlCategoryTypes.ModEngineDirSelect].IsGrayedOut = false;

                                    bool modEngineDirValid = false;
                                    bool modEngineDirSet = !string.IsNullOrWhiteSpace(textBoxPathModEngineDir.Text);
                                    if (modEngineDirSet)
                                    {
                                        try
                                        {
                                            modEngineDirValid = Directory.Exists(textBoxPathModEngineDir.Text);
                                        }
                                        catch
                                        {

                                        }
                                    }

                                    if (modEngineDirSet && !modEngineDirValid)
                                    {
                                        ControlCategories[ControlCategoryTypes.CreateButton].IsGrayedOut = true;
                                    }
                                    else
                                    {
                                        ControlCategories[ControlCategoryTypes.CreateButton].IsGrayedOut = false;
                                    }

                                }
                            }
                        }


                    }
                }

                UpdateControlCategories();
                CurrentStep = step;
            });
        }

        private enum FileTypes
        {
            None = 0,
            Chr = 1,
            Parts = 2,
            ObjAeg = 3,
        }

        private enum Steps
        {
            Start = 0,

            InterrootValidating = 1,

            InterrootSet = 2,

            FileTypeSet = 3,

            MainBinderSet = 4,

            SecondaryBinderSet = 5,

            ProjectSet = 6,


        }

        public enum ControlCategoryTypes
        {
            InterrootSelect,
            FileTypeSelect,
            MainBinderSelect,
            SecondaryBinderLabelAndHelp,
            Chr_ChrbndSelect,
            PartsObjAeg_AnibndInBinderSelect,
            OutputDirSelect,
            ModEngineDirSelect,
            CreateButton,
        }
        public class ControlCategory
        {
            public List<Control> Controls = new List<Control>();
            public bool IsGrayedOut;
            public bool IsHidden;
        }
        private Dictionary<ControlCategoryTypes, ControlCategory> ControlCategories = new Dictionary<ControlCategoryTypes, ControlCategory>();
        private void RegistControlCategory(ControlCategoryTypes type, Control c)
        {
            if (!ControlCategories.ContainsKey(type))
                ControlCategories[type] = new ControlCategory();

            if (!ControlCategories[type].Controls.Contains(c))
                ControlCategories[type].Controls.Add(c);
        }

        private void UpdateControlCategories()
        {
            foreach (var kvp in ControlCategories)
            {
                foreach (var c in kvp.Value.Controls)
                {
                    if (kvp.Value.IsHidden)
                    {
                        c.Visible = false;
                        c.Enabled = false;
                    }
                    else
                    {
                        c.Visible = true;
                        //if (c is TextBox asTextBox)
                        //{
                        //    c.Enabled = true;
                        //    asTextBox.ReadOnly = kvp.Value.IsGrayedOut;
                        //}
                        //else
                        //{
                        //    c.Enabled = !kvp.Value.IsGrayedOut;
                        //}
                        c.Enabled = !kvp.Value.IsGrayedOut;
                    }

                    if (kvp.Value.IsHidden || kvp.Value.IsGrayedOut)
                    {
                        errorProvider1.SetError(c, null);
                    }
                }
            }
        }

        private void InitControlCategories()
        {
            RegistControlCategory(ControlCategoryTypes.InterrootSelect, textBoxPathInterroot);
            RegistControlCategory(ControlCategoryTypes.InterrootSelect, buttonPathInterroot);
            RegistControlCategory(ControlCategoryTypes.FileTypeSelect, comboBoxFileType);
            RegistControlCategory(ControlCategoryTypes.MainBinderSelect, textBoxPathMainBinder);
            RegistControlCategory(ControlCategoryTypes.MainBinderSelect, buttonPathAnibnd);
            RegistControlCategory(ControlCategoryTypes.Chr_ChrbndSelect, textBox_Chr_PathChrbnd);
            RegistControlCategory(ControlCategoryTypes.Chr_ChrbndSelect, buttonPathChrbnd);
            RegistControlCategory(ControlCategoryTypes.PartsObjAeg_AnibndInBinderSelect, comboBox_PartsObjAeg_SelectAnibndInPartsbnd);
            RegistControlCategory(ControlCategoryTypes.SecondaryBinderLabelAndHelp, buttonHelpSecondaryBinder);
            RegistControlCategory(ControlCategoryTypes.SecondaryBinderLabelAndHelp, labelStep3B);
            RegistControlCategory(ControlCategoryTypes.OutputDirSelect, textBoxPathSaveLoose);
            RegistControlCategory(ControlCategoryTypes.OutputDirSelect, buttonPathSaveLoose);
            RegistControlCategory(ControlCategoryTypes.ModEngineDirSelect, textBoxPathModEngineDir);
            RegistControlCategory(ControlCategoryTypes.ModEngineDirSelect, buttonBrowseModEngineDir);
            RegistControlCategory(ControlCategoryTypes.ModEngineDirSelect, buttonClearModEngineDir);
            RegistControlCategory(ControlCategoryTypes.CreateButton, buttonCreateProject);
        }

        private Steps CurrentStep = Steps.Start;


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
                NewRefreshSteps();
            }
        }

        private void buttonPathAnibnd_Click(object sender, EventArgs e)
        {
            if (FileType == FileTypes.Chr)
            {
                var anibnd = zzz_DocumentManager.CurrentDocument.GameData.ShowPickInsideBndPath("/chr/", @".*\/c\d\d\d\d\.anibnd\.dcx$", textBoxPathMainBinder.Text, "Choose ANIBND", textBoxPathMainBinder.Text);
                if (anibnd != null)
                {
                    NewSetMainBinder(anibnd);
                    NewRefreshSteps();
                }
            }
            else if (FileType == FileTypes.Parts)
            {
                var partsbnd = zzz_DocumentManager.CurrentDocument.GameData.ShowPickInsideBndPath("/parts/", @".*\.partsbnd.dcx$", textBoxPathMainBinder.Text, "Choose PARTSBND", textBoxPathMainBinder.Text);
                if (partsbnd != null)
                {
                    NewSetMainBinder(partsbnd);
                    NewRefreshSteps();
                }
            }
            else if (FileType == FileTypes.ObjAeg)
            {
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.ERNR)
                {
                    var geombnd = zzz_DocumentManager.CurrentDocument.GameData.ShowPickInsideBndPath("/asset/aeg/", @".*\.geombnd.dcx$", textBoxPathMainBinder.Text, "Choose GEOMBND", textBoxPathMainBinder.Text);
                    if (geombnd != null)
                    {
                        NewSetMainBinder(geombnd);
                        NewRefreshSteps();
                    }
                }
                else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    var geombnd = zzz_DocumentManager.CurrentDocument.GameData.ShowPickInsideBndPath("/asset/environment/geometry/", @".*\.geombnd.dcx$", textBoxPathMainBinder.Text, "Choose GEOMBND", textBoxPathMainBinder.Text);
                    if (geombnd != null)
                    {
                        NewSetMainBinder(geombnd);
                        NewRefreshSteps();
                    }
                }
                else
                {
                    var objbnd = zzz_DocumentManager.CurrentDocument.GameData.ShowPickInsideBndPath("/obj/", @".*\.objbnd.dcx$", textBoxPathMainBinder.Text, "Choose OBJBND", textBoxPathMainBinder.Text);
                    if (objbnd != null)
                    {
                        NewSetMainBinder(objbnd);
                        NewRefreshSteps();
                    }
                }


            }
            else
            {
                throw new NotImplementedException();
            }


        }

        private void buttonPathChrbnd_Click(object sender, EventArgs e)
        {
            if (FileType is FileTypes.Chr)
            {
                var searchDefaultStart = Utils.GetFileNameWithoutAnyExtensions(textBoxPathMainBinder.Text);
                searchDefaultStart = searchDefaultStart.Substring(0, searchDefaultStart.Length - 1);
                var chrbnd = zzz_DocumentManager.CurrentDocument.GameData.ShowPickInsideBndPath("/chr/", @".*\/c\d\d\d\d.chrbnd.dcx$", searchDefaultStart, "Choose CHRBND", textBoxPathMainBinder.Text.Replace(".anibnd.dcx", ".chrbnd.dcx"));
                if (chrbnd != null)
                {
                    textBox_Chr_PathChrbnd.Text = chrbnd;
                    NewRefreshSteps();
                }
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
                NewRefreshSteps();
            }
        }

        private void buttonCreateProject_Click(object sender, EventArgs e)
        {
            SaveJsonCfg();
            NewRefreshSteps();
            if (CurrentStep >= Steps.ProjectSet)
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
                    var cacheDir = $@"{interrootDir}\_DSAS_CACHE";
                    var tempDir = $@"{interrootDir}\_DSAS_TEMP_UNPACK";
                    if (!System.IO.Directory.Exists(cacheDir))
                        System.IO.Directory.CreateDirectory(cacheDir);
                    zzz_DocumentManager.CurrentDocument.GameData.SaveCurrentEblHeaderCaches(cacheDir);
                    zzz_DocumentManager.CurrentDocument.GameRoot.InterrootPath = interrootDir;
                    zzz_DocumentManager.CurrentDocument.GameRoot.InterrootModenginePath = textBoxPathModEngineDir.Text;
                    //zzz_DocumentManager.CurrentDocument.GameRoot.CachePath = cacheDir;
                    //zzz_DocumentManager.CurrentDocument.GameRoot.TempEblUnpackPath = tempDir;
                    zzz_DocumentManager.CurrentDocument.GameRoot.ModengineLoadLooseParams = checkBoxLoadLooseParams.Checked;
                    zzz_DocumentManager.CurrentDocument.GameData.CfgLoadUnpackedGameFiles = checkBoxCfgLoadUnpackedGameFiles.Checked;
                    zzz_DocumentManager.CurrentDocument.GameData.DisableInterrootDCX = checkBoxDisableInterrootDCX.Checked;
                    var projectFile = $@"{projectDir}\_DSAS_WORKSPACE.json";
                    var saveJson = new TaeProjectJson()
                    {
                        GameDirectory = interrootDir,
                        ModEngineDirectory = textBoxPathModEngineDir.Text,
                        LoadLooseParams = checkBoxLoadLooseParams.Checked,
                        LoadUnpackedGameFiles = checkBoxCfgLoadUnpackedGameFiles.Checked,
                        DisableInterrootDCX = checkBoxDisableInterrootDCX.Checked,
                    };
                    saveJson.Save(projectFile, zzz_DocumentManager.CurrentDocument);

                    DSAProj.TaeContainerInfo container = null;

                    if (FileType is FileTypes.Chr)
                    {
                        var anibndFile = zzz_DocumentManager.CurrentDocument.GameData.ReadFile(textBoxPathMainBinder.Text);
                        var anibndOutputPath = $@"{projectDir}\{Utils.GetShortIngameFileName(textBoxPathMainBinder.Text)}.anibnd.dcx";
                        System.IO.File.WriteAllBytes(anibndOutputPath, anibndFile);

                        var chrbndFile = zzz_DocumentManager.CurrentDocument.GameData.ReadFile(textBox_Chr_PathChrbnd.Text);
                        var chrbndOutputPath = $@"{projectDir}\{Utils.GetShortIngameFileName(textBoxPathMainBinder.Text)}.chrbnd.dcx";
                        System.IO.File.WriteAllBytes(chrbndOutputPath, chrbndFile);

                        container = new DSAProj.TaeContainerInfo.ContainerAnibnd(anibndOutputPath, chrbndOutputPath);
                    }
                    else if (FileType is FileTypes.Parts)
                    {
                        var partsbndFile = zzz_DocumentManager.CurrentDocument.GameData.ReadFile(textBoxPathMainBinder.Text);
                        var partsbndOutputPath = $@"{projectDir}\{Utils.GetShortIngameFileName(textBoxPathMainBinder.Text)}.partsbnd.dcx";
                        System.IO.File.WriteAllBytes(partsbndOutputPath, partsbndFile);

                        container = new DSAProj.TaeContainerInfo.ContainerAnibndInBinder(partsbndOutputPath, selectAnibndInBinder_BindIDs[comboBox_PartsObjAeg_SelectAnibndInPartsbnd.SelectedIndex]);
                    }
                    else if (FileType is FileTypes.ObjAeg)
                    {
                        var objbndFile = zzz_DocumentManager.CurrentDocument.GameData.ReadFile(textBoxPathMainBinder.Text);
                        var objbndOutputPath = zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesAEG ? $@"{projectDir}\{Utils.GetShortIngameFileName(textBoxPathMainBinder.Text)}.geombnd.dcx"
                            : $@"{projectDir}\{Utils.GetShortIngameFileName(textBoxPathMainBinder.Text)}.objbnd.dcx";
                        System.IO.File.WriteAllBytes(objbndOutputPath, objbndFile);

                        container = new DSAProj.TaeContainerInfo.ContainerAnibndInBinder(objbndOutputPath, selectAnibndInBinder_BindIDs[comboBox_PartsObjAeg_SelectAnibndInPartsbnd.SelectedIndex]);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    zzz_DocumentManager.CurrentDocument.EditorScreen.SuppressNextModelOverridePrompt = true;
                    zzz_DocumentManager.CurrentDocument.GameRoot.SuppressNextInterrootBrowsePrompt = true;
                    //zzz_DocumentManager.CurrentDocument.EditorScreen.NewLoadFile(container);

                    zzz_DocumentManager.RequestFileOpenRecent = true;
                    zzz_DocumentManager.RequestFileOpenRecent_SelectedFile = container;

                    WizardCompleted = true;
                    Close();

                    //zzz_DocumentManager.CurrentDocument.EditorScreen.FileContainerName_Model = !string.IsNullOrWhiteSpace(textBoxPathChrbnd.Text) ? textBoxPathChrbnd.Text : textBoxPathAnibnd.Text;
                    //// This is really pepega
                    //zzz_DocumentManager.CurrentDocument.EditorScreen.SuppressNextModelOverridePrompt = true;
                    //GameRoot.SuppressNextInterrootBrowsePrompt = true;
                    //zzz_DocumentManager.CurrentDocument.EditorScreen.NewLoadFile();
                    //WizardCompleted = true;
                    //Close();
                }
            }
        }

        private void textBoxPathInterroot_Validating(object sender, CancelEventArgs e)
        {
            NewSetInterroot(textBoxPathInterroot.Text);
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
                NewRefreshSteps();
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
                AnibndPath = textBoxPathMainBinder.Text,
                ChrbndPath = textBox_Chr_PathChrbnd.Text,
                ProjectSaveFolder = textBoxPathSaveLoose.Text,

                ModEngineDirectory = textBoxPathModEngineDir.Text,
                LoadLooseGameParam = checkBoxLoadLooseParams.Checked,

            };

            Main.SaveConfig();
        }

        private void TaeLoadFromArchivesWizard_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveJsonCfg();

            if (!CANCELLED_SAFE_TO_CLOSE && loadEblTask != null && !loadEblTask.IsCompleted)
            {
                e.Cancel = true;
                CANCELLED_SAFE_TO_CLOSE = false;
                CANCELLED = true;
                loadEblTask.ContinueWith(t =>
                {
                    Invoke(new Action(() =>
                    {
                        CANCELLED_SAFE_TO_CLOSE = true;
                        Close();
                    }));
                });
            }
            
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

        private void buttonHelpFileType_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Selects the type of file you wish to load.",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void comboBoxFileType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CurrentStep >= Steps.InterrootSet)
                NewRefreshSteps();
        }

        private void NewSetMainBinder(string mainBinder)
        {
            textBoxPathMainBinder.Text = mainBinder;


            if (FileType == FileTypes.Chr)
            {
                if (textBoxPathMainBinder.Text.ToLower().EndsWith(".anibnd.dcx"))
                    textBox_Chr_PathChrbnd.Text = textBoxPathMainBinder.Text.Substring(0, textBoxPathMainBinder.Text.Length - ".anibnd.dcx".Length) + ".chrbnd.dcx";
                comboBox_PartsObjAeg_SelectAnibndInPartsbnd.SelectedIndex = -1;
                comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Items.Clear();
                selectAnibndInBinder_Binder = null;
                selectAnibndInBinder_Names.Clear();
                selectAnibndInBinder_BindIDs.Clear();
                selectAnibndInBinder_IsEmpty = true;
                selectAnibndInBinder_LastPartsbndText = null;
            }
            else if (FileType is FileTypes.Parts or FileTypes.ObjAeg)
            {
                if (selectAnibndInBinder_LastPartsbndText != textBoxPathMainBinder.Text)
                {
                    selectAnibndInBinder_Binder = zzz_DocumentManager.CurrentDocument.GameData.ReadBinder(textBoxPathMainBinder.Text);
                    if (selectAnibndInBinder_Binder != null)
                    {


                        comboBox_PartsObjAeg_SelectAnibndInPartsbnd.SelectedIndex = -1;
                        comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Items.Clear();
                        selectAnibndInBinder_Names.Clear();
                        selectAnibndInBinder_BindIDs.Clear();
                        selectAnibndInBinder_AnibndsInBinder.Clear();
                        selectAnibndInBinder_IsEmpty = true;
                        foreach (var f in selectAnibndInBinder_Binder.Files)
                        {
                            if (f.ID >= 400 && f.ID < 500)
                            {
                                IBinder bnd = null;
                                if (BND3.IsRead(f.Bytes, out BND3 asBND3))
                                    bnd = asBND3;
                                else if (BND4.IsRead(f.Bytes, out BND4 asBND4))
                                    bnd = asBND4;
                                if (bnd != null)
                                {

                                }
                                selectAnibndInBinder_AnibndsInBinder.Add(bnd);
                                selectAnibndInBinder_Names.Add(f.Name);
                                selectAnibndInBinder_BindIDs.Add(f.ID);
                                comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Items.Add(f.Name);
                                selectAnibndInBinder_IsEmpty = false;
                            }
                        }

                        if (comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Items.Count == 0)
                        {
                            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.SelectedIndex = -1;
                            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Text = "";
                            selectAnibndInBinder_IsEmpty = true;
                        }
                        else
                        {
                            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Text = null;
                            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.SelectedIndex = 0;
                        }
                        selectAnibndInBinder_LastPartsbndText = textBoxPathMainBinder.Text;
                    }


                }
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        private void textBoxPathMainBinder_Validating(object sender, CancelEventArgs e)
        {
            if (CurrentStep < Steps.FileTypeSet)
                return;

            NewSetMainBinder(textBoxPathMainBinder.Text);
            NewRefreshSteps();
        }

        private void comboBox_PartsObjAeg_SelectAnibndInPartsbnd_SelectedIndexChanged(object sender, EventArgs e)
        {
            NewRefreshSteps();
        }

        private void textBox_Chr_PathChrbnd_Validating(object sender, CancelEventArgs e)
        {
            NewRefreshSteps();
        }

        private void textBoxPathSaveLoose_Validating(object sender, CancelEventArgs e)
        {
            NewRefreshSteps();
        }

        private void textBoxPathModEngineDir_Validating(object sender, CancelEventArgs e)
        {
            NewRefreshSteps();
        }

        private void buttonHelpSecondaryBinder_Click(object sender, EventArgs e)
        {
            if (FileType is FileTypes.Chr)
            {
                System.Windows.Forms.MessageBox.Show("Select which model file to to use from within the data archives. " +
                     "You only need to change this from the default if you're editing enemy variants which use multiple models for the same ANIBND file (used a lot in Elden Ring).",
                     "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (FileType is FileTypes.Parts)
            {
                System.Windows.Forms.MessageBox.Show("Select which ANIBND file to load from within the PARTSBND file you chose.",
                    "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (FileType is FileTypes.ObjAeg)
            {
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesAEG)
                    System.Windows.Forms.MessageBox.Show("Select which ANIBND file to load from within the GEOMBND file you chose.",
                        "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    System.Windows.Forms.MessageBox.Show("Select which ANIBND file to load from within the OBJBND file you chose.",
                        "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void buttonHelpPrimaryBinder_Click(object sender, EventArgs e)
        {
            if (FileType is FileTypes.Chr)
            {
                System.Windows.Forms.MessageBox.Show("Select which animation file to edit from within the data archives. " +
                    "See the Dark Souls Modding Wiki for lists of character IDs (http://soulsmodding.wikidot.com/reference:main)." +
                    "The selected file will be unpacked into the workspace directory you select on step 4.",
                    "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (FileType is FileTypes.Parts)
            {
                System.Windows.Forms.MessageBox.Show("Select which PARTSBND file to edit from within the data archives. " +
                    "See the Dark Souls Modding Wiki for lists of Parts IDs for some of the games (http://soulsmodding.wikidot.com/reference:main)." +
                    "The selected file will be unpacked into the workspace directory you select on step 4.",
                    "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (FileType is FileTypes.ObjAeg)
            {
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesAEG)
                {
                    System.Windows.Forms.MessageBox.Show("Select which GEOMBND file to edit from within the data archives. " +
                    "See the Dark Souls Modding Wiki for lists of Asset/AEG IDs for some of the games (http://soulsmodding.wikidot.com/reference:main)." +
                    "The selected file will be unpacked into the workspace directory you select on step 4.",
                    "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Select which OBJBND file to edit from within the data archives. " +
                    "See the Dark Souls Modding Wiki for lists of Object IDs for some of the games (http://soulsmodding.wikidot.com/reference:main)." +
                    "The selected file will be unpacked into the workspace directory you select on step 4.",
                    "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }



        private bool CheckIfBinderHasTaeAndSetError(string binderPath, Control c)
        {
            binderPath = binderPath.ToLower();
            if (!(binderPath.StartsWith("/chr/") || binderPath.StartsWith("/parts/") || binderPath.StartsWith("/obj/") || binderPath.StartsWith("/asset/")))
            {
                return false;
            }

            var binder = zzz_DocumentManager.CurrentDocument.GameData.ReadBinder(binderPath);
            if (binder != null)
            {


                if (SimpleCheckIfBinderHasTae(binder))
                {
                    errorProvider1.SetError(c, null);
                    return true;
                }
                else
                {
                    errorProvider1.SetError(c, "Specified binder does not have TAE files inside of it.");
                    return false;
                }
            }
            else
            {
                errorProvider1.SetError(c, "Specified binder does not exist or could not be loaded for some reason.");
                return false;
            }
        }

        private bool CheckIfBinderHasAnibndsAndSetError(string binderPath, Control c)
        {
            binderPath = binderPath.ToLower();
            if (!(binderPath.StartsWith("/chr/") || binderPath.StartsWith("/parts/") || binderPath.StartsWith("/obj/") || binderPath.StartsWith("/asset/")))
            {
                return false;
            }

            var binder = zzz_DocumentManager.CurrentDocument.GameData.ReadBinder(binderPath);
            if (binder != null)
            {
                bool anyAnibnd = false;
                bool anyAnibndWithTae = false;
                foreach (var bf in binder.Files)
                {
                    if (bf.ID >= 400 && bf.ID < 500)
                    {
                        IBinder anibnd = null;
                        if (BND3.IsRead(bf.Bytes, out BND3 asBND3))
                            anibnd = asBND3;
                        else if (BND4.IsRead(bf.Bytes, out BND4 asBND4))
                            anibnd = asBND4;

                        if (anibnd != null)
                        {
                            anyAnibnd = true;
                            if (SimpleCheckIfBinderHasTae(anibnd))
                            {
                                anyAnibndWithTae = true;
                                break;
                            }
                        }
                    }
                }

                if (anyAnibndWithTae)
                {
                    errorProvider1.SetError(c, null);
                    return true;
                }
                else if (anyAnibnd)
                {
                    errorProvider1.SetError(c, "Specified binder has ANIBND inside but none of them have TAE files.");
                    return false;
                }
                else
                {
                    errorProvider1.SetError(c, "Specified binder does not have any ANIBND inside.");
                    return false;
                }
            }
            else
            {
                errorProvider1.SetError(c, "Specified binder does not exist or could not be loaded for some reason.");
                return false;
            }
        }

        private bool SimpleCheckIfBinderHasTae(IBinder binder)
        {
            bool ver_0001 = binder.Files.Any(f => f.ID == 9999999);
            int taeFileIDRangeMin = ver_0001 ? 5000000 : 3000000;
            int taeFileIDRangeMax = ver_0001 ? 5999999 : 3999999;
            foreach (var bf in binder.Files)
            {
                if (bf.ID >= taeFileIDRangeMin && bf.ID <= taeFileIDRangeMax)
                {
                    return true;
                }
            }
            return false;
        }

        private void NewSetMainBinderError(string error)
        {

            errorProvider1.SetError(textBoxPathMainBinder, error);
        }

        private void NewSetSecondaryBinderError(string error)
        {
            if (FileType is FileTypes.Chr)
            {
                errorProvider1.SetError(textBox_Chr_PathChrbnd, error);
            }
            else if (FileType is FileTypes.Parts or FileTypes.ObjAeg)
            {
                errorProvider1.SetError(comboBox_PartsObjAeg_SelectAnibndInPartsbnd, error);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void buttonHelpDisableInterrootDCX_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Removes .dcx extension from all paths in the game data directory. For use with DS1 PTDE unpacked via UDSFM." +
                "\n\nYou definitely want to enable Load Unpacked Game Files with this option.",
                "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void TaeLoadFromArchivesWizard_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}