namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class SkillCache
    {
        private bool bCacheCommonAttack;
        private bool bSpecialCommonAttack;
        private bool cacheMove;
        private IFrameCommand cacheMoveCommand;
        private bool cacheMoveExpire;
        private bool cacheRotateExpire;
        private bool cacheSkill;
        private IFrameCommand cacheSkillCommand;
        private SkillUseContext cacheSkillContext;
        private bool cacheSkillExpire;
        private bool commonAttackMode;
        private bool moveToAttackTarget;
        private bool newAttackCommand;

        public void Clear()
        {
            this.commonAttackMode = false;
            this.cacheSkillExpire = true;
            this.cacheRotateExpire = true;
            this.cacheMoveExpire = true;
            this.cacheSkillCommand = null;
            this.cacheMoveCommand = null;
            this.bCacheCommonAttack = false;
            this.cacheSkill = false;
            this.cacheMove = false;
        }

        public bool GetCacheMoveExpire()
        {
            return this.cacheMoveExpire;
        }

        public bool GetCacheSkill()
        {
            return ((this.cacheSkillContext != null) && !this.cacheSkillExpire);
        }

        public bool GetCacheSkillSlotType(out SkillSlotType _slotType)
        {
            if ((this.cacheSkillContext != null) && !this.cacheSkillExpire)
            {
                _slotType = this.cacheSkillContext.SlotType;
                return true;
            }
            _slotType = SkillSlotType.SLOT_SKILL_VALID;
            return false;
        }

        public bool GetCommonAttackMode()
        {
            return this.commonAttackMode;
        }

        public bool GetSpecialCommonAttack()
        {
            return this.bSpecialCommonAttack;
        }

        public bool IsCacheCommonAttack()
        {
            return (this.cacheSkillExpire && this.bCacheCommonAttack);
        }

        public bool IsExistNewAttackCommand()
        {
            return this.newAttackCommand;
        }

        public void SetCacheMove(bool _bOpen)
        {
            this.cacheMove = _bOpen;
        }

        public void SetCacheMoveCommand(IFrameCommand _command)
        {
            if (this.cacheMove)
            {
                this.cacheMoveCommand = _command;
                this.cacheMoveExpire = false;
                this.cacheRotateExpire = false;
            }
        }

        public void SetCacheMoveExpire(bool bFlag)
        {
            this.cacheMoveExpire = true;
        }

        public void SetCacheNormalAttackContext(IFrameCommand _command)
        {
            if (this.cacheSkill)
            {
                this.cacheSkillCommand = _command;
                this.bCacheCommonAttack = true;
                this.cacheSkillExpire = false;
            }
        }

        public void SetCacheSkill(bool _bOpen)
        {
            this.cacheSkill = _bOpen;
        }

        public void SetCacheSkillContext(IFrameCommand cmd, SkillUseContext _context)
        {
            if (this.cacheSkill)
            {
                this.cacheSkillCommand = cmd;
                this.cacheSkillContext = _context;
                this.bCacheCommonAttack = false;
                this.cacheSkillExpire = false;
            }
        }

        public void SetCommonAttackMode(bool _bOpen)
        {
            this.commonAttackMode = _bOpen;
        }

        public void SetMoveToAttackTarget(bool bFlag)
        {
            this.moveToAttackTarget = bFlag;
            this.newAttackCommand = false;
        }

        public void SetNewAttackCommand()
        {
            if (this.moveToAttackTarget)
            {
                this.newAttackCommand = true;
            }
            else
            {
                this.newAttackCommand = false;
            }
        }

        public void SetSpecialCommonAttack(bool _bOpen)
        {
            this.bSpecialCommonAttack = _bOpen;
        }

        public void UseSkillCache(PoolObjHandle<ActorRoot> _actorRoot)
        {
            if (_actorRoot != 0)
            {
                if (!this.cacheSkillExpire)
                {
                    if (!this.bCacheCommonAttack)
                    {
                        if (this.cacheSkillContext != null)
                        {
                            SkillSlot skillSlot = _actorRoot.handle.SkillControl.GetSkillSlot(this.cacheSkillContext.SlotType);
                            if ((skillSlot != null) && skillSlot.IsEnableSkillSlot())
                            {
                                _actorRoot.handle.ActorControl.CmdUseSkill(this.cacheSkillCommand, this.cacheSkillContext);
                            }
                        }
                    }
                    else
                    {
                        _actorRoot.handle.ActorControl.CacheNoramlAttack(this.cacheSkillCommand, SkillSlotType.SLOT_SKILL_0);
                    }
                    this.cacheSkillExpire = true;
                }
                if ((this.cacheMoveCommand != null) && !this.cacheRotateExpire)
                {
                    if (this.cacheMoveCommand.cmdType == 2)
                    {
                        FrameCommand<MoveDirectionCommand> cacheMoveCommand = (FrameCommand<MoveDirectionCommand>) this.cacheMoveCommand;
                        if (!_actorRoot.handle.ActorControl.GetNoAbilityFlag(ObjAbilityType.ObjAbility_MoveRotate))
                        {
                            VInt3 inDirection = VInt3.right.RotateY(cacheMoveCommand.cmdData.Degree);
                            _actorRoot.handle.MovementComponent.SetRotate(inDirection, true);
                            Quaternion identity = Quaternion.identity;
                            identity = Quaternion.LookRotation((Vector3) inDirection);
                            _actorRoot.handle.rotation = identity;
                        }
                    }
                    this.cacheRotateExpire = true;
                }
            }
        }

        public void UseSkillCacheMove(PoolObjHandle<ActorRoot> _actorRoot, int _deltaTime, int _moveSpeed)
        {
            if ((_actorRoot != 0) && (((this.cacheMoveCommand != null) && !this.cacheMoveExpire) && (this.cacheMoveCommand.cmdType == 2)))
            {
                FrameCommand<MoveDirectionCommand> cacheMoveCommand = (FrameCommand<MoveDirectionCommand>) this.cacheMoveCommand;
                if (!_actorRoot.handle.ActorControl.GetNoAbilityFlag(ObjAbilityType.ObjAbility_Move))
                {
                    VInt3 delta = VInt3.right.RotateY(cacheMoveCommand.cmdData.Degree).NormalizeTo((_moveSpeed * _deltaTime) / 0x3e8);
                    VInt groundY = _actorRoot.handle.groundY;
                    delta = PathfindingUtility.Move(_actorRoot.handle, delta, out groundY, out _actorRoot.handle.hasReachedNavEdge);
                    if (_actorRoot.handle.MovementComponent.isFlying)
                    {
                        int y = _actorRoot.handle.location.y;
                        ActorRoot handle = _actorRoot.handle;
                        handle.location += delta;
                        VInt3 location = _actorRoot.handle.location;
                        location.y = y;
                        _actorRoot.handle.location = location;
                    }
                    else
                    {
                        ActorRoot local2 = _actorRoot.handle;
                        local2.location += delta;
                    }
                    _actorRoot.handle.groundY = groundY;
                }
            }
        }
    }
}

