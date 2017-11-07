﻿using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using UnityEngine;

public class NewbieGuideClickDragonIcon : NewbieGuideBaseScript
{
    protected override void Initialize()
    {
        CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CBattleSystem.s_battleUIForm);
        GameObject gameObject = form.transform.Find("DragonInfo/Tuch").gameObject;
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

