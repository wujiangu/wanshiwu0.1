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
    public class CMiShuSystem : Singleton<CMiShuSystem>
    {
        public static string s_btnBeizhanFlagKey = "BeizhanEntryBtn_V1";

        public void CheckActPlayModeTipsForLobby()
        {
            uint result = 0;
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_PVP_Fire"), out result);
            stMatchOpenInfo matchOpenState = CUICommonSystem.GetMatchOpenState(RES_BATTLE_MAP_TYPE.RES_BATTLE_MAP_TYPE_ENTERTAINMENT, result);
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(LobbyForm.FORM_PATH);
            if (form != null)
            {
                if (!CSysDynamicBlock.bLobbyEntryBlocked)
                {
                    Transform transform = form.transform.Find("BtnCon/PvpBtn/ActModePanel");
                    if (transform != null)
                    {
                        transform.SetAsLastSibling();
                        transform.gameObject.CustomSetActive(matchOpenState.matchState == enMatchOpenState.enMatchOpen_InActiveTime);
                    }
                }
                Transform transform2 = form.transform.FindChild("BtnCon/UnionBtn/ShangJin");
                if (transform2 != null)
                {
                    transform2.SetAsLastSibling();
                    transform2.gameObject.CustomSetActive(CUnionBattleEntrySystem.HasMatchInActiveTime());
                }
            }
        }

        public void CheckActPlayModeTipsForPvpEntry()
        {
            uint result = 0;
            uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_PVP_Fire"), out result);
            stMatchOpenInfo matchOpenState = CUICommonSystem.GetMatchOpenState(RES_BATTLE_MAP_TYPE.RES_BATTLE_MAP_TYPE_ENTERTAINMENT, result);
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(CMatchingSystem.PATH_MATCHING_ENTRY);
            if ((form != null) && !CSysDynamicBlock.bLobbyEntryBlocked)
            {
                Transform transform = form.transform.Find("panelGroup1/btnGroup/Button1/ActModePanel");
                if (transform != null)
                {
                    transform.gameObject.CustomSetActive(matchOpenState.matchState == enMatchOpenState.enMatchOpen_InActiveTime);
                }
            }
            if (form != null)
            {
                Transform transform2 = form.transform.Find("panelGroup2/btnGroup/Button5");
                Transform transform3 = form.transform.Find("panelGroup2/btnGroup/Button5/Lock");
                Transform transform4 = form.transform.Find("panelGroup2/btnGroup/Button5/Open");
                if (transform2 != null)
                {
                    CUIMiniEventScript component = transform2.GetComponent<CUIMiniEventScript>();
                    if (((transform3 != null) && (transform4 != null)) && (component != null))
                    {
                        transform2.gameObject.CustomSetActive(false);
                        transform3.gameObject.CustomSetActive(false);
                        transform4.gameObject.CustomSetActive(false);
                        component.enabled = false;
                        if (matchOpenState.matchState == enMatchOpenState.enMatchOpen_InActiveTime)
                        {
                            CUICommonSystem.SetTextContent(transform4.Find("Text").gameObject, matchOpenState.descStr);
                            transform4.gameObject.CustomSetActive(true);
                            transform2.gameObject.CustomSetActive(true);
                            component.enabled = true;
                            if (CSysDynamicBlock.bLobbyEntryBlocked)
                            {
                                Transform transform5 = transform4.FindChild("Image");
                                Transform transform6 = transform4.FindChild("TextOpen");
                                if ((transform5 != null) && (transform6 != null))
                                {
                                    transform5.gameObject.CustomSetActive(false);
                                    transform6.gameObject.CustomSetActive(false);
                                }
                            }
                        }
                        else if (matchOpenState.matchState == enMatchOpenState.enMatchOpen_NotInActiveTime)
                        {
                            CUICommonSystem.SetTextContent(transform3.Find("Text").gameObject, matchOpenState.descStr);
                            transform3.gameObject.CustomSetActive(true);
                            transform2.gameObject.CustomSetActive(true);
                        }
                    }
                }
            }
        }

        public void CheckMiShuTalk(bool isRestarTimer = true)
        {
            bool flag = false;
            string szMiShuDesc = null;
            if (Singleton<CTaskSys>.instance.GetTotalTaskOfState(RES_TASK_TYPE.RES_TASKTYPE_MAIN, COM_TASK_STATE.COM_TASK_HAVEDONE) > 0)
            {
                flag = true;
                szMiShuDesc = Singleton<CTextManager>.instance.GetText("Secretary_Reward_Tips");
            }
            else if (Singleton<CTaskSys>.instance.model.GetMaxIndex_TaskID_InState(RES_TASK_TYPE.RES_TASKTYPE_USUAL, CTask.State.Have_Done) != null)
            {
                flag = true;
                szMiShuDesc = Singleton<CTextManager>.instance.GetText("Secretary_Reward_Tips");
            }
            else
            {
                CTask task = Singleton<CTaskSys>.instance.model.GetMaxIndex_TaskID_InState(RES_TASK_TYPE.RES_TASKTYPE_USUAL, CTask.State.OnGoing);
                if (task != null)
                {
                    flag = true;
                    szMiShuDesc = task.m_resTask.szMiShuDesc;
                }
            }
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(LobbyForm.FORM_PATH);
            if (form != null)
            {
                Transform transform = form.transform.Find("LobbyBottom/Newbie/TalkFrame");
                Text component = form.transform.Find("LobbyBottom/Newbie/TalkFrame/Text").GetComponent<Text>();
                CUITimerScript script2 = form.transform.Find("LobbyBottom/Newbie/TalkFrame/Timer").GetComponent<CUITimerScript>();
                if (flag)
                {
                    transform.gameObject.CustomSetActive(true);
                    component.text = szMiShuDesc;
                    script2.ReStartTimer();
                }
                else
                {
                    transform.gameObject.CustomSetActive(false);
                    script2.EndTimer();
                }
                if (isRestarTimer)
                {
                    form.transform.Find("LobbyBottom/Newbie/Timer").GetComponent<CUITimerScript>().ReStartTimer();
                }
            }
        }

        public ResMiShuInfo[] GetResList(int TabIndex)
        {
            List<ResMiShuInfo> list = new List<ResMiShuInfo>();
            Dictionary<long, object>.Enumerator enumerator = GameDataMgr.miShuLib.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<long, object> current = enumerator.Current;
                ResMiShuInfo item = (ResMiShuInfo) current.Value;
                if (item.bType == TabIndex)
                {
                    list.Add(item);
                }
            }
            return list.ToArray();
        }

        public void HideNewFlagForBeizhanEntry()
        {
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(LobbyForm.FORM_PATH);
            if (form != null)
            {
                Transform transform = form.transform.Find("LobbyBottom/SysEntry/ChatBtn/redDotNew");
                if ((transform != null) && !PlayerPrefs.HasKey(s_btnBeizhanFlagKey))
                {
                    PlayerPrefs.SetString(s_btnBeizhanFlagKey, "1");
                    PlayerPrefs.Save();
                    transform.gameObject.CustomSetActive(false);
                }
            }
        }

        public override void Init()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.MiShu_OnClickMiShu, new CUIEventManager.OnUIEventHandler(this.OnClickMiShu));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.MiShu_OnCheckTalk, new CUIEventManager.OnUIEventHandler(this.OnCheckMiShuTalk));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.MiShu_OnCloseTalk, new CUIEventManager.OnUIEventHandler(this.OnCloseTalk));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.MiShu_OnClickGoto, new CUIEventManager.OnUIEventHandler(this.OnClickGotoEntry));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.MiShu_OnCheckFirstWin, new CUIEventManager.OnUIEventHandler(this.OnCheckFirstWin));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.MiShu_OnPlayAnimation, new CUIEventManager.OnUIEventHandler(this.OnPlayMishuAnimation));
        }

        public void InitList(int TabIndex, CUIListScript list)
        {
            ResMiShuInfo[] resList = this.GetResList(TabIndex);
            list.SetElementAmount(resList.Length);
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            for (int i = 0; i < resList.Length; i++)
            {
                Transform transform = list.GetElemenet(i).transform;
                Image component = transform.Find("imgIcon").GetComponent<Image>();
                Text text = transform.Find("lblTitle").GetComponent<Text>();
                Text text2 = transform.Find("lblUnLock").GetComponent<Text>();
                Text text3 = transform.Find("lblDesc").GetComponent<Text>();
                Text text4 = transform.Find("lblCoinDesc").GetComponent<Text>();
                Button btn = transform.Find("btnGoto").GetComponent<Button>();
                component.SetSprite(CUIUtility.s_Sprite_Dynamic_Task_Dir + resList[i].dwIconID, list.m_belongedFormScript, true, false, false);
                text.text = resList[i].szName;
                text2.text = resList[i].szUnOpenDesc;
                text3.text = resList[i].szDesc;
                text4.text = string.Empty;
                if ((resList[i].bShowCoinLimit > 0) && (masterRoleInfo != null))
                {
                    uint getCnt = 0;
                    uint limitCnt = 0;
                    masterRoleInfo.GetCoinDailyInfo((RES_COIN_GET_PATH_TYPE) resList[i].bCoinGetPath, out getCnt, out limitCnt);
                    text4.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Coin_GetCnt_Daily_Desc"), getCnt, limitCnt);
                }
                this.InitSysBtn(btn, (RES_GAME_ENTRANCE_TYPE) resList[i].bGotoID, text2.gameObject, text4.gameObject);
            }
        }

        private void InitSysBtn(Button btn, RES_GAME_ENTRANCE_TYPE entryType, GameObject txtObj, GameObject coinTextObj)
        {
            RES_SPECIALFUNCUNLOCK_TYPE type = CUICommonSystem.EntryTypeToUnlockType(entryType);
            if (Singleton<CFunctionUnlockSys>.instance.FucIsUnlock(type))
            {
                txtObj.CustomSetActive(false);
                coinTextObj.CustomSetActive(true);
                CUICommonSystem.SetButtonEnableWithShader(btn, true, true);
                btn.GetComponent<CUIEventScript>().m_onClickEventParams.tag = (int) entryType;
            }
            else
            {
                txtObj.CustomSetActive(true);
                coinTextObj.CustomSetActive(false);
                CUICommonSystem.SetButtonEnableWithShader(btn, false, true);
            }
        }

        public void OnCheckFirstWin(CUIEvent uiEvent)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(LobbyForm.FORM_PATH);
            if ((form != null) && (masterRoleInfo != null))
            {
                Transform transform = form.transform.Find("Award");
                Text component = transform.Find("lblFirstWin").GetComponent<Text>();
                Image image = transform.Find("Icon").GetComponent<Image>();
                CUITimerScript script2 = transform.Find("Timer").GetComponent<CUITimerScript>();
                if (!masterRoleInfo.IsFirstWinOpen())
                {
                    transform.gameObject.CustomSetActive(false);
                }
                else
                {
                    transform.gameObject.CustomSetActive(true);
                    float curFirstWinRemainingTimeSec = masterRoleInfo.GetCurFirstWinRemainingTimeSec();
                    if (curFirstWinRemainingTimeSec <= 0f)
                    {
                        component.text = Singleton<CTextManager>.GetInstance().GetText("Daily_Quest_FirstVictory");
                        image.color = CUIUtility.s_Color_White;
                        script2.gameObject.CustomSetActive(false);
                    }
                    else
                    {
                        component.text = Singleton<CTextManager>.GetInstance().GetText("Daily_Quest_FirstVictoryCD");
                        image.color = CUIUtility.s_Color_GrayShader;
                        script2.gameObject.CustomSetActive(true);
                        script2.SetTotalTime(curFirstWinRemainingTimeSec);
                        script2.StartTimer();
                    }
                }
            }
        }

        public void OnCheckMiShuTalk(CUIEvent uiEvent)
        {
            this.CheckMiShuTalk(true);
        }

        private void OnClickGotoEntry(CUIEvent uiEvent)
        {
            CUICommonSystem.JumpForm((RES_GAME_ENTRANCE_TYPE) uiEvent.m_eventParams.tag);
        }

        private void OnClickMiShu(CUIEvent uiEvent)
        {
            CUIEvent event2 = new CUIEvent {
                m_eventID = enUIEventID.Task_OpenForm
            };
            event2.m_eventParams.tag = 1;
            if (Singleton<CTaskSys>.instance.GetTotalTaskOfState(RES_TASK_TYPE.RES_TASKTYPE_MAIN, COM_TASK_STATE.COM_TASK_HAVEDONE) > 0)
            {
                event2.m_eventParams.tag = 0;
            }
            Singleton<CUIEventManager>.instance.DispatchUIEvent(event2);
            SendUIClickToServer(enUIClickReprotID.rp_MishuBtn);
        }

        public void OnCloseTalk(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(LobbyForm.FORM_PATH);
            if (form != null)
            {
                Transform transform = form.transform.Find("LobbyBottom/Newbie/TalkFrame");
                CUITimerScript component = form.transform.Find("LobbyBottom/Newbie/TalkFrame/Timer").GetComponent<CUITimerScript>();
                transform.gameObject.CustomSetActive(false);
                component.EndTimer();
            }
        }

        public void OnPlayMishuAnimation(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(LobbyForm.FORM_PATH);
            if (form != null)
            {
                Transform transform = form.transform.Find("LobbyBottom/Newbie/Image_DaJi");
                if (transform != null)
                {
                    CUICommonSystem.PlayAnimator(transform.gameObject, "Blink_0" + UnityEngine.Random.Range(1, 3));
                }
            }
        }

        public static void SendReqCoinGetPathData()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x502);
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
        }

        public static void SendUIClickToServer(enUIClickReprotID clickID)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x138a);
            msg.stPkgData.stCltActionStatistics.bActionType = 1;
            msg.stPkgData.stCltActionStatistics.stActionData.construct((long) msg.stPkgData.stCltActionStatistics.bActionType);
            msg.stPkgData.stCltActionStatistics.stActionData.stSecretary.iID = (int) clickID;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
        }

        public void ShowNewFlagForBeizhanEntry()
        {
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(LobbyForm.FORM_PATH);
            if (form != null)
            {
                Transform transform = form.transform.Find("LobbyBottom/SysEntry/ChatBtn/redDotNew");
                if (transform != null)
                {
                    transform.gameObject.CustomSetActive(false);
                    if (!PlayerPrefs.HasKey(s_btnBeizhanFlagKey))
                    {
                        transform.gameObject.CustomSetActive(true);
                    }
                }
            }
        }
    }
}

