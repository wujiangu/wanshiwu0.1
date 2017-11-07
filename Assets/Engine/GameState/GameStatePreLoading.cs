using UnityEngine;
using System.Collections;

namespace cs
{
    public abstract class GameStatePreLoading : GameState
    {
        public override string GetName()
        {
            return m_strName;
        }

        public override void OnEnterState(GameState a_preState)
        {
            EventSystem.Get().RegisterEventHandler((int)EEventID.EngineScriptLoaded, _OnScriptsLoaded);
            LuaManager.Get().DoFile("CSEngine.lua");
        }

        public override void OnLeaveState(GameState a_nextState)
        {

        }

        protected abstract void _OnScriptsLoaded(Event a_event);

        private string m_strName = "PreLoading";
    }
}

