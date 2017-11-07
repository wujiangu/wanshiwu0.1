using Apollo;
using Assets.Scripts.Framework;
using Assets.Scripts.GameSystem;
using Assets.Scripts.UI;
using CSProtocol;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class ApolloHelper : Singleton<ApolloHelper>
{
    private IApolloAccountService accountService;
    private ApolloInfo info = new ApolloInfo(ApolloConfig.QQAppID, ApolloConfig.WXAppID, ApolloConfig.maxMessageBufferSize, string.Empty);
    public bool m_bPayQQVIP;
    private ApolloPlatform m_CurPlatform;
    public bool m_IsLastTriedPlatformSet;
    private bool m_IsLoginEventHandlerRegistered;
    private bool m_IsLoginReturn;
    private bool m_IsSwitchToLoginPlatform;
    private bool m_IsWXGameCenter;
    public string m_LastOpenID;
    public ApolloPlatform m_LastTriedPlatform;
    private IApolloPayService payService;
    private IApolloQuickLoginService quickLoginService;
    private RegisterInfo registerInfo;
    private IApolloReportService reportService;
    private IApolloSnsService snsService;

    public ApolloHelper()
    {
        IApollo.Instance.Initialize(this.info);
        IApollo.Instance.SetApolloLogger(ApolloLogPriority.None, null);
        this.accountService = IApollo.Instance.GetAccountService();
        this.payService = null;
        this.registerInfo = new RegisterInfo();
        this.snsService = IApollo.Instance.GetService(1) as IApolloSnsService;
        this.reportService = IApollo.Instance.GetService(3) as IApolloReportService;
        this.quickLoginService = IApollo.Instance.GetService(7) as IApolloQuickLoginService;
        this.m_IsSwitchToLoginPlatform = false;
        this.m_IsLoginEventHandlerRegistered = false;
        this.m_IsLoginReturn = false;
        this.m_CurPlatform = ApolloPlatform.None;
        this.m_LastOpenID = null;
        this.m_LastTriedPlatform = ApolloPlatform.None;
        this.m_IsLastTriedPlatformSet = false;
    }

    public void ApolloRepoertEvent(string eventName, List<KeyValuePair<string, string>> events, bool isReal)
    {
        (IApollo.Instance.GetService(3) as IApolloReportService).ApolloRepoertEvent(eventName, events, isReal);
    }

    public void CrashAttchLog(string str)
    {
        AndroidJavaClass class2 = new AndroidJavaClass(ApolloConfig.GetGameUtilityString());
        object[] args = new object[] { str };
        class2.CallStatic("dtLog", args);
        class2.Dispose();
    }

    public void EnableBugly()
    {
        if (this.reportService != null)
        {
            this.reportService.EnableExceptionHandler(LogSeverity.LogException);
            this.reportService.ApolloReportInit(true, true);
        }
    }

    public ApolloAccountInfo GetAccountInfo(bool refreshToken = false)
    {
        ApolloAccountInfo accountInfo = new ApolloAccountInfo();
        if (ApolloConfig.platform == ApolloPlatform.None)
        {
            accountInfo.OpenId = ApolloConfig.CustomOpenId;
            accountInfo.Platform = ApolloPlatform.None;
            return accountInfo;
        }
        ApolloResult record = this.accountService.GetRecord(ref accountInfo);
        if (record == ApolloResult.Success)
        {
            Debug.Log("GetAccountInfo Success");
            if (this.m_CurPlatform != accountInfo.Platform)
            {
                ApolloConfig.platform = this.m_CurPlatform = accountInfo.Platform;
            }
            return accountInfo;
        }
        if (((record == ApolloResult.TokenInvalid) && (accountInfo != null)) && ((accountInfo.Platform == ApolloPlatform.Wechat) && refreshToken))
        {
            Debug.Log("try to refresh token");
            this.accountService.RefreshAtkEvent += new RefreshAccessTokenHandler(this.OnRefreshAccessTokenEvent);
            this.accountService.RefreshAccessToken();
            return null;
        }
        Debug.Log(string.Format("GetRecord Fail result is {0}", record));
        Debug.Log(string.Format("current platform is {0}", (accountInfo != null) ? ((object) accountInfo.Platform) : ((object) 0)));
        return null;
    }

    public string GetAccountInfoStr(ref ApolloAccountInfo info)
    {
        string str = "===== Account Info =====\n";
        str = ((str + string.Format("OpenId:{0}\n", info.OpenId)) + string.Format("Pf:{0}\n", info.Pf) + string.Format("PfKey:{0}\n", info.PfKey)) + string.Format("Platform:{0}\n", info.Platform) + "TokenList Begin:\n";
        foreach (ApolloToken token in info.TokenList)
        {
            str = str + string.Format("{0}:{1}\n", token.Type, token.Value);
        }
        return (str + "TokenList End:\n" + "===== Account Info =====");
    }

    public string GetAppId()
    {
        return ((Singleton<ApolloHelper>.GetInstance().CurPlatform != ApolloPlatform.Wechat) ? ApolloConfig.QQAppID : ApolloConfig.WXAppID);
    }

    public string GetAppKey()
    {
        return ((Singleton<ApolloHelper>.GetInstance().CurPlatform != ApolloPlatform.Wechat) ? ApolloConfig.QQAppKey : ApolloConfig.WXAppKey);
    }

    public int GetChannelID()
    {
        IApolloCommonService service = IApollo.Instance.GetService(8) as IApolloCommonService;
        if (service != null)
        {
            string channelId = service.GetChannelId();
            int result = 0;
            if (int.TryParse(channelId, out result))
            {
                Debug.Log("RegChannelID|" + result);
                return result;
            }
        }
        return 0;
    }

    public COM_PRIVILEGE_TYPE GetCurrentLoginPrivilege()
    {
        COM_PRIVILEGE_TYPE com_privilege_type = COM_PRIVILEGE_TYPE.COM_PRIVILEGE_TYPE_NONE;
        if ((this.m_CurPlatform == ApolloPlatform.Wechat) && this.m_IsWXGameCenter)
        {
            com_privilege_type = COM_PRIVILEGE_TYPE.COM_PRIVILEGE_TYPE_WXGAMECENTER_LOGIN;
        }
        return com_privilege_type;
    }

    public bool GetMySnsInfo(OnRelationNotifyHandle handler)
    {
        if (this.GetAccountInfo(false) == null)
        {
            Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Need_Login);
            return false;
        }
        if (this.snsService == null)
        {
            this.snsService = IApollo.Instance.GetService(1) as IApolloSnsService;
            if (this.snsService == null)
            {
                return false;
            }
        }
        this.snsService.onRelationEvent -= handler;
        this.snsService.onRelationEvent += handler;
        switch (Singleton<ApolloHelper>.GetInstance().CurPlatform)
        {
            case ApolloPlatform.Wechat:
                return this.snsService.QueryMyInfo(ApolloPlatform.Wechat);

            case ApolloPlatform.QQ:
            case ApolloPlatform.WTLogin:
                return this.snsService.QueryMyInfo(ApolloPlatform.QQ);
        }
        return false;
    }

    public void GetNoticeData(int type, string scene)
    {
        try
        {
            INotice service = IApollo.Instance.GetService(5) as INotice;
            ApolloNoticeInfo info = new ApolloNoticeInfo();
            service.GetNoticeData((APOLLO_NOTICETYPE) type, scene, ref info);
            for (int i = 0; i < info.DataList.Count; i++)
            {
                ApolloNoticeData data = info.DataList[i];
            }
            NoticeSys.NOTICE_STATE noticeState = NoticeSys.NOTICE_STATE.LOGIN_Before;
            if (scene == "1")
            {
                noticeState = NoticeSys.NOTICE_STATE.LOGIN_Before;
            }
            else if (scene == "2")
            {
                noticeState = NoticeSys.NOTICE_STATE.LOGIN_After;
            }
            MonoSingleton<NoticeSys>.GetInstance().OnOpenForm(info, noticeState);
        }
        catch (Exception exception)
        {
            object[] inParameters = new object[] { exception.Message };
            DebugHelper.Assert(false, "Error In GetNoticeData, {0}", inParameters);
        }
    }

    public List<string> GetNoticeUrl(int type, string scene)
    {
        List<string> list = new List<string>();
        INotice service = IApollo.Instance.GetService(5) as INotice;
        ApolloNoticeInfo info = new ApolloNoticeInfo();
        service.GetNoticeData((APOLLO_NOTICETYPE) type, scene, ref info);
        for (int i = 0; i < info.DataList.Count; i++)
        {
            ApolloNoticeData data = info.DataList[i];
            if (data.ContentType == APOLLO_NOTICE_CONTENTTYPE.APO_CONTENTTYPE_WEB)
            {
                list.Add(data.ContentUrl);
            }
        }
        return list;
    }

    public string GetRecordStr()
    {
        ApolloAccountInfo accountInfo = new ApolloAccountInfo();
        if (this.accountService == null)
        {
            BugLocateLogSys.Log("accountService == null");
            return "accountService == null";
        }
        ApolloResult record = this.accountService.GetRecord(ref accountInfo);
        if (record == ApolloResult.Success)
        {
            BugLocateLogSys.Log("GetRecord Success");
            return this.GetAccountInfoStr(ref accountInfo);
        }
        BugLocateLogSys.Log(string.Format("GetRecord result is {0}", record));
        return string.Empty;
    }

    public void HideScrollNotice()
    {
        (IApollo.Instance.GetService(5) as INotice).HideNotice();
    }

    public bool InitPay()
    {
        string payEnv = ApolloConfig.payEnv;
        if (this.GetAccountInfo(false) == null)
        {
            Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Need_Login);
            return false;
        }
        if (this.payService == null)
        {
            this.payService = IApollo.Instance.GetService(2) as IApolloPayService;
            this.payService.PayEvent += new OnApolloPaySvrEvenHandle(this.OnPaySuccess);
        }
        this.registerInfo.environment = payEnv;
        this.registerInfo.enableLog = 1;
        if (this.payService.Initialize(this.registerInfo))
        {
            Debug.Log("Init pay success!");
            Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Pay_Init_Success);
            return true;
        }
        Debug.Log("Init pay failed!");
        Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Pay_Init_Failed);
        return false;
    }

    public void InviteQQFriendToRoom(string title, string desc, string roomInfo)
    {
        byte[] buffer = (Resources.Load("Share/120", typeof(Texture2D)) as Texture2D).EncodeToPNG();
        int length = 0;
        if (buffer != null)
        {
            length = buffer.Length;
        }
        IApolloSnsService service = IApollo.Instance.GetService(1) as IApolloSnsService;
        if (Singleton<ApolloHelper>.GetInstance().CurPlatform == ApolloPlatform.QQ)
        {
            ApolloAccountInfo accountInfo = this.GetAccountInfo(false);
            string url = "http://gamecenter.qq.com/gcjump?appid=1104466820&pf=invite&from=androidqq&plat=qq&originuin=" + accountInfo.OpenId + "&ADTAG=gameobj.msg_invite&gamedata=" + roomInfo;
            string thumbImageUrl = "http://image.smoba.qq.com/yywj/share_pic/120.png";
            service.SendToQQ(ApolloShareScene.QSession, title, desc, url, thumbImageUrl);
        }
    }

    public bool IsLogin()
    {
        if (this.GetAccountInfo(false) == null)
        {
            return false;
        }
        return true;
    }

    public bool IsPlatformInstalled(ApolloPlatform platform)
    {
        return this.accountService.IsPlatformInstalled(platform);
    }

    public void Login(ApolloPlatform platform, ulong uin = 0, string pwd = null)
    {
        switch (platform)
        {
            case ApolloPlatform.None:
            {
                if (ApolloConfig.CustomOpenId == null)
                {
                    Debug.Log(string.Format("Custom OpenId is Empty! current platform{0}", platform));
                }
                ApolloResult result = NoneAccountService.Initialize(new NoneAccountInitInfo(ApolloConfig.CustomOpenId));
                if (result != ApolloResult.Success)
                {
                    Debug.Log(string.Format("login apollo result is {0}! current platform{1}", result, platform));
                    List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()),
                        new KeyValuePair<string, string>("status", "1"),
                        new KeyValuePair<string, string>("errorcode", result.ToString())
                    };
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_PlatformLogin", events, true);
                    Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Login_Failed);
                    return;
                }
                Debug.Log(string.Format("Set login by none platform success", result, platform));
                break;
            }
            case ApolloPlatform.WTLogin:
                Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Login_Failed);
                return;
        }
        ApolloConfig.Uin = uin;
        ApolloConfig.Password = pwd;
        ApolloConfig.platform = this.m_CurPlatform = platform;
        if (ApolloConfig.platform == ApolloPlatform.None)
        {
            Singleton<EventRouter>.GetInstance().BroadCastEvent<ApolloAccountInfo>(EventID.ApolloHelper_Login_Success, this.GetAccountInfo(false));
        }
        else
        {
            this.OnLogin(platform);
        }
    }

    public bool Logout()
    {
        this.accountService.Logout();
        this.m_LastOpenID = null;
        return (this.GetAccountInfo(false) == null);
    }

    private void OnLogin(ApolloPlatform platform)
    {
        if (!this.m_IsSwitchToLoginPlatform)
        {
            Singleton<CUIManager>.GetInstance().OpenSendMsgAlert(5, enUIEventID.None);
            this.m_IsSwitchToLoginPlatform = true;
            if (!this.accountService.IsPlatformInstalled(platform))
            {
                if (this.m_CurPlatform == ApolloPlatform.Wechat)
                {
                    Singleton<EventRouter>.GetInstance().BroadCastEvent<ApolloPlatform>(EventID.ApolloHelper_Platform_Not_Installed, ApolloPlatform.Wechat);
                    return;
                }
                if (this.m_CurPlatform != ApolloPlatform.QQ)
                {
                    Singleton<EventRouter>.GetInstance().BroadCastEvent<ApolloPlatform>(EventID.ApolloHelper_Platform_Not_Installed, platform);
                    return;
                }
            }
            if (!this.m_IsLoginEventHandlerRegistered)
            {
                this.m_IsLoginEventHandlerRegistered = true;
                this.accountService.LoginEvent += new AccountLoginHandle(this.OnLoginEvent);
            }
            ApolloAccountInfo accountInfo = this.GetAccountInfo(false);
            if (accountInfo != null)
            {
                this.m_LastOpenID = accountInfo.OpenId;
            }
            this.accountService.Login(platform);
        }
    }

    private void OnLoginEvent(ApolloResult loginResult, ApolloAccountInfo loginfo)
    {
        Debug.Log("OnLoginEvent called");
        this.m_IsSwitchToLoginPlatform = false;
        if (loginResult == ApolloResult.Success)
        {
            if (loginfo != null)
            {
                if ((this.m_LastOpenID != null) && (loginfo.OpenId != this.m_LastOpenID))
                {
                    Singleton<CUIManager>.GetInstance().OpenMessageBox(Singleton<CTextManager>.GetInstance().GetText("Common_Login_Different_Account_Tip_Force"), enUIEventID.Login_Change_Account_Yes, false);
                    return;
                }
                Singleton<EventRouter>.GetInstance().BroadCastEvent<ApolloAccountInfo>(EventID.ApolloHelper_Login_Success, loginfo);
            }
        }
        else if (loginResult == ApolloResult.UserCancel)
        {
            Debug.Log("Login Fail. User cancel");
            BugLocateLogSys.Log(string.Format("Login Fail. User cancel", new object[0]));
            Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Login_Canceled);
        }
        else
        {
            Debug.Log(string.Format("Login Fail. Error code is {0}", loginResult));
            BugLocateLogSys.Log(string.Format("Login Fail. Error code is {0}", loginResult));
            List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()),
                new KeyValuePair<string, string>("status", "1"),
                new KeyValuePair<string, string>("errorcode", loginResult.ToString())
            };
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_PlatformLogin", events, true);
            Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Login_Failed);
        }
        if (this.accountService != null)
        {
            this.accountService.LoginEvent -= new AccountLoginHandle(this.OnLoginEvent);
        }
        else
        {
            BugLocateLogSys.Log("accountService == null");
        }
        this.m_IsLoginEventHandlerRegistered = false;
        this.m_IsLoginReturn = true;
        BugLocateLogSys.Log("LoginEvent Thread:" + Thread.CurrentThread.ManagedThreadId);
    }

    private void OnPaySuccess(ApolloBufferBase info)
    {
        ApolloPayResponseInfo info2 = info as ApolloPayResponseInfo;
        if (info2.needRelogin != 0)
        {
            Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Need_Login);
        }
        else
        {
            object[] args = new object[] { info2.status, info2.rstCode, info2.realSaveNum, info2.extendInfo };
            string str = string.Format("status:{0},result code:{1},real num {2}, extendinfo ={3} ", args);
            if (info2.status == APO_PAY_STATUS.APO_PAYSTATE_PAYSUCC)
            {
                if (this.m_bPayQQVIP)
                {
                    this.m_bPayQQVIP = false;
                    CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x1068);
                    msg.stPkgData.stQQVIPInfoReq.bReserved = 0;
                    Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
                    Debug.Log("request qqvip");
                }
                Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Pay_Success);
            }
            else
            {
                Singleton<BeaconHelper>.GetInstance().Event_CommonReport("Event_PayFail");
                Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Pay_Failed);
            }
            this.m_bPayQQVIP = false;
        }
    }

    private void OnRefreshAccessTokenEvent(ApolloResult result, ListView<ApolloToken> tokenList)
    {
        Debug.Log("OnRefreshAccessTokenEvent called");
        Debug.Log(string.Format("refresh token rs:{0}", result));
        if (result == ApolloResult.Success)
        {
            ApolloAccountInfo accountInfo = this.GetAccountInfo(false);
            if (accountInfo != null)
            {
                Singleton<EventRouter>.GetInstance().BroadCastEvent<ApolloAccountInfo>(EventID.ApolloHelper_Login_Success, accountInfo);
            }
        }
        else if (tokenList != null)
        {
            for (int i = 0; i < tokenList.Count; i++)
            {
                Debug.Log(string.Format("token list {0}:{1}:{2}", tokenList[i].Type, tokenList[i].Value, tokenList[i].Expire));
            }
        }
        else
        {
            Debug.Log("refresh token list is null!");
        }
    }

    public void OpenWeiXinDeeplink(int linkType, string url)
    {
        IApolloCommonService service = IApollo.Instance.GetService(8) as IApolloCommonService;
        if (service != null)
        {
            string link = "INDEX";
            if (linkType == 0)
            {
                link = "INDEX";
            }
            else if (linkType == 1)
            {
                link = "DETAIL";
            }
            else if (linkType == 2)
            {
                link = "LIBRARY";
            }
            else if (linkType == 3)
            {
                link = url;
            }
            service.OpenWeiXinDeeplink(link);
        }
    }

    public bool Pay(string quantity, string productId = "")
    {
        if (!ApolloConfig.payEnabled)
        {
            return false;
        }
        if (!this.InitPay())
        {
            return false;
        }
        ApolloAccountInfo accountInfo = this.GetAccountInfo(false);
        PayInfo payInfo = new PayInfo();
        string str = MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString();
        payInfo.coinIcon = 0x7f020001;
        payInfo.offerId = ApolloConfig.offerID;
        payInfo.unit = "ge";
        payInfo.zoneId = str;
        payInfo.valueChangeable = 0;
        payInfo.saveValue = quantity;
        if (!this.payService.Pay(payInfo))
        {
            Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Pay_Failed);
            return false;
        }
        return true;
    }

    public bool PayQQVip(string serviceCode, string serviceName, int serviceType)
    {
        if (!ApolloConfig.payEnabled)
        {
            return false;
        }
        if (!this.InitPay())
        {
            return false;
        }
        this.m_bPayQQVIP = true;
        ApolloAccountInfo accountInfo = this.GetAccountInfo(false);
        Pay4MonthInfo payInfo = new Pay4MonthInfo();
        string str = MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString();
        payInfo.serviceCode = serviceCode;
        payInfo.serviceName = serviceName;
        payInfo.serviceType = (APO_PAY_MONTH_TYPE) serviceType;
        payInfo.autoPay = 0;
        payInfo.remark = "aid=mvip.youxi.inside.yxzj_1104466820";
        payInfo.coinIcon = 0x7f020001;
        payInfo.offerId = ApolloConfig.offerID;
        payInfo.unit = "ge";
        payInfo.zoneId = str;
        payInfo.valueChangeable = 0;
        payInfo.saveValue = "1";
        if (!this.payService.Pay(payInfo))
        {
            Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.ApolloHelper_Pay_Failed);
            return false;
        }
        return true;
    }

    public void RegisterQuickLoginHandler(ApolloQuickLoginNotify callback)
    {
        this.quickLoginService.SetQuickLoginNotify(callback);
    }

    public void ReportCatchExcption(Exception e)
    {
        (IApollo.Instance.GetService(3) as IApolloReportService).HandleException(e);
    }

    public void ReportRQD(string ecpMsg)
    {
        AndroidJavaClass class2 = new AndroidJavaClass(ApolloConfig.GetGameUtilityString());
        object[] args = new object[] { "C# Exception_", ecpMsg };
        class2.CallStatic("UploadException", args);
        class2.Dispose();
    }

    public void ShareInviteFriend(string openId, string title, string desc, string extInfo)
    {
        byte[] buffer = (Resources.Load("Share/120", typeof(Texture2D)) as Texture2D).EncodeToPNG();
        int length = 0;
        if (buffer != null)
        {
            length = buffer.Length;
        }
        IApolloSnsService service = IApollo.Instance.GetService(1) as IApolloSnsService;
        MonoSingleton<ShareSys>.instance.OnShareCallBack();
        if (Singleton<ApolloHelper>.GetInstance().CurPlatform == ApolloPlatform.QQ)
        {
            ApolloAccountInfo accountInfo = this.GetAccountInfo(false);
            string targetUrl = "http://gamecenter.qq.com/gcjump?appid=1104466820&pf=invite&from=androidqq&plat=qq&originuin=" + accountInfo.OpenId + "&ADTAG=gameobj.msg_invite";
            string imgUrl = "http://image.smoba.qq.com/yywj/share_pic/120.png";
            service.SendToQQGameFriend(1, openId, title, desc, targetUrl, imgUrl, title, "MSG_INVITE", extInfo);
        }
        else
        {
            service.SendToWXGameFriend(openId, title, desc, "9Wste6_dDgZtoVmC6CQTh0jj29kGEp0jrVSYrGWvtZLvSTDN9fUb-_sNjacaGITt", "messageExt", "MSG_INVITE", extInfo);
        }
    }

    public void ShareQQBox(string actID, string boxID, string title, string desc)
    {
        byte[] buffer = (Resources.Load("Share/120", typeof(Texture2D)) as Texture2D).EncodeToPNG();
        int length = 0;
        if (buffer != null)
        {
            length = buffer.Length;
        }
        IApolloSnsService service = IApollo.Instance.GetService(1) as IApolloSnsService;
        if (Singleton<ApolloHelper>.GetInstance().CurPlatform == ApolloPlatform.QQ)
        {
            string url = string.Format("http://gamecenter.qq.com/giftbox/release/index/grap.html?actid={0}&_wv=1031&boxid={1}&appid={2}", actID, boxID, ApolloConfig.appID);
            string thumbImageUrl = "http://image.smoba.qq.com/yywj/share_pic/120.png";
            Debug.Log("QQBox apollo targturl " + url);
            service.SendToQQ(ApolloShareScene.QSession, title, desc, url, thumbImageUrl);
        }
    }

    public void ShareSendHeart(string openId, string title, string desc, string extInfo)
    {
        byte[] buffer = (Resources.Load("Share/120", typeof(Texture2D)) as Texture2D).EncodeToPNG();
        int length = 0;
        if (buffer != null)
        {
            length = buffer.Length;
        }
        IApolloSnsService service = IApollo.Instance.GetService(1) as IApolloSnsService;
        MonoSingleton<ShareSys>.instance.OnShareCallBack();
        if (Singleton<ApolloHelper>.GetInstance().CurPlatform == ApolloPlatform.QQ)
        {
            ApolloAccountInfo accountInfo = this.GetAccountInfo(false);
            string targetUrl = "http://gamecenter.qq.com/gcjump?appid=1104466820&pf=invite&from=androidqq&plat=qq&originuin=" + accountInfo.OpenId + "&ADTAG=gameobj.msg_heart";
            string imgUrl = "http://image.smoba.qq.com/yywj/share_pic/120.png";
            service.SendToQQGameFriend(1, openId, title, desc, targetUrl, imgUrl, title, "MSG_HEART_SEND", extInfo);
        }
        else
        {
            service.SendToWXGameFriend(openId, title, desc, "9Wste6_dDgZtoVmC6CQTh0jj29kGEp0jrVSYrGWvtZLvSTDN9fUb-_sNjacaGITt", "messageExt", "MSG_heart_send", extInfo);
        }
    }

    public void ShareToFriend(string title, string desc)
    {
        byte[] thumbImgData = (Resources.Load("Share/120", typeof(Texture2D)) as Texture2D).EncodeToPNG();
        int thumbDataLen = 0;
        if (thumbImgData != null)
        {
            thumbDataLen = thumbImgData.Length;
        }
        IApolloSnsService service = IApollo.Instance.GetService(1) as IApolloSnsService;
        if (Singleton<ApolloHelper>.GetInstance().CurPlatform == ApolloPlatform.QQ)
        {
            ApolloAccountInfo accountInfo = this.GetAccountInfo(false);
            string url = "http://gamecenter.qq.com/gcjump?appid=1104466820&pf=invite&from=androidqq&plat=qq&originuin=" + accountInfo.OpenId + "&ADTAG=gameobj.msg_invite";
            string thumbImageUrl = "http://image.smoba.qq.com/yywj/share_pic/120.png";
            service.SendToQQ(ApolloShareScene.QSession, title, desc, url, thumbImageUrl);
        }
        else
        {
            service.SendToWeixin(title, desc, "MSG_INVITE", thumbImgData, thumbDataLen, "SendToWeixin_extInfo");
        }
    }

    public void ShowIOSGuestNotice()
    {
        ApolloNoticeInfo noticeInfo = new ApolloNoticeInfo();
        ApolloNoticeData item = new ApolloNoticeData {
            MsgType = APOLLO_NOTICETYPE.APO_NOTICETYPE_ALERT,
            ContentType = APOLLO_NOTICE_CONTENTTYPE.APO_SCONTENTTYPE_TEXT,
            MsgTitle = "游客模式特别说明",
            MsgContent = "敬爱的玩家，您正在使用游客模式进行游戏，游客模式下的游戏数据（包含付费数据）会在删除游戏、更换设备后清空。为了保障您的虚拟财产安全，以及让您获得更完善的游戏体验，我们建议您使用QQ/微信登录进行游戏！\n\n《王者荣耀》运营团队"
        };
        noticeInfo.DataList.Add(item);
        for (int i = 0; i < noticeInfo.DataList.Count; i++)
        {
            ApolloNoticeData data2 = noticeInfo.DataList[i];
            Debug.Log(string.Concat(new object[] { "Guest noticesysy onopenform ", data2.ToString(), "msgtitle = ", data2.MsgTitle, " content ", data2.MsgType, " pic ", data2.MsgUrl, " ", data2.ContentType, " content url = ", data2.ContentUrl }));
        }
        MonoSingleton<NoticeSys>.GetInstance().OnOpenForm(noticeInfo, NoticeSys.NOTICE_STATE.LOGIN_Before);
    }

    public void ShowNotice(int Type, string scene)
    {
        if (!CSysDynamicBlock.bLobbyEntryBlocked)
        {
            this.GetNoticeData(Type, scene);
        }
    }

    public void SwitchUser(bool chg)
    {
        if (chg)
        {
            this.quickLoginService.SwitchUser(true);
        }
        else
        {
            this.quickLoginService.SwitchUser(false);
        }
    }

    public string ToSnsHeadUrl(string url)
    {
        if ((url == null) || !url.StartsWith("http://i.gtimg.cn"))
        {
            if ((url != null) && url.StartsWith("http://wx.qlogo.cn/"))
            {
                return string.Format("{0}/{1}", url, "96");
            }
            if ((url != null) && url.StartsWith("http://q.qlogo.cn/"))
            {
                return string.Format("{0}/{1}", url, "100");
            }
            switch (Singleton<ApolloHelper>.GetInstance().CurPlatform)
            {
                case ApolloPlatform.Wechat:
                    return string.Format("{0}/{1}", url, "96");

                case ApolloPlatform.QQ:
                case ApolloPlatform.WTLogin:
                    return string.Format("{0}/{1}", url, "100");
            }
        }
        return url;
    }

    public string ToSnsHeadUrl(ref byte[] szUrl)
    {
        string str = StringHelper.UTF8BytesToString(ref szUrl);
        if ((str != null) && str.StartsWith("http://wx.qlogo.cn/"))
        {
            return string.Format("{0}/{1}", str, "96");
        }
        if ((str != null) && str.StartsWith("http://q.qlogo.cn/"))
        {
            return string.Format("{0}/{1}", str, "100");
        }
        switch (Singleton<ApolloHelper>.GetInstance().CurPlatform)
        {
            case ApolloPlatform.Wechat:
                return string.Format("{0}/{1}", str, "96");

            case ApolloPlatform.QQ:
            case ApolloPlatform.WTLogin:
                return string.Format("{0}/{1}", str, "100");
        }
        return StringHelper.UTF8BytesToString(ref szUrl);
    }

    public ApolloPlatform CurPlatform
    {
        get
        {
            return this.m_CurPlatform;
        }
        set
        {
            this.m_CurPlatform = value;
        }
    }

    public bool IsLoginReturn
    {
        get
        {
            return this.m_IsLoginReturn;
        }
    }

    public bool IsSwitchToLoginPlatform
    {
        get
        {
            return this.m_IsSwitchToLoginPlatform;
        }
        set
        {
            this.m_IsSwitchToLoginPlatform = value;
        }
    }

    public bool IsWXGameCenter
    {
        get
        {
            return this.m_IsWXGameCenter;
        }
        set
        {
            this.m_IsWXGameCenter = value;
        }
    }
}

