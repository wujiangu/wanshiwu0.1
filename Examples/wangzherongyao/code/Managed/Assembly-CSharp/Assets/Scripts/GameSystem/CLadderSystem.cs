namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [MessageHandlerClass]
    public class CLadderSystem : Singleton<CLadderSystem>
    {
        private uint _ranking;
        private COMDT_RANKDETAIL currentRankDetail;
        private List<COMDT_RANK_CURSEASON_FIGHT_RECORD> currentSeasonGames;
        public static readonly string FORM_AD_PREFAB = "UGUI/Form/System/IDIPNotice/Form_LobbyADForm.prefab";
        public static readonly string FORM_LADDER_ENTRY = "UGUI/Form/System/PvP/Ladder/Form_LadderEntry.prefab";
        public static readonly string FORM_LADDER_HISTORY = "UGUI/Form/System/PvP/Ladder/Form_LadderHistory.prefab";
        public static readonly string FORM_LADDER_RECENT = "UGUI/Form/System/PvP/Ladder/Form_RecentLadderMatch.prefab";
        public static readonly string FORM_LADDER_REWARD = "UGUI/Form/System/PvP/Ladder/Form_LadderReward.prefab";
        private List<COMDT_RANK_PASTSEASON_FIGHT_RECORD> historySeasonData;
        private static string image_name = "UGUI/Sprite/Dynamic/Competition/million";
        public const ushort LADDER_RULE_ID = 1;
        public const int MAX_NEED_SCORE = 5;
        public static int MAX_RANK_LEVEL;
        public const int NUM_LADDER_BY_TEAM = 2;
        public static uint REQ_HERO_NUM;
        public static uint REQ_PLAYER_LEVEL;

        [MessageHandler(0xb65)]
        public static void AddCurrentSeasonRecord(CSPkg msg)
        {
            Singleton<CLadderSystem>.GetInstance().AddRecentGameData(msg.stPkgData.stNtfAddCurSeasonRecord.stRecord);
        }

        [MessageHandler(0xb66)]
        public static void AddHistorySeasonRecord(CSPkg msg)
        {
            Singleton<CLadderSystem>.GetInstance().AddRecentSeasonData(msg.stPkgData.stNtfAddPastSeasonRecord.stRecord);
        }

        private void AddRecentGameData(COMDT_RANK_CURSEASON_FIGHT_RECORD gameData)
        {
            if (this.currentSeasonGames == null)
            {
                this.currentSeasonGames = new List<COMDT_RANK_CURSEASON_FIGHT_RECORD>();
            }
            this.currentSeasonGames.Add(gameData);
            this.currentSeasonGames.Sort(new Comparison<COMDT_RANK_CURSEASON_FIGHT_RECORD>(CLadderSystem.ComparisonGameData));
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(FORM_LADDER_ENTRY);
            if (form != null)
            {
                CLadderView.SetMostRecentGameData(form, ref this.currentRankDetail, this.currentSeasonGames);
            }
            CUIFormScript script2 = Singleton<CUIManager>.GetInstance().GetForm(FORM_LADDER_RECENT);
            if (script2 != null)
            {
                CLadderView.InitLadderRecent(script2, this.currentSeasonGames);
            }
        }

        private void AddRecentSeasonData(COMDT_RANK_PASTSEASON_FIGHT_RECORD gameData)
        {
            if (this.historySeasonData == null)
            {
                this.historySeasonData = new List<COMDT_RANK_PASTSEASON_FIGHT_RECORD>();
            }
            this.historySeasonData.Add(gameData);
            this.historySeasonData.Sort(new Comparison<COMDT_RANK_PASTSEASON_FIGHT_RECORD>(CLadderSystem.ComparisonHistoryData));
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(FORM_LADDER_HISTORY);
            if (form != null)
            {
                CLadderView.InitLadderHistory(form, this.historySeasonData);
            }
        }

        private void BeginMatch()
        {
            CMatchingSystem.ReqStartSingleMatching(CLevelCfgLogicManager.GetRankBattleMapID(), false, COM_BATTLE_MAP_TYPE.COM_BATTLE_MAP_TYPE_RANK);
        }

        private bool CanOpenLadderEntry()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo == null)
            {
                return false;
            }
            if (masterRoleInfo.PvpLevel < REQ_PLAYER_LEVEL)
            {
                object[] replaceArr = new object[] { REQ_PLAYER_LEVEL };
                Singleton<CUIManager>.GetInstance().OpenTips("Activity_Open", true, 1f, null, replaceArr);
                return false;
            }
            if (Singleton<CMatchingSystem>.GetInstance().IsInMatching)
            {
                Singleton<CUIManager>.GetInstance().OpenTips("PVP_Matching", true, 1f, null, new object[0]);
                return false;
            }
            if (!IsInSeason())
            {
                Singleton<CUIManager>.GetInstance().OpenTips("Rank_Not_In_Season", true, 1f, null, new object[0]);
                return false;
            }
            if (this.IsQualified() && (masterRoleInfo.m_gradeOfRank <= 0))
            {
                Singleton<CUIManager>.GetInstance().OpenTips("Ladder_Data_Error", false, 1f, null, new object[0]);
                return false;
            }
            return true;
        }

        private static int ComparisonGameData(COMDT_RANK_CURSEASON_FIGHT_RECORD a, COMDT_RANK_CURSEASON_FIGHT_RECORD b)
        {
            if (a.dwFightTime > b.dwFightTime)
            {
                return -1;
            }
            if (a.dwFightTime < b.dwFightTime)
            {
                return 1;
            }
            return 0;
        }

        private static int ComparisonHistoryData(COMDT_RANK_PASTSEASON_FIGHT_RECORD a, COMDT_RANK_PASTSEASON_FIGHT_RECORD b)
        {
            if (a.dwSeaEndTime > b.dwSeaEndTime)
            {
                return -1;
            }
            if (a.dwSeaEndTime < b.dwSeaEndTime)
            {
                return 1;
            }
            return 0;
        }

        public static int ConvertEloToRank(uint elo)
        {
            int count = GameDataMgr.rankGradeDatabin.count;
            int num2 = (int) (elo / 0x10);
            int num3 = count;
            for (int i = 1; i <= count; i++)
            {
                ResRankGradeConf dataByKey = GameDataMgr.rankGradeDatabin.GetDataByKey(i);
                num2 -= (int) dataByKey.dwGradeUpNeedScore;
                if (num2 <= 0)
                {
                    return i;
                }
            }
            return num3;
        }

        public static int GetCurXingByEloAndRankLv(uint elo, uint lv)
        {
            int count = GameDataMgr.rankGradeDatabin.count;
            int num2 = (int) (elo / 0x10);
            if (lv >= count)
            {
                return ((num2 < 0xa3) ? 0 : (num2 - 0xa3));
            }
            for (int i = 1; i < count; i++)
            {
                ResRankGradeConf dataByKey = GameDataMgr.rankGradeDatabin.GetDataByKey(i);
                if (num2 <= dataByKey.dwGradeUpNeedScore)
                {
                    return num2;
                }
                num2 -= (int) dataByKey.dwGradeUpNeedScore;
            }
            return num2;
        }

        private void GetRankData()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0xa32);
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        [MessageHandler(0xa33)]
        public static void GetRankInfoRsp(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            Singleton<CLadderSystem>.GetInstance().OpenLadderEntry();
        }

        public override void Init()
        {
            base.Init();
            REQ_PLAYER_LEVEL = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 100).dwConfValue;
            REQ_HERO_NUM = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x65).dwConfValue;
            MAX_RANK_LEVEL = GameDataMgr.rankGradeDatabin.Count();
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Matching_OpenLadder, new CUIEventManager.OnUIEventHandler(this.OnOpenLadder));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ladder_StartMatching, new CUIEventManager.OnUIEventHandler(this.OnLadder_BeginMatch));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ladder_ShowHistory, new CUIEventManager.OnUIEventHandler(this.OnLadder_ShowHistory));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ladder_ShowRecent, new CUIEventManager.OnUIEventHandler(this.OnLadder_ShowRecent));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ladder_ShowRules, new CUIEventManager.OnUIEventHandler(this.OnLadder_ShowRules));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ladder_ExpandHistoryItem, new CUIEventManager.OnUIEventHandler(this.OnLadder_ExpandHistoryItem));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ladder_ShrinkHistoryItem, new CUIEventManager.OnUIEventHandler(this.OnLadder_ShrinkHistoryItem));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ladder_ConfirmSeasonRank, new CUIEventManager.OnUIEventHandler(this.OnLadder_ConfirmSeasonRank));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ladder_ReqGetSeasonReward, new CUIEventManager.OnUIEventHandler(this.OnLadder_ReqGetSeasonReward));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ladder_GetSeasonRewardDone, new CUIEventManager.OnUIEventHandler(this.OnLadder_GetSeasonRewardDone));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Matching_ADButton, new CUIEventManager.OnUIEventHandler(this.OnMatching_ADButton));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Matching_ADForm_Close, new CUIEventManager.OnUIEventHandler(this.OnMatching_ADForm_Close));
        }

        public static bool IsInSeason()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                DateTime time = Utility.ToUtcTime2Local((long) masterRoleInfo.getCurrentTimeSinceLogin());
                object[] args = new object[] { time.Year.ToString("0000"), time.Month.ToString("00"), time.Day.ToString("00"), time.Hour.ToString("00"), time.Minute.ToString("00"), time.Second.ToString("00") };
                ulong num = ulong.Parse(string.Format("{0}{1}{2}{3}{4}{5}", args));
                for (int i = 0; i < GameDataMgr.rankSeasonDatabin.Count(); i++)
                {
                    ResRankSeasonConf dataByIndex = GameDataMgr.rankSeasonDatabin.GetDataByIndex(i);
                    if ((num >= dataByIndex.ullStartTime) && (num <= dataByIndex.ullEndTime))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsLevelQualified()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            return ((masterRoleInfo != null) && (masterRoleInfo.PvpLevel >= REQ_PLAYER_LEVEL));
        }

        public bool IsQualified()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            return ((masterRoleInfo != null) && ((masterRoleInfo.PvpLevel >= REQ_PLAYER_LEVEL) && (masterRoleInfo.GetHaveHeroCount(false) >= REQ_HERO_NUM)));
        }

        private void OnLadder_BeginMatch(CUIEvent uiEvent)
        {
            Button button = (uiEvent.m_srcWidget == null) ? null : uiEvent.m_srcWidget.GetComponent<Button>();
            if ((button != null) && button.interactable)
            {
                if (Singleton<CMatchingSystem>.GetInstance().IsInMatching)
                {
                    Singleton<CUIManager>.GetInstance().OpenTips("PVP_Matching", true, 1f, null, new object[0]);
                }
                else
                {
                    this.BeginMatch();
                }
            }
        }

        private void OnLadder_ConfirmSeasonRank(CUIEvent uiEvent)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                CLadderView.ShowRankReward(masterRoleInfo.m_HighestGradeOfRank);
            }
        }

        private void OnLadder_ExpandHistoryItem(CUIEvent uiEvent)
        {
            if ((uiEvent.m_srcWidget != null) && (uiEvent.m_srcWidget.transform.parent != null))
            {
                Transform parent = uiEvent.m_srcWidget.transform.parent.parent;
                if (parent != null)
                {
                    CLadderView.OnHistoryItemChange(parent.gameObject, true);
                    CUIEventScript component = uiEvent.m_srcWidget.GetComponent<CUIEventScript>();
                    if (component != null)
                    {
                        component.m_onClickEventID = enUIEventID.Ladder_ShrinkHistoryItem;
                    }
                }
            }
        }

        private void OnLadder_GetSeasonRewardDone(CUIEvent uiEvent)
        {
            Singleton<CUIManager>.GetInstance().CloseForm(FORM_LADDER_REWARD);
        }

        private void OnLadder_ReqGetSeasonReward(CUIEvent uiEvent)
        {
            this.ReqGetSeasonReward();
        }

        private void OnLadder_ShowHistory(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().OpenForm(FORM_LADDER_HISTORY, false, true);
            if (form != null)
            {
                CLadderView.InitLadderHistory(form, this.historySeasonData);
            }
        }

        private void OnLadder_ShowRecent(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().OpenForm(FORM_LADDER_RECENT, false, true);
            if (form != null)
            {
                CLadderView.InitLadderRecent(form, this.currentSeasonGames);
            }
        }

        private void OnLadder_ShowRules(CUIEvent uiEvent)
        {
            ResRuleText dataByKey = GameDataMgr.s_ruleTextDatabin.GetDataByKey(1);
            if (dataByKey != null)
            {
                string title = StringHelper.UTF8BytesToString(ref dataByKey.szTitle);
                string info = StringHelper.UTF8BytesToString(ref dataByKey.szContent);
                Singleton<CUIManager>.GetInstance().OpenInfoForm(title, info);
            }
        }

        private void OnLadder_ShrinkHistoryItem(CUIEvent uiEvent)
        {
            if ((uiEvent.m_srcWidget != null) && (uiEvent.m_srcWidget.transform.parent != null))
            {
                Transform parent = uiEvent.m_srcWidget.transform.parent.parent;
                if (parent != null)
                {
                    CLadderView.OnHistoryItemChange(parent.gameObject, false);
                    CUIEventScript component = uiEvent.m_srcWidget.GetComponent<CUIEventScript>();
                    if (component != null)
                    {
                        component.m_onClickEventID = enUIEventID.Ladder_ExpandHistoryItem;
                    }
                }
            }
        }

        private void OnMatching_ADButton(CUIEvent uiEvent)
        {
            CUIFormScript formScript = Singleton<CUIManager>.GetInstance().OpenForm(FORM_AD_PREFAB, false, true);
            Image component = formScript.transform.Find("Panel/Image").GetComponent<Image>();
            component.SetSprite(image_name, formScript, true, false, false);
            component.gameObject.CustomSetActive(true);
        }

        private void OnMatching_ADForm_Close(CUIEvent uiEvent)
        {
            Singleton<CUIManager>.GetInstance().CloseForm(FORM_AD_PREFAB);
        }

        [MessageHandler(0x1455)]
        public static void OnMatchTeamDestroyNtf(CSPkg msg)
        {
            byte bReason = msg.stPkgData.stMatchTeamDestroyNtf.stDetail.bReason;
            if (bReason == 20)
            {
                Singleton<CUIManager>.instance.OpenTips("Err_Invite_Result_3", true, 1f, null, new object[0]);
                Singleton<EventRouter>.instance.BroadCastEvent<byte, string>(EventID.INVITE_ERRCODE_NTF, bReason, string.Empty);
            }
            else
            {
                Singleton<CUIManager>.instance.OpenTips(string.Format("CSProtocolMacros.SCID_MATCHTEAM_DESTROY_NTF bReason = {0} ", bReason), false, 1f, null, new object[0]);
            }
        }

        private void OnOpenLadder(CUIEvent uiEvent)
        {
            Button button = (uiEvent.m_srcWidget == null) ? null : uiEvent.m_srcWidget.GetComponent<Button>();
            if (button != null)
            {
                if (button.interactable)
                {
                    Singleton<CNewbieAchieveSys>.GetInstance().trackFlag = CNewbieAchieveSys.TrackFlag.None;
                    this.GetRankData();
                }
                else if (!this.IsLevelQualified())
                {
                    object[] replaceArr = new object[] { REQ_PLAYER_LEVEL };
                    Singleton<CUIManager>.GetInstance().OpenTips("Activity_Open", true, 1f, null, replaceArr);
                }
            }
            else if (!this.IsLevelQualified())
            {
                object[] objArray2 = new object[] { REQ_PLAYER_LEVEL };
                Singleton<CUIManager>.GetInstance().OpenTips("Activity_Open", true, 1f, null, objArray2);
            }
            else
            {
                this.GetRankData();
            }
        }

        [MessageHandler(0xb64)]
        public static void OnReceiveRankSeasonReward(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            if (msg.stPkgData.stGetRankRewardRsp.bErrCode == 0)
            {
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                if ((masterRoleInfo != null) && (Singleton<CLadderSystem>.GetInstance().currentRankDetail != null))
                {
                    Singleton<SettlementSystem>.GetInstance().SetLadderDisplayOldAndNewGrade(1, 1, masterRoleInfo.m_HighestGradeOfRank, Singleton<CLadderSystem>.GetInstance().currentRankDetail.dwScore);
                    Singleton<SettlementSystem>.GetInstance().ShowLadderSettleFormWithoutSettle();
                }
                Singleton<CLadderSystem>.GetInstance().currentRankDetail.bGetReward = 1;
            }
            else
            {
                string strContent = string.Empty;
                if (msg.stPkgData.stGetRankRewardRsp.bErrCode == 1)
                {
                    strContent = Singleton<CTextManager>.GetInstance().GetText("GETRANKREWARD_ERR_STATE_INVALID");
                }
                else if (msg.stPkgData.stGetRankRewardRsp.bErrCode == 2)
                {
                    strContent = Singleton<CTextManager>.GetInstance().GetText("GETRANKREWARD_ERR_STATE_HaveGet");
                }
                else if (msg.stPkgData.stGetRankRewardRsp.bErrCode == 3)
                {
                    strContent = Singleton<CTextManager>.GetInstance().GetText("GETRANKREWARD_ERR_STATE_Others");
                }
                else
                {
                    strContent = Singleton<CTextManager>.GetInstance().GetText("GETRANKREWARD_ERR_STATE_None");
                }
                Singleton<CUIManager>.GetInstance().OpenMessageBox(strContent, false);
            }
        }

        private void OpenLadderEntry()
        {
            bool flag = ((this.currentRankDetail != null) && (this.currentRankDetail.bState == 2)) && (this.currentRankDetail.bGetReward == 0);
            if (!this.CanOpenLadderEntry())
            {
                if (flag)
                {
                    CLadderView.InitRewardForm(Singleton<CUIManager>.GetInstance().OpenForm(FORM_LADDER_REWARD, false, true), ref this.currentRankDetail);
                }
            }
            else
            {
                CUIFormScript form = Singleton<CUIManager>.GetInstance().OpenForm(FORM_LADDER_ENTRY, false, true);
                if (form != null)
                {
                    CLadderView.InitLadderEntry(form, ref this.currentRankDetail, this.IsQualified());
                    CLadderView.SetMostRecentGameData(form, ref this.currentRankDetail, this.currentSeasonGames);
                    if (flag)
                    {
                        CLadderView.InitRewardForm(Singleton<CUIManager>.GetInstance().OpenForm(FORM_LADDER_REWARD, false, true), ref this.currentRankDetail);
                    }
                }
            }
        }

        [MessageHandler(0xb62)]
        public static void ReceiveRankHistoryInfo(CSPkg msg)
        {
            if (Singleton<CLadderSystem>.GetInstance().historySeasonData == null)
            {
                Singleton<CLadderSystem>.GetInstance().historySeasonData = new List<COMDT_RANK_PASTSEASON_FIGHT_RECORD>();
            }
            Singleton<CLadderSystem>.GetInstance().historySeasonData.Clear();
            for (int i = 0; i < msg.stPkgData.stRankPastSeasonHistory.bNum; i++)
            {
                Singleton<CLadderSystem>.GetInstance().historySeasonData.Add(msg.stPkgData.stRankPastSeasonHistory.astRecord[i]);
            }
            Singleton<CLadderSystem>.GetInstance().historySeasonData.Sort(new Comparison<COMDT_RANK_PASTSEASON_FIGHT_RECORD>(CLadderSystem.ComparisonHistoryData));
        }

        [MessageHandler(0xa29)]
        public static void ReceiveRankInfo(CSPkg msg)
        {
            SCPKG_UPDRANKINFO_NTF stUpdateRankInfo = msg.stPkgData.stUpdateRankInfo;
            Singleton<CLadderSystem>.GetInstance().UpdateRankInfo(ref stUpdateRankInfo);
        }

        [MessageHandler(0xb61)]
        public static void ReceiveRankSeasonInfo(CSPkg msg)
        {
            if (Singleton<CLadderSystem>.GetInstance().currentSeasonGames == null)
            {
                Singleton<CLadderSystem>.GetInstance().currentSeasonGames = new List<COMDT_RANK_CURSEASON_FIGHT_RECORD>();
            }
            Singleton<CLadderSystem>.GetInstance().currentSeasonGames.Clear();
            for (int i = 0; i < msg.stPkgData.stRankCurSeasonHistory.bNum; i++)
            {
                Singleton<CLadderSystem>.GetInstance().currentSeasonGames.Add(msg.stPkgData.stRankCurSeasonHistory.astRecord[i]);
            }
            Singleton<CLadderSystem>.GetInstance().currentSeasonGames.Sort(new Comparison<COMDT_RANK_CURSEASON_FIGHT_RECORD>(CLadderSystem.ComparisonGameData));
        }

        private void ReqGetSeasonReward()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0xb63);
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public override void UnInit()
        {
            this.currentRankDetail = null;
            if (this.currentSeasonGames != null)
            {
                this.currentSeasonGames.Clear();
                this.currentSeasonGames = null;
            }
            if (this.historySeasonData != null)
            {
                this.historySeasonData.Clear();
                this.historySeasonData = null;
            }
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Matching_OpenLadder, new CUIEventManager.OnUIEventHandler(this.OnOpenLadder));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ladder_StartMatching, new CUIEventManager.OnUIEventHandler(this.OnLadder_BeginMatch));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ladder_ShowHistory, new CUIEventManager.OnUIEventHandler(this.OnLadder_ShowHistory));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ladder_ShowRecent, new CUIEventManager.OnUIEventHandler(this.OnLadder_ShowRecent));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ladder_ShowRules, new CUIEventManager.OnUIEventHandler(this.OnLadder_ShowRules));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ladder_ExpandHistoryItem, new CUIEventManager.OnUIEventHandler(this.OnLadder_ExpandHistoryItem));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ladder_ShrinkHistoryItem, new CUIEventManager.OnUIEventHandler(this.OnLadder_ShrinkHistoryItem));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ladder_ConfirmSeasonRank, new CUIEventManager.OnUIEventHandler(this.OnLadder_ConfirmSeasonRank));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ladder_ReqGetSeasonReward, new CUIEventManager.OnUIEventHandler(this.OnLadder_ReqGetSeasonReward));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ladder_GetSeasonRewardDone, new CUIEventManager.OnUIEventHandler(this.OnLadder_GetSeasonRewardDone));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Matching_ADButton, new CUIEventManager.OnUIEventHandler(this.OnMatching_ADButton));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Matching_ADForm_Close, new CUIEventManager.OnUIEventHandler(this.OnMatching_ADForm_Close));
            base.UnInit();
        }

        private void UpdateRankInfo(ref SCPKG_UPDRANKINFO_NTF newData)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                masterRoleInfo.m_gradeOfRank = newData.bCurGrade;
                masterRoleInfo.m_HighestGradeOfRank = newData.stRankInfo.bMaxSeasonGrade;
            }
            this.currentRankDetail = newData.stRankInfo;
            bool flag = (this.currentRankDetail.bState == 2) && (this.currentRankDetail.bGetReward == 0);
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(FORM_LADDER_ENTRY);
            if (form != null)
            {
                CLadderView.InitLadderEntry(form, ref this.currentRankDetail, this.IsQualified());
                if (flag)
                {
                    CLadderView.InitRewardForm(Singleton<CUIManager>.GetInstance().OpenForm(FORM_LADDER_REWARD, false, true), ref this.currentRankDetail);
                }
            }
        }

        public uint Ranking
        {
            set
            {
                this._ranking = value;
            }
        }
    }
}

