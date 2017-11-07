namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    [MessageHandlerClass]
    public class CUnionBattleEntrySystem : Singleton<CUnionBattleEntrySystem>
    {
        public SCPKG_GETAWARDPOOL_RSP m_awardPoolInfo = new SCPKG_GETAWARDPOOL_RSP();
        public SCPKG_GET_MATCHINFO_RSP m_baseInfo = new SCPKG_GET_MATCHINFO_RSP();
        public SCPKG_MATCHPOINT_NTF m_personInfo = new SCPKG_MATCHPOINT_NTF();
        private const RES_BATTLE_MAP_TYPE m_ResMapType = RES_BATTLE_MAP_TYPE.RES_BATTLE_MAP_TYPE_REWARDMATCH;
        private uint m_selectMapID;
        private ResRewardMatchLevelInfo m_selectMapRes;
        private ResRewardMatchTimeInfo m_selectTimeInfo;
        private readonly int m_unionBattleRuleId = 10;
        private static readonly uint SECOND_ONE_DAY = 0x15180;
        public static string UNION_ENTRY_PATH = "UGUI/Form/System/PvP/UnionBattle/Form_UnionBattleEntry";
        public static string UNION_ENTRY_SECOND_PATH = "UGUI/Form/System/PvP/UnionBattle/Form_UnionBattleEntrySecond";
        public static string UNION_ENTRY_THIRD_PATH = "UGUI/Form/System/PvP/UnionBattle/Form_UnionBattleEntryThird";

        private COMDT_MATCHPOINT GetMapPersonMatchPoint()
        {
            if (this.m_personInfo == null)
            {
                return null;
            }
            uint selectMapID = this.m_selectMapID;
            COMDT_MATCHPOINT[] astPointList = this.m_personInfo.astPointList;
            for (int i = 0; i < this.m_personInfo.dwCount; i++)
            {
                if (selectMapID == astPointList[i].dwMapId)
                {
                    return astPointList[i];
                }
            }
            return null;
        }

        public ResCommReward GetResCommonReward(uint awardID)
        {
            return GameDataMgr.commonRewardDatabin.GetDataByKey(awardID);
        }

        public static bool HasMatchInActiveTime()
        {
            int count = GameDataMgr.uinionBattleLevelDatabin.count;
            for (int i = 0; i < count; i++)
            {
                if (CUICommonSystem.GetMatchOpenState(RES_BATTLE_MAP_TYPE.RES_BATTLE_MAP_TYPE_REWARDMATCH, GameDataMgr.uinionBattleLevelDatabin.GetDataByIndex(i).dwMapId).matchState == enMatchOpenState.enMatchOpen_InActiveTime)
                {
                    return true;
                }
            }
            return false;
        }

        public override void Init()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_ClickEntry, new CUIEventManager.OnUIEventHandler(this.Open_BattleEntry));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_BattleEntryGroup_Click, new CUIEventManager.OnUIEventHandler(this.Open_SecondBattleEntry));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_SubBattleEntryGroup_Click, new CUIEventManager.OnUIEventHandler(this.Open_ThirdBattleEntry));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_Click_SingleStartMatch, new CUIEventManager.OnUIEventHandler(this.OnClickStartMatch));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_ConfirmBuyItem, new CUIEventManager.OnUIEventHandler(this.OnConfirmBuyItem));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_BuyTiketClick, new CUIEventManager.OnUIEventHandler(this.OnBuyTiketClick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_Click_Rule, new CUIEventManager.OnUIEventHandler(this.OnClickRule));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Union_Battle_RewardMatch_TimeUp, new CUIEventManager.OnUIEventHandler(this.OnRewardMatchTimeUp));
        }

        private void initFirstFormWidget()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(UNION_ENTRY_PATH);
            if (form != null)
            {
                GameObject widget = form.GetWidget(0);
                GameObject obj3 = form.GetWidget(1);
                if ((widget != null) && (obj3 != null))
                {
                    widget.GetComponent<CUIMiniEventScript>().m_onClickEventParams.tag = 0;
                    obj3.GetComponent<CUIMiniEventScript>().m_onClickEventParams.tag = 1;
                    int dwConfValue = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0xab).dwConfValue;
                    if (this.m_baseInfo.dwPlayerNum >= dwConfValue)
                    {
                        string[] args = new string[] { this.m_baseInfo.dwPlayerNum.ToString() };
                        string text = Singleton<CTextManager>.instance.GetText("Union_Battle_Tips12", args);
                        CUICommonSystem.SetTextContent(form.GetWidget(2), text);
                        CUICommonSystem.SetObjActive(form.GetWidget(2), true);
                    }
                    else
                    {
                        CUICommonSystem.SetObjActive(form.GetWidget(2), false);
                    }
                    if (CSysDynamicBlock.bLobbyEntryBlocked && (obj3 != null))
                    {
                        obj3.CustomSetActive(false);
                    }
                }
            }
        }

        private void initSecondFormWidget()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(UNION_ENTRY_SECOND_PATH);
            if (form != null)
            {
                GameObject widget = form.GetWidget(0);
                GameObject btn = form.GetWidget(1);
                GameObject obj4 = form.GetWidget(2);
                if (((widget != null) && (btn != null)) && (obj4 != null))
                {
                    uint[] numArray = new uint[10];
                    bool[] flagArray = new bool[10];
                    uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Union_1"), out numArray[0]);
                    uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_Union_2"), out numArray[1]);
                    flagArray[0] = CUICommonSystem.IsMatchOpened(RES_BATTLE_MAP_TYPE.RES_BATTLE_MAP_TYPE_REWARDMATCH, numArray[0]);
                    flagArray[1] = CUICommonSystem.IsMatchOpened(RES_BATTLE_MAP_TYPE.RES_BATTLE_MAP_TYPE_REWARDMATCH, numArray[1]);
                    widget.GetComponent<CUIMiniEventScript>().m_onClickEventParams.tag = 0;
                    btn.GetComponent<CUIMiniEventScript>().m_onClickEventParams.tag = 1;
                    obj4.GetComponent<CUIMiniEventScript>().m_onClickEventParams.tag = 2;
                    widget.GetComponent<CUIMiniEventScript>().m_onClickEventParams.tagUInt = numArray[0];
                    btn.GetComponent<CUIMiniEventScript>().m_onClickEventParams.tagUInt = numArray[1];
                    widget.GetComponent<CUIMiniEventScript>().m_onClickEventParams.commonBool = flagArray[0];
                    btn.GetComponent<CUIMiniEventScript>().m_onClickEventParams.commonBool = flagArray[1];
                    widget.transform.FindChild("Lock").gameObject.CustomSetActive(!flagArray[0]);
                    btn.transform.FindChild("Lock").gameObject.CustomSetActive(!flagArray[1]);
                    this.ShowCountDownTime(widget, flagArray[0]);
                    this.ShowCountDownTime(btn, flagArray[1]);
                    widget.transform.FindChild("Desc/MapNameTxt").GetComponent<Text>().text = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey(widget.GetComponent<CUIMiniEventScript>().m_onClickEventParams.tagUInt).stLevelCommonInfo.szName;
                    btn.transform.FindChild("Desc/MapNameTxt").GetComponent<Text>().text = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey(btn.GetComponent<CUIMiniEventScript>().m_onClickEventParams.tagUInt).stLevelCommonInfo.szName;
                    if (CSysDynamicBlock.bLobbyEntryBlocked && (obj4 != null))
                    {
                        obj4.CustomSetActive(false);
                    }
                }
            }
        }

        private void initThirdFormWidget()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(UNION_ENTRY_THIRD_PATH);
            if (form != null)
            {
                if (this.m_selectMapRes != null)
                {
                    Text component = form.GetWidget(0).GetComponent<Text>();
                    Text text2 = form.GetWidget(3).GetComponent<Text>();
                    GameObject gameObject = form.GetWidget(4).transform.FindChild("itemCell").gameObject;
                    Text text3 = form.GetWidget(5).transform.FindChild("ScoreTxt").GetComponent<Text>();
                    Text text4 = form.GetWidget(6).transform.FindChild("ScoreTxt").GetComponent<Text>();
                    Text text5 = form.transform.FindChild("panelGroup1/Text").GetComponent<Text>();
                    component.text = this.m_selectMapRes.stLevelCommonInfo.szName;
                    text5.text = this.m_selectMapRes.szMatchName;
                    ResCommReward resCommonReward = this.GetResCommonReward(this.m_selectMapRes.dwWinAwardID);
                    if (resCommonReward != null)
                    {
                        ResRewardDetail detail = resCommonReward.astRewardInfo[0];
                        CUseable itemUseable = CUseableManager.CreateUsableByServerType((RES_REWARDS_TYPE) detail.dwRewardType, detail.iCnt, detail.dwRewardID);
                        CUICommonSystem.SetItemCell(form, gameObject, itemUseable, true, false);
                        text3.text = resCommonReward.astRewardInfo[1].iCnt.ToString();
                        text4.text = resCommonReward.astRewardInfo[2].iCnt.ToString();
                    }
                }
                if (this.m_personInfo != null)
                {
                    Text text6 = form.GetWidget(1).transform.FindChild("ScoreTxt").GetComponent<Text>();
                    GameObject widget = form.GetWidget(2);
                    Text text7 = form.GetWidget(2).transform.FindChild("ScoreTxt").GetComponent<Text>();
                    COMDT_MATCHPOINT mapPersonMatchPoint = this.GetMapPersonMatchPoint();
                    if (mapPersonMatchPoint != null)
                    {
                        text6.text = mapPersonMatchPoint.dwDayPoint.ToString();
                        text7.text = mapPersonMatchPoint.dwSeasonPoint.ToString();
                    }
                    if ((this.m_selectMapRes != null) && (this.m_selectMapRes.dwSeasonRankID == 0))
                    {
                        widget.CustomSetActive(false);
                    }
                }
                ResRewardMatchTimeInfo info = null;
                GameDataMgr.matchTimeInfoDict.TryGetValue(GameDataMgr.GetDoubleKey(5, this.m_selectMapID), out info);
                if (info != null)
                {
                    form.GetWidget(3).GetComponent<Text>().text = info.szTimeTips;
                }
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
                if (masterRoleInfo != null)
                {
                    CUseableContainer useableContainer = masterRoleInfo.GetUseableContainer(enCONTAINER_TYPE.ITEM);
                    if (this.m_selectMapRes != null)
                    {
                        int useableStackCount = useableContainer.GetUseableStackCount(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, this.m_selectMapRes.dwConsumPayItemID);
                        int bCount = useableContainer.GetUseableStackCount(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, this.m_selectMapRes.dwConsumFreeItemID);
                        CUseable useable2 = CUseableManager.CreateUseable(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, this.m_selectMapRes.dwConsumPayItemID, useableStackCount);
                        CUseable useable3 = CUseableManager.CreateUseable(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, this.m_selectMapRes.dwConsumFreeItemID, bCount);
                        GameObject obj4 = form.GetWidget(7);
                        GameObject itemCell = obj4.transform.Find("item1/itemCell").gameObject;
                        Text text9 = obj4.transform.Find("item1/itemCell/lblIconCount").GetComponent<Text>();
                        GameObject obj6 = obj4.transform.Find("item2/itemCell").gameObject;
                        Text text10 = obj4.transform.Find("item2/itemCell/lblIconCount").GetComponent<Text>();
                        CUICommonSystem.SetItemCell(form, itemCell, useable2, true, false);
                        CUICommonSystem.SetItemCell(form, obj6, useable3, true, false);
                        text9.text = "x" + useableStackCount.ToString();
                        text10.text = "x" + bCount.ToString();
                        text9.gameObject.CustomSetActive(true);
                        text10.gameObject.CustomSetActive(true);
                        CUICommonSystem.SetHostHeadItemCell(obj4.transform.FindChild("HeadItemCell").gameObject);
                        GameObject obj8 = form.GetWidget(9);
                        GameObject obj9 = obj8.transform.Find("itemCell").gameObject;
                        Text text11 = obj8.transform.Find("lblCount").GetComponent<Text>();
                        CUICommonSystem.SetItemCell(form, obj9, useable2, false, false);
                        text11.text = "x" + this.m_selectMapRes.dwCousumItemNum.ToString();
                    }
                }
            }
        }

        public bool IsUnionFuncLocked()
        {
            return !Singleton<CFunctionUnlockSys>.GetInstance().FucIsUnlock(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_REWARDMATCH);
        }

        private void OnBuyPickDialogConfirm(CUIEvent uiEvent, uint count)
        {
            int bCount = (int) count;
            uint tagUInt = uiEvent.m_eventParams.tagUInt;
            int tag = uiEvent.m_eventParams.tag;
            CUseable useable = CUseableManager.CreateUseable(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, tagUInt, bCount);
            if (useable != null)
            {
                int num4 = (int) (useable.GetBuyPrice((RES_SHOPBUY_COINTYPE) tag) * bCount);
                enPayType payType = CMallSystem.ResBuyTypeToPayType(tag);
                stUIEventParams confirmEventParams = new stUIEventParams {
                    tag = bCount
                };
                string[] args = new string[] { bCount.ToString(), useable.m_name };
                CMallSystem.TryToPay(enPayPurpose.Buy, Singleton<CTextManager>.GetInstance().GetText("Union_Battle_Tips5", args), payType, (uint) num4, enUIEventID.Union_Battle_ConfirmBuyItem, ref confirmEventParams, enUIEventID.None, true, true);
            }
        }

        private void OnBuyTiketClick(CUIEvent uiEvt)
        {
            CUIEvent uieventPars = new CUIEvent();
            stUIEventParams @params = new stUIEventParams {
                tagUInt = this.m_selectMapRes.dwConsumPayItemID,
                tag = this.m_selectMapRes.bCoinType
            };
            uieventPars.m_srcFormScript = uiEvt.m_srcFormScript;
            uieventPars.m_srcWidget = uiEvt.m_srcWidget;
            uieventPars.m_eventParams = @params;
            BuyPickDialog.Show(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, @params.tagUInt, (RES_SHOPBUY_COINTYPE) @params.tag, 100, 0x63, null, null, new BuyPickDialog.OnConfirmBuyCommonDelegate(this.OnBuyPickDialogConfirm), uieventPars);
        }

        private void OnClickRule(CUIEvent uiEvt)
        {
            int unionBattleRuleId = this.m_unionBattleRuleId;
            Singleton<CUIManager>.GetInstance().OpenInfoForm(unionBattleRuleId);
        }

        private void OnClickStartMatch(CUIEvent uiEvt)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                CUseableContainer useableContainer = masterRoleInfo.GetUseableContainer(enCONTAINER_TYPE.ITEM);
                if (CUICommonSystem.IsMatchOpened(RES_BATTLE_MAP_TYPE.RES_BATTLE_MAP_TYPE_REWARDMATCH, this.m_selectMapID))
                {
                    int num = useableContainer.GetUseableStackCount(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, this.m_selectMapRes.dwConsumPayItemID) + useableContainer.GetUseableStackCount(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, this.m_selectMapRes.dwConsumFreeItemID);
                    int dwCousumItemNum = (int) this.m_selectMapRes.dwCousumItemNum;
                    if (num >= dwCousumItemNum)
                    {
                        this.SendBeginMatchReq();
                    }
                    else
                    {
                        int bCount = dwCousumItemNum - num;
                        CUseable useable = CUseableManager.CreateUseable(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, this.m_selectMapRes.dwConsumPayItemID, bCount);
                        if (useable != null)
                        {
                            int num4 = (int) (useable.GetBuyPrice((RES_SHOPBUY_COINTYPE) this.m_selectMapRes.bCoinType) * bCount);
                            enPayType payType = CMallSystem.ResBuyTypeToPayType(this.m_selectMapRes.bCoinType);
                            stUIEventParams confirmEventParams = new stUIEventParams {
                                tag = bCount
                            };
                            string[] args = new string[] { bCount.ToString(), useable.m_name };
                            CMallSystem.TryToPay(enPayPurpose.Buy, Singleton<CTextManager>.GetInstance().GetText("Union_Battle_Tips5", args), payType, (uint) num4, enUIEventID.Union_Battle_ConfirmBuyItem, ref confirmEventParams, enUIEventID.None, true, true);
                        }
                    }
                }
                else
                {
                    Singleton<CUIManager>.instance.OpenTips("Union_Battle_Tips4", true, 1f, null, new object[0]);
                }
            }
        }

        private void OnConfirmBuyItem(CUIEvent uiEvt)
        {
            SendBuyTicketRequest(this.m_selectMapID, (uint) uiEvt.m_eventParams.tag);
        }

        private void OnRewardMatchTimeUp(CUIEvent uiEvt)
        {
            this.initSecondFormWidget();
        }

        private void Open_BattleEntry(CUIEvent uiEvt)
        {
            if (Singleton<CMatchingSystem>.instance.IsInMatching)
            {
                Singleton<CUIManager>.GetInstance().OpenTips("PVP_Matching", true, 1f, null, new object[0]);
            }
            else if (this.IsUnionFuncLocked())
            {
                Singleton<CUIManager>.instance.OpenTips("Union_Battle_Tips2", true, 1f, null, new object[0]);
            }
            else
            {
                CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(UNION_ENTRY_PATH, false, true);
                this.initFirstFormWidget();
                SendGetUnionBattleBaseInfoReq();
            }
        }

        private void Open_SecondBattleEntry(CUIEvent uiEvt)
        {
            switch (uiEvt.m_eventParams.tag)
            {
                case 0:
                {
                    CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(UNION_ENTRY_SECOND_PATH, false, true);
                    this.initSecondFormWidget();
                    break;
                }
                case 1:
                    Singleton<CUIManager>.instance.OpenTips("Union_Battle_Tips3", true, 1f, null, new object[0]);
                    break;
            }
        }

        private void Open_ThirdBattleEntry(CUIEvent uiEvt)
        {
            this.m_selectMapID = uiEvt.m_eventParams.tagUInt;
            this.m_selectMapRes = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey(this.m_selectMapID);
            GameDataMgr.matchTimeInfoDict.TryGetValue(GameDataMgr.GetDoubleKey(5, this.m_selectMapID), out this.m_selectTimeInfo);
            switch (uiEvt.m_eventParams.tag)
            {
                case 0:
                case 1:
                    if (uiEvt.m_eventParams.commonBool)
                    {
                        CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(UNION_ENTRY_THIRD_PATH, false, true);
                        this.initThirdFormWidget();
                        SendAwartPoolReq(this.m_selectMapID);
                        break;
                    }
                    Singleton<CUIManager>.instance.OpenTips("Union_Battle_Tips15", true, 1f, null, new object[0]);
                    return;

                case 2:
                    Singleton<CUIManager>.instance.OpenTips("Union_Battle_Tips3", true, 1f, null, new object[0]);
                    break;
            }
        }

        private void PlayCoinPoolAnimation()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(UNION_ENTRY_THIRD_PATH);
            if (form != null)
            {
                GameObject widget = form.GetWidget(8);
                Transform transform = widget.transform.Find("lblPoolCount");
                uint dwInitCoinPool = 0;
                byte bCoinType = 0;
                ResRewardMatchConf conf = null;
                GameDataMgr.rewardMatchRewardDict.TryGetValue(this.m_selectMapRes.dwDayRankID, out conf);
                ResRewardMatchDetailConf dataByKey = null;
                if (conf != null)
                {
                    dataByKey = GameDataMgr.unionRankRewardDetailDatabin.GetDataByKey(conf.dwRewardDetailId);
                    dwInitCoinPool = dataByKey.dwInitCoinPool;
                    bCoinType = dataByKey.bCoinType;
                    CUICommonSystem.SetTextContent(transform.gameObject, (this.m_awardPoolInfo.dwAwardPoolNum + dwInitCoinPool).ToString());
                    string str = string.Empty;
                    byte num4 = bCoinType;
                    switch (num4)
                    {
                        case 2:
                            str = "Dianquan";
                            break;

                        case 4:
                            str = "GoldCoin";
                            break;

                        default:
                            if (num4 == 10)
                            {
                                str = "Diamond";
                            }
                            break;
                    }
                    widget.transform.FindChild("lblPoolCount/Image").GetComponent<Image>().SetSprite("UGUI/Sprite/Common/" + str, form, true, false, false);
                }
            }
        }

        [MessageHandler(0x13ee)]
        public static void ReciveAwardPoolInfo(CSPkg msg)
        {
            Singleton<CUIManager>.instance.CloseSendMsgAlert();
            Singleton<CUnionBattleEntrySystem>.instance.m_awardPoolInfo = msg.stPkgData.stGetAwardPoolRsp;
            Singleton<CUnionBattleEntrySystem>.instance.PlayCoinPoolAnimation();
            Singleton<CUnionBattleRankSystem>.instance.SetRewardPoolInfo(msg.stPkgData.stGetAwardPoolRsp);
            Singleton<CUnionBattleRankSystem>.instance.RefreshDayReward();
        }

        [MessageHandler(0x13f1)]
        public static void ReciveBuyTicketInfo(CSPkg msg)
        {
            Singleton<CUIManager>.instance.CloseSendMsgAlert();
            if (msg.stPkgData.stBuyMatchTicketRsp.iResult == 0)
            {
                Singleton<CUnionBattleEntrySystem>.instance.initThirdFormWidget();
                Singleton<CUIManager>.instance.OpenTips("Union_Battle_Tips14", true, 1f, null, new object[0]);
            }
            else if (msg.stPkgData.stBuyMatchTicketRsp.iResult == 1)
            {
                Singleton<CUIManager>.instance.OpenTips("Union_Battle_Tips13", true, 1f, null, new object[0]);
            }
        }

        [MessageHandler(0x13ef)]
        public static void RecivePersonInfo(CSPkg msg)
        {
            Singleton<CUIManager>.instance.CloseSendMsgAlert();
            Singleton<CUnionBattleEntrySystem>.instance.m_personInfo = msg.stPkgData.stMatchPointNtf;
        }

        [MessageHandler(0x13f3)]
        public static void ReciveUnionBattleBaseInfo(CSPkg msg)
        {
            Singleton<CUIManager>.instance.CloseSendMsgAlert();
            Singleton<CUnionBattleEntrySystem>.instance.m_baseInfo = msg.stPkgData.stGetMatchInfoRsp;
            Singleton<CUnionBattleEntrySystem>.instance.initFirstFormWidget();
        }

        public static void SendAwartPoolReq(uint mapID)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x13ed);
            msg.stPkgData.stGetAwardPoolReq.dwMapId = mapID;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
        }

        private void SendBeginMatchReq()
        {
            CMatchingSystem.ReqStartSingleMatching(this.m_selectMapID, false, COM_BATTLE_MAP_TYPE.COM_BATTLE_MAP_TYPE_REWARDMATCH);
        }

        public static void SendBuyTicketRequest(uint mapID, uint itemNum)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x13f0);
            msg.stPkgData.stBuyMatchTicketReq.dwMapId = mapID;
            msg.stPkgData.stBuyMatchTicketReq.dwNum = itemNum;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void SendGetUnionBattleBaseInfoReq()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x13f2);
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        private void ShowCountDownTime(GameObject Btn, bool bMapOpened)
        {
            if (Btn != null)
            {
                Transform transform = Btn.transform;
                uint tagUInt = Btn.GetComponent<CUIMiniEventScript>().m_onClickEventParams.tagUInt;
                Transform transform2 = transform.FindChild("Desc");
                if (transform2 != null)
                {
                    GameObject gameObject = transform2.FindChild("Text").gameObject;
                    GameObject obj3 = transform2.FindChild("Timer").gameObject;
                    if (!bMapOpened)
                    {
                        obj3.CustomSetActive(false);
                        gameObject.CustomSetActive(false);
                    }
                    else
                    {
                        int utilOpenDay = 0;
                        uint utilOpenSec = 0;
                        CUICommonSystem.GetTimeUtilOpen(RES_BATTLE_MAP_TYPE.RES_BATTLE_MAP_TYPE_REWARDMATCH, tagUInt, out utilOpenSec, out utilOpenDay);
                        if ((utilOpenDay == 0) && (utilOpenSec == 0))
                        {
                            obj3.CustomSetActive(false);
                            gameObject.CustomSetActive(true);
                            gameObject.GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("Union_Battle_Tips6");
                        }
                        else if (utilOpenSec < SECOND_ONE_DAY)
                        {
                            gameObject.CustomSetActive(false);
                            obj3.CustomSetActive(true);
                            CUITimerScript component = obj3.transform.FindChild("Text").GetComponent<CUITimerScript>();
                            component.SetTotalTime((float) utilOpenSec);
                            component.StartTimer();
                        }
                        else
                        {
                            int num4 = utilOpenDay;
                            gameObject.CustomSetActive(true);
                            obj3.CustomSetActive(false);
                            string text = Singleton<CTextManager>.GetInstance().GetText("Union_Battle_Tips7");
                            gameObject.GetComponent<Text>().text = string.Format(text, num4);
                        }
                    }
                }
            }
        }

        public override void UnInit()
        {
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_ClickEntry, new CUIEventManager.OnUIEventHandler(this.Open_BattleEntry));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_BattleEntryGroup_Click, new CUIEventManager.OnUIEventHandler(this.Open_SecondBattleEntry));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_SubBattleEntryGroup_Click, new CUIEventManager.OnUIEventHandler(this.Open_ThirdBattleEntry));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_Click_SingleStartMatch, new CUIEventManager.OnUIEventHandler(this.OnClickStartMatch));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_ConfirmBuyItem, new CUIEventManager.OnUIEventHandler(this.OnConfirmBuyItem));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_Click_Rule, new CUIEventManager.OnUIEventHandler(this.OnClickRule));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Union_Battle_RewardMatch_TimeUp, new CUIEventManager.OnUIEventHandler(this.OnRewardMatchTimeUp));
        }

        private enum enUnionBattleEntryWidget
        {
            enEntry_Btn1,
            enEntry_Btn2,
            enEntry_PlayerCount
        }

        private enum enUnionSecBattleEntryWidget
        {
            enEntry_Btn1,
            enEntry_Btn2,
            enEntry_Btn3
        }

        private enum enUnionThirdEntryWidget
        {
            enEntry_MapName,
            enEntry_DayPoint,
            enEntry_SeasonPoint,
            enEntry_OpenTimeDesc,
            enEntry_RewardMoney,
            enEntry_RewardAcntPoint,
            enEntry_RewardTeamPoint,
            enEntry_RewardMatchTicketPanel,
            enEntry_CoinPool_Panle,
            enEntry_FightPanle
        }
    }
}

