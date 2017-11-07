using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEditor;


namespace cs
{
    public class AssetPackage : IUpdate
    {
        private string m_strPath = null;

        public EAssetLoadState LoadState { get; private set; }

        public Object LoadTemplate(string a_strName)
        {
            if (LoadState == EAssetLoadState.Done)
            {
                return m_assetBundle.LoadAsset(a_strName);
            }
            else
            {
                return null;
            }
        }

        public AssetBundleRequest LoadTemplateAsync(string a_strName)
        {
            if (LoadState == EAssetLoadState.Done)
            {
                return m_assetBundle.LoadAssetAsync(a_strName);
            }
            else
            {
                return null;
            }
        }

        public void Load(bool a_bAsync, UnityAction<AssetPackage> a_onLoadFinished)
        {
            if (LoadState == EAssetLoadState.Invalid)
            {
                _RegisterFinishedListener(a_onLoadFinished);

                // 还没有加载，则根据加载命令a_bAsync，选择同步，或者异步加载
                if (a_bAsync)
                {
                    m_bundleLoadRequest = AssetBundle.LoadFromFileAsync(m_strPath);

                    // TODO 测试异步加载不存在的资源的情况

                    LoadState = EAssetLoadState.Loading;
                }
                else
                {
                    m_assetBundle = AssetBundle.LoadFromFile(m_strPath);
                    _OnLoadFinished();
                }
            }
            else if (LoadState == EAssetLoadState.Loading)
            {
                _RegisterFinishedListener(a_onLoadFinished);
            }
            else
            {
                // 加载已完成，则只需要直接执行回调
                if (a_onLoadFinished != null)
                {
                    a_onLoadFinished(this);
                }
            }
        }

        public void Unload()
        {
            if (LoadState == EAssetLoadState.Loading)
            {
                return;
            }

            Assert.IsTrue(
                m_listOnLoadFinished.Count <= 0 &&
                m_bundleLoadRequest == null
                );
            if (m_assetBundle != null)
            {
                m_assetBundle.Unload(false);
                m_assetBundle = null;
            }
            LoadState = EAssetLoadState.Invalid;
        }

        public void Tick(float a_fElapsed)
        {
            if (LoadState == EAssetLoadState.Loading)
            {
                Assert.IsNotNull(m_bundleLoadRequest);

                if (m_bundleLoadRequest.isDone)
                {
                    m_assetBundle = m_bundleLoadRequest.assetBundle;
                    m_bundleLoadRequest = null;
                    _OnLoadFinished();
                }
            }
        }

        private void _RegisterFinishedListener(UnityAction<AssetPackage> a_onLoadFinished)
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
            if (m_assetBundle == null)
            {
                Logger.Error("资源加载失败：{0}", m_strPath);
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
                UnityAction<AssetPackage> unityAction = m_listOnLoadFinished[0];
                m_listOnLoadFinished.RemoveAt(0);
                unityAction(this);
            }
        }

        private AssetBundle m_assetBundle;

        private AssetBundleCreateRequest m_bundleLoadRequest = null;

        private List<UnityAction<AssetPackage>> m_listOnLoadFinished = new List<UnityAction<AssetPackage>>();
    }
}
