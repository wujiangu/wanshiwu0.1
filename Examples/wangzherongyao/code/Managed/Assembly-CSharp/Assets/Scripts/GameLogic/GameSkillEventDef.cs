namespace Assets.Scripts.GameLogic
{
    using System;

    public enum GameSkillEventDef
    {
        Event_SkillCDStart,
        Event_SkillCDEnd,
        Event_ChangeSkillCD,
        Event_DisableSkill,
        Event_EnableSkill,
        Event_ChangeSkill,
        Event_RecoverSkill,
        Event_UpdateSkillUI,
        Event_LimitSkill,
        Event_CancelLimitSkill,
        Event_SpawnBuff,
        Event_SelectTarget,
        Event_ClearTarget,
        Event_UseSkill,
        Event_ProtectDisappear,
        Event_LimitMove,
        Event_CancelLimitMove,
        Event_SkillCooldown,
        Enent_EnergyShortage,
        Event_NoSkillTarget,
        Event_BuffChange,
        Event_UseCanceled,
        Event_LockTarget,
        Event_ClearLockTarget,
        Event_Blindess,
        Event_Max
    }
}

