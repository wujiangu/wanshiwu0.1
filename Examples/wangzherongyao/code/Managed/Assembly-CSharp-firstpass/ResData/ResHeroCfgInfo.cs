namespace ResData
{
    using System;
    using tsf4g_tdr_csharp;

    public class ResHeroCfgInfo : IUnpackable, tsf4g_csharp_interface
    {
        public ResDT_SkillInfo[] astSkill = new ResDT_SkillInfo[5];
        public static readonly uint BASEVERSION = 1;
        public byte bAttackDistanceType;
        public byte bAttackType;
        public byte bDamageType;
        public byte bExpandType;
        public byte bIOSHide;
        public byte bIsBuyCoin;
        public byte bIsBuyCoupons;
        public byte bIsBuyDiamond;
        public byte bIsBuyItem;
        public byte bIsBuyMixPay;
        public byte bIsPlayerUse;
        public byte bIsPresent;
        public byte bIsTrainUse;
        public byte bJob;
        public byte bObtWayType;
        public byte bType;
        public static readonly uint CURRVERSION = 1;
        public uint dwBuyArenaCoin;
        public CrypticInteger dwBuyBurnCoin;
        public CrypticInteger dwBuyCoin;
        public CrypticInteger dwBuyCoupons;
        public CrypticInteger dwBuyDiamond;
        public uint dwBuyItemCnt;
        public uint dwCfgID;
        public uint dwChgItemCnt;
        public uint dwEnergyType;
        public uint dwShowSortId;
        public uint dwSortId;
        public uint dwWakeSkinID;
        public uint dwWakeTalentID;
        public CrypticInteger iAtkGrowth;
        public CrypticInteger iAtkSpdAddLvlup;
        public CrypticInteger iBaseAtkSpd;
        public CrypticInteger iBaseATT;
        public CrypticInteger iBaseDEF;
        public CrypticInteger iBaseHP;
        public CrypticInteger iBaseHPAdd;
        public CrypticInteger iBaseINT;
        public CrypticInteger iBaseRES;
        public CrypticInteger iBaseSpeed;
        public CrypticInteger iCritEft;
        public CrypticInteger iCritRate;
        public CrypticInteger iDefGrowth;
        public CrypticInteger iEnergy;
        public CrypticInteger iEnergyGrowth;
        public CrypticInteger iEnergyRec;
        public CrypticInteger iEnergyRecGrowth;
        public CrypticInteger iHPAddLvlup;
        public CrypticInteger iHpGrowth;
        public int iInitialStar;
        public uint[] InitGearID;
        public CrypticInteger iPhyDamage;
        public int iPVPNeedQuality;
        public int iPVPNeedStar;
        public int iPVPNeedSubQuality;
        public int iRecommendPosition;
        public CrypticInteger iResistGrowth;
        public int iSightR;
        public CrypticInteger iSpellDamage;
        public CrypticInteger iSpellGrowth;
        public int iStartedDifficulty;
        public CrypticInteger iViability;
        public int[] JobFeature;
        public static readonly uint LENGTH_szAI_Entry = 0x80;
        public static readonly uint LENGTH_szAI_Hard = 0x80;
        public static readonly uint LENGTH_szAI_Normal = 0x80;
        public static readonly uint LENGTH_szAI_Simple = 0x80;
        public static readonly uint LENGTH_szAI_Warm = 0x80;
        public static readonly uint LENGTH_szAI_WarmSimple = 0x80;
        public static readonly uint LENGTH_szAttackRangeDesc = 0x10;
        public static readonly uint LENGTH_szCharacterInfo = 0x80;
        public static readonly uint LENGTH_szHeroDesc = 0x100;
        public static readonly uint LENGTH_szHeroSound = 0x40;
        public static readonly uint LENGTH_szHeroTips = 0x1000;
        public static readonly uint LENGTH_szHeroTitle = 0x80;
        public static readonly uint LENGTH_szImagePath = 0x80;
        public static readonly uint LENGTH_szName = 0x20;
        public static readonly uint LENGTH_szObtWay = 0x100;
        public static readonly uint LENGTH_szStoryDesc = 0x1000;
        public static readonly uint LENGTH_szWakeDesc = 0x200;
        public uint[] PromotionID;
        public string szAI_Entry;
        public byte[] szAI_Entry_ByteArray;
        public string szAI_Hard;
        public byte[] szAI_Hard_ByteArray;
        public string szAI_Normal;
        public byte[] szAI_Normal_ByteArray;
        public string szAI_Simple;
        public byte[] szAI_Simple_ByteArray;
        public string szAI_Warm;
        public byte[] szAI_Warm_ByteArray;
        public string szAI_WarmSimple;
        public byte[] szAI_WarmSimple_ByteArray;
        public string szAttackRangeDesc;
        public byte[] szAttackRangeDesc_ByteArray;
        public string szCharacterInfo;
        public byte[] szCharacterInfo_ByteArray = new byte[1];
        public string szHeroDesc;
        public byte[] szHeroDesc_ByteArray;
        public string szHeroSound;
        public byte[] szHeroSound_ByteArray;
        public string szHeroTips;
        public byte[] szHeroTips_ByteArray;
        public string szHeroTitle;
        public byte[] szHeroTitle_ByteArray = new byte[1];
        public string szImagePath;
        public byte[] szImagePath_ByteArray = new byte[1];
        public string szName;
        public byte[] szName_ByteArray = new byte[1];
        public string szObtWay;
        public byte[] szObtWay_ByteArray;
        public string szStoryDesc;
        public byte[] szStoryDesc_ByteArray = new byte[1];
        public string szWakeDesc;
        public byte[] szWakeDesc_ByteArray;
        public ushort wPVPNeedLevel;

        public ResHeroCfgInfo()
        {
            for (int i = 0; i < 5; i++)
            {
                this.astSkill[i] = new ResDT_SkillInfo();
            }
            this.szObtWay_ByteArray = new byte[1];
            this.InitGearID = new uint[6];
            this.JobFeature = new int[2];
            this.szHeroDesc_ByteArray = new byte[1];
            this.szAI_Entry_ByteArray = new byte[1];
            this.szAI_Simple_ByteArray = new byte[1];
            this.szAI_Normal_ByteArray = new byte[1];
            this.szAI_Hard_ByteArray = new byte[1];
            this.szAI_WarmSimple_ByteArray = new byte[1];
            this.szAI_Warm_ByteArray = new byte[1];
            this.szWakeDesc_ByteArray = new byte[1];
            this.PromotionID = new uint[5];
            this.szAttackRangeDesc_ByteArray = new byte[1];
            this.szHeroTips_ByteArray = new byte[1];
            this.szHeroSound_ByteArray = new byte[1];
            this.szName = string.Empty;
            this.szHeroTitle = string.Empty;
            this.szStoryDesc = string.Empty;
            this.szImagePath = string.Empty;
            this.szCharacterInfo = string.Empty;
            this.szObtWay = string.Empty;
            this.szHeroDesc = string.Empty;
            this.szAI_Entry = string.Empty;
            this.szAI_Simple = string.Empty;
            this.szAI_Normal = string.Empty;
            this.szAI_Hard = string.Empty;
            this.szAI_WarmSimple = string.Empty;
            this.szAI_Warm = string.Empty;
            this.szWakeDesc = string.Empty;
            this.szAttackRangeDesc = string.Empty;
            this.szHeroTips = string.Empty;
            this.szHeroSound = string.Empty;
        }

        public TdrError.ErrorType construct()
        {
            return TdrError.ErrorType.TDR_NO_ERROR;
        }

        public TdrError.ErrorType load(ref TdrReadBuf srcBuf, uint cutVer)
        {
            int dest = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            int num9 = 0;
            int num10 = 0;
            int num11 = 0;
            int num12 = 0;
            int num13 = 0;
            int num14 = 0;
            int num15 = 0;
            int num16 = 0;
            int num17 = 0;
            uint num18 = 0;
            uint num19 = 0;
            uint num20 = 0;
            uint num21 = 0;
            int num22 = 0;
            int num23 = 0;
            int num24 = 0;
            int num25 = 0;
            int num26 = 0;
            int num27 = 0;
            int num28 = 0;
            srcBuf.disableEndian();
            TdrError.ErrorType type = TdrError.ErrorType.TDR_NO_ERROR;
            if ((cutVer == 0) || (CURRVERSION < cutVer))
            {
                cutVer = CURRVERSION;
            }
            if (BASEVERSION > cutVer)
            {
                return TdrError.ErrorType.TDR_ERR_CUTVER_TOO_SMALL;
            }
            type = srcBuf.readUInt32(ref this.dwCfgID);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                int count = 0x20;
                if (this.szName_ByteArray.GetLength(0) < count)
                {
                    this.szName_ByteArray = new byte[LENGTH_szName];
                }
                type = srcBuf.readCString(ref this.szName_ByteArray, count);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsTrainUse);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num30 = 0x80;
                if (this.szHeroTitle_ByteArray.GetLength(0) < num30)
                {
                    this.szHeroTitle_ByteArray = new byte[LENGTH_szHeroTitle];
                }
                type = srcBuf.readCString(ref this.szHeroTitle_ByteArray, num30);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num31 = 0x1000;
                if (this.szStoryDesc_ByteArray.GetLength(0) < num31)
                {
                    this.szStoryDesc_ByteArray = new byte[LENGTH_szStoryDesc];
                }
                type = srcBuf.readCString(ref this.szStoryDesc_ByteArray, num31);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num32 = 0x80;
                if (this.szImagePath_ByteArray.GetLength(0) < num32)
                {
                    this.szImagePath_ByteArray = new byte[LENGTH_szImagePath];
                }
                type = srcBuf.readCString(ref this.szImagePath_ByteArray, num32);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num33 = 0x80;
                if (this.szCharacterInfo_ByteArray.GetLength(0) < num33)
                {
                    this.szCharacterInfo_ByteArray = new byte[LENGTH_szCharacterInfo];
                }
                type = srcBuf.readCString(ref this.szCharacterInfo_ByteArray, num33);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iRecommendPosition);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bAttackDistanceType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iSightR);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref dest);
                this.iBaseHP = dest;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num2);
                this.iBaseHPAdd = num2;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num3);
                this.iHPAddLvlup = num3;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num4);
                this.iBaseATT = num4;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num5);
                this.iBaseINT = num5;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num6);
                this.iBaseDEF = num6;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num7);
                this.iBaseRES = num7;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num8);
                this.iBaseSpeed = num8;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num9);
                this.iAtkSpdAddLvlup = num9;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num10);
                this.iBaseAtkSpd = num10;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num11);
                this.iCritRate = num11;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num12);
                this.iCritEft = num12;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num13);
                this.iHpGrowth = num13;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num14);
                this.iAtkGrowth = num14;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num15);
                this.iSpellGrowth = num15;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num16);
                this.iDefGrowth = num16;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num17);
                this.iResistGrowth = num17;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int i = 0; i < 5; i++)
                {
                    type = this.astSkill[i].load(ref srcBuf, cutVer);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt32(ref this.dwChgItemCnt);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iInitialStar);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsPlayerUse);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bExpandType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt16(ref this.wPVPNeedLevel);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iPVPNeedQuality);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iPVPNeedSubQuality);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iPVPNeedStar);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyCoupons);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref num18);
                this.dwBuyCoupons = num18;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyCoin);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref num19);
                this.dwBuyCoin = num19;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBuyArenaCoin);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref num20);
                this.dwBuyBurnCoin = num20;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyDiamond);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref num21);
                this.dwBuyDiamond = num21;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyMixPay);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyItem);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBuyItemCnt);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bObtWayType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num35 = 0x100;
                if (this.szObtWay_ByteArray.GetLength(0) < num35)
                {
                    this.szObtWay_ByteArray = new byte[LENGTH_szObtWay];
                }
                type = srcBuf.readCString(ref this.szObtWay_ByteArray, num35);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num22);
                this.iViability = num22;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num23);
                this.iPhyDamage = num23;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num24);
                this.iSpellDamage = num24;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iStartedDifficulty);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int j = 0; j < 6; j++)
                {
                    type = srcBuf.readUInt32(ref this.InitGearID[j]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt8(ref this.bJob);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int k = 0; k < 2; k++)
                {
                    type = srcBuf.readInt32(ref this.JobFeature[k]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt8(ref this.bDamageType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bAttackType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num38 = 0x100;
                if (this.szHeroDesc_ByteArray.GetLength(0) < num38)
                {
                    this.szHeroDesc_ByteArray = new byte[LENGTH_szHeroDesc];
                }
                type = srcBuf.readCString(ref this.szHeroDesc_ByteArray, num38);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIOSHide);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwSortId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwShowSortId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num39 = 0x80;
                if (this.szAI_Entry_ByteArray.GetLength(0) < num39)
                {
                    this.szAI_Entry_ByteArray = new byte[LENGTH_szAI_Entry];
                }
                type = srcBuf.readCString(ref this.szAI_Entry_ByteArray, num39);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num40 = 0x80;
                if (this.szAI_Simple_ByteArray.GetLength(0) < num40)
                {
                    this.szAI_Simple_ByteArray = new byte[LENGTH_szAI_Simple];
                }
                type = srcBuf.readCString(ref this.szAI_Simple_ByteArray, num40);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num41 = 0x80;
                if (this.szAI_Normal_ByteArray.GetLength(0) < num41)
                {
                    this.szAI_Normal_ByteArray = new byte[LENGTH_szAI_Normal];
                }
                type = srcBuf.readCString(ref this.szAI_Normal_ByteArray, num41);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num42 = 0x80;
                if (this.szAI_Hard_ByteArray.GetLength(0) < num42)
                {
                    this.szAI_Hard_ByteArray = new byte[LENGTH_szAI_Hard];
                }
                type = srcBuf.readCString(ref this.szAI_Hard_ByteArray, num42);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num43 = 0x80;
                if (this.szAI_WarmSimple_ByteArray.GetLength(0) < num43)
                {
                    this.szAI_WarmSimple_ByteArray = new byte[LENGTH_szAI_WarmSimple];
                }
                type = srcBuf.readCString(ref this.szAI_WarmSimple_ByteArray, num43);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num44 = 0x80;
                if (this.szAI_Warm_ByteArray.GetLength(0) < num44)
                {
                    this.szAI_Warm_ByteArray = new byte[LENGTH_szAI_Warm];
                }
                type = srcBuf.readCString(ref this.szAI_Warm_ByteArray, num44);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num45 = 0x200;
                if (this.szWakeDesc_ByteArray.GetLength(0) < num45)
                {
                    this.szWakeDesc_ByteArray = new byte[LENGTH_szWakeDesc];
                }
                type = srcBuf.readCString(ref this.szWakeDesc_ByteArray, num45);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwWakeTalentID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwWakeSkinID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int m = 0; m < 5; m++)
                {
                    type = srcBuf.readUInt32(ref this.PromotionID[m]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                int num47 = 0x10;
                if (this.szAttackRangeDesc_ByteArray.GetLength(0) < num47)
                {
                    this.szAttackRangeDesc_ByteArray = new byte[LENGTH_szAttackRangeDesc];
                }
                type = srcBuf.readCString(ref this.szAttackRangeDesc_ByteArray, num47);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwEnergyType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num25);
                this.iEnergy = num25;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num26);
                this.iEnergyGrowth = num26;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num27);
                this.iEnergyRec = num27;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num28);
                this.iEnergyRecGrowth = num28;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsPresent);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num48 = 0x1000;
                if (this.szHeroTips_ByteArray.GetLength(0) < num48)
                {
                    this.szHeroTips_ByteArray = new byte[LENGTH_szHeroTips];
                }
                type = srcBuf.readCString(ref this.szHeroTips_ByteArray, num48);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num49 = 0x40;
                if (this.szHeroSound_ByteArray.GetLength(0) < num49)
                {
                    this.szHeroSound_ByteArray = new byte[LENGTH_szHeroSound];
                }
                type = srcBuf.readCString(ref this.szHeroSound_ByteArray, num49);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                this.TransferData();
            }
            return type;
        }

        public TdrError.ErrorType load(ref byte[] buffer, int size, ref int usedSize, uint cutVer)
        {
            if (((buffer == null) || (buffer.GetLength(0) == 0)) || (size > buffer.GetLength(0)))
            {
                return TdrError.ErrorType.TDR_ERR_INVALID_BUFFER_PARAMETER;
            }
            TdrReadBuf srcBuf = new TdrReadBuf(ref buffer, size);
            TdrError.ErrorType type = this.load(ref srcBuf, cutVer);
            usedSize = srcBuf.getUsedSize();
            return type;
        }

        private void TransferData()
        {
            this.szName = StringHelper.UTF8BytesToString(ref this.szName_ByteArray);
            this.szName_ByteArray = null;
            this.szHeroTitle = StringHelper.UTF8BytesToString(ref this.szHeroTitle_ByteArray);
            this.szHeroTitle_ByteArray = null;
            this.szStoryDesc = StringHelper.UTF8BytesToString(ref this.szStoryDesc_ByteArray);
            this.szStoryDesc_ByteArray = null;
            this.szImagePath = StringHelper.UTF8BytesToString(ref this.szImagePath_ByteArray);
            this.szImagePath_ByteArray = null;
            this.szCharacterInfo = StringHelper.UTF8BytesToString(ref this.szCharacterInfo_ByteArray);
            this.szCharacterInfo_ByteArray = null;
            this.szObtWay = StringHelper.UTF8BytesToString(ref this.szObtWay_ByteArray);
            this.szObtWay_ByteArray = null;
            this.szHeroDesc = StringHelper.UTF8BytesToString(ref this.szHeroDesc_ByteArray);
            this.szHeroDesc_ByteArray = null;
            this.szAI_Entry = StringHelper.UTF8BytesToString(ref this.szAI_Entry_ByteArray);
            this.szAI_Entry_ByteArray = null;
            this.szAI_Simple = StringHelper.UTF8BytesToString(ref this.szAI_Simple_ByteArray);
            this.szAI_Simple_ByteArray = null;
            this.szAI_Normal = StringHelper.UTF8BytesToString(ref this.szAI_Normal_ByteArray);
            this.szAI_Normal_ByteArray = null;
            this.szAI_Hard = StringHelper.UTF8BytesToString(ref this.szAI_Hard_ByteArray);
            this.szAI_Hard_ByteArray = null;
            this.szAI_WarmSimple = StringHelper.UTF8BytesToString(ref this.szAI_WarmSimple_ByteArray);
            this.szAI_WarmSimple_ByteArray = null;
            this.szAI_Warm = StringHelper.UTF8BytesToString(ref this.szAI_Warm_ByteArray);
            this.szAI_Warm_ByteArray = null;
            this.szWakeDesc = StringHelper.UTF8BytesToString(ref this.szWakeDesc_ByteArray);
            this.szWakeDesc_ByteArray = null;
            this.szAttackRangeDesc = StringHelper.UTF8BytesToString(ref this.szAttackRangeDesc_ByteArray);
            this.szAttackRangeDesc_ByteArray = null;
            this.szHeroTips = StringHelper.UTF8BytesToString(ref this.szHeroTips_ByteArray);
            this.szHeroTips_ByteArray = null;
            this.szHeroSound = StringHelper.UTF8BytesToString(ref this.szHeroSound_ByteArray);
            this.szHeroSound_ByteArray = null;
        }

        public TdrError.ErrorType unpack(ref TdrReadBuf srcBuf, uint cutVer)
        {
            int dest = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            int num9 = 0;
            int num10 = 0;
            int num11 = 0;
            int num12 = 0;
            int num13 = 0;
            int num14 = 0;
            int num15 = 0;
            int num16 = 0;
            int num17 = 0;
            uint num18 = 0;
            uint num19 = 0;
            uint num20 = 0;
            uint num21 = 0;
            int num22 = 0;
            int num23 = 0;
            int num24 = 0;
            int num25 = 0;
            int num26 = 0;
            int num27 = 0;
            int num28 = 0;
            TdrError.ErrorType type = TdrError.ErrorType.TDR_NO_ERROR;
            if ((cutVer == 0) || (CURRVERSION < cutVer))
            {
                cutVer = CURRVERSION;
            }
            if (BASEVERSION > cutVer)
            {
                return TdrError.ErrorType.TDR_ERR_CUTVER_TOO_SMALL;
            }
            type = srcBuf.readUInt32(ref this.dwCfgID);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                uint num29 = 0;
                type = srcBuf.readUInt32(ref num29);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num29 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num29 > this.szName_ByteArray.GetLength(0))
                {
                    if (num29 > LENGTH_szName)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szName_ByteArray = new byte[num29];
                }
                if (1 > num29)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szName_ByteArray, (int) num29);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szName_ByteArray[((int) num29) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num30 = TdrTypeUtil.cstrlen(this.szName_ByteArray) + 1;
                if (num29 != num30)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readUInt8(ref this.bIsTrainUse);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                uint num31 = 0;
                type = srcBuf.readUInt32(ref num31);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num31 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num31 > this.szHeroTitle_ByteArray.GetLength(0))
                {
                    if (num31 > LENGTH_szHeroTitle)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szHeroTitle_ByteArray = new byte[num31];
                }
                if (1 > num31)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szHeroTitle_ByteArray, (int) num31);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szHeroTitle_ByteArray[((int) num31) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num32 = TdrTypeUtil.cstrlen(this.szHeroTitle_ByteArray) + 1;
                if (num31 != num32)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num33 = 0;
                type = srcBuf.readUInt32(ref num33);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num33 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num33 > this.szStoryDesc_ByteArray.GetLength(0))
                {
                    if (num33 > LENGTH_szStoryDesc)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szStoryDesc_ByteArray = new byte[num33];
                }
                if (1 > num33)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szStoryDesc_ByteArray, (int) num33);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szStoryDesc_ByteArray[((int) num33) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num34 = TdrTypeUtil.cstrlen(this.szStoryDesc_ByteArray) + 1;
                if (num33 != num34)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num35 = 0;
                type = srcBuf.readUInt32(ref num35);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num35 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num35 > this.szImagePath_ByteArray.GetLength(0))
                {
                    if (num35 > LENGTH_szImagePath)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szImagePath_ByteArray = new byte[num35];
                }
                if (1 > num35)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szImagePath_ByteArray, (int) num35);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szImagePath_ByteArray[((int) num35) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num36 = TdrTypeUtil.cstrlen(this.szImagePath_ByteArray) + 1;
                if (num35 != num36)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num37 = 0;
                type = srcBuf.readUInt32(ref num37);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num37 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num37 > this.szCharacterInfo_ByteArray.GetLength(0))
                {
                    if (num37 > LENGTH_szCharacterInfo)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szCharacterInfo_ByteArray = new byte[num37];
                }
                if (1 > num37)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szCharacterInfo_ByteArray, (int) num37);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szCharacterInfo_ByteArray[((int) num37) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num38 = TdrTypeUtil.cstrlen(this.szCharacterInfo_ByteArray) + 1;
                if (num37 != num38)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readInt32(ref this.iRecommendPosition);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bAttackDistanceType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iSightR);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref dest);
                this.iBaseHP = dest;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num2);
                this.iBaseHPAdd = num2;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num3);
                this.iHPAddLvlup = num3;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num4);
                this.iBaseATT = num4;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num5);
                this.iBaseINT = num5;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num6);
                this.iBaseDEF = num6;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num7);
                this.iBaseRES = num7;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num8);
                this.iBaseSpeed = num8;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num9);
                this.iAtkSpdAddLvlup = num9;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num10);
                this.iBaseAtkSpd = num10;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num11);
                this.iCritRate = num11;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num12);
                this.iCritEft = num12;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num13);
                this.iHpGrowth = num13;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num14);
                this.iAtkGrowth = num14;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num15);
                this.iSpellGrowth = num15;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num16);
                this.iDefGrowth = num16;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num17);
                this.iResistGrowth = num17;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int i = 0; i < 5; i++)
                {
                    type = this.astSkill[i].unpack(ref srcBuf, cutVer);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt32(ref this.dwChgItemCnt);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iInitialStar);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsPlayerUse);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bExpandType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt16(ref this.wPVPNeedLevel);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iPVPNeedQuality);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iPVPNeedSubQuality);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iPVPNeedStar);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyCoupons);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref num18);
                this.dwBuyCoupons = num18;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyCoin);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref num19);
                this.dwBuyCoin = num19;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBuyArenaCoin);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref num20);
                this.dwBuyBurnCoin = num20;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyDiamond);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref num21);
                this.dwBuyDiamond = num21;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyMixPay);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyItem);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBuyItemCnt);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bObtWayType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                uint num40 = 0;
                type = srcBuf.readUInt32(ref num40);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num40 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num40 > this.szObtWay_ByteArray.GetLength(0))
                {
                    if (num40 > LENGTH_szObtWay)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szObtWay_ByteArray = new byte[num40];
                }
                if (1 > num40)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szObtWay_ByteArray, (int) num40);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szObtWay_ByteArray[((int) num40) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num41 = TdrTypeUtil.cstrlen(this.szObtWay_ByteArray) + 1;
                if (num40 != num41)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readInt32(ref num22);
                this.iViability = num22;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num23);
                this.iPhyDamage = num23;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num24);
                this.iSpellDamage = num24;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iStartedDifficulty);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int j = 0; j < 6; j++)
                {
                    type = srcBuf.readUInt32(ref this.InitGearID[j]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt8(ref this.bJob);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int k = 0; k < 2; k++)
                {
                    type = srcBuf.readInt32(ref this.JobFeature[k]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt8(ref this.bDamageType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bAttackType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                uint num44 = 0;
                type = srcBuf.readUInt32(ref num44);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num44 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num44 > this.szHeroDesc_ByteArray.GetLength(0))
                {
                    if (num44 > LENGTH_szHeroDesc)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szHeroDesc_ByteArray = new byte[num44];
                }
                if (1 > num44)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szHeroDesc_ByteArray, (int) num44);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szHeroDesc_ByteArray[((int) num44) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num45 = TdrTypeUtil.cstrlen(this.szHeroDesc_ByteArray) + 1;
                if (num44 != num45)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readUInt8(ref this.bIOSHide);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwSortId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwShowSortId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                uint num46 = 0;
                type = srcBuf.readUInt32(ref num46);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num46 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num46 > this.szAI_Entry_ByteArray.GetLength(0))
                {
                    if (num46 > LENGTH_szAI_Entry)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szAI_Entry_ByteArray = new byte[num46];
                }
                if (1 > num46)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szAI_Entry_ByteArray, (int) num46);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szAI_Entry_ByteArray[((int) num46) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num47 = TdrTypeUtil.cstrlen(this.szAI_Entry_ByteArray) + 1;
                if (num46 != num47)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num48 = 0;
                type = srcBuf.readUInt32(ref num48);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num48 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num48 > this.szAI_Simple_ByteArray.GetLength(0))
                {
                    if (num48 > LENGTH_szAI_Simple)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szAI_Simple_ByteArray = new byte[num48];
                }
                if (1 > num48)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szAI_Simple_ByteArray, (int) num48);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szAI_Simple_ByteArray[((int) num48) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num49 = TdrTypeUtil.cstrlen(this.szAI_Simple_ByteArray) + 1;
                if (num48 != num49)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num50 = 0;
                type = srcBuf.readUInt32(ref num50);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num50 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num50 > this.szAI_Normal_ByteArray.GetLength(0))
                {
                    if (num50 > LENGTH_szAI_Normal)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szAI_Normal_ByteArray = new byte[num50];
                }
                if (1 > num50)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szAI_Normal_ByteArray, (int) num50);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szAI_Normal_ByteArray[((int) num50) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num51 = TdrTypeUtil.cstrlen(this.szAI_Normal_ByteArray) + 1;
                if (num50 != num51)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num52 = 0;
                type = srcBuf.readUInt32(ref num52);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num52 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num52 > this.szAI_Hard_ByteArray.GetLength(0))
                {
                    if (num52 > LENGTH_szAI_Hard)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szAI_Hard_ByteArray = new byte[num52];
                }
                if (1 > num52)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szAI_Hard_ByteArray, (int) num52);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szAI_Hard_ByteArray[((int) num52) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num53 = TdrTypeUtil.cstrlen(this.szAI_Hard_ByteArray) + 1;
                if (num52 != num53)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num54 = 0;
                type = srcBuf.readUInt32(ref num54);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num54 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num54 > this.szAI_WarmSimple_ByteArray.GetLength(0))
                {
                    if (num54 > LENGTH_szAI_WarmSimple)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szAI_WarmSimple_ByteArray = new byte[num54];
                }
                if (1 > num54)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szAI_WarmSimple_ByteArray, (int) num54);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szAI_WarmSimple_ByteArray[((int) num54) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num55 = TdrTypeUtil.cstrlen(this.szAI_WarmSimple_ByteArray) + 1;
                if (num54 != num55)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num56 = 0;
                type = srcBuf.readUInt32(ref num56);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num56 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num56 > this.szAI_Warm_ByteArray.GetLength(0))
                {
                    if (num56 > LENGTH_szAI_Warm)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szAI_Warm_ByteArray = new byte[num56];
                }
                if (1 > num56)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szAI_Warm_ByteArray, (int) num56);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szAI_Warm_ByteArray[((int) num56) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num57 = TdrTypeUtil.cstrlen(this.szAI_Warm_ByteArray) + 1;
                if (num56 != num57)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num58 = 0;
                type = srcBuf.readUInt32(ref num58);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num58 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num58 > this.szWakeDesc_ByteArray.GetLength(0))
                {
                    if (num58 > LENGTH_szWakeDesc)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szWakeDesc_ByteArray = new byte[num58];
                }
                if (1 > num58)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szWakeDesc_ByteArray, (int) num58);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szWakeDesc_ByteArray[((int) num58) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num59 = TdrTypeUtil.cstrlen(this.szWakeDesc_ByteArray) + 1;
                if (num58 != num59)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readUInt32(ref this.dwWakeTalentID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwWakeSkinID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int m = 0; m < 5; m++)
                {
                    type = srcBuf.readUInt32(ref this.PromotionID[m]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                uint num61 = 0;
                type = srcBuf.readUInt32(ref num61);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num61 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num61 > this.szAttackRangeDesc_ByteArray.GetLength(0))
                {
                    if (num61 > LENGTH_szAttackRangeDesc)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szAttackRangeDesc_ByteArray = new byte[num61];
                }
                if (1 > num61)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szAttackRangeDesc_ByteArray, (int) num61);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szAttackRangeDesc_ByteArray[((int) num61) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num62 = TdrTypeUtil.cstrlen(this.szAttackRangeDesc_ByteArray) + 1;
                if (num61 != num62)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readUInt32(ref this.dwEnergyType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num25);
                this.iEnergy = num25;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num26);
                this.iEnergyGrowth = num26;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num27);
                this.iEnergyRec = num27;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref num28);
                this.iEnergyRecGrowth = num28;
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsPresent);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                uint num63 = 0;
                type = srcBuf.readUInt32(ref num63);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num63 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num63 > this.szHeroTips_ByteArray.GetLength(0))
                {
                    if (num63 > LENGTH_szHeroTips)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szHeroTips_ByteArray = new byte[num63];
                }
                if (1 > num63)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szHeroTips_ByteArray, (int) num63);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szHeroTips_ByteArray[((int) num63) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num64 = TdrTypeUtil.cstrlen(this.szHeroTips_ByteArray) + 1;
                if (num63 != num64)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num65 = 0;
                type = srcBuf.readUInt32(ref num65);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num65 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num65 > this.szHeroSound_ByteArray.GetLength(0))
                {
                    if (num65 > LENGTH_szHeroSound)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szHeroSound_ByteArray = new byte[num65];
                }
                if (1 > num65)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szHeroSound_ByteArray, (int) num65);
                if (type == TdrError.ErrorType.TDR_NO_ERROR)
                {
                    if (this.szHeroSound_ByteArray[((int) num65) - 1] != 0)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                    }
                    int num66 = TdrTypeUtil.cstrlen(this.szHeroSound_ByteArray) + 1;
                    if (num65 != num66)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                    }
                    this.TransferData();
                }
            }
            return type;
        }

        public TdrError.ErrorType unpack(ref byte[] buffer, int size, ref int usedSize, uint cutVer)
        {
            if (((buffer == null) || (buffer.GetLength(0) == 0)) || (size > buffer.GetLength(0)))
            {
                return TdrError.ErrorType.TDR_ERR_INVALID_BUFFER_PARAMETER;
            }
            TdrReadBuf srcBuf = new TdrReadBuf(ref buffer, size);
            TdrError.ErrorType type = this.unpack(ref srcBuf, cutVer);
            usedSize = srcBuf.getUsedSize();
            return type;
        }
    }
}

