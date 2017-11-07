namespace Assets.Scripts.GameLogic.DataCenter
{
    using Assets.Scripts.GameLogic.GameKernal;
    using CSProtocol;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class ActorServerDataProvider : ActorDataProviderBase
    {
        private readonly DictionaryView<uint, ListView<COMDT_CHOICEHERO>> _serverCachedInfo = new DictionaryView<uint, ListView<COMDT_CHOICEHERO>>();

        public void AddHeroServerInfo(uint playerId, COMDT_CHOICEHERO serverHeroInfo)
        {
            if ((serverHeroInfo != null) && (serverHeroInfo.stBaseInfo.stCommonInfo.dwHeroID != 0))
            {
                ListView<COMDT_CHOICEHERO> view = null;
                if (!this._serverCachedInfo.ContainsKey(playerId))
                {
                    this._serverCachedInfo.Add(playerId, new ListView<COMDT_CHOICEHERO>());
                }
                if (this._serverCachedInfo.TryGetValue(playerId, out view))
                {
                    this.ApplyExtraInfoRule(playerId, serverHeroInfo);
                    view.Add(serverHeroInfo);
                }
            }
        }

        internal void ApplyExtraInfoRule(uint playerId, COMDT_CHOICEHERO serverHeroInfo)
        {
            if (serverHeroInfo.stHeroExtral.iHeroPos == 0)
            {
                Player player = Singleton<GamePlayerCenter>.instance.GetPlayer(playerId);
                int campPos = player.CampPos;
                if (campPos != 0)
                {
                    serverHeroInfo.stHeroExtral.iHeroPos = campPos;
                }
                else
                {
                    int heroTeamPosIndex = player.GetHeroTeamPosIndex(serverHeroInfo.stBaseInfo.stCommonInfo.dwHeroID);
                    if (heroTeamPosIndex > 0)
                    {
                        ListView<COMDT_CHOICEHERO> view = null;
                        bool flag = true;
                        if (this._serverCachedInfo.TryGetValue(playerId, out view))
                        {
                            for (int i = heroTeamPosIndex - 1; i >= 0; i--)
                            {
                                if (view[i].stHeroExtral.iHeroPos != i)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            serverHeroInfo.stHeroExtral.iHeroPos = heroTeamPosIndex;
                        }
                    }
                }
            }
        }

        public void ClearHeroServerInfo()
        {
            this._serverCachedInfo.Clear();
        }

        internal bool ConvertServerHeroEquipInfo(ref ActorServerEquipData serverData, ActorEquiplSlot equipSlot, COMDT_CHOICEHERO serverHeroInfo)
        {
            if (equipSlot >= ActorEquiplSlot.SlotMaxCount)
            {
                return false;
            }
            serverData.EquipSlot = equipSlot;
            return true;
        }

        internal void ConvertServerHeroInfo(ref ActorServerData serverData, COMDT_CHOICEHERO serverHeroInfo)
        {
            if (serverHeroInfo != null)
            {
                if ((serverHeroInfo.stBaseInfo != null) && (serverHeroInfo.stBaseInfo.stCommonInfo != null))
                {
                    serverData.Exp = serverHeroInfo.stBaseInfo.stCommonInfo.dwExp;
                    serverData.Level = serverHeroInfo.stBaseInfo.stCommonInfo.wLevel;
                    serverData.Star = serverHeroInfo.stBaseInfo.stCommonInfo.wStar;
                    if (serverHeroInfo.stBaseInfo.stCommonInfo.stQuality != null)
                    {
                        serverData.TheQualityInfo.Quality = serverHeroInfo.stBaseInfo.stCommonInfo.stQuality.wQuality;
                        serverData.TheQualityInfo.SubQuality = serverHeroInfo.stBaseInfo.stCommonInfo.stQuality.wSubQuality;
                    }
                    if (serverHeroInfo.stBaseInfo.stCommonInfo.stProficiency != null)
                    {
                        serverData.TheProficiencyInfo.Level = serverHeroInfo.stBaseInfo.stCommonInfo.stProficiency.bLv;
                        serverData.TheProficiencyInfo.Proficiency = serverHeroInfo.stBaseInfo.stCommonInfo.stProficiency.dwProficiency;
                    }
                    serverData.SkinId = serverHeroInfo.stBaseInfo.stCommonInfo.wSkinID;
                }
                if (serverHeroInfo.stBurningInfo != null)
                {
                    serverData.TheBurnInfo.HeroRemainingHp = serverHeroInfo.stBurningInfo.dwBloodTTH;
                    serverData.TheBurnInfo.IsDead = serverHeroInfo.stBurningInfo.bIsDead != 0;
                }
                if (serverHeroInfo.stHeroExtral != null)
                {
                    serverData.TheExtraInfo.BornPointIndex = serverHeroInfo.stHeroExtral.iHeroPos;
                }
                serverData.SymbolID = new uint[serverHeroInfo.SymbolID.Length];
                for (int i = 0; i < serverHeroInfo.SymbolID.Length; i++)
                {
                    serverData.SymbolID[i] = serverHeroInfo.SymbolID[i];
                }
                serverData.m_customRecommendEquips = new ushort[serverHeroInfo.HeroEquipList.Length];
                for (int j = 0; j < serverHeroInfo.HeroEquipList.Length; j++)
                {
                    serverData.m_customRecommendEquips[j] = (ushort) serverHeroInfo.HeroEquipList[j];
                }
            }
        }

        internal bool ConvertServerHeroRuneInfo(ref ActorServerRuneData serverData, ActorRunelSlot runeSlot, COMDT_CHOICEHERO serverHeroInfo)
        {
            if (runeSlot >= ActorRunelSlot.SlotMaxCount)
            {
                return false;
            }
            serverData.RuneSlot = runeSlot;
            if (serverHeroInfo != null)
            {
                serverData.RuneId = serverHeroInfo.SymbolID[(int) ((IntPtr) runeSlot)];
            }
            return true;
        }

        internal bool ConvertServerHeroSkillInfo(ref ActorServerSkillData serverData, ActorSkillSlot skillSlot, COMDT_CHOICEHERO serverHeroInfo)
        {
            if (skillSlot >= ActorSkillSlot.SlotMaxCount)
            {
                return false;
            }
            serverData.SkillSlot = skillSlot;
            serverData.IsUnlocked = serverHeroInfo.stBaseInfo.stCommonInfo.stSkill.astSkillInfo[(int) ((IntPtr) skillSlot)].bUnlocked != 0;
            serverData.SkillLevel = serverHeroInfo.stBaseInfo.stCommonInfo.stSkill.astSkillInfo[(int) ((IntPtr) skillSlot)].wLevel;
            return true;
        }

        protected COMDT_CHOICEHERO Find(ListView<COMDT_CHOICEHERO> InSearchList, Predicate<COMDT_CHOICEHERO> InPredicate)
        {
            for (int i = 0; i < InSearchList.Count; i++)
            {
                if (InPredicate(InSearchList[i]))
                {
                    return InSearchList[i];
                }
            }
            return null;
        }

        public override bool GetActorServerCommonSkillData(ref ActorMeta actorMeta, out uint skillID)
        {
            <GetActorServerCommonSkillData>c__AnonStorey2B storeyb = new <GetActorServerCommonSkillData>c__AnonStorey2B();
            skillID = 0;
            ListView<COMDT_CHOICEHERO> view = null;
            if (this._serverCachedInfo.TryGetValue(actorMeta.PlayerId, out view))
            {
                storeyb.configId = actorMeta.ConfigId;
                COMDT_CHOICEHERO comdt_choicehero = this.Find(view, new Predicate<COMDT_CHOICEHERO>(storeyb.<>m__F));
                if (comdt_choicehero != null)
                {
                    skillID = comdt_choicehero.stBaseInfo.stCommonInfo.stSkill.dwSelSkillID;
                    return true;
                }
            }
            return false;
        }

        public override bool GetActorServerData(ref ActorMeta actorMeta, ref ActorServerData actorData)
        {
            <GetActorServerData>c__AnonStorey29 storey = new <GetActorServerData>c__AnonStorey29();
            actorData.TheActorMeta = actorMeta;
            ListView<COMDT_CHOICEHERO> view = null;
            if (!this._serverCachedInfo.TryGetValue(actorMeta.PlayerId, out view))
            {
                return false;
            }
            storey.configId = actorMeta.ConfigId;
            this.ConvertServerHeroInfo(ref actorData, this.Find(view, new Predicate<COMDT_CHOICEHERO>(storey.<>m__D)));
            return true;
        }

        public override bool GetActorServerEquipData(ref ActorMeta actorMeta, ActorEquiplSlot equipSlot, ref ActorServerEquipData equipData)
        {
            <GetActorServerEquipData>c__AnonStorey2C storeyc = new <GetActorServerEquipData>c__AnonStorey2C();
            equipData.TheActorMeta = actorMeta;
            ListView<COMDT_CHOICEHERO> view = null;
            if (!this._serverCachedInfo.TryGetValue(actorMeta.PlayerId, out view))
            {
                return false;
            }
            storeyc.configId = actorMeta.ConfigId;
            return this.ConvertServerHeroEquipInfo(ref equipData, equipSlot, this.Find(view, new Predicate<COMDT_CHOICEHERO>(storeyc.<>m__10)));
        }

        public override bool GetActorServerRuneData(ref ActorMeta actorMeta, ActorRunelSlot runeSlot, ref ActorServerRuneData runeData)
        {
            <GetActorServerRuneData>c__AnonStorey2D storeyd = new <GetActorServerRuneData>c__AnonStorey2D();
            runeData.TheActorMeta = actorMeta;
            ListView<COMDT_CHOICEHERO> view = null;
            if (!this._serverCachedInfo.TryGetValue(actorMeta.PlayerId, out view))
            {
                return false;
            }
            storeyd.configId = actorMeta.ConfigId;
            return this.ConvertServerHeroRuneInfo(ref runeData, runeSlot, this.Find(view, new Predicate<COMDT_CHOICEHERO>(storeyd.<>m__11)));
        }

        public override bool GetActorServerSkillData(ref ActorMeta actorMeta, ActorSkillSlot skillSlot, ref ActorServerSkillData skillData)
        {
            <GetActorServerSkillData>c__AnonStorey2A storeya = new <GetActorServerSkillData>c__AnonStorey2A();
            skillData.TheActorMeta = actorMeta;
            ListView<COMDT_CHOICEHERO> view = null;
            if (!this._serverCachedInfo.TryGetValue(actorMeta.PlayerId, out view))
            {
                return false;
            }
            storeya.configId = actorMeta.ConfigId;
            return this.ConvertServerHeroSkillInfo(ref skillData, skillSlot, this.Find(view, new Predicate<COMDT_CHOICEHERO>(storeya.<>m__E)));
        }

        [CompilerGenerated]
        private sealed class <GetActorServerCommonSkillData>c__AnonStorey2B
        {
            internal int configId;

            internal bool <>m__F(COMDT_CHOICEHERO hero)
            {
                return (hero.stBaseInfo.stCommonInfo.dwHeroID == this.configId);
            }
        }

        [CompilerGenerated]
        private sealed class <GetActorServerData>c__AnonStorey29
        {
            internal int configId;

            internal bool <>m__D(COMDT_CHOICEHERO hero)
            {
                return (hero.stBaseInfo.stCommonInfo.dwHeroID == this.configId);
            }
        }

        [CompilerGenerated]
        private sealed class <GetActorServerEquipData>c__AnonStorey2C
        {
            internal int configId;

            internal bool <>m__10(COMDT_CHOICEHERO hero)
            {
                return (hero.stBaseInfo.stCommonInfo.dwHeroID == this.configId);
            }
        }

        [CompilerGenerated]
        private sealed class <GetActorServerRuneData>c__AnonStorey2D
        {
            internal int configId;

            internal bool <>m__11(COMDT_CHOICEHERO hero)
            {
                return (hero.stBaseInfo.stCommonInfo.dwHeroID == this.configId);
            }
        }

        [CompilerGenerated]
        private sealed class <GetActorServerSkillData>c__AnonStorey2A
        {
            internal int configId;

            internal bool <>m__E(COMDT_CHOICEHERO hero)
            {
                return (hero.stBaseInfo.stCommonInfo.dwHeroID == this.configId);
            }
        }
    }
}

