namespace Assets.Scripts.GameSystem
{
    using ApolloUpdate;
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using IIPSMobile;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;

    public class CVersionUpdateSystem : MonoSingleton<CVersionUpdateSystem>
    {
        private const string c_homePageUrl = "http://pvp.qq.com";
        private ListView<CAnnouncementImage> m_announcementImages = new ListView<CAnnouncementImage>();
        private ApolloUpdateSpeedCounter m_apolloUpdateSpeedCounter = new ApolloUpdateSpeedCounter();
        private uint m_appDownloadSize;
        private string m_appDownloadUrl;
        private string m_cachedResourceVersion;
        private int m_currentDisplayedAnnouncementImage = -1;
        private string m_downloadAppVersion;
        private uint m_downloadCounter;
        private string m_downloadResourceVersion;
        private uint m_downloadSpeed;
        private DateTime m_fBeginUpdateTime = new DateTime();
        private string m_firstExtractResourceVersion;
        private IIPSMobile.IIPSMobileErrorCodeCheck m_iipsErrorCodeChecker = new IIPSMobile.IIPSMobileErrorCodeCheck();
        private IIPSMobileVersionMgrInterface m_IIPSVersionMgr;
        private IIPSMobileVersion m_IIPSVersionMgrFactory;
        private bool m_isError;
        private OnVersionUpdateComplete m_onVersionUpdateComplete;
        private uint m_recommendUpdateVersionMax;
        private uint m_recommendUpdateVersionMin;
        private bool m_resourceDownloadNeedConfirm;
        private uint m_resourceDownloadSize;
        private bool m_useCurseResourceDownloadSize;
        private CUIFormScript m_versionUpdateFormScript;
        private enVersionUpdateState m_versionUpdateState;
        private static AndroidJavaClass s_androidUtilityJavaClass;
        private static uint s_appID = 1;
        private static enAppType s_appType = enAppType.Unknown;
        private static string s_appVersion = null;
        private static string s_cachedResourceVersionFilePath = null;
        private static string s_downloadedIFSSavePath = null;
        private static string s_firstExtractIFSName = null;
        private static string s_firstExtractIFSPath = null;
        private static string s_ifsExtractPath = null;
        private static enIIPSServerType s_IIPSServerType;
        private static string[][] s_IIPSServerUrls;
        private static enPlatform s_platform = GetPlatform();
        private static string s_resourcePackerInfoSetFullPath = null;
        private static uint[] s_serviceIDsForUpdataApp = new uint[] { 0x9e0005b, 0x9e0005a };
        private static uint[] s_serviceIDsForUpdateResource = new uint[] { 0x9e00010, 0x9e0000f };
        public static string s_splashFormPathFuckLOL0 = "UGUI/Form/System/Login/Form_Splash.prefab";
        public static string s_splashFormPathFuckLOL1 = "UGUI/Form/System/Login/Form_Splash_New.prefab";
        private static string s_versionUpdateFormPath;
        private static string s_waitingFormPath;

        static CVersionUpdateSystem()
        {
            string[][] textArrayArray1 = new string[7][];
            textArrayArray1[0] = new string[] { "tcp://mtcls.qq.com:20001", "tcp://61.151.224.100:20001", "tcp://58.251.61.169:20001", "tcp://203.205.151.237:20001", "tcp://203.205.147.178:20001", "tcp://183.61.49.177:20001", "tcp://183.232.103.166:20001", "tcp://182.254.4.176:20001", "tcp://182.254.10.82:20001", "tcp://140.207.127.61:20001", "tcp://117.144.242.115:20001" };
            textArrayArray1[1] = new string[] { "tcp://middle.mtcls.qq.com:20001", "tcp://101.226.141.88:20001" };
            textArrayArray1[2] = new string[] { "tcp://testa4.mtcls.qq.com:10001", "tcp://101.227.153.83:10001" };
            textArrayArray1[3] = new string[] { "tcp://exp.mtcls.qq.com:10001", "tcp://61.151.234.47:10001", "tcp://182.254.42.103:10001", "tcp://140.207.62.111:10001", "tcp://140.207.123.164:10001", "tcp://117.144.242.28:10001", "tcp://117.135.171.74:10001", "tcp://103.7.30.91:10001", "tcp://101.227.130.79:10001" };
            textArrayArray1[4] = new string[] { "tcp://testb4.mtcls.qq.com:10001", "tcp://101.227.153.86:10001" };
            textArrayArray1[5] = new string[] { "tcp://testc.mtcls.qq.com:10001", "tcp://183.61.39.51:10001" };
            textArrayArray1[6] = new string[] { string.Empty };
            s_IIPSServerUrls = textArrayArray1;
            s_IIPSServerType = GameVersion.IIPSServerType;
            s_versionUpdateFormPath = "UGUI/Form/System/VersionUpdate/Form_VersionUpdate.prefab";
            s_waitingFormPath = "UGUI/Form/Common/Form_SendMsgAlert.prefab";
        }

        private static void Android_ExitApp()
        {
            s_androidUtilityJavaClass.CallStatic("ExitApp", new object[0]);
        }

        private static string Android_GetApkAbsPath()
        {
            return s_androidUtilityJavaClass.CallStatic<string>("GetApkAbsPath", new object[0]);
        }

        private static int Android_GetNetworkType()
        {
            return s_androidUtilityJavaClass.CallStatic<int>("GetNetworkType", new object[0]);
        }

        private void Android_InstallAPK(string path)
        {
            if (this.m_IIPSVersionMgr != null)
            {
                this.m_IIPSVersionMgr.InstallApk(path);
            }
        }

        private static bool Android_IsFileExistInStreamingAssets(string fileName)
        {
            object[] args = new object[] { fileName };
            return s_androidUtilityJavaClass.CallStatic<bool>("IsFileExistInStreamingAssets", args);
        }

        private void CloseWaitingForm()
        {
            Singleton<CUIManager>.GetInstance().CloseForm(s_waitingFormPath);
        }

        private void Complete()
        {
            this.m_versionUpdateState = enVersionUpdateState.End;
            base.StartCoroutine(this.VersionUpdateComplete());
            Singleton<BeaconHelper>.GetInstance().Event_CommonReport("Event_VerUpdateFinish");
            List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("begintime", this.m_fBeginUpdateTime.ToString()),
                new KeyValuePair<string, string>("filesize", this.m_resourceDownloadSize.ToString()),
                new KeyValuePair<string, string>("Version", (string.IsNullOrEmpty(this.m_downloadResourceVersion) == null) ? this.m_downloadResourceVersion.ToString() : string.Empty),
                new KeyValuePair<string, string>("errorCode", "SUCC")
            };
            TimeSpan span = (TimeSpan) (DateTime.Now - this.m_fBeginUpdateTime);
            events.Add(new KeyValuePair<string, string>("totaltime", span.TotalSeconds.ToString()));
            events.Add(new KeyValuePair<string, string>("UpdateType", this.m_resourceDownloadNeedConfirm.ToString()));
            events.Add(new KeyValuePair<string, string>("url", string.Empty));
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_DownloadEvent", events, true);
        }

