namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.GameSystem;
    using CSProtocol;
    using System;
    using UnityEngine;

    public class MultiGameContext : GameContextBase
    {
        public string m_LevelArtistFileName;
        public string m_LevelDesignFileName;
        public SCPKG_MULTGAME_BEGINLOAD MessageRef;

        public MultiGameContext(SCPKG_MULTGAME_BEGINLOAD InMessage)
        {
            this.MessageRef = InMessage;
            uint dwMapId = this.MessageRef.stDeskInfo.dwMapId;
            base.LevelContext = CLevelCfgLogicManager.MakeMobaContext(dwMapId, (COM_GAME_TYPE) InMessage.bGameType, 1);
            base.LevelContext.isWarmBattle = Convert.ToBoolean(InMessage.stDeskInfo.bIsWarmBattle);
            base.LevelContext.DynamicDifficulty = InMessage.stDeskInfo.bAILevel;
            DebugHelper.Assert(base.LevelContext != null);
            CLevelCfgLogicManager.SetMultiGameContextLevelName(this, dwMapId, (COM_GAME_TYPE) InMessage.bGameType);
        }

        public override IGameInfo CreateGame()
        {
            PVPMobaGame game = new PVPMobaGame();
            game.Initialize(this);
            return game;
        }

        public void SaveServerData()
        {
            Singleton<ActorDataCenter>.instance.ClearHeroServerData();
            if (this.MessageRef != null)
            {
                for (int i = 0; i < this.MessageRef.astCampInfo.Length; i++)
                {
                    CSDT_CAMPINFO csdt_campinfo = this.MessageRef.astCampInfo[i];
                    int num2 = Mathf.Min(csdt_campinfo.astCampPlayerInfo.Length, (int) csdt_campinfo.dwPlayerNum);
                    for (int j = 0; j < num2; j++)
                    {
                        COMDT_PLAYERINFO stPlayerInfo = csdt_campinfo.astCampPlayerInfo[j].stPlayerInfo;
                        Singleton<ActorDataCenter>.instance.AddHeroesServerData(stPlayerInfo.dwObjId, stPlayerInfo.astChoiceHero);
                    }
                }
            }
        }

        public override string levelArtistFileName
        {
            get
            {
                return this.m_LevelArtistFileName;
            }
        }

        public override string levelDesignFileName
        {
            get
            {
                return this.m_LevelDesignFileName;
            }
        }
    }
}

