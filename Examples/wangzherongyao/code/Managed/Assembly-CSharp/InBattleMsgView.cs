using Assets.Scripts.Framework;
using Assets.Scripts.GameLogic;
using Assets.Scripts.GameLogic.GameKernal;
using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using CSProtocol;
using ResData;
using System;
using System.Runtime.InteropServices;

public class InBattleMsgView
{
    private CUIListScript contentList;
    public static readonly string InBattleMsgView_FORM_PATH = "UGUI/Form/System/Chat/Form_InBattleChat.prefab";
    private CUIFormScript m_CUIForm;
    private int m_tabIndex = -1;
    private CUIListScript tablistScript;

    private TabElement _get_current_info(int tabIndex, int list_index)
    {
        if (tabIndex >= Singleton<InBattleMsgMgr>.instance.title_list.Count)
        {
            return null;
        }
        ListView<TabElement> view = null;
        string key = Singleton<InBattleMsgMgr>.instance.title_list[tabIndex];
        Singleton<InBattleMsgMgr>.instance.tabElements.TryGetValue(key, out view);
        if (view == null)
        {
            return null;
        }
        if (list_index >= view.Count)
        {
            return null;
        }
        return view[list_index];
    }

    private void _refresh_list(CUIListScript listScript, ListView<TabElement> data_list)
    {
        if (((listScript != null) && (data_list != null)) && (data_list.Count != 0))
        {
            int count = data_list.Count;
            listScript.SetElementAmount(count);
        }
    }

    public void Clear()
    {
        this.m_tabIndex = -1;
        this.tablistScript = null;
        this.contentList = null;
        this.m_CUIForm = null;
        Singleton<CUIManager>.GetInstance().CloseForm(InBattleMsgView_FORM_PATH);
    }

    public void On_InBattleMsg_ListElement_Click(int index)
    {
        this.Show(false);
        TabElement element = this._get_current_info(this.TabIndex, index);
        if (element != null)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if (hostPlayer != null)
            {
                ResInBatMsgCfg cfgData = Singleton<InBattleMsgMgr>.instance.GetCfgData(element.cfgId);
                DebugHelper.Assert(cfgData != null, "InbattleMsgView cfg_data == null");
                if (cfgData != null)
                {
                    SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
                    if (curLvelContext != null)
                    {
                        if (!this.ShouldBeThroughNet(curLvelContext))
                        {
                            Singleton<InBattleMsgMgr>.instance.InnerHandle_InBatMsg((COM_INBATTLE_CHAT_TYPE) cfgData.bShowType, hostPlayer.CaptainId, element.cfgId, hostPlayer.PlayerUId);
                        }
                        else
                        {
                            InBattleMsgNetCore.SendInBattleMsg_PreConfig(element.cfgId, (COM_INBATTLE_CHAT_TYPE) cfgData.bShowType, hostPlayer.CaptainId);
                        }
                        if (Singleton<InBattleMsgMgr>.instance.m_cdButton != null)
                        {
                            ResInBatChannelCfg dataByKey = GameDataMgr.inBattleChannelDatabin.GetDataByKey(cfgData.bInBatChannelID);
                            if (dataByKey != null)
                            {
                                Singleton<InBattleMsgMgr>.instance.m_cdButton.StartCooldown(dataByKey.dwCdTime);
                            }
                            else
                            {
                                Singleton<InBattleMsgMgr>.instance.m_cdButton.StartCooldown(0xfa0);
                            }
                        }
                    }
                }
            }
        }
    }

    public void On_InBattleMsg_ListElement_Enable(CUIEvent uievent)
    {
        int srcWidgetIndexInBelongedList = uievent.m_srcWidgetIndexInBelongedList;
        TabElement element = this._get_current_info(this.m_tabIndex, srcWidgetIndexInBelongedList);
        InBattleMsgShower component = uievent.m_srcWidget.GetComponent<InBattleMsgShower>();
        if ((component != null) && (element != null))
        {
            component.Set(element.cfgId, element.content);
        }
    }

    public void On_InBattleMsg_TabChange(int index)
    {
        this.TabIndex = index;
    }

    public void OpenForm(CUIEvent uiEvent, bool bShow = true)
    {
        if (this.m_CUIForm != null)
        {
            this.m_CUIForm.gameObject.CustomSetActive(true);
            this.m_tabIndex = -1;
            if (this.tablistScript != null)
            {
                this.tablistScript.m_alwaysDispatchSelectedChangeEvent = true;
                this.tablistScript.SelectElement(0, true);
                this.tablistScript.m_alwaysDispatchSelectedChangeEvent = false;
            }
            if (this.contentList != null)
            {
                this.contentList.SelectElement(-1, false);
            }
        }
        else
        {
            this.m_CUIForm = Singleton<CUIManager>.GetInstance().OpenForm(InBattleMsgView_FORM_PATH, true, true);
            DebugHelper.Assert(this.m_CUIForm != null, "InbattleMsgView m_CUIForm == null");
            if (this.m_CUIForm != null)
            {
                this.tablistScript = this.m_CUIForm.transform.Find("chatTools/node/Tab/List").GetComponent<CUIListScript>();
                this.contentList = this.m_CUIForm.transform.Find("chatTools/node/ListView/List").GetComponent<CUIListScript>();
                DebugHelper.Assert(this.tablistScript != null, "InbattleMsgView tablistScript == null");
                DebugHelper.Assert(this.contentList != null, "InbattleMsgView contentList == null");
                if ((this.tablistScript != null) && (this.contentList != null))
                {
                    UT.SetTabList(Singleton<InBattleMsgMgr>.instance.title_list, 0, this.tablistScript);
                    if (!bShow)
                    {
                        this.m_CUIForm.gameObject.CustomSetActive(false);
                    }
                }
            }
        }
    }

    public void Refresh_List(int tabIndex)
    {
        InBattleMsgMgr instance = Singleton<InBattleMsgMgr>.instance;
        if (tabIndex < instance.title_list.Count)
        {
            ListView<TabElement> view = null;
            if (tabIndex < instance.title_list.Count)
            {
                string key = instance.title_list[tabIndex];
                instance.tabElements.TryGetValue(key, out view);
            }
            if (view != null)
            {
                this._refresh_list(this.contentList, view);
            }
        }
    }

    private bool ShouldBeThroughNet(SLevelContext levelContent)
    {
        if (levelContent == null)
        {
            return false;
        }
        if (levelContent.isWarmBattle && (levelContent.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_COMBAT))
        {
            return false;
        }
        return true;
    }

    public void Show(bool bShow)
    {
        if (this.m_CUIForm != null)
        {
            this.m_CUIForm.gameObject.CustomSetActive(bShow);
        }
    }

    public int TabIndex
    {
        get
        {
            return this.m_tabIndex;
        }
        set
        {
            if (this.m_tabIndex != value)
            {
                this.m_tabIndex = value;
                this.Refresh_List(this.m_tabIndex);
            }
        }
    }
}

