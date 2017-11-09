using UnityEngine;
using System.Collections.Generic;


public class WSWSceneData : ScriptableObject
{
    /// <summary>
    /// 场景根节点在世界坐标系的位置
    /// </summary>
    public Vector3 LocalToWorldPos { get { return m_vec3LocalToWorldPos; } }

    /// <summary>
    /// 场景预制体在场景坐标系的位置
    /// </summary>
    public Vector3 ScenePrefabPos { get { return m_vec3LocalScenePrefabPos; } }

    /// <summary>
    /// 场景预制体路径
    /// </summary>
    public string ScenePrefabPath { get { return m_strScenePrefabPath; } }

    /// <summary>
    /// 地图在场景坐标系的位置
    /// </summary>
    public Vector3 MapPos { get { return m_vec3LocalMapPos; } }

    /// <summary>
    /// 地图贴图中瓦片的行数
    /// </summary>
    public int MapTextureTileRow { get { return m_nMapTextureTileRow; } }

    /// <summary>
    /// 地图贴图中瓦片的列数
    /// </summary>
    public int MapTextureTileCol { get { return m_nMapTextureTileCol; } }

    /// <summary>
    /// 地图贴图中瓦片的总数量
    /// </summary>
    public int MapTextureTileCount { get { return m_nMapTextureTileCount; } }

    /// <summary>
    /// 地图贴图路径
    /// </summary>
    public string MapTexturePath { get { return m_strMapTexturePath; } }

    /// <summary>
    /// 地图材质路径
    /// </summary>
    public string MapMaterialPath { get { return m_strMapMaterialPath; } }

    /// <summary>
    /// 地图水平方向的瓦片数
    /// </summary>
    public int MapTileX { get { return m_nMapTileX; } }

    /// <summary>
    /// 地图垂直方向的瓦片数
    /// </summary>
    public int MapTileY { get { return m_nMapTileY; } }

    /// <summary>
    /// 地图所有瓦片的信息
    /// 为了节省空间，byte共8位，从低位到高位，
    /// 前6位存储的是贴图索引，第7位存储的是是否有阻挡
    /// </summary>
    public List<byte> ListTileInfos { get { return m_listTileInfo; } }

    /// <summary>
    /// 所有的建筑数据
    /// </summary>
    public List<WSWBuildingData> ListBuildingData { get { return m_listBuildingData; } }




    // scene root pos within world space
    [SerializeField]
    private Vector3 m_vec3LocalToWorldPos;

    // scene prefab data
    [SerializeField]
    private Vector3 m_vec3LocalScenePrefabPos;
    [SerializeField]
    private string m_strScenePrefabPath;

    // TileMap data
    [SerializeField]
    private Vector3 m_vec3LocalMapPos;
    [SerializeField]
    private int m_nMapTextureTileRow;
    [SerializeField]
    private int m_nMapTextureTileCol;
    [SerializeField]
    private int m_nMapTextureTileCount;
    [SerializeField]
    private string m_strMapTexturePath;
    [SerializeField]
    private string m_strMapMaterialPath;
    [SerializeField]
    private int m_nMapTileX;
    [SerializeField]
    private int m_nMapTileY;
    [SerializeField]
    private List<byte> m_listTileInfo;
    [SerializeField]
    private List<WSWBuildingData> m_listBuildingData;
}
