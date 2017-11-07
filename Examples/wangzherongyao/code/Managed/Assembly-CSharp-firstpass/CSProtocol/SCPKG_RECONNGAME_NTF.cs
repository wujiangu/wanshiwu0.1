﻿namespace CSProtocol
{
    using Assets.Scripts.Common;
    using System;
    using tsf4g_tdr_csharp;

    public class SCPKG_RECONNGAME_NTF : ProtocolObject
    {
        public static readonly uint BASEVERSION = 1;
        public byte bState;
        public static readonly int CLASS_ID = 0x3a9;
        public static readonly uint CURRVERSION = 0x51;
        public uint dwSelfObjID;
        public ReconnStateInfo stStateData = ((ReconnStateInfo) ProtocolObjectPool.Get(ReconnStateInfo.CLASS_ID));

        public override TdrError.ErrorType construct()
        {
            TdrError.ErrorType type = TdrError.ErrorType.TDR_NO_ERROR;
            this.dwSelfObjID = 0;
            this.bState = 0;
            long bState = this.bState;
            type = this.stStateData.construct(bState);
            if (type != TdrError.ErrorType.TDR_NO_ERROR)
            {
                return type;
            }
            return type;
        }

        public override int GetClassID()
        {
            return CLASS_ID;
        }

        public override void OnRelease()
        {
            this.dwSelfObjID = 0;
            this.bState = 0;
            if (this.stStateData != null)
            {
                this.stStateData.Release();
                this.stStateData = null;
            }
        }

        public override void OnUse()
        {
            this.stStateData = (ReconnStateInfo) ProtocolObjectPool.Get(ReconnStateInfo.CLASS_ID);
        }

        public override TdrError.ErrorType pack(ref TdrWriteBuf destBuf, uint cutVer)
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
            type = destBuf.writeUInt32(this.dwSelfObjID);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                type = destBuf.writeUInt8(this.bState);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                long bState = this.bState;
                type = this.stStateData.pack(bState, ref destBuf, cutVer);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
            }
            return type;
        }

        public TdrError.ErrorType pack(ref byte[] buffer, int size, ref int usedSize, uint cutVer)
        {
            if (((buffer == null) || (buffer.GetLength(0) == 0)) || (size > buffer.GetLength(0)))
            {
                return TdrError.ErrorType.TDR_ERR_INVALID_BUFFER_PARAMETER;
            }
            TdrWriteBuf destBuf = ClassObjPool<TdrWriteBuf>.Get();
            destBuf.set(ref buffer, size);
            TdrError.ErrorType type = this.pack(ref destBuf, cutVer);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                buffer = destBuf.getBeginPtr();
                usedSize = destBuf.getUsedSize();
            }
            destBuf.Release();
            return type;
        }

        public override TdrError.ErrorType unpack(ref TdrReadBuf srcBuf, uint cutVer)
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
            type = srcBuf.readUInt32(ref this.dwSelfObjID);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                type = srcBuf.readUInt8(ref this.bState);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
                }
                long bState = this.bState;
                type = this.stStateData.unpack(bState, ref srcBuf, cutVer);
                if (type != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    return type;
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
            TdrReadBuf srcBuf = ClassObjPool<TdrReadBuf>.Get();
            srcBuf.set(ref buffer, size);
            TdrError.ErrorType type = this.unpack(ref srcBuf, cutVer);
            usedSize = srcBuf.getUsedSize();
            srcBuf.Release();
            return type;
        }
    }
}

