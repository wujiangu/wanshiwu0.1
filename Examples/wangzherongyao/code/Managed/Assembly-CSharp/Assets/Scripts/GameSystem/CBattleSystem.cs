namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.GameLogic.GameKernal;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class CBattleSystem : Singleton<CBattleSystem>
    {
        private CUIJoystickScript _joystick;
        private uint _lastFps;
        private BattleStatView battleStatView;
        private BattleTaskView battleTaskView;
        private GameObject BuffDesc;
        private BuffCDString BufStringProvider = new BuffCDString();
        private HeroHeadHud heroHeadHud;
        private KillNotify killNotifation;
        private Text m_AdTxt;
        private Text m_ApTxt;
        private BuffInfo[] m_ArrShowBuffSkill = new BuffInfo[5];
        public float m_AveBattleFPS;
        public CBattleEquipSystem m_battleEquipSystem;
        private CBattleFloatDigitManager m_battleFloatDigitManager;
        public float m_BattleFPSCount;
        private BattleMisc m_battleMisc;
        private bool m_bIsFirst = true;
        private bool m_bOpenMic;
        private bool m_bOpenSpeak;
        private GameObject m_BuffBtn0;
        private GameObject m_BuffDescBtnObj;
        private GameObject m_BuffDescNodeObj;
        private GameObject m_BuffDescTxtObj;
        private GameObject m_BuffSkillPanel;
        private int m_CurShowBuffCount;
        private BattleDragonView m_dragonView;
        private Image m_EpImg;
        public CUIFormScript m_FormScript;
        private int m_frameCount;
        private Image m_HpImg;
        private Text m_HpTxt;
        private Image m_imgSkillFrame;
        private const int m_iShowBuffNumMax = 5;
        public bool m_isInBattle;
        private const int m_iSkill3LvlMax = 3;
        private const int m_iSkillLvlMax = 6;
        private bool m_isSkillDecShow;
        public enInputMode m_leftAxisInputMode;
        public float m_MaxBattleFPS;
        private Text m_MgcDefTxt;
        public float m_MinBattleFPS = float.MaxValue;
        private GameObject m_objHeroHead;
        private Transform m_OpeankBigMap;
        private Transform m_OpeankSpeakAnim;
        private Transform m_OpenMicObj;
        private Transform m_OpenMicTipObj;
        private Text m_OpenMicTipText;
        private Transform m_OpenSpeakerObj;
        private Transform m_OpenSpeakerTipObj;
        private Text m_OpenSpeakerTipText;
        private GameObject m_panelHeroInfo;
        private Text m_PhyDefTxt;
        private PoolObjHandle<BuffSkill> m_selectBuff;
        private PoolObjHandle<ActorRoot> m_selectedHero;
        private SignalPanel m_signalPanel;
        public CSkillButtonManager m_skillButtonManager;
        public ListView<SLOTINFO> m_SkillSlotList = new ListView<SLOTINFO>();
        private CSoundChat m_soundChat;
        private Image m_textBgImage;
        private TowerHitMgr m_towerHitMgr;
        private int m_Vocetimer;
        private int m_VocetimerFirst;
        private int m_VoiceMictime;
        private int m_VoiceTipsShowTime = 0x7d0;
        private string microphone_path = "UGUI/Sprite/Battle/Battle_btn_Microphone.prefab";
        private MinimapSys minimap_sys;
        private string no_microphone_path = "UGUI/Sprite/Battle/Battle_btn_No_Microphone.prefab";
        private string no_voiceIcon_path = "UGUI/Sprite/Battle/Battle_btn_No_voice.prefab";
        public static string s_battleDragonTipForm = "UGUI/Form/Battle/Form_Battle_Dragon_Tips.prefab";
        public static string s_battleUIForm = "UGUI/Form/Battle/Form_Battle.prefab";
        public ScoreBoard scoreBoard;
        private ScoreboardPvE scoreboardPvE;
        private GameObject skillTipDesc;
        private Assets.Scripts.GameSystem.SoldierWave soldierWaveView;
        private CStarEvalPanel starEvalPanel;
        private float timeEnergyShortage;
        private float timeNoSkillTarget;
        private float timeSkillCooldown;
        private CTreasureHud treasureHud;
        public Vector2 UI_world_Factor_Big;
        public Vector2 UI_world_Factor_Small;
        private string voiceIcon_path = "UGUI/Sprite/Battle/Battle_btn_voice.prefab";
        public Vector2 world_UI_Factor_Big;
        public Vector2 world_UI_Factor_Small;

        private void AutoLearnSkill(PoolObjHandle<ActorRoot> hero)
        {
            if ((hero != 0) && hero.handle.ActorAgent.IsAutoAI())
            {
                for (int i = 3; i >= 1; i--)
                {
                    if (this.IsMatchLearnSkillRule(hero, (SkillSlotType) i))
                    {
                        FrameCommand<LearnSkillCommand> cmd = FrameCommandFactory.CreateFrameCommand<LearnSkillCommand>();
                        cmd.cmdData.dwHeroID = hero.handle.ObjID;
                        cmd.cmdData.bSlotType = (byte) i;
                        byte skillLevel = 0;
                        if ((hero.handle.SkillControl != null) && (hero.handle.SkillControl.SkillSlotArray[i] != null))
                        {
                            skillLevel = (byte) hero.handle.SkillControl.SkillSlotArray[i].GetSkillLevel();
                        }
                        cmd.cmdData.bSkillLevel = skillLevel;
                        hero.handle.ActorControl.CmdCommonLearnSkill(cmd);
                    }
                }
            }
        }

        private void Battle_ActivateForm(CUIEvent uiEvent)
        {
            if (this.m_FormScript != null)
            {
                this.m_FormScript.Appear();
            }
        }

        private void Battle_CloseForm(CUIEvent uiEvent)
        {
            Singleton<CUIManager>.GetInstance().CloseForm(s_battleUIForm);
        }

        private void Battle_OnCloseForm(CUIEvent uiEvent)
        {
            Singleton<InBattleMsgMgr>.instance.Clear();
            this.m_isInBattle = false;
            this.m_bOpenSpeak = false;
            this.m_bOpenMic = false;
            MonoSingleton<VoiceSys>.GetInstance().LeaveRoom();
            Singleton<CTimerManager>.instance.RemoveTimer(this.m_Vocetimer);
            Singleton<CTimerManager>.instance.RemoveTimer(this.m_VoiceMictime);
            Singleton<CTimerManager>.instance.RemoveTimer(this.m_VocetimerFirst);
            this.m_MaxBattleFPS = 0f;
            this.m_MinBattleFPS = float.MaxValue;
            this.m_AveBattleFPS = 0f;
            this.m_BattleFPSCount = 0f;
            this.m_frameCount = 0;
            this.m_SkillSlotList.Clear();
            this.m_battleFloatDigitManager.ClearAllBattleFloatText();
            if (this.killNotifation != null)
            {
                this.killNotifation.Clear();
                this.killNotifation = null;
            }
            if (this.battleStatView != null)
            {
                this.battleStatView.Clear();
                this.battleStatView = null;
            }
            if (this.scoreBoard != null)
            {
                this.scoreBoard.Clear();
                this.scoreBoard = null;
            }
            if (this.scoreboardPvE != null)
            {
                this.scoreboardPvE.Clear();
                this.scoreboardPvE = null;
            }
            if (this.treasureHud != null)
            {
                this.treasureHud.Clear();
                this.treasureHud = null;
            }
            if (this.starEvalPanel != null)
            {
                this.starEvalPanel.Clear();
                this.starEvalPanel = null;
            }
            if (this.battleTaskView != null)
            {
                this.battleTaskView.Clear();
                this.battleTaskView = null;
            }
            if (this.soldierWaveView != null)
            {
                this.soldierWaveView.Clear();
                this.soldierWaveView = null;
            }
            if (this.heroHeadHud != null)
            {
                this.heroHeadHud.Clear();
                this.heroHeadHud = null;
            }
            if (this.m_dragonView != null)
            {
                this.m_dragonView.Clear();
                this.m_dragonView = null;
            }
            if (this.m_signalPanel != null)
            {
                this.m_signalPanel.Clear();
                this.m_signalPanel = null;
            }
            if (this.minimap_sys != null)
            {
                this.minimap_sys.Clear();
                this.minimap_sys = null;
            }
            if (this.m_battleMisc != null)
            {
                this.m_battleMisc.Uninit();
                this.m_battleMisc.Clear();
                this.m_battleMisc = null;
            }
            if (this.m_soundChat != null)
            {
                this.m_soundChat.Clear();
                this.m_soundChat = null;
            }
            if (this.m_towerHitMgr != null)
            {
                this.m_towerHitMgr.Clear();
                this.m_towerHitMgr = null;
            }
            if (this.m_skillButtonManager != null)
            {
                this.m_skillButtonManager.Clear();
            }
            if (this.m_battleEquipSystem != null)
            {
                this.m_battleEquipSystem.Clear();
            }
            this._joystick = null;
            Singleton<CUIManager>.GetInstance().CloseForm(CSettingsSys.SETTING_FORM_BATTLE);
            Singleton<CBattleHeroInfoPanel>.GetInstance().Hide();
            this.m_FormScript = null;
        }

        private void Battle_OpenForm(CUIEvent uiEvent)
        {
            Singleton<CChatController>.instance.model.channelMgr.Clear(EChatChannel.Select_Hero, 0L, 0);
            Singleton<BattleLogic>.instance.battleStat.m_playerKDAStat.reset();
            this.m_MaxBattleFPS = 0f;
            this.m_MinBattleFPS = float.MaxValue;
            this.m_AveBattleFPS = 0f;
            this.m_BattleFPSCount = 0f;
            this.m_frameCount = 0;
            this.m_SkillSlotList.Clear();
            this.m_isInBattle = true;
            this.m_FormScript = Singleton<CUIManager>.GetInstance().OpenForm(s_battleUIForm, false, true);
            DebugHelper.Assert(this.m_FormScript != null, "Failed Battle_OpenForm");
            if (this.m_FormScript == null)
            {
                throw new Exception(string.Format("Failed open battle ui form {0}", s_battleUIForm));
            }
            this.m_Vocetimer = Singleton<CTimerManager>.instance.AddTimer(this.m_VoiceTipsShowTime, -1, new CTimer.OnTimeUpHandler(this.OnVoiceTimeEnd));
            Singleton<CTimerManager>.instance.PauseTimer(this.m_Vocetimer);
            Singleton<CTimerManager>.instance.ResetTimer(this.m_Vocetimer);
            this.m_VocetimerFirst = Singleton<CTimerManager>.instance.AddTimer(0x2710, 1, new CTimer.OnTimeUpHandler(this.OnVoiceTimeEndFirst));
            this.m_VoiceMictime = Singleton<CTimerManager>.instance.AddTimer(this.m_VoiceTipsShowTime, -1, new CTimer.OnTimeUpHandler(this.OnVoiceMicTimeEnd));
            Singleton<CTimerManager>.instance.PauseTimer(this.m_VoiceMictime);
            Singleton<CTimerManager>.instance.ResetTimer(this.m_VoiceMictime);
            this.m_OpenSpeakerObj = this.m_FormScript.transform.Find("panelTopLeft/MiniMap/Voice_OpenSpeaker");
            this.m_OpenSpeakerTipObj = this.m_FormScript.transform.Find("panelTopLeft/MiniMap/Voice_OpenSpeaker/info");
            this.m_OpeankSpeakAnim = this.m_FormScript.transform.Find("panelTopLeft/MiniMap/Voice_OpenSpeaker/voice_anim");
            this.m_OpeankBigMap = this.m_FormScript.transform.Find("panelTopLeft/MiniMap/Button_OpenBigMap");
            this.m_OpenMicObj = this.m_FormScript.transform.Find("panelTopLeft/MiniMap/Voice_OpenMic");
            this.m_OpenMicTipObj = this.m_FormScript.transform.Find("panelTopLeft/MiniMap/Voice_OpenMic/info");
            this.m_OpenSpeakerTipText = this.m_OpenSpeakerTipObj.Find("Text").GetComponent<Text>();
            try
            {
                MonoSingleton<VoiceSys>.GetInstance().ClosenSpeakers();
                MonoSingleton<VoiceSys>.GetInstance().CloseMic();
            }
            catch (Exception exception)
            {
                object[] inParameters = new object[] { exception.Message, exception.StackTrace };
                DebugHelper.Assert(false, "exception for closen speakers... {0} {1}", inParameters);
            }
            if (!MonoSingleton<NewbieGuideManager>.GetInstance().CheckTriggerTime(NewbieGuideTriggerTimeType.onBattleStart, new uint[0]))
            {
                if (this.m_OpenSpeakerTipObj != null)
                {
                    this.m_OpenSpeakerTipObj.gameObject.CustomSetActive(true);
                    this.m_OpenSpeakerTipText.text = MonoSingleton<VoiceSys>.GetInstance().m_Voice_Battle_FirstTips;
                }
                if (this.m_OpeankSpeakAnim != null)
                {
                    this.m_OpeankSpeakAnim.gameObject.CustomSetActive(true);
                }
            }
            if (this.m_OpeankBigMap != null)
            {
                this.m_OpeankBigMap.gameObject.CustomSetActive(true);
            }
            if (this.m_OpenMicTipObj != null)
            {
                this.m_OpenMicTipObj.gameObject.CustomSetActive(false);
                this.m_OpenMicTipText = this.m_OpenMicTipObj.Find("Text").GetComponent<Text>();
            }
            if (this.m_OpenSpeakerObj != null)
            {
                this.m_OpenSpeakerObj.gameObject.CustomSetActive(true);
            }
            if (this.m_OpenMicObj != null)
            {
                this.m_OpenMicObj.gameObject.CustomSetActive(true);
            }
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            ResDT_LevelCommonInfo info = CLevelCfgLogicManager.FindLevelConfigMultiGame(curLvelContext.iLevelID);
            this.SetJoyStickMoveType(GameSettings.JoyStickMoveType);
            this.SetJoyStickShowType(GameSettings.JoyStickShowType);
            this.SetFpsShowType(GameSettings.FpsShowType);
            this.killNotifation = new KillNotify();
            this.killNotifation.Init(Utility.FindChild(this.m_FormScript.gameObject, "KillNotify_New"));
            this.killNotifation.Hide();
            this.treasureHud = new CTreasureHud();
            this.treasureHud.Init(Utility.FindChild(this.m_FormScript.gameObject, "TreasurePanel"));
            this.treasureHud.Hide();
            this.starEvalPanel = new CStarEvalPanel();
            this.starEvalPanel.Init(Utility.FindChild(this.m_FormScript.gameObject, "StarEvalPanel"));
            this.starEvalPanel.reset();
            this.m_battleMisc = new BattleMisc();
            this.m_battleMisc.Init(Utility.FindChild(this.m_FormScript.gameObject, "mis"), this.m_FormScript);
            this.battleTaskView = new BattleTaskView();
            this.battleTaskView.Init(Utility.FindChild(this.m_FormScript.gameObject, "TaskView"));
            GameObject obj2 = Utility.FindChild(this.m_FormScript.gameObject, "PanelBtn/ToggleAutoBtn");
            obj2.CustomSetActive(!curLvelContext.isPVPLevel);
            if ((Singleton<BattleLogic>.GetInstance().GetCurLvelContext().LevelType == RES_LEVEL_TYPE.RES_LEVEL_TYPE_GUIDE) || !Singleton<BattleLogic>.GetInstance().GetCurLvelContext().canAutoAI)
            {
                obj2.CustomSetActive(false);
            }
            Utility.FindChild(this.m_FormScript.gameObject, "PanelBtn/btnViewBattleInfo").CustomSetActive(curLvelContext.isPVPLevel);
            GameObject obj4 = Utility.FindChild(this.m_FormScript.gameObject, "panelTopRight/SignalPanel");
            this.m_signalPanel = new SignalPanel();
            this.m_signalPanel.Init(this.m_FormScript, this.m_FormScript.GetWidget(6), null, this.m_FormScript.GetWidget(0x2b), curLvelContext.isPVPMode);
            GameObject obj5 = Utility.FindChild(this.m_FormScript.gameObject, "panelTopLeft/MiniMap");
            GameObject obj6 = Utility.FindChild(this.m_FormScript.gameObject, "panelTopLeft/BigMap");
            if (obj5 != null)
            {
                obj5.CustomSetActive(false);
            }
            if (obj6 != null)
            {
                obj6.CustomSetActive(false);
            }
            this.minimap_sys = new MinimapSys();
            this.minimap_sys.Init(this.m_FormScript, info, curLvelContext);
            Singleton<InBattleMsgMgr>.instance.InitView(Utility.FindChild(this.m_FormScript.gameObject, "panelTopRight/SignalPanel/Button_Voice"), this.m_FormScript);
            if (!curLvelContext.isPVPMode)
            {
                if (this.m_OpenSpeakerObj != null)
                {
                    this.m_OpenSpeakerObj.gameObject.CustomSetActive(false);
                }
                if (this.m_OpenMicObj != null)
                {
                    this.m_OpenMicObj.gameObject.CustomSetActive(false);
                }
                if (this.m_OpeankSpeakAnim != null)
                {
                    this.m_OpeankSpeakAnim.gameObject.CustomSetActive(false);
                }
                if (this.m_OpeankBigMap != null)
                {
                    this.m_OpeankBigMap.gameObject.CustomSetActive(false);
                }
            }
            obj4.CustomSetActive(curLvelContext.isPVPMode);
            if (curLvelContext.LevelType == RES_LEVEL_TYPE.RES_LEVEL_TYPE_GUIDE)
            {
                if (this.m_OpenSpeakerObj != null)
                {
                    this.m_OpenSpeakerObj.gameObject.CustomSetActive(false);
                }
                if (this.m_OpenMicObj != null)
                {
                    this.m_OpenMicObj.gameObject.CustomSetActive(false);
                }
            }
            if (curLvelContext.LevelType == RES_LEVEL_TYPE.RES_LEVEL_TYPE_DEFEND)
            {
                this.soldierWaveView = new Assets.Scripts.GameSystem.SoldierWave();
                this.soldierWaveView.Init(Utility.FindChild(this.m_FormScript.gameObject, "WaveStatistics"));
                this.soldierWaveView.Show();
            }
            GameObject obj7 = Utility.FindChild(this.m_FormScript.gameObject, "ScoreBoard");
            GameObject obj8 = Utility.FindChild(this.m_FormScript.gameObject, "ScoreBoardPvE");
            GameObject obj9 = Utility.FindChild(this.m_FormScript.gameObject, "panelTopLeft/DragonInfo");
            if (curLvelContext.isPVPMode)
            {
                this.scoreBoard = new ScoreBoard();
                this.scoreBoard.Init(obj7);
                this.scoreBoard.RegiseterEvent();
                this.scoreBoard.Show();
                this.battleStatView = new BattleStatView();
                this.battleStatView.Init();
            }
            else
            {
                obj7.CustomSetActive(false);
                if (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_ADVENTURE)
                {
                    this.scoreboardPvE = new ScoreboardPvE();
                    this.scoreboardPvE.Init(obj8);
                    this.scoreboardPvE.Show();
                }
            }
            if (Singleton<BattleLogic>.instance.m_dragonSpawn != null)
            {
                if ((info != null) && (info.bMaxAcntNum == 10))
                {
                    obj9.CustomSetActive(false);
                }
                else
                {
                    obj9.CustomSetActive(true);
                    this.m_dragonView = new BattleDragonView();
                    this.m_dragonView.Init(obj9, Singleton<BattleLogic>.instance.m_dragonSpawn);
                }
            }
            else
            {
                obj9.CustomSetActive(false);
            }
            GameObject widget = this.m_FormScript.GetWidget(0x27);
            GameObject obj11 = this.m_FormScript.GetWidget(40);
            GameObject obj12 = this.m_FormScript.GetWidget(0x29);
            if (curLvelContext.IsPvpGameType())
            {
                widget.CustomSetActive(false);
            }
            else
            {
                widget.CustomSetActive(true);
            }
            obj11.CustomSetActive(false);
            obj12.CustomSetActive(false);
            if (this.m_FormScript != null)
            {
                this.m_FormScript.Hide(true);
            }
            this.m_soundChat = new CSoundChat();
            this.m_soundChat.Init(Utility.FindChild(this.m_FormScript.gameObject, "panelTopRight/SignalPanel/Button_Voice/cd"), Utility.FindChild(this.m_FormScript.gameObject, "panelTopRight/SignalPanel/Button_Voice/info"));
            this.m_towerHitMgr = new TowerHitMgr();
            this.m_towerHitMgr.Init();
            Singleton<InBattleMsgMgr>.instance.RegInBattleEvent();
            if (CSysDynamicBlock.bUnfinishBlock)
            {
                Utility.FindChild(this.m_FormScript.gameObject, "panelTopRight/SignalPanel/Button_Voice").CustomSetActive(false);
            }
            this.InitShowBuffDesc();
            GameObject obj14 = this.m_FormScript.GetWidget(0x2d);
            if (obj14 != null)
            {
                obj14.CustomSetActive(curLvelContext.isPVPMode);
            }
            GameObject obj15 = this.m_FormScript.GetWidget(0x2e);
            if (obj15 != null)
            {
                Text component = obj15.GetComponent<Text>();
                if (component != null)
                {
                    component.text = 0.ToString();
                }
            }
            this._joystick = this.m_FormScript.GetWidget(0x26).transform.GetComponent<CUIJoystickScript>();
            this.m_battleEquipSystem.Initialize(this.m_FormScript, Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer().Captain, curLvelContext.isPVPMode, curLvelContext.bBattleEquipLimit);
        }

        private void CheckAndUpdateLearnSkill(PoolObjHandle<ActorRoot> hero)
        {
            if (hero != 0)
            {
                for (int i = 1; i <= 3; i++)
                {
                    if (this.IsMatchLearnSkillRule(hero, (SkillSlotType) i))
                    {
                        this.UpdateLearnSkillBtnState(i, true);
                    }
                    else
                    {
                        this.UpdateLearnSkillBtnState(i, false);
                    }
                }
            }
        }

        public void ClearSkillLvlStates(int iSkillSlotType)
        {
            SkillButton button = this.GetButton((SkillSlotType) iSkillSlotType);
            if (button != null)
            {
                GameObject skillLvlImg = button.GetSkillLvlImg(1);
                if (skillLvlImg != null)
                {
                    ListView<GameObject> view = new ListView<GameObject>();
                    Transform parent = skillLvlImg.transform.parent;
                    int childCount = parent.childCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        GameObject gameObject = parent.GetChild(i).gameObject;
                        if (gameObject.name.Contains("SkillLvlImg") && gameObject.activeSelf)
                        {
                            view.Add(gameObject);
                        }
                    }
                    childCount = view.Count;
                    for (int j = 0; j < childCount; j++)
                    {
                        view[j].CustomSetActive(false);
                    }
                    view.Clear();
                }
            }
        }

        public void CreateBattleFloatDigit(int digitValue, DIGIT_TYPE digitType, Vector3 worldPosition)
        {
            if (this.m_FormScript != null)
            {
                this.m_battleFloatDigitManager.CreateBattleFloatDigit(digitValue, digitType, ref worldPosition);
            }
        }

        public void CreateBattleFloatDigit(int digitValue, DIGIT_TYPE digitType, Vector3 worldPosition, int animatIndex)
        {
            if (this.m_FormScript != null)
            {
                this.m_battleFloatDigitManager.CreateBattleFloatDigit(digitValue, digitType, ref worldPosition, animatIndex);
            }
        }

        public void CreateOtherFloatText(enOtherFloatTextContent otherFloatTextContent, Vector3 worldPosition, params string[] args)
        {
            if (this.m_FormScript != null)
            {
                this.m_battleFloatDigitManager.CreateOtherFloatText(otherFloatTextContent, ref worldPosition, args);
            }
        }

        public void CreateRestrictFloatText(RESTRICT_TYPE restrictType, Vector3 worldPosition)
        {
            if (this.m_FormScript != null)
            {
                this.m_battleFloatDigitManager.CreateRestrictFloatText(restrictType, ref worldPosition);
            }
        }

        public void DisableUIEvent()
        {
            if ((this.m_FormScript != null) && (this.m_FormScript.gameObject != null))
            {
                GraphicRaycaster component = this.m_FormScript.gameObject.GetComponent<GraphicRaycaster>();
                if (component != null)
                {
                    component.enabled = false;
                }
            }
        }

        public void EndCameraDrag()
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if ((this.m_FormScript != null) && (curLvelContext != null))
            {
                GameObject obj2 = Utility.FindChild(this.m_FormScript.gameObject, "CameraDragPanel");
                if (curLvelContext.isPVPMode)
                {
                    obj2.CustomSetActive(false);
                }
                if ((curLvelContext.isPVPMode && (Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain != 0)) && (this.m_FormScript != null))
                {
                    GameObject gameObject = obj2.transform.Find("panelDeadInfo").gameObject;
                    gameObject.transform.Find("Timer").GetComponent<CUITimerScript>().EndTimer();
                    gameObject.CustomSetActive(false);
                }
            }
        }

        public BattleMisc GetBattleMisc()
        {
            return this.m_battleMisc;
        }

        public BattleStatView GetBattleStatView()
        {
            return this.battleStatView;
        }

        public SkillButton GetButton(SkillSlotType skillSlotType)
        {
            return this.m_skillButtonManager.GetButton(skillSlotType);
        }

        public SkillSlotType GetCurSkillSlotType()
        {
            return this.m_skillButtonManager.GetCurSkillSlotType();
        }

        public CUIJoystickScript GetJoystick()
        {
            return this._joystick;
        }

        public KillNotify GetKillNotifation()
        {
            return this.killNotifation;
        }

        public MinimapSys GetMinimapSys()
        {
            return this.minimap_sys;
        }

        public SignalPanel GetSignalPanel()
        {
            return this.m_signalPanel;
        }

        public TowerHitMgr GetTowerHitMgr()
        {
            return this.m_towerHitMgr;
        }

        public void HandleMoveInput(Vector2 axis, enInputMode inputMode)
        {
            if (axis != Vector2.zero)
            {
                if (((inputMode == enInputMode.UI) || (inputMode == this.m_leftAxisInputMode)) || (this.m_leftAxisInputMode == enInputMode.None))
                {
                    this.m_leftAxisInputMode = inputMode;
                    Singleton<GameInput>.GetInstance().SendMoveDirection(Vector2.zero, axis);
                }
            }
            else if (inputMode == this.m_leftAxisInputMode)
            {
                this.m_leftAxisInputMode = enInputMode.None;
                Singleton<GameInput>.GetInstance().SendStopMove(null, false);
            }
        }

        public void HideHeroInfoPanel()
        {
            if (this.m_panelHeroInfo == null)
            {
                this.m_panelHeroInfo = Utility.FindChild(this.m_FormScript.gameObject, "PanelHeroInfo");
            }
            if (this.m_panelHeroInfo != null)
            {
                this.m_panelHeroInfo.CustomSetActive(false);
            }
            Singleton<CBattleSelectTarget>.GetInstance().CloseForm();
            this.m_selectedHero.Release();
        }

        public void HideSkillDescInfo()
        {
            if (this.skillTipDesc == null)
            {
                this.skillTipDesc = Utility.FindChild(this.m_FormScript.gameObject, "Panel_SkillTip");
                if (this.skillTipDesc == null)
                {
                    return;
                }
            }
            if (this.skillTipDesc != null)
            {
                this.skillTipDesc.CustomSetActive(false);
                this.m_isSkillDecShow = false;
            }
        }

        public override void Init()
        {
            this.m_battleFloatDigitManager = new CBattleFloatDigitManager();
            this.m_skillButtonManager = new CSkillButtonManager();
            this.m_battleEquipSystem = new CBattleEquipSystem();
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_MultiHashInvalid, new CUIEventManager.OnUIEventHandler(this.onMultiHashNotSync));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_ActivateForm, new CUIEventManager.OnUIEventHandler(this.Battle_ActivateForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OpenForm, new CUIEventManager.OnUIEventHandler(this.Battle_OpenForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_CloseForm, new CUIEventManager.OnUIEventHandler(this.Battle_CloseForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnCloseForm, new CUIEventManager.OnUIEventHandler(this.Battle_OnCloseForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OpenSysMenu, new CUIEventManager.OnUIEventHandler(this.ShowSysMenu));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_SysReturnLobby, new CUIEventManager.OnUIEventHandler(this.onReturnLobby));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_ConfirmSysReturnLobby, new CUIEventManager.OnUIEventHandler(this.onConfirmReturnLobby));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_SysQuitGame, new CUIEventManager.OnUIEventHandler(this.onQuitGame));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_SysReturnGame, new CUIEventManager.OnUIEventHandler(this.onReturnGame));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_ActivateForm, new CUIEventManager.OnUIEventHandler(this.onChangeOperateMode));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_SwitchAutoAI, new CUIEventManager.OnUIEventHandler(this.OnToggleAutoAI));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_ChgFreeCamera, new CUIEventManager.OnUIEventHandler(this.OnToggleFreeCamera));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_HeroInfoSwitch, new CUIEventManager.OnUIEventHandler(this.OnSwitchHeroInfoPanel));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_HeroInfoPanelOpen, new CUIEventManager.OnUIEventHandler(this.OnOpenHeorInfoPanel));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_HeroInfoPanelClose, new CUIEventManager.OnUIEventHandler(this.OnCloseHeorInfoPanel));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_ToggleStatView, new CUIEventManager.OnUIEventHandler(this.onClickBoard));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_FPSAndLagUpdate, new CUIEventManager.OnUIEventHandler(this.UpdateFpsAndLag));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_Down_MiniMap, new CUIEventManager.OnUIEventHandler(this.OnMiniMap_Click_Down));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_Click_MiniMap_Up, new CUIEventManager.OnUIEventHandler(this.OnMiniMap_Click_Up));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_Drag_SignalPanel, new CUIEventManager.OnUIEventHandler(this.OnDragMiniMap));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_Drag_SignalPanel_End, new CUIEventManager.OnUIEventHandler(this.OnDragMiniMapEnd));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnOpenDragonTip, new CUIEventManager.OnUIEventHandler(this.OnDragonTipFormOpen));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnCloseDragonTip, new CUIEventManager.OnUIEventHandler(this.OnDragonTipFormClose));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnAxisChanged, new CUIEventManager.OnUIEventHandler(this.OnLeftAxisChanged));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnSkillButtonDown, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillButtonDown));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnSkillButtonUp, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillButtonUp));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_Disable_Alert, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillDisableAlert));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnSkillButtonDragged, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillButtonDragged));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_Disable_Down, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillDisableBtnDown));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_Disable_Up, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillDisableBtnUp));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnSkillButtonHold, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillBtnHold));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnSkillButtonHoldEnd, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillButtonHoldEnd));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnAtkSelectHeroDown, new CUIEventManager.OnUIEventHandler(this.OnBattleAtkSelectHeroBtnDown));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnAtkSelectHeroUp, new CUIEventManager.OnUIEventHandler(this.OnBattleAtkSelectHeroBtnUp));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnAtkSelectSoldierDown, new CUIEventManager.OnUIEventHandler(this.OnBattleAtkSelectSoldierBtnDown));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnAtkSelectSoldierUp, new CUIEventManager.OnUIEventHandler(this.OnBattleAtkSelectSoldierBtnUp));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_LearnSkillBtn_Click, new CUIEventManager.OnUIEventHandler(this.OnBattleLearnSkillButtonClick));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_SkillLevelUpEffectPlayEnd, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillLevelUpEffectPlayEnd));
            Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int>("HeroSoulLevelChange", new Action<PoolObjHandle<ActorRoot>, int>(this, (IntPtr) this.OnHeroSoulLvlChange));
            Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, byte, byte>("HeroSkillLevelUp", new Action<PoolObjHandle<ActorRoot>, byte, byte>(this, (IntPtr) this.OnHeroSkillLvlup));
            Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int, int>("HeroEnergyChange", new Action<PoolObjHandle<ActorRoot>, int, int>(this, (IntPtr) this.onHeroEnergyChange));
            Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int, int>("HeroEnergyMax", new Action<PoolObjHandle<ActorRoot>, int, int>(this, (IntPtr) this.onHeroEnergyMax));
            Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int, int>("HeroHpChange", new Action<PoolObjHandle<ActorRoot>, int, int>(this, (IntPtr) this.OnHeroHpChange));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Common_BattleWifiCheckTimer, new CUIEventManager.OnUIEventHandler(this.OnCommon_BattleWifiCheckTimer));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Common_BattleShowOrHideWifiInfo, new CUIEventManager.OnUIEventHandler(this.OnCommon_BattleShowOrHideWifiInfo));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_CameraDraging, new CUIEventManager.OnUIEventHandler(this.OnDropCamera));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_RevivieTimer, new CUIEventManager.OnUIEventHandler(this.OnReviveTimerChange));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<ChangeSkillEventParam>(GameSkillEventDef.Event_ChangeSkill, new GameSkillEvent<ChangeSkillEventParam>(this.OnPlayerSkillChanged));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_ChangeSkillCD, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerSkillCDChanged));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_SkillCDStart, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerSkillCDStart));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_SkillCDEnd, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerSkillCDEnd));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_EnableSkill, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerSkillEnable));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_DisableSkill, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerSkillDisable));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<SpawnBuffEventParam>(GameSkillEventDef.Event_SpawnBuff, new GameSkillEvent<SpawnBuffEventParam>(this.OnPlayerSpawnBuff));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<ActorSkillEventParam>(GameSkillEventDef.Event_ProtectDisappear, new GameSkillEvent<ActorSkillEventParam>(this.OnPlayerProtectDisappear));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<ActorSkillEventParam>(GameSkillEventDef.Event_SkillCooldown, new GameSkillEvent<ActorSkillEventParam>(this.OnPlayerSkillCooldown));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<ActorSkillEventParam>(GameSkillEventDef.Enent_EnergyShortage, new GameSkillEvent<ActorSkillEventParam>(this.OnPlayerEnergyShortage));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<ActorSkillEventParam>(GameSkillEventDef.Event_NoSkillTarget, new GameSkillEvent<ActorSkillEventParam>(this.OnPlayerNoSkillTarget));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<ActorSkillEventParam>(GameSkillEventDef.Event_Blindess, new GameSkillEvent<ActorSkillEventParam>(this.OnPlayerBlindess));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_LimitSkill, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerLimitSkill));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_CancelLimitSkill, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerCancelLimitSkill));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_UpdateSkillUI, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerUpdateSkill));
            Singleton<GameEventSys>.GetInstance().AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorImmune, new RefAction<DefaultGameEventParam>(this.OnActorImmune));
            Singleton<GameEventSys>.GetInstance().AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorHurtAbsorb, new RefAction<DefaultGameEventParam>(this.OnActorHurtAbsorb));
            Singleton<GameEventSys>.GetInstance().AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_AutoAISwitch, new RefAction<DefaultGameEventParam>(this.OnSwitchAutoAI));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnFloatTextAnimEnd, new CUIEventManager.OnUIEventHandler(this.OnFloatTextAnimEnd));
            Singleton<GameEventSys>.GetInstance().AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.onActorDead));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<ActorSkillEventParam>(GameSkillEventDef.Event_UseSkill, new GameSkillEvent<ActorSkillEventParam>(this.onUseSkill));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_Pause_Game, new CUIEventManager.OnUIEventHandler(this.OnBattlePauseGame));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_Resume_Game, new CUIEventManager.OnUIEventHandler(this.OnBattleResumeGame));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_BuffSkillBtn_Down, new CUIEventManager.OnUIEventHandler(this.OnBuffSkillBtnDown));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_BuffSkillBtn_Up, new CUIEventManager.OnUIEventHandler(this.OnBuffSkillBtnUp));
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<BuffChangeEventParam>(GameSkillEventDef.Event_BuffChange, new GameSkillEvent<BuffChangeEventParam>(this.OnPlayerBuffChange));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BattleEquip_Form_Open, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipFormOpen));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BattleEquip_Form_Close, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipFormClose));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BattleEquip_TypeList_Select, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipTypeListSelect));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BattleEquip_Item_Select, new CUIEventManager.OnUIEventHandler(this.OnBattleEuipItemSelect));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BattleEquip_BagItem_Select, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipBagItemSelect));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BattleEquip_BuyBtn_Click, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipBuy));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BattleEquip_SaleBtn_Click, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipSale));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BattleEquip_RecommendEquip_Buy, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipQuicklyBuy));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_EquipBoughtEffectPlayEnd, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipBoughtEffectPlayEnd));
            Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int, int, bool>("HeroGoldCoinInBattleChange", new Action<PoolObjHandle<ActorRoot>, int, int, bool>(this, (IntPtr) this.OnActorGoldCoinInBattleChanged));
            Singleton<EventRouter>.GetInstance().AddEventHandler<uint, stEquipInfo[]>("HeroEquipInBattleChange", new Action<uint, stEquipInfo[]>(this, (IntPtr) this.OnHeroEquipInBattleChanged));
            Singleton<EventRouter>.GetInstance().AddEventHandler<uint>("HeroRecommendEquipInit", new Action<uint>(this.OnHeroRecommendEquipInit));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Trusteeship_Accept, new CUIEventManager.OnUIEventHandler(this.OnAcceptTrusteeship));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Trusteeship_Cancel, new CUIEventManager.OnUIEventHandler(this.OnCancelTrusteeship));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VOICE_OpenSpeaker, new CUIEventManager.OnUIEventHandler(this.OnBattleOpenSpeaker));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VOICE_OpenMic, new CUIEventManager.OnUIEventHandler(this.OnBattleOpenMic));
            Singleton<EventRouter>.GetInstance().AddEventHandler<CommonAttactType>(EventID.GAME_SETTING_COMMONATTACK_TYPE_CHANGE, new Action<CommonAttactType>(this.OnGameSettingCommonAttackTypeChange));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_Click_Scene, new CUIEventManager.OnUIEventHandler(this.OnClickBattleScene));
        }

        private RectTransform initMap(enBattleFormWidget mapIndex, SLevelContext levelContext, bool bMinimap, out float preWidth)
        {
            preWidth = 0f;
            GameObject widget = this.m_FormScript.GetWidget((int) mapIndex);
            DebugHelper.Assert(widget != null, "initMap GetWidget is null");
            if (widget == null)
            {
                return null;
            }
            Image component = widget.GetComponent<Image>();
            DebugHelper.Assert(component != null, "initMap map.GetComponent<Image>() is null");
            if (component == null)
            {
                return null;
            }
            RectTransform transform = widget.transform as RectTransform;
            preWidth = transform.rect.width;
            Vector2 sizeDelta = transform.sizeDelta;
            widget.CustomSetActive(bMinimap);
            string prefabPath = CUIUtility.s_Sprite_Dynamic_Map_Dir + (!bMinimap ? "pvp_5_big" : levelContext.miniMapPath);
            component.SetSprite(prefabPath, this.m_FormScript, true, false, false);
            component.SetNativeSize();
            if (bMinimap)
            {
                this.initWorldUITransformFactor(sizeDelta, levelContext, bMinimap, out this.world_UI_Factor_Small, out this.UI_world_Factor_Small);
                return transform;
            }
            this.initWorldUITransformFactor(sizeDelta, levelContext, bMinimap, out this.world_UI_Factor_Big, out this.UI_world_Factor_Big);
            return transform;
        }

        private bool InitShowBuffDesc()
        {
            this.m_CurShowBuffCount = 0;
            this.m_selectBuff = new PoolObjHandle<BuffSkill>();
            this.m_BuffSkillPanel = Utility.FindChild(this.m_FormScript.gameObject, "BuffSkill");
            if (this.m_BuffSkillPanel == null)
            {
                return false;
            }
            for (int i = 0; i < 5; i++)
            {
                this.m_ArrShowBuffSkill[i] = new BuffInfo();
                string path = string.Format("BuffSkillBtn_{0}", i);
                GameObject obj2 = Utility.FindChild(this.m_BuffSkillPanel, path);
                if (obj2 != null)
                {
                    if (i == 0)
                    {
                        this.m_BuffBtn0 = obj2;
                        this.m_imgSkillFrame = Utility.GetComponetInChild<Image>(this.m_BuffBtn0, "SkillFrame");
                    }
                    obj2.CustomSetActive(false);
                }
            }
            this.m_BuffDescNodeObj = Utility.FindChild(this.m_BuffSkillPanel, "BuffDesc");
            if (this.m_BuffDescNodeObj == null)
            {
                return false;
            }
            this.m_BuffDescNodeObj.CustomSetActive(false);
            this.m_textBgImage = Utility.GetComponetInChild<Image>(this.m_BuffDescNodeObj, "bg");
            if (this.m_textBgImage == null)
            {
                return false;
            }
            this.m_textBgImage.gameObject.CustomSetActive(true);
            this.m_bIsFirst = true;
            return true;
        }

        private void initWorldUITransformFactor(Vector2 mapImgSize, SLevelContext levelContext, bool bMiniMap, out Vector2 world_UI_Factor, out Vector2 UI_world_Factor)
        {
            int num = !bMiniMap ? 150 : levelContext.mapWidth;
            int num2 = !bMiniMap ? 150 : levelContext.mapHeight;
            float x = mapImgSize.x / ((float) num);
            float y = mapImgSize.y / ((float) num2);
            world_UI_Factor = new Vector2(x, y);
            float num5 = ((float) num) / mapImgSize.x;
            float num6 = ((float) num2) / mapImgSize.y;
            UI_world_Factor = new Vector2(num5, num6);
            if (levelContext.bCameraFlip)
            {
                world_UI_Factor = new Vector2(-x, -y);
                UI_world_Factor = new Vector2(-num5, -num6);
            }
        }

        public bool IsMatchLearnSkillRule(PoolObjHandle<ActorRoot> hero, SkillSlotType slotType)
        {
            bool flag = false;
            if (((hero == 0) || (slotType < SkillSlotType.SLOT_SKILL_1)) || (slotType > SkillSlotType.SLOT_SKILL_3))
            {
                return flag;
            }
            if (((hero.handle.SkillControl != null) && (hero.handle.SkillControl.m_iSkillPoint > 0)) && (hero.handle.SkillControl.SkillSlotArray[(int) slotType] != null))
            {
                int allSkillLevel = hero.handle.SkillControl.GetAllSkillLevel();
                if ((hero.handle.ValueComponent != null) && (allSkillLevel >= hero.handle.ValueComponent.actorSoulLevel))
                {
                    return false;
                }
                int skillLevel = hero.handle.SkillControl.SkillSlotArray[(int) slotType].GetSkillLevel();
                int num3 = skillLevel + 1;
                int actorSoulLevel = hero.handle.ValueComponent.actorSoulLevel;
                if (skillLevel >= 6)
                {
                    return flag;
                }
                if ((slotType == SkillSlotType.SLOT_SKILL_3) && (skillLevel < 3))
                {
                    if (((num3 * 4) - 1) >= actorSoulLevel)
                    {
                        return flag;
                    }
                    return true;
                }
                if (((slotType < SkillSlotType.SLOT_SKILL_1) || (slotType >= SkillSlotType.SLOT_SKILL_3)) || (((num3 * 2) - 1) > actorSoulLevel))
                {
                    return flag;
                }
                return true;
            }
            if (((hero.handle.SkillControl == null) || (hero.handle.SkillControl.m_iSkillPoint <= 0)) || (hero.handle.SkillControl.SkillSlotArray[(int) slotType] != null))
            {
                return flag;
            }
            if (slotType == SkillSlotType.SLOT_SKILL_3)
            {
                if (hero.handle.ValueComponent.actorSoulLevel >= 4)
                {
                    flag = true;
                }
                return flag;
            }
            return true;
        }

        public void LateUpdate()
        {
            if (this.m_isInBattle)
            {
                this.m_battleFloatDigitManager.LateUpdate();
                if (this.battleStatView != null)
                {
                    this.battleStatView.LateUpdate();
                }
                if (this.scoreBoard != null)
                {
                    this.scoreBoard.LateUpdate();
                }
            }
        }

        public void Move_CameraToClickPosition(CUIEvent uiEvent)
        {
            MonoSingleton<CameraSystem>.instance.ToggleFreeDragCamera(true);
            if (((this.m_FormScript != null) && (uiEvent.m_srcWidget != null)) && (uiEvent.m_pointerEventData != null))
            {
                Vector2 position = uiEvent.m_pointerEventData.position;
                Vector2 vector2 = CUIUtility.WorldToScreenPoint(this.m_FormScript.GetCamera(), uiEvent.m_srcWidget.transform.position);
                float num = position.x - vector2.x;
                float num2 = position.y - vector2.y;
                float x = num * Singleton<CBattleSystem>.instance.UI_world_Factor_Small.x;
                float z = num2 * Singleton<CBattleSystem>.instance.UI_world_Factor_Small.y;
                if (MonoSingleton<CameraSystem>.instance.MobaCamera != null)
                {
                    MonoSingleton<CameraSystem>.instance.MobaCamera.SetAbsoluteLockLocation(new Vector3(x, 1f, z));
                }
            }
        }

        public void OnAcceptTrusteeship(CUIEvent uiEvent)
        {
            this.SendTrusteeshipResult(uiEvent.m_eventParams.commonUInt32Param1, ACCEPT_AIPLAYER_RSP.ACCEPT_AIPLAEYR_YES);
        }

        private void onActorDead(ref DefaultGameEventParam prm)
        {
            if (ActorHelper.IsHostCtrlActor(ref prm.src) && (this.m_skillButtonManager != null))
            {
                this.m_skillButtonManager.SkillButtonUp(this.m_FormScript);
            }
        }

        private void OnActorGoldCoinInBattleChanged(PoolObjHandle<ActorRoot> actor, int changeValue, int currentValue, bool isIncome)
        {
            if (Singleton<BattleLogic>.GetInstance().m_GameInfo.gameContext.levelContext.isPVPMode && ((actor != 0) && ActorHelper.IsHostCtrlActor(ref actor)))
            {
                if (this.m_FormScript != null)
                {
                    GameObject widget = this.m_FormScript.GetWidget(0x2e);
                    if (widget != null)
                    {
                        Text component = widget.GetComponent<Text>();
                        if (component != null)
                        {
                            component.text = currentValue.ToString();
                        }
                    }
                }
                if (this.m_battleEquipSystem != null)
                {
                    this.m_battleEquipSystem.OnActorGoldChange(changeValue, currentValue);
                }
            }
        }

        private void OnActorHurtAbsorb(ref DefaultGameEventParam _param)
        {
            if ((_param.src != 0) && ActorHelper.IsHostActor(ref _param.src))
            {
                Singleton<CBattleSystem>.GetInstance().CreateOtherFloatText(enOtherFloatTextContent.Absorb, (Vector3) _param.src.handle.location, new string[0]);
            }
            else if ((_param.atker != 0) && ActorHelper.IsHostActor(ref _param.atker))
            {
                Singleton<CBattleSystem>.GetInstance().CreateOtherFloatText(enOtherFloatTextContent.Absorb, (Vector3) _param.src.handle.location, new string[0]);
            }
        }

        private void OnActorImmune(ref DefaultGameEventParam _param)
        {
            if ((_param.src != 0) && ActorHelper.IsHostActor(ref _param.src))
            {
                Singleton<CBattleSystem>.GetInstance().CreateOtherFloatText(enOtherFloatTextContent.Immunity, (Vector3) _param.src.handle.location, new string[0]);
            }
            else if ((_param.atker != 0) && ActorHelper.IsHostActor(ref _param.atker))
            {
                Singleton<CBattleSystem>.GetInstance().CreateOtherFloatText(enOtherFloatTextContent.Immunity, (Vector3) _param.src.handle.location, new string[0]);
            }
        }

        private void OnBattleAtkSelectHeroBtnDown(CUIEvent uiEvent)
        {
            Singleton<LockModeKeySelector>.GetInstance().OnHandleClickSelectTargetBtn(AttackTargetType.ATTACK_TARGET_HERO);
            CUICommonSystem.PlayAnimator(uiEvent.m_srcWidget.transform.Find("Present").gameObject, enSkillButtonAnimationName.atkPressDown.ToString());
            Singleton<CUIParticleSystem>.GetInstance().AddParticle(CUIParticleSystem.s_particleSkillBtnEffect_Path, 0.5f, uiEvent.m_srcWidget, uiEvent.m_srcFormScript);
            Singleton<CSoundManager>.GetInstance().PostEvent("UI_signal_Change_hero", null);
        }

        private void OnBattleAtkSelectHeroBtnUp(CUIEvent uiEvent)
        {
            CUICommonSystem.PlayAnimator(uiEvent.m_srcWidget.transform.Find("Present").gameObject, enSkillButtonAnimationName.atkPressUp.ToString());
        }

        private void OnBattleAtkSelectSoldierBtnDown(CUIEvent uiEvent)
        {
            Singleton<LockModeKeySelector>.GetInstance().OnHandleClickSelectTargetBtn(AttackTargetType.ATTACK_TARGET_SOLDIER);
            CUICommonSystem.PlayAnimator(uiEvent.m_srcWidget.transform.Find("Present").gameObject, enSkillButtonAnimationName.atkPressDown.ToString());
            Singleton<CUIParticleSystem>.GetInstance().AddParticle(CUIParticleSystem.s_particleSkillBtnEffect_Path, 0.5f, uiEvent.m_srcWidget, uiEvent.m_srcFormScript);
            Singleton<CSoundManager>.GetInstance().PostEvent("UI_signal_Change_xiaobing", null);
        }

        private void OnBattleAtkSelectSoldierBtnUp(CUIEvent uiEvent)
        {
            CUICommonSystem.PlayAnimator(uiEvent.m_srcWidget.transform.Find("Present").gameObject, enSkillButtonAnimationName.atkPressUp.ToString());
        }

        private void OnBattleEquipBagItemSelect(CUIEvent uiEvent)
        {
            if (this.m_battleEquipSystem != null)
            {
                this.m_battleEquipSystem.OnEquipBagItemSelect(uiEvent);
            }
        }

        private void OnBattleEquipBoughtEffectPlayEnd(CUIEvent uiEvent)
        {
            uiEvent.m_srcWidget.CustomSetActive(false);
        }

        private void OnBattleEquipBuy(CUIEvent uiEvent)
        {
            if (this.m_battleEquipSystem != null)
            {
                this.m_battleEquipSystem.OnEquipBuyBtnClick(uiEvent);
            }
        }

        private void OnBattleEquipFormClose(CUIEvent uiEvent)
        {
            if (this.m_battleEquipSystem != null)
            {
                this.m_battleEquipSystem.OnEquipFormClose(uiEvent);
            }
        }

        private void OnBattleEquipFormOpen(CUIEvent uiEvent)
        {
            if (this.m_battleEquipSystem != null)
            {
                this.m_battleEquipSystem.OnEquipFormOpen(uiEvent);
            }
        }

        private void OnBattleEquipQuicklyBuy(CUIEvent uiEvent)
        {
            if (this.m_battleEquipSystem != null)
            {
                this.m_battleEquipSystem.OnBattleEquipQuicklyBuy(uiEvent);
                if (this.m_FormScript != null)
                {
                    GameObject widget = this.m_FormScript.GetWidget(50 + uiEvent.m_eventParams.battleEquipPar.m_indexInQuicklyBuyList);
                    if (widget != null)
                    {
                        widget.CustomSetActive(true);
                        CUIAnimationScript component = widget.GetComponent<CUIAnimationScript>();
                        if (component != null)
                        {
                            component.PlayAnimation("Battle_UI_ZhuangBei_01", true);
                        }
                    }
                    GameObject obj3 = this.m_FormScript.GetWidget(0x34 + uiEvent.m_eventParams.battleEquipPar.m_indexInQuicklyBuyList);
                    if (obj3 != null)
                    {
                        obj3.CustomSetActive(true);
                        CUIAnimationScript script2 = obj3.GetComponent<CUIAnimationScript>();
                        if (script2 != null)
                        {
                            script2.PlayAnimation("Battle_UI_ZhuangBei_01", true);
                        }
                    }
                }
            }
        }

        private void OnBattleEquipSale(CUIEvent uiEvent)
        {
            if (this.m_battleEquipSystem != null)
            {
                this.m_battleEquipSystem.OnEquipSaleBtnClick(uiEvent);
            }
        }

        private void OnBattleEquipTypeListSelect(CUIEvent uiEvent)
        {
            if (this.m_battleEquipSystem != null)
            {
                this.m_battleEquipSystem.OnEquipTypeListSelect(uiEvent);
            }
        }

        private void OnBattleEuipItemSelect(CUIEvent uiEvent)
        {
            if (this.m_battleEquipSystem != null)
            {
                this.m_battleEquipSystem.OnEquipItemSelect(uiEvent);
            }
        }

        private void OnBattleHeroSkillTipOpen(SkillSlot skillSlot, Vector3 Pos)
        {
            if (null != this.m_FormScript)
            {
                if (this.skillTipDesc == null)
                {
                    this.skillTipDesc = Utility.FindChild(this.m_FormScript.gameObject, "Panel_SkillTip");
                    if (this.skillTipDesc == null)
                    {
                        return;
                    }
                }
                if (skillSlot != null)
                {
                    PoolObjHandle<ActorRoot> captain = Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain;
                    if (captain != 0)
                    {
                        IHeroData data = CHeroDataFactory.CreateHeroData((uint) captain.handle.TheActorMeta.ConfigId);
                        Text component = this.skillTipDesc.transform.Find("skillNameText").GetComponent<Text>();
                        component.text = StringHelper.UTF8BytesToString(ref skillSlot.SkillObj.cfgData.szSkillName);
                        Text text2 = this.skillTipDesc.transform.Find("SkillDescribeText").GetComponent<Text>();
                        ValueDataInfo[] actorValue = captain.handle.ValueComponent.mActorValue.GetActorValue();
                        if ((text2 != null) && (skillSlot.SkillObj.cfgData.szSkillDesc.Length > 0))
                        {
                            text2.text = CUICommonSystem.GetSkillDesc(skillSlot.SkillObj.cfgData.szSkillDesc, actorValue, skillSlot.GetSkillLevel());
                        }
                        Text text3 = this.skillTipDesc.transform.Find("SkillCDText").GetComponent<Text>();
                        string[] args = new string[] { (skillSlot.GetSkillCDMax() / 0x3e8).ToString() };
                        text3.text = Singleton<CTextManager>.instance.GetText("Skill_Cool_Down_Tips", args);
                        string[] textArray2 = new string[] { skillSlot.NextSkillEnergyCostTotal().ToString() };
                        text3.transform.Find("SkillEnergyCostText").GetComponent<Text>().text = Singleton<CTextManager>.instance.GetText("Skill_Energy_Cost_Tips", textArray2);
                        uint[] skillEffectType = skillSlot.SkillObj.cfgData.SkillEffectType;
                        GameObject gameObject = null;
                        for (int i = 1; i <= 2; i++)
                        {
                            gameObject = component.transform.Find(string.Format("EffectNode{0}", i)).gameObject;
                            if ((i <= skillEffectType.Length) && (skillEffectType[i - 1] != 0))
                            {
                                gameObject.CustomSetActive(true);
                                gameObject.GetComponent<Image>().SetSprite(CSkillData.GetEffectSlotBg((SkillEffectType) skillEffectType[i - 1]), this.m_FormScript, true, false, false);
                                gameObject.transform.Find("Text").GetComponent<Text>().text = CSkillData.GetEffectDesc((SkillEffectType) skillEffectType[i - 1]);
                            }
                            else
                            {
                                gameObject.CustomSetActive(false);
                            }
                        }
                        Vector3 vector = Pos;
                        vector.x -= 4f;
                        vector.y += 4f;
                        this.skillTipDesc.transform.position = vector;
                        this.skillTipDesc.CustomSetActive(true);
                        this.m_isSkillDecShow = true;
                    }
                }
            }
        }

        private void OnBattleLearnSkillButtonClick(CUIEvent uiEvent)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            if (hostPlayer != null)
            {
                PoolObjHandle<ActorRoot> captain = hostPlayer.Captain;
                if ((captain != 0) && (((uiEvent != null) && (uiEvent.m_srcWidget != null)) && ((uiEvent.m_srcWidget.transform != null) && (uiEvent.m_srcWidget.transform.parent != null))))
                {
                    string name = uiEvent.m_srcWidget.transform.parent.name;
                    int index = int.Parse(name.Substring(name.Length - 1));
                    if ((index >= 1) && (index <= 3))
                    {
                        byte bSkillLvl = 0;
                        if ((captain.handle.SkillControl != null) && (captain.handle.SkillControl.SkillSlotArray[index] != null))
                        {
                            bSkillLvl = (byte) captain.handle.SkillControl.SkillSlotArray[index].GetSkillLevel();
                        }
                        this.SendLearnSkillCommand(captain, (SkillSlotType) index, bSkillLvl);
                        Singleton<CSoundManager>.GetInstance().PostEvent("UI_junei_ani_jinengxuexi", null);
                        Transform transform = uiEvent.m_srcWidget.transform.parent.Find("LearnEffect");
                        if (transform != null)
                        {
                            transform.gameObject.CustomSetActive(true);
                            CUIAnimationScript component = transform.gameObject.GetComponent<CUIAnimationScript>();
                            if (component != null)
                            {
                                component.PlayAnimation("Battle_UI_Skill_01", true);
                            }
                        }
                    }
                }
            }
        }

        private void OnBattleOpenMic(CUIEvent uiEvent)
        {
            if (this.m_OpenMicTipObj != null)
            {
                if (!this.m_bOpenSpeak)
                {
                    this.m_OpenMicTipText.text = MonoSingleton<VoiceSys>.GetInstance().m_Voice_Battle_FIrstOPenSpeak;
                    this.m_OpenMicTipObj.gameObject.CustomSetActive(true);
                    Singleton<CTimerManager>.instance.ResumeTimer(this.m_VoiceMictime);
                }
                else
                {
                    if (!CFakePvPHelper.bInFakeSelect && MonoSingleton<VoiceSys>.GetInstance().IsBattleSupportVoice())
                    {
                        if (!MonoSingleton<VoiceSys>.GetInstance().GlobalVoiceSetting)
                        {
                            this.m_OpenMicTipText.text = MonoSingleton<VoiceSys>.GetInstance().m_Voice_Server_Not_Open_Tips;
                            this.m_OpenMicTipObj.gameObject.CustomSetActive(true);
                            Singleton<CTimerManager>.instance.ResumeTimer(this.m_VoiceMictime);
                            return;
                        }
                        if (!MonoSingleton<VoiceSys>.GetInstance().IsInVoiceRoom())
                        {
                            this.m_OpenMicTipText.text = MonoSingleton<VoiceSys>.GetInstance().m_Voice_Cannot_JoinVoiceRoom;
                            this.m_OpenMicTipObj.gameObject.CustomSetActive(true);
                            Singleton<CTimerManager>.instance.ResumeTimer(this.m_VoiceMictime);
                            return;
                        }
                    }
                    if (this.m_bOpenMic)
                    {
                        this.m_OpenMicTipText.text = MonoSingleton<VoiceSys>.GetInstance().m_Voice_Battle_CloseMic;
                        MonoSingleton<VoiceSys>.GetInstance().CloseMic();
                        if (this.m_OpenMicObj != null)
                        {
                            CUIUtility.SetImageSprite(this.m_OpenMicObj.GetComponent<Image>(), this.no_microphone_path, null, true, false, false);
                        }
                    }
                    else
                    {
                        this.m_OpenMicTipText.text = MonoSingleton<VoiceSys>.GetInstance().m_Voice_Battle_OpenMic;
                        MonoSingleton<VoiceSys>.GetInstance().OpenMic();
                        if (this.m_OpenMicObj != null)
                        {
                            CUIUtility.SetImageSprite(this.m_OpenMicObj.GetComponent<Image>(), this.microphone_path, null, true, false, false);
                        }
                    }
                    this.m_bOpenMic = !this.m_bOpenMic;
                    this.m_OpenMicTipObj.gameObject.CustomSetActive(true);
                    Singleton<CTimerManager>.instance.ResumeTimer(this.m_VoiceMictime);
                }
            }
        }

        private void OnBattleOpenSpeaker(CUIEvent uiEvent)
        {
            if (this.m_OpenSpeakerTipObj != null)
            {
                if (!CFakePvPHelper.bInFakeSelect && MonoSingleton<VoiceSys>.GetInstance().IsBattleSupportVoice())
                {
                    if (!MonoSingleton<VoiceSys>.GetInstance().GlobalVoiceSetting)
                    {
                        this.m_OpenSpeakerTipText.text = MonoSingleton<VoiceSys>.GetInstance().m_Voice_Server_Not_Open_Tips;
                        this.m_OpenSpeakerTipObj.gameObject.CustomSetActive(true);
                        Singleton<CTimerManager>.instance.ResumeTimer(this.m_Vocetimer);
                        return;
                    }
                    if (!MonoSingleton<VoiceSys>.GetInstance().IsInVoiceRoom())
                    {
                        this.m_OpenSpeakerTipText.text = MonoSingleton<VoiceSys>.GetInstance().m_Voice_Cannot_JoinVoiceRoom;
                        this.m_OpenSpeakerTipObj.gameObject.CustomSetActive(true);
                        Singleton<CTimerManager>.instance.ResumeTimer(this.m_Vocetimer);
                        return;
                    }
                }
                if (this.m_bOpenSpeak)
                {
                    this.m_OpenSpeakerTipText.text = MonoSingleton<VoiceSys>.GetInstance().m_Voice_Battle_CloseSpeaker;
                    MonoSingleton<VoiceSys>.GetInstance().ClosenSpeakers();
                    MonoSingleton<VoiceSys>.GetInstance().CloseMic();
                    this.m_bOpenMic = false;
                    if (this.m_OpenSpeakerObj != null)
                    {
                        CUIUtility.SetImageSprite(this.m_OpenSpeakerObj.GetComponent<Image>(), this.no_voiceIcon_path, null, true, false, false);
                    }
                    if (this.m_OpenMicObj != null)
                    {
                        CUIUtility.SetImageSprite(this.m_OpenMicObj.GetComponent<Image>(), this.no_microphone_path, null, true, false, false);
                    }
                }
                else
                {
                    this.m_OpenSpeakerTipText.text = MonoSingleton<VoiceSys>.GetInstance().m_Voice_Battle_OpenSpeaker;
                    MonoSingleton<VoiceSys>.GetInstance().OpenSpeakers();
                    if (this.m_OpenSpeakerObj != null)
                    {
                        CUIUtility.SetImageSprite(this.m_OpenSpeakerObj.GetComponent<Image>(), this.voiceIcon_path, null, true, false, false);
                    }
                }
                this.m_bOpenSpeak = !this.m_bOpenSpeak;
                this.m_OpenSpeakerTipObj.gameObject.CustomSetActive(true);
                Singleton<CTimerManager>.instance.ResumeTimer(this.m_Vocetimer);
            }
            if (this.m_OpeankSpeakAnim != null)
            {
                this.m_OpeankSpeakAnim.gameObject.CustomSetActive(false);
            }
        }

        public void OnBattlePauseGame(CUIEvent uiEvent)
        {
            Singleton<CBattleGuideManager>.instance.PauseGame(this, true);
            if (this.m_FormScript != null)
            {
                this.m_FormScript.GetWidget(0x27).CustomSetActive(false);
                this.m_FormScript.GetWidget(40).CustomSetActive(true);
                this.m_FormScript.GetWidget(0x29).CustomSetActive(true);
            }
        }

        public void OnBattleResumeGame(CUIEvent uiEvent)
        {
            Singleton<CBattleGuideManager>.instance.ResumeGame(this);
            if (this.m_FormScript != null)
            {
                this.m_FormScript.GetWidget(40).CustomSetActive(false);
                this.m_FormScript.GetWidget(0x27).CustomSetActive(true);
                this.m_FormScript.GetWidget(0x29).CustomSetActive(false);
            }
        }

        private void OnBattleSkillBtnHold(CUIEvent uiEvent)
        {
            if (!this.m_isSkillDecShow && !this.m_skillButtonManager.CurrentSkillTipsResponed)
            {
                this.OnBattleSkillDecShow(uiEvent);
            }
            else if (this.m_isSkillDecShow && this.m_skillButtonManager.CurrentSkillTipsResponed)
            {
                this.HideSkillDescInfo();
            }
        }

        private void OnBattleSkillButtonDown(CUIEvent uiEvent)
        {
            if (this.m_signalPanel != null)
            {
                this.m_signalPanel.CancelSelectedSignalButton();
            }
            stUIEventParams par = new stUIEventParams();
            par = uiEvent.m_eventParams;
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Newbie_CloseSkillGesture, par);
            this.m_skillButtonManager.SkillButtonDown(uiEvent.m_srcFormScript, uiEvent.m_eventParams.m_skillSlotType, uiEvent.m_pointerEventData.position);
        }

        private void OnBattleSkillButtonDragged(CUIEvent uiEvent)
        {
            this.m_skillButtonManager.DragSkillButton(uiEvent.m_srcFormScript, uiEvent.m_eventParams.m_skillSlotType, uiEvent.m_pointerEventData.position);
        }

        private void OnBattleSkillButtonHoldEnd(CUIEvent uiEvent)
        {
            if (this.m_isSkillDecShow)
            {
                this.HideSkillDescInfo();
                this.m_skillButtonManager.SkillButtonUp(uiEvent.m_srcFormScript, uiEvent.m_eventParams.m_skillSlotType, false);
            }
        }

        private void OnBattleSkillButtonUp(CUIEvent uiEvent)
        {
            this.m_skillButtonManager.SkillButtonUp(uiEvent.m_srcFormScript, uiEvent.m_eventParams.m_skillSlotType, true);
        }

        private void OnBattleSkillDecShow(CUIEvent uiEvent)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if (((hostPlayer != null) && (hostPlayer.Captain != 0)) && ((hostPlayer.Captain.handle.ActorControl == null) || !hostPlayer.Captain.handle.ActorControl.IsDeadState))
            {
                int num;
                SkillSlot slot;
                string name = uiEvent.m_srcWidget.transform.parent.name;
                Vector3 position = uiEvent.m_srcWidget.transform.parent.transform.position;
                if (!int.TryParse(name.Substring(name.Length - 1), out num))
                {
                    name = uiEvent.m_srcWidget.transform.name;
                    position = uiEvent.m_srcWidget.transform.position;
                    if (!int.TryParse(name.Substring(name.Length - 1), out num))
                    {
                        return;
                    }
                }
                SkillSlotType type = (SkillSlotType) num;
                if (hostPlayer.Captain.handle.SkillControl.TryGetSkillSlot(type, out slot))
                {
                    string str2 = Utility.UTF8Convert(slot.SkillObj.cfgData.szSkillDesc);
                    this.OnBattleHeroSkillTipOpen(slot, position);
                }
            }
        }

        private void OnBattleSkillDisableAlert(CUIEvent uiEvent)
        {
            this.m_skillButtonManager.OnBattleSkillDisableAlert(uiEvent.m_eventParams.m_skillSlotType);
        }

        private void OnBattleSkillDisableBtnDown(CUIEvent uiEvent)
        {
            this.OnBattleSkillDecShow(uiEvent);
        }

        private void OnBattleSkillDisableBtnUp(CUIEvent uiEvent)
        {
            this.HideSkillDescInfo();
        }

        private void OnBattleSkillLevelUpEffectPlayEnd(CUIEvent uiEvent)
        {
            uiEvent.m_srcWidget.CustomSetActive(false);
        }

        private void OnBigMap_Open_BigMap(CUIEvent uievent)
        {
        }

        private void OnBuffSkillBtnDown(CUIEvent uiEvent)
        {
            if (uiEvent.m_srcWidget != null)
            {
                int index = int.Parse(uiEvent.m_srcWidget.name.Substring(uiEvent.m_srcWidget.name.IndexOf("_") + 1));
                if ((index < 5) && ((this.m_ArrShowBuffSkill != null) && (index < this.m_ArrShowBuffSkill.Length)))
                {
                    this.m_selectBuff = this.m_ArrShowBuffSkill[index].stBuffSkill;
                    if (this.m_selectBuff != 0)
                    {
                        uint iCfgID = (uint) this.m_selectBuff.handle.cfgData.iCfgID;
                        this.ShowBuffSkillDesc(iCfgID, uiEvent.m_srcWidget.transform.localPosition);
                    }
                }
            }
        }

        private void OnBuffSkillBtnUp(CUIEvent uiEvent)
        {
            this.m_selectBuff.Release();
            this.m_BuffDescNodeObj.CustomSetActive(false);
        }

        public void OnCancelTrusteeship(CUIEvent uiEvent)
        {
            this.SendTrusteeshipResult(uiEvent.m_eventParams.commonUInt32Param1, ACCEPT_AIPLAYER_RSP.ACCEPT_AIPLAYER_NO);
        }

        private void onChangeOperateMode(CUIEvent uiEvent)
        {
        }

        private void OnClearLockTarget(ref LockTargetEventParam prm)
        {
            this.HideHeroInfoPanel();
        }

        private void OnClearTarget(ref SelectTargetEventParam prm)
        {
            this.HideHeroInfoPanel();
        }

        private void OnClickBattleScene(CUIEvent uievent)
        {
            Singleton<LockModeScreenSelector>.GetInstance().OnClickBattleScene(uievent.m_pointerEventData.position);
        }

        public void onClickBoard(CUIEvent uiEvent)
        {
            if (this.battleStatView != null)
            {
                this.battleStatView.Toggle();
            }
        }

        public void OnCloseHeorInfoPanel(CUIEvent uiEvent)
        {
            Singleton<CBattleHeroInfoPanel>.GetInstance().Hide();
        }

        private void OnCommon_BattleShowOrHideWifiInfo(CUIEvent uiEvent)
        {
            GameObject widget = this.m_FormScript.GetWidget(0x25);
            DebugHelper.Assert(widget != null);
            if (widget != null)
            {
                widget.CustomSetActive(!widget.activeInHierarchy);
            }
        }

        private void OnCommon_BattleWifiCheckTimer(CUIEvent uiEvent)
        {
            if (this.m_FormScript != null)
            {
                GameObject widget = this.m_FormScript.GetWidget(0x23);
                GameObject obj3 = this.m_FormScript.GetWidget(0x24);
                GameObject obj4 = this.m_FormScript.GetWidget(0x25);
                DebugHelper.Assert(((widget != null) && (obj3 != null)) && (obj4 != null));
                int num = (GameSettings.FpsShowType != 1) ? Singleton<FrameSynchr>.GetInstance().GameSvrPing : Singleton<FrameSynchr>.instance.RealSvrPing;
                num = (num <= 100) ? num : ((((num - 100) * 7) / 10) + 100);
                num = Mathf.Clamp(num, 0, 460);
                uint index = 0;
                if (num == 0)
                {
                    num = 10;
                    SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
                    if (((curLvelContext != null) && curLvelContext.isWarmBattle) && (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_COMBAT))
                    {
                        int num3 = UnityEngine.Random.Range(0, 10);
                        if (num3 == 0)
                        {
                            num = 50 + UnityEngine.Random.Range(0, 100);
                        }
                        else if (num3 < 3)
                        {
                            num = 50 + UnityEngine.Random.Range(0, 50);
                        }
                        else if (num3 < 6)
                        {
                            num = 50 + UnityEngine.Random.Range(0, 30);
                        }
                        else
                        {
                            num = 50 + UnityEngine.Random.Range(0, 15);
                        }
                    }
                }
                if (num < 100)
                {
                    index = 2;
                }
                else if (num < 200)
                {
                    index = 1;
                }
                else
                {
                    index = 0;
                }
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    CUICommonSystem.PlayAnimator(widget, SysEntryForm.s_noNetStateName);
                }
                else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
                {
                    CUICommonSystem.PlayAnimator(widget, SysEntryForm.s_wifiStateName[index]);
                }
                else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
                {
                    CUICommonSystem.PlayAnimator(widget, SysEntryForm.s_netStateName[index]);
                }
                if (((obj4 != null) && obj4.activeInHierarchy) && (obj3 != null))
                {
                    Text component = obj3.transform.GetComponent<Text>();
                    if (component != null)
                    {
                        component.text = string.Format("{0}ms", num);
                    }
                }
            }
        }

        private void onConfirmReturnLobby(CUIEvent uiEvent)
        {
            Singleton<CUIManager>.GetInstance().CloseForm(CSettingsSys.SETTING_FORM_BATTLE);
            if ((Singleton<BattleLogic>.GetInstance().GetCurLvelContext().GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_GUIDE) && !Singleton<CBattleGuideManager>.instance.bTrainingAdv)
            {
                Singleton<CUIManager>.GetInstance().OpenTips(Singleton<CTextManager>.GetInstance().GetText("Tutorial_Level_Qiut_Tip"), false, 1f, null, new object[0]);
            }
            else
            {
                if (Singleton<CBattleGuideManager>.instance.bPauseGame)
                {
                    Singleton<CBattleGuideManager>.instance.ResumeGame(this);
                }
                if (Singleton<LobbyLogic>.instance.inMultiGame)
                {
                    Singleton<LobbyLogic>.instance.ReqMultiGameRunaway();
                }
                else
                {
                    Singleton<BattleLogic>.instance.DoFightOver(false);
                    Singleton<BattleLogic>.instance.SingleReqLoseGame();
                }
            }
        }

        private void OnDragMiniMap(CUIEvent uiEvent)
        {
            if (((uiEvent != null) && (this.m_FormScript != null)) && ((uiEvent.m_pointerEventData != null) && (uiEvent.m_srcWidget != null)))
            {
                MonoSingleton<CameraSystem>.instance.ToggleFreeDragCamera(true);
                Vector2 position = uiEvent.m_pointerEventData.position;
                Vector2 vector2 = CUIUtility.WorldToScreenPoint(this.m_FormScript.GetCamera(), uiEvent.m_srcWidget.transform.position);
                float num = position.x - vector2.x;
                float num2 = position.y - vector2.y;
                float x = num * Singleton<CBattleSystem>.instance.UI_world_Factor_Small.x;
                float z = num2 * Singleton<CBattleSystem>.instance.UI_world_Factor_Small.y;
                if (MonoSingleton<CameraSystem>.instance.MobaCamera != null)
                {
                    MonoSingleton<CameraSystem>.instance.MobaCamera.SetAbsoluteLockLocation(new Vector3(x, 1f, z));
                }
            }
        }

        private void OnDragMiniMapEnd(CUIEvent uievent)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if ((((hostPlayer != null) && (hostPlayer.Captain != 0)) && ((hostPlayer.Captain.handle != null) && (hostPlayer.Captain.handle.ActorControl != null))) && !hostPlayer.Captain.handle.ActorControl.IsDeadState)
            {
                MonoSingleton<CameraSystem>.instance.ToggleFreeDragCamera(false);
            }
        }

        public void OnDragonTipFormClose(CUIEvent cuiEvent)
        {
            Singleton<CUIManager>.instance.CloseForm(s_battleDragonTipForm);
        }

        public void OnDragonTipFormOpen(CUIEvent cuiEvent)
        {
            CUIFormScript script = Singleton<CUIManager>.instance.OpenForm(s_battleDragonTipForm, false, true);
            Text component = Utility.FindChild(script.gameObject, "DragonBuffx1Text").GetComponent<Text>();
            Text text2 = Utility.FindChild(script.gameObject, "DragonBuffx2Text").GetComponent<Text>();
            Text text3 = Utility.FindChild(script.gameObject, "DragonBuffx3Text").GetComponent<Text>();
            ResSkillCombineCfgInfo dataByKey = GameDataMgr.skillCombineDatabin.GetDataByKey(Singleton<BattleLogic>.instance.GetDragonBuffId(RES_SKILL_SRC_TYPE.RES_SKILL_SRC_DRAGON_ONE));
            ResSkillCombineCfgInfo info2 = GameDataMgr.skillCombineDatabin.GetDataByKey(Singleton<BattleLogic>.instance.GetDragonBuffId(RES_SKILL_SRC_TYPE.RES_SKILL_SRC_DRAGON_TWO));
            ResSkillCombineCfgInfo info3 = GameDataMgr.skillCombineDatabin.GetDataByKey(Singleton<BattleLogic>.instance.GetDragonBuffId(RES_SKILL_SRC_TYPE.RES_SKILL_SRC_DRAGON_THREE));
            if (dataByKey != null)
            {
                component.text = Utility.UTF8Convert(dataByKey.szSkillCombineDesc);
            }
            if (info2 != null)
            {
                text2.text = Utility.UTF8Convert(info2.szSkillCombineDesc);
            }
            if (info3 != null)
            {
                text3.text = Utility.UTF8Convert(info3.szSkillCombineDesc);
            }
        }

        private void OnDropCamera(CUIEvent uiEvent)
        {
            MonoSingleton<CameraSystem>.GetInstance().MoveCamera(-uiEvent.m_pointerEventData.delta.x, -uiEvent.m_pointerEventData.delta.y);
        }

        private void OnFloatTextAnimEnd(CUIEvent uiEvent)
        {
            this.m_battleFloatDigitManager.ClearBattleFloatText(uiEvent.m_srcWidgetScript as CUIAnimatorScript);
        }

        private void OnGameSettingCommonAttackTypeChange(CommonAttactType byAtkType)
        {
            if (Singleton<BattleLogic>.instance.isRuning)
            {
                this.SetCommonAttackTargetEvent(byAtkType);
                this.m_skillButtonManager.SetCommonAtkBtnState(byAtkType);
            }
        }

        private void onHeroEnergyChange(PoolObjHandle<ActorRoot> actor, int curVal, int maxVal)
        {
            if (this.m_FormScript != null)
            {
                Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
                if (hostPlayer != null)
                {
                    PoolObjHandle<ActorRoot> captain = hostPlayer.Captain;
                    if ((actor != 0) && (actor == captain))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int index = i + 1;
                            SkillSlot slot = actor.handle.SkillControl.SkillSlotArray[index];
                            SkillButton button = this.GetButton((SkillSlotType) index);
                            if ((slot != null) && slot.IsEnableSkillSlot())
                            {
                                if (!slot.IsEnergyEnough)
                                {
                                    if (!button.bDisableFlag)
                                    {
                                        this.m_skillButtonManager.SetEnergyDisableButton((SkillSlotType) index);
                                    }
                                }
                                else if (button.bDisableFlag)
                                {
                                    this.m_skillButtonManager.SetEnableButton((SkillSlotType) index);
                                }
                            }
                        }
                    }
                    this.OnHeroEpChange(actor, curVal, maxVal);
                }
            }
        }

        private void onHeroEnergyMax(PoolObjHandle<ActorRoot> actor, int curVal, int maxVal)
        {
            if (this.m_FormScript != null)
            {
                Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
                if (hostPlayer != null)
                {
                    PoolObjHandle<ActorRoot> captain = hostPlayer.Captain;
                    if ((actor != 0) && (actor == captain))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int index = i + 1;
                            SkillSlot slot = actor.handle.SkillControl.SkillSlotArray[index];
                            SkillButton button = this.GetButton((SkillSlotType) index);
                            if (((slot != null) && slot.IsEnableSkillSlot()) && button.bDisableFlag)
                            {
                                this.m_skillButtonManager.SetEnableButton((SkillSlotType) index);
                            }
                        }
                    }
                }
            }
        }

        private void OnHeroEpChange(PoolObjHandle<ActorRoot> hero, int iCurVal, int iMaxVal)
        {
            if ((this.m_selectedHero != 0) && (hero == this.m_selectedHero))
            {
                this.UpdateEpInfo();
            }
        }

        private void OnHeroEquipInBattleChanged(uint actorObjectID, stEquipInfo[] equips)
        {
            if (this.m_battleEquipSystem != null)
            {
                this.m_battleEquipSystem.HeroEquipChanged(actorObjectID, equips);
            }
        }

        private void OnHeroHpChange(PoolObjHandle<ActorRoot> hero, int iCurVal, int iMaxVal)
        {
            if ((this.m_selectedHero != 0) && (hero == this.m_selectedHero))
            {
                this.UpdateHpInfo();
            }
        }

        private void OnHeroRecommendEquipInit(uint actorObjectID)
        {
            if (this.m_battleEquipSystem != null)
            {
                this.m_battleEquipSystem.OnHeroRecommendEquipInit(actorObjectID);
            }
        }

        private void OnHeroSkillLvlup(PoolObjHandle<ActorRoot> hero, byte bSlotType, byte bSkillLevel)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            if (hostPlayer != null)
            {
                if (hostPlayer.Captain == hero)
                {
                    this.UpdateSkillLvlState(bSlotType, bSkillLevel);
                    this.CheckAndUpdateLearnSkill(hero);
                    if (bSkillLevel == 1)
                    {
                        Singleton<CBattleSystem>.GetInstance().ResetSkillButtonManager(hero);
                    }
                }
                if ((hero.handle.SkillControl.m_iSkillPoint > 0) && hero.handle.ActorAgent.IsAutoAI())
                {
                    this.AutoLearnSkill(hero);
                }
            }
        }

        private void OnHeroSoulLvlChange(PoolObjHandle<ActorRoot> hero, int level)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            if (hostPlayer != null)
            {
                PoolObjHandle<ActorRoot> captain = hostPlayer.Captain;
                if ((captain != 0) && (hero != 0))
                {
                    if (hero.handle.SkillControl != null)
                    {
                        int allSkillLevel = hero.handle.SkillControl.GetAllSkillLevel();
                        int num2 = level - allSkillLevel;
                        hero.handle.SkillControl.m_iSkillPoint = num2;
                    }
                    if ((hero.handle.ActorAgent != null) && !hero.handle.ActorAgent.IsAutoAI())
                    {
                        if (captain == hero)
                        {
                            this.CheckAndUpdateLearnSkill(hero);
                        }
                    }
                    else
                    {
                        this.AutoLearnSkill(hero);
                    }
                }
            }
        }

        private void OnLeftAxisChanged(CUIEvent uiEvent)
        {
            CUIJoystickScript srcWidgetScript = uiEvent.m_srcWidgetScript as CUIJoystickScript;
            if (srcWidgetScript != null)
            {
                this.HandleMoveInput(srcWidgetScript.GetAxis(), enInputMode.UI);
            }
        }

        private void OnLockTarget(ref LockTargetEventParam prm)
        {
            this.ShowTargetInfoByTargetId(prm.lockTargetID);
        }

        private void OnMiniMap_Click_Down(CUIEvent uievent)
        {
            if (this.m_signalPanel == null)
            {
                this.Move_CameraToClickPosition(uievent);
            }
            else if (!this.m_signalPanel.IsUseSingalButton())
            {
                this.Move_CameraToClickPosition(uievent);
            }
        }

        private void OnMiniMap_Click_Up(CUIEvent uievent)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if ((((hostPlayer != null) && (hostPlayer.Captain != 0)) && ((hostPlayer.Captain.handle != null) && (hostPlayer.Captain.handle.ActorControl != null))) && !hostPlayer.Captain.handle.ActorControl.IsDeadState)
            {
                MonoSingleton<CameraSystem>.instance.ToggleFreeDragCamera(false);
            }
        }

        private void onMultiHashNotSync(CUIEvent uiEvent)
        {
            ConnectorParam connectionParam = Singleton<NetworkModule>.instance.gameSvr.GetConnectionParam();
            Singleton<GameBuilder>.instance.EndGame();
            if (connectionParam != null)
            {
                Singleton<NetworkModule>.instance.InitGameServerConnect(connectionParam);
            }
        }

        public void OnOpenHeorInfoPanel(CUIEvent uiEvent)
        {
            Singleton<CBattleHeroInfoPanel>.GetInstance().Show();
        }

        private void OnPlayerBlindess(ref ActorSkillEventParam _param)
        {
            if (_param.src != 0)
            {
                Singleton<CBattleSystem>.GetInstance().CreateOtherFloatText(enOtherFloatTextContent.Blindess, (Vector3) _param.src.handle.location, new string[0]);
            }
        }

        private void OnPlayerBuffChange(ref BuffChangeEventParam prm)
        {
            if ((this.m_isInBattle && (Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer().Captain == prm.target)) && ((prm.stBuffSkill != 0) && (prm.stBuffSkill.handle.cfgData.bIsShowBuff == 1)))
            {
                this.UpdateShowBuffList(ref prm.stBuffSkill, prm.bIsAdd);
                this.UpdateShowBuff();
            }
        }

        private void OnPlayerCancelLimitSkill(ref DefaultSkillEventParam _param)
        {
            if ((this.m_FormScript != null) && (_param.slot != SkillSlotType.SLOT_SKILL_0))
            {
                this.m_skillButtonManager.CancelLimitButton(_param.slot);
            }
        }

        private void OnPlayerEnergyShortage(ref ActorSkillEventParam _param)
        {
            if (_param.src != 0)
            {
                float time = Time.time;
                if ((time - this.timeEnergyShortage) > 2f)
                {
                    Singleton<CBattleSystem>.GetInstance().CreateOtherFloatText(enOtherFloatTextContent.InEnergyShortage, (Vector3) _param.src.handle.location, new string[0]);
                    this.timeEnergyShortage = time;
                }
            }
        }

        private void OnPlayerLimitSkill(ref DefaultSkillEventParam _param)
        {
            if ((this.m_FormScript != null) && (_param.slot != SkillSlotType.SLOT_SKILL_0))
            {
                this.m_skillButtonManager.SetLimitButton(_param.slot);
            }
        }

        private void OnPlayerNoSkillTarget(ref ActorSkillEventParam _param)
        {
            if (_param.src != 0)
            {
                float time = Time.time;
                if ((time - this.timeNoSkillTarget) > 2f)
                {
                    Singleton<CBattleSystem>.GetInstance().CreateOtherFloatText(enOtherFloatTextContent.NoTarget, (Vector3) _param.src.handle.location, new string[0]);
                    this.timeNoSkillTarget = time;
                }
            }
        }

        private void OnPlayerProtectDisappear(ref ActorSkillEventParam _param)
        {
            if (_param.src != 0)
            {
                Singleton<CBattleSystem>.GetInstance().CreateOtherFloatText(enOtherFloatTextContent.ShieldDisappear, (Vector3) _param.src.handle.location, new string[0]);
            }
        }

        private void OnPlayerSkillCDChanged(ref DefaultSkillEventParam _param)
        {
            if (this.m_FormScript != null)
            {
                this.m_skillButtonManager.UpdateButtonCD(_param.slot, _param.param);
            }
        }

        private void OnPlayerSkillCDEnd(ref DefaultSkillEventParam _param)
        {
            if (this.m_FormScript != null)
            {
                this.m_skillButtonManager.SetButtonCDOver(_param.slot, true);
            }
        }

        private void OnPlayerSkillCDStart(ref DefaultSkillEventParam _param)
        {
            if (this.m_FormScript != null)
            {
                this.m_skillButtonManager.SetButtonCDStart(_param.slot);
            }
        }

        private void OnPlayerSkillChanged(ref ChangeSkillEventParam _param)
        {
            if (this.m_FormScript != null)
            {
                this.m_skillButtonManager.ChangeSkill(_param.slot, ref _param);
            }
        }

        private void OnPlayerSkillCooldown(ref ActorSkillEventParam _param)
        {
            if (_param.src != 0)
            {
                float time = Time.time;
                if ((time - this.timeSkillCooldown) > 2f)
                {
                    Singleton<CBattleSystem>.GetInstance().CreateOtherFloatText(enOtherFloatTextContent.InCooldown, (Vector3) _param.src.handle.location, new string[0]);
                    this.timeSkillCooldown = time;
                }
            }
        }

        private void OnPlayerSkillDisable(ref DefaultSkillEventParam _param)
        {
            if (this.m_FormScript != null)
            {
                this.m_skillButtonManager.SetDisableButton(_param.slot);
            }
        }

        private void OnPlayerSkillEnable(ref DefaultSkillEventParam _param)
        {
            if (this.m_FormScript != null)
            {
                this.m_skillButtonManager.SetEnableButton(_param.slot);
            }
        }

        private void OnPlayerSpawnBuff(ref SpawnBuffEventParam _param)
        {
            if (_param.src != 0)
            {
                this.CreateRestrictFloatText((RESTRICT_TYPE) _param.showType, (Vector3) _param.src.handle.location);
            }
        }

        private void OnPlayerUpdateSkill(ref DefaultSkillEventParam _param)
        {
            if ((this.m_FormScript != null) && (_param.slot != SkillSlotType.SLOT_SKILL_0))
            {
                PoolObjHandle<ActorRoot> captain = Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain;
                this.m_skillButtonManager.Initialise(captain);
            }
        }

        public void onQuitAppClick()
        {
            SGameApplication.Quit();
        }

        private void onQuitGame(CUIEvent uiEvent)
        {
            Utility.FindChild(this.m_FormScript.gameObject, "SysMenu").CustomSetActive(false);
            SGameApplication.Quit();
        }

        private void onReturnGame(CUIEvent uiEvent)
        {
            Utility.FindChild(this.m_FormScript.gameObject, "SysMenu").CustomSetActive(false);
        }

        private void onReturnLobby(CUIEvent uiEvent)
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if (curLvelContext != null)
            {
                if (curLvelContext.IsPvpGameType())
                {
                    Singleton<CUIManager>.GetInstance().OpenTips(Singleton<CTextManager>.GetInstance().GetText("多人对战不能退出游戏。"), false, 1f, null, new object[0]);
                }
                else
                {
                    this.onConfirmReturnLobby(null);
                }
            }
        }

        private void OnReviveTimerChange(CUIEvent uiEvent)
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if ((this.m_FormScript != null) && (curLvelContext != null))
            {
                Text component = Utility.FindChild(this.m_FormScript.gameObject, "CameraDragPanel").transform.Find("panelDeadInfo/lblReviveTime").GetComponent<Text>();
                Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
                if (hostPlayer != null)
                {
                    PoolObjHandle<ActorRoot> captain = hostPlayer.Captain;
                    if (captain != 0)
                    {
                        float num = captain.handle.ActorControl.ReviveCooldown * 0.001f;
                        component.text = string.Format("{0}", Mathf.FloorToInt(num + 0.2f));
                    }
                }
            }
        }

        private void OnSelectTarget(ref SelectTargetEventParam prm)
        {
            this.ShowTargetInfoByTargetId(prm.commonAttackTargetID);
        }

        public void OnSwitchAutoAI(ref DefaultGameEventParam param)
        {
            if (((Singleton<GamePlayerCenter>.instance != null) && (Singleton<GamePlayerCenter>.instance.GetHostPlayer() != null)) && ((Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain != 0) && (param.src == Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain)))
            {
                GameObject obj2 = (this.m_FormScript == null) ? null : Utility.FindChild(this.m_FormScript.gameObject, "PanelBtn/ToggleAutoBtn");
                if (obj2 != null)
                {
                    Transform transform = obj2.transform.Find("imgAuto");
                    if (transform != null)
                    {
                        transform.gameObject.CustomSetActive(Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain.handle.ActorControl.m_isAutoAI);
                        MonoSingleton<DialogueProcessor>.GetInstance().bAutoNextPage = Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain.handle.ActorControl.m_isAutoAI;
                    }
                }
            }
        }

        public void OnSwitchHeroInfoPanel(CUIEvent uiEvent)
        {
        }

        private void OnTalent_BtnLearnClick(CUIEvent uiEvent)
        {
        }

        private void OnTalent_Close(CUIEvent uiEvent)
        {
        }

        private void OnTalent_ItemClick(CUIEvent uiEvent)
        {
        }

        private void OnTalent_Open(CUIEvent uiEvent)
        {
        }

        public void OnToggleAutoAI(CUIEvent uiEvent)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            if (((hostPlayer != null) && (hostPlayer.Captain != 0)) && (hostPlayer.Captain.handle.ActorControl != null))
            {
                FrameCommand<SwitchActorAutoAICommand> command = FrameCommandFactory.CreateFrameCommand<SwitchActorAutoAICommand>();
                command.cmdData.IsAutoAI = !hostPlayer.Captain.handle.ActorControl.m_isAutoAI ? ((sbyte) 1) : ((sbyte) 0);
                command.Send();
                uiEvent.m_srcWidget.gameObject.transform.Find("imgAuto").gameObject.CustomSetActive(!hostPlayer.Captain.handle.ActorControl.m_isAutoAI);
                MonoSingleton<DialogueProcessor>.GetInstance().bAutoNextPage = command.cmdData.IsAutoAI != 0;
            }
        }

        public void OnToggleFreeCamera(CUIEvent uiEvent)
        {
            MonoSingleton<CameraSystem>.instance.ToggleFreeCamera();
        }

        public void OnUpdateDragonUI(int delta)
        {
            if (this.m_dragonView != null)
            {
                this.m_dragonView.UpdateDragon(delta);
            }
        }

        private void onUseSkill(ref ActorSkillEventParam prm)
        {
            if (ActorHelper.IsHostCtrlActor(ref prm.src))
            {
                int configId = Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain.handle.TheActorMeta.ConfigId;
                SLOTINFO item = null;
                for (int i = 0; i < this.m_SkillSlotList.Count; i++)
                {
                    if (this.m_SkillSlotList[i].id == configId)
                    {
                        item = this.m_SkillSlotList[i];
                        break;
                    }
                }
                if (item == null)
                {
                    item = new SLOTINFO {
                        id = configId
                    };
                    this.m_SkillSlotList.Add(item);
                }
                item.m_SKillSlot[(int) prm.slot]++;
            }
        }

        private void OnVoiceMicTimeEnd(int timersequence)
        {
            if (this.m_OpenMicTipObj != null)
            {
                Singleton<CTimerManager>.instance.PauseTimer(this.m_VoiceMictime);
                Singleton<CTimerManager>.instance.ResetTimer(this.m_VoiceMictime);
                this.m_OpenMicTipObj.gameObject.CustomSetActive(false);
            }
        }

        private void OnVoiceTimeEnd(int timersequence)
        {
            if (this.m_OpenSpeakerTipObj != null)
            {
                Singleton<CTimerManager>.instance.PauseTimer(this.m_Vocetimer);
                Singleton<CTimerManager>.instance.ResetTimer(this.m_Vocetimer);
                this.m_OpenSpeakerTipObj.gameObject.CustomSetActive(false);
            }
        }

        private void OnVoiceTimeEndFirst(int timersequence)
        {
            if (this.m_OpenSpeakerTipObj != null)
            {
                Singleton<CTimerManager>.instance.PauseTimer(this.m_Vocetimer);
                Singleton<CTimerManager>.instance.ResetTimer(this.m_Vocetimer);
                this.m_OpenSpeakerTipObj.gameObject.CustomSetActive(false);
            }
            if (this.m_OpeankSpeakAnim != null)
            {
                this.m_OpeankSpeakAnim.gameObject.CustomSetActive(false);
            }
        }

        public void PostOpenForm()
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if (!curLvelContext.isPVPMode)
            {
                Utility.FindChild(this.m_FormScript.gameObject, "HeroHeadHud").CustomSetActive(true);
                this.heroHeadHud = Utility.FindChild(this.m_FormScript.gameObject, "HeroHeadHud").GetComponent<HeroHeadHud>();
                this.heroHeadHud.Init();
            }
            else
            {
                Utility.FindChild(this.m_FormScript.gameObject, "HeroHeadHud").CustomSetActive(false);
            }
            Singleton<CBattleSystem>.GetInstance().ResetSkillButtonManager(Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain);
            SLevelContext context2 = Singleton<BattleLogic>.instance.GetCurLvelContext();
            SkillButton button = this.GetButton(SkillSlotType.SLOT_SKILL_5);
            if (((context2 != null) && (button != null)) && (button.m_button != null))
            {
                if (Singleton<CFunctionUnlockSys>.instance.FucIsUnlock(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_ADDEDSKILL) && ((((context2.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_COMBAT) || (context2.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER)) || ((context2.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_MATCH) || (context2.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_ROOM))) || ((context2.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_ENTERTAINMENT) || (context2.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_REWARDMATCH))))
                {
                    button.m_button.CustomSetActive(true);
                }
                else
                {
                    button.m_button.CustomSetActive(false);
                    SkillButton skillButton = this.GetButton(SkillSlotType.SLOT_SKILL_4);
                    if ((skillButton != null) && (skillButton.m_button != null))
                    {
                        bool flag = false;
                        if (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_GUIDE)
                        {
                            if (curLvelContext.iLevelID == CBattleGuideManager.GuideLevelID5v5)
                            {
                                flag = true;
                            }
                        }
                        else
                        {
                            ResDT_LevelCommonInfo info = CLevelCfgLogicManager.FindLevelConfigMultiGame(curLvelContext.iLevelID);
                            if ((info != null) && (info.bMaxAcntNum == 10))
                            {
                                flag = true;
                            }
                        }
                        if (flag)
                        {
                            SkillButton button3 = this.GetButton(SkillSlotType.SLOT_SKILL_6);
                            if ((button3 != null) && (button3.m_button != null))
                            {
                                button3.m_button.transform.position = skillButton.m_button.transform.position;
                                this.m_skillButtonManager.ResetSkillIndicatorFixedPosition(button3);
                            }
                        }
                        skillButton.m_button.transform.position = button.m_button.transform.position;
                        this.m_skillButtonManager.ResetSkillIndicatorFixedPosition(skillButton);
                        if (this.m_FormScript.m_sgameGraphicRaycaster != null)
                        {
                            this.m_FormScript.m_sgameGraphicRaycaster.UpdateTiles();
                        }
                    }
                }
            }
            if ((curLvelContext != null) && (curLvelContext.iLevelID == 0x4e29))
            {
                SkillButton button4 = this.GetButton(SkillSlotType.SLOT_SKILL_4);
                GameObject widget = this.m_FormScript.GetWidget(70);
                if (((button4 != null) && (button4.m_button != null)) && (widget != null))
                {
                    button4.m_button.transform.position = widget.transform.position;
                    this.m_skillButtonManager.ResetSkillIndicatorFixedPosition(button4);
                    if (this.m_FormScript.m_sgameGraphicRaycaster != null)
                    {
                        this.m_FormScript.m_sgameGraphicRaycaster.UpdateTiles();
                    }
                }
            }
            if (this.m_OpeankBigMap != null)
            {
                MiniMapSysUT.RefreshMapPointerBig(this.m_OpeankBigMap.gameObject);
            }
            if (this.m_OpenMicObj != null)
            {
                MiniMapSysUT.RefreshMapPointerBig(this.m_OpenMicObj.gameObject);
            }
            if (this.m_OpenSpeakerObj != null)
            {
                MiniMapSysUT.RefreshMapPointerBig(this.m_OpenSpeakerObj.gameObject);
            }
        }

        public static void Preload(ref ActorPreloadTab preloadTab)
        {
            preloadTab.AddMesh(CUIParticleSystem.s_particleSkillBtnEffect_Path);
        }

        public void ResetSkillButtonManager(PoolObjHandle<ActorRoot> actor)
        {
            if (actor.handle != null)
            {
                this.m_skillButtonManager.Initialise(actor);
                Singleton<EventRouter>.GetInstance().BroadCastEvent("ResetSkillButtonManager");
            }
        }

        private void SendLearnSkillCommand(PoolObjHandle<ActorRoot> actor, SkillSlotType enmSkillSlotType, byte bSkillLvl)
        {
            FrameCommand<LearnSkillCommand> command = FrameCommandFactory.CreateFrameCommand<LearnSkillCommand>();
            command.cmdData.dwHeroID = actor.handle.ObjID;
            command.cmdData.bSkillLevel = bSkillLvl;
            command.cmdData.bSlotType = (byte) enmSkillSlotType;
            command.Send();
        }

        public void SendTrusteeshipResult(uint objID, ACCEPT_AIPLAYER_RSP trusteeshipRsp)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x415);
            msg.stPkgData.stIsAcceptAiPlayerRsp.dwAiPlayerObjID = objID;
            msg.stPkgData.stIsAcceptAiPlayerRsp.bResult = (byte) trusteeshipRsp;
            Singleton<NetworkModule>.GetInstance().SendGameMsg(ref msg, 0);
        }

        public void SetButtonHighLight(GameObject button, bool highLight)
        {
            this.m_skillButtonManager.SetButtonHighLight(button, highLight);
        }

        public void SetCommonAttackTargetEvent(CommonAttactType byAtkType)
        {
            if (byAtkType == CommonAttactType.Type1)
            {
                Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<LockTargetEventParam>(GameSkillEventDef.Event_LockTarget, new GameSkillEvent<LockTargetEventParam>(this.OnLockTarget));
                Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<LockTargetEventParam>(GameSkillEventDef.Event_ClearLockTarget, new GameSkillEvent<LockTargetEventParam>(this.OnClearLockTarget));
                Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<SelectTargetEventParam>(GameSkillEventDef.Event_SelectTarget, new GameSkillEvent<SelectTargetEventParam>(this.OnSelectTarget));
                Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<SelectTargetEventParam>(GameSkillEventDef.Event_ClearTarget, new GameSkillEvent<SelectTargetEventParam>(this.OnClearTarget));
            }
            else
            {
                Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<SelectTargetEventParam>(GameSkillEventDef.Event_SelectTarget, new GameSkillEvent<SelectTargetEventParam>(this.OnSelectTarget));
                Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<SelectTargetEventParam>(GameSkillEventDef.Event_ClearTarget, new GameSkillEvent<SelectTargetEventParam>(this.OnClearTarget));
                Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<LockTargetEventParam>(GameSkillEventDef.Event_LockTarget, new GameSkillEvent<LockTargetEventParam>(this.OnLockTarget));
                Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<LockTargetEventParam>(GameSkillEventDef.Event_ClearLockTarget, new GameSkillEvent<LockTargetEventParam>(this.OnClearLockTarget));
            }
        }

        public void SetDragonUINum(COM_PLAYERCAMP camp, byte num)
        {
            if (this.m_dragonView != null)
            {
                this.m_dragonView.SetDrgonNum(camp, num);
            }
        }

        public void SetFpsShowType(int showType)
        {
            if (this.m_FormScript != null)
            {
                bool bActive = showType == 1;
                this.m_FormScript.GetWidget(0x20).CustomSetActive(bActive);
                GameObject widget = this.m_FormScript.GetWidget(0x2c);
                if (widget != null)
                {
                    CUITimerScript component = widget.GetComponent<CUITimerScript>();
                    if (component != null)
                    {
                        component.m_onChangedIntervalTime = !bActive ? ((double) 2f) : ((double) 0.3f);
                    }
                }
            }
        }

        public void SetJoyStickMoveType(int moveType)
        {
            if (this.m_FormScript != null)
            {
                CUIJoystickScript component = this.m_FormScript.GetWidget(0x26).transform.GetComponent<CUIJoystickScript>();
                if (component != null)
                {
                    if ((moveType == 0) || CCheatSystem.IsJoystickForceMoveable())
                    {
                        component.m_isAxisMoveable = true;
                    }
                    else
                    {
                        component.m_isAxisMoveable = false;
                    }
                }
            }
        }

        public void SetJoyStickShowType(int showType)
        {
            if (this.m_FormScript != null)
            {
                if (showType == 0)
                {
                    this.m_skillButtonManager.SetSkillIndicatorMode(enSkillIndicatorMode.FixedPosition);
                }
                else
                {
                    this.m_skillButtonManager.SetSkillIndicatorMode(enSkillIndicatorMode.General);
                }
            }
        }

        public void SetLearnBtnHighLight(GameObject button, bool highLight)
        {
            this.m_skillButtonManager.SetlearnBtnHighLight(button, highLight);
        }

        private void SetSelectedHeroInfo(PoolObjHandle<ActorRoot> hero)
        {
            if (this.m_panelHeroInfo == null)
            {
                this.m_panelHeroInfo = Utility.FindChild(this.m_FormScript.gameObject, "PanelHeroInfo");
            }
            if ((this.m_panelHeroInfo != null) && (hero != 0))
            {
                if (this.m_objHeroHead == null)
                {
                    this.m_objHeroHead = Utility.FindChild(this.m_panelHeroInfo, "HeroHead");
                }
                if (this.m_objHeroHead != null)
                {
                    uint configId = (uint) hero.handle.TheActorMeta.ConfigId;
                    KillDetailInfo detail = new KillDetailInfo {
                        Killer = hero
                    };
                    KillInfo info2 = KillNotifyUT.Convert_DetailInfo_KillInfo(detail);
                    this.m_objHeroHead.transform.Find("imageIcon").GetComponent<Image>().SetSprite(info2.KillerImgSrc, Singleton<CBattleSystem>.GetInstance().m_FormScript, true, false, false);
                }
                Singleton<CBattleSelectTarget>.GetInstance().OpenForm(hero);
            }
        }

        public void ShowArenaTimer()
        {
            GameObject obj2 = Utility.FindChild(this.m_FormScript.gameObject, "ArenaTimer63s");
            if (obj2 != null)
            {
                Transform transform = obj2.transform.Find("Timer");
                if (transform != null)
                {
                    CUITimerScript component = transform.GetComponent<CUITimerScript>();
                    if (component != null)
                    {
                        component.ReStartTimer();
                    }
                }
                obj2.CustomSetActive(true);
            }
        }

        private void ShowBuffSkillDesc(uint uiBuffId, Vector3 BtnPos)
        {
            ResSkillCombineCfgInfo dataByKey = GameDataMgr.skillCombineDatabin.GetDataByKey(uiBuffId);
            if ((dataByKey != null) && !string.IsNullOrEmpty(dataByKey.szSkillCombineDesc))
            {
                if (this.m_BuffSkillPanel == null)
                {
                    this.m_BuffSkillPanel = Utility.FindChild(this.m_FormScript.gameObject, "BuffSkill");
                    if (this.m_BuffSkillPanel == null)
                    {
                        return;
                    }
                }
                if (this.m_BuffDescTxtObj == null)
                {
                    this.m_BuffDescTxtObj = Utility.FindChild(this.m_BuffDescNodeObj, "Text");
                    if (this.m_BuffDescTxtObj == null)
                    {
                        return;
                    }
                }
                Text component = this.m_BuffDescTxtObj.GetComponent<Text>();
                component.text = dataByKey.szSkillCombineDesc;
                float preferredHeight = component.preferredHeight;
                Vector2 sizeDelta = this.m_textBgImage.rectTransform.sizeDelta;
                preferredHeight += ((this.m_textBgImage.gameObject.transform.localPosition.y - component.gameObject.transform.localPosition.y) * 2f) + 10f;
                this.m_textBgImage.rectTransform.sizeDelta = new Vector2(sizeDelta.x, preferredHeight);
                Vector3 vector2 = BtnPos;
                RectTransform transform = this.m_BuffBtn0.GetComponent<RectTransform>();
                vector2.y += (((this.m_BuffBtn0.transform.localScale.y * transform.rect.height) / 2f) + (preferredHeight / 2f)) + 15f;
                this.m_BuffDescNodeObj.transform.localPosition = vector2;
                this.m_BuffDescNodeObj.CustomSetActive(true);
            }
        }

        public void ShowHeroInfoPanel(PoolObjHandle<ActorRoot> hero)
        {
            if (this.m_panelHeroInfo == null)
            {
                this.m_panelHeroInfo = Utility.FindChild(this.m_FormScript.gameObject, "PanelHeroInfo");
            }
            if (((this.m_panelHeroInfo != null) && (hero != 0)) && (hero.handle.ValueComponent != null))
            {
                this.m_selectedHero = hero;
                this.SetSelectedHeroInfo(hero);
                this.m_panelHeroInfo.CustomSetActive(true);
            }
        }

        private void ShowSkillDescInfo(string strSkillDesc, Vector3 Pos)
        {
            if (this.BuffDesc == null)
            {
                this.BuffDesc = Utility.FindChild(this.m_FormScript.gameObject, "SkillDesc");
                if (this.BuffDesc == null)
                {
                    return;
                }
            }
            GameObject obj2 = Utility.FindChild(this.BuffDesc, "Text");
            if (obj2 != null)
            {
                Text component = obj2.GetComponent<Text>();
                component.text = strSkillDesc;
                float preferredHeight = component.preferredHeight;
                Image componetInChild = Utility.GetComponetInChild<Image>(this.BuffDesc, "bg");
                if (componetInChild != null)
                {
                    Vector2 sizeDelta = componetInChild.rectTransform.sizeDelta;
                    preferredHeight += ((componetInChild.gameObject.transform.localPosition.y - component.gameObject.transform.localPosition.y) * 2f) + 10f;
                    componetInChild.rectTransform.sizeDelta = new Vector2(sizeDelta.x, preferredHeight);
                    RectTransform transform = this.BuffDesc.GetComponent<RectTransform>();
                    if (transform != null)
                    {
                        transform.sizeDelta = componetInChild.rectTransform.sizeDelta;
                    }
                    Vector3 vector2 = Pos;
                    vector2.x -= 4f;
                    vector2.y += 4f;
                    this.BuffDesc.transform.position = vector2;
                    this.BuffDesc.CustomSetActive(true);
                    this.m_isSkillDecShow = true;
                }
            }
        }

        public void ShowSysMenu(CUIEvent uiEvent)
        {
            Singleton<CUIParticleSystem>.instance.Hide(null);
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Settings_OpenBattleForm);
        }

        private void ShowTargetInfoByTargetId(uint uilockTargetID)
        {
            uint objID = uilockTargetID;
            PoolObjHandle<ActorRoot> actor = Singleton<GameObjMgr>.instance.GetActor(objID);
            if (actor != 0)
            {
                this.ShowHeroInfoPanel(actor);
            }
        }

        public void ShowTaskView(bool show)
        {
            if (this.battleTaskView != null)
            {
                this.battleTaskView.Visible = show;
            }
        }

        public void ShowWinLosePanel(bool bWin)
        {
            Singleton<WinLose>.GetInstance().ShowPanel(bWin);
        }

        public void StartCameraDrag()
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if ((this.m_FormScript != null) && (curLvelContext != null))
            {
                GameObject obj2 = Utility.FindChild(this.m_FormScript.gameObject, "CameraDragPanel");
                if (curLvelContext.isPVPMode)
                {
                    obj2.CustomSetActive(true);
                }
                if (curLvelContext.isPVPMode)
                {
                    PoolObjHandle<ActorRoot> captain = Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain;
                    if ((captain != 0) && (this.m_FormScript != null))
                    {
                        GameObject gameObject = obj2.transform.Find("panelDeadInfo").gameObject;
                        CUITimerScript component = gameObject.transform.Find("Timer").GetComponent<CUITimerScript>();
                        float time = captain.handle.ActorControl.ReviveCooldown * 0.001f;
                        component.SetTotalTime(time);
                        component.StartTimer();
                        gameObject.CustomSetActive(true);
                        this.OnReviveTimerChange(null);
                    }
                }
            }
        }

        public override void UnInit()
        {
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_OnOpenDragonTip, new CUIEventManager.OnUIEventHandler(this.OnDragonTipFormOpen));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_OnCloseDragonTip, new CUIEventManager.OnUIEventHandler(this.OnDragonTipFormClose));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<ChangeSkillEventParam>(GameSkillEventDef.Event_ChangeSkill, new GameSkillEvent<ChangeSkillEventParam>(this.OnPlayerSkillChanged));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_ChangeSkillCD, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerSkillCDChanged));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_SkillCDStart, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerSkillCDStart));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_SkillCDEnd, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerSkillCDEnd));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_EnableSkill, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerSkillEnable));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_DisableSkill, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerSkillDisable));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<SpawnBuffEventParam>(GameSkillEventDef.Event_SpawnBuff, new GameSkillEvent<SpawnBuffEventParam>(this.OnPlayerSpawnBuff));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<ActorSkillEventParam>(GameSkillEventDef.Event_ProtectDisappear, new GameSkillEvent<ActorSkillEventParam>(this.OnPlayerProtectDisappear));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<ActorSkillEventParam>(GameSkillEventDef.Event_SkillCooldown, new GameSkillEvent<ActorSkillEventParam>(this.OnPlayerSkillCooldown));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<ActorSkillEventParam>(GameSkillEventDef.Enent_EnergyShortage, new GameSkillEvent<ActorSkillEventParam>(this.OnPlayerEnergyShortage));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<ActorSkillEventParam>(GameSkillEventDef.Event_NoSkillTarget, new GameSkillEvent<ActorSkillEventParam>(this.OnPlayerNoSkillTarget));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<ActorSkillEventParam>(GameSkillEventDef.Event_Blindess, new GameSkillEvent<ActorSkillEventParam>(this.OnPlayerBlindess));
            Singleton<GameEventSys>.GetInstance().RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorImmune, new RefAction<DefaultGameEventParam>(this.OnActorImmune));
            Singleton<GameEventSys>.GetInstance().RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorHurtAbsorb, new RefAction<DefaultGameEventParam>(this.OnActorHurtAbsorb));
            Singleton<GameEventSys>.GetInstance().RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_AutoAISwitch, new RefAction<DefaultGameEventParam>(this.OnSwitchAutoAI));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_LimitSkill, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerLimitSkill));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<DefaultSkillEventParam>(GameSkillEventDef.Event_CancelLimitSkill, new GameSkillEvent<DefaultSkillEventParam>(this.OnPlayerCancelLimitSkill));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<ActorSkillEventParam>(GameSkillEventDef.Event_UseSkill, new GameSkillEvent<ActorSkillEventParam>(this.onUseSkill));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_BuffSkillBtn_Down, new CUIEventManager.OnUIEventHandler(this.OnBuffSkillBtnDown));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_BuffSkillBtn_Up, new CUIEventManager.OnUIEventHandler(this.OnBuffSkillBtnUp));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<BuffChangeEventParam>(GameSkillEventDef.Event_BuffChange, new GameSkillEvent<BuffChangeEventParam>(this.OnPlayerBuffChange));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int, int, bool>("HeroGoldCoinInBattleChange", new Action<PoolObjHandle<ActorRoot>, int, int, bool>(this, (IntPtr) this.OnActorGoldCoinInBattleChanged));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<uint, stEquipInfo[]>("HeroEquipInBattleChange", new Action<uint, stEquipInfo[]>(this, (IntPtr) this.OnHeroEquipInBattleChanged));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<uint>("HeroRecommendEquipInit", new Action<uint>(this.OnHeroRecommendEquipInit));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_OnAxisChanged, new CUIEventManager.OnUIEventHandler(this.OnLeftAxisChanged));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_LearnSkillBtn_Click, new CUIEventManager.OnUIEventHandler(this.OnBattleLearnSkillButtonClick));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_SkillLevelUpEffectPlayEnd, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillLevelUpEffectPlayEnd));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int>("HeroSoulLevelChange", new Action<PoolObjHandle<ActorRoot>, int>(this, (IntPtr) this.OnHeroSoulLvlChange));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, byte, byte>("HeroSkillLevelUp", new Action<PoolObjHandle<ActorRoot>, byte, byte>(this, (IntPtr) this.OnHeroSkillLvlup));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Trusteeship_Accept, new CUIEventManager.OnUIEventHandler(this.OnAcceptTrusteeship));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Trusteeship_Cancel, new CUIEventManager.OnUIEventHandler(this.OnCancelTrusteeship));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BattleEquip_Form_Open, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipFormOpen));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BattleEquip_Form_Close, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipFormClose));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BattleEquip_TypeList_Select, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipTypeListSelect));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BattleEquip_Item_Select, new CUIEventManager.OnUIEventHandler(this.OnBattleEuipItemSelect));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BattleEquip_BagItem_Select, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipBagItemSelect));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BattleEquip_BuyBtn_Click, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipBuy));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BattleEquip_SaleBtn_Click, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipSale));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BattleEquip_RecommendEquip_Buy, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipQuicklyBuy));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_EquipBoughtEffectPlayEnd, new CUIEventManager.OnUIEventHandler(this.OnBattleEquipBoughtEffectPlayEnd));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int, int>("HeroEnergyChange", new Action<PoolObjHandle<ActorRoot>, int, int>(this, (IntPtr) this.onHeroEnergyChange));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int, int>("HeroEnergyMax", new Action<PoolObjHandle<ActorRoot>, int, int>(this, (IntPtr) this.onHeroEnergyMax));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int, int>("HeroHpChange", new Action<PoolObjHandle<ActorRoot>, int, int>(this, (IntPtr) this.OnHeroHpChange));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_Disable_Down, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillDisableBtnDown));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_Disable_Up, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillDisableBtnUp));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_OnSkillButtonHold, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillBtnHold));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_OnSkillButtonHoldEnd, new CUIEventManager.OnUIEventHandler(this.OnBattleSkillButtonHoldEnd));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.VOICE_OpenSpeaker, new CUIEventManager.OnUIEventHandler(this.OnBattleOpenSpeaker));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.VOICE_OpenMic, new CUIEventManager.OnUIEventHandler(this.OnBattleOpenMic));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_MultiHashInvalid, new CUIEventManager.OnUIEventHandler(this.onMultiHashNotSync));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<CommonAttactType>(EventID.GAME_SETTING_COMMONATTACK_TYPE_CHANGE, new Action<CommonAttactType>(this.OnGameSettingCommonAttackTypeChange));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_Click_Scene, new CUIEventManager.OnUIEventHandler(this.OnClickBattleScene));
            if (GameSettings.TheCommonAttackType == CommonAttactType.Type1)
            {
                Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<SelectTargetEventParam>(GameSkillEventDef.Event_SelectTarget, new GameSkillEvent<SelectTargetEventParam>(this.OnSelectTarget));
                Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<SelectTargetEventParam>(GameSkillEventDef.Event_ClearTarget, new GameSkillEvent<SelectTargetEventParam>(this.OnClearTarget));
            }
            else
            {
                Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<LockTargetEventParam>(GameSkillEventDef.Event_LockTarget, new GameSkillEvent<LockTargetEventParam>(this.OnLockTarget));
                Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<LockTargetEventParam>(GameSkillEventDef.Event_ClearLockTarget, new GameSkillEvent<LockTargetEventParam>(this.OnClearLockTarget));
            }
        }

        public void Update()
        {
            if (this.m_isInBattle)
            {
                if (GameFramework.m_fFps > this.m_MaxBattleFPS)
                {
                    this.m_MaxBattleFPS = GameFramework.m_fFps;
                }
                if (this.m_MinBattleFPS > GameFramework.m_fFps)
                {
                    this.m_MinBattleFPS = GameFramework.m_fFps;
                }
                this.m_frameCount++;
                if (this.m_frameCount >= 50)
                {
                    this.m_BattleFPSCount++;
                    this.m_AveBattleFPS += GameFramework.m_fFps;
                    this.m_frameCount = 0;
                }
                if (this.scoreBoard != null)
                {
                    this.scoreBoard.Update();
                }
                if (this.scoreboardPvE != null)
                {
                    this.scoreboardPvE.Update();
                }
                if (this.soldierWaveView != null)
                {
                    this.soldierWaveView.Update();
                }
                if (this.m_signalPanel != null)
                {
                    this.m_signalPanel.Update();
                }
                if (this.m_soundChat != null)
                {
                    this.m_soundChat.Update();
                }
                if (Singleton<InBattleMsgMgr>.instance != null)
                {
                    Singleton<InBattleMsgMgr>.instance.Update();
                }
                if (this.m_battleEquipSystem != null)
                {
                    this.m_battleEquipSystem.Update();
                }
            }
        }

        public void UpdateAdValueInfo()
        {
            if (this.m_selectedHero != 0)
            {
                int totalValue = this.m_selectedHero.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue;
                if (this.m_AdTxt == null)
                {
                    GameObject obj2 = Utility.FindChild(this.m_panelHeroInfo, "InfoPanel/TxtPanel/AdTxt");
                    if (obj2 != null)
                    {
                        this.m_AdTxt = obj2.GetComponent<Text>();
                    }
                }
                if (this.m_AdTxt != null)
                {
                    this.m_AdTxt.text = totalValue.ToString();
                }
            }
        }

        public void UpdateApValueInfo()
        {
            if (this.m_selectedHero != 0)
            {
                int totalValue = this.m_selectedHero.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT].totalValue;
                if (this.m_ApTxt == null)
                {
                    GameObject obj2 = Utility.FindChild(this.m_panelHeroInfo, "InfoPanel/TxtPanel/ApTxt");
                    if (obj2 != null)
                    {
                        this.m_ApTxt = obj2.GetComponent<Text>();
                    }
                }
                if (this.m_ApTxt != null)
                {
                    this.m_ApTxt.text = totalValue.ToString();
                }
            }
        }

        private void UpdateBuffCD(int delta)
        {
            int num = 0;
            for (int i = 0; i < this.m_CurShowBuffCount; i++)
            {
                if ((((this.m_ArrShowBuffSkill != null) && (this.m_ArrShowBuffSkill[i].stBuffSkill != 0)) && ((num = this.m_ArrShowBuffSkill[i].iBufCD - delta) > 0)) && (this.m_ArrShowBuffSkill[i].stBuffSkill.handle.cfgData != null))
                {
                    int iDuration = this.m_ArrShowBuffSkill[i].stBuffSkill.handle.cfgData.iDuration;
                    if (iDuration != -1)
                    {
                        this.m_ArrShowBuffSkill[i].iBufCD = num;
                        string path = this.BufStringProvider.GetString(i);
                        GameObject obj2 = Utility.FindChild(this.m_BuffSkillPanel, path);
                        if (obj2 != null)
                        {
                            Image component = obj2.GetComponent<Image>();
                            if (component != null)
                            {
                                float num4 = ((float) num) / ((float) iDuration);
                                component.CustomFillAmount(num4);
                            }
                        }
                    }
                }
            }
        }

        public void UpdateEpInfo()
        {
            if (this.m_selectedHero != 0)
            {
                int actorEp = this.m_selectedHero.handle.ValueComponent.actorEp;
                int totalValue = this.m_selectedHero.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_PROPERTY_MAXEP].totalValue;
                if (this.m_EpImg == null)
                {
                    GameObject obj2 = Utility.FindChild(this.m_panelHeroInfo, "EpImg");
                    if (obj2 != null)
                    {
                        this.m_EpImg = obj2.GetComponent<Image>();
                    }
                }
                if (this.m_EpImg != null)
                {
                    float num3 = 0f;
                    if (totalValue > 0)
                    {
                        num3 = ((float) actorEp) / ((float) totalValue);
                    }
                    this.m_EpImg.CustomFillAmount(num3);
                }
            }
        }

        protected void UpdateFpsAndLag(CUIEvent uiEvent)
        {
            if (this.m_FormScript != null)
            {
                GameObject widget = this.m_FormScript.GetWidget(0x20);
                uint fFps = (uint) GameFramework.m_fFps;
                if (widget != null)
                {
                    Text component = widget.transform.FindChild("FPSText").gameObject.GetComponent<Text>();
                    component.text = string.Format("FPS {0}", fFps);
                    if (CheatCommandCommonEntry.CPU_CLOCK_ENABLE)
                    {
                        component.text = string.Format("FPS {0}\n{1}Mhz-{2}Mhz", fFps, Utility.GetCpuCurrentClock(), Utility.GetCpuMinClock());
                    }
                }
                this._lastFps = fFps;
            }
        }

        public void UpdateHpInfo()
        {
            if (this.m_selectedHero != 0)
            {
                int actorHp = this.m_selectedHero.handle.ValueComponent.actorHp;
                int totalValue = this.m_selectedHero.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP].totalValue;
                if (this.m_HpImg == null)
                {
                    GameObject obj2 = Utility.FindChild(this.m_panelHeroInfo, "HpImg");
                    if (obj2 != null)
                    {
                        this.m_HpImg = obj2.GetComponent<Image>();
                    }
                }
                if (this.m_HpImg != null)
                {
                    this.m_HpImg.CustomFillAmount(((float) actorHp) / ((float) totalValue));
                }
                if (this.m_HpTxt == null)
                {
                    GameObject obj3 = Utility.FindChild(this.m_panelHeroInfo, "HpTxt");
                    if (obj3 != null)
                    {
                        this.m_HpTxt = obj3.GetComponent<Text>();
                    }
                }
                if (this.m_HpTxt != null)
                {
                    string str = string.Format("{0}/{1}", actorHp, totalValue);
                    this.m_HpTxt.text = str;
                }
            }
        }

        private void UpdateLearnSkillBtnState(int iSkillSlotType, bool bIsShow)
        {
            SkillButton button = this.GetButton((SkillSlotType) iSkillSlotType);
            if (button != null)
            {
                GameObject learnSkillButton = button.GetLearnSkillButton();
                if (learnSkillButton != null)
                {
                    learnSkillButton.CustomSetActive(bIsShow);
                    Singleton<EventRouter>.GetInstance().BroadCastEvent<int, bool>("HeroSkillLearnButtonStateChange", iSkillSlotType, bIsShow);
                }
            }
        }

        public void UpdateLogic(int delta)
        {
            if (this.m_isInBattle)
            {
                this.m_skillButtonManager.UpdateLogic(delta);
                this.UpdateBuffCD(delta);
                if (this.m_battleEquipSystem != null)
                {
                    this.m_battleEquipSystem.UpdateLogic(delta);
                }
                Singleton<CBattleSelectTarget>.GetInstance().Update(this.m_selectedHero);
            }
        }

        public void UpdateMgcDefValueInfo()
        {
            if (this.m_selectedHero != 0)
            {
                int totalValue = this.m_selectedHero.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT].totalValue;
                GameObject obj2 = Utility.FindChild(this.m_panelHeroInfo, "InfoPanel/TxtPanel/MgcDefTxt");
                if (obj2 != null)
                {
                    this.m_MgcDefTxt = obj2.GetComponent<Text>();
                }
                if (this.m_MgcDefTxt != null)
                {
                    this.m_MgcDefTxt.text = totalValue.ToString();
                }
            }
        }

        public void UpdatePhyDefValueInfo()
        {
            if (this.m_selectedHero != 0)
            {
                int totalValue = this.m_selectedHero.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT].totalValue;
                if (this.m_PhyDefTxt == null)
                {
                    GameObject obj2 = Utility.FindChild(this.m_panelHeroInfo, "InfoPanel/TxtPanel/PhyDefTxt");
                    if (obj2 != null)
                    {
                        this.m_PhyDefTxt = obj2.GetComponent<Text>();
                    }
                }
                if (this.m_PhyDefTxt != null)
                {
                    this.m_PhyDefTxt.text = totalValue.ToString();
                }
            }
        }

        private void UpdateShowBuff()
        {
            if (this.m_BuffSkillPanel == null)
            {
                this.m_BuffSkillPanel = Utility.FindChild(this.m_FormScript.gameObject, "BuffSkill");
                if (this.m_BuffSkillPanel == null)
                {
                    return;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                string path = string.Format("BuffSkillBtn_{0}", i);
                GameObject p = Utility.FindChild(this.m_BuffSkillPanel, path);
                if (p == null)
                {
                    return;
                }
                if (i < this.m_CurShowBuffCount)
                {
                    if ((this.m_ArrShowBuffSkill[i].stBuffSkill != 0) && (this.m_ArrShowBuffSkill[i].stBuffSkill.handle.cfgData != null))
                    {
                        Image component = Utility.FindChild(p, "BuffImg").GetComponent<Image>();
                        GameObject prefab = CUIUtility.GetSpritePrefeb(string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Skill_Dir, Utility.UTF8Convert(this.m_ArrShowBuffSkill[i].stBuffSkill.handle.cfgData.szIconPath)), true, true);
                        component.SetSprite(prefab);
                        Image image = Utility.FindChild(p, "BuffImgMask").GetComponent<Image>();
                        image.SetSprite(prefab);
                        image.CustomFillAmount(0f);
                    }
                    p.CustomSetActive(true);
                }
                else
                {
                    p.CustomSetActive(false);
                }
            }
        }

        private void UpdateShowBuffList(ref PoolObjHandle<BuffSkill> stBuffSkill, bool bIsAdd)
        {
            if (((stBuffSkill != 0) && (stBuffSkill.handle.cfgData != null)) && (stBuffSkill.handle.cfgData.bIsShowBuff != 0))
            {
                if (!bIsAdd)
                {
                    for (int i = 0; i < this.m_CurShowBuffCount; i++)
                    {
                        if ((this.m_ArrShowBuffSkill[i].stBuffSkill != 0) && (this.m_ArrShowBuffSkill[i].stBuffSkill.handle.cfgData.iCfgID == stBuffSkill.handle.cfgData.iCfgID))
                        {
                            this.m_ArrShowBuffSkill[i].iCacheCount--;
                            if (this.m_ArrShowBuffSkill[i].iCacheCount == 0)
                            {
                                if ((((this.m_selectBuff != 0) && (this.m_selectBuff.handle.cfgData != null)) && ((this.m_ArrShowBuffSkill[i].stBuffSkill != 0) && (this.m_ArrShowBuffSkill[i].stBuffSkill.handle.cfgData != null))) && (this.m_selectBuff.handle.cfgData.iCfgID == this.m_ArrShowBuffSkill[i].stBuffSkill.handle.cfgData.iCfgID))
                                {
                                    this.m_BuffDescNodeObj.CustomSetActive(false);
                                }
                                this.m_CurShowBuffCount--;
                                for (int j = i; j < this.m_CurShowBuffCount; j++)
                                {
                                    this.m_ArrShowBuffSkill[j] = this.m_ArrShowBuffSkill[j + 1];
                                }
                            }
                            break;
                        }
                    }
                }
                else if (this.m_CurShowBuffCount == 0)
                {
                    this.m_ArrShowBuffSkill[0].stBuffSkill = stBuffSkill;
                    this.m_ArrShowBuffSkill[0].iCacheCount = 1;
                    this.m_ArrShowBuffSkill[0].iBufCD = stBuffSkill.handle.cfgData.iDuration;
                    this.m_CurShowBuffCount++;
                }
                else
                {
                    int index = 0;
                    bool flag = true;
                    int num2 = -1;
                    while (index < this.m_CurShowBuffCount)
                    {
                        DebugHelper.Assert(this.m_ArrShowBuffSkill[index].stBuffSkill == 1, "UpdateShowBuffList: bad m_ArrShowBuffSkill[i].stBuffSkill");
                        if ((this.m_ArrShowBuffSkill[index].stBuffSkill != 0) && (this.m_ArrShowBuffSkill[index].stBuffSkill.handle.cfgData.bShowBuffPriority > stBuffSkill.handle.cfgData.bShowBuffPriority))
                        {
                            for (int k = this.m_CurShowBuffCount; k > index; k--)
                            {
                                if (k != 5)
                                {
                                    this.m_ArrShowBuffSkill[k] = this.m_ArrShowBuffSkill[k - 1];
                                }
                            }
                            break;
                        }
                        if ((this.m_ArrShowBuffSkill[index].stBuffSkill != 0) && (this.m_ArrShowBuffSkill[index].stBuffSkill.handle.cfgData.bShowBuffPriority == stBuffSkill.handle.cfgData.bShowBuffPriority))
                        {
                            if (this.m_ArrShowBuffSkill[index].stBuffSkill.handle.cfgData.iCfgID != stBuffSkill.handle.cfgData.iCfgID)
                            {
                                goto Label_0293;
                            }
                            this.m_ArrShowBuffSkill[index].stBuffSkill = stBuffSkill;
                            this.m_ArrShowBuffSkill[index].iBufCD = stBuffSkill.handle.cfgData.iDuration;
                            this.m_ArrShowBuffSkill[index].iCacheCount++;
                            flag = false;
                            num2 = -1;
                            break;
                        }
                        if (this.m_ArrShowBuffSkill[index].stBuffSkill == 0)
                        {
                            num2 = index;
                            flag = false;
                        }
                        else if ((index >= (this.m_CurShowBuffCount - 1)) && (this.m_CurShowBuffCount == 5))
                        {
                            flag = false;
                        }
                    Label_0293:
                        index++;
                    }
                    if (flag && (index < 5))
                    {
                        this.m_ArrShowBuffSkill[index].stBuffSkill = stBuffSkill;
                        this.m_ArrShowBuffSkill[index].iBufCD = stBuffSkill.handle.cfgData.iDuration;
                        this.m_ArrShowBuffSkill[index].iCacheCount = 1;
                        if (this.m_CurShowBuffCount < 5)
                        {
                            this.m_CurShowBuffCount++;
                        }
                    }
                    else if ((!flag && (num2 >= 0)) && (num2 < this.m_CurShowBuffCount))
                    {
                        this.m_ArrShowBuffSkill[num2].stBuffSkill = stBuffSkill;
                        this.m_ArrShowBuffSkill[num2].iBufCD = stBuffSkill.handle.cfgData.iDuration;
                        this.m_ArrShowBuffSkill[num2].iCacheCount = 1;
                    }
                }
            }
        }

        private void UpdateSkillLvlState(int iSkillSlotType, int iSkillLvl)
        {
            SkillButton button = this.GetButton((SkillSlotType) iSkillSlotType);
            if (button != null)
            {
                GameObject skillLvlImg = button.GetSkillLvlImg(iSkillLvl);
                if (skillLvlImg != null)
                {
                    skillLvlImg.CustomSetActive(true);
                }
            }
        }

        public bool OpenSpeakInBattle
        {
            get
            {
                return this.m_bOpenSpeak;
            }
        }

        private class BuffCDString
        {
            private string[] CachedString = new string[4];

            public BuffCDString()
            {
                for (int i = 0; i < this.CachedString.Length; i++)
                {
                    this.CachedString[i] = this.QueryString(i);
                }
            }

            public string GetString(int Index)
            {
                if ((Index >= 0) && (Index < this.CachedString.Length))
                {
                    return this.CachedString[Index];
                }
                return this.QueryString(Index);
            }

            private string QueryString(int Index)
            {
                return string.Format("BuffSkillBtn_{0}/BuffImgMask", Index);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BuffInfo
        {
            public int iCacheCount;
            public PoolObjHandle<BuffSkill> stBuffSkill;
            public int iBufCD;
        }
    }
}

