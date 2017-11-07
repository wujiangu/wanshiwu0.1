namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using System;

    public class EyeWrapper : ObjWrapper
    {
        public override void ActorStateLog(SLogObj _logObj)
        {
            _logObj.Log(string.Format("{0} pos={1}", base.actor.name, base.actor.location));
        }

        public override void Born(ActorRoot owner)
        {
            base.actor = owner;
            base.actorPtr = new PoolObjHandle<ActorRoot>(base.actor);
            base.actor.HorizonMarker = base.actor.CreateLogicComponent<HorizonMarker>(base.actor);
        }

        public override void Deactive()
        {
        }

        public override void Fight()
        {
        }

        public override void FightOver()
        {
        }

        public override string GetTypeName()
        {
            return "EyeWrapper";
        }

        public override void Init()
        {
        }

        public override void OnUse()
        {
            base.OnUse();
        }

        public override void Prepare()
        {
        }

        public override void Reactive()
        {
            base.Reactive();
        }

        public override void Uninit()
        {
        }

        public override void UpdateLogic(int delta)
        {
        }
    }
}

