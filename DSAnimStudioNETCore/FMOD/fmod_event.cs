#define WIN64
/* ========================================================================================== */
/* FMOD Event - C# Wrapper . Copyright (c), Firelight Technologies Pty, Ltd. 2004-2016.       */
/*                                                                                            */
/*                                                                                            */
/* ========================================================================================== */

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace FMOD
{
    /*
        FMOD EventSystem version number.  Check this against FMOD::EventSystem::getVersion.
        0xaaaabbcc -> aaaa = major version number.  bb = minor version number.  cc = development version number.
    */
    public class EVENT_VERSION
    {
        public const int    number = 0x00044462;
#if WIN64
        public const string dll    = "fmod_event64.dll";
#else
        public const string dll    = "fmod_event.dll";
#endif
    }

    /*
    [DEFINE]
    [
        [NAME]
        FMOD_EVENT_INITFLAGS

        [DESCRIPTION]   
        Initialization flags.  Use them with EventSystem::init in the eventflags parameter to change various behaviour.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]
        EventSystem::init
    ]
    */
    public enum EVENT_INITFLAGS
    {
        NORMAL                 = 0x00000000, /* All platforms - Initialize normally */
        USER_ASSETMANAGER      = 0x00000001, /* All platforms - All wave data loading/freeing will be referred back to the user through the event callback */
        FAIL_ON_MAXSTREAMS     = 0x00000002, /* All platforms - Events will fail if "Max streams" was reached when playing streamed banks, instead of going virtual. */
        DONTUSENAMES           = 0x00000004, /* All platforms - All event/eventgroup/eventparameter/eventcategory/eventreverb names will be discarded on load. Use getXXXByIndex to access them. This may potentially save a lot of memory at runtime. */
        UPPERCASE_FILENAMES    = 0x00000008, /* All platforms - All FSB filenames will be translated to upper case before being used. */
        SEARCH_PLUGINS         = 0x00000010, /* All platforms - Search the current directory for dsp/codec plugins on EventSystem::init. */
        USE_GUIDS              = 0x00000020, /* All platforms - Don't build an event GUID table when loading FEVs (this means EventSystem::getEventByGUID cannot be used) */
        DETAILED_SOUNDDEF_INFO = 0x00000040, /* All platforms - Pass an FMOD_EVENT_SOUNDDEFINFO struct to FMOD_EVENT_CALLBACKTYPE_SOUNDDEF_SELECTINDEX callbacks rather than just the sound definition name (uses more memory for sound definition waveform names). */
        LOWERCASE_FILENAMES    = 0x00000080, /* All platforms - All FSB filenames will be translated to lower case before being used. */
        RESETPARAMSTOMINIMUM   = 0x00000100  /* All platforms - Reset parameters to minimum value when getting an event instance instead of using the INFO_ONLY event's values. */
    }


    /*
    [DEFINE]
    [
        [NAME] 
        FMOD_EVENT_MODE

        [DESCRIPTION]   
        Event data loading bitfields. Bitwise OR them together for controlling how event data is loaded.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3, Wii

        [SEE_ALSO]
        EventGroup::loadEventData
        EventGroup::getEvent
        EventGroup::getEventByIndex
    ]
    */
    public enum EVENT_MODE
    {
        DEFAULT             = 0x00000000,  /* FMOD_EVENT_DEFAULT specifies default loading behaviour i.e. event data for the whole group is NOT cached and the function that initiated the loading process will block until loading is complete. */
        NONBLOCKING         = 0x00000001,  /* For loading event data asynchronously. FMOD will use a thread to load the data.  Use Event::getState to find out when loading is complete. */
        ERROR_ON_DISKACCESS = 0x00000002,  /* For EventGroup::getEvent / EventGroup::getEventByIndex.  If EventGroup::loadEventData has accidently been forgotten this flag will return an FMOD_ERR_FILE_UNWANTED if the getEvent function tries to load data. */
        INFOONLY            = 0x00000004,  /* For EventGroup::getEvent / EventGroup::getEventByIndex.  Don't allocate instances or load data, just get a handle to allow user to get information from the event. */
        USERDSP             = 0x00000008,   /* For EventGroup::getEvent / EventGroup::getEventByIndex.  Tells FMOD that you plan to add your own DSP effects to this event's ChannelGroup at runtime. Omitting this flag will yield a small memory gain. */

        NONBLOCKING_THREAD0 = (NONBLOCKING),               /* FMOD_EVENT_NONBLOCKING, execute on thread 0.  See remarks. (default) */
        NONBLOCKING_THREAD1 = (NONBLOCKING | 0x00010000),  /* FMOD_EVENT_NONBLOCKING, execute on thread 1.  See remarks. */
        NONBLOCKING_THREAD2 = (NONBLOCKING | 0x00020000),  /* FMOD_EVENT_NONBLOCKING, execute on thread 2.  See remarks. */
        NONBLOCKING_THREAD3 = (NONBLOCKING | 0x00040000),  /* FMOD_EVENT_NONBLOCKING, execute on thread 3.  See remarks. */
        NONBLOCKING_THREAD4 = (NONBLOCKING | 0x00080000),  /* FMOD_EVENT_NONBLOCKING, execute on thread 4.  See remarks. */
    }


    /*
    [DEFINE]
    [
        [NAME] 
        FMOD_EVENT_STATE

        [DESCRIPTION]   
        These values describe what state an event is in.

        [REMARKS]    
        The flags below can be combined to set multiple states at once.  Use bitwise AND operations to test for these.
        An example of a combined flag set would be FMOD_EVENT_STATE_READY | FMOD_EVENT_STATE_PLAYING.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3, Wii

        [SEE_ALSO]
        Event::getState
        FMOD_EVENT_MODE
    ]
    */
    public enum EVENT_STATE
    {
        READY           = 0x00000001,  /* Event is ready to play. */
        LOADING         = 0x00000002,  /* Loading in progress. */
        ERROR           = 0x00000004,  /* Failed to open - file not found, out of memory etc.  See return value of Event::getState for what happened. */
        PLAYING         = 0x00000008,  /* Event has been started.  This will still be true even if there are no sounds active.  Event::stop must be called or the event must stop itself using a 'one shot and stop event' parameter mode. */
        CHANNELSACTIVE  = 0x00000010,  /* Event has active voices.  Use this if you want to detect if sounds are playing in the event or not. */
        INFOONLY        = 0x00000020,  /* Event was loaded with the FMOD_EVENT_INFOONLY flag. */
        STARVING        = 0x00000040,  /* Event is streaming but not being fed data in time, so may be stuttering. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]
        Property indices for Event::getPropertyByIndex.
    
        [REMARKS]        

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3, Wii

        [SEE_ALSO]
        Event::getPropertyByIndex
    ]
    */
    public enum EVENTPROPERTY
    {
        NAME = 0,                        /* Type : char *    - Name of event. */
        VOLUME,                          /* Type : float     - Relative volume of event. */
        VOLUMERANDOMIZATION,             /* Type : float     - Random deviation in volume of event. */
        PITCH,                           /* Type : float     - Relative pitch of event in raw underlying units. */
        PITCH_OCTAVES,                   /* Type : float     - Relative pitch of event in octaves. */
        PITCH_SEMITONES,                 /* Type : float     - Relative pitch of event in semitones. */
        PITCH_TONES,                     /* Type : float     - Relative pitch of event in tones. */
        PITCHRANDOMIZATION,              /* Type : float     - Random deviation in pitch of event in raw underlying units. */
        PITCHRANDOMIZATION_OCTAVES,      /* Type : float     - Random deviation in pitch of event in octaves. */
        PITCHRANDOMIZATION_SEMITONES,    /* Type : float     - Random deviation in pitch of event in semitones. */
        PITCHRANDOMIZATION_TONES,        /* Type : float     - Random deviation in pitch of event in tones. */
        PRIORITY,                        /* Type : int       - Playback priority of event. */
        MAX_PLAYBACKS,                   /* Type : int       - Maximum simultaneous playbacks of event. */
        MAX_PLAYBACKS_BEHAVIOR,          /* Type : int       - 1 = steal oldest, 2 = steal newest, 3 = steal quietest, 4 = just fail, 5 = just fail if quietest. */
        MODE,                            /* Type : FMOD_MODE - Either FMOD_3D or FMOD_2D. */
        _3D_IGNORE_GEOMETRY,             /* Type : int       - Ignore geometry for that event. 1 = yes,  0 = no. */
        _3D_ROLLOFF,                     /* Type : FMOD_MODE - Either FMOD_3D_LOGROLLOFF, FMOD_3D_LINEARROLLOFF, or none for custom rolloff. */
        _3D_MINDISTANCE,                 /* Type : float     - Minimum 3d distance of event. */
        _3D_MAXDISTANCE,                 /* Type : float     - Maximum 3d distance of event.  Means different things depending on EVENTPROPERTY_3D_ROLLOFF. If event has custom rolloff, setting FMOD_EVENTPROPERTY_3D_MAXDISTANCE will scale the range of all distance parameters in this event e.g. set this property to 2.0 to double the range of all distance parameters, set it to 0.5 to halve the range of all distance parameters. */
        _3D_POSITION,                    /* Type : FMOD_MODE - Either FMOD_3D_HEADRELATIVE or FMOD_3D_WORLDRELATIVE. */
        _3D_CONEINSIDEANGLE,             /* Type : float     - Event cone inside angle.  0 to 360. */
        _3D_CONEOUTSIDEANGLE,            /* Type : float     - Event cone outside angle.  0 to 360. */
        _3D_CONEOUTSIDEVOLUME,           /* Type : float     - Event cone outside volume.  0 to 1.0. */
        _3D_DOPPLERSCALE,                /* Type : float     - Doppler scale where 0 = no doppler, 1.0 = normal doppler, 2.0 = double doppler etc. */
        _3D_SPEAKERSPREAD,               /* Type : float     - Angle of spread for stereo/mutlichannel source. 0 to 360. */
        _3D_PANLEVEL,                    /* Type : float     - 0 = sound pans according to speaker levels, 1 = sound pans according to 3D position. */
        SPEAKER_L,                       /* Type : float     - 2D event volume for front left speaker. */
        SPEAKER_C,                       /* Type : float     - 2D event volume for front center speaker. */
        SPEAKER_R,                       /* Type : float     - 2D event volume for front right speaker. */
        SPEAKER_LS,                      /* Type : float     - 2D event volume for side left speaker. */
        SPEAKER_RS,                      /* Type : float     - 2D event volume for side right speaker. */
        SPEAKER_LR,                      /* Type : float     - 2D event volume for back left speaker. */
        SPEAKER_RR,                      /* Type : float     - 2D event volume for back right speaker. */
        SPEAKER_LFE,                     /* Type : float     - 2D event volume for low frequency speaker. */
        REVERBWETLEVEL,                  /* Type : float     - Reverb gain for this event where 0 = full reverb, -60 = no reverb. */
        ONESHOT,                         /* Type : int       - Oneshot event - stops when no channels playing */
        FADEIN,                          /* Type : int       - Time in milliseconds over which to fade this event in when programmer starts it. 0 = no fade in. */
        FADEOUT,                         /* Type : int       - Time in milliseconds over which to fade this event out when programmer stops it. 0 = no fade out. */
        REVERBDRYLEVEL,                  /* Type : float     - Dry reverb gain for this event where 0 = full dry, -60 = no dry. */
        TIMEOFFSET,                      /* Type : float     - Time offset of sound start in seconds (0 to 60.0f) */
        SPAWNINTENSITY,                  /* Type : float     - Multiplier for spawn frequency of all sounds in this event. */
        SPAWNINTENSITY_RANDOMIZATION,    /* Type : float     - Random deviation in spawn intensity of event. */
        WII_CONTROLLERSPEAKERS,          /* Type : int       - Wii only. Use 0 to 3 to specify a Wii controller speaker to play this event on, -1 to play on normal Wii speakers. */
        _3D_POSRANDOMIZATION_MIN,        /* Type : unsigned int - Minimum radius of random deviation in the 3D position of event. */
        _3D_POSRANDOMIZATION_MAX,        /* Type : unsigned int - Maximum radius of random deviation in the 3D position of event. */
        EVENTTYPE,                       /* Type : int       - 0 = simple event, 1 = complex event */
        STEAL_PRIORITY,                  /* Type : int       - 0 to 10000.  How important this event is in relation to other events in the project. This event will never steal an event with a higher steal priority than this. */
        EFFECTS_AFFECT_REVERB,           /* Type : int       - 0 = default (no), 1 = yes.  Alternate routing for reverb path of an event so it goes from the layer dsp unit instead of the channel. */
		WILL_TERMINATE,					 /* Type : int       - 0 = no, 1 = yes, 2 = unknown (current event state is too complex).  Whether this event will terminate (stop playing) by itself. If called on an event instance with this_instance = true, the prediction is based on the current state of that instance. This means parameter values, keyoffs etc. come into play. */
        DSPCLOCKSTART_HI,                /* Type : unsigned int - High 32 bits of a 64 bit DSP clock value, for a start time for any sound in this event. */
        DSPCLOCKSTART_LO,                /* Type : unsigned int - Low 32 bits of a 64 bit DSP clock value, for a start time for any sound in this event. */
        USER_BASE                        /* User created events start from here onwards. */
    }

    /*
    [ENUM]
    [
	    [DESCRIPTION]
        Event property types.
        
	    [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3, Wii

	    [SEE_ALSO]
        Event::getPropertyByIndex
        Event::getPropertyInfo
    ]
    */
    public enum EVENTPROPERTY_TYPE
    {
        INT = 0,   /* Property is an int, unsigned int or other type of equivalent sizee e.g. FMOD_MODE. */
        FLOAT,     /* Property is a float. */
        STRING     /* Property is a char *. */
    }


    /*
    [ENUM]
    [
	    [DESCRIPTION]
        Pitch units for Event::setPitch and EventCategory::setPitch.
        
	    [REMARKS]        

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3, Wii

	    [SEE_ALSO]
        Event::setPitch
        EventCategory::setPitch
    ]
    */
    public enum EVENT_PITCHUNITS
    {
        RAW = 0,      /* Pitch is specified in raw underlying units. */
        OCTAVES,      /* Pitch is specified in units of octaves. */
        SEMITONES,    /* Pitch is specified in units of semitones. */
        TONES         /* Pitch is specified in units of tones. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]
        Flags to pass to EventGroup::loadEventData to determine what to load at the time of calling.
    
        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3, Wii

        [SEE_ALSO]
        EventGroup::loadEventData
    ]
    */
    public enum EVENT_RESOURCE
    {
        STREAMS_AND_SAMPLES,  /* Open all streams and load all banks into memory, under this group (recursive) */
        STREAMS,              /* Open all streams under this group (recursive).  No samples are loaded. */
        SAMPLES               /* Load all banks into memory, under this group (recursive).  No streams are opened. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        These callback types are used with FMOD_EVENT_CALLBACK.

        [REMARKS]
        <b>Note!</b>  Currently the user must call EventSystem::update for these callbacks to trigger!
        <br>
        An FMOD_EVENT_CALLBACKTYPE_SYNCPOINT callback is generated from 'markers' embedded in .wav files.
        These can be created by placing 'markers' in the original source wavs using a tool such as Sound Forge or Cooledit.<br>
        The wavs are then compiled into .FSB files when compiling the audio data using the FMOD designer tool.<br>
        Callbacks will be automatically generated at the correct place in the timeline when these markers are encountered
        which makes it useful for synchronization, lip syncing etc.<br>
        <br>
        An FMOD_EVENT_CALLBACKTYPE_SOUNDDEF_START callback is generated each time a sound definition is played in an event.<br>
        This happens every time a sound definition starts due to the event parameter entering the region specified in the 
        layer created by the sound designer..<br>
        This also happens when sounds are randomly respawned using the random respawn feature in the sound definition 
        properties in FMOD designer.<br>
        <br>
        An FMOD_EVENT_CALLBACKTYPE_SOUNDDEF_END callback is generated when a one-shot sound definition inside an event ends, 
        or when a looping sound definition stops due to the event parameter leaving the region specified in the layer created 
        by the sound designer.<br>
        <br>
        An FMOD_EVENT_CALLBACKTYPE_NET_MODIFIED callback is generated when someone has connected to your running application with 
        FMOD Designer and changed a property within this event, for example volume or pitch.<br>

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3, Wii, Wii

        [SEE_ALSO]      
        Event::setCallback
        FMOD_EVENT_CALLBACK
        EventSystem::update
    ]
    */
    public enum EVENT_CALLBACKTYPE
    {
        SYNCPOINT,              /* Called when a syncpoint is encountered.  Can be from wav file markers. */
        SOUNDDEF_START,         /* Called when a sound definition inside an event is triggered. */
        SOUNDDEF_END,           /* Called when a sound definition inside an event ends or is stopped. */
        STOLEN,                 /* Called when an event runs out of instances and re-uses an existing event. */
        EVENTFINISHED,          /* Called when a non looping event parameter causes an event stop. */
        NET_MODIFIED,           /* Called when a property of the event has been modified by a network-connected host. */
        SOUNDDEF_CREATE,        /* Called when a programmer sound definition entry is loaded. */
        SOUNDDEF_RELEASE,       /* Called when a programmer sound definition entry is unloaded. */
        SOUNDDEF_INFO,          /* Called when a sound definition entry is loaded. */
        EVENTSTARTED,           /* Called when an event is started. */
        SOUNDDEF_SELECTINDEX,   /* Called when a sound definition entry needs to be chosen from a "ProgrammerSelected" sound definition. */
        OCCLUSION               /* Called when an event's channel is occluded with the geometry engine. */
    }


    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure containing realtime information about a wavebank.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]      
        EventSystem::getInfo
        FMOD_EVENT_SYSTEMINFO
    ]
    */
    public struct EVENT_WAVEBANKINFO
    {
        [MarshalAs(UnmanagedType.ByValArray,SizeConst = 256)]
        public char[]        name;                  /* [out] Name of this wave bank. */
        public int           streamrefcnt;          /* [out] Number of stream references to this wave bank made by events in this event system. */
        public int           samplerefcnt;          /* [out] Number of sample references to this wave bank made by events in this event system. */
        public int           numstreams;            /* [out] Number of times this wave bank has been opened for streaming. */
        public int           maxstreams;            /* [out] Maximum number of times this wave bank will be opened for streaming. */
        public int           streamsinuse;          /* [out] Number of streams currently in use. */
        public uint          streammemory;          /* [out] Amount of memory (in bytes) used by streams. */
        public uint          samplememory;          /* [out] Amount of memory (in bytes) used by samples. */
        public int           type;                  /* [out] 0 = stream from disk, 1 = load into memory, 2 = decompress into memory. */
    }


    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure containing realtime information about an event system.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]      
        EventSystem::getInfo
        FMOD_EVENT_WAVEBANKINFO
    ]
    */
    public struct EVENT_SYSTEMINFO
    {
        public int      numevents;        /* [out] Total number of events in all event groups in this event system. */
        public int      numinstances;     /* [out] Total number of event instances in all event groups in this event system. */
        public int      maxwavebanks;     /* [in/out] Out, number of wavebanks loaded by the EventSystem.  In. Maximum size of array of wavebankinfo structures supplied by user.  Optional. */
        public IntPtr   wavebankinfo;     /* [in] Pointer to array FMOD_EVENT_WAVEBANKINFO structures (max size defined by maxwavebanks).  FMOD will fill these in with detailed information on each wave bank. Optional. */
        public int      numplayingevents; /* [in/out] On entry, maximum number of entries in playingevents array. On exit, actual number of entries in playingevents array, or if playingevents is null, then it is just the number of currently playing events. Optional. */
        public IntPtr   playingevents;    /* [in/out] Pointer to an array that will be filled with the event handles of all playing events. Optional. Specify 0 if not needed. Must be used in conjunction with numplayingevents. */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public int[]    numloadsqueued;   /* [out] Current number of sound banks queued for loading due to using FMOD_EVENT_NONBLOCKING flag. */
    }



    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure containing realtime information about an event project.

        [REMARKS]
        On entry, numplayingevents should be set to the number of elements in the playingevents array. If the actual
        number of playing events is greater than numplayingevents then the playingevents array will be filled with
        numplayingevents entries and numplayingevents will be set to the actual number of playing events on exit.
        In short, if numplayingevents on exit > numplayingevents on entry then the playingevents array wasn't large
        enough and some events were unable to be added to the array.

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]
        EventProject::getInfo
        EventSystem::getInfo
        FMOD_EVENT_WAVEBANKINFO
    ]
    */
    public struct EVENT_PROJECTINFO
    {
        public int              index;            /* [out] Index of the project. */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public char[]           name;             /* [out] Name of the project. */
        public int              numevents;        /* [out] Total number of events in all event groups in this event project. */
        public int              numinstances;     /* [out] Total number of event instances in all event groups in this event project. */
        public int              maxwavebanks;     /* [in/out] Out, number of wavebanks loaded by the EventProject.  In. Maximum size of array of wavebankinfo structures supplied by user.  Optional. */
        public IntPtr           wavebankinfo;     /* [in] Pointer to array FMOD_EVENT_WAVEBANKINFO structures (max size defined by maxwavebanks).  FMOD will fill these in with detailed information on each wave bank. Optional. */
        public int              numplayingevents; /* [in/out] On entry, maximum number of entries in playingevents array. On exit, actual number of entries in playingevents array, or if playingevents is null, then it is just the number of currently playing events. Optional. */
        public IntPtr           playingevents;    /* [in/out] Pointer to an array that will be filled with the event handles of all playing events. Optional. Specify 0 if not needed. Must be used in conjunction with numplayingevents. */
    }


    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure containing extended information about an event.

        [REMARKS]
        This structure is optional!  Specify 0 or NULL in Event::getInfo if you don't need it!<br>

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]      
        Event::getInfo
    ]
    */
    public struct EVENT_INFO
    {
        public int      memoryused;             /* [out] Amount of memory (in bytes) used by this event. */
        public int      positionms;             /* [out] Time passed in playback of this event instance in milliseconds. */
        public int      lengthms;               /* [out] Length in milliseconds of this event. Note: lengthms will be -1 if the length of the event can't be determined i.e. if it has looping sounds. */
        public int      channelsplaying;        /* [out] Number of channels currently playing in this event instance. */
        public int      instancesactive;        /* [out] Number of event instances currently in use. */
        public int      maxwavebanks;           /* [in/out] Out, number of wavebanks refered to by this event.  In. Maximum size of array of wavebankinfo structures supplied by user.  Optional. */
        public IntPtr   wavebankinfo;           /* [in] Pointer to array FMOD_EVENT_WAVEBANKINFO structures (max size defined by maxwavebanks).  FMOD will fill these in with detailed information on each wave bank. Optional. */
        public uint     projectid;              /* [out] The runtime 'EventProject' wide unique identifier for this event. */
        public uint     systemid;               /* [out] The runtime 'EventSystem' wide unique identifier for this event.  This is calculated when single or multiple projects are loaded. */
        public float    audibility;             /* [out] current audibility of event. */
		public int      numinstances;           /* [in/out] On entry, maximum number of entries in instances array. On exit, actual number of entries in instances array, or if instances is null, then it is just the number of instances of this event. Optional. */
		public IntPtr   instances;              /* [in/out] Pointer to an array that will be filled with the current reference-counted event handles of all instances of this event. Optional. Specify 0 if not needed. Must be used in conjunction with numinstances. Note: Due to reference counting, the event instance handles returned here may be different between subsequent calls to this function. If you use these event handles, make sure your code is prepared for them to be invalid! */
        public IntPtr   guid;                   /* [out] Pointer to a structure that will be filled with the event's GUID. Optional. Specify 0 if not needed. */
    }
    

    /*
    [ENUM]
    [
        [DESCRIPTION]   
        Sound definition entry types for FMOD_EVENT_SOUNDDEFINFO.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3, Wii, Wii

        [SEE_ALSO]      
        FMOD_EVENT_SOUNDDEFINFO
        FMOD_EVENT_CALLBACK
        FMOD_EVENT_CALLBACKTYPE
    ]
    */
    public enum EVENT_SOUNDDEF_ENTRYTYPE
    {
        WAVETABLE,           /* Waveform. */
        OSCILLATOR,          /* Oscillator. */
        NULL,                /* "Don't play" entry. */
        PROGRAMMER           /* Programmer sound. */
    } 


    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure containing information about a sound definition.

        [REMARKS]
        This structure is passed as the param1 argument of all 
        FMOD_EVENT_CALLBACKTYPE_SOUNDDEF_SELECTINDEX callbacks if
        FMOD_EVENT_INIT_DETAILED_SOUNDDEF_INFO was passed to EventSystem::init.

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]      
        FMOD_EVENT_CALLBACK
        FMOD_EVENT_CALLBACKTYPE
        EventSystem::init
        FMOD_EVENT_INITFLAGS
        FMOD_EVENT_SOUNDDEF_ENTRYTYPE
    ]
    */
    public struct EVENT_SOUNDDEFINFO
    {
        public string   name;              /* The name of the sound definition. */
        public int      numentries;        /* The number of entries in the sound definition. */
        public IntPtr   entrynames;        /* The names of the entries in the sound definition (an array of size numentries). Note that entrynames[i] will be null if entrytypes[i] is not FMOD_EVENT_SOUNDDEF_ENTRYTYPE_WAVETABLE. */
        public IntPtr   entrytypes;        /* The types of the entries in the sound definition (an array of size numentries). */
    }


    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Use this structure with EventSystem::load when more control is needed over loading.

        [REMARKS]
        This structure is optional!  Specify 0 or NULL in EventSystem::load if you don't need it!<br>
        <br>
        Members marked with [in] mean the user sets the value before passing it to the function.<br>
        Members marked with [out] mean FMOD sets the value to be used after the function exits.<br>
        Use sounddefentrylimit to limit the number of sound definition entries - and therefore the amount of wave data - loaded for each sound definition. This feature allows the programmer to implement a "low detail" setting at runtime without needing a seperate "low detail" set of assets.

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]      
        EventSystem::load
    ]
    */
    public struct EVENT_LOADINFO
    {
        public uint     size;                         /* [in] Size of this structure.  This is used so the structure can be expanded in the future and still work on older versions of FMOD Ex. */
        public string   encryptionkey;                /* [in] Optional. Specify 0 to ignore. Key, or 'password' to decrypt a bank.  A sound designer may have encrypted the audio data to protect their sound data from 'rippers'. */
        public float    sounddefentrylimit;           /* [in] Optional. Specify 0 to ignore. A value between 0 -> 1 that is multiplied with the number of sound definition entries in each sound definition in the project being loaded in order to programmatically reduce the number of sound definition entries used at runtime. */
        public uint     loadfrommemory_length;        /* [in] Optional. Specify 0 to ignore. Length of memory buffer pointed to by name_or_data parameter passed to EventSystem::load. If this field is non-zero then the name_or_data parameter passed to EventSystem::load will be interpreted as a pointer to a memory buffer containing the .fev data to load. If this field is zero the name_or_data parameter is interpreted as the filename of the .fev file to load. */
        public bool     override_category_vals;       /* [in] Optional. If this member is set to true, newly-loaded categories will impart their properties (volume, pitch etc.) to existing categories of the same name. */
        public uint     sizeof_instancepool_simple;   /* [in] Optional. Specify 0 to ignore. If this value is non-zero, FMOD will create an instance pool for simple events with "sizeof_instancepool_simple" entries. Note: Event instance pools currently work for simple events only. Complex events will behave as normal and not be pooled. */
    }

    public delegate RESULT EVENT_CALLBACK (IntPtr eventraw, EVENT_CALLBACKTYPE type, IntPtr param1, IntPtr param2, IntPtr userdata);

    /*
    [ENUM]
    [
        [DESCRIPTION]   
        These callback types are used with FMOD_EVENTQUEUE_CALLBACK.

        [REMARKS]
        <b>Note!</b> Currently the user must call EventSystem::update for these callbacks to trigger!<p>

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3, Wii

        [SEE_ALSO]      
        EventQueue::setCallback
        FMOD_EVENTQUEUE_CALLBACK
        EventSystem::update
    ]
    */
    public enum EVENTQUEUE_CALLBACKTYPE
    {
        PREPARE,       /* Called when an entry is being prepared for playback */
        ABOUTTOPLAY,   /* Called when an entry is about to play */
        FINISHED,      /* Called when an entry has finished playing */
        EXPIRED        /* Called when an entry has expired before being played. See EventQueueEntry::setExpiryTime */
    }

    public delegate RESULT EVENTQUEUE_CALLBACK(EVENTQUEUE_CALLBACKTYPE type, IntPtr eventqueueraw, IntPtr queueentryraw, IntPtr callbackuserdata);


    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure containing realtime information about the music system.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]      
        MusicSystem::getInfo
    ]
    */
    public struct MUSIC_INFO
    {
        public bool starving;           /* [out] True if any streams in the music system are starving. */
        public bool all_samples_loaded; /* [out] True if all non-streaming samples in the music system are loaded, false otherwise. */
    }


    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure used to hold information about music system entites.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]      
        FMOD_MUSIC_ITERATOR
    ]
    */
    public struct MUSIC_ENTITY
    {
	    public string name; /* The name of the music entity as a null terminated string. */
	    public uint   id;   /* The ID of the music entity. */
    }


    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure used to enumerate entities in the music system.

        [REMARKS]
        The music system provides methods to initialize and advance iterators. Iterator members should never need to be set manually.

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]      
        FMOD_MUSIC_ENTITY
        MusicSystem::getCues
        MusicSystem::getParameters
    ]
    */
    public struct MUSIC_ITERATOR
    {
	    public IntPtr value;    /* The music entity the iterator points to. A null value indicates an invalid iterator. */
	    public string filter;   /* The string used to filter music entities. */
    }


    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure containing information about a music sample, for use with
        FMOD_MUSIC_CALLBACKTYPE_SEGMENT_CREATE and FMOD_MUSIC_CALLBACKTYPE_SEGMENT_RELEASE.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]      
        FMOD_MUSIC_CALLBACK
        FMOD_MUSIC_CALLBACKTYPE
    ]
    */
    public struct MUSIC_SAMPLE_INFO
    {
        public uint   segment_id;   /* The ID of the parent segment. */
        public uint   index;        /* The index of the sample within the parent segment. */
        public string filename;     /* The filename of the sample.<br/> <b>Note:</b> If the sample was built by a version of FMOD Designer before 4.29.09, this field will be 0. */
    }


    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure containing information about a music segment, for use with
        FMOD_MUSIC_CALLBACKTYPE_CHANNEL_CREATED and FMOD_MUSIC_CALLBACKTYPE_CHANNEL_DESTROYED.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]      
        FMOD_MUSIC_CALLBACK
        FMOD_MUSIC_CALLBACKTYPE
    ]
    */
    public struct MUSIC_SEGMENT_INFO
    {
        public uint segment_id;   /* The ID of the segment. */
        public uint theme_id;     /* The ID of the parent theme. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        These callback types are used with FMOD_MUSIC_CALLBACK.

        [REMARKS]
        <b>Note!</b>  Currently the user must call EventSystem::update for these callbacks to trigger!<br />

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3, Wii, Wii

        [SEE_ALSO]      
        MusicSystem::setCallback
        FMOD_MUSIC_CALLBACK
        EventSystem::update
    ]
    */
    public enum MUSIC_CALLBACKTYPE
    {
        SEGMENT_START,       /* Called when a segment is started. */
        SEGMENT_END,         /* Called when a segment ends. */
        SAMPLE_CREATE,       /* Called when a segment needs a sound created. */
        SAMPLE_RELEASE,      /* Called when a segment is finished with a sound. */
        CHANNEL_CREATED,     /* Called when a channel is created to play a segment. */
        CHANNEL_DESTROYED,   /* Called when a segment channel is destroyed. */
        RESET,               /* Called when the system is reset */
        BEAT,                /* Called each time a beat is passed (based on segment tempo and time signature) */
    }

    public delegate RESULT MUSIC_CALLBACK (MUSIC_CALLBACKTYPE type, IntPtr param1, IntPtr param2, IntPtr userdata);
    

    /*
        FMOD EventSystem factory functions.
    */
    public class Event_Factory
    {
        public static RESULT EventSystem_Create(ref EventSystem eventsystem)
        {
#if WIN64
            if (IntPtr.Size != 8)
            {
                /* Attempting to use 64-bit FMOD dll with 32-bit application.*/
            
                return RESULT.ERR_FILE_BAD;
            }
#else
            if (IntPtr.Size != 4)
            {
                /* Attempting to use 32-bit FMOD dll with 64-bit application. A likely cause of this error 
                 * is targetting platform 'Any CPU'. You cannot link to unmanaged dll with 'Any CPU'
                 * target. 
                 * 
                 * For 32-bit applications: set the platform to 'x86'.
                 * 
                 * For 64-bit applications:
                 * 1. set the platform to x64
                 * 2. add the conditional complication symbol WIN64
                 * 3. download the win64 fmod release
                 * 4. copy the fmodex64.dll to the location of the .exe file for your application */

                return RESULT.ERR_FILE_BAD;
            }
#endif

            RESULT      result           = RESULT.OK;
            IntPtr      eventsystemraw   = new IntPtr();
            EventSystem eventsystemnew   = null;

            result = FMOD_EventSystem_Create(ref eventsystemraw);
            if (result != RESULT.OK)
            {
                return result;
            }

            eventsystemnew = new EventSystem();
            eventsystemnew.setRaw(eventsystemraw);
            eventsystem = eventsystemnew;

            return result;
        }

        #region importfunctions

        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_Create                      (ref IntPtr eventsystem);

        #endregion
    }


    /*
       'EventSystem' API
    */
    public class EventSystem
    {
        // Initialization / system functions.
        public RESULT init                      (int maxchannels, INITFLAGS flags, IntPtr extradriverdata)
        {
            return FMOD_EventSystem_Init(eventsystemraw, maxchannels, flags, extradriverdata, EVENT_INITFLAGS.NORMAL);
        }
        public RESULT init                      (int maxchannels, INITFLAGS flags, IntPtr extradriverdata, EVENT_INITFLAGS eventflags)
        {
            return FMOD_EventSystem_Init(eventsystemraw, maxchannels, flags, extradriverdata, eventflags);
        }
        public RESULT release                   ()
        {
            return FMOD_EventSystem_Release(eventsystemraw);
        }
        public RESULT update                    ()
        {
            return FMOD_EventSystem_Update(eventsystemraw);
        }
        public RESULT setMediaPath              (string path)
        {
            return FMOD_EventSystem_SetMediaPath(eventsystemraw, path);
        }
        public RESULT setPluginPath             (string path)
        {
            return FMOD_EventSystem_SetPluginPath(eventsystemraw, path);
        }
        public RESULT getVersion                (ref uint version)
        {
            return FMOD_EventSystem_GetVersion(eventsystemraw, ref version);
        }
        public RESULT getInfo                   (ref EVENT_SYSTEMINFO info)
        {
            return FMOD_EventSystem_GetInfo(eventsystemraw, ref info);
        }
        public RESULT getSystemObject           (ref System system)
        {
            RESULT result   = RESULT.OK;
            IntPtr systemraw   = new IntPtr();
            System systemnew   = null;

            try
            {
                result = FMOD_EventSystem_GetSystemObject(eventsystemraw, ref systemraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (system == null)
            {
                systemnew = new System();
                systemnew.setRaw(systemraw);
                system = systemnew;
            }
            else
            {
                system.setRaw(systemraw);
            }

            return result; 
        }
        public RESULT getMusicSystem(ref MusicSystem musicsystem)
        {
            RESULT result = RESULT.OK;
            IntPtr musicsystemraw = new IntPtr();
            MusicSystem musicsystemnew = null;

            try
            {
                result = FMOD_EventSystem_GetMusicSystem(eventsystemraw, ref musicsystemraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (musicsystem == null)
            {
                musicsystemnew = new MusicSystem();
                musicsystemnew.setRaw(musicsystemraw);
                musicsystem = musicsystemnew;
            }
            else
            {
                musicsystem.setRaw(musicsystemraw);
            }

            return result;
        }
        public RESULT getLanguage(ref IntPtr language)
        {
            return FMOD_EventSystem_GetLanguage(eventsystemraw, ref language);
        }
        public RESULT setLanguage(string language)
        {
            return FMOD_EventSystem_SetLanguage(eventsystemraw, language);
        }


        // FEV load/unload.                                 
        public RESULT load                      (string name_or_data, ref EVENT_LOADINFO loadinfo, ref EventProject project)
        {
            RESULT result                  = RESULT.OK;
            IntPtr eventprojectraw         = new IntPtr();
            EventProject eventprojectnew   = null;

            try
            {
                result = FMOD_EventSystem_Load(eventsystemraw, name_or_data, ref loadinfo, ref eventprojectraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (project == null)
            {
                eventprojectnew = new EventProject();
                eventprojectnew.setRaw(eventprojectraw);
                project = eventprojectnew;
            }
            else
            {
                project.setRaw(eventprojectraw);
            }

            return result; 
        }
        public RESULT load                      (string name_or_data)
        {
            return FMOD_EventSystem_Load(eventsystemraw, name_or_data, (IntPtr)null, (IntPtr)null);
        }
        public RESULT unload                    ()
        {
            return FMOD_EventSystem_Unload(eventsystemraw);
        }
                                                            
        // Event,EventGroup,EventCategory Retrieval.        
        public RESULT getProject                (string name, ref EventProject project)
        {
            RESULT result                  = RESULT.OK;
            IntPtr eventprojectraw         = new IntPtr();
            EventProject eventprojectnew   = null;

            try
            {
                result = FMOD_EventSystem_GetProject(eventsystemraw, name, ref eventprojectraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (project == null)
            {
                eventprojectnew = new EventProject();
                eventprojectnew.setRaw(eventprojectraw);
                project = eventprojectnew;
            }
            else
            {
                project.setRaw(eventprojectraw);
            }

            return result; 
        }
        public RESULT getProjectByIndex         (int index, ref EventProject project)
        {
            RESULT result                  = RESULT.OK;
            IntPtr eventprojectraw         = new IntPtr();
            EventProject eventprojectnew   = null;

            try
            {
                result = FMOD_EventSystem_GetProjectByIndex(eventsystemraw, index, ref eventprojectraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (project == null)
            {
                eventprojectnew = new EventProject();
                eventprojectnew.setRaw(eventprojectraw);
                project = eventprojectnew;
            }
            else
            {
                project.setRaw(eventprojectraw);
            }

            return result; 
        }
        public RESULT getNumProjects            (ref int numprojects)
        {
            return FMOD_EventSystem_GetNumProjects(eventsystemraw, ref numprojects);
        }
        public RESULT getCategory               (string name, ref EventCategory category)
        {
            RESULT result                    = RESULT.OK;
            IntPtr eventcategoryraw          = new IntPtr();
            EventCategory eventcategorynew   = null;

            try
            {
                result = FMOD_EventSystem_GetCategory(eventsystemraw, name, ref eventcategoryraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (category == null)
            {
                eventcategorynew = new EventCategory();
                eventcategorynew.setRaw(eventcategoryraw);
                category = eventcategorynew;
            }
            else
            {
                category.setRaw(eventcategoryraw);
            }

            return result; 
        }
        public RESULT getCategoryByIndex        (int index, ref EventCategory category)
        {
            RESULT result                    = RESULT.OK;
            IntPtr eventcategoryraw          = new IntPtr();
            EventCategory eventcategorynew   = null;

            try
            {
                result = FMOD_EventSystem_GetCategoryByIndex(eventsystemraw, index, ref eventcategoryraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (category == null)
            {
                eventcategorynew = new EventCategory();
                eventcategorynew.setRaw(eventcategoryraw);
                category = eventcategorynew;
            }
            else
            {
                category.setRaw(eventcategoryraw);
            }

            return result; 
        }
        public RESULT getMusicCategory(ref EventCategory category)
        {
            RESULT result = RESULT.OK;
            IntPtr musiccategoryraw = new IntPtr();
            EventCategory musiccategorynew = null;

            try
            {
                result = FMOD_EventSystem_GetMusicCategory(eventsystemraw, ref musiccategoryraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (category == null)
            {
                musiccategorynew = new EventCategory();
                musiccategorynew.setRaw(musiccategoryraw);
                category = musiccategorynew;
            }
            else
            {
                category.setRaw(musiccategoryraw);
            }

            return result;
        }
        public RESULT getNumCategories          (ref int numcategories)
        {
            return FMOD_EventSystem_GetNumCategories(eventsystemraw, ref numcategories);
        }
        public RESULT getGroup                  (string name, bool cacheevents, ref EventGroup group)
        {
            RESULT result              = RESULT.OK;
            IntPtr eventgroupraw       = new IntPtr();
            EventGroup eventgroupnew   = null;

            try
            {
                result = FMOD_EventSystem_GetGroup(eventsystemraw, name, (cacheevents ? 1 : 0), ref eventgroupraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (group == null)
            {
                eventgroupnew = new EventGroup();
                eventgroupnew.setRaw(eventgroupraw);
                group = eventgroupnew;
            }
            else
            {
                group.setRaw(eventgroupraw);
            }

            return result; 
        }
        public RESULT getEvent                  (string name, EVENT_MODE mode, ref Event _event)
        {
            RESULT result   = RESULT.OK;
            IntPtr eventraw = new IntPtr();
            Event eventnew  = null;

            try
            {
                result = FMOD_EventSystem_GetEvent(eventsystemraw, name, mode, ref eventraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (_event == null)
            {
                eventnew = new Event();
                eventnew.setRaw(eventraw);
                _event = eventnew;
            }
            else
            {
                _event.setRaw(eventraw);
            }

            return result; 
        }
        public RESULT getEventBySystemID        (uint systemid, EVENT_MODE mode, ref Event _event)
        {
            RESULT result   = RESULT.OK;
            IntPtr eventraw = new IntPtr();
            Event eventnew  = null;

            try
            {
                result = FMOD_EventSystem_GetEventBySystemID(eventsystemraw, systemid, mode, ref eventraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (_event == null)
            {
                eventnew = new Event();
                eventnew.setRaw(eventraw);
                _event = eventnew;
            }
            else
            {
                _event.setRaw(eventraw);
            }

            return result;
        }
        public RESULT getEventByGUID            (ref GUID guid, EVENT_MODE mode, ref Event _event)
        {
            RESULT result   = RESULT.OK;
            IntPtr eventraw = new IntPtr();
            Event eventnew  = null;

            try
            {
                result = FMOD_EventSystem_GetEventByGUID(eventsystemraw, ref guid, mode, ref eventraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (_event == null)
            {
                eventnew = new Event();
                eventnew.setRaw(eventraw);
                _event = eventnew;
            }
            else
            {
                _event.setRaw(eventraw);
            }

            return result;
        }
        public RESULT getEventByGUIDString      (string guid, EVENT_MODE mode, ref Event _event)
        {
            RESULT result   = RESULT.OK;
            IntPtr eventraw = new IntPtr();
            Event eventnew  = null;

            try
            {
                result = FMOD_EventSystem_GetEventByGUIDString(eventsystemraw, guid, mode, ref eventraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (_event == null)
            {
                eventnew = new Event();
                eventnew.setRaw(eventraw);
                _event = eventnew;
            }
            else
            {
                _event.setRaw(eventraw);
            }

            return result;
        }
        public RESULT getNumEvents              (ref int numevents)
        {
            return FMOD_EventSystem_GetNumEvents(eventsystemraw, ref numevents);
        }

        // Reverb interfaces.
        public RESULT setReverbProperties       (ref REVERB_PROPERTIES props)
        {
            return FMOD_EventSystem_SetReverbProperties(eventsystemraw, ref props);
        }
        public RESULT getReverbProperties       (ref REVERB_PROPERTIES props)
        {
            return FMOD_EventSystem_GetReverbProperties(eventsystemraw, ref props);
        }

        public RESULT getReverbPreset           (string name, ref REVERB_PROPERTIES props, ref int index)
        {
            return FMOD_EventSystem_GetReverbPreset(eventsystemraw, name, ref props, ref index);
        }
        public RESULT getReverbPresetByIndex    (int index,  ref REVERB_PROPERTIES props, ref IntPtr name)
        {
            return FMOD_EventSystem_GetReverbPresetByIndex(eventsystemraw, index, ref props, ref name);
        }
        public RESULT getNumReverbPresets       (ref int numpresets)
        {
            return FMOD_EventSystem_GetNumReverbPresets(eventsystemraw, ref numpresets);
        }

        public RESULT createReverb              (ref EventReverb reverb)
        {
            RESULT      result    = RESULT.OK;
            IntPtr      reverbraw = new IntPtr();
            EventReverb reverbnew = null;

            try
            {
                result = FMOD_EventSystem_CreateReverb(eventsystemraw, ref reverbraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (reverb == null)
            {
                reverbnew = new EventReverb();
                reverbnew.setRaw(reverbraw);
                reverb = reverbnew;
            }
            else
            {
                reverb.setRaw(reverbraw);
            }

            return result;
        }
        public RESULT setReverbAmbientProperties(ref REVERB_PROPERTIES props)
        {
            return FMOD_EventSystem_SetReverbAmbientProperties(eventsystemraw, ref props);
        }
        public RESULT getReverbAmbientProperties(ref REVERB_PROPERTIES props)
        {
            return FMOD_EventSystem_GetReverbAmbientProperties(eventsystemraw, ref props);
        }

        // Event queue interface.
        public RESULT createEventQueue(ref EventQueue queue)
        {
            RESULT result = RESULT.OK;
            IntPtr queueraw = new IntPtr();

            try
            {
                result = FMOD_EventSystem_CreateEventQueue(eventsystemraw, ref queueraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (queue == null)
            {
                EventQueue queuenew = new EventQueue();
                queuenew.setRaw(queueraw);
                queue = queuenew;
            }
            else
            {
                queue.setRaw(queueraw);
            }

            return result;
        }
        public RESULT createEventQueueEntry(Event _event, ref EventQueueEntry entry)
        {
            RESULT result = RESULT.OK;
            IntPtr entryraw = new IntPtr();

            try
            {
                result = FMOD_EventSystem_CreateEventQueueEntry(eventsystemraw, _event.getRaw(), ref entryraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (entry == null)
            {
                EventQueueEntry entrynew = new EventQueueEntry();
                entrynew.setRaw(entryraw);
                entry = entrynew;
            }
            else
            {
                entry.setRaw(entryraw);
            }

            return result;
        }

        // 3D Listener interface.
        public RESULT set3DNumListeners         (int numlisteners)
        {
            return FMOD_EventSystem_Set3DNumListeners(eventsystemraw, numlisteners);
        }
        public RESULT get3DNumListeners         (ref int numlisteners)
        {
            return FMOD_EventSystem_Get3DNumListeners(eventsystemraw, ref numlisteners);
        }
        public RESULT set3DListenerAttributes   (int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up)
        {
            return FMOD_EventSystem_Set3DListenerAttributes(eventsystemraw, listener, ref pos, ref vel, ref forward, ref up);
        }
        public RESULT get3DListenerAttributes   (int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up)
        {
            return FMOD_EventSystem_Get3DListenerAttributes(eventsystemraw, listener, ref pos, ref vel, ref forward, ref up);
        }

        // Get/set user data
        public RESULT setUserData               (IntPtr userdata)
        {
            return FMOD_EventSystem_SetUserData(eventsystemraw, userdata);
        }
        public RESULT getUserData               (ref IntPtr userdata)
        {
            return FMOD_EventSystem_GetUserData(eventsystemraw, ref userdata);
        }

        // Pre-loading FSB files (from disk or from memory, use FMOD_OPENMEMORY_POINT to point to pre-loaded memory).
        public RESULT preloadFSB                (string filename, int streaminstance, Sound sound)
        {
            return FMOD_EventSystem_PreloadFSB(eventsystemraw, filename, streaminstance, sound.getRaw());
        }
        public RESULT unloadFSB                 (string filename, int streaminstance)
        {
            return FMOD_EventSystem_UnloadFSB(eventsystemraw, filename, streaminstance);
        }

        public RESULT getMemoryInfo             (uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details)
        {
            return FMOD_EventSystem_GetMemoryInfo(eventsystemraw, memorybits, event_memorybits, ref memoryused, ref memoryused_details);
        }

        #region importfunctions

        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_Init              (IntPtr eventsystem, int maxchannels, INITFLAGS flags, IntPtr extradriverdata, EVENT_INITFLAGS eventflags);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_Release           (IntPtr eventsystem);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_Update            (IntPtr eventsystem);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_SetMediaPath      (IntPtr eventsystem, string path);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_SetPluginPath     (IntPtr eventsystem, string path);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetVersion        (IntPtr eventsystem, ref uint version);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetInfo           (IntPtr eventsystem, ref EVENT_SYSTEMINFO info);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetSystemObject   (IntPtr eventsystem, ref IntPtr system);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetMusicSystem    (IntPtr eventsystem, ref IntPtr musicsystem);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetLanguage       (IntPtr eventsystem, ref IntPtr language);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_SetLanguage       (IntPtr eventsystem, string language);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_Load              (IntPtr eventsystem, string name_or_data, ref EVENT_LOADINFO loadinfo, ref IntPtr project);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_Load              (IntPtr eventsystem, string name_or_data, IntPtr loadinfo, IntPtr project);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_Unload            (IntPtr eventsystem);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetProject        (IntPtr eventsystem, string name, ref IntPtr project);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetProjectByIndex (IntPtr eventsystem, int index, ref IntPtr project);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetNumProjects    (IntPtr eventsystem, ref int numprojects);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetCategory       (IntPtr eventsystem, string name, ref IntPtr category);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetCategoryByIndex(IntPtr eventsystem, int index, ref IntPtr category);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetMusicCategory  (IntPtr eventsystem, ref IntPtr category);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetNumCategories  (IntPtr eventsystem, ref int numcategories);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetGroup          (IntPtr eventsystem, string name, int cacheevents, ref IntPtr group);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetEvent          (IntPtr eventsystem, string name, EVENT_MODE mode, ref IntPtr _event);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetEventBySystemID(IntPtr eventsystem, uint systemid, EVENT_MODE mode, ref IntPtr _event);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetEventByGUID    (IntPtr eventsystem, ref GUID guid, EVENT_MODE mode, ref IntPtr _event);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetEventByGUIDString (IntPtr eventsystem, string guid, EVENT_MODE mode, ref IntPtr _event);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetNumEvents      (IntPtr eventsystem, ref int numevents);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_SetReverbProperties(IntPtr eventsystem, ref REVERB_PROPERTIES props);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetReverbProperties(IntPtr eventsystem, ref REVERB_PROPERTIES props);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetReverbPreset   (IntPtr eventsystem, string name, ref REVERB_PROPERTIES props, ref int index);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetReverbPresetByIndex(IntPtr eventsystem, int index, ref REVERB_PROPERTIES props, ref IntPtr name);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetNumReverbPresets(IntPtr eventsystem, ref int numpresets);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_CreateReverb      (IntPtr eventsystem, ref IntPtr reverb);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_SetReverbAmbientProperties(IntPtr eventsystem, ref REVERB_PROPERTIES props);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetReverbAmbientProperties(IntPtr eventsystem, ref REVERB_PROPERTIES props);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_CreateEventQueue(IntPtr eventsystem, ref IntPtr queue);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_CreateEventQueueEntry(IntPtr eventsystem, IntPtr _event, ref IntPtr entry);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_Set3DNumListeners (IntPtr eventsystem, int numlisteners);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_Get3DNumListeners (IntPtr eventsystem, ref int numlisteners);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_Set3DListenerAttributes(IntPtr eventsystem, int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_Get3DListenerAttributes(IntPtr eventsystem, int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_SetUserData       (IntPtr eventsystem, IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetUserData       (IntPtr eventsystem, ref IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_PreloadFSB        (IntPtr eventsystem, string filename, int streaminstance, IntPtr sound);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_UnloadFSB         (IntPtr eventsystem, string filename, int streaminstance);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventSystem_GetMemoryInfo     (IntPtr eventsystem, uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details);
        #endregion

        #region wrapperinternal

        private IntPtr eventsystemraw;

        public void setRaw(IntPtr eventsystem)
        {
            eventsystemraw = new IntPtr();
            eventsystemraw = eventsystem;
        }

        public IntPtr getRaw()
        {
            return eventsystemraw;
        }

        #endregion
    }


    /*
       'EventProject' API
    */
    public class EventProject
    {
        public RESULT release            ()
        {
            return FMOD_EventProject_Release(eventprojectraw);
        }
        public RESULT getInfo            (ref EVENT_PROJECTINFO info)
        {
            return FMOD_EventProject_GetInfo(eventprojectraw, ref info);
        }
        public RESULT getGroup           (string name, bool cacheevents, ref EventGroup group)
        {
            RESULT      result        = RESULT.OK;
            IntPtr      eventgroupraw = new IntPtr();
            EventGroup  eventgroupnew = null;

            try
            {
                result = FMOD_EventProject_GetGroup(eventprojectraw, name, (cacheevents ? 1 : 0), ref eventgroupraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (group == null)
            {
                eventgroupnew = new EventGroup();
                eventgroupnew.setRaw(eventgroupraw);
                group = eventgroupnew;
            }
            else
            {
                group.setRaw(eventgroupraw);
            }

            return result;
        }
        public RESULT getGroupByIndex    (int index, bool cacheevents, ref EventGroup group)
        {
            RESULT      result        = RESULT.OK;
            IntPtr      eventgroupraw = new IntPtr();
            EventGroup  eventgroupnew = null;

            try
            {
                result = FMOD_EventProject_GetGroupByIndex(eventprojectraw, index, (cacheevents ? 1 : 0), ref eventgroupraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (group == null)
            {
                eventgroupnew = new EventGroup();
                eventgroupnew.setRaw(eventgroupraw);
                group = eventgroupnew;
            }
            else
            {
                group.setRaw(eventgroupraw);
            }

            return result;
        }
        public RESULT getNumGroups       (ref int numgroups)
        {
            return FMOD_EventProject_GetNumGroups(eventprojectraw, ref numgroups);
        }
        public RESULT getEvent           (string name, EVENT_MODE mode, ref Event _event)
        {
            RESULT result   = RESULT.OK;
            IntPtr eventraw = new IntPtr();
            Event  eventnew = null;

            try
            {
                result = FMOD_EventProject_GetEvent(eventprojectraw, name, mode, ref eventraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (_event == null)
            {
                eventnew = new Event();
                eventnew.setRaw(eventraw);
                _event = eventnew;
            }
            else
            {
                _event.setRaw(eventraw);
            }

            return result;
        }
        public RESULT getEventByProjectID(uint projectid, EVENT_MODE mode, ref Event _event)
        {
            RESULT result   = RESULT.OK;
            IntPtr eventraw = new IntPtr();
            Event  eventnew = null;

            try
            {
                result = FMOD_EventProject_GetEventByProjectID(eventprojectraw, projectid, mode, ref eventraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (_event == null)
            {
                eventnew = new Event();
                eventnew.setRaw(eventraw);
                _event = eventnew;
            }
            else
            {
                _event.setRaw(eventraw);
            }

            return result;
        }
        public RESULT getNumEvents       (ref int numevents)
        {
            return FMOD_EventProject_GetNumEvents(eventprojectraw, ref numevents);
        }
        public RESULT loadSampleData     (ref int eventid_array, int sizeof_eventid_array, IntPtr groupname_array, int sizeof_groupname_array, EVENT_MODE eventmode)
        {
            return FMOD_EventProject_LoadSampleData(eventprojectraw, ref eventid_array, sizeof_eventid_array, groupname_array, sizeof_groupname_array, eventmode);
        }
        public RESULT setUserData        (IntPtr userdata)
        {
            return FMOD_EventProject_SetUserData(eventprojectraw, userdata);
        }
        public RESULT getUserData        (ref IntPtr userdata)
        {
            return FMOD_EventProject_GetUserData(eventprojectraw, ref userdata);
        }

        public RESULT stopAllEvents      (bool immediate)
        {
            return FMOD_EventProject_StopAllEvents(eventprojectraw, immediate ? 1 : 0);
        }

        public RESULT cancelAllLoads()
        {
            return FMOD_EventProject_CancelAllLoads(eventprojectraw);
        }

        public RESULT getMemoryInfo      (uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details)
        {
            return FMOD_EventProject_GetMemoryInfo(eventprojectraw, memorybits, event_memorybits, ref memoryused, ref memoryused_details);
        }

        #region importfunctions
        
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_Release          (IntPtr eventproject);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_GetInfo          (IntPtr eventproject, ref EVENT_PROJECTINFO info);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_GetGroup         (IntPtr eventproject, string name, int cacheevents, ref IntPtr group);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_GetGroupByIndex  (IntPtr eventproject, int index, int cacheevents, ref IntPtr group);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_GetNumGroups     (IntPtr eventproject, ref int numgroups);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_GetEvent         (IntPtr eventproject, string name, EVENT_MODE mode, ref IntPtr _event);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_GetEventByProjectID(IntPtr eventproject, uint projectid, EVENT_MODE mode, ref IntPtr _event);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_GetNumEvents     (IntPtr eventproject, ref int numevents);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_LoadSampleData   (IntPtr eventproject, ref int eventid_array, int sizeof_eventid_array, IntPtr groupname_array, int sizeof_groupname_array, EVENT_MODE eventmode);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_SetUserData      (IntPtr eventproject, IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_GetUserData      (IntPtr eventproject, ref IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_StopAllEvents    (IntPtr eventproject, int immediate);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_CancelAllLoads   (IntPtr eventproject);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventProject_GetMemoryInfo    (IntPtr eventproject, uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details);

        #endregion

        #region wrapperinternal

        private IntPtr eventprojectraw;

        public void setRaw(IntPtr eventproject)
        {
            eventprojectraw = new IntPtr();
            eventprojectraw = eventproject;
        }

        public IntPtr getRaw()
        {
            return eventprojectraw;
        }

        #endregion
    }


    /*
       'EventGroup' API
    */
    public class EventGroup
    {
        public RESULT getInfo            (ref int index, ref IntPtr name)
        {
            return FMOD_EventGroup_GetInfo(eventgroupraw, ref index, ref name);
        }
        public RESULT loadEventData      ()
        {
            return FMOD_EventGroup_LoadEventData(eventgroupraw, EVENT_RESOURCE.STREAMS_AND_SAMPLES, EVENT_MODE.DEFAULT);
        }
        public RESULT loadEventData      (EVENT_RESOURCE resource, EVENT_MODE mode)
        {
            return FMOD_EventGroup_LoadEventData(eventgroupraw, resource, mode);
        }
		public RESULT freeEventData      ()
		{
			return FMOD_EventGroup_FreeEventData(eventgroupraw, (IntPtr)null, 1);
		}
		public RESULT freeEventData      (Event _event)
		{
			return FMOD_EventGroup_FreeEventData(eventgroupraw, _event.getRaw(), 1);
		}
		public RESULT freeEventData      (Event _event, bool waituntilready)
		{
			return FMOD_EventGroup_FreeEventData(eventgroupraw, _event.getRaw(), waituntilready ? 1 : 0);
		}
		public RESULT freeEventData      (bool waituntilready)
		{
			return FMOD_EventGroup_FreeEventData(eventgroupraw, (IntPtr)null, waituntilready ? 1 : 0);
		}
        public RESULT getGroup           (string name, bool cacheevents, ref EventGroup group)
        {
            RESULT      result        = RESULT.OK;
            IntPtr      groupraw = new IntPtr();
            EventGroup  groupnew = null;

            try
            {
                result = FMOD_EventGroup_GetGroup(eventgroupraw, name, (cacheevents ? 1 : 0), ref groupraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (group == null)
            {
                groupnew = new EventGroup();
                groupnew.setRaw(groupraw);
                group = groupnew;
            }
            else
            {
                group.setRaw(groupraw);
            }

            return result;
        }
        public RESULT getGroupByIndex    (int index, bool cacheevents, ref EventGroup group)
        {
            RESULT      result        = RESULT.OK;
            IntPtr      groupraw = new IntPtr();
            EventGroup  groupnew = null;

            try
            {
                result = FMOD_EventGroup_GetGroupByIndex(eventgroupraw, index, (cacheevents ? 1 : 0), ref groupraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (group == null)
            {
                groupnew = new EventGroup();
                groupnew.setRaw(groupraw);
                group = groupnew;
            }
            else
            {
                group.setRaw(groupraw);
            }

            return result;
        }
        public RESULT getParentGroup     (ref EventGroup group)
        {
            RESULT      result        = RESULT.OK;
            IntPtr      groupraw = new IntPtr();
            EventGroup  groupnew = null;

            try
            {
                result = FMOD_EventGroup_GetParentGroup(eventgroupraw, ref groupraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (group == null)
            {
                groupnew = new EventGroup();
                groupnew.setRaw(groupraw);
                group = groupnew;
            }
            else
            {
                group.setRaw(groupraw);
            }

            return result;
        }
        public RESULT getParentProject   (ref EventProject project)
        {
            RESULT       result          = RESULT.OK;
            IntPtr       eventprojectraw = new IntPtr();
            EventProject eventprojectnew = null;

            try
            {
                result = FMOD_EventGroup_GetParentProject(eventgroupraw, ref eventprojectraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (project == null)
            {
                eventprojectnew = new EventProject();
                eventprojectnew.setRaw(eventprojectraw);
                project = eventprojectnew;
            }
            else
            {
                project.setRaw(eventprojectraw);
            }

            return result;
        }
        public RESULT getNumGroups       (ref int numgroups)
        {
            return FMOD_EventGroup_GetNumGroups(eventgroupraw, ref numgroups);
        }
        public RESULT getEvent           (string name, EVENT_MODE mode, ref Event _event)
        {
            RESULT result   = RESULT.OK;
            IntPtr eventraw = new IntPtr();
            Event  eventnew = null;

            try
            {
                result = FMOD_EventGroup_GetEvent(eventgroupraw, name, mode, ref eventraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (_event == null)
            {
                eventnew = new Event();
                eventnew.setRaw(eventraw);
                _event = eventnew;
            }
            else
            {
                _event.setRaw(eventraw);
            }

            return result;
        }
        public RESULT getEventByIndex    (int index, EVENT_MODE mode, ref Event _event)
        {
            RESULT result   = RESULT.OK;
            IntPtr eventraw = new IntPtr();
            Event  eventnew = null;

            try
            {
                result = FMOD_EventGroup_GetEventByIndex(eventgroupraw, index, mode, ref eventraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (_event == null)
            {
                eventnew = new Event();
                eventnew.setRaw(eventraw);
                _event = eventnew;
            }
            else
            {
                _event.setRaw(eventraw);
            }

            return result;
        }
        public RESULT getNumEvents       (ref int numevents)
        {
            return FMOD_EventGroup_GetNumEvents(eventgroupraw, ref numevents);
        }
        public RESULT getProperty        (string propertyname, IntPtr value)
        {
            return FMOD_EventGroup_GetProperty(eventgroupraw, propertyname, value);
        }
        public RESULT getPropertyByIndex (int propertyindex, IntPtr value)
        {
            return FMOD_EventGroup_GetPropertyByIndex(eventgroupraw, propertyindex, value);
        }
        public RESULT getNumProperties   (ref int numproperties)
        {
            return FMOD_EventGroup_GetNumProperties(eventgroupraw, ref numproperties);
        }
        public RESULT getState           (ref EVENT_STATE state)
        {
            return FMOD_EventGroup_GetState(eventgroupraw, ref state);
        }
        public RESULT setUserData        (IntPtr userdata)
        {
            return FMOD_EventGroup_SetUserData(eventgroupraw, userdata);
        }
        public RESULT getUserData        (ref IntPtr userdata)
        {
            return FMOD_EventGroup_GetUserData(eventgroupraw, ref userdata);
        }

        public RESULT getMemoryInfo      (uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details)
        {
            return FMOD_EventGroup_GetMemoryInfo(eventgroupraw, memorybits, event_memorybits, ref memoryused, ref memoryused_details);
        }


        #region importfunctions
        
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetInfo            (IntPtr eventgroup, ref int index, ref IntPtr name);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_LoadEventData      (IntPtr eventgroup, EVENT_RESOURCE resource, EVENT_MODE mode);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_FreeEventData      (IntPtr eventgroup, IntPtr _event, int waituntilready);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetGroup           (IntPtr eventgroup, string name, int cacheevents, ref IntPtr group);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetGroupByIndex    (IntPtr eventgroup, int index, int cacheevents, ref IntPtr group);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetParentGroup     (IntPtr eventgroup, ref IntPtr group);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetParentProject   (IntPtr eventgroup, ref IntPtr project);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetNumGroups       (IntPtr eventgroup, ref int numgroups);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetEvent           (IntPtr eventgroup, string name, EVENT_MODE mode, ref IntPtr _event);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetEventByIndex    (IntPtr eventgroup, int index, EVENT_MODE mode, ref IntPtr _event);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetNumEvents       (IntPtr eventgroup, ref int numevents);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetProperty        (IntPtr eventgroup, string propertyname, IntPtr value);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetPropertyByIndex (IntPtr eventgroup, int propertyindex, IntPtr value);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetNumProperties   (IntPtr eventgroup, ref int numproperties);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetState           (IntPtr eventgroup, ref EVENT_STATE state);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_SetUserData        (IntPtr eventgroup, IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetUserData        (IntPtr eventgroup, ref IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventGroup_GetMemoryInfo      (IntPtr eventgroup, uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details);

        #endregion

        #region wrapperinternal

        private IntPtr eventgroupraw;

        public void setRaw(IntPtr eventgroup)
        {
            eventgroupraw = new IntPtr();
            eventgroupraw = eventgroup;
        }

        public IntPtr getRaw()
        {
            return eventgroupraw;
        }

        #endregion
    }


    /*
       'EventCategory' API
    */
    public class EventCategory
    {
        public RESULT getInfo            (ref int index, ref IntPtr name)
        {
            return FMOD_EventCategory_GetInfo(eventcategoryraw, ref index, ref name);
        }
        public RESULT getCategory        (string name, ref EventCategory category)
        {
            RESULT      result        = RESULT.OK;
            IntPtr      categoryraw = new IntPtr();
            EventCategory  categorynew = null;

            try
            {
                result = FMOD_EventCategory_GetCategory(eventcategoryraw, name, ref categoryraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (category == null)
            {
                categorynew = new EventCategory();
                categorynew.setRaw(categoryraw);
                category = categorynew;
            }
            else
            {
                category.setRaw(categoryraw);
            }

            return result;
        }
        public RESULT getCategoryByIndex (int index, ref EventCategory category)
        {
            RESULT      result        = RESULT.OK;
            IntPtr      categoryraw = new IntPtr();
            EventCategory  categorynew = null;

            try
            {
                result = FMOD_EventCategory_GetCategoryByIndex(eventcategoryraw, index, ref categoryraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (category == null)
            {
                categorynew = new EventCategory();
                categorynew.setRaw(categoryraw);
                category = categorynew;
            }
            else
            {
                category.setRaw(categoryraw);
            }

            return result;
        }
        public RESULT getNumCategories   (ref int numcategories)
        {
            return FMOD_EventCategory_GetNumCategories(eventcategoryraw, ref numcategories);
        }
        public RESULT getEventByIndex    (int index, EVENT_MODE mode, ref Event _event)
        {
            RESULT result   = RESULT.OK;
            IntPtr eventraw = new IntPtr();
            Event  eventnew = null;

            try
            {
                result = FMOD_EventCategory_GetEventByIndex(eventcategoryraw, index, mode, ref eventraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (_event == null)
            {
                eventnew = new Event();
                eventnew.setRaw(eventraw);
                _event = eventnew;
            }
            else
            {
                _event.setRaw(eventraw);
            }

            return result;
        }
        public RESULT getNumEvents       (ref int numevents)
        {
            return FMOD_EventCategory_GetNumEvents(eventcategoryraw, ref numevents);
        }
        public RESULT getParentCategory  (ref EventCategory category)
        {
            RESULT result = RESULT.OK;
            IntPtr categoryraw = new IntPtr();
            EventCategory categorynew = null;

            try
            {
                result = FMOD_EventCategory_GetParentCategory(eventcategoryraw, ref categoryraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (category == null)
            {
                categorynew = new EventCategory();
                categorynew.setRaw(categoryraw);
                category = categorynew;
            }
            else
            {
                category.setRaw(categoryraw);
            }

            return result;
        }

        public RESULT stopAllEvents      ()
        {
            return FMOD_EventCategory_StopAllEvents(eventcategoryraw);
        }
        public RESULT setVolume          (float volume)
        {
            return FMOD_EventCategory_SetVolume(eventcategoryraw, volume);
        }
        public RESULT getVolume          (ref float volume)
        {
            return FMOD_EventCategory_GetVolume(eventcategoryraw, ref volume);
        }
        public RESULT setPitch           (float pitch, EVENT_PITCHUNITS units)
        {
            return FMOD_EventCategory_SetPitch(eventcategoryraw, pitch, units);
        }
        public RESULT getPitch           (ref float pitch, EVENT_PITCHUNITS units)
        {
            return FMOD_EventCategory_GetPitch(eventcategoryraw, ref pitch, units);
        }
        public RESULT setPaused          (bool paused)
        {
            return FMOD_EventCategory_SetPaused(eventcategoryraw, (paused ? 1 : 0));
        }
        public RESULT getPaused          (ref bool paused)
        {
            RESULT result = RESULT.OK;
            int p = 0;

            result = FMOD_EventCategory_GetPaused(eventcategoryraw, ref p);

            paused = (p != 0);

            return result;
        }
        public RESULT setMute            (bool mute)
        {
            return FMOD_EventCategory_SetMute(eventcategoryraw, (mute ? 1 : 0));
        }
        public RESULT getMute            (ref bool mute)
        {
            RESULT result = RESULT.OK;
            int m = 0;

            result = FMOD_EventCategory_GetMute(eventcategoryraw, ref m);

            mute = (m != 0);

            return result;
        }
        public RESULT getChannelGroup    (ref ChannelGroup channelgroup)
        {
            RESULT result   = RESULT.OK;
            IntPtr cgraw = new IntPtr();
            ChannelGroup  cgnew = null;

            try
            {
                result = FMOD_EventCategory_GetChannelGroup(eventcategoryraw, ref cgraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (channelgroup == null)
            {
                cgnew = new ChannelGroup();
                cgnew.setRaw(cgraw);
                channelgroup = cgnew;
            }
            else
            {
                channelgroup.setRaw(cgraw);
            }

            return result;
        }
        public RESULT setUserData        (IntPtr userdata)
        {
            return FMOD_EventCategory_SetUserData(eventcategoryraw, userdata);
        }
        public RESULT getUserData        (ref IntPtr userdata)
        {
            return FMOD_EventCategory_GetUserData(eventcategoryraw, ref userdata);
        }

        public RESULT getMemoryInfo      (uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details)
        {
            return FMOD_EventCategory_GetMemoryInfo(eventcategoryraw, memorybits, event_memorybits, ref memoryused, ref memoryused_details);
        }

        #region importfunctions

        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetInfo         (IntPtr eventcategory, ref int index, ref IntPtr name);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetCategory     (IntPtr eventcategory, string name, ref IntPtr category);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetCategoryByIndex(IntPtr eventcategory, int index, ref IntPtr category);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetNumCategories(IntPtr eventcategory, ref int numcategories);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetEventByIndex (IntPtr eventcategory, int index, EVENT_MODE mode, ref IntPtr _event);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetNumEvents    (IntPtr eventcategory, ref int numevents);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetParentCategory(IntPtr eventcategory, ref IntPtr category);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_StopAllEvents   (IntPtr eventcategory);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_SetVolume       (IntPtr eventcategory, float volume);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetVolume       (IntPtr eventcategory, ref float volume);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_SetPitch        (IntPtr eventcategory, float pitch, EVENT_PITCHUNITS units);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetPitch        (IntPtr eventcategory, ref float pitch, EVENT_PITCHUNITS units);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_SetPaused       (IntPtr eventcategory, int paused);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetPaused       (IntPtr eventcategory, ref int paused);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_SetMute         (IntPtr eventcategory, int mute);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetMute         (IntPtr eventcategory, ref int mute);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetChannelGroup (IntPtr eventcategory, ref IntPtr channelgroup);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_SetUserData     (IntPtr eventcategory, IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetUserData     (IntPtr eventcategory, ref IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventCategory_GetMemoryInfo   (IntPtr eventcategory, uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details);

        #endregion

        #region wrapperinternal

        private IntPtr eventcategoryraw;

        public void setRaw(IntPtr eventcategory)
        {
            eventcategoryraw = new IntPtr();
            eventcategoryraw = eventcategory;
        }

        public IntPtr getRaw()
        {
            return eventcategoryraw;
        }

        #endregion
    }


    /*
       'Event' API
    */
    public class Event
    {
        public RESULT release                    ()
        {
            return FMOD_Event_Release(eventraw, 0, 1);
        }
        public RESULT release                    (bool freeeventdata, bool waituntilready)
        {
            return FMOD_Event_Release(eventraw, freeeventdata ? 1 : 0, waituntilready ? 1 : 0);
        }

        public RESULT start                      ()
        {
            return FMOD_Event_Start(eventraw);
        }
        public RESULT stop                       ()
        {
            return FMOD_Event_Stop(eventraw, 1);
        }
        public RESULT stop                       (bool immediate)
        {
            return FMOD_Event_Stop(eventraw, (immediate ? 1 : 0));
        }

        public RESULT getInfo                    (ref int index, ref IntPtr name, ref EVENT_INFO info)
        {
            return FMOD_Event_GetInfo(eventraw, ref index, ref name, ref info);
        }
        public RESULT getState                   (ref EVENT_STATE state)
        {
            return FMOD_Event_GetState(eventraw, ref state);
        }
        public RESULT getParentGroup             (ref EventGroup group)
        {
            RESULT result            = RESULT.OK;
            IntPtr eventgroupraw     = new IntPtr();
            EventGroup eventgroupnew = null;

            try
            {
                result = FMOD_Event_GetParentGroup(eventraw, ref eventgroupraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (group == null)
            {
                eventgroupnew = new EventGroup();
                eventgroupnew.setRaw(eventgroupraw);
                group = eventgroupnew;
            }
            else
            {
                group.setRaw(eventgroupraw);
            }

            return result;
        }
        public RESULT getChannelGroup            (ref ChannelGroup channelgroup)
        {
            RESULT result   = RESULT.OK;
            IntPtr cgraw = new IntPtr();
            ChannelGroup  cgnew = null;

            try
            {
                result = FMOD_Event_GetChannelGroup(eventraw, ref cgraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (channelgroup == null)
            {
                cgnew = new ChannelGroup();
                cgnew.setRaw(cgraw);
                channelgroup = cgnew;
            }
            else
            {
                channelgroup.setRaw(cgraw);
            }

            return result;
        }
        public RESULT setCallback                (EVENT_CALLBACK callback, IntPtr userdata)
        {
            return FMOD_Event_SetCallback(eventraw, callback, userdata);
        }

        public RESULT getParameter               (string name, ref EventParameter parameter)
        {
            RESULT result               = RESULT.OK;
            IntPtr parameterraw         = new IntPtr();
            EventParameter parameternew = null;

            try
            {
                result = FMOD_Event_GetParameter(eventraw, name, ref parameterraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (parameter == null)
            {
                parameternew = new EventParameter();
                parameternew.setRaw(parameterraw);
                parameter = parameternew;
            }
            else
            {
                parameter.setRaw(parameterraw);
            }

            return result;
        }
        public RESULT getParameterByIndex        (int index, ref EventParameter parameter)
        {
            RESULT result               = RESULT.OK;
            IntPtr parameterraw         = new IntPtr();
            EventParameter parameternew = null;

            try
            {
                result = FMOD_Event_GetParameterByIndex(eventraw, index, ref parameterraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (parameter == null)
            {
                parameternew = new EventParameter();
                parameternew.setRaw(parameterraw);
                parameter = parameternew;
            }
            else
            {
                parameter.setRaw(parameterraw);
            }

            return result;
        }
        public RESULT getNumParameters           (ref int numparameters)
        {
            return FMOD_Event_GetNumParameters(eventraw, ref numparameters);
        }
        public RESULT getProperty                (string propertyname, IntPtr value, bool this_instance)
        {
            return FMOD_Event_GetProperty(eventraw, propertyname, value, (this_instance ? 1 : 0));
        }
        public RESULT getPropertyByIndex         (int propertyindex, IntPtr value, bool this_instance)
        {
            return FMOD_Event_GetPropertyByIndex(eventraw, propertyindex, value, (this_instance ? 1 : 0));
        }
        public RESULT setProperty                (string propertyname, IntPtr value, bool this_instance)
        {
            return FMOD_Event_SetProperty(eventraw, propertyname, value, (this_instance ? 1 : 0));
        }
        public RESULT setPropertyByIndex         (int propertyindex, IntPtr value, bool this_instance)
        {
            return FMOD_Event_SetPropertyByIndex(eventraw, propertyindex, value, (this_instance ? 1 : 0));
        }
        public RESULT getNumProperties           (ref int numproperties)
        {
            return FMOD_Event_GetNumProperties(eventraw, ref numproperties);
        }
        public RESULT getPropertyInfo            (ref int propertyindex, IntPtr propertyname, ref EVENTPROPERTY_TYPE type)
        {
            return FMOD_Event_GetPropertyInfo(eventraw, ref propertyindex, propertyname, ref type);
        }
        public RESULT getCategory                (ref EventCategory category)
        {
            RESULT result             = RESULT.OK;
            IntPtr categoryraw        = new IntPtr();
            EventCategory categorynew = null;

            try
            {
                result = FMOD_Event_GetCategory(eventraw, ref categoryraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (category == null)
            {
                categorynew = new EventCategory();
                categorynew.setRaw(categoryraw);
                category = categorynew;
            }
            else
            {
                category.setRaw(categoryraw);
            }

            return result;
        }

        public RESULT setVolume                  (float volume)
        {
            return FMOD_Event_SetVolume(eventraw, volume);
        }
        public RESULT getVolume                  (ref float volume)
        {
            return FMOD_Event_GetVolume(eventraw, ref volume);
        }
        public RESULT setPitch                   (float pitch, EVENT_PITCHUNITS units)
        {
            return FMOD_Event_SetPitch(eventraw, pitch, units);
        }
        public RESULT getPitch                   (ref float pitch, EVENT_PITCHUNITS units)
        {
            return FMOD_Event_GetPitch(eventraw, ref pitch, units);
        }
        public RESULT setPaused                  (bool paused)
        {
            return FMOD_Event_SetPaused(eventraw, (paused ? 1 : 0));
        }
        public RESULT getPaused                  (ref bool paused)
        {
            RESULT result = RESULT.OK;
            int p = 0;

            result = FMOD_Event_GetPaused(eventraw, ref p);

            paused = (p != 0);

            return result;
        }
        public RESULT setMute                    (bool mute)
        {
            return FMOD_Event_SetMute(eventraw, (mute ? 1 : 0));
        }
        public RESULT getMute                    (ref bool mute)
        {
            RESULT result = RESULT.OK;
            int m = 0;

            result = FMOD_Event_GetMute(eventraw, ref m);

            mute = (m != 0);

            return result;
        }
        public RESULT set3DAttributes            (ref VECTOR position, ref VECTOR velocity)
        {
            return FMOD_Event_Set3DAttributes(eventraw, ref position, ref velocity, (IntPtr)null);
        }
        public RESULT get3DAttributes            (ref VECTOR position, ref VECTOR velocity)
        {
            return FMOD_Event_Get3DAttributes(eventraw, ref position, ref velocity, (IntPtr)null);
        }
        public RESULT set3DAttributes            (ref VECTOR position, ref VECTOR velocity, ref VECTOR orientation)
        {
            return FMOD_Event_Set3DAttributes(eventraw, ref position, ref velocity, ref orientation);
        }
        public RESULT get3DAttributes            (ref VECTOR position, ref VECTOR velocity, ref VECTOR orientation)
        {
            return FMOD_Event_Get3DAttributes(eventraw, ref position, ref velocity, ref orientation);
        }
        public RESULT set3DOcclusion             (float directocclusion, float reverbocclusion)
        {
            return FMOD_Event_Set3DOcclusion(eventraw, directocclusion, reverbocclusion);
        }
        public RESULT get3DOcclusion             (ref float directocclusion, ref float reverbocclusion)
        {
            return FMOD_Event_Get3DOcclusion(eventraw, ref directocclusion, ref reverbocclusion);
        }
        public RESULT setReverbProperties        (ref REVERB_CHANNELPROPERTIES props)
        {
            return FMOD_Event_SetReverbProperties(eventraw, ref props);
        }
        public RESULT getReverbProperties        (ref REVERB_CHANNELPROPERTIES props)
        {
            return FMOD_Event_GetReverbProperties(eventraw, ref props);
        }

        public RESULT setUserData                (IntPtr userdata)
        {
            return FMOD_Event_SetUserData(eventraw, userdata);
        }
        public RESULT getUserData                (ref IntPtr userdata)
        {
            return FMOD_Event_GetUserData(eventraw, ref userdata);
        }

        public RESULT getMemoryInfo              (uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details)
        {
            return FMOD_Event_GetMemoryInfo(eventraw, memorybits, event_memorybits, ref memoryused, ref memoryused_details);
        }

        #region importfunctions

        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_Release                 (IntPtr _event, int freeeventdata, int waituntilready);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_Start                   (IntPtr _event);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_Stop                    (IntPtr _event, int immediate);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetInfo                 (IntPtr _event, ref int index, ref IntPtr name, ref EVENT_INFO info);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetState                (IntPtr _event, ref EVENT_STATE state);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetParentGroup          (IntPtr _event, ref IntPtr group);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetChannelGroup         (IntPtr _event, ref IntPtr channelgroup);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_SetCallback             (IntPtr _event, EVENT_CALLBACK callback, IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetParameter            (IntPtr _event, string name, ref IntPtr parameter);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetParameterByIndex     (IntPtr _event, int index, ref IntPtr parameter);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetNumParameters        (IntPtr _event, ref int numparameters);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetProperty             (IntPtr _event, string propertyname, IntPtr value, int this_instance);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetPropertyByIndex      (IntPtr _event, int propertyindex, IntPtr value, int this_instance);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_SetProperty             (IntPtr _event, string propertyname, IntPtr value, int this_instance);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_SetPropertyByIndex      (IntPtr _event, int propertyindex, IntPtr value, int this_instance);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetNumProperties        (IntPtr _event, ref int numproperties);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetPropertyInfo         (IntPtr _event, ref int propertyindex, IntPtr propertyname, ref EVENTPROPERTY_TYPE type);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetCategory             (IntPtr _event, ref IntPtr category);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_SetVolume               (IntPtr _event, float volume);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetVolume               (IntPtr _event, ref float volume);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_SetPitch                (IntPtr _event, float pitch, EVENT_PITCHUNITS units);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetPitch                (IntPtr _event, ref float pitch, EVENT_PITCHUNITS units);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_SetPaused               (IntPtr _event, int paused);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetPaused               (IntPtr _event, ref int paused);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_SetMute                 (IntPtr _event, int mute);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetMute                 (IntPtr _event, ref int mute);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_Set3DAttributes         (IntPtr _event, ref VECTOR position, ref VECTOR velocity, ref VECTOR orientation);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_Get3DAttributes         (IntPtr _event, ref VECTOR position, ref VECTOR velocity, ref VECTOR orientation);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_Set3DAttributes         (IntPtr _event, ref VECTOR position, ref VECTOR velocity, IntPtr orientation);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_Get3DAttributes         (IntPtr _event, ref VECTOR position, ref VECTOR velocity, IntPtr orientation);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_Set3DOcclusion          (IntPtr _event, float directocclusion, float reverbocclusion);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_Get3DOcclusion          (IntPtr _event, ref float directocclusion, ref float reverbocclusion);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_SetReverbProperties     (IntPtr _event, ref REVERB_CHANNELPROPERTIES props);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetReverbProperties     (IntPtr _event, ref REVERB_CHANNELPROPERTIES props);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_SetUserData             (IntPtr _event, IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetUserData             (IntPtr _event, ref IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_Event_GetMemoryInfo           (IntPtr _event, uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details);

        #endregion

        #region wrapperinternal

        private IntPtr eventraw;

        public void setRaw(IntPtr _event)
        {
            eventraw = new IntPtr();
            eventraw = _event;
        }

        public IntPtr getRaw()
        {
            return eventraw;
        }

        #endregion
    }


    /*
       'EventParameter' API
    */
    public class EventParameter
    {
        public RESULT getInfo                    (ref int index, ref IntPtr name)
        {
            return FMOD_EventParameter_GetInfo(eventparameterraw, ref index, ref name);
        }
        public RESULT getRange                   (ref float rangemin, ref float rangemax)
        {
            return FMOD_EventParameter_GetRange(eventparameterraw, ref rangemin, ref rangemax);
        }
        public RESULT setValue                   (float value)
        {
            return FMOD_EventParameter_SetValue(eventparameterraw, value);
        }
        public RESULT getValue                   (ref float value)
        {
            return FMOD_EventParameter_GetValue(eventparameterraw, ref value);
        }
        public RESULT setVelocity                (float value)
        {
            return FMOD_EventParameter_SetVelocity(eventparameterraw, value);
        }
        public RESULT getVelocity                (ref float value)
        {
            return FMOD_EventParameter_GetVelocity(eventparameterraw, ref value);
        }
        public RESULT setSeekSpeed               (float value)
        {
            return FMOD_EventParameter_SetSeekSpeed(eventparameterraw, value);
        }
        public RESULT getSeekSpeed               (ref float value)
        {
            return FMOD_EventParameter_GetSeekSpeed(eventparameterraw, ref value);
        }
        public RESULT setUserData                (IntPtr userdata)
        {
            return FMOD_EventParameter_SetUserData(eventparameterraw, userdata);
        }
        public RESULT getUserData                (ref IntPtr userdata)
        {
            return FMOD_EventParameter_GetUserData(eventparameterraw, ref userdata);
        }
        public RESULT keyOff                     ()
        {
            return FMOD_EventParameter_KeyOff(eventparameterraw);
        }

        public RESULT getMemoryInfo              (uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details)
        {
            return FMOD_EventParameter_GetMemoryInfo(eventparameterraw, memorybits, event_memorybits, ref memoryused, ref memoryused_details);
        }

        #region importfunctions

        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventParameter_GetInfo        (IntPtr eventparameter , ref int index, ref IntPtr name);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventParameter_GetRange       (IntPtr eventparameter, ref float rangemin, ref float rangemax);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventParameter_SetValue       (IntPtr eventparameter, float value);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventParameter_GetValue       (IntPtr eventparameter, ref float value);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventParameter_SetVelocity    (IntPtr eventparameter, float value);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventParameter_GetVelocity    (IntPtr eventparameter, ref float value);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventParameter_SetSeekSpeed   (IntPtr eventparameter, float value);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventParameter_GetSeekSpeed   (IntPtr eventparameter, ref float value);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventParameter_SetUserData    (IntPtr eventparameter, IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventParameter_GetUserData    (IntPtr eventparameter, ref IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventParameter_KeyOff         (IntPtr eventparameter);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventParameter_GetMemoryInfo  (IntPtr eventparameter, uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details);

        #endregion

        #region wrapperinternal

        private IntPtr eventparameterraw;

        public void setRaw(IntPtr eventparameter)
        {
            eventparameterraw = new IntPtr();
            eventparameterraw = eventparameter;
        }

        public IntPtr getRaw()
        {
            return eventparameterraw;
        }

        #endregion
    }


    /*
       'EventReverb' API
    */
    public class EventReverb
    {
        public RESULT release            ()
        {
            return FMOD_EventReverb_Release(eventreverbraw);
        }
        public RESULT set3DAttributes    (ref VECTOR position, float mindistance, float maxdistance)
        {
            return FMOD_EventReverb_Set3DAttributes(eventreverbraw, ref position, mindistance, maxdistance);
        }
        public RESULT get3DAttributes    (ref VECTOR position, ref float mindistance, ref float maxdistance)
        {
            return FMOD_EventReverb_Get3DAttributes(eventreverbraw, ref position, ref mindistance, ref maxdistance);
        }
        public RESULT setProperties      (ref REVERB_PROPERTIES props)
        {
            return FMOD_EventReverb_SetProperties(eventreverbraw, ref props);
        }
        public RESULT getProperties      (ref REVERB_PROPERTIES props)
        {
            return FMOD_EventReverb_GetProperties(eventreverbraw, ref props);
        }
        public RESULT setActive          (bool active)
        {
            return FMOD_EventReverb_SetActive(eventreverbraw, (active ? 1 : 0));
        }
        public RESULT getActive          (ref bool active)
        {
            RESULT result = RESULT.OK;
            int a = 0;

            result = FMOD_EventReverb_GetActive(eventreverbraw, ref a);

            active = (a != 0);

            return result;
        }
        public RESULT setUserData        (IntPtr userdata)
        {
            return FMOD_EventReverb_SetUserData(eventreverbraw, userdata);
        }
        public RESULT getUserData        (ref IntPtr userdata)
        {
            return FMOD_EventReverb_GetUserData(eventreverbraw, ref userdata);
        }

        public RESULT getMemoryInfo      (uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details)
        {
            return FMOD_EventReverb_GetMemoryInfo(eventreverbraw, memorybits, event_memorybits, ref memoryused, ref memoryused_details);
        }

        #region importfunctions

        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventReverb_Release           (IntPtr eventreverb);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventReverb_Set3DAttributes   (IntPtr eventreverb, ref VECTOR position, float mindistance, float maxdistance);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventReverb_Get3DAttributes   (IntPtr eventreverb, ref VECTOR position, ref float mindistance, ref float maxdistance);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventReverb_SetProperties     (IntPtr eventreverb, ref REVERB_PROPERTIES props);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventReverb_GetProperties     (IntPtr eventreverb, ref REVERB_PROPERTIES props);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventReverb_SetActive         (IntPtr eventreverb, int active);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventReverb_GetActive         (IntPtr eventreverb, ref int active);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventReverb_SetUserData       (IntPtr eventreverb, IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventReverb_GetUserData       (IntPtr eventreverb, ref IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventReverb_GetMemoryInfo     (IntPtr eventreverb, uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details);

        #endregion

        #region wrapperinternal

        private IntPtr eventreverbraw;

        public void setRaw(IntPtr eventreverb)
        {
            eventreverbraw = new IntPtr();
            eventreverbraw = eventreverb;
        }

        public IntPtr getRaw()
        {
            return eventreverbraw;
        }

        #endregion
    }

    /*
       'EventQueueEntry' API
    */
    public class EventQueueEntry
    {
        public RESULT release()
        {
            return FMOD_EventQueueEntry_Release(eventqueueentryraw);
        }
        public RESULT getInfoOnlyEvent(ref Event infoonlyevent)
        {
            RESULT result = RESULT.OK;
            IntPtr eventraw = new IntPtr();

            try
            {
                result = FMOD_EventQueueEntry_GetInfoOnlyEvent(eventqueueentryraw, ref eventraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (infoonlyevent == null)
            {
                Event eventnew = new Event();
                eventnew.setRaw(eventraw);
                infoonlyevent = eventnew;
            }
            else
            {
                infoonlyevent.setRaw(eventraw);
            }

            return result;
        }
        public RESULT getRealEvent(ref Event realevent)
        {
            RESULT result = RESULT.OK;
            IntPtr eventraw = new IntPtr();

            try
            {
                result = FMOD_EventQueueEntry_GetRealEvent(eventqueueentryraw, ref eventraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (realevent == null)
            {
                Event eventnew = new Event();
                eventnew.setRaw(eventraw);
                realevent = eventnew;
            }
            else
            {
                realevent.setRaw(eventraw);
            }

            return result;
        }
        public RESULT setPriority(byte priority)
        {
            return FMOD_EventQueueEntry_SetPriority(eventqueueentryraw, priority);
        }
        public RESULT getPriority(ref byte priority)
        {
            return FMOD_EventQueueEntry_GetPriority(eventqueueentryraw, ref priority);
        }
        public RESULT setExpiryTime(uint expirytime)
        {
            return FMOD_EventQueueEntry_SetExpiryTime(eventqueueentryraw, expirytime);
        }
        public RESULT getExpiryTime(ref uint expirytime)
        {
            return FMOD_EventQueueEntry_GetExpiryTime(eventqueueentryraw, ref expirytime);
        }
        public RESULT setDelayTime(uint delay)
        {
            return FMOD_EventQueueEntry_SetDelayTime(eventqueueentryraw, delay);
        }
        public RESULT getDelayTime(ref uint delay)
        {
            return FMOD_EventQueueEntry_GetDelayTime(eventqueueentryraw, ref delay);
        }
        public RESULT setInterrupt(bool interrupt)
        {
            return FMOD_EventQueueEntry_SetInterrupt(eventqueueentryraw, interrupt ? 1 : 0);
        }
        public RESULT getInterrupt(ref bool interrupt)
        {
            RESULT result = RESULT.OK;
            int i = 0;

            result = FMOD_EventQueueEntry_GetInterrupt(eventqueueentryraw, ref i);

            interrupt = (i != 0);

            return result;
        }
        public RESULT setCrossfadeTime(int crossfade)
        {
            return FMOD_EventQueueEntry_SetCrossfadeTime(eventqueueentryraw, crossfade);
        }
        public RESULT getCrossfadeTime(ref int crossfade)
        {
            return FMOD_EventQueueEntry_GetCrossfadeTime(eventqueueentryraw, ref crossfade);
        }
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_EventQueueEntry_SetUserData(eventqueueentryraw, userdata);
        }
        public RESULT getUserData(ref IntPtr userdata)
        {
            return FMOD_EventQueueEntry_GetUserData(eventqueueentryraw, ref userdata);
        }
        public RESULT getMemoryInfo(uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details)
        {
            return FMOD_EventQueueEntry_GetMemoryInfo(eventqueueentryraw, memorybits, event_memorybits, ref memoryused, ref memoryused_details);
        }

        #region importfunctions

        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_Release           (IntPtr eventqueueentry);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_GetInfoOnlyEvent  (IntPtr eventqueueentry, ref IntPtr infoonlyevent);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_GetRealEvent      (IntPtr eventqueueentry, ref IntPtr realevent);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_SetPriority       (IntPtr eventqueueentry, byte priority);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_GetPriority       (IntPtr eventqueueentry, ref byte priority);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_SetExpiryTime     (IntPtr eventqueueentry, uint expirytime);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_GetExpiryTime     (IntPtr eventqueueentry, ref uint expirytime);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_SetDelayTime      (IntPtr eventqueueentry, uint delay);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_GetDelayTime      (IntPtr eventqueueentry, ref uint delay);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_SetInterrupt      (IntPtr eventqueueentry, int interrupt);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_GetInterrupt      (IntPtr eventqueueentry, ref int interrupt);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_SetCrossfadeTime  (IntPtr eventqueueentry, int crossfade);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_GetCrossfadeTime  (IntPtr eventqueueentry, ref int crossfade);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_SetUserData       (IntPtr eventqueueentry, IntPtr userdata);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_GetUserData       (IntPtr eventqueueentry, ref IntPtr userdata);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueueEntry_GetMemoryInfo     (IntPtr eventqueueentry, uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details);

        #endregion

        #region wrapperinternal

        private IntPtr eventqueueentryraw;

        public void setRaw(IntPtr eventqueueentry)
        {
            eventqueueentryraw = new IntPtr();
            eventqueueentryraw = eventqueueentry;
        }

        public IntPtr getRaw()
        {
            return eventqueueentryraw;
        }

        #endregion
    }

    /*
       'EventQueue' API
    */
    public class EventQueue
    {
        public RESULT release()
        {
            return FMOD_EventQueue_Release(eventqueueraw);
        }
        public RESULT add(EventQueueEntry entry, bool allow_duplicates)
        {
            return FMOD_EventQueue_Add(eventqueueraw, entry.getRaw(), allow_duplicates ? 1 : 0);
        }
        public RESULT remove(EventQueueEntry entry)
        {
            return FMOD_EventQueue_Remove(eventqueueraw, entry.getRaw());
        }
        public RESULT removeHead()
        {
            return FMOD_EventQueue_RemoveHead(eventqueueraw);
        }
        public RESULT clear(bool stopallevents)
        {
            return FMOD_EventQueue_Clear(eventqueueraw, stopallevents ? 1 : 0);
        }
        public RESULT findFirstEntry(ref EventQueueEntry entry)
        {
            IntPtr raw = entry.getRaw();
            RESULT result = FMOD_EventQueue_FindFirstEntry(eventqueueraw, ref raw);
            entry.setRaw(raw);
            return result;
        }
        public RESULT findNextEntry(ref EventQueueEntry entry)
        {
            IntPtr raw = entry.getRaw();
            RESULT result = FMOD_EventQueue_FindNextEntry(eventqueueraw, ref raw);
            entry.setRaw(raw);
            return result;
        }
        public RESULT setPaused(bool paused)
        {
            return FMOD_EventQueue_SetPaused(eventqueueraw, paused ? 1 : 0);
        }
        public RESULT getPaused(ref bool paused)
        {
            RESULT result = RESULT.OK;
            int p = 0;

            result = FMOD_EventQueue_GetPaused(eventqueueraw, ref p);

            paused = (p != 0);

            return result;
        }
        public RESULT includeDuckingCategory(EventCategory category, float ducked_volume, float unducked_volume, uint duck_time, uint unduck_time)
        {
            return FMOD_EventQueue_IncludeDuckingCategory(eventqueueraw, category.getRaw(), ducked_volume, unducked_volume, duck_time, unduck_time);
        }
        public RESULT excludeDuckingCategory(EventCategory category)
        {
            return FMOD_EventQueue_ExcludeDuckingCategory(eventqueueraw, category.getRaw());
        }
        public RESULT setCallback(EVENTQUEUE_CALLBACK callback, IntPtr callbackuserdata)
        {
            return FMOD_EventQueue_SetCallback(eventqueueraw, callback, callbackuserdata);
        }
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_EventQueue_SetUserData(eventqueueraw, userdata);
        }
        public RESULT getUserData(ref IntPtr userdata)
        {
            return FMOD_EventQueue_GetUserData(eventqueueraw, ref userdata);
        }
        public RESULT dump()
        {
            return FMOD_EventQueue_Dump(eventqueueraw);
        }
        public RESULT getMemoryInfo(uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details)
        {
            return FMOD_EventQueue_GetMemoryInfo(eventqueueraw, memorybits, event_memorybits, ref memoryused, ref memoryused_details);
        }

        #region importfunctions

        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_Release(IntPtr eventqueue);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_Add(IntPtr eventqueue, IntPtr entry, int allow_duplicates);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_Remove(IntPtr eventqueue, IntPtr entry);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_RemoveHead(IntPtr eventqueue);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_Clear(IntPtr eventqueue, int stopallevents);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_FindFirstEntry(IntPtr eventqueue, ref IntPtr entry);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_FindNextEntry(IntPtr eventqueue, ref IntPtr entry);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_SetPaused(IntPtr eventqueue, int paused);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_GetPaused(IntPtr eventqueue, ref int paused);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_IncludeDuckingCategory(IntPtr eventqueue, IntPtr category, float ducked_volume, float unducked_volume, uint duck_time, uint unduck_time);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_ExcludeDuckingCategory(IntPtr eventqueue, IntPtr category);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_SetCallback(IntPtr eventqueue, EVENTQUEUE_CALLBACK callback, IntPtr callbackuserdata);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_SetUserData(IntPtr eventqueue, IntPtr userdata);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_GetUserData(IntPtr eventqueue, ref IntPtr userdata);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_Dump(IntPtr eventqueue);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_EventQueue_GetMemoryInfo(IntPtr eventqueue, uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details);
        #endregion

        #region wrapperinternal

        private IntPtr eventqueueraw;

        public void setRaw(IntPtr eventqueue)
        {
            eventqueueraw = new IntPtr();
            eventqueueraw = eventqueue;
        }

        public IntPtr getRaw()
        {
            return eventqueueraw;
        }

        #endregion
    }

    /*
        'MusicSystem' API 
    */
    public class MusicSystem
    {
        public RESULT reset              ()
        {
            return FMOD_MusicSystem_Reset(musicsystemraw);
        }
        public RESULT setVolume          (float volume)
        {
            return FMOD_MusicSystem_SetVolume(musicsystemraw, volume);
        }
        public RESULT getVolume          (ref float volume)
        {
            return FMOD_MusicSystem_GetVolume(musicsystemraw, ref volume);
        }
        public RESULT setReverbProperties(ref REVERB_CHANNELPROPERTIES props)
        {
            return FMOD_MusicSystem_GetReverbProperties(musicsystemraw, ref props);
        }
        public RESULT getReverbProperties(ref REVERB_CHANNELPROPERTIES props)
        {
            return FMOD_MusicSystem_GetReverbProperties(musicsystemraw, ref props);
        }
        public RESULT setPaused          (bool paused)
        {
            return FMOD_MusicSystem_SetPaused(musicsystemraw, paused ? 1 : 0);
        }
        public RESULT getPaused          (ref bool paused)
        {
            RESULT result;
            int p = 0;

            result = FMOD_MusicSystem_GetPaused(musicsystemraw, ref p);

            paused = (p != 0);

            return result;
        }
        public RESULT setMute            (bool mute)
        {
            return FMOD_MusicSystem_SetMute(musicsystemraw, mute ? 1 : 0);
        }
        public RESULT getMute            (ref bool mute)
        {
            RESULT result;
            int m = 0;

            result =  FMOD_MusicSystem_GetMute(musicsystemraw, ref m);

            mute = (m != 0);

            return result;
        }
        public RESULT getInfo            (ref MUSIC_INFO info)
        {
            return FMOD_MusicSystem_GetInfo(musicsystemraw, ref info);
        }
        public RESULT promptCue          (uint id)
        {
            return FMOD_MusicSystem_PromptCue(musicsystemraw, id);
        }
        public RESULT prepareCue         (uint id, ref MusicPrompt prompt)
        {
            RESULT result = RESULT.OK;
            IntPtr mpraw = new IntPtr();
            MusicPrompt mpnew = null;

            try
            {
                result = FMOD_MusicSystem_PrepareCue(musicsystemraw, id, ref mpraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (prompt == null)
            {
                mpnew = new MusicPrompt();
                mpnew.setRaw(mpraw);
                prompt = mpnew;
            }
            else
            {
                prompt.setRaw(mpraw);
            }

            return result;
        }
        public RESULT getParameterValue  (uint id, ref float parameter)
        {
            return FMOD_MusicSystem_GetParameterValue(musicsystemraw, id, ref parameter);
        }
        public RESULT setParameterValue  (uint id, float parameter)
        {
            return FMOD_MusicSystem_SetParameterValue(musicsystemraw, id, parameter);
        }

        public RESULT getCues            (ref MUSIC_ITERATOR it, string filter)
        {
            return FMOD_MusicSystem_GetCues(musicsystemraw, ref it, filter);
        }
        public RESULT getNextCue         (ref MUSIC_ITERATOR it)
        {
            return FMOD_MusicSystem_GetNextCue(musicsystemraw, ref it);
        }
        public RESULT getParameters      (ref MUSIC_ITERATOR it, string filter)
        {
            return FMOD_MusicSystem_GetParameters(musicsystemraw, ref it, filter);
        }
        public RESULT getNextParameter   (ref MUSIC_ITERATOR it)
        {
            return FMOD_MusicSystem_GetNextParameter(musicsystemraw, ref it);
        }


        public RESULT loadSoundData      (EVENT_RESOURCE resource, EVENT_MODE mode)
        {
            return FMOD_MusicSystem_LoadSoundData(musicsystemraw, resource, mode);
        }
        public RESULT freeSoundData      (bool waituntilready)
        {
            return FMOD_MusicSystem_FreeSoundData(musicsystemraw, waituntilready ? 1 : 0);
        }

        public RESULT setCallback        (MUSIC_CALLBACK callback, IntPtr userdata)
        {
            return FMOD_MusicSystem_SetCallback(musicsystemraw, callback, userdata);
        }

        public RESULT getMemoryInfo      (uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details)
        {
            return FMOD_MusicSystem_GetMemoryInfo(musicsystemraw, memorybits, event_memorybits, ref memoryused, ref memoryused_details);
        }

        #region importfunctions

        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_Reset                 (IntPtr musicsystem);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_SetVolume             (IntPtr musicsystem, float volume);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_GetVolume             (IntPtr musicsystem, ref float volume);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_SetReverbProperties   (IntPtr musicsystem, ref REVERB_CHANNELPROPERTIES props);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_GetReverbProperties   (IntPtr musicsystem, ref REVERB_CHANNELPROPERTIES props);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_SetPaused             (IntPtr musicsystem, int paused);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_GetPaused             (IntPtr musicsystem, ref int paused);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_SetMute               (IntPtr musicsystem, int mute);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_GetMute               (IntPtr musicsystem, ref int mute);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_GetInfo               (IntPtr musicsystem, ref MUSIC_INFO info);
        [DllImport(EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_PromptCue             (IntPtr musicsystem, uint id);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_PrepareCue            (IntPtr musicsystem, uint id, ref IntPtr prompt);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_GetParameterValue     (IntPtr musicsystem, uint id, ref float parameter);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_SetParameterValue     (IntPtr musicsystem, uint id, float parameter);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_GetCues               (IntPtr musicsystem, ref MUSIC_ITERATOR it, string filter);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_GetNextCue            (IntPtr musicsystem, ref MUSIC_ITERATOR it);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_GetParameters         (IntPtr musicsystem, ref MUSIC_ITERATOR it, string filter);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_GetNextParameter      (IntPtr musicsystem, ref MUSIC_ITERATOR it);

        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_LoadSoundData         (IntPtr musicsystem, EVENT_RESOURCE resource, EVENT_MODE mode);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_FreeSoundData         (IntPtr musicsystem, int waituntilready);

        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_SetCallback           (IntPtr musicsystem, MUSIC_CALLBACK callback, IntPtr userdata);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicSystem_GetMemoryInfo         (IntPtr musicsystem, uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details);


        #endregion

        #region wrapperinternal

        private IntPtr musicsystemraw;

        public void setRaw(IntPtr musicsystem)
        {
            musicsystemraw = new IntPtr();
            musicsystemraw = musicsystem;
        }

        public IntPtr getRaw()
        {
            return musicsystemraw;
        }

        #endregion
    }

    /*
        'MusicPrompt' API
    */
    public class MusicPrompt
    {
        public RESULT release            ()
        {
            return FMOD_MusicPrompt_Release(musicpromptraw);
        }
        public RESULT begin              ()
        {
            return FMOD_MusicPrompt_Begin(musicpromptraw);
        }
        public RESULT end                ()
        {
            return FMOD_MusicPrompt_End(musicpromptraw);
        }
        public RESULT isActive(ref bool active)
        {
            RESULT result;
            int a = 0;

            result = FMOD_MusicPrompt_IsActive(musicpromptraw, ref a);

            active = (a != 0);

            return result;
        }

        public RESULT getMemoryInfo(uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details)
        {
            return FMOD_MusicPrompt_GetMemoryInfo(musicpromptraw, memorybits, event_memorybits, ref memoryused, ref memoryused_details);
        }

        #region importfunctions

        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicPrompt_Release           (IntPtr musicprompt);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicPrompt_Begin             (IntPtr musicprompt);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicPrompt_End               (IntPtr musicprompt);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicPrompt_IsActive          (IntPtr musicprompt, ref int active);
        [DllImport (EVENT_VERSION.dll)]
        private static extern RESULT FMOD_MusicPrompt_GetMemoryInfo     (IntPtr musicprompt, uint memorybits, uint event_memorybits, ref uint memoryused, ref MEMORY_USAGE_DETAILS memoryused_details);

        #endregion

        #region wrapperinternal

        private IntPtr musicpromptraw;

        public void setRaw(IntPtr musicprompt)
        {
            musicpromptraw = new IntPtr();
            musicpromptraw = musicprompt;
        }

        public IntPtr getRaw()
        {
            return musicpromptraw;
        }

        #endregion
    }
}
