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

        public static object _lock_eventsToUpdate = new object();
        public static IReadOnlyList<FmodEventUpdater> EventsToUpdate => _eventsToUpdate;

        private static Vector3 listenerPosOnPreviousFrame;

        public static int FloorMaterial = 1;

        public static Dictionary<int, string> FloorMaterialNames = new Dictionary<int, string>();

        public static DebugPrimitives.DbgPrimWireArrow DbgPrimCamPos;



        public static void Update()
        {
            if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
               GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            {
                return;
            }

            Main.WinForm.Invoke(new Action(() =>
            {

                //_eventSystem.set3DListenerAttributes(listener: 0, ref pos, ref vel, ref forward, ref up);Vector3.Transform(

                Vector3 pos = (GFX.World.CameraTransform.Position /* - Vector3.Transform(Vector3.Zero, GFX.World.WorldMatrixMOD)*/ ) * new Vector3(1, 1, 1);
                Vector3 vel = Vector3.Zero;// (pos - listenerPosOnPreviousFrame) / Main.DELTA_UPDATE;
                Vector3 up = GFX.World.GetScreenSpaceUpVector();// Vector3.TransformNormal(Vector3.Up, GFX.World.CameraTransform.RotationMatrix);
                Vector3 forward = -GFX.World.GetScreenSpaceFowardVector();

                pos = Vector3.Transform(pos, Matrix.Invert(GFX.World.WorldMatrixMOD));

                forward = Vector3.TransformNormal(forward, GFX.World.WorldMatrixMOD) * new Vector3(1, 1, -1);

                up = Vector3.TransformNormal(up, GFX.World.WorldMatrixMOD) * new Vector3(1, 1, -1);



                DbgPrimCamPos.Transform = new Transform(Matrix.CreateWorld(Vector3.Zero, forward, up));


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
            public FmodEventUpdater(FMOD.Event evt, Func<Vector3> getPosFunc, string name)
            {
                EventName = name;
                Event = evt;
                GetPosFunc = getPosFunc;
            }
            public void Update(float deltaTime, Matrix world)
            {
                if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
                   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
                   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
                   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
                {
                    return;
                }

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
                    var position = Vector3.Transform(((GetPosFunc?.Invoke() ?? Vector3.Zero) * new Vector3(1, 1, 1)), world);
                    var velocity = Vector3.Zero;// (position - oldPos) / deltaTime;

                    // Flatten position to be on a flat plane
                    //position.Y = 0;

                    FMOD.VECTOR posVec = new VECTOR(position);
                    FMOD.VECTOR velVec = new VECTOR(velocity);

                    // If it fails due to the event being released, it's fine. No weirdness should happen.
                    evtRes = Event.set3DAttributes(ref posVec, ref velVec);
                    evtRes = Event.setVolume(BaseSoundVolume * AdjustSoundVolume);

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

            if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
               GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            {
                return;
            }

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

                if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
                   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
                   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
                   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
                {
                    return;
                }

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


        
        public static IReadOnlyList<string> LoadedFEVs => _loadedFEVs;

        private static bool LoadFEV(string fullFevPath)
        {
            if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
               GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            {
                return false;
            }

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

        public static bool PlayEventInFEV(string fevFilePath, string eventName)
        {
            if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
               GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            {
                return false;
            }

            var foundFev = LoadFEV(fevFilePath);
            if (!foundFev)
                return false;
            else
                return PlayEvent(eventName);
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
            else return null;
        }

        public static void LoadInterrootFEV(string name)
        {
            if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
               GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            {
                return;
            }

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

            return PlaySE(soundName, getPosFunc);
        }



        public static bool PlaySE(string seEventName, Func<Vector3> getPosFunc = null)
        {
            foreach (var fevName in LoadedFEVs)
            {
                if (PlayEvent(seEventName, getPosFunc))
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

                DbgPrimCamPos = new DebugPrimitives.DbgPrimWireArrow("FMOD Camera", Transform.Default, Color.Lime);
                DbgPrimCamPos.Category = DebugPrimitives.DbgPrimCategory.SoundEvent;
            }));
        }

        public static void UpdateInterroot()
        {
            if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
               GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            {
                return;
            }

            Main.WinForm.Invoke(new Action(() =>
            {
                ERRCHECK(result = _eventSystem.setMediaPath(GetDirWithBackslash(
                Utils.Frankenpath(GameDataManager.InterrootPath, "sound"))));
            }));
        }

        public static void Purge()
        {
            if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
               GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            {
                return;
            }

            Main.WinForm.Invoke(new Action(() =>
            {
                ERRCHECK(result = _eventSystem.unload());
                _loadedFEVs.Clear();
            }));
        }

        private static bool PlayEvent(string eventName, Func<Vector3> getPosFunc = null)
        {
            if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
               GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            {
                return false;
            }

            bool result = false;
            Main.WinForm.Invoke(new Action(() =>
            {
                FMOD.EventProject evProject = null;

                bool foundEvent = false;

                FMOD.Event newEvent = null;

                foreach (var fevName in LoadedFEVs)
                {
                    var fres = _eventSystem.getProject(fevName, ref evProject);
                    if (fres == RESULT.OK)
                    {
                        int groupCount = 0;
                        fres = evProject.getNumGroups(ref groupCount);
                        if (fres == RESULT.OK)
                        {
                            for (int i = 0; i < groupCount; i++)
                            {
                                FMOD.EventGroup innerGroup = null;
                                fres = evProject.getGroupByIndex(i, cacheevents: false, ref innerGroup);
                                if (fres == RESULT.OK)
                                {
                                    fres = innerGroup.getEvent(eventName, EVENT_MODE.DEFAULT, ref newEvent);
                                    if (fres == RESULT.OK)
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

                ERRCHECK(newEvent.setVolume(BaseSoundVolume * AdjustSoundVolume));

                if (getPosFunc != null)
                {
                    lock (_lock_eventsToUpdate)
                    {
                        _eventsToUpdate.Add(new FmodEventUpdater(newEvent, getPosFunc, eventName));
                    }
                }

                ERRCHECK(newEvent.start());
                result = true;
            }));

            return result;
        }

        public static void Shutdown()
        {
            if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
               GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            {
                return;
            }

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
            if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
               GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
               GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            {
                return;
            }

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
