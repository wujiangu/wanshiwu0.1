using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cs
{
    public abstract class GameState : IUpdate
    {
        public abstract string GetName();

        public virtual void OnEnterState(GameState a_preState)
        {

        }

        public virtual void OnLeaveState(GameState a_nextState)
        {

        }

        public virtual void OnClientActived()
        {

        }

        public virtual void OnClientDeactived()
        {

        }
        
        public virtual void Tick(float a_fElapsed)
        {

        }
    }
}



