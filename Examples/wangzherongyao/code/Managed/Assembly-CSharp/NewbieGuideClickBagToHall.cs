using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using UnityEngine;

public class NewbieGuideClickBagToHall : NewbieGuideBaseScript
{
    protected override void CompleteHandler()
    {
        base.CompleteHandler();
    }

    protected override void Initialize()
    {
        CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CBagSystem.s_bagFormPath);
        GameObject gameObject = form.transform.FindChild("TopCommon/Button_Close").gameObject;
        base.AddHighLightGameObject(gameObject, true, form, true, new GameObject[0]);
        base.Initialize();
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

