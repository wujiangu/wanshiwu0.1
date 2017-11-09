using UnityEngine;
using UnityEditor;

public class UtilityTool
{
    [MenuItem("Assets/自定义工具/拷贝资源路径")]
    static void CopyAssetPath()
    {
        UnityEngine.Object[] selection = Selection.GetFiltered(typeof(UnityEngine.Object), UnityEditor.SelectionMode.Assets);
        if (selection.Length > 0)
        {
            string strPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            strPath = strPath.Replace(string.Format("Assets/{0}/", cs.Setting.Get().ResourcesPath), "");
            strPath = strPath.Replace(".preafab", "");
            GUIUtility.systemCopyBuffer = strPath;
        }
    }
}