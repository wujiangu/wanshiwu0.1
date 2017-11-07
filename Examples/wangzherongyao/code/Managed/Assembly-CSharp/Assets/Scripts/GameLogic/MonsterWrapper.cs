namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameSystem;
    using ResData;
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public class MonsterWrapper : ObjWrapper
    {
        protected bool bCopyedHeroInfo;
        protected int BornTime;
        private static readonly long CheckDistance = 0xd693a40L;
        protected int DistanceTestCount;
        protected PoolObjHandle<ActorRoot> HostActor;
        private bool isBoss;
        protected int lifeTime;
        private int nOutCombatHpRecoveryTick;
        private Vector3 originalMeshScale = Vector3.one;
        protected VInt3 originalPos = VInt3.zero;
        protected SkillSlotType SpawnSkillSlot = SkillSlotType.SLOT_SKILL_VALID;

        public override void BeAttackHit(PoolObjHandle<ActorRoot> atker)
        {
            if (base.actor.IsSelfCamp(atker.handle))
            {
                return;
            }
            if (base.actorSubType == 2)
            {
                long iPursuitR = this.cfgInfo.iPursuitR;
                if (((base.actor.ValueComponent.actorHp * 0x2710) / base.actor.ValueComponent.actorHpTotal) <= 0x251c)
                {
                    VInt3 num2 = atker.handle.location - this.originalPos;
                    if (num2.sqrMagnitudeLong2D >= (iPursuitR * iPursuitR))
                    {
                        goto Label_00A7;
                    }
                }
                base.SetInBattle();
                base.m_isAttacked = true;
            }
            else
            {
                base.SetInBattle();
                base.m_isAttacked = true;
            }
        Label_00A7:
            atker.handle.ActorControl.SetInBattle();
            atker.handle.ActorControl.SetInAttack();
            DefaultGameEventParam prm = new DefaultGameEventParam(base.GetActor(), atker);
            Singleton<GameEventSys>.instance.SendEvent<DefaultGameEventParam>(GameEventDef.Event_ActorBeAttack, ref prm);
        }

        public override void Born(ActorRoot owner)
        {
            base.Born(owner);
            this.originalPos = base.actor.location;
            if (base.actor.ActorMesh != null)
            {
                this.originalMeshScale = base.actor.ActorMesh.transform.localScale;
            }
            base.actor.isMovable = base.actor.ObjLinker.CanMovable;
            base.actor.MovementComponent = base.actor.CreateLogicComponent<PlayerMovement>(base.actor);
            base.actor.MatHurtEffect = base.actor.CreateActorComponent<MaterialHurtEffect>(base.actor);
            base.actor.ShadowEffect = base.actor.CreateActorComponent<UpdateShadowPlane>(base.actor);
            VCollisionShape.InitActorCollision(base.actor);
            this.cfgInfo = MonsterDataHelper.GetDataCfgInfo(base.actor.TheActorMeta.ConfigId, base.actor.TheActorMeta.Difficuty);
            if ((this.cfgInfo != null) && (this.cfgInfo.bIsBoss > 0))
            {
                this.isBoss = true;
            }
            else
            {
                this.isBoss = false;
            }
            base.actorSubType = this.cfgInfo.bMonsterType;
            base.actorSubSoliderType = this.cfgInfo.bSoldierType;
            if (this.cfgInfo.iDropProbability == 0x65)
            {
                BattleMisc.BossRoot = base.actorPtr;
            }
        }

        public override void Deactive()
        {
            if (base.actor.ActorMesh != null)
            {
                base.actor.ActorMesh.transform.localScale = this.originalMeshScale;
            }
            this.nOutCombatHpRecoveryTick = 0;
            this.HostActor = new PoolObjHandle<ActorRoot>();
            this.spawnSkillSlot = SkillSlotType.SLOT_SKILL_VALID;
            this.bCopyedHeroInfo = false;
            this.DistanceTestCount = 0;
            base.Deactive();
        }

        public override PoolObjHandle<ActorRoot> GetOrignalActor()
        {
            if (this.isCalledMonster)
            {
                return this.HostActor;
            }
            return base.GetOrignalActor();
        }

        public override string GetTypeName()
        {
            return "MonsterWrapper";
        }

        public override void Init()
        {
            base.Init();
            this.SpawnSkillSlot = SkillSlotType.SLOT_SKILL_VALID;
        }

        protected override void InitDefaultState()
        {
            DebugHelper.Assert((base.actor != null) && (this.cfgInfo != null));
            this.BornTime = this.cfgInfo.iBornTime;
            if (this.BornTime > 0)
            {
                base.SetObjBehaviMode(ObjBehaviMode.State_Born);
                base.nextBehavior = ObjBehaviMode.State_Null;
            }
            else
            {
                base.InitDefaultState();
            }
        }

        public override bool IsBossOrHeroAutoAI()
        {
            return this.isBoss;
        }

        private void OnActorDead(ref DefaultGameEventParam prm)
        {
            if (prm.src == this.hostActor)
            {
                base.actor.Suicide();
                this.RemoveCheckDistanceTimer();
            }
        }

        protected override void OnBehaviModeChange(ObjBehaviMode oldState, ObjBehaviMode curState)
        {
            base.OnBehaviModeChange(oldState, curState);
            if (curState == ObjBehaviMode.State_Idle)
            {
                base.EnableRVO(false);
            }
        }

        private void OnCheckDistance(int seq)
        {
            if ((++this.DistanceTestCount >= 15) && (base.actor != null))
            {
                this.DistanceTestCount = 0;
                VInt3 num = this.HostActor.handle.location - base.actor.location;
                if (num.sqrMagnitudeLong2D >= CheckDistance)
                {
                    base.actor.location = this.HostActor.handle.location;
                }
            }
        }

        protected override void OnDead()
        {
            MonsterDropItemCreator creator = new MonsterDropItemCreator();
            if (base.myLastAtker != 0)
            {
                creator.MakeDropItemIfNeed(this, this.myLastAtker.handle.ActorControl);
            }
            else
            {
                creator.MakeDropItemIfNeed(this, null);
            }
            if (this.isCalledMonster)
            {
                this.RemoveCheckDistanceTimer();
            }
            base.OnDead();
            if (true)
            {
                Singleton<GameObjMgr>.instance.RecycleActor(base.actorPtr, this.cfgInfo.iRecyleTime);
            }
            if (this.isCalledMonster)
            {
                this.HostActor.handle.ActorControl.eventActorDead -= new ActorEventHandler(this.OnActorDead);
            }
        }

        protected override void OnRevive()
        {
            base.OnRevive();
            base.EnableRVO(true);
        }

        public override void OnUse()
        {
            base.OnUse();
            this.isBoss = false;
            this.nOutCombatHpRecoveryTick = 0;
            this.lifeTime = 0;
            this.BornTime = 0;
            this.cfgInfo = null;
            this.originalPos = VInt3.zero;
            this.originalMeshScale = Vector3.one;
            this.HostActor = new PoolObjHandle<ActorRoot>();
            this.spawnSkillSlot = SkillSlotType.SLOT_SKILL_VALID;
            this.bCopyedHeroInfo = false;
            this.DistanceTestCount = 0;
        }

        private void RemoveCheckDistanceTimer()
        {
        }

        public void SetHostActorInfo(ref PoolObjHandle<ActorRoot> InHostActor, SkillSlotType InSpawnSkillSlot, bool bInCopyedHeroInfo)
        {
            this.HostActor = InHostActor;
            this.SpawnSkillSlot = InSpawnSkillSlot;
            this.isDisplayHeroInfo = bInCopyedHeroInfo;
            if (this.HostActor != 0)
            {
                this.HostActor.handle.ActorControl.eventActorDead += new ActorEventHandler(this.OnActorDead);
            }
        }

        public override int TakeDamage(ref HurtDataInfo hurt)
        {
            if (base.IsBornState)
            {
                base.SetObjBehaviMode(ObjBehaviMode.State_Idle);
                base.nextBehavior = ObjBehaviMode.State_Null;
                base.PlayAnimation("Idle", 0f, 0, true);
            }
            return base.TakeDamage(ref hurt);
        }

        protected void UpdateBornLogic(int delta)
        {
            this.BornTime -= delta;
            if (this.BornTime <= 0)
            {
                base.InitDefaultState();
            }
        }

        public override void UpdateLogic(int delta)
        {
            base.actor.ActorAgent.UpdateLogic(delta);
            base.UpdateLogic(delta);
            if (base.IsBornState)
            {
                this.UpdateBornLogic(delta);
            }
            else
            {
                if (this.lifeTime > 0)
                {
                    this.lifeTime -= delta;
                    if (this.lifeTime <= 0)
                    {
                        base.SetObjBehaviMode(ObjBehaviMode.State_Dead);
                    }
                }
                if (base.actor.ActorControl.IsDeadState || (base.myBehavior != ObjBehaviMode.State_Idle))
                {
                    this.nOutCombatHpRecoveryTick = 0;
                }
                else
                {
                    this.nOutCombatHpRecoveryTick += delta;
                    if (this.nOutCombatHpRecoveryTick >= 0x3e8)
                    {
                        int nAddHp = (this.cfgInfo.iOutCombatHPAdd * base.actor.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP].totalValue) / 0x2710;
                        base.ReviveHp(nAddHp);
                        this.nOutCombatHpRecoveryTick -= 0x3e8;
                    }
                }
                if (this.isCalledMonster)
                {
                    this.OnCheckDistance(0);
                }
            }
        }

        public ResMonsterCfgInfo cfgInfo { get; protected set; }

        public override int CfgReviveCD
        {
            get
            {
                return 0x7fffffff;
            }
        }

        public PoolObjHandle<ActorRoot> hostActor
        {
            get
            {
                return this.HostActor;
            }
        }

        public bool isCalledMonster
        {
            get
            {
                return (bool) this.HostActor;
            }
        }

        public bool isDisplayHeroInfo
        {
            get
            {
                return this.bCopyedHeroInfo;
            }
            protected set
            {
                this.bCopyedHeroInfo = value;
            }
        }

        public int LifeTime
        {
            get
            {
                return this.lifeTime;
            }
            set
            {
                this.lifeTime = value;
            }
        }

        public SkillSlotType spawnSkillSlot
        {
            get
            {
                return this.SpawnSkillSlot;
            }
            protected set
            {
                this.SpawnSkillSlot = value;
            }
        }
    }
}

