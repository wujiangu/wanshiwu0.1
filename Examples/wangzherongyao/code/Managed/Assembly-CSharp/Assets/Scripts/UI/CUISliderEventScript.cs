namespace Assets.Scripts.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class CUISliderEventScript : CUIComponent
    {
        [HideInInspector]
        public enUIEventID m_onValueChangedEventID;
        private Slider m_slider;

        public override void Initialize(CUIFormScript formScript)
        {
            if (!base.m_isInitialized)
            {
                this.m_slider = base.gameObject.GetComponent<Slider>();
                this.m_slider.onValueChanged.RemoveAllListeners();
                this.m_slider.onValueChanged.AddListener(new UnityAction<float>(this.OnSliderValueChanged));
                base.Initialize(formScript);
            }
        }

        private void OnSliderValueChanged(float value)
        {
            if (this.m_onValueChangedEventID != enUIEventID.None)
            {
                CUIEvent uIEvent = Singleton<CUIEventManager>.GetInstance().GetUIEvent();
                uIEvent.m_srcFormScript = base.m_belongedFormScript;
                uIEvent.m_srcWidget = base.gameObject;
                uIEvent.m_srcWidgetScript = this;
                uIEvent.m_srcWidgetBelongedListScript = base.m_belongedListScript;
                uIEvent.m_srcWidgetIndexInBelongedList = base.m_indexInlist;
                uIEvent.m_pointerEventData = null;
                uIEvent.m_eventID = this.m_onValueChangedEventID;
                uIEvent.m_eventParams.sliderValue = (int) value;
                base.DispatchUIEvent(uIEvent);
            }
        }
    }
}

