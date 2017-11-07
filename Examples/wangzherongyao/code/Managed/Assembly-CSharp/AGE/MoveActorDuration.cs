namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using System;
    using UnityEngine;

    [EventCategory("MMGame/Skill")]
    internal class MoveActorDuration : DurationCondition
    {
        private PoolObjHandle<ActorRoot> actor_;
        [ObjectTemplate(new System.Type[] {  })]
        public int actorId;
        public bool bRecordPosition;
        public bool bUseRecordPosition;
        [ObjectTemplate(new System.Type[] {  })]
        public int destId = -1;
        public VInt3 destPos = VInt3.zero;
        private VInt3 dir = VInt3.zero;
        private bool done_;
        public bool enableRotate = true;
        private VInt3 finalPos = VInt3.zero;
        private Quaternion fromRot = Quaternion.identity;
        public bool IgnoreCollision;
        private int lastTime_;
        public int minMoveDistance;
        public VInt3 moveDir = VInt3.zero;
        public int moveDistance;
        public int moveSpeed;
        private int moveTick;
        public ActorMoveType moveType;
        public int rotationTime;
        private VInt3 srcPos = VInt3.zero;
        public bool teleport;
        private Quaternion toRot = Quaternion.identity;

        private void ActionMoveLerp(ActorRoot actor, uint nDelta, bool bReset)
        {
            if (actor != null)
            {
                if (this.done_ || this.teleport)
                {
                    actor.gameObject.transform.position = (Vector3) actor.location;
                }
                else
                {
                    VInt groundY = 0;
                    VInt3 delta = this.dir.NormalizeTo((int) ((this.moveSpeed * nDelta) / 0x3e8));
                    Vector3 position = actor.gameObject.transform.position;
                    if (!this.IgnoreCollision)
                    {
                        delta = PathfindingUtility.MoveLerp(actor, (VInt3) position, delta, out groundY);
                    }
                    if (actor.MovementComponent.isFlying)
                    {
                        float y = position.y;
                        Transform transform = actor.gameObject.transform;
                        transform.position += (Vector3) delta;
                        Vector3 vector2 = actor.gameObject.transform.position;
                        vector2.y = y;
                        actor.gameObject.transform.position = vector2;
                    }
                    else
                    {
                        Transform transform2 = actor.gameObject.transform;
                        transform2.position += (Vector3) delta;
                    }
                }
            }
        }

        public override bool Check(Action _action, Track _track)
        {
            return this.done_;
        }

        public override BaseEvent Clone()
        {
            MoveActorDuration duration = ClassObjPool<MoveActorDuration>.Get();
            duration.CopyData(this);
            return duration;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            MoveActorDuration duration = src as MoveActorDuration;
            this.destId = duration.destId;
            this.actorId = duration.actorId;
            this.destPos = duration.destPos;
            this.moveDir = duration.moveDir;
            this.moveType = duration.moveType;
            this.moveDistance = duration.moveDistance;
            this.minMoveDistance = duration.minMoveDistance;
            this.moveSpeed = duration.moveSpeed;
            this.enableRotate = duration.enableRotate;
            this.rotationTime = duration.rotationTime;
            this.teleport = duration.teleport;
            this.IgnoreCollision = duration.IgnoreCollision;
            this.bRecordPosition = duration.bRecordPosition;
            this.bUseRecordPosition = duration.bUseRecordPosition;
            this.done_ = duration.done_;
            this.fromRot = duration.fromRot;
            this.toRot = duration.toRot;
            this.dir = duration.dir;
            this.moveTick = duration.moveTick;
            this.lastTime_ = duration.lastTime_;
            this.actor_ = duration.actor_;
        }

        public override void Enter(Action _action, Track _track)
        {
            this.done_ = false;
            this.lastTime_ = 0;
            base.Enter(_action, _track);
            this.actor_ = _action.GetActorHandle(this.actorId);
            if ((this.actor_ != 0) && (this.teleport || (this.moveSpeed != 0)))
            {
                this.srcPos = this.actor_.handle.location;
                if (this.bUseRecordPosition && (this.actor_.handle.SkillControl != null))
                {
                    this.destPos = this.actor_.handle.SkillControl.RecordPosition;
                    this.dir = this.destPos - this.srcPos;
                    VInt3 num5 = this.destPos - this.srcPos;
                    int magnitude = num5.magnitude;
                    this.moveDistance += magnitude;
                }
                else if (this.moveType == ActorMoveType.Target)
                {
                    int num3;
                    PoolObjHandle<ActorRoot> actorHandle = _action.GetActorHandle(this.destId);
                    if ((this.actor_ == 0) || (actorHandle == 0))
                    {
                        this.actor_.Release();
                        return;
                    }
                    if (actorHandle.handle.ActorControl.GetNoAbilityFlag(ObjAbilityType.ObjAbility_MoveCity))
                    {
                        num3 = 0;
                    }
                    else
                    {
                        this.dir = actorHandle.handle.location - this.srcPos;
                        VInt3 num6 = actorHandle.handle.location - this.srcPos;
                        num3 = num6.magnitude;
                    }
                    this.moveDistance += num3;
                }
                else if (this.moveType == ActorMoveType.Position)
                {
                    this.dir = this.destPos - this.srcPos;
                    VInt3 num7 = this.destPos - this.srcPos;
                    int minMoveDistance = num7.magnitude;
                    if (minMoveDistance < this.minMoveDistance)
                    {
                        minMoveDistance = this.minMoveDistance;
                    }
                    this.moveDistance += minMoveDistance;
                }
                else if (this.moveType == ActorMoveType.Directional)
                {
                    this.dir = this.actor_.handle.forward;
                }
                this.dir.y = 0;
                this.actor_.handle.ActorControl.TerminateMove();
                if (this.bRecordPosition && (this.actor_.handle.SkillControl != null))
                {
                    this.actor_.handle.SkillControl.RecordPosition = this.actor_.handle.location;
                }
                if (!this.actor_.handle.isMovable || (this.dir.sqrMagnitudeLong <= 1L))
                {
                    this.actor_.Release();
                    this.done_ = true;
                }
                else
                {
                    VInt3 dir = this.dir;
                    this.finalPos = this.srcPos + dir.NormalizeTo(this.moveDistance);
                    if (!PathfindingUtility.IsValidTarget((ActorRoot) this.actor_, this.finalPos))
                    {
                        this.IgnoreCollision = false;
                    }
                    this.moveTick = (this.moveDistance * 0x3e8) / this.moveSpeed;
                    this.actor_.handle.ActorControl.AddNoAbilityFlag(ObjAbilityType.ObjAbility_Move);
                    this.actor_.handle.ObjLinker.AddCustomMoveLerp(new CustomMoveLerpFunc(this.ActionMoveLerp));
                    this.fromRot = this.actor_.handle.rotation;
                    this.actor_.handle.MovementComponent.SetRotate(this.dir, true);
                    if (this.rotationTime > 0)
                    {
                        this.toRot = Quaternion.LookRotation((Vector3) this.actor_.handle.forward);
                    }
                    else
                    {
                        this.actor_.handle.rotation = Quaternion.LookRotation((Vector3) this.actor_.handle.forward);
                    }
                }
            }
        }

        public override void Leave(Action _action, Track _track)
        {
            if (this.actor_ != 0)
            {
                if (this.IgnoreCollision)
                {
                    bool flag = false;
                    _action.refParams.GetRefParam("_HitTargetHero", ref flag);
                    if (!flag)
                    {
                        this.SetFinalPos();
                    }
                    else
                    {
                        VInt3 zero = VInt3.zero;
                        _action.refParams.GetRefParam("_HitTargetHeroPos", ref zero);
                        if (!PathfindingUtility.IsValidTarget((ActorRoot) this.actor_, zero))
                        {
                            this.SetFinalPos();
                        }
                        else
                        {
                            this.actor_.handle.location = zero;
                        }
                    }
                }
                this.actor_.handle.ObjLinker.RmvCustomMoveLerp(new CustomMoveLerpFunc(this.ActionMoveLerp));
                this.actor_.handle.ActorControl.RmvNoAbilityFlag(ObjAbilityType.ObjAbility_Move);
                this.done_ = true;
            }
        }

        public override void OnUse()
        {
            base.OnUse();
            this.destId = -1;
            this.actorId = 0;
            this.destPos = VInt3.zero;
            this.moveDir = VInt3.zero;
            this.moveType = ActorMoveType.Target;
            this.moveDistance = 0;
            this.moveSpeed = 0;
            this.enableRotate = true;
            this.rotationTime = 0;
            this.teleport = false;
            this.IgnoreCollision = false;
            this.done_ = false;
            this.fromRot = Quaternion.identity;
            this.toRot = Quaternion.identity;
            this.srcPos = VInt3.zero;
            this.finalPos = VInt3.zero;
            this.dir = VInt3.zero;
            this.moveTick = 0;
            this.lastTime_ = 0;
            this.actor_.Release();
        }

        public override void Process(Action _action, Track _track, int _localTime)
        {
            if (!this.done_ && (this.actor_ != 0))
            {
                bool flag = this.lastTime_ < this.rotationTime;
                int moveTick = _localTime - this.lastTime_;
                this.lastTime_ = _localTime;
                if (this.teleport)
                {
                    this.Teleport();
                }
                else
                {
                    if (flag)
                    {
                        float t = Mathf.Min(1f, (float) (_localTime / this.rotationTime));
                        Quaternion quaternion = Quaternion.Slerp(this.fromRot, this.toRot, t);
                        this.actor_.handle.rotation = quaternion;
                    }
                    if ((this.moveTick - moveTick) <= 0)
                    {
                        moveTick = this.moveTick;
                        this.done_ = true;
                    }
                    else
                    {
                        this.moveTick -= moveTick;
                    }
                    VInt3 delta = this.dir.NormalizeTo((this.moveSpeed * moveTick) / 0x3e8);
                    VInt groundY = this.actor_.handle.groundY;
                    if (!this.IgnoreCollision)
                    {
                        delta = PathfindingUtility.Move(this.actor_.handle, delta, out groundY, out this.actor_.handle.hasReachedNavEdge);
                    }
                    if (this.actor_.handle.MovementComponent.isFlying)
                    {
                        int y = this.actor_.handle.location.y;
                        ActorRoot handle = this.actor_.handle;
                        handle.location += delta;
                        VInt3 location = this.actor_.handle.location;
                        location.y = y;
                        this.actor_.handle.location = location;
                    }
                    else
                    {
                        ActorRoot local2 = this.actor_.handle;
                        local2.location += delta;
                    }
                    this.actor_.handle.groundY = groundY;
                    base.Process(_action, _track, _localTime);
                }
            }
        }

        private void SetFinalPos()
        {
            VInt groundY = 0;
            PathfindingUtility.GetGroundY(this.finalPos, out groundY);
            this.finalPos.y = groundY.i;
            this.actor_.handle.location = this.finalPos;
        }

        public void Teleport()
        {
            VInt groundY = this.actor_.handle.groundY;
            VInt3 delta = this.dir.NormalizeTo(this.moveDistance);
            VInt3 target = this.actor_.handle.location + delta;
            if (PathfindingUtility.IsValidTarget((ActorRoot) this.actor_, target))
            {
                PathfindingUtility.GetGroundY(target, out groundY);
                target.y = groundY.i;
                this.actor_.handle.location = target;
            }
            else
            {
                delta = PathfindingUtility.Move(this.actor_.handle, delta, out groundY, out this.actor_.handle.hasReachedNavEdge);
                ActorRoot handle = this.actor_.handle;
                handle.location += delta;
            }
            this.actor_.handle.groundY = groundY;
            this.done_ = true;
        }
    }
}

