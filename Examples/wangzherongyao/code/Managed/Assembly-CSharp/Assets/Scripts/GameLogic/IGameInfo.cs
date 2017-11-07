namespace Assets.Scripts.GameLogic
{
    using System;

    public interface IGameInfo
    {
        void EndGame();
        void OnLoadingProgress(float Progress);
        void PostBeginPlay();
        void PreBeginPlay();
        void ReduceDamage(ref HurtDataInfo HurtInfo);
        void StartFight();

        IGameContext gameContext { get; }
    }
}

