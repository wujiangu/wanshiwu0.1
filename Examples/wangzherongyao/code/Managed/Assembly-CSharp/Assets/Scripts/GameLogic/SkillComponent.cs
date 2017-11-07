namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.DataCenter;
    using Assets.Scripts.GameSystem;
    using CSProtocol;
    using ResData;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class SkillComponent : LogicComponent
    {
        private bool bIsCurAtkUseSkill;
        public CommonAttackType commonAttackType;
        public Skill CurUseSkill;
        public SkillSlot CurUseSkillSlot;
        public int[] DisableSkill = new int[7];
        public int m_iSkillPoint;
        public VInt3 RecordPosition = VInt3.zero;
        public SkillSlot[] SkillSlotArray = new SkillSlot[7];
        public SkillCache SkillUseCache;
        public ListView<BulletSkill> SpawnedBullets = new ListView<BulletSkill>();
        public CSkillStat stSkillStat;

        public bool AbortCurUseSkill(SkillAbortType _type)
        {
            if (this.CurUseSkillSlot != null)
            {
                return this.CurUseSkillSlot.Abort(_type);
            }
            return true;
        }

        public void CancelUseSkillSlot(SkillSlotType slot)
        {
            SkillSlot slot2;
            if (this.TryGetSkillSlot(slot, out slot2))
            {
                slot2.CancelUseSkill();
                DefaultSkillEventParam param = new DefaultSkillEventParam(slot, 0);
                Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_UseCanceled, base.actorPtr, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
            }
        }

        public bool CanUseSkill(SkillSlotType slotType)
        {
            SkillSlot slot;
            if ((this.CurUseSkill != null) && !this.CurUseSkill.canAbort((SkillAbortType) slotType))
            {
                return false;
            }
            Skill skill = this.FindSkill(slotType);
            if ((skill == null) || (skill.cfgData == null))
            {
                return false;
            }
            if (!this.TryGetSkillSlot(slotType, out slot))
            {
                return false;
            }
            if (!slot.IsCDReady)
            {
                return false;
            }
            return (!base.actor.ValueComponent.IsEnergyType(ENERGY_TYPE.Magic) || slot.IsEnergyEnough);
        }

        private void ClearSkillSlot()
        {
            for (int i = 0; i < 7; i++)
            {
                this.DisableSkill[i] = 0;
                this.SkillSlotArray[i] = null;
            }
        }

        public void CreateTalent(int _talentID)
        {
            this.talentSystem.InitTalent(_talentID);
        }

        public override void Deactive()
        {
            for (int i = 0; i < this.SpawnedBullets.Count; i++)
            {
                if (this.SpawnedBullets[i].isFinish)
                {
                    this.SpawnedBullets[i].Release();
                }
                else
                {
                    this.SpawnedBullets[i].bManaged = false;
                }
            }
            this.SpawnedBullets.Clear();
            base.Deactive();
        }

        public void DelayAbortCurUseSkill()
        {
            if ((this.CurUseSkillSlot != null) && (this.CurUseSkill != null))
            {
                if (!this.CurUseSkill.bProtectAbortSkill)
                {
                    this.CurUseSkillSlot.ForceAbort();
                }
                else
                {
                    this.CurUseSkill.bDelayAbortSkill = true;
                }
            }
        }

        public override void FightOver()
        {
            base.FightOver();
            for (int i = 0; i < 7; i++)
            {
                SkillSlot slot = this.SkillSlotArray[i];
                if (slot != null)
                {
                    slot.CancelUseSkill();
                }
            }
        }

        public Skill FindSkill(SkillSlotType slot)
        {
            SkillSlot slot2;
            if (this.TryGetSkillSlot(slot, out slot2))
            {
                return slot2.SkillObj;
            }
            return null;
        }

        public void ForceAbortCurUseSkill()
        {
            if (this.CurUseSkillSlot != null)
            {
                this.CurUseSkillSlot.ForceAbort();
            }
        }

        public uint GetAdvanceCommonAttackTarget()
        {
            return Singleton<CommonAttackSearcher>.GetInstance().AdvanceCommonAttackSearchEnemy(base.actorPtr, base.actor.ActorControl.SearchRange);
        }

        public int GetAllSkillLevel()
        {
            int num = 0;
            for (int i = 1; i <= 3; i++)
            {
                SkillSlot slot = this.SkillSlotArray[i];
                if (slot != null)
                {
                    num += slot.GetSkillLevel();
                }
            }
            return num;
        }

        public CommonAttackType GetCommonAttackType()
        {
            return this.commonAttackType;
        }

        public SkillSlot GetSkillSlot(SkillSlotType slot)
        {
            SkillSlot slot2 = null;
            this.TryGetSkillSlot(slot, out slot2);
            return slot2;
        }

        public bool HasPunishSkill()
        {
            SkillSlot slot = this.SkillSlotArray[5];
            return (((slot != null) && (slot.SkillObj != null)) && (slot.SkillObj.cfgData.bSkillType == 2));
        }

        public override void Init()
        {
            base.Init();
            this.talentSystem = new TalentSystem();
            this.talentSystem.Init(base.actorPtr);
            this.stSkillStat = new CSkillStat();
            if (this.stSkillStat != null)
            {
                this.stSkillStat.Initialize(base.actorPtr);
                this.InitRandomSkill();
                IGameActorDataProvider actorDataProvider = Singleton<ActorDataCenter>.instance.GetActorDataProvider(GameActorDataProviderType.StaticBattleDataProvider);
                IGameActorDataProvider provider2 = Singleton<ActorDataCenter>.instance.GetActorDataProvider(GameActorDataProviderType.ServerDataProvider);
                ActorStaticSkillData skillData = new ActorStaticSkillData();
                for (int i = 0; i < 7; i++)
                {
                    ResDT_LevelCommonInfo info4;
                    if (i == 6)
                    {
                        SLevelContext context = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
                        if (context.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_GUIDE)
                        {
                            if (context.iLevelID == CBattleGuideManager.GuideLevelID5v5)
                            {
                                goto Label_00DF;
                            }
                            continue;
                        }
                        ResDT_LevelCommonInfo info = null;
                        info = CLevelCfgLogicManager.FindLevelConfigMultiGame(context.iLevelID);
                        if ((info == null) || (info.bMaxAcntNum != 10))
                        {
                            continue;
                        }
                    }
                Label_00DF:
                    if (((i != 4) && (i != 6)) || (base.actor.TheActorMeta.ActorType != ActorTypeDef.Actor_Type_Hero))
                    {
                        goto Label_0284;
                    }
                    SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
                    if (curLvelContext == null)
                    {
                        continue;
                    }
                    int iLevelID = curLvelContext.iLevelID;
                    if (iLevelID <= 0)
                    {
                        continue;
                    }
                    if (Singleton<LobbyLogic>.instance.inMultiGame)
                    {
                        goto Label_0214;
                    }
                    ResLevelCfgInfo outLevelCfg = null;
                    ResDT_LevelCommonInfo outLevelComInfo = null;
                    CLevelCfgLogicManager.FindLevelConfigSingleGame(iLevelID, out outLevelCfg, out outLevelComInfo);
                    if (outLevelComInfo == null)
                    {
                        goto Label_01AD;
                    }
                    int iExtraSkillId = outLevelComInfo.iExtraSkillId;
                    if (i == 6)
                    {
                        iExtraSkillId = outLevelComInfo.iExtraSkill2Id;
                        if (iExtraSkillId > 0)
                        {
                            goto Label_0185;
                        }
                        continue;
                    }
                    this.CreateTalent(outLevelComInfo.iExtraPassiveSkillId);
                Label_0185:
                    this.InitSkillSlot(i, iExtraSkillId, 0);
                    SkillSlot slot = this.SkillSlotArray[i];
                    if (slot != null)
                    {
                        slot.SetSkillLevel(1);
                    }
                    continue;
                Label_01AD:
                    if (outLevelCfg == null)
                    {
                        continue;
                    }
                    int num4 = outLevelCfg.iExtraSkillId;
                    if (i == 6)
                    {
                        num4 = outLevelCfg.iExtraSkill2Id;
                        if (num4 > 0)
                        {
                            goto Label_01EC;
                        }
                        continue;
                    }
                    this.CreateTalent(outLevelCfg.iExtraPassiveSkillId);
                Label_01EC:
                    this.InitSkillSlot(i, num4, 0);
                    SkillSlot slot2 = this.SkillSlotArray[i];
                    if (slot2 != null)
                    {
                        slot2.SetSkillLevel(1);
                    }
                    continue;
                Label_0214:
                    info4 = CLevelCfgLogicManager.FindLevelConfigMultiGame(iLevelID);
                    if (info4 == null)
                    {
                        continue;
                    }
                    int num5 = info4.iExtraSkillId;
                    if (i == 6)
                    {
                        num5 = info4.iExtraSkill2Id;
                        if (num5 > 0)
                        {
                            goto Label_025C;
                        }
                        continue;
                    }
                    this.CreateTalent(info4.iExtraPassiveSkillId);
                Label_025C:
                    this.InitSkillSlot(i, num5, 0);
                    SkillSlot slot3 = this.SkillSlotArray[i];
                    if (slot3 != null)
                    {
                        slot3.SetSkillLevel(1);
                    }
                    continue;
                Label_0284:
                    if (actorDataProvider.GetActorStaticSkillData(ref base.actor.TheActorMeta, (ActorSkillSlot) i, ref skillData))
                    {
                        this.InitSkillSlot(i, skillData.SkillId, skillData.PassiveSkillId);
                        if (((i > 3) || (i < 1)) || (!Singleton<BattleLogic>.GetInstance().m_GameInfo.gameContext.IsSoulGrow() || (base.actor.TheActorMeta.ActorType != ActorTypeDef.Actor_Type_Hero)))
                        {
                            SkillSlot slot4 = this.SkillSlotArray[i];
                            if (slot4 != null)
                            {
                                slot4.SetSkillLevel(1);
                            }
                        }
                    }
                }
                uint skillID = 0;
                if (provider2.GetActorServerCommonSkillData(ref base.actor.TheActorMeta, out skillID))
                {
                    int num7 = 5;
                    if (skillID != 0)
                    {
                        this.InitSkillSlot(num7, (int) skillID, 0);
                        SkillSlot slot5 = this.SkillSlotArray[num7];
                        if (slot5 != null)
                        {
                            slot5.SetSkillLevel(1);
                        }
                    }
                }
                if (base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero)
                {
                    this.SkillUseCache = new SkillCache();
                }
            }
        }

        private void InitRandomSkill()
        {
            if (base.actor != null)
            {
                this.InitRandomSkill(base.actor.TheStaticData.TheBaseAttribute.RandomPassiveSkillRule);
            }
        }

        public void InitRandomSkill(int inPassSkillRule)
        {
            int num = 0;
            int key = inPassSkillRule;
            if ((key != 0) && Singleton<FrameSynchr>.instance.bRunning)
            {
                ResRandomSkillPassiveRule dataByKey = GameDataMgr.randomSkillPassiveDatabin.GetDataByKey(key);
                if (dataByKey != null)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        if (dataByKey.astRandomSkillPassiveID1[i].iParam == 0)
                        {
                            break;
                        }
                        num++;
                    }
                    if (num > 0)
                    {
                        ushort index = FrameRandom.Random((uint) num);
                        int iParam = dataByKey.astRandomSkillPassiveID1[index].iParam;
                        this.CreateTalent(iParam);
                        int num6 = dataByKey.astRandomSkillPassiveID2[index].iParam;
                        if (num6 != 0)
                        {
                            this.CreateTalent(num6);
                        }
                    }
                }
            }
        }

        public void InitSkillSlot(int _slotType, int _skillID, int _passiveID)
        {
            if (base.actorPtr == 0)
            {
                DebugHelper.Assert(base.actorPtr == 1);
            }
            else
            {
                Skill skill = new Skill(_skillID);
                PassiveSkill passive = null;
                if (_passiveID != 0)
                {
                    passive = new PassiveSkill(_passiveID, base.actorPtr);
                }
                SkillSlot slot = new SkillSlot((SkillSlotType) _slotType);
                slot.Init(ref this.actorPtr, skill, passive);
                slot.InitSkillControlIndicator();
                this.SkillSlotArray[_slotType] = slot;
            }
        }

        private bool InternalUseSkill(SkillUseContext context, bool bImmediate = false)
        {
            SkillSlot slot;
            if (context == null)
            {
                return false;
            }
            if (!this.TryGetSkillSlot(context.SlotType, out slot))
            {
                return false;
            }
            slot.ReadySkillObj();
            Skill skillObj = slot.SkillObj;
            if (!skillObj.Use(base.actorPtr, context))
            {
                return false;
            }
            if (!bImmediate)
            {
                this.CurUseSkill = skillObj;
                this.CurUseSkillSlot = slot;
            }
            this.SkillInfoStatistic(ref slot);
            this.bIsLastAtkUseSkill = this.bIsCurAtkUseSkill;
            if (context.SlotType == SkillSlotType.SLOT_SKILL_0)
            {
                this.bIsCurAtkUseSkill = false;
            }
            else
            {
                this.bIsCurAtkUseSkill = true;
            }
            ActorSkillEventParam param = new ActorSkillEventParam(base.GetActor(), context.SlotType);
            Singleton<GameSkillEventSys>.GetInstance().SendEvent<ActorSkillEventParam>(GameSkillEventDef.Event_UseSkill, base.GetActor(), ref param, GameSkillEventChannel.Channel_AllActor);
            return true;
        }

        public bool IsDisableSkillSlot(SkillSlotType _type)
        {
            int index = (int) _type;
            if (this.IsIngnoreDisableSkill(_type))
            {
                return false;
            }
            if ((_type < SkillSlotType.SLOT_SKILL_0) || (_type >= SkillSlotType.SLOT_SKILL_COUNT))
            {
                return false;
            }
            return (this.DisableSkill[index] > 0);
        }

        public bool IsEnableSkillSlot(SkillSlotType slot)
        {
            SkillSlot slot2;
            return (this.TryGetSkillSlot(slot, out slot2) && slot2.IsEnableSkillSlot());
        }

        private bool IsIngnoreDisableSkill(SkillSlotType _type)
        {
            SkillSlot slot = null;
            if ((_type < SkillSlotType.SLOT_SKILL_0) || (_type >= SkillSlotType.SLOT_SKILL_COUNT))
            {
                return false;
            }
            return ((this.TryGetSkillSlot(_type, out slot) && (slot.SkillObj != null)) && ((slot.SkillObj.cfgData != null) && (slot.SkillObj.cfgData.bBIngnoreDisable == 1)));
        }

        public bool IsSkillCDReady(SkillSlotType slot)
        {
            SkillSlot slot2;
            return (this.TryGetSkillSlot(slot, out slot2) && slot2.IsCDReady);
        }

        public bool IsUseSkillJoystick(SkillSlotType slot)
        {
            SkillSlot slot2;
            if (Singleton<GameInput>.GetInstance().IsSmartUse())
            {
                return false;
            }
            return (this.TryGetSkillSlot(slot, out slot2) && slot2.IsUseSkillJoystick());
        }

        public override void LateUpdate(int nDelta)
        {
            SkillSlot slot = null;
            for (int i = 0; i < 7; i++)
            {
                slot = this.SkillSlotArray[i];
                if (slot != null)
                {
                    slot.LateUpdate(nDelta);
                }
            }
        }

        public void OnDead()
        {
            if (this.CurUseSkill != null)
            {
                this.CurUseSkill.Stop();
            }
            for (int i = 0; i < this.SpawnedBullets.Count; i++)
            {
                BulletSkill skill = this.SpawnedBullets[i];
                if (skill.IsDeadRemove)
                {
                    skill.Stop();
                    skill.Release();
                    this.SpawnedBullets.RemoveAt(i);
                    i--;
                }
            }
        }

        public override void OnUse()
        {
            base.OnUse();
            this.CurUseSkill = null;
            this.CurUseSkillSlot = null;
            this.SkillUseCache = null;
            this.talentSystem = null;
            this.bIsLastAtkUseSkill = false;
            this.bIsCurAtkUseSkill = false;
            this.RecordPosition = VInt3.zero;
            this.SpawnedBullets.Clear();
            this.m_iSkillPoint = 0;
            this.bZeroCd = false;
            this.ClearSkillSlot();
            this.commonAttackType = CommonAttackType.CommonAttackType1;
            this.stSkillStat = null;
        }

        public override void Reactive()
        {
            base.Reactive();
            this.CurUseSkill = null;
            this.CurUseSkillSlot = null;
            this.RecordPosition = VInt3.zero;
            this.SpawnedBullets.Clear();
            this.m_iSkillPoint = 0;
            this.bZeroCd = false;
            this.ResetAllSkillSlot();
            this.ResetSkillCD();
            for (int i = 0; i < 7; i++)
            {
                this.DisableSkill[i] = 0;
            }
            if (this.SkillSlotArray != null)
            {
                for (int j = 0; j < this.SkillSlotArray.Length; j++)
                {
                    SkillSlot slot = this.SkillSlotArray[j];
                    if (slot != null)
                    {
                        slot.Reset();
                    }
                }
            }
            if (this.talentSystem != null)
            {
                this.talentSystem.Reset();
            }
        }

        public void ReadyUseSkillSlot(SkillSlotType slot)
        {
            SkillSlot slot2;
            if (this.TryGetSkillSlot(slot, out slot2))
            {
                slot2.ReadyUseSkill();
            }
        }

        public bool RemoveBuff(PoolObjHandle<ActorRoot> inTargetActor, int inSkillCombineId)
        {
            if (inTargetActor != 0)
            {
                inTargetActor.handle.BuffHolderComp.RemoveBuff(inSkillCombineId);
                return true;
            }
            return false;
        }

        public void RequestUseSkillSlot(SkillSlotType slot)
        {
            SkillSlot slot2;
            if (this.TryGetSkillSlot(slot, out slot2))
            {
                slot2.RequestUseSkill();
            }
        }

        public void ResetAllSkillSlot()
        {
            for (int i = 0; i < 7; i++)
            {
                if (this.SkillSlotArray[i] != null)
                {
                    this.SkillSlotArray[i].ResetSkillObj();
                    this.SkillSlotArray[i].skillIndicator.UnInitIndicatePrefab(false);
                }
            }
        }

        public void ResetSkillCD()
        {
            for (int i = 0; i < 7; i++)
            {
                if (this.SkillSlotArray[i] != null)
                {
                    this.SkillSlotArray[i].ResetSkillCD();
                }
            }
        }

        public void ResetSkillLevel()
        {
            PoolObjHandle<ActorRoot> captain = Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain;
            int length = this.SkillSlotArray.Length;
            for (int i = 0; i < length; i++)
            {
                SkillSlot slot = this.SkillSlotArray[i];
                if ((slot != null) && (((slot.SlotType == SkillSlotType.SLOT_SKILL_1) || (slot.SlotType == SkillSlotType.SLOT_SKILL_2)) || (slot.SlotType == SkillSlotType.SLOT_SKILL_3)))
                {
                    slot.SetSkillLevel(0);
                    if (captain == base.actorPtr)
                    {
                        Singleton<CBattleSystem>.instance.ClearSkillLvlStates(i);
                    }
                }
            }
            if (captain == base.actorPtr)
            {
                Singleton<CBattleSystem>.instance.ResetSkillButtonManager(base.actorPtr);
            }
            this.m_iSkillPoint = 0;
        }

        public void SelectSkillTarget(SkillSlotType slot, Vector2 axis, bool isSkillCursorInCancelArea)
        {
            SkillSlot slot2;
            if (this.TryGetSkillSlot(slot, out slot2))
            {
                slot2.skillIndicator.SelectSkillTarget(axis, isSkillCursorInCancelArea);
            }
        }

        public void SetCommonAttackIndicator(bool bShow)
        {
            SkillSlot slot;
            if (this.TryGetSkillSlot(SkillSlotType.SLOT_SKILL_0, out slot))
            {
                if (base.actor.ActorControl.IsUseAdvanceCommonAttack())
                {
                    slot.skillIndicator.SetFixedPrefabShow(bShow);
                }
                else
                {
                    slot.skillIndicator.SetGuildPrefabShow(bShow);
                    slot.skillIndicator.SetEffectPrefabShow(false);
                }
            }
        }

        public void SetCommonAttackType(CommonAttackType _type)
        {
            this.commonAttackType = _type;
        }

        public void SetDisableSkillSlot(SkillSlotType _type, bool bAdd)
        {
            int index = (int) _type;
            if ((_type >= SkillSlotType.SLOT_SKILL_0) && (_type <= SkillSlotType.SLOT_SKILL_COUNT))
            {
                if (_type == SkillSlotType.SLOT_SKILL_COUNT)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (!this.IsIngnoreDisableSkill((SkillSlotType) i))
                        {
                            if (bAdd)
                            {
                                this.DisableSkill[i]++;
                            }
                            else
                            {
                                this.DisableSkill[i]--;
                            }
                        }
                    }
                }
                else if (!this.IsIngnoreDisableSkill(_type))
                {
                    if (bAdd)
                    {
                        this.DisableSkill[index]++;
                    }
                    else
                    {
                        this.DisableSkill[index]--;
                    }
                }
            }
        }

        private void SkillInfoStatistic(ref SkillSlot stSkillSlot)
        {
            if ((stSkillSlot != null) && ((stSkillSlot.SkillStatistictInfo.iSkillCfgID == 0) || (stSkillSlot.SkillStatistictInfo.iSkillCfgID == stSkillSlot.SkillObj.cfgData.iCfgID)))
            {
                stSkillSlot.SkillStatistictInfo.iSkillCfgID = stSkillSlot.SkillObj.cfgData.iCfgID;
                stSkillSlot.SkillStatistictInfo.uiUsedTimes++;
                stSkillSlot.SkillStatistictInfo.iAttackDistanceMax = Math.Max(stSkillSlot.SkillStatistictInfo.iAttackDistanceMax, (int) stSkillSlot.SkillObj.cfgData.iMaxAttackDistance);
                long num = (long) (Time.realtimeSinceStartup * 1000f);
                uint num2 = (uint) (num - stSkillSlot.lLastUseTime);
                if (stSkillSlot.lLastUseTime != 0)
                {
                    uint num3 = Math.Min(stSkillSlot.SkillStatistictInfo.uiCDIntervalMin, num2);
                    stSkillSlot.SkillStatistictInfo.uiCDIntervalMin = num3;
                    uint num4 = Math.Max(stSkillSlot.SkillStatistictInfo.uiCDIntervalMax, num2);
                    stSkillSlot.SkillStatistictInfo.uiCDIntervalMax = num4;
                }
                stSkillSlot.lLastUseTime = num;
            }
        }

        public bool SpawnBuff(PoolObjHandle<ActorRoot> inTargetActor, SkillUseContext inContext, int inSkillCombineId, bool bExtraBuff = false)
        {
            if (((inTargetActor == 0) || (inContext == null)) || (inSkillCombineId <= 0))
            {
                return false;
            }
            BuffSkill skill = ClassObjPool<BuffSkill>.Get();
            skill.Init(inSkillCombineId);
            SkillUseContext context = inContext.Clone();
            context.EffectPos = inContext.EffectPos;
            context.EffectDir = inContext.EffectDir;
            context.TargetActor = inTargetActor;
            context.Instigator = base.actor;
            skill.bExtraBuff = bExtraBuff;
            bool flag = skill.Use(base.actorPtr, context);
            if (!flag)
            {
                skill.Release();
            }
            return flag;
        }

        public PoolObjHandle<BulletSkill> SpawnBullet(SkillUseContext context, string _actionName, bool _bDeadRemove)
        {
            PoolObjHandle<BulletSkill> handle = new PoolObjHandle<BulletSkill>();
            if (context != null)
            {
                BulletSkill item = ClassObjPool<BulletSkill>.Get();
                item.Init(_actionName, _bDeadRemove);
                if (item.Use(base.actorPtr, context.Clone()))
                {
                    this.SpawnedBullets.Add(item);
                    return new PoolObjHandle<BulletSkill>(item);
                }
                item.Release();
            }
            return handle;
        }

        public void ToggleZeroCd()
        {
            this.ResetSkillCD();
            this.bZeroCd = !this.bZeroCd;
            Singleton<EventRouter>.GetInstance().BroadCastEvent<PoolObjHandle<ActorRoot>, int, int>("HeroEnergyChange", base.actorPtr, base.actor.ValueComponent.actorEp, base.actor.ValueComponent.actorEpTotal);
        }

        public bool TryGetSkillSlot(SkillSlotType _type, out SkillSlot _slot)
        {
            int index = (int) _type;
            if (((this.SkillSlotArray == null) || (index < 0)) || (index >= 7))
            {
                _slot = null;
                return false;
            }
            _slot = this.SkillSlotArray[index];
            if (_slot == null)
            {
                return false;
            }
            return true;
        }

        public override void Uninit()
        {
            base.Uninit();
            for (int i = 0; i < this.SpawnedBullets.Count; i++)
            {
                if (this.SpawnedBullets[i].isFinish)
                {
                    this.SpawnedBullets[i].Release();
                }
                else
                {
                    this.SpawnedBullets[i].bManaged = false;
                }
            }
            this.SpawnedBullets.Clear();
            for (int j = 0; j < 7; j++)
            {
                SkillSlot slot = this.SkillSlotArray[j];
                if (((slot != null) && (slot.PassiveSkillObj != null)) && (slot.PassiveSkillObj.passiveEvent != null))
                {
                    slot.PassiveSkillObj.passiveEvent.UnInit();
                }
            }
            if (this.talentSystem != null)
            {
                this.talentSystem.UnInit();
            }
            if (this.stSkillStat != null)
            {
                this.stSkillStat.UnInit();
            }
        }

        public override void UpdateLogic(int nDelta)
        {
            SkillSlot slot = null;
            if ((this.CurUseSkill != null) && this.CurUseSkill.isFinish)
            {
                this.CurUseSkill = null;
                this.CurUseSkillSlot = null;
            }
            for (int i = 0; i < this.SpawnedBullets.Count; i++)
            {
                BulletSkill skill = this.SpawnedBullets[i];
                if (skill != null)
                {
                    skill.UpdateLogic(nDelta);
                    if (skill.isFinish)
                    {
                        skill.Release();
                        this.SpawnedBullets.RemoveAt(i);
                        i--;
                    }
                }
            }
            for (int j = 0; j < 7; j++)
            {
                slot = this.SkillSlotArray[j];
                if (slot != null)
                {
                    slot.UpdateLogic(nDelta);
                }
            }
            if (this.talentSystem != null)
            {
                this.talentSystem.UpdateLogic(nDelta);
            }
        }

        public bool UseSkill(SkillUseContext context, bool bImmediate = false)
        {
            return this.InternalUseSkill(context, bImmediate);
        }

        public bool bIsLastAtkUseSkill { get; private set; }

        public bool bZeroCd { get; private set; }

        public bool isUsing
        {
            get
            {
                return (this.CurUseSkill != null);
            }
        }

        public TalentSystem talentSystem { get; private set; }
    }
}

