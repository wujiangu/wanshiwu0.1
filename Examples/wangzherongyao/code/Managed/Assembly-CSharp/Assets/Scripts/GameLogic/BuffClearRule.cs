namespace Assets.Scripts.GameLogic
{
    using ResData;
    using System;
    using System.Collections.Generic;

    public class BuffClearRule
    {
        private BuffHolderComponent buffHolder;
        private List<int> CacheBufferList = new List<int>();

        public void CacheClearBuff(BuffSkill _buff, RES_SKILLFUNC_CLEAR_RULE _rule)
        {
            if ((_buff.cfgData.dwClearRule == ((long) _rule)) && (_buff.cfgData.dwEffectType == 3))
            {
                this.CacheBufferList.Add(_buff.cfgData.iCfgID);
            }
        }

        public void CheckBuffClear(RES_SKILLFUNC_CLEAR_RULE _rule)
        {
            if (this.buffHolder.SpawnedBuffList.Count != 0)
            {
                BuffSkill[] array = new BuffSkill[this.buffHolder.SpawnedBuffList.Count];
                this.buffHolder.SpawnedBuffList.CopyTo(array);
                for (int i = 0; i < array.Length; i++)
                {
                    BuffSkill skill = array[i];
                    if (skill.cfgData.dwClearRule == ((long) _rule))
                    {
                        skill.Stop();
                    }
                }
            }
        }

        public void CheckBuffNoClear(RES_SKILLFUNC_CLEAR_RULE _rule)
        {
            if (this.buffHolder.SpawnedBuffList.Count != 0)
            {
                BuffSkill[] array = new BuffSkill[this.buffHolder.SpawnedBuffList.Count];
                this.buffHolder.SpawnedBuffList.CopyTo(array);
                for (int i = 0; i < array.Length; i++)
                {
                    BuffSkill skill = array[i];
                    if ((skill.cfgData.dwClearRule != ((long) _rule)) && (skill.cfgData.dwEffectType != 3))
                    {
                        skill.Stop();
                    }
                }
            }
        }

        public void Init(BuffHolderComponent _buffHolder)
        {
            this.buffHolder = _buffHolder;
        }

        public void RecoverClearBuff()
        {
            int inSkillCombineId = 0;
            for (int i = 0; i < this.CacheBufferList.Count; i++)
            {
                inSkillCombineId = this.CacheBufferList[i];
                SkillUseContext inContext = new SkillUseContext();
                inContext.SetOriginator(this.buffHolder.actorPtr);
                this.buffHolder.actor.SkillControl.SpawnBuff(this.buffHolder.actorPtr, inContext, inSkillCombineId, false);
            }
            this.CacheBufferList.Clear();
        }
    }
}

