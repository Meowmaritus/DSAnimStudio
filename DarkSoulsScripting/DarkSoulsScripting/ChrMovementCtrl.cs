using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public class ChrMovementCtrl<TChrController> : GameStruct
        where TChrController : ChrController, new()
    {
        public ChrTransform Transform { get; private set; } = null;
        public TChrController Controller { get; private set; } = null;
        public PlayerController DebugPlayerController { get; private set; } = null;

        protected override void InitSubStructures()
        {
            Transform = new ChrTransform() { AddressReadFunc = () => TransformPtr };
            Controller = new TChrController() { AddressReadFunc = () => ControllerPtr };
            DebugPlayerController = new PlayerController() { AddressReadFunc = () => DebugPlayerControllerPtr };
        }

        public Player GetChrAsPlayer() => new Player() { AddressReadFunc = () => ChrPtr };
        public Enemy GetChrAsEnemy() => new Enemy() { AddressReadFunc = () => ChrPtr };

        public bool EnableLogic
        {
            get { return RBit(Address + 0xC0, 7); }
            set { WBit(Address + 0xC0, 7, value); }
        }

        public bool DisableMapHit
        {
            get { return RBit(Address + 0xC4, 27); }
            set { WBit(Address + 0xC4, 27, value); }
        }

        //0x40 flag applied to Address + 0xC6
        //public bool DisableCollision
        //{
        //    get { return GetMapFlagB(ChrMapFlagsB.DisableCollision); }
        //    set { SetMapFlagB(ChrMapFlagsB.DisableCollision, value); }
        //}

        public int ChrPtr
        {
            get { return RInt32(Address + 0x10); }
            set { WInt32(Address + 0x10, value); }
        }

        public int ControllerPtr
        {
            get { return RInt32(Address + 0x54); }
            set { WInt32(Address + 0x54, value); }
        }

        public int AnimationPtr
        {
            get { return RInt32(Address + 0x14); }
            set { WInt32(Address + 0x14, value); }
        }

        public List<ChrAnimInstance> GetAnimInstances()
        {
            if (AnimationPtr < Hook.DARKSOULS.SafeBaseMemoryOffset)
                return new List<ChrAnimInstance>();

            int animStructThing = RInt32(AnimationPtr + 0xC);
            int startAddr = RInt32(animStructThing + 0x10);
            int entryCount = RInt32(animStructThing + 0x14);

            var result = new List<ChrAnimInstance>();

            for (int i = 0; i < entryCount; i++)
            {
                int entryAddr = RInt32(startAddr + (i * 4));
                result.Add(new ChrAnimInstance() { AddressReadFunc = () => entryAddr });
            }

            return result;
        }

        public float AnimationSpeed
        {
            get { return RFloat(AnimationPtr + 0x64); }
            set { WFloat(AnimationPtr + 0x64, value); }
        }

        public bool AnimDbgDrawSkeleton
        {
            get { return RBool(AnimationPtr + 0x68); }
            set { WBool(AnimationPtr + 0x68, value); }
        }

        public bool AnimDbgDrawBoneName
        {
            get { return RBool(AnimationPtr + 0x69); }
            set { WBool(AnimationPtr + 0x69, value); }
        }

        public bool AnimDbgDrawExtractMotion
        {
            get { return RBool(AnimationPtr + 0x81); }
            set { WBool(AnimationPtr + 0x81, value); }
        }

        public bool AnimDbgSlotLog
        {
            get { return RBool(AnimationPtr + 0x82); }
            set { WBool(AnimationPtr + 0x82, value); }
        }

        public int TransformPtr
        {
            get { return RInt32(Address + 0x1c); }
            set { WInt32(Address + 0x1c, value); }
        }

        public bool WarpActivate
        {
            get { return RBool(Address + 0xC8); }
            set { WBool(Address + 0xC8, value); }
        }

        public float WarpX
        {
            get { return RFloat(Address + 0xD0); }
            set { WFloat(Address + 0xD0, value); }
        }

        public float WarpY
        {
            get { return RFloat(Address + 0xD4); }
            set { WFloat(Address + 0xD4, value); }
        }

        public float WarpZ
        {
            get { return RFloat(Address + 0xD8); }
            set { WFloat(Address + 0xD8, value); }
        }

        //TODO: Confirm warp x and z rotation exist (I just guessed based on the pattern: [ pos x, pos y, pos y, {???}, rot y, {???} ]

        public float WarpRX
        {
            get { return RFloat(Address + 0xE0); }
            set { WFloat(Address + 0xE0, value); }
        }

        public float WarpRY
        {
            get { return RFloat(Address + 0xE4); }
            set { WFloat(Address + 0xE4, value); }
        }

        public float WarpRZ
        {
            get { return RFloat(Address + 0xE8); }
            set { WFloat(Address + 0xE8, value); }
        }

        public float WarpHeading
        {
            get { return (float)((RFloat(Address + 0xE4) / Math.PI * 180) + 180); }
            set { WFloat(Address + 0xE4, (float)(value * Math.PI / 180) - (float)Math.PI); }
        }

        public int DebugPlayerControllerPtr {
			get { return Hook.RInt32(Address + 0x244); }
			set { Hook.WInt32(Address + 0x244, value); }
		}
    }
}
