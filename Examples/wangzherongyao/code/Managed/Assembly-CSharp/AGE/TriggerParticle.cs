namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using System;
    using UnityEngine;

    [EventCategory("Animation")]
    public class TriggerParticle : DurationEvent
    {
        public bool applyActionSpeedToParticle = true;
        public bool bBullerPosDir;
        public bool bBulletDir;
        public bool bBulletPos;
        public bool bEnableOptCull = true;
        [SubObject]
        public string bindPointName = string.Empty;
        public Vector3 bindPosOffset = new Vector3(0f, 0f, 0f);
        public Quaternion bindRotOffset = new Quaternion(0f, 0f, 0f, 1f);
        public bool bOnlyFollowPos;
        public bool bUseSkin;
        public bool enableLayer;
        public bool enableTag;
        public int extend = 10;
        private Transform followTransform;
        public int layer;
        [ObjectTemplate(new System.Type[] {  })]
        public int objectSpaceId = -1;
        private Vector3 offsetPosition;
        protected GameObject particleObject;
        [AssetReference(AssetRefType.Particle)]
        public string resourceName = string.Empty;
        public Vector3 scaling = new Vector3(1f, 1f, 1f);
        public string tag = string.Empty;
        [ObjectTemplate(new System.Type[] {  })]
        public int targetId;

        public override BaseEvent Clone()
        {
            TriggerParticle particle = ClassObjPool<TriggerParticle>.Get();
            particle.CopyData(this);
            return particle;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            TriggerParticle particle = src as TriggerParticle;
            this.targetId = particle.targetId;
            this.objectSpaceId = particle.objectSpaceId;
            this.resourceName = particle.resourceName;
            this.bindPointName = particle.bindPointName;
            this.bindPosOffset = particle.bindPosOffset;
            this.bindRotOffset = particle.bindRotOffset;
            this.scaling = particle.scaling;
            this.bEnableOptCull = particle.bEnableOptCull;
            this.bBulletPos = particle.bBulletPos;
            this.bBulletDir = particle.bBulletDir;
            this.bBullerPosDir = particle.bBullerPosDir;
            this.enableLayer = particle.enableLayer;
            this.layer = particle.layer;
            this.enableTag = particle.enableTag;
            this.tag = particle.tag;
            this.applyActionSpeedToParticle = particle.applyActionSpeedToParticle;
            this.particleObject = particle.particleObject;
            this.extend = particle.extend;
            this.bOnlyFollowPos = particle.bOnlyFollowPos;
            this.bUseSkin = particle.bUseSkin;
        }

        public override void Enter(Action _action, Track _track)
        {
            SkillUseContext refParamObject = null;
            Vector3 bindPosOffset = this.bindPosOffset;
            Quaternion bindRotOffset = this.bindRotOffset;
            GameObject gameObject = _action.GetGameObject(this.targetId);
            GameObject obj3 = _action.GetGameObject(this.objectSpaceId);
            Transform transform = null;
            Transform transform2 = null;
            if (this.bindPointName.Length == 0)
            {
                if (gameObject != null)
                {
                    transform = gameObject.transform;
                    PoolObjHandle<ActorRoot> actorHandle = _action.GetActorHandle(this.targetId);
                    if ((actorHandle != 0) && (actorHandle.handle.ActorMesh != null))
                    {
                        this.followTransform = actorHandle.handle.ActorMesh.transform;
                    }
                    else
                    {
                        this.followTransform = transform;
                    }
                }
                else if (obj3 != null)
                {
                    transform2 = obj3.transform;
                }
            }
            else
            {
                GameObject obj4 = null;
                if (gameObject != null)
                {
                    obj4 = SubObject.FindSubObject(gameObject, this.bindPointName);
                    if (obj4 != null)
                    {
                        transform = obj4.transform;
                    }
                    else if (gameObject != null)
                    {
                        transform = gameObject.transform;
                    }
                }
                else if (obj3 != null)
                {
                    obj4 = SubObject.FindSubObject(obj3, this.bindPointName);
                    if (obj4 != null)
                    {
                        transform2 = obj4.transform;
                    }
                    else if (gameObject != null)
                    {
                        transform2 = obj3.transform;
                    }
                }
            }
            if ((!this.bEnableOptCull || (transform2 == null)) || (transform2.gameObject.layer != LayerMask.NameToLayer("Hide")))
            {
                string resourceName;
                if (this.bBulletPos)
                {
                    VInt3 zero = VInt3.zero;
                    _action.refParams.GetRefParam("_BulletPos", ref zero);
                    bindPosOffset = (Vector3) zero;
                    bindRotOffset = Quaternion.identity;
                    if (this.bBulletDir)
                    {
                        VInt3 num2 = VInt3.zero;
                        if (_action.refParams.GetRefParam("_BulletDir", ref num2))
                        {
                            bindRotOffset = Quaternion.LookRotation((Vector3) num2);
                        }
                    }
                }
                else if (transform != null)
                {
                    bindPosOffset = transform.localToWorldMatrix.MultiplyPoint(this.bindPosOffset);
                    bindRotOffset = transform.rotation * this.bindRotOffset;
                }
                else if (transform2 != null)
                {
                    if (obj3 != null)
                    {
                        PoolObjHandle<ActorRoot> handle2 = _action.GetActorHandle(this.objectSpaceId);
                        if (handle2 != 0)
                        {
                            bindPosOffset = (Vector3) IntMath.Transform((VInt3) this.bindPosOffset, handle2.handle.forward, (VInt3) obj3.transform.position);
                            bindRotOffset = Quaternion.LookRotation((Vector3) handle2.handle.forward) * this.bindRotOffset;
                        }
                    }
                    else
                    {
                        bindPosOffset = transform2.localToWorldMatrix.MultiplyPoint(this.bindPosOffset);
                        bindRotOffset = transform2.rotation * this.bindRotOffset;
                    }
                    if (this.bBulletDir)
                    {
                        VInt3 num3 = VInt3.zero;
                        if (_action.refParams.GetRefParam("_BulletDir", ref num3))
                        {
                            bindRotOffset = Quaternion.LookRotation((Vector3) num3) * this.bindRotOffset;
                        }
                    }
                    else if (this.bBullerPosDir)
                    {
                        if (refParamObject == null)
                        {
                            refParamObject = _action.refParams.GetRefParamObject<SkillUseContext>("SkillContext");
                        }
                        if (refParamObject != null)
                        {
                            PoolObjHandle<ActorRoot> originator = refParamObject.Originator;
                            if ((originator != 0) && (originator.handle.gameObject != null))
                            {
                                Vector3 forward = transform2.position - originator.handle.gameObject.transform.position;
                                bindRotOffset = Quaternion.LookRotation(forward) * this.bindRotOffset;
                            }
                        }
                    }
                }
                bool isInit = false;
                if (this.bUseSkin)
                {
                    resourceName = SkinResourceHelper.GetResourceName(_action, this.resourceName);
                }
                else
                {
                    resourceName = this.resourceName;
                }
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
                this.particleObject = MonoSingleton<SceneMgr>.GetInstance().GetPooledGameObjLOD(resourceName, true, SceneObjType.ActionRes, bindPosOffset, bindRotOffset, out isInit);
                MonoSingleton<SceneMgr>.GetInstance().m_dynamicLOD = false;
                if (this.particleObject == null)
                {
                    MonoSingleton<SceneMgr>.GetInstance().m_dynamicLOD = flag2;
                    this.particleObject = MonoSingleton<SceneMgr>.GetInstance().GetPooledGameObjLOD(this.resourceName, true, SceneObjType.ActionRes, bindPosOffset, bindRotOffset, out isInit);
                    MonoSingleton<SceneMgr>.GetInstance().m_dynamicLOD = false;
                    if (this.particleObject == null)
                    {
                        GameSettings.ParticleLOD = particleLOD;
                        return;
                    }
                }
                GameSettings.ParticleLOD = particleLOD;
                ParticleHelper.IncParticleActiveNumber();
                if (transform != null)
                {
                    if (!this.bOnlyFollowPos)
                    {
                        PoolObjHandle<ActorRoot> handle4 = (transform.gameObject != gameObject) ? ActorHelper.GetActorRoot(transform.gameObject) : _action.GetActorHandle(this.targetId);
                        if ((handle4 != 0) && (handle4.handle.ActorMesh != null))
                        {
                            this.particleObject.transform.parent = handle4.handle.ActorMesh.transform;
                        }
                        else
                        {
                            this.particleObject.transform.parent = transform;
                        }
                    }
                    else
                    {
                        this.offsetPosition = bindPosOffset - transform.position;
                    }
                }
                if (isInit)
                {
                    if (this.enableLayer || this.enableTag)
                    {
                        Transform[] componentsInChildren = this.particleObject.GetComponentsInChildren<Transform>();
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
                    if (this.particleObject.GetComponent<ParticleSystem>() != null)
                    {
                        ParticleSystem[] systemArray = this.particleObject.GetComponentsInChildren<ParticleSystem>();
                        for (int j = 0; j < systemArray.Length; j++)
                        {
                            ParticleSystem system1 = systemArray[j];
                            system1.startSize *= this.scaling.x;
                            ParticleSystem system2 = systemArray[j];
                            system2.startLifetime *= this.scaling.y;
                            ParticleSystem system3 = systemArray[j];
                            system3.startSpeed *= this.scaling.z;
                            Transform transform1 = systemArray[j].transform;
                            transform1.localScale = (Vector3) (transform1.localScale * this.scaling.x);
                        }
                    }
                }
                string layerName = "Particles";
                if ((transform != null) && (transform.gameObject.layer == LayerMask.NameToLayer("Hide")))
                {
                    layerName = "Hide";
                }
                this.particleObject.SetLayer(layerName);
                if (this.particleObject.GetComponent<ParticleSystem>() != null)
                {
                    this.particleObject.GetComponent<ParticleSystem>().Play(true);
                }
                if (this.applyActionSpeedToParticle && (this.particleObject != null))
                {
                    _action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_Fx, this.particleObject);
                }
            }
        }

        public override void Leave(Action _action, Track _track)
        {
            if (this.particleObject != null)
            {
                this.particleObject.transform.position = new Vector3(10000f, 10000f, 10000f);
                Singleton<CGameObjectPool>.GetInstance().RecycleGameObject(this.particleObject);
                ParticleHelper.DecParticleActiveNumber();
                if (this.applyActionSpeedToParticle)
                {
                    _action.RemoveTempObject(Action.PlaySpeedAffectedType.ePSAT_Fx, this.particleObject);
                }
            }
        }

        public override void OnUse()
        {
            base.OnUse();
            this.targetId = 0;
            this.objectSpaceId = -1;
            this.resourceName = string.Empty;
            this.bindPointName = string.Empty;
            this.bindPosOffset = new Vector3(0f, 0f, 0f);
            this.bindRotOffset = new Quaternion(0f, 0f, 0f, 1f);
            this.scaling = new Vector3(1f, 1f, 1f);
            this.bEnableOptCull = true;
            this.bBulletPos = false;
            this.bBulletDir = false;
            this.bBullerPosDir = false;
            this.enableLayer = false;
            this.layer = 0;
            this.enableTag = false;
            this.tag = string.Empty;
            this.applyActionSpeedToParticle = true;
            this.particleObject = null;
            this.extend = 10;
            this.offsetPosition = Vector3.zero;
            this.followTransform = null;
        }

        public override void Process(Action _action, Track _track, int _localTime)
        {
            if ((this.bOnlyFollowPos && (this.followTransform != null)) && (this.particleObject != null))
            {
                this.particleObject.transform.position = this.followTransform.position + this.offsetPosition;
            }
            base.Process(_action, _track, _localTime);
        }

        public override bool SupportEditMode()
        {
            return true;
        }
    }
}

