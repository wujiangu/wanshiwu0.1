namespace Assets.Scripts.Framework
{
    using CSProtocol;
    using System;
    using System.Runtime.InteropServices;
    using tsf4g_tdr_csharp;
    using UnityEngine;

    [MessageHandlerClass]
    public class FrameWindow : Singleton<FrameWindow>, IGameModule
    {
        private uint _aliveFlags;
        private int _aliveFrameCount;
        private uint _basFrqNo;
        private uint _begFrqNo;
        private ulong[] _cmdPingWin;
        private uint _maxFrqNo;
        private CSDT_FRAPBOOT_INFO[] _receiveWindow;
        private float _recentPingAvg;
        private uint _repairBegNo;
        private int _repairCounter;
        private int _repairTimes;
        private uint _sendCmdSeq;
        private bool _showChart;
        private int _timeoutCounter;
        private int _timeoutFrameOffset;
        private int _timeoutFrameStep;
        private int _timeoutTimes;
        private const int ALIVE_THRESHOLD_FRAME_OF32 = 10;
        public const int CMD_PING_MAX_VAL = 0x535;
        public const int CMD_PING_MIN_VAL = 0x21;
        public const int CMD_PING_WIN_LEN = 100;
        public const uint FRQ_WIN_LEN = 900;
        public const int MAX_TIMEOUT_TIMES = 5;

        public FrameWindow()
        {
            this.Reset();
        }

        private CSDT_FRAPBOOT_INFO _FetchFBI(uint frqNo)
        {
            uint index = this._FrqNoToWinIdx_(frqNo);
            CSDT_FRAPBOOT_INFO csdt_frapboot_info = this._receiveWindow[index];
            this._receiveWindow[index] = null;
            return csdt_frapboot_info;
        }

        private uint _FrqNoToWinIdx_(uint theFrqNo)
        {
            return ((theFrqNo - this._basFrqNo) % 900);
        }

        public bool GetPings(uint cmdSeq, out uint recvPing, out uint hadlPing)
        {
            if (((0 < cmdSeq) && (cmdSeq <= this.CurSendCmdSeq)) && ((cmdSeq + this._cmdPingWin.Length) > this.CurSendCmdSeq))
            {
                ulong num = this._cmdPingWin[(int) ((IntPtr) (((ulong) cmdSeq) % ((long) this._cmdPingWin.Length)))];
                recvPing = (uint) (num & 0xffffffffL);
                if ((recvPing & 1) > 0)
                {
                    recvPing = recvPing >> 1;
                }
                else
                {
                    recvPing = 0;
                }
                hadlPing = (uint) (num >> 0x20);
                if ((hadlPing & 1) > 0)
                {
                    hadlPing = hadlPing >> 1;
                }
                else
                {
                    hadlPing = 0;
                }
                return true;
            }
            recvPing = 0;
            hadlPing = 0;
            return false;
        }

        private void HandleAIChgSyncCommand(uint dwFrqNo, CSDT_FRAPBOOT_AISTATE AIState)
        {
            IFrameCommand command = null;
            FrameCommand<AutoAIChgCommand> command2 = FrameCommandFactory.CreateFrameCommand<AutoAIChgCommand>();
            command2.cmdData.m_autoType = AIState.bType;
            command2.cmdData.m_playerID = AIState.dwPlayerObjID;
            command = command2;
            if (command != null)
            {
                command.playerID = AIState.dwPlayerObjID;
                command.frameNum = dwFrqNo;
                Singleton<FrameSynchr>.GetInstance().PushFrameCommand(command);
            }
        }

        private void HandleAssistChgSyncCommand(uint dwFrqNo, CSDT_FRAPBOOT_ASSISTSTATE assistChgState)
        {
            IFrameCommand command = null;
            FrameCommand<AssistStateChgCommand> command2 = FrameCommandFactory.CreateFrameCommand<AssistStateChgCommand>();
            command2.cmdData.m_chgType = assistChgState.bType;
            command2.cmdData.m_aiPlayerID = assistChgState.dwAiPlayerObjID;
            command2.cmdData.m_masterPlayerID = assistChgState.dwMasterObjID;
            command = command2;
            if (command != null)
            {
                command.playerID = assistChgState.dwAiPlayerObjID;
                command.frameNum = dwFrqNo;
                Singleton<FrameSynchr>.GetInstance().PushFrameCommand(command);
            }
        }

        private void HandleClientClientSyncCommand(uint dwFrqNo, CSDT_FRAPBOOT_CC ccSynDt)
        {
            int usedSize = 0;
            FRAME_CMD_PKG msg = new FRAME_CMD_PKG();
            TdrError.ErrorType type = msg.unpack(ref ccSynDt.stSyncInfo.szBuff, ccSynDt.stSyncInfo.wLen, ref usedSize, 0);
            DebugHelper.Assert(type == TdrError.ErrorType.TDR_NO_ERROR);
            if (type == TdrError.ErrorType.TDR_NO_ERROR)
            {
                IFrameCommand command = FrameCommandFactory.CreateFrameCommand(ref msg);
                if (command != null)
                {
                    command.playerID = ccSynDt.dwObjID;
                    command.frameNum = dwFrqNo;
                    Singleton<FrameSynchr>.GetInstance().PushFrameCommand(command);
                }
            }
        }

        private void HandleClientServerSyncCommand(uint dwFrqNo, CSDT_FRAPBOOT_CS csSynDt)
        {
            IFrameCommand command = FrameCommandFactory.CreateFrameCommandByCSSyncInfo(ref csSynDt.stSyncInfo);
            if (command != null)
            {
                command.playerID = csSynDt.dwObjID;
                command.frameNum = dwFrqNo;
                Singleton<FrameSynchr>.GetInstance().PushFrameCommand(command);
            }
        }

        private void HandleClientStateSyncCommand(uint dwFrqNo, CSDT_FRAPBOOT_ACNTSTATE stateSyncDt)
        {
            IFrameCommand command = null;
            switch (stateSyncDt.bStateChgType)
            {
                case 1:
                    command = FrameCommandFactory.CreateFrameCommand<SvrDisconnectCommand>();
                    break;

                case 2:
                    command = FrameCommandFactory.CreateFrameCommand<SvrReconnectCommand>();
                    break;

                case 3:
                    command = FrameCommandFactory.CreateFrameCommand<SvrRunawayCommand>();
                    break;
            }
            if (command != null)
            {
                command.playerID = stateSyncDt.dwObjID;
                command.frameNum = dwFrqNo;
                Singleton<FrameSynchr>.GetInstance().PushFrameCommand(command);
            }
        }

        private void HandleFraqBootInfo(CSDT_FRAPBOOT_INFO fbid)
        {
            this.RecordPing(fbid, false);
            if (GameSettings.enableReplay)
            {
                this.RecordReplay(fbid);
            }
            if (Singleton<FrameSynchr>.GetInstance().SetKeyFrameIndex(fbid.dwKFrapsNo))
            {
                for (int i = 0; i < fbid.bNum; i++)
                {
                    CSDT_FRAPBOOT_DETAIL csdt_frapboot_detail = fbid.astBootInfo[i];
                    switch (csdt_frapboot_detail.bType)
                    {
                        case 1:
                            this.HandleClientClientSyncCommand(fbid.dwKFrapsNo, csdt_frapboot_detail.stDetail.stCCBoot);
                            break;

                        case 2:
                            this.HandleClientServerSyncCommand(fbid.dwKFrapsNo, csdt_frapboot_detail.stDetail.stCSBoot);
                            break;

                        case 3:
                            this.HandleClientStateSyncCommand(fbid.dwKFrapsNo, csdt_frapboot_detail.stDetail.stAcntState);
                            break;

                        case 4:
                            this.HandleAssistChgSyncCommand(fbid.dwKFrapsNo, csdt_frapboot_detail.stDetail.stAssistState);
                            break;

                        case 5:
                            this.HandleAIChgSyncCommand(fbid.dwKFrapsNo, csdt_frapboot_detail.stDetail.stAiState);
                            break;
                    }
                }
            }
        }

        private static bool HandleFraqBootSingle(SCPKG_FRAPBOOT_SINGLE fbi)
        {
            CSDT_FRAPBOOT_INFO fbid = CSDT_FRAPBOOT_INFO.New();
            int usedSize = 0;
            return (((fbid.unpack(ref fbi.szInfoBuff, fbi.wLen, ref usedSize, 0) == TdrError.ErrorType.TDR_NO_ERROR) && (usedSize > 0)) && Singleton<FrameWindow>.GetInstance().SetFrqWin(fbid));
        }

        [MessageHandler(0x40b)]
        public static void onFrapBootInfoMultipleNtf(CSPkg msg)
        {
            SCPKG_FRAPBOOTINFO stFrapBootInfo = msg.stPkgData.stFrapBootInfo;
            for (int i = 0; i < stFrapBootInfo.bSpareNum; i++)
            {
                if (HandleFraqBootSingle(stFrapBootInfo.astSpareFrap[i]))
                {
                    MonoSingleton<Reconnection>.GetInstance().UpdateCachedLen(msg);
                    break;
                }
            }
            msg.Release();
        }

        [MessageHandler(0x40a)]
        public static void onFrapBootInfoSingleNtf(CSPkg msg)
        {
            if (HandleFraqBootSingle(msg.stPkgData.stFrapBootSingle))
            {
                MonoSingleton<Reconnection>.GetInstance().UpdateCachedLen(msg);
            }
            msg.Release();
        }

        private void RecordPing(CSDT_FRAPBOOT_INFO fbid, bool recvOrHand)
        {
        }

        private void RecordReplay(CSDT_FRAPBOOT_INFO fbid)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x40a);
            SCPKG_FRAPBOOT_SINGLE stFrapBootSingle = msg.stPkgData.stFrapBootSingle;
            int usedSize = 0;
            if (fbid.pack(ref stFrapBootSingle.szInfoBuff, stFrapBootSingle.szInfoBuff.Length, ref usedSize, 0) == TdrError.ErrorType.TDR_NO_ERROR)
            {
                stFrapBootSingle.wLen = (ushort) usedSize;
                Singleton<GameReplayModule>.instance.RecordMsg(msg);
            }
            msg.Release();
        }

        private void RefreshTimeout()
        {
            this._aliveFrameCount = Time.frameCount;
            this._timeoutCounter = 0;
            this._aliveFlags |= ((uint) 1) << (Time.frameCount % 0x20);
            if (this._timeoutTimes > 0)
            {
                int num = 0;
                for (int i = 0; i < 0x20; i++)
                {
                    if ((this._aliveFlags & (((int) 1) << i)) > 0)
                    {
                        num++;
                    }
                }
                if (num > 10)
                {
                    this._timeoutTimes = 0;
                }
            }
        }

        private void RequestRepairFraqBootInfo()
        {
            if (this._maxFrqNo > this._begFrqNo)
            {
                CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x40c);
                CSPKG_REQUESTFRAPBOOTSINGLE stReqFrapBootSingle = msg.stPkgData.stReqFrapBootSingle;
                stReqFrapBootSingle.bNum = 0;
                for (uint i = 0; i < 900; i++)
                {
                    if ((stReqFrapBootSingle.bNum >= stReqFrapBootSingle.KFrapsNo.Length) || (stReqFrapBootSingle.bNum > ((this._maxFrqNo - this._begFrqNo) + 2)))
                    {
                        break;
                    }
                    uint theFrqNo = this._begFrqNo + i;
                    if (this._receiveWindow[this._FrqNoToWinIdx_(theFrqNo)] == null)
                    {
                        byte num3;
                        stReqFrapBootSingle.bNum = (byte) ((num3 = stReqFrapBootSingle.bNum) + 1);
                        stReqFrapBootSingle.KFrapsNo[num3] = theFrqNo;
                    }
                }
                Singleton<NetworkModule>.GetInstance().SendGameMsg(ref msg, 0);
            }
        }

        private void RequestTimeoutFraqBootInfo()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x40d);
            msg.stPkgData.stReqFrapBootTimeout.dwCurKFrapsNo = this._begFrqNo;
            Singleton<NetworkModule>.GetInstance().SendGameMsg(ref msg, 0);
        }

        public void Reset()
        {
            this._sendCmdSeq = 0;
            this._receiveWindow = new CSDT_FRAPBOOT_INFO[900];
            this._basFrqNo = 0;
            this._begFrqNo = 1;
            this._maxFrqNo = 0;
            this._repairCounter = 0;
            this._repairBegNo = 0;
            this._repairTimes = 0;
            this._timeoutCounter = 0;
            this._timeoutTimes = 0;
            this._aliveFlags = 0;
            this._aliveFrameCount = 0;
            this._cmdPingWin = new ulong[100];
            this._recentPingAvg = 33f;
            this._timeoutFrameStep = 3;
            this._timeoutFrameOffset = 1;
        }

        public bool SendGameCmd(IFrameCommand cmd, bool bMultiGame, bool bUseCSSync = false)
        {
            if (Singleton<NetworkModule>.GetInstance().isOnlineMode)
            {
                if (bMultiGame)
                {
                    if (Singleton<NetworkModule>.GetInstance().gameSvr.connected)
                    {
                        CSPkg msg = null;
                        TdrError.ErrorType type = TdrError.ErrorType.TDR_NO_ERROR;
                        if (!bUseCSSync)
                        {
                            msg = NetworkModule.CreateDefaultCSPKG(0x3ed);
                            FRAME_CMD_PKG frame_cmd_pkg = FrameCommandFactory.CreateCommandPKG(cmd);
                            msg.stPkgData.stGamingCCSync.dwCmdSeq = this.NewSendCmdSeq;
                            msg.stPkgData.stGamingCCSync.stSyncInfo.construct();
                            cmd.TransProtocol(ref frame_cmd_pkg);
                            int usedSize = 0;
                            type = frame_cmd_pkg.pack(ref msg.stPkgData.stGamingCCSync.stSyncInfo.szBuff, 0x40, ref usedSize, 0);
                            msg.stPkgData.stGamingCCSync.stSyncInfo.wLen = (ushort) usedSize;
                        }
                        else
                        {
                            msg = NetworkModule.CreateDefaultCSPKG(0x3ec);
                            msg.stPkgData.stGamingCSSync.dwCmdSeq = this.NewSendCmdSeq;
                            msg.stPkgData.stGamingCSSync.stSyncInfo.stCSSyncDt.construct((long) cmd.cmdType);
                            cmd.TransProtocol(ref msg.stPkgData.stGamingCSSync.stSyncInfo);
                        }
                        DebugHelper.Assert(type == TdrError.ErrorType.TDR_NO_ERROR);
                        Singleton<NetworkModule>.GetInstance().gameSvr.PushSendMsg(msg);
                        Singleton<NetworkModule>.GetInstance().HandleGameMsgSend();
                        ulong num2 = (ulong) Mathf.RoundToInt(Time.time * 1000f);
                        this._cmdPingWin[(int) ((IntPtr) (((ulong) this.CurSendCmdSeq) % ((long) this._cmdPingWin.Length)))] = (num2 << 1) | (num2 << 0x21);
                        if (this._showChart)
                        {
                            MonoSingleton<RealTimeChart>.instance.AddSample("FrameCmdRecvPing", 1001f);
                            MonoSingleton<RealTimeChart>.instance.AddSample("FrameCmdHandPing", 1001f);
                            MonoSingleton<RealTimeChart>.instance.AddSample("FrameCmdAvrgPing", this._recentPingAvg);
                        }
                        cmd.Preprocess();
                    }
                }
                else
                {
                    Singleton<FrameSynchr>.GetInstance().PushFrameCommand(cmd);
                }
            }
            return true;
        }

        private bool SetFrqWin(CSDT_FRAPBOOT_INFO fbid)
        {
            if (this._aliveFrameCount != Time.frameCount)
            {
                this.RefreshTimeout();
            }
            bool flag = false;
            uint dwKFrapsNo = fbid.dwKFrapsNo;
            if (dwKFrapsNo > this._maxFrqNo)
            {
                this._maxFrqNo = dwKFrapsNo;
            }
            if ((this._begFrqNo <= dwKFrapsNo) && (dwKFrapsNo < (this._begFrqNo + 900)))
            {
                this._receiveWindow[this._FrqNoToWinIdx_(dwKFrapsNo)] = fbid;
                if (Singleton<FrameSynchr>.GetInstance().bActive)
                {
                    CSDT_FRAPBOOT_INFO csdt_frapboot_info = null;
                    while ((csdt_frapboot_info = this._FetchFBI(this._begFrqNo)) != null)
                    {
                        if ((++this._begFrqNo % 900) == 0)
                        {
                            this._basFrqNo = this._begFrqNo;
                        }
                        this.HandleFraqBootInfo(csdt_frapboot_info);
                        csdt_frapboot_info.Release();
                        flag = true;
                    }
                }
                return flag;
            }
            if (dwKFrapsNo > this._begFrqNo)
            {
                MonoSingleton<Reconnection>.GetInstance().RequestRelaySyncCacheFrames(false);
            }
            return flag;
        }

        public void ToggleShowPingChart()
        {
            this._showChart = !this._showChart;
            if (this._showChart)
            {
                MonoSingleton<RealTimeChart>.instance.isVisible = true;
                MonoSingleton<RealTimeChart>.instance.AddTrack("FrameCmdRecvPing", Color.yellow, true, 0f, 1000f).isVisiable = true;
                MonoSingleton<RealTimeChart>.instance.AddTrack("FrameCmdHandPing", Color.green, true, 0f, 1000f).isVisiable = true;
                MonoSingleton<RealTimeChart>.instance.AddTrack("FrameCmdAvrgPing", Color.blue, true, 0f, 1000f).isVisiable = true;
            }
            else
            {
                MonoSingleton<RealTimeChart>.instance.isVisible = false;
                MonoSingleton<RealTimeChart>.instance.RemoveTrack("FrameCmdRecvPing");
                MonoSingleton<RealTimeChart>.instance.RemoveTrack("FrameCmdHandPing");
                MonoSingleton<RealTimeChart>.instance.RemoveTrack("FrameCmdAvrgPing");
            }
        }

        public void UpdateFrame()
        {
            this._aliveFlags &= (uint) ~(((int) 1) << (Time.frameCount % 0x20));
            if (Singleton<FrameSynchr>.GetInstance().bActive && !MonoSingleton<Reconnection>.GetInstance().isProcessingRelayRecover)
            {
                if ((this._maxFrqNo > this._begFrqNo) && (this._receiveWindow[this._FrqNoToWinIdx_(this._begFrqNo)] == null))
                {
                    if (this._repairBegNo != this._begFrqNo)
                    {
                        this._repairBegNo = this._begFrqNo;
                        this.RequestRepairFraqBootInfo();
                        this._repairTimes = 0;
                        this._repairCounter = 0;
                    }
                    else if (++this._repairCounter > ((2 ^ this._repairTimes) * this._timeoutFrameStep))
                    {
                        this.RequestRepairFraqBootInfo();
                        this._repairCounter = 0;
                        this._repairTimes++;
                    }
                }
                if (((this._timeoutTimes < 5) && (this._begFrqNo > 1)) && (++this._timeoutCounter > (((2 ^ this._timeoutTimes) * this._timeoutFrameStep) + this._timeoutFrameOffset)))
                {
                    this.RequestTimeoutFraqBootInfo();
                    this._timeoutCounter = 0;
                    this._timeoutTimes++;
                }
            }
        }

        public uint BeginCmdPingSeq
        {
            get
            {
                return ((this.CurSendCmdSeq <= this._cmdPingWin.Length) ? 1 : ((uint) ((this.CurSendCmdSeq - this._cmdPingWin.Length) + 1)));
            }
        }

        public uint CurSendCmdSeq
        {
            get
            {
                return this._sendCmdSeq;
            }
        }

        private uint NewSendCmdSeq
        {
            get
            {
                return ++this._sendCmdSeq;
            }
        }
    }
}

