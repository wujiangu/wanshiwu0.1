namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.GameKernal;
    using Assets.Scripts.GameSystem;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    internal class CBattleGuideManager : Singleton<CBattleGuideManager>
    {
        private bool _PauseGame;
        [CompilerGenerated]
        private static CTimer.OnTimeUpHandler <>f__am$cache13;
        [CompilerGenerated]
        private static CTimer.OnTimeUpHandler <>f__am$cache14;
        public static readonly uint AdvanceGuideLevelID = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x37).dwConfValue;
        public bool bReEntry;
        public bool bTrainingAdv;
        private CUIFormScript curOpenForm;
        private string[] GuideFormPathList = new string[0x1d];
        public static readonly uint GuideLevelID1v1 = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x74).dwConfValue;
        public static readonly uint GuideLevelID3v3 = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x75).dwConfValue;
        public static readonly uint GuideLevelID5v5 = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x76).dwConfValue;
        public static readonly uint GuideLevelIDCasting = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x77).dwConfValue;
        public static readonly uint GuideLevelIDJungle = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x79).dwConfValue;
        public const string HALL_INTRO_FORM_PATH = "UGUI/Form/System/Newbie/Form_IntroHall.prefab";
        private const string INTRO_33_FORM_PATH = "UGUI/Form/System/Newbie/Form_Intro_3V3.prefab";
        private bool m_bWin;
        private EBattleGuideFormType m_lastFormType;
        private Dictionary<object, int> m_pauseGameStack = new Dictionary<object, int>();
        public static bool ms_bOldPlayerFormOpened = false;
        public const string OLD_PLAYER_FIRST_FORM_PATH = "UGUI/Form/System/Newbie/Form_OldTip.prefab";
        public const string SETTLE_FORM_PATH = "UGUI/Form/System/Newbie/Form_NewbieSettle.prefab";
        public static int TIME_OUT = 0x7530;
        private COMDT_REWARD_DETAIL TrainLevelReward;
        public static string TUTORIAL_END_FORM_PATH = "UGUI/Form/System/Newbie/Form_End_Tutorial.prefab";
        public static readonly uint Warm1v1SpecialLevelId = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x93).dwConfValue;

        private void CloseFormShared(EBattleGuideFormType inFormType)
        {
            Singleton<CTimerManager>.GetInstance().RemoveTimer(new CTimer.OnTimeUpHandler(this.TimeOutDelegate));
            this.UpdateGamePausing(false);
            string formPath = this.GuideFormPathList[(int) inFormType];
            Singleton<CUIManager>.GetInstance().CloseForm(formPath);
            if (Singleton<GameStateCtrl>.instance.isBattleState)
            {
                if (inFormType == EBattleGuideFormType.SkillGesture2)
                {
                    List<GameObject> list = Singleton<BattleSkillHudControl>.GetInstance().QuerySkillButtons(SkillSlotType.SLOT_SKILL_2, false);
                    if (list.Count > 0)
                    {
                        GameObject obj2 = list[0];
                        this.showSkillButton(obj2.transform.FindChild("Present").gameObject, true);
                    }
                }
                else if (inFormType == EBattleGuideFormType.SkillGesture2Cancel)
                {
                    List<GameObject> list2 = Singleton<BattleSkillHudControl>.GetInstance().QuerySkillButtons(SkillSlotType.SLOT_SKILL_2, false);
                    if (list2.Count > 0)
                    {
                        GameObject obj3 = list2[0];
                        this.showSkillButton(obj3.transform.FindChild("Present").gameObject, true);
                    }
                }
                else if (inFormType == EBattleGuideFormType.SkillGesture3)
                {
                    List<GameObject> list3 = Singleton<BattleSkillHudControl>.GetInstance().QuerySkillButtons(SkillSlotType.SLOT_SKILL_3, false);
                    if (list3.Count > 0)
                    {
                        GameObject obj4 = list3[0];
                        this.showSkillButton(obj4.transform.FindChild("Present").gameObject, true);
                    }
                }
            }
        }

        private void CloseSettle()
        {
            Singleton<CUIManager>.GetInstance().CloseForm("UGUI/Form/System/Newbie/Form_NewbieSettle.prefab");
            Singleton<EventRouter>.instance.BroadCastEvent("CheckNewbieIntro");
        }

        private void EnterAdvanceGuide(CUIEvent uiEvent)
        {
            uint dwConfValue = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x90).dwConfValue;
            Singleton<LobbyLogic>.GetInstance().ReqStartGuideLevel((int) AdvanceGuideLevelID, dwConfValue);
        }

        public override void Init()
        {
            base.Init();
            for (int i = 0; i < this.GuideFormPathList.Length; i++)
            {
                string str = string.Format("Newbie/Form_{0}.prefab", ((EBattleGuideFormType) i).ToString("G"));
                this.GuideFormPathList[i] = "UGUI/Form/System/" + str;
            }
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_CloseIntroForm, new CUIEventManager.OnUIEventHandler(this.onCloseIntro));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_CloseIntroForm2, new CUIEventManager.OnUIEventHandler(this.onCloseIntro2));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_CloseGestureGuide, new CUIEventManager.OnUIEventHandler(this.onCloseGesture));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_CloseJoyStickGuide, new CUIEventManager.OnUIEventHandler(this.onCloseJoyStick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_CloseSettle, new CUIEventManager.OnUIEventHandler(this.onCloseSettle));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_ConfirmAdvanceGuide, new CUIEventManager.OnUIEventHandler(this.EnterAdvanceGuide));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_RejectAdvanceGuide, new CUIEventManager.OnUIEventHandler(this.RejectAdvanceGuide));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_CloseTyrantAlert, new CUIEventManager.OnUIEventHandler(this.onCloseTyrantAlert));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_CloseTyrantTip, new CUIEventManager.OnUIEventHandler(this.onCloseTyrantTip));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_CloseTyrantTip2, new CUIEventManager.OnUIEventHandler(this.onCloseTyrantTip2));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_CloseSkillGesture, new CUIEventManager.OnUIEventHandler(this.onCloseSkillGesture));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_OldPlayerFirstFormClose, new CUIEventManager.OnUIEventHandler(this.onOldPlayerFirstFormClose));
        }

        private void onCloseAwardTips(CUIEvent evt)
        {
            MonoSingleton<NewbieGuideManager>.GetInstance().CheckTriggerTime(NewbieGuideTriggerTimeType.onEntryTrainLevelEntry, new uint[0]);
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_Guide_CloseTrainLevel_Award, new CUIEventManager.OnUIEventHandler(this.onCloseAwardTips));
        }

        private void onCloseGesture(CUIEvent uiEvent)
        {
            this.CloseFormShared(EBattleGuideFormType.Gesture);
        }

        private void onCloseIntro(CUIEvent uiEvent)
        {
            this.CloseFormShared(EBattleGuideFormType.Intro);
        }

        private void onCloseIntro2(CUIEvent uiEvent)
        {
            this.CloseFormShared(EBattleGuideFormType.Intro_2);
        }

        private void onCloseJoyStick(CUIEvent uiEvent)
        {
            if (Singleton<CUIManager>.GetInstance().GetForm(this.QueryFormPath(EBattleGuideFormType.JoyStick)) != null)
            {
                this.CloseFormShared(EBattleGuideFormType.JoyStick);
            }
        }

        private void onCloseSettle(CUIEvent uiEvent)
        {
            DynamicShadow.DisableAllDynamicShadows();
            uint dwConfValue = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x36).dwConfValue;
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            DebugHelper.Assert(curLvelContext != null, "Battle Level Context is NULL!!");
            if ((dwConfValue != 0) && (curLvelContext.iLevelID != AdvanceGuideLevelID))
            {
                Singleton<CUIManager>.instance.OpenForm("UGUI/Form/System/Newbie/Form_Intro_3V3.prefab", false, true);
            }
            else
            {
                this.CloseSettle();
            }
        }

        private void onCloseSkillGesture(CUIEvent uiEvent)
        {
            if (uiEvent != null)
            {
                SkillSlotType skillSlotType = uiEvent.m_eventParams.m_skillSlotType;
                this.CloseFormShared(this.m_lastFormType);
                Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
                if (hostPlayer != null)
                {
                    PoolObjHandle<ActorRoot> captain = hostPlayer.Captain;
                    if ((captain != 0) && (captain.handle.EffectControl != null))
                    {
                        captain.handle.EffectControl.EndSkillGestureEffect();
                    }
                }
                List<GameObject> list = Singleton<BattleSkillHudControl>.GetInstance().QuerySkillButtons(skillSlotType, false);
                if ((list != null) && (list.Count > 0))
                {
                    GameObject obj2 = list[0];
                    if (obj2 != null)
                    {
                        Transform transform = obj2.transform.FindChild("Present");
                        DebugHelper.Assert(transform != null);
                        if (transform != null)
                        {
                            transform.gameObject.CustomSetActive(true);
                        }
                    }
                }
            }
        }

        private void onCloseTyrantAlert(CUIEvent uiEvent)
        {
            this.CloseFormShared(EBattleGuideFormType.BaojunAlert);
        }

        private void onCloseTyrantTip(CUIEvent uiEvent)
        {
            this.CloseFormShared(EBattleGuideFormType.BaojunTips);
        }

        private void onCloseTyrantTip2(CUIEvent uiEvent)
        {
            this.CloseFormShared(EBattleGuideFormType.BaojunTips2);
        }

        private void onFightPrepare(ref DefaultGameEventParam prm)
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            DebugHelper.Assert(curLvelContext != null, "Battle Level Context is NULL!!");
            if ((curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_GUIDE) && (curLvelContext.iLevelID == AdvanceGuideLevelID))
            {
                if (<>f__am$cache13 == null)
                {
                    <>f__am$cache13 = seq => MonoSingleton<NewbieGuideManager>.GetInstance().CheckTriggerTime(NewbieGuideTriggerTimeType.newbiePvPTalent, new uint[0]);
                }
                Singleton<CTimerManager>.GetInstance().AddTimer(0x7d0, 1, <>f__am$cache13);
            }
        }

        private void onOldPlayerFirstFormClose(CUIEvent uiEvent)
        {
            if (uiEvent != null)
            {
                Singleton<CUIManager>.instance.CloseForm(uiEvent.m_srcFormScript);
            }
        }

        private void OnSkillGestEffTimer(int inTimeSeq)
        {
            PoolObjHandle<ActorRoot> captain = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer().Captain;
            SkillSlotType curSkillSlotType = Singleton<CBattleSystem>.GetInstance().GetCurSkillSlotType();
            if (this.m_lastFormType == EBattleGuideFormType.SkillGesture2)
            {
                if (curSkillSlotType == SkillSlotType.SLOT_SKILL_2)
                {
                    return;
                }
                if ((captain != 0) && (captain.handle.EffectControl != null))
                {
                    captain.handle.EffectControl.StartSkillGestureEffect();
                }
            }
            else if (this.m_lastFormType == EBattleGuideFormType.SkillGesture2Cancel)
            {
                if (curSkillSlotType == SkillSlotType.SLOT_SKILL_2)
                {
                    return;
                }
                if ((captain != 0) && (captain.handle.EffectControl != null))
                {
                    captain.handle.EffectControl.StartSkillGestureEffectCancel();
                }
            }
            else if (this.m_lastFormType == EBattleGuideFormType.SkillGesture3)
            {
                if (curSkillSlotType == SkillSlotType.SLOT_SKILL_3)
                {
                    return;
                }
                if ((captain != 0) && (captain.handle.EffectControl != null))
                {
                    captain.handle.EffectControl.StartSkillGestureEffect3();
                }
            }
            Singleton<CTimerManager>.instance.RemoveTimer(inTimeSeq);
        }

        public void OpenFormShared(EBattleGuideFormType inFormType, int delayTime = 0)
        {
            if ((inFormType != EBattleGuideFormType.Invalid) && (inFormType != EBattleGuideFormType.Count))
            {
                this.curOpenForm = Singleton<CUIManager>.GetInstance().OpenForm(this.GuideFormPathList[(int) inFormType], false, true);
                if ((delayTime != 0) && (this.curOpenForm != null))
                {
                    Transform transform = this.curOpenForm.transform.FindChild("Panel_Interactable");
                    if (transform != null)
                    {
                        transform.gameObject.CustomSetActive(false);
                        Singleton<CTimerManager>.GetInstance().AddTimer(delayTime, 1, new CTimer.OnTimeUpHandler(this.ShowInteractable));
                    }
                }
                Singleton<CTimerManager>.GetInstance().AddTimer(TIME_OUT, 1, new CTimer.OnTimeUpHandler(this.TimeOutDelegate));
                switch (inFormType)
                {
                    case EBattleGuideFormType.BigMapGuide:
                    case EBattleGuideFormType.SelectModeGuide:
                    case EBattleGuideFormType.BaojunAlert:
                        break;

                    default:
                        this.UpdateGamePausing(true);
                        break;
                }
                if (inFormType == EBattleGuideFormType.SkillGesture2)
                {
                    List<GameObject> list = Singleton<BattleSkillHudControl>.GetInstance().QuerySkillButtons(SkillSlotType.SLOT_SKILL_2, false);
                    if (list.Count > 0)
                    {
                        GameObject obj2 = list[0];
                        this.showSkillButton(obj2.transform.FindChild("Present").gameObject, false);
                        Singleton<CTimerManager>.instance.AddTimer(500, 1, new CTimer.OnTimeUpHandler(this.OnSkillGestEffTimer), false);
                    }
                }
                else if (inFormType == EBattleGuideFormType.SkillGesture2Cancel)
                {
                    List<GameObject> list2 = Singleton<BattleSkillHudControl>.GetInstance().QuerySkillButtons(SkillSlotType.SLOT_SKILL_2, false);
                    if (list2.Count > 0)
                    {
                        GameObject obj3 = list2[0];
                        this.showSkillButton(obj3.transform.FindChild("Present").gameObject, false);
                        Singleton<CTimerManager>.instance.AddTimer(500, 1, new CTimer.OnTimeUpHandler(this.OnSkillGestEffTimer), false);
                        GameObject obj4 = GameObject.Find("Design");
                        if (obj4 != null)
                        {
                            GlobalTrigger component = obj4.GetComponent<GlobalTrigger>();
                            if (component != null)
                            {
                                component.BindSkillCancelListener();
                            }
                        }
                    }
                }
                else if (inFormType == EBattleGuideFormType.SkillGesture3)
                {
                    List<GameObject> list3 = Singleton<BattleSkillHudControl>.GetInstance().QuerySkillButtons(SkillSlotType.SLOT_SKILL_3, false);
                    if (list3.Count > 0)
                    {
                        GameObject obj5 = list3[0];
                        this.showSkillButton(obj5.transform.FindChild("Present").gameObject, false);
                        Singleton<CTimerManager>.instance.AddTimer(500, 1, new CTimer.OnTimeUpHandler(this.OnSkillGestEffTimer), false);
                        GameObject obj6 = GameObject.Find("Design");
                        if (obj6 != null)
                        {
                            GlobalTrigger trigger2 = obj6.GetComponent<GlobalTrigger>();
                            if (trigger2 != null)
                            {
                                trigger2.UnbindSkillCancelListener();
                            }
                        }
                    }
                }
                this.m_lastFormType = inFormType;
            }
        }

        private void OpenNewbieSettle()
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                if (curLvelContext.iLevelID == GuideLevelID1v1)
                {
                    if (!this.bReEntry)
                    {
                        uint firstHeroId = masterRoleInfo.GetFirstHeroId();
                        this.ShowNewbiePassedHero(firstHeroId, true);
                        if (<>f__am$cache14 == null)
                        {
                            <>f__am$cache14 = seq => Singleton<CNewbieAchieveSys>.GetInstance().ShowAchieve(enNewbieAchieve.COM_ACNT_CLIENT_BITS_TYPE_GET_HERO);
                        }
                        Singleton<CTimerManager>.GetInstance().AddTimer(0x3e8, 1, <>f__am$cache14);
                    }
                }
                else if (curLvelContext.iLevelID == GuideLevelID5v5)
                {
                    uint heroId = masterRoleInfo.GetGuideLevel2FadeHeroId();
                    this.ShowNewbiePassedHero(heroId, false);
                }
            }
        }

        public void OpenOldPlayerFirstForm()
        {
            if (!ms_bOldPlayerFormOpened && (Singleton<CUIManager>.instance.GetForm("UGUI/Form/System/Newbie/Form_OldTip.prefab") == null))
            {
                Singleton<CUIManager>.instance.OpenForm("UGUI/Form/System/Newbie/Form_OldTip.prefab", false, true);
                ms_bOldPlayerFormOpened = true;
            }
        }

        public void OpenSettle()
        {
            this.UpdateGamePausing(false);
            if (!this.bReEntry)
            {
                this.OpenNewbieSettle();
            }
            else
            {
                this.OpenTrainLevelSettle();
            }
        }

        private void OpenTrainLevelSettle()
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            DebugHelper.Assert(curLvelContext != null, "Battle Level Context is NULL!!");
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                Singleton<CMatchingSystem>.GetInstance().OpenPvPEntry(enPvPEntryFormWidget.GuideBtnGroup);
                if ((this.TrainLevelReward != null) && (this.TrainLevelReward.bNum != 0))
                {
                    ListView<CUseable> useableListFromReward = CUseableManager.GetUseableListFromReward(this.TrainLevelReward);
                    Singleton<CUIManager>.GetInstance().OpenAwardTip(LinqS.ToArray<CUseable>(useableListFromReward), Singleton<CTextManager>.GetInstance().GetText("TrainLevel_Settel_Tile0"), true, enUIEventID.Battle_Guide_CloseTrainLevel_Award, false, false, "Form_Award");
                    Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_Guide_CloseTrainLevel_Award, new CUIEventManager.OnUIEventHandler(this.onCloseAwardTips));
                }
                if (curLvelContext.iLevelID == GuideLevelIDCasting)
                {
                    masterRoleInfo.SetNewbieAchieve(0x76, true, true);
                }
            }
        }

        public void PauseGame(object sender, bool bEffectTimeScale = true)
        {
            if (sender != null)
            {
                if (this.m_pauseGameStack.ContainsKey(sender))
                {
                    Dictionary<object, int> dictionary;
                    object obj2;
                    int num = dictionary[obj2];
                    (dictionary = this.m_pauseGameStack)[obj2 = sender] = num + 1;
                }
                else
                {
                    this.m_pauseGameStack.Add(sender, 1);
                }
                this.UpdateGamePause(bEffectTimeScale);
            }
        }

        public string QueryFormPath(EBattleGuideFormType inFormType)
        {
            return this.GuideFormPathList[(int) inFormType];
        }

        private void RejectAdvanceGuide(CUIEvent uiEvent)
        {
            this.CloseSettle();
        }

        public void resetPause()
        {
            this.m_pauseGameStack.Clear();
            this.bPauseGame = false;
            Singleton<FrameSynchr>.instance.bEscape = this.bPauseGame;
            Time.timeScale = 1f;
        }

        public void ResumeGame(object sender)
        {
            if ((sender != null) && this.m_pauseGameStack.ContainsKey(sender))
            {
                Dictionary<object, int> dictionary;
                object obj2;
                int num = dictionary[obj2];
                (dictionary = this.m_pauseGameStack)[obj2 = sender] = --num;
                if (num == 0)
                {
                    this.m_pauseGameStack.Remove(sender);
                }
                this.UpdateGamePause(true);
            }
        }

        public void ShowBuyEuipPanel(bool bShow)
        {
            Singleton<CUIManager>.GetInstance().GetForm(CBattleSystem.s_battleUIForm).GetWidget(0x2d).CustomSetActive(bShow);
        }

        private void ShowInteractable(int timerSequence)
        {
            Singleton<CTimerManager>.instance.RemoveTimer(new CTimer.OnTimeUpHandler(this.ShowInteractable));
            if (this.curOpenForm != null)
            {
                Transform transform = this.curOpenForm.transform.FindChild("Panel_Interactable");
                if (transform != null)
                {
                    transform.gameObject.CustomSetActive(true);
                    CUIAnimatorScript component = transform.GetComponent<CUIAnimatorScript>();
                    if (component != null)
                    {
                        component.PlayAnimator("Interactable_Enabled");
                    }
                }
            }
        }

        private void ShowNewbiePassedHero(uint heroId, bool bImage1)
        {
            CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm("UGUI/Form/System/Newbie/Form_NewbieSettle.prefab", false, true);
            if (script != null)
            {
                DynamicShadow.EnableDynamicShow(script.gameObject, true);
            }
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if ((masterRoleInfo != null) && (script != null))
            {
                int heroWearSkinId = (int) masterRoleInfo.GetHeroWearSkinId(heroId);
                string objectName = CUICommonSystem.GetHeroPrefabPath(heroId, heroWearSkinId, true).ObjectName;
                GameObject model = script.transform.Find("3DImage").gameObject.GetComponent<CUI3DImageScript>().AddGameObject(objectName, false, false);
                CHeroAnimaSystem instance = Singleton<CHeroAnimaSystem>.GetInstance();
                instance.Set3DModel(model);
                instance.InitAnimatList();
                instance.InitAnimatSoundList(heroId, (uint) heroWearSkinId);
                instance.OnModePlayAnima("Cheer");
                Transform transform = script.transform.FindChild("Panel_NewHero/MaskBlack/Text");
                if (transform != null)
                {
                    Text component = transform.GetComponent<Text>();
                    if (component != null)
                    {
                        ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey(heroId);
                        if (dataByKey != null)
                        {
                            component.text = StringHelper.UTF8BytesToString(ref dataByKey.szName);
                        }
                    }
                }
                if (bImage1)
                {
                    Transform transform2 = script.transform.FindChild("Panel_NewHero/Image1");
                    if (transform2 != null)
                    {
                        transform2.gameObject.CustomSetActive(true);
                    }
                }
                else
                {
                    Transform transform3 = script.transform.FindChild("Panel_NewHero/Image2");
                    if (transform3 != null)
                    {
                        transform3.gameObject.CustomSetActive(true);
                    }
                }
            }
        }

        private void showSkillButton(GameObject skillPresent, bool bShow)
        {
            CanvasGroup component = null;
            component = skillPresent.GetComponent<CanvasGroup>();
            if (component == null)
            {
                component = skillPresent.AddComponent<CanvasGroup>();
            }
            if (bShow)
            {
                component.alpha = 1f;
            }
            else
            {
                component.alpha = 0f;
            }
        }

        public void StartSettle(COMDT_SETTLE_RESULT_DETAIL detail)
        {
            this.TrainLevelReward = detail.stReward;
            this.m_bWin = detail.stGameInfo.bGameResult == 1;
            Singleton<CBattleSystem>.GetInstance().ShowWinLosePanel(this.m_bWin);
        }

        public void TimeOutDelegate(int timerSequence)
        {
            Singleton<CBattleGuideManager>.GetInstance().CloseFormShared(this.m_lastFormType);
        }

        public override void UnInit()
        {
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_CloseIntroForm, new CUIEventManager.OnUIEventHandler(this.onCloseIntro));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_CloseIntroForm2, new CUIEventManager.OnUIEventHandler(this.onCloseIntro2));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_CloseGestureGuide, new CUIEventManager.OnUIEventHandler(this.onCloseGesture));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_CloseJoyStickGuide, new CUIEventManager.OnUIEventHandler(this.onCloseJoyStick));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_CloseSettle, new CUIEventManager.OnUIEventHandler(this.onCloseSettle));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_ConfirmAdvanceGuide, new CUIEventManager.OnUIEventHandler(this.EnterAdvanceGuide));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_RejectAdvanceGuide, new CUIEventManager.OnUIEventHandler(this.RejectAdvanceGuide));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_CloseTyrantAlert, new CUIEventManager.OnUIEventHandler(this.onCloseTyrantAlert));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_CloseTyrantTip, new CUIEventManager.OnUIEventHandler(this.onCloseTyrantTip));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_CloseTyrantTip2, new CUIEventManager.OnUIEventHandler(this.onCloseTyrantTip2));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_CloseSkillGesture, new CUIEventManager.OnUIEventHandler(this.onCloseSkillGesture));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_OldPlayerFirstFormClose, new CUIEventManager.OnUIEventHandler(this.onOldPlayerFirstFormClose));
            base.UnInit();
        }

        private void UpdateGamePause(bool bEffectTimeScale)
        {
            this.bPauseGame = this.m_pauseGameStack.Count != 0;
            Singleton<FrameSynchr>.instance.bEscape = this.bPauseGame;
            if (bEffectTimeScale)
            {
                Time.timeScale = !this.bPauseGame ? ((float) 1) : ((float) 0);
            }
        }

        private void UpdateGamePausing(bool bPauseGame)
        {
            if (bPauseGame)
            {
                this.PauseGame(this, false);
                Singleton<CUIParticleSystem>.GetInstance().ClearAll(true);
            }
            else
            {
                this.ResumeGame(this);
            }
        }

        public bool bPauseGame
        {
            get
            {
                return this._PauseGame;
            }
            private set
            {
                if (value)
                {
                    Singleton<GameInput>.instance.StopInput();
                }
                this._PauseGame = value;
            }
        }

        public enum EBattleGuideFormType
        {
            Invalid,
            Intro,
            Gesture,
            JoyStick,
            Intro_2,
            BaojunAlert,
            BaojunTips,
            BaojunTips2,
            SkillGesture2,
            SkillGesture3,
            SkillGesture2Cancel,
            BigSkill,
            BlueBuff,
            Duke,
            Grass,
            Heal,
            Intro_5V5,
            Intro_Jungle,
            Intro_LunPan,
            Map_01,
            Map_02,
            Map_03,
            RedBuff,
            Tower,
            XiaoLong,
            Map_BlueBuff,
            Map_RedBuff,
            BigMapGuide,
            SelectModeGuide,
            Count
        }
    }
}

