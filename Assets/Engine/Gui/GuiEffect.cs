using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Events;
using UnityEngine;

namespace cs
{
    public class GuiEffectFinishedEvent : UnityEvent<string> { }

    [System.Serializable]
    public class GuiEffect
    {
        /// <summary>
        /// UI效果名称，用于同一个效果播放器内，唯一标识
        /// </summary>
        public string strName;
        
        /// <summary>
        /// timeline资源
        /// </summary>
        public TimelineAsset timelineAsset;

        GuiEffectFinishedEvent m_onFinished = new GuiEffectFinishedEvent();
        /// <summary>
        /// 播放完成回调
        /// </summary>
        public GuiEffectFinishedEvent onFinished { get { return m_onFinished; } }

        public bool IsValid()
        {
            return timelineAsset != null && string.IsNullOrEmpty(strName) == false;
        }
    }
}

