using Assets.Scripts.Framework;
using CSProtocol;
using ResData;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[ArgumentDescription(ArgumentDescriptionAttribute.EDefaultValueTag.Tag, 0, typeof(int), "年", "2015", new object[] {  }), CheatCommand("英雄/解锁/SetFreeHero", "设置周免英雄", 0x20), ArgumentDescription(ArgumentDescriptionAttribute.EDefaultValueTag.Tag, 4, typeof(int), "英雄ID(0表示所有英雄)", "0", new object[] {  }), ArgumentDescription(3, typeof(byte), "时", new object[] {  }), ArgumentDescription(2, typeof(byte), "日", new object[] {  }), ArgumentDescription(1, typeof(EMonth), "月", new object[] {  })]
internal class SetFreeHero : CheatCommandNetworking
{
    public override bool CheckArguments(string[] InArguments, out string OutMessage)
    {
        <CheckArguments>c__AnonStorey27 storey = new <CheckArguments>c__AnonStorey27();
        if (!base.CheckArguments(InArguments, out OutMessage))
        {
            return false;
        }
        if ((CheatCommandBase.SmartConvert<ushort>(InArguments[0]) < 0x7de) || (CheatCommandBase.SmartConvert<ushort>(InArguments[0]) > 0x7e4))
        {
            OutMessage = "年份错误";
            return false;
        }
        if ((CheatCommandBase.SmartConvert<byte>(InArguments[2]) < 1) || (CheatCommandBase.SmartConvert<byte>(InArguments[2]) > 0x1f))
        {
            OutMessage = "日期错误";
            return false;
        }
        bool flag = false;
        storey.HeroId = CheatCommandBase.SmartConvert<byte>(InArguments[4]);
        if (storey.HeroId == 0)
        {
            flag = true;
        }
        else
        {
            flag = GameDataMgr.heroDatabin.FindIf(new Func<ResHeroCfgInfo, bool>(storey, (IntPtr) this.<>m__3)) != null;
        }
        if (!flag)
        {
            OutMessage = "错误的英雄ID";
            return false;
        }
        return true;
    }

    protected override string Execute(string[] InArguments, ref CSDT_CHEATCMD_DETAIL CheatCmdRef)
    {
        string outMessage = string.Empty;
        if (this.CheckArguments(InArguments, out outMessage))
        {
            CheatCmdRef.stSetFreeHero = new CSDT_CHEAT_SET_FREE_HERO();
            CheatCmdRef.stSetFreeHero.wYear = CheatCommandBase.SmartConvert<ushort>(InArguments[0]);
            EMonth month = (EMonth) CheatCommandBase.StringToEnum(InArguments[1], typeof(EMonth));
            CheatCmdRef.stSetFreeHero.bMonth = (byte) month;
            CheatCmdRef.stSetFreeHero.bDay = CheatCommandBase.SmartConvert<byte>(InArguments[2]);
            CheatCmdRef.stSetFreeHero.bHour = CheatCommandBase.SmartConvert<byte>(InArguments[3]);
            CheatCmdRef.stSetFreeHero.dwHeroID = CheatCommandBase.SmartConvert<uint>(InArguments[4]);
            return CheatCommandBase.Done;
        }
        return outMessage;
    }

    [CompilerGenerated]
    private sealed class <CheckArguments>c__AnonStorey27
    {
        internal uint HeroId;

        internal bool <>m__3(ResHeroCfgInfo x)
        {
            return (x.dwCfgID == this.HeroId);
        }
    }
}

