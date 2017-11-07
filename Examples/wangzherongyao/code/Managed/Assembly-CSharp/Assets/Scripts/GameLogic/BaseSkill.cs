namespace Assets.Scripts.GameLogic
{
    using AGE;
    using Assets.Scripts.Common;
    using ResData;
    using System;
    using UnityEngine;

    public abstract class BaseSkill : PooledClassObject
    {
        public string ActionName = string.Empty;
        protected Action curAction;
        public int SkillID;

        protected BaseSkill()
        {
        }

        public SkillUseContext GetSkillUseContext()
        {
            if (this.curAction == null)
            {
                return null;
            }
            return this.curAction.refParams.GetRefParamObject<SkillUseContext>("SkillContext");
        }

        public PoolObjHandle<ActorRoot> GetTargetActor()
        {
            SkillUseContext skillUseContext = this.GetSkillUseContext();
            if (skillUseContext == null)
            {
                return new PoolObjHandle<ActorRoot>(null);
            }
            return skillUseContext.TargetActor;
        }

        public virtual void OnActionStoped(Action action)
        {
            if (this.curAction != null)
            {
                this.curAction.onActionStop -= new ActionStopDelegate(this.OnActionStoped);
                if (action == this.curAction)
                {
                    this.curAction = null;
                }
            }
        }

        public override void OnRelease()
        {
            this.SkillID = 0;
            this.ActionName = string.Empty;
            this.curAction = null;
            base.OnRelease();
        }

        public override void OnUse()
        {
            base.OnUse();
            this.SkillID = 0;
            this.ActionName = string.Empty;
            this.curAction = null;
        }

        public virtual void Stop()
        {
            if (this.curAction != null)
            {
                this.curAction.Stop(false);
                this.curAction = null;
            }
        }

        public virtual bool Use(PoolObjHandle<ActorRoot> user, SkillUseContext context)
        {
            if (((user == 0) || (context == null)) || string.IsNullOrEmpty(this.ActionName))
            {
                return false;
            }
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            VInt3 forward = VInt3.forward;
            switch (context.AppointType)
            {
                case SkillRangeAppointType.Auto:
                case SkillRangeAppointType.Target:
                    flag = true;
                    break;

                case SkillRangeAppointType.Pos:
                    flag2 = true;
                    break;

                case SkillRangeAppointType.Directional:
                    flag3 = true;
                    forward = context.UseVector;
                    break;

                case SkillRangeAppointType.Track:
                    flag2 = true;
                    flag3 = true;
                    forward = context.EndVector - context.UseVector;
                    if (forward.sqrMagnitudeLong < 1L)
                    {
                        forward = VInt3.forward;
                    }
                    break;
            }
            if (flag && (context.TargetActor == 0))
            {
                return false;
            }
            if (flag)
            {
                GameObject[] objArray1 = new GameObject[] { user.handle.gameObject, context.TargetActor.handle.gameObject };
                this.curAction = ActionManager.Instance.PlayAction(this.ActionName, true, false, objArray1);
            }
            else
            {
                GameObject[] objArray2 = new GameObject[] { user.handle.gameObject };
                this.curAction = ActionManager.Instance.PlayAction(this.ActionName, true, false, objArray2);
            }
            if (this.curAction == null)
            {
                return false;
            }
            this.curAction.onActionStop += new ActionStopDelegate(this.OnActionStoped);
            this.curAction.refParams.AddRefParam("SkillObj", this);
            this.curAction.refParams.AddRefParam("SkillContext", context);
            if (flag)
            {
                this.curAction.refParams.AddRefParam("TargetActor", context.TargetActor);
            }
            if (flag2)
            {
                this.curAction.refParams.SetRefParam("_TargetPos", context.UseVector);
            }
            if (flag3)
            {
                this.curAction.refParams.SetRefParam("_TargetDir", forward);
            }
            return true;
        }

        public Action CurAction
        {
            get
            {
                return this.curAction;
            }
        }

        public virtual bool isBuff
        {
            get
            {
                return false;
            }
        }

        public virtual bool isBullet
        {
            get
            {
                return false;
            }
        }

        public bool isFinish
        {
            get
            {
                return (this.curAction == null);
            }
        }
    }
}

