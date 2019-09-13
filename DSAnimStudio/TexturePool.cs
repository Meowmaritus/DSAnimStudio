using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DSAnimStudio
{
    public static class TexturePool
    {
        public static bool DisableErrorHandling = true;

        public static Dictionary<TextureFetchRequest.TextureInfo, Exception> Failures 
            = new Dictionary<TextureFetchRequest.TextureInfo, Exception>();

        private static object _lock_IO = new object();
        private static object _lock_pool = new object();
        //This might be weird because it doesn't follow convention :fatcat:
        public delegate void TextureLoadErrorDelegate(string texName, string error);
        public static event TextureLoadErrorDelegate OnLoadError;
        private static void RaiseLoadError(string texName, string error)
        {
            OnLoadError?.Invoke(texName, error);
        }

        //private Dictionary<string, string> OnDemandTexturePaths = new Dictionary<string, string>();
        private static Dictionary<string, TextureFetchRequest> fetches = new Dictionary<string, TextureFetchRequest>();

        public static IReadOnlyDictionary<string, TextureFetchRequest> Fetches => fetches;

        public static void RemoveFetchRefsWithoutFlushing()
        {
            fetches.Clear();
        }

        public static void FlushDataOnly()
        {
            lock (_lock_pool)
            {
                foreach (var fetch in fetches)
                {
                    fetch.Value.FlushCachedTexture();
                }
            }
        }

        public static void DestroyUnusedTextures()
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

        public static void Flush()
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
        }

        public static void AddFetchTPF(TPF tpf, string texName)
        {
            string shortName = Path.GetFileNameWithoutExtension(texName);
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
                    var newFetch = new TextureFetchRequest(tpf, texName);
                    fetches.Add(shortName, newFetch);
                }
            }

        }

        public static void AddFetchDDS(byte[] dds, string texName)
        {
            string shortName = Path.GetFileNameWithoutExtension(texName);
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
                    var newFetch = new TextureFetchRequest(dds, texName);
                    fetches.Add(shortName, newFetch);
                }
            }

        }

        public static void AddTpf(TPF tpf)
        {
            foreach (var tex in tpf.Textures)
            {
                AddFetchTPF(tpf, tex.Name);
            }
        }

        public static void AddLooseDDSFolder(string folderPath, bool subDirectories = false)
        {
            var dds = Directory.GetFiles(folderPath, "*.dds", subDirectories
                ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (var d in dds)
            {
                string shortName = Path.GetFileNameWithoutExtension(d);
                AddFetchDDS(File.ReadAllBytes(d), shortName);
            }
        }

        public static void AddTPFFolder(string folderPath, bool dcx = false, bool directDdsFetches = false)
        {
            var tpfs = Directory.GetFiles(folderPath, dcx ? "*.tpf.dcx" : "*.tpf");
            foreach (var t in tpfs)
            {
                string shortName = Path.GetFileNameWithoutExtension(t);
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

        public static void AddTextureBnd(IBinder bnd, IProgress<double> prog)
        {
            var tpfs = bnd.Files.Where(file => file.Name.EndsWith(".tpf")).ToList();
            var tbnds = bnd.Files.Where(file => file.Name.ToLower().EndsWith(".tbnd")).ToList();

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

        public static void AddTpfFromPath(string path)
        {
            TPF tpf = SoulsFormats.TPF.Read(path);
            AddTpf(tpf);
        }

        public static TextureCube FetchTextureCube(string name)
        {
            if (name == null)
                return null;
            var shortName = Path.GetFileNameWithoutExtension(name);
            if (fetches.ContainsKey(shortName))
            {
                lock (_lock_pool)
                {
                    if (DisableErrorHandling)
                        return fetches[shortName].FetchCube();
                    else
                        return fetches[shortName].FetchCubeWithErrorHandle();
                }
            }
            else
            {
                if (fetches.ContainsKey(shortName + "_atlas000"))
                {
                    lock (_lock_pool)
                    {
                        if (DisableErrorHandling)
                            return fetches[shortName + "_atlas000"].FetchCube();
                        else
                            return fetches[shortName + "_atlas000"].FetchCubeWithErrorHandle();
                    }
                }
                return null;
            }
        }

        public static Texture2D FetchTexture2D(string name)
        {
            if (name == null)
                return null;
            var shortName = Path.GetFileNameWithoutExtension(name);
            if (fetches.ContainsKey(shortName))
            {
                lock (_lock_pool)
                {
                    if (DisableErrorHandling)
                        return fetches[shortName].Fetch2D();
                    else
                        return fetches[shortName].Fetch2DWithErrorHandle();
                }
            }
            else
            {
                if (fetches.ContainsKey(shortName + "_atlas000"))
                {
                    lock (_lock_pool)
                    {
                        if (DisableErrorHandling)
                            return fetches[shortName + "_atlas000"].Fetch2D();
                        else
                            return fetches[shortName + "_atlas000"].Fetch2DWithErrorHandle();
                    }
                }
                return null;
            }
        }
    }
}