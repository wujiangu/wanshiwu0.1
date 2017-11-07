namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using System;
    using System.Collections.Generic;

    [EventCategory("MMGame/Skill")]
    public class HitTriggerTick : TickEvent
    {
        public bool bulletHit;
        public bool lastHit;
        [AssetReference(AssetRefType.SkillCombine)]
        public int SelfSkillCombineID_1;
        [AssetReference(AssetRefType.SkillCombine)]
        public int SelfSkillCombineID_2;
        [AssetReference(AssetRefType.SkillCombine)]
        public int SelfSkillCombineID_3;
        private VCollisionShape shape;
        [ObjectTemplate(new System.Type[] {  })]
        public int targetId = -1;
        [AssetReference(AssetRefType.SkillCombine)]
        public int TargetSkillCombine_1;
        [AssetReference(AssetRefType.SkillCombine)]
        public int TargetSkillCombine_2;
        [AssetReference(AssetRefType.SkillCombine)]
        public int TargetSkillCombine_3;
        [ObjectTemplate(new System.Type[] {  })]
        public int triggerId;
        public int victimId = -1;

        public override BaseEvent Clone()
        {
            HitTriggerTick tick = ClassObjPool<HitTriggerTick>.Get();
            tick.CopyData(this);
            return tick;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            HitTriggerTick tick = src as HitTriggerTick;
            this.targetId = tick.targetId;
            this.triggerId = tick.triggerId;
            this.victimId = tick.victimId;
            this.lastHit = tick.lastHit;
            this.SelfSkillCombineID_1 = tick.SelfSkillCombineID_1;
            this.SelfSkillCombineID_2 = tick.SelfSkillCombineID_2;
            this.SelfSkillCombineID_3 = tick.SelfSkillCombineID_3;
            this.TargetSkillCombine_1 = tick.TargetSkillCombine_1;
            this.TargetSkillCombine_2 = tick.TargetSkillCombine_2;
            this.TargetSkillCombine_3 = tick.TargetSkillCombine_3;
            this.shape = tick.shape;
        }

        private List<PoolObjHandle<ActorRoot>> FilterTargetByTriggerRegion(Action _action, PoolObjHandle<ActorRoot> _attackActor, BaseSkill _skill)
        {
            if ((_attackActor == 0) || (_skill == null))
            {
                return null;
            }
            List<PoolObjHandle<ActorRoot>> list = null;
            PoolObjHandle<ActorRoot> targetActor = _skill.GetTargetActor();
            if ((targetActor != 0) && !targetActor.handle.ActorControl.IsDeadState)
            {
                if (list == null)
                {
                    list = new List<PoolObjHandle<ActorRoot>>();
                }
                list.Add(targetActor);
            }
            PoolObjHandle<ActorRoot> actorHandle = _action.GetActorHandle(this.triggerId);
            if (actorHandle != 0)
            {
                this.shape = AGE_Helper.GetCollisionShape((ActorRoot) actorHandle);
                if (this.shape == null)
                {
                    return list;
                }
                Singleton<TargetSearcher>.instance.BeginCollidedActorList(_attackActor, this.shape, false, true, null);
                List<PoolObjHandle<ActorRoot>> collidedActors = Singleton<TargetSearcher>.instance.GetCollidedActors();
                if ((collidedActors != null) && (collidedActors.Count > 0))
                {
                    if (list == null)
                    {
                        list = new List<PoolObjHandle<ActorRoot>>();
                    }
                    List<PoolObjHandle<ActorRoot>>.Enumerator enumerator = collidedActors.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (list.IndexOf(enumerator.Current) == -1)
                        {
                            list.Add(enumerator.Current);
                        }
                    }
                }
                Singleton<TargetSearcher>.instance.EndCollidedActorList();
            }
            return list;
        }

        public override void OnUse()
        {
            base.OnUse();
            this.shape = null;
        }

        public override void Process(Action _action, Track _track)
        {
            PoolObjHandle<ActorRoot> actorHandle = _action.GetActorHandle(this.targetId);
            if (actorHandle != 0)
            {
                SkillComponent skillControl = actorHandle.handle.SkillControl;
                if (skillControl != null)
                {
                    BaseSkill refParamObject = _action.refParams.GetRefParamObject<BaseSkill>("SkillObj");
                    if (refParamObject != null)
                    {
                        List<PoolObjHandle<ActorRoot>> list = this.FilterTargetByTriggerRegion(_action, actorHandle, refParamObject);
                        if (list != null)
                        {
                            SkillChooseTargetEventParam prm = new SkillChooseTargetEventParam(actorHandle, actorHandle, list.Count);
                            Singleton<GameEventSys>.instance.SendEvent<SkillChooseTargetEventParam>(GameEventDef.Event_HitTrigger, ref prm);
                        }
                        SkillUseContext inContext = _action.refParams.GetRefParamObject<SkillUseContext>("SkillContext");
                        if (inContext != null)
                        {
                            skillControl.SpawnBuff(actorHandle, inContext, this.SelfSkillCombineID_1, false);
                            skillControl.SpawnBuff(actorHandle, inContext, this.SelfSkillCombineID_2, false);
                            skillControl.SpawnBuff(actorHandle, inContext, this.SelfSkillCombineID_3, false);
                            if (list != null)
                            {
                                for (int i = 0; i < list.Count; i++)
                                {
                                    inContext.EffectDir = actorHandle.handle.forward;
                                    bool flag = false;
                                    flag = skillControl.SpawnBuff(list[i], inContext, this.TargetSkillCombine_1, false) | skillControl.SpawnBuff(list[i], inContext, this.TargetSkillCombine_2, false);
                                    if (flag | skillControl.SpawnBuff(list[i], inContext, this.TargetSkillCombine_3, false))
                                    {
                                        PoolObjHandle<ActorRoot> handle2 = list[i];
                                        handle2.handle.ActorControl.BeAttackHit(actorHandle);
                                    }
                                }
                            }
                        }
                    }
                    this.shape = null;
                }
            }
        }
    }
}

