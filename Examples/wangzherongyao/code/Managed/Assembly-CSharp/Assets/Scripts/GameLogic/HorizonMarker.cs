namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.GameLogic.GameKernal;
    using CSProtocol;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class HorizonMarker : LogicComponent
    {
        private CampMarker[] _campMarkers;
        private bool _enabled;
        private int _jungleHideMarkCount;
        private bool _needTranslucent;
        private int _sightRadius;
        private int sightRadius;

        public void AddHideMark(COM_PLAYERCAMP targetCamp, HorizonConfig.HideMark hm, int count)
        {
            if (this._campMarkers != null)
            {
                if (targetCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_COUNT)
                {
                    for (int i = 0; i < this._campMarkers.Length; i++)
                    {
                        CampMarker marker = this._campMarkers[i];
                        bool ruleVisible = marker.RuleVisible;
                        marker.AddHideMark(hm, count);
                        if (count > 0)
                        {
                            marker.Exposed = false;
                        }
                        Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
                        if (((hostPlayer != null) && (marker.RuleVisible != ruleVisible)) && (i == BattleLogic.MapOtherCampIndex(base.actor.TheActorMeta.ActorCamp, hostPlayer.PlayerCamp)))
                        {
                            this.RefreshVisible();
                        }
                        this.StatHideMark(hm, count);
                    }
                }
                else
                {
                    int index = BattleLogic.MapOtherCampIndex(base.actor.TheActorMeta.ActorCamp, targetCamp);
                    if ((index >= 0) && (index < this._campMarkers.Length))
                    {
                        CampMarker marker2 = this._campMarkers[index];
                        bool flag2 = marker2.RuleVisible;
                        marker2.AddHideMark(hm, count);
                        if (count > 0)
                        {
                            marker2.Exposed = false;
                        }
                        Player player2 = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
                        if (((player2 != null) && (marker2.RuleVisible != flag2)) && (targetCamp == player2.PlayerCamp))
                        {
                            this.RefreshVisible();
                        }
                        this.StatHideMark(hm, count);
                    }
                }
            }
        }

        public void AddShowMark(COM_PLAYERCAMP targetCamp, HorizonConfig.ShowMark sm, int count)
        {
            if (this._campMarkers != null)
            {
                if (targetCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_COUNT)
                {
                    for (int i = 0; i < this._campMarkers.Length; i++)
                    {
                        CampMarker marker = this._campMarkers[i];
                        bool ruleVisible = marker.RuleVisible;
                        marker.AddShowMark(sm, count);
                        Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
                        if (((hostPlayer != null) && (marker.RuleVisible != ruleVisible)) && (i == BattleLogic.MapOtherCampIndex(base.actor.TheActorMeta.ActorCamp, hostPlayer.PlayerCamp)))
                        {
                            this.RefreshVisible();
                        }
                    }
                }
                else
                {
                    int index = BattleLogic.MapOtherCampIndex(base.actor.TheActorMeta.ActorCamp, targetCamp);
                    if ((index >= 0) && (index < this._campMarkers.Length))
                    {
                        CampMarker marker2 = this._campMarkers[index];
                        bool flag2 = marker2.RuleVisible;
                        marker2.AddShowMark(sm, count);
                        Player player2 = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
                        if (((player2 != null) && (marker2.RuleVisible != flag2)) && (targetCamp == player2.PlayerCamp))
                        {
                            this.RefreshVisible();
                        }
                    }
                }
            }
        }

        public bool HasHideMark(COM_PLAYERCAMP targetCamp, HorizonConfig.HideMark hm)
        {
            if (this._campMarkers == null)
            {
                return false;
            }
            int index = BattleLogic.MapOtherCampIndex(base.actor.TheActorMeta.ActorCamp, targetCamp);
            return (((index >= 0) && (index < this._campMarkers.Length)) && this._campMarkers[index].HasHideMark(hm));
        }

        public bool HasShowMark(COM_PLAYERCAMP targetCamp, HorizonConfig.ShowMark sm)
        {
            if (this._campMarkers == null)
            {
                return false;
            }
            int index = BattleLogic.MapOtherCampIndex(base.actor.TheActorMeta.ActorCamp, targetCamp);
            return (((index >= 0) && (index < this._campMarkers.Length)) && this._campMarkers[index].HasShowMark(sm));
        }

        public override void Init()
        {
            base.Init();
            Horizon.EnableMethod horizonEnableMethod = Singleton<BattleLogic>.instance.GetCurLvelContext().horizonEnableMethod;
            this._enabled = (horizonEnableMethod == Horizon.EnableMethod.EnableAll) || (horizonEnableMethod == Horizon.EnableMethod.EnableMarkNoMist);
            if (this._enabled)
            {
                this._campMarkers = new CampMarker[2];
                for (int i = 0; i < this._campMarkers.Length; i++)
                {
                    this._campMarkers[i] = new CampMarker();
                }
            }
            else
            {
                this._campMarkers = null;
            }
            this._jungleHideMarkCount = 0;
            this._needTranslucent = false;
        }

        public bool IsSightVisited(COM_PLAYERCAMP targetCamp)
        {
            if (this._campMarkers != null)
            {
                int index = BattleLogic.MapOtherCampIndex(base.actor.TheActorMeta.ActorCamp, targetCamp);
                if ((index >= 0) && (index < this._campMarkers.Length))
                {
                    return (Singleton<FrameSynchr>.instance.CurFrameNum == this._campMarkers[index].sightFrame);
                }
            }
            return true;
        }

        public bool IsVisibleFor(COM_PLAYERCAMP targetCamp)
        {
            if (this.Enabled && (this._campMarkers != null))
            {
                int index = BattleLogic.MapOtherCampIndex(base.actor.TheActorMeta.ActorCamp, targetCamp);
                if ((index >= 0) && (index < this._campMarkers.Length))
                {
                    return this._campMarkers[index].Visible;
                }
            }
            return true;
        }

        public override void OnUse()
        {
            base.OnUse();
            this._enabled = false;
            this._campMarkers = null;
            this._sightRadius = 0;
            this._jungleHideMarkCount = 0;
            this._needTranslucent = false;
        }

        public void RefreshVisible()
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            if ((hostPlayer != null) && (base.actor.TheActorMeta.ActorType != ActorTypeDef.Actor_Type_Organ))
            {
                base.actor.Visible = this.IsVisibleFor(hostPlayer.PlayerCamp);
            }
        }

        public void SetExposeMark(bool exposed, COM_PLAYERCAMP targetCamp = 3)
        {
            if (this._campMarkers != null)
            {
                if (targetCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_COUNT)
                {
                    for (int i = 0; i < this._campMarkers.Length; i++)
                    {
                        CampMarker marker = this._campMarkers[i];
                        bool ruleVisible = marker.RuleVisible;
                        marker.Exposed = exposed;
                        Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
                        if (((hostPlayer != null) && (marker.RuleVisible != ruleVisible)) && (i == BattleLogic.MapOtherCampIndex(base.actor.TheActorMeta.ActorCamp, hostPlayer.PlayerCamp)))
                        {
                            this.RefreshVisible();
                        }
                    }
                }
                else
                {
                    int index = BattleLogic.MapOtherCampIndex(base.actor.TheActorMeta.ActorCamp, targetCamp);
                    if ((index >= 0) && (index < this._campMarkers.Length))
                    {
                        CampMarker marker2 = this._campMarkers[index];
                        bool flag2 = marker2.RuleVisible;
                        marker2.Exposed = exposed;
                        Player player2 = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
                        if (((player2 != null) && (marker2.RuleVisible != flag2)) && (targetCamp == player2.PlayerCamp))
                        {
                            this.RefreshVisible();
                        }
                    }
                }
                this.StatHideMark(HorizonConfig.HideMark.INVALID, 0);
            }
        }

        private void StatHideMark(HorizonConfig.HideMark hm, int count)
        {
            int num = 0;
            bool flag = false;
            for (int i = 0; i < this._campMarkers.Length; i++)
            {
                CampMarker marker = this._campMarkers[i];
                num += marker.HideMarkTotal;
                flag |= marker.Exposed;
            }
            bool flag2 = (num > 0) && !flag;
            if (this._needTranslucent != flag2)
            {
                this._needTranslucent = flag2;
                base.actor.MatHurtEffect.SetTranslucent(this._needTranslucent);
            }
            if (hm == HorizonConfig.HideMark.Jungle)
            {
                int num3 = this._jungleHideMarkCount;
                this._jungleHideMarkCount += count;
                if ((num3 <= 0) && (this._jungleHideMarkCount > 0))
                {
                    base.actor.HudControl.ShowStatus(StatusHudType.InJungle);
                }
                else if ((num3 > 0) && (this._jungleHideMarkCount <= 0))
                {
                    base.actor.HudControl.HideStatus(StatusHudType.InJungle);
                }
            }
        }

        public override void UpdateLogic(int delta)
        {
            if (this.Enabled)
            {
                Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
                if (((hostPlayer != null) && (base.actor.TheActorMeta.ActorType != ActorTypeDef.Actor_Type_Organ)) && (base.actor.TheActorMeta.ActorCamp != hostPlayer.PlayerCamp))
                {
                    if (base.actor.VisibleIniting)
                    {
                        base.actor.Visible = this.IsVisibleFor(hostPlayer.PlayerCamp);
                    }
                    else if (base.actor.Visible && !this.IsVisibleFor(hostPlayer.PlayerCamp))
                    {
                        base.actor.Visible = false;
                    }
                }
            }
        }

        public void VisitSight(COM_PLAYERCAMP targetCamp)
        {
            if (this._campMarkers != null)
            {
                int index = BattleLogic.MapOtherCampIndex(base.actor.TheActorMeta.ActorCamp, targetCamp);
                if ((index >= 0) && (index < this._campMarkers.Length))
                {
                    this._campMarkers[index].sightFrame = Singleton<FrameSynchr>.instance.CurFrameNum;
                    Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
                    if ((hostPlayer != null) && (targetCamp == hostPlayer.PlayerCamp))
                    {
                        this.RefreshVisible();
                    }
                }
            }
        }

        public bool Enabled
        {
            get
            {
                return (this._enabled && (null != this._campMarkers));
            }
            set
            {
                if (value != this._enabled)
                {
                    this._enabled = value;
                    if (!this._enabled)
                    {
                        base.actor.Visible = true;
                    }
                }
            }
        }

        public int SightRadius
        {
            get
            {
                return this._sightRadius;
            }
            set
            {
                this._sightRadius = value;
            }
        }

        public class CampMarker
        {
            private bool _exposed = false;
            private int[] _hideMarks = new int[2];
            private int[] _showMarks = new int[3];
            public uint sightFrame = 0;

            public CampMarker()
            {
                this.RuleVisible = true;
            }

            public void AddHideMark(HorizonConfig.HideMark hm, int count)
            {
                this._hideMarks[(int) hm] += count;
                if (this._hideMarks[(int) hm] < 0)
                {
                    this._hideMarks[(int) hm] = 0;
                }
                this.ApplyVisibleRules();
            }

            public void AddShowMark(HorizonConfig.ShowMark sm, int count)
            {
                this._showMarks[(int) sm] += count;
                if (this._showMarks[(int) sm] < 0)
                {
                    this._showMarks[(int) sm] = 0;
                }
                this.ApplyVisibleRules();
            }

            private bool ApplyVisibleRules()
            {
                this.RuleVisible = true;
                if (!this._exposed)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (this._hideMarks[i] <= 0)
                        {
                            continue;
                        }
                        bool flag = false;
                        for (int j = 0; j < 3; j++)
                        {
                            if (HorizonConfig.RelationMap[j, i] && (this._showMarks[j] > 0))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            this.RuleVisible = false;
                            break;
                        }
                    }
                }
                return this.RuleVisible;
            }

            public bool HasHideMark(HorizonConfig.HideMark hm)
            {
                return (this._hideMarks[(int) hm] > 0);
            }

            public bool HasShowMark(HorizonConfig.ShowMark sm)
            {
                return (this._showMarks[(int) sm] > 0);
            }

            public bool Exposed
            {
                get
                {
                    return this._exposed;
                }
                set
                {
                    if (value != this._exposed)
                    {
                        this._exposed = value;
                        this.ApplyVisibleRules();
                    }
                }
            }

            public int HideMarkTotal
            {
                get
                {
                    int num = 0;
                    for (int i = 0; i < this._hideMarks.Length; i++)
                    {
                        num += this._hideMarks[i];
                    }
                    return num;
                }
            }

            public bool RuleVisible { get; private set; }

            public bool Visible
            {
                get
                {
                    return (this.RuleVisible && (!Singleton<BattleLogic>.GetInstance().horizon.Enabled || ((this.sightFrame > 0) && (Singleton<FrameSynchr>.instance.CurFrameNum <= (this.sightFrame + 8)))));
                }
            }
        }
    }
}

