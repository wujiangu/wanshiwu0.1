namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Framework;
    using System;

    public class BuffOverlayRule
    {
        private BuffHolderComponent buffHolder;

        public bool CheckDependCondition(BuffSkill _buffSkill)
        {
            if (_buffSkill.cfgData.iDependCfgID != 0)
            {
                if (this.buffHolder.FindBuff(_buffSkill.cfgData.iDependCfgID) == null)
                {
                    return false;
                }
                this.buffHolder.RemoveBuff(_buffSkill.cfgData.iDependCfgID);
            }
            return true;
        }

        public bool CheckMutexCondition(BuffSkill _buffSkill)
        {
            if ((_buffSkill.cfgData.iMutexCfgID1 != 0) && (this.buffHolder.FindBuff(_buffSkill.cfgData.iMutexCfgID1) != null))
            {
                return false;
            }
            if ((_buffSkill.cfgData.iMutexCfgID2 != 0) && (this.buffHolder.FindBuff(_buffSkill.cfgData.iMutexCfgID2) != null))
            {
                return false;
            }
            if ((_buffSkill.cfgData.iMutexCfgID3 != 0) && (this.buffHolder.FindBuff(_buffSkill.cfgData.iMutexCfgID3) != null))
            {
                return false;
            }
            return true;
        }

        public bool CheckOverlay(BuffSkill _buffSkill)
        {
            if (_buffSkill.cfgData == null)
            {
                return false;
            }
            if (!this.CheckTriggerRate(_buffSkill))
            {
                return false;
            }
            if (!this.CheckMutexCondition(_buffSkill))
            {
                return false;
            }
            if (!this.CheckDependCondition(_buffSkill))
            {
                return false;
            }
            if (_buffSkill.cfgData.dwOverlayMax < 1)
            {
                _buffSkill.SetOverlayCount(1);
                return true;
            }
            BuffSkill skill = this.buffHolder.FindBuff(_buffSkill.SkillID);
            if ((skill != null) && (skill.GetOverlayCount() >= _buffSkill.cfgData.dwOverlayMax))
            {
                if (_buffSkill.cfgData.dwOverlayRule != 1)
                {
                    if (_buffSkill.cfgData.dwOverlayRule == 2)
                    {
                        this.buffHolder.RemoveBuff(_buffSkill.SkillID);
                        return false;
                    }
                    if (_buffSkill.cfgData.dwOverlayRule == 3)
                    {
                        this.buffHolder.RemoveBuff(_buffSkill.SkillID);
                        _buffSkill.SetOverlayCount(1);
                        return true;
                    }
                    if (_buffSkill.cfgData.dwOverlayRule == 4)
                    {
                        int overlayCount = skill.GetOverlayCount();
                        this.buffHolder.RemoveBuff(_buffSkill.SkillID);
                        _buffSkill.SetOverlayCount(overlayCount);
                        return true;
                    }
                }
                return false;
            }
            int num2 = 0;
            if (skill != null)
            {
                num2 = skill.GetOverlayCount();
                this.buffHolder.RemoveBuff(_buffSkill.SkillID);
            }
            _buffSkill.SetOverlayCount(++num2);
            return true;
        }

        public bool CheckTriggerRate(BuffSkill _buffSkill)
        {
            return ((_buffSkill.cfgData.iTriggerRate <= 0) || (FrameRandom.Random(0x2710) < _buffSkill.cfgData.iTriggerRate));
        }

        public void Init(BuffHolderComponent _buffHolder)
        {
            this.buffHolder = _buffHolder;
        }
    }
}

