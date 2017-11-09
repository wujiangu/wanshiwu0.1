using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using System.Collections.Generic;
using UnityEngine.Timeline;
using UnityEngine.Events;
using UnityEngine.UI;

namespace cs
{
    public enum EGuiState
    {
        /// <summary>
        /// 非法，未初始化
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// 关闭
        /// </summary>
        Closed,

        /// <summary>
        /// 淡入
        /// </summary>
        FadeIn,

        /// <summary>
        /// 打开
        /// </summary>
        Opened,

        /// <summary>
        /// 淡出
        /// </summary>
        FadeOut,
    }

    [ExecuteInEditMode]
    public class GuiControl : MonoBehaviour
    {
        /// <summary>
        /// UI效果播放器
        /// </summary>
        [SerializeField]
        GuiEffectPlayer m_guiEffectPlayer;

        [SerializeField]
        int m_nSortingOrderOffset;
        public int SortingOrderOffset
        {
            get { return m_nSortingOrderOffset; }
            protected set { m_nSortingOrderOffset = value; }
        }


        public EGuiState guiState { get; private set; }

        public int ID { get { return m_nID; } }

        public virtual bool Initialize()
        {
            if (m_isInit)
            {
                return false;
            }

            if (gameObject == null)
            {
                return false;
            }

            guiState = EGuiState.Invalid;

            if (m_guiEffectPlayer != null)
            {
                m_guiEffectPlayer.Initialize(gameObject);
            }

            if (gameObject.activeSelf)
            {
                if (m_guiEffectPlayer != null && m_guiEffectPlayer.IsPlaying())
                {
                    guiState = EGuiState.FadeIn;
                }
                else
                {
                    guiState = EGuiState.Opened;
                }
            }
            else
            {
                guiState = EGuiState.Closed;
            }

            m_isInit = true;
            return true;
        }

        public virtual void Clear()
        {
            if (m_guiEffectPlayer != null)
            {
                m_guiEffectPlayer.Clear();
            }
        }

        public void Show(bool a_bPlayFadeInAnim = false)
        {
            if (guiState == EGuiState.Closed)
            {
                gameObject.SetActive(true);

                if (a_bPlayFadeInAnim)
                {
                    if (m_guiEffectPlayer != null)
                    {
                        m_guiEffectPlayer.PlayFadeInEffect(DirectorUpdateMode.GameTime, var =>
                        {
                            guiState = EGuiState.Opened;
                        });
                    }
                }
                else
                {
                    guiState = EGuiState.Opened;
                }
            }
        }

        public void Hide(bool a_bPlayFadeOutAnim = false)
        {

        }

        public virtual void SetSortingOrder(int a_nOrder)
        {

        }

        public GuiEffectPlayer GetGuiEffectPlayer()
        {
            return m_guiEffectPlayer;
        }

        void OnMouseDown()
        {
            
        }

        void OnMouseUp()
        {

        }

        ///////////////////// down cast ////////////////////////////
        public GuiButton ToButton()
        {
            return this as GuiButton;
        }

        public GuiImage ToImage()
        {
            return this as GuiImage;
        }

        public GuiParticleSystem ToParticle()
        {
            return this as GuiParticleSystem;
        }

        public GuiLabel ToLabel()
        {
            return this as GuiLabel;
        }
        ///////////////////////////////////////////////////////////

#if UNITY_EDITOR
        private void Start()
        {
            Initialize();
        }

        bool m_isSelect = false;

        public void setID(int value)
        {
            m_nID = value;
        }
#endif
        [SerializeField]
        private int m_nID = 0;

        private bool m_isInit = false;
    }
}

