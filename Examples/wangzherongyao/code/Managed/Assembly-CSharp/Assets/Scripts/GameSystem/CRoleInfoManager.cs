namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.GameLogic;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class CRoleInfoManager : Singleton<CRoleInfoManager>, IUpdateLogic
    {
        private CRoleInfoContainer s_roleInfoContainer;

        public void CalculateKDA(COMDT_GAME_INFO gameInfo)
        {
            CRoleInfo masterRoleInfo = this.GetMasterRoleInfo();
            DebugHelper.Assert(masterRoleInfo != null, "masterRoleInfo is null");
            if (masterRoleInfo != null)
            {
                PlayerKDA hostKDA = Singleton<BattleStatistic>.GetInstance().m_playerKDAStat.GetHostKDA();
                if (hostKDA != null)
                {
                    int num = 0;
                    int num2 = 0;
                    int num3 = 0;
                    int num4 = 0;
                    int num5 = 0;
                    int num6 = 0;
                    int num7 = 0;
                    IEnumerator<HeroKDA> enumerator = hostKDA.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current != null)
                        {
                            num += enumerator.Current.LegendaryNum;
                            num2 += enumerator.Current.PentaKillNum;
                            num3 += enumerator.Current.QuataryKillNum;
                            num4 += enumerator.Current.TripleKillNum;
                            num5 += enumerator.Current.DoubleKillNum;
                        }
                    }
                    if (gameInfo.bGameResult == 1)
                    {
                        uint mvpPlayer = Singleton<BattleStatistic>.instance.GetMvpPlayer(hostKDA.PlayerCamp, true);
                        if (mvpPlayer != 0)
                        {
                            num6 = (mvpPlayer != hostKDA.PlayerId) ? 0 : 1;
                        }
                    }
                    else if (gameInfo.bGameResult == 2)
                    {
                        uint num9 = Singleton<BattleStatistic>.instance.GetMvpPlayer(hostKDA.PlayerCamp, false);
                        if (num9 != 0)
                        {
                            num7 = (num9 != hostKDA.PlayerId) ? 0 : 1;
                        }
                    }
                    bool flag = false;
                    bool flag2 = false;
                    bool flag3 = false;
                    bool flag4 = false;
                    bool flag5 = false;
                    bool flag6 = false;
                    bool flag7 = false;
                    int index = 0;
                    ListView<COMDT_STATISTIC_KEY_VALUE_INFO> inList = new ListView<COMDT_STATISTIC_KEY_VALUE_INFO>();
                    while (index < masterRoleInfo.pvpDetail.stKVDetail.dwNum)
                    {
                        COMDT_STATISTIC_KEY_VALUE_INFO comdt_statistic_key_value_info = masterRoleInfo.pvpDetail.stKVDetail.astKVDetail[index];
                        switch (((RES_STATISTIC_SETTLE_DATA_TYPE) comdt_statistic_key_value_info.dwKey))
                        {
                            case RES_STATISTIC_SETTLE_DATA_TYPE.RES_STATISTIC_SETTLE_DATA_TYPE_MVP_CNT:
                                comdt_statistic_key_value_info.dwValue += (uint) num6;
                                flag6 = true;
                                break;

                            case RES_STATISTIC_SETTLE_DATA_TYPE.RES_STATISTIC_SETTLE_DATA_TYPE_LOSE_SOUL:
                                comdt_statistic_key_value_info.dwValue += (uint) num7;
                                flag7 = true;
                                break;

                            case RES_STATISTIC_SETTLE_DATA_TYPE.RES_STATISTIC_SETTLE_DATA_TYPE_GODLIKE_CNT:
                                comdt_statistic_key_value_info.dwValue += (uint) num;
                                flag5 = true;
                                break;

                            case RES_STATISTIC_SETTLE_DATA_TYPE.RES_STATISTIC_SETTLE_DATA_TYPE_DOUBLE_KILL_CNT:
                                comdt_statistic_key_value_info.dwValue += (uint) num5;
                                flag = true;
                                break;

                            case RES_STATISTIC_SETTLE_DATA_TYPE.RES_STATISTIC_SETTLE_DATA_TYPE_TRIPLE_KILL_CNT:
                                comdt_statistic_key_value_info.dwValue += (uint) num4;
                                flag2 = true;
                                break;

                            case RES_STATISTIC_SETTLE_DATA_TYPE.RES_STATISTIC_SETTLE_DATA_TYPE_QUATARY_KILL_CNT:
                                comdt_statistic_key_value_info.dwValue += (uint) num3;
                                flag3 = true;
                                break;

                            case RES_STATISTIC_SETTLE_DATA_TYPE.RES_STATISTIC_SETTLE_DATA_TYPE_PENTA_KILL_CNT:
                                comdt_statistic_key_value_info.dwValue += (uint) num2;
                                flag4 = true;
                                break;
                        }
                        index++;
                    }
                    COMDT_STATISTIC_KEY_VALUE_INFO item = null;
                    if (!flag)
                    {
                        item = new COMDT_STATISTIC_KEY_VALUE_INFO {
                            dwKey = 0x10,
                            dwValue = (uint) num5
                        };
                        inList.Add(item);
                    }
                    if (!flag2)
                    {
                        item = new COMDT_STATISTIC_KEY_VALUE_INFO {
                            dwKey = 0x11,
                            dwValue = (uint) num4
                        };
                        inList.Add(item);
                    }
                    if (!flag3)
                    {
                        item = new COMDT_STATISTIC_KEY_VALUE_INFO {
                            dwKey = 0x1b,
                            dwValue = (uint) num3
                        };
                        inList.Add(item);
                    }
                    if (!flag4)
                    {
                        item = new COMDT_STATISTIC_KEY_VALUE_INFO {
                            dwKey = 0x1c,
                            dwValue = (uint) num2
                        };
                        inList.Add(item);
                    }
                    if (!flag5)
                    {
                        item = new COMDT_STATISTIC_KEY_VALUE_INFO {
                            dwKey = 15,
                            dwValue = (uint) num
                        };
                        inList.Add(item);
                    }
                    if (!flag6)
                    {
                        item = new COMDT_STATISTIC_KEY_VALUE_INFO {
                            dwKey = 13,
                            dwValue = (uint) num6
                        };
                        inList.Add(item);
                    }
                    if (!flag7)
                    {
                        item = new COMDT_STATISTIC_KEY_VALUE_INFO {
                            dwKey = 14,
                            dwValue = (uint) num7
                        };
                        inList.Add(item);
                    }
                    if (inList.Count > 0)
                    {
                        masterRoleInfo.pvpDetail.stKVDetail.dwNum += (uint) inList.Count;
                        inList.AddRange(masterRoleInfo.pvpDetail.stKVDetail.astKVDetail);
                        masterRoleInfo.pvpDetail.stKVDetail.astKVDetail = LinqS.ToArray<COMDT_STATISTIC_KEY_VALUE_INFO>(inList);
                    }
                }
            }
        }

        public void CalculateWins(CSDT_PVPBATTLE_INFO battleInfo, int bGameResult)
        {
            if (battleInfo.dwTotalNum < 0)
            {
                battleInfo.dwTotalNum = 0;
            }
            battleInfo.dwTotalNum++;
            if (bGameResult == 1)
            {
                if (battleInfo.dwWinNum < 0)
                {
                    battleInfo.dwWinNum = 0;
                }
                battleInfo.dwWinNum++;
            }
        }

        public void Clean()
        {
            this.s_roleInfoContainer.Clear();
        }

        public void ClearMasterRoleInfo()
        {
            CRoleInfo info = !this.hasMasterUUID ? null : this.GetMasterRoleInfo();
            if (info != null)
            {
                info.Clear();
            }
        }

        public CRoleInfo CreateRoleInfo(enROLEINFO_TYPE type, ulong uuID)
        {
            ulong num = this.s_roleInfoContainer.AddRoleInfoByType(type, uuID);
            return this.GetRoleInfoByUUID(num);
        }

        public CRoleInfo GetMasterRoleInfo()
        {
            return this.GetRoleInfoByUUID(this.masterUUID);
        }

        public CRoleInfo GetRoleInfoByUUID(ulong uuID)
        {
            return this.s_roleInfoContainer.FindRoleInfoByID(uuID);
        }

        public override void Init()
        {
            this.s_roleInfoContainer = new CRoleInfoContainer();
            this.masterUUID = 0L;
        }

        public void SetMaterUUID(ulong InMaterUUID)
        {
            this.masterUUID = InMaterUUID;
        }

        public void UpdateLogic(int delta)
        {
            if (this.hasMasterUUID)
            {
                CRoleInfo masterRoleInfo = this.GetMasterRoleInfo();
                if (masterRoleInfo != null)
                {
                    masterRoleInfo.UpdateLogic(delta);
                }
            }
        }

        public bool hasMasterUUID
        {
            get
            {
                return (this.masterUUID != 0L);
            }
        }

        public ulong masterUUID { get; private set; }
    }
}

