using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

namespace cs
{
    public class Setting : ScriptableObject
    {
        /// <summary>
        /// Resources文件夹路径
        /// </summary>
        public string ResourcesPath { get { return m_strResourcesPath; } }

        /// <summary>
        /// 日志是否记录到文件
        /// </summary>
        public bool LogToFile { get { return m_logToFile; } }

        /// <summary>
        /// UI绑定器cs文件生成目录
        /// </summary>
        public string GuiBinderCSPath { get { return m_strGuiBinderCSPath; } }

        /// <summary>
        /// lua代码路径
        /// </summary>
        public string[] LuaPaths { get { return m_arrLuaPaths; } }

        /// <summary>
        /// 最大事件数量
        /// </summary>
        public int MaxEventCount { get { return m_nMaxEventCount; } }


        /// <summary>
        /// 获取配置
        /// 注意：初次创建配置文件时，请确保ResourcesPath填写正确
        /// </summary>
        /// <returns></returns>
        public static Setting Get()
        {
            if (ms_instance == null)
            {
                ms_assetObj = AssetManager.Get().CreateAsset(_GetLoadPath(), typeof(Setting));
                if (ms_assetObj.LoadState != EAssetLoadState.Done)
                {
                    AssetManager.Get().DestroyAsset(ms_assetObj);
                    ms_assetObj = null;

                    ms_instance = CreateInstance<Setting>();
                    AssetDatabase.CreateAsset(ms_instance, _GetCreatePath());
                }
                else
                {
                    ms_instance = ms_assetObj.Obj as Setting;
                }
                Assert.IsNotNull(ms_instance);
            }
            return ms_instance;
        }

        public static void Destroy()
        {
            ms_instance = null;
            AssetManager.Get().DestroyAsset(ms_assetObj);
            ms_assetObj = null;
        }

        private static string _GetLoadPath()
        {
            return ms_strPath;
        }

        private static string _GetCreatePath()
        {
            return string.Format("Assets/{0}/{1}", Setting.Get().ResourcesPath, ms_strPath);
        }
        
        [SerializeField]
        private string m_strResourcesPath = "Resources";

        [SerializeField]
        private bool m_logToFile = false;

        [SerializeField]
        private string m_strGuiBinderCSPath = "Programs/UI/GuiBinder";

        [SerializeField]
        private string[] m_arrLuaPaths = { "Main/Script", "Engine/Scripts", "Preload" };

        [SerializeField]
        private int m_nMaxEventCount = 1500;

        private static AssetObj ms_assetObj = null;
        private static Setting ms_instance = null;
        private static string ms_strPath = "Engine/Setting/Setting";
    }
}

