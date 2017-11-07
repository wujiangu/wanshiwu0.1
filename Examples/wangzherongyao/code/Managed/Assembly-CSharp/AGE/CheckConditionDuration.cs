namespace AGE
{
    using Assets.Scripts.Common;
    using System;

    [EventCategory("MMGame/Skill")]
    public class CheckConditionDuration : DurationCondition
    {
        private bool bCondition;
        public bool bHitTargetHero;
        public bool bTriggerBullet;
        public int trackId = -1;

        public override bool Check(Action _action, Track _track)
        {
            return this.bCondition;
        }

        public override BaseEvent Clone()
        {
            CheckConditionDuration duration = ClassObjPool<CheckConditionDuration>.Get();
            duration.CopyData(this);
            return duration;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            CheckConditionDuration duration = src as CheckConditionDuration;
            this.trackId = duration.trackId;
            this.bHitTargetHero = duration.bHitTargetHero;
            this.bTriggerBullet = duration.bTriggerBullet;
        }

        public override void OnUse()
        {
            base.OnUse();
            this.trackId = -1;
            this.bCondition = false;
        }

        public override void Process(Action _action, Track _track, int _localTime)
        {
            if (this.bHitTargetHero)
            {
                _action.refParams.GetRefParam("_HitTargetHero", ref this.bCondition);
            }
            else if (this.bTriggerBullet)
            {
                _action.refParams.GetRefParam("_TriggerBullet", ref this.bCondition);
            }
            base.Process(_action, _track, _localTime);
        }
    }
}

