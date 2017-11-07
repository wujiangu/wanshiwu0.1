using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public sealed class CGameObjectPool : Singleton<CGameObjectPool>
{
    private bool m_clearPooledObjects;
    private int m_clearPooledObjectsExecuteFrame;
    private DictionaryView<string, Queue<CPooledGameObjectScript>> m_pooledGameObjectMap = new DictionaryView<string, Queue<CPooledGameObjectScript>>();
    private GameObject m_poolRoot;
    private static int s_frameCounter;

    private void _RecycleGameObject(GameObject pooledGameObject, bool setIsInit)
    {
        if (pooledGameObject != null)
        {
            CPooledGameObjectScript component = pooledGameObject.GetComponent<CPooledGameObjectScript>();
            if (component != null)
            {
                Queue<CPooledGameObjectScript> queue = null;
                if (this.m_pooledGameObjectMap.TryGetValue(component.m_prefabKey, out queue))
                {
                    queue.Enqueue(component);
                    this.HandlePooledMonoBehaviour(component.m_pooledMonoBehaviours, enPooledMonoBehaviourAction.Recycle);
                    component.gameObject.transform.SetParent(this.m_poolRoot.transform, true);
                    component.gameObject.SetActive(false);
                    component.m_isInit = setIsInit;
                    return;
                }
            }
            UnityEngine.Object.Destroy(pooledGameObject);
        }
    }

    public void ClearPooledObjects()
    {
        this.m_clearPooledObjects = true;
        this.m_clearPooledObjectsExecuteFrame = s_frameCounter + 1;
    }

    private CPooledGameObjectScript CreateGameObject(string prefabFullPath, Vector3 pos, Quaternion rot, bool useRotation, enResourceType resourceType, string prefabKey)
    {
        CPooledGameObjectScript component = null;
        bool needCached = resourceType == enResourceType.BattleScene;
        GameObject content = Singleton<CResourceManager>.GetInstance().GetResource(prefabFullPath, typeof(GameObject), resourceType, needCached, false).m_content as GameObject;
        if (content == null)
        {
            return null;
        }
        GameObject gameObject = null;
        if (useRotation)
        {
            gameObject = UnityEngine.Object.Instantiate(content, pos, rot) as GameObject;
        }
        else
        {
            gameObject = UnityEngine.Object.Instantiate(content) as GameObject;
            gameObject.transform.position = pos;
        }
        IPooledMonoBehaviour[] pooledMonoBehaviours = this.GetPooledMonoBehaviours(gameObject);
        DebugHelper.Assert(gameObject != null);
        component = gameObject.GetComponent<CPooledGameObjectScript>();
        if (component == null)
        {
            component = gameObject.AddComponent<CPooledGameObjectScript>();
        }
        component.m_prefabKey = prefabKey;
        component.m_pooledMonoBehaviours = pooledMonoBehaviours;
        component.m_defaultScale = component.transform.localScale;
        component.m_isInit = true;
        this.HandlePooledMonoBehaviour(component.m_pooledMonoBehaviours, enPooledMonoBehaviourAction.Create);
        return component;
    }

    public void ExecuteClearPooledObjects()
    {
        DictionaryView<string, Queue<CPooledGameObjectScript>>.Enumerator enumerator = this.m_pooledGameObjectMap.GetEnumerator();
        while (enumerator.MoveNext())
        {
            KeyValuePair<string, Queue<CPooledGameObjectScript>> current = enumerator.Current;
            Queue<CPooledGameObjectScript> queue = current.Value;
            while (queue.Count > 0)
            {
                CPooledGameObjectScript script = queue.Dequeue();
                if ((script != null) && (script.gameObject != null))
                {
                    UnityEngine.Object.Destroy(script.gameObject);
                }
            }
        }
        this.m_pooledGameObjectMap.Clear();
    }

    public GameObject GetGameObject(string prefabFullPath, enResourceType resourceType)
    {
        bool isInit = false;
        return this.GetGameObject(prefabFullPath, Vector3.zero, Quaternion.identity, false, resourceType, out isInit);
    }

    public GameObject GetGameObject(string prefabFullPath, enResourceType resourceType, out bool isInit)
    {
        return this.GetGameObject(prefabFullPath, Vector3.zero, Quaternion.identity, false, resourceType, out isInit);
    }

    public GameObject GetGameObject(string prefabFullPath, Vector3 pos, enResourceType resourceType)
    {
        bool isInit = false;
        return this.GetGameObject(prefabFullPath, pos, Quaternion.identity, false, resourceType, out isInit);
    }

    public GameObject GetGameObject(string prefabFullPath, Vector3 pos, enResourceType resourceType, out bool isInit)
    {
        return this.GetGameObject(prefabFullPath, pos, Quaternion.identity, false, resourceType, out isInit);
    }

    public GameObject GetGameObject(string prefabFullPath, Vector3 pos, Quaternion rot, enResourceType resourceType)
    {
        bool isInit = false;
        return this.GetGameObject(prefabFullPath, pos, rot, true, resourceType, out isInit);
    }

    public GameObject GetGameObject(string prefabFullPath, Vector3 pos, Quaternion rot, enResourceType resourceType, out bool isInit)
    {
        return this.GetGameObject(prefabFullPath, pos, rot, true, resourceType, out isInit);
    }

    private GameObject GetGameObject(string prefabFullPath, Vector3 pos, Quaternion rot, bool useRotation, enResourceType resourceType, out bool isInit)
    {
        string key = CFileManager.EraseExtension(prefabFullPath).ToLower();
        Queue<CPooledGameObjectScript> queue = null;
        if (!this.m_pooledGameObjectMap.TryGetValue(key, out queue))
        {
            queue = new Queue<CPooledGameObjectScript>();
            this.m_pooledGameObjectMap.Add(key, queue);
        }
        CPooledGameObjectScript script = null;
        while (queue.Count > 0)
        {
            script = queue.Dequeue();
            if ((script != null) && (script.gameObject != null))
            {
                script.gameObject.transform.SetParent(null, true);
                script.gameObject.transform.position = pos;
                script.gameObject.transform.rotation = rot;
                script.gameObject.transform.localScale = script.m_defaultScale;
                break;
            }
            script = null;
        }
        if (script == null)
        {
            script = this.CreateGameObject(prefabFullPath, pos, rot, useRotation, resourceType, key);
        }
        if (script == null)
        {
            isInit = false;
            return null;
        }
        isInit = script.m_isInit;
        script.gameObject.SetActive(true);
        this.HandlePooledMonoBehaviour(script.m_pooledMonoBehaviours, enPooledMonoBehaviourAction.Get);
        return script.gameObject;
    }

    private IPooledMonoBehaviour[] GetPooledMonoBehaviours(GameObject gameObject)
    {
        IPooledMonoBehaviour[] behaviourArray = null;
        MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>();
        if ((componentsInChildren == null) || (componentsInChildren.Length <= 0))
        {
            return new IPooledMonoBehaviour[0];
        }
        int index = 0;
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            if (componentsInChildren[i] is IPooledMonoBehaviour)
            {
                index++;
            }
        }
        behaviourArray = new IPooledMonoBehaviour[index];
        index = 0;
        for (int j = 0; j < componentsInChildren.Length; j++)
        {
            IPooledMonoBehaviour behaviour2 = componentsInChildren[j] as IPooledMonoBehaviour;
            if (behaviour2 != null)
            {
                behaviourArray[index] = behaviour2;
                index++;
            }
        }
        return behaviourArray;
    }

    private void HandlePooledMonoBehaviour(IPooledMonoBehaviour[] pooledMonoBehaviours, enPooledMonoBehaviourAction pooledMonoBehaviourAction)
    {
        for (int i = 0; i < pooledMonoBehaviours.Length; i++)
        {
            IPooledMonoBehaviour behaviour = pooledMonoBehaviours[i];
            if ((behaviour != null) && (behaviour as MonoBehaviour).enabled)
            {
                switch (pooledMonoBehaviourAction)
                {
                    case enPooledMonoBehaviourAction.Create:
                        behaviour.OnCreate();
                        break;

                    case enPooledMonoBehaviourAction.Get:
                        behaviour.OnGet();
                        break;

                    case enPooledMonoBehaviourAction.Recycle:
                        behaviour.OnRecycle();
                        break;
                }
            }
        }
    }

    public override void Init()
    {
        this.m_poolRoot = new GameObject("CGameObjectPool");
        GameObject obj2 = GameObject.Find("BootObj");
        if (obj2 != null)
        {
            this.m_poolRoot.transform.SetParent(obj2.transform);
        }
    }

    public void PrepareGameObject(string prefabFullPath, enResourceType resourceType, int amount)
    {
        string key = CFileManager.EraseExtension(prefabFullPath).ToLower();
        Queue<CPooledGameObjectScript> queue = null;
        if (!this.m_pooledGameObjectMap.TryGetValue(key, out queue))
        {
            queue = new Queue<CPooledGameObjectScript>();
            this.m_pooledGameObjectMap.Add(key, queue);
        }
        if (queue.Count < amount)
        {
            amount -= queue.Count;
            for (int i = 0; i < amount; i++)
            {
                CPooledGameObjectScript item = this.CreateGameObject(prefabFullPath, Vector3.zero, Quaternion.identity, false, resourceType, key);
                DebugHelper.Assert(item != null);
                if (item != null)
                {
                    queue.Enqueue(item);
                    item.gameObject.transform.SetParent(this.m_poolRoot.transform, true);
                    item.gameObject.SetActive(false);
                }
            }
        }
    }

    public void RecycleGameObject(GameObject pooledGameObject)
    {
        this._RecycleGameObject(pooledGameObject, false);
    }

    public void RecyclePreparedGameObject(GameObject pooledGameObject)
    {
        this._RecycleGameObject(pooledGameObject, true);
    }

    public override void UnInit()
    {
    }

    public void Update()
    {
        s_frameCounter++;
        if (this.m_clearPooledObjects && (this.m_clearPooledObjectsExecuteFrame == s_frameCounter))
        {
            this.ExecuteClearPooledObjects();
            this.m_clearPooledObjects = false;
        }
    }

    private enum enPooledMonoBehaviourAction
    {
        Create,
        Get,
        Recycle
    }
}

