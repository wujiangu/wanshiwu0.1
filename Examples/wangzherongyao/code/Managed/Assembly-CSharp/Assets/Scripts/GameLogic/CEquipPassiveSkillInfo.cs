namespace Assets.Scripts.GameLogic
{
    using System;

    public class CEquipPassiveSkillInfo : IComparable
    {
        public CrypticInt32 m_equipBuyPrice;
        public ushort m_equipID;
        public bool m_isEnabled;
        public ushort m_passiveSkillGroupID;
        public uint m_passiveSkillID;
        public uint[] m_passiveSkillRelatedFuncIDs;
        public uint m_sequence;

        public CEquipPassiveSkillInfo(ushort equipID, uint equipBuyPrice, uint passiveSkillID, ushort passiveSkillGroupID, uint[] passiveSkillRelatedFuncIDs, uint sequence)
        {
            this.m_equipID = equipID;
            this.m_equipBuyPrice = (CrypticInt32) equipBuyPrice;
            this.m_passiveSkillID = passiveSkillID;
            this.m_passiveSkillGroupID = passiveSkillGroupID;
            this.m_passiveSkillRelatedFuncIDs = passiveSkillRelatedFuncIDs;
            this.m_sequence = sequence;
            this.m_isEnabled = false;
        }

        public int CompareTo(object obj)
        {
            CEquipPassiveSkillInfo info = obj as CEquipPassiveSkillInfo;
            if (((uint) this.m_equipBuyPrice) > ((uint) info.m_equipBuyPrice))
            {
                return 1;
            }
            if (((uint) this.m_equipBuyPrice) == ((uint) info.m_equipBuyPrice))
            {
                if (this.m_sequence > info.m_sequence)
                {
                    return 1;
                }
                if (this.m_sequence == info.m_sequence)
                {
                    return 0;
                }
            }
            return -1;
        }
    }
}

