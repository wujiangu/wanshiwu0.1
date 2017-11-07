namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameSystem;
    using CSProtocol;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), FrameCommandClass(FRAMECMD_ID_DEF.FRAME_CMD_SignalPanel)]
    public struct SignalCommand : ICommandImplement
    {
        public uint m_heroID;
        public byte m_signalID;
        public int m_worldPositionX;
        public int m_worldPositionY;
        public int m_worldPositionZ;
        public byte m_elementType;
        public byte m_bAlies;
        public uint m_targetObjId;
        public uint m_targetHeroID;
        [FrameCommandCreator]
        public static IFrameCommand Creator(ref FRAME_CMD_PKG msg)
        {
            FrameCommand<SignalCommand> command = FrameCommandFactory.CreateFrameCommand<SignalCommand>();
            command.cmdData.m_heroID = msg.stCmdInfo.stCmdPlayerSendSignal.dwHeroID;
            command.cmdData.m_signalID = msg.stCmdInfo.stCmdPlayerSendSignal.bSignalID;
            command.cmdData.m_worldPositionX = msg.stCmdInfo.stCmdPlayerSendSignal.iWorldPositionX;
            command.cmdData.m_worldPositionY = msg.stCmdInfo.stCmdPlayerSendSignal.iWorldPositionY;
            command.cmdData.m_worldPositionZ = msg.stCmdInfo.stCmdPlayerSendSignal.iWorldPositionZ;
            command.cmdData.m_bAlies = msg.stCmdInfo.stCmdPlayerSendSignal.bBAlies;
            command.cmdData.m_elementType = msg.stCmdInfo.stCmdPlayerSendSignal.bElementType;
            command.cmdData.m_targetObjId = msg.stCmdInfo.stCmdPlayerSendSignal.dwTargetObjID;
            command.cmdData.m_targetHeroID = msg.stCmdInfo.stCmdPlayerSendSignal.dwTargetHeroID;
            return command;
        }

        public bool TransProtocol(ref FRAME_CMD_PKG msg)
        {
            msg.stCmdInfo.stCmdPlayerSendSignal.dwHeroID = this.m_heroID;
            msg.stCmdInfo.stCmdPlayerSendSignal.bSignalID = this.m_signalID;
            msg.stCmdInfo.stCmdPlayerSendSignal.iWorldPositionX = this.m_worldPositionX;
            msg.stCmdInfo.stCmdPlayerSendSignal.iWorldPositionY = this.m_worldPositionY;
            msg.stCmdInfo.stCmdPlayerSendSignal.iWorldPositionZ = this.m_worldPositionZ;
            msg.stCmdInfo.stCmdPlayerSendSignal.bBAlies = this.m_bAlies;
            msg.stCmdInfo.stCmdPlayerSendSignal.bElementType = this.m_elementType;
            msg.stCmdInfo.stCmdPlayerSendSignal.dwTargetObjID = this.m_targetObjId;
            msg.stCmdInfo.stCmdPlayerSendSignal.dwTargetHeroID = this.m_targetHeroID;
            return true;
        }

        public bool TransProtocol(ref CSDT_GAMING_CSSYNCINFO msg)
        {
            return true;
        }

        public void OnReceive(IFrameCommand cmd)
        {
        }

        public void Preprocess(IFrameCommand cmd)
        {
        }

        public void ExecCommand(IFrameCommand cmd)
        {
            SignalPanel signalPanel = Singleton<CBattleSystem>.GetInstance().GetSignalPanel();
            if (signalPanel != null)
            {
                signalPanel.ExecCommand(cmd.playerID, this.m_heroID, this.m_signalID, this.m_worldPositionX, this.m_worldPositionY, this.m_worldPositionZ, this.m_bAlies, this.m_elementType, this.m_targetObjId, this.m_targetHeroID);
            }
        }

        public void AwakeCommand(IFrameCommand cmd)
        {
        }
    }
}

