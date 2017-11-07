using System;

public class NewbieGuideOpenForm : NewbieGuideBaseScript
{
    protected override void Initialize()
    {
        base.Initialize();
        string formPath = string.Format("Newbie/Form_{0}.prefab", ((enOpenFormName) base.currentConf.Param[0]).ToString());
        formPath = "UGUI/Form/System/" + formPath;
        Singleton<CUIManager>.GetInstance().OpenForm(formPath, false, true);
        this.CompleteHandler();
    }

    protected override bool IsDelegateClickEvent()
    {
        return true;
    }

    protected override bool IsDelegateModalControl()
    {
        return true;
    }

    public enum enOpenFormName
    {
        None,
        CustomRecomEquipIntro
    }
}

