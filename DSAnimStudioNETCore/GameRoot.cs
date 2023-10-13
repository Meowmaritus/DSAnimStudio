using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline;

namespace DSAnimStudio
{
    public static class GameRoot
    {
        private static object _lock_AdditionalPlayerAnibndNames = new object();
        private static List<string> GetAdditionalPlayerAnibndNames()
        {
            List<string> result = null;
            lock (_lock_AdditionalPlayerAnibndNames)
            {
                var fileName = $@"{Main.Directory}\Res\PlayerAnibnds.{GameType}.txt";
                if (File.Exists(fileName))
                    result = File.ReadAllLines(fileName).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            }
            return result;
        }

        public struct SplitAnimID
        {
            public int TaeID;
            public int SubID;
            public static SplitAnimID FromFullID(int id, SoulsGames? forceGame = null)
            {
                var split = new SplitAnimID();
                var type = GetAnimIDFormattingType(forceGame ?? GameType);
                if (type == AnimIDFormattingType.aXX_YYYY)
                {
                    split.TaeID = id / 1_0000;
                    split.SubID = id % 1_0000;
                }
                else // Includes aXX_YY_ZZZZ as that is still a split of 6 digits between XX and YYZZZZ
                {
                    split.TaeID = id / 1_000000;
                    split.SubID = id % 1_000000;
                }
                return split;
            }

            public int GetFullID(SoulsGames? forceGame = null)
            {
                var type = GetAnimIDFormattingType(forceGame ?? GameType);
                switch (type)
                {
                    case AnimIDFormattingType.aXX_YYYY: 
                        return (TaeID * 1_0000) + (SubID % 1_0000);
                    case AnimIDFormattingType.aXXX_YYYYYY:
                    case AnimIDFormattingType.aXX_YY_ZZZZ: 
                        return (TaeID * 1_000000) + (SubID % 1_000000);
                    default: throw new NotImplementedException();
                }
            }

            public string GetFormattedIDString(SoulsGames? forceGame = null)
            {
                var type = GetAnimIDFormattingType(forceGame ?? GameType);
                switch (type)
                {
                    case AnimIDFormattingType.aXX_YYYY: return $"a{TaeID:D2}_{SubID:D4}";
                    case AnimIDFormattingType.aXXX_YYYYYY: return $"a{TaeID:D3}_{SubID:D6}";
                    case AnimIDFormattingType.aXX_YY_ZZZZ:
                        var ds2Meme = $"a{TaeID:D3}_{SubID:D6}";
                        ds2Meme.Insert(ds2Meme.Length - 4, "_");
                        return ds2Meme;
                    default: throw new NotImplementedException();
                }
            }
        }

        public static bool IsBigEndianGame => GameType == SoulsAssetPipeline.SoulsGames.DES && !GameIsDemonsSoulsRemastered;
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

        //public static bool GameTypeLoads2010Files => (GameType == SoulsGames.DES);

        public enum AnimIDFormattingType
        {
            aXX_YYYY,
            aXXX_YYYYYY,
            aXX_YY_ZZZZ,
            Unknown,
        }

        public static string GameTypeName => GameTypeNames.ContainsKey(GameType) ? GameTypeNames[GameType] : GameType.ToString();

        public static readonly Dictionary<SoulsGames, string> GameTypeNames =
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

        public static bool CheckGameTypeParamIDCompatibility(SoulsGames a, SoulsGames b)
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

        private static SoulsGames lastGameType = SoulsGames.None;
        public static SoulsGames GameType { get; set; } = SoulsGames.None;

        public static bool GameTypeUsesWwise => GameType == SoulsGames.ER;

        public static bool GameTypeUsesRegulation => GameType == SoulsAssetPipeline.SoulsGames.DS3 || 
            GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS || GameType == SoulsAssetPipeline.SoulsGames.ER;

        public static void ClearInterroot()
        {
            GameType = SoulsGames.None;
            InterrootPath = null;
            InterrootModenginePath = null;
        }

        public static bool IsGame(SoulsGames gameTypes)
        {
            return (GameType & gameTypes) != 0;
        }

        public static IBinder ReadBinder(string path)
        {
            if (BND3.Is(path))
                return BND3.Read(path);
            else if (BND4.Is(path))
                return BND4.Read(path);
            else
                throw new InvalidDataException($"Tried to read file '{path}' as a BND3 or BND4 but it was neither.");
        }

        public static IBinder ReadBinder(byte[] data)
        {
            if (BND3.Is(data))
                return BND3.Read(data);
            else if (BND4.Is(data))
                return BND4.Read(data);
            else
                throw new InvalidDataException($"Tried to read block of data as a BND3 or BND4 but it was neither.");
        }

