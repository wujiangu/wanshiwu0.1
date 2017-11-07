using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using UnityEditor;
using System.Collections.Generic;

namespace cs
{
    public class AssetObj
    {

        public enum EPooledState
        {
            /// <summary>
            /// 非池化状态
            /// </summary>
            Invalid,

            /// <summary>
            /// 缓存在池子里
            /// </summary>
            InPool,

            /// <summary>
            /// 不在池子中
            /// </summary>
            OutPool,
        }

        public AssetDesc Desc { get; private set; }

        public float LoadProgress
        {
            get
            {
                if (Desc.AssetTemplate != null)
                {
                    return 0.8f + (Obj != null ? 0.2f : 0.0f);
                }
                else
                {
                    return Desc.Progress * 0.8f;
                }
            }
        }

        private Object m_obj = null;
        public Object Obj
        {
            get
            {
                if (LoadState == EAssetLoadState.Done)
                {
                    return m_obj;
                }
                else
                {
                    Logger.Error("资源[{0}]加载状态异常[{1}]，无法获取该资源!!", Desc.Path, LoadState);
                    return null;
                }
            }
            private set { m_obj = value; }
        }

        public GameObject gameObject
        {
            get
            {
                return Obj as GameObject;
            }
        }

        private EAssetLoadState m_eLoadState = EAssetLoadState.Invalid;
        public EAssetLoadState LoadState
        {
            get { return m_eLoadState; }
            private set { m_eLoadState = value; }
        }

        private EPooledState m_ePooledState = EPooledState.Invalid;
        public EPooledState PooledState
        {
            get { return m_ePooledState; }
            private set { m_ePooledState = value; }
        }

        /// <summary>
        /// 初始化
        /// 实例化操作，如果资源量大，会比较耗时
        /// </summary>
        /// <param name="a_desc">资源描述</param>
        public void Load(AssetDesc a_desc, bool a_bAsync, UnityAction<AssetObj> a_onLoadFinished)
        {
            if (a_desc == null)
            {
                Logger.Error("加载资源，资源描述为空！！");
                return;
            }

            if (LoadState == EAssetLoadState.Invalid)
            {
                Desc = a_desc;
                RegisterFinishedCallback(a_onLoadFinished);
                LoadState = EAssetLoadState.Loading;
                Desc.Load(a_bAsync, _OnDescLoadFinished);
            }
            else
            {
                Logger.Error("请求加载资源[{0}]，内部状态错误：{1}！", a_desc.Path, LoadState);
            }


            //if (LoadState == EAssetLoadState.Invalid)
            //{
            //    Desc = a_desc;
            //    m_onLoadFinished = a_onLoadFinished;
            //    LoadState = EAssetLoadState.Loading;
            //    Desc.Load(a_bAsync, _OnDescLoadFinished);
            //}
            //else if (LoadState == EAssetLoadState.Loading)
            //{
            //    m_onLoadFinished = a_onLoadFinished;
            //}
            //else if (LoadState == EAssetLoadState.Done || LoadState == EAssetLoadState.Error)
            //{
            //    if (a_onLoadFinished != null)
            //    {
            //        a_onLoadFinished(this);
            //    }
            //}
        }

        public void UnLoad()
        {
            if (LoadState == EAssetLoadState.Loading)
            {
                Desc.UnRegisterFinishedListener(_OnDescLoadFinished);
                m_listOnLoadFinished.Clear();
            }
            else if (LoadState == EAssetLoadState.Done)
            {
                if (m_obj != null)
                {
                    UnityEngine.Object.Destroy(Obj);
                    Obj = null;
                }
            }

            Desc = null;
            LoadState = EAssetLoadState.Invalid;
        }

        public void RegisterFinishedCallback(UnityAction<AssetObj> a_onLoadFinished)
        {
            if (a_onLoadFinished == null)
            {
                return;
            }

            if (m_listOnLoadFinished.Contains(a_onLoadFinished) == false)
            {
                m_listOnLoadFinished.Enqueue(a_onLoadFinished);
            }
        }

        /// <summary>
        /// 对象从池子里取出时，调用
        /// </summary>
        public virtual void OnGetFromPool()
        {
            PooledState = EPooledState.OutPool;
        }

        /// <summary>
        /// 对象放回池子时，调用
        /// </summary>
        public virtual void OnReleaseToPool()
        {
            if (PooledState == EPooledState.OutPool)
            {
                PooledState = EPooledState.InPool;

                // 放进池子里，外部不再使用，则所有加载完成回调需要清空
                m_listOnLoadFinished.Clear();
            }
            else if (PooledState == EPooledState.InPool)
            {
                Logger.Error("AssetObj已经回收到池子里，不能再次回收");
            }
            else
            {
                Logger.Error("AssetObj不是从池子里创建的，不能回收到池子里");
            }
        }

        /// <summary>
        /// 在池子里销毁
        /// </summary>
        public virtual void OnDestroyInPool()
        {
            if (PooledState == EPooledState.InPool)
            {
                PooledState = EPooledState.Invalid;

                UnLoad();
            }
            else
            {
                Logger.Error("AssetObj不在池子中，无法销毁");
            }
        }

        private void _OnDescLoadFinished(AssetDesc a_desc)
        {
            if (a_desc.LoadState == EAssetLoadState.Done)
            {
                Obj = Object.Instantiate(Desc.AssetTemplate);
                LoadState = EAssetLoadState.Done;
            }
            else
            {
                LoadState = EAssetLoadState.Error;
            }

            while (
                m_listOnLoadFinished.Count > 0 &&
                (LoadState == EAssetLoadState.Done || LoadState == EAssetLoadState.Error)
                )
            {
                UnityAction<AssetObj> unityAction = m_listOnLoadFinished.Dequeue();
                unityAction(this);
            }
        }

        private Queue<UnityAction<AssetObj>> m_listOnLoadFinished = new Queue<UnityAction<AssetObj>>();
    }
}

