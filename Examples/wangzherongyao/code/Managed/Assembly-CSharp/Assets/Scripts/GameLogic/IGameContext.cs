namespace Assets.Scripts.GameLogic
{
    using System;

    public interface IGameContext
    {
        IGameInfo CreateGame();
        bool IsBalanceProp();
        bool IsFireHole();
        bool IsPureBalanceProp();
        bool IsSoulGrow();
        void PrepareStartup();

        string levelArtistFileName { get; }

        SLevelContext levelContext { get; }

        string levelDesignFileName { get; }

        int rewardCount { get; }
    }
}

