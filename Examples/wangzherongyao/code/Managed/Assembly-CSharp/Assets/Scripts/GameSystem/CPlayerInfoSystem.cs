namespace Assets.Scripts.GameSystem
{
    using Apollo;
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    [MessageHandlerClass]
    public class CPlayerInfoSystem : Singleton<CPlayerInfoSystem>
    {
        private bool _isShowGuildAppointViceChairmanBtn;
        private bool _isShowGuildFireMemberBtn;
        private bool _isShowGuildTransferPositionBtn;
        private DetailPlayerInfoSource _lastDetailSource;
        public const ushort CREDIT_RULE_ID = 11;
        public bool isShowPlayerInfo = true;
        private Tab m_CurTab;
        private CUIFormScript m_Form;
        private bool m_IsFormOpen;
        private CPlayerProfile m_PlayerProfile = new CPlayerProfile();
        public const ushort PlAYER_INFO_RULE_ID = 3;
        public string sPlayerInfoFormPath = "UGUI/Form/System/Player/Form_Player_Info.prefab";

        private void DeepLinkClick(CUIEvent uiEvent)
        {
            if ((ApolloConfig.platform == ApolloPlatform.Wechat) && MonoSingleton<BannerImageSys>.GetInstance().DeepLinkInfo.bLoadSucc)
            {
                Debug.Log(string.Concat(new object[] { "deeplink ", MonoSingleton<BannerImageSys>.GetInstance().DeepLinkInfo.linkType, " ", MonoSingleton<BannerImageSys>.GetInstance().DeepLinkInfo.linkUrl }));
                Singleton<ApolloHelper>.GetInstance().OpenWeiXinDeeplink(MonoSingleton<BannerImageSys>.GetInstance().DeepLinkInfo.linkType, MonoSingleton<BannerImageSys>.GetInstance().DeepLinkInfo.linkUrl);
            }
        }

        private void DisplayCustomButton()
        {
            if (this.m_IsFormOpen && (this.m_Form != null))
            {
                GameObject widget = this.m_Form.GetWidget(1);
                if ((widget != null) && widget.activeSelf)
                {
                    GameObject obj3 = Utility.FindChild(widget, "pnlContainer/pnlHead/btnRename");
                    GameObject obj4 = Utility.FindChild(widget, "pnlContainer/pnlHead/btnShare");
                    switch (this._lastDetailSource)
                    {
                        case DetailPlayerInfoSource.DefaultOthers:
                            obj3.CustomSetActive(false);
                            obj4.CustomSetActive(false);
                            this.SetAllGuildBtnActive(widget, false);
                            this.SetAllFriendBtnActive(widget, false);
                            break;

                        case DetailPlayerInfoSource.Self:
                            obj3.CustomSetActive(false);
                            obj4.CustomSetActive(false);
                            this.SetAllGuildBtnActive(widget, false);
                            this.SetAllFriendBtnActive(widget, false);
                            break;

                        case DetailPlayerInfoSource.Guild:
                            obj3.CustomSetActive(false);
                            obj4.CustomSetActive(false);
                            this.SetSingleGuildBtnActive(widget);
                            this.SetAllFriendBtnActive(widget, false);
                            break;

                        case DetailPlayerInfoSource.Friend:
                            obj3.CustomSetActive(false);
                            obj4.CustomSetActive(false);
                            this.SetAllGuildBtnActive(widget, false);
                            this.SetAllFriendBtnActive(widget, true);
                            break;
                    }
                }
            }
        }

        public void ImpResDetailInfo(CSPkg msg)
        {
            if (msg.stPkgData.stGetAcntDetailInfoRsp.iErrCode != 0)
            {
                Singleton<CUIManager>.GetInstance().OpenMessageBox(string.Format("Error Code {0}", msg.stPkgData.stGetAcntDetailInfoRsp.iErrCode), false);
            }
            else
            {
                this.m_PlayerProfile.ConvertServerDetailData(msg.stPkgData.stGetAcntDetailInfoRsp.stAcntDetail.stOfSucc);
                this.OpenForm();
            }
        }

        public override void Init()
        {
            base.Init();
            CUIEventManager instance = Singleton<CUIEventManager>.GetInstance();
            instance.AddUIEventListener(enUIEventID.Player_Info_OpenForm, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfo_OpenForm));
            instance.AddUIEventListener(enUIEventID.Player_Info_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfo_CloseForm));
            instance.AddUIEventListener(enUIEventID.Player_Info_Tab_Change, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoTabChange));
            instance.AddUIEventListener(enUIEventID.Player_Info_Open_Pvp_Info, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoOpenPvpInfo));
            instance.AddUIEventListener(enUIEventID.Player_Info_Open_Base_Info, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoOpenBaseInfo));
            instance.AddUIEventListener(enUIEventID.Player_Info_Quit_Game, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoQuitGame));
            instance.AddUIEventListener(enUIEventID.Player_Info_Quit_Game_Confirm, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoQuitGameConfirm));
            instance.AddUIEventListener(enUIEventID.Player_Info_Most_Used_Hero_Item_Enable, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoMostUsedHeroItemEnable));
            instance.AddUIEventListener(enUIEventID.Player_Info_Most_Used_Hero_Item_Click, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoMostUsedHeroItemClick));
            instance.AddUIEventListener(enUIEventID.Player_Info_Show_Rule, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoShowRule));
            instance.AddUIEventListener(enUIEventID.Player_Info_License_ListItem_Enable, new CUIEventManager.OnUIEventHandler(this.OnLicenseListItemEnable));
            instance.AddUIEventListener(enUIEventID.Player_Info_Common_Hero_Enable, new CUIEventManager.OnUIEventHandler(this.OnCommonHeroItemEnable));
            this.m_IsFormOpen = false;
            this.m_CurTab = Tab.Base_Info;
            this.m_Form = null;
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BuyPick_QQ_VIP, new CUIEventManager.OnUIEventHandler(this.OpenByQQVIP));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.DeepLink_OnClick, new CUIEventManager.OnUIEventHandler(this.DeepLinkClick));
            Singleton<EventRouter>.GetInstance().AddEventHandler(EventID.NOBE_STATE_CHANGE, new Action(this, (IntPtr) this.UpdateNobeHeadIdx));
            Singleton<EventRouter>.GetInstance().AddEventHandler(EventID.HEAD_IMAGE_FLAG_CHANGE, new Action(this, (IntPtr) this.UpdateHeadFlag));
            Singleton<EventRouter>.GetInstance().AddEventHandler(EventID.NAMECHANGE_PLAYER_NAME_CHANGE, new Action(this, (IntPtr) this.OnPlayerNameChange));
        }

        private void InitTab()
        {
            if ((this.m_Form != null) && this.m_IsFormOpen)
            {
                GameObject widget = this.m_Form.GetWidget(1);
                if ((widget != null) && widget.activeSelf)
                {
                    widget.CustomSetActive(false);
                }
                GameObject obj3 = this.m_Form.GetWidget(2);
                if ((obj3 != null) && obj3.activeSelf)
                {
                    obj3.CustomSetActive(false);
                }
                Tab[] values = (Tab[]) Enum.GetValues(typeof(Tab));
                string[] strArray = new string[values.Length];
                for (byte i = 0; i < values.Length; i = (byte) (i + 1))
                {
                    switch (values[i])
                    {
                        case Tab.Base_Info:
                            strArray[i] = Singleton<CTextManager>.GetInstance().GetText("Player_Info_Tab_Base_Info");
                            break;

                        case Tab.Pvp_Info:
                            strArray[i] = Singleton<CTextManager>.GetInstance().GetText("Player_Info_Tab_Pvp_Info");
                            break;

                        case Tab.Common_Hero:
                            strArray[i] = Singleton<CTextManager>.GetInstance().GetText("Player_Info_Tab_Common_Hero_Info");
                            break;

                        case Tab.PvpCreditScore_Info:
                            strArray[i] = Singleton<CTextManager>.GetInstance().GetText("Player_Info_Tab_Credit_Info");
                            break;
                    }
                }
                CUIListScript component = this.m_Form.GetWidget(0).GetComponent<CUIListScript>();
                if (component != null)
                {
                    component.SetElementAmount(strArray.Length);
                    for (int j = 0; j < component.m_elementAmount; j++)
                    {
                        component.GetElemenet(j).gameObject.transform.Find("Text").GetComponent<Text>().text = strArray[j];
                    }
                    component.m_alwaysDispatchSelectedChangeEvent = true;
                    component.SelectElement((int) this.CurTab, true);
                }
            }
        }

        private bool isSelf(ulong playerUllUID)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            return ((masterRoleInfo != null) && (masterRoleInfo.playerUllUID == playerUllUID));
        }

        private void OnCommonHeroItemEnable(CUIEvent uiEvent)
        {
            int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
            GameObject p = Utility.FindChild(uiEvent.m_srcWidget, "heroItem");
            ListView<COMDT_MOST_USED_HERO_INFO> view = this.m_PlayerProfile.MostUsedHeroList();
            if ((view != null) && (srcWidgetIndexInBelongedList < view.Count))
            {
                COMDT_MOST_USED_HERO_INFO comdt_most_used_hero_info = view[srcWidgetIndexInBelongedList];
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                if (masterRoleInfo != null)
                {
                    IHeroData data = CHeroDataFactory.CreateHeroData(comdt_most_used_hero_info.dwHeroID);
                    GameObject proficiencyIcon = Utility.FindChild(p, "heroProficiencyImg");
                    GameObject proficiencyBg = Utility.FindChild(p, "heroProficiencyBgImg");
                    CUICommonSystem.SetHeroProficiencyIconImage(uiEvent.m_srcFormScript, proficiencyIcon, (int) comdt_most_used_hero_info.dwProficiencyLv);
                    CUICommonSystem.SetHeroProficiencyBgImage(uiEvent.m_srcFormScript, proficiencyBg, (int) comdt_most_used_hero_info.dwProficiencyLv, false);
                    CUICommonSystem.SetHeroItemImage(uiEvent.m_srcFormScript, p, masterRoleInfo.GetHeroSkinPic(comdt_most_used_hero_info.dwHeroID), enHeroHeadType.enBust, false);
                    GameObject root = Utility.FindChild(p, "profession");
                    CUICommonSystem.SetHeroJob(uiEvent.m_srcFormScript, root, (enHeroJobType) data.heroType);
                    Utility.GetComponetInChild<Text>(p, "heroNameText").text = data.heroName;
                    string[] args = new string[] { (comdt_most_used_hero_info.dwGameWinNum + comdt_most_used_hero_info.dwGameLoseNum).ToString() };
                    Utility.GetComponetInChild<Text>(p, "TotalCount").text = Singleton<CTextManager>.instance.GetText("Player_Info_PVP_Total_Count", args);
                    string[] textArray2 = new string[] { CPlayerProfile.Round(CPlayerProfile.Divide(comdt_most_used_hero_info.dwGameWinNum, comdt_most_used_hero_info.dwGameWinNum + comdt_most_used_hero_info.dwGameLoseNum) * 100f) };
                    Utility.GetComponetInChild<Text>(p, "WinRate").text = Singleton<CTextManager>.instance.GetText("Player_Info_PVP_Win_Rate", textArray2);
                }
            }
        }

        private void OnLicenseListItemEnable(CUIEvent uiEvent)
        {
            int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
            GameObject srcWidget = uiEvent.m_srcWidget;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if ((masterRoleInfo != null) && (masterRoleInfo.m_licenseInfo != null))
            {
                CLicenseItem licenseItemByIndex = masterRoleInfo.m_licenseInfo.GetLicenseItemByIndex(srcWidgetIndexInBelongedList);
                if (((srcWidget != null) && (licenseItemByIndex != null)) && (licenseItemByIndex.m_resLicenseInfo != null))
                {
                    srcWidget.transform.Find("licenseIcon").GetComponent<Image>().SetSprite(string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Task_Dir, licenseItemByIndex.m_resLicenseInfo.szIconPath), this.m_Form, true, false, false);
                    srcWidget.transform.Find("licenseNameText").GetComponent<Text>().text = licenseItemByIndex.m_resLicenseInfo.szLicenseName;
                    Transform transform3 = srcWidget.transform.Find("licenseStateText");
                    if (licenseItemByIndex.m_getSecond > 0)
                    {
                        DateTime time = Utility.ToUtcTime2Local((long) licenseItemByIndex.m_getSecond);
                        transform3.GetComponent<Text>().text = string.Format("<color=#00d519>{0}/{1}/{2}</color>", time.Year, time.Month, time.Day);
                    }
                    else
                    {
                        transform3.GetComponent<Text>().text = "<color=#fecb2f>未获得</color>";
                    }
                    srcWidget.transform.Find("licenseDescText").GetComponent<Text>().text = licenseItemByIndex.m_resLicenseInfo.szDesc;
                }
            }
        }

        private void OnPlayerInfo_CloseForm(CUIEvent uiEvent)
        {
            if (this.m_IsFormOpen)
            {
                this.m_IsFormOpen = false;
                Singleton<CUIManager>.GetInstance().CloseForm(this.sPlayerInfoFormPath);
                this.m_Form = null;
            }
        }

        private void OnPlayerInfo_OpenForm(CUIEvent uiEvent)
        {
            this.ShowPlayerDetailInfo(0L, 0, DetailPlayerInfoSource.Self);
        }

        private void OnPlayerInfoMostUsedHeroItemClick(CUIEvent uiEvent)
        {
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Player_Info_CloseForm, uiEvent.m_eventParams);
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.HeroInfo_OpenForm, uiEvent.m_eventParams);
        }

        private void OnPlayerInfoMostUsedHeroItemEnable(CUIEvent uiEvent)
        {
            int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
            GameObject srcWidget = uiEvent.m_srcWidget;
            if (srcWidget != null)
            {
                GameObject gameObject = srcWidget.transform.Find("heroItem").gameObject;
                ListView<COMDT_MOST_USED_HERO_INFO> view = this.m_PlayerProfile.MostUsedHeroList();
                if (srcWidgetIndexInBelongedList < view.Count)
                {
                    COMDT_MOST_USED_HERO_INFO heroInfo = view[srcWidgetIndexInBelongedList];
                    this.SetHeroItemData(uiEvent.m_srcFormScript, gameObject, heroInfo);
                    Text componetInChild = Utility.GetComponetInChild<Text>(srcWidget, "usedCnt");
                    if (componetInChild != null)
                    {
                        componetInChild.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Games_Cnt_Label"), heroInfo.dwGameWinNum + heroInfo.dwGameLoseNum);
                    }
                }
            }
        }

        private void OnPlayerInfoOpenBaseInfo(CUIEvent uiEvent)
        {
            this.OpenBaseInfo();
        }

        private void OnPlayerInfoOpenPvpInfo(CUIEvent uiEvent)
        {
            this.OpenPvpInfo();
        }

        private void OnPlayerInfoQuitGame(CUIEvent uiEvent)
        {
            Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("Common_QuitGameTips"), enUIEventID.Player_Info_Quit_Game_Confirm, enUIEventID.None, false);
        }

        private void OnPlayerInfoQuitGameConfirm(CUIEvent uiEvent)
        {
            SGameApplication.Quit();
        }

        private void OnPlayerInfoShowRule(CUIEvent uiEvent)
        {
            ResRuleText dataByKey = GameDataMgr.s_ruleTextDatabin.GetDataByKey(3);
            if (dataByKey != null)
            {
                string title = StringHelper.UTF8BytesToString(ref dataByKey.szTitle);
                string info = StringHelper.UTF8BytesToString(ref dataByKey.szContent);
                Singleton<CUIManager>.GetInstance().OpenInfoForm(title, info);
            }
        }

        private void OnPlayerInfoTabChange(CUIEvent uiEvent)
        {
            if ((this.m_Form != null) && this.m_IsFormOpen)
            {
                int selectedIndex = uiEvent.m_srcWidget.GetComponent<CUIListScript>().GetSelectedIndex();
                this.CurTab = (Tab) selectedIndex;
                if (this.m_Form != null)
                {
                    GameObject widget = this.m_Form.GetWidget(1);
                    GameObject obj3 = this.m_Form.GetWidget(2);
                    GameObject obj4 = this.m_Form.GetWidget(5);
                    GameObject obj5 = this.m_Form.GetWidget(4);
                    GameObject obj6 = this.m_Form.GetWidget(7);
                    GameObject obj7 = this.m_Form.GetWidget(8);
                    switch (this.m_CurTab)
                    {
                        case Tab.Base_Info:
                            if ((widget != null) && !widget.activeSelf)
                            {
                                widget.CustomSetActive(true);
                                if (obj3 != null)
                                {
                                    obj3.CustomSetActive(false);
                                }
                                if (obj4 != null)
                                {
                                    obj4.CustomSetActive(false);
                                }
                                if (obj5 != null)
                                {
                                    obj5.CustomSetActive(false);
                                }
                                if (obj6 != null)
                                {
                                    obj6.CustomSetActive(false);
                                }
                                if (obj7 != null)
                                {
                                    obj7.CustomSetActive(true);
                                }
                                this.UpdateBaseInfo();
                                this.ProcessQQVIP(this.m_Form, true);
                                this.ProcessNobeHeadIDx(this.m_Form, true);
                            }
                            break;

                        case Tab.Pvp_Info:
                            if ((((widget != null) && (obj3 != null)) && ((obj4 != null) && (obj5 != null))) && (((obj6 != null) && (obj7 != null)) && !obj3.activeSelf))
                            {
                                widget.CustomSetActive(false);
                                obj4.CustomSetActive(false);
                                obj5.CustomSetActive(false);
                                obj7.CustomSetActive(true);
                                obj3.CustomSetActive(true);
                                obj6.CustomSetActive(false);
                                this.UpdatePvpInfo();
                                this.ProcessQQVIP(this.m_Form, false);
                                this.ProcessNobeHeadIDx(this.m_Form, false);
                            }
                            break;

                        case Tab.Common_Hero:
                            if ((((widget != null) && (obj3 != null)) && ((obj4 != null) && (obj5 != null))) && (((obj6 != null) && (obj7 != null)) && !obj6.activeSelf))
                            {
                                widget.CustomSetActive(false);
                                obj4.CustomSetActive(false);
                                obj5.CustomSetActive(false);
                                obj3.CustomSetActive(false);
                                obj7.CustomSetActive(true);
                                obj6.CustomSetActive(true);
                                this.UpdateCommonHeroList();
                            }
                            break;

                        case Tab.PvpCreditScore_Info:
                            if ((((widget != null) && (obj3 != null)) && ((obj4 != null) && (obj5 != null))) && (((obj6 != null) && (obj7 != null)) && !obj5.activeSelf))
                            {
                                widget.CustomSetActive(false);
                                obj4.CustomSetActive(false);
                                obj3.CustomSetActive(false);
                                obj7.CustomSetActive(false);
                                obj5.CustomSetActive(true);
                                obj6.CustomSetActive(false);
                                this.RequestCreditScore();
                                this.UpdatePlayerInfo();
                            }
                            break;
                    }
                }
            }
        }

        private void OnPlayerNameChange()
        {
            if (this.m_IsFormOpen && (this.m_Form != null))
            {
                GameObject widget = this.m_Form.GetWidget(1);
                if (widget != null)
                {
                    Text componetInChild = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlHead/NameGroup/txtName");
                    CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                    if ((componetInChild != null) && (masterRoleInfo != null))
                    {
                        componetInChild.text = masterRoleInfo.Name;
                    }
                }
            }
        }

        public void OpenBaseInfo()
        {
            this.ShowPlayerDetailInfo(0L, 0, DetailPlayerInfoSource.Self);
        }

        private void OpenByQQVIP(CUIEvent uiEvent)
        {
            if ((ApolloConfig.platform == ApolloPlatform.QQ) || (ApolloConfig.platform == ApolloPlatform.WTLogin))
            {
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                if (masterRoleInfo != null)
                {
                    if (masterRoleInfo.HasVip(0x10))
                    {
                        Singleton<ApolloHelper>.GetInstance().PayQQVip("CJCLUBT", Singleton<CTextManager>.GetInstance().GetText("QQ_Vip_XuFei_Super_Vip"), 1);
                    }
                    else if (masterRoleInfo.HasVip(1))
                    {
                        Singleton<ApolloHelper>.GetInstance().PayQQVip("LTMCLUB", Singleton<CTextManager>.GetInstance().GetText("QQ_Vip_XuFei_Vip"), 1);
                    }
                    else
                    {
                        Singleton<ApolloHelper>.GetInstance().PayQQVip("LTMCLUB", Singleton<CTextManager>.GetInstance().GetText("QQ_Vip_Buy_Vip"), 1);
                    }
                }
            }
        }

        public void OpenForm()
        {
            if (!this.m_IsFormOpen)
            {
                this.m_IsFormOpen = true;
                this.m_Form = Singleton<CUIManager>.GetInstance().OpenForm(this.sPlayerInfoFormPath, true, true);
                this.CurTab = Tab.Base_Info;
                this.InitTab();
            }
        }

        public void OpenPvpInfo()
        {
            this.ShowPlayerDetailInfo(0L, 0, DetailPlayerInfoSource.Self);
        }

        public void ProcessCommonQQVip(GameObject parent)
        {
            if (parent != null)
            {
                GameObject gameObject = parent.transform.FindChild("QQSVipIcon").gameObject;
                parent.transform.FindChild("QQVipIcon").gameObject.CustomSetActive(false);
                gameObject.CustomSetActive(false);
            }
        }

        private void ProcessNobeHeadIDx(CUIFormScript form, bool bshow)
        {
            GameObject obj2 = Utility.FindChild(form.gameObject, "pnlBg/pnlBody/pnlBaseInfo/pnlContainer/pnlHead/changeNobeheadicon");
            if (!this.isSelf(this.m_PlayerProfile.m_uuid))
            {
                obj2.CustomSetActive(false);
            }
            else
            {
                if (CSysDynamicBlock.bVipBlock)
                {
                    bshow = false;
                }
                if (bshow)
                {
                    obj2.CustomSetActive(true);
                }
                else
                {
                    obj2.CustomSetActive(false);
                }
            }
        }

        private void ProcessQQVIP(CUIFormScript form, bool bShow)
        {
            if (form != null)
            {
                GameObject obj2 = Utility.FindChild(form.gameObject, "pnlBg/pnlBody/pnlBaseInfo/pnlContainer/BtnGroup/QQVIPBtn");
                GameObject obj3 = Utility.FindChild(form.gameObject, "pnlBg/pnlBody/pnlBaseInfo/pnlContainer/pnlHead/NameGroup/QQVipIcon");
                if (!this.isSelf(this.m_PlayerProfile.m_uuid))
                {
                    obj2.CustomSetActive(false);
                    MonoSingleton<NobeSys>.GetInstance().SetOtherQQVipHead(obj3.GetComponent<Image>(), (int) this.m_PlayerProfile.qqVipMask);
                }
                else
                {
                    GameObject obj4 = Utility.FindChild(form.gameObject, "pnlBg/pnlBody/pnlBaseInfo/pnlContainer/BtnGroup/QQVIPBtn/Text");
                    if (!bShow)
                    {
                        obj2.CustomSetActive(false);
                        obj3.CustomSetActive(false);
                        obj4.CustomSetActive(false);
                    }
                    else
                    {
                        if (ApolloConfig.platform == ApolloPlatform.QQ)
                        {
                            obj2.CustomSetActive(true);
                        }
                        else
                        {
                            obj2.CustomSetActive(false);
                        }
                        obj4.CustomSetActive(true);
                        obj3.CustomSetActive(false);
                        if (CSysDynamicBlock.bLobbyEntryBlocked)
                        {
                            obj2.CustomSetActive(false);
                            obj3.CustomSetActive(false);
                            obj4.CustomSetActive(false);
                        }
                        else
                        {
                            if (ApolloConfig.platform == ApolloPlatform.QQ)
                            {
                                obj2.CustomSetActive(true);
                            }
                            else
                            {
                                obj2.CustomSetActive(false);
                            }
                            obj4.GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("QQ_Vip_Buy_Vip");
                            if (Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo() != null)
                            {
                                MonoSingleton<NobeSys>.GetInstance().SetMyQQVipHead(obj3.GetComponent<Image>());
                            }
                        }
                    }
                }
            }
        }

        [MessageHandler(0x1457)]
        public static void ReciveCreditScoreInfo(CSPkg msg)
        {
            Singleton<CUIManager>.instance.CloseSendMsgAlert();
            uint dwCreditValue = msg.stPkgData.stNtfAcntCreditValue.dwCreditValue;
            Singleton<CPlayerInfoSystem>.GetInstance().UpdateCreditScore(dwCreditValue);
        }

        private void RefreshLicenseInfoPanel(CUIFormScript form)
        {
            if (null != form)
            {
                GameObject widget = form.GetWidget(6);
                if (null != widget)
                {
                    CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                    if ((masterRoleInfo != null) && (masterRoleInfo.m_licenseInfo != null))
                    {
                        CUIListScript component = widget.GetComponent<CUIListScript>();
                        if (component != null)
                        {
                            component.SetElementAmount(masterRoleInfo.m_licenseInfo.m_licenseList.Count);
                        }
                    }
                }
            }
        }

        public void ReqOtherPlayerDetailInfo(ulong ullUid, int iLogicWorldId, bool showInfo = true, bool isShowAlert = true)
        {
            if (ullUid > 0L)
            {
                this.isShowPlayerInfo = showInfo;
                CSPkg msg = NetworkModule.CreateDefaultCSPKG(0xa2e);
                msg.stPkgData.stGetAcntDetailInfoReq.ullUid = ullUid;
                msg.stPkgData.stGetAcntDetailInfoReq.iLogicWorldId = iLogicWorldId;
                Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, isShowAlert);
            }
        }

        public void RequestCreditScore()
        {
            Singleton<CUIManager>.GetInstance().OpenSendMsgAlert(5, enUIEventID.None);
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x1456);
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
        }

        [MessageHandler(0xa2f)]
        public static void ResPlyaerDetailInfo(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            if (Singleton<CPlayerInfoSystem>.GetInstance().isShowPlayerInfo)
            {
                Singleton<CPlayerInfoSystem>.instance.ImpResDetailInfo(msg);
            }
            else
            {
                Singleton<EventRouter>.GetInstance().BroadCastEvent<CSPkg>(EventID.PlayerInfoSystem_Info_Received, msg);
            }
        }

        private void SetAllFriendBtnActive(GameObject root, bool isActive)
        {
            GameObject obj2 = Utility.FindChild(root, "pnlContainer/BtnGroup/btnSettings");
            GameObject obj3 = Utility.FindChild(root, "pnlContainer/BtnGroup/btnQuit");
            obj2.CustomSetActive(isActive);
            obj3.CustomSetActive(isActive);
        }

        private void SetAllGuildBtnActive(GameObject root, bool isActive)
        {
            GameObject obj2 = Utility.FindChild(root, "pnlContainer/BtnGroup/btnAddFriend");
            GameObject obj3 = Utility.FindChild(root, "pnlContainer/BtnGroup/btnAppointViceChairman");
            GameObject obj4 = Utility.FindChild(root, "pnlContainer/BtnGroup/btnTransferPosition");
            GameObject obj5 = Utility.FindChild(root, "pnlContainer/BtnGroup/btnFireMember");
            obj2.CustomSetActive(isActive);
            obj3.CustomSetActive(isActive);
            obj4.CustomSetActive(isActive);
            obj5.CustomSetActive(isActive);
        }

        private void SetBaseInfoScrollable(bool scrollable = false)
        {
            if (this.m_IsFormOpen && (this.m_Form != null))
            {
                GameObject widget = this.m_Form.GetWidget(1);
                if ((widget != null) && widget.activeSelf)
                {
                    GameObject obj3 = Utility.FindChild(widget, "pnlContainer/pnlInfo/scrollRect");
                    if (obj3 != null)
                    {
                        RectTransform component = obj3.GetComponent<RectTransform>();
                        ScrollRect rect = obj3.GetComponent<ScrollRect>();
                        if (component != null)
                        {
                            if (scrollable)
                            {
                                component.offsetMin = new Vector2(component.offsetMin.x, 90f);
                            }
                            else
                            {
                                component.offsetMin = new Vector2(component.offsetMin.x, 0f);
                            }
                        }
                        if (rect != null)
                        {
                            rect.verticalNormalizedPosition = 1f;
                        }
                    }
                }
            }
        }

        public void SetHeroItemData(CUIFormScript formScript, GameObject listItem, COMDT_MOST_USED_HERO_INFO heroInfo)
        {
            if ((listItem != null) && (heroInfo != null))
            {
                IHeroData data = CHeroDataFactory.CreateHeroData(heroInfo.dwHeroID);
                Transform transform = listItem.transform;
                ResHeroProficiency heroProficiency = CHeroInfo.GetHeroProficiency(data.heroType, (int) heroInfo.dwProficiencyLv);
                if (heroProficiency != null)
                {
                    listItem.GetComponent<Image>().SetSprite(string.Format("{0}{1}", "UGUI/Sprite/Dynamic/Quality/", StringHelper.UTF8BytesToString(ref heroProficiency.szImagePath)), formScript, true, false, false);
                }
                string heroSkinPic = CSkinInfo.GetHeroSkinPic(heroInfo.dwHeroID, 0);
                CUICommonSystem.SetHeroItemImage(formScript, listItem, heroSkinPic, enHeroHeadType.enIcon, false);
                GameObject gameObject = transform.Find("profession").gameObject;
                CUICommonSystem.SetHeroJob(formScript, gameObject, (enHeroJobType) data.heroType);
                transform.Find("heroNameText").GetComponent<Text>().text = data.heroName;
                CUIEventScript component = listItem.GetComponent<CUIEventScript>();
                stUIEventParams eventParams = new stUIEventParams();
                eventParams.openHeroFormPar.heroId = data.cfgID;
                eventParams.openHeroFormPar.openSrc = enHeroFormOpenSrc.HeroListClick;
                component.SetUIEvent(enUIEventType.Click, enUIEventID.Player_Info_Most_Used_Hero_Item_Click, eventParams);
            }
        }

        private void SetSingleGuildBtnActive(GameObject root)
        {
            GameObject obj2 = Utility.FindChild(root, "pnlContainer/BtnGroup/btnAddFriend");
            GameObject obj3 = Utility.FindChild(root, "pnlContainer/BtnGroup/btnAppointViceChairman");
            GameObject obj4 = Utility.FindChild(root, "pnlContainer/BtnGroup/btnTransferPosition");
            GameObject obj5 = Utility.FindChild(root, "pnlContainer/BtnGroup/btnFireMember");
            obj2.CustomSetActive(true);
            obj3.CustomSetActive(this._isShowGuildAppointViceChairmanBtn);
            obj4.CustomSetActive(this._isShowGuildTransferPositionBtn);
            obj5.CustomSetActive(this._isShowGuildFireMemberBtn);
        }

        public void ShowPlayerDetailInfo(ulong ullUid, int iLogicWorldId, DetailPlayerInfoSource sourceType = 0)
        {
            this._lastDetailSource = sourceType;
            if (this._lastDetailSource == DetailPlayerInfoSource.Self)
            {
                this.m_PlayerProfile.ConvertRoleInfoData(Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo());
                this.OpenForm();
            }
            else if (ullUid > 0L)
            {
                this.ReqOtherPlayerDetailInfo(ullUid, iLogicWorldId, true, true);
            }
        }

        public void ShowPlayerDetailInfo(ulong ullUid, int iLogicWorldId, bool isShowGuildAppointViceChairmanBtn, bool isShowGuildTransferPositionBtn, bool isShowGuildFireMemberBtn)
        {
            this._isShowGuildAppointViceChairmanBtn = isShowGuildAppointViceChairmanBtn;
            this._isShowGuildTransferPositionBtn = isShowGuildTransferPositionBtn;
            this._isShowGuildFireMemberBtn = isShowGuildFireMemberBtn;
            this.ShowPlayerDetailInfo(ullUid, iLogicWorldId, DetailPlayerInfoSource.Guild);
        }

        public override void UnInit()
        {
            base.UnInit();
            CUIEventManager instance = Singleton<CUIEventManager>.GetInstance();
            instance.RemoveUIEventListener(enUIEventID.Player_Info_OpenForm, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfo_OpenForm));
            instance.RemoveUIEventListener(enUIEventID.Player_Info_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfo_CloseForm));
            instance.RemoveUIEventListener(enUIEventID.Player_Info_Open_Pvp_Info, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoOpenPvpInfo));
            instance.RemoveUIEventListener(enUIEventID.Player_Info_Open_Base_Info, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoOpenBaseInfo));
            instance.RemoveUIEventListener(enUIEventID.BuyPick_QQ_VIP, new CUIEventManager.OnUIEventHandler(this.OpenByQQVIP));
            instance.RemoveUIEventListener(enUIEventID.Player_Info_Quit_Game, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoQuitGame));
            instance.RemoveUIEventListener(enUIEventID.Player_Info_Quit_Game_Confirm, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoQuitGameConfirm));
            instance.RemoveUIEventListener(enUIEventID.Player_Info_Most_Used_Hero_Item_Enable, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoMostUsedHeroItemEnable));
            instance.RemoveUIEventListener(enUIEventID.Player_Info_Most_Used_Hero_Item_Click, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoMostUsedHeroItemClick));
            instance.RemoveUIEventListener(enUIEventID.Player_Info_Show_Rule, new CUIEventManager.OnUIEventHandler(this.OnPlayerInfoShowRule));
            instance.RemoveUIEventListener(enUIEventID.Player_Info_License_ListItem_Enable, new CUIEventManager.OnUIEventHandler(this.OnLicenseListItemEnable));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler(EventID.NOBE_STATE_CHANGE, new Action(this, (IntPtr) this.UpdateNobeHeadIdx));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler(EventID.HEAD_IMAGE_FLAG_CHANGE, new Action(this, (IntPtr) this.UpdateHeadFlag));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.DeepLink_OnClick, new CUIEventManager.OnUIEventHandler(this.DeepLinkClick));
        }

        private void UpdateBaseInfo()
        {
            if (this.m_IsFormOpen && (this.m_Form != null))
            {
                GameObject widget = this.m_Form.GetWidget(1);
                if (widget != null)
                {
                    CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                    this.DisplayCustomButton();
                    if (!CSysDynamicBlock.bSocialBlocked)
                    {
                        GameObject obj3 = Utility.FindChild(widget, "pnlContainer/pnlHead/pnlImg/HttpImage");
                        if ((obj3 != null) && !string.IsNullOrEmpty(this.m_PlayerProfile.HeadUrl()))
                        {
                            CUIHttpImageScript script = obj3.GetComponent<CUIHttpImageScript>();
                            script.SetImageUrl(this.m_PlayerProfile.HeadUrl());
                            MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(script.GetComponent<Image>(), (int) this.m_PlayerProfile.m_vipInfo.stGameVipClient.dwCurLevel, false);
                            GameObject obj4 = Utility.FindChild(widget, "pnlContainer/pnlHead/pnlImg/HttpImage/NobeImag");
                            if (obj4 != null)
                            {
                                MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(obj4.GetComponent<Image>(), (int) this.m_PlayerProfile.m_vipInfo.stGameVipClient.dwHeadIconId);
                            }
                        }
                    }
                    this.UpdateHeadFlag();
                    MonoSingleton<NobeSys>.GetInstance().SetGameCenterVisible(Utility.FindChild(widget, "pnlContainer/pnlHead/NameGroup/WXIcon"), this.m_PlayerProfile.PrivilegeType(), ApolloPlatform.Wechat, true, false);
                    COM_PRIVILEGE_TYPE privilegeType = this.m_PlayerProfile.PrivilegeType();
                    MonoSingleton<NobeSys>.GetInstance().SetGameCenterVisible(Utility.FindChild(widget, "pnlContainer/BtnGroup/WXGameCenter/WXGameCenterBtn"), privilegeType, ApolloPlatform.Wechat, true, false);
                    COM_PRIVILEGE_TYPE com_privilege_type2 = (privilegeType != COM_PRIVILEGE_TYPE.COM_PRIVILEGE_TYPE_WXGAMECENTER_LOGIN) ? COM_PRIVILEGE_TYPE.COM_PRIVILEGE_TYPE_WXGAMECENTER_LOGIN : COM_PRIVILEGE_TYPE.COM_PRIVILEGE_TYPE_NONE;
                    MonoSingleton<NobeSys>.GetInstance().SetGameCenterVisible(Utility.FindChild(widget, "pnlContainer/BtnGroup/WXGameCenter/WXGameCenterGrey"), com_privilege_type2, ApolloPlatform.Wechat, true, false);
                    Text componetInChild = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlHead/Level");
                    if (componetInChild != null)
                    {
                        componetInChild.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("ranking_PlayerLevel"), this.m_PlayerProfile.PvpLevel());
                    }
                    Image component = Utility.FindChild(widget, "pnlContainer/pnlHead/NameGroup/Gender").GetComponent<Image>();
                    component.gameObject.CustomSetActive(this.m_PlayerProfile.Gender() != COM_SNSGENDER.COM_SNSGENDER_NONE);
                    if (this.m_PlayerProfile.Gender() == COM_SNSGENDER.COM_SNSGENDER_MALE)
                    {
                        CUIUtility.SetImageSprite(component, string.Format("{0}icon/Ico_boy.prefab", "UGUI/Sprite/Dynamic/"), null, true, false, false);
                    }
                    else if (this.m_PlayerProfile.Gender() == COM_SNSGENDER.COM_SNSGENDER_FEMALE)
                    {
                        CUIUtility.SetImageSprite(component, string.Format("{0}icon/Ico_girl.prefab", "UGUI/Sprite/Dynamic/"), null, true, false, false);
                    }
                    Text text2 = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlHead/NameGroup/txtName");
                    if (text2 != null)
                    {
                        text2.text = this.m_PlayerProfile.Name();
                    }
                    GameObject obj5 = Utility.FindChild(widget, "pnlContainer/pnlHead/Status/Rank");
                    if (this.m_PlayerProfile.GradeOfRank() == 0)
                    {
                        if (obj5 != null)
                        {
                            obj5.CustomSetActive(false);
                        }
                    }
                    else
                    {
                        obj5.CustomSetActive(true);
                        Image image2 = null;
                        Image image3 = null;
                        if (obj5 != null)
                        {
                            image2 = Utility.GetComponetInChild<Image>(obj5, "ImgRank");
                            image3 = Utility.GetComponetInChild<Image>(obj5, "ImgRank/ImgSubRank");
                        }
                        if (image2 != null)
                        {
                            string rankSmallIconPath = CLadderView.GetRankSmallIconPath(this.m_PlayerProfile.GradeOfRank());
                            image2.SetSprite(rankSmallIconPath, this.m_Form, true, false, false);
                        }
                        if (image3 != null)
                        {
                            string subRankSmallIconPath = CLadderView.GetSubRankSmallIconPath(this.m_PlayerProfile.GradeOfRank());
                            image3.SetSprite(subRankSmallIconPath, this.m_Form, true, false, false);
                        }
                    }
                    GameObject obj6 = Utility.FindChild(widget, "pnlContainer/pnlHead/Status/HisRank");
                    if (this.m_PlayerProfile.HighestGradeOfRank() == 0)
                    {
                        if (obj6 != null)
                        {
                            obj6.CustomSetActive(false);
                        }
                    }
                    else
                    {
                        obj6.CustomSetActive(true);
                        Image image4 = null;
                        Image image5 = null;
                        if (obj6 != null)
                        {
                            image4 = Utility.GetComponetInChild<Image>(obj6, "ImgRank");
                            image5 = Utility.GetComponetInChild<Image>(obj6, "ImgRank/ImgSubRank");
                        }
                        if (image4 != null)
                        {
                            string prefabPath = CLadderView.GetRankSmallIconPath(this.m_PlayerProfile.HighestGradeOfRank());
                            image4.SetSprite(prefabPath, this.m_Form, true, false, false);
                        }
                        if (image5 != null)
                        {
                            string str4 = CLadderView.GetSubRankSmallIconPath(this.m_PlayerProfile.HighestGradeOfRank());
                            image5.SetSprite(str4, this.m_Form, true, false, false);
                        }
                    }
                    string str5 = string.Empty;
                    string str6 = string.Empty;
                    if (Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().HaveExtraCoin())
                    {
                        GameObject obj7 = Utility.FindChild(widget, "pnlContainer/pnlHead/Status/ExtraCoin");
                        obj7.CustomSetActive(true);
                        if (masterRoleInfo.GetCoinExpireHours() > 0)
                        {
                            str5 = string.Format(Singleton<CTextManager>.GetInstance().GetText("DoubleCoinExpireTimeTips"), masterRoleInfo.GetCoinExpireHours() / 0x18, masterRoleInfo.GetCoinExpireHours() % 0x18);
                        }
                        if (masterRoleInfo.GetCoinWinCount() > 0)
                        {
                            str6 = string.Format(Singleton<CTextManager>.GetInstance().GetText("DoubleCoinCountWinTips"), masterRoleInfo.GetCoinWinCount());
                        }
                        if (string.IsNullOrEmpty(str5))
                        {
                            CUICommonSystem.SetCommonTipsEvent(this.m_Form, obj7, string.Format("{0}", str6), enUseableTipsPos.enBottom);
                        }
                        else if (string.IsNullOrEmpty(str6))
                        {
                            CUICommonSystem.SetCommonTipsEvent(this.m_Form, obj7, string.Format("{0}", str5), enUseableTipsPos.enBottom);
                        }
                        else
                        {
                            CUICommonSystem.SetCommonTipsEvent(this.m_Form, obj7, string.Format("{0}\n{1}", str5, str6), enUseableTipsPos.enBottom);
                        }
                    }
                    else
                    {
                        Utility.FindChild(widget, "pnlContainer/pnlHead/Status/ExtraCoin").CustomSetActive(false);
                    }
                    if (Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().HaveExtraExp())
                    {
                        GameObject obj8 = Utility.FindChild(widget, "pnlContainer/pnlHead/Status/ExtraExp");
                        obj8.CustomSetActive(true);
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
                            CUICommonSystem.SetCommonTipsEvent(this.m_Form, obj8, string.Format("{0}", str6), enUseableTipsPos.enBottom);
                        }
                        else if (string.IsNullOrEmpty(str6))
                        {
                            CUICommonSystem.SetCommonTipsEvent(this.m_Form, obj8, string.Format("{0}", str5), enUseableTipsPos.enBottom);
                        }
                        else
                        {
                            CUICommonSystem.SetCommonTipsEvent(this.m_Form, obj8, string.Format("{0}\n{1}", str5, str6), enUseableTipsPos.enBottom);
                        }
                    }
                    else
                    {
                        Utility.FindChild(widget, "pnlContainer/pnlHead/Status/ExtraExp").CustomSetActive(false);
                    }
                    uint num = (uint) (((((this.m_PlayerProfile.Pvp5V5TotalGameCnt() + this.m_PlayerProfile.Pvp3V3TotalGameCnt()) + this.m_PlayerProfile.Pvp2V2TotalGameCnt()) + this.m_PlayerProfile.Pvp1V1TotalGameCnt()) + this.m_PlayerProfile.RankTotalGameCnt()) + this.m_PlayerProfile.EntertainmentTotalGameCnt());
                    uint a = (uint) (((((this.m_PlayerProfile.Pvp5V5WinGameCnt() + this.m_PlayerProfile.Pvp3V3WinGameCnt()) + this.m_PlayerProfile.Pvp2V2WinGameCnt()) + this.m_PlayerProfile.Pvp1V1WinGameCnt()) + this.m_PlayerProfile.RankWinGameCnt()) + this.m_PlayerProfile.EntertainmentWinGameCnt());
                    Text text3 = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlInfo/scrollRect/content/Left/PvpInfo/Cnt");
                    if (text3 != null)
                    {
                        text3.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Total_Fighting_Cnt"), num);
                    }
                    Text text4 = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlInfo/scrollRect/content/Left/PvpInfo/WinsBg/imgWins/txtWins");
                    if (text4 != null)
                    {
                        text4.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("ranking_WinRate"), CPlayerProfile.Round(CPlayerProfile.Divide(a, num) * 100f));
                    }
                    Image image = Utility.GetComponetInChild<Image>(widget, "pnlContainer/pnlInfo/scrollRect/content/Left/PvpInfo/WinsBg/imgWins");
                    if (image != null)
                    {
                        image.CustomFillAmount(CPlayerProfile.Divide(a, num));
                    }
                    Text text5 = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlInfo/scrollRect/content/Left/OwnInfo/HeroCnt");
                    if (text5 != null)
                    {
                        text5.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Total_Hero_Cnt"), this.m_PlayerProfile.HeroCnt());
                    }
                    Text text6 = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlInfo/scrollRect/content/Left/OwnInfo/SkinCnt");
                    if (text6 != null)
                    {
                        text6.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Total_Skin_Cnt"), this.m_PlayerProfile.SkinCnt());
                    }
                    Text text7 = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlInfo/scrollRect/content/Left/GuildInfo/Name");
                    Text text8 = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlInfo/scrollRect/content/Left/GuildInfo/Position");
                    Text text9 = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlInfo/scrollRect/content/Left/GuildInfo/None");
                    if (!CGuildSystem.IsInNormalGuild(this.m_PlayerProfile.GuildState) || string.IsNullOrEmpty(this.m_PlayerProfile.GuildName))
                    {
                        if (text7 != null)
                        {
                            text7.text = string.Empty;
                        }
                        if (text8 != null)
                        {
                            text8.text = string.Empty;
                        }
                        if (text9 != null)
                        {
                            text9.text = Singleton<CTextManager>.GetInstance().GetText("PlayerInfo_Guild");
                        }
                    }
                    else
                    {
                        if (text7 != null)
                        {
                            text7.text = this.m_PlayerProfile.GuildName;
                        }
                        if (text8 != null)
                        {
                            text8.text = Singleton<CTextManager>.GetInstance().GetText("Player_Info_Guild_Position") + CGuildHelper.GetPositionName(this.m_PlayerProfile.GuildState);
                        }
                        if (text9 != null)
                        {
                            text9.text = string.Empty;
                        }
                    }
                    bool bActive = this.isSelf(this.m_PlayerProfile.m_uuid);
                    this.m_Form.GetWidget(3).CustomSetActive(bActive);
                    GameObject obj10 = Utility.FindChild(widget, "pnlContainer/BtnGroup/JFQBtn");
                    if ((ApolloConfig.platform == ApolloPlatform.QQ) || (ApolloConfig.platform == ApolloPlatform.WTLogin))
                    {
                        if (!CSysDynamicBlock.bJifenHallBlock)
                        {
                            obj10.CustomSetActive(bActive);
                        }
                        else
                        {
                            obj10.CustomSetActive(false);
                        }
                    }
                    else
                    {
                        obj10.CustomSetActive(false);
                    }
                    GameObject obj11 = Utility.FindChild(widget, "pnlContainer/BtnGroup/BuLuoBtn");
                    if ((ApolloConfig.platform == ApolloPlatform.QQ) || (ApolloConfig.platform == ApolloPlatform.WTLogin))
                    {
                        obj11.CustomSetActive(bActive);
                    }
                    else
                    {
                        obj11.CustomSetActive(false);
                    }
                    GameObject obj12 = Utility.FindChild(widget, "pnlContainer/BtnGroup/DeepLinkBtn");
                    if ((ApolloConfig.platform == ApolloPlatform.QQ) || (ApolloConfig.platform == ApolloPlatform.WTLogin))
                    {
                        obj12.CustomSetActive(false);
                    }
                    else
                    {
                        long curTime = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().getCurrentTimeSinceLogin();
                        if (MonoSingleton<BannerImageSys>.GetInstance().DeepLinkInfo.isTimeValid(curTime))
                        {
                            obj12.CustomSetActive(bActive);
                        }
                        else
                        {
                            obj12.CustomSetActive(false);
                        }
                    }
                    if (CSysDynamicBlock.bLobbyEntryBlocked)
                    {
                        Transform transform = widget.transform.Find("pnlContainer/pnlHead/changeNobeheadicon");
                        if (transform != null)
                        {
                            transform.gameObject.CustomSetActive(false);
                        }
                        Transform transform2 = widget.transform.Find("pnlContainer/BtnGroup/BuLuoBtn");
                        if (transform2 != null)
                        {
                            transform2.gameObject.CustomSetActive(false);
                        }
                        Transform transform3 = widget.transform.Find("pnlContainer/BtnGroup/QQVIPBtn");
                        if (transform3 != null)
                        {
                            transform3.gameObject.CustomSetActive(false);
                        }
                        Transform transform4 = widget.transform.Find("pnlContainer/BtnGroup/JFQBtn");
                        if (transform4 != null)
                        {
                            transform4.gameObject.CustomSetActive(false);
                        }
                        Transform transform5 = widget.transform.Find("pnlContainer/BtnGroup/XYJLBBtn");
                        if (transform5 != null)
                        {
                            transform5.gameObject.CustomSetActive(false);
                        }
                    }
                }
            }
        }

        private void UpdateCommonHeroList()
        {
            GameObject widget = this.m_Form.GetWidget(7);
            if (widget != null)
            {
                CUIListScript componetInChild = Utility.GetComponetInChild<CUIListScript>(widget, "List");
                if (componetInChild != null)
                {
                    int count = this.m_PlayerProfile.MostUsedHeroList().Count;
                    uint dwConfValue = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0xb2).dwConfValue;
                    if (count > dwConfValue)
                    {
                        count = (int) dwConfValue;
                    }
                    componetInChild.SetElementAmount(count);
                }
            }
        }

        private void UpdateCreditScore(uint score)
        {
            if (this.m_IsFormOpen && (this.m_Form != null))
            {
                GameObject widget = this.m_Form.GetWidget(4);
                if (widget != null)
                {
                    Text componetInChild = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlInfo/curCreditScore/score");
                    if (componetInChild != null)
                    {
                        componetInChild.text = string.Format("{0}", score);
                    }
                    Text text2 = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlInfo/curCreditLevel/level");
                    if (text2 != null)
                    {
                        int key = 0;
                        for (ResCreditLevelInfo info = GameDataMgr.creditLevelDatabin.GetDataByKey(key); (info != null) && (info.dwCreditThreshold >= 0); info = GameDataMgr.creditLevelDatabin.GetDataByKey(++key))
                        {
                            if ((info.dwCreditThreshold == 0) || (score >= info.dwCreditThreshold))
                            {
                                text2.text = string.Format("{0}", info.szCreditLevel);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateHeadFlag()
        {
            if ((this.m_IsFormOpen && (this.m_Form != null)) && (this.m_Form.GetWidget(1) != null))
            {
                GameObject target = Utility.FindChild(this.m_Form.gameObject, "pnlBg/pnlBody/pnlBaseInfo/pnlContainer/pnlHead/changeNobeheadicon");
                if (target != null)
                {
                    if (Singleton<HeadIconSys>.instance.UnReadFlagNum > 0)
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

        private void UpdateNobeHeadIdx()
        {
            if (this.m_IsFormOpen && (this.m_Form != null))
            {
                GameObject widget = this.m_Form.GetWidget(1);
                if (widget != null)
                {
                    GameObject obj3 = Utility.FindChild(widget, "pnlContainer/pnlHead/pnlImg/HttpImage/NobeImag");
                    if (obj3 != null)
                    {
                        MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(obj3.GetComponent<Image>(), (int) MonoSingleton<NobeSys>.GetInstance().m_vipInfo.stGameVipClient.dwHeadIconId);
                    }
                }
            }
        }

        private void UpdatePlayerInfo()
        {
            if (this.m_IsFormOpen && (this.m_Form != null))
            {
                GameObject widget = this.m_Form.GetWidget(4);
                if (widget != null)
                {
                    CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                    this.DisplayCustomButton();
                    if (!CSysDynamicBlock.bSocialBlocked)
                    {
                        GameObject obj3 = Utility.FindChild(widget, "pnlContainer/pnlHead/pnlImg/HttpImage");
                        if ((obj3 != null) && !string.IsNullOrEmpty(this.m_PlayerProfile.HeadUrl()))
                        {
                            CUIHttpImageScript component = obj3.GetComponent<CUIHttpImageScript>();
                            component.SetImageUrl(this.m_PlayerProfile.HeadUrl());
                            MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(component.GetComponent<Image>(), (int) this.m_PlayerProfile.m_vipInfo.stGameVipClient.dwCurLevel, false);
                            GameObject obj4 = Utility.FindChild(widget, "pnlContainer/pnlHead/pnlImg/HttpImage/NobeImag");
                            if (obj4 != null)
                            {
                                MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(obj4.GetComponent<Image>(), (int) this.m_PlayerProfile.m_vipInfo.stGameVipClient.dwHeadIconId);
                            }
                        }
                    }
                    Text componetInChild = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlHead/Level");
                    if (componetInChild != null)
                    {
                        componetInChild.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("ranking_PlayerLevel"), this.m_PlayerProfile.PvpLevel());
                    }
                    Text text2 = Utility.GetComponetInChild<Text>(widget, "pnlContainer/pnlInfo/CreditRule/text");
                    if (text2 != null)
                    {
                        ResRuleText dataByKey = GameDataMgr.s_ruleTextDatabin.GetDataByKey(11);
                        if (dataByKey != null)
                        {
                            text2.text = string.Format("{0}", dataByKey.szContent);
                        }
                    }
                }
            }
        }

        private void UpdatePvpInfo()
        {
            if (this.m_IsFormOpen && (this.m_Form != null))
            {
                GameObject widget = this.m_Form.GetWidget(2);
                if (widget != null)
                {
                    if (!CSysDynamicBlock.bSocialBlocked)
                    {
                        GameObject obj3 = Utility.FindChild(widget, "pnlHead/mvp/HttpImage");
                        if ((obj3 != null) && !string.IsNullOrEmpty(this.m_PlayerProfile.HeadUrl()))
                        {
                            obj3.GetComponent<CUIHttpImageScript>().SetImageUrl(this.m_PlayerProfile.HeadUrl());
                        }
                        GameObject obj4 = Utility.FindChild(widget, "pnlHead/loseSoul/HttpImage");
                        if ((obj4 != null) && !string.IsNullOrEmpty(this.m_PlayerProfile.HeadUrl()))
                        {
                            obj4.GetComponent<CUIHttpImageScript>().SetImageUrl(this.m_PlayerProfile.HeadUrl());
                        }
                    }
                    Text componetInChild = Utility.GetComponetInChild<Text>(widget, "pnlLeft/pnlHead/mvp/cnt");
                    if (componetInChild != null)
                    {
                        componetInChild.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Cnt_Unit"), this.m_PlayerProfile.MVPCnt());
                    }
                    Text text2 = Utility.GetComponetInChild<Text>(widget, "pnlLeft/pnlHead/loseSoul/cnt");
                    if (text2 != null)
                    {
                        text2.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Cnt_Unit"), this.m_PlayerProfile.LoseSoulCnt());
                    }
                    Text text3 = Utility.GetComponetInChild<Text>(widget, "pnlLeft/pnlHead/godLike/cnt");
                    if (text3 != null)
                    {
                        text3.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Cnt_Unit"), this.m_PlayerProfile.HolyShit());
                    }
                    Text text4 = Utility.GetComponetInChild<Text>(widget, "pnlLeft/pnlHead/tripleKill/cnt");
                    if (text4 != null)
                    {
                        text4.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Cnt_Unit"), this.m_PlayerProfile.TripleKill());
                    }
                    Text text5 = Utility.GetComponetInChild<Text>(widget, "pnlLeft/pnlHead/QuataryKill/cnt");
                    if (text5 != null)
                    {
                        text5.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Cnt_Unit"), this.m_PlayerProfile.QuataryKill());
                    }
                    Text text6 = Utility.GetComponetInChild<Text>(widget, "pnlLeft/pnlHead/PentaKill/cnt");
                    if (text6 != null)
                    {
                        text6.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Cnt_Unit"), this.m_PlayerProfile.PentaKill());
                    }
                    GameObject obj5 = Utility.FindChild(widget, "pnlInfo/Left/qualifyInfo");
                    if (this.m_PlayerProfile.GradeOfRank() != 0)
                    {
                        obj5.CustomSetActive(true);
                        Text text7 = Utility.GetComponetInChild<Text>(widget, "pnlInfo/Left/qualifyInfo/cnt");
                        if (text7 != null)
                        {
                            text7.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Fighting_Cnt"), this.m_PlayerProfile.RankTotalGameCnt());
                        }
                        Image image = Utility.GetComponetInChild<Image>(widget, "pnlInfo/Left/qualifyInfo/WinsBg/imgWins");
                        if (image != null)
                        {
                            image.CustomFillAmount(this.m_PlayerProfile.RankWins());
                        }
                        Text text8 = Utility.GetComponetInChild<Text>(widget, "pnlInfo/Left/qualifyInfo/WinsBg/imgWins/txtWins");
                        if (text8 != null)
                        {
                            text8.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("ranking_WinRate"), CPlayerProfile.Round(this.m_PlayerProfile.RankWins() * 100f));
                        }
                    }
                    else
                    {
                        obj5.CustomSetActive(false);
                    }
                    Text text9 = Utility.GetComponetInChild<Text>(widget, "pnlInfo/Left/5V5Info/cnt");
                    if (text9 != null)
                    {
                        text9.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Fighting_Cnt"), this.m_PlayerProfile.Pvp5V5TotalGameCnt());
                    }
                    Image image2 = Utility.GetComponetInChild<Image>(widget, "pnlInfo/Left/5V5Info/WinsBg/imgWins");
                    if (image2 != null)
                    {
                        image2.CustomFillAmount(this.m_PlayerProfile.Pvp5V5Wins());
                    }
                    Text text10 = Utility.GetComponetInChild<Text>(widget, "pnlInfo/Left/5V5Info/WinsBg/imgWins/txtWins");
                    if (text10 != null)
                    {
                        text10.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("ranking_WinRate"), CPlayerProfile.Round(this.m_PlayerProfile.Pvp5V5Wins() * 100f));
                    }
                    Text text11 = Utility.GetComponetInChild<Text>(widget, "pnlInfo/Left/3V3Info/cnt");
                    if (text11 != null)
                    {
                        text11.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Fighting_Cnt"), this.m_PlayerProfile.Pvp3V3TotalGameCnt());
                    }
                    Image image3 = Utility.GetComponetInChild<Image>(widget, "pnlInfo/Left/3V3Info/WinsBg/imgWins");
                    if (image3 != null)
                    {
                        image3.CustomFillAmount(this.m_PlayerProfile.Pvp3V3Wins());
                    }
                    Text text12 = Utility.GetComponetInChild<Text>(widget, "pnlInfo/Left/3V3Info/WinsBg/imgWins/txtWins");
                    if (text12 != null)
                    {
                        text12.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("ranking_WinRate"), CPlayerProfile.Round(this.m_PlayerProfile.Pvp3V3Wins() * 100f));
                    }
                    Text text13 = Utility.GetComponetInChild<Text>(widget, "pnlInfo/Left/1V1Info/cnt");
                    if (text13 != null)
                    {
                        text13.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Fighting_Cnt"), this.m_PlayerProfile.Pvp1V1TotalGameCnt());
                    }
                    Image image4 = Utility.GetComponetInChild<Image>(widget, "pnlInfo/Left/1V1Info/WinsBg/imgWins");
                    if (image4 != null)
                    {
                        image4.CustomFillAmount(this.m_PlayerProfile.Pvp1V1Wins());
                    }
                    Text text14 = Utility.GetComponetInChild<Text>(widget, "pnlInfo/Left/1V1Info/WinsBg/imgWins/txtWins");
                    if (text14 != null)
                    {
                        text14.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("ranking_WinRate"), CPlayerProfile.Round(this.m_PlayerProfile.Pvp1V1Wins() * 100f));
                    }
                    Text text15 = Utility.GetComponetInChild<Text>(widget, "pnlInfo/Left/entertainmentInfo/cnt");
                    if (text15 != null)
                    {
                        text15.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Player_Info_Fighting_Cnt"), this.m_PlayerProfile.EntertainmentTotalGameCnt());
                    }
                    Image image5 = Utility.GetComponetInChild<Image>(widget, "pnlInfo/Left/entertainmentInfo/WinsBg/imgWins");
                    if (image5 != null)
                    {
                        image5.CustomFillAmount(this.m_PlayerProfile.EntertainmentWins());
                    }
                    Text text16 = Utility.GetComponetInChild<Text>(widget, "pnlInfo/Left/entertainmentInfo/WinsBg/imgWins/txtWins");
                    if (text16 != null)
                    {
                        text16.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("ranking_WinRate"), CPlayerProfile.Round(this.m_PlayerProfile.EntertainmentWins() * 100f));
                    }
                }
            }
        }

        public Tab CurTab
        {
            get
            {
                return this.m_CurTab;
            }
            set
            {
                this.m_CurTab = value;
            }
        }

        public enum DetailPlayerInfoSource
        {
            DefaultOthers,
            Self,
            Guild,
            Friend
        }

        public enum enPlayerFormWidget
        {
            Tab,
            Base_Info_Tab,
            Pvp_Info_Tab,
            Change_Name_Button,
            CreditScore_Tab,
            License_Info_Tab,
            License_List,
            Common_Hero_info,
            Rule_Btn
        }

        public enum Tab
        {
            Base_Info,
            Pvp_Info,
            Common_Hero,
            PvpCreditScore_Info
        }
    }
}

