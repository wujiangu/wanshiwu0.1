using UnityEngine;
using System.Collections;
using cs;

public class WSWGameStatePreLoading : GameStatePreLoading
{
    protected override void _OnScriptsLoaded(cs.Event a_event)
    {
        

        LuaManager.Get().DoFile("InitPreLoading.lua");

        GameStateManager.Get().SetCurrentState("Main");
    }
}
