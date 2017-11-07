using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using UnityEngine;

public class NewbieGuideClickHallMall : NewbieGuideBaseScript
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
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(LobbyForm.FORM_PATH);
            GameObject gameObject = form.transform.FindChild("Popup/BoardBtn").gameObject;
            base.AddHighLightGameObject(gameObject, true, form, true, new GameObject[0]);
            base.Initialize();
        }
    }
}

