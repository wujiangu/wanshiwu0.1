using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace cs
{
    /// <summary>
    /// UI管理器
    /// 管理GuiScene，如：GuiScene对象的生命周期，GuiScene对象之间的互斥关系，等等
    /// 每一个GuiScene都是一个逻辑界面，用来管理一组GuiObject
    /// </summary>
    public class GuiManager : Singleton<GuiManager>
    {
        public GameObject UIRoot { get { return m_assetUIRoot.gameObject; } }

        public GuiScene CreateGuiScene(string a_strName)
        {
            GuiScene guiScene = GetGuiScene(a_strName);
            if (guiScene == null)
            {
                guiScene = new GuiScene(a_strName);
                m_listGuiScene.Add(guiScene);
            }
            return guiScene;
        }

        public void DestroyGuiScene(string a_strName)
        {
            for (int i = 0; i < m_listGuiScene.Count; ++i)
            {
                if (m_listGuiScene[i].Name == a_strName)
                {
                    m_listGuiScene[i].Clear();
                    m_listGuiScene.RemoveAt(i);
                    break;
                }
            }
        }

        public GuiScene GetGuiScene(string a_strName)
        {
            for (int i = 0; i < m_listGuiScene.Count; ++i)
            {
                if (m_listGuiScene[i].Name == a_strName)
                {
                    return m_listGuiScene[i];
                }
            }
            return null;
        }

        public int GetMaxOrderInLayer(EGuiLayer a_eGuiLayer)
        {
            int nMaxOrder = 0;
            for (int i = 0; i < m_listGuiScene.Count; ++i)
            {
                int nOrder = m_listGuiScene[i].GetMaxOrderInLayer(a_eGuiLayer);
                if (nMaxOrder < nOrder)
                {
                    nMaxOrder = nOrder;
                }
            }

            return nMaxOrder;
        }

        public void ResetOrderInLayer(EGuiLayer a_eGuiLayer, int a_nStartOrder = 1)
        {
            for (int i = 0; i < m_listGuiScene.Count; ++i)
            {
                m_listGuiScene[i].ResetOrderInLayer(a_eGuiLayer, ref a_nStartOrder);
            }
        }

        protected override void _Initialize()
        {
            m_assetUIRoot = AssetManager.Get().CreateAsset("Engine/Prefab/UIRoot");
        }

        protected override void _Clear()
        {
            AssetManager.Get().DestroyAsset(m_assetUIRoot);
        }

        private List<GuiScene> m_listGuiScene = new List<GuiScene>();
        private AssetObj m_assetUIRoot = null;
    }

}
