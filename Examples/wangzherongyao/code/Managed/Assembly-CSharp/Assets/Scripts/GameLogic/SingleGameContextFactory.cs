namespace Assets.Scripts.GameLogic
{
    using CSProtocol;
    using System;

    public class SingleGameContextFactory
    {
        public static IGameContext CreateSingleGameContext(SCPKG_STARTSINGLEGAMERSP InMessage)
        {
            return new SingleGameContext(InMessage);
        }
    }
}

