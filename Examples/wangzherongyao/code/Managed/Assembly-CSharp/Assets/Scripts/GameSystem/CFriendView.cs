namespace Assets.Scripts.GameSystem
{
    using Apollo;
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class CFriendView
    {
        private Tab _tab;
        private GameObject addFriendBtnGameObject;
        public AddFriendView addFriendView = new AddFriendView();
        private Text btnText;
        private CUIFormScript friendform;
        public CUIListScript friendListCom;
        public GameObject friendListNode;
        private Text ifnoText;
        private GameObject info_node;
        private GameObject sns_invite_btn;
        private CUIListScript tablistScript;

        private COMDT_FRIEND_INFO _get_current_info(Tab type, int index)
        {
            COMDT_FRIEND_INFO infoAtIndex = null;
            if (type == Tab.Friend_SNS)
            {
                infoAtIndex = Singleton<CFriendContoller>.GetInstance().model.GetInfoAtIndex(CFriendModel.FriendType.SNS, index);
            }
            if (type == Tab.Friend_Request)
            {
                infoAtIndex = Singleton<CFriendContoller>.GetInstance().model.GetInfoAtIndex(CFriendModel.FriendType.RequestFriend, index);
            }
            if (type == Tab.Friend)
            {
                infoAtIndex = Singleton<CFriendContoller>.GetInstance().model.GetInfoAtIndex(CFriendModel.FriendType.GameFriend, index);
            }
            return infoAtIndex;
        }

        private void _refresh_list(CUIListScript listScript, ListView<COMDT_FRIEND_INFO> data_list, FriendShower.ItemType type, bool bShowNickName, CFriendModel.FriendType friend)
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
                        FriendShower component = elemenet.GetComponent<FriendShower>();
                        COMDT_FRIEND_INFO info = data_list[i];
                        if ((component != null) && (info != null))
                        {
                            UT.ShowFriendData(info, component, type, bShowNickName, friend);
                            if (component.sendHeartButton != null)
                            {
                                if (friend == CFriendModel.FriendType.GameFriend)
                                {
                                    component.sendHeartBtn_eventScript.m_onClickEventID = enUIEventID.Friend_SendCoin;
                                }
                                else if (friend == CFriendModel.FriendType.SNS)
                                {
                                    component.sendHeartBtn_eventScript.m_onClickEventID = enUIEventID.Friend_SNS_SendCoin;
                                }
                                component.sendHeartBtn_eventScript.m_onClickEventParams.commonUInt64Param1 = info.stUin.ullUid;
                                component.sendHeartBtn_eventScript.m_onClickEventParams.commonUInt64Param2 = info.stUin.dwLogicWorldId;
                            }
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            if (this.tablistScript != null)
            {
                CUICommonSystem.DelRedDot(this.tablistScript.GetElemenet(0).gameObject);
                CUICommonSystem.DelRedDot(this.tablistScript.GetElemenet(1).gameObject);
                if (!CSysDynamicBlock.bSocialBlocked)
                {
                    CUICommonSystem.DelRedDot(this.tablistScript.GetElemenet(2).gameObject);
                }
            }
            this.friendListNode = null;
            this.friendListCom = null;
            this.addFriendBtnGameObject = null;
            this.info_node = null;
            this.btnText = null;
            this.ifnoText = null;
            this.friendform = null;
            this.sns_invite_btn = null;
            this.tablistScript = null;
            if (this.addFriendView != null)
            {
                this.addFriendView.Clear();
            }
            Singleton<CUIManager>.GetInstance().CloseForm(CFriendContoller.FriendFormPath);
        }

        public void CloseForm()
        {
            this.Clear();
        }

        public Tab GetSelectedTab()
        {
            if (this.tablistScript != null)
            {
                return (Tab) this.tablistScript.GetSelectedIndex();
            }
            return Tab.None;
        }

        public bool IsActive()
        {
            return (this.friendform != null);
        }

        public void On_Friend_Invite_SNS_Friend(CUIEvent uiEvent)
        {
            if (MonoSingleton<ShareSys>.instance.IsInstallPlatform())
            {
                string text = UT.GetText("Friend_Invite_SNS_Title");
                string desc = UT.GetText("Friend_Invite_SNS_Desc");
                Singleton<ApolloHelper>.GetInstance().ShareToFriend(text, desc);
            }
        }

        public void On_List_ElementEnable(CUIEvent uievent)
        {
            int srcWidgetIndexInBelongedList = uievent.m_srcWidgetIndexInBelongedList;
            COMDT_FRIEND_INFO info = this._get_current_info(this.CurTab, srcWidgetIndexInBelongedList);
            FriendShower component = uievent.m_srcWidget.GetComponent<FriendShower>();
            if ((component != null) && (info != null))
            {
                if (this.CurTab == Tab.Friend_SNS)
                {
                    UT.ShowFriendData(info, component, FriendShower.ItemType.Normal, true, CFriendModel.FriendType.SNS);
                    component.sendHeartBtn_eventScript.m_onClickEventParams.commonUInt64Param1 = info.stUin.ullUid;
                    component.sendHeartBtn_eventScript.m_onClickEventParams.commonUInt64Param2 = info.stUin.dwLogicWorldId;
                }
                else if (this.CurTab == Tab.Friend)
                {
                    UT.ShowFriendData(info, component, FriendShower.ItemType.Normal, false, CFriendModel.FriendType.GameFriend);
                    component.sendHeartBtn_eventScript.m_onClickEventParams.commonUInt64Param1 = info.stUin.ullUid;
                    component.sendHeartBtn_eventScript.m_onClickEventParams.commonUInt64Param2 = info.stUin.dwLogicWorldId;
                }
                else if (this.CurTab == Tab.Friend_Request)
                {
                    UT.ShowFriendData(info, component, FriendShower.ItemType.Request, false, CFriendModel.FriendType.RequestFriend);
                }
            }
        }

        private void On_SearchFriend(CUIEvent uiEvent)
        {
            this.addFriendView.On_SearchFriend(uiEvent);
        }

        public void On_Tab_Change(int index)
        {
            if (CSysDynamicBlock.bFriendBlocked && (index >= 0))
            {
                index++;
            }
            this.CurTab = (Tab) index;
        }

        public void OpenForm(CUIEvent uiEvent)
        {
            this.friendform = Singleton<CUIManager>.GetInstance().OpenForm(CFriendContoller.FriendFormPath, false, true);
            GameObject gameObject = this.friendform.gameObject;
            this.friendListNode = gameObject.transform.Find("node/Image/FriendList").gameObject;
            this.friendListNode.CustomSetActive(true);
            this.friendListCom = this.friendListNode.GetComponent<CUIListScript>();
            this.addFriendBtnGameObject = Utility.FindChild(gameObject, "node/Buttons/Add");
            this.info_node = gameObject.transform.Find("node/Image/info_node").gameObject;
            this.info_node.CustomSetActive(false);
            this.ifnoText = Utility.GetComponetInChild<Text>(gameObject, "node/Image/info_node/Text");
            this.ifnoText.text = Singleton<CTextManager>.instance.GetText("Friend_NoFriend_Tip");
            string text = Singleton<CTextManager>.instance.GetText("FriendAdd_Tab_QQ");
            if (Singleton<ApolloHelper>.GetInstance().CurPlatform == ApolloPlatform.Wechat)
            {
                text = Singleton<CTextManager>.instance.GetText("FriendAdd_Tab_Weixin");
            }
            this.tablistScript = gameObject.transform.Find("TopCommon/Panel_Menu/List").gameObject.GetComponent<CUIListScript>();
            string[] strArray = new string[] { text, UT.GetText("Friend_Title_List"), UT.GetText("Friend_Title_Requests") };
            string[] strArray2 = new string[] { UT.GetText("Friend_Title_List"), UT.GetText("Friend_Title_Requests") };
            string[] strArray3 = !CSysDynamicBlock.bSocialBlocked ? strArray : strArray2;
            Tab tab = !CSysDynamicBlock.bSocialBlocked ? Tab.Friend_SNS : Tab.Friend;
            this.tablistScript.SetElementAmount(strArray3.Length);
            for (int i = 0; i < this.tablistScript.m_elementAmount; i++)
            {
                this.tablistScript.GetElemenet(i).gameObject.transform.Find("Text").GetComponent<Text>().text = strArray3[i];
            }
            this.btnText = Utility.GetComponetInChild<Text>(gameObject, "node/Buttons/Invite/Text");
            this.sns_invite_btn = gameObject.transform.Find("node/Buttons/Invite").gameObject;
            string str2 = Singleton<CTextManager>.instance.GetText("FriendAdd_Invite_Btn_QQ");
            if (Singleton<ApolloHelper>.GetInstance().CurPlatform == ApolloPlatform.Wechat)
            {
                str2 = Singleton<CTextManager>.instance.GetText("FriendAdd_Invite_Btn_Weixin");
            }
            this.btnText.text = str2;
            this.sns_invite_btn.CustomSetActive(false);
            this.tablistScript.m_alwaysDispatchSelectedChangeEvent = true;
            this.tablistScript.SelectElement((int) tab, true);
            this.tablistScript.m_alwaysDispatchSelectedChangeEvent = false;
            this._tab = Tab.None;
            this.Refresh_Tab();
            this.CurTab = tab;
            bool bActive = false;
            long curTime = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().getCurrentTimeSinceLogin();
            if (MonoSingleton<BannerImageSys>.GetInstance().QQBOXInfo.isTimeValid(curTime))
            {
                bActive = true;
            }
            GameObject obj4 = Utility.FindChild(gameObject, "node/Buttons/QQBoxBtn");
            if (obj4 != null)
            {
                if ((ApolloConfig.platform == ApolloPlatform.QQ) || (ApolloConfig.platform == ApolloPlatform.WTLogin))
                {
                    obj4.CustomSetActive(bActive);
                }
                else
                {
                    obj4.CustomSetActive(false);
                }
                if (CSysDynamicBlock.bLobbyEntryBlocked && (obj4 != null))
                {
                    obj4.CustomSetActive(false);
                }
            }
        }

        public void OpenSearchForm()
        {
            this.addFriendView.Init();
        }

        public void Refresh()
        {
            this.Refresh_Tab();
            this.Refresh_List(this.CurTab);
            if ((this.addFriendView != null) && this.addFriendView.bShow)
            {
                this.addFriendView.Refresh();
            }
        }

        public void Refresh_List(Tab type)
        {
            ListView<COMDT_FRIEND_INFO> list = null;
            if (type == Tab.Friend_SNS)
            {
                list = Singleton<CFriendContoller>.instance.model.GetList(CFriendModel.FriendType.SNS);
                this._refresh_list(this.friendListCom, list, FriendShower.ItemType.Normal, true, CFriendModel.FriendType.SNS);
            }
            else if (type == Tab.Friend_Request)
            {
                list = Singleton<CFriendContoller>.instance.model.GetList(CFriendModel.FriendType.RequestFriend);
                this._refresh_list(this.friendListCom, list, FriendShower.ItemType.Request, false, CFriendModel.FriendType.RequestFriend);
            }
            else if (type == Tab.Friend)
            {
                list = Singleton<CFriendContoller>.instance.model.GetList(CFriendModel.FriendType.GameFriend);
                this._refresh_list(this.friendListCom, list, FriendShower.ItemType.Normal, false, CFriendModel.FriendType.GameFriend);
            }
        }

        public void Refresh_Tab()
        {
            int dataCount = Singleton<CFriendContoller>.GetInstance().model.GetDataCount(CFriendModel.FriendType.RequestFriend);
            int index = !CSysDynamicBlock.bSocialBlocked ? 2 : 1;
            if (dataCount > 0)
            {
                CUICommonSystem.AddRedDot(this.tablistScript.GetElemenet(index).gameObject, enRedDotPos.enTopRight, dataCount);
            }
            else
            {
                CUICommonSystem.DelRedDot(this.tablistScript.GetElemenet(index).gameObject);
            }
        }

        public void Show_Search_Result(COMDT_FRIEND_INFO info)
        {
            if ((this.addFriendView != null) && this.addFriendView.bShow)
            {
                this.addFriendView.Record_SearchFriend(info);
                this.addFriendView.Show_Search_Result(info);
            }
        }

        public Tab CurTab
        {
            get
            {
                return this._tab;
            }
            set
            {
                if (this._tab != value)
                {
                    this._tab = value;
                    this.Refresh_List(this.CurTab);
                    this.info_node.CustomSetActive(false);
                    this.sns_invite_btn.CustomSetActive(false);
                    if (this._tab == Tab.Friend)
                    {
                        if (CSysDynamicBlock.bFriendBlocked)
                        {
                            if (this.addFriendBtnGameObject != null)
                            {
                                this.addFriendBtnGameObject.CustomSetActive(false);
                            }
                        }
                        else if (this.addFriendBtnGameObject != null)
                        {
                            this.addFriendBtnGameObject.CustomSetActive(true);
                        }
                    }
                    switch (this._tab)
                    {
                        case Tab.Friend_SNS:
                        {
                            this.ifnoText.text = Singleton<CTextManager>.instance.GetText("Friend_NoFriend_Tip");
                            int dataCount = Singleton<CFriendContoller>.GetInstance().model.GetDataCount(CFriendModel.FriendType.SNS);
                            this.info_node.CustomSetActive(dataCount == 0);
                            this.sns_invite_btn.CustomSetActive(!CSysDynamicBlock.bSocialBlocked);
                            return;
                        }
                        case Tab.Friend:
                        {
                            this.ifnoText.text = Singleton<CTextManager>.instance.GetText("Friend_NoFriend_Tip");
                            int num = Singleton<CFriendContoller>.GetInstance().model.GetDataCount(CFriendModel.FriendType.GameFriend);
                            this.info_node.CustomSetActive(num == 0);
                            return;
                        }
                        case Tab.Friend_Request:
                        {
                            this.ifnoText.text = Singleton<CTextManager>.instance.GetText("Friend_NoRequest_Tip");
                            int num3 = Singleton<CFriendContoller>.GetInstance().model.GetDataCount(CFriendModel.FriendType.RequestFriend);
                            this.info_node.CustomSetActive(num3 == 0);
                            return;
                        }
                    }
                }
            }
        }

        public class AddFriendView
        {
            public bool bShow;
            public GameObject buttons_node;
            private GameObject info_text;
            private Text input;
            public CUIListScript recommandFriendListCom;
            private static readonly Vector2 recommandFriendListPos1 = new Vector2(40f, 190f);
            private static readonly Vector2 recommandFriendListPos2 = new Vector2(40f, 340f);
            private static readonly Vector2 recommandFriendListSize1 = new Vector2(-80f, 180f);
            private static readonly Vector2 recommandFriendListSize2 = new Vector2(-80f, 320f);
            public COMDT_FRIEND_INFO search_info_Game;
            private FriendShower searchFriendShower;

            public void Clear()
            {
                Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Friend_SerchFriend, new CUIEventManager.OnUIEventHandler(this.On_SearchFriend));
                Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Friend_Close_AddForm, new CUIEventManager.OnUIEventHandler(this.On_Friend_Close_AddForm));
                this.input = null;
                this.searchFriendShower = null;
                this.recommandFriendListCom = null;
                this.search_info_Game = null;
                this.buttons_node = null;
                this.bShow = false;
            }

            public void Clear_SearchFriend()
            {
                this.search_info_Game = null;
            }

            public void Init()
            {
                Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_SerchFriend, new CUIEventManager.OnUIEventHandler(this.On_SearchFriend));
                Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_Close_AddForm, new CUIEventManager.OnUIEventHandler(this.On_Friend_Close_AddForm));
                CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(CFriendContoller.AddFriendFormPath, false, true);
                this.input = script.transform.FindChild("GameObject/SearchFriend/InputField/Text").GetComponent<Text>();
                this.searchFriendShower = script.transform.FindChild("GameObject/SearchFriend/Result/Friend").GetComponent<FriendShower>();
                this.searchFriendShower.gameObject.CustomSetActive(false);
                this.recommandFriendListCom = Utility.GetComponetInChild<CUIListScript>(script.gameObject, "GameObject/RecommandList");
                this.buttons_node = script.transform.FindChild("GameObject/Buttons").gameObject;
                this.info_text = script.transform.Find("GameObject/SearchFriend/Result/info").gameObject;
                if (this.info_text != null)
                {
                    this.info_text.CustomSetActive(false);
                }
                FriendSysNetCore.Send_Request_RecommandFriend_List();
                this.Refresh();
                this.bShow = true;
            }

            public void On_Friend_Close_AddForm(CUIEvent uiEvent)
            {
                this.Clear();
                Singleton<CUIManager>.GetInstance().CloseForm(CFriendContoller.AddFriendFormPath);
            }

            public void On_SearchFriend(CUIEvent uiEvent)
            {
                this.Clear_SearchFriend();
                this.searchFriendShower.gameObject.CustomSetActive(false);
                if (string.IsNullOrEmpty(this.input.text))
                {
                    Singleton<CUIManager>.GetInstance().OpenMessageBox(UT.GetText("Friend_Input_Tips"), false);
                }
                else
                {
                    FriendSysNetCore.Send_Serch_Player(this.input.text);
                }
                this.Refresh_Friend_Recommand_List_Pos();
            }

            public void Record_SearchFriend(COMDT_FRIEND_INFO info)
            {
                this.search_info_Game = info;
            }

            public void Refresh()
            {
                this.buttons_node.CustomSetActive(false);
                this.Show_Search_Game();
                this.Show_Search_Result(null);
            }

            public void Refresh_Friend_Recommand_List()
            {
                Singleton<CFriendContoller>.GetInstance().model.FilterRecommendFriends();
                int dataCount = Singleton<CFriendContoller>.GetInstance().model.GetDataCount(CFriendModel.FriendType.Recommend);
                this.recommandFriendListCom.SetElementAmount(dataCount);
                COMDT_FRIEND_INFO info = null;
                for (int i = 0; i < dataCount; i++)
                {
                    info = Singleton<CFriendContoller>.GetInstance().model.GetInfoAtIndex(CFriendModel.FriendType.Recommend, i);
                    if (info != null)
                    {
                        this.Refresh_Recomand_Friend(i, info);
                    }
                }
            }

            public void Refresh_Friend_Recommand_List_Pos()
            {
                if ((this.recommandFriendListCom != null) && (this.searchFriendShower != null))
                {
                    RectTransform transform = this.recommandFriendListCom.transform as RectTransform;
                    if (this.searchFriendShower.gameObject.activeSelf)
                    {
                        transform.anchoredPosition = recommandFriendListPos1;
                        transform.sizeDelta = recommandFriendListSize1;
                    }
                    else
                    {
                        transform.anchoredPosition = recommandFriendListPos2;
                        transform.sizeDelta = recommandFriendListSize2;
                    }
                }
            }

            public void Refresh_Recomand_Friend(int index, COMDT_FRIEND_INFO info)
            {
                CUIListElementScript elemenet = this.recommandFriendListCom.GetElemenet(index);
                if (elemenet != null)
                {
                    FriendShower component = elemenet.GetComponent<FriendShower>();
                    if (component != null)
                    {
                        UT.ShowFriendData(info, component, FriendShower.ItemType.Add, false, CFriendModel.FriendType.Recommend);
                    }
                }
            }

            private void Show_Search_Game()
            {
                this.Refresh_Friend_Recommand_List();
                this.Refresh_Friend_Recommand_List_Pos();
            }

            public void Show_Search_Result(COMDT_FRIEND_INFO info)
            {
                COMDT_FRIEND_INFO comdt_friend_info = null;
                comdt_friend_info = this.search_info_Game;
                if (comdt_friend_info == null)
                {
                    if (this.searchFriendShower != null)
                    {
                        this.searchFriendShower.gameObject.CustomSetActive(false);
                    }
                }
                else if (this.searchFriendShower != null)
                {
                    this.searchFriendShower.gameObject.CustomSetActive(true);
                    UT.ShowFriendData(comdt_friend_info, this.searchFriendShower, FriendShower.ItemType.Add, false, CFriendModel.FriendType.RequestFriend);
                }
            }
        }

        public enum Tab
        {
            Friend_SNS,
            Friend,
            Friend_Request,
            Friend_Invite,
            None
        }
    }
}

