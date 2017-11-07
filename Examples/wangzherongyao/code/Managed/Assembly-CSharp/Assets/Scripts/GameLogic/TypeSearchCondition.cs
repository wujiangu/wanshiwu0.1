namespace Assets.Scripts.GameLogic
{
    using System;

    public class TypeSearchCondition
    {
        public static bool Fit(ActorRoot _actor, ActorTypeDef _actorType)
        {
            return (_actor.TheActorMeta.ActorType == _actorType);
        }

        public static bool Fit(ActorRoot _actor, ActorTypeDef _actorType, bool isBoss)
        {
            return ((_actor.TheActorMeta.ActorType == _actorType) && (_actor.ActorControl.IsBossOrHeroAutoAI() == isBoss));
        }
    }
}

