namespace Assets.Scripts.GameSystem
{
    using Apollo;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class ShareSys : MonoSingleton<ShareSys>
    {
        private const string HeroShowImgDir = "UGUI/Sprite/Dynamic/HeroShow/";
        private bool isRegisterd;
        private bool m_bClickShareFriendBtn;
        private bool m_bClickTimeLineBtn;
        public bool m_bHide;
        private bool m_bShareHero;
        private bool m_bSharePvpForm;
        public bool m_bShowTimeline;
        public bool m_bWinPVPResult;
        public SHARE_INFO m_ShareInfo;
        private string m_sharePic = CFileManager.GetCachePath("share.png");
        private ListView<CSDT_SHARE_TLOG_INFO> m_ShareReportInfoList = new ListView<CSDT_SHARE_TLOG_INFO>();
        private Transform m_TimelineBtn;
        public static string s_formShareMysteryDiscountPath = "UGUI/Form/System/ShareUI/Form_ShareMystery_Discount.prefab";
        public static string s_formShareNewAchievementPath = "UGUI/Form/System/Achieve/Form_Achievement_ShareNewAchievement.prefab";
        public static string s_formShareNewHeroPath = "UGUI/Form/System/ShareUI/Form_ShareNewHero.prefab";
        public static string s_formSharePVPPath = "UGUI/Form/System/ShareUI/Form_SharePVPResult.prefab";
        public static string s_imageSharePVPAchievement = (CUIUtility.s_Sprite_Dynamic_PvpAchievementShare_Dir + "Img_PVP_ShareAchievement_");
        public static readonly string SNS_SHARE_COMMON = "SNS_SHARE_SEND_COMMON";
        public static readonly string SNS_SHARE_RECALL_FRIEND = "SNS_SHARE_RECALL_FRIEND";
        public static readonly string SNS_SHARE_SEND_HEART = "SNS_SHARE_SEND_HEART";

        public void AddshareReportInfo(uint dwType, uint dwSubType)
        {
            bool flag = false;
            for (int i = 0; i < this.m_ShareReportInfoList.Count; i++)
            {
                CSDT_SHARE_TLOG_INFO csdt_share_tlog_info = this.m_ShareReportInfoList[i];
                if ((csdt_share_tlog_info.dwType == dwType) && (csdt_share_tlog_info.dwSubType == dwSubType))
                {
                    csdt_share_tlog_info.dwCnt++;
                    flag = true;
                }
            }
            if (!flag)
            {
                CSDT_SHARE_TLOG_INFO item = new CSDT_SHARE_TLOG_INFO {
                    dwCnt = 1,
                    dwType = dwType,
                    dwSubType = dwSubType
                };
                this.m_ShareReportInfoList.Add(item);
            }
        }

        [DebuggerHidden]
        private IEnumerator Capture(Rect screenShotRect, Action<string> callback)
        {
            return new <Capture>c__Iterator1C { screenShotRect = screenShotRect, callback = callback, <$>screenShotRect = screenShotRect, <$>callback = callback, <>f__this = this };
        }

        public void CloseNewHeroForm(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CUICommonSystem.s_newHeroOrSkinPath);
            if (form != null)
            {
                DynamicShadow.EnableDynamicShow(form.gameObject, false);
            }
            Singleton<CUIManager>.GetInstance().CloseForm(CUICommonSystem.s_newHeroOrSkinPath);
            this.m_bShowTimeline = false;
            Singleton<EventRouter>.instance.BroadCastEvent(EventID.Mall_Get_Product_OK);
        }

        public void CloseShareHeroForm(CUIEvent uiEvent)
        {
            Singleton<CUIManager>.GetInstance().CloseForm(s_formShareNewHeroPath);
            this.m_bShareHero = false;
            this.m_bClickShareFriendBtn = false;
        }

        private GameObject GetCloseBtn(CUIFormScript form)
        {
            if (form != null)
            {
                if (form.m_formPath == s_formShareNewHeroPath)
                {
                    return form.GetWidget(2);
                }
                if (form.m_formPath == s_formSharePVPPath)
                {
                    return form.GetWidget(1);
                }
                if (form.m_formPath == s_formShareNewAchievementPath)
                {
                    return form.GetWidget(3);
                }
                if (form.m_formPath == s_formShareMysteryDiscountPath)
                {
                    return form.GetWidget(1);
                }
                if (form.m_formPath == PvpAchievementForm.s_formSharePVPDefeatPath)
                {
                    return form.GetWidget(0);
                }
                if (form.m_formPath == Singleton<SettlementSystem>.GetInstance().SettlementFormName)
                {
                    return form.gameObject.transform.FindChild("Panel/Btn_Share_PVP_DATA_CLOSE").gameObject;
                }
                if (form.m_formPath == "UGUI/Form/System/ShareUI/Form_SharePVPLadder.prefab")
                {
                    return form.gameObject.transform.FindChild("Button_Close").gameObject;
                }
                object[] inParameters = new object[] { form.m_formPath };
                DebugHelper.Assert(false, "ShareSys.GetCloseBtn(): error form path = {0}", inParameters);
            }
            return null;
        }

