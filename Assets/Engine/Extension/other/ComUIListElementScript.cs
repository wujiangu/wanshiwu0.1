namespace Scripts.UI
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class ComUIListElementScript : MonoBehaviour
    {
       
        [HideInInspector]
        public int m_index;
        public stRect m_rect;

        [HideInInspector]
        public System.Object gameObjectBindScript;

        public bool m_autoAddSelectionScript = true;
        private CanvasGroup m_canvasGroup;
        
        [HideInInspector]
        public Vector2 m_defaultSize;

        public bool m_useSetActiveForDisplay;

        [HideInInspector]
        public ComUIListScript m_belongedListScript;
        protected bool m_isInitialized;

        public virtual void ChangeDisplay(bool selected)
        {
            if(m_belongedListScript.onItemChageDisplay != null)
            {
                m_belongedListScript.onItemChageDisplay(this,selected);
            }
        }

        public void Disable()
        {
            if (this.m_useSetActiveForDisplay)
            {
                base.gameObject.SetActive(false);
                //base.gameObject.CustomActive(false);
            }
            else
            {
                this.m_canvasGroup.alpha = 0f;
                this.m_canvasGroup.blocksRaycasts = false;
            }
        }

        public void Enable(ComUIListScript belongedList, int index, string name, ref stRect rect, bool selected)
        {
            m_belongedListScript = belongedList;
            this.m_index = index;
            base.gameObject.name = name + "_" + index.ToString();
            if (this.m_useSetActiveForDisplay)
            {
                base.gameObject.SetActive(true);
                //gameObject.CustomActive(true);
            }
            else
            {
                this.m_canvasGroup.alpha = 1f;
                this.m_canvasGroup.blocksRaycasts = true;
            }
            this.SetComponentBelongedList(base.gameObject);
            this.SetRect(ref rect);
            this.ChangeDisplay(selected);
        }

        protected virtual Vector2 GetDefaultSize()
        {
            return new Vector2(((RectTransform) base.gameObject.transform).rect.width, ((RectTransform) base.gameObject.transform).rect.height);
        }

        public  void Initialize()
        {
            if (!m_isInitialized)
            {
                m_isInitialized = true;

                if (this.m_autoAddSelectionScript && (base.gameObject.GetComponent<ComUIListElementSelectionScript>() == null))
                {
                    base.gameObject.AddComponent<ComUIListElementSelectionScript>();
                }
                if (!this.m_useSetActiveForDisplay)
                {
                    this.m_canvasGroup = base.gameObject.GetComponent<CanvasGroup>();
                    if (this.m_canvasGroup == null)
                    {
                        this.m_canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
                    }
                }
                this.m_defaultSize = this.GetDefaultSize();
                this.InitRectTransform();
            }
        }

        private void InitRectTransform()
        {
            RectTransform transform = base.gameObject.transform as RectTransform;
            transform.anchorMin = new Vector2(0f, 1f);
            transform.anchorMax = new Vector2(0f, 1f);
            transform.pivot = new Vector2(0f, 1f);
            transform.sizeDelta = this.m_defaultSize;
            transform.localRotation = Quaternion.identity;
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        public void OnSelected(BaseEventData baseEventData)
        {
            m_belongedListScript.SelectElement(this.m_index, true);
        }

        public void SetComponentBelongedList(GameObject gameObject)
        {
            
            var components = cs.ListPool<Component>.Get();
            
            gameObject.GetComponents(typeof(ComUIListElementSelectionScript),components);
            if ((components != null) && (components.Count > 0))
            {
                for (int j = 0; j < components.Count; j++)
                {
                    var current = components[j] as ComUIListElementSelectionScript;
                    current.SetBelongedList(m_belongedListScript, this.m_index);
                }
            }
            cs.ListPool<Component>.Release(components);

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                this.SetComponentBelongedList(gameObject.transform.GetChild(i).gameObject);
            }
        }

        public void SetRect(ref stRect rect)
        {
            this.m_rect = rect;
            RectTransform transform = base.gameObject.transform as RectTransform;
            transform.sizeDelta = new Vector2((float) this.m_rect.m_width, (float) this.m_rect.m_height);
            transform.anchoredPosition = new Vector2((float) rect.m_left, (float) rect.m_top);
        }

        public delegate void OnSelectedDelegate();
    }
}

