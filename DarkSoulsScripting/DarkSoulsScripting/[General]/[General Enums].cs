using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsScripting
{
    public enum WeaponHoldStyle
    {
        None,
        OneHand,
        TwoHandLeft,
        TwoHandRight
    }

    public enum ITEM_CATE : int
    {
        Weapons = 0x00000000,
        Armor = 0x10000000,
        Rings = 0x20000000,
        Goods = 0x40000000,
    }

    //Note: different than AI_DEFINE.TEAM_TYPE's values
    public enum CHR_TYPE : int
    {
        Live = 0,
        WhiteGhost = 1,
        BlackGhost = 2,
        GlayGhost = 3,
        WanderGhost = 4,

        //TODO: ADD MORE CHR_TYPE

        Intruder = 12,
        ColiseumGhost = 13,

        //TODO: ADD MORE CHR_TYPE
    }

    public enum COVENANT
    {
        WayOfWhite = 1,
        PrincessGuard = 2,
        WarriorOfSunlight = 3,
        Darkwraith = 4,
        PathOfTheDragon = 5,
        GravelordServant = 6,
        ForestHunter = 7,
        DarkmoonBlade = 8,
        ChaosServant = 9
    }

    //public enum TEAM_TYPE
    //{
    //    None = 0,
    //    Live = 1,
    //    WhiteGhost = 2,
    //    BlackGhost = 3,
    //    GlayGhost = 4,
    //    WanderGhost = 5,
    //    Enemy = 6,
    //    Boss = 7,
    //    Friend = 8,
    //    AngryFriend = 9,
    //    Decoy = 10,
    //    DecoyLike = 11,
    //    BattleFriend = 12,
    //    Intruder = 13,
    //    Neutral = 14,
    //    Charm = 15,
    //}



    public enum GOAL_COMMON
    {
        Wait = 2000,
        Turn = 2001,
        TurnAround = 2002,
        Step = 2014,
        ApproachTarget = 2015,
        LeaveTarget = 2016,
        SidewayMove = 2017,
        KeepDist = 2018,
        MoveToSomewhere = 2019,
        SpinStep = 2020,
        Fall = 2021,
        Attack = 2100,
        Guard = 2101,
        ComboAttack = 2102,
        GuardBreakAttack = 2103,
        NonspinningAttack = 2104,
        ContinueAttack = 2105,
        Attack2 = 2106,
        ApproachStep = 2107,
        UseItem = 2108,
        DashAttack = 2109,
        DashAttack_Attack = 2110,
        Attack3 = 2112,
        Parry = 2113,
        SpecialTurn = 2114,
    }

    namespace AI_DEFINE
    {
        public enum GOAL_RESULT : int
        {
            Failed = -1,
            Continue = 0,
            Success = 1,
        }

        public enum AI_DIR_TYPE : int
        {
            CENTER = 0,
            F = 1,
            B = 2,
            L = 3,
            R = 4,
            ToF = 5,
            ToB = 6,
            ToL = 7,
            ToR = 8,
            Top = 9,
        }

        public enum DIST : int
        {
            Near = -1,
            Middle = -2,
            Far = -3,
            Out = -4,
            None = -5,
        }

        public enum POINT : int
        {
            INITIAL = 100,
            SNIPE = 101,
            EVENT = 102,
            MOVE_POINT = 103,
            NEAR_NAVIMESH = 104,
            FAR_NAVIGATE = 105,
            NEAR_NAVIGATE = 106,
            AI_FIXED_POS = 107,
            FAR_LANDING = 108,
            NEAR_LANDING = 109,
            //Underscore at beginning not part of actual name (C# limitation)
            _2ndNEAR_LANDING = 110,
            INIT_POSE = 111,
            HitObstacle = 112,
            LastRequestPosition = 113, //Commented out in DeS ai_define.lua (and not present in DaS ai_define.bin)
            CurrRequestPosition = 114,
            NearMovePoint = 115,
            NEAR_OBJ_ACT_POINT = 116,
            //Underscore at beginning not part of actual name (C# limitation)
            _2ndNEAR_OBJ_ACT_POINT = 117,
            LastSightPosition = 118,
            NearCorpsePosition = 119,
            AutoWalkAroundTest = 120,
        }

        public enum EVENT_TARGET : int
        {
            EVENT_TARGET_0 = 1000,
            EVENT_TARGET_1 = 1001,
            EVENT_TARGET_2 = 1002,
            EVENT_TARGET_3 = 1003,
            EVENT_TARGET_4 = 1004,
            EVENT_TARGET_5 = 1005,
            EVENT_TARGET_6 = 1006,
            EVENT_TARGET_7 = 1007,
            EVENT_TARGET_8 = 1008,
            EVENT_TARGET_9 = 1009,
            EVENT_TARGET_10 = 1010,
        }

        public enum INTERUPT : int
        {
            FindEnemy = 0,
            FindAttack = 1,
            Damaged = 2,
            Damaged_Stranger = 3,
            FindMissile = 4,
            SuccessGuard = 5,
            MissSwing = 6,
            GuardBegin = 7,
            GuardFinish = 8,
            GuardBreak = 9,
            Shoot = 10,
            ShootReady = 11,
            UseItem = 12,
            EnterBattleArea = 13,
            LeaveBattleArea = 14,
            CANNOT_MOVE = 15,
            Inside_ObserveArea = 16,
            ReboundByOpponentGuard = 17,
            ForgetTarget = 18,
            FriendRequestSupport = 19,
            TargetIsGuard = 20,
            HitEnemyWall = 21,
            SuccessParry = 22,
            CANNOT_MOVE_DisableInterupt = 23,
            //(DeS ends here)
            ParryTiming = 24,
            RideNode_LadderBottom = 25,
            FLAG_RideNode_Door = 26,
            StraightByPath = 27,
            ChangedAnimIdOffset = 28,
            SuccessThrow = 29,
            LookedTarget = 30,
            LoseSightTarget = 31,
            RideNode_InsideWall = 32,
            MissSwingSelf = 33,
            GuardBreakBlow = 34,
        }

        public enum PLATOON_STATE : int
        {
            None = 0,
            Caution = 1,
            Find = 2,
            ReplyHelp = 3,
            Battle = 4,
        }

        public enum COORDINATE_TYPE : int
        {
            None = -1,
            Attack = 0,
            SideWalk_L = 1,
            SideWalk_R = 2,
            AttackOrder = 3,
            Support = 4,
            KIMERAbite = 100,
            UROKOIwaSupport = 110,
        }

        public enum ORDER_TYPE : int
        {
            Role = 0,
            CallHelp = 1, //Not present in DeS at all for some reason
        }

        public enum ROLE_TYPE : int
        {
            None = -1,
            Attack = 0,
            Torimaki = 1,
            Kankyaku = 2,
        }

        public enum NPC_ATK : int
        {
            NormalR = 0,
            LargeR = 1,
            PushR = 2,
            NormalL = 3,
            GuardL = 4,
            Parry = 5,
            Magic = 6,
            //NPC_ATK_MagicL = NPC_ATK_Magic; (in DeS too)
            MagicL = 6,//TODO: Check if duplicate values are going to cause problems lol
            Item = 7,
            SwitchWep = 8,
            StepF = 9,
            StepB = 10,
            StepL = 11,
            StepR = 12,
            ChangeWep_R1 = 13,
            ChangeWep_R2 = 14,
            ChangeWep_L1 = 15,
            ChangeWep_L2 = 16,
            BackstepF = 17,
            BackstepB = 18,
            BackstepL = 19,
            BackstepR = 20,
            MagicR = 21,
            //(DeS ends here)
            Ladder_10 = 22,
            Ladder_11 = 23,
            Ladder_12 = 24,
            Ladder_13 = 25,
            Ladder_20 = 26,
            Ladder_21 = 27,
            Ladder_22 = 28,
            Ladder_23 = 29,
        }

        //Weird lowercase is [sic]
        public enum AI_EXCEL_THINK_PARAM_TYPE : int
        {
            NONE = 0,
            maxBackhomeDist = 1,
            backhomeDist = 2,
            backhomeBattleDist = 3,
            nonBattleActLife = 4,
            BattleStartDist = 5,
            bMoveOnHearSound = 6,
            CannotMoveAction = 7,
            battleGoalID = 8,
            BackHome_LookTargetTime = 9,
            BackHome_LookTargetDist = 10,
            BackHomeLife_OnHitEnemyWall = 11,
            //(DeS ends here)
            callHelp_IsCall = 12,
            callHelp_IsReply = 13,
            callHelp_MyPeerId = 14,
            callHelp_CallPeerId = 15,
            callHelp_DelayTime = 16,
            callHelp_CallActionId = 17,
            callHelp_ReplyBehaviorType = 18,
            callHelp_ForgetTimeByArrival = 19,
            callHelp_MinWaitTime = 20,
            callHelp_MaxWaitTime = 21,
            callHelp_ReplyActionId = 22,
            thinkAttr_doAdmirer = 23,
        }

        public enum POINT_MOVE_TYPE : int
        {
            Patrol = 0,
            RoundTrip = 1,
            Randum = 2,
            Gargoyle_Air_Patrol = 3,
            Gargoyle_Landing = 4,
        }

        //From DeS:
        //腕ID(ChrAsm::WEP_SET_POSに対応)
        public enum ARM : int
        {
            L = 0,
            R = 1,
        }

        //From DeS:
        //武器装備（ChrAsm::WEP_SLOT_OFFSETに対応）
        public enum WEP : int
        {
            Primary = 0,
            Secondary = 1,
        }

        //NOT PRESENT IN DARK SOULS
        //From DeS:
        // リプランニング時のゴールアクション。
        public enum NPC_THINK_GOAL_ACTION : int
        {
            NPC_THINK_GOAL_ACTION__NONE = 0, // /< 何もしない
            NPC_THINK_GOAL_ACTION__TURN_TO_TGT = 1, // /< ターゲットの方向に向く
            NPC_THINK_GOAL_ACTION__WALK_TO_TGT = 2, // /< ターゲットへ歩く
            NPC_THINK_GOAL_ACTION__RUN_TO_TGT = 3, // /< ターゲットへ走る
            NPC_THINK_GOAL_ACTION__SET_GOAL = 4, // /< 任意のゴールを設定
        }

        public enum PARTS_DMG : int
        {
            PARTS_DMG_NONE = 0,
            PARTS_DMG_LV1 = 1,
            PARTS_DMG_LV2 = 2,
            PARTS_DMG_LV3 = 3,
            PARTS_DMG_FINAL = 20,
        }

        public enum SP_EFFECT_TYPE : int
        {
            NONE = 0, // なし
            FIRE = 1, // 炎
            POIZON = 2, // 毒
            LEECH = 3, // ヒル
            SPOIL = 4, // 腐食
            ILLNESS = 5, // 疫病
            BLOOD = 6, // 出血
            CAMOUFLAGE = 7, // ゴーストカムフラージュ
            HIDDEN = 8, // 姿隠し
            MASOCHIST = 9, // マゾヒスト
            RECOVER_POZON = 10, // 状態異常回復【毒】
            RECOVER_ILLNESS = 11, // 状態異常回復【疫病】
            RECOVER_BLOOD = 12, // 状態異常回復【出血】
            RECOVER_ALL = 13, // 状態異常回復【万能】
            SOUL_STEAL = 14, // ソウルスティール
            ZOOM = 15, // ズーム
            WARP = 16, // ワープ
            DEMONS_SOUL = 17, // デモンズソウル
            BLACK_DISPERSE = 18, // 黒ゴースト退散
            TOUGH_GHOST = 19, // 強ゴースト
            WHITE_REQUEST = 20, // ホワイト希望
            BLACK_REQUEST = 21, // ブラック希望
            CHANGE_BLACK = 22, // 黒ゴースト化
            REVIVE = 23, // 蘇生
            FORBID_USEMAGIC = 24, // 魔法使用禁止
            MIRACLE_DIRAY = 25, // 奇跡モーション延長
            WHETSTONE = 26, // 砥石
            SUSPENDED_REVIVE = 27, // 仮死蘇生
            ENCHANT_WEAPON = 28, // 武器強化
            ENCHANT_ARMOR = 29, // 防御シールド
            WIRE_WRAPED = 30, // 糸まみれ
            GHOST_PARAM_CHANGE = 31, // 判定フラグ／ホワイト、ブラック、グレイゴースト時にシステムとして、強制的に装備させる特殊効果（生存になったら外れる）
            PARALYSIS = 32, // 金縛り中は、移動、攻撃、アイテム使用ができなくなる／効果時間を設定する.EzStateによる対応を行う.
            FLY_CROWD = 33, // 蠅たかり
            FIREMAN_STAGE_1 = 34, // 炎怪人_第１段階
            FIREMAN_STAGE_2 = 35, // 炎怪人_第２段階
            FIREMAN_STAGE_3 = 36, // 炎怪人_第３段階
            FIREMAN_STAGE_4 = 37, // 炎怪人_第４段階
            HALLUCINATION = 38, // 幻聴
            SOULCOIN = 39, // ソウルコイン
            TOUGH_SHIELD = 40, // 強防御シールド
            ANTIFIRE_SHIELD = 41, // 火耐性シールド
            HP_RECOVERY = 42, // HP回復状態
            FORCE_GHOST_STAGE1 = 43, // 強制ゴースト化　第1段階
            FORCE_GHOST_STAGE2 = 44, // 強制ゴースト化　第2段階
            FORCE_GHOST_STAGE3 = 45, // 強制ゴースト化　第3段階
            PHEROMONE = 46, // フェロモン
            CAT_LANDING = 47, // 猫着地
            PINCH_ATTACKPOWER_UP = 48, // ピンチ攻撃力アップ
            PINCH_DEFENSIBILITY_UP = 49, // ピンチ防御力アップ
            REGENERATE = 50, // リジェネレイト
            TORCH = 51, // たいまつ
            WEAK_REGENERATE = 52, // 弱リジェネレイト
            WEAK_CAMOUFLAGE = 53, // 弱ゴーストカムフラージュ
            WEAK_HIDDEN = 54, // 弱姿隠し
            HINT_BLOOD_SIGN = 55, // ヒント血文字
            LEECH_FOOT = 56, // ヒル足
            YELLOW_CLOAK = 57, // 黄衣
            POINT_LIGHT = 58, // 点光源
            BLOOD_SIGN_ESTIMATION = 59, // 血文字評価
            ENCHANT_WEAPON_REGULAR = 60, // 光の武器（魔法）
            ENCHANT_WEAPON_LARGE = 61, // 呪いの武器（魔法）
            ENCHANT_WEAPON_FIRE = 62, // 松脂（アイテム）
            ENCHANT_WEAPON_FIRE_LARGE = 63, // 黒松脂（アイテム）
            ENCHANT_WEAPON_MAGIC = 64, // デーモンの獣脂（アイテム）
            CHIMERA_POWER_UP = 65, // キメラ強化（牢城２キメラ用）
            ITEM_DROP_RATE = 66, // アイテムを装備すると、「敵を倒したときにアイテムになる確率」をアップさせます
        }

        public enum OBJ_ACT_TYPE : int
        {
            LEVER = 0,
            DOOR = 1,
        }

        public enum TEAM_TYPE : int
        {
            None = 0,
            Live = 1,
            WhiteGhost = 2,
            BlackGhost = 3,
            GlayGhost = 4,
            WanderGhost = 5,
            Enemy = 6,
            Boss = 7,
            Friend = 8,
            AngryFriend = 9,
            Decoy = 10,
            DecoyLike = 11,
            BattleFriend = 12,
            Intruder = 13,
            Neutral = 14,
            Charm = 15,
        }

        public enum GUARD_GOAL_DESIRE_RET : int
        {
            Success = 1,
            Continue = 2,
            Failed = 3,
        }

        public enum WEP_CATE : int
        {
            None = 0,
            Shield = 1,
            Bow = 2,
            Crossbow = 3,
            Wand = 4,
        }
    }
    
}
