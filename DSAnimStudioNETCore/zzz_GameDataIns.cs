using SoulsAssetPipeline;
using SoulsAssetPipeline.Animation;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class zzz_GameDataIns
    {
        public zzz_DocumentIns ParentDocument;
        public zzz_GameDataIns(zzz_DocumentIns parentDocument)
        {
            ParentDocument = parentDocument;
        }

        public object _lock_ProjectJson = new object();
        private TaeEditor.TaeProjectJson ProjectJson;

        public static bool EnableFileCaching = true;

        public void ProjectJsonLockAct(Action<TaeEditor.TaeProjectJson> act)
        {
            lock (_lock_ProjectJson)
            {
                act?.Invoke(ProjectJson);
            }
        }

        public bool LoadProjectJson()
        {
            var result = false;
            lock (_lock_ProjectJson)
            {
                if (File.Exists(ProjectPath))
                {
                    ProjectJson = TaeEditor.TaeProjectJson.Load(ProjectPath, ParentDocument);
                    if (ProjectJson.WorldViews == null)
                        ProjectJson.WorldViews = new List<WorldView>();
                    if (ProjectJson.WorldViews.Count == 0)
                        ProjectJson.WorldViews.Add(new WorldView("Default"));
                    ParentDocument.WorldViewManager.SetWorldViewList(ProjectJson.WorldViews);

                    zzz_NotificationManagerIns.PushNotification($"Loaded workspace config from '{System.IO.Path.GetFullPath(ProjectPath)}'.");
                    result = true;
                }
                else
                {
                    ProjectJson = new TaeEditor.TaeProjectJson()
                    {
                        GameDirectory = ParentDocument.GameRoot.DefaultGameDir,
                        ModEngineDirectory = "",
                    };

                    ProjectJson.InitDefaults(ParentDocument);
                    ProjectJson.Save(ProjectPath, ParentDocument);

                    zzz_NotificationManagerIns.PushNotification($"Saved default project config to '{System.IO.Path.GetFullPath(ProjectPath)}'.");
                    result = false;
                }
            }

            return result;
        }

        public void SaveProjectJson(bool silent = false)
        {
            ProjectJsonLockAct(proj =>
            {
                if (proj == null)
                {
                    ProjectJson = new TaeEditor.TaeProjectJson()
                    {
                        GameDirectory = ParentDocument.GameRoot.DefaultGameDir,
                        ModEngineDirectory = "",
                    };
                }

                //proj.WorldViews = WorldViewManager.GetWorldViewList();



                proj.Save(ProjectPath, ParentDocument);

            });
            //if (!silent)
            //    NotificationManager.PushNotification($"Saved project config to '{System.IO.Path.GetFullPath(ProjectPath)}'.");
        }

        public class BhdCacheInfo
        {
            public class BhdCacheInfoEntry
            {
                public string BhdName { get; set; }
                public byte[] First0x40 { get; set; } = new byte[0x40];

                public bool HasWwiseFnvHash = false;
                public uint WwiseFnvHash = 0;
                public bool HasLastModifiedTime = false;
                public long LastModifiedTime = 0;
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

            public void RegistBHD(zzz_DocumentIns parentDocument, string bhdName, string bhdFilePath)
            {
                var removeEntries = Entries.Where(e => e.BhdName == bhdName).ToList();
                foreach (var e in removeEntries)
                    Entries.Remove(e);

                var bhdFile = File.ReadAllBytes(bhdFilePath);
                var entry = new BhdCacheInfoEntry();
                entry.BhdName = bhdName;
                var md5 = MD5.Create();
                entry.First0x40 = bhdFile.Take(0x40).ToArray();
                entry.LastModifiedTime = new FileInfo(bhdFilePath).LastWriteTimeUtc.Ticks;
                entry.HasLastModifiedTime = true;
                entry.WwiseFnvHash = parentDocument.SoundManager.GetFnvHashOfBytes(bhdFile);
                entry.HasWwiseFnvHash = true;
                Entries.Add(entry);

                zzz_NotificationManagerIns.PushNotification($"New BHD info entry created for '{bhdName}'.");
            }
        }

        public string InterrootPath => ParentDocument.GameRoot.InterrootPath;
        public string ModengineRootPath => ParentDocument.GameRoot.InterrootModenginePath;
        public string TempEblUnpackPath => ParentDocument.GameRoot.TempEblUnpackPath;
        public string CachePath => ParentDocument.GameRoot.CachePath;
        public string CacheInfoPath => $"{ParentDocument.GameRoot.CachePath}\\_CACHE_INFO.json";
        public string ProjectPath => ParentDocument.GameRoot.ProjectPath;

        private BhdCacheInfo CacheInfo = null;
        public void InitBhdCacheInfo()
        {
            lock (_lock_DataArchives)
            {
                CacheInfo = new BhdCacheInfo();
                if (!string.IsNullOrEmpty(ParentDocument.GameRoot.CachePath))
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
                        zzz_NotificationManagerIns.PushNotification("New BHD cache info file created.");
                    }
                }
            }
        }

        public void SaveBhdCacheInfo()
        {
            lock (_lock_DataArchives)
            {
                if (!string.IsNullOrEmpty(ParentDocument.GameRoot.CachePath))
                {
                    CacheInfo.WriteJson(CacheInfoPath);
                    zzz_NotificationManagerIns.PushNotification($"BHD info entries saved.");
                }
            }
        }

        public void RegistBhdCacheEntry(string bhdName, string bhdPath)
        {
            lock (_lock_DataArchives)
            {
                if (!string.IsNullOrEmpty(ParentDocument.GameRoot.CachePath))
                {
                    CacheInfo.RegistBHD(ParentDocument, bhdName, bhdPath);
                    CacheInfo.WriteJson(CacheInfoPath);
                }
            }
        }

        public bool CheckIfBhdCacheIsValid(string bhdName)
        {
            var bhdPath = Path.GetFullPath($"{InterrootPath}\\{bhdName}.bhd");
            var md5 = MD5.Create();

            byte[] bhdFileBytes = File.ReadAllBytes(bhdPath);

            //var bhdBytes = File.ReadAllBytes(bhdPath);
            //var bhdMD5 = md5.ComputeHash(bhdBytes);
            bool result = false;
            lock (_lock_DataArchives)
            {
                if (!string.IsNullOrEmpty(ParentDocument.GameRoot.CachePath))
                {
                    foreach (var entry in CacheInfo.Entries)
                    {
                        if (entry.BhdName.ToLowerInvariant() == bhdName.ToLowerInvariant())
                        {

                            if (entry.HasLastModifiedTime)
                            {
                                var modifiedTime = new FileInfo(bhdPath).LastWriteTimeUtc.Ticks;
                                if (modifiedTime > entry.LastModifiedTime)
                                {
                                    result = false;
                                    break;
                                }
                            }

                            if (entry.First0x40 != null && bhdFileBytes.Length >= entry.First0x40.Length)
                            {
                                var bhdFirst0x40 = bhdFileBytes.Take(40).ToArray();
                                if (!entry.First0x40.SequenceEqual(bhdFirst0x40))
                                {
                                    result = false;
                                    break;
                                }
                            }

                            if (entry.HasWwiseFnvHash)
                            {
                                var fnvHash = ParentDocument.SoundManager.GetFnvHashOfBytes(bhdFileBytes);
                                if (fnvHash == entry.WwiseFnvHash)
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public bool CfgLoadLooseParams = false;
        public bool CfgLoadUnpackedGameFiles = false;

        public bool IsLoadingRegulationParams
        {
            get
            {
                if (ParentDocument.GameRoot.GameType is SoulsGames.DES or SoulsGames.DS1 or SoulsGames.DS1R or SoulsGames.SDT)
                    return false;
                else
                    return !CfgLoadLooseParams;
            }
        }

        public bool IsLoadingUnpackedFiles => CfgLoadUnpackedGameFiles ||
            (ParentDocument.GameRoot.GameType is SoulsGames.DS1R or SoulsGames.DES or SoulsGames.BB or SoulsGames.None);

        public SoulsGames GameType => ParentDocument.GameRoot.GameType;
        public Dictionary<string, EblArchive> DataArchives = new Dictionary<string, EblArchive>();
        private EblArchive WwiseSoundArchive = null;
        private EblArchive WwiseSoundArchive_dlc = null;
        public List<string> UxmDictionary = new List<string>();

        private object _lock_DataArchives = new object();

        private object _lock_readFileCache = new object();
        private Dictionary<string, byte[]> _readFileCache = new Dictionary<string, byte[]>();

        public void ClearReadFileCache()
        {
            lock (_lock_readFileCache)
            {
                _readFileCache.Clear();
            }
        }

        public bool IsFileInCache(string path)
        {
            bool result = false;
            lock (_lock_readFileCache)
            {
                result = _readFileCache.ContainsKey(path);
            }
            return result;
        }

        public byte[] ReadFileFromCache(string path)
        {
            path = path.Trim().ToLower();
            byte[] result = null;
            lock (_lock_readFileCache)
            {
                if (_readFileCache.ContainsKey(path))
                    result = _readFileCache[path];
            }
            return result;
        }

        public void AddFileToCache(string path, byte[] file)
        {
            lock (_lock_readFileCache)
            {
                _readFileCache[path] = file;
            }
        }

        public class TempEblUnpackFile : IDisposable
        {
            public readonly zzz_GameDataIns OwnerGameData;
            public TempEblUnpackFile(zzz_GameDataIns owner)
            {
                OwnerGameData = owner;
            }

            public string EblPath;
            public string AbsolutePath => $@"{OwnerGameData.ParentDocument.GameRoot.TempEblUnpackPath}\{EblPath.Replace("/", "\\")}";
            public byte[] FileBytes;
            public void Dispose()
            {
                if (File.Exists(AbsolutePath))
                    File.Delete(AbsolutePath);
            }
        }

        private object _lock_tempUnpackedEblFiles = new object();
        private List<TempEblUnpackFile> tempUnpackedEblFiles = new List<TempEblUnpackFile>();

        public string FindFileInGameData(string filePath)
        {
            if (DisableInterrootDCX && filePath.EndsWith(".dcx"))
                filePath = filePath.Substring(0, filePath.Length - 4);

            var justFileName = Path.GetFileName(filePath).ToLowerInvariant();
            var allFilesInDir = GetFilesInDir(null, SearchType.AllFiles);

            var result = allFilesInDir
                .Where(f => f.ToLowerInvariant().EndsWith(justFileName.ToLowerInvariant()))
                .FirstOrDefault();

            return result;
        }

        private string AddFakeDcxIfNeededInDS1(string val)
        {
            if (DisableInterrootDCX && !val.ToLower().EndsWith(".dcx"))
                val += ".dcx";

            return val.ToLower();
        }

        public List<string> SearchFiles(string directoryPath, string matchRegex)
        {
            if (ParentDocument.GameData.GameType == SoulsGames.DS1)
            {
                var looseFiles = GetFilesInDir(directoryPath, SearchType.LooseOnly)
                    .Where(x => System.Text.RegularExpressions.Regex.IsMatch(AddFakeDcxIfNeededInDS1(x), matchRegex, System.Text.RegularExpressions.RegexOptions.CultureInvariant)).ToList();

                var eblFiles = GetFilesInDir(directoryPath, SearchType.EblOnly)
                    .Where(x => System.Text.RegularExpressions.Regex.IsMatch(AddFakeDcxIfNeededInDS1(x), matchRegex, System.Text.RegularExpressions.RegexOptions.CultureInvariant)).ToList();

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



        public bool IsReadBinder(string relativePath, out IBinder binder, bool alwaysLoose = false, bool warningOnFail = true, bool disableCache = false)
        {
            binder = ReadBinder(relativePath, alwaysLoose, warningOnFail, disableCache);
            return binder != null;
        }

        public IBinder ReadBinder(string relativePath, bool alwaysLoose = false, bool warningOnFail = true, bool disableCache = false)
        {
            try
            {
                var fileBytes = ReadFile(relativePath, alwaysLoose, warningOnFail, disableCache);
                if (fileBytes != null)
                {
                    DCX.Type compressionType = DCX.Type.None;
                    if (DCX.Is(fileBytes))
                        fileBytes = DCX.Decompress(fileBytes, out compressionType);

                    if (BND3.IsRead(fileBytes, out BND3 asBND3))
                    {
                        asBND3.Compression = compressionType;
                        return asBND3;
                    }
                    else if (BND4.IsRead(fileBytes, out BND4 asBND4))
                    {
                        asBND4.Compression = compressionType;
                        return asBND4;
                    }
                }
                return null;
            }
            catch (SoulsFormats.Exceptions.NoOodleFoundException)
            {
                //SoulsFormatsNEXT TODO
                return null;
            }
        }

        public List<string> GetFilesInDir(string directoryPath, SearchType searchType = SearchType.AllFiles)
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

        private void LoadUxmDictionary()
        {
            string uxmDictName = $"{Main.Directory}\\Res\\UxmDict\\{GameType}.txt";
            if (File.Exists(uxmDictName))
                UxmDictionary = System.IO.File.ReadAllLines(uxmDictName).Select(x => x.TrimEnd('\r')).Where(x => !x.StartsWith("#")).ToList();
            else
                UxmDictionary = new List<string>();
        }

        public void ClearAll()
        {
            lock (_lock_DataArchives)
            {
                DataArchives.Clear();

            }
            ClearReadFileCache();
            lock (_lock_tempUnpackedEblFiles)
            {
                foreach (var x in tempUnpackedEblFiles)
                    x?.Dispose();
                tempUnpackedEblFiles?.Clear();
                if (Directory.Exists(ParentDocument.GameRoot.TempEblUnpackPath))
                {
                    Directory.Delete(ParentDocument.GameRoot.TempEblUnpackPath, true);
                }
            }
        }

        public void SaveCurrentEblHeaderCaches(string directory)
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

        public void ForceLoadEbl(string eblKey)
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

        public void SetCacheEnabled(bool useCache)
        {
            lock (_lock_DataArchives)
            {
                foreach (var d in DataArchives)
                {
                    d.Value.UseCache = useCache;
                }
            }
        }

        //public void LoadAllEblHeaders(bool useCache)
        //{
        //    lock (_lock_DataArchives)
        //    {
        //        foreach (var d in DataArchives)
        //        {
        //            d.Value.UseCache = false;
        //        }
        //    }
        //}

        public void InitEBLs()
        {
            ClearAll();

            LoadUxmDictionary();
            lock (_lock_DataArchives)
            {
                if (GameType == SoulsGames.DS1)
                {
                    DataArchives.Add("dvdbnd0", new EblArchive(this, "dvdbnd0", "dvdbnd0.bhd5", "dvdbnd0.bdt", GameType));
                    DataArchives.Add("dvdbnd1", new EblArchive(this, "dvdbnd1", "dvdbnd1.bhd5", "dvdbnd1.bdt", GameType));
                    DataArchives.Add("dvdbnd2", new EblArchive(this, "dvdbnd2", "dvdbnd2.bhd5", "dvdbnd2.bdt", GameType));
                    DataArchives.Add("dvdbnd3", new EblArchive(this, "dvdbnd3", "dvdbnd3.bhd5", "dvdbnd3.bdt", GameType));

                }
                else if (GameType == SoulsGames.DS3 || GameType == SoulsGames.SDT)
                {
                    DataArchives.Add("data1", new EblArchive(this, "data1", "data1.bhd", "data1.bdt", GameType));
                    DataArchives.Add("data2", new EblArchive(this, "data2", "data2.bhd", "data2.bdt", GameType));
                    DataArchives.Add("data3", new EblArchive(this, "data3", "data3.bhd", "data3.bdt", GameType));
                    DataArchives.Add("data4", new EblArchive(this, "data4", "data4.bhd", "data4.bdt", GameType));
                    DataArchives.Add("data5", new EblArchive(this, "data5", "data5.bhd", "data5.bdt", GameType));
                    if (GameType is SoulsGames.DS3)
                    {
                        DataArchives.Add("dlc1", new EblArchive(this, "dlc1", "dlc1.bhd", "dlc1.bdt", GameType));
                        DataArchives.Add("dlc2", new EblArchive(this, "dlc2", "dlc2.bhd", "dlc2.bdt", GameType));
                    }
                }
                else if (GameType is SoulsGames.ER or SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6)
                {
                    DataArchives.Add("data0", new EblArchive(this, "data0", "data0.bhd", "data0.bdt", GameType));
                    DataArchives.Add("data1", new EblArchive(this, "data1", "data1.bhd", "data1.bdt", GameType));
                    DataArchives.Add("data2", new EblArchive(this, "data2", "data2.bhd", "data2.bdt", GameType));
                    DataArchives.Add("data3", new EblArchive(this, "data3", "data3.bhd", "data3.bdt", GameType));
                    
                    // ERNR TODO - If DLC is ever added lol
                    if (GameType is SoulsGames.ER)
                    {
                        DataArchives.Add("dlc", new EblArchive(this, "dlc", "dlc.bhd", "dlc.bdt", GameType));
                    }
                }
                else if (GameType == SoulsGames.DS2SOTFS)
                {
                    DataArchives.Add("GameDataEbl", new EblArchive(this, "GameDataEbl", "GameDataEbl.bhd", "GameDataEbl.bdt", GameType));
                }
            }

            InitBhdCacheInfo();
        }

        public static ulong GetFilePathHash(SoulsGames game, string path)
        {
            string hashable = path.Trim().Replace('\\', '/').ToLowerInvariant();
            if (!hashable.StartsWith("/"))
                hashable = '/' + hashable;
            return (game is SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6) ? hashable.Aggregate(0u, (ulong i, char c) => i * 0x85ul + c) :
                hashable.Aggregate(0u, (uint i, char c) => i * 37u + c);
        }

        public class EblArchive
        {
            public bool Exists = true;

            public readonly zzz_GameDataIns OwnerGameData;
            public EblArchive(zzz_GameDataIns owner, string shortEblName, string bhdPath, string bdtPath, SoulsGames game)
            {
                OwnerGameData = owner;
                Game = game;
                BhdName = $"{OwnerGameData.ParentDocument.GameRoot.InterrootPath}\\{bhdPath.ToLower()}";
                BdtName = $"{OwnerGameData.ParentDocument.GameRoot.InterrootPath}\\{bdtPath.ToLower()}";
                ShortName = shortEblName;

                Exists = File.Exists(BhdName) && File.Exists(BdtName);
            }

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

            private AesManaged AES = new AesManaged() { Mode = CipherMode.ECB, Padding = PaddingMode.None, KeySize = 128 };

            public byte[] RetrieveFile(string relativePath)
            {
                if (DecryptedBhdFile == null)
                    InitBHD();

                if (DecryptedBhdFile == null)
                    return null;

                var fileNameHash = GetFilePathHash(Game, relativePath);

                foreach (var b in DecryptedBhdFile.Buckets)
                {
                    if (b.Count == 0)
                        continue;

                    //if (!(b[0].FileNameHash <= fileNameHash && b[b.Count - 1].FileNameHash >= fileNameHash))
                    //    continue;

                    foreach (var f in b)
                    {
                        if (f.FileNameHash == fileNameHash)
                        {
                            byte[] fileBytes = RetrieveFileFromBDT(f.FileOffset, f.PaddedFileSize);
                            byte[] decryptedFileBytes = fileBytes.ToArray();
                            // File in bdt is encrypted
                            if (f.AESKey != null)
                            {
                                using (var test = System.Security.Cryptography.Aes.Create())
                                {
                                    test.KeySize = 128;
                                    test.Key = f.AESKey.Key;
                                    foreach (SoulsFormats.BHD5.Range range in f.AESKey.Ranges.Where(r => r.StartOffset != -1 && r.EndOffset != -1 && r.StartOffset != r.EndOffset))
                                    {
                                        var dec = test.DecryptEcb(decryptedFileBytes[((int)range.StartOffset)..((int)range.EndOffset)], PaddingMode.None);
                                        Array.Copy(dec, 0, decryptedFileBytes, range.StartOffset, dec.Length);
                                    }
                                }


                                //using (ICryptoTransform decryptor = AES.CreateDecryptor(f.AESKey.Key, new byte[16]))
                                //{

                                //    foreach (SoulsFormats.BHD5.Range range in f.AESKey.Ranges.Where(r => r.StartOffset != -1 && r.EndOffset != -1 && r.StartOffset != r.EndOffset))
                                //    {





                                //        int start = (int)range.StartOffset;
                                //        int count = (int)(range.EndOffset - range.StartOffset);
                                //        decryptor.TransformBlock(decryptedFileBytes, start, count, decryptedFileBytes, start);
                                //    }
                                //}
                            }
                            return decryptedFileBytes;
                        }
                    }
                }
                return null;
            }

            public static string GetPemPath(string game, string bhd)
            {
                return $"{Main.Directory}\\Res\\PEM\\{game}\\{bhd}.pem";
            }



            public void InitBHD()
            {
                if (!File.Exists(BhdName) || !File.Exists(BdtName))
                    return;

                BHD5.Game bhd5Game = BHD5.Game.DarkSouls1;
                byte[] decryptedBhdBytes = null;

                string pem = null;
                if (Game == SoulsGames.DS3)
                {
                    pem = GetPemPath("DS3", ShortName);
                    bhd5Game = BHD5.Game.DarkSouls3;
                }
                else if (Game == SoulsGames.SDT)
                {
                    pem = GetPemPath("SDT", ShortName);
                    bhd5Game = BHD5.Game.DarkSouls3;
                }
                else if (Game == SoulsGames.ER)
                {
                    pem = GetPemPath("ER", ShortName);
                    bhd5Game = BHD5.Game.EldenRing;
                }
                else if (Game == SoulsGames.ERNR)
                {
                    pem = GetPemPath("ERNR", ShortName);
                    bhd5Game = BHD5.Game.EldenRing; // ERNR TODO - CHECK
                }
                else if (Game == SoulsGames.AC6)
                {
                    pem = GetPemPath("AC6", ShortName);
                    bhd5Game = BHD5.Game.EldenRing;
                }
                else if (Game == SoulsGames.DS2)
                {
                    pem = BhdName.ToLower().Replace("ebl.bhd", "keycode.pem");
                    bhd5Game = BHD5.Game.DarkSouls2;
                }

                var cacheIsValid = (Game == SoulsGames.DS1) || OwnerGameData.ParentDocument.GameData.CheckIfBhdCacheIsValid(ShortName);

                string decryptedBhdCachePath = $@"{OwnerGameData.ParentDocument.GameRoot.CachePath}\{ShortName}.bhd";
                var decryptedBhdDir = Path.GetDirectoryName(decryptedBhdCachePath);

                bool interrupted = false;

                if (File.Exists(decryptedBhdCachePath) && UseCache && cacheIsValid)
                {
                    decryptedBhdBytes = File.ReadAllBytes(decryptedBhdCachePath);
                }
                else
                {
                    if (Game != SoulsGames.DS1)
                    {
                        try
                        {
                            if (!Directory.Exists(decryptedBhdDir))
                                Directory.CreateDirectory(decryptedBhdDir);
                            //if (File.Exists(decryptedBhdCachePath))
                            //    File.Delete(decryptedBhdCachePath);

                            var pemCheck = File.ReadAllBytes(pem);
                            if (pemCheck[0] == 0xEF && pemCheck[1] == 0xBB && pemCheck[2] == 0xBF)
                            {
                                Console.WriteLine($"Removing stupid ass byte order mark Visual Studio added to PEM file '{pem}'...");
                                pemCheck = pemCheck.Skip(3).ToArray();
                                File.WriteAllBytes(pem, pemCheck);
                            }

                            Console.WriteLine($"Running bhdeez on '{BhdName}'...");

                            var procStart = new ProcessStartInfo($@"{Main.Directory}\Res\bhdeez\bhdeez.exe",
                    $"--bhd \"{BhdName}\" --pem \"{pem}\" --output \"{decryptedBhdCachePath}\"");
                            procStart.CreateNoWindow = true;
                            //procStart.WindowStyle = ProcessWindowStyle.Hidden;
                            //procStart.UseShellExecute = false;
                            //procStart.RedirectStandardOutput = true;
                            //procStart.RedirectStandardError = true;
                            procStart.WorkingDirectory = decryptedBhdDir;
                            var proc = new Process();
                            proc.StartInfo = procStart;
                            proc.Start();
                            proc.WaitForExit();

                            decryptedBhdBytes = File.ReadAllBytes(decryptedBhdCachePath);
                            if (UseCache)
                                OwnerGameData.ParentDocument.GameData.RegistBhdCacheEntry(ShortName, BhdName);
                        }
                        catch (System.Threading.ThreadInterruptedException)
                        {
                            interrupted = true;
                        }
                    }
                    else
                    {
                        decryptedBhdBytes = File.ReadAllBytes(BhdName);
                    }


                }

                if (!interrupted)
                {
                    using (var bhd5Stream = new MemoryStream(decryptedBhdBytes))
                    {
                        DecryptedBhdFile = BHD5.Read(bhd5Stream, bhd5Game);
                    }
                }
            }

            
        }

        private byte[] ReadFileFromEBLs_NoCache(string relativePath)
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

        public bool FileExists(string relativePath, bool alwaysLoose = false, bool disableCache = false)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return false;

            if (EnableFileCaching && !disableCache)
            {
                // If a file has been cached it's safe to assume it exists.
                if (IsFileInCache(relativePath))
                    return true;
            }

            // Unoptimized but effective: Just read it and see if there's a result lmao
            // Disable warning on fail.
            var file = ReadFile(relativePath, alwaysLoose, warningOnFail: false, disableCache: disableCache);
            return file != null;
        }

        public void Log(string message)
        {
            if (Main.Config.EnableGameDataIOLogging)
            {
                ErrorLog.Log("GAME DATA IO", message);
            }
        }

        private string GetLooseFilePathDirect(string relativePath)
        {
            if (!string.IsNullOrWhiteSpace(ModengineRootPath))
            {

                string modEngineFilePath = Path.GetFullPath($@"{ModengineRootPath}{relativePath.Replace("/", "\\")}");

                if (File.Exists(modEngineFilePath))
                {
                    Log($"(SUCCESS) Tried to read asset '{relativePath}' from ModEngine directory: '{modEngineFilePath}'.");
                    return modEngineFilePath;
                }
                else
                {
                    Log($"(FILE NOT FOUND) Tried to read asset '{relativePath}' from ModEngine Directory: '{modEngineFilePath}'.");
                }
            }

            string interrootFilePath = Path.GetFullPath($@"{InterrootPath}{relativePath.Replace("/", "\\")}");
            if (File.Exists(interrootFilePath))
            {
                Log($"(SUCCESS) Tried to read asset '{relativePath}' from Game Data Directory: '{interrootFilePath}'.");
                return interrootFilePath;
            }

            return null;
        }

        private DateTime? GetFileLastModifiedDirect(string relativePath)
        {
            if (!string.IsNullOrWhiteSpace(ModengineRootPath))
            {

                string modEngineFilePath = Path.GetFullPath($@"{ModengineRootPath}{relativePath.Replace("/", "\\")}");

                if (File.Exists(modEngineFilePath))
                {
                    Log($"(SUCCESS) Tried to read asset '{relativePath}' from ModEngine directory: '{modEngineFilePath}'.");
                    return File.GetLastWriteTime(modEngineFilePath);
                }
                else
                {
                    Log($"(FILE NOT FOUND) Tried to read asset '{relativePath}' from ModEngine Directory: '{modEngineFilePath}'.");
                }
            }

            string interrootFilePath = Path.GetFullPath($@"{InterrootPath}{relativePath.Replace("/", "\\")}");
            if (File.Exists(interrootFilePath))
            {
                Log($"(SUCCESS) Tried to read asset '{relativePath}' from Game Data Directory: '{interrootFilePath}'.");
                return File.GetLastWriteTime(interrootFilePath);
            }

            return null;
        }

        private byte[] ReadFileDirect(string relativePath, bool alwaysLoose, bool disableCache)
        {
            if (DisableInterrootDCX && relativePath.EndsWith(".dcx"))
                relativePath = relativePath.Substring(0, relativePath.Length - 4);

            if (EnableFileCaching && !disableCache)
            {
                var cachedFile = ReadFileFromCache(relativePath);

                if (cachedFile != null)
                {
                    return cachedFile;
                }
            }


            if (!string.IsNullOrWhiteSpace(ModengineRootPath))
            {

                string modEngineFilePath = Path.GetFullPath($@"{ModengineRootPath}{relativePath.Replace("/", "\\")}");

                if (File.Exists(modEngineFilePath))
                {
                    Log($"(SUCCESS) Tried to read asset '{relativePath}' from ModEngine directory: '{modEngineFilePath}'.");
                    var result = File.ReadAllBytes(modEngineFilePath);
                    if (result != null && EnableFileCaching && !disableCache)
                        AddFileToCache(relativePath, result);
                    return result;
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
                    var result = File.ReadAllBytes(interrootFilePath);
                    if (result != null && EnableFileCaching && !disableCache)
                        AddFileToCache(relativePath, result);
                    return result;
                }
                else
                {
                    // Should allow it to fallback to EBL if the file is not present in UXM files.
                    var result = ReadFileFromEBLs_NoCache(relativePath);
                    if (result != null && EnableFileCaching && !disableCache)
                        AddFileToCache(relativePath, result);
                    return result;
                    //Log($"(FILE NOT FOUND) Tried to read asset '{relativePath}' from Game Data Directory: '{interrootFilePath}'.");
                    //return null;
                }
            }
            else
            {
                var result = ReadFileFromEBLs_NoCache(relativePath);
                if (result != null && EnableFileCaching && !disableCache)
                    AddFileToCache(relativePath, result);
                return result;
            }
        }

        public bool WwiseSoundFileExists(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return false;
            // Unoptimized but effective: Just read it and see if there's a result lmao
            // Disable warning on fail.
            var file = ReadWwiseSoundFile(relativePath);
            return file != null;
        }



        public string ShowPickInsideBndPath(string folder, string searchRegex, string defaultOptionMatchStart, string title, string exactMatchDefault)
        {
            var possibleFiles = ParentDocument.GameData.SearchFiles(folder, searchRegex);
            if (possibleFiles.Count == 0)
                return null;
            var picker = new TaeEditor.TaeLoadFromArchivesFilePicker();
            picker.Text = title;
            picker.StartInCenterOf(Main.WinForm);
            picker.InitEblFileList(possibleFiles, defaultOptionMatchStart, exactMatchDefault);
            var result = picker.ShowDialog();
            return (result is System.Windows.Forms.DialogResult.OK or System.Windows.Forms.DialogResult.Yes) ? picker.SelectedEblFile : null;
        }

        public byte[] ReadWwiseSoundFile(string relativePath)
        {
            byte[] result = null;
            lock (_lock_DataArchives)
            {
                if (WwiseSoundArchive == null)
                {
                    if (GameType is SoulsGames.ER)
                    {
                        WwiseSoundArchive = new EblArchive(this, "sd\\sd", "sd\\sd.bhd", "sd\\sd.bdt", SoulsGames.ER);
                        WwiseSoundArchive_dlc = new EblArchive(this, "sd\\sd_dlc02", "sd\\sd_dlc02.bhd", "sd\\sd_dlc02.bdt", SoulsGames.ER);
                        if (!WwiseSoundArchive_dlc.Exists)
                            WwiseSoundArchive_dlc = null;
                    }
                    else if (GameType is SoulsGames.ERNR)
                    {
                        WwiseSoundArchive = new EblArchive(this, "sd\\sd", "sd\\sd.bhd", "sd\\sd.bdt", SoulsGames.ERNR);
                        // ERNR TODO - If they ever add DLC lol
                        //WwiseSoundArchive_dlc = new EblArchive(this, "sd\\sd_dlc02", "sd\\sd_dlc02.bhd", "sd\\sd_dlc02.bdt", SoulsGames.ER);
                        //if (!WwiseSoundArchive_dlc.Exists)
                        //    WwiseSoundArchive_dlc = null;
                    }
                    else if (GameType is SoulsGames.AC6)
                    {
                        WwiseSoundArchive = new EblArchive(this, "sd\\sd", "sd\\sd.bhd", "sd\\sd.bdt", SoulsGames.AC6);
                    }
                }

                if (WwiseSoundArchive_dlc != null)
                {
                    result = WwiseSoundArchive_dlc.RetrieveFile(relativePath);
                    if (result == null)
                        result = WwiseSoundArchive.RetrieveFile(relativePath);
                }
                else
                {
                    result = WwiseSoundArchive.RetrieveFile(relativePath);
                }

                
            }

            return result;
        }

        public bool StreamedWEMExists(uint wemID)
        {
            string wemNameStart = wemID.ToString().Substring(0, 2);
            return WwiseSoundFileExists($"/wem/{wemNameStart}/{wemID}.wem") || WwiseSoundFileExists($"/enus/wem/{wemNameStart}/{wemID}.wem");
        }

        public byte[] ReadStreamedWEM(uint wemID)
        {
            string wemNameStart = wemID.ToString().Substring(0, 2);
            return ReadWwiseSoundFile($"/wem/{wemNameStart}/{wemID}.wem") ?? ReadWwiseSoundFile($"/enus/wem/{wemNameStart}/{wemID}.wem");
        }

        public IBinder TryLoadBinder(string binderRelPath, bool alwaysLoose = false, bool warningOnFail = true)
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

        public bool DisableInterrootDCX = false;

        public string GetLooseFilePath(string relativePath, bool warningOnFail = true)
        {
            relativePath = relativePath.ToLower().Trim().Replace("\\", "/");
            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;
            if (DisableInterrootDCX && relativePath.EndsWith(".dcx"))
                relativePath = relativePath.Substring(0, relativePath.Length - 4);
            var looseFilePath = GetLooseFilePathDirect(relativePath);

            //ac6 test
            //if (file == null && !IsLoadingUnpackedFiles)
            //{
            //    file = ReadFileDirect(relativePath, true);
            //}

            if (warningOnFail && looseFilePath == null)
            {
                zzz_NotificationManagerIns.PushNotification($"Unable to load game asset '{relativePath}'.", color: Microsoft.Xna.Framework.Color.Orange);
            }

            return looseFilePath;
        }

        public DateTime? GetLooseFileLastModified(string relativePath, bool warningOnFail = true)
        {
            relativePath = relativePath.ToLower().Trim().Replace("\\", "/");
            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;
            if (DisableInterrootDCX && relativePath.EndsWith(".dcx"))
                relativePath = relativePath.Substring(0, relativePath.Length - 4);
            var lastAccess = GetFileLastModifiedDirect(relativePath);

            //ac6 test
            //if (file == null && !IsLoadingUnpackedFiles)
            //{
            //    file = ReadFileDirect(relativePath, true);
            //}

            if (warningOnFail && lastAccess == null)
            {
                zzz_NotificationManagerIns.PushNotification($"Unable to load game asset '{relativePath}'.", color: Microsoft.Xna.Framework.Color.Orange);
            }

            return lastAccess;
        }

        public bool IsReadFile(string relativePath, out byte[] file, bool alwaysLoose = false,
            bool warningOnFail = true, bool disableCache = false)
        {
            file = ReadFile(relativePath, alwaysLoose, warningOnFail, disableCache);
            return file != null;
        }

        public byte[] ReadFile(string relativePath, bool alwaysLoose = false, bool warningOnFail = true,
            bool disableCache = false)
        {
            relativePath = relativePath.ToLower().Trim().Replace("\\", "/");
            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;
            if (DisableInterrootDCX && relativePath.EndsWith(".dcx"))
                relativePath = relativePath.Substring(0, relativePath.Length - 4);
            var file = ReadFileDirect(relativePath, alwaysLoose, disableCache);

            //ac6 test
            //if (file == null && !IsLoadingUnpackedFiles)
            //{
            //    file = ReadFileDirect(relativePath, true);
            //}

            if (warningOnFail && file == null)
            {
                zzz_NotificationManagerIns.PushNotification($"Unable to load game asset '{relativePath}'.", color: Microsoft.Xna.Framework.Color.Orange);
            }

            return file;
        }

        public TempEblUnpackFile ReadFileAndTempUnpack(string relativePath, bool warningOnFail = true)
        {
            relativePath = relativePath.ToLower().Trim();
            TempEblUnpackFile file = null;
            lock (_lock_tempUnpackedEblFiles)
            {
                var fileBytes = ReadFile(relativePath, warningOnFail: warningOnFail);
                if (!tempUnpackedEblFiles.Any(x => x.EblPath.ToLower() == relativePath.ToLower()))
                {

                    if (fileBytes != null)
                    {
                        file = new TempEblUnpackFile(this)
                        {
                            EblPath = relativePath,
                            FileBytes = fileBytes,
                        };
                        var dir = Path.GetDirectoryName(file.AbsolutePath);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        File.WriteAllBytes(file.AbsolutePath, fileBytes);
                        tempUnpackedEblFiles.Add(file);
                    }
                }
                else
                {
                    file = tempUnpackedEblFiles.FirstOrDefault(x => x.EblPath.ToLower() == relativePath.ToLower());
                    if (fileBytes == null)
                    {
                        tempUnpackedEblFiles.Remove(file);
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

        //public byte[] ReadFileFromEBLs(string relativePath)
        //{
        //    byte[] file = null;
        //    lock (_lock_EblInnerFileCache)
        //    {
        //        if (EblInnerFileCache.ContainsKey(relativePath))
        //        {
        //            Log($"(SUCCESS) Loaded asset from EBL cache - '{relativePath}'");
        //            file = EblInnerFileCache[relativePath];
        //        }
        //        else
        //        {
        //            file = ReadFileFromEBLs_NoCache(relativePath);
        //            if (file != null)
        //            {
        //                Log($"(SUCCESS) Added asset to EBL cache - '{relativePath}'");
        //                EblInnerFileCache.Add(relativePath, file);
        //            }
        //        }
        //    }

        //    return file;
        //}

        public string PredictEblOfRootFolder(string rootFolder)
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
                    case SoulsGames.ERNR:
                    case SoulsGames.AC6:
                        if (_rootFolderToEblMap_ER.ContainsKey(rootFolder))
                            result = _rootFolderToEblMap_ER[rootFolder];
                        break;
                }
            }
            return result;
        }

        private Dictionary<string, string> _rootFolderToEblMap_DS3_SDT = new Dictionary<string, string>
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

        private Dictionary<string, string> _rootFolderToEblMap_ER = new Dictionary<string, string>
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
