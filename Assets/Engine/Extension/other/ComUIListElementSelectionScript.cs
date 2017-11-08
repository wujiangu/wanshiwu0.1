namespace Scripts.UI
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class ComUIListElementSelectionScript : MonoBehaviour,IPointerDownHandler, IEventSystemHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerUpHandler
    {
        public void SetBelongedList(ComUIListScript belongedList,int indexInList)
        {
            m_belongedList = belongedList;
            m_indexInList = indexInList;
        }

        protected ComUIListScript m_belongedList;
        protected int             m_indexInList;

        private const float c_clickAreaValue = 40f;
        private const float c_holdTimeValue = 1f;
        private bool m_canClick;
        [HideInInspector]
        private bool m_isDown;
        private bool m_isHold;
        private bool m_needClearInputStatus;
       
        //public OnUIEventHandler onClick;

        public bool ClearInputStatus()
        {
            this.m_needClearInputStatus = true;
            return this.m_isDown;
        }

       
        public void ExecuteInputStatus()
        {
            this.m_isDown = false;
            this.m_isHold = false;
            this.m_canClick = false;
        }


        private void LateUpdate()
        {
            if (this.m_needClearInputStatus)
            {
                this.ExecuteInputStatus();
                this.m_needClearInputStatus = false;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            /*
            if ((this.m_canClick && (base.m_belongedFormScript != null)) && (base.m_belongedFormScript.ChangeScreenValueToForm(Vector2.Distance(eventData.position, this.m_downPosition)) > 40f))
            {
                this.m_canClick = false;
            }
            */
            this.m_canClick = false;

            if ( m_belongedList != null  &&  m_belongedList.m_scrollRect != null )
            {
                m_belongedList.m_scrollRect.OnBeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            /*
            if ((this.m_canClick && (base.m_belongedFormScript != null)) && (base.m_belongedFormScript.ChangeScreenValueToForm(Vector2.Distance(eventData.position, this.m_downPosition)) > 40f))
            {
                this.m_canClick = false;
            }
             */
            this.m_canClick = false;

            if ( m_belongedList != null  &&  m_belongedList.m_scrollRect != null )
            {
                m_belongedList.m_scrollRect.OnDrag(eventData);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            //this.DispatchUIEvent(enUIEventType.Drop, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            /* 
            if ((this.m_canClick && (base.m_belongedFormScript != null)) && (base.m_belongedFormScript.ChangeScreenValueToForm(Vector2.Distance(eventData.position, this.m_downPosition)) > 40f))
            {
                this.m_canClick = false;
            }
            this.DispatchUIEvent(enUIEventType.DragEnd, eventData);
            */
            this.m_canClick = false;

            if ( m_belongedList != null  &&  m_belongedList.m_scrollRect != null )
            {
                m_belongedList.m_scrollRect.OnEndDrag(eventData);
            }

            this.ClearInputStatus();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (this.m_canClick)
            {
                if ((m_belongedList != null) && (m_indexInList >= 0))
                {
                    m_belongedList.SelectElement(m_indexInList, true);
                }
                /*
                this.DispatchUIEvent(enUIEventType.Click, eventData);
                if (this.m_closeFormWhenClicked && (base.m_belongedFormScript != null))
                {
                    base.m_belongedFormScript.Close();
                }
                */
            }
            this.ClearInputStatus();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            this.m_isDown = true;
            this.m_isHold = false;
            this.m_canClick = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            this.ClearInputStatus();
        }       
        //public delegate void OnUIEventHandler(CUIEvent uiEvent);
    }
}

