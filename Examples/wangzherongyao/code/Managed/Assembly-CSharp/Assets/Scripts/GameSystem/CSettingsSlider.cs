namespace Assets.Scripts.GameSystem
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    internal class CSettingsSlider
    {
        [CompilerGenerated]
        private static SliderValueChanged <>f__am$cache8;
        private GameObject m_BarObj;
        private int m_DescribeCount;
        private Text[] m_Describes;
        private Text m_Handletext;
        private Slider m_Slider;
        private enSliderKind m_SliderKind;
        private int m_value;
        public SliderValueChanged onValueChangedHander;

        public CSettingsSlider(SliderValueChanged valueChangeDelegate)
        {
            if (<>f__am$cache8 == null)
            {
                <>f__am$cache8 = new SliderValueChanged(CSettingsSlider.<CSettingsSlider>m__60);
            }
            this.onValueChangedHander = <>f__am$cache8;
            this.onValueChangedHander = (SliderValueChanged) Delegate.Combine(this.onValueChangedHander, valueChangeDelegate);
        }

        [CompilerGenerated]
        private static void <CSettingsSlider>m__60(int, enSliderKind)
        {
        }

        public void initPanel(GameObject barObj, enSliderKind kind)
        {
            this.m_BarObj = barObj;
            this.m_SliderKind = kind;
            this.m_DescribeCount = barObj.transform.Find("Slider/Background").childCount;
            this.m_Describes = new Text[this.m_DescribeCount];
            for (int i = 0; i < this.m_DescribeCount; i++)
            {
                this.m_Describes[i] = barObj.transform.Find(string.Format("Slider/Background/Text{0}", i + 1)).GetComponent<Text>();
            }
            this.m_Handletext = this.m_BarObj.transform.Find("Slider/Handle Slide Area/Handle/Text").GetComponent<Text>();
            this.m_Slider = this.m_BarObj.transform.Find("Slider").GetComponent<Slider>();
            this.m_Slider.onValueChanged.RemoveAllListeners();
            this.m_Slider.onValueChanged.AddListener(new UnityAction<float>(this.onSliderChange));
        }

        public void onSliderChange(float value)
        {
            this.value = (int) value;
            this.onValueChangedHander(this.value, this.m_SliderKind);
        }

        public bool Enabled
        {
            get
            {
                return ((this.m_Slider != null) && this.m_Slider.interactable);
            }
            set
            {
                if (this.m_Slider != null)
                {
                    this.m_Slider.interactable = value;
                }
            }
        }

        public int MaxValue
        {
            get
            {
                if (this.m_Slider != null)
                {
                    return (int) this.m_Slider.maxValue;
                }
                return 0;
            }
        }

        public int value
        {
            get
            {
                if (this.m_Slider != null)
                {
                    return (int) this.m_Slider.value;
                }
                return -1;
            }
            set
            {
                this.m_value = value;
                if (this.m_Slider != null)
                {
                    this.m_Slider.value = this.m_value;
                    this.m_Handletext.text = this.m_Describes[this.m_value].text;
                }
            }
        }
    }
}

