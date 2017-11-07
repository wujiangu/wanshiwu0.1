namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;

    public class UT
    {
        public static void Add2List<T>(T data, ListView<T> list)
        {
            if ((data != null) && (list != null))
            {
                list.Add(data);
            }
        }

        public static bool BEqual_ACNT_UNIQ(COMDT_ACNT_UNIQ a, COMDT_ACNT_UNIQ b, bool ingore_worldid = false)
        {
            if (!ingore_worldid)
            {
                return ((a.ullUid == b.ullUid) && (a.dwLogicWorldId == b.dwLogicWorldId));
            }
            return (a.ullUid == b.ullUid);
        }

        public static string Bytes2String(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes).TrimEnd(new char[1]);
        }

        public static string Bytes2String(string str)
        {
            return str;
        }

        public static int CalcDeltaHorus(uint fromT, uint toT)
        {
            DateTime time = Utility.ToUtcTime2Local((long) fromT);
            DateTime time2 = Utility.ToUtcTime2Local((long) toT);
            if (DateTime.Compare(time2, time) > 0)
            {
                TimeSpan span = (TimeSpan) (time2 - time);
                return Math.Max((int) span.TotalHours, 1);
            }
            return 0;
        }

        public static void Check_AddHeartCD(COMDT_ACNT_UNIQ uniq, COM_FRIEND_TYPE friendType)
        {
            if (Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo() != null)
            {
                Singleton<CFriendContoller>.GetInstance().model.HeartData.Add(uniq, friendType);
            }
        }

        public static void Check_AddReCallCD(COMDT_ACNT_UNIQ uniq, COM_FRIEND_TYPE friendType)
        {
            Singleton<CFriendContoller>.GetInstance().model.SnsReCallData.Add(uniq, friendType);
        }

        public static bool DebugBattleMiniMapConfigInfo(SLevelContext data)
        {
            if (data == null)
            {
                return false;
            }
            if (!data.isPVPMode)
            {
                return false;
            }
            bool flag = true;
            string str = string.Format("---地图，id:{0},name:{1},", data.iLevelID, data.LevelName);
            if (string.IsNullOrEmpty(data.miniMapPath))
            {
                str = str + string.Format("缩略图路径配置为空,", new object[0]);
                flag = false;
            }
            if (string.IsNullOrEmpty(data.mapPath))
            {
                str = str + string.Format("地图路径配置为空,", new object[0]);
                flag = false;
            }
            if (data.mapWidth == 0)
            {
                str = str + string.Format("地图宽度配置为空,", new object[0]);
                flag = false;
            }
            if (data.mapHeight == 0)
            {
                str = str + string.Format("地图高度配置为空,", new object[0]);
                flag = false;
            }
            if (!flag)
            {
                return false;
            }
            return flag;
        }

        public static string ErrorCode_String(uint dwResult)
        {
            switch (dwResult)
            {
                case 0x65:
                    return GetText("Friend_CS_ERR_FRIEND_TCAPLUS_ERR");

                case 0x66:
                    return GetText("Friend_CS_ERR_FRIEND_RECORD_NOT_EXSIST");

                case 0x67:
                    return GetText("Friend_CS_ERR_FRIEND_NUM_EXCEED");

                case 0x68:
                    return GetText("Friend_CS_ERR_PEER_FRIEND_NUM_EXCEED");

                case 0x69:
                    return GetText("Friend_CS_ERR_FRIEND_DONATE_AP_EXCEED");

                case 0x6a:
                    return GetText("Friend_CS_ERR_FRIEND_RECV_AP_EXCEED");

                case 0x6b:
                    return GetText("Friend_CS_ERR_FRIEND_ADD_FRIEND_DENY");

                case 0x6c:
                    return GetText("Friend_CS_ERR_FRIEND_ADD_FRIEND_SELF");

                case 0x6d:
                    return GetText("Friend_CS_ERR_FRIEND_ADD_FRIEND_EXSIST");

                case 110:
                    return GetText("Friend_CS_ERR_FRIEND_REQ_REPEATED");

                case 0x6f:
                    return GetText("Friend_CS_ERR_FRIEND_NOT_EXSIST");

                case 0x70:
                    return GetText("Friend_CS_ERR_FRIEND_SEND_MAIL");

                case 0x71:
                    return GetText("Friend_CS_ERR_FRIEND_DONATE_REPEATED");

                case 0x72:
                    return GetText("Friend_CS_ERR_FRIEND_AP_FULL");

                case 0x74:
                    return GetText("Friend_CS_ERR_FRIEND_ADD_FRIEND_ZONE");

                case 0x75:
                    return GetText("Friend_CS_ERR_FRIEND_OTHER");

                case 2:
                    return GetText("Friend_CS_ERR_STARTSINGLEGAME_FAIL");

                case 3:
                    return GetText("Friend_CS_ERR_JOINMULTGAME_FAIL");

                case 4:
                    return GetText("Friend_CS_ERR_FINSINGLEGAME_FAIL");

                case 5:
                    return GetText("Friend_CS_ERR_QUITMULTGAME_FAIL");

                case 6:
                    return GetText("Friend_CS_ERR_REGISTER_NAME_DUP_FAIL");

                case 7:
                    return GetText("Friend_CS_ERR_SHOULD_REFRESH_TASK");

                case 8:
                    return GetText("Friend_CS_ERR_COMMIT_ERR");

                case 0x90:
                    return GetText("Friend_CS_ERR_FRIEND_RECALL_REPEATED");

                case 0x91:
                    return GetText("Friend_CS_ERR_FRIEND_EXCEED");

                case 0x92:
                    return GetText("Friend_CS_ERR_FRIEND_RECALL_TIME_LIMIT");
            }
            return string.Format(GetText("Friend_CS_ERR_FRIEND_DEFAULT"), dwResult);
        }

        public static string GetFriendResultTypeString(FriendResultType type)
        {
            if (type != FriendResultType.RequestBeFriend)
            {
                return string.Empty;
            }
            return GetText("Friend_Tips_Send_BeFriend_Ok");
        }

        public static string GetText(string key)
        {
            return Singleton<CTextManager>.instance.GetText(key);
        }

        public static string GetTimeString(uint time)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            if (masterRoleInfo == null)
            {
                return string.Empty;
            }
            string str = string.Empty;
            DateTime time2 = Utility.ToUtcTime2Local((long) time);
            DateTime time3 = Utility.ToUtcTime2Local((long) masterRoleInfo.getCurrentTimeSinceLogin());
            if (DateTime.Compare(time3, time2) <= 0)
            {
                return str;
            }
            TimeSpan span = (TimeSpan) (time3 - time2);
            if (span.Days == 0)
            {
                if (span.Hours == 0)
                {
                    return string.Format(GetText("Friend_Tips_lastTime_min"), span.Minutes);
                }
                return string.Format(GetText("Friend_Tips_lastTime_hour_min"), span.Hours, span.Minutes);
            }
            int days = span.Days;
            if (days > 7)
            {
                days = 7;
            }
            return string.Format(GetText("Friend_Tips_lastTime_day"), days);
        }

        public static void If_Null_Error<T>(T v) where T: class
        {
            if (v == null)
            {
            }
        }

        public static void ResetTimer(int timer, bool bPause)
        {
            Singleton<CTimerManager>.GetInstance().PauseTimer(timer);
            Singleton<CTimerManager>.GetInstance().ResetTimer(timer);
            if (!bPause)
            {
                Singleton<CTimerManager>.GetInstance().ResumeTimer(timer);
            }
        }

        public static void SetChatFace(CUIFormScript formScript, Image img, int index)
        {
            img.SetSprite(string.Format("UGUI/Sprite/Dynamic/ChatFace/{0}", index), formScript, true, false, false);
        }

        public static void SetHttpImage(CUIHttpImageScript HttpImage, byte[] szHeadUrl)
        {
            if (szHeadUrl != null)
            {
                SetHttpImage(HttpImage, Bytes2String(szHeadUrl));
            }
        }

        public static void SetHttpImage(CUIHttpImageScript HttpImage, string url)
        {
            if (((HttpImage != null) && !CSysDynamicBlock.bFriendBlocked) && (!string.IsNullOrEmpty(url) && HttpImage.gameObject.activeSelf))
            {
                HttpImage.SetImageUrl(Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(url));
            }
        }

        public static void SetImage(Image img, bool bGray)
        {
            if (img != null)
            {
                img.color = !bGray ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 1f, 1f, 1f);
            }
        }

        public static void SetListIndex(CUIListScript com, int index)
        {
            com.m_alwaysDispatchSelectedChangeEvent = true;
            com.SelectElement(index, true);
            com.m_alwaysDispatchSelectedChangeEvent = false;
        }

        public static void SetShow(CanvasGroup cg, bool bShow)
        {
            if (cg != null)
            {
                if (bShow)
                {
                    cg.alpha = 1f;
                    cg.blocksRaycasts = true;
                }
                else
                {
                    cg.alpha = 0f;
                    cg.blocksRaycasts = false;
                }
            }
        }

        public static void SetTabList(List<string> titles, int start_index, CUIListScript tablistScript)
        {
            if (tablistScript != null)
            {
                DebugHelper.Assert(start_index < titles.Count, "SetTabList, should start_index < titles.Count");
                tablistScript.SetElementAmount(titles.Count);
                for (int i = 0; i < tablistScript.m_elementAmount; i++)
                {
                    tablistScript.GetElemenet(i).gameObject.transform.FindChild("Text").GetComponent<Text>().text = titles[i];
                }
                tablistScript.m_alwaysDispatchSelectedChangeEvent = true;
                tablistScript.SelectElement(start_index, true);
                tablistScript.m_alwaysDispatchSelectedChangeEvent = false;
            }
        }

        public static void ShowFriendData(COMDT_FRIEND_INFO info, FriendShower com, FriendShower.ItemType type, bool bShowNickName, CFriendModel.FriendType friendType)
        {
            com.ullUid = info.stUin.ullUid;
            com.dwLogicWorldID = info.stUin.dwLogicWorldId;
            SetHttpImage(com.HttpImage, info.szHeadUrl);
            if (com.nobeIcon != null)
            {
                MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(com.nobeIcon.GetComponent<Image>(), (int) info.stGameVip.dwCurLevel, false);
            }
            else
            {
                Debug.Log("nobeicon " + ((int) info.stGameVip.dwCurLevel));
            }
            if (com.HeadIconBack != null)
            {
                MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(com.HeadIconBack.GetComponent<Image>(), (int) info.stGameVip.dwHeadIconId);
            }
            else
            {
                Debug.Log("HeadIconBack " + ((int) info.stGameVip.dwHeadIconId));
            }
            if (com.QQVipImage != null)
            {
                MonoSingleton<NobeSys>.GetInstance().SetOtherQQVipHead(com.QQVipImage.GetComponent<Image>(), (int) info.dwQQVIPMask);
            }
            else
            {
                Debug.Log("QQVipImage " + ((int) info.dwQQVIPMask));
            }
            com.SetFriendItemType(type, !bShowNickName);
            com.SetBGray((type == FriendShower.ItemType.Normal) && (info.bIsOnline != 1));
            com.ShowLevel(info.dwPvpLvl);
            com.ShowVipLevel(info.dwVipLvl);
            com.ShowLastTime(info.bIsOnline != 1, GetTimeString(info.dwLastLoginTime));
            CFriendModel.FriendInGame friendInGaming = Singleton<CFriendContoller>.instance.model.GetFriendInGaming(info.stUin.ullUid, info.stUin.dwLogicWorldId);
            if (friendInGaming == null)
            {
                com.ShowName(Bytes2String(info.szUserName));
                com.ShowGameState(COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_IDLE, info.bIsOnline == 1);
            }
            else
            {
                if (!string.IsNullOrEmpty(friendInGaming.nickName) && bShowNickName)
                {
                    com.ShowName(string.Format("{0}({1})", Bytes2String(info.szUserName), friendInGaming.nickName));
                }
                else
                {
                    com.ShowName(Bytes2String(info.szUserName));
                }
                com.ShowGameState(friendInGaming.State, info.bIsOnline == 1);
            }
            if (Singleton<CGuildSystem>.GetInstance().CanInvite(info))
            {
                if (Singleton<CGuildSystem>.GetInstance().HasInvited(info.stUin.ullUid))
                {
                    com.ShowinviteGuild(true, false);
                }
                else
                {
                    com.ShowinviteGuild(true, true);
                }
            }
            else if (Singleton<CGuildSystem>.GetInstance().CanRecommend(info))
            {
                if (Singleton<CGuildSystem>.GetInstance().HasRecommended(info.stUin.ullUid))
                {
                    com.ShowRecommendGuild(true, false);
                }
                else
                {
                    com.ShowRecommendGuild(true, true);
                }
            }
            else
            {
                com.ShowinviteGuild(false, false);
            }
            bool flag = Singleton<CFriendContoller>.instance.model.HeartData.BCanSendHeart(info.stUin, COM_FRIEND_TYPE.COM_FRIEND_TYPE_GAME);
            bool flag2 = Singleton<CFriendContoller>.instance.model.HeartData.BCanSendHeart(info.stUin, COM_FRIEND_TYPE.COM_FRIEND_TYPE_SNS);
            com.ShowSendButton(flag && flag2);
            if (CSysDynamicBlock.bSocialBlocked)
            {
                com.HideSendButton();
            }
            if (CSysDynamicBlock.bSocialBlocked)
            {
                com.ShowInviteButton(false, false);
            }
            else if (friendType == CFriendModel.FriendType.GameFriend)
            {
                com.ShowInviteButton(false, false);
            }
            else if (friendType == CFriendModel.FriendType.SNS)
            {
                bool isShow = CFriendReCallData.BLose(info.stUin, COM_FRIEND_TYPE.COM_FRIEND_TYPE_SNS);
                bool flag4 = Singleton<CFriendContoller>.instance.model.SnsReCallData.BInCd(info.stUin, COM_FRIEND_TYPE.COM_FRIEND_TYPE_SNS);
                com.ShowInviteButton(isShow, !flag4);
            }
            com.ShowGenderType((COM_SNSGENDER) info.bGender);
        }

        public static void ShowFriendNetResult(uint dwResult, FriendResultType type)
        {
            string strContent = string.Empty;
            if (dwResult == 0)
            {
                strContent = GetFriendResultTypeString(type);
            }
            else
            {
                strContent = ErrorCode_String(dwResult);
            }
            Singleton<CUIManager>.GetInstance().OpenTips(strContent, false, 1f, null, new object[0]);
        }

        public static void ShowSNSFriendData(COMDT_SNS_FRIEND_INFO info, FriendShower com)
        {
            com.ullUid = info.ullUid;
            com.dwLogicWorldID = info.dwLogicWorldId;
            string str = Bytes2String(info.szHeadUrl);
            if (!string.IsNullOrEmpty(str))
            {
                com.HttpImage.SetImageUrl(Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(str));
            }
            com.SetFriendItemType(FriendShower.ItemType.Add, true);
            com.SetBGray(false);
            com.ShowName(string.Format("{0}({1})", Bytes2String(info.szRoleName), Bytes2String(info.szNickName)));
            com.ShowLevel(info.dwPvpLvl);
            com.ShowVipLevel(info.dwPvpLvl);
            com.ShowLastTime(true, GetTimeString(info.dwLastLoginTime));
        }

        public static byte[] String2Bytes(string name)
        {
            return Encoding.UTF8.GetBytes(name);
        }

        public enum FriendResultType
        {
            RequestBeFriend
        }
    }
}

