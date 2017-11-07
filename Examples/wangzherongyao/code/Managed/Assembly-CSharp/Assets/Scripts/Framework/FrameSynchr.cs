namespace Assets.Scripts.Framework
{
    using CSProtocol;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    [MessageHandlerClass]
    public class FrameSynchr : Singleton<FrameSynchr>, IGameModule
    {
        private bool _bActive;
        private int _CurPkgDelay;
        private int AvgFrameDelay;
        private uint backstepFrameCounter;
        public bool bEscape;
        public bool bRunning;
        public bool bShowJitterChart;
        public uint CacheSetLater;
        private Queue<IFrameCommand> commandQueue = new Queue<IFrameCommand>();
        private uint EndBlockWaitNum;
        private const int FrameDelay_Limit = 200;
        public uint FrameDelta = 0x42;
        public uint FrameSpeed = 1;
        public int GameSvrPing;
        private const float JitterCoverage = 0.85f;
        private int JitterDamper;
        private int JitterDelay;
        private uint KeyFrameRate = 1;
        public int m_Abnormal_PingCount;
        public uint m_AvePing;
        public uint m_LastPing;
        public uint m_MaxPing;
        public uint m_MinPing = uint.MaxValue;
        public Ping m_ping;
        public int m_PingAverage;
        public uint m_PingIdx;
        private uint[] m_PingList = new uint[15];
        public List<uint> m_pingRecords = new List<uint>();
        public float m_PingTimeBegin;
        private int[] m_pingTokenList = new int[] { 50, 100, 150, 200, 250, 300, 350, 400, 500, 600, 700, 800, 900, 0x3e8 };
        public int m_PingVariance;
        public List<uint> m_pingWobble = new List<uint>();
        public uint m_realpingStartTime;
        public int nDriftFactor = 4;
        public uint PreActFrames = 5;
        private uint ServerSeed = 0x3039;
        public float startFrameTime;
        private const int StatDelayCnt = 30;
        private int StatDelayIdx;
        private int[] StatDelaySet = new int[30];
        public uint SvrFrameDelta = 0x42;
        private uint SvrFrameIndex;
        public uint SvrFrameLater;
        public int tryCount;
        private uint uCommandId;

        public void CacheFrameLater(uint frameLater)
        {
            this.CacheSetLater = frameLater;
        }

        public void CalcBackstepTimeSinceStart(uint inSvrNum)
        {
            if (!Singleton<GameReplayModule>.instance.isReplay && (this.backstepFrameCounter != inSvrNum))
            {
                float realtimeSinceStartup = Time.realtimeSinceStartup;
                ulong num2 = inSvrNum * this.SvrFrameDelta;
                float num3 = realtimeSinceStartup - (num2 * 0.001f);
                float num4 = num3 - this.startFrameTime;
                if (num4 < 0f)
                {
                    this.startFrameTime = num3;
                }
                this.backstepFrameCounter = inSvrNum;
            }
        }

        public int CalculateJitterDelay(long nDelayMs)
        {
            this._CurPkgDelay = (nDelayMs <= 0L) ? 0 : ((int) nDelayMs);
            if (this.AvgFrameDelay < 0)
            {
                this.AvgFrameDelay = this._CurPkgDelay;
            }
            else
            {
                this.AvgFrameDelay = ((0x1d * this.AvgFrameDelay) + this._CurPkgDelay) / 30;
            }
            if (this.bShowJitterChart)
            {
                MonoSingleton<RealTimeChart>.instance.AddSample("NetPkgDelay", (float) this._CurPkgDelay);
                MonoSingleton<RealTimeChart>.instance.AddSample("NetAvgDelay", (float) this.AvgFrameDelay);
            }
            return this.AvgFrameDelay;
        }

        private void ClearSavePingList()
        {
            for (int i = 0; i < this.m_PingList.Length; i++)
            {
                this.m_PingList[i] = 0;
            }
        }

        private CreatorDelegate GetCreator(System.Type InType)
        {
            MethodInfo[] methods = InType.GetMethods();
            for (int i = 0; (methods != null) && (i < methods.Length); i++)
            {
                MethodInfo info = methods[i];
                if (info.IsStatic)
                {
                    object[] customAttributes = info.GetCustomAttributes(typeof(FrameCommandCreatorAttribute), true);
                    for (int j = 0; j < customAttributes.Length; j++)
                    {
                        if (customAttributes[j] is FrameCommandCreatorAttribute)
                        {
                            return (CreatorDelegate) Delegate.CreateDelegate(typeof(CreatorDelegate), info);
                        }
                    }
                }
            }
            return null;
        }

        private CreatorCSSyncDelegate GetCSSyncCreator(System.Type InType)
        {
            MethodInfo[] methods = InType.GetMethods();
            for (int i = 0; (methods != null) && (i < methods.Length); i++)
            {
                MethodInfo info = methods[i];
                if (info.IsStatic)
                {
                    object[] customAttributes = info.GetCustomAttributes(typeof(FrameCommandCreatorAttribute), true);
                    for (int j = 0; j < customAttributes.Length; j++)
                    {
                        if (customAttributes[j] is FrameCommandCreatorAttribute)
                        {
                            return (CreatorCSSyncDelegate) Delegate.CreateDelegate(typeof(CreatorCSSyncDelegate), info);
                        }
                    }
                }
            }
            return null;
        }

        public override void Init()
        {
            FrameCommandFactory.PrepareRegisterCommand();
            System.Type[] types = typeof(FrameSynchr).Assembly.GetTypes();
            for (int i = 0; (types != null) && (i < types.Length); i++)
            {
                System.Type inType = types[i];
                object[] customAttributes = inType.GetCustomAttributes(typeof(FrameCommandClassAttribute), true);
                for (int j = 0; j < customAttributes.Length; j++)
                {
                    FrameCommandClassAttribute attribute = customAttributes[j] as FrameCommandClassAttribute;
                    if (attribute != null)
                    {
                        CreatorDelegate creator = this.GetCreator(inType);
                        if (creator != null)
                        {
                            FrameCommandFactory.RegisterCommandCreator(attribute.ID, inType, creator);
                        }
                    }
                }
                object[] objArray2 = inType.GetCustomAttributes(typeof(FrameCSSYNCCommandClassAttribute), true);
                for (int k = 0; k < objArray2.Length; k++)
                {
                    FrameCSSYNCCommandClassAttribute attribute2 = objArray2[k] as FrameCSSYNCCommandClassAttribute;
                    if (attribute2 != null)
                    {
                        CreatorCSSyncDelegate cSSyncCreator = this.GetCSSyncCreator(inType);
                        if (cSSyncCreator != null)
                        {
                            FrameCommandFactory.RegisterCSSyncCommandCreator(attribute2.ID, inType, cSSyncCreator);
                        }
                    }
                }
            }
            this.ResetSynchr();
        }

        [MessageHandler(0x4ec)]
        public static void onRelaySvrPingMsg(CSPkg msg)
        {
            if (Singleton<FrameSynchr>.instance.bActive && Singleton<FrameSynchr>.instance.bRunning)
            {
                uint num = ((uint) (Time.realtimeSinceStartup * 1000f)) - msg.stPkgData.stRelaySvrPing.dwTime;
                Singleton<FrameSynchr>.instance.m_pingRecords.Add((uint) Mathf.Clamp((float) num, 0f, 1000f));
                FrameSynchr instance = Singleton<FrameSynchr>.instance;
                instance.m_PingIdx++;
                Singleton<FrameSynchr>.instance.m_AvePing = ((Singleton<FrameSynchr>.instance.m_AvePing * (Singleton<FrameSynchr>.instance.m_PingIdx - 1)) + num) / Singleton<FrameSynchr>.instance.m_PingIdx;
                Singleton<FrameSynchr>.instance.m_pingWobble.Add((uint) Mathf.Abs((float) (num - Singleton<FrameSynchr>.instance.m_AvePing)));
                if (num > Singleton<FrameSynchr>.instance.m_MaxPing)
                {
                    Singleton<FrameSynchr>.instance.m_MaxPing = num;
                }
                if (num < Singleton<FrameSynchr>.instance.m_MinPing)
                {
                    Singleton<FrameSynchr>.instance.m_MinPing = num;
                }
                Singleton<FrameSynchr>.instance.GameSvrPing = (((int) num) + Singleton<FrameSynchr>.instance.GameSvrPing) / 2;
                if ((Time.time - Singleton<FrameSynchr>.instance.m_PingTimeBegin) > 5f)
                {
                    Singleton<FrameSynchr>.instance.m_PingTimeBegin = Time.time;
                    if (Singleton<FrameSynchr>.instance.m_LastPing == 0)
                    {
                        Singleton<FrameSynchr>.instance.m_LastPing = num;
                    }
                    if (Math.Abs((long) (Singleton<FrameSynchr>.instance.m_LastPing - num)) > 100L)
                    {
                        FrameSynchr local2 = Singleton<FrameSynchr>.instance;
                        local2.m_Abnormal_PingCount++;
                        Singleton<FrameSynchr>.instance.m_LastPing = num;
                    }
                }
                if ((Singleton<FrameSynchr>.instance.m_pingRecords.Count > 100) && (Singleton<FrameSynchr>.instance.GameSvrPing > 300))
                {
                    RealPing();
                }
                Singleton<FrameSynchr>.instance.PrepareReportPingToBeacon(Singleton<FrameSynchr>.instance.GameSvrPing);
            }
            msg.Release();
        }

        public void PrepareReportPingToBeacon(int curPing)
        {
            int length = this.m_pingTokenList.Length;
            int index = 0;
            index = 0;
            while (index < length)
            {
                if (curPing < this.m_pingTokenList[index])
                {
                    this.m_PingList[index]++;
                    break;
                }
                index++;
            }
            if (index == length)
            {
                this.m_PingList[length]++;
            }
        }

        public void PushFrameCommand(IFrameCommand command)
        {
            command.cmdId = this.NewCommandId;
            if (this.bActive)
            {
                command.OnReceive();
            }
            else
            {
                command.frameNum = this.CurFrameNum;
            }
            this.commandQueue.Enqueue(command);
        }

        private static void RealPing()
        {
            if (((Singleton<FrameSynchr>.instance.m_realpingStartTime <= 0) && (Singleton<FrameSynchr>.instance.m_ping == null)) && !string.IsNullOrEmpty(ApolloConfig.loginOnlyIp))
            {
                Singleton<FrameSynchr>.instance.m_realpingStartTime = (uint) (Time.realtimeSinceStartup * 1000f);
                Singleton<FrameSynchr>.instance.m_ping = new Ping(ApolloConfig.loginOnlyIp);
            }
        }

        public void ReportPingToBeacon()
        {
            this.m_PingAverage = 0;
            this.m_PingVariance = 0;
            if ((this.m_pingRecords != null) && (this.m_pingRecords.Count > 100))
            {
                double num = 0.0;
                double num2 = 0.0;
                for (int i = 0; i < this.m_pingRecords.Count; i++)
                {
                    num += (double) this.m_pingRecords[i];
                }
                num2 = num / ((double) this.m_pingRecords.Count);
                int num4 = Mathf.FloorToInt(((float) num2) / 100f) * 100;
                this.m_PingAverage = num4;
                Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("PingAverage_" + num4.ToString(), null, true);
                num = 0.0;
                for (int j = 0; j < this.m_pingRecords.Count; j++)
                {
                    num += Math.Pow(((double) this.m_pingRecords[j]) - num2, 2.0);
                }
                num2 = num / ((double) this.m_pingRecords.Count);
                num4 = Mathf.FloorToInt(((float) num2) / 1000f) * 0x3e8;
                this.m_PingVariance = num4;
                if (num4 <= 0x2710)
                {
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("PingVariance_" + num4.ToString(), null, true);
                }
                else
                {
                    Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("PingVariance_>10000", null, true);
                }
            }
            if ((this.m_pingWobble != null) && (this.m_pingWobble.Count > 100))
            {
                int num6 = 0;
                for (int k = 0; k < this.m_pingWobble.Count; k++)
                {
                    if (Mathf.Abs((float) ((double) this.m_pingWobble[k])) <= 100f)
                    {
                        num6++;
                    }
                }
                int num8 = Mathf.FloorToInt(((((float) num6) / ((float) this.m_pingWobble.Count)) * 100f) / 10f) * 10;
                Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("PingWobble<100_" + num8.ToString() + "%", null, true);
            }
        }

        private void ReqKeyFrameLaterModify(uint nLater)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x448);
            msg.stPkgData.stKFrapsLaterChgReq.bKFrapsLater = (byte) nLater;
            Singleton<NetworkModule>.instance.SendGameMsg(ref msg, 0);
        }

        public void ResetSynchr()
        {
            this.bActive = false;
            this.SetSynchrRunning(true);
            this.FrameDelta = 0x42;
            this.CurFrameNum = 0;
            this.EndFrameNum = 0;
            this.LogicFrameTick = 0L;
            this.EndBlockWaitNum = 0;
            this.PreActFrames = 5;
            this.SvrFrameDelta = this.FrameDelta;
            this.SvrFrameLater = 0;
            this.SvrFrameIndex = 0;
            this.CacheSetLater = 0;
            this.KeyFrameRate = 1;
            this.FrameSpeed = 1;
            this.GameSvrPing = 0;
            this._CurPkgDelay = 0;
            this.AvgFrameDelay = 0;
            this.JitterDelay = 0;
            this.JitterDamper = 0;
            this.StatDelayIdx = 0;
            Array.Clear(this.StatDelaySet, 0, this.StatDelaySet.Length);
            this.ShowJitterChart(false, 0);
            this.NewCommandId = 0;
            this.startFrameTime = Time.time;
            this.backstepFrameCounter = 0;
            this.commandQueue.Clear();
            this.m_MaxPing = 0;
            this.m_MinPing = uint.MaxValue;
            this.m_AvePing = 0;
            this.m_PingIdx = 0;
            this.m_pingRecords.Clear();
            this.m_pingWobble.Clear();
            this.m_realpingStartTime = 0;
            this.m_ping = null;
            this.m_PingTimeBegin = 0f;
            this.m_LastPing = 0;
            this.m_Abnormal_PingCount = 0;
            this.ClearSavePingList();
        }

        public bool SetKeyFrameIndex(uint svrNum)
        {
            this.SvrFrameIndex = svrNum;
            this.EndFrameNum = (svrNum + this.SvrFrameLater) * this.KeyFrameRate;
            this.CalcBackstepTimeSinceStart(svrNum);
            return true;
        }

        public void SetSynchrConfig(uint svrDelta, uint frameLater, uint preActNum, uint randSeed)
        {
            this.SvrFrameDelta = svrDelta;
            this.SvrFrameLater = 0;
            this.CacheSetLater = this.SvrFrameLater;
            this.KeyFrameRate = 1;
            this.PreActFrames = preActNum;
            this.ServerSeed = randSeed;
        }

        public void SetSynchrRunning(bool bRun)
        {
            this.bRunning = bRun;
            if (this.bRunning && this.bActive)
            {
                FrameRandom.ResetSeed(this.ServerSeed);
            }
        }

        private void ShowJitterChart(bool bShow, int nFlag)
        {
            if (bShow)
            {
                MonoSingleton<RealTimeChart>.instance.isVisible = true;
                MonoSingleton<RealTimeChart>.instance.AddTrack("RealTimeFps", Color.cyan, true, 0f, 600f).isVisiable = (nFlag & 1) != 0;
                MonoSingleton<RealTimeChart>.instance.AddTrack("NetPkgDelay", Color.green, true, 0f, 600f).isVisiable = (nFlag & 2) != 0;
                MonoSingleton<RealTimeChart>.instance.AddTrack("NetAvgDelay", Color.yellow, true, 0f, 600f).isVisiable = (nFlag & 4) != 0;
                MonoSingleton<RealTimeChart>.instance.AddTrack("JitterDelay", Color.red, true, 0f, 600f).isVisiable = (nFlag & 8) != 0;
                MonoSingleton<RealTimeChart>.instance.AddTrack("CameraSpeed", Color.black, true, 0f, 600f).isVisiable = (nFlag & 0x10) != 0;
                MonoSingleton<RealTimeChart>.instance.AddTrack("LogicTimeUse", Color.magenta, true, 0f, 600f).isVisiable = (nFlag & 0x20) != 0;
            }
            else
            {
                this.bShowJitterChart = false;
                MonoSingleton<RealTimeChart>.instance.isVisible = false;
                MonoSingleton<RealTimeChart>.instance.RemoveTrack("NetPkgDelay");
                MonoSingleton<RealTimeChart>.instance.RemoveTrack("NetAvgDelay");
                MonoSingleton<RealTimeChart>.instance.RemoveTrack("JitterDelay");
                MonoSingleton<RealTimeChart>.instance.RemoveTrack("RealTimeFps");
                MonoSingleton<RealTimeChart>.instance.RemoveTrack("CameraSpeed");
                MonoSingleton<RealTimeChart>.instance.RemoveTrack("LogicTimeUse");
            }
        }

        public void StartSynchr(bool bAutoRun)
        {
            this.bActive = true;
            this.SetSynchrRunning(bAutoRun);
            this.SvrFrameIndex = 0;
            this.FrameDelta = this.SvrFrameDelta / this.KeyFrameRate;
            this.CurFrameNum = 0;
            this.EndFrameNum = 0;
            this.LogicFrameTick = 0L;
            this.EndBlockWaitNum = 0;
            this.FrameSpeed = 1;
            this.GameSvrPing = 0;
            this._CurPkgDelay = 0;
            this.AvgFrameDelay = 0;
            this.JitterDelay = 0;
            this.JitterDamper = 0;
            this.StatDelayIdx = 0;
            Array.Clear(this.StatDelaySet, 0, this.StatDelaySet.Length);
            this.ShowJitterChart(false, 0);
            this.commandQueue.Clear();
            this.NewCommandId = 0;
            this.startFrameTime = Time.realtimeSinceStartup;
            this.backstepFrameCounter = 0;
        }

        public void SwitchJitterChart(int nFlag)
        {
            this.bShowJitterChart = nFlag > 0;
            this.ShowJitterChart(this.bShowJitterChart, nFlag);
        }

        public void SwitchSynchrLocal()
        {
            if (this.bActive)
            {
                this.bActive = false;
                this.startFrameTime = (Time.time - (this.LogicFrameTick * 0.001f)) + Time.deltaTime;
            }
        }

        public void UpdateFrame()
        {
            if (this.bActive)
            {
                if (this.bRunning)
                {
                    this.UpdateMultiFrame();
                    UpdatePing();
                }
            }
            else
            {
                this.UpdateSingleFrame();
            }
        }

        private void UpdateFrameLater()
        {
            if (this.CacheSetLater != this.SvrFrameLater)
            {
                this.SvrFrameLater = 0;
                this.EndFrameNum = (this.SvrFrameIndex + this.SvrFrameLater) * this.KeyFrameRate;
            }
        }

        private void UpdateMultiFrame()
        {
            Singleton<GameLogic>.GetInstance().UpdateTails();
            int num = (int) ((this.EndFrameNum - this.CurFrameNum) / this.nDriftFactor);
            this.tryCount = Mathf.Clamp(num, 1, 100);
            int tryCount = this.tryCount;
            long num4 = (long) (((Time.realtimeSinceStartup - this.startFrameTime) * 1000f) * this.FrameSpeed);
            long nDelayMs = num4 - ((this.SvrFrameIndex + 1) * this.SvrFrameDelta);
            int num6 = this.CalculateJitterDelay(nDelayMs);
            while (tryCount > 0)
            {
                long num7 = this.CurFrameNum * this.FrameDelta;
                long num8 = num4 - num7;
                num8 -= num6;
                if (num8 >= this.FrameDelta)
                {
                    if (this.CurFrameNum >= this.EndFrameNum)
                    {
                        this.EndBlockWaitNum++;
                        tryCount = 0;
                        continue;
                    }
                    this.EndBlockWaitNum = 0;
                    this.CurFrameNum++;
                    this.LogicFrameTick += this.FrameDelta;
                    while (this.commandQueue.Count > 0)
                    {
                        IFrameCommand command = this.commandQueue.Peek();
                        uint num9 = (command.frameNum + this.SvrFrameLater) * this.KeyFrameRate;
                        if (num9 > this.CurFrameNum)
                        {
                            break;
                        }
                        command.frameNum = num9;
                        this.commandQueue.Dequeue().ExecCommand();
                    }
                    if (!this.bEscape)
                    {
                        Singleton<GameLogic>.GetInstance().UpdateLogic((int) this.FrameDelta, (tryCount == 1) && (num8 < (2 * this.FrameDelta)));
                    }
                    tryCount--;
                    continue;
                }
                tryCount = 0;
            }
        }

        private static void UpdatePing()
        {
            if ((Singleton<FrameSynchr>.instance.m_ping != null) && Singleton<FrameSynchr>.instance.m_ping.isDone)
            {
                uint num = ((uint) (Time.realtimeSinceStartup * 1000f)) - Singleton<FrameSynchr>.instance.m_realpingStartTime;
                Singleton<FrameSynchr>.instance.m_ping = null;
                int num2 = Mathf.FloorToInt((float) ((num - Singleton<FrameSynchr>.instance.GameSvrPing) / 100)) * 100;
                Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("RealPing-GamePing:" + num2.ToString(), null, true);
            }
        }

        private void UpdateSingleFrame()
        {
            Singleton<GameLogic>.GetInstance().UpdateTails();
            long num = (long) (Time.deltaTime * 1000f);
            num = Mathf.Clamp((int) num, 0, 100);
            this.CurFrameNum++;
            this.LogicFrameTick += (ulong) num;
            while (this.commandQueue.Count > 0)
            {
                this.commandQueue.Dequeue().ExecCommand();
            }
            if (!this.bEscape && (num > 0L))
            {
                Singleton<GameLogic>.GetInstance().UpdateLogic((int) num, false);
            }
        }

        public bool bActive
        {
            get
            {
                return this._bActive;
            }
            set
            {
                if (this._bActive != value)
                {
                    Singleton<GameLogic>.instance.UpdateTails();
                    this._bActive = value;
                }
            }
        }

        public uint CurFrameNum { get; private set; }

        public uint EndFrameNum { get; private set; }

        public ulong LogicFrameTick { get; private set; }

        public uint NewCommandId
        {
            get
            {
                this.uCommandId++;
                return this.uCommandId;
            }
            set
            {
                this.uCommandId = value;
            }
        }

        public int RealSvrPing
        {
            get
            {
                return (this.GameSvrPing + this._CurPkgDelay);
            }
        }

        public uint svrLogicFrameNum
        {
            get
            {
                return this.SvrFrameIndex;
            }
        }
    }
}

