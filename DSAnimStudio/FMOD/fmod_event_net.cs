#define WIN64
/* ========================================================================================== */
/* FMOD Event Net - C# Wrapper . Copyright (c), Firelight Technologies Pty, Ltd. 2004-2016.   */
/*                                                                                            */
/*                                                                                            */
/* ========================================================================================== */

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace FMOD
{
    /*
        FMOD NetEventSystem version number.  Check this against NetEventSystem_GetVersion.
        0xaaaabbcc -> aaaa = major version number.  bb = minor version number.  cc = development version number.
    */
    public class EVENT_NET_VERSION
    {
        public const int    number = 0x00044462;
#if WIN64
        public const string dll    = "fmod_event_net64.dll";
#else
        public const string dll    = "fmod_event_net.dll";
#endif
    }

    public class NetEventSystem
    {
        /*
            Default port that the target (game) will listen on
        */
        public const ushort EVENT_NET_PORT = 17997;


        public static RESULT init(EventSystem eventsystem)
        {
            return FMOD_NetEventSystem_Init(eventsystem.getRaw(), EVENT_NET_PORT);
        }

        public static RESULT init(EventSystem eventsystem, ushort port)
        {
            return FMOD_NetEventSystem_Init(eventsystem.getRaw(), port);
        }

        public static RESULT update()
        {
            return FMOD_NetEventSystem_Update();
        }

        public static RESULT shutDown()
        {
            return FMOD_NetEventSystem_ShutDown();
        }

        public static RESULT getVersion(ref uint version)
        {
            return FMOD_NetEventSystem_GetVersion(ref version);
        }


        #region importfunctions

        [DllImport (EVENT_NET_VERSION.dll)]
        private static extern RESULT FMOD_NetEventSystem_Init       (IntPtr eventsystem, ushort port);
        [DllImport (EVENT_NET_VERSION.dll)]
        private static extern RESULT FMOD_NetEventSystem_Update     ();
        [DllImport (EVENT_NET_VERSION.dll)]
        private static extern RESULT FMOD_NetEventSystem_ShutDown   ();
        [DllImport (EVENT_NET_VERSION.dll)]
        private static extern RESULT FMOD_NetEventSystem_GetVersion (ref uint version);
        
        #endregion

    }
    
}
