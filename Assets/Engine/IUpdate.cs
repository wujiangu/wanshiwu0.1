using UnityEngine;
using System.Collections;

namespace cs
{
    public interface IUpdate
    {
        void Tick(float a_fElapsed);
    }
}

