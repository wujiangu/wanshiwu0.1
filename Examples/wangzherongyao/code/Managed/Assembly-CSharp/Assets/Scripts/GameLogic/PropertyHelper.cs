namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.DataCenter;
    using Assets.Scripts.GameSystem;
    using CSProtocol;
    using ResData;
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class PropertyHelper
    {
        private CrypticInt32 _curExp = 0;
        private ENERGY_TYPE _energyType = ENERGY_TYPE.None;
        private int _epRecFrequency;
        private CrypticInt32 _level = 1;
        private CrypticInt32 _maxExp = 0;
        private CrypticInt32 _quality = 1;
        private CrypticInt32 _soulLevel = 1;
        private CrypticInt32 _star = 1;
        private CrypticInt32 _subQuality = 0;
        public ActorMeta m_theActorMeta;
        private ValueDataInfo[] mActorValue = new ValueDataInfo[0x24];
        public static int[] s_symbolPropValAddArr = new int[0x24];

        public event ValueChangeDelegate ExpChgEvent;

        public event ValueChangeDelegate LvlChgEvent;

        public event ValueChangeDelegate QualityChgEvent;

        public event ValueChangeDelegate StarChgEvent;

        public PropertyHelper()
        {
            this._level = 1;
            this._curExp = 0;
            this._maxExp = 0;
            this._star = 1;
            this._quality = 1;
            this._subQuality = 0;
            this._soulLevel = 1;
        }

        public void AddEquipAttToHeroInfo(uint equipID)
        {
            ResEquipInfo dataByKey = GameDataMgr.equipInfoDatabin.GetDataByKey(equipID);
            if (dataByKey != null)
            {
                ushort wType = 0;
                byte bValType = 0;
                int val = 0;
                for (int i = 0; i < dataByKey.astFuncEftList.Length; i++)
                {
                    wType = dataByKey.astFuncEftList[i].wType;
                    bValType = dataByKey.astFuncEftList[i].bValType;
                    val = dataByKey.astFuncEftList[i].iValue;
                    if (val != 0)
                    {
                        this.ChangeFuncEft((RES_FUNCEFT_TYPE) wType, (RES_VALUE_TYPE) bValType, val, false);
                    }
                }
            }
        }

        public void AddGearProp(uint gearId, int addLevel, bool bAddBaseVal)
        {
            ResGearInfo dataByKey = GameDataMgr.gearInfoDatabin.GetDataByKey(gearId);
            if (dataByKey != null)
            {
                int val = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (dataByKey.astFuncEftList[i].wType > 0)
                    {
                        if (dataByKey.astFuncEftList[i].bValType == 0)
                        {
                            if (bAddBaseVal)
                            {
                                val = dataByKey.astFuncEftList[i].iValue + ((addLevel * dataByKey.astFuncEftList[i].iAddValue) / 0x2710);
                            }
                            else
                            {
                                val = (addLevel * dataByKey.astFuncEftList[i].iAddValue) / 0x2710;
                            }
                        }
                        else if (dataByKey.astFuncEftList[i].bValType == 1)
                        {
                            if (bAddBaseVal)
                            {
                                val = dataByKey.astFuncEftList[i].iValue + (addLevel * dataByKey.astFuncEftList[i].iAddValue);
                            }
                            else
                            {
                                val = addLevel * dataByKey.astFuncEftList[i].iAddValue;
                            }
                        }
                        this.ChangeFuncEft((RES_FUNCEFT_TYPE) dataByKey.astFuncEftList[i].wType, (RES_VALUE_TYPE) dataByKey.astFuncEftList[i].bValType, val, false);
                    }
                }
            }
        }

        public void AddHeroAdvanceAttToHeroInfo(uint heroID, int iQuality, int iSubQuality)
        {
            string s = heroID.ToString() + iQuality.ToString() + iSubQuality.ToString();
            ResHeroAdvanceInfo dataByKey = GameDataMgr.heroAdvanceDatabin.GetDataByKey(uint.Parse(s));
            ResHeroCfgInfo info2 = GameDataMgr.heroDatabin.GetDataByKey(heroID);
            if (dataByKey != null)
            {
                this.mActorValue[5].addValue += dataByKey.iHpAddVal;
                this.mActorValue[1].addValue += dataByKey.iAtkAddVal;
                this.mActorValue[2].addValue += dataByKey.iSpellAddVal;
                this.mActorValue[3].addValue += dataByKey.iDefAddVal;
                this.mActorValue[4].addValue += dataByKey.iResistAddVal;
            }
        }

        public void AddHeroStepAttToHeroInfo(uint heroID)
        {
            <AddHeroStepAttToHeroInfo>c__AnonStorey30 storey = new <AddHeroStepAttToHeroInfo>c__AnonStorey30 {
                heroID = heroID,
                nowQuality = this.actorQuality,
                nowSubQuality = this.actorSubQuality
            };
            ResHeroAdvanceInfo info = null;
            bool flag = true;
            while (flag)
            {
                info = GameDataMgr.heroAdvanceDatabin.FindIf(new Func<ResHeroAdvanceInfo, bool>(storey, (IntPtr) this.<>m__19));
                if (info != null)
                {
                    this.AddHeroAdvanceAttToHeroInfo(storey.heroID, info.iQuality, info.iSubQuality);
                    storey.heroID = info.dwHeroID;
                    storey.nowQuality = info.iQuality;
                    storey.nowSubQuality = info.iSubQuality;
                    info = null;
                }
                else
                {
                    flag = false;
                }
            }
            this.AddHeroAdvanceAttToHeroInfo(storey.heroID, this.actorQuality, this.actorSubQuality);
        }

        public void AddSymbolPageAttToProp(ref ActorMeta meta, bool bPVPLevel)
        {
            for (int i = 0; i < 0x24; i++)
            {
                s_symbolPropValAddArr[i] = 0;
            }
            IGameActorDataProvider actorDataProvider = Singleton<ActorDataCenter>.GetInstance().GetActorDataProvider(GameActorDataProviderType.ServerDataProvider);
            ActorServerRuneData runeData = new ActorServerRuneData();
            int index = 0;
            int bValType = 0;
            int val = 0;
            for (int j = 0; j < 30; j++)
            {
                if (actorDataProvider.GetActorServerRuneData(ref meta, (ActorRunelSlot) j, ref runeData))
                {
                    ResSymbolInfo dataByKey = GameDataMgr.symbolInfoDatabin.GetDataByKey(runeData.RuneId);
                    if (dataByKey != null)
                    {
                        if (bPVPLevel)
                        {
                            for (int m = 0; m < dataByKey.astFuncEftList.Length; m++)
                            {
                                index = dataByKey.astFuncEftList[m].wType;
                                bValType = dataByKey.astFuncEftList[m].bValType;
                                val = dataByKey.astFuncEftList[m].iValue;
                                if (((index != 0) && (index < 0x24)) && (val != 0))
                                {
                                    switch (bValType)
                                    {
                                        case 0:
                                            s_symbolPropValAddArr[index] += val;
                                            break;

                                        case 1:
                                            this.ChangeFuncEft((RES_FUNCEFT_TYPE) index, (RES_VALUE_TYPE) bValType, val, true);
                                            break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int n = 0; n < dataByKey.astPveEftList.Length; n++)
                            {
                                index = dataByKey.astPveEftList[n].wType;
                                bValType = dataByKey.astPveEftList[n].bValType;
                                val = dataByKey.astPveEftList[n].iValue;
                                if (((index != 0) && (index < 0x24)) && (val != 0))
                                {
                                    switch (bValType)
                                    {
                                        case 0:
                                            s_symbolPropValAddArr[index] += val;
                                            break;

                                        case 1:
                                            this.ChangeFuncEft((RES_FUNCEFT_TYPE) index, (RES_VALUE_TYPE) bValType, val, true);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for (int k = 0; k < 0x24; k++)
            {
                int num9 = s_symbolPropValAddArr[k] / 100;
                if (num9 > 0)
                {
                    this.ChangeFuncEft((RES_FUNCEFT_TYPE) k, RES_VALUE_TYPE.TYPE_VALUE, num9, true);
                }
            }
        }

        public void ChangeFuncEft(RES_FUNCEFT_TYPE key, RES_VALUE_TYPE type, int val, bool bOffRatio = false)
        {
            if (this.mActorValue[(int) key] != null)
            {
                ValueDataInfo.ChangeValueData(ref this.mActorValue[(int) key], type, val, bOffRatio);
            }
        }

        public int DynamicAdjustor(ValueDataInfo vd, ValueDataType type)
        {
            if (type == ValueDataType.TYPE_TOTAL)
            {
                return this.GenericCalculator(vd, DynamicProperty.Adjustor(vd));
            }
            return this.GenericBaseCalculator(vd, DynamicProperty.Adjustor(vd));
        }

        public int DynamicAdjustorForMgcEffect(ValueDataInfo vd, ValueDataType type)
        {
            if (type != ValueDataType.TYPE_TOTAL)
            {
                return this.GenericBaseCalculator(vd, DynamicProperty.Adjustor(vd));
            }
            int num = this.GenericCalculator(vd, DynamicProperty.Adjustor(vd));
            if (vd.totalEftRatioByMgc > 0)
            {
                num += (vd.totalEftRatioByMgc * this.mActorValue[2].totalValue) / 0x2710;
            }
            return num;
        }

        public int EpBaseNumericalCalculator(ValueDataInfo vd, int baseValue)
        {
            return (baseValue + ((this.SoulLevel - 1) * vd.growValue));
        }

        public int EpBaseProportionCalculator(ValueDataInfo vd, int baseValue)
        {
            float num = (((float) baseValue) / 10000f) + (((this.SoulLevel - 1) * vd.growValue) / 10000f);
            return (int) num;
        }

        public int EpGrowCalculator(ValueDataInfo vd, ValueDataType type)
        {
            if (type == ValueDataType.TYPE_TOTAL)
            {
                return this.EpNumericalCalculator(vd, vd.baseValue);
            }
            return this.EpBaseNumericalCalculator(vd, vd.baseValue);
        }

        public int EpNumericalCalculator(ValueDataInfo vd, int baseValue)
        {
            int num = baseValue + ((this.SoulLevel - 1) * vd.growValue);
            long num2 = ((((num + vd.addValue) - vd.decValue) * ((0x2710 + vd.addRatio) - vd.decRatio)) / 0x2710L) + vd.addValueOffRatio;
            return (int) num2;
        }

        public int EpProportionCalculator(ValueDataInfo vd, int baseValue)
        {
            float num = (((float) baseValue) / 10000f) + (((this.SoulLevel - 1) * vd.growValue) / 10000f);
            float num2 = ((((num + vd.addValue) - vd.decValue) * ((0x2710 + vd.addRatio) - vd.decRatio)) / 10000f) + vd.addValueOffRatio;
            return (int) num2;
        }

        public int EpRecCalculator(ValueDataInfo vd, ValueDataType type)
        {
            if (type == ValueDataType.TYPE_TOTAL)
            {
                return this.EpProportionCalculator(vd, vd.baseValue);
            }
            return this.EpBaseProportionCalculator(vd, vd.baseValue);
        }

        public int GenericBaseCalculator(ValueDataInfo vd, int baseValue)
        {
            return (baseValue + (((this.SoulLevel - 1) * vd.growValue) / 0x2710));
        }

        public int GenericCalculator(ValueDataInfo vd, int baseValue)
        {
            int num = baseValue + (((this.SoulLevel - 1) * vd.growValue) / 0x2710);
            long num2 = ((((num + vd.addValue) - vd.decValue) * ((0x2710 + vd.addRatio) - vd.decRatio)) / 0x2710L) + vd.addValueOffRatio;
            if (vd.maxLimitValue > 0)
            {
                num2 = (num2 <= vd.maxLimitValue) ? num2 : ((long) vd.maxLimitValue);
            }
            return (int) num2;
        }

        public ValueDataInfo[] GetActorValue()
        {
            return this.mActorValue;
        }

        private int GetPropMaxValueLimit(RES_FUNCEFT_TYPE funcType, bool bPvpMode)
        {
            if (bPvpMode)
            {
                ResPropertyValueInfo dataByKey = GameDataMgr.propertyValInfo.GetDataByKey((byte) funcType);
                return ((dataByKey == null) ? 0 : dataByKey.iMaxLimitValue);
            }
            return 0;
        }

        public int GrowCalculator(ValueDataInfo vd, ValueDataType type)
        {
            if (type == ValueDataType.TYPE_TOTAL)
            {
                return this.GenericCalculator(vd, vd.baseValue);
            }
            return this.GenericBaseCalculator(vd, vd.baseValue);
        }

        public void Init(ref ActorMeta actorMeta)
        {
            this.InitValueDataArr(ref actorMeta, false);
            IGameActorDataProvider actorDataProvider = Singleton<ActorDataCenter>.instance.GetActorDataProvider(GameActorDataProviderType.ServerDataProvider);
            ActorServerData actorData = new ActorServerData();
            if (actorMeta.ActorType == ActorTypeDef.Actor_Type_Hero)
            {
                ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey(actorMeta.ConfigId);
                ResHeroEnergyInfo info2 = GameDataMgr.heroEnergyDatabin.GetDataByKey(dataByKey.dwEnergyType);
                this.EpRecFrequency = info2.iRecFrequency;
            }
            if (actorDataProvider.GetActorServerData(ref actorMeta, ref actorData))
            {
                this.actorLvl = (int) actorData.Level;
                this.actorExp = (int) actorData.Exp;
                this.actorStar = (int) actorData.Star;
                this.actorQuality = (int) actorData.TheQualityInfo.Quality;
                this.actorSubQuality = (int) actorData.TheQualityInfo.SubQuality;
            }
            else
            {
                if (actorMeta.ActorType == ActorTypeDef.Actor_Type_Monster)
                {
                    IGameActorDataProvider provider2 = Singleton<ActorDataCenter>.instance.GetActorDataProvider(GameActorDataProviderType.StaticBattleDataProvider);
                    ActorStaticData data2 = new ActorStaticData();
                    this.actorLvl = !provider2.GetActorStaticData(ref actorMeta, ref data2) ? 1 : data2.TheMonsterOnlyInfo.MonsterBaseLevel;
                }
                else
                {
                    this.actorLvl = 1;
                }
                this.actorExp = 0;
                this.actorStar = 1;
                this.actorQuality = 1;
                this.actorSubQuality = 0;
            }
        }

        public void Init(COMDT_HEROINFO svrInfo)
        {
            ActorMeta theActorMeta = new ActorMeta {
                ConfigId = (int) svrInfo.stCommonInfo.dwHeroID
            };
            this.InitValueDataArr(ref theActorMeta, true);
            this.actorLvl = svrInfo.stCommonInfo.wLevel;
            this.actorExp = (int) svrInfo.stCommonInfo.dwExp;
            this.actorStar = svrInfo.stCommonInfo.wStar;
            this.actorQuality = svrInfo.stCommonInfo.stQuality.wQuality;
            this.actorSubQuality = svrInfo.stCommonInfo.stQuality.wSubQuality;
            this.SetSkinProp(svrInfo.stCommonInfo.dwHeroID, svrInfo.stCommonInfo.wSkinID, true);
        }

        public void InitValueDataArr(ref ActorMeta theActorMeta, bool bLobby)
        {
            IGameActorDataProvider actorDataProvider = null;
            if (bLobby)
            {
                actorDataProvider = Singleton<ActorDataCenter>.instance.GetActorDataProvider(GameActorDataProviderType.StaticLobbyDataProvider);
            }
            else
            {
                actorDataProvider = Singleton<ActorDataCenter>.instance.GetActorDataProvider(GameActorDataProviderType.StaticBattleDataProvider);
            }
            ActorStaticData actorData = new ActorStaticData();
            actorDataProvider.GetActorStaticData(ref theActorMeta, ref actorData);
            this.m_theActorMeta = theActorMeta;
            this.EnergyType = (ENERGY_TYPE) actorData.TheBaseAttribute.EpType;
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            bool bPvpMode = !bLobby ? curLvelContext.isPVPMode : true;
            this.mActorValue[5] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP, actorData.TheBaseAttribute.BaseHp, actorData.TheBaseAttribute.PerLvHp, new ValueCalculator(this.DynamicAdjustorForMgcEffect), (int) actorData.TheBaseAttribute.DynamicProperty, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP, bPvpMode));
            this.mActorValue[1] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT, actorData.TheBaseAttribute.BaseAd, actorData.TheBaseAttribute.PerLvAd, new ValueCalculator(this.DynamicAdjustorForMgcEffect), (int) actorData.TheBaseAttribute.DynamicProperty, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT, bPvpMode));
            this.mActorValue[2] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT, actorData.TheBaseAttribute.BaseAp, actorData.TheBaseAttribute.PerLvAp, new ValueCalculator(this.DynamicAdjustor), (int) actorData.TheBaseAttribute.DynamicProperty, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT, bPvpMode));
            this.mActorValue[3] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT, actorData.TheBaseAttribute.BaseDef, actorData.TheBaseAttribute.PerLvDef, new ValueCalculator(this.DynamicAdjustor), (int) actorData.TheBaseAttribute.DynamicProperty, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT, bPvpMode));
            this.mActorValue[4] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT, actorData.TheBaseAttribute.BaseRes, actorData.TheBaseAttribute.PerLvRes, new ValueCalculator(this.DynamicAdjustor), (int) actorData.TheBaseAttribute.DynamicProperty, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT, bPvpMode));
            this.mActorValue[0x20] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_PROPERTY_MAXEP, actorData.TheBaseAttribute.BaseEp, actorData.TheBaseAttribute.EpGrowth, new ValueCalculator(this.EpGrowCalculator), 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_PROPERTY_MAXEP, bPvpMode));
            this.mActorValue[0x21] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_PROPERTY_EPRECOVER, actorData.TheBaseAttribute.BaseEpRecover, actorData.TheBaseAttribute.PerLvEpRecover, new ValueCalculator(this.EpRecCalculator), 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_PROPERTY_EPRECOVER, bPvpMode));
            this.mActorValue[0x22] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_PROPERTY_PHYARMORHURT_RATE, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_PROPERTY_PHYARMORHURT_RATE, bPvpMode));
            this.mActorValue[0x23] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_PROPERTY_MGCARMORHURT_RATE, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_PROPERTY_MGCARMORHURT_RATE, bPvpMode));
            this.mActorValue[0x15] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_SightArea, actorData.TheBaseAttribute.Sight, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_SightArea, bPvpMode));
            this.mActorValue[15] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_MOVESPD, actorData.TheBaseAttribute.MoveSpeed, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_MOVESPD, bPvpMode));
            this.mActorValue[0x10] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_HPRECOVER, actorData.TheBaseAttribute.BaseHpRecover, actorData.TheBaseAttribute.PerLvHpRecover, new ValueCalculator(this.GrowCalculator), 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_HPRECOVER, bPvpMode));
            this.mActorValue[0x12] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_ATKSPDADD, actorData.TheBaseAttribute.BaseAtkSpeed, actorData.TheBaseAttribute.PerLvAtkSpeed, new ValueCalculator(this.GrowCalculator), 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_ATKSPDADD, bPvpMode));
            this.mActorValue[6] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_CRITRATE, actorData.TheBaseAttribute.CriticalChance, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_CRITRATE, bPvpMode));
            this.mActorValue[12] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_CRITEFT, actorData.TheBaseAttribute.CriticalDamage, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_CRITEFT, bPvpMode));
            this.mActorValue[11] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_ANTICRIT, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_ANTICRIT, bPvpMode));
            this.mActorValue[0x16] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_HitRate, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_HitRate, bPvpMode));
            this.mActorValue[0x17] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_HitRateAvoid, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_HitRateAvoid, bPvpMode));
            this.mActorValue[13] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_REALHURT, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_REALHURT, bPvpMode));
            this.mActorValue[14] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_REALHURTLESS, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_REALHURTLESS, bPvpMode));
            this.mActorValue[7] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYARMORHURT, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYARMORHURT, bPvpMode));
            this.mActorValue[8] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCARMORHURT, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCARMORHURT, bPvpMode));
            this.mActorValue[0x13] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_BASEHURTADD, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_BASEHURTADD, bPvpMode));
            this.mActorValue[9] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYVAMP, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYVAMP, bPvpMode));
            this.mActorValue[10] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCVAMP, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCVAMP, bPvpMode));
            this.mActorValue[0x11] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_CTRLREDUCE, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_CTRLREDUCE, bPvpMode));
            this.mActorValue[20] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_FUNCEFT_CDREDUCE, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_FUNCEFT_CDREDUCE, bPvpMode));
            this.mActorValue[0x18] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_PROPERTY_CRITICAL, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_PROPERTY_CRITICAL, bPvpMode));
            this.mActorValue[0x19] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_PROPERTY_REDUCECRITICAL, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_PROPERTY_REDUCECRITICAL, bPvpMode));
            this.mActorValue[0x1a] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_PROPERTY_PHYSICSHEM, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_PROPERTY_PHYSICSHEM, bPvpMode));
            this.mActorValue[0x1b] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_PROPERTY_MAGICHEM, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_PROPERTY_MAGICHEM, bPvpMode));
            this.mActorValue[0x1c] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_PROPERTY_ATTACKSPEED, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_PROPERTY_ATTACKSPEED, bPvpMode));
            this.mActorValue[0x1d] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_PROPERTY_TENACITY, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_PROPERTY_TENACITY, bPvpMode));
            this.mActorValue[30] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_PROPERTY_HURTREDUCERATE, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_PROPERTY_HURTREDUCERATE, bPvpMode));
            this.mActorValue[0x1f] = new ValueDataInfo(RES_FUNCEFT_TYPE.RES_PROPERTY_HURTOUTPUTRATE, 0, 0, null, 0, this.GetPropMaxValueLimit(RES_FUNCEFT_TYPE.RES_PROPERTY_HURTOUTPUTRATE, bPvpMode));
        }

        public void OnHeroSoulLvlUp(PoolObjHandle<ActorRoot> hero, int level)
        {
            PoolObjHandle<ActorRoot> captain = Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain;
            if ((hero != 0) && (hero == captain))
            {
                this.SoulLevel = level;
            }
        }

        public void RemoveGearProp(uint gearId, int level)
        {
            ResGearInfo dataByKey = GameDataMgr.gearInfoDatabin.GetDataByKey(gearId);
            if (dataByKey != null)
            {
                int num = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (dataByKey.astFuncEftList[i].wType > 0)
                    {
                        if (dataByKey.astFuncEftList[i].bValType == 0)
                        {
                            num = dataByKey.astFuncEftList[i].iValue + ((int) (((level - dataByKey.dwInitLevel) * dataByKey.astFuncEftList[i].iAddValue) / 0x2710));
                        }
                        else if (dataByKey.astFuncEftList[i].bValType == 1)
                        {
                            num = dataByKey.astFuncEftList[i].iValue + ((int) ((level - dataByKey.dwInitLevel) * dataByKey.astFuncEftList[i].iAddValue));
                        }
                        this.ChangeFuncEft((RES_FUNCEFT_TYPE) dataByKey.astFuncEftList[i].wType, (RES_VALUE_TYPE) dataByKey.astFuncEftList[i].bValType, -num, false);
                    }
                }
            }
        }

        public void SetChangeEvent(RES_FUNCEFT_TYPE key, ValueChangeDelegate func)
        {
            if (this.mActorValue[(int) key] != null)
            {
                this.mActorValue[(int) key].ChangeEvent += func;
                func();
            }
        }

        public void SetSkinProp(uint heroId, uint skinId, bool bWear)
        {
            ResHeroSkin heroSkin = CSkinInfo.GetHeroSkin(heroId, skinId);
            DebugHelper.Assert(heroSkin != null, "Skin==null");
            ushort wType = 0;
            byte bValType = 0;
            int val = 0;
            for (int i = 0; i < 15; i++)
            {
                wType = heroSkin.astAttr[i].wType;
                bValType = heroSkin.astAttr[i].bValType;
                val = heroSkin.astAttr[i].iValue;
                if ((wType != 0) && (val != 0))
                {
                    if (bWear)
                    {
                        this.ChangeFuncEft((RES_FUNCEFT_TYPE) wType, (RES_VALUE_TYPE) bValType, val, true);
                    }
                    else
                    {
                        this.ChangeFuncEft((RES_FUNCEFT_TYPE) wType, (RES_VALUE_TYPE) bValType, -val, true);
                    }
                }
            }
        }

        public int actorExp
        {
            get
            {
                return (int) this._curExp;
            }
            set
            {
                this._curExp = value;
                if (this.ExpChgEvent != null)
                {
                    this.ExpChgEvent();
                }
            }
        }

        public int actorLvl
        {
            get
            {
                return (int) this._level;
            }
            set
            {
                ResHeroLvlUpInfo dataByKey = GameDataMgr.heroLvlUpDatabin.GetDataByKey((uint) value);
                if (dataByKey != null)
                {
                    this._level = value;
                    this._maxExp = (CrypticInt32) dataByKey.dwExp;
                    if (this.LvlChgEvent != null)
                    {
                        this.LvlChgEvent();
                    }
                }
            }
        }

        public int actorMaxExp
        {
            get
            {
                return (int) this._maxExp;
            }
            set
            {
                this._maxExp = value;
            }
        }

        public int actorQuality
        {
            get
            {
                return (int) this._quality;
            }
            set
            {
                this._quality = value;
                this._subQuality = 0;
                if (this.QualityChgEvent != null)
                {
                    this.QualityChgEvent();
                }
            }
        }

        public int actorStar
        {
            get
            {
                return (int) this._star;
            }
            set
            {
                this._star = value;
                if (value > 1)
                {
                    IGameActorDataProvider actorDataProvider = Singleton<ActorDataCenter>.instance.GetActorDataProvider(GameActorDataProviderType.StaticLobbyDataProvider);
                    ActorMeta meta = new ActorMeta {
                        ConfigId = this.m_theActorMeta.ConfigId
                    };
                    this.m_theActorMeta = meta;
                    ActorPerStarLvData perStarLvData = new ActorPerStarLvData();
                    if (actorDataProvider.GetActorStaticPerStarLvData(ref this.m_theActorMeta, (ActorStarLv) value, ref perStarLvData))
                    {
                        this.mActorValue[5].growValue = perStarLvData.PerLvHp;
                        this.mActorValue[1].growValue = perStarLvData.PerLvAd;
                        this.mActorValue[2].growValue = perStarLvData.PerLvAp;
                        this.mActorValue[3].growValue = perStarLvData.PerLvDef;
                        this.mActorValue[4].growValue = perStarLvData.PerLvRes;
                    }
                    if (this.StarChgEvent != null)
                    {
                        this.StarChgEvent();
                    }
                }
            }
        }

        public int actorSubQuality
        {
            get
            {
                return (int) this._subQuality;
            }
            set
            {
                this._subQuality = value;
                if (this.QualityChgEvent != null)
                {
                    this.QualityChgEvent();
                }
            }
        }

        public ENERGY_TYPE EnergyType
        {
            get
            {
                return this._energyType;
            }
            set
            {
                this._energyType = value;
            }
        }

        public int EpRecFrequency
        {
            get
            {
                return this._epRecFrequency;
            }
            set
            {
                this._epRecFrequency = value;
            }
        }

        public ValueDataInfo this[RES_FUNCEFT_TYPE key]
        {
            get
            {
                return this.mActorValue[(int) key];
            }
        }

        public int SoulLevel
        {
            get
            {
                return (int) this._soulLevel;
            }
            set
            {
                this._soulLevel = value;
            }
        }

        [CompilerGenerated]
        private sealed class <AddHeroStepAttToHeroInfo>c__AnonStorey30
        {
            internal uint heroID;
            internal int nowQuality;
            internal int nowSubQuality;

            internal bool <>m__19(ResHeroAdvanceInfo x)
            {
                return (((x.dwHeroID == this.heroID) && (x.iAdvQuality == this.nowQuality)) && (x.iAdvSubQuality == this.nowSubQuality));
            }
        }
    }
}

