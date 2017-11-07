using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using System.Collections;

namespace cs
{
    [CustomEditor(typeof(Setting))]
    public class SettingInspector : Editor
    {
        [MenuItem("自定义工具/引擎配置")]
        public static void OpenSettingFile()
        {
            Setting setting = Setting.Get();
            Assert.IsNotNull(setting);

            EditorGUIUtility.PingObject(setting);
            Selection.activeObject = setting;
        }

        //[MenuItem("Assets/自定义工具/创建场景资源")]
        //public static void CreateSceneAsset()
        //{
        //    Utility.CreateAsset<Setting>("DefaultSetting");
        //}

        SerializedProperty m_logToFile;

        void OnEnable()
        {
            m_logToFile = serializedObject.FindProperty("m_logToFile");
        }

        public override void OnInspectorGUI()
        {
            //Setting setting = target as Setting;
            //Assert.IsNotNull(setting);
            //setting.logToFile = EditorGUILayout.Toggle("日志是否记录进文件", setting.logToFile);

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_logToFile, new GUIContent("日志是否记录进文件"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}


