namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.DataCenter;
    using Assets.Scripts.GameLogic.GameKernal;
    using System;
    using System.Collections.Generic;

    public abstract class BasicGameInfo : IGameInfo
    {
        protected IGameContext GameContext;

        protected BasicGameInfo()
        {
        }

        public virtual void EndGame()
        {
        }

        public virtual bool Initialize(IGameContext InGameContext)
        {
            DebugHelper.Assert(InGameContext != null);
            this.GameContext = InGameContext;
            return (this.GameContext != null);
        }

        protected virtual void LoadAllTeamActors()
        {
            List<Player>.Enumerator enumerator = Singleton<GamePlayerCenter>.instance.GetAllPlayers().GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != null)
                {
                    ReadonlyContext<uint> allHeroIds = enumerator.Current.GetAllHeroIds();
                    for (int i = 0; i < allHeroIds.Count; i++)
                    {
                        ActorMeta actorMeta = new ActorMeta {
                            ActorCamp = enumerator.Current.PlayerCamp,
                            ConfigId = allHeroIds[i],
                            PlayerId = enumerator.Current.PlayerId
                        };
                        MonoSingleton<GameLoader>.instance.AddActor(ref actorMeta);
                    }
                }
            }
        }

        private void OnHideLoading(int timersequence)
        {
            Singleton<CUILoadingSystem>.instance.HideLoading();
            if (timersequence != -1)
            {
                Singleton<CTimerManager>.GetInstance().RemoveTimer(timersequence);
            }
        }

        public virtual void OnLoadingProgress(float Progress)
        {
        }

        public virtual void PostBeginPlay()
        {
            if (!Singleton<LobbyLogic>.instance.inMultiGame)
            {
                Singleton<FrameSynchr>.GetInstance().ResetSynchr();
                bool bDialogTriggerStart = false;
                SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
                Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
                if (((curLvelContext != null) && (curLvelContext.PreDialogId > 0)) && ((hostPlayer != null) && (hostPlayer.Captain != 0)))
                {
                    bDialogTriggerStart = true;
                    MonoSingleton<DialogueProcessor>.instance.PlayDrama(curLvelContext.PreDialogId, hostPlayer.Captain.handle.gameObject, hostPlayer.Captain.handle.gameObject, bDialogTriggerStart);
                }
                if (!bDialogTriggerStart)
                {
                    Singleton<BattleLogic>.GetInstance().DoBattleStart();
                }
                else
                {
                    Singleton<CTimerManager>.GetInstance().AddTimer(100, 1, new CTimer.OnTimeUpHandler(this.OnHideLoading), false);
                    Singleton<BattleLogic>.GetInstance().BindFightPrepareFinListener();
                }
            }
            else
            {
                GameReplayModule instance = Singleton<GameReplayModule>.GetInstance();
                if (instance.isReplay)
                {
                    instance.OnGameLoadComplete();
                }
                else
                {
                    Singleton<LobbyLogic>.GetInstance().ReqStartMultiGame();
                }
            }
            SoldierRegion.bFirstSpawnEvent = true;
        }

        public virtual void PreBeginPlay()
        {
        }

        public virtual void ReduceDamage(ref HurtDataInfo HurtInfo)
        {
        }

        public virtual void StartFight()
        {
        }

        public IGameContext gameContext
        {
            get
            {
                return this.GameContext;
            }
        }
    }
}

