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
        public static string Path()
        {
            return ms_strPath;
        }

        public static Setting Get()
        {
            if (ms_instance == null)
            {
                ms_instance = Resources.Load<Setting>(Path());
                if (ms_instance == null)
                {
                    ms_instance = CreateInstance<Setting>();
                    AssetDatabase.CreateAsset(ms_instance, Path());
                }
                Assert.IsNotNull(ms_instance);
            }
            return ms_instance;
        }

        /// <summary>
        /// 日志是否记录到文件
        /// </summary>
        public bool LogToFile { get { return m_logToFile; } }

        /// <summary>
        /// UI预制体目录列表，用于生成UI绑定器相关文件
        /// </summary>
        public string[] GuiBinderPrefabPaths { get { return m_arrGuiBinderPrefabPaths; } }

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

        [SerializeField]
        private bool m_logToFile = false;

        [SerializeField]
        private string[] m_arrGuiBinderPrefabPaths = { "Resources/Main/Prefab" };

        [SerializeField]
        private string m_strGuiBinderCSPath = "Programs/UI/GuiBinder";

        [SerializeField]
        private string[] m_arrLuaPaths = { "/Resources/Main/Script", "/Resources/Engine/Scripts", "/Resources/Preload" };

        [SerializeField]
        private int m_nMaxEventCount = 1500;

        private static Setting ms_instance = null;
        private static string ms_strPath = "Assets/Engine/Setting/Setting.asset";
    }
}

