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
    public class CTalentBuySystem : Singleton<CTalentBuySystem>
    {
        public uint m_heroID;
        public static string s_talentBuyFormPath = "UGUI/Form/System/Talent/Form_Talent_Buy.prefab";

        public bool CheckHeroTalentHaveBuy(uint heroID)
        {
            bool flag = false;
            if (TalentView.IsHaveTalentBuyFunc(heroID))
            {
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                if (masterRoleInfo == null)
                {
                    return flag;
                }
                CHeroInfo heroInfo = masterRoleInfo.GetHeroInfo(heroID, false);
                if (heroInfo == null)
                {
                    return flag;
                }
                HeroTalentViewInfo heroTalentViewInfo = TalentView.GetHeroTalentViewInfo(heroID);
                if (heroTalentViewInfo == null)
                {
                    return flag;
                }
                for (int i = 0; i < heroTalentViewInfo.m_heroTalentLevelInfoList.Count; i++)
                {
                    int num2 = heroTalentViewInfo.m_heroTalentLevelInfoList[i].m_levelDetail.bLvl3UnlockLvl;
                    uint targetValue = heroTalentViewInfo.m_heroTalentLevelInfoList[i].m_levelDetail.dwLvl3UnLockCostPrice;
                    RES_SHOPBUY_COINTYPE coinType = (RES_SHOPBUY_COINTYPE) heroTalentViewInfo.m_heroTalentLevelInfoList[i].m_levelDetail.bLvl3UnLockCostType;
                    uint num4 = heroTalentViewInfo.m_heroTalentLevelInfoList[i].m_levelDetail.dwLvl3LockCostPrice;
                    RES_SHOPBUY_COINTYPE res_shopbuy_cointype2 = (RES_SHOPBUY_COINTYPE) heroTalentViewInfo.m_heroTalentLevelInfoList[i].m_levelDetail.bLvl3LockCostType;
                    if (((i == 0) || TalentView.IsBuyTalent(heroID, i - 1)) && !TalentView.IsBuyTalent(heroID, i))
                    {
                        if ((heroInfo.m_ProficiencyLV >= num2) && masterRoleInfo.CheckCoinEnough(coinType, targetValue))
                        {
                            return true;
                        }
                        if ((heroInfo.m_ProficiencyLV < num2) && masterRoleInfo.CheckCoinEnough(res_shopbuy_cointype2, num4))
                        {
                            return true;
                        }
                    }
                }
            }
            return flag;
        }

        public override void Init()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Talent_Buy_Open, new CUIEventManager.OnUIEventHandler(this.Talent_Buy_Open));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Talent_Buy_Close, new CUIEventManager.OnUIEventHandler(this.Talent_Buy_Close));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Talent_Buy_BtnSellClick, new CUIEventManager.OnUIEventHandler(this.Talent_Buy_BtnSellClick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Talent_Buy_ConfirmClick, new CUIEventManager.OnUIEventHandler(this.Talent_Buy_ConfirmClick));
        }

        [MessageHandler(0x48d)]
        public static void ReciveTalentBuyInfo(CSPkg msg)
        {
            SCPKG_CMD_TALENT_BUY stTalentBuyRsp = msg.stPkgData.stTalentBuyRsp;
            if (stTalentBuyRsp.iResult == 0)
            {
                Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().GetHeroInfo(stTalentBuyRsp.dwHeroID, false).m_talentBuyList[stTalentBuyRsp.bTalentIdx] = 1;
                Singleton<CTalentBuySystem>.GetInstance().RefreshForm(stTalentBuyRsp.bTalentIdx);
                Singleton<CUIManager>.instance.OpenTips(Singleton<CTextManager>.instance.GetText("Talent_Buy_9"), false, 1f, null, new object[0]);
                Singleton<CHeroInfoSystem2>.GetInstance().RefreshHeroInfoForm();
            }
            else
            {
                Singleton<CUIManager>.instance.OpenTips(Singleton<CTextManager>.instance.GetText("Talent_Buy_10"), false, 1f, null, new object[0]);
            }
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
        }

        public void RefreshForm(int effectIndex = -1)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_talentBuyFormPath);
            if (form != null)
            {
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                if (masterRoleInfo != null)
                {
                    CHeroInfo heroInfo = masterRoleInfo.GetHeroInfo(this.m_heroID, false);
                    HeroTalentViewInfo heroTalentViewInfo = TalentView.GetHeroTalentViewInfo(this.m_heroID);
                    if (heroTalentViewInfo != null)
                    {
                        CUIListScript[] scriptArray = new CUIListScript[] { form.gameObject.transform.Find("Panel/PanelLeft/List1").GetComponent<CUIListScript>(), form.gameObject.transform.Find("Panel/PanelLeft/List2").GetComponent<CUIListScript>(), form.gameObject.transform.Find("Panel/PanelLeft/List3").GetComponent<CUIListScript>(), form.gameObject.transform.Find("Panel/PanelLeft/List4").GetComponent<CUIListScript>(), form.gameObject.transform.Find("Panel/PanelLeft/List5").GetComponent<CUIListScript>() };
                        for (int i = 0; i < 5; i++)
                        {
                            CUIListScript script2 = scriptArray[i];
                            HeroTalentLevelInfo info4 = heroTalentViewInfo.m_heroTalentLevelInfoList[i];
                            ResTalentLib lib = info4.m_tarlentLibList[info4.m_tarlentLibList.Count - 1];
                            Button component = script2.transform.Find("BtnBuyCoin").GetComponent<Button>();
                            Text text = script2.transform.Find("lblTips").GetComponent<Text>();
                            Text text2 = text.transform.Find("Text").GetComponent<Text>();
                            Image image = text.transform.Find("Image").GetComponent<Image>();
                            component.gameObject.CustomSetActive(false);
                            text2.gameObject.CustomSetActive(false);
                            image.gameObject.CustomSetActive(false);
                            if (!TalentView.IsHaveTalentBuyFunc(this.m_heroID))
                            {
                                text.text = Singleton<CTextManager>.instance.GetText("Talent_Buy_3");
                            }
                            else if ((heroInfo == null) && (i == 0))
                            {
                                text.text = Singleton<CTextManager>.instance.GetText("Skin_NeedToBuyAHero");
                            }
                            else if (TalentView.IsBuyTalent(this.m_heroID, i))
                            {
                                text.text = Singleton<CTextManager>.instance.GetText("Talent_Buy_2");
                            }
                            else if ((i > 0) && !TalentView.IsBuyTalent(this.m_heroID, i - 1))
                            {
                                int num2 = heroTalentViewInfo.m_heroTalentLevelInfoList[i - 1].m_tarlentLibList.Count - 1;
                                string str = StringHelper.UTF8BytesToString(ref heroTalentViewInfo.m_heroTalentLevelInfoList[i - 1].m_tarlentLibList[num2].szName);
                                string[] args = new string[] { str };
                                text.text = Singleton<CTextManager>.instance.GetText("Talent_Buy_4", args);
                            }
                            else
                            {
                                if (heroInfo == null)
                                {
                                    continue;
                                }
                                string str2 = StringHelper.UTF8BytesToString(ref CHeroInfo.GetHeroProficiency(heroInfo.cfgInfo.bJob, heroInfo.m_ProficiencyLV).szTitle);
                                string str3 = StringHelper.UTF8BytesToString(ref CHeroInfo.GetHeroProficiency(heroInfo.cfgInfo.bJob, info4.m_levelDetail.bLvl3UnlockLvl).szTitle);
                                if (heroInfo.m_ProficiencyLV < info4.m_levelDetail.bLvl3UnlockLvl)
                                {
                                    component.gameObject.CustomSetActive(true);
                                    stUIEventParams eventParams = new stUIEventParams {
                                        tag = info4.m_levelDetail.bLvl3LockCostType,
                                        tagUInt = info4.m_levelDetail.dwLvl3LockCostPrice,
                                        tag2 = i,
                                        tagStr = StringHelper.UTF8BytesToString(ref lib.szName)
                                    };
                                    CMallSystem.SetPayButton(form, (RectTransform) component.transform, CMallSystem.ResBuyTypeToPayType(info4.m_levelDetail.bLvl3LockCostType), info4.m_levelDetail.dwLvl3LockCostPrice, enUIEventID.Talent_Buy_BtnSellClick, ref eventParams);
                                    string[] textArray2 = new string[] { str3 };
                                    text.text = Singleton<CTextManager>.instance.GetText("Talent_Buy_7", textArray2);
                                    text2.gameObject.CustomSetActive(true);
                                    image.gameObject.CustomSetActive(true);
                                    CMallSystem.SetPayButton(form, (RectTransform) text.transform, CMallSystem.ResBuyTypeToPayType(info4.m_levelDetail.bLvl3UnLockCostType), info4.m_levelDetail.dwLvl3UnLockCostPrice, enUIEventID.None, ref eventParams);
                                }
                                else
                                {
                                    component.gameObject.CustomSetActive(true);
                                    stUIEventParams params2 = new stUIEventParams {
                                        tag = info4.m_levelDetail.bLvl3UnLockCostType,
                                        tagUInt = info4.m_levelDetail.dwLvl3UnLockCostPrice,
                                        tag2 = i,
                                        tagStr = StringHelper.UTF8BytesToString(ref lib.szName)
                                    };
                                    CMallSystem.SetPayButton(form, (RectTransform) component.transform, CMallSystem.ResBuyTypeToPayType(info4.m_levelDetail.bLvl3UnLockCostType), info4.m_levelDetail.dwLvl3UnLockCostPrice, enUIEventID.Talent_Buy_BtnSellClick, ref params2);
                                    string[] textArray3 = new string[] { str2 };
                                    text.text = Singleton<CTextManager>.instance.GetText("Talent_Buy_8", textArray3);
                                }
                            }
                            script2.SetElementAmount(info4.m_tarlentLibList.Count);
                            for (int j = 0; j < info4.m_tarlentLibList.Count; j++)
                            {
                                GameObject gameObject = script2.GetElemenet(j).gameObject.transform.Find("talentCell").gameObject;
                                Image image2 = gameObject.transform.Find("imgIcon").GetComponent<Image>();
                                Image image3 = gameObject.transform.Find("lock").GetComponent<Image>();
                                Text text3 = gameObject.transform.Find("lblName").GetComponent<Text>();
                                CanvasGroup group = gameObject.GetComponent<CanvasGroup>();
                                CUIEventScript script3 = gameObject.GetComponent<CUIEventScript>();
                                ResTalentLib lib2 = info4.m_tarlentLibList[j];
                                if (lib2 == null)
                                {
                                    return;
                                }
                                image2.SetSprite(CUIUtility.s_Sprite_Dynamic_Talent_Dir + lib2.dwIcon, form, true, false, false);
                                image2.color = CUIUtility.s_Color_White;
                                text3.text = StringHelper.UTF8BytesToString(ref lib2.szName);
                                stTalentEventParams params3 = new stTalentEventParams {
                                    talentLevelIndex = (byte) i,
                                    talentInfo = info4.m_tarlentLibList[j],
                                    isCanLearn = false
                                };
                                if (TalentView.IsNeedBuy(i, j) && !TalentView.IsBuyTalent(this.m_heroID, i))
                                {
                                    params3.isHaveTalent = false;
                                    image3.gameObject.CustomSetActive(true);
                                    image2.color = CUIUtility.s_Color_GrayShader;
                                }
                                else
                                {
                                    params3.isHaveTalent = true;
                                    image3.gameObject.CustomSetActive(false);
                                    image2.color = CUIUtility.s_Color_White;
                                    if ((effectIndex == i) && (j == (info4.m_tarlentLibList.Count - 1)))
                                    {
                                        CUICommonSystem.PlayAnimator(gameObject, "UnLock_Anim");
                                    }
                                }
                                string strContent = StringHelper.UTF8BytesToString(ref lib2.szDesc);
                                CUICommonSystem.SetCommonTipsEvent(form, gameObject, strContent, enUseableTipsPos.enTop);
                            }
                        }
                    }
                }
            }
        }

        public static void SendBuyTalentMsg(uint heroID, int talentIndex)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x48c);
            msg.stPkgData.stTalentBuyReq.bTalentIdx = (byte) talentIndex;
            msg.stPkgData.stTalentBuyReq.dwHeroID = heroID;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        private void Talent_Buy_BtnSellClick(CUIEvent uiEvent)
        {
            enPayType payType = CMallSystem.ResBuyTypeToPayType(uiEvent.m_eventParams.tag);
            uint tagUInt = uiEvent.m_eventParams.tagUInt;
            string[] args = new string[] { uiEvent.m_eventParams.tagStr };
            string text = Singleton<CTextManager>.instance.GetText("Talent_Buy_5", args);
            CMallSystem.TryToPay(enPayPurpose.Buy, text, payType, tagUInt, enUIEventID.Talent_Buy_ConfirmClick, ref uiEvent.m_eventParams, enUIEventID.None, true, true);
        }

        private void Talent_Buy_Close(CUIEvent uiEvent)
        {
            CUICommonSystem.CloseCommonTips();
        }

        private void Talent_Buy_ConfirmClick(CUIEvent uiEvent)
        {
            SendBuyTalentMsg(this.m_heroID, uiEvent.m_eventParams.tag2);
        }

        private void Talent_Buy_Open(CUIEvent uiEvent)
        {
            this.m_heroID = uiEvent.m_eventParams.tagUInt;
            Singleton<CUIManager>.GetInstance().OpenForm(s_talentBuyFormPath, false, true);
            this.RefreshForm(-1);
        }
    }
}

