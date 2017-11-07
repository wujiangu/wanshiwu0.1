namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;

    public class ExchangeActivity : Activity
    {
        private readonly ResCltWealExchange _config;
        private readonly Dictionary<int, uint> _exchangeCount;

        public ExchangeActivity(ActivitySys mgr, ResCltWealExchange config) : base(mgr, config.stCommon)
        {
            this._exchangeCount = new Dictionary<int, uint>();
            this._config = config;
            for (uint i = 0; i < config.bExchangeCnt; i++)
            {
                ExchangePhase ap = new ExchangePhase(this, i, config.astExchangeList[i]);
                base.AddPhase(ap);
            }
        }

        public uint GetExchangeCount(int index)
        {
            uint num = 0;
            return (!this._exchangeCount.TryGetValue(index, out num) ? 0 : num);
        }

        public uint GetMaxExchangeCount(int index)
        {
            if ((index - 1) < this._config.bExchangeCnt)
            {
                return this._config.astExchangeList[index - 1].wDupCnt;
            }
            return 0;
        }

        public void IncreaseExchangeCount(int index)
        {
            uint num = 0;
            if (this._exchangeCount.TryGetValue(index, out num))
            {
                ResDT_WealExchagne_Info info = this._config.astExchangeList[index - 1];
                if (num < info.wDupCnt)
                {
                    this._exchangeCount.Remove(index);
                    this._exchangeCount.Add(index, ++num);
                }
            }
            else
            {
                this._exchangeCount.Add(index, 1);
            }
        }

        public void ReqDoExchange(uint index)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x9c7);
            msg.stPkgData.stDrawWealReq.bWealType = 4;
            msg.stPkgData.stDrawWealReq.dwWealID = this.ID;
            msg.stPkgData.stDrawWealReq.dwPeriodID = index;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public override void UpdateInfo(ref COMDT_WEAL_UNION actvInfo)
        {
            this._exchangeCount.Clear();
            byte bWealCnt = actvInfo.stExchange.bWealCnt;
            COMDT_WEAL_EXCHANGE_OBJ[] astWealList = actvInfo.stExchange.astWealList;
            for (int i = 0; i < bWealCnt; i++)
            {
                this._exchangeCount.Add(astWealList[i].bWealIdx, astWealList[i].dwExchangeCnt);
            }
        }

        public void UpdateView()
        {
            base.NotifyTimeStateChanged();
        }

        public override bool Completed
        {
            get
            {
                return false;
            }
        }

        public override uint ID
        {
            get
            {
                return this._config.dwID;
            }
        }

        public override bool ReadyForDot
        {
            get
            {
                for (int i = 0; i < base.PhaseList.Count; i++)
                {
                    ActivityPhase phase = base.PhaseList[i];
                    if ((phase != null) && phase.ReadyForGet)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override COM_WEAL_TYPE Type
        {
            get
            {
                return COM_WEAL_TYPE.COM_WEAL_EXCHANGE;
            }
        }
    }
}

