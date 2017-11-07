namespace ResData
{
    using System;
    using tsf4g_tdr_csharp;

    public class ResHeroSkin : IUnpackable, tsf4g_csharp_interface
    {
        public ResDT_FuncEft_Obj[] astAttr = new ResDT_FuncEft_Obj[15];
        public static readonly uint BASEVERSION = 1;
        public byte bGetPathType;
        public byte bIsBuyCoupons;
        public byte bIsBuyDiamond;
        public byte bIsBuyItem;
        public byte bIsBuyMixPay;
        public byte bIsBuySkinCoin;
        public byte bIsDiscount;
        public byte bIsLimitSkin;
        public byte bIsPresent;
        public byte bIsShow;
        public byte bSkinQuality;
        public static readonly uint CURRVERSION = 1;
        public uint dwBuyCoupons;
        public uint dwBuyDiamond;
        public uint dwBuyItemCnt;
        public uint dwBuySkinCoin;
        public uint dwChgItemCnt;
        public uint dwCombatAbility;
        public uint dwDiscount;
        public uint dwHeroID;
        public uint dwID;
        public uint dwPresentHeadImg;
        public uint dwSkinID;
        public uint dwSortId;
        public static readonly uint LENGTH_szGetPath = 0x100;
        public static readonly uint LENGTH_szHeroName = 0x20;
        public static readonly uint LENGTH_szLimitLabelPic = 0x80;
        public static readonly uint LENGTH_szLimitQualityPic = 0x80;
        public static readonly uint LENGTH_szSkinDesc = 0x100;
        public static readonly uint LENGTH_szSkinName = 0x20;
        public static readonly uint LENGTH_szSkinPicID = 0x80;
        public static readonly uint LENGTH_szSkinSoundResPack = 0x80;
        public static readonly uint LENGTH_szSoundSwitchEvent = 0x80;
        public uint[] PromotionID;
        public string szGetPath;
        public byte[] szGetPath_ByteArray;
        public string szHeroName;
        public byte[] szHeroName_ByteArray = new byte[1];
        public string szLimitLabelPic;
        public byte[] szLimitLabelPic_ByteArray = new byte[1];
        public string szLimitQualityPic;
        public byte[] szLimitQualityPic_ByteArray = new byte[1];
        public string szSkinDesc;
        public byte[] szSkinDesc_ByteArray = new byte[1];
        public string szSkinName;
        public byte[] szSkinName_ByteArray = new byte[1];
        public string szSkinPicID;
        public byte[] szSkinPicID_ByteArray = new byte[1];
        public string szSkinSoundResPack;
        public byte[] szSkinSoundResPack_ByteArray = new byte[1];
        public string szSoundSwitchEvent;
        public byte[] szSoundSwitchEvent_ByteArray = new byte[1];

        public ResHeroSkin()
        {
            for (int i = 0; i < 15; i++)
            {
                this.astAttr[i] = new ResDT_FuncEft_Obj();
            }
            this.PromotionID = new uint[5];
            this.szGetPath_ByteArray = new byte[1];
            this.szHeroName = string.Empty;
            this.szSkinName = string.Empty;
            this.szSkinDesc = string.Empty;
            this.szSkinPicID = string.Empty;
            this.szSkinSoundResPack = string.Empty;
            this.szSoundSwitchEvent = string.Empty;
            this.szLimitQualityPic = string.Empty;
            this.szLimitLabelPic = string.Empty;
            this.szGetPath = string.Empty;
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
            type = srcBuf.readUInt32(ref this.dwID);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                type = srcBuf.readUInt32(ref this.dwHeroID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int count = 0x20;
                if (this.szHeroName_ByteArray.GetLength(0) < count)
                {
                    this.szHeroName_ByteArray = new byte[LENGTH_szHeroName];
                }
                type = srcBuf.readCString(ref this.szHeroName_ByteArray, count);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwSkinID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num2 = 0x20;
                if (this.szSkinName_ByteArray.GetLength(0) < num2)
                {
                    this.szSkinName_ByteArray = new byte[LENGTH_szSkinName];
                }
                type = srcBuf.readCString(ref this.szSkinName_ByteArray, num2);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num3 = 0x100;
                if (this.szSkinDesc_ByteArray.GetLength(0) < num3)
                {
                    this.szSkinDesc_ByteArray = new byte[LENGTH_szSkinDesc];
                }
                type = srcBuf.readCString(ref this.szSkinDesc_ByteArray, num3);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num4 = 0x80;
                if (this.szSkinPicID_ByteArray.GetLength(0) < num4)
                {
                    this.szSkinPicID_ByteArray = new byte[LENGTH_szSkinPicID];
                }
                type = srcBuf.readCString(ref this.szSkinPicID_ByteArray, num4);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num5 = 0x80;
                if (this.szSkinSoundResPack_ByteArray.GetLength(0) < num5)
                {
                    this.szSkinSoundResPack_ByteArray = new byte[LENGTH_szSkinSoundResPack];
                }
                type = srcBuf.readCString(ref this.szSkinSoundResPack_ByteArray, num5);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num6 = 0x80;
                if (this.szSoundSwitchEvent_ByteArray.GetLength(0) < num6)
                {
                    this.szSoundSwitchEvent_ByteArray = new byte[LENGTH_szSoundSwitchEvent];
                }
                type = srcBuf.readCString(ref this.szSoundSwitchEvent_ByteArray, num6);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsLimitSkin);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num7 = 0x80;
                if (this.szLimitQualityPic_ByteArray.GetLength(0) < num7)
                {
                    this.szLimitQualityPic_ByteArray = new byte[LENGTH_szLimitQualityPic];
                }
                type = srcBuf.readCString(ref this.szLimitQualityPic_ByteArray, num7);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num8 = 0x80;
                if (this.szLimitLabelPic_ByteArray.GetLength(0) < num8)
                {
                    this.szLimitLabelPic_ByteArray = new byte[LENGTH_szLimitLabelPic];
                }
                type = srcBuf.readCString(ref this.szLimitLabelPic_ByteArray, num8);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bSkinQuality);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyCoupons);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBuyCoupons);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuySkinCoin);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBuySkinCoin);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyDiamond);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBuyDiamond);
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
                type = srcBuf.readUInt8(ref this.bIsDiscount);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwDiscount);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwCombatAbility);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwChgItemCnt);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int i = 0; i < 15; i++)
                {
                    type = this.astAttr[i].load(ref srcBuf, cutVer);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt32(ref this.dwSortId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int j = 0; j < 5; j++)
                {
                    type = srcBuf.readUInt32(ref this.PromotionID[j]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt8(ref this.bIsShow);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bGetPathType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                int num11 = 0x100;
                if (this.szGetPath_ByteArray.GetLength(0) < num11)
                {
                    this.szGetPath_ByteArray = new byte[LENGTH_szGetPath];
                }
                type = srcBuf.readCString(ref this.szGetPath_ByteArray, num11);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsPresent);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwPresentHeadImg);
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
            this.szHeroName = StringHelper.UTF8BytesToString(ref this.szHeroName_ByteArray);
            this.szHeroName_ByteArray = null;
            this.szSkinName = StringHelper.UTF8BytesToString(ref this.szSkinName_ByteArray);
            this.szSkinName_ByteArray = null;
            this.szSkinDesc = StringHelper.UTF8BytesToString(ref this.szSkinDesc_ByteArray);
            this.szSkinDesc_ByteArray = null;
            this.szSkinPicID = StringHelper.UTF8BytesToString(ref this.szSkinPicID_ByteArray);
            this.szSkinPicID_ByteArray = null;
            this.szSkinSoundResPack = StringHelper.UTF8BytesToString(ref this.szSkinSoundResPack_ByteArray);
            this.szSkinSoundResPack_ByteArray = null;
            this.szSoundSwitchEvent = StringHelper.UTF8BytesToString(ref this.szSoundSwitchEvent_ByteArray);
            this.szSoundSwitchEvent_ByteArray = null;
            this.szLimitQualityPic = StringHelper.UTF8BytesToString(ref this.szLimitQualityPic_ByteArray);
            this.szLimitQualityPic_ByteArray = null;
            this.szLimitLabelPic = StringHelper.UTF8BytesToString(ref this.szLimitLabelPic_ByteArray);
            this.szLimitLabelPic_ByteArray = null;
            this.szGetPath = StringHelper.UTF8BytesToString(ref this.szGetPath_ByteArray);
            this.szGetPath_ByteArray = null;
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
            type = srcBuf.readUInt32(ref this.dwID);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                type = srcBuf.readUInt32(ref this.dwHeroID);
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
                if (dest > this.szHeroName_ByteArray.GetLength(0))
                {
                    if (dest > LENGTH_szHeroName)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szHeroName_ByteArray = new byte[dest];
                }
                if (1 > dest)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szHeroName_ByteArray, (int) dest);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szHeroName_ByteArray[((int) dest) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num2 = TdrTypeUtil.cstrlen(this.szHeroName_ByteArray) + 1;
                if (dest != num2)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readUInt32(ref this.dwSkinID);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
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
                if (num3 > this.szSkinName_ByteArray.GetLength(0))
                {
                    if (num3 > LENGTH_szSkinName)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szSkinName_ByteArray = new byte[num3];
                }
                if (1 > num3)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szSkinName_ByteArray, (int) num3);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szSkinName_ByteArray[((int) num3) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num4 = TdrTypeUtil.cstrlen(this.szSkinName_ByteArray) + 1;
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
                if (num5 > this.szSkinDesc_ByteArray.GetLength(0))
                {
                    if (num5 > LENGTH_szSkinDesc)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szSkinDesc_ByteArray = new byte[num5];
                }
                if (1 > num5)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szSkinDesc_ByteArray, (int) num5);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szSkinDesc_ByteArray[((int) num5) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num6 = TdrTypeUtil.cstrlen(this.szSkinDesc_ByteArray) + 1;
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
                if (num7 > this.szSkinPicID_ByteArray.GetLength(0))
                {
                    if (num7 > LENGTH_szSkinPicID)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szSkinPicID_ByteArray = new byte[num7];
                }
                if (1 > num7)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szSkinPicID_ByteArray, (int) num7);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szSkinPicID_ByteArray[((int) num7) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num8 = TdrTypeUtil.cstrlen(this.szSkinPicID_ByteArray) + 1;
                if (num7 != num8)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num9 = 0;
                type = srcBuf.readUInt32(ref num9);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num9 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num9 > this.szSkinSoundResPack_ByteArray.GetLength(0))
                {
                    if (num9 > LENGTH_szSkinSoundResPack)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szSkinSoundResPack_ByteArray = new byte[num9];
                }
                if (1 > num9)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szSkinSoundResPack_ByteArray, (int) num9);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szSkinSoundResPack_ByteArray[((int) num9) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num10 = TdrTypeUtil.cstrlen(this.szSkinSoundResPack_ByteArray) + 1;
                if (num9 != num10)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                uint num11 = 0;
                type = srcBuf.readUInt32(ref num11);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num11 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num11 > this.szSoundSwitchEvent_ByteArray.GetLength(0))
                {
                    if (num11 > LENGTH_szSoundSwitchEvent)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szSoundSwitchEvent_ByteArray = new byte[num11];
                }
                if (1 > num11)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szSoundSwitchEvent_ByteArray, (int) num11);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szSoundSwitchEvent_ByteArray[((int) num11) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num12 = TdrTypeUtil.cstrlen(this.szSoundSwitchEvent_ByteArray) + 1;
                if (num11 != num12)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readUInt8(ref this.bIsLimitSkin);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                uint num13 = 0;
                type = srcBuf.readUInt32(ref num13);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (num13 > srcBuf.getLeftSize())
                {
                    return TdrError.ErrorType.TDR_ERR_SHORT_BUF_FOR_READ;
                }
                if (num13 > this.szLimitQualityPic_ByteArray.GetLength(0))
                {
                    if (num13 > LENGTH_szLimitQualityPic)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szLimitQualityPic_ByteArray = new byte[num13];
                }
                if (1 > num13)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szLimitQualityPic_ByteArray, (int) num13);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szLimitQualityPic_ByteArray[((int) num13) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num14 = TdrTypeUtil.cstrlen(this.szLimitQualityPic_ByteArray) + 1;
                if (num13 != num14)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
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
                if (num15 > this.szLimitLabelPic_ByteArray.GetLength(0))
                {
                    if (num15 > LENGTH_szLimitLabelPic)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szLimitLabelPic_ByteArray = new byte[num15];
                }
                if (1 > num15)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szLimitLabelPic_ByteArray, (int) num15);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                if (this.szLimitLabelPic_ByteArray[((int) num15) - 1] != 0)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                int num16 = TdrTypeUtil.cstrlen(this.szLimitLabelPic_ByteArray) + 1;
                if (num15 != num16)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                }
                type = srcBuf.readUInt8(ref this.bSkinQuality);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyCoupons);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBuyCoupons);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuySkinCoin);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBuySkinCoin);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bIsBuyDiamond);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwBuyDiamond);
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
                type = srcBuf.readUInt8(ref this.bIsDiscount);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwDiscount);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwCombatAbility);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt32(ref this.dwChgItemCnt);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int i = 0; i < 15; i++)
                {
                    type = this.astAttr[i].unpack(ref srcBuf, cutVer);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt32(ref this.dwSortId);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                for (int j = 0; j < 5; j++)
                {
                    type = srcBuf.readUInt32(ref this.PromotionID[j]);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                }
                type = srcBuf.readUInt8(ref this.bIsShow);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                type = srcBuf.readUInt8(ref this.bGetPathType);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
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
                if (num19 > this.szGetPath_ByteArray.GetLength(0))
                {
                    if (num19 > LENGTH_szGetPath)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_BIG;
                    }
                    this.szGetPath_ByteArray = new byte[num19];
                }
                if (1 > num19)
                {
                    return TdrError.ErrorType.TDR_ERR_STR_LEN_TOO_SMALL;
                }
                type = srcBuf.readCString(ref this.szGetPath_ByteArray, (int) num19);
                if (type == TdrError.ErrorType.TDR_NO_ERROR)
                {
                    if (this.szGetPath_ByteArray[((int) num19) - 1] != 0)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                    }
                    int num20 = TdrTypeUtil.cstrlen(this.szGetPath_ByteArray) + 1;
                    if (num19 != num20)
                    {
                        return TdrError.ErrorType.TDR_ERR_STR_LEN_CONFLICT;
                    }
                    type = srcBuf.readUInt8(ref this.bIsPresent);
                    if (type != TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        return type;
                    }
                    type = srcBuf.readUInt32(ref this.dwPresentHeadImg);
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

