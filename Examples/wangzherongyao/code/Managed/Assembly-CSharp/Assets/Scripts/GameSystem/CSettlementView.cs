namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.GameLogic.DataCenter;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;

    internal class CSettlementView
    {
        private static float _coinFrom;
        private static LTDescr _coinLTD;
        private static float _coinTo;
        private static Text _coinTweenText;
        private static GameObject _continueBtn;
        private static float _expFrom;
        private static LTDescr _expLTD;
        private static float _expTo;
        private static RectTransform _expTweenRect;
        private static uint _lvUpGrade;
        [CompilerGenerated]
        private static Action<float> <>f__am$cacheA;
        [CompilerGenerated]
        private static Action<float> <>f__am$cacheB;
        private const float expBarWidth = 327.6f;
        public const int MAX_ACHIEVEMENT = 6;
        private const float proficientBarWidth = 205f;
        private const float TweenTime = 2f;

        private static void DoCoinAndExpTween()
        {
            try
            {
                if ((_coinTweenText != null) && (_coinTweenText.gameObject != null))
                {
                    if (<>f__am$cacheA == null)
                    {
                        <>f__am$cacheA = delegate (float value) {
                            if ((_coinTweenText != null) && (_coinTweenText.gameObject != null))
                            {
                                _coinTweenText.text = string.Format("+{0}", value.ToString("N0"));
                                if (value >= _coinTo)
                                {
                                    DoCoinTweenEnd();
                                }
                            }
                        };
                    }
                    _coinLTD = LeanTween.value(_coinTweenText.gameObject, <>f__am$cacheA, _coinFrom, _coinTo, 2f);
                }
                if ((_expTweenRect != null) && (_expTweenRect.gameObject != null))
                {
                    if (<>f__am$cacheB == null)
                    {
                        <>f__am$cacheB = delegate (float value) {
                            if ((_expTweenRect != null) && (_expTweenRect.gameObject != null))
                            {
                                _expTweenRect.sizeDelta = new Vector2(value * 327.6f, _expTweenRect.sizeDelta.y);
                                if (value >= _expTo)
                                {
                                    DoExpTweenEnd();
                                }
                            }
                        };
                    }
                    _expLTD = LeanTween.value(_expTweenRect.gameObject, <>f__am$cacheB, _expFrom, _expTo, 2f);
                }
            }
            catch (Exception exception)
            {
                object[] inParameters = new object[] { exception.Message };
                DebugHelper.Assert(false, "Exceptin in DoCoinAndExpTween, {0}", inParameters);
            }
        }

        public static void DoCoinTweenEnd()
        {
            if ((_coinLTD != null) && (_coinTweenText != null))
            {
                _coinTweenText.text = string.Format("+{0}", _coinTo.ToString("N0"));
                if (Singleton<BattleStatistic>.GetInstance().multiDetail != null)
                {
                    CUICommonSystem.AppendMultipleText(_coinTweenText, CUseable.GetMultiple(ref Singleton<BattleStatistic>.GetInstance().multiDetail, 0, -1));
                }
                _coinLTD.cancel();
                _coinLTD = null;
                _coinTweenText = null;
            }
            if (_continueBtn != null)
            {
                _continueBtn.CustomSetActive(true);
                _continueBtn = null;
            }
        }

        private static void DoExpTweenEnd()
        {
            if ((_expTweenRect != null) && (_expLTD != null))
            {
                _expTweenRect.sizeDelta = new Vector2(_expTo * 327.6f, _expTweenRect.sizeDelta.y);
                _expLTD.cancel();
                _expLTD = null;
                _expTweenRect = null;
            }
            if (_continueBtn != null)
            {
                _continueBtn.CustomSetActive(true);
                _continueBtn = null;
            }
            if (_lvUpGrade > 1)
            {
                CUIEvent event3 = new CUIEvent {
                    m_eventID = enUIEventID.Settle_OpenLvlUp
                };
                event3.m_eventParams.tag = ((int) _lvUpGrade) - 1;
                event3.m_eventParams.tag2 = (int) _lvUpGrade;
                CUIEvent uiEvent = event3;
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(uiEvent);
            }
            _lvUpGrade = 0;
        }

        private static COMDT_SETTLE_HERO_RESULT_INFO GetHeroSettleInfo(uint HeroId)
        {
            COMDT_SETTLE_HERO_RESULT_DETAIL heroSettleInfo = Singleton<BattleStatistic>.GetInstance().heroSettleInfo;
            if (heroSettleInfo != null)
            {
                for (int i = 0; i < heroSettleInfo.bNum; i++)
                {
                    if ((heroSettleInfo.astHeroList[i] != null) && (heroSettleInfo.astHeroList[i].dwHeroConfID == HeroId))
                    {
                        return heroSettleInfo.astHeroList[i];
                    }
                }
            }
            return null;
        }

        private static string GetProficiencyLvTxt(int heroType, uint level)
        {
            ResHeroProficiency heroProficiency = CHeroInfo.GetHeroProficiency(heroType, (int) level);
            DebugHelper.Assert(heroProficiency != null);
            return ((heroProficiency == null) ? string.Empty : Utility.UTF8Convert(heroProficiency.szTitle));
        }

        public static void HideData(CUIFormScript form)
        {
            form.gameObject.transform.Find("PanelB/StatCon").gameObject.CustomSetActive(false);
        }

        private static void Set3DHero(GameObject root)
        {
        }

        public static void SetAchievementIcon(CUIFormScript formScript, GameObject item, PvpAchievement type, int count)
        {
            if (count <= 6)
            {
                Image component = Utility.FindChild(item, string.Format("Achievement/Image{0}", count)).GetComponent<Image>();
                if (type == PvpAchievement.NULL)
                {
                    component.gameObject.CustomSetActive(false);
                }
                else
                {
                    string prefabPath = CUIUtility.s_Sprite_Dynamic_Pvp_Settle_Dir + type.ToString();
                    component.gameObject.CustomSetActive(true);
                    component.SetSprite(prefabPath, formScript, true, false, false);
                }
            }
        }

        private static void SetCoinInfo(GameObject root)
        {
            COMDT_ACNT_INFO acntInfo = Singleton<BattleStatistic>.GetInstance().acntInfo;
            Text component = root.transform.Find("PanelA/Award/ItemAndCoin/Panel_Gold/GoldNum").GetComponent<Text>();
            component.text = "+0";
            if (acntInfo != null)
            {
                _coinFrom = 0f;
                _coinTo = acntInfo.dwPvpSettleCoin;
                _coinTweenText = component;
            }
        }

        private static void SetExpInfo(GameObject root, CUIFormScript formScript)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            DebugHelper.Assert(masterRoleInfo != null, "can't find roleinfo");
            if (masterRoleInfo != null)
            {
                ResAcntPvpExpInfo dataByKey = GameDataMgr.acntPvpExpDatabin.GetDataByKey((byte) masterRoleInfo.PvpLevel);
                object[] inParameters = new object[] { masterRoleInfo.PvpLevel };
                DebugHelper.Assert(dataByKey != null, "can't find resexp id={0}", inParameters);
                if (dataByKey != null)
                {
                    root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpExpNode/PvpLevelTxt").GetComponent<Text>().text = string.Format("Lv.{0}", dataByKey.bLevel.ToString());
                    Text component = root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpExpNode/PvpExpTxt").GetComponent<Text>();
                    Text text3 = root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpExpNode/ExpMax").GetComponent<Text>();
                    Text text4 = root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpExpNode/PlayerName").GetComponent<Text>();
                    CUIHttpImageScript script = root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpExpNode/HeadImage").GetComponent<CUIHttpImageScript>();
                    string headUrl = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().HeadUrl;
                    script.SetImageUrl(headUrl);
                    Image image = root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpExpNode/NobeIcon").GetComponent<Image>();
                    MonoSingleton<NobeSys>.GetInstance().SetNobeIcon(image, (int) Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().GetNobeInfo().stGameVipClient.dwCurLevel, false);
                    Image image2 = root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpExpNode/HeadFrame").GetComponent<Image>();
                    MonoSingleton<NobeSys>.GetInstance().SetHeadIconBk(image2, (int) Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().GetNobeInfo().stGameVipClient.dwHeadIconId);
                    SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
                    DebugHelper.Assert(curLvelContext != null, "Battle Level Context is NULL!!");
                    GameObject gameObject = root.transform.Find("PanelA/Award/RankCon").gameObject;
                    gameObject.CustomSetActive(false);
                    if (curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER)
                    {
                        COMDT_RANK_SETTLE_INFO rankInfo = Singleton<BattleStatistic>.GetInstance().rankInfo;
                        if (rankInfo != null)
                        {
                            gameObject.CustomSetActive(true);
                            Text text5 = gameObject.transform.FindChild(string.Format("txtRankName", new object[0])).gameObject.GetComponent<Text>();
                            Text text6 = gameObject.transform.FindChild(string.Format("WangZheXingTxt", new object[0])).gameObject.GetComponent<Text>();
                            text5.text = StringHelper.UTF8BytesToString(ref GameDataMgr.rankGradeDatabin.GetDataByKey(rankInfo.bNowGrade).szGradeDesc);
                            if (rankInfo.bNowGrade == GameDataMgr.rankGradeDatabin.count)
                            {
                                Transform transform = gameObject.transform.FindChild(string.Format("XingGrid/ImgScore{0}", 1));
                                if (transform != null)
                                {
                                    transform.gameObject.CustomSetActive(true);
                                }
                                text6.gameObject.CustomSetActive(true);
                                text6.text = string.Format("X{0}", rankInfo.dwNowScore);
                            }
                            else
                            {
                                text6.gameObject.CustomSetActive(false);
                                for (int i = 1; i <= rankInfo.dwNowScore; i++)
                                {
                                    Transform transform2 = gameObject.transform.FindChild(string.Format("XingGrid/ImgScore{0}", i));
                                    if (transform2 != null)
                                    {
                                        transform2.gameObject.CustomSetActive(true);
                                    }
                                }
                            }
                            root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpLevelNode").gameObject.CustomSetActive(false);
                        }
                    }
                    Image image3 = root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpExpNode/QQVIPIcon").GetComponent<Image>();
                    MonoSingleton<NobeSys>.GetInstance().SetMyQQVipHead(image3);
                    COMDT_REWARD_MULTIPLE_DETAIL multiDetail = Singleton<BattleStatistic>.GetInstance().multiDetail;
                    if (multiDetail != null)
                    {
                        string[] strArray = new string[6];
                        StringBuilder builder = new StringBuilder();
                        int num2 = CUseable.GetMultiple(ref multiDetail, 15, -1);
                        if (num2 > 0)
                        {
                            COMDT_MULTIPLE_INFO comdt_multiple_info = CUseable.GetMultipleInfo(ref multiDetail, 15, -1);
                            string[] args = new string[] { num2.ToString() };
                            strArray[0] = Singleton<CTextManager>.instance.GetText("Pvp_settle_Common_Tips_1", args);
                            if (comdt_multiple_info.dwPvpDailyRatio > 0)
                            {
                                string[] textArray2 = new string[] { masterRoleInfo.dailyPvpCnt.ToString(), (comdt_multiple_info.dwPvpDailyRatio / 100).ToString() };
                                strArray[1] = Singleton<CTextManager>.instance.GetText("Pvp_settle_Common_Tips_2", textArray2);
                            }
                            if (comdt_multiple_info.dwQQVIPRatio > 0)
                            {
                                if (masterRoleInfo.HasVip(0x10))
                                {
                                    string[] textArray3 = new string[] { (comdt_multiple_info.dwQQVIPRatio / 100).ToString() };
                                    strArray[2] = Singleton<CTextManager>.instance.GetText("Pvp_settle_Common_Tips_9", textArray3);
                                }
                                else
                                {
                                    string[] textArray4 = new string[] { (comdt_multiple_info.dwQQVIPRatio / 100).ToString() };
                                    strArray[2] = Singleton<CTextManager>.instance.GetText("Pvp_settle_Common_Tips_3", textArray4);
                                }
                            }
                            if (comdt_multiple_info.dwPropRatio > 0)
                            {
                                strArray[3] = string.Format(Singleton<CTextManager>.GetInstance().GetText("Pvp_settle_Common_Tips_4"), comdt_multiple_info.dwPropRatio / 100, masterRoleInfo.GetExpWinCount(), Math.Ceiling((double) (((float) masterRoleInfo.GetExpExpireHours()) / 24f)));
                            }
                            if (comdt_multiple_info.dwWealRatio > 0)
                            {
                                string[] textArray5 = new string[] { (comdt_multiple_info.dwWealRatio / 100).ToString() };
                                strArray[4] = Singleton<CTextManager>.instance.GetText("Pvp_settle_Common_Tips_6", textArray5);
                            }
                            if (comdt_multiple_info.dwWXGameCenterLoginRatio > 0)
                            {
                                string[] textArray6 = new string[] { (comdt_multiple_info.dwWXGameCenterLoginRatio / 100).ToString() };
                                strArray[5] = Singleton<CTextManager>.instance.GetText("Pvp_settle_Common_Tips_13", textArray6);
                            }
                            builder.Append(strArray[0]);
                            for (int j = 1; j < strArray.Length; j++)
                            {
                                if (!string.IsNullOrEmpty(strArray[j]))
                                {
                                    builder.Append("\n");
                                    builder.Append(strArray[j]);
                                }
                            }
                            GameObject obj3 = root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpExpNode/DoubleExp").gameObject;
                            obj3.CustomSetActive(true);
                            obj3.GetComponentInChildren<Text>().text = string.Format("+{0}%", num2);
                            CUICommonSystem.SetCommonTipsEvent(formScript, obj3, builder.ToString(), enUseableTipsPos.enLeft);
                        }
                        else
                        {
                            root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpExpNode/DoubleExp").gameObject.CustomSetActive(false);
                        }
                        GameObject obj5 = root.transform.Find("PanelA/Award/ItemAndCoin/Panel_Gold/GoldMax").gameObject;
                        if (Singleton<BattleStatistic>.GetInstance().acntInfo.bReachDailyLimit > 0)
                        {
                            obj5.CustomSetActive(true);
                        }
                        else
                        {
                            obj5.CustomSetActive(false);
                        }
                        int num4 = CUseable.GetMultiple(ref multiDetail, 11, -1);
                        if (num4 > 0)
                        {
                            COMDT_MULTIPLE_INFO comdt_multiple_info2 = CUseable.GetMultipleInfo(ref multiDetail, 11, -1);
                            strArray = new string[5];
                            string[] textArray7 = new string[] { num4.ToString() };
                            strArray[0] = Singleton<CTextManager>.instance.GetText("Pvp_settle_Common_Tips_7", textArray7);
                            if (comdt_multiple_info2.dwPvpDailyRatio > 0)
                            {
                                string[] textArray8 = new string[] { masterRoleInfo.dailyPvpCnt.ToString(), (comdt_multiple_info2.dwPvpDailyRatio / 100).ToString() };
                                strArray[1] = Singleton<CTextManager>.instance.GetText("Pvp_settle_Common_Tips_8", textArray8);
                            }
                            if (comdt_multiple_info2.dwQQVIPRatio > 0)
                            {
                                if (masterRoleInfo.HasVip(0x10))
                                {
                                    string[] textArray9 = new string[] { (comdt_multiple_info2.dwQQVIPRatio / 100).ToString() };
                                    strArray[2] = Singleton<CTextManager>.instance.GetText("Pvp_settle_Common_Tips_9", textArray9);
                                }
                                else
                                {
                                    string[] textArray10 = new string[] { (comdt_multiple_info2.dwQQVIPRatio / 100).ToString() };
                                    strArray[2] = Singleton<CTextManager>.instance.GetText("Pvp_settle_Common_Tips_3", textArray10);
                                }
                            }
                            if (comdt_multiple_info2.dwPropRatio > 0)
                            {
                                strArray[3] = string.Format(Singleton<CTextManager>.GetInstance().GetText("Pvp_settle_Common_Tips_10"), comdt_multiple_info2.dwPropRatio / 100, masterRoleInfo.GetCoinWinCount(), Math.Ceiling((double) (((float) masterRoleInfo.GetCoinExpireHours()) / 24f)));
                            }
                            if (comdt_multiple_info2.dwWealRatio > 0)
                            {
                                string[] textArray11 = new string[] { (comdt_multiple_info2.dwWealRatio / 100).ToString() };
                                strArray[4] = Singleton<CTextManager>.instance.GetText("Pvp_settle_Common_Tips_12", textArray11);
                            }
                            if (comdt_multiple_info2.dwWXGameCenterLoginRatio > 0)
                            {
                                string[] textArray12 = new string[] { (comdt_multiple_info2.dwWXGameCenterLoginRatio / 100).ToString() };
                                strArray[5] = Singleton<CTextManager>.instance.GetText("Pvp_settle_Common_Tips_13", textArray12);
                            }
                            builder.Remove(0, builder.Length);
                            builder.Append(strArray[0]);
                            for (int k = 1; k < strArray.Length; k++)
                            {
                                if (!string.IsNullOrEmpty(strArray[k]))
                                {
                                    builder.Append("\n");
                                    builder.Append(strArray[k]);
                                }
                            }
                            GameObject obj6 = root.transform.Find("PanelA/Award/ItemAndCoin/Panel_Gold/DoubleCoin").gameObject;
                            obj6.CustomSetActive(true);
                            obj6.GetComponentInChildren<Text>().text = string.Format("+{0}%", num4);
                            CUICommonSystem.SetCommonTipsEvent(formScript, obj6, builder.ToString(), enUseableTipsPos.enLeft);
                        }
                        else
                        {
                            root.transform.Find("PanelA/Award/ItemAndCoin/Panel_Gold/DoubleCoin").gameObject.CustomSetActive(false);
                        }
                    }
                    text4.text = masterRoleInfo.Name;
                    RectTransform transform3 = root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpExpNode/PvpExpSliderBg/BasePvpExpSlider").gameObject.GetComponent<RectTransform>();
                    RectTransform transform4 = root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpExpNode/PvpExpSliderBg/AddPvpExpSlider").gameObject.GetComponent<RectTransform>();
                    COMDT_ACNT_INFO acntInfo = Singleton<BattleStatistic>.GetInstance().acntInfo;
                    if (acntInfo != null)
                    {
                        if (acntInfo.dwPvpSettleExp > 0)
                        {
                            Singleton<CSoundManager>.GetInstance().PostEvent("UI_count_jingyan", null);
                        }
                        int num6 = (int) (acntInfo.dwPvpExp - acntInfo.dwPvpSettleExp);
                        if (num6 < 0)
                        {
                            _lvUpGrade = acntInfo.dwPvpLv;
                        }
                        else
                        {
                            _lvUpGrade = 0;
                        }
                        float num7 = Mathf.Max((float) 0f, (float) (((float) num6) / ((float) dataByKey.dwNeedExp)));
                        float num8 = Mathf.Max((float) 0f, (float) (((num6 >= 0) ? ((float) acntInfo.dwPvpSettleExp) : ((float) acntInfo.dwPvpExp)) / ((float) dataByKey.dwNeedExp)));
                        root.transform.FindChild("PanelA/Award/Panel_PlayerExp/PvpExpNode/AddPvpExpTxt").GetComponent<Text>().text = (acntInfo.dwPvpSettleExp <= 0) ? string.Empty : string.Format("+{0}", acntInfo.dwPvpSettleExp).ToString();
                        if (acntInfo.dwPvpSettleExp == 0)
                        {
                            root.transform.FindChild("PanelA/Award/Panel_PlayerExp/PvpExpNode/Bar2").gameObject.CustomSetActive(false);
                        }
                        transform3.sizeDelta = new Vector2(num7 * 327.6f, transform3.sizeDelta.y);
                        transform4.sizeDelta = new Vector2(num7 * 327.6f, transform4.sizeDelta.y);
                        _expFrom = num7;
                        _expTo = num7 + num8;
                        _expTweenRect = transform4;
                        transform3.gameObject.CustomSetActive(num6 >= 0);
                        text3.text = (acntInfo.bExpDailyLimit <= 0) ? string.Empty : Singleton<CTextManager>.GetInstance().GetText("GetExp_Limit");
                        component.text = string.Format("{0}/{1}", acntInfo.dwPvpExp.ToString(), dataByKey.dwNeedExp.ToString());
                    }
                }
            }
        }

        private static void SetHeroStat(CUIFormScript formScript, GameObject item, GameObject statItem, PlayerKDA playerKDA, HeroKDA heroKDA, bool bSelf, bool bMvp, bool bWin)
        {
            Utility.GetComponetInChild<Text>(item, "Txt_PlayerLevel").text = string.Format("Lv.{0}", heroKDA.SoulLevel.ToString());
            ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey((uint) heroKDA.HeroId);
            DebugHelper.Assert(dataByKey != null);
            item.transform.Find("Txt_HeroName").gameObject.GetComponent<Text>().text = StringHelper.UTF8BytesToString(ref dataByKey.szName);
            string str = (heroKDA.numKill >= 10) ? heroKDA.numKill.ToString() : string.Format(" {0} ", heroKDA.numKill.ToString());
            string str2 = (heroKDA.numDead >= 10) ? heroKDA.numDead.ToString() : string.Format(" {0} ", heroKDA.numDead.ToString());
            string str3 = (heroKDA.numAssist >= 10) ? heroKDA.numAssist.ToString() : string.Format(" {0}", heroKDA.numAssist.ToString());
            item.transform.Find("Txt_KDA").gameObject.GetComponent<Text>().text = string.Format("{0} / {1} / {2}", str, str2, str3);
            statItem.transform.Find("Txt_Hurt").gameObject.GetComponent<Text>().text = heroKDA.hurtToEnemy.ToString();
            statItem.transform.Find("Txt_HurtTaken").gameObject.GetComponent<Text>().text = heroKDA.hurtTakenByEnemy.ToString();
            statItem.transform.Find("Txt_Heal").gameObject.GetComponent<Text>().text = heroKDA.hurtToHero.ToString();
            item.transform.Find("KillerImg").gameObject.GetComponent<Image>().SetSprite(string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Icon_Dir, CSkinInfo.GetHeroSkinPic((uint) heroKDA.HeroId, 0)), formScript, true, false, false);
            GameObject gameObject = item.transform.Find("Mvp").gameObject;
            gameObject.CustomSetActive(bMvp);
            if (bMvp)
            {
                Image component = gameObject.GetComponent<Image>();
                if (bWin)
                {
                    component.SetSprite(CUIUtility.s_Sprite_Dynamic_Pvp_Settle_Dir + "Img_Icon_Red_Mvp", formScript, true, false, false);
                    component.gameObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                }
                else
                {
                    component.SetSprite(CUIUtility.s_Sprite_Dynamic_Pvp_Settle_Dir + "Img_Icon_Blue_Mvp", formScript, true, false, false);
                    component.gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                }
            }
            for (int i = 0; i < 5; i++)
            {
                uint dwTalentID = heroKDA.TalentArr[i].dwTalentID;
                int num17 = i + 1;
                Image image = item.transform.FindChild(string.Format("TianFu/TianFuIcon{0}", num17.ToString())).GetComponent<Image>();
                if (dwTalentID == 0)
                {
                    image.gameObject.CustomSetActive(false);
                }
                else
                {
                    image.gameObject.CustomSetActive(true);
                    ResTalentLib lib = GameDataMgr.talentLib.GetDataByKey(dwTalentID);
                    image.SetSprite(CUIUtility.s_Sprite_Dynamic_Talent_Dir + lib.dwIcon, formScript, true, false, false);
                }
            }
            Transform transform = item.transform.FindChild(string.Format("TianFu/TianFuIcon{0}", 6));
            if (transform != null)
            {
                Image image4 = transform.GetComponent<Image>();
                uint commonSkillID = heroKDA.commonSkillID;
                if (commonSkillID != 0)
                {
                    ResSkillCfgInfo info2 = GameDataMgr.skillDatabin.GetDataByKey(commonSkillID);
                    if (info2 != null)
                    {
                        image4.gameObject.CustomSetActive(true);
                        string prefabPath = string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Skill_Dir, Utility.UTF8Convert(info2.szIconPath));
                        image4.SetSprite(prefabPath, formScript, true, false, false);
                    }
                    else
                    {
                        image4.gameObject.CustomSetActive(false);
                    }
                }
                else
                {
                    image4.gameObject.CustomSetActive(false);
                }
            }
            int count = 1;
            for (int j = 1; j < 13; j++)
            {
                switch (((PvpAchievement) j))
                {
                    case PvpAchievement.Legendary:
                        if (heroKDA.LegendaryNum > 0)
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.Legendary, count);
                            count++;
                        }
                        break;

                    case PvpAchievement.DoubleKill:
                        if (heroKDA.DoubleKillNum <= 0)
                        {
                        }
                        break;

                    case PvpAchievement.TripleKill:
                        if (heroKDA.TripleKillNum > 0)
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.TripleKill, count);
                            count++;
                        }
                        break;

                    case PvpAchievement.KillMost:
                        if (heroKDA.bKillMost)
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.KillMost, count);
                            count++;
                        }
                        break;

                    case PvpAchievement.HurtMost:
                        if (heroKDA.bHurtMost)
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.HurtMost, count);
                            count++;
                        }
                        break;

                    case PvpAchievement.HurtTakenMost:
                        if (heroKDA.bHurtTakenMost)
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.HurtTakenMost, count);
                            count++;
                        }
                        break;

                    case PvpAchievement.AsssistMost:
                        if (heroKDA.bAsssistMost)
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.AsssistMost, count);
                            count++;
                        }
                        break;
                }
            }
            for (int k = count; k <= 6; k++)
            {
                SetAchievementIcon(formScript, item, PvpAchievement.NULL, k);
            }
            GameObject obj3 = item.transform.Find("SymbolLevel").gameObject;
            IGameActorDataProvider actorDataProvider = Singleton<ActorDataCenter>.instance.GetActorDataProvider(GameActorDataProviderType.ServerDataProvider);
            ActorMeta actorMeta = new ActorMeta();
            ActorServerData actorData = new ActorServerData();
            obj3.CustomSetActive(false);
            actorMeta.PlayerId = playerKDA.PlayerId;
            actorMeta.ActorCamp = playerKDA.PlayerCamp;
            actorMeta.ConfigId = heroKDA.HeroId;
            actorMeta.ActorType = ActorTypeDef.Actor_Type_Hero;
            if (actorDataProvider.GetActorServerData(ref actorMeta, ref actorData))
            {
                int symbolLvWithArray = CSymbolInfo.GetSymbolLvWithArray(actorData.SymbolID);
                if (symbolLvWithArray > 0)
                {
                    obj3.CustomSetActive(true);
                    Utility.GetComponetInChild<Text>(obj3, "Text").text = symbolLvWithArray.ToString();
                }
            }
        }

        private static void SetHeroStat_Share(CUIFormScript formScript, GameObject item, HeroKDA kda, bool bSelf, bool bMvp, bool bWin)
        {
            Utility.GetComponetInChild<Text>(item, "Txt_PlayerLevel").text = string.Format("Lv.{0}", kda.SoulLevel.ToString());
            ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey((uint) kda.HeroId);
            DebugHelper.Assert(dataByKey != null);
            item.transform.Find("Txt_HeroName").gameObject.GetComponent<Text>().text = StringHelper.UTF8BytesToString(ref dataByKey.szName);
            string str = (kda.numKill >= 10) ? kda.numKill.ToString() : string.Format(" {0} ", kda.numKill.ToString());
            string str2 = (kda.numDead >= 10) ? kda.numDead.ToString() : string.Format(" {0} ", kda.numDead.ToString());
            string str3 = (kda.numAssist >= 10) ? kda.numAssist.ToString() : string.Format(" {0}", kda.numAssist.ToString());
            item.transform.Find("Txt_KDA").gameObject.GetComponent<Text>().text = string.Format("{0} / {1} / {2}", str, str2, str3);
            item.transform.Find("KillerImg").gameObject.GetComponent<Image>().SetSprite(string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Icon_Dir, CSkinInfo.GetHeroSkinPic((uint) kda.HeroId, 0)), formScript, true, false, false);
            GameObject gameObject = item.transform.Find("Mvp").gameObject;
            gameObject.CustomSetActive(bMvp);
            if (bMvp)
            {
                Image component = gameObject.GetComponent<Image>();
                if (bWin)
                {
                    component.SetSprite(CUIUtility.s_Sprite_Dynamic_Pvp_Settle_Dir + "Img_Icon_Red_Mvp", formScript, true, false, false);
                    component.gameObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                }
                else
                {
                    component.SetSprite(CUIUtility.s_Sprite_Dynamic_Pvp_Settle_Dir + "Img_Icon_Blue_Mvp", formScript, true, false, false);
                    component.gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                }
            }
            for (int i = 0; i < 5; i++)
            {
                uint dwTalentID = kda.TalentArr[i].dwTalentID;
                int num12 = i + 1;
                Image image = item.transform.FindChild(string.Format("TianFu/TianFuIcon{0}", num12.ToString())).GetComponent<Image>();
                if (dwTalentID == 0)
                {
                    image.gameObject.CustomSetActive(false);
                }
                else
                {
                    image.gameObject.CustomSetActive(true);
                    ResTalentLib lib = GameDataMgr.talentLib.GetDataByKey(dwTalentID);
                    image.SetSprite(CUIUtility.s_Sprite_Dynamic_Talent_Dir + lib.dwIcon, formScript, true, false, false);
                }
            }
            int count = 1;
            for (int j = 1; j < 13; j++)
            {
                switch (((PvpAchievement) j))
                {
                    case PvpAchievement.Legendary:
                        if (kda.LegendaryNum > 0)
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.Legendary, count);
                            count++;
                        }
                        break;

                    case PvpAchievement.DoubleKill:
                        if (kda.DoubleKillNum <= 0)
                        {
                        }
                        break;

                    case PvpAchievement.TripleKill:
                        if (kda.TripleKillNum > 0)
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.TripleKill, count);
                            count++;
                        }
                        break;

                    case PvpAchievement.QuataryKill:
                        if (kda.QuataryKillNum > 0)
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.QuataryKill, count);
                            count++;
                        }
                        break;

                    case PvpAchievement.PentaKill:
                        if (kda.PentaKillNum > 0)
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.PentaKill, count);
                            count++;
                        }
                        break;

                    case PvpAchievement.KillMost:
                        if (kda.bKillMost)
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.KillMost, count);
                            count++;
                        }
                        break;

                    case PvpAchievement.HurtMost:
                        if (kda.bHurtMost && (kda.hurtToEnemy > 0))
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.HurtMost, count);
                            count++;
                        }
                        break;

                    case PvpAchievement.HurtTakenMost:
                        if (kda.bHurtTakenMost && (kda.hurtTakenByEnemy > 0))
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.HurtTakenMost, count);
                            count++;
                        }
                        break;

                    case PvpAchievement.AsssistMost:
                        if (kda.bAsssistMost)
                        {
                            SetAchievementIcon(formScript, item, PvpAchievement.AsssistMost, count);
                            count++;
                        }
                        break;
                }
            }
            for (int k = count; k <= 6; k++)
            {
                SetAchievementIcon(formScript, item, PvpAchievement.NULL, k);
            }
        }

        private static void SetPlayerStat(CUIFormScript formScript, GameObject item, GameObject statItem, PlayerKDA kda, uint hostPlayerID, uint MvpPlayerId, bool bWin)
        {
            bool bMvp = MvpPlayerId == kda.PlayerId;
            Text componetInChild = Utility.GetComponetInChild<Text>(item, "Txt_PlayerName");
            componetInChild.text = kda.PlayerName;
            Text text2 = Utility.GetComponetInChild<Text>(item, "Txt_PlayerLevel");
            bool bSelf = hostPlayerID == kda.PlayerId;
            if (bSelf)
            {
                componetInChild.color = CUIUtility.s_Text_Color_Self;
                text2.color = CUIUtility.s_Text_Color_Self;
            }
            IEnumerator<HeroKDA> enumerator = kda.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SetHeroStat(formScript, item, statItem, kda, enumerator.Current, bSelf, bMvp, bWin);
            }
            GameObject gameObject = item.transform.Find("Btn_AddFriend").gameObject;
            GameObject obj3 = item.transform.Find("Btn_Report").gameObject;
            if (kda.IsComputer || bSelf)
            {
                gameObject.CustomSetActive(false);
                obj3.CustomSetActive(false);
            }
            else
            {
                gameObject.CustomSetActive(true);
                gameObject.GetComponent<CUIEventScript>().m_onClickEventParams.commonUInt64Param1 = kda.PlayerUid;
                gameObject.GetComponent<CUIEventScript>().m_onClickEventParams.commonUInt64Param2 = (ulong) kda.WorldId;
                obj3.CustomSetActive(true);
                obj3.GetComponent<CUIEventScript>().m_onClickEventParams.commonUInt64Param1 = kda.PlayerUid;
                obj3.GetComponent<CUIEventScript>().m_onClickEventParams.commonUInt64Param2 = (ulong) kda.WorldId;
            }
        }

        private static void SetPlayerStat_Share(CUIFormScript formScript, GameObject item, PlayerKDA kda, uint hostPlayerID, uint MvpPlayerId, bool bWin)
        {
            bool bMvp = MvpPlayerId == kda.PlayerId;
            Text componetInChild = Utility.GetComponetInChild<Text>(item, "Txt_PlayerName");
            componetInChild.text = kda.PlayerName;
            Text text2 = Utility.GetComponetInChild<Text>(item, "Txt_PlayerLevel");
            bool bSelf = hostPlayerID == kda.PlayerId;
            if (bSelf)
            {
                componetInChild.color = CUIUtility.s_Text_Color_Self;
                text2.color = CUIUtility.s_Text_Color_Self;
            }
            IEnumerator<HeroKDA> enumerator = kda.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SetHeroStat_Share(formScript, item, enumerator.Current, bSelf, bMvp, bWin);
            }
        }

        private static void SetProficiency(GameObject root)
        {
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            DebugHelper.Assert(masterRoleInfo != null, "roleInfo == null SetProficiency");
            if (((masterRoleInfo != null) && (Singleton<BattleLogic>.GetInstance().battleStat != null)) && ((Singleton<BattleLogic>.GetInstance().battleStat.m_playerKDAStat != null) && (Singleton<BattleLogic>.GetInstance().battleStat.m_playerKDAStat.GetHostKDA() != null)))
            {
                CHeroInfo info2;
                GameObject gameObject = root.transform.Find("PanelA/Award/ProficiencyNode").gameObject;
                RectTransform component = root.transform.Find("PanelA/Award/ProficiencyNode/ProficiencySliderBg/BaseProficiencySlider").gameObject.GetComponent<RectTransform>();
                RectTransform transform2 = root.transform.Find("PanelA/Award/ProficiencyNode/ProficiencySliderBg/AddProficiencySlider").gameObject.GetComponent<RectTransform>();
                Text text = root.transform.Find("PanelA/Award/ProficiencyNode/HeroName").GetComponent<Text>();
                Text text2 = root.transform.Find("PanelA/Award/ProficiencyNode/ProficiencyLv").GetComponent<Text>();
                Text text3 = root.transform.Find("PanelA/Award/ProficiencyNode/ProficiencyTxt").GetComponent<Text>();
                Text text4 = root.transform.Find("PanelA/Award/ProficiencyNode/AddProficiencyTxt").GetComponent<Text>();
                Image image = root.transform.Find("PanelA/Award/ProficiencyNode/HeroCell/imgIcon").GetComponent<Image>();
                IEnumerator<HeroKDA> enumerator = Singleton<BattleLogic>.GetInstance().battleStat.m_playerKDAStat.GetHostKDA().GetEnumerator();
                uint id = 0;
                uint skinId = 0;
                while (enumerator.MoveNext())
                {
                    HeroKDA current = enumerator.Current;
                    if (current != null)
                    {
                        id = (uint) current.HeroId;
                        skinId = current.SkinId;
                        break;
                    }
                }
                masterRoleInfo.GetHeroInfo(id, out info2, false);
                IHeroData data = CHeroDataFactory.CreateHeroData(id);
                if (info2 != null)
                {
                    ResHeroCfgInfo info3 = GameDataMgr.heroDatabin.GetDataByKey(id);
                    image.SetSprite(string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Icon_Dir, CSkinInfo.GetHeroSkinPic(id, 0)), root.GetComponent<CUIFormScript>(), true, false, false);
                }
                COMDT_SETTLE_HERO_RESULT_INFO heroSettleInfo = GetHeroSettleInfo(id);
                float num3 = 0f;
                float num4 = 0f;
                int num5 = 0;
                if (Singleton<BattleLogic>.GetInstance().GetCurLvelContext().isPVPLevel && (heroSettleInfo != null))
                {
                    int maxProficiency = CHeroInfo.GetMaxProficiency();
                    ResHeroProficiency heroProficiency = CHeroInfo.GetHeroProficiency(data.heroType, (int) heroSettleInfo.dwProficiencyLv);
                    DebugHelper.Assert(heroProficiency != null);
                    if (heroProficiency == null)
                    {
                        return;
                    }
                    if (maxProficiency == heroSettleInfo.dwProficiencyLv)
                    {
                        num3 = 0f;
                        num4 = 0f;
                        text3.text = "MAX";
                    }
                    else
                    {
                        num5 = (int) (heroSettleInfo.dwProficiency - heroSettleInfo.dwSettleProficiency);
                        num3 = Mathf.Max((float) 0f, (float) (((float) num5) / ((float) heroProficiency.dwTopPoint)));
                        num4 = Mathf.Max((float) 0f, (float) (((num5 >= 0) ? ((float) heroSettleInfo.dwSettleProficiency) : ((float) heroSettleInfo.dwProficiency)) / ((float) heroProficiency.dwTopPoint)));
                        text3.text = string.Format("{0} / {1}", heroSettleInfo.dwProficiency.ToString(), heroProficiency.dwTopPoint.ToString());
                    }
                    text2.text = GetProficiencyLvTxt(data.heroType, heroSettleInfo.dwProficiencyLv);
                    gameObject.CustomSetActive(true);
                }
                else if (info2 != null)
                {
                    int num7 = CHeroInfo.GetMaxProficiency();
                    ResHeroProficiency proficiency2 = CHeroInfo.GetHeroProficiency(info2.cfgInfo.bJob, info2.m_ProficiencyLV);
                    object[] objArray1 = new object[] { info2.m_ProficiencyLV };
                    DebugHelper.Assert(proficiency2 != null, " ResHeroProficiency {0} can not be found!!!", objArray1);
                    if (proficiency2 == null)
                    {
                        return;
                    }
                    if (num7 == info2.m_ProficiencyLV)
                    {
                        num3 = 1f;
                        num4 = 0f;
                        text3.text = "MAX";
                    }
                    else
                    {
                        num3 = Mathf.Max((float) 0f, (float) (((float) info2.m_Proficiency) / ((float) proficiency2.dwTopPoint)));
                        num4 = 0f;
                        text3.text = string.Format("{0} / {1}", info2.m_Proficiency.ToString(), proficiency2.dwTopPoint.ToString());
                    }
                    text2.text = GetProficiencyLvTxt(info2.cfgInfo.bJob, info2.m_ProficiencyLV);
                    gameObject.CustomSetActive(true);
                }
                else
                {
                    gameObject.CustomSetActive(false);
                }
                if (heroSettleInfo != null)
                {
                    text4.text = (heroSettleInfo.dwSettleProficiency <= 0) ? null : string.Format("+{0}", heroSettleInfo.dwSettleProficiency);
                    if (heroSettleInfo.dwSettleProficiency == 0)
                    {
                        root.transform.Find("PanelA/Award/ProficiencyNode/Bar2").gameObject.CustomSetActive(false);
                    }
                }
                else
                {
                    text4.text = null;
                    root.transform.Find("PanelA/Award/ProficiencyNode/Bar2").gameObject.CustomSetActive(false);
                }
                transform2.sizeDelta = new Vector2((num3 + num4) * 205f, transform2.sizeDelta.y);
                component.sizeDelta = new Vector2(num3 * 205f, component.sizeDelta.y);
                transform2.gameObject.CustomSetActive(num4 > 0f);
                ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey(id);
                object[] inParameters = new object[] { id };
                DebugHelper.Assert(dataByKey != null, " ResHeroCfgInfo[{0}] can not be find!!!", inParameters);
                if (dataByKey != null)
                {
                    text.text = Utility.UTF8Convert(dataByKey.szName);
                }
            }
        }

        private static void SetRankPoint(GameObject root)
        {
            <SetRankPoint>c__AnonStorey52 storey = new <SetRankPoint>c__AnonStorey52();
            SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
            if (curLvelContext != null)
            {
                GameObject gameObject = root.transform.Find("PanelA/Award/PvpGuildNode").gameObject;
                GameObject obj3 = gameObject.transform.Find("Content/UpImage").gameObject;
                storey.GuildCoinTxt = gameObject.transform.Find("Content/GuildCoinTxt").GetComponent<Text>();
                GuildMemInfo playerGuildMemberInfo = CGuildHelper.GetPlayerGuildMemberInfo();
                if ((Singleton<CGuildSystem>.GetInstance().IsInNormalGuild() && (playerGuildMemberInfo != null)) && (((curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER) || (curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_MATCH)) || ((curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_ENTERTAINMENT) || (curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_REWARDMATCH))))
                {
                    gameObject.CustomSetActive(true);
                    COMDT_ACNT_INFO acntInfo = Singleton<BattleStatistic>.GetInstance().acntInfo;
                    if (acntInfo != null)
                    {
                        CUIEventScript component = gameObject.transform.FindChild("Content").GetComponent<CUIEventScript>();
                        stUIEventParams eventParams = new stUIEventParams {
                            tag = (int) acntInfo.dwGuildRankPoint
                        };
                        component.SetUIEvent(enUIEventType.Down, enUIEventID.PvPSettle_ShowRankPointTips, eventParams);
                        component.SetUIEvent(enUIEventType.HoldEnd, enUIEventID.PvPSettle_HideRankPointTips, eventParams);
                        component.SetUIEvent(enUIEventType.Click, enUIEventID.PvPSettle_HideRankPointTips, eventParams);
                        component.SetUIEvent(enUIEventType.DragEnd, enUIEventID.PvPSettle_HideRankPointTips, eventParams);
                        if (acntInfo.dwGuildRankPoint > Singleton<CSettleSystem>.instance.maxRankPoint)
                        {
                            LeanTween.value(storey.GuildCoinTxt.gameObject, new Action<float>(storey.<>m__5D), (float) Singleton<CSettleSystem>.instance.maxRankPoint, (float) acntInfo.dwGuildRankPoint, 2f);
                            obj3.CustomSetActive(true);
                        }
                        else
                        {
                            storey.GuildCoinTxt.text = playerGuildMemberInfo.RankInfo.maxRankPoint.ToString();
                            obj3.CustomSetActive(false);
                        }
                    }
                    else
                    {
                        obj3.CustomSetActive(false);
                        storey.GuildCoinTxt.text = "0";
                    }
                }
                else
                {
                    gameObject.CustomSetActive(false);
                }
            }
        }

        public static void SetRankPointTips(GameObject form, bool bShow, int rankPoint)
        {
            form.transform.FindChild("PanelA/Award/PvpRankTip").gameObject.CustomSetActive(bShow);
            string[] args = new string[] { rankPoint.ToString() };
            form.transform.FindChild("PanelA/Award/PvpRankTip/GuildCoinTxt").GetComponent<Text>().text = Singleton<CTextManager>.instance.GetText("PvP_Settle_Tips_3", args);
        }

        public static void SetSettleData(CUIFormScript formScript, GameObject root, bool bWin)
        {
            CPlayerKDAStat playerKDAStat = Singleton<BattleLogic>.GetInstance().battleStat.m_playerKDAStat;
            SetTab(0, root);
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = playerKDAStat.GetEnumerator();
            int num = 1;
            int num2 = 1;
            int num3 = 0;
            int num4 = 0;
            uint hostPlayerID = 0;
            COM_PLAYERCAMP playerCamp = COM_PLAYERCAMP.COM_PLAYERCAMP_MID;
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                if (current.Value.IsHost)
                {
                    KeyValuePair<uint, PlayerKDA> pair2 = enumerator.Current;
                    playerCamp = pair2.Value.PlayerCamp;
                    KeyValuePair<uint, PlayerKDA> pair3 = enumerator.Current;
                    hostPlayerID = pair3.Value.PlayerId;
                    break;
                }
            }
            bool flag = false;
            bool flag2 = false;
            if (bWin && (playerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_1))
            {
                flag = true;
                flag2 = false;
            }
            else if (bWin && (playerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_2))
            {
                flag = false;
                flag2 = true;
            }
            else if (!bWin && (playerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_1))
            {
                flag = false;
                flag2 = true;
            }
            else if (!bWin && (playerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_2))
            {
                flag = true;
                flag2 = false;
            }
            uint mvpPlayer = Singleton<BattleStatistic>.instance.GetMvpPlayer(COM_PLAYERCAMP.COM_PLAYERCAMP_1, flag);
            uint mvpPlayerId = Singleton<BattleStatistic>.instance.GetMvpPlayer(COM_PLAYERCAMP.COM_PLAYERCAMP_2, flag2);
            enumerator = playerKDAStat.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> pair4 = enumerator.Current;
                if (pair4.Value.PlayerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_1)
                {
                    GameObject gameObject = root.transform.Find("PanelB/Left_Player" + num).gameObject;
                    GameObject obj3 = root.transform.Find("PanelB/StatCon/Left_Player" + num).gameObject;
                    gameObject.CustomSetActive(true);
                    obj3.CustomSetActive(true);
                    KeyValuePair<uint, PlayerKDA> pair5 = enumerator.Current;
                    SetPlayerStat(formScript, gameObject, obj3, pair5.Value, hostPlayerID, mvpPlayer, flag);
                    KeyValuePair<uint, PlayerKDA> pair6 = enumerator.Current;
                    num3 += pair6.Value.numKill;
                    num++;
                }
                KeyValuePair<uint, PlayerKDA> pair7 = enumerator.Current;
                if (pair7.Value.PlayerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_2)
                {
                    GameObject obj4 = root.transform.Find("PanelB/Right_Player" + num2).gameObject;
                    GameObject obj5 = root.transform.Find("PanelB/StatCon/Right_Player" + num2).gameObject;
                    obj4.CustomSetActive(true);
                    obj5.CustomSetActive(true);
                    KeyValuePair<uint, PlayerKDA> pair8 = enumerator.Current;
                    SetPlayerStat(formScript, obj4, obj5, pair8.Value, hostPlayerID, mvpPlayerId, flag2);
                    KeyValuePair<uint, PlayerKDA> pair9 = enumerator.Current;
                    num4 += pair9.Value.numKill;
                    num2++;
                }
            }
            root.transform.Find("PanelB/Txt_LeftKill").gameObject.GetComponent<Text>().text = num3.ToString();
            root.transform.Find("PanelB/Txt_RightKill").gameObject.GetComponent<Text>().text = num4.ToString();
            for (int i = num; i <= 3; i++)
            {
                GameObject obj6 = root.transform.Find("PanelB/Left_Player" + i).gameObject;
                GameObject obj7 = root.transform.Find("PanelB/StatCon/Left_Player" + i).gameObject;
                obj6.CustomSetActive(false);
                obj7.CustomSetActive(false);
            }
            for (int j = num2; j <= 3; j++)
            {
                GameObject obj8 = root.transform.Find("PanelB/Right_Player" + j).gameObject;
                GameObject obj9 = root.transform.Find("PanelB/StatCon/Right_Player" + j).gameObject;
                obj8.CustomSetActive(false);
                obj9.CustomSetActive(false);
            }
            SetWin(root, bWin);
            CLevelCfgLogicManager.SetPvpLevelInfo(root);
            SetExpInfo(root, formScript);
            SetCoinInfo(root);
            DoCoinAndExpTween();
            SetProficiency(root);
            SetRankPoint(root);
            SetRankPointTips(root, false, 0);
            Set3DHero(root);
            _continueBtn = root.transform.Find("PanelA/Btn_Continue").gameObject;
            _continueBtn.CustomSetActive(true);
            root.transform.Find("PanelB/ButtonGrid/Button_Share").gameObject.CustomSetActive((bWin && !CSysDynamicBlock.bSocialBlocked) && !MonoSingleton<ShareSys>.GetInstance().m_bHide);
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if (curLvelContext != null)
            {
                if (curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER)
                {
                    root.transform.Find("PanelB/ButtonGrid/Btn_LadderBack").gameObject.CustomSetActive(true);
                    root.transform.Find("PanelB/ButtonGrid/Btn_Back").gameObject.CustomSetActive(false);
                }
                else
                {
                    root.transform.Find("PanelB/ButtonGrid/Btn_LadderBack").gameObject.CustomSetActive(false);
                    root.transform.Find("PanelB/ButtonGrid/Btn_Again").gameObject.CustomSetActive(Singleton<CMatchingSystem>.instance.cacheMathingInfo.CanGameAgain);
                }
            }
        }

        public static void SetTab(int index, GameObject root)
        {
            if (index == 0)
            {
                Utility.FindChild(root, "PanelA").CustomSetActive(true);
                Utility.FindChild(root, "PanelB").CustomSetActive(false);
            }
            else if (index == 1)
            {
                DoCoinTweenEnd();
                DoExpTweenEnd();
                Utility.FindChild(root, "PanelA").CustomSetActive(false);
                Utility.FindChild(root, "PanelB").CustomSetActive(true);
                MonoSingleton<NewbieGuideManager>.GetInstance().CheckTriggerTime(NewbieGuideTriggerTimeType.pvpFin, new uint[0]);
            }
        }

        private static void SetWin(GameObject root, bool bWin)
        {
            Utility.FindChild(root, "PanelA/WinOrLoseTitle/win").CustomSetActive(bWin);
            Utility.FindChild(root, "PanelA/WinOrLoseTitle/lose").CustomSetActive(!bWin);
            Utility.FindChild(root, "PanelB/WinOrLoseTitle/win").CustomSetActive(bWin);
            Utility.FindChild(root, "PanelB/WinOrLoseTitle/lose").CustomSetActive(!bWin);
        }

        public static void ShowData(CUIFormScript form)
        {
            form.gameObject.transform.Find("PanelB/StatCon").gameObject.CustomSetActive(true);
        }

        [CompilerGenerated]
        private sealed class <SetRankPoint>c__AnonStorey52
        {
            internal Text GuildCoinTxt;

            internal void <>m__5D(float value)
            {
                if (this.GuildCoinTxt.gameObject != null)
                {
                    this.GuildCoinTxt.text = value.ToString("N0");
                }
            }
        }
    }
}

