namespace Assets.Scripts.GameLogic
{
    using AGE;
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.GameKernal;
    using Assets.Scripts.GameSystem;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public class BattleLogic : Singleton<BattleLogic>, IUpdateLogic
    {
        public AttackOrder attackOrder = new AttackOrder();
        public BattleStatistic battleStat;
        public GameTaskSys battleTaskSys = new GameTaskSys();
        public ClashAddition clashAddition = new ClashAddition();
        public DynamicDifficulty dynamicDifficulty = new DynamicDifficulty();
        public DynamicProperty dynamicProperty = new DynamicProperty();
        public Horizon horizon = new Horizon();
        public HostPlayerLogic hostPlayerLogic = new HostPlayerLogic();
        public IncomeControl incomeCtrl;
        public bool isFighting;
        public bool isGameOver;
        public bool IsModifyingCamera;
        public bool isRuning;
        public bool isWaitGameEnd;
        public bool isWaitMultiStart;
        public float m_Ab_FPS_time;
        public float m_Abnormal_FPS_Count;
        public Vector2 m_battleSceneSize = new Vector2(133f, 22f);
        public bool m_bIsPayStat;
        public CSPkg m_cachedSvrEndData;
        public string m_countDownTips;
        public SpawnGroup m_dragonSpawn;
        public float m_fAveFPS;
        public long m_fpsCount;
        public long m_fpsCunt10;
        public long m_fpsCunt18;
        private float m_FpsTimeBegin;
        public IGameInfo m_GameInfo;
        private bool m_isUserConfirmedQualityDegrade;
        public float m_LastFps;
        private int m_lowFPSTimeDeadline = 0x4e20;
        private bool m_needAutoCheckQUality = true;
        private int m_qualitySetting;
        private int m_totalLowFPSTime;
        public MapWrapper mapLogic;
        private Dictionary<uint, int> s_dragonBuffIds;
        public SoldierControl soldierCtrl;
        public CBattleValueAdjust valAdjustCtrl = new CBattleValueAdjust();

        private void _LevelDownQuality()
        {
            GameSettings.EnableOutline = false;
            GameSettings.ParticleLOD++;
            if (this.m_qualitySetting < GameSettings.ParticleLOD)
            {
                this.m_qualitySetting = GameSettings.ParticleLOD;
            }
            this.LevelDownShadowQuality();
            this.m_totalLowFPSTime = 0;
            PlayerPrefs.SetInt("degrade", 1);
            PlayerPrefs.Save();
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("LOD_Down", null, true);
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("LOD_Down_" + this.GetLevelTypeDescription(), null, true);
            this.m_needAutoCheckQUality = false;
        }

        private void ApplyDynamicQualityCheck()
        {
            GameSettings.ModelLOD = this.m_qualitySetting;
        }

        public void BindFightPrepareFinListener()
        {
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_FightPrepareFin, new RefAction<DefaultGameEventParam>(this.OnFightPrepareFin));
        }

