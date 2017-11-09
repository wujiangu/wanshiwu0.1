using UnityEngine;
using UnityEditor;
using System.Collections;
using cs;

[CustomEditor(typeof(WSWSceneData))]
public class WSWSceneDataInspector : Editor
{
    [MenuItem("Assets/自定义工具/创建场景资源")]
    public static void CreateSceneData()
    {
        cs.Utility.CreateAsset<WSWSceneData>("DefaultScene");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_propLocalToWorldPos, new GUIContent("根节点位置", "场景根节点在世界坐标系的位置"));
        EditorGUILayout.PropertyField(m_propLocalScenePrefabPos, new GUIContent("场景位置", "场景预制体在场景坐标系的位置"));
        EditorGUI.EndChangeCheck();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_propScenePrefabPath, new GUIContent("场景预制体路径", "编辑时使用拷贝资源路径，复制粘贴路径"));
        if (EditorGUI.EndChangeCheck())
        {
            AssetObj asset = cs.AssetManager.Get().CreateAsset(m_propScenePrefabPath.stringValue);

        }

        serializedObject.ApplyModifiedProperties();
    }


    private void OnEnable()
    {
        m_propLocalToWorldPos = serializedObject.FindProperty("m_vec3LocalToWorldPos");
        m_propLocalScenePrefabPos = serializedObject.FindProperty("m_vec3LocalScenePrefabPos");
        m_propScenePrefabPath = serializedObject.FindProperty("m_strScenePrefabPath");
        m_propLocalMapPos = serializedObject.FindProperty("m_vec3LocalMapPos");
        m_propTextureTileRow = serializedObject.FindProperty("m_nMapTextureTileRow");
        m_propTextureTileCol = serializedObject.FindProperty("m_nMapTextureTileCol");
        m_propTextureTileCount = serializedObject.FindProperty("m_nMapTextureTileCount");
        m_propMapTexturePath = serializedObject.FindProperty("m_strMapTexturePath");
        m_propMapMaterialPath = serializedObject.FindProperty("m_strMapMaterialPath");
        m_propMapTileX = serializedObject.FindProperty("m_nMapTileX");
        m_propMapTileY = serializedObject.FindProperty("m_nMapTileY");
        m_propTileInfos = serializedObject.FindProperty("m_listTileInfo");
        m_propBuildingData = serializedObject.FindProperty("m_listBuildingData");



    }



    private SerializedProperty m_propLocalToWorldPos;
    private SerializedProperty m_propLocalScenePrefabPos;
    private SerializedProperty m_propScenePrefabPath;
    private SerializedProperty m_propLocalMapPos;
    private SerializedProperty m_propTextureTileRow;
    private SerializedProperty m_propTextureTileCol;
    private SerializedProperty m_propTextureTileCount;
    private SerializedProperty m_propMapTexturePath;
    private SerializedProperty m_propMapMaterialPath;
    private SerializedProperty m_propMapTileX;
    private SerializedProperty m_propMapTileY;
    private SerializedProperty m_propTileInfos;
    private SerializedProperty m_propBuildingData;
}
