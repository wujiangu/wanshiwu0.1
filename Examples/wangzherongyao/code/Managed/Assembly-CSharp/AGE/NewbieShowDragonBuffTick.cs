namespace AGE
{
    using Assets.Scripts.Common;
    using CSProtocol;
    using System;

    [EventCategory("MMGame/Newbie")]
    internal class NewbieShowDragonBuffTick : TickEvent
    {
        public override BaseEvent Clone()
        {
            NewbieShowDragonBuffTick tick = ClassObjPool<NewbieShowDragonBuffTick>.Get();
            tick.CopyData(this);
            return tick;
        }

        public override void Process(Action _action, Track _track)
        {
            Singleton<CBattleSystem>.GetInstance().SetDragonUINum(COM_PLAYERCAMP.COM_PLAYERCAMP_1, 3);
        }
    }
}

