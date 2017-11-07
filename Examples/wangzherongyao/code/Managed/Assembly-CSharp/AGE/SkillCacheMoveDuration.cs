namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using System;

    [EventCategory("MMGame/Skill")]
    internal class SkillCacheMoveDuration : DurationCondition
    {
        private bool bInit;
        private bool bStart;
        private int lastTime;
        public int moveSpeed;
        private SkillComponent skillControl;
        private PoolObjHandle<ActorRoot> targetActor;
        [ObjectTemplate(new Type[] {  })]
        public int targetId;

        public override bool Check(Action _action, Track _track)
        {
            return this.bStart;
        }

        public override BaseEvent Clone()
        {
            SkillCacheMoveDuration duration = ClassObjPool<SkillCacheMoveDuration>.Get();
            duration.CopyData(this);
            return duration;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            SkillCacheMoveDuration duration = src as SkillCacheMoveDuration;
            this.targetId = duration.targetId;
            this.moveSpeed = duration.moveSpeed;
        }

        public override void Enter(Action _action, Track _track)
        {
            this.targetActor = _action.GetActorHandle(this.targetId);
            if (this.targetActor != 0)
            {
                this.skillControl = this.targetActor.handle.SkillControl;
                if ((this.skillControl != null) && (this.skillControl.SkillUseCache != null))
                {
                    this.bInit = true;
                    if (((this.moveSpeed > 0) && !this.skillControl.SkillUseCache.GetCacheMoveExpire()) && (this.skillControl.CurUseSkill != null))
                    {
                        this.skillControl.CurUseSkill.skillAbort.InitAbort(false);
                        this.skillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_1);
                        this.skillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_2);
                        this.skillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_3);
                        this.skillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_4);
                        this.bStart = true;
                    }
                }
            }
        }

        public override void Leave(Action _action, Track _track)
        {
            if (this.bInit)
            {
                if (((this.moveSpeed > 0) && !this.skillControl.SkillUseCache.GetCacheMoveExpire()) && (this.skillControl.CurUseSkill != null))
                {
                    this.skillControl.CurUseSkill.skillAbort.InitAbort(true);
                }
                this.skillControl.SkillUseCache.SetCacheMoveExpire(true);
            }
        }

        public override void OnUse()
        {
            base.OnUse();
            this.bInit = false;
            this.bStart = false;
            this.lastTime = 0;
            this.skillControl = null;
            this.targetActor.Release();
        }

        public override void Process(Action _action, Track _track, int _localTime)
        {
            base.Process(_action, _track, _localTime);
            if (this.bInit)
            {
                int num = _localTime - this.lastTime;
                this.lastTime = _localTime;
                this.skillControl.SkillUseCache.UseSkillCacheMove(this.targetActor, num, this.moveSpeed);
                if (((this.moveSpeed > 0) && !this.skillControl.SkillUseCache.GetCacheMoveExpire()) && (this.skillControl.CurUseSkill != null))
                {
                    this.skillControl.CurUseSkill.skillAbort.InitAbort(false);
                    this.skillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_1);
                    this.skillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_2);
                    this.skillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_3);
                    this.skillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_4);
                }
            }
        }
    }
}

