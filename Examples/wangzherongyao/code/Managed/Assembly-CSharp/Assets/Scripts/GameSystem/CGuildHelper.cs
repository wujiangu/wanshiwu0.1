namespace Assets.Scripts.GameSystem
{
    using Apollo;
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class CGuildHelper
    {
        public const int CrownStarConversionRatio = 0xd8;
        public const string DynamicPrefabCoinIconName = "90001";
        public const string DynamicPrefabDiamondIconName = "90005";
        public static readonly string DynamicPrefabPathCrown = (CUIUtility.s_Sprite_Dynamic_Guild_Dir + "Guild_Icon_crown");
        public static readonly string DynamicPrefabPathMoon = (CUIUtility.s_Sprite_Dynamic_Guild_Dir + "Guild_Icon_moon");
        public static readonly string DynamicPrefabPathStar = (CUIUtility.s_Sprite_Dynamic_Guild_Dir + "Guild_Icon_star");
        public static readonly string DynamicPrefabPathSun = (CUIUtility.s_Sprite_Dynamic_Guild_Dir + "Guild_Icon_sun");
        public const int MoonStarConversionRatio = 6;
        public const int SunStarConversionRatio = 0x24;

        public static RankpointRankInfo CreatePlayerGuildRankpointRankInfo()
        {
            return new RankpointRankInfo { guildId = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.stBriefInfo.uulUid, rankScore = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.RankInfo.totalRankPoint, guildHeadId = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.stBriefInfo.dwHeadId, guildName = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.stBriefInfo.sName, guildLevel = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.stBriefInfo.bLevel, rankNo = 0 };
        }

        public static uint GetAllBuildingMaintainMoney()
        {
            uint num = 0;
            List<GuildBuildingInfo> listBuildingInfo = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.listBuildingInfo;
            for (int i = 0; i < listBuildingInfo.Count; i++)
            {
                ResGuildBuilding dataByKey = GameDataMgr.guildBuildingDatabin.GetDataByKey((byte) listBuildingInfo[i].type);
                int level = listBuildingInfo[i].level;
                if (IsAffectedByGuildDegrade(listBuildingInfo[i].level))
                {
                    level--;
                }
                num += dataByKey.MaintainMoney[level - 1];
            }
            return num;
        }

        public static uint GetBagGuildSymbolIdByLevelOneSymbolId(uint levelOneSymbolId)
        {
            List<uint> list = new List<uint>();
            uint item = levelOneSymbolId;
            do
            {
                list.Add(item);
                item = GameDataMgr.symbolInfoDatabin.GetDataByKey(item).dwNextLvID;
            }
            while (item != 0);
            List<uint> bagGuildSymbolIds = GetBagGuildSymbolIds();
            for (int i = 0; i < bagGuildSymbolIds.Count; i++)
            {
                uint num3 = bagGuildSymbolIds[i];
                if (list.Contains(num3))
                {
                    return num3;
                }
            }
            return 0;
        }

        public static List<uint> GetBagGuildSymbolIds()
        {
            List<uint> list = new List<uint>();
            CUseableContainer useableContainer = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().GetUseableContainer(enCONTAINER_TYPE.ITEM);
            int curUseableCount = useableContainer.GetCurUseableCount();
            for (int i = 0; i < curUseableCount; i++)
            {
                CUseable useableByIndex = useableContainer.GetUseableByIndex(i);
                if (((useableByIndex != null) && (useableByIndex.m_type == COM_ITEM_TYPE.COM_OBJTYPE_ITEMSYMBOL)) && (GameDataMgr.symbolInfoDatabin.GetDataByKey(useableByIndex.m_baseID).dwGuildFacLv > 0))
                {
                    list.Add(useableByIndex.m_baseID);
                }
            }
            return list;
        }

        public static string GetBindQQGroupSignature()
        {
            ApolloAccountInfo accountInfo = Singleton<ApolloHelper>.GetInstance().GetAccountInfo(false);
            if (accountInfo != null)
            {
                object[] objArray1 = new object[] { accountInfo.OpenId, "_", Singleton<ApolloHelper>.GetInstance().GetAppId(), "_", Singleton<ApolloHelper>.GetInstance().GetAppKey(), "_", GetGroupGuildId(), "_", GetGuildLogicWorldId() };
                string input = string.Concat(objArray1);
                Debug.LogError("signature=" + input);
                return Utility.CreateMD5Hash(input);
            }
            return string.Empty;
        }

        public static string GetBuildingGuide(int buildingType)
        {
            switch (((RES_GUILD_BUILDING_TYPE) buildingType))
            {
                case RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_HALL:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Construct_Building_Hall_Guide_Text");

                case RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_BARRACK:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Construct_Building_Barrack_Guide_Text");

                case RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_FACTORY:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Construct_Building_Factory_Guide_Text");

                case RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_STATUE:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Construct_Building_Statue_Guide_Text");

                case RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_SHOP:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Construct_Building_Shop_Guide_Text");
            }
            return Singleton<CTextManager>.GetInstance().GetText("Guild_Guild_Building_Type_Unknown");
        }

        public static string GetBuildingInfo(int buildingType, int buildingLevel)
        {
            string str3;
            string text;
            string str = buildingLevel.ToString();
            if (IsAffectedByGuildDegrade(buildingLevel))
            {
                str = str + Singleton<CTextManager>.GetInstance().GetText("Guild_Guild_Degrade_Building_Level_Info_Plus");
            }
            string buildingName = GetBuildingName(buildingType);
            if (buildingLevel < 0x13)
            {
                str3 = GetSingleBuildingUpgradeMoney(buildingType, buildingLevel).ToString();
            }
            else
            {
                str3 = Singleton<CTextManager>.GetInstance().GetText("Common_None");
            }
            string str4 = GetSingleBuildingMaintainMoney(buildingType, buildingLevel).ToString();
            if (buildingType == 1)
            {
                string str6 = GetGuildUpgradeNeedOtherBuildingLevelSum().ToString();
                string[] textArray1 = new string[] { str, buildingName, str3, str4 };
                text = Singleton<CTextManager>.GetInstance().GetText("Guild_Construct_Building_Info_Text", textArray1);
                if (buildingLevel < 0x13)
                {
                    string[] textArray2 = new string[] { str6 };
                    text = text + "\n\n" + Singleton<CTextManager>.GetInstance().GetText("Guild_Construct_Building_Info_Text_Hall_Plus", textArray2);
                }
                return text;
            }
            string str7 = GetOtherBuildingUpgradeNeedGuildLevel(buildingLevel).ToString();
            string[] args = new string[] { str, buildingName, str3, str4 };
            text = Singleton<CTextManager>.GetInstance().GetText("Guild_Construct_Building_Info_Text", args);
            if (buildingLevel < 0x13)
            {
                string[] textArray4 = new string[] { str7 };
                text = text + "\n\n" + Singleton<CTextManager>.GetInstance().GetText("Guild_Construct_Building_Info_Text_Not_Hall_Plus", textArray4);
            }
            return text;
        }

        public static int GetBuildingLevelSumExceptHall()
        {
            int num = 0;
            List<GuildBuildingInfo> listBuildingInfo = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.listBuildingInfo;
            for (int i = 0; i < listBuildingInfo.Count; i++)
            {
                if (listBuildingInfo[i].type != RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_HALL)
                {
                    num += listBuildingInfo[i].level;
                }
            }
            return num;
        }

        public static string GetBuildingName(int buildingType)
        {
            switch (((RES_GUILD_BUILDING_TYPE) buildingType))
            {
                case RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_HALL:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Guild_Building_Type_Hall");

                case RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_BARRACK:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Guild_Building_Type_Barrack");

                case RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_FACTORY:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Guild_Building_Type_Factory");

                case RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_STATUE:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Guild_Building_Type_Statue");

                case RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_SHOP:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Guild_Building_Type_Shop");
            }
            return Singleton<CTextManager>.GetInstance().GetText("Guild_Guild_Building_Type_Unknown");
        }

        public static ulong GetChairmanUid()
        {
            ListView<GuildMemInfo> listMemInfo = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.listMemInfo;
            for (int i = 0; i < listMemInfo.Count; i++)
            {
                if (listMemInfo[i].enPosition == COM_PLAYER_GUILD_STATE.COM_PLAYER_GUILD_STATE_CHAIRMAN)
                {
                    return listMemInfo[i].stBriefInfo.uulUid;
                }
            }
            return 0L;
        }

        public static uint GetCoinProfitPercentage(int guildLevel)
        {
            int index = guildLevel - 1;
            if (((CGuildSystem.s_coinProfitPercentage != null) && (0 <= index)) && (index < CGuildSystem.s_coinProfitPercentage.Length))
            {
                return CGuildSystem.s_coinProfitPercentage[index];
            }
            return 0;
        }

        public static string GetConstructGuide(int guildLevel)
        {
            string str = GameDataMgr.guildLevelDatabin.GetDataByKey((byte) guildLevel).dwBaseActive.ToString();
            string str2 = Singleton<CGuildModel>.GetInstance().PlayerDailyActive.ToString();
            string str3 = GameDataMgr.guildMiscDatabin.GetDataByKey(14).dwConfValue.ToString();
            string[] args = new string[] { str, str2, str3 };
            return Singleton<CTextManager>.GetInstance().GetText("Guild_Construct_Guide_Text", args);
        }

        public static uint GetDonateCostCoin(RES_GUILD_DONATE_TYPE donateType)
        {
            return GameDataMgr.guildDonateDatabin.GetDataByKey((byte) donateType).dwCostGold;
        }

        public static uint GetDonateCostDianQuan(RES_GUILD_DONATE_TYPE donateType)
        {
            return GameDataMgr.guildDonateDatabin.GetDataByKey((byte) donateType).dwCostCoupons;
        }

        public static string GetDonateDescription(RES_GUILD_DONATE_TYPE donateType)
        {
            ResGuildDonate dataByKey = GameDataMgr.guildDonateDatabin.GetDataByKey((byte) donateType);
            uint dwCostGold = dataByKey.dwCostGold;
            uint dwCostCoupons = dataByKey.dwCostCoupons;
            uint dwGetConstruct = dataByKey.dwGetConstruct;
            uint dwGetGuildMoney = dataByKey.dwGetGuildMoney;
            uint dwGetCoinPool = dataByKey.dwGetCoinPool;
            string str = (dwCostGold != 0) ? dwCostGold.ToString() : dwCostCoupons.ToString();
            string text = Singleton<CTextManager>.GetInstance().GetText((dwCostGold != 0) ? "Money_Type_GoldCoin" : "Money_Type_DianQuan");
            string[] args = new string[] { str, text, dwGetConstruct.ToString(), dwGetGuildMoney.ToString(), dwGetCoinPool.ToString() };
            return Singleton<CTextManager>.GetInstance().GetText("Guild_Donate_Description", args);
        }

        public static string GetDonateSuccessTip(RES_GUILD_DONATE_TYPE donateType)
        {
            ResGuildDonate dataByKey = GameDataMgr.guildDonateDatabin.GetDataByKey((byte) donateType);
            uint dwGetConstruct = dataByKey.dwGetConstruct;
            uint dwGetGuildMoney = dataByKey.dwGetGuildMoney;
            uint dwGetCoinPool = dataByKey.dwGetCoinPool;
            string[] args = new string[] { dwGetConstruct.ToString(), dwGetGuildMoney.ToString(), dwGetCoinPool.ToString() };
            return Singleton<CTextManager>.GetInstance().GetText("Guild_Donate_Success", args);
        }

        public static int GetFactoryLevel()
        {
            List<GuildBuildingInfo> listBuildingInfo = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.listBuildingInfo;
            for (int i = 0; i < listBuildingInfo.Count; i++)
            {
                if (listBuildingInfo[i].type == RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_FACTORY)
                {
                    return listBuildingInfo[i].level;
                }
            }
            return 0;
        }

        public static uint GetGradeByRankpointScore(uint rankpointScore)
        {
            ResGuildGradeConf gradeResByRankpointScore = GetGradeResByRankpointScore(rankpointScore);
            return ((gradeResByRankpointScore == null) ? 0 : gradeResByRankpointScore.bIndex);
        }

        public static string GetGradeIconPathByGrade(int grade)
        {
            ResGuildGradeConf dataByKey = GameDataMgr.guildGradeDatabin.GetDataByKey((byte) grade);
            if (dataByKey != null)
            {
                return (CUIUtility.s_Sprite_Dynamic_Guild_Dir + dataByKey.szIcon);
            }
            return string.Empty;
        }

        public static string GetGradeIconPathByRankpointScore(uint rankpointScore)
        {
            ResGuildGradeConf gradeResByRankpointScore = GetGradeResByRankpointScore(rankpointScore);
            return ((gradeResByRankpointScore == null) ? string.Empty : (CUIUtility.s_Sprite_Dynamic_Guild_Dir + gradeResByRankpointScore.szIcon));
        }

        public static string GetGradeName(uint rankpointScore)
        {
            ResGuildGradeConf gradeResByRankpointScore = GetGradeResByRankpointScore(rankpointScore);
            return ((gradeResByRankpointScore == null) ? string.Empty : gradeResByRankpointScore.szGradeDesc);
        }

        public static string GetGradeNameForOpenGuildHeadImageShopSlot(int slotOffset)
        {
            uint dwGuildHeadImageShopOpenSlotCnt = 0;
            int count = GameDataMgr.guildGradeDatabin.count;
            for (int i = 0; i < count; i++)
            {
                ResGuildGradeConf dataByIndex = GameDataMgr.guildGradeDatabin.GetDataByIndex(i);
                if ((dwGuildHeadImageShopOpenSlotCnt <= slotOffset) && (slotOffset < dataByIndex.dwGuildHeadImageShopOpenSlotCnt))
                {
                    return StringHelper.UTF8BytesToString(ref dataByIndex.szGradeDesc);
                }
                dwGuildHeadImageShopOpenSlotCnt = dataByIndex.dwGuildHeadImageShopOpenSlotCnt;
            }
            object[] inParameters = new object[] { slotOffset };
            DebugHelper.Assert(false, "error slotOffset{0}: check shop and guildGrade res!!!", inParameters);
            return string.Empty;
        }

        private static ResGuildGradeConf GetGradeResByRankpointScore(uint rankpointScore)
        {
            for (int i = 0; i < GameDataMgr.guildGradeDatabin.count; i++)
            {
                ResGuildGradeConf dataByIndex = GameDataMgr.guildGradeDatabin.GetDataByIndex(i);
                if (rankpointScore <= dataByIndex.iScore)
                {
                    return dataByIndex;
                }
            }
            return GameDataMgr.guildGradeDatabin.GetDataByIndex(GameDataMgr.guildGradeDatabin.count - 1);
        }

        public static uint GetGroupGuildId()
        {
            return Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.groupGuildId;
        }

        public static uint GetGuildGrade()
        {
            if (Singleton<CGuildSystem>.GetInstance().IsInNormalGuild())
            {
                return GetGradeByRankpointScore(Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.RankInfo.totalRankPoint);
            }
            return 0;
        }

        public static uint GetGuildHeadImageShopOpenSlotCount()
        {
            uint guildGrade = GetGuildGrade();
            if (guildGrade > 0)
            {
                return GameDataMgr.guildGradeDatabin.GetDataByKey(guildGrade).dwGuildHeadImageShopOpenSlotCnt;
            }
            object[] inParameters = new object[] { guildGrade };
            DebugHelper.Assert(false, "error guild grade: {0}!!!", inParameters);
            return 0;
        }

        public static string GetGuildHeadPath()
        {
            if (Singleton<CGuildSystem>.GetInstance().IsInNormalGuild())
            {
                return (CUIUtility.s_Sprite_Dynamic_GuildHead_Dir + Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.stBriefInfo.dwHeadId);
            }
            return string.Empty;
        }

        public static uint GetGuildItemShopOpenSlotCount()
        {
            <GetGuildItemShopOpenSlotCount>c__AnonStorey3C storeyc = new <GetGuildItemShopOpenSlotCount>c__AnonStorey3C {
                guildStarLevel = GetGuildStarLevel()
            };
            if (storeyc.guildStarLevel == 0)
            {
                object[] inParameters = new object[] { storeyc.guildStarLevel };
                DebugHelper.Assert(false, "error guildStarLevel: {0}!!!", inParameters);
                return 0;
            }
            ResGuildShopStarIndexConf conf = GameDataMgr.guildStarLevel.FindIf(new Func<ResGuildShopStarIndexConf, bool>(storeyc, (IntPtr) this.<>m__30));
            if (conf != null)
            {
                return conf.dwGuildItemShopOpenSlotCnt;
            }
            return 0;
        }

        public static int GetGuildLevel()
        {
            if (Singleton<CGuildSystem>.GetInstance().IsInNormalGuild())
            {
                return Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.stBriefInfo.bLevel;
            }
            return -1;
        }

        public static int GetGuildLogicWorldId()
        {
            return MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID;
        }

        public static int GetGuildMemberCount()
        {
            return Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.listMemInfo.Count;
        }

        public static GuildMemInfo GetGuildMemberInfo(ulong uid)
        {
            return Singleton<CGuildModel>.GetInstance().GetGuildMemberInfoByUid(uid);
        }

        public static ListView<GuildMemInfo> GetGuildMemberInfos()
        {
            return Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.listMemInfo;
        }

        public static string GetGuildName()
        {
            if (Singleton<CGuildSystem>.GetInstance().IsInNormalGuild())
            {
                return Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.stBriefInfo.sName;
            }
            return string.Empty;
        }

        public static uint GetGuildStarLevel()
        {
            if (Singleton<CGuildSystem>.GetInstance().IsInNormalGuild())
            {
                return Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.star;
            }
            return 0;
        }

        public static uint GetGuildUpgradeNeedOtherBuildingLevelSum()
        {
            int num = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.stBriefInfo.bLevel + 1;
            if (num <= 0x13)
            {
                return GameDataMgr.guildLevelDatabin.GetDataByKey((byte) num).dwUpGradeLevelSum;
            }
            return 0;
        }

        public static string GetHeadUrl(string serverUrl)
        {
            return Singleton<ApolloHelper>.GetInstance().ToSnsHeadUrl(serverUrl);
        }

        public static List<uint> GetLevelOneGuildSymbolIds()
        {
            <GetLevelOneGuildSymbolIds>c__AnonStorey3B storeyb = new <GetLevelOneGuildSymbolIds>c__AnonStorey3B {
                ids = new List<uint>()
            };
            GameDataMgr.symbolInfoDatabin.Accept(new Action<ResSymbolInfo>(storeyb.<>m__2F));
            return storeyb.ids;
        }

        public static int GetMaxGuildMemberCountByLevel(int guildLevel)
        {
            ResGuildLevel dataByKey = GameDataMgr.guildLevelDatabin.GetDataByKey((byte) guildLevel);
            if (dataByKey != null)
            {
                return dataByKey.bMaxMemberCnt;
            }
            object[] inParameters = new object[] { guildLevel };
            DebugHelper.Assert(false, "CGuildHelper.GetMaxGuildMemberCountByLevel(): resGuildLevel is null, guildLevel={0}", inParameters);
            return -1;
        }

        public static int GetNextLevelSymbolAttValue(ResSymbolInfo symbolInfo, int attIndex)
        {
            if (symbolInfo.dwNextLvID == 0)
            {
                return 0;
            }
            return GameDataMgr.symbolInfoDatabin.GetDataByKey(symbolInfo.dwNextLvID).astFuncEftList[attIndex].iValue;
        }

        public static int GetNobeHeadIconId(ulong playerUid, uint nobeHeadIconIdFromGuild)
        {
            return (!IsSelf(playerUid) ? ((int) nobeHeadIconIdFromGuild) : MonoSingleton<NobeSys>.GetInstance().GetSelfNobeHeadIdx());
        }

        public static int GetNobeLevel(ulong playerUid, uint nobeLevelFromGuild)
        {
            return (!IsSelf(playerUid) ? ((int) nobeLevelFromGuild) : MonoSingleton<NobeSys>.GetInstance().GetSelfNobeLevel());
        }

        public static uint GetOtherBuildingCanReachMaxLevel()
        {
            byte bLevel = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.stBriefInfo.bLevel;
            return GameDataMgr.guildLevelDatabin.GetDataByKey(bLevel).bUpGradeLevelMax;
        }

        public static int GetOtherBuildingUpgradeNeedGuildLevel(int buildingLevel)
        {
            <GetOtherBuildingUpgradeNeedGuildLevel>c__AnonStorey38 storey = new <GetOtherBuildingUpgradeNeedGuildLevel>c__AnonStorey38 {
                nextBuildingLevel = buildingLevel + 1
            };
            ResGuildLevel level = GameDataMgr.guildLevelDatabin.FindIf(new Func<ResGuildLevel, bool>(storey, (IntPtr) this.<>m__2C));
            return ((level == null) ? -1 : level.bGuildLevel);
        }

        public static uint GetPlayerGuildConstruct()
        {
            GuildMemInfo playerGuildMemberInfo = Singleton<CGuildModel>.GetInstance().GetPlayerGuildMemberInfo();
            if (playerGuildMemberInfo != null)
            {
                return playerGuildMemberInfo.dwConstruct;
            }
            DebugHelper.Assert(false, "CGuildHelper.GetPlayerGuildConstruct() playerMemInfo == null!!! Maybe server not send GuildInfo at login time!!!");
            return 0;
        }

        public static GuildMemInfo GetPlayerGuildMemberInfo()
        {
            return Singleton<CGuildModel>.GetInstance().GetPlayerGuildMemberInfo();
        }

        public static RankpointRankInfo GetPlayerGuildRankpointRankInfo(enGuildRankpointRankListType rankListType)
        {
            ListView<RankpointRankInfo> view = Singleton<CGuildModel>.GetInstance().RankpointRankInfoLists[(int) rankListType];
            for (int i = 0; i < view.Count; i++)
            {
                if (Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.stBriefInfo.uulUid == view[i].guildId)
                {
                    return view[i];
                }
            }
            return CreatePlayerGuildRankpointRankInfo();
        }

        public static string GetPositionName(COM_PLAYER_GUILD_STATE position)
        {
            switch (position)
            {
                case COM_PLAYER_GUILD_STATE.COM_PLAYER_GUILD_STATE_CHAIRMAN:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_ChairMan");

                case COM_PLAYER_GUILD_STATE.COM_PLAYER_GUILD_STATE_VICE_CHAIRMAN:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Vice_Chairman");

                case COM_PLAYER_GUILD_STATE.COM_PLAYER_GUILD_STATE_ELDER:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Elder");

                case COM_PLAYER_GUILD_STATE.COM_PLAYER_GUILD_STATE_MEMBER:
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Normal_Member");
            }
            return string.Empty;
        }

        public static string GetRankpointClearTimeFormatString()
        {
            uint dwConfValue = GameDataMgr.guildMiscDatabin.GetDataByKey(0x26).dwConfValue;
            uint num2 = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.RankInfo.seasonStartTime + dwConfValue;
            return Utility.ToUtcTime2Local((long) num2).ToString(Singleton<CTextManager>.GetInstance().GetText("Guild_Rankpoint_Clear_Time_Format"));
        }

        public static int GetRankpointMemberListPlayerIndex()
        {
            List<KeyValuePair<ulong, MemberRankInfo>> rankpointMemberInfoList = Singleton<CGuildModel>.GetInstance().RankpointMemberInfoList;
            for (int i = 0; i < rankpointMemberInfoList.Count; i++)
            {
                KeyValuePair<ulong, MemberRankInfo> pair = rankpointMemberInfoList[i];
                if (pair.Key == Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().playerUllUID)
                {
                    return i;
                }
            }
            return -1;
        }

        public static uint GetRankpointSeasonAwardCoin(uint grade)
        {
            ResGuildGradeConf dataByKey = GameDataMgr.guildGradeDatabin.GetDataByKey(grade);
            if (dataByKey != null)
            {
                return dataByKey.dwGold;
            }
            return 0;
        }

        public static uint GetRankpointSeasonAwardDiamond(uint grade)
        {
            ResGuildGradeConf dataByKey = GameDataMgr.guildGradeDatabin.GetDataByKey(grade);
            if (dataByKey != null)
            {
                return dataByKey.dwDiamond;
            }
            return 0;
        }

        public static uint GetRankpointWeekAwardCoin(uint rank)
        {
            <GetRankpointWeekAwardCoin>c__AnonStorey39 storey = new <GetRankpointWeekAwardCoin>c__AnonStorey39 {
                rank = rank
            };
            ResGuildRankReward reward = GameDataMgr.guildRankRewardDatabin.FindIf(new Func<ResGuildRankReward, bool>(storey, (IntPtr) this.<>m__2D));
            return ((reward == null) ? GameDataMgr.guildRankRewardDatabin.GetDataByKey(-1).dwGold : reward.dwGold);
        }

        public static uint GetRankpointWeekAwardDiamond(uint rank)
        {
            <GetRankpointWeekAwardDiamond>c__AnonStorey3A storeya = new <GetRankpointWeekAwardDiamond>c__AnonStorey3A {
                rank = rank
            };
            ResGuildRankReward reward = GameDataMgr.guildRankRewardDatabin.FindIf(new Func<ResGuildRankReward, bool>(storeya, (IntPtr) this.<>m__2E));
            return ((reward == null) ? GameDataMgr.guildRankRewardDatabin.GetDataByKey(-1).dwDiamond : reward.dwDiamond);
        }

        public static double GetSelfRecommendTimeout()
        {
            uint dwConfValue = GameDataMgr.guildMiscDatabin.GetDataByKey(0x11).dwConfValue;
            TimeSpan span = new TimeSpan(0, 0, 0, (int) dwConfValue);
            return span.TotalHours;
        }

        public static int GetSerialGuildSymbolCount(uint levelOneSymbolId)
        {
            List<uint> list = new List<uint>();
            uint item = levelOneSymbolId;
            do
            {
                list.Add(item);
                item = GameDataMgr.symbolInfoDatabin.GetDataByKey(item).dwNextLvID;
            }
            while (item != 0);
            return list.Count;
        }

        public static uint GetSingleBuildingMaintainMoney(int buildingType, int buildingLevel)
        {
            ResGuildBuilding dataByKey = GameDataMgr.guildBuildingDatabin.GetDataByKey((byte) buildingType);
            int num = buildingLevel;
            if (IsAffectedByGuildDegrade(buildingLevel))
            {
                num--;
            }
            return dataByKey.MaintainMoney[num - 1];
        }

        public static uint GetSingleBuildingUpgradeMoney(int buildingType, int curBuildingLevel)
        {
            ResGuildBuilding dataByKey = GameDataMgr.guildBuildingDatabin.GetDataByKey((byte) buildingType);
            int num = curBuildingLevel + 1;
            if (num <= 0x13)
            {
                return dataByKey.UpGradeMoney[num - 1];
            }
            return 0;
        }

        public static uint GetStarLevelForOpenGuildItemShopSlot(int slotOffset)
        {
            uint dwGuildItemShopOpenSlotCnt = 0;
            int count = GameDataMgr.guildStarLevel.count;
            for (int i = 0; i < count; i++)
            {
                ResGuildShopStarIndexConf dataByIndex = GameDataMgr.guildStarLevel.GetDataByIndex(i);
                if ((dwGuildItemShopOpenSlotCnt <= slotOffset) && (slotOffset < dataByIndex.dwGuildItemShopOpenSlotCnt))
                {
                    return dataByIndex.dwBeginStar;
                }
                dwGuildItemShopOpenSlotCnt = dataByIndex.dwGuildItemShopOpenSlotCnt;
            }
            object[] inParameters = new object[] { slotOffset };
            DebugHelper.Assert(false, "error slotOffset{0}: check shop and guildStarLevel res!!!", inParameters);
            return 0;
        }

        private static string GetStarLevelTipString(uint starLevel)
        {
            string[] args = new string[] { starLevel.ToString() };
            return Singleton<CTextManager>.GetInstance().GetText("Guild_StarLevel_Current", args);
        }

        public static string GetSymbolAttUpgradeText(ResSymbolInfo symbolInfo)
        {
            string str = string.Empty;
            for (int i = 0; i < symbolInfo.astFuncEftList.Length; i++)
            {
                int wType = symbolInfo.astFuncEftList[i].wType;
                int bValType = symbolInfo.astFuncEftList[i].bValType;
                if (wType != 0)
                {
                    int nextLevelSymbolAttValue = GetNextLevelSymbolAttValue(symbolInfo, i);
                    switch (bValType)
                    {
                        case 0:
                        {
                            string[] args = new string[] { StringHelper.UTF8BytesToString(ref symbolInfo.szDesc), nextLevelSymbolAttValue.ToString() };
                            str = str + Singleton<CTextManager>.GetInstance().GetText("Guild_Symbol_Upgrade_Att_Process_Value", args);
                            break;
                        }
                        case 1:
                        {
                            string[] textArray2 = new string[] { StringHelper.UTF8BytesToString(ref symbolInfo.szDesc), (((double) nextLevelSymbolAttValue) / 100.0).ToString() };
                            str = str + Singleton<CTextManager>.GetInstance().GetText("Guild_Symbol_Upgrade_Att_Process_Percent", textArray2);
                            break;
                        }
                    }
                    str = str + "\n";
                }
            }
            return str;
        }

        public static string GetSymbolConditionText(ResSymbolInfo symbolInfo, bool isUpgrade)
        {
            string[] args = new string[] { Singleton<CTextManager>.GetInstance().GetText(!isUpgrade ? "Guild_Symbol_Open" : "Common_Up_Level") };
            string text = Singleton<CTextManager>.GetInstance().GetText("Guild_Symbol_Condition_Prefix", args);
            uint dwGuildFacLv = symbolInfo.dwGuildFacLv;
            int factoryLevel = GetFactoryLevel();
            string[] textArray2 = new string[] { dwGuildFacLv.ToString(), factoryLevel.ToString() };
            string str2 = Singleton<CTextManager>.GetInstance().GetText("Guild_Symbol_Condition_Factory", textArray2);
            if (factoryLevel < dwGuildFacLv)
            {
                str2 = "<color=red>" + str2 + "</color>";
            }
            uint dwGuildHisConstruct = symbolInfo.dwGuildHisConstruct;
            uint totalContruct = Singleton<CGuildModel>.GetInstance().GetPlayerGuildMemberInfo().TotalContruct;
            string[] textArray3 = new string[] { dwGuildHisConstruct.ToString(), totalContruct.ToString() };
            string str3 = Singleton<CTextManager>.GetInstance().GetText("Guild_Symbol_Condition_History_Construct", textArray3);
            if (totalContruct < dwGuildHisConstruct)
            {
                str3 = "<color=red>" + str3 + "</color>";
            }
            uint dwGuildConstruct = symbolInfo.dwGuildConstruct;
            uint dwConstruct = Singleton<CGuildModel>.GetInstance().GetPlayerGuildMemberInfo().dwConstruct;
            string[] textArray4 = new string[] { dwGuildConstruct.ToString() };
            string str4 = Singleton<CTextManager>.GetInstance().GetText("Guild_Symbol_Condition_Construct", textArray4);
            if (dwConstruct < dwGuildConstruct)
            {
                str4 = "<color=red>" + str4 + "</color>";
            }
            string[] textArray5 = new string[] { text, "\n", str2, "\n", str3, "\n", str4 };
            return string.Concat(textArray5);
        }

        public static int GetUpgradeCostDianQuanByLevel(int guildLevel)
        {
            ResGuildLevel dataByKey = GameDataMgr.guildLevelDatabin.GetDataByKey((byte) guildLevel);
            if (dataByKey != null)
            {
                return dataByKey.iUpgradeCostCoupons;
            }
            object[] inParameters = new object[] { guildLevel };
            DebugHelper.Assert(false, "CGuildHelper.GetUpgradeCostDianQuanByLevel(): resGuildLevel is null, guildLevel={0}", inParameters);
            return -1;
        }

        public static int GetViceChairmanMaxCount()
        {
            int guildLevel = GetGuildLevel();
            if (guildLevel > 0)
            {
                return GameDataMgr.guildLevelDatabin.GetDataByKey((byte) guildLevel).bViceChairManCnt;
            }
            return -1;
        }

        public static void GetViceChairmanUidAndName(out List<ulong> uids, out List<string> names)
        {
            uids = new List<ulong>();
            names = new List<string>();
            ListView<GuildMemInfo> listMemInfo = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.listMemInfo;
            for (int i = 0; i < listMemInfo.Count; i++)
            {
                if (listMemInfo[i].enPosition == COM_PLAYER_GUILD_STATE.COM_PLAYER_GUILD_STATE_VICE_CHAIRMAN)
                {
                    uids.Add(listMemInfo[i].stBriefInfo.uulUid);
                    names.Add(listMemInfo[i].stBriefInfo.sName);
                }
            }
        }

        public static bool IsAffectedByGuildDegrade(int buildingLevel)
        {
            return (buildingLevel > GetOtherBuildingCanReachMaxLevel());
        }

        public static bool IsBuildingUnlocked(int buildingType, out int guildLevelLimit)
        {
            int bLevel = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.stBriefInfo.bLevel;
            guildLevelLimit = GameDataMgr.guildBuildingDatabin.GetDataByKey((byte) buildingType).bGuildLvLimet;
            return (bLevel >= guildLevelLimit);
        }

        public static bool IsDonateUseCoin(RES_GUILD_DONATE_TYPE donateType)
        {
            return (GameDataMgr.guildDonateDatabin.GetDataByKey((byte) donateType).dwCostGold != 0);
        }

        public static bool IsFirstGuildListPage(SCPKG_GET_RANKING_LIST_RSP rsp)
        {
            return (rsp.stRankingListDetail.stOfSucc.iStart == 1);
        }

        public static bool IsGuildMaxGrade()
        {
            ResGuildGradeConf dataByIndex = GameDataMgr.guildGradeDatabin.GetDataByIndex(GameDataMgr.guildGradeDatabin.count - 1);
            return ((dataByIndex != null) && (GetGuildGrade() == dataByIndex.bIndex));
        }

        public static bool IsGuildMaxLevel(int curLevel)
        {
            return (GetUpgradeCostDianQuanByLevel(curLevel) == -1);
        }

        public static bool IsLastPage(int curPageId, uint totalCnt, int maxCntPerPage)
        {
            int num = ((int) Math.Ceiling(((double) totalCnt) / ((double) maxCntPerPage))) - 1;
            return (curPageId >= num);
        }

        public static bool IsMemberOnline(GuildMemInfo guildMemInfo)
        {
            return (guildMemInfo.stBriefInfo.dwGameEntity != 0);
        }

        public static bool IsNeedRequestNewRankpoinRank(enGuildRankpointRankListType rankListType)
        {
            return ((Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().getCurrentTimeSinceLogin() - Singleton<CGuildModel>.GetInstance().RankpointRankLastGottenTimes[(int) rankListType]) > 300);
        }

        public static bool IsPlayerSigned()
        {
            GuildMemInfo playerGuildMemberInfo = GetPlayerGuildMemberInfo();
            return ((playerGuildMemberInfo != null) && playerGuildMemberInfo.RankInfo.isSigned);
        }

        public static bool IsSelf(ulong playerUid)
        {
            return (playerUid == Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().playerUllUID);
        }

        public static bool IsViceChairmanFull()
        {
            ListView<GuildMemInfo> listMemInfo = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.listMemInfo;
            int num = 0;
            for (int i = 0; i < listMemInfo.Count; i++)
            {
                if (listMemInfo[i].enPosition == COM_PLAYER_GUILD_STATE.COM_PLAYER_GUILD_STATE_VICE_CHAIRMAN)
                {
                    num++;
                }
            }
            int viceChairmanMaxCount = GetViceChairmanMaxCount();
            return ((viceChairmanMaxCount > 0) && (num >= viceChairmanMaxCount));
        }

        public static bool IsWeekRankpointRank(enGuildRankpointRankListType rankListType)
        {
            return ((rankListType == enGuildRankpointRankListType.CurrentWeek) || (rankListType == enGuildRankpointRankListType.LastWeek));
        }

        public static void SetHallBuildingLevel(byte hallLevel)
        {
            List<GuildBuildingInfo> listBuildingInfo = Singleton<CGuildModel>.GetInstance().CurrentGuildInfo.listBuildingInfo;
            for (int i = 0; i < listBuildingInfo.Count; i++)
            {
                if (listBuildingInfo[i].type == RES_GUILD_BUILDING_TYPE.RES_GUILD_BUILDING_TYPE_HALL)
                {
                    listBuildingInfo[i].level = hallLevel;
                    return;
                }
            }
        }

        public static void SetPlayerSigned(bool isSigned)
        {
            GuildMemInfo playerGuildMemberInfo = GetPlayerGuildMemberInfo();
            if (playerGuildMemberInfo != null)
            {
                playerGuildMemberInfo.RankInfo.isSigned = isSigned;
            }
        }

        public static void SetRankDisplay(uint rankNumber, Transform rankTransform)
        {
            GameObject gameObject = rankTransform.Find("imgRank1st").gameObject;
            GameObject obj3 = rankTransform.Find("imgRank2nd").gameObject;
            GameObject obj4 = rankTransform.Find("imgRank3rd").gameObject;
            GameObject txtRank = rankTransform.Find("txtRank").gameObject;
            GameObject txtNotInRank = rankTransform.Find("txtNotInRank").gameObject;
            SetRankDisplay(rankNumber, gameObject, obj3, obj4, txtRank, txtNotInRank);
        }

        private static void SetRankDisplay(uint rankNumber, GameObject imgRank1st, GameObject imgRank2nd, GameObject imgRank3rd, GameObject txtRank, GameObject txtNotInRank)
        {
            if (rankNumber == 0)
            {
                imgRank1st.CustomSetActive(false);
                imgRank2nd.CustomSetActive(false);
                imgRank3rd.CustomSetActive(false);
                txtRank.CustomSetActive(false);
                txtNotInRank.CustomSetActive(true);
            }
            else if (rankNumber == 1)
            {
                imgRank1st.CustomSetActive(true);
                imgRank2nd.CustomSetActive(false);
                imgRank3rd.CustomSetActive(false);
                txtRank.CustomSetActive(false);
                txtNotInRank.CustomSetActive(false);
            }
            else if (rankNumber == 2)
            {
                imgRank1st.CustomSetActive(false);
                imgRank2nd.CustomSetActive(true);
                imgRank3rd.CustomSetActive(false);
                txtRank.CustomSetActive(false);
                txtNotInRank.CustomSetActive(false);
            }
            else if (rankNumber == 3)
            {
                imgRank1st.CustomSetActive(false);
                imgRank2nd.CustomSetActive(false);
                imgRank3rd.CustomSetActive(true);
                txtRank.CustomSetActive(false);
                txtNotInRank.CustomSetActive(false);
            }
            else
            {
                imgRank1st.CustomSetActive(false);
                imgRank2nd.CustomSetActive(false);
                imgRank3rd.CustomSetActive(false);
                txtRank.CustomSetActive(true);
                txtNotInRank.CustomSetActive(false);
                txtRank.GetComponent<Text>().text = rankNumber.ToString();
            }
        }

        public static void SetStarLevelPanel(uint starLevel, Transform panelTransform, CUIFormScript form)
        {
            if (panelTransform != null)
            {
                int num = (int) (starLevel / 0xd8);
                int num2 = (int) ((starLevel % 0xd8) / 0x24);
                int num3 = (int) (((starLevel % 0xd8) % 0x24) / 6);
                int num4 = (int) (((starLevel % 0xd8) % 0x24) % 6);
                int childCount = panelTransform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = panelTransform.GetChild(i);
                    if (child == null)
                    {
                        return;
                    }
                    Image component = child.GetComponent<Image>();
                    if (component == null)
                    {
                        return;
                    }
                    child.gameObject.CustomSetActive(true);
                    if (i < num)
                    {
                        component.SetSprite(DynamicPrefabPathCrown, form, true, false, false);
                    }
                    else if (i < (num + num2))
                    {
                        component.SetSprite(DynamicPrefabPathSun, form, true, false, false);
                    }
                    else if (i < ((num + num2) + num3))
                    {
                        component.SetSprite(DynamicPrefabPathMoon, form, true, false, false);
                    }
                    else if (i < (((num + num2) + num3) + num4))
                    {
                        component.SetSprite(DynamicPrefabPathStar, form, true, false, false);
                    }
                    else
                    {
                        child.gameObject.CustomSetActive(false);
                    }
                }
                CUICommonSystem.SetCommonTipsEvent(form, panelTransform.gameObject, GetStarLevelTipString(starLevel), enUseableTipsPos.enTop);
            }
        }

        [CompilerGenerated]
        private sealed class <GetGuildItemShopOpenSlotCount>c__AnonStorey3C
        {
            internal uint guildStarLevel;

            internal bool <>m__30(ResGuildShopStarIndexConf x)
            {
                return ((x.dwBeginStar <= this.guildStarLevel) && (this.guildStarLevel <= x.dwEndStar));
            }
        }

        [CompilerGenerated]
        private sealed class <GetLevelOneGuildSymbolIds>c__AnonStorey3B
        {
            internal List<uint> ids;

            internal void <>m__2F(ResSymbolInfo x)
            {
                if ((x.dwGuildFacLv > 0) && (x.wLevel == 1))
                {
                    this.ids.Add(x.dwID);
                }
            }
        }

        [CompilerGenerated]
        private sealed class <GetOtherBuildingUpgradeNeedGuildLevel>c__AnonStorey38
        {
            internal int nextBuildingLevel;

            internal bool <>m__2C(ResGuildLevel x)
            {
                return (x.bUpGradeLevelMax == this.nextBuildingLevel);
            }
        }

        [CompilerGenerated]
        private sealed class <GetRankpointWeekAwardCoin>c__AnonStorey39
        {
            internal uint rank;

            internal bool <>m__2D(ResGuildRankReward x)
            {
                return ((x.iStartRankNo <= this.rank) && (this.rank <= x.iEndRankNo));
            }
        }

        [CompilerGenerated]
        private sealed class <GetRankpointWeekAwardDiamond>c__AnonStorey3A
        {
            internal uint rank;

            internal bool <>m__2E(ResGuildRankReward x)
            {
                return ((x.iStartRankNo <= this.rank) && (this.rank <= x.iEndRankNo));
            }
        }
    }
}

