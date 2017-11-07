namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.GameLogic.GameKernal;
    using CSProtocol;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ScoreBoard
    {
        private int _duration;
        private int _lastTimeSec;
        private Text _leftScore;
        private Text _rightScore;
        private GameObject _root;
        private Text _txtTime;
        private SLevelContext levelContext;
        private bool m_battleKDAChanged;
        private PoolObjHandle<ActorRoot> m_kingActor;
        private int m_lastMostKill;

        private int CalcCurrentTime()
        {
            int logicFrameTick = (int) Singleton<FrameSynchr>.GetInstance().LogicFrameTick;
            SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
            if (((curLvelContext != null) && curLvelContext.bShowTrainingHelper) && (Singleton<BattleLogic>.instance.dynamicProperty != null))
            {
                logicFrameTick = (int) Singleton<BattleLogic>.instance.dynamicProperty.m_frameTimer;
            }
            return (int) ((logicFrameTick * 1f) / 1000f);
        }

        public void Clear()
        {
            int num = this._duration / 60;
            int num2 = this._duration - (num * 60);
            if (num2 < 0)
            {
                num2 = 0;
            }
            string timeStr = string.Format("{0:D2}:{1:D2}", num, num2);
            Singleton<SettlementSystem>.instance.SetLastMatchDuration(timeStr);
            this._duration = 0;
            DictionaryView<uint, CampInfo> campStat = Singleton<BattleLogic>.GetInstance().battleStat.GetCampStat();
            if (campStat.ContainsKey(1))
            {
                campStat[1].onCampScoreChanged -= new CampInfo.CampInfoValueChanged(this.validateScore);
            }
            if (campStat.ContainsKey(2))
            {
                campStat[2].onCampScoreChanged -= new CampInfo.CampInfoValueChanged(this.validateScore);
            }
            Singleton<EventRouter>.instance.RemoveEventHandler(EventID.BATTLE_KDA_CHANGED, new Action(this, (IntPtr) this.OnBattleKDAChanged));
            this._root = null;
            this._leftScore = null;
            this._rightScore = null;
            this._txtTime = null;
            this.levelContext = null;
            this.m_kingActor.Release();
            this.m_lastMostKill = 0;
        }

        public void Hide()
        {
            this._root.CustomSetActive(false);
        }

        public void Init(GameObject obj)
        {
            this.levelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if (this.levelContext != null)
            {
                this._root = obj;
                this._leftScore = Utility.GetComponetInChild<Text>(obj, "LeftScore");
                this._rightScore = Utility.GetComponetInChild<Text>(obj, "RightScore");
                this._txtTime = Utility.GetComponetInChild<Text>(obj, "TxtTime");
                this._lastTimeSec = 0;
                Utility.GetComponetInChild<Text>(this._root, "Kill").text = "0";
                Utility.GetComponetInChild<Text>(this._root, "Death").text = "0";
                Utility.GetComponetInChild<Text>(this._root, "Assist").text = "0";
            }
        }

        public bool IsShown()
        {
            return this._root.activeSelf;
        }

        public void LateUpdate()
        {
            if (this.m_battleKDAChanged)
            {
                this.UpdateSelfKDADisplay();
                this.UpdateKingOfKiller();
                this.m_battleKDAChanged = false;
            }
        }

        private void OnBattleKDAChanged()
        {
            this.m_battleKDAChanged = true;
        }

        public void RegiseterEvent()
        {
            DictionaryView<uint, CampInfo> campStat = Singleton<BattleLogic>.GetInstance().battleStat.GetCampStat();
            if (campStat.ContainsKey(1))
            {
                campStat[1].onCampScoreChanged += new CampInfo.CampInfoValueChanged(this.validateScore);
            }
            if (campStat.ContainsKey(2))
            {
                campStat[2].onCampScoreChanged += new CampInfo.CampInfoValueChanged(this.validateScore);
            }
            Singleton<EventRouter>.instance.AddEventHandler(EventID.BATTLE_KDA_CHANGED, new Action(this, (IntPtr) this.OnBattleKDAChanged));
        }

        public void Show()
        {
            this._root.CustomSetActive(true);
            DictionaryView<uint, CampInfo> campStat = Singleton<BattleLogic>.GetInstance().battleStat.GetCampStat();
            if (campStat.ContainsKey(1))
            {
                this.validateScore(COM_PLAYERCAMP.COM_PLAYERCAMP_1, campStat[1].campScore, campStat[1].HeadPoints);
            }
            if (campStat.ContainsKey(2))
            {
                this.validateScore(COM_PLAYERCAMP.COM_PLAYERCAMP_2, campStat[2].campScore, campStat[2].HeadPoints);
            }
        }

        public void Update()
        {
            int num = this.CalcCurrentTime();
            this._duration = num;
            if (num != this._lastTimeSec)
            {
                if (Singleton<WinLoseByStarSys>.instance.CurLevelTimeDuration <= 0)
                {
                    int num2 = num / 60;
                    int num3 = num - (num2 * 60);
                    this._txtTime.text = string.Format("{0:D2}:{1:D2}", num2, num3);
                }
                else
                {
                    uint num4 = Singleton<WinLoseByStarSys>.instance.CurLevelTimeDuration / 0x3e8;
                    uint num5 = 0;
                    if (num4 > num)
                    {
                        num5 = num4 - ((uint) num);
                    }
                    uint num6 = num5 / 60;
                    uint num7 = num5 - (num6 * 60);
                    this._txtTime.text = string.Format("{0:D2}:{1:D2}", num6, num7);
                }
                this._lastTimeSec = num;
            }
        }

        private void UpdateKingOfKiller()
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
            if ((curLvelContext != null) && (curLvelContext.HeadPtsUpperLimit > 0))
            {
                CPlayerKDAStat playerKDAStat = Singleton<BattleLogic>.GetInstance().battleStat.m_playerKDAStat;
                if (playerKDAStat != null)
                {
                    uint playerId = 0;
                    int numKill = 0;
                    DictionaryView<uint, PlayerKDA>.Enumerator enumerator = playerKDAStat.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                        if (current.Value != null)
                        {
                            KeyValuePair<uint, PlayerKDA> pair2 = enumerator.Current;
                            if (pair2.Value.numKill > numKill)
                            {
                                KeyValuePair<uint, PlayerKDA> pair3 = enumerator.Current;
                                numKill = pair3.Value.numKill;
                                KeyValuePair<uint, PlayerKDA> pair4 = enumerator.Current;
                                playerId = pair4.Value.PlayerId;
                            }
                        }
                    }
                    if (numKill > this.m_lastMostKill)
                    {
                        Player player = Singleton<GamePlayerCenter>.instance.GetPlayer(playerId);
                        if ((player != null) && (player.Captain != this.m_kingActor))
                        {
                            if ((this.m_kingActor != 0) && (this.m_kingActor.handle.EffectControl != null))
                            {
                                this.m_kingActor.handle.EffectControl.EndKingOfKillerEffect();
                            }
                            this.m_kingActor = player.Captain;
                            if ((this.m_kingActor != 0) && (this.m_kingActor.handle.EffectControl != null))
                            {
                                this.m_kingActor.handle.EffectControl.StartKingOfKillerEffect();
                            }
                        }
                        this.m_lastMostKill = numKill;
                    }
                }
            }
        }

        private void UpdateSelfKDADisplay()
        {
            CPlayerKDAStat playerKDAStat = Singleton<BattleLogic>.GetInstance().battleStat.m_playerKDAStat;
            if (playerKDAStat != null)
            {
                PlayerKDA hostKDA = playerKDAStat.GetHostKDA();
                if ((hostKDA != null) && (this._root != null))
                {
                    Utility.GetComponetInChild<Text>(this._root, "Kill").text = hostKDA.numKill.ToString();
                    Utility.GetComponetInChild<Text>(this._root, "Death").text = hostKDA.numDead.ToString();
                    Utility.GetComponetInChild<Text>(this._root, "Assist").text = hostKDA.numAssist.ToString();
                }
            }
        }

        private void validateScore(COM_PLAYERCAMP campType, int inCampScore, int inHeadPts)
        {
            if (inHeadPts >= 0)
            {
                if (campType == Singleton<GamePlayerCenter>.instance.GetHostPlayer().PlayerCamp)
                {
                    if (this.levelContext.HeadPtsUpperLimit > 0)
                    {
                        this._leftScore.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("ScoreBoard_FireHole_1"), inHeadPts, this.levelContext.HeadPtsUpperLimit);
                    }
                    else
                    {
                        this._leftScore.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("ScoreBoard_Normal_1"), inHeadPts);
                    }
                }
                else if ((campType != COM_PLAYERCAMP.COM_PLAYERCAMP_MID) && (campType != COM_PLAYERCAMP.COM_PLAYERCAMP_COUNT))
                {
                    if (this.levelContext.HeadPtsUpperLimit > 0)
                    {
                        this._rightScore.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("ScoreBoard_FireHole_2"), inHeadPts, this.levelContext.HeadPtsUpperLimit);
                    }
                    else
                    {
                        this._rightScore.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("ScoreBoard_Normal_2"), inHeadPts);
                    }
                }
            }
        }
    }
}

