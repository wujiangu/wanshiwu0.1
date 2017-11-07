namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using CSProtocol;
    using System;
    using UnityEngine;

    [EventCategory("MMGame/Skill")]
    public class SpawnObjectDuration : DurationEvent
    {
        private PoolObjHandle<ActorRoot> actorRoot;
        private ActorRootSlot actorSlot;
        public bool applyActionSpeedToAnimation = true;
        public bool applyActionSpeedToParticle = true;
        public bool bTargetPosition;
        public bool bUseSkin;
        public VInt3 direction = VInt3.forward;
        public bool enableLayer;
        public bool enableTag;
        public int layer;
        public bool modifyDirection;
        public bool modifyScaling;
        public bool modifyTranslation = true;
        [ObjectTemplate(new System.Type[] {  })]
        public int objectSpaceId = -1;
        [ObjectTemplate(new System.Type[] {  })]
        public int parentId = -1;
        private GameObject particleObj;
        [AssetReference(AssetRefType.Particle)]
        public string prefabName = string.Empty;
        public bool recreateExisting = true;
        public Vector3 scaling = Vector3.one;
        public int sightRadius;
        public bool superTranslation;
        public string tag = string.Empty;
        [ObjectTemplate(true)]
        public int targetId = -1;
        public VInt3 targetPosition = VInt3.zero;
        public VInt3 translation = VInt3.zero;

        public override BaseEvent Clone()
        {
            SpawnObjectDuration duration = ClassObjPool<SpawnObjectDuration>.Get();
            duration.CopyData(this);
            return duration;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            SpawnObjectDuration duration = src as SpawnObjectDuration;
            this.targetId = duration.targetId;
            this.parentId = duration.parentId;
            this.objectSpaceId = duration.objectSpaceId;
            this.prefabName = duration.prefabName;
            this.recreateExisting = duration.recreateExisting;
            this.modifyTranslation = duration.modifyTranslation;
            this.superTranslation = duration.superTranslation;
            this.translation = duration.translation;
            this.bTargetPosition = duration.bTargetPosition;
            this.targetPosition = duration.targetPosition;
            this.modifyDirection = duration.modifyDirection;
            this.direction = duration.direction;
            this.modifyScaling = duration.modifyScaling;
            this.scaling = duration.scaling;
            this.enableLayer = duration.enableLayer;
            this.layer = duration.layer;
            this.enableTag = duration.enableTag;
            this.tag = duration.tag;
            this.applyActionSpeedToAnimation = duration.applyActionSpeedToAnimation;
            this.applyActionSpeedToParticle = duration.applyActionSpeedToParticle;
            this.sightRadius = duration.sightRadius;
            this.bUseSkin = duration.bUseSkin;
        }

        private void CreateEye(COM_PLAYERCAMP _camp)
        {
            if ((this.actorRoot != 0) && (this.sightRadius > 0))
            {
                this.actorRoot.handle.TheActorMeta.ActorCamp = _camp;
                if (this.actorRoot.handle.HorizonMarker != null)
                {
                    this.actorRoot.handle.HorizonMarker.SightRadius = this.sightRadius;
                }
                this.actorRoot.handle.ObjID = Singleton<GameObjMgr>.GetInstance().NewActorID;
                this.actorRoot.handle.ObjLinker.Invincible = true;
                Singleton<GameObjMgr>.instance.AddActor(this.actorRoot);
                VCollisionSphere sphere = new VCollisionSphere();
                sphere.Born((ActorRoot) this.actorRoot);
                sphere.Pos = VInt3.zero;
                sphere.Radius = 500;
                sphere.dirty = true;
                sphere.ConditionalUpdateShape();
            }
        }

        public override void Enter(Action _action, Track _track)
        {
            string resourceName;
            if (this.bUseSkin)
            {
                resourceName = SkinResourceHelper.GetResourceName(_action, this.prefabName);
            }
            else
            {
                resourceName = this.prefabName;
            }
            VInt3 zero = VInt3.zero;
            VInt3 forward = VInt3.forward;
            SkillUseContext refParamObject = null;
            GameObject gameObject = _action.GetGameObject(this.parentId);
            PoolObjHandle<ActorRoot> actorHandle = _action.GetActorHandle(this.parentId);
            PoolObjHandle<ActorRoot> handle2 = _action.GetActorHandle(this.objectSpaceId);
            if (handle2 != 0)
            {
                ActorRoot handle = handle2.handle;
                if (this.superTranslation)
                {
                    VInt3 num3 = VInt3.zero;
                    _action.refParams.GetRefParam("_BulletPos", ref num3);
                    zero = IntMath.Transform(num3, handle.forward, handle.location);
                }
                else if (this.modifyTranslation)
                {
                    zero = IntMath.Transform(this.translation, handle.forward, handle.location);
                }
                if (this.modifyDirection)
                {
                    forward = handle2.handle.forward;
                }
            }
            else if (this.bTargetPosition)
            {
                zero = this.translation + this.targetPosition;
                if (this.modifyDirection)
                {
                    if (refParamObject == null)
                    {
                        refParamObject = _action.refParams.GetRefParamObject<SkillUseContext>("SkillContext");
                    }
                    if ((refParamObject != null) && (refParamObject.Originator != 0))
                    {
                        forward = refParamObject.Originator.handle.forward;
                    }
                }
            }
            else
            {
                if (this.modifyTranslation)
                {
                    zero = this.translation;
                }
                if ((this.modifyDirection && (this.direction.x != 0)) && (this.direction.y != 0))
                {
                    forward = this.direction;
                    forward.NormalizeTo(0x3e8);
                }
            }
            if (this.targetId >= 0)
            {
                _action.ExpandGameObject(this.targetId);
                GameObject obj3 = _action.GetGameObject(this.targetId);
                if (this.recreateExisting && (obj3 != null))
                {
                    if (this.applyActionSpeedToAnimation)
                    {
                        _action.RemoveTempObject(Action.PlaySpeedAffectedType.ePSAT_Anim, obj3);
                    }
                    if (this.applyActionSpeedToParticle)
                    {
                        _action.RemoveTempObject(Action.PlaySpeedAffectedType.ePSAT_Fx, obj3);
                    }
                    ActorHelper.DetachActorRoot(obj3);
                    ActionManager.DestroyGameObject(obj3);
                    _action.SetGameObject(this.targetId, null);
                }
                GameObject go = null;
                bool isInit = true;
                if (obj3 == null)
                {
                    go = MonoSingleton<SceneMgr>.GetInstance().Spawn("TempObject", SceneObjType.Bullet, zero, forward);
                    if (go != null)
                    {
                        go.transform.localScale = Vector3.one;
                        bool flag2 = true;
                        int particleLOD = GameSettings.ParticleLOD;
                        if (refParamObject == null)
                        {
                            refParamObject = _action.refParams.GetRefParamObject<SkillUseContext>("SkillContext");
                        }
                        if ((refParamObject != null) && (((refParamObject.Originator != 0) && (refParamObject.Originator.handle.TheActorMeta.PlayerId == Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer().PlayerId)) || ((refParamObject.TargetActor != 0) && (refParamObject.TargetActor.handle.TheActorMeta.PlayerId == Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer().PlayerId))))
                        {
                            flag2 = false;
                        }
                        if (!flag2 && (particleLOD > 1))
                        {
                            GameSettings.ParticleLOD = 1;
                        }
                        MonoSingleton<SceneMgr>.GetInstance().m_dynamicLOD = flag2;
                        this.particleObj = MonoSingleton<SceneMgr>.GetInstance().GetPooledGameObjLOD(resourceName, true, SceneObjType.ActionRes, go.transform.position, go.transform.rotation, out isInit);
                        MonoSingleton<SceneMgr>.GetInstance().m_dynamicLOD = false;
                        if (this.particleObj == null)
                        {
                            MonoSingleton<SceneMgr>.GetInstance().m_dynamicLOD = flag2;
                            this.particleObj = MonoSingleton<SceneMgr>.GetInstance().GetPooledGameObjLOD(this.prefabName, true, SceneObjType.ActionRes, go.transform.position, go.transform.rotation, out isInit);
                            MonoSingleton<SceneMgr>.GetInstance().m_dynamicLOD = false;
                        }
                        GameSettings.ParticleLOD = particleLOD;
                        if (this.particleObj != null)
                        {
                            this.particleObj.transform.SetParent(go.transform);
                            this.particleObj.transform.localPosition = Vector3.zero;
                            this.particleObj.transform.localRotation = Quaternion.identity;
                        }
                        if (this.sightRadius > 0)
                        {
                            this.actorRoot = ActorHelper.AttachActorRoot(go, ActorTypeDef.Actor_Type_EYE, false);
                        }
                        else
                        {
                            this.actorRoot = ActorHelper.AttachActorRoot(go, ActorTypeDef.Actor_Type_Bullet, false);
                        }
                        if (this.actorRoot.handle.ObjLinker != null)
                        {
                            this.actorRoot.handle.ObjLinker.ActorStart();
                        }
                        this.actorRoot.handle.location = zero;
                        this.actorRoot.handle.forward = forward;
                        _action.SetGameObject(this.targetId, go);
                        VCollisionShape.InitActorCollision((ActorRoot) this.actorRoot, this.particleObj);
                        if (this.actorRoot.handle.shape != null)
                        {
                            this.actorRoot.handle.shape.ConditionalUpdateShape();
                        }
                        if (refParamObject == null)
                        {
                            refParamObject = _action.refParams.GetRefParamObject<SkillUseContext>("SkillContext");
                        }
                        if (refParamObject != null)
                        {
                            refParamObject.EffectPos = this.actorRoot.handle.location;
                            if (refParamObject.Originator != 0)
                            {
                                COM_PLAYERCAMP actorCamp = refParamObject.Originator.handle.TheActorMeta.ActorCamp;
                                this.CreateEye(actorCamp);
                            }
                        }
                        if (this.applyActionSpeedToAnimation)
                        {
                            _action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_Anim, go);
                        }
                        if (this.applyActionSpeedToParticle)
                        {
                            _action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_Fx, go);
                        }
                        if (this.enableLayer || this.enableTag)
                        {
                            if (this.enableLayer)
                            {
                                go.layer = this.layer;
                            }
                            if (this.enableTag)
                            {
                                go.tag = this.tag;
                            }
                            Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>();
                            for (int i = 0; i < componentsInChildren.Length; i++)
                            {
                                if (this.enableLayer)
                                {
                                    componentsInChildren[i].gameObject.layer = this.layer;
                                }
                                if (this.enableTag)
                                {
                                    componentsInChildren[i].gameObject.tag = this.tag;
                                }
                            }
                        }
                        if (isInit)
                        {
                            ParticleHelper.Init(go, this.scaling);
                        }
                        PoolObjHandle<ActorRoot> newActor = _action.GetActorHandle(this.targetId);
                        this.SetParent(ref actorHandle, ref newActor, this.translation);
                        if (this.modifyScaling)
                        {
                            go.transform.localScale = this.scaling;
                        }
                    }
                }
            }
            else
            {
                GameObject obj5;
                if (this.modifyDirection)
                {
                    obj5 = MonoSingleton<SceneMgr>.GetInstance().InstantiateLOD(this.prefabName, true, SceneObjType.ActionRes, (Vector3) zero, Quaternion.LookRotation((Vector3) forward));
                }
                else
                {
                    obj5 = MonoSingleton<SceneMgr>.GetInstance().InstantiateLOD(this.prefabName, true, SceneObjType.ActionRes, (Vector3) zero);
                }
                if (obj5 != null)
                {
                    if (this.applyActionSpeedToAnimation)
                    {
                        _action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_Anim, obj5);
                    }
                    if (this.applyActionSpeedToParticle)
                    {
                        _action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_Fx, obj5);
                    }
                    if (this.enableLayer)
                    {
                        obj5.layer = this.layer;
                        Transform[] transformArray2 = obj5.GetComponentsInChildren<Transform>();
                        for (int j = 0; j < transformArray2.Length; j++)
                        {
                            transformArray2[j].gameObject.layer = this.layer;
                        }
                    }
                    if (this.enableTag)
                    {
                        obj5.tag = this.tag;
                        Transform[] transformArray3 = obj5.GetComponentsInChildren<Transform>();
                        for (int k = 0; k < transformArray3.Length; k++)
                        {
                            transformArray3[k].gameObject.tag = this.tag;
                        }
                    }
                    if ((obj5.GetComponent<ParticleSystem>() != null) && this.modifyScaling)
                    {
                        ParticleSystem[] systemArray = obj5.GetComponentsInChildren<ParticleSystem>();
                        for (int m = 0; m < systemArray.Length; m++)
                        {
                            ParticleSystem system1 = systemArray[m];
                            system1.startSize *= this.scaling.x;
                            ParticleSystem system2 = systemArray[m];
                            system2.startLifetime *= this.scaling.y;
                            ParticleSystem system3 = systemArray[m];
                            system3.startSpeed *= this.scaling.z;
                            Transform transform = systemArray[m].transform;
                            transform.localScale = (Vector3) (transform.localScale * this.scaling.x);
                        }
                    }
                    PoolObjHandle<ActorRoot> actorRoot = ActorHelper.GetActorRoot(obj5);
                    this.SetParent(ref actorHandle, ref actorRoot, this.translation);
                    if (this.modifyScaling)
                    {
                        obj5.transform.localScale = this.scaling;
                    }
                }
            }
        }

        public override void Leave(Action _action, Track _track)
        {
            if (this.particleObj != null)
            {
                this.particleObj.transform.parent = null;
                ActionManager.DestroyGameObject(this.particleObj);
            }
            GameObject gameObject = _action.GetGameObject(this.targetId);
            if ((this.targetId >= 0) && (gameObject != null))
            {
                if (this.applyActionSpeedToAnimation)
                {
                    _action.RemoveTempObject(Action.PlaySpeedAffectedType.ePSAT_Anim, gameObject);
                }
                if (this.applyActionSpeedToParticle)
                {
                    _action.RemoveTempObject(Action.PlaySpeedAffectedType.ePSAT_Fx, gameObject);
                }
                this.RemoveEye();
                ActorHelper.DetachActorRoot(gameObject);
                ActionManager.DestroyGameObjectFromAction(_action, gameObject);
            }
            if (this.actorSlot != null)
            {
                PoolObjHandle<ActorRoot> actorHandle = _action.GetActorHandle(this.parentId);
                if (actorHandle != 0)
                {
                    actorHandle.handle.RemoveActorRootSlot(this.actorSlot);
                }
            }
            this.actorSlot = null;
        }

        public override void OnUse()
        {
            base.OnUse();
            this.actorSlot = null;
            this.sightRadius = 0;
            this.particleObj = null;
            this.actorRoot.Release();
        }

        private void RemoveEye()
        {
            if ((this.actorRoot != 0) && (this.sightRadius > 0))
            {
                Singleton<GameObjMgr>.instance.DestroyActor(this.actorRoot.handle.ObjID);
            }
        }

        private void SetParent(ref PoolObjHandle<ActorRoot> parentActor, ref PoolObjHandle<ActorRoot> newActor, VInt3 trans)
        {
            if ((parentActor != 0) && (newActor != 0))
            {
                this.actorSlot = parentActor.handle.CreateActorRootSlot(newActor, trans);
            }
        }

        public override bool SupportEditMode()
        {
            return true;
        }
    }
}

