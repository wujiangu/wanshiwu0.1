namespace Assets.Scripts.Framework
{
    using Assets.Scripts.GameSystem;
    using Assets.Scripts.Sound;
    using System;

    [GameState]
    public class LobbyState : BaseState
    {
        private void OnLobbySceneCompleted()
        {
            Singleton<GameLogic>.GetInstance().OpenLobby();
            CUICommonSystem.OpenFps();
            Singleton<CUIParticleSystem>.GetInstance().Open();
            Singleton<CChatController>.GetInstance().SetChatTimerEnable(true);
            Singleton<ApolloHelper>.GetInstance().ShowNotice(1, "295");
        }

        public override void OnStateEnter()
        {
            Singleton<NewbieWeakGuideControl>.GetInstance().OpenGuideForm();
            MonoSingleton<NewbieGuideManager>.GetInstance().CheckSkipIntoLobby();
            Singleton<CChatController>.GetInstance().bSendChat = true;
            Singleton<CChatController>.GetInstance().SubmitRefreshEvent();
            Singleton<ResourceLoader>.GetInstance().LoadScene("LobbyScene", new ResourceLoader.LoadCompletedDelegate(this.OnLobbySceneCompleted));
            Singleton<CChatController>.instance.model.channelMgr.Clear(EChatChannel.Room, 0L, 0);
            Singleton<CChatController>.instance.model.channelMgr.SetChatTab(CChatChannelMgr.EChatTab.Normal);
            Singleton<CChatController>.instance.view.UpView(false);
            Singleton<CChatController>.instance.model.sysData.ClearEntryText();
            Singleton<CSoundManager>.GetInstance().PostEvent("Login_Stop", null);
            Singleton<CSoundManager>.GetInstance().PostEvent("Main_Play", null);
            Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.LOBBY_STATE_ENTER);
        }

        public override void OnStateLeave()
        {
            Singleton<CChatController>.GetInstance().bSendChat = false;
            Singleton<CChatController>.GetInstance().SetChatTimerEnable(false);
            Singleton<CChatController>.GetInstance().CancleRefreshEvent();
            Singleton<CChatController>.GetInstance().ClearAllPanel();
            Singleton<CSoundManager>.GetInstance().PostEvent("Main_Stop", null);
            Singleton<CSoundManager>.GetInstance().UnloadBanks(CSoundManager.BankType.Lobby);
            Singleton<ApolloHelper>.GetInstance().HideScrollNotice();
            Singleton<NewbieWeakGuideControl>.GetInstance().CloseGuideForm();
            Singleton<EventRouter>.GetInstance().BroadCastEvent(EventID.LOBBY_STATE_LEAVE);
        }
    }
}

