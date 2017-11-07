namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class CHeroInfo
    {
        public COMDT_HERO_WEARINFO[] astEquipWear;
        public ResHeroCfgInfo cfgInfo;
        public int m_awakeState;
        public uint m_awakeStepID;
        public uint m_experienceDeadLine;
        public bool m_isStepFinish;
        private static int m_maxProficiency;
        public uint m_Proficiency;
        public byte m_ProficiencyLV;
        public int m_selectPageIndex;
        public CSkinInfo m_skinInfo = new CSkinInfo();
        public byte[] m_talentBuyList;
        public PropertyHelper mActorValue = new PropertyHelper();
        public uint MaskBits;
        public CSkillData skillInfo;

        public void GetAdvancePropData(ref int[] propArr, ref int[] propPctArr, ref int[] propValAddArr, ref int[] propPctAddArr)
        {
            int index = 0;
            int num2 = 0x24;
            for (index = 0; index < num2; index++)
            {
                propArr[index] = 0;
                propPctArr[index] = 0;
                propValAddArr[index] = 0;
                propPctAddArr[index] = 0;
            }
            ResHeroAdvanceInfo heroAdvanceInfo = this.GetHeroAdvanceInfo();
            int num3 = 0;
            DebugHelper.Assert(heroAdvanceInfo != null);
            if (heroAdvanceInfo != null)
            {
                num3 = 5;
                propArr[num3] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP].totalValue - heroAdvanceInfo.iHpAddVal;
                propValAddArr[num3] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP].totalValue;
                num3 = 1;
                propArr[num3] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue - heroAdvanceInfo.iAtkAddVal;
                propValAddArr[num3] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue;
                num3 = 3;
                propArr[num3] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT].totalValue - heroAdvanceInfo.iDefAddVal;
                propValAddArr[num3] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT].totalValue;
                num3 = 2;
                propArr[num3] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT].totalValue - heroAdvanceInfo.iSpellAddVal;
                propValAddArr[num3] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT].totalValue;
                num3 = 4;
                propArr[num3] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT].totalValue - heroAdvanceInfo.iResistAddVal;
                propValAddArr[num3] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT].totalValue;
            }
        }

        public static int GetCombatByHeroQuality(uint heroID, int quality, int subQuality)
        {
            int iQuality = quality;
            int iSubQuality = subQuality;
            ResHeroAdvanceInfo info = null;
            bool flag = true;
            int num3 = 0;
            while (flag)
            {
                info = GetLastStepInfo(heroID, iQuality, iSubQuality);
                if (info != null)
                {
                    num3 += info.iCombatEft;
                    iQuality = info.iQuality;
                    iSubQuality = info.iSubQuality;
                }
                else
                {
                    flag = false;
                }
            }
            info = GetHeroAdvanceInfo(heroID, quality, subQuality);
            if (info != null)
            {
                num3 += info.iCombatEft;
            }
            return num3;
        }

        public int GetCombatEft()
        {
            DebugHelper.Assert(this.mActorValue != null, "GetCombatEft mActorValue is null");
            int combatEftByStarLevel = 0;
            if (this.mActorValue != null)
            {
                combatEftByStarLevel = GetCombatEftByStarLevel(this.mActorValue.actorLvl, this.mActorValue.actorStar);
            }
            int combatEft = CSkinInfo.GetCombatEft(this.cfgInfo.dwCfgID, this.m_skinInfo.GetWearSkinId());
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            DebugHelper.Assert(masterRoleInfo != null, "GetCombatEft master is null");
            int symbolPageEft = 0;
            if (masterRoleInfo != null)
            {
                symbolPageEft = masterRoleInfo.m_symbolInfo.GetSymbolPageEft(this.m_selectPageIndex);
            }
            return ((combatEftByStarLevel + combatEft) + symbolPageEft);
        }

        public static int GetCombatEftByStarLevel(int level, int star)
        {
            ResHeroLvlUpInfo dataByKey = GameDataMgr.heroLvlUpDatabin.GetDataByKey((uint) level);
            if (((dataByKey != null) && (star >= 1)) && (star <= 5))
            {
                return dataByKey.StarCombatEft[star - 1];
            }
            return 0;
        }

        public static int GetExperienceHeroOrSkinExtendDays(uint extendSeconds)
        {
            TimeSpan span = new TimeSpan((extendSeconds + 0xe10) * 0x989680L);
            return span.Days;
        }

        public static int GetExperienceHeroOrSkinValidDays(uint experienceDeadLine)
        {
            int num = ((int) experienceDeadLine) - Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().getCurrentTimeSinceLogin();
            TimeSpan span = new TimeSpan((num + 0xe10) * 0x989680L);
            return span.Days;
        }

        public static string GetFeatureStr(RES_HERO_JOB_FEATURE featureType)
        {
            string str = string.Empty;
            CTextManager instance = Singleton<CTextManager>.GetInstance();
            switch (featureType)
            {
                case RES_HERO_JOB_FEATURE.RES_JOB_FEATURE_DASH:
                    return instance.GetText("Hero_Job_Feature_Dash");

                case RES_HERO_JOB_FEATURE.RES_JOB_FEATURE_CONTROL:
                    return instance.GetText("Hero_Job_Feature_Control");

                case RES_HERO_JOB_FEATURE.RES_JOB_FEATURE_ACTIVE:
                    return instance.GetText("Hero_Job_Feature_Active");

                case RES_HERO_JOB_FEATURE.RES_JOB_FEATURE_SLAVE:
                    return instance.GetText("Hero_Job_Feature_Slave");

                case RES_HERO_JOB_FEATURE.RES_JOB_FEATURE_RECOVER:
                    return instance.GetText("Hero_Job_Feature_Recover");

                case RES_HERO_JOB_FEATURE.RES_JOB_FEATURE_HPSTEAL:
                    return instance.GetText("Hero_Job_Feature_HpSteal");

                case RES_HERO_JOB_FEATURE.RES_JOB_FEATURE_POKE:
                    return instance.GetText("Hero_Job_Feature_Poke");

                case RES_HERO_JOB_FEATURE.RES_JOB_FEATURE_BUFF:
                    return instance.GetText("Hero_Job_Feature_Buff");
            }
            return str;
        }

        public ResHeroAdvanceInfo GetHeroAdvanceInfo()
        {
            ResHeroAdvanceInfo info = GetHeroAdvanceInfo(this.cfgInfo.dwCfgID, this.mActorValue.actorQuality, this.mActorValue.actorSubQuality);
            if (info == null)
            {
                string str = string.Format("ResHeroAdvanceInfo can not find heroId = {0}, quality = {1}, subQuality = {2}", this.cfgInfo.dwCfgID, this.mActorValue.actorQuality, this.mActorValue.actorSubQuality);
                return null;
            }
            return info;
        }

        public static ResHeroAdvanceInfo GetHeroAdvanceInfo(uint heroID, int iQuality, int iSubQuality)
        {
            uint key = Convert.ToUInt32(string.Format("{0}{1}{2}", heroID, iQuality, iSubQuality));
            return GameDataMgr.heroAdvanceDatabin.GetDataByKey(key);
        }

        public static string GetHeroDesc(uint heroId)
        {
            return Utility.UTF8Convert(GameDataMgr.heroDatabin.GetDataByKey(heroId).szHeroDesc);
        }

        public static ResHeroProficiency GetHeroProficiency(int job, int level)
        {
            HashSet<object>.Enumerator enumerator = GameDataMgr.heroProficiencyDatabin.GetDataByKey((int) ((byte) level)).GetEnumerator();
            while (enumerator.MoveNext())
            {
                ResHeroProficiency current = (ResHeroProficiency) enumerator.Current;
                if (current.bJob == job)
                {
                    return current;
                }
            }
            return null;
        }

        public static string GetHeroTitle(uint heroId, uint skinId)
        {
            string str = string.Empty;
            if (skinId == 0)
            {
                ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey(heroId);
                if (dataByKey != null)
                {
                    str = StringHelper.UTF8BytesToString(ref dataByKey.szHeroTitle);
                }
                return str;
            }
            ResHeroSkin heroSkin = CSkinInfo.GetHeroSkin(heroId, skinId);
            if (heroSkin != null)
            {
                str = StringHelper.UTF8BytesToString(ref heroSkin.szSkinName);
            }
            return str;
        }

        public static int GetInitCombatByHeroId(uint id)
        {
            ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey(id);
            if (dataByKey == null)
            {
                return 0;
            }
            int combatEftByStarLevel = GetCombatEftByStarLevel(1, dataByKey.iInitialStar);
            int combatEft = CSkinInfo.GetCombatEft(id, 0);
            return (combatEftByStarLevel + combatEft);
        }

        public static string GetJobFeature(uint heroId)
        {
            string str = string.Empty;
            ResHeroCfgInfo dataByKey = GameDataMgr.heroDatabin.GetDataByKey(heroId);
            CTextManager instance = Singleton<CTextManager>.GetInstance();
            if (dataByKey != null)
            {
                switch (dataByKey.bJob)
                {
                    case 1:
                        str = str + instance.GetText("Hero_Job_Tank");
                        break;

                    case 2:
                        str = str + instance.GetText("Hero_Job_Soldier");
                        break;

                    case 3:
                        str = str + instance.GetText("Hero_Job_Assassin");
                        break;

                    case 4:
                        str = str + instance.GetText("Hero_Job_Master");
                        break;

                    case 5:
                        str = str + instance.GetText("Hero_Job_Archer");
                        break;

                    case 6:
                        str = str + instance.GetText("Hero_Job_Aid");
                        break;
                }
                string featureStr = string.Empty;
                for (int i = 0; i < dataByKey.JobFeature.Length; i++)
                {
                    featureStr = GetFeatureStr((RES_HERO_JOB_FEATURE) dataByKey.JobFeature[i]);
                    if (!string.IsNullOrEmpty(featureStr))
                    {
                        str = string.Format("{0}/{1}", str, featureStr);
                    }
                }
            }
            return str;
        }

        public static ResHeroAdvanceInfo GetLastStepInfo(uint heroID, int iQuality, int iSubQuality)
        {
            <GetLastStepInfo>c__AnonStorey34 storey = new <GetLastStepInfo>c__AnonStorey34 {
                heroID = heroID,
                iQuality = iQuality,
                iSubQuality = iSubQuality
            };
            return GameDataMgr.heroAdvanceDatabin.FindIf(new Func<ResHeroAdvanceInfo, bool>(storey, (IntPtr) this.<>m__28));
        }

        public void GetLevelExp(out int curExp, out int maxExp)
        {
            curExp = this.mActorValue.actorExp;
            ResHeroLvlUpInfo dataByKey = GameDataMgr.heroLvlUpDatabin.GetDataByKey((uint) this.mActorValue.actorLvl);
            maxExp = (int) dataByKey.dwExp;
        }

        public void GetLevelUpPropData(ref int[] propArr, ref int[] propPctArr, ref int[] propValAddArr, ref int[] propPctAddArr)
        {
            int index = 0;
            int num2 = 0x24;
            for (index = 0; index < num2; index++)
            {
                propArr[index] = 0;
                propPctArr[index] = 0;
                propValAddArr[index] = 0;
                propPctAddArr[index] = 0;
            }
            int actorStar = this.mActorValue.actorStar;
            int num4 = 0;
            num4 = 5;
            propArr[num4] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MAXHP].totalValue;
            propValAddArr[num4] = propArr[num4] + (this.cfgInfo.iHpGrowth / 0x2710);
            num4 = 1;
            propArr[num4] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYATKPT].totalValue;
            propValAddArr[num4] = propArr[num4] + (this.cfgInfo.iAtkGrowth / 0x2710);
            num4 = 3;
            propArr[num4] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_PHYDEFPT].totalValue;
            propValAddArr[num4] = propArr[num4] + (this.cfgInfo.iDefGrowth / 0x2710);
            num4 = 2;
            propArr[num4] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCATKPT].totalValue;
            propValAddArr[num4] = propArr[num4] + (this.cfgInfo.iSpellGrowth / 0x2710);
            num4 = 4;
            propArr[num4] = this.mActorValue[RES_FUNCEFT_TYPE.RES_FUNCEFT_MGCDEFPT].totalValue;
            propValAddArr[num4] = propArr[num4] + (this.cfgInfo.iResistGrowth / 0x2710);
        }

        public uint GetMaxLevel(uint roleLevel)
        {
            ResHeroAdvanceInfo info = GetHeroAdvanceInfo(this.cfgInfo.dwCfgID, this.mActorValue.actorQuality, this.mActorValue.actorSubQuality);
            return Math.Min(roleLevel, info.dwMaxLevel);
        }

        public int GetMaxLvlUpExp(uint roleLevel)
        {
            uint maxLevel = this.GetMaxLevel(roleLevel);
            int actorLvl = this.mActorValue.actorLvl;
            int num3 = 0;
            while (actorLvl < maxLevel)
            {
                ResHeroLvlUpInfo dataByKey = GameDataMgr.heroLvlUpDatabin.GetDataByKey((uint) actorLvl);
                num3 += (int) dataByKey.dwExp;
                actorLvl++;
            }
            num3 -= this.mActorValue.actorExp;
            if (num3 < 0)
            {
                num3 = 0;
            }
            return num3;
        }

        public static int GetMaxProficiency()
        {
            if (m_maxProficiency <= 0)
            {
                for (int i = 0; i < GameDataMgr.heroProficiencyDatabin.Count(); i++)
                {
                    HashSet<object>.Enumerator enumerator = GameDataMgr.heroProficiencyDatabin.GetDataByIndex(i).GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        m_maxProficiency = Math.Max(m_maxProficiency, ((ResHeroProficiency) enumerator.Current).bLevel);
                    }
                }
            }
            return m_maxProficiency;
        }

        public static ResHeroQualityPicInfo GetQualityPathInfo(int quality, int subQuality)
        {
            HashSet<object>.Enumerator enumerator = GameDataMgr.heroQualityPicDatabin.GetDataByKey(quality).GetEnumerator();
            while (enumerator.MoveNext())
            {
                ResHeroQualityPicInfo current = (ResHeroQualityPicInfo) enumerator.Current;
                if (current.iSubQualityId == subQuality)
                {
                    return current;
                }
            }
            return null;
        }

        public void Init(ulong playerId, COMDT_HEROINFO svrInfo)
        {
            this.cfgInfo = GameDataMgr.heroDatabin.GetDataByKey(svrInfo.stCommonInfo.dwHeroID);
            this.m_selectPageIndex = svrInfo.stCommonInfo.bSymbolPageWear;
            if (this.mActorValue == null)
            {
                this.mActorValue = new PropertyHelper();
            }
            this.mActorValue.Init(svrInfo);
            if (this.skillInfo == null)
            {
                this.skillInfo = new CSkillData();
            }
            this.skillInfo.InitSkillData(this.cfgInfo, svrInfo.stCommonInfo.stSkill);
            this.m_Proficiency = svrInfo.stCommonInfo.stProficiency.dwProficiency;
            this.m_ProficiencyLV = svrInfo.stCommonInfo.stProficiency.bLv;
            this.MaskBits = svrInfo.stCommonInfo.dwMaskBits;
            this.m_skinInfo.Init(svrInfo.stCommonInfo.wSkinID);
            this.m_talentBuyList = new byte[svrInfo.stCommonInfo.stTalent.astTalentInfo.Length];
            for (int i = 0; i < svrInfo.stCommonInfo.stTalent.astTalentInfo.Length; i++)
            {
                this.m_talentBuyList[i] = svrInfo.stCommonInfo.stTalent.astTalentInfo[i].bIsBuyed;
            }
            this.m_awakeState = svrInfo.stCommonInfo.stTalent.bWakeState;
            this.m_awakeStepID = svrInfo.stCommonInfo.stTalent.stWakeStep.dwStepID;
            this.m_isStepFinish = svrInfo.stCommonInfo.stTalent.stWakeStep.bIsFinish == 1;
            this.m_experienceDeadLine = !this.IsExperienceHero() ? 0 : svrInfo.stCommonInfo.dwDeadLine;
        }

        public bool IsExperienceHero()
        {
            return ((this.MaskBits & 8) != 0);
        }

        public bool IsMaxQuality()
        {
            ResHeroAdvanceInfo heroAdvanceInfo = this.GetHeroAdvanceInfo();
            DebugHelper.Assert(heroAdvanceInfo != null);
            return ((heroAdvanceInfo != null) && (heroAdvanceInfo.iAdvQuality == 0));
        }

        public bool IsQualityMaxLevel()
        {
            ResHeroAdvanceInfo heroAdvanceInfo = this.GetHeroAdvanceInfo();
            DebugHelper.Assert(heroAdvanceInfo != null);
            return ((heroAdvanceInfo != null) && (this.mActorValue.actorLvl >= heroAdvanceInfo.dwMaxLevel));
        }

        public bool IsValidExperienceHero()
        {
            bool flag = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().getCurrentTimeSinceLogin() < this.m_experienceDeadLine;
            return (this.IsExperienceHero() && flag);
        }

        private void OnExpChange()
        {
            Singleton<EventRouter>.instance.BroadCastEvent<uint>("HeroExpChange", this.cfgInfo.dwCfgID);
        }

        public void OnHeroAdvance(int quality, int subQuality)
        {
            this.mActorValue.actorQuality = quality;
            this.mActorValue.actorSubQuality = subQuality;
            this.mActorValue.AddHeroAdvanceAttToHeroInfo(this.cfgInfo.dwCfgID, this.mActorValue.actorQuality, this.mActorValue.actorSubQuality);
        }

        public void OnHeroInfoUpdate(SCPKG_NTF_HERO_INFO_UPD svrHeroInfoUp)
        {
            for (int i = 0; i < svrHeroInfoUp.iHeroUpdNum; i++)
            {
                CS_HEROINFO_UPD_TYPE bUpdType = (CS_HEROINFO_UPD_TYPE) svrHeroInfoUp.astHeroUpdInfo[i].bUpdType;
                int slotId = svrHeroInfoUp.astHeroUpdInfo[i].stValueParam.Value[0];
                switch (bUpdType)
                {
                    case CS_HEROINFO_UPD_TYPE.CS_HEROINFO_UPD_LEVEL:
                        this.mActorValue.actorLvl = slotId;
                        break;

                    case CS_HEROINFO_UPD_TYPE.CS_HEROINFO_UPD_EXP:
                        this.mActorValue.actorExp = slotId;
                        break;

                    case CS_HEROINFO_UPD_TYPE.CS_HEROINFO_UPD_STAR:
                        this.mActorValue.actorStar = slotId;
                        break;

                    case CS_HEROINFO_UPD_TYPE.CS_HEROINFO_UPD_QUALITY:
                        this.mActorValue.actorQuality = slotId;
                        break;

                    case CS_HEROINFO_UPD_TYPE.CS_HEROINFO_UPD_SUBQUALITY:
                        this.mActorValue.actorSubQuality = slotId;
                        break;

                    case CS_HEROINFO_UPD_TYPE.CS_HEROINFO_UPD_UNLOCKSKILLSLOT:
                        this.skillInfo.UnLockSkill(slotId);
                        break;

                    case CS_HEROINFO_UPD_TYPE.CS_HEROINFO_UPD_PROFICIENCY:
                        this.m_ProficiencyLV = (byte) slotId;
                        this.m_Proficiency = (uint) svrHeroInfoUp.astHeroUpdInfo[i].stValueParam.Value[1];
                        break;

                    case CS_HEROINFO_UPD_TYPE.CS_HEROINFO_UPD_MASKBITS:
                    {
                        uint maskBits = this.MaskBits;
                        this.MaskBits = (uint) slotId;
                        if (((maskBits & 2) == 0) && ((this.MaskBits & 2) > 0))
                        {
                            Singleton<EventRouter>.instance.BroadCastEvent<string>("HeroUnlockPvP", StringHelper.UTF8BytesToString(ref this.cfgInfo.szName));
                        }
                        break;
                    }
                    case CS_HEROINFO_UPD_TYPE.CS_HEROINFO_UPD_LIMITTIME:
                    {
                        string str = StringHelper.UTF8BytesToString(ref this.cfgInfo.szName);
                        Singleton<EventRouter>.instance.BroadCastEvent<string, uint, uint>("HeroExperienceTimeUpdate", str, this.m_experienceDeadLine, (uint) slotId);
                        this.m_experienceDeadLine = (uint) slotId;
                        break;
                    }
                }
            }
        }

        public void OnHeroLevelUp(SCPKG_UPHEROLVL_RSP lvlUpRsp)
        {
            this.mActorValue.actorLvl = lvlUpRsp.wCurLevel;
            this.mActorValue.actorExp = (int) lvlUpRsp.dwCurExp;
        }

        public void OnHeroSkinWear(uint skinId)
        {
            uint wearSkinId = this.m_skinInfo.GetWearSkinId();
            this.mActorValue.SetSkinProp(this.cfgInfo.dwCfgID, wearSkinId, false);
            this.m_skinInfo.SetWearSkinId(skinId);
            this.mActorValue.SetSkinProp(this.cfgInfo.dwCfgID, skinId, true);
        }

        private void OnLevelChange()
        {
            Singleton<EventRouter>.instance.BroadCastEvent<uint>("HeroLevelChange", this.cfgInfo.dwCfgID);
        }

        private void OnQualityChange()
        {
            Singleton<EventRouter>.instance.BroadCastEvent<uint>("HeroQualityChange", this.cfgInfo.dwCfgID);
        }

        private void OnStarChange()
        {
            Singleton<EventRouter>.instance.BroadCastEvent<uint>("HeroStarChange", this.cfgInfo.dwCfgID);
        }

        public void OnSymbolPageChange(int pageIdx)
        {
            this.m_selectPageIndex = pageIdx;
            Singleton<EventRouter>.instance.BroadCastEvent<uint>("HeroSymbolPageChange", this.cfgInfo.dwCfgID);
        }

        [CompilerGenerated]
        private sealed class <GetLastStepInfo>c__AnonStorey34
        {
            internal uint heroID;
            internal int iQuality;
            internal int iSubQuality;

            internal bool <>m__28(ResHeroAdvanceInfo x)
            {
                return (((x.dwHeroID == this.heroID) && (x.iAdvQuality == this.iQuality)) && (x.iAdvSubQuality == this.iSubQuality));
            }
        }
    }
}

