using CSProtocol;
using System;

[CheatCommand("英雄/属性修改/其它/AddSymbolCoin", "加符文碎片", 0x30), ArgumentDescription(typeof(int), "数量", new object[] {  })]
internal class AddSymbolCoinCommand : CommonValueChangeCommand
{
    protected override void FillMessageField(ref CSDT_CHEATCMD_DETAIL CheatCmdRef, int InValue)
    {
        CheatCmdRef.stAddSymbolCoin = new CSDT_CHEAT_COMVAL();
        CheatCmdRef.stAddSymbolCoin.iValue = InValue;
    }
}

