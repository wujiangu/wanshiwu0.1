using System;
using UnityEngine;

public class CPooledGameObjectScript : MonoBehaviour
{
    public Vector3 m_defaultScale;
    public bool m_isInit;
    public IPooledMonoBehaviour[] m_pooledMonoBehaviours;
    public string m_prefabKey;
}

