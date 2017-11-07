using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Reflection;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace cs
{
    [InitializeOnLoad]
    public class GuiBinderEditor : AssetPostprocessor
    {
        static GuiBinderEditor()
        {
            EditorApplication.update += _Update;
        }

        [MenuItem("自定义工具/UI绑定")]
        static void CreateGuiBinder()
        {
            GameObject obj = Selection.activeObject as GameObject;
            if (obj != null)
            {
                // 获取信息源
                ComCtrlBind[] comCtrlBinds = obj.GetComponentsInChildren<ComCtrlBind>(true);
                if (comCtrlBinds == null || comCtrlBinds.Length <= 0)
                {
                    return;
                }

                string strClassName = string.Format("ComGuiBinder_{0}", obj.name);
                bool bNeedRefreshDataBase = false;
                
                // 生成绑定类
                {
                    string strTemplateFilePath = string.Format("{0}/{1}/ComGuiBinder_Template.txt", Application.dataPath, ms_strTemplatePath);
                    string strContent = File.ReadAllText(strTemplateFilePath);
                    strContent = strContent.Replace("#SCRIPTNAME#", strClassName);

                    string strMembers = string.Empty;
                    string strBindFunc = string.Empty;

                    for (int i = 0; i < comCtrlBinds.Length; ++i)
                    {
                        Component[] compoents = comCtrlBinds[i].GetComponents<Component>();
                        for (int j = 0; j < compoents.Length; ++j)
                        {
                            Type type = compoents[j].GetType();
                            if (type.IsSubclassOf(typeof(GuiControl)) || type == typeof(GuiControl))
                            {
                                string strComTypeName = compoents[j].GetType().Name;
                                string strComName = compoents[j].name;
                                string strMemberName = string.Format("{0}{1}", _GetTypeAbbreviations(strComTypeName), _MakeValidName(comCtrlBinds[i].gameObject.name));
                                strMembers += string.Format("\tpublic {0} {1};\r\n", strComTypeName, strMemberName);

                                strBindFunc += string.Format("\t\t {0} = Utility.FindCompoent<{2}>(a_objRoot, \"{1}\");\r\n",
                                    strMemberName, _GetRelativePath(obj.transform, comCtrlBinds[i].transform), strComTypeName);
                            }
                        }
                    }

                    strContent = strContent.Replace("#MEMBERS#", strMembers);
                    strContent = strContent.Replace("#BIND#", strBindFunc);

                    string strFilePath = string.Format("{0}/{1}/{2}.cs", Application.dataPath,
                        Setting.Get().GuiBinderCSPath, strClassName);

                    bool bNeedWrite = true;
                    if (File.Exists(strFilePath))
                    {
                        string oldContent = File.ReadAllText(strFilePath);
                        if (oldContent == strContent)
                        {
                            bNeedWrite = false;
                        }
                    }

                    if (bNeedWrite)
                    {
                        File.WriteAllText(strFilePath, strContent);
                        bNeedRefreshDataBase = true;
                    }
                    else
                    {
                        _BindUI();
                    }
                }

                // 生成绑定基类，主要进行下行转换
                if (bNeedRefreshDataBase)
                {
                    string strContent = File.ReadAllText(
                        string.Format("{0}/{1}/ComGuiBinderBase_Template.txt", Application.dataPath, ms_strTemplatePath)
                        );
                    string strFuncTemplate = File.ReadAllText(
                        string.Format("{0}/{1}/ComGuiBinderBaseFunc_Template.txt", Application.dataPath, ms_strTemplatePath)
                        );

                    string strFuncs = string.Empty;

                    //string[] arrPrefabGUIDs = AssetDatabase.FindAssets("t:prefab", Setting.Get().GuiBinderPrefabPaths.ToArray());
                    //if (arrPrefabGUIDs != null)
                    //{
                    //    for (int i = 0; i < arrPrefabGUIDs.Length; ++i)
                    //    {
                    //        AssetDatabase.GUIDToAssetPath()
                    //    }
                    //}

                    bool bNewClass = true;
                    string strTempFunc = string.Empty;
                    Type baseType = typeof(ComGuiBinderBase);
                    Assembly[] assemblys = AppDomain.CurrentDomain.GetAssemblies();
                    for (int i = 0; i < assemblys.Length; ++i)
                    {
                        Type[] arrTypes = assemblys[i].GetTypes();
                        if (arrTypes != null)
                        {
                            for (int j = 0; j < arrTypes.Length; ++j)
                            {
                                if (arrTypes[j].BaseType == baseType)
                                {
                                    string strTempName = arrTypes[j].Name;
                                    if (strTempName == strClassName)
                                    {
                                        bNewClass = false;
                                    }
                                    strTempFunc = strFuncTemplate.Replace("#CLASS_NAME#", strTempName);
                                    string[] strNames = strTempName.Split('_');
                                    Assert.IsTrue(strNames.Length == 2);
                                    strTempFunc = strTempFunc.Replace("#FUNC_NAME#", string.Format("ToGuiBinder_{0}", strNames[1]));
                                    strFuncs += strTempFunc;
                                }
                            }
                        }
                    }

                    if (bNewClass)
                    {
                        strTempFunc = strFuncTemplate.Replace("#CLASS_NAME#", strClassName);
                        strTempFunc = strTempFunc.Replace("#FUNC_NAME#", string.Format("ToGuiBinder_{0}", obj.name));
                        strFuncs += strTempFunc;
                    }

                    strContent = strContent.Replace("#FUNCS#", strFuncs);
                    string strFilePath = string.Format("{0}/{1}/{2}.cs", Application.dataPath,
                        Setting.Get().GuiBinderCSPath, "ComGuiBinderBaseExtend");

                    bool bNeedWrite = true;
                    if (File.Exists(strFilePath))
                    {
                        string oldContent = File.ReadAllText(strFilePath);
                        if (oldContent == strContent)
                        {
                            bNeedWrite = false;
                        }
                    }

                    if (bNeedWrite)
                    {
                        File.WriteAllText(strFilePath, strContent);
                        bNeedRefreshDataBase = true;
                    }
                }

                if (bNeedRefreshDataBase)
                {
                    AssetDatabase.Refresh();
                }
            }
        }

        static void _BindUI()
        {
            GameObject obj = Selection.activeObject as GameObject;
            if (obj != null)
            {
                ComGuiBinderBase[] oldBinders = obj.GetComponents<ComGuiBinderBase>();
                if (oldBinders != null)
                {
                    for (int i = 0; i < oldBinders.Length; ++i)
                    {
                        GameObject.DestroyImmediate(oldBinders[i], true);
                    }
                }

                string strClassName = string.Format("ComGuiBinder_{0}", obj.name);

                // 添加C#组件, 绑定对象
                Type type = null;
                Assembly[] assemblys = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblys.Length; ++i)
                {
                    Type tt = assemblys[i].GetType(strClassName);
                    if (tt != null)
                    {
                        type = tt;
                        break;
                    }
                }

                if (type == null)
                {
                    //Logger.LogError();
                    return;
                }

                IGuiBinder guiBinder = obj.AddComponent(type) as IGuiBinder;
                guiBinder.Bind(obj);
            }
        }

        static string _GetRelativePath(Transform a_tranRoot, Transform a_tranTarget)
        {
            if (a_tranRoot == a_tranTarget)
            {
                return string.Empty;
            }


            if (a_tranTarget.IsChildOf(a_tranRoot) == false)
            {
                return string.Empty;
            }

            string strPath = a_tranTarget.name;
            Transform temp = a_tranTarget;
            while (temp.parent != a_tranRoot)
            {
                strPath = strPath.Insert(0, string.Format("{0}/", a_tranTarget.parent.name));
                temp = temp.parent;
            }

            return strPath;
        }

        private class TypeNameMap
        {
            public string strFullName;
            public string strSimpleName;

            public TypeNameMap(string a_strFullName, string a_strSimpleName)
            {
                strFullName = a_strFullName;
                strSimpleName = a_strSimpleName;
            }
        }

        static TypeNameMap[] ms_arrTypeNames = {
            new TypeNameMap("GuiParticleSystem", "par"),
        };

        /// <summary>
        /// 获取类型缩写
        /// </summary>
        /// <returns></returns>
        static string _GetTypeAbbreviations(string a_strTypeFullName)
        {
            for (int i = 0; i < ms_arrTypeNames.Length; ++i)
            {
                if (ms_arrTypeNames[i].strFullName == a_strTypeFullName)
                {
                    return ms_arrTypeNames[i].strSimpleName;
                }
            }

            return a_strTypeFullName.Substring(0, 4).ToLower();
        }

        static string _MakeValidName(string a_strName)
        {
            return a_strName.Replace(" ", "");
        }

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (int i = 0; i < importedAssets.Length; i++)
            {
                if (importedAssets[i].Contains(Setting.Get().GuiBinderCSPath))
                {
                    PlayerPrefs.SetInt("ImportScripts", 1);
                    return;
                }
            }
        }

        private static void _Update()
        {
            bool importScripts = Convert.ToBoolean(PlayerPrefs.GetInt("ImportScripts", 0));
            if (importScripts && !EditorApplication.isCompiling)
            {
                _BindUI();
                importScripts = false;
                PlayerPrefs.SetInt("ImportScripts", 0);
            }
        }

        private static string ms_strTemplatePath = "/Engine/GuiBinder/Editor/Editor Default Resources";
    }
}


