namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using CSProtocol;
    using ResData;
    using System;
    using System.Runtime.CompilerServices;

    public abstract class ActivityPhase
    {
        private int _extraRewardCount;
        private ResRewardForWeal _extraRewardStore;
        private bool _marked;
        private Activity _owner;
        private int _rewardCount;
        private ResRewardForWeal _rewardStore;
        private int _secondSpan;
        private TimeState _timeState;

        public event ActivityPhaseEvent OnMaskStateChange;

        public event ActivityPhaseEvent OnTimeStateChange;

        public ActivityPhase(Activity owner)
        {
            this._owner = owner;
            this._timeState = TimeState.NotStart;
            this._secondSpan = 0;
            this._marked = false;
            this._rewardStore = null;
            this._rewardCount = 0;
            this._extraRewardStore = null;
            this._extraRewardCount = 0;
        }

        internal void _NotifyStateChanged()
        {
            if (this.OnMaskStateChange != null)
            {
                this.OnMaskStateChange(this);
            }
        }

        public virtual bool AchieveJump()
        {
            return false;
        }

        public virtual bool CheckTimeState()
        {
            TimeState notStart;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo == null)
            {
                return false;
            }
            if (this._owner.timeState == Activity.TimeState.Going)
            {
                if ((this.StartTime > 0) && (this.CloseTime > 0))
                {
                    DateTime time = Utility.ToUtcTime2Local((long) masterRoleInfo.getCurrentTimeSinceLogin());
                    DateTime time2 = Utility.SecondsToDateTime(time.Year, time.Month, time.Day, this.StartTime);
                    TimeSpan span = (TimeSpan) (time - time2);
                    this._secondSpan = (int) span.TotalSeconds;
                    if (this._secondSpan < 0)
                    {
                        this._secondSpan = -this._secondSpan;
                        notStart = TimeState.NotStart;
                    }
                    else
                    {
                        DateTime time3 = Utility.SecondsToDateTime(time.Year, time.Month, time.Day, this.CloseTime);
                        span = (TimeSpan) (time - time3);
                        this._secondSpan = (int) span.TotalSeconds;
                        if (this._secondSpan < 0)
                        {
                            this._secondSpan = -this._secondSpan;
                            notStart = TimeState.Started;
                        }
                        else
                        {
                            notStart = TimeState.Closed;
                        }
                    }
                }
                else
                {
                    this._secondSpan = 0;
                    notStart = TimeState.Started;
                }
            }
            else if ((this._owner.timeState == Activity.TimeState.ForeShow) || (this._owner.timeState == Activity.TimeState.InHiding))
            {
                notStart = TimeState.NotStart;
            }
            else
            {
                notStart = TimeState.Closed;
            }
            if (notStart == this._timeState)
            {
                return false;
            }
            this._timeState = notStart;
            if (this.OnTimeStateChange != null)
            {
                this.OnTimeStateChange(this);
            }
            return true;
        }

        public virtual void Clear()
        {
        }

        public void DrawReward()
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x9c7);
            msg.stPkgData.stDrawWealReq.bWealType = (byte) this._owner.Type;
            msg.stPkgData.stDrawWealReq.dwWealID = this._owner.ID;
            uint iD = this.ID;
            ExeTaskPhase phase = this as ExeTaskPhase;
            if (phase != null)
            {
                ExeTaskActivity owner = phase.Owner as ExeTaskActivity;
                if ((owner != null) && owner.IsSingleConfig())
                {
                    iD = phase.FakeID;
                }
            }
            msg.stPkgData.stDrawWealReq.dwPeriodID = iD;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public uint GetDropCount(int index)
        {
            if (this.RewardDrop != null)
            {
                ResDT_WealRewardItem item = this.RewardDrop.astRewardDetail[index];
                if (item.dwLowCnt == item.dwHighCnt)
                {
                    return item.dwLowCnt;
                }
            }
            return 0;
        }

        public CUseable GetExtraUseable(int index)
        {
            if ((this.ExtraRewardDrop != null) && (index < this.ExtraRewardCount))
            {
                ResDT_WealRewardItem item = this.ExtraRewardDrop.astRewardDetail[index];
                return CUseableManager.CreateUsableByRandowReward((RES_RANDOM_REWARD_TYPE) item.bItemType, (int) item.dwLowCnt, item.dwItemID);
            }
            return null;
        }

        public CUseable GetUseable(int index)
        {
            if ((this.RewardDrop != null) && (index < this.RewardCount))
            {
                ResDT_WealRewardItem item = this.RewardDrop.astRewardDetail[index];
                return CUseableManager.CreateUsableByRandowReward((RES_RANDOM_REWARD_TYPE) item.bItemType, (int) item.dwLowCnt, item.dwItemID);
            }
            return null;
        }

        public virtual uint GetVipAddition(int vipFlagBit)
        {
            return 0;
        }

        private static string SecondsToHMText(int secondsInDay)
        {
            int num = secondsInDay / 0xe10;
            int num2 = (secondsInDay - (num * 0xe10)) / 60;
            return string.Format("{0:D2}:{1:D2}", num, num2);
        }

        public virtual bool Achieved
        {
            get
            {
                return false;
            }
        }

        public virtual int AchieveInDays
        {
            get
            {
                return 0;
            }
        }

        public virtual bool AchieveStateValid
        {
            get
            {
                return false;
            }
        }

        public abstract int CloseTime { get; }

        public string CloseTimeText
        {
            get
            {
                return SecondsToHMText(this.CloseTime);
            }
        }

        public virtual int Current
        {
            get
            {
                return this._owner.Current;
            }
        }

        public int ExtraRewardCount
        {
            get
            {
                if (this.ExtraRewardDrop != null)
                {
                    return this._extraRewardCount;
                }
                return 0;
            }
        }

        private ResRewardForWeal ExtraRewardDrop
        {
            get
            {
                if (((this._extraRewardStore == null) && GameDataMgr.wealRewardDict.TryGetValue(this.ExtraRewardID, out this._extraRewardStore)) && (this._extraRewardStore != null))
                {
                    this._extraRewardCount = 0;
                    while (this._extraRewardCount < this._extraRewardStore.astRewardDetail.Length)
                    {
                        if ((this._extraRewardStore.astRewardDetail[this._extraRewardCount].bItemType == 0) || (this._extraRewardStore.astRewardDetail[this._extraRewardCount].bItemType >= 0x11))
                        {
                            break;
                        }
                        this._extraRewardCount++;
                    }
                }
                return this._extraRewardStore;
            }
        }

        public virtual uint ExtraRewardID
        {
            get
            {
                return 0;
            }
        }

        public abstract uint ID { get; }

        public virtual bool InMultipleTime
        {
            get
            {
                return false;
            }
        }

        public bool Marked
        {
            get
            {
                return this._marked;
            }
            set
            {
                if (value != this._marked)
                {
                    this._marked = value;
                    this._NotifyStateChanged();
                }
            }
        }

        public virtual uint MultipleTimes
        {
            get
            {
                return ((!this.InMultipleTime || !this.Owner.InMultipleTime) ? 0 : this.Owner.MultipleTimes);
            }
        }

        public Activity Owner
        {
            get
            {
                return this._owner;
            }
        }

        public virtual bool ReadyForGet
        {
            get
            {
                return ((this.timeState == TimeState.Started) && !this._marked);
            }
        }

        public int RewardCount
        {
            get
            {
                if (this.RewardDrop == null)
                {
                    return 0;
                }
                return this._rewardCount;
            }
        }

        public string RewardDesc
        {
            get
            {
                if (this.RewardDrop == null)
                {
                    return string.Empty;
                }
                return Utility.UTF8Convert(this.RewardDrop.szRewardDesc).Trim();
            }
        }

        private ResRewardForWeal RewardDrop
        {
            get
            {
                if (((this._rewardStore == null) && GameDataMgr.wealRewardDict.TryGetValue(this.RewardID, out this._rewardStore)) && (this._rewardStore != null))
                {
                    this._rewardCount = 0;
                    while (this._rewardCount < this._rewardStore.astRewardDetail.Length)
                    {
                        if ((this._rewardStore.astRewardDetail[this._rewardCount].bItemType == 0) || (this._rewardStore.astRewardDetail[this._rewardCount].bItemType >= 0x11))
                        {
                            break;
                        }
                        this._rewardCount++;
                    }
                }
                return this._rewardStore;
            }
        }

        public abstract uint RewardID { get; }

        public abstract int StartTime { get; }

        public string StartTimeText
        {
            get
            {
                return SecondsToHMText(this.StartTime);
            }
        }

        public virtual int Target
        {
            get
            {
                return (((int) this.ID) + 1);
            }
        }

        public TimeState timeState
        {
            get
            {
                return this._timeState;
            }
        }

        public virtual string Tips
        {
            get
            {
                return (this.StartTimeText + "~" + this.CloseTimeText);
            }
        }

        public delegate void ActivityPhaseEvent(ActivityPhase ap);

        public enum TimeState
        {
            NotStart,
            Started,
            Closed
        }
    }
}

