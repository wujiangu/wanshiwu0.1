using Assets.Scripts.UI;
using System;
using UnityEngine;

public class NewbieGuideClickConfirmReward : NewbieGuideBaseScript
{
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
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm("UGUI/Form/Common/Form_Award");
            if (form != null)
            {
                GameObject gameObject = form.transform.FindChild("btnGroup/Button_Back").gameObject;
                base.AddHighLightGameObject(gameObject, true, form, true, new GameObject[0]);
                base.Initialize();
            }
        }
    }
}

