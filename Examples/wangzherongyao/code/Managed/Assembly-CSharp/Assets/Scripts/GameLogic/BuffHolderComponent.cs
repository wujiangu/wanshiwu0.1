namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using ResData;
    using System;
    using System.Collections.Generic;

    public class BuffHolderComponent : LogicComponent
    {
        public bool bRemoveList = true;
        public BuffChangeSkillRule changeSkillRule;
        public BuffClearRule clearRule;
        private List<BuffSkill> delBuffList = new List<BuffSkill>(3);
        public BufferLogicEffect logicEffect;
        public BufferMarkRule markRule;
        public BuffOverlayRule overlayRule;
        public BuffProtectRule protectRule;
        public List<BuffSkill> SpawnedBuffList = new List<BuffSkill>();

        public void ActionRemoveBuff(BuffSkill inBuff)
        {
            if (this.SpawnedBuffList.Remove(inBuff))
            {
                PoolObjHandle<BuffSkill> handle = new PoolObjHandle<BuffSkill>(inBuff);
                this.protectRule.RemoveBuff(ref handle);
                BuffChangeEventParam param = new BuffChangeEventParam(false, base.actorPtr, inBuff);
                Singleton<GameSkillEventSys>.GetInstance().SendEvent<BuffChangeEventParam>(GameSkillEventDef.Event_BuffChange, base.actorPtr, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                if (((inBuff.cfgData.dwEffectType == 2) && (inBuff.cfgData.dwShowType != 2)) && (base.actorPtr != 0))
                {
                    LimitMoveEventParam param2 = new LimitMoveEventParam(0, inBuff.SkillID, base.actorPtr);
                    Singleton<GameSkillEventSys>.GetInstance().SendEvent<LimitMoveEventParam>(GameSkillEventDef.Event_CancelLimitMove, base.actorPtr, ref param2, GameSkillEventChannel.Channel_AllActor);
                }
                inBuff.Release();
            }
        }

        public void AddBuff(BuffSkill inBuff)
        {
            this.SpawnedBuffList.Add(inBuff);
            this.protectRule.AddBuff(inBuff);
            BuffChangeEventParam param = new BuffChangeEventParam(true, base.actorPtr, inBuff);
            Singleton<GameSkillEventSys>.GetInstance().SendEvent<BuffChangeEventParam>(GameSkillEventDef.Event_BuffChange, base.actorPtr, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
        }

        public bool CheckTargetSubType(int typeMask, int typeSubMask)
        {
            if (typeMask == 0)
            {
                return true;
            }
            if (base.actorPtr != 0)
            {
                int actorType = (int) this.actorPtr.handle.TheActorMeta.ActorType;
                if ((typeMask & (((int) 1) << actorType)) > 0)
                {
                    if (actorType != 1)
                    {
                        return true;
                    }
                    if (typeSubMask == 0)
                    {
                        return true;
                    }
                    if (this.actorPtr.handle.ActorControl.GetActorSubType() == typeSubMask)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void ClearBuff()
        {
            BuffSkill skill;
            this.bRemoveList = false;
            for (int i = 0; i < this.SpawnedBuffList.Count; i++)
            {
                skill = this.SpawnedBuffList[i];
                if (skill != null)
                {
                    skill.Stop();
                }
            }
            if (this.protectRule != null)
            {
                this.protectRule.ClearBuff();
            }
            for (int j = 0; j < this.SpawnedBuffList.Count; j++)
            {
                skill = this.SpawnedBuffList[j];
                if (skill != null)
                {
                    skill.Release();
                }
            }
            this.SpawnedBuffList.Clear();
            this.delBuffList.Clear();
            this.bRemoveList = true;
        }

        public void ClearEffectTypeBuff(int _typeMask)
        {
            if (this.SpawnedBuffList.Count != 0)
            {
                this.delBuffList = this.SpawnedBuffList;
                for (int i = 0; i < this.delBuffList.Count; i++)
                {
                    BuffSkill skill = this.delBuffList[i];
                    if ((_typeMask & (((int) 1) << skill.cfgData.dwEffectType)) > 0)
                    {
                        skill.Stop();
                    }
                }
            }
        }

        public override void Deactive()
        {
            this.ClearBuff();
            base.Deactive();
        }

        public BuffSkill FindBuff(int inSkillCombineId)
        {
            if (this.SpawnedBuffList != null)
            {
                for (int i = 0; i < this.SpawnedBuffList.Count; i++)
                {
                    BuffSkill skill = this.SpawnedBuffList[i];
                    if ((skill != null) && (skill.SkillID == inSkillCombineId))
                    {
                        return skill;
                    }
                }
            }
            return null;
        }

        public int FindBuffCount(int inSkillCombineId)
        {
            int num = 0;
            for (int i = 0; i < this.SpawnedBuffList.Count; i++)
            {
                BuffSkill skill = this.SpawnedBuffList[i];
                if ((skill != null) && (skill.SkillID == inSkillCombineId))
                {
                    num++;
                }
            }
            return num;
        }

        public int GetExtraHurtOutputRate(PoolObjHandle<ActorRoot> _attack)
        {
            int num = 0;
            BuffSkill skill = null;
            if (_attack == 0)
            {
                return 0;
            }
            for (int i = 0; i < _attack.handle.BuffHolderComp.SpawnedBuffList.Count; i++)
            {
                skill = _attack.handle.BuffHolderComp.SpawnedBuffList[i];
                num += this.OnConditionExtraHurt(skill, _attack);
                num += this.OnTargetExtraHurt(skill, _attack);
                num += this.OnControlExtraHurt(skill, _attack);
            }
            return num;
        }

        public int GetSoulExpAddRate(PoolObjHandle<ActorRoot> _target)
        {
            int num = 0;
            BuffSkill skill = null;
            ResDT_SkillFunc outSkillFunc = null;
            if (_target != 0)
            {
                for (int i = 0; i < this.SpawnedBuffList.Count; i++)
                {
                    skill = this.SpawnedBuffList[i];
                    if ((skill != null) && skill.FindSkillFunc(0x31, out outSkillFunc))
                    {
                        int typeMask = skill.CustomParams[0];
                        int typeSubMask = skill.CustomParams[1];
                        int num5 = skill.CustomParams[2];
                        if (this.CheckTargetSubType(typeMask, typeSubMask))
                        {
                            num += num5;
                        }
                    }
                }
            }
            return num;
        }

        public override void Init()
        {
            this.overlayRule = new BuffOverlayRule();
            this.clearRule = new BuffClearRule();
            this.protectRule = new BuffProtectRule();
            this.changeSkillRule = new BuffChangeSkillRule();
            this.markRule = new BufferMarkRule();
            this.overlayRule.Init(this);
            this.clearRule.Init(this);
            this.protectRule.Init(this);
            this.changeSkillRule.Init(this);
            this.markRule.Init(this);
            base.Init();
        }

        private int OnConditionExtraHurt(BuffSkill _buffSkill, PoolObjHandle<ActorRoot> _attack)
        {
            int num = 0;
            ResDT_SkillFunc outSkillFunc = null;
            if ((_buffSkill != null) && _buffSkill.FindSkillFunc(0x2c, out outSkillFunc))
            {
                int num2 = _buffSkill.CustomParams[0];
                int num3 = _buffSkill.CustomParams[1];
                int num4 = _buffSkill.CustomParams[2];
                int num5 = _buffSkill.CustomParams[3];
                bool flag = num2 == 1;
                int num6 = !flag ? base.actor.ValueComponent.actorHp : _attack.handle.ValueComponent.actorHp;
                int num7 = !flag ? base.actor.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP].totalValue : _attack.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP].totalValue;
                int num8 = (num6 * 0x2710) / num7;
                if (num4 == 1)
                {
                    if (num8 <= num3)
                    {
                        num = num5;
                    }
                    return num;
                }
                if ((num4 == 4) && (num8 >= num3))
                {
                    num = num5;
                }
            }
            return num;
        }

        private int OnControlExtraHurt(BuffSkill _buffSkill, PoolObjHandle<ActorRoot> _attack)
        {
            ResDT_SkillFunc outSkillFunc = null;
            if (((_buffSkill != null) && _buffSkill.FindSkillFunc(0x33, out outSkillFunc)) && (base.actor != null))
            {
                BuffSkill skill = null;
                for (int i = 0; i < base.actor.BuffHolderComp.SpawnedBuffList.Count; i++)
                {
                    skill = base.actor.BuffHolderComp.SpawnedBuffList[i];
                    if ((skill != null) && (skill.cfgData.dwEffectType == 2))
                    {
                        return _buffSkill.CustomParams[0];
                    }
                }
            }
            return 0;
        }

        public int OnDamage(ref HurtDataInfo _hurt, int _hurtValue)
        {
            int num = _hurtValue;
            if (!_hurt.bLastHurt)
            {
                this.clearRule.CheckBuffClear(RES_SKILLFUNC_CLEAR_RULE.RES_SKILLFUNC_CLEAR_DAMAGE);
            }
            if (!_hurt.bExtraBuff)
            {
                this.OnDamageExtraEffect(_hurt.atker, _hurt.atkSlot);
            }
            return this.protectRule.ResistDamage(ref _hurt, num);
        }

        private void OnDamageExtraEffect(PoolObjHandle<ActorRoot> _attack, SkillSlotType _slotType)
        {
            BuffSkill inBuff = null;
            ResDT_SkillFunc outSkillFunc = null;
            if (_attack != 0)
            {
                for (int i = 0; i < _attack.handle.BuffHolderComp.SpawnedBuffList.Count; i++)
                {
                    inBuff = _attack.handle.BuffHolderComp.SpawnedBuffList[i];
                    if ((inBuff != null) && inBuff.FindSkillFunc(0x21, out outSkillFunc))
                    {
                        bool flag = false;
                        bool flag2 = true;
                        int inSkillCombineId = inBuff.CustomParams[0];
                        int num3 = inBuff.CustomParams[1];
                        int num4 = inBuff.CustomParams[2];
                        int typeMask = inBuff.CustomParams[3];
                        int typeSubMask = inBuff.CustomParams[4];
                        int num7 = inBuff.CustomParams[5];
                        if ((num4 == 0) && this.CheckTargetSubType(typeMask, typeSubMask))
                        {
                            if (num3 == 0)
                            {
                                flag = true;
                            }
                            else if ((num3 & (((int) 1) << _slotType)) > 0)
                            {
                                flag = true;
                            }
                            if (num7 > 0)
                            {
                                if ((Singleton<FrameSynchr>.GetInstance().LogicFrameTick - inBuff.controlTime) >= num7)
                                {
                                    flag2 = true;
                                }
                                else
                                {
                                    flag2 = false;
                                }
                            }
                            if (flag && flag2)
                            {
                                SkillUseContext inContext = new SkillUseContext();
                                inContext.SetOriginator(_attack);
                                _attack.handle.SkillControl.SpawnBuff(base.actorPtr, inContext, inSkillCombineId, true);
                                inBuff.controlTime = Singleton<FrameSynchr>.GetInstance().LogicFrameTick;
                                if (num7 == -1)
                                {
                                    _attack.handle.BuffHolderComp.RemoveBuff(inBuff);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OnDamageExtraValueEffect(ref HurtDataInfo _hurt, PoolObjHandle<ActorRoot> _attack, SkillSlotType _slotType)
        {
            BuffSkill skill = null;
            ResDT_SkillFunc outSkillFunc = null;
            if (_attack != 0)
            {
                for (int i = 0; i < _attack.handle.BuffHolderComp.SpawnedBuffList.Count; i++)
                {
                    skill = _attack.handle.BuffHolderComp.SpawnedBuffList[i];
                    if (_hurt.hurtType == HurtTypeDef.Therapic)
                    {
                        if ((skill != null) && skill.FindSkillFunc(0x40, out outSkillFunc))
                        {
                            int num2 = skill.CustomParams[0];
                            int num3 = skill.CustomParams[1];
                            _hurt.iAddTotalHurtType = num2;
                            _hurt.iAddTotalHurtValue = num3;
                        }
                    }
                    else
                    {
                        if ((((_slotType == SkillSlotType.SLOT_SKILL_0) && (_attack.handle.SkillControl != null)) && (_attack.handle.SkillControl.bIsLastAtkUseSkill && (skill != null))) && skill.FindSkillFunc(0x3d, out outSkillFunc))
                        {
                            int num4 = skill.CustomParams[0];
                            int num5 = skill.CustomParams[1];
                            int num6 = skill.CustomParams[2];
                            int num7 = skill.CustomParams[3];
                            if (num4 == 1)
                            {
                                num5 = (num5 * _hurt.hurtValue) / 0x2710;
                                _hurt.hurtValue += num5;
                                _hurt.adValue += num6;
                                _hurt.apValue += num7;
                            }
                            else
                            {
                                _hurt.hurtValue += num5;
                                _hurt.attackInfo.iActorATT += num6;
                                _hurt.attackInfo.iActorINT += num7;
                            }
                        }
                        if ((skill != null) && skill.FindSkillFunc(0x44, out outSkillFunc))
                        {
                            int num8 = skill.CustomParams[0];
                            int num9 = skill.CustomParams[4];
                            int num10 = skill.CustomParams[5];
                            if (_hurt.target.handle.ValueComponent != null)
                            {
                                if (num8 == 1)
                                {
                                    num9 = (_hurt.target.handle.ValueComponent.actorHpTotal * num9) / 0x2710;
                                }
                                if (((_hurt.target.handle.ValueComponent.actorHp <= num9) && (_hurt.target.handle.ActorControl != null)) && ((Singleton<FrameSynchr>.instance.LogicFrameTick - _hurt.target.handle.ActorControl.lastExtraHurtByLowHpBuffTime) >= num10))
                                {
                                    _hurt.target.handle.ActorControl.lastExtraHurtByLowHpBuffTime = Singleton<FrameSynchr>.instance.LogicFrameTick;
                                    int num11 = skill.CustomParams[1];
                                    int num12 = skill.CustomParams[2];
                                    int num13 = skill.CustomParams[3];
                                    if (num8 == 1)
                                    {
                                        num11 = (num11 * _hurt.hurtValue) / 0x2710;
                                        num12 = (num12 * _hurt.adValue) / 0x2710;
                                        num13 = (num13 * _hurt.apValue) / 0x2710;
                                    }
                                    _hurt.hurtValue += num11;
                                    _hurt.adValue += num12;
                                    _hurt.apValue += num13;
                                }
                            }
                        }
                        if ((skill != null) && skill.FindSkillFunc(0x40, out outSkillFunc))
                        {
                            int num14 = skill.CustomParams[0];
                            int num15 = skill.CustomParams[1];
                            if (num14 == 1)
                            {
                                _hurt.attackInfo.iPhysicsHemophagiaRate += num15;
                                _hurt.attackInfo.iMagicHemophagiaRate += num15;
                            }
                            else
                            {
                                _hurt.attackInfo.iPhysicsHemophagia += num15;
                                _hurt.attackInfo.iMagicHemophagia += num15;
                            }
                        }
                    }
                }
            }
        }

        public void OnDead(PoolObjHandle<ActorRoot> _attack)
        {
            if (this.clearRule != null)
            {
                this.clearRule.CheckBuffNoClear(RES_SKILLFUNC_CLEAR_RULE.RES_SKILLFUNC_CLEAR_DEAD);
            }
            BuffSkill inBuff = null;
            ResDT_SkillFunc outSkillFunc = null;
            for (int i = 0; i < this.SpawnedBuffList.Count; i++)
            {
                inBuff = this.SpawnedBuffList[i];
                if ((inBuff != null) && inBuff.FindSkillFunc(0x20, out outSkillFunc))
                {
                    int reviveTime = inBuff.CustomParams[0];
                    int reviveLife = inBuff.CustomParams[1];
                    bool autoReset = inBuff.CustomParams[2] == 1;
                    bool bBaseRevive = inBuff.CustomParams[3] == 0;
                    bool bCDReset = inBuff.CustomParams[4] == 1;
                    base.actor.ActorControl.SetReviveContext(reviveTime, reviveLife, autoReset, bBaseRevive, bCDReset);
                    this.RemoveBuff(inBuff);
                }
                if ((((base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero) && (_attack != 0)) && ((_attack.handle.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero) && (inBuff != null))) && ((inBuff.cfgData != null) && (inBuff.cfgData.bIsInheritByKiller == 1)))
                {
                    this.RemoveBuff(inBuff);
                    SkillUseContext inContext = new SkillUseContext();
                    inContext.SetOriginator(_attack);
                    _attack.handle.SkillControl.SpawnBuff(_attack, inContext, inBuff.SkillID, true);
                }
            }
            this.OnDeadExtraEffect(_attack);
        }

        private void OnDeadExtraEffect(PoolObjHandle<ActorRoot> _attack)
        {
            BuffSkill skill = null;
            ResDT_SkillFunc outSkillFunc = null;
            if (_attack != 0)
            {
                for (int i = 0; i < _attack.handle.BuffHolderComp.SpawnedBuffList.Count; i++)
                {
                    skill = _attack.handle.BuffHolderComp.SpawnedBuffList[i];
                    if ((skill != null) && skill.FindSkillFunc(0x21, out outSkillFunc))
                    {
                        int inSkillCombineId = skill.CustomParams[0];
                        int num3 = skill.CustomParams[1];
                        int num4 = skill.CustomParams[2];
                        int typeMask = skill.CustomParams[3];
                        int typeSubMask = skill.CustomParams[4];
                        if ((num4 != 0) && this.CheckTargetSubType(typeMask, typeSubMask))
                        {
                            SkillUseContext inContext = new SkillUseContext();
                            inContext.SetOriginator(_attack);
                            _attack.handle.SkillControl.SpawnBuff(_attack, inContext, inSkillCombineId, true);
                        }
                    }
                }
            }
        }

        private int OnTargetExtraHurt(BuffSkill _buffSkill, PoolObjHandle<ActorRoot> _attack)
        {
            int num = 0;
            ResDT_SkillFunc outSkillFunc = null;
            if ((_buffSkill != null) && _buffSkill.FindSkillFunc(0x30, out outSkillFunc))
            {
                int typeMask = _buffSkill.CustomParams[0];
                int typeSubMask = _buffSkill.CustomParams[1];
                int num4 = _buffSkill.CustomParams[2];
                if (this.CheckTargetSubType(typeMask, typeSubMask))
                {
                    num = num4;
                }
            }
            return num;
        }

        public override void OnUse()
        {
            base.OnUse();
            this.SpawnedBuffList.Clear();
            this.overlayRule = null;
            this.clearRule = null;
            this.protectRule = null;
            this.changeSkillRule = null;
            this.markRule = null;
            this.logicEffect = null;
            this.bRemoveList = true;
            this.delBuffList.Clear();
        }

        public override void Reactive()
        {
            base.Reactive();
            this.overlayRule.Init(this);
            this.clearRule.Init(this);
            this.protectRule.Init(this);
            this.changeSkillRule.Init(this);
            this.markRule.Init(this);
        }

        public void RemoveBuff(BuffSkill inBuff)
        {
            if (this.SpawnedBuffList.Count != 0)
            {
                this.delBuffList = this.SpawnedBuffList;
                for (int i = 0; i < this.delBuffList.Count; i++)
                {
                    BuffSkill skill = this.delBuffList[i];
                    if (skill == inBuff)
                    {
                        BuffChangeEventParam param = new BuffChangeEventParam(false, base.actorPtr, inBuff);
                        Singleton<GameSkillEventSys>.GetInstance().SendEvent<BuffChangeEventParam>(GameSkillEventDef.Event_BuffChange, base.actorPtr, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                        skill.Stop();
                    }
                }
            }
        }

        public void RemoveBuff(int inSkillCombineId)
        {
            if (this.SpawnedBuffList.Count != 0)
            {
                this.delBuffList = this.SpawnedBuffList;
                for (int i = 0; i < this.delBuffList.Count; i++)
                {
                    BuffSkill skill = this.delBuffList[i];
                    if ((skill != null) && (skill.SkillID == inSkillCombineId))
                    {
                        skill.Stop();
                    }
                }
            }
        }

        public override void UpdateLogic(int nDelta)
        {
            if (this.markRule != null)
            {
                this.markRule.UpdateLogic(nDelta);
            }
        }
    }
}

