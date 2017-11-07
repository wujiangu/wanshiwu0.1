using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using UnityEngine;

internal class NewbieGuideClickOneItem : NewbieGuideBaseScript
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
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CBagSystem.s_bagFormPath);
            CUIListScript component = form.transform.FindChild("Panel_Right/List").gameObject.GetComponent<CUIListScript>();
            if (component != null)
            {
                int index = 0;
                bool flag = false;
                if (base.currentConf.Param[0] > 0)
                {
                    uint availableItemId = NewbieGuideCheckTriggerConditionUtil.AvailableItemId;
                    NewbieGuideCheckTriggerConditionUtil.AvailableItemId = 0;
                    if (base.currentConf.Param[1] > 0)
                    {
                        availableItemId = base.currentConf.Param[1];
                    }
                    int num3 = Singleton<CBagSystem>.GetInstance().FindItemIndex(availableItemId);
                    if (num3 != -1)
                    {
                        index = num3;
                        flag = true;
                    }
                }
                else
                {
                    index = base.currentConf.Param[1];
                    flag = true;
                }
                if (flag)
                {
                    CUIListElementScript elemenet = component.GetElemenet(index);
                    if (elemenet != null)
                    {
                        GameObject gameObject = elemenet.transform.Find("itemCell").gameObject;
                        base.AddHighLightGameObject(gameObject, true, form, true, new GameObject[0]);
                        base.Initialize();
                    }
                }
            }
        }
    }
}

