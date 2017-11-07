namespace Assets.Scripts.GameLogic
{
    using AGE;
    using Assets.Scripts.Common;
    using ResData;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SSkillFuncContext
    {
        public PoolObjHandle<ActorRoot> inTargetObj;
        public PoolObjHandle<ActorRoot> inOriginator;
        public SkillUseContext inUseContext;
        public ResDT_SkillFunc inSkillFunc;
        public ESkillFuncStage inStage;
        public ResDT_IntParamArrayNode[] LocalParams;
        public Action inAction;
        public PoolObjHandle<BuffSkill> inBuffSkill;
        public HurtAttackerInfo inCustomData;
        public int inDoCount;
        public int inSkillLevel;
        public int inOverlayCount;
        public bool inLastEffect;
        public void InitSkillFuncContext()
        {
            if (this.inBuffSkill != 0)
            {
                if (this.inBuffSkill.handle.cfgData.bGrowthType == 0)
                {
                    if ((this.inOriginator != 0) && (this.inOriginator.handle.SkillControl != null))
                    {
                        SkillSlot skillSlot = null;
                        skillSlot = this.inOriginator.handle.SkillControl.GetSkillSlot(this.inUseContext.SlotType);
                        if (skillSlot != null)
                        {
                            this.inSkillLevel = skillSlot.GetSkillLevel();
                        }
                        else if (this.inOriginator.handle.ValueComponent != null)
                        {
                            this.inSkillLevel = this.inOriginator.handle.ValueComponent.actorSoulLevel;
                        }
                    }
                }
                else if (((this.inBuffSkill.handle.cfgData.bGrowthType == 1) && (this.inOriginator != 0)) && (this.inOriginator.handle.ValueComponent != null))
                {
                    this.inSkillLevel = this.inOriginator.handle.ValueComponent.actorSoulLevel;
                }
            }
        }

        public int GetSkillFuncParam(int _index, bool _bGrow)
        {
            if ((_index < 0) || ((_index + 1) > 6))
            {
                object[] inParameters = new object[] { _index };
                DebugHelper.Assert(false, "GetSkillFuncParam: index = {0}", inParameters);
            }
            if (_bGrow)
            {
                int iParam = this.inSkillFunc.astSkillFuncParam[_index].iParam;
                int num3 = this.inSkillFunc.astSkillFuncGroup[_index].iParam * (this.inSkillLevel - 1);
                iParam += num3;
                return (iParam * this.inOverlayCount);
            }
            return this.inSkillFunc.astSkillFuncParam[_index].iParam;
        }
    }
}

