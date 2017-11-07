namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Framework;
    using System;

    public abstract class GameContextBase : IGameContext
    {
        protected SLevelContext LevelContext;
        protected int RewardCount;

        protected GameContextBase()
        {
        }

        public virtual IGameInfo CreateGame()
        {
            return null;
        }

        public virtual bool IsBalanceProp()
        {
            return this.levelContext.isPVPLevel;
        }

        public virtual bool IsFireHole()
        {
            return (((this.LevelContext != null) && (this.LevelContext.MapType == 4)) && (this.LevelContext.SubMapType == 2));
        }

        public virtual bool IsPureBalanceProp()
        {
            int dwConfValue = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x29).dwConfValue;
            return (this.levelContext.isPVPLevel && (dwConfValue > 0));
        }

        public virtual bool IsSoulGrow()
        {
            return (this.levelContext.isPVPLevel || (this.LevelContext.bSoulGrow > 0));
        }

        public virtual void PrepareStartup()
        {
        }

        public virtual string levelArtistFileName
        {
            get
            {
                return string.Empty;
            }
        }

        public SLevelContext levelContext
        {
            get
            {
                return this.LevelContext;
            }
        }

        public virtual string levelDesignFileName
        {
            get
            {
                return string.Empty;
            }
        }

        public int rewardCount
        {
            get
            {
                return this.RewardCount;
            }
        }
    }
}

