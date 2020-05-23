using DSAnimStudio.TaeEditor;
using FMOD;
using Microsoft.Xna.Framework;
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
        public static float AdjustSoundVolume = 1;

        public static string StatusString = "FMOD: (Not Running)";

        private static FMOD.System _system = null;

        private static FMOD.EventSystem _eventSystem = null;

        private static List<string> _loadedFEVs = new List<string>();

        private static List<FmodEventUpdater> _eventsToUpdate = new List<FmodEventUpdater>();

        private static Vector3 listenerPosOnPreviousFrame;

        public static int FloorMaterial = 1;

        public static Dictionary<int, string> FloorMaterialNames = new Dictionary<int, string>();

        public static void LoadFloorMaterialNamesFromInterroot()
        {

            FloorMaterialNames.Clear();

            foreach (var id in ParamManager.HitMtrlParamEntries)
            {
                FloorMaterialNames.Add((int)id, $"HitMtrlParam {id}");
            }

            string[] defsFile = null;
            if (GameDataManager.GameType == GameDataManager.GameTypes.DS1 || GameDataManager.GameType == GameDataManager.GameTypes.DS1R)
            {
                defsFile = File.ReadAllLines(Utils.Frankenpath(Main.Directory, @"RES\HitMtrlParamNames.DS1.txt"));
            }
            else if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
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

            BuildSoundMenuBar();
        }

        public static void BuildSoundMenuBar()
        {
            // Ghetto-ness warning
            Main.WinForm.Invoke(new Action(() =>
            {
                //Main.TAE_EDITOR.MenuBar.AddTopItem("Sound");

                Main.TAE_EDITOR.MenuBar.ClearItem("Sound");

                Main.TAE_EDITOR.MenuBar.AddItem("Sound", "[STOP ALL]", () =>
                {
                    StopAllSounds();
                });

                Main.TAE_EDITOR.MenuBar.AddSeparator("Sound");

                Main.TAE_EDITOR.MenuBar.AddItem("Sound", "Player Armor Material", new Dictionary<string, Action>
                {
                    { nameof(ArmorMaterialType.Plates), () => ArmorMaterial = ArmorMaterialType.Plates },
                    { nameof(ArmorMaterialType.Chains), () => ArmorMaterial = ArmorMaterialType.Chains },
                    { nameof(ArmorMaterialType.Cloth), () => ArmorMaterial = ArmorMaterialType.Cloth },
                    { nameof(ArmorMaterialType.None), () => ArmorMaterial = ArmorMaterialType.None },
                }, () => ArmorMaterial.ToString());

                Dictionary<string, Action> floorMatMenuItems = new Dictionary<string, Action>();

                foreach (var kvp in FloorMaterialNames)
                {
                    floorMatMenuItems.Add($"{kvp.Key:D2}: {kvp.Value}", () =>
                    {
                        FloorMaterial = kvp.Key;
                    });
                }

                Main.TAE_EDITOR.MenuBar.AddItem(@"Sound", "Floor Material", floorMatMenuItems, 
                    () => $"{FloorMaterial:D2}: {FloorMaterialNames[FloorMaterial]}");
            }));

           

        }

        public static ArmorMaterialType ArmorMaterial = ArmorMaterialType.Plates;

        public enum ArmorMaterialType
        {
            Plates = 56,
            Chains = 57,
            Cloth = 58,
            None = 59,
        }

        public class FmodEventUpdater
        {
            FMOD.Event Event;
            Func<Vector3> GetPosFunc;
            public bool EventIsOver = false;
            RESULT evtRes;
            Vector3 oldPos;
            public FmodEventUpdater(FMOD.Event evt, Func<Vector3> getPosFunc)
            {
                Event = evt;
                GetPosFunc = getPosFunc;
            }
            public void Update(float deltaTime)
            {
                FMOD.EVENT_STATE state = EVENT_STATE.PLAYING;
                evtRes = Event.getState(ref state);

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
                    var position = (GetPosFunc?.Invoke() ?? Vector3.Zero) * new Vector3(-1, 1, 1);
                    var velocity = Vector3.Zero;// (position - oldPos) / deltaTime;

                    FMOD.VECTOR posVec = new VECTOR(position);
                    FMOD.VECTOR velVec = new VECTOR(velocity);

                    // If it fails due to the event being released, it's fine. No weirdness should happen.
                    evtRes = Event.set3DAttributes(ref posVec, ref velVec);
                    evtRes = Event.setVolume(BaseSoundVolume * AdjustSoundVolume);

                    oldPos = position;
                }
            }
        }

        public static IReadOnlyList<string> LoadedFEVs => _loadedFEVs;

        private static bool LoadFEV(string fullFevPath)
        {
            string fevKey = Utils.GetShortIngameFileName(fullFevPath);

            if (_loadedFEVs.Contains(fevKey))
                return true;

            if (!File.Exists(fullFevPath))
            {
                return false;
            }

            Main.WinForm.Invoke(new Action(() =>
            {
                ERRCHECK(result = _eventSystem.load(fevKey + ".fev"));
            }));
            _loadedFEVs.Add(fevKey);

            return true;
        }

        public static bool PlayEventInFEV(string fevFilePath, string eventPath)
        {
            var foundFev = LoadFEV(fevFilePath);
            if (!foundFev)
                return false;
            else
                return PlayEvent(eventPath);
        }

        private static string GetEventPath(string fevName, string eventName)
        {
            if (GameDataManager.GameType == GameDataManager.GameTypes.SDT)
            {
                return $"{fevName}/{fevName}/{eventName}";
            }
            else
            {
                var secondPartOfName = fevName.Split('_')[1];
                return $"{fevName}/{secondPartOfName}/{eventName}";
            }
  
        }

        /// <summary>
        /// Example "main" will return the full path ending like "Game/sound/fdp_main.fev" in DS3.
        /// </summary>
        /// <param name="fevNameAfterPrefix"></param>
        /// <returns></returns>
        private static string GetFevPathFromInterroot(string name, bool isDs1Dlc = false)
        {
            if (GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
                GameDataManager.GameType == GameDataManager.GameTypes.DS1R)
                return Utils.Frankenpath(GameDataManager.InterrootPath, $@"sound\{(isDs1Dlc ? "fdlc" : "frpg")}_{name}.fev");
            else if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
                return Utils.Frankenpath(GameDataManager.InterrootPath, $@"sound\fdp_{name}.fev");
            else if (GameDataManager.GameType == GameDataManager.GameTypes.SDT)
                return Utils.Frankenpath(GameDataManager.InterrootPath, $@"sound\{name}.fev");
            else throw new NotImplementedException();
        }

        public static void LoadInterrootFEV(string name)
        {
            string path = GetFevPathFromInterroot(name);
            if (!File.Exists(path) && 
                (GameDataManager.GameType == GameDataManager.GameTypes.DS1 || 
                GameDataManager.GameType == GameDataManager.GameTypes.DS1R))
            {
                path = GetFevPathFromInterroot(name, isDs1Dlc: true);
            }
            LoadFEV(path);
        }

        public static void LoadMainFEVs()
        {
            if (GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
                GameDataManager.GameType == GameDataManager.GameTypes.DS1R)
            {
                LoadInterrootFEV("main");
                var dlc = GetFevPathFromInterroot("main", isDs1Dlc: true);
                if (File.Exists(dlc))
                    LoadFEV(dlc);
            }
            else if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
            {
                LoadInterrootFEV("main");

                var dlc1 = GetFevPathFromInterroot("main_dlc1");
                var dlc2 = GetFevPathFromInterroot("main_dlc2");

                if (File.Exists(dlc1))
                    LoadFEV(dlc1);

                if (File.Exists(dlc2))
                    LoadFEV(dlc2);
            }
            else if (GameDataManager.GameType == GameDataManager.GameTypes.SDT)
            {
                LoadInterrootFEV("main");
            }
            else throw new NotImplementedException();
        }

        public static bool PlaySE(int category, int id, Func<Vector3> getPosFunc = null)
        {
            if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 || 
                GameDataManager.GameType == GameDataManager.GameTypes.DS1R || 
                GameDataManager.GameType == GameDataManager.GameTypes.DS3 || 
                GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            {
                return false;
            }

            string soundName = null;
            if (category == 0)
                soundName = $"a{id:D9}";
            else if (category == 1)
                soundName = $"c{id:D9}";
            else if (category == 2)
                soundName = $"s{id:D9}";
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

            foreach (var fevName in LoadedFEVs)
            {
                if (PlayEvent(GetEventPath(fevName, soundName), getPosFunc))
                {
                    return true;
                }
            }

            return false;
        }

        public static void InitTest()
        {
            Main.WinForm.Invoke(new Action(() =>
            {
                ERRCHECK(result = FMOD.Event_Factory.EventSystem_Create(ref _eventSystem));
                ERRCHECK(result = _eventSystem.init(MAX_CHANNELS, FMOD.INITFLAGS.NORMAL, (IntPtr)null, FMOD.EVENT_INITFLAGS.NORMAL));
                ERRCHECK(result = _eventSystem.getSystemObject(ref _system));
            }));
        }

        public static void UpdateInterroot()
        {
            Main.WinForm.Invoke(new Action(() =>
            {
                ERRCHECK(result = _eventSystem.setMediaPath(GetDirWithBackslash(
                Utils.Frankenpath(GameDataManager.InterrootPath, "sound"))));
            }));
        }

        public static void Update()
        {
            Main.WinForm.Invoke(new Action(() =>
            {
                
                //_eventSystem.set3DListenerAttributes(listener: 0, ref pos, ref vel, ref forward, ref up);Vector3.Transform(

                Vector3 pos = (GFX.World.CameraTransform.Position - Vector3.Transform(Vector3.Zero, GFX.World.WorldMatrixMOD)) * new Vector3(-1, 1, 1);
                Vector3 vel = Vector3.Zero;// (pos - listenerPosOnPreviousFrame) / Main.DELTA_UPDATE;
                Vector3 up = Vector3.TransformNormal(Vector3.Up, GFX.World.CameraTransform.RotationMatrix);
                Vector3 forward = Vector3.TransformNormal(Vector3.Forward, GFX.World.CameraTransform.RotationMatrix);

                VECTOR posVec = new VECTOR(pos);
                VECTOR velVec = new VECTOR(vel);
                VECTOR upVec = new VECTOR(up);
                VECTOR forwardVec = new VECTOR(forward);

                ERRCHECK(result = _eventSystem.set3DListenerAttributes(0, ref posVec, ref velVec, ref forwardVec, ref upVec));

                List<FmodEventUpdater> finishedEvents = new List<FmodEventUpdater>();
                foreach (var evt in _eventsToUpdate)
                {
                    evt.Update(Main.DELTA_UPDATE);
                    if (evt.EventIsOver)
                        finishedEvents.Add(evt);
                }
                foreach (var evt in finishedEvents)
                {
                    _eventsToUpdate.Remove(evt);
                }

                ERRCHECK(result = _eventSystem.update());

                listenerPosOnPreviousFrame = pos;
            }));
        }

        public static void Purge()
        {
            Main.WinForm.Invoke(new Action(() =>
            {
                ERRCHECK(result = _eventSystem.unload());
                _loadedFEVs.Clear();
            }));
        }

        private static bool PlayEvent(string eventPath, Func<Vector3> getPosFunc = null)
        {
            bool result = false;
            Main.WinForm.Invoke(new Action(() =>
            {
                FMOD.Event newEvent = null;
                var evtResult = _eventSystem.getEvent(eventPath, FMOD.EVENT_MODE.DEFAULT, ref newEvent);
                if (evtResult == RESULT.ERR_EVENT_NOTFOUND)
                {
                    result = false;
                }
                else if (evtResult == RESULT.ERR_EVENT_FAILED)
                {
                    result = false;
                }
                else
                {
                    //if (newEvent != null)
                    //    eventList.Add(new FEVEvent(eventPath, newEvent));

                    ERRCHECK(evtResult);
                    ERRCHECK(evtResult = newEvent.setVolume(BaseSoundVolume * AdjustSoundVolume));

                    if (getPosFunc != null)
                    {
                        _eventsToUpdate.Add(new FmodEventUpdater(newEvent, getPosFunc));
                    }

                    ERRCHECK(evtResult = newEvent.start());
                    result = true;
                }
            }));

            return result;
        }

        public static void Shutdown()
        {
            Main.WinForm.Invoke(new Action(() =>
            {
                ERRCHECK(result = _eventSystem.release());
            }));
        }

        public static void StopAllSounds()
        {
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
