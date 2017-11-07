namespace Assets.Scripts.GameLogic.GameKernal
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.GameLogic.DataCenter;
    using Assets.Scripts.GameSystem;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class Player
    {
        private readonly List<PoolObjHandle<ActorRoot>> _heroes = new List<PoolObjHandle<ActorRoot>>();
        private readonly List<uint> _heroIds = new List<uint>();
        private CrypticInt32 _level = 1;
        public SelectEnemyType AttackTargetMode = SelectEnemyType.SelectLowHp;
        public bool bCommonAttackLockMode = true;
        private bool bUseAdvanceCommonAttack = true;
        public int CampPos;
        public PoolObjHandle<ActorRoot> Captain;
        public uint CaptainId = 0;
        public bool Computer;
        public uint GradeOfRank;
        public int HeadIconId;
        public bool isGM;
        public int LogicWrold;
        public bool m_bMoved;
        public string Name;
        public string OpenId;
        public COM_PLAYERCAMP PlayerCamp;
        public uint PlayerId;
        public ulong PlayerUId;
        private OperateMode useOperateMode;
        public uint VipLv;

        public Player()
        {
            this.Level = 1;
        }

        public void AddHero(uint heroCfgId)
        {
            if (((heroCfgId != 0) && !this._heroIds.Contains(heroCfgId)) && (this._heroIds.Count < 3))
            {
                this._heroIds.Add(heroCfgId);
                if (this.Computer && string.IsNullOrEmpty(this.Name))
                {
                    ActorStaticData actorData = new ActorStaticData();
                    ActorMeta actorMeta = new ActorMeta();
                    IGameActorDataProvider actorDataProvider = Singleton<ActorDataCenter>.instance.GetActorDataProvider(GameActorDataProviderType.StaticBattleDataProvider);
                    actorMeta.PlayerId = this.PlayerId;
                    actorMeta.ActorType = ActorTypeDef.Actor_Type_Hero;
                    actorMeta.ConfigId = (int) heroCfgId;
                    string str = !actorDataProvider.GetActorStaticData(ref actorMeta, ref actorData) ? null : actorData.TheResInfo.Name;
                    this.Name = string.Format("{0}[{1}]", str, Singleton<CTextManager>.GetInstance().GetText("PVP_NPC"));
                }
                if (this._heroIds.Count == 1)
                {
                    this.CaptainId = heroCfgId;
                }
            }
        }

        public void ClearHeroes()
        {
            this._heroIds.Clear();
            this._heroes.Clear();
            this.CaptainId = 0;
            this.Captain = new PoolObjHandle<ActorRoot>();
        }

        public void ConnectHeroActorRoot(ref PoolObjHandle<ActorRoot> hero)
        {
            if ((hero != 0) && this._heroIds.Contains((uint) hero.handle.TheActorMeta.ConfigId))
            {
                this._heroes.Add(hero);
                if (hero.handle.TheActorMeta.ConfigId == this.CaptainId)
                {
                    this.Captain = hero;
                }
            }
        }

        public int GetAllHeroCombatEft()
        {
            int num = 0;
            for (int i = 0; i < this._heroes.Count; i++)
            {
                num += GetHeroCombatEft(this._heroes[i]);
            }
            return num;
        }

        public ReadonlyContext<PoolObjHandle<ActorRoot>> GetAllHeroes()
        {
            return new ReadonlyContext<PoolObjHandle<ActorRoot>>(this._heroes);
        }

        public ReadonlyContext<uint> GetAllHeroIds()
        {
            return new ReadonlyContext<uint>(this._heroIds);
        }

        private static int GetHeroCombatEft(PoolObjHandle<ActorRoot> actor)
        {
            if (actor == 0)
            {
                return 0;
            }
            int num = 0;
            IGameActorDataProvider actorDataProvider = Singleton<ActorDataCenter>.instance.GetActorDataProvider(GameActorDataProviderType.ServerDataProvider);
            ActorServerData actorData = new ActorServerData();
            if (actorDataProvider.GetActorServerData(ref actor.handle.TheActorMeta, ref actorData))
            {
                num += CHeroInfo.GetCombatEftByStarLevel((int) actorData.Level, (int) actorData.Star);
                num += CSkinInfo.GetCombatEft((uint) actorData.TheActorMeta.ConfigId, actorData.SkinId);
                ActorServerRuneData runeData = new ActorServerRuneData();
                for (int i = 0; i < 30; i++)
                {
                    if (actorDataProvider.GetActorServerRuneData(ref actor.handle.TheActorMeta, (ActorRunelSlot) i, ref runeData))
                    {
                        ResSymbolInfo dataByKey = GameDataMgr.symbolInfoDatabin.GetDataByKey(runeData.RuneId);
                        if (dataByKey != null)
                        {
                            num += dataByKey.iCombatEft;
                        }
                    }
                }
            }
            return num;
        }

        public int GetHeroTeamPosIndex(uint heroCfgId)
        {
            return this._heroIds.IndexOf(heroCfgId);
        }

        public OperateMode GetOperateMode()
        {
            return this.useOperateMode;
        }

        public bool IsAllHeroesDead()
        {
            for (int i = 0; i < this._heroes.Count; i++)
            {
                PoolObjHandle<ActorRoot> handle = this._heroes[i];
                if (!handle.handle.ActorControl.IsDeadState)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsAtMyTeam(ref ActorMeta actorMeta)
        {
            return ((actorMeta.PlayerId == this.PlayerId) && this._heroIds.Contains((uint) actorMeta.ConfigId));
        }

        public bool IsMyTeamOutOfBattle()
        {
            for (int i = 0; i < this._heroes.Count; i++)
            {
                PoolObjHandle<ActorRoot> handle = this._heroes[i];
                if (handle.handle.ActorControl.IsInBattle)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsUseAdvanceCommonAttack()
        {
            return this.bUseAdvanceCommonAttack;
        }

        public void SetCaptain(uint configId)
        {
            <SetCaptain>c__AnonStorey31 storey = new <SetCaptain>c__AnonStorey31 {
                configId = configId
            };
            this.CaptainId = storey.configId;
            this.Captain = this._heroes.Find(new Predicate<PoolObjHandle<ActorRoot>>(storey.<>m__1C));
        }

        public void SetOperateMode(OperateMode _mode)
        {
            if (_mode == OperateMode.DefaultMode)
            {
                if ((this.Captain != 0) && (this.Captain.handle.LockTargetAttackModeControl != null))
                {
                    this.Captain.handle.LockTargetAttackModeControl.ClearTargetID();
                }
            }
            else if ((this.Captain != 0) && (this.Captain.handle.DefaultAttackModeControl != null))
            {
                this.Captain.handle.DefaultAttackModeControl.ClearCommonAttackTarget();
            }
            this.useOperateMode = _mode;
            Singleton<EventRouter>.GetInstance().BroadCastEvent<CommonAttactType>(EventID.GAME_SETTING_COMMONATTACK_TYPE_CHANGE, (CommonAttactType) _mode);
        }

        public void SetUseAdvanceCommonAttack(bool bFlag)
        {
            this.bUseAdvanceCommonAttack = bFlag;
        }

        public int heroCount
        {
            get
            {
                return this._heroIds.Count;
            }
        }

        public int Level
        {
            get
            {
                return (int) this._level;
            }
            set
            {
                this._level = value;
            }
        }

        [CompilerGenerated]
        private sealed class <SetCaptain>c__AnonStorey31
        {
            internal uint configId;

            internal bool <>m__1C(PoolObjHandle<ActorRoot> hero)
            {
                return (hero.handle.TheActorMeta.ConfigId == this.configId);
            }
        }
    }
}

