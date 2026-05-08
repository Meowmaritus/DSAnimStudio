using DSAnimStudio.ImguiOSD;
using FMOD;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DSAnimStudio.ImguiOSD.Window;
using static DSAnimStudio.ManagerAction;

namespace DSAnimStudio.SafeFmod
{
    public partial class SafeFmodEventSys : IDisposable
    {
        private FMOD.EventSystem _eventSystem = null;
        private FMOD.System _system = null; // This is included in _eventSystem. Only call release() on EventSystem.
        private FMOD.RESULT fres;

        private bool eventSystemPointerIsValid = false;
        private bool eventSystemHasBeenInitialized = false;

        public string currentMediaPath = null;

        public readonly int MaxChannels;

        private bool disposed = false;

        private SafeSingleThreadDispatcher Dispatcher = new SafeSingleThreadDispatcher();

        private Dictionary<string, string> loadedFevsAndTheirDirectories = new Dictionary<string, string>();

        private Dictionary<long, EventInfo> eventHandles = new Dictionary<long, EventInfo>();

        private long nextHandle = 0;



        public SafeFmodEventSys(int maxChannels)
        {
            MaxChannels = maxChannels;

            Dispatcher.Invoke(() =>
            {

                fres = FMOD.Event_Factory.EventSystem_Create(ref _eventSystem);

                eventSystemPointerIsValid = SafeFmodUtils.AssertResultOK(fres, "Failed to create FMOD EventSystem.");

                fres = _eventSystem.init(MaxChannels, FMOD.INITFLAGS.NORMAL, (IntPtr)null, FMOD.EVENT_INITFLAGS.NORMAL);
                if (fres == FMOD.RESULT.ERR_OUTPUT_INIT)
                {
                    DialogManager.DialogOK(null, "Failed to initialize audio output. " +
                        "Make sure you have an audio device connected and working and " +
                        "that no other app is taking exclusive control of the device.\n\n" +
                        "Once you free the device, go to the 'Sound' window and check 'Enable Audio System'\n" +
                        "or restart DS Anim Studio for Wwise games (Elden Ring and later)");
                    zzz_SoundManagerIns.SOUND_DISABLED = true;
                    eventSystemHasBeenInitialized = false;
                }
                else if (fres == FMOD.RESULT.OK)
                {
                    eventSystemHasBeenInitialized = true;
                    zzz_SoundManagerIns.SOUND_DISABLED = false;
                }
                else
                {
                    SafeFmodUtils.AssertResultOK(fres, "Failed to initialize FMOD EventSystem after creating it.");
                }



                if (eventSystemPointerIsValid)
                {
                    fres = _eventSystem.getSystemObject(ref _system);
                    SafeFmodUtils.AssertResultOK(fres, "Failed to retrieve the underlying FMOD System from the FMOD EventSystem.");
                }

            });

            
            


        }

        public void SwitchLanguage(string fmodLanguageKey)
        {
            Dispatcher.Invoke(() =>
            {
                fres = _eventSystem.setLanguage(fmodLanguageKey);
                SafeFmodUtils.AssertResultOK(fres, "Failed to set language on FMOD EventSystem.");
            });
        }

        public Dictionary<string, string> GetCopyOfLoadedFevsAndTheirDirectories()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Dispatcher.Invoke(() =>
            {
                foreach (var kvp in loadedFevsAndTheirDirectories)
                    result.Add(kvp.Key, kvp.Value);
            });
            return result;
        }

        public void INNER_LoadFEV(string fullFevPath)
        {
            string fevKey = Utils.GetShortIngameFileName(fullFevPath).ToLower();

            if (!loadedFevsAndTheirDirectories.ContainsKey(fevKey))
            {
                string dir = Path.GetDirectoryName(fullFevPath);
                if (INNER_SetMediaPath(dir))
                {
                    fres = _eventSystem.load(fevKey + ".fev");
                    if (fres == FMOD.RESULT.OK)
                    {
                        loadedFevsAndTheirDirectories[fevKey] = dir;
                    }
                    else
                    {
                        SafeFmodUtils.AssertResultOK(fres, "Failed to load FEV.");
                    }

                }
                else
                {
                    Console.WriteLine("???");
                }

            }
        }

        public void LoadFEV(string fullFevPath)
        {
            Dispatcher.Invoke(() =>
            {
                INNER_LoadFEV(fullFevPath);
            });
        }

        //private FMOD.EventProject INNER_GetEventProject(string fevKey)
        //{
        //    FMOD.EventProject evProject = null;
        //    fres = _eventSystem.getProject(fevKey, ref evProject);
        //    if (fres == FMOD.RESULT.OK)
        //    {
        //        return evProject;
        //    }
        //    else
        //    {
        //        SafeFmodUtils.AssertResultOK(fres, $"Failed to get FMOD Event Project '{fevKey}' from Event System.");
        //        return null;
        //    }
            
        //}

