using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using UnityEngine;

public class NewbieGuideClickAttack : NewbieGuideBaseScript
{
    private CUIFormScript battleForm;
    private GameObject buttonObj;
    private CSignalButton signalButton;

    protected override void Initialize()
    {
        this.battleForm = Singleton<CUIManager>.GetInstance().GetForm(CBattleSystem.s_battleUIForm);
        if (this.battleForm != null)
        {
            this.buttonObj = this.battleForm.GetWidget(13);
            if (this.buttonObj != null)
            {
                CUIEventScript component = this.buttonObj.GetComponent<CUIEventScript>();
                this.signalButton = Singleton<CBattleSystem>.GetInstance().GetSignalPanel().GetSingleButton(component.m_onClickEventParams.tag);
            }
        }
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
        else if (((this.signalButton != null) && !this.signalButton.IsInCooldown()) && (this.buttonObj != null))
        {
            base.AddHighLightGameObject(this.buttonObj, true, this.battleForm, true, new GameObject[0]);
            base.Initialize();
        }
    }
}

