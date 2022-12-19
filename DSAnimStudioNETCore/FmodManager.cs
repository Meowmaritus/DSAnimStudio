using DSAnimStudio.ImguiOSD;
using DSAnimStudio.TaeEditor;
using FMOD;
using Microsoft.Xna.Framework;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio
{
    

    public static class FmodManager
    {
        private static object _lock_MediaRoot = new object();

        private static object _lock_LoadedFevList = new object();

        public static bool IsInitialized => initialised;

        private static bool eventSystemCreated = false;
        private static bool initialised = false;
        private static bool eventstart = false;
        private static bool exit = false;

        private static FMOD.RESULT result;
        //static FMOD.EventGroup group = null;
        //private static FMOD.EventProject project = null;
        //private static FMOD.Sound fsb = null;
        //private static FMOD.System sys;

        public const int MAX_CHANNELS = 32;

        public static float BaseSoundVolume = 0.5f;
        

        public static string MediaPath = null;

        public static string StatusString = "FMOD: (Not Running)";

        private static FMOD.System _system = null;

        private static FMOD.EventSystem _eventSystem = null;

        public static bool IsFevLoaded(string fevName)
        {
            string fevKey = Utils.GetShortIngameFileName(fevName).ToLower();
            bool isLoaded = false;
            lock (_lock_LoadedFevList)
            {
                if (_loadedFEVs.Contains(fevKey) && !File.Exists($@"{LoadedFEVs_FullPaths[fevKey]}\{fevKey}.fev"))
                {
                    _loadedFEVs.Remove(fevKey);
                    _loadedFEVs_FullPaths.Remove(fevKey);
                }

                isLoaded = _loadedFEVs.Contains(fevKey);
            }

            return isLoaded;
        }

        public static bool AreMainFevsLoaded()
        {
            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                return IsFevLoaded("frpg_main") && IsFevLoaded("fdlc_main");
                //dlc main auto loaded for ds1
            }
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
            {
                return IsFevLoaded("fdp_main") &&
                    IsFevLoaded("fdp_main_dlc1") &&
                    IsFevLoaded("fdp_main_dlc2");
            }
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
            {
                return IsFevLoaded("main");
            }
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
            {
                return IsFevLoaded("sprj_main");
            }

            return false;
        }

        private static Dictionary<string, string> _loadedFEVs_FullPaths = new Dictionary<string, string>();
        private static List<string> _loadedFEVs = new List<string>();

        public static IReadOnlyList<string> LoadedFEVs
        {
            get
            {
                IReadOnlyList<string> result = null;
                lock (_lock_LoadedFevList)
                {
                    result = _loadedFEVs.ToList();
                }
                return result ?? new List<string>();
            }
        }

        public static IReadOnlyDictionary<string, string> LoadedFEVs_FullPaths
        {
            get
            {
                IReadOnlyDictionary<string, string> result = null;
                lock (_lock_LoadedFevList)
                {
                    result = _loadedFEVs_FullPaths.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return result ?? new Dictionary<string, string>();
            }
        }

        //private static List<FmodEventUpdater> _eventsToUpdate = new List<FmodEventUpdater>();

        //public static object _lock_eventsToUpdate = new object();
        //public static IReadOnlyList<FmodEventUpdater> EventsToUpdate => _eventsToUpdate;

        private static Vector3 listenerPosOnPreviousFrame;

        public static int FloorMaterial = 1;

        public static Dictionary<int, string> FloorMaterialNames = new Dictionary<int, string>();

        public static DebugPrimitives.DbgPrimWireArrow DbgPrimCamPos;

        public static void Update()
        {
            if (!initialised)
                return;

            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return;
            //}

            Main.WinForm.Invoke(new Action(() =>
            {

                //_eventSystem.set3DListenerAttributes(listener: 0, ref pos, ref vel, ref forward, ref up);Vector3.Transform(

                Vector3 pos = GFX.CurrentWorldView.CameraLocationInWorld_CloserForSound.Position * new Vector3(1, 1, -1);
                Vector3 vel = Vector3.Zero;
                Vector3 up = GFX.CurrentWorldView.GetCameraUp() * new Vector3(1, 1, -1);
                Vector3 forward = GFX.CurrentWorldView.GetCameraForward() * new Vector3(1, 1, -1);


                DbgPrimCamPos.Transform = new Transform(Matrix.CreateWorld(pos, forward, up));


                //if (GFX.World.CameraTransform.EulerRotation.X > MathHelper.PiOver4)
                //{
                //    // If camera is pointing upward, get forward XZ from the down vector (for preventing gimbal issues)
                //    forward = -GFX.World.GetScreenSpaceUpVector();
                //}
                //else if (GFX.World.CameraTransform.EulerRotation.X < -MathHelper.PiOver4)
                //{
                //    // If camera is pointing downward, get forward XZ from the up vector (for preventing gimbal issues)
                //    forward = GFX.World.GetScreenSpaceUpVector();
                //}

                //// Flatten forward to be pointing directly forward.
                //forward.Y = 0;
                //forward = Vector3.Normalize(forward);

                //// Flatten position to be on a flat plane
                //pos.Y = 0;

                VECTOR posVec = new VECTOR(pos);
                VECTOR velVec = new VECTOR(vel);
                VECTOR upVec = new VECTOR(up);
                VECTOR forwardVec = new VECTOR(forward);



                ERRCHECK(result = _eventSystem.set3DListenerAttributes(0, ref posVec, ref velVec, ref forwardVec, ref upVec));



                //lock (_lock_eventsToUpdate)
                //{
                //    List<FmodEventUpdater> finishedEvents = new List<FmodEventUpdater>();

                //    foreach (var evt in _eventsToUpdate)
                //    {
                //        evt.Update(Main.DELTA_UPDATE, Matrix.Identity);

                //        if (evt.EventIsOver)
                //            finishedEvents.Add(evt);
                //    }
                //    foreach (var evt in finishedEvents)
                //    {
                //        _eventsToUpdate.Remove(evt);
                //    }
                //}



                ERRCHECK(result = _eventSystem.update());

                listenerPosOnPreviousFrame = pos;
            }));
        }


        public class FmodEventUpdater
        {
            public string EventName;
            FMOD.Event Event;
            public bool IsFirstFrameExisting = true;
            public bool EventIsOver = false;
            RESULT evtRes;

            bool hasntStartedPlayingYet = true;

            //public int Slot = -1;

            public float Lifetime = -1;
            public float InitialLifetime = -1;

            //public string DebugViewString => $"{(Slot >= 0 ? $"[{Slot}] " : "")}{EventName}{(Lifetime >= 0 ? $" ({Lifetime:0.000} / {InitialLifetime:0.000})" : "")}";

            public FmodEventUpdater(FMOD.Event evt, string name, float lifetime)
            {
                EventName = name;
                Event = evt;
                //Slot = slot;
                Lifetime = InitialLifetime = lifetime;
            }

            public void Stop(bool immediate)
            {
                if (EventIsOver && !immediate)
                    return;
                var res = Event.stop(immediate);

                if (res == RESULT.ERR_INVALID_HANDLE)
                {
                    EventIsOver = true;
                    return;
                }
            }

            public void Pause()
            {
                if (EventIsOver)
                    return;

                var res = Event.setPaused(true);
                if (res == RESULT.ERR_INVALID_HANDLE)
                {
                    EventIsOver = true;
                    return;
                }
            }

            public void Resume()
            {
                if (EventIsOver)
                    return;

                var res = Event.setPaused(false);

                if (res == RESULT.ERR_INVALID_HANDLE)
                {
                    EventIsOver = true;
                    return;
                }
            }

            public void StartPlay()
            {
                var res = Event.start();
                if (res == RESULT.ERR_INVALID_HANDLE)
                    EventIsOver = true;
                hasntStartedPlayingYet = false;
            }

            public void Update(float deltaTime, Matrix world, Vector3 position)
            {
                if (Lifetime > 0)
                {
                    Lifetime -= deltaTime;
                    if (Lifetime < 0)
                        Lifetime = 0;

                    if (Lifetime == 0)
                        Stop(false);
                }

                //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
                //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
                //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
                //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
                //{
                //    return;
                //}

                FMOD.EVENT_STATE state = EVENT_STATE.PLAYING;
                var evtRes = Event.getState(ref state);

                if (evtRes == RESULT.ERR_INVALID_HANDLE)
                {
                    EventIsOver = true;
                    return;
                }

                if (state == EVENT_STATE.READY && !hasntStartedPlayingYet)
                {
                    EventIsOver = true;
                }
                else
                {
                    //if (UpdatingPosition || IsFirstFrameExisting)
                    //    CurrentPos = Vector3.Transform(((GetPosFunc?.Invoke() ?? Vector3.Zero) * new Vector3(1, 1, 1)), world);

                    // I don't feel like fixing up the world positions with the pos wrapping etc to make the above not horrible
                    //CurrentPos = Vector3.Transform(((GetPosFunc?.Invoke() ?? Vector3.Zero) * new Vector3(1, 1, 1)), world);

                    var velocity = Vector3.Zero;// (position - oldPos) / deltaTime;

                    // Flatten position to be on a flat plane
                    //position.Y = 0;

                    FMOD.VECTOR posVec = new VECTOR(position);
                    FMOD.VECTOR velVec = new VECTOR(velocity);

                    // If it fails due to the event being released, it's fine. No weirdness should happen.
                    evtRes = Event.set3DAttributes(ref posVec, ref velVec);
                    evtRes = Event.setVolume(BaseSoundVolume * (SoundManager.AdjustSoundVolume / 100));

                    //oldPos = CurrentPos;
                }

                IsFirstFrameExisting = false;
            }
        }


        public static FMOD.EventProject GetEventProject(string name)
        {
            FMOD.EventProject proj = null;
            var numProjects = 0;
            result = _eventSystem.getNumProjects(ref numProjects);
            ERRCHECK(result = _eventSystem.getProjectByIndex(0, ref proj));
            return proj;
        }

        public static void LoadFloorMaterialNamesFromInterroot()
        {
            FloorMaterialNames.Clear();

            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return;
            //}

            foreach (var id in ParamManager.HitMtrlParamEntries)
            {
                FloorMaterialNames.Add((int)id.Key, $"[{id.Key}] " + (!string.IsNullOrEmpty(id.Value) ? id.Value : $"HitMtrlParam {id.Key}"));
            }

            string[] defsFile = null;
            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                defsFile = File.ReadAllLines(Utils.Frankenpath(Main.Directory, @"RES\HitMtrlParamNames.DS1.txt"));
            }
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
            {
                defsFile = File.ReadAllLines(Utils.Frankenpath(Main.Directory, @"RES\HitMtrlParamNames.DS3.txt"));
            }

            if (defsFile != null)
            {
                foreach (var line in defsFile)
                {
                    var split = line.Split('|');
                    var id = int.Parse(split[0]);
                    var name = split[1];
                    if (!FloorMaterialNames.ContainsKey(id))
                        FloorMaterialNames.Add(id, name);
                    else
                        FloorMaterialNames[id] = name;
                }
            }
        }

        public static int ArmorMaterial;

        //public enum ArmorMaterialType
        //{
        //    Plates = 56,
        //    Chains = 57,
        //    Cloth = 58,
        //    None = 59,
        //}


        
        

        public static bool LoadFEV(string fullFevPath)
        {
            if (!initialised)
                return false;

            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return false;
            //}

            string fevKey = Utils.GetShortIngameFileName(fullFevPath).ToLower();

            bool alreadyLoaded = false;
            lock (_lock_LoadedFevList)
            {
                if (_loadedFEVs.Contains(fevKey))
                    alreadyLoaded = true;
            }
            if (alreadyLoaded)
                return true;

            if (!File.Exists(fullFevPath))
            {
                return false;
            }

            Main.WinForm.Invoke(new Action(() =>
            {
                UpdateMediaRoot(Path.GetDirectoryName(fullFevPath));
                result = _eventSystem.load(fevKey + ".fev");
                if (result != RESULT.ERR_FILE_NOTFOUND)
                {
                    ERRCHECK(result);
                }
                
            }));
            lock (_lock_LoadedFevList)
            {
                _loadedFEVs.Add(fevKey);
                _loadedFEVs_FullPaths.Add(fevKey, Path.GetDirectoryName(fullFevPath));
            }

            return true;
        }

        //public static bool PlayEventInFEV(string fevFilePath, string eventName)
        //{
        //    //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
        //    //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
        //    //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
        //    //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
        //    //{
        //    //    return false;
        //    //}

        //    var foundFev = LoadFEV(fevFilePath);
        //    if (!foundFev)
        //        return false;
        //    else
        //        return PlayEvent(eventName, null, null);
        //}

        /// <summary>
        /// Example "main" will return the full path ending like "Game/sound/fdp_main.fev" in DS3.
        /// </summary>
        /// <param name="fevNameAfterPrefix"></param>
        /// <returns></returns>
        public static string GetFevPathFromInterroot(string name, bool isDs1Dlc = false)
        {
            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                return $@"/sound/{(isDs1Dlc ? "fdlc" : "frpg")}_{name}.fev";
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
                return $@"/sound/fdp_{name}.fev";
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
                return $@"/sound_win/sprj_{name}.fev";
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                return $@"/sound/{name}.fev";
            else return null;
        }

        public static void LoadInterrootFEV(string name)
        {
            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER)
                return;

            if (!initialised)
                return;

            bool alreadyLoaded = false;
            lock (_lock_LoadedFevList)
            {
                if (_loadedFEVs.Contains(Utils.GetShortIngameFileName(name).ToLower()))
                    alreadyLoaded = true;
            }
            if (alreadyLoaded)
                return;

            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return;
            //}

            string path = GetFevPathFromInterroot(name);
            if (GameData.FileExists(path))
            {
                var unpackedFile = GameData.ReadFileAndTempUnpack(path);
                GameData.ReadFileAndTempUnpack(path.Substring(0, path.Length - 4) + ".fsb");
                if (unpackedFile != null)
                    LoadFEV(unpackedFile.AbsolutePath);

               
            }

            //LoadFEV(path);
            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                path = GetFevPathFromInterroot(name, isDs1Dlc: true);
                if (GameData.FileExists(path))
                {
                    var unpackedFileDLC = GameData.ReadFileAndTempUnpack(path);
                    GameData.ReadFileAndTempUnpack(path.Substring(0, path.Length - 4) + ".fsb");
                    if (unpackedFileDLC != null)
                        LoadFEV(unpackedFileDLC.AbsolutePath);
                }
            }

        }

        public static void LoadMainFEVs()
        {
            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER)
                return;

            if (!initialised)
                return;

            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                LoadInterrootFEV("main");
                //dlc main auto loaded for ds1
            }
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
            {
                LoadInterrootFEV("main");
                LoadInterrootFEV("main_dlc1");
                LoadInterrootFEV("main_dlc2");
            }
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
            {
                LoadInterrootFEV("main");
            }
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
            {
                LoadInterrootFEV("main");
            }
        }

        public static string GetSoundName(int category, int id)
        {
            string soundName = null;
            if (category == 0)
                soundName = $"a{id:D9}";
            else if (category == 1)
                soundName = $"c{id:D9}";
            else if (category == 2)
                soundName = $"f{id:D9}";
            else if (category == 3)
                soundName = $"o{id:D9}";
            else if (category == 4)
                soundName = $"p{id:D9}";
            else if (category == 5)
                soundName = $"s{id:D9}";
            else if (category == 6)
                soundName = $"m{id:D9}";
            else if (category == 7)
                soundName = $"v{id:D9}";

            // Floor and armor material IDs are now added before this function after sound engine rewrite.
            // 8 = Floor Material Determined
            else if (category == 8)
                soundName = $"c{id:D9}";

            // 9 = Armor Material Determined
            else if (category == 9)
                soundName = $"c{id:D9}";

            else if (category == 10)
                soundName = $"g{id:D9}";

            return soundName;
        }

        public static FmodEventUpdater PlaySE(int category, int id, float lifetime)
        {
            if (!initialised)
                return null;

            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 || 
            //    GameDataManager.GameType == GameDataManager.GameTypes.DS1R || 
            //    GameDataManager.GameType == GameDataManager.GameTypes.DS3 || 
            //    GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return false;
            //}

            var soundName = GetSoundName(category, id);

            if (soundName == null)
                return null;

            return PlaySE(soundName, lifetime);
        }

        //public static void StopAllSESlotsNotInList(List<int> slots, bool immediate)
        //{
        //    foreach (var snd in _eventsToUpdate)
        //    {
        //        if (snd.Slot >= 0 && !slots.Contains(snd.Slot))
        //        {
        //            snd.Stop(immediate);
        //        }
        //    }
        //}

        //public static void StopSESlot(int slot, bool immediate)
        //{
        //    foreach (var snd in _eventsToUpdate)
        //    {
        //        if (snd.Slot == slot)
        //            snd.Stop(immediate);
        //    }
        //}

        //public static bool IsSlotAlreadyPlaying(int slot)
        //{
        //    foreach (var snd in _eventsToUpdate)
        //    {
        //        if (snd.Slot == slot)
        //            return true;
        //    }
        //    return false;
        //}

        //public static string GetEventNameOfSlot(int slot)
        //{
        //    foreach (var snd in _eventsToUpdate)
        //    {
        //        if (snd.Slot == slot)
        //            return snd.EventName;
        //    }
        //    return null;
        //}


        private static FmodEventUpdater PlaySE(string seEventName, float lifetime)
        {
            return PlayEvent(seEventName, lifetime);
        }

        public static void InitTest()
        {
            if (initialised)
                return;

            Main.WinForm.Invoke(new Action(() =>
            {
                if (eventSystemCreated)
                {
                    Shutdown();
                }

                result = FMOD.Event_Factory.EventSystem_Create(ref _eventSystem);

                if (result == RESULT.OK)
                {
                    eventSystemCreated = true;
                }
                else
                {
                    ERRCHECK(result);
                }

                result = _eventSystem.init(MAX_CHANNELS, FMOD.INITFLAGS.NORMAL, (IntPtr)null, FMOD.EVENT_INITFLAGS.NORMAL);
                if (result == RESULT.ERR_OUTPUT_INIT)
                {
                    DialogManager.DialogOK(null, "Failed to initialize FMOD audio output. " +
                        "Make sure you have an audio device connected and working and " +
                        "that no other app is taking exclusive control of the device.\n\n" +
                        "Once you free the device, select FMOD Sound -> Retry Initialization");
                    initialised = false;
                }
                else if (result == RESULT.OK)
                {
                    initialised = true;
                }
                else
                {
                    ERRCHECK(result);
                }

                ERRCHECK(result = _eventSystem.getSystemObject(ref _system));

                DbgPrimCamPos = new DebugPrimitives.DbgPrimWireArrow("FMOD Camera", Transform.Default, Color.Lime);
                DbgPrimCamPos.Category = DebugPrimitives.DbgPrimCategory.SoundEvent;
            }));
        }

        public static void UpdateInterroot()
        {
            if (!initialised)
                return;
            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return;
            //}

            Main.WinForm.Invoke(new Action(() =>
            {
                string possiblePath = GetDirWithBackslash(
                Utils.Frankenpath(GameRoot.InterrootPath, GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB ? "sound_win" : "sound"));
                result = _eventSystem.setMediaPath(possiblePath);
                if (result == RESULT.OK)
                {
                    MediaPath = possiblePath;
                }
                else
                {
                    ERRCHECK(result);
                    MediaPath = null;
                }
            }));
        }

        public static void UpdateMediaRoot(string mediaRoot)
        {
            if (!initialised)
                return;

            Main.WinForm.Invoke(new Action(() =>
            {
                result = _eventSystem.setMediaPath(GetDirWithBackslash(mediaRoot));
                if (result == RESULT.OK)
                {
                    MediaPath = mediaRoot;
                }
                else
                {
                    ERRCHECK(result);
                    MediaPath = null;
                }
            }));
        }

        public static void Purge()
        {
            if (!initialised)
                return;

            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return;
            //}

            Main.WinForm.Invoke(new Action(() =>
            {
                ERRCHECK(result = _eventSystem.unload());
                lock (_lock_LoadedFevList)
                {
                    _loadedFEVs.Clear();
                    _loadedFEVs_FullPaths.Clear();
                }
            }));
        }

        private static FmodEventUpdater PlayEvent(string eventName, float lifetime)
        {
            if (!initialised)
                return null;

            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return false;
            //}

            FmodEventUpdater result = null;
            Main.WinForm.Invoke(new Action(() =>
            {
                FMOD.EventProject evProject = null;

                bool foundEvent = false;

                FMOD.Event newEvent = null;
                string newEvent_FullFevPath = null;

                var loadedFevsCopy = LoadedFEVs.ToList();

                foreach (var fevName in loadedFevsCopy)
                {
                    var fres = _eventSystem.getProject(fevName, ref evProject);
                    if (fres == RESULT.OK)
                    {
                        int groupCount = 0;
                        fres = evProject.getNumGroups(ref groupCount);
                        if (fres == RESULT.OK)
                        {
                            bool searchGroup(FMOD.EventGroup grp)
                            {
                                fres = grp.getEvent(eventName, EVENT_MODE.DEFAULT, ref newEvent);
                                if (fres == RESULT.OK)
                                {
                                    lock (_lock_LoadedFevList)
                                    {
                                        newEvent_FullFevPath = _loadedFEVs_FullPaths[fevName];
                                    }
                                    return true; // Returning from searchGroup() lol
                                }
                                int numInnerGroups = 0;
                                fres = grp.getNumGroups(ref numInnerGroups);

                                if (fres == RESULT.OK)
                                {
                                    for (int j = 0; j < numInnerGroups; j++)
                                    {
                                        FMOD.EventGroup innerInnerGroup = null;
                                        fres = grp.getGroupByIndex(j, false, ref innerInnerGroup);
                                        if (fres == RESULT.OK)
                                        {
                                            if (searchGroup(innerInnerGroup))
                                            {
                                                lock (_lock_LoadedFevList)
                                                {
                                                    newEvent_FullFevPath = _loadedFEVs_FullPaths[fevName];
                                                }
                                                return true;
                                            }
                                        }
                                    }
                                }

                                return false;
                            }

                            for (int i = 0; i < groupCount; i++)
                            {
                                FMOD.EventGroup innerGroup = null;
                                fres = evProject.getGroupByIndex(i, cacheevents: false, ref innerGroup);
                                if (fres == RESULT.OK)
                                {
                                    if (searchGroup(innerGroup))
                                    {
                                        foundEvent = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!foundEvent)
                {
                    result = null;
                    // Return from delegate fatcat
                    return;
                }

                lock (_lock_MediaRoot)
                {
                    UpdateMediaRoot(newEvent_FullFevPath);

                    ERRCHECK(newEvent.setVolume(BaseSoundVolume * (SoundManager.AdjustSoundVolume / 100)));

                    result = new FmodEventUpdater(newEvent, eventName, lifetime);

                    //ERRCHECK(newEvent.start());
                }
                
            }));

            return result;
        }

        public static void Shutdown()
        {
            if (GameRoot.GameTypeUsesWwise)
                Wwise.DisposeAll();
            //Even if not initialized the event system is created and must be released.
            //if (!initialised)
            //    return;

            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return;
            //}

            Main.WinForm.Invoke(new Action(() =>
            {
                ERRCHECK(result = _eventSystem.release());
                //foreach (var kvp in _eventProjects)
                //{
                //    ERRCHECK(result = kvp.Value.release());
                //}
            }));
        }

        public static void StopAllSounds()
        {

            if (!initialised)
                return;

            RemoManager.CancelFullPlayback();

            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return;
            //}

            Main.WinForm.Invoke(new Action(() =>
            {
                int channelsPlaying = 0;
                result = _system.getChannelsPlaying(ref channelsPlaying);

                if (result == RESULT.ERR_INVALID_PARAM || result == RESULT.ERR_INVALID_HANDLE)
                    return;

                for (int i = 0; i < MAX_CHANNELS; i++)
                {
                    FMOD.Channel channel = null;

                    result = _system.getChannel(i, ref channel);

                    if (result == RESULT.ERR_INVALID_PARAM || result == RESULT.ERR_INVALID_HANDLE)
                        continue;

                    result = channel.stop();
                }
            }));
            
        }

        private static void ERRCHECK(FMOD.RESULT result)
        {
            return;
            if (result != FMOD.RESULT.OK)
            {
                string e = result + " - " + FMOD.Error.String(result);
                NotificationManager.PushNotification(
                    $"FMOD (sound engine) error: {e}\n" +
                    $"STACK TRACE:\n{System.Environment.StackTrace}",
                    color: NotificationManager.ColorError, showDuration: 10);
            }
        }

        private static string GetDirWithBackslash(string dir)
        {
            if (!dir.EndsWith("\\"))
                dir += "\\";
            return dir;
        }
    }
}
