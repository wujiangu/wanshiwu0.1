using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cs
{
    public class GameTimer : IUpdate
    {
        private int m_timerId;
        private string m_timerName;
        /// <summary>执行的间隔</summary>
        private uint m_delay;


        public virtual void Tick(float a_fElapsed)
        {

        }
    }
}

