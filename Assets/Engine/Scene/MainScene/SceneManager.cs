using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cs
{

    class SceneManager : Singleton<SceneManager>
    {
        public static string MAINSCENE = "mainScene";

        protected override void _Initialize()
        {

        }

        public void LoadScene( string strScene,string strPath)
        {
            m_assetObj = cs.AssetManager.Get().CreateAsset(strPath);
            if (strScene == m_currScene) return;
            m_currScene = strScene;
            if (strScene == SceneManager.MAINSCENE)
            {
                loadMainScene();
            }
        }

        //load main scene
        private void loadMainScene()
        {
            mapData md = m_assetObj.Obj as mapData;
            m_ms.Create(md);
        }

        public void DestroyScene(string strScene)
        {
            if (strScene == SceneManager.MAINSCENE)
            {
                m_ms.Close();

            }
            m_currScene = "";
        }

        //获得主场景的类
        public MainScene GetMainScene { get { return m_ms; } }

        /// <summary>
        /// 删除实现
        /// </summary>
        protected override void _Clear()
        {

        }

        private cs.AssetObj m_assetObj;
        private string m_currScene = "";

        /* scene  object */
        private MainScene m_ms = new MainScene();
    }
}
