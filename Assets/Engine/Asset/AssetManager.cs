using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace cs
{


    /// <summary>
    /// 资源管理器
    /// 禁止裸用Unity提供的 Object.Destroy 接口
    /// </summary>
    public class AssetManager : Singleton<AssetManager>, IUpdate
    {
        /// <summary>
        /// 加载资源
        /// 注意：
        /// 如果资源正在异步加载，那么无法再进行同步加载资源，返回的AssetObj里的资源对象为空，并有异步加载的进度，可供查询！
        /// 养成良好的编码习惯，查询AssetObj的加载状态
        /// </summary>
        /// <param name="a_strPath">资源路径</param>
        /// <param name="a_assetType">资源类型，默认类型填写typeof(Object)</param>
        /// <param name="a_bFromPool">是否从池子里加载</param>
        /// <param name="a_bAsync">是否异步加载</param>
        /// <param name="a_onLoadFinished">加载完成的回调</param>
        /// <returns></returns>
        public AssetObj CreateAsset(string a_strPath, Type a_assetType = null, bool a_bFromPool = false, bool a_bAsync = false, UnityAction<AssetObj> a_onLoadFinished = null)
        {
            AssetObj assetObj = null;
            if (a_bFromPool)
            {
                // 从池子里取资源对象
                ObjectPool<AssetObj> objectPool = null;
                m_dictAssetObjPool.TryGetValue(a_strPath, out objectPool);
                if (objectPool == null)
                {
                    objectPool = new ObjectPool<AssetObj>(_OnAssetObjGet, _OnAssetObjRelease, _OnAssetObjDestroy);
                    m_dictAssetObjPool.Add(a_strPath, objectPool);
                }
                assetObj = objectPool.Get();
                if (assetObj.LoadState == EAssetLoadState.Invalid)
                {
                    assetObj.Load(_GetAssetDesc(a_strPath, a_assetType), a_bAsync, a_onLoadFinished);
                }
                else if (assetObj.LoadState == EAssetLoadState.Loading)
                {
                    assetObj.RegisterFinishedCallback(a_onLoadFinished);
                }
                else
                {
                    a_onLoadFinished(assetObj);
                }
            }
            else
            {
                // 直接创建资源对象
                assetObj = new AssetObj();
                assetObj.Load(_GetAssetDesc(a_strPath, a_assetType), a_bAsync, a_onLoadFinished);
            }

            if (assetObj.LoadState == EAssetLoadState.Loading)
            {
                AssetDesc assetDesc = assetObj.Desc;
                if (assetDesc != null && assetDesc.LoadState == EAssetLoadState.Loading)
                {
                    if (m_listLoadingAssetDescs.Contains(assetDesc) == false)
                    {
                        m_listLoadingAssetDescs.Add(assetDesc);
                    }

                    AssetPackage assetPackage = assetDesc.Package;
                    if (assetPackage != null && assetPackage.LoadState == EAssetLoadState.Loading)
                    {
                        if (m_listLoadingPackage.Contains(assetPackage) == false)
                        {
                            m_listLoadingPackage.Add(assetPackage);
                        }
                    }
                }
            }

            return assetObj;
        }

        public void DestroyAsset(AssetObj a_asset)
        {
            if (a_asset != null)
            {
                if (a_asset.PooledState == AssetObj.EPooledState.OutPool)
                {
                    ObjectPool<AssetObj> objectPool = null;
                    m_dictAssetObjPool.TryGetValue(a_asset.Desc.Path, out objectPool);
                    Assert.IsNotNull(objectPool);

                    objectPool.Release(a_asset);
                }
                else if (a_asset.PooledState == AssetObj.EPooledState.Invalid)
                {
                    a_asset.UnLoad();
                }
            }
        }

        public void Tick(float a_fElapsed)
        {
            AssetPackage assetPackage = null;
            for (int i = 0; i < m_listLoadingPackage.Count; ++i)
            {
                assetPackage = m_listLoadingPackage[i];
                assetPackage.Tick(a_fElapsed);
                if (assetPackage.LoadState != EAssetLoadState.Loading)
                {
                    m_listLoadingPackage.RemoveAt(i);
                    --i;
                }
            }

            AssetDesc assetDesc = null;
            for (int i = 0; i < m_listLoadingAssetDescs.Count; ++i)
            {
                assetDesc = m_listLoadingAssetDescs[i];
                assetDesc.Tick(a_fElapsed);
                if (assetDesc.LoadState != EAssetLoadState.Loading)
                {
                    m_listLoadingAssetDescs.RemoveAt(i);
                    --i;
                }
            }
        }

        public void GetPackage(string a_strAssetPath, out AssetPackage a_package)
        {
            a_package = null;
        }

        private AssetDesc _GetAssetDesc(string a_strPath, Type a_type)
        {
            AssetDesc assetDesc = null;
            m_dictAssetDescs.TryGetValue(a_strPath, out assetDesc);
            if (assetDesc == null)
            {
                assetDesc = new AssetDesc(a_strPath, a_type);
                m_dictAssetDescs.Add(a_strPath, assetDesc);
            }
            return assetDesc;
        }

        private void _OnAssetObjGet(AssetObj a_assetObj)
        {
            if (a_assetObj != null)
            {
                a_assetObj.OnGetFromPool();
            }
        }

        private void _OnAssetObjRelease(AssetObj a_assetObj)
        {
            if (a_assetObj != null)
            {
                a_assetObj.OnReleaseToPool();
            }
        }

        private void _OnAssetObjDestroy(AssetObj a_assetObj)
        {
            if (a_assetObj != null)
            {
                a_assetObj.OnDestroyInPool();
            }
        }

        protected override void _Initialize()
        {
            // TODO
        }

        protected override void _Clear()
        {
            // TODO
        }

        private Dictionary<string, ObjectPool<AssetObj>> m_dictAssetObjPool = new Dictionary<string, ObjectPool<AssetObj>>();
        private Dictionary<string, AssetDesc> m_dictAssetDescs = new Dictionary<string, AssetDesc>();
        private Dictionary<string, AssetPackage> m_dictAssetPackage = new Dictionary<string, AssetPackage>();
        private List<AssetPackage> m_listLoadingPackage = new List<AssetPackage>();
        private List<AssetDesc> m_listLoadingAssetDescs = new List<AssetDesc>();
    }
}

