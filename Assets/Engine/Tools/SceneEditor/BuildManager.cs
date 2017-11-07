using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public BuildManager(GameObject parent,List<GoodsInfo> giList, List<GoodsTempData> gtdList, List<GameObject>floorObjList=null, List<int>ftList=null)
    {
        m_giList = giList;
        m_gtdList = gtdList;
        m_ftList = ftList;
        m_floorObjList = floorObjList;
        m_parent = parent;
    }

    public void CreateHouseObject(string name, int nId, Vector2 vec)
    {
        string str = "Main/Prefab/goods/" + name;
        GameObject obj = Instantiate(Resources.Load(str)) as GameObject;
        obj.transform.position = vec;
        obj.transform.SetParent(m_parent.transform, true);
        GoodsMove gm = (GoodsMove)obj.GetComponent<GoodsMove>();
        gm.InitData(name, nId, this);
    }

    float GetHypeLength(Vector2 vec)
    {
        float angle = 0.0f;
        float distanceX = Mathf.Abs(-11.36F - vec.x);
        float distanceY = Mathf.Abs(6.4f - vec.y);
        angle = Mathf.Pow(distanceX, 2) + Mathf.Pow(distanceY, 2);
        return angle;
    }

    void swap(int i, int j)
    {
        GoodsInfo gi = m_giList[i];
        m_giList[i] = m_giList[j];
        m_giList[j] = gi;
    }

    void houseLayerSort()
    {

        for (int i = 0; i < m_giList.Count; i++)
        {
            for (int j = i + 1; j < m_giList.Count; j++)
            {
                if (m_giList[i].hypeLength > m_giList[j].hypeLength)
                {
                    if (m_giList[i].mesh_list[0].y < m_giList[j].mesh_list[0].y)
                    {
                        swap(i, j);
                    }
                }
                else
                {
                    if (m_giList[i].mesh_list[0].y <= m_giList[j].mesh_list[0].y)
                    {
                        HouseData hd = MapBasicData.GetHouseData(m_giList[i].gt);
                        float w = hd.width;
                        float h = hd.height - 0.4f;
                        if (m_giList[i].mesh_list[0].x + w >= m_giList[j].mesh_list[0].x &&
                            m_giList[i].mesh_list[0].y + h >= m_giList[j].mesh_list[0].y)
                        {
                            swap(i, j);
                        }
                    }
                }
            }
        }
    }

    public void ChangeObjSort(Vector2[] vc_list, Vector2 vec, int nId)
    {
        for (int i = 0; i < m_giList.Count; i++)
        {
            if (nId == m_giList[i].nID)
            {
                GoodsInfo gi = m_giList[i];
                gi.mesh_list = vc_list;
                gi.hypeLength = GetHypeLength(vc_list[0]);
                gi.vec = vec;
                m_giList[i] = gi;
                break;
            }
        }

        houseLayerSort();
        m_gtdList.Clear();

        for (int i = 0; i < m_giList.Count; i++)
        {
            m_giList[i].obj.transform.GetComponent<Renderer>().sortingOrder = i;
            GoodsTempData gtd = MapBasicData.GetGoodsTempData(m_giList[i].nID, m_giList[i].name, m_giList[i].vec);
            m_gtdList.Add(gtd);
        }
    }

    public void AddGoods(int nId, GoodsType goodsType, GameObject obj, Vector2[] vc_list, Vector2 oldMesh, string name)
    {
        GoodsInfo gi = new GoodsInfo();
        gi.nID = nId;
        gi.gt = goodsType;
        gi.vec = obj.transform.position;
        gi.obj = obj;
        gi.mesh_list = vc_list;
        gi.oldMesh = oldMesh;
        gi.hypeLength = GetHypeLength(vc_list[0]);
        gi.name = name;
        m_giList.Add(gi);
    }

    public void changeFloorType(int nIndex, int ft)
    {
        GameObject obj = Instantiate(Resources.Load(MapBasicData.GetFloorRes(ft))) as GameObject;
        obj.transform.SetParent(m_parent.transform, true);
        obj.transform.position = m_floorObjList[nIndex].transform.position;
        obj.transform.GetComponent<Renderer>().sortingOrder = 0;
        DestroyImmediate(m_floorObjList[nIndex]);
        m_floorObjList[nIndex] = obj;
        m_ftList[nIndex] = ft;
    }

    public void CheckDifferentFloor(int ft, Vector2 vc)
    {
        for (int i = 0; i < m_floorObjList.Count; i++)
        {
            GameObject tempObj = m_floorObjList[i];
            if (m_ftList[i] != ft)
            {
                if (Mathf.Abs(vc.x - tempObj.transform.position.x) <= 0.355f &&
                Mathf.Abs(vc.y - tempObj.transform.position.y) <= 0.205f)
                {
                    changeFloorType(i, ft);
                }
            }
        }
    }

    public List<GoodsInfo> GoodsList { get { return m_giList; } }
    public List<int> FloorTypeList { get { return m_ftList; } }

    private List<GoodsInfo> m_giList;
    private List<GameObject> m_floorObjList;
    private List<int> m_ftList;
    private List<GoodsTempData> m_gtdList;
    private GameObject m_parent;
}