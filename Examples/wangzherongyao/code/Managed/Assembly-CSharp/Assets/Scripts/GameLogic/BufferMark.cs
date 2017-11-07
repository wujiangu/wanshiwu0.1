namespace Assets.Scripts.GameLogic
{
    using AGE;
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using ResData;
    using System;

    public class BufferMark : BaseSkill
    {
        public ResSkillMarkCfgInfo cfgData;
        private int curLayer;
        private int curTime;
        private int immuneTime;
        private int lastMaxTime;
        private PoolObjHandle<ActorRoot> sourceActor;

        public BufferMark(int _markID)
        {
            base.SkillID = _markID;
            this.cfgData = GameDataMgr.skillMarkDatabin.GetDataByKey(_markID);
        }

        public void AddLayer(int _addLayer)
        {
            this.curLayer += _addLayer;
            this.curLayer = (this.curLayer <= this.cfgData.iMaxLayer) ? this.curLayer : this.cfgData.iMaxLayer;
            this.curTime = 0;
        }

        public void AddTrigger(PoolObjHandle<ActorRoot> _originator)
        {
            if ((this.curLayer >= this.cfgData.iMaxLayer) && (this.cfgData.bAutoTrigger == 1))
            {
                this.Trigger(_originator);
            }
        }

        public void DecLayer(int _decLayer)
        {
            this.curLayer -= _decLayer;
            this.curLayer = (this.curLayer <= 0) ? 0 : this.curLayer;
        }

        public void Init(BuffHolderComponent _buffHolder)
        {
            this.sourceActor = _buffHolder.actorPtr;
            base.ActionName = StringHelper.UTF8BytesToString(ref this.cfgData.szActionName);
            this.curLayer = 1;
            this.immuneTime = 0;
            this.curTime = 0;
            this.lastMaxTime = (this.cfgData.iLastMaxTime != 0) ? this.cfgData.iLastMaxTime : 0x7fffffff;
        }

        public override void OnActionStoped(Action action)
        {
            if (base.curAction != null)
            {
                base.curAction.onActionStop -= new ActionStopDelegate(this.OnActionStoped);
                if (action == base.curAction)
                {
                    base.curAction = null;
                }
            }
        }

        public void Trigger(PoolObjHandle<ActorRoot> _originator)
        {
            if (this.immuneTime == 0)
            {
                this.TriggerAction(_originator);
                this.DecLayer(this.cfgData.iCostLayer);
                this.immuneTime = this.cfgData.iImmuneTime;
            }
        }

        private void TriggerAction(PoolObjHandle<ActorRoot> _originator)
        {
            SkillUseContext context = new SkillUseContext();
            context.SetOriginator(_originator);
            context.TargetActor = this.sourceActor;
            this.Use(_originator, context);
        }

        public void UpdateLogic(int nDelta)
        {
            if (this.immuneTime > 0)
            {
                this.immuneTime -= nDelta;
                this.immuneTime = (this.immuneTime <= 0) ? 0 : this.immuneTime;
            }
            this.curTime += nDelta;
            if (this.curTime >= this.lastMaxTime)
            {
                this.curLayer = 0;
            }
        }

        public void UpperTrigger(PoolObjHandle<ActorRoot> _originator)
        {
            if ((this.curLayer >= this.cfgData.iTriggerLayer) && (this.cfgData.iTriggerLayer > 0))
            {
                this.Trigger(_originator);
            }
        }
    }
}

