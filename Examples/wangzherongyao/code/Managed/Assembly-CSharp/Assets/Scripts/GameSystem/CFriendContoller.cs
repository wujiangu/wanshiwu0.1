namespace Assets.Scripts.GameSystem
{
    using Apollo;
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using UnityEngine;

    public class CFriendContoller : Singleton<CFriendContoller>
    {
        public static string AddFriendFormPath = "UGUI/Form/System/Friend/AddFriend.prefab";
        private GameObject com;
        public static string FriendFormPath = "UGUI/Form/System/Friend/FriendForm.prefab";
        public CFriendModel model = new CFriendModel();
        public COMDT_FRIEND_INFO search_info;
        public CFriendView view = new CFriendView();

        private void Add_And_Refresh(CFriendModel.FriendType type, COMDT_FRIEND_INFO data)
        {
            this.model.Add(type, data, false);
            if (type == CFriendModel.FriendType.GameFriend)
            {
                Singleton<CFriendContoller>.GetInstance().model.SortGameFriend();
            }
            if (this.view.IsActive())
            {
                this.view.Refresh();
            }
        }

        public void ClearAll()
        {
            this.model.ClearAll();
            this.search_info = null;
            this.com = null;
        }

        private ulong GetFriendUid(CUIEvent uiEvent)
        {
            FriendShower component = uiEvent.m_srcWidget.transform.parent.parent.parent.GetComponent<FriendShower>();
            if (component == null)
            {
                return 0L;
            }
            CFriendModel.FriendType type = (this.view.GetSelectedTab() != CFriendView.Tab.Friend_SNS) ? CFriendModel.FriendType.GameFriend : CFriendModel.FriendType.SNS;
            COMDT_FRIEND_INFO comdt_friend_info = this.model.GetInfo(type, component.ullUid, component.dwLogicWorldID);
            if (comdt_friend_info == null)
            {
                return 0L;
            }
            return comdt_friend_info.stUin.ullUid;
        }

        private void Handle_CoinSend_Data(CSDT_FRIEND_INFO info)
        {
            this.Update_Send_Coin_Data(info.stFriendInfo.stUin, info.ullDonateAPSec, COM_FRIEND_TYPE.COM_FRIEND_TYPE_GAME);
        }

        private void Handle_CoinSend_Data(CSDT_SNS_FRIEND_INFO info)
        {
            this.Update_Send_Coin_Data(info.stSnsFrindInfo.stUin, (ulong) info.dwDonateTime, COM_FRIEND_TYPE.COM_FRIEND_TYPE_SNS);
        }

        private void Handle_Invite_Data(COMDT_ACNT_UNIQ uin)
        {
            this.model.SnsReCallData.Add(uin, COM_FRIEND_TYPE.COM_FRIEND_TYPE_SNS);
        }

        public override void Init()
        {
            base.Init();
            this.InitEvent();
        }

        private void InitEvent()
        {
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_List", new Action<CSPkg>(this.On_FriendSys_Friend_List));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_Request_List", new Action<CSPkg>(this.On_FriendSys_Friend_Request_List));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_Recommand_List", new Action<CSPkg>(this.On_FriendSys_Friend_Recommand_List));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_Search", new Action<CSPkg>(this.On_FriendSys_Friend_Search));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_RequestBeFriend", new Action<CSPkg>(this.On_FriendSys_Friend_RequestBeFriend));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_Confrim", new Action<CSPkg>(this.On_FriendSys_Friend_Confrim));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_Deny", new Action<CSPkg>(this.On_FriendSys_Friend_Deny));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_Delete", new Action<CSPkg>(this.On_FriendSys_Friend_Delete));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_ADD_NTF", new Action<CSPkg>(this.On_FriendSys_Friend_ADD_NTF));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_Delete_NTF", new Action<CSPkg>(this.On_FriendSys_Friend_Delete_NTF));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_Request_NTF", new Action<CSPkg>(this.On_FriendSys_Friend_Request_NTF));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_Login_NTF", new Action<CSPkg>(this.On_Friend_Login_NTF));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_GAME_STATE_NTF", new Action<CSPkg>(this.On_Friend_GAME_STATE_NTF));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_SNS_STATE_NTF", new Action<CSPkg>(this.On_Friend_SNS_STATE_NTF));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_SNS_CHG_PROFILE", new Action<CSPkg>(this.On_Friend_SNS_CHG_PROFILE));
            Singleton<EventRouter>.GetInstance().AddEventHandler("Friend_RecommandFriend_Refresh", new Action(this, (IntPtr) this.On_Friend_RecommandFriend_Refresh));
            Singleton<EventRouter>.GetInstance().AddEventHandler("Friend_FriendList_Refresh", new Action(this, (IntPtr) this.On_Friend_FriendList_Refresh));
            Singleton<EventRouter>.GetInstance().AddEventHandler("Friend_SNSFriendList_Refresh", new Action(this, (IntPtr) this.On_Friend_SNSFriendList_Refresh));
            Singleton<EventRouter>.GetInstance().AddEventHandler("Guild_Invite_Success", new Action(this, (IntPtr) this.On_GuildSys_Guild_Invite_Success));
            Singleton<EventRouter>.GetInstance().AddEventHandler("Guild_Recommend_Success", new Action(this, (IntPtr) this.On_GuildSys_Guild_Recommend_Success));
            Singleton<EventRouter>.GetInstance().AddEventHandler(EventID.NEWDAY_NTF, new Action(this, (IntPtr) this.OnNewDayNtf));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_OpenForm, new CUIEventManager.OnUIEventHandler(this.On_OpenForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_CloseForm, new CUIEventManager.OnUIEventHandler(this.On_CloseForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_Tab_Change, new CUIEventManager.OnUIEventHandler(this.On_TabChange));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_OpenAddFriendForm, new CUIEventManager.OnUIEventHandler(this.On_OpenAddFriendForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_RequestBeFriend, new CUIEventManager.OnUIEventHandler(this.On_AddFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_Accept_RequestFriend, new CUIEventManager.OnUIEventHandler(this.On_Accept_RequestFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_Refuse_RequestFriend, new CUIEventManager.OnUIEventHandler(this.On_Refuse_RequestFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_DelFriend, new CUIEventManager.OnUIEventHandler(this.On_DelFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_Tab_Friend, new CUIEventManager.OnUIEventHandler(this.On_Friend_Tab_Friend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_Tab_FriendRequest, new CUIEventManager.OnUIEventHandler(this.On_Friend_Tab_FriendRequest));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_DelFriend_OK, new CUIEventManager.OnUIEventHandler(this.On_DelFriend_OK));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_DelFriend_Cancle, new CUIEventManager.OnUIEventHandler(this.On_Friend_DelFriend_Cancle));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_SendCoin, new CUIEventManager.OnUIEventHandler(this.On_Friend_SendCoin));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_SNS_SendCoin, new CUIEventManager.OnUIEventHandler(this.On_SNSFriend_SendCoin));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_InviteGuild, new CUIEventManager.OnUIEventHandler(this.On_Friend_InviteGuild));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_RecommendGuild, new CUIEventManager.OnUIEventHandler(this.On_Friend_RecommendGuild));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_CheckInfo, new CUIEventManager.OnUIEventHandler(this.On_Friend_CheckInfo));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_List_ElementEnable, new CUIEventManager.OnUIEventHandler(this.On_Friend_List_ElementEnable));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_Invite_SNS_Friend, new CUIEventManager.OnUIEventHandler(this.On_Friend_Invite_SNS_Friend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_Share_SendCoin, new CUIEventManager.OnUIEventHandler(this.On_Friend_Share_SendCoin));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Friend_SNS_ReCall, new CUIEventManager.OnUIEventHandler(this.On_Friend_SNS_ReCall));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.QQBOX_OnClick, new CUIEventManager.OnUIEventHandler(this.QQBox_OnClick));
        }

        private void On_Accept_RequestFriend(CUIEvent uievent)
        {
            FriendShower component = uievent.m_srcWidget.transform.parent.parent.parent.GetComponent<FriendShower>();
            if (component != null)
            {
                COMDT_FRIEND_INFO comdt_friend_info = this.model.GetInfo(CFriendModel.FriendType.RequestFriend, component.ullUid, component.dwLogicWorldID);
                if (comdt_friend_info != null)
                {
                    FriendSysNetCore.Send_Confrim_BeFriend(comdt_friend_info.stUin);
                }
            }
        }

        private void On_AddFriend(CUIEvent evt)
        {
            COMDT_FRIEND_INFO comdt_friend_info = Singleton<CFriendContoller>.GetInstance().search_info;
            if ((evt.m_srcWidgetBelongedListScript == null) && (comdt_friend_info != null))
            {
                FriendSysNetCore.Send_Request_BeFriend(comdt_friend_info.stUin);
            }
            else
            {
                FriendShower component = evt.m_srcWidget.transform.parent.parent.parent.GetComponent<FriendShower>();
                if (component != null)
                {
                    COMDT_FRIEND_INFO data = this.model.GetInfo(CFriendModel.FriendType.Recommend, component.ullUid, component.dwLogicWorldID);
                    FriendSysNetCore.Send_Request_BeFriend(component.ullUid, component.dwLogicWorldID);
                    if (data != null)
                    {
                        this.model.Remove(CFriendModel.FriendType.Recommend, data);
                    }
                }
            }
        }

        private void On_CloseForm(CUIEvent uiEvent)
        {
            this.view.CloseForm();
        }

        private void On_DelFriend(CUIEvent evt)
        {
            this.com = evt.m_srcWidget;
            Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(UT.GetText("Friend_Tips_DelFriend"), enUIEventID.Friend_DelFriend_OK, enUIEventID.Friend_DelFriend_Cancle, false);
        }

        private void On_DelFriend_OK(CUIEvent evt)
        {
            FriendShower component = this.com.transform.parent.parent.parent.GetComponent<FriendShower>();
            if (component != null)
            {
                COMDT_FRIEND_INFO comdt_friend_info = this.model.GetInfo(CFriendModel.FriendType.GameFriend, component.ullUid, component.dwLogicWorldID);
                if (comdt_friend_info != null)
                {
                    FriendSysNetCore.Send_Del_Friend(comdt_friend_info.stUin);
                }
            }
        }

        private void On_Friend_CheckInfo(CUIEvent uievent)
        {
            FriendShower component = uievent.m_srcWidget.transform.parent.parent.parent.GetComponent<FriendShower>();
            if (component != null)
            {
                Singleton<CPlayerInfoSystem>.instance.ShowPlayerDetailInfo(component.ullUid, (int) component.dwLogicWorldID, CPlayerInfoSystem.DetailPlayerInfoSource.DefaultOthers);
            }
        }

        private void On_Friend_DelFriend_Cancle(CUIEvent evt)
        {
        }

        private void On_Friend_FriendList_Refresh()
        {
            if ((this.view != null) && this.view.IsActive())
            {
                this.view.Refresh();
            }
        }

        private void On_Friend_GAME_STATE_NTF(CSPkg msg)
        {
            SCPKG_CMD_NTF_FRIEND_GAME_STATE stNtfFriendGameState = msg.stPkgData.stNtfFriendGameState;
            this.model.SetFriendGameState(stNtfFriendGameState.stAcntUniq.ullUid, stNtfFriendGameState.stAcntUniq.dwLogicWorldId, (COM_ACNT_GAME_STATE) stNtfFriendGameState.bGameState, string.Empty, false);
            if ((this.view != null) && this.view.IsActive())
            {
                this.view.Refresh();
            }
            Singleton<EventRouter>.instance.BroadCastEvent(EventID.Friend_Game_State_Change);
        }

        private void On_Friend_Invite_SNS_Friend(CUIEvent uievent)
        {
            if ((this.view != null) && this.view.IsActive())
            {
                this.view.On_Friend_Invite_SNS_Friend(uievent);
            }
        }

        private void On_Friend_InviteGuild(CUIEvent uiEvent)
        {
            Singleton<EventRouter>.GetInstance().BroadCastEvent<ulong>("Guild_Invite", this.GetFriendUid(uiEvent));
        }

        private void On_Friend_List_ElementEnable(CUIEvent uievent)
        {
            if ((this.view != null) && this.view.IsActive())
            {
                this.view.On_List_ElementEnable(uievent);
            }
        }

        private void On_Friend_Login_NTF(CSPkg msg)
        {
            SCPKG_CMD_NTF_FRIEND_LOGIN_STATUS stNtfFriendLoginStatus = msg.stPkgData.stNtfFriendLoginStatus;
            CFriendModel.FriendType type = (stNtfFriendLoginStatus.bFriendType != 1) ? CFriendModel.FriendType.SNS : CFriendModel.FriendType.GameFriend;
            COMDT_FRIEND_INFO comdt_friend_info = Singleton<CFriendContoller>.GetInstance().model.GetInfo(type, stNtfFriendLoginStatus.stUin);
            if (comdt_friend_info != null)
            {
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
                if (masterRoleInfo != null)
                {
                    comdt_friend_info.bIsOnline = stNtfFriendLoginStatus.bLoginStatus;
                    comdt_friend_info.dwLastLoginTime = (uint) masterRoleInfo.getCurrentTimeSinceLogin();
                    Singleton<CFriendContoller>.GetInstance().model.SortGameFriend();
                    if ((this.view != null) && this.view.IsActive())
                    {
                        this.view.Refresh();
                    }
                }
            }
        }

        private void On_Friend_RecommandFriend_Refresh()
        {
        }

        private void On_Friend_RecommendGuild(CUIEvent uiEvent)
        {
            Singleton<EventRouter>.GetInstance().BroadCastEvent<ulong>("Guild_Recommend", this.GetFriendUid(uiEvent));
        }

        private void On_Friend_SendCoin(CUIEvent uievent)
        {
            ulong ullUid = uievent.m_eventParams.commonUInt64Param1;
            uint dwLogicWorldID = (uint) uievent.m_eventParams.commonUInt64Param2;
            COMDT_FRIEND_INFO comdt_friend_info = this.model.GetInfo(CFriendModel.FriendType.GameFriend, ullUid, dwLogicWorldID);
            if (comdt_friend_info != null)
            {
                FriendSysNetCore.Send_FriendCoin(comdt_friend_info.stUin, COM_FRIEND_TYPE.COM_FRIEND_TYPE_GAME);
            }
        }

        private void On_Friend_Share_SendCoin(CUIEvent uievent)
        {
            try
            {
                if (MonoSingleton<ShareSys>.instance.IsInstallPlatform())
                {
                    string openId = uievent.m_eventParams.snsFriendEventParams.openId;
                    Singleton<ApolloHelper>.instance.ShareSendHeart(openId, Singleton<CTextManager>.instance.GetText("Common_Sns_Tips_1"), Singleton<CTextManager>.instance.GetText("Common_Sns_Tips_2"), ShareSys.SNS_SHARE_SEND_HEART);
                }
            }
            catch (Exception exception)
            {
                object[] inParameters = new object[] { exception.Message };
                DebugHelper.Assert(false, "Exception in On_Friend_Share_SendCoin, {0}", inParameters);
            }
        }

        private void On_Friend_SNS_CHG_PROFILE(CSPkg msg)
        {
            SCPKG_CHG_SNS_FRIEND_PROFILE stSnsFriendChgProfile = msg.stPkgData.stSnsFriendChgProfile;
            this.model.Add(CFriendModel.FriendType.SNS, stSnsFriendChgProfile.stSnsFrindInfo, true);
            this.model.SetFriendGameState(stSnsFriendChgProfile.stSnsFrindInfo.stUin.ullUid, stSnsFriendChgProfile.stSnsFrindInfo.stUin.dwLogicWorldId, (COM_ACNT_GAME_STATE) stSnsFriendChgProfile.bGameState, string.Empty, true);
            this.model.SortSNSFriend();
            if ((this.view != null) && this.view.IsActive())
            {
                this.view.Refresh();
            }
            Singleton<EventRouter>.instance.BroadCastEvent(EventID.Friend_Game_State_Change);
        }

        private void On_Friend_SNS_ReCall(CUIEvent uievent)
        {
            FriendShower component = uievent.m_srcWidget.transform.parent.parent.parent.GetComponent<FriendShower>();
            if (component != null)
            {
                COMDT_FRIEND_INFO comdt_friend_info = this.model.GetInfo(CFriendModel.FriendType.SNS, component.ullUid, component.dwLogicWorldID);
                if (comdt_friend_info != null)
                {
                    FriendSysNetCore.ReCallSnsFriend(comdt_friend_info.stUin, COM_FRIEND_TYPE.COM_FRIEND_TYPE_SNS);
                }
            }
        }

        private void On_Friend_SNS_STATE_NTF(CSPkg msg)
        {
            SCPKG_NTF_SNS_FRIEND stNtfSnsFriend = msg.stPkgData.stNtfSnsFriend;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            uint num = (masterRoleInfo == null) ? 0 : ((uint) masterRoleInfo.getCurrentTimeSinceLogin());
            uint num2 = 0x15180 * GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x9e).dwConfValue;
            for (int i = 0; i < stNtfSnsFriend.dwSnsFriendNum; i++)
            {
                CSDT_SNS_FRIEND_INFO info = stNtfSnsFriend.astSnsFriendList[i];
                if (info != null)
                {
                    this.model.Add(CFriendModel.FriendType.SNS, info.stSnsFrindInfo, false);
                    this.model.SetFriendGameState(info.stSnsFrindInfo.stUin.ullUid, info.stSnsFrindInfo.stUin.dwLogicWorldId, (COM_ACNT_GAME_STATE) info.bGameState, UT.Bytes2String(info.szNickName), false);
                    this.Handle_CoinSend_Data(info);
                }
            }
            this.model.SortSNSFriend();
            if ((this.view != null) && this.view.IsActive())
            {
                this.view.Refresh();
            }
            Singleton<EventRouter>.instance.BroadCastEvent(EventID.Friend_Game_State_Change);
        }

        private void On_Friend_SNSFriendList_Refresh()
        {
            if (((this.view != null) && (this.view.addFriendView != null)) && this.view.addFriendView.bShow)
            {
                this.view.addFriendView.Refresh();
            }
        }

        private void On_Friend_Tab_Friend(CUIEvent uievent)
        {
            if ((this.view != null) && this.view.IsActive())
            {
                this.view.On_Tab_Change(0);
            }
        }

        private void On_Friend_Tab_FriendRequest(CUIEvent uievent)
        {
            if ((this.view != null) && this.view.IsActive())
            {
                this.view.On_Tab_Change(1);
            }
        }

        private void On_FriendSys_Friend_ADD_NTF(CSPkg msg)
        {
            SCPKG_CMD_NTF_FRIEND_ADD stNtfFriendAdd = msg.stPkgData.stNtfFriendAdd;
            this.Add_And_Refresh(CFriendModel.FriendType.GameFriend, stNtfFriendAdd.stUserInfo);
        }

        private void On_FriendSys_Friend_Confrim(CSPkg msg)
        {
            SCPKG_CMD_ADD_FRIEND_CONFIRM stFriendAddConfirmRsp = msg.stPkgData.stFriendAddConfirmRsp;
            COMDT_FRIEND_INFO stUserInfo = stFriendAddConfirmRsp.stUserInfo;
            if (stFriendAddConfirmRsp.dwResult == 0)
            {
                Singleton<CUIManager>.GetInstance().OpenTips(UT.GetText("Friend_Tips_BeFriend_Ok"), false, 1f, null, new object[0]);
                Singleton<CFriendContoller>.GetInstance().model.Remove(CFriendModel.FriendType.RequestFriend, stUserInfo.stUin);
                Singleton<CFriendContoller>.GetInstance().model.Add(CFriendModel.FriendType.GameFriend, stUserInfo, false);
                Singleton<CFriendContoller>.GetInstance().model.SortGameFriend();
                CFriendView view = Singleton<CFriendContoller>.GetInstance().view;
                if ((view != null) && view.IsActive())
                {
                    view.Refresh();
                }
            }
            else
            {
                Singleton<CUIManager>.GetInstance().OpenTips(UT.ErrorCode_String(stFriendAddConfirmRsp.dwResult), false, 1f, null, new object[0]);
                this.Remove_And_Refresh(CFriendModel.FriendType.RequestFriend, stUserInfo.stUin);
            }
        }

        private void On_FriendSys_Friend_Delete(CSPkg msg)
        {
            SCPKG_CMD_DEL_FRIEND stFriendDelRsp = msg.stPkgData.stFriendDelRsp;
            if (stFriendDelRsp.dwResult == 0)
            {
                this.Remove_And_Refresh(CFriendModel.FriendType.GameFriend, stFriendDelRsp.stUin);
            }
        }

        private void On_FriendSys_Friend_Delete_NTF(CSPkg msg)
        {
            SCPKG_CMD_NTF_FRIEND_DEL stNtfFriendDel = msg.stPkgData.stNtfFriendDel;
            this.Remove_And_Refresh(CFriendModel.FriendType.GameFriend, stNtfFriendDel.stUin);
        }

        private void On_FriendSys_Friend_Deny(CSPkg msg)
        {
            SCPKG_CMD_ADD_FRIEND_DENY stFriendAddDenyRsp = msg.stPkgData.stFriendAddDenyRsp;
            if (stFriendAddDenyRsp.dwResult == 0)
            {
                this.Remove_And_Refresh(CFriendModel.FriendType.RequestFriend, stFriendAddDenyRsp.stUin);
            }
            else
            {
                Singleton<CUIManager>.GetInstance().OpenTips(UT.ErrorCode_String(stFriendAddDenyRsp.dwResult), false, 1f, null, new object[0]);
                this.Remove_And_Refresh(CFriendModel.FriendType.RequestFriend, stFriendAddDenyRsp.stUin);
            }
        }

        private void On_FriendSys_Friend_List(CSPkg msg)
        {
            SCPKG_CMD_LIST_FRIEND stFriendListRsp = msg.stPkgData.stFriendListRsp;
            if (stFriendListRsp != null)
            {
                int num = Mathf.Min(stFriendListRsp.astFrindList.Length, (int) stFriendListRsp.dwFriendNum);
                for (int i = 0; i < num; i++)
                {
                    CSDT_FRIEND_INFO info = stFriendListRsp.astFrindList[i];
                    this.model.Add(CFriendModel.FriendType.GameFriend, info.stFriendInfo, false);
                    this.Handle_CoinSend_Data(info);
                }
                Singleton<CFriendContoller>.GetInstance().model.SortGameFriend();
                if ((this.view != null) && this.view.IsActive())
                {
                    this.view.Refresh();
                }
                Singleton<EventRouter>.GetInstance().BroadCastEvent("Rank_Friend_List");
            }
        }

        private void On_FriendSys_Friend_Recommand_List(CSPkg msg)
        {
            SCPKG_CMD_LIST_FREC stFRecRsp = msg.stPkgData.stFRecRsp;
            this.model.Clear(CFriendModel.FriendType.Recommend);
            if (stFRecRsp.dwResult == 0)
            {
                for (int i = 0; i < stFRecRsp.dwAcntNum; i++)
                {
                    COMDT_FRIEND_INFO data = stFRecRsp.astAcnts[i];
                    if (this.model.getFriendByUid(data.stUin.ullUid, CFriendModel.FriendType.GameFriend) == null)
                    {
                        this.model.Add(CFriendModel.FriendType.Recommend, data, false);
                    }
                }
            }
            else
            {
                Singleton<CUIManager>.GetInstance().OpenTips(UT.ErrorCode_String(stFRecRsp.dwResult), false, 1f, null, new object[0]);
            }
        }

        private void On_FriendSys_Friend_Request_List(CSPkg msg)
        {
            SCPKG_CMD_LIST_FRIENDREQ stFriendReqListRsp = msg.stPkgData.stFriendReqListRsp;
            if (stFriendReqListRsp != null)
            {
                int num = Mathf.Min(stFriendReqListRsp.astFrindReqList.Length, (int) stFriendReqListRsp.dwFriendReqNum);
                for (int i = 0; i < num; i++)
                {
                    Singleton<CFriendContoller>.GetInstance().model.Add(CFriendModel.FriendType.RequestFriend, stFriendReqListRsp.astFrindReqList[i], false);
                }
                if ((this.view != null) && this.view.IsActive())
                {
                    this.view.Refresh();
                }
            }
        }

        private void On_FriendSys_Friend_Request_NTF(CSPkg msg)
        {
            SCPKG_CMD_NTF_FRIEND_REQUEST stNtfFriendRequest = msg.stPkgData.stNtfFriendRequest;
            this.Add_And_Refresh(CFriendModel.FriendType.RequestFriend, stNtfFriendRequest.stUserInfo);
        }

        private void On_FriendSys_Friend_RequestBeFriend(CSPkg msg)
        {
            SCPKG_CMD_ADD_FRIEND stFriendAddRsp = msg.stPkgData.stFriendAddRsp;
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            Singleton<EventRouter>.instance.BroadCastEvent("Friend_SNSFriendList_Refresh");
            UT.ShowFriendNetResult(stFriendAddRsp.dwResult, UT.FriendResultType.RequestBeFriend);
        }

        private void On_FriendSys_Friend_Search(CSPkg msg)
        {
            SCPKG_CMD_SEARCH_PLAYER stFriendSearchPlayerRsp = msg.stPkgData.stFriendSearchPlayerRsp;
            if (stFriendSearchPlayerRsp.dwResult == 0)
            {
                if (this.view != null)
                {
                    this.view.Show_Search_Result(stFriendSearchPlayerRsp.stUserInfo);
                }
            }
            else
            {
                Singleton<CUIManager>.GetInstance().OpenTips(UT.ErrorCode_String(stFriendSearchPlayerRsp.dwResult), false, 1f, null, new object[0]);
            }
            this.view.addFriendView.Refresh_Friend_Recommand_List_Pos();
        }

        private void On_GuildSys_Guild_Invite_Success()
        {
            if ((this.view != null) && this.view.IsActive())
            {
                this.view.Refresh();
            }
        }

        private void On_GuildSys_Guild_Recommend_Success()
        {
            if ((this.view != null) && this.view.IsActive())
            {
                this.view.Refresh();
            }
        }

        private void On_OpenAddFriendForm(CUIEvent uiEvent)
        {
            this.view.OpenSearchForm();
        }

        private void On_OpenForm(CUIEvent uiEvent)
        {
            this.view.OpenForm(uiEvent);
        }

        private void On_Refuse_RequestFriend(CUIEvent uievent)
        {
            FriendShower component = uievent.m_srcWidget.transform.parent.parent.parent.GetComponent<FriendShower>();
            if (component != null)
            {
                COMDT_FRIEND_INFO comdt_friend_info = this.model.GetInfo(CFriendModel.FriendType.RequestFriend, component.ullUid, component.dwLogicWorldID);
                if (comdt_friend_info != null)
                {
                    FriendSysNetCore.Send_DENY_BeFriend(comdt_friend_info.stUin);
                }
            }
        }

        private void On_SNSFriend_SendCoin(CUIEvent uievent)
        {
            ulong ullUid = uievent.m_eventParams.commonUInt64Param1;
            uint dwLogicWorldID = (uint) uievent.m_eventParams.commonUInt64Param2;
            COMDT_FRIEND_INFO comdt_friend_info = this.model.GetInfo(CFriendModel.FriendType.SNS, ullUid, dwLogicWorldID);
            if (comdt_friend_info != null)
            {
                FriendSysNetCore.Send_FriendCoin(comdt_friend_info.stUin, COM_FRIEND_TYPE.COM_FRIEND_TYPE_SNS);
            }
        }

        private void On_TabChange(CUIEvent uievent)
        {
            if ((this.view != null) && this.view.IsActive())
            {
                int selectedIndex = uievent.m_srcWidget.GetComponent<CUIListScript>().GetSelectedIndex();
                this.view.On_Tab_Change(selectedIndex);
            }
        }

        private void OnNewDayNtf()
        {
            this.model.SnsReCallData.Clear();
            this.model.HeartData.Clear();
            if ((this.view != null) && this.view.IsActive())
            {
                this.view.Refresh();
            }
        }

        public void OnReCallFriendNtf(CSPkg msg)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            uint num = (masterRoleInfo == null) ? 0 : ((uint) masterRoleInfo.getCurrentTimeSinceLogin());
            uint num2 = 0x15180 * GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x9e).dwConfValue;
            for (int i = 0; i < msg.stPkgData.stNtfRecallFirend.wRecallNum; i++)
            {
                COMDT_RECALL_FRIEND comdt_recall_friend = msg.stPkgData.stNtfRecallFirend.astRecallAcntList[i];
                if (comdt_recall_friend != null)
                {
                    this.Handle_Invite_Data(comdt_recall_friend.stAcntUniq);
                }
            }
        }

        private void QQBox_OnClick(CUIEvent uievent)
        {
            if (ApolloConfig.platform == ApolloPlatform.QQ)
            {
                Debug.Log("QQBox_OnClick");
                MonoSingleton<IDIPSys>.GetInstance().RequestQQBox();
            }
        }

        private void Remove_And_Refresh(CFriendModel.FriendType type, COMDT_ACNT_UNIQ uniq)
        {
            this.model.Remove(type, uniq);
            if (this.view.IsActive())
            {
                this.view.Refresh();
            }
        }

        public void ShareTo_SNSFriend_ReCall(string openId)
        {
            if (MonoSingleton<ShareSys>.instance.IsInstallPlatform())
            {
                string text = Singleton<CTextManager>.instance.GetText("Common_Sns_Tips_11");
                string desc = Singleton<CTextManager>.instance.GetText("Common_Sns_Tips_12");
                Singleton<ApolloHelper>.instance.ShareInviteFriend(openId, text, desc, ShareSys.SNS_SHARE_RECALL_FRIEND);
            }
        }

        private void Update_Send_Coin_Data(COMDT_ACNT_UNIQ uin, ulong donateAPSec, COM_FRIEND_TYPE friendType)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                DateTime time = Utility.ToUtcTime2Local((long) donateAPSec);
                DateTime time2 = Utility.ToUtcTime2Local((long) masterRoleInfo.getCurrentTimeSinceLogin());
                if (((time2.Year == time.Year) && (time2.Month == time.Month)) && (time2.Day == time.Day))
                {
                    this.model.HeartData.Add(uin, friendType);
                }
                if (((time2.Year >= time.Year) && ((time2.Year != time.Year) || (time2.Month >= time.Month))) && (((time2.Year != time.Year) || (time2.Month != time.Month)) || (time2.Day < time.Day)))
                {
                }
            }
        }
    }
}

