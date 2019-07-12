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
        public static class Unmapped
        {
            public static int ActionEnd(params dynamic[] args)
                => Call<int>(FuncAddress.ActionEnd, args);

            public static int AddActionCount(params dynamic[] args)
                => Call<int>(FuncAddress.AddActionCount, args);

            public static int AddBlockClearBonus(params dynamic[] args)
                => Call<int>(FuncAddress.AddBlockClearBonus, args);

            public static int AddClearCount(params dynamic[] args)
                => Call<int>(FuncAddress.AddClearCount, args);

            public static int AddCorpseEvent(params dynamic[] args)
                => Call<int>(FuncAddress.AddCorpseEvent, args);

            public static int AddCurrentVowRankPoint(params dynamic[] args)
                => Call<int>(FuncAddress.AddCurrentVowRankPoint, args);

            public static int AddCustomRoutePoint(params dynamic[] args)
                => Call<int>(FuncAddress.AddCustomRoutePoint, args);

            public static int AddDeathCount(params dynamic[] args)
                => Call<int>(FuncAddress.AddDeathCount, args);

            public static int AddEventGoal(params dynamic[] args)
                => Call<int>(FuncAddress.AddEventGoal, args);

            public static int AddEventParts(params dynamic[] args)
                => Call<int>(FuncAddress.AddEventParts, args);

            public static int AddEventParts_Ignore(params dynamic[] args)
                => Call<int>(FuncAddress.AddEventParts_Ignore, args);

            public static int AddEventSimpleTalk(params dynamic[] args)
                => Call<int>(FuncAddress.AddEventSimpleTalk, args);

            public static int AddEventSimpleTalkTimer(params dynamic[] args)
                => Call<int>(FuncAddress.AddEventSimpleTalkTimer, args);

            public static int AddFieldInsFilter(params dynamic[] args)
                => Call<int>(FuncAddress.AddFieldInsFilter, args);

            public static int AddGeneEvent(params dynamic[] args)
                => Call<int>(FuncAddress.AddGeneEvent, args);

            public static int AddHelpWhiteGhost(params dynamic[] args)
                => Call<int>(FuncAddress.AddHelpWhiteGhost, args);

            public static int AddHitMaskByBit(params dynamic[] args)
                => Call<int>(FuncAddress.AddHitMaskByBit, args);

            public static int AddInfomationBuffer(params dynamic[] args)
                => Call<int>(FuncAddress.AddInfomationBuffer, args);

            public static int AddInfomationBufferTag(params dynamic[] args)
                => Call<int>(FuncAddress.AddInfomationBufferTag, args);

            public static int AddInfomationList(params dynamic[] args)
                => Call<int>(FuncAddress.AddInfomationList, args);

            public static int AddInfomationListItem(params dynamic[] args)
                => Call<int>(FuncAddress.AddInfomationListItem, args);

            public static int AddInfomationTimeMsgTag(params dynamic[] args)
                => Call<int>(FuncAddress.AddInfomationTimeMsgTag, args);

            public static int AddInfomationTosBuffer(params dynamic[] args)
                => Call<int>(FuncAddress.AddInfomationTosBuffer, args);

            public static int AddInfomationTosBufferPlus(params dynamic[] args)
                => Call<int>(FuncAddress.AddInfomationTosBufferPlus, args);

            public static int AddInventoryItem(params dynamic[] args)
                => Call<int>(FuncAddress.AddInventoryItem, args);

            public static int AddKillBlackGhost(params dynamic[] args)
                => Call<int>(FuncAddress.AddKillBlackGhost, args);

            public static int AddQWC(params dynamic[] args)
                => Call<int>(FuncAddress.AddQWC, args);

            public static int AddRumble(params dynamic[] args)
                => Call<int>(FuncAddress.AddRumble, args);

            public static int AddTreasureEvent(params dynamic[] args)
                => Call<int>(FuncAddress.AddTreasureEvent, args);

            public static int AddTrueDeathCount(params dynamic[] args)
                => Call<int>(FuncAddress.AddTrueDeathCount, args);

            public static int BeginAction(params dynamic[] args)
                => Call<int>(FuncAddress.BeginAction, args);

            public static int BeginLoopCheck(params dynamic[] args)
                => Call<int>(FuncAddress.BeginLoopCheck, args);

            public static int CalcExcuteMultiBonus(params dynamic[] args)
                => Call<int>(FuncAddress.CalcExcuteMultiBonus, args);

            public static int CalcGetCurrentMapEntityId(params dynamic[] args)
                => Call<int>(FuncAddress.CalcGetCurrentMapEntityId, args);

            public static int CalcGetMultiWallEntityId(params dynamic[] args)
                => Call<int>(FuncAddress.CalcGetMultiWallEntityId, args);

            public static int CamReset(params dynamic[] args)
                => Call<int>(FuncAddress.CamReset, args);

            public static int CastPointSpell(params dynamic[] args)
                => Call<int>(FuncAddress.CastPointSpell, args);

            public static int CastPointSpell_Horming(params dynamic[] args)
                => Call<int>(FuncAddress.CastPointSpell_Horming, args);

            public static int CastPointSpellPlus(params dynamic[] args)
                => Call<int>(FuncAddress.CastPointSpellPlus, args);

            public static int CastPointSpellPlus_Horming(params dynamic[] args)
                => Call<int>(FuncAddress.CastPointSpellPlus_Horming, args);

            public static int CastTargetSpell(params dynamic[] args)
                => Call<int>(FuncAddress.CastTargetSpell, args);

            public static int CastTargetSpellPlus(params dynamic[] args)
                => Call<int>(FuncAddress.CastTargetSpellPlus, args);

            public static int ChangeGreyGhost(params dynamic[] args)
                => Call<int>(FuncAddress.ChangeGreyGhost, args);

            public static int ChangeInitPosAng(params dynamic[] args)
                => Call<int>(FuncAddress.ChangeInitPosAng, args);

            public static int ChangeModel(params dynamic[] args)
                => Call<int>(FuncAddress.ChangeModel, args);

            public static int ChangeTarget(params dynamic[] args)
                => Call<int>(FuncAddress.ChangeTarget, args);

            public static int ChangeThink(params dynamic[] args)
                => Call<int>(FuncAddress.ChangeThink, args);

            public static int ChangeWander(params dynamic[] args)
                => Call<int>(FuncAddress.ChangeWander, args);

            public static int CharacterAllAttachSys(params dynamic[] args)
                => Call<int>(FuncAddress.CharacterAllAttachSys, args);

            public static int CharactorCopyPosAng(params dynamic[] args)
                => Call<int>(FuncAddress.CharactorCopyPosAng, args);

            public static int CheckChrHit_Obj(params dynamic[] args)
                => Call<int>(FuncAddress.CheckChrHit_Obj, args);

            public static int CheckChrHit_Wall(params dynamic[] args)
                => Call<int>(FuncAddress.CheckChrHit_Wall, args);

            public static int CheckEventBody(params dynamic[] args)
                => Call<int>(FuncAddress.CheckEventBody, args);

            public static int CheckEventChr_Proxy(params dynamic[] args)
                => Call<int>(FuncAddress.CheckEventChr_Proxy, args);

            public static int CheckPenalty(params dynamic[] args)
                => Call<int>(FuncAddress.CheckPenalty, args);

            public static int ChrDisableUpdate(params dynamic[] args)
                => Call<int>(FuncAddress.ChrDisableUpdate, args);

            public static int ChrFadeIn(params dynamic[] args)
                => Call<int>(FuncAddress.ChrFadeIn, args);

            public static int ChrFadeOut(params dynamic[] args)
                => Call<int>(FuncAddress.ChrFadeOut, args);

            public static int ChrResetAnimation(params dynamic[] args)
                => Call<int>(FuncAddress.ChrResetAnimation, args);

            public static int ChrResetRequest(params dynamic[] args)
                => Call<int>(FuncAddress.ChrResetRequest, args);

            public static int ClearBossGauge(params dynamic[] args)
                => Call<int>(FuncAddress.ClearBossGauge, args);

            public static int ClearMyWorldState(params dynamic[] args)
                => Call<int>(FuncAddress.ClearMyWorldState, args);

            public static int ClearSosSign(params dynamic[] args)
                => Call<int>(FuncAddress.ClearSosSign, args);

            public static int ClearTarget(params dynamic[] args)
                => Call<int>(FuncAddress.ClearTarget, args);

            public static int CloseGenDialog(params dynamic[] args)
                => Call<int>(FuncAddress.CloseGenDialog, args);

            public static int CloseMenu(params dynamic[] args)
                => Call<int>(FuncAddress.CloseMenu, args);

            public static int CloseRankingDialog(params dynamic[] args)
                => Call<int>(FuncAddress.CloseRankingDialog, args);

            public static int CloseTalk(params dynamic[] args)
                => Call<int>(FuncAddress.CloseTalk, args);

            public static int CompleteEvent(params dynamic[] args)
                => Call<int>(FuncAddress.CompleteEvent, args);

            public static int CreateCamSfx(params dynamic[] args)
                => Call<int>(FuncAddress.CreateCamSfx, args);

            public static int CreateDamage_NoCollision(params dynamic[] args)
                => Call<int>(FuncAddress.CreateDamage_NoCollision, args);

            public static int CreateEventBody(params dynamic[] args)
                => Call<int>(FuncAddress.CreateEventBody, args);

            public static int CreateEventBodyPlus(params dynamic[] args)
                => Call<int>(FuncAddress.CreateEventBodyPlus, args);

            public static int CreateHeroBloodStain(params dynamic[] args)
                => Call<int>(FuncAddress.CreateHeroBloodStain, args);

            public static int CreateSfx(params dynamic[] args)
                => Call<int>(FuncAddress.CreateSfx, args);

            public static int CreateSfx_DummyPoly(params dynamic[] args)
                => Call<int>(FuncAddress.CreateSfx_DummyPoly, args);

            public static int CroseBriefingMsg(params dynamic[] args)
                => Call<int>(FuncAddress.CroseBriefingMsg, args);

            public static int CustomLuaCall(params dynamic[] args)
                => Call<int>(FuncAddress.CustomLuaCall, args);

            public static int CustomLuaCallStart(params dynamic[] args)
                => Call<int>(FuncAddress.CustomLuaCallStart, args);

            public static int CustomLuaCallStartPlus(params dynamic[] args)
                => Call<int>(FuncAddress.CustomLuaCallStartPlus, args);

            public static int DeleteCamSfx(params dynamic[] args)
                => Call<int>(FuncAddress.DeleteCamSfx, args);

            public static int DeleteEvent(params dynamic[] args)
                => Call<int>(FuncAddress.DeleteEvent, args);

            public static int DeleteObjSfxAll(params dynamic[] args)
                => Call<int>(FuncAddress.DeleteObjSfxAll, args);

            public static int DeleteObjSfxDmyPlyID(params dynamic[] args)
                => Call<int>(FuncAddress.DeleteObjSfxDmyPlyID, args);

            public static int DeleteObjSfxEventID(params dynamic[] args)
                => Call<int>(FuncAddress.DeleteObjSfxEventID, args);

            public static int DisableCollection(params dynamic[] args)
                => Call<int>(FuncAddress.DisableCollection, args);

            public static int DisableDamage(params dynamic[] args)
                => Call<int>(FuncAddress.DisableDamage, args);

            public static int DisableHpGauge(params dynamic[] args)
                => Call<int>(FuncAddress.DisableHpGauge, args);

            public static int DisableInterupt(params dynamic[] args)
                => Call<int>(FuncAddress.DisableInterupt, args);

            public static int DisableMapHit(params dynamic[] args)
                => Call<int>(FuncAddress.DisableMapHit, args);

            public static int DisableMove(params dynamic[] args)
                => Call<int>(FuncAddress.DisableMove, args);

            public static int DivideRest(params dynamic[] args)
                => Call<int>(FuncAddress.DivideRest, args);

            public static int EnableAction(params dynamic[] args)
                => Call<int>(FuncAddress.EnableAction, args);

            public static int EnableGeneratorSystem(params dynamic[] args)
                => Call<int>(FuncAddress.EnableGeneratorSystem, args);

            public static int EnableHide(params dynamic[] args)
                => Call<int>(FuncAddress.EnableHide, args);

            public static int EnableInvincible(params dynamic[] args)
                => Call<int>(FuncAddress.EnableInvincible, args);

            public static int EnableLogic(params dynamic[] args)
                => Call<int>(FuncAddress.EnableLogic, args);

            public static int EnableObjTreasure(params dynamic[] args)
                => Call<int>(FuncAddress.EnableObjTreasure, args);

            public static int EndAnimation(params dynamic[] args)
                => Call<int>(FuncAddress.EndAnimation, args);

            public static int EraseEventSpecialEffect(params dynamic[] args)
                => Call<int>(FuncAddress.EraseEventSpecialEffect, args);

            public static int EraseEventSpecialEffect_2(params dynamic[] args)
                => Call<int>(FuncAddress.EraseEventSpecialEffect_2, args);

            public static int EventTagInsertString_forPlayerNo(params dynamic[] args)
                => Call<int>(FuncAddress.EventTagInsertString_forPlayerNo, args);

            public static int ExcutePenalty(params dynamic[] args)
                => Call<int>(FuncAddress.ExcutePenalty, args);

            public static int ForceChangeTarget(params dynamic[] args)
                => Call<int>(FuncAddress.ForceChangeTarget, args);

            public static int ForceDead(params dynamic[] args)
                => Call<int>(FuncAddress.ForceDead, args);

            public static int ForcePlayAnimation(params dynamic[] args)
                => Call<int>(FuncAddress.ForcePlayAnimation, args);

            public static int ForcePlayAnimationStayCancel(params dynamic[] args)
                => Call<int>(FuncAddress.ForcePlayAnimationStayCancel, args);

            public static int ForcePlayLoopAnimation(params dynamic[] args)
                => Call<int>(FuncAddress.ForcePlayLoopAnimation, args);

            public static int ForceSetOmissionLevel(params dynamic[] args)
                => Call<int>(FuncAddress.ForceSetOmissionLevel, args);

            public static int ForceUpdateNextFrame(params dynamic[] args)
                => Call<int>(FuncAddress.ForceUpdateNextFrame, args);

            public static int GetBountyRankPoint(params dynamic[] args)
                => Call<int>(FuncAddress.GetBountyRankPoint, args);

            public static int GetClearBonus(params dynamic[] args)
                => Call<int>(FuncAddress.GetClearBonus, args);

            public static int GetClearCount(params dynamic[] args)
                => Call<int>(FuncAddress.GetClearCount, args);

            public static int GetClearState(params dynamic[] args)
                => Call<int>(FuncAddress.GetClearState, args);

            public static int GetCurrentMapAreaNo(params dynamic[] args)
                => Call<int>(FuncAddress.GetCurrentMapAreaNo, args);

            public static int GetCurrentMapBlockNo(params dynamic[] args)
                => Call<int>(FuncAddress.GetCurrentMapBlockNo, args);

            public static int GetDeathState(params dynamic[] args)
                => Call<int>(FuncAddress.GetDeathState, args);

            public static int GetDistance(params dynamic[] args)
                => Call<int>(FuncAddress.GetDistance, args);

            public static int GetEnemyPlayerId_Random(params dynamic[] args)
                => Call<int>(FuncAddress.GetEnemyPlayerId_Random, args);

            public static int GetEventFlagValue(params dynamic[] args)
                => Call<int>(FuncAddress.GetEventFlagValue, args);

            public static int GetEventGoalState(params dynamic[] args)
                => Call<int>(FuncAddress.GetEventGoalState, args);

            public static int GetEventMode(params dynamic[] args)
                => Call<int>(FuncAddress.GetEventMode, args);

            public static int GetEventRequest(params dynamic[] args)
                => Call<int>(FuncAddress.GetEventRequest, args);

            public static int GetFloorMaterial(params dynamic[] args)
                => Call<int>(FuncAddress.GetFloorMaterial, args);

            public static int GetGlobalQWC(params dynamic[] args)
                => Call<int>(FuncAddress.GetGlobalQWC, args);

            public static int GetHeroPoint(params dynamic[] args)
                => Call<int>(FuncAddress.GetHeroPoint, args);

            public static int GetHostPlayerNo(params dynamic[] args)
                => Call<int>(FuncAddress.GetHostPlayerNo, args);

            public static int GetHp(params dynamic[] args)
                => Call<int>(FuncAddress.GetHp, args);

            public static int GetHpRate(params dynamic[] args)
                => Call<int>(FuncAddress.GetHpRate, args);

            public static int GetItem(params dynamic[] args)
                => Call<int>(FuncAddress.GetItem, args);

            public static int GetLadderCount(params dynamic[] args)
                => Call<int>(FuncAddress.GetLadderCount, args);

            public static int GetLastBlockId(params dynamic[] args)
                => Call<int>(FuncAddress.GetLastBlockId, args);

            public static int GetLocalPlayerChrType(params dynamic[] args)
                => Call<int>(FuncAddress.GetLocalPlayerChrType, args);

            public static int GetLocalPlayerId(params dynamic[] args)
                => Call<int>(FuncAddress.GetLocalPlayerId, args);

            public static int GetLocalPlayerInvadeType(params dynamic[] args)
                => Call<int>(FuncAddress.GetLocalPlayerInvadeType, args);

            public static int GetLocalPlayerSoulLv(params dynamic[] args)
                => Call<int>(FuncAddress.GetLocalPlayerSoulLv, args);

            public static int GetLocalPlayerVowType(params dynamic[] args)
                => Call<int>(FuncAddress.GetLocalPlayerVowType, args);

            public static int GetLocalQWC(params dynamic[] args)
                => Call<int>(FuncAddress.GetLocalQWC, args);

            public static int GetMultiWallNum(params dynamic[] args)
                => Call<int>(FuncAddress.GetMultiWallNum, args);

            public static int GetNetPlayerChrType(params dynamic[] args)
                => Call<int>(FuncAddress.GetNetPlayerChrType, args);

            public static int GetObjHp(params dynamic[] args)
                => Call<int>(FuncAddress.GetObjHp, args);

            public static int GetParam(params dynamic[] args)
                => Call<int>(FuncAddress.GetParam, args);

            public static int GetParam1(params dynamic[] args)
                => Call<int>(FuncAddress.GetParam1, args);

            public static int GetParam2(params dynamic[] args)
                => Call<int>(FuncAddress.GetParam2, args);

            public static int GetParam3(params dynamic[] args)
                => Call<int>(FuncAddress.GetParam3, args);

            public static int GetPartyMemberNum_InvadeType(params dynamic[] args)
                => Call<int>(FuncAddress.GetPartyMemberNum_InvadeType, args);

            public static int GetPartyMemberNum_VowType(params dynamic[] args)
                => Call<int>(FuncAddress.GetPartyMemberNum_VowType, args);

            public static int GetPlayerId_Random(params dynamic[] args)
                => Call<int>(FuncAddress.GetPlayerId_Random, args);

            public static int GetPlayerNo_LotNitoMultiItem(params dynamic[] args)
                => Call<int>(FuncAddress.GetPlayerNo_LotNitoMultiItem, args);

            public static int GetPlayID(params dynamic[] args)
                => Call<int>(FuncAddress.GetPlayID, args);

            public static int GetQWC(params dynamic[] args)
                => Call<int>(FuncAddress.GetQWC, args);

            public static int GetRandom(params dynamic[] args)
                => Call<int>(FuncAddress.GetRandom, args);

            public static int GetRateItem(params dynamic[] args)
                => Call<int>(FuncAddress.GetRateItem, args);

            public static int GetRateItem_IgnoreMultiPlay(params dynamic[] args)
                => Call<int>(FuncAddress.GetRateItem_IgnoreMultiPlay, args);

            public static int GetReturnState(params dynamic[] args)
                => Call<int>(FuncAddress.GetReturnState, args);

            public static int GetRightCurrentWeaponId(params dynamic[] args)
                => Call<int>(FuncAddress.GetRightCurrentWeaponId, args);

            public static int GetSoloClearBonus(params dynamic[] args)
                => Call<int>(FuncAddress.GetSoloClearBonus, args);

            public static int GetSummonAnimId(params dynamic[] args)
                => Call<int>(FuncAddress.GetSummonAnimId, args);

            public static int GetSummonBlackResult(params dynamic[] args)
                => Call<int>(FuncAddress.GetSummonBlackResult, args);

            public static int GetTargetChrID(params dynamic[] args)
                => Call<int>(FuncAddress.GetTargetChrID, args);

            public static int GetTempSummonParam(params dynamic[] args)
                => Call<int>(FuncAddress.GetTempSummonParam, args);

            public static int GetTravelItemParamId(params dynamic[] args)
                => Call<int>(FuncAddress.GetTravelItemParamId, args);

            public static int GetWhiteGhostCount(params dynamic[] args)
                => Call<int>(FuncAddress.GetWhiteGhostCount, args);

            public static int HasSuppleItem(params dynamic[] args)
                => Call<int>(FuncAddress.HasSuppleItem, args);

            public static int HavePartyMember(params dynamic[] args)
                => Call<int>(FuncAddress.HavePartyMember, args);

            public static int HoverMoveVal(params dynamic[] args)
                => Call<int>(FuncAddress.HoverMoveVal, args);

            public static int HoverMoveValDmy(params dynamic[] args)
                => Call<int>(FuncAddress.HoverMoveValDmy, args);

            public static int IncrementCoopPlaySuccessCount(params dynamic[] args)
                => Call<int>(FuncAddress.IncrementCoopPlaySuccessCount, args);

            public static int IncrementThiefInvadePlaySuccessCount(params dynamic[] args)
                => Call<int>(FuncAddress.IncrementThiefInvadePlaySuccessCount, args);

            public static int InfomationMenu(params dynamic[] args)
                => Call<int>(FuncAddress.InfomationMenu, args);

            public static int InitDeathState(params dynamic[] args)
                => Call<int>(FuncAddress.InitDeathState, args);

            public static int InvalidMyBloodMarkInfo(params dynamic[] args)
                => Call<int>(FuncAddress.InvalidMyBloodMarkInfo, args);

            public static int InvalidMyBloodMarkInfo_Tutorial(params dynamic[] args)
                => Call<int>(FuncAddress.InvalidMyBloodMarkInfo_Tutorial, args);

            public static int InvalidPointLight(params dynamic[] args)
                => Call<int>(FuncAddress.InvalidPointLight, args);

            public static int InvalidSfx(params dynamic[] args)
                => Call<int>(FuncAddress.InvalidSfx, args);

            public static int IsAction(params dynamic[] args)
                => Call<int>(FuncAddress.IsAction, args);

            public static int IsAlive(params dynamic[] args)
                => Call<int>(FuncAddress.IsAlive, args);

            public static int IsAliveMotion(params dynamic[] args)
                => Call<int>(FuncAddress.IsAliveMotion, args);

            public static int IsAngle(params dynamic[] args)
                => Call<int>(FuncAddress.IsAngle, args);

            public static int IsAnglePlus(params dynamic[] args)
                => Call<int>(FuncAddress.IsAnglePlus, args);

            public static int IsAppearancePlayer(params dynamic[] args)
                => Call<int>(FuncAddress.IsAppearancePlayer, args);

            public static int IsBlackGhost(params dynamic[] args)
                => Call<int>(FuncAddress.IsBlackGhost, args);

            public static int IsBlackGhost_NetPlayer(params dynamic[] args)
                => Call<int>(FuncAddress.IsBlackGhost_NetPlayer, args);

            public static int IsClearItem(params dynamic[] args)
                => Call<int>(FuncAddress.IsClearItem, args);

            public static int IsClient(params dynamic[] args)
                => Call<int>(FuncAddress.IsClient, args);

            public static int IsColiseumGhost(params dynamic[] args)
                => Call<int>(FuncAddress.IsColiseumGhost, args);

            public static int IsCompleteEvent(params dynamic[] args)
                => Call<int>(FuncAddress.IsCompleteEvent, args);

            public static int IsCompleteEventValue(params dynamic[] args)
                => Call<int>(FuncAddress.IsCompleteEventValue, args);

            public static int IsDead_NextGreyGhost(params dynamic[] args)
                => Call<int>(FuncAddress.IsDead_NextGreyGhost, args);

            public static int IsDeathPenaltySkip(params dynamic[] args)
                => Call<int>(FuncAddress.IsDeathPenaltySkip, args);

            public static int IsDestroyed(params dynamic[] args)
                => Call<int>(FuncAddress.IsDestroyed, args);

            public static int IsDisable(params dynamic[] args)
                => Call<int>(FuncAddress.IsDisable, args);

            public static int IsDistance(params dynamic[] args)
                => Call<int>(FuncAddress.IsDistance, args);

            public static int IsDropCheck_Only(params dynamic[] args)
                => Call<int>(FuncAddress.IsDropCheck_Only, args);

            public static int IsEquip(params dynamic[] args)
                => Call<int>(FuncAddress.IsEquip, args);

            public static int IsEventAnim(params dynamic[] args)
                => Call<int>(FuncAddress.IsEventAnim, args);

            public static int IsFireDead(params dynamic[] args)
                => Call<int>(FuncAddress.IsFireDead, args);

            public static int IsForceSummoned(params dynamic[] args)
                => Call<int>(FuncAddress.IsForceSummoned, args);

            public static int IsGameClient(params dynamic[] args)
                => Call<int>(FuncAddress.IsGameClient, args);

            public static int IsGreyGhost(params dynamic[] args)
                => Call<int>(FuncAddress.IsGreyGhost, args);

            public static int IsGreyGhost_NetPlayer(params dynamic[] args)
                => Call<int>(FuncAddress.IsGreyGhost_NetPlayer, args);

            public static int IsHost(params dynamic[] args)
                => Call<int>(FuncAddress.IsHost, args);

            public static int IsInParty(params dynamic[] args)
                => Call<int>(FuncAddress.IsInParty, args);

            public static int IsInParty_EnemyMember(params dynamic[] args)
                => Call<int>(FuncAddress.IsInParty_EnemyMember, args);

            public static int IsInParty_FriendMember(params dynamic[] args)
                => Call<int>(FuncAddress.IsInParty_FriendMember, args);

            public static int IsIntruder(params dynamic[] args)
                => Call<int>(FuncAddress.IsIntruder, args);

            public static int IsInventoryEquip(params dynamic[] args)
                => Call<int>(FuncAddress.IsInventoryEquip, args);

            public static int IsJobType(params dynamic[] args)
                => Call<int>(FuncAddress.IsJobType, args);

            public static int IsLand(params dynamic[] args)
                => Call<int>(FuncAddress.IsLand, args);

            public static int IsLiveNetPlayer(params dynamic[] args)
                => Call<int>(FuncAddress.IsLiveNetPlayer, args);

            public static int IsLivePlayer(params dynamic[] args)
                => Call<int>(FuncAddress.IsLivePlayer, args);

            public static int IsLoadWait(params dynamic[] args)
                => Call<int>(FuncAddress.IsLoadWait, args);

            public static int IsMatchingMultiPlay(params dynamic[] args)
                => Call<int>(FuncAddress.IsMatchingMultiPlay, args);

            public static int IsOnline(params dynamic[] args)
                => Call<int>(FuncAddress.IsOnline, args);

            public static int IsOnlineMode(params dynamic[] args)
                => Call<int>(FuncAddress.IsOnlineMode, args);

            public static int IsPlayerAssessMenu_Tutorial(params dynamic[] args)
                => Call<int>(FuncAddress.IsPlayerAssessMenu_Tutorial, args);

            public static int IsPlayerStay(params dynamic[] args)
                => Call<int>(FuncAddress.IsPlayerStay, args);

            public static int IsPlayMovie(params dynamic[] args)
                => Call<int>(FuncAddress.IsPlayMovie, args);

            public static int IsPrevGreyGhost(params dynamic[] args)
                => Call<int>(FuncAddress.IsPrevGreyGhost, args);

            public static int IsProcessEventGoal(params dynamic[] args)
                => Call<int>(FuncAddress.IsProcessEventGoal, args);

            public static int IsReady_Obj(params dynamic[] args)
                => Call<int>(FuncAddress.IsReady_Obj, args);

            public static int IsRegionDrop(params dynamic[] args)
                => Call<int>(FuncAddress.IsRegionDrop, args);

            public static int IsRegionIn(params dynamic[] args)
                => Call<int>(FuncAddress.IsRegionIn, args);

            public static int IsRevengeRequested(params dynamic[] args)
                => Call<int>(FuncAddress.IsRevengeRequested, args);

            public static int IsReviveWait(params dynamic[] args)
                => Call<int>(FuncAddress.IsReviveWait, args);

            public static int IsShow_CampMenu(params dynamic[] args)
                => Call<int>(FuncAddress.IsShow_CampMenu, args);

            public static int IsShowMenu(params dynamic[] args)
                => Call<int>(FuncAddress.IsShowMenu, args);

            public static int IsShowMenu_BriefingMsg(params dynamic[] args)
                => Call<int>(FuncAddress.IsShowMenu_BriefingMsg, args);

            public static int IsShowMenu_GenDialog(params dynamic[] args)
                => Call<int>(FuncAddress.IsShowMenu_GenDialog, args);

            public static int IsShowMenu_InfoMenu(params dynamic[] args)
                => Call<int>(FuncAddress.IsShowMenu_InfoMenu, args);

            public static int IsShowSosMsg_Tutorial(params dynamic[] args)
                => Call<int>(FuncAddress.IsShowSosMsg_Tutorial, args);

            public static int IsSuccessQWC(params dynamic[] args)
                => Call<int>(FuncAddress.IsSuccessQWC, args);

            public static int IsTryJoinSession(params dynamic[] args)
                => Call<int>(FuncAddress.IsTryJoinSession, args);

            public static int IsValidInstance(params dynamic[] args)
                => Call<int>(FuncAddress.IsValidInstance, args);

            public static int IsValidTalk(params dynamic[] args)
                => Call<int>(FuncAddress.IsValidTalk, args);

            public static int IsWhiteGhost(params dynamic[] args)
                => Call<int>(FuncAddress.IsWhiteGhost, args);

            public static int IsWhiteGhost_NetPlayer(params dynamic[] args)
                => Call<int>(FuncAddress.IsWhiteGhost_NetPlayer, args);

            public static int LeaveSession(params dynamic[] args)
                => Call<int>(FuncAddress.LeaveSession, args);

            public static int LockSession(params dynamic[] args)
                => Call<int>(FuncAddress.LockSession, args);

            public static int LuaCall(params dynamic[] args)
                => Call<int>(FuncAddress.LuaCall, args);

            public static int LuaCallStart(params dynamic[] args)
                => Call<int>(FuncAddress.LuaCallStart, args);

            public static int LuaCallStartPlus(params dynamic[] args)
                => Call<int>(FuncAddress.LuaCallStartPlus, args);

            public static int MultiDoping_AllEventBody(params dynamic[] args)
                => Call<int>(FuncAddress.MultiDoping_AllEventBody, args);

            public static int NoAnimeTurnCharactor(params dynamic[] args)
                => Call<int>(FuncAddress.NoAnimeTurnCharactor, args);

            public static int NotNetMessage_begin(params dynamic[] args)
                => Call<int>(FuncAddress.NotNetMessage_begin, args);

            public static int NotNetMessage_end(params dynamic[] args)
                => Call<int>(FuncAddress.NotNetMessage_end, args);

            public static int ObjRootMtxMove(params dynamic[] args)
                => Call<int>(FuncAddress.ObjRootMtxMove, args);

            public static int ObjRootMtxMoveByChrDmyPoly(params dynamic[] args)
                => Call<int>(FuncAddress.ObjRootMtxMoveByChrDmyPoly, args);

            public static int ObjRootMtxMoveDmyPoly(params dynamic[] args)
                => Call<int>(FuncAddress.ObjRootMtxMoveDmyPoly, args);

            public static int OnActionCheckKey(params dynamic[] args)
                => Call<int>(FuncAddress.OnActionCheckKey, args);

            public static int OnActionEventRegion(params dynamic[] args)
                => Call<int>(FuncAddress.OnActionEventRegion, args);

            public static int OnActionEventRegionAttribute(params dynamic[] args)
                => Call<int>(FuncAddress.OnActionEventRegionAttribute, args);

            public static int OnBallista(params dynamic[] args)
                => Call<int>(FuncAddress.OnBallista, args);

            public static int OnBloodMenuClose(params dynamic[] args)
                => Call<int>(FuncAddress.OnBloodMenuClose, args);

            public static int OnBonfireEvent(params dynamic[] args)
                => Call<int>(FuncAddress.OnBonfireEvent, args);

            public static int OnCharacterAnimEnd(params dynamic[] args)
                => Call<int>(FuncAddress.OnCharacterAnimEnd, args);

            public static int OnCharacterDead(params dynamic[] args)
                => Call<int>(FuncAddress.OnCharacterDead, args);

            public static int OnCharacterHP(params dynamic[] args)
                => Call<int>(FuncAddress.OnCharacterHP, args);

            public static int OnCharacterHP_CheckAttacker(params dynamic[] args)
                => Call<int>(FuncAddress.OnCharacterHP_CheckAttacker, args);

            public static int OnCharacterHpRate(params dynamic[] args)
                => Call<int>(FuncAddress.OnCharacterHpRate, args);

            public static int OnCharacterTotalDamage(params dynamic[] args)
                => Call<int>(FuncAddress.OnCharacterTotalDamage, args);

            public static int OnCharacterTotalRateDamage(params dynamic[] args)
                => Call<int>(FuncAddress.OnCharacterTotalRateDamage, args);

            public static int OnCheckEzStateMessage(params dynamic[] args)
                => Call<int>(FuncAddress.OnCheckEzStateMessage, args);

            public static int OnChrAnimEnd(params dynamic[] args)
                => Call<int>(FuncAddress.OnChrAnimEnd, args);

            public static int OnChrAnimEndPlus(params dynamic[] args)
                => Call<int>(FuncAddress.OnChrAnimEndPlus, args);

            public static int OnDistanceAction(params dynamic[] args)
                => Call<int>(FuncAddress.OnDistanceAction, args);

            public static int OnDistanceActionAttribute(params dynamic[] args)
                => Call<int>(FuncAddress.OnDistanceActionAttribute, args);

            public static int OnDistanceActionDmyPoly(params dynamic[] args)
                => Call<int>(FuncAddress.OnDistanceActionDmyPoly, args);

            public static int OnDistanceActionPlus(params dynamic[] args)
                => Call<int>(FuncAddress.OnDistanceActionPlus, args);

            public static int OnDistanceActionPlusAttribute(params dynamic[] args)
                => Call<int>(FuncAddress.OnDistanceActionPlusAttribute, args);

            public static int OnDistanceJustIn(params dynamic[] args)
                => Call<int>(FuncAddress.OnDistanceJustIn, args);

            public static int OnEndFlow(params dynamic[] args)
                => Call<int>(FuncAddress.OnEndFlow, args);

            public static int OnFireDamage(params dynamic[] args)
                => Call<int>(FuncAddress.OnFireDamage, args);

            public static int OnKeyTime2(params dynamic[] args)
                => Call<int>(FuncAddress.OnKeyTime2, args);

            public static int OnNetDistanceIn(params dynamic[] args)
                => Call<int>(FuncAddress.OnNetDistanceIn, args);

            public static int OnNetRegion(params dynamic[] args)
                => Call<int>(FuncAddress.OnNetRegion, args);

            public static int OnNetRegionAttr(params dynamic[] args)
                => Call<int>(FuncAddress.OnNetRegionAttr, args);

            public static int OnNetRegionAttrPlus(params dynamic[] args)
                => Call<int>(FuncAddress.OnNetRegionAttrPlus, args);

            public static int OnNetRegionPlus(params dynamic[] args)
                => Call<int>(FuncAddress.OnNetRegionPlus, args);

            public static int OnObjAnimEnd(params dynamic[] args)
                => Call<int>(FuncAddress.OnObjAnimEnd, args);

            public static int OnObjAnimEndPlus(params dynamic[] args)
                => Call<int>(FuncAddress.OnObjAnimEndPlus, args);

            public static int OnObjDestroy(params dynamic[] args)
                => Call<int>(FuncAddress.OnObjDestroy, args);

            public static int OnObjectDamageHit(params dynamic[] args)
                => Call<int>(FuncAddress.OnObjectDamageHit, args);

            public static int OnObjectDamageHit_NoCall(params dynamic[] args)
                => Call<int>(FuncAddress.OnObjectDamageHit_NoCall, args);

            public static int OnObjectDamageHit_NoCallPlus(params dynamic[] args)
                => Call<int>(FuncAddress.OnObjectDamageHit_NoCallPlus, args);

            public static int OnPlayerActionInRegion(params dynamic[] args)
                => Call<int>(FuncAddress.OnPlayerActionInRegion, args);

            public static int OnPlayerActionInRegionAngle(params dynamic[] args)
                => Call<int>(FuncAddress.OnPlayerActionInRegionAngle, args);

            public static int OnPlayerActionInRegionAngleAttribute(params dynamic[] args)
                => Call<int>(FuncAddress.OnPlayerActionInRegionAngleAttribute, args);

            public static int OnPlayerActionInRegionAttribute(params dynamic[] args)
                => Call<int>(FuncAddress.OnPlayerActionInRegionAttribute, args);

            public static int OnPlayerAssessMenu(params dynamic[] args)
                => Call<int>(FuncAddress.OnPlayerAssessMenu, args);

            public static int OnPlayerDistanceAngleInTarget(params dynamic[] args)
                => Call<int>(FuncAddress.OnPlayerDistanceAngleInTarget, args);

            public static int OnPlayerDistanceInTarget(params dynamic[] args)
                => Call<int>(FuncAddress.OnPlayerDistanceInTarget, args);

            public static int OnPlayerDistanceOut(params dynamic[] args)
                => Call<int>(FuncAddress.OnPlayerDistanceOut, args);

            public static int OnPlayerKill(params dynamic[] args)
                => Call<int>(FuncAddress.OnPlayerKill, args);

            public static int OnRegionIn(params dynamic[] args)
                => Call<int>(FuncAddress.OnRegionIn, args);

            public static int OnRegionJustIn(params dynamic[] args)
                => Call<int>(FuncAddress.OnRegionJustIn, args);

            public static int OnRegionJustOut(params dynamic[] args)
                => Call<int>(FuncAddress.OnRegionJustOut, args);

            public static int OnRegistFunc(params dynamic[] args)
                => Call<int>(FuncAddress.OnRegistFunc, args);

            public static int OnRequestMenuEnd(params dynamic[] args)
                => Call<int>(FuncAddress.OnRequestMenuEnd, args);

            public static int OnRevengeMenuClose(params dynamic[] args)
                => Call<int>(FuncAddress.OnRevengeMenuClose, args);

            public static int OnSelectMenu(params dynamic[] args)
                => Call<int>(FuncAddress.OnSelectMenu, args);

            public static int OnSelfBloodMark(params dynamic[] args)
                => Call<int>(FuncAddress.OnSelfBloodMark, args);

            public static int OnSelfHeroBloodMark(params dynamic[] args)
                => Call<int>(FuncAddress.OnSelfHeroBloodMark, args);

            public static int OnSessionIn(params dynamic[] args)
                => Call<int>(FuncAddress.OnSessionIn, args);

            public static int OnSessionInfo(params dynamic[] args)
                => Call<int>(FuncAddress.OnSessionInfo, args);

            public static int OnSessionJustIn(params dynamic[] args)
                => Call<int>(FuncAddress.OnSessionJustIn, args);

            public static int OnSessionJustOut(params dynamic[] args)
                => Call<int>(FuncAddress.OnSessionJustOut, args);

            public static int OnSessionOut(params dynamic[] args)
                => Call<int>(FuncAddress.OnSessionOut, args);

            public static int OnSimpleDamage(params dynamic[] args)
                => Call<int>(FuncAddress.OnSimpleDamage, args);

            public static int OnTalkEvent(params dynamic[] args)
                => Call<int>(FuncAddress.OnTalkEvent, args);

            public static int OnTalkEventAngleOut(params dynamic[] args)
                => Call<int>(FuncAddress.OnTalkEventAngleOut, args);

            public static int OnTalkEventDistIn(params dynamic[] args)
                => Call<int>(FuncAddress.OnTalkEventDistIn, args);

            public static int OnTalkEventDistOut(params dynamic[] args)
                => Call<int>(FuncAddress.OnTalkEventDistOut, args);

            public static int OnTestEffectEndPlus(params dynamic[] args)
                => Call<int>(FuncAddress.OnTestEffectEndPlus, args);

            public static int OnTextEffectEnd(params dynamic[] args)
                => Call<int>(FuncAddress.OnTextEffectEnd, args);

            public static int OnTurnCharactorEnd(params dynamic[] args)
                => Call<int>(FuncAddress.OnTurnCharactorEnd, args);

            public static int OnWanderFade(params dynamic[] args)
                => Call<int>(FuncAddress.OnWanderFade, args);

            public static int OnWanderingDemon(params dynamic[] args)
                => Call<int>(FuncAddress.OnWanderingDemon, args);

            public static int OnWarpMenuClose(params dynamic[] args)
                => Call<int>(FuncAddress.OnWarpMenuClose, args);

            public static int OnYesNoDialog(params dynamic[] args)
                => Call<int>(FuncAddress.OnYesNoDialog, args);

            public static int OpenCampMenu(params dynamic[] args)
                => Call<int>(FuncAddress.OpenCampMenu, args);

            public static int OpeningDead(params dynamic[] args)
                => Call<int>(FuncAddress.OpeningDead, args);

            public static int OpeningDeadPlus(params dynamic[] args)
                => Call<int>(FuncAddress.OpeningDeadPlus, args);

            public static int OpenSOSMsg_Tutorial(params dynamic[] args)
                => Call<int>(FuncAddress.OpenSOSMsg_Tutorial, args);

            public static int ParamInitialize(params dynamic[] args)
                => Call<int>(FuncAddress.ParamInitialize, args);

            public static int PauseTutorial(params dynamic[] args)
                => Call<int>(FuncAddress.PauseTutorial, args);

            public static int PlayAnimation(params dynamic[] args)
                => Call<int>(FuncAddress.PlayAnimation, args);

            public static int PlayAnimationStayCancel(params dynamic[] args)
                => Call<int>(FuncAddress.PlayAnimationStayCancel, args);

            public static int PlayerChrResetAnimation_RemoOnly(params dynamic[] args)
                => Call<int>(FuncAddress.PlayerChrResetAnimation_RemoOnly, args);

            public static int PlayLoopAnimation(params dynamic[] args)
                => Call<int>(FuncAddress.PlayLoopAnimation, args);

            public static int PlayObjectSE(params dynamic[] args)
                => Call<int>(FuncAddress.PlayObjectSE, args);

            public static int PlayPointSE(params dynamic[] args)
                => Call<int>(FuncAddress.PlayPointSE, args);

            public static int PlayTypeSE(params dynamic[] args)
                => Call<int>(FuncAddress.PlayTypeSE, args);

            public static int RecallMenuEvent(params dynamic[] args)
                => Call<int>(FuncAddress.RecallMenuEvent, args);

            public static int ReconstructBreak(params dynamic[] args)
                => Call<int>(FuncAddress.ReconstructBreak, args);

            public static int RecoveryHeroin(params dynamic[] args)
                => Call<int>(FuncAddress.RecoveryHeroin, args);

            public static int RegistObjact(params dynamic[] args)
                => Call<int>(FuncAddress.RegistObjact, args);

            public static int RegistSimpleTalk(params dynamic[] args)
                => Call<int>(FuncAddress.RegistSimpleTalk, args);

            public static int RemoveInventoryEquip(params dynamic[] args)
                => Call<int>(FuncAddress.RemoveInventoryEquip, args);

            public static int RepeatMessage_begin(params dynamic[] args)
                => Call<int>(FuncAddress.RepeatMessage_begin, args);

            public static int RepeatMessage_end(params dynamic[] args)
                => Call<int>(FuncAddress.RepeatMessage_end, args);

            public static int RequestEnding(params dynamic[] args)
                => Call<int>(FuncAddress.RequestEnding, args);

            public static int RequestForceUpdateNetwork(params dynamic[] args)
                => Call<int>(FuncAddress.RequestForceUpdateNetwork, args);

            public static int RequestFullRecover(params dynamic[] args)
                => Call<int>(FuncAddress.RequestFullRecover, args);

            public static int RequestGenerate(params dynamic[] args)
                => Call<int>(FuncAddress.RequestGenerate, args);

            public static int RequestNormalUpdateNetwork(params dynamic[] args)
                => Call<int>(FuncAddress.RequestNormalUpdateNetwork, args);

            public static int RequestOpenBriefingMsg(params dynamic[] args)
                => Call<int>(FuncAddress.RequestOpenBriefingMsg, args);

            public static int RequestOpenBriefingMsgPlus(params dynamic[] args)
                => Call<int>(FuncAddress.RequestOpenBriefingMsgPlus, args);

            public static int RequestPlayMovie(params dynamic[] args)
                => Call<int>(FuncAddress.RequestPlayMovie, args);

            public static int RequestPlayMoviePlus(params dynamic[] args)
                => Call<int>(FuncAddress.RequestPlayMoviePlus, args);

            public static int RequestRemo(params dynamic[] args)
                => Call<int>(FuncAddress.RequestRemo, args);

            public static int RequestRemoPlus(params dynamic[] args)
                => Call<int>(FuncAddress.RequestRemoPlus, args);

            public static int RequestUnlockTrophy(params dynamic[] args)
                => Call<int>(FuncAddress.RequestUnlockTrophy, args);

            public static int ReqularLeavePlayer(params dynamic[] args)
                => Call<int>(FuncAddress.ReqularLeavePlayer, args);

            public static int ResetCamAngle(params dynamic[] args)
                => Call<int>(FuncAddress.ResetCamAngle, args);

            public static int ResetEventQwcSpEffect(params dynamic[] args)
                => Call<int>(FuncAddress.ResetEventQwcSpEffect, args);

            public static int ResetSummonParam(params dynamic[] args)
                => Call<int>(FuncAddress.ResetSummonParam, args);

            public static int ResetSyncRideObjInfo(params dynamic[] args)
                => Call<int>(FuncAddress.ResetSyncRideObjInfo, args);

            public static int ResetThink(params dynamic[] args)
                => Call<int>(FuncAddress.ResetThink, args);

            public static int RestorePiece(params dynamic[] args)
                => Call<int>(FuncAddress.RestorePiece, args);

            public static int ReturnMapSelect(params dynamic[] args)
                => Call<int>(FuncAddress.ReturnMapSelect, args);

            public static int RevivePlayer(params dynamic[] args)
                => Call<int>(FuncAddress.RevivePlayer, args);

            public static int RevivePlayerNext(params dynamic[] args)
                => Call<int>(FuncAddress.RevivePlayerNext, args);

            public static int SaveRequest(params dynamic[] args)
                => Call<int>(FuncAddress.SaveRequest, args);

            public static int SaveRequest_Profile(params dynamic[] args)
                => Call<int>(FuncAddress.SaveRequest_Profile, args);

            public static int SendEventRequest(params dynamic[] args)
                => Call<int>(FuncAddress.SendEventRequest, args);

            public static int SetAlive(params dynamic[] args)
                => Call<int>(FuncAddress.SetAlive, args);

            public static int SetAliveMotion(params dynamic[] args)
                => Call<int>(FuncAddress.SetAliveMotion, args);

            public static int SetAlwaysDrawForEvent(params dynamic[] args)
                => Call<int>(FuncAddress.SetAlwaysDrawForEvent, args);

            public static int SetAlwaysEnableBackread_forEvent(params dynamic[] args)
                => Call<int>(FuncAddress.SetAlwaysEnableBackread_forEvent, args);

            public static int SetAngleFoward(params dynamic[] args)
                => Call<int>(FuncAddress.SetAngleFoward, args);

            public static int SetAreaStartMapUid(params dynamic[] args)
                => Call<int>(FuncAddress.SetAreaStartMapUid, args);

            public static int SetBossGauge(params dynamic[] args)
                => Call<int>(FuncAddress.SetBossGauge, args);

            public static int SetBossUnitJrHit(params dynamic[] args)
                => Call<int>(FuncAddress.SetBossUnitJrHit, args);

            public static int SetBountyRankPoint(params dynamic[] args)
                => Call<int>(FuncAddress.SetBountyRankPoint, args);

            public static int SetBrokenPiece(params dynamic[] args)
                => Call<int>(FuncAddress.SetBrokenPiece, args);

            public static int SetCamModeParamTargetId(params dynamic[] args)
                => Call<int>(FuncAddress.SetCamModeParamTargetId, args);

            public static int SetCamModeParamTargetIdForBossLock(params dynamic[] args)
                => Call<int>(FuncAddress.SetCamModeParamTargetIdForBossLock, args);

            public static int SetChrType(params dynamic[] args)
                => Call<int>(FuncAddress.SetChrType, args);

            public static int SetChrTypeDataGrey(params dynamic[] args)
                => Call<int>(FuncAddress.SetChrTypeDataGrey, args);

            public static int SetChrTypeDataGreyNext(params dynamic[] args)
                => Call<int>(FuncAddress.SetChrTypeDataGreyNext, args);

            public static int SetClearBonus(params dynamic[] args)
                => Call<int>(FuncAddress.SetClearBonus, args);

            public static int SetClearItem(params dynamic[] args)
                => Call<int>(FuncAddress.SetClearItem, args);

            public static int SetClearSesiionCount(params dynamic[] args)
                => Call<int>(FuncAddress.SetClearSesiionCount, args);

            public static int SetClearState(params dynamic[] args)
                => Call<int>(FuncAddress.SetClearState, args);

            public static int SetColiEnable(params dynamic[] args)
                => Call<int>(FuncAddress.SetColiEnable, args);

            public static int SetColiEnableArray(params dynamic[] args)
                => Call<int>(FuncAddress.SetColiEnableArray, args);

            public static int SetCompletelyNoMove(params dynamic[] args)
                => Call<int>(FuncAddress.SetCompletelyNoMove, args);

            public static int SetDeadMode(params dynamic[] args)
                => Call<int>(FuncAddress.SetDeadMode, args);

            public static int SetDeadMode2(params dynamic[] args)
                => Call<int>(FuncAddress.SetDeadMode2, args);

            public static int SetDefaultAnimation(params dynamic[] args)
                => Call<int>(FuncAddress.SetDefaultAnimation, args);

            public static int SetDefaultMapUid(params dynamic[] args)
                => Call<int>(FuncAddress.SetDefaultMapUid, args);

            public static int SetDefaultRoutePoint(params dynamic[] args)
                => Call<int>(FuncAddress.SetDefaultRoutePoint, args);

            public static int SetDisable(params dynamic[] args)
                => Call<int>(FuncAddress.SetDisable, args);

            public static int SetDisableBackread_forEvent(params dynamic[] args)
                => Call<int>(FuncAddress.SetDisableBackread_forEvent, args);

            public static int SetDisableDamage(params dynamic[] args)
                => Call<int>(FuncAddress.SetDisableDamage, args);

            public static int SetDisableGravity(params dynamic[] args)
                => Call<int>(FuncAddress.SetDisableGravity, args);

            public static int SetDisableWeakDamageAnim(params dynamic[] args)
                => Call<int>(FuncAddress.SetDisableWeakDamageAnim, args);

            public static int SetDisableWeakDamageAnim_light(params dynamic[] args)
                => Call<int>(FuncAddress.SetDisableWeakDamageAnim_light, args);

            public static int SetDispMask(params dynamic[] args)
                => Call<int>(FuncAddress.SetDispMask, args);

            public static int SetDrawEnable(params dynamic[] args)
                => Call<int>(FuncAddress.SetDrawEnable, args);

            public static int SetDrawEnableArray(params dynamic[] args)
                => Call<int>(FuncAddress.SetDrawEnableArray, args);

            public static int SetDrawGroup(params dynamic[] args)
                => Call<int>(FuncAddress.SetDrawGroup, args);

            public static int SetEnableEventPad(params dynamic[] args)
                => Call<int>(FuncAddress.SetEnableEventPad, args);

            public static int SetEventBodyBulletCorrect(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventBodyBulletCorrect, args);

            public static int SetEventBodyMaterialSeAndSfx(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventBodyMaterialSeAndSfx, args);

            public static int SetEventBodyMaxHp(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventBodyMaxHp, args);

            public static int SetEventCommand(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventCommand, args);

            public static int SetEventCommandIndex(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventCommandIndex, args);

            public static int SetEventFlag(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventFlag, args);

            public static int SetEventFlagValue(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventFlagValue, args);

            public static int SetEventGenerate(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventGenerate, args);

            public static int SetEventMovePointType(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventMovePointType, args);

            public static int SetEventSimpleTalk(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventSimpleTalk, args);

            public static int SetEventSpecialEffect(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventSpecialEffect, args);

            public static int SetEventSpecialEffect_2(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventSpecialEffect_2, args);

            public static int SetEventSpecialEffectOwner(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventSpecialEffectOwner, args);

            public static int SetEventSpecialEffectOwner_2(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventSpecialEffectOwner_2, args);

            public static int SetEventTarget(params dynamic[] args)
                => Call<int>(FuncAddress.SetEventTarget, args);

            public static int SetExVelocity(params dynamic[] args)
                => Call<int>(FuncAddress.SetExVelocity, args);

            public static int SetFirstSpeed(params dynamic[] args)
                => Call<int>(FuncAddress.SetFirstSpeed, args);

            public static int SetFirstSpeedPlus(params dynamic[] args)
                => Call<int>(FuncAddress.SetFirstSpeedPlus, args);

            public static int SetFlagInitState(params dynamic[] args)
                => Call<int>(FuncAddress.SetFlagInitState, args);

            public static int SetFootIKInterpolateType(params dynamic[] args)
                => Call<int>(FuncAddress.SetFootIKInterpolateType, args);

            public static int SetForceJoinBlackRequest(params dynamic[] args)
                => Call<int>(FuncAddress.SetForceJoinBlackRequest, args);

            public static int SetHitInfo(params dynamic[] args)
                => Call<int>(FuncAddress.SetHitInfo, args);

            public static int SetHitMask(params dynamic[] args)
                => Call<int>(FuncAddress.SetHitMask, args);

            public static int SetHp(params dynamic[] args)
                => Call<int>(FuncAddress.SetHp, args);

            public static int SetIgnoreHit(params dynamic[] args)
                => Call<int>(FuncAddress.SetIgnoreHit, args);

            public static int SetInfomationPriority(params dynamic[] args)
                => Call<int>(FuncAddress.SetInfomationPriority, args);

            public static int SetInsideBattleArea(params dynamic[] args)
                => Call<int>(FuncAddress.SetInsideBattleArea, args);

            public static int SetIsAnimPauseOnRemoPlayForEvent(params dynamic[] args)
                => Call<int>(FuncAddress.SetIsAnimPauseOnRemoPlayForEvent, args);

            public static int SetKeepCommandIndex(params dynamic[] args)
                => Call<int>(FuncAddress.SetKeepCommandIndex, args);

            public static int SetLoadWait(params dynamic[] args)
                => Call<int>(FuncAddress.SetLoadWait, args);

            public static int SetLockActPntInvalidateMask(params dynamic[] args)
                => Call<int>(FuncAddress.SetLockActPntInvalidateMask, args);

            public static int SetMapUid(params dynamic[] args)
                => Call<int>(FuncAddress.SetMapUid, args);

            public static int SetMaxHp(params dynamic[] args)
                => Call<int>(FuncAddress.SetMaxHp, args);

            public static int SetMenuBrake(params dynamic[] args)
                => Call<int>(FuncAddress.SetMenuBrake, args);

            public static int SetMiniBlockIndex(params dynamic[] args)
                => Call<int>(FuncAddress.SetMiniBlockIndex, args);

            public static int SetMovePoint(params dynamic[] args)
                => Call<int>(FuncAddress.SetMovePoint, args);

            public static int SetMultiWallMapUid(params dynamic[] args)
                => Call<int>(FuncAddress.SetMultiWallMapUid, args);

            public static int SetNoNetSync(params dynamic[] args)
                => Call<int>(FuncAddress.SetNoNetSync, args);

            public static int SetObjDeactivate(params dynamic[] args)
                => Call<int>(FuncAddress.SetObjDeactivate, args);

            public static int SetObjDisableBreak(params dynamic[] args)
                => Call<int>(FuncAddress.SetObjDisableBreak, args);

            public static int SetObjEventCollisionFill(params dynamic[] args)
                => Call<int>(FuncAddress.SetObjEventCollisionFill, args);

            public static int SetObjSfx(params dynamic[] args)
                => Call<int>(FuncAddress.SetObjSfx, args);

            public static int SetReturnPointEntityId(params dynamic[] args)
                => Call<int>(FuncAddress.SetReturnPointEntityId, args);

            public static int SetReviveWait(params dynamic[] args)
                => Call<int>(FuncAddress.SetReviveWait, args);

            public static int SetSelfBloodMapUid(params dynamic[] args)
                => Call<int>(FuncAddress.SetSelfBloodMapUid, args);

            public static int SetSosSignPos(params dynamic[] args)
                => Call<int>(FuncAddress.SetSosSignPos, args);

            public static int SetSosSignWarp(params dynamic[] args)
                => Call<int>(FuncAddress.SetSosSignWarp, args);

            public static int SetSpStayAndDamageAnimId(params dynamic[] args)
                => Call<int>(FuncAddress.SetSpStayAndDamageAnimId, args);

            public static int SetSpStayAndDamageAnimIdPlus(params dynamic[] args)
                => Call<int>(FuncAddress.SetSpStayAndDamageAnimIdPlus, args);

            public static int SetSubMenuBrake(params dynamic[] args)
                => Call<int>(FuncAddress.SetSubMenuBrake, args);

            public static int SetSummonedPos(params dynamic[] args)
                => Call<int>(FuncAddress.SetSummonedPos, args);

            public static int SetSyncRideObjInfo(params dynamic[] args)
                => Call<int>(FuncAddress.SetSyncRideObjInfo, args);

            public static int SetSystemIgnore(params dynamic[] args)
                => Call<int>(FuncAddress.SetSystemIgnore, args);

            public static int SetTalkMsg(params dynamic[] args)
                => Call<int>(FuncAddress.SetTalkMsg, args);

            public static int SetTeamType(params dynamic[] args)
                => Call<int>(FuncAddress.SetTeamType, args);

            public static int SetTeamTypeDefault(params dynamic[] args)
                => Call<int>(FuncAddress.SetTeamTypeDefault, args);

            public static int SetTeamTypePlus(params dynamic[] args)
                => Call<int>(FuncAddress.SetTeamTypePlus, args);

            public static int SetTextEffect(params dynamic[] args)
                => Call<int>(FuncAddress.SetTextEffect, args);

            public static int SetTutorialSummonedPos(params dynamic[] args)
                => Call<int>(FuncAddress.SetTutorialSummonedPos, args);

            public static int SetValidTalk(params dynamic[] args)
                => Call<int>(FuncAddress.SetValidTalk, args);

            public static int ShowGenDialog(params dynamic[] args)
                => Call<int>(FuncAddress.ShowGenDialog, args);

            public static int ShowRankingDialog(params dynamic[] args)
                => Call<int>(FuncAddress.ShowRankingDialog, args);

            public static int SOSMsgGetResult_Tutorial(params dynamic[] args)
                => Call<int>(FuncAddress.SOSMsgGetResult_Tutorial, args);

            public static int StopLoopAnimation(params dynamic[] args)
                => Call<int>(FuncAddress.StopLoopAnimation, args);

            public static int StopPlayer(params dynamic[] args)
                => Call<int>(FuncAddress.StopPlayer, args);

            public static int StopPointSE(params dynamic[] args)
                => Call<int>(FuncAddress.StopPointSE, args);

            public static int SubActionCount(params dynamic[] args)
                => Call<int>(FuncAddress.SubActionCount, args);

            public static int SubDispMaskByBit(params dynamic[] args)
                => Call<int>(FuncAddress.SubDispMaskByBit, args);

            public static int SubHitMask(params dynamic[] args)
                => Call<int>(FuncAddress.SubHitMask, args);

            public static int SubHitMaskByBit(params dynamic[] args)
                => Call<int>(FuncAddress.SubHitMaskByBit, args);

            public static int SummonBlackRequest(params dynamic[] args)
                => Call<int>(FuncAddress.SummonBlackRequest, args);

            public static int SummonedMapReload(params dynamic[] args)
                => Call<int>(FuncAddress.SummonedMapReload, args);

            public static int SummonSuccess(params dynamic[] args)
                => Call<int>(FuncAddress.SummonSuccess, args);

            public static int SwitchDispMask(params dynamic[] args)
                => Call<int>(FuncAddress.SwitchDispMask, args);

            public static int SwitchHitMask(params dynamic[] args)
                => Call<int>(FuncAddress.SwitchHitMask, args);

            public static int TalkNextPage(params dynamic[] args)
                => Call<int>(FuncAddress.TalkNextPage, args);

            public static int TreasureDispModeChange(params dynamic[] args)
                => Call<int>(FuncAddress.TreasureDispModeChange, args);

            public static int TreasureDispModeChange2(params dynamic[] args)
                => Call<int>(FuncAddress.TreasureDispModeChange2, args);

            public static int TurnCharactor(params dynamic[] args)
                => Call<int>(FuncAddress.TurnCharactor, args);

            public static int Tutorial_begin(params dynamic[] args)
                => Call<int>(FuncAddress.Tutorial_begin, args);

            public static int Tutorial_end(params dynamic[] args)
                => Call<int>(FuncAddress.Tutorial_end, args);

            public static int UnLockSession(params dynamic[] args)
                => Call<int>(FuncAddress.UnLockSession, args);

            public static int UpDateBloodMark(params dynamic[] args)
                => Call<int>(FuncAddress.UpDateBloodMark, args);

            public static int Util_RequestLevelUp(params dynamic[] args)
                => Call<int>(FuncAddress.Util_RequestLevelUp, args);

            public static int Util_RequestLevelUpFirst(params dynamic[] args)
                => Call<int>(FuncAddress.Util_RequestLevelUpFirst, args);

            public static int Util_RequestRegene(params dynamic[] args)
                => Call<int>(FuncAddress.Util_RequestRegene, args);

            public static int Util_RequestRespawn(params dynamic[] args)
                => Call<int>(FuncAddress.Util_RequestRespawn, args);

            public static int ValidPointLight(params dynamic[] args)
                => Call<int>(FuncAddress.ValidPointLight, args);

            public static int ValidSfx(params dynamic[] args)
                => Call<int>(FuncAddress.ValidSfx, args);

            public static int VariableExpand_211_Param1(params dynamic[] args)
                => Call<int>(FuncAddress.VariableExpand_211_Param1, args);

            public static int VariableExpand_211_param2(params dynamic[] args)
                => Call<int>(FuncAddress.VariableExpand_211_param2, args);

            public static int VariableExpand_211_param3(params dynamic[] args)
                => Call<int>(FuncAddress.VariableExpand_211_param3, args);

            public static int VariableExpand_22_param1(params dynamic[] args)
                => Call<int>(FuncAddress.VariableExpand_22_param1, args);

            public static int VariableExpand_22_param2(params dynamic[] args)
                => Call<int>(FuncAddress.VariableExpand_22_param2, args);

            public static int VariableOrder_211(params dynamic[] args)
                => Call<int>(FuncAddress.VariableOrder_211, args);

            public static int VariableOrder_22(params dynamic[] args)
                => Call<int>(FuncAddress.VariableOrder_22, args);

            public static int WARN(params dynamic[] args)
                => Call<int>(FuncAddress.WARN, args);

            public static int Warp(params dynamic[] args)
                => Call<int>(FuncAddress.Warp, args);

            public static int WarpDmy(params dynamic[] args)
                => Call<int>(FuncAddress.WarpDmy, args);

            public static int WarpNextStage(params dynamic[] args)
                => Call<int>(FuncAddress.WarpNextStage, args);

            public static int WarpNextStage_Bonfire(params dynamic[] args)
                => Call<int>(FuncAddress.WarpNextStage_Bonfire, args);

            public static int WarpNextStageKick(params dynamic[] args)
                => Call<int>(FuncAddress.WarpNextStageKick, args);

            public static int WarpRestart(params dynamic[] args)
                => Call<int>(FuncAddress.WarpRestart, args);

            public static int WarpRestartNoGrey(params dynamic[] args)
                => Call<int>(FuncAddress.WarpRestartNoGrey, args);

            public static int WarpSelfBloodMark(params dynamic[] args)
                => Call<int>(FuncAddress.WarpSelfBloodMark, args);


        }
    }
}