        private GameObject GetDisplayPanel(CUIFormScript form)
        {
            if (form != null)
            {
                if (form.m_formPath == s_formShareNewHeroPath)
                {
                    return form.GetWidget(0);
                }
                if (form.m_formPath == s_formSharePVPPath)
                {
                    return form.GetWidget(0);
                }
                if (form.m_formPath == s_formShareNewAchievementPath)
                {
                    return form.GetWidget(4);
                }
                if (form.m_formPath == PvpAchievementForm.s_formSharePVPDefeatPath)
                {
                    return form.GetWidget(2);
                }
                if (form.m_formPath == Singleton<SettlementSystem>.GetInstance().SettlementFormName)
                {
                    return form.gameObject.transform.FindChild("Panel").gameObject;
                }
                if (form.m_formPath == "UGUI/Form/System/ShareUI/Form_SharePVPLadder.prefab")
                {
                    return form.gameObject.transform.FindChild("ShareFrame").gameObject;
                }
                object[] inParameters = new object[] { form.m_formPath };
                DebugHelper.Assert(false, "ShareSys.GetDisplayPanel(): error form path = {0}", inParameters);
            }
            return null;
        }

        private Rect GetScreenShotRect(GameObject displayeRect)
        {
            RectTransform transform = (displayeRect == null) ? new RectTransform() : displayeRect.GetComponent<RectTransform>();
            float x = transform.rect.width * 0.5f;
            float y = transform.rect.height * 0.5f;
            Vector3 position = displayeRect.transform.TransformPoint(new Vector3(-x, -y, 0f));
            position = Singleton<CUIManager>.instance.FormCamera.WorldToScreenPoint(position);
            Vector3 vector2 = displayeRect.transform.TransformPoint(new Vector3(x, y, 0f));
            vector2 = Singleton<CUIManager>.instance.FormCamera.WorldToScreenPoint(vector2);
            float width = Math.Abs((float) (position.x - vector2.x));
            return new Rect(position.x, position.y, width, Math.Abs((float) (position.y - vector2.y)));
        }

