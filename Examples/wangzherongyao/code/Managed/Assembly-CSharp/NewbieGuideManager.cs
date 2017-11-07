using Assets.Scripts.Common;
using Assets.Scripts.GameLogic;
using Assets.Scripts.GameSystem;
using CSProtocol;
using ResData;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[MessageHandlerClass]
public class NewbieGuideManager : MonoSingleton<NewbieGuideManager>
{
    public static NewbieGuideScriptControl.AddScriptDelegate addScriptDelegate;
    public bool bTimeOutSkip;
    public static string FORM_3v3GUIDE_CONFIRM = "UGUI/Form/System/Newbie/Form_3v3Guide_Confirm.prefab";
    public static string FORM_5v5GUIDE_CONFIRM = "UGUI/Form/System/Newbie/Form_5v5Guide_Confirm.prefab";
    private int hostDeadNum;
    private bool m_IsCheckSkip;
    private Dictionary<uint, bool> mCompleteCacheDic;
    private uint mCurrentNewbieGuideId;
    private NewbieGuideScriptControl mCurrentScriptControl;
    private bool mIsInit;
    private List<uint> mSingleBattleCompleteCacheDic;
    private Dictionary<uint, bool> mWeakCompleteCacheDic;
    public bool newbieGuideEnable;
    public const int SPECIAL_CUT_SCENE_GUIDE = 1;
    public static short WEAKGUIDE_BIT_OFFSET = 110;

    private void AddSingleBattleCompleteID(uint id)
    {
        this.mSingleBattleCompleteCacheDic.Add(id);
    }

    protected override void Awake()
    {
        base.Awake();
        addScriptDelegate = new NewbieGuideScriptControl.AddScriptDelegate(NewbieGuideSctiptFactory.AddScript);
        DebugHelper.Assert(addScriptDelegate != null);
        this.mCompleteCacheDic = new Dictionary<uint, bool>();
        this.mWeakCompleteCacheDic = new Dictionary<uint, bool>();
        this.mSingleBattleCompleteCacheDic = new List<uint>();
        List<uint> mainLineIDList = Singleton<NewbieGuideDataManager>.GetInstance().GetMainLineIDList();
        int count = mainLineIDList.Count;
        for (int i = 0; i < count; i++)
        {
            this.mCompleteCacheDic.Add(mainLineIDList[i], false);
        }
        List<uint> weakMianLineIDList = Singleton<NewbieGuideDataManager>.GetInstance().GetWeakMianLineIDList();
        count = weakMianLineIDList.Count;
        for (int j = 0; j < count; j++)
        {
            this.mWeakCompleteCacheDic.Add(weakMianLineIDList[j], false);
        }
        this.newbieGuideEnable = true;
        this.bTimeOutSkip = true;
        this.Initialize();
    }

    private void CheckForceSkipCondition(NewbieGuideSkipConditionType type, params uint[] param)
    {
        ListView<NewbieGuideMainLineConf> newbieGuideMainLineConfListBySkipType = Singleton<NewbieGuideDataManager>.GetInstance().GetNewbieGuideMainLineConfListBySkipType(type);
        int count = newbieGuideMainLineConfListBySkipType.Count;
        for (int i = 0; i < count; i++)
        {
            NewbieGuideMainLineConf conf = newbieGuideMainLineConfListBySkipType[i];
            if (!this.IsMianLineComplete(conf.dwID))
            {
                for (int j = 0; j < conf.astSkipCondition.Length; j++)
                {
                    if ((((NewbieGuideSkipConditionType) conf.astSkipCondition[j].wType) == type) && NewbieGuideCheckSkipConditionUtil.CheckSkipCondition(conf.astSkipCondition[j], param))
                    {
                        if (null != this.mCurrentScriptControl)
                        {
                            if (this.mCurrentScriptControl.currentMainLineId == conf.dwID)
                            {
                                continue;
                            }
                            this.SetNewbieGuideComplete(conf.dwID, false, false);
                        }
                        else
                        {
                            this.SetNewbieGuideComplete(conf.dwID, false, false);
                        }
                        break;
                    }
                }
            }
        }
    }

