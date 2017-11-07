namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.GameLogic.GameKernal;
    using System;

    [EventCategory("MMGame/Skill")]
    public class FilterTargetType : TickCondition
    {
        private bool bCheckFilter = true;
        public bool bFilterBoss;
        public bool bFilterDiffCamp;
        public bool bFilterHero;
        public bool bFilterMonter;
        public bool bFilterOrgan;
        public bool bFilterSameCamp;
        [ObjectTemplate(new System.Type[] {  })]
        public int targetId = -1;

        public override bool Check(Action _action, Track _track)
        {
            return this.bCheckFilter;
        }

        public override BaseEvent Clone()
        {
            FilterTargetType type = ClassObjPool<FilterTargetType>.Get();
            type.CopyData(this);
            return type;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            FilterTargetType type = src as FilterTargetType;
            this.targetId = type.targetId;
            this.bFilterHero = type.bFilterHero;
            this.bFilterMonter = type.bFilterMonter;
            this.bFilterBoss = type.bFilterBoss;
            this.bFilterOrgan = type.bFilterOrgan;
            this.bFilterSameCamp = type.bFilterSameCamp;
            this.bFilterDiffCamp = type.bFilterDiffCamp;
            this.bCheckFilter = type.bCheckFilter;
        }

        private void FilterActorCamp(PoolObjHandle<ActorRoot> actorObj)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            if ((hostPlayer != null) && (hostPlayer.Captain != 0))
            {
                if (this.bFilterSameCamp && !hostPlayer.Captain.handle.IsEnemyCamp(actorObj.handle))
                {
                    this.bCheckFilter = false;
                }
                else if (this.bFilterDiffCamp && hostPlayer.Captain.handle.IsEnemyCamp(actorObj.handle))
                {
                    this.bCheckFilter = false;
                }
            }
        }

        private void FilterActorType(PoolObjHandle<ActorRoot> actorObj)
        {
            if (this.bFilterHero && (actorObj.handle.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero))
            {
                this.bCheckFilter = false;
            }
            else
            {
                if (this.bFilterMonter)
                {
                    MonsterWrapper wrapper = actorObj.handle.AsMonster();
                    if (((wrapper != null) && (wrapper.cfgInfo != null)) && (wrapper.cfgInfo.bMonsterGrade != 3))
                    {
                        this.bCheckFilter = false;
                        return;
                    }
                }
                if (this.bFilterBoss)
                {
                    MonsterWrapper wrapper2 = actorObj.handle.AsMonster();
                    if (((wrapper2 != null) && (wrapper2.cfgInfo != null)) && (wrapper2.cfgInfo.bMonsterGrade == 3))
                    {
                        this.bCheckFilter = false;
                        return;
                    }
                }
                if (this.bFilterOrgan && (actorObj.handle.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Organ))
                {
                    this.bCheckFilter = false;
                }
            }
        }

        public override void OnUse()
        {
            base.OnUse();
            this.targetId = -1;
            this.bFilterHero = false;
            this.bFilterMonter = false;
            this.bFilterBoss = false;
            this.bFilterOrgan = false;
            this.bFilterSameCamp = false;
            this.bFilterDiffCamp = false;
            this.bCheckFilter = true;
        }

        public override void Process(Action _action, Track _track)
        {
            PoolObjHandle<ActorRoot> actorHandle = _action.GetActorHandle(this.targetId);
            if (actorHandle == 0)
            {
                if (ActionManager.Instance.isPrintLog)
                {
                }
            }
            else
            {
                this.FilterActorType(actorHandle);
                this.FilterActorCamp(actorHandle);
                base.Process(_action, _track);
            }
        }
    }
}

