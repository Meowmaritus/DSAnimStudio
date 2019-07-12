using DarkSoulsScripting.AI_DEFINE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public static partial class IngameFuncs
    {
        public static bool ActionEnd(int ChrID)
            => Call<bool>(FuncAddress.ActionEnd, ChrID);

        public static bool AddActionCount(int ChrID, int b)
            => Call<bool>(FuncAddress.AddActionCount, ChrID, b);

        public static int AddBlockClearBonus()
            => Call<int>(FuncAddress.AddBlockClearBonus);

        public static int AddClearCount()
            => Call<int>(FuncAddress.AddClearCount);

        public static int AddCorpseEvent(int a, int b)
            => Call<int>(FuncAddress.AddCorpseEvent, a, b);

        public static int AddCustomRoutePoint(int ChrID, int b)
            => Call<int>(FuncAddress.AddCustomRoutePoint, ChrID, b);

        public static int AddDeathCount()
            => Call<int>(FuncAddress.AddDeathCount);

        public static int AddEventGoal(int ChrID, GOAL_COMMON Goal, float Time, float d, float e, float f, float g, float h, float i, float j, float k)
            => Call<int>(FuncAddress.AddEventGoal, ChrID, (int)Goal, Time, d, e, f, g, h, i, j, k);

        public static bool AddEventSimpleTalk(int a, int b)
            => Call<bool>(FuncAddress.AddEventSimpleTalk, a, b);

        public static bool AddEventSimpleTalkTimer(int a, int b)
            => Call<bool>(FuncAddress.AddEventSimpleTalkTimer, a, b);

        public static int AddFieldInsFilter(int a)
            => Call<int>(FuncAddress.AddFieldInsFilter, a);

        public static int AddGeneEvent(int EventID, int b)
            => Call<int>(FuncAddress.AddGeneEvent, EventID, b);

        public static int AddHelpWhiteGhost()
            => Call<int>(FuncAddress.AddHelpWhiteGhost);

        public static byte AddHitMaskByBit(int ChrID, byte b)
            => Call<byte>(FuncAddress.AddHitMaskByBit, ChrID, b);

        public static byte AddInfomationBuffer(int a)
            => Call<byte>(FuncAddress.AddInfomationBuffer, a);

        public static byte AddInfomationBufferTag(int a, int b, int c)
            => Call<byte>(FuncAddress.AddInfomationBufferTag, a, b, c);

        //TODO: ENUMS
        public static byte AddInfomationList(int IconID, int CategoryID, int MessageID)
            => Call<byte>(FuncAddress.AddInfomationList, IconID, CategoryID, MessageID);

        public static int AddInfomationListItem(int a, int b, int c)
            => Call<int>(FuncAddress.AddInfomationListItem, a, b, c);

        //public static int AddInfomationTimeMsgTag(_ARGS_)
        //    => Call<int>(FuncAddress.AddInfomationTimeMsgTag, _ARGS_);

        public static int AddInfomationTosBuffer(int a)
            => Call<int>(FuncAddress.AddInfomationTosBuffer, a);

        //public static int AddInfomationTosBufferPlus(_ARGS_)
        //    => Call<int>(FuncAddress.AddInfomationTosBufferPlus, _ARGS_);

        public static bool AddInventoryItem(int ItemID, ITEM_CATE Category, int Quantity)
            => Call<bool>(FuncAddress.AddInventoryItem, ItemID, (int)Category, Quantity);

        public static int AddKillBlackGhost()
            => Call<int>(FuncAddress.AddKillBlackGhost);

        //Returns weird pointer.
        public static int AddQWC(int QwcID)
            => Call<int>(FuncAddress.AddQWC, QwcID);

        public static int AddRumble(int a, int b, int c, int d, int e)
            => Call<int>(FuncAddress.AddRumble, a, b, c, d, e);

        //public static int AddTreasureEvent(_ARGS_)
        //    => Call<int>(FuncAddress.AddTreasureEvent, _ARGS_);

        //Returns pointer to GameStats struct.
        public static int AddTrueDeathCount()
            => Call<int>(FuncAddress.AddTrueDeathCount);

        public static bool BeginAction(int ChrID, int b, int c, int d)
            => Call<bool>(FuncAddress.BeginAction, ChrID, b, c, d);

        public static int BeginLoopCheck(int ChrID)
            => Call<int>(FuncAddress.BeginLoopCheck, ChrID);

        public static int CalcExcuteMultiBonus(int a, float b, int c)
            => Call<int>(FuncAddress.CalcExcuteMultiBonus, a, b, c);

        //public static int CalcGetCurrentMapEntityId(_ARGS_)
        //    => Call<int>(FuncAddress.CalcGetCurrentMapEntityId, _ARGS_);

        //public static int CalcGetMultiWallEntityId(_ARGS_)
        //    => Call<int>(FuncAddress.CalcGetMultiWallEntityId, _ARGS_);

        public static bool CamReset(int ChrID, bool State)
            => Call<bool>(FuncAddress.CamReset, ChrID, State);

        public static int CastPointSpell(int ChrID, int ObjID, int BehaviourID, float AngleX, float AngleY, float AngleZ)
            => Call<int>(FuncAddress.CastPointSpell, ChrID, ObjID, BehaviourID, AngleX, AngleY, AngleZ);

        //public static int CastPointSpell_Horming(_ARGS_)
        //    => Call<int>(FuncAddress.CastPointSpell_Horming, _ARGS_);

        //public static int CastPointSpellPlus(_ARGS_)
        //    => Call<int>(FuncAddress.CastPointSpellPlus, _ARGS_);

        //public static int CastPointSpellPlus_Horming(_ARGS_)
        //    => Call<int>(FuncAddress.CastPointSpellPlus_Horming, _ARGS_);

        //TODO: Check if returns bool.
        public static byte CastTargetSpell(int ChrID, int b, int c, int d, int e)
            => Call<byte>(FuncAddress.CastTargetSpell, ChrID, b, c, d, e);

        public static byte CastTargetSpellPlus(int ChrID, int b, int c, int d, int e, int f)
            => Call<byte>(FuncAddress.CastTargetSpellPlus, ChrID, b, c, d, e, f);

        public static int ChangeGreyGhost()
            => Call<int>(FuncAddress.ChangeGreyGhost);

        public static int ChangeInitPosAng(int ChrID, int AreaID)
            => Call<int>(FuncAddress.ChangeInitPosAng, ChrID, AreaID);

        public static bool ChangeModel(int ObjID, int ModelID)
            => Call<bool>(FuncAddress.ChangeModel, ObjID, ModelID);

        public static int ChangeTarget(int ChrID1, int ChrID2)
            => Call<int>(FuncAddress.ChangeTarget, ChrID1, ChrID2);

        public static bool ChangeThink(int ChrID, int b)
            => Call<bool>(FuncAddress.ChangeThink, ChrID, b);

        public static bool ChangeWander(int a)
            => Call<bool>(FuncAddress.ChangeWander, a);

        public static bool CharacterAllAttachSys(int ChrID)
            => Call<bool>(FuncAddress.CharacterAllAttachSys, ChrID);

        //TODO: CHECK
        public static bool CharactorCopyPosAng(int ChrID1, int b, int ChrID2)
            => Call<bool>(FuncAddress.CharactorCopyPosAng, ChrID1, b, ChrID2);

        public static bool CheckChrHit_Obj(int a, int b)
            => Call<bool>(FuncAddress.CheckChrHit_Obj, a, b);

        public static bool CheckChrHit_Wall(int a, int b)
            => Call<bool>(FuncAddress.CheckChrHit_Wall, a, b);

        public static void CheckEventBody(int a)
            => Call<int>(FuncAddress.CheckEventBody, a);

        public static bool CheckEventChr_Proxy(int a, int b)
            => Call<bool>(FuncAddress.CheckEventChr_Proxy, a, b);

        public static int CheckPenalty()
            => Call<int>(FuncAddress.CheckPenalty);

        public static int ChrDisableUpdate(int ChrID, bool State)
            => Call<int>(FuncAddress.ChrDisableUpdate, ChrID, State);

        public static int ChrFadeIn(int ChrID, float Duration, float Opacity)
            => Call<int>(FuncAddress.ChrFadeIn, ChrID, Duration, Opacity);

        public static int ChrFadeOut(int ChrID, float Duration, float Opacity)
            => Call<int>(FuncAddress.ChrFadeOut, ChrID, Duration, Opacity);

        public static int ChrResetAnimation(int ChrID)
            => Call<int>(FuncAddress.ChrResetAnimation, ChrID);

        public static int ChrResetRequest(int ChrID)
            => Call<int>(FuncAddress.ChrResetRequest, ChrID);

        public static void ClearBossGauge()
            => Call<int>(FuncAddress.ClearBossGauge);

        //Returns pointer to WorldState struct.
        public static int ClearMyWorldState()
            => Call<int>(FuncAddress.ClearMyWorldState);

        public static int ClearSosSign()
            => Call<int>(FuncAddress.ClearSosSign);

        public static bool ClearTarget(int ChrID)
            => Call<bool>(FuncAddress.ClearTarget, ChrID);

        public static void CloseGenDialog()
            => Call<int>(FuncAddress.CloseGenDialog);

        public static int CloseMenu()
            => Call<int>(FuncAddress.CloseMenu);

        public static void CloseRankingDialog()
            => Call<int>(FuncAddress.CloseRankingDialog);

        //TODO: Check if the number is a talk ID and not a pointer, chr id, etc
        public static void CloseTalk(int TalkID)
            => Call<int>(FuncAddress.CloseTalk, TalkID);

        public static void CompleteEvent(int EventID)
            => Call<int>(FuncAddress.CompleteEvent, EventID);

        //public static int CreateCamSfx(_ARGS_)
        //    => Call<int>(FuncAddress.CreateCamSfx, _ARGS_);

        //public static int CreateDamage_NoCollision(_ARGS_)
        //    => Call<int>(FuncAddress.CreateDamage_NoCollision, _ARGS_);

        //public static int CreateEventBody(_ARGS_)
        //    => Call<int>(FuncAddress.CreateEventBody, _ARGS_);

        //public static int CreateEventBodyPlus(_ARGS_)
        //    => Call<int>(FuncAddress.CreateEventBodyPlus, _ARGS_);

        //public static int CreateHeroBloodStain(_ARGS_)
        //    => Call<int>(FuncAddress.CreateHeroBloodStain, _ARGS_);

        //public static int CreateSfx(_ARGS_)
        //    => Call<int>(FuncAddress.CreateSfx, _ARGS_);

        //public static int CreateSfx_DummyPoly(_ARGS_)
        //    => Call<int>(FuncAddress.CreateSfx_DummyPoly, _ARGS_);

        public static void CroseBriefingMsg()
            => Call<int>(FuncAddress.CroseBriefingMsg);

        public static int CustomLuaCall(int a, string b, bool OccurOnce)
            => Call<int>(FuncAddress.CustomLuaCall, a, b, OccurOnce);

        //public static int CustomLuaCallStart(_ARGS_)
        //    => Call<int>(FuncAddress.CustomLuaCallStart, _ARGS_);

        //public static int CustomLuaCallStartPlus(_ARGS_)
        //    => Call<int>(FuncAddress.CustomLuaCallStartPlus, _ARGS_);

        //public static int DeleteCamSfx(_ARGS_)
        //    => Call<int>(FuncAddress.DeleteCamSfx, _ARGS_);

        //public static int DeleteEvent(_ARGS_)
        //    => Call<int>(FuncAddress.DeleteEvent, _ARGS_);

        //public static int DeleteObjSfxAll(_ARGS_)
        //    => Call<int>(FuncAddress.DeleteObjSfxAll, _ARGS_);

        //public static int DeleteObjSfxDmyPlyID(_ARGS_)
        //    => Call<int>(FuncAddress.DeleteObjSfxDmyPlyID, _ARGS_);

        //public static int DeleteObjSfxEventID(_ARGS_)
        //    => Call<int>(FuncAddress.DeleteObjSfxEventID, _ARGS_);

        public static int DisableCollection(int ChrID, bool State)
            => Call<int>(FuncAddress.DisableCollection, ChrID, State);

        public static int DisableDamage(int ChrID, bool State)
            => Call<int>(FuncAddress.DisableDamage, ChrID, State);

        public static int DisableHpGauge(int ChrID, bool State)
            => Call<int>(FuncAddress.DisableHpGauge, ChrID, State);

        public static int DisableInterupt(int ChrID, bool State)
            => Call<int>(FuncAddress.DisableInterupt, ChrID, State);

        public static int DisableMapHit(int ChrID, bool State)
            => Call<int>(FuncAddress.DisableMapHit, ChrID, State);

        public static int DisableMove(int ChrID, bool State)
            => Call<int>(FuncAddress.DisableMove, ChrID, State);

        //public static int DivideRest(_ARGS_)
        //    => Call<int>(FuncAddress.DivideRest, _ARGS_);

        public static int EnableAction(int ChrID, bool State)
            => Call<int>(FuncAddress.EnableAction, ChrID, State);

        public static int EnableGeneratorSystem(int UnknownID, bool State)
            => Call<int>(FuncAddress.EnableGeneratorSystem, UnknownID, State);

        public static int EnableHide(int ChrID, bool State)
            => Call<int>(FuncAddress.EnableHide, ChrID, State);

        public static int EnableInvincible(int ChrID, bool State)
            => Call<int>(FuncAddress.EnableInvincible, ChrID, State);

        public static int EnableLogic(int ChrID, bool State)
            => Call<int>(FuncAddress.EnableLogic, ChrID, State);

        public static int EnableObjTreasure(int ObjID, bool State)
            => Call<int>(FuncAddress.EnableObjTreasure, ObjID, State);

        //TODO: DOUBLE CHECK
        public static int EndAnimation(int ChrID, int AnimID)
            => Call<int>(FuncAddress.EndAnimation, ChrID, AnimID);

        //public static int EraseEventSpecialEffect(_ARGS_)
        //    => Call<int>(FuncAddress.EraseEventSpecialEffect, _ARGS_);

        //public static int EraseEventSpecialEffect_2(_ARGS_)
        //    => Call<int>(FuncAddress.EraseEventSpecialEffect_2, _ARGS_);

        //public static int EventTagInsertString_forPlayerNo(_ARGS_)
        //    => Call<int>(FuncAddress.EventTagInsertString_forPlayerNo, _ARGS_);

        //public static int ExcutePenalty(_ARGS_)
        //    => Call<int>(FuncAddress.ExcutePenalty, _ARGS_);

        //public static int ForceChangeTarget(_ARGS_)
        //    => Call<int>(FuncAddress.ForceChangeTarget, _ARGS_);

        //public static int ForceDead(_ARGS_)
        //    => Call<int>(FuncAddress.ForceDead, _ARGS_);

        public static int ForcePlayAnimation(int ChrID, int AnimID)
            => Call<int>(FuncAddress.ForcePlayAnimation, ChrID, AnimID);

        public static int ForcePlayAnimationStayCancel(int ChrID, int AnimID)
            => Call<int>(FuncAddress.ForcePlayAnimationStayCancel, ChrID, AnimID);

        public static int ForcePlayLoopAnimation(int ChrID, int AnimID)
            => Call<int>(FuncAddress.ForcePlayLoopAnimation, ChrID, AnimID);

        public static int ForceSetOmissionLevel(int ChrID, bool State, int NumFrames)
            => Call<int>(FuncAddress.ForceSetOmissionLevel, ChrID, State, NumFrames);

        //public static int ForceUpdateNextFrame(_ARGS_)
        //    => Call<int>(FuncAddress.ForceUpdateNextFrame, _ARGS_);

        //public static int GetBountyRankPoint(_ARGS_)
        //    => Call<int>(FuncAddress.GetBountyRankPoint, _ARGS_);

        //public static int GetClearBonus(_ARGS_)
        //    => Call<int>(FuncAddress.GetClearBonus, _ARGS_);

        //public static int GetClearCount(_ARGS_)
        //    => Call<int>(FuncAddress.GetClearCount, _ARGS_);

        //public static int GetClearState(_ARGS_)
        //    => Call<int>(FuncAddress.GetClearState, _ARGS_);

        public static int GetCurrentMapAreaNo()
            => Call<int>(FuncAddress.GetCurrentMapAreaNo);

        public static int GetCurrentMapBlockNo()
            => Call<int>(FuncAddress.GetCurrentMapBlockNo);

        //public static int GetDeathState(_ARGS_)
        //    => Call<int>(FuncAddress.GetDeathState, _ARGS_);

        //public static int GetDistance(_ARGS_)
        //    => Call<int>(FuncAddress.GetDistance, _ARGS_);

        //public static int GetEnemyPlayerId_Random(_ARGS_)
        //    => Call<int>(FuncAddress.GetEnemyPlayerId_Random, _ARGS_);

        //public static int GetEventFlagValue(_ARGS_)
        //    => Call<int>(FuncAddress.GetEventFlagValue, _ARGS_);

        //public static int GetEventGoalState(_ARGS_)
        //    => Call<int>(FuncAddress.GetEventGoalState, _ARGS_);

        public static int GetEventMode(int ChrID)
            => Call<int>(FuncAddress.GetEventMode, ChrID);

        public static int GetEventRequest()
            => Call<int>(FuncAddress.GetEventRequest);

        public static int GetFloorMaterial(int ChrID)
            => Call<int>(FuncAddress.GetFloorMaterial, ChrID);

        //public static int GetGlobalQWC(_ARGS_)
        //    => Call<int>(FuncAddress.GetGlobalQWC, _ARGS_);

        //public static int GetHeroPoint(_ARGS_)
        //    => Call<int>(FuncAddress.GetHeroPoint, _ARGS_);

        public static int GetHostPlayerNo()
            => Call<int>(FuncAddress.GetHostPlayerNo);

        public static int GetHp(int ChrID)
            => Call<int>(FuncAddress.GetHp, ChrID);

        //public static int GetHpRate(_ARGS_)
        //    => Call<int>(FuncAddress.GetHpRate, _ARGS_);

        public static int GetItem(int a, int b)
            => Call<int>(FuncAddress.GetItem, a, b);

        //public static int GetLadderCount(_ARGS_)
        //    => Call<int>(FuncAddress.GetLadderCount, _ARGS_);

        public static int GetLastBlockId()
            => Call<int>(FuncAddress.GetLastBlockId);

        public static CHR_TYPE GetLocalPlayerChrType()
            => (CHR_TYPE)Call<int>(FuncAddress.GetLocalPlayerChrType);

        public static int GetLocalPlayerId()
            => Call<int>(FuncAddress.GetLocalPlayerId);

        //public static int GetLocalPlayerInvadeType(_ARGS_)
        //    => Call<int>(FuncAddress.GetLocalPlayerInvadeType, _ARGS_);

        //public static int GetLocalPlayerSoulLv(_ARGS_)
        //    => Call<int>(FuncAddress.GetLocalPlayerSoulLv, _ARGS_);

        //public static int GetLocalPlayerVowType(_ARGS_)
        //    => Call<int>(FuncAddress.GetLocalPlayerVowType, _ARGS_);

        //public static int GetLocalQWC(_ARGS_)
        //    => Call<int>(FuncAddress.GetLocalQWC, _ARGS_);

        //public static int GetMultiWallNum(_ARGS_)
        //    => Call<int>(FuncAddress.GetMultiWallNum, _ARGS_);

        public static int GetNetPlayerChrType(int NetworkPlayerID)
            => Call<int>(FuncAddress.GetNetPlayerChrType, NetworkPlayerID);

        //public static int GetObjHp(_ARGS_)
        //    => Call<int>(FuncAddress.GetObjHp, _ARGS_);

        //public static int GetParam(_ARGS_)
        //    => Call<int>(FuncAddress.GetParam, _ARGS_);

        //public static int GetParam1(_ARGS_)
        //    => Call<int>(FuncAddress.GetParam1, _ARGS_);

        //public static int GetParam2(_ARGS_)
        //    => Call<int>(FuncAddress.GetParam2, _ARGS_);

        //public static int GetParam3(_ARGS_)
        //    => Call<int>(FuncAddress.GetParam3, _ARGS_);

        //public static int GetPartyMemberNum_InvadeType(_ARGS_)
        //    => Call<int>(FuncAddress.GetPartyMemberNum_InvadeType, _ARGS_);

        //public static int GetPartyMemberNum_VowType(_ARGS_)
        //    => Call<int>(FuncAddress.GetPartyMemberNum_VowType, _ARGS_);

        //public static int GetPlayerId_Random(_ARGS_)
        //    => Call<int>(FuncAddress.GetPlayerId_Random, _ARGS_);

        //public static int GetPlayerNo_LotNitoMultiItem(_ARGS_)
        //    => Call<int>(FuncAddress.GetPlayerNo_LotNitoMultiItem, _ARGS_);

        //public static int GetPlayID(_ARGS_)
        //    => Call<int>(FuncAddress.GetPlayID, _ARGS_);

        //public static int GetQWC(_ARGS_)
        //    => Call<int>(FuncAddress.GetQWC, _ARGS_);

        public static int GetRandom(int Min, int Max)
            => Call<int>(FuncAddress.GetRandom, Min, Max);

        public static int GetRateItem(int ItemLotID)
            => Call<int>(FuncAddress.GetRateItem, ItemLotID);

        //public static int GetRateItem_IgnoreMultiPlay(_ARGS_)
        //    => Call<int>(FuncAddress.GetRateItem_IgnoreMultiPlay, _ARGS_);

        public static int GetReturnState()
            => Call<int>(FuncAddress.GetReturnState);

        public static int GetRightCurrentWeaponId()
            => Call<int>(FuncAddress.GetRightCurrentWeaponId);

        public static int GetSoloClearBonus(int a)
            => Call<int>(FuncAddress.GetSoloClearBonus, a);

        //TODO: DOUBLE CHECK
        public static int GetSummonAnimId(int ChrID)
            => Call<int>(FuncAddress.GetSummonAnimId, ChrID);

        //public static int GetSummonBlackResult(_ARGS_)
        //    => Call<int>(FuncAddress.GetSummonBlackResult, _ARGS_);

        //public static int GetTargetChrID(int ChrID)
        //    => Call<int>(FuncAddress.GetTargetChrID, ChrID);

        //public static int GetTempSummonParam(_ARGS_)
        //    => Call<int>(FuncAddress.GetTempSummonParam, _ARGS_);

        //public static int GetTravelItemParamId(_ARGS_)
        //    => Call<int>(FuncAddress.GetTravelItemParamId, _ARGS_);

        public static int GetWhiteGhostCount()
            => Call<int>(FuncAddress.GetWhiteGhostCount);

        //public static int HasSuppleItem(_ARGS_)
        //    => Call<int>(FuncAddress.HasSuppleItem, _ARGS_);

        //public static int HavePartyMember(_ARGS_)
        //    => Call<int>(FuncAddress.HavePartyMember, _ARGS_);

        //public static int HoverMoveVal(_ARGS_)
        //    => Call<int>(FuncAddress.HoverMoveVal, _ARGS_);

        //public static int HoverMoveValDmy(_ARGS_)
        //    => Call<int>(FuncAddress.HoverMoveValDmy, _ARGS_);

        //public static int IncrementCoopPlaySuccessCount(_ARGS_)
        //    => Call<int>(FuncAddress.IncrementCoopPlaySuccessCount, _ARGS_);

        //public static int IncrementThiefInvadePlaySuccessCount(_ARGS_)
        //    => Call<int>(FuncAddress.IncrementThiefInvadePlaySuccessCount, _ARGS_);

        public static int InfomationMenu(bool Simple, int TitleIconID, int TitleCatID, int TitleMsgID, int SysMsgID)
            => Call<int>(FuncAddress.InfomationMenu, Simple, TitleIconID, TitleCatID, TitleMsgID, SysMsgID);

        //public static int InitDeathState(_ARGS_)
        //    => Call<int>(FuncAddress.InitDeathState, _ARGS_);

        //public static int InvalidMyBloodMarkInfo(_ARGS_)
        //    => Call<int>(FuncAddress.InvalidMyBloodMarkInfo, _ARGS_);

        //public static int InvalidMyBloodMarkInfo_Tutorial(_ARGS_)
        //    => Call<int>(FuncAddress.InvalidMyBloodMarkInfo_Tutorial, _ARGS_);

        public static int InvalidPointLight(int LightID)
            => Call<int>(FuncAddress.InvalidPointLight, LightID);

        //public static int InvalidSfx(_ARGS_)
        //    => Call<int>(FuncAddress.InvalidSfx, _ARGS_);

        public static bool IsAction(int ChrID, int b)
            => Call<bool>(FuncAddress.IsAction, ChrID, b);

        //TODO: DOUBLE CHECK
        public static bool IsAlive(int ChrID)
            => Call<bool>(FuncAddress.IsAlive, ChrID);

        public static bool IsAliveMotion()
            => Call<bool>(FuncAddress.IsAliveMotion);

        public static bool IsAngle(int ChrID1, int ChrID2, float Angle)
            => Call<bool>(FuncAddress.IsAngle, ChrID1, ChrID2, Angle);

        public static bool IsAnglePlus(int ChrID1, int ChrID2, int Angle, int d)
            => Call<bool>(FuncAddress.IsAnglePlus, ChrID1, ChrID2, Angle, d);

        //public static bool IsAppearancePlayer(_ARGS_)
        //    => Call<bool>(FuncAddress.IsAppearancePlayer, _ARGS_);

        //public static bool IsBlackGhost(_ARGS_)
        //    => Call<bool>(FuncAddress.IsBlackGhost, _ARGS_);

        //public static bool IsBlackGhost_NetPlayer(_ARGS_)
        //    => Call<bool>(FuncAddress.IsBlackGhost_NetPlayer, _ARGS_);

        public static bool IsClearItem()
            => Call<bool>(FuncAddress.IsClearItem);

        public static bool IsClient()
            => Call<bool>(FuncAddress.IsClient);

        //public static bool IsColiseumGhost(_ARGS_)
        //    => Call<bool>(FuncAddress.IsColiseumGhost, _ARGS_);

        public static bool IsCompleteEvent(int EventID)
            => Call<bool>(FuncAddress.IsCompleteEvent, EventID);

        public static bool IsCompleteEventValue(int EventID)
            => Call<bool>(FuncAddress.IsCompleteEventValue, EventID);

        //public static bool IsDead_NextGreyGhost(_ARGS_)
        //    => Call<bool>(FuncAddress.IsDead_NextGreyGhost, _ARGS_);

        //public static bool IsDeathPenaltySkip(_ARGS_)
        //    => Call<bool>(FuncAddress.IsDeathPenaltySkip, _ARGS_);

        //public static bool IsDestroyed(_ARGS_)
        //    => Call<bool>(FuncAddress.IsDestroyed, _ARGS_);

        public static bool IsDisable(int ChrID)
            => Call<bool>(FuncAddress.IsDisable, ChrID);

        public static bool IsDistance(int ChrID1, int ChrID2, float Distance)
            => Call<bool>(FuncAddress.IsDistance, ChrID1, ChrID2, Distance);

        //public static bool IsDropCheck_Only(_ARGS_)
        //    => Call<bool>(FuncAddress.IsDropCheck_Only, _ARGS_);

        public static bool IsEquip(ITEM_CATE Category, int ItemID)
            => Call<bool>(FuncAddress.IsEquip, (int)Category, ItemID);

        public static bool IsEventAnim(int ChrID, int b)
            => Call<bool>(FuncAddress.IsEventAnim, ChrID, b);

        //public static bool IsFireDead(_ARGS_)
        //    => Call<bool>(FuncAddress.IsFireDead, _ARGS_);

        //public static bool IsForceSummoned(_ARGS_)
        //    => Call<bool>(FuncAddress.IsForceSummoned, _ARGS_);

        //public static bool IsGameClient(_ARGS_)
        //    => Call<bool>(FuncAddress.IsGameClient, _ARGS_);

        //public static bool IsGreyGhost(_ARGS_)
        //    => Call<bool>(FuncAddress.IsGreyGhost, _ARGS_);

        //public static bool IsGreyGhost_NetPlayer(_ARGS_)
        //    => Call<bool>(FuncAddress.IsGreyGhost_NetPlayer, _ARGS_);

        //public static bool IsHost(_ARGS_)
        //    => Call<bool>(FuncAddress.IsHost, _ARGS_);

        //public static bool IsInParty(_ARGS_)
        //    => Call<bool>(FuncAddress.IsInParty, _ARGS_);

        //public static bool IsInParty_EnemyMember(_ARGS_)
        //    => Call<bool>(FuncAddress.IsInParty_EnemyMember, _ARGS_);

        //public static bool IsInParty_FriendMember(_ARGS_)
        //    => Call<bool>(FuncAddress.IsInParty_FriendMember, _ARGS_);

        //public static bool IsIntruder(_ARGS_)
        //    => Call<bool>(FuncAddress.IsIntruder, _ARGS_);

        public static bool IsInventoryEquip(ITEM_CATE Category, int ItemID)
            => Call<bool>(FuncAddress.IsInventoryEquip, (int)Category, ItemID);

        //public static bool IsJobType(_ARGS_)
        //    => Call<bool>(FuncAddress.IsJobType, _ARGS_);

        public static bool IsLand(int ChrID)
            => Call<bool>(FuncAddress.IsLand, ChrID);

        //public static bool IsLiveNetPlayer(_ARGS_)
        //    => Call<bool>(FuncAddress.IsLiveNetPlayer, _ARGS_);

        public static bool IsLivePlayer()
            => Call<bool>(FuncAddress.IsLivePlayer);

        //public static bool IsLoadWait(_ARGS_)
        //    => Call<bool>(FuncAddress.IsLoadWait, _ARGS_);

        //public static bool IsMatchingMultiPlay(_ARGS_)
        //    => Call<bool>(FuncAddress.IsMatchingMultiPlay, _ARGS_);

        //public static bool IsOnline(_ARGS_)
        //    => Call<bool>(FuncAddress.IsOnline, _ARGS_);

        //public static bool IsOnlineMode(_ARGS_)
        //    => Call<bool>(FuncAddress.IsOnlineMode, _ARGS_);

        //public static bool IsPlayerAssessMenu_Tutorial(_ARGS_)
        //    => Call<bool>(FuncAddress.IsPlayerAssessMenu_Tutorial, _ARGS_);

        public static bool IsPlayerStay(int NetworkPlayerID)
            => Call<bool>(FuncAddress.IsPlayerStay, NetworkPlayerID);

        //public static bool IsPlayMovie(_ARGS_)
        //    => Call<bool>(FuncAddress.IsPlayMovie, _ARGS_);

        //public static bool IsPrevGreyGhost(_ARGS_)
        //    => Call<bool>(FuncAddress.IsPrevGreyGhost, _ARGS_);

        //public static bool IsProcessEventGoal(_ARGS_)
        //    => Call<bool>(FuncAddress.IsProcessEventGoal, _ARGS_);

        //public static bool IsReady_Obj(_ARGS_)
        //    => Call<bool>(FuncAddress.IsReady_Obj, _ARGS_);

        //TODO: CHECK
        //DeS example has *1 more arg*:
        //if proxy:IsRegionDrop( itemevent_eventidlist[setNo] , 10000,itemevent_typelist[setNo],itemevent_idlist[setNo],2284) == true then
        public static bool IsRegionDrop(int a, int b, int c, int Region)
            => Call<bool>(FuncAddress.IsRegionDrop, a, b, c, Region);

        public static bool IsRegionIn(int ChrID, int Region)
            => Call<bool>(FuncAddress.IsRegionIn, ChrID, Region);

        //public static bool IsRevengeRequested(_ARGS_)
        //    => Call<bool>(FuncAddress.IsRevengeRequested, _ARGS_);

        //public static bool IsReviveWait(_ARGS_)
        //    => Call<bool>(FuncAddress.IsReviveWait, _ARGS_);

        //public static bool IsShow_CampMenu(_ARGS_)
        //    => Call<bool>(FuncAddress.IsShow_CampMenu, _ARGS_);

        public static bool IsShowMenu()
            => Call<bool>(FuncAddress.IsShowMenu);

        //public static bool IsShowMenu_BriefingMsg(_ARGS_)
        //    => Call<bool>(FuncAddress.IsShowMenu_BriefingMsg, _ARGS_);

        //public static bool IsShowMenu_GenDialog(_ARGS_)
        //    => Call<bool>(FuncAddress.IsShowMenu_GenDialog, _ARGS_);

        //public static bool IsShowMenu_InfoMenu(_ARGS_)
        //    => Call<bool>(FuncAddress.IsShowMenu_InfoMenu, _ARGS_);

        //public static bool IsShowSosMsg_Tutorial(_ARGS_)
        //    => Call<bool>(FuncAddress.IsShowSosMsg_Tutorial, _ARGS_);

        //public static bool IsSuccessQWC(_ARGS_)
        //    => Call<bool>(FuncAddress.IsSuccessQWC, _ARGS_);

        //public static bool IsTryJoinSession(_ARGS_)
        //    => Call<bool>(FuncAddress.IsTryJoinSession, _ARGS_);

        //public static bool IsValidInstance(_ARGS_)
        //    => Call<bool>(FuncAddress.IsValidInstance, _ARGS_);

        //public static bool IsValidTalk(_ARGS_)
        //    => Call<bool>(FuncAddress.IsValidTalk, _ARGS_);

        //public static bool IsWhiteGhost(_ARGS_)
        //    => Call<bool>(FuncAddress.IsWhiteGhost, _ARGS_);

        //public static bool IsWhiteGhost_NetPlayer(_ARGS_)
        //    => Call<bool>(FuncAddress.IsWhiteGhost_NetPlayer, _ARGS_);

        public static void LeaveSession()
            => Call<int>(FuncAddress.LeaveSession);

        public static void LockSession()
            => Call<int>(FuncAddress.LockSession);

        //public static int LuaCall(_ARGS_)
        //    => Call<int>(FuncAddress.LuaCall, _ARGS_);

        //public static int LuaCallStart(_ARGS_)
        //    => Call<int>(FuncAddress.LuaCallStart, _ARGS_);

        //public static int LuaCallStartPlus(_ARGS_)
        //    => Call<int>(FuncAddress.LuaCallStartPlus, _ARGS_);

        //public static int MultiDoping_AllEventBody(_ARGS_)
        //    => Call<int>(FuncAddress.MultiDoping_AllEventBody, _ARGS_);

        public static int NoAnimeTurnCharactor(int ChrID1, int ChrID2, float Angle)
            => Call<int>(FuncAddress.NoAnimeTurnCharactor, ChrID1, ChrID2, Angle);

        public static int NotNetMessage_begin()
            => Call<int>(FuncAddress.NotNetMessage_begin);

        public static int NotNetMessage_end()
            => Call<int>(FuncAddress.NotNetMessage_end);

        //public static int ObjRootMtxMove(_ARGS_)
        //    => Call<int>(FuncAddress.ObjRootMtxMove, _ARGS_);

        //public static int ObjRootMtxMoveByChrDmyPoly(_ARGS_)
        //    => Call<int>(FuncAddress.ObjRootMtxMoveByChrDmyPoly, _ARGS_);

        //public static int ObjRootMtxMoveDmyPoly(_ARGS_)
        //    => Call<int>(FuncAddress.ObjRootMtxMoveDmyPoly, _ARGS_);

        //public static int OnActionCheckKey(_ARGS_)
        //    => Call<int>(FuncAddress.OnActionCheckKey, _ARGS_);

        //public static int OnActionEventRegion(_ARGS_)
        //    => Call<int>(FuncAddress.OnActionEventRegion, _ARGS_);

        //public static int OnActionEventRegionAttribute(_ARGS_)
        //    => Call<int>(FuncAddress.OnActionEventRegionAttribute, _ARGS_);

        //public static int OnBallista(_ARGS_)
        //    => Call<int>(FuncAddress.OnBallista, _ARGS_);

        //public static int OnBloodMenuClose(_ARGS_)
        //    => Call<int>(FuncAddress.OnBloodMenuClose, _ARGS_);

        //public static int OnBonfireEvent(_ARGS_)
        //    => Call<int>(FuncAddress.OnBonfireEvent, _ARGS_);

        //public static int OnCharacterAnimEnd(_ARGS_)
        //    => Call<int>(FuncAddress.OnCharacterAnimEnd, _ARGS_);

        //public static int OnCharacterDead(_ARGS_)
        //    => Call<int>(FuncAddress.OnCharacterDead, _ARGS_);

        //public static int OnCharacterHP(_ARGS_)
        //    => Call<int>(FuncAddress.OnCharacterHP, _ARGS_);

        //public static int OnCharacterHP_CheckAttacker(_ARGS_)
        //    => Call<int>(FuncAddress.OnCharacterHP_CheckAttacker, _ARGS_);

        //public static int OnCharacterHpRate(_ARGS_)
        //    => Call<int>(FuncAddress.OnCharacterHpRate, _ARGS_);

        //public static int OnCharacterTotalDamage(_ARGS_)
        //    => Call<int>(FuncAddress.OnCharacterTotalDamage, _ARGS_);

        //public static int OnCharacterTotalRateDamage(_ARGS_)
        //    => Call<int>(FuncAddress.OnCharacterTotalRateDamage, _ARGS_);

        //public static int OnCheckEzStateMessage(_ARGS_)
        //    => Call<int>(FuncAddress.OnCheckEzStateMessage, _ARGS_);

        //public static int OnChrAnimEnd(_ARGS_)
        //    => Call<int>(FuncAddress.OnChrAnimEnd, _ARGS_);

        //public static int OnChrAnimEndPlus(_ARGS_)
        //    => Call<int>(FuncAddress.OnChrAnimEndPlus, _ARGS_);

        //public static int OnDistanceAction(_ARGS_)
        //    => Call<int>(FuncAddress.OnDistanceAction, _ARGS_);

        //public static int OnDistanceActionAttribute(_ARGS_)
        //    => Call<int>(FuncAddress.OnDistanceActionAttribute, _ARGS_);

        //public static int OnDistanceActionDmyPoly(_ARGS_)
        //    => Call<int>(FuncAddress.OnDistanceActionDmyPoly, _ARGS_);

        //public static int OnDistanceActionPlus(_ARGS_)
        //    => Call<int>(FuncAddress.OnDistanceActionPlus, _ARGS_);

        //public static int OnDistanceActionPlusAttribute(_ARGS_)
        //    => Call<int>(FuncAddress.OnDistanceActionPlusAttribute, _ARGS_);

        //public static int OnDistanceJustIn(_ARGS_)
        //    => Call<int>(FuncAddress.OnDistanceJustIn, _ARGS_);

        //public static int OnEndFlow(_ARGS_)
        //    => Call<int>(FuncAddress.OnEndFlow, _ARGS_);

        //public static int OnFireDamage(_ARGS_)
        //    => Call<int>(FuncAddress.OnFireDamage, _ARGS_);

        //public static int OnKeyTime2(_ARGS_)
        //    => Call<int>(FuncAddress.OnKeyTime2, _ARGS_);

        //public static int OnNetDistanceIn(_ARGS_)
        //    => Call<int>(FuncAddress.OnNetDistanceIn, _ARGS_);

        //public static int OnNetRegion(_ARGS_)
        //    => Call<int>(FuncAddress.OnNetRegion, _ARGS_);

        //public static int OnNetRegionAttr(_ARGS_)
        //    => Call<int>(FuncAddress.OnNetRegionAttr, _ARGS_);

        //public static int OnNetRegionAttrPlus(_ARGS_)
        //    => Call<int>(FuncAddress.OnNetRegionAttrPlus, _ARGS_);

        //public static int OnNetRegionPlus(_ARGS_)
        //    => Call<int>(FuncAddress.OnNetRegionPlus, _ARGS_);

        //public static int OnObjAnimEnd(_ARGS_)
        //    => Call<int>(FuncAddress.OnObjAnimEnd, _ARGS_);

        //public static int OnObjAnimEndPlus(_ARGS_)
        //    => Call<int>(FuncAddress.OnObjAnimEndPlus, _ARGS_);

        //public static int OnObjDestroy(_ARGS_)
        //    => Call<int>(FuncAddress.OnObjDestroy, _ARGS_);

        //public static int OnObjectDamageHit(_ARGS_)
        //    => Call<int>(FuncAddress.OnObjectDamageHit, _ARGS_);

        //public static int OnObjectDamageHit_NoCall(_ARGS_)
        //    => Call<int>(FuncAddress.OnObjectDamageHit_NoCall, _ARGS_);

        //public static int OnObjectDamageHit_NoCallPlus(_ARGS_)
        //    => Call<int>(FuncAddress.OnObjectDamageHit_NoCallPlus, _ARGS_);

        //public static int OnPlayerActionInRegion(_ARGS_)
        //    => Call<int>(FuncAddress.OnPlayerActionInRegion, _ARGS_);

        //public static int OnPlayerActionInRegionAngle(_ARGS_)
        //    => Call<int>(FuncAddress.OnPlayerActionInRegionAngle, _ARGS_);

        //public static int OnPlayerActionInRegionAngleAttribute(_ARGS_)
        //    => Call<int>(FuncAddress.OnPlayerActionInRegionAngleAttribute, _ARGS_);

        //public static int OnPlayerActionInRegionAttribute(_ARGS_)
        //    => Call<int>(FuncAddress.OnPlayerActionInRegionAttribute, _ARGS_);

        //public static int OnPlayerAssessMenu(_ARGS_)
        //    => Call<int>(FuncAddress.OnPlayerAssessMenu, _ARGS_);

        //public static int OnPlayerDistanceAngleInTarget(_ARGS_)
        //    => Call<int>(FuncAddress.OnPlayerDistanceAngleInTarget, _ARGS_);

        //public static int OnPlayerDistanceInTarget(_ARGS_)
        //    => Call<int>(FuncAddress.OnPlayerDistanceInTarget, _ARGS_);

        //public static int OnPlayerDistanceOut(_ARGS_)
        //    => Call<int>(FuncAddress.OnPlayerDistanceOut, _ARGS_);

        //public static int OnPlayerKill(_ARGS_)
        //    => Call<int>(FuncAddress.OnPlayerKill, _ARGS_);

        //public static int OnRegionIn(_ARGS_)
        //    => Call<int>(FuncAddress.OnRegionIn, _ARGS_);

        //public static int OnRegionJustIn(_ARGS_)
        //    => Call<int>(FuncAddress.OnRegionJustIn, _ARGS_);

        //public static int OnRegionJustOut(_ARGS_)
        //    => Call<int>(FuncAddress.OnRegionJustOut, _ARGS_);

        //public static int OnRegistFunc(_ARGS_)
        //    => Call<int>(FuncAddress.OnRegistFunc, _ARGS_);

        //public static int OnRequestMenuEnd(_ARGS_)
        //    => Call<int>(FuncAddress.OnRequestMenuEnd, _ARGS_);

        //public static int OnRevengeMenuClose(_ARGS_)
        //    => Call<int>(FuncAddress.OnRevengeMenuClose, _ARGS_);

        //public static int OnSelectMenu(_ARGS_)
        //    => Call<int>(FuncAddress.OnSelectMenu, _ARGS_);

        //public static int OnSelfBloodMark(_ARGS_)
        //    => Call<int>(FuncAddress.OnSelfBloodMark, _ARGS_);

        //public static int OnSelfHeroBloodMark(_ARGS_)
        //    => Call<int>(FuncAddress.OnSelfHeroBloodMark, _ARGS_);

        //public static int OnSessionIn(_ARGS_)
        //    => Call<int>(FuncAddress.OnSessionIn, _ARGS_);

        //public static int OnSessionInfo(_ARGS_)
        //    => Call<int>(FuncAddress.OnSessionInfo, _ARGS_);

        //public static int OnSessionJustIn(_ARGS_)
        //    => Call<int>(FuncAddress.OnSessionJustIn, _ARGS_);

        //public static int OnSessionJustOut(_ARGS_)
        //    => Call<int>(FuncAddress.OnSessionJustOut, _ARGS_);

        //public static int OnSessionOut(_ARGS_)
        //    => Call<int>(FuncAddress.OnSessionOut, _ARGS_);

        //public static int OnSimpleDamage(_ARGS_)
        //    => Call<int>(FuncAddress.OnSimpleDamage, _ARGS_);

        //public static int OnTalkEvent(_ARGS_)
        //    => Call<int>(FuncAddress.OnTalkEvent, _ARGS_);

        //public static int OnTalkEventAngleOut(_ARGS_)
        //    => Call<int>(FuncAddress.OnTalkEventAngleOut, _ARGS_);

        //public static int OnTalkEventDistIn(_ARGS_)
        //    => Call<int>(FuncAddress.OnTalkEventDistIn, _ARGS_);

        //public static int OnTalkEventDistOut(_ARGS_)
        //    => Call<int>(FuncAddress.OnTalkEventDistOut, _ARGS_);

        //public static int OnTestEffectEndPlus(_ARGS_)
        //    => Call<int>(FuncAddress.OnTestEffectEndPlus, _ARGS_);

        //public static int OnTextEffectEnd(_ARGS_)
        //    => Call<int>(FuncAddress.OnTextEffectEnd, _ARGS_);

        //public static int OnTurnCharactorEnd(_ARGS_)
        //    => Call<int>(FuncAddress.OnTurnCharactorEnd, _ARGS_);

        //public static int OnWanderFade(_ARGS_)
        //    => Call<int>(FuncAddress.OnWanderFade, _ARGS_);

        //public static int OnWanderingDemon(_ARGS_)
        //    => Call<int>(FuncAddress.OnWanderingDemon, _ARGS_);

        //public static int OnWarpMenuClose(_ARGS_)
        //    => Call<int>(FuncAddress.OnWarpMenuClose, _ARGS_);

        //public static int OnYesNoDialog(_ARGS_)
        //    => Call<int>(FuncAddress.OnYesNoDialog, _ARGS_);

        public static int OpenCampMenu()
            => Call<int>(FuncAddress.OpenCampMenu);

        public static int OpeningDead(int ChrID, bool b)
            => Call<int>(FuncAddress.OpeningDead, ChrID, b);

        //public static int OpeningDeadPlus(_ARGS_)
        //    => Call<int>(FuncAddress.OpeningDeadPlus, _ARGS_);

        //public static int OpenSOSMsg_Tutorial(_ARGS_)
        //    => Call<int>(FuncAddress.OpenSOSMsg_Tutorial, _ARGS_);

        //public static int ParamInitialize(_ARGS_)
        //    => Call<int>(FuncAddress.ParamInitialize, _ARGS_);

        //public static int PauseTutorial(_ARGS_)
        //    => Call<int>(FuncAddress.PauseTutorial, _ARGS_);

        public static int PlayAnimation(int ChrID, int AnimID)
            => Call<int>(FuncAddress.PlayAnimation, ChrID, AnimID);

        public static int PlayAnimationStayCancel(int ChrID, int AnimID)
            => Call<int>(FuncAddress.PlayAnimationStayCancel, ChrID, AnimID);

        //public static int PlayerChrResetAnimation_RemoOnly(_ARGS_)
        //    => Call<int>(FuncAddress.PlayerChrResetAnimation_RemoOnly, _ARGS_);

        public static int PlayLoopAnimation(int ChrID, int AnimID)
            => Call<int>(FuncAddress.PlayLoopAnimation, ChrID, AnimID);

        //public static int PlayObjectSE(_ARGS_)
        //    => Call<int>(FuncAddress.PlayObjectSE, _ARGS_);

        //public static int PlayPointSE(_ARGS_)
        //    => Call<int>(FuncAddress.PlayPointSE, _ARGS_);

        //public static int PlayTypeSE(_ARGS_)
        //    => Call<int>(FuncAddress.PlayTypeSE, _ARGS_);

        //public static int RecallMenuEvent(_ARGS_)
        //    => Call<int>(FuncAddress.RecallMenuEvent, _ARGS_);

        //public static int ReconstructBreak(_ARGS_)
        //    => Call<int>(FuncAddress.ReconstructBreak, _ARGS_);

        //public static int RecoveryHeroin(_ARGS_)
        //    => Call<int>(FuncAddress.RecoveryHeroin, _ARGS_);

        //public static int RegistObjact(_ARGS_)
        //    => Call<int>(FuncAddress.RegistObjact, _ARGS_);

        //public static int RegistSimpleTalk(_ARGS_)
        //    => Call<int>(FuncAddress.RegistSimpleTalk, _ARGS_);

        public static bool RemoveInventoryEquip(ITEM_CATE Category, int ItemID)
            => Call<int>(FuncAddress.RemoveInventoryEquip, (int)Category, ItemID) == 1; //1 = success, -256 = fail

        //public static int RepeatMessage_begin(_ARGS_)
        //    => Call<int>(FuncAddress.RepeatMessage_begin, _ARGS_);

        //public static int RepeatMessage_end(_ARGS_)
        //    => Call<int>(FuncAddress.RepeatMessage_end, _ARGS_);

        public static int RequestEnding()
            => Call<int>(FuncAddress.RequestEnding);

        public static int RequestForceUpdateNetwork(int ChrID)
            => Call<int>(FuncAddress.RequestForceUpdateNetwork, ChrID);

        public static int RequestFullRecover()
            => Call<int>(FuncAddress.RequestFullRecover);

        public static int RequestGenerate(int ChrID)
            => Call<int>(FuncAddress.RequestGenerate, ChrID);

        //public static int RequestNormalUpdateNetwork(_ARGS_)
        //    => Call<int>(FuncAddress.RequestNormalUpdateNetwork, _ARGS_);

        public static int RequestOpenBriefingMsg(int MessageID, bool State)
            => Call<int>(FuncAddress.RequestOpenBriefingMsg, MessageID, State);

        //public static int RequestOpenBriefingMsgPlus(_ARGS_)
        //    => Call<int>(FuncAddress.RequestOpenBriefingMsgPlus, _ARGS_);

        //public static int RequestPlayMovie(_ARGS_)
        //    => Call<int>(FuncAddress.RequestPlayMovie, _ARGS_);

        //public static int RequestPlayMoviePlus(_ARGS_)
        //    => Call<int>(FuncAddress.RequestPlayMoviePlus, _ARGS_);

        //public static int RequestRemo(_ARGS_)
        //    => Call<int>(FuncAddress.RequestRemo, _ARGS_);

        //public static int RequestRemoPlus(_ARGS_)
        //    => Call<int>(FuncAddress.RequestRemoPlus, _ARGS_);

        public static int RequestUnlockTrophy(int TrophyID)
            => Call<int>(FuncAddress.RequestUnlockTrophy, TrophyID);

        public static int ReqularLeavePlayer(int NetworkPlayerID)
            => Call<int>(FuncAddress.ReqularLeavePlayer, NetworkPlayerID);

        public static int ResetCamAngle()
            => Call<int>(FuncAddress.ResetCamAngle);

        public static int ResetEventQwcSpEffect(int ChrID)
            => Call<int>(FuncAddress.ResetEventQwcSpEffect, ChrID);

        public static int ResetSummonParam()
            => Call<int>(FuncAddress.ResetSummonParam);

        public static int ResetSyncRideObjInfo(int UnknownID)
            => Call<int>(FuncAddress.ResetSyncRideObjInfo, UnknownID);

        public static int ResetThink(int ChrID)
            => Call<int>(FuncAddress.ResetThink, ChrID);

        public static int RestorePiece(int ObjID)
            => Call<int>(FuncAddress.RestorePiece, ObjID);

        public static int ReturnMapSelect()
            => Call<int>(FuncAddress.ReturnMapSelect);

        public static bool RevivePlayer()
            => Call<bool>(FuncAddress.RevivePlayer);

        public static int RevivePlayerNext()
            => Call<int>(FuncAddress.RevivePlayerNext);

        public static int SaveRequest()
            => Call<int>(FuncAddress.SaveRequest);

        public static int SaveRequest_Profile()
            => Call<int>(FuncAddress.SaveRequest_Profile);

        //public static int SendEventRequest(_ARGS_)
        //    => Call<int>(FuncAddress.SendEventRequest, _ARGS_);

        public static int SetAlive(int ChrID, bool State)
            => Call<int>(FuncAddress.SetAlive, ChrID, State);

        public static int SetAliveMotion(bool a)
            => Call<int>(FuncAddress.SetAliveMotion, a);

        //public static int SetAlwaysDrawForEvent(_ARGS_)
        //    => Call<int>(FuncAddress.SetAlwaysDrawForEvent, _ARGS_);

        //public static int SetAlwaysEnableBackread_forEvent(_ARGS_)
        //    => Call<int>(FuncAddress.SetAlwaysEnableBackread_forEvent, _ARGS_);

        //public static int SetAngleFoward(_ARGS_)
        //    => Call<int>(FuncAddress.SetAngleFoward, _ARGS_);

        public static int SetAreaStartMapUid(int AreaID)
            => Call<int>(FuncAddress.SetAreaStartMapUid, AreaID);

        public static int SetBossGauge(int ChrID, int GaugeID, int NameID)
            => Call<int>(FuncAddress.SetBossGauge, ChrID, GaugeID, NameID);

        //public static int SetBossUnitJrHit(_ARGS_)
        //    => Call<int>(FuncAddress.SetBossUnitJrHit, _ARGS_);

        //public static int SetBountyRankPoint(_ARGS_)
        //    => Call<int>(FuncAddress.SetBountyRankPoint, _ARGS_);

        public static int SetBrokenPiece(int ObjID)
            => Call<int>(FuncAddress.SetBrokenPiece, ObjID);

        //public static int SetCamModeParamTargetId(_ARGS_)
        //    => Call<int>(FuncAddress.SetCamModeParamTargetId, _ARGS_);

        //public static int SetCamModeParamTargetIdForBossLock(_ARGS_)
        //    => Call<int>(FuncAddress.SetCamModeParamTargetIdForBossLock, _ARGS_);

        public static int SetChrType(int ChrID, CHR_TYPE Type)
            => Call<int>(FuncAddress.SetChrType, ChrID, (int)Type);

        public static int SetChrTypeDataGrey()
            => Call<int>(FuncAddress.SetChrTypeDataGrey);

        //public static int SetChrTypeDataGreyNext(_ARGS_)
        //    => Call<int>(FuncAddress.SetChrTypeDataGreyNext, _ARGS_);

        //public static int SetClearBonus(_ARGS_)
        //    => Call<int>(FuncAddress.SetClearBonus, _ARGS_);

        public static int SetClearItem(bool a)
            => Call<int>(FuncAddress.SetClearItem, a);

        public static int SetClearSesiionCount()
            => Call<int>(FuncAddress.SetClearSesiionCount);

        //public static int SetClearState(_ARGS_)
        //    => Call<int>(FuncAddress.SetClearState, _ARGS_);

        //public static int SetColiEnable(_ARGS_)
        //    => Call<int>(FuncAddress.SetColiEnable, _ARGS_);

        //public static int SetColiEnableArray(_ARGS_)
        //    => Call<int>(FuncAddress.SetColiEnableArray, _ARGS_);

        //public static int SetCompletelyNoMove(_ARGS_)
        //    => Call<int>(FuncAddress.SetCompletelyNoMove, _ARGS_);

        //public static int SetDeadMode(_ARGS_)
        //    => Call<int>(FuncAddress.SetDeadMode, _ARGS_);

        //public static int SetDeadMode2(_ARGS_)
        //    => Call<int>(FuncAddress.SetDeadMode2, _ARGS_);

        //public static int SetDefaultAnimation(_ARGS_)
        //    => Call<int>(FuncAddress.SetDefaultAnimation, _ARGS_);

        //public static int SetDefaultMapUid(_ARGS_)
        //    => Call<int>(FuncAddress.SetDefaultMapUid, _ARGS_);

        public static int SetDefaultRoutePoint(int ChrID)
            => Call<int>(FuncAddress.SetDefaultRoutePoint, ChrID);

        public static int SetDisable(int ChrID, bool State)
            => Call<int>(FuncAddress.SetDisable, ChrID, State);

        public static int SetDisableBackread_forEvent(int ChrID, bool State)
            => Call<int>(FuncAddress.SetDisableBackread_forEvent, ChrID, State);

        public static int SetDisableDamage(int ChrID, bool State)
            => Call<int>(FuncAddress.SetDisableDamage, ChrID, State);

        public static int SetDisableGravity(int ChrID, bool State)
            => Call<int>(FuncAddress.SetDisableGravity, ChrID, State);

        //public static int SetDisableWeakDamageAnim(_ARGS_)
        //    => Call<int>(FuncAddress.SetDisableWeakDamageAnim, _ARGS_);

        //public static int SetDisableWeakDamageAnim_light(_ARGS_)
        //    => Call<int>(FuncAddress.SetDisableWeakDamageAnim_light, _ARGS_);

        //public static int SetDispMask(_ARGS_)
        //    => Call<int>(FuncAddress.SetDispMask, _ARGS_);

        public static int SetDrawEnable(int ChrID, bool State)
            => Call<int>(FuncAddress.SetDrawEnable, ChrID, State);

        //public static int SetDrawEnableArray(_ARGS_)
        //    => Call<int>(FuncAddress.SetDrawEnableArray, _ARGS_);

        //public static int SetDrawGroup(_ARGS_)
        //    => Call<int>(FuncAddress.SetDrawGroup, _ARGS_);

        public static int SetEnableEventPad(int ChrID, bool State)
            => Call<int>(FuncAddress.SetEnableEventPad, ChrID, State);

        //public static int SetEventBodyBulletCorrect(_ARGS_)
        //    => Call<int>(FuncAddress.SetEventBodyBulletCorrect, _ARGS_);

        //public static int SetEventBodyMaterialSeAndSfx(_ARGS_)
        //    => Call<int>(FuncAddress.SetEventBodyMaterialSeAndSfx, _ARGS_);

        //public static int SetEventBodyMaxHp(_ARGS_)
        //    => Call<int>(FuncAddress.SetEventBodyMaxHp, _ARGS_);

        public static int SetEventCommand(int ChrID, int b)
            => Call<int>(FuncAddress.SetEventCommand, ChrID, b);

        //public static int SetEventCommandIndex(_ARGS_)
        //    => Call<int>(FuncAddress.SetEventCommandIndex, _ARGS_);

        public static int SetEventFlag(int EventID, bool State)
            => Call<int>(FuncAddress.SetEventFlag, EventID, State);

        //public static int SetEventFlagValue(_ARGS_)
        //    => Call<int>(FuncAddress.SetEventFlagValue, _ARGS_);

        public static int SetEventGenerate(int ChrID, bool State)
            => Call<int>(FuncAddress.SetEventGenerate, ChrID, State);

        public static int SetEventMovePointType(int ChrID, POINT_MOVE_TYPE Type)
            => Call<int>(FuncAddress.SetEventMovePointType, ChrID, (int)Type);

        //public static int SetEventSimpleTalk(_ARGS_)
        //    => Call<int>(FuncAddress.SetEventSimpleTalk, _ARGS_);

        public static int SetEventSpecialEffect(int ChrID, int EffectID)
            => Call<int>(FuncAddress.SetEventSpecialEffect, ChrID, EffectID);

        public static int SetEventSpecialEffect_2(int ChrID, int EffectID)
            => Call<int>(FuncAddress.SetEventSpecialEffect_2, ChrID, EffectID);

        //public static int SetEventSpecialEffectOwner(_ARGS_)
        //    => Call<int>(FuncAddress.SetEventSpecialEffectOwner, _ARGS_);

        //public static int SetEventSpecialEffectOwner_2(_ARGS_)
        //    => Call<int>(FuncAddress.SetEventSpecialEffectOwner_2, _ARGS_);

        public static int SetEventTarget(int ChrID1, int ChrID2)
            => Call<int>(FuncAddress.SetEventTarget, ChrID1, ChrID2);

        //public static int SetExVelocity(_ARGS_)
        //    => Call<int>(FuncAddress.SetExVelocity, _ARGS_);

        //public static int SetFirstSpeed(_ARGS_)
        //    => Call<int>(FuncAddress.SetFirstSpeed, _ARGS_);

        //public static int SetFirstSpeedPlus(_ARGS_)
        //    => Call<int>(FuncAddress.SetFirstSpeedPlus, _ARGS_);

        //public static int SetFlagInitState(_ARGS_)
        //    => Call<int>(FuncAddress.SetFlagInitState, _ARGS_);

        //public static int SetFootIKInterpolateType(_ARGS_)
        //    => Call<int>(FuncAddress.SetFootIKInterpolateType, _ARGS_);

        public static int SetForceJoinBlackRequest()
            => Call<int>(FuncAddress.SetForceJoinBlackRequest);

        //public static int SetHitInfo(_ARGS_)
        //    => Call<int>(FuncAddress.SetHitInfo, _ARGS_);

        public static bool SetHitMask(int ChrID, int NewHitMask)
            => Call<bool>(FuncAddress.SetHitMask, NewHitMask);

        public static int SetHp(int ChrID, float HP)
            => Call<int>(FuncAddress.SetHp, ChrID, HP);

        public static int SetIgnoreHit(int ChrID, bool State)
            => Call<int>(FuncAddress.SetIgnoreHit, ChrID, State);

        public static int SetInfomationPriority(int a)
            => Call<int>(FuncAddress.SetInfomationPriority, a);

        public static int SetInsideBattleArea(int ChrID, bool State)
            => Call<int>(FuncAddress.SetInsideBattleArea, ChrID, State);

        //public static int SetIsAnimPauseOnRemoPlayForEvent(_ARGS_)
        //    => Call<int>(FuncAddress.SetIsAnimPauseOnRemoPlayForEvent, _ARGS_);

        //public static int SetKeepCommandIndex(_ARGS_)
        //    => Call<int>(FuncAddress.SetKeepCommandIndex, _ARGS_);

        //public static int SetLoadWait(_ARGS_)
        //    => Call<int>(FuncAddress.SetLoadWait, _ARGS_);

        //public static int SetLockActPntInvalidateMask(_ARGS_)
        //    => Call<int>(FuncAddress.SetLockActPntInvalidateMask, _ARGS_);

        //Note: c and d are likely the 3rd and 4th numbers in map name (m##_##_##_##)
        public static int SetMapUid(int Area, int Block, int c, int d, int Point)
            => Call<int>(FuncAddress.SetMapUid, Area, Block, c, d, Point);

        //public static int SetMaxHp(_ARGS_)
        //    => Call<int>(FuncAddress.SetMaxHp, _ARGS_);

        //public static int SetMenuBrake(_ARGS_)
        //    => Call<int>(FuncAddress.SetMenuBrake, _ARGS_);

        //public static int SetMiniBlockIndex(_ARGS_)
        //    => Call<int>(FuncAddress.SetMiniBlockIndex, _ARGS_);

        public static int SetMovePoint(int ChrID, int AreaID, float c)
            => Call<int>(FuncAddress.SetMovePoint, ChrID, AreaID, c);

        //public static int SetMultiWallMapUid(_ARGS_)
        //    => Call<int>(FuncAddress.SetMultiWallMapUid, _ARGS_);

        //public static int SetNoNetSync(_ARGS_)
        //    => Call<int>(FuncAddress.SetNoNetSync, _ARGS_);

        //public static int SetObjDeactivate(_ARGS_)
        //    => Call<int>(FuncAddress.SetObjDeactivate, _ARGS_);

        //public static int SetObjDisableBreak(_ARGS_)
        //    => Call<int>(FuncAddress.SetObjDisableBreak, _ARGS_);

        //public static int SetObjEventCollisionFill(_ARGS_)
        //    => Call<int>(FuncAddress.SetObjEventCollisionFill, _ARGS_);

        public static int SetObjSfx(int a, int b, int c, bool d)
            => Call<int>(FuncAddress.SetObjSfx, a, b, c, d);

        //public static int SetReturnPointEntityId(_ARGS_)
        //    => Call<int>(FuncAddress.SetReturnPointEntityId, _ARGS_);

        public static int SetReviveWait(bool State)
            => Call<int>(FuncAddress.SetReviveWait, State);

        //public static int SetSelfBloodMapUid(_ARGS_)
        //    => Call<int>(FuncAddress.SetSelfBloodMapUid, _ARGS_);

        //public static int SetSosSignPos(_ARGS_)
        //    => Call<int>(FuncAddress.SetSosSignPos, _ARGS_);

        //public static int SetSosSignWarp(_ARGS_)
        //    => Call<int>(FuncAddress.SetSosSignWarp, _ARGS_);

        //public static int SetSpStayAndDamageAnimId(_ARGS_)
        //    => Call<int>(FuncAddress.SetSpStayAndDamageAnimId, _ARGS_);

        //public static int SetSpStayAndDamageAnimIdPlus(_ARGS_)
        //    => Call<int>(FuncAddress.SetSpStayAndDamageAnimIdPlus, _ARGS_);

        //public static int SetSubMenuBrake(_ARGS_)
        //    => Call<int>(FuncAddress.SetSubMenuBrake, _ARGS_);

        public static int SetSummonedPos()
            => Call<int>(FuncAddress.SetSummonedPos);

        //public static int SetSyncRideObjInfo(_ARGS_)
        //    => Call<int>(FuncAddress.SetSyncRideObjInfo, _ARGS_);

        //public static int SetSystemIgnore(_ARGS_)
        //    => Call<int>(FuncAddress.SetSystemIgnore, _ARGS_);

        //public static int SetTalkMsg(_ARGS_)
        //    => Call<int>(FuncAddress.SetTalkMsg, _ARGS_);

        public static int SetTeamType(int ChrID, TEAM_TYPE Type)
            => Call<int>(FuncAddress.SetTeamType, ChrID, (int)Type);

        //TODO: CHECK IF INFERRED CORRECTLY
        public static int SetTeamTypeDefault(int ChrID)
            => Call<int>(FuncAddress.SetTeamTypeDefault, ChrID);

        //public static int SetTeamTypePlus(_ARGS_)
        //    => Call<int>(FuncAddress.SetTeamTypePlus, _ARGS_);

        public static int SetTextEffect(int EffectID)
            => Call<int>(FuncAddress.SetTextEffect, EffectID);

        public static int SetTutorialSummonedPos()
            => Call<int>(FuncAddress.SetTutorialSummonedPos);

        public static int SetValidTalk(int ChrID, bool b)
            => Call<int>(FuncAddress.SetValidTalk, ChrID, b);

        public static int ShowGenDialog(int MessageID, int b, int c, bool d)
            => Call<int>(FuncAddress.ShowGenDialog, MessageID, b, c, d);

        //public static int ShowRankingDialog(_ARGS_)
        //    => Call<int>(FuncAddress.ShowRankingDialog, _ARGS_);

        //public static int SOSMsgGetResult_Tutorial(_ARGS_)
        //    => Call<int>(FuncAddress.SOSMsgGetResult_Tutorial, _ARGS_);

        //public static int StopLoopAnimation(_ARGS_)
        //    => Call<int>(FuncAddress.StopLoopAnimation, _ARGS_);

        //WARNING: NOT REVERSABLE. PLAYER WILL BE UNABLE TO MOVE UNTIL YOU QUIT TO TITLE SCREEN.
        public static int StopPlayer()
            => Call<int>(FuncAddress.StopPlayer);

        //public static int StopPointSE(_ARGS_)
        //    => Call<int>(FuncAddress.StopPointSE, _ARGS_);

        //public static int SubActionCount(_ARGS_)
        //    => Call<int>(FuncAddress.SubActionCount, _ARGS_);

        //public static int SubDispMaskByBit(_ARGS_)
        //    => Call<int>(FuncAddress.SubDispMaskByBit, _ARGS_);

        //public static int SubHitMask(_ARGS_)
        //    => Call<int>(FuncAddress.SubHitMask, _ARGS_);

        //public static int SubHitMaskByBit(_ARGS_)
        //    => Call<int>(FuncAddress.SubHitMaskByBit, _ARGS_);

        public static int SummonBlackRequest(int UnknownID)
            => Call<int>(FuncAddress.SummonBlackRequest, UnknownID);

        public static int SummonedMapReload()
            => Call<int>(FuncAddress.SummonedMapReload);

        //public static int SummonSuccess(_ARGS_)
        //    => Call<int>(FuncAddress.SummonSuccess, _ARGS_);

        //public static int SwitchDispMask(_ARGS_)
        //    => Call<int>(FuncAddress.SwitchDispMask, _ARGS_);

        //public static int SwitchHitMask(_ARGS_)
        //    => Call<int>(FuncAddress.SwitchHitMask, _ARGS_);

        public static int TalkNextPage(int a)
            => Call<int>(FuncAddress.TalkNextPage, a);

        //public static int TreasureDispModeChange(_ARGS_)
        //    => Call<int>(FuncAddress.TreasureDispModeChange, _ARGS_);

        //public static int TreasureDispModeChange2(_ARGS_)
        //    => Call<int>(FuncAddress.TreasureDispModeChange2, _ARGS_);

        public static int TurnCharactor(int ChrID1, int ChrID2)
            => Call<int>(FuncAddress.TurnCharactor, ChrID1, ChrID2);

        //public static int Tutorial_begin(_ARGS_)
        //    => Call<int>(FuncAddress.Tutorial_begin, _ARGS_);

        //public static int Tutorial_end(_ARGS_)
        //    => Call<int>(FuncAddress.Tutorial_end, _ARGS_);

        //public static int UnLockSession(_ARGS_)
        //    => Call<int>(FuncAddress.UnLockSession, _ARGS_);

        //public static int UpDateBloodMark(_ARGS_)
        //    => Call<int>(FuncAddress.UpDateBloodMark, _ARGS_);

        //public static int Util_RequestLevelUp(_ARGS_)
        //    => Call<int>(FuncAddress.Util_RequestLevelUp, _ARGS_);

        //public static int Util_RequestLevelUpFirst(_ARGS_)
        //    => Call<int>(FuncAddress.Util_RequestLevelUpFirst, _ARGS_);

        //public static int Util_RequestRegene(_ARGS_)
        //    => Call<int>(FuncAddress.Util_RequestRegene, _ARGS_);

        //public static int Util_RequestRespawn(_ARGS_)
        //    => Call<int>(FuncAddress.Util_RequestRespawn, _ARGS_);

        //public static int ValidPointLight(_ARGS_)
        //    => Call<int>(FuncAddress.ValidPointLight, _ARGS_);

        //public static int ValidSfx(_ARGS_)
        //    => Call<int>(FuncAddress.ValidSfx, _ARGS_);

        //public static int VariableExpand_211_Param1(_ARGS_)
        //    => Call<int>(FuncAddress.VariableExpand_211_Param1, _ARGS_);

        //public static int VariableExpand_211_param2(_ARGS_)
        //    => Call<int>(FuncAddress.VariableExpand_211_param2, _ARGS_);

        //public static int VariableExpand_211_param3(_ARGS_)
        //    => Call<int>(FuncAddress.VariableExpand_211_param3, _ARGS_);

        //public static int VariableExpand_22_param1(_ARGS_)
        //    => Call<int>(FuncAddress.VariableExpand_22_param1, _ARGS_);

        //public static int VariableExpand_22_param2(_ARGS_)
        //    => Call<int>(FuncAddress.VariableExpand_22_param2, _ARGS_);

        //public static int VariableOrder_211(_ARGS_)
        //    => Call<int>(FuncAddress.VariableOrder_211, _ARGS_);

        //public static int VariableOrder_22(_ARGS_)
        //    => Call<int>(FuncAddress.VariableOrder_22, _ARGS_);

        public static int WARN(string txt)
            => Call<int>(FuncAddress.WARN, txt);

        public static int Warp(int ChrID, int AreaID)
            => Call<int>(FuncAddress.Warp, ChrID, AreaID);

        public static int WarpDmy(int ChrID, int b, int c)
            => Call<int>(FuncAddress.WarpDmy, ChrID, b, c);

        public static int WarpNextStage(int Area, int Block, int c, int d, int Point)
            => Call<int>(FuncAddress.WarpNextStage, Area, Block, c, d, Point);

        public static int WarpNextStage_Bonfire(int BonfireID)
            => Call<int>(FuncAddress.WarpNextStage_Bonfire, BonfireID);

        public static int WarpNextStageKick()
            => Call<int>(FuncAddress.WarpNextStageKick);

        public static int WarpRestart(int ChrID, int AreaID)
            => Call<int>(FuncAddress.WarpRestart, ChrID, AreaID);

        //public static int WarpRestartNoGrey(_ARGS_)
        //    => Call<int>(FuncAddress.WarpRestartNoGrey, _ARGS_);

        public static int WarpSelfBloodMark(bool a)
            => Call<int>(FuncAddress.WarpSelfBloodMark, a);


    }
}
