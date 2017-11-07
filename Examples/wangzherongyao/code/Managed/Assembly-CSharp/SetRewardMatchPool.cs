using CSProtocol;
using System;

[ArgumentDescription(2, typeof(int), "奖金池数目", new object[] {  }), ArgumentDescription(1, typeof(int), "地图ID", new object[] {  }), CheatCommand("通用/赏金赛/SetRewardMatchPool", "加赏金赛奖金池", 0x45)]
internal class SetRewardMatchPool : CheatCommandNetworking
{
    protected override string Execute(string[] InArguments, ref CSDT_CHEATCMD_DETAIL CheatCmdRef)
    {
        CheatCmdRef.stAddRewardMatchPool = new CSDT_CHEAT_MATCHPOOL();
        CheatCmdRef.stAddRewardMatchPool.iMapID = CheatCommandBase.SmartConvert<int>(InArguments[0]);
        CheatCmdRef.stAddRewardMatchPool.iValue = CheatCommandBase.SmartConvert<int>(InArguments[1]);
        return CheatCommandBase.Done;
    }
}

