using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace cs
{
    public class Utility
    {
        public static T FindCompoent<T>(GameObject a_obj, string a_strPath)
        {
            if (a_obj == null)
            {
                return default(T);
            }

            if (string.IsNullOrEmpty(a_strPath))
            {
                return a_obj.GetComponent<T>();
            }

            Transform transform = a_obj.transform.Find(a_strPath);
            if (transform == null)
            {
                return default(T);
            }

            return transform.GetComponent<T>();
        }

        /// <summary>
        /// 获取节点所属的GuiControl
        /// </summary>
        /// <param name="a_trans"></param>
        /// <returns></returns>
        public static GuiControl FindGuiCtrlOwner(Transform a_trans)
        {
            if (a_trans == null)
            {
                return null;
            }

            GuiControl result;
            Transform currTrans = a_trans;
            while (currTrans != null)
            {
                result = currTrans.GetComponent<GuiControl>();
                if (result != null)
                {
                    return result;
                }
                else
                {
                    currTrans = currTrans.parent;
                }
            }

            return null;
        }

        public static GuiObject FindGuiObjectOwner(Transform a_trans)
        {
            if (a_trans == null)
            {
                return null;
            }

            GuiObject result;
            Transform currTrans = a_trans;
            while (currTrans != null)
            {
                result = currTrans.GetComponent<GuiObject>();
                if (result != null)
                {
                    return result;
                }
                else
                {
                    currTrans = currTrans.parent;
                }
            }

            return null;
        }

        public static T CreateAsset<T>(string a_strFilename) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = "";

            if (Selection.assetGUIDs.Length > 0)
            {
                path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            }

            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + a_strFilename + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();

            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
            return asset;
        }

        public static GameObject[] GetRootGameObjectsInScene()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            return scene.GetRootGameObjects();
        }
    }
}

