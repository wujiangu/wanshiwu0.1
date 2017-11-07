namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Framework;
    using ResData;
    using System;
    using UnityEngine;

    public class SkillControlIndicator
    {
        private bool bControlMove = false;
        private bool bMoveFlag;
        private bool bRotateFlag;
        private bool bUseAdvanceSelect;
        private float deltaAngle;
        private Vector3 deltaDirection = Vector3.zero;
        private Vector3 deltaPosition = Vector3.zero;
        private int effectHideFrameNum = 0;
        private GameObject effectPrefab;
        private GameObject effectWarnPrefab;
        private GameObject fixedPrefab;
        private GameObject fixedWarnPrefab;
        private GameObject guidePrefab;
        private GameObject guideWarnPrefab;
        public Vector3 highLitColor = new Vector3(1f, 1f, 1f);
        private Vector3 movePosition = Vector3.zero;
        private float moveSpeed = 0.03f;
        public const float PrefabHeight = 0.3f;
        private int pressTime = 0;
        private Vector3 rootRosition = Vector3.zero;
        private float rotateAngle;
        private Vector3 rotateDirection = Vector3.zero;
        private float rotateSpeed = 0.5f;
        private SkillSlot skillSlot;
        private ActorRoot targetActor;
        private Vector3 useOffsetPosition = Vector3.zero;
        private Vector3 useSkillDirection = Vector3.zero;
        private Vector3 useSkillPosition = Vector3.zero;

        public SkillControlIndicator(SkillSlot _skillSlot)
        {
            this.skillSlot = _skillSlot;
            this.bUseAdvanceSelect = true;
            this.targetActor = null;
        }

        public void CreateIndicatePrefab(Skill _skillObj)
        {
            if (((this.skillSlot.Actor != 0) && ActorHelper.IsHostActor(ref this.skillSlot.Actor)) && ((_skillObj != null) && (_skillObj.cfgData != null)))
            {
                this.effectHideFrameNum = 0;
                GameObject content = Singleton<CResourceManager>.GetInstance().GetResource(_skillObj.GuidePrefabName, typeof(GameObject), enResourceType.BattleScene, false, false).m_content as GameObject;
                ActorRoot handle = this.skillSlot.Actor.handle;
                Quaternion rotation = handle.gameObject.transform.rotation;
                Vector3 position = handle.gameObject.transform.position;
                position.y += 0.3f;
                if (content != null)
                {
                    this.guidePrefab = (GameObject) UnityEngine.Object.Instantiate(content, position, rotation);
                    this.guidePrefab.transform.SetParent(handle.gameObject.transform);
                    this.guidePrefab.SetActive(false);
                }
                content = Singleton<CResourceManager>.GetInstance().GetResource(_skillObj.GuideWarnPrefabName, typeof(GameObject), enResourceType.BattleScene, false, false).m_content as GameObject;
                if (content != null)
                {
                    this.guideWarnPrefab = (GameObject) UnityEngine.Object.Instantiate(content, position, rotation);
                    this.guideWarnPrefab.transform.SetParent(handle.gameObject.transform);
                    this.guideWarnPrefab.SetActive(false);
                }
                content = Singleton<CResourceManager>.GetInstance().GetResource(_skillObj.EffectPrefabName, typeof(GameObject), enResourceType.BattleScene, false, false).m_content as GameObject;
                if (content != null)
                {
                    this.effectPrefab = (GameObject) UnityEngine.Object.Instantiate(content, position, rotation);
                    this.effectPrefab.SetActive(false);
                    MonoSingleton<SceneMgr>.GetInstance().AddToRoot(this.effectPrefab, SceneObjType.ActionRes);
                }
                content = Singleton<CResourceManager>.GetInstance().GetResource(_skillObj.EffectWarnPrefabName, typeof(GameObject), enResourceType.BattleScene, false, false).m_content as GameObject;
                if (content != null)
                {
                    this.effectWarnPrefab = (GameObject) UnityEngine.Object.Instantiate(content, position, rotation);
                    this.effectWarnPrefab.SetActive(false);
                    MonoSingleton<SceneMgr>.GetInstance().AddToRoot(this.effectWarnPrefab, SceneObjType.ActionRes);
                }
                content = Singleton<CResourceManager>.GetInstance().GetResource(_skillObj.FixedPrefabName, typeof(GameObject), enResourceType.BattleScene, false, false).m_content as GameObject;
                if (content != null)
                {
                    this.fixedPrefab = (GameObject) UnityEngine.Object.Instantiate(content, position, rotation);
                    this.fixedPrefab.SetActive(false);
                    this.fixedPrefab.transform.SetParent(handle.gameObject.transform);
                }
                content = Singleton<CResourceManager>.GetInstance().GetResource(_skillObj.FixedWarnPrefabName, typeof(GameObject), enResourceType.BattleScene, false, false).m_content as GameObject;
                if (content != null)
                {
                    this.fixedWarnPrefab = (GameObject) UnityEngine.Object.Instantiate(content, position, rotation);
                    this.fixedWarnPrefab.SetActive(false);
                    this.fixedWarnPrefab.transform.SetParent(handle.gameObject.transform);
                }
                int iGuideDistance = (int) _skillObj.cfgData.iGuideDistance;
                this.SetPrefabScaler(this.guidePrefab, iGuideDistance);
                this.SetPrefabScaler(this.guideWarnPrefab, iGuideDistance);
                if ((_skillObj.cfgData.dwRangeAppointType == 3) || (_skillObj.cfgData.dwRangeAppointType == 1))
                {
                    this.SetPrefabScaler(this.effectPrefab, iGuideDistance);
                    this.SetPrefabScaler(this.effectWarnPrefab, iGuideDistance);
                }
                int iFixedDistance = (int) _skillObj.cfgData.iFixedDistance;
                this.SetPrefabScaler(this.fixedPrefab, iFixedDistance);
                this.SetPrefabScaler(this.fixedWarnPrefab, iFixedDistance);
            }
        }

        private void ForceSetGuildPrefabShow(bool bShow)
        {
            if (this.guidePrefab != null)
            {
                this.guidePrefab.SetActive(bShow);
            }
            if ((this.effectPrefab != null) && !Singleton<GameInput>.GetInstance().IsSmartUse())
            {
                this.effectPrefab.SetActive(bShow);
            }
        }

        public bool GetUseAdvanceMode()
        {
            return this.bUseAdvanceSelect;
        }

        public Vector3 GetUseSkillDirection()
        {
            return this.useSkillDirection;
        }

        public Vector3 GetUseSkillPosition()
        {
            return this.useSkillPosition;
        }

        public ActorRoot GetUseSkillTargetDefaultAttackMode()
        {
            if (!this.bUseAdvanceSelect)
            {
                SkillSelectControl instance = Singleton<SkillSelectControl>.GetInstance();
                Skill skill = (this.skillSlot.NextSkillObj == null) ? this.skillSlot.SkillObj : this.skillSlot.NextSkillObj;
                this.targetActor = instance.SelectTarget((SkillTargetRule) skill.cfgData.dwSkillTargetRule, this.skillSlot);
            }
            return this.targetActor;
        }

        public ActorRoot GetUseSkillTargetLockAttackMode()
        {
            if (this.bUseAdvanceSelect)
            {
                return this.targetActor;
            }
            return null;
        }

        public void InitControlIndicator()
        {
            if (ActorHelper.IsHostActor(ref this.skillSlot.Actor))
            {
                float spd = 0f;
                float angularSpd = 0f;
                GameSettings.GetLunPanSensitivity(out spd, out angularSpd);
                this.SetIndicatorSpeed(spd, angularSpd);
            }
        }

        public bool IsAllowUseSkill()
        {
            Skill skill = (this.skillSlot.NextSkillObj == null) ? this.skillSlot.SkillObj : this.skillSlot.NextSkillObj;
            if (((skill.cfgData.dwRangeAppointType == 2) && !this.bControlMove) && (this.pressTime <= 0x3e8))
            {
                return false;
            }
            return true;
        }

        public void LateUpdate(int nDelta)
        {
            if (((this.skillSlot != null) && (this.skillSlot.SkillObj != null)) && (this.skillSlot.SkillObj.cfgData != null))
            {
                if ((this.effectHideFrameNum > 0) && (Time.frameCount > this.effectHideFrameNum))
                {
                    this.ForceSetGuildPrefabShow(false);
                    this.effectHideFrameNum = 0;
                }
                this.pressTime += nDelta;
                if (this.effectPrefab != null)
                {
                    if (this.bMoveFlag)
                    {
                        Vector3 vector = (Vector3) ((this.deltaDirection * this.moveSpeed) * nDelta);
                        this.deltaPosition += vector;
                        if (this.deltaPosition.magnitude >= this.movePosition.magnitude)
                        {
                            this.bMoveFlag = false;
                            this.useSkillPosition = this.skillSlot.Actor.handle.gameObject.transform.position + this.useOffsetPosition;
                            this.effectPrefab.transform.position = this.useSkillPosition;
                        }
                        else
                        {
                            this.useSkillPosition = this.effectPrefab.transform.position + vector;
                            this.useSkillPosition += this.skillSlot.Actor.handle.gameObject.transform.position - this.rootRosition;
                            this.effectPrefab.transform.position = this.useSkillPosition;
                            this.rootRosition = this.skillSlot.Actor.handle.gameObject.transform.position;
                        }
                    }
                    else
                    {
                        this.useSkillPosition += this.skillSlot.Actor.handle.gameObject.transform.position - this.rootRosition;
                        this.effectPrefab.transform.position = this.useSkillPosition;
                        this.rootRosition = this.skillSlot.Actor.handle.gameObject.transform.position;
                    }
                    if (this.bRotateFlag)
                    {
                        float y = this.rotateSpeed * nDelta;
                        this.deltaAngle += y;
                        if (this.deltaAngle >= this.rotateAngle)
                        {
                            this.bRotateFlag = false;
                            this.useSkillDirection = this.rotateDirection;
                            this.effectPrefab.transform.forward = this.useSkillDirection;
                        }
                        else
                        {
                            Vector3 forward = this.effectPrefab.transform.forward;
                            if (Vector3.Cross(this.useSkillDirection, this.rotateDirection).y < 0f)
                            {
                                forward = (Vector3) (Quaternion.Euler(0f, -y, 0f) * forward);
                            }
                            else
                            {
                                forward = (Vector3) (Quaternion.Euler(0f, y, 0f) * forward);
                            }
                            this.useSkillDirection = forward;
                            this.effectPrefab.transform.forward = this.useSkillDirection;
                        }
                    }
                    VInt groundY = 0;
                    if (PathfindingUtility.GetGroundY((VInt3) this.effectPrefab.transform.position, out groundY))
                    {
                        Vector3 position = this.effectPrefab.transform.position;
                        position.y = ((float) groundY) + 0.3f;
                        this.effectPrefab.transform.position = position;
                    }
                }
                if ((this.effectWarnPrefab != null) && (this.effectPrefab != null))
                {
                    this.effectWarnPrefab.transform.position = this.effectPrefab.transform.position;
                    this.effectWarnPrefab.transform.forward = this.effectPrefab.transform.forward;
                }
                this.SetUseSkillTarget();
            }
        }

        private void PlayCommonAttackTargetEffect(ActorRoot actorRoot)
        {
            if (actorRoot != null)
            {
                Singleton<SkillIndicateSystem>.GetInstance().PlayCommonAttackTargetEffect(actorRoot);
                if (actorRoot.MatHurtEffect != null)
                {
                    actorRoot.MatHurtEffect.PlayHighLitEffect(this.highLitColor);
                }
            }
        }

        public void SelectSkillTarget(Vector2 axis, bool isSkillCursorInCancelArea)
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
            Skill skill = (this.skillSlot.NextSkillObj == null) ? this.skillSlot.SkillObj : this.skillSlot.NextSkillObj;
            if ((curLvelContext != null) && curLvelContext.bCameraFlip)
            {
                axis = -axis;
            }
            Vector3 zero = Vector3.zero;
            Vector3 vector2 = Vector3.zero;
            if ((this.effectPrefab != null) && (skill != null))
            {
                if (skill.cfgData.dwRangeAppointType == 1)
                {
                    vector2.x = skill.cfgData.iGuideDistance * axis.x;
                    vector2.z = skill.cfgData.iGuideDistance * axis.y;
                    if (vector2.magnitude <= (((double) skill.cfgData.iGuideDistance) * 0.5))
                    {
                        this.bUseAdvanceSelect = false;
                    }
                    else
                    {
                        this.bUseAdvanceSelect = true;
                    }
                    zero.x = axis.x;
                    zero.z = axis.y;
                    this.useOffsetPosition = Vector3.zero;
                    this.bRotateFlag = true;
                    this.rotateAngle = Vector3.Angle(this.useSkillDirection, zero);
                    this.deltaAngle = 0f;
                    this.rotateDirection = zero;
                    this.rootRosition = this.skillSlot.Actor.handle.gameObject.transform.position;
                    this.useSkillPosition = this.skillSlot.Actor.handle.gameObject.transform.position;
                }
                else if (skill.cfgData.dwRangeAppointType == 2)
                {
                    vector2.x = (skill.cfgData.iGuideDistance / 1000f) * axis.x;
                    vector2.z = (skill.cfgData.iGuideDistance / 1000f) * axis.y;
                    this.useOffsetPosition = vector2;
                    this.movePosition = (this.skillSlot.Actor.handle.gameObject.transform.position + vector2) - this.effectPrefab.transform.position;
                    this.movePosition.y = 0f;
                    this.deltaDirection = this.movePosition;
                    this.deltaDirection.Normalize();
                    this.deltaPosition = Vector3.zero;
                    this.bMoveFlag = true;
                    this.rootRosition = this.skillSlot.Actor.handle.gameObject.transform.position;
                    this.bControlMove = true;
                }
                else if (skill.cfgData.dwRangeAppointType == 3)
                {
                    if ((axis == Vector2.zero) && !isSkillCursorInCancelArea)
                    {
                        return;
                    }
                    zero.x = axis.x;
                    zero.z = axis.y;
                    this.useOffsetPosition = Vector3.zero;
                    this.bRotateFlag = true;
                    this.rotateAngle = Vector3.Angle(this.useSkillDirection, zero);
                    this.deltaAngle = 0f;
                    this.rotateDirection = zero;
                    this.rootRosition = this.skillSlot.Actor.handle.gameObject.transform.position;
                    this.useSkillPosition = this.skillSlot.Actor.handle.gameObject.transform.position;
                }
            }
            if (isSkillCursorInCancelArea)
            {
                if ((this.effectWarnPrefab != null) && (this.effectPrefab != null))
                {
                    this.effectWarnPrefab.transform.position = this.effectPrefab.transform.position;
                    this.effectWarnPrefab.transform.forward = this.effectPrefab.transform.forward;
                }
                this.SetGuildWarnPrefabShow(true);
                this.SetGuildPrefabShow(false);
                this.SetFixedPrefabShow(false);
            }
            else
            {
                this.SetGuildWarnPrefabShow(false);
                this.SetFixedPrefabShow(true);
                if (!this.bUseAdvanceSelect)
                {
                    this.SetGuildPrefabShow(false);
                    this.SetEffectPrefabShow(false);
                }
                else
                {
                    this.SetGuildPrefabShow(true);
                    this.SetEffectPrefabShow(true);
                }
            }
        }

        public void SetEffectPrefabShow(bool bShow)
        {
            if ((this.effectPrefab != null) && !Singleton<GameInput>.GetInstance().IsSmartUse())
            {
                this.effectPrefab.SetActive(bShow);
            }
        }

        public void SetEffectWarnPrefabShow(bool bShow)
        {
            if ((this.effectWarnPrefab != null) && !Singleton<GameInput>.GetInstance().IsSmartUse())
            {
                this.effectWarnPrefab.SetActive(bShow);
            }
        }

        public void SetFixedPrefabShow(bool bShow)
        {
            if (this.fixedPrefab != null)
            {
                this.fixedPrefab.SetActive(bShow);
            }
        }

        public void SetFixedWarnPrefabShow(bool bShow)
        {
            if (this.fixedWarnPrefab != null)
            {
                this.fixedWarnPrefab.SetActive(bShow);
            }
        }

        public void SetGuildPrefabShow(bool bShow)
        {
            if (bShow)
            {
                this.effectHideFrameNum = 0;
                this.ForceSetGuildPrefabShow(bShow);
            }
            else
            {
                this.effectHideFrameNum = Time.frameCount;
            }
        }

        public void SetGuildWarnPrefabShow(bool bShow)
        {
            if (this.guideWarnPrefab != null)
            {
                this.guideWarnPrefab.SetActive(bShow);
            }
            if ((this.effectWarnPrefab != null) && !Singleton<GameInput>.GetInstance().IsSmartUse())
            {
                this.effectWarnPrefab.SetActive(bShow);
            }
            if ((this.fixedWarnPrefab != null) && !Singleton<GameInput>.GetInstance().IsSmartUse())
            {
                this.fixedWarnPrefab.SetActive(bShow);
            }
        }

        public void SetIndicatorSpeed(float _moveSpeed, float _rotateSpeed)
        {
            this.moveSpeed = _moveSpeed;
            this.rotateSpeed = _rotateSpeed;
        }

        public void SetPrefabScaler(GameObject _obj, int _distance)
        {
            if (_obj != null)
            {
                ParticleScaler[] componentsInChildren = _obj.GetComponentsInChildren<ParticleScaler>(true);
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].particleScale = ((float) _distance) / 10000f;
                    componentsInChildren[i].CheckAndApplyScale();
                }
            }
        }

        public void SetSkillUseDefaultPosition()
        {
            this.pressTime = 0;
            this.bControlMove = false;
            this.useOffsetPosition = Vector3.zero;
            this.useSkillPosition = this.skillSlot.Actor.handle.gameObject.transform.position;
            this.useSkillDirection = this.skillSlot.Actor.handle.gameObject.transform.forward;
            if (this.effectPrefab != null)
            {
                this.effectPrefab.transform.position = this.skillSlot.Actor.handle.gameObject.transform.position;
                this.effectPrefab.transform.Translate(0f, 0.3f, 0f);
                this.effectPrefab.transform.forward = this.skillSlot.Actor.handle.gameObject.transform.forward;
            }
        }

        public void SetSkillUsePosition(ActorRoot target)
        {
            Vector3 zero = Vector3.zero;
            Vector3 vector2 = Vector3.zero;
            Skill skill = (this.skillSlot.NextSkillObj == null) ? this.skillSlot.SkillObj : this.skillSlot.NextSkillObj;
            if (skill != null)
            {
                if (skill.cfgData.dwRangeAppointType == 2)
                {
                    this.useSkillPosition = target.gameObject.transform.position;
                    zero = target.gameObject.transform.position - this.skillSlot.Actor.handle.gameObject.transform.position;
                    this.useOffsetPosition = zero;
                    if (this.effectPrefab != null)
                    {
                        this.effectPrefab.transform.position = target.gameObject.transform.position;
                        this.effectPrefab.transform.Translate(0f, 0.3f, 0f);
                    }
                }
                else if (skill.cfgData.dwRangeAppointType == 3)
                {
                    zero = target.gameObject.transform.position - this.skillSlot.Actor.handle.gameObject.transform.position;
                    vector2 = zero;
                    vector2.y = 0f;
                    vector2.Normalize();
                    this.useSkillDirection = vector2;
                    if (this.effectPrefab != null)
                    {
                        this.effectPrefab.transform.forward = vector2;
                        this.effectPrefab.transform.position = this.skillSlot.Actor.handle.gameObject.transform.position;
                        this.effectPrefab.transform.Translate(0f, 0.3f, 0f);
                    }
                }
            }
        }

        public void SetUseAdvanceMode(bool b)
        {
            this.bUseAdvanceSelect = b;
        }

        public void SetUseSkillTarget()
        {
            ActorRoot actorRoot = null;
            Skill skill = (this.skillSlot.NextSkillObj == null) ? this.skillSlot.SkillObj : this.skillSlot.NextSkillObj;
            if ((skill != null) && (skill.cfgData.dwRangeAppointType == 1))
            {
                if ((!this.bUseAdvanceSelect && (this.guidePrefab != null)) && this.guidePrefab.activeSelf)
                {
                    actorRoot = Singleton<SkillSelectControl>.GetInstance().SelectTarget((SkillTargetRule) skill.cfgData.dwSkillTargetRule, this.skillSlot);
                }
                else if ((this.bUseAdvanceSelect && (this.effectPrefab != null)) && this.effectPrefab.activeSelf)
                {
                    int srchR = 0;
                    uint filter = 2;
                    if (this.skillSlot.SlotType != SkillSlotType.SLOT_SKILL_0)
                    {
                        srchR = (int) skill.cfgData.iMaxAttackDistance;
                    }
                    else
                    {
                        srchR = skill.cfgData.iMaxSearchDistance;
                    }
                    actorRoot = Singleton<SectorTargetSearcher>.GetInstance().GetEnemyTarget((ActorRoot) this.skillSlot.Actor, srchR, this.useSkillDirection, 50f, filter);
                    if (actorRoot == null)
                    {
                        uint num3 = 1;
                        actorRoot = Singleton<SectorTargetSearcher>.GetInstance().GetEnemyTarget((ActorRoot) this.skillSlot.Actor, srchR, this.useSkillDirection, 50f, num3);
                    }
                }
                if (actorRoot != this.targetActor)
                {
                    this.StopCommonAttackTargetEffect(this.targetActor);
                    this.PlayCommonAttackTargetEffect(actorRoot);
                    this.targetActor = actorRoot;
                }
            }
        }

        private void StopCommonAttackTargetEffect(ActorRoot actorRoot)
        {
            Singleton<SkillIndicateSystem>.GetInstance().StopCommonAttackTargetEffect();
            if ((actorRoot != null) && (actorRoot.MatHurtEffect != null))
            {
                actorRoot.MatHurtEffect.StopHighLitEffect();
            }
        }

        public void UnInitIndicatePrefab(bool bDestroy)
        {
            if ((this.skillSlot.Actor != 0) && ActorHelper.IsHostActor(ref this.skillSlot.Actor))
            {
                if (this.guidePrefab != null)
                {
                    this.guidePrefab.SetActive(false);
                    if (bDestroy)
                    {
                        UnityEngine.Object.Destroy(this.guidePrefab);
                    }
                }
                if (this.effectPrefab != null)
                {
                    this.effectPrefab.SetActive(false);
                    if (bDestroy)
                    {
                        UnityEngine.Object.Destroy(this.effectPrefab);
                    }
                }
                if (this.guideWarnPrefab != null)
                {
                    this.guideWarnPrefab.SetActive(false);
                    if (bDestroy)
                    {
                        UnityEngine.Object.Destroy(this.guideWarnPrefab);
                    }
                }
                if (this.effectWarnPrefab != null)
                {
                    this.effectWarnPrefab.SetActive(false);
                    if (bDestroy)
                    {
                        UnityEngine.Object.Destroy(this.effectWarnPrefab);
                    }
                }
                if (this.fixedPrefab != null)
                {
                    this.fixedPrefab.SetActive(false);
                    if (bDestroy)
                    {
                        UnityEngine.Object.Destroy(this.fixedPrefab);
                    }
                }
                if (this.fixedWarnPrefab != null)
                {
                    this.fixedWarnPrefab.SetActive(false);
                    if (bDestroy)
                    {
                        UnityEngine.Object.Destroy(this.fixedWarnPrefab);
                    }
                }
            }
        }
    }
}

