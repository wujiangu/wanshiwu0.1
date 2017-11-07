using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using System;
using UnityEngine;

public class NewbieGuideClickMistreatCom : NewbieGuideBaseScript
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
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CTaskSys.TASK_FORM_PATH);
            if (form != null)
            {
                int num = base.currentConf.Param[1];
                if (base.currentConf.Param[0] > 0)
                {
                }
                string name = string.Format("ListElement{0}/LinkBtn", num);
                Transform transform = form.transform.FindChild(name);
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

