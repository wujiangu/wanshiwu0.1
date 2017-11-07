namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using System;

    public class CSkillStat
    {
        public PoolObjHandle<ActorRoot> actorHero;
        private uint m_uiBeStunnedTime;
        private uint m_uiStunTime;

        public int GetStunSkillNum()
        {
            int num = 0;
            if (this.actorHero == 0)
            {
                return 0;
            }
            SkillSlot[] skillSlotArray = this.actorHero.handle.SkillControl.SkillSlotArray;
            for (int i = 0; i < 7; i++)
            {
                if ((((skillSlotArray[i] != null) && (skillSlotArray[i].SkillObj != null)) && (skillSlotArray[i].SkillObj.cfgData != null)) && (skillSlotArray[i].SkillObj.cfgData.bIsStunSkill == 1))
                {
                    num++;
                }
            }
            return num;
        }

        public void Initialize(PoolObjHandle<ActorRoot> _actorHero)
        {
            this.m_uiStunTime = 0;
            this.m_uiBeStunnedTime = 0;
            this.actorHero = _actorHero;
            Singleton<GameSkillEventSys>.GetInstance().AddEventHandler<BuffChangeEventParam>(GameSkillEventDef.Event_BuffChange, new GameSkillEvent<BuffChangeEventParam>(this.OnActorBuffSkillChange));
        }

        private void OnActorBuffSkillChange(ref BuffChangeEventParam prm)
        {
            if (((((!prm.bIsAdd && ((prm.stBuffSkill != 0) && (prm.stBuffSkill.handle.skillContext != null))) && ((prm.stBuffSkill.handle.skillContext.Originator != 0) && (prm.stBuffSkill.handle.skillContext.TargetActor != 0))) && ((prm.stBuffSkill.handle.skillContext.SlotType >= SkillSlotType.SLOT_SKILL_1) && (prm.stBuffSkill.handle.skillContext.SlotType < SkillSlotType.SLOT_SKILL_COUNT))) && (prm.stBuffSkill.handle.cfgData.dwEffectType == 2)) && ((((prm.stBuffSkill.handle.cfgData.dwShowType == 1) || (prm.stBuffSkill.handle.cfgData.dwShowType == 3)) || ((prm.stBuffSkill.handle.cfgData.dwShowType == 4) || (prm.stBuffSkill.handle.cfgData.dwShowType == 5))) || (prm.stBuffSkill.handle.cfgData.dwShowType == 6)))
            {
                ulong num = Singleton<FrameSynchr>.GetInstance().LogicFrameTick - prm.stBuffSkill.handle.ulStartTime;
                if (prm.stBuffSkill.handle.skillContext.Originator.handle.SkillControl != null)
                {
                    prm.stBuffSkill.handle.skillContext.Originator.handle.SkillControl.stSkillStat.m_uiStunTime += (uint) num;
                }
                if (prm.stBuffSkill.handle.skillContext.TargetActor.handle.SkillControl != null)
                {
                    prm.stBuffSkill.handle.skillContext.TargetActor.handle.SkillControl.stSkillStat.m_uiBeStunnedTime += (uint) num;
                }
            }
        }

        public void UnInit()
        {
            Singleton<GameSkillEventSys>.GetInstance().RmvEventHandler<BuffChangeEventParam>(GameSkillEventDef.Event_BuffChange, new GameSkillEvent<BuffChangeEventParam>(this.OnActorBuffSkillChange));
        }

        public uint BeStunTime
        {
            get
            {
                return this.m_uiBeStunnedTime;
            }
        }

        public uint StunTime
        {
            get
            {
                return this.m_uiStunTime;
            }
        }
    }
}