        //private int INNER_GetNumGroupsInEventProject(string fevKey, FMOD.EventProject evProject)
        //{
        //    int numGroups = 0;
        //    fres = evProject.getNumGroups(ref numGroups);
        //    if (fres == FMOD.RESULT.OK)
        //    {
        //        return numGroups;
        //    }
        //    else
        //    {
        //        SafeFmodUtils.AssertResultOK(fres, $"Failed to get number of groups from FMOD Event Project '{fevKey}'.");
        //        return 0;
        //    }
        //}

        public void ReleaseEventHandle(long handle)
        {
            Dispatcher.Invoke(() =>
            {


                if (eventHandles.ContainsKey(handle))
                {
                    eventHandles.Remove(handle);
                }


            });
        }

        private long INNER_RecursiveSearchEventGroupForEvent(FMOD.EventGroup group, string eventName, float lifetime)
        {
            FMOD.Event ev = null;
            fres = group.getEvent(eventName, EVENT_MODE.DEFAULT, ref ev);
            if (SafeFmodUtils.AssertResultOK(fres, $"Failed to get FMOD Event '{eventName}' from Group."))
            {
                long newHandle = ++nextHandle;
                eventHandles[newHandle] = new EventInfo(eventName, ev, lifetime);
                return newHandle;
            }

            // If wasn't found in this group, check child groups
            int numInnerGroups = 0;
            fres = group.getNumGroups(ref numInnerGroups);
            if (SafeFmodUtils.AssertResultOK(fres, "Failed to get FMOD Group count from Group."))
            {
                for (int i = 0; i < numInnerGroups; i++)
                {
                    FMOD.EventGroup innerGroup = null;
                    fres = group.getGroupByIndex(i, false, ref innerGroup);
                    if (SafeFmodUtils.AssertResultOK(fres, $"Failed to get FMOD Group {i} from Group."))
                    {
                        var tryFindEventFromInnerGroup = INNER_RecursiveSearchEventGroupForEvent(innerGroup, eventName, lifetime);
                        if (tryFindEventFromInnerGroup != 0)
                        {
                            return tryFindEventFromInnerGroup;
                        }
                    }
                }
            }

            // Didn't find it
            return 0;
        }

        public long CreateEvent(string eventName, float lifetime)
        {
            long foundEventHandle = 0;

            Dispatcher.Invoke(() =>
            {
                

                foreach (var kvp in loadedFevsAndTheirDirectories)
                {
                    FMOD.EventProject evProject = null;
                    fres = _eventSystem.getProject(kvp.Key, ref evProject);
                    if (SafeFmodUtils.AssertResultOK(fres, $"Failed to get number of groups from FMOD Event Project '{kvp.Key}'."))
                    {
                        int numGroups = 0;
                        fres = evProject.getNumGroups(ref numGroups);
                        if (SafeFmodUtils.AssertResultOK(fres, $"Failed to get number of groups from FMOD Event Project '{kvp.Key}'."))
                        {
                            for (int i = 0; i < numGroups; i++)
                            {

                                FMOD.EventGroup evGroup = null;
                                fres = evProject.getGroupByIndex(i, false, ref evGroup);
                                if (SafeFmodUtils.AssertResultOK(fres, $"Failed to get Event Group {i} from FMOD Event Project '{kvp.Key}'."))
                                {
                                    var tryFindEventHandle = INNER_RecursiveSearchEventGroupForEvent(evGroup, eventName, lifetime);
                                    if (tryFindEventHandle != 0)
                                    {
                                        foundEventHandle = tryFindEventHandle;
                                        break;
                                    }
                                }

                            }
                        }
                    }

                    if (foundEventHandle != 0)
                    {
                        // Found event already, no need to keep iterating through FEVs.
                        break;
                    }


                }


            });

            return foundEventHandle;
        }

        public bool SetMediaPath(string newMediaPath)
        {
            bool result = false;
            Dispatcher.Invoke(() =>
            {
                result = INNER_SetMediaPath(newMediaPath);
            });
            return result;
        }

