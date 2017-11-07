using UnityEngine;
using System.Collections;
using cs;

public class WSWGameStateMain : GameState
{

    public override string GetName()
    {
        return m_strName;
    }

    public override void OnEnterState(GameState a_preState)
    {
        //LuaManager.Get().DoFile("");

        cs.Logger.Log("假装已经加载了主场景");
        LuaManager.Get().DoFile("../Main/Script/luaScript/InitGameEventTable.lua");
    }

    public override void OnLeaveState(GameState a_nextState)
    {

    }

    private string m_strName = "Main";
}
