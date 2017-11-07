namespace Assets.Scripts.GameSystem
{
    using Apollo;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    internal class LobbyForm
    {
        private static bool _autoPoped = false;
        private GameObject _rankingBtn;
        private GameObject addSkill_btn;
        private CUIAnimatorScript animatorScript;
        public static bool AutoPopAllow = true;
        private GameObject bag_btn;
        public static string FORM_PATH = "UGUI/Form/System/Lobby/Form_Lobby.prefab";
        private GameObject hero_btn;
        public bool m_bInLobby;
        private GameObject m_FormObj;
        private Text m_PlayerExp;
        private Text m_PlayerName;
        private Text m_PlayerVipLevel;
        private Image m_PvpExpImg;
        private Text m_PvpExpTxt;
        private Text m_PvpLevel;
        private GameObject m_QQbuluBtn;
        private RankingSystem.RankingSubView m_rankingType = RankingSystem.RankingSubView.Friend;
        private int myRankingNo = -1;
        public static string Pve_BtnRes_PATH = (CUIUtility.s_Sprite_System_Lobby_Dir + "PveBtnDynamic.prefab");
        public static string Pvp_BtnRes_PATH = (CUIUtility.s_Sprite_System_Lobby_Dir + "PvpBtnDynamic.prefab");
        private ListView<COMDT_FRIEND_INFO> rankFriendList;
        private GameObject social_btn;
        private GameObject symbol_btn;
        private GameObject task_btn;

        private void AutoPopup1_IDIP()
        {
            if (MonoSingleton<IDIPSys>.GetInstance().RedPotState)
            {
                Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.IDIP_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnIDIPClose));
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.IDIP_OpenForm);
            }
            else
            {
                this.AutoPopup2_Activity();
            }
        }

        private void AutoPopup2_Activity()
        {
            if (Singleton<ActivitySys>.GetInstance().CheckReadyForDot(RES_WEAL_ENTRANCE_TYPE.RES_WEAL_ENTRANCE_TYPE_ACTIVITY))
            {
                Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Activity_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnActivityClose));
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Activity_OpenForm);
            }
        }

        public void Check_Enable(RES_SPECIALFUNCUNLOCK_TYPE type)
        {
            bool bShow = Singleton<CFunctionUnlockSys>.instance.TipsHasShow(type);
            if (!bShow && !Singleton<CFunctionUnlockSys>.instance.IsTypeHasCondition(type))
            {
                bShow = true;
            }
            this.SetEnable(type, bShow);
        }

        private void CheckNewbieIntro(int timerSeq)
        {
            if (!this.PopupNewbieIntro() && !_autoPoped)
            {
                _autoPoped = true;
            }
        }

        public void Clear()
        {
            this.m_rankingType = RankingSystem.RankingSubView.Friend;
        }

        private void HideMoreBtn(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(FORM_PATH);
            if (form != null)
            {
                Transform transform = form.transform.Find("Popup/MoreCon");
                if (transform != null)
                {
                    transform.gameObject.CustomSetActive(false);
                }
                Transform transform2 = form.transform.Find("Popup/MoreBtn");
                if (transform2 != null)
                {
                    transform2.gameObject.CustomSetActive(true);
                }
            }
        }

        public void init()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.IDIP_QQVIP_OpenWealForm, new CUIEventManager.OnUIEventHandler(this.OpenQQVIPWealForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.WEB_OpenHome, new CUIEventManager.OnUIEventHandler(this.OpenWebHome));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.WEB_IntegralHall, new CUIEventManager.OnUIEventHandler(this.OpenIntegralHall));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.OPEN_QQ_Buluo, new CUIEventManager.OnUIEventHandler(this.OpenQQBuluo));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Lobby_ShowMoreBtn, new CUIEventManager.OnUIEventHandler(this.ShowMoreBtn));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Lobby_HideMoreBtn, new CUIEventManager.OnUIEventHandler(this.HideMoreBtn));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Lobby_UnlockAnimation_End, new CUIEventManager.OnUIEventHandler(this.On_Lobby_UnlockAnimation_End));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Lobby_MysteryShopClose, new CUIEventManager.OnUIEventHandler(this.On_Lobby_MysteryShopClose));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.GameCenter_OpenWXRight, new CUIEventManager.OnUIEventHandler(this.OpenWXGameCenterRightForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Lobby_RankingListElementEnable, new CUIEventManager.OnUIEventHandler(this.OnRankingListElementEnable));
            Singleton<EventRouter>.GetInstance().AddEventHandler("CheckNewbieIntro", new Action(this, (IntPtr) this.OnCheckNewbieIntro));
            Singleton<EventRouter>.GetInstance().AddEventHandler("VipInfoHadSet", new Action(this, (IntPtr) this.UpdateQQVIPState));
            Singleton<EventRouter>.GetInstance().AddEventHandler(EventID.NOBE_STATE_CHANGE, new Action(this, (IntPtr) this.UpdateNobeIcon));
            Singleton<EventRouter>.GetInstance().AddEventHandler(EventID.NOBE_STATE_HEAD_CHANGE, new Action(this, (IntPtr) this.UpdateNobeHeadIdx));
            Singleton<EventRouter>.GetInstance().AddEventHandler("MasterPvpLevelChanged", new Action(this, (IntPtr) this.OnPlayerLvlChange));
            Singleton<EventRouter>.GetInstance().AddEventHandler("Rank_Friend_List", new Action(this, (IntPtr) this.RefreshRankList));
            Singleton<EventRouter>.GetInstance().AddEventHandler<RankingSystem.RankingSubView>("Rank_List", new Action<RankingSystem.RankingSubView>(this.RefreshRankList));
            Singleton<EventRouter>.GetInstance().AddEventHandler(EventID.NAMECHANGE_PLAYER_NAME_CHANGE, new Action(this, (IntPtr) this.OnPlayerNameChange));
            Singleton<EventRouter>.GetInstance().AddEventHandler(EventID.HEAD_IMAGE_FLAG_CHANGE, new Action(this, (IntPtr) this.UpdatePlayerData));
        }

        private void InitForm(CUIFormScript form)
        {
            this.m_FormObj = form.gameObject;
            this.m_PlayerName = this.m_FormObj.transform.Find("PlayerHead/NameGroup/PlayerName").GetComponent<Text>();
            this.m_PvpLevel = this.m_FormObj.transform.Find("PlayerHead/pvpLevel").GetComponent<Text>();
            this.m_PlayerExp = this.m_FormObj.transform.Find("PlayerHead/PlayerExp").GetComponent<Text>();
            this.m_PlayerVipLevel = this.m_FormObj.transform.Find("PlayerHead/imgVipBg/txtVipLevel").GetComponent<Text>();
            this.m_PvpExpImg = this.m_FormObj.transform.Find("PlayerHead/pvpExp/expBg/imgExp").GetComponent<Image>();
            this.m_PvpExpTxt = this.m_FormObj.transform.Find("PlayerHead/pvpExp/expBg/txtExp").GetComponent<Text>();
            this.animatorScript = this.m_FormObj.GetComponent<CUIAnimatorScript>();
            this.hero_btn = this.m_FormObj.transform.Find("LobbyBottom/SysEntry/HeroBtn").gameObject;
            this.symbol_btn = this.m_FormObj.transform.Find("LobbyBottom/SysEntry/SymbolBtn").gameObject;
            this.bag_btn = this.m_FormObj.transform.Find("LobbyBottom/SysEntry/BagBtn").gameObject;
            this.task_btn = this.m_FormObj.transform.Find("LobbyBottom/Newbie").gameObject;
            this.social_btn = this.m_FormObj.transform.Find("LobbyBottom/SysEntry/SocialBtn").gameObject;
            this.addSkill_btn = this.m_FormObj.transform.Find("LobbyBottom/SysEntry/AddedSkillBtn").gameObject;
            this.Check_Enable(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_HERO);
            this.Check_Enable(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_SYMBOL);
            this.Check_Enable(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_BAG);
            this.Check_Enable(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_TASK);
            this.Check_Enable(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_UNION);
            this.Check_Enable(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_FRIEND);
            this.Check_Enable(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_ADDEDSKILL);
            if (CSysDynamicBlock.bLobbyEntryBlocked)
            {
                Transform transform = this.m_FormObj.transform.Find("Popup");
                if (transform != null)
                {
                    transform.gameObject.CustomSetActive(false);
                }
                Transform transform2 = this.m_FormObj.transform.Find("BtnCon/CompetitionBtn");
                if (transform2 != null)
                {
                    transform2.gameObject.CustomSetActive(false);
                }
                if (this.task_btn != null)
                {
                    this.task_btn.CustomSetActive(false);
                }
                Transform transform3 = this.m_FormObj.transform.Find("DiamondPayBtn");
                if (transform3 != null)
                {
                    transform3.gameObject.CustomSetActive(false);
                }
            }
            if (CSysDynamicBlock.bUnfinishBlock)
            {
                Transform transform4 = this.m_FormObj.transform.Find("BtnCon/WebBtn");
                if (transform4 != null)
                {
                    transform4.gameObject.CustomSetActive(false);
                }
            }
            Button component = this.m_FormObj.transform.Find("BtnCon/LadderBtn").GetComponent<Button>();
            if (component != null)
            {
                component.interactable = Singleton<CLadderSystem>.GetInstance().IsLevelQualified();
                Transform transform5 = component.transform.Find("Lock");
                if (transform5 != null)
                {
                    transform5.gameObject.CustomSetActive(!component.interactable);
                }
            }
            Button button2 = this.m_FormObj.transform.FindChild("BtnCon/UnionBtn").GetComponent<Button>();
            if (button2 != null)
            {
                bool bActive = Singleton<CUnionBattleEntrySystem>.GetInstance().IsUnionFuncLocked();
                button2.interactable = !bActive;
                button2.transform.FindChild("Lock").gameObject.CustomSetActive(bActive);
            }
            GameObject gameObject = this.m_FormObj.transform.Find("PlayerHead/pvpExp").gameObject;
            if (gameObject != null)
            {
                CUIEventScript script = gameObject.GetComponent<CUIEventScript>();
                if (script == null)
                {
                    script = gameObject.AddComponent<CUIEventScript>();
                    script.Initialize(form);
                }
                CUseable useable = CUseableManager.CreateVirtualUseable(enVirtualItemType.enExp, 0);
                stUIEventParams eventParams = new stUIEventParams {
                    iconUseable = useable,
                    tag = 3
                };
                script.SetUIEvent(enUIEventType.Down, enUIEventID.Tips_ItemInfoOpen, eventParams);
                script.SetUIEvent(enUIEventType.HoldEnd, enUIEventID.Tips_ItemInfoClose, eventParams);
                script.SetUIEvent(enUIEventType.Click, enUIEventID.Tips_ItemInfoClose, eventParams);
                script.SetUIEvent(enUIEventType.DragEnd, enUIEventID.Tips_ItemInfoClose, eventParams);
            }
            RefreshDianQuanPayButton(false);
            Transform transform6 = this.m_FormObj.transform.Find("BtnCon/PvpBtn");
            Transform transform7 = this.m_FormObj.transform.Find("BtnCon/PveBtn");
            if (transform6 != null)
            {
                Singleton<CMallSystem>.instance.LoadUIPrefab(Pvp_BtnRes_PATH, "PvpBtnDynamic", transform6.gameObject, form);
            }
            if (transform7 != null)
            {
                Singleton<CMallSystem>.instance.LoadUIPrefab(Pve_BtnRes_PATH, "PveBtnDynamic", transform7.gameObject, form);
            }
        }

        private void InitOther(CUIFormScript m_FormScript)
        {
            this._rankingBtn = m_FormScript.GetWidget(1);
            if ((this._rankingBtn != null) && CSysDynamicBlock.bSocialBlocked)
            {
                this._rankingBtn.CustomSetActive(false);
            }
            this.RefreshRankList();
            this.UpdatePlayerData();
            Singleton<CTimerManager>.GetInstance().AddTimer(50, 1, new CTimer.OnTimeUpHandler(this.CheckNewbieIntro));
            this.ProcessQQVIP(m_FormScript);
            this.UpdateGameCenterState(m_FormScript);
            this.m_QQbuluBtn = Utility.FindChild(m_FormScript.gameObject, "Popup/MoreCon/Buluo");
            GameObject obj2 = Utility.FindChild(m_FormScript.gameObject, "Popup/JFQBtn");
            if ((ApolloConfig.platform == ApolloPlatform.QQ) || (ApolloConfig.platform == ApolloPlatform.WTLogin))
            {
                this.m_QQbuluBtn.CustomSetActive(true);
                obj2.CustomSetActive(false);
            }
            else
            {
                this.m_QQbuluBtn.CustomSetActive(false);
                obj2.CustomSetActive(true);
            }
            MonoSingleton<NobeSys>.GetInstance().ShowDelayNobeTipsInfo();
        }

        private void On_Lobby_MysteryShopClose(CUIEvent uiEvent)
        {
            GameObject obj2 = Utility.FindChild(uiEvent.m_srcFormScript.gameObject, "Popup/BoardBtn/MysteryShop");
            Debug.LogWarning(string.Format("mystery shop icon on close:{0}", obj2));
            obj2.CustomSetActive(false);
        }

        private void On_Lobby_UnlockAnimation_End(CUIEvent uievent)
        {
            Singleton<CSoundManager>.instance.PostEvent("UI_hall_system_back", null);
            this.SetEnable((RES_SPECIALFUNCUNLOCK_TYPE) uievent.m_eventParams.tag, true);
        }

        private void OnActivityClose(CUIEvent uiEvt)
        {
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Activity_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnActivityClose));
        }

        private void OnCheckNewbieIntro()
        {
            Singleton<CTimerManager>.GetInstance().AddTimer(100, 1, seq => this.PopupNewbieIntro());
        }

        protected void OnCloseForm()
        {
            this.m_bInLobby = false;
            this.UnInitWidget();
        }

        private void OnIDIPClose(CUIEvent uiEvt)
        {
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.IDIP_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnIDIPClose));
            this.AutoPopup2_Activity();
        }

        private void OnPlayerLvlChange()
        {
            if (this.m_FormObj != null)
            {
                Button component = this.m_FormObj.transform.Find("BtnCon/LadderBtn").GetComponent<Button>();
                if (component != null)
                {
                    component.interactable = Singleton<CLadderSystem>.GetInstance().IsLevelQualified();
                    Transform transform = component.transform.Find("Lock");
                    if (transform != null)
                    {
                        transform.gameObject.CustomSetActive(!component.interactable);
                    }
                }
                Button button2 = this.m_FormObj.transform.FindChild("BtnCon/UnionBtn").GetComponent<Button>();
                if (button2 != null)
                {
                    bool bActive = Singleton<CUnionBattleEntrySystem>.GetInstance().IsUnionFuncLocked();
                    button2.interactable = !bActive;
                    button2.transform.FindChild("Lock").gameObject.CustomSetActive(bActive);
                }
            }
        }

        private void OnPlayerNameChange()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if ((this.m_PlayerName != null) && (masterRoleInfo != null))
            {
                this.m_PlayerName.text = masterRoleInfo.Name;
            }
        }

        private void OnRankingListElementEnable(CUIEvent uiEvent)
        {
            int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
            GameObject srcWidget = uiEvent.m_srcWidget;
            if (this.m_rankingType == RankingSystem.RankingSubView.Friend)
            {
                this.OnUpdateRankingFriendElement(srcWidget, srcWidgetIndexInBelongedList);
            }
            else if (this.m_rankingType == RankingSystem.RankingSubView.All)
            {
                this.OnUpdateRankingAllElement(srcWidget, srcWidgetIndexInBelongedList);
            }
        }

        private void OnUpdateRankingAllElement(GameObject go, int index)
        {
            CSDT_RANKING_LIST_SUCC rankList = Singleton<RankingSystem>.instance.GetRankList(RankingSystem.RankingType.Ladder);
            if (rankList != null)
            {
                string serverUrl = string.Empty;
                GameObject headIcon = null;
                GameObject gameObject = null;
                GameObject obj4 = null;
                headIcon = go.transform.Find("HeadIcon").gameObject;
                gameObject = go.transform.Find("HeadbgNo1").gameObject;
                obj4 = go.transform.Find("123No").gameObject;
                serverUrl = Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(ref rankList.astItemDetail[index].stExtraInfo.stDetailInfo.stLadderPoint.szHeadUrl);
                gameObject.CustomSetActive(index == 0);
                RankingSystem.SetUrlHeadIcon(headIcon, serverUrl);
                obj4.transform.GetChild(0).gameObject.CustomSetActive(0 == index);
                obj4.transform.GetChild(1).gameObject.CustomSetActive(1 == index);
                obj4.transform.GetChild(2).gameObject.CustomSetActive(2 == index);
                int dwHeadIconId = (int) rankList.astItemDetail[index].stExtraInfo.stDetailInfo.stLadderPoint.stGameVip.dwHeadIconId;
                Image component = go.transform.Find("NobeImag").GetComponent<Image>();
                if (component != null)
                {
                    MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(component, dwHeadIconId);
                }
                GameObject qQVipIcon = go.transform.Find("QQVipIcon").gameObject;
                this.SetQQVip(qQVipIcon, false, (int) rankList.astItemDetail[index].stExtraInfo.stDetailInfo.stLadderPoint.dwVipLevel);
            }
        }

        private void OnUpdateRankingFriendElement(GameObject go, int index)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            string serverUrl = string.Empty;
            GameObject headIcon = null;
            GameObject gameObject = null;
            GameObject obj4 = null;
            int num = (index <= this.myRankingNo) ? 0 : -1;
            Transform transform = go.transform;
            headIcon = transform.Find("HeadIcon").gameObject;
            gameObject = transform.transform.Find("HeadbgNo1").gameObject;
            obj4 = transform.transform.Find("123No").gameObject;
            int headIdx = 0;
            if (index == this.myRankingNo)
            {
                if (masterRoleInfo != null)
                {
                    serverUrl = masterRoleInfo.HeadUrl;
                    headIdx = (int) masterRoleInfo.GetNobeInfo().stGameVipClient.dwHeadIconId;
                    GameObject qQVipIcon = transform.transform.Find("QQVipIcon").gameObject;
                    this.SetQQVip(qQVipIcon, true, 0);
                }
            }
            else if ((index + num) < this.rankFriendList.Count)
            {
                serverUrl = Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(ref this.rankFriendList[index + num].szHeadUrl);
                headIdx = (int) this.rankFriendList[index + num].stGameVip.dwHeadIconId;
                GameObject obj6 = transform.transform.Find("QQVipIcon").gameObject;
                this.SetQQVip(obj6, false, (int) this.rankFriendList[index + num].dwQQVIPMask);
            }
            gameObject.CustomSetActive(index == 0);
            obj4.transform.GetChild(0).gameObject.CustomSetActive(0 == index);
            obj4.transform.GetChild(1).gameObject.CustomSetActive(1 == index);
            obj4.transform.GetChild(2).gameObject.CustomSetActive(2 == index);
            Image component = transform.transform.Find("NobeImag").GetComponent<Image>();
            if (component != null)
            {
                MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(component, headIdx);
            }
            RankingSystem.SetUrlHeadIcon(headIcon, serverUrl);
        }

        public void OpenForm()
        {
            this.m_bInLobby = true;
            CUIFormScript form = Singleton<CUIManager>.GetInstance().OpenForm(FORM_PATH, false, true);
            this.InitForm(form);
            this.InitOther(form);
        }

        private void OpenIntegralHall(CUIEvent uiEvent)
        {
            string str = "http://jfq.qq.com/comm/index_android.html";
            CUICommonSystem.OpenUrl(string.Format("{0}?partition={1}", str, MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID), true);
        }

        private void OpenQQBuluo(CUIEvent uievent)
        {
            if ((ApolloConfig.platform == ApolloPlatform.QQ) || (ApolloConfig.platform == ApolloPlatform.WTLogin))
            {
                string strUrl = "http://xiaoqu.qq.com/cgi-bin/bar/qqgame/handle_ticket?redirect_url=http%3A%2F%2Fxiaoqu.qq.com%2Fmobile%2Fbarindex.html%3F%26_bid%3D%26_wv%3D1027%23bid%3D227061";
                CUICommonSystem.OpenUrl(strUrl, true);
            }
        }

        private void OpenQQVIPWealForm(CUIEvent uiEvent)
        {
            string formPath = string.Format("{0}{1}", "UGUI/Form/System/", "IDIPNotice/Form_QQVipPrivilege.prefab");
            CUIFormScript formScript = Singleton<CUIManager>.GetInstance().OpenForm(formPath, false, true);
            if (formScript != null)
            {
                Singleton<QQVipWidget>.instance.SetData(formScript.gameObject, formScript);
            }
        }

        private void OpenWebHome(CUIEvent uiEvent)
        {
            CUICommonSystem.OpenUrl("http://yxzj.qq.com/ingame/all/index.shtml?partition=" + MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID, true);
        }

        private void OpenWXGameCenterRightForm(CUIEvent uiEvent)
        {
            string formPath = string.Format("{0}{1}", "UGUI/Form/System/", "GameCenter/Form_WXGameCenter.prefab");
            Singleton<CUIManager>.GetInstance().OpenForm(formPath, false, true);
        }

        public void Play_UnLock_Animation(RES_SPECIALFUNCUNLOCK_TYPE type)
        {
            string str = string.Empty;
            RES_SPECIALFUNCUNLOCK_TYPE res_specialfuncunlock_type = type;
            switch (res_specialfuncunlock_type)
            {
                case RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_UNION:
                    str = "SocialBtn";
                    break;

                case RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_TASK:
                    str = "TaskBtn";
                    break;

                case RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_HERO:
                    str = "HeroBtn";
                    break;

                case RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_BAG:
                    str = "BagBtn";
                    break;

                case RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_FRIEND:
                    str = "FriendBtn";
                    break;

                case RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_ADDEDSKILL:
                    str = "AddedSkillBtn";
                    break;

                default:
                    if (res_specialfuncunlock_type == RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_SYMBOL)
                    {
                        str = "SymbolBtn";
                    }
                    break;
            }
            if (!string.IsNullOrEmpty(str))
            {
                stUIEventParams eventParams = new stUIEventParams {
                    tag = (int) type
                };
                this.animatorScript.SetUIEvent(enAnimatorEventType.AnimatorEnd, enUIEventID.Lobby_UnlockAnimation_End, eventParams);
                this.animatorScript.PlayAnimator(str);
                Singleton<CSoundManager>.instance.PostEvent("UI_hall_system_unlock", null);
            }
        }

        private bool PopupNewbieIntro()
        {
            if (CSysDynamicBlock.bNewbieBlocked)
            {
                return true;
            }
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            DebugHelper.Assert(masterRoleInfo != null, "Master Role info is NULL!");
            if (((masterRoleInfo != null) && !masterRoleInfo.IsNewbieAchieveSet(0x54)) && (Singleton<CUIManager>.GetInstance().GetForm("UGUI/Form/System/Newbie/Form_NewbieSettle.prefab") == null))
            {
                masterRoleInfo.SetNewbieAchieve(0x54, true, true);
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Wealfare_CloseForm);
                return true;
            }
            return false;
        }

        private void ProcessQQVIP(CUIFormScript form)
        {
            if (null != form)
            {
                Transform transform = form.transform.Find("Popup/QQVIpBtn");
                GameObject gameObject = null;
                if (transform != null)
                {
                    gameObject = transform.gameObject;
                }
                GameObject obj3 = Utility.FindChild(form.gameObject, "PlayerHead/NameGroup/QQVipIcon");
                if ((ApolloConfig.platform == ApolloPlatform.QQ) || (ApolloConfig.platform == ApolloPlatform.WTLogin))
                {
                    if (CSysDynamicBlock.bLobbyEntryBlocked)
                    {
                        gameObject.CustomSetActive(false);
                        obj3.CustomSetActive(false);
                    }
                    else
                    {
                        gameObject.CustomSetActive(true);
                        if (Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo() != null)
                        {
                            MonoSingleton<NobeSys>.GetInstance().SetMyQQVipHead(obj3.GetComponent<Image>());
                        }
                    }
                }
                else
                {
                    gameObject.CustomSetActive(false);
                    obj3.CustomSetActive(false);
                }
            }
        }

        public static void RefreshDianQuanPayButton(bool notifyFromSvr = false)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(FORM_PATH);
            if (form != null)
            {
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                GameObject gameObject = form.transform.Find("DiamondPayBtn").gameObject;
                CUIEventScript component = gameObject.GetComponent<CUIEventScript>();
                CTextManager instance = Singleton<CTextManager>.GetInstance();
                if (!masterRoleInfo.IsGuidedStateSet(0x16))
                {
                    CUICommonSystem.SetButtonName(gameObject, instance.GetText("Pay_Btn_FirstPay"));
                    component.SetUIEvent(enUIEventType.Click, enUIEventID.Pay_OpenFirstPayPanel);
                    CUICommonSystem.DelRedDot(gameObject);
                }
                else if (!masterRoleInfo.IsGuidedStateSet(0x17))
                {
                    CUICommonSystem.SetButtonName(gameObject, instance.GetText("Pay_Btn_FirstPay"));
                    component.SetUIEvent(enUIEventType.Click, enUIEventID.Pay_OpenFirstPayPanel);
                    CUICommonSystem.AddRedDot(gameObject, enRedDotPos.enTopRight, 0);
                }
                else if (!masterRoleInfo.IsGuidedStateSet(0x18))
                {
                    CUICommonSystem.SetButtonName(gameObject, instance.GetText("Pay_Btn_Renewal"));
                    component.SetUIEvent(enUIEventType.Click, enUIEventID.Pay_OpenRenewalPanel);
                    CUICommonSystem.DelRedDot(gameObject);
                }
                else if (!masterRoleInfo.IsGuidedStateSet(0x19))
                {
                    CUICommonSystem.SetButtonName(gameObject, instance.GetText("Pay_Btn_Renewal"));
                    component.SetUIEvent(enUIEventType.Click, enUIEventID.Pay_OpenRenewalPanel);
                    CUICommonSystem.AddRedDot(gameObject, enRedDotPos.enTopRight, 0);
                }
                else if (masterRoleInfo.IsClientBitsSet(0))
                {
                    CUICommonSystem.SetButtonName(gameObject, instance.GetText("GotoTehuiShopName"));
                    component.SetUIEvent(enUIEventType.Click, enUIEventID.Pay_TehuiShop);
                }
                else if (notifyFromSvr)
                {
                    masterRoleInfo.SetClientBits(0, true, false);
                    RefreshDianQuanPayButton(false);
                }
                else
                {
                    gameObject.CustomSetActive(false);
                }
            }
        }

        private void RefreshRankList()
        {
            if (this._rankingBtn != null)
            {
                CUIListScript componetInChild = Utility.GetComponetInChild<CUIListScript>(this._rankingBtn, "RankingList");
                int amount = 0;
                if (this.m_rankingType == RankingSystem.RankingSubView.Friend)
                {
                    this.myRankingNo = Singleton<RankingSystem>.instance.GetMyFriendRankNo();
                    if (this.myRankingNo != -1)
                    {
                        this.rankFriendList = Singleton<CFriendContoller>.instance.model.GetSortedRankingFriendList(COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_LADDER_POINT);
                        if (this.rankFriendList != null)
                        {
                            amount = this.rankFriendList.Count + 1;
                            componetInChild.SetElementAmount(amount);
                            CUIListElementScript elemenet = null;
                            GameObject go = null;
                            for (int i = 0; i < amount; i++)
                            {
                                elemenet = componetInChild.GetElemenet(i);
                                if (elemenet != null)
                                {
                                    go = elemenet.gameObject;
                                    if (go != null)
                                    {
                                        this.OnUpdateRankingFriendElement(go, i);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (this.m_rankingType == RankingSystem.RankingSubView.All)
                {
                    CSDT_RANKING_LIST_SUCC rankList = Singleton<RankingSystem>.instance.GetRankList(RankingSystem.RankingType.Ladder);
                    if (rankList != null)
                    {
                        amount = (int) rankList.dwItemNum;
                        componetInChild.SetElementAmount(amount);
                        CUIListElementScript script3 = null;
                        GameObject gameObject = null;
                        for (int j = 0; j < amount; j++)
                        {
                            script3 = componetInChild.GetElemenet(j);
                            if (script3 != null)
                            {
                                gameObject = script3.gameObject;
                                if (gameObject != null)
                                {
                                    this.OnUpdateRankingAllElement(gameObject, j);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RefreshRankList(RankingSystem.RankingSubView rankingType)
        {
            this.m_rankingType = rankingType;
            this.RefreshRankList();
        }

        private void SetEnable(RES_SPECIALFUNCUNLOCK_TYPE type, bool bShow)
        {
            GameObject obj2 = null;
            if (type == RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_HERO)
            {
                obj2 = this.hero_btn;
            }
            else if (type == RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_SYMBOL)
            {
                obj2 = this.symbol_btn;
            }
            else if (type == RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_BAG)
            {
                obj2 = this.bag_btn;
            }
            else if (type == RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_TASK)
            {
                obj2 = this.task_btn;
            }
            else if (type == RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_UNION)
            {
                obj2 = this.social_btn;
            }
            else if (type == RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_ADDEDSKILL)
            {
                obj2 = this.addSkill_btn;
            }
            else
            {
                obj2 = null;
            }
            if (obj2 != null)
            {
                obj2.CustomSetActive(bShow);
            }
        }

        private void SetQQVip(GameObject QQVipIcon, bool bSelf, int mask = 0)
        {
            if (QQVipIcon != null)
            {
                if ((ApolloConfig.platform == ApolloPlatform.QQ) || (ApolloConfig.platform == ApolloPlatform.WTLogin))
                {
                    if (CSysDynamicBlock.bLobbyEntryBlocked)
                    {
                        QQVipIcon.CustomSetActive(false);
                    }
                    else if (bSelf)
                    {
                        if (Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo() != null)
                        {
                            MonoSingleton<NobeSys>.GetInstance().SetMyQQVipHead(QQVipIcon.GetComponent<Image>());
                        }
                    }
                    else
                    {
                        MonoSingleton<NobeSys>.GetInstance().SetOtherQQVipHead(QQVipIcon.GetComponent<Image>(), mask);
                    }
                }
                else
                {
                    QQVipIcon.CustomSetActive(false);
                }
            }
        }

        public void ShowHideRankingBtn(bool show)
        {
            if (this._rankingBtn != null)
            {
                if (CSysDynamicBlock.bSocialBlocked)
                {
                    this._rankingBtn.CustomSetActive(false);
                }
                else
                {
                    this._rankingBtn.CustomSetActive(show);
                }
            }
        }

        private void ShowMoreBtn(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(FORM_PATH);
            if (form != null)
            {
                Transform transform = form.transform.Find("Popup/MoreCon");
                if (transform != null)
                {
                    transform.gameObject.CustomSetActive(true);
                }
                Transform transform2 = form.transform.Find("Popup/MoreBtn");
                if (transform2 != null)
                {
                    transform2.gameObject.CustomSetActive(false);
                }
            }
        }

        private void StartAutoPopupChain(int timerSeq)
        {
            AutoPopAllow &= !MonoSingleton<NewbieGuideManager>.GetInstance().isNewbieGuiding;
            if (AutoPopAllow)
            {
                this.AutoPopup1_IDIP();
            }
        }

        public void unInit()
        {
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.IDIP_QQVIP_OpenWealForm, new CUIEventManager.OnUIEventHandler(this.OpenQQVIPWealForm));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.WEB_OpenHome, new CUIEventManager.OnUIEventHandler(this.OpenWebHome));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.WEB_IntegralHall, new CUIEventManager.OnUIEventHandler(this.OpenIntegralHall));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.OPEN_QQ_Buluo, new CUIEventManager.OnUIEventHandler(this.OpenQQBuluo));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Lobby_ShowMoreBtn, new CUIEventManager.OnUIEventHandler(this.ShowMoreBtn));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Lobby_HideMoreBtn, new CUIEventManager.OnUIEventHandler(this.HideMoreBtn));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Lobby_UnlockAnimation_End, new CUIEventManager.OnUIEventHandler(this.On_Lobby_UnlockAnimation_End));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Lobby_MysteryShopClose, new CUIEventManager.OnUIEventHandler(this.On_Lobby_MysteryShopClose));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.GameCenter_OpenWXRight, new CUIEventManager.OnUIEventHandler(this.OpenWXGameCenterRightForm));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Lobby_RankingListElementEnable, new CUIEventManager.OnUIEventHandler(this.OnRankingListElementEnable));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler(EventID.NOBE_STATE_CHANGE, new Action(this, (IntPtr) this.UpdateNobeIcon));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler(EventID.NOBE_STATE_HEAD_CHANGE, new Action(this, (IntPtr) this.UpdateNobeHeadIdx));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler("MasterPvpLevelChanged", new Action(this, (IntPtr) this.OnPlayerLvlChange));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler("Rank_Friend_List", new Action(this, (IntPtr) this.RefreshRankList));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<RankingSystem.RankingSubView>("Rank_List", new Action<RankingSystem.RankingSubView>(this.RefreshRankList));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler(EventID.HEAD_IMAGE_FLAG_CHANGE, new Action(this, (IntPtr) this.UpdatePlayerData));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler("CheckNewbieIntro", new Action(this, (IntPtr) this.OnCheckNewbieIntro));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler("VipInfoHadSet", new Action(this, (IntPtr) this.UpdateQQVIPState));
        }

        private void UnInitWidget()
        {
            this._rankingBtn = null;
        }

        private void UpdateGameCenterState(CUIFormScript form)
        {
            if (null != form)
            {
                GameObject obj2 = Utility.FindChild(form.gameObject, "WXGameCenterBtn");
                GameObject obj3 = Utility.FindChild(form.gameObject, "PlayerHead/NameGroup/WXGameCenterIcon");
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                COM_PRIVILEGE_TYPE privilegeType = (masterRoleInfo != null) ? masterRoleInfo.m_privilegeType : COM_PRIVILEGE_TYPE.COM_PRIVILEGE_TYPE_NONE;
                MonoSingleton<NobeSys>.GetInstance().SetGameCenterVisible(obj2, privilegeType, ApolloPlatform.Wechat, false, CSysDynamicBlock.bLobbyEntryBlocked);
                MonoSingleton<NobeSys>.GetInstance().SetGameCenterVisible(obj3, privilegeType, ApolloPlatform.Wechat, true, false);
            }
        }

        private void UpdateNobeHeadIdx()
        {
            int dwHeadIconId = (int) MonoSingleton<NobeSys>.GetInstance().m_vipInfo.stGameVipClient.dwHeadIconId;
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(FORM_PATH);
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (((form != null) && form.gameObject.activeSelf) && (masterRoleInfo != null))
            {
                Image component = form.GetWidget(3).GetComponent<Image>();
                if (component != null)
                {
                    MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(component, dwHeadIconId);
                }
                this.RefreshRankList();
            }
        }

        private void UpdateNobeIcon()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(FORM_PATH);
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (((form != null) && form.gameObject.activeSelf) && (masterRoleInfo != null))
            {
                GameObject widget = form.GetWidget(2);
                if (widget != null)
                {
                    CUIHttpImageScript component = widget.GetComponent<CUIHttpImageScript>();
                    MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(component.GetComponent<Image>(), (int) masterRoleInfo.GetNobeInfo().stGameVipClient.dwCurLevel, false);
                    Image image = form.GetWidget(3).GetComponent<Image>();
                    MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(image, (int) masterRoleInfo.GetNobeInfo().stGameVipClient.dwHeadIconId);
                }
            }
        }

        public void UpdatePlayerData()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(FORM_PATH);
                if (this.m_PlayerName != null)
                {
                    this.m_PlayerName.text = masterRoleInfo.Name;
                }
                if (this.m_PlayerExp != null)
                {
                    this.m_PlayerExp.text = masterRoleInfo.Level.ToString();
                }
                if ((masterRoleInfo != null) && (this.m_PvpExpImg != null))
                {
                    this.m_PvpExpImg.CustomFillAmount(CPlayerProfile.Divide(masterRoleInfo.PvpExp, masterRoleInfo.PvpNeedExp));
                    this.m_PvpExpTxt.text = masterRoleInfo.PvpExp + "/" + masterRoleInfo.PvpNeedExp;
                }
                if (this.m_PvpLevel != null)
                {
                    string text = Singleton<CTextManager>.GetInstance().GetText("ranking_PlayerLevel");
                    if ((!string.IsNullOrEmpty(text) && (this.m_PvpLevel.text != null)) && (masterRoleInfo != null))
                    {
                        this.m_PvpLevel.text = string.Format(text, masterRoleInfo.PvpLevel);
                    }
                }
                if (!CSysDynamicBlock.bSocialBlocked)
                {
                    if ((this.m_PlayerVipLevel != null) && (masterRoleInfo != null))
                    {
                        this.m_PlayerVipLevel.text = string.Format("VIP{0}", masterRoleInfo.m_payLevel);
                    }
                    if (((form != null) && form.gameObject.activeSelf) && (masterRoleInfo != null))
                    {
                        GameObject widget = form.GetWidget(2);
                        if ((widget != null) && !string.IsNullOrEmpty(masterRoleInfo.HeadUrl))
                        {
                            CUIHttpImageScript component = widget.GetComponent<CUIHttpImageScript>();
                            component.SetImageUrl(masterRoleInfo.HeadUrl);
                            MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(component.GetComponent<Image>(), (int) masterRoleInfo.GetNobeInfo().stGameVipClient.dwCurLevel, false);
                            Image image = form.GetWidget(3).GetComponent<Image>();
                            MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(image, (int) masterRoleInfo.GetNobeInfo().stGameVipClient.dwHeadIconId);
                            bool flag = Singleton<HeadIconSys>.instance.UnReadFlagNum > 0;
                            GameObject target = Utility.FindChild(widget, "RedDot");
                            if (target != null)
                            {
                                if (flag)
                                {
                                    CUICommonSystem.AddRedDot(target, enRedDotPos.enTopRight, 0);
                                }
                                else
                                {
                                    CUICommonSystem.DelRedDot(target);
                                }
                            }
                        }
                    }
                }
                else if ((form != null) && form.gameObject.activeSelf)
                {
                    GameObject obj4 = form.GetWidget(2);
                    if (obj4 != null)
                    {
                        CUIHttpImageScript script3 = obj4.GetComponent<CUIHttpImageScript>();
                        if (script3 != null)
                        {
                            MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(script3.GetComponent<Image>(), 0, false);
                        }
                    }
                }
            }
        }

        private void UpdateQQVIPState()
        {
            if ((this.m_FormObj != null) && this.m_bInLobby)
            {
                Transform transform = this.m_FormObj.transform.Find("Popup/QQVIpBtn");
                GameObject gameObject = null;
                if (transform != null)
                {
                    gameObject = transform.gameObject;
                }
                GameObject obj3 = Utility.FindChild(this.m_FormObj, "PlayerHead/QQVipIcon");
                GameObject obj4 = Utility.FindChild(this.m_FormObj, "PlayerHead/QQSVipIcon");
                if ((ApolloConfig.platform == ApolloPlatform.QQ) || (ApolloConfig.platform == ApolloPlatform.WTLogin))
                {
                    if (CSysDynamicBlock.bLobbyEntryBlocked)
                    {
                        gameObject.CustomSetActive(false);
                        obj3.CustomSetActive(false);
                        obj4.CustomSetActive(false);
                    }
                    else
                    {
                        gameObject.CustomSetActive(true);
                        CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                        if (masterRoleInfo != null)
                        {
                            if (masterRoleInfo.HasVip(0x10))
                            {
                                obj3.CustomSetActive(false);
                                obj4.CustomSetActive(true);
                            }
                            else if (masterRoleInfo.HasVip(1))
                            {
                                obj3.CustomSetActive(true);
                                obj4.CustomSetActive(false);
                            }
                            else
                            {
                                obj3.CustomSetActive(false);
                                obj4.CustomSetActive(false);
                            }
                        }
                    }
                }
                else
                {
                    gameObject.CustomSetActive(false);
                    obj3.CustomSetActive(false);
                    obj4.CustomSetActive(false);
                }
            }
        }

        public enum LobbyFormWidget
        {
            HeadImgBack = 3,
            LoudSpeakerRolling = 5,
            LoudSpeakerRollingBg = 6,
            None = -1,
            RankingBtn = 1,
            Reserve = 0,
            Rolling = 4,
            SnsHead = 2
        }
    }
}

