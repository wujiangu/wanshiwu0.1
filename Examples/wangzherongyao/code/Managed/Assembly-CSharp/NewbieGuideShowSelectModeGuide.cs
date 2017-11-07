using Assets.Scripts.GameLogic;
using System;

internal class NewbieGuideShowSelectModeGuide : NewbieGuideBaseScript
{
    protected override void Initialize()
    {
        Singleton<CBattleGuideManager>.GetInstance().OpenFormShared(CBattleGuideManager.EBattleGuideFormType.SelectModeGuide, 0x1388);
        this.CompleteHandler();
    }

    protected override bool IsDelegateClickEvent()
    {
        return true;
    }

    protected override bool IsDelegateModalControl()
    {
        return true;
    }
}

