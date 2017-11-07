using Assets.Scripts.UI;
using System;
using UnityEngine;

internal class NewbieGuideClickMysteryShop : NewbieGuideBaseScript
{
    protected override void Initialize()
    {
        CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(Singleton<CShopSystem>.GetInstance().sShopFormPath);
        GameObject gameObject = form.transform.FindChild("pnlShop/Tab/ScrollRect/Content/ListElement_1").gameObject;
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

