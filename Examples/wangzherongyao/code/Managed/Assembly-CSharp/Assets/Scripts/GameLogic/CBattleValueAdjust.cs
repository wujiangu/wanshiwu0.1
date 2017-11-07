namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.GameKernal;
    using ResData;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class CBattleValueAdjust
    {
        private bool m_bSoulGrow;

        public static void BalanceComputerHeroProperty(List<PoolObjHandle<ActorRoot>> actorList)
        {
            if (actorList != null)
            {
                List<PoolObjHandle<ActorRoot>> list = new List<PoolObjHandle<ActorRoot>>();
                int count = actorList.Count;
                int num2 = 0;
                for (num2 = 0; num2 < count; num2++)
                {
                    if (actorList[num2] != 0)
                    {
                        PoolObjHandle<ActorRoot> handle = actorList[num2];
                        if (handle.handle.ValueComponent != null)
                        {
                            PoolObjHandle<ActorRoot> handle2 = actorList[num2];
                            if (handle2.handle.ValueComponent.mActorValue != null)
                            {
                                PoolObjHandle<ActorRoot> handle3 = actorList[num2];
                                Player player = Singleton<GamePlayerCenter>.instance.GetPlayer(handle3.handle.TheActorMeta.PlayerId);
                                if ((player != null) && player.Computer)
                                {
                                    PoolObjHandle<ActorRoot> handle4 = actorList[num2];
                                    if (handle4.handle.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero)
                                    {
                                        PoolObjHandle<ActorRoot> handle5 = actorList[num2];
                                        handle5.handle.ValueComponent.mActorValue.actorLvl = player.Level;
                                        list.Add(actorList[num2]);
                                    }
                                }
                            }
                        }
                    }
                }
                count = list.Count;
                for (num2 = 0; num2 < count; num2++)
                {
                    PoolObjHandle<ActorRoot> handle6 = list[num2];
                    ValueProperty valueComponent = handle6.handle.ValueComponent;
                    PoolObjHandle<ActorRoot> handle7 = list[num2];
                    SetHeroValueByBalanceLvl((uint) handle7.handle.TheActorMeta.ConfigId, valueComponent);
                }
            }
        }

        public static void BalanceMutiHeroProperty(List<PoolObjHandle<ActorRoot>> actorList)
        {
            List<PoolObjHandle<ActorRoot>> list = new List<PoolObjHandle<ActorRoot>>();
            int dwConfValue = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x29).dwConfValue;
            bool flag = Singleton<BattleLogic>.GetInstance().m_GameInfo.gameContext.IsPureBalanceProp();
            int count = actorList.Count;
            int num3 = 0;
            for (num3 = 0; num3 < count; num3++)
            {
                if (actorList[num3] != 0)
                {
                    PoolObjHandle<ActorRoot> handle = actorList[num3];
                    if (handle.handle.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero)
                    {
                        PoolObjHandle<ActorRoot> handle2 = actorList[num3];
                        ValueProperty valueComponent = handle2.handle.ValueComponent;
                        if ((valueComponent != null) && (valueComponent.mActorValue != null))
                        {
                            if (!flag && (dwConfValue < valueComponent.mActorValue.actorLvl))
                            {
                                dwConfValue = valueComponent.mActorValue.actorLvl;
                            }
                            list.Add(actorList[num3]);
                        }
                    }
                }
            }
            count = list.Count;
            for (num3 = 0; num3 < count; num3++)
            {
                PoolObjHandle<ActorRoot> handle3 = list[num3];
                ValueProperty propCom = handle3.handle.ValueComponent;
                PoolObjHandle<ActorRoot> handle4 = list[num3];
                BalanceProperty((uint) handle4.handle.TheActorMeta.ConfigId, ref propCom, dwConfValue);
                PoolObjHandle<ActorRoot> handle5 = list[num3];
                handle5.handle.ValueComponent.mActorValue.actorLvl = dwConfValue;
            }
        }

        private static void BalanceProperty(uint heroID, ref ValueProperty propCom, int maxLevel)
        {
            int dwConfValue = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x1f).dwConfValue;
            int num2 = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x20).dwConfValue;
            uint balanceInfoId = GetBalanceInfoId(heroID, maxLevel);
            ResHeroBalanceInfo dataByKey = GameDataMgr.heroBalanceDatabin.GetDataByKey(balanceInfoId);
            if (dataByKey == null)
            {
                Debug.LogError(string.Concat(new object[] { "ResHeroBalanceInfo can not find heroId = ", heroID, " level = ", maxLevel }));
            }
            else
            {
                int level = maxLevel - dwConfValue;
                level = (level > 0) ? level : 1;
                uint key = GetBalanceInfoId(heroID, level);
                ResHeroBalanceInfo info2 = GameDataMgr.heroBalanceDatabin.GetDataByKey(key);
                if (info2 == null)
                {
                    Debug.LogError(string.Concat(new object[] { "ResHeroBalanceInfo can not find heroId = ", heroID, " level = ", maxLevel - dwConfValue }));
                }
                else
                {
                    int num6 = maxLevel - num2;
                    num6 = (num6 > 0) ? num6 : 1;
                    uint num7 = GetBalanceInfoId(heroID, num6);
                    ResHeroBalanceInfo info3 = GameDataMgr.heroBalanceDatabin.GetDataByKey(num7);
                    if (info3 == null)
                    {
                        Debug.LogError(string.Concat(new object[] { "ResHeroBalanceInfo can not find heroId = ", heroID, " level = ", maxLevel - num2 }));
                    }
                    else
                    {
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT, (float) dataByKey.iPhyAtkPt, (float) info2.iPhyAtkPt, (float) info3.iPhyAtkPt);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT, (float) dataByKey.iMgcAtkPt, (float) info2.iMgcAtkPt, (float) info3.iMgcAtkPt);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT, (float) dataByKey.iPhyDefPt, (float) info2.iPhyDefPt, (float) info3.iPhyDefPt);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT, (float) dataByKey.iMgcDefPt, (float) info2.iMgcDefPt, (float) info3.iMgcDefPt);
                        int num8 = (propCom.actorHp * 100) / propCom.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP].totalValue;
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP, (float) dataByKey.iHp, (float) info2.iHp, (float) info3.iHp);
                        propCom.actorHp = (num8 * propCom.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP].totalValue) / 100;
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_CRITRATE, (float) dataByKey.iCritRate, (float) info2.iCritRate, (float) info3.iCritRate);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYARMORHURT, (float) dataByKey.iPhyArmorHurt, (float) info2.iPhyArmorHurt, (float) info3.iPhyArmorHurt);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCARMORHURT, (float) dataByKey.iMgcArmorHurt, (float) info2.iMgcArmorHurt, (float) info3.iMgcArmorHurt);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYVAMP, (float) dataByKey.iPhyVamp, (float) info2.iPhyVamp, (float) info3.iPhyVamp);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCVAMP, (float) dataByKey.iMgcVamp, (float) info2.iMgcVamp, (float) info3.iMgcVamp);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_ANTICRIT, (float) dataByKey.iAntiCrit, (float) info2.iAntiCrit, (float) info3.iAntiCrit);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_CRITEFT, (float) dataByKey.iCritEft, (float) info2.iCritEft, (float) info3.iCritEft);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_REALHURT, (float) dataByKey.iRealHurt, (float) info2.iRealHurt, (float) info3.iRealHurt);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_REALHURTLESS, (float) dataByKey.iRealHurtLess, (float) info2.iRealHurtLess, (float) info3.iRealHurtLess);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_CTRLREDUCE, (float) dataByKey.iCtrlReduce, (float) info2.iCtrlReduce, (float) info3.iCtrlReduce);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_BASEHURTADD, (float) dataByKey.iBaseHurtAdd, (float) info2.iBaseHurtAdd, (float) info3.iBaseHurtAdd);
                        BalancePropValue(ref propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_CDREDUCE, (float) dataByKey.iCdReduce, (float) info2.iCdReduce, (float) info3.iCdReduce);
                    }
                }
            }
        }

        private static void BalancePropValue(ref PropertyHelper propVal, RES_FUNCEFT_TYPE type, float standardVal, float lowLmtVal, float translowLmtVal)
        {
            ValueDataInfo info = propVal[type];
            int totalValue = info.totalValue;
            int num2 = 0;
            if (totalValue <= translowLmtVal)
            {
                num2 = (int) lowLmtVal;
            }
            else if (totalValue < standardVal)
            {
                DebugHelper.Assert((standardVal > translowLmtVal) && (standardVal > lowLmtVal));
                num2 = (int) ((((totalValue - translowLmtVal) / (standardVal - translowLmtVal)) * (standardVal - lowLmtVal)) + lowLmtVal);
            }
            else
            {
                num2 = (int) standardVal;
            }
            info.baseValue = num2;
            info.growValue = 0;
            info.addValue = 0;
            info.decValue = 0;
        }

        public static uint GetBalanceInfoId(uint heroId, int level)
        {
            return ((heroId * 0x3e8) + ((uint) level));
        }

        public static int GetPureBalanceLevel()
        {
            return (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x29).dwConfValue;
        }

        public void Init()
        {
            this.UnInit();
            this.m_bSoulGrow = Singleton<BattleLogic>.GetInstance().m_GameInfo.gameContext.IsSoulGrow();
            Singleton<GameEventSys>.instance.AddEventHandler<PoolObjHandle<ActorRoot>>(GameEventDef.Event_ActorStartFight, new RefAction<PoolObjHandle<ActorRoot>>(this.onActorStartFight));
        }

        private void onActorStartFight(ref PoolObjHandle<ActorRoot> src)
        {
            if ((src != 0) && ((this.m_bSoulGrow && (src.handle.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero)) && (src.handle.ValueComponent != null)))
            {
                SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
                if ((curLvelContext != null) && (curLvelContext.BirthLevelConfig > 0))
                {
                    src.handle.ValueComponent.ForceSetSoulLevel(curLvelContext.BirthLevelConfig);
                    src.handle.ValueComponent.actorHp = src.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP].totalValue;
                    src.handle.ValueComponent.actorEp = src.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_PROPERTY_MAXEP].totalValue;
                }
                else
                {
                    src.handle.ValueComponent.actorSoulLevel = 1;
                }
            }
        }

        public static void SetHeroBalanceValueArr(uint heroId, ref ValueDataInfo[] dataArr, int level)
        {
            if (dataArr.Length >= 0x24)
            {
                uint balanceInfoId = GetBalanceInfoId(heroId, level);
                ResHeroBalanceInfo dataByKey = GameDataMgr.heroBalanceDatabin.GetDataByKey(balanceInfoId);
                if (dataByKey != null)
                {
                    SetPropValue(dataArr[1], dataByKey.iPhyAtkPt);
                    SetPropValue(dataArr[3], dataByKey.iPhyDefPt);
                    SetPropValue(dataArr[2], dataByKey.iMgcAtkPt);
                    SetPropValue(dataArr[4], dataByKey.iMgcDefPt);
                    SetPropValue(dataArr[5], dataByKey.iHp);
                    SetPropValue(dataArr[6], dataByKey.iCritRate);
                    SetPropValue(dataArr[12], dataByKey.iCritEft);
                    SetPropValue(dataArr[13], dataByKey.iRealHurt);
                    SetPropValue(dataArr[14], dataByKey.iRealHurtLess);
                    SetPropValue(dataArr[0x11], dataByKey.iCtrlReduce);
                    SetPropValue(dataArr[0x13], dataByKey.iBaseHurtAdd);
                    SetPropValue(dataArr[20], dataByKey.iCdReduce);
                    SetPropValue(dataArr[7], dataByKey.iPhyArmorHurt);
                    SetPropValue(dataArr[8], dataByKey.iMgcArmorHurt);
                    SetPropValue(dataArr[9], dataByKey.iPhyVamp);
                    SetPropValue(dataArr[10], dataByKey.iMgcVamp);
                    SetPropValue(dataArr[11], dataByKey.iAntiCrit);
                }
            }
        }

        private static void SetHeroValueByBalanceLvl(uint heroId, ValueProperty propCom)
        {
            uint balanceInfoId = GetBalanceInfoId(heroId, propCom.mActorValue.actorLvl);
            ResHeroBalanceInfo dataByKey = GameDataMgr.heroBalanceDatabin.GetDataByKey(balanceInfoId);
            if (dataByKey == null)
            {
                Debug.LogError(string.Concat(new object[] { "ResHeroBalanceInfo can not find heroId = ", heroId, " level = ", propCom.mActorValue.actorLvl }));
            }
            else
            {
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT, dataByKey.iPhyAtkPt);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT, dataByKey.iMgcAtkPt);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT, dataByKey.iPhyDefPt);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT, dataByKey.iMgcDefPt);
                VFactor hpRate = propCom.GetHpRate();
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP, dataByKey.iHp);
                propCom.SetHpByRate(hpRate);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_CRITRATE, dataByKey.iCritRate);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYARMORHURT, dataByKey.iPhyArmorHurt);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCARMORHURT, dataByKey.iMgcArmorHurt);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYVAMP, dataByKey.iPhyVamp);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCVAMP, dataByKey.iMgcVamp);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_ANTICRIT, dataByKey.iAntiCrit);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_CRITEFT, dataByKey.iCritEft);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_REALHURT, dataByKey.iRealHurt);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_REALHURTLESS, dataByKey.iRealHurtLess);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_CTRLREDUCE, dataByKey.iCtrlReduce);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_BASEHURTADD, dataByKey.iBaseHurtAdd);
                SetPropValue(propCom.mActorValue, RES_FUNCEFT_TYPE.RES_FUNCEFT_CDREDUCE, dataByKey.iCdReduce);
            }
        }

        public static void SetPropValue(ValueDataInfo info, int balanceVal)
        {
            info.baseValue = balanceVal;
            info.growValue = 0;
            info.addValue = 0;
            info.decValue = 0;
            info.addRatio = 0;
            info.decRatio = 0;
        }

        private static void SetPropValue(PropertyHelper propVal, RES_FUNCEFT_TYPE type, int balanceVal)
        {
            propVal[type].baseValue = balanceVal;
            propVal[type].growValue = 0;
            propVal[type].addValue = 0;
            propVal[type].decValue = 0;
            propVal[type].addRatio = 0;
            propVal[type].decRatio = 0;
        }

        public void UnInit()
        {
            Singleton<GameEventSys>.instance.RmvEventHandler<PoolObjHandle<ActorRoot>>(GameEventDef.Event_ActorStartFight, new RefAction<PoolObjHandle<ActorRoot>>(this.onActorStartFight));
        }
    }
}

