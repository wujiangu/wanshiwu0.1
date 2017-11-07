using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace cs
{
    /// <summary>
    /// 图片控件
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class GuiImage : GuiControl
    {
        public override bool Initialize()
        {
            if (base.Initialize())
            {
                m_image = GetComponent<Image>();
                if (m_image == null)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public override void Clear()
        {
            base.Clear();

            m_image = null;

            if (m_asset != null)
            {
                AssetManager.Get().DestroyAsset(m_asset);
                m_asset = null;
            }
            if (m_defaultAsset != null)
            {
                AssetManager.Get().DestroyAsset(m_defaultAsset);
                m_defaultAsset = null;
            }
        }

        /// <summary>
        /// 设置缺省图片
        /// 缺省图片，最好进行预加载
        /// </summary>
        /// <param name="a_strDefaultPath"></param>
        public void SetDefaultSprite(string a_strDefaultPath)
        {
            if (m_defaultAsset != null)
            {
                if (m_defaultAsset.Desc.Path == a_strDefaultPath)
                {
                    return;
                }
                AssetManager.Get().DestroyAsset(m_defaultAsset);
                m_defaultAsset = null;
            }

            m_defaultAsset = AssetManager.Get().CreateAsset(a_strDefaultPath, typeof(Sprite), true);

            if (m_asset == null)
            {
                m_image.sprite = m_defaultAsset.Obj as Sprite;
            }
        }

        /// <summary>
        /// 设置图片，支持异步加载图片
        /// </summary>
        /// <param name="a_strPath"></param>
        /// <param name="a_bAsync"></param>
        public void SetSprite(string a_strPath, bool a_bAsync = false)
        {
            if (m_asset != null)
            {
                if (m_asset.Desc.Path == a_strPath)
                {
                    return;
                }
                AssetManager.Get().DestroyAsset(m_asset);
                m_asset = null;
            }

            AssetObj asset = AssetManager.Get().CreateAsset(a_strPath, typeof(Sprite), true, a_bAsync, _OnSpriteLoaded);
            if (asset.LoadState == EAssetLoadState.Error)
            {
                AssetManager.Get().DestroyAsset(asset);
                asset = null;
                m_image.sprite = null;
            }
            else if (asset.LoadState == EAssetLoadState.Done)
            {
                m_asset = asset;
                m_image.sprite = m_asset.Obj as Sprite;
            }
            else if (asset.LoadState == EAssetLoadState.Loading)
            {
                if (m_defaultAsset != null)
                {
                    m_image.sprite = m_defaultAsset.Obj as Sprite;
                }
            }
        }

        public void SetNativeSize()
        {
            m_image.SetNativeSize();
        }

        /// <summary>
        /// 慎用，只有在GuiImage没有提供相关接口时，使用
        /// </summary>
        public Image UGUI_Image
        {
            get { return m_image; }
        }

        private void _OnSpriteLoaded(AssetObj a_assetObj)
        {
            if (a_assetObj.LoadState == EAssetLoadState.Error)
            {
                AssetManager.Get().DestroyAsset(a_assetObj);
                m_image.sprite = null;
            }
            else
            {
                m_asset = a_assetObj;
                m_image.sprite = m_asset.Obj as Sprite;
            }
        }

        private Image m_image;
        private AssetObj m_asset;
        private AssetObj m_defaultAsset;
    }
}

