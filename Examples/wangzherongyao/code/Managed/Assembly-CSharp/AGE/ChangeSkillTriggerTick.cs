namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using System;
    using System.Collections.Generic;

    [EventCategory("MMGame/Skill")]
    public class ChangeSkillTriggerTick : TickEvent
    {
        [ObjectTemplate(new Type[] {  })]
        public int attackTargetId = -1;
        public bool bCheckCondition;
        public bool bOvertimeCD = true;
        public bool bSendEvent = true;
        [AssetReference(AssetRefType.SkillID)]
        public int changeSkillID;
        [AssetReference(AssetRefType.SkillID)]
        public int changeSkillID2;
        [AssetReference(AssetRefType.SkillID)]
        public int changeSkillID3;
        public int effectTime;
        private static List<int> randomList = new List<int>(3);
        public int recoverSkillID;
        public SkillSlotType slotType;
        [ObjectTemplate(new Type[] {  })]
        public int targetId = -1;

        private bool CheckChangeSkillCondition(Action _action)
        {
            if (!this.bCheckCondition)
            {
                return true;
            }
            PoolObjHandle<ActorRoot> actorHandle = _action.GetActorHandle(this.attackTargetId);
            return ((actorHandle != 0) && actorHandle.handle.ActorControl.IsDeadState);
        }

        public override BaseEvent Clone()
        {
            ChangeSkillTriggerTick tick = ClassObjPool<ChangeSkillTriggerTick>.Get();
            tick.CopyData(this);
            return tick;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            ChangeSkillTriggerTick tick = src as ChangeSkillTriggerTick;
            this.targetId = tick.targetId;
            this.slotType = tick.slotType;
            this.effectTime = tick.effectTime;
            this.changeSkillID = tick.changeSkillID;
            this.changeSkillID2 = tick.changeSkillID2;
            this.changeSkillID3 = tick.changeSkillID3;
            this.recoverSkillID = tick.recoverSkillID;
            this.bCheckCondition = tick.bCheckCondition;
            this.attackTargetId = tick.attackTargetId;
            this.bOvertimeCD = tick.bOvertimeCD;
            this.bSendEvent = tick.bSendEvent;
        }

        public override void OnUse()
        {
            base.OnUse();
            this.targetId = -1;
            this.slotType = SkillSlotType.SLOT_SKILL_0;
            this.effectTime = 0;
            this.changeSkillID = 0;
            this.changeSkillID2 = 0;
            this.changeSkillID3 = 0;
            this.recoverSkillID = 0;
            this.bCheckCondition = false;
            this.bOvertimeCD = true;
            this.bSendEvent = true;
        }

        public override void Process(Action _action, Track _track)
        {
            PoolObjHandle<ActorRoot> actorHandle = _action.GetActorHandle(this.targetId);
            if (actorHandle == 0)
            {
                if (ActionManager.Instance.isPrintLog)
                {
                }
            }
            else
            {
                SkillComponent skillControl = actorHandle.handle.SkillControl;
                if (skillControl == null)
                {
                    if (ActionManager.Instance.isPrintLog)
                    {
                    }
                }
                else
                {
                    SkillSlot slot;
                    int num = this.RandomSkillID();
                    if ((num != 0) && (skillControl.TryGetSkillSlot(this.slotType, out slot) && (slot != null)))
                    {
                        if (!this.CheckChangeSkillCondition(_action))
                        {
                            slot.StartSkillCD();
                        }
                        else
                        {
                            int effectTime = this.effectTime;
                            slot.skillChangeEvent.Start(effectTime, num, this.recoverSkillID, this.bOvertimeCD, this.bSendEvent);
                        }
                    }
                }
            }
        }

        private int RandomSkillID()
        {
            randomList.Clear();
            if (this.changeSkillID != 0)
            {
                randomList.Add(this.changeSkillID);
            }
            if (this.changeSkillID2 != 0)
            {
                randomList.Add(this.changeSkillID2);
            }
            if (this.changeSkillID3 != 0)
            {
                randomList.Add(this.changeSkillID3);
            }
            if (randomList.Count == 1)
            {
                return randomList[0];
            }
            if (randomList.Count == 0)
            {
                return 0;
            }
            int num = FrameRandom.Random((uint) randomList.Count);
            return randomList[num];
        }
    }
}