        public static bool GameTypeHasLongAnimIDs => CurrentAnimIDFormatType == AnimIDFormattingType.aXXX_YYYYYY ||
            CurrentAnimIDFormatType == AnimIDFormattingType.aXX_YY_ZZZZ;

        public static bool GameTypeUsesMetallicShaders => GameType == SoulsGames.SDT || GameType == SoulsGames.ER;

        public static bool GameTypeUsesLegacyEmptyEventGroups => IsGame(SoulsGames.DES | SoulsGames.DS1 | SoulsGames.DS1R);

        public static bool GameTypeUsesBoilerplateEventGroups => 
            IsGame(SoulsGames.DS3 | SoulsGames.SDT | SoulsGames.BB | SoulsGames.ER) || 
            (GameTypeUsesLegacyEmptyEventGroups && Main.Config.SaveAdditionalEventRowInfoToLegacyGames);

        public static bool GameTypeUsesWepAbsorpPosParam => GameType == SoulsAssetPipeline.SoulsGames.DS3 || GameType == SoulsAssetPipeline.SoulsGames.SDT || GameType == SoulsAssetPipeline.SoulsGames.ER;

        public static AnimIDFormattingType GetAnimIDFormattingType(SoulsGames game)
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
                    return AnimIDFormattingType.aXXX_YYYYYY;
                case SoulsGames.DS2SOTFS:
                    return AnimIDFormattingType.aXX_YY_ZZZZ;
                default:
                    return AnimIDFormattingType.Unknown;
            }
        }

        public static AnimIDFormattingType CurrentAnimIDFormatType
        {
            get
            {
                return GetAnimIDFormattingType(GameType);
            }
        }

        //DESR TODO
        public static bool GameIsDemonsSoulsRemastered = false;

        public static bool GameTypeIsHavokTagfile =>
            (GameType == SoulsGames.DS1R || GameType == SoulsGames.SDT || GameType == SoulsGames.ER) || GameIsDemonsSoulsRemastered;

        public static HKX.HKXVariation GetCurrentLegacyHKXType()
        {
            if (GameType == SoulsGames.DES && !GameIsDemonsSoulsRemastered)
                return HKX.HKXVariation.HKXDeS;
            else if (GameType == SoulsGames.DS1 || GameType == SoulsGames.DS1R)
                return HKX.HKXVariation.HKXDS1;
            else if (GameType == SoulsGames.DS3)
                return HKX.HKXVariation.HKXDS3;
            else if (GameType == SoulsGames.BB)
                return HKX.HKXVariation.HKXBloodBorne;
            else if (GameType == SoulsGames.SDT || GameType == SoulsGames.ER || GameIsDemonsSoulsRemastered)
                return HKX.HKXVariation.HKXDS1;

            return HKX.HKXVariation.Invalid;
        }

        public static string ScratchPath { get; set; } = null;
        public static string CachePath { get; set; } = null;
        public static string ProjectPath { get; set; } = null;
        public static string DefaultGameDir { get; set; } = null;
        public static string InterrootPath { get; set; } = null;
        public static string InterrootModenginePath { get; set; } = null;
        public static bool ModengineLoadLooseParams { get; set; } = true;

        public static bool SuppressNextInterrootBrowsePrompt = false;

        public static string GetInterrootPathOld(string path, bool isDirectory = false, bool doNotCheckIfExists = false)
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

        public static List<string> GetInterrootFiles(string path, string match)
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

        public static void LoadMTDBND()
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
            else if (GameType == SoulsGames.ER)
            {
                FlverMaterialDefInfo.LoadMaterialBinders($@"/material/allmaterial.matbinbnd.dcx", null);
            }
        }

        public static bool InitializeFromBND(string bndPath)
        {
            IBinder bnd = null;
            if (BND3.Is(bndPath))
                bnd = BND3.Read(bndPath);
            else if (BND4.Is(bndPath))
                bnd = BND4.Read(bndPath);

            foreach (var f in bnd.Files)
            {
                var check = f.Name.ToUpper().Replace("\\", "/");
                string scratchFolder = Path.GetDirectoryName(bndPath);
                if (check.Contains("FRPG2"))
                {
                    GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.DS2SOTFS, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/FRPG/") && check.Contains(@"INTERROOT_X64"))
                {
                    GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.DS1R, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/FRPG/") && check.Contains(@"INTERROOT_WIN32"))
                {
                    GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.DS1, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/SPRJ/"))
                {
                    GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.BB, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/FDP/"))
                {
                    GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.DS3, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/DemonsSoul/"))
                {
                    GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.DES, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/NTC/"))
                {
                    GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.SDT, scratchFolder);
                    return true;
                }
                else if (check.Contains(@"/GR/"))
                {
                    GameRoot.Init(bndPath, SoulsAssetPipeline.SoulsGames.ER, scratchFolder);
                    return true;
                }
            }

            return false;
            
        }

        public static string GetInterrootFromFilePath(string filePath)
        {
            var folder = new System.IO.FileInfo(filePath).DirectoryName;

            var lastSlashInFolder = folder.LastIndexOf("\\");

            return folder.Substring(0, lastSlashInFolder);
        }

        public static string GetParentDirectory(string directoryPath)
        {
            var lastSlashInFolder = directoryPath.LastIndexOf("\\");

            return directoryPath.Substring(0, lastSlashInFolder);
        }

        public static void SoftInit(SoulsGames gameType)
        {
            GameType = gameType;
            lastGameType = SoulsGames.None;
        }

        public static void SetInterrootPickerPathsManually(string projectDir, string gameDir, string modengineDir, bool loadLooseParams)
        {
            var jsonData = new TaeEditor.TaeProjectJson()
            {
                GameDirectory = gameDir,
                ModEngineDirectory = modengineDir,
                LoadLooseParams = loadLooseParams,
            };
            jsonData.Save($@"{projectDir}\_DSAS_PROJECT.json");
        }

        public static void Init(string assetPath, SoulsGames gameType, string scratchFolder, bool forceReload = false, bool overrideInterrootPicker = false)
        {
            if (GameType == SoulsGames.None || string.IsNullOrWhiteSpace(InterrootPath))
                forceReload = true;
            GameType = gameType;
            

            if (gameType != lastGameType || forceReload)
            {
                ProjectPath = $"{Path.GetDirectoryName(assetPath)}/_DSAS_PROJECT.json";
                DefaultGameDir = Path.GetFullPath(Path.GetDirectoryName(assetPath) + (GameRoot.GameType is SoulsGames.DES ? "\\..\\..\\" : "\\..\\"));
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
                        directoryDialog.ShowDialog();
                    }

                    InterrootPath = directoryDialog.SelectedGameDir;
                    InterrootModenginePath = directoryDialog.SelectedModengineDir;
                    ScratchPath = scratchFolder + @"\_DSAS_TEMP";
                    CachePath = scratchFolder + @"\_DSAS_CACHE";
                    GameData.CfgLoadLooseParams = directoryDialog.LoadLooseParams;
                    GameData.CfgLoadUnpackedGameFiles = directoryDialog.LoadUnpackedGameFiles;
                }

                if (gameType != SoulsGames.None)
                {
                    GameData.InitEBLs();

                    //var test = GameEbl.ReadFileFromEBLs(@"/action/eventnameid.txt");
                    //var testMeme = new BinaryReaderEx(false, test);
                    //var testMemeStr = testMeme.GetASCII(0, test.Length);

                    //ParamManager.LoadParamBND(forceReload);
                    ParamManager.LoadParamBND(true);
                    //FmgManager.LoadAllFMG(forceReload);
                    FmgManager.LoadAllFMG(true);
                    LoadSystex();
                    LoadMTDBND();
                    FmodManager.Purge();
                    //FmodManager.UpdateInterrootForFile(null);
                    FmodManager.LoadFloorMaterialNamesFromInterroot();
                }

                
            }
            lastGameType = GameType;
        }

        public static void ReloadAllData()
        {
            ReloadParams();
            ReloadFmgs();
        }

        public static void ReloadParams()
        {
            ParamManager.LoadParamBND(forceReload: true);
        }

        public static void ReloadFmgs()
        {
            FmgManager.LoadAllFMG(forceReload: true);
        }

        //public enum MapModelSearchType
        //{
        //    CHR,
        //    OBJ,
        //    MAP
        //}
        //public static string GetMapThatModelIsIn(string modelName, MapModelSearchType searchType)
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


        public static void LoadSystex()
        {
            LoadingTaskMan.DoLoadingTask("LoadSystex", "Loading SYSTEX textures...", progress =>
            {
                if (GameType == SoulsGames.DS1 || GameType == SoulsGames.DES)
                {
                    TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        GameData.ReadFile($@"/other/SYSTEX_TEX.tpf"),
                        GameData.ReadFile($@"/other/envlight.tpf"),
                        GameData.ReadFile($@"/other/lensflare.tpf"),
                    }, progress);
                }
                else if (GameType == SoulsGames.DS1R)
                {
                    TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        GameData.ReadFile($@"/other/SYSTEX_TEX.tpf.dcx"),
                        GameData.ReadFile($@"/other/envlight.tpf.dcx"),
                        GameData.ReadFile($@"/other/lensflare.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == SoulsGames.DS3)
                {
                    TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        GameData.ReadFile($@"/other/systex.tpf.dcx"),
                        GameData.ReadFile($@"/other/bloodtex.tpf.dcx"),
                        GameData.ReadFile($@"/other/decaltex.tpf.dcx"),
                        GameData.ReadFile($@"/other/sysenvtex.tpf.dcx"),
                        GameData.ReadFile($@"/parts/common_body.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == SoulsGames.BB)
                {
                    // TODO: completely confirm these because I just
                    // copied them from a BB network test file list.
                    TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        GameData.ReadFile($@"/other/SYSTEX.tpf.dcx"),
                        GameData.ReadFile($@"/other/decalTex.tpf.dcx"),
                        GameData.ReadFile($@"/other/bloodTex.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == SoulsGames.SDT)
                {
                    TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        GameData.ReadFile($@"/other/systex.tpf.dcx"),
                        GameData.ReadFile($@"/other/maptex.tpf.dcx"),
                        GameData.ReadFile($@"/other/decaltex.tpf.dcx"),
                        GameData.ReadFile($@"/parts/common_body.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == SoulsGames.ER)
                {
                    TexturePool.AddTpfsFromFiles(new List<byte[]>
                    {
                        GameData.ReadFile($@"/other/systex.tpf.dcx"),
                        GameData.ReadFile($@"/other/sysenvtex.tpf.dcx"),
                        GameData.ReadFile($@"/other/decaltex.tpf.dcx"),
                        GameData.ReadFile($@"/parts/common_body.tpf.dcx"),
                    }, progress);
                }
                //else
                //{
                //    throw new NotImplementedException($"Not implemented for GameType {GameType}.");
                //}
            });

           
        }

        public static Model LoadObject(string id)
        {
            Scene.DisableModelDrawing();
            Scene.DisableModelDrawing2();

            Model obj = null;

            LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_OBJ_{id}", $"Loading object {id}...", progress =>
            {
                
                    if (GameType == SoulsGames.DS3 || GameType == SoulsGames.BB || GameType == SoulsGames.SDT || GameType == SoulsGames.ER)
                    {
                        var chrbnd = BND4.Read(GameData.ReadFile($@"obj/{id}.objbnd.dcx"));

                        obj = new Model(progress, id, chrbnd, 0, null, null);
                    }
                    else if (GameType == SoulsGames.DS1)
                    {
                        var chrbnd = BND3.Read(GameData.ReadFile($@"obj/{id}.objbnd"));

                        obj = new Model(progress, id, chrbnd, 0, null, null);
                    }
                    else if (GameType == SoulsGames.DS1R)
                    {
                        var chrbnd = BND3.Read(GameData.ReadFile($@"obj/{id}.objbnd.dcx"));

                        obj = new Model(progress, id, chrbnd, 0, null, null);
                    }
                    else
                    {
                        throw new NotImplementedException($"Not implemented for GameType {GameType}.");
                    }

                Scene.AddModel(obj);



                    if (obj.MainMesh != null)
                    {
                        var texturesToLoad = obj.MainMesh.GetAllTexNamesToLoad();

                        LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_OBJ_{id}_TEX",
                            "Loading additional object textures...", innerProgress =>
                            {
                                if (GameType == SoulsGames.DS1)
                                {
                                    foreach (var tex in texturesToLoad)
                                    {
                                        TexturePool.AddTpfFromFile(GameData.ReadFile($@"map/tx/{tex}.tpf"));
                                    }
                                }
                                else if (GameType == SoulsGames.DS3 || GameType == SoulsGames.SDT || GameType == SoulsGames.BB)
                                {
                                    int objGroup = int.Parse(id.Substring(1)) / 1_0000;
                                    string mapDirName = $@"/map/m{objGroup:D2}";
                                    var tpfBnds = GameData.SearchFiles(mapDirName, @".*\.tpfbhd");
                                    foreach (var t in tpfBnds)
                                        TexturePool.AddSpecificTexturesFromBinder(t, texturesToLoad, directEntryNameMatch: true);
                                }
                                obj.MainMesh.TextureReloadQueued = true;
                            });
                    }

                



            });

            Scene.EnableModelDrawing();
            Scene.EnableModelDrawing2();

            return obj;

        }

        public static List<string> GetModelFilesOfChr(string name)
        {
            List<string> result = new List<string>();

            string chrFolder = (GameType == SoulsGames.DS2 || GameType == SoulsGames.DS2SOTFS) ? @"/model/chr" : "/chr";
            var filesRelatedToThisChr = GameData.SearchFiles(chrFolder, $".*{name}.*");
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

        public static void CreateCharacterModelBackup(string name, bool isAsync = false)
        {
            LoadingTaskMan.DoLoadingTask($"GameDataManagerCreateCharacterModelBackup_{name}", $"Creating backup of character model '{name}'", prog =>
            {
                var chrModelFiles = GetModelFilesOfChr(name);
                foreach (var f in chrModelFiles)
                {
                    string bakPath = f + ModelImportBackupExtension;
                    if (!File.Exists(bakPath))
                        File.Copy(f, bakPath);
                }
            }, isUnimportant: true, waitForTaskToComplete: !isAsync);
        }

        public static void RestoreCharacterModelBackup(string name, bool isAsync = false)
        {
            LoadingTaskMan.DoLoadingTask($"GameDataManagerRestoreCharacterModelBackup_{name}", $"Restoring backup of character model '{name}'", prog =>
            {
                var chrModelFiles = GetModelFilesOfChr(name);
                foreach (var f in chrModelFiles)
                {
                    string bakPath = f + ModelImportBackupExtension;
                    if (File.Exists(bakPath))
                        File.Copy(bakPath, f, overwrite: true);
                }
            }, isUnimportant: true, waitForTaskToComplete: !isAsync);
            
        }

        public static Model LoadMapPiece(int area, int block, int x, int y, int pieceID)
        {

            Model chr = null;

            Scene.DisableModelDrawing();
            Scene.DisableModelDrawing2();


            if (GameType == SoulsGames.DS1 || GameType == SoulsGames.DS1R)
            {
                string fullMapID = $"m{area:D2}_{block:D2}_{x:D2}_{y:D2}";
                string mapPieceName = $"m{pieceID:D4}B{block}A{area}";
                byte[] mapPieceFileBytes = GameData.ReadFile($@"/map/{fullMapID}/{mapPieceName}.flver.dcx");

                LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_MAPPIECE_{fullMapID}_{mapPieceName}", $"Loading map piece {mapPieceName} from map {fullMapID}...", progress =>
                {
                    var flver = FLVER2.Read(mapPieceFileBytes);
                    chr = new Model(flver, false);

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

                    TexturePool.AddMapTextures(area, texturesUsed);
                });
            }
            else
            {
                throw new NotImplementedException("ONLY FOR DS1(R)");
            }

           

            Scene.EnableModelDrawing();
            Scene.EnableModelDrawing2();

            return chr;
        }



        public static Model LoadCharacter(string anibndID, string chrbndID,
            SoulsAssetPipeline.FLVERImporting.FLVER2Importer.ImportedFLVER2Model modelToImportDuringLoad = null,
            List<Action> doImportActionList = null, SapImportConfigs.ImportConfigFlver2 importConfig = null)
        {
            if (chrbndID == null)
                chrbndID = anibndID;

            Model chr = null;

            Scene.DisableModelDrawing();
            Scene.DisableModelDrawing2();

            string overrideAnibndID = null;

            LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_CHR_{anibndID}", $"Loading character {anibndID}...", progress =>
                {
                    var defaultModelGameDataPath = $@"/chr/{chrbndID}.chrbnd.dcx";
                    var chrBndPath = defaultModelGameDataPath;
                    var modelBrowseSearchDefaultStart = $@"/chr/{chrbndID.Substring(0,4)}";
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
                        var checkModelBytes = GameData.ReadFile(checkChrbndPath);
                        IBinder checkChrbnd = null;
                        if (checkModelBytes != null)
                        {
                            if (BND3.Is(checkModelBytes))
                                checkChrbnd = BND3.Read(checkModelBytes);
                            else if (BND4.Is(checkModelBytes))
                                checkChrbnd = BND4.Read(checkModelBytes);
                        }
                        else
                        {
                            var res = System.Windows.Forms.MessageBox.Show($"Character model '/chr/{chrbndID}.chrbnd.dcx' does not exist. Would you like to select a different one?",
                                "Error", System.Windows.Forms.MessageBoxButtons.YesNo);
                            if (res == System.Windows.Forms.DialogResult.Yes)
                            {
                                var newChrbndName = GameData.ShowPickInsideBndPath("/chr/", @".*\/c\d\d\d\d.chrbnd.dcx$", modelBrowseSearchDefaultStart, $"Choose Character Model for '{anibndID}.anibnd.dcx'", defaultModelGameDataPath);
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
                        //Main.REQUEST_REINIT_EDITOR = true;
                        return;
                    }

                    var modelBytes = GameData.ReadFile(defaultModelGameDataPath);
                    IBinder chrbnd = null;
                    if (BND3.Is(modelBytes))
                        chrbnd = BND3.Read(modelBytes);
                    else if (BND4.Is(modelBytes))
                        chrbnd = BND4.Read(modelBytes);

                    if (anibndID != chrbndID)
                    {
                        bool foundTaeInSelectedAnibnd = false;
                        var anibndCheckName = Utils.GetFileNameWithoutAnyExtensions(chrBndPath) + ".anibnd.dcx";
                        var anibndCheckBytes = GameData.ReadFile(anibndCheckName);
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

                    if (GameType == SoulsGames.DS3 || GameType == SoulsGames.SDT || GameType == SoulsGames.ER)
                    {
                        
                        //var chrbnd = BND4.Read();
                        IBinder texbnd = null;
                        IBinder extraTexbnd = null;
                        IBinder anibnd = null;

                        if (GameData.FileExists($@"/chr/{chrbndID}.texbnd.dcx"))
                            texbnd = ReadBinder(GameData.ReadFile($@"/chr/{chrbndID}.texbnd.dcx"));
                        else if (GameType == SoulsGames.ER && GameData.FileExists($@"/chr/{chrbndID}_h.texbnd.dcx"))
                            texbnd = ReadBinder(GameData.ReadFile($@"/chr/{chrbndID}_h.texbnd.dcx"));

                        if (GameData.FileExists($@"/chr/{chrbndID.Substring(0, 4)}9.texbnd.dcx"))
                            extraTexbnd = ReadBinder(GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.texbnd.dcx"));



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



                        if (GameData.FileExists($@"/chr/{anibndID}.anibnd.dcx"))
                            anibnd = ReadBinder(GameData.ReadFile($@"/chr/{anibndID}.anibnd.dcx"));
                        

                       

                        if ((anibnd == null || !anibnd.Files.Any(f => f.Name.ToLower().EndsWith(".tae"))) && GameData.FileExists($@"/chr/{anibndID.Substring(0, 4)}0.anibnd.dcx"))
                        {
                            overrideAnibndID = $@"{anibndID.Substring(0, 4)}0";
                            anibnd = ReadBinder(GameData.ReadFile($@"/chr/{overrideAnibndID}.anibnd.dcx"));
                        }

                        chr = new Model(progress, chrbndID, chrbnd, 0, anibnd, texbnd,
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexbnd, 
                            modelToImportDuringLoad: modelToImportDuringLoad, 
                            modelImportConfig: importConfig);

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

                        if ((GameData.FileExists($@"/chr/{anibndID}.anibnd.dcx")))
                            anibnd = BND3.Read(GameData.ReadFile($@"/chr/{anibndID}.anibnd.dcx"));

                        if (GameData.FileExists($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"))
                            extraTexChrbnd = BND3.Read(GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"));

                        BinderFile chrtpfbhd = null;

                        if (GameData.FileExists($@"/chr/{chrbndID}.chrtpfbdt"))
                        {
                            chrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                            if (chrtpfbhd != null)
                            {
                                texbxf = BXF3.Read(chrtpfbhd.Bytes, GameData.ReadFile($@"/chr/{chrbndID}.chrtpfbdt"));

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

                        if (GameData.FileExists($@"/chr/{chrbndID.Substring(0, 4)}9.chrtpfbdt"))
                        {
                            extraChrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                            if (extraChrtpfbhd != null)
                            {
                                extraTexbxf = BXF3.Read(chrtpfbhd.Bytes, GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.chrtpfbdt"));

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
                        chr = new Model(progress, chrbndID, chrbnd, 0, anibnd, texbxf,
                            possibleLooseTpfFolder: $@"/chr/{chrbndID}/",
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexChrbnd, 
                            modelToImportDuringLoad: modelToImportDuringLoad,
                            modelImportConfig: importConfig, tpfsUsed: tpfsUsedForPtdeMemoryStuff);

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

                                byte[] existingExtraTexChrBytes = GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx");

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

                        if (GameData.FileExists($@"/chr/{anibndID}.anibnd.dcx"))
                            anibnd = BND3.Read(GameData.ReadFile($@"/chr/{anibndID}.anibnd.dcx"));

                        if (GameData.FileExists($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"))
                            extraTexChrbnd = BND3.Read(GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"));

                        BinderFile chrtpfbhd = null;

                        if (GameData.FileExists($@"/chr/{chrbndID}.chrtpfbdt"))
                        {
                            chrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                            if (chrtpfbhd != null)
                            {
                                texbxf = BXF3.Read(chrtpfbhd.Bytes, GameData.ReadFile($@"/chr/{chrbndID}.chrtpfbdt"));

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

                        if (GameData.FileExists($@"/chr/{chrbndID.Substring(0, 4)}9.chrtpfbdt"))
                        {
                            extraChrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                            if (extraChrtpfbhd != null)
                            {
                                try
                                {
                                    extraTexbxf = BXF3.Read(chrtpfbhd.Bytes, GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.chrtpfbdt"));
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
                        chr = new Model(progress, chrbndID, chrbnd, 0, anibnd, texbxf,
                            possibleLooseTpfFolder: $@"/chr/{chrbndID}/",
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexChrbnd,
                            modelToImportDuringLoad: modelToImportDuringLoad,
                            modelImportConfig: importConfig, tpfsUsed: tpfsUsedForPtdeMemoryStuff);

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

                                byte[] existingExtraTexChrBytes = GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9.chrbnd.dcx");

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

                        if (GameData.FileExists($@"/chr/{anibndID}.anibnd.dcx"))
                            anibnd = BND4.Read(GameData.ReadFile($@"/chr/{anibndID}.anibnd.dcx"));

                        if (GameData.FileExists($@"/chr/{anibndID.Substring(0, 4)}9.chrbnd.dcx"))
                            extraTexbnd = BND4.Read(GameData.ReadFile($@"/chr/{anibndID.Substring(0, 4)}9.chrbnd.dcx"));

                        chr = new Model(progress, chrbndID, chrbnd, 0, anibnd, texbnd: null,
                            additionalTpfNames: new List<string> { GetInterrootPathOld($@"/chr/{chrbndID}_2.tpf.dcx") },
                            possibleLooseTpfFolder: $@"/chr/{chrbndID}/",
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexbnd,
                            modelToImportDuringLoad: modelToImportDuringLoad,
                            modelImportConfig: importConfig);

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

                        if (GameData.FileExists($@"/model/chr/{chrbndID}.texbnd"))
                            texbnd = BND4.Read(GameData.ReadFile($@"/model/chr/{chrbndID}.texbnd"));

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

                        chr = new Model(progress, chrbndID, chrbnd, 0, anibnd: null, texbnd,
                            additionalTpfNames: null,
                            possibleLooseTpfFolder: null,
                            ignoreStaticTransforms: true, additionalTexbnd: null,
                            modelToImportDuringLoad: modelToImportDuringLoad,
                            modelImportConfig: importConfig);

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
                        if (GameRoot.GameIsDemonsSoulsRemastered)
                        {
                            anibnd = BND3.Read(GameData.ReadFile($@"/chr/{anibndID}/{anibndID}.anibnd"));
                        }
                        else
                        {
                            if (!GameData.FileExists($@"/chr/{anibndID}/{anibndID}.anibnd.dcx.2010"))
                                anibnd = TaeEditor.TaeFileContainer.GenerateDemonsSoulsConvertedAnibnd($"{InterrootPath}\\chr\\{anibndID}\\{anibndID}.anibnd.dcx");
                            else
                                anibnd = BND3.Read(GameData.ReadFile($@"/chr/{anibndID}/{anibndID}.anibnd.dcx.2010"));
                        }

                        if (GameData.FileExists($@"/chr/{chrbndID.Substring(0, 4)}9/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"))
                            extraTexChrbnd = BND3.Read(GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9/{chrbndID.Substring(0, 4)}9.chrbnd.dcx"));

                        var tpfsUsedForPtdeMemoryStuff = new List<TPF>();
                        chr = new Model(progress, chrbndID, chrbnd, 0, anibnd, null,
                            possibleLooseTpfFolder: $@"/chr/{chrbndID}/",
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexChrbnd,
                            modelToImportDuringLoad: modelToImportDuringLoad,
                            modelImportConfig: importConfig, tpfsUsed: tpfsUsedForPtdeMemoryStuff);

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

                                byte[] existingExtraTexChrBytes = GameData.ReadFile($@"/chr/{chrbndID.Substring(0, 4)}9/{chrbndID.Substring(0, 4)}9.chrbnd.dcx");

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

                    Scene.AddModel(chr);
                });

            if (chr == null)
            {
                return Model.GetDummyModel();
            }

                if (anibndID == "c0000" || anibndID == "c0000_0000")
                {
                    LoadingTaskMan.DoLoadingTaskSynchronous("c0000_ANIBNDs",
                        "Loading additional player ANIBNDs...", progress =>
                    {

                        var predefinedAdditionalPlayerAnibnds = GetAdditionalPlayerAnibndNames();

                        var additionalPlayerAnibnds = predefinedAdditionalPlayerAnibnds ?? GameData.SearchFiles($@"/chr{(GameType == SoulsGames.DES ? $@"/{anibndID}" : "")}", 
                            $@".*c0000_.*\.anibnd.dcx$");

                        if (GameType == SoulsGames.DS1 && predefinedAdditionalPlayerAnibnds == null)
                        {
                            var additionalPlayerAnibnds_PTDE = GameData.SearchFiles($@"/chr",
                            $@".*c0000_.*\.anibnd$");
                            if (additionalPlayerAnibnds_PTDE.Count > 0)
                            {
                                additionalPlayerAnibnds.AddRange(additionalPlayerAnibnds_PTDE);
                            }
                        }

                        var throwAnibnds = GameData.SearchFiles("/chr", $@"c0000_c\d\d\d\d\.anibnd.dcx$");
                        if (throwAnibnds.Count > 0)
                        {
                            additionalPlayerAnibnds.AddRange(throwAnibnds);
                        }

                        if (GameType == SoulsGames.DS1)
                        {
                            var throwAnibnds_PTDE = GameData.SearchFiles("/chr", $@"c0000_c\d\d\d\d\.anibnd");
                            if (throwAnibnds_PTDE.Count > 0)
                            {
                                additionalPlayerAnibnds.AddRange(throwAnibnds_PTDE);
                            }
                        }

                        additionalPlayerAnibnds = additionalPlayerAnibnds.OrderBy(fn =>
                        {
                            var fnCheck = fn.ToLower();
                            if (fnCheck.Contains("lo"))
                                return 0;
                            else if (fnCheck.Contains("md"))
                                return 1;
                            else if (fnCheck.Contains("hi"))
                                return 2;
                            else
                                return 3;
                        }).ToList();

                        for (int i = 0; i < additionalPlayerAnibnds.Count; i++)
                        {
                            IBinder anibnd = null;

                            string anibndName = additionalPlayerAnibnds[i];

                            if (GameType == SoulsGames.DES && !GameIsDemonsSoulsRemastered)
                            {
                                // Since DeS is unpacked we can just use legacy loose file only shit here
                                anibnd = TaeEditor.TaeFileContainer.GenerateDemonsSoulsConvertedAnibnd($"{InterrootPath}\\{additionalPlayerAnibnds[i].Replace("/", "\\")}");
                            }
                            else
                            {
                                var anibndBytes = GameData.ReadFile(anibndName);
                                if (anibndBytes == null)
                                {
                                    progress.Report(1.0 * i / additionalPlayerAnibnds.Count);
                                    continue;
                                }
                                if (BND3.Is(anibndBytes))
                                    anibnd = BND3.Read(anibndBytes);
                                else
                                    anibnd = BND4.Read(anibndBytes);
                            }

                            bool isFirstPlayerAnibnd = anibndName.ToLower().Contains(GameType == SoulsGames.SDT ? "c0000_a000_lo" : "c0000_a00_lo");

                            chr.AnimContainer.LoadAdditionalANIBND(anibnd, null, scanAnims: isFirstPlayerAnibnd);

                            progress.Report(1.0 * i / additionalPlayerAnibnds.Count);
                        }

                        progress.Report(1);

                        Scene.EnableModelDrawing2();
                    });
                }
                else
                {
                    var additionalAnibnds = GameData.SearchFiles($@"/chr", $@".*{(overrideAnibndID ?? anibndID)}_.*\.anibnd\.dcx$");

                    if (GameType == SoulsGames.DS1)
                    {
                        var additionalAnibnds_PTDE = GameData.SearchFiles($@"/chr", $@".*{(overrideAnibndID ?? anibndID)}_.*\.anibnd$");
                        if (additionalAnibnds_PTDE.Count > 0)
                            additionalAnibnds.AddRange(additionalAnibnds_PTDE);
                    }

                    if (additionalAnibnds.Count > 0)
                    {
                        LoadingTaskMan.DoLoadingTaskSynchronous($"{(overrideAnibndID ?? anibndID)}_ANIBNDs",
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
                                    var anibndBytes = GameData.ReadFile(anibndName);
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

                                chr.AnimContainer.LoadAdditionalANIBND(anibnd, null, scanAnims: true);

                                progress.Report(1.0 * i / additionalAnibnds.Count);
                            }

                            
                        });
                    }
                }

            

            Scene.EnableModelDrawing();
            Scene.EnableModelDrawing2();

            FlverMaterialDefInfo.FlushBinderCache();

            return chr;
            
        }
    }
}
