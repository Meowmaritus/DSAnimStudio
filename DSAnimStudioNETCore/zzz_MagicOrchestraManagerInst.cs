using Microsoft.Xna.Framework;
using NAudio.Wave;
using SoulsAssetPipeline.Audio.Wwise;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;
using DSAnimStudio.ImguiOSD;
using SoulsAssetPipeline.Audio.MagicOrchestra;
using SoulsFormats;

namespace DSAnimStudio
{
    public class zzz_MagicOrchestraManagerInst
    {
        public zzz_DocumentIns ParentDocument;
        public zzz_SoundManagerIns SoundManager;
        public zzz_MagicOrchestraManagerInst(zzz_DocumentIns doc, zzz_SoundManagerIns soundMan)
        {
            ParentDocument = doc;
            SoundManager = soundMan;
        }

        public static bool DEBUG_DUMP_ALL_WEM = false;
        public static bool DEBUG_KEEP_CONVERT_TEMP_FILES = false;

        public Random RAND = new Random();

        public class MemoryInfo
        {
            public long ByteCount;
            public int SoundFileCount;
            public bool AnySoundsLoading;
        }

        public MemoryInfo GetMemoryInfo()
        {
            var info = new MemoryInfo();
            lock (_lock_soundFileConvertCache)
            {
                foreach (var kvp in soundFileConvertCache)
                {
                    if (kvp.Value.IsCompleted)
                    {
                        var wem = kvp.Value.Result;
                        info.ByteCount += wem.WavBytes.Length;
                        info.SoundFileCount++;
                    }
                    else
                    {
                        info.AnySoundsLoading = true;
                    }

                }
            }

            return info;
        }

        public void PurgeLoadedAssets(zzz_SoundManagerIns soundMan)
        {
            //lock (_lock_NAudio)
            //{
            //    foreach (var kvp in NAudioOutputs)
            //    {
            //        kvp.Value.Output.Stop();
            //        kvp.Value.Output.Dispose();
            //    }
            //    NAudioOutputs.Clear();
            //}

            lock (soundMan._LOCK)
            {
                soundFileConvertCache.Clear();
                //foreach (var kvp in loadedMOSBs)
                //    kvp.Value.Dispose();
                loadedMOSBs.Clear();
                loadedMOWBs.Clear();
                loadedMOIBs.Clear();
                loadedMOWVs.Clear();
            }
        }



        public class LoadedSoundFile
        {
            public MOSoundFileKey Key;
            public byte[] WavBytes;
            public bool LoopEnabled;
            public long LoopStart;
            public long LoopEnd;
            public long TotalSampleCount;
        }

        private Dictionary<MOSoundFileKey, Task<LoadedSoundFile>> soundFileConvertCache = new Dictionary<MOSoundFileKey, Task<LoadedSoundFile>>();

        private object _lock_soundFileConvertCache = new object();

        public struct MOSoundFileKey
        {
            public string WavebankName;
            public int SoundIndex;
        }

        public LoadedSoundFile LoadSoundFile(MOSoundFileKey key, Func<byte[]> getSoundBytesIfNotLoaded,
            string bank, int soundID, int vagIndex)
        {
            Task<LoadedSoundFile> soundLoadTask = null;

            lock (_lock_soundFileConvertCache)
            {
                if (!soundFileConvertCache.ContainsKey(key))
                {
                    var loadTask = Task.Run(() =>
                    {
                        LoadedSoundFile loadedSound = null;
                        if (loadedSound != null)
                        {
                            return loadedSound;
                        }

                        loadedSound = new LoadedSoundFile();


                        //TODO obviously
                        loadedSound.LoopEnabled = false;
                        loadedSound.LoopStart = 0;
                        loadedSound.LoopEnd = 0;

                        //throw new NotImplementedException();
                        //TODO: Extract loop etc from VAGp

                        ////byte[] oggBytes = null;
                        byte[] soundBytes = getSoundBytesIfNotLoaded.Invoke();
                        //using (var wemStream = new MemoryStream(wemBytes))
                        //{
                        //    WEMSharp.WEMFile wemConvert = new WEMSharp.WEMFile(wemStream, WEMSharp.WEMForcePacketFormat.NoForcePacketFormat);
                        //    //oggBytes = wemConvert.GenerateOGG($"{Main.Directory}\\Res\\codebooks.bin", false, false);
                        //    loadedWem.LoopEnabled = wemConvert.LoopEnabled != 0;
                        //    loadedWem.LoopStart = wemConvert.LoopStart;
                        //    loadedWem.LoopEnd = wemConvert.LoopEnd;
                        //    loadedWem.TotalSampleCount = wemConvert.SampleCount;
                        //}

                        loadedSound.WavBytes = ConvertSoundFiletoWAV(soundBytes, "VAG", bank, soundID, vagIndex);

                        ////loadedOgg.FixedOggBytes = Wwise.FixOggWithRevorb(oggBytes);
                        //loadedWem.WavBytes = ConvertWEMtoWAV(wemBytes);

                        return loadedSound;
                    });



                    soundFileConvertCache.Add(key, loadTask);
                }

                soundLoadTask = soundFileConvertCache[key];
            }

            soundLoadTask.Wait();
            return soundLoadTask.Result;
        }

