﻿namespace ResData
{
    using System;
    using tsf4g_tdr_csharp;

    public class ResTalentRule : IUnpackable, tsf4g_csharp_interface
    {
        public static readonly uint BASEVERSION = 1;
        public byte bID;
        public byte bSoulRequestValue;
        public static readonly uint CURRVERSION = 1;

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
            type = srcBuf.readUInt8(ref this.bID);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                type = srcBuf.readUInt8(ref this.bSoulRequestValue);
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
            type = srcBuf.readUInt8(ref this.bID);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                type = srcBuf.readUInt8(ref this.bSoulRequestValue);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                this.TransferData();
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

