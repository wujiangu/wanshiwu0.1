namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.GameLogic.GameKernal;
    using Assets.Scripts.UI;
    using ResData;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class TalentView
    {
        private PoolObjHandle<ActorRoot> m_heroCaption;
        private List<PoolObjHandle<ActorRoot>> m_heroList = new List<PoolObjHandle<ActorRoot>>();
        private ListView<HeroTalentViewInfo> m_heroTalentViewList;
        private GameObject m_root;
        private ListView<ResTalentRule> m_talentRuleList;
        public static string s_TalentFormPath = "UGUI/Form/Battle/Form_Talent";

        public void AutoLearnTalent(PoolObjHandle<ActorRoot> hero, int level)
        {
            if (hero.handle.ActorAgent.IsAutoAI())
            {
                int bID = 0;
                bool flag = false;
                int num2 = 0;
                ResTalentLib lib = null;
                for (int i = 0; i < this.m_talentRuleList.Count; i++)
                {
                    if (this.m_talentRuleList[i].bSoulRequestValue == level)
                    {
                        bID = this.m_talentRuleList[i].bID;
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    ResTalentHero dataByKey = GameDataMgr.talentHero.GetDataByKey((uint) hero.handle.TheActorMeta.ConfigId);
                    if (dataByKey != null)
                    {
                        num2 = FrameRandom.Random(2);
                        if (bID == 2)
                        {
                            lib = GameDataMgr.talentLib.GetDataByKey(dataByKey.astTalentList[bID - 1].dwLvl1ID);
                        }
                        else if ((bID >= 1) && (bID <= dataByKey.astTalentList.Length))
                        {
                            if (num2 == 0)
                            {
                                lib = GameDataMgr.talentLib.GetDataByKey(dataByKey.astTalentList[bID - 1].dwLvl1ID);
                            }
                            else
                            {
                                lib = GameDataMgr.talentLib.GetDataByKey(dataByKey.astTalentList[bID - 1].dwLvl2ID);
                            }
                        }
                        if (lib != null)
                        {
                            this.DirectLearnTalentCommand(hero, lib.dwID, (sbyte) (bID - 1));
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            this.m_root = null;
            this.m_heroCaption.Release();
            this.m_heroList.Clear();
            this.m_talentRuleList.Clear();
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int>("HeroSoulLevelChange", new Action<PoolObjHandle<ActorRoot>, int>(this, (IntPtr) this.OnHeroSoulLvlChange));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, sbyte, uint>("HeroLearnTalent", new Action<PoolObjHandle<ActorRoot>, sbyte, uint>(this, (IntPtr) this.OnHeroLearnTalent));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_CaptainSwitch, new RefAction<DefaultGameEventParam>(this.OnCaptainSwitched));
            this.Hide();
        }

        private void DirectLearnTalentCommand(PoolObjHandle<ActorRoot> hero, uint talentID, sbyte talentLevelIndex)
        {
            FrameCommand<LearnTalentCommand> cmd = FrameCommandFactory.CreateFrameCommand<LearnTalentCommand>();
            Player ownerPlayer = ActorHelper.GetOwnerPlayer(ref hero);
            cmd.playerID = ownerPlayer.PlayerId;
            cmd.cmdData.TalentLevelIndex = talentLevelIndex;
            cmd.cmdData.TalentID = talentID;
            cmd.cmdData.HeroObjID = hero.handle.ObjID;
            hero.handle.ActorControl.CmdCommonLearnTalent(cmd);
        }

        public static HeroTalentViewInfo GetHeroTalentViewInfo(uint heroCfgID)
        {
            HeroTalentViewInfo info = new HeroTalentViewInfo {
                m_talentLevel = 0,
                m_learnTalentList = new uint[5]
            };
            ResTalentHero dataByKey = GameDataMgr.talentHero.GetDataByKey(heroCfgID);
            if (dataByKey != null)
            {
                info.m_heroTalentLevelInfoList = new ListView<HeroTalentLevelInfo>();
                for (int i = 0; i < dataByKey.astTalentList.Length; i++)
                {
                    HeroTalentLevelInfo info2 = new HeroTalentLevelInfo();
                    InitHeroTalentLevelInfo(info2, dataByKey.astTalentList[i].dwLvl1ID, dataByKey.astTalentList[i].dwLvl2ID, dataByKey.astTalentList[i].dwLvl3ID, dataByKey.astTalentList[i]);
                    info.m_heroTalentLevelInfoList.Add(info2);
                }
            }
            return info;
        }

        public HeroTalentViewInfo GetHeroTalentViewInfo(PoolObjHandle<ActorRoot> heroActor)
        {
            for (int i = 0; i < this.m_heroTalentViewList.Count; i++)
            {
                if (this.m_heroTalentViewList[i].m_hero == heroActor)
                {
                    return this.m_heroTalentViewList[i];
                }
            }
            return null;
        }

        public void Hide()
        {
            Singleton<CUIManager>.GetInstance().CloseForm(s_TalentFormPath);
        }

        public void Init(GameObject obj, PoolObjHandle<ActorRoot> actorRoot, ReadonlyContext<PoolObjHandle<ActorRoot>> acotrList)
        {
            this.m_root = obj;
            this.m_heroCaption = actorRoot;
            this.m_heroList.Clear();
            for (int i = 0; i < acotrList.Count; i++)
            {
                this.m_heroList.Add(acotrList[i]);
            }
            this.m_talentRuleList = new ListView<ResTalentRule>();
            GameDataMgr.talentRule.Accept(x => this.m_talentRuleList.Add(x));
            this.m_heroTalentViewList = new ListView<HeroTalentViewInfo>();
            for (int j = 0; j < this.m_heroList.Count; j++)
            {
                this.InitResData(this.m_heroList[j]);
            }
            if (this.m_heroTalentViewList.Count != 0)
            {
                Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int>("HeroSoulLevelChange", new Action<PoolObjHandle<ActorRoot>, int>(this, (IntPtr) this.OnHeroSoulLvlChange));
                Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, sbyte, uint>("HeroLearnTalent", new Action<PoolObjHandle<ActorRoot>, sbyte, uint>(this, (IntPtr) this.OnHeroLearnTalent));
                Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_CaptainSwitch, new RefAction<DefaultGameEventParam>(this.OnCaptainSwitched));
            }
        }

        public static void InitHeroTalentLevelInfo(HeroTalentLevelInfo info, uint skill1, uint skill2, uint skill3, RESDT_TALENT_DETAIL talentDetail)
        {
            info.m_tarlentLibList = new ListView<ResTalentLib>();
            info.m_levelDetail = talentDetail;
            if (skill1 != 0)
            {
                info.m_tarlentLibList.Add(GameDataMgr.talentLib.GetDataByKey(skill1));
            }
            if (skill2 != 0)
            {
                info.m_tarlentLibList.Add(GameDataMgr.talentLib.GetDataByKey(skill2));
            }
            if (skill3 != 0)
            {
                info.m_tarlentLibList.Add(GameDataMgr.talentLib.GetDataByKey(skill3));
            }
        }

        public void InitResData(PoolObjHandle<ActorRoot> heroActor)
        {
            HeroTalentViewInfo heroTalentViewInfo = GetHeroTalentViewInfo((uint) heroActor.handle.TheActorMeta.ConfigId);
            heroTalentViewInfo.m_hero = heroActor;
            this.m_heroTalentViewList.Add(heroTalentViewInfo);
        }

        public static bool IsBuyTalent(uint heroID, int talentLevelIndex)
        {
            bool flag = false;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo == null)
            {
                return false;
            }
            CHeroInfo heroInfo = masterRoleInfo.GetHeroInfo(heroID, false);
            if (heroInfo == null)
            {
                return false;
            }
            if ((talentLevelIndex >= heroInfo.m_talentBuyList.Length) || (talentLevelIndex < 0))
            {
                return false;
            }
            if (heroInfo.m_talentBuyList[talentLevelIndex] == 1)
            {
                flag = true;
            }
            return flag;
        }

        public static bool IsHaveTalentBuyFunc(uint heroID)
        {
            bool flag = false;
            ResTalentHero dataByKey = GameDataMgr.talentHero.GetDataByKey(heroID);
            if (((dataByKey != null) && (dataByKey.astTalentList.Length > 0)) && (dataByKey.astTalentList[0].dwLvl3ID != 0))
            {
                flag = true;
            }
            return flag;
        }

        public static bool IsNeedBuy(int talentLevelIndex, int talentLibIndex)
        {
            bool flag = false;
            if ((talentLevelIndex != 1) && (talentLibIndex == 2))
            {
                return true;
            }
            if ((talentLevelIndex == 1) && (talentLibIndex == 1))
            {
                flag = true;
            }
            return flag;
        }

        public static bool IsTalentBelongHero(uint uiTalentId, uint uiHeroId)
        {
            ResTalentHero dataByKey = GameDataMgr.talentHero.GetDataByKey(uiHeroId);
            if (dataByKey == null)
            {
                return false;
            }
            for (int i = 0; i < dataByKey.astTalentList.Length; i++)
            {
                if (((dataByKey.astTalentList[i].dwLvl1ID == uiTalentId) || (dataByKey.astTalentList[i].dwLvl2ID == uiTalentId)) || (dataByKey.astTalentList[i].dwLvl3ID == uiTalentId))
                {
                    return true;
                }
            }
            return false;
        }

        private void OnCaptainSwitched(ref DefaultGameEventParam prm)
        {
            this.m_heroCaption = prm.src;
            this.Refresh();
        }

        public void OnHeroLearnTalent(PoolObjHandle<ActorRoot> hero, sbyte talentLevel, uint talentID)
        {
            HeroTalentViewInfo heroTalentViewInfo = this.GetHeroTalentViewInfo(hero);
            if (heroTalentViewInfo != null)
            {
                ResTalentLib dataByKey = GameDataMgr.talentLib.GetDataByKey(talentID);
                heroTalentViewInfo.m_learnTalentList[(int) talentLevel] = dataByKey.dwID;
                if (hero == this.m_heroCaption)
                {
                    this.Refresh();
                    if (dataByKey.bType == 3)
                    {
                        Singleton<CBattleSystem>.GetInstance().ResetSkillButtonManager(this.m_heroCaption);
                        Singleton<CBattleSystem>.GetInstance().m_skillButtonManager.SetButtonCDOver(SkillSlotType.SLOT_SKILL_3, false);
                    }
                    string str = StringHelper.UTF8BytesToString(ref dataByKey.szName);
                    string[] args = new string[] { str };
                    Singleton<CBattleSystem>.GetInstance().CreateOtherFloatText(enOtherFloatTextContent.LearnTalent, (Vector3) this.m_heroCaption.handle.location, args);
                    Singleton<CSoundManager>.GetInstance().PlayBattleSound("UI_Prompt_get_tianfu", null);
                }
                Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            }
        }

        public void OnHeroSoulLvlChange(PoolObjHandle<ActorRoot> hero, int level)
        {
            HeroTalentViewInfo heroTalentViewInfo = this.GetHeroTalentViewInfo(hero);
            if (((heroTalentViewInfo != null) && this.SetTalentLevel(heroTalentViewInfo, level)) && (hero == this.m_heroCaption))
            {
                this.Refresh();
                if ((this.m_root != null) && this.m_root.activeSelf)
                {
                    Singleton<CBattleSystem>.GetInstance().CreateOtherFloatText(enOtherFloatTextContent.OpenTalent, (Vector3) this.m_heroCaption.handle.location, new string[0]);
                }
                Singleton<CSoundManager>.GetInstance().PlayBattleSound("UI_Prompt_tianfu", null);
                TalentLevelChangeParam prm = new TalentLevelChangeParam {
                    src = hero,
                    SoulLevel = level,
                    TalentLevel = heroTalentViewInfo.m_talentLevel
                };
                Singleton<GameEventSys>.instance.SendEvent<TalentLevelChangeParam>(GameEventDef.Event_TalentLevelChange, ref prm);
            }
            this.AutoLearnTalent(hero, level);
        }

        public void OnTalent_BtnLearnClick(CUIEvent uiEvent)
        {
            stTalentEventParams talentParams = uiEvent.m_eventParams.talentParams;
            Singleton<CUIManager>.GetInstance().CloseForm(s_TalentFormPath);
            HeroTalentViewInfo heroTalentViewInfo = this.GetHeroTalentViewInfo(this.m_heroCaption);
            if ((heroTalentViewInfo != null) && (talentParams.talentInfo != null))
            {
                this.SendLearnTalentCommand(heroTalentViewInfo, talentParams.talentInfo.dwID, (sbyte) talentParams.talentLevelIndex);
            }
        }

        public void OnTalent_ItemClick(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_TalentFormPath);
            if (form != null)
            {
                CUIListScript[] scriptArray = new CUIListScript[] { form.gameObject.transform.Find("Panel/PanelLeft/List1").GetComponent<CUIListScript>(), form.gameObject.transform.Find("Panel/PanelLeft/List2").GetComponent<CUIListScript>(), form.gameObject.transform.Find("Panel/PanelLeft/List3").GetComponent<CUIListScript>(), form.gameObject.transform.Find("Panel/PanelLeft/List4").GetComponent<CUIListScript>(), form.gameObject.transform.Find("Panel/PanelLeft/List5").GetComponent<CUIListScript>() };
                CUIListScript srcWidgetBelongedListScript = uiEvent.m_srcWidgetBelongedListScript;
                stTalentEventParams talentParams = uiEvent.m_eventParams.talentParams;
                if ((uiEvent.m_srcWidgetIndexInBelongedList != -1) && (talentParams.talentInfo != null))
                {
                    for (int i = 0; i < scriptArray.Length; i++)
                    {
                        if (scriptArray[i] != srcWidgetBelongedListScript)
                        {
                            CUIListElementScript lastSelectedElement = scriptArray[i].GetLastSelectedElement();
                            if (lastSelectedElement != null)
                            {
                                lastSelectedElement.ChangeDisplay(false);
                            }
                            scriptArray[i].SelectElement(-1, true);
                        }
                    }
                    GameObject gameObject = form.gameObject.transform.Find("Panel/PanelRight").gameObject;
                    Image component = form.gameObject.transform.Find("Panel/PanelRight/talentCell/imgIcon").GetComponent<Image>();
                    Text text = form.gameObject.transform.Find("Panel/PanelRight/lblDesc").GetComponent<Text>();
                    Button btn = form.gameObject.transform.Find("Panel/PanelRight/btnLearn").GetComponent<Button>();
                    component.SetSprite(CUIUtility.s_Sprite_Dynamic_Talent_Dir + talentParams.talentInfo.dwIcon, form, true, false, false);
                    text.text = StringHelper.UTF8BytesToString(ref talentParams.talentInfo.szDesc);
                    if (!talentParams.isCanLearn && !talentParams.isHaveTalent)
                    {
                        text.text = Singleton<CTextManager>.instance.GetText("Talent_Buy_1");
                    }
                    btn.gameObject.CustomSetActive(true);
                    if (talentParams.isCanLearn)
                    {
                        CUICommonSystem.SetButtonEnable(btn, true, true, true);
                        btn.gameObject.transform.GetComponent<CUIEventScript>().SetUIEvent(enUIEventType.Click, enUIEventID.Talent_BtnLearnClick, uiEvent.m_eventParams);
                    }
                    else
                    {
                        CUICommonSystem.SetButtonEnable(btn, false, false, true);
                    }
                    gameObject.CustomSetActive(true);
                }
            }
        }

        public void Refresh()
        {
            if (Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo() != null)
            {
                HeroTalentViewInfo heroTalentViewInfo = this.GetHeroTalentViewInfo(this.m_heroCaption);
                if (heroTalentViewInfo != null)
                {
                    uint configId = (uint) heroTalentViewInfo.m_hero.handle.TheActorMeta.ConfigId;
                    List<stTalentEventParams> list = new List<stTalentEventParams>();
                    for (int i = heroTalentViewInfo.m_talentLevel - 1; i >= 0; i--)
                    {
                        if (heroTalentViewInfo.m_learnTalentList[i] == 0)
                        {
                            for (int k = 0; k < heroTalentViewInfo.m_heroTalentLevelInfoList[i].m_tarlentLibList.Count; k++)
                            {
                                if (heroTalentViewInfo.m_heroTalentLevelInfoList[i].m_tarlentLibList[k] == null)
                                {
                                    return;
                                }
                                stTalentEventParams item = new stTalentEventParams {
                                    talentLevelIndex = (byte) i,
                                    talentInfo = heroTalentViewInfo.m_heroTalentLevelInfoList[i].m_tarlentLibList[k],
                                    isCanLearn = true
                                };
                                if (!IsNeedBuy(i, k) || IsBuyTalent(configId, i))
                                {
                                    item.isHaveTalent = true;
                                    list.Add(item);
                                }
                            }
                            break;
                        }
                    }
                    GameObject gameObject = this.m_root.transform.Find("List1").gameObject;
                    GameObject[] objArray = new GameObject[] { gameObject.transform.Find("talentCell1").gameObject, gameObject.transform.Find("talentCell2").gameObject, gameObject.transform.Find("talentCell3").gameObject };
                    for (int j = 0; j < objArray.Length; j++)
                    {
                        GameObject obj3 = objArray[j];
                        if (j < list.Count)
                        {
                            obj3.CustomSetActive(true);
                            stTalentEventParams params2 = list[j];
                            Image component = obj3.transform.Find("imgIcon").GetComponent<Image>();
                            Text text = obj3.transform.Find("panelDesc/Text").GetComponent<Text>();
                            component.SetSprite(CUIUtility.s_Sprite_Dynamic_Talent_Dir + params2.talentInfo.dwIcon, Singleton<CBattleSystem>.GetInstance().m_FormScript, true, false, false);
                            text.text = StringHelper.UTF8BytesToString(ref params2.talentInfo.szDesc2);
                            CUIEventScript script = obj3.GetComponent<CUIEventScript>();
                            stUIEventParams eventParams = new stUIEventParams {
                                talentParams = params2
                            };
                            script.SetUIEvent(enUIEventType.Click, enUIEventID.Talent_BtnLearnClick, eventParams);
                        }
                        else
                        {
                            obj3.CustomSetActive(false);
                        }
                    }
                    if (list.Count > 0)
                    {
                        gameObject.gameObject.CustomSetActive(true);
                    }
                    else
                    {
                        gameObject.gameObject.CustomSetActive(false);
                    }
                    CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_TalentFormPath);
                    if (form != null)
                    {
                        CUIListScript[] scriptArray = new CUIListScript[] { form.gameObject.transform.Find("Panel/PanelLeft/List1").GetComponent<CUIListScript>(), form.gameObject.transform.Find("Panel/PanelLeft/List2").GetComponent<CUIListScript>(), form.gameObject.transform.Find("Panel/PanelLeft/List3").GetComponent<CUIListScript>(), form.gameObject.transform.Find("Panel/PanelLeft/List4").GetComponent<CUIListScript>(), form.gameObject.transform.Find("Panel/PanelLeft/List5").GetComponent<CUIListScript>() };
                        for (int m = 0; m < this.m_talentRuleList.Count; m++)
                        {
                            HeroTalentLevelInfo info3 = heroTalentViewInfo.m_heroTalentLevelInfoList[m];
                            uint num6 = heroTalentViewInfo.m_learnTalentList[m];
                            bool flag = heroTalentViewInfo.m_talentLevel > m;
                            CUIListScript script3 = scriptArray[m];
                            script3.SetElementAmount(info3.m_tarlentLibList.Count);
                            GameObject obj4 = script3.gameObject.transform.Find("Current").gameObject;
                            obj4.CustomSetActive(false);
                            for (int n = 0; n < info3.m_tarlentLibList.Count; n++)
                            {
                                GameObject obj5 = script3.GetElemenet(n).gameObject.transform.Find("talentCell").gameObject;
                                Image image = obj5.transform.Find("imgIcon").GetComponent<Image>();
                                Image image3 = obj5.transform.Find("lock").GetComponent<Image>();
                                Text text2 = obj5.transform.Find("lblName").GetComponent<Text>();
                                CanvasGroup group = obj5.GetComponent<CanvasGroup>();
                                CUIEventScript script4 = obj5.GetComponent<CUIEventScript>();
                                ResTalentLib lib = info3.m_tarlentLibList[n];
                                if (lib == null)
                                {
                                    return;
                                }
                                image.SetSprite(CUIUtility.s_Sprite_Dynamic_Talent_Dir + lib.dwIcon, form, true, false, false);
                                image.color = CUIUtility.s_Color_White;
                                text2.text = StringHelper.UTF8BytesToString(ref lib.szName);
                                stTalentEventParams params4 = new stTalentEventParams {
                                    talentLevelIndex = (byte) m,
                                    talentInfo = info3.m_tarlentLibList[n],
                                    isCanLearn = false
                                };
                                if (IsNeedBuy(m, n) && !IsBuyTalent(configId, m))
                                {
                                    params4.isHaveTalent = false;
                                    image3.gameObject.CustomSetActive(true);
                                }
                                else
                                {
                                    params4.isHaveTalent = true;
                                    image3.gameObject.CustomSetActive(false);
                                }
                                if (flag)
                                {
                                    if (num6 == 0)
                                    {
                                        if (params4.isHaveTalent)
                                        {
                                            params4.isCanLearn = true;
                                        }
                                        obj4.CustomSetActive(true);
                                    }
                                    else if (lib.dwID != num6)
                                    {
                                        group.alpha = 0.25f;
                                    }
                                    else
                                    {
                                        group.alpha = 1f;
                                    }
                                    image.color = CUIUtility.s_Color_White;
                                    image3.color = CUIUtility.s_Color_White;
                                    text2.color = CUIUtility.s_Color_White;
                                }
                                else
                                {
                                    image.color = CUIUtility.s_Color_GrayShader;
                                    image3.color = CUIUtility.s_Color_GrayShader;
                                    text2.color = CUIUtility.s_Color_Grey;
                                }
                                stUIEventParams params5 = new stUIEventParams {
                                    talentParams = params4
                                };
                                script4.SetUIEvent(enUIEventType.Click, enUIEventID.Talent_ItemClick, params5);
                            }
                        }
                    }
                }
            }
        }

        private void SendLearnTalentCommand(HeroTalentViewInfo viewInfo, uint talentID, sbyte talentLevelIndex)
        {
            FrameCommand<LearnTalentCommand> command = FrameCommandFactory.CreateFrameCommand<LearnTalentCommand>();
            command.cmdData.TalentLevelIndex = talentLevelIndex;
            command.cmdData.TalentID = talentID;
            command.cmdData.HeroObjID = viewInfo.m_hero.handle.ObjID;
            command.Send();
        }

        public bool SetTalentLevel(HeroTalentViewInfo viewInfo, int soulLevel)
        {
            bool flag = false;
            for (int i = 0; i < this.m_talentRuleList.Count; i++)
            {
                if (((soulLevel >= this.m_talentRuleList[i].bSoulRequestValue) && (viewInfo.m_talentLevel < this.m_talentRuleList[i].bID)) && (viewInfo.m_talentLevel != this.m_talentRuleList[i].bID))
                {
                    viewInfo.m_talentLevel = this.m_talentRuleList[i].bID;
                    flag = true;
                }
            }
            return flag;
        }

        public void Show()
        {
            Singleton<CUIManager>.GetInstance().OpenForm(s_TalentFormPath, false, true);
            this.Refresh();
        }
    }
}

