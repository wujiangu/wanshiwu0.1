using Assets.Scripts.Framework;
using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using ResData;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CTaskView
{
    private Text day_huoyue_txt;
    private Image day_progress;
    private Image day_progress_bg;
    private CUIContainerScript m_container;
    private CUIFormScript m_CUIForm;
    private int m_tabIndex = -1;
    private CUIListScript tablistScript;
    private CUIListScript tasklistScript_main;
    private CUIListScript tasklistScript_mishu;
    private CUIListScript tasklistScript_usual;
    private Text week_huoyue_text;
    private GameObject week_node1;
    private GameObject week_node2;

    private void _calc_red_dot(RES_TASK_TYPE type)
    {
        int index = (type != RES_TASK_TYPE.RES_TASKTYPE_MAIN) ? 1 : 0;
        if (Singleton<CTaskSys>.instance.model.task_Data.GetTask_Count(type, CTask.State.Have_Done) > 0)
        {
            this.AddRedDot(index, enRedDotPos.enTopRight);
        }
        else
        {
            this.DelRedDot(index);
        }
    }

    private CTask _get_current_info(RES_TASK_TYPE taskType, int index)
    {
        ListView<CTask> listView = Singleton<CTaskSys>.instance.model.task_Data.GetListView(taskType);
        if (((listView != null) && (index >= 0)) && (index < listView.Count))
        {
            return listView[index];
        }
        return null;
    }

    private void _init_day_huoyue()
    {
        CTaskModel model = Singleton<CTaskSys>.instance.model;
        uint num = model.huoyue_data.max_dayhuoyue_num;
        float x = (this.day_progress_bg.transform as RectTransform).sizeDelta.x;
        GameObject obj2 = null;
        int count = model.huoyue_data.day_huoyue_list.Count;
        for (int i = 0; i < count; i++)
        {
            ushort key = model.huoyue_data.day_huoyue_list[i];
            ResHuoYueDuReward reward = null;
            GameDataMgr.huoyueduDict.TryGetValue(key, out reward);
            if (reward != null)
            {
                float num6 = (((float) reward.dwHuoYueDu) / ((float) num)) * x;
                if (num6 > x)
                {
                    num6 = x;
                }
                int element = this.m_container.GetElement();
                obj2 = this.m_container.GetElement(element);
                if (obj2 != null)
                {
                    (obj2.transform as RectTransform).anchoredPosition3D = new Vector3(num6, 0f, 0f);
                    obj2.gameObject.name = string.Format("box_{0}", reward.wID);
                    obj2.transform.Find("icon").GetComponent<CUIEventScript>().m_onDownEventParams.tagUInt = reward.wID;
                }
            }
        }
    }

    private void _refresh_list(CUIListScript listScript, ListView<CTask> data_list)
    {
        if (listScript != null)
        {
            int count = data_list.Count;
            listScript.SetElementAmount(count);
            for (int i = 0; i < count; i++)
            {
                CUIListElementScript elemenet = listScript.GetElemenet(i);
                if ((elemenet != null) && listScript.IsElementInScrollArea(i))
                {
                    CTaskShower component = elemenet.GetComponent<CTaskShower>();
                    CTask task = data_list[i];
                    if ((component != null) && (task != null))
                    {
                        component.ShowTask(task, this.m_CUIForm);
                    }
                }
            }
        }
    }

    private void _show_day_box(GameObject box, ushort id)
    {
        Text component = box.transform.FindChild("num").GetComponent<Text>();
        Image image = box.transform.FindChild("mark").GetComponent<Image>();
        Image image2 = box.transform.FindChild("icon").GetComponent<Image>();
        ResHuoYueDuReward reward = null;
        GameDataMgr.huoyueduDict.TryGetValue(id, out reward);
        if (reward != null)
        {
            bool bActive = Singleton<CTaskSys>.instance.model.huoyue_data.BAlready_Reward(RES_HUOYUEDU_TYPE.RES_HUOYUEDU_TYPE_DAY, id);
            component.text = reward.dwHuoYueDu.ToString();
            image.gameObject.CustomSetActive(bActive);
            if (image2 != null)
            {
                ResDT_HuoYueDuReward_PeriodInfo info = Singleton<CTaskSys>.instance.model.huoyue_data.IsInTime(reward);
                if (info != null)
                {
                    image2.SetSprite(CUIUtility.s_Sprite_Dynamic_Icon_Dir + info.szIcon, this.m_CUIForm, true, false, false);
                }
                else
                {
                    image2.SetSprite(CUIUtility.s_Sprite_Dynamic_Icon_Dir + reward.szIcon, this.m_CUIForm, true, false, false);
                }
            }
            bool flag2 = !bActive && (Singleton<CTaskSys>.instance.model.huoyue_data.day_curNum >= reward.dwHuoYueDu);
            box.transform.FindChild("effect").gameObject.CustomSetActive(flag2);
            box.GetComponent<Animation>().enabled = flag2;
        }
    }

    public void AddRedDot(int index, enRedDotPos redDotPos)
    {
        CUIListElementScript elemenet = this.tablistScript.GetElemenet(index);
        if (elemenet != null)
        {
            CUICommonSystem.AddRedDot(elemenet.gameObject, redDotPos, 0);
        }
    }

    public void Bind_Week_Node(GameObject node, ushort week_id)
    {
        CTaskModel model = Singleton<CTaskSys>.instance.model;
        HuoyueData data = model.huoyue_data;
        ResHuoYueDuReward rewardCfg = model.huoyue_data.GetRewardCfg(week_id);
        if (rewardCfg != null)
        {
            Transform transform = node.transform.Find("node/box/icon");
            transform.GetComponent<CUIEventScript>().m_onDownEventParams.tagUInt = week_id;
            node.GetComponent<Text>().text = rewardCfg.dwHuoYueDu.ToString();
            Image component = transform.GetComponent<Image>();
            ResDT_HuoYueDuReward_PeriodInfo info = Singleton<CTaskSys>.instance.model.huoyue_data.IsInTime(rewardCfg);
            if (info != null)
            {
                component.SetSprite(CUIUtility.s_Sprite_Dynamic_Icon_Dir + info.szIcon, this.m_CUIForm, true, false, false);
            }
            else
            {
                component.SetSprite(CUIUtility.s_Sprite_Dynamic_Icon_Dir + rewardCfg.szIcon, this.m_CUIForm, true, false, false);
            }
            bool bActive = data.BAlready_Reward(RES_HUOYUEDU_TYPE.RES_HUOYUEDU_TYPE_WEEK, week_id);
            node.transform.FindChild("node/box/mark").gameObject.CustomSetActive(bActive);
            bool flag2 = !bActive && (data.week_curNum >= rewardCfg.dwHuoYueDu);
            node.transform.FindChild("node/box/effect").gameObject.CustomSetActive(flag2);
            node.transform.FindChild("node/box").GetComponent<Animation>().enabled = flag2;
        }
    }

    public void Clear()
    {
        this.m_tabIndex = -1;
        if (this.m_container != null)
        {
            this.m_container.RecycleAllElement();
        }
        this.m_container = null;
        this.day_progress_bg = (Image) (this.day_progress = null);
        this.day_huoyue_txt = null;
        this.tasklistScript_main = null;
        this.tasklistScript_usual = null;
        this.week_huoyue_text = null;
        this.week_node1 = null;
        this.week_node2 = null;
        this.tablistScript = null;
        this.m_CUIForm = null;
        Singleton<CUIManager>.GetInstance().CloseForm(CTaskSys.TASK_FORM_PATH);
    }

    public void DelRedDot(int index)
    {
        CUIListElementScript elemenet = this.tablistScript.GetElemenet(index);
        if (elemenet != null)
        {
            CUICommonSystem.DelRedDot(elemenet.gameObject);
        }
    }

    private CUIListScript getListScript(int type)
    {
        if (type == 0)
        {
            return this.tasklistScript_main;
        }
        if (type == 1)
        {
            return this.tasklistScript_usual;
        }
        return null;
    }

    public void On_List_ElementEnable(CUIEvent uievent)
    {
        int srcWidgetIndexInBelongedList = uievent.m_srcWidgetIndexInBelongedList;
        CTask task = this._get_current_info((RES_TASK_TYPE) this.m_tabIndex, srcWidgetIndexInBelongedList);
        CTaskShower component = uievent.m_srcWidget.GetComponent<CTaskShower>();
        if ((component != null) && (task != null))
        {
            component.ShowTask(task, this.m_CUIForm);
        }
    }

    public void On_Tab_Change(int index)
    {
        this.TabIndex = index;
    }

    public void OpenForm(CUIEvent uiEvent)
    {
        if (this.m_CUIForm != null)
        {
            this.tablistScript.SelectElement(uiEvent.m_eventParams.tag, true);
        }
        else
        {
            this.m_CUIForm = Singleton<CUIManager>.GetInstance().OpenForm(CTaskSys.TASK_FORM_PATH, true, true);
            this.tablistScript = this.m_CUIForm.GetWidget(1).GetComponent<CUIListScript>();
            this.tasklistScript_main = this.m_CUIForm.GetWidget(2).GetComponent<CUIListScript>();
            this.tasklistScript_usual = this.m_CUIForm.GetWidget(3).GetComponent<CUIListScript>();
            this.tasklistScript_mishu = this.m_CUIForm.GetWidget(4).GetComponent<CUIListScript>();
            string[] strArray = null;
            CTaskModel model = Singleton<CTaskSys>.instance.model;
            strArray = new string[] { model.Daily_Quest_Career, model.Daily_Quest_NeedGrowing, model.Daily_Quest_NeedMoney, model.Daily_Quest_NeedSeal, model.Daily_Quest_NeedHero };
            this.tablistScript.SetElementAmount(strArray.Length);
            for (int i = 0; i < this.tablistScript.m_elementAmount; i++)
            {
                this.tablistScript.GetElemenet(i).gameObject.transform.FindChild("Text").GetComponent<Text>().text = strArray[i];
            }
            this.tablistScript.m_alwaysDispatchSelectedChangeEvent = true;
            this.tablistScript.SelectElement(uiEvent.m_eventParams.tag, true);
            this.tablistScript.m_alwaysDispatchSelectedChangeEvent = false;
            this.week_huoyue_text = this.m_CUIForm.GetWidget(5).GetComponent<Text>();
            this.week_node1 = this.m_CUIForm.GetWidget(6);
            this.week_node2 = this.m_CUIForm.GetWidget(7);
            this.m_container = this.m_CUIForm.GetWidget(8).GetComponent<CUIContainerScript>();
            this.day_progress_bg = this.m_CUIForm.GetWidget(9).GetComponent<Image>();
            this.day_progress = this.m_CUIForm.GetWidget(10).GetComponent<Image>();
            this.day_huoyue_txt = this.m_CUIForm.GetWidget(11).GetComponent<Text>();
            this.Refresh_Tab_RedPoint();
            this._init_day_huoyue();
            this.Refresh_Huoyue();
        }
    }

    public void Refresh()
    {
        if (this.m_CUIForm != null)
        {
            this.Refresh_Tab_RedPoint();
            this.Refresh_List(this.m_tabIndex);
        }
    }

    public void refresh_Day_HuoYue()
    {
        CTaskModel model = Singleton<CTaskSys>.instance.model;
        this.day_huoyue_txt.text = model.huoyue_data.day_curNum.ToString();
        float x = (this.day_progress_bg.transform as RectTransform).sizeDelta.x;
        float y = (this.day_progress.transform as RectTransform).sizeDelta.y;
        uint num3 = model.huoyue_data.max_dayhuoyue_num;
        float num4 = (((float) model.huoyue_data.day_curNum) / ((float) num3)) * x;
        if (num4 > x)
        {
            num4 = x;
        }
        (this.day_progress.transform as RectTransform).sizeDelta = new Vector2(num4, y);
        for (int i = 0; i < model.huoyue_data.day_huoyue_list.Count; i++)
        {
            ushort key = model.huoyue_data.day_huoyue_list[i];
            ResHuoYueDuReward reward = null;
            GameDataMgr.huoyueduDict.TryGetValue(key, out reward);
            if (reward != null)
            {
                GameObject gameObject = this.m_container.gameObject.transform.FindChild(string.Format("box_{0}", reward.wID)).gameObject;
                DebugHelper.Assert(gameObject != null);
                this._show_day_box(gameObject, reward.wID);
            }
        }
    }

    public void Refresh_Huoyue()
    {
        if (this.m_CUIForm != null)
        {
            this.Refresh_Week_Huoyue();
            this.refresh_Day_HuoYue();
        }
    }

    public void Refresh_List(int tIndex)
    {
        ListView<CTask> listView = Singleton<CTaskSys>.instance.model.task_Data.GetListView((RES_TASK_TYPE) tIndex);
        CUIListScript listScript = this.getListScript(tIndex);
        if ((listScript != null) && (listView != null))
        {
            listScript.transform.parent.gameObject.CustomSetActive(true);
            this._refresh_list(listScript, listView);
        }
    }

    public void Refresh_Tab_RedPoint()
    {
        this._calc_red_dot(RES_TASK_TYPE.RES_TASKTYPE_MAIN);
        this._calc_red_dot(RES_TASK_TYPE.RES_TASKTYPE_USUAL);
    }

    public void Refresh_Week_Huoyue()
    {
        CTaskModel model = Singleton<CTaskSys>.instance.model;
        this.Bind_Week_Node(this.week_node1, model.huoyue_data.week_reward1);
        this.Bind_Week_Node(this.week_node2, model.huoyue_data.week_reward2);
        this.week_huoyue_text.text = model.huoyue_data.week_curNum.ToString();
    }

    public int TabIndex
    {
        get
        {
            return this.m_tabIndex;
        }
        set
        {
            this.m_tabIndex = value;
            if (this.tasklistScript_main != null)
            {
                this.tasklistScript_main.transform.parent.gameObject.CustomSetActive(false);
            }
            if (this.tasklistScript_usual != null)
            {
                this.tasklistScript_usual.transform.parent.gameObject.CustomSetActive(false);
            }
            if (this.tasklistScript_mishu != null)
            {
                this.tasklistScript_mishu.transform.parent.gameObject.CustomSetActive(false);
            }
            if ((this.m_tabIndex == 0) || (this.m_tabIndex == 1))
            {
                this.Refresh_List(this.m_tabIndex);
            }
            else if (this.tasklistScript_mishu != null)
            {
                this.tasklistScript_mishu.transform.parent.gameObject.CustomSetActive(true);
                Singleton<CMiShuSystem>.instance.InitList(this.m_tabIndex, this.tasklistScript_mishu);
            }
        }
    }

    public class CTaskUT
    {
        public static void ShowTaskAward(CUIFormScript formScript, CTask task, GameObject awardContainer)
        {
            if (((formScript != null) && (awardContainer != null)) && (task.m_baseId != 0))
            {
                ResTaskReward resAward = task.resAward;
                if (resAward != null)
                {
                    for (int i = 0; i < resAward.astRewardInfo.Length; i++)
                    {
                        ResTaskRewardDetail detail = resAward.astRewardInfo[i];
                        GameObject gameObject = awardContainer.GetComponent<Transform>().FindChild(string.Format("itemCell{0}", i)).gameObject;
                        if ((detail != null) && (detail.iCnt > 0))
                        {
                            CUseable itemUseable = CUseableManager.CreateUsableByServerType((RES_REWARDS_TYPE) detail.dwRewardType, detail.iCnt, detail.dwRewardID);
                            CUICommonSystem.SetItemCell(formScript, gameObject, itemUseable, true, false);
                            gameObject.transform.FindChild("lblIconCount").GetComponent<Text>().text = string.Format("x{0}", detail.iCnt.ToString());
                            gameObject.gameObject.CustomSetActive(true);
                        }
                        else
                        {
                            gameObject.CustomSetActive(false);
                        }
                    }
                }
            }
        }
    }

    public enum enTaskFormWidget
    {
        day_huoyue_txt = 11,
        day_progress = 10,
        day_progress_bg = 9,
        m_container = 8,
        None = -1,
        Reserve = 0,
        tablistScript = 1,
        tasklistScript_main = 2,
        tasklistScript_mishu = 4,
        tasklistScript_usual = 3,
        week_huoyue_text = 5,
        week_node1 = 6,
        week_node2 = 7
    }
}

