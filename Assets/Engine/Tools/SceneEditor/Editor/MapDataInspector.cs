using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(mapData))]
public class MapDataInspector : Editor
{

    
    public static void CreateMapData()
    {
        cs.Utility.CreateAsset<mapData>("DefaultMap");
    }

    void createFloor(string name)
    {
        string str = "";
        int ft = 0;
        if (name == "ground"){
            str = "Main/Prefab/floor/normal_floor";
            ft = CType.NORMAL_FLOOR;
        } 
        else if (name == "general_floor")
        {
            str = "Main/Prefab/floor/general_floor";
            ft = CType.GENERAL_FLOOR;
        } 
        GameObject obj = Instantiate(Resources.Load(str)) as GameObject;
        GoodsMove gm = (GoodsMove)obj.GetComponent<GoodsMove>();
        obj.transform.localScale = new Vector2(4, 4);
        obj.transform.position = new Vector2(12, 12);
        gm.SetGoodsInfo(obj, GoodsType.FLOOR, ft, m_buildmanager);
    }

    void setMapPos()
    {
        int num = 0;
        Vector2 temp = Camera.main.ScreenToWorldPoint(serial_list[0].vector2Value);
        for (int i = 0; i < m_col; i++)
        {
            for (int j = 0; j < m_raw; j++)
            {
                GameObject tempObj = m_floorObjList[num];
                tempObj.transform.position = new Vector2(temp.x + j * 0.355f + i * 0.355f, temp.y + j * 0.205f - i * 0.205f);
                MapBasicData.map_list[num] = tempObj.transform.position;
                num++;
            }
        }
    }

    void setLinePos()
    {
        Vector2 tempRaw = Camera.main.ScreenToWorldPoint(serial_list[1].vector2Value);
        Vector2 tempCol = Camera.main.ScreenToWorldPoint(serial_list[2].vector2Value);
        for (int i = 0; i < m_rawLine; i++)
        {
            GameObject temp = m_objRawList[i];
            temp.transform.position = new Vector2(tempRaw.x + i * 0.355f, tempRaw.y + i * 0.205f);
        }

        for (int i = 0; i < m_colLine; i++)
        {
            GameObject temp = m_objColList[i];
            temp.transform.position = new Vector2(tempCol.x + i * 0.355f, tempCol.y - i * 0.205f);
        }
    }


    void CheckPos(int index)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serial_list[index], new GUIContent(vc_list[index]));
        if (EditorGUI.EndChangeCheck())
        {
            if (vc_list[index] == "srcMap")
            {
                setMapPos();
            }
            else
            {
                setLinePos();
            }
        }
    }

    void drawVec()
    {
        for (int i = 0; i < vc_list.Length; i++)
        {
            CheckPos(i);
        }
    }

    void drawButton()
    {
        int nCol, nRaw;
        string[] list = new string[] { "table", "highStove", "alloyCabinet", "cabinet", "electricBox",
                                        "electricCabinet", "oxygenBottle","general_floor","ground"};

        for (int i = 0; i < list.Length; i++)
        {
            nRaw = i % 5;
            nCol = (i / 5);
            Texture2D texture = new Texture2D(50, 50);
            string str = "Assets/Resources/Main/Image/build/" + list[i] + ".png";
            texture = (Texture2D)AssetDatabase.LoadAssetAtPath(str, typeof(Texture2D));
            if (GUI.Button(new Rect(5 + nRaw * 80, 130 + 60 * nCol, 50, 50), texture))
            {
                if (list[i] == "general_floor" || list[i] == "ground")
                {
                    createFloor(list[i]);
                }
                else
                {
                    m_buildmanager.CreateHouseObject(list[i], MapBasicData.GetRandId(), new Vector2(0, 0));
                }
            }
        }

        if (GUI.Button(new Rect(350, 60, 50, 30), "保存坐标"))
        {
            MapBasicData.LoadHouseData(m_mapData.GTDList);
            //AssetDatabase.SaveAssets();
        }
    }

    void drawHouse()
    {

        for (int i = 0; i < m_mapData.GTDList.Count; i++)
        {
            m_buildmanager.CreateHouseObject(m_mapData.GTDList[i].name, m_mapData.GTDList[i].nId, m_mapData.GTDList[i].vec);
        }
    }

    void DrawFloor()
    {
        int nNum = 0;
        for (int i = 0; i < m_col; i++)
        {
            for (int j = 0; j < m_raw; j++)
            {
                string str = MapBasicData.GetFloorRes(m_mapData.m_ftList[nNum]);
                GameObject obj = Instantiate(Resources.Load(str)) as GameObject;
                obj.transform.SetParent(m_buildEdit.transform, true);
                m_floorObjList.Add(obj);
                nNum++;
            }
        }
        setMapPos();
    }

    void DrawLine()
    {
        for (int i = 0; i < m_rawLine; i++)
        {
            GameObject obj = Instantiate(Resources.Load("Main/Prefab/other/rawLine")) as GameObject;
            obj.transform.SetParent(m_buildEdit.transform, true);
            m_objRawList.Add(obj);
        }

        for (int i = 0; i < m_colLine; i++)
        {
            GameObject obj = Instantiate(Resources.Load("Main/Prefab/other/colLine")) as GameObject;
            obj.transform.SetParent(m_buildEdit.transform, true);
            m_objColList.Add(obj);
        }

        setLinePos();
    }

    void initData(mapData md)
    {
        if (md.GTDList.Count == 0)
        {
            List<GoodsTempData> gtdList = MapBasicData.ReadHouseData();
            for (int i = 0; i < gtdList.Count; i++)
                md.GTDList.Add(gtdList[i]);
        }

        if (md.m_ftList.Count == 0)
        {
            MapBasicData.ReadFloorFile(md.m_ftList);
        }
    }

    void OnEnable()
    {
        m_mapData = target as mapData;
        initData(m_mapData);

        for (int i = 0; i < vc_list.Length; i++)
        {
            serial_list[i] = serializedObject.FindProperty(vc_list[i]);
        }

        m_buildEdit = GameObject.Find("buildEdit");
        if (m_buildEdit && m_buildEdit != null)
            DestroyImmediate(m_buildEdit);
        m_buildEdit = new GameObject();
        m_buildEdit.name = "buildEdit";

        m_buildmanager = new BuildManager(m_buildEdit,m_giList, m_mapData.GTDList, m_floorObjList, m_mapData.m_ftList);

        DrawFloor();
        DrawLine();
        drawHouse();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        drawVec();
        drawButton();

        serializedObject.ApplyModifiedProperties();
    }

    private int m_raw = 80;
    private int m_col = 80;
    private int m_rawLine = 81;
    private int m_colLine = 81;

    private List<GoodsInfo> m_giList = new List<GoodsInfo>();
    private List<GameObject> m_objRawList = new List<GameObject>();
    private List<GameObject> m_objColList = new List<GameObject>();
    private List<GameObject> m_floorObjList = new List<GameObject>();
    
    GameObject m_buildEdit;
    BuildManager m_buildmanager;
    mapData m_mapData;

    SerializedProperty[] serial_list = new SerializedProperty[3];
    string[] vc_list = new string[] { "srcMap", "srcRaw", "srcCol" };
}
