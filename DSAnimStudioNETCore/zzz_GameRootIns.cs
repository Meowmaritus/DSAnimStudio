using SoulsAssetPipeline;
using SoulsAssetPipeline.Animation;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio
{
    public class zzz_GameRootIns
    {
        public zzz_DocumentIns ParentDocument;
        public zzz_GameRootIns(zzz_DocumentIns parentDocument)
        {
            ParentDocument = parentDocument;
        }

        public bool IsChrPlayerChr(string anibndID)
        {
            return (anibndID == "c0000" || anibndID == "c0000_0000")
                || (anibndID.StartsWith("c0") && GameType == SoulsGames.AC6);
        }

        //private object _lock_AdditionalPlayerAnibndNames = new object();
        private List<string> GetChrAdditionalAnibndNames(string chrID)
        {

            string dir = "/chr";
            if (ParentDocument.GameRoot.GameType is SoulsGames.DES)
                dir += $"/{chrID}";

            IBinder anibnd = ParentDocument.GameData.ReadBinder($"{dir}/{chrID}.anibnd.dcx");


            var result = new List<string>();

            if (anibnd != null)
            {
                bool ver_0001 = anibnd.Files.Any(f => f.ID == 9999999) && ParentDocument.GameRoot.GameType != SoulsGames.DES;

                int additionalAnibndBindIDStart = ver_0001 ? 6000000 : 4000000;

                foreach (var bf in anibnd.Files)
                {
                    if (bf.ID >= additionalAnibndBindIDStart && bf.ID <= additionalAnibndBindIDStart + 99999)
                    {
                        result.Add($"{dir}/{Utils.GetShortIngameFileName(bf.Name)}.anibnd.dcx");
                    }
                    else if (ver_0001 && bf.ID >= 6200000 && bf.ID < 6300000)
                    {
                        var name = Utils.GetShortIngameFileName(bf.Name);
                        if (name.ToLower().StartsWith("delayload_") && ParentDocument.GameRoot.GameTypeUsesDelayloadAnibnd)
                            name = name.Substring("delayload_".Length);
                        result.Add($"{dir}/{name}.anibnd.dcx");
                    }
                    else if (ver_0001 && bf.ID >= 6300000 && bf.ID < 6400000 && GameTypeUsesDivAnibnd)
                    {
                        result.Add($"{dir}/{Utils.GetShortIngameFileName(bf.Name)}.anibnd.dcx");
                    }
                }
            }

            // ERNR TODO - If DLC is ever added lol
            if (ParentDocument.GameRoot.GameType is SoulsGames.ER && chrID == "c0000")
            {
                result.Add("/chr/c0000_dlc01.anibnd.dcx");
                result.Add("/chr/c0000_dlc02.anibnd.dcx");
            }


            return result;


            //List<string> result = null;
            //lock (_lock_AdditionalPlayerAnibndNames)
            //{
            //    var fileName = $@"{Main.Directory}\Res\PlayerAnibnds.{GameType}.txt";
            //    if (File.Exists(fileName))
            //        result = File.ReadAllLines(fileName).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            //}
            //return result;
        }



        public bool IsBigEndianGame => GameType == SoulsAssetPipeline.SoulsGames.DES && !GameIsDemonsSoulsRemastered;
        //public enum SoulsGames
        //{
        //    None,
        //    DES,
        //    DS1,
        //    DS1R,
        //    //DS2,
        //    DS2SOTFS,
        //    DS3,
        //    BB,
        //    SDT,
        //}

        public const string ModelImportBackupExtension = ".mdibak";

        //public bool GameTypeLoads2010Files => (GameType == SoulsGames.DES);

        public enum AnimIDFormattingType
        {
            aXX_YYYY,
            aXXX_YYYYYY,
            aXX_YY_ZZZZ,
            Unknown,
        }

        public NewSkeletonMapper.RemapModes CurrentGameSkeletonRemapperMode => GameType is SoulsGames.AC6 ? NewSkeletonMapper.RemapModes.RetargetRelativeAndDirectFKOrientation : NewSkeletonMapper.RemapModes.DirectBoneMap;

        public string GameTypeName => GameTypeNames.ContainsKey(GameType) ? GameTypeNames[GameType] : GameType.ToString();

        public readonly Dictionary<SoulsGames, string> GameTypeNames =
            new Dictionary<SoulsGames, string>
        {
            { SoulsGames.None, "<NONE>" },
            { SoulsGames.DES, "Demon's Souls" },
            { SoulsGames.DS1, "Dark Souls: Prepare to Die Edition" },
            { SoulsGames.DS1R, "Dark Souls Remastered" },
            //{ GameTypes.DS2, "Dark Souls II" },
            { SoulsGames.DS2SOTFS, "Dark Souls II: Scholar of the First Sin" },
            { SoulsGames.DS3, "Dark Souls III" },
            { SoulsGames.BB, "Bloodborne" },
            { SoulsGames.SDT, "Sekiro: Shadows Die Twice" },
        };

        public bool CheckGameTypeParamIDCompatibility(SoulsGames a, SoulsGames b)
        {
            if (a == SoulsGames.DS1 && b == SoulsGames.DS1R)
                return true;
            else if (a == SoulsGames.DS1R && b == SoulsGames.DS1)
                return true;
            // TODO: Check if these DS2 ones would be a good idea with 
            // Forlorn set only being in sotfs etc.
            //if (a == GameTypes.DS2 && b == GameTypes.DS2SOTFS)
            //    return true;
            //if (a == GameTypes.DS2SOTFS && b == GameTypes.DS2)
            //    return true;
            else
                return a == b;
        }

        private SoulsGames lastGameType = SoulsGames.None;
        public SoulsGames GameType { get; set; } = SoulsGames.None;

        /// <summary>
        /// This just makes DS1 and DS1R both show up as DS1 for TAE stuff since they have same TAE files exactly
        /// </summary>
        public SoulsGames GameTypeForTAE
        {
            get
            {
                var gameType = GameType;
                if (gameType == SoulsGames.DS1R)
                    gameType = SoulsGames.DS1;
                return gameType;
            }
        }

        //public bool GameTypeUsesWwise => GameType is SoulsGames.ER or SoulsGames.AC6;


        public bool GameTypeUsesRegulation => GameType is SoulsGames.DS3 or SoulsGames.DS2SOTFS or SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6;

        public bool GameTypeUsesDivAnibnd => GameType is SoulsGames.ER or SoulsGames.AC6 or SoulsGames.ERNR;
        public bool GameTypeUsesDelayloadAnibnd => GameType is SoulsGames.DS1 or SoulsGames.DS1R or SoulsGames.SDT;

        public void ClearInterroot()
        {
            GameType = SoulsGames.None;
            //TempEblUnpackPath = null;
            //CachePath = null;
            ProjectPath = null;
            DefaultGameDir = null;
            InterrootPath = null;
            InterrootModenginePath = null;
            SuppressNextInterrootBrowsePrompt = false;
        }

        public bool IsGame(SoulsGames gameTypes)
        {
            return (GameType & gameTypes) != 0;
        }

        //public IBinder ReadBinder(string path)
        //{
        //    if (BND3.Is(path))
        //        return BND3.Read(path);
        //    else if (BND4.Is(path))
        //        return BND4.Read(path);
        //    else
        //        throw new InvalidDataException($"Tried to read file '{path}' as a BND3 or BND4 but it was neither.");
        //}

        public IBinder ReadBinder(byte[] data)
        {
            if (BND3.Is(data))
                return BND3.Read(data);
            else if (BND4.Is(data))
                return BND4.Read(data);
            else
                throw new InvalidDataException($"Tried to read block of data as a BND3 or BND4 but it was neither.");
        }

        public bool GameTypeHasLongAnimIDs => CurrentAnimIDFormatType == AnimIDFormattingType.aXXX_YYYYYY ||
            CurrentAnimIDFormatType == AnimIDFormattingType.aXX_YY_ZZZZ;

        public bool GameTypeUsesMaleForAllProtectorParts => GameType is SoulsGames.SDT or SoulsGames.AC6;
        public bool GameTypeUsesOldShittyFacegen => GameType is SoulsGames.DES;
        public bool GameTypeSupportsPartsSuffixM => GameType is SoulsGames.DS1 or SoulsGames.DS1R;
        public bool GameTypeSupportsPartsSuffixL => true;
        public bool GameTypeSupportsPartsSuffixU => GameType is SoulsGames.AC6;
        public int GameTypeUpperAnimIDModBy => (GameTypeHasLongAnimIDs ? (1_000000) : (1_0000));
        public bool GameTypeUsesMetallicShaders => GameType is SoulsGames.SDT or SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6;
        public bool GameTypeUsesLegacyEmptyEventGroups => GameType is SoulsGames.DES or SoulsGames.DS1 or SoulsGames.DS1R;
        public bool GameTypeUsesAEG => GameType is SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6;
        public bool GameTypeUsesWepAbsorpPosParam => GameType is SoulsGames.DS3 or SoulsGames.SDT or SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6;

        public AnimIDFormattingType GetAnimIDFormattingType(SoulsGames game)
        {
            switch (game)
            {
                case SoulsGames.DS1:
                case SoulsGames.DS1R:
                case SoulsGames.DES:
                    return AnimIDFormattingType.aXX_YYYY;
                case SoulsGames.BB:
                case SoulsGames.DS3:
                case SoulsGames.SDT:
                case SoulsGames.ER:
                case SoulsGames.ERNR:
                case SoulsGames.AC6:
                    return AnimIDFormattingType.aXXX_YYYYYY;
                case SoulsGames.DS2SOTFS:
                    return AnimIDFormattingType.aXX_YY_ZZZZ;
                default:
                    return AnimIDFormattingType.Unknown;
            }
        }

        public AnimIDFormattingType CurrentAnimIDFormatType
        {
            get
            {
                return GetAnimIDFormattingType(GameType);
            }
        }

        public int CurrentGameAnimUpperIDMax =>
            CurrentAnimIDFormatType == AnimIDFormattingType.aXXX_YYYYYY ? 999 : 255;

        public int CurrentGameAnimLowerIDMax =>
            CurrentAnimIDFormatType == AnimIDFormattingType.aXXX_YYYYYY ? 999999 : 9999;

        //DESR TODO
        public bool GameIsDemonsSoulsRemastered = false;

        public bool GameTypeIsHavokTagfile =>
            (GameType is SoulsGames.DS1R or SoulsGames.SDT or SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6) || GameIsDemonsSoulsRemastered;

        public bool GameTypeIsGiant => GameType is SoulsGames.AC6;

        public HKX.HKXVariation GetCurrentLegacyHKXType()
        {
            if (GameType == SoulsGames.DES && !GameIsDemonsSoulsRemastered)
                return HKX.HKXVariation.HKXDeS;
            else if (GameType == SoulsGames.DS1 || GameType == SoulsGames.DS1R)
                return HKX.HKXVariation.HKXDS1;
            else if (GameType == SoulsGames.DS3)
                return HKX.HKXVariation.HKXDS3;
            else if (GameType == SoulsGames.BB)
                return HKX.HKXVariation.HKXBloodBorne;
            else if ((GameType is SoulsGames.SDT or SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6) || GameIsDemonsSoulsRemastered)
                return HKX.HKXVariation.HKXDS1;

            return HKX.HKXVariation.Invalid;
        }

        public string TempEblUnpackPath => (InterrootPath != null) ? (InterrootPath + @"\_DSAS_TEMP_UNPACK") : null;
        public string CachePath => (InterrootPath != null) ? (InterrootPath + @"\_DSAS_CACHE") : null;
        public string ProjectPath { get; set; } = null;
        public string DefaultGameDir { get; set; } = null;
        public string InterrootPath { get; set; } = null;
        public string InterrootModenginePath { get; set; } = null;
        public bool ModengineLoadLooseParams { get; set; } = true;

        public bool SuppressNextInterrootBrowsePrompt = false;

        public string GetInterrootPathOld(string path, bool isDirectory = false, bool doNotCheckIfExists = false)
        {
            //if (doNotCheckIfExists)
            //{
            //    return $@"{InterrootPath}/{path}";
            //}

            if (path.StartsWith("/"))
                path = path.Substring(1);

            if (!string.IsNullOrWhiteSpace(InterrootModenginePath) && (isDirectory ? Directory.Exists($@"{InterrootModenginePath}/{path}") : File.Exists($@"{InterrootModenginePath}/{path}")))
            {
                return $@"{InterrootModenginePath}/{path}";
            }
            else if (isDirectory ? Directory.Exists($@"{InterrootPath}/{path}") : File.Exists($@"{InterrootPath}/{path}"))
            {
                return $@"{InterrootPath}/{path}";
            }
            else
            {
                if (!doNotCheckIfExists)
                    ErrorLog.LogWarning($@"Unable to find the path './{path}' in the main data root directory ('{InterrootPath}') or the ModEngine directory.");

                return $@"{InterrootPath}/{path}";
            }
        }

        public List<string> GetInterrootFiles(string path, string match)
        {
            IEnumerable<string> result = new List<string>();

            string mainPath = $@"{InterrootPath}/{path}";
            if (Directory.Exists(mainPath))
            {
                result = result.Concat(Directory.GetFiles(mainPath, match));
            }

            if (!string.IsNullOrWhiteSpace(InterrootModenginePath))
            {
                string parentPath = $@"{InterrootModenginePath}/{path}";
                if (Directory.Exists(parentPath))
                {
                    result = result.Concat(Directory.GetFiles(parentPath, match));
                }
            }

            return result.ToList();
        }

        public void LoadMTDBND()
        {
            FlverMaterialDefInfo.UnloadAll();

            if (GameType == SoulsGames.DS1 || GameType == SoulsGames.DES)
            {
                FlverMaterialDefInfo.LoadMaterialBinders($@"/mtd/Mtd.mtdbnd.dcx", null);
            }
            else if (GameType == SoulsGames.DS1R)
            {
                FlverMaterialDefInfo.LoadMaterialBinders($@"/mtd/Mtd.mtdbnd.dcx", $@"/mtd/MtdPatch.mtdbnd.dcx");
            }
            else if (GameType == SoulsGames.DS2 || GameType == SoulsGames.DS2SOTFS)
            {
                FlverMaterialDefInfo.LoadMaterialBinders($@"/material/allmaterialbnd.bnd", null);
            }
            else if (GameType == SoulsGames.BB || GameType == SoulsGames.DS3 || GameType == SoulsGames.SDT)
            {
                FlverMaterialDefInfo.LoadMaterialBinders($@"/mtd/allmaterialbnd.mtdbnd.dcx", null);
                if (GameType == SoulsGames.DS3 || GameType == SoulsGames.SDT)
                {
                    FlverMaterialDefInfo.LoadShaderBdleBinder($@"/shader/shaderbdle.shaderbdlebnd.dcx", $@"/shader/gxflvershader.shaderbnd.dcx");
                }
            }
            else if (GameType is SoulsGames.ER)
            {
                FlverMaterialDefInfo.LoadMaterialBinders($@"/material/allmaterial.matbinbnd.dcx", $@"/material/allmaterial_dlc01.matbinbnd.dcx", $@"/material/allmaterial_dlc02.matbinbnd.dcx");
            }
            else if (GameType is SoulsGames.ERNR)
            {
                // ERNR TODO - IF DLC IS EVER ADDED
                FlverMaterialDefInfo.LoadMaterialBinders($@"/material/allmaterial.matbinbnd.dcx", null);
            }
            else if (GameType is SoulsGames.AC6)
            {
                FlverMaterialDefInfo.LoadMaterialBinders($@"/material/allmaterial.matbinbnd.dcx", null);
            }
        }

        public bool InitializeFromBND(string bndPath)
        {
            IBinder bnd = null;
            if (BND3.Is(bndPath))
                bnd = BND3.Read(bndPath);
            else if (BND4.Is(bndPath))
                bnd = BND4.Read(bndPath);

            return InitializeFromBND(bndPath, bnd);
        }

        public void ShowUnableToDetermineGameTypeError()
        {
            Main.MainThreadLazyDispatch(() =>
            {
                System.Windows.Forms.MessageBox.Show("Unable to determine what game this file is for. Cancelling load operation.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private bool ManuallyAskInitialize(string bndPath, IBinder bnd, string scratchFolder)
        {
            var form = new FormAskWhatGame();
            if (form.ShowDialog() == DialogResult.OK)
            {
                ParentDocument.GameRoot.Init(bndPath, form.SelectedGame, scratchFolder);
                return true;
            }
            else
            {
                Main.REQUEST_REINIT_EDITOR = true;
                return false;
            }
        }

        public bool InitializeFromBND(string bndPath, IBinder bnd)
        {
            string scratchFolder = Path.GetDirectoryName(bndPath);

            var detectedGame = SoulsGames.None;

            foreach (var f in bnd.Files)
            {
                var check = f.Name.ToUpper().Replace("\\", "/");

                if (Main.Input.ShiftHeld)
                {
                    return ManuallyAskInitialize(bndPath, bnd, scratchFolder);
                }

                if (check.Contains("FRPG2"))
                {
                    ParentDocument.GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.DS2SOTFS, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/FRPG/") && check.Contains(@"INTERROOT_X64"))
                {
                    ParentDocument.GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.DS1R, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/FRPG/") && check.Contains(@"INTERROOT_WIN32"))
                {
                    ParentDocument.GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.DS1, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/SPRJ/"))
                {
                    ParentDocument.GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.BB, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/FDP/"))
                {
                    ParentDocument.GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.DS3, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/DemonsSoul/"))
                {
                    ParentDocument.GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.DES, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/NTC/"))
                {
                    ParentDocument.GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.SDT, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/GR/"))
                {
                    ParentDocument.GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.ER, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/CL/"))
                {
                    ParentDocument.GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.ERNR, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/FNR/"))
                {
                    ParentDocument.GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.AC6, scratchFolder);
                    return true;
                }
                else
                {
                    return ManuallyAskInitialize(bndPath, bnd, scratchFolder);
                }
            }

            return false;

        }

        public string GetInterrootFromFilePath(string filePath)
        {
            var folder = new System.IO.FileInfo(filePath).DirectoryName;

            var lastSlashInFolder = folder.LastIndexOf("\\");

            return folder.Substring(0, lastSlashInFolder);
        }

        public string GetParentDirectory(string directoryPath)
        {
            var lastSlashInFolder = directoryPath.LastIndexOf("\\");

            return directoryPath.Substring(0, lastSlashInFolder);
        }

        public void SoftInit(SoulsGames gameType)
        {
            GameType = gameType;
            lastGameType = SoulsGames.None;
        }

        public void SetInterrootPickerPathsManually(string projectDir, string gameDir, string modengineDir, bool loadLooseParams)
        {
            var jsonData = new TaeEditor.TaeProjectJson()
            {
                GameDirectory = gameDir,
                ModEngineDirectory = modengineDir,
                LoadLooseParams = loadLooseParams,
            };
            jsonData.Save($@"{projectDir}\_DSAS_WORKSPACE.json", ParentDocument);
        }

        public void Init(string assetPath, SoulsGames gameType, string scratchFolder, bool forceReload = false, bool overrideInterrootPicker = false)
        {
            if (GameType == SoulsGames.None || string.IsNullOrWhiteSpace(InterrootPath))
                forceReload = true;
            GameType = gameType;

            

            if (gameType != lastGameType || forceReload)
            {
                ProjectPath = $"{Path.GetDirectoryName(assetPath)}/_DSAS_WORKSPACE.json";
                DefaultGameDir = Path.GetFullPath(Path.GetDirectoryName(assetPath) + (gameType is SoulsGames.DES ? "\\..\\..\\" : "\\..\\"));
                if (!overrideInterrootPicker)
                {
                    var directoryDialog = new TaeEditor.TaeGameDirPicker();
                    directoryDialog.StartInCenterOf(Main.WinForm);
                    bool needsToBeShown = directoryDialog.InitAndCheckIfNeedsToBeShownLol();

                    if (SuppressNextInterrootBrowsePrompt)
                    {
                        needsToBeShown = false;
                        SuppressNextInterrootBrowsePrompt = false;
                    }

                    if (needsToBeShown)
                    {
                        if (directoryDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        {
                            Main.REQUEST_REINIT_EDITOR = true;
                            zzz_DocumentManager.ClearAllRequests();
                            return;
                        }
                    }

                    InterrootPath = directoryDialog.SelectedGameDir;
                    InterrootModenginePath = directoryDialog.SelectedModengineDir;
                    //TempEblUnpackPath = InterrootPath + @"\_DSAS_TEMP_UNPACK";
                    //CachePath = InterrootPath + @"\_DSAS_CACHE";
                    ParentDocument.GameData.CfgLoadLooseParams = directoryDialog.LoadLooseParams;
                    ParentDocument.GameData.CfgLoadUnpackedGameFiles = directoryDialog.LoadUnpackedGameFiles;
                    ParentDocument.GameData.DisableInterrootDCX = directoryDialog.DisableInterrootDCX;
                }

                if (gameType != SoulsGames.None)
                {
                    ParentDocument.GameData.InitEBLs();

                    //var test = GameEbl.ReadFileFromEBLs(@"/action/eventnameid.txt");
                    //var testMeme = new BinaryReaderEx(false, test);
                    //var testMemeStr = testMeme.GetASCII(0, test.Length);

                    //ParamManager.LoadParamBND(forceReload);
                    ParentDocument.ParamManager.LoadParamBND(true);
                    //FmgManager.LoadAllFMG(forceReload);
                    ParentDocument.FmgManager.LoadAllFMG(true);
                    LoadSystex();
                    LoadMTDBND();

                    //FmodManager.Purge();
                    //ParentDocument.SoundManager.FmodManager.UpdateInterrootForFile(null);
                    ParentDocument.Fmod.LoadFloorMaterialNamesFromInterroot();

                    ParentDocument.SoundManager.SetEngineToCurrentGame(GameType);
                }




            }
            lastGameType = GameType;
        }

        public void ReloadAllData()
        {
            ReloadParams();
            ReloadFmgs();
        }

        public void ReloadParams()
        {
            ParentDocument.ParamManager.LoadParamBND(forceReload: true);
        }

        public void ReloadFmgs()
        {
            ParentDocument.FmgManager.LoadAllFMG(forceReload: true);
        }

        //public enum MapModelSearchType
        //{
        //    CHR,
        //    OBJ,
        //    MAP
        //}
        //public string GetMapThatModelIsIn(string modelName, MapModelSearchType searchType)
        //{
        //    if (GameType == GameTypes.DS3)
        //    {
        //        foreach (var msbName in System.IO.Directory.GetFiles(GameData.ReadFile($@"map/MapStudio", "*.msb.dcx")))
        //        {
        //            var shortMsbName = Utils.GetShortIngameFileName(msbName);
        //            var msb = MSB3.Read(msbName);

        //            if (searchType == MapModelSearchType.CHR)
        //            {
        //                foreach (var m in msb.Models.Enemies)
        //                    if (m.Name == modelName)
        //                        return shortMsbName;
        //            }
        //            else if (searchType == MapModelSearchType.OBJ)
        //            {
        //                foreach (var m in msb.Models.Objects)
        //                    if (m.Name == modelName)
        //                        return shortMsbName;
        //            }
        //            else if (searchType == MapModelSearchType.MAP)
        //            {
        //                foreach (var m in msb.Models.MapPieces)
        //                    if (m.Name == modelName)
        //                        return shortMsbName;
        //            }
        //        }
        //    }
        //}


        public void LoadSystex()
        {
            ParentDocument.LoadingTaskMan.DoLoadingTask("LoadSystex", "Loading SYSTEX textures...", progress =>
            {
                if (GameType == SoulsGames.DES)
                {
                    ParentDocument.TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        ParentDocument.GameData.ReadFile($@"/other/SYSTEX_TEX.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/envlight.tpf"),
                        ParentDocument.GameData.ReadFile($@"/other/lensflare.tpf"),
                        ParentDocument.GameData.ReadFile($@"/parts/common_body.tpf"),
                        ParentDocument.GameData.ReadFile($@"/parts/common_ghost.tpf"),
                        ParentDocument.GameData.ReadFile($@"/facegen/facegen.tpf"),
                    }, progress);

                    ParentDocument.TexturePool.AddTextureBnd(ParentDocument.GameData.ReadBinder($@"/facegen/FaceGen.fgbnd"));
                }
                else if (GameType is SoulsGames.DS1 or SoulsGames.DS1R)
                {
                    ParentDocument.TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        ParentDocument.GameData.ReadFile($@"/other/SYSTEX_TEX.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/envlight.tpf"),
                        ParentDocument.GameData.ReadFile($@"/other/lensflare.tpf"),
                        ParentDocument.GameData.ReadFile($@"/parts/Common_Body.tpf"),
                        ParentDocument.GameData.ReadFile($@"/parts/Common_Body_M.tpf"),
                        ParentDocument.GameData.ReadFile($@"/parts/Common_Ghost.tpf"),
                    }, progress);

                    ParentDocument.TexturePool.AddTextureBnd(ParentDocument.GameData.ReadBinder($@"/facegen/FaceGen.fgbnd"));
                }
                else if (GameType == SoulsGames.DS3)
                {
                    ParentDocument.TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        ParentDocument.GameData.ReadFile($@"/other/systex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/bloodtex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/decaltex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/sysenvtex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/parts/common_body.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == SoulsGames.BB)
                {
                    // TODO: completely confirm these because I just
                    // copied them from a BB network test file list.
                    ParentDocument.TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        ParentDocument.GameData.ReadFile($@"/other/SYSTEX.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/decaltex_0.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/decaltex_1.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/decaltex_2.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/decaltex_3.tpf.dcx"),
                        //ParentDocument.GameData.ReadFile($@"/other/bloodTex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/parts/common_body.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == SoulsGames.SDT)
                {
                    ParentDocument.TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        ParentDocument.GameData.ReadFile($@"/other/systex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/maptex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/decaltex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/parts/common_body.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == SoulsGames.ER)
                {
                    ParentDocument.TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        ParentDocument.GameData.ReadFile($@"/other/systex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/sysenvtex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/decaltex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/parts/common_body.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == SoulsGames.ERNR)
                {
                    ParentDocument.TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        ParentDocument.GameData.ReadFile($@"/other/systex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/sysenvtex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/decaltex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/parts/common_body.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == SoulsGames.AC6)
                {
                    ParentDocument.TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        ParentDocument.GameData.ReadFile($@"/other/systex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/other/decaltex.tpf.dcx"),
                        ParentDocument.GameData.ReadFile($@"/parts/common_body_m.tpf.dcx"),
                    }, progress);
                }
                //else
                //{
                //    throw new NotImplementedException($"Not implemented for GameType {GameType}.");
                //}
                ParentDocument.Scene.RequestTextureLoad();
            });


        }

        public Model NewLoadObj(string id, int modelIndex)
        {
            ParentDocument.Scene.DisableModelDrawing();

            Model mdl = null;

            ParentDocument.LoadingTaskMan.DoLoadingTaskSynchronous($"NEW_LOAD_OBJ_{id}", $"Loading object {id}...", progress =>
            {

                string objbndPath = $"/obj/{id}.objbnd.dcx";
                var objbnd = ParentDocument.GameData.ReadBinder(objbndPath);
                mdl = new Model(ParentDocument, progress, id, objbnd, modelIndex: modelIndex, null, ignoreStaticTransforms: true);
                if (modelIndex > 0)
                    mdl.Name += $"_{modelIndex}";
                ParentDocument.Scene.AddModel(mdl);
            });

            ParentDocument.Scene.EnableModelDrawing();
            return mdl;

        }

        public Model NewLoadParts(string id, int modelIndex)
        {
            ParentDocument.Scene.DisableModelDrawing();

            Model mdl = null;

            ParentDocument.LoadingTaskMan.DoLoadingTaskSynchronous($"NEW_LOAD_PARTS_{id}", $"Loading parts {id}...", progress =>
            {
                string objbndPath = $"/parts/{id}.partsbnd.dcx";
                var objbnd = ParentDocument.GameData.ReadBinder(objbndPath);
                mdl = new Model(ParentDocument, progress, id, objbnd, modelIndex: modelIndex, null, ignoreStaticTransforms: true);
                if (modelIndex > 0)
                    mdl.Name += $"_{modelIndex}";
                ParentDocument.Scene.AddModel(mdl);
            });

            ParentDocument.Scene.EnableModelDrawing();
            return mdl;

        }

        public Model LoadObject(string id)
        {
            ParentDocument.Scene.DisableModelDrawing();
            ParentDocument.Scene.DisableModelDrawing2();

            Model obj = null;

            ParentDocument.LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_OBJ_{id}", $"Loading object {id}...", progress =>
            {

                if (GameType is SoulsGames.DS3 or SoulsGames.BB or SoulsGames.SDT or SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6)
                {
                    var chrbnd = BND4.Read(ParentDocument.GameData.ReadFile($@"obj/{id}.objbnd.dcx"));

                    obj = new Model(ParentDocument, progress, id, chrbnd, 0, null, null);
                }
                else if (GameType == SoulsGames.DS1)
                {
                    var chrbnd = BND3.Read(ParentDocument.GameData.ReadFile($@"obj/{id}.objbnd"));

                    obj = new Model(ParentDocument, progress, id, chrbnd, 0, null, null);
                }
                else if (GameType == SoulsGames.DS1R)
                {
                    var chrbnd = BND3.Read(ParentDocument.GameData.ReadFile($@"obj/{id}.objbnd.dcx"));

                    obj = new Model(ParentDocument, progress, id, chrbnd, 0, null, null);
                }
                else
                {
                    throw new NotImplementedException($"Not implemented for GameType {GameType}.");
                }

                ParentDocument.Scene.AddModel(obj);
                //Scene.SetMainModelToFirst(hideNonMainModels: true);



                if (obj.MainMesh != null)
                {
                    var texturesToLoad = obj.MainMesh.GetAllTexNamesToLoad();

                    ParentDocument.LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_OBJ_{id}_TEX",
                        "Loading additional object textures...", innerProgress =>
                        {
                            if (GameType == SoulsGames.DS1)
                            {
                                foreach (var tex in texturesToLoad)
                                {
                                    ParentDocument.TexturePool.AddTpfFromFile(ParentDocument.GameData.ReadFile($@"map/tx/{tex}.tpf"));
                                }
                            }
                            else if (GameType == SoulsGames.DS3 || GameType == SoulsGames.SDT || GameType == SoulsGames.BB)
                            {
                                int objGroup = int.Parse(id.Substring(1)) / 1_0000;
                                string mapDirName = $@"/map/m{objGroup:D2}";
                                var tpfBnds = ParentDocument.GameData.SearchFiles(mapDirName, @".*\.tpfbhd");
                                foreach (var t in tpfBnds)
                                    ParentDocument.TexturePool.AddSpecificTexturesFromBinder(t, texturesToLoad, directEntryNameMatch: true);
                            }
                            obj.MainMesh.TextureReloadQueued = true;
                        });
                }





            });

            ParentDocument.Scene.EnableModelDrawing();
            ParentDocument.Scene.EnableModelDrawing2();

            return obj;

        }

        public List<string> GetModelFilesOfChr(string name)
        {
            List<string> result = new List<string>();

            string chrFolder = (GameType == SoulsGames.DS2 || GameType == SoulsGames.DS2SOTFS) ? @"/model/chr" : "/chr";
            var filesRelatedToThisChr = ParentDocument.GameData.SearchFiles(chrFolder, $".*{name}.*");
            foreach (var f in filesRelatedToThisChr)
            {
                string file = f.ToLower();
                if (file.EndsWith(".bak") || file.EndsWith(".dsasbak") || file.Contains("anibnd") //|| file.Contains(".2010")
                    || file.EndsWith(ModelImportBackupExtension))
                    continue;

                result.Add(f);
            }

            if (GameType == SoulsGames.DS1)
            {
                string looseTexFolder = Path.Combine(InterrootPath, $@"/chr/{name}");
                if (Directory.Exists(looseTexFolder))
                {
                    foreach (var tpf in Directory.GetFiles(looseTexFolder, "*.tpf"))
                    {
                        result.Add(tpf);
                    }
                }

                if (name[4] != '9') //lol
                {
                    var memeOverrideTextureFiles = GetModelFilesOfChr(name.Substring(0, 4)/*cXXX*/ + "9");
                    result.AddRange(memeOverrideTextureFiles);
                }
            }

            return result;
        }

        public void CreateCharacterModelBackup(string name, bool isAsync = false)
        {
            ParentDocument.LoadingTaskMan.DoLoadingTask($"GameDataManagerCreateCharacterModelBackup_{name}", $"Creating backup of character model '{name}'", prog =>
            {
                var chrModelFiles = GetModelFilesOfChr(name);
                foreach (var f in chrModelFiles)
                {
                    string bakPath = f + ModelImportBackupExtension;
                    if (!File.Exists(bakPath))
                        File.Copy(f, bakPath);
                }
            }, flags: zzz_LoadingTaskManIns.TaskFlags.AllowsInteraction, waitForTaskToComplete: !isAsync);
        }

        public void RestoreCharacterModelBackup(string name, bool isAsync = false)
        {
            ParentDocument.LoadingTaskMan.DoLoadingTask($"GameDataManagerRestoreCharacterModelBackup_{name}", $"Restoring backup of character model '{name}'", prog =>
            {
                var chrModelFiles = GetModelFilesOfChr(name);
                foreach (var f in chrModelFiles)
                {
                    string bakPath = f + ModelImportBackupExtension;
                    if (File.Exists(bakPath))
                        File.Copy(bakPath, f, overwrite: true);
                }
            }, flags: zzz_LoadingTaskManIns.TaskFlags.AllowsInteraction, waitForTaskToComplete: !isAsync);

        }

        public Model LoadMapPiece(int area, int block, int x, int y, int pieceID)
        {

            Model chr = null;

            ParentDocument.Scene.DisableModelDrawing();
            ParentDocument.Scene.DisableModelDrawing2();


            if (GameType == SoulsGames.DS1 || GameType == SoulsGames.DS1R)
            {
                string fullMapID = $"m{area:D2}_{block:D2}_{x:D2}_{y:D2}";
                string mapPieceName = $"m{pieceID:D4}B{block}A{area}";
                byte[] mapPieceFileBytes = ParentDocument.GameData.ReadFile($@"/map/{fullMapID}/{mapPieceName}.flver.dcx");

                ParentDocument.LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_MAPPIECE_{fullMapID}_{mapPieceName}", $"Loading map piece {mapPieceName} from map {fullMapID}...", progress =>
                {
                    var flver = FLVER2.Read(mapPieceFileBytes);
                    chr = new Model(ParentDocument, flver, false);

                    List<string> texturesUsed = new List<string>();
                    foreach (var mat in flver.Materials)
                    {
                        foreach (var tex in mat.Textures)
                        {
                            if (string.IsNullOrWhiteSpace(tex.Path))
                                continue;
                            var texName = Utils.GetShortIngameFileName(tex.Path);
                            texturesUsed.Add(texName);
                        }
                    }

                    ParentDocument.TexturePool.AddMapTextures(area, texturesUsed);
                });
            }
            else
            {
                throw new NotImplementedException("ONLY FOR DS1(R)");
            }



            ParentDocument.Scene.EnableModelDrawing();
            ParentDocument.Scene.EnableModelDrawing2();

            return chr;
        }

        public int GetChrModelCount(IBinder chrbnd)
        {
            int maxModelIndex = 0;

            foreach (var f in chrbnd.Files)
            {
                if (f.ID >= 200 && f.ID <= 299)
                {
                    int idx = f.ID - 200;
                    if (idx > maxModelIndex)
                        maxModelIndex = idx;
                }
            }

            return maxModelIndex + 1;
        }

        public Model[] LoadCharacter(string anibndID, string chrbndID,
            SoulsAssetPipeline.FLVERImporting.FLVER2Importer.ImportedFLVER2Model modelToImportDuringLoad = null,
            List<Action> doImportActionList = null, SapImportConfigs.ImportConfigFlver2 importConfig = null)
        {
            if (chrbndID == null)
                chrbndID = anibndID;

            Model[] chr = { null, null, null, null };
            int chrModelCount = 1;

            ParentDocument.Scene.DisableModelDrawing();
            ParentDocument.Scene.DisableModelDrawing2();

            string overrideAnibndID = null;

            ParentDocument.LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_CHR_{anibndID}", $"Loading character {anibndID}...", progress =>
            {
                var defaultModelGameDataPath = $@"/chr/{chrbndID}.chrbnd.dcx";
                var chrBndPath = defaultModelGameDataPath;
                var modelBrowseSearchDefaultStart = $@"/chr/{chrbndID.Substring(0, 4)}";
                if (GameType == SoulsGames.DS2SOTFS)
                {
                    defaultModelGameDataPath = $@"/model/chr/{chrbndID}.bnd";
                    modelBrowseSearchDefaultStart = $@"/model/chr/{chrbndID.Substring(0, 4)}";
                }
                else if (GameType == SoulsGames.DES)
                {
                    defaultModelGameDataPath = $@"/chr/{chrbndID}/{chrbndID}.chrbnd.dcx";
                    modelBrowseSearchDefaultStart = $@"/chr/{chrbndID}/{chrbndID.Substring(0, 4)}";
                }

                bool checkCharacterExistsOrGiveUp(string checkChrbndPath)
                {
                    IBinder checkChrbnd = ParentDocument.GameData.ReadBinder(checkChrbndPath);
                    if (checkChrbnd == null)
                    {
                        var res = System.Windows.Forms.MessageBox.Show($"Character model '{checkChrbndPath}' does not exist. Would you like to select a different one?",
                            "Error", System.Windows.Forms.MessageBoxButtons.YesNo);
                        if (res == System.Windows.Forms.DialogResult.Yes)
                        {
                            var newChrbndName = ParentDocument.GameData.ShowPickInsideBndPath(" /chr/", @".*\/c\d\d\d\d.chrbnd.dcx$", modelBrowseSearchDefaultStart, $"Choose Character Model for '{anibndID}.anibnd.dcx'", defaultModelGameDataPath);
                            if (newChrbndName == null)
                            {
                                Main.REQUEST_REINIT_EDITOR = true;
                                System.Windows.Forms.MessageBox.Show($"No character models found. Make sure your game directory is set correctly.",
                            "Error", System.Windows.Forms.MessageBoxButtons.OK);
                                return false;
                            }
                            else
                                return checkCharacterExistsOrGiveUp(newChrbndName);
                        }
                        else
                        {
                            return false;
                        }
                    }

                    return checkChrbnd != null;
                }

                if (!checkCharacterExistsOrGiveUp(defaultModelGameDataPath))
                {
                    Main.REQUEST_REINIT_EDITOR = true;
                    return;
                }

                var modelBytes = ParentDocument.GameData.ReadFile(defaultModelGameDataPath);
                IBinder chrbnd = null;
                if (BND3.Is(modelBytes))
                    chrbnd = BND3.Read(modelBytes);
                else if (BND4.Is(modelBytes))
                    chrbnd = BND4.Read(modelBytes);

                if (anibndID != chrbndID)
                {
                    bool foundTaeInSelectedAnibnd = false;
                    var anibndCheckName = Utils.GetFileNameWithoutAnyExtensions(chrBndPath) + ".anibnd.dcx";
                    var anibndCheckBytes = ParentDocument.GameData.ReadFile(anibndCheckName);
                    if (anibndCheckBytes != null)
                    {
                        IBinder anibndCheck = null;
                        if (BND3.Is(anibndCheckBytes))
                            anibndCheck = BND3.Read(anibndCheckBytes);
                        else if (BND4.Is(anibndCheckBytes))
                            anibndCheck = BND4.Read(anibndCheckBytes);
                        foreach (var f in anibndCheck.Files)
                        {
                            if (TAE.Is(f.Bytes))
                            {
                                foundTaeInSelectedAnibnd = true;
                                break;
                            }
                        }
                    }

                    if (foundTaeInSelectedAnibnd)
                    {
                        ImguiOSD.DialogManager.DialogOK("Warning", $"Warning: TimeAct data (.TAE) was found in the ANIBND matching the ID of the model you selected to load.\n" +
                            $"You should probably just load that ANIBND ('{anibndCheckName}') instead.");
                    }
                }

                if (GameType is SoulsGames.DS3 or SoulsGames.SDT or SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6)
                {

                    //var chrbnd = BND4.Read();
                    IBinder texbnd = null;
                    IBinder extraTexbnd = null;
                    IBinder anibnd = null;

                    if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID}.texbnd.dcx"))
                        texbnd = ReadBinder(ParentDocument.GameData.ReadFile($@"/chr/{chrbndID}.texbnd.dcx"));
                    else if (GameType is SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6 && ParentDocument.GameData.FileExists($@"/chr/{chrbndID}_h.texbnd.dcx"))
                        texbnd = ReadBinder(ParentDocument.GameData.ReadFile($@"/chr/{chrbndID}_h.texbnd.dcx"));

                    if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID.Substring(0, 4)}9.texbnd.dcx"))
                        extraTexbnd = ReadBinder(ParentDocument.GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.texbnd.dcx"));



                    if (texbnd != null && modelToImportDuringLoad != null)
                    {
                        texbnd.Files.Clear();
                        foreach (var t in modelToImportDuringLoad.Textures)
                        {
                            var tpf = new TPF();
                            tpf.Textures.Add(t);
                            texbnd.Files.Add(new BinderFile(Binder.FileFlags.Flag1, texbnd.Files.Count, tpf.Write()));
                        }
                    }



                    if (ParentDocument.GameData.FileExists($@"/chr/{anibndID}.anibnd.dcx"))
                        anibnd = ReadBinder(ParentDocument.GameData.ReadFile($@"/chr/{anibndID}.anibnd.dcx"));




                    if ((anibnd == null || !anibnd.Files.Any(f => f.Name.ToLower().EndsWith(".tae"))) && ParentDocument.GameData.FileExists($@"/chr/{anibndID.Substring(0, 4)}0.anibnd.dcx"))
                    {
                        overrideAnibndID = $@"{anibndID.Substring(0, 4)}0";
                        anibnd = ReadBinder(ParentDocument.GameData.ReadFile($@"/chr/{overrideAnibndID}.anibnd.dcx"));
                    }

                    chrModelCount = GetChrModelCount(chrbnd);
                    Array.Resize(ref chr, chrModelCount);
                    for (int i = 0; i < chrModelCount; i++)
                    {
                        chr[i] = new Model(ParentDocument, progress, chrbndID, chrbnd, i, anibnd, texbnd,
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexbnd,
                            modelToImportDuringLoad: modelToImportDuringLoad,
                            modelImportConfig: importConfig);
                    }

                    if (doImportActionList != null)
                    {
                        if (texbnd != null)
                            doImportActionList.Add(() =>
                            {
                                (texbnd as BND4).Write(GetInterrootPathOld($@"/chr/{chrbndID}.texbnd.dcx"));
                            });

                        if (chrbnd != null)
                            doImportActionList.Add(() =>
                            {
                                (chrbnd as BND4).Write(GetInterrootPathOld($@"/chr/{chrbndID}.chrbnd.dcx"));
                            });
                    }
                }
                else if (GameType == SoulsGames.DS1)
                {
                    //var chrbnd = BND3.Read(GameData.ReadFile($@"/chr/{chrbndID}.chrbnd.dcx"));
                    IBinder anibnd = null;
                    IBinder texbxf = null;
                    IBinder extraTexChrbnd = null;
                    IBinder extraTexbxf = null;

                    if ((ParentDocument.GameData.FileExists($@"/chr/{anibndID}.anibnd.dcx")))
                        anibnd = BND3.Read(ParentDocument.GameData.ReadFile($@"/chr/{anibndID}.anibnd.dcx"));

                    if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"))
                        extraTexChrbnd = BND3.Read(ParentDocument.GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"));

                    BinderFile chrtpfbhd = null;

                    if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID}.chrtpfbdt"))
                    {
                        chrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                        if (chrtpfbhd != null)
                        {
                            texbxf = BXF3.Read(chrtpfbhd.Bytes, ParentDocument.GameData.ReadFile($@"/chr/{chrbndID}.chrtpfbdt"));

                            if (modelToImportDuringLoad != null)
                            {
                                texbxf.Files.Clear();
                                foreach (var t in modelToImportDuringLoad.Textures)
                                {
                                    var tpf = new TPF();
                                    tpf.Textures.Add(t);
                                    texbxf.Files.Add(new BinderFile(Binder.FileFlags.Flag1, texbxf.Files.Count, tpf.Write()));
                                }
                            }
                        }
                    }

                    BinderFile extraChrtpfbhd = null;

                    if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID.Substring(0, 4)}9.chrtpfbdt"))
                    {
                        extraChrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                        if (extraChrtpfbhd != null)
                        {
                            extraTexbxf = BXF3.Read(chrtpfbhd.Bytes, ParentDocument.GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.chrtpfbdt"));

                            if (modelToImportDuringLoad != null)
                            {
                                //We don't wanna mess with other vanilla files that are sharing with this cXXX9
                                //texbnd.Files.Clear();
                                foreach (var t in modelToImportDuringLoad.Textures)
                                {
                                    var tpf = new TPF();
                                    tpf.Textures.Add(t);

                                    string innerTpfName = t.Name + ".tpf";

                                    var matchingInnerTpf = extraTexChrbnd.Files.FirstOrDefault(ff => ff.Name.ToLower() == innerTpfName.ToLower());

                                    if (matchingInnerTpf != null)
                                    {
                                        // Update existing TPF in chrtpfbdt
                                        matchingInnerTpf.Bytes = tpf.Write();
                                    }
                                    else
                                    {
                                        // Add new TPF to chrtpfbdt
                                        texbxf.Files.Add(new BinderFile(Binder.FileFlags.Flag1, texbxf.Files.Count, innerTpfName, tpf.Write()));
                                    }


                                }
                            }
                        }
                    }


                    var tpfsUsedForPtdeMemoryStuff = new List<TPF>();

                    chrModelCount = GetChrModelCount(chrbnd);
                    Array.Resize(ref chr, chrModelCount);
                    for (int i = 0; i < chrModelCount; i++)
                    {
                        chr[i] = new Model(ParentDocument, progress, chrbndID, chrbnd, i, anibnd, texbxf,
                        possibleLooseTpfFolder: $@"/chr/{chrbndID}/",
                        ignoreStaticTransforms: true, additionalTexbnd: extraTexChrbnd,
                        modelToImportDuringLoad: modelToImportDuringLoad,
                        modelImportConfig: importConfig, tpfsUsed: tpfsUsedForPtdeMemoryStuff);
                    }




                    if (doImportActionList != null)
                    {
                        doImportActionList.Add(() =>
                        {
                            Dictionary<string, TPF.Texture> allUsedTexturesForPtdeMemory = new Dictionary<string, TPF.Texture>();

                            foreach (var tpfused in tpfsUsedForPtdeMemoryStuff)
                            {
                                foreach (var t in tpfused.Textures)
                                {
                                    var targetTpfName = Utils.GetShortIngameFileName(t.Name).ToLower();
                                    if (!allUsedTexturesForPtdeMemory.ContainsKey(targetTpfName))
                                    {
                                        allUsedTexturesForPtdeMemory.Add(targetTpfName, t);
                                    }
                                    else
                                    {
                                        allUsedTexturesForPtdeMemory[targetTpfName] = t;
                                    }
                                }
                            }

                            byte[] existingExtraTexChrBytes = ParentDocument.GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx");

                            string ptdeMemoryLooseTexFolderName = GetInterrootPathOld($@"/chr/{chrbndID}", isDirectory: true, doNotCheckIfExists: true);

                            if (existingExtraTexChrBytes != null)
                            {
                                var existingExtraTexChrTpfs = BND3.Read(existingExtraTexChrBytes).Files
                                .Where(bf => bf.Name.ToLower().EndsWith(".tpf"))
                                .Select(bff => TPF.Read(bff.Bytes))
                                .ToList();
                                foreach (var ect in existingExtraTexChrTpfs)
                                {
                                    foreach (var t in ect.Textures)
                                    {
                                        var targetTpfName = Utils.GetShortIngameFileName(t.Name).ToLower();
                                        if (!allUsedTexturesForPtdeMemory.ContainsKey(targetTpfName))
                                        {
                                            allUsedTexturesForPtdeMemory.Add(targetTpfName, t);
                                        }
                                        else
                                        {
                                            allUsedTexturesForPtdeMemory[targetTpfName] = t;
                                        }
                                    }
                                }

                                ptdeMemoryLooseTexFolderName = GetInterrootPathOld($@"/chr/{chrbndID.Substring(0, 4)}9", isDirectory: true, doNotCheckIfExists: true);
                            }

                            if (!Directory.Exists(ptdeMemoryLooseTexFolderName))
                            {
                                Directory.CreateDirectory(ptdeMemoryLooseTexFolderName);
                            }

                            var delegateCaptureEnsure_allUsedTexturesForPtdeMemory = allUsedTexturesForPtdeMemory;


                            foreach (var tx in delegateCaptureEnsure_allUsedTexturesForPtdeMemory)
                            {
                                string fullTpfPath = $@"{ptdeMemoryLooseTexFolderName}/{Utils.GetShortIngameFileName(tx.Value.Name)}.tpf";
                                var newTpf = new TPF()
                                {
                                    Platform = TPF.TPFPlatform.PC,
                                    Encoding = 0x2,
                                    Flag2 = 0x3,
                                };
                                newTpf.Textures.Add(tx.Value);
                                if (File.Exists(fullTpfPath) && !File.Exists(fullTpfPath + ".bak"))
                                    File.Copy(fullTpfPath, fullTpfPath + ".bak");
                                newTpf.Write(fullTpfPath);
                            }

                        });





                        //if (texbxf != null)
                        //{
                        //    doImportActionList.Add(() =>
                        //    {
                        //        (texbxf as BXF3).Write(out byte[] customBhd, out byte[] customBdt);
                        //        chrtpfbhd.Bytes = customBhd;
                        //        File.WriteAllBytes(GameData.ReadFile($@"/chr/{id}.chrtpfbdt"), customBdt);
                        //        (chrbnd as BND3).Write(GameData.ReadFile($@"/chr/{id}.chrbnd"));
                        //    });
                        //}

                        //if (extraTexbxf != null)
                        //{
                        //    doImportActionList.Add(() =>
                        //    {
                        //        (texbxf as BXF3).Write(out byte[] customBhd, out byte[] customBdt);
                        //        extraChrtpfbhd.Bytes = customBhd;
                        //        File.WriteAllBytes(GameData.ReadFile($@"/chr/{id.Substring(0, 4)}9.chrtpfbdt"), customBdt);
                        //        (extraTexChrbnd as BND3).Write(GameData.ReadFile($@"/chr/{id.Substring(0, 4)}9.chrbnd"));
                        //    });
                        //}

                        if (chrbnd != null)
                        {
                            doImportActionList.Add(() =>
                            {
                                // PTDE MEMORY ISSUES; USE BXF TEXTURES (LOOSE FOLDER WITH UDSFM)
                                (chrbnd as BND3).Files.RemoveAll(f => f.Name.ToLower().EndsWith(".tpf"));

                                (chrbnd as BND3).Write(GetInterrootPathOld($@"/chr/{anibndID}.chrbnd"));
                            });
                        }

                        if (extraTexChrbnd != null)
                        {
                            doImportActionList.Add(() =>
                            {
                                // PTDE MEMORY ISSUES; USE BXF TEXTURES (LOOSE FOLDER WITH UDSFM)
                                (extraTexChrbnd as BND3).Files.RemoveAll(f => f.Name.ToLower().EndsWith(".tpf"));

                                (extraTexChrbnd as BND3).Write(GetInterrootPathOld($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd"));
                            });
                        }
                    }
                }
                else if (GameType == SoulsGames.DS1R)
                {
                    //var chrbnd = BND3.Read(GameData.ReadFile($@"/chr/{chrbndID}.chrbnd.dcx"));
                    IBinder anibnd = null;
                    IBinder texbxf = null;
                    IBinder extraTexChrbnd = null;
                    IBinder extraTexbxf = null;

                    if (ParentDocument.GameData.FileExists($@"/chr/{anibndID}.anibnd.dcx"))
                        anibnd = BND3.Read(ParentDocument.GameData.ReadFile($@"/chr/{anibndID}.anibnd.dcx"));

                    if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"))
                        extraTexChrbnd = BND3.Read(ParentDocument.GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"));

                    BinderFile chrtpfbhd = null;

                    if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID}.chrtpfbdt"))
                    {
                        chrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                        if (chrtpfbhd != null)
                        {
                            texbxf = BXF3.Read(chrtpfbhd.Bytes, ParentDocument.GameData.ReadFile($@"/chr/{chrbndID}.chrtpfbdt"));

                            if (modelToImportDuringLoad != null)
                            {
                                texbxf.Files.Clear();
                                foreach (var t in modelToImportDuringLoad.Textures)
                                {
                                    var tpf = new TPF();
                                    tpf.Textures.Add(t);
                                    texbxf.Files.Add(new BinderFile(Binder.FileFlags.Flag1, texbxf.Files.Count, tpf.Write()));
                                }
                            }
                        }
                    }

                    BinderFile extraChrtpfbhd = null;

                    if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID.Substring(0, 4)}9.chrtpfbdt"))
                    {
                        extraChrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                        if (extraChrtpfbhd != null)
                        {
                            try
                            {
                                extraTexbxf = BXF3.Read(chrtpfbhd.Bytes, ParentDocument.GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.chrtpfbdt"));
                            }
                            catch
                            {
                                extraTexbxf = null;
                            }

                            if (modelToImportDuringLoad != null && extraTexbxf != null)
                            {
                                //We don't wanna mess with other vanilla files that are sharing with this cXXX9
                                //texbnd.Files.Clear();
                                foreach (var t in modelToImportDuringLoad.Textures)
                                {
                                    var tpf = new TPF();
                                    tpf.Textures.Add(t);

                                    string innerTpfName = t.Name + ".tpf";

                                    var matchingInnerTpf = extraTexChrbnd.Files.FirstOrDefault(ff => ff.Name.ToLower() == innerTpfName.ToLower());

                                    if (matchingInnerTpf != null)
                                    {
                                        // Update existing TPF in chrtpfbdt
                                        matchingInnerTpf.Bytes = tpf.Write();
                                    }
                                    else
                                    {
                                        // Add new TPF to chrtpfbdt
                                        texbxf.Files.Add(new BinderFile(Binder.FileFlags.Flag1, texbxf.Files.Count, innerTpfName, tpf.Write()));
                                    }


                                }
                            }
                        }
                    }
                    var tpfsUsedForPtdeMemoryStuff = new List<TPF>();

                    chrModelCount = GetChrModelCount(chrbnd);
                    Array.Resize(ref chr, chrModelCount);
                    for (int i = 0; i < chrModelCount; i++)
                    {
                        chr[i] = new Model(ParentDocument, progress, chrbndID, chrbnd, i, anibnd, texbxf,
                        possibleLooseTpfFolder: $@"/chr/{chrbndID}/",
                        ignoreStaticTransforms: true, additionalTexbnd: extraTexChrbnd,
                        modelToImportDuringLoad: modelToImportDuringLoad,
                        modelImportConfig: importConfig, tpfsUsed: tpfsUsedForPtdeMemoryStuff);
                    }



                    if (doImportActionList != null)
                    {
                        doImportActionList.Add(() =>
                        {
                            Dictionary<string, TPF.Texture> allUsedTexturesForPtdeMemory = new Dictionary<string, TPF.Texture>();

                            foreach (var tpfused in tpfsUsedForPtdeMemoryStuff)
                            {
                                foreach (var t in tpfused.Textures)
                                {
                                    var targetTpfName = Utils.GetShortIngameFileName(t.Name).ToLower();
                                    if (!allUsedTexturesForPtdeMemory.ContainsKey(targetTpfName))
                                    {
                                        allUsedTexturesForPtdeMemory.Add(targetTpfName, t);
                                    }
                                    else
                                    {
                                        allUsedTexturesForPtdeMemory[targetTpfName] = t;
                                    }
                                }
                            }

                            byte[] existingExtraTexChrBytes = ParentDocument.GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx");

                            if (existingExtraTexChrBytes != null)
                            {
                                var existingExtraTexChrTpfs = BND3.Read(existingExtraTexChrBytes).Files
                                .Where(bf => bf.Name.ToLower().EndsWith(".tpf"))
                                .Select(bff => TPF.Read(bff.Bytes))
                                .ToList();
                                foreach (var ect in existingExtraTexChrTpfs)
                                {
                                    foreach (var t in ect.Textures)
                                    {
                                        var targetTpfName = Utils.GetShortIngameFileName(t.Name).ToLower();
                                        if (!allUsedTexturesForPtdeMemory.ContainsKey(targetTpfName))
                                        {
                                            allUsedTexturesForPtdeMemory.Add(targetTpfName, t);
                                        }
                                        else
                                        {
                                            allUsedTexturesForPtdeMemory[targetTpfName] = t;
                                        }
                                    }
                                }
                            }

                            var delegateCaptureEnsure_allUsedTexturesForPtdeMemory = allUsedTexturesForPtdeMemory;

                            var fakeTexBhf = new BXF3();

                            foreach (var tx in delegateCaptureEnsure_allUsedTexturesForPtdeMemory)
                            {
                                string fullTpfPath = $@"{Utils.GetShortIngameFileName(tx.Value.Name)}.tpf";
                                var newTpf = new TPF()
                                {
                                    Platform = TPF.TPFPlatform.PC,
                                    Encoding = 0x2,
                                    Flag2 = 0x3,
                                };
                                newTpf.Textures.Add(tx.Value);

                                if (texbxf != null)
                                {
                                    var existingBinderFile = texbxf.Files.LastOrDefault(bff => bff.Name.ToLower() == fullTpfPath.ToLower());
                                    if (existingBinderFile != null)
                                        existingBinderFile.Bytes = newTpf.Write();
                                    else
                                        texbxf.Files.Add(new BinderFile(Binder.FileFlags.Flag1, texbxf.Files.Count, fullTpfPath, newTpf.Write()));
                                }

                                if (extraTexbxf != null)
                                {
                                    var existingBinderFile = extraTexbxf.Files.LastOrDefault(bff => bff.Name.ToLower() == fullTpfPath.ToLower());
                                    if (existingBinderFile != null)
                                        existingBinderFile.Bytes = newTpf.Write();
                                    else
                                        extraTexbxf.Files.Add(new BinderFile(Binder.FileFlags.Flag1, texbxf.Files.Count, fullTpfPath, newTpf.Write()));
                                }
                            }



                        });



                        if (texbxf != null)
                        {
                            doImportActionList.Add(() =>
                            {
                                (texbxf as BXF3).Write(out byte[] customBhd, out byte[] customBdt);
                                chrtpfbhd.Bytes = customBhd;
                                File.WriteAllBytes(GetInterrootPathOld($@"/chr/{chrbndID}.chrtpfbdt"), customBdt);
                                (chrbnd as BND3).Write(GetInterrootPathOld($@"/chr/{chrbndID}.chrbnd.dcx"));
                            });
                        }

                        if (extraTexbxf != null)
                        {
                            doImportActionList.Add(() =>
                            {
                                (texbxf as BXF3).Write(out byte[] customBhd, out byte[] customBdt);
                                extraChrtpfbhd.Bytes = customBhd;
                                File.WriteAllBytes(GetInterrootPathOld($@"/chr/{chrbndID.Substring(0, 4)}9.chrtpfbdt"), customBdt);
                                (extraTexChrbnd as BND3).Write(GetInterrootPathOld($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"));
                            });
                        }

                        if (chrbnd != null)
                            doImportActionList.Add(() =>
                            {
                                (chrbnd as BND3).Write(GetInterrootPathOld($@"/chr/{chrbndID}.chrbnd.dcx"));
                            });

                        if (extraTexChrbnd != null)
                            doImportActionList.Add(() =>
                            {
                                (extraTexChrbnd as BND3).Write(GetInterrootPathOld($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"));
                            });
                    }
                }
                else if (GameType == SoulsGames.BB)
                {
                    //IBinder chrbnd = BND4.Read(GameData.ReadFile($@"/chr/{chrbndID}.chrbnd.dcx"));
                    IBinder anibnd = null;
                    IBinder extraTexbnd = null;

                    if (ParentDocument.GameData.FileExists($@"/chr/{anibndID}.anibnd.dcx"))
                        anibnd = BND4.Read(ParentDocument.GameData.ReadFile($@"/chr/{anibndID}.anibnd.dcx"));

                    if (ParentDocument.GameData.FileExists($@"/chr/{anibndID.Substring(0, 4)}9.chrbnd.dcx"))
                        extraTexbnd = BND4.Read(ParentDocument.GameData.ReadFile($@"/chr/{anibndID.Substring(0, 4)}9.chrbnd.dcx"));

                    chrModelCount = GetChrModelCount(chrbnd);
                    Array.Resize(ref chr, chrModelCount);
                    for (int i = 0; i < chrModelCount; i++)
                    {
                        chr[i] = new Model(ParentDocument, progress, chrbndID, chrbnd, i, anibnd, texbnd: null,
                        additionalTpfNames: new List<string> { GetInterrootPathOld($@"/chr/{chrbndID}_2.tpf.dcx") },
                        possibleLooseTpfFolder: $@"/chr/{chrbndID}/",
                        ignoreStaticTransforms: true, additionalTexbnd: extraTexbnd,
                        modelToImportDuringLoad: modelToImportDuringLoad,
                        modelImportConfig: importConfig);
                    }



                    if (doImportActionList != null)
                    {
                        if (chrbnd != null)
                            doImportActionList.Add(() =>
                            {
                                (chrbnd as BND4).Write(GetInterrootPathOld($@"/chr/{chrbndID}.chrbnd.dcx"));
                            });
                    }
                }
                else if (GameType == SoulsGames.DS2SOTFS)
                {
                    //IBinder chrbnd = BND4.Read(GameData.ReadFile($@"/model/chr/{chrbndID}.bnd"));
                    IBinder texbnd = null;

                    if (ParentDocument.GameData.FileExists($@"/model/chr/{chrbndID}.texbnd"))
                        texbnd = BND4.Read(ParentDocument.GameData.ReadFile($@"/model/chr/{chrbndID}.texbnd"));

                    if (texbnd != null && modelToImportDuringLoad != null)
                    {
                        texbnd.Files.Clear();
                        foreach (var t in modelToImportDuringLoad.Textures)
                        {
                            var tpf = new TPF();
                            tpf.Textures.Add(t);
                            texbnd.Files.Add(new BinderFile(Binder.FileFlags.Flag1, texbnd.Files.Count, tpf.Write()));
                        }
                    }

                    chrModelCount = GetChrModelCount(chrbnd);
                    Array.Resize(ref chr, chrModelCount);
                    for (int i = 0; i < chrModelCount; i++)
                    {
                        chr[i] = new Model(ParentDocument, progress, chrbndID, chrbnd, i, anibnd: null, texbnd,
                        additionalTpfNames: null,
                        possibleLooseTpfFolder: null,
                        ignoreStaticTransforms: true, additionalTexbnd: null,
                        modelToImportDuringLoad: modelToImportDuringLoad,
                        modelImportConfig: importConfig);
                    }



                    if (doImportActionList != null)
                    {
                        if (texbnd != null)
                            doImportActionList.Add(() =>
                            {
                                (texbnd as BND4).Write(GetInterrootPathOld($@"/model/chr/{chrbndID}.bnd"));
                            });

                        if (chrbnd != null)
                            doImportActionList.Add(() =>
                            {
                                (chrbnd as BND4).Write(GetInterrootPathOld($@"/model/chr/{chrbndID}.texbnd"));
                            });
                    }
                }
                else if (GameType == SoulsGames.DES)
                {
                    //var chrbnd = BND3.Read(GameData.ReadFile($@"/chr/{chrbndID}/{chrbndID}.chrbnd.dcx"));
                    IBinder anibnd = null;
                    IBinder extraTexChrbnd = null;

                    //DESR TEST
                    if (ParentDocument.GameRoot.GameIsDemonsSoulsRemastered)
                    {
                        anibnd = BND3.Read(ParentDocument.GameData.ReadFile($@"/chr/{anibndID}/{anibndID}.anibnd"));
                    }
                    else
                    {
                        if (!ParentDocument.GameData.FileExists($@"/chr/{anibndID}/{anibndID}.anibnd.dcx.2010"))
                            anibnd = TaeEditor.TaeFileContainer.GenerateDemonsSoulsConvertedAnibnd($"{InterrootPath}\\chr\\{anibndID}\\{anibndID}.anibnd.dcx");
                        else
                            anibnd = BND3.Read(ParentDocument.GameData.ReadFile($@"/chr/{anibndID}/{anibndID}.anibnd.dcx.2010"));
                    }

                    if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID.Substring(0, 4)}9/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"))
                        extraTexChrbnd = BND3.Read(ParentDocument.GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"));

                    var tpfsUsedForPtdeMemoryStuff = new List<TPF>();

                    chrModelCount = GetChrModelCount(chrbnd);
                    Array.Resize(ref chr, chrModelCount);
                    for (int i = 0; i < chrModelCount; i++)
                    {
                        chr[i] = new Model(ParentDocument, progress, chrbndID, chrbnd, i, anibnd, null,
                        possibleLooseTpfFolder: $@"/chr/{chrbndID}/",
                        ignoreStaticTransforms: true, additionalTexbnd: extraTexChrbnd,
                        modelToImportDuringLoad: modelToImportDuringLoad,
                        modelImportConfig: importConfig, tpfsUsed: tpfsUsedForPtdeMemoryStuff);
                    }



                    if (doImportActionList != null)
                    {
                        doImportActionList.Add(() =>
                        {
                            Dictionary<string, TPF.Texture> allUsedTexturesForPtdeMemory = new Dictionary<string, TPF.Texture>();

                            foreach (var tpfused in tpfsUsedForPtdeMemoryStuff)
                            {
                                foreach (var t in tpfused.Textures)
                                {
                                    var targetTpfName = Utils.GetShortIngameFileName(t.Name).ToLower();
                                    if (!allUsedTexturesForPtdeMemory.ContainsKey(targetTpfName))
                                    {
                                        allUsedTexturesForPtdeMemory.Add(targetTpfName, t);
                                    }
                                    else
                                    {
                                        allUsedTexturesForPtdeMemory[targetTpfName] = t;
                                    }
                                }
                            }

                            byte[] existingExtraTexChrBytes = ParentDocument.GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9/{chrbndID.Substring(0, 4)}9.chrbnd.dcx");

                            if (existingExtraTexChrBytes != null)
                            {
                                var existingExtraTexChrTpfs = BND3.Read(existingExtraTexChrBytes).Files
                                .Where(bf => bf.Name.ToLower().EndsWith(".tpf"))
                                .Select(bff => TPF.Read(bff.Bytes))
                                .ToList();
                                foreach (var ect in existingExtraTexChrTpfs)
                                {
                                    foreach (var t in ect.Textures)
                                    {
                                        var targetTpfName = Utils.GetShortIngameFileName(t.Name).ToLower();
                                        if (!allUsedTexturesForPtdeMemory.ContainsKey(targetTpfName))
                                        {
                                            allUsedTexturesForPtdeMemory.Add(targetTpfName, t);
                                        }
                                        else
                                        {
                                            allUsedTexturesForPtdeMemory[targetTpfName] = t;
                                        }
                                    }
                                }
                            }

                            var delegateCaptureEnsure_allUsedTexturesForPtdeMemory = allUsedTexturesForPtdeMemory;

                            var fakeTexBhf = new BXF3();

                            foreach (var tx in delegateCaptureEnsure_allUsedTexturesForPtdeMemory)
                            {
                                string fullTpfPath = $@"{Utils.GetShortIngameFileName(tx.Value.Name)}.tpf";
                                var newTpf = new TPF()
                                {
                                    Platform = TPF.TPFPlatform.PC,
                                    Encoding = 0x2,
                                    Flag2 = 0x3,
                                };
                                newTpf.Textures.Add(tx.Value);


                            }



                        });





                        if (chrbnd != null)
                            doImportActionList.Add(() =>
                            {
                                (chrbnd as BND3).Write(GetInterrootPathOld($@"/chr/{chrbndID}/{chrbndID}.chrbnd.dcx"));
                            });

                        if (extraTexChrbnd != null)
                            doImportActionList.Add(() =>
                            {
                                (extraTexChrbnd as BND3).Write(GetInterrootPathOld($@"/chr/{chrbndID.Substring(0, 4)}9/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"));
                            });
                    }
                }
                else
                {
                    throw new NotImplementedException($"Not implemented for GameType {GameType}.");
                }

                for (int i = 0; i < chr.Length; i++)
                {
                    if (i > 0)
                        chr[i].Name += $"_{i}";
                    ParentDocument.Scene.AddModel(chr[i]);
                }
                //Scene.SetMainModelToFirst(hideNonMainModels: true);


            });

            if (chr == null)
            {
                return Model.GetDummyModel(ParentDocument);
            }

            if (IsChrPlayerChr(anibndID))
            {
                ParentDocument.LoadingTaskMan.DoLoadingTaskSynchronous("c0000_ANIBNDs",
                    "Loading additional player ANIBNDs...", progress =>
                    {
                        
                        var additionalPlayerAnibnds = GetChrAdditionalAnibndNames(anibndID);

                        for (int i = 0; i < additionalPlayerAnibnds.Count; i++)
                        {
                            IBinder addAnibnd = null;

                            string anibndName = additionalPlayerAnibnds[i];

                            if (GameType == SoulsGames.DES && !GameIsDemonsSoulsRemastered)
                            {
                                // Since DeS is unpacked we can just use legacy loose file only shit here
                                addAnibnd = TaeEditor.TaeFileContainer.GenerateDemonsSoulsConvertedAnibnd($"{InterrootPath}\\{additionalPlayerAnibnds[i].Replace("/", "\\")}");
                            }
                            else
                            {
                                addAnibnd = ParentDocument.GameData.ReadBinder(anibndName);
                            }
                            if (addAnibnd != null)
                            {
                                for (int j = 0; j < chr.Length; j++)
                                {
                                    chr[j].AnimContainer.LoadAdditionalANIBND(addAnibnd, null, scanAnims: false);
                                }
                            }
                            else
                            {
                                zzz_NotificationManagerIns.PushNotification($"Warning: Referenced ANIBND file '{anibndName}' not found.");
                            }





                            progress.Report(1.0 * i / additionalPlayerAnibnds.Count);
                        }

                        progress.Report(1);

                        ParentDocument.Scene.EnableModelDrawing2();
                    });
            }
            else
            {
                var additionalAnibnds = GetChrAdditionalAnibndNames(overrideAnibndID ?? anibndID);

                //if (GameType == SoulsGames.DS1)
                //    {
                //        var additionalAnibnds_PTDE = GameData.SearchFiles($@"/chr", $@".*{(overrideAnibndID ?? anibndID)}_.*\.anibnd$");
                //        if (additionalAnibnds_PTDE.Count > 0)
                //            additionalAnibnds.AddRange(additionalAnibnds_PTDE);
                //    }

                if (additionalAnibnds.Count > 0)
                {
                    ParentDocument.LoadingTaskMan.DoLoadingTaskSynchronous($"{(overrideAnibndID ?? anibndID)}_AdditionalANIBNDs",
                    "Loading additional character ANIBNDs...", progress =>
                    {
                        for (int i = 0; i < additionalAnibnds.Count; i++)
                        {
                            IBinder anibnd = null;

                            string anibndName = additionalAnibnds[i];

                            if (GameType == SoulsGames.DES && !GameIsDemonsSoulsRemastered)
                            {
                                // Since DeS is unpacked we can just use legacy loose file only shit here
                                anibnd = TaeEditor.TaeFileContainer.GenerateDemonsSoulsConvertedAnibnd($"{InterrootPath}\\{additionalAnibnds[i].Replace("/", "\\")}");
                            }
                            else
                            {
                                var anibndBytes = ParentDocument.GameData.ReadFile(anibndName);
                                if (anibndBytes == null)
                                {
                                    progress.Report(1.0 * i / additionalAnibnds.Count);
                                    continue;
                                }
                                if (BND3.Is(anibndBytes))
                                    anibnd = BND3.Read(anibndBytes);
                                else
                                    anibnd = BND4.Read(anibndBytes);
                            }

                            for (int j = 0; j < chr.Length; j++)
                            {
                                if (chr[j] != null)
                                    chr[j].AnimContainer.LoadAdditionalANIBND(anibnd, null, scanAnims: false);
                            }



                            progress.Report(1.0 * i / additionalAnibnds.Count);
                        }


                    });
                }
            }



            ParentDocument.Scene.EnableModelDrawing();
            ParentDocument.Scene.EnableModelDrawing2();

            FlverMaterialDefInfo.FlushBinderCache();

            return chr;

        }
    }
}
