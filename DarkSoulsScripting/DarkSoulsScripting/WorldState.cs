using System;
using DarkSoulsScripting.Injection;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public static class WorldState
    {
		public static int Address => RInt32(0x13784A0);

        //TODO: ADD MORE STUFF

        public static byte WarpNextStageKick
        {
            get => RByte(Address + 0x11);
            set => WByte(Address + 0x11, value);
        }

        public static byte SetMapUid_Area
        {
            get => RByte(Address + 0x16);
            set => WByte(Address + 0x16, value);
        }

        public static byte SetMapUid_World
        {
            get => RByte(Address + 0x17);
            set => WByte(Address + 0x17, value);
        }

        public static int SetMapUid_Point
        {
            get => RInt32(Address + 0x18);
            set => WInt32(Address + 0x18, value);
        }

        public static int SetMiniBlockIndex
        {
            get => RInt32(Address + 0x24);
            set => WInt32(Address + 0x24, value);
        }

        public static byte SetSummonedPos
        {
            get => RByte(Address + 0x18);
            set => WByte(Address + 0x18, value);
        }

        public static int SaveSlot
        {
            get => RInt32(Address + 0xA70);
            set => WInt32(Address + 0xA70, value);
        }

        public static float SosSignPosX
        {
            get => RFloat(Address + 0xA80);
            set => WFloat(Address + 0xA80, value);
        }

        public static float SosSignPosY
        {
            get => RFloat(Address + 0xA84);
            set => WFloat(Address + 0xA84, value);
        }

        public static float SosSignPosZ
        {
            get => RFloat(Address + 0xA88);
            set => WFloat(Address + 0xA88, value);
        }

        public static byte SosSignAreaID
        {
            get => RByte(Address + 0xA92);
            set => WByte(Address + 0xA92, value);
        }

        public static byte SosSignWorldID
        {
            get => RByte(Address + 0xA93);
            set => WByte(Address + 0xA93, value);
        }

        public static byte SosSignWarp
        {
            get => RByte(Address + 0xB00);
            set => WByte(Address + 0xB00, value);
        }

        public static int BonfireID
        {
            get => RInt32(Address + 0xB04);
            set => WInt32(Address + 0xB04, value);
        }

        public static bool TutorialBegin
        {
            get => RBool(Address + 0xB0C);
            set => WBool(Address + 0xB0C, value);
        }

        public static byte TutorialSummonedPos
        {
            get => RByte(Address + 0xB0E);
            set => WByte(Address + 0xB0E, value);
        }

        public static bool TriggerSaveA
        {
            get => RBool(Address + 0xB0F);
            set => WBool(Address + 0xB0F, value);
        }

        public static bool TriggerSaveB
        {
            get => RBool(Address + 0xB10);
            set => WBool(Address + 0xB10, value);
        }

        public static bool TriggerSaveC
        {
            get => RBool(Address + 0xB11);
            set => WBool(Address + 0xB11, value);
        }

        public static void TriggerSave()
        {
            TriggerSaveA = true;
        }

        public static bool bRequestToEnding
        {
            get => RBool(Address + 0xB18);
            set => WBool(Address + 0xB18, value);
        }

        public static bool Autosave
        {
            get => RBool(Address + 0xB40);
            set => WBool(Address + 0xB40, value);
        }

        public static bool IsFpsDisconnection
        {
            get => RBool(Address + 0xB4C);
            set => WBool(Address + 0xB4C, value);
        }

        public static bool IsOnlineMode
        {
            get => RBool(Address + 0xB4D);
            set => WBool(Address + 0xB4D, value);
        }

        public static bool IsTitleStart
        {
            get => RBool(Address + 0xB4E);
            set => WBool(Address + 0xB4E, value);
        }

        //public static bool Unknown_OfflineState_QuestionMark
        //{
        //    get => RBool(Address + 0xB60);
        //    set => WBool(Address + 0xB60, value);
        //}

        public static int ClearMyWorldState_1
        {
            get => RInt32(Address + 0xBCC);
            set => WInt32(Address + 0xBCC, value);
        }

        public static int ClearMyWorldState_2
        {
            get => RInt32(Address + 0xBD0);
            set => WInt32(Address + 0xBD0, value);
        }

        public static int ClearMyWorldState_3
        {
            get => RInt32(Address + 0xBDC);
            set => WInt32(Address + 0xBDC, value);
        }

        public static int ClearMyWorldState_4
        {
            get => RInt32(Address + 0xBE0);
            set => WInt32(Address + 0xBE0, value);
        }

        public static int InvasionType
        {
            get => RInt32(Address + 0xBE4);
            set => WInt32(Address + 0xBE4, value);
        }

        /*
            +0x0BEC Pointer              Unknown
                +0x0004 GetWhiteGhostCount
                +0x0014 HavePartyMember
                +0x007C IsGameClient
        */

        public static bool IsDisableAllAreaEne
        {
            get => RBool(Address + 0xC2C);
            set => WBool(Address + 0xC2C, value);
        }

        public static bool IsDisableAllAreaEvent
        {
            get => RBool(Address + 0xC2D);
            set => WBool(Address + 0xC2D, value);
        }

        public static bool IsDisableAllAreaMap
        {
            get => RBool(Address + 0xC2E);
            set => WBool(Address + 0xC2E, value);
        }

        public static bool IsDisableAllAreaObj
        {
            get => RBool(Address + 0xC2F);
            set => WBool(Address + 0xC2F, value);
        }

        public static bool IsEnableAllAreaObj
        {
            get => RBool(Address + 0xC30);
            set => WBool(Address + 0xC30, value);
        }

        public static bool IsEnableAllAreaObjBreak
        {
            get => RBool(Address + 0xC31);
            set => WBool(Address + 0xC31, value);
        }

        public static bool IsDisableAllAreaHiHit
        {
            get => RBool(Address + 0xC32);
            set => WBool(Address + 0xC32, value);
        }

        public static bool IsEnableAllAreaLoHit
        {
            get => RBool(Address + 0xC33);
            set => WBool(Address + 0xC33, value);
        }

        public static bool IsDisableAllAreaSFX
        {
            get => RBool(Address + 0xC34);
            set => WBool(Address + 0xC34, value);
        }

        public static bool IsDisableAllAreaSound
        {
            get => RBool(Address + 0xC35);
            set => WBool(Address + 0xC35, value);
        }

        public static bool IsObjBreakRecordMode
        {
            get => RBool(Address + 0xC36);
            set => WBool(Address + 0xC36, value);
        }

        public static bool IsAutoMapWarpMode
        {
            get => RBool(Address + 0xC37);
            set => WBool(Address + 0xC37, value);
        }

        public static bool IsChrNpcWanderTest
        {
            get => RBool(Address + 0xC38);
            set => WBool(Address + 0xC38, value);
        }

        public static bool IsDbgChrAllDead
        {
            get => RBool(Address + 0xC39);
            set => WBool(Address + 0xC39, value);
        }

        public static float LastStandPosX
        {
            get => RFloat(Address + 0xB70);
            set => WFloat(Address + 0xB70, value);
        }

        public static float LastStandPosY
        {
            get => RFloat(Address + 0xB74);
            set => WFloat(Address + 0xB74, value);
        }

        public static float LastStandPosZ
        {
            get => RFloat(Address + 0xB78);
            set => WFloat(Address + 0xB78, value);
        }

        public static float LastStandAngle
        {
            get => RFloat(Address + 0xB7C);
            set => WFloat(Address + 0xB7C, value);
        }
    }
}
