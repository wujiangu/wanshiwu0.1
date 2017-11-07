namespace Assets.Scripts.GameSystem
{
    using CSProtocol;
    using ResData;
    using System;

    public class ExeTaskActivity : Activity
    {
        private ResWealCondition _config;

        public ExeTaskActivity(ActivitySys mgr, ResWealCondition config) : base(mgr, config.stCommon)
        {
            this._config = config;
            if (this._config.bTrigType == 1)
            {
                for (int i = this._config.wConNum - 1; i >= 0; i--)
                {
                    ExeTaskPhase ap = new ExeTaskPhase(this, (uint) ((this._config.wConNum - 1) - i), this._config.astConInfo[i]);
                    base.AddPhase(ap);
                }
            }
            else
            {
                for (ushort j = 0; (j < this._config.wConNum) && (j < this._config.astConInfo.Length); j = (ushort) (j + 1))
                {
                    ExeTaskPhase phase2 = new ExeTaskPhase(this, j, this._config.astConInfo[j]);
                    base.AddPhase(phase2);
                }
            }
        }

        public ExeTaskPhase GetPhaseById(int id)
        {
            for (int i = 0; i < base.PhaseList.Count; i++)
            {
                ExeTaskPhase phase = base.PhaseList[i] as ExeTaskPhase;
                if ((phase != null) && (phase.FakeID == id))
                {
                    return phase;
                }
            }
            return null;
        }

        public int GetPhaseCount()
        {
            return base.PhaseList.Count;
        }

        public bool IsSingleConfig()
        {
            return (this._config.bTrigType == 1);
        }

        public void LoadInfo(COMDT_WEAL_CON_DATA_DETAIL conData)
        {
            for (int i = 0; i < base.PhaseList.Count; i++)
            {
                int id = 0;
                if (this._config.bTrigType != 1)
                {
                    id = i;
                }
                else
                {
                    id = (conData.wConNum - i) - 1;
                }
                if ((id < conData.wConNum) && (id >= 0))
                {
                    ExeTaskPhase phaseById = this.GetPhaseById(id);
                    phaseById.SetAchiveve((conData.dwReachMask & (((int) 1) << id)) > 0, (conData.dwLimitReachMask & (((int) 1) << id)) > 0);
                    phaseById.SetCurrent((int) conData.astConData[id].dwValue);
                }
            }
            this.SetPhaseMarks((ulong) conData.dwRewardMask);
        }

        public override void SetPhaseMarks(ulong mask)
        {
            for (int i = 0; i < base.PhaseList.Count; i++)
            {
                ExeTaskPhase phaseById = this.GetPhaseById(i);
                if (phaseById != null)
                {
                    phaseById.Marked = (mask & (((ulong) 1L) << i)) > 0L;
                }
            }
            base.NotifyMaskStateChanged();
        }

        public override int Current
        {
            get
            {
                ExeTaskPhase phase = null;
                for (int i = 0; i < base.PhaseList.Count; i++)
                {
                    ExeTaskPhase phaseById = this.GetPhaseById(i);
                    if (!phaseById.Achieved)
                    {
                        phase = phaseById;
                        break;
                    }
                }
                if ((phase == null) && (base.PhaseList.Count > 0))
                {
                    phase = this.GetPhaseById(base.PhaseList.Count - 1);
                }
                return ((phase == null) ? 0 : phase.Current);
            }
        }

        public override uint ID
        {
            get
            {
                return this._config.dwID;
            }
        }

        public override int Target
        {
            get
            {
                return ((this.GetPhaseById(base.PhaseList.Count - 1) == null) ? 0 : this.GetPhaseById(base.PhaseList.Count - 1).Target);
            }
        }

        public override COM_WEAL_TYPE Type
        {
            get
            {
                return COM_WEAL_TYPE.COM_WEAL_CONDITION;
            }
        }
    }
}