        private void CreateIIPSVersionMgr(string config)
        {
            CVersionUpdateCallback callback;
            if ((this.m_IIPSVersionMgr != null) || (this.m_IIPSVersionMgrFactory != null))
            {
                this.DisposeIIPSVersionMgr();
            }
            callback = new CVersionUpdateCallback {
                m_onGetNewVersionInfoDelegate = (CVersionUpdateCallback.OnGetNewVersionInfoDelegate) Delegate.Combine(callback.m_onGetNewVersionInfoDelegate, new CVersionUpdateCallback.OnGetNewVersionInfoDelegate(this.OnGetNewVersionInfo)),
                m_onProgressDelegate = (CVersionUpdateCallback.OnProgressDelegate) Delegate.Combine(callback.m_onProgressDelegate, new CVersionUpdateCallback.OnProgressDelegate(this.OnProgress)),
                m_onErrorDelegate = (CVersionUpdateCallback.OnErrorDelegate) Delegate.Combine(callback.m_onErrorDelegate, new CVersionUpdateCallback.OnErrorDelegate(this.OnError)),
                m_onSuccessDelegate = (CVersionUpdateCallback.OnSuccessDelegate) Delegate.Combine(callback.m_onSuccessDelegate, new CVersionUpdateCallback.OnSuccessDelegate(this.OnSuccess)),
                m_onNoticeInstallApkDelegate = (CVersionUpdateCallback.OnNoticeInstallApkDelegate) Delegate.Combine(callback.m_onNoticeInstallApkDelegate, new CVersionUpdateCallback.OnNoticeInstallApkDelegate(this.OnNoticeInstallApk)),
                m_onActionMsgDelegate = (CVersionUpdateCallback.OnActionMsgDelegate) Delegate.Combine(callback.m_onActionMsgDelegate, new CVersionUpdateCallback.OnActionMsgDelegate(this.OnActionMsg))
            };
            this.m_IIPSVersionMgrFactory = new IIPSMobileVersion();
            this.m_IIPSVersionMgr = this.m_IIPSVersionMgrFactory.CreateVersionMgr(callback, config);
        }

        private void DisplayAnnouncementImage(int index)
        {
            if (this.m_currentDisplayedAnnouncementImage != index)
            {
                if (this.m_currentDisplayedAnnouncementImage >= 0)
                {
                    this.EnableAnnouncementImagePointer(this.m_currentDisplayedAnnouncementImage, false);
                }
                this.m_currentDisplayedAnnouncementImage = index;
                if (this.m_currentDisplayedAnnouncementImage >= 0)
                {
                    this.DisplayAnnouncementImage(this.m_announcementImages[this.m_currentDisplayedAnnouncementImage].m_texture2D);
                    this.EnableAnnouncementImagePointer(this.m_currentDisplayedAnnouncementImage, true);
                }
            }
        }

        private void DisplayAnnouncementImage(Texture2D texture2D)
        {
            if (texture2D != null)
            {
                GameObject widget = this.m_versionUpdateFormScript.GetWidget(3);
                if (widget != null)
                {
                    Image component = widget.GetComponent<Image>();
                    if (component != null)
                    {
                        component.SetSprite(Sprite.Create(texture2D, new Rect(0f, 0f, (float) texture2D.width, (float) texture2D.height), new Vector2(0.5f, 0.5f)), ImageAlphaTexLayout.None);
                    }
                }
            }
        }

        private void DisposeAnnouncementImages()
        {
            for (int i = 0; i < this.m_announcementImages.Count; i++)
            {
                this.m_announcementImages[i].Dispose();
            }
            this.m_announcementImages.Clear();
        }

        private void DisposeIIPSVersionMgr()
        {
            if (this.m_IIPSVersionMgr != null)
            {
                this.m_IIPSVersionMgr.MgrUnitVersionManager();
                this.m_IIPSVersionMgr = null;
            }
            if (this.m_IIPSVersionMgrFactory != null)
            {
                this.m_IIPSVersionMgrFactory.DeleteVersionMgr();
                this.m_IIPSVersionMgrFactory = null;
            }
        }

        [DebuggerHidden]
        private IEnumerator DownloadAnnouncementImage()
        {
            return new <DownloadAnnouncementImage>c__Iterator20 { <>f__this = this };
        }

        private void EnableAnnouncementImagePointer(int index, bool enabled)
        {
            GameObject widget = this.m_versionUpdateFormScript.GetWidget(4);
            if (widget != null)
            {
                CUIContainerScript component = widget.GetComponent<CUIContainerScript>();
                if ((component != null) && (index >= 0))
                {
                    GameObject element = component.GetElement(this.m_announcementImages[this.m_currentDisplayedAnnouncementImage].m_pointElementSequence);
                    if (element != null)
                    {
                        Transform transform = element.transform.FindChild("Image_Pointer");
                        if (transform != null)
                        {
                            transform.gameObject.CustomSetActive(enabled);
                        }
                    }
                }
            }
        }

        private void EnableAnnouncementPanel()
        {
            if (this.m_currentDisplayedAnnouncementImage < 0)
            {
                GameObject widget = this.m_versionUpdateFormScript.GetWidget(7);
                if (widget != null)
                {
                    widget.CustomSetActive(true);
                }
                GameObject obj3 = this.m_versionUpdateFormScript.GetWidget(4);
                if (obj3 != null)
                {
                    CUIContainerScript component = obj3.GetComponent<CUIContainerScript>();
                    if (component != null)
                    {
                        component.RecycleAllElement();
                        for (int i = 0; i < this.m_announcementImages.Count; i++)
                        {
                            this.m_announcementImages[i].m_pointElementSequence = component.GetElement();
                        }
                    }
                }
                GameObject obj4 = this.m_versionUpdateFormScript.GetWidget(5);
                if (obj4 != null)
                {
                    CUITimerScript script2 = obj4.GetComponent<CUITimerScript>();
                    if (script2 != null)
                    {
                        script2.StartTimer();
                    }
                }
                this.SwitchAnnouncementImage();
            }
        }

        private void FinishFirstExtractResource()
        {
            this.DisposeIIPSVersionMgr();
            this.m_versionUpdateState = enVersionUpdateState.StartCheckResourceVersion;
        }

        private void FinishUpdateApp()
        {
            this.DisposeIIPSVersionMgr();
            this.m_versionUpdateState = enVersionUpdateState.StartCheckFirstExtractResource;
        }

