﻿using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using UnityEngine;

internal class NewbieGuideClickHeroStepUpActual : NewbieGuideBaseScript
{
    protected override void Initialize()
    {
        CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CHeroInfoSystem2.s_heroInfoFormPath);
        GameObject gameObject = form.transform.FindChild("Panel_Right/Panel_HeroInfo/Panel_QualityUp/Panel_Property/Btn_QualityUp").gameObject;
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

