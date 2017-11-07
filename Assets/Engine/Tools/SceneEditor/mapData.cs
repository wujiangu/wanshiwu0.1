using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum GoodsType
{
    TABLE = 0,          //桌子
    ALLOYCABINET,       //合金柜子
    CABINET,            //柜子
    ELECTRICBOX,        //电气盒
    ELECTRICCABINET,    //电气柜子
    OXYGENBOTTLE,       //氧气瓶
    HIGHSTOVE,          //高压炉
    FLOOR,              //底板
}

public class GoodsInfo
{
    public int nID;
    public string name;
    public GoodsType gt;
    public float hypeLength;
    public Vector2 vec;
    public Vector2[] mesh_list;
    public Vector2 oldMesh;
    public GameObject obj;
}

public class HouseData
{
    public float width;
    public float height;
    public int raw;
    public int col;
    public Vector2 srcMesh;

    public void InitData(float nWidth, float nHeight, int nRaw, int nCol, Vector2 vec)
    {
        width = nWidth;
        height = nHeight;
        raw = nRaw;
        col = nCol;
        srcMesh = vec;
    }
}

[System.Serializable]
public struct GoodsTempData
{
    public int nId;
    public Vector2 vec;
    public string name;
}

//类型类
public class CType
{
    public static int NORMAL_FLOOR = 1;
    public static int GENERAL_FLOOR = 2;
}

/* 基本类型数据*/
public class MapBasicData
{
    public static bool IsSelect = false;
    public static int SelectID = 0;
    public static int snCurrId = 0;

    /* 摄像机视角的边角巨鹿 */
    public static float CameraLeft  = -18.81f;
    public static float CameraRight = 20.74f;
    public static float CameraUp    = 11.4f;
    public static float CameraDown  = -11.42f;

    /*建筑视角的边角距离*/
    public static float BuildLeft  = -27.09f;
    public static float BuildRight = 28.99f;
    public static float BuildUp    = 16.15f;
    public static float BuildDown  = -16.195f;

    public static Vector2[] map_list = new Vector2[80 * 80];

    //获得随机的id
    public static int GetRandId()
    {
        return Random.Range(1, 1000000);
    }

    public static GoodsType GetHouseType(string name)
    {
        GoodsType nType = 0;
        switch (name)
        {
            case "table":
                nType = GoodsType.TABLE;
                break;
            case "cabinet":
                nType = GoodsType.CABINET;
                break;
            case "electricBox":
                nType = GoodsType.ELECTRICBOX;
                break;
            case "oxygenBottle":
                nType = GoodsType.OXYGENBOTTLE;
                break;
            case "highStove":
                nType = GoodsType.HIGHSTOVE;
                break;
            case "electricCabinet":
                nType = GoodsType.ELECTRICCABINET;
                break;
            case "alloyCabinet":
                nType = GoodsType.ALLOYCABINET;
                break;
        }

        return nType;
    }

    public static HouseData GetHouseData(GoodsType gt)
    {
        HouseData hd = new HouseData();

        if (gt == GoodsType.TABLE)
        {
            hd.InitData(2.4f, 2.66f, 2, 5, new Vector2(0.7f / 2 + 0.12f, 0.4f / 2 + 0.11f));
        }
        else if (gt == GoodsType.CABINET)
        {
            hd.InitData(2.04f, 2.68f, 2, 5, new Vector2(0.7f / 2, 0.4f / 2 + 0.15f));
        }
        else if (gt == GoodsType.ELECTRICBOX)
        {
            hd.InitData(2.01f, 2.18f, 4, 2, new Vector2(0.5f, 0.4f * 2 - 0.1f));
        }
        else if (gt == GoodsType.OXYGENBOTTLE)
        {
            hd.InitData(1.22f, 2.14f, 2, 2, new Vector2(0.27f, 0.4f / 2 + 0.07f));
        }
        else if (gt == GoodsType.ALLOYCABINET)
        {
            hd.InitData(1.63f, 1.79f, 3, 2, new Vector2(0.38f, 0.45f));
        }
        else if (gt == GoodsType.HIGHSTOVE)
        {
            hd.InitData(1.63f, 1.79f, 3, 3, new Vector2(0.1f, 0.3f));
        }
        else if (gt == GoodsType.ELECTRICCABINET)
        {
            hd.InitData(1.63f, 1.79f, 3, 4, new Vector2(0.1f, 0.3f));
        }
        return hd;
    }

