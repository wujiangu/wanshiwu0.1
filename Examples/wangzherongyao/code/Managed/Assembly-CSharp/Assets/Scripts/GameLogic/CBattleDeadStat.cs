namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;

    public class CBattleDeadStat
    {
        private int m_deadMonsterNum;
        private List<DeadRecord> m_deadRecordList = new List<DeadRecord>(0x20);
        public uint m_uiFBTime;

        public void Clear()
        {
            this.m_deadMonsterNum = 0;
            this.m_deadRecordList.Clear();
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.OnDeadRecord));
        }

        public int GetAllMonsterDeadNum()
        {
            return this.m_deadMonsterNum;
        }

        public int GetBaronDeadCount()
        {
            int num = 0;
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if ((record.actorType == ActorTypeDef.Actor_Type_Monster) && (record.actorSubType == 2))
                {
                    ResMonsterCfgInfo dataCfgInfoByCurLevelDiff = MonsterDataHelper.GetDataCfgInfoByCurLevelDiff(record.cfgId);
                    if ((dataCfgInfoByCurLevelDiff != null) && (dataCfgInfoByCurLevelDiff.bSoldierType == 8))
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        public int GetBaronDeadCount(COM_PLAYERCAMP killerCamp)
        {
            int num = 0;
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (((killerCamp == record.killerCamp) && (record.actorType == ActorTypeDef.Actor_Type_Monster)) && (record.actorSubType == 2))
                {
                    ResMonsterCfgInfo dataCfgInfoByCurLevelDiff = MonsterDataHelper.GetDataCfgInfoByCurLevelDiff(record.cfgId);
                    if ((dataCfgInfoByCurLevelDiff != null) && (dataCfgInfoByCurLevelDiff.bSoldierType == 8))
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        public int GetBaronDeadTime(int index)
        {
            int num = 0;
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if ((record.actorType == ActorTypeDef.Actor_Type_Monster) && (record.actorSubType == 2))
                {
                    ResMonsterCfgInfo dataCfgInfoByCurLevelDiff = MonsterDataHelper.GetDataCfgInfoByCurLevelDiff(record.cfgId);
                    if ((dataCfgInfoByCurLevelDiff != null) && (dataCfgInfoByCurLevelDiff.bSoldierType == 8))
                    {
                        if (num == index)
                        {
                            DeadRecord record2 = this.m_deadRecordList[i];
                            return record2.deadTime;
                        }
                        num++;
                    }
                }
            }
            return 0;
        }

        public int GetDeadNum(COM_PLAYERCAMP camp, ActorTypeDef actorType)
        {
            int num = 0;
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (actorType == record.actorType)
                {
                    DeadRecord record2 = this.m_deadRecordList[i];
                    if (camp == record2.camp)
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        public int GetDeadTime(COM_PLAYERCAMP camp, ActorTypeDef actorType, int index)
        {
            int num = 0;
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (actorType == record.actorType)
                {
                    DeadRecord record2 = this.m_deadRecordList[i];
                    if (camp == record2.camp)
                    {
                        if (num == index)
                        {
                            DeadRecord record3 = this.m_deadRecordList[i];
                            return record3.deadTime;
                        }
                        num++;
                    }
                }
            }
            return 0;
        }

        public byte GetDestroyTowerCount(COM_PLAYERCAMP killerCamp, int TowerType)
        {
            byte num = 0;
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (((killerCamp == record.killerCamp) && (record.actorType == ActorTypeDef.Actor_Type_Organ)) && (record.actorSubType == TowerType))
                {
                    num = (byte) (num + 1);
                }
            }
            return num;
        }

        public int GetDragonDeadTime(int index)
        {
            int num = 0;
            int dragonId = Singleton<BattleLogic>.instance.DragonId;
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (dragonId == record.cfgId)
                {
                    if (num == index)
                    {
                        DeadRecord record2 = this.m_deadRecordList[i];
                        return record2.deadTime;
                    }
                    num++;
                }
            }
            return 0;
        }

        public int GetDragonDeadTimeByPlayer(uint playerID, int index)
        {
            int num = 0;
            int dragonId = Singleton<BattleLogic>.instance.DragonId;
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (dragonId == record.cfgId)
                {
                    DeadRecord record2 = this.m_deadRecordList[i];
                    if (record2.AttackPlayerID == playerID)
                    {
                        if (num == index)
                        {
                            DeadRecord record3 = this.m_deadRecordList[i];
                            return record3.deadTime;
                        }
                        num++;
                    }
                }
            }
            return 0;
        }

        public int GetHeroDeadAtTime(uint playerID, int deadTime)
        {
            int num = 0;
            List<DeadRecord> list = new List<DeadRecord>();
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (((record.AttackPlayerID == playerID) && (record.actorType == ActorTypeDef.Actor_Type_Hero)) && (record.deadTime < deadTime))
                {
                    num++;
                }
            }
            return num;
        }

        public int GetKillBlueBaNumAtTime(uint playerID, int deadTime)
        {
            return this.GetKillSpecialMonsterNumAtTime(playerID, deadTime, 10);
        }

        public int GetKillDragonNum()
        {
            int num = 0;
            int dragonId = Singleton<BattleLogic>.instance.DragonId;
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (dragonId == record.cfgId)
                {
                    num++;
                }
                else
                {
                    DeadRecord record2 = this.m_deadRecordList[i];
                    if (record2.actorType == ActorTypeDef.Actor_Type_Monster)
                    {
                        DeadRecord record3 = this.m_deadRecordList[i];
                        if (record3.actorSubType == 2)
                        {
                            DeadRecord record4 = this.m_deadRecordList[i];
                            ResMonsterCfgInfo dataCfgInfoByCurLevelDiff = MonsterDataHelper.GetDataCfgInfoByCurLevelDiff(record4.cfgId);
                            if ((dataCfgInfoByCurLevelDiff != null) && (dataCfgInfoByCurLevelDiff.bSoldierType == 9))
                            {
                                num++;
                            }
                        }
                    }
                }
            }
            return num;
        }

        public int GetKillDragonNum(COM_PLAYERCAMP killerCamp)
        {
            int num = 0;
            int dragonId = Singleton<BattleLogic>.instance.DragonId;
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (killerCamp == record.killerCamp)
                {
                    DeadRecord record2 = this.m_deadRecordList[i];
                    if (dragonId == record2.cfgId)
                    {
                        num++;
                        continue;
                    }
                }
                DeadRecord record3 = this.m_deadRecordList[i];
                if (killerCamp == record3.killerCamp)
                {
                    DeadRecord record4 = this.m_deadRecordList[i];
                    if (record4.actorType == ActorTypeDef.Actor_Type_Monster)
                    {
                        DeadRecord record5 = this.m_deadRecordList[i];
                        if (record5.actorSubType == 2)
                        {
                            DeadRecord record6 = this.m_deadRecordList[i];
                            ResMonsterCfgInfo dataCfgInfoByCurLevelDiff = MonsterDataHelper.GetDataCfgInfoByCurLevelDiff(record6.cfgId);
                            if ((dataCfgInfoByCurLevelDiff != null) && (dataCfgInfoByCurLevelDiff.bSoldierType == 9))
                            {
                                num++;
                            }
                        }
                    }
                }
            }
            return num;
        }

        public int GetKillDragonNumAtTime(uint playerID, int deadTime)
        {
            int num = 0;
            int dragonId = Singleton<BattleLogic>.instance.DragonId;
            List<DeadRecord> list = new List<DeadRecord>();
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (((record.AttackPlayerID == playerID) && (record.cfgId == dragonId)) && (record.deadTime < deadTime))
                {
                    num++;
                }
            }
            return num;
        }

        public int GetKillDragonNumByPlayer(uint playerID)
        {
            int num = 0;
            int dragonId = Singleton<BattleLogic>.instance.DragonId;
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (dragonId == record.cfgId)
                {
                    DeadRecord record2 = this.m_deadRecordList[i];
                    if (record2.AttackPlayerID == playerID)
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        public int GetKillRedBaNumAtTime(uint playerID, int deadTime)
        {
            return this.GetKillSpecialMonsterNumAtTime(playerID, deadTime, 11);
        }

        public int GetKillSpecialMonsterNumAtTime(uint playerID, int deadTime, byte bySoldierType)
        {
            int num = 0;
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (((record.AttackPlayerID == playerID) && (record.actorType == ActorTypeDef.Actor_Type_Monster)) && (record.actorSubType == 2))
                {
                    ResMonsterCfgInfo dataCfgInfoByCurLevelDiff = MonsterDataHelper.GetDataCfgInfoByCurLevelDiff(record.cfgId);
                    if (((dataCfgInfoByCurLevelDiff != null) && (dataCfgInfoByCurLevelDiff.bSoldierType == bySoldierType)) && (record.deadTime < deadTime))
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        public int GetMonsterDeadAtTime(uint playerID, int deadTime)
        {
            int num = 0;
            List<DeadRecord> list = new List<DeadRecord>();
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (((record.AttackPlayerID == playerID) && (record.actorType == ActorTypeDef.Actor_Type_Monster)) && ((record.actorSubType == 2) && (record.deadTime < deadTime)))
                {
                    num++;
                }
            }
            return num;
        }

        public int GetOrganTimeByOrder(int iOrder)
        {
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (record.actorType == ActorTypeDef.Actor_Type_Organ)
                {
                    DeadRecord record2 = this.m_deadRecordList[i];
                    if (record2.iOrder == iOrder)
                    {
                        DeadRecord record3 = this.m_deadRecordList[i];
                        return record3.deadTime;
                    }
                }
            }
            return 0;
        }

        public int GetSoldierDeadAtTime(uint playerID, int deadTime)
        {
            int num = 0;
            List<DeadRecord> list = new List<DeadRecord>();
            for (int i = 0; i < this.m_deadRecordList.Count; i++)
            {
                DeadRecord record = this.m_deadRecordList[i];
                if (((record.AttackPlayerID == playerID) && (record.actorType == ActorTypeDef.Actor_Type_Monster)) && ((record.actorSubType == 1) && (record.deadTime < deadTime)))
                {
                    num++;
                }
            }
            return num;
        }

        private void OnDeadRecord(ref DefaultGameEventParam prm)
        {
            PoolObjHandle<ActorRoot> src = prm.src;
            PoolObjHandle<ActorRoot> orignalAtker = prm.orignalAtker;
            if ((src != 0) && (orignalAtker != 0))
            {
                if (src.handle.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero)
                {
                    this.m_deadRecordList.Add(new DeadRecord(src.handle.TheActorMeta.ActorCamp, src.handle.TheActorMeta.ActorType, src.handle.TheActorMeta.ConfigId, (int) Singleton<FrameSynchr>.instance.LogicFrameTick, orignalAtker.handle.TheActorMeta.ActorCamp, orignalAtker.handle.TheActorMeta.PlayerId));
                    if (this.m_uiFBTime == 0)
                    {
                        this.m_uiFBTime = (uint) Singleton<FrameSynchr>.instance.LogicFrameTick;
                    }
                }
                else if (src.handle.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Monster)
                {
                    DeadRecord item = new DeadRecord(src.handle.TheActorMeta.ActorCamp, src.handle.TheActorMeta.ActorType, src.handle.TheActorMeta.ConfigId, (int) Singleton<FrameSynchr>.instance.LogicFrameTick, orignalAtker.handle.TheActorMeta.ActorCamp, orignalAtker.handle.TheActorMeta.PlayerId);
                    if (src.handle.ActorControl != null)
                    {
                        item.actorSubType = src.handle.ActorControl.GetActorSubType();
                    }
                    this.m_deadRecordList.Add(item);
                    this.m_deadMonsterNum++;
                }
                else if ((src.handle.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Organ) && (((src.handle.TheStaticData.TheOrganOnlyInfo.OrganType == 1) || (src.handle.TheStaticData.TheOrganOnlyInfo.OrganType == 4)) || (src.handle.TheStaticData.TheOrganOnlyInfo.OrganType == 2)))
                {
                    DeadRecord record2 = new DeadRecord(src.handle.TheActorMeta.ActorCamp, src.handle.TheActorMeta.ActorType, src.handle.TheActorMeta.ConfigId, (int) Singleton<FrameSynchr>.instance.LogicFrameTick, orignalAtker.handle.TheActorMeta.ActorCamp, orignalAtker.handle.TheActorMeta.PlayerId);
                    if (src.handle.ObjLinker != null)
                    {
                        record2.iOrder = src.handle.ObjLinker.BattleOrder;
                        record2.actorSubType = (byte) src.handle.TheStaticData.TheOrganOnlyInfo.OrganType;
                    }
                    this.m_deadRecordList.Add(record2);
                }
            }
        }

        public void StartRecord()
        {
            this.Clear();
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.OnDeadRecord));
        }
    }
}

