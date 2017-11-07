using CSProtocol;
using System;

[CheatCommand("通用/赏金赛/SetRewardMatchPoint", "加赏金赛积分", 0x44), ArgumentDescription(1, typeof(int), "地图ID", new object[] {  }), ArgumentDescription(3, typeof(int), "战队积分", new object[] {  }), ArgumentDescription(2, typeof(int), "个人积分", new object[] {  })]
internal class SetRewardMatchPoint : CheatCommandNetworking
{
    protected override string Execute(string[] InArguments, ref CSDT_CHEATCMD_DETAIL CheatCmdRef)
    {
        CheatCmdRef.stAddRewardMatchPoint = new CSDT_CHEAT_MATCHPOINT();
        CheatCmdRef.stAddRewardMatchPoint.iMapID = CheatCommandBase.SmartConvert<int>(InArguments[0]);
        CheatCmdRef.stAddRewardMatchPoint.iValue1 = CheatCommandBase.SmartConvert<int>(InArguments[1]);
        CheatCmdRef.stAddRewardMatchPoint.iValue2 = CheatCommandBase.SmartConvert<int>(InArguments[2]);
        return CheatCommandBase.Done;
    }
}

