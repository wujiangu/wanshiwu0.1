namespace AGE
{
    using Assets.Scripts.Common;
    using System;
    using UnityEngine;

    [EventCategory("MMGame/Drama")]
    public class AddGoldInBattle : TickEvent
    {
        public int iGoldToAdd;

        public override BaseEvent Clone()
        {
            AddGoldInBattle battle = ClassObjPool<AddGoldInBattle>.Get();
            battle.CopyData(this);
            return battle;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            AddGoldInBattle battle = src as AddGoldInBattle;
            this.iGoldToAdd = battle.iGoldToAdd;
        }

        public override void Process(Action _action, Track _track)
        {
            base.Process(_action, _track);
            Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer().Captain.handle.ValueComponent.ChangeGoldCoinInBattle(this.iGoldToAdd, true, false, new Vector3());
        }

        public override bool SupportEditMode()
        {
            return true;
        }
    }
}

