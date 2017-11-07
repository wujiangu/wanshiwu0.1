using CSProtocol;
using System;

[CheatCommand("英雄/属性修改/数值/SetGearQyality", "设置所有装备品阶", 0x2d), ArgumentDescription(0, typeof(int), "ID", new object[] {  }), ArgumentDescription(1, typeof(int), "装备品质", new object[] {  }), ArgumentDescription(2, typeof(int), "装备品阶", new object[] {  })]
internal class SetHeroGearQualityCommand : CheatCommandNetworking
{
    protected override string Execute(string[] InArguments, ref CSDT_CHEATCMD_DETAIL CheatCmdRef)
    {
        int num = CheatCommandBase.SmartConvert<int>(InArguments[0]);
        int num2 = CheatCommandBase.SmartConvert<int>(InArguments[1]);
        int num3 = CheatCommandBase.SmartConvert<int>(InArguments[2]);
        CheatCmdRef.stGearAdvAll = new CSDT_CHEAT_GEARADV_ALL();
        CheatCmdRef.stGearAdvAll.dwHeroID = (uint) num;
        CheatCmdRef.stGearAdvAll.iQuality = num2;
        CheatCmdRef.stGearAdvAll.iSubQuality = num3;
        return CheatCommandBase.Done;
    }
}

