namespace Assets.Scripts.GameSystem
{
    using ResData;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct stTalentEventParams
    {
        public byte talentLevelIndex;
        public ResTalentLib talentInfo;
        public bool isCanLearn;
        public bool isHaveTalent;
    }
}

