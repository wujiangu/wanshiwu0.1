namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.GameKernal;
    using Assets.Scripts.GameSystem;
    using Assets.Scripts.UI;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class HudComponent3D : LogicComponent
    {
        private Sprite3D _bloodDecImage;
        private int _curShield1;
        private bool _isDecreasingHp;
        private int _lastMaxBarValue;
        private GameObject _mountPoint;
        private GameObject _shieldGo;
        private float _shieldImagWidth;
        private GameObject[] _statusGo;
        private static string[] _statusResPath = new string[] { "Prefab_Skill_Effects/tongyong_effects/UI_fx/Yinshen_tongyong_01" };
        private const int BASE_ATTACK_INTERVAL = 0x4e20;
        public bool bBossHpBar;
        private Image bigTower_Img;
        private const float DecSpeed = 0.0086f;
        private const int DEPTH = 30;
        public const string HUD_BLOOD_BLACK_BAR_PREFAB = "UI3D/Battle/BlackBarBlack.prefab";
        public const string HUD_BLOOD_PREFAB = "UI3D/Battle/BloodHud.prefab";
        public const string HUD_HERO_PREFAB = "UI3D/Battle/Blood_Bar_Hero.prefab";
        public int hudHeight;
        public HudCompType HudType;
        private ulong LastBaseAttackTime;
        private Vector3 m_actorPos;
        private List<GameObject> m_blackBars = new List<GameObject>();
        private bool m_bloodDirty;
        private Sprite3D m_bloodImage;
        private Vector3 m_bloodPos;
        public Animation m_curAni;
        private GameObject m_effectRoot_big;
        private GameObject m_effectRoot_small;
        private Sprite3D m_energyImage;
        public ParticleLifeHelper m_exclamationMarkHelper;
        public Animation m_heroProficiencyAni3;
        public Animation m_heroProficiencyAni4;
        public Animation m_heroProficiencyAni5;
        private GameObject m_hud;
        private int m_hudFrameStampOnCreated;
        private bool m_hudVisible;
        private GameObject m_mapPointer_big;
        private GameObject m_mapPointer_small;
        private CUIContainerScript m_mapPointerContainer_big;
        private CUIContainerScript m_mapPointerContainer_small;
        private Sprite3D m_outOfControlBar;
        private GameObject m_outOfControlGo;
        private ListView<CoutofControlInfo> m_outofControlList;
        private bool m_pointerVisible;
        private Image m_signalImage;
        private Sprite3D m_soulImage;
        private TextMesh m_soulLevel;
        private CUIContainerScript m_textHudContainer;
        private GameObject m_textHudNode;
        private bool m_textVisible;
        private Sprite3D m_VoiceIconImage;
        public static Vector3 OFFSET_HEIGHT = new Vector3(0f, 2.65f, 0f);
        private const int OVERLAY_RENDER_QUEUE = 0xfa0;
        private RectTransform rtTransform;
        private Image smallTower_Img;
        private Text textCom;
        private int txt_hud_offset_x;
        private int txt_hud_offset_y;

        public override void Born(ActorRoot owner)
        {
            base.Born(owner);
            if (base.actor.CharInfo != null)
            {
                this.hudHeight = base.actor.CharInfo.hudHeight;
                this.HudType = base.actor.CharInfo.HudType;
            }
            this.m_hudFrameStampOnCreated = 0;
            this.m_hud = null;
            this.m_bloodImage = null;
            this.m_blackBars.Clear();
            this.m_soulImage = null;
            this.m_energyImage = null;
            this.m_soulLevel = null;
            this.m_outOfControlBar = null;
            this.m_outofControlList = null;
            this.m_outOfControlGo = null;
            this.m_mapPointerContainer_small = null;
            this.m_mapPointer_small = null;
            this.m_mapPointerContainer_big = null;
            this.m_mapPointer_big = null;
            this.bigTower_Img = null;
            this.smallTower_Img = null;
            this.m_signalImage = null;
            this.m_actorPos = Vector3.zero;
            this.m_bloodPos = Vector3.zero;
            this.m_textHudContainer = null;
            this.m_textHudNode = null;
            this.textCom = null;
            this.txt_hud_offset_x = 0;
            this.txt_hud_offset_y = 0;
            this.rtTransform = null;
            this.m_effectRoot_small = null;
            this.m_effectRoot_big = null;
        }

        public ParticleLifeHelper CreateSignalGameObject(string singlePath, Vector3 worldPosition)
        {
            GameObject obj2 = MonoSingleton<SceneMgr>.GetInstance().GetPooledGameObjLOD(singlePath, true, SceneObjType.Temp, worldPosition);
            obj2.CustomSetActive(true);
            ParticleLifeHelper component = null;
            component = obj2.GetComponent<ParticleLifeHelper>();
            if (component == null)
            {
                component = obj2.AddComponent<ParticleLifeHelper>();
                component.OnCreate();
                component.OnGet();
                CPooledGameObjectScript script = obj2.GetComponent<CPooledGameObjectScript>();
                if (script == null)
                {
                    return component;
                }
                IPooledMonoBehaviour[] behaviourArray = new IPooledMonoBehaviour[script.m_pooledMonoBehaviours.Length + 1];
                for (int i = 0; i < script.m_pooledMonoBehaviours.Length; i++)
                {
                    behaviourArray[i] = script.m_pooledMonoBehaviours[i];
                }
                behaviourArray[script.m_pooledMonoBehaviours.Length] = component;
                script.m_pooledMonoBehaviours = behaviourArray;
            }
            return component;
        }

        public override void Deactive()
        {
            if ((this.HudType != HudCompType.Type_Hide) && (this.m_hud != null))
            {
                this.m_hud.CustomSetActive(false);
            }
            this.setHudVisible(false, true);
            this.setPointerVisible(false, true);
            if ((this.m_mapPointerContainer_small != null) && (this.m_mapPointer_small != null))
            {
                this.m_mapPointerContainer_small.RecycleElement(this.m_mapPointer_small);
            }
            this.m_mapPointerContainer_small = null;
            this.m_mapPointer_small = null;
            CUIFormScript formScript = Singleton<CBattleSystem>.instance.m_FormScript;
            if (((this.m_mapPointer_big != null) && (formScript != null)) && (formScript.m_sgameGraphicRaycaster != null))
            {
                formScript.m_sgameGraphicRaycaster.RemoveGameObject(this.m_mapPointer_big);
            }
            if ((this.m_mapPointerContainer_big != null) && (this.m_mapPointer_big != null))
            {
                this.m_mapPointerContainer_big.RecycleElement(this.m_mapPointer_big);
            }
            this.m_mapPointerContainer_big = null;
            this.m_mapPointer_big = null;
            base.Deactive();
        }

        public override void Fight()
        {
            if (!MonoSingleton<GameFramework>.instance.EditorPreviewMode && !GameObjMgr.isPreSpawnActors)
            {
                if ((this.HudType != HudCompType.Type_Hide) && (this.m_hud != null))
                {
                    this.m_hud.CustomSetActive(true);
                }
                this.ResetHudUI();
                DragonIcon.Check_Dragon_Born_Evt(base.actor, true);
            }
        }

        public Image GetBigTower_Img()
        {
            return this.bigTower_Img;
        }

        private CUIContainerScript GetContainer(enBattleFormWidget enValue)
        {
            CUIFormScript formScript = Singleton<CBattleSystem>.instance.m_FormScript;
            if (formScript != null)
            {
                GameObject widget = formScript.GetWidget((int) enValue);
                if (widget != null)
                {
                    return widget.GetComponent<CUIContainerScript>();
                }
                Debug.LogWarning("[HudComponent3D][GetContainer][container == null]:" + enValue.ToString());
            }
            else
            {
                Debug.LogWarning("[HudComponent3D][GetContainer][battleFormScript == null]");
            }
            return null;
        }

        public GameObject GetCurrent_MapObj()
        {
            return this.m_mapPointer_small;
        }

        private Transform GetHudPanel(HudCompType hudType, bool isHostCamp)
        {
            string str = "Unknown_Panel";
            Transform transform = Moba_Camera.currentMobaCamera.transform.Find("Hud");
            if (transform == null)
            {
                GameObject obj2 = new GameObject("Hud");
                transform = obj2.transform;
                transform.parent = Moba_Camera.currentMobaCamera.transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
                transform.gameObject.AddComponent<Canvas3D>();
                string[] strArray = new string[] { enBattleFormWidget.Panel_EnemySoliderHud.ToString(), enBattleFormWidget.Panel_SelfSoliderHud.ToString(), enBattleFormWidget.Panel_EnemyHeroHud.ToString(), enBattleFormWidget.Panel_SelfHeroHud.ToString(), str };
                for (int i = 0; i < strArray.Length; i++)
                {
                    GameObject obj3 = new GameObject(strArray[i]);
                    Transform transform2 = obj3.transform;
                    transform2 = obj3.transform;
                    transform2.parent = transform;
                    transform2.localPosition = Vector3.zero;
                    transform2.localRotation = Quaternion.identity;
                    transform2.localScale = Vector3.one;
                }
            }
            string str2 = string.Empty;
            if (hudType == HudCompType.Type_Soldier)
            {
                str2 = !isHostCamp ? enBattleFormWidget.Panel_EnemySoliderHud.ToString() : enBattleFormWidget.Panel_SelfSoliderHud.ToString();
            }
            else if ((hudType == HudCompType.Type_Hero) || (hudType == HudCompType.Type_Organ))
            {
                str2 = !isHostCamp ? enBattleFormWidget.Panel_EnemyHeroHud.ToString() : enBattleFormWidget.Panel_SelfHeroHud.ToString();
            }
            if (string.IsNullOrEmpty(str2))
            {
                str2 = str;
            }
            Transform transform3 = transform.Find(str2);
            if (transform3 == null)
            {
            }
            return transform3;
        }

        private CUIContainerScript GetMapPointerContainer(bool bMiniMap)
        {
            CUIFormScript formScript = Singleton<CBattleSystem>.instance.m_FormScript;
            if (formScript != null)
            {
                enBattleFormWidget none = enBattleFormWidget.None;
                if (base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero)
                {
                    none = !bMiniMap ? enBattleFormWidget.Container_BigMapPointer_Hero : enBattleFormWidget.Container_MinimapPointer_Hero;
                }
                else if (base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Monster)
                {
                    switch (base.actor.ActorControl.GetActorSubSoliderType())
                    {
                        case 8:
                        case 9:
                        case 13:
                        case 7:
                            return null;
                    }
                    if (bMiniMap)
                    {
                        none = !base.actor.IsHostCamp() ? enBattleFormWidget.Container_MinimapPointer_Enemy : enBattleFormWidget.Container_MinimapPointer_Alies;
                    }
                    else
                    {
                        none = !base.actor.IsHostCamp() ? enBattleFormWidget.Container_BigMapPointer_Enemy : enBattleFormWidget.Container_BigMapPointer_Alies;
                    }
                }
                else if (base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Organ)
                {
                    if (((base.actor.TheStaticData.TheOrganOnlyInfo.OrganType != 1) && (base.actor.TheStaticData.TheOrganOnlyInfo.OrganType != 2)) && (base.actor.TheStaticData.TheOrganOnlyInfo.OrganType != 4))
                    {
                        return null;
                    }
                    none = !bMiniMap ? enBattleFormWidget.Container_BigMapPointer_Organ : enBattleFormWidget.Container_MinimapPointer_Organ;
                }
                GameObject widget = formScript.GetWidget((int) none);
                if (widget != null)
                {
                    return widget.GetComponent<CUIContainerScript>();
                }
            }
            return null;
        }

        public Vector3 GetSmallMapPointer_WorldPosition()
        {
            if (this.m_mapPointer_small != null)
            {
                return this.m_mapPointer_small.transform.position;
            }
            return new Vector3(0f, 0f, 0f);
        }

        public Image GetSmallTower_Img()
        {
            return this.smallTower_Img;
        }

        private CUIContainerScript GetTextHudContainer()
        {
            return this.GetContainer(enBattleFormWidget.Container_TextHud);
        }

        public void HideStatus(StatusHudType st)
        {
            DebugHelper.Assert(this._statusGo != null, "_statusGo ==null");
            int index = (int) st;
            GameObject obj2 = (this._statusGo == null) ? null : this._statusGo[index];
            if (obj2 != null)
            {
                obj2.CustomSetActive(false);
            }
        }

        private void HudInit_MapPointer()
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if ((this.m_mapPointerContainer_small == null) && (this.m_mapPointer_small == null))
            {
                this.m_mapPointerContainer_small = this.GetMapPointerContainer(true);
            }
            if (this.m_mapPointerContainer_small != null)
            {
                this.initHudIcon(this.m_mapPointerContainer_small, curLvelContext, true, out this.m_mapPointer_small);
            }
            if ((this.m_mapPointerContainer_big == null) && (this.m_mapPointer_big == null))
            {
                this.m_mapPointerContainer_big = this.GetMapPointerContainer(false);
            }
            if (this.m_mapPointerContainer_big != null)
            {
                this.initHudIcon(this.m_mapPointerContainer_big, curLvelContext, false, out this.m_mapPointer_big);
            }
            CUIFormScript formScript = Singleton<CBattleSystem>.instance.m_FormScript;
            if (formScript != null)
            {
                this.m_effectRoot_small = formScript.GetWidget(0x22);
                this.m_effectRoot_big = formScript.GetWidget(0x22);
            }
            if (curLvelContext != null)
            {
                this.setPointerVisible(curLvelContext.isPVPMode, true);
            }
        }

        public override void Init()
        {
            base.Init();
        }

        private void Init_TextHud(int hudSequence, bool bCenter = false)
        {
            this.m_textHudNode = this.m_textHudContainer.GetElement(hudSequence);
            if (this.m_textHudNode != null)
            {
                this.textCom = Utility.GetComponetInChild<Text>(this.m_textHudNode, "bg/Text");
                this.m_textHudNode.CustomSetActive(true);
            }
        }

        private void initHudIcon(CUIContainerScript containerScript, SLevelContext levelContext, bool bMiniMap, out GameObject mapPointer)
        {
            mapPointer = null;
            if ((containerScript != null) && (levelContext != null))
            {
                int element = containerScript.GetElement();
                if (element >= 0)
                {
                    RectTransform transform = null;
                    mapPointer = containerScript.GetElement(element);
                    if (mapPointer != null)
                    {
                        transform = mapPointer.transform as RectTransform;
                        if (transform != null)
                        {
                            if (base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero)
                            {
                                string str2;
                                Image component = mapPointer.transform.Find("hero").gameObject.GetComponent<Image>();
                                DebugHelper.Assert(GameDataMgr.heroDatabin.GetDataByKey((uint) base.actor.TheActorMeta.ConfigId) != null);
                                string prefabPath = KillNotifyUT.GetHero_Icon(base.actor, true);
                                component.SetSprite(prefabPath, Singleton<CBattleSystem>.instance.m_FormScript, true, false, false);
                                if (ActorHelper.IsHostCtrlActor(ref this.actorPtr))
                                {
                                    str2 = "Img_Map_GreenLoop";
                                }
                                else if (base.actor.IsHostCamp())
                                {
                                    str2 = "Img_Map_BlueLoop";
                                }
                                else
                                {
                                    str2 = "Img_Map_RedLoop";
                                }
                                mapPointer.GetComponent<Image>().SetSprite(string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_BustCircleSmall_Dir, str2), Singleton<CBattleSystem>.instance.m_FormScript, true, false, false);
                                transform.SetAsLastSibling();
                                if (!bMiniMap)
                                {
                                    MiniMapSysUT.SetMapElement_EventParam(mapPointer, base.actor.IsHostCamp(), MinimapSys.ElementType.Hero, base.actor.ObjID, (uint) base.actor.TheActorMeta.ConfigId);
                                }
                            }
                            else if (base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Organ)
                            {
                                if (base.actor.IsHostCamp())
                                {
                                    if ((base.actor.TheStaticData.TheOrganOnlyInfo.OrganType == 1) || (base.actor.TheStaticData.TheOrganOnlyInfo.OrganType == 4))
                                    {
                                        if (!bMiniMap)
                                        {
                                            MiniMapSysUT.SetMapElement_EventParam(mapPointer, true, MinimapSys.ElementType.Tower, 0, 0);
                                            Singleton<CBattleSystem>.instance.GetTowerHitMgr().Register(base.actor.ObjID, (RES_ORGAN_TYPE) base.actor.TheStaticData.TheOrganOnlyInfo.OrganType);
                                        }
                                        Image image3 = this.SetTower_Image(true, base.actor.TheStaticData.TheOrganOnlyInfo.OrganType, mapPointer, Singleton<CBattleSystem>.instance.m_FormScript);
                                        if (bMiniMap)
                                        {
                                            this.smallTower_Img = image3;
                                        }
                                        else
                                        {
                                            this.bigTower_Img = image3;
                                        }
                                    }
                                    else if (base.actor.TheStaticData.TheOrganOnlyInfo.OrganType == 2)
                                    {
                                        if (!bMiniMap)
                                        {
                                            MiniMapSysUT.SetMapElement_EventParam(mapPointer, true, MinimapSys.ElementType.Base, 0, 0);
                                            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorBeAttack, new RefAction<DefaultGameEventParam>(this.OnBaseAttacked));
                                            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorBeAttack, new RefAction<DefaultGameEventParam>(this.OnBaseAttacked));
                                            Singleton<CBattleSystem>.instance.GetTowerHitMgr().Register(base.actor.ObjID, (RES_ORGAN_TYPE) base.actor.TheStaticData.TheOrganOnlyInfo.OrganType);
                                        }
                                        Image image4 = this.SetTower_Image(true, base.actor.TheStaticData.TheOrganOnlyInfo.OrganType, mapPointer, Singleton<CBattleSystem>.instance.m_FormScript);
                                        if (bMiniMap)
                                        {
                                            this.smallTower_Img = image4;
                                        }
                                        else
                                        {
                                            this.bigTower_Img = image4;
                                        }
                                    }
                                }
                                else if ((base.actor.TheStaticData.TheOrganOnlyInfo.OrganType == 1) || (base.actor.TheStaticData.TheOrganOnlyInfo.OrganType == 4))
                                {
                                    if (!bMiniMap)
                                    {
                                        MiniMapSysUT.SetMapElement_EventParam(mapPointer, false, MinimapSys.ElementType.Tower, 0, 0);
                                    }
                                    Image image5 = this.SetTower_Image(false, base.actor.TheStaticData.TheOrganOnlyInfo.OrganType, mapPointer, Singleton<CBattleSystem>.instance.m_FormScript);
                                    if (bMiniMap)
                                    {
                                        this.smallTower_Img = image5;
                                    }
                                    else
                                    {
                                        this.bigTower_Img = image5;
                                    }
                                }
                                else if (base.actor.TheStaticData.TheOrganOnlyInfo.OrganType == 2)
                                {
                                    MiniMapSysUT.SetMapElement_EventParam(mapPointer, false, MinimapSys.ElementType.Base, 0, 0);
                                    Image image6 = this.SetTower_Image(false, base.actor.TheStaticData.TheOrganOnlyInfo.OrganType, mapPointer, Singleton<CBattleSystem>.instance.m_FormScript);
                                    if (bMiniMap)
                                    {
                                        this.smallTower_Img = image6;
                                    }
                                    else
                                    {
                                        this.bigTower_Img = image6;
                                    }
                                }
                                transform.SetAsFirstSibling();
                                this.m_actorPos = base.actor.gameObject.transform.position;
                                if ((levelContext != null) && levelContext.isPVPMode)
                                {
                                    this.UpdateUIMap(ref this.m_actorPos, (float) levelContext.mapWidth, (float) levelContext.mapHeight);
                                }
                            }
                            else if (base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Monster)
                            {
                                switch (base.actor.ActorControl.GetActorSubSoliderType())
                                {
                                    case 8:
                                    case 9:
                                    case 7:
                                    case 13:
                                        return;
                                }
                                transform.SetAsFirstSibling();
                                if ((levelContext != null) && levelContext.isPVPMode)
                                {
                                    this.m_actorPos = base.actor.gameObject.transform.position;
                                    this.UpdateUIMap(ref this.m_actorPos, (float) levelContext.mapWidth, (float) levelContext.mapHeight);
                                }
                            }
                            mapPointer.CustomSetActive(false);
                            this.m_pointerVisible = false;
                        }
                    }
                }
            }
        }

        private void InitHudUI()
        {
            this.m_hudFrameStampOnCreated = CUIManager.s_uiSystemRenderFrameCounter;
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if ((this.HudType != HudCompType.Type_Hide) && (this.m_hud == null))
            {
                bool flag = ((base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero) || (this.HudType == HudCompType.Type_Hero)) && Singleton<BattleLogic>.GetInstance().m_GameInfo.gameContext.IsSoulGrow();
                string prefabFullPath = !flag ? "UI3D/Battle/BloodHud.prefab" : "UI3D/Battle/Blood_Bar_Hero.prefab";
                bool flag2 = base.actor.IsHostCamp() && (base.actor.TheActorMeta.ConfigId == Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer().CaptainId);
                this.m_hud = Singleton<CGameObjectPool>.GetInstance().GetGameObject(prefabFullPath, enResourceType.BattleScene);
                DebugHelper.Assert(this.m_hud != null, "wtf?");
                if (this.m_hud == null)
                {
                    return;
                }
                if (flag)
                {
                    Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int, int, int>("HeroSoulExpChange", new Action<PoolObjHandle<ActorRoot>, int, int, int>(this, (IntPtr) this.onSoulExpChange));
                    Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int, int>("HeroEnergyChange", new Action<PoolObjHandle<ActorRoot>, int, int>(this, (IntPtr) this.onEnergyExpChange));
                    Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int>("HeroSoulLevelChange", new Action<PoolObjHandle<ActorRoot>, int>(this, (IntPtr) this.onSoulLvlChange));
                    Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<LimitMoveEventParam>(GameSkillEventDef.Event_LimitMove, new GameSkillEvent<LimitMoveEventParam>(this.OnPlayerLimitMove));
                    Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<LimitMoveEventParam>(GameSkillEventDef.Event_CancelLimitMove, new GameSkillEvent<LimitMoveEventParam>(this.OnPlayerCancelLimitMove));
                    this.m_bloodImage = this.m_hud.transform.Find("BloodFore").GetComponent<Sprite3D>();
                    DebugHelper.Assert(this.m_bloodImage != null);
                    if (this.m_bloodImage != null)
                    {
                        if (flag2)
                        {
                            this.m_bloodImage.spriteName = "Battle_greenHp";
                        }
                        else if (base.actor.IsHostCamp())
                        {
                            this.m_bloodImage.spriteName = "Battle_blueHp";
                        }
                        else
                        {
                            this.m_bloodImage.spriteName = "Battle_redHp";
                        }
                        this._shieldGo = this.m_bloodImage.gameObject.transform.FindChild("Shield").gameObject;
                        this._shieldImagWidth = this.m_bloodImage.width;
                        this._shieldGo.CustomSetActive(false);
                        this._bloodDecImage = this.m_hud.transform.FindChild("BloodBack").gameObject.GetComponent<Sprite3D>();
                        this._curShield1 = 0;
                        if (base.actor.ValueComponent.actorHpTotal > 0)
                        {
                            this.m_bloodImage.fillAmount = ((float) base.actor.ValueComponent.actorHp) / ((float) base.actor.ValueComponent.actorHpTotal);
                        }
                        else
                        {
                            this.m_bloodImage.fillAmount = 0f;
                        }
                        int actorHpTotal = base.actor.ValueComponent.actorHpTotal;
                        if (this.IsPlayerCopy())
                        {
                            MonsterWrapper actorControl = base.actor.ActorControl as MonsterWrapper;
                            if ((actorControl != null) && (actorControl.hostActor != 0))
                            {
                                actorHpTotal = actorControl.hostActor.handle.ValueComponent.actorHpTotal;
                            }
                        }
                        this.SetBlackBar(actorHpTotal);
                        this._bloodDecImage.fillAmount = this.m_bloodImage.fillAmount;
                    }
                    this.m_soulImage = this.m_hud.transform.Find("SoulFore").GetComponent<Sprite3D>();
                    DebugHelper.Assert(this.m_soulImage != null);
                    if (this.m_soulImage != null)
                    {
                        this.m_soulImage.fillAmount = 0f;
                    }
                    this.m_soulLevel = this.m_hud.transform.Find("SoulLevel").GetComponent<TextMesh>();
                    DebugHelper.Assert(this.m_soulLevel != null);
                    if (this.m_soulLevel != null)
                    {
                        this.m_soulLevel.text = "0";
                        this.m_soulLevel.gameObject.GetComponent<MeshRenderer>().sharedMaterial.renderQueue = 0x1194;
                    }
                    this.m_energyImage = this.m_hud.transform.Find("EnergyFore").GetComponent<Sprite3D>();
                    DebugHelper.Assert(this.m_energyImage != null);
                    if (base.actor.ValueComponent.IsEnergyType(ENERGY_TYPE.Magic) && (this.m_energyImage != null))
                    {
                        if (base.actor.ValueComponent.actorEpTotal > 0)
                        {
                            this.m_energyImage.fillAmount = ((float) base.actor.ValueComponent.actorEp) / ((float) base.actor.ValueComponent.actorEpTotal);
                        }
                        else
                        {
                            this.m_energyImage.fillAmount = 0f;
                        }
                    }
                    else
                    {
                        this.m_hud.transform.Find("EnergyFore").gameObject.CustomSetActive(false);
                    }
                    this.m_outOfControlGo = this.m_hud.transform.Find("OutOfControl").gameObject;
                    this.m_outOfControlBar = this.m_outOfControlGo.transform.Find("OutOfControlFore").GetComponent<Sprite3D>();
                    DebugHelper.Assert(this.m_outOfControlBar != null);
                    if (this.m_outOfControlBar != null)
                    {
                        this.m_outOfControlGo.CustomSetActive(false);
                        this.m_outOfControlBar.fillAmount = 0f;
                    }
                    this.m_outofControlList = new ListView<CoutofControlInfo>();
                    this.m_heroProficiencyAni3 = this.m_hud.transform.Find("Hero_Icon3_Ani").gameObject.GetComponent<Animation>();
                    this.m_heroProficiencyAni3.gameObject.CustomSetActive(false);
                    this.m_heroProficiencyAni4 = this.m_hud.transform.Find("Hero_Icon4_Ani").gameObject.GetComponent<Animation>();
                    this.m_heroProficiencyAni4.gameObject.CustomSetActive(false);
                    this.m_heroProficiencyAni5 = this.m_hud.transform.Find("Hero_Icon5_Ani").gameObject.GetComponent<Animation>();
                    this.m_heroProficiencyAni5.gameObject.CustomSetActive(false);
                    bool flag3 = (curLvelContext != null) && curLvelContext.isPVPLevel;
                    TextMesh component = this.m_hud.transform.Find("PlayerName").GetComponent<TextMesh>();
                    Player ownerPlayer = ActorHelper.GetOwnerPlayer(ref this.actorPtr);
                    if (this.IsPlayerCopy() && !base.actor.IsHostCamp())
                    {
                        MonsterWrapper wrapper2 = base.actor.ActorControl as MonsterWrapper;
                        if ((wrapper2 != null) && (wrapper2.hostActor != 0))
                        {
                            ownerPlayer = ActorHelper.GetOwnerPlayer(ref wrapper2.hostActor);
                        }
                    }
                    DebugHelper.Assert(component != null);
                    if (component != null)
                    {
                        if (flag3 && (ownerPlayer != null))
                        {
                            component.text = ownerPlayer.Name;
                            component.gameObject.CustomSetActive(true);
                        }
                        else
                        {
                            component.gameObject.CustomSetActive(false);
                        }
                        component.gameObject.GetComponent<MeshRenderer>().sharedMaterial.renderQueue = 0x1194;
                    }
                }
                else
                {
                    ActorTypeDef actorType = base.actor.TheActorMeta.ActorType;
                    if (this.HudType == HudCompType.Type_Boss)
                    {
                        actorType = ActorTypeDef.Actor_Type_Hero;
                    }
                    Sprite3D sprited = this.m_hud.transform.Find("BloodHud").GetComponent<Sprite3D>();
                    sprited.spriteName = Enum.GetName(typeof(SpriteNameEnum), ActorTypeDef.Actor_Type_EYE * actorType);
                    sprited.SetNativeSize(Moba_Camera.currentMobaCamera, 30f);
                    this.m_bloodImage = this.m_hud.transform.Find("Fore").GetComponent<Sprite3D>();
                    if (flag2)
                    {
                        this.m_bloodImage.spriteName = "bl_hero_self";
                    }
                    else
                    {
                        this.m_bloodImage.spriteName = Enum.GetName(typeof(SpriteNameEnum), (ActorTypeDef.Actor_Type_EYE * actorType) + (!base.actor.IsHostCamp() ? ActorTypeDef.Actor_Type_Organ : ActorTypeDef.Actor_Type_Monster));
                    }
                    this.m_bloodImage.SetNativeSize(Moba_Camera.currentMobaCamera, 30f);
                    if (base.actor.ValueComponent.actorHpTotal > 0)
                    {
                        this.m_bloodImage.fillAmount = ((float) base.actor.ValueComponent.actorHp) / ((float) base.actor.ValueComponent.actorHpTotal);
                    }
                    else
                    {
                        this.m_bloodImage.fillAmount = 0f;
                    }
                    bool flag4 = base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Organ;
                    Sprite3D componetInChild = Utility.GetComponetInChild<Sprite3D>(this.m_hud, "Icon");
                    if (flag4)
                    {
                        componetInChild.spriteName = !base.actor.IsHostCamp() ? SpriteNameEnum.bl_icon_organ_enemy.ToString() : SpriteNameEnum.bl_icon_organ_self.ToString();
                        componetInChild.SetNativeSize(Moba_Camera.currentMobaCamera, 30f);
                        componetInChild.transform.localPosition = new Vector3((this.m_bloodImage.transform.localPosition.x - (0.5f * this.m_bloodImage.width)) - (0.25f * componetInChild.width), componetInChild.transform.localPosition.y, componetInChild.transform.localPosition.z);
                        componetInChild.gameObject.CustomSetActive(true);
                    }
                    else
                    {
                        componetInChild.gameObject.CustomSetActive(false);
                    }
                }
                Transform transform = this.m_hud.transform.Find("VoiceIcon");
                if (transform != null)
                {
                    this.m_VoiceIconImage = transform.GetComponent<Sprite3D>();
                    this.m_VoiceIconImage.gameObject.CustomSetActive(false);
                }
                Transform hudPanel = this.GetHudPanel(this.HudType, base.actor.IsHostCamp());
                if (hudPanel != null)
                {
                    this.m_hud.transform.SetParent(hudPanel, true);
                    this.m_hud.transform.localScale = Vector3.one;
                    this.m_hud.transform.localRotation = Quaternion.identity;
                }
                this.setHudVisible(false, true);
                if (this.m_bloodDirty)
                {
                    this.m_bloodDirty = false;
                    bool bActive = (this.m_hudVisible && base.actor.Visible) && base.actor.InCamera;
                    if (this.m_hud != null)
                    {
                        this.m_hud.CustomSetActive(bActive);
                    }
                }
            }
            if ((base.actor.TheActorMeta.ActorType != ActorTypeDef.Actor_Type_Organ) || base.actor.TheStaticData.TheOrganOnlyInfo.ShowInMinimap)
            {
                this.HudInit_MapPointer();
            }
        }

        private void InitStatus()
        {
            this._statusGo = new GameObject[1];
            if (this._mountPoint == null)
            {
                this._mountPoint = new GameObject("MountPoint");
                this._mountPoint.transform.SetParent(base.actor.gameObject.transform);
                this._mountPoint.transform.localPosition = Vector3.zero;
                this._mountPoint.transform.localScale = Vector3.one;
                this._mountPoint.transform.localRotation = Quaternion.identity;
            }
        }

        private bool IsNeedUpdateUI()
        {
            return ((CUIManager.s_uiSystemRenderFrameCounter - this.m_hudFrameStampOnCreated) <= 2);
        }

        private bool IsPlayerCopy()
        {
            return ((base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Monster) && (this.HudType == HudCompType.Type_Hero));
        }

        public override void LateUpdate(int delta)
        {
            Vector3 position = base.actor.gameObject.transform.position;
            bool flag = this.m_actorPos != position;
            this.m_actorPos = position;
            if (base.actor.Visible && (this.IsNeedUpdateUI() || flag))
            {
                this.UpdateUIMap(ref this.m_actorPos);
            }
            if (this.m_bloodDirty)
            {
                this.m_bloodDirty = false;
                bool bActive = (this.m_hudVisible && base.actor.Visible) && base.actor.InCamera;
                if (this.m_hud != null)
                {
                    this.m_hud.CustomSetActive(bActive);
                }
                if ((this.m_signalImage != null) && (bActive != this.m_signalImage.gameObject.activeSelf))
                {
                    this.m_signalImage.gameObject.CustomSetActive(bActive);
                }
            }
            if (((this.HudType != HudCompType.Type_Hide) && base.actor.Visible) && base.actor.InCamera)
            {
                if ((this.m_hud != null) && this.m_hud.activeSelf)
                {
                    position.y += this.hudHeight * 0.001f;
                    Vector3 bloodPosition = Moba_Camera.currentMobaCamera.WorldToScreenPoint(position);
                    bool flag3 = this.m_bloodPos != bloodPosition;
                    this.m_bloodPos = bloodPosition;
                    if (this.IsNeedUpdateUI() || flag3)
                    {
                        this.UpdateUIHud(ref bloodPosition);
                    }
                }
                if (this.m_exclamationMarkHelper != null)
                {
                    this.m_exclamationMarkHelper.gameObject.transform.position = new Vector3(this.m_actorPos.x, this.m_actorPos.y + this.m_exclamationMarkHelper.m_yOffset, this.m_actorPos.z);
                }
                if (this.m_outOfControlBar != null)
                {
                    for (int i = 0; i < this.m_outofControlList.Count; i++)
                    {
                        CoutofControlInfo local1 = this.m_outofControlList[i];
                        local1.leftTime -= (int) (Time.deltaTime * 1000f);
                        if (this.m_outofControlList[i].leftTime <= 0)
                        {
                            this.m_outofControlList.RemoveAt(i);
                            i--;
                        }
                    }
                    this.SetOutOfControlBar();
                }
                if ((this.m_curAni != null) && !this.m_curAni.isPlaying)
                {
                    this.m_curAni.gameObject.CustomSetActive(false);
                }
                if (this._isDecreasingHp && ((this._bloodDecImage != null) && (this._bloodDecImage.fillAmount >= this.m_bloodImage.fillAmount)))
                {
                    this._bloodDecImage.fillAmount -= 0.0086f;
                    if (this._bloodDecImage.fillAmount <= this.m_bloodImage.fillAmount)
                    {
                        this._bloodDecImage.fillAmount = this.m_bloodImage.fillAmount;
                        this._isDecreasingHp = false;
                    }
                }
            }
        }

        public void OnActorDead()
        {
            this.setHudVisible(false, true);
            this.setTextVisible(false, true);
            this.setPointerVisible(false, true);
            DragonIcon.Check_Dragon_Born_Evt(base.actor, false);
            TowerHitMgr towerHitMgr = Singleton<CBattleSystem>.instance.GetTowerHitMgr();
            if ((base.actor != null) && (towerHitMgr != null))
            {
                towerHitMgr.Remove(base.actor.ObjID);
            }
        }

        public void OnActorRevive()
        {
            this.setPointerVisible(true, true);
        }

        private void OnBaseAttacked(ref DefaultGameEventParam evtParam)
        {
            if ((evtParam.src != 0) && (evtParam.src == base.actorPtr))
            {
                if (base.actor.IsHostCamp())
                {
                    if ((Singleton<FrameSynchr>.GetInstance().LogicFrameTick - this.LastBaseAttackTime) > 0x4e20L)
                    {
                        GameObject obj2 = this.m_mapPointer_small;
                        if (obj2 != null)
                        {
                            Animator component = obj2.GetComponent<Animator>();
                            if (component != null)
                            {
                                component.enabled = true;
                                component.Play("BaseTip", 0, 0f);
                            }
                        }
                        Singleton<CSoundManager>.GetInstance().PlayBattleSound("UI_retreat", null);
                        this.LastBaseAttackTime = Singleton<FrameSynchr>.GetInstance().LogicFrameTick;
                    }
                    TowerHitMgr towerHitMgr = Singleton<CBattleSystem>.instance.GetTowerHitMgr();
                    if (towerHitMgr != null)
                    {
                        towerHitMgr.TryActive(base.actor.ObjID, this.GetCurrent_MapObj());
                    }
                }
                if ((this.bigTower_Img != null) && (this.smallTower_Img != null))
                {
                    float single = evtParam.src.handle.ValueComponent.GetHpRate().single;
                    this.bigTower_Img.fillAmount = single;
                    this.smallTower_Img.fillAmount = single;
                }
            }
        }

        private void onEnergyExpChange(PoolObjHandle<ActorRoot> act, int curVal, int maxVal)
        {
            if (((this.m_energyImage != null) && (act.handle == base.actor)) && (maxVal > 0))
            {
                this.m_energyImage.fillAmount = ((float) curVal) / ((float) maxVal);
            }
        }

        private void OnPlayerCancelLimitMove(ref LimitMoveEventParam _param)
        {
            if ((_param.src != 0) && (_param.src.handle == base.actor))
            {
                for (int i = 0; i < this.m_outofControlList.Count; i++)
                {
                    if (this.m_outofControlList[i].combId == _param.combineID)
                    {
                        this.m_outofControlList.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private void OnPlayerLimitMove(ref LimitMoveEventParam _param)
        {
            if ((_param.src != 0) && (_param.src.handle == base.actor))
            {
                DebugHelper.Assert(_param.totalTime > 0, "被控时间是0，还控个毛啊, combineid" + _param.combineID);
                if (_param.totalTime > 0)
                {
                    CoutofControlInfo item = new CoutofControlInfo(_param.combineID, _param.totalTime, _param.totalTime);
                    this.m_outofControlList.Add(item);
                }
            }
        }

        private void OnSinglePlayEnd(GameObject go)
        {
            if ((this.m_exclamationMarkHelper != null) && (this.m_exclamationMarkHelper.gameObject == go))
            {
                this.m_exclamationMarkHelper = null;
            }
        }

        private void onSoulExpChange(PoolObjHandle<ActorRoot> act, int changeValue, int curVal, int maxVal)
        {
            if ((this.m_soulImage != null) && (act.handle == base.actor))
            {
                this.m_soulImage.fillAmount = ((float) curVal) / ((float) maxVal);
            }
            else if (this.IsPlayerCopy())
            {
                MonsterWrapper actorControl = base.actor.ActorControl as MonsterWrapper;
                if (((actorControl != null) && (actorControl.hostActor != 0)) && (actorControl.hostActor == act))
                {
                    this.m_soulImage.fillAmount = ((float) curVal) / ((float) maxVal);
                }
            }
        }

        private void onSoulLvlChange(PoolObjHandle<ActorRoot> act, int curVal)
        {
            if ((this.m_soulLevel != null) && (act.handle == base.actor))
            {
                this.m_soulLevel.text = curVal.ToString();
            }
            else if (this.IsPlayerCopy())
            {
                MonsterWrapper actorControl = base.actor.ActorControl as MonsterWrapper;
                if (((actorControl != null) && (actorControl.hostActor != 0)) && (actorControl.hostActor == act))
                {
                    this.m_soulLevel.text = curVal.ToString();
                }
            }
        }

        public override void OnUse()
        {
            base.OnUse();
            this.hudHeight = 0;
            this.HudType = HudCompType.Type_Hero;
            this.m_hudFrameStampOnCreated = 0;
            this.m_hud = null;
            this.m_bloodImage = null;
            this.m_blackBars.Clear();
            this.m_soulImage = null;
            this.m_energyImage = null;
            this.m_soulLevel = null;
            this.m_outOfControlBar = null;
            this.m_outofControlList = null;
            this.m_outOfControlGo = null;
            this.m_mapPointerContainer_small = null;
            this.m_mapPointer_small = null;
            this.m_mapPointerContainer_big = null;
            this.m_mapPointer_big = null;
            this.bigTower_Img = null;
            this.smallTower_Img = null;
            this.m_signalImage = null;
            this.m_actorPos = Vector3.zero;
            this.m_bloodPos = Vector3.zero;
            this.m_bloodDirty = false;
            this.m_textHudContainer = null;
            this.m_textHudNode = null;
            this.textCom = null;
            this.txt_hud_offset_x = 0;
            this.txt_hud_offset_y = 0;
            this.rtTransform = null;
            this.m_exclamationMarkHelper = null;
            this.LastBaseAttackTime = 0L;
            this.bBossHpBar = false;
            this.m_hudVisible = false;
            this.m_textVisible = false;
            this.m_pointerVisible = false;
            this._statusGo = null;
            this._mountPoint = null;
            this._shieldGo = null;
            this._bloodDecImage = null;
            this._lastMaxBarValue = 0;
            this._curShield1 = 0;
            this._shieldImagWidth = 0f;
            this._isDecreasingHp = false;
            this.m_effectRoot_small = null;
            this.m_effectRoot_big = null;
            this.m_VoiceIconImage = null;
            this.m_heroProficiencyAni3 = null;
            this.m_heroProficiencyAni4 = null;
            this.m_heroProficiencyAni5 = null;
            this.m_curAni = null;
        }

        public void PlayMapEffect(MiniMapEffect mme)
        {
            if (this.m_effectRoot_small != null)
            {
                GameObject obj2 = Utility.FindChild(this.m_effectRoot_small, mme.ToString());
                SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
                if ((obj2 != null) && (curLvelContext != null))
                {
                    this.m_actorPos = base.actor.gameObject.transform.position;
                    (obj2.transform as RectTransform).anchoredPosition = new Vector2(this.m_actorPos.x * Singleton<CBattleSystem>.instance.world_UI_Factor_Small.x, this.m_actorPos.z * Singleton<CBattleSystem>.instance.world_UI_Factor_Small.y);
                    if (obj2.activeSelf)
                    {
                        obj2.CustomSetActive(false);
                    }
                    obj2.CustomSetActive(true);
                }
            }
        }

        public bool PlayProficiencyAni(uint proficiencyLevel)
        {
            if ((this.m_curAni != null) && this.m_curAni.isPlaying)
            {
                return false;
            }
            if (proficiencyLevel == 3)
            {
                if (this.m_heroProficiencyAni3 != null)
                {
                    this.m_heroProficiencyAni3.gameObject.CustomSetActive(true);
                    this.m_heroProficiencyAni3.Play("Hero_Icon3_Show");
                    this.m_curAni = this.m_heroProficiencyAni3;
                }
            }
            else if (proficiencyLevel == 4)
            {
                if (this.m_heroProficiencyAni4 != null)
                {
                    this.m_heroProficiencyAni4.gameObject.CustomSetActive(true);
                    this.m_heroProficiencyAni4.Play("Hero_Icon4_Show");
                    this.m_curAni = this.m_heroProficiencyAni4;
                }
            }
            else if ((proficiencyLevel == 5) && (this.m_heroProficiencyAni5 != null))
            {
                this.m_heroProficiencyAni5.gameObject.CustomSetActive(true);
                this.m_heroProficiencyAni5.Play("Hero_Icon5_Show");
                this.m_curAni = this.m_heroProficiencyAni5;
            }
            return true;
        }

        public static void PreallocMapPointer(int aliesNum, int enemyNum)
        {
            CUIFormScript formScript = Singleton<CBattleSystem>.GetInstance().m_FormScript;
            if (formScript != null)
            {
                List<int> seqs = new List<int>(enemyNum);
                GameObject widget = formScript.GetWidget(4);
                PreallocMapPointer(seqs, widget, aliesNum);
                widget = formScript.GetWidget(5);
                PreallocMapPointer(seqs, widget, enemyNum);
            }
        }

        private static void PreallocMapPointer(List<int> seqs, GameObject go, int num)
        {
            if (go != null)
            {
                seqs.Clear();
                CUIContainerScript component = go.GetComponent<CUIContainerScript>();
                for (int i = 0; i < num; i++)
                {
                    int element = component.GetElement();
                    if (element != -1)
                    {
                        seqs.Add(element);
                    }
                }
                for (int j = 0; j < seqs.Count; j++)
                {
                    component.RecycleElement(seqs[j]);
                }
            }
        }

        public static void Preload(ref ActorPreloadTab preloadTab)
        {
            for (int i = 0; i < _statusResPath.Length; i++)
            {
                preloadTab.AddParticle(_statusResPath[i]);
            }
            preloadTab.AddParticle("UI3D/Battle/Blood_Bar_Hero.prefab");
            preloadTab.AddParticle("UI3D/Battle/BloodHud.prefab");
        }

        public override void Prepare()
        {
            if (!MonoSingleton<GameFramework>.instance.EditorPreviewMode)
            {
                this.InitHudUI();
                this.InitStatus();
            }
        }

        public void RefreshMapPointerBig()
        {
            CUIFormScript formScript = Singleton<CBattleSystem>.instance.m_FormScript;
            if (((formScript != null) && (formScript.m_sgameGraphicRaycaster != null)) && (this.m_mapPointer_big != null))
            {
                formScript.m_sgameGraphicRaycaster.RefreshGameObject(this.m_mapPointer_big);
            }
        }

        private void ResetHudUI()
        {
            if (((base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Monster) && (this.HudType != HudCompType.Type_Hero)) && ((this.m_hud != null) && (this.HudType != HudCompType.Type_Hide)))
            {
                bool isHostCamp = base.actor.IsHostCamp();
                ActorTypeDef actorType = base.actor.TheActorMeta.ActorType;
                if (this.HudType == HudCompType.Type_Boss)
                {
                    actorType = ActorTypeDef.Actor_Type_Hero;
                }
                this.m_bloodImage.spriteName = Enum.GetName(typeof(SpriteNameEnum), (ActorTypeDef.Actor_Type_EYE * actorType) + (!isHostCamp ? ActorTypeDef.Actor_Type_Organ : ActorTypeDef.Actor_Type_Monster));
                this.m_bloodImage.SetNativeSize(Moba_Camera.currentMobaCamera, 30f);
                if (base.actor.ValueComponent.actorHpTotal > 0)
                {
                    this.m_bloodImage.fillAmount = ((float) base.actor.ValueComponent.actorHp) / ((float) base.actor.ValueComponent.actorHpTotal);
                }
                else
                {
                    this.m_bloodImage.fillAmount = 0f;
                }
                if (this.m_VoiceIconImage != null)
                {
                    this.m_VoiceIconImage.gameObject.CustomSetActive(false);
                }
                Transform hudPanel = this.GetHudPanel(this.HudType, isHostCamp);
                if (hudPanel != null)
                {
                    this.m_hud.transform.SetParent(hudPanel, true);
                    this.m_hud.transform.localScale = Vector3.one;
                    this.m_hud.transform.localRotation = Quaternion.identity;
                }
                this.setHudVisible(true, true);
                this.m_bloodDirty = true;
                this.HudInit_MapPointer();
                SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
                if (curLvelContext != null)
                {
                    this.setPointerVisible(curLvelContext.isPVPMode, true);
                }
            }
        }

        protected void SetBlackBar(int curMaxHpBarValue)
        {
            if (curMaxHpBarValue != this._lastMaxBarValue)
            {
                this._lastMaxBarValue = curMaxHpBarValue;
                int num = 0x3e8;
                int num2 = -1;
                while (num < curMaxHpBarValue)
                {
                    num2++;
                    if (num2 < this.m_blackBars.Count)
                    {
                        if (!this.m_blackBars[num2].activeSelf)
                        {
                            this.m_blackBars[num2].SetActive(true);
                        }
                    }
                    else
                    {
                        GameObject gameObject = Singleton<CGameObjectPool>.GetInstance().GetGameObject("UI3D/Battle/BlackBarBlack.prefab", enResourceType.BattleScene);
                        if (!gameObject.activeSelf)
                        {
                            gameObject.SetActive(true);
                        }
                        gameObject.transform.parent = this.m_bloodImage.transform;
                        gameObject.transform.SetSiblingIndex(this.m_bloodImage.transform.childCount - 1);
                        gameObject.transform.localRotation = Quaternion.identity;
                        this.m_blackBars.Add(gameObject);
                    }
                    this.m_blackBars[num2].transform.localPosition = new Vector3((((float) num) / ((float) curMaxHpBarValue)) * this._shieldImagWidth, 0f, 0f);
                    num += 0x3e8;
                }
                for (int i = num2 + 1; i < this.m_blackBars.Count; i++)
                {
                    this.m_blackBars[i].SetActive(false);
                }
            }
        }

        public void SetComVisible(bool bVisiable)
        {
            if (this._mountPoint != null)
            {
                this._mountPoint.CustomSetActive(bVisiable);
            }
            this.setHudVisible(false, false);
            this.setTextVisible(false, false);
            if ((base.actor.ActorControl.GetActorSubType() == 2) && (base.actor.TheActorMeta.ConfigId != Singleton<BattleLogic>.instance.DragonId))
            {
                this.setPointerVisible(bVisiable, true);
            }
            else
            {
                this.setPointerVisible(false, false);
            }
        }

        private void setHudVisible(bool show, bool local)
        {
            if (local)
            {
                this.m_hudVisible = show;
            }
            if (this.m_hud != null)
            {
                bool flag = (this.m_hudVisible && base.actor.Visible) && base.actor.InCamera;
                if (flag != this.m_hud.activeSelf)
                {
                    this.m_bloodDirty = true;
                }
            }
        }

        public void SetOutOfControlBar()
        {
            if (this.m_outofControlList.Count <= 0)
            {
                this.m_outOfControlGo.CustomSetActive(false);
                if (this.m_outOfControlBar.fillAmount != 0f)
                {
                    this.m_outOfControlBar.fillAmount = 0f;
                }
            }
            else
            {
                CoutofControlInfo info = this.m_outofControlList[0];
                for (int i = 1; i < this.m_outofControlList.Count; i++)
                {
                    if (info.leftTime < this.m_outofControlList[i].leftTime)
                    {
                        info = this.m_outofControlList[i];
                    }
                }
                this.m_outOfControlGo.CustomSetActive(true);
                float num2 = ((float) info.leftTime) / ((float) info.totalTime);
                this.m_outOfControlBar.fillAmount = num2;
            }
        }

        private void setPointerVisible(bool show, bool local)
        {
            bool flag = (base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Monster) && (base.actor.ActorControl.GetActorSubType() == 2);
            bool flag2 = flag && (base.actor.TheActorMeta.ConfigId == Singleton<BattleLogic>.instance.DragonId);
            if (((flag && !flag2) && (local && !this.m_pointerVisible)) && (show && base.actor.Visible))
            {
                bool bActive = !base.actor.ActorControl.IsDeadState;
                this.m_mapPointer_small.CustomSetActive(bActive);
                this.m_mapPointer_big.CustomSetActive(bActive);
                this.m_pointerVisible = false;
            }
            else
            {
                if (local)
                {
                    this.m_pointerVisible = show;
                }
                if (!flag)
                {
                    bool flag4 = (this.m_pointerVisible && base.actor.Visible) && !base.actor.ActorControl.IsDeadState;
                    if ((base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Organ) && !base.actor.ActorControl.IsDeadState)
                    {
                        flag4 = true;
                    }
                    if (this.m_mapPointer_small != null)
                    {
                        this.m_mapPointer_small.CustomSetActive(flag4);
                    }
                    if (this.m_mapPointer_big != null)
                    {
                        this.m_mapPointer_big.CustomSetActive(flag4);
                    }
                }
            }
        }

        public void SetSelected(bool bHost)
        {
            if (bHost)
            {
                this.m_bloodImage.spriteName = "bl_hero_self";
            }
            else
            {
                this.m_bloodImage.spriteName = "bl_hero_mid";
            }
        }

        public void SetTextHud(string text, int txt_offset_x, int txt_offset_y, bool bCenter = false)
        {
            if (this.m_textHudContainer == null)
            {
                this.m_textHudContainer = this.GetTextHudContainer();
            }
            if ((this.m_textHudContainer != null) && (this.m_textHudNode == null))
            {
                this.Init_TextHud(this.m_textHudContainer.GetElement(), false);
            }
            this.txt_hud_offset_x = txt_offset_x;
            this.txt_hud_offset_y = txt_offset_y;
            bool flag = string.IsNullOrEmpty(text);
            if (flag)
            {
                this.setTextVisible(false, true);
            }
            if (!flag && (this.m_textHudNode != null))
            {
                this.SetTextHudContent(text);
                this.setTextVisible(true, true);
            }
        }

        private void SetTextHudContent(string txt)
        {
            this.textCom.text = txt;
        }

        private void setTextVisible(bool show, bool local)
        {
            if (local)
            {
                this.m_textVisible = show;
            }
            if (this.m_textHudNode != null)
            {
                bool bActive = (this.m_textVisible && base.actor.Visible) && base.actor.InCamera;
                if (bActive != this.m_textHudNode.activeSelf)
                {
                    this.m_textHudNode.CustomSetActive(bActive);
                }
            }
        }

        private Image SetTower_Image(bool bSelf, int value, GameObject mapPointer, CUIFormScript formScript)
        {
            if ((mapPointer == null) || (formScript == null))
            {
                return null;
            }
            GameObject gameObject = mapPointer.transform.Find("icon").gameObject;
            if (gameObject == null)
            {
                return null;
            }
            Image component = gameObject.GetComponent<Image>();
            GameObject obj3 = mapPointer.transform.Find("icon/front").gameObject;
            if (obj3 == null)
            {
                return null;
            }
            Image image = obj3.GetComponent<Image>();
            if ((component == null) || (image == null))
            {
                return null;
            }
            if (value == 2)
            {
                component.SetSprite(!bSelf ? MinimapSys.enemy_base : MinimapSys.self_base, formScript, true, false, false);
                image.SetSprite(!bSelf ? MinimapSys.enemy_base_outline : MinimapSys.self_base_outline, formScript, true, false, false);
                return component;
            }
            if ((value == 1) || (value == 4))
            {
                component.SetSprite(!bSelf ? MinimapSys.enemy_tower : MinimapSys.self_tower, formScript, true, false, false);
                image.SetSprite(!bSelf ? MinimapSys.enemy_tower_outline : MinimapSys.self_tower_outline, formScript, true, false, false);
            }
            return component;
        }

        public void ShowHeadExclamationMark(string eftPath, float offset_height)
        {
            if ((Singleton<CBattleSystem>.GetInstance().GetSignalPanel() != null) && (this.m_exclamationMarkHelper == null))
            {
                Vector3 position = base.actor.gameObject.transform.position;
                Vector3 worldPosition = new Vector3(position.x, position.y + offset_height, position.z);
                this.m_exclamationMarkHelper = this.CreateSignalGameObject(eftPath, worldPosition);
                this.m_exclamationMarkHelper.m_yOffset = offset_height;
                this.m_exclamationMarkHelper.m_delegatePlayEnd = (ParticleLifeHelper.DelegateOnPlayEnd) Delegate.Combine(this.m_exclamationMarkHelper.m_delegatePlayEnd, new ParticleLifeHelper.DelegateOnPlayEnd(this.OnSinglePlayEnd));
            }
        }

        public void ShowStatus(StatusHudType st)
        {
            if (this._mountPoint != null)
            {
                this._mountPoint.CustomSetActive(base.actor.Visible);
                int index = (int) st;
                GameObject obj2 = this._statusGo[index];
                if (obj2 == null)
                {
                    UnityEngine.Object content = Singleton<CResourceManager>.GetInstance().GetResource(_statusResPath[index], typeof(GameObject), enResourceType.BattleScene, false, false).m_content;
                    if (content != null)
                    {
                        obj2 = (GameObject) UnityEngine.Object.Instantiate(content);
                        this._statusGo[index] = obj2;
                        if (obj2 != null)
                        {
                            obj2.transform.SetParent(this._mountPoint.transform);
                            obj2.transform.localPosition = OFFSET_HEIGHT;
                        }
                    }
                }
                if (obj2 != null)
                {
                    obj2.CustomSetActive(true);
                }
            }
        }

        public void ShowVoiceIcon(bool bShow)
        {
            if (this.m_VoiceIconImage != null)
            {
                this.m_VoiceIconImage.gameObject.CustomSetActive(bShow);
            }
        }

        protected void TryToStartDecreaseHpEffect()
        {
            if (this._bloodDecImage != null)
            {
                if (this.m_bloodImage.fillAmount >= this._bloodDecImage.fillAmount)
                {
                    this._bloodDecImage.fillAmount = this.m_bloodImage.fillAmount;
                    this._isDecreasingHp = false;
                }
                if (!this._isDecreasingHp && (this.m_bloodImage.fillAmount < this._bloodDecImage.fillAmount))
                {
                    this._isDecreasingHp = true;
                }
            }
        }

        public override void Uninit()
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int, int, int>("HeroSoulExpChange", new Action<PoolObjHandle<ActorRoot>, int, int, int>(this, (IntPtr) this.onSoulExpChange));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int, int>("HeroEnergyChange", new Action<PoolObjHandle<ActorRoot>, int, int>(this, (IntPtr) this.onEnergyExpChange));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int>("HeroSoulLevelChange", new Action<PoolObjHandle<ActorRoot>, int>(this, (IntPtr) this.onSoulLvlChange));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<LimitMoveEventParam>(GameSkillEventDef.Event_LimitMove, new GameSkillEvent<LimitMoveEventParam>(this.OnPlayerLimitMove));
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<LimitMoveEventParam>(GameSkillEventDef.Event_CancelLimitMove, new GameSkillEvent<LimitMoveEventParam>(this.OnPlayerCancelLimitMove));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorBeAttack, new RefAction<DefaultGameEventParam>(this.OnBaseAttacked));
            for (int i = 0; i < this.m_blackBars.Count; i++)
            {
                Singleton<CGameObjectPool>.GetInstance().RecycleGameObject(this.m_blackBars[i]);
            }
            this.m_blackBars.Clear();
            if (this.m_hud != null)
            {
                Singleton<CGameObjectPool>.GetInstance().RecycleGameObject(this.m_hud);
            }
            this.m_hud = null;
            this.m_bloodImage = null;
            this._shieldGo = null;
            this.m_soulImage = null;
            this.m_energyImage = null;
            this.m_soulLevel = null;
            this.m_outOfControlBar = null;
            this.m_outofControlList = null;
            this.m_outOfControlGo = null;
            if ((this.m_mapPointerContainer_small != null) && (this.m_mapPointer_small != null))
            {
                this.m_mapPointerContainer_small.RecycleElement(this.m_mapPointer_small);
            }
            this.m_mapPointerContainer_small = null;
            this.m_mapPointer_small = null;
            if ((this.m_mapPointerContainer_big != null) && (this.m_mapPointer_big != null))
            {
                this.m_mapPointerContainer_big.RecycleElement(this.m_mapPointer_big);
            }
            this.m_mapPointerContainer_big = null;
            this.m_mapPointer_big = null;
            this.bigTower_Img = null;
            this.smallTower_Img = null;
            if ((this.m_textHudContainer != null) && (this.m_textHudNode != null))
            {
                this.m_textHudContainer.RecycleElement(this.m_textHudNode);
            }
            this.m_textHudContainer = null;
            this.m_textHudNode = null;
            this.textCom = null;
            this.txt_hud_offset_x = 0;
            this.txt_hud_offset_y = 0;
            this.rtTransform = null;
            this._statusGo = null;
            if (this._mountPoint != null)
            {
                UnityEngine.Object.Destroy(this._mountPoint);
                this._mountPoint = null;
            }
            this._shieldGo = null;
            this._bloodDecImage = null;
            this._curShield1 = 0;
            this._shieldImagWidth = 0f;
            this._isDecreasingHp = false;
            this._lastMaxBarValue = 0;
            this.m_effectRoot_small = null;
            this.m_effectRoot_big = null;
            if (this.m_exclamationMarkHelper != null)
            {
                this.m_exclamationMarkHelper.m_delegatePlayEnd = (ParticleLifeHelper.DelegateOnPlayEnd) Delegate.Remove(this.m_exclamationMarkHelper.m_delegatePlayEnd, new ParticleLifeHelper.DelegateOnPlayEnd(this.OnSinglePlayEnd));
                this.m_exclamationMarkHelper.Stop();
                this.m_exclamationMarkHelper = null;
            }
            this.m_heroProficiencyAni3 = null;
            this.m_heroProficiencyAni4 = null;
            this.m_heroProficiencyAni5 = null;
            this.m_curAni = null;
        }

        public void UpdateBloodBar(int curValue, int maxValue)
        {
            if ((this.m_hud != null) && (maxValue != 0))
            {
                if ((curValue == 0) || ((this.HudType == HudCompType.Type_Soldier) && (curValue >= maxValue)))
                {
                    this.setHudVisible(false, true);
                    if (this.m_textHudNode != null)
                    {
                        this.setTextVisible(false, true);
                    }
                }
                else
                {
                    this.setHudVisible(true, true);
                    if (ActorHelper.IsHostCtrlActor(ref this.actorPtr))
                    {
                        this.m_hud.transform.SetAsLastSibling();
                    }
                    if (this.m_bloodImage != null)
                    {
                        this.m_bloodImage.fillAmount = ((float) curValue) / ((float) maxValue);
                        this.UpdateHpAndShieldBarEffect();
                        this.TryToStartDecreaseHpEffect();
                    }
                }
            }
        }

        protected void UpdateHpAndShieldBarEffect()
        {
            if (this._shieldGo != null)
            {
                this._shieldGo.CustomSetActive(this._curShield1 > 0);
                int curMaxHpBarValue = (this._curShield1 <= 0) ? base.actor.ValueComponent.actorHpTotal : Math.Max(base.actor.ValueComponent.actorHpTotal, this._curShield1 + base.actor.ValueComponent.actorHp);
                if (curMaxHpBarValue > 0f)
                {
                    this.m_bloodImage.fillAmount = ((float) base.actor.ValueComponent.actorHp) / ((float) curMaxHpBarValue);
                }
                else
                {
                    this.m_bloodImage.fillAmount = 0f;
                }
                this.SetBlackBar(curMaxHpBarValue);
                if (this._curShield1 > 0)
                {
                    float x = this._shieldImagWidth * this.m_bloodImage.fillAmount;
                    this._shieldGo.GetComponent<Sprite3D>().fillAmount = ((float) this._curShield1) / ((float) curMaxHpBarValue);
                    this._shieldGo.GetComponent<Transform>().localPosition = new Vector3(x, 0f, 0f);
                }
            }
        }

        public override void UpdateLogic(int delta)
        {
        }

        public void UpdateShieldValue(ProtectType shieldType, int changeValue)
        {
            this._curShield1 += changeValue;
            if (this._curShield1 < 0)
            {
                this._curShield1 = 0;
            }
            this.UpdateHpAndShieldBarEffect();
        }

        private void UpdateUIHud(ref Vector3 bloodPosition)
        {
            if (this.m_hud != null)
            {
                bloodPosition.Set(bloodPosition.x, bloodPosition.y, 30f);
                this.m_hud.transform.position = Moba_Camera.currentMobaCamera.ScreenToWorldPoint(bloodPosition);
                if ((this.m_textHudNode != null) && (Singleton<CBattleSystem>.GetInstance().m_FormScript != null))
                {
                    Vector3 vector = CUIUtility.ScreenToWorldPoint(Singleton<CBattleSystem>.GetInstance().m_FormScript.GetCamera(), bloodPosition, this.m_textHudNode.transform.position.z);
                    this.m_textHudNode.transform.position = new Vector3(vector.x + this.txt_hud_offset_x, vector.y + this.txt_hud_offset_y, vector.z);
                }
            }
        }

        private void UpdateUIMap(ref Vector3 actorPosition)
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if ((curLvelContext != null) && curLvelContext.isPVPMode)
            {
                this.UpdateUIMap(ref actorPosition, (float) curLvelContext.mapWidth, (float) curLvelContext.mapHeight);
            }
        }

        private void UpdateUIMap(ref Vector3 actorPosition, float mapWidth, float mapHeight)
        {
            if ((mapWidth != 0f) && (mapHeight != 0f))
            {
                if (this.m_mapPointer_small != null)
                {
                    RectTransform transform = this.m_mapPointer_small.transform as RectTransform;
                    transform.anchoredPosition = new Vector2(actorPosition.x * Singleton<CBattleSystem>.instance.world_UI_Factor_Small.x, actorPosition.z * Singleton<CBattleSystem>.instance.world_UI_Factor_Small.y);
                }
                if (this.m_mapPointer_big != null)
                {
                    RectTransform transform2 = this.m_mapPointer_big.transform as RectTransform;
                    transform2.anchoredPosition = new Vector2(actorPosition.x * Singleton<CBattleSystem>.instance.world_UI_Factor_Big.x, actorPosition.z * Singleton<CBattleSystem>.instance.world_UI_Factor_Big.y);
                    CUIFormScript formScript = Singleton<CBattleSystem>.instance.m_FormScript;
                    if ((base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Organ) || (((base.actor.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero) && (Singleton<CBattleSystem>.instance.GetMinimapSys() != null)) && (Singleton<CBattleSystem>.instance.GetMinimapSys().CurMapType() == MinimapSys.EMapType.Big)))
                    {
                        this.RefreshMapPointerBig();
                    }
                }
            }
        }
    }
}

