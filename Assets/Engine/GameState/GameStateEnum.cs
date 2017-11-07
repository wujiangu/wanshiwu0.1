using UnityEngine;
using System.Collections;

namespace cs
{
    public enum EGameState
    {
        Invalid = -1,

        Splash,
        Preloading,
        Login,
        CreateRole,
        Loading,
        Main,

        Count,
    }

}
