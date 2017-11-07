namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using UnityEngine;

    public class WinLose : Singleton<WinLose>
    {
        public bool LastSingleGameWin = true;
        private bool m_bWin;
        public static string m_FormPath = "UGUI/Form/Battle/Form_BattleResult";
        private GameObject node;

        public override void Init()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_ClickReault, new CUIEventManager.OnUIEventHandler(this.onBackToHall));
        }

        private void onBackToHall(CUIEvent uiEvent)
        {
            Singleton<CUIManager>.GetInstance().CloseForm(m_FormPath);
            SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
            if (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_GUIDE)
            {
                Singleton<GameBuilder>.instance.EndGame();
                Singleton<CBattleGuideManager>.GetInstance().OpenSettle();
            }
            else if (curLvelContext.GameType == COM_GAME_TYPE.COM_MULTI_GAME_OF_LADDER)
            {
                Singleton<SettlementSystem>.instance.ShowLadderSettleForm(this.m_bWin);
            }
            else
            {
                Singleton<SettlementSystem>.instance.ShowPersonalProfit(this.m_bWin);
            }
        }

        public void ShowPanel(bool bWin)
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
            if ((curLvelContext != null) && curLvelContext.bShowTrainingHelper)
            {
                Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Training_HelperUninit);
            }
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Battle_CloseForm);
            CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(m_FormPath, false, true);
            if (script != null)
            {
                this.node = script.gameObject;
                this.m_bWin = bWin;
                Utility.FindChild(this.node, "Win").CustomSetActive(false);
                Utility.FindChild(this.node, "Lose").CustomSetActive(false);
                if (bWin)
                {
                    Utility.FindChild(this.node, "Win").CustomSetActive(true);
                    Singleton<CSoundManager>.GetInstance().PlayBattleSound("Self_Victory", null);
                    Singleton<CSoundManager>.GetInstance().PostEvent("Set_Victor", null);
                }
                else
                {
                    Utility.FindChild(this.node, "Lose").CustomSetActive(true);
                    Singleton<CSoundManager>.GetInstance().PlayBattleSound("Self_Defeat", null);
                    Singleton<CSoundManager>.GetInstance().PostEvent("Set_Defeat", null);
                }
                Singleton<LobbyLogic>.GetInstance().StopSettlePanelTimer();
            }
        }

        private void Test()
        {
        }

        public override void UnInit()
        {
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_ClickReault, new CUIEventManager.OnUIEventHandler(this.onBackToHall));
            this.node = null;
        }
    }
}

