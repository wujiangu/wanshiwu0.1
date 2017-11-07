namespace Assets.Scripts.Framework
{
    using Apollo;
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using System.Collections.Generic;
    using tsf4g_tdr_csharp;

    public class GameConnector : BaseConnector
    {
        private NetworkState changedNetState;
        private List<CSPkg> confirmSendQueue = new List<CSPkg>();
        private List<CSPkg> gameMsgSendQueue = new List<CSPkg>();
        private int nBuffSize = 0x32000;
        private bool netStateChanged;
        private ReconnectPolicy reconPolicy = new ReconnectPolicy();
        private byte[] szSendBuffer = new byte[0x32000];

        public void CleanUp()
        {
            this.gameMsgSendQueue.Clear();
            this.confirmSendQueue.Clear();
            this.reconPolicy.StopPolicy();
            this.ClearBuffer();
        }

        private void ClearBuffer()
        {
            this.szSendBuffer.Initialize();
        }

        protected override void DealConnectClose(ApolloResult result)
        {
        }

        protected override void DealConnectError(ApolloResult result)
        {
            this.reconPolicy.StartPolicy(result);
            MonoSingleton<Reconnection>.instance.QueryIsRelayGaming(result);
            List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()),
                new KeyValuePair<string, string>("status", "1"),
                new KeyValuePair<string, string>("type", "challenge"),
                new KeyValuePair<string, string>("errorCode", result.ToString())
            };
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_SvrConnectFail", events, true);
        }

        protected override void DealConnectFail(ApolloResult result, ApolloLoginInfo loginInfo)
        {
            this.reconPolicy.StartPolicy(result);
            MonoSingleton<Reconnection>.instance.QueryIsRelayGaming(result);
            List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()),
                new KeyValuePair<string, string>("status", "1"),
                new KeyValuePair<string, string>("type", "challenge"),
                new KeyValuePair<string, string>("errorCode", result.ToString())
            };
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_SvrConnectFail", events, true);
        }

        protected override void DealConnectSucc()
        {
            this.reconPolicy.StopPolicy();
            MonoSingleton<Reconnection>.GetInstance().OnConnectSuccess();
            List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()),
                new KeyValuePair<string, string>("status", "0"),
                new KeyValuePair<string, string>("type", "challenge"),
                new KeyValuePair<string, string>("errorCode", "SUCC")
            };
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_SvrConnectFail", events, true);
        }

        public void Disconnect()
        {
            ApolloNetworkService.Intance.NetworkChangedEvent -= new Apollo.NetworkStateChanged(this.NetworkStateChanged);
            base.DestroyConnector();
            this.reconPolicy.StopPolicy();
            this.reconPolicy.SetConnector(null, null, 0);
            base.initParam = null;
        }

        ~GameConnector()
        {
            base.DestroyConnector();
            this.reconPolicy = null;
        }

        public void ForceReconnect()
        {
            this.reconPolicy.UpdatePolicy(true);
        }

        public void HandleMsg(CSPkg msg)
        {
            if (GameSettings.enableReplay && (((msg.stPkgHead.dwMsgID == 0x433) || (msg.stPkgHead.dwMsgID == 0x435)) || (msg.stPkgHead.dwMsgID == 0x43a)))
            {
                Singleton<GameReplayModule>.instance.RecordMsg(msg);
            }
            NetMsgDelegate msgHandler = Singleton<NetworkModule>.instance.GetMsgHandler(msg.stPkgHead.dwMsgID);
            if (msgHandler != null)
            {
                msgHandler(msg);
            }
        }

        public void HandleSending()
        {
            if (base.connected)
            {
                for (int i = 0; i < this.confirmSendQueue.Count; i++)
                {
                    CSPkg msg = this.confirmSendQueue[i];
                    if ((Singleton<GameLogic>.instance.GameRunningTick - msg.stPkgHead.dwSvrPkgSeq) > 0x1388)
                    {
                        this.SendPackage(msg);
                        msg.stPkgHead.dwSvrPkgSeq = Singleton<GameLogic>.instance.GameRunningTick;
                    }
                }
                while (base.connected && (this.gameMsgSendQueue.Count > 0))
                {
                    CSPkg pkg2 = this.gameMsgSendQueue[0];
                    if (!this.SendPackage(pkg2))
                    {
                        break;
                    }
                    if (pkg2.stPkgHead.dwReserve > 0)
                    {
                        pkg2.stPkgHead.dwSvrPkgSeq = Singleton<GameLogic>.instance.GameRunningTick;
                        this.confirmSendQueue.Add(pkg2);
                    }
                    else
                    {
                        pkg2.Release();
                    }
                    this.gameMsgSendQueue.RemoveAt(0);
                }
            }
            else
            {
                MonoSingleton<Reconnection>.instance.UpdateReconnect();
            }
        }

        public bool Init(ConnectorParam para)
        {
            this.reconPolicy.SetConnector(this, new tryReconnectDelegate(this.onTryReconnect), 4);
            ApolloNetworkService.Intance.NetworkChangedEvent -= new Apollo.NetworkStateChanged(this.NetworkStateChanged);
            ApolloNetworkService.Intance.NetworkChangedEvent += new Apollo.NetworkStateChanged(this.NetworkStateChanged);
            return base.CreateConnector(para);
        }

        private void NetworkStateChanged(NetworkState state)
        {
            this.changedNetState = state;
            this.netStateChanged = true;
        }

        private uint onTryReconnect(uint nCount, uint nMax)
        {
            try
            {
                MonoSingleton<Reconnection>.GetInstance().ShowReconnectMsgAlert((int) nCount, (int) nMax);
            }
            catch (Exception exception)
            {
                object[] inParameters = new object[] { exception.Message, exception.StackTrace };
                DebugHelper.Assert(false, "Exception In GameConnector Try Reconnect, {0} {1}", inParameters);
            }
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("GameConnector.onTryReconnect", null, true);
            NetworkModule instance = Singleton<NetworkModule>.GetInstance();
            instance.m_GameReconnetCount++;
            return nCount;
        }

        public void PushSendMsg(CSPkg msg)
        {
            this.gameMsgSendQueue.Add(msg);
        }

        public CSPkg RecvPackage()
        {
            if (base.connected && (base.connector != null))
            {
                byte[] buffer;
                int num;
                if (base.connector.ReadUdpData(out buffer, out num) == ApolloResult.Success)
                {
                    int usedSize = 0;
                    CSPkg pkg = CSPkg.New();
                    if ((pkg.unpack(ref buffer, num, ref usedSize, 0) == TdrError.ErrorType.TDR_NO_ERROR) && (usedSize > 0))
                    {
                        return pkg;
                    }
                }
                if (base.connector.ReadData(out buffer, out num) == ApolloResult.Success)
                {
                    int num3 = 0;
                    CSPkg pkg2 = CSPkg.New();
                    if ((pkg2.unpack(ref buffer, num, ref num3, 0) == TdrError.ErrorType.TDR_NO_ERROR) && (num3 > 0))
                    {
                        int index = 0;
                        while (index < this.confirmSendQueue.Count)
                        {
                            CSPkg pkg3 = this.confirmSendQueue[index];
                            if ((pkg3.stPkgHead.dwReserve > 0) && (pkg3.stPkgHead.dwReserve == pkg2.stPkgHead.dwMsgID))
                            {
                                pkg3.Release();
                                this.confirmSendQueue.RemoveAt(index);
                            }
                            else
                            {
                                index++;
                            }
                        }
                        return pkg2;
                    }
                }
            }
            return null;
        }

        public bool SendPackage(CSPkg msg)
        {
            if (!base.connected || (base.connector == null))
            {
                return false;
            }
            int usedSize = 0;
            if (msg.pack(ref this.szSendBuffer, this.nBuffSize, ref usedSize, 0) != TdrError.ErrorType.TDR_NO_ERROR)
            {
                return false;
            }
            byte[] destinationArray = new byte[usedSize];
            Array.Copy(this.szSendBuffer, destinationArray, usedSize);
            if (!base.initParam.bIsUDP || ((msg.stPkgHead.dwMsgID != 0x3ed) && (msg.stPkgHead.dwMsgID != 0x4ec)))
            {
                return (base.connector.WriteData(destinationArray, -1) == ApolloResult.Success);
            }
            return (base.connector.WriteUdpData(destinationArray, -1) == ApolloResult.Success);
        }

        public void Update()
        {
            this.reconPolicy.UpdatePolicy(false);
            if (this.netStateChanged)
            {
                if (this.changedNetState == NetworkState.NotReachable)
                {
                    Singleton<CUIManager>.GetInstance().OpenSendMsgAlert(Singleton<CTextManager>.GetInstance().GetText("NetworkConnecting"), 10, enUIEventID.None);
                }
                else
                {
                    Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
                }
                this.netStateChanged = false;
            }
        }
    }
}

