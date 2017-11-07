namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using ResData;
    using System;

    public class PassiveEvent
    {
        protected ResSkillPassiveCfgInfo cfgData;
        protected ListView<PassiveCondition> conditions = new ListView<PassiveCondition>();
        protected int deltaTime;
        protected int[] localParams = new int[2];
        protected PassiveSkill passiveSkill;
        protected PoolObjHandle<ActorRoot> sourceActor;
        protected PoolObjHandle<ActorRoot> triggerActor = new PoolObjHandle<ActorRoot>(null);

        public void AddCondition(PassiveCondition _condition)
        {
            this.conditions.Add(_condition);
        }

        protected bool Fit()
        {
            if ((this.conditions.Count == 0) && (this.deltaTime <= 0))
            {
                return true;
            }
            for (int i = 0; i < this.conditions.Count; i++)
            {
                PassiveCondition condition = this.conditions[i];
                if (condition.Fit() && (this.deltaTime <= 0))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual int GetCDTime()
        {
            return this.deltaTime;
        }

        public virtual void Init(PoolObjHandle<ActorRoot> _actor, PassiveSkill _skill)
        {
            this.sourceActor = _actor;
            this.passiveSkill = _skill;
            this.cfgData = _skill.cfgData;
            this.deltaTime = 0;
            for (int i = 0; i < this.conditions.Count; i++)
            {
                this.conditions[i].Init(this.sourceActor, this, ref this.cfgData.astPassiveConditon[i]);
            }
            this.SetEventParam();
        }

        public virtual void InitCDTime(int _cdTime)
        {
            this.deltaTime = _cdTime;
        }

        protected void Reset()
        {
            for (int i = 0; i < this.conditions.Count; i++)
            {
                this.conditions[i].Reset();
            }
        }

        private void SetEventParam()
        {
            this.localParams[0] = this.cfgData.iPassiveEventParam1;
            this.localParams[1] = this.cfgData.iPassiveEventParam2;
        }

        public void SetTriggerActor(PoolObjHandle<ActorRoot> _actor)
        {
            this.triggerActor = _actor;
        }

        protected void Trigger()
        {
            SkillUseContext context = new SkillUseContext(this.passiveSkill.SlotType);
            context.SetOriginator(this.sourceActor);
            if (this.triggerActor == 0)
            {
                context.TargetActor = this.sourceActor;
            }
            else
            {
                context.TargetActor = this.triggerActor;
            }
            this.passiveSkill.Use(this.sourceActor, context);
            this.deltaTime = this.cfgData.iCoolDown;
        }

        public virtual void UnInit()
        {
            for (int i = 0; i < this.conditions.Count; i++)
            {
                this.conditions[i].UnInit();
            }
        }

        public virtual void UpdateLogic(int _delta)
        {
            if (this.deltaTime > 0)
            {
                this.deltaTime -= _delta;
                this.deltaTime = (this.deltaTime <= 0) ? 0 : this.deltaTime;
            }
        }
    }
}

