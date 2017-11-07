namespace Assets.Scripts.GameSystem
{
    using Apollo;
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    internal class CSettingsSys : Singleton<CSettingsSys>
    {
        private CUIFormScript _form;
        private Slider _sensitivitySlider;
        private CUIListScript _tabList;
        private CSettingsSlider m_SliderFps;
        private CSettingsSlider m_SliderModelLOD;
        private CSettingsSlider m_SliderMusic;
        private CSettingsSlider m_SliderParticleLOD;
        private CSettingsSlider m_SliderSkillTip;
        private CSettingsSlider m_SliderSoundEffect;
        private CSettingsSlider m_SliderVoice;
        private static string SETTING_FORM = "UGUI/Form/System/Settings/Form_Settings.prefab";
        public static string SETTING_FORM_BATTLE = "UGUI/Form/System/Settings/Form_Settings_Battle.prefab";

        private void ChangeText(CUIListElementScript element, string InText)
        {
            DebugHelper.Assert(element != null);
            if (element != null)
            {
                Transform transform = element.gameObject.transform;
                DebugHelper.Assert(transform != null);
                Transform transform2 = transform.FindChild("Text");
                DebugHelper.Assert(transform2 != null);
                Text text = (transform2 == null) ? null : transform2.GetComponent<Text>();
                DebugHelper.Assert(text != null);
                if (text != null)
                {
                    text.text = InText;
                }
            }
        }

        public override void Init()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_OpenForm, new CUIEventManager.OnUIEventHandler(this.onOpenSetting));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_OpenBattleForm, new CUIEventManager.OnUIEventHandler(this.onOpenBattleSetting));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_ReqLogout, new CUIEventManager.OnUIEventHandler(this.onReqLogout));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_ConfirmLogout, new CUIEventManager.OnUIEventHandler(this.onConfirmLogout));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_SettingTypeChange, new CUIEventManager.OnUIEventHandler(this.OnSettingTabChange));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnCloseSetting));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_CameraHeight, new CUIEventManager.OnUIEventHandler(this.OnCameraHeightChange));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_PrivacyPolicy, new CUIEventManager.OnUIEventHandler(this.OnClickPrivacyPolicy));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_TermOfService, new CUIEventManager.OnUIEventHandler(this.OnClickTermOfService));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_Contract, new CUIEventManager.OnUIEventHandler(this.OnClickContract));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_UpdateTimer, new CUIEventManager.OnUIEventHandler(this.OnUpdateTimer));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_SurrenderCDReady, new CUIEventManager.OnUIEventHandler(this.OnSurrenderCDReady));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_ConfirmQuality_Accept, new CUIEventManager.OnUIEventHandler(this.onQualitySettingAccept));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Settings_ConfirmQuality_Cancel, new CUIEventManager.OnUIEventHandler(this.onQualitySettingCancel));
            this.m_SliderMusic = new CSettingsSlider(new SliderValueChanged(this.onSliderChange));
            this.m_SliderSoundEffect = new CSettingsSlider(new SliderValueChanged(this.onSliderChange));
            this.m_SliderModelLOD = new CSettingsSlider(new SliderValueChanged(this.onSliderChange));
            this.m_SliderParticleLOD = new CSettingsSlider(new SliderValueChanged(this.onSliderChange));
            this.m_SliderSkillTip = new CSettingsSlider(new SliderValueChanged(this.onSliderChange));
            this.m_SliderFps = new CSettingsSlider(new SliderValueChanged(this.onSliderChange));
            this.m_SliderVoice = new CSettingsSlider(new SliderValueChanged(this.onSliderChange));
        }

        private void initPanel(CUIFormScript form)
        {
            this.m_SliderMusic.initPanel(this._form.m_formWidgets[4], enSliderKind.Slider_Music);
            this.m_SliderSoundEffect.initPanel(this._form.m_formWidgets[5], enSliderKind.Slider_SoundEffect);
            this.m_SliderModelLOD.initPanel(this._form.m_formWidgets[6], enSliderKind.Slider_ModelLOD);
            this.m_SliderParticleLOD.initPanel(this._form.m_formWidgets[7], enSliderKind.Slider_ParticleLOD);
            this.m_SliderSkillTip.initPanel(this._form.m_formWidgets[8], enSliderKind.Slider_SkillTip);
            this.m_SliderFps.initPanel(this._form.m_formWidgets[0x16], enSliderKind.Slider_Fps);
            if ((this._form.m_formWidgets.Length > 0x17) && (this._form.m_formWidgets[0x17] != null))
            {
                this.m_SliderVoice.initPanel(this._form.m_formWidgets[0x17], enSliderKind.Slider_Voice);
            }
            Text component = this._form.m_formWidgets[9].transform.FindChild("Text").gameObject.GetComponent<Text>();
            ApolloAccountInfo accountInfo = Singleton<ApolloHelper>.GetInstance().GetAccountInfo(false);
            if (accountInfo != null)
            {
                if (accountInfo.Platform == ApolloPlatform.QQ)
                {
                    component.text = Singleton<CTextManager>.GetInstance().GetText("Common_Login_QQ");
                }
                else if (accountInfo.Platform == ApolloPlatform.Wechat)
                {
                    component.text = Singleton<CTextManager>.GetInstance().GetText("Common_Login_Weixin");
                }
                else if (accountInfo.Platform == ApolloPlatform.WTLogin)
                {
                    component.text = Singleton<CTextManager>.GetInstance().GetText("Common_Login_PC");
                }
                else if (accountInfo.Platform == ApolloPlatform.Guest)
                {
                    component.text = Singleton<CTextManager>.GetInstance().GetText("Common_Login_Guest");
                }
            }
            else
            {
                this._form.m_formWidgets[9].CustomSetActive(false);
            }
            if (Singleton<BattleLogic>.instance.isRuning)
            {
                GameObject widget = this._form.GetWidget(0x1a);
                if (widget != null)
                {
                    if (Singleton<CSurrenderSystem>.instance.CanSurrender())
                    {
                        widget.CustomSetActive(true);
                        GameObject p = Utility.FindChild(widget, "Button_Surrender");
                        if (p != null)
                        {
                            Button btn = p.GetComponent<Button>();
                            if (btn != null)
                            {
                                GameObject obj4 = Utility.FindChild(p, "CountDown");
                                if (obj4 != null)
                                {
                                    CUITimerScript componetInChild = Utility.GetComponetInChild<CUITimerScript>(obj4, "timerSurrender");
                                    if (componetInChild != null)
                                    {
                                        uint time = 0;
                                        if (Singleton<CSurrenderSystem>.instance.InSurrenderCD(out time))
                                        {
                                            obj4.CustomSetActive(true);
                                            CUICommonSystem.SetButtonEnable(btn, false, false, true);
                                            componetInChild.SetTotalTime((float) time);
                                            componetInChild.StartTimer();
                                        }
                                        else
                                        {
                                            obj4.CustomSetActive(false);
                                            CUICommonSystem.SetButtonEnable(btn, true, true, true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        widget.CustomSetActive(false);
                    }
                }
            }
        }

        private void InitWidget(string formPath)
        {
            this._form = Singleton<CUIManager>.GetInstance().OpenForm(formPath, false, true);
            this._tabList = this._form.m_formWidgets[3].GetComponent<CUIListScript>();
            DebugHelper.Assert(this._tabList != null);
            this._tabList.SelectElement(0, true);
            CUIListElementScript elemenet = this._tabList.GetElemenet(0);
            this.ChangeText(elemenet, "基础设置");
            elemenet = this._tabList.GetElemenet(1);
            this.ChangeText(elemenet, "操作设置");
            this._sensitivitySlider = this._form.m_formWidgets[13].transform.FindChild("Slider").gameObject.GetComponent<Slider>();
            this._sensitivitySlider.value = GameSettings.LunPanSensitivity;
            this._sensitivitySlider.onValueChanged.AddListener(new UnityAction<float>(this.OnSensitivityChange));
            if (GameSettings.TheCastType == CastType.LunPanCast)
            {
                this._form.m_formWidgets[11].GetComponent<Toggle>().isOn = false;
                this._form.m_formWidgets[12].GetComponent<Toggle>().isOn = true;
                this._form.m_formWidgets[0x13].CustomSetActive(true);
            }
            else
            {
                this._form.m_formWidgets[11].GetComponent<Toggle>().isOn = true;
                this._form.m_formWidgets[12].GetComponent<Toggle>().isOn = false;
                this._form.m_formWidgets[0x13].CustomSetActive(false);
            }
            this._form.m_formWidgets[14].GetComponent<Toggle>().isOn = GameSettings.TheSelectType == SelectEnemyType.SelectNearest;
            this._form.m_formWidgets[15].GetComponent<Toggle>().isOn = GameSettings.TheSelectType == SelectEnemyType.SelectLowHp;
            this._form.m_formWidgets[0x18].GetComponent<Toggle>().isOn = GameSettings.TheCommonAttackType == CommonAttactType.Type1;
            this._form.m_formWidgets[0x19].GetComponent<Toggle>().isOn = GameSettings.TheCommonAttackType == CommonAttactType.Type2;
            this._form.m_formWidgets[0x11].GetComponent<Toggle>().isOn = GameSettings.JoyStickMoveType == 0;
            this._form.m_formWidgets[0x12].GetComponent<Toggle>().isOn = GameSettings.JoyStickMoveType == 1;
            this._form.m_formWidgets[20].GetComponent<Toggle>().isOn = GameSettings.JoyStickShowType == 0;
            this._form.m_formWidgets[0x15].GetComponent<Toggle>().isOn = GameSettings.JoyStickShowType == 1;
            this._form.m_formWidgets[11].GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(this.OnSmartCastChange));
            this._form.m_formWidgets[12].GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(this.OnLunPanCastChange));
            this._form.m_formWidgets[14].GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(CSettingsSys.OnPickNearestChange));
            this._form.m_formWidgets[15].GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(CSettingsSys.OnPickMinHpChange));
            this._form.m_formWidgets[0x18].GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(CSettingsSys.OnCommonAttackType1Change));
            this._form.m_formWidgets[0x19].GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(CSettingsSys.OnCommonAttackType2Change));
            this._form.m_formWidgets[0x11].GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(CSettingsSys.OnJoyStickMoveChange));
            this._form.m_formWidgets[0x12].GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(CSettingsSys.OnJoyStickNoMoveChange));
            this._form.m_formWidgets[20].GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(CSettingsSys.OnRightJoyStickBtnLocChange));
            this._form.m_formWidgets[0x15].GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(CSettingsSys.OnRightJoyStickFingerLocChange));
            this.SetCameraHeightShow();
        }

        private void OnCameraHeightChange(CUIEvent uiEvent)
        {
            GameSettings.CameraHeight = uiEvent.m_eventParams.sliderValue;
            this.SetCameraHeightShow();
        }

        private void OnClickContract(CUIEvent uiEvent)
        {
            CUICommonSystem.OpenUrl("http://game.qq.com/contract.shtml", false);
        }

        private void OnClickPrivacyPolicy(CUIEvent uiEvent)
        {
            CUICommonSystem.OpenUrl("http://www.tencent.com/en-us/zc/privacypolicy.shtml", false);
        }

        private void OnClickTermOfService(CUIEvent uiEvent)
        {
            CUICommonSystem.OpenUrl("http://www.tencent.com/en-us/zc/termsofservice.shtml", false);
        }

        protected void OnCloseSetting(CUIEvent uiEvent)
        {
            this.UnInitWidget();
            GameSettings.Save();
        }

        private static void OnCommonAttackType1Change(bool toggle)
        {
            if (toggle)
            {
                GameSettings.TheCommonAttackType = CommonAttactType.Type1;
            }
        }

        private static void OnCommonAttackType2Change(bool toggle)
        {
            if (toggle)
            {
                GameSettings.TheCommonAttackType = CommonAttactType.Type2;
            }
        }

        private void onConfirmLogout(CUIEvent uiEvent)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x3f8);
            msg.stPkgData.stGameLogoutReq.iLogoutType = 0;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
        }

        private static void OnJoyStickMoveChange(bool toggle)
        {
            if (toggle)
            {
                GameSettings.JoyStickMoveType = 0;
            }
        }

        private static void OnJoyStickNoMoveChange(bool toggle)
        {
            if (toggle)
            {
                GameSettings.JoyStickMoveType = 1;
            }
        }

        private void OnLunPanCastChange(bool toggle)
        {
            if (toggle)
            {
                this._form.m_formWidgets[0x13].CustomSetActive(true);
                GameSettings.TheCastType = CastType.LunPanCast;
            }
        }

        private void onOpenBattleSetting(CUIEvent uiEvent)
        {
            this.InitWidget(SETTING_FORM_BATTLE);
            this.initPanel(this._form);
            this.setUI();
        }

        private void onOpenSetting(CUIEvent uiEvent)
        {
            this.InitWidget(SETTING_FORM);
            this.initPanel(this._form);
            this.setUI();
        }

        private static void OnPickMinHpChange(bool toggle)
        {
            if (toggle)
            {
                GameSettings.TheSelectType = SelectEnemyType.SelectLowHp;
            }
        }

        private static void OnPickNearestChange(bool toggle)
        {
            if (toggle)
            {
                GameSettings.TheSelectType = SelectEnemyType.SelectNearest;
            }
        }

        private void onQualitySettingAccept(CUIEvent uiEvent)
        {
            stUIEventParams eventParams = uiEvent.m_eventParams;
            switch (eventParams.tag)
            {
                case 2:
                    GameSettings.ModelLOD = this.m_SliderModelLOD.MaxValue - eventParams.tag2;
                    break;

                case 3:
                    GameSettings.ParticleLOD = this.m_SliderModelLOD.MaxValue - eventParams.tag2;
                    break;

                case 5:
                    GameSettings.EnableOutline = eventParams.tag2 != 0;
                    break;
            }
            PlayerPrefs.SetInt("degrade", 0);
            PlayerPrefs.Save();
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Force_Modify_Quality", null, true);
        }

        private void onQualitySettingCancel(CUIEvent uiEvent)
        {
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Give_Up_Modify_Quality", null, true);
            this.m_SliderModelLOD.value = this.m_SliderModelLOD.MaxValue - GameSettings.ModelLOD;
            this.m_SliderParticleLOD.value = this.m_SliderParticleLOD.MaxValue - GameSettings.ParticleLOD;
            this.m_SliderSkillTip.value = !GameSettings.EnableOutline ? 0 : 1;
        }

        private void onReqLogout(CUIEvent uiEvent)
        {
            Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("Common_Exit_Tip"), enUIEventID.Settings_ConfirmLogout, enUIEventID.None, false);
        }

        private static void OnRightJoyStickBtnLocChange(bool toggle)
        {
            if (toggle)
            {
                GameSettings.JoyStickShowType = 0;
            }
        }

        private static void OnRightJoyStickFingerLocChange(bool toggle)
        {
            if (toggle)
            {
                GameSettings.JoyStickShowType = 1;
            }
        }

        private void OnSensitivityChange(float value)
        {
            GameSettings.LunPanSensitivity = this._sensitivitySlider.value;
        }

        protected void OnSettingTabChange(CUIEvent uiEvent)
        {
            if ((this._form != null) && (this._tabList != null))
            {
                if (this._tabList.GetSelectedIndex() == 0)
                {
                    this._form.m_formWidgets[1].CustomSetActive(true);
                    this._form.m_formWidgets[2].CustomSetActive(false);
                }
                else
                {
                    this._form.m_formWidgets[1].CustomSetActive(false);
                    this._form.m_formWidgets[2].CustomSetActive(true);
                }
            }
        }

        private void onSliderChange(int value, enSliderKind SliderKind)
        {
            switch (SliderKind)
            {
                case enSliderKind.Slider_Music:
                    GameSettings.EnableMusic = value == 1;
                    break;

                case enSliderKind.Slider_SoundEffect:
                    GameSettings.EnableSound = value == 1;
                    break;

                case enSliderKind.Slider_ModelLOD:
                {
                    if (((this.m_SliderModelLOD.MaxValue - value) >= GameSettings.ModelLOD) || (PlayerPrefs.GetInt("degrade", 0) != 1))
                    {
                        GameSettings.ModelLOD = this.m_SliderModelLOD.MaxValue - value;
                        break;
                    }
                    stUIEventParams par = new stUIEventParams {
                        tag = (int) SliderKind,
                        tag2 = value
                    };
                    Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("Setting_Quality_Confirm"), enUIEventID.Settings_ConfirmQuality_Accept, enUIEventID.Settings_ConfirmQuality_Cancel, par, false);
                    break;
                }
                case enSliderKind.Slider_ParticleLOD:
                {
                    if (((this.m_SliderParticleLOD.MaxValue - value) >= GameSettings.ParticleLOD) || (PlayerPrefs.GetInt("degrade", 0) != 1))
                    {
                        GameSettings.ParticleLOD = this.m_SliderParticleLOD.MaxValue - value;
                        break;
                    }
                    stUIEventParams params2 = new stUIEventParams {
                        tag = (int) SliderKind,
                        tag2 = value
                    };
                    Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("Setting_Quality_Confirm"), enUIEventID.Settings_ConfirmQuality_Accept, enUIEventID.Settings_ConfirmQuality_Cancel, params2, false);
                    break;
                }
                case enSliderKind.Slider_SkillTip:
                    if (((value != 0) && !GameSettings.EnableOutline) && (PlayerPrefs.GetInt("degrade", 0) == 1))
                    {
                        stUIEventParams params3 = new stUIEventParams {
                            tag = (int) SliderKind,
                            tag2 = value
                        };
                        Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("Setting_Quality_Confirm"), enUIEventID.Settings_ConfirmQuality_Accept, enUIEventID.Settings_ConfirmQuality_Cancel, params3, false);
                    }
                    else
                    {
                        GameSettings.EnableOutline = value != 0;
                    }
                    break;

                case enSliderKind.Slider_Fps:
                    GameSettings.FpsShowType = value;
                    break;

                case enSliderKind.Slider_Voice:
                    GameSettings.EnableVoice = value == 1;
                    break;
            }
        }

        private void OnSmartCastChange(bool toggle)
        {
            if (toggle)
            {
                this._form.m_formWidgets[0x13].CustomSetActive(false);
                GameSettings.TheCastType = CastType.SmartCast;
            }
        }

        private void OnSurrenderCDReady(CUIEvent uiEvent)
        {
            if (Singleton<BattleLogic>.instance.isRuning && (this._form != null))
            {
                GameObject widget = this._form.GetWidget(0x1a);
                if (widget != null)
                {
                    GameObject p = Utility.FindChild(widget, "Button_Surrender");
                    if (p != null)
                    {
                        Button component = p.GetComponent<Button>();
                        if (component != null)
                        {
                            GameObject obj4 = Utility.FindChild(p, "CountDown");
                            if (obj4 != null)
                            {
                                obj4.CustomSetActive(false);
                                CUICommonSystem.SetButtonEnable(component, true, true, true);
                            }
                        }
                    }
                }
            }
        }

        private void OnUpdateTimer(CUIEvent uiEvent)
        {
            if (Singleton<BattleLogic>.instance.isRuning && (this._form != null))
            {
            }
        }

        private void SetCameraHeightShow()
        {
            GameObject obj2 = this._form.m_formWidgets[0x10];
            int childCount = obj2.transform.Find("Slider/Background").childCount;
            Text[] textArray = new Text[childCount];
            for (int i = 0; i < childCount; i++)
            {
                textArray[i] = obj2.transform.Find(string.Format("Slider/Background/Text{0}", i + 1)).GetComponent<Text>();
            }
            Text component = obj2.transform.Find("Slider/Handle Slide Area/Handle/Text").GetComponent<Text>();
            Slider slider = obj2.transform.Find("Slider").GetComponent<Slider>();
            component.text = textArray[GameSettings.CameraHeight].text;
            if (slider.value != GameSettings.CameraHeight)
            {
                slider.value = GameSettings.CameraHeight;
            }
        }

        private void setUI()
        {
            this.m_SliderMusic.value = !GameSettings.EnableMusic ? 0 : 1;
            this.m_SliderSoundEffect.value = !GameSettings.EnableSound ? 0 : 1;
            this.m_SliderModelLOD.value = this.m_SliderModelLOD.MaxValue - GameSettings.ModelLOD;
            this.m_SliderParticleLOD.value = this.m_SliderParticleLOD.MaxValue - GameSettings.ParticleLOD;
            this.m_SliderSkillTip.value = !GameSettings.EnableOutline ? 0 : 1;
            this.m_SliderFps.value = GameSettings.FpsShowType;
            this.m_SliderVoice.value = !GameSettings.EnableVoice ? 0 : 1;
        }

        public override void UnInit()
        {
            base.UnInit();
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_OpenForm, new CUIEventManager.OnUIEventHandler(this.onOpenSetting));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_ReqLogout, new CUIEventManager.OnUIEventHandler(this.onReqLogout));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_ConfirmLogout, new CUIEventManager.OnUIEventHandler(this.onConfirmLogout));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_SettingTypeChange, new CUIEventManager.OnUIEventHandler(this.OnSettingTabChange));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnCloseSetting));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_CameraHeight, new CUIEventManager.OnUIEventHandler(this.OnCameraHeightChange));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_PrivacyPolicy, new CUIEventManager.OnUIEventHandler(this.OnClickPrivacyPolicy));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_TermOfService, new CUIEventManager.OnUIEventHandler(this.OnClickTermOfService));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_Contract, new CUIEventManager.OnUIEventHandler(this.OnClickContract));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_UpdateTimer, new CUIEventManager.OnUIEventHandler(this.OnUpdateTimer));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_SurrenderCDReady, new CUIEventManager.OnUIEventHandler(this.OnSurrenderCDReady));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_ConfirmQuality_Accept, new CUIEventManager.OnUIEventHandler(this.onQualitySettingAccept));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Settings_ConfirmQuality_Cancel, new CUIEventManager.OnUIEventHandler(this.onQualitySettingCancel));
            this.m_SliderMusic = null;
            this.m_SliderSoundEffect = null;
            this.m_SliderModelLOD = null;
            this.m_SliderParticleLOD = null;
            this.m_SliderSkillTip = null;
            this.m_SliderFps = null;
            this.m_SliderVoice = null;
        }

        private void UnInitWidget()
        {
            if (this._sensitivitySlider != null)
            {
                this._sensitivitySlider.onValueChanged.RemoveAllListeners();
                this._form.m_formWidgets[11].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                this._form.m_formWidgets[12].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                this._form.m_formWidgets[14].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                this._form.m_formWidgets[15].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                this._form.m_formWidgets[0x11].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                this._form.m_formWidgets[0x12].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                this._form.m_formWidgets[0x18].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                this._form.m_formWidgets[0x19].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                this._sensitivitySlider = null;
                this._tabList = null;
                this._form = null;
            }
        }

        protected enum SettingType
        {
            Basic,
            Operation
        }
    }
}

