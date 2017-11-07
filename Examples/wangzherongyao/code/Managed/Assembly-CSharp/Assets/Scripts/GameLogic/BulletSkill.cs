namespace Assets.Scripts.GameLogic
{
    using AGE;
    using Assets.Scripts.Common;
    using ResData;
    using System;

    public class BulletSkill : BaseSkill
    {
        private bool bDeadRemove;
        public bool bManaged = true;
        public ResBulletCfgInfo cfgData;
        public int lifeTime;

        public void Init(string _actionName, bool _bDeadRemove)
        {
            this.bDeadRemove = _bDeadRemove;
            base.ActionName = _actionName;
        }

        public override void OnActionStoped(Action action)
        {
            base.OnActionStoped(action);
            if (!this.bManaged)
            {
                base.Release();
            }
        }

        public override void OnRelease()
        {
            this.bManaged = true;
            this.lifeTime = 0;
            this.bDeadRemove = false;
            this.cfgData = null;
            base.OnRelease();
        }

        public override void OnUse()
        {
            base.OnUse();
            this.bManaged = true;
            this.lifeTime = 0;
            this.bDeadRemove = false;
            this.cfgData = null;
        }

        public void UpdateLogic(int nDelta)
        {
            if (this.lifeTime > 0)
            {
                this.lifeTime -= nDelta;
                if (this.lifeTime < 0)
                {
                    this.lifeTime = 0;
                    this.Stop();
                }
            }
        }

        public override bool Use(PoolObjHandle<ActorRoot> user, SkillUseContext context)
        {
            if (context != null)
            {
                context.Instigator = this;
                DebugHelper.Assert((bool) context.Originator);
            }
            if (!base.Use(user, context))
            {
                return false;
            }
            return true;
        }

        public override bool isBullet
        {
            get
            {
                return true;
            }
        }

        public bool IsDeadRemove
        {
            get
            {
                return this.bDeadRemove;
            }
        }
    }
}

