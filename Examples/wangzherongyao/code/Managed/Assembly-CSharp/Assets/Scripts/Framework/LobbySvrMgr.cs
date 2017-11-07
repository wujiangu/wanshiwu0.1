namespace Assets.Scripts.Framework
{
    using Apollo;
    using Assets.Scripts.UI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public class LobbySvrMgr : Singleton<LobbySvrMgr>
    {
        public bool isFirstLogin;
        public bool isLogin;
        private bool isLoginingByDefault = true;

        public event ConnectFailHandler connectFailHandler;

        private void ConnectFailed()
        {
            Debug.Log("LobbyConnector ConnectFailed called!");
            if (this.connectFailHandler != null)
            {
                this.connectFailHandler(Singleton<NetworkModule>.GetInstance().lobbySvr.lastResult);
            }
        }

        public bool ConnectServer()
        {
            if (!Singleton<NetworkModule>.GetInstance().isOnlineMode || this.isLogin)
            {
                return false;
            }
            Singleton<NetworkModule>.GetInstance().lobbySvr.ConnectedEvent -= new NetConnectedEvent(this.onLobbyConnected);
            Singleton<NetworkModule>.GetInstance().lobbySvr.DisconnectEvent -= new NetDisconnectEvent(this.onLobbyDisconnected);
            Singleton<NetworkModule>.GetInstance().lobbySvr.ConnectedEvent += new NetConnectedEvent(this.onLobbyConnected);
            Singleton<NetworkModule>.GetInstance().lobbySvr.DisconnectEvent += new NetDisconnectEvent(this.onLobbyDisconnected);
            Singleton<NetworkModule>.GetInstance().lobbySvr.GetTryReconnect = (LobbyConnector.DelegateGetTryReconnect) Delegate.Remove(Singleton<NetworkModule>.GetInstance().lobbySvr.GetTryReconnect, new LobbyConnector.DelegateGetTryReconnect(this.OnTryReconnect));
            Singleton<NetworkModule>.GetInstance().lobbySvr.GetTryReconnect = (LobbyConnector.DelegateGetTryReconnect) Delegate.Combine(Singleton<NetworkModule>.GetInstance().lobbySvr.GetTryReconnect, new LobbyConnector.DelegateGetTryReconnect(this.OnTryReconnect));
            ConnectorParam para = new ConnectorParam {
                url = ApolloConfig.loginUrl,
                ip = ApolloConfig.loginOnlyIpOrUrl,
                vPort = ApolloConfig.loginOnlyVPort
            };
            bool flag = Singleton<NetworkModule>.GetInstance().InitLobbyServerConnect(para);
            if (flag)
            {
                Singleton<CUIManager>.GetInstance().OpenSendMsgAlert(null, 10, enUIEventID.None);
            }
            return flag;
        }

        private void ConnectServerWithTdirCandidate()
        {
            this.isLoginingByDefault = false;
            object param = 0;
            bool flag = false;
            if (MonoSingleton<TdirMgr>.GetInstance().ParseNodeAppAttr(MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.attr, TdirNodeAttrType.ISPChoose, ref param))
            {
                Dictionary<string, int> dictionary = (Dictionary<string, int>) param;
                if (dictionary != null)
                {
                    foreach (KeyValuePair<string, int> pair in dictionary)
                    {
                        if (pair.Value == MonoSingleton<TdirMgr>.GetInstance().GetISP())
                        {
                            IPAddrInfo info = MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.addrs[0];
                            ApolloConfig.loginUrl = string.Format("tcp://{0}:{1}", pair.Key, info.port);
                            ApolloConfig.loginOnlyIpOrUrl = pair.Key;
                            IPAddrInfo info2 = MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.addrs[0];
                            ApolloConfig.loginOnlyVPort = ushort.Parse(info2.port);
                            ApolloConfig.ISPType = MonoSingleton<TdirMgr>.GetInstance().GetISP();
                            flag = true;
                            break;
                        }
                    }
                }
            }
            if (flag)
            {
                this.ConnectServer();
            }
            else
            {
                this.ConnectFailed();
            }
        }

        public void ConnectServerWithTdirDefault(int index)
        {
            this.isLoginingByDefault = true;
            IPAddrInfo info = MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.addrs[index];
            IPAddrInfo info2 = MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.addrs[index];
            ApolloConfig.loginUrl = string.Format("tcp://{0}:{1}", info.ip, info2.port);
            IPAddrInfo info3 = MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.addrs[index];
            ApolloConfig.loginOnlyIpOrUrl = info3.ip;
            IPAddrInfo info4 = MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.addrs[index];
            ApolloConfig.loginOnlyVPort = ushort.Parse(info4.port);
            ApolloConfig.ISPType = 1;
            this.ConnectServer();
            Singleton<BeaconHelper>.GetInstance().Event_CommonReport("Event_LoginMsgSend");
        }

        private void onLobbyConnected(object sender)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
        }

        private void onLobbyDisconnected(object sender)
        {
        }

        public uint OnTryReconnect(uint curConnectTime, uint maxcount)
        {
            if (this.isLogin)
            {
                if (!Singleton<LobbyLogic>.instance.inMultiGame && (Singleton<BattleLogic>.instance.isGameOver || Singleton<BattleLogic>.instance.m_bIsPayStat))
                {
                    if (curConnectTime > maxcount)
                    {
                        if (curConnectTime == (maxcount + 1))
                        {
                            Singleton<LobbyLogic>.instance.OnSendSingleGameFinishFail();
                        }
                        return curConnectTime;
                    }
                    Singleton<CUIManager>.GetInstance().OpenSendMsgAlert("尝试重新连接服务器...", 5, enUIEventID.None);
                    return curConnectTime;
                }
                if (!Singleton<BattleLogic>.instance.isRuning)
                {
                    Singleton<CUIManager>.GetInstance().OpenSendMsgAlert(5, enUIEventID.None);
                }
                return 0;
            }
            if (this.isLoginingByDefault)
            {
                this.ConnectServerWithTdirCandidate();
            }
            else
            {
                this.ConnectFailed();
            }
            return (curConnectTime + maxcount);
        }

        public delegate void ConnectFailHandler(ApolloResult result);
    }
}