        public volatile byte[] BLANK_WAV_BYTES = {
            0x52, 0x49, 0x46, 0x46, 0x30, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45, 0x66, 0x6D, 0x74, 0x20,
            0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x44, 0xAC, 0x00, 0x00, 0x10, 0xB1, 0x02, 0x00,
            0x04, 0x00, 0x10, 0x00, 0x64, 0x61, 0x74, 0x61, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFE, 0xFF
        };

        // Use "VAG" or "AT3" or whatever for extension
        public byte[] ConvertSoundFiletoWAV(byte[] vag, string extension, string bank, int soundIndex, int vagIndex)
        {
            var guid = Guid.NewGuid().ToString();
            var exePath = $@"{Main.Directory}\Res\vgmstream\vgmstream_cmd.exe";
            var temp = $@"{Main.Directory}\Temp\{extension}";
            //var tid = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            var pathIn = $@"{temp}\{bank}__{soundIndex:D2}__{vagIndex:D2}__in_{guid}.{extension}";
            var pathOut = $@"{temp}\{bank}__{soundIndex:D2}__{vagIndex:D2}__out_{guid}.wav";

            if (!Directory.Exists(temp))
                Directory.CreateDirectory(temp);

            File.WriteAllBytes(pathIn, vag);

            var procStart = new ProcessStartInfo(exePath, $"-o \"{pathOut}\" \"{pathIn}\"");

            procStart.CreateNoWindow = true;
            procStart.WindowStyle = ProcessWindowStyle.Hidden;
            procStart.UseShellExecute = false;
            procStart.WorkingDirectory = temp;

            byte[] result = null;

            try
            {
                using (var proc = Process.Start(procStart))
                {
                    proc.WaitForExit();

                    result = File.ReadAllBytes(pathOut);
                }
            }
            catch
            {
                Main.REQUEST_DISABLE_SOUND = true;
                System.Windows.Forms.MessageBox.Show("Unable to decode audio with vgmstream. Make sure you or your antivirus have" +
                    " not deleted the '/Res/vgmstream/vgmstream_cmd.exe' file (re-extract the .zip to get another copy if needed)." +
                    "\n\nDisabling sound simulation now.\n" +
                    "After fixing the issue, you may re-enable it from the 'Simulation' tab.", "",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

            }
            finally
            {
                if (!DEBUG_KEEP_CONVERT_TEMP_FILES)
                {
                    if (File.Exists(pathIn))
                        File.Delete(pathIn);
                    if (File.Exists(pathOut))
                        File.Delete(pathOut);
                }
            }




            return result ?? BLANK_WAV_BYTES;
        }

        private Dictionary<string, MOSB> loadedMOSBs = new Dictionary<string, MOSB>();
        private object _lock_GetMOSB = new object();
        private MOSB GetMOSB(string bankName)
        {
            MOSB result = null;

            lock (_lock_GetMOSB)
            {


                if (loadedMOSBs.ContainsKey(bankName))
                {
                    result = loadedMOSBs[bankName];
                }
                else
                {
                    var bytes = ParentDocument.GameData.ReadFile($"/sound/{bankName}.mosb", warningOnFail: false);
                    if (bytes != null)
                    {
                        result = MOSB.Read(bytes);
                        loadedMOSBs.Add(bankName, result);
                        ParentDocument.SoundManager.RegisterAdditionalSoundBank(bankName);
                    }
                }
            }

            return result;
        }


        private Dictionary<string, MOIB> loadedMOIBs = new Dictionary<string, MOIB>();
        private object _lock_GetMOIB = new object();
        private MOIB GetMOIB(string bankName)
        {
            MOIB result = null;

            lock (_lock_GetMOIB)
            {
                if (loadedMOIBs.ContainsKey(bankName))
                {
                    result = loadedMOIBs[bankName];
                }
                else
                {
                    var bytes = ParentDocument.GameData.ReadFile($"/sound/{bankName}.moib");
                    if (bytes != null)
                    {
                        result = MOIB.Read(bytes);
                        loadedMOIBs.Add(bankName, result);
                        //ParentDocument.SoundManager.RegisterAdditionalSoundBank(bankName);
                    }
                }
            }

            return result;
        }





        private Dictionary<string, MOWV> loadedMOWVs = new Dictionary<string, MOWV>();
        private object _lock_GetMOWV = new object();
        private MOWV GetMOWV(string bankName)
        {
            MOWV result = null;

            lock (_lock_GetMOWV)
            {
                if (loadedMOWVs.ContainsKey(bankName))
                {
                    result = loadedMOWVs[bankName];
                }
                else
                {
                    var mowb = GetMOWB(bankName);
                    if (mowb != null && mowb.Files.Count > 0)
                    {
                        var bytes = mowb.Files[0].Bytes;
                        if (bytes != null)
                        {
                            result = MOWV.Read(bytes);
                            loadedMOWVs.Add(bankName, result);
                            //ParentDocument.SoundManager.RegisterAdditionalSoundBank(bankName);
                        }
                    }
                }
            }

            return result;
        }





        private Dictionary<string, BND3> loadedMOWBs = new Dictionary<string, BND3>();
        private object _lock_GetMOWB = new object();
        private BND3 GetMOWB(string bankName)
        {
            BND3 result = null;

            lock (_lock_GetMOWB)
            {
                if (loadedMOWBs.ContainsKey(bankName))
                {
                    result = loadedMOWBs[bankName];
                }
                else
                {
                    var bytes = ParentDocument.GameData.ReadFile($"/sound/{bankName}.mowb");
                    if (bytes != null)
                    {
                        result = BND3.Read(bytes);
                        loadedMOWBs.Add(bankName, result);
                        //ParentDocument.SoundManager.RegisterAdditionalSoundBank(bankName);
                    }
                }
            }

            return result;
        }


        public List<MOPlaybackInstance> GetPlaybackInstances(zzz_SoundManagerIns soundMan, SoundPlayInfo info)
        {

            string sound = info.GetSoundEventName();
            if (sound != null)
            {
                List<string> bankNames = soundMan.GetAdditionalSoundBankNames();
                return GetPlaybackInstances(soundMan, bankNames, sound);
            }
            else
            {
                return new List<MOPlaybackInstance>();
            }
        }


        public List<MOPlaybackInstance> GetPlaybackInstances(zzz_SoundManagerIns soundMan, List<string> moBankNames,
            string sound)
        {
            //if (soundMan.NeedsWwiseRefresh)
            //{
            //    soundMan.NeedsWwiseRefresh = false;
            //    var soundBankNames = soundMan.GetAdditionalSoundBankNames();
            //    Wwise.PurgeLoadedAssets(soundMan);
            //    Wwise.AddLookupBanks(soundBankNames);
            //}

            List<Task<MOPlaybackInstance>> instancesSpawnedByThisSound = new List<Task<MOPlaybackInstance>>();

            bool success = false;

            //if (DoObjectInBank(wwiseBank1, eventNameHash))
            //{
            //    success = true;
            //}

            //if (wwiseBankName1 != wwiseBankName2)
            //{
            //    if (DoObjectInBank(wwiseBank2, eventNameHash))
            //    {
            //        success = true;
            //    }
            //}

            List<string> checkedBanks = new List<string>();
            foreach (var bn in moBankNames)
            {
                if (checkedBanks.Contains(bn))
                    continue;
                checkedBanks.Add(bn);
                var mosb = GetMOSB(bn);
                if (mosb == null)
                    continue;

                var moib = GetMOIB(bn);

                //tryFindStopEvent(bnk);
                //int countBeforeCheck = instancesSpawnedByThisSound.Count;
                //DoObjectInBank(bnk, eventNameHash, bn, new List<string>());
                int countBeforeCheck = instancesSpawnedByThisSound.Count;


                for (int i = 0; i < mosb.Events.Count; i++)
                {
                    var ev = mosb.Events[i];


                    if (ev.name == sound)
                    {
                        //float falloff = 20;
                        float hardCutoffDist = -1;
                        if (moib != null && i < moib.Entries.Count)
                        {
                            hardCutoffDist = moib.Entries[i].unk0C;
                        }

                        //falloff = ev.

                        int chosenDefIndex = Main.Rand.Next(ev.Defs.Count);
                        var soundDefRef = ev.Defs[chosenDefIndex];

                        var defID = soundDefRef.soundDefID;
                        if (defID >= 0 && defID < mosb.SoundDefs.Count)
                        {
                            var def = mosb.SoundDefs[defID];
                            float volume = def.volume;
                            float pitch = def.pitch;

                            //if (ev.unk0C > 0)
                            //    volume /= ev.unk0C;

                            float falloff = def.struct3s.Length > 0 ? (def.struct3s[0] * 4) : -1;

                            //volume *= 0.5f;
                            volume = MathF.Pow(volume, 0.5f);

                            //int chosenLayerIdx = Main.Rand.Next(def.Layers.Count);
                            //var layer = def.Layers[chosenLayerIdx];

                            foreach (var layer in def.Layers)
                            {
                                int chosenSoundIdx = Main.Rand.Next(layer.Struct5.Sounds.Count);
                                var moSound = layer.Struct5.Sounds[chosenSoundIdx];

                                if (moSound.soundID >= 0)
                                {
                                    var mowb = GetMOWB(bn);
                                    var mowv = GetMOWV(bn);



                                    if (moSound.soundID >= mowv.Struct1s.Count)
                                    {
                                        continue;
                                    }

                                    var captureBankName = bn;

                                    var captureSoundID = moSound.soundID;
                                    var captureVagIndex = mowv.Struct1s[moSound.soundID].vagIndex;

                                    var captureSoundFileBytes = mowb.Files[captureVagIndex].Bytes;

                                    var captureLoop = false; // TODO

                                    var readSoundFileBytesAct = () => captureSoundFileBytes;

                                    if (readSoundFileBytesAct != null)
                                    {
                                        var instanceGetTask = Task.Run(() =>
                                        {
                                            var key = new MOSoundFileKey()
                                            {
                                                WavebankName = bn,
                                                SoundIndex = captureVagIndex
                                            };

                                            var loadedSoundFile = LoadSoundFile(
                                                key, readSoundFileBytesAct, captureBankName, captureSoundID, captureVagIndex);

                                            return new MOPlaybackInstance(key, loadedSoundFile, volume * 0.25f,
                                                MathF.Pow(2, pitch / 12), GFX.CurrentWorldView.CameraLocationInWorld.WorldMatrix,
                                                distanceFalloff: falloff, hardCutoffDist, captureLoop, 0, 0, 0, 0);
                                        });

                                        instancesSpawnedByThisSound.Add(instanceGetTask);
                                    }
                                }
                            }




                        }

                        //foreach (var soundDefRef in ev.Defs)
                        //{



                        //}



                        // Found sound event. stop now.
                        break;
                    }
                }

                // New sounds found pog
                if (instancesSpawnedByThisSound.Count > countBeforeCheck)
                {
                    success = true;
                    break;
                }
            }

            var result = new List<MOPlaybackInstance>();

            foreach (var inst in instancesSpawnedByThisSound)
            {
                inst.Wait();
                result.Add(inst.Result);
            }

            return result;
        }

        public void DisposeAll()
        {

        }
    }
}
