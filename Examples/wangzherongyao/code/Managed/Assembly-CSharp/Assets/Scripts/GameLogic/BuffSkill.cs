namespace Assets.Scripts.GameLogic
{
    using AGE;
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using ResData;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class BuffSkill : BaseSkill
    {
        private ResBattleParam battleParam;
        public bool bExtraBuff;
        public ResSkillCombineCfgInfo cfgData;
        public ulong controlTime;
        public int[] CustomParams = new int[6];
        public const int MaxCustomParam = 6;
        private int overlayCount;
        public SkillUseContext skillContext;
        public ulong ulStartTime;

        private bool CheckUseRule(SkillUseContext context)
        {
            if (context.TargetActor.handle.ActorControl.GetNoAbilityFlag(ObjAbilityType.ObjAbility_ImmuneNegative) && ((this.cfgData.dwEffectType == 1) || (this.cfgData.dwEffectType == 2)))
            {
                return false;
            }
            if (context.TargetActor.handle.ActorControl.GetNoAbilityFlag(ObjAbilityType.ObjAbility_ImmuneControl) && (this.cfgData.dwEffectType == 2))
            {
                return false;
            }
            return true;
        }

        private void DealTenacity(PoolObjHandle<ActorRoot> target)
        {
            int inLengthMs = 0;
            int totalValue = 0;
            int num3 = 0;
            int num4 = 0;
            ValueDataInfo info = null;
            info = target.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_PROPERTY_TENACITY];
            totalValue = info.totalValue;
            num4 = (int) ((totalValue + (target.handle.ValueComponent.mActorValue.actorLvl * this.battleParam.dwM_Tenacity)) + this.battleParam.dwN_Tenacity);
            if (num4 != 0)
            {
                num3 = (totalValue * 0x2710) / num4;
            }
            num3 += target.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_CTRLREDUCE].totalValue;
            inLengthMs = base.curAction.length;
            if (num3 != 0)
            {
                inLengthMs = (inLengthMs * (0x2710 - num3)) / 0x2710;
                base.curAction.ResetLength(inLengthMs, false);
            }
        }

        public bool FindSkillFunc(int inSkillFuncType, out ResDT_SkillFunc outSkillFunc)
        {
            outSkillFunc = null;
            if (this.cfgData != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (this.cfgData.astSkillFuncInfo[i].dwSkillFuncType == inSkillFuncType)
                    {
                        outSkillFunc = this.cfgData.astSkillFuncInfo[i];
                        return true;
                    }
                }
            }
            return false;
        }

        public int GetOverlayCount()
        {
            return this.overlayCount;
        }

        public void Init(int id)
        {
            base.SkillID = id;
            this.cfgData = GameDataMgr.skillCombineDatabin.GetDataByKey(id);
            if (this.cfgData != null)
            {
                base.ActionName = StringHelper.UTF8BytesToString(ref this.cfgData.szPrefab);
            }
            for (int i = 0; i < 6; i++)
            {
                this.CustomParams[i] = 0;
            }
            this.controlTime = 0L;
            this.battleParam = GameDataMgr.battleParam.GetAnyData();
            this.ulStartTime = Singleton<FrameSynchr>.GetInstance().LogicFrameTick;
        }

        public override void OnActionStoped(Action action)
        {
            PoolObjHandle<ActorRoot> handle = new PoolObjHandle<ActorRoot>();
            action.refParams.GetRefParam("TargetActor", ref handle);
            if (((handle != 0) && (handle.handle.BuffHolderComp != null)) && handle.handle.BuffHolderComp.bRemoveList)
            {
                handle.handle.BuffHolderComp.ActionRemoveBuff(this);
            }
            base.OnActionStoped(action);
        }

        public override void OnRelease()
        {
            this.overlayCount = 0;
            Array.Clear(this.CustomParams, 0, this.CustomParams.Length);
            this.bExtraBuff = false;
            this.cfgData = null;
            this.battleParam = null;
            base.OnRelease();
        }

        public override void OnUse()
        {
            base.OnUse();
            this.overlayCount = 0;
            this.controlTime = 0L;
            Array.Clear(this.CustomParams, 0, this.CustomParams.Length);
            this.bExtraBuff = false;
            this.cfgData = null;
            this.battleParam = null;
            this.skillContext = null;
            this.ulStartTime = 0L;
        }

        public void SetOverlayCount(int _count)
        {
            this.overlayCount = _count;
        }

        public override bool Use(PoolObjHandle<ActorRoot> user, SkillUseContext context)
        {
            if (((context == null) || (context.TargetActor == 0)) || (this.cfgData == null))
            {
                return false;
            }
            this.skillContext = context.Clone();
            BuffHolderComponent buffHolderComp = context.TargetActor.handle.BuffHolderComp;
            if (buffHolderComp == null)
            {
                return false;
            }
            if (!this.CheckUseRule(context))
            {
                return false;
            }
            if (!buffHolderComp.overlayRule.CheckOverlay(this))
            {
                return false;
            }
            bool flag = false;
            bool flag2 = false;
            VInt3 forward = VInt3.forward;
            switch (context.AppointType)
            {
                case SkillRangeAppointType.Pos:
                    flag = true;
                    break;

                case SkillRangeAppointType.Directional:
                    flag2 = true;
                    forward = context.UseVector;
                    break;

                case SkillRangeAppointType.Track:
                    flag = true;
                    flag2 = true;
                    forward = context.EndVector - context.UseVector;
                    if (forward.sqrMagnitudeLong < 1L)
                    {
                        forward = VInt3.forward;
                    }
                    break;
            }
            GameObject obj2 = (context.Originator == 0) ? null : context.Originator.handle.gameObject;
            GameObject obj3 = (context.TargetActor == 0) ? null : context.TargetActor.handle.gameObject;
            GameObject[] objArray1 = new GameObject[] { obj2, obj3 };
            base.curAction = ActionManager.Instance.PlayAction(base.ActionName, true, false, objArray1);
            if (base.curAction == null)
            {
                return false;
            }
            base.curAction.onActionStop += new ActionStopDelegate(this.OnActionStoped);
            base.curAction.refParams.AddRefParam("SkillObj", this);
            base.curAction.refParams.AddRefParam("SkillContext", context);
            base.curAction.refParams.AddRefParam("TargetActor", context.TargetActor);
            base.curAction.refParams.SetRefParam("_BulletPos", context.EffectPos);
            base.curAction.refParams.SetRefParam("_BulletDir", context.EffectDir);
            if (flag)
            {
                base.curAction.refParams.SetRefParam("_TargetPos", context.UseVector);
            }
            if (flag2)
            {
                base.curAction.refParams.SetRefParam("_TargetDir", forward);
            }
            if (this.cfgData != null)
            {
                SkillSlotType slotType = context.SlotType;
                int iDuration = this.cfgData.iDuration;
                if ((slotType >= SkillSlotType.SLOT_SKILL_1) && (slotType <= SkillSlotType.SLOT_SKILL_3))
                {
                    int skillLevel = 1;
                    if ((context.Originator != 0) && (context.Originator.handle.SkillControl != null))
                    {
                        SkillSlot slot = null;
                        context.Originator.handle.SkillControl.TryGetSkillSlot(slotType, out slot);
                        if (slot != null)
                        {
                            skillLevel = slot.GetSkillLevel();
                        }
                    }
                    skillLevel = (skillLevel >= 1) ? skillLevel : 1;
                    iDuration += (skillLevel - 1) * this.cfgData.iDurationGrow;
                }
                base.curAction.ResetLength(iDuration, false);
                if (this.cfgData.dwEffectType == 2)
                {
                    this.DealTenacity(context.TargetActor);
                }
            }
            context.TargetActor.handle.BuffHolderComp.AddBuff(this);
            if (this.cfgData.dwShowType != 0)
            {
                SpawnBuffEventParam param = new SpawnBuffEventParam(this.cfgData.dwShowType, context.TargetActor);
                Singleton<GameSkillEventSys>.GetInstance().SendEvent<SpawnBuffEventParam>(GameSkillEventDef.Event_SpawnBuff, context.TargetActor, ref param, GameSkillEventChannel.Channel_HostCtrlActor);
            }
            if ((this.cfgData.dwEffectType == 2) && (this.cfgData.dwShowType != 2))
            {
                LimitMoveEventParam param2 = new LimitMoveEventParam(base.CurAction.length, base.SkillID, context.TargetActor);
                Singleton<GameSkillEventSys>.GetInstance().SendEvent<LimitMoveEventParam>(GameSkillEventDef.Event_LimitMove, context.TargetActor, ref param2, GameSkillEventChannel.Channel_AllActor);
            }
            return true;
        }

        public override bool isBuff
        {
            get
            {
                return true;
            }
        }

        public string SkillFuncCombineDesc
        {
            get
            {
                return Utility.UTF8Convert(this.cfgData.szSkillCombineDesc);
            }
        }

        public string SkillFuncCombineName
        {
            get
            {
                return Utility.UTF8Convert(this.cfgData.szSkillCombineName);
            }
        }
    }
}

