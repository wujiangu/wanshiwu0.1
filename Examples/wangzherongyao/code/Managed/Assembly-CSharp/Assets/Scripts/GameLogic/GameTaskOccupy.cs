namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Framework;
    using ResData;
    using System;

    public class GameTaskOccupy : GameTask
    {
        private void onActorInside(AreaEventTrigger sourceTrigger, object param)
        {
            if (sourceTrigger.Mark == this.TargetArea)
            {
                if ((this.SubjectType == RES_BATTLE_TASK_SUBJECT.CAMP) && sourceTrigger.HasActorInside(new Func<PoolObjHandle<ActorRoot>, bool>(this, (IntPtr) this.<onActorInside>m__B)))
                {
                    base.Current += (int) param;
                }
                else if ((this.SubjectType == RES_BATTLE_TASK_SUBJECT.ORGAN) && sourceTrigger.HasActorInside(new Func<PoolObjHandle<ActorRoot>, bool>(this, (IntPtr) this.<onActorInside>m__C)))
                {
                    base.Current += (int) param;
                }
            }
        }

        protected override void OnClose()
        {
            Singleton<TriggerEventSys>.instance.OnActorInside -= new TriggerEventDelegate(this.onActorInside);
        }

        protected override void OnDestroy()
        {
            Singleton<TriggerEventSys>.instance.OnActorInside -= new TriggerEventDelegate(this.onActorInside);
        }

        protected override void OnInitial()
        {
        }

        protected override void OnStart()
        {
            Singleton<TriggerEventSys>.instance.OnActorInside += new TriggerEventDelegate(this.onActorInside);
        }

        protected int SourceSubj
        {
            get
            {
                return base.Config.iParam2;
            }
        }

        protected RES_BATTLE_TASK_SUBJECT SubjectType
        {
            get
            {
                return (RES_BATTLE_TASK_SUBJECT) base.Config.iParam1;
            }
        }

        protected int TargetArea
        {
            get
            {
                return base.Config.iParam3;
            }
        }
    }
}

