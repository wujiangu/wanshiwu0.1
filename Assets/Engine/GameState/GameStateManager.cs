using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace cs
{
    public class GameStateManager : Singleton<GameStateManager> , IUpdate
    {
        public void RegisterState(GameState a_state)
        {
            if (a_state != null)
            {
                if (m_dictStates.ContainsKey(a_state.GetName()) == false)
                {
                    m_dictStates.Add(a_state.GetName(), a_state);
                }
            }
        }

        public void SetCurrentState(string a_strName)
        {
            if (m_nextState == null)
            {
                GameState tempState = GetState(a_strName);
                if (tempState != null)
                {
                    if (
                        (m_currState != null && tempState.GetName() != m_currState.GetName()) ||
                        m_currState == null
                        )
                    {
                        m_nextState = tempState;
                    }
                }
            }
        }

        public GameState GetCurrentState()
        {
            return m_currState;
        }

        public GameState GetState(string a_strName)
        {
            GameState gameState = null;
            m_dictStates.TryGetValue(a_strName, out gameState);
            return gameState;
        }

        public void ActiveState()
        {
            if (m_currState != null)
            {
                m_currState.OnClientActived();
            }
        }
        
        public void DeactiveState()
        {
            if (m_currState != null)
            {
                m_currState.OnClientDeactived();
            }
        }

        public void Tick(float a_fElapsed)
        {
            if (m_nextState != null)
            {
                GameState oldState = m_currState;
                GameState newState = m_nextState;

                if (m_currState != null)
                {
                    m_currState.OnLeaveState(m_nextState);
                }

                m_nextState.OnEnterState(m_currState);

                m_currState = m_nextState;
                m_nextState = null;

                EventSystem.Get().SendEvent((int)EEventID.GameStateChanged, oldState, newState);
            }
            else
            {
                if (m_currState != null)
                {
                    m_currState.Tick(a_fElapsed);
                }
            }
        }

        protected override void _Initialize()
        {
            m_currState = null;
            m_nextState = null;
            m_dictStates.Clear();
        }

        protected override void _Clear()
        {
            m_currState = null;
            m_nextState = null;
            m_dictStates.Clear();
        }

        private GameState m_currState = null;
        private GameState m_nextState = null;
        private Dictionary<string, GameState> m_dictStates = new Dictionary<string, GameState>();
    }
}

