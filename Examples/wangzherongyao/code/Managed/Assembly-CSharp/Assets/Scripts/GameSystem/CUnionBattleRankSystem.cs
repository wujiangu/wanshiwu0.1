namespace Assets.Scripts.GameSystem
{
    using Apollo;
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class CUnionBattleRankSystem : Singleton<CUnionBattleRankSystem>
    {
        private const int DAY_REWARD_NUM = 6;
        public static string DIAMOND_PATH = (CUIUtility.s_Sprite_Dynamic_Icon_Dir + "90005");
        public static string DIANJUAN_PATH = (CUIUtility.s_Sprite_Dynamic_Icon_Dir + "90002");
        private ResRewardMatchLevelInfo m_CurMapInfo = new ResRewardMatchLevelInfo();
        private uint m_CurSelMapId;
        private enUnionRankDateType m_CurSelRankDateType = enUnionRankDateType.enRankDateType_None;
        private int m_CurSelRankItemIndex = -1;
        private enUnionRankMatchType m_CurSelRankMatchType = enUnionRankMatchType.enRankMatchType_None;
        private enUnionRankType m_CurSelRankType = enUnionRankType.enRankType_None;
        private Dictionary<uint, uint> m_rewardPoolDic = new Dictionary<uint, uint>();
        private stUnionRankInfo[] m_UnionRankInfo = new stUnionRankInfo[0x1b];
        public static string PVPCOIN_PATH = (CUIUtility.s_Sprite_Dynamic_Icon_Dir + "90001");
        public static string UNION_RANK_PATH = "UGUI/Form/System/PvP/UnionBattle/Form_UnionRank";

        public void Clear()
        {
            for (int i = 0; i < 0x1b; i++)
            {
                this.m_UnionRankInfo[i].lastRetrieveTime = 0;
                this.m_UnionRankInfo[i].listInfo = null;
                this.m_UnionRankInfo[i].selfInfo = null;
                this.m_UnionRankInfo[i].selfTeamInfo = null;
            }
            this.m_rewardPoolDic.Clear();
        }

        public static COM_APOLLO_TRANK_SCORE_TYPE ConvertLocalToSeverRankType(enUnionRankType rankType)
        {
            COM_APOLLO_TRANK_SCORE_TYPE com_apollo_trank_score_type = COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_NULL;
            if ((rankType > enUnionRankType.enRankType_None) && (rankType < enUnionRankType.enRankType_Count))
            {
                com_apollo_trank_score_type = (COM_APOLLO_TRANK_SCORE_TYPE) (rankType + 0x21);
            }
            return com_apollo_trank_score_type;
        }

        private enUnionRankType ConvertMatchAndDateTypeToRankType(enUnionRankMatchType matchType, enUnionRankDateType dateType)
        {
            enUnionRankType type = enUnionRankType.enRankType_CoinMatch_Low_Day;
            if (((matchType > enUnionRankMatchType.enRankMatchType_None) && (matchType < enUnionRankMatchType.enRankMatchType_Count)) && ((dateType > enUnionRankDateType.enRankDateType_None) && (dateType < enUnionRankDateType.enRankDateType_Count)))
            {
                type = (enUnionRankType) ((dateType * ((enUnionRankDateType) 9)) + ((enUnionRankDateType) ((int) matchType)));
            }
            return type;
        }

        public static enUnionRankType ConvertSeverToLocalRankType(COM_APOLLO_TRANK_SCORE_TYPE rankType)
        {
            enUnionRankType type = enUnionRankType.enRankType_None;
            if ((rankType >= COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_COINMATCH_LOW_DAY) && (rankType <= COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_COUPONSMATCH_HIGH_GUILD))
            {
                type = (enUnionRankType) (rankType - 0x21);
            }
            return type;
        }

        private enUnionRankType GetCurSelRankType(enUnionRankDateType dateType)
        {
            enUnionRankType type = enUnionRankType.enRankType_CoinMatch_Low_Day;
            if (this.m_CurMapInfo != null)
            {
                uint key = 0;
                switch (dateType)
                {
                    case enUnionRankDateType.enRankDateType_Daily:
                        key = this.m_CurMapInfo.dwDayRankID;
                        break;

                    case enUnionRankDateType.enRankDateType_Season:
                        key = this.m_CurMapInfo.dwSeasonRankID;
                        break;

                    case enUnionRankDateType.enRankDateType_Team:
                        key = this.m_CurMapInfo.dwGuildRankID;
                        break;
                }
                ResRewardMatchConf conf = null;
                GameDataMgr.rewardMatchRewardDict.TryGetValue(key, out conf);
                if (conf != null)
                {
                    type = ConvertSeverToLocalRankType((COM_APOLLO_TRANK_SCORE_TYPE) conf.dwApolloType);
                }
            }
            return type;
        }

        private string GetMoneyIconPath(RES_SHOPBUY_COINTYPE coinType)
        {
            switch (coinType)
            {
                case RES_SHOPBUY_COINTYPE.RES_SHOPBUY_TYPE_COUPONS:
                    return DIANJUAN_PATH;

                case RES_SHOPBUY_COINTYPE.RES_SHOPBUY_TYPE_PVPCOIN:
                    return PVPCOIN_PATH;

                case RES_SHOPBUY_COINTYPE.RES_SHOPBUY_TYPE_DIAMOND:
                    return DIAMOND_PATH;
            }
            return string.Empty;
        }

        private COMDT_RANKING_LIST_ITEM_EXTRA_PLAYER GetRankItemDetailInfo(enUnionRankType rankType, int listIndex)
        {
            CSDT_RANKING_LIST_SUCC listInfo = this.m_UnionRankInfo[(int) this.m_CurSelRankType].listInfo;
            if (((listInfo != null) || (listIndex < listInfo.astItemDetail.Length)) || (listIndex < listInfo.dwItemNum))
            {
                switch (rankType)
                {
                    case enUnionRankType.enRankType_CoinMatch_Low_Day:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stLowCoinDay;

                    case enUnionRankType.enRankType_CoinMatch_Mid_Day:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stMidCoinDay;

                    case enUnionRankType.enRankType_CoinMatch_High_Day:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stHighCoinDay;

                    case enUnionRankType.enRankType_DiamondMatch_Low_Day:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stLowDiamDay;

                    case enUnionRankType.enRankType_DiamondMatch_Mid_Day:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stMidDiamDay;

                    case enUnionRankType.enRankType_DiamondMatch_High_Day:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stHighDiamDay;

                    case enUnionRankType.enRankType_CouponsMatch_Low_Day:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stLowCoupDay;

                    case enUnionRankType.enRankType_CouponsMatch_Mid_Day:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stMidCoupDay;

                    case enUnionRankType.enRankType_CouponsMatch_High_Day:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stHighCoupDay;

                    case enUnionRankType.enRankType_CoinMatch_Low_Season:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stLowCoinSeason;

                    case enUnionRankType.enRankType_CoinMatch_Mid_Season:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stMidCoinSeason;

                    case enUnionRankType.enRankType_CoinMatch_High_season:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stHighCoinSeason;

                    case enUnionRankType.enRankType_DiamondMatch_Low_Season:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stLowDiamSeason;

                    case enUnionRankType.enRankType_DiamondMatch_Mid_Season:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stMidDiamSeason;

                    case enUnionRankType.enRankType_DiamondMatch_High_Season:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stHighDiamSeason;

                    case enUnionRankType.enRankType_CouponsMatch_Low_Season:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stLowCoupSeason;

                    case enUnionRankType.enRankType_CouponsMatch_Mid_Season:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stMidCoupSeason;

                    case enUnionRankType.enRankType_CouponsMatch_High_Season:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stHighCoupSeason;
                }
            }
            return new COMDT_RANKING_LIST_ITEM_EXTRA_PLAYER();
        }

        private string GetSeasonEndTimeStr(ResRewardMatchConf unionRankReward)
        {
            string text = Singleton<CTextManager>.GetInstance().GetText("Union_Battle_Tips10");
            DateTime time = Utility.ToUtcTime2Local((long) Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().getCurrentTimeSinceLogin());
            if ((unionRankReward == null) || (unionRankReward.bRewardDateType != 3))
            {
                return text;
            }
            int num = (int) unionRankReward.RewardDateParam[0];
            int year = (int) (unionRankReward.RewardDateParam[0] / 0x2710);
            num = num % 0x2710;
            int month = num / 100;
            int day = num % 100;
            DateTime time2 = new DateTime(year, month, day);
            RES_TIME_INTERVAL_TYPE res_time_interval_type = (RES_TIME_INTERVAL_TYPE) unionRankReward.RewardDateParam[1];
            uint num5 = unionRankReward.RewardDateParam[2];
            while (time2.CompareTo(time) < 0)
            {
                if (res_time_interval_type == RES_TIME_INTERVAL_TYPE.RES_TIME_INTERVAL_TYPE_OFDAY)
                {
                    time2 = time2.AddDays((double) num5);
                }
                else
                {
                    if (res_time_interval_type != RES_TIME_INTERVAL_TYPE.RES_TIME_INTERVAL_TYPE_OFMONTH)
                    {
                        break;
                    }
                    time2 = time2.AddMonths((int) num5);
                    continue;
                }
            }
            return string.Format(text, time2.ToString("yyyy//MM//dd"));
        }

        private COMDT_RANKING_LIST_ITEM_EXTRA_GUILD_RANK_POINT GetTeamRankItemDetailInfo(enUnionRankType rankType, int listIndex)
        {
            CSDT_RANKING_LIST_SUCC listInfo = this.m_UnionRankInfo[(int) this.m_CurSelRankType].listInfo;
            if (((listInfo != null) || (listIndex < listInfo.astItemDetail.Length)) || (listIndex < listInfo.dwItemNum))
            {
                switch (rankType)
                {
                    case enUnionRankType.enRankType_CoinMatch_Low_Guild:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stLowCoinGuild;

                    case enUnionRankType.enRankType_CoinMatch_Mid_Guild:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stMidCoinGuild;

                    case enUnionRankType.enRankType_CoinMatch_High_Guild:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stHighCoinGuild;

                    case enUnionRankType.enRankType_DiamondMatch_Low_Guild:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stLowDiamGuild;

                    case enUnionRankType.enRankType_DiamondMatch_Mid_Guild:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stMidDiamGuild;

                    case enUnionRankType.enRankType_DiamonMatch_High_Guild:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stHighDiamGuild;

                    case enUnionRankType.enRankType_CouponsMatch_Low_Guild:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stLowCoupGuild;

                    case enUnionRankType.enRankType_CouponsMatch_Mid_Guild:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stMidCoupGuild;

                    case enUnionRankType.enRankType_CouponsMatch_High_Guild:
                        return listInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stHighCoupGuild;
                }
            }
            return new COMDT_RANKING_LIST_ITEM_EXTRA_GUILD_RANK_POINT();
        }

        public override void Init()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_Click_Rank, new CUIEventManager.OnUIEventHandler(this.OnClickRank));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_Click_MatchType_Menu, new CUIEventManager.OnUIEventHandler(this.OnClickMatchTypeMenu));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_Click_DateType_Menu, new CUIEventManager.OnUIEventHandler(this.OnClickDateTypeMenu));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_Rank_ClickDetail, new CUIEventManager.OnUIEventHandler(this.OnClickDetail));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_Rank_DateList_Element_Enable, new CUIEventManager.OnUIEventHandler(this.OnDateListElementEnable));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_Rank_TeamList_Element_Enable, new CUIEventManager.OnUIEventHandler(this.OnTeamListElementEnable));
            Singleton<EventRouter>.GetInstance().AddEventHandler<SCPKG_GET_RANKING_LIST_RSP>("UnionRank_Get_Rank_List", new Action<SCPKG_GET_RANKING_LIST_RSP>(this.OnGetRankList));
            Singleton<EventRouter>.GetInstance().AddEventHandler<SCPKG_GET_RANKING_ACNT_INFO_RSP>("UnionRank_Get_Rank_Account_Info", new Action<SCPKG_GET_RANKING_ACNT_INFO_RSP>(this.OnGetAccountInfo));
            Singleton<EventRouter>.GetInstance().AddEventHandler<SCPKG_GET_SPECIAL_GUILD_RANK_INFO_RSP>("UnionRank_Get_Rank_Team_Account_Info", new Action<SCPKG_GET_SPECIAL_GUILD_RANK_INFO_RSP>(this.OnGetTeamAccountInfo));
        }

        public void initWidget()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(UNION_RANK_PATH);
            if (form != null)
            {
                this.m_CurSelRankType = enUnionRankType.enRankType_None;
                this.m_CurSelRankDateType = enUnionRankDateType.enRankDateType_None;
                this.m_CurSelRankMatchType = enUnionRankMatchType.enRankMatchType_None;
                this.m_CurSelRankItemIndex = -1;
                this.m_CurSelMapId = 0;
                CUIListScript component = form.GetWidget(0).GetComponent<CUIListScript>();
                int count = GameDataMgr.uinionBattleLevelDatabin.count;
                component.SetElementAmount(count);
                int index = 0;
                for (int i = 0; i < count; i++)
                {
                    ResRewardMatchLevelInfo dataByIndex = GameDataMgr.uinionBattleLevelDatabin.GetDataByIndex(i);
                    if (CUICommonSystem.IsMatchOpened(RES_BATTLE_MAP_TYPE.RES_BATTLE_MAP_TYPE_REWARDMATCH, dataByIndex.dwMapId))
                    {
                        CUIListElementScript elemenet = component.GetElemenet(index);
                        if (elemenet != null)
                        {
                            CUIEventScript script4 = elemenet.GetComponent<CUIEventScript>();
                            elemenet.transform.FindChild("Text").GetComponent<Text>().text = dataByIndex.szMatchName;
                            script4.m_onClickEventParams.tagUInt = dataByIndex.dwMapId;
                            script4.m_onClickEventParams.commonUInt32Param1 = dataByIndex.dwMatchType;
                        }
                        index++;
                    }
                }
                if (index != count)
                {
                    component.SetElementAmount(index);
                }
            }
        }

        public bool IsNeedToRetrieveRankTypeInfo(enUnionRankType rankType)
        {
            if ((rankType <= enUnionRankType.enRankType_None) || (rankType >= enUnionRankType.enRankType_Count))
            {
                return false;
            }
            int index = (int) rankType;
            return (((this.m_UnionRankInfo[index].listInfo == null) || (this.m_UnionRankInfo[index].lastRetrieveTime == 0)) || (Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().getCurrentTimeSinceLogin() >= (this.m_UnionRankInfo[index].lastRetrieveTime + this.m_UnionRankInfo[index].listInfo.dwTimeLimit)));
        }

        private void OnClickDateTypeMenu(CUIEvent uiEvt)
        {
            this.SelectRankDateType((enUnionRankDateType) uiEvt.m_eventParams.commonUInt32Param1);
        }

        private void OnClickDetail(CUIEvent uiEvt)
        {
            int selectedIndex = uiEvt.m_srcWidgetBelongedListScript.GetComponent<CUIListScript>().GetSelectedIndex();
            this.m_CurSelRankItemIndex = selectedIndex;
            COMDT_RANKING_LIST_ITEM_EXTRA_PLAYER rankItemDetailInfo = this.GetRankItemDetailInfo(this.m_CurSelRankType, this.m_CurSelRankItemIndex);
            ulong ullUid = 0L;
            int iLogicWorldId = 0;
            if (rankItemDetailInfo != null)
            {
                ullUid = rankItemDetailInfo.ullUid;
                iLogicWorldId = rankItemDetailInfo.iLogicWorldId;
                if ((ullUid == Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().playerUllUID) && (iLogicWorldId == MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID))
                {
                    Singleton<CPlayerInfoSystem>.GetInstance().ShowPlayerDetailInfo(ullUid, iLogicWorldId, CPlayerInfoSystem.DetailPlayerInfoSource.Self);
                }
                else
                {
                    Singleton<CPlayerInfoSystem>.GetInstance().ShowPlayerDetailInfo(ullUid, iLogicWorldId, CPlayerInfoSystem.DetailPlayerInfoSource.DefaultOthers);
                }
            }
        }

        private void OnClickMatchTypeMenu(CUIEvent uiEvt)
        {
            this.SelectRankMatchType(uiEvt);
        }

        private void OnClickRank(CUIEvent uiEvt)
        {
            CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(UNION_RANK_PATH, false, true);
            if (script != null)
            {
                this.Clear();
                this.initWidget();
                CUIListScript component = script.GetWidget(0).GetComponent<CUIListScript>();
                component.SelectElement(0, true);
                CUIEventScript script3 = component.GetElemenet(0).GetComponent<CUIEventScript>();
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(script3.m_onClickEventID, script3.m_onClickEventParams);
            }
        }

        private void OnDateListElementEnable(CUIEvent uiEvt)
        {
            this.RefreshOneElement(uiEvt.m_srcWidget, uiEvt.m_srcWidgetIndexInBelongedList);
        }

        private void OnGetAccountInfo(SCPKG_GET_RANKING_ACNT_INFO_RSP acntInfo)
        {
            enUnionRankType type = ConvertSeverToLocalRankType((COM_APOLLO_TRANK_SCORE_TYPE) acntInfo.stAcntRankingDetail.stOfSucc.bNumberType);
            if (type != enUnionRankType.enRankType_None)
            {
                this.m_UnionRankInfo[(int) type].lastRetrieveTime = (uint) Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().getCurrentTimeSinceLogin();
                this.m_UnionRankInfo[(int) type].selfInfo = acntInfo.stAcntRankingDetail.stOfSucc;
                this.RefreshAcntInfo();
            }
        }

        private void OnGetRankList(SCPKG_GET_RANKING_LIST_RSP rankList)
        {
            enUnionRankType type = ConvertSeverToLocalRankType((COM_APOLLO_TRANK_SCORE_TYPE) rankList.stRankingListDetail.stOfSucc.bNumberType);
            if (type != enUnionRankType.enRankType_None)
            {
                this.m_UnionRankInfo[(int) type].lastRetrieveTime = (uint) Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().getCurrentTimeSinceLogin();
                this.m_UnionRankInfo[(int) type].listInfo = rankList.stRankingListDetail.stOfSucc;
                if (this.m_CurSelRankDateType != enUnionRankDateType.enRankDateType_Team)
                {
                    this.RefreshRankList();
                }
                else
                {
                    this.RefreshTeamRankList();
                }
            }
        }

        private void OnGetTeamAccountInfo(SCPKG_GET_SPECIAL_GUILD_RANK_INFO_RSP teamAcntInfo)
        {
            SCPKG_GET_SPECIAL_GUILD_RANK_INFO_RSP scpkg_get_special_guild_rank_info_rsp = teamAcntInfo;
            enUnionRankType type = ConvertSeverToLocalRankType((COM_APOLLO_TRANK_SCORE_TYPE) scpkg_get_special_guild_rank_info_rsp.bNumberType);
            if (type != enUnionRankType.enRankType_None)
            {
                this.m_UnionRankInfo[(int) type].lastRetrieveTime = (uint) Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().getCurrentTimeSinceLogin();
                this.m_UnionRankInfo[(int) type].selfTeamInfo = scpkg_get_special_guild_rank_info_rsp.stItemDetail;
                this.RefreshTeamAcntInfo();
            }
        }

        private void OnTeamListElementEnable(CUIEvent uiEvt)
        {
            this.RefreshOneTeamElement(uiEvt.m_srcWidget, uiEvt.m_srcWidgetIndexInBelongedList, uiEvt.m_srcFormScript);
        }

        private static void RankNobSet(uint rankNumber, RankingItemHelper rankingHelper)
        {
            rankingHelper.RankingNumText.CustomSetActive(false);
            rankingHelper.No1.CustomSetActive(false);
            rankingHelper.No2.CustomSetActive(false);
            rankingHelper.No3.CustomSetActive(false);
            rankingHelper.No1BG.CustomSetActive(false);
            if (rankNumber == 0)
            {
                if (rankingHelper.NoRankingText != null)
                {
                    rankingHelper.NoRankingText.CustomSetActive(true);
                }
            }
            else
            {
                if (rankingHelper.NoRankingText != null)
                {
                    rankingHelper.NoRankingText.CustomSetActive(false);
                }
                switch (rankNumber)
                {
                    case 1:
                        rankingHelper.No1.CustomSetActive(true);
                        if (rankingHelper.No1BG != null)
                        {
                            rankingHelper.No1BG.CustomSetActive(true);
                        }
                        return;

                    case 2:
                        rankingHelper.No2.CustomSetActive(true);
                        return;

                    case 3:
                        rankingHelper.No3.CustomSetActive(true);
                        return;
                }
                rankingHelper.RankingNumText.CustomSetActive(true);
                rankingHelper.RankingNumText.GetComponent<Text>().text = string.Format("{0}", rankNumber);
            }
        }

        private void RefreshAcntInfo()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(UNION_RANK_PATH);
            if (form != null)
            {
                CSDT_GET_RANKING_ACNT_DETAIL_SELF selfInfo = this.m_UnionRankInfo[(int) this.m_CurSelRankType].selfInfo;
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                GameObject widget = form.GetWidget(6);
                RankingItemHelper component = widget.GetComponent<RankingItemHelper>();
                uint rankNumber = 0;
                if ((selfInfo != null) && (masterRoleInfo != null))
                {
                    widget.CustomSetActive(true);
                    string name = masterRoleInfo.Name;
                    uint level = masterRoleInfo.Level;
                    rankNumber = selfInfo.dwRankNo;
                    int iRankChgNo = selfInfo.iRankChgNo;
                    uint dwDayPoint = 0;
                    if (rankNumber == 0)
                    {
                        SCPKG_MATCHPOINT_NTF personInfo = Singleton<CUnionBattleEntrySystem>.instance.m_personInfo;
                        for (int i = 0; i < personInfo.dwCount; i++)
                        {
                            if (personInfo.astPointList[i].dwMapId == this.m_CurSelMapId)
                            {
                                if (this.m_CurSelRankDateType == enUnionRankDateType.enRankDateType_Daily)
                                {
                                    dwDayPoint = personInfo.astPointList[i].dwDayPoint;
                                    break;
                                }
                                if (this.m_CurSelRankDateType == enUnionRankDateType.enRankDateType_Season)
                                {
                                    dwDayPoint = personInfo.astPointList[i].dwSeasonPoint;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        dwDayPoint = selfInfo.dwScore;
                    }
                    widget.transform.FindChild("Value").gameObject.CustomSetActive(true);
                    widget.transform.FindChild("ValueType").gameObject.CustomSetActive(true);
                    SetGameObjChildText(widget, "Value", dwDayPoint.ToString(CultureInfo.InvariantCulture));
                    SetGameObjChildText(widget, "NameGroup/PlayerName", masterRoleInfo.Name);
                    SetGameObjChildText(widget, "PlayerLv", string.Format("Lv.{0}", level.ToString(CultureInfo.InvariantCulture)));
                }
                RankNobSet(rankNumber, component);
                MonoSingleton<NobeSys>.GetInstance().SetMyQQVipHead(component.QqVip.GetComponent<Image>());
                CUICommonSystem.SetHostHeadItemCell(widget.transform.FindChild("HeadItemCell").gameObject);
                MonoSingleton<NobeSys>.GetInstance().SetGameCenterVisible(component.WxIcon, Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().m_privilegeType, ApolloPlatform.Wechat, true, false);
            }
        }

        public void RefreshDayReward()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(UNION_RANK_PATH);
            if (form != null)
            {
                Transform transform = form.GetWidget(3).transform;
                Text component = transform.FindChild("RewardPool/Reward/Text").GetComponent<Text>();
                Image image = transform.FindChild("RewardPool/Reward/Icon").GetComponent<Image>();
                ResRewardMatchLevelInfo curMapInfo = this.m_CurMapInfo;
                ResRewardMatchDetailConf dataByKey = null;
                if (curMapInfo != null)
                {
                    ResRewardMatchConf conf2 = null;
                    GameDataMgr.rewardMatchRewardDict.TryGetValue(curMapInfo.dwDayRankID, out conf2);
                    if (conf2 != null)
                    {
                        dataByKey = GameDataMgr.unionRankRewardDetailDatabin.GetDataByKey(conf2.dwRewardDetailId);
                    }
                    if (dataByKey != null)
                    {
                        uint num;
                        string moneyIconPath = this.GetMoneyIconPath((RES_SHOPBUY_COINTYPE) dataByKey.bCoinType);
                        image.SetSprite(moneyIconPath, form, true, false, false);
                        uint dwInitCoinPool = dataByKey.dwInitCoinPool;
                        ulong num3 = dwInitCoinPool;
                        if (!this.m_rewardPoolDic.TryGetValue(this.m_CurSelMapId, out num))
                        {
                            CUnionBattleEntrySystem.SendAwartPoolReq(this.m_CurSelMapId);
                        }
                        else
                        {
                            num3 = dwInitCoinPool + num;
                            component.text = num3.ToString();
                            CUIListScript script2 = transform.FindChild("RewardDec/RewardList").GetComponent<CUIListScript>();
                            script2.SetElementAmount(6);
                            for (int i = 0; i < 6; i++)
                            {
                                Transform transform2 = script2.GetElemenet(i).transform.FindChild("Reward").transform;
                                Image image2 = transform2.FindChild("Image").GetComponent<Image>();
                                Text text2 = transform2.FindChild("Text").GetComponent<Text>();
                                Text text3 = transform2.FindChild("Title").GetComponent<Text>();
                                image2.SetSprite(moneyIconPath, form, true, false, false);
                                text2.text = (dataByKey.astRewardDetail[i].dwFixCoin + ((num3 * dataByKey.astRewardDetail[i].dwCoinRatio) / ((ulong) 0x2710L))).ToString();
                                int num6 = i + 1;
                                text3.text = Singleton<CTextManager>.GetInstance().GetText(string.Format("RewardMatch_DayReward_Txt{0}", num6.ToString()));
                            }
                        }
                    }
                }
            }
        }

        private void RefreshOneElement(GameObject element, int index)
        {
            CSDT_RANKING_LIST_SUCC listInfo = this.m_UnionRankInfo[(int) this.m_CurSelRankType].listInfo;
            int num = index;
            if (((element != null) && (listInfo != null)) && ((num < listInfo.astItemDetail.Length) && (num < listInfo.dwItemNum)))
            {
                RankingItemHelper component = element.GetComponent<RankingItemHelper>();
                uint dwRankScore = 0;
                string text = string.Empty;
                uint dwPvpLevel = 1;
                string serverUrl = null;
                uint dwCurLevel = 0;
                uint dwHeadIconId = 0;
                uint dwVipLevel = 0;
                COM_PRIVILEGE_TYPE privilegeType = COM_PRIVILEGE_TYPE.COM_PRIVILEGE_TYPE_NONE;
                dwRankScore = listInfo.astItemDetail[num].dwRankScore;
                COMDT_RANKING_LIST_ITEM_EXTRA_PLAYER rankItemDetailInfo = this.GetRankItemDetailInfo(this.m_CurSelRankType, num);
                text = StringHelper.UTF8BytesToString(ref rankItemDetailInfo.szPlayerName);
                dwPvpLevel = rankItemDetailInfo.dwPvpLevel;
                serverUrl = Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(ref rankItemDetailInfo.szHeadUrl);
                dwCurLevel = rankItemDetailInfo.stGameVip.dwCurLevel;
                dwHeadIconId = rankItemDetailInfo.stGameVip.dwHeadIconId;
                privilegeType = (COM_PRIVILEGE_TYPE) rankItemDetailInfo.bPrivilege;
                dwVipLevel = rankItemDetailInfo.dwVipLevel;
                MonoSingleton<NobeSys>.GetInstance().SetGameCenterVisible(component.WxIcon, privilegeType, ApolloPlatform.Wechat, true, false);
                SetGameObjChildText(element, "NameGroup/PlayerName", text);
                SetGameObjChildText(element, "PlayerLv", string.Format("Lv.{0}", Math.Max(1, dwPvpLevel)));
                element.transform.FindChild("Value").gameObject.CustomSetActive(true);
                SetGameObjChildText(element, "Value", dwRankScore.ToString(CultureInfo.InvariantCulture));
                uint rankNumber = (uint) (num + 1);
                RankNobSet(rankNumber, component);
                if (!CSysDynamicBlock.bSocialBlocked)
                {
                    if (rankItemDetailInfo.ullUid == Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().playerUllUID)
                    {
                        MonoSingleton<NobeSys>.GetInstance().SetMyQQVipHead(component.QqVip.GetComponent<Image>());
                        MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(component.VipIcon.GetComponent<Image>(), (int) Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().GetNobeInfo().stGameVipClient.dwCurLevel, false);
                        MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(component.HeadIconFrame.GetComponent<Image>(), (int) Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().GetNobeInfo().stGameVipClient.dwHeadIconId);
                        MonoSingleton<NobeSys>.GetInstance().SetGameCenterVisible(component.WxIcon, Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().m_privilegeType, ApolloPlatform.Wechat, true, false);
                        RankingSystem.SetHostUrlHeadIcon(component.HeadIcon);
                    }
                    else
                    {
                        MonoSingleton<NobeSys>.GetInstance().SetOtherQQVipHead(component.QqVip.GetComponent<Image>(), (int) dwVipLevel);
                        MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(component.VipIcon.GetComponent<Image>(), (int) dwCurLevel, false);
                        MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(component.HeadIconFrame.GetComponent<Image>(), (int) dwHeadIconId);
                        MonoSingleton<NobeSys>.GetInstance().SetGameCenterVisible(component.WxIcon, privilegeType, ApolloPlatform.Wechat, true, false);
                        RankingSystem.SetUrlHeadIcon(component.HeadIcon, serverUrl);
                    }
                }
            }
        }

        private void RefreshOneTeamElement(GameObject element, int index, CUIFormScript form)
        {
            if (form != null)
            {
                CSDT_RANKING_LIST_SUCC listInfo = this.m_UnionRankInfo[(int) this.m_CurSelRankType].listInfo;
                int num = index;
                if (((element != null) && (listInfo != null)) && ((num < listInfo.astItemDetail.Length) && (num < listInfo.dwItemNum)))
                {
                    RankingItemHelper component = element.GetComponent<RankingItemHelper>();
                    COMDT_RANKING_LIST_ITEM_EXTRA_GUILD_RANK_POINT teamRankItemDetailInfo = this.GetTeamRankItemDetailInfo(this.m_CurSelRankType, index);
                    uint dwRankScore = listInfo.astItemDetail[index].dwRankScore;
                    string str = StringHelper.UTF8BytesToString(ref teamRankItemDetailInfo.szGuildName);
                    uint dwGuildHeadID = teamRankItemDetailInfo.dwGuildHeadID;
                    byte bGuildLevel = teamRankItemDetailInfo.bGuildLevel;
                    byte bMemberNum = teamRankItemDetailInfo.bMemberNum;
                    int gradeByRankpointScore = (int) CGuildHelper.GetGradeByRankpointScore(teamRankItemDetailInfo.dwTotalRankPoint);
                    Utility.GetComponetInChild<Text>(element, "txtName").text = str;
                    Utility.GetComponetInChild<Text>(element, "Score").text = dwRankScore.ToString();
                    Utility.GetComponetInChild<Text>(element, "MemberNum").text = bMemberNum + "/" + CGuildHelper.GetMaxGuildMemberCountByLevel(bGuildLevel);
                    Utility.GetComponetInChild<Image>(element, "GuildIcon").SetSprite(CUIUtility.s_Sprite_Dynamic_GuildHead_Dir + dwGuildHeadID, form, true, false, false);
                    Utility.GetComponetInChild<Image>(element, "LadderIcon").SetSprite(CGuildHelper.GetGradeIconPathByGrade(gradeByRankpointScore), form, true, false, false);
                    uint rankNumber = (uint) (index + 1);
                    RankNobSet(rankNumber, component);
                }
            }
        }

        private void RefreshRankList()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(UNION_RANK_PATH);
            if (form != null)
            {
                GameObject widget = form.GetWidget(2);
                CSDT_RANKING_LIST_SUCC listInfo = this.m_UnionRankInfo[(int) this.m_CurSelRankType].listInfo;
                Transform transform = widget.transform.FindChild("RankingList");
                Transform transform2 = widget.transform.FindChild("NoRankTxt");
                if ((listInfo == null) || (listInfo.dwItemNum == 0))
                {
                    transform.gameObject.CustomSetActive(false);
                    transform2.gameObject.CustomSetActive(true);
                }
                else
                {
                    transform.gameObject.CustomSetActive(true);
                    transform2.gameObject.CustomSetActive(false);
                    int dwItemNum = (int) listInfo.dwItemNum;
                    CUIListScript component = transform.GetComponent<CUIListScript>();
                    component.SetElementAmount(dwItemNum);
                    component.MoveElementInScrollArea(0, true);
                    for (int i = 0; i < dwItemNum; i++)
                    {
                        if ((component.GetElemenet(i) != null) && component.IsElementInScrollArea(i))
                        {
                            this.RefreshOneElement(component.GetElemenet(i).gameObject, i);
                        }
                    }
                }
            }
        }

        private void RefreshSeasonReward(CUIFormScript form)
        {
            Transform transform = form.GetWidget(4).transform;
            Image component = transform.FindChild("Reward/Image").GetComponent<Image>();
            Text text = transform.FindChild("Intro1/Text").GetComponent<Text>();
            Text text2 = transform.FindChild("Intro2/Text").GetComponent<Text>();
            string prefabPath = CUIUtility.s_Sprite_Dynamic_UnionBattleBaner_Dir + this.m_CurMapInfo.dwSeasonAwardIconID.ToString();
            component.SetSprite(prefabPath, form, true, false, false);
            ResRewardMatchConf conf = null;
            GameDataMgr.rewardMatchRewardDict.TryGetValue(this.m_CurMapInfo.dwSeasonRankID, out conf);
            if (conf != null)
            {
                text.text = this.GetSeasonEndTimeStr(conf);
                text2.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Union_Battle_Tips11"), conf.dwMaxRewardCount);
            }
        }

        private void RefreshTeamAcntInfo()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(UNION_RANK_PATH);
            if (form != null)
            {
                CSDT_RANKING_LIST_ITEM_INFO selfTeamInfo = this.m_UnionRankInfo[(int) this.m_CurSelRankType].selfTeamInfo;
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                Transform transform = form.GetWidget(7).transform;
                GameObject gameObject = transform.FindChild("Panel_AcntGuild").gameObject;
                GameObject obj3 = transform.FindChild("NoGuilTxt").gameObject;
                if ((masterRoleInfo != null) && (selfTeamInfo != null))
                {
                    if (Singleton<CGuildSystem>.GetInstance().IsInNormalGuild())
                    {
                        gameObject.CustomSetActive(true);
                        obj3.CustomSetActive(false);
                        uint dwRankNo = selfTeamInfo.dwRankNo;
                        uint dwRankScore = selfTeamInfo.dwRankScore;
                        string name = masterRoleInfo.m_baseGuildInfo.name;
                        int guildMemberCount = CGuildHelper.GetGuildMemberCount();
                        int guildLevel = CGuildHelper.GetGuildLevel();
                        RankingItemHelper component = transform.GetComponent<RankingItemHelper>();
                        int guildGrade = (int) CGuildHelper.GetGuildGrade();
                        Utility.GetComponetInChild<Text>(gameObject, "txtName").text = name;
                        Utility.GetComponetInChild<Text>(gameObject, "Score").text = dwRankScore.ToString();
                        Utility.GetComponetInChild<Text>(gameObject, "MemberNum").text = guildMemberCount + "/" + CGuildHelper.GetMaxGuildMemberCountByLevel((guildLevel != -1) ? guildLevel : 1);
                        Utility.GetComponetInChild<Image>(gameObject, "GuildIcon").SetSprite(CGuildHelper.GetGuildHeadPath(), form, true, false, false);
                        Utility.GetComponetInChild<Image>(gameObject, "LadderIcon").SetSprite(CGuildHelper.GetGradeIconPathByGrade(guildGrade), form, true, false, false);
                        RankNobSet(dwRankNo, component);
                    }
                    else
                    {
                        gameObject.CustomSetActive(false);
                        obj3.CustomSetActive(true);
                    }
                }
            }
        }

        private void RefreshTeamRankList()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(UNION_RANK_PATH);
            if (form != null)
            {
                if (this.IsNeedToRetrieveRankTypeInfo(this.m_CurSelRankType))
                {
                    this.RetrieveRankTypeInfo(this.m_CurSelRankType);
                }
                GameObject widget = form.GetWidget(5);
                CSDT_RANKING_LIST_SUCC listInfo = this.m_UnionRankInfo[(int) this.m_CurSelRankType].listInfo;
                Transform transform = widget.transform.FindChild("RankingList");
                Transform transform2 = widget.transform.FindChild("NoRankTxt");
                if ((listInfo == null) || (listInfo.dwItemNum == 0))
                {
                    transform.gameObject.CustomSetActive(false);
                    transform2.gameObject.CustomSetActive(true);
                }
                else
                {
                    transform.gameObject.CustomSetActive(true);
                    transform2.gameObject.CustomSetActive(false);
                    int dwItemNum = (int) listInfo.dwItemNum;
                    CUIListScript component = transform.GetComponent<CUIListScript>();
                    component.SetElementAmount(dwItemNum);
                    component.MoveElementInScrollArea(0, true);
                    for (int i = 0; i < dwItemNum; i++)
                    {
                        if ((component.GetElemenet(i) != null) && component.IsElementInScrollArea(i))
                        {
                            this.RefreshOneTeamElement(component.GetElemenet(i).gameObject, i, form);
                        }
                    }
                }
            }
        }

        public static void ReqRankAcntInfo(COM_APOLLO_TRANK_SCORE_TYPE rankType)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0xa2c);
            msg.stPkgData.stGetRankingAcntInfoReq.bNumberType = (byte) rankType;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void ReqRankListInfo(COM_APOLLO_TRANK_SCORE_TYPE rankType)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0xa2a);
            msg.stPkgData.stGetRankingListReq.bNumberType = (byte) rankType;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void ReqTeamRankAcntInfo(COM_APOLLO_TRANK_SCORE_TYPE setverRankType)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0xa37);
            msg.stPkgData.stGetSpecialGuildRankInfoReq.bNumberType = (byte) setverRankType;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
        }

        private void RetrieveRankTypeInfo(enUnionRankType rankType)
        {
            COM_APOLLO_TRANK_SCORE_TYPE com_apollo_trank_score_type = ConvertLocalToSeverRankType(rankType);
            ReqRankListInfo(com_apollo_trank_score_type);
            if (this.m_CurSelRankDateType != enUnionRankDateType.enRankDateType_Team)
            {
                ReqRankAcntInfo(com_apollo_trank_score_type);
            }
            else
            {
                ReqTeamRankAcntInfo(com_apollo_trank_score_type);
            }
        }

        private void SelectRankDateType(enUnionRankDateType rankDateType)
        {
            this.m_CurSelRankDateType = rankDateType;
            enUnionRankType curSelRankType = this.GetCurSelRankType(this.m_CurSelRankDateType);
            if (this.m_CurSelRankType != curSelRankType)
            {
                this.m_CurSelRankType = curSelRankType;
                CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(UNION_RANK_PATH);
                if (form != null)
                {
                    GameObject widget = form.GetWidget(2);
                    GameObject obj3 = form.GetWidget(3);
                    GameObject obj4 = form.GetWidget(4);
                    GameObject obj5 = form.GetWidget(5);
                    GameObject obj6 = form.GetWidget(6);
                    switch (this.m_CurSelRankDateType)
                    {
                        case enUnionRankDateType.enRankDateType_Daily:
                            widget.CustomSetActive(true);
                            obj3.CustomSetActive(true);
                            obj4.CustomSetActive(false);
                            obj5.CustomSetActive(false);
                            obj6.CustomSetActive(true);
                            if (this.IsNeedToRetrieveRankTypeInfo(this.m_CurSelRankType))
                            {
                                this.RetrieveRankTypeInfo(this.m_CurSelRankType);
                            }
                            this.RefreshRankList();
                            this.RefreshAcntInfo();
                            this.RefreshDayReward();
                            break;

                        case enUnionRankDateType.enRankDateType_Season:
                            widget.CustomSetActive(true);
                            obj3.CustomSetActive(false);
                            obj4.CustomSetActive(true);
                            obj5.CustomSetActive(false);
                            obj6.CustomSetActive(true);
                            if (this.IsNeedToRetrieveRankTypeInfo(this.m_CurSelRankType))
                            {
                                this.RetrieveRankTypeInfo(this.m_CurSelRankType);
                            }
                            this.RefreshRankList();
                            this.RefreshAcntInfo();
                            this.RefreshSeasonReward(form);
                            break;

                        case enUnionRankDateType.enRankDateType_Team:
                            widget.CustomSetActive(false);
                            obj3.CustomSetActive(false);
                            obj4.CustomSetActive(false);
                            obj5.CustomSetActive(true);
                            obj6.CustomSetActive(false);
                            if (this.IsNeedToRetrieveRankTypeInfo(this.m_CurSelRankType))
                            {
                                this.RetrieveRankTypeInfo(this.m_CurSelRankType);
                            }
                            this.RefreshTeamRankList();
                            this.RefreshTeamAcntInfo();
                            break;
                    }
                }
            }
        }

        private void SelectRankMatchType(CUIEvent uiEvt)
        {
            uint tagUInt = uiEvt.m_eventParams.tagUInt;
            enUnionRankMatchType type = (enUnionRankMatchType) uiEvt.m_eventParams.commonUInt32Param1;
            if (this.m_CurSelMapId != tagUInt)
            {
                this.m_CurSelMapId = tagUInt;
                this.m_CurSelRankMatchType = type;
                this.m_CurMapInfo = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey(this.m_CurSelMapId);
                CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(UNION_RANK_PATH);
                if (form != null)
                {
                    CUIListScript component = form.GetWidget(1).GetComponent<CUIListScript>();
                    int amount = 3;
                    uint[] numArray = new uint[amount];
                    numArray[0] = this.m_CurMapInfo.dwDayRankID;
                    numArray[1] = this.m_CurMapInfo.dwSeasonRankID;
                    numArray[2] = this.m_CurMapInfo.dwGuildRankID;
                    for (int i = 0; i < numArray.Length; i++)
                    {
                        if (numArray[i] == 0)
                        {
                            amount--;
                        }
                    }
                    component.SetElementAmount(amount);
                    int index = 0;
                    for (int j = 0; (j < 3) && (index < amount); j++)
                    {
                        if (numArray[j] != 0)
                        {
                            CUIListElementScript elemenet = component.GetElemenet(index);
                            CUIEventScript script4 = elemenet.GetComponent<CUIEventScript>();
                            index++;
                            elemenet.transform.FindChild("Text").GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText(((enUnionRankDateType) j).ToString());
                            script4.m_onClickEventParams.commonUInt32Param1 = (uint) j;
                        }
                    }
                    CUIListScript script5 = form.GetWidget(1).GetComponent<CUIListScript>();
                    script5.SelectElement(0, true);
                    CUIEventScript script6 = script5.GetElemenet(0).GetComponent<CUIEventScript>();
                    Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(script6.m_onClickEventID, script6.m_onClickEventParams);
                }
            }
        }

        private static void SetGameObjChildText(GameObject parentObj, string childName, string text)
        {
            if (parentObj != null)
            {
                parentObj.transform.FindChild(childName).gameObject.GetComponent<Text>().text = text;
            }
        }

        public void SetRewardPoolInfo(SCPKG_GETAWARDPOOL_RSP rewardPool)
        {
            uint dwAwardPoolNum;
            if (this.m_rewardPoolDic.TryGetValue(rewardPool.dwMapId, out dwAwardPoolNum))
            {
                dwAwardPoolNum = rewardPool.dwAwardPoolNum;
            }
            else
            {
                this.m_rewardPoolDic.Add(rewardPool.dwMapId, rewardPool.dwAwardPoolNum);
            }
        }

        public override void UnInit()
        {
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_Click_Rank, new CUIEventManager.OnUIEventHandler(this.OnClickRank));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_Click_MatchType_Menu, new CUIEventManager.OnUIEventHandler(this.OnClickMatchTypeMenu));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_Click_DateType_Menu, new CUIEventManager.OnUIEventHandler(this.OnClickDateTypeMenu));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_Rank_ClickDetail, new CUIEventManager.OnUIEventHandler(this.OnClickDetail));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_Rank_DateList_Element_Enable, new CUIEventManager.OnUIEventHandler(this.OnDateListElementEnable));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_Rank_TeamList_Element_Enable, new CUIEventManager.OnUIEventHandler(this.OnTeamListElementEnable));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<SCPKG_GET_RANKING_LIST_RSP>("UnionRank_Get_Rank_List", new Action<SCPKG_GET_RANKING_LIST_RSP>(this.OnGetRankList));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<SCPKG_GET_RANKING_ACNT_INFO_RSP>("UnionRank_Get_Rank_Account_Info", new Action<SCPKG_GET_RANKING_ACNT_INFO_RSP>(this.OnGetAccountInfo));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<SCPKG_GET_SPECIAL_GUILD_RANK_INFO_RSP>("UnionRank_Get_Rank_Team_Account_Info", new Action<SCPKG_GET_SPECIAL_GUILD_RANK_INFO_RSP>(this.OnGetTeamAccountInfo));
        }

        public enum enWidget
        {
            enWidGet_MatchTypeMenu,
            enWidGet_DateTypeMenu,
            enWidGet_RankList,
            enWidGet_DayReward,
            enWidGet_SeasonReward,
            enWidGet_TeamRankList,
            enWidGet_SelfInfo,
            enWidGet_TeamSelfInfo,
            enWidGet_Count
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct stUnionRankInfo
        {
            public uint lastRetrieveTime;
            public CSDT_RANKING_LIST_SUCC listInfo;
            public CSDT_GET_RANKING_ACNT_DETAIL_SELF selfInfo;
            public CSDT_RANKING_LIST_ITEM_INFO selfTeamInfo;
        }
    }
}

