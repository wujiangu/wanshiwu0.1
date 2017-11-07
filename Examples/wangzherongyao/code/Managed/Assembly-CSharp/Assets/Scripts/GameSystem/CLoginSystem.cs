namespace Assets.Scripts.GameSystem
{
    using Apollo;
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class CLoginSystem : Singleton<CLoginSystem>
    {
        [CompilerGenerated]
        private static CTimer.OnTimeUpHandler <>f__am$cache10;
        [CompilerGenerated]
        private static CTimer.OnTimeUpHandler <>f__am$cache11;
        public const int GET_ADDRESS_INTERVAL = 0x3e8;
        private IAsyncResult getAddressResult;
        public const string LAST_RECONNECT_FAILED_TIME = "SGAME_Reconnect_Failed_Time";
        private string login_port = string.Empty;
        private int m_CheckLoginTimeoutTimerSeq;
        private bool m_ConnectLimitDeregulated;
        public float m_fLoginBeginTime;
        private CUIFormScript m_Form;
        private bool m_IsQuickLoginNotifySet;
        private int m_ReConnectFailedTime;
        private int m_ReConnectTimes;
        public TdirUrl m_selectTdirUrl;
        private int m_UpdateApolloSwitchToLoginTimerSeq;
        private int m_zoneGroupSelectedIndex;
        private int m_zoneSelectIndex;
        public const string RECONNECT_TIMES = "SGAME_Reconnect_Times";
        public static string s_splashFormPath = "UGUI/Form/System/Login/Form_Splash.prefab";
        public static string sLoginFormPath = "UGUI/Form/System/Login/Form_Login.prefab";
        public static string sOpenIdFilePath = "/customOpenId.txt";
        public const string str_visitor_uid = "visitorUid";

        public void CloseLogin()
        {
            if (this.m_Form != null)
            {
                Singleton<EventRouter>.GetInstance().RemoveEventHandler<ApolloAccountInfo>(EventID.ApolloHelper_Login_Success, new Action<ApolloAccountInfo>(this.LoginSuccess));
                Singleton<CUIManager>.GetInstance().CloseForm(s_splashFormPath);
                Singleton<CUIManager>.GetInstance().CloseForm(sLoginFormPath);
                this.m_Form = null;
            }
            this.m_ConnectLimitDeregulated = true;
            this.m_ReConnectTimes = 0;
            PlayerPrefs.SetInt("SGAME_Reconnect_Times", 0);
            PlayerPrefs.SetInt("SGAME_Reconnect_Failed_Time", 0);
        }

        private void ConnectLimit(bool showTips = true, ApolloResult result = 0)
        {
            bool flag = false;
            float num = 0f;
            if (this.m_Form == null)
            {
                return;
            }
            GameObject widget = this.m_Form.GetWidget(8);
            GameObject obj3 = Utility.FindChild(widget, "CountDown");
            if (widget == null)
            {
                return;
            }
            Button component = widget.GetComponent<Button>();
            if (component == null)
            {
                return;
            }
            CUITimerScript componetInChild = Utility.GetComponetInChild<CUITimerScript>(widget, "CountDown/timerEnableStartBtn");
            if (componetInChild == null)
            {
                return;
            }
            bool flag2 = false;
            bool flag3 = false;
            if (!this.m_ConnectLimitDeregulated && (this.m_ReConnectTimes >= 6))
            {
                flag2 = true;
                num = 300f;
            }
            else if (!this.m_ConnectLimitDeregulated && (this.m_ReConnectTimes >= 3))
            {
                flag2 = true;
                num = 10f;
            }
            DateTime time2 = new DateTime(0x7b2, 1, 1, 0, 0, 0, 0);
            int totalSeconds = (int) DateTime.Now.Subtract(time2.AddSeconds((double) this.m_ReConnectFailedTime)).TotalSeconds;
            num -= totalSeconds;
            if (num <= 0f)
            {
                flag2 = false;
            }
            string str = string.Empty;
            ApolloResult result2 = result;
            switch (result2)
            {
                case ApolloResult.PeerStopSession:
                case ApolloResult.TokenSvrError:
                case ApolloResult.Timeout:
                    break;

                case ApolloResult.StayInQueue:
                    flag3 = true;
                    str = "当前服务器人数过多需要排队";
                    goto Label_01D0;

                case ApolloResult.SvrIsFull:
                    flag3 = true;
                    str = "当前服务器已满";
                    goto Label_01D0;

                case ApolloResult.Success:
                    goto Label_01D0;

                case ApolloResult.NetworkException:
                    flag3 = true;
                    str = "网络异常";
                    goto Label_01D0;

                default:
                    switch (result2)
                    {
                        case ApolloResult.TokenInvalid:
                            flag = true;
                            flag3 = true;
                            str = "授权失败，请重新登录";
                            goto Label_01D0;

                        case ApolloResult.ConnectFailed:
                            break;

                        default:
                            flag3 = true;
                            str = string.Format("未知错误({0})", result);
                            goto Label_01D0;
                    }
                    break;
            }
            flag3 = true;
            str = "服务器未响应";
        Label_01D0:
            if (flag2)
            {
                if (showTips)
                {
                    str = string.Format("{0}，{1}", str, string.Format("请等待{0}再尝试开始游戏", (num <= 60f) ? string.Format("{0}秒", num) : string.Format("{0}分钟", (int) (num / 60f))));
                }
                obj3.CustomSetActive(true);
                CUICommonSystem.SetButtonEnable(component, false, false, true);
                componetInChild.SetTotalTime(num);
                componetInChild.StartTimer();
            }
            else
            {
                obj3.CustomSetActive(false);
                CUICommonSystem.SetButtonEnable(component, true, true, true);
            }
            if (flag3)
            {
                PlayerPrefs.SetInt("SGAME_Reconnect_Times", this.m_ReConnectTimes);
                PlayerPrefs.SetInt("SGAME_Reconnect_Failed_Time", (int) DateTime.Now.Subtract(new DateTime(0x7b2, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            }
            if (flag)
            {
                Singleton<LobbyLogic>.GetInstance().GotoAccLoginPage();
            }
            if (showTips && (str != string.Empty))
            {
                Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
                Singleton<CUIManager>.GetInstance().OpenTips(str, false, 1f, null, new object[0]);
            }
        }

        public void Draw()
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<ApolloAccountInfo>(EventID.ApolloHelper_Login_Success, new Action<ApolloAccountInfo>(this.LoginSuccess));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler(EventID.ApolloHelper_Login_Canceled, new Action(this, (IntPtr) this.LoginCancel));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler(EventID.ApolloHelper_Login_Failed, new Action(this, (IntPtr) this.LoginFailed));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<ApolloPlatform>(EventID.ApolloHelper_Platform_Not_Installed, new Action<ApolloPlatform>(this.PlatformNotInstalled));
            Singleton<EventRouter>.GetInstance().AddEventHandler<ApolloAccountInfo>(EventID.ApolloHelper_Login_Success, new Action<ApolloAccountInfo>(this.LoginSuccess));
            Singleton<EventRouter>.GetInstance().AddEventHandler(EventID.ApolloHelper_Login_Canceled, new Action(this, (IntPtr) this.LoginCancel));
            Singleton<EventRouter>.GetInstance().AddEventHandler(EventID.ApolloHelper_Login_Failed, new Action(this, (IntPtr) this.LoginFailed));
            Singleton<EventRouter>.GetInstance().AddEventHandler<ApolloPlatform>(EventID.ApolloHelper_Platform_Not_Installed, new Action<ApolloPlatform>(this.PlatformNotInstalled));
            if (!this.m_IsQuickLoginNotifySet)
            {
                Singleton<ApolloHelper>.GetInstance().RegisterQuickLoginHandler(new ApolloQuickLoginNotify(this.QuickLoginDone));
                this.m_IsQuickLoginNotifySet = true;
            }
            if (this.m_UpdateApolloSwitchToLoginTimerSeq == 0)
            {
                this.m_UpdateApolloSwitchToLoginTimerSeq = Singleton<CTimerManager>.GetInstance().AddTimer(0x3e8, -1, new CTimer.OnTimeUpHandler(this.UpdateApolloSwitchToLoginFlag));
            }
            if (this.m_Form == null)
            {
                CUIFormScript script = null;
                if (Singleton<CUIManager>.GetInstance().GetForm(s_splashFormPath) == null)
                {
                    script = Singleton<CUIManager>.GetInstance().OpenForm(s_splashFormPath, false, true);
                }
                this.m_Form = Singleton<CUIManager>.GetInstance().OpenForm(sLoginFormPath, false, true);
            }
            GameObject widget = this.m_Form.GetWidget(0x12);
            if (widget != null)
            {
                Text component = widget.GetComponent<Text>();
                if (component != null)
                {
                    object[] args = new object[] { GameFramework.AppVersion, MonoSingleton<CVersionUpdateSystem>.GetInstance().CachedResourceVersion, CVersion.GetBuildNumber(), CVersion.GetRevisonNumber() };
                    component.text = string.Format("App v{0} Res v{1} Build{2} R{3}", args);
                }
            }
            this.m_Form.GetWidget(7).SetActive(false);
            this.m_Form.GetWidget(0).SetActive(false);
            this.m_Form.GetWidget(4).SetActive(false);
            this.m_Form.GetWidget(15).SetActive(false);
            ApolloAccountInfo accountInfo = Singleton<ApolloHelper>.GetInstance().GetAccountInfo(true);
            if (accountInfo != null)
            {
                Singleton<EventRouter>.GetInstance().BroadCastEvent<ApolloAccountInfo>(EventID.ApolloHelper_Login_Success, accountInfo);
            }
            else
            {
                this.m_Form.GetWidget(0).SetActive(true);
                this.m_Form.GetWidget(2).SetActive(true);
                if (File.Exists(Application.persistentDataPath + sOpenIdFilePath))
                {
                    this.m_Form.GetWidget(0x13).SetActive(true);
                }
                else
                {
                    this.m_Form.GetWidget(0x13).SetActive(false);
                }
                this.m_Form.GetWidget(3).SetActive(false);
                if (!NoticeSys.m_bShowLoginBefore)
                {
                    if (<>f__am$cache11 == null)
                    {
                        <>f__am$cache11 = delegate (int seq) {
                            NoticeSys.m_bShowLoginBefore = true;
                            Singleton<ApolloHelper>.GetInstance().ShowNotice(0, "1");
                        };
                    }
                    Singleton<CTimerManager>.GetInstance().AddTimer(1, 1, <>f__am$cache11);
                }
            }
        }

        public override void Init()
        {
            base.Init();
            this.m_Form = null;
            this.getAddressResult = null;
            CUIEventManager instance = Singleton<CUIEventManager>.GetInstance();
            instance.AddUIEventListener(enUIEventID.Login_Platform_Guest, new CUIEventManager.OnUIEventHandler(this.OnLogin_GuestLogin));
            instance.AddUIEventListener(enUIEventID.Login_Platform_QQ, new CUIEventManager.OnUIEventHandler(this.OnLogin_QQLogin));
            instance.AddUIEventListener(enUIEventID.Login_Platform_WX, new CUIEventManager.OnUIEventHandler(this.OnLogin_WxLogin));
            instance.AddUIEventListener(enUIEventID.Login_Platform_Quit, new CUIEventManager.OnUIEventHandler(this.OnLogin_Quit));
            instance.AddUIEventListener(enUIEventID.Login_Platform_WTLogin, new CUIEventManager.OnUIEventHandler(this.OnLogin_WtLogin));
            instance.AddUIEventListener(enUIEventID.Login_Platform_None, new CUIEventManager.OnUIEventHandler(this.OnLogin_None));
            instance.AddUIEventListener(enUIEventID.Login_Start_Game, new CUIEventManager.OnUIEventHandler(this.OnLogin_Start_Game));
            instance.AddUIEventListener(enUIEventID.Login_Platform_Logout, new CUIEventManager.OnUIEventHandler(this.OnPlatformLogout));
            instance.AddUIEventListener(enUIEventID.Login_Trans_Visitor_Yes, new CUIEventManager.OnUIEventHandler(this.OnConfirmTransferData));
            instance.AddUIEventListener(enUIEventID.Login_Trans_Visitor_No, new CUIEventManager.OnUIEventHandler(this.OnRejectTransferData));
            instance.AddUIEventListener(enUIEventID.Login_Change_Account_Yes, new CUIEventManager.OnUIEventHandler(this.OnChangeAccountYes));
            instance.AddUIEventListener(enUIEventID.Login_Change_Account_No, new CUIEventManager.OnUIEventHandler(this.OnChangeAccountNo));
            instance.AddUIEventListener(enUIEventID.Login_Enable_Start_Btn_Timer_End, new CUIEventManager.OnUIEventHandler(this.OnEnableStartButton));
            instance.AddUIEventListener(enUIEventID.TDir_ZoneGroupSelect, new CUIEventManager.OnUIEventHandler(this.OnZoneGroupSelect));
            instance.AddUIEventListener(enUIEventID.TDir_ZoneSelect, new CUIEventManager.OnUIEventHandler(this.OnZoneSelect));
            instance.AddUIEventListener(enUIEventID.TDir_LastZoneSelect, new CUIEventManager.OnUIEventHandler(this.OnLastLoginZoneClick));
            instance.AddUIEventListener(enUIEventID.TDir_ShowZoneSelect, new CUIEventManager.OnUIEventHandler(this.OnShowZoneSelcet));
            instance.AddUIEventListener(enUIEventID.TDir_BackToStartGame, new CUIEventManager.OnUIEventHandler(this.OnBackToStartGame));
            TdirMgr local1 = MonoSingleton<TdirMgr>.GetInstance();
            local1.SvrListLoaded = (TdirMgr.TdirManagerEvent) Delegate.Combine(local1.SvrListLoaded, new TdirMgr.TdirManagerEvent(this.OnTDirLoadFinish));
            this.m_UpdateApolloSwitchToLoginTimerSeq = 0;
            this.m_IsQuickLoginNotifySet = false;
            this.m_ReConnectTimes = PlayerPrefs.GetInt("SGAME_Reconnect_Times", 0);
            this.m_ReConnectFailedTime = PlayerPrefs.GetInt("SGAME_Reconnect_Failed_Time", 0);
        }

        private void LoginCancel()
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            Singleton<CTimerManager>.GetInstance().RemoveTimerSafely(ref this.m_CheckLoginTimeoutTimerSeq);
            ApolloPlatform curPlatform = Singleton<ApolloHelper>.GetInstance().CurPlatform;
            if (curPlatform != ApolloPlatform.Wechat)
            {
                if (curPlatform != ApolloPlatform.QQ)
                {
                    Debug.LogWarning("user cancel on platform: " + Singleton<ApolloHelper>.instance.CurPlatform);
                }
                else
                {
                    Singleton<CUIManager>.GetInstance().OpenTips("Common_Login_QQ_Canceled", true, 1f, null, new object[0]);
                }
            }
            else
            {
                Singleton<CUIManager>.GetInstance().OpenTips("Common_Login_Weixin_Canceled", true, 1f, null, new object[0]);
            }
        }

        private void LoginFailed()
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            Singleton<CTimerManager>.GetInstance().RemoveTimerSafely(ref this.m_CheckLoginTimeoutTimerSeq);
            ApolloPlatform curPlatform = Singleton<ApolloHelper>.GetInstance().CurPlatform;
            if (curPlatform != ApolloPlatform.Wechat)
            {
                if (curPlatform != ApolloPlatform.QQ)
                {
                    Debug.LogError(string.Format("login failed on platform: {0}", Singleton<ApolloHelper>.GetInstance().CurPlatform));
                }
                else
                {
                    Singleton<CUIManager>.GetInstance().OpenTips("Common_Login_QQ_Failed", true, 1f, null, new object[0]);
                }
            }
            else
            {
                Singleton<CUIManager>.GetInstance().OpenTips("Common_Login_Weixin_Failed", true, 1f, null, new object[0]);
            }
        }

        private void LoginSuccess(ApolloAccountInfo info)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<ApolloAccountInfo>(EventID.ApolloHelper_Login_Success, new Action<ApolloAccountInfo>(this.LoginSuccess));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler(EventID.ApolloHelper_Login_Canceled, new Action(this, (IntPtr) this.LoginCancel));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler(EventID.ApolloHelper_Login_Failed, new Action(this, (IntPtr) this.LoginFailed));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<ApolloPlatform>(EventID.ApolloHelper_Platform_Not_Installed, new Action<ApolloPlatform>(this.PlatformNotInstalled));
            Singleton<CTimerManager>.GetInstance().RemoveTimerSafely(ref this.m_UpdateApolloSwitchToLoginTimerSeq);
            Singleton<CTimerManager>.GetInstance().RemoveTimerSafely(ref this.m_CheckLoginTimeoutTimerSeq);
            MonoSingleton<TdirMgr>.GetInstance().TdirAsync(info, null, null, true);
            this.RegisterXGPush(info.OpenId);
            if (!NoticeSys.m_bShowLoginBefore)
            {
                if (<>f__am$cache10 == null)
                {
                    <>f__am$cache10 = delegate (int seq) {
                        NoticeSys.m_bShowLoginBefore = true;
                        Singleton<ApolloHelper>.GetInstance().ShowNotice(0, "1");
                    };
                }
                Singleton<CTimerManager>.GetInstance().AddTimer(1, 1, <>f__am$cache10);
            }
            List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()),
                new KeyValuePair<string, string>("status", "0"),
                new KeyValuePair<string, string>("errorcode", "0")
            };
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_PlatformLogin", events, true);
        }

        private void LoginTimeout(int seq)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            this.m_CheckLoginTimeoutTimerSeq = 0;
            Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Login_Canceled);
        }

        private void OnBackToStartGame(CUIEvent uiEvent)
        {
            this.m_Form.GetWidget(9).SetActive(false);
            this.m_Form.GetWidget(0).SetActive(false);
            this.m_Form.GetWidget(4).SetActive(false);
            this.m_Form.GetWidget(7).SetActive(true);
            this.m_Form.GetWidget(15).SetActive(false);
            this.ToggleSplash3DImage(true);
            this.ConnectLimit(false, ApolloResult.Success);
        }

        private void OnChangeAccountNo(CUIEvent uiEvent)
        {
            Singleton<ApolloHelper>.GetInstance().SwitchUser(false);
            Singleton<ApolloHelper>.GetInstance().m_IsLastTriedPlatformSet = false;
            Singleton<ApolloHelper>.GetInstance().m_LastTriedPlatform = ApolloPlatform.None;
        }

        private void OnChangeAccountYes(CUIEvent uiEvent)
        {
            Singleton<ApolloHelper>.GetInstance().SwitchUser(false);
            Singleton<LobbyLogic>.GetInstance().GotoAccLoginPage();
            if (Singleton<ApolloHelper>.GetInstance().m_IsLastTriedPlatformSet)
            {
                ApolloPlatform lastTriedPlatform = Singleton<ApolloHelper>.GetInstance().m_LastTriedPlatform;
                Singleton<ApolloHelper>.GetInstance().m_IsLastTriedPlatformSet = false;
                Singleton<ApolloHelper>.GetInstance().m_LastTriedPlatform = ApolloPlatform.None;
                Singleton<ApolloHelper>.GetInstance().Login(lastTriedPlatform, 0L, null);
            }
            else
            {
                Singleton<ApolloHelper>.GetInstance().Login(Singleton<ApolloHelper>.GetInstance().CurPlatform, 0L, null);
            }
        }

        private void OnConfirmTransferData(CUIEvent uiEvent)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x403);
            msg.stPkgData.stRspAcntTransVisitorSvrData.bAgree = 1;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
            if (PlayerPrefs.HasKey("visitorUid"))
            {
                PlayerPrefs.DeleteKey("visitorUid");
            }
        }

        private void OnEnableStartButton(CUIEvent uiEvent)
        {
            this.m_ConnectLimitDeregulated = true;
            GameObject widget = this.m_Form.GetWidget(8);
            if (widget != null)
            {
                GameObject obj3 = Utility.FindChild(widget, "CountDown");
                Button component = widget.GetComponent<Button>();
                if (component != null)
                {
                    obj3.CustomSetActive(false);
                    CUICommonSystem.SetButtonEnable(component, true, true, true);
                }
            }
        }

        private void OnLastLoginZoneClick(CUIEvent uiEvent)
        {
            if (((MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.nodeID != 0) && (MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.name != null)) && (MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.name.Length != 0))
            {
                this.ShowEneterViewWithSelected();
            }
        }

        private void OnLobbyConnectFail(ApolloResult result)
        {
            Debug.Log("CLoginSystem OnLobbyConnectFail called!");
            Singleton<LobbySvrMgr>.GetInstance().connectFailHandler -= new LobbySvrMgr.ConnectFailHandler(this.OnLobbyConnectFail);
            this.ConnectLimit(true, result);
        }

        private void OnLogin_GuestLogin(CUIEvent uiEvent)
        {
            Singleton<ApolloHelper>.GetInstance().Login(ApolloPlatform.Guest, 0L, null);
        }

        private void OnLogin_None(CUIEvent uiEvent)
        {
            if (File.Exists(Application.persistentDataPath + sOpenIdFilePath))
            {
                string path = null;
                try
                {
                    path = Application.persistentDataPath + sOpenIdFilePath;
                    StreamReader reader = new StreamReader(path);
                    while ((ApolloConfig.CustomOpenId = reader.ReadLine().Trim()) != null)
                    {
                        Debug.Log(string.Format("custom openid: {0}", ApolloConfig.CustomOpenId));
                        goto Label_00AE;
                    }
                }
                catch (Exception exception)
                {
                    ApolloConfig.CustomOpenId = null;
                    Debug.Log(string.Format("File Not Found filePath: {0}, Exception {1}", path, exception.ToString()));
                    Singleton<CUIManager>.GetInstance().OpenTips("File Not Found!", false, 1f, null, new object[0]);
                    return;
                }
            }
        Label_00AE:
            Singleton<ApolloHelper>.GetInstance().Login(ApolloPlatform.None, 0L, null);
        }

        private void OnLogin_QQLogin(CUIEvent uiEvent)
        {
            Singleton<ApolloHelper>.GetInstance().Login(ApolloPlatform.QQ, 0L, null);
        }

        private void OnLogin_Quit(CUIEvent uiEvent)
        {
            SGameApplication.Quit();
        }

        private void OnLogin_Start_Game(CUIEvent uiEvent)
        {
            if (MonoSingleton<TdirMgr>.GetInstance() != null)
            {
                Singleton<CUIManager>.GetInstance().OpenSendMsgAlert(5, enUIEventID.None);
                this.m_ReConnectTimes++;
                this.m_ConnectLimitDeregulated = false;
                Singleton<LobbySvrMgr>.GetInstance().connectFailHandler -= new LobbySvrMgr.ConnectFailHandler(this.OnLobbyConnectFail);
                Singleton<LobbySvrMgr>.GetInstance().connectFailHandler += new LobbySvrMgr.ConnectFailHandler(this.OnLobbyConnectFail);
                MonoSingleton<TdirMgr>.GetInstance().ChooseGameServer(this.m_selectTdirUrl);
                List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("status", "0")
                };
                Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_StartGame", events, true);
            }
        }

        private void OnLogin_WtLogin(CUIEvent uiEvent)
        {
            if (this.m_Form != null)
            {
                GameObject widget = this.m_Form.GetWidget(4);
                InputField field = (widget == null) ? null : widget.transform.Find("UinInput").gameObject.GetComponent<InputField>();
                InputField field2 = (widget == null) ? null : widget.transform.Find("PswInput").gameObject.GetComponent<InputField>();
                InputField field3 = (widget == null) ? null : widget.transform.Find("OpenIdInput").gameObject.GetComponent<InputField>();
                if ((field3 != null) && (field3.text.Length > 0))
                {
                    stUIEventParams par = new stUIEventParams {
                        tagStr = field3.text
                    };
                    Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Login_Platform_None, par);
                }
                else if ((((field != null) && (field2 != null)) && ((field.text != null) && (field.text.Length > 0))) && (field2.text.Length > 0))
                {
                    PlayerPrefs.SetString("SGamePlayerUinCache", field.text);
                    PlayerPrefs.SetString("SGamePlayerPwdCache", field2.text);
                    Singleton<ApolloHelper>.GetInstance().Login(ApolloPlatform.WTLogin, Convert.ToUInt64(field.text), field2.text);
                }
            }
        }

        private void OnLogin_WxLogin(CUIEvent uiEvent)
        {
            if (this.m_CheckLoginTimeoutTimerSeq == 0)
            {
                this.m_CheckLoginTimeoutTimerSeq = Singleton<CTimerManager>.GetInstance().AddTimer(0x2710, 1, new CTimer.OnTimeUpHandler(this.LoginTimeout));
            }
            Singleton<ApolloHelper>.GetInstance().Login(ApolloPlatform.Wechat, 0L, null);
        }

        private void OnPlatformLogout(CUIEvent uiEvent)
        {
            Singleton<ApolloHelper>.GetInstance().Logout();
            Singleton<GameLogic>.GetInstance().OnPlayerLogout();
            Singleton<GameStateCtrl>.GetInstance().GotoState("LoginState");
        }

        private void OnRejectTransferData(CUIEvent uiEvent)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x403);
            msg.stPkgData.stRspAcntTransVisitorSvrData.bAgree = 0;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
        }

        private void OnShowZoneSelcet(CUIEvent uiEvent)
        {
            this.m_Form.GetWidget(9).SetActive(false);
            this.m_Form.GetWidget(0).SetActive(false);
            this.m_Form.GetWidget(4).SetActive(false);
            this.m_Form.GetWidget(7).SetActive(false);
            this.m_Form.GetWidget(15).SetActive(true);
            this.ToggleSplash3DImage(false);
            this.SetTdirZoneGroupList();
            this.SetLastLoginZone();
        }

        public void OnTDirLoadFinish()
        {
            if (this.m_Form == null)
            {
                this.m_Form = Singleton<CUIManager>.GetInstance().OpenForm(sLoginFormPath, false, true);
            }
            if (MonoSingleton<TdirMgr>.GetInstance().LastLoginUrl.nodeID == 0)
            {
                this.m_zoneGroupSelectedIndex = 1;
            }
            if (MonoSingleton<TdirMgr>.GetInstance().CheckTdirUrlValid(MonoSingleton<TdirMgr>.GetInstance().SelectedTdir))
            {
                this.ShowEneterViewWithSelected();
            }
            else
            {
                this.m_Form.GetWidget(9).SetActive(false);
                this.m_Form.GetWidget(0).SetActive(false);
                this.m_Form.GetWidget(4).SetActive(false);
                this.m_Form.GetWidget(7).SetActive(false);
                this.m_Form.GetWidget(15).SetActive(true);
                this.ToggleSplash3DImage(false);
                this.SetTdirZoneGroupList();
                this.SetLastLoginZone();
            }
        }

        private void OnZoneGroupSelect(CUIEvent uiEvent)
        {
            this.m_zoneGroupSelectedIndex = uiEvent.m_srcWidget.GetComponent<CUIListScript>().GetSelectedIndex();
            this.SetTdirZoneList(this.m_zoneGroupSelectedIndex);
        }

        private void OnZoneSelect(CUIEvent uiEvent)
        {
            this.m_zoneSelectIndex = uiEvent.m_srcWidget.GetComponent<CUIListScript>().GetSelectedIndex();
            List<TdirSvrGroup> svrUrlList = MonoSingleton<TdirMgr>.GetInstance().SvrUrlList;
            if (this.m_zoneGroupSelectedIndex < svrUrlList.Count)
            {
                TdirSvrGroup group = svrUrlList[this.m_zoneGroupSelectedIndex];
                if (this.m_zoneSelectIndex < group.tdirUrls.Count)
                {
                    TdirSvrGroup group2 = svrUrlList[this.m_zoneGroupSelectedIndex];
                    this.m_selectTdirUrl = group2.tdirUrls[this.m_zoneSelectIndex];
                    this.m_Form.GetWidget(9).SetActive(false);
                    this.m_Form.GetWidget(0).SetActive(false);
                    this.m_Form.GetWidget(4).SetActive(false);
                    this.m_Form.GetWidget(7).SetActive(true);
                    this.ToggleSplash3DImage(true);
                    this.ConnectLimit(false, ApolloResult.Success);
                    this.m_Form.GetWidget(15).SetActive(false);
                    this.ShowSnsName();
                    GameObject gameObject = this.m_Form.GetWidget(7).transform.Find("Panel").gameObject;
                    this.SetZone(gameObject, this.m_selectTdirUrl);
                    return;
                }
            }
            DebugHelper.Assert(false, "选区选服数组越界");
        }

        private void PlatformNotInstalled(ApolloPlatform platform)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            Singleton<CTimerManager>.GetInstance().RemoveTimerSafely(ref this.m_CheckLoginTimeoutTimerSeq);
            switch (platform)
            {
                case ApolloPlatform.Wechat:
                {
                    string text = Singleton<CTextManager>.GetInstance().GetText("Common_Platform_Weixin");
                    object[] replaceArr = new object[] { text, text };
                    Singleton<CUIManager>.GetInstance().OpenTips("Common_Login_Install_Platform", true, 1f, null, replaceArr);
                    break;
                }
            }
        }

        private void QuickLoginDone(ApolloWakeupInfo wakeupInfo)
        {
            Singleton<ApolloHelper>.GetInstance().IsWXGameCenter = wakeupInfo.MessageExt == "WX_GameCenter";
            ApolloAccountInfo accountInfo = Singleton<ApolloHelper>.GetInstance().GetAccountInfo(false);
            Singleton<ApolloHelper>.GetInstance().m_LastTriedPlatform = wakeupInfo.Platform;
            Singleton<ApolloHelper>.GetInstance().m_IsLastTriedPlatformSet = true;
            if (accountInfo != null)
            {
                if (wakeupInfo.OpenId != accountInfo.OpenId)
                {
                    Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("Common_Login_Different_Account_Tip"), enUIEventID.Login_Change_Account_Yes, enUIEventID.Login_Change_Account_No, false);
                    Singleton<ApolloHelper>.GetInstance().IsWXGameCenter = false;
                }
            }
            else if (Singleton<ApolloHelper>.GetInstance().IsWXGameCenter && (Singleton<GameStateCtrl>.GetInstance().GetCurrentState() is LoginState))
            {
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Login_Platform_WX);
            }
        }

        private void RegisterXGPush(string openID)
        {
            AndroidJavaClass class2 = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (class2 != null)
            {
                AndroidJavaObject @static = class2.GetStatic<AndroidJavaObject>("currentActivity");
                if (@static != null)
                {
                    Debug.LogWarning("RegisterXGPush in unity");
                    object[] args = new object[] { openID };
                    @static.Call("RegisterXGPush", args);
                }
            }
        }

        public void SetLastLoginZone()
        {
            GameObject gameObject = this.m_Form.GetWidget(14).transform.Find("Panel").gameObject;
            TdirUrl lastLoginUrl = MonoSingleton<TdirMgr>.GetInstance().LastLoginUrl;
            this.SetZone(gameObject, lastLoginUrl);
        }

        private void SetNickName(ApolloRelation aRelation)
        {
            Debug.LogWarning("Enter set nickname");
            GameObject widget = this.m_Form.GetWidget(0x10);
            if (widget != null)
            {
                Text component = widget.GetComponent<Text>();
                Debug.LogWarning(string.Format("aRelation.Result is :{0}", aRelation.Result));
                if ((component != null) && (aRelation.Result == ApolloResult.Success))
                {
                    if (!widget.activeSelf)
                    {
                        widget.SetActive(true);
                    }
                    foreach (ApolloPerson person in aRelation.Persons)
                    {
                        component.text = person.NickName;
                        break;
                    }
                }
                else
                {
                    GameObject obj3 = this.m_Form.GetWidget(0x11);
                    if ((obj3 != null) && obj3.activeSelf)
                    {
                        obj3.SetActive(false);
                    }
                }
            }
        }

        public void SetTdirZoneGroupList()
        {
            CUIListScript component = this.m_Form.GetWidget(12).GetComponent<CUIListScript>();
            List<TdirSvrGroup> svrUrlList = MonoSingleton<TdirMgr>.GetInstance().SvrUrlList;
            component.SetElementAmount(svrUrlList.Count);
            for (int i = 0; i < svrUrlList.Count; i++)
            {
                TdirSvrGroup group = svrUrlList[i];
                component.GetElemenet(i).gameObject.transform.Find("Text").gameObject.GetComponent<Text>().text = group.name;
            }
            component.SelectElement(this.m_zoneGroupSelectedIndex, true);
        }

        public void SetTdirZoneList(int index)
        {
            CUIListScript component = this.m_Form.GetWidget(13).GetComponent<CUIListScript>();
            List<TdirSvrGroup> svrUrlList = MonoSingleton<TdirMgr>.GetInstance().SvrUrlList;
            if (((svrUrlList != null) && (index >= 0)) && (index < svrUrlList.Count))
            {
                TdirSvrGroup group = svrUrlList[index];
                List<TdirUrl> tdirUrls = group.tdirUrls;
                component.SetElementAmount(tdirUrls.Count);
                for (int i = 0; i < tdirUrls.Count; i++)
                {
                    GameObject gameObject = component.GetElemenet(i).gameObject.transform.Find("Panel").gameObject;
                    this.SetZone(gameObject, tdirUrls[i]);
                }
            }
        }

        public void SetZone(GameObject zonePanel, TdirUrl tdirUrl)
        {
            zonePanel.transform.Find("ZoneName").gameObject.GetComponent<Text>().text = tdirUrl.name;
            Text component = zonePanel.transform.Find("State").gameObject.GetComponent<Text>();
            if (tdirUrl.flag == SvrFlag.New)
            {
                component.text = Singleton<CTextManager>.GetInstance().GetText("Zone_NEW");
            }
            else if (tdirUrl.flag == SvrFlag.Recommend)
            {
                component.text = Singleton<CTextManager>.GetInstance().GetText("Zone_Recommend");
            }
            else if (tdirUrl.flag == SvrFlag.Hot)
            {
                component.text = Singleton<CTextManager>.GetInstance().GetText("Zone_HOT");
            }
            else
            {
                component.text = string.Empty;
            }
            GameObject gameObject = zonePanel.transform.Find("StateImage").gameObject;
            if (!MonoSingleton<TdirMgr>.GetInstance().CheckTdirUrlValid(tdirUrl))
            {
                gameObject.CustomSetActive(false);
            }
            else
            {
                Animator animator = zonePanel.GetComponent<Animator>();
                if ((gameObject != null) && (animator != null))
                {
                    gameObject.CustomSetActive(true);
                    if (tdirUrl.statu == TdirSvrStatu.CROWDED)
                    {
                        animator.Play("ServerState_1");
                    }
                    else if (tdirUrl.statu == TdirSvrStatu.HEAVY)
                    {
                        animator.Play("ServerState_1");
                    }
                    else if (tdirUrl.statu == TdirSvrStatu.FINE)
                    {
                        animator.Play("ServerState_3");
                    }
                    else if (tdirUrl.statu == TdirSvrStatu.UNAVAILABLE)
                    {
                        animator.Play("ServerState_4");
                    }
                }
            }
        }

        public void ShowEneterViewWithSelected()
        {
            if (this.m_Form == null)
            {
                this.m_Form = Singleton<CUIManager>.GetInstance().OpenForm(sLoginFormPath, false, true);
            }
            this.m_selectTdirUrl = MonoSingleton<TdirMgr>.GetInstance().SelectedTdir;
            this.m_Form.GetWidget(9).SetActive(false);
            this.m_Form.GetWidget(0).SetActive(false);
            this.m_Form.GetWidget(4).SetActive(false);
            this.m_Form.GetWidget(7).SetActive(true);
            this.m_Form.GetWidget(15).SetActive(false);
            this.ToggleSplash3DImage(true);
            this.ConnectLimit(false, ApolloResult.Success);
            this.ShowSnsName();
            GameObject gameObject = this.m_Form.GetWidget(7).transform.Find("Panel").gameObject;
            this.SetZone(gameObject, this.m_selectTdirUrl);
        }

        private void ShowSnsName()
        {
            if (!Singleton<ApolloHelper>.GetInstance().GetMySnsInfo(new OnRelationNotifyHandle(this.SetNickName)))
            {
                Debug.LogError("Get sns failed");
                GameObject widget = this.m_Form.GetWidget(0x11);
                if ((widget != null) && widget.activeSelf)
                {
                    widget.SetActive(false);
                }
            }
        }

        private void ToggleSplash3DImage(bool show = true)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_splashFormPath);
            if (form != null)
            {
                GameObject obj2 = Utility.FindChild(form.gameObject, "3DImage");
                if (obj2 != null)
                {
                    obj2.CustomSetActive(show);
                }
            }
        }

        public override void UnInit()
        {
            this.m_Form = null;
            CUIEventManager instance = Singleton<CUIEventManager>.GetInstance();
            instance.RemoveUIEventListener(enUIEventID.Login_Platform_Guest, new CUIEventManager.OnUIEventHandler(this.OnLogin_GuestLogin));
            instance.RemoveUIEventListener(enUIEventID.Login_Platform_QQ, new CUIEventManager.OnUIEventHandler(this.OnLogin_QQLogin));
            instance.RemoveUIEventListener(enUIEventID.Login_Platform_WTLogin, new CUIEventManager.OnUIEventHandler(this.OnLogin_WtLogin));
            instance.RemoveUIEventListener(enUIEventID.Login_Platform_None, new CUIEventManager.OnUIEventHandler(this.OnLogin_None));
            instance.RemoveUIEventListener(enUIEventID.Login_Platform_WX, new CUIEventManager.OnUIEventHandler(this.OnLogin_WxLogin));
            instance.RemoveUIEventListener(enUIEventID.Login_Platform_Quit, new CUIEventManager.OnUIEventHandler(this.OnLogin_Quit));
            instance.RemoveUIEventListener(enUIEventID.Login_Start_Game, new CUIEventManager.OnUIEventHandler(this.OnLogin_Start_Game));
            instance.RemoveUIEventListener(enUIEventID.Login_Trans_Visitor_Yes, new CUIEventManager.OnUIEventHandler(this.OnConfirmTransferData));
            instance.RemoveUIEventListener(enUIEventID.Login_Trans_Visitor_No, new CUIEventManager.OnUIEventHandler(this.OnRejectTransferData));
            instance.RemoveUIEventListener(enUIEventID.Login_Change_Account_Yes, new CUIEventManager.OnUIEventHandler(this.OnChangeAccountYes));
            instance.RemoveUIEventListener(enUIEventID.Login_Change_Account_No, new CUIEventManager.OnUIEventHandler(this.OnChangeAccountNo));
            instance.RemoveUIEventListener(enUIEventID.Login_Enable_Start_Btn_Timer_End, new CUIEventManager.OnUIEventHandler(this.OnEnableStartButton));
            instance.RemoveUIEventListener(enUIEventID.TDir_ZoneGroupSelect, new CUIEventManager.OnUIEventHandler(this.OnZoneGroupSelect));
            instance.RemoveUIEventListener(enUIEventID.TDir_ZoneSelect, new CUIEventManager.OnUIEventHandler(this.OnZoneSelect));
            instance.RemoveUIEventListener(enUIEventID.TDir_LastZoneSelect, new CUIEventManager.OnUIEventHandler(this.OnLastLoginZoneClick));
            instance.RemoveUIEventListener(enUIEventID.TDir_ShowZoneSelect, new CUIEventManager.OnUIEventHandler(this.OnShowZoneSelcet));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<ApolloAccountInfo>(EventID.ApolloHelper_Login_Success, new Action<ApolloAccountInfo>(this.LoginSuccess));
            TdirMgr local1 = MonoSingleton<TdirMgr>.GetInstance();
            local1.SvrListLoaded = (TdirMgr.TdirManagerEvent) Delegate.Remove(local1.SvrListLoaded, new TdirMgr.TdirManagerEvent(this.OnTDirLoadFinish));
            Singleton<CTimerManager>.GetInstance().RemoveTimerSafely(ref this.m_UpdateApolloSwitchToLoginTimerSeq);
            this.m_UpdateApolloSwitchToLoginTimerSeq = 0;
        }

        private void UpdateApolloSwitchToLoginFlag(int seq)
        {
            Singleton<ApolloHelper>.GetInstance().IsSwitchToLoginPlatform = false;
        }

        public enum LoginFormWidget
        {
            MobilePanel,
            QQButton,
            WxButton,
            GuestButton,
            WtLoginPanel,
            WtLoginButton,
            QuitButton,
            StartGamePanel,
            StartButton,
            ServersPanel,
            ToggleGroup,
            Toggle,
            ListZoneGroup,
            ListZones,
            LastLoginZone,
            PnlSelectZone,
            SnsName,
            SnsPanel,
            Version,
            NoneButton
        }
    }
}