    public static string GetFloorRes(int ft)
    {
        string strRes = "";
        if (ft == CType.NORMAL_FLOOR)
            strRes = "Main/Prefab/floor/normal_floor";
        else if (ft == CType.GENERAL_FLOOR)
            strRes = "Main/Prefab/floor/general_floor";

        return strRes;
    }

    //导入地板文件
    public static void LoadFloorFile(List<int> ftList)
    {
        FileManager.GetInstance().DeleteFile(Application.dataPath, "floorFile.txt");
        string str = "";
        for (int i = 0; i < ftList.Count; i++)
            str += ftList[i] + "_";

        List<string> strList = new List<string>();
        strList.Add(str);
        FileManager.GetInstance().LoadFile(Application.dataPath, "floorFile.txt", strList);
    }

    //读取地板文件
      public static void ReadFloorFile(List<int> ftList)
      {
           List<string> str_list = FileManager.GetInstance().ReadFile(Application.dataPath, "floorFile.txt");
           if (int.Parse(str_list[0]) > 0 && int.Parse(str_list[0]) == 6400)
           {
               string[] str_split = str_list[1].Split('_');
               for (int i = 0; i < str_split.Length - 1; i++)
                   ftList.Add(int.Parse(str_split[i]));
           }
           else
           {
               for (int i = 0; i < 6400; i++)
               {
                   ftList.Add(CType.NORMAL_FLOOR);
               }
           }
      }

    public static GoodsTempData GetGoodsTempData(int id, string name, Vector2 vec)
    {
        GoodsTempData gtd;
        gtd.name = name;
        gtd.nId = id;
        gtd.vec = vec;
        return gtd;
    }

    public static List<GoodsTempData> ReadHouseData()
    {
        List<GoodsTempData> gdList = new List<GoodsTempData>();
        List<string> str_list = FileManager.GetInstance().ReadFile(Application.dataPath, "mapFile.txt");
        if (int.Parse(str_list[0]) > 0)
        {
            GoodsMove gm = new GoodsMove();
            for (int i = 1; i < str_list.Count; i++)
            {
                string[] str_split = str_list[i].Split('_');
                int nId = int.Parse(str_split[1]);
                Vector2 vc = new Vector2(float.Parse(str_split[2]), float.Parse(str_split[3]));
                gdList.Add(GetGoodsTempData(nId, str_split[0], vc));
            }
        }

        return gdList;
    }

    //load house data
    public static void LoadHouseData(List<GoodsTempData> gtdList)
    {
        FileManager.GetInstance().DeleteFile(Application.dataPath, "mapFile.txt");
        List<string> str_list = new List<string>();
        for (int i = 0; i < gtdList.Count; i++)
        {
            GoodsTempData gtd = gtdList[i];
            string str = gtd.name + "_" + gtd.nId + "_" + gtd.vec.x + "_" + gtd.vec.y;
            str_list.Add(str);
        }
        FileManager.GetInstance().LoadFile(Application.dataPath, "mapFile.txt", str_list);
    }
}

public class mapData : ScriptableObject
{
    [SerializeField]
    private List<GoodsTempData> m_gtdList;

    public Vector2 srcMap = new Vector2(0, 0);
    public Vector2 srcRaw = new Vector2(0, 0);
    public Vector2 srcCol = new Vector2(0, 0);
    public List<int> m_ftList = new List<int>();

    public List<GoodsTempData> GTDList { get { return m_gtdList; } }
}
