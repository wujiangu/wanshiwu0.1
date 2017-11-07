namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic.GameKernal;
    using System;

    [EventCategory("MMGame/Newbie")]
    internal class GuideToggleHeroShowTick : TickEvent
    {
        public bool bShow;

        public override BaseEvent Clone()
        {
            GuideToggleHeroShowTick tick = ClassObjPool<GuideToggleHeroShowTick>.Get();
            tick.CopyData(this);
            return tick;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            GuideToggleHeroShowTick tick = src as GuideToggleHeroShowTick;
            this.bShow = tick.bShow;
        }

        public override void Process(Action _action, Track _track)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if ((hostPlayer != null) && (hostPlayer.Captain != 0))
            {
                DebugHelper.Assert((bool) hostPlayer.Captain, "Captain Hero is NULL!!");
                hostPlayer.Captain.handle.Visible = this.bShow;
            }
        }
    }
}

