namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    [MessageHandlerClass]
    public class CHeroSkinBuyManager : Singleton<CHeroSkinBuyManager>
    {
        private uint m_buyHeroIDForFriend;
        private uint m_buyPriceForFriend;
        private uint m_buySkinIDForFriend;
        private ListView<COMDT_FRIEND_INFO> m_friendList;
        private bool m_isBuySkinForFriend;
        public static string s_buyHeroSkin3DFormPath = "UGUI/Form/System/HeroInfo/Form_Buy_HeroSkin_3D.prefab";
        public static string s_buyHeroSkinFormPath = "UGUI/Form/System/HeroInfo/Form_Buy_HeroSkin.prefab";
        public static string s_heroBuyFormPath = "UGUI/Form/System/Mall/Form_MallBuyHero.prefab";
        public static string s_heroBuyFriendPath = "UGUI/Form/System/HeroInfo/Form_BuyForFriend.prefab";

        public override void Init()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroInfo_OpenBuyHeroForm, new CUIEventManager.OnUIEventHandler(this.OnOpenBuyHeroForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSkin_OpenBuySkinForm, new CUIEventManager.OnUIEventHandler(this.OnOpenBuySkinForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSkin_Buy, new CUIEventManager.OnUIEventHandler(this.OnHeroSkin_Buy));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSkin_BuyConfirm, new CUIEventManager.OnUIEventHandler(this.OnHeroSkinBuyConfirm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroView_BuyHero, new CUIEventManager.OnUIEventHandler(this.OnHeroInfo_BuyHero));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroView_ConfirmBuyHero, new CUIEventManager.OnUIEventHandler(this.OnHeroInfo_ConfirmBuyHero));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroView_OpenBuyHeroForFriend, new CUIEventManager.OnUIEventHandler(this.OnHeroInfo_OpenBuyHeroForFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroView_BuyHeroForFriend, new CUIEventManager.OnUIEventHandler(this.OnHeroInfo_BuyHeroForFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroView_ConfirmBuyHeroForFriend, new CUIEventManager.OnUIEventHandler(this.OnHeroInfo_ConfirmBuyHeroForFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSkin_OpenBuyHeroSkinForFriend, new CUIEventManager.OnUIEventHandler(this.OnHeroInfo_OpenBuyHeroSkinForFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSkin_BuyHeroSkinForFriend, new CUIEventManager.OnUIEventHandler(this.OnHeroInfo_BuyHeroSkinForFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSkin_ConfirmBuyHeroSkinForFriend, new CUIEventManager.OnUIEventHandler(this.OnHeroInfo_ConfirmBuyHeroSkinForFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSkin_SearchFriend, new CUIEventManager.OnUIEventHandler(this.OnHeroInfo_SearchFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.HeroSkin_OnFriendListElementEnable, new CUIEventManager.OnUIEventHandler(this.OnFriendListElementEnable));
        }

        private void InitBuyForFriendForm(CUIFormScript form, bool bSkin, uint heroId, uint skinId = 0)
        {
            Transform transform9;
            CUIEventScript script;
            uint payValue = 0;
            if (!bSkin)
            {
                ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey(heroId);
                DebugHelper.Assert(dataByKey != null);
                if (dataByKey == null)
                {
                    goto Label_03D3;
                }
                form.transform.Find("Panel/Title/titleText").GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("Mall_Hero_Buy_Title");
                GameObject gameObject = form.transform.Find("Panel/skinBgImage/skinIconImage").gameObject;
                form.transform.Find("Panel/skinBgImage/skinNameText").GetComponent<Text>().text = StringHelper.UTF8BytesToString(ref dataByKey.szName);
                form.transform.Find("Panel/skinBgImage/skinIconImage").GetComponent<Image>().SetSprite(CUIUtility.s_Sprite_Dynamic_BustHero_Dir + StringHelper.UTF8BytesToString(ref dataByKey.szImagePath), form, false, true, true);
                form.transform.Find("Panel/Panel_Prop").gameObject.CustomSetActive(false);
                Transform transform6 = form.transform.Find("Panel/skinPricePanel");
                Transform costIcon = transform6.Find("costImage");
                SetPayCostIcon(form, costIcon, enPayType.DianQuan);
                SetPayCostTypeText(transform6.Find("costTypeText"), enPayType.DianQuan);
                transform9 = transform6.Find("costPanel");
                if (transform9 == null)
                {
                    goto Label_03D3;
                }
                ResHeroPromotion resPromotion = CHeroDataFactory.CreateHeroData(heroId).promotion();
                stPayInfoSet payInfoSetOfGood = CMallSystem.GetPayInfoSetOfGood(dataByKey, resPromotion);
                for (int i = 0; i < payInfoSetOfGood.m_payInfoCount; i++)
                {
                    if (((payInfoSetOfGood.m_payInfos[i].m_payType == enPayType.Diamond) || (payInfoSetOfGood.m_payInfos[i].m_payType == enPayType.DianQuan)) || (payInfoSetOfGood.m_payInfos[i].m_payType == enPayType.DiamondAndDianQuan))
                    {
                        payValue = payInfoSetOfGood.m_payInfos[i].m_payValue;
                        break;
                    }
                }
            }
            else
            {
                ResHeroSkin heroSkin = CSkinInfo.GetHeroSkin(heroId, skinId);
                DebugHelper.Assert(heroSkin != null);
                if (heroSkin != null)
                {
                    Image image = form.transform.Find("Panel/skinBgImage/skinIconImage").GetComponent<Image>();
                    string prefabPath = string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_BustHero_Dir, StringHelper.UTF8BytesToString(ref heroSkin.szSkinPicID));
                    image.SetSprite(prefabPath, form, true, false, false);
                    form.transform.Find("Panel/skinBgImage/skinNameText").GetComponent<Text>().text = StringHelper.UTF8BytesToString(ref heroSkin.szSkinName);
                    form.transform.Find("Panel/Panel_Prop").gameObject.CustomSetActive(true);
                    GameObject listObj = form.transform.Find("Panel/Panel_Prop/List_Prop").gameObject;
                    CSkinInfo.GetHeroSkinProp(heroId, skinId, ref CHeroInfoSystem2.s_propArr, ref CHeroInfoSystem2.s_propPctArr);
                    CUICommonSystem.SetListProp(listObj, ref CHeroInfoSystem2.s_propArr, ref CHeroInfoSystem2.s_propPctArr);
                    Transform transform = form.transform.Find("Panel/skinPricePanel");
                    Transform transform2 = transform.Find("costImage");
                    SetPayCostIcon(form, transform2, enPayType.DianQuan);
                    SetPayCostTypeText(transform.Find("costTypeText"), enPayType.DianQuan);
                    Transform transform4 = transform.Find("costPanel");
                    if (transform4 != null)
                    {
                        ResSkinPromotion skinPromotion = CSkinInfo.GetSkinPromotion(heroId, skinId);
                        stPayInfoSet set = CMallSystem.GetPayInfoSetOfGood(heroSkin, skinPromotion);
                        for (int j = 0; j < set.m_payInfoCount; j++)
                        {
                            if (((set.m_payInfos[j].m_payType == enPayType.Diamond) || (set.m_payInfos[j].m_payType == enPayType.DianQuan)) || (set.m_payInfos[j].m_payType == enPayType.DiamondAndDianQuan))
                            {
                                payValue = set.m_payInfos[j].m_payValue;
                                break;
                            }
                        }
                        SetPayCurrentPrice(transform4.Find("costText"), payValue);
                    }
                }
                goto Label_03D3;
            }
            SetPayCurrentPrice(transform9.Find("costText"), payValue);
        Label_03D3:
            script = form.transform.Find("Panel/SearchFriend/Button").GetComponent<CUIEventScript>();
            script.m_onClickEventParams.friendHeroSkinPar.bSkin = bSkin;
            script.m_onClickEventParams.friendHeroSkinPar.heroId = heroId;
            script.m_onClickEventParams.friendHeroSkinPar.skinId = skinId;
            script.m_onClickEventParams.friendHeroSkinPar.price = payValue;
            Text component = form.transform.Find("Panel/TipTxt").GetComponent<Text>();
            uint[] conditionParam = Singleton<CFunctionUnlockSys>.instance.GetConditionParam(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_PRESENTHERO, RES_UNLOCKCONDITION_TYPE.RES_UNLOCKCONDITIONTYPE_ABOVELEVEL);
            uint num4 = (conditionParam.Length <= 1) ? 1 : conditionParam[0];
            string[] args = new string[] { num4.ToString() };
            component.text = Singleton<CTextManager>.GetInstance().GetText("Buy_For_Friend_Tip", args);
            ListView<COMDT_FRIEND_INFO> list = Singleton<CFriendContoller>.GetInstance().model.GetList(CFriendModel.FriendType.GameFriend);
            CUIListScript script2 = form.transform.Find("Panel/List").GetComponent<CUIListScript>();
            this.UpdateFriendList(ref list, ref script2, bSkin, heroId, skinId, payValue);
        }

        [MessageHandler(0x71a)]
        public static void OnBuyHero(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            if (msg.stPkgData.stBuyHeroRsp.iResult == 0)
            {
                DebugHelper.Assert(GameDataMgr.heroDatabin.GetDataByKey(msg.stPkgData.stBuyHeroRsp.dwHeroID) != null);
                Singleton<CHeroInfoSystem2>.GetInstance().OnNtyAddHero(msg.stPkgData.stBuyHeroRsp.dwHeroID);
                CUICommonSystem.ShowNewHeroOrSkin(msg.stPkgData.stBuyHeroRsp.dwHeroID, 0, enUIEventID.None, true, COM_REWARDS_TYPE.COM_REWARDS_TYPE_HERO, false, null, enFormPriority.Priority1, 0, 0);
            }
            else
            {
                Singleton<CUIManager>.GetInstance().OpenTips(Utility.ProtErrCodeToStr(0x71a, msg.stPkgData.stBuyHeroRsp.iResult), false, 1f, null, new object[0]);
            }
        }

        [MessageHandler(0x727)]
        public static void OnBuyHeroForFriend(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            if (msg.stPkgData.stPresentHeroRsp.iResult == 0)
            {
                Singleton<CUIManager>.GetInstance().OpenTips("Buy_For_Friend_Success", true, 1f, null, new object[0]);
            }
            else
            {
                string strContent = Utility.ProtErrCodeToStr(0x729, msg.stPkgData.stPresentHeroRsp.iResult);
                Singleton<CUIManager>.GetInstance().OpenTips(strContent, false, 1f, null, new object[0]);
            }
        }

        [MessageHandler(0x71c)]
        public static void OnBuyHeroSkinRsp(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            SCPKG_BUYHEROSKIN_RSP stBuyHeroSkinRsp = msg.stPkgData.stBuyHeroSkinRsp;
            if (stBuyHeroSkinRsp.iResult == 0)
            {
                Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().OnAddHeroSkin(stBuyHeroSkinRsp.dwHeroID, stBuyHeroSkinRsp.dwSkinID);
                Singleton<CHeroInfoSystem2>.GetInstance().OnHeroSkinBuySuc(stBuyHeroSkinRsp.dwHeroID);
                CUICommonSystem.ShowNewHeroOrSkin(stBuyHeroSkinRsp.dwHeroID, stBuyHeroSkinRsp.dwSkinID, enUIEventID.None, true, COM_REWARDS_TYPE.COM_REWARDS_TYPE_SKIN, false, null, enFormPriority.Priority1, 0, 0);
            }
            else
            {
                CS_BUYHEROSKIN_ERRCODE iResult = (CS_BUYHEROSKIN_ERRCODE) stBuyHeroSkinRsp.iResult;
                CTextManager instance = Singleton<CTextManager>.GetInstance();
                switch (iResult)
                {
                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_OWNED:
                        Singleton<CUIManager>.GetInstance().OpenTips(instance.GetText("Hero_SkinBuy_Has_Own"), false, 1f, null, new object[0]);
                        return;

                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_FORBID_GOLD:
                        Singleton<CUIManager>.GetInstance().OpenTips(instance.GetText("Hero_SkinBuy_Forbid_Gold"), false, 1f, null, new object[0]);
                        return;

                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_FORBID_COUPONS:
                        Singleton<CUIManager>.GetInstance().OpenTips(instance.GetText("Hero_SkinBuy_Forbid_DianQuan"), false, 1f, null, new object[0]);
                        return;

                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_PAY_COUPONS:
                        Singleton<CUIManager>.GetInstance().OpenTips(instance.GetText("Hero_SkinBuy_Pay_DianQuan_Fail"), false, 1f, null, new object[0]);
                        return;

                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_NO_SKIN:
                        Singleton<CUIManager>.GetInstance().OpenTips(instance.GetText("Hero_SkinBuy_Data_Not_Find"), false, 1f, null, new object[0]);
                        return;

                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_NO_HERO:
                        Singleton<CUIManager>.GetInstance().OpenTips(instance.GetText("Hero_SkinBuy_Hero_Not_Exist"), false, 1f, null, new object[0]);
                        return;

                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_OTHER:
                        Singleton<CUIManager>.GetInstance().OpenTips(instance.GetText("Hero_SkinBuy_Other_Error"), false, 1f, null, new object[0]);
                        return;

                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_FORBID_SKINCOIN:
                        return;

                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_FORBID_DIAMOND:
                        Singleton<CUIManager>.GetInstance().OpenTips(instance.GetText("Hero_SkinBuy_Forbid_Diamond"), false, 1f, null, new object[0]);
                        return;

                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_FORBID_MIXPAY:
                        Singleton<CUIManager>.GetInstance().OpenTips(instance.GetText("Hero_SkinBuy_Forbid_MixPay"), false, 1f, null, new object[0]);
                        return;

                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_FORBID_LACKCOIN:
                        Singleton<CUIManager>.GetInstance().OpenTips(instance.GetText("Hero_SkinBuy_Forbid_LackCoin"), false, 1f, null, new object[0]);
                        return;

                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_TIME_ERROR:
                        Singleton<CUIManager>.GetInstance().OpenTips(instance.GetText("Hero_SkinBuy_Time_Error"), false, 1f, null, new object[0]);
                        return;

                    case CS_BUYHEROSKIN_ERRCODE.CS_BUYHEROSKIN_FORBID_SHOW:
                        Singleton<CUIManager>.GetInstance().OpenTips(instance.GetText("Hero_SkinBuy_Forbid_Show"), false, 1f, null, new object[0]);
                        return;
                }
                Singleton<CUIManager>.GetInstance().OpenTips(iResult.ToString(), false, 1f, null, new object[0]);
            }
        }

        [MessageHandler(0x729)]
        public static void OnBuySkinForFriend(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            if (msg.stPkgData.stPresentSkinRsp.iResult == 0)
            {
                Singleton<CUIManager>.GetInstance().OpenTips("Buy_For_Friend_Success", true, 1f, null, new object[0]);
            }
            else
            {
                string strContent = Utility.ProtErrCodeToStr(0x729, msg.stPkgData.stPresentSkinRsp.iResult);
                Singleton<CUIManager>.GetInstance().OpenTips(strContent, false, 1f, null, new object[0]);
            }
        }

        private void OnFriendListElementEnable(CUIEvent uiEvent)
        {
            int srcWidgetIndexInBelongedList = uiEvent.m_srcWidgetIndexInBelongedList;
            RefreshFriendListElementForGift(uiEvent.m_srcWidget, this.m_friendList[srcWidgetIndexInBelongedList], this.m_isBuySkinForFriend);
            CUIEventScript component = uiEvent.m_srcWidget.transform.FindChild("InviteButton").GetComponent<CUIEventScript>();
            component.m_onClickEventParams.commonUInt64Param1 = this.m_friendList[srcWidgetIndexInBelongedList].stUin.ullUid;
            component.m_onClickEventParams.tagUInt = this.m_friendList[srcWidgetIndexInBelongedList].stUin.dwLogicWorldId;
            if (this.m_isBuySkinForFriend)
            {
                component.m_onClickEventID = enUIEventID.HeroSkin_BuyHeroSkinForFriend;
                component.m_onClickEventParams.heroSkinParam.heroId = this.m_buyHeroIDForFriend;
                component.m_onClickEventParams.heroSkinParam.skinId = this.m_buySkinIDForFriend;
                component.m_onClickEventParams.commonUInt32Param1 = this.m_buyPriceForFriend;
            }
            else
            {
                component.m_onClickEventID = enUIEventID.HeroView_BuyHeroForFriend;
                component.m_onClickEventParams.commonUInt32Param1 = this.m_buyPriceForFriend;
                component.m_onClickEventParams.tag = (int) this.m_buyHeroIDForFriend;
            }
        }

        public void OnHeroInfo_BuyHero(CUIEvent uiEvent)
        {
            enPayType tag = (enPayType) uiEvent.m_eventParams.tag;
            uint payValue = uiEvent.m_eventParams.commonUInt32Param1;
            uint heroId = uiEvent.m_eventParams.heroId;
            ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey(heroId);
            DebugHelper.Assert(dataByKey != null);
            CUIEvent uIEvent = Singleton<CUIEventManager>.GetInstance().GetUIEvent();
            uIEvent.m_eventID = enUIEventID.HeroView_ConfirmBuyHero;
            uIEvent.m_eventParams.heroId = heroId;
            switch (tag)
            {
                case enPayType.GoldCoin:
                    uIEvent.m_eventParams.tag = 1;
                    break;

                case enPayType.DianQuan:
                    uIEvent.m_eventParams.tag = 0;
                    break;

                case enPayType.Diamond:
                    uIEvent.m_eventParams.tag = 2;
                    break;

                case enPayType.DiamondAndDianQuan:
                    uIEvent.m_eventParams.tag = 3;
                    break;
            }
            CMallSystem.TryToPay(enPayPurpose.Buy, StringHelper.UTF8BytesToString(ref dataByKey.szName), tag, payValue, uIEvent.m_eventID, ref uIEvent.m_eventParams, enUIEventID.None, false, true);
        }

        private void OnHeroInfo_BuyHeroForFriend(CUIEvent uiEvent)
        {
            uiEvent.m_eventID = enUIEventID.HeroView_ConfirmBuyHeroForFriend;
            uint payValue = uiEvent.m_eventParams.commonUInt32Param1;
            int tag = uiEvent.m_eventParams.tag;
            ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey(tag);
            DebugHelper.Assert(dataByKey != null);
            if (dataByKey != null)
            {
                string[] args = new string[] { StringHelper.UTF8BytesToString(ref dataByKey.szName) };
                string text = Singleton<CTextManager>.GetInstance().GetText("Buy_For_Friend", args);
                CMallSystem.TryToPay(enPayPurpose.Buy, text, enPayType.DianQuan, payValue, uiEvent.m_eventID, ref uiEvent.m_eventParams, enUIEventID.None, true, true);
            }
        }

        private void OnHeroInfo_BuyHeroSkinForFriend(CUIEvent uiEvent)
        {
            uiEvent.m_eventID = enUIEventID.HeroSkin_ConfirmBuyHeroSkinForFriend;
            uint payValue = uiEvent.m_eventParams.commonUInt32Param1;
            uint heroId = uiEvent.m_eventParams.heroSkinParam.heroId;
            uint skinId = uiEvent.m_eventParams.heroSkinParam.skinId;
            ResHeroSkin heroSkin = CSkinInfo.GetHeroSkin(heroId, skinId);
            DebugHelper.Assert(heroSkin != null);
            if (heroSkin != null)
            {
                string[] args = new string[] { StringHelper.UTF8BytesToString(ref heroSkin.szSkinName) };
                string text = Singleton<CTextManager>.GetInstance().GetText("Buy_For_Friend", args);
                CMallSystem.TryToPay(enPayPurpose.Buy, text, enPayType.DianQuan, payValue, uiEvent.m_eventID, ref uiEvent.m_eventParams, enUIEventID.None, true, true);
            }
        }

        public void OnHeroInfo_ConfirmBuyHero(CUIEvent uiEvent)
        {
            int tag = uiEvent.m_eventParams.tag;
            ReqBuyHero(uiEvent.m_eventParams.heroId, tag);
        }

        private void OnHeroInfo_ConfirmBuyHeroForFriend(CUIEvent uiEvent)
        {
            int tag = uiEvent.m_eventParams.tag;
            COMDT_ACNT_UNIQ friendUin = new COMDT_ACNT_UNIQ {
                ullUid = uiEvent.m_eventParams.commonUInt64Param1,
                dwLogicWorldId = uiEvent.m_eventParams.tagUInt
            };
            ReqBuyHeroForFriend((uint) tag, ref friendUin);
        }

        private void OnHeroInfo_ConfirmBuyHeroSkinForFriend(CUIEvent uiEvent)
        {
            uint heroId = uiEvent.m_eventParams.heroSkinParam.heroId;
            uint skinId = uiEvent.m_eventParams.heroSkinParam.skinId;
            COMDT_ACNT_UNIQ friendUin = new COMDT_ACNT_UNIQ {
                ullUid = uiEvent.m_eventParams.commonUInt64Param1,
                dwLogicWorldId = uiEvent.m_eventParams.tagUInt
            };
            ReqBuySkinForFriend(heroId, skinId, ref friendUin);
        }

        private void OnHeroInfo_OpenBuyHeroForFriend(CUIEvent uiEvent)
        {
            uint heroId = uiEvent.m_eventParams.heroId;
            CUIFormScript form = Singleton<CUIManager>.GetInstance().OpenForm(s_heroBuyFriendPath, false, true);
            if (form != null)
            {
                this.InitBuyForFriendForm(form, false, heroId, 0);
            }
        }

        private void OnHeroInfo_OpenBuyHeroSkinForFriend(CUIEvent uiEvent)
        {
            uint heroId = uiEvent.m_eventParams.heroSkinParam.heroId;
            uint skinId = uiEvent.m_eventParams.heroSkinParam.skinId;
            CUIFormScript form = Singleton<CUIManager>.GetInstance().OpenForm(s_heroBuyFriendPath, false, true);
            if (form != null)
            {
                this.InitBuyForFriendForm(form, true, heroId, skinId);
            }
        }

        private void OnHeroInfo_SearchFriend(CUIEvent uiEvent)
        {
            CUIFormScript srcFormScript = uiEvent.m_srcFormScript;
            CUIListScript component = srcFormScript.transform.Find("Panel/List").GetComponent<CUIListScript>();
            InputField field = srcFormScript.transform.Find("Panel/SearchFriend/InputField").GetComponent<InputField>();
            if (field != null)
            {
                ListView<COMDT_FRIEND_INFO> list = Singleton<CFriendContoller>.GetInstance().model.GetList(CFriendModel.FriendType.GameFriend);
                if (field.text != string.Empty)
                {
                    ListView<COMDT_FRIEND_INFO> view2 = list;
                    list = new ListView<COMDT_FRIEND_INFO>();
                    for (int i = 0; i < view2.Count; i++)
                    {
                        COMDT_FRIEND_INFO item = view2[i];
                        if (StringHelper.UTF8BytesToString(ref item.szUserName).Contains(field.text))
                        {
                            list.Add(item);
                        }
                    }
                }
                bool bSkin = uiEvent.m_eventParams.friendHeroSkinPar.bSkin;
                uint heroId = uiEvent.m_eventParams.friendHeroSkinPar.heroId;
                uint skinId = uiEvent.m_eventParams.friendHeroSkinPar.skinId;
                uint price = uiEvent.m_eventParams.friendHeroSkinPar.price;
                this.UpdateFriendList(ref list, ref component, bSkin, heroId, skinId, price);
            }
        }

        public void OnHeroSkin_Buy(CUIEvent uiEvent)
        {
            enPayType tag = (enPayType) uiEvent.m_eventParams.tag;
            uint payValue = uiEvent.m_eventParams.commonUInt32Param1;
            uint heroId = uiEvent.m_eventParams.heroSkinParam.heroId;
            uint skinId = uiEvent.m_eventParams.heroSkinParam.skinId;
            bool isCanCharge = uiEvent.m_eventParams.heroSkinParam.isCanCharge;
            string goodName = StringHelper.UTF8BytesToString(ref CSkinInfo.GetHeroSkin(heroId, skinId).szSkinName);
            CUIEvent uIEvent = Singleton<CUIEventManager>.GetInstance().GetUIEvent();
            uIEvent.m_eventID = enUIEventID.HeroSkin_BuyConfirm;
            uIEvent.m_eventParams.heroSkinParam.heroId = heroId;
            uIEvent.m_eventParams.heroSkinParam.skinId = skinId;
            uIEvent.m_eventParams.tag = (int) tag;
            uIEvent.m_eventParams.commonUInt32Param1 = payValue;
            CMallSystem.TryToPay(enPayPurpose.Buy, goodName, tag, payValue, uIEvent.m_eventID, ref uIEvent.m_eventParams, enUIEventID.None, false, isCanCharge);
        }

        public void OnHeroSkinBuyConfirm(CUIEvent uiEvent)
        {
            enPayType tag = (enPayType) uiEvent.m_eventParams.tag;
            uint heroId = uiEvent.m_eventParams.heroSkinParam.heroId;
            uint skinId = uiEvent.m_eventParams.heroSkinParam.skinId;
            BUY_HEROSKIN_TYPE buyType = BUY_HEROSKIN_TYPE.BUY_HEROSKIN_TYPE_DIAMOND;
            switch (tag)
            {
                case enPayType.DianQuan:
                    buyType = BUY_HEROSKIN_TYPE.BUY_HEROSKIN_TYPE_COUPONS;
                    break;

                case enPayType.Diamond:
                    buyType = BUY_HEROSKIN_TYPE.BUY_HEROSKIN_TYPE_DIAMOND;
                    break;

                case enPayType.DiamondAndDianQuan:
                    buyType = BUY_HEROSKIN_TYPE.BUY_HEROSKIN_TYPE_MIXPAY;
                    break;

                default:
                    buyType = BUY_HEROSKIN_TYPE.BUY_HEROSKIN_TYPE_DIAMOND;
                    break;
            }
            ReqBuyHeroSkin(heroId, skinId, buyType, false);
        }

        private void OnOpenBuyHeroForm(CUIEvent uiEvent)
        {
            OpenBuyHeroForm(uiEvent.m_srcFormScript, uiEvent.m_eventParams.heroId, new stPayInfoSet(), enUIEventID.None);
        }

        private void OnOpenBuySkinForm(CUIEvent uiEvent)
        {
            OpenBuyHeroSkinForm(uiEvent.m_eventParams.heroSkinParam.heroId, uiEvent.m_eventParams.heroSkinParam.skinId, uiEvent.m_eventParams.heroSkinParam.isCanCharge, new stPayInfoSet(), enUIEventID.None);
        }

        public static void OpenBuyHeroForm(CUIFormScript srcform, uint heroId, stPayInfoSet payInfoSet, enUIEventID btnClickEventID = 0)
        {
            CUIFormScript formScript = Singleton<CUIManager>.GetInstance().OpenForm(s_heroBuyFormPath, false, true);
            if (formScript != null)
            {
                ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey(heroId);
                if (dataByKey != null)
                {
                    formScript.transform.Find("heroInfoPanel/title/Text").GetComponent<Text>().text = Singleton<CTextManager>.GetInstance().GetText("Mall_Hero_Buy_Title");
                    GameObject gameObject = formScript.transform.Find("heroInfoPanel/heroItem").gameObject;
                    Text component = gameObject.transform.Find("heroNameText").GetComponent<Text>();
                    CUICommonSystem.SetHeroItemImage(formScript, gameObject, StringHelper.UTF8BytesToString(ref dataByKey.szImagePath), enHeroHeadType.enBust, false);
                    component.text = StringHelper.UTF8BytesToString(ref dataByKey.szName);
                    GameObject pricePanel = formScript.transform.Find("heroInfoPanel/heroPricePanel").gameObject;
                    if (payInfoSet.m_payInfoCount > 0)
                    {
                        SetHeroBuyPricePanel(formScript, pricePanel, ref payInfoSet, heroId, btnClickEventID);
                    }
                    else
                    {
                        ResHeroPromotion resPromotion = CHeroDataFactory.CreateHeroData(heroId).promotion();
                        stPayInfoSet payInfoSetOfGood = new stPayInfoSet();
                        payInfoSetOfGood = CMallSystem.GetPayInfoSetOfGood(dataByKey, resPromotion);
                        SetHeroBuyPricePanel(formScript, pricePanel, ref payInfoSetOfGood, heroId, btnClickEventID);
                    }
                    Transform transform = formScript.transform.Find("heroInfoPanel/heroPricePanel/pnlDiamondBuy/buyForFriendBtn");
                    if (transform != null)
                    {
                        if (ShouldShowBuyForFriend(false, heroId, 0))
                        {
                            transform.gameObject.CustomSetActive(true);
                            CUIEventScript script2 = transform.GetComponent<CUIEventScript>();
                            if (script2 != null)
                            {
                                script2.m_onClickEventParams.heroId = heroId;
                            }
                        }
                        else
                        {
                            transform.gameObject.CustomSetActive(false);
                        }
                    }
                }
            }
        }

        public static void OpenBuyHeroSkinForm(uint heroId, uint skinId, bool isCanCharge, stPayInfoSet payInfoSet, enUIEventID btnClickEventID = 0)
        {
            CUIFormScript formScript = Singleton<CUIManager>.GetInstance().OpenForm(s_buyHeroSkinFormPath, false, true);
            if (formScript != null)
            {
                ResHeroSkin heroSkin = CSkinInfo.GetHeroSkin(heroId, skinId);
                if (heroSkin != null)
                {
                    Transform transform = formScript.gameObject.transform.Find("Panel");
                    Image component = transform.Find("skinBgImage/skinIconImage").GetComponent<Image>();
                    string prefabPath = string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_BustHero_Dir, StringHelper.UTF8BytesToString(ref heroSkin.szSkinPicID));
                    component.SetSprite(prefabPath, formScript, true, false, false);
                    transform.Find("skinNameText").GetComponent<Text>().text = StringHelper.UTF8BytesToString(ref heroSkin.szSkinName);
                    GameObject gameObject = transform.Find("Panel_Prop/List_Prop").gameObject;
                    CSkinInfo.GetHeroSkinProp(heroId, skinId, ref CHeroInfoSystem2.s_propArr, ref CHeroInfoSystem2.s_propPctArr);
                    CUICommonSystem.SetListProp(gameObject, ref CHeroInfoSystem2.s_propArr, ref CHeroInfoSystem2.s_propPctArr);
                    CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                    if (payInfoSet.m_payInfoCount == 0)
                    {
                        ResSkinPromotion resPromotion = new ResSkinPromotion();
                        resPromotion = CSkinInfo.GetSkinPromotion(heroId, skinId);
                        payInfoSet = CMallSystem.GetPayInfoSetOfGood(heroSkin, resPromotion);
                    }
                    Transform skinPricePanel = transform.Find("skinPricePanel");
                    if (payInfoSet.m_payInfoCount > 0)
                    {
                        SetSkinPricePanel(formScript, skinPricePanel, ref payInfoSet.m_payInfos[0]);
                        Transform transform3 = skinPricePanel.Find("buyButton");
                        if (masterRoleInfo != null)
                        {
                            if (!masterRoleInfo.IsHaveHero(heroId, false))
                            {
                                if (transform3 != null)
                                {
                                    Transform transform4 = transform3.Find("Text");
                                    if (transform4 != null)
                                    {
                                        Text text2 = transform4.GetComponent<Text>();
                                        if (text2 != null)
                                        {
                                            text2.text = Singleton<CTextManager>.GetInstance().GetText("Mall_Skin_Text_1");
                                        }
                                    }
                                    CUIEventScript script2 = transform3.gameObject.GetComponent<CUIEventScript>();
                                    if (script2 != null)
                                    {
                                        stUIEventParams eventParams = new stUIEventParams();
                                        eventParams.openHeroFormPar.heroId = heroId;
                                        eventParams.openHeroFormPar.skinId = skinId;
                                        eventParams.openHeroFormPar.openSrc = enHeroFormOpenSrc.SkinBuyClick;
                                        script2.SetUIEvent(enUIEventType.Click, enUIEventID.HeroInfo_OpenForm, eventParams);
                                    }
                                }
                            }
                            else if (transform3 != null)
                            {
                                CUIEventScript script3 = transform3.gameObject.GetComponent<CUIEventScript>();
                                if (script3 != null)
                                {
                                    CUIEvent uIEvent = Singleton<CUIEventManager>.GetInstance().GetUIEvent();
                                    if (btnClickEventID == enUIEventID.None)
                                    {
                                        uIEvent.m_eventID = enUIEventID.HeroSkin_Buy;
                                    }
                                    else
                                    {
                                        uIEvent.m_eventID = btnClickEventID;
                                    }
                                    uIEvent.m_eventParams.tag = (int) payInfoSet.m_payInfos[0].m_payType;
                                    uIEvent.m_eventParams.commonUInt32Param1 = payInfoSet.m_payInfos[0].m_payValue;
                                    uIEvent.m_eventParams.heroSkinParam.heroId = heroId;
                                    uIEvent.m_eventParams.heroSkinParam.skinId = skinId;
                                    uIEvent.m_eventParams.heroSkinParam.isCanCharge = isCanCharge;
                                    script3.SetUIEvent(enUIEventType.Click, uIEvent.m_eventID, uIEvent.m_eventParams);
                                }
                            }
                        }
                    }
                    Transform transform5 = formScript.transform.Find("Panel/skinPricePanel/buyForFriendButton");
                    if (transform5 != null)
                    {
                        if (ShouldShowBuyForFriend(true, heroId, skinId))
                        {
                            transform5.gameObject.CustomSetActive(true);
                            CUIEventScript script4 = transform5.GetComponent<CUIEventScript>();
                            if (script4 != null)
                            {
                                script4.m_onClickEventParams.heroSkinParam.heroId = heroId;
                                script4.m_onClickEventParams.heroSkinParam.skinId = skinId;
                            }
                        }
                        else
                        {
                            transform5.gameObject.CustomSetActive(false);
                        }
                    }
                }
            }
        }

        public static void OpenBuyHeroSkinForm3D(uint heroId, uint skinId, bool isCanCharge)
        {
            CUIFormScript formScript = Singleton<CUIManager>.GetInstance().OpenForm(s_buyHeroSkin3DFormPath, false, true);
            if (formScript != null)
            {
                ResHeroSkin heroSkin = CSkinInfo.GetHeroSkin(heroId, skinId);
                if (heroSkin != null)
                {
                    Transform transform = formScript.gameObject.transform.Find("Panel");
                    transform.Find("skinNameText").GetComponent<Text>().text = StringHelper.UTF8BytesToString(ref heroSkin.szSkinName);
                    GameObject gameObject = transform.Find("Panel_Prop/List_Prop").gameObject;
                    CSkinInfo.GetHeroSkinProp(heroId, skinId, ref CHeroInfoSystem2.s_propArr, ref CHeroInfoSystem2.s_propPctArr);
                    CUICommonSystem.SetListProp(gameObject, ref CHeroInfoSystem2.s_propArr, ref CHeroInfoSystem2.s_propPctArr);
                    ResSkinPromotion skinPromotion = new ResSkinPromotion();
                    stPayInfoSet payInfoSetOfGood = new stPayInfoSet();
                    skinPromotion = CSkinInfo.GetSkinPromotion(heroId, skinId);
                    if (skinPromotion != null)
                    {
                        payInfoSetOfGood = CMallSystem.GetPayInfoSetOfGood(false, 0, skinPromotion.bIsBuyCoupons > 0, skinPromotion.dwBuyCoupons, skinPromotion.bIsBuyDiamond > 0, skinPromotion.dwBuyDiamond, 0x2710);
                    }
                    else
                    {
                        payInfoSetOfGood = CMallSystem.GetPayInfoSetOfGood(heroSkin);
                    }
                    Transform transform2 = transform.Find("groupPanel/goldPanel");
                    Transform transform3 = transform.Find("groupPanel/diamondPanel");
                    if (transform2 != null)
                    {
                        transform2.gameObject.CustomSetActive(false);
                    }
                    if (transform3 != null)
                    {
                        transform3.gameObject.CustomSetActive(false);
                    }
                    for (int i = 0; i < payInfoSetOfGood.m_payInfoCount; i++)
                    {
                        if ((payInfoSetOfGood.m_payInfos[i].m_payType != enPayType.GoldCoin) && (transform3 != null))
                        {
                            transform3.gameObject.CustomSetActive(true);
                            Transform transform4 = transform3.Find("image");
                            if (transform4 != null)
                            {
                                Image image = transform4.gameObject.GetComponent<Image>();
                                if (image != null)
                                {
                                    image.SetSprite(CMallSystem.GetPayTypeIconPath(payInfoSetOfGood.m_payInfos[i].m_payType), formScript, true, false, false);
                                }
                            }
                            Transform transform5 = transform3.Find("priceText");
                            if (transform5 != null)
                            {
                                Text text2 = transform5.gameObject.GetComponent<Text>();
                                if (text2 != null)
                                {
                                    text2.text = payInfoSetOfGood.m_payInfos[i].m_payValue.ToString();
                                }
                            }
                            Transform transform6 = transform3.Find("buyButton");
                            if (transform6 != null)
                            {
                                CUIEventScript script2 = transform6.gameObject.GetComponent<CUIEventScript>();
                                if (script2 != null)
                                {
                                    CUIEvent uIEvent = Singleton<CUIEventManager>.GetInstance().GetUIEvent();
                                    uIEvent.m_eventID = enUIEventID.HeroSkin_Buy;
                                    uIEvent.m_eventParams.tag = (int) payInfoSetOfGood.m_payInfos[i].m_payType;
                                    uIEvent.m_eventParams.commonUInt32Param1 = payInfoSetOfGood.m_payInfos[i].m_payValue;
                                    uIEvent.m_eventParams.heroSkinParam.heroId = heroId;
                                    uIEvent.m_eventParams.heroSkinParam.skinId = skinId;
                                    uIEvent.m_eventParams.heroSkinParam.isCanCharge = isCanCharge;
                                    script2.SetUIEvent(enUIEventType.Click, uIEvent.m_eventID, uIEvent.m_eventParams);
                                }
                            }
                        }
                    }
                    CUI3DImageScript component = transform.Find("3DImage").gameObject.GetComponent<CUI3DImageScript>();
                    ObjNameData data = CUICommonSystem.GetHeroPrefabPath(heroId, (int) skinId, true);
                    GameObject model = component.AddGameObject(data.ObjectName, false, false);
                    if (model != null)
                    {
                        if (data.ActorInfo != null)
                        {
                            model.transform.localScale = new Vector3(data.ActorInfo.LobbyScale, data.ActorInfo.LobbyScale, data.ActorInfo.LobbyScale);
                        }
                        DynamicShadow.EnableDynamicShow(component.gameObject, true);
                        CHeroAnimaSystem instance = Singleton<CHeroAnimaSystem>.GetInstance();
                        instance.Set3DModel(model);
                        instance.InitAnimatList();
                        instance.InitAnimatSoundList(heroId, skinId);
                        instance.OnModePlayAnima("Come");
                    }
                }
            }
        }

        public static void RefreshFriendListElementForGift(GameObject element, COMDT_FRIEND_INFO friend, bool bSkin)
        {
            CInviteView.UpdateFriendListElementBase(element, ref friend);
            Transform transform = element.transform.Find("Gender");
            if (transform != null)
            {
                COM_SNSGENDER bGender = (COM_SNSGENDER) friend.bGender;
                transform.gameObject.CustomSetActive(bGender != COM_SNSGENDER.COM_SNSGENDER_NONE);
                switch (bGender)
                {
                    case COM_SNSGENDER.COM_SNSGENDER_MALE:
                        CUIUtility.SetImageSprite(transform.GetComponent<Image>(), string.Format("{0}icon/Ico_boy", "UGUI/Sprite/Dynamic/"), null, true, false, false);
                        break;

                    case COM_SNSGENDER.COM_SNSGENDER_FEMALE:
                        CUIUtility.SetImageSprite(transform.GetComponent<Image>(), string.Format("{0}icon/Ico_girl", "UGUI/Sprite/Dynamic/"), null, true, false, false);
                        break;
                }
            }
        }

        public static void ReqBuyHero(uint HeroId, int BuyType)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x719);
            msg.stPkgData.stBuyHeroReq.dwHeroID = HeroId;
            msg.stPkgData.stBuyHeroReq.bBuyType = (byte) BuyType;
            IHeroData data = CHeroDataFactory.CreateHeroData(HeroId);
            if (data != null)
            {
                if (data.promotion() != null)
                {
                    msg.stPkgData.stBuyHeroReq.bIsPromotion = Convert.ToByte(true);
                }
                else
                {
                    msg.stPkgData.stBuyHeroReq.bIsPromotion = 0;
                }
                Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
            }
        }

        public static void ReqBuyHeroForFriend(uint heroId, ref COMDT_ACNT_UNIQ friendUin)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x726);
            msg.stPkgData.stPresentHeroReq.stFriendUin = friendUin;
            msg.stPkgData.stPresentHeroReq.dwHeroID = heroId;
            IHeroData data = CHeroDataFactory.CreateHeroData(heroId);
            if (data != null)
            {
                if (data.promotion() != null)
                {
                    msg.stPkgData.stPresentHeroReq.bIsPromotion = Convert.ToByte(true);
                }
                else
                {
                    msg.stPkgData.stPresentHeroReq.bIsPromotion = 0;
                }
            }
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void ReqBuyHeroSkin(uint heroId, uint skinId, BUY_HEROSKIN_TYPE buyType, bool isSendGameSvr = false)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x71b);
            msg.stPkgData.stBuyHeroSkinReq.dwHeroID = heroId;
            msg.stPkgData.stBuyHeroSkinReq.dwSkinID = skinId;
            msg.stPkgData.stBuyHeroSkinReq.bBuyType = (byte) buyType;
            ResSkinPromotion promotion = new ResSkinPromotion();
            stPayInfoSet set = new stPayInfoSet();
            if (CSkinInfo.GetSkinPromotion(heroId, skinId) != null)
            {
                msg.stPkgData.stBuyHeroSkinReq.bIsPromotion = Convert.ToByte(true);
            }
            else
            {
                msg.stPkgData.stBuyHeroSkinReq.bIsPromotion = 0;
            }
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void ReqBuySkinForFriend(uint heroId, uint skinId, ref COMDT_ACNT_UNIQ friendUin)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x728);
            msg.stPkgData.stPresentSkinReq.stFriendUin = friendUin;
            msg.stPkgData.stPresentSkinReq.dwSkinID = CSkinInfo.GetSkinCfgId(heroId, skinId);
            if (CSkinInfo.GetSkinPromotion(heroId, skinId) != null)
            {
                msg.stPkgData.stPresentSkinReq.bIsPromotion = Convert.ToByte(true);
            }
            else
            {
                msg.stPkgData.stPresentSkinReq.bIsPromotion = 0;
            }
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public static void SetHeroBuyPricePanel(CUIFormScript formScript, GameObject pricePanel, ref stPayInfoSet payInfoSet, uint heroID, enUIEventID btnClickEventID = 0)
        {
            if ((null != formScript) && (pricePanel != null))
            {
                Transform payInfoPanel = pricePanel.transform.Find("pnlCoinBuy");
                Transform transform2 = pricePanel.transform.Find("pnlDiamondBuy");
                Transform transform3 = pricePanel.transform.Find("Text");
                if ((payInfoPanel != null) && (transform2 != null))
                {
                    payInfoPanel.gameObject.CustomSetActive(false);
                    transform2.gameObject.CustomSetActive(false);
                    if (transform3 != null)
                    {
                        transform3.gameObject.CustomSetActive(payInfoSet.m_payInfoCount > 1);
                    }
                    for (int i = 0; i < payInfoSet.m_payInfoCount; i++)
                    {
                        if (payInfoSet.m_payInfos[i].m_payType == enPayType.GoldCoin)
                        {
                            payInfoPanel.gameObject.CustomSetActive(true);
                            SetPayInfoPanel(formScript, payInfoPanel, ref payInfoSet.m_payInfos[i], heroID, btnClickEventID);
                        }
                        else
                        {
                            transform2.gameObject.CustomSetActive(true);
                            SetPayInfoPanel(formScript, transform2, ref payInfoSet.m_payInfos[i], heroID, btnClickEventID);
                        }
                    }
                }
            }
        }

        public static void SetPayCostIcon(CUIFormScript formScript, Transform costIcon, enPayType payType)
        {
            if ((null != formScript) && (null != costIcon))
            {
                Image component = costIcon.GetComponent<Image>();
                if (component != null)
                {
                    component.SetSprite(CMallSystem.GetPayTypeIconPath(payType), formScript, true, false, false);
                }
            }
        }

        public static void SetPayCostTypeText(Transform costTypeText, enPayType payType)
        {
            if (costTypeText != null)
            {
                Text component = costTypeText.GetComponent<Text>();
                if (component != null)
                {
                    component.text = CMallSystem.GetPayTypeText(payType);
                }
            }
        }

        public static void SetPayCurrentPrice(Transform currentPrice, uint payValue)
        {
            if (currentPrice != null)
            {
                Text component = currentPrice.GetComponent<Text>();
                if (component != null)
                {
                    component.text = payValue.ToString();
                }
            }
        }

        private static void SetPayInfoPanel(CUIFormScript formScript, Transform payInfoPanel, ref stPayInfo payInfo, uint heroID, enUIEventID btnClickEventID)
        {
            Transform costIcon = payInfoPanel.Find("costImage");
            SetPayCostIcon(formScript, costIcon, payInfo.m_payType);
            SetPayCostTypeText(payInfoPanel.Find("costTypeText"), payInfo.m_payType);
            Transform transform3 = payInfoPanel.Find("costPanel");
            if (transform3 != null)
            {
                Transform transform4 = transform3.Find("oldPricePanel");
                if (transform4 != null)
                {
                    transform4.gameObject.CustomSetActive(payInfo.m_oriValue != payInfo.m_payValue);
                    SetPayOldPrice(transform4.Find("oldPriceText"), payInfo.m_oriValue);
                }
                SetPayCurrentPrice(transform3.Find("costText"), payInfo.m_payValue);
            }
            Text text = null;
            Transform transform7 = payInfoPanel.Find("buyBtn");
            if (transform7 != null)
            {
                Transform transform8 = transform7.Find("Text");
                if (transform8 != null)
                {
                    text = transform8.gameObject.GetComponent<Text>();
                    if (text != null)
                    {
                        text.text = CMallSystem.GetPriceTypeBuyString(payInfo.m_payType);
                    }
                }
                CUIEventScript component = transform7.GetComponent<CUIEventScript>();
                stUIEventParams eventParams = new stUIEventParams {
                    tag = (int) payInfo.m_payType,
                    commonUInt32Param1 = payInfo.m_payValue,
                    heroId = heroID
                };
                if (btnClickEventID != enUIEventID.None)
                {
                    component.SetUIEvent(enUIEventType.Click, btnClickEventID, eventParams);
                }
                else
                {
                    component.SetUIEvent(enUIEventType.Click, enUIEventID.HeroView_BuyHero, eventParams);
                }
            }
        }

        public static void SetPayOldPrice(Transform oldPrice, uint oriValue)
        {
            if (oldPrice != null)
            {
                Text component = oldPrice.GetComponent<Text>();
                if (component != null)
                {
                    component.text = oriValue.ToString();
                }
            }
        }

        private static void SetSkinPricePanel(CUIFormScript formScript, Transform skinPricePanel, ref stPayInfo payInfo)
        {
            if ((null != formScript) && (null != skinPricePanel))
            {
                Transform costIcon = skinPricePanel.Find("costImage");
                SetPayCostIcon(formScript, costIcon, payInfo.m_payType);
                SetPayCostTypeText(skinPricePanel.Find("costTypeText"), payInfo.m_payType);
                Transform transform3 = skinPricePanel.Find("costPanel");
                if (transform3 != null)
                {
                    Transform transform4 = transform3.Find("oldPricePanel");
                    if (transform4 != null)
                    {
                        transform4.gameObject.CustomSetActive(payInfo.m_oriValue != payInfo.m_payValue);
                        SetPayOldPrice(transform4.Find("oldPriceText"), payInfo.m_oriValue);
                    }
                    SetPayCurrentPrice(transform3.Find("costText"), payInfo.m_payValue);
                }
            }
        }

        public static bool ShouldShowBuyForFriend(bool bSkin, uint heroId, uint skinId = 0)
        {
            if (!bSkin)
            {
                ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey(heroId);
                DebugHelper.Assert(dataByKey != null);
                if (dataByKey != null)
                {
                    stPayInfoSet payInfoSetOfGood = CMallSystem.GetPayInfoSetOfGood(dataByKey);
                    bool flag2 = false;
                    for (int i = 0; i < payInfoSetOfGood.m_payInfoCount; i++)
                    {
                        if (((payInfoSetOfGood.m_payInfos[i].m_payType == enPayType.Diamond) || (payInfoSetOfGood.m_payInfos[i].m_payType == enPayType.DianQuan)) || (payInfoSetOfGood.m_payInfos[i].m_payType == enPayType.DiamondAndDianQuan))
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    return ((Singleton<CFunctionUnlockSys>.GetInstance().FucIsUnlock(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_PRESENTHERO) && flag2) && (dataByKey.bIsPresent > 0));
                }
            }
            else
            {
                ResHeroSkin heroSkin = CSkinInfo.GetHeroSkin(heroId, skinId);
                DebugHelper.Assert(heroSkin != null);
                if (heroSkin != null)
                {
                    stPayInfoSet set = CMallSystem.GetPayInfoSetOfGood(heroSkin);
                    bool flag = false;
                    for (int j = 0; j < set.m_payInfoCount; j++)
                    {
                        if (((set.m_payInfos[j].m_payType == enPayType.Diamond) || (set.m_payInfos[j].m_payType == enPayType.DianQuan)) || (set.m_payInfos[j].m_payType == enPayType.DiamondAndDianQuan))
                        {
                            flag = true;
                            break;
                        }
                    }
                    return ((Singleton<CFunctionUnlockSys>.GetInstance().FucIsUnlock(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_PRESENTHERO) && flag) && (heroSkin.bIsPresent > 0));
                }
            }
            return false;
        }

        public override void UnInit()
        {
        }

        private void UpdateFriendList(ref ListView<COMDT_FRIEND_INFO> allFriends, ref CUIListScript list, bool bSkin, uint heroId, uint skinId, uint price)
        {
            this.m_friendList = allFriends;
            this.m_isBuySkinForFriend = bSkin;
            this.m_buyHeroIDForFriend = heroId;
            this.m_buySkinIDForFriend = skinId;
            this.m_buyPriceForFriend = price;
            list.SetElementAmount(0);
            list.SetElementAmount(this.m_friendList.Count);
        }
    }
}

