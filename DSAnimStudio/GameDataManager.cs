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
    public static class GameDataManager
    {
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

        public enum AnimIDFormattingType
        {
            aXX_YYYY,
            aXXX_YYYYYY,
            aXX_YY_ZZZZ,
        }

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
        public static SoulsGames GameType { get; private set; } = SoulsGames.None;

        public static bool GameTypeHasLongAnimIDs => CurrentAnimIDFormatType == AnimIDFormattingType.aXXX_YYYYYY ||
            CurrentAnimIDFormatType == AnimIDFormattingType.aXX_YY_ZZZZ;

        public static AnimIDFormattingType CurrentAnimIDFormatType
        {
            get
            {
                switch (GameType)
                {
                    case SoulsGames.DS1:
                    case SoulsGames.DS1R:
                    case SoulsGames.DES:
                        return AnimIDFormattingType.aXX_YYYY;
                    case SoulsGames.BB:
                    case SoulsGames.DS3:
                    case SoulsGames.SDT:
                        return AnimIDFormattingType.aXXX_YYYYYY;
                    case SoulsGames.DS2SOTFS:
                        return AnimIDFormattingType.aXX_YY_ZZZZ;
                    default:
                        return AnimIDFormattingType.aXXX_YYYYYY;
                }
            }
        }

        public static bool GameTypeIsHavokTagfile =>
            (GameType == SoulsGames.DS1R || GameType == SoulsGames.SDT);

        public static HKX.HKXVariation GetCurrentLegacyHKXType()
        {
            if (GameType == SoulsGames.DES)
                return HKX.HKXVariation.HKXDeS;
            else if (GameType == SoulsGames.DS1 || GameType == SoulsGames.DS1R)
                return HKX.HKXVariation.HKXDS1;
            else if (GameType == SoulsGames.DS3)
                return HKX.HKXVariation.HKXDS3;
            else if (GameType == SoulsGames.BB)
                return HKX.HKXVariation.HKXBloodBorne;
            else if (GameType == SoulsGames.SDT)
                return HKX.HKXVariation.HKXDS1;

            return HKX.HKXVariation.Invalid;
        }

        public static string InterrootPath { get; set; } = null;

        public static string GetInterrootPath(string path, bool isDirectory = false)
        {
            if (isDirectory ? Directory.Exists($@"{InterrootPath}\{path}") : File.Exists($@"{InterrootPath}\{path}"))
            {
                return $@"{InterrootPath}\{path}";
            }
            else if (isDirectory ? Directory.Exists($@"{InterrootPath}\..\{path}") : File.Exists($@"{InterrootPath}\..\{path}"))
            {
                return $@"{InterrootPath}\..\{path}";
            }
            else
            {
                ErrorLog.LogWarning($@"Unable to find the path '<Data Root>\{path}' in the main data root directory ('{InterrootPath}') or the directory in which that one is located.");
                return $@"{InterrootPath}\..\{path}";
            }
        }

        public static List<string> GetInterrootFiles(string path, string match)
        {
            IEnumerable<string> result = new List<string>();

            string mainPath = $@"{InterrootPath}\{path}";
            if (Directory.Exists(mainPath))
            {
                result = result.Concat(Directory.GetFiles(mainPath, match));
            }

            string parentPath = $@"{InterrootPath}\..\{path}";
            if (Directory.Exists(parentPath))
            {
                result = result.Concat(Directory.GetFiles(parentPath, match));
            }

            return result.ToList();
        }

        public static IBinder MTDBND { get; private set; } = null;
        public static Dictionary<string, MTD> MtdDict { get; private set; } = new Dictionary<string, MTD>();

        public static void LoadMTDBND()
        {
            MtdDict.Clear();
            MTDBND = null;

            if (GameType == SoulsGames.SDT)
            {
                MTDBND = BND4.Read(GetInterrootPath($@"mtd\allmaterialbnd.mtdbnd.dcx"));
                
                foreach (var f in MTDBND.Files)
                {
                    var key = Utils.GetShortIngameFileName(f.Name);
                    if (!MtdDict.ContainsKey(key))
                        MtdDict.Add(key, MTD.Read(f.Bytes));
                    else
                        MtdDict[key] = MTD.Read(f.Bytes);
                }
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
                var check = f.Name.ToUpper();
                string interroot = GameDataManager.GetInterrootFromFilePath(bndPath);
                if (check.Contains("FRPG2"))
                {
                    GameDataManager.Init(SoulsAssetPipeline.SoulsGames.DS2SOTFS, interroot);
                    return true;
                }
                else if (check.Contains(@"\FRPG\") && check.Contains(@"INTERROOT_X64"))
                {
                    GameDataManager.Init(SoulsAssetPipeline.SoulsGames.DS1R, interroot);
                    return true;
                }
                else if (check.Contains(@"\FRPG\") && check.Contains(@"INTERROOT_WIN32"))
                {
                    GameDataManager.Init(SoulsAssetPipeline.SoulsGames.DS1, interroot);
                    return true;
                }
                else if (check.Contains(@"\SPRJ\"))
                {
                    GameDataManager.Init(SoulsAssetPipeline.SoulsGames.BB, interroot);
                    return true;
                }
                else if (check.Contains(@"\FDP\"))
                {
                    GameDataManager.Init(SoulsAssetPipeline.SoulsGames.DS3, interroot);
                    return true;
                }
                else if (check.Contains(@"\DemonsSoul\"))
                {
                    GameDataManager.Init(SoulsAssetPipeline.SoulsGames.DES, interroot);
                    return true;
                }
                else if (check.Contains(@"\NTC\"))
                {
                    GameDataManager.Init(SoulsAssetPipeline.SoulsGames.SDT, interroot);
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

        public static void SoftInit(SoulsGames gameType)
        {
            GameType = gameType;
            lastGameType = SoulsGames.None;
        }

        public static void Init(SoulsGames gameType, string interroot, bool forceReload = false)
        {
            GameType = gameType;
            InterrootPath = interroot;
            if (gameType != lastGameType || forceReload)
            {
                ParamManager.LoadParamBND(forceReload);
                FmgManager.LoadAllFMG(forceReload);
                LoadSystex();
                LoadMTDBND();
                FmodManager.Purge();
                //FmodManager.UpdateInterrootForFile(null);
                FmodManager.LoadFloorMaterialNamesFromInterroot();
            }
            lastGameType = GameType;
        }

        public static (string Path, Vector2 UVScale) LookupMTDTexture(string mtdName, string texType)
        {
            LoadSystex();
            var mtdShortName = Utils.GetShortIngameFileName(mtdName);
            if (MtdDict.ContainsKey(mtdShortName))
            {
                var mtd = MtdDict[mtdShortName];

                foreach (var x in mtd.Textures)
                {
                    if (x.Type.ToUpper() == texType.ToUpper())
                    {
                        Vector2 uvScale = Vector2.One;
                        if (x.UnkFloats.Count == 2 && x.UnkFloats[0] > 0 && x.UnkFloats[1] > 0)
                        {
                            uvScale = new Vector2(x.UnkFloats[0], x.UnkFloats[1]);
                        }
                        return (x.Path, uvScale);
                    }
                }
            }

            return (null, Vector2.One);
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
        //        foreach (var msbName in System.IO.Directory.GetFiles(GetInterrootPath($@"map\MapStudio", "*.msb.dcx")))
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
                if (GameType == SoulsGames.DS1)
                {
                    TexturePool.AddTpfsFromPaths(new List<string>
                    {
                        GetInterrootPath($@"other\SYSTEX_TEX.tpf"),
                        GetInterrootPath($@"other\envlight.tpf"),
                        GetInterrootPath($@"other\lensflare.tpf"),
                    }, progress);
                }
                else if (GameType == SoulsGames.DS1R)
                {
                    TexturePool.AddTpfsFromPaths(new List<string>
                    {
                        GetInterrootPath($@"other\SYSTEX_TEX.tpf.dcx"),
                        GetInterrootPath($@"other\envlight.tpf.dcx"),
                        GetInterrootPath($@"other\lensflare.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == SoulsGames.DS3)
                {
                    TexturePool.AddTpfsFromPaths(new List<string>
                    {
                        GetInterrootPath($@"other\systex.tpf.dcx"),
                        GetInterrootPath($@"other\bloodtex.tpf.dcx"),
                        GetInterrootPath($@"other\decaltex.tpf.dcx"),
                        GetInterrootPath($@"other\sysenvtex.tpf.dcx"),
                        GetInterrootPath($@"parts\common_body.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == SoulsGames.BB)
                {
                    // TODO: completely confirm these because I just
                    // copied them from a BB network test file list.
                    TexturePool.AddTpfsFromPaths(new List<string>
                    {
                        GetInterrootPath($@"other\SYSTEX.tpf.dcx"),
                        GetInterrootPath($@"other\decalTex.tpf.dcx"),
                        GetInterrootPath($@"other\bloodTex.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == SoulsGames.SDT)
                {
                    TexturePool.AddTpfsFromPaths(new List<string>
                    {
                        GetInterrootPath($@"other\systex.tpf.dcx"),
                        GetInterrootPath($@"other\maptex.tpf.dcx"),
                        GetInterrootPath($@"other\decaltex.tpf.dcx"),
                        GetInterrootPath($@"parts\common_body.tpf.dcx"),
                    }, progress);
                }
            });

           
        }

        public static Model LoadObject(string id)
        {
            Scene.DisableModelDrawing();
            Scene.DisableModelDrawing2();

            Model obj = null;

            LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_OBJ_{id}", $"Loading object {id}...", progress =>
            {
                
                    if (GameType == SoulsGames.DS3 || GameType == SoulsGames.BB || GameType == SoulsGames.SDT)
                    {
                        var chrbnd = BND4.Read(GetInterrootPath($@"obj\{id}.objbnd.dcx"));

                        obj = new Model(progress, id, chrbnd, 0, null, null);
                    }
                    else if (GameType == SoulsGames.DS1)
                    {
                        var chrbnd = BND3.Read(GetInterrootPath($@"obj\{id}.objbnd"));

                        obj = new Model(progress, id, chrbnd, 0, null, null);
                    }
                    else if (GameType == SoulsGames.DS1R)
                    {
                        var chrbnd = BND3.Read(GetInterrootPath($@"obj\{id}.objbnd.dcx"));

                        obj = new Model(progress, id, chrbnd, 0, null, null);
                    }

                    Scene.AddModel(obj, doLock: false);



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
                                        TexturePool.AddTpfFromPath(GetInterrootPath($@"map\tx\{tex}.tpf"));
                                    }
                                }
                                else if (GameType == SoulsGames.DS3 || GameType == SoulsGames.SDT || GameType == SoulsGames.BB)
                                {
                                    int objGroup = int.Parse(id.Substring(1)) / 1_0000;
                                    string mapDirName = GetInterrootPath($@"map\m{objGroup:D2}");
                                    var tpfBnds = GameDataManager.GetInterrootFiles(mapDirName, "*.tpfbhd");
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

            string chrFolder = GetInterrootPath(
                (GameType == SoulsGames.DS2 || GameType == SoulsGames.DS2SOTFS) ? @"model\chr" : "chr",
                isDirectory: true);
            string[] filesRelatedToThisChr = Directory.GetFiles(chrFolder, $"*{name}*");
            foreach (var f in filesRelatedToThisChr)
            {
                string file = f.ToLower();
                if (file.EndsWith(".bak") || file.EndsWith(".dsasbak") || file.Contains("anibnd") || file.Contains(".2010")
                    || file.EndsWith(ModelImportBackupExtension))
                    continue;

                result.Add(f);
            }

            if (GameType == SoulsGames.DS1)
            {
                string looseTexFolder = Path.Combine(InterrootPath, $@"chr\{name}");
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
                string mapPieceFileName = GetInterrootPath($@"map\{fullMapID}\{mapPieceName}.flver");

                if (GameType == SoulsGames.DS1R)
                    mapPieceFileName += ".dcx";

                LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_MAPPIECE_{fullMapID}_{mapPieceName}", $"Loading map piece {mapPieceName} from map {fullMapID}...", progress =>
                {
                    var flver = FLVER2.Read(mapPieceFileName);
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

        public static Model LoadCharacter(string id, 
            SoulsAssetPipeline.FLVERImporting.FLVER2Importer.ImportedFLVER2Model modelToImportDuringLoad = null,
            List<Action> doImportActionList = null, SapImportConfigs.ImportConfigFlver2 importConfig = null)
        {
            Model chr = null;

            Scene.DisableModelDrawing();
            Scene.DisableModelDrawing2();


                LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_CHR_{id}", $"Loading character {id}...", progress =>
                {
                    if (GameType == SoulsGames.DS3 || GameType == SoulsGames.SDT)
                    {
                        var chrbnd = BND4.Read(GetInterrootPath($@"chr\{id}.chrbnd.dcx"));
                        IBinder texbnd = null;
                        IBinder extraTexbnd = null;
                        IBinder anibnd = null;

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.texbnd.dcx")))
                            texbnd = BND4.Read(GetInterrootPath($@"chr\{id}.texbnd.dcx"));

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.texbnd.dcx")))
                            extraTexbnd = BND4.Read(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.texbnd.dcx"));

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



                        if (GameType == SoulsGames.SDT)
                        {
                            if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.anibnd.dcx.2010")))
                                anibnd = BND4.Read(GetInterrootPath($@"chr\{id}.anibnd.dcx.2010"));
                        }
                        else
                        {
                            if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.anibnd.dcx")))
                                anibnd = BND4.Read(GetInterrootPath($@"chr\{id}.anibnd.dcx"));
                        }

                        chr = new Model(progress, id, chrbnd, 0, anibnd, texbnd,
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexbnd, 
                            modelToImportDuringLoad: modelToImportDuringLoad, 
                            modelImportConfig: importConfig);

                        if (doImportActionList != null)
                        {
                            if (texbnd != null)
                                doImportActionList.Add(() =>
                                {
                                    (texbnd as BND4).Write(GetInterrootPath($@"chr\{id}.texbnd.dcx"));
                                });

                            if (chrbnd != null)
                                doImportActionList.Add(() =>
                                {
                                    (chrbnd as BND4).Write(GetInterrootPath($@"chr\{id}.chrbnd.dcx"));
                                });
                        }
                    }
                    else if (GameType == SoulsGames.DS1)
                    {
                        var chrbnd = BND3.Read(GetInterrootPath($@"chr\{id}.chrbnd"));
                        IBinder anibnd = null;
                        IBinder texbxf = null;
                        IBinder extraTexChrbnd = null;
                        IBinder extraTexbxf = null;

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.anibnd")))
                            anibnd = BND3.Read(GetInterrootPath($@"chr\{id}.anibnd"));

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd")))
                            extraTexChrbnd = BND3.Read(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd"));

                        BinderFile chrtpfbhd = null;

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.chrtpfbdt")))
                        {
                            chrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                            if (chrtpfbhd != null)
                            {
                                texbxf = BXF3.Read(chrtpfbhd.Bytes, GetInterrootPath($@"chr\{id}.chrtpfbdt"));

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

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrtpfbdt")))
                        {
                            extraChrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                            if (extraChrtpfbhd != null)
                            {
                                extraTexbxf = BXF3.Read(chrtpfbhd.Bytes, GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrtpfbdt"));

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

                        chr = new Model(progress, id, chrbnd, 0, anibnd, texbxf,
                            possibleLooseTpfFolder: $@"chr\{id}\",
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexChrbnd, 
                            modelToImportDuringLoad: modelToImportDuringLoad,
                            modelImportConfig: importConfig);

                        if (doImportActionList != null)
                        {
                            if (texbxf != null)
                            {
                                doImportActionList.Add(() =>
                                {
                                    (texbxf as BXF3).Write(out byte[] customBhd, out byte[] customBdt);
                                    chrtpfbhd.Bytes = customBhd;
                                    File.WriteAllBytes(GetInterrootPath($@"chr\{id}.chrtpfbdt"), customBdt);
                                    (chrbnd as BND3).Write(GetInterrootPath($@"chr\{id}.chrbnd"));
                                });
                            }

                            if (extraTexbxf != null)
                            {
                                doImportActionList.Add(() =>
                                {
                                    (texbxf as BXF3).Write(out byte[] customBhd, out byte[] customBdt);
                                    extraChrtpfbhd.Bytes = customBhd;
                                    File.WriteAllBytes(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrtpfbdt"), customBdt);
                                    (extraTexChrbnd as BND3).Write(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd"));
                                });
                            }

                            if (chrbnd != null)
                            {
                                doImportActionList.Add(() =>
                                {
                                    (chrbnd as BND3).Write(GetInterrootPath($@"chr\{id}.chrbnd"));
                                });
                            }

                            if (extraTexChrbnd != null)
                            {
                                doImportActionList.Add(() =>
                                {
                                    (extraTexChrbnd as BND3).Write(GetInterrootPath($@"chr\{id}.chrbnd"));
                                });
                            }
                        }
                    }
                    else if (GameType == SoulsGames.DS1R)
                    {
                        var chrbnd = BND3.Read(GetInterrootPath($@"chr\{id}.chrbnd.dcx"));
                        IBinder anibnd = null;
                        IBinder texbxf = null;
                        IBinder extraTexChrbnd = null;
                        IBinder extraTexbxf = null;

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.anibnd.dcx.2010")))
                            anibnd = BND3.Read(GetInterrootPath($@"chr\{id}.anibnd.dcx.2010"));
                        else if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.anibnd.dcx")))
                            anibnd = BND3.Read(GetInterrootPath($@"chr\{id}.anibnd.dcx"));

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd.dcx")))
                            extraTexChrbnd = BND3.Read(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd.dcx"));

                        BinderFile chrtpfbhd = null;

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.chrtpfbdt")))
                        {
                            chrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                            if (chrtpfbhd != null)
                            {
                                texbxf = BXF3.Read(chrtpfbhd.Bytes, GetInterrootPath($@"chr\{id}.chrtpfbdt"));

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

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrtpfbdt")))
                        {
                            extraChrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                            if (extraChrtpfbhd != null)
                            {
                                extraTexbxf = BXF3.Read(chrtpfbhd.Bytes, GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrtpfbdt"));

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

                        chr = new Model(progress, id, chrbnd, 0, anibnd, texbxf,
                            possibleLooseTpfFolder: $@"chr\{id}\",
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexChrbnd,
                            modelToImportDuringLoad: modelToImportDuringLoad,
                            modelImportConfig: importConfig);

                        if (doImportActionList != null)
                        {
                            if (texbxf != null)
                            {
                                doImportActionList.Add(() =>
                                {
                                    (texbxf as BXF3).Write(out byte[] customBhd, out byte[] customBdt);
                                    chrtpfbhd.Bytes = customBhd;
                                    File.WriteAllBytes(GetInterrootPath($@"chr\{id}.chrtpfbdt"), customBdt);
                                    (chrbnd as BND3).Write(GetInterrootPath($@"chr\{id}.chrbnd.dcx"));
                                });
                            }

                            if (extraTexbxf != null)
                            {
                                doImportActionList.Add(() =>
                                {
                                    (texbxf as BXF3).Write(out byte[] customBhd, out byte[] customBdt);
                                    extraChrtpfbhd.Bytes = customBhd;
                                    File.WriteAllBytes(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrtpfbdt"), customBdt);
                                    (extraTexChrbnd as BND3).Write(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd.dcx"));
                                });
                            }

                            if (chrbnd != null)
                                doImportActionList.Add(() =>
                                {
                                    (chrbnd as BND3).Write(GetInterrootPath($@"chr\{id}.chrbnd.dcx"));
                                });

                            if (extraTexChrbnd != null)
                                doImportActionList.Add(() =>
                                {
                                    (extraTexChrbnd as BND3).Write(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd.dcx"));
                                });
                        }
                    }
                    else if (GameType == SoulsGames.BB)
                    {
                        IBinder chrbnd = BND4.Read(GetInterrootPath($@"chr\{id}.chrbnd.dcx"));
                        IBinder anibnd = null;
                        IBinder extraTexbnd = null;

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.anibnd.dcx")))
                            anibnd = BND4.Read(GetInterrootPath($@"chr\{id}.anibnd.dcx"));

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd.dcx")))
                            extraTexbnd = BND4.Read(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd.dcx"));

                        chr = new Model(progress, id, chrbnd, 0, anibnd, texbnd: null,
                            additionalTpfNames: new List<string> { GetInterrootPath($@"chr\{id}_2.tpf.dcx") },
                            possibleLooseTpfFolder: $@"chr\{id}\",
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexbnd,
                            modelToImportDuringLoad: modelToImportDuringLoad,
                            modelImportConfig: importConfig);

                        if (doImportActionList != null)
                        {
                            if (chrbnd != null)
                                doImportActionList.Add(() =>
                                {
                                    (chrbnd as BND4).Write(GetInterrootPath($@"chr\{id}.chrbnd.dcx"));
                                });
                        }
                    }
                    else if (GameType == SoulsGames.DS2SOTFS)
                    {
                        IBinder chrbnd = BND4.Read(GetInterrootPath($@"model\chr\{id}.bnd"));
                        IBinder texbnd = null;

                        if (System.IO.File.Exists(GetInterrootPath($@"model\chr\{id}.texbnd")))
                            texbnd = BND4.Read(GetInterrootPath($@"model\chr\{id}.texbnd"));

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

                        chr = new Model(progress, id, chrbnd, 0, anibnd: null, texbnd,
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
                                    (texbnd as BND4).Write(GetInterrootPath($@"model\chr\{id}.bnd"));
                                });

                            if (chrbnd != null)
                                doImportActionList.Add(() =>
                                {
                                    (chrbnd as BND4).Write(GetInterrootPath($@"model\chr\{id}.texbnd"));
                                });
                        }
                    }

                    Scene.AddModel(chr, doLock: false);
                });

                if (id == "c0000" || id == "c0000_0000")
                {
                    LoadingTaskMan.DoLoadingTaskSynchronous("c0000_ANIBNDs",
                        "Loading additional player ANIBNDs...", progress =>
                    {
                        string[] anibnds = GameDataManager.GetInterrootFiles($@"chr",
                            GameType == SoulsGames.DS1 ? "c0000_*.anibnd" : "c0000_*.anibnd.dcx")
                        .OrderBy(fn =>
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
                        }).ToArray();

                        for (int i = 0; i < anibnds.Length; i++)
                        {
                            IBinder anibnd = null;

                            string anibndName = anibnds[i];

                            if ((GameType == SoulsGames.SDT || GameType == SoulsGames.DS1R) && System.IO.File.Exists(anibnds[i] + ".2010"))
                            {
                                anibndName = anibnds[i] + ".2010";
                            }

                            if (BND3.Is(anibndName))
                                anibnd = BND3.Read(anibndName);
                            else
                                anibnd = BND4.Read(anibndName);

                            string anibndCheck = Path.GetFileNameWithoutExtension(anibndName).ToLower();

                            bool isFirstPlayerAnibnd = anibndCheck.Contains(GameType == SoulsGames.SDT ? "c0000_a000_lo" : "c0000_a00_lo");

                            chr.AnimContainer.LoadAdditionalANIBND(anibnd, null, scanAnims: isFirstPlayerAnibnd);

                            progress.Report(1.0 * i / anibnds.Length);
                        }

                        progress.Report(1);

                        Scene.EnableModelDrawing2();
                    });
                }
                else
                {
                    var additionalAnibnds = GameDataManager.GetInterrootFiles($@"chr", $"{id}_*.anibnd*");

                    if (additionalAnibnds.Count > 0)
                    {
                        LoadingTaskMan.DoLoadingTaskSynchronous($"{id}_ANIBNDs",
                        "Loading additional character ANIBNDs...", progress =>
                        {
                            for (int i = 0; i < additionalAnibnds.Count; i++)
                            {
                                IBinder anibnd = null;

                                string anibndName = additionalAnibnds[i];

                                if ((GameType == SoulsGames.SDT || GameType == SoulsGames.DS1R) && System.IO.File.Exists(additionalAnibnds[i] + ".2010"))
                                {
                                    anibndName = additionalAnibnds[i] + ".2010";
                                }

                                if (BND3.Is(anibndName))
                                    anibnd = BND3.Read(anibndName);
                                else
                                    anibnd = BND4.Read(anibndName);

                                chr.AnimContainer.LoadAdditionalANIBND(anibnd, null, scanAnims: true);

                                progress.Report(1.0 * i / additionalAnibnds.Count);
                            }

                            
                        });
                    }
                }

            

            Scene.EnableModelDrawing();
            Scene.EnableModelDrawing2();

            return chr;
            
        }
    }
}
