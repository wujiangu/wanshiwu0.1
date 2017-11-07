using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using UnityEngine;

public class NewbieGuideClickEquipPanel : NewbieGuideBaseScript
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
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CBattleSystem.s_battleUIForm);
            if (form != null)
            {
                GameObject gameObject = form.GetWidget(0x2d).transform.FindChild("EquipBtn").gameObject;
                DebugHelper.Assert(gameObject != null, "Can't find EquipBtn~!!");
                base.AddHighLightGameObject(gameObject, true, form, true, new GameObject[0]);
                base.Initialize();
            }
        }
    }
}

