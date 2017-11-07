namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using ResData;
    using System;

    [PassiveEvent(PassiveEventType.DeadHurtPassiveEvent)]
    public class DeadHurtPassiveEvent : PassiveEvent
    {
        private bool bExistBuff;

        public override void Init(PoolObjHandle<ActorRoot> _actor, PassiveSkill _skill)
        {
            base.deltaTime = 0;
            this.bExistBuff = false;
            base.Init(_actor, _skill);
        }

        public override void UpdateLogic(int _delta)
        {
            if (!this.bExistBuff)
            {
                base.UpdateLogic(_delta);
                if (base.deltaTime <= 0)
                {
                    base.Trigger();
                    base.Reset();
                    this.bExistBuff = true;
                    base.deltaTime = 0;
                }
            }
            for (int i = 0; i < base.conditions.Count; i++)
            {
                PassiveCondition condition = base.conditions[i];
                if (condition.Fit() && this.bExistBuff)
                {
                    base.Reset();
                    this.bExistBuff = false;
                    base.deltaTime = base.cfgData.iCoolDown;
                    return;
                }
            }
        }
    }
}

