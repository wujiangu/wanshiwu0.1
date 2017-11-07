using System;

public class CheatWindowExternalIntializer : Singleton<CheatWindowExternalIntializer>
{
    public override void Init()
    {
        base.Init();
        MonoSingleton<ConsoleWindow>.GetInstance();
        Singleton<CheatCommandRegister>.instance.Register(typeof(CheatCommandNetworking).Assembly);
    }
}

