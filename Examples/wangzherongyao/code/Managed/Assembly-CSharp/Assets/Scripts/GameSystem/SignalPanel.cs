namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.GameLogic.GameKernal;
    using Assets.Scripts.UI;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class SignalPanel
    {
        private const float c_latestSignalTipsMaxDuringTime = 5f;
        private Plane m_battleSceneGroundPlane;
        private CUIFormScript m_formScript;
        private float m_latestSignalTipsDuringTime;
        private GameObject m_miniMap;
        private Vector2 m_miniMapScreenPosition;
        private Dictionary<uint, CPlayerSignalCooldown> m_playerSignalCooldowns;
        private int m_selectedSignalID = -1;
        private CSignalButton[] m_signalButtons;
        private CUIContainerScript m_signalInUIContainer_big;
        private CUIContainerScript m_signalInUIContainer_small;
        private ListView<CSignal> m_signals;
        private CUIContainerScript m_signalSrcHeroNameContainer;
        private ListView<CSignalTipsElement> m_signalTipses;
        private CUIListScript m_signalTipsList;
        private CanvasGroup m_signalTipsListCanvasGroup;
        private bool m_useSignalButton;
        private static int[][] s_signalButtonInfos;
        private const float speed = 0.15f;

        static SignalPanel()
        {
            int[][] numArrayArray1 = new int[4][];
            numArrayArray1[0] = new int[] { 1, 12 };
            numArrayArray1[1] = new int[] { 2, 13 };
            numArrayArray1[2] = new int[] { 3, 14 };
            numArrayArray1[3] = new int[] { 4, 15 };
            s_signalButtonInfos = numArrayArray1;
        }

        public void Add_SignalTip(CSignalTipsElement obj)
        {
            if (obj != null)
            {
                this.m_signalTipses.Add(obj);
            }
            this.RefreshSignalTipsList();
        }

        public void CancelSelectedSignalButton()
        {
            if (this.m_useSignalButton && (this.m_selectedSignalID >= 0))
            {
                CSignalButton singleButton = this.GetSingleButton(this.m_selectedSignalID);
                if (singleButton != null)
                {
                    singleButton.SetHighLight(false);
                }
                this.m_selectedSignalID = -1;
            }
        }

        public void Clear()
        {
            if (this.m_useSignalButton)
            {
                Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_ClickMiniMap, new CUIEventManager.OnUIEventHandler(this.OnClickMiniMap));
                Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_Click_Scene, new CUIEventManager.OnUIEventHandler(this.OnClickBattleScene));
                Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_OnSignalButtonClicked, new CUIEventManager.OnUIEventHandler(this.OnSignalButtonClicked));
                Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_OnSignalTipsListElementEnable, new CUIEventManager.OnUIEventHandler(this.OnSignalListElementEnabled));
                this.m_signals = null;
                this.m_playerSignalCooldowns = null;
                this.m_signalTipses = null;
            }
            this.m_signalButtons = null;
            this.m_formScript = null;
            this.m_miniMap = null;
            this.m_signalSrcHeroNameContainer = null;
            this.m_signalInUIContainer_small = null;
            this.m_signalInUIContainer_big = null;
            this.m_signalTipsList = null;
        }

        private void ClearSignalTipses()
        {
            if ((this.m_signalTipses != null) && (this.m_signalTipses.Count > 0))
            {
                this.m_signalTipses.Clear();
                if (this.m_signalTipsList != null)
                {
                    this.m_signalTipsList.SetElementAmount(0);
                    this.m_signalTipsList.ResetContentPosition();
                }
            }
        }

        public void ExecCommand(uint playerID, uint heroID, int signalID, int worldPositionX, int worldPositionY, int worldPositionZ, byte bAlice = 0, byte elementType = 0, uint targetObjID = 0, uint targetHeroID = 0)
        {
            if (this.m_useSignalButton && (this.m_formScript != null))
            {
                uint cooldownTime = 0x1388;
                ResSignalInfo dataByKey = GameDataMgr.signalDatabin.GetDataByKey(signalID);
                if (dataByKey == null)
                {
                    DebugHelper.Assert(dataByKey != null, "ExecCommand signalInfo is null, check out...");
                }
                else
                {
                    if (dataByKey != null)
                    {
                        cooldownTime = (uint) (dataByKey.bCooldownTime * 0x3e8);
                    }
                    if (Singleton<BattleLogic>.GetInstance().GetCurLvelContext().iLevelID == CBattleGuideManager.GuideLevelID5v5)
                    {
                        cooldownTime = 0x7d0;
                    }
                    ulong logicFrameTick = Singleton<FrameSynchr>.GetInstance().LogicFrameTick;
                    CPlayerSignalCooldown cooldown = null;
                    this.m_playerSignalCooldowns.TryGetValue(playerID, out cooldown);
                    if (cooldown != null)
                    {
                        if (((uint) (logicFrameTick - cooldown.m_lastSignalExecuteTimestamp)) < cooldown.m_cooldownTime)
                        {
                            return;
                        }
                        cooldown.m_lastSignalExecuteTimestamp = logicFrameTick;
                        cooldown.m_cooldownTime = cooldownTime;
                    }
                    else
                    {
                        cooldown = new CPlayerSignalCooldown(logicFrameTick, cooldownTime);
                        this.m_playerSignalCooldowns.Add(playerID, cooldown);
                    }
                    Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
                    Player player = Singleton<GamePlayerCenter>.GetInstance().GetPlayer(playerID);
                    if (((hostPlayer != null) && (player != null)) && (hostPlayer.PlayerCamp == player.PlayerCamp))
                    {
                        uint num4;
                        if ((hostPlayer == player) && (this.m_signalButtons != null))
                        {
                            for (int i = 0; i < this.m_signalButtons.Length; i++)
                            {
                                if (this.m_signalButtons[i] != null)
                                {
                                    this.m_signalButtons[i].StartCooldown(cooldownTime);
                                }
                            }
                        }
                        bool bSmall = Singleton<CBattleSystem>.instance.GetMinimapSys().CurMapType() == MinimapSys.EMapType.Mini;
                        uint num5 = 0;
                        if (targetObjID == 0)
                        {
                            num4 = playerID;
                            num5 = heroID;
                        }
                        else
                        {
                            num4 = targetObjID;
                            num5 = targetHeroID;
                        }
                        this.PlaySignalTipsSound(elementType, bAlice, targetHeroID);
                        bool bFollow = elementType == 3;
                        bool bUseCfgSound = elementType == 0;
                        CSignal item = new CSignal(num4, signalID, num5, worldPositionX, worldPositionY, worldPositionZ, !bSmall ? this.m_signalInUIContainer_big : this.m_signalInUIContainer_small, this.m_signalSrcHeroNameContainer, bSmall, bFollow, bUseCfgSound, (MinimapSys.ElementType) elementType);
                        item.Initialize(this.m_formScript);
                        this.m_signals.Add(item);
                        CSignalTips tips = new CSignalTips(signalID, heroID, hostPlayer == player, bAlice, elementType, targetHeroID);
                        this.Add_SignalTip(tips);
                    }
                }
            }
        }

        public CSignalButton GetSingleButton(int signalID)
        {
            if (this.m_signalButtons != null)
            {
                for (int i = 0; i < this.m_signalButtons.Length; i++)
                {
                    if ((this.m_signalButtons[i] != null) && (this.m_signalButtons[i].m_signalID == signalID))
                    {
                        return this.m_signalButtons[i];
                    }
                }
            }
            return null;
        }

        public void Init(CUIFormScript formScript, GameObject minimapGameObject, GameObject signalSrcHeroNameContainer, GameObject signalTipsList, bool useSignalButton)
        {
            if (formScript != null)
            {
                this.m_formScript = formScript;
                this.m_miniMap = minimapGameObject;
                if (this.m_miniMap != null)
                {
                    this.m_miniMapScreenPosition = CUIUtility.WorldToScreenPoint(formScript.GetCamera(), this.m_miniMap.transform.position);
                }
                this.m_signalInUIContainer_small = formScript.GetWidget(0x21).GetComponent<CUIContainerScript>();
                this.m_signalInUIContainer_big = formScript.GetWidget(0x41).GetComponent<CUIContainerScript>();
                this.m_signalSrcHeroNameContainer = (signalSrcHeroNameContainer != null) ? signalSrcHeroNameContainer.GetComponent<CUIContainerScript>() : null;
                this.m_signalTipsList = (signalTipsList != null) ? signalTipsList.GetComponent<CUIListScript>() : null;
                if (this.m_signalTipsList != null)
                {
                    this.m_signalTipsListCanvasGroup = this.m_signalTipsList.gameObject.GetComponent<CanvasGroup>();
                    if (this.m_signalTipsListCanvasGroup == null)
                    {
                        this.m_signalTipsListCanvasGroup = this.m_signalTipsList.gameObject.AddComponent<CanvasGroup>();
                    }
                    this.m_signalTipsListCanvasGroup.alpha = 0f;
                    this.m_signalTipsListCanvasGroup.blocksRaycasts = false;
                }
                this.m_useSignalButton = useSignalButton;
                this.m_signalButtons = new CSignalButton[s_signalButtonInfos.Length];
                for (int i = 0; i < this.m_signalButtons.Length; i++)
                {
                    this.m_signalButtons[i] = new CSignalButton(this.m_formScript.GetWidget(s_signalButtonInfos[i][1]), s_signalButtonInfos[i][0]);
                    this.m_signalButtons[i].Initialize(this.m_formScript);
                    if (!useSignalButton)
                    {
                        this.m_signalButtons[i].Disable();
                    }
                }
                if (this.m_useSignalButton)
                {
                    Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_ClickMiniMap, new CUIEventManager.OnUIEventHandler(this.OnClickMiniMap));
                    Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_Click_Scene, new CUIEventManager.OnUIEventHandler(this.OnClickBattleScene));
                    Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnSignalButtonClicked, new CUIEventManager.OnUIEventHandler(this.OnSignalButtonClicked));
                    Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_OnSignalTipsListElementEnable, new CUIEventManager.OnUIEventHandler(this.OnSignalListElementEnabled));
                    this.m_battleSceneGroundPlane = new Plane(new Vector3(0f, 1f, 0f), 0.15f);
                    this.m_signals = new ListView<CSignal>();
                    this.m_playerSignalCooldowns = new Dictionary<uint, CPlayerSignalCooldown>();
                    this.m_signalTipses = new ListView<CSignalTipsElement>();
                }
            }
        }

        public bool IsUseSingalButton()
        {
            if (this.m_miniMap == null)
            {
                return false;
            }
            return (this.m_useSignalButton && (this.m_selectedSignalID >= 0));
        }

        private void OnClickBattleScene(CUIEvent uievent)
        {
            if (!this.m_useSignalButton || (this.m_selectedSignalID < 0))
            {
                Singleton<CBattleSystem>.instance.GetMinimapSys().Switch(MinimapSys.EMapType.Mini);
                Singleton<InBattleMsgMgr>.instance.HideView();
            }
            else
            {
                float num;
                Ray ray = Camera.main.ScreenPointToRay((Vector3) uievent.m_pointerEventData.position);
                if (this.m_battleSceneGroundPlane.Raycast(ray, out num))
                {
                    Vector3 point = ray.GetPoint(num);
                    this.SendFrameCommand(this.m_selectedSignalID, (int) point.x, (int) point.y, (int) point.z, 0, 0, 0, 0);
                }
            }
        }

        public void OnClickMiniMap(CUIEvent uiEvent)
        {
            if ((this.m_useSignalButton && (this.m_selectedSignalID >= 0)) && (this.m_miniMap != null))
            {
                VInt num;
                Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
                ActorRoot root = (hostPlayer != null) ? hostPlayer.Captain.handle : null;
                this.m_miniMapScreenPosition = CUIUtility.WorldToScreenPoint(uiEvent.m_srcFormScript.GetCamera(), this.m_miniMap.transform.position);
                Vector3 zero = Vector3.zero;
                zero.x = (uiEvent.m_pointerEventData.position.x - this.m_miniMapScreenPosition.x) * Singleton<CBattleSystem>.GetInstance().UI_world_Factor_Small.x;
                zero.y = (root == null) ? 0.15f : ((Vector3) root.location).y;
                zero.z = (uiEvent.m_pointerEventData.position.y - this.m_miniMapScreenPosition.y) * Singleton<CBattleSystem>.GetInstance().UI_world_Factor_Small.y;
                PathfindingUtility.GetGroundY((VInt3) zero, out num);
                zero.y = num.scalar;
                this.SendFrameCommand(this.m_selectedSignalID, (int) zero.x, (int) zero.y, (int) zero.z, 0, 0, 0, 0);
            }
        }

        private void OnSignalButtonClicked(CUIEvent uiEvent)
        {
            if (!Singleton<CBattleGuideManager>.instance.bPauseGame && this.m_useSignalButton)
            {
                int tag = uiEvent.m_eventParams.tag;
                CSignalButton singleButton = this.GetSingleButton(tag);
                if ((singleButton != null) && !singleButton.IsInCooldown())
                {
                    if (singleButton.m_signalInfo.bSignalType == 0)
                    {
                        if (this.m_selectedSignalID != tag)
                        {
                            if (this.m_selectedSignalID >= 0)
                            {
                                CSignalButton button2 = this.GetSingleButton(this.m_selectedSignalID);
                                if (button2 != null)
                                {
                                    button2.SetHighLight(false);
                                }
                            }
                            this.m_selectedSignalID = tag;
                            singleButton.SetHighLight(true);
                        }
                    }
                    else
                    {
                        this.SendFrameCommand(tag, 0, 0, 0, 0, 0, 0, 0);
                    }
                }
            }
        }

        private void OnSignalListElementEnabled(CUIEvent uiEvent)
        {
            CUIListElementScript srcWidgetScript = (CUIListElementScript) uiEvent.m_srcWidgetScript;
            int index = srcWidgetScript.m_index;
            if ((index >= 0) || (index < this.m_signalTipses.Count))
            {
                CSignalTipShower component = srcWidgetScript.GetComponent<CSignalTipShower>();
                if (component != null)
                {
                    component.Set(this.m_signalTipses[index], uiEvent.m_srcFormScript);
                }
            }
        }

        private void PlaySignalTipsSound(byte elementType, byte bAlice, uint targetHeroID)
        {
            string str = string.Empty;
            switch (elementType)
            {
                case 1:
                    str = (bAlice != 1) ? "Play_notice_map_1" : "Play_notice_map_2";
                    break;

                case 2:
                    str = (bAlice != 1) ? "Play_sys_bobao_jihe_6" : "Play_sys_bobao_jihe_7";
                    break;

                case 3:
                {
                    ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey(targetHeroID);
                    if ((dataByKey != null) && !string.IsNullOrEmpty(dataByKey.szHeroSound))
                    {
                        Singleton<CSoundManager>.GetInstance().PlayBattleSound(dataByKey.szHeroSound, null);
                    }
                    str = (bAlice != 1) ? "Play_Call_Attack" : "Play_Call_Guard";
                    break;
                }
                case 4:
                    str = "Play_sys_bobao_jihe_3";
                    break;

                case 5:
                    str = "Play_sys_bobao_jihe_5";
                    break;

                case 6:
                    str = "Play_sys_bobao_jihe_4";
                    break;
            }
            if (!string.IsNullOrEmpty(str))
            {
                Singleton<CSoundManager>.GetInstance().PlayBattleSound(str, null);
            }
        }

        private void RefreshSignalTipsList()
        {
            this.m_latestSignalTipsDuringTime = 5f;
            if (this.m_signalTipsList != null)
            {
                this.m_signalTipsList.SetElementAmount(this.m_signalTipses.Count);
                this.m_signalTipsList.MoveElementInScrollArea(this.m_signalTipses.Count - 1, false);
            }
        }

        public void SendFrameCommand(int signalID, int worldPositionX, int worldPositionY, int worldPositionZ, byte bAlliance = 0, byte type = 0, uint targetObj_id = 0, uint targetHeroID = 0)
        {
            this.CancelSelectedSignalButton();
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if (((hostPlayer != null) && (hostPlayer.Captain != 0)) && (hostPlayer.Captain.handle != null))
            {
                FrameCommand<SignalCommand> command = FrameCommandFactory.CreateFrameCommand<SignalCommand>();
                command.playerID = hostPlayer.PlayerId;
                command.cmdData.m_heroID = (uint) hostPlayer.Captain.handle.TheActorMeta.ConfigId;
                command.cmdData.m_signalID = (byte) signalID;
                command.cmdData.m_worldPositionX = worldPositionX;
                command.cmdData.m_worldPositionY = worldPositionY;
                command.cmdData.m_worldPositionZ = worldPositionZ;
                command.cmdData.m_bAlies = bAlliance;
                command.cmdData.m_elementType = type;
                command.cmdData.m_targetObjId = targetObj_id;
                command.cmdData.m_targetHeroID = targetHeroID;
                command.Send();
            }
        }

        public void Update()
        {
            if (this.m_useSignalButton)
            {
                if (this.m_signalButtons != null)
                {
                    for (int i = 0; i < this.m_signalButtons.Length; i++)
                    {
                        if (this.m_signalButtons[i] != null)
                        {
                            this.m_signalButtons[i].UpdateCooldown();
                        }
                    }
                }
                if (this.m_signals != null)
                {
                    int index = 0;
                    while (index < this.m_signals.Count)
                    {
                        if (this.m_signals[index].IsNeedDisposed())
                        {
                            this.m_signals[index].Dispose();
                            this.m_signals.RemoveAt(index);
                        }
                        else
                        {
                            this.m_signals[index].Update(this.m_formScript, Time.deltaTime);
                            index++;
                        }
                    }
                }
                this.UpdateSignalTipses();
            }
        }

        private void UpdateSignalTipses()
        {
            if (this.m_signalTipsListCanvasGroup != null)
            {
                if (this.m_latestSignalTipsDuringTime > 0f)
                {
                    if (this.m_signalTipsListCanvasGroup.alpha < 1f)
                    {
                        this.m_signalTipsListCanvasGroup.alpha += 0.15f;
                        if (this.m_signalTipsListCanvasGroup.alpha > 1f)
                        {
                            this.m_signalTipsListCanvasGroup.alpha = 1f;
                        }
                    }
                }
                else if (this.m_signalTipsListCanvasGroup.alpha > 0f)
                {
                    this.m_signalTipsListCanvasGroup.alpha -= 0.15f;
                    if (this.m_signalTipsListCanvasGroup.alpha < 0f)
                    {
                        this.m_signalTipsListCanvasGroup.alpha = 0f;
                    }
                }
                else
                {
                    this.ClearSignalTipses();
                }
            }
            if (((this.m_latestSignalTipsDuringTime > 0f) && (this.m_signalTipsList != null)) && this.m_signalTipsList.IsElementInScrollArea(this.m_signalTipsList.GetElementAmount() - 1))
            {
                this.m_latestSignalTipsDuringTime -= Time.deltaTime;
                if (this.m_latestSignalTipsDuringTime < 0f)
                {
                    this.m_latestSignalTipsDuringTime = 0f;
                }
            }
        }

        private class CPlayerSignalCooldown
        {
            public uint m_cooldownTime;
            public ulong m_lastSignalExecuteTimestamp;

            public CPlayerSignalCooldown(ulong lastSignalExecuteTimestamp, uint cooldownTime)
            {
                this.m_lastSignalExecuteTimestamp = lastSignalExecuteTimestamp;
                this.m_cooldownTime = cooldownTime;
            }
        }
    }
}

