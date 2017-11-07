namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic.GameKernal;
    using CSProtocol;
    using System;

    public class InBattleMsgShower
    {
        public static void ShowBubble(ulong playerid, uint heroID, string content)
        {
            Player playerByUid = Singleton<GamePlayerCenter>.instance.GetPlayerByUid(playerid);
            if (playerByUid != null)
            {
                ReadonlyContext<PoolObjHandle<ActorRoot>> allHeroes = playerByUid.GetAllHeroes();
                for (int i = 0; i < allHeroes.Count; i++)
                {
                    PoolObjHandle<ActorRoot> handle = allHeroes[i];
                    if (handle.handle != null)
                    {
                        PoolObjHandle<ActorRoot> handle2 = allHeroes[i];
                        if (handle2.handle.TheActorMeta.ConfigId == heroID)
                        {
                            PoolObjHandle<ActorRoot> handle3 = allHeroes[i];
                            handle3.handle.HudControl.SetTextHud(content, 12, 0, false);
                            return;
                        }
                    }
                }
            }
        }

        public static void ShowInBattleMsg(COM_INBATTLE_CHAT_TYPE type, ulong playerid, uint heroID, string content, string sound)
        {
            if (type == COM_INBATTLE_CHAT_TYPE.COM_INBATTLE_CHATTYPE_SIGNAL)
            {
                CSignalTips_InBatMsg msg = new CSignalTips_InBatMsg(playerid, heroID, content, sound);
                Singleton<CBattleSystem>.instance.GetSignalPanel().Add_SignalTip(msg);
                if (!string.IsNullOrEmpty(sound))
                {
                    Singleton<CSoundManager>.GetInstance().PlayBattleSound(sound, null);
                }
            }
            else if (type == COM_INBATTLE_CHAT_TYPE.COM_INBATTLE_CHATTYPE_BUBBLE)
            {
                ShowBubble(playerid, heroID, content);
                Singleton<InBattleMsgMgr>.instance.UpdatePlayerBubbleTimer(playerid, heroID);
                if (!string.IsNullOrEmpty(sound))
                {
                    Singleton<CSoundManager>.GetInstance().PlayBattleSound(sound, null);
                }
            }
        }
    }
}

