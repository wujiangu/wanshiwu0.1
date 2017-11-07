namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class InBattleMsgMgr : Singleton<InBattleMsgMgr>
    {
        private DictionaryView<uint, DictionaryView<uint, ResInBatMsgHeroActCfg>> heroActData = new DictionaryView<uint, DictionaryView<uint, ResInBatMsgHeroActCfg>>();
        public int InBat_Bubble_CDTime = 0xbb8;
        public CDButton m_cdButton;
        private CUIFormScript m_formScript;
        private InBattleMsgView m_view = new InBattleMsgView();
        private Dictionary<ulong, BubbleTimerEntity> player_bubbleTime_map = new Dictionary<ulong, BubbleTimerEntity>();
        public DictionaryView<string, ListView<TabElement>> tabElements = new DictionaryView<string, ListView<TabElement>>();
        public List<string> title_list = new List<string>();

        private bool BShouldShowButton()
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
            if (curLvelContext == null)
            {
                return false;
            }
            return (curLvelContext.isWarmBattle || Singleton<LobbyLogic>.instance.inMultiGame);
        }

        public void Clear()
        {
            Dictionary<ulong, BubbleTimerEntity>.Enumerator enumerator = this.player_bubbleTime_map.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<ulong, BubbleTimerEntity> current = enumerator.Current;
                BubbleTimerEntity entity = current.Value;
                if (entity != null)
                {
                    entity.Clear();
                }
            }
            this.player_bubbleTime_map.Clear();
            this.UnRegInBattleEvent();
            if (this.m_cdButton != null)
            {
                this.m_cdButton.Clear();
                this.m_cdButton = null;
            }
            this.m_view.Clear();
            this.m_formScript = null;
        }

        public ResInBatMsgCfg GetCfgData(uint id)
        {
            return GameDataMgr.inBattleMsgDatabin.GetDataByKey(id);
        }

        public ResInBatMsgHeroActCfg GetHeroActCfg(uint heroid, uint actID)
        {
            DictionaryView<uint, ResInBatMsgHeroActCfg> view = null;
            this.heroActData.TryGetValue(heroid, out view);
            if (view == null)
            {
                return null;
            }
            ResInBatMsgHeroActCfg cfg = null;
            view.TryGetValue(actID, out cfg);
            return cfg;
        }

        public void Handle_InBattleMsg_Ntf(COMDT_CHAT_MSG_INBATTLE obj)
        {
            if (obj != null)
            {
                ulong ullUid = obj.stFrom.ullUid;
                uint dwAcntHeroID = obj.stFrom.dwAcntHeroID;
                uint dwTextID = 0;
                if (obj.bChatType == 1)
                {
                    dwTextID = obj.stChatInfo.stSignalID.dwTextID;
                }
                else if (obj.bChatType == 2)
                {
                    dwTextID = obj.stChatInfo.stBubbleID.dwTextID;
                }
                else if (obj.bChatType == 3)
                {
                    DebugHelper.Assert(false, "暂时没有局内交流，自定义功能...");
                }
                this.InnerHandle_InBatMsg((COM_INBATTLE_CHAT_TYPE) obj.bChatType, dwAcntHeroID, dwTextID, ullUid);
            }
        }

        public void HideView()
        {
            this.m_view.Show(false);
        }

        public void InitView(GameObject cdButton, CUIFormScript formScript)
        {
            if ((cdButton != null) && (formScript != null))
            {
                this.m_view.OpenForm(null, false);
                if ((this.m_cdButton == null) && this.BShouldShowButton())
                {
                    cdButton.CustomSetActive(true);
                    this.m_cdButton = new CDButton(cdButton);
                }
                else
                {
                    cdButton.CustomSetActive(false);
                }
                this.m_formScript = formScript;
            }
        }

        public void InnerHandle_InBatMsg(COM_INBATTLE_CHAT_TYPE chatType, uint herocfgID, uint cfg_id, ulong ullUid)
        {
            ResInBatMsgHeroActCfg heroActCfg = this.GetHeroActCfg(herocfgID, cfg_id);
            ResInBatMsgCfg cfgData = this.GetCfgData(cfg_id);
            if (cfgData != null)
            {
                if (heroActCfg != null)
                {
                    Assets.Scripts.GameSystem.InBattleMsgShower.ShowInBattleMsg(chatType, ullUid, herocfgID, heroActCfg.szContent, heroActCfg.szSound);
                }
                else
                {
                    Assets.Scripts.GameSystem.InBattleMsgShower.ShowInBattleMsg(chatType, ullUid, herocfgID, cfgData.szContent, cfgData.szSound);
                }
                if ((chatType == COM_INBATTLE_CHAT_TYPE.COM_INBATTLE_CHATTYPE_SIGNAL) && (Singleton<CBattleSystem>.instance.GetMinimapSys().CurMapType() == MinimapSys.EMapType.Mini))
                {
                    ReadonlyContext<PoolObjHandle<ActorRoot>> allHeroes = Singleton<GamePlayerCenter>.instance.GetPlayerByUid(ullUid).GetAllHeroes();
                    for (int i = 0; i < allHeroes.Count; i++)
                    {
                        PoolObjHandle<ActorRoot> handle = allHeroes[i];
                        ActorRoot root = handle.handle;
                        if ((root != null) && (root.TheActorMeta.ConfigId == herocfgID))
                        {
                            Vector2 sreenLoc = CUIUtility.WorldToScreenPoint(this.m_formScript.GetCamera(), root.HudControl.GetSmallMapPointer_WorldPosition());
                            Singleton<CUIParticleSystem>.instance.AddParticle(cfgData.szMiniMapEffect, 2f, sreenLoc);
                            return;
                        }
                    }
                }
            }
        }

        public bool IsAllChannel_CD_Valid()
        {
            return true;
        }

        public bool IsChannel_CD_Valid(int channel_id)
        {
            return true;
        }

        public void On_InBattleMsg_CloseForm(CUIEvent uiEvent)
        {
            this.m_view.Show(false);
        }

        public void On_InBattleMsg_ListElement_Click(CUIEvent uiEvent)
        {
            this.m_view.On_InBattleMsg_ListElement_Click(uiEvent.m_srcWidgetIndexInBelongedList);
        }

        public void On_InBattleMsg_ListElement_Enable(CUIEvent uiEvent)
        {
            this.m_view.On_InBattleMsg_ListElement_Enable(uiEvent);
        }

        public void On_InBattleMsg_OpenForm(CUIEvent uiEvent)
        {
            this.m_view.OpenForm(uiEvent, true);
        }

        public void On_InBattleMsg_TabChange(CUIEvent uiEvent)
        {
            int selectedIndex = uiEvent.m_srcWidget.GetComponent<CUIListScript>().GetSelectedIndex();
            this.m_view.On_InBattleMsg_TabChange(selectedIndex);
        }

        public void ParseCfgData()
        {
            if (!int.TryParse(Singleton<CTextManager>.instance.GetText("InBat_Bubble_CDTime"), out this.InBat_Bubble_CDTime))
            {
                DebugHelper.Assert(false, "---InBatMsg 教练你配的 InBat_Bubble_CDTime 好像不是整数哦， check out");
            }
            ListView<TabElement> view = null;
            Dictionary<long, object>.Enumerator enumerator = GameDataMgr.inBattleMsgDatabin.GetEnumerator();
            while (enumerator.MoveNext())
            {
                view = null;
                KeyValuePair<long, object> current = enumerator.Current;
                ResInBatMsgCfg cfg = (ResInBatMsgCfg) current.Value;
                if (cfg != null)
                {
                    string szChannelTitle = cfg.szChannelTitle;
                    this.tabElements.TryGetValue(szChannelTitle, out view);
                    if (view == null)
                    {
                        view = new ListView<TabElement>();
                        this.tabElements.Add(szChannelTitle, view);
                        this.title_list.Add(szChannelTitle);
                    }
                    view.Add(new TabElement(cfg.dwID, cfg.szContent));
                }
            }
            Dictionary<long, object>.Enumerator enumerator2 = GameDataMgr.inBattleHeroActDatabin.GetEnumerator();
            while (enumerator2.MoveNext())
            {
                KeyValuePair<long, object> pair2 = enumerator2.Current;
                ResInBatMsgHeroActCfg cfg2 = (ResInBatMsgHeroActCfg) pair2.Value;
                if (cfg2 != null)
                {
                    DictionaryView<uint, ResInBatMsgHeroActCfg> view2 = null;
                    this.heroActData.TryGetValue(cfg2.dwHeroID, out view2);
                    if (view2 == null)
                    {
                        view2 = new DictionaryView<uint, ResInBatMsgHeroActCfg>();
                        this.heroActData.Add(cfg2.dwHeroID, view2);
                    }
                    if (!view2.ContainsKey(cfg2.dwActionID))
                    {
                        view2.Add(cfg2.dwActionID, cfg2);
                    }
                }
            }
            GameDataMgr.inBattleHeroActDatabin.Unload();
        }

        public void RegInBattleEvent()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.InBattleMsg_OpenForm, new CUIEventManager.OnUIEventHandler(this.On_InBattleMsg_OpenForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.InBattleMsg_CloseForm, new CUIEventManager.OnUIEventHandler(this.On_InBattleMsg_CloseForm));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.InBattleMsg_TabChange, new CUIEventManager.OnUIEventHandler(this.On_InBattleMsg_TabChange));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.InBattleMsg_ListElement_Enable, new CUIEventManager.OnUIEventHandler(this.On_InBattleMsg_ListElement_Enable));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.InBattleMsg_ListElement_Click, new CUIEventManager.OnUIEventHandler(this.On_InBattleMsg_ListElement_Click));
        }

        public void UnRegInBattleEvent()
        {
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.InBattleMsg_OpenForm, new CUIEventManager.OnUIEventHandler(this.On_InBattleMsg_OpenForm));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.InBattleMsg_CloseForm, new CUIEventManager.OnUIEventHandler(this.On_InBattleMsg_CloseForm));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.InBattleMsg_TabChange, new CUIEventManager.OnUIEventHandler(this.On_InBattleMsg_TabChange));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.InBattleMsg_ListElement_Enable, new CUIEventManager.OnUIEventHandler(this.On_InBattleMsg_ListElement_Enable));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.InBattleMsg_ListElement_Click, new CUIEventManager.OnUIEventHandler(this.On_InBattleMsg_ListElement_Click));
        }

        public void Update()
        {
            if (this.m_cdButton != null)
            {
                this.m_cdButton.Update();
            }
        }

        public void UpdatePlayerBubbleTimer(ulong playerid, uint heroid)
        {
            BubbleTimerEntity entity = null;
            this.player_bubbleTime_map.TryGetValue(playerid, out entity);
            if (entity == null)
            {
                entity = new BubbleTimerEntity(playerid, heroid, this.InBat_Bubble_CDTime);
                this.player_bubbleTime_map.Add(playerid, entity);
            }
            entity.Start();
        }
    }
}

