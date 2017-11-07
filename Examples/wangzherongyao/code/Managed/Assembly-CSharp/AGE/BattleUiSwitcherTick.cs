namespace AGE
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameSystem;
    using Assets.Scripts.UI;
    using System;
    using UnityEngine;

    [EventCategory("MMGame/Drama")]
    public class BattleUiSwitcherTick : TickEvent
    {
        public bool bIncludeBattleHero;
        public bool bIncludeBattleUi = true;
        public bool bIncludeFpsForm;
        public bool bOpenOrClose;

        public override BaseEvent Clone()
        {
            BattleUiSwitcherTick tick = ClassObjPool<BattleUiSwitcherTick>.Get();
            tick.CopyData(this);
            return tick;
        }

        protected override void CopyData(BaseEvent src)
        {
            base.CopyData(src);
            BattleUiSwitcherTick tick = src as BattleUiSwitcherTick;
            this.bOpenOrClose = tick.bOpenOrClose;
            this.bIncludeBattleUi = tick.bIncludeBattleUi;
            this.bIncludeBattleHero = tick.bIncludeBattleHero;
            this.bIncludeFpsForm = tick.bIncludeFpsForm;
        }

        public override void Process(AGE.Action _action, Track _track)
        {
            if (this.bIncludeBattleUi)
            {
                CUIFormScript formScript = Singleton<CBattleSystem>.GetInstance().m_FormScript;
                if (formScript != null)
                {
                    if (this.bOpenOrClose)
                    {
                        formScript.Appear();
                        if (GameSettings.EnableOutline)
                        {
                            Transform transform = Camera.main.transform.Find(Camera.main.name + " particles");
                            if (null != transform)
                            {
                                Camera component = transform.GetComponent<Camera>();
                                if (null != component)
                                {
                                    string[] layerNames = new string[] { "3DUI" };
                                    component.cullingMask |= LayerMask.GetMask(layerNames);
                                }
                            }
                        }
                        else
                        {
                            Camera main = Camera.main;
                            string[] textArray2 = new string[] { "3DUI" };
                            main.cullingMask |= LayerMask.GetMask(textArray2);
                        }
                    }
                    else
                    {
                        formScript.Hide(true);
                        if (GameSettings.EnableOutline)
                        {
                            Transform transform2 = Camera.main.transform.Find(Camera.main.name + " particles");
                            if (null != transform2)
                            {
                                Camera camera2 = transform2.GetComponent<Camera>();
                                if (null != camera2)
                                {
                                    string[] textArray3 = new string[] { "3DUI" };
                                    camera2.cullingMask &= ~LayerMask.GetMask(textArray3);
                                }
                            }
                        }
                        else
                        {
                            Camera camera3 = Camera.main;
                            string[] textArray4 = new string[] { "3DUI" };
                            camera3.cullingMask &= ~LayerMask.GetMask(textArray4);
                        }
                    }
                }
            }
            if (this.bIncludeBattleHero)
            {
                CUIFormScript script2 = Singleton<CBattleHeroInfoPanel>.GetInstance().m_FormScript;
                if (script2 != null)
                {
                    if (this.bOpenOrClose)
                    {
                        script2.Appear();
                    }
                    else
                    {
                        script2.Hide(true);
                    }
                }
            }
            if (this.bIncludeFpsForm)
            {
                CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(CUICommonSystem.FPS_FORM_PATH);
                if (form != null)
                {
                    if (this.bOpenOrClose)
                    {
                        form.Appear();
                    }
                    else
                    {
                        form.Hide(true);
                    }
                }
            }
        }

        public override bool SupportEditMode()
        {
            return true;
        }
    }
}

