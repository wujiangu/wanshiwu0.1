namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using CSProtocol;
    using System;
    using System.Runtime.InteropServices;

    public class VDStat
    {
        private CampData[] CampGolds = new CampData[3];

        public VDStat()
        {
            for (int i = 0; i < this.CampGolds.Length; i++)
            {
                this.CampGolds[i] = new CampData();
            }
        }

        public int CalcCampStat(COM_PLAYERCAMP InFrom, COM_PLAYERCAMP InTo)
        {
            return (this.CampGolds[(int) InFrom].Golds - this.CampGolds[(int) InTo].Golds);
        }

        public void Clear()
        {
            for (int i = 0; i < this.CampGolds.Length; i++)
            {
                this.CampGolds[i].Clear();
            }
            Singleton<EventRouter>.instance.RemoveEventHandler<PoolObjHandle<ActorRoot>, int, int, bool>("HeroGoldCoinInBattleChange", new Action<PoolObjHandle<ActorRoot>, int, int, bool>(this, (IntPtr) this.OnHeroGoldCoinChanged));
        }

        public void GetMaxCampStat(COM_PLAYERCAMP InFrom, COM_PLAYERCAMP InTo, out int OutMaxPositive, out int OutMaxNegative)
        {
            OutMaxPositive = this.CampGolds[(int) InFrom].PositiveGolds[(int) InTo];
            OutMaxNegative = this.CampGolds[(int) InFrom].NegativeGolds[(int) InTo];
        }

        private void OnHeroGoldCoinChanged(PoolObjHandle<ActorRoot> InActor, int InChangedValue, int InCurrentValue, bool bInIsIncome)
        {
            if (((InChangedValue > 0) && bInIsIncome) && (InActor != 0))
            {
                byte actorCamp = (byte) InActor.handle.TheActorMeta.ActorCamp;
                if (actorCamp < this.CampGolds.Length)
                {
                    CampData data1 = this.CampGolds[actorCamp];
                    data1.Golds += InChangedValue;
                    this.RefreshFlags(actorCamp);
                }
            }
        }

        private void RefreshFlags(byte InChangedIndex)
        {
            CampData data = this.CampGolds[InChangedIndex];
            for (int i = 0; i < this.CampGolds.Length; i++)
            {
                if (InChangedIndex != i)
                {
                    CampData data2 = this.CampGolds[i];
                    int num2 = data.Golds - data2.Golds;
                    if ((num2 < 0) && (num2 < data.NegativeGolds[i]))
                    {
                        data.NegativeGolds[i] = num2;
                        data2.PositiveGolds[InChangedIndex] = num2 * -1;
                    }
                    else if ((num2 > 0) && (num2 > data.PositiveGolds[i]))
                    {
                        data.PositiveGolds[i] = num2;
                        data2.NegativeGolds[InChangedIndex] = num2 * -1;
                    }
                }
            }
        }

        public bool ShouldStat()
        {
            return Singleton<BattleLogic>.instance.GetCurLvelContext().isPVPLevel;
        }

        public void StartRecord()
        {
            this.Clear();
            if (this.ShouldStat())
            {
                Singleton<EventRouter>.instance.AddEventHandler<PoolObjHandle<ActorRoot>, int, int, bool>("HeroGoldCoinInBattleChange", new Action<PoolObjHandle<ActorRoot>, int, int, bool>(this, (IntPtr) this.OnHeroGoldCoinChanged));
            }
        }
    }
}

