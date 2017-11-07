namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using System;

    public class TalentSystem
    {
        private PoolObjHandle<ActorRoot> actor = new PoolObjHandle<ActorRoot>();
        private int talentCount;
        private PassiveSkill[] TalentObjArray = new PassiveSkill[6];

        public int GetTalentCDTime(int _talentID)
        {
            for (int i = 0; i < 6; i++)
            {
                PassiveSkill skill = this.TalentObjArray[i];
                if ((skill != null) && (skill.SkillID == _talentID))
                {
                    return skill.GetCDTime();
                }
            }
            return 0;
        }

        public void Init(PoolObjHandle<ActorRoot> _actor)
        {
            this.actor = _actor;
            this.talentCount = 0;
            for (int i = 0; i < 6; i++)
            {
                this.TalentObjArray[i] = null;
            }
        }

        public void InitTalent(int _talentID)
        {
            if (this.talentCount < 6)
            {
                PassiveSkill skill = new PassiveSkill(_talentID, this.actor);
                for (int i = 0; i < 6; i++)
                {
                    if (this.TalentObjArray[i] == null)
                    {
                        this.TalentObjArray[i] = skill;
                        this.talentCount++;
                        return;
                    }
                }
            }
        }

        public void InitTalent(int _talentID, int _cdTime)
        {
            if (this.talentCount < 6)
            {
                PassiveSkill skill = new PassiveSkill(_talentID, this.actor);
                for (int i = 0; i < 6; i++)
                {
                    if (this.TalentObjArray[i] == null)
                    {
                        this.TalentObjArray[i] = skill;
                        this.talentCount++;
                        skill.InitCDTime(_cdTime);
                        return;
                    }
                }
            }
        }

        public PassiveSkill[] QueryTalents()
        {
            return this.TalentObjArray;
        }

        public void RemoveTalent(int _talentID)
        {
            for (int i = 0; i < 6; i++)
            {
                PassiveSkill skill = this.TalentObjArray[i];
                if ((skill != null) && (skill.SkillID == _talentID))
                {
                    if (skill.passiveEvent != null)
                    {
                        skill.passiveEvent.UnInit();
                    }
                    this.TalentObjArray[i] = null;
                    this.talentCount--;
                }
            }
        }

        public void Reset()
        {
            this.actor.Validate();
        }

        public void UnInit()
        {
            for (int i = 0; i < 6; i++)
            {
                PassiveSkill skill = this.TalentObjArray[i];
                if ((skill != null) && (skill.passiveEvent != null))
                {
                    skill.passiveEvent.UnInit();
                }
            }
        }

        public void UpdateLogic(int nDelta)
        {
            for (int i = 0; i < this.talentCount; i++)
            {
                PassiveSkill skill = this.TalentObjArray[i];
                if (skill != null)
                {
                    skill.UpdateLogic(nDelta);
                }
            }
        }
    }
}

