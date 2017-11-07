using Assets.Scripts.Framework;
using System;

[CheatCommandEntry("性能")]
internal class CheatCommandReplayEntry
{
    [CheatCommandEntryMethod(" 锁帧模式", true, false)]
    public static string LockFPS()
    {
        GameFramework instance = MonoSingleton<GameFramework>.GetInstance();
        instance.LockFPS_SGame = !instance.LockFPS_SGame;
        return (!instance.LockFPS_SGame ? "UNITY" : "SGAME");
    }

    [CheatCommandEntryMethod("PROFILE!", true, false)]
    public static string Profile()
    {
        MonoSingleton<ConsoleWindow>.instance.isVisible = false;
        MonoSingleton<SProfiler>.GetInstance().ToggleVisible();
        return CheatCommandBase.Done;
    }

    [CheatCommandEntryMethod("播放录像", true, false)]
    public static string Replay()
    {
        if (Singleton<GameReplayModule>.HasInstance())
        {
            MonoSingleton<ConsoleWindow>.instance.ToggleVisible();
            Singleton<GameReplayModule>.GetInstance().ShowReplayWndInGame();
        }
        return CheatCommandBase.Done;
    }
}

