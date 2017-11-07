namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using System;

    [EventCategory("MMGame/Skill")]
    public class ForbidAbilityDuration : DurationEvent
    {
        public bool abortFilterDamage;
        public bool abortFilterMove;
        public bool abortFilterSkill0;
        public bool abortFilterSkill1;
        public bool abortFilterSkill2;
        public bool abortFilterSkill3;
        public bool abortFilterSkill4;
        private PoolObjHandle<ActorRoot> actor_;
        [ObjectTemplate(new System.Type[] {  })]
        public int attackerId;
        public bool delaySkillAbort;
        public bool forbidMove;
        public bool forbidMoveRotate;
        public bool forbidSkill;
        public bool forbidSkillAbort = true;

        public override BaseEvent Clone()
        {
            ForbidAbilityDuration duration = ClassObjPool<ForbidAbilityDuration>.Get();
            duration.CopyData(this);
            return duration;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            ForbidAbilityDuration duration = src as ForbidAbilityDuration;
            this.attackerId = duration.attackerId;
            this.forbidMove = duration.forbidMove;
            this.forbidSkill = duration.forbidSkill;
            this.forbidSkillAbort = duration.forbidSkillAbort;
            this.forbidMoveRotate = duration.forbidMoveRotate;
            this.delaySkillAbort = duration.delaySkillAbort;
            this.abortFilterSkill0 = duration.abortFilterSkill0;
            this.abortFilterSkill1 = duration.abortFilterSkill1;
            this.abortFilterSkill2 = duration.abortFilterSkill2;
            this.abortFilterSkill3 = duration.abortFilterSkill3;
            this.abortFilterSkill4 = duration.abortFilterSkill4;
            this.abortFilterMove = duration.abortFilterMove;
            this.abortFilterDamage = duration.abortFilterDamage;
            this.actor_ = duration.actor_;
        }

        public override void Enter(Action _action, Track _track)
        {
            this.actor_ = _action.GetActorHandle(this.attackerId);
            if (this.actor_ != 0)
            {
                if (this.forbidMove)
                {
                    this.actor_.handle.ActorControl.AddNoAbilityFlag(ObjAbilityType.ObjAbility_Move);
                }
                if (this.forbidSkill)
                {
                    this.actor_.handle.ActorControl.AddDisableSkillFlag(SkillSlotType.SLOT_SKILL_COUNT);
                }
                if (this.delaySkillAbort && (this.actor_.handle.SkillControl.CurUseSkill != null))
                {
                    this.actor_.handle.SkillControl.CurUseSkill.bProtectAbortSkill = true;
                }
                if (this.forbidSkillAbort && (this.actor_.handle.SkillControl.CurUseSkill != null))
                {
                    this.actor_.handle.SkillControl.CurUseSkill.skillAbort.InitAbort(false);
                    if (this.abortFilterSkill0)
                    {
                        this.actor_.handle.SkillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_0);
                    }
                    if (this.abortFilterSkill1)
                    {
                        this.actor_.handle.SkillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_1);
                    }
                    if (this.abortFilterSkill2)
                    {
                        this.actor_.handle.SkillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_2);
                    }
                    if (this.abortFilterSkill3)
                    {
                        this.actor_.handle.SkillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_3);
                    }
                    if (this.abortFilterSkill4)
                    {
                        this.actor_.handle.SkillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_SKILL_4);
                    }
                    if (this.abortFilterMove)
                    {
                        this.actor_.handle.SkillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_MOVE);
                    }
                    if (this.abortFilterDamage)
                    {
                        this.actor_.handle.SkillControl.CurUseSkill.skillAbort.SetAbort(SkillAbortType.TYPE_DAMAGE);
                    }
                }
                if (this.forbidMoveRotate)
                {
                    this.actor_.handle.ActorControl.AddNoAbilityFlag(ObjAbilityType.ObjAbility_MoveRotate);
                }
            }
        }

        public override void Leave(Action _action, Track _track)
        {
            if (this.actor_ != 0)
            {
                if (this.forbidMove)
                {
                    this.actor_.handle.ActorControl.RmvNoAbilityFlag(ObjAbilityType.ObjAbility_Move);
                    if (!this.actor_.handle.ActorControl.GetNoAbilityFlag(ObjAbilityType.ObjAbility_Move))
                    {
                        DefaultGameEventParam prm = new DefaultGameEventParam(this.actor_, this.actor_);
                        Singleton<GameEventSys>.instance.SendEvent<DefaultGameEventParam>(GameEventDef.Event_ActorClearMove, ref prm);
                    }
                }
                if (this.forbidSkill)
                {
                    this.actor_.handle.ActorControl.RmvDisableSkillFlag(SkillSlotType.SLOT_SKILL_COUNT);
                }
                if (this.forbidSkillAbort && (this.actor_.handle.SkillControl.CurUseSkill != null))
                {
                    this.actor_.handle.SkillControl.CurUseSkill.skillAbort.InitAbort(true);
                }
                if (this.forbidMoveRotate)
                {
                    this.actor_.handle.ActorControl.RmvNoAbilityFlag(ObjAbilityType.ObjAbility_MoveRotate);
                }
                if (this.delaySkillAbort && (this.actor_.handle.SkillControl.CurUseSkill != null))
                {
                    this.actor_.handle.SkillControl.CurUseSkill.bProtectAbortSkill = false;
                    if (this.actor_.handle.SkillControl.CurUseSkill.bDelayAbortSkill)
                    {
                        this.actor_.handle.SkillControl.CurUseSkill.bDelayAbortSkill = false;
                        this.actor_.handle.SkillControl.ForceAbortCurUseSkill();
                    }
                }
            }
        }

        public override void OnUse()
        {
            base.OnUse();
            this.attackerId = 0;
            this.forbidMove = false;
            this.forbidSkill = false;
            this.forbidSkillAbort = true;
            this.forbidMoveRotate = false;
            this.delaySkillAbort = false;
            this.abortFilterSkill0 = false;
            this.abortFilterSkill1 = false;
            this.abortFilterSkill2 = false;
            this.abortFilterSkill3 = false;
            this.abortFilterSkill4 = false;
            this.abortFilterMove = false;
            this.abortFilterDamage = false;
            this.actor_.Release();
        }
    }
}

