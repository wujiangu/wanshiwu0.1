using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using UnityEngine;

public class NewbieGuideClickPveOrPvpEntry : NewbieGuideBaseScript
{
    protected override void Initialize()
    {
        CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(LobbyForm.FORM_PATH);
        GameObject gameObject = form.transform.FindChild("BtnCon/PveBtn").gameObject;
        GameObject obj3 = form.transform.FindChild("BtnCon/PvpBtn").gameObject;
        GameObject[] addGo = new GameObject[] { gameObject, obj3 };
        base.AddHighLightGameObject(null, true, form, true, addGo);
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

