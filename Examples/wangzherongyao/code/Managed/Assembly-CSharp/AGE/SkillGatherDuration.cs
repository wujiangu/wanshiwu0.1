namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using System;

    [EventCategory("MMGame/Skill")]
    public class SkillGatherDuration : DurationEvent
    {
        private PoolObjHandle<ActorRoot> actorObj;
        private int lastTime;
        [ObjectTemplate(new Type[] {  })]
        public int targetId;
        public int triggerRadius;
        public int triggerTime;

        public override BaseEvent Clone()
        {
            SkillGatherDuration duration = ClassObjPool<SkillGatherDuration>.Get();
            duration.CopyData(this);
            return duration;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            SkillGatherDuration duration = src as SkillGatherDuration;
            this.targetId = duration.targetId;
            this.triggerTime = duration.triggerTime;
            this.triggerRadius = duration.triggerRadius;
        }

        public override void Enter(Action _action, Track _track)
        {
            base.Enter(_action, _track);
            this.actorObj = _action.GetActorHandle(this.targetId);
            this.lastTime = 0;
        }

        public override void Leave(Action _action, Track _track)
        {
            this.TriggerBullet();
            base.Leave(_action, _track);
        }

        public override void OnUse()
        {
            base.OnUse();
            this.targetId = 0;
            this.lastTime = 0;
            this.actorObj.Release();
        }

        public override void Process(Action _action, Track _track, int _localTime)
        {
            this.lastTime = _localTime;
            base.Process(_action, _track, _localTime);
        }

        public override bool SupportEditMode()
        {
            return true;
        }

        private void TriggerBullet()
        {
            BulletSkill skill = null;
            int count = this.actorObj.handle.SkillControl.SpawnedBullets.Count;
            for (int i = 0; i < count; i++)
            {
                skill = this.actorObj.handle.SkillControl.SpawnedBullets[i];
                if ((skill != null) && (skill.CurAction != null))
                {
                    skill.CurAction.refParams.SetRefParam("_TriggerBullet", true);
                    SkillUseContext refParamObject = skill.CurAction.refParams.GetRefParamObject<SkillUseContext>("SkillContext");
                    if (refParamObject != null)
                    {
                        refParamObject.GatherTime = (this.lastTime / 0x3e8) + 1;
                        skill.lifeTime = this.triggerTime * refParamObject.GatherTime;
                    }
                }
            }
        }
    }
}

