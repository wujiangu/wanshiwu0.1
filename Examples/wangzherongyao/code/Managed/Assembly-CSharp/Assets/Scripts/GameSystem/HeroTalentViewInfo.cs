namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Common;
    using System;

    public class HeroTalentViewInfo
    {
        public PoolObjHandle<ActorRoot> m_hero;
        public ListView<HeroTalentLevelInfo> m_heroTalentLevelInfoList;
        public uint[] m_learnTalentList;
        public byte m_talentLevel;
    }
}

