namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SpawnBuffEventParam
    {
        public uint showType;
        public PoolObjHandle<ActorRoot> src;
        public SpawnBuffEventParam(uint _showType, PoolObjHandle<ActorRoot> _src)
        {
            this.showType = _showType;
            this.src = _src;
        }
    }
}

