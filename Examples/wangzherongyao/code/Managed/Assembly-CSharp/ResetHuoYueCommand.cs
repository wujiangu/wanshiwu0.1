using CSProtocol;
using System;

[CheatCommand("工具/游戏/活跃度/Reset_Huoyue", "重置活跃值", 0x39), ArgumentDescription(typeof(int), "活跃值ID", new object[] {  })]
internal class ResetHuoYueCommand : CommonValueChangeCommand
{
    protected override void FillMessageField(ref CSDT_CHEATCMD_DETAIL CheatCmdRef, int InValue)
    {
        CheatCmdRef.stHuoYueDuOpt = new CSDT_CHEAT_HUOYUEDU();
        CheatCmdRef.stHuoYueDuOpt.iValue = InValue;
        CheatCmdRef.stHuoYueDuOpt.bOpt = 3;
    }
}

