﻿namespace behaviac
{
    using Assets.Scripts.GameLogic;

    internal class Action_bt_WrapperAI_Monster_BTMonsterPassive_zhouwang_node15 : Action
    {
        protected override EBTStatus update_impl(Agent pAgent, EBTStatus childStatus)
        {
            ((ObjAgent) pAgent).ShowHeadExclamationMark();
            return EBTStatus.BT_SUCCESS;
        }
    }
}

