namespace Assets.Scripts.GameLogic.SkillFunc
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using ResData;
    using System;

    [SkillFuncHandlerClass]
    internal class SkillFuncPropertyDelegator
    {
        private static int GetChangeValueProperty(ActorRoot targetActor, ref SSkillFuncContext inContext)
        {
            int skillFuncParam = inContext.GetSkillFuncParam(1, true);
            int num2 = inContext.GetSkillFuncParam(2, true);
            int num3 = inContext.GetSkillFuncParam(3, true);
            ESkillFuncMode mode = (ESkillFuncMode) inContext.GetSkillFuncParam(4, false);
            int num4 = inContext.GetSkillFuncParam(5, true);
            skillFuncParam += ((num2 * targetActor.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue) / 0x2710) + ((num3 * targetActor.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT].totalValue) / 0x2710);
            if (mode == ESkillFuncMode.SkillFuncMode_Constant)
            {
                return skillFuncParam;
            }
            int num5 = 0;
            int dwSkillFuncFreq = (int) inContext.inSkillFunc.dwSkillFuncFreq;
            if (num4 <= 0)
            {
                DebugHelper.Assert(false, "ESkillFuncMode LastTime error!");
                return skillFuncParam;
            }
            if (dwSkillFuncFreq <= 0)
            {
                dwSkillFuncFreq = 30;
            }
            num5 = num4 / dwSkillFuncFreq;
            if (num5 <= 0)
            {
                return skillFuncParam;
            }
            if (mode == ESkillFuncMode.SkillFuncMode_Fade)
            {
                if (inContext.inDoCount == 1)
                {
                    return skillFuncParam;
                }
                if ((inContext.inDoCount - 1) <= num5)
                {
                    return (-skillFuncParam / num5);
                }
                return 0;
            }
            if ((inContext.inDoCount != 1) && ((inContext.inDoCount - 1) <= num5))
            {
                return (skillFuncParam / num5);
            }
            return 0;
        }

        [SkillFuncHandler(0x3a, new int[] {  })]
        public static bool OnSkillFuncAddEp(ref SSkillFuncContext inContext)
        {
            OnSKillFuncChangeEpValue(ref inContext, true);
            return true;
        }

        [SkillFuncHandler(0x2b, new int[] {  })]
        public static bool OnSkillFuncAnticrit(ref SSkillFuncContext inContext)
        {
            OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_ANTICRIT, true);
            return true;
        }

        [SkillFuncHandler(0x11, new int[] { 0x12 })]
        public static bool OnSkillFuncChangeAp(ref SSkillFuncContext inContext)
        {
            if (inContext.inSkillFunc.dwSkillFuncType == 0x11)
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT, true);
            }
            else
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT, false);
            }
            return true;
        }

        [SkillFuncHandler(8, new int[] { 9 })]
        public static bool OnSkillFuncChangeAtk(ref SSkillFuncContext inContext)
        {
            if (inContext.inSkillFunc.dwSkillFuncType == 8)
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT, true);
            }
            else
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT, false);
            }
            return true;
        }

        [SkillFuncHandler(4, new int[] { 5 })]
        public static bool OnSkillFuncChangeAtkSpd(ref SSkillFuncContext inContext)
        {
            if (inContext.inSkillFunc.dwSkillFuncType == 4)
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_ATKSPDADD, true);
            }
            else
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_ATKSPDADD, false);
            }
            return true;
        }

        [SkillFuncHandler(0x13, new int[] { 20 })]
        public static bool OnSkillFuncChangeCritStrikeRate(ref SSkillFuncContext inContext)
        {
            if (inContext.inSkillFunc.dwSkillFuncType == 0x13)
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_CRITRATE, true);
            }
            else
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_CRITRATE, false);
            }
            return true;
        }

        [SkillFuncHandler(11, new int[] { 12 })]
        public static bool OnSkillFuncChangeDefend(ref SSkillFuncContext inContext)
        {
            if (inContext.inSkillFunc.dwSkillFuncType == 11)
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT, true);
            }
            else
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT, false);
            }
            return true;
        }

        [SkillFuncHandler(0x17, new int[] { 0x18 })]
        public static bool OnSkillFuncChangeDefStrike(ref SSkillFuncContext inContext)
        {
            if (inContext.inSkillFunc.dwSkillFuncType == 0x17)
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYARMORHURT, true);
            }
            else
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYARMORHURT, false);
            }
            return true;
        }

        private static void OnSKillFuncChangeEpValue(ref SSkillFuncContext inContext, bool _bAddValue)
        {
            PoolObjHandle<ActorRoot> inTargetObj = inContext.inTargetObj;
            if (inTargetObj != 0)
            {
                int changeValueProperty = GetChangeValueProperty((ActorRoot) inTargetObj, ref inContext);
                int skillFuncParam = inContext.GetSkillFuncParam(0, false);
                if (inContext.inStage != ESkillFuncStage.Leave)
                {
                    if (_bAddValue)
                    {
                        inTargetObj.handle.ValueComponent.ChangeActorEp(changeValueProperty, skillFuncParam);
                    }
                    else
                    {
                        inTargetObj.handle.ValueComponent.ChangeActorEp(-changeValueProperty, skillFuncParam);
                    }
                }
            }
        }

        [SkillFuncHandler(0x15, new int[] { 0x16 })]
        public static bool OnSkillFuncChangeMaxHp(ref SSkillFuncContext inContext)
        {
            VFactor one = VFactor.one;
            if ((inContext.inTargetObj != 0) && (inContext.inTargetObj.handle.ValueComponent != null))
            {
                one = inContext.inTargetObj.handle.ValueComponent.GetHpRate();
            }
            if (inContext.inSkillFunc.dwSkillFuncType == 0x15)
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP, true);
            }
            else
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP, false);
            }
            if ((inContext.inTargetObj != 0) && (inContext.inTargetObj.handle.ValueComponent != null))
            {
                inContext.inTargetObj.handle.ValueComponent.SetHpByRate(one);
            }
            return true;
        }

        [SkillFuncHandler(60, new int[] {  })]
        public static bool OnSkillFuncChangeMgcArmorHurtRate(ref SSkillFuncContext inContext)
        {
            OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_PROPERTY_MGCARMORHURT_RATE, true);
            return true;
        }

        [SkillFuncHandler(0x3f, new int[] {  })]
        public static bool OnSkillFuncChangeMgcEffect(ref SSkillFuncContext inContext)
        {
            if (inContext.inStage == ESkillFuncStage.Enter)
            {
                int skillFuncParam = inContext.GetSkillFuncParam(0, false);
                int num2 = inContext.GetSkillFuncParam(2, false);
                int num3 = inContext.GetSkillFuncParam(4, false);
                ValueDataInfo info1 = inContext.inTargetObj.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT];
                info1.totalEftRatioByMgc += num2;
                ValueDataInfo info2 = inContext.inTargetObj.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP];
                info2.totalEftRatioByMgc += num3;
            }
            else if (inContext.inStage == ESkillFuncStage.Leave)
            {
                int num4 = inContext.GetSkillFuncParam(2, false);
                int num5 = inContext.GetSkillFuncParam(4, false);
                ValueDataInfo info3 = inContext.inTargetObj.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT];
                info3.totalEftRatioByMgc -= num4;
                ValueDataInfo info4 = inContext.inTargetObj.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP];
                info4.totalEftRatioByMgc -= num5;
            }
            return true;
        }

        [SkillFuncHandler(0x3e, new int[] {  })]
        public static bool OnSkillFuncChangeMgcRate(ref SSkillFuncContext inContext)
        {
            if (inContext.inStage == ESkillFuncStage.Enter)
            {
                int skillFuncParam = inContext.GetSkillFuncParam(3, false);
                ValueDataInfo info1 = inContext.inTargetObj.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT];
                int num2 = info1.totalEftRatio += skillFuncParam;
            }
            else if (inContext.inStage == ESkillFuncStage.Leave)
            {
                int num3 = inContext.GetSkillFuncParam(3, false);
                ValueDataInfo info2 = inContext.inTargetObj.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT];
                int num4 = info2.totalEftRatio -= num3;
            }
            return true;
        }

        [SkillFuncHandler(6, new int[] { 7 })]
        public static bool OnSkillFuncChangeMoveSpd(ref SSkillFuncContext inContext)
        {
            if (inContext.inSkillFunc.dwSkillFuncType == 6)
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_MOVESPD, true);
            }
            else
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_MOVESPD, false);
            }
            return true;
        }

        [SkillFuncHandler(0x41, new int[] {  })]
        public static bool OnSkillFuncChangeMoveSpdWhenInOutBattle(ref SSkillFuncContext inContext)
        {
            OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_MOVESPD, true);
            return true;
        }

        [SkillFuncHandler(0x3b, new int[] {  })]
        public static bool OnSkillFuncChangePhyArmorHurtRate(ref SSkillFuncContext inContext)
        {
            OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_PROPERTY_PHYARMORHURT_RATE, true);
            return true;
        }

        [SkillFuncHandler(13, new int[] { 14 })]
        public static bool OnSkillFuncChangeResist(ref SSkillFuncContext inContext)
        {
            if (inContext.inSkillFunc.dwSkillFuncType == 13)
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT, true);
            }
            else
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT, false);
            }
            return true;
        }

        [SkillFuncHandler(0x19, new int[] { 0x1a })]
        public static bool OnSkillFuncChangeRessStrike(ref SSkillFuncContext inContext)
        {
            if (inContext.inSkillFunc.dwSkillFuncType == 0x19)
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCARMORHURT, true);
            }
            else
            {
                OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCARMORHURT, false);
            }
            return true;
        }

        private static void OnSkillFuncChangeValueProperty(ref SSkillFuncContext inContext, RES_FUNCEFT_TYPE _defType, bool _bAddValue)
        {
            PoolObjHandle<ActorRoot> inTargetObj = inContext.inTargetObj;
            if (inTargetObj != 0)
            {
                int num = 0;
                int changeValueProperty = GetChangeValueProperty((ActorRoot) inTargetObj, ref inContext);
                int skillFuncParam = inContext.GetSkillFuncParam(0, false);
                if (inContext.inStage != ESkillFuncStage.Leave)
                {
                    if (_bAddValue)
                    {
                        if (skillFuncParam == 0)
                        {
                            num = ((int) inTargetObj.handle.ValueComponent.mActorValue[_defType]) + changeValueProperty;
                        }
                        else
                        {
                            num = (int) (inTargetObj.handle.ValueComponent.mActorValue[_defType] << changeValueProperty);
                        }
                        ResDT_IntParamArrayNode node1 = inContext.LocalParams[0];
                        node1.iParam += changeValueProperty;
                    }
                    else
                    {
                        if (skillFuncParam == 0)
                        {
                            num = ((int) inTargetObj.handle.ValueComponent.mActorValue[_defType]) - changeValueProperty;
                        }
                        else
                        {
                            num = (int) (inTargetObj.handle.ValueComponent.mActorValue[_defType] >> changeValueProperty);
                        }
                        ResDT_IntParamArrayNode node2 = inContext.LocalParams[0];
                        node2.iParam += changeValueProperty;
                    }
                }
                else if (_bAddValue)
                {
                    if (skillFuncParam == 0)
                    {
                        num = ((int) inTargetObj.handle.ValueComponent.mActorValue[_defType]) - inContext.LocalParams[0].iParam;
                    }
                    else
                    {
                        num = (int) (inTargetObj.handle.ValueComponent.mActorValue[_defType] >> inContext.LocalParams[0].iParam);
                    }
                }
                else if (skillFuncParam == 0)
                {
                    num = ((int) inTargetObj.handle.ValueComponent.mActorValue[_defType]) + inContext.LocalParams[0].iParam;
                }
                else
                {
                    num = (int) (inTargetObj.handle.ValueComponent.mActorValue[_defType] << inContext.LocalParams[0].iParam);
                }
            }
        }

        [SkillFuncHandler(0x39, new int[] {  })]
        public static bool OnSkillFuncCritEffect(ref SSkillFuncContext inContext)
        {
            OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_CRITEFT, true);
            return true;
        }

        [SkillFuncHandler(40, new int[] {  })]
        public static bool OnSkillFuncHurtOutputRate(ref SSkillFuncContext inContext)
        {
            OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_PROPERTY_HURTOUTPUTRATE, true);
            return true;
        }

        [SkillFuncHandler(0x25, new int[] {  })]
        public static bool OnSkillFuncHurtReduceRate(ref SSkillFuncContext inContext)
        {
            OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_PROPERTY_HURTREDUCERATE, true);
            return true;
        }

        [SkillFuncHandler(0x24, new int[] {  })]
        public static bool OnSkillFuncMagicHemo(ref SSkillFuncContext inContext)
        {
            OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCVAMP, true);
            return true;
        }

        [SkillFuncHandler(0x23, new int[] {  })]
        public static bool OnSkillFuncPhysHemo(ref SSkillFuncContext inContext)
        {
            OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYVAMP, true);
            return true;
        }

        [SkillFuncHandler(0x40, new int[] {  })]
        public static bool OnSkillFuncRecoveryEffect(ref SSkillFuncContext inContext)
        {
            if (inContext.inStage == ESkillFuncStage.Enter)
            {
                int skillFuncParam = inContext.GetSkillFuncParam(0, false);
                int num2 = inContext.GetSkillFuncParam(1, false);
                int num3 = inContext.GetSkillFuncParam(2, false);
                int num4 = inContext.GetSkillFuncParam(3, false);
                int num5 = inContext.GetSkillFuncParam(4, false);
                inContext.inBuffSkill.handle.CustomParams[0] = skillFuncParam;
                inContext.inBuffSkill.handle.CustomParams[1] = num2;
                inContext.inBuffSkill.handle.CustomParams[2] = num3;
                inContext.inBuffSkill.handle.CustomParams[3] = num4;
                inContext.inBuffSkill.handle.CustomParams[4] = num5;
            }
            return true;
        }

        [SkillFuncHandler(0x2a, new int[] {  })]
        public static bool OnSkillFuncReduceCD(ref SSkillFuncContext inContext)
        {
            OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_CDREDUCE, true);
            return true;
        }

        [SkillFuncHandler(0x29, new int[] {  })]
        public static bool OnSkillFuncReduceControl(ref SSkillFuncContext inContext)
        {
            OnSkillFuncChangeValueProperty(ref inContext, RES_FUNCEFT_TYPE.RES_FUNCEFT_CTRLREDUCE, true);
            return true;
        }
    }
}

