using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if UNITY_EDITOR  
using UnityEditor;//自动调用OnValidate()方法  
#endif

namespace cs
{
    /// <summary>标签排布的方向</summary>
    public enum TabDirection
    {
        HORIZONTAL,
        VERTICAL
    }

    /// <summary>
    /// 标签页
    /// </summary>
    public class GuiTabPage : MonoBehaviour
    {
        /// <summary>标签的个数</summary>
        private int cellsCount;
        /// <summary>当前激活的标签</summary>
        public int activeIndex;
        /// <summary>标签的对象</summary>
        private GameObject cell_tbl;
        /// <summary>标签排布的方向</summary>
        public TabDirection direction;

        /// <summary>标签的大小</summary>
        private Vector2 cellsSize;

        /// <summary>标签的回调</summary>
        protected UnityAction<int, GameObject> onCellHandle;
        protected GameObject tabCell;
        protected GameObject list;
        protected List<GuiTabCell> cells;
        private string _tabType;

        public void SetCellHandle(UnityAction<int,GameObject> call)
        {
            onCellHandle = call;
        }

        public void SetActiveIndex(int index)
        {
            activeIndex = index;
            if (_tabType == "DefaultBtn")
            {
                for (int idx = 0; idx < cells.Count; idx++)
                {
                    if (activeIndex == cells[idx].idx)
                        cells[idx].btn.interactable = false;
                    else
                        cells[idx].btn.interactable = true;
                }
            }
            else
            {
                for (int idx = 0; idx < cells.Count; idx++)
                {
                    if (activeIndex == cells[idx].idx)
                    {
                        cells[idx].activeObj.gameObject.SetActive(false);
                        cells[idx].forbidObj.gameObject.SetActive(true);
                    }
                    else
                    {
                        cells[idx].activeObj.gameObject.SetActive(true);
                        cells[idx].forbidObj.gameObject.SetActive(false);
                    }
                }
            }
        }

        void Awake()
        {
            cellsCount = 1;
            cellsSize = Vector2.zero;
            activeIndex = 0;
            _tabType = "";
            cells = new List<GuiTabCell>();
            list = transform.Find("List").gameObject;
            tabCell = transform.Find("TabCell").gameObject;
        }

        void Start()
        {
            GameObject active = tabCell.transform.Find("Active").gameObject;
            if (active.transform.childCount == 0)
            {
                //使用默认的按钮
                _tabType = "DefaultBtn";
                
            }
            else
            {
                //不适用默认的按钮
                _tabType = "TabCell";
            }
            for (int i = 0; i < list.transform.childCount; i++)
            {
                addCell(i);
            }
            cellsCount = list.transform.childCount;
            reloadData(_tabType);
        }

        public void reloadData(string type)
        {
            Transform tran = transform.Find(type);
            if (tran != null)
            {
                tran.gameObject.SetActive(false);
            }
            RectTransform rtran = tran.GetComponent<RectTransform>();
            if (type == "TabCell")
            {
                GameObject active = tabCell.transform.Find("Active").gameObject;
                GameObject child = active.transform.GetChild(0).gameObject;
                rtran = child.transform.GetComponent<RectTransform>();
            }
            cellsSize = rtran.sizeDelta;
            createCells();
            layerUpdate();
            SetActiveIndex(activeIndex);
        }

        public void addCell(int idx)
        {
            GuiTabCell cell = new GuiTabCell();
            cell.idx = idx;
            cell.node = list.transform.GetChild(idx).gameObject;
            if (_tabType == "DefaultBtn")
            {
                cell.btn = cell.node.transform.GetComponent<Button>();
            }
            else
            {
                cell.btn = cell.node.AddComponent<Button>();
                cell.activeObj = cell.node.transform.Find("Active").gameObject;
                cell.forbidObj = cell.node.transform.Find("Forbid").gameObject;
            }
            cells.Add(cell);
            if (onCellHandle != null)
            {
                cell.btn.onClick.AddListener(delegate () {
                    onCellHandle.Invoke(cell.idx, cell.node);
                    SetActiveIndex(cell.idx);
                });
            }
        }

        protected void createCells()
        {
            if (cellsCount == 0)
            {
                return;
            }

        }

        /// <summary>标签的排布</summary>
        private void layerUpdate()
        {
            RectTransform rtran = transform.GetComponent<RectTransform>();
            RectTransform listRtran = list.transform.GetComponent<RectTransform>();
            if (direction == TabDirection.HORIZONTAL)
            {
                //if (list.GetComponent<VerticalLayoutGroup>()) Destroy(list.GetComponent<VerticalLayoutGroup>());
                rtran.sizeDelta = new Vector2(cellsSize.x * cellsCount, cellsSize.y);
                listRtran.sizeDelta = new Vector2(cellsSize.x * cellsCount, cellsSize.y);
                if (list.GetComponent<HorizontalLayoutGroup>() == null)
                {
                    HorizontalLayoutGroup horiz = list.AddComponent<HorizontalLayoutGroup>();
                }
            }
            else
            {
                //if (list.GetComponent<HorizontalLayoutGroup>()) Destroy(list.GetComponent<HorizontalLayoutGroup>());
                rtran.sizeDelta = new Vector2(cellsSize.x, cellsSize.y * cellsCount);
                listRtran.sizeDelta = new Vector2(cellsSize.x, cellsSize.y * cellsCount);
                if (list.GetComponent<VerticalLayoutGroup>() == null)
                {
                    VerticalLayoutGroup ver = list.AddComponent<VerticalLayoutGroup>();
                }
            }
        }
    }

}
