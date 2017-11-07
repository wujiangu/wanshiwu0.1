namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using System;

    public class DamageValueAdjust
    {
        private int m_heroCombatEft;
        private int m_hurtDownRate;
        private int m_hurtDownRateLimit;
        private int m_hurtedDownRate;
        private int m_hurtedDownRateLimit;
        private int m_hurtedUpRate;
        private int m_hurtedUpRateLimit;
        private int m_hurtUpRate;
        private int m_hurtUpRateLimit;
        private int m_levelCombatEft;

        public void FightOver()
        {
            this.m_levelCombatEft = 0;
            this.m_heroCombatEft = 0;
        }

        public void FightStart()
        {
            this.m_hurtUpRate = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x48).dwConfValue;
            this.m_hurtUpRateLimit = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x49).dwConfValue;
            this.m_hurtedDownRate = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x4a).dwConfValue;
            this.m_hurtedDownRateLimit = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x4b).dwConfValue;
            this.m_hurtDownRate = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x4c).dwConfValue;
            this.m_hurtDownRateLimit = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x4d).dwConfValue;
            this.m_hurtedUpRate = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x4e).dwConfValue;
            this.m_hurtedUpRateLimit = (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x4f).dwConfValue;
            this.m_levelCombatEft = Singleton<BattleLogic>.GetInstance().GetCurLvelContext().recommendCombatEft;
            this.m_heroCombatEft = Singleton<GamePlayerCenter>.instance.GetHostPlayer().GetAllHeroCombatEft();
        }

        public int GetAdjustDamage(PoolObjHandle<ActorRoot> atker, PoolObjHandle<ActorRoot> target, int hp)
        {
            if (((this.m_levelCombatEft != 0) && (this.m_heroCombatEft != 0)) && ((atker != 0) && (target != 0)))
            {
                bool flag = atker.handle.IsAtHostPlayerSameTeam();
                bool flag2 = target.handle.IsAtHostPlayerSameTeam();
                if (!flag && !flag2)
                {
                    return hp;
                }
                int num = 0;
                if (flag)
                {
                    if (this.m_levelCombatEft <= this.m_heroCombatEft)
                    {
                        num = (this.m_hurtUpRate * (this.m_heroCombatEft - this.m_levelCombatEft)) / this.m_levelCombatEft;
                        num = (num <= this.m_hurtUpRateLimit) ? num : this.m_hurtUpRateLimit;
                        hp = (hp * (0x2710 + num)) / 0x2710;
                        return hp;
                    }
                    if (this.m_levelCombatEft > this.m_heroCombatEft)
                    {
                        num = (this.m_hurtDownRate * (this.m_levelCombatEft - this.m_heroCombatEft)) / this.m_heroCombatEft;
                        num = (num <= this.m_hurtDownRateLimit) ? num : this.m_hurtDownRateLimit;
                        hp = (hp * (0x2710 - num)) / 0x2710;
                    }
                    return hp;
                }
                if (!flag2)
                {
                    return hp;
                }
                if (this.m_levelCombatEft <= this.m_heroCombatEft)
                {
                    num = (this.m_hurtedDownRate * (this.m_heroCombatEft - this.m_levelCombatEft)) / this.m_levelCombatEft;
                    num = (num <= this.m_hurtedDownRateLimit) ? num : this.m_hurtedDownRateLimit;
                    hp = (hp * (0x2710 - num)) / 0x2710;
                    return hp;
                }
                if (this.m_levelCombatEft > this.m_heroCombatEft)
                {
                    num = (this.m_hurtedUpRate * (this.m_levelCombatEft - this.m_heroCombatEft)) / this.m_heroCombatEft;
                    num = (num <= this.m_hurtedUpRateLimit) ? num : this.m_hurtedUpRateLimit;
                    hp = (hp * (0x2710 + num)) / 0x2710;
                }
            }
            return hp;
        }
    }
}

