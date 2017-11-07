using CSProtocol;
using System;

[CheatCommandEntry("关卡")]
internal class CheatCommandLevelEntry
{
    [CheatCommandEntryMethod("通过当前关卡", true, false)]
    public static string FinishLevel()
    {
        if (!Singleton<GameStateCtrl>.instance.isBattleState)
        {
            return "不在副本里面你通过个毛线？？";
        }
        if (Singleton<BattleLogic>.GetInstance().GetCurLvelContext().isPVPLevel && (Singleton<BattleLogic>.GetInstance().GetCurLvelContext().GameType != COM_GAME_TYPE.COM_SINGLE_GAME_OF_GUIDE))
        {
            return "单人游戏才能用";
        }
        Singleton<PVEReviveHeros>.instance.ClearTimeOutTimer();
        Singleton<LobbyLogic>.instance.ReqSingleGameFinish(true, true);
        return CheatCommandBase.Done;
    }
}

