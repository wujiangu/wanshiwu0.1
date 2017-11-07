namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    internal class CInviteView
    {
        public static string[] TabName = new string[] { Singleton<CTextManager>.GetInstance().GetText("Common_My_Friend"), Singleton<CTextManager>.GetInstance().GetText("Guild_Guild_Member") };

        public static void InitListTab(CUIFormScript form)
        {
            CUIListScript component = form.GetWidget(7).GetComponent<CUIListScript>();
            int amount = !Singleton<CGuildSystem>.GetInstance().IsInNormalGuild() ? 1 : 2;
            component.SetElementAmount(amount);
            for (int i = 0; i < component.GetElementAmount(); i++)
            {
                component.GetElemenet(i).transform.Find("txtName").GetComponent<Text>().text = TabName[i];
            }
            component.SelectElement(0, true);
        }

        public static void RefreshInviteGuildMemberList(CUIFormScript form, int allGuildMemberLen)
        {
            form.GetWidget(5).GetComponent<CUIListScript>().SetElementAmount(allGuildMemberLen);
        }

        private static void SetFriendState(GameObject element, ref COMDT_FRIEND_INFO friend)
        {
            GameObject gameObject = element.transform.FindChild("HeadBg").gameObject;
            Text component = element.transform.FindChild("Online").GetComponent<Text>();
            GameObject obj3 = element.transform.FindChild("InviteButton").gameObject;
            Text text2 = element.transform.FindChild("PlayerName").GetComponent<Text>();
            Text text3 = element.transform.FindChild("NickName").GetComponent<Text>();
            COM_ACNT_GAME_STATE friendInGamingState = COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_IDLE;
            if (friend.bIsOnline != 1)
            {
                component.text = string.Format(Singleton<CTextManager>.instance.GetText("Common_Offline"), new object[0]);
                text2.color = CUIUtility.s_Color_Grey;
                text3.color = CUIUtility.s_Color_Grey;
                CUIUtility.GetComponentInChildren<Image>(gameObject).color = CUIUtility.s_Color_GrayShader;
            }
            else
            {
                friendInGamingState = Singleton<CFriendContoller>.instance.model.GetFriendInGamingState(friend.stUin.ullUid, friend.stUin.dwLogicWorldId);
                switch (friendInGamingState)
                {
                    case COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_IDLE:
                    {
                        component.text = Singleton<CInviteSystem>.instance.GetInviteStateStr(friend.stUin.ullUid);
                        CUIEventScript script = obj3.GetComponent<CUIEventScript>();
                        script.m_onClickEventParams.tag = (int) Singleton<CInviteSystem>.instance.InviteType;
                        script.m_onClickEventParams.commonUInt64Param1 = friend.stUin.ullUid;
                        break;
                    }
                    case COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_SINGLEGAME:
                    case COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_MULTIGAME:
                        component.text = string.Format("<color=#ffff00>{0}</color>", Singleton<CTextManager>.instance.GetText("Common_Gaming"));
                        break;

                    case COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_TEAM:
                        component.text = string.Format("<color=#ffff00>{0}</color>", Singleton<CTextManager>.instance.GetText("Common_Teaming"));
                        break;
                }
                text2.color = CUIUtility.s_Color_White;
                text3.color = CUIUtility.s_Color_White;
                CUIUtility.GetComponentInChildren<Image>(gameObject).color = CUIUtility.s_Color_White;
            }
            obj3.CustomSetActive((friend.bIsOnline == 1) && (friendInGamingState == COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_IDLE));
        }

        public static void SetInviteFriendData(CUIFormScript form, COM_INVITE_JOIN_TYPE joinType)
        {
            ListView<COMDT_FRIEND_INFO> allFriendList = Singleton<CInviteSystem>.instance.GetAllFriendList();
            int count = allFriendList.Count;
            int num2 = 0;
            CUIListScript component = form.GetWidget(2).GetComponent<CUIListScript>();
            component.SetElementAmount(count);
            form.GetWidget(3).gameObject.CustomSetActive(allFriendList.Count == 0);
            for (int i = 0; i < count; i++)
            {
                CUIListElementScript elemenet = component.GetElemenet(i);
                if ((elemenet != null) && (elemenet.gameObject != null))
                {
                    UpdateFriendListElement(elemenet.gameObject, allFriendList[i]);
                }
                if (allFriendList[i].bIsOnline == 1)
                {
                    num2++;
                }
            }
            string[] args = new string[] { num2.ToString(), count.ToString() };
            form.GetWidget(4).GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("Common_Online_Member", args);
        }

        public static void SetInviteGuildMemberData(CUIFormScript form, COM_INVITE_JOIN_TYPE joinType)
        {
            ListView<GuildMemInfo> allGuildMemberList = Singleton<CInviteSystem>.GetInstance().GetAllGuildMemberList();
            int count = allGuildMemberList.Count;
            int num2 = 0;
            RefreshInviteGuildMemberList(form, count);
            for (int i = 0; i < count; i++)
            {
                if (CGuildHelper.IsMemberOnline(allGuildMemberList[i]))
                {
                    num2++;
                }
            }
            string[] args = new string[] { num2.ToString(), count.ToString() };
            form.GetWidget(6).GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("Common_Online_Member", args);
        }

        public static void UpdateFriendListElement(GameObject element, COMDT_FRIEND_INFO friend)
        {
            UpdateFriendListElementBase(element, ref friend);
            SetFriendState(element, ref friend);
        }

        public static void UpdateFriendListElementBase(GameObject element, ref COMDT_FRIEND_INFO friend)
        {
            GameObject gameObject = element.transform.FindChild("HeadBg").gameObject;
            Text component = element.transform.FindChild("PlayerName").GetComponent<Text>();
            Text text2 = element.transform.FindChild("NickName").GetComponent<Text>();
            Image image = element.transform.FindChild("NobeIcon").GetComponent<Image>();
            Image image2 = element.transform.FindChild("HeadBg/NobeImag").GetComponent<Image>();
            if (image != null)
            {
                MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(image, (int) friend.stGameVip.dwCurLevel, false);
            }
            if (image2 != null)
            {
                MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(image2, (int) friend.stGameVip.dwHeadIconId);
            }
            component.text = Utility.UTF8Convert(friend.szUserName);
            CFriendModel.FriendInGame friendInGaming = Singleton<CFriendContoller>.instance.model.GetFriendInGaming(friend.stUin.ullUid, friend.stUin.dwLogicWorldId);
            if (friendInGaming == null)
            {
                text2.text = string.Empty;
            }
            else if (string.IsNullOrEmpty(friendInGaming.nickName))
            {
                text2.text = string.Empty;
            }
            else
            {
                text2.text = string.Format("({0})", friendInGaming.nickName);
            }
            string str = Utility.UTF8Convert(friend.szHeadUrl);
            if (!string.IsNullOrEmpty(str) && !CSysDynamicBlock.bFriendBlocked)
            {
                CUIUtility.GetComponentInChildren<CUIHttpImageScript>(gameObject).SetImageUrl(Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(str));
            }
        }

        public static void UpdateGuildMemberListElement(GameObject element, GuildMemInfo guildMember)
        {
            GameObject gameObject = element.transform.FindChild("HeadBg").gameObject;
            GameObject obj3 = element.transform.FindChild("InviteButton").gameObject;
            Text component = element.transform.FindChild("PlayerName").GetComponent<Text>();
            Text text2 = element.transform.FindChild("Online").GetComponent<Text>();
            Image image = element.transform.FindChild("NobeIcon").GetComponent<Image>();
            Image image2 = element.transform.FindChild("HeadBg/NobeImag").GetComponent<Image>();
            if (image != null)
            {
                MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(image, (int) guildMember.stBriefInfo.stVip.level, false);
            }
            if (image2 != null)
            {
                MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(image2, (int) guildMember.stBriefInfo.stVip.headIconId);
            }
            component.text = Utility.UTF8Convert(guildMember.stBriefInfo.sName);
            if (CGuildHelper.IsMemberOnline(guildMember))
            {
                if (guildMember.GameState == COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_IDLE)
                {
                    text2.text = string.Format("<color=#00ff00>{0}</color>", Singleton<CTextManager>.instance.GetText("Common_Online"));
                    CUIEventScript script = obj3.GetComponent<CUIEventScript>();
                    script.m_onClickEventParams.tag = (int) Singleton<CInviteSystem>.instance.InviteType;
                    script.m_onClickEventParams.commonUInt64Param1 = guildMember.stBriefInfo.uulUid;
                }
                else if ((guildMember.GameState == COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_SINGLEGAME) || (guildMember.GameState == COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_MULTIGAME))
                {
                    text2.text = string.Format("<color=#ffff00>{0}</color>", Singleton<CTextManager>.instance.GetText("Common_Gaming"));
                }
                else if (guildMember.GameState == COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_TEAM)
                {
                    text2.text = string.Format("<color=#ffff00>{0}</color>", Singleton<CTextManager>.instance.GetText("Common_Teaming"));
                }
                component.color = CUIUtility.s_Color_White;
                CUIUtility.GetComponentInChildren<Image>(gameObject).color = CUIUtility.s_Color_White;
            }
            else
            {
                text2.text = string.Format(Singleton<CTextManager>.instance.GetText("Common_Offline"), new object[0]);
                component.color = CUIUtility.s_Color_Grey;
                CUIUtility.GetComponentInChildren<Image>(gameObject).color = CUIUtility.s_Color_GrayShader;
            }
            obj3.CustomSetActive(CGuildHelper.IsMemberOnline(guildMember) && (guildMember.GameState == COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_IDLE));
            string szHeadUrl = guildMember.stBriefInfo.szHeadUrl;
            if (!string.IsNullOrEmpty(szHeadUrl))
            {
                CUIUtility.GetComponentInChildren<CUIHttpImageScript>(gameObject).SetImageUrl(Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(szHeadUrl));
            }
        }

        public enum enInviteFormWidget
        {
            Friend_Panel,
            GuildMember_Panel,
            Friend_List,
            FriendEmpty_Panel,
            FriendTotalNum_Text,
            GuildMember_List,
            GuildMemberTotalNum_Text,
            InviteTab_List,
            RefreshGuildMemberGameState_Timer
        }

        public enum enInviteListTab
        {
            Friend,
            GuildMember,
            Count
        }
    }
}

