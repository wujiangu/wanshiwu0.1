namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic.GameKernal;
    using CSProtocol;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class CPlayerKDAStat
    {
        private DictionaryView<uint, PlayerKDA> m_PlayerKDA = new DictionaryView<uint, PlayerKDA>();

        public void DumpDebugInfo()
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                Debug.Log(string.Format("PlayerKDA Id {0}", current.Key));
            }
        }

        public void GenerateStatData()
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            HeroKDA okda = null;
            HeroKDA okda2 = null;
            HeroKDA okda3 = null;
            HeroKDA okda4 = null;
            HeroKDA okda5 = null;
            HeroKDA okda6 = null;
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    if (okda == null)
                    {
                        okda = enumerator2.Current;
                    }
                    if (okda.numKill < enumerator2.Current.numKill)
                    {
                        okda = enumerator2.Current;
                    }
                    if (okda2 == null)
                    {
                        okda2 = enumerator2.Current;
                    }
                    if (okda2.hurtToEnemy < enumerator2.Current.hurtToEnemy)
                    {
                        okda2 = enumerator2.Current;
                    }
                    if (okda3 == null)
                    {
                        okda3 = enumerator2.Current;
                    }
                    if (okda3.hurtTakenByEnemy < enumerator2.Current.hurtTakenByEnemy)
                    {
                        okda3 = enumerator2.Current;
                    }
                    if (okda4 == null)
                    {
                        okda4 = enumerator2.Current;
                    }
                    if (okda4.numAssist < enumerator2.Current.numAssist)
                    {
                        okda4 = enumerator2.Current;
                    }
                    if (okda5 == null)
                    {
                        okda5 = enumerator2.Current;
                    }
                    if (okda5.TotalCoin < enumerator2.Current.TotalCoin)
                    {
                        okda5 = enumerator2.Current;
                    }
                    if (okda6 == null)
                    {
                        okda6 = enumerator2.Current;
                    }
                    if (okda6.numKillOrgan < enumerator2.Current.numKillOrgan)
                    {
                        okda6 = enumerator2.Current;
                    }
                }
            }
            if ((okda != null) && (okda.numKill >= 5))
            {
                okda.bKillMost = true;
            }
            if ((okda2 != null) && (okda2.hurtToEnemy > 0))
            {
                okda2.bHurtMost = true;
            }
            if ((okda3 != null) && (okda3.hurtTakenByEnemy > 0))
            {
                okda3.bHurtTakenMost = true;
            }
            if ((okda4 != null) && (okda4.numAssist >= 10))
            {
                okda4.bAsssistMost = true;
            }
            if ((okda5 != null) && (okda4.TotalCoin > 0))
            {
                okda5.bGetCoinMost = true;
            }
            if ((okda6 != null) && (okda6.numKillOrgan > 0))
            {
                okda6.bKillOrganMost = true;
            }
        }

        public DictionaryView<uint, PlayerKDA>.Enumerator GetEnumerator()
        {
            return this.m_PlayerKDA.GetEnumerator();
        }

        public PlayerKDA GetHostKDA()
        {
            PlayerKDA rkda;
            this.m_PlayerKDA.TryGetValue(Singleton<GamePlayerCenter>.instance.HostPlayerId, out rkda);
            return rkda;
        }

        public PlayerKDA GetPlayerKDA(uint playerId)
        {
            PlayerKDA rkda;
            this.m_PlayerKDA.TryGetValue(playerId, out rkda);
            return rkda;
        }

        public int GetTeamAssistNum(COM_PLAYERCAMP camp)
        {
            int num = 0;
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                if (current.Value.PlayerCamp == camp)
                {
                    KeyValuePair<uint, PlayerKDA> pair2 = enumerator.Current;
                    num += pair2.Value.numAssist;
                }
            }
            return num;
        }

        public int GetTeamDeadNum(COM_PLAYERCAMP camp)
        {
            int num = 0;
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                if (current.Value.PlayerCamp == camp)
                {
                    KeyValuePair<uint, PlayerKDA> pair2 = enumerator.Current;
                    num += pair2.Value.numDead;
                }
            }
            return num;
        }

        public float GetTeamKDA(COM_PLAYERCAMP camp)
        {
            float num = 0f;
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                if (current.Value.PlayerCamp == camp)
                {
                    KeyValuePair<uint, PlayerKDA> pair2 = enumerator.Current;
                    num += pair2.Value.KDAValue;
                }
            }
            return num;
        }

        public int GetTeamKillNum(COM_PLAYERCAMP camp)
        {
            int num = 0;
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                if (current.Value.PlayerCamp == camp)
                {
                    KeyValuePair<uint, PlayerKDA> pair2 = enumerator.Current;
                    num += pair2.Value.numKill;
                }
            }
            return num;
        }

        private void initialize()
        {
            List<Player>.Enumerator enumerator = Singleton<GamePlayerCenter>.instance.GetAllPlayers().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Player current = enumerator.Current;
                if (current != null)
                {
                    PlayerKDA rkda = new PlayerKDA();
                    rkda.initialize(current);
                    this.m_PlayerKDA.Add(current.PlayerId, rkda);
                }
            }
            Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, sbyte, uint>("HeroLearnTalent", new Action<PoolObjHandle<ActorRoot>, sbyte, uint>(this, (IntPtr) this.OnHeroLearnTalent));
            Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int>("HeroSoulLevelChange", new Action<PoolObjHandle<ActorRoot>, int>(this, (IntPtr) this.OnHeroSoulLvlChange));
            Singleton<EventRouter>.GetInstance().AddEventHandler<uint, stEquipInfo[]>("HeroEquipInBattleChange", new Action<uint, stEquipInfo[]>(this, (IntPtr) this.OnHeroBattleEquipChange));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.OnActorDead));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_GameEnd, new RefAction<DefaultGameEventParam>(this.OnGameEnd));
            Singleton<GameEventSys>.instance.AddEventHandler<HurtEventResultInfo>(GameEventDef.Event_ActorDamage, new RefAction<HurtEventResultInfo>(this.OnActorDamage));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_DoubleKill, new RefAction<DefaultGameEventParam>(this.OnActorDoubleKill));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_TripleKill, new RefAction<DefaultGameEventParam>(this.OnActorTripleKill));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_QuataryKill, new RefAction<DefaultGameEventParam>(this.OnActorQuataryKill));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_PentaKill, new RefAction<DefaultGameEventParam>(this.OnActorPentaKill));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_Odyssey, new RefAction<DefaultGameEventParam>(this.OnActorOdyssey));
            Singleton<GameEventSys>.instance.AddEventHandler<SkillChooseTargetEventParam>(GameEventDef.Event_ActorBeChosenAsTarget, new RefAction<SkillChooseTargetEventParam>(this.OnActorBeChosen));
            Singleton<GameEventSys>.instance.AddEventHandler<SkillChooseTargetEventParam>(GameEventDef.Event_HitTrigger, new RefAction<SkillChooseTargetEventParam>(this.OnHitTrigger));
            Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int, int, bool>("HeroGoldCoinInBattleChange", new Action<PoolObjHandle<ActorRoot>, int, int, bool>(this, (IntPtr) this.OnActorBattleCoinChanged));
        }

        public void OnActorBattleCoinChanged(PoolObjHandle<ActorRoot> actor, int changeValue, int currentValue, bool isIncome)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    if (actor == enumerator2.Current.actorHero)
                    {
                        enumerator2.Current.OnActorBattleCoinChanged(actor, changeValue, currentValue, isIncome);
                        Singleton<EventRouter>.instance.BroadCastEvent(EventID.BATTLE_KDA_CHANGED);
                        return;
                    }
                }
            }
        }

        public void OnActorBeChosen(ref SkillChooseTargetEventParam prm)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.OnActorBeChosen(ref prm);
                }
            }
        }

        public void OnActorDamage(ref HurtEventResultInfo prm)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.OnActorDamage(ref prm);
                }
            }
        }

        public void OnActorDead(ref DefaultGameEventParam prm)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.OnActorDead(ref prm);
                }
            }
            if ((prm.src != 0) && (prm.src.handle.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero))
            {
                Singleton<EventRouter>.instance.BroadCastEvent(EventID.BATTLE_KDA_CHANGED);
            }
        }

        public void OnActorDoubleKill(ref DefaultGameEventParam prm)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    if ((prm.atker != 0) && object.ReferenceEquals(prm.atker.handle, enumerator2.Current.actorHero.handle))
                    {
                        enumerator2.Current.OnActorDoubleKill(ref prm);
                        return;
                    }
                }
            }
        }

        public void OnActorOdyssey(ref DefaultGameEventParam prm)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    if ((prm.atker != 0) && object.ReferenceEquals(prm.atker.handle, enumerator2.Current.actorHero.handle))
                    {
                        enumerator2.Current.OnActorOdyssey(ref prm);
                        return;
                    }
                }
            }
        }

        public void OnActorPentaKill(ref DefaultGameEventParam prm)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    if ((prm.atker != 0) && object.ReferenceEquals(prm.atker.handle, enumerator2.Current.actorHero.handle))
                    {
                        enumerator2.Current.OnActorPentaKill(ref prm);
                        return;
                    }
                }
            }
        }

        public void OnActorQuataryKill(ref DefaultGameEventParam prm)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    if ((prm.atker != 0) && object.ReferenceEquals(prm.atker.handle, enumerator2.Current.actorHero.handle))
                    {
                        enumerator2.Current.OnActorQuataryKill(ref prm);
                        return;
                    }
                }
            }
        }

        public void OnActorTripleKill(ref DefaultGameEventParam prm)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    if ((prm.atker != 0) && object.ReferenceEquals(prm.atker.handle, enumerator2.Current.actorHero.handle))
                    {
                        enumerator2.Current.OnActorTripleKill(ref prm);
                        return;
                    }
                }
            }
        }

        private void OnGameEnd(ref DefaultGameEventParam prm)
        {
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.OnActorDead));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_GameEnd, new RefAction<DefaultGameEventParam>(this.OnGameEnd));
            Singleton<GameEventSys>.instance.RmvEventHandler<HurtEventResultInfo>(GameEventDef.Event_ActorDamage, new RefAction<HurtEventResultInfo>(this.OnActorDamage));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_DoubleKill, new RefAction<DefaultGameEventParam>(this.OnActorDoubleKill));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_TripleKill, new RefAction<DefaultGameEventParam>(this.OnActorTripleKill));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_QuataryKill, new RefAction<DefaultGameEventParam>(this.OnActorQuataryKill));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_PentaKill, new RefAction<DefaultGameEventParam>(this.OnActorPentaKill));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_Odyssey, new RefAction<DefaultGameEventParam>(this.OnActorOdyssey));
            Singleton<GameEventSys>.instance.RmvEventHandler<SkillChooseTargetEventParam>(GameEventDef.Event_ActorBeChosenAsTarget, new RefAction<SkillChooseTargetEventParam>(this.OnActorBeChosen));
            Singleton<GameEventSys>.instance.RmvEventHandler<SkillChooseTargetEventParam>(GameEventDef.Event_HitTrigger, new RefAction<SkillChooseTargetEventParam>(this.OnHitTrigger));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int, int, bool>("HeroGoldCoinInBattleChange", new Action<PoolObjHandle<ActorRoot>, int, int, bool>(this, (IntPtr) this.OnActorBattleCoinChanged));
        }

        public void OnHeroBattleEquipChange(uint actorId, stEquipInfo[] equips)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    if ((enumerator2.Current != null) && (enumerator2.Current.actorHero.handle.ObjID == actorId))
                    {
                        equips.CopyTo(enumerator2.Current.Equips, 0);
                        Singleton<EventRouter>.instance.BroadCastEvent(EventID.BATTLE_KDA_CHANGED);
                        return;
                    }
                }
            }
        }

        public void OnHeroLearnTalent(PoolObjHandle<ActorRoot> hero, sbyte talentLevel, uint talentID)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    if (hero == enumerator2.Current.actorHero)
                    {
                        enumerator2.Current.TalentArr[(int) talentLevel].dwTalentID = talentID;
                        enumerator2.Current.TalentArr[(int) talentLevel].dwLearnLevel = (uint) hero.handle.ValueComponent.actorSoulLevel;
                        return;
                    }
                }
            }
        }

        public void OnHeroSoulLvlChange(PoolObjHandle<ActorRoot> hero, int level)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    if (hero == enumerator2.Current.actorHero)
                    {
                        enumerator2.Current.SoulLevel = Math.Max(level, 1);
                        Singleton<EventRouter>.instance.BroadCastEvent(EventID.BATTLE_KDA_CHANGED);
                        return;
                    }
                }
            }
        }

        public void OnHitTrigger(ref SkillChooseTargetEventParam prm)
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                IEnumerator<HeroKDA> enumerator2 = current.Value.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.OnHitTrigger(ref prm);
                }
            }
        }

        public void reset()
        {
            DictionaryView<uint, PlayerKDA>.Enumerator enumerator = this.m_PlayerKDA.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                current.Value.clear();
            }
            this.m_PlayerKDA.Clear();
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, sbyte, uint>("HeroLearnTalent", new Action<PoolObjHandle<ActorRoot>, sbyte, uint>(this, (IntPtr) this.OnHeroLearnTalent));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int>("HeroSoulLevelChange", new Action<PoolObjHandle<ActorRoot>, int>(this, (IntPtr) this.OnHeroSoulLvlChange));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<uint, stEquipInfo[]>("HeroEquipInBattleChange", new Action<uint, stEquipInfo[]>(this, (IntPtr) this.OnHeroBattleEquipChange));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.OnActorDead));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_GameEnd, new RefAction<DefaultGameEventParam>(this.OnGameEnd));
            Singleton<GameEventSys>.instance.RmvEventHandler<HurtEventResultInfo>(GameEventDef.Event_ActorDamage, new RefAction<HurtEventResultInfo>(this.OnActorDamage));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_DoubleKill, new RefAction<DefaultGameEventParam>(this.OnActorDoubleKill));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_TripleKill, new RefAction<DefaultGameEventParam>(this.OnActorTripleKill));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_QuataryKill, new RefAction<DefaultGameEventParam>(this.OnActorQuataryKill));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_PentaKill, new RefAction<DefaultGameEventParam>(this.OnActorPentaKill));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_Odyssey, new RefAction<DefaultGameEventParam>(this.OnActorOdyssey));
            Singleton<GameEventSys>.instance.RmvEventHandler<SkillChooseTargetEventParam>(GameEventDef.Event_ActorBeChosenAsTarget, new RefAction<SkillChooseTargetEventParam>(this.OnActorBeChosen));
            Singleton<GameEventSys>.instance.RmvEventHandler<SkillChooseTargetEventParam>(GameEventDef.Event_HitTrigger, new RefAction<SkillChooseTargetEventParam>(this.OnHitTrigger));
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int, int, bool>("HeroGoldCoinInBattleChange", new Action<PoolObjHandle<ActorRoot>, int, int, bool>(this, (IntPtr) this.OnActorBattleCoinChanged));
        }

        public void StartKDARecord()
        {
            this.reset();
            this.initialize();
        }
    }
}