        private bool INNER_SetMediaPath(string newMediaPath)
        {
            if (!newMediaPath.EndsWith("\\"))
                newMediaPath += "\\";
            if (currentMediaPath?.ToLower() != newMediaPath.ToLower())
            {

                if (Directory.Exists(newMediaPath))
                {
                    fres = _eventSystem.setMediaPath(newMediaPath);
                    if (fres == FMOD.RESULT.OK)
                    {
                        currentMediaPath = newMediaPath;
                        return true;
                    }
                    else
                    {
                        return SafeFmodUtils.AssertResultOK(fres, "Failed to set FMOD media path.");
                    }
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return true;
            }

                
        }

        public void StopAllChannels()
        {
            Dispatcher.Invoke(() =>
            {
                INNER_StopAllChannels();
            });
        }

        private void INNER_StopAllChannels()
        {
            for (int i = 0; i < MaxChannels; i++)
            {
                FMOD.Channel channel = null;

                fres = _system.getChannel(i, ref channel);
                if (fres == FMOD.RESULT.OK)
                {
                    fres = channel.stop();
                    if (fres == RESULT.ERR_INVALID_HANDLE)
                        continue;
                    SafeFmodUtils.AssertResultOK(fres, $"Failed to stop FMOD System Channel {i}.");
                }
                else
                {
                    SafeFmodUtils.AssertResultOK(fres, $"Failed to get FMOD System Channel {i}.");
                }
            }


            //int channelsPlaying = 0;
            //result = _system.getChannelsPlaying(ref channelsPlaying);

            //if (result == FMOD.RESULT.OK)
            //{
            //    for (int i = 0; i < MaxChannels; i++)
            //    {
            //        FMOD.Channel channel = null;

            //        result = _system.getChannel(i, ref channel);
            //        if (result == FMOD.RESULT.OK)
            //        {
            //            result = channel.stop();
            //            SafeFmodUtils.AssertResultOK(result, $"Failed to stop FMOD System Channel {i}.");
            //        }
            //        else
            //        {
            //            SafeFmodUtils.AssertResultOK(result, $"Failed to get FMOD System Channel {i}.");
            //        }
            //    }
            //}
            //else
            //{
            //    SafeFmodUtils.AssertResultOK(result, "Failed to get the channels that are playing from the FMOD System.");
            //}


        }

        private void INNER_UnloadAllFevs()
        {
            INNER_StopAllChannels();
            fres = _eventSystem.unload();
            if (SafeFmodUtils.AssertResultOK(fres, "Failed to call FMOD EventSystem unload."))
            {
                loadedFevsAndTheirDirectories.Clear();
            }
        }

        public void UnloadAllFevs()
        {
            Dispatcher.Invoke(() =>
            {
                INNER_UnloadAllFevs();
            });
        }



        public void Update(Vector3 position, Vector3 velocity, Vector3 forward, Vector3 up)
        {
            Dispatcher.Invoke(() =>
            {
                
                FMOD.VECTOR posVec = new FMOD.VECTOR(position);
                FMOD.VECTOR velVec = new FMOD.VECTOR(velocity);
                FMOD.VECTOR upVec = new FMOD.VECTOR(up);
                FMOD.VECTOR forwardVec = new FMOD.VECTOR(forward);



                fres = _eventSystem.set3DListenerAttributes(0, ref posVec, ref velVec, ref forwardVec, ref upVec);
                SafeFmodUtils.AssertResultOK(fres, "Failed to set FMOD EventSystem 3D listener attributes.");



                fres = _eventSystem.update();
                SafeFmodUtils.AssertResultOK(fres, "Failed to call FMOD EventSystem update.");


            });
        }

        public void Event_Stop(long eventHandle, bool immediate)
        {
            Dispatcher.Invoke(() =>
            {
                if (eventHandles.ContainsKey(eventHandle))
                {
                    eventHandles[eventHandle].Stop(immediate);
                }
            });
        }

        public void Event_StartPlay(long eventHandle)
        {
            Dispatcher.Invoke(() =>
            {
                if (eventHandles.ContainsKey(eventHandle))
                {
                    eventHandles[eventHandle].StartPlay();
                }
            });
        }

        public void Event_Pause(long eventHandle)
        {
            Dispatcher.Invoke(() =>
            {
                if (eventHandles.ContainsKey(eventHandle))
                {
                    eventHandles[eventHandle].Pause();
                }
            });
        }

        public void Event_Resume(long eventHandle)
        {
            Dispatcher.Invoke(() =>
            {
                if (eventHandles.ContainsKey(eventHandle))
                {
                    eventHandles[eventHandle].Resume();
                }
            });
        }

        public SoundInstanceFmod.States Event_Update(long eventHandle, float deltaTime, float volume, Matrix world, Vector3 soundLocation)
        {
            // Default to finished, since that would mean deleting the instance in the case of it not having a handle reference anymore.
            SoundInstanceFmod.States state = SoundInstanceFmod.States.Finished;
            Dispatcher.Invoke(() =>
            {

                if (eventHandles.ContainsKey(eventHandle))
                {
                    var eventInfo = eventHandles[eventHandle];
                    eventInfo.Update(deltaTime, volume, world, soundLocation);
                    state = eventInfo.State;
                    
                }

            });
            return state;
        }







        //public void EnsureBankListMatch(Dictionary<string, string> banks)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        bool reinit = false;
        //        foreach (var kvp in loadedFevsAndTheirDirectories)
        //        {
        //            if (!banks.ContainsKey(kvp.Key))
        //            {
        //                reinit = true;
        //                break;
        //            }
        //        }

        //        if (reinit)
        //        {
        //            INNER_UnloadAllFevs();
        //        }

        //        foreach (var kvp in banks)
        //        {
        //            if (!loadedFevsAndTheirDirectories.ContainsKey(kvp.Key))
        //            {
        //                INNER_LoadFEV(kvp.Value);
        //            }
        //        }
        //    });
        //}


        public void Dispose()
        {
            if (!disposed)
            {


                Dispatcher.Invoke(() =>
                {

                    if (eventSystemPointerIsValid)
                    {
                        if (_eventSystem != null)
                        {
                            fres = _eventSystem.release();
                            SafeFmodUtils.AssertResultOK(fres, "Failed to release FMOD EventSystem.");
                        }

                        eventSystemPointerIsValid = false;
                    }

                });

                Dispatcher?.Dispose();
                disposed = true;

            }
            
            
        }
    }
}
