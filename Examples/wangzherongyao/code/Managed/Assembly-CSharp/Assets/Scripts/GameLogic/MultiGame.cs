namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.GameKernal;
    using Assets.Scripts.GameSystem;
    using CSProtocol;
    using System;
    using System.Collections.Generic;

    public abstract class MultiGame : BasicGameInfo
    {
        protected MultiGame()
        {
        }

        public override void EndGame()
        {
            Singleton<WinLoseByStarSys>.instance.OnEvaluationChanged -= new OnEvaluationChangedDelegate(BattleLogic.OnWinStarSysChanged);
            Singleton<WinLoseByStarSys>.instance.OnFailureEvaluationChanged -= new OnEvaluationChangedDelegate(BattleLogic.OnLoseStarSysChanged);
            Singleton<WinLoseByStarSys>.instance.Clear();
            base.EndGame();
        }

        public override void OnLoadingProgress(float Progress)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x43b);
            msg.stPkgData.stMultGameLoadProcessReq.wProcess = Convert.ToUInt16((float) (Progress * 100f));
            Singleton<NetworkModule>.GetInstance().SendGameMsg(ref msg, 0);
            CUILoadingSystem.OnSelfLoadProcess(Progress);
        }

        public override void PreBeginPlay()
        {
            base.PreBeginPlay();
            this.PreparePlayer();
            this.ResetSynchrConfig();
            this.LoadAllTeamActors();
        }

        protected virtual void PreparePlayer()
        {
            MultiGameContext gameContext = base.GameContext as MultiGameContext;
            DebugHelper.Assert(gameContext != null);
            if (gameContext != null)
            {
                if (Singleton<GamePlayerCenter>.instance.GetAllPlayers().Count > 0)
                {
                }
                Singleton<GamePlayerCenter>.instance.ClearAllPlayers();
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < gameContext.MessageRef.astCampInfo[i].dwPlayerNum; j++)
                    {
                        uint dwObjId = gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.dwObjId;
                        Player player = Singleton<GamePlayerCenter>.GetInstance().GetPlayer(dwObjId);
                        DebugHelper.Assert(player == null, "你tm肯定在逗我");
                        bool isComputer = gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.bObjType == 2;
                        if (player == null)
                        {
                            ulong uid = 0L;
                            uint dwFakeLogicWorldID = 0;
                            uint dwFakePvpLevel = 1;
                            string openId = string.Empty;
                            uint vipLv = 0;
                            if (isComputer)
                            {
                                if (Convert.ToBoolean(gameContext.MessageRef.stDeskInfo.bIsWarmBattle))
                                {
                                    uid = gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.stDetail.stPlayerOfNpc.ullFakeUid;
                                    dwFakeLogicWorldID = gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.stDetail.stPlayerOfNpc.dwFakeLogicWorldID;
                                    dwFakePvpLevel = gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.stDetail.stPlayerOfNpc.dwFakePvpLevel;
                                    openId = string.Empty;
                                }
                                else
                                {
                                    uid = 0L;
                                    dwFakeLogicWorldID = 0;
                                    dwFakePvpLevel = gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.dwLevel;
                                    openId = string.Empty;
                                }
                            }
                            else
                            {
                                uid = gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.stDetail.stPlayerOfAcnt.ullUid;
                                dwFakeLogicWorldID = (uint) gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.stDetail.stPlayerOfAcnt.iLogicWorldID;
                                dwFakePvpLevel = gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.dwLevel;
                                openId = Utility.UTF8Convert(gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].szOpenID);
                                vipLv = gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.stDetail.stPlayerOfAcnt.stGameVip.dwCurLevel;
                            }
                            player = Singleton<GamePlayerCenter>.GetInstance().AddPlayer(dwObjId, (COM_PLAYERCAMP) (i + 1), gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.bPosOfCamp, gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.dwLevel, isComputer, Utility.UTF8Convert(gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.szName), 0, (int) dwFakeLogicWorldID, uid, vipLv, openId, gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].dwGradeOfRank);
                            DebugHelper.Assert(player != null, "创建player失败");
                            if (player != null)
                            {
                                player.isGM = gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].bIsGM > 0;
                            }
                        }
                        for (int k = 0; k < gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.astChoiceHero.Length; k++)
                        {
                            COMDT_CHOICEHERO comdt_choicehero = gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.astChoiceHero[k];
                            int dwHeroID = (int) comdt_choicehero.stBaseInfo.stCommonInfo.dwHeroID;
                            if (dwHeroID != 0)
                            {
                                bool flag2 = ((comdt_choicehero.stBaseInfo.stCommonInfo.dwMaskBits & 4) > 0) && ((comdt_choicehero.stBaseInfo.stCommonInfo.dwMaskBits & 1) == 0);
                                if ((gameContext.MessageRef.astCampInfo[i].astCampPlayerInfo[j].stPlayerInfo.bObjType != 1) || !flag2)
                                {
                                }
                                if (player != null)
                                {
                                    player.AddHero((uint) dwHeroID);
                                }
                            }
                        }
                    }
                }
                if (Singleton<GameReplayModule>.HasInstance() && Singleton<GameReplayModule>.instance.isReplay)
                {
                    List<Player> allPlayers = Singleton<GamePlayerCenter>.GetInstance().GetAllPlayers();
                    int num10 = (Singleton<GameReplayModule>.instance.playerIdx >= allPlayers.Count) ? (allPlayers.Count - 1) : Singleton<GameReplayModule>.instance.playerIdx;
                    Singleton<GamePlayerCenter>.GetInstance().SetHostPlayer(allPlayers[num10].PlayerId);
                }
                else
                {
                    Player playerByUid = Singleton<GamePlayerCenter>.GetInstance().GetPlayerByUid(Singleton<CRoleInfoManager>.instance.masterUUID);
                    DebugHelper.Assert(playerByUid != null, "load multi game but hostPlayer is null");
                    Singleton<GamePlayerCenter>.GetInstance().SetHostPlayer(playerByUid.PlayerId);
                }
                gameContext.levelContext.isWarmBattle = Convert.ToBoolean(gameContext.MessageRef.stDeskInfo.bIsWarmBattle);
                gameContext.SaveServerData();
            }
        }

        protected virtual void ResetSynchrConfig()
        {
            MultiGameContext gameContext = base.GameContext as MultiGameContext;
            DebugHelper.Assert(gameContext != null);
            Singleton<FrameSynchr>.GetInstance().SetSynchrConfig(gameContext.MessageRef.dwKFrapsFreqMs, gameContext.MessageRef.bKFrapsLater, gameContext.MessageRef.bPreActFrap, gameContext.MessageRef.dwRandomSeed);
        }

        public override void StartFight()
        {
            base.StartFight();
            if (Singleton<WinLoseByStarSys>.instance.Reset(base.GameContext.levelContext.iLevelID, true))
            {
                Singleton<WinLoseByStarSys>.instance.OnEvaluationChanged += new OnEvaluationChangedDelegate(BattleLogic.OnWinStarSysChanged);
                Singleton<WinLoseByStarSys>.instance.OnFailureEvaluationChanged += new OnEvaluationChangedDelegate(BattleLogic.OnLoseStarSysChanged);
                Singleton<WinLoseByStarSys>.instance.Start();
            }
        }
    }
}

