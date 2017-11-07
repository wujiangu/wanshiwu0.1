using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using ResData;
using System;
using UnityEngine;

public class NewbieGuideClickTaskReward : NewbieGuideBaseScript
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
            ListView<CTask> tasks = Singleton<CTaskSys>.instance.model.GetTasks(RES_TASK_TYPE.RES_TASKTYPE_MAIN);
            uint num = base.currentConf.Param[0];
            if ((form != null) && (tasks != null))
            {
                CTaskView taskView = Singleton<CTaskSys>.GetInstance().GetTaskView();
                if (taskView != null)
                {
                    taskView.On_Tab_Change(0);
                    int index = -1;
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        if ((tasks[i].m_baseId == num) && (tasks[i].m_taskState == 1))
                        {
                            index = i;
                            break;
                        }
                    }
                    if ((index >= 0) && (index < 3))
                    {
                        Transform transform = form.transform.FindChild("node/list_node_main/tasklist");
                        if (transform != null)
                        {
                            CUIListScript component = transform.GetComponent<CUIListScript>();
                            if (component != null)
                            {
                                component.MoveElementInScrollArea(index, true);
                                Transform transform2 = component.GetElemenet(index).transform.FindChild("getReward_btn");
                                if (transform2 != null)
                                {
                                    base.AddHighLightGameObject(transform2.gameObject, true, form, true, new GameObject[0]);
                                    base.Initialize();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

