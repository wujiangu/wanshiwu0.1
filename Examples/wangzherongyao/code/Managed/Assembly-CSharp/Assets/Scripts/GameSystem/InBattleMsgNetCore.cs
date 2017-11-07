namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using CSProtocol;
    using System;

    public class InBattleMsgNetCore
    {
        public static void SendInBattleMsg_PreConfig(uint id, COM_INBATTLE_CHAT_TYPE msgType, uint heroID)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x514);
            msg.stPkgData.stChatReq.stChatMsg.bType = 7;
            msg.stPkgData.stChatReq.stChatMsg.stContent.select(7L);
            msg.stPkgData.stChatReq.stChatMsg.stContent.stInBattle.bChatType = (byte) msgType;
            msg.stPkgData.stChatReq.stChatMsg.stContent.stInBattle.stChatInfo.select((long) msgType);
            msg.stPkgData.stChatReq.stChatMsg.stContent.stInBattle.stFrom.dwAcntHeroID = heroID;
            if (msgType == COM_INBATTLE_CHAT_TYPE.COM_INBATTLE_CHATTYPE_SIGNAL)
            {
                msg.stPkgData.stChatReq.stChatMsg.stContent.stInBattle.stChatInfo.stSignalID.dwTextID = id;
            }
            else if (msgType == COM_INBATTLE_CHAT_TYPE.COM_INBATTLE_CHATTYPE_BUBBLE)
            {
                msg.stPkgData.stChatReq.stChatMsg.stContent.stInBattle.stChatInfo.stBubbleID.dwTextID = id;
            }
            Singleton<NetworkModule>.GetInstance().SendGameMsg(ref msg, 0);
        }
    }
}

