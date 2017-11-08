using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using LuaInterface;
using System.Diagnostics;
using System.Reflection;
namespace cs
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class GuiButton : GuiControl, IPointerDownHandler, IPointerUpHandler
    {
        public override bool Initialize()
        {
            if (base.Initialize())
            {
                m_button = GetComponent<Button>();
                m_image = GetComponent<Image>();
                if (m_button == null || m_image == null)
                {
                    Logger.Error("GuiButton加载失败！");
                    return false;
                }

                m_button.onClick.AddListener(_OnButtonClicked);

                return true;
            }
            return false;
        }

        public override void Clear()
        {
            base.Clear();

            RemoveAllListener();
            m_button = null;
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

        public void AddListener(UnityAction a_call)
        {
            m_button.onClick.AddListener(a_call);
        }

        public void RemoveListener(UnityAction a_call)
        {
            m_button.onClick.RemoveListener(a_call);
        }

        public void AddListener(LuaFunction a_listener)
        {
            if (m_listLuaClickCallback.Contains(a_listener) == false)
            {
                m_listLuaClickCallback.Add(a_listener);
            }
        }

        public void RemoveListener(LuaFunction a_listener)
        {
            m_listLuaClickCallback.Remove(a_listener);
            a_listener.Dispose();
            a_listener = null;
        }

        public void RemoveAllListener()
        {
            for (int i = 0; i < m_listLuaClickCallback.Count; ++i)
            {
                m_listLuaClickCallback[i].Dispose();
            }
            m_listLuaClickCallback.Clear();

            m_button.onClick.RemoveAllListeners();
            m_button.onClick.AddListener(_OnButtonClicked);
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

        public void OnPointerDown(PointerEventData eventData)
        {
            for (int i = 0; i < m_listLuaMouseDownCallback.Count; i++)
            {
                m_listLuaMouseDownCallback[i].Call();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            for (int i = 0; i < m_listLuaMouseUpCallback.Count; i++)
            {
                m_listLuaMouseUpCallback[i].Call();
            }
        }

        public void AddMouseDownListener(LuaFunction listener)
        {
            if (m_listLuaMouseDownCallback.Contains(listener) == false)
            {
                m_listLuaMouseDownCallback.Add(listener);
            }
        }

        public void RemoveMouseDownListener(LuaFunction listener)
        {
            m_listLuaMouseDownCallback.Remove(listener);
            listener.Dispose();
            listener = null;
        }

        public void AddMouseUpListener(LuaFunction listener)
        {
            if (m_listLuaMouseUpCallback.Contains(listener) == false)
            {
                m_listLuaMouseUpCallback.Add(listener);
            }
        }

        public void RemoveMouseUpListener(LuaFunction listener)
        {
            m_listLuaMouseUpCallback.Remove(listener);
            listener.Dispose();
            listener = null;
        }

        public Button UGUI_Button
        {
            get { return m_button; }
        }

        public Image UGUI_Image
        {
            get { return m_image; }
        }

        private void _OnButtonClicked()
        {
            StackFrame frame = new StackFrame(1);
            MethodBase method = frame.GetMethod();
            Logger.Log(method.Name);
            for (int i = 0; i < m_listLuaClickCallback.Count; ++i)
            {
                m_listLuaClickCallback[i].Call();
            }
        }

        private void _OnSpriteLoaded(AssetObj a_assetObj)
        {
            m_image.sprite = m_asset.Obj as Sprite;
        }

        private Button m_button;
        private Image m_image;
        private AssetObj m_asset;
        private AssetObj m_defaultAsset;
        private List<LuaFunction> m_listLuaClickCallback = new List<LuaFunction>();
        private List<LuaFunction> m_listLuaMouseDownCallback = new List<LuaFunction>();
        private List<LuaFunction> m_listLuaMouseUpCallback = new List<LuaFunction>();
    }
}

