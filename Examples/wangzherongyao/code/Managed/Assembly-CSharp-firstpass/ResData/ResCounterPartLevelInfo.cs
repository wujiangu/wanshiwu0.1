namespace ResData
{
    using System;
    using tsf4g_tdr_csharp;

    public class ResCounterPartLevelInfo : IUnpackable, tsf4g_csharp_interface
    {
        public static readonly uint BASEVERSION = 1;
        public byte bIsSingle;
        public static readonly uint CURRVERSION = 1;
        public uint dwAttackOrderID;
        public uint dwBattleTaskOfCamp1;
        public uint dwBattleTaskOfCamp2;
        public uint dwDynamicPropertyCfg;
        public uint dwMapId;
        public int iPvpDifficulty;
        public static readonly uint LENGTH_szAmbientSoundEvent = 0x20;
        public static readonly uint LENGTH_szBankResourceName = 0x20;
        public static readonly uint LENGTH_szMusicEndEvent = 0x20;
        public static readonly uint LENGTH_szMusicStartEvent = 0x20;
        public ResDT_LevelCommonInfo stLevelCommonInfo = new ResDT_LevelCommonInfo();
        public string szAmbientSoundEvent = string.Empty;
        public byte[] szAmbientSoundEvent_ByteArray = new byte[1];
        public string szBankResourceName = string.Empty;
        public byte[] szBankResourceName_ByteArray = new byte[1];
        public string szMusicEndEvent = string.Empty;
        public byte[] szMusicEndEvent_ByteArray = new byte[1];
        public string szMusicStartEvent = string.Empty;
        public byte[] szMusicStartEvent_ByteArray = new byte[1];
        public ushort wOriginalGoldCoinInBattle;

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
            type = srcBuf.readUInt32(ref this.dwMapId);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                type = this.stLevelCommonInfo.load(ref srcBuf, cutVer);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwAttackOrderID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt16(ref this.wOriginalGoldCoinInBattle);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwDynamicPropertyCfg);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBattleTaskOfCamp1);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBattleTaskOfCamp2);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int count = 0x20;
                if (this.szMusicStartEvent_ByteArray.GetLength(0) < count)
                {
                    this.szMusicStartEvent_ByteArray = new byte[LENGTH_szMusicStartEvent];
                }
                type = srcBuf.readCString(ref this.szMusicStartEvent_ByteArray, count);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num2 = 0x20;
                if (this.szMusicEndEvent_ByteArray.GetLength(0) < num2)
                {
                    this.szMusicEndEvent_ByteArray = new byte[LENGTH_szMusicEndEvent];
                }
                type = srcBuf.readCString(ref this.szMusicEndEvent_ByteArray, num2);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num3 = 0x20;
                if (this.szAmbientSoundEvent_ByteArray.GetLength(0) < num3)
                {
                    this.szAmbientSoundEvent_ByteArray = new byte[LENGTH_szAmbientSoundEvent];
                }
                type = srcBuf.readCString(ref this.szAmbientSoundEvent_ByteArray, num3);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num4 = 0x20;
                if (this.szBankResourceName_ByteArray.GetLength(0) < num4)
                {
                    this.szBankResourceName_ByteArray = new byte[LENGTH_szBankResourceName];
                }
                type = srcBuf.readCString(ref this.szBankResourceName_ByteArray, num4);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsSingle);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readInt32(ref this.iPvpDifficulty);
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
            this.szMusicStartEvent = StringHelper.UTF8BytesToString(ref this.szMusicStartEvent_ByteArray);
            this.szMusicStartEvent_ByteArray = null;
            this.szMusicEndEvent = StringHelper.UTF8BytesToString(ref this.szMusicEndEvent_ByteArray);
            this.szMusicEndEvent_ByteArray = null;
            this.szAmbientSoundEvent = StringHelper.UTF8BytesToString(ref this.szAmbientSoundEvent_ByteArray);
            this.szAmbientSoundEvent_ByteArray = null;
            this.szBankResourceName = StringHelper.UTF8BytesToString(ref this.szBankResourceName_ByteArray);
            this.szBankResourceName_ByteArray = null;
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
            type = srcBuf.readUInt32(ref this.dwMapId);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                type = this.stLevelCommonInfo.unpack(ref srcBuf, cutVer);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwAttackOrderID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt16(ref this.wOriginalGoldCoinInBattle);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwDynamicPropertyCfg);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBattleTaskOfCamp1);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBattleTaskOfCamp2);
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
                if (dest > this.szMusicStartEvent_ByteArray.GetLength(0))
                {
                    if (dest > LENGTH_szMusicStartEvent)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szMusicStartEvent_ByteArray = new byte[dest];
                }
                if (1 > dest)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szMusicStartEvent_ByteArray, (int) dest);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szMusicStartEvent_ByteArray[((int) dest) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num2 = TdrTypeUtil.cstrlen(this.szMusicStartEvent_ByteArray) + 1;
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
                if (num3 > this.szMusicEndEvent_ByteArray.GetLength(0))
                {
                    if (num3 > LENGTH_szMusicEndEvent)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szMusicEndEvent_ByteArray = new byte[num3];
                }
                if (1 > num3)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szMusicEndEvent_ByteArray, (int) num3);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szMusicEndEvent_ByteArray[((int) num3) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num4 = TdrTypeUtil.cstrlen(this.szMusicEndEvent_ByteArray) + 1;
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
                if (num5 > this.szAmbientSoundEvent_ByteArray.GetLength(0))
                {
                    if (num5 > LENGTH_szAmbientSoundEvent)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szAmbientSoundEvent_ByteArray = new byte[num5];
                }
                if (1 > num5)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szAmbientSoundEvent_ByteArray, (int) num5);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szAmbientSoundEvent_ByteArray[((int) num5) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num6 = TdrTypeUtil.cstrlen(this.szAmbientSoundEvent_ByteArray) + 1;
                if (num5 != num6)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
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
                if (num7 > this.szBankResourceName_ByteArray.GetLength(0))
                {
                    if (num7 > LENGTH_szBankResourceName)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szBankResourceName_ByteArray = new byte[num7];
                }
                if (1 > num7)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szBankResourceName_ByteArray, (int) num7);
                if (type == TdrError.ErrorType.TDR_NO_ERROR)
                {
                    if (this.szBankResourceName_ByteArray[((int) num7) - 1] != 0)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                    }
                    int num8 = TdrTypeUtil.cstrlen(this.szBankResourceName_ByteArray) + 1;
                    if (num7 != num8)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                    }
                    type = srcBuf.readUInt8(ref this.bIsSingle);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                    type = srcBuf.readInt32(ref this.iPvpDifficulty);
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