        protected override void Init()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Share_CloseNewHeroorSkin, new CUIEventManager.OnUIEventHandler(this.CloseNewHeroForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Share_NewHero, new CUIEventManager.OnUIEventHandler(this.OpenShareNewHeroForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Share_CloseNewHeroShareForm, new CUIEventManager.OnUIEventHandler(this.CloseShareHeroForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Share_ShareFriend, new CUIEventManager.OnUIEventHandler(this.ShareFriend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Share_ShareTimeLine, new CUIEventManager.OnUIEventHandler(this.ShareTimeLine));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Share_ShareSavePic, new CUIEventManager.OnUIEventHandler(this.SavePic));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Share_SharePVPScore, new CUIEventManager.OnUIEventHandler(this.SettlementShareBtnHandle));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Share_SharePVPSCcoreClose, new CUIEventManager.OnUIEventHandler(this.OnCloseShowPVPSCore));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Share_MysteryDiscount, new CUIEventManager.OnUIEventHandler(this.ShareMysteryDiscount));
            uint dwConfValue = GameDataMgr.globalInfoDatabin.GetDataByKey(0x88).dwConfValue;
            Singleton<CTimerManager>.GetInstance().AddTimer((int) (dwConfValue * 0x3e8), -1, new CTimer.OnTimeUpHandler(this.OnReportShareInfo));
            Singleton<EventRouter>.instance.AddEventHandler(EventID.SHARE_PVP_SETTLEDATA_CLOSE, new Action(this, (IntPtr) this.On_SHARE_PVP_SETTLEDATA_CLOSE));
        }

        public bool IsInstallPlatform()
        {
            if (Singleton<ApolloHelper>.GetInstance().IsPlatformInstalled(Singleton<ApolloHelper>.GetInstance().CurPlatform))
            {
                return true;
            }
            if (Singleton<ApolloHelper>.GetInstance().CurPlatform == ApolloPlatform.Wechat)
            {
                Singleton<CUIManager>.GetInstance().OpenMessageBox("未安装微信，无法使用该功能", false);
            }
            else if (Singleton<ApolloHelper>.GetInstance().CurPlatform == ApolloPlatform.QQ)
            {
                Singleton<CUIManager>.GetInstance().OpenMessageBox("未安装手机QQ，无法使用该功能", false);
            }
            return false;
        }

        private void On_SHARE_PVP_SETTLEDATA_CLOSE()
        {
            this.m_bSharePvpForm = false;
        }

        public void OnCloseShowPVPSCore(CUIEvent ievent)
        {
            Singleton<CUIManager>.GetInstance().CloseForm(s_formSharePVPPath);
            this.m_bSharePvpForm = false;
            MonoSingleton<NewbieGuideManager>.GetInstance().CheckTriggerTime(NewbieGuideTriggerTimeType.PvPShareFin, new uint[0]);
        }

        private static void OnLoadNewHeroOrSkin3DModel(GameObject rawImage, uint heroId, uint skinId, bool bInitAnima)
        {
            CUI3DImageScript script = (rawImage == null) ? null : rawImage.GetComponent<CUI3DImageScript>();
            string objectName = CUICommonSystem.GetHeroPrefabPath(heroId, (int) skinId, true).ObjectName;
            GameObject model = (script == null) ? null : script.AddGameObject(objectName, false, false);
            CHeroAnimaSystem instance = Singleton<CHeroAnimaSystem>.GetInstance();
            instance.Set3DModel(model);
            if (model == null)
            {
                objectName = null;
            }
            else if (bInitAnima)
            {
                instance.InitAnimatList();
                instance.InitAnimatSoundList(heroId, skinId);
                instance.OnModePlayAnima("Come");
            }
        }

        private void OnReportShareInfo(int timerSequence)
        {
            if (!Singleton<CBattleSystem>.instance.m_isInBattle)
            {
                this.ReportShareInfo();
            }
        }

        public void OnShareCallBack()
        {
            IApolloSnsService service = IApollo.Instance.GetService(1) as IApolloSnsService;
            if ((service != null) && !this.isRegisterd)
            {
                service.onShareEvent += delegate (ApolloShareResult shareResponseInfo) {
                    object[] args = new object[] { shareResponseInfo.result, shareResponseInfo.platform, shareResponseInfo.drescription, shareResponseInfo.extInfo };
                    Debug.Log("sns += " + string.Format("share result:{0} \n share platform:{1} \n share description:{2}\n share extInfo:{3}", args));
                    if (shareResponseInfo.result == ApolloResult.Success)
                    {
                        if (shareResponseInfo.extInfo == SNS_SHARE_SEND_HEART)
                        {
                            Singleton<CUIManager>.instance.OpenTips("Common_Sns_Tips_7", true, 1f, null, new object[0]);
                        }
                        else if (shareResponseInfo.extInfo != SNS_SHARE_RECALL_FRIEND)
                        {
                            if (this.m_bClickTimeLineBtn)
                            {
                                this.m_bShowTimeline = true;
                                this.UpdateTimelineBtn();
                            }
                            uint dwType = 0;
                            if (this.m_bShareHero)
                            {
                                dwType = 0;
                            }
                            else if (this.m_bSharePvpForm)
                            {
                                dwType = 1;
                            }
                            if (this.m_bClickShareFriendBtn)
                            {
                                if (ApolloConfig.platform == ApolloPlatform.Wechat)
                                {
                                    this.AddshareReportInfo(dwType, 3);
                                }
                                else if (ApolloConfig.platform == ApolloPlatform.QQ)
                                {
                                    this.AddshareReportInfo(dwType, 2);
                                }
                            }
                            else if (this.m_bClickTimeLineBtn)
                            {
                                if (ApolloConfig.platform == ApolloPlatform.Wechat)
                                {
                                    this.AddshareReportInfo(dwType, 5);
                                }
                                else if (ApolloConfig.platform == ApolloPlatform.QQ)
                                {
                                    this.AddshareReportInfo(dwType, 4);
                                }
                            }
                            Singleton<CUIManager>.instance.OpenTips("Common_Sns_Tips_7", true, 1f, null, new object[0]);
                            CTaskSys.Send_Share_Task();
                            this.m_bClickTimeLineBtn = false;
                            this.m_bClickShareFriendBtn = false;
                        }
                    }
                    else
                    {
                        this.m_bClickTimeLineBtn = false;
                        this.m_bClickShareFriendBtn = false;
                    }
                };
                this.isRegisterd = true;
            }
        }

        [MessageHandler(0x10d4)]
        public static void OnShareReport(CSPkg msg)
        {
            Debug.Log("share report " + msg.stPkgData.stShareTLogRsp.iErrCode);
        }

        public void OpenShareNewHeroForm(CUIEvent uiEvent)
        {
            this.AddshareReportInfo(0, 0);
            this.m_bShareHero = true;
            this.m_bClickShareFriendBtn = false;
            this.ShowNewHeroShare(this.m_ShareInfo.heroId, this.m_ShareInfo.skinId, false, this.m_ShareInfo.rewardType, false);
        }

        public void OpenShowSharePVPFrom(RES_SHOW_ACHIEVEMENT_TYPE type)
        {
            this.m_bSharePvpForm = true;
            CUIFormScript form = Singleton<CUIManager>.GetInstance().OpenForm(s_formSharePVPPath, false, true);
            this.UpdateSharePVPForm(form, form.GetWidget(0));
            CUIUtility.SetImageSprite(form.GetWidget(2).GetComponent<Image>(), s_imageSharePVPAchievement + ((int) type) + ".prefab", form, true, false, false);
        }

        private void RefeshPhoto(string filename)
        {
            AndroidJavaClass class2 = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (class2 != null)
            {
                AndroidJavaObject @static = class2.GetStatic<AndroidJavaObject>("currentActivity");
                if (@static != null)
                {
                    Debug.Log("RefeshPhoto in unity");
                    object[] args = new object[] { filename };
                    @static.Call("RefeshPhoto", args);
                }
            }
        }

        private void ReportShareInfo()
        {
            int count = this.m_ShareReportInfoList.Count;
            if (count > 0)
            {
                CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x10d3);
                msg.stPkgData.stShareTLogReq.bNum = (byte) count;
                for (int i = 0; i < count; i++)
                {
                    msg.stPkgData.stShareTLogReq.astShareDetail[i] = this.m_ShareReportInfoList[i];
                }
                Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
                this.m_ShareReportInfoList.Clear();
            }
        }

        public void SavePic(CUIEvent uiEvent)
        {
            <SavePic>c__AnonStorey55 storey = new <SavePic>c__AnonStorey55 {
                <>f__this = this,
                btnClose = this.GetCloseBtn(uiEvent.m_srcFormScript)
            };
            if (storey.btnClose != null)
            {
                if (storey.btnClose != null)
                {
                    storey.btnClose.CustomSetActive(false);
                }
                Singleton<CUIManager>.instance.CloseTips();
                storey.bSettltment = false;
                if (uiEvent.m_srcFormScript.m_formPath == Singleton<SettlementSystem>.GetInstance().SettlementFormName)
                {
                    storey.bSettltment = true;
                    Singleton<SettlementSystem>.GetInstance().SnapScreenShotShowBtn(false);
                }
                GameObject displayPanel = this.GetDisplayPanel(uiEvent.m_srcFormScript);
                if (displayPanel != null)
                {
                    Rect screenShotRect = this.GetScreenShotRect(displayPanel);
                    base.StartCoroutine(this.Capture(screenShotRect, new Action<string>(storey.<>m__65)));
                    uint dwType = 0;
                    if (this.m_bShareHero)
                    {
                        dwType = 0;
                    }
                    else if (this.m_bSharePvpForm)
                    {
                        dwType = 1;
                    }
                    this.AddshareReportInfo(dwType, 1);
                }
            }
        }

        public static void SetSharePlatfText(Text platText)
        {
            if (null != platText)
            {
                if (ApolloConfig.platform == ApolloPlatform.QQ)
                {
                    platText.text = "分享空间";
                }
                else
                {
                    platText.text = "分享朋友圈";
                }
            }
        }

        private void SettlementShareBtnHandle(CUIEvent ievent)
        {
            if (!MonoSingleton<NewbieGuideManager>.GetInstance().isNewbieGuiding)
            {
                this.AddshareReportInfo(1, 0);
                this.OpenShowSharePVPFrom(RES_SHOW_ACHIEVEMENT_TYPE.RES_SHOW_ACHIEVEMENT_COUNT);
            }
        }

        private void Share(string buttonType)
        {
            IApolloSnsService service = IApollo.Instance.GetService(1) as IApolloSnsService;
            if (service != null)
            {
                FileStream stream = new FileStream(this.m_sharePic, FileMode.Open, FileAccess.Read);
                byte[] array = new byte[stream.Length];
                int count = Convert.ToInt32(stream.Length);
                stream.Read(array, 0, count);
                stream.Close();
                this.OnShareCallBack();
                if (ApolloConfig.platform == ApolloPlatform.Wechat)
                {
                    if (buttonType == "TimeLine/Qzone")
                    {
                        service.SendToWeixinWithPhoto(ApolloShareScene.TimeLine, "MSG_INVITE", array, count, string.Empty, "WECHAT_SNS_JUMP_APP");
                    }
                    else if (buttonType == "Session")
                    {
                        service.SendToWeixinWithPhoto(ApolloShareScene.Session, "apollo test", array, count);
                    }
                }
                else if (ApolloConfig.platform == ApolloPlatform.QQ)
                {
                    if (buttonType == "TimeLine/Qzone")
                    {
                        service.SendToQQWithPhoto(ApolloShareScene.TimeLine, this.m_sharePic);
                    }
                    else if (buttonType == "Session")
                    {
                        service.SendToQQWithPhoto(ApolloShareScene.QSession, this.m_sharePic);
                    }
                }
            }
        }

        public void ShareFriend(CUIEvent uiEvent)
        {
            <ShareFriend>c__AnonStorey53 storey = new <ShareFriend>c__AnonStorey53 {
                <>f__this = this
            };
            if (this.IsInstallPlatform())
            {
                storey.btnClose = this.GetCloseBtn(uiEvent.m_srcFormScript);
                if (storey.btnClose != null)
                {
                    Singleton<CUIManager>.instance.CloseTips();
                    storey.btnClose.CustomSetActive(false);
                    storey.bSettltment = false;
                    if (uiEvent.m_srcFormScript.m_formPath == Singleton<SettlementSystem>.GetInstance().SettlementFormName)
                    {
                        storey.bSettltment = true;
                        Singleton<SettlementSystem>.GetInstance().SnapScreenShotShowBtn(false);
                    }
                    GameObject displayPanel = this.GetDisplayPanel(uiEvent.m_srcFormScript);
                    if (displayPanel != null)
                    {
                        Rect screenShotRect = this.GetScreenShotRect(displayPanel);
                        this.m_bClickShareFriendBtn = true;
                        base.StartCoroutine(this.Capture(screenShotRect, new Action<string>(storey.<>m__63)));
                    }
                }
            }
        }

        private void ShareMysteryDiscount(CUIEvent uiEvent)
        {
            CMallMysteryShop instance = Singleton<CMallMysteryShop>.GetInstance();
            if (instance.HasGotDiscount)
            {
                CUIFormScript formScript = Singleton<CUIManager>.GetInstance().OpenForm(s_formShareMysteryDiscountPath, false, true);
                DebugHelper.Assert(formScript != null, "神秘商店分享form打开失败");
                if (formScript != null)
                {
                    GameObject widget = formScript.GetWidget(0);
                    if (widget != null)
                    {
                        Image component = widget.GetComponent<Image>();
                        if (component != null)
                        {
                            component.SetSprite(instance.GetDiscountNumIconPath(instance.Discount), formScript, true, false, false);
                        }
                    }
                    GameObject obj3 = formScript.GetWidget(2);
                    if (obj3 != null)
                    {
                        SetSharePlatfText(obj3.GetComponent<Text>());
                    }
                }
            }
        }

        public void ShareTimeLine(CUIEvent uiEvent)
        {
            <ShareTimeLine>c__AnonStorey54 storey = new <ShareTimeLine>c__AnonStorey54 {
                <>f__this = this
            };
            if (this.IsInstallPlatform())
            {
                storey.btnClose = this.GetCloseBtn(uiEvent.m_srcFormScript);
                if ((storey.btnClose != null) && !this.m_bClickTimeLineBtn)
                {
                    this.m_bClickTimeLineBtn = true;
                    this.m_TimelineBtn = uiEvent.m_srcWidget.transform;
                    storey.btnClose.CustomSetActive(false);
                    Singleton<CUIManager>.instance.CloseTips();
                    storey.bSettltment = false;
                    if (uiEvent.m_srcFormScript.m_formPath == Singleton<SettlementSystem>.GetInstance().SettlementFormName)
                    {
                        storey.bSettltment = true;
                        Singleton<SettlementSystem>.GetInstance().SnapScreenShotShowBtn(false);
                    }
                    GameObject displayPanel = this.GetDisplayPanel(uiEvent.m_srcFormScript);
                    if (displayPanel != null)
                    {
                        Rect screenShotRect = this.GetScreenShotRect(displayPanel);
                        base.StartCoroutine(this.Capture(screenShotRect, new Action<string>(storey.<>m__64)));
                    }
                }
            }
        }

        public void ShowNewHeroShare(uint heroId, uint skinId, bool bInitAnima = true, COM_REWARDS_TYPE rewardType = 5, bool interactableTransition = false)
        {
            CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(s_formShareNewHeroPath, false, true);
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CUICommonSystem.s_newHeroOrSkinPath);
            enFormPriority priority = enFormPriority.Priority1;
            if (form != null)
            {
                priority = form.m_priority;
            }
            script.SetPriority(priority);
            script.GetWidget(2).CustomSetActive(true);
            script.GetWidget(0).GetComponent<Image>().SetSprite(CUIUtility.GetSpritePrefeb("UGUI/Sprite/Dynamic/HeroShow/" + heroId, false, false));
            SetSharePlatfText(script.GetWidget(3).GetComponent<Text>());
            if (this.m_bShowTimeline)
            {
                Transform transform = null;
                foreach (Text text in script.transform.GetComponentsInChildren<Text>())
                {
                    if (((text != null) && (text.text == "分享空间")) || (text.text == "分享朋友圈"))
                    {
                        Transform parent = text.transform.parent;
                        if (parent.GetComponent<Button>() != null)
                        {
                            transform = parent;
                            break;
                        }
                    }
                }
                if (transform != null)
                {
                    GameObject gameObject = transform.gameObject;
                    if ((gameObject != null) || this.m_bShowTimeline)
                    {
                        gameObject.GetComponent<CUIEventScript>().enabled = false;
                        gameObject.GetComponent<Button>().interactable = false;
                        gameObject.GetComponent<Image>().color = new Color(gameObject.GetComponent<Image>().color.r, gameObject.GetComponent<Image>().color.g, gameObject.GetComponent<Image>().color.b, 0.37f);
                        Text componentInChildren = gameObject.GetComponentInChildren<Text>();
                        componentInChildren.color = new Color(componentInChildren.color.r, componentInChildren.color.g, componentInChildren.color.b, 0.37f);
                    }
                }
            }
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                script.GetWidget(1).GetComponent<Text>().text = masterRoleInfo.GetHaveHeroCount(false).ToString();
            }
        }

        public void UpdateShareGradeForm(CUIFormScript form)
        {
            if (form != null)
            {
                SetSharePlatfText(Utility.GetComponetInChild<Text>(form.gameObject, "ShareGroup/Button_TimeLine/ClickText"));
                if (this.m_bShowTimeline)
                {
                    Transform transform = null;
                    foreach (Text text in form.transform.GetComponentsInChildren<Text>())
                    {
                        if (((text != null) && (text.text == "分享空间")) || (text.text == "分享朋友圈"))
                        {
                            Transform parent = text.transform.parent;
                            if (parent.GetComponent<Button>() != null)
                            {
                                transform = parent;
                                break;
                            }
                        }
                    }
                    if (transform != null)
                    {
                        GameObject gameObject = transform.gameObject;
                        if ((gameObject != null) || this.m_bShowTimeline)
                        {
                            gameObject.GetComponent<CUIEventScript>().enabled = false;
                            gameObject.GetComponent<Button>().interactable = false;
                            gameObject.GetComponent<Image>().color = new Color(gameObject.GetComponent<Image>().color.r, gameObject.GetComponent<Image>().color.g, gameObject.GetComponent<Image>().color.b, 0.37f);
                            Text componentInChildren = gameObject.GetComponentInChildren<Text>();
                            componentInChildren.color = new Color(componentInChildren.color.r, componentInChildren.color.g, componentInChildren.color.b, 0.37f);
                        }
                    }
                }
            }
        }

        public void UpdateSharePVPForm(CUIFormScript form, GameObject shareRootGO)
        {
            if (form != null)
            {
                SetSharePlatfText(Utility.GetComponetInChild<Text>(form.gameObject, "ShareGroup/Button_TimeLine/ClickText"));
                if (this.m_bShowTimeline)
                {
                    Transform transform = null;
                    foreach (Text text in form.transform.GetComponentsInChildren<Text>())
                    {
                        if (((text != null) && (text.text == "分享空间")) || (text.text == "分享朋友圈"))
                        {
                            Transform parent = text.transform.parent;
                            if (parent.GetComponent<Button>() != null)
                            {
                                transform = parent;
                                break;
                            }
                        }
                    }
                    if (transform != null)
                    {
                        GameObject gameObject = transform.gameObject;
                        if ((gameObject != null) || this.m_bShowTimeline)
                        {
                            gameObject.GetComponent<CUIEventScript>().enabled = false;
                            gameObject.GetComponent<Button>().interactable = false;
                            gameObject.GetComponent<Image>().color = new Color(gameObject.GetComponent<Image>().color.r, gameObject.GetComponent<Image>().color.g, gameObject.GetComponent<Image>().color.b, 0.37f);
                            Text componentInChildren = gameObject.GetComponentInChildren<Text>();
                            componentInChildren.color = new Color(componentInChildren.color.r, componentInChildren.color.g, componentInChildren.color.b, 0.37f);
                        }
                    }
                }
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                if (masterRoleInfo != null)
                {
                    CUIHttpImageScript componetInChild = Utility.GetComponetInChild<CUIHttpImageScript>(shareRootGO, "PlayerHead");
                    componetInChild.SetImageUrl(masterRoleInfo.HeadUrl);
                    Utility.GetComponetInChild<Text>(shareRootGO, "PlayerName").text = masterRoleInfo.Name;
                    DictionaryView<uint, PlayerKDA>.Enumerator enumerator = Singleton<BattleLogic>.GetInstance().battleStat.m_playerKDAStat.GetEnumerator();
                    PlayerKDA rkda = null;
                    int[] numArray = new int[3];
                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                        PlayerKDA rkda2 = current.Value;
                        if (rkda2.IsHost)
                        {
                            rkda = rkda2;
                        }
                        numArray[(int) rkda2.PlayerCamp] += rkda2.numKill;
                    }
                    Utility.FindChild(componetInChild.gameObject, "Mvp").CustomSetActive(Singleton<BattleStatistic>.instance.GetMvpPlayer(rkda.PlayerCamp, this.m_bWinPVPResult) == rkda.PlayerId);
                    if (rkda != null)
                    {
                        Utility.GetComponetInChild<Text>(shareRootGO, "HostKillNum").text = rkda.numKill.ToString();
                        Utility.GetComponetInChild<Text>(shareRootGO, "HostDeadNum").text = rkda.numDead.ToString();
                        Utility.GetComponetInChild<Text>(shareRootGO, "HostAssistNum").text = rkda.numAssist.ToString();
                        Utility.GetComponetInChild<Text>(shareRootGO, "HostKillTotalNum").text = numArray[(int) rkda.PlayerCamp].ToString();
                        Utility.GetComponetInChild<Text>(shareRootGO, "OppoKillTotalNum").text = numArray[(int) BattleLogic.GetOppositeCmp(rkda.PlayerCamp)].ToString();
                        IEnumerator<HeroKDA> enumerator2 = rkda.GetEnumerator();
                        if (enumerator2.MoveNext())
                        {
                            HeroKDA okda = enumerator2.Current;
                            ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey((uint) okda.HeroId);
                            Utility.GetComponetInChild<Image>(shareRootGO, "HeroHead").SetSprite(CUIUtility.s_Sprite_Dynamic_Icon_Dir + StringHelper.UTF8BytesToString(ref dataByKey.szImagePath), form, true, false, false);
                            int num2 = 1;
                            for (int i = 1; i < 13; i++)
                            {
                                switch (((PvpAchievement) i))
                                {
                                    case PvpAchievement.Legendary:
                                        if (okda.LegendaryNum > 0)
                                        {
                                            CSettlementView.SetAchievementIcon(form, shareRootGO, PvpAchievement.Legendary, num2++);
                                        }
                                        break;

                                    case PvpAchievement.DoubleKill:
                                        if (okda.DoubleKillNum <= 0)
                                        {
                                        }
                                        break;

                                    case PvpAchievement.TripleKill:
                                        if (okda.TripleKillNum > 0)
                                        {
                                            CSettlementView.SetAchievementIcon(form, shareRootGO, PvpAchievement.TripleKill, num2++);
                                        }
                                        break;

                                    case PvpAchievement.KillMost:
                                        if (okda.bKillMost)
                                        {
                                            CSettlementView.SetAchievementIcon(form, shareRootGO, PvpAchievement.KillMost, num2++);
                                        }
                                        break;

                                    case PvpAchievement.HurtMost:
                                        if (okda.bHurtMost)
                                        {
                                            CSettlementView.SetAchievementIcon(form, shareRootGO, PvpAchievement.HurtMost, num2++);
                                        }
                                        break;

                                    case PvpAchievement.HurtTakenMost:
                                        if (okda.bHurtTakenMost)
                                        {
                                            CSettlementView.SetAchievementIcon(form, shareRootGO, PvpAchievement.HurtTakenMost, num2++);
                                        }
                                        break;

                                    case PvpAchievement.AsssistMost:
                                        if (okda.bAsssistMost)
                                        {
                                            CSettlementView.SetAchievementIcon(form, shareRootGO, PvpAchievement.AsssistMost, num2++);
                                        }
                                        break;
                                }
                            }
                            for (int j = num2; j <= 6; j++)
                            {
                                CSettlementView.SetAchievementIcon(form, shareRootGO, PvpAchievement.NULL, j);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateTimelineBtn()
        {
            if (this.m_TimelineBtn != null)
            {
                GameObject gameObject = this.m_TimelineBtn.gameObject;
                if (this.m_bShowTimeline && (gameObject != null))
                {
                    gameObject.GetComponent<CUIEventScript>().enabled = false;
                    gameObject.GetComponent<Button>().interactable = false;
                    gameObject.GetComponent<Image>().color = new Color(gameObject.GetComponent<Image>().color.r, gameObject.GetComponent<Image>().color.g, gameObject.GetComponent<Image>().color.b, 0.37f);
                    Text componentInChildren = gameObject.GetComponentInChildren<Text>();
                    componentInChildren.color = new Color(componentInChildren.color.r, componentInChildren.color.g, componentInChildren.color.b, 0.37f);
                }
                this.m_TimelineBtn = null;
            }
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Share_ClosePVPAchievement);
        }

        [CompilerGenerated]
        private sealed class <Capture>c__Iterator1C : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal Action<string> <$>callback;
            internal Rect <$>screenShotRect;
            internal ShareSys <>f__this;
            internal byte[] <data>__2;
            internal Exception <e>__3;
            internal string <filename>__0;
            internal Texture2D <result>__1;
            internal Action<string> callback;
            internal Rect screenShotRect;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.$current = new WaitForEndOfFrame();
                        this.$PC = 1;
                        return true;

                    case 1:
                        try
                        {
                            this.<filename>__0 = this.<>f__this.m_sharePic;
                            this.<result>__1 = new Texture2D((int) this.screenShotRect.width, (int) this.screenShotRect.height, TextureFormat.RGB24, false);
                            this.<result>__1.ReadPixels(this.screenShotRect, 0, 0);
                            this.<result>__1.Apply();
                            this.<data>__2 = this.<result>__1.EncodeToPNG();
                            File.WriteAllBytes(this.<filename>__0, this.<data>__2);
                            if (this.callback != null)
                            {
                                this.callback(this.<filename>__0);
                            }
                        }
                        catch (Exception exception)
                        {
                            this.<e>__3 = exception;
                            object[] inParameters = new object[] { this.<e>__3.Message };
                            DebugHelper.Assert(false, "Exception in ShareSys.Capture, {0}", inParameters);
                        }
                        this.$PC = -1;
                        break;
                }
                return false;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <SavePic>c__AnonStorey55
        {
            internal ShareSys <>f__this;
            internal bool bSettltment;
            internal GameObject btnClose;

            internal void <>m__65(string filename)
            {
                if (this.btnClose != null)
                {
                    this.btnClose.CustomSetActive(true);
                }
                if (this.bSettltment)
                {
                    Singleton<SettlementSystem>.GetInstance().SnapScreenShotShowBtn(true);
                }
                if (Application.platform == RuntimePlatform.Android)
                {
                    try
                    {
                        string path = "/mnt/sdcard/DCIM/Sgame";
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        path = string.Format("{0}/share_{1}.png", path, DateTime.Now.ToFileTimeUtc());
                        Debug.Log("sns += SavePic " + path);
                        FileStream stream = new FileStream(this.<>f__this.m_sharePic, FileMode.Open, FileAccess.Read);
                        byte[] array = new byte[stream.Length];
                        int count = Convert.ToInt32(stream.Length);
                        stream.Read(array, 0, count);
                        stream.Close();
                        File.WriteAllBytes(path, array);
                        this.<>f__this.RefeshPhoto(path);
                        Singleton<CUIManager>.instance.OpenTips("成功保存到相册", false, 1f, null, new object[0]);
                    }
                    catch (Exception exception)
                    {
                        object[] inParameters = new object[] { exception.Message };
                        DebugHelper.Assert(false, "Error In Save Pic, {0}", inParameters);
                        Singleton<CUIManager>.instance.OpenTips("保存到相册出错", false, 1f, null, new object[0]);
                    }
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    this.<>f__this.RefeshPhoto(this.<>f__this.m_sharePic);
                    Singleton<CUIManager>.instance.OpenTips("成功保存到相册", false, 1f, null, new object[0]);
                }
            }
        }

        [CompilerGenerated]
        private sealed class <ShareFriend>c__AnonStorey53
        {
            internal ShareSys <>f__this;
            internal bool bSettltment;
            internal GameObject btnClose;

            internal void <>m__63(string filename)
            {
                Debug.Log("sns += capture showfriend filename" + filename);
                this.<>f__this.Share("Session");
                this.btnClose.CustomSetActive(true);
                if (this.bSettltment)
                {
                    Singleton<SettlementSystem>.GetInstance().SnapScreenShotShowBtn(true);
                    this.bSettltment = false;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <ShareTimeLine>c__AnonStorey54
        {
            internal ShareSys <>f__this;
            internal bool bSettltment;
            internal GameObject btnClose;

            internal void <>m__64(string filename)
            {
                Debug.Log("sns += capture showfriend filename" + filename);
                this.<>f__this.Share("TimeLine/Qzone");
                this.btnClose.CustomSetActive(true);
                if (this.bSettltment)
                {
                    Singleton<SettlementSystem>.GetInstance().SnapScreenShotShowBtn(true);
                    this.bSettltment = false;
                }
            }
        }

        private enum HeroShareFormWidgets
        {
            DisplayRect,
            HeroAmount,
            ButtonClose,
            ShareClickText
        }

        private enum MysteryDiscountFOrmWigets
        {
            DiscountNum,
            ButtonClose,
            ShareClickText
        }

        public enum PVPShareFormWidgets
        {
            DisplayRect,
            ButtonClose,
            ShareImg
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHARE_INFO
        {
            public uint heroId;
            public uint skinId;
            public COM_REWARDS_TYPE rewardType;
        }
    }
}

