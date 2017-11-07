namespace Assets.Scripts.GameLogic
{
    using Apollo;
    using Assets.Scripts.Framework;
    using Assets.Scripts.Sound;
    using CSProtocol;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public sealed class GameBuilder : Singleton<GameBuilder>
    {
        private List<KeyValuePair<string, string>> m_eventsLoadingTime = new List<KeyValuePair<string, string>>();
        private float m_fLoadingTime;
        public int m_iMapId;
        public COM_GAME_TYPE m_kGameType = COM_GAME_TYPE.COM_GAME_TYPE_MAX;

        public void EndGame()
        {
            if (Singleton<BattleLogic>.instance.isRuning)
            {
                Singleton<LobbyLogic>.GetInstance().StopGameEndTimer();
                Singleton<LobbyLogic>.GetInstance().StopSettleMsgTimer();
                Singleton<LobbyLogic>.GetInstance().StopSettlePanelTimer();
                MonoSingleton<GameLoader>.instance.AdvanceStopLoad();
                Singleton<FrameSynchr>.instance.bEscape = false;
                Singleton<CBattleGuideManager>.GetInstance().resetPause();
                if (this.gameInfo != null)
                {
                    this.gameInfo.EndGame();
                }
                Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("GameBuild.EndGame", null, true);
                List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()),
                    new KeyValuePair<string, string>("GameType", this.m_kGameType.ToString()),
                    new KeyValuePair<string, string>("MapID", this.m_iMapId.ToString()),
                    new KeyValuePair<string, string>("LoadingTime", this.m_fLoadingTime.ToString())
                };
                Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_LoadingBattle", events, true);
                float num = (Singleton<FrameSynchr>.GetInstance().LogicFrameTick * 0.016667f) * 0.001f;
                List<KeyValuePair<string, string>> list2 = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()),
                    new KeyValuePair<string, string>("GameType", this.m_kGameType.ToString()),
                    new KeyValuePair<string, string>("MapID", this.m_iMapId.ToString()),
                    new KeyValuePair<string, string>("Max_FPS", Singleton<CBattleSystem>.GetInstance().m_MaxBattleFPS.ToString()),
                    new KeyValuePair<string, string>("Min_FPS", Singleton<CBattleSystem>.GetInstance().m_MinBattleFPS.ToString()),
                    new KeyValuePair<string, string>("Avg_FPS", Singleton<CBattleSystem>.GetInstance().m_AveBattleFPS.ToString()),
                    new KeyValuePair<string, string>("Ab_FPS_time", Singleton<BattleLogic>.GetInstance().m_Ab_FPS_time.ToString()),
                    new KeyValuePair<string, string>("Abnormal_FPS", Singleton<BattleLogic>.GetInstance().m_Abnormal_FPS_Count.ToString()),
                    new KeyValuePair<string, string>("Min_Ping", Singleton<FrameSynchr>.instance.m_MinPing.ToString()),
                    new KeyValuePair<string, string>("Max_Ping", Singleton<FrameSynchr>.instance.m_MaxPing.ToString()),
                    new KeyValuePair<string, string>("Avg_Ping", Singleton<FrameSynchr>.instance.m_AvePing.ToString()),
                    new KeyValuePair<string, string>("Abnormal_Ping", Singleton<FrameSynchr>.instance.m_Abnormal_PingCount.ToString()),
                    new KeyValuePair<string, string>("Param_Battle_Time", num.ToString()),
                    new KeyValuePair<string, string>("BattleSvr_Reconnect", Singleton<NetworkModule>.GetInstance().m_GameReconnetCount.ToString()),
                    new KeyValuePair<string, string>("GameSvr_Reconnect", Singleton<NetworkModule>.GetInstance().m_lobbyReconnetCount.ToString()),
                    new KeyValuePair<string, string>("Drag_Times", string.Empty)
                };
                Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_PVPBattle_Summary", list2, true);
                List<KeyValuePair<string, string>> list3 = new List<KeyValuePair<string, string>>();
                float num2 = Singleton<BattleLogic>.GetInstance().m_fAveFPS / ((float) Singleton<BattleLogic>.GetInstance().m_fpsCount);
                list3.Add(new KeyValuePair<string, string>("AveFPS", num2.ToString()));
                list3.Add(new KeyValuePair<string, string>("<10FPSCount", Singleton<BattleLogic>.GetInstance().m_fpsCunt10.ToString()));
                list3.Add(new KeyValuePair<string, string>("<18FPSCount", Singleton<BattleLogic>.GetInstance().m_fpsCunt18.ToString()));
                if (this.m_eventsLoadingTime != null)
                {
                    for (int i = 0; i < this.m_eventsLoadingTime.Count; i++)
                    {
                        KeyValuePair<string, string> item = this.m_eventsLoadingTime[i];
                        list3.Add(item);
                    }
                }
                Singleton<BeaconHelper>.GetInstance().EventBase(ref list3);
                if (num2 >= 25f)
                {
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_ave >= 25", list3, true);
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_ave >= 25_" + Singleton<BattleLogic>.GetInstance().GetLevelTypeDescription(), list3, true);
                }
                else if ((num2 >= 20f) && (num2 < 25f))
                {
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_ave >= 20", list3, true);
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_ave >= 20_" + Singleton<BattleLogic>.GetInstance().GetLevelTypeDescription(), list3, true);
                }
                else if ((num2 >= 18f) && (num2 < 20f))
                {
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_ave >= 18", list3, true);
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_ave >= 18_" + Singleton<BattleLogic>.GetInstance().GetLevelTypeDescription(), list3, true);
                }
                else if ((num2 >= 15f) && (num2 < 18f))
                {
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_ave >= 15", list3, true);
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_ave >= 15_" + Singleton<BattleLogic>.GetInstance().GetLevelTypeDescription(), list3, true);
                }
                else if ((num2 >= 10f) && (num2 < 15f))
                {
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_ave >= 10", list3, true);
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_ave >= 10_" + Singleton<BattleLogic>.GetInstance().GetLevelTypeDescription(), list3, true);
                }
                else
                {
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_ave < 10", list3, true);
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_ave < 10_" + Singleton<BattleLogic>.GetInstance().GetLevelTypeDescription(), list3, true);
                }
                float num4 = ((float) Singleton<BattleLogic>.GetInstance().m_fpsCunt10) / ((float) Singleton<BattleLogic>.GetInstance().m_fpsCount);
                int num5 = Mathf.CeilToInt((num4 * 100f) / 10f) * 10;
                Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS_<=10_Percent:" + num5.ToString() + "%", null, true);
                float num6 = ((float) (Singleton<BattleLogic>.GetInstance().m_fpsCunt18 + Singleton<BattleLogic>.GetInstance().m_fpsCunt10)) / ((float) Singleton<BattleLogic>.GetInstance().m_fpsCount);
                int num7 = Mathf.CeilToInt((num6 * 100f) / 10f) * 10;
                Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Event_FPS<=18_Percent:" + num7.ToString() + "%", null, true);
                this.m_eventsLoadingTime.Clear();
                Singleton<FrameSynchr>.instance.ReportPingToBeacon();
                try
                {
                    CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x1388);
                    msg.stPkgData.stCltPerformance.iMapID = this.m_iMapId;
                    msg.stPkgData.stCltPerformance.iPlayerCnt = Singleton<GamePlayerCenter>.instance.GetAllPlayers().Count;
                    msg.stPkgData.stCltPerformance.chModelLOD = (sbyte) GameSettings.ModelLOD;
                    msg.stPkgData.stCltPerformance.chParticleLOD = (sbyte) GameSettings.ParticleLOD;
                    msg.stPkgData.stCltPerformance.chCameraHeight = (sbyte) GameSettings.CameraHeight;
                    msg.stPkgData.stCltPerformance.chEnableOutline = !GameSettings.EnableOutline ? ((sbyte) 0) : ((sbyte) 1);
                    msg.stPkgData.stCltPerformance.iFps10PercentNum = num5;
                    msg.stPkgData.stCltPerformance.iFps18PercentNum = num7;
                    msg.stPkgData.stCltPerformance.iAveFps = (int) Singleton<CBattleSystem>.GetInstance().m_AveBattleFPS;
                    msg.stPkgData.stCltPerformance.iPingAverage = Singleton<FrameSynchr>.instance.m_PingAverage;
                    msg.stPkgData.stCltPerformance.iPingVariance = Singleton<FrameSynchr>.instance.m_PingVariance;
                    Utility.StringToByteArray(SystemInfo.deviceModel, ref msg.stPkgData.stCltPerformance.szDeviceModel);
                    Utility.StringToByteArray(SystemInfo.graphicsDeviceName, ref msg.stPkgData.stCltPerformance.szGPUName);
                    msg.stPkgData.stCltPerformance.iCpuCoreNum = SystemInfo.processorCount;
                    msg.stPkgData.stCltPerformance.iSysMemorySize = SystemInfo.systemMemorySize;
                    msg.stPkgData.stCltPerformance.iAvailMemory = DeviceCheckSys.GetAvailMemory();
                    Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
                }
                catch (Exception exception)
                {
                    Debug.Log(exception.Message);
                }
                Singleton<BattleStatistic>.instance.unInitEvent();
                MonoSingleton<DialogueProcessor>.GetInstance().Uninit();
                Singleton<TipProcessor>.GetInstance().Uninit();
                Singleton<LobbyLogic>.instance.inMultiRoom = false;
                Singleton<LobbyLogic>.instance.inMultiGame = false;
                Singleton<BattleLogic>.GetInstance().isRuning = false;
                Singleton<BattleLogic>.GetInstance().isFighting = false;
                Singleton<BattleLogic>.GetInstance().isGameOver = false;
                Singleton<BattleLogic>.GetInstance().isWaitMultiStart = false;
                Singleton<NetworkModule>.GetInstance().CloseGameServerConnect();
                Singleton<ShenFuSystem>.instance.ClearAll();
                MonoSingleton<ActionManager>.GetInstance().ForceStop();
                Singleton<GameObjMgr>.GetInstance().ClearActor();
                Singleton<SceneManagement>.GetInstance().Clear();
                MonoSingleton<SceneMgr>.GetInstance().ClearAll();
                Singleton<GamePlayerCenter>.GetInstance().ClearAllPlayers();
                Singleton<ActorDataCenter>.instance.ClearHeroServerData();
                Singleton<CSoundManager>.GetInstance().UnloadBanks(CSoundManager.BankType.Battle);
                Singleton<FrameSynchr>.GetInstance().ResetSynchr();
                Singleton<GameReplayModule>.GetInstance().OnMultiGameEnd();
                Singleton<BattleLogic>.GetInstance().ResetBattleSystem();
                if (!Singleton<GameStateCtrl>.instance.isLobbyState)
                {
                    Singleton<GameStateCtrl>.GetInstance().GotoState("LobbyState");
                }
                Singleton<BattleSkillHudControl>.DestroyInstance();
                this.m_kGameType = COM_GAME_TYPE.COM_GAME_TYPE_MAX;
                this.m_iMapId = 0;
                Singleton<BattleStatistic>.instance.PostEndGame();
            }
        }

        private void OnGameLoadComplete()
        {
            if (!Singleton<BattleLogic>.instance.isRuning)
            {
                DebugHelper.Assert(false, "都没有在游戏局内，何来的游戏加载完成");
            }
            else
            {
                this.gameInfo.PostBeginPlay();
                this.m_fLoadingTime = Time.time - this.m_fLoadingTime;
                if (MonoSingleton<Reconnection>.GetInstance().g_fBeginReconnectTime > 0f)
                {
                    List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>>();
                    float num = Time.time - MonoSingleton<Reconnection>.GetInstance().g_fBeginReconnectTime;
                    events.Add(new KeyValuePair<string, string>("ReconnectTime", num.ToString()));
                    MonoSingleton<Reconnection>.GetInstance().g_fBeginReconnectTime = -1f;
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_Reconnet_IntoGame", events, true);
                }
            }
        }

        private void onGameLoadProgress(float progress)
        {
            if (this.gameInfo != null)
            {
                this.gameInfo.OnLoadingProgress(progress);
            }
        }

        public void RestoreGame()
        {
        }

        public IGameInfo StartGame(IGameContext InGameContext)
        {
            DebugHelper.Assert(InGameContext != null);
            if (InGameContext == null)
            {
                return null;
            }
            if (Singleton<BattleLogic>.instance.isRuning)
            {
                return null;
            }
            this.m_fLoadingTime = Time.time;
            this.m_eventsLoadingTime.Clear();
            ApolloAccountInfo accountInfo = Singleton<ApolloHelper>.GetInstance().GetAccountInfo(false);
            DebugHelper.Assert(accountInfo != null, "account info is null");
            this.m_iMapId = InGameContext.levelContext.iLevelID;
            this.m_kGameType = InGameContext.levelContext.GameType;
            this.m_eventsLoadingTime.Add(new KeyValuePair<string, string>("OpenID", (accountInfo == null) ? "0" : accountInfo.OpenId));
            this.m_eventsLoadingTime.Add(new KeyValuePair<string, string>("LevelID", InGameContext.levelContext.iLevelID.ToString()));
            this.m_eventsLoadingTime.Add(new KeyValuePair<string, string>("isPVPLevel", InGameContext.levelContext.isPVPLevel.ToString()));
            this.m_eventsLoadingTime.Add(new KeyValuePair<string, string>("isPVPMode", InGameContext.levelContext.isPVPMode.ToString()));
            this.m_eventsLoadingTime.Add(new KeyValuePair<string, string>("bLevelNo", InGameContext.levelContext.bLevelNo.ToString()));
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("GameBuilder.StartGame", this.m_eventsLoadingTime, true);
            if (InGameContext.levelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_BURNING)
            {
                Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Burning Game Started", null, true);
            }
            Singleton<BattleLogic>.GetInstance().isRuning = true;
            Singleton<BattleLogic>.GetInstance().isFighting = false;
            Singleton<BattleLogic>.GetInstance().isGameOver = false;
            Singleton<BattleLogic>.GetInstance().isWaitMultiStart = false;
            MonoSingleton<ActionManager>.GetInstance().ForceStop();
            Singleton<GameObjMgr>.GetInstance().ClearActor();
            Singleton<SceneManagement>.GetInstance().Clear();
            MonoSingleton<SceneMgr>.GetInstance().ClearAll();
            MonoSingleton<GameLoader>.GetInstance().ResetLoader();
            InGameContext.PrepareStartup();
            if (!MonoSingleton<GameFramework>.instance.EditorPreviewMode)
            {
                DebugHelper.Assert(InGameContext.levelContext != null);
                DebugHelper.Assert(!string.IsNullOrEmpty(InGameContext.levelDesignFileName));
                if (string.IsNullOrEmpty(InGameContext.levelArtistFileName))
                {
                    MonoSingleton<GameLoader>.instance.AddLevel(InGameContext.levelDesignFileName);
                }
                else
                {
                    MonoSingleton<GameLoader>.instance.AddDesignSerializedLevel(InGameContext.levelDesignFileName);
                    MonoSingleton<GameLoader>.instance.AddArtistSerializedLevel(InGameContext.levelArtistFileName);
                }
                MonoSingleton<GameLoader>.instance.AddSoundBank("Effect_Common");
                MonoSingleton<GameLoader>.instance.AddSoundBank("System_Voice");
            }
            IGameInfo info2 = InGameContext.CreateGame();
            DebugHelper.Assert(info2 != null, "can't create game logic object!");
            this.gameInfo = info2;
            info2.PreBeginPlay();
            Singleton<BattleLogic>.instance.m_GameInfo = this.gameInfo;
            Singleton<TreasureChestMgr>.instance.Reset(InGameContext.levelContext, InGameContext.rewardCount);
            if (!MonoSingleton<GameFramework>.instance.EditorPreviewMode)
            {
                MonoSingleton<GameLoader>.GetInstance().Load(new GameLoader.LoadProgressDelegate(this.onGameLoadProgress), new GameLoader.LoadCompleteDelegate(this.OnGameLoadComplete));
                MonoSingleton<VoiceSys>.GetInstance().HeroSelectTobattle();
                Singleton<GameStateCtrl>.GetInstance().GotoState("LoadingState");
            }
            return info2;
        }

        public void StoreGame()
        {
        }

        public IGameInfo gameInfo { get; private set; }
    }
}

