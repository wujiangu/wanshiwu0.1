namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class CChatChannel
    {
        public int addTimeSplice_timer = -1;
        private bool bCD_Valid = true;
        public uint cd_time;
        public EChatChannel ChannelType;
        private static uint clt_pendding_time = 0x7d0;
        public uint dwLogicWorldId;
        private int inputCDTimer = -1;
        public uint lastTimeStamp;
        public ListView<CChatEntity> list = new ListView<CChatEntity>();
        public static int MaxDeltaTime_Seconds = 60;
        public List<Vector2> sizeVec = new List<Vector2>();
        public ulong ullUid;
        private int unread_time_entity_count;
        private int unreadIndex;

        public CChatChannel(EChatChannel channelType, uint cdTime = 0, ulong ullUid = 0, uint dwLogicWorldId = 0)
        {
            this.ChannelType = channelType;
            this.cd_time = 0;
            this.ullUid = ullUid;
            this.dwLogicWorldId = dwLogicWorldId;
        }

        public void Add(CChatEntity ent)
        {
            if (ent.type == EChaterType.Time)
            {
                this.unread_time_entity_count++;
            }
            this.list.Add(ent);
            if (this.list.Count > CChatController.MaxCount)
            {
                this.list.RemoveAt(0);
            }
        }

        public void Clear()
        {
            this.list.Clear();
            this.sizeVec.Clear();
            this.ullUid = 0L;
            this.dwLogicWorldId = 0;
            for (int i = 0; i < this.list.Count; i++)
            {
                this.list[i].Clear();
            }
            this.list.Clear();
            this.unread_time_entity_count = this.unreadIndex = 0;
            this.lastTimeStamp = 0;
            if (this.inputCDTimer != -1)
            {
                Singleton<CTimerManager>.instance.RemoveTimer(this.inputCDTimer);
            }
            this.inputCDTimer = -1;
        }

        public void ClearCd()
        {
            this.bCD_Valid = true;
        }

        public int Get_Left_CDTime()
        {
            return Singleton<CTimerManager>.GetInstance().GetLeftTime(this.inputCDTimer);
        }

        public int GetCount(EChaterType type)
        {
            int num = 0;
            for (int i = 0; i < this.list.Count; i++)
            {
                if (this.list[i].type == type)
                {
                    num++;
                }
            }
            return num;
        }

        public CChatEntity GetLast()
        {
            if (this.list.Count == 0)
            {
                return null;
            }
            if (this.ChannelType == EChatChannel.Friend)
            {
                return ((this.list[this.list.Count - 1].type != EChaterType.System) ? this.list[this.list.Count - 1] : null);
            }
            return this.list[this.list.Count - 1];
        }

        public int GetUnreadCount()
        {
            return ((this.list.Count - this.unreadIndex) - this.unread_time_entity_count);
        }

        public void Init_Timer()
        {
            if (this.ChannelType == EChatChannel.Lobby)
            {
                ResAcntExpInfo dataByKey = GameDataMgr.acntExpDatabin.GetDataByKey(Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().PvpLevel);
                this.cd_time = dataByKey.dwChatCD;
                Singleton<CTimerManager>.GetInstance().RemoveTimer(this.inputCDTimer);
                if (this.cd_time > 0)
                {
                    this.cd_time += clt_pendding_time;
                    this.inputCDTimer = Singleton<CTimerManager>.GetInstance().AddTimer((int) this.cd_time, -1, new CTimer.OnTimeUpHandler(this.On_InputCD_Done));
                    Singleton<CTimerManager>.GetInstance().PauseTimer(this.inputCDTimer);
                }
            }
        }

        public void InitChat_InputTimer(int time)
        {
            if (time > 0)
            {
                this.bCD_Valid = true;
                Singleton<CTimerManager>.GetInstance().RemoveTimer(this.inputCDTimer);
                if (time > 0)
                {
                    this.cd_time = ((uint) time) + clt_pendding_time;
                    this.inputCDTimer = Singleton<CTimerManager>.GetInstance().AddTimer((int) this.cd_time, -1, new CTimer.OnTimeUpHandler(this.On_InputCD_Done));
                    Singleton<CTimerManager>.GetInstance().PauseTimer(this.inputCDTimer);
                }
            }
        }

        public bool IsInputValid()
        {
            return this.bCD_Valid;
        }

        private void On_InputCD_Done(int timerSequence)
        {
            this.bCD_Valid = true;
            Singleton<CTimerManager>.GetInstance().PauseTimer(this.inputCDTimer);
            Singleton<CTimerManager>.GetInstance().ResetTimer(this.inputCDTimer);
        }

        public void ReadAll()
        {
            this.unreadIndex = this.list.Count;
            this.unread_time_entity_count = 0;
        }

        public void Start_InputCD()
        {
            if (this.cd_time != 0)
            {
                this.bCD_Valid = false;
                Singleton<CTimerManager>.GetInstance().ResumeTimer(this.inputCDTimer);
            }
        }
    }
}

