namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.UI;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    internal class SysEntryForm
    {
        public static string FORM_PATH = "UGUI/Form/System/Lobby/Form_Lobby_SysTray.prefab";
        private DictionaryView<int, GameObject> m_Btns;
        public GameObject m_FormObj;
        private Text m_lblDiamond;
        private Text m_lblDianquan;
        private Text m_lblGlodCoin;
        private Text m_ping;
        private GameObject m_SysEntry;
        private GameObject m_wifiIcon;
        private GameObject m_wifiInfo;
        public static uint s_CoinShowMaxValue = 0xf1b30;
        public static uint s_CoinShowStepValue = 0x2710;
        public static string[] s_netStateName = new string[] { "Net_1", "Net_2", "Net_3" };
        public static string s_noNetStateName = "NoNet";
        public static string[] s_wifiStateName = new string[] { "Wifi_1", "Wifi_2", "Wifi_3" };

        public void AddRedDot(enSysEntryID sysEntryId, enRedDotPos redDotPos, int count = 0)
        {
            if (this.m_Btns != null)
            {
                GameObject obj2;
                this.m_Btns.TryGetValue((int) sysEntryId, out obj2);
                CUICommonSystem.AddRedDot(obj2, redDotPos, count);
            }
        }

        public void AddRedDotEx(enSysEntryID sysEntryId, enRedDotPos redDotPos, int alertNum = 0)
        {
            if (this.m_Btns != null)
            {
                GameObject obj2;
                this.m_Btns.TryGetValue((int) sysEntryId, out obj2);
                CUICommonSystem.AddRedDot(obj2, redDotPos, alertNum);
            }
        }

        private bool checkIsHaveRedDot()
        {
            DictionaryView<int, GameObject>.Enumerator enumerator = this.m_Btns.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<int, GameObject> current = enumerator.Current;
                if (CUICommonSystem.IsHaveRedDot(current.Value))
                {
                    return true;
                }
            }
            return false;
        }

        public void CheckWifi()
        {
            if (((this.m_wifiIcon != null) && (this.m_wifiIcon != null)) && (this.m_ping != null))
            {
                int lobbyPing = (int) Singleton<NetworkModule>.GetInstance().lobbyPing;
                lobbyPing = (lobbyPing <= 100) ? lobbyPing : ((((lobbyPing - 100) * 7) / 10) + 100);
                lobbyPing = Mathf.Clamp(lobbyPing, 0, 460);
                uint index = 0;
                if (lobbyPing < 100)
                {
                    index = 2;
                }
                else if (lobbyPing < 200)
                {
                    index = 1;
                }
                else
                {
                    index = 0;
                }
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    CUICommonSystem.PlayAnimator(this.m_wifiIcon, s_noNetStateName);
                }
                else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
                {
                    CUICommonSystem.PlayAnimator(this.m_wifiIcon, s_wifiStateName[index]);
                }
                else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
                {
                    CUICommonSystem.PlayAnimator(this.m_wifiIcon, s_netStateName[index]);
                }
                if ((this.m_wifiInfo != null) && this.m_wifiInfo.activeInHierarchy)
                {
                    this.m_ping.text = lobbyPing + "ms";
                }
            }
        }

        public void DelRedDot(enSysEntryID sysEntryId)
        {
            if (this.m_Btns != null)
            {
                GameObject obj2;
                this.m_Btns.TryGetValue((int) sysEntryId, out obj2);
                CUICommonSystem.DelRedDot(obj2);
            }
        }

        public void FullShow()
        {
            if (this.m_FormObj != null)
            {
                this.m_FormObj.transform.Find("PlayerBtn/MailBtn").gameObject.CustomSetActive(true);
                this.m_FormObj.transform.Find("PlayerBtn/SettingBtn").gameObject.CustomSetActive(true);
                this.m_FormObj.transform.Find("PlayerBtn/FriendBtn").gameObject.CustomSetActive(true);
            }
        }

        public string GetCoinString(uint coinValue)
        {
            string str = coinValue.ToString();
            if (coinValue > s_CoinShowMaxValue)
            {
                int num = (int) (coinValue / s_CoinShowStepValue);
                str = string.Format("{0}万", num);
            }
            return str;
        }

        public void init()
        {
            Singleton<EventRouter>.instance.AddEventHandler("TaskUpdated", new Action(this, (IntPtr) this.OnTaskUpdate));
            Singleton<EventRouter>.instance.AddEventHandler("Friend_LobbyIconRedDot_Refresh", new Action(this, (IntPtr) this.OnFriendSysIconUpdate));
            Singleton<EventRouter>.instance.AddEventHandler(EventID.Mall_Entry_Add_RedDotCheck, new Action(this, (IntPtr) this.OnCheckAddMallEntryRedDot));
            Singleton<EventRouter>.instance.AddEventHandler(EventID.Mall_Entry_Del_RedDotCheck, new Action(this, (IntPtr) this.OnCheckDelMallEntryRedDot));
            Singleton<EventRouter>.instance.AddEventHandler("MailUnReadNumUpdate", new Action(this, (IntPtr) this.OnMailUnReadUpdate));
            Singleton<EventRouter>.instance.AddEventHandler(EventID.ACHIEVE_STATE_UPDATE, new Action(this, (IntPtr) this.OnAchieveStateUpdate));
            Singleton<EventRouter>.instance.AddEventHandler(EventID.SymbolEquipSuc, new Action(this, (IntPtr) this.OnCheckSymbolEquipAlert));
            Singleton<EventRouter>.instance.AddEventHandler(EventID.BAG_ITEMS_UPDATE, new Action(this, (IntPtr) this.OnCheckSymbolEquipAlert));
            Singleton<EventRouter>.instance.AddEventHandler("MasterPvpLevelChanged", new Action(this, (IntPtr) this.OnCheckSymbolEquipAlert));
            Singleton<EventRouter>.instance.AddEventHandler<bool>("Guild_Sign_State_Changed", new Action<bool>(this.OnGuildSignStateChanged));
            Singleton<ActivitySys>.GetInstance().OnStateChange += new ActivitySys.StateChangeDelegate(this.ValidateActivitySpot);
            Singleton<EventRouter>.instance.AddEventHandler("IDIPNOTICE_UNREAD_NUM_UPDATE", new Action(this, (IntPtr) this.ValidateActivitySpot));
        }

        private void initForm(CUIFormScript form)
        {
            this.m_FormObj = form.gameObject;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            this.m_lblGlodCoin = this.m_FormObj.transform.Find("PlayerBtn/GoldCoin/Text").GetComponent<Text>();
            this.m_lblDianquan = this.m_FormObj.transform.Find("PlayerBtn/Dianquan/Text").GetComponent<Text>();
            this.m_lblDiamond = this.m_FormObj.transform.Find("PlayerBtn/Diamond/Text").GetComponent<Text>();
            this.m_wifiIcon = form.GetWidget(0);
            this.m_wifiInfo = form.GetWidget(1);
            this.m_ping = form.GetWidget(2).GetComponent<Text>();
            this.m_lblGlodCoin.text = this.GetCoinString(masterRoleInfo.GoldCoin);
            this.m_lblDianquan.text = this.GetCoinString((uint) masterRoleInfo.DianQuan);
            this.m_lblDiamond.text = this.GetCoinString(masterRoleInfo.Diamond);
            GameObject gameObject = this.m_FormObj.transform.Find("PlayerBtn/GoldCoin").gameObject;
            if (gameObject != null)
            {
                CUIEventScript component = gameObject.GetComponent<CUIEventScript>();
                if (component == null)
                {
                    component = gameObject.AddComponent<CUIEventScript>();
                    component.Initialize(form);
                }
                CUseable useable = CUseableManager.CreateCoinUseable(RES_SHOPBUY_COINTYPE.RES_SHOPBUY_TYPE_PVPCOIN, (int) masterRoleInfo.GoldCoin);
                stUIEventParams eventParams = new stUIEventParams {
                    iconUseable = useable,
                    tag = 3
                };
                component.SetUIEvent(enUIEventType.Down, enUIEventID.Tips_ItemInfoOpen, eventParams);
                component.SetUIEvent(enUIEventType.HoldEnd, enUIEventID.Tips_ItemInfoClose, eventParams);
                component.SetUIEvent(enUIEventType.Click, enUIEventID.Tips_ItemInfoClose, eventParams);
                component.SetUIEvent(enUIEventType.DragEnd, enUIEventID.Tips_ItemInfoClose, eventParams);
            }
            GameObject obj3 = this.m_FormObj.transform.Find("PlayerBtn/Diamond").gameObject;
            if (obj3 != null)
            {
                CUIEventScript script2 = obj3.GetComponent<CUIEventScript>();
                if (script2 == null)
                {
                    script2 = obj3.AddComponent<CUIEventScript>();
                    script2.Initialize(form);
                }
                CUseable useable2 = CUseableManager.CreateCoinUseable(RES_SHOPBUY_COINTYPE.RES_SHOPBUY_TYPE_DIAMOND, (int) masterRoleInfo.Diamond);
                stUIEventParams params2 = new stUIEventParams {
                    iconUseable = useable2,
                    tag = 3
                };
                script2.SetUIEvent(enUIEventType.Down, enUIEventID.Tips_ItemInfoOpen, params2);
                script2.SetUIEvent(enUIEventType.HoldEnd, enUIEventID.Tips_ItemInfoClose, params2);
                script2.SetUIEvent(enUIEventType.Click, enUIEventID.Tips_ItemInfoClose, params2);
                script2.SetUIEvent(enUIEventType.DragEnd, enUIEventID.Tips_ItemInfoClose, params2);
            }
            if (!ApolloConfig.payEnabled)
            {
                Transform transform = this.m_FormObj.transform.Find("PlayerBtn/Dianquan/Button");
                if (transform != null)
                {
                    transform.gameObject.CustomSetActive(false);
                }
            }
            CUIFormScript script3 = Singleton<CUIManager>.GetInstance().GetForm(LobbyForm.FORM_PATH);
            this.m_SysEntry = script3.gameObject.transform.Find("LobbyBottom/SysEntry").gameObject;
            this.m_Btns = new DictionaryView<int, GameObject>();
            this.m_Btns.Add(0, this.m_SysEntry.transform.Find("HeroBtn").gameObject);
            this.m_Btns.Add(1, this.m_SysEntry.transform.Find("SymbolBtn").gameObject);
            this.m_Btns.Add(2, this.m_SysEntry.transform.Find("AchievementBtn").gameObject);
            this.m_Btns.Add(3, this.m_SysEntry.transform.Find("BagBtn").gameObject);
            this.m_Btns.Add(5, this.m_SysEntry.transform.Find("SocialBtn").gameObject);
            this.m_Btns.Add(6, form.transform.Find("PlayerBtn/FriendBtn").gameObject);
            this.m_Btns.Add(7, this.m_SysEntry.transform.Find("AddedSkillBtn").gameObject);
            this.m_Btns.Add(8, form.transform.Find("PlayerBtn/MailBtn").gameObject);
            this.m_Btns.Add(9, Utility.FindChild(script3.gameObject, "Popup/ActBtn"));
            this.m_Btns.Add(10, Utility.FindChild(script3.gameObject, "Popup/BoardBtn"));
            this.m_Btns.Add(4, script3.gameObject.transform.Find("LobbyBottom/Newbie/RedDotPanel").gameObject);
        }

        public void MiniShow()
        {
            if (this.m_FormObj != null)
            {
                this.m_FormObj.transform.Find("PlayerBtn/MailBtn").gameObject.CustomSetActive(false);
                this.m_FormObj.transform.Find("PlayerBtn/SettingBtn").gameObject.CustomSetActive(false);
                this.m_FormObj.transform.Find("PlayerBtn/FriendBtn").gameObject.CustomSetActive(false);
            }
        }

        private void OnAchieveStateUpdate()
        {
            CAchieveInfo achieveInfo = CAchieveInfo.GetAchieveInfo();
            if (achieveInfo != null)
            {
                if (achieveInfo.IsHaveFinishButNotGetRewardAchievement(0))
                {
                    Singleton<LobbyUISys>.GetInstance().AddRedDot(enSysEntryID.AchievementBtn, enRedDotPos.enTopRight);
                }
                else
                {
                    Singleton<LobbyUISys>.GetInstance().DelRedDot(enSysEntryID.AchievementBtn);
                }
            }
        }

        private void OnCheckAddMallEntryRedDot()
        {
            if ((((CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_HeroTab) || CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_HeroSkinTab)) || (CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_SymbolTab) || CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_SaleTab))) || ((CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_LotteryTab) || CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_RecommendTab)) || (CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_MysteryTab) && CUIRedDotSystem.IsShowRedDotByLogic(enRedID.Mall_MysteryTab)))) || (CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_BoutiqueTab) || CUIRedDotSystem.IsShowRedDotByLogic(enRedID.Mall_SymbolTab)))
            {
                this.AddRedDot(enSysEntryID.MallBtn, enRedDotPos.enTopRight, 0);
            }
        }

        private void OnCheckDelMallEntryRedDot()
        {
            if ((((!CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_HeroTab) && !CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_HeroSkinTab)) && (!CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_SymbolTab) && !CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_SaleTab))) && ((!CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_LotteryTab) && !CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_RecommendTab)) && (!CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_MysteryTab) || !CUIRedDotSystem.IsShowRedDotByLogic(enRedID.Mall_MysteryTab)))) && (!CUIRedDotSystem.IsShowRedDotByVersion(enRedID.Mall_BoutiqueTab) && !CUIRedDotSystem.IsShowRedDotByLogic(enRedID.Mall_SymbolTab)))
            {
                Singleton<LobbyUISys>.instance.DelRedDot(enSysEntryID.MallBtn);
            }
        }

        private void OnCheckSymbolEquipAlert()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                int num;
                uint num2;
                if (masterRoleInfo.m_symbolInfo.CheckAnyWearSymbol(out num, out num2))
                {
                    Singleton<LobbyUISys>.GetInstance().AddRedDot(enSysEntryID.SymbolBtn, enRedDotPos.enTopRight);
                }
                else
                {
                    Singleton<LobbyUISys>.GetInstance().DelRedDot(enSysEntryID.SymbolBtn);
                }
            }
        }

        private void OnCheckUpdateClientVersion()
        {
            if (Singleton<LobbyLogic>.instance.NeedUpdateClient)
            {
                Singleton<LobbyLogic>.instance.NeedUpdateClient = false;
                Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("VersionNeedUpdate"), enUIEventID.Lobby_ConfirmErrExit, enUIEventID.None, false);
            }
        }

        private void OnFriendSysIconUpdate()
        {
            if (Singleton<CFriendContoller>.GetInstance().model.GetDataCount(CFriendModel.FriendType.RequestFriend) > 0)
            {
                Singleton<LobbyUISys>.GetInstance().AddRedDot(enSysEntryID.FriendBtn, enRedDotPos.enTopRight);
            }
            else
            {
                Singleton<LobbyUISys>.GetInstance().DelRedDot(enSysEntryID.FriendBtn);
            }
        }

        private void OnGuildSignStateChanged(bool isSigned)
        {
            if (isSigned)
            {
                Singleton<LobbyUISys>.GetInstance().DelRedDot(enSysEntryID.GuildBtn);
            }
            else
            {
                Singleton<LobbyUISys>.GetInstance().AddRedDot(enSysEntryID.GuildBtn, enRedDotPos.enTopRight);
            }
        }

        private void OnMailUnReadUpdate()
        {
            int unReadMailCount = Singleton<CMailSys>.instance.UnReadMailCount;
            if (this.m_FormObj != null)
            {
                if (unReadMailCount > 0)
                {
                    Singleton<LobbyUISys>.GetInstance().AddRedDot(enSysEntryID.MailBtn, enRedDotPos.enTopRight);
                }
                else
                {
                    Singleton<LobbyUISys>.GetInstance().DelRedDot(enSysEntryID.MailBtn);
                }
            }
        }

        private void OnTaskUpdate()
        {
            CTaskModel model = Singleton<CTaskSys>.instance.model;
            model.task_Data.Sort(RES_TASK_TYPE.RES_TASKTYPE_HEROWAKE);
            model.task_Data.Sort(RES_TASK_TYPE.RES_TASKTYPE_USUAL);
            model.task_Data.Sort(RES_TASK_TYPE.RES_TASKTYPE_MAIN);
            int count = Singleton<CTaskSys>.instance.model.task_Data.GetTask_Count(RES_TASK_TYPE.RES_TASKTYPE_MAIN, CTask.State.Have_Done) + Singleton<CTaskSys>.instance.model.task_Data.GetTask_Count(RES_TASK_TYPE.RES_TASKTYPE_USUAL, CTask.State.Have_Done);
            if (count > 0)
            {
                this.AddRedDot(enSysEntryID.TaskBtn, enRedDotPos.enTopRight, count);
            }
            else
            {
                this.DelRedDot(enSysEntryID.TaskBtn);
            }
        }

        public void OpenForm()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().OpenForm(FORM_PATH, false, true);
            this.m_FormObj = form.gameObject;
            this.initForm(form);
            this.OnFriendSysIconUpdate();
            this.OnTaskUpdate();
            this.ValidateActivitySpot();
            this.OnMailUnReadUpdate();
            this.OnCheckSymbolEquipAlert();
            this.OnCheckUpdateClientVersion();
            Singleton<EventRouter>.instance.BroadCastEvent(EventID.Mall_Entry_Add_RedDotCheck);
            Singleton<EventRouter>.instance.BroadCastEvent(EventID.Mall_Set_Free_Draw_Timer);
            Singleton<CMiShuSystem>.instance.CheckMiShuTalk(true);
            Singleton<CMiShuSystem>.instance.OnCheckFirstWin(null);
            Singleton<CMiShuSystem>.instance.CheckActPlayModeTipsForLobby();
            Singleton<CMiShuSystem>.instance.ShowNewFlagForBeizhanEntry();
        }

        public void ShowOrHideWifiInfo()
        {
            this.m_wifiInfo.CustomSetActive(!this.m_wifiInfo.activeInHierarchy);
            this.CheckWifi();
        }

        public void unInit()
        {
            Singleton<EventRouter>.instance.RemoveEventHandler("TaskUpdated", new Action(this, (IntPtr) this.OnTaskUpdate));
            Singleton<EventRouter>.instance.RemoveEventHandler("Friend_LobbyIconRedDot_Refresh", new Action(this, (IntPtr) this.OnFriendSysIconUpdate));
            Singleton<EventRouter>.instance.RemoveEventHandler(EventID.Mall_Entry_Add_RedDotCheck, new Action(this, (IntPtr) this.OnCheckAddMallEntryRedDot));
            Singleton<EventRouter>.instance.RemoveEventHandler(EventID.Mall_Entry_Del_RedDotCheck, new Action(this, (IntPtr) this.OnCheckDelMallEntryRedDot));
            Singleton<EventRouter>.instance.RemoveEventHandler("MailUnReadNumUpdate", new Action(this, (IntPtr) this.OnMailUnReadUpdate));
            Singleton<EventRouter>.instance.RemoveEventHandler(EventID.ACHIEVE_STATE_UPDATE, new Action(this, (IntPtr) this.OnAchieveStateUpdate));
            Singleton<EventRouter>.instance.RemoveEventHandler(EventID.SymbolEquipSuc, new Action(this, (IntPtr) this.OnCheckSymbolEquipAlert));
            Singleton<EventRouter>.instance.RemoveEventHandler(EventID.BAG_ITEMS_UPDATE, new Action(this, (IntPtr) this.OnCheckSymbolEquipAlert));
            Singleton<EventRouter>.instance.RemoveEventHandler("MasterPvpLevelChanged", new Action(this, (IntPtr) this.OnCheckSymbolEquipAlert));
            Singleton<ActivitySys>.GetInstance().OnStateChange -= new ActivitySys.StateChangeDelegate(this.ValidateActivitySpot);
            Singleton<EventRouter>.instance.RemoveEventHandler("IDIPNOTICE_UNREAD_NUM_UPDATE", new Action(this, (IntPtr) this.ValidateActivitySpot));
        }

        public void UpdatePlayerData()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            if (this.m_lblGlodCoin != null)
            {
                this.m_lblGlodCoin.text = this.GetCoinString(masterRoleInfo.GoldCoin);
            }
            if (this.m_lblDianquan != null)
            {
                this.m_lblDianquan.text = this.GetCoinString((uint) masterRoleInfo.DianQuan);
            }
            if (this.m_lblDiamond != null)
            {
                this.m_lblDiamond.text = this.GetCoinString(masterRoleInfo.Diamond);
            }
        }

        public void ValidateActivitySpot()
        {
            if (this.m_FormObj != null)
            {
                if (Singleton<ActivitySys>.GetInstance().CheckReadyForDot(RES_WEAL_ENTRANCE_TYPE.RES_WEAL_ENTRANCE_TYPE_ACTIVITY))
                {
                    uint reveivableRedDot = Singleton<ActivitySys>.GetInstance().GetReveivableRedDot(RES_WEAL_ENTRANCE_TYPE.RES_WEAL_ENTRANCE_TYPE_ACTIVITY);
                    Singleton<LobbyUISys>.GetInstance().AddRedDotEx(enSysEntryID.ActivityBtn, enRedDotPos.enTopRight, (int) reveivableRedDot);
                }
                else if (MonoSingleton<IDIPSys>.GetInstance().HaveUpdateList)
                {
                    Singleton<LobbyUISys>.GetInstance().AddRedDotEx(enSysEntryID.ActivityBtn, enRedDotPos.enTopRight, 0);
                }
                else
                {
                    Singleton<LobbyUISys>.GetInstance().DelRedDot(enSysEntryID.ActivityBtn);
                }
            }
        }

        public enum enSysEntryFormWidget
        {
            WifiIcon,
            WifiInfo,
            WifiPing,
            GlodCoin,
            Dianquan,
            MailBtn,
            SettingBtn,
            Wifi_Bg
        }
    }
}