    private bool CheckLevelLimit(NewbieWeakGuideMainLineConf conf)
    {
        if (conf.wTriggerLevelUpperLimit != 0)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                return (masterRoleInfo.PvpLevel <= conf.wTriggerLevelUpperLimit);
            }
        }
        return true;
    }

    private bool CheckLevelLimitLower(NewbieWeakGuideMainLineConf conf)
    {
        if (conf.wTriggerLevelLowerLimit != 0)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                return (masterRoleInfo.PvpLevel >= conf.wTriggerLevelLowerLimit);
            }
        }
        return true;
    }

    public void CheckScriptComplete(NewbieGuideScriptType type, params uint[] param)
    {
        if (this.newbieGuideEnable && ((this.currentNewbieGuideId != 0) && (null != this.mCurrentScriptControl)))
        {
            this.mCurrentScriptControl.CheckSriptComplete(type, param);
        }
    }

    public void CheckSkipCondition(NewbieGuideSkipConditionType type, params uint[] param)
    {
        this.CheckForceSkipCondition(type, param);
        this.CheckWeakSkipCondition(type, param);
    }

    public void CheckSkipIntoLobby()
    {
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasComplete33Guide, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasRewardTaskPvp, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasRewardTaskPve, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteHeroAdv, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteHeroStar, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasBoughtHero, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasGotChapterReward, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasAdvancedEquip, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasMopup, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasEnteredPvP, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasEnteredTrial, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasEnteredZhuangzi, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasEnteredBurning, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasEnteredElitePvE, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasEnteredGuild, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasUsedSymbol, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasEnteredMysteryShop, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteNewbieArchive, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteBaseGuide, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteHumanAi33Match, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteHuman33Match, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteHumanAi33, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasManufacuredSymbol, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasEnoughSymbolOf, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteLottery, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasIncreaseEquip, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteHeroUp, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasEnteredTournament, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasOverThreeHeroes, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteWeakNewbieArchive, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteDungeon, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteNewbieGuide, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteTrainLevel55, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasComplete11Match, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteTrainLevel33, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasDiamondDraw, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteFirst55Warm, new uint[0]);
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteCoronaGuide, new uint[0]);
        this.m_IsCheckSkip = true;
    }

    private bool CheckTrigger(NewbieGuideTriggerTimeType type, NewbieGuideMainLineConf conf)
    {
        if (this.CheckTriggerTime(conf) && this.CheckTriggerCondition(conf.dwID, conf.astTriggerCondition))
        {
            int startIndexByTriggerTime = this.GetStartIndexByTriggerTime(type, conf);
            this.TriggerNewbieGuide(conf, startIndexByTriggerTime);
            return true;
        }
        return false;
    }

    private bool CheckTriggerCondition(uint id, NewbieGuideTriggerConditionItem[] conditions)
    {
        int length = conditions.Length;
        for (int i = 0; i < length; i++)
        {
            NewbieGuideTriggerConditionItem condition = conditions[i];
            if ((condition.wType != 0) && !NewbieGuideCheckTriggerConditionUtil.CheckTriggerCondition(id, condition))
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckTriggerTime(NewbieGuideMainLineConf conf)
    {
        if (this.IsMianLineComplete(conf.dwID))
        {
            return false;
        }
        bool flag = true;
        CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
        if (masterRoleInfo != null)
        {
            if (conf.wTriggerLevelUpperLimit > 0)
            {
                flag &= masterRoleInfo.PvpLevel <= conf.wTriggerLevelUpperLimit;
            }
            if (conf.wTriggerLevelLowerLimit > 0)
            {
                flag &= masterRoleInfo.PvpLevel >= conf.wTriggerLevelLowerLimit;
            }
        }
        return flag;
    }

    private bool CheckTriggerTime(NewbieWeakGuideMainLineConf conf)
    {
        return (!this.IsWeakLineComplete(conf.dwID) && (this.CheckLevelLimit(conf) && this.CheckLevelLimitLower(conf)));
    }

    public bool CheckTriggerTime(NewbieGuideTriggerTimeType type, params uint[] param)
    {
        if (this.newbieGuideEnable)
        {
            if (!this.m_IsCheckSkip)
            {
                return false;
            }
            if (this.currentNewbieGuideId == 0)
            {
                ListView<NewbieGuideMainLineConf> newbieGuideMainLineConfListByTriggerTimeType = Singleton<NewbieGuideDataManager>.GetInstance().GetNewbieGuideMainLineConfListByTriggerTimeType(type, param);
                int count = newbieGuideMainLineConfListByTriggerTimeType.Count;
                for (int i = 0; i < count; i++)
                {
                    NewbieGuideMainLineConf conf = newbieGuideMainLineConfListByTriggerTimeType[i];
                    if (this.CheckTrigger(type, conf))
                    {
                        if (Singleton<NewbieWeakGuideControl>.instance.isGuiding)
                        {
                            Singleton<NewbieWeakGuideControl>.instance.RemoveAllEffect();
                        }
                        return true;
                    }
                }
                ListView<NewbieWeakGuideMainLineConf> newBieGuideWeakMainLineConfListByTiggerTimeType = Singleton<NewbieGuideDataManager>.GetInstance().GetNewBieGuideWeakMainLineConfListByTiggerTimeType(type, param);
                count = newBieGuideWeakMainLineConfListByTiggerTimeType.Count;
                for (int j = 0; j < count; j++)
                {
                    NewbieWeakGuideMainLineConf conf2 = newBieGuideWeakMainLineConfListByTiggerTimeType[j];
                    if (this.TriggerWeakNewbieGuide(conf2, type, true))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void CheckWeakGuideTrigger(NewbieGuideWeakGuideType type, params uint[] param)
    {
    }

    private void CheckWeakSkipCondition(NewbieGuideSkipConditionType type, params uint[] param)
    {
        ListView<NewbieWeakGuideMainLineConf> newbieGuideWeakMianLineConfListBySkipType = Singleton<NewbieGuideDataManager>.GetInstance().GetNewbieGuideWeakMianLineConfListBySkipType(type);
        int count = newbieGuideWeakMianLineConfListBySkipType.Count;
        for (int i = 0; i < count; i++)
        {
            NewbieWeakGuideMainLineConf conf = newbieGuideWeakMianLineConfListBySkipType[i];
            if (!this.IsWeakLineComplete(conf.dwID))
            {
                for (int j = 0; j < conf.astSkipCondition.Length; j++)
                {
                    NewbieGuideSkipConditionItem item = conf.astSkipCondition[j];
                    if ((((NewbieGuideSkipConditionType) item.wType) == type) && NewbieGuideCheckSkipConditionUtil.CheckSkipCondition(item, param))
                    {
                        this.SetWeakGuideComplete(conf.dwID, true);
                        break;
                    }
                }
            }
        }
    }

    public void ClearSingleBattleCache()
    {
        this.mSingleBattleCompleteCacheDic.Clear();
    }

    private void DestroyCurrentScriptControl()
    {
        this.mCurrentScriptControl.CompleteEvent -= new NewbieGuideScriptControl.NewbieGuideScriptControlDelegate(this.NewbieGuideCompleteHandler);
        this.mCurrentScriptControl.SaveEvent -= new NewbieGuideScriptControl.NewbieGuideScriptControlDelegate(this.NewbieGuideSaveHandler);
        this.mCurrentScriptControl.addScriptDelegate = null;
        UnityEngine.Object.Destroy(this.mCurrentScriptControl);
        this.mCurrentScriptControl = null;
        this.mCurrentNewbieGuideId = 0;
    }

    public void ForceCompleteNewbieGuide()
    {
        if ((this.mCompleteCacheDic.ContainsKey(this.mCurrentNewbieGuideId) && !this.mCompleteCacheDic[this.mCurrentNewbieGuideId]) && (Singleton<NewbieGuideDataManager>.GetInstance().GetNewbieGuideMainLineConf(this.mCurrentNewbieGuideId) != null))
        {
            this.SetNewbieGuideComplete(this.mCurrentNewbieGuideId, true, false);
        }
        if (null != this.mCurrentScriptControl)
        {
            this.mCurrentScriptControl.Stop();
            this.DestroyCurrentScriptControl();
        }
    }

    public void ForceCompleteNewbieGuideAll(bool bReset)
    {
        if (null != this.mCurrentScriptControl)
        {
            this.mCurrentScriptControl.Stop();
            this.DestroyCurrentScriptControl();
        }
        List<uint> list = new List<uint>();
        Dictionary<uint, bool>.KeyCollection.Enumerator enumerator = this.mCompleteCacheDic.Keys.GetEnumerator();
        while (enumerator.MoveNext())
        {
            uint current = enumerator.Current;
            if (!this.mCompleteCacheDic[current])
            {
                NewbieGuideMainLineConf newbieGuideMainLineConf = Singleton<NewbieGuideDataManager>.GetInstance().GetNewbieGuideMainLineConf(current);
                if ((newbieGuideMainLineConf != null) && (newbieGuideMainLineConf.dwOldPlayerGuide == 0))
                {
                    list.Add(current);
                }
            }
        }
        List<uint>.Enumerator enumerator2 = list.GetEnumerator();
        while (enumerator2.MoveNext())
        {
            this.SetNewbieGuideComplete(enumerator2.Current, bReset, false);
        }
    }

    public void ForceCompleteWeakGuide()
    {
        if (this.newbieGuideEnable)
        {
            Singleton<NewbieWeakGuideControl>.GetInstance().ForceCompleteWeakGuide();
        }
    }

    public void ForceSetWeakGuideCompleteAll(bool bReset)
    {
        List<uint> list = new List<uint>();
        Dictionary<uint, bool>.KeyCollection.Enumerator enumerator = this.mWeakCompleteCacheDic.Keys.GetEnumerator();
        while (enumerator.MoveNext())
        {
            uint current = enumerator.Current;
            if (!this.mWeakCompleteCacheDic[current])
            {
                NewbieWeakGuideMainLineConf newbieWeakGuideMainLineConf = Singleton<NewbieGuideDataManager>.GetInstance().GetNewbieWeakGuideMainLineConf(current);
                if (((newbieWeakGuideMainLineConf != null) && (newbieWeakGuideMainLineConf.bOnlyOnce == 1)) && (newbieWeakGuideMainLineConf.dwOldPlayerGuide == 0))
                {
                    list.Add(newbieWeakGuideMainLineConf.dwID);
                }
            }
        }
        for (int i = 0; i < list.Count; i++)
        {
            MonoSingleton<NewbieGuideManager>.GetInstance().SetWeakGuideComplete(list[i], bReset);
        }
    }

    private int GetStartIndexByTriggerTime(NewbieGuideTriggerTimeType type, NewbieGuideMainLineConf conf)
    {
        int length = conf.astTriggerTime.Length;
        for (int i = 0; i < length; i++)
        {
            NewbieGuideTriggerTimeItem item = conf.astTriggerTime[i];
            if (type == ((NewbieGuideTriggerTimeType) item.wType))
            {
                return (int) item.dwStartIndex;
            }
        }
        return 0;
    }

    private uint GetWeakStartIndexByMianLineConf(NewbieGuideTriggerTimeType type, NewbieWeakGuideMainLineConf conf)
    {
        uint dwStartIndex = 1;
        int length = conf.astTriggerTime.Length;
        for (int i = 0; i < length; i++)
        {
            NewbieGuideTriggerTimeItem item = conf.astTriggerTime[i];
            if ((((NewbieGuideTriggerTimeType) item.wType) == type) && (item.dwStartIndex > 0))
            {
                dwStartIndex = item.dwStartIndex;
            }
        }
        return dwStartIndex;
    }

    public bool HasSingleBattleComplete(uint id)
    {
        return this.mSingleBattleCompleteCacheDic.Contains(id);
    }

    protected override void Init()
    {
        base.Init();
        Singleton<GameEventSys>.instance.AddEventHandler<TalentLevelChangeParam>(GameEventDef.Event_TalentLevelChange, new RefAction<TalentLevelChangeParam>(this.onTalentLevelChange));
        if (!this.IsBigMapSignGuideCompelte())
        {
            Singleton<GameEventSys>.GetInstance().AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_FightPrepare, new RefAction<DefaultGameEventParam>(this.OnFightPrepare));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.onActorDead));
        }
    }

    public void Initialize()
    {
        Singleton<NewbieWeakGuideControl>.CreateInstance();
        this.mIsInit = true;
    }

    private bool IsBigMapSignGuideCompelte()
    {
        bool flag = false;
        CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
        if (((masterRoleInfo != null) && masterRoleInfo.IsNewbieAchieveSet(0x4c)) && masterRoleInfo.IsNewbieAchieveSet(0x4d))
        {
            flag = true;
        }
        return flag;
    }

    private bool IsMianLineComplete(uint id)
    {
        bool flag;
        return (this.mCompleteCacheDic.TryGetValue(id, out flag) && flag);
    }

    public bool IsNewbieGuideComplete(uint id)
    {
        bool flag;
        if (this.mCompleteCacheDic.TryGetValue(id, out flag))
        {
            return flag;
        }
        CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
        return ((masterRoleInfo != null) && masterRoleInfo.IsNewbieAchieveSet((int) id));
    }

    public bool IsWeakLineComplete(uint lineId)
    {
        bool flag;
        if (this.mWeakCompleteCacheDic.TryGetValue(lineId, out flag))
        {
            return flag;
        }
        CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
        return ((masterRoleInfo != null) && masterRoleInfo.IsNewbieAchieveSet((int) (lineId + WEAKGUIDE_BIT_OFFSET)));
    }

    public bool NeedBattlePause()
    {
        if (null != this.mCurrentScriptControl)
        {
            NewbieGuideBaseScript currentScript = this.mCurrentScriptControl.currentScript;
            if (((null != currentScript) && (currentScript.currentConf != null)) && ((currentScript.currentConf.bStopGame == 2) && currentScript.isGuiding))
            {
                return true;
            }
        }
        return false;
    }

    private void NewbieGuideCompleteHandler()
    {
        if (null != this.mCurrentScriptControl)
        {
            uint currentNewbieGuideId = this.currentNewbieGuideId;
            this.SetNewbieGuideComplete(this.currentNewbieGuideId, true, true);
            Singleton<GameRecord>.GetInstance().RecordNewbieGuide(currentNewbieGuideId, true);
            this.AddSingleBattleCompleteID(currentNewbieGuideId);
            this.DestroyCurrentScriptControl();
            uint[] param = new uint[] { currentNewbieGuideId };
            if (this.CheckTriggerTime(NewbieGuideTriggerTimeType.preNewbieGuideComplete, param))
            {
            }
        }
    }

    private void NewbieGuideSaveHandler()
    {
        this.Save();
    }

    private void onActorDead(ref DefaultGameEventParam param)
    {
        if ((param.src != 0) && ActorHelper.IsHostCtrlActor(ref param.src))
        {
            this.hostDeadNum++;
            if ((this.hostDeadNum == 1) && (Singleton<BattleLogic>.GetInstance().GetCurLvelContext().iLevelID == 0x4e2b))
            {
                this.CheckTriggerTime(NewbieGuideTriggerTimeType.onHostFirstDead, new uint[0]);
            }
        }
    }

    protected override void OnDestroy()
    {
        Singleton<GameEventSys>.instance.RmvEventHandler<TalentLevelChangeParam>(GameEventDef.Event_TalentLevelChange, new RefAction<TalentLevelChangeParam>(this.onTalentLevelChange));
        Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_FightPrepare, new RefAction<DefaultGameEventParam>(this.OnFightPrepare));
        Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.onActorDead));
        Singleton<NewbieWeakGuideControl>.DestroyInstance();
        base.OnDestroy();
    }

    private void OnFightPrepare(ref DefaultGameEventParam param)
    {
        this.hostDeadNum = 0;
    }

    public void OnGetNewbieClntBits()
    {
    }

    [MessageHandler(0x4a8)]
    public static void OnNTFNewieAllBitSyn(CSPkg msg)
    {
        SCPKG_NTF_NEWIEALLBITSYN stNewieAllBitSyn = msg.stPkgData.stNewieAllBitSyn;
        CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
        if (masterRoleInfo != null)
        {
            masterRoleInfo.InitGuidedStateBits(stNewieAllBitSyn.stNewbieBits);
        }
    }

    private void onTalentLevelChange(ref TalentLevelChangeParam inParam)
    {
        if (Singleton<GamePlayerCenter>.instance.GetHostPlayer() != null)
        {
            PoolObjHandle<ActorRoot> captain = Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain;
            if ((captain != 0) && ((inParam.src != 0) && (inParam.src == captain)))
            {
                uint[] param = new uint[] { inParam.SoulLevel, inParam.TalentLevel };
                MonoSingleton<NewbieGuideManager>.GetInstance().CheckTriggerTime(NewbieGuideTriggerTimeType.onTalentLevelChange, param);
            }
        }
    }

    public void Pause()
    {
        if (this.currentNewbieGuideId != 0)
        {
        }
    }

    public void Resume()
    {
        if (this.currentNewbieGuideId != 0)
        {
        }
    }

    private void Save()
    {
        if ((this.mIsInit && (null != this.mCurrentScriptControl)) && this.mCurrentScriptControl.CheckSavePoint())
        {
            this.SetNewbieGuideComplete(this.mCurrentNewbieGuideId, false, false);
        }
    }

    private void SetCurrentNewbieGuideId(uint id)
    {
        this.mCurrentNewbieGuideId = id;
    }

    public void SetNewbieBit(int index, bool bOpen)
    {
        CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
        DebugHelper.Assert(masterRoleInfo != null, "Master Roleinfo is NULL!");
        if (masterRoleInfo != null)
        {
            masterRoleInfo.SetNewbieAchieve(index, bOpen, false);
        }
    }

    public void SetNewbieGuideComplete(uint id, bool reset, bool bNormalComplete = false)
    {
        bool flag = this.mCompleteCacheDic[id];
        if (!bNormalComplete)
        {
            this.mCompleteCacheDic[id] = true;
        }
        else if ((this.mCurrentScriptControl == null) || (this.mCurrentScriptControl.savePoint >= 0))
        {
            this.mCompleteCacheDic[id] = true;
        }
        if (reset)
        {
            this.SetCurrentNewbieGuideId(0);
        }
        if (!flag && this.mCompleteCacheDic[id])
        {
            Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().SetNewbieAchieve((int) id, true, true);
            uint[] param = new uint[] { id };
            this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteNewbieGuide, param);
        }
    }

    public void SetWeakGuideComplete(uint lineId, bool reset = true)
    {
        bool flag;
        if (this.mWeakCompleteCacheDic.TryGetValue(lineId, out flag))
        {
            this.mWeakCompleteCacheDic[lineId] = true;
        }
        Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().SetNewbieAchieve((int) (lineId + WEAKGUIDE_BIT_OFFSET), true, true);
        uint[] param = new uint[] { lineId };
        this.CheckSkipCondition(NewbieGuideSkipConditionType.hasCompleteNewbieGuide, param);
    }

    public void StopCurrentGuide()
    {
        if (null != this.mCurrentScriptControl)
        {
            this.mCurrentScriptControl.Stop();
            this.DestroyCurrentScriptControl();
        }
    }

    private void TriggerNewbieGuide(NewbieGuideMainLineConf conf, int startIndex)
    {
        if (Singleton<NetworkModule>.GetInstance().lobbySvr.connected)
        {
            Singleton<GameRecord>.GetInstance().RecordNewbieGuide(conf.dwID, false);
            this.SetCurrentNewbieGuideId(conf.dwID);
            this.mCurrentScriptControl = base.gameObject.AddComponent<NewbieGuideScriptControl>();
            this.mCurrentScriptControl.CompleteEvent += new NewbieGuideScriptControl.NewbieGuideScriptControlDelegate(this.NewbieGuideCompleteHandler);
            this.mCurrentScriptControl.SaveEvent += new NewbieGuideScriptControl.NewbieGuideScriptControlDelegate(this.NewbieGuideSaveHandler);
            this.mCurrentScriptControl.SetData(conf.dwID, startIndex);
            this.mCurrentScriptControl.addScriptDelegate = addScriptDelegate;
        }
    }

    private bool TriggerWeakNewbieGuide(uint weakLineId, uint startIndex)
    {
        return Singleton<NewbieWeakGuideControl>.GetInstance().TriggerWeakGuide(weakLineId, startIndex);
    }

    public bool TriggerWeakNewbieGuide(NewbieWeakGuideMainLineConf conf, NewbieGuideTriggerTimeType type, bool checkCondition)
    {
        if (((conf == null) || this.IsWeakLineComplete(conf.dwID)) || (checkCondition && ((!this.CheckLevelLimit(conf) || !this.CheckLevelLimitLower(conf)) || !this.CheckTriggerCondition(0, conf.astTriggerCondition))))
        {
            return false;
        }
        uint weakStartIndexByMianLineConf = this.GetWeakStartIndexByMianLineConf(type, conf);
        return this.TriggerWeakNewbieGuide(conf.dwID, weakStartIndexByMianLineConf);
    }

    public uint currentNewbieGuideId
    {
        get
        {
            return this.mCurrentNewbieGuideId;
        }
    }

    public bool isInit
    {
        get
        {
            return this.mIsInit;
        }
    }

    public bool isNewbieGuiding
    {
        get
        {
            return (this.currentNewbieGuideId != 0);
        }
    }

    private string logTitle
    {
        get
        {
            return "[<color=cyan>NewbieGuide</color>][<color=green>test</color>]";
        }
    }
}

