using System;
using DarkSoulsScripting.Injection;
using System.ComponentModel;
using DarkSoulsScripting.AI_DEFINE;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
	public abstract class Chr<TChrMovementCtrl, TChrController> : GameStruct
        where TChrController : ChrController, new()
        where TChrMovementCtrl : ChrMovementCtrl<TChrController>, new()
	{
        public const int MAX_NAME_LENGTH = 10;

        public ChrSlot Slot { get; private set; } = null;
        public TChrMovementCtrl MovementCtrl { get; private set; } = null;
        public ChrUNK1 UNK1 { get; private set; } = null;

        protected override void InitSubStructures()
        {
            Slot = new ChrSlot() { AddressReadFunc = () => SlotPtr };
            MovementCtrl = new TChrMovementCtrl() { AddressReadFunc = () => MovementCtrlPtr };
            UNK1 = new ChrUNK1() { AddressReadFunc = () => UNK1Ptr };
        }

        public int SlotPtr {
			get { return RInt32(Address + 0xC); }
			set { WInt32(Address + 0xC, value); }
		}

        public int UNK1Ptr {
			get { return RInt32(Address + 0x10); }
			set { WInt32(Address + 0x10, value); }
		}

		public bool DisableEventBackreadState {
			get { return RBool(Address + 0x14); }
			set { WBool(Address + 0x14, value); }
		}

		

		public string ModelName {
			get { return RUnicodeStr(Address + 0x38, 10); }
			set { WUnicodeStr(Address + 0x38, value.Substring(0, Math.Min(value.Length, 10))); }
		}

        public int UnknownMSBStructPointer {
			get { return RInt32(Address + 0x54); }
			set { WInt32(Address + 0x54, value); }
		}

        public int UnknownMSBStructIndex {
			get { return RInt32(Address + 0x58); }
			set { WInt32(Address + 0x58, value); }
		}

		public int NPCID {
			get { return RInt32(Address + 0x68); }
			set { WInt32(Address + 0x68, value); }
		}

		public int NPCID2 {
			get { return RInt32(Address + 0x6c); }
			set { WInt32(Address + 0x6c, value); }
		}

		public CHR_TYPE ChrType {
			get { return (CHR_TYPE)RInt32(Address + 0x70); }
			set { WInt32(Address + 0x70, (int)value); }
		}

		public TEAM_TYPE TeamType {
			get { return (TEAM_TYPE)RInt32(Address + 0x74); }
			set { WInt32(Address + 0x74, (int)value); }
		}

        // CRASHES GAME
        public int OmissionLevel
        {
            get { return RInt32(Address + 0xF4); }
            set { WInt32(Address + 0xF4, value); }
        }

        public int ForcePlayAnimation1
        {
            get { return RInt32(Address + 0xFC); }
            set { WInt32(Address + 0xFC, value); }
        }

        public int ForcePlayAnimation2
        {
            get { return RInt32(Address + 0x100); }
            set { WInt32(Address + 0x100, value); }
        }

        public int ForcePlayAnimation3
        {
            get { return RInt32(Address + 0x104); }
            set { WInt32(Address + 0x104, value); }
        }

        public int ForcePlayAnimation4
        {
            get { return RInt32(Address + 0x108); }
            set { WInt32(Address + 0x108, value); }
        }

        public bool IsTargetLocked {
			get { return RBool(Address + 0x128); }
			set { WBool(Address + 0x128, value); }
		}

		public int DeathStructPointer {
			get { return RInt32(Address + 0x170); }
		}

		public bool IsDead {
			get { return RBool(DeathStructPointer + 0x18); }
		}
		//Writing value crashed game...
		//Set(value As Boolean)
		//    WBool(DeathStructPointer + &H18, value)
		//End Set

		public float PoiseCurrent {
			get { return RFloat(Address + 0x1c0); }
			set { WFloat(Address + 0x1c0, value); }
		}

		public float PoiseMax {
			get { return RFloat(Address + 0x1c4); }
			set { WFloat(Address + 0x1c4, value); }
		}

		public float PoiseRecoverTimer {
			get { return RFloat(Address + 0x1cc); }
			set { WFloat(Address + 0x1cc, value); }
		}

		public int ID {
			get { return RInt32(Address + 0x208); }
			set { WInt32(Address + 0x208, value); }
		}

		public float Opacity {
			get { return RFloat(Address + 0x258); }
			set { WFloat(Address + 0x258, value); }
		}

		public int DrawGroup1 {
			get { return RInt32(Address + 0x264); }
			set { WInt32(Address + 0x264, value); }
		}

		public int DrawGroup2 {
			get { return RInt32(Address + 0x268); }
			set { WInt32(Address + 0x268, value); }
		}

		public int DrawGroup3 {
			get { return RInt32(Address + 0x26c); }
			set { WInt32(Address + 0x26c, value); }
		}

		public int DrawGroup4 {
			get { return RInt32(Address + 0x270); }
			set { WInt32(Address + 0x270, value); }
		}

		public int DispGroup1 {
			get { return RInt32(Address + 0x274); }
			set { WInt32(Address + 0x274, value); }
		}

		public int DispGroup2 {
			get { return RInt32(Address + 0x278); }
			set { WInt32(Address + 0x278, value); }
		}

		public int DispGroup3 {
			get { return RInt32(Address + 0x27c); }
			set { WInt32(Address + 0x27c, value); }
		}

		public int DispGroup4 {
			get { return RInt32(Address + 0x280); }
			set { WInt32(Address + 0x280, value); }
		}

		public int MultiplayerZone {
			get { return RInt32(Address + 0x284); }
			set { WInt32(Address + 0x284, value); }
		}

		public short Material_Floor {
			get { return RInt16(Address + 0x288); }
			set { WInt16(Address + 0x288, value); }
		}

		public short Material_ArmorSE {
			get { return RInt16(Address + 0x28a); }
			set { WInt16(Address + 0x28a, value); }
		}

		public short Material_ArmorSFX {
			get { return RInt16(Address + 0x28c); }
			set { WInt16(Address + 0x28c, value); }
		}

		public int HP {
			get { return RInt32(Address + 0x2d4); }
			set { WInt32(Address + 0x2d4, value); }
		}

		public int MaxHP {
			get { return RInt32(Address + 0x2d8); }
			set { WInt32(Address + 0x2d8, value); }
		}

		public int Stamina {
			get { return RInt32(Address + 0x2e4); }
			set { WInt32(Address + 0x2e4, value); }
		}

		public int MaxStamina {
			get { return RInt32(Address + 0x2e8); }
			set { WInt32(Address + 0x2e8, value); }
		}

		public int ResistancePoisonCurrent {
			get { return RInt32(Address + 0x300); }
			set { WInt32(Address + 0x300, value); }
		}

		public int ResistanceToxicCurrent {
			get { return RInt32(Address + 0x304); }
			set { WInt32(Address + 0x304, value); }
		}

		public int ResistanceBleedCurrent {
			get { return RInt32(Address + 0x308); }
			set { WInt32(Address + 0x308, value); }
		}

		public int ResistanceCurseCurrent {
			get { return RInt32(Address + 0x30c); }
			set { WInt32(Address + 0x30c, value); }
		}

		public int ResistancePoisonMax {
			get { return RInt32(Address + 0x310); }
			set { WInt32(Address + 0x310, value); }
		}

		public int ResistanceToxicMax {
			get { return RInt32(Address + 0x314); }
			set { WInt32(Address + 0x314, value); }
		}

		public int ResistanceBleedMax {
			get { return RInt32(Address + 0x318); }
			set { WInt32(Address + 0x318, value); }
		}

		public int ResistanceCurseMax {
			get { return RInt32(Address + 0x31c); }
			set { WInt32(Address + 0x31c, value); }
		}

		public int Unknown1Ptr {
			get { return RInt32(Address + 0x330); }
			set { WInt32(Address + 0x330, value); }
		}

        public int MovementCtrlPtr
        {
            get { return RInt32(Address + 0x28); }
            set { WInt32(Address + 0x28, value); }
        }

        public int StatsPtr
        {
            get { return RInt32(Address + 0x414); }
            set { WInt32(Address + 0x414, value); }
        }

		#region "DebugFlags"
		public bool NoGoodsConsume {
            get { return RBit(Address + 0x3C4, 7); }
            set { WBit(Address + 0x3C4, 7, value); }
        }

		public bool DisableDrawingA {
            get { return RBit(Address + 0x3C4, 11); }
            set { WBit(Address + 0x3C4, 11, value); }
        }

		public bool DrawCounter {
            get { return RBit(Address + 0x3C4, 10); }
            set { WBit(Address + 0x3C4, 10, value); }
        }

		public bool DisableDrawingB {
            get { return RBit(Address + 0x3C4, 12); }
            set { WBit(Address + 0x3C4, 12, value); }
        }

		public bool DrawDirection {
            get { return RBit(Address + 0x3C4, 17); }
            set { WBit(Address + 0x3C4, 17, value); }
        }

		public bool NoUpdate {
            get { return RBit(Address + 0x3C4, 16); }
            set { WBit(Address + 0x3C4, 16, value); }
        }

		public bool NoAttack {
            get { return RBit(Address + 0x3C4, 23); }
            set { WBit(Address + 0x3C4, 23, value); }
        }

		public bool NoMove {
            get { return RBit(Address + 0x3C4, 22); }
            set { WBit(Address + 0x3C4, 22, value); }
        }

		public bool NoStamConsume {
            get { return RBit(Address + 0x3C4, 21); }
            set { WBit(Address + 0x3C4, 21, value); }
        }

		public bool NoMPConsume {
            get { return RBit(Address + 0x3C4, 20); }
            set { WBit(Address + 0x3C4, 20, value); }
        }

		public bool NoDead {
            get { return RBit(Address + 0x3C4, 26); }
            set { WBit(Address + 0x3C4, 26, value); }
        }

		public bool NoDamage {
            get { return RBit(Address + 0x3C4, 25); }
            set { WBit(Address + 0x3C4, 25, value); }
        }

        public bool NoHit
        {
            get { return RBit(Address + 0x3C4, 24); }
            set { WBit(Address + 0x3C4, 24, value); }
        }

        public bool DrawHit {
            get { return RBit(Address + 0x3C4, 29); }
            set { WBit(Address + 0x3C4, 29, value); }
        }
		#endregion

		#region "Flags"
		public bool DisableDamage {
			get { return RBit(Address + 0x1FC, 5); }
			set { WBit(Address + 0x1FC, 5, value); }
		}

		public bool EnableInvincible {
            get { return RBit(Address + 0x1FC, 4); }
            set { WBit(Address + 0x1FC, 4, value); }
        }

		public bool FirstPersonView {
            get { return RBit(Address + 0x1FC, 11); }
            set { WBit(Address + 0x1FC, 11, value); }
        }

		public bool DeadMode {
            get { return RBit(Address + 0x1FC, 6); }
            set { WBit(Address + 0x1FC, 6, value); }
        }

		public bool DisableGravity {
            get { return RBit(Address + 0x1FC, 17); }
            set { WBit(Address + 0x1FC, 17, value); }
        }

		public bool SetDrawEnable {
            get { return RBit(Address + 0x1FC, 8); }
            set { WBit(Address + 0x1FC, 8, value); }
        }

        //TODO: CHECK WTF THIS EVEN IS. BLAME WULF
        public bool DisableGravity2
        {
            get { return RBit(Address + 0x1FC, 9); }
            set { WBit(Address + 0x1FC, 9, value); }
        }

        //CRASHES GAME
        public bool OmissionLock
        {
            get { return RBit(Address + 0x1FC, 15); }
            set { WBit(Address + 0x1FC, 15, value); }
        }

        public bool ForceUpdateNextFrame
        {
            get { return RBit(Address + 0x1FC, 14); }
            set { WBit(Address + 0x1FC, 14, value); }
        }

        public bool ForcePlayAnimationStayCancel
        {
            get { return RBit(Address + 0x1FC, 21); }
            set { WBit(Address + 0x1FC, 21, value); }
        }

        public bool Generate
        {
            get { return RBit(Address + 0x1FC, 27); }
            set { WBit(Address + 0x1FC, 27, value); }
        }

        public bool DisableHpGauge
        {
            get { return RBit(Address + 0x1FC, 28); }
            set { WBit(Address + 0x1FC, 28, value); }
        }
        #endregion

        public void View()
		{
            //TODO: MAP THIS IN ITS RESPECTIVE STATIC CLASS:
            WInt32(RInt32(0x137D648) + 0xEC, Address);
        }

        public void WarpToCoords(float x, float y, float z, float heading)
        {
            MovementCtrl.WarpX = x;
            MovementCtrl.WarpY = y;
            MovementCtrl.WarpZ = z;
            MovementCtrl.WarpHeading = heading;
            MovementCtrl.WarpActivate = true;
        }

        public void WarpToTransform(ChrTransform dest)
        {
            WarpToCoords(dest.X, dest.Y, dest.Z, dest.Heading);
        }

		public void WarpToPlayer(Player dest)
		{
            WarpToTransform(dest.MovementCtrl.Transform);
		}

        public void WarpToEnemy(Enemy dest)
		{
            WarpToTransform(dest.MovementCtrl.Transform);
		}

        public void SwitchControlPlayer()
        {
            MovementCtrl.DebugPlayerControllerPtr = WorldChrMan.LocalPlayer.MovementCtrl.ControllerPtr;
            IngameFuncs.EnableLogic(10000, false);
            View();
        }

        public void ReturnControlPlayer()
        {
            MovementCtrl.DebugPlayerControllerPtr = 0;
            IngameFuncs.EnableLogic(10000, true);
            WorldChrMan.LocalPlayer.View();
        }

        public string GetName()
        {
            return RAsciiStr(RInt32(RInt32(RInt32(UnknownMSBStructPointer + 0x28) + 0x10 + 4 * UnknownMSBStructIndex)), MAX_NAME_LENGTH);
        }
    }
}
