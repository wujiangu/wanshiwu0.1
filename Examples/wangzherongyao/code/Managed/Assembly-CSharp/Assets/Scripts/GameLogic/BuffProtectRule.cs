namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using ResData;
    using System;
    using System.Collections.Generic;

    public class BuffProtectRule
    {
        private List<PoolObjHandle<BuffSkill>> AllProtectBuffList = new List<PoolObjHandle<BuffSkill>>();
        private BuffHolderComponent buffHolder;
        private BuffLimiteHurt limiteMaxHpHurt;
        private List<PoolObjHandle<BuffSkill>> MagicProtectList = new List<PoolObjHandle<BuffSkill>>();
        private List<PoolObjHandle<BuffSkill>> NoHurtBuffList = new List<PoolObjHandle<BuffSkill>>();
        private List<PoolObjHandle<BuffSkill>> PhysicsProtectList = new List<PoolObjHandle<BuffSkill>>();
        private int protectValue;

        public void AddBuff(BuffSkill inBuff)
        {
            if (inBuff.cfgData.dwEffectSubType == 1)
            {
                this.PhysicsProtectList.Add(new PoolObjHandle<BuffSkill>(inBuff));
            }
            else if (inBuff.cfgData.dwEffectSubType == 2)
            {
                this.MagicProtectList.Add(new PoolObjHandle<BuffSkill>(inBuff));
            }
            else if (inBuff.cfgData.dwEffectSubType == 3)
            {
                this.AllProtectBuffList.Add(new PoolObjHandle<BuffSkill>(inBuff));
            }
            else if (((inBuff.cfgData.dwEffectSubType == 4) || (inBuff.cfgData.dwEffectSubType == 5)) || (inBuff.cfgData.dwEffectSubType == 6))
            {
                this.NoHurtBuffList.Add(new PoolObjHandle<BuffSkill>(inBuff));
            }
        }

        private bool CheckTargetNoDamage(ref HurtDataInfo _hurt, BuffSkill _buffSkill)
        {
            int num = _buffSkill.CustomParams[0];
            int num2 = _buffSkill.CustomParams[1];
            if (num == 0)
            {
                return true;
            }
            if (_hurt.atker != 0)
            {
                int actorType = (int) _hurt.atker.handle.TheActorMeta.ActorType;
                if ((num & (((int) 1) << actorType)) > 0)
                {
                    if (actorType != 1)
                    {
                        return true;
                    }
                    if (num2 == 0)
                    {
                        return true;
                    }
                    if (_hurt.atker.handle.ActorControl.GetActorSubType() == num2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void ClearBuff()
        {
            this.ClearProtectBuff(this.PhysicsProtectList);
            this.ClearProtectBuff(this.MagicProtectList);
            this.ClearProtectBuff(this.AllProtectBuffList);
            this.NoHurtBuffList.Clear();
        }

        private void ClearProtectBuff(List<PoolObjHandle<BuffSkill>> _inList)
        {
            if (_inList.Count != 0)
            {
                PoolObjHandle<BuffSkill>[] handleArray = _inList.ToArray();
                for (int i = 0; i < handleArray.Length; i++)
                {
                    this.RemoveBuff(ref handleArray[i]);
                }
            }
        }

        public void Init(BuffHolderComponent _buffHolder)
        {
            this.protectValue = 0;
            this.buffHolder = _buffHolder;
            this.limiteMaxHpHurt.bValid = false;
        }

        private bool NoDamageImpl(ref HurtDataInfo _hurt)
        {
            BuffSkill skill = null;
            for (int i = 0; i < this.NoHurtBuffList.Count; i++)
            {
                skill = this.NoHurtBuffList[i];
                if (skill != null)
                {
                    if (skill.cfgData.dwEffectSubType == 6)
                    {
                        if (this.CheckTargetNoDamage(ref _hurt, skill))
                        {
                            return true;
                        }
                    }
                    else if (_hurt.hurtType == HurtTypeDef.PhysHurt)
                    {
                        if ((skill.cfgData.dwEffectSubType == 4) && this.CheckTargetNoDamage(ref _hurt, skill))
                        {
                            return true;
                        }
                    }
                    else if (((_hurt.hurtType == HurtTypeDef.MagicHurt) && (skill.cfgData.dwEffectSubType == 5)) && this.CheckTargetNoDamage(ref _hurt, skill))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void RemoveBuff(ref PoolObjHandle<BuffSkill> inBuff)
        {
            if (inBuff.handle.cfgData.dwEffectSubType == 1)
            {
                this.SendProtectEvent(0, -inBuff.handle.CustomParams[0]);
                this.PhysicsProtectList.Remove(inBuff);
            }
            else if (inBuff.handle.cfgData.dwEffectSubType == 2)
            {
                this.SendProtectEvent(1, -inBuff.handle.CustomParams[1]);
                this.MagicProtectList.Remove(inBuff);
            }
            else if (inBuff.handle.cfgData.dwEffectSubType == 3)
            {
                this.SendProtectEvent(2, -inBuff.handle.CustomParams[2]);
                this.AllProtectBuffList.Remove(inBuff);
            }
            else if (((inBuff.handle.cfgData.dwEffectSubType == 4) || (inBuff.handle.cfgData.dwEffectSubType == 5)) || (inBuff.handle.cfgData.dwEffectSubType == 6))
            {
                this.NoHurtBuffList.Remove(inBuff);
            }
        }

        public int ResistDamage(ref HurtDataInfo _hurt, int _hurtValue)
        {
            int changeValue = 0;
            int num2 = _hurtValue;
            if (_hurtValue > 0)
            {
                if (this.NoDamageImpl(ref _hurt))
                {
                    this.SendHurtImmuneEvent(_hurt.atker);
                    return 0;
                }
                if (_hurt.hurtType == HurtTypeDef.PhysHurt)
                {
                    changeValue = _hurtValue;
                    _hurtValue = this.ResistProtectImpl(_hurtValue, this.PhysicsProtectList, 0);
                    changeValue -= _hurtValue;
                    this.SendProtectEvent(0, -changeValue);
                    if (_hurtValue > 0)
                    {
                        changeValue = _hurtValue;
                        _hurtValue = this.ResistProtectImpl(_hurtValue, this.AllProtectBuffList, 2);
                        changeValue -= _hurtValue;
                        this.SendProtectEvent(2, -changeValue);
                    }
                }
                else if (_hurt.hurtType == HurtTypeDef.MagicHurt)
                {
                    changeValue = _hurtValue;
                    _hurtValue = this.ResistProtectImpl(_hurtValue, this.MagicProtectList, 1);
                    changeValue -= _hurtValue;
                    this.SendProtectEvent(1, -changeValue);
                    if (_hurtValue > 0)
                    {
                        changeValue = _hurtValue;
                        _hurtValue = this.ResistProtectImpl(_hurtValue, this.AllProtectBuffList, 2);
                        changeValue -= _hurtValue;
                        this.SendProtectEvent(2, -changeValue);
                    }
                }
                changeValue = num2 - _hurtValue;
                this.SendHurtAbsorbEvent(_hurt.atker, changeValue);
                if (this.limiteMaxHpHurt.bValid)
                {
                    int num3 = (this.buffHolder.actor.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP].totalValue * this.limiteMaxHpHurt.hurtRate) / 0x2710;
                    if (_hurtValue > num3)
                    {
                        _hurtValue = num3;
                    }
                }
                _hurtValue = this.ResistDeadDamage(ref _hurt, _hurtValue);
            }
            return _hurtValue;
        }

        private int ResistDeadDamage(ref HurtDataInfo _hurt, int _hurtValue)
        {
            BuffSkill inBuff = null;
            ResDT_SkillFunc outSkillFunc = null;
            if ((this.buffHolder != null) && (this.buffHolder.actor != null))
            {
                ActorRoot actor = this.buffHolder.actor;
                for (int i = 0; i < actor.BuffHolderComp.SpawnedBuffList.Count; i++)
                {
                    inBuff = actor.BuffHolderComp.SpawnedBuffList[i];
                    if ((inBuff != null) && inBuff.FindSkillFunc(0x36, out outSkillFunc))
                    {
                        int inSkillCombineId = inBuff.CustomParams[0];
                        if (inBuff.CustomParams[1] == 0)
                        {
                            if (actor.ValueComponent.actorHp <= _hurtValue)
                            {
                                SkillUseContext inContext = new SkillUseContext();
                                inContext.SetOriginator(_hurt.atker);
                                actor.SkillControl.SpawnBuff(actor.SelfPtr, inContext, inSkillCombineId, true);
                                this.buffHolder.RemoveBuff(inBuff);
                                DefaultGameEventParam prm = new DefaultGameEventParam(this.buffHolder.actorPtr, _hurt.atker);
                                Singleton<GameEventSys>.instance.SendEvent<DefaultGameEventParam>(GameEventDef.Event_ActorImmuneDeadHurt, ref prm);
                                _hurtValue = 0;
                            }
                        }
                        else
                        {
                            SkillUseContext context2 = new SkillUseContext();
                            context2.SetOriginator(_hurt.atker);
                            actor.SkillControl.SpawnBuff(actor.SelfPtr, context2, inSkillCombineId, true);
                            this.buffHolder.RemoveBuff(inBuff);
                            _hurtValue = 0;
                        }
                    }
                    if (((_hurt.atkSlot == SkillSlotType.SLOT_SKILL_0) && (inBuff != null)) && inBuff.FindSkillFunc(0x43, out outSkillFunc))
                    {
                        int num4 = inBuff.CustomParams[0];
                        int num5 = inBuff.CustomParams[4];
                        switch (num4)
                        {
                            case 1:
                                _hurtValue = (_hurtValue * (0x2710 - num5)) / 0x2710;
                                break;

                            case 0:
                                _hurtValue -= num5;
                                break;
                        }
                    }
                }
            }
            return _hurtValue;
        }

        private int ResistProtectImpl(int _hurtValue, List<PoolObjHandle<BuffSkill>> _inList, int _index)
        {
            if (_inList.Count != 0)
            {
                PoolObjHandle<BuffSkill>[] handleArray = _inList.ToArray();
                for (int i = 0; i < handleArray.Length; i++)
                {
                    BuffSkill handle = handleArray[i].handle;
                    if (handle.CustomParams[_index] > _hurtValue)
                    {
                        handle.CustomParams[_index] -= _hurtValue;
                        return 0;
                    }
                    _hurtValue -= handle.CustomParams[_index];
                    handle.CustomParams[_index] = 0;
                    this.buffHolder.RemoveBuff(handle);
                    _inList.Remove(handleArray[i]);
                }
            }
            return _hurtValue;
        }

        private void SendHurtAbsorbEvent(PoolObjHandle<ActorRoot> atker, int changeValue)
        {
            if ((changeValue > 0) && (this.protectValue != 0))
            {
                DefaultGameEventParam prm = new DefaultGameEventParam(this.buffHolder.actorPtr, atker);
                Singleton<GameEventSys>.instance.SendEvent<DefaultGameEventParam>(GameEventDef.Event_ActorHurtAbsorb, ref prm);
            }
        }

        private void SendHurtImmuneEvent(PoolObjHandle<ActorRoot> atker)
        {
            DefaultGameEventParam prm = new DefaultGameEventParam(this.buffHolder.actorPtr, atker);
            Singleton<GameEventSys>.instance.SendEvent<DefaultGameEventParam>(GameEventDef.Event_ActorImmune, ref prm);
        }

        public void SendProtectEvent(int type, int changeValue)
        {
            if (changeValue != 0)
            {
                this.protectValue += changeValue;
                this.buffHolder.actor.ActorControl.OnShieldChange(type, changeValue);
                if (this.protectValue == 0)
                {
                    ActorSkillEventParam param = new ActorSkillEventParam(this.buffHolder.actorPtr, SkillSlotType.SLOT_SKILL_0);
                    Singleton<GameSkillEventSys>.GetInstance().SendEvent<ActorSkillEventParam>(GameSkillEventDef.Event_ProtectDisappear, this.buffHolder.actorPtr, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                }
            }
        }

        public void SetLimiteMaxHurt(bool _bOpen, int _value)
        {
            this.limiteMaxHpHurt.bValid = _bOpen;
            this.limiteMaxHpHurt.hurtRate = _value;
        }
    }
}

