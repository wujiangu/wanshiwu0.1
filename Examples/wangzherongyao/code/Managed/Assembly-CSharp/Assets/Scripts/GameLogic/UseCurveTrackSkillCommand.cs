namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.GameKernal;
    using CSProtocol;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), FrameCommandClass(FRAMECMD_ID_DEF.FRAME_CMD_USECURVETRACKSKILL)]
    public struct UseCurveTrackSkillCommand : ICommandImplement
    {
        public VInt3 BeginPos;
        public VInt3 EndPos;
        public SkillSlotType SlotType;
        public int iSkillID;
        [FrameCommandCreator]
        public static IFrameCommand Creator(ref FRAME_CMD_PKG msg)
        {
            FrameCommand<UseCurveTrackSkillCommand> command = FrameCommandFactory.CreateFrameCommand<UseCurveTrackSkillCommand>();
            command.cmdData.SlotType = (SkillSlotType) msg.stCmdInfo.stCmdPlayerUseCurveTrackSkill.chSlotType;
            command.cmdData.BeginPos = CommonTools.ToVector3(msg.stCmdInfo.stCmdPlayerUseCurveTrackSkill.stBegin);
            command.cmdData.EndPos = CommonTools.ToVector3(msg.stCmdInfo.stCmdPlayerUseCurveTrackSkill.stEnd);
            command.cmdData.iSkillID = msg.stCmdInfo.stCmdPlayerUseCurveTrackSkill.iSkillID;
            return command;
        }

        public bool TransProtocol(ref FRAME_CMD_PKG msg)
        {
            msg.stCmdInfo.stCmdPlayerUseCurveTrackSkill.chSlotType = (sbyte) this.SlotType;
            msg.stCmdInfo.stCmdPlayerUseCurveTrackSkill.stBegin = CommonTools.FromVector3(this.BeginPos);
            msg.stCmdInfo.stCmdPlayerUseCurveTrackSkill.stEnd = CommonTools.FromVector3(this.EndPos);
            msg.stCmdInfo.stCmdPlayerUseCurveTrackSkill.iSkillID = this.iSkillID;
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
            Player player = Singleton<GamePlayerCenter>.GetInstance().GetPlayer(cmd.playerID);
            if ((player != null) && (player.Captain != 0))
            {
                player.Captain.handle.ActorControl.CmdUseSkill(cmd, new SkillUseContext(this.SlotType, this.BeginPos, this.EndPos));
            }
        }

        public void AwakeCommand(IFrameCommand cmd)
        {
        }
    }
}

