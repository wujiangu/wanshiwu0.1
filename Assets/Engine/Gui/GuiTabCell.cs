using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR  
using UnityEditor;//自动调用OnValidate()方法  
#endif

namespace cs
{
    public class GuiTabCell
    {
        /// <summary>标签的页</summary>
        public int idx { get; set; }
        /// <summary>对象</summary>
        public GameObject activeObj;
        public GameObject forbidObj;

        /// <summary>标签的节点</summary>
        protected GameObject _node;

        protected GameObject tabCell;

        public GameObject node
        {
            get { return _node; }
            set
            {
                _node = value;
                RectTransform rtran = _node.GetComponent<RectTransform>();
                if (rtran == null)
                {
                    rtran = _node.AddComponent<RectTransform>();
                }
                rtran.pivot = new Vector2(0.5f, 0.5f);
                rtran.anchorMin = new Vector2(0.5f, 0.5f);
                rtran.anchorMax = new Vector2(0.5f, 0.5f);
            }
        }

        public Button btn;
    }
}

