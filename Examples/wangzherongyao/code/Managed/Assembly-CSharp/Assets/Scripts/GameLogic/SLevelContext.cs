namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Framework;
    using CSProtocol;
    using ResData;
    using System;
    using System.Runtime.CompilerServices;

    public class SLevelContext
    {
        private uint _dynamicDifficulty;
        public RES_LEVEL_HEROAITYPE AIModeType;
        public string ambientSoundEvent;
        public string bankResName;
        public int baseReviveTime = 0x3a98;
        public uint[] battleTaskOfCamps = new uint[3];
        public bool bBattleEquipLimit;
        public bool bCameraFlip;
        public bool bChaosPickRule;
        public int bigMapHeight;
        public string bigMapPath;
        public int bigMapWidth;
        public int BirthLevelConfig;
        public byte bLevelNo;
        public bool bShowTrainingHelper;
        public byte bSoulGrow;
        public bool canAutoAI;
        public int difficulty;
        public uint dwAttackOrder;
        public uint dynamicPropertyConfig;
        public int FailureDialogId;
        public COM_GAME_TYPE GameType;
        public int HeadPtsUpperLimit;
        public Horizon.EnableMethod horizonEnableMethod;
        public int iChapterNo;
        public int iLevelID;
        public bool isPVPLevel;
        public bool isPVPMode;
        public bool isWarmBattle;
        public string LevelArtistFileName;
        public string LevelDesignFileName;
        public string LevelName;
        public RES_LEVEL_TYPE LevelType;
        public string m_SecondName = string.Empty;
        public ResDT_MapBuff[] MapBuffs;
        public int mapHeight;
        public string mapPath;
        public int MapType = -1;
        public int mapWidth;
        public string miniMapPath;
        public string musicEndEvent;
        public string musicStartEvent;
        public int PassDialogId;
        public int PreDialogId;
        public int pvpLevelPlayerNum;
        public int recommendCombatEft;
        public bool showMiniMap = true;
        public int SubMapType = -1;

        public void Init(ResLevelCfgInfo levelCfg, int difficult)
        {
            this.LevelName = StringHelper.UTF8BytesToString(ref levelCfg.szName);
            this.LevelDesignFileName = StringHelper.UTF8BytesToString(ref levelCfg.szDesignFileName);
            if ((levelCfg.szArtistFileName != null) && (levelCfg.szArtistFileName.Length > 0))
            {
                this.LevelArtistFileName = StringHelper.UTF8BytesToString(ref levelCfg.szArtistFileName);
            }
            this.iLevelID = levelCfg.iCfgID;
            this.iChapterNo = levelCfg.iChapterId;
            this.bLevelNo = levelCfg.bLevelNo;
            this.difficulty = difficult;
            this.LevelType = (RES_LEVEL_TYPE) levelCfg.iLevelType;
            if (this.LevelType == RES_LEVEL_TYPE.RES_LEVEL_TYPE_GUIDE)
            {
                this.GameType = COM_GAME_TYPE.COM_SINGLE_GAME_OF_GUIDE;
            }
            this.PassDialogId = levelCfg.iPassDialogId;
            this.PreDialogId = levelCfg.iPreDialogId;
            this.FailureDialogId = levelCfg.iFailureDialogId;
            this.AIModeType = (RES_LEVEL_HEROAITYPE) levelCfg.iHeroAIType;
            this.bSoulGrow = levelCfg.bSoulGrow;
            this.dwAttackOrder = levelCfg.dwAttackOrderID;
            this.baseReviveTime = (int) levelCfg.dwReviveTime;
            this.dynamicPropertyConfig = levelCfg.dwDynamicPropertyCfg;
            this.isPVPMode = this.LevelType == RES_LEVEL_TYPE.RES_LEVEL_TYPE_PVP;
            this.miniMapPath = StringHelper.UTF8BytesToString(ref levelCfg.szThumbnailPath);
            this.bigMapPath = StringHelper.UTF8BytesToString(ref levelCfg.szBigMapPath);
            this.mapPath = StringHelper.UTF8BytesToString(ref levelCfg.szMapPath);
            this.mapWidth = levelCfg.iMapWidth;
            this.mapHeight = levelCfg.iMapHeight;
            this.bigMapWidth = levelCfg.iBigMapWidth;
            this.bigMapHeight = levelCfg.iBigMapHeight;
            this.musicStartEvent = StringHelper.UTF8BytesToString(ref levelCfg.szMusicStartEvent);
            this.musicEndEvent = StringHelper.UTF8BytesToString(ref levelCfg.szMusicEndEvent);
            this.ambientSoundEvent = StringHelper.UTF8BytesToString(ref levelCfg.szAmbientSoundEvent);
            this.bankResName = StringHelper.UTF8BytesToString(ref levelCfg.szBankResourceName);
            this.horizonEnableMethod = (Horizon.EnableMethod) levelCfg.bEnableHorizon;
            if (this.isPVPMode)
            {
                this.horizonEnableMethod = Horizon.EnableMethod.EnableAll;
            }
            this.canAutoAI = levelCfg.bIsOpenAutoAI != 0;
            this.MapBuffs = levelCfg.astMapBuffs;
            if ((difficult >= 1) && (difficult < (levelCfg.RecommendPower.Length + 1)))
            {
                this.recommendCombatEft = levelCfg.RecommendPower[difficult - 1];
            }
            this.bCameraFlip = false;
        }

        public bool IsPvpGameType()
        {
            return (((((this.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_MATCH) || (this.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_ROOM)) || ((this.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER) || (this.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_ENTERTAINMENT))) || (this.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_REWARDMATCH)) || this.isWarmBattle);
        }

        public uint DynamicDifficulty
        {
            get
            {
                return this._dynamicDifficulty;
            }
            set
            {
                this._dynamicDifficulty = value;
                this.DynamicDifficultyCfg = GameDataMgr.battleDynamicDifficultyDB.GetDataByKey(this._dynamicDifficulty);
            }
        }

        public ResBattleDynamicDifficulty DynamicDifficultyCfg { get; private set; }
    }
}

