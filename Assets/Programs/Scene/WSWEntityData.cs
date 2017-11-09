using UnityEngine;
using System.Collections;

[System.Serializable]
public class WSWEntityData
{
    /// <summary>
    /// 实体资源的表ID
    /// </summary>
    public int TableID { get { return m_nTableID; } }

    /// <summary>
    /// 在场景中的横坐标（逻辑坐标），记录的是实体的最下方格子在场景中的位置，
    /// 如下图最下方的菱形格子就是基准格子
    ///  /\
    /// /\/\
    /// \/\/\
    ///  \/\/  
    ///   \/
    /// </summary>
    public int PosX { get { return m_nPosX; } }

    /// <summary>
    /// 在场景中的纵坐标（逻辑坐标），记录的是实体的最下方格子在场景中的位置，
    /// 如下图最下方的菱形格子就是基准格子
    ///  /\
    /// /\/\
    /// \/\/\
    ///  \/\/  
    ///   \/
    /// </summary>
    public int PosY { get { return m_nPosY; } }

    private int m_nTableID;
    private int m_nPosX;
    private int m_nPosY;
}
