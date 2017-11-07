using UnityEngine;
using System.Collections;
using cs;

public class WSWGameStateLoading : GameState
{
    public override string GetName()
    {
        return m_strName;
    }

    public override void OnEnterState(GameState a_preState)
    {
        //LuaManager.Get().DoFile("CSEngine.lua");
    }

    public override void OnLeaveState(GameState a_nextState)
    {
        
    }

    private string m_strName = "Loading";
}
