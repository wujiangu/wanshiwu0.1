using Assets.Scripts.Framework;
using Assets.Scripts.UI;
using ResData;
using System;

public class CAddSkillSys : Singleton<CAddSkillSys>
{
    public const string ADD_SKILL_FORM_PATH = "UGUI/Form/System/AddedSkill/Form_AddedSkill.prefab";

    public override void Init()
    {
        Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.AddedSkill_OpenForm, new CUIEventManager.OnUIEventHandler(this.OnOpenForm));
        Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.AddedSkill_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnCloseForm));
        Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.AddedSkill_GetDetail, new CUIEventManager.OnUIEventHandler(this.OnGetDetail));
        base.Init();
    }

    private void OnCloseForm(CUIEvent cuiEvent)
    {
        Singleton<CUIManager>.instance.CloseForm("UGUI/Form/System/AddedSkill/Form_AddedSkill.prefab");
        Singleton<CResourceManager>.instance.UnloadUnusedAssets();
    }

    private void OnGetDetail(CUIEvent cuiEvent)
    {
        CUIFormScript form = Singleton<CUIManager>.instance.GetForm("UGUI/Form/System/AddedSkill/Form_AddedSkill.prefab");
        if ((form != null) && !form.IsHided())
        {
            CAddSkillView.OnRefresh(form.gameObject, (ushort) cuiEvent.m_eventParams.tag);
        }
    }

    private void OnOpenForm(CUIEvent cuiEvent)
    {
        if (Singleton<CFunctionUnlockSys>.instance.FucIsUnlock(RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_ADDEDSKILL))
        {
            CUIFormScript script = Singleton<CUIManager>.instance.OpenForm("UGUI/Form/System/AddedSkill/Form_AddedSkill.prefab", false, true);
            if (script != null)
            {
                CAddSkillView.OpenForm(script.gameObject);
            }
        }
        else
        {
            ResSpecialFucUnlock dataByKey = GameDataMgr.specialFunUnlockDatabin.GetDataByKey((uint) 0x16);
            Singleton<CUIManager>.instance.OpenTips(Utility.UTF8Convert(dataByKey.szLockedTip), false, 1f, null, new object[0]);
        }
    }

    public override void UnInit()
    {
        Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.AddedSkill_OpenForm, new CUIEventManager.OnUIEventHandler(this.OnOpenForm));
        Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.AddedSkill_CloseForm, new CUIEventManager.OnUIEventHandler(this.OnCloseForm));
        Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.AddedSkill_GetDetail, new CUIEventManager.OnUIEventHandler(this.OnGetDetail));
        base.UnInit();
    }
}

