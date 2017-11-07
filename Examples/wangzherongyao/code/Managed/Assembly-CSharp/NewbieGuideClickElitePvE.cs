using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using UnityEngine;

public class NewbieGuideClickElitePvE : NewbieGuideBaseScript
{
    protected override void Initialize()
    {
        CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CAdventureSys.ADVENTURE_SELECT_FORM);
        form.gameObject.transform.Find("ChapterList").gameObject.GetComponent<CUIStepListScript>().SelectElementImmediately(0);
        GameObject gameObject = form.transform.Find("TopCommon/Panel_Menu/ListMenu/ScrollRect/Content/ListElement_1").gameObject;
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

