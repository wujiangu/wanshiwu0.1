using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;

namespace cs
{
    public enum EGuiLayer
    {
        Bottom = 1,
        Low,
        Middle,
        High,
        Top,
        TopMost,
    }

    /// <summary>
    /// Gui对象
    /// 实现了控件自动绑定GuiBinder，可以直接获取到需要的GuiControl
    /// 所以，为了节约性能，就不再去管理所包含的的所有GuiControl
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class GuiObject : MonoBehaviour
    {
        public string Name
        {
            get { return m_strName; }
        }

        public bool Visible
        {
            get { return m_bVisible; }
        }

        public ComGuiBinderBase GuiBinder
        {
            get { return m_comGuiBinder; }
        }

        public EGuiState State
        {
            get { return m_guiState; }
        }

        public EGuiLayer GuiLayer
        {
            get { return m_eGuiLayer; }
        }

        public int OpenOrder
        {
            get { return m_nOpenOrder; }
        }

        public bool Initialize(string a_strName)
        {
            m_comGuiBinder = Utility.FindCompoent<ComGuiBinderBase>(gameObject, string.Empty);

            m_canvas = Utility.FindCompoent<Canvas>(gameObject, string.Empty);
            if (m_canvas == null)
            {
                m_canvas = gameObject.AddComponent<Canvas>();
            }
            m_canvas.overrideSorting = true;
            m_canvas.sortingLayerID = SortingLayer.NameToID("Default");
            m_canvas.sortingOrder = 0;

            m_raycaster = Utility.FindCompoent<GraphicRaycaster>(gameObject, string.Empty);
            if (m_raycaster == null)
            {
                m_raycaster = gameObject.AddComponent<GraphicRaycaster>();
            }

            m_arrGuiControls = gameObject.GetComponentsInChildren<GuiControl>();
            for (int i = 0; i < m_arrGuiControls.Length; ++i)
            {
                m_arrGuiControls[i].Initialize();
            }

            m_guiEffectPlayer.Initialize(gameObject);

            _SetVisible(false);
            m_guiState = EGuiState.Closed;
            m_strName = a_strName;

            return true;
        }

        public void Clear()
        {
            m_guiEffectPlayer.Clear();

            for (int i = 0; i < m_arrGuiControls.Length; ++i)
            {
                m_arrGuiControls[i].Clear();
            }
            m_arrGuiControls = null;
        }

        public void Show(bool a_bPlayFadeInAnim = false)
        {
            if (m_guiState == EGuiState.Closed)
            {
                _SetVisible(true);

                if (a_bPlayFadeInAnim && m_guiEffectPlayer != null)
                {
                    if (
                        m_guiEffectPlayer != null &&
                        m_guiEffectPlayer.PlayFadeInEffect(DirectorUpdateMode.GameTime, _OnFadeInFinished)
                        )
                    {
                        m_guiState = EGuiState.FadeIn;
                    }
                    else
                    {
                        Logger.Error("打开UI:{0}，播放淡入动画失败", m_strName);
                        m_guiState = EGuiState.Opened;
                    }
                }
                else
                {
                    m_guiState = EGuiState.Opened;
                }
            }
        }

        public void Hide(bool a_bPlayFadeOutAnim = false)
        {
            if (m_guiState == EGuiState.Opened)
            {
                if (a_bPlayFadeOutAnim)
                {
                    if (
                        m_guiEffectPlayer != null &&
                        m_guiEffectPlayer.PlayFadeOutEffect(DirectorUpdateMode.GameTime, _OnFadeOutFinished)
                        )
                    {
                        m_guiState = EGuiState.FadeOut;
                    }
                    else
                    {
                        Logger.Error("关闭UI:{0}，播放淡出动画失败", m_strName);
                        _SetVisible(false);
                        m_guiState = EGuiState.Closed;
                    }
                }
                else
                {
                    _SetVisible(false);
                    m_guiState = EGuiState.Closed;
                }
            }
        }

        public GuiControl GetControl(int a_nID)
        {
            for (int i = 0; i < m_arrGuiControls.Length; ++i)
            {
                if (m_arrGuiControls[i].ID == a_nID)
                {
                    return m_arrGuiControls[i];
                }
            }
            return null;
        }

        public void SetOpenLayer(EGuiLayer a_eLayer)
        {
            m_eGuiLayer = a_eLayer;
            _SetSortingOrder(_CalculateSortingOrder());
        }

        public void SetOpenOrder(int a_nOrder)
        {
            if (a_nOrder >= ms_nMaxOpenOrder || a_nOrder < 1)
            {
                Logger.Error("设置UI打开顺序，错误！顺序：{0}", a_nOrder);
                return;
            }

            m_nOpenOrder = a_nOrder;
            _SetSortingOrder(_CalculateSortingOrder());
        }

        public void MoveToFront()
        {
            int nOrder = GuiManager.Get().GetMaxOrderInLayer(m_eGuiLayer) + 1;

            if (nOrder >= ms_nMaxOpenOrder)
            {
                GuiManager.Get().ResetOrderInLayer(m_eGuiLayer);
                nOrder = GuiManager.Get().GetMaxOrderInLayer(m_eGuiLayer) + 1;
            }

            SetOpenOrder(nOrder);
        }

        public void MoveToFront(EGuiLayer a_eGuiLayer)
        {
            m_eGuiLayer = a_eGuiLayer;
            MoveToFront();
        }

        private void _SetVisible(bool a_bVisible)
        {
            m_bVisible = a_bVisible;
            m_canvas.enabled = m_bVisible;
            m_raycaster.enabled = m_bVisible;

            if (a_bVisible)
            {
                for (int i = 0; i < m_arrGuiControls.Length; ++i)
                {
                    m_arrGuiControls[i].Show();
                }
            }
            else
            {
                for (int i = 0; i < m_arrGuiControls.Length; ++i)
                {
                    m_arrGuiControls[i].Hide();
                }
            }
        }

        private void _OnFadeOutFinished(string a_strEffectName)
        {
            _SetVisible(false);
            m_guiState = EGuiState.Closed;
        }

        private void _OnFadeInFinished(string a_strEffectName)
        {
            m_guiState = EGuiState.Opened;
        }

        private int _CalculateSortingOrder()
        {
            return (int)m_eGuiLayer * ms_nMaxOpenOrder + m_nOpenOrder;
        }
        
        private void _SetSortingOrder(int a_nOrder)
        {
            if (m_canvas != null)
            {
                m_canvas.sortingOrder = a_nOrder;
                for (int i = 0; i < m_arrGuiControls.Length; i++)
                {
                    m_arrGuiControls[i].SetSortingOrder(a_nOrder);
                }
            }
        }

        /// <summary>
        /// UI效果播放器
        /// </summary>
        [SerializeField]
        private GuiEffectPlayer m_guiEffectPlayer = null;

        //[SerializeField]
        private GuiControl[] m_arrGuiControls = null;

        private string m_strName = string.Empty;
        private EGuiState m_guiState = EGuiState.Invalid;
        private ComGuiBinderBase m_comGuiBinder = null;
        private bool m_bVisible = false;
        private Canvas m_canvas = null;
        private GraphicRaycaster m_raycaster = null;
        private EGuiLayer m_eGuiLayer = EGuiLayer.Middle;
        private int m_nOpenOrder = 0;
        private static int ms_nMaxOpenOrder = 1000;
    }

}
