namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    [MessageHandlerClass]
    internal class CRoomSystem : Singleton<CRoomSystem>
    {
        private bool bInRoom;
        private uint mapId;
        private ListView<ResAcntBattleLevelInfo> mapList;
        private static ulong NpcUlId = 1L;
        public static string PATH_CREATE_ROOM = "UGUI/Form/System/PvP/Room/Form_CreateRoom.prefab";
        public static string PATH_ROOM = "UGUI/Form/System/PvP/Room/Form_Room.prefab";
        public Assets.Scripts.GameSystem.RoomInfo roomInfo;

        public void BuildRoomInfo(COMDT_JOINMULTGAMERSP_SUCC roomData)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                this.roomInfo = new Assets.Scripts.GameSystem.RoomInfo();
                this.roomInfo.iRoomEntity = roomData.iRoomEntity;
                this.roomInfo.dwRoomID = roomData.dwRoomID;
                this.roomInfo.dwRoomSeq = roomData.dwRoomSeq;
                this.roomInfo.roomAttrib.bGameMode = roomData.stRoomInfo.bGameMode;
                this.roomInfo.roomAttrib.bPkAI = 0;
                this.roomInfo.roomAttrib.bMapType = roomData.stRoomInfo.bMapType;
                this.roomInfo.roomAttrib.dwMapId = roomData.stRoomInfo.dwMapId;
                this.roomInfo.roomAttrib.bWarmBattle = false;
                this.roomInfo.roomAttrib.npcAILevel = 2;
                this.roomInfo.selfInfo.ullUid = roomData.ullSelfUid;
                this.roomInfo.selfInfo.iGameEntity = roomData.iSelfGameEntity;
                this.roomInfo.roomOwner.ullUid = roomData.stRoomMaster.ullMasterUid;
                this.roomInfo.roomOwner.iGameEntity = roomData.stRoomMaster.iMasterGameEntity;
                for (int i = 0; i < 2; i++)
                {
                    COM_PLAYERCAMP camp = (COM_PLAYERCAMP) (i + 1);
                    this.roomInfo.CampMemberList[i].Clear();
                    for (int j = 0; j < roomData.stMemInfo.astCampMem[i].dwMemNum; j++)
                    {
                        COMDT_ROOMMEMBER_DT memberDT = roomData.stMemInfo.astCampMem[i].astMemInfo[j];
                        MemberInfo item = this.CreateMemInfo(ref memberDT, camp, this.roomInfo.roomAttrib.bWarmBattle);
                        if (item.ullUid == masterRoleInfo.playerUllUID)
                        {
                            this.roomInfo.selfObjID = item.dwObjId;
                        }
                        this.roomInfo.CampMemberList[i].Add(item);
                    }
                    this.SortCampMemList(ref this.roomInfo.CampMemberList[i]);
                }
            }
        }

        public void BuildRoomInfo(COMDT_MATCH_SUCC_DETAIL roomData)
        {
            this.roomInfo = new Assets.Scripts.GameSystem.RoomInfo();
            this.roomInfo.iRoomEntity = roomData.iRoomEntity;
            this.roomInfo.dwRoomID = roomData.dwRoomID;
            this.roomInfo.dwRoomSeq = roomData.dwRoomSeq;
            this.roomInfo.roomAttrib.bGameMode = roomData.stRoomInfo.bGameMode;
            this.roomInfo.roomAttrib.bPkAI = roomData.stRoomInfo.bPkAI;
            this.roomInfo.roomAttrib.bMapType = roomData.stRoomInfo.bMapType;
            this.roomInfo.roomAttrib.dwMapId = roomData.stRoomInfo.dwMapId;
            this.roomInfo.roomAttrib.bWarmBattle = Convert.ToBoolean(roomData.stRoomInfo.bIsWarmBattle);
            this.roomInfo.roomAttrib.npcAILevel = roomData.stRoomInfo.bAILevel;
            this.roomInfo.selfInfo.ullUid = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().playerUllUID;
            for (int i = 0; i < 2; i++)
            {
                COM_PLAYERCAMP camp = (COM_PLAYERCAMP) (i + 1);
                this.roomInfo.CampMemberList[i].Clear();
                for (int j = 0; j < roomData.stMemInfo.astCampMem[i].dwMemNum; j++)
                {
                    COMDT_ROOMMEMBER_DT memberDT = roomData.stMemInfo.astCampMem[i].astMemInfo[j];
                    MemberInfo item = this.CreateMemInfo(ref memberDT, camp, this.roomInfo.roomAttrib.bWarmBattle);
                    this.roomInfo.CampMemberList[i].Add(item);
                }
                this.SortCampMemList(ref this.roomInfo.CampMemberList[i]);
            }
        }

        public void Clear()
        {
            this.bInRoom = false;
            this.roomInfo = null;
        }

        public void CloseRoom()
        {
            Singleton<CUIManager>.GetInstance().CloseForm(PATH_CREATE_ROOM);
            Singleton<CUIManager>.GetInstance().CloseForm(PATH_ROOM);
            Singleton<CTopLobbyEntry>.GetInstance().CloseForm();
            Singleton<CInviteSystem>.GetInstance().CloseInviteForm();
            Singleton<CChatController>.instance.model.channelMgr.Clear(EChatChannel.Room, 0L, 0);
            Singleton<CChatController>.instance.model.channelMgr.SetChatTab(CChatChannelMgr.EChatTab.Normal);
            Singleton<CChatController>.instance.view.UpView(false);
            Singleton<CChatController>.instance.model.sysData.ClearEntryText();
            this.bInRoom = false;
        }

        private MemberInfo CreateMemInfo(ref COMDT_ROOMMEMBER_DT memberDT, COM_PLAYERCAMP camp, bool bWarmBattle)
        {
            MemberInfo info = new MemberInfo {
                RoomMemberType = memberDT.dwRoomMemberType,
                dwPosOfCamp = memberDT.dwPosOfCamp,
                camp = camp
            };
            if (memberDT.dwRoomMemberType == 1)
            {
                info.ullUid = memberDT.stMemberDetail.stMemberOfAcnt.ullUid;
                info.iFromGameEntity = memberDT.stMemberDetail.stMemberOfAcnt.iFromGameEntity;
                info.iLogicWorldID = memberDT.stMemberDetail.stMemberOfAcnt.iLogicWorldID;
                info.MemberName = StringHelper.UTF8BytesToString(ref memberDT.stMemberDetail.stMemberOfAcnt.szMemberName);
                info.dwMemberLevel = memberDT.stMemberDetail.stMemberOfAcnt.dwMemberLevel;
                info.dwMemberPvpLevel = memberDT.stMemberDetail.stMemberOfAcnt.dwMemberPvpLevel;
                info.dwMemberHeadId = memberDT.stMemberDetail.stMemberOfAcnt.dwMemberHeadId;
                info.MemberHeadUrl = StringHelper.UTF8BytesToString(ref memberDT.stMemberDetail.stMemberOfAcnt.szMemberHeadUrl);
                info.ChoiceHero = new COMDT_CHOICEHERO[] { new COMDT_CHOICEHERO() };
                info.isPrepare = false;
                info.dwObjId = 0;
                return info;
            }
            if (memberDT.dwRoomMemberType == 2)
            {
                NpcUlId += (ulong) 1L;
                info.ullUid = NpcUlId;
                info.iFromGameEntity = 0;
                info.MemberName = Singleton<CTextManager>.GetInstance().GetText("PVP_NPC");
                info.dwMemberLevel = memberDT.stMemberDetail.stMemberOfNpc.bLevel;
                info.dwMemberHeadId = 1;
                info.ChoiceHero = new COMDT_CHOICEHERO[] { new COMDT_CHOICEHERO() };
                info.isPrepare = true;
                info.dwObjId = 0;
                info.WarmNpc = memberDT.stMemberDetail.stMemberOfNpc.stDetail;
                if (bWarmBattle)
                {
                    info.ullUid = info.WarmNpc.ullUid;
                    info.dwMemberPvpLevel = info.WarmNpc.dwAcntPvpLevel;
                    info.MemberName = StringHelper.UTF8BytesToString(ref info.WarmNpc.szUserName);
                    info.MemberHeadUrl = StringHelper.UTF8BytesToString(ref info.WarmNpc.szUserHeadUrl);
                    info.isPrepare = false;
                }
            }
            return info;
        }

        private void entertainmentAddLock(CUIFormScript form)
        {
            Transform transform = form.transform.FindChild("Panel_Main/List").GetComponent<CUIListScript>().GetElemenet(3).transform;
            if (transform != null)
            {
                if (!Singleton<CFunctionUnlockSys>.instance.FucIsUnlock(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_ENTERTAINMENT))
                {
                    transform.GetComponent<Image>().color = CUIUtility.s_Color_Button_Disable;
                    transform.FindChild("Lock").gameObject.CustomSetActive(true);
                    ResSpecialFucUnlock dataByKey = GameDataMgr.specialFunUnlockDatabin.GetDataByKey((uint) 0x19);
                    transform.FindChild("Lock/Text").GetComponent<Text>().text = Utility.UTF8Convert(dataByKey.szLockedTip);
                }
                else
                {
                    transform.GetComponent<Image>().color = CUIUtility.s_Color_White;
                    transform.FindChild("Lock").gameObject.CustomSetActive(false);
                }
            }
        }

        private void GetMemberPosInfo(GameObject go, out COM_PLAYERCAMP Camp, out int Pos)
        {
            Camp = COM_PLAYERCAMP.COM_PLAYERCAMP_1;
            if (go.name.StartsWith("Left"))
            {
                Camp = COM_PLAYERCAMP.COM_PLAYERCAMP_1;
            }
            else if (go.name.StartsWith("Right"))
            {
                Camp = COM_PLAYERCAMP.COM_PLAYERCAMP_2;
            }
            Pos = 0;
            if (go.name.EndsWith("1"))
            {
                Pos = 0;
            }
            else if (go.name.EndsWith("2"))
            {
                Pos = 1;
            }
            else if (go.name.EndsWith("3"))
            {
                Pos = 2;
            }
            else if (go.name.EndsWith("4"))
            {
                Pos = 3;
            }
            else if (go.name.EndsWith("5"))
            {
                Pos = 4;
            }
        }

        public override void Init()
        {
            base.Init();
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Room_OpenCreateForm, new CUIEventManager.OnUIEventHandler(this.OnRoom_OpenCreateForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Room_CreateRoom, new CUIEventManager.OnUIEventHandler(this.OnRoom_CreateRoom));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Room_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnRoom_CloseForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Room_SelectMap, new CUIEventManager.OnUIEventHandler(this.OnRoom_SelectMap));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Room_OpenInvite, new CUIEventManager.OnUIEventHandler(this.OnRoom_OpenInvite));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Room_StartGame, new CUIEventManager.OnUIEventHandler(this.OnRoom_StartGame));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Room_AddRobot, new CUIEventManager.OnUIEventHandler(this.OnRoom_AddRobot));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Room_ChangePos, new CUIEventManager.OnUIEventHandler(this.OnRoom_ChangePos));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Room_KickPlayer, new CUIEventManager.OnUIEventHandler(this.OnRoom_KickPlayer));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Room_LeaveRoom, new CUIEventManager.OnUIEventHandler(this.OnRoom_LeaveRoom));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Room_AddFriend, new CUIEventManager.OnUIEventHandler(this.OnRoom_AddFriend));
        }

        private void InitMaps(CUIFormScript rootFormScript)
        {
            this.mapList = new ListView<ResAcntBattleLevelInfo>();
            uint[] numArray = new uint[10];
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Room_1"), out numArray[0]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Room_2"), out numArray[1]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Room_3"), out numArray[2]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Room_4"), out numArray[3]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Room_5"), out numArray[4]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Room_6"), out numArray[5]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Room_7"), out numArray[6]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Room_8"), out numArray[7]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Room_9"), out numArray[8]);
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Room_10"), out numArray[9]);
            for (int i = 0; i < numArray.Length; i++)
            {
                if (numArray[i] != 0)
                {
                    ResAcntBattleLevelInfo dataByKey = GameDataMgr.pvpLevelDatabin.GetDataByKey(numArray[i]);
                    if (dataByKey != null)
                    {
                        this.mapList.Add(dataByKey);
                    }
                }
            }
            CUIListScript component = rootFormScript.transform.Find("Panel_Main/List").gameObject.GetComponent<CUIListScript>();
            component.SetElementAmount(this.mapList.Count);
            for (int j = 0; j < component.m_elementAmount; j++)
            {
                Image image = component.GetElemenet(j).transform.GetComponent<Image>();
                string prefabPath = CUIUtility.s_Sprite_Dynamic_PvpEntry_Dir + this.mapList[j].dwMapId;
                image.SetSprite(prefabPath, rootFormScript, true, false, false);
            }
            component.SelectElement(-1, true);
            if (CSysDynamicBlock.bLobbyEntryBlocked)
            {
                Transform transform = rootFormScript.transform.Find("panelGroup5");
                if (transform != null)
                {
                    transform.gameObject.CustomSetActive(false);
                }
            }
        }

        private void OnFriendChange(CSPkg msg)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(PATH_ROOM);
            if (form != null)
            {
                CRoomView.SetRoomData(form.gameObject, this.roomInfo);
            }
        }

        private void OnFriendOnlineChg()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(PATH_ROOM);
            if (form != null)
            {
                CRoomView.SetRoomData(form.gameObject, this.roomInfo);
            }
        }

        [MessageHandler(0x400)]
        public static void OnLeaveRoom(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            if (msg.stPkgData.stQuitMultGameRsp.iErrCode == 0)
            {
                if (msg.stPkgData.stQuitMultGameRsp.bLevelFromType == 1)
                {
                    Singleton<CRoomSystem>.GetInstance().bInRoom = false;
                    Singleton<CUIManager>.GetInstance().CloseForm(PATH_ROOM);
                    Singleton<CTopLobbyEntry>.GetInstance().CloseForm();
                    Singleton<CInviteSystem>.GetInstance().CloseInviteForm();
                    Singleton<CChatController>.instance.model.channelMgr.Clear(EChatChannel.Room, 0L, 0);
                    Singleton<CChatController>.instance.model.channelMgr.SetChatTab(CChatChannelMgr.EChatTab.Normal);
                    Singleton<CChatController>.instance.view.UpView(false);
                    Singleton<CChatController>.instance.model.sysData.ClearEntryText();
                }
                else if (msg.stPkgData.stQuitMultGameRsp.bLevelFromType == 2)
                {
                    CMatchingSystem.OnPlayerLeaveMatching();
                }
            }
            else
            {
                object[] replaceArr = new object[] { Utility.ProtErrCodeToStr(0x400, msg.stPkgData.stQuitMultGameRsp.iErrCode) };
                Singleton<CUIManager>.GetInstance().OpenTips("PVP_Exit_Room_Error", true, 1f, null, replaceArr);
            }
            Singleton<CMatchingSystem>.instance.bPlayerActive = true;
        }

        [MessageHandler(0x3fe)]
        public static void OnPlayerJoinRoom(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            if (msg.stPkgData.stJoinMultGameRsp.iErrCode == 0)
            {
                Singleton<GameBuilder>.instance.EndGame();
                CRoomSystem instance = Singleton<CRoomSystem>.GetInstance();
                instance.bInRoom = true;
                instance.BuildRoomInfo(msg.stPkgData.stJoinMultGameRsp.stInfo.stOfSucc);
                CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(PATH_ROOM, false, true);
                Singleton<CTopLobbyEntry>.GetInstance().OpenForm();
                Singleton<CInviteSystem>.GetInstance().OpenInviteForm(COM_INVITE_JOIN_TYPE.COM_INVITE_JOIN_ROOM);
                Singleton<CChatController>.instance.model.channelMgr.Clear(EChatChannel.Room, 0L, 0);
                Singleton<CChatController>.instance.model.channelMgr.SetChatTab(CChatChannelMgr.EChatTab.Room);
                Singleton<CChatController>.instance.ShowPanel(true, false);
                Singleton<CChatController>.instance.view.UpView(true);
                Singleton<CChatController>.instance.model.sysData.ClearEntryText();
                CRoomView.SetRoomData(script.gameObject, instance.roomInfo);
                Singleton<CMatchingSystem>.instance.cacheMathingInfo.CanGameAgain = instance.IsSelfRoomOwner;
                if (!instance.IsSelfRoomOwner)
                {
                    MonoSingleton<NewbieGuideManager>.instance.StopCurrentGuide();
                }
            }
            else if (msg.stPkgData.stJoinMultGameRsp.iErrCode == 14)
            {
                DateTime banTime = MonoSingleton<IDIPSys>.GetInstance().GetBanTime(COM_ACNT_BANTIME_TYPE.COM_ACNT_BANTIME_BANPLAYPVP);
                object[] args = new object[] { banTime.Year, banTime.Month, banTime.Day, banTime.Hour, banTime.Minute };
                string strContent = string.Format("您被禁止竞技！截止时间为{0}年{1}月{2}日{3}时{4}分", args);
                Singleton<CUIManager>.GetInstance().OpenMessageBox(strContent, false);
            }
            else
            {
                object[] replaceArr = new object[] { Utility.ProtErrCodeToStr(0x3fe, msg.stPkgData.stJoinMultGameRsp.iErrCode) };
                Singleton<CUIManager>.GetInstance().OpenTips("PVP_Enter_Room_Error", true, 1f, null, replaceArr);
            }
        }

        public static void OnPlayerPosChanged(CSPkg msg)
        {
        }

        private void OnRoom_AddFriend(CUIEvent uiEvent)
        {
            GameObject gameObject = uiEvent.m_srcWidget.transform.parent.parent.gameObject;
            COM_PLAYERCAMP camp = COM_PLAYERCAMP.COM_PLAYERCAMP_1;
            int pos = 0;
            this.GetMemberPosInfo(gameObject, out camp, out pos);
            if (this.roomInfo != null)
            {
                MemberInfo memberInfo = this.roomInfo.GetMemberInfo(camp, pos);
                object[] inParameters = new object[] { camp, pos };
                DebugHelper.Assert(memberInfo != null, "Room member info is NULL!! Camp -- {0}, Pos -- {1}", inParameters);
                if (memberInfo != null)
                {
                    FriendSysNetCore.Send_Request_BeFriend(memberInfo.ullUid, (uint) memberInfo.iFromGameEntity);
                }
            }
        }

        private void OnRoom_AddRobot(CUIEvent uiEvent)
        {
            if (this.IsSelfRoomOwner)
            {
                COM_PLAYERCAMP camp = COM_PLAYERCAMP.COM_PLAYERCAMP_2;
                if (uiEvent.m_eventParams.tag == 1)
                {
                    camp = COM_PLAYERCAMP.COM_PLAYERCAMP_1;
                }
                else if (uiEvent.m_eventParams.tag == 2)
                {
                    camp = COM_PLAYERCAMP.COM_PLAYERCAMP_2;
                }
                ReqAddRobot(camp);
            }
            else
            {
                DebugHelper.Assert(false);
            }
        }

        private void OnRoom_ChangePos(CUIEvent uiEvent)
        {
            COM_PLAYERCAMP camp = COM_PLAYERCAMP.COM_PLAYERCAMP_1;
            if (uiEvent.m_eventParams.tag == 1)
            {
                camp = COM_PLAYERCAMP.COM_PLAYERCAMP_1;
            }
            else if (uiEvent.m_eventParams.tag == 2)
            {
                camp = COM_PLAYERCAMP.COM_PLAYERCAMP_2;
            }
            if (this.roomInfo != null)
            {
                uint dwMapId = this.roomInfo.roomAttrib.dwMapId;
                ResAcntBattleLevelInfo dataByKey = GameDataMgr.pvpLevelDatabin.GetDataByKey(dwMapId);
                DebugHelper.Assert(dataByKey != null);
                int bMaxAcntNum = dataByKey.stLevelCommonInfo.bMaxAcntNum;
                int freePos = this.roomInfo.GetFreePos(camp, bMaxAcntNum);
                if (freePos >= 0)
                {
                    ReqChangeCamp(camp, freePos);
                }
            }
        }

        private void OnRoom_CloseForm(CUIEvent uiEvent)
        {
            ReqLeaveRoom();
        }

        private void OnRoom_CreateRoom(CUIEvent uiEvent)
        {
            if (this.mapId > 0)
            {
                Singleton<CMatchingSystem>.instance.cacheMathingInfo.uiEventId = uiEvent.m_eventID;
                Singleton<CMatchingSystem>.instance.cacheMathingInfo.mapId = this.mapId;
                ReqCreateRoom(this.mapId);
            }
        }

        private void OnRoom_KickPlayer(CUIEvent uiEvent)
        {
            if (this.IsSelfRoomOwner)
            {
                GameObject gameObject = uiEvent.m_srcWidget.transform.parent.parent.gameObject;
                COM_PLAYERCAMP camp = COM_PLAYERCAMP.COM_PLAYERCAMP_1;
                int pos = 0;
                this.GetMemberPosInfo(gameObject, out camp, out pos);
                ReqKickPlayer(camp, pos);
            }
            else
            {
                DebugHelper.Assert(false, "Not Room Owner!");
            }
        }

        private void OnRoom_LeaveRoom(CUIEvent uiEvent)
        {
            ReqLeaveRoom();
        }

        private void OnRoom_OpenCreateForm(CUIEvent uiEvent)
        {
            CUIFormScript rootFormScript = Singleton<CUIManager>.GetInstance().OpenForm(PATH_CREATE_ROOM, false, true);
            this.InitMaps(rootFormScript);
            this.ShowBonusImage(rootFormScript);
            this.entertainmentAddLock(rootFormScript);
        }

        private void OnRoom_OpenInvite(CUIEvent uiEvent)
        {
            CUIEvent event2 = new CUIEvent {
                m_eventID = enUIEventID.Invite_OpenForm
            };
            event2.m_eventParams.tag = 1;
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(event2);
        }

        private void OnRoom_SelectMap(CUIEvent uiEvent)
        {
            int selectedIndex = uiEvent.m_srcWidget.GetComponent<CUIListScript>().GetSelectedIndex();
            if ((selectedIndex >= 0) && (selectedIndex < this.mapList.Count))
            {
                ResAcntBattleLevelInfo info = this.mapList[selectedIndex];
                this.mapId = info.dwMapId;
                if ((selectedIndex == 3) && !Singleton<CFunctionUnlockSys>.instance.FucIsUnlock(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_ENTERTAINMENT))
                {
                    ResSpecialFucUnlock dataByKey = GameDataMgr.specialFunUnlockDatabin.GetDataByKey((uint) 0x19);
                    Singleton<CUIManager>.GetInstance().OpenTips(dataByKey.szLockedTip, false, 1f, null, new object[0]);
                }
                else
                {
                    Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Room_CreateRoom);
                }
            }
        }

        private void OnRoom_StartGame(CUIEvent uiEvent)
        {
            if (this.IsSelfRoomOwner)
            {
                if (uiEvent.m_srcWidget.GetComponent<Button>().interactable)
                {
                    ReqStartGame();
                }
            }
            else
            {
                DebugHelper.Assert(false);
            }
        }

        [MessageHandler(0x401)]
        public static void OnRoomChange(CSPkg msg)
        {
            COMDT_ROOMCHG_CHGMEMBERPOS stChgMemberPos;
            MemberInfo info3;
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            Assets.Scripts.GameSystem.RoomInfo roomInfo = Singleton<CRoomSystem>.GetInstance().roomInfo;
            if (roomInfo == null)
            {
                DebugHelper.Assert(false, "Room Info is NULL!!!");
                return;
            }
            bool flag = false;
            if (msg.stPkgData.stRoomChgNtf.stRoomChgInfo.iChgType == 0)
            {
                int index = msg.stPkgData.stRoomChgNtf.stRoomChgInfo.stChgInfo.stPlayerAdd.iCamp - 1;
                MemberInfo item = Singleton<CRoomSystem>.GetInstance().CreateMemInfo(ref msg.stPkgData.stRoomChgNtf.stRoomChgInfo.stChgInfo.stPlayerAdd.stMemInfo, (COM_PLAYERCAMP) msg.stPkgData.stRoomChgNtf.stRoomChgInfo.stChgInfo.stPlayerAdd.iCamp, roomInfo.roomAttrib.bWarmBattle);
                roomInfo.CampMemberList[index].Add(item);
                flag = true;
                goto Label_05C2;
            }
            if (msg.stPkgData.stRoomChgNtf.stRoomChgInfo.iChgType != 1)
            {
                if (msg.stPkgData.stRoomChgNtf.stRoomChgInfo.iChgType == 2)
                {
                    Singleton<CRoomSystem>.GetInstance().bInRoom = false;
                    Singleton<CUIManager>.GetInstance().CloseForm(PATH_CREATE_ROOM);
                    Singleton<CUIManager>.GetInstance().CloseForm(PATH_ROOM);
                    Singleton<CTopLobbyEntry>.GetInstance().CloseForm();
                    Singleton<CInviteSystem>.GetInstance().CloseInviteForm();
                    Singleton<CChatController>.instance.model.channelMgr.Clear(EChatChannel.Room, 0L, 0);
                    Singleton<CChatController>.instance.model.channelMgr.SetChatTab(CChatChannelMgr.EChatTab.Normal);
                    Singleton<CChatController>.instance.view.UpView(false);
                    Singleton<CChatController>.instance.model.sysData.ClearEntryText();
                    Singleton<CUIManager>.GetInstance().OpenTips("PVP_Room_Kick_Tip", true, 1f, null, new object[0]);
                    goto Label_05C2;
                }
                if (msg.stPkgData.stRoomChgNtf.stRoomChgInfo.iChgType == 4)
                {
                    roomInfo.roomOwner.ullUid = msg.stPkgData.stRoomChgNtf.stRoomChgInfo.stChgInfo.stMasterChg.stNewMaster.ullMasterUid;
                    roomInfo.roomOwner.iGameEntity = msg.stPkgData.stRoomChgNtf.stRoomChgInfo.stChgInfo.stMasterChg.stNewMaster.iMasterGameEntity;
                    flag = true;
                    goto Label_05C2;
                }
                if (msg.stPkgData.stRoomChgNtf.stRoomChgInfo.iChgType != 5)
                {
                    if (msg.stPkgData.stRoomChgNtf.stRoomChgInfo.iChgType == 3)
                    {
                        enRoomState bOldState = (enRoomState) msg.stPkgData.stRoomChgNtf.stRoomChgInfo.stChgInfo.stStateChg.bOldState;
                        enRoomState bNewState = (enRoomState) msg.stPkgData.stRoomChgNtf.stRoomChgInfo.stChgInfo.stStateChg.bNewState;
                        if ((bOldState == enRoomState.E_ROOM_PREPARE) && (bNewState == enRoomState.E_ROOM_WAIT))
                        {
                            Singleton<LobbyLogic>.GetInstance().inMultiRoom = false;
                            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.HeroSelect_CloseForm);
                            Singleton<CUIManager>.GetInstance().OpenForm(PATH_ROOM, false, true);
                            Singleton<CChatController>.instance.model.channelMgr.Clear(EChatChannel.Room, 0L, 0);
                            Singleton<CChatController>.instance.model.channelMgr.SetChatTab(CChatChannelMgr.EChatTab.Room);
                            Singleton<CChatController>.instance.ShowPanel(true, false);
                            Singleton<CChatController>.instance.view.UpView(true);
                            Singleton<CChatController>.instance.model.sysData.ClearEntryText();
                        }
                        if ((bOldState == enRoomState.E_ROOM_WAIT) && (bNewState == enRoomState.E_ROOM_CONFIRM))
                        {
                            CUIEvent uiEvent = new CUIEvent {
                                m_eventID = enUIEventID.Matching_OpenConfirmBox
                            };
                            uiEvent.m_eventParams.tag = roomInfo.roomAttrib.bPkAI;
                            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(uiEvent);
                            if (roomInfo.roomAttrib.bWarmBattle)
                            {
                                CFakePvPHelper.SetConfirmFakeData();
                            }
                        }
                    }
                    goto Label_05C2;
                }
                stChgMemberPos = msg.stPkgData.stRoomChgNtf.stRoomChgInfo.stChgInfo.stChgMemberPos;
                if (stChgMemberPos.bNewCamp == stChgMemberPos.bOldCamp)
                {
                    for (int j = 0; j < roomInfo.CampMemberList[stChgMemberPos.bOldCamp - 1].Count; j++)
                    {
                        if (roomInfo.CampMemberList[stChgMemberPos.bOldCamp - 1][j].ullUid == stChgMemberPos.ullMemberUid)
                        {
                            roomInfo.CampMemberList[stChgMemberPos.bOldCamp - 1][j].dwPosOfCamp = stChgMemberPos.bNewPosOfCamp;
                            break;
                        }
                    }
                    goto Label_046C;
                }
                info3 = null;
                for (int i = 0; i < roomInfo.CampMemberList[stChgMemberPos.bOldCamp - 1].Count; i++)
                {
                    if (roomInfo.CampMemberList[stChgMemberPos.bOldCamp - 1][i].ullUid == stChgMemberPos.ullMemberUid)
                    {
                        info3 = roomInfo.CampMemberList[stChgMemberPos.bOldCamp - 1][i];
                        roomInfo.CampMemberList[stChgMemberPos.bOldCamp - 1].RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                int num2 = msg.stPkgData.stRoomChgNtf.stRoomChgInfo.stChgInfo.stPlayerLeave.iCamp - 1;
                int bPos = msg.stPkgData.stRoomChgNtf.stRoomChgInfo.stChgInfo.stPlayerLeave.bPos;
                for (int k = 0; k < roomInfo.CampMemberList[num2].Count; k++)
                {
                    if (roomInfo.CampMemberList[num2][k].dwPosOfCamp == bPos)
                    {
                        roomInfo.CampMemberList[num2].RemoveAt(k);
                        break;
                    }
                }
                flag = true;
                goto Label_05C2;
            }
            DebugHelper.Assert(info3 != null, "oldMemberInfo is NULL!!");
            info3.camp = (COM_PLAYERCAMP) stChgMemberPos.bNewCamp;
            info3.dwPosOfCamp = stChgMemberPos.bNewPosOfCamp;
            roomInfo.CampMemberList[stChgMemberPos.bNewCamp - 1].Add(info3);
        Label_046C:
            flag = true;
        Label_05C2:
            if (flag)
            {
                CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(PATH_ROOM);
                if (form != null)
                {
                    CRoomView.SetRoomData(form.gameObject, roomInfo);
                }
            }
        }

        [MessageHandler(0x7de)]
        public static void OnRoomStarted(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            if (msg.stPkgData.stStartMultiGameRsp.bErrcode == 0)
            {
                Singleton<CHeroSelectSystem>.GetInstance().SetPvpDataByRoomInfo(Singleton<CRoomSystem>.GetInstance().roomInfo, 2);
            }
            else
            {
                object[] replaceArr = new object[] { Utility.ProtErrCodeToStr(0x7de, msg.stPkgData.stStartMultiGameRsp.bErrcode) };
                Singleton<CUIManager>.GetInstance().OpenTips("PVP_Start_Game_Error", true, 1f, null, replaceArr);
            }
        }

        public static void ReqAddRobot(COM_PLAYERCAMP Camp)
        {
            Debug.Log("ReqAddRobot");
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x7df);
            msg.stPkgData.stAddNpcReq.stNpcInfo.iCamp = (int) Camp;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void ReqChangeCamp(COM_PLAYERCAMP Camp, int Pos)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x7f1);
            msg.stPkgData.stChgMemberPosReq.bCamp = (byte) Camp;
            msg.stPkgData.stChgMemberPosReq.bPosOfCamp = (byte) Pos;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void ReqCreateRoom(uint MapId)
        {
            Debug.Log("ReqCreateRoom");
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x3fc);
            msg.stPkgData.stCreateMultGameReq.dwMapId = MapId;
            StringHelper.StringToUTF8Bytes("testRoom", ref msg.stPkgData.stCreateMultGameReq.szRoomName);
            msg.stPkgData.stCreateMultGameReq.bGameMode = 1;
            msg.stPkgData.stCreateMultGameReq.bMapType = 1;
            ResAcntBattleLevelInfo dataByKey = GameDataMgr.pvpLevelDatabin.GetDataByKey(MapId);
            if ((dataByKey != null) && (dataByKey.stLevelCommonInfo.bChaosPickRule > 0))
            {
                msg.stPkgData.stCreateMultGameReq.bMapType = 4;
            }
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void ReqKickPlayer(COM_PLAYERCAMP Camp, int Pos)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x7e3);
            msg.stPkgData.stKickoutRoomMemberReq.stKickMemberInfo.iCamp = (int) Camp;
            msg.stPkgData.stKickoutRoomMemberReq.stKickMemberInfo.bPosOfCamp = (byte) Pos;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void ReqLeaveRoom()
        {
            Debug.Log("ReqLeaveRoom");
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x3ff);
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void ReqStartGame()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x7dd);
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        private void ShowBonusImage(CUIFormScript form)
        {
            if (form != null)
            {
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
                GameObject gameObject = form.transform.FindChild("panelGroup5/ButtonTrain/ImageBonus").gameObject;
                if ((masterRoleInfo != null) && masterRoleInfo.IsTrainingLevelFin())
                {
                    gameObject.CustomSetActive(false);
                }
                else
                {
                    gameObject.CustomSetActive(true);
                }
            }
        }

        private void SortCampMemList(ref ListView<MemberInfo> inMemList)
        {
            SortedList<uint, MemberInfo> list = new SortedList<uint, MemberInfo>();
            ListView<MemberInfo>.Enumerator enumerator = inMemList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                MemberInfo current = enumerator.Current;
                uint dwPosOfCamp = current.dwPosOfCamp;
                list.Add(dwPosOfCamp, current);
            }
            inMemList = new ListView<MemberInfo>(list.Values);
        }

        public override void UnInit()
        {
            this.roomInfo = null;
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Room_OpenCreateForm, new CUIEventManager.OnUIEventHandler(this.OnRoom_OpenCreateForm));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Room_CreateRoom, new CUIEventManager.OnUIEventHandler(this.OnRoom_CreateRoom));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Room_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnRoom_CloseForm));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Room_SelectMap, new CUIEventManager.OnUIEventHandler(this.OnRoom_SelectMap));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Room_OpenInvite, new CUIEventManager.OnUIEventHandler(this.OnRoom_OpenInvite));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Room_StartGame, new CUIEventManager.OnUIEventHandler(this.OnRoom_StartGame));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Room_AddRobot, new CUIEventManager.OnUIEventHandler(this.OnRoom_AddRobot));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Room_ChangePos, new CUIEventManager.OnUIEventHandler(this.OnRoom_ChangePos));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Room_KickPlayer, new CUIEventManager.OnUIEventHandler(this.OnRoom_KickPlayer));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Room_LeaveRoom, new CUIEventManager.OnUIEventHandler(this.OnRoom_LeaveRoom));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Room_AddFriend, new CUIEventManager.OnUIEventHandler(this.OnRoom_AddFriend));
            base.UnInit();
        }

        public void UpdateRoomInfo(COMDT_DESKINFO inDeskInfo, CSDT_CAMPINFO[] inCampInfo)
        {
            uint dwObjId = 0;
            if (this.roomInfo == null)
            {
                this.roomInfo = new Assets.Scripts.GameSystem.RoomInfo();
                this.roomInfo.roomAttrib.bGameMode = inDeskInfo.bGameMode;
                this.roomInfo.roomAttrib.bPkAI = 0;
                this.roomInfo.roomAttrib.bMapType = inDeskInfo.bMapType;
                this.roomInfo.roomAttrib.dwMapId = inDeskInfo.dwMapId;
                this.roomInfo.roomAttrib.bWarmBattle = Convert.ToBoolean(inDeskInfo.bIsWarmBattle);
                this.roomInfo.roomAttrib.npcAILevel = inDeskInfo.bAILevel;
                for (int i = 0; i < inCampInfo.Length; i++)
                {
                    COM_PLAYERCAMP com_playercamp = (COM_PLAYERCAMP) (i + 1);
                    CSDT_CAMPINFO csdt_campinfo = inCampInfo[i];
                    this.roomInfo.CampMemberList[i].Clear();
                    for (int j = 0; j < csdt_campinfo.dwPlayerNum; j++)
                    {
                        MemberInfo item = new MemberInfo();
                        COMDT_PLAYERINFO stPlayerInfo = csdt_campinfo.astCampPlayerInfo[j].stPlayerInfo;
                        item.RoomMemberType = stPlayerInfo.bObjType;
                        item.dwPosOfCamp = stPlayerInfo.bPosOfCamp;
                        item.camp = com_playercamp;
                        item.dwMemberLevel = stPlayerInfo.dwLevel;
                        if (item.RoomMemberType == 1)
                        {
                            item.dwMemberPvpLevel = stPlayerInfo.stDetail.stPlayerOfAcnt.dwPvpLevel;
                        }
                        item.dwObjId = stPlayerInfo.dwObjId;
                        item.MemberName = StringHelper.UTF8BytesToString(ref stPlayerInfo.szName);
                        item.ChoiceHero = stPlayerInfo.astChoiceHero;
                        if (stPlayerInfo.bObjType == 1)
                        {
                            item.dwMemberHeadId = stPlayerInfo.stDetail.stPlayerOfAcnt.dwHeadId;
                            if (stPlayerInfo.stDetail.stPlayerOfAcnt.ullUid == Singleton<CRoleInfoManager>.instance.masterUUID)
                            {
                                dwObjId = stPlayerInfo.dwObjId;
                                Singleton<CHeroSelectSystem>.instance.ResetRandHeroLeftCount((int) csdt_campinfo.astCampPlayerInfo[j].dwRandomHeroCnt);
                            }
                        }
                        else if (stPlayerInfo.bObjType == 2)
                        {
                            item.dwMemberHeadId = 1;
                        }
                        this.roomInfo.CampMemberList[i].Add(item);
                    }
                    this.SortCampMemList(ref this.roomInfo.CampMemberList[i]);
                }
            }
            else
            {
                this.roomInfo.roomAttrib.bGameMode = inDeskInfo.bGameMode;
                this.roomInfo.roomAttrib.bPkAI = 0;
                this.roomInfo.roomAttrib.bMapType = inDeskInfo.bMapType;
                this.roomInfo.roomAttrib.dwMapId = inDeskInfo.dwMapId;
                for (int k = 0; k < inCampInfo.Length; k++)
                {
                    COM_PLAYERCAMP camp = (COM_PLAYERCAMP) (k + 1);
                    CSDT_CAMPINFO csdt_campinfo2 = inCampInfo[k];
                    for (int m = 0; m < csdt_campinfo2.dwPlayerNum; m++)
                    {
                        COMDT_PLAYERINFO comdt_playerinfo2 = csdt_campinfo2.astCampPlayerInfo[m].stPlayerInfo;
                        MemberInfo memberInfo = this.roomInfo.GetMemberInfo(camp, comdt_playerinfo2.bPosOfCamp);
                        if (memberInfo != null)
                        {
                            memberInfo.dwObjId = comdt_playerinfo2.dwObjId;
                            memberInfo.camp = camp;
                            memberInfo.ChoiceHero = comdt_playerinfo2.astChoiceHero;
                            if ((comdt_playerinfo2.bObjType == 1) && (comdt_playerinfo2.stDetail.stPlayerOfAcnt.ullUid == Singleton<CRoleInfoManager>.instance.masterUUID))
                            {
                                dwObjId = comdt_playerinfo2.dwObjId;
                                Singleton<CHeroSelectSystem>.instance.ResetRandHeroLeftCount((int) csdt_campinfo2.astCampPlayerInfo[m].dwRandomHeroCnt);
                            }
                        }
                    }
                }
            }
            this.roomInfo.selfObjID = dwObjId;
        }

        public void UpdateRoomInfoReconnectPick(COMDT_DESKINFO inDeskInfo, CSDT_RECONN_CAMPPICKINFO[] inCampInfo)
        {
            this.roomInfo = new Assets.Scripts.GameSystem.RoomInfo();
            this.roomInfo.roomAttrib.bGameMode = inDeskInfo.bGameMode;
            this.roomInfo.roomAttrib.bPkAI = 0;
            this.roomInfo.roomAttrib.bMapType = inDeskInfo.bMapType;
            this.roomInfo.roomAttrib.dwMapId = inDeskInfo.dwMapId;
            this.roomInfo.roomAttrib.bWarmBattle = Convert.ToBoolean(inDeskInfo.bIsWarmBattle);
            this.roomInfo.roomAttrib.npcAILevel = inDeskInfo.bAILevel;
            for (int i = 0; i < inCampInfo.Length; i++)
            {
                COM_PLAYERCAMP com_playercamp = (COM_PLAYERCAMP) (i + 1);
                CSDT_RECONN_CAMPPICKINFO csdt_reconn_camppickinfo = inCampInfo[i];
                this.roomInfo.CampMemberList[i].Clear();
                for (int j = 0; j < csdt_reconn_camppickinfo.dwPlayerNum; j++)
                {
                    MemberInfo item = new MemberInfo();
                    COMDT_PLAYERINFO stPlayerInfo = csdt_reconn_camppickinfo.astPlayerInfo[j].stPickHeroInfo.stPlayerInfo;
                    item.isPrepare = csdt_reconn_camppickinfo.astPlayerInfo[j].bIsPickOK > 0;
                    item.RoomMemberType = stPlayerInfo.bObjType;
                    item.dwPosOfCamp = stPlayerInfo.bPosOfCamp;
                    item.camp = com_playercamp;
                    item.dwMemberLevel = stPlayerInfo.dwLevel;
                    if (item.RoomMemberType == 1)
                    {
                        item.ullUid = stPlayerInfo.stDetail.stPlayerOfAcnt.ullUid;
                        item.dwMemberPvpLevel = stPlayerInfo.stDetail.stPlayerOfAcnt.dwPvpLevel;
                    }
                    item.dwObjId = stPlayerInfo.dwObjId;
                    item.MemberName = StringHelper.UTF8BytesToString(ref stPlayerInfo.szName);
                    item.ChoiceHero = stPlayerInfo.astChoiceHero;
                    item.isGM = csdt_reconn_camppickinfo.astPlayerInfo[j].stPickHeroInfo.bIsGM > 0;
                    if (stPlayerInfo.bObjType == 1)
                    {
                        item.dwMemberHeadId = stPlayerInfo.stDetail.stPlayerOfAcnt.dwHeadId;
                        if (stPlayerInfo.stDetail.stPlayerOfAcnt.ullUid == Singleton<CRoleInfoManager>.instance.masterUUID)
                        {
                            this.roomInfo.selfObjID = stPlayerInfo.dwObjId;
                            Singleton<CHeroSelectSystem>.instance.ResetRandHeroLeftCount((int) csdt_reconn_camppickinfo.astPlayerInfo[j].stPickHeroInfo.dwRandomHeroCnt);
                        }
                    }
                    else if (stPlayerInfo.bObjType == 2)
                    {
                        item.dwMemberHeadId = 1;
                    }
                    this.roomInfo.CampMemberList[i].Add(item);
                }
                this.SortCampMemList(ref this.roomInfo.CampMemberList[i]);
            }
        }

        public bool IsInRoom
        {
            get
            {
                return this.bInRoom;
            }
        }

        public bool IsSelfRoomOwner
        {
            get
            {
                return ((this.roomInfo.selfInfo.ullUid == this.roomInfo.roomOwner.ullUid) && (this.roomInfo.selfInfo.iGameEntity == this.roomInfo.roomOwner.iGameEntity));
            }
        }
    }
}

