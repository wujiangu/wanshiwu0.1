namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class CMatchingView
    {
        private static COM_AI_LEVEL[] mapDifficultyList = new COM_AI_LEVEL[] { 
            COM_AI_LEVEL.COM_AI_LEVEL_MIDDLE_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_MIDDLE_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_EASY_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_MIDDLE_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_HARD_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_EASY_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_MIDDLE_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_HARD_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_EASY_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_MIDDLE_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_HARD_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_EASY_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_MIDDLE_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_HARD_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_MIDDLE_OF_NORMALBATTLE, COM_AI_LEVEL.COM_AI_LEVEL_MIDDLE_OF_NORMALBATTLE, 
            COM_AI_LEVEL.COM_AI_LEVEL_MIDDLE_OF_NORMALBATTLE
         };
        private const int ShowDefaultHeadImgStartLadderLevel = 7;

        public static TeamMember GetMemberInfo(TeamInfo teamInfo, int Pos)
        {
            for (int i = 0; i < teamInfo.MemInfoList.Count; i++)
            {
                if (teamInfo.MemInfoList[i].dwPosOfTeam == (Pos - 1))
                {
                    return teamInfo.MemInfoList[i];
                }
            }
            return null;
        }

        public static void InitConfirmBox(GameObject root, int PlayerNum, ref Assets.Scripts.GameSystem.RoomInfo roomInfo, CUIFormScript form)
        {
            if (root.transform.Find("Panel/Timer") != null)
            {
                CUITimerScript script = root.transform.Find("Panel/Timer").GetComponent<CUITimerScript>();
                if (script != null)
                {
                    script.EndTimer();
                    script.StartTimer();
                }
            }
            if (root.transform.Find("Panel/Panel/Timer") != null)
            {
                CUITimerScript script2 = root.transform.Find("Panel/Panel/Timer").GetComponent<CUITimerScript>();
                if (script2 != null)
                {
                    script2.EndTimer();
                    script2.StartTimer();
                }
            }
            Transform transform = root.transform.Find("Panel/Panel/stateGroup");
            GridLayoutGroup component = transform.GetComponent<GridLayoutGroup>();
            if (component != null)
            {
                component.constraintCount = (PlayerNum != 6) ? 5 : 3;
            }
            bool flag = roomInfo.roomAttrib.bPkAI == 2;
            int num = !(!roomInfo.roomAttrib.bWarmBattle ? flag : false) ? PlayerNum : (PlayerNum / 2);
            for (int i = 1; i <= 10; i++)
            {
                transform.Find(string.Format("icon{0}", i)).gameObject.CustomSetActive(num >= i);
            }
            int num3 = 1;
            for (int j = 0; j < roomInfo.CampMemberList.Length; j++)
            {
                for (int k = 0; k < roomInfo.CampMemberList[j].Count; k++)
                {
                    MemberInfo info = roomInfo.CampMemberList[j][k];
                    Transform transform2 = transform.Find(string.Format("icon{0}", num3));
                    CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                    if ((((roomInfo.roomAttrib.bMapType == 3) && (masterRoleInfo != null)) && (masterRoleInfo.m_gradeOfRank >= 7)) || (roomInfo.roomAttrib.bMapType == 5))
                    {
                        Image image = transform2.Find("HttpImage").GetComponent<Image>();
                        if (image != null)
                        {
                            image.SetSprite(CUIUtility.s_Sprite_Dynamic_BustPlayer_Dir + "Common_PlayerImg", form, true, false, false);
                        }
                    }
                    else
                    {
                        CUIHttpImageScript script3 = transform2.Find("HttpImage").GetComponent<CUIHttpImageScript>();
                        if (script3 != null)
                        {
                            script3.SetImageUrl(Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(info.MemberHeadUrl));
                        }
                    }
                    Transform transform3 = transform.Find(string.Format("icon{0}/ready", num3));
                    if (transform3 != null)
                    {
                        transform3.gameObject.CustomSetActive(false);
                    }
                    Transform transform4 = transform.Find(string.Format("icon{0}/unready", num3));
                    if (transform4 != null)
                    {
                        transform4.gameObject.CustomSetActive(true);
                    }
                    num3++;
                }
            }
            Transform transform5 = root.transform.Find("Panel/Panel/TxtReadyNum");
            if (transform5 != null)
            {
                Text text = transform5.GetComponent<Text>();
                if (text != null)
                {
                    text.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Matching_Confirm_PlayerNum"), 0, num);
                }
            }
            Transform transform6 = root.transform.Find("Panel/Panel/btnGroup/Button_Confirm");
            if (transform6 != null)
            {
                transform6.GetComponent<Button>().interactable = true;
            }
        }

        public static void InitMatchingEntry(CUIFormScript form)
        {
            uint[] numArray = new uint[0x11];
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_PVP_1V1"), out numArray[0]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_PVP_3V3"), out numArray[1]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Robot_1V1_1"), out numArray[2]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Robot_1V1_1"), out numArray[3]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Robot_1V1_1"), out numArray[4]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Robot_3V3Team_1"), out numArray[5]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Robot_3V3Team_1"), out numArray[6]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Robot_3V3Team_1"), out numArray[7]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Robot_5V5Team_1"), out numArray[8]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Robot_5V5Team_1"), out numArray[9]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Robot_5V5Team_1"), out numArray[10]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Robot_Melee_Team_1"), out numArray[11]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Robot_Melee_Team_1"), out numArray[12]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Robot_Melee_Team_1"), out numArray[13]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_PVP_5V5"), out numArray[14]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_PVP_MELEE"), out numArray[15]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_PVP_Fire"), out numArray[0x10]);
            CUIMiniEventScript[] scriptArray = new CUIMiniEventScript[] { 
                form.transform.Find("panelGroup2/btnGroup/Button1").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup2/btnGroup/Button2").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup3/btnGroup/Button1/btnGrp/Button1").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup3/btnGroup/Button1/btnGrp/Button2").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup3/btnGroup/Button1/btnGrp/Button3").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup3/btnGroup/Button2/btnGrp/Button1").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup3/btnGroup/Button2/btnGrp/Button2").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup3/btnGroup/Button2/btnGrp/Button3").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup3/btnGroup/Button3/btnGrp/Button1").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup3/btnGroup/Button3/btnGrp/Button2").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup3/btnGroup/Button3/btnGrp/Button3").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup3/btnGroup/Button4/btnGrp/Button1").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup3/btnGroup/Button4/btnGrp/Button2").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup3/btnGroup/Button4/btnGrp/Button3").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup2/btnGroup/Button4").GetComponent<CUIMiniEventScript>(), form.transform.Find("panelGroup2/btnGroup/Button3").GetComponent<CUIMiniEventScript>(), 
                form.transform.Find("panelGroup2/btnGroup/Button5").GetComponent<CUIMiniEventScript>()
             };
            for (int i = 0; i < numArray.Length; i++)
            {
                scriptArray[i].m_onClickEventParams.tagUInt = numArray[i];
                scriptArray[i].m_onClickEventParams.tag = (int) mapDifficultyList[i];
            }
            form.transform.Find("panelGroup1/btnGroup/Button1").GetComponent<CUIMiniEventScript>().m_onClickEventParams.tag = 1;
            form.transform.Find("panelGroup1/btnGroup/Button2").GetComponent<CUIMiniEventScript>().m_onClickEventParams.tag = 2;
            form.transform.Find("panelGroup3/btnGroup/Button1").GetComponent<CUIMiniEventScript>().m_onClickEventParams.tag = 0;
            form.transform.Find("panelGroup3/btnGroup/Button2").GetComponent<CUIMiniEventScript>().m_onClickEventParams.tag = 1;
            form.transform.Find("panelGroup3/btnGroup/Button3").GetComponent<CUIMiniEventScript>().m_onClickEventParams.tag = 2;
            form.transform.Find("panelGroup3/btnGroup/Button4").GetComponent<CUIMiniEventScript>().m_onClickEventParams.tag = 3;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                GameObject gameObject = form.transform.FindChild("CoinAndExp/DoubleCoin").gameObject;
                GameObject obj3 = form.transform.FindChild("CoinAndExp/DoubleExp").gameObject;
                obj3.CustomSetActive(false);
                gameObject.CustomSetActive(false);
                masterRoleInfo.UpdateCoinAndExpValidTime();
                if (masterRoleInfo.HaveExtraCoin())
                {
                    gameObject.CustomSetActive(true);
                    string str = string.Empty;
                    string str2 = string.Empty;
                    if (masterRoleInfo.GetCoinExpireHours() > 0)
                    {
                        str = string.Format(Singleton<CTextManager>.GetInstance().GetText("DoubleCoinExpireTimeTips"), masterRoleInfo.GetCoinExpireHours() / 0x18, masterRoleInfo.GetCoinExpireHours() % 0x18);
                    }
                    if (masterRoleInfo.GetCoinWinCount() > 0)
                    {
                        str2 = string.Format(Singleton<CTextManager>.GetInstance().GetText("DoubleCoinCountWinTips"), masterRoleInfo.GetCoinWinCount());
                    }
                    if (string.IsNullOrEmpty(str))
                    {
                        CUICommonSystem.SetCommonTipsEvent(form, gameObject, string.Format("{0}", str2), enUseableTipsPos.enBottom);
                    }
                    else if (string.IsNullOrEmpty(str2))
                    {
                        CUICommonSystem.SetCommonTipsEvent(form, gameObject, string.Format("{0}", str), enUseableTipsPos.enBottom);
                    }
                    else
                    {
                        CUICommonSystem.SetCommonTipsEvent(form, gameObject, string.Format("{0}\n{1}", str, str2), enUseableTipsPos.enBottom);
                    }
                }
                if (masterRoleInfo.HaveExtraExp())
                {
                    obj3.CustomSetActive(true);
                    string str5 = string.Empty;
                    string str6 = string.Empty;
                    if (masterRoleInfo.GetExpExpireHours() > 0)
                    {
                        str5 = string.Format(Singleton<CTextManager>.GetInstance().GetText("DoubleExpExpireTimeTips"), masterRoleInfo.GetExpExpireHours() / 0x18, masterRoleInfo.GetExpExpireHours() % 0x18);
                    }
                    if (masterRoleInfo.GetExpWinCount() > 0)
                    {
                        str6 = string.Format(Singleton<CTextManager>.GetInstance().GetText("DoubleExpCountWinTips"), masterRoleInfo.GetExpWinCount());
                    }
                    if (string.IsNullOrEmpty(str5))
                    {
                        CUICommonSystem.SetCommonTipsEvent(form, obj3, string.Format("{0}", str6), enUseableTipsPos.enBottom);
                    }
                    else if (string.IsNullOrEmpty(str6))
                    {
                        CUICommonSystem.SetCommonTipsEvent(form, obj3, string.Format("{0}", str5), enUseableTipsPos.enBottom);
                    }
                    else
                    {
                        CUICommonSystem.SetCommonTipsEvent(form, obj3, string.Format("{0}\n{1}", str5, str6), enUseableTipsPos.enBottom);
                    }
                }
                if (CSysDynamicBlock.bLobbyEntryBlocked)
                {
                    Transform transform = form.transform.Find("panelBottom/btnShop");
                    if (transform != null)
                    {
                        transform.gameObject.CustomSetActive(false);
                    }
                    Transform transform2 = form.transform.Find("CoinAndExp");
                    if (transform2 != null)
                    {
                        transform2.gameObject.CustomSetActive(false);
                    }
                }
                GameObject obj4 = form.gameObject.transform.Find("Panel").gameObject;
                obj4.transform.Find("Name").gameObject.GetComponent<Text>().text = masterRoleInfo.Name;
                ResAcntPvpExpInfo dataByKey = GameDataMgr.acntPvpExpDatabin.GetDataByKey((byte) masterRoleInfo.PvpLevel);
                DebugHelper.Assert(dataByKey != null);
                DebugHelper.Assert(dataByKey.dwNeedExp > 0);
                obj4.transform.Find("DegreeBarBg/bar").gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(204f * Math.Min((float) 1f, (float) ((masterRoleInfo.PvpExp * 1f) / ((float) dataByKey.dwNeedExp))), 19f);
                obj4.transform.Find("DegreeTitle").gameObject.CustomSetActive(false);
                if (masterRoleInfo.PvpLevel >= GameDataMgr.acntPvpExpDatabin.Count())
                {
                    obj4.transform.Find("DegreeNum").gameObject.GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("PVP_Level_Max");
                }
                else
                {
                    obj4.transform.Find("DegreeNum").gameObject.GetComponent<Text>().text = string.Format("{0}/{1}", masterRoleInfo.PvpExp, dataByKey.dwNeedExp);
                }
                obj4.transform.Find("DegreeIcon").gameObject.CustomSetActive(false);
                MonoSingleton<NewbieGuideManager>.GetInstance().CheckTriggerTime(NewbieGuideTriggerTimeType.enterPvpEntry, new uint[0]);
            }
        }

        public static void SetPkMode(CUIFormScript form, bool bPkAI)
        {
            string prefabPath = !bPkAI ? "UGUI/Sprite/System/Matching/playerPVP_btn_Selected" : "UGUI/Sprite/System/Matching/playerPVP_btn_Normal";
            string str2 = !bPkAI ? "UGUI/Sprite/System/Matching/computerPVP_btn_Normal" : "UGUI/Sprite/System/Matching/computerPVP_btn_Selected";
            form.gameObject.transform.Find("Panel_Menu/Btn_PvP").gameObject.GetComponent<Image>().SetSprite(prefabPath, form, true, false, false);
            form.gameObject.transform.Find("Panel_Menu/Btn_PkAI").gameObject.GetComponent<Image>().SetSprite(str2, form, true, false, false);
            form.gameObject.transform.Find("Panel_Main/TxtPkMode").gameObject.GetComponent<Text>().text = !bPkAI ? Singleton<CTextManager>.GetInstance().GetText("PVP_VS_Player") : Singleton<CTextManager>.GetInstance().GetText("PVP_VS_NPC");
        }

        public static void SetPlayerSlotData(GameObject item, TeamMember memberInfo, bool bAvailable)
        {
            if (bAvailable)
            {
                bool bActive = false;
                bool flag2 = false;
                bool isSelfTeamMaster = Singleton<CMatchingSystem>.GetInstance().IsSelfTeamMaster;
                if (memberInfo == null)
                {
                    item.CustomSetActive(true);
                    item.transform.Find("Occupied").gameObject.CustomSetActive(false);
                }
                else
                {
                    PlayerUniqueID stTeamMaster = Singleton<CMatchingSystem>.GetInstance().teamInfo.stTeamMaster;
                    bActive = (stTeamMaster.ullUid == memberInfo.uID.ullUid) && (stTeamMaster.iGameEntity == memberInfo.uID.iGameEntity);
                    PlayerUniqueID stSelfInfo = Singleton<CMatchingSystem>.GetInstance().teamInfo.stSelfInfo;
                    flag2 = (stSelfInfo.ullUid == memberInfo.uID.ullUid) && (stSelfInfo.iGameEntity == memberInfo.uID.iGameEntity);
                    item.CustomSetActive(true);
                    item.transform.Find("Occupied").gameObject.CustomSetActive(true);
                    CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CInviteSystem.PATH_INVITE_FORM);
                    bool flag4 = true;
                    if (form != null)
                    {
                        flag4 = form.GetWidget(7).GetComponent<CUIListScript>().GetSelectedIndex() == 0;
                    }
                    string str = !flag4 ? Singleton<CInviteSystem>.GetInstance().GetInviteGuildMemberName(memberInfo.uID.ullUid) : Singleton<CInviteSystem>.GetInstance().GetInviteFriendName(memberInfo.uID.ullUid, (uint) memberInfo.uID.iLogicWorldId);
                    item.transform.Find("Occupied/txtPlayerName").GetComponent<Text>().text = !string.IsNullOrEmpty(str) ? str : memberInfo.MemberName;
                    item.transform.Find("Occupied/BtnKick").gameObject.CustomSetActive(isSelfTeamMaster && !flag2);
                    Transform transform = item.transform.Find("Occupied/HeadBg/NobeIcon");
                    Transform transform2 = item.transform.Find("Occupied/HeadBg/NobeImag");
                    if (flag2)
                    {
                        if (!CSysDynamicBlock.bFriendBlocked)
                        {
                            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                            if (masterRoleInfo != null)
                            {
                                Transform transform3 = item.transform.Find("Occupied/HeadBg/imgHead");
                                if (transform3 != null)
                                {
                                    transform3.GetComponent<CUIHttpImageScript>().SetImageUrl(masterRoleInfo.HeadUrl);
                                }
                            }
                        }
                    }
                    else
                    {
                        bool flag5 = Singleton<CFriendContoller>.instance.model.IsGameFriend(memberInfo.uID.ullUid);
                        COMDT_FRIEND_INFO comdt_friend_info = Singleton<CFriendContoller>.instance.model.getFriendByUid(memberInfo.uID.ullUid, !flag5 ? CFriendModel.FriendType.SNS : CFriendModel.FriendType.GameFriend);
                        if (comdt_friend_info != null)
                        {
                            string str2 = Utility.UTF8Convert(comdt_friend_info.szHeadUrl);
                            if (!string.IsNullOrEmpty(str2) && !CSysDynamicBlock.bFriendBlocked)
                            {
                                Transform transform4 = item.transform.Find("Occupied/HeadBg/imgHead");
                                if (transform4 != null)
                                {
                                    transform4.GetComponent<CUIHttpImageScript>().SetImageUrl(Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(str2));
                                }
                            }
                        }
                    }
                }
                item.transform.Find("Occupied/imgOwner").gameObject.CustomSetActive(bActive);
            }
            else
            {
                item.CustomSetActive(false);
            }
        }

        public static void SetStartBtnStatus(GameObject root)
        {
            GameObject gameObject = root.transform.Find("Panel_Main/Btn_Matching").gameObject;
            bool isSelfTeamMaster = Singleton<CMatchingSystem>.GetInstance().IsSelfTeamMaster;
            gameObject.CustomSetActive(isSelfTeamMaster);
        }

        public static void UpdateConfirmBox(GameObject root, ulong confirmPlayerUid)
        {
            Assets.Scripts.GameSystem.RoomInfo roomInfo = Singleton<CRoomSystem>.GetInstance().roomInfo;
            DebugHelper.Assert(roomInfo != null, "Room Info is NULL!!!");
            int currentMapPlayerNum = Singleton<CMatchingSystem>.GetInstance().currentMapPlayerNum;
            int confirmPlayerNum = Singleton<CMatchingSystem>.GetInstance().confirmPlayerNum;
            Transform transform = root.transform.Find("Panel/Panel/stateGroup");
            if (transform != null)
            {
                int num3 = 1;
                for (int i = 0; i < roomInfo.CampMemberList.Length; i++)
                {
                    for (int j = 0; j < roomInfo.CampMemberList[i].Count; j++)
                    {
                        MemberInfo info2 = roomInfo.CampMemberList[i][j];
                        if (info2.ullUid == confirmPlayerUid)
                        {
                            Transform transform2 = transform.Find(string.Format("icon{0}/ready", num3));
                            if (transform2 != null)
                            {
                                transform2.gameObject.CustomSetActive(true);
                            }
                            Transform transform3 = transform.Find(string.Format("icon{0}/unready", num3));
                            if (transform3 != null)
                            {
                                transform3.gameObject.CustomSetActive(false);
                            }
                            break;
                        }
                        num3++;
                    }
                }
            }
            bool flag = roomInfo.roomAttrib.bPkAI == 2;
            int num6 = !(!roomInfo.roomAttrib.bWarmBattle ? flag : false) ? currentMapPlayerNum : (currentMapPlayerNum / 2);
            Transform transform4 = root.transform.Find("Panel/Panel/TxtReadyNum");
            if (transform4 != null)
            {
                Text component = transform4.GetComponent<Text>();
                if (component != null)
                {
                    component.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Matching_Confirm_PlayerNum"), confirmPlayerNum, num6);
                }
            }
        }
    }
}

