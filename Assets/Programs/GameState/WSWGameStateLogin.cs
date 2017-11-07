using UnityEngine;
using System.Collections;
using cs;

public class WSWGameStateLogin : GameState
{
    public override string GetName()
    {
        return m_strName;
    }

    public override void OnEnterState(GameState a_preState)
    {
        //LuaManager.Get().DoFile("Login.lua");
        //EventSystem.Get().SendEvent(GameEventID.Invalid);
    }

    public override void OnLeaveState(GameState a_nextState)
    {

    }

    private string m_strName = "Login";
}
