using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using UnityEngine;

public class NewbieGuideClickUsingItem : NewbieGuideBaseScript
{
    protected override void CompleteHandler()
    {
        base.CompleteHandler();
    }

    protected override void Initialize()
    {
    }

    protected override bool IsDelegateClickEvent()
    {
        return true;
    }

    protected override bool IsDelegateModalControl()
    {
        return true;
    }

    protected override void Update()
    {
        if (base.isInitialize)
        {
            base.Update();
        }
        else
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CBagSystem.s_bagFormPath);
            GameObject gameObject = form.transform.FindChild("Panel_Left/BtnGroup/Button_Use").gameObject;
            base.AddHighLightGameObject(gameObject, true, form, true, new GameObject[0]);
            base.Initialize();
        }
    }
}

