namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    [MessageHandlerClass]
    public class CSymbolSystem : Singleton<CSymbolSystem>
    {
        private ushort m_breakLevelMask = 0xffff;
        private ListView<CSDT_SYMBOLOPT_INFO> m_breakSymbolList = new ListView<CSDT_SYMBOLOPT_INFO>();
        private ResSymbolInfo m_curTransformSymbol;
        private CSymbolItem m_curViewSymbol;
        private ListView<CSymbolItem> m_pageSymbolBagList = new ListView<CSymbolItem>();
        private enSymbolMenuType m_selectMenuType;
        private int m_selectSymbolPos = -1;
        private int m_symbolFilterLevel = 1;
        private enSymbolType m_symbolFilterType;
        private ListView<CSymbolItem> m_symbolList = new ListView<CSymbolItem>();
        private ListView<ResSymbolInfo> m_symbolMakeList = new ListView<ResSymbolInfo>();
        private int m_symbolPageIndex;
        private static ListView<ResSymbolInfo> s_allSymbolCfgList = new ListView<ResSymbolInfo>();
        private static int s_breakSymbolCoinCnt = 0;
        public static int s_maxSameIDSymbolEquipNum = 10;
        public static int s_maxSymbolLevel = 5;
        public static string s_symbolBagPanel = "Panel_SymbolEquip/Panel_SymbolBag";
        public static string s_symbolBreakPath = "UGUI/Form/System/Symbol/Form_SymbolBreak.prefab";
        public static string s_symbolEquipPanel = "Panel_SymbolEquip";
        public static string s_symbolFormPath = "UGUI/Form/System/Symbol/Form_Symbol.prefab";
        public static string s_symbolMakePanel = "Panel_SymbolMake";
        public static string s_symbolPagePanel = "Panel_SymbolEquip/Panel_SymbolPageRect/Panel_SymbolPage";
        private static int[] s_symbolPagePropArr = new int[0x24];
        private static int[] s_symbolPagePropPctArr = new int[0x24];
        public static readonly Vector2 s_symbolPos1 = new Vector2(25f, -1f);
        public static readonly Vector2 s_symbolPos2 = new Vector2(0f, -1f);
        public static int[] s_symbolPropPctAddArr = new int[0x24];
        public static int[] s_symbolPropValAddArr = new int[0x24];
        public static string s_symbolTransformPath = "UGUI/Form/System/Symbol/Form_SymbolTransform.prefab";

        public static void AddTip(GameObject target, string tip, enUseableTipsPos pos)
        {
            if (null != target)
            {
                stUIEventParams eventParams = new stUIEventParams {
                    tagStr = tip,
                    tag = (int) pos
                };
                CUIEventScript component = target.GetComponent<CUIEventScript>();
                if (component != null)
                {
                    component.SetUIEvent(enUIEventType.Down, enUIEventID.Tips_CommonInfoOpen, eventParams);
                    component.SetUIEvent(enUIEventType.HoldEnd, enUIEventID.Tips_CommonInfoClose, eventParams);
                    component.SetUIEvent(enUIEventType.Click, enUIEventID.Tips_CommonInfoClose, eventParams);
                    component.SetUIEvent(enUIEventType.DragEnd, enUIEventID.Tips_CommonInfoClose, eventParams);
                }
            }
        }

        private bool CheckSymbolBreak(CSymbolItem symbol, ushort breakLvlMask)
        {
            return (((symbol != null) && (symbol.m_SymbolData.wLevel < s_maxSymbolLevel)) && ((symbol.m_stackCount > symbol.GetMaxWearCnt()) && (((((int) 1) << symbol.m_SymbolData.wLevel) & breakLvlMask) != 0)));
        }

        public void Clear()
        {
            this.m_selectMenuType = enSymbolMenuType.SymbolEquip;
            this.ClearSymbolEquipData();
            this.ClearSymbolMakeData();
        }

        private void ClearSymbolEquipData()
        {
            this.m_selectSymbolPos = -1;
            this.m_symbolPageIndex = 0;
        }

        private void ClearSymbolMakeData()
        {
            this.m_symbolFilterLevel = 1;
            this.m_symbolFilterType = enSymbolType.All;
        }

        private void ConfirmWhenMoneyNotEnough(CUIEvent uiEvent)
        {
            int tag = uiEvent.m_eventParams.tag;
            DebugHelper.Assert(tag > 0, "gridPos should be above 0!!!");
            ResShopInfo gridShopInfo = this.GetGridShopInfo(tag);
            DebugHelper.Assert(gridShopInfo != null, "shopCfg is NULL!!!");
            string goodName = StringHelper.UTF8BytesToString(ref gridShopInfo.szDesc);
            CMallSystem.TryToPay(enPayPurpose.Open, goodName, CMallSystem.ResBuyTypeToPayType(gridShopInfo.bCoinType), gridShopInfo.dwCoinPrice, enUIEventID.Symbol_ConfirmBuyGrid, ref uiEvent.m_eventParams, enUIEventID.None, true, true);
        }

        private int GetBreakExcessSymbolCoinCnt(ushort breakLvlMask = 0xffff)
        {
            int num = 0;
            for (int i = 0; i < this.m_symbolList.Count; i++)
            {
                if (this.CheckSymbolBreak(this.m_symbolList[i], breakLvlMask))
                {
                    num += (int) ((this.m_symbolList[i].m_stackCount - this.m_symbolList[i].GetMaxWearCnt()) * this.m_symbolList[i].m_SymbolData.dwBreakCoin);
                }
            }
            return num;
        }

        private ResShopInfo GetGridShopInfo(int pos)
        {
            return CPurchaseSys.GetCfgShopInfo(RES_SHOPBUY_TYPE.RES_BUYTYPE_SYMBOLPAGEPOS, pos);
        }

        public static string GetSymbolAttString(CSymbolItem symbol, bool bPvp = true)
        {
            if (bPvp)
            {
                return CUICommonSystem.GetFuncEftDesc(ref symbol.m_SymbolData.astFuncEftList, true);
            }
            return CUICommonSystem.GetFuncEftDesc(ref symbol.m_SymbolData.astPveEftList, true);
        }

        public static string GetSymbolAttString(uint cfgId, bool bPvp = true)
        {
            ResSymbolInfo dataByKey = GameDataMgr.symbolInfoDatabin.GetDataByKey(cfgId);
            if (dataByKey == null)
            {
                return string.Empty;
            }
            if (bPvp)
            {
                return CUICommonSystem.GetFuncEftDesc(ref dataByKey.astFuncEftList, true);
            }
            return CUICommonSystem.GetFuncEftDesc(ref dataByKey.astPveEftList, true);
        }

        private CSymbolItem GetSymbolByCfgID(uint cfgId)
        {
            for (int i = 0; i < this.m_symbolList.Count; i++)
            {
                CSymbolItem item2 = this.m_symbolList[i];
                if ((item2 != null) && (item2.m_baseID == cfgId))
                {
                    return item2;
                }
            }
            return null;
        }

        private CSymbolItem GetSymbolByObjID(ulong objID)
        {
            for (int i = 0; i < this.m_symbolList.Count; i++)
            {
                CSymbolItem item2 = this.m_symbolList[i];
                if ((item2 != null) && (item2.m_objID == objID))
                {
                    return item2;
                }
            }
            return null;
        }

        public int GetSymbolListIndex(uint symbolCfgId)
        {
            if (this.m_pageSymbolBagList.Count > 0)
            {
                for (int i = 0; i < this.m_pageSymbolBagList.Count; i++)
                {
                    if (this.m_pageSymbolBagList[i].m_baseID == symbolCfgId)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static void GetSymbolProp(uint symbolId, ref int[] propArr, ref int[] propPctArr, bool bPvp)
        {
            int index = 0;
            int num2 = 0x24;
            for (index = 0; index < num2; index++)
            {
                propArr[index] = 0;
                propPctArr[index] = 0;
            }
            ResSymbolInfo dataByKey = GameDataMgr.symbolInfoDatabin.GetDataByKey(symbolId);
            if (dataByKey != null)
            {
                ResDT_FuncEft_Obj[] objArray = !bPvp ? dataByKey.astPveEftList : dataByKey.astFuncEftList;
                for (int i = 0; i < objArray.Length; i++)
                {
                    if (((objArray[i].wType != 0) && (objArray[i].wType < 0x24)) && (objArray[i].iValue != 0))
                    {
                        if (objArray[i].bValType == 0)
                        {
                            propArr[objArray[i].wType] += objArray[i].iValue;
                        }
                        else if (objArray[i].bValType == 1)
                        {
                            propPctArr[objArray[i].wType] += objArray[i].iValue;
                        }
                    }
                }
            }
        }

        public static string GetSymbolWearTip(uint cfgId, bool bWear)
        {
            string format = !bWear ? Singleton<CTextManager>.GetInstance().GetText("Symbol_TakeOffTip") : Singleton<CTextManager>.GetInstance().GetText("Symbol_WearTip");
            ResSymbolInfo dataByKey = GameDataMgr.symbolInfoDatabin.GetDataByKey(cfgId);
            if (dataByKey == null)
            {
                return string.Empty;
            }
            format = string.Format(format, StringHelper.UTF8BytesToString(ref dataByKey.szName)) + "\n";
            ushort item = 0;
            byte bValType = 0;
            int iValue = 0;
            for (int i = 0; i < dataByKey.astFuncEftList.Length; i++)
            {
                item = dataByKey.astFuncEftList[i].wType;
                bValType = dataByKey.astFuncEftList[i].bValType;
                iValue = dataByKey.astFuncEftList[i].iValue;
                if (item != 0)
                {
                    switch (bValType)
                    {
                        case 0:
                            if (CUICommonSystem.s_pctFuncEftList.IndexOf(item) != -1)
                            {
                                format = format + (!bWear ? string.Format("{0}-{1}", CUICommonSystem.s_attNameList[item], CUICommonSystem.GetValuePercent(iValue / 100)) : string.Format("{0}+{1}", CUICommonSystem.s_attNameList[item], CUICommonSystem.GetValuePercent(iValue / 100)));
                            }
                            else
                            {
                                format = format + (!bWear ? string.Format("{0}-{1}", CUICommonSystem.s_attNameList[item], ((float) iValue) / 100f) : string.Format("{0}+{1}", CUICommonSystem.s_attNameList[item], ((float) iValue) / 100f));
                            }
                            break;

                        case 1:
                            format = format + (!bWear ? string.Format("{0}-{1}", CUICommonSystem.s_attNameList[item], CUICommonSystem.GetValuePercent(iValue)) : string.Format("{0}+{1}", CUICommonSystem.s_attNameList[item], CUICommonSystem.GetValuePercent(iValue)));
                            break;
                    }
                }
            }
            return format;
        }

        public override void Init()
        {
            this.InitSymbolCfgList();
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_OpenForm, new CUIEventManager.OnUIEventHandler(this.OnOpenSymbolForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_FormClose, new CUIEventManager.OnUIEventHandler(this.OnSymbolFormClose));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_RevertToVisible, new CUIEventManager.OnUIEventHandler(this.OnSymbolRevertToVisible));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_OpenForm_ToMakeTab, new CUIEventManager.OnUIEventHandler(this.OnOpenSymbolFormToMakeTab));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_MenuSelect, new CUIEventManager.OnUIEventHandler(this.OnMenuSelect));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_Off, new CUIEventManager.OnUIEventHandler(this.OnOffSymbol));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_View, new CUIEventManager.OnUIEventHandler(this.OnSymbolView));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_BagItemEnable, new CUIEventManager.OnUIEventHandler(this.OnSymbolBagElementEnable));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_BagItemClick, new CUIEventManager.OnUIEventHandler(this.OnSymbolBagItemClick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_BagViewHide, new CUIEventManager.OnUIEventHandler(this.OnSymbolBagSymbolViewHide));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_BagShow, new CUIEventManager.OnUIEventHandler(this.OnSymbolBagShow));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_ChangeSymbolClick, new CUIEventManager.OnUIEventHandler(this.OnChangeSymbolClick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_ChangeConfirm, new CUIEventManager.OnUIEventHandler(this.OnSymbolChangeConfirm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_SymbolPageDownBtnClick, new CUIEventManager.OnUIEventHandler(this.OnSymbolPageDownBtnClick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_PageItemSelect, new CUIEventManager.OnUIEventHandler(this.OnSymbolPageItemSelect));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_ChangePageName, new CUIEventManager.OnUIEventHandler(this.OnChangeSymbolPageName));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_PageAddClick, new CUIEventManager.OnUIEventHandler(this.OnSymbol_PageAddClick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_PageClear, new CUIEventManager.OnUIEventHandler(this.OnSymbolPageClear));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_PageClearConfirm, new CUIEventManager.OnUIEventHandler(this.OnSymbolPageClearConfirm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_ConfirmChgPageName, new CUIEventManager.OnUIEventHandler(this.OnConfirmChgPageName));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Purchase_BuySymbolPage, new CUIEventManager.OnUIEventHandler(this.OnBuySymbolPage));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_OpenUnlockLvlTip, new CUIEventManager.OnUIEventHandler(this.OnOpenUnlockLvlTip));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.SymbolMake_TypeMenuSelect, new CUIEventManager.OnUIEventHandler(this.OnSymbolTypeMenuSelect));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.SymbolMake_LevelMenuSelect, new CUIEventManager.OnUIEventHandler(this.OnSymbolLevelMenuSelect));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.SymbolMake_ListItemEnable, new CUIEventManager.OnUIEventHandler(this.OnSymbolMakeListEnable));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.SymbolMake_ListItemClick, new CUIEventManager.OnUIEventHandler(this.OnSymbolMakeListClick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.SymbolMake_OnItemMakeClick, new CUIEventManager.OnUIEventHandler(this.OnMakeSymbolClick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.SymbolMake_OnItemMakeConfirm, new CUIEventManager.OnUIEventHandler(this.OnItemMakeConfirm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.SymbolMake_OnItemBreakClick, new CUIEventManager.OnUIEventHandler(this.OnBreakSymbolClick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.SymbolMake_OnItemBreakConfirm, new CUIEventManager.OnUIEventHandler(this.OnBreakSymbolConfirm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.SymbolMake_OnBreakExcessSymbolClick, new CUIEventManager.OnUIEventHandler(this.OnBreakExcessSymbolClick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.SymbolMake_SelectBreakLvlItem, new CUIEventManager.OnUIEventHandler(this.OnSelectBreakLvlItem));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.SymbolMake_OnBreakExcessSymbolConfirm, new CUIEventManager.OnUIEventHandler(this.OnBreakExcessSymbolClickConfirm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.SymbolMake_ItemBreakAnimatorEnd, new CUIEventManager.OnUIEventHandler(this.OnItemBreakAnimatorEnd));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_PromptBuyGrid, new CUIEventManager.OnUIEventHandler(this.OnPromptBuyGrid));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_ConfirmBuyGrid, new CUIEventManager.OnUIEventHandler(this.OnConfirmBuyGrid));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Symbol_ConfirmWhenMoneyNotEnough, new CUIEventManager.OnUIEventHandler(this.ConfirmWhenMoneyNotEnough));
            Singleton<EventRouter>.instance.AddEventHandler("MasterSymbolCoinChanged", new Action(this, (IntPtr) this.RefreshSymbolCntText));
            Singleton<EventRouter>.instance.AddEventHandler("MasterPvpLevelChanged", new Action(this, (IntPtr) this.RefreshPageDropList));
        }

        private void InitSymbolCfgList()
        {
            s_allSymbolCfgList.Clear();
            Dictionary<long, object>.Enumerator enumerator = GameDataMgr.symbolInfoDatabin.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<long, object> current = enumerator.Current;
                ResSymbolInfo item = (ResSymbolInfo) current.Value;
                if ((item != null) && (item.bIsMakeShow > 0))
                {
                    s_allSymbolCfgList.Add(item);
                }
            }
        }

        private bool IsBagItemCanUse()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo == null)
            {
                DebugHelper.Assert(false, "role is null");
                return false;
            }
            for (int i = 0; i < this.m_pageSymbolBagList.Count; i++)
            {
                if ((this.m_symbolList[i].m_SymbolData.wLevel <= masterRoleInfo.m_symbolInfo.m_pageMaxLvlLimit) && (this.m_symbolList[i].GetPageWearCnt(this.m_symbolPageIndex) < s_maxSameIDSymbolEquipNum))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsSymbolChangePanelShow(CUIFormScript form)
        {
            if (null == form)
            {
                return false;
            }
            GameObject widget = form.GetWidget(15);
            return ((widget != null) && widget.activeSelf);
        }

        private bool MovePosToNextCanEquipPos()
        {
            bool flag = false;
            int num = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().m_symbolInfo.GetNextCanEquipPos(this.m_symbolPageIndex, this.m_selectSymbolPos, ref this.m_symbolList);
            if (num != -1)
            {
                stUIEventParams par = new stUIEventParams();
                par.symbolParam.symbol = null;
                par.symbolParam.page = this.m_symbolPageIndex;
                par.symbolParam.pos = num;
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Symbol_BagShow, par);
                flag = true;
            }
            return flag;
        }

        private void OnBreakExcessSymbolClick(CUIEvent uiEvent)
        {
            if (Singleton<CUIManager>.GetInstance().OpenForm(s_symbolBreakPath, false, true) != null)
            {
                this.m_breakLevelMask = 0xffff;
                this.RefreshSymbolBreakForm();
            }
        }

        private void OnBreakExcessSymbolClickConfirm(CUIEvent uiEvent)
        {
            if (this.m_breakLevelMask == 0)
            {
                Singleton<CUIManager>.GetInstance().OpenTips("Symbol_Select_BreakLvl_Tip", true, 1f, null, new object[0]);
            }
            else
            {
                this.SetBreakSymbolList();
                if (this.m_breakSymbolList.Count > 0)
                {
                    SendReqExcessSymbolBreak(this.m_breakSymbolList);
                }
                else
                {
                    Singleton<CUIManager>.GetInstance().OpenTips("Symbol_No_More_Item_Break", true, 1f, null, new object[0]);
                }
            }
        }

        private void OnBreakSymbolClick(CUIEvent uiEvent)
        {
            if (this.m_curTransformSymbol != null)
            {
                CSymbolItem symbolByCfgID = this.GetSymbolByCfgID(this.m_curTransformSymbol.dwID);
                if (symbolByCfgID == null)
                {
                    Singleton<CUIManager>.GetInstance().OpenTips("Symbol__Item_Not_Exist_Tip", true, 1f, null, new object[0]);
                }
                else if (symbolByCfgID.m_stackCount > symbolByCfgID.GetMaxWearCnt())
                {
                    string text = Singleton<CTextManager>.GetInstance().GetText("Symbol_Break_Tip");
                    Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(text, enUIEventID.SymbolMake_OnItemBreakConfirm, enUIEventID.None, false);
                }
                else
                {
                    CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                    if (masterRoleInfo != null)
                    {
                        string strContent = string.Format(Singleton<CTextManager>.GetInstance().GetText("Symbol_Break_Item_Wear_Tip"), masterRoleInfo.m_symbolInfo.GetMaxWearSymbolPageName(symbolByCfgID));
                        Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(strContent, enUIEventID.SymbolMake_OnItemBreakConfirm, enUIEventID.None, false);
                    }
                }
            }
        }

        private void OnBreakSymbolConfirm(CUIEvent uiEvent)
        {
            if (this.m_curTransformSymbol != null)
            {
                CSymbolItem symbolByCfgID = this.GetSymbolByCfgID(this.m_curTransformSymbol.dwID);
                if (symbolByCfgID != null)
                {
                    SendReqSymbolBreak(symbolByCfgID.m_baseID, 1);
                }
            }
        }

        public static void OnBuyNewSymbolPage()
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo.m_symbolInfo.IsPageFull())
            {
                string text = Singleton<CTextManager>.GetInstance().GetText("Symbol_Page_Buy_PageFull");
                Singleton<CUIManager>.GetInstance().OpenTips(text, false, 1f, null, new object[0]);
            }
            else
            {
                RES_SHOPBUY_COINTYPE costType = RES_SHOPBUY_COINTYPE.RES_SHOPBUY_TYPE_PVPCOIN;
                uint costVal = 0;
                masterRoleInfo.m_symbolInfo.GetNewPageCost(out costType, out costVal);
                string goodName = "新的符文页";
                stUIEventParams confirmEventParams = new stUIEventParams();
                CMallSystem.TryToPay(enPayPurpose.Buy, goodName, CMallSystem.ResBuyTypeToPayType((int) costType), costVal, enUIEventID.Purchase_BuySymbolPage, ref confirmEventParams, enUIEventID.None, true, true);
            }
        }

        public void OnBuySymbolPage(CUIEvent uiEvent)
        {
            SendBuySymbol(Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().m_symbolInfo.m_pageBuyCnt + 1);
        }

        private void OnChangeSymbolClick(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (form != null)
            {
                form.GetWidget(3).CustomSetActive(true);
                form.GetWidget(5).CustomSetActive(false);
                form.GetWidget(0x10).CustomSetActive(false);
                form.GetWidget(15).CustomSetActive(true);
                this.RefreshSymbolChangePanel(this.m_curViewSymbol, null);
                this.ShowSymbolBag(true);
            }
        }

        private void OnChangeSymbolPageName(CUIEvent uiEvent)
        {
            string text = Singleton<CTextManager>.GetInstance().GetText("Symbol_ChangeName");
            string inputTip = Singleton<CTextManager>.GetInstance().GetText("Symbol_InputName");
            Singleton<CUIManager>.GetInstance().OpenInputBox(text, inputTip, enUIEventID.Symbol_ConfirmChgPageName);
        }

        private void OnConfirmBuyGrid(CUIEvent uiEvent)
        {
            DebugHelper.Assert(uiEvent.m_eventParams.tag > 0, "gridPos should be above 0!!!");
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x459);
            msg.stPkgData.stShopBuyReq.iBuyType = 12;
            msg.stPkgData.stShopBuyReq.iBuySubType = uiEvent.m_eventParams.tag;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        private void OnConfirmChgPageName(CUIEvent uiEvent)
        {
            string text = uiEvent.m_srcFormScript.gameObject.transform.Find("Panel/inputText/Text").GetComponent<Text>().text;
            if (string.IsNullOrEmpty(text) || (text.Length > 6))
            {
                string strContent = Singleton<CTextManager>.GetInstance().GetText("Symbol_Name_LenError");
                Singleton<CUIManager>.GetInstance().OpenTips(strContent, false, 1f, null, new object[0]);
            }
            else
            {
                SendChgSymbolPageName(this.m_symbolPageIndex, text);
            }
        }

        private void OnItemBreakAnimatorEnd(CUIEvent uiEvent)
        {
            if (s_breakSymbolCoinCnt > 0)
            {
                CUseable useable = CUseableManager.CreateVirtualUseable(enVirtualItemType.enSymbolCoin, s_breakSymbolCoinCnt);
                if (useable != null)
                {
                    CUseable[] items = new CUseable[] { useable };
                    Singleton<CUIManager>.GetInstance().OpenAwardTip(items, null, false, enUIEventID.None, false, false, "Form_Award");
                }
                s_breakSymbolCoinCnt = 0;
            }
        }

        private void OnItemMakeConfirm(CUIEvent uiEvent)
        {
            if (this.m_curTransformSymbol != null)
            {
                SendReqSymbolMake(this.m_curTransformSymbol.dwID, 1);
            }
        }

        private void OnMakeSymbolClick(CUIEvent uiEvent)
        {
            if (this.m_curTransformSymbol != null)
            {
                if (Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().SymbolCoin >= this.m_curTransformSymbol.dwMakeCoin)
                {
                    CSymbolItem symbolByCfgID = this.GetSymbolByCfgID(this.m_curTransformSymbol.dwID);
                    if (symbolByCfgID != null)
                    {
                        if (symbolByCfgID.m_stackCount >= symbolByCfgID.m_SymbolData.iOverLimit)
                        {
                            Singleton<CUIManager>.GetInstance().OpenTips("Symbol_Make_MaxCnt_Limit", true, 1f, null, new object[0]);
                        }
                        else if (symbolByCfgID.m_stackCount >= s_maxSameIDSymbolEquipNum)
                        {
                            string text = Singleton<CTextManager>.GetInstance().GetText("Symbol_Make_WearMaxLimit_Tip");
                            Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(text, enUIEventID.SymbolMake_OnItemMakeConfirm, enUIEventID.None, false);
                        }
                        else
                        {
                            SendReqSymbolMake(this.m_curTransformSymbol.dwID, 1);
                        }
                    }
                    else
                    {
                        SendReqSymbolMake(this.m_curTransformSymbol.dwID, 1);
                    }
                }
                else
                {
                    Singleton<CUIManager>.GetInstance().OpenTips("Symbol_Coin_Not_Enough_Tip", true, 1f, null, new object[0]);
                }
            }
        }

        private void OnMenuSelect(CUIEvent uiEvent)
        {
            CUIFormScript srcFormScript = uiEvent.m_srcFormScript;
            if (srcFormScript != null)
            {
                CUIListScript component = uiEvent.m_srcWidget.GetComponent<CUIListScript>();
                if (null != component)
                {
                    int selectedIndex = component.GetSelectedIndex();
                    this.m_selectMenuType = (enSymbolMenuType) selectedIndex;
                    Transform transform = srcFormScript.transform;
                    if (this.m_selectMenuType == enSymbolMenuType.SymbolEquip)
                    {
                        transform.Find(s_symbolEquipPanel).gameObject.CustomSetActive(true);
                        transform.Find(s_symbolMakePanel).gameObject.CustomSetActive(false);
                        this.SetSymbolItemSelect(this.m_selectSymbolPos, false);
                        this.ClearSymbolEquipData();
                        this.RefreshSymbolEquipForm();
                        this.RefreshPageDropList();
                        CTextManager instance = Singleton<CTextManager>.GetInstance();
                        AddTip(srcFormScript.GetWidget(6).transform.Find("titlePanel").gameObject, instance.GetText("Symbol_PvpProp_Desc"), enUseableTipsPos.enLeft);
                        AddTip(srcFormScript.GetWidget(7).transform.Find("titlePanel").gameObject, instance.GetText("Symbol_EnhancePveProp_Desc"), enUseableTipsPos.enLeft);
                    }
                    else if (this.m_selectMenuType == enSymbolMenuType.SymbolMake)
                    {
                        transform.Find(s_symbolEquipPanel).gameObject.CustomSetActive(false);
                        transform.Find(s_symbolMakePanel).gameObject.CustomSetActive(true);
                        this.ClearSymbolMakeData();
                        this.RefreshSymbolMakeForm();
                        this.RefreshSymbolCntText();
                        CUseable useable = CUseableManager.CreateVirtualUseable(enVirtualItemType.enSymbolCoin, 0);
                        stUIEventParams eventParams = new stUIEventParams {
                            iconUseable = useable,
                            tag = 3
                        };
                        CUIEventScript script3 = uiEvent.m_srcFormScript.GetWidget(14).GetComponent<CUIEventScript>();
                        if (script3 != null)
                        {
                            script3.SetUIEvent(enUIEventType.Down, enUIEventID.Tips_ItemInfoOpen, eventParams);
                            script3.SetUIEvent(enUIEventType.HoldEnd, enUIEventID.Tips_ItemInfoClose, eventParams);
                            script3.SetUIEvent(enUIEventType.Click, enUIEventID.Tips_ItemInfoClose, eventParams);
                            script3.SetUIEvent(enUIEventType.DragEnd, enUIEventID.Tips_ItemInfoClose, eventParams);
                        }
                    }
                    uiEvent.m_srcFormScript.GetWidget(1).CustomSetActive(this.m_selectMenuType == enSymbolMenuType.SymbolEquip);
                    uiEvent.m_srcFormScript.GetWidget(14).CustomSetActive(this.m_selectMenuType == enSymbolMenuType.SymbolMake);
                }
            }
        }

        private void OnOffSymbol(CUIEvent uiEvent)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x46f);
            msg.stPkgData.stSymbolOff.bPage = (byte) this.m_symbolPageIndex;
            msg.stPkgData.stSymbolOff.bPos = (byte) this.m_selectSymbolPos;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        private void OnOpenSymbolForm(CUIEvent uiEvent)
        {
            this.m_selectMenuType = enSymbolMenuType.SymbolEquip;
            this.OpenSymbolForm();
        }

        private void OnOpenSymbolFormToMakeTab(CUIEvent uiEvent)
        {
            this.m_selectMenuType = enSymbolMenuType.SymbolMake;
            this.OpenSymbolForm();
        }

        private void OnOpenUnlockLvlTip(CUIEvent uiEvent)
        {
            int tag = uiEvent.m_eventParams.tag;
            object[] replaceArr = new object[] { tag };
            Singleton<CUIManager>.GetInstance().OpenTips("Symbol_PosUnlock_lvl_Tip", true, 1f, null, replaceArr);
        }

        private void OnPromptBuyGrid(CUIEvent uiEvent)
        {
            int tag = uiEvent.m_eventParams.tag;
            DebugHelper.Assert(tag > 0, "gridPos should be above 0!!!");
            ResShopInfo gridShopInfo = this.GetGridShopInfo(tag);
            DebugHelper.Assert(gridShopInfo != null, "shopCfg is NULL!!!");
            string goodName = StringHelper.UTF8BytesToString(ref gridShopInfo.szDesc);
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                enPayType payType = CMallSystem.ResBuyTypeToPayType(gridShopInfo.bCoinType);
                uint dwCoinPrice = gridShopInfo.dwCoinPrice;
                if (CMallSystem.GetCurrencyValueFromRoleInfo(masterRoleInfo, payType) < dwCoinPrice)
                {
                    string str2 = string.Empty;
                    if (payType == enPayType.DiamondAndDianQuan)
                    {
                        uint diamond = masterRoleInfo.Diamond;
                        if (diamond < dwCoinPrice)
                        {
                            object[] objArray1 = new object[] { (dwCoinPrice - diamond).ToString(), Singleton<CTextManager>.GetInstance().GetText(CMallSystem.s_payTypeNameKeys[3]), (dwCoinPrice - diamond).ToString(), Singleton<CTextManager>.GetInstance().GetText(CMallSystem.s_payTypeNameKeys[2]) };
                            str2 = string.Format(Singleton<CTextManager>.GetInstance().GetText("MixPayNotice"), objArray1);
                        }
                    }
                    string[] args = new string[] { gridShopInfo.dwCoinPrice.ToString(), Singleton<CTextManager>.GetInstance().GetText(CMallSystem.s_payTypeNameKeys[(int) payType]), Singleton<CTextManager>.GetInstance().GetText(CMallSystem.s_payPurposeNameKeys[4]), goodName, str2 };
                    string strContent = string.Format(Singleton<CTextManager>.GetInstance().GetText("ConfirmPay", args), new object[0]);
                    Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(strContent, enUIEventID.Symbol_ConfirmWhenMoneyNotEnough, enUIEventID.None, uiEvent.m_eventParams, false);
                }
                else
                {
                    CMallSystem.TryToPay(enPayPurpose.Open, goodName, CMallSystem.ResBuyTypeToPayType(gridShopInfo.bCoinType), gridShopInfo.dwCoinPrice, enUIEventID.Symbol_ConfirmBuyGrid, ref uiEvent.m_eventParams, enUIEventID.None, true, true);
                }
            }
        }

        private void OnSelectBreakLvlItem(CUIEvent uiEvent)
        {
            this.RefreshSymbolBreakForm();
        }

        private void OnSymbol_PageAddClick(CUIEvent uiEvent)
        {
            OnBuyNewSymbolPage();
        }

        private void OnSymbolBagElementEnable(CUIEvent uiEvent)
        {
            int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
            GameObject gameObject = uiEvent.m_srcWidget.transform.Find("itemCell").gameObject;
            if (((this.m_selectMenuType == enSymbolMenuType.SymbolEquip) && (srcWidgetIndexInBelongedList >= 0)) && (srcWidgetIndexInBelongedList < this.m_pageSymbolBagList.Count))
            {
                this.SetSymbolListItem(uiEvent.m_srcFormScript, gameObject, this.m_pageSymbolBagList[srcWidgetIndexInBelongedList]);
            }
        }

        private void OnSymbolBagItemClick(CUIEvent uiEvent)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            CSymbolItem toSymbol = this.m_pageSymbolBagList[uiEvent.m_srcWidgetIndexInBelongedList];
            if (toSymbol.m_SymbolData.wLevel > masterRoleInfo.m_symbolInfo.m_pageMaxLvlLimit)
            {
                int minWearPvpLvl = CSymbolInfo.GetMinWearPvpLvl(toSymbol.m_SymbolData.wLevel);
                string[] args = new string[] { minWearPvpLvl.ToString() };
                Singleton<CUIManager>.GetInstance().OpenTips(Singleton<CTextManager>.GetInstance().GetText("Symbol_LvLimit", args), false, 1f, null, new object[0]);
            }
            else if (toSymbol.GetPageWearCnt(this.m_symbolPageIndex) >= s_maxSameIDSymbolEquipNum)
            {
                Singleton<CUIManager>.GetInstance().OpenTips(Singleton<CTextManager>.GetInstance().GetText("Symbol_CountLimit"), false, 1f, null, new object[0]);
            }
            else if (this.IsSymbolChangePanelShow(uiEvent.m_srcFormScript))
            {
                this.RefreshSymbolChangePanel(this.m_curViewSymbol, toSymbol);
                GameObject widget = uiEvent.m_srcFormScript.GetWidget(0x11);
                GameObject obj3 = uiEvent.m_srcFormScript.GetWidget(0x13);
                stUIEventParams eventParams = new stUIEventParams();
                eventParams.symbolParam.symbol = toSymbol;
                widget.GetComponent<CUIEventScript>().SetUIEvent(enUIEventType.Click, enUIEventID.Symbol_ChangeConfirm, eventParams);
                widget.CustomSetActive(true);
                obj3.CustomSetActive(false);
            }
            else
            {
                SendWearSymbol((byte) this.m_symbolPageIndex, (byte) this.m_selectSymbolPos, toSymbol.m_objID);
            }
        }

        private void OnSymbolBagShow(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (form != null)
            {
                form.GetWidget(3).CustomSetActive(true);
                form.GetWidget(5).CustomSetActive(false);
                this.SetSeletSymbolPos(uiEvent.m_eventParams.symbolParam.pos);
                this.ShowSymbolBag(false);
            }
        }

        private void OnSymbolBagSymbolViewHide(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (form != null)
            {
                form.GetWidget(3).CustomSetActive(false);
                form.GetWidget(5).CustomSetActive(false);
                form.GetWidget(15).CustomSetActive(false);
                form.GetWidget(0x10).CustomSetActive(true);
                this.SetSymbolItemSelect(this.m_selectSymbolPos, false);
            }
        }

        public static void OnSymbolBuySuccess(int pageCount, int buyCnt)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                masterRoleInfo.m_symbolInfo.SetSymbolPageCount(pageCount);
                masterRoleInfo.m_symbolInfo.SetSymbolPageBuyCnt(buyCnt);
            }
            Singleton<CSymbolSystem>.GetInstance().RefreshPageDropList();
            Singleton<CUIManager>.GetInstance().OpenTips("购买成功", false, 1f, null, new object[0]);
        }

        private void OnSymbolChangeConfirm(CUIEvent uiEvent)
        {
            CSymbolItem symbol = uiEvent.m_eventParams.symbolParam.symbol;
            if (symbol != null)
            {
                SendWearSymbol((byte) this.m_symbolPageIndex, (byte) this.m_selectSymbolPos, symbol.m_objID);
            }
        }

        public void OnSymbolEquip(bool bMoveNext)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (form != null)
            {
                form.GetWidget(15).CustomSetActive(false);
                form.GetWidget(0x10).CustomSetActive(true);
                this.RefreshSymbolPage();
                this.RefreshSymbolProp();
                this.RefreshPageDropList();
                if (!bMoveNext || !this.MovePosToNextCanEquipPos())
                {
                    this.OnSymbolBagSymbolViewHide(null);
                }
            }
        }

        public void OnSymbolEquipOff(bool bClear)
        {
            if (this.m_selectMenuType == enSymbolMenuType.SymbolEquip)
            {
                this.RefreshSymbolPage();
                this.RefreshSymbolProp();
                this.RefreshPageDropList();
                if (bClear)
                {
                    this.m_selectSymbolPos = -1;
                }
                else
                {
                    stUIEventParams par = new stUIEventParams();
                    par.symbolParam.symbol = null;
                    par.symbolParam.page = this.m_symbolPageIndex;
                    par.symbolParam.pos = this.m_selectSymbolPos;
                    Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Symbol_BagShow, par);
                }
            }
        }

        private void OnSymbolFormClose(CUIEvent uiEvent)
        {
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Tips_ItemInfoClose);
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Tips_CommonInfoClose);
        }

        public static void OnSymbolGridBuySuccess(int gridPos)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                masterRoleInfo.m_symbolInfo.UpdateBuyGridInfo(gridPos);
                Singleton<CSymbolSystem>.GetInstance().RefreshSymbolForm();
            }
        }

        private void OnSymbolLevelMenuSelect(CUIEvent uiEvent)
        {
            CUIListScript component = uiEvent.m_srcWidget.GetComponent<CUIListScript>();
            this.m_symbolFilterLevel = component.GetSelectedIndex() + 1;
            this.SetSymbolMakeListData();
            this.RefreshSymbolMakeList(uiEvent.m_srcFormScript);
        }

        private void OnSymbolMakeListClick(CUIEvent uiEvent)
        {
            this.m_curTransformSymbol = uiEvent.m_eventParams.symbolTransParam.symbolCfgInfo;
            Singleton<CUIManager>.GetInstance().OpenForm(s_symbolTransformPath, false, true);
            this.RefreshSymbolTransformForm();
        }

        private void OnSymbolMakeListEnable(CUIEvent uiEvent)
        {
            int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
            GameObject srcWidget = uiEvent.m_srcWidget;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                CUseableContainer useableContainer = masterRoleInfo.GetUseableContainer(enCONTAINER_TYPE.ITEM);
                ResSymbolInfo info2 = this.m_symbolMakeList[srcWidgetIndexInBelongedList];
                if ((useableContainer != null) && (info2 != null))
                {
                    CUseable useableByBaseID = useableContainer.GetUseableByBaseID(COM_ITEM_TYPE.COM_OBJTYPE_ITEMSYMBOL, info2.dwID);
                    Image component = srcWidget.transform.Find("iconImage").GetComponent<Image>();
                    Text text = srcWidget.transform.Find("countText").GetComponent<Text>();
                    Text text2 = srcWidget.transform.Find("nameText").GetComponent<Text>();
                    Text text3 = srcWidget.transform.Find("descText").GetComponent<Text>();
                    component.SetSprite(string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Icon_Dir, info2.dwIcon), uiEvent.m_srcFormScript, true, false, false);
                    text.text = (useableByBaseID == null) ? string.Empty : useableByBaseID.m_stackCount.ToString();
                    text2.text = StringHelper.UTF8BytesToString(ref info2.szName);
                    text3.text = GetSymbolAttString(info2.dwID, true);
                    CUIEventScript script = srcWidget.GetComponent<CUIEventScript>();
                    if (script != null)
                    {
                        stUIEventParams eventParams = new stUIEventParams();
                        eventParams.symbolTransParam.symbolCfgInfo = this.m_symbolMakeList[srcWidgetIndexInBelongedList];
                        script.SetUIEvent(enUIEventType.Click, enUIEventID.SymbolMake_ListItemClick, eventParams);
                    }
                    if (useableByBaseID != null)
                    {
                        CUICommonSystem.PlayAnimator(srcWidget, "Symbol_Normal");
                    }
                    else
                    {
                        CUICommonSystem.PlayAnimator(srcWidget, "Symbol_Disabled");
                    }
                }
            }
        }

        public void OnSymbolPageChange()
        {
            this.RefreshSymbolPage();
            this.RefreshSymbolProp();
            this.OnSymbolBagSymbolViewHide(null);
        }

        private void OnSymbolPageClear(CUIEvent uiEvent)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if ((masterRoleInfo != null) && masterRoleInfo.m_symbolInfo.IsPageEmpty(this.m_symbolPageIndex))
            {
                Singleton<CUIManager>.GetInstance().OpenTips("Symbol_Clear_NoWear_Tip", true, 1f, null, new object[0]);
            }
            else
            {
                string text = Singleton<CTextManager>.GetInstance().GetText("Symbol_Page_Clear_Tip");
                Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(text, enUIEventID.Symbol_PageClearConfirm, enUIEventID.None, false);
            }
        }

        private void OnSymbolPageClearConfirm(CUIEvent uiEvent)
        {
            SendReqClearSymbolPage(this.m_symbolPageIndex);
        }

        public void OnSymbolPageDownBtnClick(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (form != null)
            {
                Transform transform = form.GetWidget(1).transform.Find("DropList/List");
                transform.gameObject.CustomSetActive(!transform.gameObject.activeSelf);
            }
        }

        private void OnSymbolPageItemSelect(CUIEvent uiEvent)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo == null)
            {
                DebugHelper.Assert(false, "OnSymbolPageItemSelect role is null");
            }
            else
            {
                CUIListScript component = uiEvent.m_srcWidget.GetComponent<CUIListScript>();
                int elementAmount = component.GetElementAmount();
                int selectedIndex = component.GetSelectedIndex();
                GameObject widget = uiEvent.m_srcFormScript.GetWidget(1);
                widget.transform.Find("DropList/List").gameObject.CustomSetActive(false);
                if ((elementAmount == masterRoleInfo.m_symbolInfo.m_pageCount) || ((elementAmount > masterRoleInfo.m_symbolInfo.m_pageCount) && (selectedIndex != (elementAmount - 1))))
                {
                    this.m_symbolPageIndex = selectedIndex;
                    this.OnSymbolPageChange();
                    widget.transform.Find("DropList/Button_Down/Text").GetComponent<Text>().text = masterRoleInfo.m_symbolInfo.GetSymbolPageName(this.m_symbolPageIndex);
                    widget.transform.Find("DropList/Button_Down/SymbolLevel/Text").GetComponent<Text>().text = masterRoleInfo.m_symbolInfo.GetSymbolPageMaxLvl(this.m_symbolPageIndex).ToString();
                }
            }
        }

        private void OnSymbolRevertToVisible(CUIEvent uiEvent)
        {
        }

        private void OnSymbolTypeMenuSelect(CUIEvent uiEvent)
        {
            int selectedIndex = uiEvent.m_srcWidget.GetComponent<CUIListScript>().GetSelectedIndex();
            this.m_symbolFilterType = (enSymbolType) selectedIndex;
            this.SetSymbolMakeListData();
            this.RefreshSymbolMakeList(uiEvent.m_srcFormScript);
        }

        private void OnSymbolView(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (form != null)
            {
                form.GetWidget(5).CustomSetActive(true);
                form.GetWidget(3).CustomSetActive(false);
            }
            this.SetSeletSymbolPos(uiEvent.m_eventParams.symbolParam.pos);
            this.ShowSymbolView(uiEvent);
        }

        private void OpenSymbolForm()
        {
            MonoSingleton<NewbieGuideManager>.GetInstance().SetNewbieBit(14, true);
            CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(s_symbolFormPath, false, true);
            if (script != null)
            {
                string text = Singleton<CTextManager>.GetInstance().GetText("Symbol_Sheet_Symbol");
                string str2 = Singleton<CTextManager>.GetInstance().GetText("Symbol_Sheet_Make");
                string[] titleList = new string[] { text, str2 };
                CUICommonSystem.InitMenuPanel(script.GetWidget(0), titleList, (int) this.m_selectMenuType);
            }
        }

        private void PlayBatchBreakAnimator()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolBreakPath);
            if (null != form)
            {
                Transform transform = form.transform.Find("Panel_SymbolBreak/Panel_Content");
                for (int i = 0; i < 4; i++)
                {
                    GameObject gameObject = transform.Find(string.Format("breakElement{0}", i)).gameObject;
                    if (gameObject.transform.Find("OnBreakBtn").GetComponent<Toggle>().isOn)
                    {
                        CUICommonSystem.PlayAnimator(gameObject.transform.Find("Img_Lv").gameObject, "FenjieAnimation");
                    }
                }
                Singleton<CSoundManager>.GetInstance().PostEvent("UI_fuwen_fenjie_piliang", null);
            }
        }

        private void PlaySingleBreakAnimator()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolTransformPath);
            if (null != form)
            {
                CUICommonSystem.PlayAnimator(form.transform.Find("Panel_SymbolTranform/Panel_Content/iconImage").gameObject, "FenjieAnimation");
                Singleton<CSoundManager>.GetInstance().PostEvent("UI_fuwen_fenjie_dange", null);
            }
        }

        [MessageHandler(0x486)]
        public static void ReciveSymbolBreakRsp(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            SCPKG_CMD_SYMBOL_BREAK stSymbolBreakRsp = msg.stPkgData.stSymbolBreakRsp;
            int num = 0;
            for (int i = 0; i < stSymbolBreakRsp.wSymbolCnt; i++)
            {
                ResSymbolInfo dataByKey = GameDataMgr.symbolInfoDatabin.GetDataByKey(stSymbolBreakRsp.astSymbolList[i].dwSymbolID);
                if (dataByKey != null)
                {
                    num += (int) (dataByKey.dwBreakCoin * stSymbolBreakRsp.astSymbolList[i].iSymbolCnt);
                }
            }
            s_breakSymbolCoinCnt = num;
            if (num > 0)
            {
                if (stSymbolBreakRsp.bBreakType == 0)
                {
                    Singleton<CSymbolSystem>.GetInstance().PlaySingleBreakAnimator();
                    Singleton<CSymbolSystem>.GetInstance().RefreshSymbolTransformForm();
                }
                else if (stSymbolBreakRsp.bBreakType == 1)
                {
                    Singleton<CSymbolSystem>.GetInstance().PlayBatchBreakAnimator();
                    Singleton<CSymbolSystem>.GetInstance().RefreshSymbolBreakForm();
                }
            }
        }

        [MessageHandler(0x470)]
        public static void ReciveSymbolChange(CSPkg msg)
        {
            CSymbolInfo.enSymbolOperationType type;
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            SCPKG_CMD_SYMBOLCHG stSymbolChg = msg.stPkgData.stSymbolChg;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            uint cfgId = 0;
            masterRoleInfo.m_symbolInfo.OnSymbolChange(stSymbolChg.bPageIdx, stSymbolChg.bPosIdx, stSymbolChg.ullUniqueID, out cfgId, out type);
            switch (type)
            {
                case CSymbolInfo.enSymbolOperationType.Wear:
                case CSymbolInfo.enSymbolOperationType.Replace:
                {
                    string symbolWearTip = GetSymbolWearTip(cfgId, true);
                    Singleton<CUIManager>.GetInstance().OpenTips(symbolWearTip, false, 1f, null, new object[0]);
                    if (type == CSymbolInfo.enSymbolOperationType.Wear)
                    {
                        Singleton<CSoundManager>.GetInstance().PostEvent("UI_fuwen_zhuangbei", null);
                    }
                    Singleton<CSymbolSystem>.GetInstance().OnSymbolEquip(type == CSymbolInfo.enSymbolOperationType.Wear);
                    break;
                }
                default:
                    if (type == CSymbolInfo.enSymbolOperationType.TakeOff)
                    {
                        string strContent = GetSymbolWearTip(cfgId, false);
                        Singleton<CUIManager>.GetInstance().OpenTips(strContent, false, 1f, null, new object[0]);
                        Singleton<CSoundManager>.GetInstance().PostEvent("UI_rune_dblclick", null);
                        Singleton<CSymbolSystem>.GetInstance().OnSymbolEquipOff(false);
                    }
                    break;
            }
            Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.SymbolEquipSuc);
            Singleton<CBagSystem>.GetInstance().RefreshBagForm();
        }

        [MessageHandler(0x46d)]
        public static void ReciveSymbolDatail(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            SCPKG_CMD_SYMBOLDETAIL stSymbolDetail = msg.stPkgData.stSymbolDetail;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                masterRoleInfo.m_symbolInfo.SetData(stSymbolDetail.stSymbolInfo);
                Singleton<CSymbolSystem>.GetInstance().RefreshSymbolForm();
            }
            else
            {
                DebugHelper.Assert(false, "ReciveSymbolDatail Master RoleInfo is NULL!!!");
            }
        }

        [MessageHandler(0x485)]
        public static void ReciveSymbolMakeRsp(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            SCPKG_CMD_SYMBOL_MAKE stSymbolMakeRsp = msg.stPkgData.stSymbolMakeRsp;
            if (stSymbolMakeRsp.iResult == 0)
            {
                CUseable useable = CUseableManager.CreateUseable(COM_ITEM_TYPE.COM_OBJTYPE_ITEMSYMBOL, stSymbolMakeRsp.stSymbolInfo.dwSymbolID, stSymbolMakeRsp.stSymbolInfo.iSymbolCnt);
                if ((useable != null) && (((CSymbolItem) useable) != null))
                {
                    CUseableContainer container = new CUseableContainer(enCONTAINER_TYPE.ITEM);
                    container.Add(useable);
                    CUICommonSystem.ShowSymbol(container, enUIEventID.None);
                }
                Singleton<CSymbolSystem>.GetInstance().RefreshSymbolTransformForm();
            }
        }

        [MessageHandler(0x462)]
        public static void ReciveSymbolNameChange(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            SCPKG_CMD_SYMBOLNAMECHG stSymbolNameChgRsp = msg.stPkgData.stSymbolNameChgRsp;
            if (stSymbolNameChgRsp.iResult == 0)
            {
                int bPageIdx = stSymbolNameChgRsp.bPageIdx;
                string pageName = StringHelper.UTF8BytesToString(ref stSymbolNameChgRsp.szPageName);
                Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().m_symbolInfo.SetSymbolPageName(bPageIdx, pageName);
                Singleton<CSymbolSystem>.GetInstance().RefreshPageDropList();
            }
            else if (stSymbolNameChgRsp.iResult == 0x7d)
            {
                Singleton<CUIManager>.GetInstance().OpenTips("Symbol_Name_illegal", true, 1f, null, new object[0]);
            }
        }

        [MessageHandler(0x48b)]
        public static void ReciveSymbolPageClear(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            SCPKG_CMD_SYMBOLPAGE_CLR stSymbolPageClrRsp = msg.stPkgData.stSymbolPageClrRsp;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                masterRoleInfo.m_symbolInfo.OnSymbolPageClear(stSymbolPageClrRsp.bSymbolPageIdx);
            }
            Singleton<CSymbolSystem>.GetInstance().OnSymbolEquipOff(true);
            Singleton<CBagSystem>.GetInstance().RefreshBagForm();
        }

        private void RefreshPageDropList()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (form != null)
            {
                this.SetPageDropListData(form, this.m_symbolPageIndex);
            }
        }

        private static void RefreshPropPanel(GameObject propPanel, ref int[] propArr, ref int[] propPctArr)
        {
            int num = 0x24;
            int amount = 0;
            for (int i = 0; i < num; i++)
            {
                if ((propArr[i] > 0) || (propPctArr[i] > 0))
                {
                    amount++;
                }
            }
            CUIListScript component = propPanel.GetComponent<CUIListScript>();
            component.SetElementAmount(amount);
            amount = 0;
            for (int j = 0; j < num; j++)
            {
                if ((propArr[j] > 0) || (propPctArr[j] > 0))
                {
                    CUIListElementScript elemenet = component.GetElemenet(amount);
                    DebugHelper.Assert(elemenet != null);
                    if (elemenet != null)
                    {
                        Text text = elemenet.gameObject.transform.Find("titleText").GetComponent<Text>();
                        Text text2 = elemenet.gameObject.transform.Find("valueText").GetComponent<Text>();
                        DebugHelper.Assert(text != null);
                        if (text != null)
                        {
                            text.text = CUICommonSystem.s_attNameList[j];
                        }
                        DebugHelper.Assert(text2 != null);
                        if (text2 != null)
                        {
                            if (propArr[j] > 0)
                            {
                                if (CUICommonSystem.s_pctFuncEftList.IndexOf((uint) j) != -1)
                                {
                                    text2.text = string.Format("+{0}", CUICommonSystem.GetValuePercent(propArr[j] / 100));
                                }
                                else
                                {
                                    text2.text = string.Format("+{0}", ((float) propArr[j]) / 100f);
                                }
                            }
                            else if (propPctArr[j] > 0)
                            {
                                text2.text = string.Format("+{0}", CUICommonSystem.GetValuePercent(propPctArr[j]));
                            }
                        }
                    }
                    amount++;
                }
            }
        }

        private void RefreshSymbolBag()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (form != null)
            {
                Transform transform = form.gameObject.transform.Find(s_symbolBagPanel);
                transform.Find("Panel_BagList/List").GetComponent<CUIListScript>().SetElementAmount(this.m_pageSymbolBagList.Count);
                GameObject gameObject = transform.Find("Panel_BagList/lblTips").gameObject;
                bool flag = this.IsSymbolChangePanelShow(form);
                if (this.m_pageSymbolBagList.Count == 0)
                {
                    gameObject.CustomSetActive(true);
                }
                else
                {
                    gameObject.CustomSetActive(false);
                }
                GameObject widget = form.GetWidget(0x12);
                widget.CustomSetActive(!flag || (this.m_pageSymbolBagList.Count == 0));
                if (CSysDynamicBlock.bLobbyEntryBlocked)
                {
                    widget.CustomSetActive(false);
                }
                form.GetWidget(0x11).CustomSetActive(false);
                form.GetWidget(0x13).CustomSetActive(flag && (this.m_pageSymbolBagList.Count > 0));
            }
        }

        private void RefreshSymbolBreakForm()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolBreakPath);
            if (null != form)
            {
                Transform transform = form.transform.Find("Panel_SymbolBreak/Panel_Content");
                int num = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (transform.Find(string.Format("breakElement{0}", i)).gameObject.transform.Find("OnBreakBtn").GetComponent<Toggle>().isOn)
                    {
                        num |= ((int) 1) << (i + 1);
                    }
                }
                this.m_breakLevelMask = (ushort) num;
                form.transform.Find("Panel_SymbolBreak/Panel_Bottom/Pl_countText/symbolCoinCntText").GetComponent<Text>().text = this.GetBreakExcessSymbolCoinCnt(this.m_breakLevelMask).ToString();
            }
        }

        private void RefreshSymbolChangePanel(CSymbolItem fromSymbol, CSymbolItem toSymbol)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if ((null != form) && (fromSymbol != null))
            {
                GameObject widget = form.GetWidget(15);
                if (null != widget)
                {
                    GameObject gameObject = widget.transform.Find("symbolFrom").gameObject;
                    this.RefreshSymbolInfo(form, gameObject, fromSymbol);
                    GameObject obj4 = widget.transform.Find("symbolTo").gameObject;
                    obj4.CustomSetActive(toSymbol != null);
                    if (toSymbol != null)
                    {
                        this.RefreshSymbolInfo(form, obj4, toSymbol);
                    }
                }
            }
        }

        public void RefreshSymbolCntText()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (form != null)
            {
                Text component = form.GetWidget(14).transform.Find("symbolCoinCntText").GetComponent<Text>();
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                if (component != null)
                {
                    if (masterRoleInfo != null)
                    {
                        component.text = masterRoleInfo.SymbolCoin.ToString();
                    }
                    else
                    {
                        component.text = string.Empty;
                    }
                }
            }
        }

        public void RefreshSymbolEquipForm()
        {
            if (Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath) != null)
            {
                this.SetSymbolData();
                this.RefreshSymbolPage();
                this.RefreshPageDropList();
                this.RefreshSymbolProp();
                this.OnSymbolBagSymbolViewHide(null);
            }
        }

        public void RefreshSymbolForm()
        {
            if (Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath) != null)
            {
                if (this.m_selectMenuType == enSymbolMenuType.SymbolEquip)
                {
                    this.RefreshSymbolEquipForm();
                }
                else
                {
                    this.RefreshSymbolMakeForm();
                }
            }
        }

        private void RefreshSymbolInfo(CUIFormScript form, GameObject symbolPanel, CSymbolItem symbol)
        {
            if ((null != symbolPanel) && (symbol != null))
            {
                symbolPanel.transform.Find("itemCell/imgIcon").GetComponent<Image>().SetSprite(symbol.GetIconPath(), form, true, false, false);
                symbolPanel.transform.Find("lblName").GetComponent<Text>().text = symbol.m_name;
                RefreshSymbolPropContent(symbolPanel.transform.Find("symbolPropPanel").gameObject, symbol.m_baseID);
            }
        }

        public void RefreshSymbolMakeForm()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (form != null)
            {
                CTextManager instance = Singleton<CTextManager>.GetInstance();
                string[] titleList = new string[] { instance.GetText("Symbol_Prop_Tab_All"), instance.GetText("Symbol_Prop_Tab_Atk"), instance.GetText("Symbol_Prop_Tab_Hp"), instance.GetText("Symbol_Prop_Tab_Defense"), instance.GetText("Symbol_Prop_Tab_Function"), instance.GetText("Symbol_Prop_Tab_HpSteal"), instance.GetText("Symbol_Prop_Tab_AtkSpeed"), instance.GetText("Symbol_Prop_Tab_Crit"), instance.GetText("Symbol_Prop_Tab_Penetrate") };
                CUICommonSystem.InitMenuPanel(form.GetWidget(10), titleList, (int) this.m_symbolFilterType);
                string[] strArray2 = new string[] { "1", "2", "3", "4", "5" };
                CUICommonSystem.InitMenuPanel(form.GetWidget(11), strArray2, this.m_symbolFilterLevel - 1);
                this.SetSymbolData();
                this.SetSymbolMakeListData();
                this.RefreshSymbolMakeList(form);
                form.GetWidget(13).GetComponent<Text>().text = this.GetBreakExcessSymbolCoinCnt(0xffff).ToString();
            }
        }

        private void RefreshSymbolMakeList(CUIFormScript form)
        {
            if (form != null)
            {
                form.GetWidget(12).GetComponent<CUIListScript>().SetElementAmount(this.m_symbolMakeList.Count);
            }
        }

        private void RefreshSymbolPage()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (form != null)
            {
                Transform transform = form.gameObject.transform.Find(s_symbolPagePanel);
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                if (masterRoleInfo != null)
                {
                    ulong[] pageSymbolData = masterRoleInfo.m_symbolInfo.GetPageSymbolData(this.m_symbolPageIndex);
                    if (pageSymbolData != null)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            Transform transform2 = transform.Find(string.Format("itemCell{0}", i));
                            if (transform2 != null)
                            {
                                this.SetSymbolItem(form, transform2.gameObject, i, pageSymbolData[i]);
                            }
                        }
                        form.GetWidget(20).GetComponent<Text>().text = masterRoleInfo.m_symbolInfo.GetSymbolPageMaxLvl(this.m_symbolPageIndex).ToString();
                    }
                }
            }
        }

        public void RefreshSymbolPageProp(GameObject propListPanel, int pageIndex, bool bPvp = true)
        {
            if (propListPanel != null)
            {
                Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().m_symbolInfo.GetSymbolPageProp(pageIndex, ref s_symbolPagePropArr, ref s_symbolPagePropPctArr, bPvp);
                RefreshPropPanel(propListPanel, ref s_symbolPagePropArr, ref s_symbolPagePropPctArr);
            }
        }

        public void RefreshSymbolPagePveEnhanceProp(GameObject propList, int pageIndex)
        {
            if (propList != null)
            {
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                masterRoleInfo.m_symbolInfo.GetSymbolPageProp(pageIndex, ref s_symbolPagePropArr, ref s_symbolPagePropPctArr, true);
                masterRoleInfo.m_symbolInfo.GetSymbolPageProp(pageIndex, ref s_symbolPropValAddArr, ref s_symbolPropPctAddArr, false);
                int num = 0x24;
                for (int i = 0; i < num; i++)
                {
                    s_symbolPropValAddArr[i] -= s_symbolPagePropArr[i];
                    s_symbolPropPctAddArr[i] -= s_symbolPagePropPctArr[i];
                }
                RefreshPropPanel(propList, ref s_symbolPropValAddArr, ref s_symbolPropPctAddArr);
            }
        }

        private void RefreshSymbolProp()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (form != null)
            {
                GameObject gameObject = form.GetWidget(6).transform.Find("propList").gameObject;
                this.RefreshSymbolPageProp(gameObject, this.m_symbolPageIndex, true);
                GameObject propList = form.GetWidget(7).transform.Find("propList").gameObject;
                this.RefreshSymbolPagePveEnhanceProp(propList, this.m_symbolPageIndex);
            }
        }

        private static void RefreshSymbolPropContent(GameObject propPanel, uint symbolId)
        {
            ResSymbolInfo dataByKey = GameDataMgr.symbolInfoDatabin.GetDataByKey(symbolId);
            if ((propPanel != null) && (dataByKey != null))
            {
                GetSymbolProp(symbolId, ref s_symbolPagePropArr, ref s_symbolPagePropPctArr, true);
                GetSymbolProp(symbolId, ref s_symbolPropValAddArr, ref s_symbolPropPctAddArr, false);
                int num = 0x24;
                for (int i = 0; i < num; i++)
                {
                    s_symbolPropValAddArr[i] -= s_symbolPagePropArr[i];
                    s_symbolPropPctAddArr[i] -= s_symbolPagePropPctArr[i];
                }
                RefreshPropPanel(propPanel.transform.Find("pvpPropPanel").Find("propList").gameObject, ref s_symbolPagePropArr, ref s_symbolPagePropPctArr);
                RefreshPropPanel(propPanel.transform.Find("pveEnhancePropPanel").Find("propList").gameObject, ref s_symbolPropValAddArr, ref s_symbolPropPctAddArr);
            }
        }

        private void RefreshSymbolTransformForm()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolTransformPath);
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (((form != null) && (this.m_curTransformSymbol != null)) && (masterRoleInfo != null))
            {
                GameObject gameObject = form.transform.Find("Panel_SymbolTranform/Panel_Content").gameObject;
                gameObject.transform.Find("iconImage").GetComponent<Image>().SetSprite(string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Icon_Dir, this.m_curTransformSymbol.dwIcon), form, true, false, false);
                gameObject.transform.Find("nameText").GetComponent<Text>().text = StringHelper.UTF8BytesToString(ref this.m_curTransformSymbol.szName);
                Text component = gameObject.transform.Find("countText").GetComponent<Text>();
                component.text = string.Empty;
                int useableStackCount = 0;
                CUseableContainer useableContainer = masterRoleInfo.GetUseableContainer(enCONTAINER_TYPE.ITEM);
                if (useableContainer != null)
                {
                    useableStackCount = useableContainer.GetUseableStackCount(COM_ITEM_TYPE.COM_OBJTYPE_ITEMSYMBOL, this.m_curTransformSymbol.dwID);
                    CTextManager instance = Singleton<CTextManager>.GetInstance();
                    component.text = (useableStackCount <= 0) ? instance.GetText("Symbol_Not_Own") : string.Format(instance.GetText("Symbol_Own_Cnt"), useableStackCount);
                }
                RefreshSymbolPropContent(gameObject.transform.Find("symbolPropPanel").gameObject, this.m_curTransformSymbol.dwID);
                gameObject.transform.Find("makeCoinText").GetComponent<Text>().text = this.m_curTransformSymbol.dwMakeCoin.ToString();
                gameObject.transform.Find("breakCoinText").GetComponent<Text>().text = this.m_curTransformSymbol.dwBreakCoin.ToString();
                GameObject obj4 = gameObject.transform.Find("btnBreak").gameObject;
                obj4.GetComponent<Button>().interactable = useableStackCount > 0;
                obj4.GetComponent<CUIEventScript>().enabled = useableStackCount > 0;
            }
        }

        public static void SendBuySymbol(int symbolIndex)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x459);
            msg.stPkgData.stShopBuyReq = new CSPKG_CMD_SHOPBUY();
            msg.stPkgData.stShopBuyReq.iBuyType = 7;
            msg.stPkgData.stShopBuyReq.iBuySubType = symbolIndex;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void SendChgSymbolPageName(int pageIndex, string pageName)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x461);
            msg.stPkgData.stSymbolNameChgReq.bPageIdx = (byte) pageIndex;
            StringHelper.StringToUTF8Bytes(pageName, ref msg.stPkgData.stSymbolNameChgReq.szPageName);
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void SendQuerySymbol()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x46c);
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void SendReqClearSymbolPage(int pageIndex)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x48a);
            msg.stPkgData.stSymbolPageClrReq.bSymbolPageIdx = (byte) pageIndex;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void SendReqExcessSymbolBreak(ListView<CSDT_SYMBOLOPT_INFO> breakSymbolList)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x484);
            msg.stPkgData.stSymbolBreak.wSymbolCnt = (ushort) breakSymbolList.Count;
            msg.stPkgData.stSymbolBreak.bBreakType = 1;
            for (int i = 0; (i < breakSymbolList.Count) && (i < 400); i++)
            {
                msg.stPkgData.stSymbolBreak.astSymbolList[i] = breakSymbolList[i];
            }
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void SendReqSymbolBreak(uint symbolId, int cnt)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x484);
            msg.stPkgData.stSymbolBreak.wSymbolCnt = 1;
            msg.stPkgData.stSymbolBreak.bBreakType = 0;
            CSDT_SYMBOLOPT_INFO csdt_symbolopt_info = new CSDT_SYMBOLOPT_INFO {
                dwSymbolID = symbolId,
                iSymbolCnt = cnt
            };
            msg.stPkgData.stSymbolBreak.astSymbolList[0] = csdt_symbolopt_info;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void SendReqSymbolMake(uint symbolId, int cnt)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x483);
            msg.stPkgData.stSymbolMake.stSymbolInfo.dwSymbolID = symbolId;
            msg.stPkgData.stSymbolMake.stSymbolInfo.iSymbolCnt = cnt;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void SendWearSymbol(byte page, byte pos, ulong objId)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x46e);
            msg.stPkgData.stSymbolWear.bPage = page;
            msg.stPkgData.stSymbolWear.bPos = pos;
            msg.stPkgData.stSymbolWear.ullUniqueID = objId;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        private void SetBreakSymbolList()
        {
            this.m_breakSymbolList.Clear();
            int count = this.m_symbolList.Count;
            for (int i = 0; i < count; i++)
            {
                if (this.CheckSymbolBreak(this.m_symbolList[i], this.m_breakLevelMask))
                {
                    CSDT_SYMBOLOPT_INFO item = new CSDT_SYMBOLOPT_INFO {
                        dwSymbolID = this.m_symbolList[i].m_baseID,
                        iSymbolCnt = this.m_symbolList[i].m_stackCount - this.m_symbolList[i].GetMaxWearCnt()
                    };
                    this.m_breakSymbolList.Add(item);
                }
            }
        }

        public void SetPageDropListData(CUIFormScript form, int selectIndex)
        {
            GameObject widget = form.GetWidget(1);
            Transform transform = widget.transform.Find("DropList/List");
            transform.gameObject.CustomSetActive(false);
            CUIListScript component = transform.GetComponent<CUIListScript>();
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            bool flag = masterRoleInfo.m_symbolInfo.IsPageFull();
            int nextFreePageLevel = masterRoleInfo.m_symbolInfo.GetNextFreePageLevel();
            component.SetElementAmount((nextFreePageLevel <= masterRoleInfo.PvpLevel) ? masterRoleInfo.m_symbolInfo.m_pageCount : (masterRoleInfo.m_symbolInfo.m_pageCount + 1));
            for (int i = 0; i < masterRoleInfo.m_symbolInfo.m_pageCount; i++)
            {
                CUIListElementScript elemenet = component.GetElemenet(i);
                if (elemenet != null)
                {
                    Text text = elemenet.gameObject.transform.Find("Text").GetComponent<Text>();
                    text.text = masterRoleInfo.m_symbolInfo.GetSymbolPageName(i);
                    text.GetComponent<RectTransform>().anchoredPosition = s_symbolPos1;
                    elemenet.gameObject.transform.Find("SymbolLevel/Text").GetComponent<Text>().text = masterRoleInfo.m_symbolInfo.GetSymbolPageMaxLvl(i).ToString();
                    elemenet.gameObject.transform.Find("SymbolLevel").gameObject.CustomSetActive(true);
                }
            }
            if (nextFreePageLevel > masterRoleInfo.PvpLevel)
            {
                CUIListElementScript script3 = component.GetElemenet(masterRoleInfo.m_symbolInfo.m_pageCount);
                if (script3 != null)
                {
                    Text text3 = script3.gameObject.transform.Find("Text").GetComponent<Text>();
                    text3.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Symbol_Free_GetPage"), nextFreePageLevel);
                    text3.GetComponent<RectTransform>().anchoredPosition = s_symbolPos2;
                    script3.gameObject.transform.Find("SymbolLevel").gameObject.CustomSetActive(false);
                }
            }
            component.SelectElement(selectIndex, true);
            if (flag)
            {
                form.GetWidget(2).CustomSetActive(false);
            }
            widget.transform.Find("DropList/Button_Down/Text").GetComponent<Text>().text = masterRoleInfo.m_symbolInfo.GetSymbolPageName(this.m_symbolPageIndex);
            widget.transform.Find("DropList/Button_Down/SymbolLevel/Text").GetComponent<Text>().text = masterRoleInfo.m_symbolInfo.GetSymbolPageMaxLvl(selectIndex).ToString();
        }

        public static void SetPageDropListDataByHeroSelect(GameObject panelObj, int selectIndex)
        {
            if (panelObj != null)
            {
                Transform transform = panelObj.transform.Find("DropList/List");
                CUIListScript component = transform.GetComponent<CUIListScript>();
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                component.SetElementAmount(masterRoleInfo.m_symbolInfo.m_pageCount);
                for (int i = 0; i < masterRoleInfo.m_symbolInfo.m_pageCount; i++)
                {
                    CUIListElementScript elemenet = component.GetElemenet(i);
                    elemenet.gameObject.transform.Find("Text").GetComponent<Text>().text = masterRoleInfo.m_symbolInfo.GetSymbolPageName(i);
                    elemenet.gameObject.transform.Find("SymbolLevel/Text").GetComponent<Text>().text = masterRoleInfo.m_symbolInfo.GetSymbolPageMaxLvl(i).ToString();
                }
                component.SelectElement(selectIndex, true);
                panelObj.transform.Find("DropList/Button_Down/Text").GetComponent<Text>().text = masterRoleInfo.m_symbolInfo.GetSymbolPageName(selectIndex);
                panelObj.transform.Find("DropList/Button_Down/SymbolLevel/Text").GetComponent<Text>().text = masterRoleInfo.m_symbolInfo.GetSymbolPageMaxLvl(selectIndex).ToString();
                panelObj.transform.Find("DropList/Button_Down/SymbolLevel/Text").GetComponent<Text>().text = masterRoleInfo.m_symbolInfo.GetSymbolPageMaxLvl(selectIndex).ToString();
                transform.gameObject.CustomSetActive(false);
            }
        }

        private void SetPageSymbolData(bool bChange)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            ResSymbolPos symbolPos = CSymbolInfo.GetSymbolPos(this.m_selectSymbolPos);
            ListView<CSymbolItem> symbolList = new ListView<CSymbolItem>();
            this.m_pageSymbolBagList.Clear();
            int count = this.m_symbolList.Count;
            for (int i = 0; i < count; i++)
            {
                if (((!bChange || (this.m_curViewSymbol == null)) || (this.m_symbolList[i].m_SymbolData.dwID != this.m_curViewSymbol.m_SymbolData.dwID)) && (CSymbolInfo.CheckSymbolColor(symbolPos, this.m_symbolList[i].m_SymbolData.bColor) && (this.m_symbolList[i].m_stackCount > this.m_symbolList[i].GetPageWearCnt(this.m_symbolPageIndex))))
                {
                    if ((this.m_symbolList[i].m_SymbolData.wLevel <= masterRoleInfo.m_symbolInfo.m_pageMaxLvlLimit) && (this.m_symbolList[i].GetPageWearCnt(this.m_symbolPageIndex) < s_maxSameIDSymbolEquipNum))
                    {
                        this.m_pageSymbolBagList.Add(this.m_symbolList[i]);
                    }
                    else
                    {
                        symbolList.Add(this.m_symbolList[i]);
                    }
                }
            }
            this.SortSymbolList(ref this.m_pageSymbolBagList);
            this.SortSymbolList(ref symbolList);
            this.m_pageSymbolBagList.AddRange(symbolList);
        }

        private void SetSeletSymbolPos(int newPos)
        {
            this.SetSymbolItemSelect(this.m_selectSymbolPos, false);
            this.m_selectSymbolPos = newPos;
            this.SetSymbolItemSelect(this.m_selectSymbolPos, true);
        }

        private void SetSymbolData()
        {
            CUseableContainer useableContainer = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().GetUseableContainer(enCONTAINER_TYPE.ITEM);
            int curUseableCount = useableContainer.GetCurUseableCount();
            CUseable useableByIndex = null;
            this.m_symbolList.Clear();
            int index = 0;
            for (index = 0; index < curUseableCount; index++)
            {
                useableByIndex = useableContainer.GetUseableByIndex(index);
                if (useableByIndex.m_type == COM_ITEM_TYPE.COM_OBJTYPE_ITEMSYMBOL)
                {
                    CSymbolItem item = (CSymbolItem) useableByIndex;
                    if (item != null)
                    {
                        this.m_symbolList.Add(item);
                    }
                }
            }
        }

        private void SetSymbolItem(CUIFormScript formScript, GameObject item, int i, ulong objId)
        {
            if ((Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath) != null) && (item != null))
            {
                CUseable symbolByObjID = Singleton<CSymbolSystem>.GetInstance().GetSymbolByObjID(objId);
                CUIEventScript component = item.GetComponent<CUIEventScript>();
                Transform transform = item.transform;
                GameObject gameObject = transform.Find("bg").gameObject;
                GameObject obj3 = transform.Find("imgLock").gameObject;
                GameObject obj4 = transform.Find("lblOpenLevel").gameObject;
                GameObject obj5 = transform.Find("imgIcon").gameObject;
                GameObject obj6 = transform.Find("imgLevel").gameObject;
                GameObject obj7 = transform.Find("imgCanBuy").gameObject;
                Text text = obj4.transform.Find("Level").GetComponent<Text>();
                gameObject.CustomSetActive(true);
                obj3.CustomSetActive(false);
                obj4.CustomSetActive(false);
                obj5.CustomSetActive(false);
                obj6.CustomSetActive(false);
                obj7.CustomSetActive(false);
                int param = 0;
                enSymbolWearState state = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().m_symbolInfo.GetSymbolPosWearState(this.m_symbolPageIndex, i, out param);
                stUIEventParams eventParams = new stUIEventParams();
                switch (state)
                {
                    case enSymbolWearState.WearSuccess:
                        if (symbolByObjID != null)
                        {
                            obj5.GetComponent<Image>().SetSprite(symbolByObjID.GetIconPath(), formScript, false, false, false);
                            obj5.CustomSetActive(true);
                            eventParams.symbolParam.symbol = (CSymbolItem) symbolByObjID;
                            eventParams.symbolParam.page = this.m_symbolPageIndex;
                            eventParams.symbolParam.pos = i;
                            component.SetUIEvent(enUIEventType.Click, enUIEventID.Symbol_View, eventParams);
                        }
                        break;

                    case enSymbolWearState.OpenToWear:
                        eventParams.symbolParam.symbol = (CSymbolItem) symbolByObjID;
                        eventParams.symbolParam.page = this.m_symbolPageIndex;
                        eventParams.symbolParam.pos = i;
                        component.SetUIEvent(enUIEventType.Click, enUIEventID.Symbol_BagShow, eventParams);
                        break;

                    case enSymbolWearState.WillOpen:
                        text.text = "Lv." + param.ToString();
                        obj4.CustomSetActive(true);
                        eventParams.tag = param;
                        component.SetUIEvent(enUIEventType.Click, enUIEventID.Symbol_OpenUnlockLvlTip, eventParams);
                        break;

                    case enSymbolWearState.UnOpen:
                        obj3.CustomSetActive(true);
                        gameObject.CustomSetActive(false);
                        eventParams.tag = param;
                        component.SetUIEvent(enUIEventType.Click, enUIEventID.Symbol_OpenUnlockLvlTip, eventParams);
                        break;

                    case enSymbolWearState.CanBuy:
                        gameObject.CustomSetActive(false);
                        obj7.CustomSetActive(true);
                        eventParams.tag = i + 1;
                        component.SetUIEvent(enUIEventType.Click, enUIEventID.Symbol_PromptBuyGrid, eventParams);
                        break;

                    case enSymbolWearState.CanBuyAndWillOpen:
                        gameObject.CustomSetActive(false);
                        text.text = "Lv." + param.ToString();
                        obj4.CustomSetActive(true);
                        eventParams.tag = i + 1;
                        component.SetUIEvent(enUIEventType.Click, enUIEventID.Symbol_PromptBuyGrid, eventParams);
                        break;
                }
            }
        }

        private void SetSymbolItemSelect(int pos, bool isSelect = true)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_symbolFormPath);
            if (((form != null) && (pos >= 0)) && (pos < 30))
            {
                Transform transform2 = form.gameObject.transform.Find(s_symbolPagePanel).Find(string.Format("itemCell{0}/imgSelect", pos));
                if (transform2 != null)
                {
                    transform2.gameObject.CustomSetActive(isSelect);
                }
            }
        }

        private void SetSymbolListItem(CUIFormScript formScript, GameObject itemObj, CSymbolItem item)
        {
            if ((itemObj != null) && (item != null))
            {
                Image component = itemObj.transform.Find("imgIcon").GetComponent<Image>();
                component.SetSprite(item.GetIconPath(), formScript, true, false, false);
                itemObj.transform.Find("SymbolName").GetComponent<Text>().text = item.m_name;
                itemObj.transform.Find("SymbolDesc").GetComponent<Text>().text = GetSymbolAttString(item, true);
                Text text3 = itemObj.transform.Find("lblIconCount").GetComponent<Text>();
                int num = item.m_stackCount - item.GetPageWearCnt(this.m_symbolPageIndex);
                text3.text = string.Format("x{0}", num);
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                if ((item.m_SymbolData.wLevel <= masterRoleInfo.m_symbolInfo.m_pageMaxLvlLimit) && (item.GetPageWearCnt(this.m_symbolPageIndex) < s_maxSameIDSymbolEquipNum))
                {
                    component.color = CUIUtility.s_Color_White;
                }
                else
                {
                    component.color = CUIUtility.s_Color_GrayShader;
                }
            }
        }

        private void SetSymbolMakeListData()
        {
            this.m_symbolMakeList.Clear();
            int count = s_allSymbolCfgList.Count;
            for (int i = 0; i < count; i++)
            {
                if (((s_allSymbolCfgList[i] != null) && (s_allSymbolCfgList[i].wLevel == this.m_symbolFilterLevel)) && ((this.m_symbolFilterType == enSymbolType.All) || ((s_allSymbolCfgList[i].wType & (((int) 1) << this.m_symbolFilterType)) != 0)))
                {
                    this.m_symbolMakeList.Add(s_allSymbolCfgList[i]);
                }
            }
        }

        private void ShowSymbolBag(bool bChange = false)
        {
            this.SetPageSymbolData(bChange);
            this.RefreshSymbolBag();
        }

        private void ShowSymbolView(CUIEvent uiEvent)
        {
            GameObject widget = uiEvent.m_srcFormScript.GetWidget(5);
            Text component = widget.transform.Find("root/lblName").GetComponent<Text>();
            Image image = widget.transform.Find("root/itemCell0/imgIcon").GetComponent<Image>();
            CSymbolItem symbol = uiEvent.m_eventParams.symbolParam.symbol;
            this.m_curViewSymbol = symbol;
            component.text = symbol.m_name;
            image.SetSprite(symbol.GetIconPath(), uiEvent.m_srcFormScript, true, false, false);
            RefreshSymbolPropContent(widget.transform.Find("root/symbolPropPanel").gameObject, symbol.m_baseID);
        }

        public void SortSymbolList(ref ListView<CSymbolItem> symbolList)
        {
            int count = symbolList.Count;
            CSymbolItem item = null;
            for (int i = 0; i < (count - 1); i++)
            {
                for (int j = 0; j < ((count - 1) - i); j++)
                {
                    if (symbolList[j].m_SymbolData.wLevel < symbolList[j + 1].m_SymbolData.wLevel)
                    {
                        item = symbolList[j];
                        symbolList[j] = symbolList[j + 1];
                        symbolList[j + 1] = item;
                    }
                }
            }
        }
    }
}

