namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using System;
    using UnityEngine;

    internal class MoveBulletDurationContext
    {
        public bool bAdjustSpeed;
        public bool bMoveRotate;
        public int destId;
        private VInt3 destPosition;
        public int distance;
        public int gravity;
        private AccelerateMotionControler gravityControler;
        private int hitHeight;
        public int lastTime;
        public int length;
        private VInt3 lerpDirection;
        public PoolObjHandle<ActorRoot> moveActor;
        private VInt3 moveDirection;
        public ActorMoveType MoveType;
        public VInt3 offsetDir;
        private SkillUseContext skillContext;
        public bool stopCondtion;
        public PoolObjHandle<ActorRoot> tarActor;
        public int targetId;
        public VInt3 targetPosition;
        public int velocity;

        private void ActionMoveLerp(ActorRoot actor, uint nDelta, bool bReset)
        {
            if ((actor != null) && !this.stopCondtion)
            {
                Vector3 one = Vector3.one;
                int newMagn = (int) ((this.velocity * nDelta) / 0x3e8);
                one = actor.gameObject.transform.position;
                if (this.gravity < 0)
                {
                    VInt num2;
                    this.lerpDirection.y = 0;
                    one += (Vector3) this.lerpDirection.NormalizeTo(newMagn);
                    one.y += ((float) this.gravityControler.GetMotionLerpDistance((int) nDelta)) / 1000f;
                    if (PathfindingUtility.GetGroundY(this.destPosition, out num2) && (one.y < ((float) num2)))
                    {
                        one.y = (float) num2;
                    }
                }
                else
                {
                    one += (Vector3) this.lerpDirection.NormalizeTo(newMagn);
                }
                actor.gameObject.transform.position = one;
            }
        }

        public void CopyData(ref MoveBulletDurationContext r)
        {
            this.length = r.length;
            this.targetId = r.targetId;
            this.destId = r.destId;
            this.MoveType = r.MoveType;
            this.targetPosition = r.targetPosition;
            this.offsetDir = r.offsetDir;
            this.velocity = r.velocity;
            this.distance = r.distance;
            this.gravity = r.gravity;
            this.bMoveRotate = r.bMoveRotate;
            this.bAdjustSpeed = r.bAdjustSpeed;
            this.skillContext = r.skillContext;
            this.destPosition = r.destPosition;
            this.lastTime = r.lastTime;
            this.hitHeight = r.hitHeight;
            this.tarActor = r.tarActor;
            this.moveActor = r.moveActor;
            this.gravityControler = r.gravityControler;
            this.stopCondtion = r.stopCondtion;
            this.moveDirection = r.moveDirection;
            this.lerpDirection = r.lerpDirection;
        }

        public void Enter(Action _action)
        {
            this.skillContext = _action.refParams.GetRefParamObject<SkillUseContext>("SkillContext");
            this.lastTime = 0;
            this.stopCondtion = false;
            this.moveActor = _action.GetActorHandle(this.targetId);
            if (this.moveActor != 0)
            {
                this.gravityControler = new AccelerateMotionControler();
                this.moveActor.handle.ObjLinker.AddCustomMoveLerp(new CustomMoveLerpFunc(this.ActionMoveLerp));
                if (this.MoveType == ActorMoveType.Target)
                {
                    this.tarActor = _action.GetActorHandle(this.destId);
                    if (this.tarActor == 0)
                    {
                        return;
                    }
                    this.destPosition = this.tarActor.handle.location;
                    CActorInfo charInfo = this.tarActor.handle.CharInfo;
                    if (charInfo != null)
                    {
                        this.hitHeight = charInfo.iBulletHeight;
                        VInt3 a = this.moveActor.handle.location - this.destPosition;
                        a.y = 0;
                        a = a.NormalizeTo(0x3e8);
                        this.destPosition += IntMath.Divide(a, (long) charInfo.iCollisionSize.x, 0x3e8L);
                    }
                    this.destPosition.y += this.hitHeight;
                }
                else if (this.MoveType == ActorMoveType.Directional)
                {
                    if (this.skillContext == null)
                    {
                        return;
                    }
                    PoolObjHandle<ActorRoot> originator = this.skillContext.Originator;
                    if (originator == 0)
                    {
                        return;
                    }
                    VInt3 num2 = originator.handle.forward.RotateY(this.offsetDir.y);
                    this.destPosition = this.moveActor.handle.location + num2.NormalizeTo(this.distance);
                    this.destPosition.y = this.moveActor.handle.location.y;
                }
                else if (this.MoveType == ActorMoveType.Position)
                {
                    this.destPosition = this.targetPosition;
                }
                if (this.bAdjustSpeed)
                {
                    VInt3 num3 = this.destPosition - this.moveActor.handle.location;
                    int num4 = this.length - 100;
                    num4 = (num4 > 0) ? num4 : this.length;
                    this.velocity = (int) IntMath.Divide((long) (num3.magnitude2D * 0x3e8L), (long) num4);
                }
                if (this.gravity < 0)
                {
                    if (this.velocity == 0)
                    {
                        this.stopCondtion = true;
                    }
                    else
                    {
                        VInt3 num5 = this.destPosition - this.moveActor.handle.location;
                        int num6 = (int) IntMath.Divide((long) (num5.magnitude2D * 0x3e8L), (long) this.velocity);
                        if (num6 == 0)
                        {
                            this.stopCondtion = true;
                        }
                        else
                        {
                            VInt num7;
                            if (PathfindingUtility.GetGroundY(this.destPosition, out num7))
                            {
                                this.gravityControler.InitMotionControler(num6, num7.i - this.moveActor.handle.location.y, this.gravity);
                            }
                            else
                            {
                                this.gravityControler.InitMotionControler(num6, 0, this.gravity);
                            }
                        }
                    }
                }
            }
        }

        public void Leave(Action _action, Track _track)
        {
            if (this.moveActor != 0)
            {
                this.moveActor.handle.ObjLinker.RmvCustomMoveLerp(new CustomMoveLerpFunc(this.ActionMoveLerp));
            }
            this.skillContext = null;
            this.tarActor.Release();
            this.moveActor.Release();
            this.gravityControler = null;
        }

        public void Process(Action _action, Track _track, int _localTime)
        {
            if ((this.moveActor != 0) && !this.stopCondtion)
            {
                int delta = _localTime - this.lastTime;
                this.lastTime = _localTime;
                this.ProcessInner(_action, _track, delta);
            }
        }

        public void ProcessInner(Action _action, Track _track, int delta)
        {
            VInt3 location = this.moveActor.handle.location;
            if ((this.MoveType == ActorMoveType.Target) && (this.tarActor != 0))
            {
                this.destPosition = this.tarActor.handle.location;
                if ((this.tarActor != 0) && (this.tarActor.handle.CharInfo != null))
                {
                    CActorInfo charInfo = this.tarActor.handle.CharInfo;
                    this.hitHeight = charInfo.iBulletHeight;
                    VInt3 a = this.moveActor.handle.location - this.destPosition;
                    a.y = 0;
                    a = a.NormalizeTo(0x3e8);
                    this.destPosition += IntMath.Divide(a, (long) charInfo.iCollisionSize.x, 0x3e8L);
                }
                this.destPosition.y += this.hitHeight;
            }
            this.moveDirection = this.destPosition - location;
            this.lerpDirection = this.moveDirection;
            if (this.bMoveRotate)
            {
                this.RotateMoveBullet(this.moveDirection);
            }
            int newMagn = (this.velocity * delta) / 0x3e8;
            if ((newMagn * newMagn) >= this.moveDirection.sqrMagnitudeLong2D)
            {
                this.moveActor.handle.location = this.destPosition;
                this.stopCondtion = true;
            }
            else
            {
                VInt3 num4;
                if (this.gravity < 0)
                {
                    VInt num5;
                    this.moveDirection.y = 0;
                    num4 = location + this.moveDirection.NormalizeTo(newMagn);
                    num4.y += this.gravityControler.GetMotionDeltaDistance(delta);
                    if (PathfindingUtility.GetGroundY(this.destPosition, out num5) && (num4.y < num5.i))
                    {
                        num4.y = num5.i;
                    }
                }
                else
                {
                    num4 = location + this.moveDirection.NormalizeTo(newMagn);
                }
                this.moveActor.handle.location = num4;
            }
            SkillUseContext refParamObject = _action.refParams.GetRefParamObject<SkillUseContext>("SkillContext");
            if (refParamObject != null)
            {
                refParamObject.EffectPos = this.moveActor.handle.location;
                refParamObject.EffectDir = this.moveDirection;
            }
        }

        public int ProcessSubdivide(Action _action, Track _track, int _localTime, int _count)
        {
            if (((this.moveActor == 0) || this.stopCondtion) || (_count <= 0))
            {
                return 0;
            }
            int num = _localTime - this.lastTime;
            this.lastTime = _localTime;
            int delta = num / _count;
            int num3 = num - delta;
            this.lastTime -= num3;
            this.ProcessInner(_action, _track, delta);
            return num3;
        }

        public void Reset(BulletTriggerDuration InBulletTrigger)
        {
            this.length = InBulletTrigger.length;
            this.targetId = InBulletTrigger.targetId;
            this.destId = InBulletTrigger.destId;
            this.MoveType = InBulletTrigger.MoveType;
            this.targetPosition = InBulletTrigger.targetPosition;
            this.offsetDir = InBulletTrigger.offsetDir;
            this.velocity = InBulletTrigger.velocity;
            this.distance = InBulletTrigger.distance;
            this.gravity = InBulletTrigger.gravity;
            this.bMoveRotate = InBulletTrigger.bMoveRotate;
            this.bAdjustSpeed = InBulletTrigger.bAdjustSpeed;
        }

        public void Reset(MoveBulletDuration InBulletDuration)
        {
            this.length = InBulletDuration.length;
            this.targetId = InBulletDuration.targetId;
            this.destId = InBulletDuration.destId;
            this.MoveType = InBulletDuration.MoveType;
            this.targetPosition = InBulletDuration.targetPosition;
            this.offsetDir = InBulletDuration.offsetDir;
            this.velocity = InBulletDuration.velocity;
            this.distance = InBulletDuration.distance;
            this.gravity = InBulletDuration.gravity;
            this.bMoveRotate = InBulletDuration.bMoveRotate;
            this.bAdjustSpeed = InBulletDuration.bAdjustSpeed;
        }

        private void RotateMoveBullet(VInt3 _dir)
        {
            if (((this.MoveType == ActorMoveType.Target) || (this.MoveType == ActorMoveType.Directional)) && (_dir != VInt3.zero))
            {
                this.moveActor.handle.forward = _dir.NormalizeTo(0x3e8);
                Quaternion identity = Quaternion.identity;
                identity = Quaternion.LookRotation((Vector3) _dir);
                this.moveActor.handle.rotation = identity;
            }
        }
    }
}

