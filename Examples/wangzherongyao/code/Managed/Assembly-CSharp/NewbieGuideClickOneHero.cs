using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using UnityEngine;

public class NewbieGuideClickOneHero : NewbieGuideBaseScript
{
    private bool m_bFirstUpdated;
    private int m_elemIndex;

    protected override void CompleteHandler()
    {
        base.CompleteHandler();
    }

    protected override void Initialize()
    {
        this.m_elemIndex = 0;
        if (base.currentConf.Param[0] == 0)
        {
            this.m_elemIndex = base.currentConf.Param[1];
        }
        else
        {
            uint inCfgId = base.currentConf.Param[1];
            if (NewbieGuideCheckTriggerConditionUtil.AvailableHeroId > 0)
            {
                inCfgId = NewbieGuideCheckTriggerConditionUtil.AvailableHeroId;
                NewbieGuideCheckTriggerConditionUtil.AvailableHeroId = 0;
            }
            this.m_elemIndex = Singleton<CHeroOverviewSystem>.GetInstance().GetHeroIndexByConfigId(inCfgId);
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
        else if (!this.m_bFirstUpdated)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CHeroOverviewSystem.s_heroViewFormPath);
            if (form != null)
            {
                Transform transform = form.transform.FindChild("Panel_Hero/List");
                if (transform != null)
                {
                    CUIListScript component = transform.gameObject.GetComponent<CUIListScript>();
                    if (component != null)
                    {
                        component.MoveElementInScrollArea(this.m_elemIndex, true);
                        this.m_bFirstUpdated = true;
                    }
                }
            }
        }
        else
        {
            CUIFormScript inOriginalForm = Singleton<CUIManager>.GetInstance().GetForm(CHeroOverviewSystem.s_heroViewFormPath);
            Transform transform2 = inOriginalForm.transform.FindChild("Panel_Hero/List");
            if (transform2 != null)
            {
                CUIListScript script4 = transform2.gameObject.GetComponent<CUIListScript>();
                if (script4 != null)
                {
                    CUIListElementScript elemenet = script4.GetElemenet(this.m_elemIndex);
                    if (elemenet != null)
                    {
                        Transform transform3 = elemenet.transform.Find("heroItem");
                        if (transform3 != null)
                        {
                            GameObject gameObject = transform3.gameObject;
                            if (gameObject.activeInHierarchy)
                            {
                                base.AddHighLightGameObject(gameObject, true, inOriginalForm, true, new GameObject[0]);
                                base.Initialize();
                            }
                        }
                    }
                }
            }
        }
    }
}

