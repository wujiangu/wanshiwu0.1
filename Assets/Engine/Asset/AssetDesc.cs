using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using UnityEditor;
using System;
using System.Collections.Generic;


namespace cs
{
    /// <summary>
    /// 资源描述
    /// 设计成一个资源所需要的所有信息，资源路径信息-->资源包-->模板对象
    /// 资源路径信息是原始信息，用于获取资源包
    /// 资源包用于加载、卸载模板对象
    /// 模板对象，动态管理
    /// </summary>
    public class AssetDesc : IUpdate
    {
        public string Path { get; private set; }

        public Type AssetType { get; private set; }

        public float Hot { get; private set; }

        public EAssetLoadState LoadState { get; private set; }

        public float Progress { get; private set; }

        private AssetPackage m_assetPackage = null;
        public AssetPackage Package
        {
            get
            {
                return m_assetPackage;
            }
        }

        private UnityEngine.Object m_assetTemplate = null;
        public UnityEngine.Object AssetTemplate
        {
            get { return m_assetTemplate; }
            private set { m_assetTemplate = value; }
        }

        public AssetDesc(string a_strPath, Type a_type)
        {
            Path = a_strPath;
            AssetType = a_type;
            LoadState = EAssetLoadState.Invalid;
        }

        public void Load(bool a_bAsync, UnityAction<AssetDesc> a_onLoadFinished)
        {
            if (LoadState == EAssetLoadState.Invalid)
            {
                _RegisterFinishedListener(a_onLoadFinished);
                m_bAsync = a_bAsync;

                LoadState = EAssetLoadState.Loading;

                // 先从包里加载资源，如果没有相关的包，则直接用Resources的方式加载
                if (m_assetPackage == null)
                {
                    AssetManager.Get().GetPackage(Path, out m_assetPackage);
                }
                if (m_assetPackage != null)
                {
                    // 存在对应的包，从包里加载
                    m_assetPackage.Load(m_bAsync, _OnPackageLoadFinished);
                }
                else
                {
                    // 不存在对应的包，用Resources的方式加载资源
                    if (m_bAsync)
                    {
                        if (AssetType == null)
                        {
                            m_resourceRequest = Resources.LoadAsync(Path);
                        }
                        else
                        {
                            m_resourceRequest = Resources.LoadAsync(Path, AssetType);
                        }
                    }
                    else
                    {
                        if (AssetType == null)
                        {
                            m_assetTemplate = Resources.Load(Path);
                        }
                        else
                        {
                            m_assetTemplate = Resources.Load(Path, AssetType);
                        }
                        _OnLoadFinished();
                    }
                }
            }
            else if (LoadState == EAssetLoadState.Loading)
            {
                _RegisterFinishedListener(a_onLoadFinished);
            }
            else if (LoadState == EAssetLoadState.Done || LoadState == EAssetLoadState.Error)
            {
                if (a_onLoadFinished != null)
                {
                    a_onLoadFinished(this);
                }
            }
        }

        public void UnLoad()
        {
            // 正在加载中，禁止卸载
            if (LoadState == EAssetLoadState.Loading)
            {
                return;
            }

            Assert.IsTrue(
                m_listOnLoadFinished.Count <= 0 &&
                m_resourceRequest == null &&
                m_assetBundleRequest == null
                );
            // 销毁资源模板
            if (m_assetTemplate != null)
            {
                UnityEngine.Object.Destroy(m_assetTemplate);
                m_assetTemplate = null;
            }
            // 重置状态
            LoadState = EAssetLoadState.Invalid;
            m_bAsync = false;
        }

        public void UnRegisterFinishedListener(UnityAction<AssetDesc> a_onLoadFinished)
        {
            if (a_onLoadFinished == null)
            {
                return;
            }

            m_listOnLoadFinished.Remove(a_onLoadFinished);
        }

        public void Tick(float a_fElapsed)
        {
            if (LoadState == EAssetLoadState.Loading)
            {
                if (m_assetBundleRequest != null)
                {
                    if (m_assetBundleRequest.isDone)
                    {
                        m_assetTemplate = m_assetBundleRequest.asset;
                        m_assetBundleRequest = null;
                        _OnLoadFinished();
                    }
                }
                else if (m_resourceRequest != null)
                {
                    if (m_resourceRequest.isDone)
                    {
                        m_assetTemplate = m_resourceRequest.asset;
                        m_resourceRequest = null;
                        _OnLoadFinished();
                    }
                }
            }
        }

        private void _RegisterFinishedListener(UnityAction<AssetDesc> a_onLoadFinished)
        {
            if (a_onLoadFinished == null)
            {
                return;
            }

            if (m_listOnLoadFinished.Contains(a_onLoadFinished) == false)
            {
                m_listOnLoadFinished.Add(a_onLoadFinished);
            }
        }

        private void _OnLoadFinished()
        {
            if (m_assetTemplate == null)
            {
                Logger.Error("资源加载失败：{0}", Path);
                LoadState = EAssetLoadState.Error;
            }
            else
            {
                LoadState = EAssetLoadState.Done;
            }

            while (
                m_listOnLoadFinished.Count > 0 && 
                (LoadState == EAssetLoadState.Done || LoadState == EAssetLoadState.Error)
                )
            {
                UnityAction<AssetDesc> unityAction = m_listOnLoadFinished[0];
                m_listOnLoadFinished.RemoveAt(0);
                unityAction(this);
            }
        }

        private void _OnPackageLoadFinished(AssetPackage a_package)
        {
            if (m_bAsync)
            {
                m_assetBundleRequest = a_package.LoadTemplateAsync(Path);
            }
            else
            {
                m_assetTemplate = a_package.LoadTemplate(Path);
                _OnLoadFinished();
            }
        }

        
        private bool m_bAsync = false;

        private AssetBundleRequest m_assetBundleRequest = null;
        private ResourceRequest m_resourceRequest = null;
        private List<UnityAction<AssetDesc>> m_listOnLoadFinished = new List<UnityAction<AssetDesc>>();
    }
}
