using Assets.Scripts.UI;
using System;

internal class NewbieGuideIntroFireMatch : NewbieGuideBaseScript
{
    protected override void Initialize()
    {
        CUIEvent uieventPars = new CUIEvent {
            m_eventID = enUIEventID.MatchingExt_BeginFire
        };
        uint.TryParse(Singleton<CTextManager>.instance.GetText("MapID_PVP_Fire"), out uieventPars.m_eventParams.tagUInt);
        NewbieBannerIntroDialog.Show(new string[] { string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Newbie_Dir, "huokeng1"), string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Newbie_Dir, "huokeng2"), string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Newbie_Dir, "huokeng3") }, 3, uieventPars, null, null, true);
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
}

