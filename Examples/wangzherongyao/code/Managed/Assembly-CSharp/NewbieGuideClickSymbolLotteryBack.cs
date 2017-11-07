using Assets.Scripts.UI;
using System;
using UnityEngine;

internal class NewbieGuideClickSymbolLotteryBack : NewbieGuideBaseScript
{
    private const float Delay = 8f;
    private float DelayTimer;

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
            this.DelayTimer += Time.deltaTime;
            if (this.DelayTimer >= 8f)
            {
                CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(Singleton<CMallSystem>.instance.sMallFormPath);
                if (form != null)
                {
                    Transform transform = form.transform.FindChild("TopCommon/Button_Close");
                    if (transform != null)
                    {
                        GameObject gameObject = transform.gameObject;
                        if (gameObject.activeInHierarchy)
                        {
                            base.AddHighLightGameObject(gameObject, true, form, true, new GameObject[0]);
                            base.Initialize();
                        }
                    }
                }
            }
        }
    }
}

