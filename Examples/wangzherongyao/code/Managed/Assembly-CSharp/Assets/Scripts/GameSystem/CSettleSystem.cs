namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    [MessageHandlerClass]
    internal class CSettleSystem : Singleton<CSettleSystem>
    {
        private GameObject _cacheLastReportGo;
        private bool _changingGrage;
        private uint _curDian = 1;
        private uint _curGrade = 1;
        private uint _curMaxScore = 3;
        private bool _doWangZheSpecial;
        private bool _isDown;
        private bool _isSettlementContinue;
        private bool _isUp;
        private Animator _ladderAnimator;
        private CUIFormScript _ladderForm;
        private GameObject _ladderRoot;
        private bool _lastWin;
        private uint _newGrade = 1;
        private uint _newMaxScore = 3;
        private uint _newScore = 1;
        private uint _oldGrade = 1;
        private uint _oldMaxScore = 3;
        private uint _oldScore = 1;
        private ulong _reportUid;
        private int _reportWordId;
        public uint maxRankPoint;
        public static string PATH_LADDER_SETTLE = string.Format("{0}{1}", "UGUI/Form/System/", "PvP/Settlement/Form_LadderSettle");
        public static string PATH_PVP_SETTLE_ACHIEVEMENT = "UGUI/Form/System/PvP/Settlement/Form_SettleAchievement";
        public static string PATH_PVP_SETTLE_PVP = "UGUI/Form/System/PvP/Settlement/Form_PVPSettle";

        private void ClosePvPSettleForm(bool isCloseForm = true)
        {
            Singleton<CUIManager>.GetInstance().CloseForm(PATH_PVP_SETTLE_PVP);
            MonoSingleton<ShareSys>.instance.m_bShowTimeline = false;
        }

        public void DianXing()
        {
            if (this.NeedDianXing())
            {
                uint num = this.NeedChangeGrade();
                if ((num > 0) && !this._changingGrage)
                {
                    this._changingGrage = true;
                    this._curMaxScore = num;
                    if (this._isUp)
                    {
                        this._curGrade++;
                        this._curDian = 0;
                    }
                    else
                    {
                        this._curGrade--;
                        this._curDian = this._curMaxScore;
                    }
                    if (this._isUp)
                    {
                        this.Ladder_PlayLevelUpStart();
                    }
                    else
                    {
                        this.Ladder_PlayLevelDownStart();
                    }
                }
                else if (!this._changingGrage)
                {
                    if (this._isUp)
                    {
                        this._curDian++;
                        this.PlayXingAnim(this._curDian, this._curMaxScore, false);
                    }
                    else
                    {
                        this.PlayXingAnim(this._curDian, this._curMaxScore, this._isDown);
                    }
                }
            }
            else if (((!this._doWangZheSpecial && (this._oldGrade == this._newGrade)) && ((this._newGrade == GameDataMgr.rankGradeDatabin.count) && (this._oldScore == this._newScore))) && (this._newScore == 0))
            {
                this._doWangZheSpecial = true;
                this.PlayXingAnim(this._curDian, this._curMaxScore, this._isDown);
            }
            else
            {
                this._doWangZheSpecial = false;
                this.LadderAllDisplayEnd();
            }
        }

        private GameObject GetXing(uint targetScore, uint targetMax)
        {
            if (this._ladderRoot == null)
            {
                return null;
            }
            Transform transform = this._ladderRoot.transform.FindChild(string.Format("RankConNow/ScoreCon/Con{0}Star", targetMax));
            if (transform == null)
            {
                return null;
            }
            GameObject gameObject = transform.gameObject;
            if (gameObject == null)
            {
                return null;
            }
            Transform transform2 = gameObject.transform.FindChild(string.Format("Xing{0}", targetScore));
            return ((transform2 == null) ? null : transform2.gameObject);
        }

        public override void Init()
        {
            base.Init();
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_Confirm, new CUIEventManager.OnUIEventHandler(this.OnSettle_Confirm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_Again, new CUIEventManager.OnUIEventHandler(this.OnSettle_Again));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_ShowStatistics, new CUIEventManager.OnUIEventHandler(this.OnSettle_ShowStatistics));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_ShowData, new CUIEventManager.OnUIEventHandler(this.OnSettle_ShowData));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_HideData, new CUIEventManager.OnUIEventHandler(this.OnSettle_HideData));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_AddFriend, new CUIEventManager.OnUIEventHandler(this.OnSettle_AddFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_ShowAchievementTips, new CUIEventManager.OnUIEventHandler(this.ShowAchievementTips));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_ShowRankPointTips, new CUIEventManager.OnUIEventHandler(this.OnSettle_ShowRankPointTips));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_HideRankPointTips, new CUIEventManager.OnUIEventHandler(this.OnSettle_HideRankPointTips));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_ShowReportPlayer, new CUIEventManager.OnUIEventHandler(this.OnShowReport));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_CloseReportPlayer, new CUIEventManager.OnUIEventHandler(this.OnCloseReport));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_DoReporting, new CUIEventManager.OnUIEventHandler(this.OnDoReport));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_LadderSettleClickContinue, new CUIEventManager.OnUIEventHandler(this.OnLadderClickContinue));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_LadderSettleAgain, new CUIEventManager.OnUIEventHandler(this.OnLadderSettleAgain));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.PvPSettle_OnChangeStateTimerEnd, new CUIEventManager.OnUIEventHandler(this.OnPvPSettle_OnChangeStateTimerEnd));
        }

        protected bool IsLianSheng()
        {
            return (this._isUp && (((this._newGrade > this._oldGrade) && (((this._oldMaxScore - this._oldScore) + this._newScore) > 1)) || ((this._newGrade == this._oldGrade) && ((this._newScore - this._oldScore) > 1))));
        }

        public void Ladder_PlayLevelDownEnd()
        {
            if (this._ladderAnimator != null)
            {
                this._ladderAnimator.Play("Base Layer.RankConNow_LevelDownEnd");
                Singleton<CSoundManager>.GetInstance().PostEvent("UI_paiwei_jiangji", null);
            }
        }

        public void Ladder_PlayLevelDownStart()
        {
            if (this._ladderAnimator != null)
            {
                this._ladderAnimator.Play("Base Layer.RankConNow_LevelDownStart");
            }
        }

        public void Ladder_PlayLevelUpEnd()
        {
            if (this._ladderAnimator != null)
            {
                this._ladderAnimator.Play("Base Layer.RankConNow_LevelUpEnd");
                Singleton<CSoundManager>.GetInstance().PostEvent("UI_paiwei_shengji", null);
            }
        }

        public void Ladder_PlayLevelUpStart()
        {
            if (this._ladderAnimator != null)
            {
                this._ladderAnimator.Play("Base Layer.RankConNow_LevelUpStart");
            }
        }

        public void Ladder_PlayShowIn()
        {
            if (this._ladderAnimator != null)
            {
                this._ladderAnimator.Play("Base Layer.RankConNow_ShowIn");
            }
        }

        private void LadderAllDisplayEnd()
        {
            this._ladderForm.gameObject.transform.FindChild("Btn_Continue").gameObject.CustomSetActive(true);
        }

        protected void LadderDisplayProcess(CUIFormScript form, bool showLianSheng = false)
        {
            CLadderView.ShowRankDetail(this._ladderRoot.transform.FindChild("RankConNow").gameObject, (byte) this._oldGrade);
            if (showLianSheng && this.IsLianSheng())
            {
                this._ladderRoot.transform.FindChild("LianShengTxt").gameObject.CustomSetActive(true);
            }
            this.ResetAllXing(this._curDian, this._curMaxScore, false);
            this.Ladder_PlayShowIn();
        }

        public uint NeedChangeGrade()
        {
            uint dwGradeUpNeedScore = 0;
            if (this._isUp && this.NeedDianXing())
            {
                if (this._curDian == GameDataMgr.rankGradeDatabin.GetDataByKey(this._curGrade).dwGradeUpNeedScore)
                {
                    dwGradeUpNeedScore = GameDataMgr.rankGradeDatabin.GetDataByKey((uint) (this._curGrade + 1)).dwGradeUpNeedScore;
                }
                return dwGradeUpNeedScore;
            }
            if ((this._isDown && this.NeedDianXing()) && (this._curDian == 0))
            {
                dwGradeUpNeedScore = GameDataMgr.rankGradeDatabin.GetDataByKey((uint) (this._curGrade - 1)).dwGradeUpNeedScore;
            }
            return dwGradeUpNeedScore;
        }

        public bool NeedDianXing()
        {
            if (this._isUp)
            {
                return ((this._curGrade < this._newGrade) || ((this._curGrade == this._newGrade) && (this._curDian < this._newScore)));
            }
            return ((this._curGrade > this._newGrade) || ((this._curGrade == this._newGrade) && (this._curDian > this._newScore)));
        }

        protected void OnCloseReport(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(PATH_PVP_SETTLE_PVP);
            if ((form != null) && (form.gameObject != null))
            {
                this._cacheLastReportGo = null;
                this._reportUid = 0L;
                this._reportWordId = 0;
                form.gameObject.transform.Find("Report").gameObject.CustomSetActive(false);
            }
        }

        protected void OnDoReport(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(PATH_PVP_SETTLE_PVP);
            if ((form != null) && (form.gameObject != null))
            {
                GameObject gameObject = form.gameObject.transform.Find("Report").gameObject;
                gameObject.CustomSetActive(false);
                Singleton<CUIManager>.instance.OpenTips(Singleton<CTextManager>.GetInstance().GetText("Report_Report"), false, 1f, null, new object[0]);
                CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x10d1);
                msg.stPkgData.stUserComplaintReq.dwComplaintReason = 1;
                msg.stPkgData.stUserComplaintReq.ullComplaintUserUid = this._reportUid;
                msg.stPkgData.stUserComplaintReq.iComplaintLogicWorldID = this._reportWordId;
                GameObject obj3 = gameObject.transform.FindChild("ReportToggle").gameObject;
                if (obj3.transform.FindChild("ReportGuaJi").gameObject.GetComponent<Toggle>().isOn)
                {
                    msg.stPkgData.stUserComplaintReq.dwComplaintReason = 1;
                }
                else if (obj3.transform.FindChild("ReportSong").gameObject.GetComponent<Toggle>().isOn)
                {
                    msg.stPkgData.stUserComplaintReq.dwComplaintReason = 2;
                }
                else if (obj3.transform.FindChild("ReportXiaoJi").gameObject.GetComponent<Toggle>().isOn)
                {
                    msg.stPkgData.stUserComplaintReq.dwComplaintReason = 3;
                }
                else if (obj3.transform.FindChild("ReportDiaoXian").gameObject.GetComponent<Toggle>().isOn)
                {
                    msg.stPkgData.stUserComplaintReq.dwComplaintReason = 4;
                }
                else if (obj3.transform.FindChild("ReportCai").gameObject.GetComponent<Toggle>().isOn)
                {
                    msg.stPkgData.stUserComplaintReq.dwComplaintReason = 5;
                }
                else if (obj3.transform.FindChild("ReportGua").gameObject.GetComponent<Toggle>().isOn)
                {
                    msg.stPkgData.stUserComplaintReq.dwComplaintReason = 6;
                }
                Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
                this._reportUid = 0L;
                this._reportWordId = 0;
                if (this._cacheLastReportGo != null)
                {
                    this._cacheLastReportGo.CustomSetActive(false);
                    this._cacheLastReportGo = null;
                }
            }
        }

        protected void OnLadderClickContinue(CUIEvent uiEvent)
        {
            Singleton<CUIManager>.GetInstance().CloseForm(PATH_LADDER_SETTLE);
            this._ladderForm = null;
            this._ladderAnimator = null;
            this._ladderRoot = null;
            if (this._isSettlementContinue)
            {
                this.ShowPvpSettleForm(this._lastWin);
                this._lastWin = false;
            }
        }

        public void OnLadderLevelDownEndOver()
        {
            this._changingGrage = false;
            this.DianXing();
        }

        public void OnLadderLevelDownStartOver()
        {
            this.ResetAllXing(this._curMaxScore, this._curMaxScore, true);
            CLadderView.ShowRankDetail(this._ladderRoot.transform.FindChild("RankConNow").gameObject, (byte) this._curGrade);
            this.Ladder_PlayLevelDownEnd();
        }

        public void OnLadderLevelUpEndOver()
        {
            this._changingGrage = false;
            this.DianXing();
        }

        public void OnLadderLevelUpStartOver()
        {
            this.ResetAllXing(0, this._curMaxScore, false);
            CLadderView.ShowRankDetail(this._ladderRoot.transform.FindChild("RankConNow").gameObject, (byte) this._curGrade);
            this.Ladder_PlayLevelUpEnd();
        }

        protected void OnLadderSettleAgain(CUIEvent uiEvent)
        {
            this.ClosePvPSettleForm(true);
            CUIEvent event2 = new CUIEvent {
                m_eventID = enUIEventID.Matching_OpenLadder
            };
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(event2);
        }

        public void OnLadderShowInOver()
        {
            this.DianXing();
        }

        public void OnLadderWangZheXingEndOver()
        {
            this._ladderRoot.transform.FindChild("RankConNow/WangZheXing").gameObject.GetComponent<Animator>().enabled = false;
            this.DianXing();
        }

        public void OnLadderWangZheXingStartOver()
        {
        }

        public void OnLadderXingDownOver()
        {
            GameObject xing = this.GetXing(this._curDian, this._curMaxScore);
            if (xing != null)
            {
                xing.CustomSetActive(true);
                Animator component = xing.GetComponent<Animator>();
                if (component == null)
                {
                    return;
                }
                component.enabled = false;
            }
            this._curDian--;
            this.DianXing();
        }

        public void OnLadderXingUpOver()
        {
            GameObject xing = this.GetXing(this._curDian, this._curMaxScore);
            if (xing != null)
            {
                xing.CustomSetActive(true);
                Animator component = xing.GetComponent<Animator>();
                if (component == null)
                {
                    return;
                }
                component.enabled = false;
            }
            this.DianXing();
        }

        private void OnPvPSettle_OnChangeStateTimerEnd(CUIEvent uiEvent)
        {
            Singleton<GameBuilder>.instance.EndGame();
            Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharShow");
            Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharIcon");
            Transform transform = uiEvent.m_srcFormScript.gameObject.transform;
            Transform transform2 = transform.Find("PanelB/ButtonGrid");
            if (transform2 != null)
            {
                transform2.gameObject.CustomSetActive(true);
            }
            Transform transform3 = transform.Find("PanelB/lblWait");
            if (transform3 != null)
            {
                transform3.gameObject.CustomSetActive(false);
            }
        }

        private void OnSettle_AddFriend(CUIEvent uiEvent)
        {
            FriendSysNetCore.Send_Request_BeFriend(uiEvent.m_eventParams.commonUInt64Param1, (uint) uiEvent.m_eventParams.commonUInt64Param2);
            uiEvent.m_srcWidget.CustomSetActive(false);
        }

        private void OnSettle_Again(CUIEvent uiEvent)
        {
            this.ClosePvPSettleForm(true);
            if ((Singleton<CMatchingSystem>.instance.cacheMathingInfo != null) && (Singleton<CMatchingSystem>.instance.cacheMathingInfo.uiEventId != enUIEventID.None))
            {
                if (Singleton<CMatchingSystem>.instance.cacheMathingInfo.uiEventId == enUIEventID.Room_CreateRoom)
                {
                    CRoomSystem.ReqCreateRoom(Singleton<CMatchingSystem>.instance.cacheMathingInfo.mapId);
                }
                else
                {
                    CUIEvent event2 = new CUIEvent {
                        m_eventID = Singleton<CMatchingSystem>.instance.cacheMathingInfo.uiEventId
                    };
                    event2.m_eventParams.tagUInt = Singleton<CMatchingSystem>.instance.cacheMathingInfo.mapId;
                    Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(event2);
                }
            }
        }

        private void OnSettle_Confirm(CUIEvent uiEvent)
        {
            this.ClosePvPSettleForm(true);
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Matching_OpenEntry);
        }

        private void OnSettle_HideData(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(PATH_PVP_SETTLE_PVP);
            if ((form != null) && (form.gameObject != null))
            {
                CSettlementView.HideData(form);
            }
        }

        private void OnSettle_HideRankPointTips(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(PATH_PVP_SETTLE_PVP);
            if (form != null)
            {
                CSettlementView.SetRankPointTips(form.gameObject, false, 0);
            }
        }

        private void OnSettle_ShowData(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(PATH_PVP_SETTLE_PVP);
            if ((form != null) && (form.gameObject != null))
            {
                CSettlementView.ShowData(form);
            }
        }

        private void OnSettle_ShowRankPointTips(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(PATH_PVP_SETTLE_PVP);
            if (form != null)
            {
                CSettlementView.SetRankPointTips(form.gameObject, true, uiEvent.m_eventParams.tag);
            }
        }

        private void OnSettle_ShowStatistics(CUIEvent uiEvent)
        {
            CSettlementView.DoCoinTweenEnd();
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(PATH_PVP_SETTLE_PVP);
            if (form != null)
            {
                if ((form != null) && (form.gameObject != null))
                {
                    CSettlementView.SetTab(1, form.gameObject);
                }
                Transform transform = form.gameObject.transform.Find("PanelB/ButtonGrid");
                if (transform != null)
                {
                    transform.gameObject.CustomSetActive(false);
                }
                Transform transform2 = form.gameObject.transform.Find("PanelB/StateChangeTimer");
                if (transform2 != null)
                {
                    CUITimerScript component = transform2.GetComponent<CUITimerScript>();
                    if (component != null)
                    {
                        component.StartTimer();
                    }
                }
                Transform transform3 = form.gameObject.transform.Find("PanelB/lblWait");
                if (transform3 != null)
                {
                    transform3.gameObject.CustomSetActive(true);
                }
            }
        }

        protected void OnShowReport(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(PATH_PVP_SETTLE_PVP);
            if ((form != null) && (form.gameObject != null))
            {
                this._cacheLastReportGo = uiEvent.m_srcWidget;
                GameObject gameObject = form.gameObject.transform.Find("Report").gameObject;
                gameObject.CustomSetActive(true);
                this._reportUid = uiEvent.m_eventParams.commonUInt64Param1;
                this._reportWordId = (int) uiEvent.m_eventParams.commonUInt64Param2;
                CPlayerKDAStat playerKDAStat = Singleton<BattleLogic>.GetInstance().battleStat.m_playerKDAStat;
                string playerName = null;
                if (playerKDAStat != null)
                {
                    DictionaryView<uint, PlayerKDA>.Enumerator enumerator = playerKDAStat.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                        if (current.Value.PlayerUid == this._reportUid)
                        {
                            KeyValuePair<uint, PlayerKDA> pair2 = enumerator.Current;
                            if (pair2.Value.WorldId == this._reportWordId)
                            {
                                KeyValuePair<uint, PlayerKDA> pair3 = enumerator.Current;
                                playerName = pair3.Value.PlayerName;
                                break;
                            }
                        }
                    }
                }
                gameObject.transform.FindChild("ReportToggle/ReportName").gameObject.GetComponent<Text>().text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Report_PlayerName"), playerName);
            }
        }

        public void OpenSettle(bool bWin)
        {
            MonoSingleton<ShareSys>.GetInstance().m_bWinPVPResult = bWin;
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            DebugHelper.Assert(curLvelContext != null);
            DebugHelper.Assert(curLvelContext.isPVPLevel);
            if (curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER)
            {
                this.ShowLadderSettleForm(bWin);
            }
            else
            {
                this.ShowPvpSettleForm(bWin);
            }
        }

        private void PlayXingAnim(uint targetScore, uint targetMax, bool disappear = false)
        {
            if (this._ladderRoot != null)
            {
                GameObject xing = this.GetXing(targetScore, targetMax);
                if ((xing == null) && (targetMax > 5))
                {
                    if (disappear && (this._curDian > 0))
                    {
                        this._curDian--;
                    }
                    GameObject gameObject = this._ladderRoot.transform.FindChild("RankConNow/WangZheXing").gameObject;
                    gameObject.CustomSetActive(true);
                    gameObject.transform.FindChild("XingNumTxt").gameObject.GetComponent<Text>().text = string.Format("X{0}", this._curDian);
                    Animator component = gameObject.GetComponent<Animator>();
                    component.enabled = true;
                    component.Play("Base Layer.wangzhe_starend");
                    Singleton<CSoundManager>.GetInstance().PostEvent(!disappear ? "UI_paiwei_dexing" : "UI_paiwei_diuxing", null);
                }
                else if (xing != null)
                {
                    xing.CustomSetActive(true);
                    Animator animator2 = xing.GetComponent<Animator>();
                    if (animator2 != null)
                    {
                        animator2.enabled = true;
                        xing.transform.FindChild("LiangXing").gameObject.CustomSetActive(true);
                        animator2.Play(!disappear ? "Base Layer.Start_ShowIn" : "Base Layer.Start_Disappear");
                        Singleton<CSoundManager>.GetInstance().PostEvent(!disappear ? "UI_paiwei_dexing" : "UI_paiwei_diuxing", null);
                    }
                }
            }
        }

        public void PrepareSettle()
        {
            GuildMemInfo playerGuildMemberInfo = CGuildHelper.GetPlayerGuildMemberInfo();
            if (playerGuildMemberInfo != null)
            {
                this.maxRankPoint = playerGuildMemberInfo.RankInfo.maxRankPoint;
            }
        }

        private void ResetAllXing(uint targetScore, uint targetMax, bool inverseShow = false)
        {
            GameObject gameObject = this._ladderRoot.transform.FindChild(string.Format("RankConNow/ScoreCon/Con{0}Star", 3)).gameObject;
            GameObject obj3 = this._ladderRoot.transform.FindChild(string.Format("RankConNow/ScoreCon/Con{0}Star", 4)).gameObject;
            GameObject obj4 = this._ladderRoot.transform.FindChild(string.Format("RankConNow/ScoreCon/Con{0}Star", 5)).gameObject;
            gameObject.CustomSetActive(false);
            obj3.CustomSetActive(false);
            obj4.CustomSetActive(false);
            GameObject obj5 = this._ladderRoot.transform.FindChild("RankConNow/WangZheXing").gameObject;
            if (targetMax > 5)
            {
                obj5.transform.FindChild("XingNumTxt").gameObject.GetComponent<Text>().text = string.Format("X{0}", this._curDian);
                obj5.gameObject.GetComponent<Animator>().enabled = false;
            }
            else
            {
                obj5.CustomSetActive(false);
            }
            Transform transform = this._ladderRoot.transform.FindChild(string.Format("RankConNow/ScoreCon/Con{0}Star", targetMax));
            if (transform != null)
            {
                GameObject obj6 = transform.gameObject;
                obj6.CustomSetActive(true);
                for (int i = 1; i <= 5; i++)
                {
                    Transform transform2 = obj6.transform.FindChild(string.Format("Xing{0}", i));
                    Transform transform3 = obj6.transform.FindChild(string.Format("Xing{0}/LiangXing", i));
                    if ((transform2 != null) && (transform3 != null))
                    {
                        transform2.gameObject.GetComponent<Animator>().enabled = inverseShow;
                        if (i <= targetScore)
                        {
                            transform3.gameObject.CustomSetActive(true);
                        }
                        else
                        {
                            transform3.gameObject.CustomSetActive(false);
                        }
                    }
                }
            }
        }

        public void SetLadderDisplayOldAndNewGrade(uint oldGrade, uint oldScore, uint newGrade, uint newScore)
        {
            this._oldGrade = Math.Max(oldGrade, 1);
            this._oldScore = oldScore;
            this._oldMaxScore = GameDataMgr.rankGradeDatabin.GetDataByKey(this._oldGrade).dwGradeUpNeedScore;
            this._newGrade = Math.Max(newGrade, 1);
            this._newScore = newScore;
            this._newMaxScore = GameDataMgr.rankGradeDatabin.GetDataByKey(this._oldGrade).dwGradeUpNeedScore;
            this._isUp = false;
            this._isDown = false;
            if ((this._oldGrade < this._newGrade) || ((this._oldGrade == this._newGrade) && (this._oldScore < this._newScore)))
            {
                this._isUp = true;
                this._isDown = false;
            }
            else
            {
                this._isDown = true;
                this._isUp = false;
            }
            this._curDian = this._oldScore;
            this._curGrade = this._oldGrade;
            this._curMaxScore = this._oldMaxScore;
        }

        private void ShowAchievementTips(CUIEvent uiEvent)
        {
            Singleton<CUIManager>.instance.OpenForm(PATH_PVP_SETTLE_ACHIEVEMENT, false, true);
        }

        public void ShowLadderSettleForm(bool win)
        {
            if (Singleton<CUIManager>.GetInstance().GetForm(PATH_LADDER_SETTLE) == null)
            {
                this._ladderForm = Singleton<CUIManager>.GetInstance().OpenForm(PATH_LADDER_SETTLE, false, true);
                this._ladderRoot = this._ladderForm.gameObject.transform.FindChild("Ladder").gameObject;
                this._ladderAnimator = this._ladderRoot.GetComponent<Animator>();
                this._lastWin = win;
                COMDT_RANK_SETTLE_INFO rankInfo = Singleton<BattleStatistic>.GetInstance().rankInfo;
                if (rankInfo != null)
                {
                    this.SetLadderDisplayOldAndNewGrade(rankInfo.bOldGrade, rankInfo.dwOldScore, rankInfo.bNowGrade, rankInfo.dwNowScore);
                }
                this._isSettlementContinue = true;
                this.LadderDisplayProcess(this._ladderForm, true);
            }
        }

        public void ShowLadderSettleFormWithoutSettle()
        {
            if (Singleton<CUIManager>.GetInstance().GetForm(PATH_LADDER_SETTLE) == null)
            {
                this._ladderForm = Singleton<CUIManager>.GetInstance().OpenForm(PATH_LADDER_SETTLE, false, true);
                this._ladderRoot = this._ladderForm.gameObject.transform.FindChild("Ladder").gameObject;
                this._ladderAnimator = this._ladderRoot.GetComponent<Animator>();
                this._isSettlementContinue = false;
                this.LadderDisplayProcess(this._ladderForm, false);
            }
        }

        public void ShowPvpSettleForm(bool win)
        {
            if (Singleton<CUIManager>.GetInstance().GetForm(PATH_PVP_SETTLE_PVP) == null)
            {
                MonoSingleton<ShareSys>.instance.m_bShowTimeline = false;
                CUIFormScript formScript = Singleton<CUIManager>.GetInstance().OpenForm(PATH_PVP_SETTLE_PVP, false, true);
                CSettlementView.SetSettleData(formScript, formScript.gameObject, win);
                COMDT_REWARD_MULTIPLE_DETAIL multiDetail = Singleton<BattleStatistic>.GetInstance().multiDetail;
                if ((multiDetail != null) && (multiDetail.bZeroProfit == 1))
                {
                    Singleton<CUIManager>.instance.OpenTips("ZeroProfit_Tips", true, 1f, null, new object[0]);
                }
                Singleton<EventRouter>.instance.BroadCastEvent(EventID.ADVANCE_STOP_LOADING);
            }
        }

        public override void UnInit()
        {
            base.UnInit();
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_Confirm, new CUIEventManager.OnUIEventHandler(this.OnSettle_Confirm));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_Again, new CUIEventManager.OnUIEventHandler(this.OnSettle_Again));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_ShowStatistics, new CUIEventManager.OnUIEventHandler(this.OnSettle_ShowStatistics));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_ShowData, new CUIEventManager.OnUIEventHandler(this.OnSettle_ShowData));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_HideData, new CUIEventManager.OnUIEventHandler(this.OnSettle_HideData));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_AddFriend, new CUIEventManager.OnUIEventHandler(this.OnSettle_AddFriend));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_ShowAchievementTips, new CUIEventManager.OnUIEventHandler(this.ShowAchievementTips));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_ShowRankPointTips, new CUIEventManager.OnUIEventHandler(this.OnSettle_ShowRankPointTips));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_HideRankPointTips, new CUIEventManager.OnUIEventHandler(this.OnSettle_HideRankPointTips));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_ShowReportPlayer, new CUIEventManager.OnUIEventHandler(this.OnShowReport));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_CloseReportPlayer, new CUIEventManager.OnUIEventHandler(this.OnCloseReport));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_DoReporting, new CUIEventManager.OnUIEventHandler(this.OnDoReport));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_LadderSettleClickContinue, new CUIEventManager.OnUIEventHandler(this.OnLadderClickContinue));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.PvPSettle_LadderSettleAgain, new CUIEventManager.OnUIEventHandler(this.OnLadderSettleAgain));
        }
    }
}

