using Microsoft.Xna.Framework.Graphics;
using SoulsAssetPipeline;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class zzz_TexturePoolIns
    {
        public zzz_DocumentIns ParentDocument;
        public zzz_TexturePoolIns(zzz_DocumentIns parentDocument)
        {
            ParentDocument = parentDocument;
        }



        public Dictionary<TextureFetchRequest.TextureInfo, Exception> Failures
    = new Dictionary<TextureFetchRequest.TextureInfo, Exception>();

        private object _lock_IO = new object();
        private object _lock_pool = new object();
        //This might be weird because it doesn't follow convention :fatcat:
        public delegate void TextureLoadErrorDelegate(string texName, string error);
        public event TextureLoadErrorDelegate OnLoadError;
        private void RaiseLoadError(string texName, string error)
        {
            OnLoadError?.Invoke(texName, error);
        }

        //private Dictionary<string, string> OnDemandTexturePaths = new Dictionary<string, string>();
        private Dictionary<string, TextureFetchRequest> fetches = new Dictionary<string, TextureFetchRequest>();

        public IReadOnlyDictionary<string, TextureFetchRequest> Fetches => fetches;

        public Dictionary<string, TextureFetchRequestDbgInfo> GetAllFetchesDbgInfos()
        {
            var result = new Dictionary<string, TextureFetchRequestDbgInfo>();
            lock (_lock_pool)
            {
                foreach (var kvp in fetches)
                    result.Add(kvp.Key, kvp.Value.GetDbgInfo());
            }
            return result;

        }

        public bool AnyTexturesMatch(Texture tex)
        {
            var result = false;
            lock (_lock_pool)
            {
                foreach (var kvp in fetches)
                {
                    if (kvp.Value.CachedTextureMatch(tex))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }


        public void RemoveFetchRefsWithoutFlushing()
        {
            fetches.Clear();
        }

        public void FlushDataOnly()
        {
            lock (_lock_pool)
            {
                foreach (var fetch in fetches)
                {
                    fetch.Value.FlushCachedTexture();
                }
            }
        }

        public void DestroyUnusedTextures()
        {
            lock (_lock_pool)
            {
                var fetchesToDestroy = new List<string>();
                foreach (var fetch in fetches)
                {
                    if (!fetch.Value.IsTextureLoaded)
                        fetchesToDestroy.Add(fetch.Key);
                }

                foreach (var fetch in fetchesToDestroy)
                {
                    var fetchObj = fetches[fetch];
                    fetches.Remove(fetch);
                    fetchObj?.Dispose();
                }
            }
        }

        public void Flush()
        {
            lock (_lock_pool)
            {
                foreach (var fetch in fetches)
                {
                    fetch.Value.Dispose();
                }
                fetches.Clear();
                Failures.Clear();
            }
            //if (ParentDocument.GameRoot.GameType != SoulsAssetPipeline.SoulsGames.None)
            //    ParentDocument.GameRoot.LoadSystex();
        }

        public void AddFetchTPF(TPF tpf, string texName)
        {
            string shortName = Utils.GetShortIngameFileName(texName).ToLower();
            if (!fetches.ContainsKey(shortName))
            {
                lock (_lock_pool)
                {
                    //if (tpf.Platform == TPF.TPFPlatform.PS3)
                    //{
                    //    tpf.ConvertPS3ToPC();
                    //}
                    //if (tpf.Platform == TPF.TPFPlatform.PS4)
                    //{
                    //    tpf.ConvertPS4ToPC();
                    //}
                    var newFetch = new TextureFetchRequest(tpf, texName.ToLower());
                    fetches.Add(shortName, newFetch);
                }
            }

        }

        public void AddFetchDDS(byte[] dds, string texName)
        {
            string shortName = Utils.GetShortIngameFileName(texName).ToLower();
            if (!fetches.ContainsKey(shortName))
            {
                lock (_lock_pool)
                {
                    //if (tpf.Platform == TPF.TPFPlatform.PS3)
                    //{
                    //    tpf.ConvertPS3ToPC();
                    //}
                    //if (tpf.Platform == TPF.TPFPlatform.PS4)
                    //{
                    //    tpf.ConvertPS4ToPC();
                    //}
                    var newFetch = new TextureFetchRequest(dds, texName.ToLower());
                    fetches.Add(shortName, newFetch);
                }
            }

        }

        public void AddAC6AET(int a, int b)
        {
            if (ParentDocument.GameRoot.GameType is SoulsGames.AC6)
            {
                string path = @$"/asset/environment/texture/aet{a:D3}_{b:D3}.tpf.dcx";
                var tpfBytes = ParentDocument.GameData.ReadFile(path);
                if (tpfBytes != null)
                    AddTpf(TPF.Read(tpfBytes));
            }
        }

        public void AddTpf(TPF tpf, IProgress<double> progress = null)
        {
            double i = 0;
            foreach (var tex in tpf.Textures)
            {
                AddFetchTPF(tpf, tex.Name.ToLower());
                progress?.Report(++i / tpf.Textures.Count);
            }
        }

        public void AddLooseDDSFolder(string folderPath, bool subDirectories = false)
        {
            var dds = Directory.GetFiles(folderPath, "*.dds", subDirectories
                ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (var d in dds)
            {
                string shortName = Utils.GetShortIngameFileName(d).ToLower();
                AddFetchDDS(File.ReadAllBytes(d), shortName);
            }
        }

        public void AddInterrootTPFFolder(string folderPath, bool dcx = false, bool directDdsFetches = false)
        {
            var tpfs = ParentDocument.GameRoot.GetInterrootFiles(folderPath, dcx ? "*.tpf.dcx" : "*.tpf");
            foreach (var t in tpfs)
            {
                string shortName = Utils.GetShortIngameFileName(t).ToLower();
                if (directDdsFetches)
                {
                    foreach (var tex in TPF.Read(t).Textures)
                    {
                        AddFetchDDS(tex.Bytes, tex.Name);
                    }
                }
                else
                {
                    AddFetchTPF(TPF.Read(File.ReadAllBytes(t)), shortName);
                }
            }
        }

        public void AddTextureBnd(IBinder bnd, IProgress<double> prog = null)
        {
            var tpfs = bnd.Files.Where(file => file.Name != null && (file.Name.ToLower().EndsWith(".tpf") || file.Name.ToLower().EndsWith(".tpf.dcx"))).ToList();
            var tbnds = bnd.Files.Where(file => file.Name != null && file.Name.ToLower().EndsWith(".tbnd")).ToList();

            double total = tpfs.Count + tbnds.Count;
            double tpfFraction = 0;
            double tbndFraction = 0;
            if (total > 0)
            {
                tpfFraction = tpfs.Count / total;
                tbndFraction = tbnds.Count / total;
            }

            for (int i = 0; i < tpfs.Count; i++)
            {
                var file = tpfs[i];
                if (file.Bytes.Length > 0)
                {
                    TPF tpf = TPF.Read(file.Bytes);
                    AddTpf(tpf);
                }

                prog?.Report(i / tpfFraction);
            }

            for (int i = 0; i < tbnds.Count; i++)
            {
                var file = tbnds[i];
                if (file.Bytes.Length > 0)
                {
                    IBinder tbnd = BND3.Read(file.Bytes);
                    for (int j = 0; j < tbnd.Files.Count; j++)
                    {
                        TPF tpf = TPF.Read(tbnd.Files[j].Bytes);
                        AddTpf(tpf);

                        prog?.Report(tpfFraction + i / tbndFraction + j / tbnd.Files.Count * (tbndFraction / tbnds.Count));
                    }
                }

                prog?.Report(tpfFraction + i / tbndFraction);
            }

            prog?.Report(1);
        }

        public void AddTpfFromPath(string path)
        {
            if (File.Exists(path))
            {
                TPF tpf = SoulsFormats.TPF.Read(path);
                AddTpf(tpf);
            }
        }

        public void AddTpfFromFile(byte[] file)
        {
            TPF tpf = SoulsFormats.TPF.Read(file);
            AddTpf(tpf);
        }

        public void AddTpfsFromFiles(List<byte[]> files, IProgress<double> progress)
        {
            var tpfList = new List<TPF>();
            int totalTexCount = 0;
            for (int i = 0; i < files.Count; i++)
            {
                if (files[i] != null)
                {
                    TPF tpf = SoulsFormats.TPF.Read(files[i]);
                    tpfList.Add(tpf);
                    totalTexCount += tpf.Textures.Count;
                }
            }
            double texIndex = 0;
            for (int i = 0; i < tpfList.Count; i++)
            {
                for (int j = 0; j < tpfList[i].Textures.Count; j++)
                {
                    AddFetchTPF(tpfList[i], Utils.GetShortIngameFileName(tpfList[i].Textures[j].Name).ToLower());
                    progress?.Report(++texIndex / totalTexCount);
                }
            }
            progress?.Report(1);
        }

        public void AddTpfsFromPaths(List<string> paths, IProgress<double> progress)
        {
            var tpfList = new List<TPF>();
            int totalTexCount = 0;
            for (int i = 0; i < paths.Count; i++)
            {
                if (File.Exists(paths[i]))
                {
                    TPF tpf = SoulsFormats.TPF.Read(paths[i]);
                    tpfList.Add(tpf);
                    totalTexCount += tpf.Textures.Count;
                }
                else
                {
                    Console.WriteLine($"Warning: TPF '{paths[i]}' does not exist.");
                }
            }
            double texIndex = 0;
            for (int i = 0; i < tpfList.Count; i++)
            {
                for (int j = 0; j < tpfList[i].Textures.Count; j++)
                {
                    AddFetchTPF(tpfList[i], Utils.GetShortIngameFileName(tpfList[i].Textures[j].Name).ToLower());
                    progress?.Report(++texIndex / totalTexCount);
                }
            }
            progress?.Report(1);
        }

        public TextureCube FetchTextureCube(string name)
        {
            if (name == null)
                return null;
            var shortName = Utils.GetShortIngameFileName(name).ToLower();

            TextureCube result = null;

            if (fetches.ContainsKey(shortName))
            {
                lock (_lock_pool)
                {
                    result = fetches[shortName].FetchCube();
                }
            }
            else
            {
                if (fetches.ContainsKey(shortName + "_atlas000"))
                {
                    lock (_lock_pool)
                    {
                        result = fetches[shortName + "_atlas000"].FetchCube();
                    }
                }
            }

            if (result == null)
                Console.WriteLine($"Could not find TextureCube '{name}'.");

            return result;
        }

        public void AddMapTextures(int area, List<string> textureNames)
        {
            if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1)
            {
                foreach (var t in textureNames)
                {
                    if (fetches.ContainsKey(t))
                        continue;
                    string tpfPath = ParentDocument.GameRoot.GetInterrootPathOld($@"/map/tx/{t}.tpf");
                    if (File.Exists(tpfPath))
                        AddTpfFromPath(tpfPath);
                }
            }
            else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                var bxfs = ParentDocument.GameData.SearchFiles($@"/map/m{area:D2}", @".*\.tpfbhd");
                foreach (var b in bxfs)
                {
                    AddSpecificTexturesFromBXF3(b, textureNames);
                }
            }
            else
            {
                throw new NotImplementedException("ONLY FOR DS1(R)");
            }



        }

        public void AddSpecificTexturesFromBXF3(string name, List<string> textures)
        {
            var bhdBytes = ParentDocument.GameData.ReadFile(name);
            var bdtBytes = ParentDocument.GameData.ReadFile(name.Substring(0, name.Length - 7) + ".tpfbdt");
            var bxf = BXF3.Read(bhdBytes, bdtBytes);

            foreach (var f in bxf.Files)
            {
                if (f.Name != null && f.Name.ToLower().Contains(".tpf") &&
                    textures.Contains(Utils.GetShortIngameFileName(f.Name.ToLower())))
                {
                    AddTpf(TPF.Read(f.Bytes));
                }
            }
        }

        public void AddSpecificTexturesFromBinder(string name, List<string> textures, bool directEntryNameMatch = false)
        {
            if (!File.Exists(name))
                return;

            IBinder bnd = null;


            if (BXF4.IsHeader(name))
                bnd = BXF4.Read(name, name.Substring(0, name.Length - 7) + ".tpfbdt");
            else if (BXF4.IsData(name))
                bnd = BXF4.Read(name.Substring(0, name.Length - 7) + ".tpfbhd", name);
            else if (BXF3.IsHeader(name))
                bnd = BXF3.Read(name, name.Substring(0, name.Length - 7) + ".tpfbdt");
            else if (BXF3.IsData(name))
                bnd = BXF3.Read(name.Substring(0, name.Length - 7) + ".tpfbhd", name);
            else if (BND4.Is(name))
                bnd = BND4.Read(name);
            else if (BND3.Is(name))
                bnd = BND3.Read(name);

            foreach (var f in bnd.Files)
            {

                if (directEntryNameMatch ? textures.Contains(Utils.GetShortIngameFileName(f.Name).ToLower()) : TPF.Is(f.Bytes))
                {
                    var tpf = TPF.Read(f.Bytes);
                    foreach (var tx in tpf.Textures)
                    {
                        var shortTexName = Utils.GetShortIngameFileName(tx.Name).ToLower();
                        if (textures.Contains(shortTexName))
                        {
                            AddFetchTPF(tpf, tx.Name.ToLower());

                            textures.Remove(shortTexName);
                        }
                    }
                }
            }
        }

        public Texture2D FetchTexture2D(string name)
        {
            if (name == null)
                return null;
            var shortName = Utils.GetShortIngameFileName(name).ToLower();

            Texture2D result = null;

            if (fetches.ContainsKey(shortName))
            {
                lock (_lock_pool)
                {
                    result = fetches[shortName].Fetch2D();
                }
            }
            else
            {
                if (fetches.ContainsKey(shortName + "_atlas000"))
                {
                    lock (_lock_pool)
                    {
                        result = fetches[shortName + "_atlas000"].Fetch2D();
                    }
                }
            }

            if (result == null)
                Console.WriteLine($"Could not find Texture2D '{name}'.");

            return result;
        }

        public void AddTexturesOfChr(int chrID)
        {
            var chrbndID = $"c{chrID}";

            if (ParentDocument.GameRoot.GameType == SoulsGames.DS1 || ParentDocument.GameRoot.GameType == SoulsGames.DS1R)
            {
                IBinder chrbnd = null;
                if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID}.chrbnd.dcx"))
                {
                    chrbnd = BND3.Read(ParentDocument.GameData.ReadFile($@"/chr/{chrbndID}.chrbnd.dcx"));
                    AddTextureBnd(chrbnd);
                }

                if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID}.chrtpfbdt") && chrbnd != null)
                {
                    var chrtpfbhd = chrbnd.Files.FirstOrDefault(fi => fi.Name.ToLower().EndsWith(".chrtpfbhd"));

                    if (chrtpfbhd != null)
                    {
                        var texbxf = BXF3.Read(chrtpfbhd.Bytes, ParentDocument.GameData.ReadFile($@"/chr/{chrbndID}.chrtpfbdt"));
                        AddTextureBnd(texbxf);
                    }
                }

                if (ParentDocument.GameRoot.GameType == SoulsGames.DS1)
                {
                    AddInterrootTPFFolder($@"/chr/{chrbndID}");
                }
            }
            else if (ParentDocument.GameRoot.GameType is SoulsGames.DS3 or SoulsGames.SDT or SoulsGames.ER or SoulsGames.ERNR or SoulsGames.BB or SoulsGames.AC6)
            {
                if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID}.texbnd.dcx"))
                {
                    var texbnd = ParentDocument.GameRoot.ReadBinder(ParentDocument.GameData.ReadFile($@"/chr/{chrbndID}.texbnd.dcx"));
                    AddTextureBnd(texbnd);
                }
                else if (ParentDocument.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6 && ParentDocument.GameData.FileExists($@"/chr/{chrbndID}_h.texbnd.dcx"))
                {
                    var texbnd = ParentDocument.GameRoot.ReadBinder(ParentDocument.GameData.ReadFile($@"/chr/{chrbndID}_h.texbnd.dcx"));
                    AddTextureBnd(texbnd);
                }

                if (ParentDocument.GameRoot.GameType == SoulsGames.BB && ParentDocument.GameData.FileExists("/chr/{chrbndID}_2.tpf.dcx"))
                {
                    AddTpf(TPF.Read(ParentDocument.GameData.ReadFile($@"/chr/{chrbndID}_2.tpf.dcx")));
                }
            }
            else if (ParentDocument.GameRoot.GameType == SoulsGames.DES)
            {
                if (ParentDocument.GameData.FileExists($@"/chr/{chrbndID}/{chrbndID}.chrbnd.dcx"))
                {
                    var extraTexChrbnd = BND3.Read(ParentDocument.GameData.ReadFile($@"/chr/{chrbndID}/{chrbndID}.chrbnd.dcx"));
                    AddTextureBnd(extraTexChrbnd, null);
                }
            }


        }
    }
}