        private void FinishUpdateResource()
        {
            this.DisposeIIPSVersionMgr();
            this.UpdateUIProgress(enVersionUpdateFormWidget.Slider_SingleProgress, enVersionUpdateFormWidget.Text_SinglePercent, 1f);
            this.UpdateUIStateInfoTextContent(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_Complete"));
            this.m_versionUpdateState = enVersionUpdateState.Complete;
        }

        private string GetAndroidApkAbsPath()
        {
            return Android_GetApkAbsPath();
        }

        private string GetCheckAppVersionJsonConfig(enPlatform platform)
        {
            string str = string.Empty;
            if (platform == enPlatform.IOS)
            {
                object[] args = new object[] { this.GetIIPSServerUrl(), s_appID, s_serviceIDsForUpdataApp[(int) platform], s_appVersion, this.GetIIPSServerAmount() + 2 };
                return string.Format("{{\r\n                            \"m_update_type\" : 4,\r\n                            \"log_debug\" : false,\r\n\r\n                            \"basic_version\":\r\n                            {{\r\n                                \"m_server_url_list\" : [{0}],\r\n                                \"m_app_id\" : {1},\r\n                                \"m_service_id\" : {2},\r\n                                \"m_current_version_str\" : \"{3}\",\r\n                                \"m_retry_count\" : {4},\r\n                                \"m_retry_interval_ms\" : 1000,\r\n                                \"m_connect_timeout_ms\" : 1000,\r\n                                \"m_send_timeout_ms\" : 2000,\r\n                                \"m_recv_timeout_ms\" : 3000\r\n                            }}\r\n                    }}", args);
            }
            if (platform == enPlatform.Android)
            {
                object[] objArray2 = new object[] { this.GetIIPSServerUrl(), s_appID, s_serviceIDsForUpdataApp[(int) platform], s_appVersion, s_downloadedIFSSavePath, this.GetAndroidApkAbsPath(), this.GetIIPSServerAmount() + 2 };
                str = string.Format("{{\r\n                    \"basic_update\":\r\n                    {{\r\n                        \"m_ifs_save_path\" : \"{4}\",\r\n                        \"m_nextaction\" : \"basic_diffupdata\"\r\n                    }},\r\n\r\n                    \"basic_diffupdata\":             \r\n                    {{\r\n                        \"m_diff_config_save_path\" : \"{4}\",\r\n                        \"m_diff_temp_path\" : \"{4}\",\r\n                        \"m_nMaxDownloadSpeed\" : 102400000,\r\n                        \"m_apk_abspath\" : \"{5}\"\r\n                    }},\r\n\r\n                    \"m_update_type\" : 4,\r\n                    \"log_debug\" : false,\r\n\r\n                    \"basic_version\":\r\n                    {{\r\n                        \"m_server_url_list\" : [{0}],\r\n                        \"m_app_id\" : {1},\r\n                        \"m_service_id\" : {2},\r\n                        \"m_current_version_str\" : \"{3}\",\r\n                        \"m_retry_count\" : {6},\r\n                        \"m_retry_interval_ms\" : 1000,\r\n                        \"m_connect_timeout_ms\" : 1000,\r\n                        \"m_send_timeout_ms\" : 2000,\r\n                        \"m_recv_timeout_ms\" : 3000\r\n                    }}\r\n                }}", objArray2);
            }
            return str;
        }

        private string GetCheckResourceVersionJsonConfig(enPlatform platform)
        {
            object[] args = new object[] { this.GetIIPSServerUrl(), s_appID, s_serviceIDsForUpdateResource[(int) platform], this.m_cachedResourceVersion, s_downloadedIFSSavePath, s_ifsExtractPath, this.GetIIPSServerAmount() + 2, !this.m_useCurseResourceDownloadSize ? string.Empty : "\"need_down_size\" : true" };
            return string.Format("{{\r\n                        \"basic_update\":\r\n                        {{\r\n                            \"m_ifs_save_path\" : \"{4}\",\r\n                            \"m_nextaction\" : \"basic_diffupdata\"\r\n                        }},\r\n                \r\n                        \"full_diff\":\r\n\t\t\t\t        {{ \r\n\t\t\t\t\t        \"m_ifs_save_path\":\"{4}\",\r\n\t\t\t\t\t        \"m_file_extract_path\":\"{5}\"\r\n\t\t\t\t        }},\r\n                \r\n                        \"m_update_type\" : 5,\r\n                        \"log_debug\" : false,\r\n                        {7}\r\n\r\n                        \"basic_version\":\r\n                        {{\r\n                            \"m_server_url_list\" : [{0}],\r\n                            \"m_app_id\" : {1},\r\n                            \"m_service_id\" : {2},\r\n                            \"m_current_version_str\" : \"{3}\",\r\n                            \"m_retry_count\" : {6},\r\n\t\t                    \"m_retry_interval_ms\" : 1000,\r\n\t\t                    \"m_connect_timeout_ms\" : 1000,\r\n\t\t                    \"m_send_timeout_ms\" : 2000,\r\n\t\t                    \"m_recv_timeout_ms\" : 3000\r\n                        }}\r\n                    }}\r\n                ", args);
        }

        private string GetDownloadSpeed(int speed)
        {
            return string.Format("{0}/s", this.GetSizeString(speed));
        }

        private string GetDownloadTotalSize(int size)
        {
            return this.GetSizeString(size);
        }

        private string GetErrorResult(uint errorCode)
        {
            string key = "IIPS_Error_Result_Unknown";
            switch (this.m_iipsErrorCodeChecker.CheckIIPSErrorCode((int) errorCode).m_nErrorType)
            {
                case 1:
                    key = "IIPS_Error_Result_NetworkError";
                    break;

                case 2:
                    key = "IIPS_Error_Result_NetworkTimeout";
                    break;

                case 3:
                    key = "IIPS_Error_Result_DiskFull";
                    break;

                case 4:
                    key = "IIPS_Error_Result_OtherSystemError";
                    break;

                case 5:
                    key = "IIPS_Error_Result_OtherError";
                    break;

                case 6:
                    key = "IIPS_Error_Result_NoSupportUpdate";
                    break;

                case 7:
                    key = "IIPS_Error_Result_NotSure";
                    break;

                case 8:
                    key = "IIPS_Error_Result_NoSupportUpdate";
                    break;
            }
            return Singleton<CTextManager>.GetInstance().GetText(key);
        }

        private string GetFirstExtractResourceJsonConfig(enPlatform platform)
        {
            object[] args = new object[] { this.GetIIPSServerUrl(), s_appID, s_serviceIDsForUpdateResource[(int) platform], this.m_firstExtractResourceVersion, s_downloadedIFSSavePath, s_ifsExtractPath, s_firstExtractIFSName, s_firstExtractIFSPath, this.GetIIPSServerAmount() + 2 };
            return string.Format("{{\r\n                        \"basic_update\": \r\n                        {{ \r\n                            \"m_ifs_save_path\" : \"{4}\", \r\n                            \"m_nextaction\" : \"basic_diffupdata\" \r\n                        }}, \r\n                \r\n                        \"full_diff\":          \r\n\t\t\t\t        {{ \r\n\t\t\t\t\t        \"m_ifs_save_path\":\"{4}\", \r\n\t\t\t\t\t        \"m_file_extract_path\":\"{5}\" \r\n\t\t\t\t        }}, \r\n                \r\n                        \"m_update_type\" : 5,\r\n                        \"log_debug\" : false,\r\n\r\n                        \"basic_version\":\r\n                        {{\r\n                            \"m_server_url_list\" : [{0}],\r\n                            \"m_app_id\" : {1},\r\n                            \"m_service_id\" : {2},\r\n                            \"m_current_version_str\" : \"{3}\",\r\n                            \"m_retry_count\" : {8},\r\n\t\t                    \"m_retry_interval_ms\" : 1000,\r\n\t\t                    \"m_connect_timeout_ms\" : 1000,\r\n\t\t                    \"m_send_timeout_ms\" : 2000,\r\n\t\t                    \"m_recv_timeout_ms\" : 3000\r\n                        }},\r\n\r\n                        \"first_extract\":\r\n\t\t\t\t        {{\r\n\t\t\t\t\t        \"m_ifs_extract_path\":\"{5}\",\r\n\t\t\t\t\t        \"m_ifs_res_save_path\":\"{4}\",\r\n\t\t\t\t\t        \"filelist\":[\r\n\t\t\t\t\t\t        {{\r\n\t\t\t\t\t\t\t        \"filename\":\"{6}\",\r\n\t\t\t\t\t\t\t        \"filepath\":\"{7}\"\r\n\t\t\t\t\t\t        }}\r\n\t\t\t\t\t        ]\r\n\t\t\t\t        }}\r\n                    }}\r\n                ", args);
        }

        private int GetIIPSServerAmount()
        {
            string[] strArray = s_IIPSServerUrls[(int) s_IIPSServerType];
            return strArray.Length;
        }

        public static enIIPSServerType GetIIPSServerType()
        {
            return s_IIPSServerType;
        }

        private string GetIIPSServerUrl()
        {
            string[] strArray = s_IIPSServerUrls[(int) s_IIPSServerType];
            string str = string.Empty;
            for (int i = 0; i < strArray.Length; i++)
            {
                if (i != (strArray.Length - 1))
                {
                    str = str + string.Format("\"{0}\",", strArray[i]);
                }
                else
                {
                    str = str + string.Format("\"{0}\"", strArray[i]);
                }
            }
            return str;
        }

        private static string GetIIPSStreamingAssetsPath(string ifsName)
        {
            return string.Format("apk://{0}?assets/{1}", Android_GetApkAbsPath(), ifsName);
        }

        private void GetParamPair(string paramPairStr, out string param, out string value)
        {
            param = string.Empty;
            value = string.Empty;
            char[] separator = new char[] { ':' };
            string[] strArray = paramPairStr.Split(separator, 2);
            if ((strArray != null) && (strArray.Length == 2))
            {
                param = this.RemoveQuotationMark(strArray[0].Trim());
                value = this.RemoveQuotationMark(strArray[1].Trim());
            }
        }

        private string GetParamValueInContent(string titleContent, string param)
        {
            char[] separator = new char[] { ',' };
            string[] strArray = titleContent.Split(separator);
            for (int i = 0; i < strArray.Length; i++)
            {
                string str;
                string str2;
                this.GetParamPair(strArray[i], out str, out str2);
                if (string.Equals(param, str))
                {
                    return str2;
                }
            }
            return string.Empty;
        }

        private static enPlatform GetPlatform()
        {
            return enPlatform.Android;
        }

        private string GetSizeString(int size)
        {
            if (size >= 0x100000)
            {
                float num = ((float) size) / 1048576f;
                return string.Format("{0}MB", Mathf.RoundToInt(num));
            }
            float f = ((float) size) / 1024f;
            return string.Format("{0}KB", Mathf.RoundToInt(f));
        }

        private string GetTitleContentInMsg(string msg, string title)
        {
            int index = msg.IndexOf(title);
            if (index >= 0)
            {
                int num2 = msg.IndexOf("{", index);
                int num3 = msg.IndexOf("}", index);
                if ((num2 > 0) && (num3 > 0))
                {
                    return msg.Substring(num2 + 1, ((num3 - num2) + 1) - 2);
                }
            }
            return string.Empty;
        }

        private T GetUIComponent<T>(CUIFormScript formScript, enVersionUpdateFormWidget widget) where T: MonoBehaviour
        {
            if (formScript == null)
            {
                return null;
            }
            GameObject obj2 = this.m_versionUpdateFormScript.GetWidget((int) widget);
            if (obj2 == null)
            {
                return null;
            }
            return obj2.GetComponent<T>();
        }

        private static bool I2B(int value)
        {
            return (value > 0);
        }

        protected override void Init()
        {
            s_downloadedIFSSavePath = CFileManager.GetCachePath();
            s_ifsExtractPath = CFileManager.GetIFSExtractPath();
            s_firstExtractIFSName = CFileManager.EraseExtension(CResourcePackerInfoSet.s_resourceIFSFileName) + ".png";
            s_firstExtractIFSPath = null;
            s_resourcePackerInfoSetFullPath = CFileManager.CombinePath(s_ifsExtractPath, CResourcePackerInfoSet.s_resourcePackerInfoSetFileName);
            s_appVersion = GameFramework.AppVersion;
            s_cachedResourceVersionFilePath = CFileManager.CombinePath(CFileManager.GetCachePath(), "ifs.bytes");
            s_appType = enAppType.General;
            this.m_versionUpdateState = enVersionUpdateState.None;
            this.m_cachedResourceVersion = CVersion.s_emptyResourceVersion;
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VersionUpdate_JumpToHomePage, new CUIEventManager.OnUIEventHandler(this.OnJumpToHomePage));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VersionUpdate_RetryCheckAppVersion, new CUIEventManager.OnUIEventHandler(this.OnRetryCheckApp));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VersionUpdate_ConfirmUpdateApp, new CUIEventManager.OnUIEventHandler(this.OnConfirmUpdateApp));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VersionUpdate_ConfirmUpdateAppNoWifi, new CUIEventManager.OnUIEventHandler(this.OnConfirmUpdateAppNoWifi));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VersionUpdate_CancelUpdateApp, new CUIEventManager.OnUIEventHandler(this.OnCancelUpdateApp));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VersionUpdate_QuitApp, new CUIEventManager.OnUIEventHandler(this.OnQuitApp));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VersionUpdate_RetryCheckFirstExtractResource, new CUIEventManager.OnUIEventHandler(this.OnRetryCheckFirstExtractResource));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VersionUpdate_RetryCheckResourceVersion, new CUIEventManager.OnUIEventHandler(this.OnRetryCheckResourceVersion));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VersionUpdate_ConfirmUpdateResource, new CUIEventManager.OnUIEventHandler(this.OnConfirmUpdateResource));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VersionUpdate_ConfirmUpdateResourceNoWifi, new CUIEventManager.OnUIEventHandler(this.OnConfirmUpdateResourceNoWifi));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.VersionUpdate_SwitchAnnouncementImage, new CUIEventManager.OnUIEventHandler(this.OnSwitchAnnouncementImage));
            s_androidUtilityJavaClass = new AndroidJavaClass(ApolloConfig.GetGameUtilityString());
            s_firstExtractIFSPath = GetIIPSStreamingAssetsPath(s_firstExtractIFSName);
            CLoginSystem.s_splashFormPath = s_splashFormPathFuckLOL1;
            if (PlayerPrefs.GetInt("SplashHack", 0) == 1)
            {
                CLoginSystem.s_splashFormPath = s_splashFormPathFuckLOL0;
            }
        }

        private bool IsFileExistInStreamingAssets(string fileName)
        {
            return Android_IsFileExistInStreamingAssets(fileName);
        }

        private bool IsInFirstExtractResourceStage()
        {
            return ((this.m_versionUpdateState >= enVersionUpdateState.StartCheckFirstExtractResource) && (this.m_versionUpdateState <= enVersionUpdateState.FinishFirstExtractResouce));
        }

        private bool IsInUpdateAppStage()
        {
            return ((this.m_versionUpdateState >= enVersionUpdateState.StartCheckAppVersion) && (this.m_versionUpdateState <= enVersionUpdateState.FinishUpdateApp));
        }

        private bool IsInUpdateResourceStage()
        {
            return ((this.m_versionUpdateState >= enVersionUpdateState.StartCheckResourceVersion) && (this.m_versionUpdateState <= enVersionUpdateState.FinishUpdateResource));
        }

        private bool IsUseWifi()
        {
            return (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork);
        }

        public byte OnActionMsg(string msg)
        {
            string titleContentInMsg = this.GetTitleContentInMsg(msg, "@App");
            if (!string.IsNullOrEmpty(titleContentInMsg))
            {
                this.m_appDownloadUrl = this.GetParamValueInContent(titleContentInMsg, "url");
                string paramValueInContent = this.GetParamValueInContent(titleContentInMsg, "size");
                string str3 = this.GetParamValueInContent(titleContentInMsg, "minversion");
                string str4 = this.GetParamValueInContent(titleContentInMsg, "maxversion");
                if (!string.IsNullOrEmpty(paramValueInContent))
                {
                    try
                    {
                        this.m_appDownloadSize = uint.Parse(paramValueInContent);
                    }
                    catch (Exception)
                    {
                        this.m_appDownloadSize = 0;
                    }
                }
                if (!string.IsNullOrEmpty(str3))
                {
                    this.m_recommendUpdateVersionMin = CVersion.GetVersionNumber(str3);
                }
                else
                {
                    this.m_recommendUpdateVersionMin = 0;
                }
                if (!string.IsNullOrEmpty(str4))
                {
                    this.m_recommendUpdateVersionMax = CVersion.GetVersionNumber(str4);
                }
                else
                {
                    this.m_recommendUpdateVersionMax = 0;
                }
            }
            string str5 = this.GetTitleContentInMsg(msg, "@Announcement");
            if (!string.IsNullOrEmpty(str5))
            {
                string str6 = this.GetParamValueInContent(str5, "url");
                string str7 = this.GetParamValueInContent(str5, "amount");
                string str8 = this.GetParamValueInContent(str5, "needconfirm");
                string str9 = this.GetParamValueInContent(str5, "size");
                this.m_resourceDownloadNeedConfirm = false;
                if (!string.IsNullOrEmpty(str8))
                {
                    this.m_resourceDownloadNeedConfirm = string.Equals(str8, "1");
                }
                if (!this.m_useCurseResourceDownloadSize && !string.IsNullOrEmpty(str9))
                {
                    try
                    {
                        this.m_resourceDownloadSize = uint.Parse(str9);
                    }
                    catch (Exception)
                    {
                        this.m_resourceDownloadSize = 0;
                    }
                }
                int num = 0;
                if (!string.IsNullOrEmpty(str7))
                {
                    try
                    {
                        num = int.Parse(str7);
                    }
                    catch (Exception)
                    {
                        num = 0;
                    }
                }
                this.m_currentDisplayedAnnouncementImage = -1;
                this.m_announcementImages.Clear();
                for (int i = 0; i < num; i++)
                {
                    CAnnouncementImage item = new CAnnouncementImage();
                    item.m_url = str6.Replace("%ID%", (i + 1).ToString());
                    item.m_texture2D = null;
                    item.m_pointElementSequence = -1;
                    this.m_announcementImages.Add(item);
                }
            }
            return 1;
        }

        private void OnCancelUpdateApp(CUIEvent uiEvent)
        {
            this.m_versionUpdateState = enVersionUpdateState.FinishUpdateApp;
        }

        private void OnConfirmUpdateApp(CUIEvent uiEvent)
        {
            if (this.IsUseWifi() || (s_platform == enPlatform.IOS))
            {
                this.StartDownloadApp();
            }
            else
            {
                Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(string.Format(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_NoWifiConfirm"), this.GetSizeString((int) this.m_appDownloadSize)), enUIEventID.VersionUpdate_ConfirmUpdateAppNoWifi, enUIEventID.VersionUpdate_QuitApp, Singleton<CTextManager>.GetInstance().GetText("Common_Confirm"), Singleton<CTextManager>.GetInstance().GetText("Common_Cancel"), false);
            }
        }

        private void OnConfirmUpdateAppNoWifi(CUIEvent uiEvent)
        {
            this.StartDownloadApp();
        }

        private void OnConfirmUpdateResource(CUIEvent uiEvent)
        {
            if (this.IsUseWifi())
            {
                base.StartCoroutine(this.DownloadAnnouncementImage());
                this.StartDownloadResource();
            }
            else
            {
                Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(string.Format(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_NoWifiConfirm"), this.GetSizeString((int) this.m_resourceDownloadSize)), enUIEventID.VersionUpdate_ConfirmUpdateResourceNoWifi, enUIEventID.VersionUpdate_QuitApp, Singleton<CTextManager>.GetInstance().GetText("Common_Confirm"), Singleton<CTextManager>.GetInstance().GetText("Common_Cancel"), false);
            }
        }

        private void OnConfirmUpdateResourceNoWifi(CUIEvent uiEvent)
        {
            base.StartCoroutine(this.DownloadAnnouncementImage());
            this.StartDownloadResource();
        }

        public void OnError(IIPSMobileVersionCallBack.VERSIONSTAGE curVersionStage, uint errorCode)
        {
            this.m_isError = true;
            Singleton<BeaconHelper>.GetInstance().Event_CommonReport("Event_VerUpdateFail");
            if (this.IsInUpdateAppStage())
            {
                enUIEventID confirmID = enUIEventID.VersionUpdate_RetryCheckAppVersion;
                IIPSMobile.IIPSMobileErrorCodeCheck.ErrorCodeInfo info = this.m_iipsErrorCodeChecker.CheckIIPSErrorCode((int) errorCode);
                if ((errorCode == 6) || (errorCode == 8))
                {
                    confirmID = enUIEventID.VersionUpdate_JumpToHomePage;
                }
                Singleton<CUIManager>.GetInstance().OpenMessageBox(string.Format(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_AppUpdateFail"), errorCode.ToString(), this.GetErrorResult(errorCode)), confirmID, false);
            }
            else if (this.IsInFirstExtractResourceStage())
            {
                Singleton<CUIManager>.GetInstance().OpenMessageBox(string.Format(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_FirstExtractFail"), errorCode.ToString(), this.GetErrorResult(errorCode)), enUIEventID.VersionUpdate_RetryCheckFirstExtractResource, false);
            }
            else if (this.IsInUpdateResourceStage())
            {
                this.m_apolloUpdateSpeedCounter.StopSpeedCounter();
                this.m_downloadCounter = 0;
                this.m_downloadSpeed = 0;
                Singleton<CUIManager>.GetInstance().OpenMessageBox(string.Format(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_ResourceUpdateFail"), errorCode.ToString(), this.GetErrorResult(errorCode)), enUIEventID.VersionUpdate_RetryCheckResourceVersion, false);
            }
        }

        public byte OnGetNewVersionInfo(IIPSMobileVersionCallBack.VERSIONINFO newVersionInfo)
        {
            if (this.IsInUpdateAppStage())
            {
                if (I2B(newVersionInfo.isAppUpdating))
                {
                    object[] args = new object[] { newVersionInfo.newAppVersion.programmeVersion.MajorVersion_Number, newVersionInfo.newAppVersion.programmeVersion.MinorVersion_Number, newVersionInfo.newAppVersion.programmeVersion.Revision_Number, newVersionInfo.newAppVersion.dataVersion.DataVersion };
                    this.m_downloadAppVersion = string.Format("{0}.{1}.{2}.{3}", args);
                    if (I2B(newVersionInfo.isForcedUpdating))
                    {
                        Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_ForceUpdateClient"), enUIEventID.VersionUpdate_ConfirmUpdateApp, enUIEventID.VersionUpdate_QuitApp, Singleton<CTextManager>.GetInstance().GetText("Common_Confirm"), Singleton<CTextManager>.GetInstance().GetText("Common_Cancel"), false);
                    }
                    else if ((this.m_recommendUpdateVersionMin > 0) && (CVersion.GetVersionNumber(s_appVersion) < this.m_recommendUpdateVersionMin))
                    {
                        Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_ForceUpdateClient"), enUIEventID.VersionUpdate_ConfirmUpdateApp, enUIEventID.VersionUpdate_QuitApp, Singleton<CTextManager>.GetInstance().GetText("Common_Confirm"), Singleton<CTextManager>.GetInstance().GetText("Common_Cancel"), false);
                    }
                    else if ((this.m_recommendUpdateVersionMax > 0) && (CVersion.GetVersionNumber(s_appVersion) > this.m_recommendUpdateVersionMax))
                    {
                        this.m_versionUpdateState = enVersionUpdateState.FinishUpdateApp;
                    }
                    else
                    {
                        Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_RecommendUpdateClient"), enUIEventID.VersionUpdate_ConfirmUpdateApp, enUIEventID.VersionUpdate_CancelUpdateApp, false);
                    }
                }
                else
                {
                    if (s_platform == enPlatform.Android)
                    {
                        string[] fileExtensionFilter = new string[] { ".apk" };
                        CFileManager.ClearDirectory(CFileManager.GetCachePath(), fileExtensionFilter, null);
                    }
                    this.m_versionUpdateState = enVersionUpdateState.FinishUpdateApp;
                }
            }
            else if (this.IsInFirstExtractResourceStage())
            {
                this.StartFirstExtractResource();
            }
            else if (this.IsInUpdateResourceStage())
            {
                object[] objArray2 = new object[] { newVersionInfo.newAppVersion.programmeVersion.MajorVersion_Number, newVersionInfo.newAppVersion.programmeVersion.MinorVersion_Number, newVersionInfo.newAppVersion.programmeVersion.Revision_Number, newVersionInfo.newAppVersion.dataVersion.DataVersion };
                this.m_downloadResourceVersion = string.Format("{0}.{1}.{2}.{3}", objArray2);
                if (this.m_useCurseResourceDownloadSize)
                {
                    this.m_resourceDownloadSize = (uint) newVersionInfo.needDownloadSize;
                }
                if (I2B(newVersionInfo.isAppUpdating))
                {
                    if (this.m_resourceDownloadNeedConfirm)
                    {
                        Singleton<CUIManager>.GetInstance().OpenMessageBoxWithCancel(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_ForceUpdateResource"), enUIEventID.VersionUpdate_ConfirmUpdateResource, enUIEventID.VersionUpdate_QuitApp, Singleton<CTextManager>.GetInstance().GetText("Common_Confirm"), Singleton<CTextManager>.GetInstance().GetText("Common_Cancel"), false);
                    }
                    else
                    {
                        base.StartCoroutine(this.DownloadAnnouncementImage());
                        this.StartDownloadResource();
                    }
                }
                else
                {
                    this.m_versionUpdateState = enVersionUpdateState.FinishUpdateResource;
                }
            }
            return 1;
        }

        private void OnJumpToHomePage(CUIEvent uiEvent)
        {
            CUICommonSystem.OpenUrl("http://pvp.qq.com", false);
            this.m_versionUpdateState = enVersionUpdateState.StartCheckAppVersion;
        }

        public byte OnNoticeInstallApk(string path)
        {
            this.UpdateUIProgress(enVersionUpdateFormWidget.Slider_SingleProgress, enVersionUpdateFormWidget.Text_SinglePercent, 1f);
            this.Android_InstallAPK(path);
            this.m_versionUpdateState = enVersionUpdateState.StartCheckAppVersion;
            return 1;
        }

        public void OnProgress(IIPSMobileVersionCallBack.VERSIONSTAGE curVersionStage, ulong totalSize, ulong nowSize)
        {
            this.UpdateUIProgress(enVersionUpdateFormWidget.Slider_SingleProgress, enVersionUpdateFormWidget.Text_SinglePercent, ((float) nowSize) / ((totalSize != 0) ? ((float) totalSize) : ((float) nowSize)));
            switch (curVersionStage)
            {
                case IIPSMobileVersionCallBack.VERSIONSTAGE.VS_ExtractData:
                case IIPSMobileVersionCallBack.VERSIONSTAGE.VS_FirstExtract:
                case IIPSMobileVersionCallBack.VERSIONSTAGE.VS_FullUpdate_Extract:
                case IIPSMobileVersionCallBack.VERSIONSTAGE.VS_SourceExtract:
                    this.UpdateUIStateInfoTextContent(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_ExtractResource"));
                    return;

                case IIPSMobileVersionCallBack.VERSIONSTAGE.VS_FullUpdate:
                case IIPSMobileVersionCallBack.VERSIONSTAGE.VS_SourceDownload:
                    this.UpdateUIStateInfoTextContent(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_DownloadResource"));
                    if (curVersionStage == IIPSMobileVersionCallBack.VERSIONSTAGE.VS_SourceDownload)
                    {
                        if (this.m_downloadCounter == 0)
                        {
                            this.m_apolloUpdateSpeedCounter.StartSpeedCounter();
                        }
                        this.m_apolloUpdateSpeedCounter.SetSize((uint) nowSize);
                        this.m_apolloUpdateSpeedCounter.SpeedCounter();
                        this.m_downloadSpeed = this.m_apolloUpdateSpeedCounter.GetSpeed();
                        this.m_downloadCounter++;
                    }
                    else
                    {
                        this.m_downloadSpeed = this.m_IIPSVersionMgr.MgrGetActionDownloadSpeed();
                    }
                    this.UpdateUIDownloadProgressTextContent(string.Format(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_DownloadResourceProgress"), this.GetDownloadTotalSize((int) totalSize), this.GetDownloadSpeed((int) this.m_downloadSpeed), !string.IsNullOrEmpty(this.m_downloadResourceVersion) ? string.Format("(v{0})", this.m_downloadResourceVersion) : string.Empty));
                    return;

                case IIPSMobileVersionCallBack.VERSIONSTAGE.VS_CreateApk:
                    this.UpdateUIStateInfoTextContent(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_DownloadApp"));
                    this.UpdateUIDownloadProgressTextContent(string.Format(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_DownloadResourceProgress"), this.GetDownloadTotalSize((int) totalSize), this.GetDownloadSpeed((int) this.m_IIPSVersionMgr.MgrGetActionDownloadSpeed()), string.Format("(v{0})", this.m_downloadAppVersion)));
                    return;

                case IIPSMobileVersionCallBack.VERSIONSTAGE.VS_CheckApkMd5:
                    this.UpdateUIStateInfoTextContent(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_PrepareInstall"));
                    this.UpdateUIDownloadProgressTextContent(string.Empty);
                    return;
            }
        }

        private void OnQuitApp(CUIEvent uiEvent)
        {
            QuitApp();
        }

        private void OnRetryCheckApp(CUIEvent uiEvent)
        {
            this.m_versionUpdateState = enVersionUpdateState.StartCheckAppVersion;
        }

        private void OnRetryCheckFirstExtractResource(CUIEvent uiEvent)
        {
            this.m_versionUpdateState = enVersionUpdateState.StartCheckFirstExtractResource;
        }

        private void OnRetryCheckResourceVersion(CUIEvent uiEvent)
        {
            this.m_versionUpdateState = enVersionUpdateState.StartCheckResourceVersion;
        }

        public void OnSuccess()
        {
            if (!this.m_isError)
            {
                if (this.IsInUpdateAppStage())
                {
                    this.m_versionUpdateState = enVersionUpdateState.StartCheckAppVersion;
                }
                else if (this.IsInFirstExtractResourceStage())
                {
                    this.RecordResourceVersion();
                    this.m_versionUpdateState = enVersionUpdateState.FinishFirstExtractResouce;
                }
                else if (this.IsInUpdateResourceStage())
                {
                    this.m_apolloUpdateSpeedCounter.StopSpeedCounter();
                    this.m_downloadCounter = 0;
                    this.m_downloadSpeed = 0;
                    this.RecordResourceVersion();
                    this.m_versionUpdateState = enVersionUpdateState.FinishUpdateResource;
                }
                this.UpdateUIProgress(enVersionUpdateFormWidget.Slider_SingleProgress, enVersionUpdateFormWidget.Text_SinglePercent, 1f);
            }
        }

        private void OnSwitchAnnouncementImage(CUIEvent uiEvent)
        {
            this.SwitchAnnouncementImage();
        }

        private void OpenWaitingForm()
        {
            CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(s_waitingFormPath, false, false);
            if (script != null)
            {
                script.transform.Find("Panel/Panel").gameObject.CustomSetActive(false);
                script.transform.Find("Panel/Image").gameObject.CustomSetActive(true);
            }
        }

        public static void QuitApp()
        {
            SGameApplication.Quit();
            Android_ExitApp();
        }

        private void RecordResourceVersion()
        {
            string str = CVersion.s_emptyResourceVersion;
            byte[] data = null;
            int offset = 0;
            if (CFileManager.IsFileExist(s_resourcePackerInfoSetFullPath))
            {
                data = CFileManager.ReadFile(s_resourcePackerInfoSetFullPath);
                offset = 0;
                if ((data != null) && (data.Length > 2))
                {
                    str = CMemoryManager.ReadString(data, ref offset);
                }
            }
            data = new byte[0x400];
            offset = 0;
            CMemoryManager.WriteString(str, data, ref offset);
            CMemoryManager.WriteByte((byte) s_appType, data, ref offset);
            CMemoryManager.WriteString(CVersion.GetBuildNumber(), data, ref offset);
            CFileManager.WriteFile(s_cachedResourceVersionFilePath, data, 0, offset);
            this.m_cachedResourceVersion = str;
            this.UpdateUIVersionTextContent(s_appVersion, this.m_cachedResourceVersion);
        }

        private string RemoveQuotationMark(string str)
        {
            int index = str.IndexOf('"');
            int num2 = str.LastIndexOf('"');
            if (((index >= 0) && (num2 >= 0)) && (index != num2))
            {
                char[] trimChars = new char[] { '\\' };
                return str.Substring(index + 1, (num2 - index) - 1).Trim(trimChars).Trim();
            }
            return str.Trim();
        }

        public void Repair()
        {
        }

        public static void SetIIPSServerType(enIIPSServerType iipsServerType)
        {
            s_IIPSServerType = iipsServerType;
        }

        public static void SetIIPSServerTypeFromFile()
        {
            TdirConfigData fileTdirAndTverData = TdirConfig.GetFileTdirAndTverData();
            if (fileTdirAndTverData != null)
            {
                s_IIPSServerType = (enIIPSServerType) fileTdirAndTverData.versionType;
            }
        }

        private void StartCheckAppVersion()
        {
            this.UpdateUIStateInfoTextContent(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_CheckAppVersion"));
            this.CreateIIPSVersionMgr(this.GetCheckAppVersionJsonConfig(s_platform));
            if ((this.m_IIPSVersionMgr == null) || !this.m_IIPSVersionMgr.MgrCheckAppUpdate())
            {
                Singleton<CUIManager>.GetInstance().OpenMessageBox(string.Format(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_AppUpdateFail"), 0, this.GetErrorResult(0)), enUIEventID.VersionUpdate_RetryCheckAppVersion, false);
            }
            this.m_versionUpdateState = enVersionUpdateState.CheckAppVersion;
            this.m_isError = false;
        }

        [DebuggerHidden]
        private IEnumerator StartCheckFirstExtractResource()
        {
            return new <StartCheckFirstExtractResource>c__Iterator1E { <>f__this = this };
        }

        private void StartCheckPathPermission()
        {
            if (string.IsNullOrEmpty(CFileManager.GetCachePath()))
            {
                Singleton<CUIManager>.GetInstance().OpenMessageBox(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_PermissionFail"), enUIEventID.VersionUpdate_QuitApp, false);
                this.m_versionUpdateState = enVersionUpdateState.CheckPathPermission;
            }
            else
            {
                this.m_versionUpdateState = enVersionUpdateState.StartCheckAppVersion;
            }
        }

        private void StartCheckResourceVersion()
        {
            this.UpdateUIStateInfoTextContent(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_CheckResourceVersion"));
            this.CreateIIPSVersionMgr(this.GetCheckResourceVersionJsonConfig(s_platform));
            if ((this.m_IIPSVersionMgr == null) || !this.m_IIPSVersionMgr.MgrCheckAppUpdate())
            {
                Singleton<CUIManager>.GetInstance().OpenMessageBox(string.Format(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_ResourceUpdateFail"), 0, this.GetErrorResult(0)), enUIEventID.VersionUpdate_RetryCheckResourceVersion, false);
            }
            this.m_versionUpdateState = enVersionUpdateState.CheckResourceVersion;
            this.m_isError = false;
        }

        private void StartDownloadApp()
        {
            if (s_platform == enPlatform.Android)
            {
                if (this.m_IIPSVersionMgr != null)
                {
                    this.m_IIPSVersionMgr.MgrSetNextStage(true);
                }
                this.m_versionUpdateState = enVersionUpdateState.DownloadApp;
            }
            else
            {
                CUICommonSystem.OpenUrl(this.m_appDownloadUrl, false);
                this.m_versionUpdateState = enVersionUpdateState.StartCheckAppVersion;
            }
        }

        private void StartDownloadResource()
        {
            this.UpdateUIStateInfoTextContent(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_PrepareDownloadResource"));
            if (this.m_IIPSVersionMgr != null)
            {
                this.m_IIPSVersionMgr.MgrSetNextStage(true);
            }
            this.m_apolloUpdateSpeedCounter.StopSpeedCounter();
            this.m_downloadCounter = 0;
            this.m_downloadSpeed = 0;
            this.m_versionUpdateState = enVersionUpdateState.DownloadResource;
        }

        private void StartFirstExtractResource()
        {
            if (this.m_IIPSVersionMgr != null)
            {
                this.m_IIPSVersionMgr.MgrSetNextStage(true);
            }
            this.m_versionUpdateState = enVersionUpdateState.FirstExtractResource;
        }

        public void StartVersionUpdate(OnVersionUpdateComplete onVersionUpdateComplete)
        {
            this.m_fBeginUpdateTime = DateTime.Now;
            this.m_onVersionUpdateComplete = onVersionUpdateComplete;
            if (s_IIPSServerType == enIIPSServerType.None)
            {
                this.m_cachedResourceVersion = CVersion.s_emptyResourceVersion;
                Singleton<CResourceManager>.GetInstance().ClearCachePath();
                this.m_versionUpdateState = enVersionUpdateState.Complete;
            }
            else
            {
                if (CFileManager.IsFileExist(s_cachedResourceVersionFilePath))
                {
                    try
                    {
                        byte[] data = CFileManager.ReadFile(s_cachedResourceVersionFilePath);
                        int offset = 0;
                        if ((data == null) || (data.Length <= 2))
                        {
                            this.m_cachedResourceVersion = CVersion.s_emptyResourceVersion;
                        }
                        else
                        {
                            this.m_cachedResourceVersion = CMemoryManager.ReadString(data, ref offset);
                            enAppType unknown = enAppType.Unknown;
                            if (offset < data.Length)
                            {
                                unknown = (enAppType) CMemoryManager.ReadByte(data, ref offset);
                            }
                            string b = string.Empty;
                            if (offset < data.Length)
                            {
                                try
                                {
                                    b = CMemoryManager.ReadString(data, ref offset);
                                }
                                catch (Exception)
                                {
                                    b = string.Empty;
                                }
                            }
                            if (!string.Equals(CVersion.GetBuildNumber(), b) || (s_appType != unknown))
                            {
                                this.m_cachedResourceVersion = CVersion.s_emptyResourceVersion;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        this.m_cachedResourceVersion = CVersion.s_emptyResourceVersion;
                    }
                }
                else
                {
                    this.m_cachedResourceVersion = CVersion.s_emptyResourceVersion;
                }
                if (string.Equals(this.m_cachedResourceVersion, CVersion.s_emptyResourceVersion))
                {
                    Singleton<CResourceManager>.GetInstance().ClearCachePath();
                }
                Singleton<CUIManager>.GetInstance().OpenForm(CLoginSystem.s_splashFormPath, false, true);
                this.m_versionUpdateFormScript = Singleton<CUIManager>.GetInstance().OpenForm(s_versionUpdateFormPath, false, true);
                this.UpdateUIVersionTextContent(s_appVersion, this.m_cachedResourceVersion);
                this.UpdateUIStateInfoTextContent(string.Empty);
                this.UpdateUIDownloadProgressTextContent(string.Empty);
                this.m_versionUpdateState = enVersionUpdateState.StartCheckPathPermission;
            }
        }

        private void SwitchAnnouncementImage()
        {
            int currentDisplayedAnnouncementImage = this.m_currentDisplayedAnnouncementImage;
            if (currentDisplayedAnnouncementImage < 0)
            {
                currentDisplayedAnnouncementImage = 0;
            }
            else
            {
                currentDisplayedAnnouncementImage++;
            }
            currentDisplayedAnnouncementImage = currentDisplayedAnnouncementImage % this.m_announcementImages.Count;
            for (int i = 0; i < this.m_announcementImages.Count; i++)
            {
                if (this.m_announcementImages[currentDisplayedAnnouncementImage].m_texture2D != null)
                {
                    this.DisplayAnnouncementImage(currentDisplayedAnnouncementImage);
                    return;
                }
                currentDisplayedAnnouncementImage++;
                currentDisplayedAnnouncementImage = currentDisplayedAnnouncementImage % this.m_announcementImages.Count;
            }
        }

        public void Update()
        {
            switch (this.m_versionUpdateState)
            {
                case enVersionUpdateState.StartCheckPathPermission:
                    this.StartCheckPathPermission();
                    break;

                case enVersionUpdateState.StartCheckAppVersion:
                    this.StartCheckAppVersion();
                    break;

                case enVersionUpdateState.FinishUpdateApp:
                    this.FinishUpdateApp();
                    break;

                case enVersionUpdateState.StartCheckFirstExtractResource:
                    base.StartCoroutine(this.StartCheckFirstExtractResource());
                    break;

                case enVersionUpdateState.FinishFirstExtractResouce:
                    this.FinishFirstExtractResource();
                    break;

                case enVersionUpdateState.StartCheckResourceVersion:
                    this.StartCheckResourceVersion();
                    break;

                case enVersionUpdateState.FinishUpdateResource:
                    this.FinishUpdateResource();
                    break;

                case enVersionUpdateState.Complete:
                    this.Complete();
                    break;
            }
            this.UpdateIIPSVersionMgr();
            this.UpdateUIProgress(enVersionUpdateFormWidget.Slider_TotalProgress, enVersionUpdateFormWidget.Text_TotalPercent, ((float) this.m_versionUpdateState) / 15f);
        }

        private void UpdateIIPSVersionMgr()
        {
            if (this.m_IIPSVersionMgr != null)
            {
                this.m_IIPSVersionMgr.MgrPoll();
            }
        }

        private void UpdateUIDownloadProgressTextContent(string content)
        {
            Text uIComponent = this.GetUIComponent<Text>(this.m_versionUpdateFormScript, enVersionUpdateFormWidget.Text_UpdateInfo);
            if (uIComponent != null)
            {
                uIComponent.text = content;
            }
        }

        private void UpdateUIProgress(enVersionUpdateFormWidget progressBarWidget, enVersionUpdateFormWidget progressPercentTextWidget, float progress)
        {
            if (progress > 1f)
            {
                progress = 1f;
            }
            Slider uIComponent = this.GetUIComponent<Slider>(this.m_versionUpdateFormScript, progressBarWidget);
            if (uIComponent != null)
            {
                uIComponent.value = progress;
            }
            Text text = this.GetUIComponent<Text>(this.m_versionUpdateFormScript, progressPercentTextWidget);
            if (text != null)
            {
                text.text = string.Format("{0}%", (int) (progress * 100f));
            }
        }

        private void UpdateUIStateInfoTextContent(string content)
        {
            Text uIComponent = this.GetUIComponent<Text>(this.m_versionUpdateFormScript, enVersionUpdateFormWidget.Text_CurrentState);
            if (uIComponent != null)
            {
                uIComponent.text = content;
            }
        }

        private void UpdateUITextContent(enVersionUpdateFormWidget textWidget, string content)
        {
            Text uIComponent = this.GetUIComponent<Text>(this.m_versionUpdateFormScript, textWidget);
            if (uIComponent != null)
            {
                uIComponent.text = content;
            }
        }

        private void UpdateUIVersionTextContent(string appVersion, string resourceVersion)
        {
            this.UpdateUITextContent(enVersionUpdateFormWidget.Text_Version, string.Format("App v{0}   Res v{1}", appVersion, resourceVersion));
        }

        [DebuggerHidden]
        private IEnumerator VersionUpdateComplete()
        {
            return new <VersionUpdateComplete>c__Iterator1F { <>f__this = this };
        }

        public string CachedResourceVersion
        {
            get
            {
                return this.m_cachedResourceVersion;
            }
        }

        [CompilerGenerated]
        private sealed class <DownloadAnnouncementImage>c__Iterator20 : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal CVersionUpdateSystem <>f__this;
            internal int <i>__0;
            internal WWW <www>__1;

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
                        this.<i>__0 = 0;
                        break;

                    case 1:
                        if (string.IsNullOrEmpty(this.<www>__1.error))
                        {
                            this.<>f__this.m_announcementImages[this.<i>__0].m_texture2D = this.<www>__1.texture;
                            this.<>f__this.EnableAnnouncementPanel();
                        }
                        this.<i>__0++;
                        break;

                    default:
                        goto Label_00E1;
                }
                if (this.<i>__0 < this.<>f__this.m_announcementImages.Count)
                {
                    this.<www>__1 = new WWW(this.<>f__this.m_announcementImages[this.<i>__0].m_url);
                    this.$current = this.<www>__1;
                    this.$PC = 1;
                    return true;
                }
                this.$PC = -1;
            Label_00E1:
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
        private sealed class <StartCheckFirstExtractResource>c__Iterator1E : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal CVersionUpdateSystem <>f__this;
            internal int <offset>__1;
            internal WWW <www>__0;

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
                        this.<>f__this.UpdateUIStateInfoTextContent(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_CheckFirstExtractResource"));
                        this.<>f__this.m_versionUpdateState = enVersionUpdateState.CheckFirstExtractResource;
                        if (this.<>f__this.IsFileExistInStreamingAssets(CVersionUpdateSystem.s_firstExtractIFSName) && this.<>f__this.IsFileExistInStreamingAssets(CResourcePackerInfoSet.s_resourcePackerInfoSetFileName))
                        {
                            this.<www>__0 = new WWW(CFileManager.GetStreamingAssetsPathWithHeader(CResourcePackerInfoSet.s_resourcePackerInfoSetFileName));
                            this.$current = this.<www>__0;
                            this.$PC = 1;
                            return true;
                        }
                        this.<>f__this.m_versionUpdateState = enVersionUpdateState.FinishFirstExtractResouce;
                        break;

                    case 1:
                        this.<offset>__1 = 0;
                        this.<>f__this.m_firstExtractResourceVersion = CMemoryManager.ReadString(this.<www>__0.bytes, ref this.<offset>__1);
                        if (CVersion.GetVersionNumber(this.<>f__this.m_cachedResourceVersion) < CVersion.GetVersionNumber(this.<>f__this.m_firstExtractResourceVersion))
                        {
                            Singleton<CResourceManager>.GetInstance().ClearCachePath();
                            this.<>f__this.CreateIIPSVersionMgr(this.<>f__this.GetFirstExtractResourceJsonConfig(CVersionUpdateSystem.s_platform));
                            if ((this.<>f__this.m_IIPSVersionMgr == null) || !this.<>f__this.m_IIPSVersionMgr.MgrCheckAppUpdate())
                            {
                                Singleton<CUIManager>.GetInstance().OpenMessageBox(string.Format(Singleton<CTextManager>.GetInstance().GetText("VersionUpdate_FirstExtractFail"), 0, this.<>f__this.GetErrorResult(0)), enUIEventID.VersionUpdate_RetryCheckFirstExtractResource, false);
                            }
                            this.<>f__this.m_isError = false;
                            this.$PC = -1;
                            break;
                        }
                        this.<>f__this.m_versionUpdateState = enVersionUpdateState.FinishFirstExtractResouce;
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
        private sealed class <VersionUpdateComplete>c__Iterator1F : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal CVersionUpdateSystem <>f__this;
            internal string <content>__2;
            internal CBinaryObject <tAsstet>__1;
            internal CResource <textResource>__0;

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
                        this.<>f__this.OpenWaitingForm();
                        Singleton<CResourceManager>.GetInstance().LoadResourcePackerInfoSet();
                        PlayerPrefs.SetInt("SplashHack", 1);
                        this.<textResource>__0 = Singleton<CResourceManager>.GetInstance().GetResource("Config/Splash.txt", typeof(TextAsset), enResourceType.Numeric, false, true);
                        if (this.<textResource>__0 != null)
                        {
                            this.<tAsstet>__1 = this.<textResource>__0.m_content as CBinaryObject;
                            if ((null != this.<tAsstet>__1) && (this.<tAsstet>__1.m_data != null))
                            {
                                this.<content>__2 = Encoding.UTF8.GetString(this.<tAsstet>__1.m_data);
                                if ((this.<content>__2 != null) && (this.<content>__2.Trim() == "0"))
                                {
                                    PlayerPrefs.SetInt("SplashHack", 0);
                                }
                            }
                        }
                        PlayerPrefs.Save();
                        this.$current = this.<>f__this.StartCoroutine(Singleton<CResourceManager>.GetInstance().LoadResidentAssetBundles());
                        this.$PC = 1;
                        goto Label_01A1;

                    case 1:
                        this.$current = this.<>f__this.StartCoroutine(MonoSingleton<GameFramework>.GetInstance().PrepareGameSystem());
                        this.$PC = 2;
                        goto Label_01A1;

                    case 2:
                        this.<>f__this.DisposeAnnouncementImages();
                        this.<>f__this.CloseWaitingForm();
                        Singleton<CUIManager>.GetInstance().CloseForm(CVersionUpdateSystem.s_versionUpdateFormPath);
                        this.<>f__this.m_versionUpdateFormScript = null;
                        if (this.<>f__this.m_onVersionUpdateComplete != null)
                        {
                            this.<>f__this.m_onVersionUpdateComplete();
                        }
                        this.$PC = -1;
                        break;
                }
                return false;
            Label_01A1:
                return true;
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

        public delegate void OnVersionUpdateComplete();
    }
}

