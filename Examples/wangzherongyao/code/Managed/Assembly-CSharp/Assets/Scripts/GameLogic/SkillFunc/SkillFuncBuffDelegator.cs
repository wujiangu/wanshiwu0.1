namespace Assets.Scripts.GameLogic.SkillFunc
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using System;

    [SkillFuncHandlerClass]
    internal class SkillFuncBuffDelegator
    {
        [SkillFuncHandler(0x1c, new int[] {  })]
        public static bool OnSkillFuncAddMark(ref SSkillFuncContext inContext)
        {
            if (inContext.inStage != ESkillFuncStage.Enter)
            {
                return false;
            }
            PoolObjHandle<ActorRoot> inTargetObj = inContext.inTargetObj;
            if (inTargetObj != 0)
            {
                int skillFuncParam = inContext.GetSkillFuncParam(0, false);
                inTargetObj.handle.BuffHolderComp.markRule.AddBufferMark(inContext.inOriginator, skillFuncParam);
            }
            return true;
        }

        [SkillFuncHandler(0x33, new int[] {  })]
        public static bool OnSkillFuncControlExtraEffect(ref SSkillFuncContext inContext)
        {
            if (inContext.inStage == ESkillFuncStage.Enter)
            {
                int skillFuncParam = inContext.GetSkillFuncParam(0, false);
                inContext.inBuffSkill.handle.CustomParams[0] = skillFuncParam;
                return true;
            }
            return true;
        }

        [SkillFuncHandler(0x1d, new int[] {  })]
        public static bool OnSkillFuncTriggerMark(ref SSkillFuncContext inContext)
        {
            if (inContext.inStage != ESkillFuncStage.Enter)
            {
                return false;
            }
            PoolObjHandle<ActorRoot> inTargetObj = inContext.inTargetObj;
            if (inTargetObj != 0)
            {
                int skillFuncParam = inContext.GetSkillFuncParam(0, false);
                inTargetObj.handle.BuffHolderComp.markRule.TriggerBufferMark(inContext.inOriginator, skillFuncParam);
                int inSkillCombineId = inContext.GetSkillFuncParam(1, false);
                if (inSkillCombineId != 0)
                {
                    inTargetObj.handle.BuffHolderComp.RemoveBuff(inSkillCombineId);
                }
            }
            return true;
        }
    }
}

