using FMOD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.SafeFmod
{
    public partial class SafeFmodEventSys
    {
        private class EventInfo
        {
            public readonly string EventName; // For debugging
            public readonly float StartingLifetime; // For debugging

            public readonly FMOD.Event Event;
            public float Lifetime = -1;
            
            public SoundInstanceFmod.States State = SoundInstanceFmod.States.HasntStartedYet;

            public EventInfo(string eventName, FMOD.Event ev, float lifetime)
            {
                EventName = eventName;
                Event = ev;
                StartingLifetime = Lifetime = lifetime;
            }

            public void Update(float deltaTime, float volume, Matrix world, Vector3 soundLocation)
            {
                if (Lifetime > 0)
                {
                    Lifetime -= deltaTime;
                    if (Lifetime < 0)
                        Lifetime = 0;

                    if (Lifetime == 0)
                        Stop(false);
                }

                FMOD.EVENT_STATE fmodEventState = FMOD.EVENT_STATE.PLAYING;
                var fres = Event.getState(ref fmodEventState);
                if (fres == FMOD.RESULT.OK)
                {
                    // If the FMOD event says it's in READY state then it means it's not playing yet
                    // So if the EventInfo already started playing it before, this means the actual
                    // FMOD event finished playing (hence how it's READY again).
                    if (fmodEventState == EVENT_STATE.READY && State != SoundInstanceFmod.States.HasntStartedYet)
                    {
                        State = SoundInstanceFmod.States.Finished;
                        return;
                    }

                    // Event is still playing, update the parameters
                    FMOD.VECTOR posVec = new VECTOR(soundLocation);
                    FMOD.VECTOR velVec = new VECTOR(Vector3.Zero);

                    // If it fails due to the event being released, it's fine. No weirdness should happen.
                    fres = Event.set3DAttributes(ref posVec, ref velVec);
                    if (SafeFmodUtils.AssertResultOK(fres, $"Failed to set 3D attributes of FMOD Event '{EventName}'."))
                    {
                        fres = Event.setVolume(volume);
                        SafeFmodUtils.AssertResultOK(fres, $"Failed to set volume of FMOD Event '{EventName}'.");
                    }
                }
                else if (fres == FMOD.RESULT.ERR_INVALID_HANDLE)
                {
                    State = SoundInstanceFmod.States.Finished;
                    return;
                }


            }

            public void StartPlay()
            {
                if (State == SoundInstanceFmod.States.HasntStartedYet)
                {

                    var fres = Event.start();
                    if (fres == FMOD.RESULT.OK)
                    {
                        State = SoundInstanceFmod.States.Playing;
                    }
                    else if (fres == FMOD.RESULT.ERR_INVALID_HANDLE)
                    {
                        State = SoundInstanceFmod.States.Finished;
                    }
                    else
                    {
                        SafeFmodUtils.AssertResultOK(fres, $"Failed to start FMOD Event '{EventName}'.");
                    }

                }
            }


            public void Stop(bool immediate)
            {
                if (State == SoundInstanceFmod.States.HasntStartedYet)
                {
                    // Didn't ever start event, so shouldn't call FMOD to try to stop it.
                    State = SoundInstanceFmod.States.Finished;
                }
                else if (State != SoundInstanceFmod.States.Finished)
                {

                    var fres = Event.stop(immediate);
                    if (fres == FMOD.RESULT.OK)
                    {
                        State = SoundInstanceFmod.States.Finished;
                    }
                    else if (fres == FMOD.RESULT.ERR_INVALID_HANDLE)
                    {
                        State = SoundInstanceFmod.States.Finished;
                    }
                    else
                    {
                        SafeFmodUtils.AssertResultOK(fres, $"Failed to stop FMOD Event '{EventName}'.");
                    }

                }
            }



            public void Pause()
            {
                if (State == SoundInstanceFmod.States.Playing)
                {

                    var fres = Event.setPaused(true);
                    if (fres == FMOD.RESULT.OK)
                    {
                        State = SoundInstanceFmod.States.Paused;
                    }
                    else if (fres == RESULT.ERR_INVALID_HANDLE)
                    {
                        State = SoundInstanceFmod.States.Finished;
                    }
                    else
                    {
                        SafeFmodUtils.AssertResultOK(fres, $"Failed to pause FMOD Event '{EventName}'.");
                    }

                }

            }

            public void Resume()
            {
                if (State == SoundInstanceFmod.States.Paused)
                {

                    var fres = Event.setPaused(false);
                    if (fres == FMOD.RESULT.OK)
                    {
                        State = SoundInstanceFmod.States.Playing;
                    }
                    else if (fres == RESULT.ERR_INVALID_HANDLE)
                    {
                        State = SoundInstanceFmod.States.Finished;
                    }
                    else
                    {
                        SafeFmodUtils.AssertResultOK(fres, $"Failed to resume FMOD Event '{EventName}'.");
                    }

                }
            }












        }
    }
}
