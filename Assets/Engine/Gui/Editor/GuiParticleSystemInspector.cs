using UnityEngine;
using UnityEditor;

namespace cs
{
    [CustomEditor(typeof(GuiParticleSystem))]
    public class GuiParticleSystemInspector : Editor
    {
        SerializedProperty m_propSortingOrderOffset;
        
        private void OnEnable()
        {
            m_propSortingOrderOffset = serializedObject.FindProperty("m_nSortingOrderOffset");
            _RefreshSortingOrder();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.IntSlider(m_propSortingOrderOffset, 1, 1000, new GUIContent("渲染排序偏移量"));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                _RefreshSortingOrder();
            }
        }

        void _RefreshSortingOrder()
        {
            GuiParticleSystem guiPar = serializedObject.targetObject as GuiParticleSystem;
            if (guiPar != null)
            {
                guiPar.InitRenderers();
                guiPar.SetSortingOrder(0);
            }
        }
    }
}

