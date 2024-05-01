using SoulsAssetPipeline;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class GameData
    {
        public static object _lock_ProjectJson = new object();
        private static TaeEditor.TaeProjectJson ProjectJson;

        public static void ProjectJsonLockAct(Action<TaeEditor.TaeProjectJson> act)
        {
            lock (_lock_ProjectJson)
            {
                act?.Invoke(ProjectJson);
            }
        }

        public static bool LoadProjectJson()
        {
            var result = false;
            lock (_lock_ProjectJson)
            {
                if (File.Exists(ProjectPath))
                {
                    ProjectJson = TaeEditor.TaeProjectJson.Load(ProjectPath);
                    if (ProjectJson.WorldViews == null)
                        ProjectJson.WorldViews = new List<WorldView>();
                    if (ProjectJson.WorldViews.Count == 0)
                        ProjectJson.WorldViews.Add(new WorldView());
                    WorldViewManager.SetWorldViewList(ProjectJson.WorldViews);

                    NotificationManager.PushNotification($"Loaded project config from '{System.IO.Path.GetFullPath(ProjectPath)}'.");
                    result = true;
                }
                else
                {
                    ProjectJson = new TaeEditor.TaeProjectJson()
                    {
                        GameDirectory = GameRoot.DefaultGameDir,
                        ModEngineDirectory = "",
                    };

                    ProjectJson.InitDefaults();
                    ProjectJson.Save(ProjectPath);

                    NotificationManager.PushNotification($"Saved default project config to '{System.IO.Path.GetFullPath(ProjectPath)}'.");
                    result = false;
                }
            }
            
            return result;
        }

        public static void SaveProjectJson(bool silent = false)
        {
            ProjectJsonLockAct(proj =>
            {
                if (proj == null)
                {
                    ProjectJson = new TaeEditor.TaeProjectJson()
                    {
                        GameDirectory = GameRoot.DefaultGameDir,
                        ModEngineDirectory = "",
                    };
                }

                //proj.WorldViews = WorldViewManager.GetWorldViewList();



                proj.Save(ProjectPath);

            });
            //if (!silent)
            //    NotificationManager.PushNotification($"Saved project config to '{System.IO.Path.GetFullPath(ProjectPath)}'.");
        }

        public class BhdCacheInfo
        {
            public class BhdCacheInfoEntry
            {
                public string BhdName { get; set; }
                public byte[] EncMD5Hash { get; set; } = new byte[0];
            }

            public static BhdCacheInfo ReadJson(string filePath)
            {
                var jsonText = File.ReadAllText(filePath);
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<BhdCacheInfo>(jsonText);
                return result;
            }

            public void WriteJson(string filePath)
            {
                var jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
                var dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(filePath, jsonText);
            }

            public List<BhdCacheInfoEntry> Entries { get; set; } = new List<BhdCacheInfoEntry>();

            public void RegistBHD(string bhdName, string bhdFilePath)
            {
                var bhdFile = File.ReadAllBytes(bhdFilePath);
                var entry = new BhdCacheInfoEntry();
                entry.BhdName = bhdName;
                var md5 = MD5.Create();
                entry.EncMD5Hash = md5.ComputeHash(bhdFile);

                Entries.Add(entry);

                NotificationManager.PushNotification($"New BHD info entry created for '{bhdName}'.");
            }
        }

        public static string InterrootPath => GameRoot.InterrootPath;
        public static string ModengineRootPath => GameRoot.InterrootModenginePath;
        public static string ScratchPath => GameRoot.ScratchPath;
        public static string CachePath => GameRoot.CachePath;
        public static string CacheInfoPath => $"{GameRoot.CachePath}\\_CACHE_INFO.json";
        public static string ProjectPath => GameRoot.ProjectPath;

        private static BhdCacheInfo CacheInfo = null;
        public static void InitBhdCacheInfo()
        {
            lock (_lock_DataArchives)
            {
                CacheInfo = new BhdCacheInfo();
                if (!string.IsNullOrEmpty(CachePath))
                {
                    var path = CacheInfoPath;
                    if (File.Exists(path))
                    {
                        CacheInfo = BhdCacheInfo.ReadJson(path);
                    }
                    else
                    {
                        CacheInfo = new BhdCacheInfo();
                        CacheInfo.WriteJson(path);
                        NotificationManager.PushNotification("New BHD cache info file created.");
                    }
                }
            }
        }

        public static void SaveBhdCacheInfo()
        {
            lock (_lock_DataArchives)
            {
                if (!string.IsNullOrEmpty(CachePath))
                {
                    CacheInfo.WriteJson(CacheInfoPath);
                    NotificationManager.PushNotification($"BHD info entries saved.");
                }
            }
        }

        public static void RegistBhdCacheEntry(string bhdName, string bhdPath)
        {
            lock (_lock_DataArchives)
            {
                if (!string.IsNullOrEmpty(CachePath))
                {
                    CacheInfo.RegistBHD(bhdName, bhdPath);
                    CacheInfo.WriteJson(CacheInfoPath);
                }
            }
        }

        public static bool CheckIfBhdCacheIsValid(string bhdName)
        {
            var bhdPath = Path.GetFullPath($"{InterrootPath}\\{bhdName}.bhd");
            var md5 = MD5.Create();
            var bhdBytes = File.ReadAllBytes(bhdPath);
            var bhdMD5 = md5.ComputeHash(bhdBytes);
            bool result = false;
            lock (_lock_DataArchives)
            {
                if (!string.IsNullOrEmpty(CachePath))
                {
                    foreach (var entry in CacheInfo.Entries)
                    {
                        if (entry.BhdName.ToLowerInvariant() == bhdName.ToLowerInvariant()
                            && entry.EncMD5Hash != null && entry.EncMD5Hash.Length == bhdMD5.Length)
                        {
                            if (entry.EncMD5Hash.SequenceEqual(bhdMD5))
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static bool CfgLoadLooseParams = false;
        public static bool CfgLoadUnpackedGameFiles = false;

        public static bool IsLoadingRegulationParams
        {
            get
            {
                if (GameRoot.GameType is SoulsGames.DES or SoulsGames.DS1 or SoulsGames.DS1R or SoulsGames.SDT)
                    return false;
                else
                    return !CfgLoadLooseParams;
            }
        }

        public static bool IsLoadingUnpackedFiles => CfgLoadUnpackedGameFiles || 
            (GameRoot.GameType is SoulsGames.DS1R or SoulsGames.DES or SoulsGames.BB);

        public static SoulsGames GameType => GameRoot.GameType;
        public static Dictionary<string, EblArchive> DataArchives = new Dictionary<string, EblArchive>();
        private static EblArchive EldenRingSoundArchive = null;
        public static List<string> UxmDictionary = new List<string>();

        private static object _lock_DataArchives = new object();

        private static object _lock_EblInnerFileCache = new object();
        private static Dictionary<string, byte[]> EblInnerFileCache = new Dictionary<string, byte[]>();

        public class TempEblUnpackFile : IDisposable
        {
            public string EblPath;
            public string AbsolutePath => $@"{ScratchPath}\{EblPath.Replace("/", "\\")}";
            public byte[] FileBytes;
            public void Dispose()
            {
                if (File.Exists(AbsolutePath))
                    File.Delete(AbsolutePath);
            }
        }
        private static List<TempEblUnpackFile> TempUnpackedEblFiles = new List<TempEblUnpackFile>();

        public static string FindFileInGameData(string filePath)
        {
            var justFileName = Path.GetFileName(filePath).ToLowerInvariant();
            var allFilesInDir = GetFilesInDir(null, SearchType.AllFiles);

            var result = allFilesInDir
                .Where(f => f.ToLowerInvariant().EndsWith(justFileName.ToLowerInvariant()))
                .FirstOrDefault();
            if (result == null && CheckUnDcxFiles)
            {
                if (justFileName.EndsWith(".dcx"))
                    result = GetFilesInDir(null, SearchType.AllFiles)
                        .Where(f => f.ToLowerInvariant().EndsWith(justFileName.Substring(0, justFileName.Length - 4)))
                        .FirstOrDefault();
                else
                    result = GetFilesInDir(null, SearchType.AllFiles)
                        .Where(f => f.ToLowerInvariant().EndsWith(justFileName + ".dcx"))
                        .FirstOrDefault();
            }

            return result;
        }

        public static List<string> SearchFiles(string directoryPath, string matchRegex)
        {
            if (GameData.GameType == SoulsGames.DS1)
            {
                var looseFiles = GetFilesInDir(directoryPath, SearchType.LooseOnly)
                    .Where(x => System.Text.RegularExpressions.Regex.IsMatch(x, matchRegex.Replace(".dcx$", "$"), System.Text.RegularExpressions.RegexOptions.CultureInvariant)).ToList();

                var eblFiles = GetFilesInDir(directoryPath, SearchType.EblOnly)
                    .Where(x => System.Text.RegularExpressions.Regex.IsMatch(x, matchRegex, System.Text.RegularExpressions.RegexOptions.CultureInvariant)).ToList();

                return eblFiles.Concat(looseFiles).ToList();
            }
            else
            {
                var filesInDir = GetFilesInDir(directoryPath, SearchType.AllFiles);
                return filesInDir.Where(x => System.Text.RegularExpressions.Regex.IsMatch(x, matchRegex, System.Text.RegularExpressions.RegexOptions.CultureInvariant)).ToList();
            }

         
        }

        public enum SearchType
        {
            AllFiles,
            EblOnly,
            LooseOnly,
        }

        public static List<string> GetFilesInDir(string directoryPath, SearchType searchType = SearchType.AllFiles)
        {
            if (directoryPath != null)
            {
                directoryPath = directoryPath.ToLower().Replace("\\", "/");
                if (!directoryPath.StartsWith("/"))
                    directoryPath = "/" + directoryPath;
            }

            var result = ((!IsLoadingUnpackedFiles || searchType is SearchType.EblOnly) && searchType != SearchType.LooseOnly) ? UxmDictionary.Where(x => directoryPath == null || x.StartsWith(directoryPath)).ToList() : new List<string>();

            if (searchType is SearchType.EblOnly)
            {
                result = result
                .OrderBy(x => x)
                .ToList();
                return result;
            }

            string looseFolderPath = $@"{InterrootPath}{(directoryPath?.Replace("/", "\\") ?? "")}";
            if (System.IO.Directory.Exists(looseFolderPath) && IsLoadingUnpackedFiles)
                result = result.Concat(System.IO.Directory.GetFiles(looseFolderPath, "*", SearchOption.AllDirectories)
                    .Select(x => x.ToLower().Replace("\\", "/").StartsWith(InterrootPath.ToLower().Replace("\\", "/")) ?
                    x.Substring(InterrootPath.Length).Replace("\\", "/") : x.Replace("\\", "/"))).ToList();

            if (searchType is SearchType.LooseOnly)
            {
                result = result
                .OrderBy(x => x)
                .ToList();
                return result;
            }

            if (!string.IsNullOrWhiteSpace(ModengineRootPath))
            {
                string modengineLooseFolderPath = $@"{ModengineRootPath}{(directoryPath?.Replace("/", "\\") ?? "")}";
                if (System.IO.Directory.Exists(modengineLooseFolderPath))
                    result = result.Concat(System.IO.Directory.GetFiles(modengineLooseFolderPath, "*", SearchOption.AllDirectories)
                        .Select(x => x.ToLower().Replace("\\", "/").StartsWith(ModengineRootPath.ToLower().Replace("\\", "/")) ?
                        x.Substring(ModengineRootPath.Length).Replace("\\", "/") : x.Replace("\\", "/"))).ToList();
            }

            result = result
                .OrderBy(x => x)
                .ToList();
            return result;
        }

        private static void LoadUxmDictionary()
        {
            string uxmDictName = $"{Main.Directory}\\Res\\UxmDict\\{GameType}.txt";
            if (File.Exists(uxmDictName))
                UxmDictionary = System.IO.File.ReadAllLines(uxmDictName).Select(x => x.TrimEnd('\r')).ToList();
            else
                UxmDictionary = new List<string>();
        }

        public static void ClearAll()
        {
            lock (_lock_DataArchives)
            {
                DataArchives.Clear();
            }
            lock (_lock_EblInnerFileCache)
            {
                EblInnerFileCache.Clear();
                foreach (var x in TempUnpackedEblFiles)
                    x?.Dispose();
                TempUnpackedEblFiles?.Clear();
                if (Directory.Exists($@"{ScratchPath}\_DSAS_Temp"))
                {
                    Directory.Delete($@"{ScratchPath}\_DSAS_Temp", true);
                }
            }
        }

        public static void SaveCurrentEblHeaderCaches(string directory)
        {
            if (!System.IO.Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            lock (_lock_DataArchives)
            {
                foreach (var kvp in DataArchives)
                {
                    if (kvp.Value.DecryptedBhdFile != null)
                    {
                        using (var fileWriteStream = File.OpenWrite($@"{directory}\{kvp.Key}.bhd"))
                        {
                            kvp.Value.DecryptedBhdFile.Write(fileWriteStream);
                        }
                    }
                }
            }
        }

        public static void ForceLoadEbl(string eblKey)
        {
            //bool failed = false;
            lock (_lock_DataArchives)
            {
                if (DataArchives.ContainsKey(eblKey))
                    DataArchives[eblKey].InitBHD();
                //else
                //    failed = true;
            }
            //if (failed)
            //    throw new ArgumentException("Specified EBL key did not exist.");
        }

        public static void SetCacheEnabled(bool useCache)
        {
            lock (_lock_DataArchives)
            {
                foreach (var d in DataArchives)
                {
                    d.Value.UseCache = useCache;
                }
            }
        }

        //public static void LoadAllEblHeaders(bool useCache)
        //{
        //    lock (_lock_DataArchives)
        //    {
        //        foreach (var d in DataArchives)
        //        {
        //            d.Value.UseCache = false;
        //        }
        //    }
        //}

        public static void InitEBLs()
        {
            ClearAll();
            
            LoadUxmDictionary();
            lock (_lock_DataArchives)
            {
                if (GameType == SoulsGames.DS1)
                {
                    DataArchives.Add("dvdbnd0", new EblArchive("dvdbnd0", "dvdbnd0.bhd5", "dvdbnd0.bdt", GameType));
                    DataArchives.Add("dvdbnd1", new EblArchive("dvdbnd1", "dvdbnd1.bhd5", "dvdbnd1.bdt", GameType));
                    DataArchives.Add("dvdbnd2", new EblArchive("dvdbnd2", "dvdbnd2.bhd5", "dvdbnd2.bdt", GameType));
                    DataArchives.Add("dvdbnd3", new EblArchive("dvdbnd3", "dvdbnd3.bhd5", "dvdbnd3.bdt", GameType));

                }
                else if (GameType == SoulsGames.DS3)
                {
                    DataArchives.Add("data1", new EblArchive("data1", "data1.bhd", "data1.bdt", GameType));
                    DataArchives.Add("data2", new EblArchive("data2", "data2.bhd", "data2.bdt", GameType));
                    DataArchives.Add("data3", new EblArchive("data3", "data3.bhd", "data3.bdt", GameType));
                    DataArchives.Add("data4", new EblArchive("data4", "data4.bhd", "data4.bdt", GameType));
                    DataArchives.Add("data5", new EblArchive("data5", "data5.bhd", "data5.bdt", GameType));
                    DataArchives.Add("dlc1", new EblArchive("dlc1", "dlc1.bhd", "dlc1.bdt", GameType));
                    DataArchives.Add("dlc2", new EblArchive("dlc2", "dlc2.bhd", "dlc2.bdt", GameType));
                }
                else if (GameType == SoulsGames.SDT)
                {
                    DataArchives.Add("data1", new EblArchive("data1", "data1.bhd", "data1.bdt", GameType));
                    DataArchives.Add("data2", new EblArchive("data2", "data2.bhd", "data2.bdt", GameType));
                    DataArchives.Add("data3", new EblArchive("data3", "data3.bhd", "data3.bdt", GameType));
                    DataArchives.Add("data4", new EblArchive("data4", "data4.bhd", "data4.bdt", GameType));
                    DataArchives.Add("data5", new EblArchive("data5", "data5.bhd", "data5.bdt", GameType));
                }
                else if (GameType == SoulsGames.ER)
                {
                    DataArchives.Add("data0", new EblArchive("data0", "data0.bhd", "data0.bdt", GameType));
                    DataArchives.Add("data1", new EblArchive("data1", "data1.bhd", "data1.bdt", GameType));
                    DataArchives.Add("data2", new EblArchive("data2", "data2.bhd", "data2.bdt", GameType));
                    DataArchives.Add("data3", new EblArchive("data3", "data3.bhd", "data3.bdt", GameType));
                }
                else if (GameType == SoulsGames.DS2SOTFS)
                {
                    DataArchives.Add("GameDataEbl", new EblArchive("GameDataEbl", "GameDataEbl.bhd", "GameDataEbl.bdt", GameType));
                }
            }

            InitBhdCacheInfo();
        }

        public static ulong GetFilePathHash(SoulsGames game, string path)
        {
            string hashable = path.Trim().Replace('\\', '/').ToLowerInvariant();
            if (!hashable.StartsWith("/"))
                hashable = '/' + hashable;
            return game == SoulsGames.ER ? hashable.Aggregate(0u, (ulong i, char c) => i * 0x85ul + c) : 
                hashable.Aggregate(0u, (uint i, char c) => i * 37u + c);
        }

        public class EblArchive
        {
            public string ShortName;
            public SoulsGames Game = SoulsGames.None;
            public string BhdName;
            public string BdtName;
            public BHD5 DecryptedBhdFile;
            public bool UseCache = true;

            private byte[] RetrieveFileFromBDT(long offset, long size)
            {
                byte[] result = new byte[size];
                using (var stream = File.OpenRead(BdtName))
                {
                    var br = new BinaryReaderEx(false, stream);
                    result = br.GetBytes(offset, (int)size);
                }
                return result;
            }

            private static AesManaged AES = new AesManaged() { Mode = CipherMode.ECB, Padding = PaddingMode.None, KeySize = 128 };

            public byte[] RetrieveFile(string relativePath)
            {
                if (DecryptedBhdFile == null)
                    InitBHD();

                if (DecryptedBhdFile == null)
                    return null;

                foreach (var b in DecryptedBhdFile.Buckets)
                {
                    foreach (var f in b)
                    {
                        if (f.FileNameHash == GetFilePathHash(Game, relativePath))
                        {
                            byte[] fileBytes = RetrieveFileFromBDT(f.FileOffset, f.PaddedFileSize);
                            byte[] decryptedFileBytes = fileBytes.ToArray();
                            // File in bdt is encrypted
                            if (f.AESKey != null)
                            {
                                using (ICryptoTransform decryptor = AES.CreateDecryptor(f.AESKey.Key, new byte[16]))
                                {
                                    foreach (SoulsFormats.BHD5.Range range in f.AESKey.Ranges.Where(r => r.StartOffset != -1 && r.EndOffset != -1 && r.StartOffset != r.EndOffset))
                                    {
                                        int start = (int)range.StartOffset;
                                        int count = (int)(range.EndOffset - range.StartOffset);
                                        decryptor.TransformBlock(decryptedFileBytes, start, count, decryptedFileBytes, start);
                                    }
                                }
                            }
                            return decryptedFileBytes;
                        }
                    }
                }
                return null;
            }

            public void InitBHD()
            {
                if (!File.Exists(BhdName) || !File.Exists(BdtName))
                    return;

                BHD5.Game bhd5Game = BHD5.Game.DarkSouls1;
                byte[] decryptedBhdBytes = null;

                string rsa = null;
                if (Game == SoulsGames.DS3)
                {
                    rsa = BhdRsaKeys.DarkSouls3Keys[ShortName];
                    bhd5Game = BHD5.Game.DarkSouls3;
                }
                else if (Game == SoulsGames.SDT)
                {
                    rsa = BhdRsaKeys.SekiroKeys[ShortName];
                    bhd5Game = BHD5.Game.Sekiro;
                }
                else if (Game == SoulsGames.ER)
                {
                    rsa = BhdRsaKeys.EldenRingKeys[ShortName];
                    bhd5Game = BHD5.Game.EldenRing;
                }
                else if (Game == SoulsGames.DS2)
                {
                    rsa = File.ReadAllText(BhdName.Replace("ebl.bhd", "keycode.pem"));
                    bhd5Game = BHD5.Game.DarkSouls2;
                }

                var cacheIsValid = (Game == SoulsGames.DS1) || CheckIfBhdCacheIsValid(ShortName);

                string decryptedBhdCachePath = $@"{CachePath}\{GameType}\{ShortName}.bhd";
                var decryptedBhdDir = Path.GetDirectoryName(decryptedBhdCachePath);
                
                if (File.Exists(decryptedBhdCachePath) && UseCache && cacheIsValid)
                {
                    decryptedBhdBytes = File.ReadAllBytes(decryptedBhdCachePath);
                }
                else
                {
                    if (Game != SoulsGames.DS1)
                    {
                        if (!Directory.Exists(decryptedBhdDir))
                            Directory.CreateDirectory(decryptedBhdDir);
                        if (File.Exists(decryptedBhdCachePath))
                            File.Delete(decryptedBhdCachePath);

                        using (var rsaDecryptedStream = CryptographyUtility.DecryptRsa(BhdName, rsa))
                        {
                            decryptedBhdBytes = rsaDecryptedStream.ToArray();
                        }
                        if (UseCache)
                        {
                            File.WriteAllBytes(decryptedBhdCachePath, decryptedBhdBytes);
                            RegistBhdCacheEntry(ShortName, BhdName);
                        }
                    }
                    else
                    {
                        decryptedBhdBytes = File.ReadAllBytes(BhdName);
                    }

                    
                }
                

                using (var bhd5Stream = new MemoryStream(decryptedBhdBytes))
                {
                    DecryptedBhdFile = BHD5.Read(bhd5Stream, bhd5Game);
                }
            }

            public EblArchive(string shortEblName, string bhdPath, string bdtPath, SoulsGames game)
            {
                Game = game;
                BhdName = $"{InterrootPath}\\{bhdPath.ToLower()}";
                BdtName = $"{InterrootPath}\\{bdtPath.ToLower()}";
                ShortName = shortEblName;



            }
        }

        public class GameDataCache
        {
            private Dictionary<string, byte[]> bdtFileCache = new Dictionary<string, byte[]>();
            private Dictionary<string, byte[]> decryptedBhdCache = new Dictionary<string, byte[]>();
        }

        private static byte[] ReadFileFromEBLs_NoCache(string relativePath)
        {
            byte[] result = null;
            relativePath = relativePath.ToLower().Trim().Replace("\\", "/");
            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;
            // "/x/y" --> "x"
            int slashIndex = relativePath.Substring(1).IndexOf('/');
            string rootFolder = (slashIndex >= 0) ? relativePath.Substring(1, slashIndex) : relativePath;
            var predictedEbl = PredictEblOfRootFolder(rootFolder);
            if (predictedEbl != null)
            {
                lock (_lock_DataArchives)
                {
                    // Check in case it was cleared before getting this lock.
                    if (DataArchives.ContainsKey(predictedEbl))
                    {
                        var predictedEbl_FullPath = $"{InterrootPath}\\{predictedEbl}.bdt";
                        Log($"(INFO) Asset '{relativePath}' is most likely in EBL `{predictedEbl_FullPath}`, checking it first...");
                        result = DataArchives[predictedEbl].RetrieveFile(relativePath);
                        if (result != null)
                            Log($"(SUCCESS) Loaded asset '{relativePath}' from EBL '{predictedEbl_FullPath}'.");
                        else
                            Log($"(FILE NOT FOUND IN EBL) Did not find asset '{relativePath}' inside EBL '{predictedEbl_FullPath}'.");
                    }
                }
            }
            if (result != null)
                return result;
            foreach (var kvp in DataArchives)
            {
                if (kvp.Key != predictedEbl)
                {
                    var eblFullPath = $"{InterrootPath}\\{kvp.Key}.bdt";
                    result = kvp.Value.RetrieveFile(relativePath);
                    if (result != null)
                    {
                        Log($"(SUCCESS) Loaded asset '{relativePath}' from EBL '{eblFullPath}'.");
                        return result;
                    }
                    else
                    {
                        Log($"(FILE NOT FOUND IN EBL) Did not find asset '{relativePath}' inside EBL '{eblFullPath}'.");
                    }
                }
            }
            return null;
        }

        public static bool FileExists(string relativePath, bool alwaysLoose = false)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return false;
            // Unoptimized but effective: Just read it and see if there's a result lmao
            // Disable warning on fail.
            var file = ReadFile(relativePath, alwaysLoose, warningOnFail: false);
            return file != null;
        }

        public static void Log(string message)
        {
            if (Main.Config.EnableGameDataIOLogging)
            {
                ErrorLog.Log("GAME DATA IO", message);
            }
        }

        private static byte[] ReadFileDirect(string relativePath, bool alwaysLoose)
        {
            if (!string.IsNullOrWhiteSpace(ModengineRootPath))
            {

                string modEngineFilePath = Path.GetFullPath($@"{ModengineRootPath}{relativePath.Replace("/", "\\")}");

                if (File.Exists(modEngineFilePath))
                {
                    Log($"(SUCCESS) Tried to read asset '{relativePath}' from ModEngine directory: '{modEngineFilePath}'.");
                    return File.ReadAllBytes(modEngineFilePath);
                }
                else
                {
                    Log($"(FILE NOT FOUND) Tried to read asset '{relativePath}' from ModEngine Directory: '{modEngineFilePath}'.");
                }
            }

            bool loadLoose = IsLoadingUnpackedFiles || alwaysLoose;

            string interrootFilePath = Path.GetFullPath($@"{InterrootPath}{relativePath.Replace("/", "\\")}");
            if (loadLoose)
            {
                if (File.Exists(interrootFilePath))
                {
                    Log($"(SUCCESS) Tried to read asset '{relativePath}' from Game Data Directory: '{interrootFilePath}'.");
                    return File.ReadAllBytes(interrootFilePath);
                }
                else
                {
                    Log($"(FILE NOT FOUND) Tried to read asset '{relativePath}' from Game Data Directory: '{interrootFilePath}'.");
                    return null;
                }
            }
            else
            {
                return ReadFileFromEBLs(relativePath);
            }
        }

        public static bool SoundFileExists(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return false;
            // Unoptimized but effective: Just read it and see if there's a result lmao
            // Disable warning on fail.
            var file = ReadSoundFile(relativePath);
            return file != null;
        }

        public static string ShowPickInsideBndPath(string folder, string searchRegex, string defaultOptionMatchStart, string title, string exactMatchDefault)
        {
            var possibleFiles = GameData.SearchFiles(folder, searchRegex);
            if (possibleFiles.Count == 0)
                return null;
            var picker = new TaeEditor.TaeLoadFromArchivesFilePicker();
            picker.Text = title;
            picker.StartInCenterOf(Main.WinForm);
            picker.InitEblFileList(possibleFiles, defaultOptionMatchStart, exactMatchDefault);
            var result = picker.ShowDialog();
            return result == System.Windows.Forms.DialogResult.OK ? picker.SelectedEblFile : null;
        }

        public static byte[] ReadSoundFile(string relativePath)
        {
            byte[] result = null;
            lock (_lock_DataArchives)
            {
                if (EldenRingSoundArchive == null)
                    EldenRingSoundArchive = new EblArchive("sd\\sd", "sd\\sd.bhd", "sd\\sd.bdt", SoulsGames.ER);

                result = EldenRingSoundArchive.RetrieveFile(relativePath);
            }

            return result;
        }

        public static bool StreamedWEMExists(uint wemID)
        {
            string wemNameStart = wemID.ToString().Substring(0, 2);
            return SoundFileExists($"/wem/{wemNameStart}/{wemID}.wem") || SoundFileExists($"/enus/wem/{wemNameStart}/{wemID}.wem");
        }

        public static byte[] ReadStreamedWEM(uint wemID)
        {
            string wemNameStart = wemID.ToString().Substring(0, 2);
            return ReadSoundFile($"/wem/{wemNameStart}/{wemID}.wem") ?? ReadSoundFile($"/enus/wem/{wemNameStart}/{wemID}.wem");
        }

        public static IBinder TryLoadBinder(string binderRelPath, bool alwaysLoose = false, bool warningOnFail = true)
        {
            IBinder result = null;
            var file = ReadFile(binderRelPath, alwaysLoose, warningOnFail);
            if (file != null)
            {
                if (BND3.IsRead(file, out var bnd3))
                    result = bnd3;
                else if (BND4.IsRead(file, out var bnd4))
                    result = bnd4;
            }
            return result;
        }

        public static bool CheckUnDcxFiles => GameRoot.GameType is SoulsGames.DS1 or SoulsGames.DES;

        public static byte[] ReadFile(string relativePath, bool alwaysLoose = false, bool warningOnFail = true)
        {
            relativePath = relativePath.ToLower().Trim().Replace("\\", "/");
            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;
            var file = ReadFileDirect(relativePath, alwaysLoose);
            if (file == null && CheckUnDcxFiles && relativePath.EndsWith(".dcx"))
                file = ReadFileDirect(relativePath.Substring(0, relativePath.Length - 4), alwaysLoose);

            if (warningOnFail && file == null)
            {
                NotificationManager.PushNotification($"Unable to load game asset '{relativePath}'.", color: Microsoft.Xna.Framework.Color.Orange);
            }

            return file;
        }

        public static TempEblUnpackFile ReadFileAndTempUnpack(string relativePath)
        {
            relativePath = relativePath.ToLower().Trim();
            TempEblUnpackFile file = null;
            lock (_lock_EblInnerFileCache)
            {
                var fileBytes = ReadFile(relativePath);
                if (!TempUnpackedEblFiles.Any(x => x.EblPath.ToLower() == relativePath.ToLower()))
                {
                    
                    if (fileBytes != null)
                    {
                        file = new TempEblUnpackFile()
                        {
                            EblPath = relativePath,
                            FileBytes = fileBytes,
                        };
                        var dir = Path.GetDirectoryName(file.AbsolutePath);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        File.WriteAllBytes(file.AbsolutePath, fileBytes);
                        TempUnpackedEblFiles.Add(file);
                    }
                }
                else
                {
                    file = TempUnpackedEblFiles.FirstOrDefault(x => x.EblPath.ToLower() == relativePath.ToLower());
                    if (fileBytes == null)
                    {
                        TempUnpackedEblFiles.Remove(file);
                    }
                    else
                    {
                        file.FileBytes = fileBytes;
                        var dir = Path.GetDirectoryName(file.AbsolutePath);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        File.WriteAllBytes(file.AbsolutePath, fileBytes);
                    }
                    
                }
            }
            return file;
        }

        public static byte[] ReadFileFromEBLs(string relativePath)
        {
            byte[] file = null;
            lock (_lock_EblInnerFileCache)
            {
                if (EblInnerFileCache.ContainsKey(relativePath))
                {
                    Log($"(SUCCESS) Loaded asset from EBL cache - '{relativePath}'");
                    file = EblInnerFileCache[relativePath];
                }
                else
                {
                    file = ReadFileFromEBLs_NoCache(relativePath);
                    if (file != null)
                    {
                        Log($"(SUCCESS) Added asset to EBL cache - '{relativePath}'");
                        EblInnerFileCache.Add(relativePath, file);
                    }
                }
            }

            return file;
        }

        public static string PredictEblOfRootFolder(string rootFolder)
        {
            string result = null;
            lock (_lock_DataArchives)
            {
                rootFolder = rootFolder.Trim().ToLower();
                switch (GameType)
                {
                    case SoulsGames.DS3:
                    case SoulsGames.SDT:
                        if (_rootFolderToEblMap_DS3_SDT.ContainsKey(rootFolder))
                            result = _rootFolderToEblMap_DS3_SDT[rootFolder];
                        break;
                    case SoulsGames.ER:
                        if (_rootFolderToEblMap_ER.ContainsKey(rootFolder))
                            result = _rootFolderToEblMap_ER[rootFolder];
                        break;
                }
            }
            return result;
        }

        private static Dictionary<string, string> _rootFolderToEblMap_DS3_SDT = new Dictionary<string, string>
        {
            { "event", "data1" },
            { "facegen", "data1" },
            { "font", "data1" },
            { "menu", "data1" },
            { "movie", "data1" },
            { "msg", "data1" },
            { "mtd", "data1" },
            { "other", "data1" },
            { "param", "data1" },
            { "paramdef", "data1" },
            { "stayparamdef", "data1" },
            { "remo", "data1" },
            { "script", "data1" },
            { "sfx", "data1" },
            { "shader", "data1" },
            { "sound", "data1" },
            { "testdata", "data1" },

            { "cutscene", "data1" },

            { "parts", "data2" },

            { "action", "data3" },
            { "chr", "data3" },

            { "obj", "data4" },

            { "map", "data5" },
        };

        private static Dictionary<string, string> _rootFolderToEblMap_ER = new Dictionary<string, string>
        {
            { "action", "data0" },
            { "cutscene", "data0" },
            { "event", "data0" },
            { "expression", "data0" },
            { "facegen", "data0" },
            { "font", "data0" },
            { "material", "data0" },
            { "menu", "data0" },
            { "movie", "data0" },
            { "msg", "data0" },
            { "obj", "data0" },
            { "other", "data0" },
            { "param", "data0" },
            { "parts", "data0" },
            { "script", "data0" },
            { "script_interroot", "data0" },
            { "shader", "data0" },
            { "sound", "data0" },
            { "sfx", "data0" },

            { "asset", "data1" },

            { "map", "data2" },

            { "chr", "data3" },
        };
    }
}
