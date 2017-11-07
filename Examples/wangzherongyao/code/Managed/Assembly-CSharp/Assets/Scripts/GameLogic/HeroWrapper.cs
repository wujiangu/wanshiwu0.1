namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.DataCenter;
    using Assets.Scripts.GameLogic.GameKernal;
    using Assets.Scripts.GameSystem;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class HeroWrapper : ObjWrapper
    {
        private bool autoRevived;
        public bool bGodMode;
        private int contiDeadNum;
        private int contiKillNum;
        private HeroProficiency m_heroProficiency;
        private uint m_skinCfgId;
        private uint m_skinId;
        private uint[] m_talentArr = new uint[5];
        private int multiKillNum;

        public override void AddDisableSkillFlag(SkillSlotType _type)
        {
            base.AddDisableSkillFlag(_type);
            if (_type == SkillSlotType.SLOT_SKILL_COUNT)
            {
                for (int i = 0; i < 7; i++)
                {
                    if (base.actor.SkillControl.DisableSkill[i] == 1)
                    {
                        DefaultSkillEventParam param = new DefaultSkillEventParam((SkillSlotType) i, 0);
                        Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_LimitSkill, base.GetActor(), ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                    }
                }
            }
            else if (base.actor.SkillControl.DisableSkill[(int) _type] == 1)
            {
                DefaultSkillEventParam param2 = new DefaultSkillEventParam(_type, 0);
                Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_LimitSkill, base.GetActor(), ref param2, GameSkillEventChannel.Channel_HostCtrlActor);
            }
        }

        public override void Born(ActorRoot owner)
        {
            base.Born(owner);
            base.actor.MovementComponent = base.actor.CreateLogicComponent<PlayerMovement>(base.actor);
            base.actor.MatHurtEffect = base.actor.CreateActorComponent<MaterialHurtEffect>(base.actor);
            base.actor.EffectControl = base.actor.CreateLogicComponent<EffectPlayComponent>(base.actor);
            base.actor.EquipComponent = base.actor.CreateLogicComponent<EquipComponent>(base.actor);
            base.actor.ShadowEffect = base.actor.CreateActorComponent<UpdateShadowPlane>(base.actor);
            VCollisionShape.InitActorCollision(base.actor);
            base.actor.DefaultAttackModeControl = base.actor.CreateLogicComponent<DefaultAttackMode>(base.actor);
            base.actor.LockTargetAttackModeControl = base.actor.CreateLogicComponent<LockTargetAttackMode>(base.actor);
            this.m_heroProficiency = new HeroProficiency();
            this.m_heroProficiency.Init(this);
        }

        public override void CmdCommonLearnSkill(IFrameCommand cmd)
        {
            FrameCommand<LearnSkillCommand> command = (FrameCommand<LearnSkillCommand>) cmd;
            if (Singleton<CBattleSystem>.instance.IsMatchLearnSkillRule(base.actorPtr, (SkillSlotType) command.cmdData.bSlotType) && (base.actor.SkillControl.m_iSkillPoint > 0))
            {
                base.actor.SkillControl.m_iSkillPoint--;
            }
            else
            {
                return;
            }
            PoolObjHandle<ActorRoot> actorPtr = base.actorPtr;
            if (base.actor.SkillControl.m_iSkillPoint >= 0)
            {
                SkillSlot slot;
                if (command.cmdData.bSkillLevel == 0)
                {
                    int num;
                    if (base.actor.BuffHolderComp.changeSkillRule.GetChangeSkillSlot(command.cmdData.bSlotType, out num))
                    {
                        actorPtr.handle.SkillControl.InitSkillSlot(command.cmdData.bSlotType, num, 0);
                    }
                    else
                    {
                        IGameActorDataProvider actorDataProvider = Singleton<ActorDataCenter>.instance.GetActorDataProvider(GameActorDataProviderType.StaticBattleDataProvider);
                        ActorStaticSkillData skillData = new ActorStaticSkillData();
                        if (actorDataProvider.GetActorStaticSkillData(ref base.actor.TheActorMeta, (ActorSkillSlot) command.cmdData.bSlotType, ref skillData))
                        {
                            actorPtr.handle.SkillControl.InitSkillSlot(command.cmdData.bSlotType, skillData.SkillId, skillData.PassiveSkillId);
                        }
                    }
                }
                actorPtr.handle.SkillControl.TryGetSkillSlot((SkillSlotType) command.cmdData.bSlotType, out slot);
                if (slot != null)
                {
                    int skillLevel = slot.GetSkillLevel();
                    if (skillLevel == command.cmdData.bSkillLevel)
                    {
                        slot.SetSkillLevel(skillLevel + 1);
                        Singleton<EventRouter>.GetInstance().BroadCastEvent<PoolObjHandle<ActorRoot>, byte, byte>("HeroSkillLevelUp", actorPtr, command.cmdData.bSlotType, (byte) (skillLevel + 1));
                    }
                }
            }
        }

        public override void CmdCommonLearnTalent(IFrameCommand cmd)
        {
        }

        public override void Deactive()
        {
            if (this.m_heroProficiency != null)
            {
                this.m_heroProficiency.UnInit();
            }
            this.m_heroProficiency = null;
            base.Deactive();
        }

        public override void Fight()
        {
            base.Fight();
            if (ActorHelper.IsCaptainActor(ref this.actorPtr))
            {
                base.m_isControledByMan = true;
                base.m_isAutoAI = false;
            }
            else
            {
                base.m_isControledByMan = false;
                base.m_isAutoAI = true;
            }
            IGameActorDataProvider actorDataProvider = Singleton<ActorDataCenter>.instance.GetActorDataProvider(GameActorDataProviderType.ServerDataProvider);
            ActorServerData actorData = new ActorServerData();
            if ((actorDataProvider != null) && actorDataProvider.GetActorServerData(ref base.actor.TheActorMeta, ref actorData))
            {
                this.m_skinId = actorData.SkinId;
                this.m_skinCfgId = CSkinInfo.GetSkinCfgId((uint) base.actor.TheActorMeta.ConfigId, this.m_skinId);
                if (this.m_skinId != 0)
                {
                    ResHeroSkin heroSkin = CSkinInfo.GetHeroSkin((uint) base.actor.TheActorMeta.ConfigId, this.m_skinId);
                    if ((heroSkin != null) && !string.IsNullOrEmpty(heroSkin.szSoundSwitchEvent))
                    {
                        Singleton<CSoundManager>.instance.PostEvent(heroSkin.szSoundSwitchEvent, base.actor.gameObject);
                    }
                }
            }
            base.EnableRVO(false);
        }

        public bool GetSkinCfgID(out uint skinCfgID)
        {
            skinCfgID = this.m_skinCfgId;
            return (this.m_skinId != 0);
        }

        public override string GetTypeName()
        {
            return "HeroWrapper";
        }

        public override bool IsBossOrHeroAutoAI()
        {
            return (base.myBehavior == ObjBehaviMode.State_AutoAI);
        }

        protected override void OnDead()
        {
            base.OnDead();
            for (int i = 0; i < base.hurtSelfActorList.Count; i++)
            {
                KeyValuePair<uint, ulong> pair = base.hurtSelfActorList[i];
                PoolObjHandle<ActorRoot> actor = Singleton<GameObjMgr>.GetInstance().GetActor(pair.Key);
                if (actor != 0)
                {
                    base.NotifyAssistActor(ref actor);
                }
            }
            this.contiKillNum = 0;
            this.contiDeadNum++;
            base.actor.SkillControl.SkillUseCache.Clear();
        }

        protected override void OnRevive()
        {
            VInt num3;
            VInt3 zero = VInt3.zero;
            VInt3 forward = VInt3.forward;
            if (this.autoRevived && this.m_reviveContext.bBaseRevive)
            {
                Singleton<BattleLogic>.GetInstance().mapLogic.GetRevivePosDir(ref base.actor.TheActorMeta, false, out zero, out forward);
                base.actor.EquipComponent.ResetHasLeftEquipBoughtArea();
            }
            else
            {
                Player player = ActorHelper.GetOwnerPlayer(ref this.actorPtr);
                zero = player.Captain.handle.location;
                forward = player.Captain.handle.forward;
            }
            if (PathfindingUtility.GetGroundY(zero, out num3))
            {
                base.actor.groundY = num3;
                zero.y = num3.i;
            }
            base.actor.forward = forward;
            base.actor.location = zero;
            base.actor.ObjLinker.SetForward(forward, -1);
            base.OnRevive();
            Player ownerPlayer = ActorHelper.GetOwnerPlayer(ref this.actorPtr);
            if (ownerPlayer != null)
            {
                Singleton<EventRouter>.instance.BroadCastEvent<Player>(EventID.PlayerReviveTime, ownerPlayer);
            }
        }

        public override void OnUse()
        {
            base.OnUse();
            if (this.m_heroProficiency != null)
            {
                this.m_heroProficiency.UnInit();
            }
            this.m_heroProficiency = null;
            this.multiKillNum = 0;
            this.contiKillNum = 0;
            this.contiDeadNum = 0;
            this.bGodMode = false;
            this.autoRevived = false;
            for (byte i = 0; i < this.m_talentArr.Length; i = (byte) (i + 1))
            {
                this.m_talentArr[i] = 0;
            }
            this.m_skinCfgId = 0;
            this.m_skinId = 0;
        }

        public override void Revive(bool auto)
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
            bool flag = (curLvelContext != null) && curLvelContext.isPVPMode;
            if (flag == auto)
            {
                this.autoRevived = auto;
                base.Revive(auto);
            }
        }

        public override void RmvDisableSkillFlag(SkillSlotType _type)
        {
            base.RmvDisableSkillFlag(_type);
            if (_type == SkillSlotType.SLOT_SKILL_COUNT)
            {
                for (int i = 0; i < 7; i++)
                {
                    if (base.actor.SkillControl.DisableSkill[i] == 0)
                    {
                        DefaultSkillEventParam param = new DefaultSkillEventParam((SkillSlotType) i, 0);
                        Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_CancelLimitSkill, base.GetActor(), ref param, GameSkillEventChannel.Channel_HostCtrlActor);
                    }
                }
            }
            else if (base.actor.SkillControl.DisableSkill[(int) _type] == 0)
            {
                DefaultSkillEventParam param2 = new DefaultSkillEventParam(_type, 0);
                Singleton<GameSkillEventSys>.GetInstance().SendEvent<DefaultSkillEventParam>(GameSkillEventDef.Event_CancelLimitSkill, base.GetActor(), ref param2, GameSkillEventChannel.Channel_HostCtrlActor);
            }
        }

        public override int TakeDamage(ref HurtDataInfo hurt)
        {
            if (this.bGodMode)
            {
                return 0;
            }
            if (((Singleton<BattleLogic>.instance.GetCurLvelContext() != null) && (hurt.atker != 0)) && (hurt.atker.handle.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Organ))
            {
                OrganWrapper wrapper = hurt.atker.handle.AsOrgan();
                if (wrapper != null)
                {
                    int attackCounter = wrapper.GetAttackCounter(base.actorPtr);
                    if (attackCounter > 1)
                    {
                        int iContiAttakMax = (attackCounter - 1) * wrapper.cfgInfo.iContiAttakAdd;
                        if (iContiAttakMax > wrapper.cfgInfo.iContiAttakMax)
                        {
                            iContiAttakMax = wrapper.cfgInfo.iContiAttakMax;
                        }
                        hurt.adValue += iContiAttakMax;
                    }
                }
            }
            return base.TakeDamage(ref hurt);
        }

        public override void UpdateLogic(int nDelta)
        {
            base.actor.ActorAgent.UpdateLogic(nDelta);
            base.UpdateLogic(nDelta);
            this.m_heroProficiency.UpdateLogic(nDelta);
        }

        public override int CfgReviveCD
        {
            get
            {
                SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
                return Singleton<BattleLogic>.instance.dynamicProperty.GetDynamicReviveTime(curLvelContext.dynamicPropertyConfig, curLvelContext.baseReviveTime);
            }
        }

        public int ContiDeadNum
        {
            get
            {
                return this.contiDeadNum;
            }
            set
            {
                this.contiDeadNum = value;
            }
        }

        public int ContiKillNum
        {
            get
            {
                return this.contiKillNum;
            }
            set
            {
                this.contiKillNum = value;
            }
        }

        public uint[] GetTalentArr
        {
            get
            {
                return this.m_talentArr;
            }
        }

        public int MultiKillNum
        {
            get
            {
                return this.multiKillNum;
            }
            set
            {
                this.multiKillNum = value;
            }
        }
    }
}

