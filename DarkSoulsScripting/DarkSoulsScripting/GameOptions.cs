using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public enum BloodLevel : byte
    {
        Off = 0,
        Normal = 1,
        Mild = 2
    }

    public class GameOptions : GameStruct
    {
        protected override void InitSubStructures()
        {
            
        }

        public byte CameraSpeed
        {
            get { return RByte(Address + 0x4); }
            set { WByte(Address + 0x4, value); }
        }

        public byte Vibration
        {
            get { return RByte(Address + 0x5); }
            set { WByte(Address + 0x5, value); }
        }

        public byte Brightness
        {
            get { return RByte(Address + 0x6); }
            set { WByte(Address + 0x6, value); }
        }

        public byte SoundType
        {
            get { return RByte(Address + 0x7); }
            set { WByte(Address + 0x7, value); }
        }

        public byte VolumeBGM
        {
            get { return RByte(Address + 0x8); }
            set { WByte(Address + 0x8, value); }
        }

        public byte VolumeSFX
        {
            get { return RByte(Address + 0x9); }
            set { WByte(Address + 0x9, value); }
        }

        public byte VolumeVoice
        {
            get { return RByte(Address + 0xA); }
            set { WByte(Address + 0xA, value); }
        }

        public BloodLevel Blood
        {
            get { return (BloodLevel)RByte(Address + 0xB); }
            set { WByte(Address + 0xB, (byte)value); }
        }

        public bool Subtitles
        {
            get { return RBool(Address + 0xC); }
            set { WBool(Address + 0xC, value); }
        }

        public bool HUD
        {
            get { return RBool(Address + 0xD); }
            set { WBool(Address + 0xD, value); }
        }

        public bool CameraReverseX
        {
            get { return RBool(Address + 0xE); }
            set { WBool(Address + 0xE, value); }
        }

        public bool CameraReverseY
        {
            get { return RBool(Address + 0xF); }
            set { WBool(Address + 0xF, value); }
        }

        public bool AutoLockOn
        {
            get { return RBool(Address + 0x10); }
            set { WBool(Address + 0x10, value); }
        }

        public bool CameraAutoWallRecovery
        {
            get { return RBool(Address + 0x11); }
            set { WBool(Address + 0x11, value); }
        }

        public bool JoinLeaderboard
        {
            get { return RBool(Address + 0x12); }
            set { WBool(Address + 0x12, value); }
        }

        public byte DebugRankRegisterProfileIdx
        {
            get { return RByte(Address + 0x13); }
            set { WByte(Address + 0x13, value); }
        }
    }
}
