namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.UI;

    [MessageHandlerClass]
    public class CMallRouletteController : Singleton<CMallRouletteController>
    {
        [CompilerGenerated]
        private static Comparison<ResDT_LuckyDrawExternReward> <>f__am$cache12;
        public const byte ACCELERATE_STEPS = 4;
        public const byte CONTINUOUS_DRAW_MIN_STEPS = 1;
        public const byte DECELERATE_STEPS = 4;
        public const float FASTEST_SPEED = 0.03f;
        public const int LEAST_LOOPS = 2;
        private byte m_CurContinousDrawSteps;
        private int m_CurLoops;
        private int m_CurRewardIdx;
        private int m_CurSpinCnt;
        private int m_CurSpinIdx;
        private Roulette_State m_CurState;
        private Tab m_CurTab;
        private DictionaryView<uint, ListView<ResDT_LuckyDrawExternReward>> m_ExternRewardDic;
        private bool m_GotAllUnusualItems;
        private bool m_IsClockwise;
        private bool m_IsContinousDraw;
        private bool m_IsLuckyBarInited;
        private SCPKG_LUCKYDRAW_RSP m_LuckyDrawRsp;
        private DictionaryView<uint, ListView<ResLuckyDrawRewardForClient>> m_RewardDic;
        private ListView<CUseable> m_RewardList;
        public Dictionary<uint, uint> m_RewardPoolDic;
        private List<Tab> m_UsedTabs;
        public const int MAX_EXTERN_REWARD_LIST_CNT = 5;
        public const int MAX_LUCK_CNT = 200;
        public const float NORMAL_SPEED = 0.03f;
        public static int reqSentTimerSeq = -1;
        public const ushort ROULETTE_RULE_ID = 2;
        public const float SLOWEST_SPEED = 0.1f;

        public void DisplayTmpRewardList(bool isShow, int amount = 0)
        {
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                GameObject p = Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette/tmpRewardList");
                CUIListScript componetInChild = Utility.GetComponetInChild<CUIListScript>(p, "List");
                if (componetInChild != null)
                {
                    if (isShow)
                    {
                        p.CustomSetActive(true);
                        Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette/Luck/Effect").CustomSetActive(false);
                        if (amount >= 1)
                        {
                            Singleton<CSoundManager>.GetInstance().PostEvent("UI_buy_chou_duobao_degao", null);
                            componetInChild.SetElementAmount(amount);
                            componetInChild.MoveElementInScrollArea(amount - 1, false);
                        }
                    }
                    else
                    {
                        p.CustomSetActive(false);
                    }
                }
                else
                {
                    p.CustomSetActive(false);
                }
            }
        }

        public void Draw(CUIFormScript form)
        {
            CUIEventManager instance = Singleton<CUIEventManager>.GetInstance();
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Tab_Change, new CUIEventManager.OnUIEventHandler(this.OnRouletteTabChange));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Buy_One, new CUIEventManager.OnUIEventHandler(this.OnRouletteBuyOne));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Buy_Five, new CUIEventManager.OnUIEventHandler(this.OnRouletteBuyFive));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Buy_One_Confirm, new CUIEventManager.OnUIEventHandler(this.OnRouletteBuyOneConfirm));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Buy_Five_Confirm, new CUIEventManager.OnUIEventHandler(this.OnRouletteBuyFiveConfirm));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Spin_Change, new CUIEventManager.OnUIEventHandler(this.OnSpinInterval));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Spin_End, new CUIEventManager.OnUIEventHandler(this.OnSpinEnd));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Open_Extern_Reward_Tip, new CUIEventManager.OnUIEventHandler(this.OnOpenExternRewardTip));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Draw_Extern_Reward, new CUIEventManager.OnUIEventHandler(this.OnDrawExternReward));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Reset_Reward_Draw_Cnt, new CUIEventManager.OnUIEventHandler(this.OnDrawCntReset));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Show_Reward_Item, new CUIEventManager.OnUIEventHandler(this.OnShowRewardItem));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Show_Rule, new CUIEventManager.OnUIEventHandler(this.OnShowRule));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Tmp_Reward_Enable, new CUIEventManager.OnUIEventHandler(this.OnTmpRewardEnable));
            instance.AddUIEventListener(enUIEventID.Mall_Skip_Animation, new CUIEventManager.OnUIEventHandler(this.OnSkipAnimation));
            instance.AddUIEventListener(enUIEventID.Mall_Roulette_Close_Award_Form, new CUIEventManager.OnUIEventHandler(this.OnCloseAwardForm));
            Singleton<EventRouter>.instance.AddEventHandler(EventID.Mall_Receive_Roulette_Data, new Action(this, (IntPtr) this.RefreshExternRewards));
            Singleton<EventRouter>.instance.AddEventHandler(EventID.Mall_Change_Tab, new Action(this, (IntPtr) this.OnMallTabChange));
            Singleton<EventRouter>.instance.AddEventHandler<uint>("HeroAdd", new Action<uint>(this.OnNtyAddHero));
            Singleton<EventRouter>.instance.AddEventHandler<uint, uint, uint>("HeroSkinAdd", new Action<uint, uint, uint>(this, (IntPtr) this.OnNtyAddSkin));
            this.InitElements();
            this.RefreshData(0);
            this.InitTab();
        }

        private int GetNextRefreshTime()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo == null)
            {
                return 0x93a80;
            }
            DateTime time = Utility.ToUtcTime2Local((long) masterRoleInfo.getCurrentTimeSinceLogin());
            DateTime time2 = new DateTime(0x7b2, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            ResGlobalInfo dataByKey = GameDataMgr.globalInfoDatabin.GetDataByKey(0x67);
            int num = 0;
            if ((time.DayOfWeek - ((DayOfWeek) dataByKey.dwConfValue)) > DayOfWeek.Sunday)
            {
                num = 7 - ((int) (time.DayOfWeek - ((DayOfWeek) dataByKey.dwConfValue)));
            }
            else
            {
                num = (int) (((DayOfWeek) dataByKey.dwConfValue) - time.DayOfWeek);
            }
            DateTime time3 = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, DateTimeKind.Utc);
            time3 = time3.AddDays((double) num);
            int num2 = 0;
            int num3 = 0;
            ResGlobalInfo info3 = GameDataMgr.globalInfoDatabin.GetDataByKey(0x68);
            num2 = (int) (info3.dwConfValue / 100);
            num3 = (int) (info3.dwConfValue % 100);
            time3 = time3.AddSeconds((num2 * 0xe10) + (num3 * 60));
            if (time.Subtract(time3).TotalSeconds > 0.0)
            {
                time3 = time3.AddDays(7.0);
            }
            return (int) time3.Subtract(time).TotalSeconds;
        }

        private stPayInfo GetPayInfo(RES_SHOPDRAW_SUBTYPE drawType = 2)
        {
            stPayInfo info = new stPayInfo {
                m_payType = CMallSystem.ResBuyTypeToPayType(10)
            };
            switch (this.CurTab)
            {
                case Tab.DianQuan:
                    info.m_payType = CMallSystem.ResBuyTypeToPayType(2);
                    break;

                case Tab.Diamond:
                    info.m_payType = CMallSystem.ResBuyTypeToPayType(10);
                    break;
            }
            ResLuckyDrawPrice price = new ResLuckyDrawPrice();
            RES_SHOPDRAW_SUBTYPE res_shopdraw_subtype = drawType;
            if (res_shopdraw_subtype != RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_ONE)
            {
                if (res_shopdraw_subtype != RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_FIVE)
                {
                    return info;
                }
            }
            else
            {
                if (GameDataMgr.mallRoulettePriceDict.TryGetValue(info.m_payType, out price))
                {
                    info.m_payValue = price.dwSinglePrice;
                    return info;
                }
                info.m_payValue = uint.MaxValue;
                return info;
            }
            if (GameDataMgr.mallRoulettePriceDict.TryGetValue(info.m_payType, out price))
            {
                info.m_payValue = price.dwMultiPrice;
                return info;
            }
            info.m_payValue = uint.MaxValue;
            return info;
        }

        private ResDT_LuckyDrawPeriod GetPeriodCfg(ResLuckyDrawPrice price)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                DateTime time = Utility.ToUtcTime2Local((long) masterRoleInfo.getCurrentTimeSinceLogin());
                DateTime now = DateTime.Now;
                DateTime time3 = DateTime.Now;
                Regex regex = new Regex(@"(\d{4})(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})");
                for (int i = 0; i < price.astLuckyDrawPeriod.Length; i++)
                {
                    ulong ullStartDate = price.astLuckyDrawPeriod[i].ullStartDate;
                    ulong ullEndDate = price.astLuckyDrawPeriod[i].ullEndDate;
                    if (ullStartDate > ullEndDate)
                    {
                        ullStartDate ^= ullEndDate;
                        ullEndDate ^= ullStartDate;
                        ullStartDate = ullEndDate ^ ullStartDate;
                    }
                    string input = ullStartDate.ToString();
                    string str2 = ullEndDate.ToString();
                    if (regex.IsMatch(input))
                    {
                        now = DateTime.Parse(regex.Replace(input, "$1-$2-$3 $4:$5:$6"));
                        if (regex.IsMatch(str2))
                        {
                            time3 = DateTime.Parse(regex.Replace(str2, "$1-$2-$3 $4:$5:$6"));
                            if ((DateTime.Compare(now, time) < 0) && (DateTime.Compare(time, time3) < 0))
                            {
                                return price.astLuckyDrawPeriod[i];
                            }
                        }
                    }
                }
            }
            return null;
        }

        public ListView<CUseable> GetRewardUseables(SCPKG_LUCKYDRAW_RSP stLuckyDrawRsp)
        {
            ListView<CUseable> view = new ListView<CUseable>();
            if (stLuckyDrawRsp != null)
            {
                ResLuckyDrawRewardForClient client = new ResLuckyDrawRewardForClient();
                uint dwRewardPoolID = stLuckyDrawRsp.dwRewardPoolID;
                long key = 0L;
                for (int i = 0; i < stLuckyDrawRsp.bRewardCnt; i++)
                {
                    CHeroInfoData data2;
                    ResGlobalInfo dataByKey;
                    ResHeroSkin heroSkin;
                    ResGlobalInfo info3;
                    uint num7;
                    key = GameDataMgr.GetDoubleKey(dwRewardPoolID, stLuckyDrawRsp.szRewardIndex[i]);
                    if (!GameDataMgr.mallRouletteRewardDict.TryGetValue(key, out client))
                    {
                        continue;
                    }
                    CUseable item = CUseableManager.CreateUsableByRandowReward((RES_RANDOM_REWARD_TYPE) client.dwItemType, (int) client.dwItemCnt, client.dwItemID);
                    if (item == null)
                    {
                        continue;
                    }
                    switch (item.m_type)
                    {
                        case COM_ITEM_TYPE.COM_OBJTYPE_HERO:
                        {
                            IHeroData data = CHeroDataFactory.CreateHeroData(client.dwItemID);
                            if (!data.bPlayerOwn)
                            {
                                goto Label_01C2;
                            }
                            data2 = (CHeroInfoData) data;
                            dataByKey = GameDataMgr.globalInfoDatabin.GetDataByKey(0x86);
                            DebugHelper.Assert(dataByKey != null, "global cfg databin err: hero exchange id doesnt exist");
                            if (dataByKey != null)
                            {
                                break;
                            }
                            return null;
                        }
                        case COM_ITEM_TYPE.COM_OBJTYPE_HEROSKIN:
                        {
                            uint heroId = 0;
                            uint skinId = 0;
                            CSkinInfo.ResolveHeroSkin(client.dwItemID, out heroId, out skinId);
                            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                            if ((masterRoleInfo == null) || !masterRoleInfo.IsHaveHeroSkin(heroId, skinId, false))
                            {
                                goto Label_01C2;
                            }
                            heroSkin = CSkinInfo.GetHeroSkin(heroId, skinId);
                            info3 = GameDataMgr.globalInfoDatabin.GetDataByKey(0x87);
                            DebugHelper.Assert(info3 != null, "global cfg databin err: hero skin exchange id doesnt exist");
                            if (info3 != null)
                            {
                                goto Label_018E;
                            }
                            return null;
                        }
                        default:
                            goto Label_01C2;
                    }
                    uint dwConfValue = dataByKey.dwConfValue;
                    item = CUseableManager.CreateUseable(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, dwConfValue, (int) data2.m_info.cfgInfo.dwChgItemCnt);
                    item.ExtraFromType = 1;
                    item.ExtraFromData = (int) client.dwItemID;
                    goto Label_01C2;
                Label_018E:
                    num7 = info3.dwConfValue;
                    item = CUseableManager.CreateUseable(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, num7, (int) heroSkin.dwChgItemCnt);
                    item.ExtraFromType = 2;
                    item.ExtraFromData = (int) client.dwItemID;
                Label_01C2:
                    if (item != null)
                    {
                        item.m_itemSortNum = client.dwItemPreciousValue;
                        view.Add(item);
                    }
                }
            }
            return view;
        }

        public override void Init()
        {
            base.Init();
            this.m_RewardDic = new DictionaryView<uint, ListView<ResLuckyDrawRewardForClient>>();
            this.m_ExternRewardDic = new DictionaryView<uint, ListView<ResDT_LuckyDrawExternReward>>();
            this.m_RewardPoolDic = new Dictionary<uint, uint>();
            this.m_UsedTabs = new List<Tab>();
            this.m_CurLoops = 0;
            this.m_CurSpinIdx = 0;
            this.m_CurRewardIdx = 0;
            this.m_CurSpinCnt = 0;
            this.m_CurContinousDrawSteps = 0;
            this.m_GotAllUnusualItems = false;
            this.m_CurState = Roulette_State.NONE;
            this.m_IsContinousDraw = false;
            this.m_LuckyDrawRsp = null;
            this.m_IsLuckyBarInited = false;
            reqSentTimerSeq = -1;
        }

        public void InitElements()
        {
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette").CustomSetActive(true);
            }
        }

        private void InitTab()
        {
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                this.m_UsedTabs.Clear();
                Tab[] values = (Tab[]) Enum.GetValues(typeof(Tab));
                for (byte i = 0; i < values.Length; i = (byte) (i + 1))
                {
                    if (values[i] != Tab.None)
                    {
                        switch (values[i])
                        {
                            case Tab.DianQuan:
                                if (this.m_RewardDic.ContainsKey(2))
                                {
                                    this.m_UsedTabs.Add(values[i]);
                                }
                                break;

                            case Tab.Diamond:
                                goto Label_0099;
                        }
                    }
                    continue;
                Label_0099:
                    if (this.m_RewardDic.ContainsKey(10))
                    {
                        this.m_UsedTabs.Add(values[i]);
                    }
                }
                DebugHelper.Assert(this.m_UsedTabs.Count != 0, "夺宝单价设定数据档不对");
                if (this.m_UsedTabs.Count != 0)
                {
                    CUIListScript componetInChild = Utility.GetComponetInChild<CUIListScript>(mallForm.gameObject, "pnlBodyBg/pnlRoulette/Tab");
                    if (componetInChild != null)
                    {
                        CUIListElementScript elemenet = null;
                        componetInChild.SetElementAmount(this.m_UsedTabs.Count);
                        for (int j = 0; j < componetInChild.m_elementAmount; j++)
                        {
                            elemenet = componetInChild.GetElemenet(j);
                            if (elemenet != null)
                            {
                                Tab tab = this.m_UsedTabs[j];
                                Text component = elemenet.transform.Find("Text").GetComponent<Text>();
                                Image image = elemenet.transform.Find("Image").GetComponent<Image>();
                                switch (tab)
                                {
                                    case Tab.DianQuan:
                                        component.text = Singleton<CTextManager>.GetInstance().GetText("Mall_Roulette_DianQuan_Buy_Tab");
                                        if (image != null)
                                        {
                                            image.SetSprite(CMallSystem.GetPayTypeIconPath(enPayType.DianQuan), mallForm, true, false, false);
                                        }
                                        break;

                                    case Tab.Diamond:
                                        component.text = Singleton<CTextManager>.GetInstance().GetText("Mall_Roulette_Diamond_Buy_Tab");
                                        if (image != null)
                                        {
                                            image.SetSprite(CMallSystem.GetPayTypeIconPath(enPayType.Diamond), mallForm, true, false, false);
                                        }
                                        break;
                                }
                            }
                        }
                        componetInChild.m_alwaysDispatchSelectedChangeEvent = true;
                        if (((this.CurTab == Tab.None) || (this.CurTab < Tab.DianQuan)) || (this.CurTab >= componetInChild.GetElementAmount()))
                        {
                            componetInChild.SelectElement(0, true);
                        }
                        else
                        {
                            componetInChild.SelectElement((int) this.CurTab, true);
                        }
                    }
                }
            }
        }

        public void Load(CUIFormScript form)
        {
            Singleton<CMallSystem>.GetInstance().LoadUIPrefab("UGUI/Form/System/Mall/Roulette", "pnlRoulette", form.GetWidget(3), form);
        }

        public bool Loaded(CUIFormScript form)
        {
            if (Utility.FindChild(form.GetWidget(3), "pnlRoulette") == null)
            {
                return false;
            }
            return true;
        }

        private void OnCloseAwardForm(CUIEvent uiEvent)
        {
            this.RefreshRewards();
            this.RefreshExternRewards();
            this.DisplayTmpRewardList(false, 0);
            if (reqSentTimerSeq <= 0)
            {
                Singleton<CMallSystem>.GetInstance().ToggleActionMask(false, 30f);
                this.ToggleBtnGroup(true);
            }
        }

        private void OnDrawCntReset(CUIEvent uiEvent)
        {
            CSDT_LUCKYDRAW_INFO csdt_luckydraw_info = new CSDT_LUCKYDRAW_INFO();
            enPayType diamond = enPayType.Diamond;
            switch (this.CurTab)
            {
                case Tab.DianQuan:
                    diamond = enPayType.Diamond;
                    break;

                case Tab.Diamond:
                    diamond = enPayType.Diamond;
                    break;
            }
            CMallSystem.luckyDrawDic.TryGetValue(diamond, out csdt_luckydraw_info);
            if (csdt_luckydraw_info != null)
            {
                csdt_luckydraw_info.dwDrawMask = 0;
                csdt_luckydraw_info.dwReachMask = 0;
                csdt_luckydraw_info.dwCnt = 0;
            }
            this.RefreshExternRewards();
            this.RefreshTimer();
        }

        private void OnDrawExternReward(CUIEvent uiEvent)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x12c3);
            msg.stPkgData.stLuckyDrawExternReq.bMoneyType = (byte) uiEvent.m_eventParams.tag2;
            msg.stPkgData.stLuckyDrawExternReq.bExternIndex = (byte) uiEvent.m_eventParams.tag;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        private void OnMallTabChange()
        {
            CUIEventManager instance = Singleton<CUIEventManager>.GetInstance();
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Tab_Change, new CUIEventManager.OnUIEventHandler(this.OnRouletteTabChange));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Buy_One, new CUIEventManager.OnUIEventHandler(this.OnRouletteBuyOne));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Buy_Five, new CUIEventManager.OnUIEventHandler(this.OnRouletteBuyFive));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Buy_One_Confirm, new CUIEventManager.OnUIEventHandler(this.OnRouletteBuyOneConfirm));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Buy_Five_Confirm, new CUIEventManager.OnUIEventHandler(this.OnRouletteBuyFiveConfirm));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Spin_Change, new CUIEventManager.OnUIEventHandler(this.OnSpinInterval));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Spin_End, new CUIEventManager.OnUIEventHandler(this.OnSpinEnd));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Open_Extern_Reward_Tip, new CUIEventManager.OnUIEventHandler(this.OnOpenExternRewardTip));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Draw_Extern_Reward, new CUIEventManager.OnUIEventHandler(this.OnDrawExternReward));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Reset_Reward_Draw_Cnt, new CUIEventManager.OnUIEventHandler(this.OnDrawCntReset));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Show_Reward_Item, new CUIEventManager.OnUIEventHandler(this.OnShowRewardItem));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Show_Rule, new CUIEventManager.OnUIEventHandler(this.OnShowRule));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Tmp_Reward_Enable, new CUIEventManager.OnUIEventHandler(this.OnTmpRewardEnable));
            instance.RemoveUIEventListener(enUIEventID.Mall_Skip_Animation, new CUIEventManager.OnUIEventHandler(this.OnSkipAnimation));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Close_Award_Form, new CUIEventManager.OnUIEventHandler(this.OnCloseAwardForm));
            Singleton<EventRouter>.instance.RemoveEventHandler(EventID.Mall_Receive_Roulette_Data, new Action(this, (IntPtr) this.RefreshExternRewards));
            Singleton<EventRouter>.instance.RemoveEventHandler(EventID.Mall_Change_Tab, new Action(this, (IntPtr) this.OnMallTabChange));
            Singleton<EventRouter>.instance.RemoveEventHandler<uint>("HeroAdd", new Action<uint>(this.OnNtyAddHero));
            Singleton<EventRouter>.instance.RemoveEventHandler<uint, uint, uint>("HeroSkinAdd", new Action<uint, uint, uint>(this, (IntPtr) this.OnNtyAddSkin));
        }

        private void OnNtyAddHero(uint id)
        {
            this.RefreshRewards();
        }

        private void OnNtyAddSkin(uint heroId, uint skinId, uint addReason)
        {
            this.RefreshRewards();
        }

        private void OnOpenExternRewardTip(CUIEvent uiEvent)
        {
            if ((Singleton<CMallSystem>.GetInstance().m_MallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                int tag = uiEvent.m_eventParams.tag;
                ListView<ResDT_LuckyDrawExternReward> view = new ListView<ResDT_LuckyDrawExternReward>();
                CSDT_LUCKYDRAW_INFO csdt_luckydraw_info = new CSDT_LUCKYDRAW_INFO();
                switch (this.CurTab)
                {
                    case Tab.DianQuan:
                        CMallSystem.luckyDrawDic.TryGetValue(enPayType.DianQuan, out csdt_luckydraw_info);
                        this.m_ExternRewardDic.TryGetValue(2, out view);
                        break;

                    case Tab.Diamond:
                        CMallSystem.luckyDrawDic.TryGetValue(enPayType.Diamond, out csdt_luckydraw_info);
                        this.m_ExternRewardDic.TryGetValue(10, out view);
                        break;
                }
                if (((view != null) && (view.Count != 0)) && (tag <= view.Count))
                {
                    ResDT_LuckyDrawExternReward reward = view[tag];
                    ResRewardForWeal weal = new ResRewardForWeal();
                    ListView<CUseable> view2 = new ListView<CUseable>();
                    if (GameDataMgr.wealRewardDict.TryGetValue(reward.dwRewardID, out weal))
                    {
                        for (int i = 0; i < weal.astRewardDetail.Length; i++)
                        {
                            if ((weal.astRewardDetail[i].bItemType == 0) || (weal.astRewardDetail[i].bItemType >= 0x11))
                            {
                                break;
                            }
                            ListView<CUseable> collection = CUseableManager.CreateUsableListByRandowReward((RES_RANDOM_REWARD_TYPE) weal.astRewardDetail[i].bItemType, (int) weal.astRewardDetail[i].dwLowCnt, weal.astRewardDetail[i].dwItemID);
                            if ((collection != null) && (collection.Count > 0))
                            {
                                view2.AddRange(collection);
                            }
                        }
                        byte result = 0;
                        byte num4 = 0;
                        if (csdt_luckydraw_info != null)
                        {
                            string str = Convert.ToString((long) csdt_luckydraw_info.dwReachMask, 2).PadLeft(0x20, '0');
                            string str2 = Convert.ToString((long) csdt_luckydraw_info.dwDrawMask, 2).PadLeft(0x20, '0');
                            byte.TryParse(str.Substring(0x20 - (tag + 1), 1), out result);
                            byte.TryParse(str2.Substring(0x20 - (tag + 1), 1), out num4);
                        }
                        CUIFormScript formScript = Singleton<CUIManager>.GetInstance().OpenForm("UGUI/Form/System/Mall/Form_Roullete_ExternReward.prefab", false, true);
                        if (formScript != null)
                        {
                            stPayInfo payInfo = this.GetPayInfo(RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_ONE);
                            GameObject obj2 = Utility.FindChild(formScript.gameObject, "Panel/btnGroup/btnBuyOne");
                            if (obj2 != null)
                            {
                                stUIEventParams eventParams = new stUIEventParams();
                                CMallSystem.SetPayButton(formScript, obj2.transform as RectTransform, payInfo.m_payType, payInfo.m_payValue, enUIEventID.Mall_Roulette_Buy_One, ref eventParams);
                            }
                            Text componetInChild = Utility.GetComponetInChild<Text>(formScript.gameObject, "Panel/Centent/Desc/Count");
                            if ((componetInChild != null) && (csdt_luckydraw_info != null))
                            {
                                componetInChild.text = csdt_luckydraw_info.dwCnt.ToString();
                            }
                            GameObject obj3 = Utility.FindChild(formScript.gameObject, "Panel/Centent/BuyDesc");
                            GameObject obj4 = Utility.FindChild(formScript.gameObject, "Panel/btnGroup/btnConfirm");
                            Text text2 = Utility.GetComponetInChild<Text>(formScript.gameObject, "Panel/btnGroup/btnConfirm/Text");
                            Text text3 = Utility.GetComponetInChild<Text>(formScript.gameObject, "Panel/Centent/BuyDesc/Count");
                            GameObject obj5 = Utility.FindChild(formScript.gameObject, "Panel/btnGroup/btnGet");
                            if (result == 0)
                            {
                                obj3.CustomSetActive(true);
                                obj4.CustomSetActive(true);
                                obj5.CustomSetActive(false);
                                if (text2 != null)
                                {
                                    text2.text = "去完成";
                                }
                                if ((text3 != null) && (csdt_luckydraw_info != null))
                                {
                                    text3.text = string.Format("{0}", reward.dwDrawCnt - csdt_luckydraw_info.dwCnt);
                                }
                            }
                            else
                            {
                                obj3.CustomSetActive(false);
                                if (num4 == 0)
                                {
                                    obj4.CustomSetActive(false);
                                    obj5.CustomSetActive(true);
                                    CUIEventScript component = obj5.GetComponent<CUIEventScript>();
                                    if (component == null)
                                    {
                                        component = obj5.AddComponent<CUIEventScript>();
                                        component.Initialize(formScript);
                                    }
                                    stUIEventParams params2 = new stUIEventParams {
                                        tag = tag,
                                        tag2 = (int) CMallSystem.PayTypeToResBuyCoinType(payInfo.m_payType)
                                    };
                                    component.SetUIEvent(enUIEventType.Click, enUIEventID.Mall_Roulette_Draw_Extern_Reward, params2);
                                }
                                else
                                {
                                    obj4.CustomSetActive(true);
                                    if (text2 != null)
                                    {
                                        text2.text = "已领取";
                                    }
                                    obj5.CustomSetActive(false);
                                }
                            }
                            int b = 5;
                            int num6 = Mathf.Min(view2.Count, b);
                            for (int j = 0; j < num6; j++)
                            {
                                GameObject itemCell = Utility.FindChild(formScript.gameObject, string.Format("Panel/Centent/itemCells/itemCell{0}", j + 1));
                                CUICommonSystem.SetItemCell(formScript, itemCell, view2[j], true, false);
                                itemCell.CustomSetActive(true);
                                itemCell.transform.FindChild("ItemName").GetComponent<Text>().text = view2[j].m_name;
                            }
                            for (int k = num6; k < b; k++)
                            {
                                Utility.FindChild(formScript.gameObject, string.Format("Panel/Centent/itemCells/itemCell{0}", k + 1)).CustomSetActive(false);
                            }
                        }
                    }
                }
            }
        }

        private void OnRouletteBuyFive(CUIEvent uiEvent)
        {
            if ((Singleton<CMallSystem>.GetInstance().m_MallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                if (Singleton<CMatchingSystem>.GetInstance().IsInMatching)
                {
                    GameObject widget = Singleton<CMallSystem>.GetInstance().m_MallForm.GetWidget(3);
                    Singleton<CUIManager>.GetInstance().OpenTips("PVP_Matching", true, 1f, widget, new object[0]);
                }
                else
                {
                    stPayInfo payInfo = this.GetPayInfo(RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_FIVE);
                    CUIEvent uIEvent = Singleton<CUIEventManager>.GetInstance().GetUIEvent();
                    uIEvent.m_eventID = enUIEventID.Mall_Roulette_Buy_Five_Confirm;
                    if (payInfo.m_payType == enPayType.Diamond)
                    {
                        CMallSystem.TryToPay(enPayPurpose.Roulette, string.Empty, enPayType.DiamondAndDianQuan, payInfo.m_payValue, uIEvent.m_eventID, ref uIEvent.m_eventParams, enUIEventID.None, false, true);
                    }
                    else
                    {
                        CMallSystem.TryToPay(enPayPurpose.Roulette, string.Empty, payInfo.m_payType, payInfo.m_payValue, uIEvent.m_eventID, ref uIEvent.m_eventParams, enUIEventID.None, false, true);
                    }
                }
            }
        }

        private void OnRouletteBuyFiveConfirm(CUIEvent uiEvent)
        {
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                Singleton<CMallSystem>.GetInstance().ToggleActionMask(true, 10f);
                int num = this.m_CurSpinIdx % 14;
                GameObject target = Utility.FindChild(mallForm.gameObject, string.Format("pnlBodyBg/pnlRoulette/rewardBody/rewardContainer/itemCell{0}/halo", num));
                if (target != null)
                {
                    CUICommonSystem.PlayAnimator(target, "Roulette_Halo_Disappear");
                }
                this.m_IsContinousDraw = true;
                this.Spin(-1);
                this.ToggleBtnGroup(false);
                this.SendLotteryMsg(RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_FIVE);
            }
        }

        private void OnRouletteBuyOne(CUIEvent uiEvent)
        {
            if ((Singleton<CMallSystem>.GetInstance().m_MallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                if (Singleton<CMatchingSystem>.GetInstance().IsInMatching)
                {
                    GameObject widget = Singleton<CMallSystem>.GetInstance().m_MallForm.GetWidget(3);
                    Singleton<CUIManager>.GetInstance().OpenTips("PVP_Matching", true, 1f, widget, new object[0]);
                }
                else
                {
                    stPayInfo payInfo = this.GetPayInfo(RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_ONE);
                    CUIEvent uIEvent = Singleton<CUIEventManager>.GetInstance().GetUIEvent();
                    uIEvent.m_eventID = enUIEventID.Mall_Roulette_Buy_One_Confirm;
                    if (payInfo.m_payType == enPayType.Diamond)
                    {
                        CMallSystem.TryToPay(enPayPurpose.Roulette, string.Empty, enPayType.DiamondAndDianQuan, payInfo.m_payValue, uIEvent.m_eventID, ref uIEvent.m_eventParams, enUIEventID.None, false, true);
                    }
                    else
                    {
                        CMallSystem.TryToPay(enPayPurpose.Roulette, string.Empty, payInfo.m_payType, payInfo.m_payValue, uIEvent.m_eventID, ref uIEvent.m_eventParams, enUIEventID.None, false, true);
                    }
                }
            }
        }

        private void OnRouletteBuyOneConfirm(CUIEvent uiEvent)
        {
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                Singleton<CMallSystem>.GetInstance().ToggleActionMask(true, 10f);
                int num = this.m_CurSpinIdx % 14;
                GameObject target = Utility.FindChild(mallForm.gameObject, string.Format("pnlBodyBg/pnlRoulette/rewardBody/rewardContainer/itemCell{0}/halo", num));
                if (target != null)
                {
                    CUICommonSystem.PlayAnimator(target, "Roulette_Halo_Disappear");
                }
                Singleton<CSoundManager>.GetInstance().PostEvent("UI_buy_chou_duobao_btu", null);
                this.m_IsContinousDraw = false;
                this.Spin(-1);
                this.ToggleBtnGroup(false);
                this.SendLotteryMsg(RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_ONE);
            }
        }

        private void OnRouletteTabChange(CUIEvent uiEvent)
        {
            int selectedIndex = uiEvent.m_srcWidget.GetComponent<CUIListScript>().GetSelectedIndex();
            if (selectedIndex <= this.m_UsedTabs.Count)
            {
                this.CurTab = this.m_UsedTabs[selectedIndex];
                this.RefreshRewards();
                this.m_IsLuckyBarInited = false;
                this.RefreshExternRewards();
                this.DisplayTmpRewardList(false, 0);
                this.RefreshButtonView();
                this.RefreshTimer();
            }
        }

        private void OnShowRewardItem(CUIEvent uiEvent)
        {
            if ((this.m_CurRewardIdx >= 0) && (this.m_CurRewardIdx < this.m_LuckyDrawRsp.bRewardCnt))
            {
                Utility.FindChild(uiEvent.m_srcFormScript.gameObject, string.Format("Panel/Centent/itemCell{0}", this.m_CurRewardIdx + 1)).CustomSetActive(true);
                Singleton<CSoundManager>.GetInstance().PostEvent("UI_buy_chou_get_N", null);
                this.m_CurRewardIdx++;
            }
            else
            {
                (uiEvent.m_srcWidgetScript as CUITimerScript).EndTimer();
            }
        }

        private void OnShowRule(CUIEvent uiEvent)
        {
            ResRuleText dataByKey = GameDataMgr.s_ruleTextDatabin.GetDataByKey(2);
            if (dataByKey != null)
            {
                string title = StringHelper.UTF8BytesToString(ref dataByKey.szTitle);
                string info = StringHelper.UTF8BytesToString(ref dataByKey.szContent);
                Singleton<CUIManager>.GetInstance().OpenInfoForm(title, info);
            }
        }

        private void OnSkipAnimation(CUIEvent uiEvent)
        {
            this.m_CurState = Roulette_State.SKIP;
        }

        private void OnSpinEnd(CUIEvent uiEvent)
        {
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                int tag = uiEvent.m_eventParams.tag;
                if (tag != -1)
                {
                    GameObject target = Utility.FindChild(mallForm.gameObject, string.Format("pnlBodyBg/pnlRoulette/rewardBody/rewardContainer/itemCell{0}/halo", tag));
                    if (target != null)
                    {
                        CUICommonSystem.PlayAnimator(target, "Roulette_Halo_Focus");
                    }
                }
                if (((this.m_CurState == Roulette_State.CONTINUOUS_DRAW) && (this.m_LuckyDrawRsp != null)) && (this.m_CurRewardIdx < this.m_LuckyDrawRsp.bRewardCnt))
                {
                    Singleton<CMallSystem>.GetInstance().ToggleSkipAnimationMask(true, 30f);
                    Singleton<CMallSystem>.GetInstance().ToggleActionMask(false, 30f);
                    this.m_CurRewardIdx++;
                    this.DisplayTmpRewardList(true, this.m_CurRewardIdx);
                    Debug.Log(string.Format("五连抽第{0}次", this.m_CurRewardIdx));
                    if (this.m_CurRewardIdx < this.m_LuckyDrawRsp.bRewardCnt)
                    {
                        if (Math.Abs((int) (this.m_LuckyDrawRsp.szRewardIndex[this.m_CurRewardIdx] - tag)) > 7)
                        {
                            if ((this.m_LuckyDrawRsp.szRewardIndex[this.m_CurRewardIdx] - tag) > 0)
                            {
                                this.m_IsClockwise = false;
                            }
                            else
                            {
                                this.m_IsClockwise = true;
                            }
                        }
                        else if ((this.m_LuckyDrawRsp.szRewardIndex[this.m_CurRewardIdx] - tag) < 0)
                        {
                            this.m_IsClockwise = false;
                        }
                        else
                        {
                            this.m_IsClockwise = true;
                        }
                        this.m_IsClockwise = true;
                        Singleton<CTimerManager>.GetInstance().AddTimer(500, 1, delegate (int sequence) {
                            this.m_CurContinousDrawSteps = 0;
                            this.Spin(this.m_LuckyDrawRsp.szRewardIndex[this.m_CurRewardIdx]);
                        });
                        return;
                    }
                }
                CUITimerScript componetInChild = Utility.GetComponetInChild<CUITimerScript>(mallForm.gameObject, "pnlBodyBg/pnlRoulette/timerSpin");
                if (componetInChild != null)
                {
                    componetInChild.m_eventIDs[1] = enUIEventID.None;
                    componetInChild.EndTimer();
                }
                Singleton<CMallSystem>.GetInstance().ToggleActionMask(true, 10f);
                Singleton<CMallSystem>.GetInstance().ToggleSkipAnimationMask(false, 30f);
                if ((this.m_LuckyDrawRsp != null) && (this.m_LuckyDrawRsp.bRewardCnt != 0))
                {
                    Singleton<CTimerManager>.GetInstance().AddTimer(600, 1, delegate (int sequence) {
                        string title = null;
                        switch (this.CurTab)
                        {
                            case Tab.DianQuan:
                                title = "点券夺宝奖励";
                                break;

                            case Tab.Diamond:
                                title = "钻石夺宝奖励";
                                break;
                        }
                        Singleton<CSoundManager>.GetInstance().PostEvent("Stop_UI_buy_chou_duobao_zhuan", null);
                        if (this.m_LuckyDrawRsp.bRewardCnt == 1)
                        {
                            this.OpenAwardTip(RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_ONE, this.m_RewardList, title, true, enUIEventID.None, false);
                        }
                        else
                        {
                            this.OpenAwardTip(RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_FIVE, this.m_RewardList, title, true, enUIEventID.None, false);
                        }
                        this.ShowHeroSkin(this.m_RewardList);
                    });
                }
            }
        }

        private void OnSpinInterval(CUIEvent uiEvent)
        {
            this.m_CurSpinCnt++;
            int num = Math.Abs((int) (this.m_CurSpinIdx % 14));
            if ((this.m_CurSpinCnt % 14) == 0)
            {
                this.m_CurLoops++;
            }
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                GameObject target = Utility.FindChild(mallForm.gameObject, string.Format("pnlBodyBg/pnlRoulette/rewardBody/rewardContainer/itemCell{0}/halo", num));
                if (target != null)
                {
                    CUICommonSystem.PlayAnimator(target, "Roulette_Halo_Appear");
                }
                CUITimerScript srcWidgetScript = uiEvent.m_srcWidgetScript as CUITimerScript;
                if (this.m_CurState == Roulette_State.ACCELERATE)
                {
                    float num2 = 0.1f - ((this.m_CurSpinCnt * 0.07f) / 4f);
                    if (num2 > 0.03f)
                    {
                        if (srcWidgetScript != null)
                        {
                            srcWidgetScript.SetOnChangedIntervalTime((num2 > 0.03f) ? num2 : 0.03f);
                        }
                    }
                    else
                    {
                        this.m_CurState = Roulette_State.UNIFORM;
                    }
                }
                if ((((this.m_CurState == Roulette_State.UNIFORM) && (uiEvent.m_eventParams.tag != -1)) && (this.m_CurLoops > 2)) && ((((uiEvent.m_eventParams.tag - 4) < 0) && (num == ((14 + uiEvent.m_eventParams.tag) - 4))) || (((uiEvent.m_eventParams.tag - 4) >= 0) && (num == (uiEvent.m_eventParams.tag - 4)))))
                {
                    this.m_CurState = Roulette_State.DECELERATE;
                    this.m_CurSpinCnt = 0;
                }
                if (this.m_CurState == Roulette_State.DECELERATE)
                {
                    float num3 = 0.03f + ((this.m_CurSpinCnt * 0.07f) / 4f);
                    if (((uiEvent.m_eventParams.tag == num) && (num3 >= 0.1f)) && (srcWidgetScript != null))
                    {
                        if (!this.m_IsContinousDraw)
                        {
                            this.m_CurState = Roulette_State.NONE;
                        }
                        else
                        {
                            this.m_CurState = Roulette_State.CONTINUOUS_DRAW;
                            this.DisplayTmpRewardList(true, 0);
                        }
                        srcWidgetScript.m_eventIDs[1] = enUIEventID.Mall_Roulette_Spin_End;
                        srcWidgetScript.m_eventParams[1].tag = uiEvent.m_eventParams.tag;
                        srcWidgetScript.SetOnChangedIntervalTime(0.03f);
                        srcWidgetScript.SetTotalTime(0f);
                        return;
                    }
                    if (srcWidgetScript != null)
                    {
                        srcWidgetScript.SetOnChangedIntervalTime((num3 < 0.1f) ? num3 : 0.1f);
                    }
                }
                if (((this.m_CurState == Roulette_State.CONTINUOUS_DRAW) || (this.m_CurState == Roulette_State.SKIP)) && ((uiEvent.m_eventParams.tag == num) && (this.m_CurContinousDrawSteps >= 1)))
                {
                    if (srcWidgetScript != null)
                    {
                        srcWidgetScript.m_eventIDs[1] = enUIEventID.Mall_Roulette_Spin_End;
                        srcWidgetScript.m_eventParams[1].tag = uiEvent.m_eventParams.tag;
                        srcWidgetScript.SetTotalTime(0f);
                    }
                }
                else
                {
                    if (this.m_IsClockwise)
                    {
                        this.m_CurSpinIdx++;
                    }
                    else
                    {
                        this.m_CurSpinIdx--;
                    }
                    this.m_CurContinousDrawSteps = (byte) (this.m_CurContinousDrawSteps + 1);
                }
            }
        }

        private void OnTmpRewardEnable(CUIEvent uiEvent)
        {
            int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
            if ((this.m_RewardList != null) && (srcWidgetIndexInBelongedList <= this.m_RewardList.Count))
            {
                GameObject gameObject = uiEvent.m_srcWidget.transform.Find("itemCell").gameObject;
                CUseable itemUseable = this.m_RewardList[srcWidgetIndexInBelongedList];
                CUICommonSystem.SetItemCell(uiEvent.m_srcFormScript, gameObject, itemUseable, false, false);
            }
        }

        public void OpenAwardTip(RES_SHOPDRAW_SUBTYPE drawType, ListView<CUseable> items, string title = null, bool playSound = false, enUIEventID eventID = 0, bool displayAll = false)
        {
            if (items != null)
            {
                int b = 5;
                int num2 = Mathf.Min(items.Count, b);
                this.m_CurRewardIdx = 0;
                CUIFormScript formScript = null;
                if (items.Count < 5)
                {
                    formScript = Singleton<CUIManager>.GetInstance().OpenForm("UGUI/Form/System/Mall/Form_Roullete_Reward.prefab", false, true);
                }
                else
                {
                    formScript = Singleton<CUIManager>.GetInstance().OpenForm("UGUI/Form/System/Mall/Form_Roullete_Reward_Five.prefab", false, true);
                }
                if (formScript == null)
                {
                    Singleton<CUIManager>.GetInstance().OpenTips("Error get reward form failed", false, 1f, null, new object[0]);
                }
                else
                {
                    CUIEventScript componetInChild = Utility.GetComponetInChild<CUIEventScript>(formScript.gameObject, "Panel/Button_Back");
                    if (componetInChild != null)
                    {
                        componetInChild.m_onClickEventID = eventID;
                    }
                    CUIEventScript script3 = Utility.GetComponetInChild<CUIEventScript>(formScript.gameObject, "Panel/CloseBtn");
                    if (script3 != null)
                    {
                        script3.m_onClickEventID = eventID;
                    }
                    GameObject obj2 = null;
                    stPayInfo payInfo = new stPayInfo();
                    stUIEventParams eventParams = new stUIEventParams();
                    enUIEventID none = enUIEventID.None;
                    switch (drawType)
                    {
                        case RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_ONE:
                            payInfo = this.GetPayInfo(RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_ONE);
                            Utility.FindChild(formScript.gameObject, "Panel/btnBuyFive").CustomSetActive(false);
                            obj2 = Utility.FindChild(formScript.gameObject, "Panel/btnBuyOne");
                            none = enUIEventID.Mall_Roulette_Buy_One;
                            break;

                        case RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_FIVE:
                            payInfo = this.GetPayInfo(RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_FIVE);
                            Utility.FindChild(formScript.gameObject, "Panel/btnBuyOne").CustomSetActive(false);
                            obj2 = Utility.FindChild(formScript.gameObject, "Panel/btnBuyFive");
                            none = enUIEventID.Mall_Roulette_Buy_Five;
                            break;
                    }
                    obj2.CustomSetActive(true);
                    if (obj2 != null)
                    {
                        CMallSystem.SetPayButton(formScript, obj2.transform as RectTransform, payInfo.m_payType, payInfo.m_payValue, none, ref eventParams);
                    }
                    if (title != null)
                    {
                        Utility.GetComponetInChild<Text>(formScript.gameObject, "Panel/bg/Title").text = title;
                    }
                    for (int i = 0; i < num2; i++)
                    {
                        formScript.transform.FindChild(string.Format("Panel/Centent/itemCell{0}", i + 1)).gameObject.CustomSetActive(false);
                    }
                    Transform transform = formScript.transform.Find("showRewardTimer");
                    CUITimerScript component = null;
                    if (transform != null)
                    {
                        component = transform.gameObject.GetComponent<CUITimerScript>();
                    }
                    if (component == null)
                    {
                        for (int j = 0; j < num2; j++)
                        {
                            GameObject gameObject = formScript.transform.FindChild(string.Format("Panel/Centent/itemCell{0}", j + 1)).gameObject;
                            CUICommonSystem.SetItemCell(formScript, gameObject, items[j], true, displayAll);
                            if (items[j].m_itemSortNum > 0L)
                            {
                                Utility.FindChild(gameObject, "Effect_glow").CustomSetActive(true);
                            }
                            gameObject.CustomSetActive(true);
                            Singleton<CSoundManager>.GetInstance().PostEvent("UI_buy_chou_get_01", null);
                            if (playSound)
                            {
                                COM_REWARDS_TYPE mapRewardType = items[j].MapRewardType;
                                if (mapRewardType != COM_REWARDS_TYPE.COM_REWARDS_TYPE_COIN)
                                {
                                    if (mapRewardType == COM_REWARDS_TYPE.COM_REWARDS_TYPE_AP)
                                    {
                                        goto Label_0332;
                                    }
                                    if (mapRewardType == COM_REWARDS_TYPE.COM_REWARDS_TYPE_DIAMOND)
                                    {
                                        goto Label_031C;
                                    }
                                }
                                else
                                {
                                    Singleton<CSoundManager>.GetInstance().PostEvent("UI_hall_add_coin", null);
                                }
                            }
                            continue;
                        Label_031C:
                            Singleton<CSoundManager>.GetInstance().PostEvent("UI_hall_add_diamond", null);
                            continue;
                        Label_0332:
                            Singleton<CSoundManager>.GetInstance().PostEvent("UI_hall_add_physical_power", null);
                        }
                    }
                    else
                    {
                        for (int k = 0; k < num2; k++)
                        {
                            GameObject itemCell = formScript.transform.FindChild(string.Format("Panel/Centent/itemCell{0}", k + 1)).gameObject;
                            CUICommonSystem.SetItemCell(formScript, itemCell, items[k], true, displayAll);
                            if (items[k].m_itemSortNum > 0L)
                            {
                                Utility.FindChild(itemCell, "Effect_glow").CustomSetActive(true);
                            }
                            itemCell.CustomSetActive(false);
                        }
                        component.SetTotalTime(100f);
                        component.StartTimer();
                    }
                }
            }
        }

        [MessageHandler(0x12c4)]
        public static void ReceiveDrawExternRewardRes(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            if (msg.stPkgData.stLuckyDrawExternRsp.iResult != 0)
            {
                Singleton<CUIManager>.GetInstance().OpenTips(Utility.ProtErrCodeToStr(0x12c4, msg.stPkgData.stLuckyDrawExternRsp.iResult), false, 1f, null, new object[0]);
            }
            else
            {
                ListView<CUseable> useableListFromReward = CUseableManager.GetUseableListFromReward(msg.stPkgData.stLuckyDrawExternRsp.stReward);
                if (useableListFromReward.Count != 0)
                {
                    Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.Mall_Receive_Roulette_Data);
                    Singleton<CUIManager>.GetInstance().OpenAwardTip(useableListFromReward.ToArray<CUseable>(), Singleton<CTextManager>.GetInstance().GetText("gotAward"), false, enUIEventID.None, false, false, "Form_Award");
                    Singleton<CMallRouletteController>.GetInstance().ShowHeroSkin(useableListFromReward);
                }
            }
        }

        [MessageHandler(0x12c2)]
        public static void ReceiveLotteryRes(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                if (msg.stPkgData.stLuckyDrawRsp.iResult != 0)
                {
                    CUITimerScript componetInChild = Utility.GetComponetInChild<CUITimerScript>(mallForm.gameObject, "pnlBodyBg/pnlRoulette/timerSpin");
                    if (componetInChild != null)
                    {
                        componetInChild.SetTotalTime(0f);
                    }
                    Singleton<CUIManager>.GetInstance().OpenTips(Utility.ProtErrCodeToStr(0x12c2, msg.stPkgData.stLuckyDrawRsp.iResult), false, 1f, null, new object[0]);
                    Singleton<CMallRouletteController>.GetInstance().ToggleBtnGroup(true);
                }
                else
                {
                    Singleton<CTimerManager>.GetInstance().RemoveTimerSafely(ref reqSentTimerSeq);
                    uint num = 0;
                    if (Singleton<CMallRouletteController>.GetInstance().m_RewardPoolDic.TryGetValue(msg.stPkgData.stLuckyDrawRsp.bMoneyType, out num))
                    {
                        if (num != msg.stPkgData.stLuckyDrawRsp.dwRewardPoolID)
                        {
                            Singleton<CMallRouletteController>.GetInstance().RefreshData(msg.stPkgData.stLuckyDrawRsp.dwRewardPoolID);
                        }
                        else
                        {
                            Singleton<CMallRouletteController>.GetInstance().RefreshData(0);
                        }
                    }
                    Singleton<CMallRouletteController>.GetInstance().SetRewardData(msg.stPkgData.stLuckyDrawRsp);
                    int idx = msg.stPkgData.stLuckyDrawRsp.szRewardIndex[0];
                    Singleton<CMallRouletteController>.GetInstance().Spin(idx);
                }
            }
        }

        private void RefreshButtonView()
        {
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                this.ToggleBtnGroup(true);
                GameObject obj2 = Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette/Luck/btnGroup/btnBuyOne");
                GameObject p = Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette/Luck/btnGroup/btnBuyFive");
                if ((obj2 != null) && (p != null))
                {
                    stPayInfo payInfo = this.GetPayInfo(RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_ONE);
                    stPayInfo info2 = this.GetPayInfo(RES_SHOPDRAW_SUBTYPE.RES_DRAWSUBTYPE_FIVE);
                    stUIEventParams eventParams = new stUIEventParams();
                    GameObject obj4 = Utility.FindChild(p, "tag");
                    ResLuckyDrawPrice price = new ResLuckyDrawPrice();
                    switch (this.CurTab)
                    {
                        case Tab.DianQuan:
                            GameDataMgr.mallRoulettePriceDict.TryGetValue(enPayType.DianQuan, out price);
                            break;

                        case Tab.Diamond:
                            GameDataMgr.mallRoulettePriceDict.TryGetValue(enPayType.Diamond, out price);
                            break;
                    }
                    obj4.CustomSetActive(false);
                    CMallSystem.SetPayButton(mallForm, obj2.transform as RectTransform, payInfo.m_payType, payInfo.m_payValue, enUIEventID.Mall_Roulette_Buy_One, ref eventParams);
                    CMallSystem.SetPayButton(mallForm, p.transform as RectTransform, payInfo.m_payType, info2.m_payValue, enUIEventID.Mall_Roulette_Buy_Five, ref eventParams);
                }
            }
        }

        public void RefreshData(uint targetPoolId = 0)
        {
            this.m_RewardDic.Clear();
            this.m_ExternRewardDic.Clear();
            DictionaryView<enPayType, ResLuckyDrawPrice>.Enumerator enumerator = GameDataMgr.mallRoulettePriceDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<enPayType, ResLuckyDrawPrice> current = enumerator.Current;
                ResLuckyDrawPrice price = current.Value;
                if (price != null)
                {
                    uint dwRewardPoolID = price.dwRewardPoolID;
                    if (targetPoolId != 0)
                    {
                        dwRewardPoolID = targetPoolId;
                    }
                    else
                    {
                        ResDT_LuckyDrawPeriod periodCfg = this.GetPeriodCfg(price);
                        if (periodCfg != null)
                        {
                            dwRewardPoolID = periodCfg.dwRewardPoolID;
                        }
                    }
                    DictionaryView<long, ResLuckyDrawRewardForClient>.Enumerator enumerator2 = GameDataMgr.mallRouletteRewardDict.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        KeyValuePair<long, ResLuckyDrawRewardForClient> pair2 = enumerator2.Current;
                        ResLuckyDrawRewardForClient item = pair2.Value;
                        if ((item != null) && (item.dwRewardPoolID == dwRewardPoolID))
                        {
                            if (!this.m_RewardDic.ContainsKey(price.bMoneyType))
                            {
                                this.m_RewardDic.Add(price.bMoneyType, new ListView<ResLuckyDrawRewardForClient>());
                            }
                            if (!this.m_RewardPoolDic.ContainsKey(price.bMoneyType))
                            {
                                this.m_RewardPoolDic.Add(price.bMoneyType, dwRewardPoolID);
                            }
                            else
                            {
                                uint num2 = 0;
                                if (this.m_RewardPoolDic.TryGetValue(price.bMoneyType, out num2))
                                {
                                    num2 = dwRewardPoolID;
                                }
                            }
                            ListView<ResLuckyDrawRewardForClient> view = new ListView<ResLuckyDrawRewardForClient>();
                            if (this.m_RewardDic.TryGetValue(price.bMoneyType, out view))
                            {
                                view.Add(item);
                            }
                        }
                    }
                    DictionaryView<uint, ResLuckyDrawExternReward>.Enumerator enumerator3 = GameDataMgr.mallRouletteExternRewardDict.GetEnumerator();
                    while (enumerator3.MoveNext())
                    {
                        KeyValuePair<uint, ResLuckyDrawExternReward> pair3 = enumerator3.Current;
                        ResLuckyDrawExternReward reward = pair3.Value;
                        if ((reward != null) && (reward.bMoneyType == price.bMoneyType))
                        {
                            if (!this.m_ExternRewardDic.ContainsKey(price.bMoneyType))
                            {
                                this.m_ExternRewardDic.Add(price.bMoneyType, new ListView<ResDT_LuckyDrawExternReward>());
                            }
                            ListView<ResDT_LuckyDrawExternReward> view2 = new ListView<ResDT_LuckyDrawExternReward>();
                            if (this.m_ExternRewardDic.TryGetValue(price.bMoneyType, out view2))
                            {
                                for (int i = 0; i < reward.bExternCnt; i++)
                                {
                                    ResDT_LuckyDrawExternReward reward2 = reward.astReward[i];
                                    view2.Add(reward2);
                                }
                                if (<>f__am$cache12 == null)
                                {
                                    <>f__am$cache12 = delegate (ResDT_LuckyDrawExternReward a, ResDT_LuckyDrawExternReward b) {
                                        if ((a == null) && (b == null))
                                        {
                                            return 0;
                                        }
                                        if ((a != null) || (b == null))
                                        {
                                            if ((b == null) && (a != null))
                                            {
                                                return 1;
                                            }
                                            if (a.dwDrawCnt < b.dwDrawCnt)
                                            {
                                                return -1;
                                            }
                                            if (a.dwDrawCnt == b.dwDrawCnt)
                                            {
                                                return 0;
                                            }
                                            if (a.dwDrawCnt > b.dwDrawCnt)
                                            {
                                                return 1;
                                            }
                                        }
                                        return -1;
                                    };
                                }
                                view2.Sort(<>f__am$cache12);
                            }
                        }
                    }
                }
            }
        }

        private void RefreshDrawCnt(enPayType payType, out CSDT_LUCKYDRAW_INFO drawInfo, int drawCnt = -1)
        {
            CMallSystem.luckyDrawDic.TryGetValue(payType, out drawInfo);
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                Text componetInChild = Utility.GetComponetInChild<Text>(Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette/PL_Premiums"), "Top/Value");
                if (componetInChild != null)
                {
                    if (drawCnt != -1)
                    {
                        componetInChild.text = drawCnt.ToString();
                    }
                    else if (drawInfo != null)
                    {
                        componetInChild.text = drawInfo.dwCnt.ToString();
                    }
                }
            }
        }

        private void RefreshExternRewards()
        {
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                GameObject p = Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette/PL_Premiums");
                ListView<ResDT_LuckyDrawExternReward> view = new ListView<ResDT_LuckyDrawExternReward>();
                CSDT_LUCKYDRAW_INFO drawInfo = new CSDT_LUCKYDRAW_INFO();
                if (p != null)
                {
                    if (Utility.GetComponetInChild<Text>(p, "Top/Value") != null)
                    {
                        switch (this.CurTab)
                        {
                            case Tab.DianQuan:
                                this.RefreshDrawCnt(enPayType.DianQuan, out drawInfo, -1);
                                this.RefreshLuck(enPayType.DianQuan, drawInfo);
                                this.m_ExternRewardDic.TryGetValue(2, out view);
                                break;

                            case Tab.Diamond:
                                this.RefreshDrawCnt(enPayType.Diamond, out drawInfo, -1);
                                this.RefreshLuck(enPayType.Diamond, drawInfo);
                                this.m_ExternRewardDic.TryGetValue(10, out view);
                                break;
                        }
                    }
                    if ((view == null) || (view.Count == 0))
                    {
                        p.CustomSetActive(false);
                    }
                    else
                    {
                        p.CustomSetActive(true);
                        int count = view.Count;
                        for (byte i = 0; i < 5; i = (byte) (i + 1))
                        {
                            GameObject obj3 = Utility.FindChild(p, string.Format("Award{0}", i));
                            if (i < view.Count)
                            {
                                if (obj3 != null)
                                {
                                    obj3.CustomSetActive(true);
                                    string str = Convert.ToString((long) drawInfo.dwReachMask, 2).PadLeft(0x20, '0');
                                    string str2 = Convert.ToString((long) drawInfo.dwDrawMask, 2).PadLeft(0x20, '0');
                                    byte result = 0;
                                    byte num4 = 0;
                                    byte.TryParse(str.Substring(0x20 - (i + 1), 1), out result);
                                    byte.TryParse(str2.Substring(0x20 - (i + 1), 1), out num4);
                                    ResDT_LuckyDrawExternReward reward = view[i];
                                    Text componetInChild = Utility.GetComponetInChild<Text>(obj3, "Value");
                                    Text text3 = Utility.GetComponetInChild<Text>(obj3, "Value/Text");
                                    if (componetInChild != null)
                                    {
                                        componetInChild.text = reward.dwDrawCnt.ToString();
                                    }
                                    if (text3 != null)
                                    {
                                        text3.text = "个";
                                    }
                                    if ((result > 0) && (num4 == 0))
                                    {
                                        CUICommonSystem.PlayAnimator(obj3, "Premiums_Normal");
                                    }
                                    else if ((result > 0) && (num4 > 0))
                                    {
                                        CUICommonSystem.PlayAnimator(obj3, "Premiums_Disbled");
                                        if (componetInChild != null)
                                        {
                                            componetInChild.text = string.Empty;
                                        }
                                        if (text3 != null)
                                        {
                                            text3.text = "已领取";
                                        }
                                    }
                                    else
                                    {
                                        CUICommonSystem.PlayAnimator(obj3, "Premiums_Disbled");
                                    }
                                    CUIEventScript component = obj3.GetComponent<CUIEventScript>();
                                    if (component == null)
                                    {
                                        component = obj3.AddComponent<CUIEventScript>();
                                        component.Initialize(mallForm);
                                    }
                                    stUIEventParams eventParams = new stUIEventParams {
                                        tag = i
                                    };
                                    component.SetUIEvent(enUIEventType.Click, enUIEventID.Mall_Roulette_Open_Extern_Reward_Tip, eventParams);
                                }
                            }
                            else if (obj3 != null)
                            {
                                obj3.CustomSetActive(false);
                            }
                        }
                    }
                }
            }
        }

        private void RefreshLuck(enPayType payType, CSDT_LUCKYDRAW_INFO drawInfo)
        {
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                ResLuckyDrawPrice price = new ResLuckyDrawPrice();
                int dwPreciousHighCnt = 200;
                switch (this.CurTab)
                {
                    case Tab.DianQuan:
                        DebugHelper.Assert(this.m_RewardDic.ContainsKey(2), "夺宝奖励池数据档错误");
                        if (!this.m_RewardDic.ContainsKey(2))
                        {
                            mallForm.Close();
                            return;
                        }
                        GameDataMgr.mallRoulettePriceDict.TryGetValue(enPayType.DianQuan, out price);
                        break;

                    case Tab.Diamond:
                        DebugHelper.Assert(this.m_RewardDic.ContainsKey(10), "夺宝奖励池数据档错误");
                        if (!this.m_RewardDic.ContainsKey(10))
                        {
                            mallForm.Close();
                            return;
                        }
                        GameDataMgr.mallRoulettePriceDict.TryGetValue(enPayType.Diamond, out price);
                        break;
                }
                DebugHelper.Assert(price != null, "商城夺宝配置档有错");
                if (price != null)
                {
                    ResDT_LuckyDrawPeriod periodCfg = this.GetPeriodCfg(price);
                    if (periodCfg != null)
                    {
                        dwPreciousHighCnt = (int) periodCfg.dwPreciousHighCnt;
                    }
                    else
                    {
                        dwPreciousHighCnt = (int) price.dwPreciousHighCnt;
                    }
                }
                int num2 = 0;
                if (drawInfo != null)
                {
                    num2 = (int) Math.Min((long) drawInfo.dwLuckyPoint, (long) dwPreciousHighCnt);
                }
                GameObject obj2 = Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette/Luck/LuckBar");
                GameObject obj3 = Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette/Luck/LuckComplete");
                if (this.m_GotAllUnusualItems)
                {
                    obj2.CustomSetActive(false);
                    Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette/Luck/Effect").CustomSetActive(false);
                    obj3.CustomSetActive(true);
                    if (this.m_IsLuckyBarInited)
                    {
                        Singleton<CSoundManager>.GetInstance().PostEvent("UI_common_gongxi", null);
                    }
                }
                else
                {
                    obj2.CustomSetActive(true);
                    obj3.CustomSetActive(false);
                    double num3 = ((float) num2) / ((float) dwPreciousHighCnt);
                    Image componetInChild = Utility.GetComponetInChild<Image>(mallForm.gameObject, "pnlBodyBg/pnlRoulette/Luck/LuckBar/BarBg/Image");
                    Image image2 = Utility.GetComponetInChild<Image>(mallForm.gameObject, "pnlBodyBg/pnlRoulette/Luck/LuckBar/BarBg/Bar_Light");
                    Text text = Utility.GetComponetInChild<Text>(mallForm.gameObject, "pnlBodyBg/pnlRoulette/Luck/LuckBar/Text");
                    if ((componetInChild != null) && (text != null))
                    {
                        float fillAmount = componetInChild.fillAmount;
                        componetInChild.fillAmount = (float) num3;
                        image2.fillAmount = (float) num3;
                        if (this.m_IsLuckyBarInited && (Math.Abs((float) (fillAmount - componetInChild.fillAmount)) > float.Epsilon))
                        {
                            CUICommonSystem.PlayAnimator(obj2, "BarLight_Anim");
                        }
                        text.text = num2.ToString();
                    }
                }
                this.m_IsLuckyBarInited = true;
            }
        }

        public void RefreshRewards()
        {
            if ((Singleton<CMallSystem>.GetInstance().m_MallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                ResLuckyDrawPrice price = new ResLuckyDrawPrice();
                switch (this.CurTab)
                {
                    case Tab.DianQuan:
                    {
                        DebugHelper.Assert(this.m_RewardDic.ContainsKey(2), "夺宝奖励池数据档错误");
                        if (!this.m_RewardDic.ContainsKey(2))
                        {
                            return;
                        }
                        GameDataMgr.mallRoulettePriceDict.TryGetValue(enPayType.DianQuan, out price);
                        ListView<ResLuckyDrawRewardForClient> view2 = null;
                        this.m_RewardDic.TryGetValue(2, out view2);
                        if ((view2 == null) || (view2.Count == 0))
                        {
                            return;
                        }
                        this.SetRewardItemCells(view2);
                        break;
                    }
                    case Tab.Diamond:
                    {
                        DebugHelper.Assert(this.m_RewardDic.ContainsKey(10), "夺宝奖励池数据档错误");
                        if (!this.m_RewardDic.ContainsKey(10))
                        {
                            return;
                        }
                        GameDataMgr.mallRoulettePriceDict.TryGetValue(enPayType.Diamond, out price);
                        ListView<ResLuckyDrawRewardForClient> view = null;
                        this.m_RewardDic.TryGetValue(10, out view);
                        if ((view == null) || (view.Count == 0))
                        {
                            return;
                        }
                        this.SetRewardItemCells(view);
                        break;
                    }
                }
            }
        }

        private void RefreshTimer()
        {
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                ListView<ResDT_LuckyDrawExternReward> view = new ListView<ResDT_LuckyDrawExternReward>();
                switch (this.CurTab)
                {
                    case Tab.DianQuan:
                        this.m_ExternRewardDic.TryGetValue(2, out view);
                        break;

                    case Tab.Diamond:
                        this.m_ExternRewardDic.TryGetValue(10, out view);
                        break;
                }
                GameObject obj2 = Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette/pnlRefresh");
                CUITimerScript componetInChild = Utility.GetComponetInChild<CUITimerScript>(mallForm.gameObject, "pnlBodyBg/pnlRoulette/pnlRefresh/refreshTimer");
                if ((view == null) || (view.Count == 0))
                {
                    if (componetInChild != null)
                    {
                        componetInChild.EndTimer();
                    }
                    obj2.CustomSetActive(false);
                }
                else
                {
                    obj2.CustomSetActive(true);
                    if (componetInChild != null)
                    {
                        componetInChild.SetTotalTime((float) this.GetNextRefreshTime());
                        componetInChild.StartTimer();
                    }
                }
            }
        }

        public void RequestTimeOut(int seq)
        {
            this.ToggleBtnGroup(true);
        }

        private void SendLotteryMsg(RES_SHOPDRAW_SUBTYPE drawType = 2)
        {
            this.m_LuckyDrawRsp = null;
            if (this.m_RewardList != null)
            {
                this.m_RewardList.Clear();
            }
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x12c1);
            CSPKG_LUCKYDRAW_REQ cspkg_luckydraw_req = new CSPKG_LUCKYDRAW_REQ();
            stPayInfo payInfo = this.GetPayInfo(drawType);
            if (payInfo.m_payType == enPayType.Diamond)
            {
                payInfo.m_payType = enPayType.DiamondAndDianQuan;
            }
            cspkg_luckydraw_req.bMoneyType = (byte) CMallSystem.PayTypeToResBuyCoinType(payInfo.m_payType);
            cspkg_luckydraw_req.bDrawType = (byte) drawType;
            msg.stPkgData.stLuckyDrawReq = cspkg_luckydraw_req;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
            reqSentTimerSeq = Singleton<CTimerManager>.GetInstance().AddTimer(0x2710, 1, new CTimer.OnTimeUpHandler(this.RequestTimeOut));
        }

        public void SetRewardData(SCPKG_LUCKYDRAW_RSP stLuckyDrawRsp)
        {
            this.m_CurLoops = 0;
            this.m_CurRewardIdx = 0;
            this.m_LuckyDrawRsp = stLuckyDrawRsp;
            this.m_RewardList = this.GetRewardUseables(this.m_LuckyDrawRsp);
        }

        private void SetRewardItemCells(ListView<ResLuckyDrawRewardForClient> rewardList)
        {
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                byte num = 0;
                byte num2 = 0;
                for (byte i = 0; i < 14; i = (byte) (i + 1))
                {
                    GameObject p = Utility.FindChild(mallForm.gameObject, string.Format("pnlBodyBg/pnlRoulette/rewardBody/rewardContainer/itemCell{0}", i));
                    if (p != null)
                    {
                        ResLuckyDrawRewardForClient client = rewardList[i];
                        Image componetInChild = Utility.GetComponetInChild<Image>(p, "icon");
                        GameObject obj3 = Utility.FindChild(p, "tag");
                        GameObject obj4 = Utility.FindChild(p, "XiYou");
                        Image image = Utility.GetComponetInChild<Image>(p, "tag");
                        CUseable useable = CUseableManager.CreateUsableByRandowReward((RES_RANDOM_REWARD_TYPE) client.dwItemType, (int) client.dwItemCnt, client.dwItemID);
                        Text text = Utility.GetComponetInChild<Text>(p, "cntBg/cnt");
                        GameObject obj5 = Utility.FindChild(p, "cntBg");
                        Text text2 = Utility.GetComponetInChild<Text>(p, "Name");
                        GameObject obj6 = Utility.FindChild(p, "imgExperienceMark");
                        CUIEventScript component = p.GetComponent<CUIEventScript>();
                        if (component == null)
                        {
                            component = p.AddComponent<CUIEventScript>();
                            component.Initialize(mallForm);
                        }
                        bool owned = false;
                        if (client.dwItemType == 4)
                        {
                            stUIEventParams eventParams = new stUIEventParams();
                            eventParams.openHeroFormPar.heroId = client.dwItemID;
                            eventParams.openHeroFormPar.openSrc = enHeroFormOpenSrc.HeroBuyClick;
                            component.SetUIEvent(enUIEventType.Click, enUIEventID.HeroInfo_OpenForm, eventParams);
                            component.SetUIEvent(enUIEventType.Down, enUIEventID.None);
                            component.SetUIEvent(enUIEventType.HoldEnd, enUIEventID.None);
                            component.SetUIEvent(enUIEventType.DragEnd, enUIEventID.None);
                            if (CHeroDataFactory.CreateHeroData(client.dwItemID).bPlayerOwn)
                            {
                                owned = true;
                            }
                        }
                        else if (client.dwItemType == 11)
                        {
                            stUIEventParams params2 = new stUIEventParams();
                            uint heroId = 0;
                            uint skinId = 0;
                            CSkinInfo.ResolveHeroSkin(client.dwItemID, out heroId, out skinId);
                            params2.openHeroFormPar.heroId = heroId;
                            params2.openHeroFormPar.skinId = skinId;
                            params2.openHeroFormPar.openSrc = enHeroFormOpenSrc.SkinBuyClick;
                            component.SetUIEvent(enUIEventType.Click, enUIEventID.HeroInfo_OpenForm, params2);
                            component.SetUIEvent(enUIEventType.Down, enUIEventID.None);
                            component.SetUIEvent(enUIEventType.HoldEnd, enUIEventID.None);
                            component.SetUIEvent(enUIEventType.DragEnd, enUIEventID.None);
                            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                            if ((masterRoleInfo != null) && masterRoleInfo.IsHaveHeroSkin(heroId, skinId, false))
                            {
                                owned = true;
                            }
                        }
                        else
                        {
                            stUIEventParams params3 = new stUIEventParams {
                                iconUseable = useable,
                                tag = 0
                            };
                            component.SetUIEvent(enUIEventType.Down, enUIEventID.Tips_ItemInfoOpen, params3);
                            component.SetUIEvent(enUIEventType.HoldEnd, enUIEventID.Tips_ItemInfoClose, params3);
                            component.SetUIEvent(enUIEventType.Click, enUIEventID.Tips_ItemInfoClose, params3);
                            component.SetUIEvent(enUIEventType.DragEnd, enUIEventID.Tips_ItemInfoClose, params3);
                        }
                        if ((componetInChild != null) && (useable != null))
                        {
                            componetInChild.SetSprite(useable.GetIconPath(), mallForm, true, false, false);
                        }
                        if ((text != null) && (useable != null))
                        {
                            if (useable.m_stackCount <= 1)
                            {
                                obj5.CustomSetActive(false);
                            }
                            else
                            {
                                obj5.CustomSetActive(true);
                                text.text = useable.m_stackCount.ToString();
                            }
                        }
                        if ((text2 != null) && (useable != null))
                        {
                            text2.text = useable.m_name;
                        }
                        if (client.dwItemTag == 1)
                        {
                            obj4.CustomSetActive(true);
                            num = (byte) (num + 1);
                            if (owned)
                            {
                                num2 = (byte) (num2 + 1);
                            }
                        }
                        else
                        {
                            obj4.CustomSetActive(false);
                        }
                        string productTagIconPath = CMallSystem.GetProductTagIconPath((int) client.dwItemTag, owned);
                        if (productTagIconPath == null)
                        {
                            obj3.CustomSetActive(false);
                        }
                        else
                        {
                            obj3.CustomSetActive(true);
                            image.SetSprite(productTagIconPath, mallForm, true, false, false);
                            Text text3 = Utility.GetComponetInChild<Text>(obj3, "Text");
                            if (text3 != null)
                            {
                                string str2 = string.Empty;
                                switch (client.dwItemTag)
                                {
                                    case 1:
                                    case 4:
                                        str2 = Singleton<CTextManager>.GetInstance().GetText("Common_Tag_Rare");
                                        break;

                                    case 2:
                                        str2 = Singleton<CTextManager>.GetInstance().GetText("Common_Tag_New");
                                        break;

                                    case 3:
                                        str2 = Singleton<CTextManager>.GetInstance().GetText("Common_Tag_Hot");
                                        break;

                                    default:
                                        str2 = string.Empty;
                                        break;
                                }
                                if (owned)
                                {
                                    text3.text = "拥有";
                                }
                                else
                                {
                                    text3.text = str2;
                                }
                            }
                        }
                        if (obj6 != null)
                        {
                            obj6.CustomSetActive(false);
                            Image image3 = obj6.GetComponent<Image>();
                            if (image3 != null)
                            {
                                if (CItem.IsHeroExperienceCard(client.dwItemID))
                                {
                                    obj6.CustomSetActive(true);
                                    image3.SetSprite(CUIUtility.GetSpritePrefeb(CExperienceCardSystem.HeroExperienceCardMarkPath, false, false));
                                }
                                else if (CItem.IsSkinExperienceCard(client.dwItemID))
                                {
                                    obj6.CustomSetActive(true);
                                    image3.SetSprite(CUIUtility.GetSpritePrefeb(CExperienceCardSystem.SkinExperienceCardMarkPath, false, false));
                                }
                            }
                        }
                    }
                }
                if (num == num2)
                {
                    this.m_GotAllUnusualItems = true;
                }
                else
                {
                    this.m_GotAllUnusualItems = false;
                }
            }
        }

        public void ShowHeroSkin(ListView<CUseable> items)
        {
            int count = items.Count;
            if (count != 0)
            {
                uint heroId = 0;
                uint skinId = 0;
                for (int i = 0; i < count; i++)
                {
                    switch (items[i].m_type)
                    {
                        case COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP:
                        {
                            if (items[i].ExtraFromType != 1)
                            {
                                break;
                            }
                            CUICommonSystem.ShowNewHeroOrSkin((uint) items[i].ExtraFromData, 0, enUIEventID.None, true, COM_REWARDS_TYPE.COM_REWARDS_TYPE_HERO, true, null, enFormPriority.Priority4, (uint) items[i].m_stackCount, 0);
                            continue;
                        }
                        case COM_ITEM_TYPE.COM_OBJTYPE_ITEMEQUIP:
                        case COM_ITEM_TYPE.COM_OBJTYPE_ITEMSYMBOL:
                        case COM_ITEM_TYPE.COM_OBJTYPE_ITEMGEAR:
                        {
                            continue;
                        }
                        case COM_ITEM_TYPE.COM_OBJTYPE_HERO:
                        {
                            CUICommonSystem.ShowNewHeroOrSkin(items[i].m_baseID, 0, enUIEventID.None, true, COM_REWARDS_TYPE.COM_REWARDS_TYPE_HERO, false, null, enFormPriority.Priority4, 0, 0);
                            continue;
                        }
                        case COM_ITEM_TYPE.COM_OBJTYPE_HEROSKIN:
                        {
                            CSkinInfo.ResolveHeroSkin(items[i].m_baseID, out heroId, out skinId);
                            CUICommonSystem.ShowNewHeroOrSkin(heroId, skinId, enUIEventID.None, true, COM_REWARDS_TYPE.COM_REWARDS_TYPE_SKIN, false, null, enFormPriority.Priority4, 0, 0);
                            continue;
                        }
                        default:
                        {
                            continue;
                        }
                    }
                    if (items[i].ExtraFromType == 2)
                    {
                        CSkinInfo.ResolveHeroSkin((uint) items[i].ExtraFromData, out heroId, out skinId);
                        CUICommonSystem.ShowNewHeroOrSkin(heroId, skinId, enUIEventID.None, true, COM_REWARDS_TYPE.COM_REWARDS_TYPE_SKIN, true, null, enFormPriority.Priority4, (uint) items[i].m_stackCount, 0);
                    }
                }
            }
        }

        public void Spin(int idx = -1)
        {
            Singleton<CSoundManager>.GetInstance().PostEvent("Stop_UI_buy_chou_duobao_zhuan", null);
            Singleton<CSoundManager>.GetInstance().PostEvent("UI_buy_chou_duobao_zhuan", null);
            if (idx == -1)
            {
                this.m_CurSpinCnt = 0;
                this.m_CurState = Roulette_State.ACCELERATE;
                this.m_IsClockwise = true;
            }
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                float time = 10f;
                CUITimerScript componetInChild = Utility.GetComponetInChild<CUITimerScript>(mallForm.gameObject, "pnlBodyBg/pnlRoulette/timerSpin");
                if (componetInChild != null)
                {
                    componetInChild.SetTotalTime(time);
                    if (idx == -1)
                    {
                        componetInChild.SetOnChangedIntervalTime(0.1f);
                    }
                    componetInChild.m_eventParams[2].tag = idx;
                    componetInChild.m_eventParams[1].tag = idx;
                    componetInChild.StartTimer();
                }
            }
        }

        public void ToggleBtnGroup(bool active)
        {
            CUIFormScript mallForm = Singleton<CMallSystem>.GetInstance().m_MallForm;
            if ((mallForm != null) && Singleton<CMallSystem>.GetInstance().m_IsMallFormOpen)
            {
                GameObject obj2 = Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette/Luck/btnGroup");
                GameObject obj3 = Utility.FindChild(mallForm.gameObject, "pnlBodyBg/pnlRoulette/Luck/Effect");
                if (active)
                {
                    obj2.CustomSetActive(true);
                    obj3.CustomSetActive(false);
                }
                else
                {
                    obj2.CustomSetActive(false);
                    if (!this.m_GotAllUnusualItems)
                    {
                        obj3.CustomSetActive(true);
                    }
                }
            }
        }

        public override void UnInit()
        {
            base.UnInit();
            this.m_UsedTabs.Clear();
            this.m_RewardDic.Clear();
            this.m_ExternRewardDic.Clear();
            this.m_RewardPoolDic.Clear();
            CUIEventManager instance = Singleton<CUIEventManager>.GetInstance();
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Tab_Change, new CUIEventManager.OnUIEventHandler(this.OnRouletteTabChange));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Buy_One, new CUIEventManager.OnUIEventHandler(this.OnRouletteBuyOne));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Buy_Five, new CUIEventManager.OnUIEventHandler(this.OnRouletteBuyFive));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Buy_One_Confirm, new CUIEventManager.OnUIEventHandler(this.OnRouletteBuyOneConfirm));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Buy_Five_Confirm, new CUIEventManager.OnUIEventHandler(this.OnRouletteBuyFiveConfirm));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Spin_Change, new CUIEventManager.OnUIEventHandler(this.OnSpinInterval));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Spin_End, new CUIEventManager.OnUIEventHandler(this.OnSpinEnd));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Open_Extern_Reward_Tip, new CUIEventManager.OnUIEventHandler(this.OnOpenExternRewardTip));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Draw_Extern_Reward, new CUIEventManager.OnUIEventHandler(this.OnDrawExternReward));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Reset_Reward_Draw_Cnt, new CUIEventManager.OnUIEventHandler(this.OnDrawCntReset));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Show_Reward_Item, new CUIEventManager.OnUIEventHandler(this.OnShowRewardItem));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Show_Rule, new CUIEventManager.OnUIEventHandler(this.OnShowRule));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Tmp_Reward_Enable, new CUIEventManager.OnUIEventHandler(this.OnTmpRewardEnable));
            instance.RemoveUIEventListener(enUIEventID.Mall_Skip_Animation, new CUIEventManager.OnUIEventHandler(this.OnSkipAnimation));
            instance.RemoveUIEventListener(enUIEventID.Mall_Roulette_Close_Award_Form, new CUIEventManager.OnUIEventHandler(this.OnCloseAwardForm));
            Singleton<EventRouter>.instance.RemoveEventHandler(EventID.Mall_Receive_Roulette_Data, new Action(this, (IntPtr) this.RefreshExternRewards));
            Singleton<EventRouter>.instance.RemoveEventHandler(EventID.Mall_Change_Tab, new Action(this, (IntPtr) this.OnMallTabChange));
            Singleton<EventRouter>.instance.RemoveEventHandler<uint>("HeroAdd", new Action<uint>(this.OnNtyAddHero));
            Singleton<EventRouter>.instance.RemoveEventHandler<uint, uint, uint>("HeroSkinAdd", new Action<uint, uint, uint>(this, (IntPtr) this.OnNtyAddSkin));
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

        public enum Roulette_State
        {
            NONE,
            ACCELERATE,
            UNIFORM,
            DECELERATE,
            CONTINUOUS_DRAW,
            SKIP
        }

        public enum Tab
        {
            Diamond = 1,
            DianQuan = 0,
            None = -1
        }
    }
}

