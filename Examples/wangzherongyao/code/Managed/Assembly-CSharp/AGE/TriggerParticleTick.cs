namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using System;
    using UnityEngine;

    [EventCategory("Animation")]
    public class TriggerParticleTick : TickEvent
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
        public bool bUseSkin;
        public bool enableLayer;
        public bool enableMaxLimit;
        public bool enableTag;
        public int extend = 10;
        public int layer;
        public float lifeTime = 5f;
        public int LimitType = -1;
        public int MaxLimit = 10;
        [ObjectTemplate(new System.Type[] {  })]
        public int objectSpaceId = -1;
        private GameObject particleObject;
        [AssetReference(AssetRefType.Particle)]
        public string resourceName = string.Empty;
        public Vector3 scaling = new Vector3(1f, 1f, 1f);
        public string tag = string.Empty;
        [ObjectTemplate(new System.Type[] {  })]
        public int targetId;

        public override BaseEvent Clone()
        {
            TriggerParticleTick tick = ClassObjPool<TriggerParticleTick>.Get();
            tick.CopyData(this);
            return tick;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            TriggerParticleTick tick = src as TriggerParticleTick;
            this.targetId = tick.targetId;
            this.objectSpaceId = tick.objectSpaceId;
            this.resourceName = tick.resourceName;
            this.lifeTime = tick.lifeTime;
            this.bindPointName = tick.bindPointName;
            this.bindPosOffset = tick.bindPosOffset;
            this.bindRotOffset = tick.bindRotOffset;
            this.scaling = tick.scaling;
            this.bEnableOptCull = tick.bEnableOptCull;
            this.bBulletPos = tick.bBulletPos;
            this.bBulletDir = tick.bBulletDir;
            this.bBullerPosDir = tick.bBullerPosDir;
            this.enableLayer = tick.enableLayer;
            this.layer = tick.layer;
            this.enableTag = tick.enableTag;
            this.tag = tick.tag;
            this.enableMaxLimit = tick.enableMaxLimit;
            this.MaxLimit = tick.MaxLimit;
            this.LimitType = tick.LimitType;
            this.applyActionSpeedToParticle = tick.applyActionSpeedToParticle;
            this.particleObject = tick.particleObject;
            this.extend = tick.extend;
            this.bUseSkin = tick.bUseSkin;
        }

        public override void OnUse()
        {
            base.OnUse();
            this.targetId = 0;
            this.objectSpaceId = -1;
            this.lifeTime = 5f;
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
            this.enableMaxLimit = false;
            this.MaxLimit = 10;
            this.LimitType = -1;
            this.applyActionSpeedToParticle = true;
            this.particleObject = null;
            this.extend = 10;
        }

        public override void Process(Action _action, Track _track)
        {
            if (!MonoSingleton<Reconnection>.GetInstance().isProcessingRelayRecover)
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
                    }
                    else if (obj3 != null)
                    {
                        transform2 = obj3.transform;
                    }
                }
                else
                {
                    Transform transform3 = null;
                    if (gameObject != null)
                    {
                        transform3 = SubObject.FindSubObject(gameObject, this.bindPointName).transform;
                        if (transform3 != null)
                        {
                            transform = transform3;
                        }
                        else if (gameObject != null)
                        {
                            transform = gameObject.transform;
                        }
                    }
                    else if (obj3 != null)
                    {
                        transform3 = SubObject.FindSubObject(obj3, this.bindPointName).transform;
                        if (transform3 != null)
                        {
                            transform2 = transform3;
                        }
                        else if (gameObject != null)
                        {
                            transform2 = obj3.transform;
                        }
                    }
                }
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
                        PoolObjHandle<ActorRoot> actorHandle = _action.GetActorHandle(this.objectSpaceId);
                        if (actorHandle != 0)
                        {
                            bindPosOffset = (Vector3) IntMath.Transform((VInt3) this.bindPosOffset, actorHandle.handle.forward, (VInt3) obj3.transform.position);
                            bindRotOffset = Quaternion.LookRotation((Vector3) actorHandle.handle.forward) * this.bindRotOffset;
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
                        refParamObject = _action.refParams.GetRefParamObject<SkillUseContext>("SkillContext");
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
                if (((!this.bEnableOptCull || (transform2 == null)) || (transform2.gameObject.layer != LayerMask.NameToLayer("Hide"))) && ((!this.bEnableOptCull || !MonoSingleton<GlobalConfig>.instance.bEnableParticleCullOptimize) || MonoSingleton<CameraSystem>.instance.CheckVisiblity(new Bounds(bindPosOffset, new Vector3((float) this.extend, (float) this.extend, (float) this.extend)))))
                {
                    string resourceName;
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
                    if (this.particleObject != null)
                    {
                        ParticleHelper.IncParticleActiveNumber();
                        if (transform != null)
                        {
                            PoolObjHandle<ActorRoot> handle3 = (transform.gameObject != gameObject) ? ActorHelper.GetActorRoot(transform.gameObject) : _action.GetActorHandle(this.targetId);
                            if ((handle3 != 0) && (handle3.handle.ActorMesh != null))
                            {
                                this.particleObject.transform.parent = handle3.handle.ActorMesh.transform;
                            }
                            else
                            {
                                this.particleObject.transform.parent = transform.parent;
                            }
                        }
                        string layerName = "Particles";
                        if ((transform != null) && (transform.gameObject.layer == LayerMask.NameToLayer("Hide")))
                        {
                            layerName = "Hide";
                        }
                        this.particleObject.SetLayer(layerName);
                        if (isInit)
                        {
                            ParticleHelper.Init(this.particleObject, this.scaling);
                        }
                        if (isInit)
                        {
                            AutoPoolRecycle recycle = this.particleObject.AddComponent<AutoPoolRecycle>();
                            recycle.m_needRecordNumber = true;
                            recycle.lifeTime = Mathf.Max(((float) _action.length) / 1000f, this.lifeTime);
                        }
                        if (this.applyActionSpeedToParticle && (this.particleObject != null))
                        {
                            _action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_Fx, this.particleObject);
                        }
                    }
                }
            }
        }

        public override bool SupportEditMode()
        {
            return true;
        }
    }
}

