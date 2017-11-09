using UnityEngine;
using System.Collections;

[System.Serializable]
public class WSWBuildingData : WSWEntityData
{
    /// <summary>
    /// 建筑物名字
    /// </summary>
    public string Name { get { return m_strName; } }



    private string m_strName;
}
