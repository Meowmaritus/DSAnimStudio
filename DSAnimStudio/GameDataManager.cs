using SoulsFormats;
using SFAnimExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;

namespace DSAnimStudio
{
    public static class GameDataManager
    {
        public enum GameTypes
        {
            None,
            DES,
            DS1,
            DS1R,
            //DS2,
            DS2SOTFS,
            DS3,
            BB,
            SDT,
        }

        public enum AnimIDFormattingType
        {
            a00_0000,
            a000_000000,
            a00_00_0000,
        }

        public static readonly Dictionary<GameTypes, string> GameTypeNames =
            new Dictionary<GameTypes, string>
        {
            { GameTypes.None, "<NONE>" },
            { GameTypes.DES, "Demon's Souls" },
            { GameTypes.DS1, "Dark Souls: Prepare to Die Edition" },
            { GameTypes.DS1R, "Dark Souls Remastered" },
            //{ GameTypes.DS2, "Dark Souls II" },
            { GameTypes.DS2SOTFS, "Dark Souls II: Scholar of the First Sin" },
            { GameTypes.DS3, "Dark Souls III" },
            { GameTypes.BB, "Bloodborne" },
            { GameTypes.SDT, "Sekiro: Shadows Die Twice" },
        };

        public static bool CheckGameTypeParamIDCompatibility(GameTypes a, GameTypes b)
        {
            if (a == GameTypes.DS1 && b == GameTypes.DS1R)
                return true;
            else if (a == GameTypes.DS1R && b == GameTypes.DS1)
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

        private static GameTypes lastGameType = GameTypes.None;
        public static GameTypes GameType { get; private set; } = GameTypes.None;

        public static bool GameTypeHasLongAnimIDs => CurrentAnimIDFormatType == AnimIDFormattingType.a000_000000 ||
            CurrentAnimIDFormatType == AnimIDFormattingType.a00_00_0000;

        public static AnimIDFormattingType CurrentAnimIDFormatType
        {
            get
            {
                switch (GameType)
                {
                    case GameTypes.DS1:
                    case GameTypes.DS1R:
                    case GameTypes.DES:
                        return AnimIDFormattingType.a00_0000;
                    case GameTypes.BB:
                    case GameTypes.DS3:
                    case GameTypes.SDT:
                        return AnimIDFormattingType.a000_000000;
                    case GameTypes.DS2SOTFS:
                        return AnimIDFormattingType.a00_00_0000;
                    default:
                        return AnimIDFormattingType.a000_000000;
                }
            }
        }

        public static bool GameTypeIsHavokTagfile =>
            (GameType == GameTypes.DS1R || GameType == GameTypes.SDT);

        public static HKX.HKXVariation GetCurrentLegacyHKXType()
        {
            if (GameType == GameTypes.DES)
                return HKX.HKXVariation.HKXDeS;
            else if (GameType == GameTypes.DS1 || GameType == GameTypes.DS1R)
                return HKX.HKXVariation.HKXDS1;
            else if (GameType == GameTypes.DS3)
                return HKX.HKXVariation.HKXDS3;
            else if (GameType == GameTypes.BB)
                return HKX.HKXVariation.HKXBloodBorne;
            else if (GameType == GameTypes.SDT)
                return HKX.HKXVariation.HKXDS1;

            return HKX.HKXVariation.Invalid;
        }

        public static string InterrootPath { get; set; } = null;

        public static string GetInterrootPath(string path)
        {
            if (File.Exists($@"{InterrootPath}\{path}"))
            {
                return $@"{InterrootPath}\{path}";
            }
            else if (File.Exists($@"{InterrootPath}\..\{path}"))
            {
                return $@"{InterrootPath}\..\{path}";
            }
            else
            {
                ErrorLog.LogWarning($@"Unable to find the file '<Data Root>\{path}' in the main data root directory ('{InterrootPath}') or the directory in which that one is located.");
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

            if (GameType == GameTypes.SDT)
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
                    GameDataManager.Init(GameDataManager.GameTypes.DS2SOTFS, interroot);
                    return true;
                }
                else if (check.Contains(@"\FRPG\") && check.Contains(@"INTERROOT_X64"))
                {
                    GameDataManager.Init(GameDataManager.GameTypes.DS1R, interroot);
                    return true;
                }
                else if (check.Contains(@"\FRPG\") && check.Contains(@"INTERROOT_WIN32"))
                {
                    GameDataManager.Init(GameDataManager.GameTypes.DS1, interroot);
                    return true;
                }
                else if (check.Contains(@"\SPRJ\"))
                {
                    GameDataManager.Init(GameDataManager.GameTypes.BB, interroot);
                    return true;
                }
                else if (check.Contains(@"\FDP\"))
                {
                    GameDataManager.Init(GameDataManager.GameTypes.DS3, interroot);
                    return true;
                }
                else if (check.Contains(@"\DemonsSoul\"))
                {
                    GameDataManager.Init(GameDataManager.GameTypes.DES, interroot);
                    return true;
                }
                else if (check.Contains(@"\NTC\"))
                {
                    GameDataManager.Init(GameDataManager.GameTypes.SDT, interroot);
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

        public static void Init(GameTypes gameType, string interroot, bool forceReload = false)
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
                if (GameType == GameTypes.DS1)
                {
                    TexturePool.AddTpfsFromPaths(new List<string>
                    {
                        GetInterrootPath($@"other\SYSTEX_TEX.tpf"),
                        GetInterrootPath($@"other\envlight.tpf"),
                        GetInterrootPath($@"other\lensflare.tpf"),
                    }, progress);
                }
                else if (GameType == GameTypes.DS1R)
                {
                    TexturePool.AddTpfsFromPaths(new List<string>
                    {
                        GetInterrootPath($@"other\SYSTEX_TEX.tpf.dcx"),
                        GetInterrootPath($@"other\envlight.tpf.dcx"),
                        GetInterrootPath($@"other\lensflare.tpf.dcx"),
                    }, progress);
                }
                else if (GameType == GameTypes.DS3)
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
                else if (GameType == GameTypes.BB)
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
                else if (GameType == GameTypes.SDT)
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
                
                    if (GameType == GameTypes.DS3 || GameType == GameTypes.BB || GameType == GameTypes.SDT)
                    {
                        var chrbnd = BND4.Read(GetInterrootPath($@"obj\{id}.objbnd.dcx"));

                        obj = new Model(progress, id, chrbnd, 0, null, null);
                    }
                    else if (GameType == GameTypes.DS1)
                    {
                        var chrbnd = BND3.Read(GetInterrootPath($@"obj\{id}.objbnd"));

                        obj = new Model(progress, id, chrbnd, 0, null, null);
                    }
                    else if (GameType == GameTypes.DS1R)
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
                                if (GameType == GameTypes.DS1)
                                {
                                    foreach (var tex in texturesToLoad)
                                    {
                                        TexturePool.AddTpfFromPath(GetInterrootPath($@"map\tx\{tex}.tpf"));
                                    }
                                }
                                else if (GameType == GameTypes.DS3 || GameType == GameTypes.SDT || GameType == GameTypes.BB)
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

        public static Model LoadCharacter(string id)
        {
            Model chr = null;

            Scene.DisableModelDrawing();
            Scene.DisableModelDrawing2();


                LoadingTaskMan.DoLoadingTaskSynchronous($"LOAD_CHR_{id}", $"Loading character {id}...", progress =>
                {
                    if (GameType == GameTypes.DS3 || GameType == GameTypes.SDT)
                    {
                        var chrbnd = BND4.Read(GetInterrootPath($@"chr\{id}.chrbnd.dcx"));
                        IBinder texbnd = null;
                        IBinder extraTexbnd = null;
                        IBinder anibnd = null;

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.texbnd.dcx")))
                            texbnd = BND4.Read(GetInterrootPath($@"chr\{id}.texbnd.dcx"));

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.texbnd.dcx")))
                            extraTexbnd = BND4.Read(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.texbnd.dcx"));

                        if (GameType == GameTypes.SDT)
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
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexbnd);
                    }
                    else if (GameType == GameTypes.DS1)
                    {
                        var chrbnd = BND3.Read(GetInterrootPath($@"chr\{id}.chrbnd"));
                        IBinder anibnd = null;
                        IBinder texbnd = null;
                        IBinder extraTexbnd = null;

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.anibnd")))
                            anibnd = BND3.Read(GetInterrootPath($@"chr\{id}.anibnd"));

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd")))
                            extraTexbnd = BND3.Read(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd"));

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.chrtpfbdt")))
                        {
                            var chrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                            if (chrtpfbhd != null)
                            {
                                texbnd = BXF3.Read(chrtpfbhd.Bytes, GetInterrootPath($@"chr\{id}.chrtpfbdt"));
                            }
                        }

                        chr = new Model(progress, id, chrbnd, 0, anibnd, texbnd,
                            possibleLooseTpfFolder: $@"chr\{id}\",
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexbnd);
                    }
                    else if (GameType == GameTypes.DS1R)
                    {
                        var chrbnd = BND3.Read(GetInterrootPath($@"chr\{id}.chrbnd.dcx"));
                        IBinder anibnd = null;
                        IBinder texbnd = null;
                        IBinder extraTexbnd = null;

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.anibnd.dcx.2010")))
                            anibnd = BND3.Read(GetInterrootPath($@"chr\{id}.anibnd.dcx.2010"));
                        else if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.anibnd.dcx")))
                            anibnd = BND3.Read(GetInterrootPath($@"chr\{id}.anibnd.dcx"));

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd.dcx")))
                            extraTexbnd = BND3.Read(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd.dcx"));

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.chrtpfbdt")))
                        {
                            var chrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                            if (chrtpfbhd != null)
                            {
                                texbnd = BXF3.Read(chrtpfbhd.Bytes, GetInterrootPath($@"chr\{id}.chrtpfbdt"));
                            }
                        }

                        chr = new Model(progress, id, chrbnd, 0, anibnd, texbnd,
                            possibleLooseTpfFolder: $@"chr\{id}\",
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexbnd);
                    }
                    else if (GameType == GameTypes.BB)
                    {
                        var chrbnd = BND4.Read(GetInterrootPath($@"chr\{id}.chrbnd.dcx"));
                        IBinder anibnd = null;
                        IBinder extraTexbnd = null;

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id}.anibnd.dcx")))
                            anibnd = BND4.Read(GetInterrootPath($@"chr\{id}.anibnd.dcx"));

                        if (System.IO.File.Exists(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd.dcx")))
                            extraTexbnd = BND4.Read(GetInterrootPath($@"chr\{id.Substring(0, 4)}9.chrbnd.dcx"));

                        chr = new Model(progress, id, chrbnd, 0, anibnd, texbnd: null,
                            additionalTpfNames: new List<string> { GetInterrootPath($@"chr\{id}_2.tpf.dcx") },
                            possibleLooseTpfFolder: $@"chr\{id}\",
                            ignoreStaticTransforms: true, additionalTexbnd: extraTexbnd);
                    }
                    else if (GameType == GameTypes.DS2SOTFS)
                    {
                        var chrbnd = BND4.Read(GetInterrootPath($@"model\chr\{id}.bnd"));
                        IBinder texbnd = null;

                        if (System.IO.File.Exists(GetInterrootPath($@"model\chr\{id}.texbnd")))
                            texbnd = BND4.Read(GetInterrootPath($@"model\chr\{id}.texbnd"));

                        chr = new Model(progress, id, chrbnd, 0, anibnd: null, texbnd,
                            additionalTpfNames: null,
                            possibleLooseTpfFolder: null,
                            ignoreStaticTransforms: true, additionalTexbnd: null);
                    }

                    Scene.AddModel(chr, doLock: false);
                });

                if (id == "c0000")
                {
                    chr.IS_PLAYER = true;

                    LoadingTaskMan.DoLoadingTaskSynchronous("c0000_ANIBNDs",
                        "Loading additional player ANIBNDs...", progress =>
                    {
                        string[] anibnds = GameDataManager.GetInterrootFiles($@"chr",
                            GameType == GameTypes.DS1 ? "c0000_*.anibnd" : "c0000_*.anibnd.dcx")
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

                            if ((GameType == GameTypes.SDT || GameType == GameTypes.DS1R) && System.IO.File.Exists(anibnds[i] + ".2010"))
                            {
                                anibndName = anibnds[i] + ".2010";
                            }

                            if (BND3.Is(anibndName))
                                anibnd = BND3.Read(anibndName);
                            else
                                anibnd = BND4.Read(anibndName);

                            string anibndCheck = Path.GetFileNameWithoutExtension(anibndName).ToLower();

                            bool isFirstPlayerAnibnd = anibndCheck.Contains(GameType == GameTypes.SDT ? "c0000_a000_lo" : "c0000_a00_lo");

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

                                if ((GameType == GameTypes.SDT || GameType == GameTypes.DS1R) && System.IO.File.Exists(additionalAnibnds[i] + ".2010"))
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
