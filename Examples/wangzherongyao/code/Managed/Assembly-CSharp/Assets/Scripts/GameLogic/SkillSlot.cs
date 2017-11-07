namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.GameKernal;
    using ResData;
    using System;
    using System.Runtime.CompilerServices;

    public class SkillSlot
    {
        public PoolObjHandle<ActorRoot> Actor;
        private bool bLimitUse;
        private int changeSkillCDRate;
        private int eventTime;
        public Skill InitSkillObj;
        public long lLastUseTime;
        public SkillChangeEvent skillChangeEvent;
        public SkillControlIndicator skillIndicator;
        private int skillLevel;
        public SKILLSTATISTICTINFO SkillStatistictInfo;

        private SkillSlot()
        {
            this.SkillStatistictInfo = new SKILLSTATISTICTINFO();
        }

        public SkillSlot(SkillSlotType type)
        {
            this.SkillStatistictInfo = new SKILLSTATISTICTINFO();
            this.IsCDReady = true;
            this.SlotType = type;
            this.bLimitUse = false;
            this.skillChangeEvent = new SkillChangeEvent(this);
            this.skillIndicator = new SkillControlIndicator(this);
            this.changeSkillCDRate = 0;
            this.lLastUseTime = 0L;
            this.CurSkillCD = 0;
        }

        public bool Abort(SkillAbortType _type)
        {
            if (this.SkillObj != null)
            {
                if (this.SkillObj.isFinish)
                {
                    return true;
                }
                if (!this.SkillObj.canAbort(_type))
                {
                    return false;
                }
                this.SkillObj.Stop();
                if (this.SlotType != ((SkillSlotType) ((int) _type)))
                {
                    this.skillChangeEvent.Abort();
                }
            }
            return true;
        }

        public void CancelUseSkill()
        {
            if (this.skillIndicator != null)
            {
                this.skillIndicator.UnInitIndicatePrefab(false);
            }
        }

        public void ChangeMaxCDRate(int _rate)
        {
            this.changeSkillCDRate += _rate;
        }

        public void ChangeSkillCD(int _time)
        {
            if (!this.IsCDReady)
            {
                this.CurSkillCD -= _time;
                if (this.CurSkillCD < 0)
                {
                    this.CurSkillCD = 0;
                    this.IsCDReady = true;
                }
                if (this.SlotType != SkillSlotType.SLOT_SKILL_0)
                {
                    DefaultSkillEventParam param = new DefaultSkillEventParam(this.SlotType, (int) this.CurSkillCD);
                    Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_ChangeSkillCD, this.Actor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                    if (this.IsCDReady)
                    {
                        Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_SkillCDEnd, this.Actor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                    }
                }
            }
        }

        public void ForceAbort()
        {
            if ((this.SkillObj != null) && !this.SkillObj.isFinish)
            {
                this.SkillObj.Stop();
                this.skillChangeEvent.Abort();
            }
        }

        public int GetSkillCDMax()
        {
            ValueDataInfo info = null;
            int iCoolDown = (int) this.SkillObj.cfgData.iCoolDown;
            int num2 = this.skillLevel - 1;
            if (num2 < 0)
            {
                num2 = 0;
            }
            iCoolDown += this.SkillObj.cfgData.iCoolDownGrowth * num2;
            if (this.Actor != 0)
            {
                int totalValue = 0;
                info = this.Actor.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_CDREDUCE];
                if (((info != null) && (this.SlotType != SkillSlotType.SLOT_SKILL_0)) && ((this.SlotType != SkillSlotType.SLOT_SKILL_4) && (this.SlotType != SkillSlotType.SLOT_SKILL_5)))
                {
                    totalValue = info.totalValue;
                }
                int num4 = totalValue + this.changeSkillCDRate;
                if (info.maxLimitValue > 0)
                {
                    num4 = (num4 <= info.maxLimitValue) ? num4 : info.maxLimitValue;
                }
                long num5 = iCoolDown * (0x2710L - num4);
                iCoolDown = (int) (num5 / 0x2710L);
            }
            iCoolDown = (iCoolDown >= 0) ? iCoolDown : 0;
            if (this.SlotType == SkillSlotType.SLOT_SKILL_0)
            {
                info = this.Actor.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_ATKSPDADD];
                iCoolDown = (iCoolDown * 0x2710) / (0x2710 + info.totalValue);
                iCoolDown = (iCoolDown >= 0) ? iCoolDown : 0;
            }
            if (((this.Actor != 0) && this.Actor.handle.SkillControl.bZeroCd) && (this.SlotType != SkillSlotType.SLOT_SKILL_0))
            {
                iCoolDown = 0;
            }
            return iCoolDown;
        }

        public int GetSkillLevel()
        {
            return this.skillLevel;
        }

        public void Init(ref PoolObjHandle<ActorRoot> _actor, Skill skill, PassiveSkill passive)
        {
            this.Actor = _actor;
            this.IsCDReady = true;
            this.CurSkillCD = 0;
            this.SkillObj = skill;
            this.InitSkillObj = skill;
            this.NextSkillObj = null;
            this.SkillObj.SlotType = this.SlotType;
            this.PassiveSkillObj = passive;
            if (this.PassiveSkillObj != null)
            {
                this.PassiveSkillObj.SlotType = this.SlotType;
            }
        }

        public void InitSkillControlIndicator()
        {
            this.skillIndicator.InitControlIndicator();
            this.skillIndicator.CreateIndicatePrefab(this.SkillObj);
        }

        public bool IsAbort(SkillAbortType _type)
        {
            if (this.SkillObj != null)
            {
                if (this.SkillObj.isFinish)
                {
                    return true;
                }
                if (!this.SkillObj.canAbort(_type))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsEnableSkillSlot()
        {
            return ((this.IsCDReady && this.IsUnLock()) && (!this.bLimitUse && !this.Actor.handle.ActorControl.IsDeadState));
        }

        public bool IsUnLock()
        {
            return (this.skillLevel > 0);
        }

        public bool IsUseSkillJoystick()
        {
            if ((this.SkillObj == null) || (this.SkillObj.cfgData == null))
            {
                return false;
            }
            if (!this.IsEnableSkillSlot() && !this.Actor.handle.ActorControl.IsUseAdvanceCommonAttack())
            {
                return false;
            }
            if ((this.SlotType == SkillSlotType.SLOT_SKILL_0) && !this.Actor.handle.ActorControl.IsUseAdvanceCommonAttack())
            {
                return false;
            }
            return true;
        }

        public void LateUpdate(int nDelta)
        {
            this.skillIndicator.LateUpdate(nDelta);
        }

        public int NextSkillEnergyCostTotal()
        {
            if (((this.Actor == 0) || (this.Actor.handle.SkillControl == null)) || !this.Actor.handle.SkillControl.bZeroCd)
            {
                int skillLevel = this.skillLevel;
                if (skillLevel == 0)
                {
                    skillLevel++;
                }
                if ((this.SkillObj != null) && (this.SkillObj.cfgData != null))
                {
                    ResSkillCfgInfo cfgData = this.SkillObj.cfgData;
                    if (this.NextSkillObj != null)
                    {
                        cfgData = this.NextSkillObj.cfgData;
                    }
                    if (cfgData != null)
                    {
                        return this.SkillEnergyCost(cfgData, skillLevel);
                    }
                }
            }
            return 0;
        }

        public void ReadySkillObj()
        {
            if (this.NextSkillObj != null)
            {
                this.SkillObj = this.NextSkillObj;
                this.NextSkillObj = null;
                this.skillChangeEvent.Stop();
            }
        }

        public void ReadyUseSkill()
        {
            Skill readySkillObj = (this.NextSkillObj == null) ? this.SkillObj : this.NextSkillObj;
            if ((readySkillObj != null) && (readySkillObj.cfgData != null))
            {
                OperateMode playerOperateMode = ActorHelper.GetPlayerOperateMode(ref this.Actor);
                if (!Singleton<GameInput>.GetInstance().IsSmartUse() && (playerOperateMode == OperateMode.DefaultMode))
                {
                    this.ReadyUseSkillDefaultAttackMode(readySkillObj);
                }
                else if (playerOperateMode == OperateMode.LockMode)
                {
                    this.ReadyUseSkillLockAttackMode(readySkillObj);
                }
            }
        }

        private void ReadyUseSkillDefaultAttackMode(Skill readySkillObj)
        {
            ActorRoot target = null;
            if (readySkillObj.AppointType != SkillRangeAppointType.Target)
            {
                target = Singleton<TargetSearcher>.GetInstance().GetLowestHpTarget(this.Actor.handle, (int) readySkillObj.cfgData.iMaxAttackDistance, TargetPriority.TargetPriority_Hero, readySkillObj.cfgData.dwSkillTargetFilter, true);
            }
            if (target != null)
            {
                this.skillIndicator.SetSkillUsePosition(target);
            }
            else
            {
                this.skillIndicator.SetSkillUseDefaultPosition();
            }
            if ((readySkillObj.AppointType == SkillRangeAppointType.Target) && (readySkillObj.cfgData.dwSkillTargetRule != 2))
            {
                this.skillIndicator.SetGuildPrefabShow(false);
                this.skillIndicator.SetGuildWarnPrefabShow(false);
                this.skillIndicator.SetUseAdvanceMode(false);
                this.skillIndicator.SetSkillUseDefaultPosition();
                this.skillIndicator.SetEffectPrefabShow(false);
                this.skillIndicator.SetFixedPrefabShow(true);
            }
            else
            {
                this.skillIndicator.SetGuildPrefabShow(true);
                this.skillIndicator.SetGuildWarnPrefabShow(false);
                this.skillIndicator.SetUseAdvanceMode(true);
            }
        }

        private void ReadyUseSkillLockAttackMode(Skill readySkillObj)
        {
            int dwSkillTargetFilter = 0;
            SelectEnemyType selectLowHp = SelectEnemyType.SelectLowHp;
            uint lockTargetID = this.Actor.handle.LockTargetAttackModeControl.GetLockTargetID();
            if (!this.Actor.handle.LockTargetAttackModeControl.IsValidLockTargetID(lockTargetID))
            {
                Player ownerPlayer = ActorHelper.GetOwnerPlayer(ref this.Actor);
                if (ownerPlayer != null)
                {
                    selectLowHp = ownerPlayer.AttackTargetMode;
                }
                if (readySkillObj.AppointType == SkillRangeAppointType.Target)
                {
                    lockTargetID = 0;
                }
                else
                {
                    int iMaxAttackDistance = (int) readySkillObj.cfgData.iMaxAttackDistance;
                    dwSkillTargetFilter = (int) readySkillObj.cfgData.dwSkillTargetFilter;
                    if (selectLowHp == SelectEnemyType.SelectLowHp)
                    {
                        lockTargetID = Singleton<AttackModeTargetSearcher>.GetInstance().SearchLowestHpTarget(ref this.Actor, iMaxAttackDistance, dwSkillTargetFilter);
                    }
                    else
                    {
                        lockTargetID = Singleton<AttackModeTargetSearcher>.GetInstance().SearchNearestTarget(ref this.Actor, iMaxAttackDistance, dwSkillTargetFilter);
                    }
                }
            }
            PoolObjHandle<ActorRoot> actor = Singleton<GameObjMgr>.GetInstance().GetActor(lockTargetID);
            if (actor != 0)
            {
                this.skillIndicator.SetSkillUsePosition(actor.handle);
            }
            else
            {
                this.skillIndicator.SetSkillUseDefaultPosition();
            }
            if ((readySkillObj.AppointType == SkillRangeAppointType.Target) && (readySkillObj.cfgData.dwSkillTargetRule != 2))
            {
                this.skillIndicator.SetGuildPrefabShow(false);
                this.skillIndicator.SetGuildWarnPrefabShow(false);
                this.skillIndicator.SetUseAdvanceMode(false);
                this.skillIndicator.SetSkillUseDefaultPosition();
                this.skillIndicator.SetEffectPrefabShow(false);
                this.skillIndicator.SetFixedPrefabShow(true);
            }
            else
            {
                this.skillIndicator.SetGuildPrefabShow(true);
                this.skillIndicator.SetGuildWarnPrefabShow(false);
                this.skillIndicator.SetUseAdvanceMode(true);
            }
        }

        public void RequestUseSkill()
        {
            bool flag = false;
            Skill skill = (this.NextSkillObj == null) ? this.SkillObj : this.NextSkillObj;
            this.skillIndicator.SetFixedPrefabShow(false);
            this.skillIndicator.SetGuildPrefabShow(false);
            this.skillIndicator.SetGuildWarnPrefabShow(false);
            if ((this.IsEnableSkillSlot() && (skill != null)) && (skill.cfgData != null))
            {
                if (Singleton<SkillDetectionControl>.GetInstance().Detection((SkillUseRule) skill.cfgData.dwSkillUseRule, this))
                {
                    flag = this.SendRequestUseSkill();
                }
                if (!flag)
                {
                    ActorSkillEventParam param = new ActorSkillEventParam(this.Actor, SkillSlotType.SLOT_SKILL_0);
                    Singleton<GameSkillEventSys>.GetInstance().SendEvent<ActorSkillEventParam>(GameSkillEventDef.Event_NoSkillTarget, this.Actor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                }
            }
        }

        public void Reset()
        {
            this.Actor.Validate();
            if (this.PassiveSkillObj != null)
            {
                this.PassiveSkillObj.Reset();
            }
            this.NextSkillObj = null;
            this.CurSkillCD = 0;
            this.IsCDReady = true;
            if (this.skillChangeEvent != null)
            {
                this.skillChangeEvent.Reset();
            }
        }

        public void ResetSkillCD()
        {
            if (!this.IsCDReady)
            {
                this.CurSkillCD = 0;
                this.IsCDReady = true;
                if (this.SlotType != SkillSlotType.SLOT_SKILL_0)
                {
                    DefaultSkillEventParam param = new DefaultSkillEventParam(this.SlotType, (int) this.CurSkillCD);
                    Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_ChangeSkillCD, this.Actor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                    if (this.IsCDReady)
                    {
                        Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_SkillCDEnd, this.Actor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                    }
                }
            }
        }

        public void ResetSkillObj()
        {
            this.SkillObj = this.InitSkillObj;
            this.skillChangeEvent.Stop();
            this.NextSkillObj = null;
        }

        private bool SendRequestUseSkill()
        {
            Skill readySkillObj = (this.NextSkillObj == null) ? this.SkillObj : this.NextSkillObj;
            if (readySkillObj == null)
            {
                return false;
            }
            if (readySkillObj.AppointType == SkillRangeAppointType.Target)
            {
                return this.SendRequestUseSkillTarget(readySkillObj);
            }
            if (readySkillObj.AppointType == SkillRangeAppointType.Directional)
            {
                this.SendRequestUseSkillDir(readySkillObj);
            }
            else if (readySkillObj.AppointType == SkillRangeAppointType.Pos)
            {
                this.SendRequestUseSkillPos(readySkillObj);
            }
            return true;
        }

        private bool SendRequestUseSkillDir(Skill readySkillObj)
        {
            VInt3 one = VInt3.one;
            BaseAttackMode currentAttackMode = this.Actor.handle.ActorControl.GetCurrentAttackMode();
            FrameCommand<UseDirectionalSkillCommand> command = FrameCommandFactory.CreateCSSyncFrameCommand<UseDirectionalSkillCommand>();
            if (currentAttackMode != null)
            {
                one = currentAttackMode.SelectSkillDirection(this);
            }
            command.cmdData.SlotType = this.SlotType;
            command.cmdData.Direction = one;
            command.cmdData.iSkillID = readySkillObj.SkillID;
            command.Send(true);
            return true;
        }

        private bool SendRequestUseSkillPos(Skill readySkillObj)
        {
            bool flag = false;
            VInt3 zero = VInt3.zero;
            BaseAttackMode currentAttackMode = this.Actor.handle.ActorControl.GetCurrentAttackMode();
            FrameCommand<UsePositionSkillCommand> command = FrameCommandFactory.CreateCSSyncFrameCommand<UsePositionSkillCommand>();
            if (currentAttackMode != null)
            {
                flag = currentAttackMode.SelectSkillPos(this, out zero);
            }
            if (flag)
            {
                command.cmdData.Position = zero;
            }
            else
            {
                return false;
            }
            command.cmdData.SlotType = this.SlotType;
            command.cmdData.iSkillID = readySkillObj.SkillID;
            command.Send(true);
            return true;
        }

        private bool SendRequestUseSkillTarget(Skill readySkillObj)
        {
            uint num = 0;
            BaseAttackMode currentAttackMode = this.Actor.handle.ActorControl.GetCurrentAttackMode();
            FrameCommand<UseObjectiveSkillCommand> command = FrameCommandFactory.CreateCSSyncFrameCommand<UseObjectiveSkillCommand>();
            if (currentAttackMode != null)
            {
                num = currentAttackMode.SelectSkillTarget(this);
            }
            if (num == 0)
            {
                return false;
            }
            command.cmdData.ObjectID = num;
            command.cmdData.SlotType = this.SlotType;
            command.cmdData.iSkillID = readySkillObj.SkillID;
            command.Send(true);
            return true;
        }

        public void SendSkillCooldownEvent()
        {
            if (!this.IsCDReady)
            {
                ActorSkillEventParam param = new ActorSkillEventParam(this.Actor, SkillSlotType.SLOT_SKILL_0);
                Singleton<GameSkillEventSys>.GetInstance().SendEvent<ActorSkillEventParam>(GameSkillEventDef.Event_SkillCooldown, this.Actor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
            }
        }

        public void SendSkillShortageEvent()
        {
            if (!this.IsEnergyEnough)
            {
                ActorSkillEventParam param = new ActorSkillEventParam(this.Actor, SkillSlotType.SLOT_SKILL_0);
                Singleton<GameSkillEventSys>.GetInstance().SendEvent<ActorSkillEventParam>(GameSkillEventDef.Enent_EnergyShortage, this.Actor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
            }
        }

        public void SetMaxCDRate(int inRate)
        {
            this.changeSkillCDRate = inRate;
        }

        public void SetSkillLevel(int _level)
        {
            this.skillLevel = _level;
            if (this.skillLevel == 0)
            {
                DefaultSkillEventParam param = new DefaultSkillEventParam(this.SlotType, 0);
                Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_DisableSkill, this.Actor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
            }
            else if (this.skillLevel == 1)
            {
                DefaultSkillEventParam param2 = new DefaultSkillEventParam(this.SlotType, 0);
                Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_EnableSkill, this.Actor, ref param2, GameSkillEventChannel.Channel_HostCtrlActor);
            }
        }

        private int SkillEnergyCost(ResSkillCfgInfo cfgData, int CurSkillLevel)
        {
            if (cfgData.dwEnergyCostType == 0)
            {
                if (cfgData.iEnergyCostCalcType == 0)
                {
                    return (((int) cfgData.iEnergyCost) + ((CurSkillLevel - 1) * cfgData.iEnergyCostGrowth));
                }
                if (cfgData.iEnergyCostCalcType == 1)
                {
                    int actorEpTotal = this.Actor.handle.ValueComponent.actorEpTotal;
                    long num3 = (long) (cfgData.iEnergyCost + (((actorEpTotal * (CurSkillLevel - 1)) * cfgData.iEnergyCostGrowth) / 0x2710));
                    return (int) num3;
                }
                if (cfgData.iEnergyCostCalcType == 2)
                {
                    int actorEp = this.Actor.handle.ValueComponent.actorEp;
                    long num5 = (long) (cfgData.iEnergyCost + (((actorEp * (CurSkillLevel - 1)) * cfgData.iEnergyCostGrowth) / 0x2710));
                    return (int) num5;
                }
                return 0;
            }
            if (this.SkillObj.cfgData.dwEnergyCostType == 1)
            {
                return 0;
            }
            return 0;
        }

        public int SkillEnergyCostTotal()
        {
            if (((this.Actor == 0) || (this.Actor.handle.SkillControl == null)) || !this.Actor.handle.SkillControl.bZeroCd)
            {
                int skillLevel = this.skillLevel;
                if (skillLevel == 0)
                {
                    skillLevel++;
                }
                if ((this.SkillObj != null) && (this.SkillObj.cfgData != null))
                {
                    return this.SkillEnergyCost(this.SkillObj.cfgData, skillLevel);
                }
            }
            return 0;
        }

        public void StartSkillCD()
        {
            this.CurSkillCD = this.GetSkillCDMax();
            this.IsCDReady = false;
            this.eventTime = 0;
            DefaultSkillEventParam param = new DefaultSkillEventParam(this.SlotType, (int) this.CurSkillCD);
            if (this.SlotType != SkillSlotType.SLOT_SKILL_0)
            {
                Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_SkillCDStart, this.Actor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_ChangeSkillCD, this.Actor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
            }
        }

        public void UnInit()
        {
            this.Actor.Release();
            this.SkillObj = null;
            this.InitSkillObj = null;
            this.NextSkillObj = null;
            this.PassiveSkillObj = null;
            this.skillIndicator.UnInitIndicatePrefab(true);
        }

        public void UpdateLogic(int nDelta)
        {
            this.UpdateSkillCD(nDelta);
            this.skillChangeEvent.UpdateSkillCD(nDelta);
            if (this.PassiveSkillObj != null)
            {
                this.PassiveSkillObj.UpdateLogic(nDelta);
            }
        }

        private void UpdateSkillCD(int nDelta)
        {
            if (!this.IsCDReady)
            {
                this.CurSkillCD -= nDelta;
                this.eventTime += nDelta;
                if (this.CurSkillCD <= 0)
                {
                    this.CurSkillCD = 0;
                    this.IsCDReady = true;
                }
                if ((this.eventTime >= 500) || (this.CurSkillCD == 0))
                {
                    if (this.SlotType != SkillSlotType.SLOT_SKILL_0)
                    {
                        DefaultSkillEventParam param = new DefaultSkillEventParam(this.SlotType, (int) this.CurSkillCD);
                        Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_ChangeSkillCD, this.Actor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                        if (this.IsCDReady)
                        {
                            Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_SkillCDEnd, this.Actor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                        }
                    }
                    this.eventTime -= 500;
                }
            }
        }

        public CrypticInt32 CurSkillCD { get; set; }

        public bool EnableButtonFlag
        {
            get
            {
                return (((this.CurSkillCD <= 0) && this.IsEnergyEnough) && ((this.skillLevel > 0) && !this.Actor.handle.ActorControl.IsDeadState));
            }
        }

        public bool IsCDReady { get; set; }

        public bool IsEnergyEnough
        {
            get
            {
                return (this.Actor.handle.ValueComponent.actorEp >= this.NextSkillEnergyCostTotal());
            }
        }

        public Skill NextSkillObj { get; set; }

        public PassiveSkill PassiveSkillObj { get; set; }

        public Skill SkillObj { get; set; }

        public SkillSlotType SlotType { get; set; }

        private enum SKILL_ENERGY_COST_CALC_TYPE
        {
            NUMERICAL_VALUE,
            PROPORTION_TOTAL_VALUE,
            PROPORTION_CURRENT_VALUE
        }

        private enum SKILL_ENERGY_COST_TYPE
        {
            MAGIC,
            HP
        }
    }
}

