using UnityEngine;
using System.Collections;
using cs;

public class WSWGameWorld : GameWorld
{
    public override void Initialize()
    {
        base.Initialize();

        _SetupGameState();
    }

    private void _SetupGameState()
    {
        GameStateManager.Get().RegisterState(new WSWGameStatePreLoading());
        GameStateManager.Get().RegisterState(new WSWGameStateLogin());
        GameStateManager.Get().RegisterState(new WSWGameStateLoading());
        GameStateManager.Get().RegisterState(new WSWGameStateMain());
        GameStateManager.Get().SetCurrentState("PreLoading");
    }
}
