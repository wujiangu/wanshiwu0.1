namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using ResData;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class SkillUseContext
    {
        public bool bSpecialUse;
        public int GatherTime;
        public object Instigator;
        public PoolObjHandle<ActorRoot> Originator;
        public PoolObjHandle<ActorRoot> TargetActor;

        public SkillUseContext()
        {
            this.GatherTime = 1;
            this.SlotType = SkillSlotType.SLOT_SKILL_VALID;
        }

        public SkillUseContext(SkillSlotType InSlot)
        {
            this.GatherTime = 1;
            this.SlotType = InSlot;
            this.bSpecialUse = false;
            this.AppointType = SkillRangeAppointType.Target;
        }

        private SkillUseContext(SkillUseContext rhs)
        {
            this.GatherTime = 1;
            this.SlotType = rhs.SlotType;
            this.AppointType = rhs.AppointType;
            this.TargetID = rhs.TargetID;
            this.UseVector = rhs.UseVector;
            this.EndVector = rhs.EndVector;
            this.TargetActor = rhs.TargetActor;
            this.Originator = rhs.Originator;
            this.Instigator = rhs.Instigator;
            this.bSpecialUse = rhs.bSpecialUse;
            this.GatherTime = rhs.GatherTime;
        }

        public SkillUseContext(SkillSlotType InSlot, VInt3 InVec)
        {
            this.GatherTime = 1;
            this.SlotType = InSlot;
            this.UseVector = InVec;
            this.AppointType = SkillRangeAppointType.Pos;
            this.bSpecialUse = false;
        }

        public SkillUseContext(SkillSlotType InSlot, PoolObjHandle<ActorRoot> InActorRoot)
        {
            this.GatherTime = 1;
            this.SlotType = InSlot;
            this.TargetActor = InActorRoot;
            this.bSpecialUse = false;
        }

        public SkillUseContext(SkillSlotType InSlot, uint ObjID)
        {
            this.GatherTime = 1;
            this.SlotType = InSlot;
            this.TargetID = ObjID;
            this.TargetActor = Singleton<GameObjMgr>.GetInstance().GetActor(this.TargetID);
            this.UseVector = (this.TargetActor == 0) ? VInt3.zero : this.TargetActor.handle.location;
            this.AppointType = SkillRangeAppointType.Target;
            this.bSpecialUse = false;
        }

        public SkillUseContext(SkillSlotType InSlot, VInt3 InVec, bool bSpecial)
        {
            this.GatherTime = 1;
            this.SlotType = InSlot;
            this.UseVector = InVec;
            this.AppointType = SkillRangeAppointType.Directional;
            this.bSpecialUse = bSpecial;
        }

        public SkillUseContext(SkillSlotType InSlot, VInt3 InBegin, VInt3 InEnd)
        {
            this.GatherTime = 1;
            this.SlotType = InSlot;
            this.UseVector = InBegin;
            this.EndVector = InEnd;
            this.bSpecialUse = false;
        }

        public bool CalcAttackerDir(out VInt3 dir, PoolObjHandle<ActorRoot> attacker)
        {
            if (attacker == 0)
            {
                dir = VInt3.forward;
                return false;
            }
            dir = attacker.handle.forward;
            switch (this.AppointType)
            {
                case SkillRangeAppointType.Auto:
                case SkillRangeAppointType.Target:
                    if (this.TargetActor == 0)
                    {
                        return false;
                    }
                    dir = this.TargetActor.handle.location;
                    dir -= attacker.handle.location;
                    dir.y = 0;
                    break;

                case SkillRangeAppointType.Pos:
                    dir = this.UseVector;
                    dir -= attacker.handle.location;
                    dir.y = 0;
                    break;

                case SkillRangeAppointType.Directional:
                    dir = this.UseVector;
                    dir.y = 0;
                    break;

                case SkillRangeAppointType.Track:
                {
                    VInt3 num = this.EndVector + this.UseVector;
                    dir = num.DivBy2() - attacker.handle.location;
                    dir.y = 0;
                    break;
                }
            }
            return true;
        }

        public SkillUseContext Clone()
        {
            return new SkillUseContext(this);
        }

        public void SetOriginator(PoolObjHandle<ActorRoot> inOriginator)
        {
            this.Originator = inOriginator;
        }

        public SkillRangeAppointType AppointType { get; set; }

        public VInt3 EffectDir { get; set; }

        public VInt3 EffectPos { get; set; }

        public VInt3 EndVector { get; private set; }

        public SkillSlotType SlotType { get; set; }

        public uint TargetID { get; private set; }

        public VInt3 UseVector { get; private set; }
    }
}

