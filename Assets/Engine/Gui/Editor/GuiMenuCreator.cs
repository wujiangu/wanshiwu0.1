using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

namespace cs
{
    public class GuiMenuCreator
    {
        [MenuItem("Assets/自定义工具/预览预制体")]
        public static void QuickOpenUIPrefab()
        {
            GameObject objRoot = _CreateUIRoot();

            Object[] selection = Selection.GetFiltered(typeof(Object), UnityEditor.SelectionMode.Assets);
            if (selection != null && selection.Length > 0)
            {
                GameObject select = PrefabUtility.InstantiatePrefab(selection[0]) as GameObject;
                if (select != null)
                {
                    select.transform.SetParent(objRoot.transform, false);

                    Selection.activeObject = select;
                    EditorGUIUtility.PingObject(select);
                }
                else
                {
                    Logger.Error("请选择一个Prefab对象");
                }
            }
        }

        [MenuItem("GameObject/CS GUI/GuiObject", priority = 9)]
        public static void CreateGuiObject()
        {
            GameObject objRoot = _CreateUIRoot();

            GameObject obj = new GameObject("GuiObject");
            obj.transform.SetParent(objRoot.transform, false);
            obj.AddComponent<GuiObject>();

            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        [MenuItem("GameObject/CS GUI/GuiControl")]
        public static void CreateGuiControl()
        {
            GameObject[] arrObjSelect = Selection.GetFiltered<GameObject>(SelectionMode.ExcludePrefab);
            if (arrObjSelect != null && arrObjSelect.Length > 0)
            {
                GuiObject guiObject = Utility.FindGuiObjectOwner(arrObjSelect[0].transform);
                if (guiObject == null)
                {
                    Logger.Error("UI控件必须在GuiObject节点下创建！！请选中GuiObject节点或其子节点，再创建");
                }
                else
                {
                    GameObject objCtrl = new GameObject("GuiControl");
                    objCtrl.transform.SetParent(arrObjSelect[0].transform, false);
                    objCtrl.AddComponent<GuiControl>();

                    Selection.activeObject = objCtrl;
                    EditorGUIUtility.PingObject(objCtrl);
                }
            }
            else
            {
                Logger.Error("UI控件必须在GuiObject节点下创建！！请选中GuiObject节点或其子节点，再创建");
            }
        }

        [MenuItem("GameObject/CS GUI/GuiImage")]
        public static void CreateGuiImage()
        {
            GameObject[] arrObjSelect = Selection.GetFiltered<GameObject>(SelectionMode.ExcludePrefab);
            if (arrObjSelect != null && arrObjSelect.Length > 0)
            {
                GuiObject guiObject = Utility.FindGuiObjectOwner(arrObjSelect[0].transform);
                if (guiObject == null)
                {
                    Logger.Error("UI控件必须在GuiObject节点下创建！！请选中GuiObject节点或其子节点，再创建");
                }
                else
                {
                    GameObject objImage = new GameObject("GuiImage");
                    objImage.transform.SetParent(arrObjSelect[0].transform, false);
                    objImage.AddComponent<GuiImage>();
                    
                    Selection.activeObject = objImage;
                    EditorGUIUtility.PingObject(objImage);
                }
            }
            else
            {
                Logger.Error("UI控件必须在GuiObject节点下创建！！请选中GuiObject节点或其子节点，再创建" );
            }
        }

        [MenuItem("GameObject/CS GUI/GuiButton")]
        public static void CreateGuiButton()
        {
            GameObject[] arrObjSelect = Selection.GetFiltered<GameObject>(SelectionMode.ExcludePrefab);
            if (arrObjSelect != null && arrObjSelect.Length > 0)
            {
                GuiObject guiObject = Utility.FindGuiObjectOwner(arrObjSelect[0].transform);
                if (guiObject == null)
                {
                    Logger.Error("UI控件必须在GuiObject节点下创建！！请选中GuiObject节点或其子节点，再创建");
                }
                else
                {
                    GameObject obtButton = new GameObject("GuiButton");
                    obtButton.transform.SetParent(arrObjSelect[0].transform, false);
                    obtButton.AddComponent<GuiButton>();

                    Selection.activeObject = obtButton;
                    EditorGUIUtility.PingObject(obtButton);
                }
            }
            else
            {
                Logger.Error("UI控件必须在GuiObject节点下创建！！请选中GuiObject节点或其子节点，再创建");
            }
        }

        [MenuItem("GameObject/CS GUI/GuiLabel")]
        public static void CreateGuiLabel()
        {
            GameObject[] arrObjSelect = Selection.GetFiltered<GameObject>(SelectionMode.ExcludePrefab);
            if (arrObjSelect != null && arrObjSelect.Length > 0)
            {
                GuiObject guiObject = Utility.FindGuiObjectOwner(arrObjSelect[0].transform);
                if (guiObject == null)
                {
                    Logger.Error("UI控件必须在GuiObject节点下创建！！请选中GuiObject节点或其子节点，再创建");
                }
                else
                {
                    GameObject objLabel = new GameObject("GuiLabel");
                    objLabel.transform.SetParent(arrObjSelect[0].transform, false);
                    objLabel.AddComponent<GuiLabel>();

                    Selection.activeObject = objLabel;
                    EditorGUIUtility.PingObject(objLabel);
                }
            }
            else
            {
                Logger.Error("UI控件必须在GuiObject节点下创建！！请选中GuiObject节点或其子节点，再创建");
            }
        }

        static void setObjAnchor(GameObject obj)
        {
            obj.transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
            obj.transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        }

        [MenuItem("GameObject/CS GUI/GuiScrollView")]
        public static void CreateGuiScrollView()
        {
            GameObject[] arrObjSelect = Selection.GetFiltered<GameObject>(SelectionMode.ExcludePrefab);
            if (arrObjSelect != null && arrObjSelect.Length > 0)
            {
                GuiObject guiObject = Utility.FindGuiObjectOwner(arrObjSelect[0].transform);
                if (guiObject == null)
                {
                    Logger.Error("UI控件必须在GuiObject节点下创建！！请选中GuiObject节点或其子节点，再创建");
                }
                else
                {
                    GameObject obj = Resources.Load("Main/Prefab/GuiScrollView") as GameObject;
                    if (obj)
                    {
                        GameObject guiScrollView = GameObject.Instantiate(obj);
                        guiScrollView.transform.SetParent(arrObjSelect[0].transform, false);
                    }
                }
            }
            else
            {
                Logger.Error("UI控件必须在GuiObject节点下创建！！请选中GuiObject节点或其子节点，再创建");
            }
        }

        private static GameObject _CreateUIRoot()
        {
            bool bNeedLoad = true;
            UnityEngine.SceneManagement.Scene currScene = EditorSceneManager.GetActiveScene();
            if (currScene != null && currScene.path == ms_strUIEditorScenePath)
            {
                bNeedLoad = false;
            }
            if (bNeedLoad)
            {
                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(ms_strUIEditorScenePath);

                GameObject[] arrObjs = scene.GetRootGameObjects();

                for (int i = 0; i < arrObjs.Length; ++i)
                {
                    GameObject.DestroyImmediate(arrObjs[i], true);
                    arrObjs[i] = null;
                }
                arrObjs = null;
            }

            GameObject objRoot = GameObject.Find("UIRoot(Clone)");
            if (objRoot == null)
            {
                Object objRootTemplate = Resources.Load("Engine/Prefab/UIRoot");
                objRoot = Object.Instantiate(objRootTemplate) as GameObject;
            }

            return objRoot;
        }

        private static string ms_strUIEditorScenePath = "Assets/Engine/Gui/Editor/UIEditor.unity";
    }
}


