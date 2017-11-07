using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace cs
{
    class MainScene : Scene
    {
        void drawFloor(Vector2 vec)
        {
            MapBasicData.ReadFloorFile(m_ftList);

            int num = 0;
            Vector2 temp = Camera.main.ScreenToWorldPoint(vec);
            for (int i = 0; i < m_nCol; i++)
            {
                for (int j = 0; j < m_nRaw; j++)
                {
                    string str = MapBasicData.GetFloorRes(m_ftList[num]);
                    GameObject obj = Instantiate(Resources.Load(str)) as GameObject;
                    obj.transform.SetParent(m_build.transform, true);
                    obj.transform.position = new Vector2(temp.x + j * 0.355f + i * 0.355f, temp.y + j * 0.205f - i * 0.205f);
                    MapBasicData.map_list[num] = obj.transform.position;
                    num++;
                }
            }
        }

        void drawLine(Vector2 vcRaw, Vector2 vcCol)
        {
            Vector2 tempRaw = Camera.main.ScreenToWorldPoint(vcRaw);
            Vector2 tempCol = Camera.main.ScreenToWorldPoint(vcCol);
            for (int i = 0; i < m_nRawLine; i++)
            {
                GameObject obj = Instantiate(Resources.Load("Main/Prefab/other/rawLine")) as GameObject;
                obj.transform.SetParent(m_build.transform, true);
                obj.transform.position = new Vector2(tempRaw.x + i * 0.355f, tempRaw.y + i * 0.205f);
                obj.SetActive(false);
                m_rawList.Add(obj);
            }

            for (int i = 0; i < m_nColLine; i++)
            {
                GameObject obj = Instantiate(Resources.Load("Main/Prefab/other/colLine")) as GameObject;
                obj.transform.SetParent(m_build.transform, true);
                obj.transform.position = new Vector2(tempCol.x + i * 0.355f, tempCol.y - i * 0.205f);
                obj.SetActive(false);
                m_colList.Add(obj);
            }
        }

        void drawHouse()
        {
            m_gtdList = MapBasicData.ReadHouseData();
            for (int i = 0; i < m_gtdList.Count; i++)
            {
                m_buildManager.CreateHouseObject(m_gtdList[i].name, m_gtdList[i].nId, m_gtdList[i].vec);
            }
        }

        public void Create(mapData md)
        {
            if (m_build == null)
                m_build = new GameObject();
            m_build.name = "buildMap";

            m_buildManager = new BuildManager(m_build, m_giList, m_gtdList);

            drawFloor(md.srcMap);
            drawLine(md.srcRaw, md.srcCol);
            drawHouse();
        }

        public void Close()
        {
            m_build = GameObject.Find("buildMap");
            Destroy(m_build);
            m_build = null;
        }

        public void LineActiveStatus(bool isActive)
        {
            GameObject obj = null;
            for (int i = 0; i < m_nColLine; i++)
            {
                obj = m_rawList[i];
                obj.SetActive(isActive);
                obj = m_colList[i];
                obj.SetActive(isActive);
            }
        }

        private bool isCreate = false;
        private GameObject m_build;

        //line
        private List<GameObject> m_rawList = new List<GameObject>();
        private List<GameObject> m_colList = new List<GameObject>();
        private List<int> m_ftList = new List<int>();
        private List<GoodsTempData> m_gtdList = new List<GoodsTempData>();
        private List<GoodsInfo> m_giList = new List<GoodsInfo>();

        /* int */
        protected int m_nRaw = 80;
        protected int m_nCol = 80;
        protected int m_nRawLine = 81;
        protected int m_nColLine = 81;

        BuildManager m_buildManager;
    }
}
