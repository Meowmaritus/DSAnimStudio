using DSAnimStudio.ImguiOSD;
using FMOD;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewFmodIns
    {
        public zzz_DocumentIns ParentDocument;
        public NewFmodIns(zzz_DocumentIns doc)
        {
            ParentDocument = doc;
        }

        private object _lock_MediaRoot = new object();

        private object _lock_LoadedFevList = new object();

        public bool IsInitialized => initialised;

        private static bool eventSystemCreated => _eventSystem?.Created == true;
        private static bool initialised = false;
        private static bool eventstart = false;
        private static bool exit = false;

        private FMOD.RESULT result;
        //FMOD.EventGroup group = null;
        //private FMOD.EventProject project = null;
        //private FMOD.Sound fsb = null;
        //private FMOD.System sys;

        public const int MAX_CHANNELS = 32;

        //public float BaseSoundVolume = 0.5f;


        public string MediaPath = null;

        public string StatusString = "FMOD: (Not Running)";

        private static FMOD.System _system = null;

        private static FMOD.EventSystem _eventSystem = null;




        public enum FmodLanguageTypes
        {
            English,              //BB:eng/engb, SDT:enu/enus
            Japanese,             //BB:jaj/jajp, SDT:jaj/jajp
            Spanish_Europe,       //BB:ese/eses, SDT:ese/eses
            French,               //BB:frf/frfr, SDT:frf/frfr
            Italian,              //BB:iti/itit, SDT:iti/itit
            German,               //BB:ded/dede, SDT:ded/dede
            Spanish_LatinAmerica, //BB:esa/esar
            Portuguese_Brazil,    //BB:ptb/ptbr
        }

        public FmodLanguageTypes LanguageType = FmodLanguageTypes.English;
        public bool HasSetInitLanguageType = false;

        public void CheckSetInitLanguageType()
        {
            if (!HasSetInitLanguageType)
            {
                // Set this before calling SwitchLanguage since SwitchLanguage will try to set init Language type again
                HasSetInitLanguageType = true;
                var newLanguageType = ((ParentDocument.GameRoot.GameType is SoulsGames.SDT) ? FmodLanguageTypes.Japanese : FmodLanguageTypes.English);
                SwitchLanguage(newLanguageType);
            }
        }

        public static string GetFmodLanguageSuffix(SoulsGames game, FmodLanguageTypes lang)
        {
            var key = GetFmodLanguageKey(game, lang);
            return key?.Substring(0, 3);
        }

        public static string GetFmodLanguageKey(SoulsGames game, FmodLanguageTypes lang)
        {
            switch (lang)
            {
                case FmodLanguageTypes.English:
                    if (game is SoulsGames.BB)
                        return "engb";
                    else if (game is SoulsGames.SDT)
                        return "enus";
                    break;
                case FmodLanguageTypes.Japanese:
                    if (game is SoulsGames.BB or SoulsGames.SDT)
                        return "jajp";
                    break;
                case FmodLanguageTypes.Spanish_Europe:
                    if (game is SoulsGames.BB or SoulsGames.SDT)
                        return "eses";
                    break;

                case FmodLanguageTypes.French:
                    if (game is SoulsGames.BB or SoulsGames.SDT)
                        return "frfr";
                    break;

                case FmodLanguageTypes.Italian:
                    if (game is SoulsGames.BB or SoulsGames.SDT)
                        return "itit";
                    break;

                case FmodLanguageTypes.German:
                    if (game is SoulsGames.BB or SoulsGames.SDT)
                        return "dede";
                    break;


                case FmodLanguageTypes.Spanish_LatinAmerica:
                    if (game is SoulsGames.BB)
                        return "esar";
                    break;

                case FmodLanguageTypes.Portuguese_Brazil:
                    if (game is SoulsGames.BB)
                        return "ptbr";
                    break;



            }

            return null;
        }

        public bool IsFevLoaded(string fevName)
        {
            if (fevName == null)
                return false;
            string fevKey = Utils.GetShortIngameFileName(fevName).ToLower();
            bool isLoaded = false;
            lock (_lock_LoadedFevList)
            {
                if (_loadedFEVs_FullPaths.ContainsKey(fevKey) && !File.Exists($@"{LoadedFEVs_FullPaths[fevKey]}\{fevKey}.fev"))
                {
                    //TODO: Add unloadFev and call here?
                    //_loadedFEVs.Remove(fevKey);
                    _loadedFEVs_FullPaths.Remove(fevKey);
                }

                isLoaded = _loadedFEVs_FullPaths.ContainsKey(fevKey);
            }

            return isLoaded;
        }

        private Dictionary<string, string> _loadedFEVs_FullPaths = new Dictionary<string, string>();
        //private List<string> _loadedFEVs = new List<string>();

        // public IReadOnlyList<string> LoadedFEVs
        // {
        //     get
        //     {
        //         IReadOnlyList<string> result = null;
        //         lock (_lock_LoadedFevList)
        //         {
        //             result = _loadedFEVs.ToList();
        //         }
        //         return result ?? new List<string>();
        //     }
        // }

        //public List<string> LastLoadedFEVs = new List<string>();

        public IReadOnlyDictionary<string, string> LoadedFEVs_FullPaths
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

        //private List<FmodEventUpdater> _eventsToUpdate = new List<FmodEventUpdater>();

        //public object _lock_eventsToUpdate = new object();
        //public IReadOnlyList<FmodEventUpdater> EventsToUpdate => _eventsToUpdate;

        private Vector3 listenerPosOnPreviousFrame;

        public int FloorMaterial = 1;

        public Dictionary<int, string> FloorMaterialNames = new Dictionary<int, string>();

        public DebugPrimitives.DbgPrimWireArrow DbgPrimCamPos;

        public void Update()
        {
            if (!initialised || !eventSystemCreated)
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
                try
                {
                    if (Event?.eventSysParent?.Created == true)
                    {
                        var res = Event.stop(immediate);

                        if (res == RESULT.ERR_INVALID_HANDLE)
                        {
                            EventIsOver = true;
                            return;
                        }
                    }
                    else
                    {
                        EventIsOver = true;
                    }
                }
                catch
                {
                    EventIsOver = true;
                }
            }

            public void Pause()
            {
                if (EventIsOver)
                    return;

                if (Event?.eventSysParent?.Created == true)
                {
                    var res = Event.setPaused(true);
                    if (res == RESULT.ERR_INVALID_HANDLE)
                    {
                        EventIsOver = true;
                        return;
                    }
                }
                else
                {
                    EventIsOver = true;
                }
            }

            public void Resume()
            {
                if (EventIsOver)
                    return;

                if (Event?.eventSysParent?.Created == true)
                {
                    var res = Event.setPaused(false);

                    if (res == RESULT.ERR_INVALID_HANDLE)
                    {
                        EventIsOver = true;
                        return;
                    }
                }
                else
                {
                    EventIsOver = true;
                }
            }

            public void StartPlay()
            {
                if (Event?.eventSysParent?.Created == true)
                {
                    var res = Event.start();
                    if (res == RESULT.ERR_INVALID_HANDLE)
                        EventIsOver = true;
                    hasntStartedPlayingYet = false;
                }
                else
                {
                    EventIsOver = true;
                }
            }

            public void Update(zzz_SoundManagerIns soundMan, float deltaTime, Matrix world, Vector3 position)
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

                if (Event?.eventSysParent?.Created != true)
                {
                    EventIsOver = true;
                    return;
                }

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
                    evtRes = Event.setVolume(soundMan.FmodBaseSoundVolume * (soundMan.AdjustSoundVolume / 100));

                    //oldPos = CurrentPos;
                }

                IsFirstFrameExisting = false;
            }
        }


        public FMOD.EventProject GetEventProject(string name)
        {
            FMOD.EventProject proj = null;
            var numProjects = 0;
            result = _eventSystem.getNumProjects(ref numProjects);
            ERRCHECK(result = _eventSystem.getProjectByIndex(0, ref proj));
            return proj;
        }

        public void LoadFloorMaterialNamesFromInterroot()
        {
            FloorMaterialNames.Clear();

            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return;
            //}

            foreach (var id in ParentDocument.ParamManager.HitMtrlParamEntries)
            {
                FloorMaterialNames.Add((int)id.Key, $"[{id.Key}] " + (!string.IsNullOrEmpty(id.Value) ? id.Value : $"HitMtrlParam {id.Key}"));
            }

            string[] defsFile = null;
            if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                defsFile = File.ReadAllLines(Utils.Frankenpath(Main.Directory, @"RES\HitMtrlParamNames.DS1.txt"));
            }
            else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
            {
                defsFile = File.ReadAllLines(Utils.Frankenpath(Main.Directory, @"RES\HitMtrlParamNames.DES.txt"));
            }
            else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
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

        public int ArmorMaterial;

        //public enum ArmorMaterialType
        //{
        //    Plates = 56,
        //    Chains = 57,
        //    Cloth = 58,
        //    None = 59,
        //}




        private object _lock_LoadFEV = new object();
        public bool LoadFEV(string fullFevPath)
        {
            if (!initialised)
                return false;
            bool funcResult = true;
            lock (_lock_LoadFEV)
            {

                //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
                //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
                //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
                //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
                //{
                //    return false;
                //}

                string fevKey = Utils.GetShortIngameFileName(fullFevPath).ToLower();

                ParentDocument.SoundManager.RegisterAdditionalSoundBank(fevKey);

               




                bool alreadyLoaded = false;
               
                    
                        lock (_lock_LoadedFevList)
                        {
                            bool alreadyLoaded2_FuckingWorkPieceOfShit = false;
                            if (_loadedFEVs_FullPaths.ContainsKey(fevKey))
                            {
                                alreadyLoaded = true;
                                alreadyLoaded2_FuckingWorkPieceOfShit = true;
                            }

                            if (alreadyLoaded || alreadyLoaded2_FuckingWorkPieceOfShit)
                            {
                                return true;
                            }

                            if (!File.Exists(fullFevPath))
                            {
                                return false;
                            }


                            UpdateMediaRoot(Path.GetDirectoryName(fullFevPath));
                            
                            Main.WinForm.Invoke(new Action(() =>
                            {
                                result = _eventSystem.load(fevKey + ".fev");
                                if (result != RESULT.ERR_FILE_NOTFOUND)
                                {
                                    ERRCHECK(result);
                                }
                            }));
                            


                            //_loadedFEVs.Add(fevKey);
                            //_loadedFEVs_FullPaths[fevKey] = Path.GetDirectoryName(fullFevPath);
                            _loadedFEVs_FullPaths.Add(fevKey, Path.GetDirectoryName(fullFevPath));
                            ParentDocument.SoundManager.RegisterAdditionalSoundBank(fevKey);
                        }
                        
                

                funcResult = true;
            }

            return funcResult;
        }

        //public bool PlayEventInFEV(string fevFilePath, string eventName)
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
        public string GetFevPathFromInterroot(string name, bool isDs1Dlc = false)
        {
            //if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
            //    ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            //    return $@"/sound/{(isDs1Dlc ? "fdlc" : "frpg")}_{name}.fev";
            //else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
            //    return $@"/sound/fdp_{name}.fev";
            //else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
            //    return $@"/_DSAS_CONVERTED_SOUNDS/sprj_{name}.fev";
            //else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
            //    return $@"/sound/{name}.fev";
            //else return null;
            if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                return $@"/sound/{name}.fev";
            else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
                return $@"/sound/{name}.fev";
            else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
                return $@"/_DSAS_CONVERTED_SOUNDS/{name}.fev";
            else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                return $@"/sound/{name}.fev";
            else return null;
        }

        public void SwitchLanguage(FmodLanguageTypes lang)
        {
            if (lang != LanguageType)
            {
                LanguageType = lang;
                if (eventSystemCreated && _eventSystem != null)
                {
                    _eventSystem.setLanguage(GetFmodLanguageKey(ParentDocument.GameRoot.GameType, lang));
                }
                List<string> fevsToRestore = null;
                lock (_lock_LoadedFevList)
                {
                    fevsToRestore = _loadedFEVs_FullPaths.Keys.ToList();
                }
                Purge();
                foreach (var f in fevsToRestore)
                {
                    LoadInterrootFEV(f);
                }
            }
        }

        public void LoadInterrootFEV(string name)
        {
            CheckSetInitLanguageType();

            var langSuffix = GetFmodLanguageSuffix(ParentDocument.GameRoot.GameType, LanguageType);

            if (ParentDocument.GameRoot.GameType is SoulsGames.BB)
            {
                NewFmodManager.PreProcessBloodborneFEV(ParentDocument, name, langSuffix);
            }

            if (name == null)
                return;

            if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6)
                return;

            if (!initialised)
                return;

            bool alreadyLoaded = false;
            lock (_lock_LoadedFevList)
            {
                if (_loadedFEVs_FullPaths.ContainsKey(Utils.GetShortIngameFileName(name).ToLower()))
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
            if (ParentDocument.GameData.FileExists(path))
            {
                var unpackedFile = ParentDocument.GameData.ReadFileAndTempUnpack(path, warningOnFail: true);
                ParentDocument.GameData.ReadFileAndTempUnpack(path.Substring(0, path.Length - 4) + ".fsb", warningOnFail: false);

                
                //var langSuffix = GetFmodLanguageSuffix(ParentDocument.GameRoot.GameType, LanguageType);

                if (langSuffix != null)
                {
                    ParentDocument.GameData.ReadFileAndTempUnpack(path.Substring(0, path.Length - 4) + $"_{langSuffix}.fsb", warningOnFail: false);
                }

                if (unpackedFile != null)
                    LoadFEV(unpackedFile.AbsolutePath);


            }

            //LoadFEV(path);
            if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                path = GetFevPathFromInterroot(name, isDs1Dlc: true);
                if (ParentDocument.GameData.FileExists(path))
                {
                    var unpackedFileDLC = ParentDocument.GameData.ReadFileAndTempUnpack(path);
                    ParentDocument.GameData.ReadFileAndTempUnpack(path.Substring(0, path.Length - 4) + ".fsb");
                    if (unpackedFileDLC != null)
                        LoadFEV(unpackedFileDLC.AbsolutePath);
                }
            }
        }

        //public List<string> ListOfAllFEVs = new List<string>();

        //public void LoadMainFEVs()
        //{
        //    if (ParentDocument.SoundManager.EngineType != zzz_SoundManagerIns.EngineTypes.FMOD)
        //        return;
        //    if (!initialised)
        //        return;

        //    ListOfAllFEVs = ParentDocument.GameData.SearchFiles("/sound", @".*\.fev");

        //    if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
        //        ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
        //    {
        //        LoadInterrootFEV("main");
        //        //dlc main auto loaded for ds1
        //    }
        //    else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
        //    {
        //        LoadInterrootFEV("main");
        //        LoadInterrootFEV("main_dlc1");
        //        LoadInterrootFEV("main_dlc2");
        //    }
        //    else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
        //    {
        //        LoadInterrootFEV("main");
        //    }
        //    else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
        //    {
        //        LoadInterrootFEV("main");
        //    }
        //}

        public string GetSoundName(int category, int id)
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

        public FmodEventUpdater PlaySE(int category, int id, float lifetime)
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

        //public void StopAllSESlotsNotInList(List<int> slots, bool immediate)
        //{
        //    foreach (var snd in _eventsToUpdate)
        //    {
        //        if (snd.Slot >= 0 && !slots.Contains(snd.Slot))
        //        {
        //            snd.Stop(immediate);
        //        }
        //    }
        //}

        //public void StopSESlot(int slot, bool immediate)
        //{
        //    foreach (var snd in _eventsToUpdate)
        //    {
        //        if (snd.Slot == slot)
        //            snd.Stop(immediate);
        //    }
        //}

        //public bool IsSlotAlreadyPlaying(int slot)
        //{
        //    foreach (var snd in _eventsToUpdate)
        //    {
        //        if (snd.Slot == slot)
        //            return true;
        //    }
        //    return false;
        //}

        //public string GetEventNameOfSlot(int slot)
        //{
        //    foreach (var snd in _eventsToUpdate)
        //    {
        //        if (snd.Slot == slot)
        //            return snd.EventName;
        //    }
        //    return null;
        //}


        private FmodEventUpdater PlaySE(string seEventName, float lifetime)
        {
            return PlayEvent(seEventName, lifetime);
        }

        public void InitTest()
        {
            if (initialised)
                return;

            if (zzz_SoundManagerIns.SOUND_DISABLED)
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
                    //eventSystemCreated = true;
                }
                else
                {
                    ERRCHECK(result);
                }

                result = _eventSystem.init(MAX_CHANNELS, FMOD.INITFLAGS.NORMAL, (IntPtr)null, FMOD.EVENT_INITFLAGS.NORMAL);
                if (result == RESULT.ERR_OUTPUT_INIT)
                {
                    DialogManager.DialogOK(null, "Failed to initialize audio output. " +
                        "Make sure you have an audio device connected and working and " +
                        "that no other app is taking exclusive control of the device.\n\n" +
                        "Once you free the device, go to the 'Sound' window and check 'Enable Audio System'\n" +
                        "or restart DS Anim Studio for Wwise games (Elden Ring and later)");
                    zzz_SoundManagerIns.SOUND_DISABLED = true;
                    initialised = false;
                }
                else if (result == RESULT.OK)
                {
                    initialised = true;
                    zzz_SoundManagerIns.SOUND_DISABLED = false;
                }
                else
                {
                    ERRCHECK(result);
                }

                //RESULT langResult;
                //if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.BB)
                //{
                //    langResult = _eventSystem.setLanguage("engb");
                //}

                ERRCHECK(result = _eventSystem.getSystemObject(ref _system));

                DbgPrimCamPos = new DebugPrimitives.DbgPrimWireArrow(Transform.Default, Color.Lime);
            }));
        }

        public void UpdateInterroot()
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
                Utils.Frankenpath(ParentDocument.GameRoot.InterrootPath, ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB ? "_DSAS_CONVERTED_SOUNDS" : "sound"));
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

        public void UpdateMediaRoot(string mediaRoot)
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

        public void Purge()
        {
            lock (_lock_LoadedFevList)
            {
                //_loadedFEVs.Clear();
                _loadedFEVs_FullPaths.Clear();
            }

            if (eventSystemCreated)
            {
                try
                {
                    ERRCHECK(result = _eventSystem.unload());
                }
                catch
                {

                }
                //eventSystemCreated = false;
            }

            //if (!(GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS1R ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
            //   GameDataManager.GameType == GameDataManager.GameTypes.SDT))
            //{
            //    return;
            //}

            //Main.WinForm.Invoke(new Action(() =>
            //{
            //    ERRCHECK(result = _eventSystem.unload());
            //    lock (_lock_LoadedFevList)
            //    {
            //        _loadedFEVs.Clear();
            //        _loadedFEVs_FullPaths.Clear();
            //    }
            //}));
            
        }

        private object _lock_MatchBanksToSoundMan = new object();
        private void MATCH_BANKS_TO_SOUND_MAN()
        {
            lock (_lock_MatchBanksToSoundMan)
            {
                var banks = ParentDocument.SoundManager.GetAdditionalSoundBankNames();
                List<string> fevsToUnload = new List<string>();
                List<string> fevsToLoad = new List<string>();
                lock (_lock_LoadedFevList)
                {
                    foreach (var kvp in _loadedFEVs_FullPaths)
                    {
                        if (!banks.Contains(kvp.Key))
                            fevsToUnload.Add(kvp.Key);
                    }

                    foreach (var b in banks)
                    {
                        if (!_loadedFEVs_FullPaths.ContainsKey(b))
                            fevsToLoad.Add(b);
                    }

                }

                if (fevsToUnload.Count > 0)
                {
                    List<string> fevNamesToRestore = new List<string>();
                    lock (_lock_LoadedFevList)
                    {
                        fevNamesToRestore.AddRange(_loadedFEVs_FullPaths.Keys.ToList());
                    }

                    foreach (var f in fevsToUnload)
                    {
                        if (fevNamesToRestore.Contains(f))
                            fevNamesToRestore.Remove(f);
                    }

                    Purge();
                    Shutdown();
                    InitTest();
                    foreach (var fev in fevNamesToRestore)
                    {
                        LoadInterrootFEV(fev);
                    }
                }

                foreach (var fev in fevsToLoad)
                {
                    LoadInterrootFEV(fev);
                }

            }
        }

        private FmodEventUpdater PlayEvent(string eventName, float lifetime)
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

            // var testTask = Main.MainThreadLazyDispatchTask(new Task<FmodEventUpdater>(() =>
            // {
            //     
            // }));
            //
            // var testTaskActualTask = (testTask.Act as Task<FmodEventUpdater>);
            // testTaskActualTask.Wait();
            // result = testTaskActualTask.Result;

            MATCH_BANKS_TO_SOUND_MAN();

            FMOD.EventProject evProject = null;

            bool foundEvent = false;

            FMOD.Event newEvent = null;
            string newEvent_FullFevPath = null;

            var loadedFevsCopy = LoadedFEVs_FullPaths.Keys.ToList();

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
                                newEvent.eventSysParent = _eventSystem;
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
                return null;
            }

            lock (_lock_MediaRoot)
            {
                UpdateMediaRoot(newEvent_FullFevPath);

                ERRCHECK(newEvent.setVolume(ParentDocument.SoundManager.FmodBaseSoundVolume * (ParentDocument.SoundManager.AdjustSoundVolume / 100)));

                    

                result = new FmodEventUpdater(newEvent, eventName, lifetime);

                //ERRCHECK(newEvent.start());
            }

            return result;
            
            //return result;
        }

        public void Shutdown()
        {
            //if (ParentDocument.GameRoot.GameTypeUsesWwise)
            //    Wwise.DisposeAll();
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

            if (!initialised)
                return;

            if (!eventSystemCreated)
                return;

            bool failed = false;
            if (Main.WinForm != null)
            {
                try
                {
                    Main.WinForm.Invoke(new Action(() =>
                    {
                        ERRCHECK(result = _eventSystem.release());
                        //foreach (var kvp in _eventProjects)
                        //{
                        //    ERRCHECK(result = kvp.Value.release());
                        //}
                        initialised = false;
                        //eventSystemCreated = false;
                    }));
                }
                catch
                {
                    failed = true;
                }
            }

            if (failed)
            {
                try
                {
                    _eventSystem.release();
                    //eventSystemCreated = false;
                }
                catch
                {

                }
            }
        }

        public void StopAllSounds()
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

        private void ERRCHECK(FMOD.RESULT result)
        {
            return;
            if (result != FMOD.RESULT.OK)
            {
                string e = result + " - " + FMOD.Error.String(result);
                zzz_NotificationManagerIns.PushNotification(
                    $"FMOD (sound engine) error: {e}\n" +
                    $"STACK TRACE:\n{System.Environment.StackTrace}",
                    color: zzz_NotificationManagerIns.ColorError, showDuration: 10);
            }
        }

        private string GetDirWithBackslash(string dir)
        {
            if (!dir.EndsWith("\\"))
                dir += "\\";
            return dir;
        }
    }
}
