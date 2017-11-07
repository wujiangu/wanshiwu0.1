namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    [MessageHandlerClass]
    internal class CInviteSystem : Singleton<CInviteSystem>
    {
        private ListView<COMDT_FRIEND_INFO> allFriendList_internal;
        private ListView<GuildMemInfo> m_allGuildMemberList;
        private COM_INVITE_JOIN_TYPE m_inviteType;
        private bool m_isFirstlySelectGuildMemberTab = true;
        private bool m_isNeedRefreshGuildMemberPanel = true;
        private ListView<InviteState> m_stateList = new ListView<InviteState>();
        public static string PATH_INVITE_FORM = "UGUI/Form/System/PvP/Form_InviteFriend.prefab";
        private const int REFRESH_GUILD_MEMBER_GAME_STATE_SECONDS = 10;
        private const int REFRESH_GUILD_MEMBER_GAME_STATE_WAIT_MILLISECONDS = 0xbb8;

        private void AddInviteStateList(ulong uid, uint time, enInviteState state)
        {
            for (int i = 0; i < this.m_stateList.Count; i++)
            {
                if (uid == this.m_stateList[i].uid)
                {
                    this.m_stateList[i].uid = uid;
                    this.m_stateList[i].time = time;
                    this.m_stateList[i].state = state;
                    return;
                }
            }
            InviteState item = new InviteState {
                uid = uid,
                time = time,
                state = state
            };
            this.m_stateList.Add(item);
        }

        private void ChangeInviteStateList(ulong uid, enInviteState state)
        {
            for (int i = 0; i < this.m_stateList.Count; i++)
            {
                if (uid == this.m_stateList[i].uid)
                {
                    this.m_stateList[i].state = state;
                    return;
                }
            }
        }

        public void CloseInviteForm()
        {
            Singleton<CUIManager>.GetInstance().CloseForm(PATH_INVITE_FORM);
            if (this.m_inviteType == COM_INVITE_JOIN_TYPE.COM_INVITE_JOIN_TEAM)
            {
                Singleton<CChatController>.instance.model.channelMgr.Clear(EChatChannel.Team, 0L, 0);
                Singleton<CChatController>.instance.model.channelMgr.SetChatTab(CChatChannelMgr.EChatTab.Normal);
                Singleton<CChatController>.instance.ShowPanel(true, false);
                Singleton<CChatController>.instance.view.UpView(false);
                Singleton<CChatController>.instance.model.sysData.ClearEntryText();
            }
        }

        private static int FriendComparison(COMDT_FRIEND_INFO a, COMDT_FRIEND_INFO b)
        {
            if ((a.bIsOnline == 1) && (b.bIsOnline == 0))
            {
                return -1;
            }
            if ((a.bIsOnline == 0) && (b.bIsOnline == 1))
            {
                return 1;
            }
            return 0;
        }

        public ListView<COMDT_FRIEND_INFO> GetAllFriendList()
        {
            return this.m_allFriendList;
        }

        public ListView<GuildMemInfo> GetAllGuildMemberList()
        {
            return this.m_allGuildMemberList;
        }

        public string GetInviteFriendName(ulong friendUid, uint friendLogicWorldId)
        {
            if (this.m_allFriendList != null)
            {
                for (int i = 0; i < this.m_allFriendList.Count; i++)
                {
                    if ((friendUid == this.m_allFriendList[i].stUin.ullUid) && (friendLogicWorldId == this.m_allFriendList[i].stUin.dwLogicWorldId))
                    {
                        return StringHelper.UTF8BytesToString(ref this.m_allFriendList[i].szUserName);
                    }
                }
            }
            return string.Empty;
        }

        public string GetInviteGuildMemberName(ulong guildMemberUid)
        {
            if (this.m_allGuildMemberList != null)
            {
                for (int i = 0; i < this.m_allGuildMemberList.Count; i++)
                {
                    if (guildMemberUid == this.m_allGuildMemberList[i].stBriefInfo.uulUid)
                    {
                        return this.m_allGuildMemberList[i].stBriefInfo.sName;
                    }
                }
            }
            return string.Empty;
        }

        private static string GetInviteRoomFailReason(string fName, int errCode)
        {
            string str = string.Empty;
            switch (errCode)
            {
                case 11:
                    return string.Format(Singleton<CTextManager>.GetInstance().GetText("PVP_Can_Not_Find_Friend"), fName);

                case 12:
                {
                    COMDT_FRIEND_INFO comdt_friend_info = Singleton<CFriendContoller>.instance.model.getFriendByName(fName, CFriendModel.FriendType.GameFriend);
                    if (comdt_friend_info == null)
                    {
                        comdt_friend_info = Singleton<CFriendContoller>.instance.model.getFriendByName(fName, CFriendModel.FriendType.SNS);
                    }
                    if (comdt_friend_info != null)
                    {
                        comdt_friend_info.bIsOnline = 0;
                        Singleton<EventRouter>.GetInstance().BroadCastEvent("Chat_Friend_Online_Change");
                    }
                    return string.Format(Singleton<CTextManager>.GetInstance().GetText("PVP_Friend_Off_Line"), fName);
                }
                case 13:
                    return string.Format(Singleton<CTextManager>.GetInstance().GetText("PVP_Gaming_Tip"), fName);

                case 14:
                    return string.Format(Singleton<CTextManager>.GetInstance().GetText("PVP_Invite_Refuse"), fName);

                case 15:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 20:
                    return str;

                case 0x15:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_Version_Different");

                case 0x16:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_Version_Higher");

                case 0x17:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_Version_Lower");

                case 0x18:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_ENTERTAINMENT_Lock");
            }
            return str;
        }

        private enInviteState GetInviteState(ulong uid)
        {
            for (int i = 0; i < this.m_stateList.Count; i++)
            {
                if (uid == this.m_stateList[i].uid)
                {
                    return this.m_stateList[i].state;
                }
            }
            return enInviteState.None;
        }

        public string GetInviteStateStr(ulong uid)
        {
            switch (this.GetInviteState(uid))
            {
                case enInviteState.None:
                    return string.Format("<color=#00ff00>{0}</color>", Singleton<CTextManager>.instance.GetText("Common_Online"));

                case enInviteState.Invited:
                    return string.Format("<color=#ffffff>{0}</color>", Singleton<CTextManager>.instance.GetText("Guild_Has_Invited"));

                case enInviteState.BeRejcet:
                    return string.Format("<color=#ff0000>{0}</color>", Singleton<CTextManager>.instance.GetText("Invite_Friend_Tips_2"));
            }
            return string.Empty;
        }

        private static string GetInviteTeamFailReason(string fName, int errCode, uint timePunished = 0)
        {
            string str = string.Empty;
            switch (errCode)
            {
                case 3:
                    return string.Format(Singleton<CTextManager>.GetInstance().GetText("PVP_Can_Not_Find_Friend"), fName);

                case 4:
                {
                    COMDT_FRIEND_INFO comdt_friend_info = Singleton<CFriendContoller>.instance.model.getFriendByName(fName, CFriendModel.FriendType.GameFriend);
                    if (comdt_friend_info == null)
                    {
                        comdt_friend_info = Singleton<CFriendContoller>.instance.model.getFriendByName(fName, CFriendModel.FriendType.SNS);
                    }
                    if (comdt_friend_info != null)
                    {
                        comdt_friend_info.bIsOnline = 0;
                        Singleton<EventRouter>.GetInstance().BroadCastEvent("Chat_Friend_Online_Change");
                    }
                    return string.Format(Singleton<CTextManager>.GetInstance().GetText("PVP_Friend_Off_Line"), fName);
                }
                case 5:
                    return string.Format(Singleton<CTextManager>.GetInstance().GetText("PVP_Gaming_Tip"), fName);

                case 6:
                    return string.Format(Singleton<CTextManager>.GetInstance().GetText("PVP_Invite_Refuse"), fName);

                case 7:
                case 8:
                case 10:
                case 11:
                case 13:
                    return str;

                case 9:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_Team_Member_Full");

                case 12:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_Invite_Result_4");

                case 14:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_Version_Higher");

                case 15:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_Version_Lower");

                case 0x10:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_ENTERTAINMENT_Lock");

                case 0x11:
                {
                    string str2 = string.Format("{0}分{1}秒", timePunished / 60, timePunished % 60);
                    return string.Format(Singleton<CTextManager>.GetInstance().GetText("PVP_Invite_Punished"), fName, str2);
                }
                case 0x12:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_Invite_Result_1");

                case 0x13:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_Invite_Result_2");

                case 20:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_Invite_Result_3");

                case 0x15:
                    return Singleton<CTextManager>.GetInstance().GetText("Err_Invite_Result_5");
            }
            return str;
        }

        private static string GetInviteType(byte Type, byte SubType = 0)
        {
            string text = string.Empty;
            if (Type == 1)
            {
                return Singleton<CTextManager>.GetInstance().GetText("PVP_Enter_Room");
            }
            if (Type == 2)
            {
                if ((SubType == 1) || (SubType == 4))
                {
                    return Singleton<CTextManager>.GetInstance().GetText("PVP_Team_Normal_Matching");
                }
                if (SubType == 3)
                {
                    return Singleton<CTextManager>.GetInstance().GetText("PVP_Team_Ranking");
                }
                if (SubType == 5)
                {
                    text = Singleton<CTextManager>.GetInstance().GetText("PVP_Team_UnionBattle");
                }
            }
            return text;
        }

        public CUIListElementScript GetListItem(string username)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(PATH_INVITE_FORM);
            if (form != null)
            {
                CUIListScript component = form.transform.Find("Panel_Friend/List").gameObject.GetComponent<CUIListScript>();
                for (int i = 0; i < component.m_elementAmount; i++)
                {
                    CUIListElementScript elemenet = component.GetElemenet(i);
                    if (elemenet.gameObject.transform.Find("PlayerName").GetComponent<Text>().text == username)
                    {
                        return elemenet;
                    }
                }
            }
            return null;
        }

        private uint GetNextInviteSec(ulong uid, uint time)
        {
            uint dwConfValue = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x9c).dwConfValue;
            for (int i = 0; i < this.m_stateList.Count; i++)
            {
                if (uid == this.m_stateList[i].uid)
                {
                    return ((this.m_stateList[i].time + dwConfValue) - time);
                }
            }
            return 0;
        }

        private static int GuildMemberComparison(GuildMemInfo a, GuildMemInfo b)
        {
            if (CGuildHelper.IsMemberOnline(a) && !CGuildHelper.IsMemberOnline(b))
            {
                return -1;
            }
            if (!CGuildHelper.IsMemberOnline(a) && CGuildHelper.IsMemberOnline(b))
            {
                return 1;
            }
            return ((a.stBriefInfo.uulUid >= b.stBriefInfo.uulUid) ? 1 : -1);
        }

        private bool InInviteCdList(ulong uid, uint time)
        {
            for (int i = 0; i < this.m_stateList.Count; i++)
            {
                if (uid == this.m_stateList[i].uid)
                {
                    return ((time - this.m_stateList[i].time) < GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x9c).dwConfValue);
                }
            }
            return false;
        }

        public override void Init()
        {
            base.Init();
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Invite_SendInviteFriend, new CUIEventManager.OnUIEventHandler(this.OnInvite_SendInviteFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Invite_SendInviteGuildMember, new CUIEventManager.OnUIEventHandler(this.OnInvite_SendInviteGuildMember));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Invite_AcceptInvite, new CUIEventManager.OnUIEventHandler(this.OnInvite_Accept));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Invite_RejectInvite, new CUIEventManager.OnUIEventHandler(this.OnInvite_Reject));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Invite_FriendListElementEnable, new CUIEventManager.OnUIEventHandler(this.OnInvite_FriendListElementEnable));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Invite_GuildMemberListElementEnable, new CUIEventManager.OnUIEventHandler(this.OnInvite_GuildMemberListElementEnable));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Invite_TabChange, new CUIEventManager.OnUIEventHandler(this.OnInvite_TabChange));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Invite_RefreshGameStateTimeout, new CUIEventManager.OnUIEventHandler(this.OnInvite_RefreshGameStateTimeout));
            Singleton<EventRouter>.GetInstance().AddEventHandler<byte, string>(EventID.INVITE_ERRCODE_NTF, new Action<byte, string>(this, (IntPtr) this.OnInviteErrCodeNtf));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_ADD_NTF", new Action<CSPkg>(this.OnFriendChange));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CSPkg>("Friend_Delete_NTF", new Action<CSPkg>(this.OnFriendChange));
            Singleton<EventRouter>.GetInstance().AddEventHandler("Chat_Friend_Online_Change", new Action(this, (IntPtr) this.OnFriendOnlineChg));
            Singleton<EventRouter>.GetInstance().AddEventHandler(EventID.Friend_Game_State_Change, new Action(this, (IntPtr) this.OnFriendOnlineChg));
        }

        private void InitAllGuildMemberList()
        {
            ListView<GuildMemInfo> guildMemberInfos = CGuildHelper.GetGuildMemberInfos();
            this.m_allGuildMemberList = new ListView<GuildMemInfo>();
            this.m_allGuildMemberList.AddRange(guildMemberInfos);
            for (int i = this.m_allGuildMemberList.Count - 1; i >= 0; i--)
            {
                if (CGuildHelper.IsSelf(this.m_allGuildMemberList[i].stBriefInfo.uulUid))
                {
                    this.m_allGuildMemberList.RemoveAt(i);
                    break;
                }
            }
        }

        private bool IsInList(ListView<COMDT_FRIEND_INFO> list, COMDT_FRIEND_INFO info)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if ((list[i].stUin.ullUid == info.stUin.ullUid) && (list[i].stUin.dwLogicWorldId == info.stUin.dwLogicWorldId))
                {
                    return true;
                }
            }
            return false;
        }

        private void OnFriendChange(CSPkg msg)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(PATH_INVITE_FORM);
            if (form != null)
            {
                this.SortAllFriendList();
                CInviteView.SetInviteFriendData(form, this.m_inviteType);
            }
        }

        private void OnFriendOnlineChg()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(PATH_INVITE_FORM);
            if (form != null)
            {
                this.SortAllFriendList();
                CInviteView.SetInviteFriendData(form, this.m_inviteType);
            }
        }

        [MessageHandler(0x7e4)]
        public static void OnGetInvited(CSPkg msg)
        {
            if (!Singleton<SettlementSystem>.GetInstance().IsExistSettleForm())
            {
                string str = StringHelper.UTF8BytesToString(ref msg.stPkgData.stInviteJoinGameReq.szInviterName);
                COMDT_TEAM_BASE stTeamDetail = msg.stPkgData.stInviteJoinGameReq.stInviteDetail.stTeamDetail;
                string inviteType = GetInviteType(msg.stPkgData.stInviteJoinGameReq.bInviteType, (stTeamDetail != null) ? stTeamDetail.bMapType : ((byte) 0));
                stUIEventParams par = new stUIEventParams {
                    tag = msg.stPkgData.stInviteJoinGameReq.bIndex,
                    tagStr = str
                };
                int result = 15;
                int.TryParse(Singleton<CTextManager>.instance.GetText("MessageBox_Close_Time"), out result);
                Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancelAndAutoClose(string.Format(Singleton<CTextManager>.GetInstance().GetText("PVP_Invite_Tip"), str, inviteType), enUIEventID.Invite_AcceptInvite, enUIEventID.Invite_RejectInvite, par, false, result, enUIEventID.Invite_AddToMsgCenter);
            }
        }

        private void OnInvite_Accept(CUIEvent uiEvent)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x7e5);
            msg.stPkgData.stInviteJoinGameRsp.bIndex = (byte) uiEvent.m_eventParams.tag;
            msg.stPkgData.stInviteJoinGameRsp.bResult = 0;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
            Singleton<CUIManager>.GetInstance().CloseMessageBox();
        }

        private void OnInvite_FriendListElementEnable(CUIEvent uiEvent)
        {
            int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
            GameObject srcWidget = uiEvent.m_srcWidget;
            if ((srcWidgetIndexInBelongedList >= 0) && (srcWidgetIndexInBelongedList < this.m_allFriendList.Count))
            {
                CInviteView.UpdateFriendListElement(srcWidget, this.m_allFriendList[srcWidgetIndexInBelongedList]);
            }
        }

        private void OnInvite_GuildMemberListElementEnable(CUIEvent uiEvent)
        {
            int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
            GameObject srcWidget = uiEvent.m_srcWidget;
            if ((srcWidgetIndexInBelongedList >= 0) && (srcWidgetIndexInBelongedList < this.m_allGuildMemberList.Count))
            {
                CInviteView.UpdateGuildMemberListElement(srcWidget, this.m_allGuildMemberList[srcWidgetIndexInBelongedList]);
            }
        }

        private void OnInvite_RefreshGameStateTimeout(CUIEvent uiEvent)
        {
            this.SendGetGuildMemberGameStateReq();
        }

        private void OnInvite_Reject(CUIEvent uiEvent)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x7e5);
            msg.stPkgData.stInviteJoinGameRsp.bIndex = (byte) uiEvent.m_eventParams.tag;
            msg.stPkgData.stInviteJoinGameRsp.bResult = 14;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
        }

        private void OnInvite_SendInviteFriend(CUIEvent uiEvent)
        {
            COM_INVITE_JOIN_TYPE tag = (COM_INVITE_JOIN_TYPE) uiEvent.m_eventParams.tag;
            ulong uid = uiEvent.m_eventParams.commonUInt64Param1;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                uint time = (uint) masterRoleInfo.getCurrentTimeSinceLogin();
                if (this.InInviteCdList(uid, time))
                {
                    object[] replaceArr = new object[] { this.GetNextInviteSec(uid, time) };
                    Singleton<CUIManager>.instance.OpenTips("Invite_Friend_Tips_1", true, 1f, null, replaceArr);
                }
                else
                {
                    bool flag = Singleton<CFriendContoller>.instance.model.IsGameFriend(uid);
                    CFriendModel.FriendType friendType = !flag ? CFriendModel.FriendType.SNS : CFriendModel.FriendType.GameFriend;
                    byte num3 = !flag ? ((byte) 2) : ((byte) 1);
                    COMDT_FRIEND_INFO comdt_friend_info = Singleton<CFriendContoller>.instance.model.getFriendByUid(uid, friendType);
                    if (comdt_friend_info != null)
                    {
                        switch (tag)
                        {
                            case COM_INVITE_JOIN_TYPE.COM_INVITE_JOIN_ROOM:
                            {
                                CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x7e1);
                                msg.stPkgData.stInviteFriendJoinRoomReq.stFriendInfo.ullUid = comdt_friend_info.stUin.ullUid;
                                msg.stPkgData.stInviteFriendJoinRoomReq.stFriendInfo.dwLogicWorldId = comdt_friend_info.stUin.dwLogicWorldId;
                                msg.stPkgData.stInviteFriendJoinRoomReq.bFriendType = num3;
                                Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
                                break;
                            }
                            case COM_INVITE_JOIN_TYPE.COM_INVITE_JOIN_TEAM:
                            {
                                CSPkg pkg2 = NetworkModule.CreateDefaultCSPKG(0x7e8);
                                pkg2.stPkgData.stInviteFriendJoinTeamReq.stFriendInfo.ullUid = comdt_friend_info.stUin.ullUid;
                                pkg2.stPkgData.stInviteFriendJoinTeamReq.stFriendInfo.dwLogicWorldId = comdt_friend_info.stUin.dwLogicWorldId;
                                pkg2.stPkgData.stInviteFriendJoinTeamReq.bFriendType = num3;
                                Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref pkg2, false);
                                break;
                            }
                        }
                        if (uiEvent.m_srcWidget.transform.parent != null)
                        {
                            Transform transform = uiEvent.m_srcWidget.transform.parent.Find("Online");
                            if (transform != null)
                            {
                                transform.GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("Guild_Has_Invited");
                            }
                        }
                        this.AddInviteStateList(uid, time, enInviteState.Invited);
                    }
                }
            }
        }

        private void OnInvite_SendInviteGuildMember(CUIEvent uiEvent)
        {
            int tag = uiEvent.m_eventParams.tag;
            ulong num2 = uiEvent.m_eventParams.commonUInt64Param1;
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x7f2);
            msg.stPkgData.stInviteGuildMemberJoinReq.iInviteType = tag;
            msg.stPkgData.stInviteGuildMemberJoinReq.ullInviteeUid = num2;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
            uiEvent.m_srcWidget.CustomSetActive(false);
            if (uiEvent.m_srcWidget.transform.parent != null)
            {
                Transform transform = uiEvent.m_srcWidget.transform.parent.Find("Online");
                if (transform != null)
                {
                    transform.GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("Guild_Has_Invited");
                }
            }
        }

        private void OnInvite_TabChange(CUIEvent uiEvent)
        {
            CUIFormScript srcFormScript = uiEvent.m_srcFormScript;
            CUIListScript component = uiEvent.m_srcWidget.GetComponent<CUIListScript>();
            GameObject widget = srcFormScript.GetWidget(0);
            GameObject obj3 = srcFormScript.GetWidget(1);
            if (component.GetSelectedIndex() == 0)
            {
                widget.CustomSetActive(true);
                obj3.CustomSetActive(false);
            }
            else
            {
                obj3.CustomSetActive(true);
                widget.CustomSetActive(false);
                if (this.m_isFirstlySelectGuildMemberTab)
                {
                    this.InitAllGuildMemberList();
                    this.SendGetGuildMemberGameStateReq();
                    CUITimerScript script3 = srcFormScript.GetWidget(8).GetComponent<CUITimerScript>();
                    script3.SetTotalTime(1000f);
                    script3.SetOnChangedIntervalTime(10f);
                    script3.StartTimer();
                    this.m_isFirstlySelectGuildMemberTab = false;
                }
            }
        }

        private void OnInviteErrCodeNtf(byte errorCode, string userName)
        {
            if (errorCode == 14)
            {
                CUIListElementScript listItem = this.GetListItem(userName);
                if (listItem != null)
                {
                    listItem.transform.FindChild("Online").GetComponent<Text>().text = string.Format("<color=#ff0000>{0}</color>", Singleton<CTextManager>.instance.GetText("Invite_Friend_Tips_2"));
                }
                COMDT_FRIEND_INFO comdt_friend_info = Singleton<CFriendContoller>.instance.model.getFriendByName(userName, CFriendModel.FriendType.GameFriend);
                if (comdt_friend_info == null)
                {
                    comdt_friend_info = Singleton<CFriendContoller>.instance.model.getFriendByName(userName, CFriendModel.FriendType.SNS);
                }
                if (comdt_friend_info != null)
                {
                    this.ChangeInviteStateList(comdt_friend_info.stUin.ullUid, enInviteState.BeRejcet);
                }
            }
        }

        [MessageHandler(0x7e2)]
        public static void OnInviteFriendRoom(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            byte bErrcode = msg.stPkgData.stInviteFriendJoinRoomRsp.bErrcode;
            string str = StringHelper.UTF8BytesToString(ref msg.stPkgData.stInviteFriendJoinRoomRsp.szFriendName);
            if (bErrcode == 20)
            {
                DateTime banTime = MonoSingleton<IDIPSys>.GetInstance().GetBanTime(COM_ACNT_BANTIME_TYPE.COM_ACNT_BANTIME_BANPLAYPVP);
                object[] args = new object[] { banTime.Year, banTime.Month, banTime.Day, banTime.Hour, banTime.Minute };
                string strContent = string.Format("您被禁止竞技！截止时间为{0}年{1}月{2}日{3}时{4}分", args);
                Singleton<CUIManager>.GetInstance().OpenMessageBox(strContent, false);
            }
            else
            {
                Singleton<CUIManager>.GetInstance().OpenTips(GetInviteRoomFailReason(StringHelper.UTF8BytesToString(ref msg.stPkgData.stInviteFriendJoinRoomRsp.szFriendName), msg.stPkgData.stInviteFriendJoinRoomRsp.bErrcode), false, 1f, null, new object[0]);
            }
            Singleton<EventRouter>.instance.BroadCastEvent<byte, string>(EventID.INVITE_ERRCODE_NTF, bErrcode, str);
        }

        [MessageHandler(0x7e9)]
        public static void OnInviteFriendTeam(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            byte bErrcode = msg.stPkgData.stInviteFriendJoinTeamRsp.bErrcode;
            string fName = StringHelper.UTF8BytesToString(ref msg.stPkgData.stInviteFriendJoinTeamRsp.szFriendName);
            uint timePunished = 0;
            if (msg.stPkgData.stInviteFriendJoinTeamRsp.bErrcode == 0x11)
            {
                timePunished = msg.stPkgData.stInviteFriendJoinTeamRsp.dwParam;
            }
            Singleton<CUIManager>.GetInstance().OpenTips(GetInviteTeamFailReason(fName, bErrcode, timePunished), false, 1f, null, new object[0]);
            Singleton<EventRouter>.instance.BroadCastEvent<byte, string>(EventID.INVITE_ERRCODE_NTF, bErrcode, fName);
        }

        private void OnRefreshGuildMemberGameStateTimeout(int timerSequence)
        {
            if (this.m_isNeedRefreshGuildMemberPanel)
            {
                this.RefreshGuildMemberPanel();
                this.m_isNeedRefreshGuildMemberPanel = false;
            }
        }

        public void OpenInviteForm(COM_INVITE_JOIN_TYPE inviteType)
        {
            this.m_stateList.Clear();
            this.m_isFirstlySelectGuildMemberTab = true;
            this.SortAllFriendList();
            CUIFormScript form = Singleton<CUIManager>.GetInstance().OpenForm(PATH_INVITE_FORM, false, true);
            if (form != null)
            {
                this.m_inviteType = inviteType;
                CInviteView.InitListTab(form);
                CInviteView.SetInviteFriendData(form, inviteType);
            }
            if (this.m_inviteType == COM_INVITE_JOIN_TYPE.COM_INVITE_JOIN_TEAM)
            {
                Singleton<CChatController>.instance.model.channelMgr.Clear(EChatChannel.Team, 0L, 0);
                Singleton<CChatController>.instance.model.channelMgr.SetChatTab(CChatChannelMgr.EChatTab.Team);
                Singleton<CChatController>.instance.ShowPanel(true, false);
                Singleton<CChatController>.instance.view.UpView(true);
                Singleton<CChatController>.instance.model.sysData.ClearEntryText();
            }
        }

        [MessageHandler(0x7f4)]
        public static void ReceiveGetGuildMemberGameStateRsp(CSPkg msg)
        {
            SCPKG_GET_GUILD_MEMBER_GAME_STATE_RSP stGetGuildMemberGameStateRsp = msg.stPkgData.stGetGuildMemberGameStateRsp;
            ListView<GuildMemInfo> allGuildMemberList = Singleton<CInviteSystem>.GetInstance().m_allGuildMemberList;
            for (int i = 0; i < stGetGuildMemberGameStateRsp.iMemberCnt; i++)
            {
                for (int j = 0; j < allGuildMemberList.Count; j++)
                {
                    if (CGuildHelper.IsMemberOnline(allGuildMemberList[j]) && (stGetGuildMemberGameStateRsp.astMemberInfo[i].ullUid == allGuildMemberList[j].stBriefInfo.uulUid))
                    {
                        allGuildMemberList[j].GameState = (COM_ACNT_GAME_STATE) stGetGuildMemberGameStateRsp.astMemberInfo[i].bGameState;
                    }
                }
            }
            Singleton<CInviteSystem>.GetInstance().RefreshGuildMemberPanel();
            Singleton<CInviteSystem>.GetInstance().m_isNeedRefreshGuildMemberPanel = false;
        }

        private void RefreshGuildMemberPanel()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().OpenForm(PATH_INVITE_FORM, false, true);
            if (form != null)
            {
                this.SortAllGuildMemberList();
                CInviteView.SetInviteGuildMemberData(form, this.m_inviteType);
            }
        }

        private void SendGetGuildMemberGameStateReq()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x7f3);
            CSPKG_GET_GUILD_MEMBER_GAME_STATE_REQ stGetGuildMemberGameStateReq = msg.stPkgData.stGetGuildMemberGameStateReq;
            int index = 0;
            for (int i = 0; i < this.m_allGuildMemberList.Count; i++)
            {
                if (CGuildHelper.IsMemberOnline(this.m_allGuildMemberList[i]))
                {
                    stGetGuildMemberGameStateReq.MemberUid[index] = this.m_allGuildMemberList[i].stBriefInfo.uulUid;
                    index++;
                }
            }
            stGetGuildMemberGameStateReq.iMemberCnt = index;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
            this.m_isNeedRefreshGuildMemberPanel = true;
            Singleton<CTimerManager>.GetInstance().AddTimer(0xbb8, 1, new CTimer.OnTimeUpHandler(this.OnRefreshGuildMemberGameStateTimeout));
        }

        public void SortAllFriendList()
        {
            ListView<COMDT_FRIEND_INFO> list = Singleton<CFriendContoller>.GetInstance().model.GetList(CFriendModel.FriendType.GameFriend);
            ListView<COMDT_FRIEND_INFO> collection = Singleton<CFriendContoller>.GetInstance().model.GetList(CFriendModel.FriendType.SNS);
            this.allFriendList_internal = new ListView<COMDT_FRIEND_INFO>();
            this.allFriendList_internal.AddRange(list);
            this.allFriendList_internal.AddRange(collection);
            for (int i = 0; i < this.allFriendList_internal.Count; i++)
            {
                for (int j = i + 1; j < this.allFriendList_internal.Count; j++)
                {
                    if (this.allFriendList_internal[i].stUin.ullUid == this.allFriendList_internal[j].stUin.ullUid)
                    {
                        this.allFriendList_internal.RemoveAt(j);
                        j--;
                    }
                }
            }
            this.allFriendList_internal.Sort(new Comparison<COMDT_FRIEND_INFO>(CInviteSystem.FriendComparison));
        }

        private void SortAllGuildMemberList()
        {
            this.m_allGuildMemberList.Sort(new Comparison<GuildMemInfo>(CInviteSystem.GuildMemberComparison));
        }

        public override void UnInit()
        {
            base.UnInit();
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Invite_SendInviteFriend, new CUIEventManager.OnUIEventHandler(this.OnInvite_SendInviteFriend));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Invite_SendInviteGuildMember, new CUIEventManager.OnUIEventHandler(this.OnInvite_SendInviteGuildMember));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Invite_AcceptInvite, new CUIEventManager.OnUIEventHandler(this.OnInvite_Accept));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Invite_RejectInvite, new CUIEventManager.OnUIEventHandler(this.OnInvite_Reject));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Invite_FriendListElementEnable, new CUIEventManager.OnUIEventHandler(this.OnInvite_FriendListElementEnable));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Invite_GuildMemberListElementEnable, new CUIEventManager.OnUIEventHandler(this.OnInvite_GuildMemberListElementEnable));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Invite_TabChange, new CUIEventManager.OnUIEventHandler(this.OnInvite_TabChange));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Invite_RefreshGameStateTimeout, new CUIEventManager.OnUIEventHandler(this.OnInvite_RefreshGameStateTimeout));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<byte, string>(EventID.INVITE_ERRCODE_NTF, new Action<byte, string>(this, (IntPtr) this.OnInviteErrCodeNtf));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<CSPkg>("Friend_ADD_NTF", new Action<CSPkg>(this.OnFriendChange));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<CSPkg>("Friend_Delete_NTF", new Action<CSPkg>(this.OnFriendChange));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler("Chat_Friend_Online_Change", new Action(this, (IntPtr) this.OnFriendOnlineChg));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler(EventID.Friend_Game_State_Change, new Action(this, (IntPtr) this.OnFriendOnlineChg));
        }

        public COM_INVITE_JOIN_TYPE InviteType
        {
            get
            {
                return this.m_inviteType;
            }
        }

        private ListView<COMDT_FRIEND_INFO> m_allFriendList
        {
            get
            {
                if (this.allFriendList_internal == null)
                {
                    this.SortAllFriendList();
                }
                return this.allFriendList_internal;
            }
        }

        private enum enInviteState
        {
            None,
            Invited,
            BeRejcet
        }

        private class InviteState
        {
            public CInviteSystem.enInviteState state;
            public uint time;
            public ulong uid;
        }
    }
}

