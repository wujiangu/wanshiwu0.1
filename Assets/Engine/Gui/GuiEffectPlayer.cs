using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Playables;
using System.Collections.Generic;
using UnityEngine.Timeline;
using UnityEngine.Events;

namespace cs
{
    [Serializable]
    public class GuiEffectPlayer : IUpdate, IGuiEffectPlayer
    {
        public GuiEffect fadeInEffect;
        public GuiEffect fadeOutEffect;
        //[Reorderable("Effects")]
        public List<GuiEffect> commonEffects = new List<GuiEffect>();

        public const string s_strFadeIn = "fadeIn";
        public const string s_strFadeOut = "fadeOut";

        GameObject m_objOwner = null;
        GuiEffect m_currEfect = null;
        PlayableDirector m_director = null;


        public bool IsValid()
        {
            return (fadeInEffect != null && fadeInEffect.IsValid()) || 
                (fadeOutEffect != null && fadeOutEffect.IsValid())|| 
                (commonEffects != null && commonEffects.Count > 0);
        }

        public bool Initialize(GameObject a_objOwner)
        {
            if (a_objOwner == null)
            {
                return false;
            }

            m_objOwner = a_objOwner;

            if (IsValid())
            {
                m_director = m_objOwner.GetComponent<PlayableDirector>();
                if (m_director == null)
                {
                    m_director = m_objOwner.AddComponent<PlayableDirector>();
                    m_director.Stop();
                }
            }

            m_currEfect = null;

            return true;
        }

        public void Clear()
        {
            m_currEfect = null;
            m_objOwner = null;
            m_director = null;
        }

        public bool PlayFadeInEffect(DirectorUpdateMode a_eUpdateMode, UnityAction<string> a_onFinished = null)
        {
            if (fadeInEffect != null && fadeInEffect.IsValid())
            {
                return _PlayEffect(fadeInEffect, DirectorWrapMode.None, a_eUpdateMode, a_onFinished);
            }
            return false;
        }

        public bool PlayFadeOutEffect(DirectorUpdateMode a_eUpdateMode, UnityAction<string> a_onFinished = null)
        {
            if (fadeOutEffect != null && fadeOutEffect.IsValid())
            {
                return _PlayEffect(fadeOutEffect, DirectorWrapMode.None, a_eUpdateMode, a_onFinished);
            }
            return false;
        }

        public void PlayEffect(string a_strEffectName, DirectorWrapMode a_eWrapMode, DirectorUpdateMode a_eUpdateMode, UnityAction<string> a_onFinished = null)
        {
            GuiEffect guiEffect = null;
            for (int i = 0; i < commonEffects.Count; ++i)
            {
                if (commonEffects[i].strName == a_strEffectName)
                {
                    guiEffect = commonEffects[i];
                    break;
                }
            }

            if (guiEffect == null)
            {
                return;
            }

            _PlayEffect(guiEffect, a_eWrapMode, a_eUpdateMode, a_onFinished);
        }


        public void PauseEffect()
        {
            if (m_currEfect != null)
            {
                m_director.Pause();
            }
        }

        public void ResumeEffect()
        {
            if (m_currEfect != null)
            {
                m_director.Resume();
            }
        }

        public void StopEffect()
        {
            if (m_currEfect != null)
            {
                m_director.Stop();
                m_currEfect = null;
            }
        }

        public bool IsPlaying()
        {
            if (m_director != null && m_director.state == PlayState.Playing)
            {
                return true;
            }
            return false;
        }

        public void Tick(float a_fElapsed)
        {
            if (m_currEfect != null)
            {
                if (m_director.time >= m_director.duration &&
                    m_director.state == PlayState.Paused)
                {
                    m_director.Stop();
                    if (m_currEfect.onFinished != null)
                    {
                        m_currEfect.onFinished.Invoke(m_currEfect.strName);
                        m_currEfect.onFinished.RemoveAllListeners();
                    }
                    m_currEfect = null;
                }
            }
        }

        protected bool _PlayEffect(GuiEffect a_guiEffect, DirectorWrapMode a_eWrapMode, DirectorUpdateMode a_eUpdateMode, UnityAction<string> a_onFinished)
        {
            if (a_guiEffect == null || a_guiEffect.timelineAsset == null || m_objOwner.activeInHierarchy == false)
            {
                return false;
            }

            m_director.Stop();

            m_director.extrapolationMode = a_eWrapMode;
            m_director.initialTime = 0;
            m_director.timeUpdateMode = a_eUpdateMode;

            m_director.Play(a_guiEffect.timelineAsset);
            m_currEfect = a_guiEffect;
            m_currEfect.onFinished.AddListener(a_onFinished);

            return true;
        }

        //protected void _OnEffectListAdd(ReorderableList a_reorderableList)
        //{
        //    a_reorderableList.
        //}
    }
}