        public void DoBattleStart()
        {
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("DoBattleStart", null, true);
            this.m_fAveFPS = 0f;
            this.m_fpsCount = 0L;
            this.m_fpsCunt10 = 0L;
            this.m_fpsCunt18 = 0L;
            this.m_FpsTimeBegin = Time.time;
            this.m_Ab_FPS_time = 0f;
            this.m_Abnormal_FPS_Count = 0f;
            this.m_LastFps = 0f;
            DebugHelper.Assert(!this.isFighting, "isFighting == false");
            if (!this.isFighting)
            {
                this.isFighting = true;
                Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Degrade_Quality_Accept, new CUIEventManager.OnUIEventHandler(this.LevelDownQualityAccept));
                Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Degrade_Quality_Cancel, new CUIEventManager.OnUIEventHandler(this.LevelDownQualityCancel));
                if (this.battleStat != null)
                {
                    this.battleStat.StartStatistic();
                    if ((Singleton<CBattleSystem>.GetInstance().scoreBoard != null) && Singleton<CBattleSystem>.GetInstance().scoreBoard.IsShown())
                    {
                        Singleton<CBattleSystem>.GetInstance().scoreBoard.RegiseterEvent();
                        Singleton<CBattleSystem>.GetInstance().scoreBoard.Show();
                    }
                }
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Battle_ActivateForm);
                if (this.m_GameInfo != null)
                {
                    this.m_GameInfo.StartFight();
                }
                if (this.mapLogic != null)
                {
                    this.mapLogic.Startup();
                }
                if (this.soldierCtrl != null)
                {
                    this.soldierCtrl.Startup();
                }
                if (this.incomeCtrl == null)
                {
                    this.incomeCtrl = new IncomeControl();
                }
                this.incomeCtrl.ResetAllocSoulLvlMap();
                Singleton<GameObjMgr>.GetInstance().StartFight();
                Singleton<GameStateCtrl>.GetInstance().GotoState("BattleState");
                Singleton<GameInput>.GetInstance().ChangeBattleMode(false);
                this.DoBattleStartEvent();
                this.DoBattleStartFightStart();
                this.SpawnMapBuffs();
                this.SendSecureStartInfoReq();
                Singleton<CSurrenderSystem>.instance.Reset();
                this.InitDynamicQualityCheck();
            }
        }

        private void DoBattleStartEvent()
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            DebugHelper.Assert(hostPlayer != null, "Fatal Error when DoBattleStartEvent, HostPlayer is null!");
            DefaultGameEventParam prm = new DefaultGameEventParam(hostPlayer.Captain, hostPlayer.Captain);
            Singleton<GameEventSys>.instance.SendEvent<DefaultGameEventParam>(GameEventDef.Event_FightStart, ref prm);
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_FightOver, new RefAction<DefaultGameEventParam>(this.onFightOver));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_GameEnd, new RefAction<DefaultGameEventParam>(this.onGameEnd));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.onActorDead));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorRevive, new RefAction<DefaultGameEventParam>(this.onActorRevive));
            Singleton<GameEventSys>.instance.RmvEventHandler<HurtEventResultInfo>(GameEventDef.Event_ActorDamage, new RefAction<HurtEventResultInfo>(this.OnActorDamage));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_FightOver, new RefAction<DefaultGameEventParam>(this.onFightOver));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_GameEnd, new RefAction<DefaultGameEventParam>(this.onGameEnd));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.onActorDead));
            Singleton<GameEventSys>.instance.AddEventHandler<HurtEventResultInfo>(GameEventDef.Event_ActorDamage, new RefAction<HurtEventResultInfo>(this.OnActorDamage));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorRevive, new RefAction<DefaultGameEventParam>(this.onActorRevive));
        }

        private void DoBattleStartFightStart()
        {
            Debug.Log("Enter DoBattleStartFightStart");
            GameSettings.FightStart();
            DebugHelper.Assert(this.attackOrder != null, "attackOrder is null");
            this.attackOrder.FightStart();
            DebugHelper.Assert(this.dynamicProperty != null, "dynamicProperty is null");
            this.dynamicProperty.FightStart();
            DebugHelper.Assert(this.clashAddition != null, "clashAddition is null");
            this.clashAddition.FightStart();
            DebugHelper.Assert(this.dynamicDifficulty != null, "dynamicDifficulty is null");
            this.dynamicDifficulty.FightStart();
            DebugHelper.Assert(this.horizon != null, "horizon is null");
            this.horizon.FightStart();
            Singleton<CSettleSystem>.instance.PrepareSettle();
            SLevelContext curLvelContext = this.GetCurLvelContext();
            DebugHelper.Assert(curLvelContext != null, "slc is null");
            if (curLvelContext != null)
            {
                Debug.Log("Enter slc init.");
                DebugHelper.Assert(this.incomeCtrl != null, "incomeCtrl is null");
                this.incomeCtrl.Init(curLvelContext);
                DebugHelper.Assert(curLvelContext.battleTaskOfCamps != null, "slc.battleTaskOfCamps is null");
                DebugHelper.Assert(this.battleTaskSys != null, "battleTaskSys is null");
                for (int i = 0; i < curLvelContext.battleTaskOfCamps.Length; i++)
                {
                    if (curLvelContext.battleTaskOfCamps[i] > 0)
                    {
                        this.battleTaskSys.AddTask(curLvelContext.battleTaskOfCamps[i], true);
                    }
                }
                Singleton<CBattleSystem>.instance.ShowTaskView(this.battleTaskSys.HasTask);
                if (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_ARENA)
                {
                    Singleton<CBattleSystem>.GetInstance().ShowArenaTimer();
                }
            }
            if (Singleton<CBattleSystem>.instance.GetBattleMisc() != null)
            {
                Singleton<CBattleSystem>.instance.GetBattleMisc().BindHP();
            }
            DebugHelper.Assert(this.hostPlayerLogic != null, "hostPlayerLogic is null");
            this.hostPlayerLogic.FightStart();
            Debug.Log("End of DoBattleStartFightStart");
        }

        public void DoFightOver(bool bNormalEnd)
        {
            if (!this.isFighting)
            {
                DebugHelper.Assert(false, "wtf, 重复调用DoFightOver");
            }
            else
            {
                Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("DoFightOver", null, true);
                this.horizon.FightOver();
                Singleton<GameObjMgr>.GetInstance().FightOver();
                if (this.mapLogic != null)
                {
                    this.mapLogic.Reset();
                }
                this.attackOrder.FightOver();
                this.dynamicProperty.FightOver();
                this.clashAddition.FightOver();
                this.battleTaskSys.Clear();
                Singleton<CBattleSystem>.GetInstance().DisableUIEvent();
                this.hostPlayerLogic.FightOver();
                Singleton<FrameSynchr>.instance.SwitchSynchrLocal();
                this.isFighting = false;
                this.isGameOver = true;
                this.isWaitGameEnd = bNormalEnd;
                this.m_cachedSvrEndData = null;
            }
        }

        private void DynamicCheckQualitySetting(int delta)
        {
            if (((!MonoSingleton<Reconnection>.GetInstance().isProcessingRelayRecover && (Singleton<FrameSynchr>.GetInstance().tryCount <= 1)) && (this.isFighting && this.m_needAutoCheckQUality)) && !this.IsAreadyLowestQuality())
            {
                if (GameFramework.m_fFps < 15f)
                {
                    this.m_totalLowFPSTime += delta;
                }
                if (this.m_totalLowFPSTime > this.m_lowFPSTimeDeadline)
                {
                    this.LevelDownQuality();
                    this.m_lowFPSTimeDeadline = Mathf.Max(0x2af8, this.m_lowFPSTimeDeadline / 2);
                }
            }
        }

        public bool FilterEnemyActor(ref PoolObjHandle<ActorRoot> actor)
        {
            return ActorHelper.IsHostEnemyActor(ref actor);
        }

        public bool FilterTeamActor(ref PoolObjHandle<ActorRoot> actor)
        {
            return ActorHelper.IsHostCampActor(ref actor);
        }

        public SLevelContext GetCurLvelContext()
        {
            if ((this.m_GameInfo != null) && (this.m_GameInfo.gameContext != null))
            {
                return this.m_GameInfo.gameContext.levelContext;
            }
            return null;
        }

        public int GetDragonBuffId(RES_SKILL_SRC_TYPE type)
        {
            int num = 0;
            if (this.s_dragonBuffIds == null)
            {
                this.s_dragonBuffIds = new Dictionary<uint, int>();
                GameDataMgr.skillCombineDatabin.Accept(new Action<ResSkillCombineCfgInfo>(this.OnVisit));
            }
            this.s_dragonBuffIds.TryGetValue((uint) type, out num);
            return num;
        }

        public string GetLevelTypeDescription()
        {
            if (Singleton<BattleLogic>.GetInstance().GetCurLvelContext() == null)
            {
                return string.Empty;
            }
            if (!Singleton<BattleLogic>.GetInstance().GetCurLvelContext().isPVPLevel)
            {
                return "PVE";
            }
            return ((Singleton<BattleLogic>.GetInstance().GetCurLvelContext().pvpLevelPlayerNum / 2) + "V" + (Singleton<BattleLogic>.GetInstance().GetCurLvelContext().pvpLevelPlayerNum / 2));
        }

        public static COM_PLAYERCAMP GetOppositeCmp(COM_PLAYERCAMP InCamp)
        {
            if (InCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_1)
            {
                return COM_PLAYERCAMP.COM_PLAYERCAMP_2;
            }
            return COM_PLAYERCAMP.COM_PLAYERCAMP_1;
        }

        public static COM_PLAYERCAMP[] GetOthersCmp(COM_PLAYERCAMP InCamp)
        {
            int num = 3;
            COM_PLAYERCAMP[] com_playercampArray = new COM_PLAYERCAMP[num - 1];
            int num2 = (int) InCamp;
            for (int i = 0; i < num; i++)
            {
                if (i != num2)
                {
                    com_playercampArray[(((i <= num2) ? num : 0) + (i - num2)) - 1] = (COM_PLAYERCAMP) i;
                }
            }
            return com_playercampArray;
        }

        public override void Init()
        {
            int num = LayerMask.NameToLayer("Ignore Raycast");
            int num2 = LayerMask.NameToLayer("Actor");
            Physics.IgnoreLayerCollision(num, num2);
            this.battleStat = Singleton<BattleStatistic>.GetInstance();
            Singleton<SkillDetectionControl>.GetInstance();
            Singleton<SkillSelectControl>.GetInstance();
            this.IsModifyingCamera = false;
        }

        public void InitDynamicQualityCheck()
        {
            this.m_needAutoCheckQUality = true;
            this.m_isUserConfirmedQualityDegrade = false;
            this.m_totalLowFPSTime = 0;
            this.m_lowFPSTimeDeadline = 0x4e20;
            this.m_qualitySetting = GameSettings.ModelLOD;
            int @int = PlayerPrefs.GetInt("autoCheckQualityCoolDown", 0);
            if (@int > 0)
            {
                this.m_needAutoCheckQUality = false;
                PlayerPrefs.SetInt("autoCheckQualityCoolDown", @int - 1);
                PlayerPrefs.Save();
            }
        }

        private bool IsAreadyLowestQuality()
        {
            return (((GameSettings.ModelLOD == 2) && (GameSettings.ParticleLOD == 2)) && !GameSettings.EnableOutline);
        }

        public int JudgeBattleResult(PoolObjHandle<ActorRoot> src, PoolObjHandle<ActorRoot> atker)
        {
            SLevelContext curLvelContext = this.GetCurLvelContext();
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            if (hostPlayer != null)
            {
                int playerCamp = (int) hostPlayer.PlayerCamp;
                if (((curLvelContext != null) && (playerCamp >= 0)) && (playerCamp < curLvelContext.battleTaskOfCamps.Length))
                {
                    GameTask task = this.battleTaskSys.GetTask(curLvelContext.battleTaskOfCamps[playerCamp], false);
                    if (task != null)
                    {
                        return (!task.Achieving ? 2 : 1);
                    }
                }
                if (Singleton<WinLoseByStarSys>.instance.bStarted)
                {
                    if (Singleton<WinLoseByStarSys>.instance.isSuccess)
                    {
                        return 1;
                    }
                    if (Singleton<WinLoseByStarSys>.instance.isFailure)
                    {
                        return 2;
                    }
                    return 0;
                }
                if (src != 0)
                {
                    return ((playerCamp == src.handle.TheActorMeta.ActorCamp) ? 2 : 1);
                }
            }
            return 0;
        }

        private void LevelDownQuality()
        {
            if (!this.m_isUserConfirmedQualityDegrade)
            {
                string text = Singleton<CTextManager>.GetInstance().GetText("TIPS_DEGRADE_QUALITY");
                stUIEventParams par = new stUIEventParams();
                Singleton<CUIManager>.GetInstance().OpenSmallMessageBox(text, true, enUIEventID.Degrade_Quality_Accept, enUIEventID.Degrade_Quality_Cancel, par, 10, enUIEventID.Degrade_Quality_Accept, string.Empty, string.Empty, false);
                this.m_needAutoCheckQUality = false;
            }
            else
            {
                if (GameSettings.ParticleLOD < 2)
                {
                    string strContent = Singleton<CTextManager>.GetInstance().GetText("TIPS_AUTO_DEGRADE_QUALITY");
                    Singleton<CUIManager>.GetInstance().OpenTips(strContent, false, 1f, null, new object[0]);
                }
                this._LevelDownQuality();
            }
        }

        public void LevelDownQualityAccept(CUIEvent uiEvent)
        {
            this.m_isUserConfirmedQualityDegrade = true;
            this.m_needAutoCheckQUality = true;
            this._LevelDownQuality();
        }

        public void LevelDownQualityCancel(CUIEvent uiEvent)
        {
            this.m_needAutoCheckQUality = false;
            Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("LOD_Down_Cancel", null, true);
            PlayerPrefs.SetInt("autoCheckQualityCoolDown", 5);
            PlayerPrefs.Save();
        }

        public void LevelDownShadowQuality()
        {
            GameSettings.EnableOutline = false;
            if (GameSettings.ShadowQuality == SGameRenderQuality.High)
            {
                GameSettings.ShadowQuality = SGameRenderQuality.Medium;
            }
        }

        public void MakeAllHeroActorInvincible()
        {
            List<PoolObjHandle<ActorRoot>> heroActors = Singleton<GameObjMgr>.instance.HeroActors;
            int count = heroActors.Count;
            for (int i = 0; i < count; i++)
            {
                PoolObjHandle<ActorRoot> handle = heroActors[i];
                if ((handle != 0) && (handle.handle.ActorControl != null))
                {
                    HeroWrapper actorControl = handle.handle.ActorControl as HeroWrapper;
                    if (actorControl != null)
                    {
                        actorControl.bGodMode = true;
                    }
                }
            }
        }

        public static int MapOtherCampIndex(COM_PLAYERCAMP myCamp, COM_PLAYERCAMP otherCamp)
        {
            return (((int) (((otherCamp <= myCamp) ? COM_PLAYERCAMP.COM_PLAYERCAMP_COUNT : COM_PLAYERCAMP.COM_PLAYERCAMP_MID) + (otherCamp - myCamp))) - 1);
        }

        private void OnActorDamage(ref HurtEventResultInfo hri)
        {
            if ((hri.src == 0) || (hri.src.handle.Visible && hri.src.handle.InCamera))
            {
                PoolObjHandle<ActorRoot> orignalActor;
                PoolObjHandle<ActorRoot> atker;
                DIGIT_TYPE invalid = DIGIT_TYPE.Invalid;
                if ((hri.src != 0) && (hri.src.handle.ActorControl != null))
                {
                    orignalActor = hri.src.handle.ActorControl.GetOrignalActor();
                }
                else
                {
                    orignalActor = hri.src;
                }
                if ((hri.atker != 0) && (hri.atker.handle.ActorAgent != null))
                {
                    atker = hri.atker.handle.ActorControl.GetOrignalActor();
                }
                else
                {
                    atker = hri.atker;
                }
                if (((orignalActor != 0) && ActorHelper.IsHostActor(ref orignalActor)) || ((atker != 0) && ActorHelper.IsHostActor(ref atker)))
                {
                    if (hri.hurtInfo.hurtType == HurtTypeDef.Therapic)
                    {
                        invalid = DIGIT_TYPE.ReviveHp;
                    }
                    else if (hri.hurtInfo.hurtType == HurtTypeDef.MagicHurt)
                    {
                        invalid = (hri.critValue <= 0) ? DIGIT_TYPE.MagicAttackNormal : DIGIT_TYPE.MagicAttackCrit;
                    }
                    else if (hri.hurtInfo.hurtType == HurtTypeDef.PhysHurt)
                    {
                        invalid = (hri.critValue <= 0) ? DIGIT_TYPE.PhysicalAttackNormal : DIGIT_TYPE.PhysicalAttackCrit;
                    }
                    else
                    {
                        invalid = (hri.critValue <= 0) ? DIGIT_TYPE.RealAttackNormal : DIGIT_TYPE.RealAttackCrit;
                    }
                }
                if (invalid != DIGIT_TYPE.Invalid)
                {
                    CBattleSystem instance = Singleton<CBattleSystem>.GetInstance();
                    if (hri.hpChanged != 0)
                    {
                        switch (invalid)
                        {
                            case DIGIT_TYPE.MagicAttackCrit:
                            case DIGIT_TYPE.MagicAttackNormal:
                            case DIGIT_TYPE.PhysicalAttackCrit:
                            case DIGIT_TYPE.PhysicalAttackNormal:
                            case DIGIT_TYPE.RealAttackCrit:
                            case DIGIT_TYPE.RealAttackCrit:
                            {
                                Vector3 vector;
                                Vector3 position = hri.atker.handle.gameObject.transform.position;
                                Vector3 vector3 = hri.src.handle.gameObject.transform.position;
                                float num = UnityEngine.Random.Range((float) -1f, (float) -0.5f);
                                float num2 = UnityEngine.Random.Range((float) 0.5f, (float) 1f);
                                SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
                                if ((curLvelContext != null) && curLvelContext.bCameraFlip)
                                {
                                    position = hri.src.handle.gameObject.transform.position;
                                    vector3 = hri.atker.handle.gameObject.transform.position;
                                    num = UnityEngine.Random.Range((float) 0.5f, (float) 1f);
                                    num2 = UnityEngine.Random.Range((float) -1f, (float) -0.5f);
                                }
                                Vector3 vector4 = position - vector3;
                                if (vector4.x > 0f)
                                {
                                    vector = new Vector3(hri.src.handle.gameObject.transform.position.x + num, hri.src.handle.gameObject.transform.position.y, hri.src.handle.gameObject.transform.position.z);
                                    instance.CreateBattleFloatDigit(hri.hpChanged, invalid, vector, 1);
                                }
                                else
                                {
                                    vector = new Vector3(hri.src.handle.gameObject.transform.position.x + num2, hri.src.handle.gameObject.transform.position.y, hri.src.handle.gameObject.transform.position.z);
                                    instance.CreateBattleFloatDigit(hri.hpChanged, invalid, vector, 0);
                                }
                                return;
                            }
                        }
                        instance.CreateBattleFloatDigit(hri.hpChanged, invalid, hri.src.handle.gameObject.transform.position);
                    }
                }
            }
        }

        private void OnActorDamage_OLD(ref HurtEventResultInfo hri)
        {
            if ((hri.src == 0) || (hri.src.handle.Visible && hri.src.handle.InCamera))
            {
                DIGIT_TYPE invalid = DIGIT_TYPE.Invalid;
                if ((hri.src != 0) && ActorHelper.IsHostActor(ref hri.src))
                {
                    if (hri.hurtInfo.hurtType == HurtTypeDef.Therapic)
                    {
                        invalid = DIGIT_TYPE.ReviveHp;
                    }
                    else if (hri.hurtInfo.hurtType == HurtTypeDef.MagicHurt)
                    {
                        invalid = DIGIT_TYPE.SufferMagicDamage;
                    }
                    else if (hri.hurtInfo.hurtType == HurtTypeDef.PhysHurt)
                    {
                        invalid = DIGIT_TYPE.SufferPhysicalDamage;
                    }
                    else
                    {
                        invalid = DIGIT_TYPE.SufferRealDamage;
                    }
                }
                else if ((hri.atker != 0) && ActorHelper.IsHostActor(ref hri.atker))
                {
                    if (hri.hurtInfo.hurtType == HurtTypeDef.Therapic)
                    {
                        invalid = DIGIT_TYPE.ReviveHp;
                    }
                    else if (hri.hurtInfo.hurtType == HurtTypeDef.MagicHurt)
                    {
                        invalid = (hri.critValue <= 0) ? DIGIT_TYPE.MagicAttackNormal : DIGIT_TYPE.MagicAttackCrit;
                    }
                    else if (hri.hurtInfo.hurtType == HurtTypeDef.PhysHurt)
                    {
                        invalid = (hri.critValue <= 0) ? DIGIT_TYPE.PhysicalAttackNormal : DIGIT_TYPE.PhysicalAttackCrit;
                    }
                    else
                    {
                        invalid = (hri.critValue <= 0) ? DIGIT_TYPE.RealAttackNormal : DIGIT_TYPE.RealAttackCrit;
                    }
                }
                if (invalid != DIGIT_TYPE.Invalid)
                {
                    CBattleSystem instance = Singleton<CBattleSystem>.GetInstance();
                    if (hri.hpChanged != 0)
                    {
                        switch (invalid)
                        {
                            case DIGIT_TYPE.MagicAttackCrit:
                            case DIGIT_TYPE.MagicAttackNormal:
                            case DIGIT_TYPE.PhysicalAttackCrit:
                            case DIGIT_TYPE.PhysicalAttackNormal:
                            case DIGIT_TYPE.RealAttackCrit:
                            case DIGIT_TYPE.RealAttackCrit:
                            {
                                Vector3 vector;
                                Vector3 vector2 = hri.atker.handle.gameObject.transform.position - hri.src.handle.gameObject.transform.position;
                                if (vector2.x > 0f)
                                {
                                    vector = new Vector3(hri.src.handle.gameObject.transform.position.x + UnityEngine.Random.Range((float) -1f, (float) -0.5f), hri.src.handle.gameObject.transform.position.y, hri.src.handle.gameObject.transform.position.z);
                                    instance.CreateBattleFloatDigit(hri.hpChanged, invalid, vector, 1);
                                }
                                else
                                {
                                    vector = new Vector3(hri.src.handle.gameObject.transform.position.x + UnityEngine.Random.Range((float) 0.5f, (float) 1f), hri.src.handle.gameObject.transform.position.y, hri.src.handle.gameObject.transform.position.z);
                                    instance.CreateBattleFloatDigit(hri.hpChanged, invalid, vector, 0);
                                }
                                return;
                            }
                        }
                        instance.CreateBattleFloatDigit(hri.hpChanged, invalid, hri.src.handle.gameObject.transform.position);
                    }
                }
            }
        }

        private void onActorDead(ref DefaultGameEventParam prm)
        {
            SLevelContext curLvelContext = this.GetCurLvelContext();
            if (((curLvelContext != null) && curLvelContext.isPVPMode) && ((prm.src != 0) && ActorHelper.IsHostCtrlActor(ref prm.src)))
            {
                Singleton<CSoundManager>.instance.PostEvent("Set_Dead_Low_Pass", null);
                this.SendHostPlayerDieOrReLive(CS_MULTGAME_DIE_TYPE.CS_MULTGAME_DIE);
            }
        }

        private void onActorRevive(ref DefaultGameEventParam prm)
        {
            SLevelContext curLvelContext = this.GetCurLvelContext();
            if (((curLvelContext != null) && curLvelContext.isPVPMode) && ((prm.src != 0) && ActorHelper.IsHostCtrlActor(ref prm.src)))
            {
                Singleton<CSoundManager>.instance.PostEvent("Reset_Dead_Low_Pass", null);
                this.SendHostPlayerDieOrReLive(CS_MULTGAME_DIE_TYPE.CS_MULTGAME_RELIVE);
            }
        }

        public void OnFailure(PoolObjHandle<ActorRoot> src, PoolObjHandle<ActorRoot> atker)
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if ((curLvelContext != null) && (curLvelContext.SubMapType == 2))
            {
                LobbyMsgHandler.ForceKillCrystal((int) Singleton<GamePlayerCenter>.instance.GetHostPlayer().PlayerCamp);
            }
            else
            {
                this.OnFinish(src, atker, Singleton<BattleLogic>.GetInstance().GetCurLvelContext().FailureDialogId);
            }
        }

        public void onFightOver(ref DefaultGameEventParam prm)
        {
            if (!this.isFighting)
            {
                DebugHelper.Assert(false, "wtf, 重复触发onFightOver");
            }
            else
            {
                Singleton<GameEventSys>.instance.SendEvent<DefaultGameEventParam>(GameEventDef.Event_BeginFightOver, ref prm);
                Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Degrade_Quality_Accept, new CUIEventManager.OnUIEventHandler(this.LevelDownQualityAccept));
                Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Degrade_Quality_Cancel, new CUIEventManager.OnUIEventHandler(this.LevelDownQualityCancel));
                this.DoFightOver(true);
                if (Singleton<LobbyLogic>.instance.inMultiGame)
                {
                    Singleton<LobbyLogic>.GetInstance().StartSettleTimers();
                    Singleton<LobbyLogic>.GetInstance().ReqMultiGameOver(false);
                }
                else
                {
                    if ((Singleton<CBattleSystem>.GetInstance().m_FormScript != null) && Singleton<BattleLogic>.GetInstance().GetCurLvelContext().isPVPLevel)
                    {
                        Singleton<WinLose>.instance.LastSingleGameWin = Singleton<BattleLogic>.instance.JudgeBattleResult(prm.src, prm.orignalAtker) == 1;
                        SettleEventParam param = new SettleEventParam {
                            isWin = Singleton<WinLose>.instance.LastSingleGameWin
                        };
                        Singleton<GameEventSys>.GetInstance().SendEvent<SettleEventParam>(GameEventDef.Event_SettleComplete, ref param);
                    }
                    Singleton<LobbyLogic>.GetInstance().ReqSingleGameFinish(false, false);
                }
                Singleton<GameReplayModule>.instance.OnGameFightOver();
            }
        }

        private void OnFightPrepareFin(ref DefaultGameEventParam prm)
        {
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_FightPrepareFin, new RefAction<DefaultGameEventParam>(this.OnFightPrepareFin));
            this.DoBattleStart();
        }

        protected void OnFinish(PoolObjHandle<ActorRoot> src, PoolObjHandle<ActorRoot> atker, int dialogID)
        {
            DefaultGameEventParam prm = new DefaultGameEventParam(src, atker);
            Singleton<GameEventSys>.instance.SendEvent<DefaultGameEventParam>(GameEventDef.Event_FightOver, ref prm);
            this.MakeAllHeroActorInvincible();
            if (dialogID > 0)
            {
                GameObject inSrc = (src == 0) ? null : src.handle.gameObject;
                GameObject inAtker = (atker == 0) ? null : atker.handle.gameObject;
                MonoSingleton<DialogueProcessor>.GetInstance().PlayDrama(dialogID, inSrc, inAtker, false);
            }
            else
            {
                DefaultGameEventParam param2 = new DefaultGameEventParam(src, atker);
                Singleton<GameEventSys>.instance.SendEvent<DefaultGameEventParam>(GameEventDef.Event_GameEnd, ref param2);
            }
        }

        public void onGameEnd(ref DefaultGameEventParam prm)
        {
            Singleton<LobbyLogic>.GetInstance().StopGameEndTimer();
            Singleton<LobbyLogic>.GetInstance().StartSettlePanelTimer();
            if (this.isWaitGameEnd && (this.m_cachedSvrEndData != null))
            {
                CSPkg cachedSvrEndData = this.m_cachedSvrEndData;
                this.m_cachedSvrEndData = null;
                if (Singleton<LobbyLogic>.instance.inMultiGame)
                {
                    if (cachedSvrEndData.stPkgData.stMultGameSettleGain.iErrCode == 0)
                    {
                        CLevelCfgLogicManager.SetMasterPvpDetailWhenGameSettle(cachedSvrEndData.stPkgData.stMultGameSettleGain.stDetail.stGameInfo);
                    }
                    LobbyMsgHandler.HandleGameSettle(cachedSvrEndData.stPkgData.stMultGameSettleGain.iErrCode == 0, true, cachedSvrEndData.stPkgData.stMultGameSettleGain.stDetail.stGameInfo.bGameResult, cachedSvrEndData.stPkgData.stMultGameSettleGain.stDetail.stHeroList, cachedSvrEndData.stPkgData.stMultGameSettleGain.stDetail.stRankInfo, cachedSvrEndData.stPkgData.stMultGameSettleGain.stDetail.stAcntInfo, cachedSvrEndData.stPkgData.stMultGameSettleGain.stDetail.stMultipleDetail, cachedSvrEndData.stPkgData.stMultGameSettleGain.stDetail.stSpecReward, cachedSvrEndData.stPkgData.stMultGameSettleGain.stDetail.stReward);
                }
                else
                {
                    LobbyMsgHandler.HandleSingleGameSettle(cachedSvrEndData);
                }
            }
            this.isWaitGameEnd = false;
            this.UnRegistEvent();
        }

        public static void OnLoseStarSysChanged(IStarEvaluation InStarEvaluation, IStarCondition InStarCondition)
        {
            if ((Singleton<WinLoseByStarSys>.instance.LoserEvaluation == InStarEvaluation) && Singleton<WinLoseByStarSys>.instance.isFailure)
            {
                PoolObjHandle<ActorRoot> handle;
                PoolObjHandle<ActorRoot> handle2;
                InStarCondition.GetActorRef(out handle, out handle2);
                Singleton<BattleLogic>.instance.OnFailure(handle, handle2);
            }
        }

        public void onPreGameSettle()
        {
            if (this.mapLogic != null)
            {
                this.mapLogic.Reset();
            }
        }

        private void OnVisit(ResSkillCombineCfgInfo InCfg)
        {
            int num = 1;
            int num2 = 4;
            if ((InCfg.iSrcType >= num) && (InCfg.iSrcType < num2))
            {
                if (this.s_dragonBuffIds.ContainsKey((uint) InCfg.iSrcType))
                {
                    this.s_dragonBuffIds[(uint) InCfg.iSrcType] = InCfg.iCfgID;
                }
                else
                {
                    this.s_dragonBuffIds.Add((uint) InCfg.iSrcType, InCfg.iCfgID);
                }
            }
        }

        public void OnWinning(PoolObjHandle<ActorRoot> src, PoolObjHandle<ActorRoot> atker)
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if ((curLvelContext != null) && (curLvelContext.SubMapType == 2))
            {
                if (Singleton<GamePlayerCenter>.instance.GetHostPlayer().PlayerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_1)
                {
                    LobbyMsgHandler.ForceKillCrystal(2);
                }
                else
                {
                    LobbyMsgHandler.ForceKillCrystal(1);
                }
            }
            else
            {
                this.OnFinish(src, atker, Singleton<BattleLogic>.GetInstance().GetCurLvelContext().PassDialogId);
            }
        }

        public static void OnWinStarSysChanged(IStarEvaluation InStarEvaluation, IStarCondition InStarCondition)
        {
            if ((Singleton<WinLoseByStarSys>.instance.WinnerEvaluation == InStarEvaluation) && Singleton<WinLoseByStarSys>.instance.isSuccess)
            {
                PoolObjHandle<ActorRoot> handle;
                PoolObjHandle<ActorRoot> handle2;
                InStarCondition.GetActorRef(out handle, out handle2);
                Singleton<BattleLogic>.instance.OnWinning(handle, handle2);
            }
        }

        public bool PrepareFight()
        {
            SLevelContext curLvelContext = this.GetCurLvelContext();
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            if (curLvelContext != null)
            {
                if (!Singleton<LobbyLogic>.instance.inMultiGame)
                {
                    ResLevelCfgInfo outLevelCfg = null;
                    ResDT_LevelCommonInfo outLevelComInfo = null;
                    CLevelCfgLogicManager.FindLevelConfigSingleGame(curLvelContext.iLevelID, out outLevelCfg, out outLevelComInfo);
                    if (outLevelComInfo != null)
                    {
                        curLvelContext.bBattleEquipLimit = outLevelComInfo.bBattleEquipLimit > 0;
                        curLvelContext.bChaosPickRule = outLevelComInfo.bChaosPickRule > 0;
                        curLvelContext.HeadPtsUpperLimit = outLevelComInfo.bHeadPtsUpperLimit;
                        curLvelContext.BirthLevelConfig = outLevelComInfo.bBirthLevelConfig;
                        if ((hostPlayer != null) && (hostPlayer.PlayerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_2))
                        {
                            curLvelContext.bCameraFlip = outLevelComInfo.bCameraFlip > 0;
                        }
                    }
                }
                else
                {
                    ResDT_LevelCommonInfo info3 = CLevelCfgLogicManager.FindLevelConfigMultiGame(curLvelContext.iLevelID);
                    if (info3 != null)
                    {
                        curLvelContext.bBattleEquipLimit = info3.bBattleEquipLimit > 0;
                        curLvelContext.bChaosPickRule = info3.bChaosPickRule > 0;
                        curLvelContext.HeadPtsUpperLimit = info3.bHeadPtsUpperLimit;
                        curLvelContext.BirthLevelConfig = info3.bBirthLevelConfig;
                        if ((hostPlayer != null) && (hostPlayer.PlayerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_2))
                        {
                            curLvelContext.bCameraFlip = info3.bCameraFlip > 0;
                        }
                    }
                }
            }
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Battle_OpenForm);
            SLevelContext context2 = this.GetCurLvelContext();
            if ((context2 != null) && context2.bShowTrainingHelper)
            {
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Training_HelperInit);
            }
            if (this.valAdjustCtrl == null)
            {
                this.valAdjustCtrl = new CBattleValueAdjust();
            }
            this.valAdjustCtrl.Init();
            Singleton<GameObjMgr>.GetInstance().PrepareFight();
            MonoSingleton<CameraSystem>.GetInstance().PrepareFight();
            this.IsModifyingCamera = false;
            GameObject obj2 = GameObject.Find("Design");
            if (obj2 != null)
            {
                GlobalTrigger component = obj2.GetComponent<GlobalTrigger>();
                if (component != null)
                {
                    component.PrepareFight();
                    this.m_globalTrigger = component;
                }
            }
            Singleton<CBattleSystem>.GetInstance().PostOpenForm();
            bool flag = MonoSingleton<DialogueProcessor>.GetInstance().PrepareFight();
            DefaultGameEventParam prm = new DefaultGameEventParam(Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain, Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain);
            Singleton<GameEventSys>.instance.SendEvent<DefaultGameEventParam>(GameEventDef.Event_FightPrepare, ref prm);
            ActionManager.Instance.frameMode = true;
            return flag;
        }

        public void ResetBattleSystem()
        {
            if (this.mapLogic != null)
            {
                this.mapLogic.Reset();
            }
            this.battleTaskSys.Clear();
            this.ApplyDynamicQualityCheck();
            Singleton<CBattleSystem>.GetInstance().DisableUIEvent();
        }

        public void SendHostPlayerDieOrReLive(CS_MULTGAME_DIE_TYPE type)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x44a);
            msg.stPkgData.stMultGameDieReq.bType = (byte) type;
            Singleton<NetworkModule>.GetInstance().SendGameMsg(ref msg, 0);
        }

        private void SendSecureStartInfoReq()
        {
            Singleton<NetworkModule>.instance.RecvGameMsgCount = 0;
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x426);
            CSPKG_SECURE_INFO_START_REQ stSecureInfoStartReq = msg.stPkgData.stSecureInfoStartReq;
            DateTime time = new DateTime(0x7b2, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan span = (TimeSpan) (DateTime.Now - time);
            msg.stPkgData.stSecureInfoStartReq.dwClientStartTime = (uint) span.TotalSeconds;
            List<PoolObjHandle<ActorRoot>> list = ActorHelper.FilterActors(Singleton<GameObjMgr>.instance.HeroActors, new ActorFilterDelegate(this.FilterEnemyActor));
            stSecureInfoStartReq.iSvrBossCount = list.Count;
            for (int i = 0; i < list.Count; i++)
            {
                PoolObjHandle<ActorRoot> handle3 = list[i];
                stSecureInfoStartReq.iSvrBossHPMax = (stSecureInfoStartReq.iSvrBossHPMax <= handle3.handle.ValueComponent.actorHp) ? list[i].handle.ValueComponent.actorHp : stSecureInfoStartReq.iSvrBossHPMax;
                if (stSecureInfoStartReq.iSvrBossHPMin == 0)
                {
                    PoolObjHandle<ActorRoot> handle5 = list[i];
                    stSecureInfoStartReq.iSvrBossHPMin = handle5.handle.ValueComponent.actorHp;
                }
                else
                {
                    PoolObjHandle<ActorRoot> handle6 = list[i];
                    stSecureInfoStartReq.iSvrBossHPMin = (stSecureInfoStartReq.iSvrBossHPMin >= handle6.handle.ValueComponent.actorHp) ? list[i].handle.ValueComponent.actorHp : stSecureInfoStartReq.iSvrBossHPMin;
                }
                PoolObjHandle<ActorRoot> handle8 = list[i];
                stSecureInfoStartReq.iSvrBossHPTotal += handle8.handle.ValueComponent.actorHp;
                PoolObjHandle<ActorRoot> handle9 = list[i];
                int totalValue = handle9.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue;
                stSecureInfoStartReq.iSvrBossAttackMax = (stSecureInfoStartReq.iSvrBossAttackMax <= totalValue) ? totalValue : stSecureInfoStartReq.iSvrBossAttackMax;
                if (stSecureInfoStartReq.iSvrBossAttackMin == 0)
                {
                    stSecureInfoStartReq.iSvrBossAttackMin = totalValue;
                }
                else
                {
                    stSecureInfoStartReq.iSvrBossAttackMin = (stSecureInfoStartReq.iSvrBossAttackMin >= totalValue) ? totalValue : stSecureInfoStartReq.iSvrBossAttackMin;
                }
                if (list.Count > 0)
                {
                    PoolObjHandle<ActorRoot> handle10 = list[0];
                    stSecureInfoStartReq.iSvrEmenyCardID1 = handle10.handle.TheActorMeta.ConfigId;
                    PoolObjHandle<ActorRoot> handle11 = list[0];
                    stSecureInfoStartReq.iSvrEmenyCardHP1 = handle11.handle.ValueComponent.actorHp;
                    PoolObjHandle<ActorRoot> handle12 = list[0];
                    int num3 = handle12.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardATN1 = num3;
                    PoolObjHandle<ActorRoot> handle13 = list[0];
                    num3 = handle13.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardINT1 = num3;
                    PoolObjHandle<ActorRoot> handle14 = list[0];
                    num3 = handle14.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MOVESPD].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardSpeed1 = num3;
                    PoolObjHandle<ActorRoot> handle15 = list[0];
                    num3 = handle15.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardPhyDef1 = num3;
                    PoolObjHandle<ActorRoot> handle16 = list[0];
                    num3 = handle16.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardSpellDef1 = num3;
                }
                if (list.Count > 1)
                {
                    PoolObjHandle<ActorRoot> handle17 = list[1];
                    stSecureInfoStartReq.iSvrEmenyCardID2 = handle17.handle.TheActorMeta.ConfigId;
                    PoolObjHandle<ActorRoot> handle18 = list[1];
                    stSecureInfoStartReq.iSvrEmenyCardHP2 = handle18.handle.ValueComponent.actorHp;
                    PoolObjHandle<ActorRoot> handle19 = list[1];
                    int num4 = handle19.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardATN2 = num4;
                    PoolObjHandle<ActorRoot> handle20 = list[1];
                    num4 = handle20.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardINT2 = num4;
                    PoolObjHandle<ActorRoot> handle21 = list[1];
                    num4 = handle21.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MOVESPD].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardSpeed2 = num4;
                    PoolObjHandle<ActorRoot> handle22 = list[1];
                    num4 = handle22.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardPhyDef2 = num4;
                    PoolObjHandle<ActorRoot> handle23 = list[1];
                    num4 = handle23.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardSpellDef2 = num4;
                }
                if (list.Count > 2)
                {
                    PoolObjHandle<ActorRoot> handle24 = list[2];
                    stSecureInfoStartReq.iSvrEmenyCardID3 = handle24.handle.TheActorMeta.ConfigId;
                    PoolObjHandle<ActorRoot> handle25 = list[2];
                    stSecureInfoStartReq.iSvrEmenyCardHP3 = handle25.handle.ValueComponent.actorHp;
                    PoolObjHandle<ActorRoot> handle26 = list[2];
                    int num5 = handle26.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardATN3 = num5;
                    PoolObjHandle<ActorRoot> handle27 = list[2];
                    num5 = handle27.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardINT3 = num5;
                    PoolObjHandle<ActorRoot> handle28 = list[2];
                    num5 = handle28.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MOVESPD].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardSpeed3 = num5;
                    PoolObjHandle<ActorRoot> handle29 = list[2];
                    num5 = handle29.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardPhyDef3 = num5;
                    PoolObjHandle<ActorRoot> handle30 = list[2];
                    num5 = handle30.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT].totalValue;
                    stSecureInfoStartReq.iSvrEmenyCardSpellDef3 = num5;
                }
            }
            List<PoolObjHandle<ActorRoot>> organActors = Singleton<GameObjMgr>.instance.OrganActors;
            for (int j = 0; j < organActors.Count; j++)
            {
                PoolObjHandle<ActorRoot> actor = organActors[j];
                int actorHp = actor.handle.ValueComponent.actorHp;
                int num8 = actor.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue;
                if (ActorHelper.IsHostEnemyActor(ref actor))
                {
                    stSecureInfoStartReq.iSvrEmenyBuildingHPMax = (stSecureInfoStartReq.iSvrEmenyBuildingHPMax <= actorHp) ? actorHp : stSecureInfoStartReq.iSvrEmenyBuildingHPMax;
                    if (stSecureInfoStartReq.iSvrEmenyBuildingHPMin == 0)
                    {
                        stSecureInfoStartReq.iSvrEmenyBuildingHPMin = actorHp;
                    }
                    else
                    {
                        stSecureInfoStartReq.iSvrEmenyBuildingHPMin = (stSecureInfoStartReq.iSvrEmenyBuildingHPMin >= actorHp) ? actorHp : stSecureInfoStartReq.iSvrEmenyBuildingHPMin;
                    }
                    stSecureInfoStartReq.iSvrEmenyHPTotal += actorHp;
                    stSecureInfoStartReq.iSvrEmenyBuildingAttackMax = (stSecureInfoStartReq.iSvrEmenyBuildingAttackMax <= num8) ? num8 : stSecureInfoStartReq.iSvrEmenyBuildingAttackMax;
                    if (stSecureInfoStartReq.iSvrEmenyBuildingAttackMin == 0)
                    {
                        stSecureInfoStartReq.iSvrEmenyBuildingAttackMin = num8;
                    }
                    else
                    {
                        stSecureInfoStartReq.iSvrEmenyBuildingAttackMin = (stSecureInfoStartReq.iSvrEmenyBuildingAttackMin >= num8) ? num8 : stSecureInfoStartReq.iSvrEmenyBuildingAttackMin;
                    }
                }
                else
                {
                    stSecureInfoStartReq.iSvrBuildingHPMax = (stSecureInfoStartReq.iSvrBuildingHPMax <= actorHp) ? actorHp : stSecureInfoStartReq.iSvrBuildingHPMax;
                    if (stSecureInfoStartReq.iSvrBuildingHPMin == 0)
                    {
                        stSecureInfoStartReq.iSvrBuildingHPMin = actorHp;
                    }
                    else
                    {
                        stSecureInfoStartReq.iSvrBuildingHPMin = (stSecureInfoStartReq.iSvrBuildingHPMin >= actorHp) ? actorHp : stSecureInfoStartReq.iSvrBuildingHPMin;
                    }
                    stSecureInfoStartReq.iSvrBuildingAttackMax = (stSecureInfoStartReq.iSvrBuildingAttackMax <= num8) ? num8 : stSecureInfoStartReq.iSvrBuildingAttackMax;
                    if (stSecureInfoStartReq.iSvrBuildingAttackMin == 0)
                    {
                        stSecureInfoStartReq.iSvrBuildingAttackMin = num8;
                    }
                    else
                    {
                        stSecureInfoStartReq.iSvrBuildingAttackMin = (stSecureInfoStartReq.iSvrBuildingAttackMin >= num8) ? num8 : stSecureInfoStartReq.iSvrBuildingAttackMin;
                    }
                }
            }
            List<PoolObjHandle<ActorRoot>> list3 = ActorHelper.FilterActors(Singleton<GameObjMgr>.instance.SoldierActors, new ActorFilterDelegate(this.FilterEnemyActor));
            for (int k = 0; k < list3.Count; k++)
            {
                PoolObjHandle<ActorRoot> handle2 = list3[k];
                int num10 = handle2.handle.ValueComponent.actorHp;
                int num11 = handle2.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue;
                stSecureInfoStartReq.iSvrEmenyHPMax = (stSecureInfoStartReq.iSvrEmenyHPMax <= num10) ? num10 : stSecureInfoStartReq.iSvrEmenyHPMax;
                if (stSecureInfoStartReq.iSvrEmenyHPMin == 0)
                {
                    stSecureInfoStartReq.iSvrEmenyHPMin = num10;
                }
                else
                {
                    stSecureInfoStartReq.iSvrEmenyHPMin = (stSecureInfoStartReq.iSvrEmenyHPMin >= num10) ? num10 : stSecureInfoStartReq.iSvrEmenyHPMin;
                }
                stSecureInfoStartReq.iSvrEmenyAttackMax = (stSecureInfoStartReq.iSvrEmenyAttackMax <= num11) ? num11 : stSecureInfoStartReq.iSvrEmenyAttackMax;
                if (stSecureInfoStartReq.iSvrEmenyAttackMin == 0)
                {
                    stSecureInfoStartReq.iSvrEmenyAttackMin = num11;
                }
                else
                {
                    stSecureInfoStartReq.iSvrEmenyAttackMin = (stSecureInfoStartReq.iSvrEmenyAttackMin >= num11) ? num11 : stSecureInfoStartReq.iSvrEmenyAttackMin;
                }
            }
            List<PoolObjHandle<ActorRoot>> list4 = ActorHelper.FilterActors(Singleton<GameObjMgr>.instance.HeroActors, new ActorFilterDelegate(this.FilterTeamActor));
            if (list4.Count > 0)
            {
                PoolObjHandle<ActorRoot> handle31 = list4[0];
                stSecureInfoStartReq.iSvrUserCardID1 = handle31.handle.TheActorMeta.ConfigId;
                PoolObjHandle<ActorRoot> handle32 = list4[0];
                stSecureInfoStartReq.iSvrUserCardHP1 = handle32.handle.ValueComponent.actorHp;
                PoolObjHandle<ActorRoot> handle33 = list4[0];
                int num12 = handle33.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue;
                stSecureInfoStartReq.iSvrUserCardATN1 = num12;
                PoolObjHandle<ActorRoot> handle34 = list4[0];
                num12 = handle34.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT].totalValue;
                stSecureInfoStartReq.iSvrUserCardINT1 = num12;
                PoolObjHandle<ActorRoot> handle35 = list4[0];
                num12 = handle35.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MOVESPD].totalValue;
                stSecureInfoStartReq.iSvrUserCardSpeed1 = num12;
                PoolObjHandle<ActorRoot> handle36 = list4[0];
                num12 = handle36.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT].totalValue;
                stSecureInfoStartReq.iSvrUserCardPhyDef1 = num12;
                PoolObjHandle<ActorRoot> handle37 = list4[0];
                num12 = handle37.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT].totalValue;
                stSecureInfoStartReq.iSvrUserCardSpellDef1 = num12;
            }
            if (list4.Count > 1)
            {
                PoolObjHandle<ActorRoot> handle38 = list4[1];
                stSecureInfoStartReq.iSvrUserCardID2 = handle38.handle.TheActorMeta.ConfigId;
                PoolObjHandle<ActorRoot> handle39 = list4[1];
                stSecureInfoStartReq.iSvrUserCardHP2 = handle39.handle.ValueComponent.actorHp;
                PoolObjHandle<ActorRoot> handle40 = list4[1];
                int num13 = handle40.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue;
                stSecureInfoStartReq.iSvrUserCardATN2 = num13;
                PoolObjHandle<ActorRoot> handle41 = list4[1];
                num13 = handle41.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT].totalValue;
                stSecureInfoStartReq.iSvrUserCardINT2 = num13;
                PoolObjHandle<ActorRoot> handle42 = list4[1];
                num13 = handle42.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MOVESPD].totalValue;
                stSecureInfoStartReq.iSvrUserCardSpeed2 = num13;
                PoolObjHandle<ActorRoot> handle43 = list4[1];
                num13 = handle43.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT].totalValue;
                stSecureInfoStartReq.iSvrUserCardPhyDef2 = num13;
                PoolObjHandle<ActorRoot> handle44 = list4[1];
                num13 = handle44.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT].totalValue;
                stSecureInfoStartReq.iSvrUserCardSpellDef2 = num13;
            }
            if (list4.Count > 2)
            {
                PoolObjHandle<ActorRoot> handle45 = list4[2];
                stSecureInfoStartReq.iSvrUserCardID3 = handle45.handle.TheActorMeta.ConfigId;
                PoolObjHandle<ActorRoot> handle46 = list4[2];
                stSecureInfoStartReq.iSvrUserCardHP3 = handle46.handle.ValueComponent.actorHp;
                PoolObjHandle<ActorRoot> handle47 = list4[2];
                int num14 = handle47.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue;
                stSecureInfoStartReq.iSvrUserCardATN3 = num14;
                PoolObjHandle<ActorRoot> handle48 = list4[2];
                num14 = handle48.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT].totalValue;
                stSecureInfoStartReq.iSvrUserCardINT3 = num14;
                PoolObjHandle<ActorRoot> handle49 = list4[2];
                num14 = handle49.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MOVESPD].totalValue;
                stSecureInfoStartReq.iSvrUserCardSpeed3 = num14;
                PoolObjHandle<ActorRoot> handle50 = list4[2];
                num14 = handle50.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT].totalValue;
                stSecureInfoStartReq.iSvrUserCardPhyDef3 = num14;
                PoolObjHandle<ActorRoot> handle51 = list4[2];
                num14 = handle51.handle.ValueComponent.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT].totalValue;
                stSecureInfoStartReq.iSvrUserCardSpellDef3 = num14;
            }
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        public void SetupMap(MapWrapper map)
        {
            this.mapLogic = map;
            if (this.mapLogic != null)
            {
                this.battleTaskSys.Initial("Assets.Scripts.GameLogic", GameDataMgr.gameTaskDatabin, GameDataMgr.gameTaskGroupDatabin, this.mapLogic.ActionHelper);
            }
        }

        public void SetupSoldier(SoldierControl soldier)
        {
            this.soldierCtrl = soldier;
        }

        public void ShowWinLose(bool bWin)
        {
            if ((Singleton<CBattleSystem>.GetInstance().m_FormScript != null) && Singleton<BattleLogic>.GetInstance().GetCurLvelContext().isPVPLevel)
            {
                Singleton<CBattleSystem>.GetInstance().ShowWinLosePanel(bWin);
            }
        }

        public void SingleReqLoseGame()
        {
            if ((Singleton<CBattleSystem>.GetInstance().m_FormScript != null) && Singleton<BattleLogic>.GetInstance().GetCurLvelContext().isPVPLevel)
            {
                Singleton<WinLose>.GetInstance().LastSingleGameWin = false;
            }
            Singleton<LobbyLogic>.GetInstance().ReqSingleGameFinish(true, false);
            this.UnRegistEvent();
        }

        private void SpawnMapBuffs()
        {
            SLevelContext curLvelContext = this.GetCurLvelContext();
            if ((curLvelContext != null) && (curLvelContext.MapBuffs != null))
            {
                for (int i = 0; i < curLvelContext.MapBuffs.Length; i++)
                {
                    ResDT_MapBuff buff = curLvelContext.MapBuffs[i];
                    if (buff.dwID == 0)
                    {
                        break;
                    }
                    List<PoolObjHandle<ActorRoot>> heroActors = Singleton<GameObjMgr>.GetInstance().HeroActors;
                    for (int j = 0; j < heroActors.Count; j++)
                    {
                        PoolObjHandle<ActorRoot> inTargetActor = heroActors[j];
                        if ((((((((int) 1) << inTargetActor.handle.TheActorMeta.ActorCamp) & buff.bCamp) > 0) && ((buff.bHeroType == 0) || (inTargetActor.handle.TheStaticData.TheHeroOnlyInfo.HeroCapability == buff.bHeroType))) && ((buff.bHeroDamageType == 0) || (inTargetActor.handle.TheStaticData.TheHeroOnlyInfo.HeroDamageType == buff.bHeroDamageType))) && ((buff.bHeroAttackType == 0) || (inTargetActor.handle.TheStaticData.TheHeroOnlyInfo.HeroAttackType == buff.bHeroAttackType)))
                        {
                            inTargetActor.handle.SkillControl.SpawnBuff(inTargetActor, new SkillUseContext(), (int) buff.dwID, false);
                        }
                    }
                }
            }
        }

        public void StartFightMultiGame()
        {
            this.isWaitMultiStart = false;
            Singleton<FrameSynchr>.instance.SetSynchrRunning(true);
            Singleton<GameReplayModule>.GetInstance().BattleStart();
            Singleton<BattleLogic>.GetInstance().DoBattleStart();
        }

        public void UnRegistEvent()
        {
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_FightOver, new RefAction<DefaultGameEventParam>(this.onFightOver));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_GameEnd, new RefAction<DefaultGameEventParam>(this.onGameEnd));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.onActorDead));
            Singleton<GameEventSys>.instance.RmvEventHandler<HurtEventResultInfo>(GameEventDef.Event_ActorDamage, new RefAction<HurtEventResultInfo>(this.OnActorDamage));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_FightPrepareFin, new RefAction<DefaultGameEventParam>(this.OnFightPrepareFin));
            if (this.incomeCtrl != null)
            {
                this.incomeCtrl.uninit();
            }
            if (this.valAdjustCtrl != null)
            {
                this.valAdjustCtrl.UnInit();
            }
        }

        private void UpdateDragonSpawnUI(int delta)
        {
            Singleton<CBattleSystem>.GetInstance().OnUpdateDragonUI(delta);
        }

        public void UpdateLogic(int delta)
        {
            if (this.isRuning)
            {
                this.m_fAveFPS = GameFramework.m_fFps + this.m_fAveFPS;
                this.m_fpsCount += 1L;
                if (GameFramework.m_fFps <= 10f)
                {
                    this.m_fpsCunt10 += 1L;
                }
                else if ((GameFramework.m_fFps <= 18f) && (GameFramework.m_fFps > 10f))
                {
                    this.m_fpsCunt18 += 1L;
                }
                if ((Time.time - this.m_FpsTimeBegin) > 5f)
                {
                    this.m_FpsTimeBegin = Time.time;
                    if (this.m_LastFps == 0f)
                    {
                        this.m_LastFps = GameFramework.m_fFps;
                    }
                    if (Mathf.Abs((float) (this.m_LastFps - GameFramework.m_fFps)) > 10f)
                    {
                        this.m_Ab_FPS_time = Singleton<FrameSynchr>.instance.LogicFrameTick * 0.001f;
                        this.m_Abnormal_FPS_Count++;
                        this.m_LastFps = GameFramework.m_fFps;
                    }
                }
                this.horizon.UpdateLogic(delta);
                if (this.mapLogic != null)
                {
                    this.mapLogic.UpdateLogic(delta);
                }
                if (this.soldierCtrl != null)
                {
                    this.soldierCtrl.UpdateLogic(delta);
                }
                this.UpdateDragonSpawnUI(delta);
                this.hostPlayerLogic.UpdateLogic(delta);
                if (this.dynamicProperty != null)
                {
                    this.dynamicProperty.UpdateLogic(delta);
                }
                if (this.m_globalTrigger != null)
                {
                    this.m_globalTrigger.UpdateLogic(delta);
                }
                Singleton<CBattleSystem>.GetInstance().UpdateLogic(delta);
                Singleton<BattleStatistic>.instance.UpdateLogic(delta);
                this.DynamicCheckQualitySetting(delta);
            }
        }

        public int DragonId
        {
            get
            {
                if (this.m_dragonSpawn != null)
                {
                    return this.m_dragonSpawn.TheActorsMeta[0].ConfigId;
                }
                return 0;
            }
        }

        public GlobalTrigger m_globalTrigger { get; private set; }
    }
}

