namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using CSProtocol;
    using ResData;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class CLevelCfgLogicManager
    {
        public static ResDT_LevelCommonInfo FindLevelConfigMultiGame(int levelId)
        {
            ResDT_LevelCommonInfo stLevelCommonInfo = null;
            SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
            if (curLvelContext != null)
            {
                if (((curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_MATCH) || (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_COMBAT)) || ((curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_ROOM) || (curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_ENTERTAINMENT)))
                {
                    ResAcntBattleLevelInfo dataByKey = GameDataMgr.pvpLevelDatabin.GetDataByKey(levelId);
                    ResCounterPartLevelInfo info4 = null;
                    if (dataByKey == null)
                    {
                        info4 = GameDataMgr.cpLevelDatabin.GetDataByKey(levelId);
                        object[] inParameters = new object[] { levelId };
                        DebugHelper.Assert(info4 != null, "Failed find counterpart level config for id {0}", inParameters);
                        return info4.stLevelCommonInfo;
                    }
                    return dataByKey.stLevelCommonInfo;
                }
                if (curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER)
                {
                    ResRankLevelInfo info5 = GameDataMgr.rankLevelDatabin.GetDataByKey(levelId);
                    if (info5 != null)
                    {
                        stLevelCommonInfo = info5.stLevelCommonInfo;
                    }
                    return stLevelCommonInfo;
                }
                if (curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_REWARDMATCH)
                {
                    ResRewardMatchLevelInfo info6 = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey(levelId);
                    if (info6 != null)
                    {
                        stLevelCommonInfo = info6.stLevelCommonInfo;
                    }
                    return stLevelCommonInfo;
                }
                if (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_GUIDE)
                {
                    ResLevelCfgInfo info7 = GameDataMgr.levelDatabin.GetDataByKey(levelId);
                    stLevelCommonInfo = new ResDT_LevelCommonInfo {
                        bMaxAcntNum = info7.bMaxAcntNum
                    };
                }
            }
            return stLevelCommonInfo;
        }

        public static void FindLevelConfigSingleGame(int levelId, out ResLevelCfgInfo outLevelCfg, out ResDT_LevelCommonInfo outLevelComInfo)
        {
            outLevelCfg = null;
            outLevelComInfo = null;
            SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
            if (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_COMBAT)
            {
                ResAcntBattleLevelInfo dataByKey = GameDataMgr.pvpLevelDatabin.GetDataByKey(levelId);
                ResDT_LevelCommonInfo stLevelCommonInfo = null;
                ResCounterPartLevelInfo info3 = null;
                if (dataByKey == null)
                {
                    info3 = GameDataMgr.cpLevelDatabin.GetDataByKey(levelId);
                    object[] inParameters = new object[] { levelId };
                    DebugHelper.Assert(info3 != null, "Failed find counterpart level config for id {0}", inParameters);
                    stLevelCommonInfo = info3.stLevelCommonInfo;
                }
                else
                {
                    stLevelCommonInfo = dataByKey.stLevelCommonInfo;
                }
                outLevelComInfo = stLevelCommonInfo;
            }
            else if (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_ARENA)
            {
                ResLevelCfgInfo info4 = GameDataMgr.arenaLevelDatabin.GetDataByKey(levelId);
                outLevelCfg = info4;
            }
            else if (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_BURNING)
            {
                ResLevelCfgInfo info5 = GameDataMgr.burnMap.GetDataByKey(levelId);
                outLevelCfg = info5;
            }
            else
            {
                ResLevelCfgInfo info6 = GameDataMgr.levelDatabin.GetDataByKey(levelId);
                outLevelCfg = info6;
            }
        }

        public static uint GetLevelIncomeRuleID(SLevelContext _levelContext, IncomeControl inControl)
        {
            inControl.m_isExpCompensate = false;
            inControl.m_originalGoldCoinInBattle = 0;
            inControl.m_compensateRateList.Clear();
            uint dwSoulID = 0;
            if ((_levelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_ADVENTURE) || (_levelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_GUIDE))
            {
                ResLevelCfgInfo dataByKey = GameDataMgr.levelDatabin.GetDataByKey(_levelContext.iLevelID);
                return ((dataByKey == null) ? 0 : dataByKey.dwSoulID);
            }
            if (((_levelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_COMBAT) || (_levelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_MATCH)) || ((_levelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_ROOM) || (_levelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_ENTERTAINMENT)))
            {
                ResAcntBattleLevelInfo info2 = GameDataMgr.pvpLevelDatabin.GetDataByKey((uint) _levelContext.iLevelID);
                ResCounterPartLevelInfo info3 = null;
                if (info2 != null)
                {
                    dwSoulID = info2.stLevelCommonInfo.dwSoulID;
                    inControl.InitExpCompensateInfo(info2.stLevelCommonInfo.bIsOpenExpCompensate, ref info2.stLevelCommonInfo.astExpCompensateDetail);
                    inControl.m_originalGoldCoinInBattle = info2.wOriginalGoldCoinInBattle;
                    return dwSoulID;
                }
                info3 = GameDataMgr.cpLevelDatabin.GetDataByKey((uint) _levelContext.iLevelID);
                if (info3 != null)
                {
                    dwSoulID = info3.stLevelCommonInfo.dwSoulID;
                    inControl.InitExpCompensateInfo(info3.stLevelCommonInfo.bIsOpenExpCompensate, ref info3.stLevelCommonInfo.astExpCompensateDetail);
                    inControl.m_originalGoldCoinInBattle = info3.wOriginalGoldCoinInBattle;
                }
                return dwSoulID;
            }
            if (_levelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER)
            {
                ResRankLevelInfo info4 = GameDataMgr.rankLevelDatabin.GetDataByKey((uint) _levelContext.iLevelID);
                dwSoulID = (info4 == null) ? 0 : info4.stLevelCommonInfo.dwSoulID;
                if (info4 != null)
                {
                    inControl.InitExpCompensateInfo(info4.stLevelCommonInfo.bIsOpenExpCompensate, ref info4.stLevelCommonInfo.astExpCompensateDetail);
                    inControl.m_originalGoldCoinInBattle = info4.wOriginalGoldCoinInBattle;
                }
                return dwSoulID;
            }
            if (_levelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_REWARDMATCH)
            {
                ResRewardMatchLevelInfo info5 = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey((uint) _levelContext.iLevelID);
                dwSoulID = (info5 == null) ? 0 : info5.stLevelCommonInfo.dwSoulID;
                if (info5 != null)
                {
                    inControl.InitExpCompensateInfo(info5.stLevelCommonInfo.bIsOpenExpCompensate, ref info5.stLevelCommonInfo.astExpCompensateDetail);
                    inControl.m_originalGoldCoinInBattle = info5.wOriginalGoldCoinInBattle;
                }
                return dwSoulID;
            }
            if (_levelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_ACTIVITY)
            {
                ResLevelCfgInfo info6 = GameDataMgr.activityLevelDatabin.GetDataByKey(_levelContext.iLevelID);
                return ((info6 == null) ? 0 : info6.dwSoulID);
            }
            if (_levelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_BURNING)
            {
                ResLevelCfgInfo info7 = GameDataMgr.burnMap.GetDataByKey(_levelContext.iLevelID);
                return ((info7 == null) ? 0 : info7.dwSoulID);
            }
            if (_levelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_ARENA)
            {
                ResLevelCfgInfo info8 = GameDataMgr.burnMap.GetDataByKey(_levelContext.iLevelID);
                dwSoulID = (info8 == null) ? 0 : info8.dwSoulID;
            }
            return dwSoulID;
        }

        public static int GetMapPlayerNum(uint MapId, COM_BATTLE_MAP_TYPE MapType)
        {
            if ((MapType == COM_BATTLE_MAP_TYPE.COM_BATTLE_MAP_TYPE_VERSUS) || (MapType == COM_BATTLE_MAP_TYPE.COM_BATTLE_MAP_TYPE_ENTERTAINMENT))
            {
                ResAcntBattleLevelInfo dataByKey = GameDataMgr.pvpLevelDatabin.GetDataByKey(MapId);
                object[] inParameters = new object[] { MapId };
                DebugHelper.Assert(dataByKey != null, "can't find map by id = {0}", inParameters);
                return ((dataByKey == null) ? 0 : dataByKey.stLevelCommonInfo.bMaxAcntNum);
            }
            if (MapType == COM_BATTLE_MAP_TYPE.COM_BATTLE_MAP_TYPE_RANK)
            {
                ResRankLevelInfo info2 = GameDataMgr.rankLevelDatabin.GetDataByKey(MapId);
                object[] objArray2 = new object[] { MapId };
                DebugHelper.Assert(info2 != null, "can't find map by id = {0}", objArray2);
                return ((info2 == null) ? 0 : info2.stLevelCommonInfo.bMaxAcntNum);
            }
            if (MapType == COM_BATTLE_MAP_TYPE.COM_BATTLE_MAP_TYPE_REWARDMATCH)
            {
                ResRewardMatchLevelInfo info3 = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey(MapId);
                object[] objArray3 = new object[] { MapId };
                DebugHelper.Assert(info3 != null, "can't find map by id = {0}", objArray3);
                return ((info3 == null) ? 0 : info3.stLevelCommonInfo.bMaxAcntNum);
            }
            return 0;
        }

        public static uint GetRankBattleMapID()
        {
            return GameDataMgr.rankLevelDatabin.GetAnyData().dwMapId;
        }

        public static void InitMapDataInfo()
        {
            Assets.Scripts.GameSystem.RoomInfo roomInfo = Singleton<CHeroSelectSystem>.GetInstance().m_roomInfo;
            ResDT_LevelCommonInfo mapData = Singleton<CHeroSelectSystem>.GetInstance().m_mapData;
            DebugHelper.Assert(roomInfo != null);
            uint dwMapId = roomInfo.roomAttrib.dwMapId;
            if (roomInfo.roomAttrib.bMapType == 1)
            {
                ResAcntBattleLevelInfo dataByKey = GameDataMgr.pvpLevelDatabin.GetDataByKey(dwMapId);
                DebugHelper.Assert(dataByKey != null);
                mapData.szName = dataByKey.stLevelCommonInfo.szName;
                mapData.szDesignFileName = dataByKey.stLevelCommonInfo.szDesignFileName;
                mapData.szArtistFileName = dataByKey.stLevelCommonInfo.szArtistFileName;
                mapData.szThumbnailPath = dataByKey.stLevelCommonInfo.szThumbnailPath;
                mapData.bMaxAcntNum = dataByKey.stLevelCommonInfo.bMaxAcntNum;
                mapData.bValidRoomType = dataByKey.stLevelCommonInfo.bValidRoomType;
                mapData.bHeroNum = dataByKey.stLevelCommonInfo.bHeroNum;
                mapData.bIsAllowHeroDup = dataByKey.stLevelCommonInfo.bIsAllowHeroDup;
                mapData.dwHeroFormId = dataByKey.stLevelCommonInfo.dwHeroFormId;
                mapData.iHeroAIType = dataByKey.stLevelCommonInfo.iHeroAIType;
            }
            else if (roomInfo.roomAttrib.bMapType == 3)
            {
                ResRankLevelInfo info4 = GameDataMgr.rankLevelDatabin.GetDataByKey(dwMapId);
                DebugHelper.Assert(info4 != null);
                mapData.szName = info4.stLevelCommonInfo.szName;
                mapData.szDesignFileName = info4.stLevelCommonInfo.szDesignFileName;
                mapData.szArtistFileName = info4.stLevelCommonInfo.szArtistFileName;
                mapData.szThumbnailPath = info4.stLevelCommonInfo.szThumbnailPath;
                mapData.bMaxAcntNum = info4.stLevelCommonInfo.bMaxAcntNum;
                mapData.bValidRoomType = info4.stLevelCommonInfo.bValidRoomType;
                mapData.bHeroNum = info4.stLevelCommonInfo.bHeroNum;
                mapData.bIsAllowHeroDup = info4.stLevelCommonInfo.bIsAllowHeroDup;
                mapData.dwHeroFormId = info4.stLevelCommonInfo.dwHeroFormId;
                mapData.iHeroAIType = info4.stLevelCommonInfo.iHeroAIType;
            }
            else if (roomInfo.roomAttrib.bMapType == 2)
            {
                ResCounterPartLevelInfo info5 = GameDataMgr.cpLevelDatabin.GetDataByKey(dwMapId);
                DebugHelper.Assert(info5 != null);
                mapData.szName = info5.stLevelCommonInfo.szName;
                mapData.szDesignFileName = info5.stLevelCommonInfo.szDesignFileName;
                mapData.szArtistFileName = info5.stLevelCommonInfo.szArtistFileName;
                mapData.szThumbnailPath = info5.stLevelCommonInfo.szThumbnailPath;
                mapData.bMaxAcntNum = info5.stLevelCommonInfo.bMaxAcntNum;
                mapData.bValidRoomType = info5.stLevelCommonInfo.bValidRoomType;
                mapData.bHeroNum = info5.stLevelCommonInfo.bHeroNum;
                mapData.bIsAllowHeroDup = info5.stLevelCommonInfo.bIsAllowHeroDup;
                mapData.dwHeroFormId = info5.stLevelCommonInfo.dwHeroFormId;
                mapData.iHeroAIType = info5.stLevelCommonInfo.iHeroAIType;
            }
            else if (roomInfo.roomAttrib.bMapType == 4)
            {
                ResAcntBattleLevelInfo info6 = GameDataMgr.pvpLevelDatabin.GetDataByKey(dwMapId);
                DebugHelper.Assert(info6 != null);
                mapData.szName = info6.stLevelCommonInfo.szName;
                mapData.szDesignFileName = info6.stLevelCommonInfo.szDesignFileName;
                mapData.szArtistFileName = info6.stLevelCommonInfo.szArtistFileName;
                mapData.szThumbnailPath = info6.stLevelCommonInfo.szThumbnailPath;
                mapData.bMaxAcntNum = info6.stLevelCommonInfo.bMaxAcntNum;
                mapData.bValidRoomType = info6.stLevelCommonInfo.bValidRoomType;
                mapData.bHeroNum = info6.stLevelCommonInfo.bHeroNum;
                mapData.bIsAllowHeroDup = info6.stLevelCommonInfo.bIsAllowHeroDup;
                mapData.dwHeroFormId = info6.stLevelCommonInfo.dwHeroFormId;
                mapData.iHeroAIType = info6.stLevelCommonInfo.iHeroAIType;
            }
            else if (roomInfo.roomAttrib.bMapType == 5)
            {
                ResRewardMatchLevelInfo info7 = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey(dwMapId);
                DebugHelper.Assert(info7 != null);
                mapData.szName = info7.stLevelCommonInfo.szName;
                mapData.szDesignFileName = info7.stLevelCommonInfo.szDesignFileName;
                mapData.szArtistFileName = info7.stLevelCommonInfo.szArtistFileName;
                mapData.szThumbnailPath = info7.stLevelCommonInfo.szThumbnailPath;
                mapData.bMaxAcntNum = info7.stLevelCommonInfo.bMaxAcntNum;
                mapData.bValidRoomType = info7.stLevelCommonInfo.bValidRoomType;
                mapData.bHeroNum = info7.stLevelCommonInfo.bHeroNum;
                mapData.bIsAllowHeroDup = info7.stLevelCommonInfo.bIsAllowHeroDup;
                mapData.dwHeroFormId = info7.stLevelCommonInfo.dwHeroFormId;
                mapData.iHeroAIType = info7.stLevelCommonInfo.iHeroAIType;
                Singleton<CHeroSelectSystem>.GetInstance().m_mapSubType = (int) info7.dwSubType;
            }
            CSDT_SINGLE_GAME_OF_COMBAT reportInfo = new CSDT_SINGLE_GAME_OF_COMBAT {
                bRoomType = (byte) Singleton<CHeroSelectSystem>.GetInstance().m_roomType,
                dwMapId = roomInfo.roomAttrib.dwMapId,
                bMapType = roomInfo.roomAttrib.bMapType,
                bAILevel = roomInfo.roomAttrib.npcAILevel
            };
            byte bHeroNum = mapData.bHeroNum;
            uint dwHeroFormId = mapData.dwHeroFormId;
            Singleton<CHeroSelectSystem>.GetInstance().SetPVEDataWithCombat(bHeroNum, dwHeroFormId, reportInfo, "Room Type");
        }

        public static bool IsLuanDouRuleWithUnion(uint mapID)
        {
            ResRewardMatchLevelInfo dataByKey = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey(mapID);
            if (dataByKey == null)
            {
                return false;
            }
            return (dataByKey.dwSubType == 2);
        }

        public static SLevelContext MakeMobaContext(uint LevelId, COM_GAME_TYPE GameType, int difficult)
        {
            SLevelContext context = new SLevelContext {
                isPVPLevel = true,
                GameType = GameType,
                isPVPMode = true,
                difficulty = difficult,
                horizonEnableMethod = Horizon.EnableMethod.EnableAll
            };
            if (GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_ENTERTAINMENT)
            {
                context.MapType = 4;
            }
            if ((GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_REWARDMATCH) && IsLuanDouRuleWithUnion(LevelId))
            {
                context.MapType = 4;
            }
            if (((GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_MATCH) || (GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_COMBAT)) || ((GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_ROOM) || (GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_ENTERTAINMENT)))
            {
                ResAcntBattleLevelInfo info = GameDataMgr.pvpLevelDatabin.GetDataByKey(LevelId);
                ResDT_LevelCommonInfo stLevelCommonInfo = null;
                ResCounterPartLevelInfo info3 = null;
                if (info == null)
                {
                    info3 = GameDataMgr.cpLevelDatabin.GetDataByKey(LevelId);
                    object[] inParameters = new object[] { LevelId };
                    DebugHelper.Assert(info3 != null, "Failed find counterpart level config for id {0}", inParameters);
                    stLevelCommonInfo = info3.stLevelCommonInfo;
                }
                else
                {
                    stLevelCommonInfo = info.stLevelCommonInfo;
                }
                context.LevelName = StringHelper.UTF8BytesToString(ref stLevelCommonInfo.szName);
                context.LevelDesignFileName = StringHelper.UTF8BytesToString(ref stLevelCommonInfo.szDesignFileName);
                if (stLevelCommonInfo.szArtistFileName != null)
                {
                    context.LevelArtistFileName = StringHelper.UTF8BytesToString(ref stLevelCommonInfo.szArtistFileName);
                }
                context.mapWidth = stLevelCommonInfo.iMapWidth;
                context.mapHeight = stLevelCommonInfo.iMapHeight;
                context.bigMapWidth = stLevelCommonInfo.iBigMapWidth;
                context.bigMapHeight = stLevelCommonInfo.iBigMapHeight;
                context.miniMapPath = stLevelCommonInfo.szThumbnailPath;
                context.bigMapPath = stLevelCommonInfo.szBigMapPath;
                context.mapPath = StringHelper.UTF8BytesToString(ref stLevelCommonInfo.szMapPath);
                context.AIModeType = (RES_LEVEL_HEROAITYPE) stLevelCommonInfo.iHeroAIType;
                if (info == null)
                {
                    context.iLevelID = (int) info3.dwMapId;
                    context.dwAttackOrder = info3.dwAttackOrderID;
                    context.dynamicPropertyConfig = info3.dwDynamicPropertyCfg;
                    context.battleTaskOfCamps[1] = info3.dwBattleTaskOfCamp1;
                    context.battleTaskOfCamps[2] = info3.dwBattleTaskOfCamp2;
                    context.musicStartEvent = StringHelper.UTF8BytesToString(ref info3.szMusicStartEvent);
                    context.musicEndEvent = StringHelper.UTF8BytesToString(ref info3.szMusicEndEvent);
                    context.bankResName = StringHelper.UTF8BytesToString(ref info3.szBankResourceName);
                    context.ambientSoundEvent = StringHelper.UTF8BytesToString(ref info3.szAmbientSoundEvent);
                }
                else
                {
                    context.iLevelID = (int) info.dwMapId;
                    context.dwAttackOrder = info.dwAttackOrderID;
                    context.dynamicPropertyConfig = info.dwDynamicPropertyCfg;
                    context.battleTaskOfCamps[1] = info.dwBattleTaskOfCamp1;
                    context.battleTaskOfCamps[2] = info.dwBattleTaskOfCamp2;
                    context.musicStartEvent = StringHelper.UTF8BytesToString(ref info.szMusicStartEvent);
                    context.musicEndEvent = StringHelper.UTF8BytesToString(ref info.szMusicEndEvent);
                    context.bankResName = StringHelper.UTF8BytesToString(ref info.szBankResourceName);
                    context.ambientSoundEvent = StringHelper.UTF8BytesToString(ref info.szAmbientSoundEvent);
                    context.pvpLevelPlayerNum = info.stLevelCommonInfo.bMaxAcntNum;
                }
            }
            else if (GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER)
            {
                ResRankLevelInfo info4 = GameDataMgr.rankLevelDatabin.GetDataByKey(LevelId);
                DebugHelper.Assert(info4 != null);
                context.LevelName = StringHelper.UTF8BytesToString(ref info4.stLevelCommonInfo.szName);
                context.LevelDesignFileName = StringHelper.UTF8BytesToString(ref info4.stLevelCommonInfo.szDesignFileName);
                if (info4.stLevelCommonInfo.szArtistFileName != null)
                {
                    context.LevelArtistFileName = StringHelper.UTF8BytesToString(ref info4.stLevelCommonInfo.szArtistFileName);
                }
                context.iLevelID = (int) info4.dwMapId;
                context.dwAttackOrder = info4.dwAttackOrderID;
                context.dynamicPropertyConfig = info4.dwDynamicPropertyCfg;
                context.AIModeType = (RES_LEVEL_HEROAITYPE) info4.stLevelCommonInfo.iHeroAIType;
                context.mapWidth = info4.stLevelCommonInfo.iMapWidth;
                context.mapHeight = info4.stLevelCommonInfo.iMapHeight;
                context.bigMapWidth = info4.stLevelCommonInfo.iBigMapWidth;
                context.bigMapHeight = info4.stLevelCommonInfo.iBigMapHeight;
                context.miniMapPath = info4.stLevelCommonInfo.szThumbnailPath;
                context.bigMapPath = info4.stLevelCommonInfo.szBigMapPath;
                context.battleTaskOfCamps[1] = info4.dwBattleTaskOfCamp1;
                context.battleTaskOfCamps[2] = info4.dwBattleTaskOfCamp2;
                context.musicStartEvent = StringHelper.UTF8BytesToString(ref info4.szMusicStartEvent);
                context.musicEndEvent = StringHelper.UTF8BytesToString(ref info4.szMusicEndEvent);
                context.ambientSoundEvent = StringHelper.UTF8BytesToString(ref info4.szAmbientSoundEvent);
                context.bankResName = StringHelper.UTF8BytesToString(ref info4.szBankResourceName);
                context.pvpLevelPlayerNum = info4.stLevelCommonInfo.bMaxAcntNum;
            }
            else if (GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_REWARDMATCH)
            {
                ResRewardMatchLevelInfo info5 = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey(LevelId);
                DebugHelper.Assert(info5 != null);
                context.LevelName = StringHelper.UTF8BytesToString(ref info5.stLevelCommonInfo.szName);
                context.LevelDesignFileName = StringHelper.UTF8BytesToString(ref info5.stLevelCommonInfo.szDesignFileName);
                if (info5.stLevelCommonInfo.szArtistFileName != null)
                {
                    context.LevelArtistFileName = StringHelper.UTF8BytesToString(ref info5.stLevelCommonInfo.szArtistFileName);
                }
                context.iLevelID = (int) info5.dwMapId;
                context.dwAttackOrder = info5.dwAttackOrderID;
                context.dynamicPropertyConfig = info5.dwDynamicPropertyCfg;
                context.AIModeType = (RES_LEVEL_HEROAITYPE) info5.stLevelCommonInfo.iHeroAIType;
                context.mapWidth = info5.stLevelCommonInfo.iMapWidth;
                context.mapHeight = info5.stLevelCommonInfo.iMapHeight;
                context.bigMapWidth = info5.stLevelCommonInfo.iBigMapWidth;
                context.bigMapHeight = info5.stLevelCommonInfo.iBigMapHeight;
                context.miniMapPath = info5.stLevelCommonInfo.szThumbnailPath;
                context.bigMapPath = info5.stLevelCommonInfo.szBigMapPath;
                context.battleTaskOfCamps[1] = info5.dwBattleTaskOfCamp1;
                context.battleTaskOfCamps[2] = info5.dwBattleTaskOfCamp2;
                context.musicStartEvent = StringHelper.UTF8BytesToString(ref info5.szMusicStartEvent);
                context.musicEndEvent = StringHelper.UTF8BytesToString(ref info5.szMusicEndEvent);
                context.ambientSoundEvent = StringHelper.UTF8BytesToString(ref info5.szAmbientSoundEvent);
                context.bankResName = StringHelper.UTF8BytesToString(ref info5.szBankResourceName);
                context.pvpLevelPlayerNum = info5.stLevelCommonInfo.bMaxAcntNum;
                context.m_SecondName = info5.szMatchName;
            }
            ResEntertainmentLevelInfo dataByKey = GameDataMgr.entertainLevelDatabin.GetDataByKey(LevelId);
            if (dataByKey != null)
            {
                context.SubMapType = dataByKey.bEntertainmentSubType;
            }
            return context;
        }

        public static void SetMasterPvpDetailWhenGameSettle(COMDT_GAME_INFO gameInfo)
        {
            byte num5;
            byte bGameType = gameInfo.bGameType;
            byte bMapType = gameInfo.bMapType;
            uint iLevelID = (uint) gameInfo.iLevelID;
            byte bMaxAcntNum = 0;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo();
            DebugHelper.Assert(masterRoleInfo != null, "masterRoleInfo is null");
            if (masterRoleInfo == null)
            {
                return;
            }
            switch (((COM_GAME_TYPE) bGameType))
            {
                case COM_GAME_TYPE.COM_SINGLE_GAME_OF_COMBAT:
                    Singleton<CRoleInfoManager>.instance.CalculateWins(masterRoleInfo.pvpDetail.stVsMachineInfo, gameInfo.bGameResult);
                    return;

                case COM_GAME_TYPE.COM_SINGLE_GAME_OF_GUIDE:
                case COM_GAME_TYPE.COM_SINGLE_GAME_OF_ACTIVITY:
                case COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_ROOM:
                case COM_GAME_TYPE.COM_SINGLE_GAME_OF_BURNING:
                case COM_GAME_TYPE.COM_SINGLE_GAME_OF_ARENA:
                    return;

                case COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER:
                    Singleton<CRoleInfoManager>.instance.CalculateWins(masterRoleInfo.pvpDetail.stLadderInfo, gameInfo.bGameResult);
                    Singleton<CRoleInfoManager>.instance.CalculateKDA(gameInfo);
                    return;

                case COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_MATCH:
                case COM_GAME_TYPE.COM_MULTI_GAME_OF_REWARDMATCH:
                    if (gameInfo.bIsPKAI != 2)
                    {
                        switch (bMapType)
                        {
                            case 1:
                            case 4:
                            {
                                ResAcntBattleLevelInfo dataByKey = GameDataMgr.pvpLevelDatabin.GetDataByKey(iLevelID);
                                DebugHelper.Assert(dataByKey != null, "ResAcntBattleLevelInfo is null");
                                if (dataByKey == null)
                                {
                                    return;
                                }
                                bMaxAcntNum = dataByKey.stLevelCommonInfo.bMaxAcntNum;
                                goto Label_0192;
                            }
                            case 3:
                            {
                                ResRankLevelInfo info3 = GameDataMgr.rankLevelDatabin.GetDataByKey(iLevelID);
                                DebugHelper.Assert(info3 != null, "ResRankLevelInfo is null");
                                if (info3 == null)
                                {
                                    return;
                                }
                                bMaxAcntNum = info3.stLevelCommonInfo.bMaxAcntNum;
                                goto Label_0192;
                            }
                            case 5:
                            {
                                ResRewardMatchLevelInfo info4 = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey(iLevelID);
                                DebugHelper.Assert(info4 != null, "ResRankLevelInfo is null");
                                if (info4 == null)
                                {
                                    return;
                                }
                                bMaxAcntNum = info4.stLevelCommonInfo.bMaxAcntNum;
                                goto Label_0192;
                            }
                        }
                        break;
                    }
                    Singleton<CRoleInfoManager>.instance.CalculateWins(masterRoleInfo.pvpDetail.stVsMachineInfo, gameInfo.bGameResult);
                    return;

                case COM_GAME_TYPE.COM_MULTI_GAME_OF_ENTERTAINMENT:
                    if (gameInfo.bIsPKAI == 1)
                    {
                        Singleton<CRoleInfoManager>.instance.CalculateWins(masterRoleInfo.pvpDetail.stEntertainmentInfo, gameInfo.bGameResult);
                        Singleton<CRoleInfoManager>.instance.CalculateKDA(gameInfo);
                    }
                    return;

                default:
                    return;
            }
        Label_0192:
            num5 = bMaxAcntNum;
            switch (num5)
            {
                case 2:
                    Singleton<CRoleInfoManager>.instance.CalculateWins(masterRoleInfo.pvpDetail.stOneVsOneInfo, gameInfo.bGameResult);
                    break;

                case 4:
                    Singleton<CRoleInfoManager>.instance.CalculateWins(masterRoleInfo.pvpDetail.stTwoVsTwoInfo, gameInfo.bGameResult);
                    break;

                case 6:
                    Singleton<CRoleInfoManager>.instance.CalculateWins(masterRoleInfo.pvpDetail.stThreeVsThreeInfo, gameInfo.bGameResult);
                    break;

                default:
                    if (num5 == 10)
                    {
                        Singleton<CRoleInfoManager>.instance.CalculateWins(masterRoleInfo.pvpDetail.stFiveVsFiveInfo, gameInfo.bGameResult);
                    }
                    break;
            }
            Singleton<CRoleInfoManager>.instance.CalculateKDA(gameInfo);
        }

        public static void SetMultiGameContextLevelName(MultiGameContext mGameCon, uint mapId, COM_GAME_TYPE gameType)
        {
            if (gameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_REWARDMATCH)
            {
                ResRewardMatchLevelInfo dataByKey = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey(mapId);
                if (dataByKey != null)
                {
                    mGameCon.m_LevelDesignFileName = StringHelper.UTF8BytesToString(ref dataByKey.stLevelCommonInfo.szDesignFileName);
                    if (dataByKey.stLevelCommonInfo.szArtistFileName != null)
                    {
                        mGameCon.m_LevelArtistFileName = StringHelper.UTF8BytesToString(ref dataByKey.stLevelCommonInfo.szArtistFileName);
                    }
                }
            }
            else if (gameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER)
            {
                ResRankLevelInfo info2 = GameDataMgr.rankLevelDatabin.GetDataByKey(mapId);
                if (info2 != null)
                {
                    mGameCon.m_LevelDesignFileName = StringHelper.UTF8BytesToString(ref info2.stLevelCommonInfo.szDesignFileName);
                    if (info2.stLevelCommonInfo.szArtistFileName != null)
                    {
                        mGameCon.m_LevelArtistFileName = StringHelper.UTF8BytesToString(ref info2.stLevelCommonInfo.szArtistFileName);
                    }
                }
            }
            else
            {
                ResAcntBattleLevelInfo info3 = GameDataMgr.pvpLevelDatabin.GetDataByKey(mapId);
                if (info3 != null)
                {
                    mGameCon.m_LevelDesignFileName = StringHelper.UTF8BytesToString(ref info3.stLevelCommonInfo.szDesignFileName);
                    if (info3.stLevelCommonInfo.szArtistFileName != null)
                    {
                        mGameCon.m_LevelArtistFileName = StringHelper.UTF8BytesToString(ref info3.stLevelCommonInfo.szArtistFileName);
                    }
                }
            }
        }

        public static void SetPvpLevelInfo(GameObject root)
        {
            root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpLevelNode").gameObject.CustomSetActive(true);
            Text component = root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpLevelNode/GameType").GetComponent<Text>();
            Text text2 = root.transform.Find("PanelA/Award/Panel_PlayerExp/PvpLevelNode/PvpLevel").GetComponent<Text>();
            string text = Singleton<CTextManager>.instance.GetText("Battle_Settle_Game_Type_Single");
            string str2 = string.Empty;
            SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
            DebugHelper.Assert(curLvelContext != null, "Battle Level Context is NULL!!");
            uint iLevelID = (uint) curLvelContext.iLevelID;
            if (curLvelContext.isPVPLevel)
            {
                if (((curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_MATCH) || (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_COMBAT)) || ((curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_PVP_ROOM) || (curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_ENTERTAINMENT)))
                {
                    ResAcntBattleLevelInfo dataByKey = GameDataMgr.pvpLevelDatabin.GetDataByKey(iLevelID);
                    if (dataByKey == null)
                    {
                        ResCounterPartLevelInfo info2 = GameDataMgr.cpLevelDatabin.GetDataByKey(iLevelID);
                        object[] inParameters = new object[] { iLevelID };
                        DebugHelper.Assert(info2 != null, "Failed find counterpart level config for id {0}", inParameters);
                        str2 = Utility.UTF8Convert(info2.stLevelCommonInfo.szName);
                        text = Singleton<CTextManager>.instance.GetText(string.Format("Battle_Settle_Game_Type{0}", info2.stLevelCommonInfo.bMaxAcntNum / 2));
                    }
                    else
                    {
                        str2 = Utility.UTF8Convert(dataByKey.stLevelCommonInfo.szName);
                        text = Singleton<CTextManager>.instance.GetText(string.Format("Battle_Settle_Game_Type{0}", dataByKey.stLevelCommonInfo.bMaxAcntNum / 2));
                    }
                }
                else if (curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER)
                {
                    ResRankLevelInfo info3 = GameDataMgr.rankLevelDatabin.GetDataByKey(iLevelID);
                    DebugHelper.Assert(info3 != null);
                    str2 = Utility.UTF8Convert(info3.stLevelCommonInfo.szName);
                    text = Singleton<CTextManager>.instance.GetText(string.Format("Battle_Settle_Game_Type{0}", info3.stLevelCommonInfo.bMaxAcntNum / 2));
                }
            }
            else if (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_ARENA)
            {
                ResLevelCfgInfo info4 = GameDataMgr.arenaLevelDatabin.GetDataByKey((int) iLevelID);
                object[] objArray2 = new object[] { iLevelID };
                DebugHelper.Assert(info4 != null, "Failed find level config for id {0}", objArray2);
                str2 = Utility.UTF8Convert(info4.szName);
            }
            else if (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_COMBAT)
            {
                ResAcntBattleLevelInfo info5 = GameDataMgr.pvpLevelDatabin.GetDataByKey(iLevelID);
                object[] objArray3 = new object[] { iLevelID };
                DebugHelper.Assert(info5 != null, "Failed find level config for id {0}", objArray3);
                str2 = Utility.UTF8Convert(info5.stLevelCommonInfo.szName);
            }
            component.text = text;
            text2.text = str2;
        }

        public static void SetTeamData(GameObject root, TeamInfo data)
        {
            uint dwMapId = data.stTeamInfo.dwMapId;
            int bMapType = data.stTeamInfo.bMapType;
            int bMaxAcntNum = 0;
            switch (bMapType)
            {
                case 1:
                case 4:
                {
                    ResAcntBattleLevelInfo dataByKey = GameDataMgr.pvpLevelDatabin.GetDataByKey(dwMapId);
                    DebugHelper.Assert(dataByKey != null);
                    root.transform.Find("Panel_Main/MapInfo/txtMapName").gameObject.GetComponent<Text>().text = StringHelper.UTF8BytesToString(ref dataByKey.stLevelCommonInfo.szName);
                    bMaxAcntNum = dataByKey.stLevelCommonInfo.bMaxAcntNum;
                    break;
                }
                case 3:
                {
                    ResRankLevelInfo info2 = GameDataMgr.rankLevelDatabin.GetDataByKey(dwMapId);
                    DebugHelper.Assert(info2 != null);
                    root.transform.Find("Panel_Main/MapInfo/txtMapName").gameObject.GetComponent<Text>().text = StringHelper.UTF8BytesToString(ref info2.stLevelCommonInfo.szName);
                    bMaxAcntNum = 4;
                    break;
                }
                case 5:
                {
                    ResRewardMatchLevelInfo info3 = GameDataMgr.uinionBattleLevelDatabin.GetDataByKey(dwMapId);
                    DebugHelper.Assert(info3 != null);
                    root.transform.Find("Panel_Main/MapInfo/txtMapName").gameObject.GetComponent<Text>().text = StringHelper.UTF8BytesToString(ref info3.stLevelCommonInfo.szName);
                    bMaxAcntNum = info3.stLevelCommonInfo.bMaxAcntNum;
                    break;
                }
            }
            if (bMapType == 3)
            {
                root.transform.Find("Panel_Main/MapInfo/txtTeam").gameObject.GetComponent<Text>().text = Singleton<CTextManager>.instance.GetText("Common_Team_Player_Type_6");
            }
            else
            {
                root.transform.Find("Panel_Main/MapInfo/txtTeam").gameObject.GetComponent<Text>().text = Singleton<CTextManager>.instance.GetText(string.Format("Common_Team_Player_Type_{0}", bMaxAcntNum / 2));
            }
            CMatchingView.SetStartBtnStatus(root);
            GameObject gameObject = root.transform.Find("Panel_Main/Player1").gameObject;
            TeamMember memberInfo = CMatchingView.GetMemberInfo(data, 1);
            CMatchingView.SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 2);
            gameObject = root.transform.Find("Panel_Main/Player2").gameObject;
            memberInfo = CMatchingView.GetMemberInfo(data, 2);
            CMatchingView.SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 4);
            gameObject = root.transform.Find("Panel_Main/Player3").gameObject;
            memberInfo = CMatchingView.GetMemberInfo(data, 3);
            CMatchingView.SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 6);
            gameObject = root.transform.Find("Panel_Main/Player4").gameObject;
            memberInfo = CMatchingView.GetMemberInfo(data, 4);
            CMatchingView.SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 8);
            gameObject = root.transform.Find("Panel_Main/Player5").gameObject;
            memberInfo = CMatchingView.GetMemberInfo(data, 5);
            CMatchingView.SetPlayerSlotData(gameObject, memberInfo, bMaxAcntNum >= 10);
        }
    }
}

