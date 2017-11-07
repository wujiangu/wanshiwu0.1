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

    [MessageHandlerClass]
    public class RankingSystem : Singleton<RankingSystem>
    {
        private string _allViewName;
        private Animator _animator;
        private Dictionary<stFriendByUUIDAndLogicID, int> _coinSentFriendDic = new Dictionary<stFriendByUUIDAndLogicID, int>();
        private int _curRankingListIndex = -1;
        private RankingType _curRankingType = RankingType.None;
        private RankingSubView _curSubViewType;
        private RankingSubView _defualtSubViewType = RankingSubView.Friend;
        private CUIFormScript _form;
        private string _friendViewName;
        private uint _myLastFriendRank = 0x98967f;
        private GameObject _myselfInfo;
        private readonly LocalRankingInfo[] _rankingInfo = new LocalRankingInfo[10];
        private CUIListScript _rankingList;
        private bool _rankingListReady;
        private bool _rankingSelfReady;
        private ScrollRect _scroll;
        private bool _showRankTypeMenu;
        private ListView<COMDT_FRIEND_INFO> _sortedFriendRankList;
        private CUIListScript _tabList;
        private CUIListScript _viewList;
        private const string AnimCondition = "IsDisplayRankingPanel";
        public readonly string formName = string.Format("{0}{1}", "UGUI/Form/System/", "Ranking/Form_Ranking.prefab");

        internal void Clear()
        {
            this._tabList = null;
            this._rankingList = null;
            this._animator = null;
            this._form = null;
            this._showRankTypeMenu = false;
            this._curRankingType = RankingType.None;
            this._curSubViewType = RankingSubView.All;
        }

        public void ClearAll()
        {
            this.Clear();
            this._defualtSubViewType = RankingSubView.Friend;
        }

        protected void CommitUpdate()
        {
            if (this._rankingList.GetSelectedElement() != null)
            {
                this._rankingList.GetSelectedElement().ChangeDisplay(false);
            }
            this._curRankingListIndex = -1;
            if (this.NeedToRetrieveNewList())
            {
                this.RetrieveNewList();
            }
            else
            {
                this._rankingList.MoveElementInScrollArea(0, true);
                this.UpdateAllElementInView();
                this.UpdateSelfInfo();
            }
        }

        protected int ConvertFriendRankIndex(int rankNo)
        {
            int num = rankNo;
            if (rankNo >= this._myLastFriendRank)
            {
                num--;
            }
            return num;
        }

        private static void ConvertPvpLevelAndPhase(uint score, out int level, out int remaining)
        {
            level = 1;
            uint num = score;
            int num2 = 1;
            int num3 = GameDataMgr.acntPvpExpDatabin.Count();
            for (int i = 1; i <= (num3 - 1); i++)
            {
                ResAcntPvpExpInfo dataByKey = GameDataMgr.acntPvpExpDatabin.GetDataByKey((byte) i);
                if (num < dataByKey.dwNeedExp)
                {
                    num2 = i;
                    break;
                }
                num -= dataByKey.dwNeedExp;
                num2 = i + 1;
            }
            level = num2;
            remaining = (int) num;
        }

        protected static RankingType ConvertRankingLocalType(COM_APOLLO_TRANK_SCORE_TYPE rankType)
        {
            RankingType power = RankingType.Power;
            switch (rankType)
            {
                case COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_POWER:
                    return RankingType.Power;

                case COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_PVP_EXP:
                    return RankingType.PvpRank;

                case COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_GUILD_POWER:
                case COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_GUILD_RANK_POINT:
                    return power;

                case COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_HERO_NUM:
                    return RankingType.HeroNum;

                case COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_SKIN_NUM:
                    return RankingType.SkinNum;

                case COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_LADDER_POINT:
                    return RankingType.Ladder;

                case COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_ACHIEVEMENT:
                    return RankingType.Achievement;

                case COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_WIN_GAMENUM:
                    return RankingType.WinCount;

                case COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_CONTINOUS_WIN:
                    return RankingType.ConWinCount;

                case COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_USE_COUPONS:
                    return RankingType.ConsumeQuan;

                case COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_VIP_SCORE:
                    return RankingType.GameVip;
            }
            return power;
        }

        protected static COM_APOLLO_TRANK_SCORE_TYPE ConvertRankingServerType(RankingType rankType)
        {
            COM_APOLLO_TRANK_SCORE_TYPE com_apollo_trank_score_type = COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_NULL;
            switch (rankType)
            {
                case RankingType.PvpRank:
                    return COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_PVP_EXP;

                case RankingType.Power:
                    return COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_POWER;

                case RankingType.HeroNum:
                    return COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_HERO_NUM;

                case RankingType.SkinNum:
                    return COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_SKIN_NUM;

                case RankingType.Ladder:
                    return COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_LADDER_POINT;

                case RankingType.Achievement:
                    return COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_ACHIEVEMENT;

                case RankingType.WinCount:
                    return COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_WIN_GAMENUM;

                case RankingType.ConWinCount:
                    return COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_CONTINOUS_WIN;

                case RankingType.ConsumeQuan:
                    return COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_USE_COUPONS;

                case RankingType.GameVip:
                    return COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_VIP_SCORE;
            }
            return com_apollo_trank_score_type;
        }

        private void DoDisplayAnimation()
        {
            if (this._animator != null)
            {
                this._animator.SetBool("IsDisplayRankingPanel", true);
            }
        }

        protected void DoHideAnimation(CUIEvent uiEvent)
        {
            if (this._animator != null)
            {
                this._animator.SetBool("IsDisplayRankingPanel", false);
            }
        }

        private void EmptyOneElement(GameObject objElement, int viewIndex)
        {
            objElement.GetComponent<RankingItemHelper>().RankingNumText.CustomSetActive(false);
        }

        public int GetMyFriendRankNo()
        {
            int num = -1;
            RankingType ladder = RankingType.Ladder;
            if (this._rankingInfo[(int) ladder].SelfInfo != null)
            {
                uint dwScore = this._rankingInfo[(int) ladder].SelfInfo.dwScore;
                ListView<COMDT_FRIEND_INFO> sortedRankingFriendList = Singleton<CFriendContoller>.instance.model.GetSortedRankingFriendList(ConvertRankingServerType(ladder));
                uint num3 = (uint) (sortedRankingFriendList.Count + 1);
                uint num4 = 0;
                uint dwPvpLvl = 0;
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
                for (int i = 0; i < num3; i++)
                {
                    num4 = 0;
                    num = i;
                    if (i < sortedRankingFriendList.Count)
                    {
                        num4 = sortedRankingFriendList[i].RankVal[(int) ConvertRankingServerType(ladder)];
                        dwPvpLvl = sortedRankingFriendList[i].dwPvpLvl;
                    }
                    if (((i < sortedRankingFriendList.Count) && (dwScore >= num4)) && (((ladder != RankingType.Ladder) || (dwScore != num4)) || (masterRoleInfo.PvpLevel >= dwPvpLvl)))
                    {
                        return num;
                    }
                }
            }
            return num;
        }

        private static string GetPvpRankName(int level)
        {
            ResAcntPvpExpInfo dataByKey = GameDataMgr.acntPvpExpDatabin.GetDataByKey((byte) level);
            return ((dataByKey != null) ? string.Format("Lv.{0}", dataByKey.bLevel) : string.Empty);
        }

        private static string GetPvpRankNameEx(uint score)
        {
            int level = 1;
            int remaining = 0;
            ConvertPvpLevelAndPhase(score, out level, out remaining);
            return GetPvpRankName(level);
        }

        private COMDT_RANKING_LIST_ITEM_EXTRA_PLAYER GetRankItemDetailInfo(RankingType rankType, int listIndex)
        {
            if (((this._rankingInfo[(int) rankType].ListInfo != null) && (listIndex >= 0)) && ((listIndex < this._rankingInfo[(int) rankType].ListInfo.astItemDetail.Length) && (listIndex < this._rankingInfo[(int) rankType].ListInfo.dwItemNum)))
            {
                switch (rankType)
                {
                    case RankingType.PvpRank:
                        return this._rankingInfo[(int) rankType].ListInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stPvpExp;

                    case RankingType.Power:
                        return this._rankingInfo[(int) rankType].ListInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stPower;

                    case RankingType.HeroNum:
                        return this._rankingInfo[(int) rankType].ListInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stHeroNum;

                    case RankingType.SkinNum:
                        return this._rankingInfo[(int) rankType].ListInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stSkinNum;

                    case RankingType.Ladder:
                        return this._rankingInfo[(int) rankType].ListInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stLadderPoint;

                    case RankingType.Achievement:
                        return this._rankingInfo[(int) rankType].ListInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stAchievement;

                    case RankingType.WinCount:
                        return this._rankingInfo[(int) rankType].ListInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stWinGameNum;

                    case RankingType.ConWinCount:
                        return this._rankingInfo[(int) rankType].ListInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stContinousWin;

                    case RankingType.ConsumeQuan:
                        return this._rankingInfo[(int) rankType].ListInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stUseCoupons;

                    case RankingType.GameVip:
                        return this._rankingInfo[(int) rankType].ListInfo.astItemDetail[listIndex].stExtraInfo.stDetailInfo.stVipScore;
                }
            }
            return new COMDT_RANKING_LIST_ITEM_EXTRA_PLAYER();
        }

        public CSDT_RANKING_LIST_SUCC GetRankList(RankingType rankingType)
        {
            return this._rankingInfo[(int) rankingType].ListInfo;
        }

        protected void HideAllRankMenu()
        {
            this._showRankTypeMenu = false;
            this._form.m_formWidgets[14].CustomSetActive(false);
        }

        public void ImpResRankingDetail(SCPKG_GET_RANKING_ACNT_INFO_RSP rsp)
        {
            RankingType type = ConvertRankingLocalType((COM_APOLLO_TRANK_SCORE_TYPE) rsp.stAcntRankingDetail.stOfSucc.bNumberType);
            if (type == RankingType.Ladder)
            {
                Singleton<CLadderSystem>.GetInstance().Ranking = rsp.stAcntRankingDetail.stOfSucc.dwRankNo;
            }
            this._rankingInfo[(int) type].LastRetrieveTime = (uint) Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().getCurrentTimeSinceLogin();
            this._rankingInfo[(int) type].SelfInfo = rsp.stAcntRankingDetail.stOfSucc;
            Singleton<EventRouter>.GetInstance().BroadCastEvent("Rank_Friend_List");
            if (this._form != null)
            {
                this._rankingSelfReady = true;
                if (((type == this._curRankingType) && this._rankingSelfReady) && this._rankingListReady)
                {
                    this.UpdateAllElementInView();
                    this.UpdateSelfInfo();
                }
                this.TryToUnlock();
            }
        }

        public void ImpResRankingList(SCPKG_GET_RANKING_LIST_RSP rsp)
        {
            if (this._form != null)
            {
                RankingType type = ConvertRankingLocalType((COM_APOLLO_TRANK_SCORE_TYPE) rsp.stRankingListDetail.stOfSucc.bNumberType);
                this._rankingListReady = true;
                this._rankingInfo[(int) type].LastRetrieveTime = (uint) Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().getCurrentTimeSinceLogin();
                this._rankingInfo[(int) type].ListInfo = rsp.stRankingListDetail.stOfSucc;
                if (((type == this._curRankingType) && this._rankingListReady) && this._rankingSelfReady)
                {
                    this.UpdateAllElementInView();
                    this.UpdateSelfInfo();
                }
                this.TryToUnlock();
            }
        }

        public override void Init()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_OpenForm, new CUIEventManager.OnUIEventHandler(this.OnRanking_OpenForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnRanking_CloseForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ShowAllRankType, new CUIEventManager.OnUIEventHandler(this.OnShowAllRankTypeMenu));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ChangeView, new CUIEventManager.OnUIEventHandler(this.OnChangeSubViewTab));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ChangeRankTypeToLadder, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToLadder));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ChangeRankTypeToHeroCount, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToHeroCount));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ChangeRankTypeToSkinCount, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToSkinCount));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ChangeRankTypeToAchievement, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToAchievement));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ChangeRankTypeToWinCount, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToWinCount));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ChangeRankTypeToConWinCount, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToConWinCount));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ChangeRankTypeToVip, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToVip));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_HoldDetail, new CUIEventManager.OnUIEventHandler(this.OnHoldDetail));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ElementEnable, new CUIEventManager.OnUIEventHandler(this.OnElementEnable));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_AddFriend, new CUIEventManager.OnUIEventHandler(this.OnAddFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ClickListItem, new CUIEventManager.OnUIEventHandler(this.OnClickOneListItem));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ClickMe, new CUIEventManager.OnUIEventHandler(this.OnClickMe));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_ClickCloseBtn, new CUIEventManager.OnUIEventHandler(this.DoHideAnimation));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_Friend_SNS_SendCoin, new CUIEventManager.OnUIEventHandler(this.OnFriendSnsSendCoin));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Ranking_Friend_GAME_SendCoin, new CUIEventManager.OnUIEventHandler(this.OnFriendSendCoin));
            Singleton<EventRouter>.GetInstance().AddEventHandler<SCPKG_GET_RANKING_LIST_RSP>("Ranking_Get_Ranking_List", new Action<SCPKG_GET_RANKING_LIST_RSP>(this.OnGetRankingList));
            Singleton<EventRouter>.GetInstance().AddEventHandler<SCPKG_GET_RANKING_ACNT_INFO_RSP>("Ranking_Get_Ranking_Account_Info", new Action<SCPKG_GET_RANKING_ACNT_INFO_RSP>(this.OnGetRankingAccountInfo));
            Singleton<EventRouter>.GetInstance().AddEventHandler<SCPKG_CMD_DONATE_FRIEND_POINT>("Friend_Send_Coin_Done", new Action<SCPKG_CMD_DONATE_FRIEND_POINT>(this.OnCoinSent));
            for (int i = 0; i < this._rankingInfo.Length; i++)
            {
                this._rankingInfo[i].LastRetrieveTime = 0;
                this._rankingInfo[i].ListInfo = null;
                this._rankingInfo[i].SelfInfo = null;
            }
            this._showRankTypeMenu = false;
        }

        internal void InitWidget()
        {
            this._form = Singleton<CUIManager>.GetInstance().OpenForm(this.formName, false, true);
            this._animator = this._form.gameObject.GetComponent<Animator>();
            this._tabList = this._form.m_formWidgets[1].GetComponent<CUIListScript>();
            this._viewList = this._form.m_formWidgets[13].GetComponent<CUIListScript>();
            this._rankingList = this._form.m_formWidgets[3].GetComponent<CUIListScript>();
            this._scroll = this._form.m_formWidgets[4].GetComponent<ScrollRect>();
            this._myselfInfo = this._form.m_formWidgets[8];
            this._scroll.elasticity = 0.08f;
            this._rankingList.SetElementAmount(0);
            this._rankingList.m_alwaysDispatchSelectedChangeEvent = true;
            this._tabList.SetElementAmount(1);
            this._tabList.SelectElement(0, true);
            CUIListElementScript elemenet = this._viewList.GetElemenet(0);
            if (elemenet != null)
            {
                this._allViewName = Singleton<CTextManager>.GetInstance().GetText("ranking_ViewAll");
                this._friendViewName = Singleton<CTextManager>.GetInstance().GetText("ranking_ViewFriend");
                elemenet.gameObject.transform.FindChild("Text").GetComponent<Text>().text = this._allViewName;
            }
            elemenet = this._viewList.GetElemenet(1);
            if (elemenet != null)
            {
                elemenet.gameObject.transform.FindChild("Text").GetComponent<Text>().text = this._friendViewName;
            }
            this._form.m_formWidgets[14].CustomSetActive(false);
            this.SetMenuElementText();
            this.TryToChangeRankType(RankingType.Ladder);
            if (this._defualtSubViewType == RankingSubView.All)
            {
                this._viewList.SelectElement(0, true);
            }
            else if (this._defualtSubViewType == RankingSubView.Friend)
            {
                this._viewList.SelectElement(1, true);
            }
        }

        protected bool NeedToRetrieveNewList()
        {
            int index = (int) this._curRankingType;
            return ((((this._rankingInfo[index].SelfInfo == null) || (this._rankingInfo[index].ListInfo == null)) || (this._rankingInfo[index].LastRetrieveTime == 0)) || (Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().getCurrentTimeSinceLogin() >= (this._rankingInfo[index].LastRetrieveTime + this._rankingInfo[index].ListInfo.dwTimeLimit)));
        }

        protected void OnAddFriend(CUIEvent uiEvent)
        {
            if (this._rankingList != null)
            {
                this._curRankingListIndex = this._rankingList.GetSelectedIndex();
                ulong ullUid = 0L;
                int dwLogicWorldId = 0;
                if (this._curSubViewType == RankingSubView.Friend)
                {
                    if (this._curRankingListIndex != this._myLastFriendRank)
                    {
                        int num3 = this.ConvertFriendRankIndex(this._curRankingListIndex);
                        COMDT_FRIEND_INFO comdt_friend_info = this._sortedFriendRankList[num3];
                        ullUid = comdt_friend_info.stUin.ullUid;
                        dwLogicWorldId = (int) comdt_friend_info.stUin.dwLogicWorldId;
                    }
                    else
                    {
                        ullUid = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().playerUllUID;
                        dwLogicWorldId = MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID;
                    }
                }
                else
                {
                    COMDT_RANKING_LIST_ITEM_EXTRA_PLAYER rankItemDetailInfo = this.GetRankItemDetailInfo(this._curRankingType, this._curRankingListIndex);
                    if (rankItemDetailInfo != null)
                    {
                        ullUid = rankItemDetailInfo.ullUid;
                        dwLogicWorldId = rankItemDetailInfo.iLogicWorldId;
                    }
                }
                FriendSysNetCore.Send_Request_BeFriend(ullUid, (uint) dwLogicWorldId);
            }
        }

        protected void OnChangeRankToAchievement(CUIEvent uiEvent)
        {
            this.TryToChangeRankType(RankingType.Achievement);
            this.HideAllRankMenu();
        }

        protected void OnChangeRankToConWinCount(CUIEvent uiEvent)
        {
            this.TryToChangeRankType(RankingType.ConWinCount);
            this.HideAllRankMenu();
        }

        protected void OnChangeRankToHeroCount(CUIEvent uiEvent)
        {
            this.TryToChangeRankType(RankingType.HeroNum);
            this.HideAllRankMenu();
        }

        protected void OnChangeRankToLadder(CUIEvent uiEvent)
        {
            this.TryToChangeRankType(RankingType.Ladder);
            this.HideAllRankMenu();
        }

        protected void OnChangeRankToSkinCount(CUIEvent uiEvent)
        {
            this.TryToChangeRankType(RankingType.SkinNum);
            this.HideAllRankMenu();
        }

        protected void OnChangeRankToVip(CUIEvent uiEvent)
        {
            this.TryToChangeRankType(RankingType.GameVip);
            this.HideAllRankMenu();
        }

        protected void OnChangeRankToWinCount(CUIEvent uiEvent)
        {
            this.TryToChangeRankType(RankingType.WinCount);
            this.HideAllRankMenu();
        }

        protected void OnChangeSubViewTab(CUIEvent uiEvent)
        {
            if (this._curSubViewType != this._viewList.GetSelectedIndex())
            {
                this._defualtSubViewType = this._curSubViewType = (RankingSubView) this._viewList.GetSelectedIndex();
                this._form.m_formWidgets[15].CustomSetActive(false);
                this.UpdateTabText();
                this.CommitUpdate();
                Singleton<EventRouter>.GetInstance().BroadCastEvent<RankingSubView>("Rank_List", this._curSubViewType);
            }
        }

        protected void OnClickMe(CUIEvent uiEvent)
        {
            if ((this._rankingList != null) && (this._rankingList.GetSelectedElement() != null))
            {
                this._rankingList.GetSelectedElement().ChangeDisplay(false);
            }
        }

        protected void OnClickOneListItem(CUIEvent uiEvent)
        {
            if (this._rankingList != null)
            {
                this._curRankingListIndex = this._rankingList.GetSelectedIndex();
                this._rankingList.GetSelectedElement().ChangeDisplay(true);
            }
        }

        private void OnCoinSent(SCPKG_CMD_DONATE_FRIEND_POINT data)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(this.formName);
            CFriendModel.FriendType type = (data.bFriendType != 1) ? CFriendModel.FriendType.SNS : CFriendModel.FriendType.GameFriend;
            ulong ullUid = data.stUin.ullUid;
            uint dwLogicWorldId = data.stUin.dwLogicWorldId;
            stFriendByUUIDAndLogicID key = new stFriendByUUIDAndLogicID(ullUid, dwLogicWorldId, type);
            if (this._coinSentFriendDic.ContainsKey(key))
            {
                int num3 = -1;
                if (this._coinSentFriendDic.TryGetValue(key, out num3))
                {
                    this._coinSentFriendDic.Remove(key);
                    if (this._rankingList != null)
                    {
                        CUIListElementScript elemenet = this._rankingList.GetElemenet(num3);
                        if (elemenet != null)
                        {
                            CUIEvent uiEvent = new CUIEvent {
                                m_eventID = enUIEventID.Ranking_ElementEnable,
                                m_srcFormScript = form,
                                m_srcWidget = elemenet.gameObject,
                                m_srcWidgetIndexInBelongedList = num3
                            };
                            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(uiEvent);
                            return;
                        }
                    }
                }
            }
            if (form != null)
            {
                this.UpdateAllElementInView();
            }
        }

        protected void OnElementEnable(CUIEvent uiEvent)
        {
            this.UpdateOneElement(uiEvent.m_srcWidget, uiEvent.m_srcWidgetIndexInBelongedList);
            CUIListElementScript component = uiEvent.m_srcWidget.GetComponent<CUIListElementScript>();
            if ((component != null) && ((this._curRankingListIndex == -1) || (uiEvent.m_srcWidgetIndexInBelongedList != this._curRankingListIndex)))
            {
                component.ChangeDisplay(false);
            }
        }

        private void OnFriendSendCoin(CUIEvent uiEvent)
        {
            stFriendByUUIDAndLogicID key = new stFriendByUUIDAndLogicID(uiEvent.m_eventParams.commonUInt64Param1, (uint) uiEvent.m_eventParams.commonUInt64Param2, CFriendModel.FriendType.GameFriend);
            if (!this._coinSentFriendDic.ContainsKey(key))
            {
                this._coinSentFriendDic.Add(key, uiEvent.m_eventParams.tag);
            }
            uiEvent.m_eventID = enUIEventID.Friend_SendCoin;
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(uiEvent);
        }

        private void OnFriendSnsSendCoin(CUIEvent uiEvent)
        {
            stFriendByUUIDAndLogicID key = new stFriendByUUIDAndLogicID(uiEvent.m_eventParams.commonUInt64Param1, (uint) uiEvent.m_eventParams.commonUInt64Param2, CFriendModel.FriendType.SNS);
            if (!this._coinSentFriendDic.ContainsKey(key))
            {
                this._coinSentFriendDic.Add(key, uiEvent.m_eventParams.tag);
            }
            uiEvent.m_eventID = enUIEventID.Friend_SNS_SendCoin;
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(uiEvent);
        }

        public void OnGetRankingAccountInfo(SCPKG_GET_RANKING_ACNT_INFO_RSP rsp)
        {
            Singleton<RankingSystem>.instance.ImpResRankingDetail(rsp);
        }

        public void OnGetRankingList(SCPKG_GET_RANKING_LIST_RSP rsp)
        {
            Singleton<RankingSystem>.instance.ImpResRankingList(rsp);
        }

        public void OnHideAnimationEnd()
        {
            Singleton<CUIManager>.GetInstance().CloseForm(this.formName);
        }

        protected void OnHoldDetail(CUIEvent uiEvent)
        {
            if (this._rankingList != null)
            {
                this._curRankingListIndex = this._rankingList.GetSelectedIndex();
                ulong ullUid = 0L;
                int dwLogicWorldId = 0;
                if (this._curSubViewType == RankingSubView.Friend)
                {
                    if (this._curRankingListIndex == this._myLastFriendRank)
                    {
                        return;
                    }
                    int num3 = this.ConvertFriendRankIndex(this._curRankingListIndex);
                    COMDT_FRIEND_INFO comdt_friend_info = this._sortedFriendRankList[num3];
                    ullUid = comdt_friend_info.stUin.ullUid;
                    dwLogicWorldId = (int) comdt_friend_info.stUin.dwLogicWorldId;
                }
                else
                {
                    COMDT_RANKING_LIST_ITEM_EXTRA_PLAYER rankItemDetailInfo = this.GetRankItemDetailInfo(this._curRankingType, this._curRankingListIndex);
                    if (rankItemDetailInfo != null)
                    {
                        ullUid = rankItemDetailInfo.ullUid;
                        dwLogicWorldId = rankItemDetailInfo.iLogicWorldId;
                        if ((ullUid == Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().playerUllUID) && (dwLogicWorldId == MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID))
                        {
                            return;
                        }
                    }
                }
                CUIEvent event2 = new CUIEvent {
                    m_eventID = enUIEventID.Mini_Player_Info_Open_Form,
                    m_srcFormScript = uiEvent.m_srcFormScript
                };
                event2.m_eventParams.tag = 1;
                event2.m_eventParams.commonUInt64Param1 = ullUid;
                event2.m_eventParams.tag2 = dwLogicWorldId;
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(event2);
            }
        }

        protected void OnRanking_CloseForm(CUIEvent uiEvent)
        {
            this.Clear();
            Singleton<LobbyUISys>.instance.ShowHideRankingBtn(true);
        }

        protected void OnRanking_OpenForm(CUIEvent uiEvent)
        {
            Singleton<LobbyUISys>.instance.ShowHideRankingBtn(false);
            this.InitWidget();
            this.TryToChangeRankType(RankingType.Ladder);
            this.DoDisplayAnimation();
        }

        protected void OnShowAllRankTypeMenu(CUIEvent uiEvent)
        {
            if (this._form.m_formWidgets[14].activeSelf)
            {
                this.HideAllRankMenu();
            }
            else
            {
                this.ShowAllRankMenu();
            }
        }

        private static void RankingNumSet(uint rankNumber, RankingItemHelper rankingHelper)
        {
            rankingHelper.RankingNumText.CustomSetActive(false);
            rankingHelper.No1.CustomSetActive(false);
            rankingHelper.No2.CustomSetActive(false);
            rankingHelper.No3.CustomSetActive(false);
            rankingHelper.No1BG.CustomSetActive(false);
            rankingHelper.No1IconFrame.CustomSetActive(false);
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
                        if ((rankingHelper.No1BG != null) && (rankingHelper.No1IconFrame != null))
                        {
                            rankingHelper.No1BG.CustomSetActive(true);
                            rankingHelper.No1IconFrame.CustomSetActive(true);
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

        protected void ReqRankingDetail(int listIndex, bool isSelf = false, RankingType rankType = -1)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0xa2c);
            if (isSelf)
            {
                msg.stPkgData.stGetRankingAcntInfoReq.bNumberType = (byte) ConvertRankingServerType(rankType);
            }
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        protected void ReqRankingList(RankingType rankType)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0xa2a);
            msg.stPkgData.stGetRankingListReq.bNumberType = (byte) ConvertRankingServerType(rankType);
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        private void RetrieveNewList()
        {
            RankingType rankType = this._curRankingType;
            this.ReqRankingList(rankType);
            this.ReqRankingDetail(-1, true, rankType);
            this._rankingListReady = this._rankingSelfReady = false;
        }

        protected void RetrieveSortedFriendRankList()
        {
            this._myLastFriendRank = 0x98967f;
            this._sortedFriendRankList = Singleton<CFriendContoller>.instance.model.GetSortedRankingFriendList(ConvertRankingServerType(this._curRankingType));
        }

        public void SendReqRankingDetail()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0xa2c);
            msg.stPkgData.stGetRankingAcntInfoReq.bNumberType = 7;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
        }

        private static void SetGameObjChildText(GameObject parentObj, string childName, string text)
        {
            if (parentObj != null)
            {
                parentObj.transform.FindChild(childName).gameObject.GetComponent<Text>().text = text;
            }
        }

        public static void SetHostUrlHeadIcon(GameObject headIcon)
        {
            if (!CSysDynamicBlock.bSocialBlocked)
            {
                string headUrl = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().HeadUrl;
                headIcon.GetComponent<CUIHttpImageScript>().SetImageUrl(headUrl);
            }
        }

        private void SetMenuElementText()
        {
            GameObject obj2 = this._form.m_formWidgets[14];
            if (obj2 != null)
            {
                obj2.transform.FindChild("ListElement0").gameObject.transform.FindChild("Text").GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("ranking_LadderRankName");
                obj2.transform.FindChild("ListElement1").gameObject.transform.FindChild("Text").GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("ranking_HeroCountRankName");
                obj2.transform.FindChild("ListElement2").gameObject.transform.FindChild("Text").GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("ranking_SkinCountRankName");
                obj2.transform.FindChild("ListElement3").gameObject.transform.FindChild("Text").GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("ranking_AchieveRankName");
                obj2.transform.FindChild("ListElement4").gameObject.transform.FindChild("Text").GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("ranking_WinCountRankName");
                obj2.transform.FindChild("ListElement5").gameObject.transform.FindChild("Text").GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("ranking_ConWinRankName");
                obj2.transform.FindChild("ListElement6").gameObject.transform.FindChild("Text").GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("ranking_GameVIPRankName");
            }
        }

        public static void SetUrlHeadIcon(GameObject headIcon, string serverUrl)
        {
            if (!CSysDynamicBlock.bSocialBlocked)
            {
                headIcon.GetComponent<CUIHttpImageScript>().SetImageUrl(serverUrl);
            }
        }

        protected void ShowAllRankMenu()
        {
            this._showRankTypeMenu = true;
            this._form.m_formWidgets[14].CustomSetActive(true);
        }

        protected void TryToChangeRankType(RankingType rankType)
        {
            if (rankType != this._curRankingType)
            {
                this._form.m_formWidgets[15].CustomSetActive(false);
                this._curRankingType = rankType;
                this.UpdateTabText();
                this.CommitUpdate();
            }
        }

        private void TryToUnlock()
        {
            if (this._rankingListReady && this._rankingSelfReady)
            {
                Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
                this._rankingListReady = this._rankingSelfReady = false;
            }
        }

        public override void UnInit()
        {
            base.UnInit();
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_OpenForm, new CUIEventManager.OnUIEventHandler(this.OnRanking_OpenForm));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnRanking_CloseForm));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ShowAllRankType, new CUIEventManager.OnUIEventHandler(this.OnShowAllRankTypeMenu));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ChangeView, new CUIEventManager.OnUIEventHandler(this.OnChangeSubViewTab));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ChangeRankTypeToLadder, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToLadder));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ChangeRankTypeToHeroCount, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToHeroCount));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ChangeRankTypeToSkinCount, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToSkinCount));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ChangeRankTypeToAchievement, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToAchievement));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ChangeRankTypeToWinCount, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToWinCount));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ChangeRankTypeToConWinCount, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToConWinCount));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ChangeRankTypeToVip, new CUIEventManager.OnUIEventHandler(this.OnChangeRankToVip));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_HoldDetail, new CUIEventManager.OnUIEventHandler(this.OnHoldDetail));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ElementEnable, new CUIEventManager.OnUIEventHandler(this.OnElementEnable));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_AddFriend, new CUIEventManager.OnUIEventHandler(this.OnAddFriend));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ClickListItem, new CUIEventManager.OnUIEventHandler(this.OnClickOneListItem));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ClickMe, new CUIEventManager.OnUIEventHandler(this.OnClickMe));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_ClickCloseBtn, new CUIEventManager.OnUIEventHandler(this.DoHideAnimation));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_Friend_SNS_SendCoin, new CUIEventManager.OnUIEventHandler(this.OnFriendSnsSendCoin));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Ranking_Friend_GAME_SendCoin, new CUIEventManager.OnUIEventHandler(this.OnFriendSendCoin));
        }

        protected void UpdateAllElementInView()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            if (this._curSubViewType != RankingSubView.All)
            {
                int elementAmount = this._rankingList.GetElementAmount();
                for (int j = 0; j < elementAmount; j++)
                {
                    if ((this._rankingList.GetElemenet(j) != null) && this._rankingList.IsElementInScrollArea(j))
                    {
                        this.EmptyOneElement(this._rankingList.GetElemenet(j).gameObject, j);
                    }
                }
            }
            uint dwItemNum = 0;
            if (this._curSubViewType == RankingSubView.Friend)
            {
                this.RetrieveSortedFriendRankList();
                dwItemNum = (uint) (this._sortedFriendRankList.Count + 1);
                uint dwScore = this._rankingInfo[(int) this._curRankingType].SelfInfo.dwScore;
                for (int k = 0; k < dwItemNum; k++)
                {
                    this._myLastFriendRank = (uint) k;
                    uint num6 = 0;
                    uint dwPvpLvl = 0;
                    if (k < this._sortedFriendRankList.Count)
                    {
                        num6 = this._sortedFriendRankList[k].RankVal[(int) ConvertRankingServerType(this._curRankingType)];
                        dwPvpLvl = this._sortedFriendRankList[k].dwPvpLvl;
                    }
                    if (((k < this._sortedFriendRankList.Count) && (dwScore >= num6)) && (((this._curRankingType != RankingType.Ladder) || (dwScore != num6)) || (masterRoleInfo.PvpLevel >= dwPvpLvl)))
                    {
                        break;
                    }
                }
            }
            else
            {
                dwItemNum = this._rankingInfo[(int) this._curRankingType].ListInfo.dwItemNum;
            }
            this._rankingList.SetElementAmount((int) dwItemNum);
            this._rankingList.MoveElementInScrollArea(0, true);
            for (int i = 0; i < dwItemNum; i++)
            {
                if ((this._rankingList.GetElemenet(i) != null) && this._rankingList.IsElementInScrollArea(i))
                {
                    this.UpdateOneElement(this._rankingList.GetElemenet(i).gameObject, i);
                }
            }
            if (dwItemNum == 0)
            {
                this._form.m_formWidgets[15].CustomSetActive(true);
            }
        }

        private void UpdateOneElement(GameObject objElement, int viewIndex)
        {
            if (this._rankingInfo[(int) this._curRankingType].ListInfo != null)
            {
                RankingItemHelper component = objElement.GetComponent<RankingItemHelper>();
                uint score = 0;
                string name = string.Empty;
                uint pvpLevel = 1;
                string serverUrl = null;
                ulong ullUid = 0L;
                uint dwLogicWorldID = 0;
                uint dwCurLevel = 0;
                uint dwHeadIconId = 0;
                uint dwQQVIPMask = 0;
                COM_PRIVILEGE_TYPE privilegeType = COM_PRIVILEGE_TYPE.COM_PRIVILEGE_TYPE_NONE;
                if (this._curSubViewType == RankingSubView.Friend)
                {
                    if (viewIndex == this._myLastFriendRank)
                    {
                        CSDT_GET_RANKING_ACNT_DETAIL_SELF selfInfo = this._rankingInfo[(int) this._curRankingType].SelfInfo;
                        if ((selfInfo != null) && (Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo() != null))
                        {
                            score = selfInfo.dwScore;
                            name = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().Name;
                            pvpLevel = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().PvpLevel;
                            serverUrl = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().HeadUrl;
                            privilegeType = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().m_privilegeType;
                            ullUid = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().playerUllUID;
                            dwLogicWorldID = (uint) MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID;
                            dwCurLevel = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().GetNobeInfo().stGameVipClient.dwCurLevel;
                            dwHeadIconId = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().GetNobeInfo().stGameVipClient.dwHeadIconId;
                            dwQQVIPMask = 0xdf1f9;
                            SetGameObjChildText(this._myselfInfo, "NameGroup/PlayerName", Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().Name);
                            SetGameObjChildText(this._myselfInfo, "PlayerLv", string.Format("Lv.{0}", Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().PvpLevel.ToString(CultureInfo.InvariantCulture)));
                        }
                    }
                    else
                    {
                        int num8 = this.ConvertFriendRankIndex(viewIndex);
                        if (((this._sortedFriendRankList != null) && (num8 < this._sortedFriendRankList.Count)) && (num8 >= 0))
                        {
                            COMDT_FRIEND_INFO comdt_friend_info = this._sortedFriendRankList[num8];
                            if (comdt_friend_info != null)
                            {
                                score = comdt_friend_info.RankVal[(int) ConvertRankingServerType(this._curRankingType)];
                                name = StringHelper.UTF8BytesToString(ref comdt_friend_info.szUserName);
                                pvpLevel = comdt_friend_info.dwPvpLvl;
                                serverUrl = Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(ref comdt_friend_info.szHeadUrl);
                                ullUid = comdt_friend_info.stUin.ullUid;
                                dwLogicWorldID = comdt_friend_info.stUin.dwLogicWorldId;
                                dwCurLevel = comdt_friend_info.stGameVip.dwCurLevel;
                                dwHeadIconId = comdt_friend_info.stGameVip.dwHeadIconId;
                                dwQQVIPMask = comdt_friend_info.dwQQVIPMask;
                                privilegeType = (COM_PRIVILEGE_TYPE) comdt_friend_info.bPrivilege;
                                CFriendModel.FriendInGame friendInGaming = Singleton<CFriendContoller>.instance.model.GetFriendInGaming(ullUid, dwLogicWorldID);
                                string nickName = string.Empty;
                                if (friendInGaming != null)
                                {
                                    nickName = friendInGaming.nickName;
                                }
                                if (!string.IsNullOrEmpty(nickName))
                                {
                                    name = string.Format("{0}({1})", StringHelper.UTF8BytesToString(ref comdt_friend_info.szUserName), nickName);
                                }
                            }
                        }
                    }
                }
                else
                {
                    int index = viewIndex;
                    if (((this._rankingInfo[(int) this._curRankingType].ListInfo.astItemDetail != null) && (index < this._rankingInfo[(int) this._curRankingType].ListInfo.astItemDetail.Length)) && (index < this._rankingInfo[(int) this._curRankingType].ListInfo.dwItemNum))
                    {
                        CSDT_RANKING_LIST_ITEM_INFO csdt_ranking_list_item_info = this._rankingInfo[(int) this._curRankingType].ListInfo.astItemDetail[index];
                        if (csdt_ranking_list_item_info != null)
                        {
                            score = csdt_ranking_list_item_info.dwRankScore;
                            COMDT_RANKING_LIST_ITEM_EXTRA_PLAYER rankItemDetailInfo = this.GetRankItemDetailInfo(this._curRankingType, index);
                            if (rankItemDetailInfo != null)
                            {
                                name = StringHelper.UTF8BytesToString(ref rankItemDetailInfo.szPlayerName);
                                pvpLevel = rankItemDetailInfo.dwPvpLevel;
                                serverUrl = Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(ref rankItemDetailInfo.szHeadUrl);
                                ullUid = rankItemDetailInfo.ullUid;
                                dwLogicWorldID = (uint) rankItemDetailInfo.iLogicWorldId;
                                dwCurLevel = rankItemDetailInfo.stGameVip.dwCurLevel;
                                dwHeadIconId = rankItemDetailInfo.stGameVip.dwHeadIconId;
                                privilegeType = (COM_PRIVILEGE_TYPE) rankItemDetailInfo.bPrivilege;
                                dwQQVIPMask = rankItemDetailInfo.dwVipLevel;
                            }
                        }
                    }
                }
                MonoSingleton<NobeSys>.GetInstance().SetGameCenterVisible(component.WxIcon, privilegeType, ApolloPlatform.Wechat, true, false);
                SetGameObjChildText(objElement, "NameGroup/PlayerName", name);
                SetGameObjChildText(objElement, "PlayerLv", string.Format("Lv.{0}", Math.Max(1, pvpLevel)));
                SetUrlHeadIcon(component.HeadIcon, serverUrl);
                if (this._curRankingType == RankingType.Ladder)
                {
                    component.LadderGo.CustomSetActive(true);
                    objElement.transform.FindChild("Value").gameObject.CustomSetActive(false);
                    objElement.transform.FindChild("ValueType").gameObject.CustomSetActive(false);
                }
                else
                {
                    component.LadderGo.CustomSetActive(false);
                    objElement.transform.FindChild("Value").gameObject.CustomSetActive(true);
                    objElement.transform.FindChild("ValueType").gameObject.CustomSetActive(true);
                }
                switch (this._curRankingType)
                {
                    case RankingType.PvpRank:
                    {
                        SetGameObjChildText(objElement, "ValueType", GetPvpRankNameEx(score));
                        int level = 1;
                        int remaining = 0;
                        ConvertPvpLevelAndPhase(score, out level, out remaining);
                        SetGameObjChildText(objElement, "Value", remaining.ToString(CultureInfo.InvariantCulture));
                        break;
                    }
                    case RankingType.HeroNum:
                        SetGameObjChildText(objElement, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemHeroCountName"));
                        SetGameObjChildText(objElement, "Value", score.ToString(CultureInfo.InvariantCulture));
                        break;

                    case RankingType.SkinNum:
                        SetGameObjChildText(objElement, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemSkinCountName"));
                        SetGameObjChildText(objElement, "Value", score.ToString(CultureInfo.InvariantCulture));
                        break;

                    case RankingType.Ladder:
                    {
                        if (pvpLevel < CLadderSystem.REQ_PLAYER_LEVEL)
                        {
                            SetGameObjChildText(objElement, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemNoLadderName"));
                            component.LadderGo.CustomSetActive(false);
                            objElement.transform.FindChild("Value").gameObject.CustomSetActive(false);
                            objElement.transform.FindChild("ValueType").gameObject.CustomSetActive(true);
                            break;
                        }
                        int num12 = CLadderSystem.ConvertEloToRank(score);
                        CLadderView.ShowRankDetail(component.LadderGo, (byte) num12, 1, false, true);
                        int curXingByEloAndRankLv = CLadderSystem.GetCurXingByEloAndRankLv(score, (uint) num12);
                        component.LadderXing.GetComponent<Text>().text = string.Format("x{0}", curXingByEloAndRankLv);
                        break;
                    }
                    case RankingType.Achievement:
                        SetGameObjChildText(objElement, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemAchieveName"));
                        SetGameObjChildText(objElement, "Value", score.ToString(CultureInfo.InvariantCulture));
                        break;

                    case RankingType.WinCount:
                        SetGameObjChildText(objElement, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemWinCountName"));
                        SetGameObjChildText(objElement, "Value", score.ToString(CultureInfo.InvariantCulture));
                        break;

                    case RankingType.ConWinCount:
                        SetGameObjChildText(objElement, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemConWinCountName"));
                        SetGameObjChildText(objElement, "Value", score.ToString(CultureInfo.InvariantCulture));
                        break;

                    case RankingType.ConsumeQuan:
                        SetGameObjChildText(objElement, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemConsumeQuanName"));
                        SetGameObjChildText(objElement, "Value", score.ToString(CultureInfo.InvariantCulture));
                        break;

                    case RankingType.GameVip:
                        SetGameObjChildText(objElement, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemGameVIPName"));
                        SetGameObjChildText(objElement, "Value", score.ToString(CultureInfo.InvariantCulture));
                        break;
                }
                uint rankNumber = (uint) (viewIndex + 1);
                RankingNumSet(rankNumber, component);
                COMDT_FRIEND_INFO comdt_friend_info2 = Singleton<CFriendContoller>.instance.model.GetInfo(CFriendModel.FriendType.GameFriend, ullUid, dwLogicWorldID);
                COMDT_FRIEND_INFO comdt_friend_info3 = Singleton<CFriendContoller>.instance.model.GetInfo(CFriendModel.FriendType.SNS, ullUid, dwLogicWorldID);
                bool flag = comdt_friend_info2 != null;
                bool flag2 = comdt_friend_info3 != null;
                COMDT_ACNT_UNIQ uniq = new COMDT_ACNT_UNIQ {
                    ullUid = ullUid,
                    dwLogicWorldId = dwLogicWorldID
                };
                if (this._curSubViewType == RankingSubView.Friend)
                {
                    component.AddFriend.CustomSetActive(false);
                }
                else
                {
                    uint playerUllUID = (uint) Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().playerUllUID;
                    component.AddFriend.CustomSetActive((!flag && !flag2) && (playerUllUID != ullUid));
                }
                CUIEventScript script = component.SendCoin.GetComponent<CUIEventScript>();
                if (flag2)
                {
                    component.ShowSendButton(Singleton<CFriendContoller>.instance.model.HeartData.BCanSendHeart(uniq, COM_FRIEND_TYPE.COM_FRIEND_TYPE_SNS));
                    script.m_onClickEventID = enUIEventID.Ranking_Friend_SNS_SendCoin;
                    script.m_onClickEventParams.tag = viewIndex;
                    script.m_onClickEventParams.commonUInt64Param1 = ullUid;
                    script.m_onClickEventParams.commonUInt64Param2 = dwLogicWorldID;
                    component.Online.CustomSetActive(true);
                    if (component.Online != null)
                    {
                        Text componetInChild = Utility.GetComponetInChild<Text>(component.Online, "Text");
                        if (componetInChild != null)
                        {
                            componetInChild.text = (comdt_friend_info3.bIsOnline == 0) ? Singleton<CTextManager>.GetInstance().GetText("Common_Offline") : string.Format("<color=#00ff00>{0}</color>", Singleton<CTextManager>.instance.GetText("Common_Online"));
                        }
                    }
                }
                else if (flag)
                {
                    component.ShowSendButton(Singleton<CFriendContoller>.instance.model.HeartData.BCanSendHeart(uniq, COM_FRIEND_TYPE.COM_FRIEND_TYPE_GAME));
                    script.m_onClickEventID = enUIEventID.Ranking_Friend_GAME_SendCoin;
                    script.m_onClickEventParams.tag = viewIndex;
                    script.m_onClickEventParams.commonUInt64Param1 = ullUid;
                    script.m_onClickEventParams.commonUInt64Param2 = dwLogicWorldID;
                    component.Online.CustomSetActive(true);
                    if (component.Online != null)
                    {
                        Text text2 = Utility.GetComponetInChild<Text>(component.Online, "Text");
                        if (text2 != null)
                        {
                            text2.text = (comdt_friend_info2.bIsOnline == 0) ? Singleton<CTextManager>.GetInstance().GetText("Common_Offline") : string.Format("<color=#00ff00>{0}</color>", Singleton<CTextManager>.instance.GetText("Common_Online"));
                        }
                    }
                }
                else
                {
                    component.SendCoin.CustomSetActive(false);
                    component.Online.CustomSetActive(false);
                    component.Online.CustomSetActive(false);
                }
                if (dwQQVIPMask == 0xdf1f9)
                {
                    MonoSingleton<NobeSys>.GetInstance().SetMyQQVipHead(component.QqVip.GetComponent<Image>());
                }
                else
                {
                    MonoSingleton<NobeSys>.GetInstance().SetOtherQQVipHead(component.QqVip.GetComponent<Image>(), (int) dwQQVIPMask);
                }
                MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(component.VipIcon.GetComponent<Image>(), (int) dwCurLevel, false);
                MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(component.HeadIconFrame.GetComponent<Image>(), (int) dwHeadIconId);
            }
        }

        protected void UpdateSelfInfo()
        {
            CSDT_GET_RANKING_ACNT_DETAIL_SELF selfInfo = this._rankingInfo[(int) this._curRankingType].SelfInfo;
            RankingItemHelper component = this._myselfInfo.GetComponent<RankingItemHelper>();
            uint pvpLevel = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().PvpLevel;
            SetGameObjChildText(this._myselfInfo, "NameGroup/PlayerName", Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().Name);
            SetGameObjChildText(this._myselfInfo, "PlayerLv", string.Format("Lv.{0}", pvpLevel.ToString(CultureInfo.InvariantCulture)));
            SetHostUrlHeadIcon(component.HeadIcon);
            if (this._curRankingType == RankingType.Ladder)
            {
                component.LadderGo.CustomSetActive(true);
                this._myselfInfo.transform.FindChild("Value").gameObject.CustomSetActive(false);
                this._myselfInfo.transform.FindChild("ValueType").gameObject.CustomSetActive(false);
            }
            else
            {
                component.LadderGo.CustomSetActive(false);
                this._myselfInfo.transform.FindChild("Value").gameObject.CustomSetActive(true);
                this._myselfInfo.transform.FindChild("ValueType").gameObject.CustomSetActive(true);
            }
            switch (this._curRankingType)
            {
                case RankingType.PvpRank:
                {
                    SetGameObjChildText(this._myselfInfo, "ValueType", GetPvpRankNameEx(selfInfo.dwScore));
                    int level = 1;
                    int remaining = 0;
                    ConvertPvpLevelAndPhase(selfInfo.dwScore, out level, out remaining);
                    SetGameObjChildText(this._myselfInfo, "Value", remaining.ToString(CultureInfo.InvariantCulture));
                    if (GameDataMgr.acntPvpExpDatabin.GetDataByKey((byte) level) == null)
                    {
                    }
                    break;
                }
                case RankingType.HeroNum:
                    SetGameObjChildText(this._myselfInfo, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemHeroCountName"));
                    SetGameObjChildText(this._myselfInfo, "Value", selfInfo.dwScore.ToString(CultureInfo.InvariantCulture));
                    break;

                case RankingType.SkinNum:
                    SetGameObjChildText(this._myselfInfo, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemSkinCountName"));
                    SetGameObjChildText(this._myselfInfo, "Value", selfInfo.dwScore.ToString(CultureInfo.InvariantCulture));
                    break;

                case RankingType.Ladder:
                {
                    if (pvpLevel < CLadderSystem.REQ_PLAYER_LEVEL)
                    {
                        SetGameObjChildText(this._myselfInfo, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemNoLadderName"));
                        component.LadderGo.CustomSetActive(false);
                        this._myselfInfo.transform.FindChild("Value").gameObject.CustomSetActive(false);
                        this._myselfInfo.transform.FindChild("ValueType").gameObject.CustomSetActive(true);
                        break;
                    }
                    int num4 = CLadderSystem.ConvertEloToRank(selfInfo.dwScore);
                    CLadderView.ShowRankDetail(component.LadderGo, (byte) num4, 1, false, true);
                    int curXingByEloAndRankLv = CLadderSystem.GetCurXingByEloAndRankLv(selfInfo.dwScore, (uint) num4);
                    component.LadderXing.GetComponent<Text>().text = string.Format("x{0}", curXingByEloAndRankLv);
                    break;
                }
                case RankingType.Achievement:
                    SetGameObjChildText(this._myselfInfo, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemAchieveName"));
                    SetGameObjChildText(this._myselfInfo, "Value", selfInfo.dwScore.ToString(CultureInfo.InvariantCulture));
                    break;

                case RankingType.WinCount:
                    SetGameObjChildText(this._myselfInfo, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemWinCountName"));
                    SetGameObjChildText(this._myselfInfo, "Value", selfInfo.dwScore.ToString(CultureInfo.InvariantCulture));
                    break;

                case RankingType.ConWinCount:
                    SetGameObjChildText(this._myselfInfo, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemConWinCountName"));
                    SetGameObjChildText(this._myselfInfo, "Value", selfInfo.dwScore.ToString(CultureInfo.InvariantCulture));
                    break;

                case RankingType.ConsumeQuan:
                    SetGameObjChildText(this._myselfInfo, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemConsumeQuanName"));
                    SetGameObjChildText(this._myselfInfo, "Value", selfInfo.dwScore.ToString(CultureInfo.InvariantCulture));
                    break;

                case RankingType.GameVip:
                    SetGameObjChildText(this._myselfInfo, "ValueType", Singleton<CTextManager>.GetInstance().GetText("ranking_ItemGameVIPName"));
                    SetGameObjChildText(this._myselfInfo, "Value", selfInfo.dwScore.ToString(CultureInfo.InvariantCulture));
                    break;
            }
            uint rankNumber = 0;
            if (this._curSubViewType == RankingSubView.Friend)
            {
                rankNumber = this._myLastFriendRank + 1;
            }
            else
            {
                rankNumber = selfInfo.dwRankNo;
            }
            if ((selfInfo.iRankChgNo == 0) || (this._curSubViewType != RankingSubView.All))
            {
                component.RankingUpDownIcon.CustomSetActive(false);
                SetGameObjChildText(this._myselfInfo, "ChangeNum", "--");
            }
            else if (selfInfo.iRankChgNo > 0)
            {
                component.RankingUpDownIcon.CustomSetActive(true);
                component.RankingUpDownIcon.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                SetGameObjChildText(this._myselfInfo, "ChangeNum", selfInfo.iRankChgNo.ToString(CultureInfo.InvariantCulture));
            }
            else if (selfInfo.iRankChgNo < 0)
            {
                component.RankingUpDownIcon.CustomSetActive(true);
                component.RankingUpDownIcon.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
                SetGameObjChildText(this._myselfInfo, "ChangeNum", -selfInfo.iRankChgNo.ToString(CultureInfo.InvariantCulture));
            }
            RankingNumSet(rankNumber, component);
            MonoSingleton<NobeSys>.GetInstance().SetMyQQVipHead(component.QqVip.GetComponent<Image>());
            MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(component.VipIcon.GetComponent<Image>(), (int) Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().GetNobeInfo().stGameVipClient.dwCurLevel, false);
            MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(component.HeadIconFrame.GetComponent<Image>(), (int) Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().GetNobeInfo().stGameVipClient.dwHeadIconId);
            MonoSingleton<NobeSys>.GetInstance().SetGameCenterVisible(component.WxIcon, Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().m_privilegeType, ApolloPlatform.Wechat, true, false);
        }

        private void UpdateTabText()
        {
            CUIListElementScript elemenet = this._tabList.GetElemenet(0);
            if (elemenet != null)
            {
                string text = null;
                if (this._curRankingType == RankingType.HeroNum)
                {
                    text = Singleton<CTextManager>.GetInstance().GetText("ranking_HeroCountRankName");
                }
                else if (this._curRankingType == RankingType.SkinNum)
                {
                    text = Singleton<CTextManager>.GetInstance().GetText("ranking_SkinCountRankName");
                }
                else if (this._curRankingType == RankingType.Ladder)
                {
                    text = Singleton<CTextManager>.GetInstance().GetText("ranking_LadderRankName");
                }
                else if (this._curRankingType == RankingType.Achievement)
                {
                    text = Singleton<CTextManager>.GetInstance().GetText("ranking_AchieveRankName");
                }
                else if (this._curRankingType == RankingType.WinCount)
                {
                    text = Singleton<CTextManager>.GetInstance().GetText("ranking_WinCountRankName");
                }
                else if (this._curRankingType == RankingType.ConWinCount)
                {
                    text = Singleton<CTextManager>.GetInstance().GetText("ranking_ConWinRankName");
                }
                else if (this._curRankingType == RankingType.ConsumeQuan)
                {
                    text = Singleton<CTextManager>.GetInstance().GetText("ranking_ConsumeQuanRankName");
                }
                else if (this._curRankingType == RankingType.GameVip)
                {
                    text = Singleton<CTextManager>.GetInstance().GetText("ranking_GameVIPRankName");
                }
                switch (this._curSubViewType)
                {
                    case RankingSubView.All:
                        elemenet.gameObject.transform.FindChild("Text").GetComponent<Text>().text = string.Format("{0}{1}", this._allViewName, text);
                        break;

                    case RankingSubView.Friend:
                        elemenet.gameObject.transform.FindChild("Text").GetComponent<Text>().text = string.Format("{0}{1}", this._friendViewName, text);
                        break;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct LocalRankingInfo
        {
            public uint LastRetrieveTime;
            public CSDT_RANKING_LIST_SUCC ListInfo;
            public CSDT_GET_RANKING_ACNT_DETAIL_SELF SelfInfo;
        }

        public enum RankingSubView
        {
            All,
            Friend,
            GuildMember
        }

        public enum RankingType
        {
            Achievement = 5,
            ConsumeQuan = 8,
            ConWinCount = 7,
            GameVip = 9,
            HeroNum = 2,
            Ladder = 4,
            MaxNum = 10,
            None = -1,
            Power = 1,
            PvpRank = 0,
            SkinNum = 3,
            WinCount = 6
        }
    }
}

