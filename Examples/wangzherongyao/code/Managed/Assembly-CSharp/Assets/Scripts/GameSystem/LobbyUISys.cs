namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.UI;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    internal class LobbyUISys : Singleton<LobbyUISys>
    {
        private LobbyForm m_LobbyForm;
        private SysEntryForm m_SysEntryForm;

        public void ActivateBg(bool bActive)
        {
        }

        public void AddRedDot(enSysEntryID sysEntryId, enRedDotPos redDotPos = 2)
        {
            this.m_SysEntryForm.AddRedDot(sysEntryId, redDotPos, 0);
        }

        public void AddRedDotEx(enSysEntryID sysEntryId, enRedDotPos redDotPos, int alertNum = 0)
        {
            this.m_SysEntryForm.AddRedDotEx(sysEntryId, redDotPos, alertNum);
        }

        public void Clear()
        {
            if (this.m_LobbyForm != null)
            {
                this.m_LobbyForm.Clear();
            }
        }

        public void DelRedDot(enSysEntryID sysEntryId)
        {
            this.m_SysEntryForm.DelRedDot(sysEntryId);
        }

        public override void Init()
        {
            this.m_LobbyForm = new LobbyForm();
            this.m_SysEntryForm = new SysEntryForm();
            this.m_LobbyForm.init();
            this.m_SysEntryForm.init();
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Lobby_OpenLobbyForm, new CUIEventManager.OnUIEventHandler(this.onOpenLobby));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Lobby_OpenSysEntryForm, new CUIEventManager.OnUIEventHandler(this.onOpenSysEntry));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.WEB_OpenURL, new CUIEventManager.OnUIEventHandler(this.onOpenWeb));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Common_WifiCheckTimer, new CUIEventManager.OnUIEventHandler(this.onCommon_WifiCheckTimer));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Common_ShowOrHideWifiInfo, new CUIEventManager.OnUIEventHandler(this.onCommon_ShowOrHideWifiInfo));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Lobby_ConfirmErrExit, new CUIEventManager.OnUIEventHandler(this.onErrorExit));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Lobby_LobbyFormShow, new CUIEventManager.OnUIEventHandler(this.Lobby_LobbyFormShow));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Lobby_LobbyFormHide, new CUIEventManager.OnUIEventHandler(this.Lobby_LobbyFormHide));
            Singleton<EventRouter>.instance.AddEventHandler("MasterAttributesChanged", new Action(this, (IntPtr) this.UpdatePlayerData));
        }

        public bool IsInLobbyForm()
        {
            return ((this.m_LobbyForm != null) && this.m_LobbyForm.m_bInLobby);
        }

        private void Lobby_LobbyFormHide(CUIEvent uiEvent)
        {
            if (this.m_SysEntryForm != null)
            {
                this.m_SysEntryForm.MiniShow();
            }
        }

        private void Lobby_LobbyFormShow(CUIEvent uiEvent)
        {
            if (this.m_SysEntryForm != null)
            {
                this.m_SysEntryForm.FullShow();
                Singleton<CMiShuSystem>.instance.CheckActPlayModeTipsForLobby();
            }
        }

        private void onCommon_ShowOrHideWifiInfo(CUIEvent uiEvent)
        {
            if (this.m_SysEntryForm != null)
            {
                this.m_SysEntryForm.ShowOrHideWifiInfo();
            }
        }

        private void onCommon_WifiCheckTimer(CUIEvent uiEvent)
        {
            if (this.m_SysEntryForm != null)
            {
                this.m_SysEntryForm.CheckWifi();
            }
        }

        private void onErrorExit(CUIEvent uiEvent)
        {
            SGameApplication.Quit();
        }

        private void onOpenLobby(CUIEvent uiEvent)
        {
            this.m_LobbyForm.OpenForm();
            if (Singleton<CLoginSystem>.GetInstance().m_fLoginBeginTime > 0f)
            {
                List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()),
                    new KeyValuePair<string, string>("status", "1")
                };
                float num = Time.time - Singleton<CLoginSystem>.GetInstance().m_fLoginBeginTime;
                events.Add(new KeyValuePair<string, string>("totaltime", num.ToString()));
                events.Add(new KeyValuePair<string, string>("errorCode", "0"));
                Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_Loginsvr", events, true);
                Singleton<CLoginSystem>.GetInstance().m_fLoginBeginTime = 0f;
            }
        }

        private void onOpenSysEntry(CUIEvent uiEvent)
        {
            if (this.m_SysEntryForm != null)
            {
                this.m_SysEntryForm.OpenForm();
            }
        }

        private void onOpenWeb(CUIEvent uiEvent)
        {
            string strUrl = "http://www.qq.com";
            CUICommonSystem.OpenUrl(strUrl, true);
        }

        public void Play_UnLock_Animation(RES_SPECIALFUNCUNLOCK_TYPE type)
        {
            this.m_LobbyForm.Play_UnLock_Animation(type);
        }

        public void SetTopBarPriority(enFormPriority prioRity)
        {
            if ((this.m_SysEntryForm != null) && (this.m_SysEntryForm.m_FormObj != null))
            {
                CUIFormScript component = this.m_SysEntryForm.m_FormObj.GetComponent<CUIFormScript>();
                if (component != null)
                {
                    component.SetPriority(prioRity);
                }
            }
        }

        public void ShowHideRankingBtn(bool show)
        {
            if (this.m_LobbyForm != null)
            {
                this.m_LobbyForm.ShowHideRankingBtn(show);
            }
        }

        public override void UnInit()
        {
            base.UnInit();
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Lobby_OpenLobbyForm, new CUIEventManager.OnUIEventHandler(this.onOpenLobby));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Lobby_OpenSysEntryForm, new CUIEventManager.OnUIEventHandler(this.onOpenSysEntry));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.WEB_OpenURL, new CUIEventManager.OnUIEventHandler(this.onOpenWeb));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Common_WifiCheckTimer, new CUIEventManager.OnUIEventHandler(this.onCommon_WifiCheckTimer));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Common_ShowOrHideWifiInfo, new CUIEventManager.OnUIEventHandler(this.onCommon_ShowOrHideWifiInfo));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Lobby_LobbyFormShow, new CUIEventManager.OnUIEventHandler(this.Lobby_LobbyFormShow));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Lobby_LobbyFormHide, new CUIEventManager.OnUIEventHandler(this.Lobby_LobbyFormHide));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Lobby_ConfirmErrExit, new CUIEventManager.OnUIEventHandler(this.onErrorExit));
            Singleton<EventRouter>.instance.RemoveEventHandler("MasterAttributesChanged", new Action(this, (IntPtr) this.UpdatePlayerData));
            this.m_LobbyForm.unInit();
            this.m_SysEntryForm.unInit();
            this.m_LobbyForm = null;
            this.m_SysEntryForm = null;
        }

        private void UpdatePlayerData()
        {
            this.m_LobbyForm.UpdatePlayerData();
            this.m_SysEntryForm.UpdatePlayerData();
        }
    }
}

