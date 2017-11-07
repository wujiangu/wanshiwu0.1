using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using LuaInterface;


namespace cs
{
    public class GuiScene
    {
        public GuiScene(string a_strName)
        {
            m_strName = a_strName;
        }

        public string Name
        {
            get { return m_strName; }
        }

        public void Init()
        {
            if (m_initCallback != null)
            {
                m_initCallback.Call();
            }
        }

        public void Reset()
        {
            
        }

        public void Hide()
        {

        }

        public void Clear()
        {
            for (int i = 0; i < m_listGuiInfos.Count; ++i)
            {
                m_listGuiInfos[i].guiObject.Clear();
                AssetManager.Get().DestroyAsset(m_listGuiInfos[i].assetObj);
            }
            m_listGuiInfos.Clear();
        }

        public GuiObject LoadGuiObject(string a_strPath)
        {
            return LoadGuiObject(a_strPath, a_strPath);
        }

        public GuiObject LoadGuiObject(string a_strName, string a_strPath)
        {
            GuiObject guiObject = FindGuiObject(a_strName);
            if (guiObject == null)
            {
                AssetObj assetObj = AssetManager.Get().CreateAsset(a_strPath);
                Assert.IsTrue(assetObj.LoadState != EAssetLoadState.Loading);

                GameObject objRoot = assetObj.gameObject;
                Assert.IsTrue(objRoot != null);

                guiObject = objRoot.GetComponent<GuiObject>();
                Assert.IsNotNull(guiObject);
                objRoot.transform.SetParent(GuiManager.Get().UIRoot.transform, false);
                guiObject.Initialize(a_strName);

                GuiInfo guiInfo = new GuiInfo();
                guiInfo.assetObj = assetObj;
                guiInfo.guiObject = guiObject;
                m_listGuiInfos.Add(guiInfo);
            }
            return guiObject;
        }

        public void UnloadGuiObject(string a_strName)
        {
            for (int i = 0; i < m_listGuiInfos.Count; ++i)
            {
                if (m_listGuiInfos[i].guiObject.Name == a_strName)
                {
                    AssetManager.Get().DestroyAsset(m_listGuiInfos[i].assetObj);
                    m_listGuiInfos.RemoveAt(i);
                    break;
                }
            }
        }

        public GuiObject FindGuiObject(string a_strName)
        {
            for (int i = 0; i < m_listGuiInfos.Count; ++i)
            {
                if (m_listGuiInfos[i].guiObject.Name == a_strName)
                {
                    return m_listGuiInfos[i].guiObject;
                }
            }

            return null;
        }

        public int GetMaxOrderInLayer(EGuiLayer a_eGuiLayer)
        {
            int nMaxOrder = 0;
            for (int i = 0; i < m_listGuiInfos.Count; ++i)
            {
                GuiObject guiObject = m_listGuiInfos[i].guiObject;
                if (
                    guiObject.GuiLayer == a_eGuiLayer &&
                    (guiObject.State == EGuiState.FadeIn || 
                    guiObject.State == EGuiState.Opened || 
                    guiObject.State == EGuiState.FadeOut) &&
                    nMaxOrder < guiObject.OpenOrder
                    )
                {
                    nMaxOrder = guiObject.OpenOrder;
                }
            }
            return nMaxOrder;
        }

        public void ResetOrderInLayer(EGuiLayer a_eGuiLayer, ref int a_nStartOrder)
        {
            for (int i = 0; i < m_listGuiInfos.Count; ++i)
            {
                GuiObject guiObject = m_listGuiInfos[i].guiObject;
                if (
                    guiObject.GuiLayer == a_eGuiLayer &&
                    (guiObject.State == EGuiState.FadeIn ||
                    guiObject.State == EGuiState.Opened ||
                    guiObject.State == EGuiState.FadeOut)
                    )
                {
                    guiObject.SetOpenOrder(a_nStartOrder);
                    a_nStartOrder++;
                }
            }
        }

        public void SetInitCallback(LuaFunction a_luaFunction)
        {
            if (m_initCallback != null)
            {
                m_initCallback.Dispose();
                m_initCallback = null;
            }
            m_initCallback = a_luaFunction;
        }

        private class GuiInfo
        {
            public AssetObj assetObj;
            public GuiObject guiObject;
        }

        private string m_strName;
        private List<GuiInfo> m_listGuiInfos = new List<GuiInfo>();
        private LuaFunction m_initCallback = null;
    }
}


