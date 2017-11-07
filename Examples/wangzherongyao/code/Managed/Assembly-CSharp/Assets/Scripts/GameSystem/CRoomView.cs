namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    internal class CRoomView
    {
        private static MemberInfo GetMemberInfo(Assets.Scripts.GameSystem.RoomInfo roomInfo, COM_PLAYERCAMP Camp, int Pos)
        {
            ListView<MemberInfo> view = roomInfo.CampMemberList[(Camp != COM_PLAYERCAMP.COM_PLAYERCAMP_1) ? 1 : 0];
            for (int i = 0; i < view.Count; i++)
            {
                if (view[i].dwPosOfCamp == (Pos - 1))
                {
                    return view[i];
                }
            }
            return null;
        }

        private static void SetPlayerSlotData(GameObject item, MemberInfo memberInfo, bool bAvailable)
        {
            if (bAvailable)
            {
                bool isSelfRoomOwner = Singleton<CRoomSystem>.GetInstance().IsSelfRoomOwner;
                if (memberInfo == null)
                {
                    item.CustomSetActive(true);
                    item.transform.Find("Occupied").gameObject.CustomSetActive(false);
                }
                else
                {
                    item.CustomSetActive(true);
                    item.transform.Find("Occupied").gameObject.CustomSetActive(true);
                    bool bActive = false;
                    bActive = Singleton<CRoomSystem>.GetInstance().roomInfo.roomOwner.ullUid == memberInfo.ullUid;
                    bool flag3 = false;
                    flag3 = Singleton<CRoomSystem>.GetInstance().roomInfo.selfInfo.ullUid == memberInfo.ullUid;
                    item.transform.Find("Occupied/imgOwner").gameObject.CustomSetActive(bActive);
                    CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CInviteSystem.PATH_INVITE_FORM);
                    bool flag4 = true;
                    if (form != null)
                    {
                        flag4 = form.GetWidget(7).GetComponent<CUIListScript>().GetSelectedIndex() == 0;
                    }
                    string str = !flag4 ? Singleton<CInviteSystem>.GetInstance().GetInviteGuildMemberName(memberInfo.ullUid) : Singleton<CInviteSystem>.GetInstance().GetInviteFriendName(memberInfo.ullUid, (uint) memberInfo.iLogicWorldID);
                    item.transform.Find("Occupied/txtPlayerName").GetComponent<Text>().text = !string.IsNullOrEmpty(str) ? str : memberInfo.MemberName;
                    if (flag3)
                    {
                        if (!CSysDynamicBlock.bFriendBlocked)
                        {
                            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                            if (masterRoleInfo != null)
                            {
                                item.transform.Find("Occupied/HeadBg/imgHead").GetComponent<CUIHttpImageScript>().SetImageUrl(masterRoleInfo.HeadUrl);
                            }
                        }
                    }
                    else if (memberInfo.RoomMemberType == 1)
                    {
                        if (!string.IsNullOrEmpty(memberInfo.MemberHeadUrl))
                        {
                            if (!CSysDynamicBlock.bFriendBlocked)
                            {
                                item.transform.Find("Occupied/HeadBg/imgHead").GetComponent<CUIHttpImageScript>().SetImageUrl(Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(memberInfo.MemberHeadUrl));
                            }
                            else
                            {
                                CUIFormScript formScript = Singleton<CUIManager>.GetInstance().GetForm(CRoomSystem.PATH_ROOM);
                                if (formScript != null)
                                {
                                    item.transform.Find("Occupied/HeadBg/imgHead").GetComponent<Image>().SetSprite(CUIUtility.s_Sprite_Dynamic_BustPlayer_Dir + "Common_PlayerImg", formScript, true, false, false);
                                }
                            }
                        }
                    }
                    else if (memberInfo.RoomMemberType == 2)
                    {
                        CUIFormScript script4 = Singleton<CUIManager>.GetInstance().GetForm(CRoomSystem.PATH_ROOM);
                        if (script4 != null)
                        {
                            item.transform.Find("Occupied/HeadBg/imgHead").GetComponent<Image>().SetSprite(CUIUtility.s_Sprite_Dynamic_BustPlayer_Dir + "Img_ComputerHead", script4, true, false, false);
                        }
                    }
                    item.transform.Find("Occupied/BtnKick").gameObject.CustomSetActive(isSelfRoomOwner && !flag3);
                }
            }
            else
            {
                item.CustomSetActive(false);
            }
        }

        public static void SetRoomData(GameObject root, Assets.Scripts.GameSystem.RoomInfo data)
        {
            uint dwMapId = data.roomAttrib.dwMapId;
            ResAcntBattleLevelInfo dataByKey = GameDataMgr.pvpLevelDatabin.GetDataByKey(dwMapId);
            DebugHelper.Assert(dataByKey != null);
            SetStartBtnStatus(root, data);
            UpdateBtnStatus(root, data);
            int bMaxAcntNum = dataByKey.stLevelCommonInfo.bMaxAcntNum;
            root.transform.Find("Panel_Main/MapInfo/txtMapName").gameObject.GetComponent<Text>().text = StringHelper.UTF8BytesToString(ref dataByKey.stLevelCommonInfo.szName);
            root.transform.Find("Panel_Main/MapInfo/txtTeam").gameObject.GetComponent<Text>().text = Singleton<CTextManager>.instance.GetText(string.Format("Common_Team_Player_Type_{0}", bMaxAcntNum / 2));
            GameObject gameObject = root.transform.Find("Panel_Main/Left_Player1").gameObject;
            MemberInfo memberInfo = GetMemberInfo(data, COM_PLAYERCAMP.COM_PLAYERCAMP_1, 1);
            SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 2);
            gameObject = root.transform.Find("Panel_Main/Left_Player2").gameObject;
            memberInfo = GetMemberInfo(data, COM_PLAYERCAMP.COM_PLAYERCAMP_1, 2);
            SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 4);
            gameObject = root.transform.Find("Panel_Main/Left_Player3").gameObject;
            memberInfo = GetMemberInfo(data, COM_PLAYERCAMP.COM_PLAYERCAMP_1, 3);
            SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 6);
            gameObject = root.transform.Find("Panel_Main/Left_Player4").gameObject;
            memberInfo = GetMemberInfo(data, COM_PLAYERCAMP.COM_PLAYERCAMP_1, 4);
            SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 8);
            gameObject = root.transform.Find("Panel_Main/Left_Player5").gameObject;
            memberInfo = GetMemberInfo(data, COM_PLAYERCAMP.COM_PLAYERCAMP_1, 5);
            SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 10);
            gameObject = root.transform.Find("Panel_Main/Right_Player1").gameObject;
            memberInfo = GetMemberInfo(data, COM_PLAYERCAMP.COM_PLAYERCAMP_2, 1);
            SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 2);
            gameObject = root.transform.Find("Panel_Main/Right_Player2").gameObject;
            memberInfo = GetMemberInfo(data, COM_PLAYERCAMP.COM_PLAYERCAMP_2, 2);
            SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 4);
            gameObject = root.transform.Find("Panel_Main/Right_Player3").gameObject;
            memberInfo = GetMemberInfo(data, COM_PLAYERCAMP.COM_PLAYERCAMP_2, 3);
            SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 6);
            gameObject = root.transform.Find("Panel_Main/Right_Player4").gameObject;
            memberInfo = GetMemberInfo(data, COM_PLAYERCAMP.COM_PLAYERCAMP_2, 4);
            SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 8);
            gameObject = root.transform.Find("Panel_Main/Right_Player5").gameObject;
            memberInfo = GetMemberInfo(data, COM_PLAYERCAMP.COM_PLAYERCAMP_2, 5);
            SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 10);
        }

        public static void SetStartBtnStatus(GameObject root, Assets.Scripts.GameSystem.RoomInfo data)
        {
            GameObject gameObject = root.transform.Find("Panel_Main/Btn_Start").gameObject;
            bool isSelfRoomOwner = Singleton<CRoomSystem>.GetInstance().IsSelfRoomOwner;
            gameObject.CustomSetActive(isSelfRoomOwner);
            if (isSelfRoomOwner)
            {
                Button component = gameObject.GetComponent<Button>();
                bool flag2 = (data.CampMemberList[0].Count > 0) && (data.CampMemberList[1].Count > 0);
                component.interactable = flag2;
            }
        }

        public static void UpdateBtnStatus(GameObject root, Assets.Scripts.GameSystem.RoomInfo data)
        {
            uint dwMapId = data.roomAttrib.dwMapId;
            ResAcntBattleLevelInfo dataByKey = GameDataMgr.pvpLevelDatabin.GetDataByKey(dwMapId);
            DebugHelper.Assert(dataByKey != null);
            int bMaxAcntNum = dataByKey.stLevelCommonInfo.bMaxAcntNum;
            bool isSelfRoomOwner = Singleton<CRoomSystem>.GetInstance().IsSelfRoomOwner;
            GameObject gameObject = root.transform.Find("Panel_Main/bg1/LeftRobot").gameObject;
            GameObject obj3 = root.transform.Find("Panel_Main/bg1/LeftJoin").gameObject;
            GameObject obj4 = root.transform.Find("Panel_Main/bg2/RightRobot").gameObject;
            GameObject obj5 = root.transform.Find("Panel_Main/bg2/RightJoin").gameObject;
            gameObject.CustomSetActive(false);
            obj3.CustomSetActive(false);
            obj4.CustomSetActive(false);
            obj5.CustomSetActive(false);
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            if (isSelfRoomOwner)
            {
                if (masterRoleInfo != null)
                {
                    MemberInfo memberInfo = data.GetMemberInfo(masterRoleInfo.playerUllUID);
                    if (memberInfo != null)
                    {
                        COM_PLAYERCAMP camp = (memberInfo.camp != COM_PLAYERCAMP.COM_PLAYERCAMP_1) ? COM_PLAYERCAMP.COM_PLAYERCAMP_1 : COM_PLAYERCAMP.COM_PLAYERCAMP_2;
                        if (data.GetFreePos(memberInfo.camp, bMaxAcntNum) >= 0)
                        {
                            if (memberInfo.camp == COM_PLAYERCAMP.COM_PLAYERCAMP_1)
                            {
                                gameObject.CustomSetActive(true);
                            }
                            else
                            {
                                obj4.CustomSetActive(true);
                            }
                        }
                        if (data.GetFreePos(camp, bMaxAcntNum) >= 0)
                        {
                            if (camp == COM_PLAYERCAMP.COM_PLAYERCAMP_1)
                            {
                                gameObject.CustomSetActive(true);
                                obj3.CustomSetActive(true);
                            }
                            else
                            {
                                obj4.CustomSetActive(true);
                                obj5.CustomSetActive(true);
                            }
                        }
                    }
                }
            }
            else if (masterRoleInfo != null)
            {
                MemberInfo info4 = data.GetMemberInfo(masterRoleInfo.playerUllUID);
                if (info4 != null)
                {
                    COM_PLAYERCAMP com_playercamp2 = (info4.camp != COM_PLAYERCAMP.COM_PLAYERCAMP_1) ? COM_PLAYERCAMP.COM_PLAYERCAMP_1 : COM_PLAYERCAMP.COM_PLAYERCAMP_2;
                    if (data.GetFreePos(com_playercamp2, bMaxAcntNum) >= 0)
                    {
                        if (com_playercamp2 == COM_PLAYERCAMP.COM_PLAYERCAMP_1)
                        {
                            obj3.CustomSetActive(true);
                        }
                        else
                        {
                            obj5.CustomSetActive(true);
                        }
                    }
                }
            }
            CUIEventScript component = gameObject.GetComponent<CUIEventScript>();
            component.m_onClickEventID = enUIEventID.Room_AddRobot;
            component.m_onClickEventParams.tag = 1;
            component = obj3.GetComponent<CUIEventScript>();
            component.m_onClickEventID = enUIEventID.Room_ChangePos;
            component.m_onClickEventParams.tag = 1;
            component = obj4.GetComponent<CUIEventScript>();
            component.m_onClickEventID = enUIEventID.Room_AddRobot;
            component.m_onClickEventParams.tag = 2;
            component = obj5.GetComponent<CUIEventScript>();
            component.m_onClickEventID = enUIEventID.Room_ChangePos;
            component.m_onClickEventParams.tag = 2;
        }
    }
}

