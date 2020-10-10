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
        public static float AdjustSoundVolume = 100;

        public static string MediaPath = null;

        public static string StatusString = "FMOD: (Not Running)";

        private static FMOD.System _system = null;

        private static FMOD.EventSystem _eventSystem = null;

        private static Dictionary<string, string> _loadedFEVs_FullPaths = new Dictionary<string, string>();
        private static List<string> _loadedFEVs = new List<string>();

        private static List<FmodEventUpdater> _eventsToUpdate = new List<FmodEventUpdater>();

        public static object _lock_eventsToUpdate = new object();
        public static IReadOnlyList<FmodEventUpdater> EventsToUpdate => _eventsToUpdate;

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

                Vector3 pos = GFX.World.CameraLocationInWorld_CloserForSound.Position * new Vector3(1, 1, -1);
                Vector3 vel = Vector3.Zero;
                Vector3 up = GFX.World.GetCameraUp() * new Vector3(1, 1, -1);
                Vector3 forward = GFX.World.GetCameraForward() * new Vector3(1, 1, -1);


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



                lock (_lock_eventsToUpdate)
                {
                    List<FmodEventUpdater> finishedEvents = new List<FmodEventUpdater>();

                    foreach (var evt in _eventsToUpdate)
                    {
                        evt.Update(Main.DELTA_UPDATE, Matrix.Identity);
                        if (evt.EventIsOver)
                            finishedEvents.Add(evt);
                    }
                    foreach (var evt in finishedEvents)
                    {
                        _eventsToUpdate.Remove(evt);
                    }
                }



                ERRCHECK(result = _eventSystem.update());

                listenerPosOnPreviousFrame = pos;
            }));
        }


        public class FmodEventUpdater
        {
            public string EventName;
            FMOD.Event Event;
            public Func<Vector3> GetPosFunc { get; private set; } = null;
            public bool EventIsOver = false;
            RESULT evtRes;
            Vector3 oldPos;

            public int? StateInfo = null;

            public FmodEventUpdater(FMOD.Event evt, Func<Vector3> getPosFunc, string name, int? stateInfo = null)
            {
                EventName = name;
                Event = evt;
                GetPosFunc = getPosFunc;
                StateInfo = stateInfo;
            }

            public void Stop(bool immediate)
            {
                var res = Event.stop(immediate);

                if (res == RESULT.ERR_INVALID_HANDLE)
                {
                    EventIsOver = true;
                    return;
                }
            }

            public void Update(float deltaTime, Matrix world)
            {
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

                if (state == EVENT_STATE.READY)
                {
                    EventIsOver = true;
                }
                else
                {
                    var position = Vector3.Transform(((GetPosFunc?.Invoke() ?? Vector3.Zero) * new Vector3(1, 1, 1)), world);
                    var velocity = Vector3.Zero;// (position - oldPos) / deltaTime;

                    // Flatten position to be on a flat plane
                    //position.Y = 0;

                    FMOD.VECTOR posVec = new VECTOR(position);
                    FMOD.VECTOR velVec = new VECTOR(velocity);

                    // If it fails due to the event being released, it's fine. No weirdness should happen.
                    evtRes = Event.set3DAttributes(ref posVec, ref velVec);
                    evtRes = Event.setVolume(BaseSoundVolume * (AdjustSoundVolume / 100));

                    oldPos = position;
                }
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
                FloorMaterialNames.Add((int)id, $"HitMtrlParam {id}");
            }

            string[] defsFile = null;
            if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                defsFile = File.ReadAllLines(Utils.Frankenpath(Main.Directory, @"RES\HitMtrlParamNames.DS1.txt"));
            }
            else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3)
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

        public static ArmorMaterialType ArmorMaterial = ArmorMaterialType.Plates;

        public enum ArmorMaterialType
        {
            Plates = 56,
            Chains = 57,
            Cloth = 58,
            None = 59,
        }


        
        public static IReadOnlyList<string> LoadedFEVs => _loadedFEVs;

        private static bool LoadFEV(string fullFevPath)
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

            string fevKey = Utils.GetShortIngameFileName(fullFevPath);

            if (_loadedFEVs.Contains(fevKey))
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
            _loadedFEVs.Add(fevKey);
            _loadedFEVs_FullPaths.Add(fevKey, Path.GetDirectoryName(fullFevPath));

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
        private static string GetFevPathFromInterroot(string name, bool isDs1Dlc = false)
        {
            if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                return GameDataManager.GetInterrootPath($@"sound\{(isDs1Dlc ? "fdlc" : "frpg")}_{name}.fev");
            else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3)
                return GameDataManager.GetInterrootPath($@"sound\fdp_{name}.fev");
            else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.BB)
                return GameDataManager.GetInterrootPath($@"sound_win\sprj_{name}.fev");
            else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                return GameDataManager.GetInterrootPath($@"sound\{name}.fev");
            else return null;
        }

        public static void LoadInterrootFEV(string name)
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

            string path = GetFevPathFromInterroot(name);
            if (!File.Exists(path) && 
                (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1 || 
                GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R))
            {
                path = GetFevPathFromInterroot(name, isDs1Dlc: true);
            }
            LoadFEV(path);
        }

        public static void LoadMainFEVs()
        {
            if (!initialised)
                return;

            if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                LoadInterrootFEV("main");
                var dlc = GetFevPathFromInterroot("main", isDs1Dlc: true);
                if (File.Exists(dlc))
                    LoadFEV(dlc);
            }
            else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3)
            {
                LoadInterrootFEV("main");

                var dlc1 = GetFevPathFromInterroot("main_dlc1");
                var dlc2 = GetFevPathFromInterroot("main_dlc2");

                if (File.Exists(dlc1))
                    LoadFEV(dlc1);

                if (File.Exists(dlc2))
                    LoadFEV(dlc2);
            }
            else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
            {
                LoadInterrootFEV("main");
            }
            else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.BB)
            {
                LoadInterrootFEV("main");
            }
        }

        public static bool PlaySE(int category, int id, Func<Vector3> getPosFunc, int? stateInfo)
        {
            if (!initialised)
                return false;

            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 || 
            //    GameDataManager.GameType == GameDataManager.GameTypes.DS1R || 
            //    GameDataManager.GameType == GameDataManager.GameTypes.DS3 || 
            //    GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return false;
            //}

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

            // 8 = Floor Material Determined
            else if (category == 8)
                soundName = $"c{(id + (FloorMaterial - 1)):D9}";

            // 9 = Armor Material Determined
            else if (category == 9)
                soundName = $"c{(id + (int)ArmorMaterial):D9}";

            else if (category == 10)
                soundName = $"g{id:D9}";

            if (soundName == null)
                return false;

            return PlaySE(soundName, getPosFunc, stateInfo);
        }

        public static void StopSE(int stateInfo, bool immediate)
        {
            foreach (var snd in _eventsToUpdate)
            {
                if (snd.StateInfo == stateInfo)
                    snd.Stop(immediate);
            }
        }

        public static bool IsStateInfoAlreadyPlaying(int stateInfo)
        {
            foreach (var snd in _eventsToUpdate)
            {
                if (snd.StateInfo == stateInfo)
                    return true;
            }
            return false;
        }


        public static bool PlaySE(string seEventName, Func<Vector3> getPosFunc, int? stateInfo)
        {
            if (PlayEvent(seEventName, getPosFunc, stateInfo))
            {
                return true;
            }

            return false;
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
                    OSD.Tools.Notice("Failed to initialize FMOD audio output. " +
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
                Utils.Frankenpath(GameDataManager.InterrootPath, GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.BB ? "sound_win" : "sound"));
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
                _loadedFEVs.Clear();
                _loadedFEVs_FullPaths.Clear();
            }));
        }

        private static bool PlayEvent(string eventName, Func<Vector3> getPosFunc, int? stateInfo)
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

            bool result = false;
            Main.WinForm.Invoke(new Action(() =>
            {
                FMOD.EventProject evProject = null;

                bool foundEvent = false;

                FMOD.Event newEvent = null;
                string newEvent_FullFevPath = null;

                foreach (var fevName in LoadedFEVs)
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
                                    newEvent_FullFevPath = _loadedFEVs_FullPaths[fevName];
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
                                                newEvent_FullFevPath = _loadedFEVs_FullPaths[fevName];
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
                    result = false;
                    return;
                }

                lock (_lock_MediaRoot)
                {
                    UpdateMediaRoot(newEvent_FullFevPath);

                    ERRCHECK(newEvent.setVolume(BaseSoundVolume * (AdjustSoundVolume / 100)));

                    if (getPosFunc != null)
                    {
                        lock (_lock_eventsToUpdate)
                        {
                            _eventsToUpdate.Add(new FmodEventUpdater(newEvent, getPosFunc, eventName, stateInfo));
                        }
                    }

                    ERRCHECK(newEvent.start());
                    result = true;
                }
                
            }));

            return result;
        }

        public static void Shutdown()
        {
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
            if (result != FMOD.RESULT.OK)
            {
                string e = "FMOD error! " + result + " - " + FMOD.Error.String(result);
                System.Windows.Forms.MessageBox.Show(
                    $"[PLEASE SCREENCAP THIS ENTIRE ERROR MESSAGE AND SEND IT ON DISCORD]" +
                    $"\n\nFMOD (sound engine) error:\n{e}\n\n" +
                    $"STACK TRACE:\n{System.Environment.StackTrace}",
                    "FMOD Sound Engine Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
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
