namespace Assets.Scripts.GameLogic
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct DefaultSkillEventParam
    {
        public SkillSlotType slot;
        public int param;
        public DefaultSkillEventParam(SkillSlotType _slot, int _param)
        {
            this.slot = _slot;
            this.param = _param;
        }
    }
}

