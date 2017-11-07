namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using ResData;
    using System;

    [PassiveCondition(PassiveConditionType.BeKilledPassiveCondition)]
    public class PassiveBeKilledCondition : PassiveCondition
    {
        private bool bKilled;

        public override bool Fit()
        {
            return this.bKilled;
        }

        public override void Init(PoolObjHandle<ActorRoot> _source, PassiveEvent _event, ref ResDT_SkillPassiveCondition _config)
        {
            this.bKilled = false;
            base.Init(_source, _event, ref _config);
            this.sourceActor.handle.ActorControl.eventActorDead += new ActorEventHandler(this.onActorDead);
        }

        private void onActorDead(ref DefaultGameEventParam prm)
        {
            if (prm.src == base.sourceActor)
            {
                this.bKilled = true;
                base.rootEvent.SetTriggerActor(prm.orignalAtker);
            }
        }

        public override void Reset()
        {
            this.bKilled = false;
        }

        public override void UnInit()
        {
            if (base.sourceActor != 0)
            {
                this.sourceActor.handle.ActorControl.eventActorDead -= new ActorEventHandler(this.onActorDead);
            }
        }
    }
}

