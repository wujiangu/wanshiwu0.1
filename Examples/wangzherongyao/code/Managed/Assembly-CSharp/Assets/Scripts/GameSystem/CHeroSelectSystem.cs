namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    [MessageHandlerClass]
    public class CHeroSelectSystem : Singleton<CHeroSelectSystem>
    {
        private const int c_maxSelectedHeroIDsBeforeGC = 3;
        public uint m_battleListID;
        public COMDT_BATTLELIST_LIST m_defaultBattleListInfo;
        public enSelectHeroType m_gameType = enSelectHeroType.enNull;
        public string m_heroGameObjName = string.Empty;
        public enHeroInfoShowType m_heroInfoShowType;
        private ListView<IHeroData> m_hostHeroList;
        public bool m_isSelectConfirm;
        public ResDT_LevelCommonInfo m_mapData;
        public int m_mapSubType = 1;
        public uint m_nowShowHeroID;
        public string m_pveLevelName = string.Empty;
        public int m_roomType;
        private List<uint> m_selectedHeroModelIDs = new List<uint>();
        public byte m_selectHeroCount;
        public List<uint> m_selectHeroIDList = new List<uint>();
        public uint m_showHeroID;
        public CSDT_SINGLE_GAME_OF_ACTIVITY m_stGameOfActivity;
        public CSDT_SINGLE_GAME_OF_ADVENTURE m_stGameOfAdventure;
        public CSDT_SINGLE_GAME_OF_ARENA m_stGameOfArena;
        public CSDT_SINGLE_GAME_OF_BURNING m_stGameOfBurnning;
        public CSDT_SINGLE_GAME_OF_COMBAT m_stGameOfCombat;
        private int m_UseRandSelCount;
        public static string s_heroSelectFormPath = "UGUI/Form/System/HeroSelect/Form_HeroSelect.prefab";
        public static int[] s_propArr = new int[0x24];
        public static int[] s_propPctArr = new int[0x24];
        public static string s_symbolPropPanelPath = "Other/Panel_SymbolProp";

        private void BuyHeroCount(CUIEvent uiEvent)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x459);
            msg.stPkgData.stShopBuyReq.iBuyType = 13;
            msg.stPkgData.stShopBuyReq.iBuySubType = this.m_UseRandSelCount + 1;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public int GetHeroIndex(uint heroID)
        {
            if (this.m_hostHeroList != null)
            {
                for (int i = 0; i < this.m_hostHeroList.Count; i++)
                {
                    if (this.m_hostHeroList[i].cfgID == heroID)
                    {
                        return i;
                    }
                }
            }
            return 0;
        }

        public List<uint> GetHeroListForBattleListID(uint battleListID)
        {
            List<uint> list = new List<uint>();
            if (this.m_defaultBattleListInfo != null)
            {
                for (int i = 0; i < this.m_defaultBattleListInfo.dwListNum; i++)
                {
                    if (this.m_defaultBattleListInfo.astBattleList[i].dwBattleListID == battleListID)
                    {
                        COMDT_BATTLEHERO stBattleList = this.m_defaultBattleListInfo.astBattleList[i].stBattleList;
                        for (int j = 0; j < stBattleList.BattleHeroList.Length; j++)
                        {
                            uint heroID = stBattleList.BattleHeroList[j];
                            if (!this.IsHeroCanUse(heroID))
                            {
                                heroID = 0;
                            }
                            if (heroID != 0)
                            {
                                list.Add(heroID);
                            }
                        }
                    }
                }
            }
            return list;
        }

        public List<uint> GetPveTeamHeroIDList()
        {
            List<uint> selectHeroIDList = this.m_selectHeroIDList;
            if (this.m_gameType == enSelectHeroType.enPVE_Computer)
            {
                selectHeroIDList = new List<uint>();
                MemberInfo masterMemberInfo = this.m_roomInfo.GetMasterMemberInfo();
                selectHeroIDList.Add(masterMemberInfo.ChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID);
            }
            return selectHeroIDList;
        }

        private ListView<ResSkillUnlock> GetSelSkillAvailable()
        {
            ListView<ResSkillUnlock> view = new ListView<ResSkillUnlock>();
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            ResSkillUnlock item = null;
            for (int i = 0; i < GameDataMgr.addedSkiilDatabin.count; i++)
            {
                item = GameDataMgr.addedSkiilDatabin.GetDataByIndex(i);
                if (((masterRoleInfo != null) && (masterRoleInfo.PvpLevel >= item.wAcntLevel)) && (!this.isLuanDouRule() || (item.bEntertainmentValid != 0)))
                {
                    view.Add(item);
                }
            }
            return view;
        }

        private List<uint> GetTeamHeroList()
        {
            List<uint> list = new List<uint>();
            if (this.m_heroInfoShowType == enHeroInfoShowType.enPVP)
            {
                if (this.m_roomInfo != null)
                {
                    MemberInfo masterMemberInfo = this.m_roomInfo.GetMasterMemberInfo();
                    if (masterMemberInfo == null)
                    {
                        return list;
                    }
                    ListView<MemberInfo> campMemberList = this.m_roomInfo.GetCampMemberList(masterMemberInfo.camp);
                    for (int i = 0; i < campMemberList.Count; i++)
                    {
                        list.Add(campMemberList[i].ChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID);
                    }
                }
                return list;
            }
            return this.m_selectHeroIDList;
        }

        public Transform GetTeamPlayerElement(ulong playerUID)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form == null)
            {
                return null;
            }
            Transform transform2 = form.transform.Find("PanelRight/ListTeamHeroInfo");
            if (transform2 == null)
            {
                return null;
            }
            CUIListScript component = transform2.gameObject.GetComponent<CUIListScript>();
            if (component == null)
            {
                return null;
            }
            return component.GetElemenet(this.GetTeamPlayerIndex(playerUID)).transform;
        }

        public int GetTeamPlayerIndex(ulong playerUID)
        {
            int num = 0;
            if ((this.m_heroInfoShowType == enHeroInfoShowType.enPVP) && (this.m_roomInfo != null))
            {
                MemberInfo masterMemberInfo = this.m_roomInfo.GetMasterMemberInfo();
                if (masterMemberInfo == null)
                {
                    return num;
                }
                ListView<MemberInfo> campMemberList = this.m_roomInfo.GetCampMemberList(masterMemberInfo.camp);
                for (int i = 0; i < campMemberList.Count; i++)
                {
                    if (campMemberList[i].ullUid == playerUID)
                    {
                        return i;
                    }
                }
            }
            return num;
        }

        private List<int> HasPositionHero(ref Calc9SlotHeroData[] heroes, RES_HERO_RECOMMEND_POSITION pos)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                if (heroes[i].RecommendPos == pos)
                {
                    list.Add(i);
                }
            }
            return list;
        }

        private void HeroSelect_CloseForm(CUIEvent uiEvent)
        {
            if (this.m_gameType == enSelectHeroType.enPVE_Computer)
            {
                CUIEvent event2 = new CUIEvent {
                    m_eventID = enUIEventID.Matching_OpenEntry
                };
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(event2);
            }
            Singleton<CUIManager>.GetInstance().CloseForm(s_heroSelectFormPath);
        }

        private void HeroSelect_CloseFullHeroList(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                Transform transform = form.gameObject.transform.Find("PanelLeft/ListHostHeroInfo");
                Transform transform2 = form.gameObject.transform.Find("PanelLeft/ListHostHeroInfoFull");
                if ((transform != null) && (transform2 != null))
                {
                    transform.gameObject.CustomSetActive(true);
                    transform2.gameObject.CustomSetActive(false);
                }
                Singleton<CChatController>.instance.Set_Show_Bottom(true);
            }
        }

        private void HeroSelect_CloseFullSkinList(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                Transform transform = form.gameObject.transform.Find("PanelLeftSkin/ListHostSkinInfo");
                Transform transform2 = form.gameObject.transform.Find("PanelLeftSkin/ListHostSkinInfoFull");
                if ((transform != null) && (transform2 != null))
                {
                    transform.gameObject.CustomSetActive(true);
                    transform2.gameObject.CustomSetActive(false);
                }
            }
        }

        private void HeroSelect_ConfirmHeroSelect(CUIEvent uiEvent)
        {
            if (((this.m_gameType == enSelectHeroType.enPVP) || (this.m_gameType == enSelectHeroType.enLadder)) || ((this.m_gameType == enSelectHeroType.enLuanDou) || (this.m_gameType == enSelectHeroType.enUnion)))
            {
                SendMuliPrepareToBattleMsg();
                uiEvent.m_srcFormScript.gameObject.transform.Find("TabList").GetComponent<CUIListScript>().SelectElement(1, true);
            }
            else
            {
                this.m_isSelectConfirm = true;
                this.RefreshHeroPanel(false);
                if (this.m_gameType == enSelectHeroType.enArenaDefTeamConfig)
                {
                    stUIEventParams par = new stUIEventParams {
                        heroIdList = new List<uint>()
                    };
                    for (int i = 0; i < this.m_selectHeroCount; i++)
                    {
                        par.heroIdList.Add(this.m_selectHeroIDList[i]);
                    }
                    Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Arena_ReciveDefTeamInfo, par);
                    Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.HeroSelect_CloseForm);
                }
                else if ((this.m_roomInfo != null) && this.m_roomInfo.roomAttrib.bWarmBattle)
                {
                    CFakePvPHelper.OnSelfHeroConfirmed();
                }
                else
                {
                    this.SendSinglePrepareToBattleMsg();
                }
            }
            if (this.isLuanDouRule())
            {
                CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
                if (form != null)
                {
                    Transform transform = form.transform.FindChild("Other/RandomHero");
                    if (transform != null)
                    {
                        CUICommonSystem.SetButtonEnableWithShader(transform.GetComponent<Button>(), false, true);
                    }
                }
            }
        }

        private void HeroSelect_Del_Hero(CUIEvent uiEvent)
        {
            int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
            if ((srcWidgetIndexInBelongedList < this.m_selectHeroIDList.Count) && (srcWidgetIndexInBelongedList >= 0))
            {
                if (this.m_selectHeroIDList[srcWidgetIndexInBelongedList] != 0)
                {
                    this.m_selectHeroIDList.RemoveAt(srcWidgetIndexInBelongedList);
                    this.m_selectHeroIDList.Add(0);
                    this.m_selectHeroCount = (byte) (this.m_selectHeroCount - 1);
                }
                this.m_showHeroID = this.m_selectHeroIDList[0];
                this.RefreshHeroPanel(false);
                this.RefreshSkinPanel(null);
            }
        }

        private void HeroSelect_OnFormClose(CUIEvent uiEvent)
        {
            if (uiEvent.m_srcWidget != null)
            {
                DynamicShadow.EnableDynamicShow(uiEvent.m_srcWidget, false);
            }
            OutlineFilter.EnableSurfaceShaderOutline(false);
            MonoSingleton<VoiceSys>.GetInstance().HeroSelectTobattle();
            this.m_mapData = null;
            this.m_mapSubType = 1;
            this.m_battleListID = 0;
            this.m_selectHeroIDList.Clear();
            this.m_selectedHeroModelIDs.Clear();
            this.m_stGameOfAdventure = null;
            this.m_stGameOfCombat = null;
            this.m_stGameOfActivity = null;
            this.m_stGameOfBurnning = null;
            this.m_stGameOfArena = null;
            this.ResetBaseProp();
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Chat_Hero_Select_CloseForm);
            Singleton<CUIManager>.instance.CloseForm(CHeroSkinBuyManager.s_buyHeroSkin3DFormPath);
            Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharShow");
            Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharSkillIcon");
            Singleton<CResourceManager>.GetInstance().UnloadUnusedAssets();
        }

        private void HeroSelect_OnMenuSelect(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                int selectedIndex = uiEvent.m_srcWidget.GetComponent<CUIListScript>().GetSelectedIndex();
                if (this.isLuanDouRule() && (selectedIndex == 0))
                {
                    form.gameObject.transform.Find("TabList").GetComponent<CUIListScript>().SelectElement(1, true);
                    Singleton<CUIManager>.instance.OpenTips(Singleton<CTextManager>.instance.GetText("Luandou_heroabandon_Tips_1"), false, 1f, null, new object[0]);
                }
                else
                {
                    Transform transform = form.gameObject.transform.Find("PanelLeft");
                    Transform transform2 = form.gameObject.transform.Find("PanelLeftSkin");
                    if (selectedIndex == 0)
                    {
                        transform.gameObject.CustomSetActive(true);
                        transform2.gameObject.CustomSetActive(false);
                        this.HeroSelect_CloseFullHeroList(null);
                        this.RefreshHeroPanel(false);
                    }
                    else
                    {
                        transform.gameObject.CustomSetActive(false);
                        transform2.gameObject.CustomSetActive(true);
                        this.HeroSelect_CloseFullSkinList(null);
                        this.RefreshSkinPanel(null);
                    }
                }
            }
        }

        private void HeroSelect_OnSkinSelect(CUIEvent uiEvent)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            uint showHeroID = this.m_showHeroID;
            uint tagUInt = uiEvent.m_eventParams.tagUInt;
            bool commonBool = uiEvent.m_eventParams.commonBool;
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                Transform transform = form.gameObject.transform.Find("PanelLeftSkin/ListHostSkinInfo");
                Transform transform2 = form.gameObject.transform.Find("PanelLeftSkin/ListHostSkinInfoFull");
                Transform transform3 = form.gameObject.transform.Find("PanelLeftSkin/ListHostSkinInfo/panelEffect/List");
                Transform transform4 = form.gameObject.transform.Find("PanelLeftSkin/ListHostSkinInfoFull/panelEffect/List");
                if (((transform != null) && (transform2 != null)) && ((transform3 != null) && (transform4 != null)))
                {
                    CUIListScript component = transform.GetComponent<CUIListScript>();
                    if (masterRoleInfo.IsCanUseSkin(showHeroID, tagUInt))
                    {
                        this.InitSkinEffect(transform3.gameObject, showHeroID, tagUInt);
                        this.InitSkinEffect(transform4.gameObject, showHeroID, tagUInt);
                    }
                    else
                    {
                        component.SelectElement(component.GetLastSelectedIndex(), true);
                    }
                    if (masterRoleInfo.IsCanUseSkin(showHeroID, tagUInt))
                    {
                        if (masterRoleInfo.GetHeroWearSkinId(showHeroID) != tagUInt)
                        {
                            CHeroInfoSystem2.ReqWearHeroSkin(showHeroID, tagUInt, true);
                        }
                    }
                    else if (!CSkinInfo.IsCanBuy(showHeroID, tagUInt))
                    {
                        Singleton<CUIManager>.GetInstance().OpenTips(Singleton<CTextManager>.instance.GetText("Skin_unpurchasable"), false, 1f, null, new object[0]);
                    }
                    else if (masterRoleInfo.IsHaveHero(showHeroID, false))
                    {
                        CHeroSkinBuyManager.OpenBuyHeroSkinForm3D(showHeroID, tagUInt, false);
                    }
                    else
                    {
                        Singleton<CUIManager>.GetInstance().OpenTips(Singleton<CTextManager>.instance.GetText("Skin_NeedToBuyAHero"), false, 1f, null, new object[0]);
                    }
                }
            }
        }

        private void HeroSelect_OpenForm(CUIEvent uiEvent)
        {
            Singleton<CRoomSystem>.GetInstance().CloseRoom();
            Singleton<CMatchingSystem>.GetInstance().CloseMatchingConfirm();
            if (Singleton<CUIManager>.instance.GetForm("Form_PvpNewProfit") != null)
            {
                Singleton<SettlementSystem>.instance.ClosePersonalProfit();
            }
            OutlineFilter.EnableSurfaceShaderOutline(true);
            this.m_gameType = uiEvent.m_eventParams.heroSelectGameType;
            if (((this.m_gameType == enSelectHeroType.enPVP) || (this.m_gameType == enSelectHeroType.enPVE_Computer)) || (((this.m_gameType == enSelectHeroType.enLadder) || (this.m_gameType == enSelectHeroType.enLuanDou)) || (this.m_gameType == enSelectHeroType.enUnion)))
            {
                this.m_heroInfoShowType = enHeroInfoShowType.enPVP;
            }
            else
            {
                this.m_heroInfoShowType = enHeroInfoShowType.enPVE;
                if (this.m_gameType == enSelectHeroType.enArenaDefTeamConfig)
                {
                    this.LoadArenaDefHeroList(uiEvent.m_eventParams.heroIdList);
                }
                else
                {
                    this.LoadPveDefaultHeroList();
                }
            }
            if (this.m_gameType == enSelectHeroType.enLadder)
            {
                this.m_hostHeroList = CHeroDataFactory.GetHostHeroList(false);
            }
            else if (((this.m_gameType == enSelectHeroType.enGuide) && (this.m_stGameOfAdventure != null)) && (this.m_stGameOfAdventure.iLevelID == GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 120).dwConfValue))
            {
                this.m_hostHeroList = CHeroDataFactory.GetTrainingHeroList();
            }
            else
            {
                this.m_hostHeroList = CHeroDataFactory.GetPvPHeroList();
            }
            CUIFormScript formScript = Singleton<CUIManager>.GetInstance().OpenForm(s_heroSelectFormPath, false, true);
            Singleton<CUIManager>.GetInstance().LoadUIScenePrefab(CUIUtility.s_heroSelectBgPath, formScript);
            if (formScript != null)
            {
                DynamicShadow.EnableDynamicShow(formScript.gameObject, true);
            }
            string[] titleList = new string[] { Singleton<CTextManager>.instance.GetText("Choose_Hero"), Singleton<CTextManager>.instance.GetText("Choose_Skin") };
            GameObject gameObject = formScript.gameObject.transform.Find("TabList").gameObject;
            int selectIndex = 0;
            if (this.isLuanDouRule())
            {
                selectIndex = 1;
                if (this.m_gameType == enSelectHeroType.enPVE_Computer)
                {
                    this.RadomHeroBySelf();
                    this.RefreshHeroPanel(false);
                }
            }
            CUICommonSystem.InitMenuPanel(gameObject, titleList, selectIndex);
            this.RefreshLeftRandCountText();
            this.InitAddedSkillPanel(formScript);
            this.OnCloseAddedSkillPanel(null);
            if (((this.m_gameType == enSelectHeroType.enPVP) || (this.m_gameType == enSelectHeroType.enLadder)) || (((this.m_gameType == enSelectHeroType.enLuanDou) || (this.m_gameType == enSelectHeroType.enUnion)) || ((this.m_roomInfo != null) && this.m_roomInfo.roomAttrib.bWarmBattle)))
            {
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Chat_Hero_Select_OpenForm);
            }
            if (((this.m_gameType == enSelectHeroType.enPVE_Computer) && (this.m_roomInfo != null)) && this.m_roomInfo.roomAttrib.bWarmBattle)
            {
                CFakePvPHelper.BeginFakeSelectHero();
            }
            MonoSingleton<NewbieGuideManager>.GetInstance().CheckTriggerTime(NewbieGuideTriggerTimeType.enterBattleHeroSel, new uint[0]);
        }

        private void HeroSelect_OpenFullHeroList(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                Transform transform = form.gameObject.transform.Find("PanelLeft/ListHostHeroInfo");
                Transform transform2 = form.gameObject.transform.Find("PanelLeft/ListHostHeroInfoFull");
                if ((transform != null) && (transform2 != null))
                {
                    transform.gameObject.CustomSetActive(false);
                    transform2.gameObject.CustomSetActive(true);
                }
                Singleton<CChatController>.instance.Hide_SelectChat_MidNode();
                Singleton<CChatController>.instance.Set_Show_Bottom(false);
            }
        }

        private void HeroSelect_OpenFullSkinList(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                Transform transform = form.gameObject.transform.Find("PanelLeftSkin/ListHostSkinInfo");
                Transform transform2 = form.gameObject.transform.Find("PanelLeftSkin/ListHostSkinInfoFull");
                if ((transform != null) && (transform2 != null))
                {
                    transform.gameObject.CustomSetActive(false);
                    transform2.gameObject.CustomSetActive(true);
                }
            }
        }

        private void HeroSelect_ReqCloseForm(CUIEvent uiEvent)
        {
            if (this.m_gameType == enSelectHeroType.enPVE_Computer)
            {
                SendQuitSingleGameReq();
            }
            else
            {
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.HeroSelect_CloseForm);
            }
        }

        private void HeroSelect_SelectHero(CUIEvent uiEvent)
        {
            if (!this.isLuanDouRule())
            {
                int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
                if (((this.m_hostHeroList != null) && (srcWidgetIndexInBelongedList >= 0)) && (srcWidgetIndexInBelongedList < this.m_hostHeroList.Count))
                {
                    IHeroData data = this.m_hostHeroList[srcWidgetIndexInBelongedList];
                    if (((data != null) && (Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo() != null)) && (this.m_selectHeroIDList.Count != 0))
                    {
                        if (data.cfgID > 0)
                        {
                            if (this.m_selectedHeroModelIDs.Count >= 3)
                            {
                                Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharShow");
                                Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharSkillIcon");
                                Singleton<CResourceManager>.GetInstance().UnloadUnusedAssets();
                                this.m_selectedHeroModelIDs.Clear();
                            }
                            if (!this.m_selectedHeroModelIDs.Contains(data.cfgID))
                            {
                                this.m_selectedHeroModelIDs.Add(data.cfgID);
                            }
                        }
                        if (((this.m_gameType == enSelectHeroType.enPVP) || (this.m_gameType == enSelectHeroType.enLadder)) || ((this.m_gameType == enSelectHeroType.enLuanDou) || (this.m_gameType == enSelectHeroType.enUnion)))
                        {
                            SendHeroSelectMsg(0, 0, data.cfgID);
                        }
                        else if (this.m_gameType == enSelectHeroType.enPVE_Computer)
                        {
                            if (this.m_roomInfo == null)
                            {
                                return;
                            }
                            MemberInfo masterMemberInfo = this.m_roomInfo.GetMasterMemberInfo();
                            if (masterMemberInfo == null)
                            {
                                return;
                            }
                            masterMemberInfo.ChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID = data.cfgID;
                            this.m_showHeroID = data.cfgID;
                            this.m_selectHeroIDList[0] = this.m_showHeroID;
                            this.m_selectHeroCount = 1;
                        }
                        else if (this.m_selectHeroIDList.Count == 1)
                        {
                            this.m_selectHeroIDList[0] = data.cfgID;
                            this.m_showHeroID = data.cfgID;
                            this.m_selectHeroCount = 1;
                        }
                        else if (this.m_selectHeroCount < this.m_selectHeroIDList.Count)
                        {
                            this.m_selectHeroIDList[this.m_selectHeroCount] = data.cfgID;
                            this.m_showHeroID = data.cfgID;
                            this.m_selectHeroCount = (byte) (this.m_selectHeroCount + 1);
                        }
                        else
                        {
                            Singleton<CUIManager>.GetInstance().OpenTips("hero is select over", false, 1f, null, new object[0]);
                        }
                        this.RefreshHeroPanel(false);
                        if (this.m_heroInfoShowType == enHeroInfoShowType.enPVP)
                        {
                            this.HeroSelect_CloseFullHeroList(null);
                        }
                        this.HeroSelect_Skill_Up(null);
                        if (CFakePvPHelper.bInFakeSelect)
                        {
                            CFakePvPHelper.OnSelfSelectHero(this.m_roomInfo.selfInfo.ullUid, data.cfgID);
                        }
                        uint[] param = new uint[] { this.m_selectHeroCount };
                        MonoSingleton<NewbieGuideManager>.GetInstance().CheckTriggerTime(NewbieGuideTriggerTimeType.heroSelectedForBattle, param);
                    }
                }
            }
        }

        private void HeroSelect_SelectTeamHero(CUIEvent uiEvent)
        {
            if (this.m_heroInfoShowType != enHeroInfoShowType.enPVP)
            {
                int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
                if ((srcWidgetIndexInBelongedList < this.m_selectHeroIDList.Count) && (srcWidgetIndexInBelongedList >= 0))
                {
                    this.m_showHeroID = this.m_selectHeroIDList[srcWidgetIndexInBelongedList];
                    this.RefreshHeroPanel(false);
                    this.RefreshSkinPanel(null);
                }
            }
        }

        private void HeroSelect_Skill_Down(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                Transform transform = form.gameObject.transform.Find("Other/PanelSkillInfo");
                if (transform != null)
                {
                    Text component = transform.Find("lblName").gameObject.GetComponent<Text>();
                    Text text2 = transform.Find("lblDesc").gameObject.GetComponent<Text>();
                    Text text3 = transform.Find("SkillCDText").gameObject.GetComponent<Text>();
                    Text text4 = transform.Find("SkillEnergyCostText").gameObject.GetComponent<Text>();
                    component.text = uiEvent.m_eventParams.skillTipParam.strTipTitle;
                    text2.text = uiEvent.m_eventParams.skillTipParam.strTipText;
                    text3.text = uiEvent.m_eventParams.skillTipParam.skillCoolDown;
                    text4.text = uiEvent.m_eventParams.skillTipParam.skillEnergyCost;
                    uint[] skillEffect = uiEvent.m_eventParams.skillTipParam.skillEffect;
                    if (skillEffect != null)
                    {
                        GameObject gameObject = null;
                        for (int i = 1; i <= 2; i++)
                        {
                            gameObject = transform.transform.Find(string.Format("EffectNode{0}", i)).gameObject;
                            if ((i <= skillEffect.Length) && (skillEffect[i - 1] != 0))
                            {
                                gameObject.CustomSetActive(true);
                                gameObject.GetComponent<Image>().SetSprite(CSkillData.GetEffectSlotBg((SkillEffectType) skillEffect[i - 1]), uiEvent.m_srcFormScript, true, false, false);
                                gameObject.transform.Find("Text").GetComponent<Text>().text = CSkillData.GetEffectDesc((SkillEffectType) skillEffect[i - 1]);
                            }
                            else
                            {
                                gameObject.CustomSetActive(false);
                            }
                        }
                        transform.gameObject.CustomSetActive(true);
                    }
                }
            }
        }

        private void HeroSelect_Skill_Up(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if ((form != null) && (form.gameObject != null))
            {
                Transform transform = form.gameObject.transform.Find("Other/PanelSkillInfo");
                GameObject obj2 = (transform == null) ? null : transform.gameObject;
                if (obj2 != null)
                {
                    obj2.gameObject.CustomSetActive(false);
                }
            }
        }

        private void ImpCalc9SlotHeroStandingPosition(ref Calc9SlotHeroData[] heroes)
        {
            List<int> list = this.HasPositionHero(ref heroes, RES_HERO_RECOMMEND_POSITION.RES_HERO_RECOMMEND_POSITION_T_FRONT);
            int index = 0;
            switch (list.Count)
            {
                case 1:
                    for (int i = 0; i < 3; i++)
                    {
                        if (heroes[i].RecommendPos == 0)
                        {
                            heroes[i].selected = true;
                            heroes[i].BornIndex = 1;
                            break;
                        }
                    }
                    index = this.WhoIsBestHero(ref heroes);
                    heroes[index].selected = true;
                    if (heroes[index].RecommendPos == 1)
                    {
                        heroes[index].BornIndex = 3;
                        index = this.WhoIsBestHero(ref heroes);
                        heroes[index].selected = true;
                        heroes[index].BornIndex = (heroes[index].RecommendPos != 1) ? 8 : 5;
                    }
                    else
                    {
                        heroes[index].BornIndex = 8;
                        index = this.WhoIsBestHero(ref heroes);
                        heroes[index].selected = true;
                        heroes[index].BornIndex = (heroes[index].RecommendPos != 1) ? 6 : 3;
                    }
                    return;

                case 2:
                    for (int j = 0; j < 3; j++)
                    {
                        if (heroes[j].RecommendPos == 1)
                        {
                            heroes[j].selected = true;
                            heroes[j].BornIndex = 3;
                            break;
                        }
                        if (heroes[j].RecommendPos == 2)
                        {
                            heroes[j].selected = true;
                            heroes[j].BornIndex = 6;
                            break;
                        }
                    }
                    break;

                case 3:
                    index = this.WhoIsBestHero(ref heroes);
                    heroes[index].selected = true;
                    heroes[index].BornIndex = 1;
                    index = this.WhoIsBestHero(ref heroes);
                    heroes[index].selected = true;
                    heroes[index].BornIndex = 0;
                    index = this.WhoIsBestHero(ref heroes);
                    heroes[index].selected = true;
                    heroes[index].BornIndex = 2;
                    return;

                default:
                    switch (this.HasPositionHero(ref heroes, RES_HERO_RECOMMEND_POSITION.RES_HERO_RECOMMEND_POSITION_T_CENTER).Count)
                    {
                        case 1:
                            for (int k = 0; k < 3; k++)
                            {
                                if (heroes[k].RecommendPos == 1)
                                {
                                    heroes[k].selected = true;
                                    heroes[k].BornIndex = 1;
                                    break;
                                }
                            }
                            index = this.WhoIsBestHero(ref heroes);
                            heroes[index].selected = true;
                            heroes[index].BornIndex = 8;
                            index = this.WhoIsBestHero(ref heroes);
                            heroes[index].selected = true;
                            heroes[index].BornIndex = 6;
                            return;

                        case 2:
                            for (int m = 0; m < 3; m++)
                            {
                                if (heroes[m].RecommendPos == 2)
                                {
                                    heroes[m].selected = true;
                                    heroes[m].BornIndex = 3;
                                    break;
                                }
                            }
                            index = this.WhoIsBestHero(ref heroes);
                            heroes[index].selected = true;
                            heroes[index].BornIndex = 1;
                            index = this.WhoIsBestHero(ref heroes);
                            heroes[index].selected = true;
                            heroes[index].BornIndex = 0;
                            return;

                        case 3:
                            index = this.WhoIsBestHero(ref heroes);
                            heroes[index].selected = true;
                            heroes[index].BornIndex = 1;
                            index = this.WhoIsBestHero(ref heroes);
                            heroes[index].selected = true;
                            heroes[index].BornIndex = 0;
                            index = this.WhoIsBestHero(ref heroes);
                            heroes[index].selected = true;
                            heroes[index].BornIndex = 2;
                            return;
                    }
                    index = this.WhoIsBestHero(ref heroes);
                    heroes[index].selected = true;
                    heroes[index].BornIndex = 4;
                    index = this.WhoIsBestHero(ref heroes);
                    heroes[index].selected = true;
                    heroes[index].BornIndex = 3;
                    index = this.WhoIsBestHero(ref heroes);
                    heroes[index].selected = true;
                    heroes[index].BornIndex = 5;
                    return;
            }
            index = this.WhoIsBestHero(ref heroes);
            heroes[index].selected = true;
            heroes[index].BornIndex = 1;
            index = this.WhoIsBestHero(ref heroes);
            heroes[index].selected = true;
            heroes[index].BornIndex = 0;
        }

        public override void Init()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_OpenForm, new CUIEventManager.OnUIEventHandler(this.HeroSelect_OpenForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_CloseForm, new CUIEventManager.OnUIEventHandler(this.HeroSelect_CloseForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_FormClose, new CUIEventManager.OnUIEventHandler(this.HeroSelect_OnFormClose));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_SelectHero, new CUIEventManager.OnUIEventHandler(this.HeroSelect_SelectHero));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_LeftHeroItemEnable, new CUIEventManager.OnUIEventHandler(this.LeftHeroItemEnable));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_SelectTeamHero, new CUIEventManager.OnUIEventHandler(this.HeroSelect_SelectTeamHero));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_ConfirmHeroSelect, new CUIEventManager.OnUIEventHandler(this.HeroSelect_ConfirmHeroSelect));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_Del_Hero, new CUIEventManager.OnUIEventHandler(this.HeroSelect_Del_Hero));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_Skill_Down, new CUIEventManager.OnUIEventHandler(this.HeroSelect_Skill_Down));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_Skill_Up, new CUIEventManager.OnUIEventHandler(this.HeroSelect_Skill_Up));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_OpenFullHeroList, new CUIEventManager.OnUIEventHandler(this.HeroSelect_OpenFullHeroList));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_CloseFullHeroList, new CUIEventManager.OnUIEventHandler(this.HeroSelect_CloseFullHeroList));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_SymbolPageSelect, new CUIEventManager.OnUIEventHandler(this.OnHeroSymbolPageSelect));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_PageDownBtnClick, new CUIEventManager.OnUIEventHandler(this.OnSymbolPageDownBtnClick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_ReqCloseForm, new CUIEventManager.OnUIEventHandler(this.HeroSelect_ReqCloseForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_ViewProp_Down, new CUIEventManager.OnUIEventHandler(this.OnOpenSymbolProp));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_ViewProp_Up, new CUIEventManager.OnUIEventHandler(this.OnCloseSymbolProp));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_MenuSelect, new CUIEventManager.OnUIEventHandler(this.HeroSelect_OnMenuSelect));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_RefreshSkinPanel, new CUIEventManager.OnUIEventHandler(this.RefreshSkinPanel));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_OpenFullSkinList, new CUIEventManager.OnUIEventHandler(this.HeroSelect_OpenFullSkinList));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_CloseFullSkinList, new CUIEventManager.OnUIEventHandler(this.HeroSelect_CloseFullSkinList));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_SkinSelect, new CUIEventManager.OnUIEventHandler(this.HeroSelect_OnSkinSelect));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_AddedSkillSelected, new CUIEventManager.OnUIEventHandler(this.OnSelectedAddedSkill));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_AddedSkillOpenForm, new CUIEventManager.OnUIEventHandler(this.OnOpenAddedSkillPanel));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_AddedSkillCloseForm, new CUIEventManager.OnUIEventHandler(this.OnCloseAddedSkillPanel));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_AddedSkillConfirm, new CUIEventManager.OnUIEventHandler(this.OnConfirmAddedSkill));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSelect_RandomHero, new CUIEventManager.OnUIEventHandler(this.OnReqRandHero));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroCount_Buy, new CUIEventManager.OnUIEventHandler(this.BuyHeroCount));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroCount_CancelBuy, new CUIEventManager.OnUIEventHandler(this.OnCancelBuy));
        }

        public void InitAddedSkillPanel(CUIFormScript form)
        {
            if (Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo() != null)
            {
                if (this.IsSelSkillAvailable())
                {
                    CUIToggleListScript component = form.transform.Find("PanelAddSkill/ToggleList").GetComponent<CUIToggleListScript>();
                    CUIListElementScript elemenet = null;
                    CUIEventScript script3 = null;
                    ResSkillUnlock dataByIndex = null;
                    ResSkillCfgInfo dataByKey = null;
                    uint key = 0;
                    ListView<ResSkillUnlock> selSkillAvailable = this.GetSelSkillAvailable();
                    component.SetElementAmount(selSkillAvailable.Count);
                    int index = 0;
                    for (int i = 0; i < selSkillAvailable.Count; i++)
                    {
                        elemenet = component.GetElemenet(i);
                        script3 = elemenet.GetComponent<CUIEventScript>();
                        dataByIndex = selSkillAvailable[i];
                        key = dataByIndex.dwUnlockSkillID;
                        dataByKey = GameDataMgr.skillDatabin.GetDataByKey(key);
                        if (dataByKey != null)
                        {
                            script3.m_onClickEventID = enUIEventID.HeroSelect_AddedSkillSelected;
                            script3.m_onClickEventParams.tag = (int) dataByIndex.dwUnlockSkillID;
                            string prefabPath = string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Skill_Dir, Utility.UTF8Convert(dataByKey.szIconPath));
                            elemenet.transform.Find("Icon").GetComponent<Image>().SetSprite(prefabPath, form.GetComponent<CUIFormScript>(), true, false, false);
                            elemenet.transform.Find("SkillNameTxt").GetComponent<Text>().text = Utility.UTF8Convert(dataByKey.szSkillName);
                        }
                        else
                        {
                            DebugHelper.Assert(false, string.Format("ResSkillCfgInfo[{0}] can not be found!", key));
                        }
                    }
                    component.SelectElement(index, true);
                    dataByIndex = GameDataMgr.addedSkiilDatabin.GetDataByIndex(index);
                    form.transform.Find("Other/SkillList/AddedSkillItem").gameObject.CustomSetActive(selSkillAvailable.Count > 0);
                }
                else
                {
                    form.transform.Find("Other/SkillList/AddedSkillItem").gameObject.CustomSetActive(false);
                }
            }
        }

        public void InitSelectHeroIDListAndBattleID(byte heroMaxCount, uint battleListID)
        {
            this.m_selectHeroIDList.Clear();
            for (int i = 0; i < heroMaxCount; i++)
            {
                this.m_selectHeroIDList.Add(0);
            }
            this.m_battleListID = battleListID;
            this.m_showHeroID = 0;
            this.m_selectHeroCount = 0;
        }

        private void InitSkinEffect(GameObject objList, uint heroID, uint skinID)
        {
            CSkinInfo.GetHeroSkinProp(heroID, skinID, ref s_propArr, ref s_propPctArr);
            CUICommonSystem.SetListProp(objList, ref s_propArr, ref s_propPctArr);
        }

        private bool IsBetterHero(ref Calc9SlotHeroData heroe1, ref Calc9SlotHeroData heroe2)
        {
            return (((heroe1.ConfigId > 0) && !heroe1.selected) && (((((heroe2.ConfigId == 0) || heroe2.selected) || (heroe1.Ability > heroe2.Ability)) || ((heroe1.Ability == heroe2.Ability) && (heroe1.Level > heroe2.Level))) || (((heroe1.Ability == heroe2.Ability) && (heroe1.Level == heroe2.Level)) && (heroe1.Quality >= heroe2.Quality))));
        }

        private bool IsHeroCanUse(uint heroID)
        {
            bool flag = false;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                if (masterRoleInfo.GetHeroInfoDic().ContainsKey(heroID))
                {
                    return true;
                }
                if (masterRoleInfo.freeHeroList.Contains(heroID))
                {
                    flag = true;
                }
            }
            return flag;
        }

        private bool IsHeroExist(uint heroID)
        {
            if (Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo() == null)
            {
                return false;
            }
            bool flag = true;
            if (this.m_heroInfoShowType == enHeroInfoShowType.enPVP)
            {
                if (((this.m_mapData != null) && (this.m_mapData.bIsAllowHeroDup == 0)) && (this.m_roomInfo != null))
                {
                    MemberInfo masterMemberInfo = this.m_roomInfo.GetMasterMemberInfo();
                    if (masterMemberInfo == null)
                    {
                        string str = this.m_roomInfo.selfObjID.ToString();
                    }
                    if (masterMemberInfo == null)
                    {
                        return flag;
                    }
                    return this.m_roomInfo.IsHeroExistWithCamp(masterMemberInfo.camp, heroID);
                }
                return false;
            }
            if ((this.m_selectHeroIDList != null) && ((this.m_selectHeroCount < this.m_selectHeroIDList.Count) || (this.m_selectHeroIDList.Count == 1)))
            {
                return this.m_selectHeroIDList.Contains(heroID);
            }
            return true;
        }

        public bool isLuanDouRule()
        {
            if (this.m_gameType == enSelectHeroType.enLuanDou)
            {
                return true;
            }
            if (((this.m_gameType == enSelectHeroType.enPVE_Computer) && (this.m_stGameOfCombat != null)) && (this.m_stGameOfCombat.bMapType == 4))
            {
                return true;
            }
            if (this.m_gameType != enSelectHeroType.enUnion)
            {
                return false;
            }
            return (this.m_mapSubType == 2);
        }

        public bool IsSelSkillAvailable()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            return ((((this.m_gameType == enSelectHeroType.enPVP) || (this.m_gameType == enSelectHeroType.enPVE_Computer)) || (((this.m_gameType == enSelectHeroType.enLadder) || (this.m_gameType == enSelectHeroType.enLuanDou)) || (this.m_gameType == enSelectHeroType.enUnion))) && (Singleton<CFunctionUnlockSys>.GetInstance().FucIsUnlock(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_ADDEDSKILL) && (masterRoleInfo != null)));
        }

        private bool IsSelSkillAvailable(enSelectHeroType gameType, uint selSkillId)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            ResSkillUnlock unlock = null;
            ResSkillUnlock dataByIndex = null;
            for (int i = 0; i < GameDataMgr.addedSkiilDatabin.count; i++)
            {
                dataByIndex = GameDataMgr.addedSkiilDatabin.GetDataByIndex(i);
                if (dataByIndex.dwUnlockSkillID == selSkillId)
                {
                    unlock = dataByIndex;
                    break;
                }
            }
            if (((unlock == null) || (masterRoleInfo == null)) || (masterRoleInfo.PvpLevel < unlock.wAcntLevel))
            {
                return false;
            }
            if (this.isLuanDouRule() && (unlock.bEntertainmentValid == 0))
            {
                return false;
            }
            return true;
        }

        public bool IsShowPveHeroInfo()
        {
            return (this.m_heroInfoShowType == enHeroInfoShowType.enPVE);
        }

        private void LeftHeroItemEnable(CUIEvent uiEvent)
        {
            CUIFormScript srcFormScript = uiEvent.m_srcFormScript;
            CUIListScript srcWidgetBelongedListScript = uiEvent.m_srcWidgetBelongedListScript;
            GameObject srcWidget = uiEvent.m_srcWidget;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
            if ((((srcFormScript != null) && (srcWidgetBelongedListScript != null)) && ((srcWidget != null) && (masterRoleInfo != null))) && ((srcWidgetIndexInBelongedList >= 0) && (srcWidgetIndexInBelongedList < this.m_hostHeroList.Count)))
            {
                CUIListElementScript component = srcWidget.GetComponent<CUIListElementScript>();
                if (component != null)
                {
                    Image image = srcWidget.transform.Find("imgHP").gameObject.GetComponent<Image>();
                    Image image2 = srcWidget.transform.Find("imgDead").gameObject.GetComponent<Image>();
                    GameObject gameObject = srcWidget.transform.Find("heroItemCell").gameObject;
                    GameObject obj4 = gameObject.transform.Find("TxtFree").gameObject;
                    GameObject obj5 = gameObject.transform.Find("imgExperienceMark").gameObject;
                    CUIEventScript script4 = gameObject.GetComponent<CUIEventScript>();
                    CUIEventScript script5 = srcWidget.GetComponent<CUIEventScript>();
                    IHeroData data = this.m_hostHeroList[srcWidgetIndexInBelongedList];
                    if (data != null)
                    {
                        obj4.CustomSetActive(masterRoleInfo.IsFreeHero(data.cfgID));
                        obj5.CustomSetActive(masterRoleInfo.IsValidExperienceHero(data.cfgID));
                        image.gameObject.CustomSetActive(false);
                        image2.gameObject.CustomSetActive(false);
                        script4.enabled = false;
                        script5.enabled = false;
                        if (!this.m_isSelectConfirm)
                        {
                            bool flag = this.IsHeroExist(data.cfgID);
                            if (this.m_gameType == enSelectHeroType.enBurning)
                            {
                                if (Singleton<BurnExpeditionController>.instance.model.IsHeroInRecord(data.cfgID))
                                {
                                    int num2 = Singleton<BurnExpeditionController>.instance.model.Get_HeroHP(data.cfgID);
                                    int num3 = Singleton<BurnExpeditionController>.instance.model.Get_HeroMaxHP(data.cfgID);
                                    if (num2 <= 0)
                                    {
                                        flag = true;
                                        image2.gameObject.CustomSetActive(true);
                                    }
                                    else
                                    {
                                        image.CustomFillAmount(((float) num2) / (num3 * 1f));
                                        image.gameObject.CustomSetActive(true);
                                    }
                                }
                                else
                                {
                                    image.CustomFillAmount(1f);
                                    image.gameObject.CustomSetActive(true);
                                }
                            }
                            if (!flag)
                            {
                                script4.enabled = true;
                                script5.enabled = true;
                                CUICommonSystem.SetHeroItemData(srcFormScript, gameObject, data, enHeroHeadType.enIcon, false, this.m_heroInfoShowType);
                            }
                            else
                            {
                                CUICommonSystem.SetHeroItemData(srcFormScript, gameObject, data, enHeroHeadType.enIcon, true, this.m_heroInfoShowType);
                            }
                            if (this.m_selectHeroCount > 0)
                            {
                                if (data.cfgID == this.m_selectHeroIDList[this.m_selectHeroCount - 1])
                                {
                                    component.ChangeDisplay(true);
                                }
                                else
                                {
                                    component.ChangeDisplay(false);
                                }
                            }
                            else
                            {
                                component.ChangeDisplay(false);
                            }
                        }
                        else
                        {
                            CUICommonSystem.SetHeroItemData(srcFormScript, gameObject, data, enHeroHeadType.enIcon, true, this.m_heroInfoShowType);
                        }
                    }
                }
            }
        }

        public void LoadArenaDefHeroList(List<uint> defaultHeroList)
        {
            for (int i = 0; i < defaultHeroList.Count; i++)
            {
                uint heroID = defaultHeroList[i];
                if (!this.IsHeroCanUse(heroID))
                {
                    heroID = 0;
                }
                if (heroID != 0)
                {
                    this.m_selectHeroIDList[this.m_selectHeroCount] = heroID;
                    this.m_showHeroID = heroID;
                    this.m_selectHeroCount = (byte) (this.m_selectHeroCount + 1);
                }
            }
        }

        public void LoadPveDefaultHeroList()
        {
            if (this.m_defaultBattleListInfo != null)
            {
                for (int i = 0; i < this.m_defaultBattleListInfo.dwListNum; i++)
                {
                    if (this.m_defaultBattleListInfo.astBattleList[i].dwBattleListID == this.m_battleListID)
                    {
                        COMDT_BATTLEHERO stBattleList = this.m_defaultBattleListInfo.astBattleList[i].stBattleList;
                        for (int j = 0; j < stBattleList.BattleHeroList.Length; j++)
                        {
                            if (j < this.m_selectHeroIDList.Count)
                            {
                                uint heroCfgID = stBattleList.BattleHeroList[j];
                                if (((this.m_gameType == enSelectHeroType.enBurning) && Singleton<BurnExpeditionController>.instance.model.IsHeroInRecord(heroCfgID)) && (Singleton<BurnExpeditionController>.instance.model.Get_HeroHP(heroCfgID) <= 0))
                                {
                                    heroCfgID = 0;
                                }
                                if (!this.IsHeroCanUse(heroCfgID))
                                {
                                    heroCfgID = 0;
                                }
                                if (heroCfgID != 0)
                                {
                                    this.m_selectHeroIDList[this.m_selectHeroCount] = heroCfgID;
                                    this.m_showHeroID = heroCfgID;
                                    this.m_selectHeroCount = (byte) (this.m_selectHeroCount + 1);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                this.m_defaultBattleListInfo = new COMDT_BATTLELIST_LIST();
                this.m_defaultBattleListInfo.dwListNum = 0;
            }
        }

        private void OnCancelBuy(CUIEvent uiEvent)
        {
            if (uiEvent.m_srcFormScript.transform != null)
            {
                CUICommonSystem.SetButtonEnableWithShader(uiEvent.m_srcFormScript.transform.FindChild("Other/RandomHero").GetComponent<Button>(), true, true);
            }
        }

        public void OnCloseAddedSkillPanel(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                form.transform.Find("PanelAddSkill").gameObject.CustomSetActive(false);
                Singleton<CChatController>.instance.Set_Show_Bottom(true);
            }
        }

        private void OnCloseSymbolProp(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                form.gameObject.transform.Find(s_symbolPropPanelPath).gameObject.gameObject.CustomSetActive(false);
            }
        }

        public void OnConfirmAddedSkill(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                uint tag = (uint) uiEvent.m_eventParams.tag;
                this.RefreshAddedSkillItem(form, tag, true);
                CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x48e);
                msg.stPkgData.stUnlockSkillSelReq.dwHeroID = this.m_showHeroID;
                msg.stPkgData.stUnlockSkillSelReq.dwSkillID = tag;
                Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
                this.OnCloseAddedSkillPanel(null);
            }
        }

        public void OnHeroCountBought()
        {
            if (this.isLuanDouRule())
            {
                if (this.m_gameType == enSelectHeroType.enPVE_Computer)
                {
                    this.RadomHeroBySelf();
                    this.RefreshHeroPanel(false);
                    this.RefreshSkinPanel(null);
                }
                this.m_UseRandSelCount++;
                this.RefreshLeftRandCountText();
            }
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
        }

        public void OnHeroSkinWearSuc(uint heroId, uint skinId)
        {
            if (Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath) != null)
            {
                Singleton<CHeroSelectSystem>.GetInstance().m_nowShowHeroID = 0;
                Singleton<CHeroSelectSystem>.GetInstance().RefreshHeroPanel(false);
                Singleton<CHeroSelectSystem>.GetInstance().RefreshSkinPanel(null);
            }
        }

        public void OnHeroSymbolPageSelect(CUIEvent uiEvent)
        {
            CHeroInfo info2;
            int selectedIndex = uiEvent.m_srcWidget.GetComponent<CUIListScript>().GetSelectedIndex();
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            uiEvent.m_srcFormScript.gameObject.transform.Find("Other").Find("Panel_SymbolChange/DropList/List").gameObject.CustomSetActive(false);
            bool flag = masterRoleInfo.GetHeroInfo(this.m_showHeroID, out info2, true);
            if (flag && (selectedIndex != info2.m_selectPageIndex))
            {
                SendHeroSelectSymbolPage(this.m_showHeroID, selectedIndex, (((this.m_gameType == enSelectHeroType.enPVP) || (this.m_gameType == enSelectHeroType.enLadder)) || (this.m_gameType == enSelectHeroType.enLuanDou)) || (this.m_gameType == enSelectHeroType.enUnion));
            }
            else if ((!flag && masterRoleInfo.IsFreeHero(this.m_showHeroID)) && (selectedIndex != masterRoleInfo.GetFreeHeroSymbolId(this.m_showHeroID)))
            {
                SendHeroSelectSymbolPage(this.m_showHeroID, selectedIndex, (((this.m_gameType == enSelectHeroType.enPVP) || (this.m_gameType == enSelectHeroType.enLadder)) || (this.m_gameType == enSelectHeroType.enLuanDou)) || (this.m_gameType == enSelectHeroType.enUnion));
            }
        }

        public void OnOpenAddedSkillPanel(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                this.RefreshHeroPanel(true);
                form.transform.Find("PanelAddSkill").gameObject.CustomSetActive(true);
                Singleton<CChatController>.instance.Hide_SelectChat_MidNode();
                Singleton<CChatController>.instance.Set_Show_Bottom(false);
            }
        }

        private void OnOpenSymbolProp(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                CHeroInfo info2;
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                if (masterRoleInfo.GetHeroInfo(this.m_showHeroID, out info2, true))
                {
                    this.OpenSymbolPropPanel(form, info2.m_selectPageIndex);
                }
                else if (masterRoleInfo.IsFreeHero(this.m_showHeroID))
                {
                    int freeHeroSymbolId = masterRoleInfo.GetFreeHeroSymbolId(this.m_showHeroID);
                    this.OpenSymbolPropPanel(form, freeHeroSymbolId);
                }
            }
        }

        public void OnReqRandHero(CUIEvent uiEvent)
        {
            if (this.isLuanDouRule())
            {
                ResShopInfo cfgShopInfo = CPurchaseSys.GetCfgShopInfo(RES_SHOPBUY_TYPE.RES_BUYTYPE_ENTERTAINMENTRANDHERO, this.m_UseRandSelCount + 1);
                if (cfgShopInfo != null)
                {
                    Transform transform = uiEvent.m_srcFormScript.transform.FindChild("Other/RandomHero");
                    CUICommonSystem.SetButtonEnableWithShader(transform.GetComponent<Button>(), false, true);
                    transform.transform.Find("Timer").GetComponent<CUITimerScript>().ReStartTimer();
                    uint dwCoinPrice = cfgShopInfo.dwCoinPrice;
                    int dwValue = (int) cfgShopInfo.dwValue;
                    enPayType payType = CMallSystem.ResBuyTypeToPayType(cfgShopInfo.bCoinType);
                    stUIEventParams confirmEventParams = new stUIEventParams();
                    CMallSystem.TryToPay(enPayPurpose.Buy, string.Empty, payType, dwCoinPrice, enUIEventID.HeroCount_Buy, ref confirmEventParams, enUIEventID.None, false, false);
                }
            }
        }

        public void OnSelectedAddedSkill(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                uint tag = (uint) uiEvent.m_eventParams.tag;
                form.transform.Find("PanelAddSkill/btnConfirm").GetComponent<CUIEventScript>().m_onClickEventParams.tag = (int) tag;
                string pvpHeroSkillDesc = CUICommonSystem.GetPvpHeroSkillDesc((int) tag, this.m_showHeroID);
                form.transform.Find("PanelAddSkill/AddSkilltxt").GetComponent<Text>().text = pvpHeroSkillDesc;
            }
        }

        public void OnSymbolPageChange()
        {
            this.RefreshHeroInfo_DropList();
        }

        public void OnSymbolPageDownBtnClick(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                Transform transform = form.gameObject.transform.Find("Other/Panel_SymbolChange/DropList/List");
                transform.gameObject.CustomSetActive(!transform.gameObject.activeSelf);
            }
        }

        private void OpenSymbolPropPanel(CUIFormScript form, int pageIndex)
        {
            GameObject gameObject = form.transform.Find(s_symbolPropPanelPath).gameObject;
            GameObject obj3 = gameObject.gameObject.transform.Find("basePropPanel").gameObject;
            GameObject obj4 = gameObject.gameObject.transform.Find("enhancePropPanel").gameObject;
            GameObject propListPanel = obj3.transform.Find("List").gameObject;
            Singleton<CSymbolSystem>.GetInstance().RefreshSymbolPageProp(propListPanel, pageIndex, true);
            obj4.CustomSetActive(this.m_heroInfoShowType != enHeroInfoShowType.enPVP);
            if (this.m_heroInfoShowType != enHeroInfoShowType.enPVP)
            {
                GameObject propList = obj4.transform.Find("List").gameObject;
                Singleton<CSymbolSystem>.GetInstance().RefreshSymbolPagePveEnhanceProp(propList, pageIndex);
            }
            gameObject.gameObject.CustomSetActive(true);
        }

        private void PostAdventureSingleGame(CSDT_BATTLE_PLAYER_BRIEF stBattlePlayer)
        {
            ResLevelCfgInfo dataByKey = GameDataMgr.levelDatabin.GetDataByKey(Singleton<CAdventureSys>.instance.currentLevelId);
            if ((dataByKey != null) && ((dataByKey.iLevelType == 0) || ((dataByKey.iLevelType == 4) && Singleton<CBattleGuideManager>.instance.bTrainingAdv)))
            {
                uint dwAIPlayerLevel = dataByKey.dwAIPlayerLevel;
                uint[] aIHeroID = dataByKey.AIHeroID;
                stBattlePlayer.astFighter[0].bObjType = 1;
                stBattlePlayer.astFighter[0].bPosOfCamp = 0;
                stBattlePlayer.astFighter[0].bObjCamp = 1;
                for (int i = 0; i < this.m_selectHeroIDList.Count; i++)
                {
                    stBattlePlayer.astFighter[0].astChoiceHero[i].stBaseInfo.stCommonInfo.dwHeroID = this.m_selectHeroIDList[i];
                }
                int index = 1;
                for (int j = 0; j < dataByKey.SelfCampAIHeroID.Length; j++)
                {
                    if (dataByKey.SelfCampAIHeroID[j] != 0)
                    {
                        stBattlePlayer.astFighter[index].bPosOfCamp = (byte) (j + 1);
                        stBattlePlayer.astFighter[index].bObjType = 2;
                        stBattlePlayer.astFighter[index].bObjCamp = 1;
                        stBattlePlayer.astFighter[index].astChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID = dataByKey.SelfCampAIHeroID[j];
                        index++;
                    }
                }
                for (int k = 0; k < dataByKey.AIHeroID.Length; k++)
                {
                    if (dataByKey.AIHeroID[k] != 0)
                    {
                        stBattlePlayer.astFighter[index].bPosOfCamp = (byte) k;
                        stBattlePlayer.astFighter[index].bObjType = 2;
                        stBattlePlayer.astFighter[index].bObjCamp = 2;
                        stBattlePlayer.astFighter[index].astChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID = dataByKey.AIHeroID[k];
                        index++;
                    }
                }
                stBattlePlayer.bNum = (byte) index;
            }
        }

        private void PostArenaSingleGame(CSDT_BATTLE_PLAYER_BRIEF stBattlePlayer)
        {
            stBattlePlayer.bNum = 2;
            stBattlePlayer.astFighter[0].bObjType = 1;
            stBattlePlayer.astFighter[0].bPosOfCamp = 0;
            stBattlePlayer.astFighter[0].bObjCamp = 1;
            for (int i = 0; i < this.m_selectHeroIDList.Count; i++)
            {
                stBattlePlayer.astFighter[0].astChoiceHero[i].stBaseInfo.stCommonInfo.dwHeroID = this.m_selectHeroIDList[i];
            }
            COMDT_ARENA_MEMBER_OF_ACNT tarInfo = Singleton<CArenaSystem>.GetInstance().m_tarInfo;
            stBattlePlayer.astFighter[1].bObjType = 2;
            stBattlePlayer.astFighter[1].bPosOfCamp = 0;
            stBattlePlayer.astFighter[1].dwLevel = tarInfo.dwPVPLevel;
            stBattlePlayer.astFighter[1].bObjCamp = 2;
            for (int j = 0; j < tarInfo.stBattleHero.astHero.Length; j++)
            {
                stBattlePlayer.astFighter[1].astChoiceHero[j].stBaseInfo.stCommonInfo.dwHeroID = tarInfo.stBattleHero.astHero[j].dwHeroId;
            }
        }

        private void PostBurningSingleGame(CSDT_BATTLE_PLAYER_BRIEF stBattlePlayer)
        {
            BurnExpeditionModel model = Singleton<BurnExpeditionController>.instance.model;
            List<uint> list = model.Get_Enemy_HeroIDS();
            COMDT_PLAYERINFO comdt_playerinfo = model.Get_Current_Enemy_PlayerInfo();
            stBattlePlayer.bNum = 2;
            stBattlePlayer.astFighter[0].bObjType = 1;
            stBattlePlayer.astFighter[0].bPosOfCamp = 0;
            stBattlePlayer.astFighter[0].bObjCamp = 1;
            for (int i = 0; i < this.m_selectHeroIDList.Count; i++)
            {
                stBattlePlayer.astFighter[0].astChoiceHero[i].stBaseInfo.stCommonInfo.dwHeroID = this.m_selectHeroIDList[i];
            }
            stBattlePlayer.astFighter[1].bObjType = 2;
            stBattlePlayer.astFighter[1].bPosOfCamp = 0;
            stBattlePlayer.astFighter[1].dwLevel = comdt_playerinfo.dwLevel;
            stBattlePlayer.astFighter[1].bObjCamp = 2;
            comdt_playerinfo.szName.CopyTo(stBattlePlayer.astFighter[1].szName, 0);
            for (int j = 0; j < list.Count; j++)
            {
                for (int k = 0; k < comdt_playerinfo.astChoiceHero.Length; k++)
                {
                    if (comdt_playerinfo.astChoiceHero[k].stBaseInfo.stCommonInfo.dwHeroID == list[j])
                    {
                        if (comdt_playerinfo.astChoiceHero[k].stBurningInfo.bIsDead == 0)
                        {
                            stBattlePlayer.astFighter[1].astChoiceHero[j].stBaseInfo.stCommonInfo.dwHeroID = list[j];
                        }
                        break;
                    }
                }
            }
        }

        private void PostCombatSingleGame(CSDT_BATTLE_PLAYER_BRIEF stBattlePlayer)
        {
            <PostCombatSingleGame>c__AnonStorey43 storey = new <PostCombatSingleGame>c__AnonStorey43();
            ResDT_LevelCommonInfo stLevelCommonInfo = null;
            if (this.m_roomInfo.roomAttrib.bMapType == 2)
            {
                ResCounterPartLevelInfo dataByKey = GameDataMgr.cpLevelDatabin.GetDataByKey(Singleton<CRoomSystem>.GetInstance().roomInfo.roomAttrib.dwMapId);
                DebugHelper.Assert(dataByKey != null);
                stLevelCommonInfo = dataByKey.stLevelCommonInfo;
            }
            else
            {
                ResAcntBattleLevelInfo info3 = GameDataMgr.pvpLevelDatabin.GetDataByKey(Singleton<CRoomSystem>.GetInstance().roomInfo.roomAttrib.dwMapId);
                DebugHelper.Assert(info3 != null);
                stLevelCommonInfo = info3.stLevelCommonInfo;
            }
            COM_PLAYERCAMP camp = COM_PLAYERCAMP.COM_PLAYERCAMP_1;
            int dwHeroID = 0;
            for (int i = 0; i < this.m_roomInfo.CampMemberList.Length; i++)
            {
                ListView<MemberInfo> view = this.m_roomInfo.CampMemberList[i];
                for (int k = 0; k < view.Count; k++)
                {
                    if (view[k].ullUid == this.m_roomInfo.selfInfo.ullUid)
                    {
                        camp = view[k].camp;
                        dwHeroID = (int) view[k].ChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID;
                        break;
                    }
                }
            }
            List<uint>[] listArray = new List<uint>[2];
            if (stLevelCommonInfo.bIsAllowHeroDup == 0)
            {
                listArray[0] = new List<uint>();
                listArray[1] = new List<uint>();
                listArray[(camp != COM_PLAYERCAMP.COM_PLAYERCAMP_1) ? 1 : 0] = Singleton<CHeroSelectSystem>.GetInstance().GetPveTeamHeroIDList();
            }
            int index = 0;
            storey.canPickHeroNum = 0;
            GameDataMgr.heroDatabin.Accept(new Action<ResHeroCfgInfo>(storey.<>m__37));
            DebugHelper.Assert(storey.canPickHeroNum >= 3, "Not Enough Hero To Pick!!!");
            for (int j = 0; j < 2; j++)
            {
                for (int m = 0; m < this.m_roomInfo.CampMemberList[j].Count; m++)
                {
                    MemberInfo info4 = this.m_roomInfo.CampMemberList[j][m];
                    if (info4.RoomMemberType == 2)
                    {
                        stBattlePlayer.astFighter[index].bObjType = 2;
                        stBattlePlayer.astFighter[index].bPosOfCamp = (byte) m;
                        stBattlePlayer.astFighter[index].bObjCamp = (byte) (j + 1);
                        stBattlePlayer.astFighter[index].dwLevel = 1;
                        for (int n = 0; n < stLevelCommonInfo.bHeroNum; n++)
                        {
                            <PostCombatSingleGame>c__AnonStorey44 storey2 = new <PostCombatSingleGame>c__AnonStorey44();
                            int id = UnityEngine.Random.Range(0, GameDataMgr.heroDatabin.Count());
                            storey2.heroCfg = GameDataMgr.heroDatabin.GetDataByIndex(id);
                            while (storey2.heroCfg.bIsPlayerUse == 0)
                            {
                                id = UnityEngine.Random.Range(0, GameDataMgr.heroDatabin.Count());
                                storey2.heroCfg = GameDataMgr.heroDatabin.GetDataByIndex(id);
                            }
                            if (stLevelCommonInfo.bIsAllowHeroDup == 0)
                            {
                                while (((listArray[j].FindIndex(new Predicate<uint>(storey2.<>m__38)) != -1) || (storey2.heroCfg.bIsPlayerUse == 0)) || (CSysDynamicBlock.bLobbyEntryBlocked && (storey2.heroCfg.bIOSHide == 1)))
                                {
                                    id = UnityEngine.Random.Range(0, GameDataMgr.heroDatabin.Count());
                                    storey2.heroCfg = GameDataMgr.heroDatabin.GetDataByIndex(id);
                                }
                                listArray[j].Add(storey2.heroCfg.dwCfgID);
                            }
                            stBattlePlayer.astFighter[index].astChoiceHero[n].stBaseInfo.stCommonInfo.dwHeroID = storey2.heroCfg.dwCfgID;
                        }
                    }
                    else if (info4.RoomMemberType == 1)
                    {
                        stBattlePlayer.astFighter[index].bObjType = 1;
                        stBattlePlayer.astFighter[index].bPosOfCamp = (byte) m;
                        stBattlePlayer.astFighter[index].bObjCamp = (byte) camp;
                        for (int num9 = 0; num9 < stLevelCommonInfo.bHeroNum; num9++)
                        {
                            stBattlePlayer.astFighter[index].astChoiceHero[num9].stBaseInfo.stCommonInfo.dwHeroID = (uint) dwHeroID;
                        }
                    }
                    index++;
                }
            }
            stBattlePlayer.bNum = (byte) index;
        }

        private void RadomHeroBySelf()
        {
            bool flag = false;
            List<uint> list = new List<uint>();
            ResBanHeroConf dataByKey = GameDataMgr.banHeroBin.GetDataByKey(GameDataMgr.GetDoubleKey(4, this.m_roomInfo.roomAttrib.dwMapId));
            if (dataByKey != null)
            {
                for (int i = 0; i < dataByKey.BanHero.Length; i++)
                {
                    if (dataByKey.BanHero[i] != 0)
                    {
                        list.Add(dataByKey.BanHero[i]);
                    }
                }
            }
            while (!flag)
            {
                IHeroData data = this.m_hostHeroList[UnityEngine.Random.Range(0, this.m_hostHeroList.Count)];
                if (((this.m_selectHeroIDList[0] != data.cfgID) && !list.Contains(data.cfgID)) || (this.m_hostHeroList.Count == 1))
                {
                    this.m_showHeroID = data.cfgID;
                    this.m_selectHeroIDList[0] = this.m_showHeroID;
                    this.m_selectHeroCount = 1;
                    MemberInfo masterMemberInfo = this.m_roomInfo.GetMasterMemberInfo();
                    if (masterMemberInfo == null)
                    {
                        return;
                    }
                    masterMemberInfo.ChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID = data.cfgID;
                    flag = true;
                }
            }
        }

        [MessageHandler(0x48f)]
        public static void ReceiveAddedSkillSel(CSPkg msg)
        {
            if (msg.stPkgData.stUnlockSkillSelRsp.iResult == 0)
            {
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
                if ((masterRoleInfo != null) && (masterRoleInfo.playerUllUID == msg.stPkgData.stUnlockSkillSelRsp.ullAcntUid))
                {
                    masterRoleInfo.SetHeroSelSkillID(msg.stPkgData.stUnlockSkillSelRsp.dwHeroID, msg.stPkgData.stUnlockSkillSelRsp.dwSkillID);
                }
                Assets.Scripts.GameSystem.RoomInfo roomInfo = Singleton<CHeroSelectSystem>.instance.m_roomInfo;
                if (roomInfo != null)
                {
                    MemberInfo memberInfo = roomInfo.GetMemberInfo(msg.stPkgData.stUnlockSkillSelRsp.ullAcntUid);
                    if ((memberInfo != null) && (memberInfo.ChoiceHero[0] != null))
                    {
                        memberInfo.ChoiceHero[0].stBaseInfo.stCommonInfo.stSkill.dwSelSkillID = msg.stPkgData.stUnlockSkillSelRsp.dwSkillID;
                        Singleton<CHeroSelectSystem>.instance.RefreshHeroPanel(false);
                    }
                }
            }
        }

        [MessageHandler(0x70f)]
        public static void ReceiveBATTLELIST_NTY(CSPkg msg)
        {
        }

        [MessageHandler(0x70e)]
        public static void ReceiveBATTLELIST_RSP(CSPkg msg)
        {
            Singleton<CHeroSelectSystem>.GetInstance().SendSingleGameStartMsg();
        }

        [MessageHandler(0x4d9)]
        public static void ReceiveHeroSelect(CSPkg msg)
        {
            if (msg.stPkgData.stOperHeroRsp.bErrCode != 0)
            {
                Singleton<CHeroSelectSystem>.GetInstance().m_selectHeroIDList[0] = 0;
                Singleton<CHeroSelectSystem>.GetInstance().m_showHeroID = 0;
                Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            }
            else if (Singleton<GameDataValidator>.instance.ValidateGameData())
            {
                RefreshHeroSel(msg.stPkgData.stOperHeroRsp.stChoiceHero);
                if (Singleton<CHeroSelectSystem>.instance.isLuanDouRule())
                {
                    Singleton<CHeroSelectSystem>.instance.RefreshSkinPanel(null);
                }
                Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            }
        }

        [MessageHandler(0x472)]
        public static void ReceiveHeroSymbolPageSel(CSPkg msg)
        {
            uint dwHeroID = msg.stPkgData.stSymbolPageChgRsp.dwHeroID;
            int bPageIdx = msg.stPkgData.stSymbolPageChgRsp.bPageIdx;
            Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().SetHeroSymbolPageIdx(dwHeroID, bPageIdx);
            Singleton<CHeroSelectSystem>.GetInstance().OnSymbolPageChange();
        }

        [MessageHandler(0x4dc)]
        public static void ReciveDefaultSelectHeroes(CSPkg msg)
        {
            if (Singleton<GameDataValidator>.instance.ValidateGameData())
            {
                SCPKG_DEFAULT_HERO_NTF stDefaultHeroNtf = msg.stPkgData.stDefaultHeroNtf;
                Assets.Scripts.GameSystem.RoomInfo roomInfo = Singleton<CHeroSelectSystem>.GetInstance().m_roomInfo;
                for (int i = 0; i < stDefaultHeroNtf.bAcntNum; i++)
                {
                    COMDT_PLAYERINFO comdt_playerinfo = stDefaultHeroNtf.astDefaultHeroGrp[i];
                    MemberInfo memberInfo = roomInfo.GetMemberInfo((COM_PLAYERCAMP) comdt_playerinfo.bObjCamp, comdt_playerinfo.bPosOfCamp);
                    if (memberInfo != null)
                    {
                        memberInfo.ChoiceHero = comdt_playerinfo.astChoiceHero;
                        if (memberInfo.dwObjId == Singleton<CHeroSelectSystem>.GetInstance().m_roomInfo.selfObjID)
                        {
                            Singleton<CHeroSelectSystem>.GetInstance().m_selectHeroIDList[0] = memberInfo.ChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID;
                            Singleton<CHeroSelectSystem>.GetInstance().m_showHeroID = memberInfo.ChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID;
                        }
                    }
                }
                Singleton<CHeroSelectSystem>.GetInstance().RefreshHeroPanel(false);
                Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            }
        }

        [MessageHandler(0x42e)]
        public static void ReciveMultiChooseHeroBegin(CSPkg msg)
        {
            ReciveMultiChooseHeroBeginInner(msg);
            stUIEventParams par = new stUIEventParams();
            if (msg.stPkgData.stMultGameBeginPick.stDeskInfo.bMapType == 1)
            {
                par.heroSelectGameType = enSelectHeroType.enPVP;
            }
            else if (msg.stPkgData.stMultGameBeginPick.stDeskInfo.bMapType == 3)
            {
                par.heroSelectGameType = enSelectHeroType.enLadder;
            }
            else if (msg.stPkgData.stMultGameBeginPick.stDeskInfo.bMapType == 4)
            {
                par.heroSelectGameType = enSelectHeroType.enLuanDou;
            }
            else if (msg.stPkgData.stMultGameBeginPick.stDeskInfo.bMapType == 5)
            {
                par.heroSelectGameType = enSelectHeroType.enUnion;
            }
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.HeroSelect_OpenForm, par);
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
        }

        public static void ReciveMultiChooseHeroBeginInner(CSPkg msg)
        {
            SCPKG_MULTGAME_BEGINPICK stMultGameBeginPick = msg.stPkgData.stMultGameBeginPick;
            if (msg.stPkgData.stMultGameBeginPick.stDeskInfo.bMapType != 3)
            {
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                masterRoleInfo.SetFreeHeroInfo(stMultGameBeginPick.stFreeHero);
                masterRoleInfo.SetFreeHeroSymbol(stMultGameBeginPick.stFreeHeroSymbol);
            }
            Singleton<CRoomSystem>.GetInstance().UpdateRoomInfo(stMultGameBeginPick.stDeskInfo, stMultGameBeginPick.astCampInfo);
            Singleton<LobbyLogic>.GetInstance().inMultiRoom = true;
        }

        [MessageHandler(0x4db)]
        public static void RecivePlayerConfirm(CSPkg msg)
        {
            if (Singleton<GameDataValidator>.instance.ValidateGameData())
            {
                SCPKG_CONFIRM_HERO_NTF stConfirmHeroNtf = msg.stPkgData.stConfirmHeroNtf;
                Assets.Scripts.GameSystem.RoomInfo roomInfo = Singleton<CHeroSelectSystem>.GetInstance().m_roomInfo;
                MemberInfo memberInfo = roomInfo.GetMemberInfo(stConfirmHeroNtf.dwObjId);
                memberInfo.isPrepare = true;
                if (memberInfo.dwObjId == roomInfo.selfObjID)
                {
                    Singleton<CHeroSelectSystem>.GetInstance().m_isSelectConfirm = true;
                }
                Singleton<CHeroSelectSystem>.GetInstance().RefreshHeroPanel(false);
                Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            }
        }

        [MessageHandler(0x423)]
        public static void ReciveQuitSingleGame(CSPkg msg)
        {
            if (msg.stPkgData.stQuitSingleGameRsp.bErrCode == 0)
            {
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.HeroSelect_CloseForm);
            }
            else
            {
                Singleton<CUIManager>.GetInstance().OpenTips(Utility.ProtErrCodeToStr(0x423, msg.stPkgData.stQuitSingleGameRsp.bErrCode), false, 1f, null, new object[0]);
            }
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
        }

        [MessageHandler(0x7dc)]
        public static void ReciveSingleChooseHeroBegin(CSPkg msg)
        {
            Assets.Scripts.GameSystem.RoomInfo roomInfo = Singleton<CRoomSystem>.GetInstance().roomInfo;
            DebugHelper.Assert(roomInfo != null);
            if ((roomInfo != null) && roomInfo.roomAttrib.bWarmBattle)
            {
                CUIEvent uiEvent = new CUIEvent {
                    m_eventID = enUIEventID.Matching_OpenConfirmBox
                };
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(uiEvent);
                CFakePvPHelper.SetConfirmFakeData();
                CFakePvPHelper.StartFakeConfirm();
            }
            else
            {
                stUIEventParams par = new stUIEventParams {
                    heroSelectGameType = enSelectHeroType.enPVE_Computer
                };
                Singleton<LobbyLogic>.GetInstance().inMultiRoom = false;
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.HeroSelect_OpenForm, par);
            }
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
        }

        public void RefreshAddedSkillItem(CUIFormScript form, uint addedSkillID, bool bForceRefresh = false)
        {
            if (this.IsSelSkillAvailable())
            {
                GameObject gameObject = form.transform.Find("Other/SkillList/AddedSkillItem").gameObject;
                CUIEventScript component = gameObject.GetComponent<CUIEventScript>();
                if (!this.IsSelSkillAvailable(this.m_gameType, addedSkillID))
                {
                    addedSkillID = GameDataMgr.addedSkiilDatabin.GetAnyData().dwUnlockSkillID;
                }
                if (!this.IsSelSkillAvailable(this.m_gameType, addedSkillID))
                {
                    addedSkillID = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x9a).dwConfValue;
                }
                ResSkillCfgInfo dataByKey = GameDataMgr.skillDatabin.GetDataByKey(addedSkillID);
                if (dataByKey != null)
                {
                    string prefabPath = string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Skill_Dir, Utility.UTF8Convert(dataByKey.szIconPath));
                    gameObject.transform.Find("Icon").GetComponent<Image>().SetSprite(prefabPath, form, true, false, false);
                    gameObject.transform.Find("SkillNameTxt").GetComponent<Text>().text = Utility.UTF8Convert(dataByKey.szSkillName);
                    string pvpHeroSkillDesc = CUICommonSystem.GetPvpHeroSkillDesc((int) addedSkillID, this.m_showHeroID);
                    stUIEventParams eventParams = new stUIEventParams {
                        skillTipParam = new stSkillTipParams()
                    };
                    eventParams.skillTipParam.strTipText = pvpHeroSkillDesc;
                    eventParams.skillTipParam.strTipTitle = StringHelper.UTF8BytesToString(ref dataByKey.szSkillName);
                    string[] args = new string[] { ((int) (dataByKey.iCoolDown / 0x3e8)).ToString() };
                    eventParams.skillTipParam.skillCoolDown = Singleton<CTextManager>.instance.GetText("Skill_Cool_Down_Tips", args);
                    eventParams.skillTipParam.skillEffect = dataByKey.SkillEffectType;
                    eventParams.skillTipParam.skillEnergyCost = (dataByKey.iEnergyCost != 0) ? Singleton<CTextManager>.instance.GetText("Skill_Energy_Cost_Tips", new string[] { ((int) dataByKey.iEnergyCost).ToString() }) : string.Empty;
                    component.SetUIEvent(enUIEventType.Down, enUIEventID.HeroSelect_Skill_Down, eventParams);
                    if (bForceRefresh)
                    {
                        form.transform.Find("PanelAddSkill/AddSkilltxt").GetComponent<Text>().text = pvpHeroSkillDesc;
                        form.transform.Find("PanelAddSkill/btnConfirm").GetComponent<CUIEventScript>().m_onClickEventParams.tag = (int) addedSkillID;
                        ListView<ResSkillUnlock> selSkillAvailable = this.GetSelSkillAvailable();
                        for (int i = 0; i < selSkillAvailable.Count; i++)
                        {
                            if (selSkillAvailable[i].dwUnlockSkillID == addedSkillID)
                            {
                                form.transform.Find("PanelAddSkill/ToggleList").GetComponent<CUIToggleListScript>().SelectElement(i, true);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    DebugHelper.Assert(false, string.Format("ResSkillCfgInfo[{0}] can not be found!", addedSkillID));
                }
            }
        }

        private void RefreshHeroInfo_CenterPanel(CUIFormScript form, CRoleInfo roleInfo, CUIListScript skillList)
        {
            CUI3DImageScript component = form.transform.Find("PanelCenter/3DImage").gameObject.GetComponent<CUI3DImageScript>();
            Text text = form.transform.Find("Other/HeroInfo/HeroName/lblName").gameObject.GetComponent<Text>();
            Image image = form.transform.Find("Other/HeroInfo/HeroName/imgJob").gameObject.GetComponent<Image>();
            Text text2 = form.transform.Find("Other/HeroInfo/HeroJob/jobTitleText").gameObject.GetComponent<Text>();
            Text text3 = form.transform.Find("Other/HeroInfo/HeroJob/jobFeatureText").gameObject.GetComponent<Text>();
            if (this.m_showHeroID == 0)
            {
                text.gameObject.CustomSetActive(false);
                image.gameObject.CustomSetActive(false);
                text2.gameObject.CustomSetActive(false);
                text3.gameObject.CustomSetActive(false);
            }
            if (this.m_nowShowHeroID != this.m_showHeroID)
            {
                component.RemoveGameObject(this.m_heroGameObjName);
                this.m_nowShowHeroID = this.m_showHeroID;
                if (this.m_nowShowHeroID != 0)
                {
                    int heroWearSkinId = (int) roleInfo.GetHeroWearSkinId(this.m_nowShowHeroID);
                    ObjNameData data = CUICommonSystem.GetHeroPrefabPath(this.m_nowShowHeroID, heroWearSkinId, true);
                    this.m_heroGameObjName = data.ObjectName;
                    GameObject model = component.AddGameObject(this.m_heroGameObjName, false, false);
                    if (model != null)
                    {
                        model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                        if (data.ActorInfo != null)
                        {
                            model.transform.localScale = new Vector3(data.ActorInfo.LobbyScale, data.ActorInfo.LobbyScale, data.ActorInfo.LobbyScale);
                        }
                        CHeroAnimaSystem instance = Singleton<CHeroAnimaSystem>.GetInstance();
                        instance.Set3DModel(model);
                        instance.InitAnimatList();
                        instance.InitAnimatSoundList(this.m_nowShowHeroID, (uint) heroWearSkinId);
                        instance.OnModePlayAnima("Come");
                    }
                    IHeroData data2 = CHeroDataFactory.CreateHeroData(this.m_nowShowHeroID);
                    if (data2 != null)
                    {
                        ResDT_SkillInfo[] skillArr = data2.skillArr;
                        skillList.SetElementAmount(skillArr.Length - 1);
                        for (int i = 0; i < (skillArr.Length - 1); i++)
                        {
                            GameObject gameObject = skillList.GetElemenet(i).gameObject.transform.Find("heroSkillItemCell").gameObject;
                            ResSkillCfgInfo skillCfgInfo = CSkillData.GetSkillCfgInfo(skillArr[i].iSkillID);
                            CUIEventScript script2 = gameObject.GetComponent<CUIEventScript>();
                            if (skillCfgInfo == null)
                            {
                                return;
                            }
                            if (i == 0)
                            {
                                gameObject.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
                            }
                            else
                            {
                                gameObject.transform.localScale = Vector3.one;
                            }
                            GameObject obj4 = gameObject.transform.Find("skillMask/skillIcon").gameObject;
                            CUIUtility.SetImageSprite(obj4.GetComponent<Image>(), CUIUtility.s_Sprite_Dynamic_Skill_Dir + StringHelper.UTF8BytesToString(ref skillCfgInfo.szIconPath), form, true, false, false);
                            obj4.CustomSetActive(true);
                            stUIEventParams eventParams = new stUIEventParams();
                            stSkillTipParams params2 = new stSkillTipParams();
                            eventParams.skillTipParam = params2;
                            if (((this.m_gameType == enSelectHeroType.enPVP) || (this.m_gameType == enSelectHeroType.enLadder)) || ((this.m_gameType == enSelectHeroType.enLuanDou) || (this.m_gameType == enSelectHeroType.enUnion)))
                            {
                                eventParams.skillTipParam.strTipText = CUICommonSystem.GetSkillDescLobby(skillCfgInfo.szSkillDesc, this.m_nowShowHeroID);
                                eventParams.skillTipParam.strTipTitle = StringHelper.UTF8BytesToString(ref skillCfgInfo.szSkillName);
                            }
                            else if (data2 is CHeroInfoData)
                            {
                                eventParams.skillTipParam.strTipText = ((CHeroInfoData) data2).m_info.skillInfo.GetSkillDesc(i);
                                eventParams.skillTipParam.strTipTitle = StringHelper.UTF8BytesToString(ref skillCfgInfo.szSkillName);
                            }
                            else
                            {
                                eventParams.skillTipParam.strTipText = CUICommonSystem.GetSkillDescLobby(skillCfgInfo.szSkillDesc, this.m_nowShowHeroID);
                                eventParams.skillTipParam.strTipTitle = StringHelper.UTF8BytesToString(ref skillCfgInfo.szSkillName);
                            }
                            eventParams.skillTipParam.skillCoolDown = (i != 0) ? Singleton<CTextManager>.instance.GetText("Skill_Cool_Down_Tips", new string[1]) : Singleton<CTextManager>.instance.GetText("Skill_Common_Effect_Type_5");
                            eventParams.skillTipParam.skillEnergyCost = (i != 0) ? Singleton<CTextManager>.instance.GetText("Skill_Energy_Cost_Tips", new string[] { ((int) skillCfgInfo.iEnergyCost).ToString() }) : string.Empty;
                            eventParams.skillTipParam.skillEffect = skillCfgInfo.SkillEffectType;
                            script2.SetUIEvent(enUIEventType.Down, enUIEventID.HeroSelect_Skill_Down, eventParams);
                        }
                        text.text = data2.heroName;
                        CUICommonSystem.SetHeroJob(form, image.gameObject, (enHeroJobType) data2.heroType);
                        text.gameObject.CustomSetActive(true);
                        image.gameObject.CustomSetActive(true);
                        text2.gameObject.CustomSetActive(true);
                        text3.gameObject.CustomSetActive(true);
                        text3.text = CHeroInfo.GetJobFeature(this.m_nowShowHeroID);
                    }
                }
            }
        }

        private void RefreshHeroInfo_ConfirmButtonPanel(CUIFormScript form, CRoleInfo roleInfo)
        {
            Button component = form.transform.Find("PanelRight/btnConfirm").gameObject.GetComponent<Button>();
            bool isSelectConfirm = true;
            if (this.m_heroInfoShowType == enHeroInfoShowType.enPVP)
            {
                isSelectConfirm = this.m_isSelectConfirm;
                MemberInfo masterMemberInfo = this.m_roomInfo.GetMasterMemberInfo();
                if (masterMemberInfo == null)
                {
                    return;
                }
                if (masterMemberInfo.ChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID == 0)
                {
                    isSelectConfirm = true;
                }
            }
            else if (this.m_selectHeroCount == 0)
            {
                isSelectConfirm = true;
            }
            else if (!this.m_isSelectConfirm)
            {
                isSelectConfirm = false;
            }
            if (isSelectConfirm)
            {
                CUICommonSystem.SetButtonEnableWithShader(component.GetComponent<Button>(), false, true);
            }
            else
            {
                CUICommonSystem.SetButtonEnableWithShader(component.GetComponent<Button>(), true, true);
            }
        }

        public void RefreshHeroInfo_DropList()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                Transform transform = form.gameObject.transform.Find("Other/Panel_SymbolChange");
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                if (Singleton<CFunctionUnlockSys>.GetInstance().FucIsUnlock(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_SYMBOL))
                {
                    CHeroInfo info2;
                    if (masterRoleInfo.GetHeroInfo(this.m_showHeroID, out info2, true))
                    {
                        transform.gameObject.CustomSetActive(true);
                        CSymbolSystem.SetPageDropListDataByHeroSelect(transform.gameObject, info2.m_selectPageIndex);
                    }
                    else if (masterRoleInfo.IsFreeHero(this.m_showHeroID))
                    {
                        transform.gameObject.CustomSetActive(true);
                        int freeHeroSymbolId = masterRoleInfo.GetFreeHeroSymbolId(this.m_showHeroID);
                        CSymbolSystem.SetPageDropListDataByHeroSelect(transform.gameObject, freeHeroSymbolId);
                    }
                    else
                    {
                        transform.gameObject.CustomSetActive(false);
                    }
                }
                else
                {
                    transform.gameObject.CustomSetActive(false);
                }
            }
        }

        private void RefreshHeroInfo_ExperiencePanel(CUIFormScript form)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            GameObject widget = form.GetWidget(0);
            GameObject obj3 = form.GetWidget(1);
            GameObject obj4 = form.GetWidget(2);
            GameObject obj5 = form.GetWidget(3);
            widget.CustomSetActive(false);
            obj3.CustomSetActive(false);
            obj4.CustomSetActive(false);
            obj5.CustomSetActive(false);
            if (masterRoleInfo.IsValidExperienceHero(this.m_showHeroID))
            {
                CUICommonSystem.RefreshExperienceHeroLeftTime(widget, obj4, this.m_showHeroID);
            }
            uint heroWearSkinId = masterRoleInfo.GetHeroWearSkinId(this.m_showHeroID);
            if (masterRoleInfo.IsValidExperienceSkin(this.m_showHeroID, heroWearSkinId))
            {
                CUICommonSystem.RefreshExperienceSkinLeftTime(obj3, obj5, this.m_showHeroID, heroWearSkinId, null);
            }
        }

        private void RefreshHeroInfo_LeftPanel(CUIFormScript form, CRoleInfo roleInfo)
        {
            CUIListScript component = form.transform.Find("PanelLeft/ListHostHeroInfo").gameObject.GetComponent<CUIListScript>();
            CUIListScript script2 = form.transform.Find("PanelLeft/ListHostHeroInfoFull").gameObject.GetComponent<CUIListScript>();
            CUIListScript[] scriptArray = new CUIListScript[] { component, script2 };
            component.m_alwaysDispatchSelectedChangeEvent = true;
            script2.m_alwaysDispatchSelectedChangeEvent = true;
            for (int i = 0; i < scriptArray.Length; i++)
            {
                scriptArray[i].SetElementAmount(this.m_hostHeroList.Count);
            }
        }

        private void RefreshHeroInfo_RightPanel(CUIFormScript form, CRoleInfo roleInfo)
        {
            <RefreshHeroInfo_RightPanel>c__AnonStorey42 storey = new <RefreshHeroInfo_RightPanel>c__AnonStorey42 {
                form = form
            };
            CUIListScript component = storey.form.transform.Find("PanelRight/ListTeamHeroInfo").gameObject.GetComponent<CUIListScript>();
            component.m_alwaysDispatchSelectedChangeEvent = true;
            List<uint> teamHeroList = this.GetTeamHeroList();
            component.SetElementAmount(teamHeroList.Count);
            int num = 0;
            for (int i = 0; i < teamHeroList.Count; i++)
            {
                <RefreshHeroInfo_RightPanel>c__AnonStorey41 storey2 = new <RefreshHeroInfo_RightPanel>c__AnonStorey41();
                GameObject gameObject = component.GetElemenet(i).gameObject;
                GameObject item = gameObject.transform.Find("heroItemCell").gameObject;
                GameObject obj4 = item.transform.Find("ItemBg1").gameObject;
                GameObject obj5 = item.transform.Find("ItemBg2").gameObject;
                GameObject obj6 = item.transform.Find("redReadyIcon").gameObject;
                GameObject obj7 = item.transform.Find("redReadyIcon").gameObject;
                GameObject obj8 = item.transform.Find("selfIcon").gameObject;
                GameObject obj9 = item.transform.Find("delBtn").gameObject;
                storey2.selSkillCell = gameObject.transform.Find("selSkillItemCell").gameObject;
                Image image = item.transform.Find("imageIcon").gameObject.GetComponent<Image>();
                Text text = item.transform.Find("lblName").gameObject.GetComponent<Text>();
                CUIEventScript script2 = item.GetComponent<CUIEventScript>();
                uint id = teamHeroList[i];
                text.text = string.Empty;
                obj4.CustomSetActive(false);
                obj5.CustomSetActive(false);
                obj6.CustomSetActive(false);
                obj7.CustomSetActive(false);
                obj8.CustomSetActive(false);
                obj9.CustomSetActive(false);
                storey2.selSkillCell.CustomSetActive(false);
                script2.enabled = false;
                if (this.m_heroInfoShowType == enHeroInfoShowType.enPVP)
                {
                    <RefreshHeroInfo_RightPanel>c__AnonStorey40 storey3 = new <RefreshHeroInfo_RightPanel>c__AnonStorey40 {
                        <>f__ref$66 = storey,
                        <>f__ref$65 = storey2
                    };
                    MemberInfo masterMemberInfo = this.m_roomInfo.GetMasterMemberInfo();
                    MemberInfo info2 = this.m_roomInfo.GetCampMemberList(masterMemberInfo.camp)[i];
                    text.text = info2.MemberName;
                    storey3.selSkillID = info2.ChoiceHero[0].stBaseInfo.stCommonInfo.stSkill.dwSelSkillID;
                    if ((((this.m_gameType == enSelectHeroType.enPVP) || (this.m_gameType == enSelectHeroType.enLadder)) || (((this.m_gameType == enSelectHeroType.enLuanDou) || (this.m_gameType == enSelectHeroType.enUnion)) || ((this.m_roomInfo != null) && this.m_roomInfo.roomAttrib.bWarmBattle))) && (storey3.selSkillID != 0))
                    {
                        GameDataMgr.addedSkiilDatabin.Accept(new Action<ResSkillUnlock>(storey3.<>m__36));
                    }
                    if ((info2.dwObjId == this.m_roomInfo.selfObjID) && (info2.RoomMemberType != 2))
                    {
                        obj4.CustomSetActive(true);
                        if (info2.isPrepare)
                        {
                            obj6.CustomSetActive(true);
                        }
                        string[] args = new string[] { info2.MemberName };
                        text.text = Singleton<CTextManager>.instance.GetText("Pvp_PlayerName", args);
                    }
                    else
                    {
                        obj5.CustomSetActive(true);
                        if (info2.isPrepare)
                        {
                            obj7.CustomSetActive(true);
                        }
                    }
                }
                else
                {
                    if (id != 0)
                    {
                        IHeroData data = CHeroDataFactory.CreateHeroData(id);
                        if (data == null)
                        {
                            return;
                        }
                        text.text = data.heroName;
                        num += data.combatEft;
                    }
                    obj4.CustomSetActive(true);
                    if (i == 0)
                    {
                        obj8.CustomSetActive(true);
                    }
                    if (i < this.m_selectHeroCount)
                    {
                        obj9.CustomSetActive(true);
                    }
                }
                if (id != 0)
                {
                    IHeroData data2 = CHeroDataFactory.CreateHeroData(id);
                    if (data2 == null)
                    {
                        return;
                    }
                    CUICommonSystem.SetHeroItemData(storey.form, item, data2, enHeroHeadType.enIcon, false, this.m_heroInfoShowType);
                }
                else if (this.m_heroInfoShowType != enHeroInfoShowType.enPVP)
                {
                    image.SetSprite(CUIUtility.s_Sprite_System_HeroSelect_Dir + "HeroChoose_unknownIcon", storey.form, true, false, false);
                }
                else
                {
                    MemberInfo info3 = this.m_roomInfo.GetMasterMemberInfo();
                    MemberInfo info4 = this.m_roomInfo.GetCampMemberList(info3.camp)[i];
                    if ((info4.RoomMemberType == 2) && !this.m_roomInfo.roomAttrib.bWarmBattle)
                    {
                        image.SetSprite(CUIUtility.s_Sprite_System_HeroSelect_Dir + "Img_ComputerHead", storey.form, true, false, false);
                    }
                    else
                    {
                        image.SetSprite(CUIUtility.s_Sprite_System_HeroSelect_Dir + "HeroChoose_unknownIcon", storey.form, true, false, false);
                    }
                }
                if (this.m_heroInfoShowType == enHeroInfoShowType.enPVP)
                {
                    if (this.m_isSelectConfirm && (id != 0))
                    {
                        script2.enabled = true;
                    }
                }
                else if (id != 0)
                {
                    script2.enabled = true;
                }
            }
        }

        private void RefreshHeroInfo_SpecSkillPanel(CUIListScript skillList, CRoleInfo roleInfo, bool bForceRefreshAddSkillPanel, CUIFormScript form)
        {
            skillList.gameObject.CustomSetActive(false);
            if (this.m_nowShowHeroID != 0)
            {
                skillList.gameObject.CustomSetActive(true);
                if (this.IsSelSkillAvailable())
                {
                    if (((this.m_gameType == enSelectHeroType.enPVP) || (this.m_gameType == enSelectHeroType.enLadder)) || ((this.m_gameType == enSelectHeroType.enLuanDou) || (this.m_gameType == enSelectHeroType.enUnion)))
                    {
                        MemberInfo masterMemberInfo = this.m_roomInfo.GetMasterMemberInfo();
                        if (masterMemberInfo != null)
                        {
                            this.RefreshAddedSkillItem(form, masterMemberInfo.ChoiceHero[0].stBaseInfo.stCommonInfo.stSkill.dwSelSkillID, bForceRefreshAddSkillPanel);
                        }
                    }
                    else if (this.m_gameType == enSelectHeroType.enPVE_Computer)
                    {
                        CHeroInfo heroInfo = roleInfo.GetHeroInfo(this.m_nowShowHeroID, true);
                        if (heroInfo != null)
                        {
                            this.RefreshAddedSkillItem(form, heroInfo.skillInfo.SelSkillID, bForceRefreshAddSkillPanel);
                        }
                        else if (roleInfo.IsFreeHero(this.m_nowShowHeroID))
                        {
                            COMDT_FREEHERO_INFO freeHeroSymbol = roleInfo.GetFreeHeroSymbol(this.m_nowShowHeroID);
                            if (freeHeroSymbol != null)
                            {
                                this.RefreshAddedSkillItem(form, freeHeroSymbol.dwSkillID, bForceRefreshAddSkillPanel);
                            }
                            else
                            {
                                this.RefreshAddedSkillItem(form, 0, bForceRefreshAddSkillPanel);
                            }
                        }
                    }
                }
            }
        }

        public void RefreshHeroPanel(bool bForceRefreshAddSkillPanel = false)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if ((form != null) && (masterRoleInfo != null))
            {
                CUIListScript component = form.transform.Find("Other/SkillList").gameObject.GetComponent<CUIListScript>();
                CUITimerScript script3 = form.transform.Find("CountDown/Timer").gameObject.GetComponent<CUITimerScript>();
                Button button = form.transform.Find("btnClose").gameObject.GetComponent<Button>();
                if (this.m_selectHeroCount <= 0)
                {
                    CUICommonSystem.PlayAnimator(form.gameObject, "show");
                }
                else
                {
                    CUICommonSystem.PlayAnimator(form.gameObject, "hide");
                }
                button.gameObject.CustomSetActive(false);
                script3.gameObject.CustomSetActive(false);
                if ((((this.m_gameType == enSelectHeroType.enPVP) || (this.m_gameType == enSelectHeroType.enLadder)) || ((this.m_gameType == enSelectHeroType.enLuanDou) || (this.m_gameType == enSelectHeroType.enUnion))) || (((this.m_gameType == enSelectHeroType.enPVE_Computer) && (this.m_roomInfo != null)) && this.m_roomInfo.roomAttrib.bWarmBattle))
                {
                    if (!script3.gameObject.activeInHierarchy)
                    {
                        script3.SetTotalTime(60f);
                        script3.m_timerType = Assets.Scripts.UI.enTimerType.CountDown;
                        script3.StartTimer();
                        script3.gameObject.CustomSetActive(true);
                    }
                }
                else
                {
                    button.gameObject.CustomSetActive(true);
                }
                this.RefreshHeroInfo_LeftPanel(form, masterRoleInfo);
                this.RefreshHeroInfo_RightPanel(form, masterRoleInfo);
                this.RefreshHeroInfo_CenterPanel(form, masterRoleInfo, component);
                this.RefreshHeroInfo_SpecSkillPanel(component, masterRoleInfo, bForceRefreshAddSkillPanel, form);
                this.RefreshHeroInfo_DropList();
                this.RefreshHeroInfo_ExperiencePanel(form);
                this.RefreshHeroInfo_ConfirmButtonPanel(form, masterRoleInfo);
            }
        }

        public static void RefreshHeroSel(MemberInfo memberInfo)
        {
            if (memberInfo != null)
            {
                if (memberInfo.dwObjId == Singleton<CHeroSelectSystem>.GetInstance().m_roomInfo.selfObjID)
                {
                    Singleton<CHeroSelectSystem>.GetInstance().m_selectHeroIDList[0] = memberInfo.ChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID;
                    Singleton<CHeroSelectSystem>.GetInstance().m_selectHeroCount = 1;
                    Singleton<CHeroSelectSystem>.GetInstance().m_showHeroID = memberInfo.ChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID;
                }
                Singleton<CHeroSelectSystem>.GetInstance().RefreshHeroPanel(false);
            }
        }

        public static void RefreshHeroSel(COMDT_PLAYERINFO inPlayerInfo)
        {
            if (inPlayerInfo != null)
            {
                MemberInfo memberInfo = Singleton<CHeroSelectSystem>.GetInstance().m_roomInfo.GetMemberInfo(inPlayerInfo.dwObjId);
                memberInfo.ChoiceHero = inPlayerInfo.astChoiceHero;
                RefreshHeroSel(memberInfo);
            }
        }

        private void RefreshLeftRandCountText()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                Transform transform = form.transform.FindChild("Other/RandomHero");
                if (this.isLuanDouRule())
                {
                    transform.gameObject.CustomSetActive(true);
                    ResShopInfo cfgShopInfo = CPurchaseSys.GetCfgShopInfo(RES_SHOPBUY_TYPE.RES_BUYTYPE_ENTERTAINMENTRANDHERO, this.m_UseRandSelCount + 1);
                    if (cfgShopInfo != null)
                    {
                        stPayInfo info2 = new stPayInfo {
                            m_payType = CMallSystem.ResBuyTypeToPayType(cfgShopInfo.bCoinType),
                            m_payValue = cfgShopInfo.dwCoinPrice
                        };
                        stUIEventParams eventParams = new stUIEventParams();
                        CMallSystem.SetPayButton(form, transform.transform as RectTransform, info2.m_payType, info2.m_payValue, enUIEventID.HeroSelect_RandomHero, ref eventParams);
                    }
                    else
                    {
                        transform.gameObject.CustomSetActive(false);
                    }
                }
                else
                {
                    transform.gameObject.CustomSetActive(false);
                }
            }
        }

        public void RefreshSkinPanel(CUIEvent uiEvent = null)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            uint showHeroID = this.m_showHeroID;
            ListView<ResHeroSkin> view = new ListView<ResHeroSkin>();
            ListView<ResHeroSkin> collection = new ListView<ResHeroSkin>();
            int index = -1;
            ResHeroSkin item = null;
            if (showHeroID != 0)
            {
                ListView<ResHeroSkin> view3 = CSkinInfo.s_heroSkinDic[showHeroID];
                for (int i = 0; i < view3.Count; i++)
                {
                    item = view3[i];
                    if (masterRoleInfo.IsCanUseSkin(showHeroID, item.dwSkinID))
                    {
                        view.Add(item);
                    }
                    else
                    {
                        collection.Add(item);
                    }
                    if (masterRoleInfo.GetHeroWearSkinId(showHeroID) == item.dwSkinID)
                    {
                        index = view.Count - 1;
                    }
                }
                view.AddRange(collection);
            }
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (form != null)
            {
                Transform transform = form.gameObject.transform.Find("PanelLeftSkin/ListHostSkinInfo");
                Transform transform2 = form.gameObject.transform.Find("PanelLeftSkin/ListHostSkinInfoFull");
                Transform transform3 = form.gameObject.transform.Find("PanelLeftSkin/ListHostSkinInfo/panelEffect");
                if ((transform != null) && (transform2 != null))
                {
                    CUIListScript[] scriptArray1 = new CUIListScript[] { transform.GetComponent<CUIListScript>() };
                    foreach (CUIListScript script2 in scriptArray1)
                    {
                        script2.SetElementAmount(view.Count);
                        for (int j = 0; j < view.Count; j++)
                        {
                            CUIListElementScript elemenet = script2.GetElemenet(j);
                            Transform transform4 = script2.GetElemenet(j).transform;
                            Image component = transform4.Find("imageIcon").GetComponent<Image>();
                            Image image = transform4.Find("imageIconGray").GetComponent<Image>();
                            Text text = transform4.Find("lblName").GetComponent<Text>();
                            GameObject gameObject = transform4.Find("imgExperienceMark").gameObject;
                            item = view[j];
                            bool bActive = masterRoleInfo.IsValidExperienceSkin(showHeroID, item.dwSkinID);
                            gameObject.CustomSetActive(bActive);
                            if (masterRoleInfo.IsCanUseSkin(showHeroID, item.dwSkinID))
                            {
                                component.gameObject.CustomSetActive(true);
                                image.gameObject.CustomSetActive(false);
                                elemenet.enabled = true;
                            }
                            else
                            {
                                component.gameObject.CustomSetActive(false);
                                image.gameObject.CustomSetActive(true);
                                elemenet.enabled = false;
                            }
                            GameObject prefab = CUIUtility.GetSpritePrefeb(CUIUtility.s_Sprite_Dynamic_Icon_Dir + StringHelper.UTF8BytesToString(ref item.szSkinPicID), true, true);
                            component.SetSprite(prefab);
                            image.SetSprite(prefab);
                            text.text = StringHelper.UTF8BytesToString(ref item.szSkinName);
                            CUIEventScript script4 = component.gameObject.GetComponent<CUIEventScript>();
                            CUIEventScript script5 = image.gameObject.GetComponent<CUIEventScript>();
                            stUIEventParams eventParams = new stUIEventParams {
                                tagUInt = item.dwSkinID,
                                commonBool = bActive
                            };
                            script4.SetUIEvent(enUIEventType.Click, enUIEventID.HeroSelect_SkinSelect, eventParams);
                            script5.SetUIEvent(enUIEventType.Click, enUIEventID.HeroSelect_SkinSelect, eventParams);
                            if (j == index)
                            {
                                this.InitSkinEffect(script2.transform.Find("panelEffect/List").gameObject, showHeroID, item.dwSkinID);
                            }
                        }
                        script2.SelectElement(index, true);
                    }
                    if (index == -1)
                    {
                        transform3.gameObject.CustomSetActive(false);
                    }
                    else
                    {
                        transform3.gameObject.CustomSetActive(true);
                    }
                }
            }
        }

        private void ResetBaseProp()
        {
            this.m_gameType = enSelectHeroType.enNull;
            this.m_selectHeroCount = 0;
            this.m_showHeroID = 0;
            this.m_nowShowHeroID = 0;
            this.m_isSelectConfirm = false;
            this.m_UseRandSelCount = 0;
        }

        public void ResetHero3DObj()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroSelectFormPath);
            if (((form != null) && (form.gameObject != null)) && (this.m_nowShowHeroID != 0))
            {
                GameObject gameObject = form.gameObject.transform.Find("PanelCenter/3DImage").gameObject.GetComponent<CUI3DImageScript>().GetGameObject(this.m_heroGameObjName);
                if (gameObject != null)
                {
                    CHeroAnimaSystem instance = Singleton<CHeroAnimaSystem>.GetInstance();
                    instance.Set3DModel(gameObject);
                    instance.InitAnimatList();
                    uint heroWearSkinId = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().GetHeroWearSkinId(this.m_nowShowHeroID);
                    instance.InitAnimatSoundList(this.m_nowShowHeroID, heroWearSkinId);
                }
            }
        }

        public void ResetRandHeroLeftCount(int inLeftCount)
        {
            this.m_UseRandSelCount = inLeftCount;
        }

        public void SavePveDefaultHeroList()
        {
            if (this.m_defaultBattleListInfo != null)
            {
                bool flag = false;
                for (int i = 0; i < this.m_defaultBattleListInfo.dwListNum; i++)
                {
                    if (this.m_defaultBattleListInfo.astBattleList[i].dwBattleListID == this.m_battleListID)
                    {
                        COMDT_BATTLEHERO stBattleList = this.m_defaultBattleListInfo.astBattleList[i].stBattleList;
                        stBattleList.wHeroCnt = (ushort) this.m_selectHeroIDList.Count;
                        stBattleList.BattleHeroList = this.m_selectHeroIDList.ToArray();
                        flag = true;
                    }
                }
                if (!flag)
                {
                    ListLinqView<COMDT_BATTLELIST> view = new ListLinqView<COMDT_BATTLELIST>();
                    for (int j = 0; j < this.m_defaultBattleListInfo.dwListNum; j++)
                    {
                        view.Add(this.m_defaultBattleListInfo.astBattleList[j]);
                    }
                    COMDT_BATTLELIST item = new COMDT_BATTLELIST {
                        dwBattleListID = this.m_battleListID,
                        stBattleList = new COMDT_BATTLEHERO()
                    };
                    item.stBattleList.wHeroCnt = (ushort) this.m_selectHeroIDList.Count;
                    item.stBattleList.BattleHeroList = this.m_selectHeroIDList.ToArray();
                    view.Add(item);
                    this.m_defaultBattleListInfo.dwListNum = (uint) view.Count;
                    this.m_defaultBattleListInfo.astBattleList = view.ToArray();
                }
            }
        }

        public static void SendHeroSelectMsg(byte operType, byte operPos, uint heroID)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x4d8);
            msg.stPkgData.stOperHeroReq.bOperType = operType;
            msg.stPkgData.stOperHeroReq.stOperDetail = new CSDT_OPER_HERO();
            if (operType == 0)
            {
                msg.stPkgData.stOperHeroReq.stOperDetail.stSetHero = new CSDT_SETHERO();
                msg.stPkgData.stOperHeroReq.stOperDetail.stSetHero.bHeroPos = operPos;
                msg.stPkgData.stOperHeroReq.stOperDetail.stSetHero.dwHeroId = heroID;
            }
            Singleton<NetworkModule>.GetInstance().SendGameMsg(ref msg, 0);
            Singleton<CUIManager>.GetInstance().OpenSendMsgAlert(5, enUIEventID.None);
        }

        public static void SendHeroSelectSymbolPage(uint heroId, int selIndex, bool bSendGame = false)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x471);
            msg.stPkgData.stSymbolPageChgReq = new CSPKG_CMD_SYMBOLPAGESEL();
            msg.stPkgData.stSymbolPageChgReq.dwHeroID = heroId;
            msg.stPkgData.stSymbolPageChgReq.bPageIdx = (byte) selIndex;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
        }

        public static void SendMuliPrepareToBattleMsg()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x4da);
            Singleton<NetworkModule>.GetInstance().SendGameMsg(ref msg, 0);
            Singleton<CUIManager>.GetInstance().OpenSendMsgAlert(5, enUIEventID.None);
        }

        public static void SendQuitSingleGameReq()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x422);
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public void SendSingleGameStartMsg()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x41a);
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            msg.stPkgData.stStartSingleGameReq.stBattleParam.bGameType = (byte) this.m_gameType;
            msg.stPkgData.stStartSingleGameReq.stBattleList.dwBattleListID = this.m_battleListID;
            if (this.m_gameType == enSelectHeroType.enPVE_Adventure)
            {
                msg.stPkgData.stStartSingleGameReq.stBattleParam.stGameDetail.stGameOfAdventure = Singleton<CHeroSelectSystem>.GetInstance().m_stGameOfAdventure;
                msg.stPkgData.stStartSingleGameReq.stBattleList.stBattleList.wHeroCnt = this.m_selectHeroCount;
                msg.stPkgData.stStartSingleGameReq.stBattleList.stBattleList.BattleHeroList = this.m_selectHeroIDList.ToArray();
                masterRoleInfo.battleHeroList = this.m_selectHeroIDList;
                this.SavePveDefaultHeroList();
                this.PostAdventureSingleGame(msg.stPkgData.stStartSingleGameReq.stBattlePlayer);
            }
            else if (this.m_gameType == enSelectHeroType.enPVE_Computer)
            {
                msg.stPkgData.stStartSingleGameReq.stBattleParam.stGameDetail.stGameOfCombat = Singleton<CHeroSelectSystem>.GetInstance().m_stGameOfCombat;
                msg.stPkgData.stStartSingleGameReq.stBattleList.stBattleList.wHeroCnt = 1;
                MemberInfo masterMemberInfo = this.m_roomInfo.GetMasterMemberInfo();
                msg.stPkgData.stStartSingleGameReq.stBattleList.stBattleList.BattleHeroList[0] = masterMemberInfo.ChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID;
                this.PostCombatSingleGame(msg.stPkgData.stStartSingleGameReq.stBattlePlayer);
            }
            else if (this.m_gameType == enSelectHeroType.enBurning)
            {
                msg.stPkgData.stStartSingleGameReq.stBattleParam.stGameDetail.stGameOfBurning = Singleton<CHeroSelectSystem>.GetInstance().m_stGameOfBurnning;
                msg.stPkgData.stStartSingleGameReq.stBattleList.stBattleList.wHeroCnt = this.m_selectHeroCount;
                msg.stPkgData.stStartSingleGameReq.stBattleList.stBattleList.BattleHeroList = this.m_selectHeroIDList.ToArray();
                this.SavePveDefaultHeroList();
                this.PostBurningSingleGame(msg.stPkgData.stStartSingleGameReq.stBattlePlayer);
            }
            else if (this.m_gameType == enSelectHeroType.enArena)
            {
                msg.stPkgData.stStartSingleGameReq.stBattleParam.stGameDetail.stGameOfArena = Singleton<CHeroSelectSystem>.GetInstance().m_stGameOfArena;
                msg.stPkgData.stStartSingleGameReq.stBattleList.stBattleList.wHeroCnt = this.m_selectHeroCount;
                msg.stPkgData.stStartSingleGameReq.stBattleList.stBattleList.BattleHeroList = this.m_selectHeroIDList.ToArray();
                this.SavePveDefaultHeroList();
                this.PostArenaSingleGame(msg.stPkgData.stStartSingleGameReq.stBattlePlayer);
            }
            else if (this.m_gameType == enSelectHeroType.enGuide)
            {
                msg.stPkgData.stStartSingleGameReq.stBattleList.stBattleList.wHeroCnt = this.m_selectHeroCount;
                msg.stPkgData.stStartSingleGameReq.stBattleList.stBattleList.BattleHeroList = this.m_selectHeroIDList.ToArray();
                msg.stPkgData.stStartSingleGameReq.stBattleParam.stGameDetail.construct(2L);
                msg.stPkgData.stStartSingleGameReq.stBattleParam.stGameDetail.stGameOfGuide.iLevelID = Singleton<CHeroSelectSystem>.GetInstance().m_stGameOfAdventure.iLevelID;
                masterRoleInfo.battleHeroList = this.m_selectHeroIDList;
                this.PostAdventureSingleGame(msg.stPkgData.stStartSingleGameReq.stBattlePlayer);
            }
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void SendSingleGameStartMsgSkipHeroSelect(int iLevelID, int iDifficult)
        {
            ResLevelCfgInfo dataByKey = GameDataMgr.levelDatabin.GetDataByKey(iLevelID);
            DebugHelper.Assert(dataByKey != null);
            CSDT_SINGLE_GAME_OF_ADVENTURE reportInfo = new CSDT_SINGLE_GAME_OF_ADVENTURE {
                iLevelID = iLevelID,
                bChapterNo = (byte) dataByKey.iChapterId,
                bLevelNo = dataByKey.bLevelNo,
                bDifficultType = (byte) iDifficult
            };
            byte iHeroNum = (byte) dataByKey.iHeroNum;
            uint dwBattleListID = dataByKey.dwBattleListID;
            Singleton<CHeroSelectSystem>.GetInstance().SetPVEDataWithAdventure(iHeroNum, dwBattleListID, reportInfo, string.Empty);
            Singleton<CHeroSelectSystem>.GetInstance().m_gameType = enSelectHeroType.enPVE_Adventure;
            Singleton<CHeroSelectSystem>.GetInstance().LoadPveDefaultHeroList();
            Singleton<CHeroSelectSystem>.GetInstance().SendSinglePrepareToBattleMsg();
        }

        public void SendSinglePrepareToBattleMsg()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x70d);
            msg.stPkgData.stBattleListReq.stBattleList.dwBattleListID = this.m_battleListID;
            if (this.m_gameType == enSelectHeroType.enPVE_Computer)
            {
                msg.stPkgData.stBattleListReq.stBattleList.stBattleList.wHeroCnt = 1;
                MemberInfo masterMemberInfo = this.m_roomInfo.GetMasterMemberInfo();
                msg.stPkgData.stBattleListReq.stBattleList.stBattleList.BattleHeroList[0] = masterMemberInfo.ChoiceHero[0].stBaseInfo.stCommonInfo.dwHeroID;
            }
            else
            {
                msg.stPkgData.stBattleListReq.stBattleList.stBattleList.wHeroCnt = this.m_selectHeroCount;
                for (int i = 0; i < msg.stPkgData.stBattleListReq.stBattleList.stBattleList.wHeroCnt; i++)
                {
                    msg.stPkgData.stBattleListReq.stBattleList.stBattleList.BattleHeroList[i] = this.m_selectHeroIDList[i];
                }
            }
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public void SetPVEDataWithActivity(byte heroMaxCount, uint battleListID, CSDT_SINGLE_GAME_OF_ACTIVITY reportInfo, string levleName = "")
        {
            this.InitSelectHeroIDListAndBattleID(heroMaxCount, battleListID);
            this.m_stGameOfActivity = reportInfo;
            this.m_pveLevelName = levleName;
        }

        public void SetPVEDataWithAdventure(byte heroMaxCount, uint battleListID, CSDT_SINGLE_GAME_OF_ADVENTURE reportInfo, string levleName = "")
        {
            this.InitSelectHeroIDListAndBattleID(heroMaxCount, battleListID);
            this.m_stGameOfAdventure = reportInfo;
            this.m_pveLevelName = levleName;
        }

        public void SetPveDataWithArena(byte heroMaxCount, uint battleListID, CSDT_SINGLE_GAME_OF_ARENA reportInfo = null, string levleName = "")
        {
            this.InitSelectHeroIDListAndBattleID(heroMaxCount, battleListID);
            this.m_stGameOfArena = reportInfo;
            this.m_pveLevelName = levleName;
        }

        public void SetPVEDataWithBurnExpedition(byte heroMaxCount, uint battleListID, CSDT_SINGLE_GAME_OF_BURNING reportInfo, string levleName = "")
        {
            this.InitSelectHeroIDListAndBattleID(heroMaxCount, battleListID);
            this.m_stGameOfBurnning = reportInfo;
            this.m_pveLevelName = levleName;
        }

        public void SetPVEDataWithCombat(byte heroMaxCount, uint battleListID, CSDT_SINGLE_GAME_OF_COMBAT reportInfo, string levleName = "")
        {
            this.InitSelectHeroIDListAndBattleID(heroMaxCount, battleListID);
            this.m_stGameOfCombat = reportInfo;
            this.m_pveLevelName = levleName;
        }

        public void SetPvpDataByRoomInfo(Assets.Scripts.GameSystem.RoomInfo inRoomInfo, int inRoomType)
        {
            this.m_roomType = inRoomType;
            this.m_mapData = new ResDT_LevelCommonInfo();
            CLevelCfgLogicManager.InitMapDataInfo();
        }

        private int WhoIsBestHero(ref Calc9SlotHeroData[] heroes)
        {
            if (this.IsBetterHero(ref heroes[0], ref heroes[1]) && this.IsBetterHero(ref heroes[0], ref heroes[2]))
            {
                return 0;
            }
            if (this.IsBetterHero(ref heroes[1], ref heroes[0]) && this.IsBetterHero(ref heroes[1], ref heroes[2]))
            {
                return 1;
            }
            return 2;
        }

        public Assets.Scripts.GameSystem.RoomInfo m_roomInfo
        {
            get
            {
                return Singleton<CRoomSystem>.GetInstance().roomInfo;
            }
        }

        [CompilerGenerated]
        private sealed class <PostCombatSingleGame>c__AnonStorey43
        {
            internal int canPickHeroNum;

            internal void <>m__37(ResHeroCfgInfo heroCfg)
            {
                if ((heroCfg != null) && (heroCfg.bIsPlayerUse != 0))
                {
                    this.canPickHeroNum++;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <PostCombatSingleGame>c__AnonStorey44
        {
            internal ResHeroCfgInfo heroCfg;

            internal bool <>m__38(uint x)
            {
                return (x == this.heroCfg.dwCfgID);
            }
        }

        [CompilerGenerated]
        private sealed class <RefreshHeroInfo_RightPanel>c__AnonStorey40
        {
            internal CHeroSelectSystem.<RefreshHeroInfo_RightPanel>c__AnonStorey41 <>f__ref$65;
            internal CHeroSelectSystem.<RefreshHeroInfo_RightPanel>c__AnonStorey42 <>f__ref$66;
            internal uint selSkillID;

            internal void <>m__36(ResSkillUnlock rule)
            {
                if ((rule != null) && (rule.dwUnlockSkillID == this.selSkillID))
                {
                    ResSkillCfgInfo dataByKey = GameDataMgr.skillDatabin.GetDataByKey(this.selSkillID);
                    if (dataByKey != null)
                    {
                        this.<>f__ref$65.selSkillCell.CustomSetActive(true);
                        string prefabPath = string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Skill_Dir, Utility.UTF8Convert(dataByKey.szIconPath));
                        this.<>f__ref$65.selSkillCell.transform.Find("Icon").GetComponent<Image>().SetSprite(prefabPath, this.<>f__ref$66.form, true, false, false);
                    }
                }
            }
        }

        [CompilerGenerated]
        private sealed class <RefreshHeroInfo_RightPanel>c__AnonStorey41
        {
            internal GameObject selSkillCell;
        }

        [CompilerGenerated]
        private sealed class <RefreshHeroInfo_RightPanel>c__AnonStorey42
        {
            internal CUIFormScript form;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Calc9SlotHeroData
        {
            public uint ConfigId;
            public int RecommendPos;
            public uint Ability;
            public uint Level;
            public int Quality;
            public int BornIndex;
            public bool selected;
        }
    }
}

