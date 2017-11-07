namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class EffectPlayComponent : LogicComponent
    {
        private GameObject m_kingOfKillerEff;
        private GameObject m_skillGestureGuide;
        private const string s_dyingGoldEffectPath = "Prefab_Skill_Effects/Systems_Effects/GoldenCoin_UI_01";
        public static string s_heroHunHurtPath = "Prefab_Skill_Effects/tongyong_effects/tongyong_hurt/Hun_hurt_01";
        public static string s_heroSoulLevelUpEftPath = "Prefab_Skill_Effects/tongyong_effects/tongyong_hurt/shengji_tongongi_01";
        public static string s_heroSuckEftPath = "Prefab_Skill_Effects/tongyong_effects/tongyong_hurt/Hunqiu_01";
        private const string s_kingOfKiller = "Prefab_Skill_Effects/Systems_Effects/huangguan_buff_01";
        private const string s_skillGesture = "Prefab_Skill_Effects/tongyong_effects/Indicator/Arrow_3_move";
        private const string s_skillGesture3 = "Prefab_Skill_Effects/tongyong_effects/Indicator/Arrow_2_move";
        private const string s_skillGestureCancel = "Prefab_Skill_Effects/tongyong_effects/Indicator/Arrow_3_move_red";
        public static int s_suckSoulMSec = 0x3e8;
        private List<DuraEftPlayParam> soulSuckObjList = new List<DuraEftPlayParam>();

        private void ClearVariables()
        {
            this.m_skillGestureGuide = null;
            this.m_kingOfKillerEff = null;
            for (int i = 0; i < this.soulSuckObjList.Count; i++)
            {
                DuraEftPlayParam param = this.soulSuckObjList[i];
                Singleton<CGameObjectPool>.GetInstance().RecycleGameObject(param.EftObj);
            }
            this.soulSuckObjList.Clear();
        }

        public override void Deactive()
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int>("HeroSoulLevelChange", new Action<PoolObjHandle<ActorRoot>, int>(this, (IntPtr) this.OnHeroSoulLevelChange));
            this.ClearVariables();
            base.Deactive();
        }

        public void EndKingOfKillerEffect()
        {
            if (this.m_kingOfKillerEff != null)
            {
                Singleton<CGameObjectPool>.GetInstance().RecycleGameObject(this.m_kingOfKillerEff);
                this.m_kingOfKillerEff = null;
            }
        }

        public void EndSkillGestureEffect()
        {
            if (this.m_skillGestureGuide != null)
            {
                Singleton<CGameObjectPool>.GetInstance().RecycleGameObject(this.m_skillGestureGuide);
                this.m_skillGestureGuide = null;
            }
        }

        public override void Fight()
        {
            Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int>("HeroSoulLevelChange", new Action<PoolObjHandle<ActorRoot>, int>(this, (IntPtr) this.OnHeroSoulLevelChange));
        }

        public override void LateUpdate(int nDelta)
        {
        }

        private void OnHeroSoulLevelChange(PoolObjHandle<ActorRoot> actorObj, int level)
        {
            if ((((actorObj != 0) && (actorObj.handle == base.actor)) && (!actorObj.handle.ActorControl.IsDeadState && actorObj.handle.Visible)) && (actorObj.handle.InCamera && (level > 1)))
            {
                Vector3 pos = new Vector3(base.actor.gameObject.transform.position.x, base.actor.gameObject.transform.position.y + 0.24f, base.actor.gameObject.transform.position.z);
                Quaternion rot = Quaternion.Euler(-90f, 0f, 0f);
                bool isInit = false;
                GameObject go = MonoSingleton<SceneMgr>.GetInstance().GetPooledGameObjLOD(s_heroSoulLevelUpEftPath, true, SceneObjType.ActionRes, pos, rot, out isInit);
                if (go != null)
                {
                    if (isInit)
                    {
                        go.AddComponent<AutoPoolRecycle>().lifeTime = 5f;
                    }
                    Transform parent = (base.actor.ActorMesh == null) ? base.actor.gameObject.transform : base.actor.ActorMesh.transform;
                    string layerName = "Particles";
                    if ((parent != null) && (parent.gameObject.layer == LayerMask.NameToLayer("Hide")))
                    {
                        layerName = "Hide";
                    }
                    go.transform.SetParent(parent);
                    go.SetLayer(layerName);
                    Singleton<CSoundManager>.instance.PlayBattleSound("Level_Up", base.gameObject);
                }
            }
        }

        public override void OnUse()
        {
            base.OnUse();
            this.ClearVariables();
        }

        public void PlayDyingGoldEffect(PoolObjHandle<ActorRoot> inActor)
        {
            if (inActor != 0)
            {
                float num = inActor.handle.CharInfo.iBulletHeight * 0.001f;
                Vector3 pos = inActor.handle.gameObject.transform.localToWorldMatrix.MultiplyPoint(new Vector3(0f, num + 1f, 0f));
                Quaternion rot = Quaternion.Euler(-90f, 0f, 0f);
                GameObject obj2 = MonoSingleton<SceneMgr>.GetInstance().GetPooledGameObjLOD("Prefab_Skill_Effects/Systems_Effects/GoldenCoin_UI_01", true, SceneObjType.ActionRes, pos, rot);
                if ((obj2 != null) && (obj2.GetComponent<ParticleSystem>() != null))
                {
                    obj2.GetComponent<ParticleSystem>().Play(true);
                    if (obj2.GetComponent<ParticleLifeHelper>() == null)
                    {
                        ParticleLifeHelper helper = obj2.AddComponent<ParticleLifeHelper>();
                    }
                }
            }
        }

        public void PlayHunHurtEft()
        {
            if (!base.actor.ActorControl.IsDeadState)
            {
                float y = base.actor.CharInfo.iBulletHeight * 0.001f;
                Vector3 pos = base.actor.gameObject.transform.localToWorldMatrix.MultiplyPoint(new Vector3(0f, y, 0f));
                Quaternion rot = Quaternion.Euler(-90f, 0f, 0f);
                GameObject obj2 = MonoSingleton<SceneMgr>.GetInstance().GetPooledGameObjLOD(s_heroHunHurtPath, true, SceneObjType.ActionRes, pos, rot);
                if ((obj2 != null) && (obj2.GetComponent<ParticleSystem>() != null))
                {
                    obj2.GetComponent<ParticleSystem>().Play(true);
                    if (obj2.GetComponent<ParticleLifeHelper>() == null)
                    {
                        ParticleLifeHelper helper = obj2.AddComponent<ParticleLifeHelper>();
                    }
                }
            }
        }

        public void PlaySuckSoulEft(PoolObjHandle<ActorRoot> src)
        {
            if ((src != 0) && ((ActorHelper.IsCaptainActor(ref this.actorPtr) && base.actor.Visible) && base.actor.InCamera))
            {
                float z = src.handle.CharInfo.iBulletHeight * 0.001f;
                Vector3 pos = src.handle.gameObject.transform.localToWorldMatrix.MultiplyPoint(new Vector3(0f, 0f, z));
                Quaternion rot = Quaternion.Euler(-90f, 0f, 0f);
                GameObject obj2 = MonoSingleton<SceneMgr>.GetInstance().GetPooledGameObjLOD(s_heroSuckEftPath, true, SceneObjType.ActionRes, pos, rot);
                if (obj2 != null)
                {
                    DuraEftPlayParam item = new DuraEftPlayParam {
                        EftObj = obj2,
                        RemainMSec = s_suckSoulMSec
                    };
                    this.soulSuckObjList.Add(item);
                }
            }
        }

        public static void Preload(ref ActorPreloadTab preloadTab)
        {
            preloadTab.AddParticle(s_heroSoulLevelUpEftPath);
            preloadTab.AddParticle(s_heroSuckEftPath);
            preloadTab.AddParticle(s_heroHunHurtPath);
            preloadTab.AddParticle("Prefab_Skill_Effects/Systems_Effects/GoldenCoin_UI_01");
            preloadTab.AddParticle("Prefab_Skill_Effects/tongyong_effects/Indicator/Arrow_3_move");
            preloadTab.AddParticle("Prefab_Skill_Effects/tongyong_effects/Indicator/Arrow_2_move");
            preloadTab.AddParticle("Prefab_Skill_Effects/tongyong_effects/Indicator/Arrow_3_move_red");
            preloadTab.AddParticle("Prefab_Skill_Effects/Systems_Effects/huangguan_buff_01");
        }

        public void StartKingOfKillerEffect()
        {
            if (base.actorPtr != 0)
            {
                this.EndKingOfKillerEffect();
                Quaternion rot = Quaternion.LookRotation(Vector3.right);
                Vector3 position = base.actor.gameObject.transform.position;
                position.y += 3.9f;
                GameObject obj2 = MonoSingleton<SceneMgr>.GetInstance().GetPooledGameObjLOD("Prefab_Skill_Effects/Systems_Effects/huangguan_buff_01", true, SceneObjType.ActionRes, position, rot);
                if (obj2 != null)
                {
                    obj2.transform.SetParent(base.actor.gameObject.transform);
                    obj2.SetActive(true);
                }
                this.m_kingOfKillerEff = obj2;
            }
        }

        public void StartSkillGestureEffect()
        {
            this.StartSkillGestureEffectShared(SkillSlotType.SLOT_SKILL_2, "Prefab_Skill_Effects/tongyong_effects/Indicator/Arrow_3_move");
        }

        public void StartSkillGestureEffect3()
        {
            this.StartSkillGestureEffectShared(SkillSlotType.SLOT_SKILL_3, "Prefab_Skill_Effects/tongyong_effects/Indicator/Arrow_2_move");
        }

        public void StartSkillGestureEffectCancel()
        {
            this.StartSkillGestureEffectShared(SkillSlotType.SLOT_SKILL_2, "Prefab_Skill_Effects/tongyong_effects/Indicator/Arrow_3_move_red");
        }

        private void StartSkillGestureEffectShared(SkillSlotType inSlotType, string guidePath)
        {
            if (base.actorPtr != 0)
            {
                SkillSlot skillSlot = base.actor.SkillControl.GetSkillSlot(inSlotType);
                if (skillSlot != null)
                {
                    this.EndSkillGestureEffect();
                    Quaternion rot = Quaternion.LookRotation(Vector3.right);
                    Vector3 position = base.actor.gameObject.transform.position;
                    position.y += 0.3f;
                    GameObject obj2 = MonoSingleton<SceneMgr>.GetInstance().GetPooledGameObjLOD(guidePath, true, SceneObjType.ActionRes, position, rot);
                    if (obj2 != null)
                    {
                        obj2.transform.SetParent(base.actor.gameObject.transform);
                        obj2.SetActive(true);
                        skillSlot.skillIndicator.SetPrefabScaler(obj2, (int) skillSlot.SkillObj.cfgData.iGuideDistance);
                    }
                    this.m_skillGestureGuide = obj2;
                }
            }
        }

        public override void UpdateLogic(int delta)
        {
            if (this.soulSuckObjList.Count != 0)
            {
                int index = 0;
                while (index < this.soulSuckObjList.Count)
                {
                    DuraEftPlayParam eftParam = this.soulSuckObjList[index];
                    if (this.UpdateSuckSoulEftMove(ref eftParam, delta))
                    {
                        Singleton<CGameObjectPool>.GetInstance().RecycleGameObject(eftParam.EftObj);
                        this.soulSuckObjList.RemoveAt(index);
                        this.PlayHunHurtEft();
                    }
                    else
                    {
                        this.soulSuckObjList[index] = eftParam;
                        index++;
                    }
                }
            }
        }

        private bool UpdateSuckSoulEftMove(ref DuraEftPlayParam eftParam, int delta)
        {
            Vector3 position = base.actor.gameObject.transform.position;
            position.y += base.actor.CharInfo.iBulletHeight * 0.001f;
            if (eftParam.EftObj == null)
            {
                return true;
            }
            Vector3 from = eftParam.EftObj.transform.position;
            eftParam.EftObj.transform.position = Vector3.Slerp(from, position, ((float) delta) / ((float) eftParam.RemainMSec));
            eftParam.RemainMSec -= delta;
            return (eftParam.RemainMSec <= 0);
        }
    }
}

