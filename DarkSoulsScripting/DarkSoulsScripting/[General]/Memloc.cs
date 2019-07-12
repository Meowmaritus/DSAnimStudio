using DarkSoulsScripting.Injection.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsScripting
{
    public struct Memloc
    {
        public IntPtr Address
        {
            get
            {
                switch (Hook.DARKSOULS.Version)
                {
                    case DarkSoulsVersion.LatestRelease: return addrSteamRelease;
                    case DarkSoulsVersion.Debug: return addrSteamDebug;
                    default: return addr_Zero;
                }
            }
        }

        private readonly IntPtr addrSteamRelease;
        private readonly IntPtr addrSteamDebug;

        private static readonly IntPtr addr_Zero = (IntPtr)0;

        public Memloc(long addrSteamRelease, long addrSteamDebug)
        {
            this.addrSteamRelease = new IntPtr(addrSteamRelease);
            this.addrSteamDebug = new IntPtr(addrSteamDebug);
        }

        public Memloc(IntPtr addrSteamRelease, IntPtr addrSteamDebug)
        {
            this.addrSteamRelease = addrSteamRelease;
            this.addrSteamDebug = addrSteamDebug;
        }

        public Memloc(long addrSteamRelease)
        {
            this.addrSteamRelease = new IntPtr(addrSteamRelease);
            this.addrSteamDebug = IntPtr.Zero;
        }

        public Memloc(IntPtr addrSteamRelease)
        {
            this.addrSteamRelease = addrSteamRelease;
            this.addrSteamDebug = IntPtr.Zero;
        }

        public static implicit operator Memloc(int releaseAddress) => new Memloc(releaseAddress, 0);
        public static implicit operator Memloc(long releaseAddress) => new Memloc(releaseAddress, 0);
        public static implicit operator Memloc(uint releaseAddress) => new Memloc(releaseAddress, 0);
        public static implicit operator Memloc(IntPtr releaseAddress) => new Memloc(releaseAddress, IntPtr.Zero);

        public static implicit operator Memloc((long Release, long Debug) addr) => new Memloc(addr.Release, addr.Debug);
        public static implicit operator Memloc((IntPtr Release, IntPtr Debug) addr) => new Memloc(addr.Release, addr.Debug);

        public static implicit operator int(Memloc m) => m.Address.ToInt32();
        public static implicit operator uint(Memloc m) => (uint)m.Address;
        public static implicit operator long(Memloc m) => m.Address.ToInt64();
        public static implicit operator IntPtr(Memloc m) => m.Address;
    }
}
