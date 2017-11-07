namespace ResData
{
    using System;
    using tsf4g_tdr_csharp;

    public class ResLevelCfgInfo : IUnpackable, tsf4g_csharp_interface
    {
        public uint[] AIHeroID = new uint[5];
        public ResDT_MapBuff[] astMapBuffs;
        public ResDT_PveReviveInfo[] astReviveInfo;
        public ResDT_PveRewardShowInfo[] astRewardShowDetail;
        public ResDT_IntParamArrayNode[] astStarDetail = new ResDT_IntParamArrayNode[3];
        public static readonly uint BASEVERSION = 1;
        public byte bEnableHorizon;
        public byte bFinResultType;
        public byte bGuideLevelSubType;
        public byte bHideMinimap;
        public byte bIsOpenAutoAI;
        public byte bLevelDifficulty;
        public byte bLevelNo;
        public byte bMaxAcntNum;
        public byte bRandPickHero;
        public byte bReviveTimeMax;
        public byte bShowTrainingHelper;
        public byte bSoulGrow;
        public static readonly uint CURRVERSION = 1;
        public uint dwAIPlayerLevel;
        public uint dwAttackOrderID;
        public uint dwBattleListID;
        public uint dwChallengeNum;
        public uint dwDefaultActive;
        public uint dwDynamicPropertyCfg;
        public uint dwEnterConsumeAP;
        public uint dwFinishConsumeAP;
        public uint dwReviveTime;
        public uint dwSelfCampAIPlayerLevel;
        public uint dwSoulAllocId;
        public uint dwSoulID;
        public int iActivateLevelId;
        public int iBigMapHeight;
        public int iBigMapWidth;
        public int iCfgID;
        public int iChapterId;
        public int iExtraPassiveSkillId;
        public int iExtraSkill2Id;
        public int iExtraSkillId;
        public int iFailureDialogId;
        public int iHeroAIType;
        public int iHeroNum;
        public int iLevelType;
        public int iLoseCondition;
        public int iMapHeight;
        public int iMapWidth;
        public int iPassDialogId;
        public int iPreDialogId;
        public static readonly uint LENGTH_szAmbientSoundEvent = 0x20;
        public static readonly uint LENGTH_szArtistFileName = 0x20;
        public static readonly uint LENGTH_szBankResourceName = 0x20;
        public static readonly uint LENGTH_szBigMapPath = 0x80;
        public static readonly uint LENGTH_szDesignFileName = 0x20;
        public static readonly uint LENGTH_szLevelDesc = 150;
        public static readonly uint LENGTH_szLevelIcon = 0x80;
        public static readonly uint LENGTH_szMapPath = 0x80;
        public static readonly uint LENGTH_szMusicEndEvent = 0x20;
        public static readonly uint LENGTH_szMusicStartEvent = 0x20;
        public static readonly uint LENGTH_szName = 0x20;
        public static readonly uint LENGTH_szThumbnailPath = 0x80;
        public int[] RecommendLevel;
        public int[] RecommendPower;
        public uint[] SelfCampAIHeroID = new uint[5];
        public uint[] SettleIDDetail;
        public string szAmbientSoundEvent;
        public byte[] szAmbientSoundEvent_ByteArray;
        public string szArtistFileName;
        public byte[] szArtistFileName_ByteArray = new byte[1];
        public string szBankResourceName;
        public byte[] szBankResourceName_ByteArray;
        public string szBigMapPath;
        public byte[] szBigMapPath_ByteArray;
        public string szDesignFileName;
        public byte[] szDesignFileName_ByteArray = new byte[1];
        public string szLevelDesc;
        public byte[] szLevelDesc_ByteArray;
        public string szLevelIcon;
        public byte[] szLevelIcon_ByteArray = new byte[1];
        public string szMapPath;
        public byte[] szMapPath_ByteArray;
        public string szMusicEndEvent;
        public byte[] szMusicEndEvent_ByteArray;
        public string szMusicStartEvent;
        public byte[] szMusicStartEvent_ByteArray;
        public string szName;
        public byte[] szName_ByteArray = new byte[1];
        public string szThumbnailPath;
        public byte[] szThumbnailPath_ByteArray;

        public ResLevelCfgInfo()
        {
            for (int i = 0; i < 3; i++)
            {
                this.astStarDetail[i] = new ResDT_IntParamArrayNode();
            }
            this.astRewardShowDetail = new ResDT_PveRewardShowInfo[5];
            for (int j = 0; j < 5; j++)
            {
                this.astRewardShowDetail[j] = new ResDT_PveRewardShowInfo();
            }
            this.RecommendLevel = new int[4];
            this.RecommendPower = new int[4];
            this.szThumbnailPath_ByteArray = new byte[1];
            this.szMapPath_ByteArray = new byte[1];
            this.szBigMapPath_ByteArray = new byte[1];
            this.SettleIDDetail = new uint[4];
            this.szMusicStartEvent_ByteArray = new byte[1];
            this.szMusicEndEvent_ByteArray = new byte[1];
            this.szAmbientSoundEvent_ByteArray = new byte[1];
            this.szBankResourceName_ByteArray = new byte[1];
            this.astMapBuffs = new ResDT_MapBuff[4];
            for (int k = 0; k < 4; k++)
            {
                this.astMapBuffs[k] = new ResDT_MapBuff();
            }
            this.szLevelDesc_ByteArray = new byte[1];
            this.astReviveInfo = new ResDT_PveReviveInfo[4];
            for (int m = 0; m < 4; m++)
            {
                this.astReviveInfo[m] = new ResDT_PveReviveInfo();
            }
            this.szName = string.Empty;
            this.szDesignFileName = string.Empty;
            this.szArtistFileName = string.Empty;
            this.szLevelIcon = string.Empty;
            this.szThumbnailPath = string.Empty;
            this.szMapPath = string.Empty;
            this.szBigMapPath = string.Empty;
            this.szMusicStartEvent = string.Empty;
            this.szMusicEndEvent = string.Empty;
            this.szAmbientSoundEvent = string.Empty;
            this.szBankResourceName = string.Empty;
            this.szLevelDesc = string.Empty;
        }

        public TdrError.ErrorType construct()
        {
            return TdrError.ErrorType.TDR_NO_ERROR;
        }

        public TdrError.ErrorType load(ref TdrReadBuf srcBuf, uint cutVer)
        {
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
            type = srcBuf.readInt32(ref this.iCfgID);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                type = srcBuf.readInt32(ref this.iChapterId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bLevelNo);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bLevelDifficulty);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwChallengeNum);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bMaxAcntNum);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
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
                int num2 = 0x20;
                if (this.szDesignFileName_ByteArray.GetLength(0) < num2)
                {
                    this.szDesignFileName_ByteArray = new byte[LENGTH_szDesignFileName];
                }
                type = srcBuf.readCString(ref this.szDesignFileName_ByteArray, num2);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num3 = 0x20;
                if (this.szArtistFileName_ByteArray.GetLength(0) < num3)
                {
                    this.szArtistFileName_ByteArray = new byte[LENGTH_szArtistFileName];
                }
                type = srcBuf.readCString(ref this.szArtistFileName_ByteArray, num3);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iLevelType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num4 = 0x80;
                if (this.szLevelIcon_ByteArray.GetLength(0) < num4)
                {
                    this.szLevelIcon_ByteArray = new byte[LENGTH_szLevelIcon];
                }
                type = srcBuf.readCString(ref this.szLevelIcon_ByteArray, num4);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwSelfCampAIPlayerLevel);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int i = 0; i < 5; i++)
                {
                    type = srcBuf.readUInt32(ref this.SelfCampAIHeroID[i]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt32(ref this.dwAIPlayerLevel);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int j = 0; j < 5; j++)
                {
                    type = srcBuf.readUInt32(ref this.AIHeroID[j]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readInt32(ref this.iHeroNum);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iHeroAIType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int k = 0; k < 3; k++)
                {
                    type = this.astStarDetail[k].load(ref srcBuf, cutVer);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readInt32(ref this.iLoseCondition);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwDefaultActive);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iActivateLevelId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int m = 0; m < 5; m++)
                {
                    type = this.astRewardShowDetail[m].load(ref srcBuf, cutVer);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                for (int n = 0; n < 4; n++)
                {
                    type = srcBuf.readInt32(ref this.RecommendLevel[n]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                for (int num10 = 0; num10 < 4; num10++)
                {
                    type = srcBuf.readInt32(ref this.RecommendPower[num10]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt8(ref this.bHideMinimap);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num11 = 0x80;
                if (this.szThumbnailPath_ByteArray.GetLength(0) < num11)
                {
                    this.szThumbnailPath_ByteArray = new byte[LENGTH_szThumbnailPath];
                }
                type = srcBuf.readCString(ref this.szThumbnailPath_ByteArray, num11);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num12 = 0x80;
                if (this.szMapPath_ByteArray.GetLength(0) < num12)
                {
                    this.szMapPath_ByteArray = new byte[LENGTH_szMapPath];
                }
                type = srcBuf.readCString(ref this.szMapPath_ByteArray, num12);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num13 = 0x80;
                if (this.szBigMapPath_ByteArray.GetLength(0) < num13)
                {
                    this.szBigMapPath_ByteArray = new byte[LENGTH_szBigMapPath];
                }
                type = srcBuf.readCString(ref this.szBigMapPath_ByteArray, num13);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iMapWidth);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iMapHeight);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iBigMapWidth);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iBigMapHeight);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iPassDialogId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iPreDialogId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iFailureDialogId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwEnterConsumeAP);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwFinishConsumeAP);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBattleListID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int num14 = 0; num14 < 4; num14++)
                {
                    type = srcBuf.readUInt32(ref this.SettleIDDetail[num14]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt8(ref this.bSoulGrow);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwSoulID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwAttackOrderID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwReviveTime);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwDynamicPropertyCfg);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num15 = 0x20;
                if (this.szMusicStartEvent_ByteArray.GetLength(0) < num15)
                {
                    this.szMusicStartEvent_ByteArray = new byte[LENGTH_szMusicStartEvent];
                }
                type = srcBuf.readCString(ref this.szMusicStartEvent_ByteArray, num15);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num16 = 0x20;
                if (this.szMusicEndEvent_ByteArray.GetLength(0) < num16)
                {
                    this.szMusicEndEvent_ByteArray = new byte[LENGTH_szMusicEndEvent];
                }
                type = srcBuf.readCString(ref this.szMusicEndEvent_ByteArray, num16);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num17 = 0x20;
                if (this.szAmbientSoundEvent_ByteArray.GetLength(0) < num17)
                {
                    this.szAmbientSoundEvent_ByteArray = new byte[LENGTH_szAmbientSoundEvent];
                }
                type = srcBuf.readCString(ref this.szAmbientSoundEvent_ByteArray, num17);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num18 = 0x20;
                if (this.szBankResourceName_ByteArray.GetLength(0) < num18)
                {
                    this.szBankResourceName_ByteArray = new byte[LENGTH_szBankResourceName];
                }
                type = srcBuf.readCString(ref this.szBankResourceName_ByteArray, num18);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bEnableHorizon);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsOpenAutoAI);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int num19 = 0; num19 < 4; num19++)
                {
                    type = this.astMapBuffs[num19].load(ref srcBuf, cutVer);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                int num20 = 150;
                if (this.szLevelDesc_ByteArray.GetLength(0) < num20)
                {
                    this.szLevelDesc_ByteArray = new byte[LENGTH_szLevelDesc];
                }
                type = srcBuf.readCString(ref this.szLevelDesc_ByteArray, num20);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int num21 = 0; num21 < 4; num21++)
                {
                    type = this.astReviveInfo[num21].load(ref srcBuf, cutVer);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt8(ref this.bReviveTimeMax);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iExtraSkillId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iExtraSkill2Id);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iExtraPassiveSkillId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bFinResultType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bRandPickHero);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwSoulAllocId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bShowTrainingHelper);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bGuideLevelSubType);
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
            this.szDesignFileName = StringHelper.UTF8BytesToString(ref this.szDesignFileName_ByteArray);
            this.szDesignFileName_ByteArray = null;
            this.szArtistFileName = StringHelper.UTF8BytesToString(ref this.szArtistFileName_ByteArray);
            this.szArtistFileName_ByteArray = null;
            this.szLevelIcon = StringHelper.UTF8BytesToString(ref this.szLevelIcon_ByteArray);
            this.szLevelIcon_ByteArray = null;
            this.szThumbnailPath = StringHelper.UTF8BytesToString(ref this.szThumbnailPath_ByteArray);
            this.szThumbnailPath_ByteArray = null;
            this.szMapPath = StringHelper.UTF8BytesToString(ref this.szMapPath_ByteArray);
            this.szMapPath_ByteArray = null;
            this.szBigMapPath = StringHelper.UTF8BytesToString(ref this.szBigMapPath_ByteArray);
            this.szBigMapPath_ByteArray = null;
            this.szMusicStartEvent = StringHelper.UTF8BytesToString(ref this.szMusicStartEvent_ByteArray);
            this.szMusicStartEvent_ByteArray = null;
            this.szMusicEndEvent = StringHelper.UTF8BytesToString(ref this.szMusicEndEvent_ByteArray);
            this.szMusicEndEvent_ByteArray = null;
            this.szAmbientSoundEvent = StringHelper.UTF8BytesToString(ref this.szAmbientSoundEvent_ByteArray);
            this.szAmbientSoundEvent_ByteArray = null;
            this.szBankResourceName = StringHelper.UTF8BytesToString(ref this.szBankResourceName_ByteArray);
            this.szBankResourceName_ByteArray = null;
            this.szLevelDesc = StringHelper.UTF8BytesToString(ref this.szLevelDesc_ByteArray);
            this.szLevelDesc_ByteArray = null;
        }

        public TdrError.ErrorType unpack(ref TdrReadBuf srcBuf, uint cutVer)
        {
            TdrError.ErrorType type = TdrError.ErrorType.TDR_NO_ERROR;
            if ((cutVer == 0) || (CURRVERSION < cutVer))
            {
                cutVer = CURRVERSION;
            }
            if (BASEVERSION > cutVer)
            {
                return TdrError.ErrorType.TDR_ERR_CUTVER_TOO_SMALL;
            }
            type = srcBuf.readInt32(ref this.iCfgID);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                type = srcBuf.readInt32(ref this.iChapterId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bLevelNo);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bLevelDifficulty);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwChallengeNum);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bMaxAcntNum);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                uint dest = 0;
                type = srcBuf.readUInt32(ref dest);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (dest > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (dest > this.szName_ByteArray.GetLength(0))
                {
                    if (dest > LENGTH_szName)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szName_ByteArray = new byte[dest];
                }
                if (1 > dest)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szName_ByteArray, (int) dest);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szName_ByteArray[((int) dest) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num2 = TdrTypeUtil.cstrlen(this.szName_ByteArray) + 1;
                if (dest != num2)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num3 = 0;
                type = srcBuf.readUInt32(ref num3);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num3 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num3 > this.szDesignFileName_ByteArray.GetLength(0))
                {
                    if (num3 > LENGTH_szDesignFileName)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szDesignFileName_ByteArray = new byte[num3];
                }
                if (1 > num3)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szDesignFileName_ByteArray, (int) num3);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szDesignFileName_ByteArray[((int) num3) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num4 = TdrTypeUtil.cstrlen(this.szDesignFileName_ByteArray) + 1;
                if (num3 != num4)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num5 = 0;
                type = srcBuf.readUInt32(ref num5);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num5 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num5 > this.szArtistFileName_ByteArray.GetLength(0))
                {
                    if (num5 > LENGTH_szArtistFileName)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szArtistFileName_ByteArray = new byte[num5];
                }
                if (1 > num5)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szArtistFileName_ByteArray, (int) num5);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szArtistFileName_ByteArray[((int) num5) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num6 = TdrTypeUtil.cstrlen(this.szArtistFileName_ByteArray) + 1;
                if (num5 != num6)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readInt32(ref this.iLevelType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                uint num7 = 0;
                type = srcBuf.readUInt32(ref num7);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num7 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num7 > this.szLevelIcon_ByteArray.GetLength(0))
                {
                    if (num7 > LENGTH_szLevelIcon)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szLevelIcon_ByteArray = new byte[num7];
                }
                if (1 > num7)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szLevelIcon_ByteArray, (int) num7);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szLevelIcon_ByteArray[((int) num7) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num8 = TdrTypeUtil.cstrlen(this.szLevelIcon_ByteArray) + 1;
                if (num7 != num8)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readUInt32(ref this.dwSelfCampAIPlayerLevel);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int i = 0; i < 5; i++)
                {
                    type = srcBuf.readUInt32(ref this.SelfCampAIHeroID[i]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt32(ref this.dwAIPlayerLevel);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int j = 0; j < 5; j++)
                {
                    type = srcBuf.readUInt32(ref this.AIHeroID[j]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readInt32(ref this.iHeroNum);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iHeroAIType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int k = 0; k < 3; k++)
                {
                    type = this.astStarDetail[k].unpack(ref srcBuf, cutVer);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readInt32(ref this.iLoseCondition);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwDefaultActive);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iActivateLevelId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int m = 0; m < 5; m++)
                {
                    type = this.astRewardShowDetail[m].unpack(ref srcBuf, cutVer);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                for (int n = 0; n < 4; n++)
                {
                    type = srcBuf.readInt32(ref this.RecommendLevel[n]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                for (int num14 = 0; num14 < 4; num14++)
                {
                    type = srcBuf.readInt32(ref this.RecommendPower[num14]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt8(ref this.bHideMinimap);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                uint num15 = 0;
                type = srcBuf.readUInt32(ref num15);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num15 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num15 > this.szThumbnailPath_ByteArray.GetLength(0))
                {
                    if (num15 > LENGTH_szThumbnailPath)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szThumbnailPath_ByteArray = new byte[num15];
                }
                if (1 > num15)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szThumbnailPath_ByteArray, (int) num15);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szThumbnailPath_ByteArray[((int) num15) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num16 = TdrTypeUtil.cstrlen(this.szThumbnailPath_ByteArray) + 1;
                if (num15 != num16)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num17 = 0;
                type = srcBuf.readUInt32(ref num17);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num17 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num17 > this.szMapPath_ByteArray.GetLength(0))
                {
                    if (num17 > LENGTH_szMapPath)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szMapPath_ByteArray = new byte[num17];
                }
                if (1 > num17)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szMapPath_ByteArray, (int) num17);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szMapPath_ByteArray[((int) num17) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num18 = TdrTypeUtil.cstrlen(this.szMapPath_ByteArray) + 1;
                if (num17 != num18)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num19 = 0;
                type = srcBuf.readUInt32(ref num19);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num19 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num19 > this.szBigMapPath_ByteArray.GetLength(0))
                {
                    if (num19 > LENGTH_szBigMapPath)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szBigMapPath_ByteArray = new byte[num19];
                }
                if (1 > num19)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szBigMapPath_ByteArray, (int) num19);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szBigMapPath_ByteArray[((int) num19) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num20 = TdrTypeUtil.cstrlen(this.szBigMapPath_ByteArray) + 1;
                if (num19 != num20)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readInt32(ref this.iMapWidth);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iMapHeight);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iBigMapWidth);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iBigMapHeight);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iPassDialogId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iPreDialogId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iFailureDialogId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwEnterConsumeAP);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwFinishConsumeAP);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBattleListID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int num21 = 0; num21 < 4; num21++)
                {
                    type = srcBuf.readUInt32(ref this.SettleIDDetail[num21]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt8(ref this.bSoulGrow);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwSoulID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwAttackOrderID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwReviveTime);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwDynamicPropertyCfg);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                uint num22 = 0;
                type = srcBuf.readUInt32(ref num22);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num22 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num22 > this.szMusicStartEvent_ByteArray.GetLength(0))
                {
                    if (num22 > LENGTH_szMusicStartEvent)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szMusicStartEvent_ByteArray = new byte[num22];
                }
                if (1 > num22)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szMusicStartEvent_ByteArray, (int) num22);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szMusicStartEvent_ByteArray[((int) num22) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num23 = TdrTypeUtil.cstrlen(this.szMusicStartEvent_ByteArray) + 1;
                if (num22 != num23)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num24 = 0;
                type = srcBuf.readUInt32(ref num24);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num24 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num24 > this.szMusicEndEvent_ByteArray.GetLength(0))
                {
                    if (num24 > LENGTH_szMusicEndEvent)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szMusicEndEvent_ByteArray = new byte[num24];
                }
                if (1 > num24)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szMusicEndEvent_ByteArray, (int) num24);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szMusicEndEvent_ByteArray[((int) num24) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num25 = TdrTypeUtil.cstrlen(this.szMusicEndEvent_ByteArray) + 1;
                if (num24 != num25)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num26 = 0;
                type = srcBuf.readUInt32(ref num26);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num26 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num26 > this.szAmbientSoundEvent_ByteArray.GetLength(0))
                {
                    if (num26 > LENGTH_szAmbientSoundEvent)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szAmbientSoundEvent_ByteArray = new byte[num26];
                }
                if (1 > num26)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szAmbientSoundEvent_ByteArray, (int) num26);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szAmbientSoundEvent_ByteArray[((int) num26) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num27 = TdrTypeUtil.cstrlen(this.szAmbientSoundEvent_ByteArray) + 1;
                if (num26 != num27)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num28 = 0;
                type = srcBuf.readUInt32(ref num28);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num28 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num28 > this.szBankResourceName_ByteArray.GetLength(0))
                {
                    if (num28 > LENGTH_szBankResourceName)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szBankResourceName_ByteArray = new byte[num28];
                }
                if (1 > num28)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szBankResourceName_ByteArray, (int) num28);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szBankResourceName_ByteArray[((int) num28) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num29 = TdrTypeUtil.cstrlen(this.szBankResourceName_ByteArray) + 1;
                if (num28 != num29)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readUInt8(ref this.bEnableHorizon);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsOpenAutoAI);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int num30 = 0; num30 < 4; num30++)
                {
                    type = this.astMapBuffs[num30].unpack(ref srcBuf, cutVer);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
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
                if (num31 > this.szLevelDesc_ByteArray.GetLength(0))
                {
                    if (num31 > LENGTH_szLevelDesc)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szLevelDesc_ByteArray = new byte[num31];
                }
                if (1 > num31)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szLevelDesc_ByteArray, (int) num31);
                if (type == TdrError.ErrorType.TDR_NO_ERROR)
                {
                    if (this.szLevelDesc_ByteArray[((int) num31) - 1] != 0)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                    }
                    int num32 = TdrTypeUtil.cstrlen(this.szLevelDesc_ByteArray) + 1;
                    if (num31 != num32)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                    }
                    for (int num33 = 0; num33 < 4; num33++)
                    {
                        type = this.astReviveInfo[num33].unpack(ref srcBuf, cutVer);
                        if (type != TdrError.ErrorType.TDR_NO_ERROR)
                        {
                            return type;
                        }
                    }
                    type = srcBuf.readUInt8(ref this.bReviveTimeMax);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                    type = srcBuf.readInt32(ref this.iExtraSkillId);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                    type = srcBuf.readInt32(ref this.iExtraSkill2Id);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                    type = srcBuf.readInt32(ref this.iExtraPassiveSkillId);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                    type = srcBuf.readUInt8(ref this.bFinResultType);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                    type = srcBuf.readUInt8(ref this.bRandPickHero);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                    type = srcBuf.readUInt32(ref this.dwSoulAllocId);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                    type = srcBuf.readUInt8(ref this.bShowTrainingHelper);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                    type = srcBuf.readUInt8(ref this.bGuideLevelSubType);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
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

